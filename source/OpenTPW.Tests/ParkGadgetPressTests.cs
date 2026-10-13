using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park gadget's body, handle, arm and aerial stop the pointer, as every shown control of the original's does
/// (<c>docs/exe/hud.md</c>, "The management gadget"; <c>docs/exe/park-engine.md</c>, "Whose a right press is"). These
/// read the gadget's meshes from the shipped game and are skipped where there is no installation - see
/// <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkGadgetPressTests
{
	private Point2 _screen;
	private float _now;

	[TestInitialize]
	public void MountTheGame()
	{
		// The interface's own 2048x1536, so a point in the window is the same point on the gadget's layout.
		_screen = Screen.Size;
		_now = Time.Now;
		Screen.Size = new Point2( 2048, 1536 );

		Log ??= new();
		FileSystem = GameData.Required();
		Input.Mouse = new();
		Input.ForgetHeldKeys();
	}

	[TestCleanup]
	public void PutItAllBack()
	{
		Time.Now = _now;
		Time.PinWall( 0 );
		Input.Mouse = new();
		Input.ForgetHeldKeys();
		Screen.Size = _screen;
	}

	/// <summary>
	/// <b>The body takes a press inside its 23-point outline and not in the rest of its rectangle</b>, and a right press
	/// there is the interface's.
	/// </summary>
	/// <remarks><b>Mutations:</b> the body with no outline; the body answering over its whole rectangle.</remarks>
	[TestMethod]
	public void TheBodyTakesAPressInsideItsOutline()
	{
		var (stack, gadget) = AGadget();

		Assert.AreEqual( 0x1d, Under( gadget, 240, 1030 ), "bare metal above the date" );
		Assert.AreEqual( 0x1d, Under( gadget, 300, 1490 ), "bare metal under the camcorder button" );
		Assert.IsNull( Under( gadget, 50, 1000 ), "the rectangle's top-left corner, which the outline leaves bare" );
		Assert.IsNull( Under( gadget, 420, 1500 ), "its bottom-right corner, past the outline and the handle" );

		Assert.IsTrue( stack.TakesRightPress( 240, 1030 ) );
		Assert.IsFalse( stack.TakesRightPress( 50, 1000 ) );
		Assert.IsTrue( stack.ClickAt( 240, 1030 ), "and a left press there goes no further" );
	}

	/// <summary>
	/// <b>With the arm in, the handle sits 645 to the left of the stream's place, past the body's right edge</b>, and the
	/// arm and its end are 30 wide behind the body; out, all three are at the stream's rectangles.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the arm left out at build; the handle not moved with the end; its outline left behind when it
	/// moves; the arm or its end not stopping the pointer; the travel 675, the arm's whole width; the slide not
	/// counted, or counted at the build.
	/// </remarks>
	[TestMethod]
	public void TheHandleFollowsTheArmInAndOut()
	{
		var slides = Times( "GADGET_ARM_SLIDE" );
		var (stack, gadget) = AGadget();

		Assert.AreEqual( slides, Times( "GADGET_ARM_SLIDE" ), "the build puts the arm in and slides nothing" );

		Assert.IsFalse( gadget.ArmOut, "built in" );
		Assert.AreEqual( 0x23, Under( gadget, 460, 1310 ), "the handle, past the body's 439" );
		Assert.AreEqual( 0x23, Under( gadget, 483, 1310 ), "its outline's rightmost point, 1129 less 645, is 484" );
		Assert.IsNull( Under( gadget, 486, 1310 ), "and nothing past it" );
		Assert.IsNull( Under( gadget, 1100, 1300 ), "nothing where the stream puts the handle" );
		Assert.IsNull( Under( gadget, 850, 1150 ), "nor where it puts the arm" );
		Assert.AreEqual( new UI.UiRect( 331, 1069, 361, 1495 ), Find( gadget, 0x21 ).Rect );
		Assert.AreEqual( new UI.UiRect( 361, 1069, 426, 1382 ), Find( gadget, 0x22 ).Rect );
		Assert.AreEqual( new UI.UiRect( 331, 1058, 484, 1503 ), Find( gadget, 0x23 ).Rect );

		Assert.IsTrue( stack.ClickAt( 261, 1414 ), "the camcorder button, 0x27" );

		Assert.IsTrue( gadget.ArmOut );
		Assert.AreEqual( slides + 1, Times( "GADGET_ARM_SLIDE" ), "out, which the original slides" );
		Assert.AreEqual( 0x21, Under( gadget, 850, 1150 ), "the arm's bare panel" );
		Assert.AreEqual( 0x22, Under( gadget, 1030, 1200 ), "its end" );
		Assert.AreEqual( 0x23, Under( gadget, 1100, 1300 ), "the handle inside its outline" );
		Assert.IsNull( Under( gadget, 1120, 1070 ), "and not in the corner of its rectangle the outline leaves bare" );
		Assert.AreEqual( 0x21, Under( gadget, 460, 1310 ), "where the handle was is the arm now" );
		Assert.IsTrue( stack.TakesRightPress( 850, 1150 ) );
		Assert.AreEqual( new UI.UiRect( 331, 1069, 1006, 1495 ), Find( gadget, 0x21 ).Rect );
		Assert.AreEqual( new UI.UiRect( 976, 1058, 1129, 1503 ), Find( gadget, 0x23 ).Rect );

		Assert.IsTrue( stack.ClickAt( 389, 1426 ), "the retract button, 0x24" );

		Assert.IsFalse( gadget.ArmOut );
		Assert.AreEqual( slides + 2, Times( "GADGET_ARM_SLIDE" ), "and in" );
		Assert.AreEqual( 0x23, Under( gadget, 460, 1310 ), "and the handle is back" );
		Assert.IsNull( Under( gadget, 850, 1150 ) );
	}

	/// <summary>
	/// <b>Where two of the body's children meet, the later in the stream answers</b> (<c>FUN_0065db25</c>): the six
	/// buttons' panel before the arm, the arm before the date, any of them before the body.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the arm added after the buttons' panel; the arm added before the date; the panel not stopping
	/// the pointer.
	/// </remarks>
	[TestMethod]
	public void TheLaterChildAnswersWhereTwoMeet()
	{
		var (_, gadget) = AGadget();

		Assert.AreEqual( 0x21, Under( gadget, 345, 1100 ), "the date's (153,1044)-(404,1121) under the arm's 331 to 361" );
		Assert.AreEqual( 0x20, Under( gadget, 300, 1100 ), "the date beside it" );
		Assert.AreEqual( 0x25, Under( gadget, 340, 1127 ), "the buttons' panel over the arm, above the info button" );
		Assert.AreEqual( 0x2b, Under( gadget, 390, 1370 ), "research's corner, which the handle's outline reaches" );
		Assert.AreEqual( 0x23, Under( gadget, 395, 1370 ), "and the handle just past it" );
	}

	/// <summary>
	/// <b>The arm and its end are drawn behind the body and the handle in front of it</b>, by the depths
	/// <c>FUN_004a1d70</c> gives them: 4, 4, the retract button 7, the body 8, the handle 9, the rest 10 and up.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the body not drawing by depth; the handle left at its parent's depth and two; the sort not
	/// stable; a hidden control drawn.
	/// </remarks>
	[TestMethod]
	public void TheArmIsDrawnBehindTheBodyAndTheHandleInFront()
	{
		var (stack, gadget) = AGadget();

		CollectionAssert.AreEqual(
			new[] { 0x21, 0x22, 0x1d, 0x23, 0x1e, 0x20, 0x25, 0x1f, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b },
			Find( gadget, 0x1d ).DrawOrder().Select( control => control.Id ).ToArray(), "the arm in" );

		stack.ClickAt( 261, 1414 );

		CollectionAssert.AreEqual(
			new[] { 0x21, 0x22, 0x62, 0x24, 0x1d, 0x63, 0x64, 0x23, 0x1e, 0x20, 0x25, 0x1f, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x2b },
			Find( gadget, 0x1d ).DrawOrder().Select( control => control.Id ).ToArray(), "the arm out" );
	}

	/// <summary>
	/// <b>The aerial stands as the builder leaves it with no tag</b>: the mast eight high on the body, the red top
	/// 58 below the stream's place; both take a press, and the top's right click with no tag changes nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the aerial at the stream's rectangles; the mast not stopping the pointer.</remarks>
	[TestMethod]
	public void TheAerialTakesAPress()
	{
		var (stack, gadget) = AGadget();
		gadget.Tags = new ParkTags();

		Assert.AreEqual( new UI.UiRect( 94, 976, 144, 984 ), Find( gadget, 0x2d ).Rect );
		Assert.AreEqual( new UI.UiRect( 77, 848, 161, 976 ), Find( gadget, 0x2e ).Rect );
		Assert.AreEqual( 0x2e, Under( gadget, 119, 900 ), "the red top" );
		Assert.AreEqual( 0x2d, Under( gadget, 119, 980 ), "the mast" );
		Assert.IsNull( Under( gadget, 119, 820 ), "nothing where the top would be with a tag under it" );
		Assert.AreEqual( 476, Find( gadget, 0x2e ).HelpText );

		Frame( stack, true, new Vector2( 119, 900 ), 10.00f );
		Frame( stack, false, new Vector2( 119, 900 ), 10.08f );
		Assert.AreEqual( new UI.UiRect( 77, 848, 161, 976 ), Find( gadget, 0x2e ).Rect );
	}

	/// <summary>
	/// <b>A tag stands on the mast and lifts the red top by its height and ten</b> (<c>LAB_004a11e0</c>, 0x100001):
	/// the oldest lowest, each 70 by 65 across the mast's middle, eight under where the top stood, showing its row's
	/// part of msgtag.md2; a hidden one keeps its place; one taken off puts the top and the later tags down.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the top not lifted; lifted by the tag's height alone; the newest tag lowest; the tag at the
	/// mast's top edge; the picture not set; a hidden tag still shown; a hidden tag's place given up at once; the old controls left on the mast.
	/// </remarks>
	[TestMethod]
	public void ATagStandsOnTheMastAndLiftsTheTop()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags();

		Frame( stack, false, new Vector2( 1000, 100 ), 1.0f );
		var first = tags.Said( 68, 13, 1000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );

		Assert.AreEqual( new UI.UiRect( 94, 901, 144, 984 ), Find( gadget, 0x2d ).Rect, "the mast up 75" );
		Assert.AreEqual( new UI.UiRect( 77, 773, 161, 901 ), Find( gadget, 0x2e ).Rect, "and the top on it" );
		Assert.AreEqual( new UI.UiRect( 84, 909, 154, 974 ), Find( gadget, 900 ).Rect );
		Assert.AreEqual( (1, 475, true), (Find( gadget, 900 ).Frame, Find( gadget, 900 ).HelpText, Find( gadget, 900 ).Visible) );
		Assert.AreEqual( 900, Under( gadget, 119, 940 ), "the tag takes the pointer" );
		Assert.AreEqual( 0x2e, Under( gadget, 119, 820 ), "the top where it now stands" );

		tags.Said( 150, 0, 1200 );
		Frame( stack, false, new Vector2( 1000, 100 ), 1.3f );
		Assert.AreEqual( new UI.UiRect( 94, 826, 144, 984 ), Find( gadget, 0x2d ).Rect );
		Assert.AreEqual( new UI.UiRect( 84, 909, 154, 974 ), Find( gadget, 900 ).Rect, "the older stays lowest" );
		Assert.AreEqual( new UI.UiRect( 84, 834, 154, 899 ), Find( gadget, 901 ).Rect );

		tags.Hide( first, 1400 );
		Frame( stack, false, new Vector2( 1000, 100 ), 1.5f );
		Assert.IsFalse( Find( gadget, 900 ).Visible, "hidden" );
		Assert.AreEqual( new UI.UiRect( 94, 826, 144, 984 ), Find( gadget, 0x2d ).Rect, "and its place kept for half a second" );
		Assert.AreNotEqual( 900, Under( gadget, 119, 940 ), "a hidden tag takes no pointer" );

		Frame( stack, false, new Vector2( 1000, 100 ), 1.9f );
		Assert.AreEqual( new UI.UiRect( 94, 901, 144, 984 ), Find( gadget, 0x2d ).Rect, "off: the top comes down" );
		Assert.AreEqual( new UI.UiRect( 84, 909, 154, 974 ), Find( gadget, 900 ).Rect, "and the later tag with it" );
		Assert.AreEqual( 1, tags.Standing.Count );
		Assert.AreEqual( 2, Find( gadget, 0x2d ).Children.Count, "the top and the one tag: no control of a tag taken off is left on the mast" );
	}

	/// <summary>
	/// <b>A tag's right click hides it, its left click opens the message bar and takes nothing off, and the top's
	/// right click takes every tag off, the oldest first, a tenth of a second apart</b> (<c>FUN_0048dae0</c>,
	/// <c>FUN_004a1190</c>).
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a tag's right click doing nothing; taking it off at once; the left click opening nothing; the
	/// top's right click doing nothing; taking every tag at once; the newest first.
	/// </remarks>
	[TestMethod]
	public void ATagAndTheTopAnswerTheirClicks()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags();

		tags.Said( 68, 13, 1000 );
		tags.Said( 68, 14, 1000 );
		tags.Said( 68, 15, 1000 );
		Frame( stack, false, new Vector2( 1000, 100 ), 2.0f );
		Assert.AreEqual( 902, Under( gadget, 119, 790 ) );

		Input.Mouse = new() { Left = true, LeftWentDown = true, Position = new Vector2( 119, 790 ) };
		stack.Update();
		Input.Mouse = new() { Position = new Vector2( 119, 790 ) };
		stack.Update();
		Assert.AreEqual( (true, tags.Standing[2]), (gadget.BarOut, tags.OnShow), "the left click puts its message on the bar" );
		Assert.AreEqual( 3, tags.Standing.Count( tag => !tag.Hidden ), "and takes nothing off" );

		Frame( stack, true, new Vector2( 119, 790 ), 3.00f );
		Frame( stack, false, new Vector2( 119, 790 ), 3.08f );
		Assert.AreEqual( (3, true), (tags.Standing.Count, tags.Standing[2].Hidden), "the right click hides it where it stands" );
		Frame( stack, false, new Vector2( 1000, 100 ), 3.57f );
		Assert.AreEqual( 3, tags.Standing.Count );
		Frame( stack, false, new Vector2( 1000, 100 ), 3.58f );
		Assert.AreEqual( 2, tags.Standing.Count, "and half a second on it is off" );

		// The top stands on two tags: 976 - 150 = 826, so its middle is at 762.
		Assert.AreEqual( 0x2e, Under( gadget, 119, 762 ) );
		Frame( stack, true, new Vector2( 119, 762 ), 5.00f );
		Frame( stack, false, new Vector2( 119, 762 ), 5.08f );
		Assert.AreEqual( 2, tags.Standing.Count, "not at the click" );
		Frame( stack, false, new Vector2( 1000, 100 ), 5.18f );
		Assert.AreEqual( (1, 14), (tags.Standing.Count, tags.Standing[0].Subject), "the oldest a tenth of a second on" );
		Frame( stack, false, new Vector2( 1000, 100 ), 5.28f );
		Assert.AreEqual( 0, tags.Standing.Count, "and the next a tenth after" );
		Assert.AreEqual( new UI.UiRect( 77, 848, 161, 976 ), Find( gadget, 0x2e ).Rect, "the top back on the body" );
	}

	/// <summary>
	/// <b>A tag's left click puts its message on the bar and the bar on the arm</b> (<c>FUN_0048d9c0</c>, the arm's
	/// <c>0x004a14f0</c>): the arm 820 out, the bar at (486,1112)-(1101,1453) with the tag's text, its first button
	/// dressed by the row's type and its second hidden for nought, the erase button shown; the tag swells and
	/// shrinks while on show and stands past its life; the same tag clicked again folds the arm away.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the bar not shown; the arm's travel the camcorder's 645; the handle not moved; the text not
	/// set; the first button undressed; the second shown for nought; the erase button hidden; the tag's timers left
	/// running; no swell; the swell in the census; a second click opening again; the slide uncounted; the advisor
	/// not posted his line, or it left in the slots as the arm folds; the camcorder's panel shown with the bar.
	/// </remarks>
	[TestMethod]
	public void ATagsLeftClickPutsItsMessageOnTheArm()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags();
		var slides = Times( "GADGET_ARM_SLIDE" );
		var lines = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		gadget.Park = new ParkState( null ) { AdvisorMessages = lines };

		Frame( stack, false, new Vector2( 1000, 100 ), 1.0f );
		var tag = tags.Said( 72, 13, 1000 )!;
		tag.Text = "A ride has broken down.";
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );
		Assert.IsFalse( Find( gadget, 0x10d2d ).Visible, "no bar until a tag is opened" );
		Assert.IsNull( Under( gadget, 565, 1172 ) );

		Time.PinWall( 3000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );

		Assert.AreEqual( (true, true, tag), (gadget.BarOut, gadget.ArmOut, tags.OnShow) );
		Assert.AreEqual( slides + 1, Times( "GADGET_ARM_SLIDE" ) );
		StringAssert.Contains( lines.Census(), "pending=1 attempts=0 threshold=25 slot=284/subject=0 ", "the advisor is posted message 0x11c" );
		Assert.AreEqual( new UI.UiRect( 331, 1069, 1181, 1495 ), Find( gadget, 0x21 ).Rect, "the arm 820 out" );
		Assert.AreEqual( new UI.UiRect( 1151, 1058, 1304, 1503 ), Find( gadget, 0x23 ).Rect, "and the handle with it" );
		Assert.AreEqual( (new UI.UiRect( 486, 1112, 1101, 1453 ), true), (Find( gadget, 0x10d2d ).Rect, Find( gadget, 0x10d2d ).Visible) );
		Assert.AreEqual( ("A ride has broken down.", 6, new UI.UiRect( 637, 1129, 1084, 1433 )),
			(Find( gadget, 0x10d32 ).Text, Find( gadget, 0x10d32 ).Font, Find( gadget, 0x10d32 ).TextRect) );
		Assert.AreEqual( (true, 516), (Find( gadget, 0x10d30 ).Visible, Find( gadget, 0x10d30 ).HelpText), "mb_locate, \"Left-click to go there\"" );
		Assert.IsFalse( Find( gadget, 0x10d2f ).Visible, "a second button of type nought is not shown" );
		Assert.IsTrue( Find( gadget, 0x10d2e ).Visible, "the erase button" );
		Assert.IsFalse( Find( gadget, 0x62 ).Visible, "the camcorder's panel is not on the arm" );
		Assert.IsTrue( Find( gadget, 0x24 ).Visible, "the retract button" );
		Assert.AreEqual( 0x10d30, Under( gadget, 565, 1172 ) );
		Assert.AreEqual( 0x10d2e, Under( gadget, 565, 1387 ) );
		Assert.AreNotEqual( 0x10d2f, Under( gadget, 565, 1280 ), "the hidden button takes no pointer" );
		StringAssert.Contains( gadget.BarCensus(), "bar: out=1 rect=486,1112,1101,1453 arm=331,1069,1181,1495 handle=1151,1058,1304,1503" );
		StringAssert.Contains( gadget.BarCensus(), "second=hidden" );
		StringAssert.Contains( gadget.BarCensus(), "text=\"A ride has broken down.\"" );

		// The stream at 0x007502b8: 0, 2, 4, 6, 4, 2, 0 a side, a step every 100 ms, from the opening.
		foreach ( var (at, swell) in new[] { (3.05f, 0), (3.15f, 2), (3.25f, 4), (3.35f, 6), (3.45f, 4), (3.55f, 2), (3.65f, 0), (3.75f, 0), (3.85f, 2), (4.05f, 6) } )
		{
			Frame( stack, false, new Vector2( 1000, 100 ), at );
			Assert.AreEqual( new UI.UiRect( 84 - swell, 909 - swell, 154 + swell, 974 + swell ), Find( gadget, 900 ).Rect, $"the tag on show at {at}" );
		}

		StringAssert.Contains( gadget.AerialCensus(), "tag900=84,909,154,974/", "the census reads the tag's own rectangle" );

		// A second tag, not on show, keeps its own rectangle beside the swelling one.
		tags.Said( 150, 0, 4000 );
		Frame( stack, false, new Vector2( 1000, 100 ), 4.05f );
		Assert.AreEqual( new UI.UiRect( 78, 903, 160, 980 ), Find( gadget, 900 ).Rect );
		Assert.AreEqual( new UI.UiRect( 84, 834, 154, 899 ), Find( gadget, 901 ).Rect, "only the tag on show swells" );
		Frame( stack, false, new Vector2( 1000, 100 ), 34.1f );
		Frame( stack, false, new Vector2( 1000, 100 ), 34.7f );

		Frame( stack, false, new Vector2( 1000, 100 ), 45.0f );
		Assert.AreEqual( (1, false, true), (tags.Standing.Count, tag.Hidden, gadget.BarOut), "an opened tag stands past its thirty seconds" );

		Time.PinWall( 46000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( (false, false, null, 1), (gadget.BarOut, gadget.ArmOut, tags.OnShow, tags.Standing.Count), "the tag on show clicked again folds the arm away" );
		Assert.AreEqual( slides + 2, Times( "GADGET_ARM_SLIDE" ) );
		StringAssert.Contains( lines.Census(), "pending=0 attempts=0 ", "and it is withdrawn as the bar leaves" );
		Assert.AreEqual( new UI.UiRect( 331, 1069, 361, 1495 ), Find( gadget, 0x21 ).Rect );
		Assert.AreEqual( new UI.UiRect( 331, 1058, 484, 1503 ), Find( gadget, 0x23 ).Rect );
		Assert.IsFalse( Find( gadget, 0x10d2d ).Visible );
		StringAssert.Contains( gadget.BarCensus(), "bar: out=0 " );
		Frame( stack, false, new Vector2( 1000, 100 ), 46.35f );
		Assert.AreEqual( new UI.UiRect( 84, 909, 154, 974 ), Find( gadget, 900 ).Rect, "and the tag is still" );

		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( (true, tag), (gadget.BarOut, tags.OnShow), "and a third click opens it again" );
		StringAssert.Contains( lines.Census(), "pending=1 attempts=0 threshold=25 slot=284/subject=0 " );
	}

	/// <summary>
	/// <b>The bar leaving the arm cuts the advisor off its line</b> (<c>FUN_0048e2f0</c>'s 0x1001d8,
	/// <c>FUN_00486b40</c>): folded away while he says message 0x11c, the message is cut off, on the game's clock,
	/// and may be posted again; a second tag opened over the first withdraws before it posts
	/// (<c>FUN_0048d9c0</c>: <c>FUN_004a2590</c>, then <c>FUN_00486b00</c>); the erase button withdraws too; after
	/// three cuts an opening posts nothing.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> no withdrawal at the fold, at the erase or at a second tag's opening; the post before the
	/// withdrawal; the wall clock for the game's.
	/// </remarks>
	[TestMethod]
	public void TheBarLeavingTheArmCutsTheAdvisorOffItsLine()
	{
		using var clock = new SimulationClockScope();
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags();
		var lines = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var park = gadget.Park = new ParkState( null ) { AdvisorMessages = lines };
		var said = 0;
		int Play( int response, int sample ) { ++said; return 9000; }

		for ( var i = 0; i < 7; ++i )
			park.AdvanceGameTick();

		Frame( stack, false, new Vector2( 1000, 100 ), 1.0f );
		tags.Said( 72, 13, 1000 );
		tags.Said( 150, 0, 1000 );
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );

		// The wall clock far past the line's end: the cut is judged on the game's clock, which stands at nought.
		Time.PinWall( 60000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		lines.Tick( park.GameTick, 0, Play );
		StringAssert.Contains( lines.Census(), $"message=284/score=1000/line=0/lastTick={park.GameTick}/said=1/slaps=0" );
		Assert.AreEqual( 7, park.GameTick );

		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.IsFalse( gadget.BarOut );
		StringAssert.Contains( lines.Census(), "/said=0/slaps=1", "folded away while he says it" );

		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		StringAssert.Contains( lines.Census(), "pending=1 attempts=1 threshold=25 slot=284/subject=0 ", "and posted again at the next opening" );
		lines.Tick( park.GameTick, 0, Play );

		// The second tag opened over the first: withdrawn, a cut, then posted.
		Assert.IsTrue( stack.ClickAt( 119, 866 ) );
		Assert.AreEqual( (true, 150), (gadget.BarOut, tags.OnShow!.Row.Id) );
		StringAssert.Contains( lines.Census(), "pending=1 attempts=2 threshold=25 slot=284/subject=0 " );
		StringAssert.Contains( lines.Census(), "/said=0/slaps=2" );
		lines.Tick( park.GameTick, 0, Play );
		Assert.AreEqual( 3, said );

		Assert.IsTrue( stack.ClickAt( 565, 1387 ), "the erase button" );
		Assert.IsFalse( gadget.BarOut );
		StringAssert.Contains( lines.Census(), "/said=0/slaps=3" );

		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.IsTrue( gadget.BarOut );
		StringAssert.Contains( lines.Census(), "pending=0 attempts=3 ", "cut off three times, it is posted no more" );
	}

	/// <summary>
	/// <b>The bar's ways off the arm</b> (<c>FUN_0048e2f0</c>, <c>FUN_0048dae0</c>): the erase button hides the tag
	/// on show at once, takes it off half a second on and folds the arm away; the tag's right click the same; the tag
	/// taken off by a later line; the retract button, which also lifts the camcorder button; and the camcorder
	/// button, which puts its own panel on the arm or folds it away, and stays down while a tag takes the arm.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> erase leaving the tag; taking it off at once; leaving the arm out; the right click leaving
	/// the arm out; a tag gone and its bar left; the camcorder's panel and the bar both shown; the camcorder button
	/// lifted by a tag's click, or left down by the retract button; a button of an unbuilt type uncounted, or
	/// folding the arm; a declining tag's erase button shown.
	/// </remarks>
	[TestMethod]
	public void TheBarLeavesTheArmWithItsTag()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags( [.. ParkTags.Rows, new ParkTags.Row( 5, ParkTags.NoTag, 1, 0, 0x13, ParkTags.DeclineButton, 30000, 0 ),
			new ParkTags.Row( 6, ParkTags.NoTag, 1, 0, 4, 0, 30000, 0 )] );
		var camcorder = (UI.UiButton)Find( gadget, 0x27 );

		// Erase.
		var tag = tags.Said( 72, 13, 1000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );
		Time.PinWall( 2000 );
		stack.ClickAt( 119, 941 );
		Assert.IsTrue( gadget.BarOut );
		Assert.IsTrue( stack.ClickAt( 565, 1387 ), "mb_erase" );
		Assert.AreEqual( (true, 1, false, null), (tag.Hidden, tags.Standing.Count, gadget.BarOut, tags.OnShow), "hidden at once, the arm in" );
		Frame( stack, false, new Vector2( 1000, 100 ), 2.49f );
		Assert.AreEqual( 1, tags.Standing.Count );
		Frame( stack, false, new Vector2( 1000, 100 ), 2.51f );
		Assert.AreEqual( 0, tags.Standing.Count, "and off half a second on" );

		// The tag's own right click.
		tag = tags.Said( 72, 13, 3000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 3.1f );
		stack.ClickAt( 119, 941 );
		Assert.IsTrue( gadget.BarOut );
		Frame( stack, true, new Vector2( 119, 941 ), 4.00f );
		Frame( stack, false, new Vector2( 119, 941 ), 4.08f );
		Assert.AreEqual( (true, false, null), (tag.Hidden, gadget.BarOut, tags.OnShow), "the right click hides it and folds the arm away" );
		Frame( stack, false, new Vector2( 1000, 100 ), 4.7f );
		Assert.AreEqual( 0, tags.Standing.Count );

		// Taken off by a later line.
		var worn = tags.Said( 68, 13, 5000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 5.1f );
		stack.ClickAt( 119, 941 );
		Assert.AreEqual( (true, worn), (gadget.BarOut, tags.OnShow) );
		tags.Said( 72, 13, 6000 );
		Frame( stack, false, new Vector2( 1000, 100 ), 6.1f );
		Assert.AreEqual( (false, false), (gadget.BarOut, gadget.ArmOut), "the tag on show gone, the bar goes with it" );

		// The camcorder button and the retract button.
		stack.ClickAt( 119, 941 );
		Assert.IsTrue( gadget.BarOut );
		Assert.IsTrue( stack.ClickAt( 261, 1414 ), "the camcorder button" );
		Assert.AreEqual( (false, true, true, null), (gadget.BarOut, gadget.ArmOut, camcorder.IsDown, tags.OnShow), "its panel takes the arm" );
		Assert.AreEqual( (true, false), (Find( gadget, 0x62 ).Visible, Find( gadget, 0x10d2d ).Visible) );
		Assert.AreEqual( new UI.UiRect( 976, 1058, 1129, 1503 ), Find( gadget, 0x23 ).Rect );

		stack.ClickAt( 119, 941 );
		Assert.AreEqual( (true, true), (gadget.BarOut, camcorder.IsDown), "a tag takes the arm and the camcorder button stays down" );
		Assert.AreEqual( (false, true), (Find( gadget, 0x62 ).Visible, Find( gadget, 0x10d2d ).Visible) );
		Assert.AreEqual( new UI.UiRect( 1151, 1058, 1304, 1503 ), Find( gadget, 0x23 ).Rect );
		Assert.IsTrue( stack.ClickAt( 261, 1414 ), "the camcorder button, up" );
		Assert.AreEqual( (false, false, false, null), (gadget.BarOut, gadget.ArmOut, camcorder.IsDown, tags.OnShow) );

		// The bar's own ways in leave the camcorder button as it is: erase, with the button down.
		stack.ClickAt( 261, 1414 );
		stack.ClickAt( 119, 941 );
		Assert.AreEqual( (true, true), (gadget.BarOut, camcorder.IsDown) );
		Assert.IsTrue( stack.ClickAt( 565, 1387 ), "mb_erase" );
		Assert.AreEqual( (false, false, true), (gadget.BarOut, gadget.ArmOut, camcorder.IsDown), "erase folds the arm and leaves the camcorder button down" );
		Assert.IsTrue( stack.ClickAt( 261, 1414 ), "up again, with nothing out" );
		Assert.AreEqual( (false, false), (gadget.ArmOut, camcorder.IsDown) );
		Frame( stack, false, new Vector2( 1000, 100 ), 6.7f );
		Assert.AreEqual( 0, tags.Standing.Count );
		tags.Said( 72, 13, 6800 );
		Frame( stack, false, new Vector2( 1000, 100 ), 6.9f );

		stack.ClickAt( 261, 1414 );
		stack.ClickAt( 119, 941 );
		Assert.AreEqual( (true, true), (gadget.BarOut, camcorder.IsDown) );
		Assert.IsTrue( stack.ClickAt( 389, 1426 ), "the retract button" );
		Assert.AreEqual( (false, false, false, null), (gadget.BarOut, gadget.ArmOut, camcorder.IsDown, tags.OnShow), "which lifts the camcorder button too" );
		Assert.AreEqual( 1, tags.Standing.Count, "and leaves the tag" );

		// A tag whose buttons are not built: an offer of research, and to decline.
		var buttons = Times( "TAG_BAR_BUTTON" );
		var offer = tags.Said( 5, 0, 7000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 7.1f );
		Assert.IsTrue( stack.ClickAt( 119, 866 ), "the second tag up" );
		Assert.AreEqual( offer, tags.OnShow );
		Assert.AreEqual( (true, 0x213, true, -1, false),
			(Find( gadget, 0x10d30 ).Visible, Find( gadget, 0x10d30 ).HelpText, Find( gadget, 0x10d2f ).Visible, Find( gadget, 0x10d2f ).HelpText, Find( gadget, 0x10d2e ).Visible),
			"mb_resrch, mb_decline, and no erase button beside a decline" );
		Assert.IsTrue( stack.ClickAt( 565, 1172 ) );
		Assert.IsTrue( stack.ClickAt( 565, 1280 ) );
		Assert.AreEqual( (buttons + 2, true), (Times( "TAG_BAR_BUTTON" ), gadget.BarOut), "each counted, and the bar left out" );
		Assert.AreNotEqual( 0x10d2e, Under( gadget, 565, 1387 ), "the hidden erase button takes no pointer" );

		// mb_locate of the other type, which goes by the item (FUN_004868d0, not read): counted, and the camera left.
		var looked = ParkOrbitCameraMode.PointOfInterest;
		var byItem = tags.Said( 6, 13, 7200 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 7.3f );
		Assert.IsTrue( stack.ClickAt( 119, 791 ), "the third tag up" );
		Assert.AreEqual( (byItem, 515), (tags.OnShow, Find( gadget, 0x10d30 ).HelpText) );
		Assert.IsTrue( stack.ClickAt( 565, 1172 ) );
		Assert.AreEqual( (buttons + 3, looked), (Times( "TAG_BAR_BUTTON" ), ParkOrbitCameraMode.PointOfInterest) );
	}

	/// <summary>
	/// <b>The bar's buy button</b> (<c>FUN_0048e0f0</c>, types 9 to 0xc, which answer 2): the no-rides tag shows
	/// part 2 of the tag's picture and dresses the bar's first button as <c>mb_buy</c>; its click opens the buy
	/// screen on the rides tab whatever tab it was last left on, hides the tag at once and folds the arm away, and
	/// is not counted as unbuilt. With the buy screen open on another tab the click picks the type's tab on that
	/// screen and opens no second one.
	/// </summary>
	/// <remarks><b>Mutations:</b> the button counted as unbuilt; the tag left; the arm left out; the screen not opened, opened on the tab last left, or opened twice; the open screen's tab left, or a new screen put in its place; the tab the type itself; the item's row counted for no item, or never.</remarks>
	[TestMethod]
	public void TheBuyButtonOpensTheBuyScreenOnItsTabAndTakesItsTagOff()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags( [.. ParkTags.Rows, new ParkTags.Row( 8, ParkTags.NoTag, 1, 0, 0xb, 0, 30000, 0 )] );
		var buttons = Times( "TAG_BAR_BUTTON" );
		var rows = Times( "TAG_BAR_BUY_ITEM_ROW" );

		// The screen last left on its shops tab.
		var earlier = new UI.ParkBuyScreen( stack, 1 );
		stack.Open( earlier );
		Assert.AreEqual( 1, earlier.Tab );
		stack.Close( earlier );

		var tag = tags.Said( 64, 0, 1000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );
		Assert.AreEqual( (2, true), (Find( gadget, 900 ).Frame, Find( gadget, 900 ).Visible) );
		Time.PinWall( 2000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( (true, tag), (gadget.BarOut, tags.OnShow) );
		Assert.AreEqual( (true, 0x209, false, true),
			(Find( gadget, 0x10d30 ).Visible, Find( gadget, 0x10d30 ).HelpText, Find( gadget, 0x10d2f ).Visible, Find( gadget, 0x10d2e ).Visible),
			"mb_buy, no second button, and the erase button" );

		Assert.IsTrue( stack.ClickAt( 565, 1172 ), "mb_buy" );
		Assert.AreEqual( (true, false, false, null, buttons),
			(tag.Hidden, gadget.BarOut, gadget.ArmOut, tags.OnShow, Times( "TAG_BAR_BUTTON" )), "hidden at once, the arm in, nothing counted" );
		var screens = stack.Windows.OfType<UI.ParkBuyScreen>().ToArray();
		Assert.AreEqual( (1, 0, rows), (screens.Length, screens[0].Tab, Times( "TAG_BAR_BUY_ITEM_ROW" )), "the buy screen, on its rides tab; a tag of no item asks for no row" );
		var opened = screens[0];

		// The screen open: a tag of type 0xb picks the sideshows tab on it.
		Frame( stack, false, new Vector2( 1000, 100 ), 2.6f );
		Assert.AreEqual( 0, tags.Standing.Count );
		var second = tags.Said( 8, 0, 3000, item: 1303 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 3.1f );
		Time.PinWall( 3500 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( second, tags.OnShow );
		Assert.IsTrue( stack.ClickAt( 565, 1172 ) );
		screens = stack.Windows.OfType<UI.ParkBuyScreen>().ToArray();
		Assert.AreEqual( (1, 2, true, buttons), (screens.Length, screens[0].Tab, second.Hidden, Times( "TAG_BAR_BUTTON" )) );
		Assert.AreSame( opened, screens[0], "the screen that was open, not a new one in its place" );
		Assert.AreEqual( rows + 1, Times( "TAG_BAR_BUY_ITEM_ROW" ), "the item's row is not selected: counted" );
	}

	/// <summary>
	/// <b>The bar's hire button</b> (<c>FUN_0048e0f0</c>, types 0xd to 0x11, which answer 2): the worn-rides tag
	/// shows part 2 of the tag's picture and dresses the bar's first button as <c>mb_buy</c> with the help row of
	/// its type; its click opens the hire screen on the mechanics' tab whatever tab it was last left on, hides the
	/// tag at once and folds the arm away, and is not counted as unbuilt. With the hire screen open on that tab or
	/// another the click builds the screen again on the type's tab (<c>FUN_0049bdd0</c> destroys the open park
	/// screen first), and each of the five types names its own tab.
	/// </summary>
	/// <remarks><b>Mutations:</b> the button counted as unbuilt; the tag left; the arm left out; the screen not opened, or opened on the tab last left; the tab the type itself, or one off; the open screen kept; a type past 0x11 built; row 7's button another's.</remarks>
	[TestMethod]
	public void TheHireButtonOpensTheHireScreenOnItsTabAndTakesItsTagOff()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags( [.. ParkTags.Rows, .. Enumerable.Range( 0xd, 6 ).Select( type => new ParkTags.Row( 200 + type, ParkTags.NoTag, 1, 0, type, 0, 30000, 0 ) )] );
		var buttons = Times( "TAG_BAR_BUTTON" );

		// The screen last left on its guards' tab.
		var earlier = new UI.ParkHireScreen( stack, openOn: 3 );
		stack.Open( earlier );
		Assert.AreEqual( 3, earlier.Tab );
		stack.Close( earlier );

		var tag = tags.Said( 7, 0, 1000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );
		Assert.AreEqual( (2, true), (Find( gadget, 900 ).Frame, Find( gadget, 900 ).Visible) );
		Time.PinWall( 2000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( (true, tag), (gadget.BarOut, tags.OnShow) );
		Assert.AreEqual( ("mb_buy", 0x20e), UI.ParkGadget.BarButtonLook( 0xe ) );
		Assert.AreEqual( (true, 0x20e, false, true),
			(Find( gadget, 0x10d30 ).Visible, Find( gadget, 0x10d30 ).HelpText, Find( gadget, 0x10d2f ).Visible, Find( gadget, 0x10d2e ).Visible),
			"mb_buy with type 0xe's help row, no second button, and the erase button" );

		Assert.IsTrue( stack.ClickAt( 565, 1172 ), "mb_buy" );
		Assert.AreEqual( (true, false, false, null, buttons),
			(tag.Hidden, gadget.BarOut, gadget.ArmOut, tags.OnShow, Times( "TAG_BAR_BUTTON" )), "hidden at once, the arm in, nothing counted" );
		var screens = stack.Windows.OfType<UI.ParkHireScreen>().ToArray();
		Assert.AreEqual( (1, 1), (screens.Length, screens[0].Tab), "the hire screen, on its mechanics' tab, not the guards'" );
		var opened = screens[0];

		// Each type's own tab, on a screen built again; the type after the last is not a hire button.
		var now = 2.6f;
		for ( var type = 0xd; type <= 0x12; ++type )
		{
			Frame( stack, false, new Vector2( 1000, 100 ), now );
			Assert.AreEqual( 0, tags.Standing.Count, $"type 0x{type:x}: the mast bare" );
			var next = tags.Said( 200 + type, 0, (long)(now * 1000) + 100 )!;
			Frame( stack, false, new Vector2( 1000, 100 ), now + 0.2f );
			Time.PinWall( (long)(now * 1000) + 300 );
			Assert.IsTrue( stack.ClickAt( 119, 941 ) );
			Assert.AreEqual( next, tags.OnShow );

			if ( type == 0x12 )
			{
				Assert.IsFalse( Find( gadget, 0x10d30 ).Visible, "type 0x12 is not dressed" );
				break;
			}

			Assert.IsTrue( stack.ClickAt( 565, 1172 ) );
			screens = stack.Windows.OfType<UI.ParkHireScreen>().ToArray();
			Assert.AreEqual( (1, type - 0xd, true, buttons), (screens.Length, screens[0].Tab, next.Hidden, Times( "TAG_BAR_BUTTON" )), $"type 0x{type:x}" );
			Assert.AreNotSame( opened, screens[0], "built again, as the opener destroys the open screen first" );
			opened = screens[0];
			now += 1f;
		}

		// The tab last left on is the session's: put back for the tests after this one.
		stack.Open( new UI.ParkHireScreen( stack, openOn: 0 ) );
	}

	/// <summary>
	/// <b>The bar's entry-price button</b> (<c>FUN_0048e0f0</c>, type 0x18, which answers 2): the shut park's tag
	/// shows part 2 of the tag's picture and dresses the bar's first button as <c>b_entry</c>; its click hides the
	/// tag at once, takes it off the mast half a second on and folds the arm away, and is not counted as unbuilt.
	/// </summary>
	/// <remarks><b>Mutations:</b> the button counted as unbuilt; the tag left; the tag off at once; the arm left out; the button not dressed; the screen not opened.</remarks>
	[TestMethod]
	public void TheEntryPriceButtonTakesItsTagOffAndFoldsTheArm()
	{
		var (stack, gadget) = AGadget();
		var tags = gadget.Tags = new ParkTags();
		var buttons = Times( "TAG_BAR_BUTTON" );

		var tag = tags.Said( 3, 0, 1000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 1.1f );
		Assert.AreEqual( (2, true), (Find( gadget, 900 ).Frame, Find( gadget, 900 ).Visible) );
		Time.PinWall( 2000 );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( (true, tag), (gadget.BarOut, tags.OnShow) );
		Assert.AreEqual( (true, 0xec, false, true),
			(Find( gadget, 0x10d30 ).Visible, Find( gadget, 0x10d30 ).HelpText, Find( gadget, 0x10d2f ).Visible, Find( gadget, 0x10d2e ).Visible),
			"b_entry, no second button, and the erase button" );

		Assert.IsTrue( stack.ClickAt( 565, 1172 ), "b_entry" );
		Assert.AreEqual( (true, 1, false, false, null, buttons),
			(tag.Hidden, tags.Standing.Count, gadget.BarOut, gadget.ArmOut, tags.OnShow, Times( "TAG_BAR_BUTTON" )), "hidden at once, the arm in, nothing counted" );
		Assert.AreEqual( 1, stack.Windows.Count( window => window is UI.ParkEntryPriceScreen ), "the entry-price screen opened" );
		Frame( stack, false, new Vector2( 1000, 100 ), 2.49f );
		Assert.AreEqual( 1, tags.Standing.Count );
		Frame( stack, false, new Vector2( 1000, 100 ), 2.51f );
		Assert.AreEqual( 0, tags.Standing.Count, "and off half a second on" );

		// With the screen open already the button builds no second one, and takes its tag off all the same.
		var screen = stack.Windows.Single( window => window is UI.ParkEntryPriceScreen );
		var again = tags.Said( 3, 0, 3000 )!;
		Frame( stack, false, new Vector2( 1000, 100 ), 3.1f );
		Assert.IsTrue( stack.ClickAt( 119, 941 ) );
		Assert.AreEqual( again, tags.OnShow );
		Assert.IsTrue( stack.ClickAt( 565, 1172 ) );
		Assert.AreSame( screen, stack.Windows.Single( window => window is UI.ParkEntryPriceScreen ), "the screen that was open" );
		Assert.AreEqual( (true, false), (again.Hidden, gadget.BarOut) );
	}

	private static (UI.WindowStack Stack, UI.ParkGadget Gadget) AGadget()
	{
		var stack = new UI.WindowStack();
		var gadget = new UI.ParkGadget( stack );

		stack.Open( gadget );

		return (stack, gadget);
	}

	/// <summary>The id of the control the pointer stops at, or null.</summary>
	private static int? Under( UI.ParkGadget gadget, float x, float y ) => gadget.Root.HitTest( x, y )?.Id;

	private static UI.UiControl Find( UI.ParkGadget gadget, int id )
	{
		return In( gadget.Root ) ?? throw new AssertFailedException( $"no control 0x{id:x}" );

		UI.UiControl? In( UI.UiControl control )
			=> control.Id == id ? control : control.Children.Select( In ).FirstOrDefault( found => found != null );
	}

	/// <summary>One frame of the game: the right button and the pointer at a time, then the stack's update.</summary>
	private static void Frame( UI.WindowStack stack, bool right, Vector2 at, float now )
	{
		Time.PinWall( (long)System.MathF.Round( now * 1000f ) );
		Input.Mouse = new() { Right = right, RightWentDown = right && !Input.Mouse.Right, Position = at };
		stack.Update();
	}

	private static int Times( string what ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == what ).Times;
}

