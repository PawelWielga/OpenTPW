using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace OpenTPW.Tests;

/// <summary>
/// The procedural ground's tiles against the original's own <c>FUN_0055f780</c> run under an emulator on the shipped
/// files (<c>lndtile.py</c>; docs/exe/park-engine.md, "The procedural ground"). Every expected hash is of its tiles'
/// red, green and blue, row 0 first, made there and nowhere else.
/// </summary>
[TestClass]
public class ProceduralGroundTests
{
	private static ProceduralGround Ground( string theme, int size = 16 )
	{
		var data = GameData.Required();

		using var stream = data.OpenRead( $"levels/{theme}/terrain/base.lnd" );
		return new ProceduralGround( new ProceduralGroundFile( stream ), size );
	}

	private static string Hash( byte[] bytes ) => Convert.ToHexString( SHA256.HashData( bytes ) ).ToLowerInvariant();

	/// <summary>
	/// Every cell of the box, 0..95 x 0..84, of all four parks at 16 pixels, as one run of tiles row by row.
	/// </summary>
	/// <remarks><b>Mutation:</b> any step of the tile changed - a ramp, the window, a layer's pair, a span's class, the
	/// frozen u1, the rows' order - changes a hash.</remarks>
	[TestMethod]
	[DataRow( "jungle", "f8c9664ac8568557dadaba9a1610d8edd58d893f6515334cdbf90d6363cb02f7" )]
	[DataRow( "fantasy", "7078add17d1fc5a4bc00c2aa57b16a338919532febfe5fe03d7f5955b4b9cd04" )]
	[DataRow( "hallow", "6e86ce1f643687708f5eaa6140d5ff773615f9312ce16daf9098c75b1c4f30b9" )]
	[DataRow( "space", "b9360aa15ba92dd1fb7d97622f241c520049ecadbbe1148ed8ef4f1d22b0cadd" )]
	public void EveryTileOfEveryParkIsTheOriginals( string theme, string expected )
	{
		var ground = Ground( theme );
		using var all = new MemoryStream();

		for ( var y = 0; y < 85; ++y )
			for ( var x = 0; x < 96; ++x )
				all.Write( ground.Tile( x, y ) );

		Assert.AreEqual( expected, Hash( all.ToArray() ) );
	}

	/// <summary>
	/// One jungle cell for each rule a wrong reading gets wrong, so a failure names its rule: (0, 0) has spans with both
	/// masks mixed, where every pixel takes the span's first u1; (28, 0) is refused by the map's byte 0 and grey;
	/// (62, 41) is all the second grass; (95, 40) is the box's last column, whose samples run on into the map's next row.
	/// </summary>
	/// <remarks><b>Mutation:</b> u1 stepped in the mixed spans fails (0, 0); the gate's four samples moved fails
	/// (28, 0); a whole span lerped by 255 fails (62, 41); the map read clamped or by two indices fails (95, 40).</remarks>
	[TestMethod]
	[DataRow( 0, 0, "8b0289e2b488b20dce18920c5f8a7a8e3d74c43bb5d4b761be750eb6a6ed52ac" )]
	[DataRow( 28, 0, "cbab715ff6cfaa81c9b09e014dc1406ceff24034caa265de65f9f948c5434807" )]
	[DataRow( 62, 41, "4298bd86e742d9f0e04c34d20c35fed8d8cfbb97f1297aa5b9fa602d08dcb48c" )]
	[DataRow( 95, 40, "e3a190acb91313675c1e1d65b307c59dee4567f27cbcba800e8c8994b1169c27" )]
	public void EachRuleOnItsOwnCell( int x, int y, string expected )
		=> Assert.AreEqual( expected, Hash( Ground( "jungle" ).Tile( x, y ) ) );

	/// <summary>The gate's grey is <c>0x7f7f7f</c> on every pixel.</summary>
	[TestMethod]
	public void ARefusedCellIsGrey()
		=> Assert.IsTrue( Ground( "jungle" ).Tile( 28, 0 ).All( b => b == ProceduralGround.Grey ) );

