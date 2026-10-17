using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// The person under the pointer by the picture drawn there (<see cref="SpritePick"/>; <c>docs/exe/park-engine.md</c>,
/// "The thing under a sprite"). The figures are a line the original's own latch was read at (<c>q468/orig/</c>):
/// a picture 56 x 60 with its origin 21 across, drawn at a w of 65.45 with its circle's middle at (0.317, 0.611), r2
/// 0.0262, and the pointer at (0.287, 0.557) latched.
/// </summary>
[TestClass]
public class SpritePickTests
{
	/// <summary>A projection whose depth is the world's Z and whose box is X and -Y over it, so a place is written in the box.</summary>
	private static readonly Matrix4x4 Plain = new(
		1, 0, 0, 0,
		0, 1, 0, 0,
		0, 0, 0, 1,
		0, 0, 0, 0 );

	/// <summary>The eye's depth that the original's w of 65.45 is: w is sin 45 of it.</summary>
	private static readonly float Depth = 65.45f / MathF.Sin( MathF.PI / 4f );

	/// <summary>The picture's left edge, 21 pixels of span at 0.75 across, is 0.0376 of the box left of its origin, over
	/// the original's w.</summary>
	private static SpritePick.Drawn Mike( int thing = 26, float depth = 0f, bool mirrored = false, float middleX = 0.317f )
	{
		depth = depth == 0f ? Depth : depth;
		var originX = middleX + (21f * 20f / 128f * 0.75f / (depth * MathF.Sin( MathF.PI / 4f )));
		return new( thing, new OpenTPW.Vector3( originX * depth, -0.611f * depth, depth ), 56, 60, 21, 41, mirrored );
	}

	private static int Latch( float x, float y, params SpritePick.Drawn[] drawn )
		=> SpritePick.Latch( drawn, new OpenTPW.Vector2( x, y ), Plain, 0.75f );

	/// <summary>
	/// <b>The circle is about the picture's left edge at its origin's height, its radius squared 0.0002 of the span's,
	/// the across at 0.75, and not divided by the depth.</b> The radius is 0.1618 here: 0.160 out from the middle is
	/// inside, 0.164 is not, across and down.
	/// </summary>
	/// <remarks><b>Mutations:</b> the circle about the origin; the across at 1; the radius over the depth; the height
	/// from the top; the test not strict.</remarks>
	[TestMethod]
	public void TheCircleIsAboutThePicturesLeftEdgeAtItsOrigin()
	{
		Assert.AreEqual( 26, Latch( 0.287f, 0.557f, Mike() ), "the line the original latched at" );
		Assert.AreEqual( 26, Latch( 0.317f + 0.160f, 0.611f, Mike() ) );
		Assert.AreEqual( 0, Latch( 0.317f + 0.164f, 0.611f, Mike() ), "across, past the radius" );
		Assert.AreEqual( 26, Latch( 0.317f - 0.160f, 0.611f, Mike() ) );
		Assert.AreEqual( 0, Latch( 0.317f - 0.164f, 0.611f, Mike() ) );
		Assert.AreEqual( 26, Latch( 0.317f, 0.611f + 0.160f, Mike() ) );
		Assert.AreEqual( 0, Latch( 0.317f, 0.611f + 0.164f, Mike() ), "down, past the radius" );
		Assert.AreEqual( 0, Latch( 0.317f, 0.611f - 0.164f, Mike() ), "up, past the radius" );

		// Twice as deep, the circle is the same size in the box.
		var far = Mike( depth: 2f * Depth );
		Assert.AreEqual( 26, Latch( 0.317f + 0.160f, 0.611f, far ) );
		Assert.AreEqual( 0, Latch( 0.317f + 0.164f, 0.611f, far ) );
	}

	/// <summary><b>Reflected, the circle is about the right edge</b>: the origin's 21 pixels the other way.</summary>
	/// <remarks><b>Mutations:</b> the reflection not taken.</remarks>
	[TestMethod]
	public void AReflectedPicturesCircleIsAboutItsRightEdge()
	{
		var mirrored = Mike( mirrored: true );
		var middle = 0.317f + (2f * 21f * 20f / 128f * 0.75f / 65.45f);

		Assert.AreEqual( 26, Latch( middle + 0.160f, 0.611f, mirrored ) );
		Assert.AreEqual( 0, Latch( 0.317f - 0.100f, 0.611f, mirrored ), "inside the unreflected circle, outside this one" );
	}

