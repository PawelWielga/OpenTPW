using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The all-staff and all-items screens rewrite their rows in place every two seconds of real time, as the original's
/// timer <c>0x80083</c> does (<c>docs/exe/hud.md</c>, "How allstaff and allitems keep their rows current"). Reads the
/// shipped park; skipped without the game (<see cref="GameData"/>).
/// </summary>
[TestClass]
public class ParkListScreenRewriteTests
{
	private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

	private Level? before;
	private Point2 size;
	private ParkWorld world = null!;
	private ParkState state = null!;
	private ParkPeople people = null!;
	private Level level = null!;
	private WindowStack stack = null!;
	private readonly List<string> lines = [];

	[TestInitialize]
	public void OpenThePark()
	{
		before = Level.Current;
		size = Screen.Size;
		Screen.Size = new( 2048, 1536 );
		Log ??= new();
		FileSystem = GameData.Required();

		using var stream = new MemoryStream( FileSystem.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		world = new ParkWorld( new SaveReader( stream ).ReadFile() );
		state = new( world );

		var balance = new ParkBalance( "jungle", easyMode: true );
		var catalogue = new ParkItemCatalogue( "jungle", FileSystem );

		level = (Level)RuntimeHelpers.GetUninitializedObject( typeof( Level ) );
		typeof( Level ).GetProperty( nameof( Level.Park ) )!.SetValue( level, world );
		typeof( Level ).GetProperty( nameof( Level.ParkState ) )!.SetValue( level, state );
		typeof( Level ).GetProperty( nameof( Level.Catalogue ) )!.SetValue( level, catalogue );
		typeof( Level ).GetProperty( nameof( Level.StaffPool ) )!.SetValue( level, Level.StaffPoolFor( world, balance, world.GameTick ) );
		Level.Current = level;

		people = new( world, balance, null, state, catalogue );

		stack = (WindowStack)RuntimeHelpers.GetUninitializedObject( typeof( WindowStack ) );
		typeof( WindowStack ).GetField( "_windows", Private )!.SetValue( stack, new List<UiWindow>() );

		Logger.OnLog += Heard;
	}

	[TestCleanup]
	public void PutBack()
	{
		Logger.OnLog -= Heard;
		Time.PinWall( 0 );
		people?.Delete();
		Entity.ApplyDeletions();
		Level.Current = before!;
		Screen.Size = size;
	}

	private void Heard( Logger.Level severity, string line ) => lines.Add( line );

	private int Rewrites( string screen ) => lines.Count( line => line.StartsWith( screen ) && line.Contains( "rows rewritten" ) );

	private static T Field<T>( object screen, string name ) => (T)screen.GetType().GetField( name, Private )!.GetValue( screen )!;

	/// <summary>
	/// <b>The all-staff list rewrites each row's status, skill and happiness every two seconds from its opening, and
	/// leaves the name, the wage and both meters as they were</b> (<c>FUN_00495da0</c>, mask <c>0x1a</c>); a tab does not
	/// restart the timer, armed once by the opener.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> no rewrite; the rewrite a frame early; the wage rewritten too; the status, the skill or the
	/// happiness kept; the meters moved every frame, by the rewrite or every member's by a tab; a tab restarting the timer.
	/// </remarks>
	[TestMethod]
	public void TheStaffListRewritesStatusSkillAndHappinessEveryTwoSecondsAndNotTheWageOrTheMeters()
	{
		var member = people.Staff[0];
		var kind = ParkStaffPool.KindFor( member.Model );
		var pool = level.StaffPool!;
		var grade = member.PayGrade == 0 ? 1 : 0;

		Assert.AreNotEqual( pool.WageFor( kind, member.PayGrade ), pool.WageFor( kind, grade ), "two grades, two wages" );

		typeof( ParkStaffScreen ).GetField( "_lastKind", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, kind );

		// Opened five seconds into real time: the rewrite is two seconds from there, not from nought.
		Time.PinWall( 5000 );

		var screen = new ParkStaffScreen( stack );
		var list = Field<UiList>( screen, "_list" );
		var kindMeter = Field<UiMeter>( screen, "_kindHappiness" );
		var allMeter = Field<UiMeter>( screen, "_allHappiness" );
		var row = list.Rows.ToList().FindIndex( each => each.Id == member.ThingId );
		var opened = list.Rows[row].Values!.ToArray();
		var (kindAtOpening, allAtOpening) = (kindMeter.Value, allMeter.Value);

		var activity = System.Enum.GetValues<StaffActivity>().First( each => each != member.Activity );

		member.PayGrade = grade;
		member.Happiness = member.Happiness < 50 ? 90 : 10;
		typeof( Staff ).GetProperty( nameof( Staff.Activity ) )!.SetValue( member, activity );

		Time.PinWall( 6999 );
		screen.Update();
		CollectionAssert.AreEqual( opened, list.Rows[row].Values!.ToArray(), "1999 ms: not rewritten" );
		Assert.AreEqual( kindAtOpening, kindMeter.Value, "the kind's meter stands between rewrites" );
		Assert.AreEqual( allAtOpening, allMeter.Value, "and every member's" );

		Time.PinWall( 7000 );
		screen.Update();
		var now = list.Rows[row].Values!;
		Assert.AreNotEqual( $"{activity}", opened[0] );
		Assert.AreEqual( $"{activity}", now[0], "the status rewritten" );
		Assert.AreEqual( opened[1], now[1], "the wage as the fill wrote it" );
		Assert.AreEqual( $"{grade}", now[2], "the skill rewritten" );
		Assert.AreEqual( $"{(int)member.Happiness}", now[3], "the happiness rewritten" );
		Assert.AreEqual( member.ThingId, list.Rows[row].Id );
		Assert.AreEqual( kindAtOpening, kindMeter.Value, "the rewrite leaves the kind's meter" );
		Assert.AreEqual( allAtOpening, allMeter.Value, "and every member's" );
		Assert.AreEqual( 1, Rewrites( "Staff list" ) );

		// A tab at 7500: the timer keeps the opening's phase, so the next rewrite is at 9000.
		Time.PinWall( 7500 );
		var tabs = Field<UiRadioGroup>( screen, "_tabs" );
		tabs.Select( tabs.Children.OfType<UiButton>().First( option => option.Id != tabs.Selected ).Id );
		screen.Update();
		Assert.AreEqual( allAtOpening, allMeter.Value, "a tab sets the kind's meter alone" );

		Time.PinWall( 8999 );
		screen.Update();
		Assert.AreEqual( 1, Rewrites( "Staff list" ), "8999 ms: not yet" );

		Time.PinWall( 9000 );
		screen.Update();
		Assert.AreEqual( 2, Rewrites( "Staff list" ), "9000 ms: two seconds from the last, not from the tab" );
	}

	/// <summary>
	/// <b>The all-items list rewrites each row's columns after its name every two seconds on the rides', shops' and
	/// sideshows' tabs, a tab starting the timer again, and never on the miscellaneous tab</b> (<c>FUN_004955e0</c>,
	/// armed by each tab's own list tree at message <c>0x15</c>).
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> no rewrite; the rewrite a frame early; the values kept; the name rewritten; a tab keeping the
	/// old phase; the miscellaneous tab rewritten.
	/// </remarks>
	[TestMethod]
	public void TheItemsListRewritesItsRowsEveryTwoSecondsFromItsTabAndNotTheMiscellaneousOne()
	{
		typeof( ParkItemsScreen ).GetField( "_lastTab", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, 0 );

		Time.PinWall( 5000 );

		var screen = new ParkItemsScreen( stack );
		var list = Field<Dictionary<int, UiList>>( screen, "_lists" )[5];
		var ride = state.Objects.First( placed => list.Rows.Any( row => row.Id == placed.ThingId ) );
		var at = list.Rows.ToList().FindIndex( row => row.Id == ride.ThingId );
		var name = list.Rows[at].Name;
		var opened = list.Rows[at].Numbers!.ToArray();
		var repair = (int)ride.StateOfRepair == 40 ? 60f : 40f;

		state.ReplaceObject( ride with { StateOfRepair = repair } );

		Time.PinWall( 6999 );
		screen.Update();
		CollectionAssert.AreEqual( opened, list.Rows[at].Numbers!.ToArray(), "1999 ms: not rewritten" );

		Time.PinWall( 7000 );
		screen.Update();
		Assert.AreEqual( ((int)repair << 10) / 100, list.Rows[at].Numbers![3], "the state of repair's bar rewritten" );
		Assert.AreEqual( name, list.Rows[at].Name, "the name as the fill wrote it" );
		Assert.AreEqual( 1, Rewrites( "Items list" ) );

		// The shops' tab at 7500 starts the timer again: nothing at 9000, a rewrite at 9500.
		Time.PinWall( 7500 );
		var show = typeof( ParkItemsScreen ).GetMethod( "Show", Private )!;
		show.Invoke( screen, [1] );

		Time.PinWall( 9000 );
		screen.Update();
		Assert.AreEqual( 1, Rewrites( "Items list" ), "9000 ms: 1500 ms from the tab" );

		Time.PinWall( 9500 );
		screen.Update();
		Assert.AreEqual( 2, Rewrites( "Items list" ), "9500 ms: two seconds from the tab" );

		// The miscellaneous tab: no rewrite, however long.
		show.Invoke( screen, [3] );
		Time.PinWall( 20000 );
		screen.Update();
		Assert.AreEqual( 2, Rewrites( "Items list" ), "the miscellaneous tab is never rewritten" );
	}

	/// <summary>
	/// <b>Each tab's second column is the customers of the last thirty finished days, a number, and the rides'
	/// excitement, state of repair and remaining life and the sideshows' excitement are bars</b>, each
	/// <c>((value &amp; 0xff) &lt;&lt; 10) / 100</c> of 1024, as the fill <c>FUN_00493cd0</c> and the rewrite
	/// <c>FUN_004955e0</c> hand them; the shops' and sideshows' satisfaction bars are left unfilled and counted.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the customers not written, or today's taken in, or more than thirty days; a bar's low byte not
	/// taken; the repair and the life exchanged; the excitement left out; the rides' bars in the shops' columns; the
	/// sideshows' excitement not a bar; the count left on the rides' tab.
	/// </remarks>
	[TestMethod]
	public void TheItemsRowsAreTheCustomersAsANumberAndTheBarsAsTheOriginalHandsThem()
	{
		Assert.AreEqual( 1024, ParkItemsScreen.Bar( 100 ) );
		Assert.AreEqual( 890, ParkItemsScreen.Bar( 87 ) );
		Assert.AreEqual( 1024, ParkItemsScreen.Bar( 356 ), "the low byte alone" );
		Assert.AreEqual( 2611, ParkItemsScreen.Bar( -1 ), "255 of it, held by the bar" );

		typeof( ParkItemsScreen ).GetField( "_lastTab", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, 0 );

		// The Belly Bounce given thirty-two finished days of 1 to 32 customers and 1000 today, a repair of 87.9 and a
		// life of 300: the column is days 3 to 32, 525; the repair's bar 87's, the life's its low byte 44's.
		var bounce = state.Objects.Single( placed => placed.ThingId == 13 );
		var rings = state.RingsFor( 13 );

		for ( var day = 1; day <= 32; ++day )
		{
			rings.Customers.Today = day;
			rings.Roll();
		}

		rings.Customers.Today = 1000;
		state.ReplaceObject( bounce with { StateOfRepair = 87.9f, RemainingLife = 300f } );
		bounce = state.Objects.Single( placed => placed.ThingId == 13 );

		var screen = new ParkItemsScreen( stack );
		var lists = Field<Dictionary<int, UiList>>( screen, "_lists" );
		var rides = lists[5];
		var at = rides.Rows.ToList().FindIndex( row => row.Id == 13 );
		Assert.IsTrue( at >= 0, "the Belly Bounce is listed" );

		var catalogue = level.Catalogue!;
		catalogue.TryGet( bounce.CatalogueId, out var item );

		// Its excitement is 40 (the `spend` census), 409 of 1024.
		Assert.AreEqual( 40, ParkRideScore.ExcitementOf( bounce, item, state.TrackRides ) );
		CollectionAssert.AreEqual( new[] { 0, 525, 409, 890, 450 }, rides.Rows[at].Numbers!.ToArray() );
		CollectionAssert.AreEqual( new[] { "525", "", "", "" }, rides.Rows[at].Values!.ToArray() );
		CollectionAssert.AreEqual( new[] { false, false, true, true, true }, rides.BarColumns );
		CollectionAssert.AreEqual( new[] { 868, 1167, 1466 },
			rides.Children.OfType<ParkObjectWindow.UiStatBar>().Select( bar => bar.Rect.Left ).Distinct().ToArray() );

		var counted = Unimplemented.Summary.FirstOrDefault( entry => entry.What == "ALL_ITEMS_MONTHLY_HISTORY" ).Times;
		var show = typeof( ParkItemsScreen ).GetMethod( "Show", Private )!;

		show.Invoke( screen, [1] );
		var shops = lists[5];
		CollectionAssert.AreEqual( new[] { false, false, false, false, true }, shops.BarColumns, "the shops' one bar" );
		CollectionAssert.AreEqual( new[] { 1466 },
			shops.Children.OfType<ParkObjectWindow.UiStatBar>().Select( bar => bar.Rect.Left ).Distinct().ToArray(),
			"its cells made again: the rides' three bars gone" );
		Assert.IsTrue( shops.Rows.Count > 0, "the stock park has shops" );
		Assert.IsTrue( shops.Rows.All( row => row.Numbers![2] == UiList.NoBar && row.Numbers[4] == UiList.NoBar ) );
		Assert.AreEqual( counted + 1, Unimplemented.Summary.First( entry => entry.What == "ALL_ITEMS_MONTHLY_HISTORY" ).Times );

		show.Invoke( screen, [2] );
		var sideshows = lists[6];
		CollectionAssert.AreEqual( new[] { false, false, true, false, false, true }, sideshows.BarColumns );
		Assert.IsTrue( sideshows.Rows.Count > 0, "the stock park has sideshows" );

		foreach ( var row in sideshows.Rows )
		{
			var placed = state.Objects.Single( each => each.ThingId == row.Id );
			catalogue.TryGet( placed.CatalogueId, out var game );
			Assert.AreEqual( ((ParkRideScore.ExcitementOf( placed, game, state.TrackRides ) & 0xff) << 10) / 100, row.Numbers![2] );
			Assert.AreEqual( UiList.NoBar, row.Numbers[5] );
		}

		show.Invoke( screen, [0] );
		Assert.AreEqual( counted + 2, Unimplemented.Summary.First( entry => entry.What == "ALL_ITEMS_MONTHLY_HISTORY" ).Times,
			"the rides' tab works out every column" );
	}
}