	/// <summary>
	/// <c>FUN_0055e710</c> on jungle's first pair (48, 40): lo 28, hi 68, nought at 27, 255 at 69, and at 48
	/// <c>(48 x 255 - 28 x 255) / 41</c> = 124.
	/// </summary>
	/// <remarks><b>Mutation:</b> dividing by w where the original divides by w + 1 gives 127 at 48; 255 from hi itself
	/// rather than above it gives 255 at 68.</remarks>
	[TestMethod]
	public void TheRampIsTheOriginals()
	{
		var ramp = ProceduralGround.Ramp( 48, 40 );

		Assert.AreEqual( 0, ramp[27] );
		Assert.AreEqual( 0, ramp[28] );
		Assert.AreEqual( 124, ramp[48] );
		Assert.AreEqual( 248, ramp[68] );
		Assert.AreEqual( 255, ramp[69] );
	}

	/// <summary>The file reads to its end, its sets are 64, 32 and 16, and its map is 384 x 344 of four bytes.</summary>
	[TestMethod]
	public void TheFileReadsWhole()
	{
		var data = GameData.Required();

		foreach ( var theme in new[] { "jungle", "fantasy", "hallow", "space" } )
		{
			using var stream = data.OpenRead( $"levels/{theme}/terrain/base.lnd" );
			var file = new ProceduralGroundFile( stream );

			Assert.AreEqual( stream.Length, stream.Position, theme );
			CollectionAssert.AreEqual( new[] { 64, 32, 16 }, file.Sets.Select( set => set.TileSize ).ToArray(), theme );
			Assert.AreEqual( (384, 344, 4), (file.Map.Width, file.Map.Height, file.Map.Depth), theme );
		}
	}

	/// <summary>
	/// The texture coordinates of grid corners (x, y), (x + 1, y), (x + 1, y + 1), (x, y + 1) for a cell's word, the
	/// original's ring (x, y), (x, y + 1), (x + 1, y + 1), (x + 1, y) read in <c>FUN_0056f670</c>: a quarter turn
	/// (<c>0x08</c>) gives (x, y) ring corner 3's (1, 0); three quarters (<c>0x20</c>) gives it ring corner 1's (0, 1);
	/// with two turn bits set the first of <c>0x08</c>, <c>0x10</c>, <c>0x20</c> wins.
	/// </summary>
	/// <remarks><b>Mutation:</b> the ring run our old way round (0, 0), (1, 0), (1, 1), (0, 1) swaps the 0x08 and 0x20
	/// rows; the rotation taken as one field (a switch on <c>0x38</c>) gives 0x18 nothing and 0x30 the three-quarter
	/// turn; the mirror dropped fails 0x40 and 0x48.</remarks>
	[TestMethod]
	[DataRow( (ushort)0x00, new[] { 0f, 0f, 1f, 0f, 1f, 1f, 0f, 1f } )]
	[DataRow( (ushort)0x40, new[] { 0f, 0f, 0f, 1f, 1f, 1f, 1f, 0f } )]
	[DataRow( (ushort)0x08, new[] { 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f } )]
	[DataRow( (ushort)0x10, new[] { 1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f } )]
	[DataRow( (ushort)0x20, new[] { 0f, 1f, 0f, 0f, 1f, 0f, 1f, 1f } )]
	[DataRow( (ushort)0x18, new[] { 1f, 0f, 1f, 1f, 0f, 1f, 0f, 0f } )]
	[DataRow( (ushort)0x30, new[] { 1f, 1f, 0f, 1f, 0f, 0f, 1f, 0f } )]
	[DataRow( (ushort)0x48, new[] { 0f, 1f, 1f, 1f, 1f, 0f, 0f, 0f } )]
	public void ACellsWordTurnsItsArtAsTheOriginalsRing( ushort flags, float[] expected )
	{
		var uvs = ParkGround.CellUvs( flags );

		CollectionAssert.AreEqual( expected, uvs.SelectMany( uv => new[] { uv.X, uv.Y } ).ToArray() );
	}

