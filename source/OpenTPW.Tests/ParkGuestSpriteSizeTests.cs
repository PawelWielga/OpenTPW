using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// A person is drawn the original's size: its span is added in clip space, after the draw matrix and before the divide by
/// w, which is sin 45 of the eye's depth (<c>FUN_005890e0</c>, <c>FUN_00578be0</c>; <c>docs/exe/park-engine.md</c>, "The
/// thing under a sprite"). The figures are Mike Cooper as the original drew him under the mechanics' locator
/// (<c>q470/orig/</c>): a picture 41 x 52, origin 21 across and 45 down, at a w of 77.782, his hair's top at row 350 and
/// his feet's bottom at 389 of the 1024 x 768 frame.
/// </summary>
[TestClass]
public class ParkGuestSpriteSizeTests
{
	private static readonly float Depth = 77.782f / MathF.Sin( MathF.PI / 4f );

	/// <summary>A point square on to the eye, on its axis, as a pixel of the original's 1024 x 768 frame through the park's lens at 4:3.</summary>
	private static (float X, float Y) Pixel( float across, float up )
	{
		var place = System.Numerics.Vector4.Transform( new System.Numerics.Vector4( across, up, -Depth, 1f ), Camera.Lens( ParkOrbitCameraMode.Lens, 4f / 3f ).Projection );
		return ((place.X / place.W + 1f) * 512f, (1f - place.Y / place.W) * 384f);
	}

	/// <summary>The original's span over w, in its frame's pixels: a picture pixel is 20 / 128 of clip, over w, times 384 down
	/// and, at 0.75 across, times 512 across; 60 / w both ways.</summary>
	private static float Original( float pixels ) => pixels * 20f / 128f / 77.782f * 384f;

	/// <summary>
	/// <b>Mike stands where the original drew him</b>: 34.71 px above his origin and 5.40 below, 40.11 in all, and 16.20
	/// left of it and 15.43 right. Spanning the world twenty a reference instead draws him 1.41 times that,
	/// 56.73 high.
	/// </summary>
	[TestMethod]
	public void APersonIsTheOriginalsSize()
	{
		var (left, right, above, below) = ParkGuestSprites.Extent( 41, 52, 21, 45 );

		Assert.AreEqual( 384f - Original( 45 ), Pixel( 0f, above ).Y, 0.05f, "his top, row 349.3 (350 drawn)" );
		Assert.AreEqual( 384f + Original( 7 ), Pixel( 0f, -below ).Y, 0.05f, "his feet, row 389.4 (389 drawn)" );
		Assert.AreEqual( 512f - Original( 21 ), Pixel( -left, 0f ).X, 0.05f, "his left edge" );
		Assert.AreEqual( 512f + Original( 20 ), Pixel( right, 0f ).X, 0.05f, "his right edge" );
	}

	/// <summary>
	/// <b>A reflected person turns about their feet</b>: the original negates both sides about the origin and keeps the
	/// texture, so Mike's reflected 38 x 50, origin 22 across (<c>q470/orig/b-run.out</c>, flags <c>0x10</c>), shows his
	/// picture's left edge on the right, at 528.97 where his circle's middle read 529, and its right edge on the left, at
	/// 499.66; his origin's column stands on the origin's pixel, 512, both ways round. Reflected about the quad's middle
	/// it would stand at 507.37, the edges at 524.34 and 495.03.
	/// </summary>
	[TestMethod]
	public void AReflectedPersonTurnsAboutTheirFeet()
	{
		var (left, leftColumn, right, rightColumn) = ParkGuestSprites.Across( 38, 50, 22, 42, mirrored: true );

		Assert.AreEqual( (38, 0), (leftColumn, rightColumn ), "the picture's right edge on the left" );
		Assert.AreEqual( 512f - Original( 16 ), Pixel( left, 0f ).X, 0.05f, "the left side, 499.66" );
		Assert.AreEqual( 512f + Original( 22 ), Pixel( right, 0f ).X, 0.05f, "the right side, 528.97" );

		var origin = left + ((right - left) * (22 - leftColumn) / (rightColumn - leftColumn));
		Assert.AreEqual( 512f, Pixel( origin, 0f ).X, 0.05f, "the origin's column on the origin" );

		var (directLeft, directLeftColumn, directRight, directRightColumn) = ParkGuestSprites.Across( 38, 50, 22, 42, mirrored: false );
		Assert.AreEqual( (0, 38), (directLeftColumn, directRightColumn) );
		Assert.AreEqual( 512f - Original( 22 ), Pixel( directLeft, 0f ).X, 0.05f, "unreflected, the left edge 495.03" );
		Assert.AreEqual( 512f + Original( 16 ), Pixel( directRight, 0f ).X, 0.05f );
	}

