using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// A person's body is tested nearer the eye than it is drawn: <c>Sprite_BuildQuad</c> moves each corner's clip z to
/// <c>z - 0.2 / (z + 1)</c> through a draw matrix of near 0.1 and far 1000 (<c>docs/exe/park-engine.md</c>, "What hides a
/// person below their feet"), and the people's quads go to the buffer furthest first, as they write depth.
/// </summary>
[TestClass]
public class ParkGuestSpriteDepthTests
{
	[TestCleanup]
	public void LetThePoolsGo() => TestRun.DeleteEvery<ParkGuestSprites>();

	/// <summary>
	/// <b>The pull is the original's at every depth</b>: at the locator's 110 Mike is tested at 106.185, 3.81 nearer, and
	/// at 20 at 16.840, 3.16 nearer. A pull of a fixed 3.81 puts 20 at 16.19, and the decode's own
	/// <c>1/e + 3.95 / e²</c> shorthand at 16.70.
	/// </summary>
	[TestMethod]
	public void ThePullIsTheOriginalsAtEveryDepth()
	{
		Assert.AreEqual( 106.185f, ParkGuestSprites.PulledDepth( 110f ), 0.005f, "the locator's zoom" );
		Assert.AreEqual( 16.840f, ParkGuestSprites.PulledDepth( 20f ), 0.005f, "close up" );
		Assert.AreEqual( 3.061f, ParkGuestSprites.PulledDepth( 5f ), 0.005f, "closer still" );
	}

	/// <summary>
	/// <b>A pulled corner lands on the same pixel</b>: it moves along its own line from the eye, so through any lens it
	/// projects where it did, and its depth from the eye is the pulled one.
	/// </summary>
	[TestMethod]
	public void APulledCornerLandsOnTheSamePixel()
	{
		var eye = new OpenTPW.Vector3( 10f, -20f, 30f );
		var forward = new OpenTPW.Vector3( 0f, 0.6f, -0.8f );
		var corner = eye + (forward * 110f) + new OpenTPW.Vector3( 7f, 0.8f, 0.6f );

		var corners = new[] { new Vertex( corner, new OpenTPW.Vector2( 0.25f, 0.5f ) ) { MatFlags = 0xff123456u } };
		ParkGuestSprites.Pull( corners, eye, forward );

		var before = corner - eye;
		var after = corners[0].Position - eye;

		Assert.AreEqual( 106.185f, after.Dot( forward ), 0.005f, "tested at the pulled depth" );
		Assert.AreEqual( 0f, before.Normal.Cross( after.Normal ).Length, 1e-5f, "on the same line from the eye" );
		Assert.IsTrue( after.Dot( before ) > 0f, "on the eye's side it was on" );
		Assert.AreEqual( (0.25f, 0.5f, 0xff123456u), (corners[0].TexCoords.X, corners[0].TexCoords.Y, corners[0].MatFlags), "the picture and colour kept" );
	}

	/// <summary>
	/// <b>The people go to the buffer furthest first</b>, each quad's four corners and its depth together: drawn at
	/// 5, 50 and 20, they are written 50, 20, 5, and a fourth past the count stays where it is.
	/// </summary>
	[TestMethod]
	public void ThePeopleGoFurthestFirst()
	{
		var depths = new[] { 5f, 50f, 20f, 99f };
		var vertices = new Vertex[16];

		for ( var i = 0; i < vertices.Length; ++i )
			vertices[i] = new Vertex( new OpenTPW.Vector3( i / 4, i % 4, 0f ), new OpenTPW.Vector2( 0f, 0f ) );

		ParkGuestSprites.FurthestFirst( vertices, depths, 3 );

		CollectionAssert.AreEqual( new[] { 50f, 20f, 5f, 99f }, depths );

		for ( var i = 0; i < vertices.Length; ++i )
			Assert.AreEqual( (new[] { 1, 2, 0, 3 }[i / 4], i % 4), ((int)vertices[i].Position.X, (int)vertices[i].Position.Y), $"vertex {i}" );
	}

	/// <summary>
	/// <b>A frame's quads are sorted before they go up</b>: written at 5, 50 and 20, the frame ends with them 50, 20, 5,
	/// and the upload still reaches the three.
	/// </summary>
	[TestMethod]
	public void AFrameEndsFurthestFirst()
	{
		var pool = new ParkGuestSprites( "jungle", null );
		pool.Resize( 4 );

		var (vertices, depths) = pool.Written;
		var drawn = new[] { 5f, 50f, 20f };

		for ( var i = 0; i < 12; ++i )
			vertices[i] = new Vertex( new OpenTPW.Vector3( i / 4, 0f, 0f ), new OpenTPW.Vector2( 0f, 0f ) );

		drawn.CopyTo( depths, 0 );

		Assert.AreEqual( 3, pool.Finish( 3 ) );
		CollectionAssert.AreEqual( new[] { 50f, 20f, 5f }, depths[..3] );
		Assert.AreEqual( (1, 2, 0), ((int)vertices[0].Position.X, (int)vertices[4].Position.X, (int)vertices[8].Position.X) );
	}

	/// <summary>
	/// <b>The people write depth through the original's alpha test</b>: their pipeline leaves depth writing on, and
	/// their shader drops a fragment under 16 of 255 (ALPHAFUNC GREATEREQUAL, ALPHAREF 16; <c>render-states.md</c>), not
	/// only a clear one as the particles' shader does.
	/// </summary>
	[TestMethod]
	public void ThePeopleWriteDepthThroughTheAlphaTest()
	{
		Assert.IsFalse( ParkGuestSprites.PeopleMaterial.HasFlag( MaterialFlags.DisableDepthWrite ), "depth written" );
		Assert.IsFalse( ParkGuestSprites.PeopleMaterial.HasFlag( MaterialFlags.DisableDepthTest ), "depth tested" );
		Assert.IsTrue( ParkGuestSprites.PeopleMaterial.HasFlag( MaterialFlags.DisableCulling ), "both faces" );

		var text = File.ReadAllText( Path.Combine( AppContext.BaseDirectory, ParkGuestSprites.PeopleShader ) );
		StringAssert.Contains( text, "if ( fragColor.a < 16.0 / 255.0 )\n          discard;" );
	}
}
