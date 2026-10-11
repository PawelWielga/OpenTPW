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
	/// <b>A tag's right click hides it, its left click is the message bar's, and the top's right click takes every
	/// tag off, the oldest first, a tenth of a second apart</b> (<c>FUN_0048dae0</c>, <c>FUN_004a1190</c>).
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a tag's right click doing nothing; taking it off at once; the left click not counted; the
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

		var bar = Times( "TAG_MESSAGE_BAR" );
		Input.Mouse = new() { Left = true, LeftWentDown = true, Position = new Vector2( 119, 790 ) };
		stack.Update();
		Input.Mouse = new() { Position = new Vector2( 119, 790 ) };
		stack.Update();
		Assert.AreEqual( bar + 1, Times( "TAG_MESSAGE_BAR" ), "the left click asks for the bar" );
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
