using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.Files;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Land outside the park is drawn at half light: <c>Terrain_MarkLandOutsidePark</c> (<c>0x00450d10</c>) marks each ground cell whose map cell, held to
/// <c>MapInfo</c>'s limits, carries <c>mFlags</c> <c>0x40</c> and not <c>0x80</c> (docs/exe/park-engine.md, "The park
/// view's colours").
/// </summary>
[TestClass]
public class ParkGroundHalfLightTests
{
	/// <summary>
	/// One row of map flags, each a case a wrong rule gets wrong: outside the park, the approach kept lit, inside, the
	/// approach with another bit beside it, and outside with another bit beside it.
	/// </summary>
	/// <remarks><b>Mutation:</b> the <c>0x80</c> test dropped fails on cells 1 and 3; <c>0x40</c> alone read as
	/// <c>flags == 0x40</c> fails on cell 4.</remarks>
	[TestMethod]
	public void OnlyLandOutsideTheParkAndNotKeptLitIsHalved()
	{
		ushort[] row = [0x40, 0xc0, 0x00, 0x4c0, 0x440];

		var half = ParkGround.LandOutsideThePark( 5, 1, 5, 1, ( x, y ) => row[x] );

		CollectionAssert.AreEqual( new[] { true, false, false, false, true }, half );
	}

	/// <summary>
	/// The field's last column and row ask the map's last (<c>0x00450d3d</c> to <c>0x00450d64</c>): the map here is
	/// outside the park in its last column and row, inside past them, so a rule that does not hold the cell lights the
	/// field's last column and row in full.
	/// </summary>
	/// <remarks><b>Mutation:</b> the clamp dropped, or held one further out, fails here.</remarks>
	[TestMethod]
	public void TheFieldsLastColumnAndRowAskTheMapsLast()
	{
		ushort Flags( int x, int y ) => x == 4 || y == 1 ? (ushort)0x40 : (ushort)0;

		var half = ParkGround.LandOutsideThePark( 6, 3, 5, 2, Flags );

		for ( var y = 0; y < 3; ++y )
			for ( var x = 0; x < 6; ++x )
				Assert.AreEqual( x >= 4 || y >= 1, half[(y * 6) + x], $"cell ({x}, {y})" );
	}

	/// <summary>
	/// The stock Lost Kingdom park: 5,102 of the 96 x 85 ground cells, the 4,922 the running original carries over
	/// 0..94 x 0..83 (Q479, read live from its cell words), and all 180 of the last column and row. The
	/// approach before the gate and the park's inside stay lit.
	/// </summary>
	/// <remarks><b>Mutation:</b> the <c>0x80</c> test dropped fails on the count and on (40, 5). The clamp is not seen
	/// here: the map's column 95 and row 84 are outside the park as well, so <see cref="TheFieldsLastColumnAndRowAskTheMapsLast"/>
	/// guards it.</remarks>
	[TestMethod]
	public void TheStockJunglePark()
	{
		var data = GameData.Required();
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );

		var half = ParkGround.LandOutsideThePark( 96, 85, 95, 84, ( x, y ) => world.CellAt( x, y ).Flags );

		Assert.AreEqual( 5102, half.Count( h => h ) );
		Assert.AreEqual( 4922, Enumerable.Range( 0, 84 ).Sum( y => Enumerable.Range( 0, 95 ).Count( x => half[(y * 96) + x] ) ) );
		Assert.IsTrue( half[(50 * 96) + 95], "(95, 50) asks (94, 50), outside the park" );
		Assert.IsFalse( half[(5 * 96) + 40], "(40, 5) is the approach, kept lit" );
		Assert.IsFalse( half[(20 * 96) + 40], "(40, 20) is inside the park" );
	}

	/// <summary>
	/// Where <c>GraphicalOptions.LIGHTING</c> is off no cell is half lit, and where it is on a half-lit cell's corners
	/// carry <see cref="ParkGround.HalfLight"/> and no other cell's does.
	/// </summary>
	/// <remarks><b>Mutation:</b> the lighting test dropped fails the null; <c>CornerFlags</c> returning the flag for
	/// every cell, or never, fails the pair of cells.</remarks>
	[TestMethod]
	public void OnlyAHalfLitCellsCornersCarryTheFlagAndOnlyWhileLightingIsOn()
	{
		ushort[] row = [0x40, 0x00];

		Assert.IsNull( ParkGround.HalfLitCells( false, 2, 1, 2, 1, ( x, y ) => row[x] ) );

		var half = ParkGround.HalfLitCells( true, 2, 1, 2, 1, ( x, y ) => row[x] );

		Assert.AreEqual( ParkGround.HalfLight, ParkGround.CornerFlags( half, 0 ) );
		Assert.AreEqual( 0u, ParkGround.CornerFlags( half, 1 ) );
		Assert.AreEqual( 0u, ParkGround.CornerFlags( null, 0 ) );
	}

	/// <summary>
	/// The model shader halves a park-lit corner's byte colour under the flag <see cref="ParkGround"/> sets, and
	/// under no other: its constant is the code's, and the halving is behind the flag and behind the park light.
	/// </summary>
	/// <remarks><b>Mutation:</b> the shader's constant changed, or the halving moved out of the flag's test, fails.</remarks>
	[TestMethod]
	public void TheModelShaderHalvesUnderTheFlagTheGroundSets()
	{
		var text = File.ReadAllText( Path.Combine( System.AppContext.BaseDirectory, "content", "shaders", "test.shader" ) );

		StringAssert.Contains( text, $"const uint FLAG_HALF_LIGHT = 0x{ParkGround.HalfLight:x};" );
		StringAssert.Contains( text,
			"if ((matFlags & FLAG_HALF_LIGHT) != 0)\n                vParkLight = floor(round(vParkLight * 255.0) / 2.0) / 255.0;" );
	}
}