/// <summary>
/// What <see cref="UI.UiControl"/> was given for the gadget: a move that takes everything with it, and a draw in order
/// of depth. These need no game.
/// </summary>
[TestClass]
public class UiControlMoveAndDepthTests
{
	/// <summary>
	/// <b>A moved control takes its outline, its text rectangle and its children with it</b> (<c>FUN_0065c7fe</c>).
	///
	/// </summary>
	/// <remarks><b>Mutations:</b> the children left; the outline left; the text rectangle left; the outline shared with the array it was given.</remarks>
	[TestMethod]
	public void AMovedControlTakesEverythingWithIt()
	{
		var given = new[] { new UI.UiPoint( 10, 10 ), new UI.UiPoint( 30, 10 ), new UI.UiPoint( 20, 40 ) };

		var control = new UI.UiControl
		{
			Rect = new UI.UiRect( 10, 10, 30, 40 ),
			TextRect = new UI.UiRect( 12, 12, 28, 20 ),
			Outline = given
		};

		var child = control.Add( new UI.UiControl { Rect = new UI.UiRect( 15, 15, 25, 25 ) } );

		control.Move( -5, 100 );

		Assert.AreEqual( new UI.UiRect( 5, 110, 25, 140 ), control.Rect );
		Assert.AreEqual( new UI.UiRect( 7, 112, 23, 120 ), control.TextRect );
		Assert.AreEqual( new UI.UiPoint( 15, 140 ), control.Outline![2] );
		Assert.AreEqual( new UI.UiRect( 10, 115, 20, 125 ), child.Rect );
		Assert.AreEqual( new UI.UiPoint( 20, 40 ), given[2], "the array it was given is not written to" );
	}