	/// <summary>
	/// The box: every shipped park's offsets are -1, held to nought, so it is 0, 0 to 95, 84; an offset of 1 or more
	/// widens it.
	/// </summary>
	/// <remarks><b>Mutation:</b> the offset taken as it is gives 94, 83; held to nought at 1 rather than below it gives
	/// 98 or 86 where an offset is 1.</remarks>
	[TestMethod]
	public void TheBoxIsMapInfosAndTheOffsetsHeldAtNought()
	{
		Assert.AreEqual( (0, 0, 95, 84), GroundTiles.Box( 0, 0, 95, 84, -1, -1 ) );
		Assert.AreEqual( (3, 2, 99, 88), GroundTiles.Box( 3, 2, 95, 84, 1, 2 ) );
		Assert.AreEqual( (3, 2, 100, 87), GroundTiles.Box( 3, 2, 95, 84, 2, 1 ) );
	}

	/// <summary>
	/// A tile is drawn from its first texel's centre to its last's: cell (2, 3) of 16-pixel tiles in a 1,536 x 1,360
	/// texture runs from (32.5, 48.5) to (47.5, 63.5) texels.
	/// </summary>
	/// <remarks><b>Mutation:</b> no half-texel inset gives 32 and 48; the full size across gives 48.5.</remarks>
	[TestMethod]
	public void ATileRunsFromTexelCentreToTexelCentre()
	{
		var first = GroundTiles.TileUv( 2, 3, new Vector2( 0f, 0f ), 16, 1536, 1360 );
		var last = GroundTiles.TileUv( 2, 3, new Vector2( 1f, 1f ), 16, 1536, 1360 );

		Assert.AreEqual( 32.5f, first.X * 1536, 1e-3f );
		Assert.AreEqual( 48.5f, first.Y * 1360, 1e-3f );
		Assert.AreEqual( 47.5f, last.X * 1536, 1e-3f );
		Assert.AreEqual( 63.5f, last.Y * 1360, 1e-3f );
	}

	/// <summary>
	/// With the compositor on a ground cell loses its mirror and turn (<c>FUN_0046ded0(1)</c>): jungle's (0, 0) reads
	/// <c>0x42</c> in base.MD2 and is drawn as <c>0x02</c>; a path cell (<c>0x80</c>) and a cell the scenery draws
	/// (<c>0x01</c>) keep theirs; with it off the word stands.
	/// </summary>
	/// <remarks><b>Mutation:</b> the clear dropped keeps 0x42; clearing path cells too gives 0x82 for 0xea; masking with
	/// 0xff87 on 0x01 cells gives 0x03 for 0x7b.</remarks>
	[TestMethod]
	public void WithTheCompositorOnAGroundCellIsNotTurned()
	{
		Assert.AreEqual( (ushort)0x02, ParkGround.DrawnWord( 0x42, true ) );
		Assert.AreEqual( (ushort)0x502, ParkGround.DrawnWord( 0x57a, true ) );
		Assert.AreEqual( (ushort)0xea, ParkGround.DrawnWord( 0xea, true ) );
		Assert.AreEqual( (ushort)0x7b, ParkGround.DrawnWord( 0x7b, true ) );
		Assert.AreEqual( (ushort)0x42, ParkGround.DrawnWord( 0x42, false ) );
	}

	/// <summary>
	/// The tiles are laid bottom up, as <c>test.shader</c> turns every v over: in a box of 4 x 5 jungle tiles (80 rows),
	/// tile (2, 3)'s row 0 stands on row 80 - 1 - 48 = 31 and its row 15 on row 16, from column 32.
	/// </summary>
	/// <remarks><b>Mutation:</b> laid top down, row 0 stands on row 48 and the field is drawn mirrored about its middle
	/// row (Q488's run2, every cell showing another's tile).</remarks>
	[TestMethod]
	public void TheTilesAreLaidBottomUp()
	{
		var ground = Ground( "jungle" );
		var pixels = GroundTiles.Lay( ground, 4, 5, out _ );
		var tile = ground.Tile( 2, 3 );

		foreach ( var (k, row) in new[] { (0, 31), (15, 16) } )
		{
			for ( var j = 0; j < 16; ++j )
			{
				var at = ((row * 64) + 32 + j) * 4;
				var from = ((k * 16) + j) * 3;
				CollectionAssert.AreEqual( tile[from..(from + 3)], pixels[at..(at + 3)], $"row {k}, column {j}" );
			}
		}
	}
}