	/// <summary>
	/// <b>The quad written for it stands the same way</b>: seen along +y, the reflected 38 x 50's top left corner is
	/// 16 picture pixels left of the point and shows the picture's right edge (u 0.5 of 0.25 to 0.5), its top right 22
	/// right of it and shows the left edge, so the origin's column, u 0.25 + 22/38 of the way, lies on the point.
	/// </summary>
	[TestMethod]
	public void TheReflectedQuadShowsTheOriginOnThePoint()
	{
		var picture = new ParkGuestSprites.Region( 0.25f, 0.1f, 0.5f, 0.3f, 38, 50, 22, 42 );
		var corners = new Vertex[4];
		var centre = new OpenTPW.Vector3( 100f, 0f, 0f );
		var pixel = ParkGuestSprites.Extent( 1, 1, 0, 0 ).Right;

		Assert.IsTrue( ParkGuestSprites.Corners( corners, new OpenTPW.Vector3( 0f, 1f, 0f ), centre, picture, mirrored: true, 255 ) );
		Assert.AreEqual( 100f - (16f * pixel), corners[0].Position.X, 1e-4f, "the top left, 16 picture pixels left" );
		Assert.AreEqual( 100f + (22f * pixel), corners[1].Position.X, 1e-4f, "the top right, 22 right" );
		Assert.AreEqual( (0.5f, 0.25f), (corners[0].TexCoords.X, corners[1].TexCoords.X), "the picture reflected" );
		Assert.AreEqual( (corners[0].Position.X, corners[1].Position.X), (corners[3].Position.X, corners[2].Position.X) );
		Assert.AreEqual( (0.5f, 0.25f), (corners[3].TexCoords.X, corners[2].TexCoords.X), "the bottom edge reflected the same way" );
		Assert.AreEqual( (0.1f, 0.1f, 0.3f, 0.3f), (corners[0].TexCoords.Y, corners[1].TexCoords.Y, corners[2].TexCoords.Y, corners[3].TexCoords.Y), "rows are not reflected" );

		var origin = 0.25f + (0.25f * 22f / 38f);
		var along = (origin - corners[0].TexCoords.X) / (corners[1].TexCoords.X - corners[0].TexCoords.X);
		Assert.AreEqual( 100f, corners[0].Position.X + ((corners[1].Position.X - corners[0].Position.X) * along), 1e-3f, "the origin's column on the point" );

		var direct = new Vertex[4];
		Assert.IsTrue( ParkGuestSprites.Corners( direct, new OpenTPW.Vector3( 0f, 1f, 0f ), centre, picture, mirrored: false, 255 ) );
		Assert.AreEqual( 100f - (22f * pixel), direct[0].Position.X, 1e-4f, "unreflected, the top left is 22 picture pixels left" );
		Assert.AreEqual( 100f + (16f * pixel), direct[1].Position.X, 1e-4f, "and the top right 16 right" );
		Assert.AreEqual( (0.25f, 0.5f, 0.5f, 0.25f), (direct[0].TexCoords.X, direct[1].TexCoords.X, direct[2].TexCoords.X, direct[3].TexCoords.X), "the picture as drawn" );
		Assert.AreEqual( (direct[0].Position.X, direct[1].Position.X), (direct[3].Position.X, direct[2].Position.X) );
	}