	/// <summary>
	/// <b>A control that draws by depth draws the lowest first, itself among them</b>, so a child can lie behind its
	/// parent; a hidden one and all under it are left out; and without the switch a parent comes before its children.
	///
	/// </summary>
	/// <remarks><b>Mutations:</b> the draw not reading the switch; the switch read as always on; a hidden control drawn.</remarks>
	[TestMethod]
	public void ADepthDrawnControlDrawsTheLowestFirst()
	{
		foreach ( var (byDepth, expected) in new[] { (true, "2 1 3 4"), (false, "1 2 3 4") } )
		{
			var drawn = new System.Collections.Generic.List<int>();
			var parent = new Recording( drawn ) { Id = 1, Depth = 8, DrawsByDepth = byDepth };
			var behind = parent.Add( new Recording( drawn ) { Id = 2, Depth = 4 } );

			behind.Add( new Recording( drawn ) { Id = 3, Depth = 9 } );
			parent.Add( new Recording( drawn ) { Id = 4 } );
			parent.Add( new Recording( drawn ) { Id = 5, Depth = 0, Visible = false } ).Add( new Recording( drawn ) { Id = 6 } );

			parent.Draw();

			Assert.AreEqual( expected, string.Join( " ", drawn ), byDepth ? "by depth" : "by the tree" );
		}
	}

	/// <summary>A control that notes its id when it is drawn, and draws nothing.</summary>
	private sealed class Recording( System.Collections.Generic.List<int> drawn ) : UI.UiControl
	{
		protected override void OnDraw() => drawn.Add( Id );
	}
}
