using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The staff and visitor locators, Ctrl+S and Ctrl+V (<c>FUN_004b3f60</c>, <c>FUN_004b4280</c>, the panel's handler
/// <c>FUN_004b3790</c>; <c>docs/exe/hud.md</c>, "The staff and visitor locators"), on the shipped park's people, the
/// buttons clicked where the arm puts them. Skipped without the game (<see cref="GameData"/>).
/// </summary>
[TestClass]
public class ParkLocatorTests
{
	private Level? _level;
	private Point2 _screen;
	private float _now;
	private Vector3 _looksAt;
	private ParkPeople _people = null!;
	private WindowStack _stack = null!;
	private ParkGadget _gadget = null!;

	[TestInitialize]
	public void LoadThePark()
	{
		_level = Level.Current;
		_screen = Screen.Size;
		_now = Time.Now;
		_looksAt = ParkOrbitCameraMode.PointOfInterest;

		Screen.Size = new Point2( 2048, 1536 );
		Log ??= new();
		FileSystem = GameData.Required();
		Input.Mouse = new();

		using var stream = new MemoryStream( FileSystem.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", FileSystem );
		var state = new ParkState( world );

		var level = (Level)RuntimeHelpers.GetUninitializedObject( typeof( Level ) );
		typeof( Level ).GetProperty( nameof( Level.Park ) )!.SetValue( level, world );
		typeof( Level ).GetProperty( nameof( Level.ParkState ) )!.SetValue( level, state );
		typeof( Level ).GetProperty( nameof( Level.Catalogue ) )!.SetValue( level, catalogue );
		Level.Current = level;

		_people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state, catalogue );
		_stack = new WindowStack();
		_gadget = new ParkGadget( _stack );
		_stack.Open( _gadget );

		ParkOrbitCameraMode.PointOfInterest = new Vector3( 300f, 300f, 0f );
	}

	[TestCleanup]
	public void PutItAllBack()
	{
		_people?.Delete();
		Entity.ApplyDeletions();
		Level.Current = _level!;
		Screen.Size = _screen;
		Time.Now = _now;
		Input.Mouse = new();
		ParkOrbitCameraMode.PointOfInterest = _looksAt;
	}

	private ParkLocator Locator => _gadget.Locator;

	private void CtrlS()
	{
		Locator.OpenStaff( _gadget.CarryLocatorPanel, () => _stack.CloseParkScreen() );
		ParkGadgetPressTests.Settle( _gadget );
	}

	private void CtrlV()
	{
		Locator.OpenVisitors( _gadget.CarryLocatorPanel, () => _stack.CloseParkScreen() );
		ParkGadgetPressTests.Settle( _gadget );
	}

	/// <summary>Escape over an open locator, and the arm's slide in run to its end.</summary>
	private bool Fold()
	{
		var folded = _gadget.FoldLocator();
		ParkGadgetPressTests.Settle( _gadget );
		return folded;
	}

	/// <summary>A button clicked at its middle, where the arm puts it.</summary>
	private void Click( int id )
	{
		var button = Find( _gadget.Root, id );
		Assert.IsTrue( button.Visible && button.Parent!.Visible && button.Parent.Parent!.Visible, $"{id:x} shown" );
		Assert.IsTrue( _stack.ClickAt( (button.Rect.Left + button.Rect.Right) / 2, (button.Rect.Top + button.Rect.Bottom) / 2 ), $"{id:x} clicked" );
	}

	/// <summary>
	/// <b>The arm puts a panel where its handler figures it</b>: the message bar's 820 and (486, 1112) from the same rule
	/// as the staff panel's 670 and (484, 1127), and the visitors' 670 and (485, 1127); each button moved with its panel.
	/// </summary>
	/// <remarks><b>Mutations:</b> the travel's hundred or its whole division; the middle not halved; the visitors' panel moved as the staff's.</remarks>
	[TestMethod]
	public void ThePanelsStandWhereTheArmPutsThem()
	{
		Assert.AreEqual( 820, ParkGadget.TravelFor( 615 ) );
		Assert.AreEqual( (486, 1112), ParkGadget.PlaceFor( new UiRect( 458, 1101, 1073, 1442 ) ), "the bar, measured in the original" );
		Assert.AreEqual( 670, ParkGadget.TravelFor( 467 ) );

		CtrlS();
		Assert.AreEqual( new UiRect( 484, 1127, 951, 1438 ), Locator.StaffPanel.Rect );
		Assert.AreEqual( new UiRect( 556, 1175, 659, 1278 ), Find( Locator.StaffPanel, 0x95e ).Rect, "the handymen's button" );
		Assert.AreEqual( new UiRect( 331, 1069, 1006 - 645 + 670, 1495 ), Find( _gadget.Root, 0x21 ).Rect, "the arm out 670" );

		CtrlV();
		Assert.AreEqual( new UiRect( 485, 1127, 951, 1438 ), Locator.VisitorPanel.Rect );
		Assert.AreEqual( new UiRect( 666, 1231, 769, 1334 ), Find( Locator.VisitorPanel, 0x964 ).Rect, "the visitors' button" );
		Assert.IsFalse( Locator.StaffPanel.Visible );
	}

	/// <summary>
	/// <b>A kind's button steps its members, newest first and round again</b>: the first press the first, the same
	/// button the next, past the last the first; the strip shows their name and the camera is on them. A kind with
	/// nobody hides the strip and lets the camera go.
	/// </summary>
	/// <remarks><b>Mutations:</b> the step not wrapping; the list taken oldest first; another kind's press stepping on from the last kind's member; the strip left up for a kind with nobody.</remarks>
	[TestMethod]
	public void AKindsButtonStepsItsMembers()
	{
		// Two of a kind hired after the file's, so the kind has at least two and the newest heads it.
		var ada = _people.Hire( new ParkStaffPool.Candidate( Id: 998, Kind: 1, Name: "Ada Test", Grade: 0, Costume: 0, Wage: 69 ), 47, 21 );
		var zed = _people.Hire( new ParkStaffPool.Candidate( Id: 999, Kind: 1, Name: "Zed Test", Grade: 0, Costume: 0, Wage: 69 ), 55, 30 );
		var model = _people.Staff.First( member => member.ThingId == zed ).Model;

		// The stock park has one of each kind: its guard fired, so the guards have nobody.
		Assert.IsTrue( _people.Fire( _people.Staff.Single( member => member.Model == ParkLocator.ModelOf( 3 ) ).ThingId ) );

		var byKind = Enumerable.Range( 1, 5 ).ToDictionary( role => role,
			role => _people.Staff.Where( member => member.Model == ParkLocator.ModelOf( role ) ).ToList() );
		var many = byKind.First( kind => ParkLocator.ModelOf( kind.Key ) == model );
		var none = byKind.First( kind => kind.Value.Count == 0 );
		var button = new[] { 0x95e, 0x95f, 0x962, 0x961, 0x960 }[many.Key - 1];
		var empty = new[] { 0x95e, 0x95f, 0x962, 0x961, 0x960 }[none.Key - 1];
		Assert.AreEqual( (zed, ada), (many.Value[0].ThingId, many.Value[1].ThingId), "the newest first" );

		CtrlS();
		Assert.AreEqual( (ParkLocator.Kind.Staff, 0, 0, false), (Locator.Open, Locator.Role, Locator.Actor, Locator.Strip.Visible) );

		// The handymen's one first, so the mechanics' button is pressed with another kind's member found.
		Click( 0x95e );
		Assert.AreEqual( byKind[1][0].ThingId, Locator.Actor );

		Click( button );
		var first = many.Value[0];
		Assert.AreEqual( (first.ThingId, true, true), (Locator.Actor, Locator.Strip.Visible, Locator.Following) );
		Assert.AreEqual( first.Name, Find( Locator.Strip, 0x147 ).Text );
		Assert.AreEqual( ParkLocator.PointOf( first.Navigator.Previous, first.Navigator.Position, ParkPeople.ThingTickFraction ),
			ParkOrbitCameraMode.PointOfInterest );

		for ( var i = 1; i <= many.Value.Count; ++i )
		{
			Click( button );
			Assert.AreEqual( many.Value[i % many.Value.Count].ThingId, Locator.Actor, $"press {i + 1}" );
		}

		var at = ParkOrbitCameraMode.PointOfInterest;
		Click( empty );
		Assert.AreEqual( (0, false, false), (Locator.Actor, Locator.Strip.Visible, Locator.Following), "a kind with nobody" );

		Click( button );
		Assert.AreEqual( first.ThingId, Locator.Actor, "another kind's press starts from its first" );
		Assert.AreNotEqual( at, new Vector3( 300f, 300f, 0f ) );
	}

	/// <summary>
	/// <b>Each frame the camera is put on whoever it follows</b>, where they are drawn: moved by hand, the next frame
	/// has the camera there, and the arm folding away (Escape) lets it go and closes the locator.
	/// </summary>
	/// <remarks><b>Mutations:</b> the follow not run each frame; Escape leaving the follow on; the fold not closing the locator.</remarks>
	[TestMethod]
	public void TheCameraFollowsUntilTheArmFolds()
	{
		CtrlS();
		var role = Enumerable.Range( 1, 5 ).First( role => _people.Staff.Any( member => member.Model == ParkLocator.ModelOf( role ) ) );
		Click( new[] { 0x95e, 0x95f, 0x962, 0x961, 0x960 }[role - 1] );
		var member = _people.Staff.First( member => member.ThingId == Locator.Actor );

		member.Navigator.Position = new FixedVector( 40 * FixedVector.One + 12345, 30 * FixedVector.One );
		member.Navigator.StampPrevious();
		_gadget.Update();
		Assert.AreEqual( new Vector3( (40 * FixedVector.One + 12345) * 10f / FixedVector.One, 300f, 0f ), ParkOrbitCameraMode.PointOfInterest );

		// Gone from the park: let go of, the strip hidden, the camera left where it was.
		var there = ParkOrbitCameraMode.PointOfInterest;
		Assert.IsTrue( _people.Fire( member.ThingId ) );
		_gadget.Update();
		Assert.AreEqual( (0, false, false, there), (Locator.Actor, Locator.Strip.Visible, Locator.Following, ParkOrbitCameraMode.PointOfInterest) );
		Assert.AreEqual( ParkLocator.Kind.Staff, Locator.Open, "and still open" );

		Assert.IsTrue( Fold() );
		Assert.AreEqual( (ParkLocator.Kind.None, 0, false, false), (Locator.Open, Locator.Actor, Locator.Following, Locator.Strip.Visible) );
		Assert.IsFalse( _gadget.ArmOut );
		Assert.IsFalse( Fold(), "nothing to fold" );

	}

	/// <summary>
	/// <b>Ctrl+S with a locator out leaves its panel out and closed</b>, as the original sets its locator open before
	/// the arm folds the panel it carries: a button then goes down and finds nobody. Ctrl+V sets its own after, and works.
	/// </summary>
	/// <remarks><b>Mutations:</b> Ctrl+S setting its locator open after the arm; Ctrl+V setting its own before.</remarks>
	[TestMethod]
	public void CtrlSOverALocatorLeavesItClosed()
	{
		var role = Enumerable.Range( 1, 5 ).First( role => _people.Staff.Any( member => member.Model == ParkLocator.ModelOf( role ) ) );
		var button = new[] { 0x95e, 0x95f, 0x962, 0x961, 0x960 }[role - 1];

		CtrlS();
		Click( button );
		Assert.AreEqual( (true, true), (Locator.Strip.Visible, ((UiButton)Find( Locator.StaffPanel, button )).IsDown) );

		CtrlS();
		Assert.AreEqual( (ParkLocator.Kind.None, true, false, false), (Locator.Open, Locator.StaffPanel.Visible, Locator.Strip.Visible, Locator.Following) );
		Assert.IsFalse( Locator.StaffPanel.Children[0].Children.OfType<UiButton>().Any( option => option.IsDown ), "every button up" );

		Click( button );
		Assert.AreEqual( (0, false, true), (Locator.Actor, Locator.Strip.Visible, ((UiButton)Find( Locator.StaffPanel, button )).IsDown), "down, and nobody" );

		CtrlV();
		CtrlV();
		Assert.AreEqual( ParkLocator.Kind.Visitors, Locator.Open );

		CtrlS();
		Assert.AreEqual( ParkLocator.Kind.None, Locator.Open, "over the visitors' too" );
	}

	/// <summary>
	/// <b>The visitors' button steps every guest, newest first</b>, the strip lettering "Visitor #" and their number,
	/// or "New Visitor" for one not yet counted, and the thought shown in its corner.
	/// </summary>
	/// <remarks><b>Mutations:</b> the guests oldest first; the number's row or parameter wrong; the status shown for a guest.</remarks>
	[TestMethod]
	public void TheVisitorsButtonStepsTheGuests()
	{
		Assert.IsTrue( _people.Peeps.Count >= 2 );

		CtrlV();
		Assert.AreEqual( (ParkLocator.Kind.Visitors, ParkLocator.VisitorsRole, 0), (Locator.Open, Locator.Role, Locator.Actor) );

		// The game's stock park had its newest guest Visitor #13; a guest not yet counted is the other row.
		_people.Peeps[0].VisitorNumber = 13;
		_people.Peeps[1].VisitorNumber = 0;

		Click( 0x964 );
		var first = _people.Peeps[0];
		Assert.AreEqual( (first.ThingId, true), (Locator.Actor, Locator.Strip.Visible) );
		Assert.AreEqual( "Visitor #13", Find( Locator.Strip, 0x147 ).Text );
		Assert.AreEqual( (false, true), (Find( Locator.Strip, 0x148 ).Visible, Find( Locator.Strip, 0x14a ).Visible) );

		Click( 0x964 );
		Assert.AreEqual( (_people.Peeps[1].ThingId, "New Visitor"), (Locator.Actor, Find( Locator.Strip, 0x147 ).Text) );
	}

	/// <summary>
	/// <b>The advisor tells of a locator as it opens and is taken back as the arm folds it</b>: Ctrl+S posts message
	/// <c>0xc6</c> after the arm takes its panel (<c>FUN_004b3f60</c>), Ctrl+V <c>0x11f</c> last (<c>FUN_004b4280</c>), each
	/// on the park's tick, and the fold (<c>FUN_004b3790</c>'s <c>0x1001d8</c>) withdraws both, which empties their slots
	/// and, while he says one, cuts him off on the game's clock. Ctrl+S over its own locator withdraws its line and posts
	/// it again, so one copy waits. Both are tutorial lines: with the Tutorial option off neither is taken.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> either post missing or the other's id; a post before the arm folds; no withdrawal at the fold,
	/// or one id alone withdrawn; the wall clock for the game's; tick nought for the park's; either row's response or
	/// sample, its category not the tutorial's, or two lines; no park.
	/// </remarks>
	[TestMethod]
	public void TheAdvisorTellsOfALocatorAsItOpensAndIsTakenBackAsItFolds()
	{
		using var clock = new SimulationClockScope();
		var tutorial = true;
		var lines = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ), tutorial: () => tutorial );
		var park = Level.Current!.ParkState!;
		park.AdvisorMessages = lines;
		var said = new List<(int Response, int Sample)>();
		int Play( int response, int sample ) { said.Add( (response, sample) ); return 9000; }

		for ( var i = 0; i < 40; ++i )
			park.AdvanceGameTick();
		var tick = park.GameTick;
		Assert.AreNotEqual( 0, tick );

		// The wall clock far past any line's end: a cut is judged on the game's clock, which stands at nought.
		Time.PinWall( 60000 );

		try
		{
			// The Tutorial option off: neither line is taken.
			tutorial = false;
			CtrlS();
			CtrlV();
			StringAssert.Contains( lines.Census(), "pending=0 ", "the Tutorial option off" );
			Assert.IsTrue( Fold() );
			tutorial = true;

			CtrlS();
			StringAssert.Contains( lines.Census(), "pending=1 attempts=0 threshold=25 slot=198/subject=0 " );
			StringAssert.Contains( lines.Census(), "message=185/score=110/", "posted on the park's own tick" );

			CtrlS();
			StringAssert.Contains( lines.Census(), "pending=1 attempts=0 threshold=25 slot=198/subject=0 ", "withdrawn as the arm folds, then posted again" );

			// The staff's panel folded for the visitors': the staff's line taken back, the visitors' posted.
			CtrlV();
			StringAssert.Contains( lines.Census(), "pending=1 attempts=0 threshold=25 slot=287/subject=0 " );

			lines.Tick( park.GameTick, 0, Play );
			CollectionAssert.AreEqual( new[] { (547, 564) }, said, "response 547, sample 564" );
			StringAssert.Contains( lines.Census(), $"message=287/score=1000/line=0/lastTick={tick}/said=1/slaps=0" );

			// Folded while he says it: cut off.
			Assert.IsTrue( Fold() );
			StringAssert.Contains( lines.Census(), $"message=287/score=1000/line=0/lastTick={tick}/said=0/slaps=1", "cut off" );

			CtrlS();
			lines.Tick( park.GameTick, 0, Play );
			Assert.AreEqual( (417, 389), said[1], "response 417, sample 389" );
			Assert.IsTrue( Fold() );
			StringAssert.Contains( lines.Census(), $"message=198/score=1000/line=0/lastTick={tick}/said=0/slaps=1", "cut off" );
			StringAssert.Contains( lines.Census(), "pending=0 " );

			// One line, so said again it is the same.
			CtrlS();
			lines.Tick( park.GameTick, 0, Play );
			Assert.AreEqual( (417, 389), said[2], "its one line again" );
		}
		finally
		{
			Time.PinWall( null );
		}
	}

	/// <summary>
	/// <b>The camera's point is the person's blend, truncated, ten to a cell</b> (<c>FUN_004f9f00</c>), and the status
	/// picture is the state group's (<c>FUN_00485ec0</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the blend rounded, not truncated; the fraction ignored; a picture off by one.</remarks>
	[TestMethod]
	public void ThePointAndThePictureAreTheOriginals()
	{
		var previous = new FixedVector( 10 * FixedVector.One, 20 * FixedVector.One );
		var position = new FixedVector( 10 * FixedVector.One + 3, 20 * FixedVector.One - 3 );

		Assert.AreEqual( new Vector3( (10 * FixedVector.One + 1) * 10f / FixedVector.One, (20 * FixedVector.One - 2) * 10f / FixedVector.One, 0f ),
			ParkLocator.PointOf( previous, position, 0.5f ), "plus 1.5 and less 1.5, each sum truncated" );
		Assert.AreEqual( new Vector3( (10 * FixedVector.One + 3) * 10f / FixedVector.One, (20 * FixedVector.One - 3) * 10f / FixedVector.One, 0f ),
			ParkLocator.PointOf( previous, position, 1f ) );

		Assert.AreEqual( "0 0 1 2 3 9", string.Join( " ", Enumerable.Range( 0, 6 ).Select( ParkLocator.StatusPart ) ) );
	}

	/// <summary>
	/// <b>A patrol area's edges are <c>FUN_004b3640</c>'s four lines</b>: the top row less its corners, the right column
	/// down, the bottom row, the left column up; a row two cells wide runs back from its right corner, so both corners
	/// are laid twice; a cell off the map is passed over (<c>FUN_00536100</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the rows' corners kept; a narrow row's corners not taken; the left column walked down; the
	/// line along the shorter way; an off-map cell laid.</remarks>
	[TestMethod]
	public void APatrolAreasEdgesAreTheOriginalsFourLines()
	{
		// The stock mechanic's (39,21)-(56,28): every outline cell once, 48.
		var edges = ParkLocator.PatrolEdgeCells( (39, 21), (56, 28) );
		Assert.AreEqual( 48, edges.Count );
		Assert.AreEqual( 48, edges.Distinct().Count() );
		Assert.AreEqual( "(40, 21) (55, 21) (56, 21) (56, 28) (40, 28) (55, 28) (39, 28) (39, 21)",
			string.Join( " ", new[] { 0, 15, 16, 23, 24, 39, 40, 47 }.Select( i => edges[i] ) ) );

		// The stock entertainer's (47,18)-(48,25): two wide, so the top and bottom rows each lay both corners again.
		var narrow = ParkLocator.PatrolEdgeCells( (47, 18), (48, 25) );
		Assert.AreEqual( 20, narrow.Count );
		Assert.AreEqual( 16, narrow.Distinct().Count() );
		Assert.AreEqual( "(48, 18) (47, 18) (48, 18)", string.Join( " ", narrow.Take( 3 ) ) );

		// One wide: the rows are the corner alone.
		Assert.AreEqual( "(5, 5) (5, 5) (5, 6) (5, 7) (5, 7) (5, 7) (5, 6) (5, 5)",
			string.Join( " ", ParkLocator.PatrolEdgeCells( (5, 5), (5, 7) ) ) );

		Assert.AreEqual( "(127, 0) (127, 1) (126, 1) (126, 0)", string.Join( " ", ParkLocator.PatrolEdgeCells( (126, 0), (128, 1) ) ) );
	}

	/// <summary>
	/// <b>The edges are laid each frame for a fenced member followed, and for nobody else</b>: the stock mechanic's 48,
	/// handed to the marker mesh for its frame alone; the researcher, whose area is the whole map, none; none once the
	/// follow ends or the arm folds the locator away.
	/// </summary>
	/// <remarks><b>Mutations:</b> the edges laid for the whole map; left after the frame; never handed to the mesh; kept by
	/// the mesh past its frame.</remarks>
	[TestMethod]
	public void AFencedMembersEdgesAreLaidWhileFollowed()
	{
		CtrlS();
		Click( 0x95f );
		var mechanic = _people.Staff.First( member => member.ThingId == Locator.Actor );
		Assert.AreEqual( ((39, 21), (56, 28)), (mechanic.PatrolFrom, mechanic.PatrolTo), "the stock mechanic's fence" );

		_gadget.Update();
		CollectionAssert.AreEqual( ParkLocator.PatrolEdgeCells( (39, 21), (56, 28) ), Locator.PatrolEdges );
		StringAssert.EndsWith( Locator.Census(), " patrol=48" );

		Click( 0x960 );
		Assert.IsTrue( _people.Staff.First( member => member.ThingId == Locator.Actor ).PatrolsWholeMap, "the researcher's is the map" );
		_gadget.Update();
		Assert.AreEqual( 0, Locator.PatrolEdges.Count, "the whole map" );

		Click( 0x95f );
		_gadget.Update();
		Assert.AreEqual( 48, Locator.PatrolEdges.Count );

		// The camera taken off him (the camcorder's first person) ends the follow, and with it the edges.
		typeof( ParkLocator ).GetProperty( nameof( ParkLocator.Following ), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic )!.SetValue( Locator, false );
		_gadget.Update();
		Assert.AreEqual( 0, Locator.PatrolEdges.Count, "not followed" );

		// Each frame's edges go to the marker mesh, which takes them for its frame and keeps none for the next.
		var markers = (ParkBuildMarkers)RuntimeHelpers.GetUninitializedObject( typeof( ParkBuildMarkers ) );
		var current = typeof( ParkBuildMarkers ).GetProperty( nameof( ParkBuildMarkers.Current ) )!;
		current.SetValue( null, markers );

		try
		{
			Click( 0x95f );
			_gadget.Update();
			Assert.AreEqual( 48, Locator.PatrolEdges.Count );
			CollectionAssert.AreEqual( Locator.PatrolEdges, markers.PatrolQueued.ToList(), "queued for the mesh" );

			typeof( ParkBuildMarkers ).GetMethod( "OnUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic )!
				.Invoke( markers, null );
			Assert.AreEqual( 0, markers.PatrolQueued.Count, "taken by the frame" );
		}
		finally
		{
			current.SetValue( null, null );
		}

		Assert.IsTrue( Fold() );
		_gadget.Update();
		Assert.AreEqual( 0, Locator.PatrolEdges.Count, "folded away" );
	}

	private static UiControl Find( UiControl root, int id )
		=> FindOrNull( root, id ) ?? throw new AssertFailedException( $"no control {id:x}" );

	private static UiControl? FindOrNull( UiControl root, int id )
		=> root.Id == id ? root : root.Children.Select( child => FindOrNull( child, id ) ).FirstOrDefault( found => found != null );
}
