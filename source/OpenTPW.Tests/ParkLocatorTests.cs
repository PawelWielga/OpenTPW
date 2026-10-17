using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVeldrid;
using OpenTPW.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
		Unimplemented.Forget();

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
		level.Locator = _gadget.Locator;
		typeof( Level ).GetField( "_windows", BindingFlags.NonPublic | BindingFlags.Instance )!.SetValue( level, _stack );

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
		Input.Keyboard = new();
		ParkOrbitCameraMode.PointOfInterest = _looksAt;
		ParkBuildMode.Forget();
		Unimplemented.Forget();
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

	/// <summary>The first cell of path in the shipped park, row by row.</summary>
	private static (int X, int Y) APathCell()
	{
		for ( var y = 0; y < ParkWorld.MapSize; y++ )
		{
			for ( var x = 0; x < ParkWorld.MapSize; x++ )
			{
				if ( ParkState.CellFor( Level.Current!.Park!, x, y ).Type == CellEdge.Path )
					return (x, y);
			}
		}

		throw new AssertFailedException( "the shipped park has path" );
	}

	/// <summary>A guest in the park, admitted on the road's far side where the shipped park has none inside.</summary>
	private int AGuest() => _people.Guests.Count > 0 ? _people.Guests.Keys.First() : _people.Admit( 55, 30 );

	/// <summary>A click on a cell with one thing under the pointer: a person as the person, anything else as the object.</summary>
	private string ClickAt( int x, int y, int thing )
		=> _people.Guests.ContainsKey( thing ) || _people.IsStaff( thing )
			? Level.Current!.ClickWorldAt( x, y, thing, 0 )
			: Level.Current!.ClickWorldAt( x, y, 0, thing );

	/// <summary>
	/// <b>A guest under the pointer leaves a click on a ride to the ride</b> (<c>FUN_004879d0</c>: category 4 opens the
	/// object the hover took from the cell, <c>[0x007c2654]</c>, kept apart from the person, <c>[0x007c2650]</c>). The
	/// pointer over the Belly Bounce's anchor (51,23), a guest heading its list and latched, takes the guest and the ride
	/// as two values; nobody latched, the ride alone. With no locator and no modifier the click opens the ride's window;
	/// with Shift alone the same two open the guest's; the frame's own click opens the ride's; a member under the pointer
	/// is still their window first.
	/// </summary>
	/// <remarks><b>Mutations:</b> the latched person dropped; the cell asked with people; the object window given the
	/// person; the frame's click given the person twice.</remarks>
	[TestMethod]
	public void AGuestUnderThePointerLeavesAClickOnARideToTheRide()
	{
		var state = Level.Current!.ParkState!;
		var guest = AGuest();
		var member = _people.Staff[0].ThingId;
		typeof( Level ).GetProperty( nameof( Level.Kind ) )!.SetValue( Level.Current, Level.Scene.Park );

		state.StandOn( guest, 51, 23 );
		Assert.AreEqual( guest, state.CellAt( 51, 23 ).Occupant, "the guest heads the ride's anchor" );

		var (person, placed) = ParkPicking.Take( state, guest, 51, 23 );
		Assert.AreEqual( (guest, 13), (person, placed), "the guest latched, and the ride under them" );
		Assert.AreEqual( (0, 13), ParkPicking.Take( state, 0, 51, 23 ), "nobody latched: the ride alone" );

		StringAssert.Contains( Level.Current.ClickWorldAt( 51, 23, person, placed ), "opened the window for thing 13" );
		Assert.AreEqual( 13, _stack.Windows.OfType<ParkObjectWindow>().Single().ThingId );
		Assert.IsFalse( _stack.Windows.OfType<ParkVisitorWindow>().Any() );
		_stack.Close( _stack.Windows.OfType<ParkObjectWindow>().Single() );

		Input.Keyboard = new Input.KeyboardInfo( new List<Key> { Key.ShiftLeft } );
		StringAssert.Contains( Level.Current.ClickWorldAt( 51, 23, person, placed ), $"opened the visitor window for thing {guest}" );
		Input.Keyboard = new();
		_stack.Close( _stack.Windows.OfType<ParkVisitorWindow>().Single() );

		// The frame's own click hands the two values on as the pointer left them (Level.WorldClick).
		var cell = typeof( ParkPicking ).GetProperty( nameof( ParkPicking.Cell ) )!;
		var personUnder = typeof( ParkPicking ).GetProperty( nameof( ParkPicking.PersonUnderCursor ) )!;
		var objectUnder = typeof( ParkPicking ).GetProperty( nameof( ParkPicking.ObjectUnderCursor ) )!;
		var worldClick = typeof( Level ).GetMethod( "WorldClick", BindingFlags.Instance | BindingFlags.NonPublic )!;

		try
		{
			// What the interface's update leaves for the park's click, which an earlier test's press can leave set.
			typeof( WindowStack ).GetProperty( nameof( WindowStack.PointerTaken ), BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, false );
			typeof( WindowStack ).GetProperty( nameof( WindowStack.ParkScreenOpen ), BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, false );
			cell.SetValue( null, MapStep.CellId( 51, 23 ) );
			personUnder.SetValue( null, person );
			objectUnder.SetValue( null, placed );

			foreach ( var down in new[] { true, false } )
			{
				Input.Mouse = new() { Left = down, Position = new Vector2( 1024, 600 ) };
				worldClick.Invoke( Level.Current, [] );
			}

			Assert.AreEqual( 13, _stack.Windows.OfType<ParkObjectWindow>().Single().ThingId, "the frame's click: the ride's window" );
			_stack.Close( _stack.Windows.OfType<ParkObjectWindow>().Single() );
		}
		finally
		{
			cell.SetValue( null, 0 );
			personUnder.SetValue( null, 0 );
			objectUnder.SetValue( null, 0 );
			Input.Mouse = new();
		}

		StringAssert.Contains( Level.Current.ClickWorldAt( 51, 23, member, placed ), $"opened the staff window for thing {member}" );
		Assert.IsFalse( _stack.Windows.OfType<ParkObjectWindow>().Any() );
	}

	/// <summary>
	/// <b>With the staff's locator open a click on the park arms no tool and opens no window</b> (<c>0x0048721c</c>): bare
	/// ground (43,16), path (49,23), the Belly Bounce's anchor (51,23) and a guest all do nothing; a member under the pointer
	/// still has their window. With it folded away the same ground arms the path tool, which (43,16), outside the park, refuses at once.
	/// </summary>
	/// <remarks><b>Mutations:</b> the locator not asked; the visitors' alone stopping the ground; the member's window refused.</remarks>
	[TestMethod]
	public void WithTheStaffLocatorOpenAClickOnTheParkDoesNothingButAMembersWindow()
	{
		var (pathX, pathY) = APathCell();
		var guest = AGuest();

		CtrlS();
		Assert.AreEqual( ParkLocator.Kind.Staff, Locator.Open );

		foreach ( var (x, y, thing) in new[] { (43, 16, 0), (pathX, pathY, 0), (51, 23, 13), (49, 23, guest) } )
		{
			StringAssert.Contains( ClickAt( x, y, thing ), "nothing to do", $"({x},{y}) thing {thing}" );
			Assert.AreEqual( ParkBuildMode.None, ParkBuildMode.Current, $"({x},{y}) armed nothing" );
		}

		Assert.IsFalse( _stack.Windows.Any( window => window is ParkObjectWindow or ParkVisitorWindow ) );

		var member = _people.Staff[0];
		StringAssert.Contains( ClickAt( 49, 23, member.ThingId ), $"opened the staff window for thing {member.ThingId}" );
		Assert.IsTrue( _stack.Windows.OfType<ParkStaffWindow>().Any() );

		_stack.Close( _stack.Windows.OfType<ParkStaffWindow>().Single() );
		Fold();
		Assert.AreEqual( ParkLocator.Kind.None, Locator.Open );
		StringAssert.StartsWith( ClickAt( 43, 16, 0 ), "world click: path", "with no locator the ground arms the path tool" );
	}

	/// <summary>
	/// <b>With the visitors' locator open and no modifier a guest clicked opens their window and nothing else does</b>
	/// (<c>0x00487009</c> to <c>0x0048702d</c>, category 7 to <c>FUN_004b79b0</c>), not even a member; with Control held the member's window
	/// opens again and the ground still does nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the guest's window not opened; a member's opened with no modifier; Control taken as none.</remarks>
	[TestMethod]
	public void WithTheVisitorsLocatorOpenAGuestClickedOpensTheirWindowAndAMemberNothing()
	{
		var guest = AGuest();
		var member = _people.Staff[0];

		CtrlV();
		Assert.AreEqual( ParkLocator.Kind.Visitors, Locator.Open );

		StringAssert.Contains( ClickAt( 49, 23, member.ThingId ), "nothing to do" );
		StringAssert.Contains( ClickAt( 43, 16, 0 ), "nothing to do" );
		Assert.IsFalse( _stack.Windows.Any( window => window is ParkStaffWindow or ParkVisitorWindow ) );

		StringAssert.Contains( ClickAt( 49, 23, guest ), $"opened the visitor window for thing {guest}" );
		Assert.AreEqual( guest, _stack.Windows.OfType<ParkVisitorWindow>().Single().ThingId );
		_stack.Close( _stack.Windows.OfType<ParkVisitorWindow>().Single() );

		CtrlV();
		Input.Keyboard = new Input.KeyboardInfo( new List<Key> { Key.ControlLeft } );
		StringAssert.Contains( ClickAt( 43, 16, 0 ), "nothing to do" );
		Assert.AreEqual( ParkBuildMode.None, ParkBuildMode.Current );
		StringAssert.Contains( ClickAt( 49, 23, member.ThingId ), "opened the staff window" );
	}

	/// <summary>
	/// <b>Shift alone with no locator open is the guest pick</b> (<c>0x00487009</c> to <c>0x0048702d</c>): a guest's window, and nothing for the
	/// ground, the ride or a member; let go, the ground arms the path tool.
	/// </summary>
	/// <remarks><b>Mutations:</b> Shift not asked; Shift with a locator taken as the pick; the member's window under Shift.</remarks>
	[TestMethod]
	public void ShiftAloneWithNoLocatorPicksAGuestAndNothingElse()
	{
		var guest = AGuest();
		var member = _people.Staff[0];

		Input.Keyboard = new Input.KeyboardInfo( new List<Key> { Key.ShiftLeft } );

		foreach ( var (x, y, thing) in new[] { (43, 16, 0), (51, 23, 13), (49, 23, member.ThingId) } )
			StringAssert.Contains( ClickAt( x, y, thing ), "nothing to do", $"({x},{y}) thing {thing}" );

		Assert.AreEqual( ParkBuildMode.None, ParkBuildMode.Current );
		StringAssert.Contains( ClickAt( 49, 23, guest ), $"opened the visitor window for thing {guest}" );

		Input.Keyboard = new();
		_stack.Close( _stack.Windows.OfType<ParkVisitorWindow>().Single() );
		StringAssert.StartsWith( ClickAt( 43, 16, 0 ), "world click: path" );
	}

	/// <summary>
	/// <b>The hover shows the path's row where the click would arm the path tool, and a person's where it would take
	/// them</b>: 441 over bare ground and 442 over path with no locator open; a member 555 with their name and a guest 553
	/// with their number (554 at nought), each with the pick pointer; with the staff's locator open and nobody taken the
	/// row last posted stands, and where the guests decide and nobody is taken the row is emptied (<c>FUN_00486d90</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the rows swapped, the values dropped, the row not kept under a locator, the row kept where
	/// the guests decide, the pointer not put up.</remarks>
	[TestMethod]
	public void TheHoverShowsThePathsRowOrThePersonsWhereTheClickWouldTakeThem()
	{
		_stack.Update();
		var (pathX, pathY) = APathCell();
		var guest = AGuest();
		var number = 13;
		var member = _people.Staff[0];
		var level = Level.Current!;
		int Row( int x, int y, int thing ) => level.WorldHelpRow( x, y, thing );

		Assert.AreEqual( (441, 442, 441), (Row( 43, 16, 0 ), Row( pathX, pathY, 0 ), Row( 43, 16, guest )) );
		Assert.IsFalse( level.PersonPointer );

		Assert.AreEqual( 555, Row( 43, 16, member.ThingId ), "a member with no locator open" );
		Assert.AreEqual( $"{member.Name} : Left-click to view this staff member's details",
			Localization.FormatHelp( 555, level.WorldHelpValues ) );
		Assert.IsTrue( level.PersonPointer );
		Assert.AreEqual( 441, Row( 43, 16, 0 ) );

		CtrlS();
		Assert.AreEqual( (441, 441), (Row( pathX, pathY, 0 ), Row( 43, 16, guest )), "the ground's row stands under the locator" );
		Assert.IsFalse( level.PersonPointer );
		Assert.AreEqual( 555, Row( 43, 16, member.ThingId ) );
		Assert.AreEqual( 555, Row( 43, 16, member.ThingId ) );
		Assert.AreEqual( (555, 555), (Row( 43, 16, 0 ), Row( 43, 16, guest )), "the member's row stands" );
		Assert.AreEqual( member.Name, level.WorldHelpValues.Single().Value );
		Assert.IsFalse( level.PersonPointer );

		Fold();
		CtrlV();
		var numbered = _people.Guests[guest].VisitorNumber;
		_people.Guests[guest].VisitorNumber = 0;
		Assert.AreEqual( 554, Row( 43, 16, guest ), "New Visitor at nought" );
		Assert.AreEqual( 0, level.WorldHelpValues.Length );
		_people.Guests[guest].VisitorNumber = number;
		Assert.AreEqual( 553, Row( 43, 16, guest ) );
		Assert.AreEqual( $"Visitor #{number} : Left-click to view this person's details", Localization.FormatHelp( 553, level.WorldHelpValues ) );
		Assert.IsTrue( level.PersonPointer );

		// The help bar fills the row's parameters with the hover's values.
		var popupHelp = GameOptions.Current.PopupHelp;
		GameOptions.Current.PopupHelp = true;
		UI.WindowStack.WorldHelpText = 553;
		UI.WindowStack.WorldHelpValues = level.WorldHelpValues;
		Input.Mouse = new();
		_stack.Update();
		Assert.AreEqual( $"Visitor #{number} : Left-click to view this person's details", _stack.HelpBarText );
		GameOptions.Current.PopupHelp = popupHelp;
		_people.Guests[guest].VisitorNumber = numbered;
		UI.WindowStack.WorldHelpText = -1;
		UI.WindowStack.WorldHelpValues = [];
		Assert.AreEqual( (-1, -1), (Row( 43, 16, member.ThingId ), Row( 43, 16, 0 )), "the guests decide: the row is emptied" );
		Assert.IsFalse( level.PersonPointer );
		Assert.AreEqual( 0, level.WorldHelpValues.Length );
	}

	/// <summary>
	/// <b>Where the guests decide and nobody is taken, the empty row is posted only after a category</b>
	/// (<c>0x004870f3</c>): Shift over bare ground empties the ground's row; with the visitors' locator open, Ctrl held
	/// takes the category to nought with the ground's row standing, and let go the row still stands; off a guest it is
	/// emptied; and a guest's row posted as the pointer comes onto the view stands off them, the category nought after
	/// the entry.
	/// </summary>
	/// <remarks><b>Mutations:</b> always emptied; never emptied; the last category not kept; Ctrl's hover not nought; the
	/// entry not nought.</remarks>
	[TestMethod]
	public void TheGuestsEmptyRowIsPostedOnlyAfterACategory()
	{
		_stack.Update();
		var guest = AGuest();
		var level = Level.Current!;
		int Row( int thing, bool onTheView = true ) => level.WorldHelpRow( 43, 16, thing, onTheView );
		void Hold( params Key[] keys ) => Input.Keyboard = new Input.KeyboardInfo( keys.ToList() );

		Assert.AreEqual( (441, 441), (Row( 0 ), Row( 0 )), "onto the view, then the ground's category" );
		Hold( Key.ShiftLeft );
		Assert.AreEqual( (-1, -1), (Row( 0 ), Row( 0 )), "Shift alone: emptied after the ground's category" );

		Hold();
		Assert.AreEqual( 441, Row( 0 ) );
		CtrlV();
		Hold( Key.ControlLeft );
		Assert.AreEqual( 441, Row( 0 ), "Ctrl held: the locator's, nought, the row standing" );
		Hold();
		Assert.AreEqual( (441, 441), (Row( 0 ), Row( 0 )), "let go: nought before, so the row stands" );

		var numbered = _people.Guests[guest].VisitorNumber;
		_people.Guests[guest].VisitorNumber = 13;
		Assert.AreEqual( 553, Row( guest ) );
		Assert.AreEqual( (-1, -1), (Row( 0 ), Row( 0 )), "off the guest: emptied after their category" );

		Row( guest, onTheView: false );
		Assert.AreEqual( 553, Row( guest ), "onto the view, onto the guest" );
		Assert.AreEqual( 553, Row( 0 ), "off them at once: the category nought after the entry" );
		_people.Guests[guest].VisitorNumber = numbered;
	}

	/// <summary>
	/// <b>The hover's arms that post nothing leave the row standing, and the switch's default empties it only after a
	/// category</b> (<c>FUN_00486d90</c>): from bare ground onto a hoarding 441 stands (<c>0x004873bd</c>), and on from
	/// there onto a cell of map type 7 beside the gate and one of type 2, the default (<c>0x004877a3</c>), it still
	/// stands; from bare ground straight onto either it is emptied; off the map it stands (<c>0x00487240</c>), counted; an object's
	/// cell whose parent holds no object stands (<c>0x00487667</c>), and one whose parent holds one empties it; a type-12
	/// track cell under another parent is counted. With a tool armed the empty row is posted only after a category
	/// (<c>0x00486fd9</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the hoarding not silent; the default always emptied; the default never emptied; no cell
	/// not silent; the edge cell's count dropped; the object's cell silent with its object; not silent without; the tool's arm always emptied; never
	/// emptied; the category not nought after the tool's arm; the track's count dropped.</remarks>
	[TestMethod]
	public void TheHoversSilentArmsLeaveTheRowStanding()
	{
		_stack.Update();
		var level = Level.Current!;
		var state = level.ParkState!;
		int Row( (int X, int Y) cell ) => level.WorldHelpRow( cell.X, cell.Y, 0 );
		var (ground, hoarding, outside) = ((43, 16), (42, 16), (44, 16));
		var two = Enumerable.Range( 0, 128 * 128 ).Select( i => (X: i % 128, Y: i / 128) ).First( c => state.Record( c.X, c.Y ).Type == 2 );
		var footprint = Enumerable.Range( 0, 128 * 128 ).Select( i => (X: i % 128, Y: i / 128) )
			.First( c => state.Record( c.X, c.Y ).Type == CellEdge.Footprint && ParkItemHeights.Owner( state, c.X, c.Y ) != null );
		Assert.AreEqual( (0, 12, 7), (state.Record( 42, 16 ).Type, state.Record( 42, 16 ).TrackType, state.Record( 44, 16 ).Type) );

		Assert.AreEqual( (441, 441), (Row( ground ), Row( ground )), "onto the view, then the ground's category" );
		Assert.AreEqual( 441, Row( hoarding ), "onto a hoarding: nothing posted" );
		Assert.AreEqual( (441, 441), (Row( outside ), Row( two )), "on to the default from nought: the row stands" );
		Assert.AreEqual( 441, Row( ground ) );
		Assert.AreEqual( (-1, -1), (Row( outside ), Row( hoarding )), "straight onto the default: emptied after the ground's" );
		Row( ground );
		Assert.AreEqual( -1, Row( two ), "map type 2 the same" );
		Row( ground );
		Assert.AreEqual( (441, 441), (Row( (-1, -1) ), Row( outside )), "off the map nothing posted, and nought after it" );
		Assert.IsTrue( Unimplemented.Summary.Any( gap => gap.What == "PICK_EDGE_CELL" ), "the original's picker holds an edge cell" );

		Row( ground );
		Assert.AreEqual( (-1, -1), (Row( footprint ), Row( outside )), "an object's cell: its category, its row not built" );
		var orphan = (X: 60, Y: 30);
		var was = state.Record( orphan.X, orphan.Y );
		Assert.AreEqual( 441, Row( ground ) );
		state.SetRecord( orphan.X, orphan.Y, was with { Type = CellEdge.Footprint, ParentId = (ushort)((orphan.Y * 128) + orphan.X + 1) } );
		Assert.AreEqual( 441, Row( orphan ), "an object's cell whose parent holds no object: nothing posted" );
		state.SetRecord( orphan.X, orphan.Y, was with { TrackType = 12, TrackParentId = (ushort)((orphan.Y * 128) + orphan.X + 2) } );
		Row( orphan );
		Assert.IsTrue( Unimplemented.Summary.Any( gap => gap.What == "HOVER_TRACK_CELL_CATEGORY" ), "a type-12 cell under another parent" );
		state.SetRecord( orphan.X, orphan.Y, was );

		Row( ground );
		Row( hoarding );
		ParkBuildMode.Arm( ParkBuildMode.Path );
		Assert.AreEqual( (441, 441), (Row( ground ), Row( ground )), "a tool armed after nought: the row stands" );
		ParkBuildMode.Disarm();
		Assert.AreEqual( 441, Row( ground ) );
		ParkBuildMode.Arm( ParkBuildMode.Path );
		Assert.AreEqual( (-1, -1), (Row( ground ), Row( hoarding )), "after the ground's category: emptied, then nought" );
		ParkBuildMode.Disarm();
		Assert.AreEqual( (441, 441), (Row( ground ), Row( hoarding )) );
	}

	/// <summary>
	/// <b>The pointer asks the cell only for what is not a person</b> (<see cref="ParkPicking.ThingOn"/> with
	/// <c>people: false</c>): people are taken by their pictures (<see cref="SpritePick"/>). A guest heading a cell's list
	/// is passed over for what stands behind them, a member too, and a console click on the cell still takes the head.
	/// </summary>
	/// <remarks><b>Mutations:</b> people never passed over; staff not passed over; the list's head alone asked.</remarks>
	[TestMethod]
	public void ThePointerAsksTheCellOnlyForWhatIsNotAPerson()
	{
		var state = Level.Current!.ParkState!;
		var guest = AGuest();
		var member = _people.Staff[0].ThingId;
		var (x, y) = (Enumerable.Range( 30, 60 ).First( column => state.CellAt( column, 60 ).Occupant == 0 ), 60);
		var behind = state.Objects.First().ThingId;

		state.StandOn( behind, x, y );
		state.StandOn( member, x, y );
		state.StandOn( guest, x, y );

		Assert.AreEqual( guest, ParkPicking.ThingOn( state, x, y ), "a click on the cell: the head" );
		Assert.AreEqual( behind, ParkPicking.ThingOn( state, x, y, people: false ), "the guest and the member passed over" );
	}

	/// <summary>
	/// <b>The hover asks whether the pointer is on the park's view where the pointer is now</b>, through the frame's own
	/// entry (<c>Level.WorldHelpRow()</c>), which runs before the interface's update: the pointer moved onto the gadget,
	/// and the stack not yet updated, nothing behind it is taken and the row stands, with the arrow.
	/// </summary>
	/// <remarks><b>Mutations:</b> the gate taken as the view; the gate from the stack's last update.</remarks>
	[TestMethod]
	public void TheHoverAsksTheViewWhereThePointerIsNow()
	{
		var level = Level.Current!;
		var entry = typeof( Level ).GetMethod( "WorldHelpRow", BindingFlags.NonPublic | BindingFlags.Instance, System.Type.EmptyTypes )!;
		int Hover() => (int)entry.Invoke( level, [] )!;

		Input.Mouse = new() { Position = new Vector2( 1800, 200 ) };
		_stack.Update();
		level.WorldHelpRow( 43, 16, 0 );
		Assert.AreEqual( 441, level.WorldHelpRow( 43, 16, 0 ) );

		Input.Mouse = new() { Position = new Vector2( 200, 1350 ) };
		Assert.AreEqual( 441, Hover(), "over the gadget before the stack's update: the row stands" );
		Assert.AreEqual( Input.CursorTypes.Normal, level.IdleCursor() );
		Input.Mouse = new();
	}

	/// <summary>
	/// <b>The hover sets the member idle as it posts their row, wherever PICK UP would take them</b>
	/// (<c>FUN_00505f50</c> asks <c>FUN_00505bd0</c>, then <c>SetState( 0 )</c>): once as the pointer comes onto them and
	/// not again while it stays, from a walk with the clock as the stamp, never a member on strike. Off the park's view
	/// (<c>[0x007c24fc]</c>) no hover runs and the row stands; coming back onto it the member is posted twice, the thing
	/// forgotten before and the category nought after (<c>0x0048884e</c>). Over a control the pointer is the arrow, as
	/// the original's over the gadget's buttons.
	/// </summary>
	/// <remarks><b>Mutations:</b> not set; set in any state; set every frame; set off the view; the stamp nought from a
	/// walk; the thing not forgotten as the pointer comes onto the view; the category not nought after it; the entry
	/// never seen; the hand off the view; a control under the pointer taken for the view.</remarks>
	[TestMethod]
	public void TheHoverSetsTheMemberIdleWherePickUpWouldTakeThem()
	{
		_stack.Update();
		var level = Level.Current!;
		var state = level.ParkState!;
		var member = _people.Staff.First( staff => Peep.CountsOn( state.Record( staff.Navigator.Position.Cell.X, staff.Navigator.Position.Cell.Y ).Type ) );
		int Row( int thing, bool onTheView = true ) => level.WorldHelpRow( 43, 16, thing, onTheView );
		var sets = 0;

		// Whether the hover set them idle since the last look, counted, and the member walking again either way.
		bool Set()
		{
			var idle = member.Activity == StaffActivity.Idle;
			sets += idle ? 1 : 0;
			member.SetActivity( StaffActivity.Walking, state.GameTick );
			return idle;
		}

		member.SetActivity( StaffActivity.Walking, state.GameTick );

		Input.Mouse = new() { Position = new Vector2( 200, 1350 ) };
		_stack.Update();
		Assert.IsTrue( _stack.Hovered != null && !_stack.PointerOnTheView, "over the gadget" );
		Input.Mouse = new() { Position = new Vector2( 1800, 200 ) };
		_stack.Update();
		Assert.IsTrue( _stack.Hovered == null && _stack.PointerOnTheView, "over the park" );
		Input.Mouse = new();

		Row( 0 );
		Row( 0 );
		Assert.AreEqual( 555, Row( member.ThingId ) );
		Assert.AreEqual( (StaffActivity.Idle, state.GameTick), (member.Activity, member.TimeStartedIdling), "from a walk, the clock" );
		Assert.IsTrue( Set(), "as the pointer comes onto them" );
		Row( member.ThingId );
		Assert.IsFalse( Set(), "not again while the pointer stays on them" );

		Row( 0 );
		member.SetActivity( StaffActivity.OnStrike, state.GameTick );
		Row( member.ThingId );
		Assert.AreEqual( StaffActivity.OnStrike, member.Activity, "on strike, PICK UP would not take them" );
		member.SetActivity( StaffActivity.Walking, state.GameTick );

		Row( 0 );
		Assert.AreEqual( 441, Row( member.ThingId, onTheView: false ), "off the view the row last posted stands" );
		Assert.IsFalse( Set(), "no hover off the view" );
		Assert.AreEqual( Input.CursorTypes.Normal, level.IdleCursor(), "the arrow over a control" );

		Row( 0 );
		Row( member.ThingId );
		Assert.IsTrue( Set() );
		Row( member.ThingId, onTheView: false );
		Assert.AreEqual( Input.CursorTypes.Normal, level.IdleCursor(), "the arrow over a control, the member's row standing" );
		Assert.AreEqual( 555, Row( member.ThingId ), "back onto the view, onto the member they last were" );
		Assert.AreEqual( Input.CursorTypes.Pick, level.IdleCursor() );
		Assert.IsTrue( Set(), "the thing forgotten as the pointer comes on" );
		Row( member.ThingId );
		Assert.IsTrue( Set(), "posted again, the category nought after the entry" );
		Row( member.ThingId );
		Assert.IsFalse( Set(), "and then not again" );
		Assert.AreEqual( 4, sets );
	}
}
