using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVeldrid;
using System.Collections.Generic;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// A press let go inside one frame: the window system sends its press and its release, and the button stands up as
/// the frame ends. The original is sent each as a message (WM_LBUTTONDOWN posts <c>0x10005</c>), so none is lost.
/// </summary>
[TestClass]
public class PressInsideAFrameTests
{
	[TestCleanup]
	public void PutItBack() => Input.Mouse = new();

	/// <summary>
	/// <b>The frame's presses are counted from its events, and the button's state is where it ends</b>: a press let go
	/// inside the frame leaves it up with one press, a double click inside one leaves two.
	/// </summary>
	/// <remarks><b>Mutations:</b> the count read as whether there was one; the release's event counted as a press.</remarks>
	[TestMethod]
	public void AFramesPressesAreItsEvents()
	{
		Input.UpdateFrom( new Snapshot( false, Press, Release ) );
		Assert.IsFalse( Input.Mouse.Left, "let go" );
		Assert.IsTrue( Input.Mouse.LeftWentDown );
		Assert.AreEqual( 1, Input.Mouse.LeftPresses );

		Input.UpdateFrom( new Snapshot( false, Press, Release, Press, Release ) );
		Assert.AreEqual( 2, Input.Mouse.LeftPresses, "a double click inside one frame" );

		Input.UpdateFrom( new Snapshot( true ) );
		Assert.IsFalse( Input.Mouse.LeftWentDown, "held, and sent nothing" );
		Assert.AreEqual( 0, Input.Mouse.LeftPresses );
	}

	/// <summary>
	/// <b>The interface is given each step the button took</b> (<see cref="UI.WindowStack.LeftSteps"/>): a press let go
	/// inside the frame is a press and a release, a release and a press inside one held frame are both, and a frame
	/// with no press is the button as it ends.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the button's end state alone loses the quick press and the release before a press; no release
	/// before a press made while it was down gives a press on a held button.
	/// </remarks>
	[TestMethod]
	public void TheInterfaceIsGivenEachStep()
	{
		Assert.AreEqual( "down up", Steps( false, new() { LeftWentDown = true, LeftPresses = 1 } ) );
		Assert.AreEqual( "up down", Steps( true, new() { Left = true, LeftWentDown = true, LeftPresses = 1 } ) );
		Assert.AreEqual( "up down up", Steps( true, new() { LeftWentDown = true, LeftPresses = 1 } ) );
		Assert.AreEqual( "down up down up", Steps( false, new() { LeftWentDown = true, LeftPresses = 2 } ) );
		Assert.AreEqual( "down", Steps( false, new() { Left = true, LeftWentDown = true, LeftPresses = 1 } ) );

		Assert.AreEqual( "down", Steps( false, new() { Left = true } ), "held when the stack first looked" );
		Assert.AreEqual( "down", Steps( true, new() { Left = true } ) );
		Assert.AreEqual( "up", Steps( true, new() ) );
		Assert.AreEqual( "up", Steps( false, new() ) );
	}

	private static string Steps( bool wasDown, Input.MouseInfo mouse )
		=> string.Join( " ", UI.WindowStack.LeftSteps( wasDown, mouse ).Select( down => down ? "down" : "up" ) );

	private static readonly MouseEvent Press = new( MouseButton.Left, true );
	private static readonly MouseEvent Release = new( MouseButton.Left, false );

	/// <summary>SDL's mouse events for one frame, and where the left button stands as it ends.</summary>
	private sealed class Snapshot( bool left, params MouseEvent[] events ) : InputSnapshot
	{
		public IReadOnlyList<KeyEvent> KeyEvents => [];
		public IReadOnlyList<MouseEvent> MouseEvents => events;
		public IReadOnlyList<char> KeyCharPresses => [];
		public System.Numerics.Vector2 MousePosition => System.Numerics.Vector2.Zero;
		public float WheelDelta => 0f;
		public bool IsMouseDown( MouseButton button ) => button == MouseButton.Left && left;
	}
}
