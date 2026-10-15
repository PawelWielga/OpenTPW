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
/// timer <c>0x80083</c> does (<c>docs/exe/hud.md</c>, "How allstaff and allitems keep their rows current"), and frame
/// their headings as its openers do. Reads the shipped park; skipped without the game (<see cref="GameData"/>).
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
		var kindMeter = Field<ParkObjectWindow.UiStatBar>( screen, "_kindHappiness" );
		var allMeter = Field<ParkObjectWindow.UiStatBar>( screen, "_allHappiness" );
		var row = list.Rows.ToList().FindIndex( each => each.Id == member.ThingId );
		var opened = list.Rows[row].Values!.ToArray();
		var openedNumbers = list.Rows[row].Numbers!.ToArray();
		var (kindAtOpening, allAtOpening) = (kindMeter.Units, allMeter.Units);

		var activity = System.Enum.GetValues<StaffActivity>().First( each => each != member.Activity );

		member.PayGrade = grade;
		member.Happiness = member.Happiness < 50 ? 90 : 10;
		typeof( Staff ).GetProperty( nameof( Staff.Activity ) )!.SetValue( member, activity );

		Time.PinWall( 6999 );
		screen.Update();
		CollectionAssert.AreEqual( opened, list.Rows[row].Values!.ToArray(), "1999 ms: not rewritten" );
		CollectionAssert.AreEqual( openedNumbers, list.Rows[row].Numbers!.ToArray(), "nor its bars" );
		Assert.AreEqual( kindAtOpening, kindMeter.Units, "the kind's meter stands between rewrites" );
		Assert.AreEqual( allAtOpening, allMeter.Units, "and every member's" );

		Time.PinWall( 7000 );
		screen.Update();
		var now = list.Rows[row].Values!;
		var numbers = list.Rows[row].Numbers!;
		Assert.AreNotEqual( $"{activity}", opened[0] );
		Assert.AreEqual( $"{activity}", now[0], "the status rewritten" );
		Assert.AreEqual( opened[1], now[1], "the wage as the fill wrote it" );
		Assert.AreEqual( openedNumbers[2], numbers[2], "and its number" );
		Assert.AreEqual( (ParkStaffScreen.SkillOf( member ) << 10) / 100, numbers[3], "the skill's bar rewritten" );
		Assert.AreNotEqual( openedNumbers[3], numbers[3] );
		Assert.AreEqual( (((int)member.Happiness & 0xff) << 10) / 100, numbers[4], "the happiness's bar rewritten" );
		Assert.AreNotEqual( openedNumbers[4], numbers[4] );
		Assert.AreEqual( member.ThingId, list.Rows[row].Id );
		Assert.AreEqual( kindAtOpening, kindMeter.Units, "the rewrite leaves the kind's meter" );
		Assert.AreEqual( allAtOpening, allMeter.Units, "and every member's" );
		Assert.AreEqual( 1, Rewrites( "Staff list" ) );

		// A tab at 7500: the timer keeps the opening's phase, so the next rewrite is at 9000.
		Time.PinWall( 7500 );
		var tabs = Field<UiRadioGroup>( screen, "_tabs" );
		tabs.Select( tabs.Children.OfType<UiButton>().First( option => option.Id != tabs.Selected ).Id );
		screen.Update();
		Assert.AreEqual( allAtOpening, allMeter.Units, "a tab sets the kind's meter alone" );

		Time.PinWall( 8999 );
		screen.Update();
		Assert.AreEqual( 1, Rewrites( "Staff list" ), "8999 ms: not yet" );

		Time.PinWall( 9000 );
		screen.Update();
		Assert.AreEqual( 2, Rewrites( "Staff list" ), "9000 ms: two seconds from the last, not from the tab" );
	}

	/// <summary>
	/// <b>The all-staff row's cells are the styler <c>FUN_00496d20</c>'s</b>: the wage lettered as a money cell, the skill a
	/// bar in <c>ridestatbar.wct</c> and the happiness a bar in <c>happygrad.wct</c>, each inset 8, filled from the row
	/// adder's <c>(skill &lt;&lt; 10) / 100</c> and <c>((happiness &amp; 0xff) &lt;&lt; 10) / 100</c>; and the two meters
	/// are the same skin in <c>happygrad.wct</c>, inset 8 (<c>docs/exe/hud.md</c>, "The all-staff rows: a money cell, then
	/// bars").
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the skill as the grade's fifths alone; the percentage left out or not a byte; the happiness not
	/// a byte; the wage as a bare number; the happiness in the ride bars' skin; an inset of 10; the meters not inset or
	/// in the ride bars' skin.
	/// </remarks>
	[TestMethod]
	public void TheStaffRowsLetterTheWageAsMoneyAndDrawSkillAndHappinessAsTheOriginalsBars()
	{
		var member = people.Staff[0];
		var kind = ParkStaffPool.KindFor( member.Model );

		member.PayGrade = 3;
		member.PercentageThroughGrade = 0x10f;
		member.Happiness = 300.75f;

		typeof( ParkStaffScreen ).GetField( "_lastKind", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, kind );

		var screen = new ParkStaffScreen( stack );
		var list = Field<UiList>( screen, "_list" );
		var row = list.Rows.First( each => each.Id == member.ThingId );
		var wage = level.StaffPool!.WageFor( kind, 3 );

		Assert.AreEqual( 63, ParkStaffScreen.SkillOf( member ), "(3 x 100 + 15) / 5: the percentage's low byte" );
		CollectionAssert.AreEqual( new[] { 0, 0, wage, 645, 450 }, row.Numbers!.ToArray(),
			"the skill's (63 << 10) / 100 and the happiness's ((300 & 0xff) << 10) / 100" );
		Assert.AreEqual( $"$ {wage}", row.Values![1], "the wage in a money cell" );

		member.PayGrade = 4;
		member.PercentageThroughGrade = 99;
		Assert.AreEqual( 99, ParkStaffScreen.SkillOf( member ), "(400 + 99) / 5 truncated" );

		var bars = list.Children.OfType<ParkObjectWindow.UiStatBar>().ToArray();
		var top = bars.Min( bar => bar.Rect.Top );
		var first = bars.Where( bar => bar.Rect.Top == top ).OrderBy( bar => bar.Rect.Left ).ToArray();

		Assert.AreEqual( 2, first.Length, "two bar cells a row" );
		Assert.AreEqual( (1231, ParkObjectWindow.UiStatBar.RideStatBar), (first[0].Rect.Left, first[0].Skin), "the skill" );
		Assert.AreEqual( (1488, ParkObjectWindow.UiStatBar.HappyGrad), (first[1].Rect.Left, first[1].Skin), "the happiness" );
		Assert.AreEqual( 418 + 8, top, "inset 8 from the row's top" );
		Assert.AreEqual( member.ThingId, list.Rows[0].Id, "the member stands first" );
		Assert.AreEqual( 645, first[0].Units, "the first row's skill bar filled" );
		Assert.AreEqual( 450, first[1].Units, "and its happiness bar" );

		foreach ( var (name, rect) in new[] { ("_kindHappiness", new UiRect( 1191, 840, 1567, 869 )), ("_allHappiness", new UiRect( 1191, 891, 1567, 920 )) } )
		{
			var meter = Field<ParkObjectWindow.UiStatBar>( screen, name );
			Assert.AreEqual( rect, meter.Rect, $"{name} inset 8" );
			Assert.AreEqual( ParkObjectWindow.UiStatBar.HappyGrad, meter.Skin, name );
			Assert.IsTrue( meter.Known, $"{name} drawn at any value" );
		}
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

		var show = typeof( ParkItemsScreen ).GetMethod( "Show", Private )!;

		// The first shop and the first sideshow: twenty-eight days to push the file's out of the thirty, the first of them
		// 7 taken at a satisfaction of -30, then two finished days of 40 taken against 10 and 25 spent and of satisfaction
		// -20 and -25, and today 70 against 100.
		show.Invoke( screen, [1] );
		var shop = lists[5].Rows[0].Id;
		show.Invoke( screen, [2] );
		var sideshow = lists[6].Rows[0].Id;

		foreach ( var id in new[] { shop, sideshow } )
		{
			var each = state.RingsFor( id );
			var totalBefore = state.TakingsFor( id ) - state.CostsFor( id );

			for ( var day = 0; day < 28; ++day )
			{
				if ( day == 0 )
				{
					state.TakeAt( id, 7 );
					each.Satisfaction.Today = -30;
				}

				each.Roll();
			}

			for ( var day = 0; day < 2; ++day )
			{
				state.TakeAt( id, 40 );
				state.BookCostOfGoods( id, day == 0 ? 10 : 25 );
				each.Satisfaction.Today = day == 0 ? -20 : -25;
				each.Roll();
			}

			state.TakeAt( id, 70 );
			state.BookCostOfGoods( id, 100 );
			Assert.AreEqual( totalBefore + 157 - 135, state.TakingsFor( id ) - state.CostsFor( id ) );
		}

		show.Invoke( screen, [1] );
		var shops = lists[5];
		CollectionAssert.AreEqual( new[] { false, false, false, false, true }, shops.BarColumns, "the shops' one bar" );
		CollectionAssert.AreEqual( new[] { 1466 },
			shops.Children.OfType<ParkObjectWindow.UiStatBar>().Select( bar => bar.Rect.Left ).Distinct().ToArray(),
			"its cells made again: the rides' three bars gone" );

		// Last month 87 - 35 = 52 (today's not among it; 45 over twenty-nine days); the total the file's and today's; the
		// satisfaction 50 plus -75 / 30 truncated toward nought, 48, a bar of 491 (floored 481; over twenty-nine days 501).
		var shopRow = shops.Rows.Single( row => row.Id == shop );
		var shopTotal = state.TakingsFor( shop ) - state.CostsFor( shop );
		CollectionAssert.AreEqual( new[] { 0, state.RingsFor( shop ).Customers.LastDays( 30 ), 52, shopTotal, 491 },
			shopRow.Numbers!.ToArray() );
		Assert.AreEqual( "$ 52", shopRow.Values![1] );
		Assert.AreEqual( ParkItemsScreen.Money( shopTotal ), shopRow.Values[2] );
		Assert.AreEqual( "", shopRow.Values[3], "the satisfaction is a bar" );

		show.Invoke( screen, [2] );
		var sideshows = lists[6];
		CollectionAssert.AreEqual( new[] { false, false, true, false, false, true }, sideshows.BarColumns );
		Assert.IsTrue( sideshows.Rows.Count > 0, "the stock park has sideshows" );

		foreach ( var row in sideshows.Rows )
		{
			var placed = state.Objects.Single( each => each.ThingId == row.Id );
			catalogue.TryGet( placed.CatalogueId, out var game );
			Assert.AreEqual( ((ParkRideScore.ExcitementOf( placed, game, state.TrackRides ) & 0xff) << 10) / 100, row.Numbers![2] );
		}

		var sideshowRow = sideshows.Rows.Single( row => row.Id == sideshow );
		Assert.AreEqual( 52, sideshowRow.Numbers![3] );
		Assert.AreEqual( state.TakingsFor( sideshow ) - state.CostsFor( sideshow ), sideshowRow.Numbers[4] );
		Assert.AreEqual( 491, sideshowRow.Numbers[5] );
		CollectionAssert.AreEqual( new[] { "", "$ 52", "" }, new[] { sideshowRow.Values![1], sideshowRow.Values[2], sideshowRow.Values[4] } );
	}

	/// <summary>
	/// <b>The all-staff, all-items and visitors lists' headings are framed as the original's openers frame them</b>
	/// (<c>FUN_00486660</c>): <c>!cbut</c>, font slot 8, 255, 239, 0, the text centred, and each column's own help row,
	/// the items' by tab (<c>docs/exe/hud.md</c>, "The list headings' frames").
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a screen's headings bare; another font or colour; a help row off by one, or the items' rides' rows
	/// kept on the shops' tab; the sideshows' third column's help taken from the shops'. Another frame's name is the
	/// game's to catch: a mesh loads only beside the renderer, and the screens log each heading's.
	/// </remarks>
	[TestMethod]
	public void TheStaffItemsAndVisitorsListsFrameTheirHeadingsWithEachColumnsHelpRow()
	{
		static void Framed( UiList list, int first, int[] helps, string screen )
		{
			for ( var column = 0; column < helps.Length; ++column )
			{
				var heading = list.Children.OfType<UiButton>().Single( each => each.Id == 0x10 + column );
				var at = $"{screen} column {column}";

				Assert.AreEqual( 8, heading.Font, at );
				Assert.AreEqual( new UiColour( 255, 239, 0 ), heading.TextColour, at );
				// Centred only where a frame is named (UiList.AddHeading); the frame itself needs the renderer to load.
				Assert.AreEqual( TextAlign.Centre, heading.TextAcross, at );
				Assert.AreEqual( helps[column], heading.HelpText, at );
				Assert.AreEqual( Localization.Text( first + column ), heading.Text, at );
			}
		}

		var staff = new ParkStaffScreen( stack );
		Framed( Field<UiList>( staff, "_list" ), 101, [119, 120, 121, 122, 123], "staff" );

		var visitors = new ParkVisitorsScreen( stack );
		Framed( Field<UiList>( visitors, "_list" ), 113, [129, 130, 131, 132, 133, 134], "visitors" );

		typeof( ParkItemsScreen ).GetField( "_lastTab", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, 0 );
		var items = new ParkItemsScreen( stack );
		var lists = Field<Dictionary<int, UiList>>( items, "_lists" );
		var show = typeof( ParkItemsScreen ).GetMethod( "Show", Private )!;

		Framed( lists[5], 82, [105, 106, 107, 108, 109], "rides" );

		show.Invoke( items, [1] );
		Framed( lists[5], 87, [105, 584, 585, 586, 587], "shops" );

		show.Invoke( items, [2] );
		Framed( lists[6], 92, [105, 584, 107, 585, 586, 587], "sideshows" );

		show.Invoke( items, [3] );
		Framed( lists[2], 98, [105, 588], "misc" );

		Assert.IsTrue( lines.Any( line => line.StartsWith( "Staff list: headings " ) && line.Contains( " 8 119 \"Name\"" ) ), "the staff's log line" );
	}

	/// <summary>
	/// A money cell letters UITEXT 449 and the figure, or 450 and the figure without its sign under nought
	/// (<c>0x004903e0</c>).
	/// </summary>
	[TestMethod]
	public void AMoneyCellIsTheDollarAndTheFigureOrTheMinusDollarAndItsSize()
	{
		Assert.AreEqual( "$ 0", ParkItemsScreen.Money( 0 ) );
		Assert.AreEqual( "$ 1234", ParkItemsScreen.Money( 1234 ) );
		Assert.AreEqual( "-$ 55", ParkItemsScreen.Money( -55 ) );
	}
}