	/// <summary><b>Of two circles the pointer is in, the nearer the eye wins, whichever was drawn first; at one depth, the
	/// first drawn</b> (<c>0x0058930b</c>: latched only when nearer than the latch). Behind the eye nothing is latched.</summary>
	/// <remarks><b>Mutations:</b> the last drawn wins; the further wins; the test not strict; behind the eye taken.</remarks>
	[TestMethod]
	public void TheNearerPictureWins()
	{
		var near = Mike( 7, depth: 80f );
		var far = Mike( 8, depth: 90f );

		Assert.AreEqual( 7, Latch( 0.317f, 0.611f, near, far ) );
		Assert.AreEqual( 7, Latch( 0.317f, 0.611f, far, near ) );
		Assert.AreEqual( 8, Latch( 0.317f, 0.611f, Mike( 8 ), Mike( 9 ) ), "one depth: the first drawn" );

		// Its circle would hold the pointer, the divide by a w below nought putting it at the box's middle.
		var behind = Mike( 5 ) with { Origin = new OpenTPW.Vector3( 0f, 0f, -10f ), OriginX = 0 };
		Assert.AreEqual( 0, Latch( 0f, 0f, behind ), "behind the eye" );
	}

	/// <summary><b>The pointer must lie strictly inside</b> (<c>0x005892fd</c>: <c>TEST AH,0x41</c> jumps on equal too): a
	/// picture of no size, whose circle is a point, is not latched with the pointer on that point.</summary>
	/// <remarks><b>Mutations:</b> the test not strict.</remarks>
	[TestMethod]
	public void ThePointerMustLieStrictlyInside()
	{
		var point = Mike() with { Width = 0, Height = 0, OriginX = 0 };
		var x = point.Origin.X / Depth;

		Assert.AreEqual( 0, Latch( x, 0.611f, point ) );
		Assert.AreEqual( 26, Latch( 0.317f, 0.611f, Mike(), point ), "beside a picture that has a circle" );
	}

	/// <summary>
	/// <b>The latched person is taken only on the hovered cell or one of its eight neighbours</b>, by the cells' lists,
	/// whoever heads them (<c>FUN_00532bd0</c>); two cells away, or nobody latched, nobody is taken.
	/// </summary>
	/// <remarks><b>Mutations:</b> the hovered cell alone; the list's head alone; any distance taken.</remarks>
	[TestMethod]
	public void TheLatchedPersonIsTakenOnTheCellOrANeighbour()
	{
		var state = new ParkState( parkIsClosed: false, visitorsToDate: 0 );
		state.StandOn( 26, 41, 21 );
		state.StandOn( 30, 41, 21 );
		state.StandOn( 31, 43, 23 );

		Assert.AreEqual( 26, SpritePick.Taken( state, 26, 40, 20 ), "a diagonal neighbour, behind another on its list" );
		Assert.AreEqual( 26, SpritePick.Taken( state, 26, 41, 21 ) );
		Assert.AreEqual( 0, SpritePick.Taken( state, 26, 39, 21 ), "two cells off" );
		Assert.AreEqual( 31, SpritePick.Taken( state, 31, 42, 22 ) );
		Assert.AreEqual( 0, SpritePick.Taken( state, 0, 41, 21 ), "nobody latched" );
		Assert.AreEqual( 0, SpritePick.Taken( state, 26, 127, 127 ), "at the map's edge, nobody near" );
	}

	/// <summary>
	/// <b>The pointer's latch puts the window's pixel in the box and takes the across at the window's height over its
	/// width</b> (<see cref="ParkPicking.LatchFor"/>, <c>0x008bcbcc</c>). At 1280 x 720 that is 0.5625: the circle's middle
	/// moves to 0.3264 and its radius to 0.1497, so 0.146 left of the middle is inside and 0.153 is not, where at 0.75, at
	/// 1 or at the width over the height both would be inside.
	/// </summary>
	/// <remarks><b>Mutations:</b> the across at 0.75, at 1 or at the width over the height; the box's width and height
	/// swapped.</remarks>
	[TestMethod]
	public void ThePointersLatchTakesTheAcrossAtTheWindowsHeightOverItsWidth()
	{
		static int At( float x, float y ) => ParkPicking.LatchFor( [Mike()], new OpenTPW.Vector2( (x + 1f) * 640f, (y + 1f) * 360f ),
			1280f, 720f, Plain );

		Assert.AreEqual( 26, At( 0.3264f - 0.146f, 0.611f ) );
		Assert.AreEqual( 0, At( 0.3264f - 0.153f, 0.611f ), "past the radius at 0.5625" );
		Assert.AreEqual( 26, At( 0.3264f + 0.146f, 0.611f ) );
	}

	/// <summary>The window's pixel in the -1 to 1 box, down positive.</summary>
	[TestMethod]
	public void APixelIsPutInTheBoxDownPositive()
	{
		var box = SpritePick.Box( new OpenTPW.Vector2( 1500, 1080 ), 2000, 1200 );
		Assert.AreEqual( 0.5f, box.X, 1e-5f );
		Assert.AreEqual( 0.8f, box.Y, 1e-5f );
	}
}