	/// <summary>
	/// <b>The quad written for a picture is the size Extent gives, square on to the eye</b>: <c>ParkGuestSprites.WriteQuad</c>
	/// hands its corners to <c>Corners</c> with the picture, the reflection and the instance scale, and nothing else. Mike's
	/// 41 x 52, origin 21, 45, seen from 50 degrees down: his top 45 picture pixels above the point and his feet 7 below,
	/// each pixel 20 / 128 over 1.4142 of the world, along the camera's own up (0, sin 50, cos 50), and all four corners
	/// at the one depth along the view. A rider's head (scale 0.685) is that much of it. A quad that took its own scale
	/// (20 / 128 itself, or the world up) or dropped the instance scale fails.
	/// </summary>
	[TestMethod]
	public void TheQuadWrittenIsTheExtentSquareToTheEye()
	{
		var pitch = 50f * MathF.PI / 180f;
		var forward = new OpenTPW.Vector3( 0f, MathF.Cos( pitch ), -MathF.Sin( pitch ) );
		var upward = new OpenTPW.Vector3( 0f, MathF.Sin( pitch ), MathF.Cos( pitch ) );
		var centre = new OpenTPW.Vector3( 3f, 4f, 5f );
		var picture = new ParkGuestSprites.Region( 0.1f, 0.2f, 0.3f, 0.4f, 41, 52, 21, 45 );
		var corners = new Vertex[4];
		var pixel = 20f / 128f / 1.41421356f;

		Assert.IsTrue( ParkGuestSprites.Corners( corners, forward, centre, picture, mirrored: false, 255 ) );
		AssertCorners( corners, centre, upward, forward, pixel, left: 21, right: 20, above: 45, below: 7 );

		Assert.IsTrue( ParkGuestSprites.Corners( corners, forward, centre, picture, mirrored: false, 255, ParkGuestSprites.HeadScale ) );
		AssertCorners( corners, centre, upward, forward, pixel * ParkGuestSprites.HeadScale, left: 21, right: 20, above: 45, below: 7 );
	}

	private static void AssertCorners( Vertex[] corners, OpenTPW.Vector3 centre, OpenTPW.Vector3 upward, OpenTPW.Vector3 forward,
		float pixel, float left, float right, float above, float below )
	{
		var across = new OpenTPW.Vector3( 1f, 0f, 0f );
		var expected = new[]
		{
			centre - (across * left * pixel) + (upward * above * pixel),
			centre + (across * right * pixel) + (upward * above * pixel),
			centre + (across * right * pixel) - (upward * below * pixel),
			centre - (across * left * pixel) - (upward * below * pixel),
		};

		for ( var i = 0; i < 4; ++i )
		{
			Assert.AreEqual( expected[i].X, corners[i].Position.X, 1e-4f, $"corner {i} across" );
			Assert.AreEqual( expected[i].Y, corners[i].Position.Y, 1e-4f, $"corner {i} depth" );
			Assert.AreEqual( expected[i].Z, corners[i].Position.Z, 1e-4f, $"corner {i} up" );
		}

		Assert.AreEqual( (corners[0].Position - corners[3].Position).Dot( forward ), 0f, 1e-4f, "square on to the eye" );
	}

	/// <summary>A rider's head keeps its 0.685 of that (<c>0x0074ced0</c>): a 30-px head 30 x 0.685 x 60 / w = 15.85 px high.</summary>
	[TestMethod]
	public void AHeadIsItsShareOfThat()
	{
		var (_, _, above, below) = ParkGuestSprites.Extent( 24, 30, 12, 15, ParkGuestSprites.HeadScale );

		Assert.AreEqual( Original( 30 ) * 0.685f, Pixel( 0f, -below ).Y - Pixel( 0f, above ).Y, 0.05f );
	}

	/// <summary>
	/// The park's projection carries cos 45 on x and y and the screen scale 2 on y: 1.4142 of clip a unit of world. The law
	/// is the cosine's, which at 90 the sine equals: at the lobby's 100 it is 2 cos 50, 1.2856, where the sine gives 1.5321.
	/// </summary>
	[TestMethod]
	public void TheParksClipIsTheRootOfTwoAUnit()
	{
		Assert.AreEqual( 1.41421f, Camera.ClipPerWorld( ParkOrbitCameraMode.HandedAngle ), 1e-4f );
		Assert.AreEqual( 1.28558f, Camera.ClipPerWorld( 100f ), 1e-4f );
		Assert.AreEqual( 20f / 1.41421f, ParkGuestSprites.WorldPerReference, 1e-3f );
	}
}
