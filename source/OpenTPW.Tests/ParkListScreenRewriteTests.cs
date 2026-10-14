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
		var opened = list.Rows[at].Values!.ToArray();
		var repair = (int)ride.StateOfRepair == 40 ? 60f : 40f;

		state.ReplaceObject( ride with { StateOfRepair = repair } );

		Time.PinWall( 6999 );
		screen.Update();
		CollectionAssert.AreEqual( opened, list.Rows[at].Values!.ToArray(), "1999 ms: not rewritten" );

		Time.PinWall( 7000 );
		screen.Update();
		Assert.AreEqual( $"{(int)repair}", list.Rows[at].Values![2], "the state of repair rewritten" );
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
}
