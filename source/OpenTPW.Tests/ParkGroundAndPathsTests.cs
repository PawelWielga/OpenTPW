using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.Files;
using System.IO;

namespace OpenTPW.Tests;

/// <summary>
/// The ground and the paths divide a park's path cells between them (<c>ParkGround.LaysPath</c>: inside the compositor's
/// box the ground lays the path over the cell's tile, outside it <see cref="ParkPaths"/> draws the cell), and a path cell
/// the ground draws is drawn with the word its path wrote (docs/exe/park-engine.md, "What is laid").
/// </summary>
[TestClass]
public class ParkGroundAndPathsTests
{
	private static ParkWorld StockWorld()
	{
		var data = GameData.Required();
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	/// <summary>
	/// Of the stock park's path cells, each is drawn by exactly one of the two: by the ground where the box carries it, by
	/// the paths where it does not, and a cell that is no path is never the paths'.
	/// </summary>
	/// <remarks><b>Mutation:</b> <c>DrawsPathCell</c> answering true inside the box (the cell drawn twice), or false outside
	/// it (the cell drawn by neither), fails; <c>LeavesToOthers</c> dropping the path term fails the outside half.</remarks>
	[TestMethod]
	public void EveryPathCellIsDrawnByTheGroundOrByThePathsAndNeverBoth()
	{
		var world = StockWorld();
		var paths = 0;

		for ( var y = 0; y < 85; ++y )
		{
			for ( var x = 0; x < 96; ++x )
			{
				var cell = ParkState.CellFor( world, x, y );

				foreach ( var laysPath in new[] { true, false } )
				{
					var pathsDraw = ParkPaths.DrawsPathCell( cell, laysPath );

					if ( !ParkPaths.IsPath( cell ) )
					{
						Assert.IsFalse( pathsDraw, $"({x}, {y}) is no path" );
						continue;
					}

					Assert.IsFalse( ParkQueues.IsQueue( cell ) || ParkObjects.CoversGround( cell ), $"({x}, {y}) is only a path" );
					Assert.AreEqual( pathsDraw, ParkGround.LeavesToOthers( cell, laysPath ),
						$"path cell ({x}, {y}), laid over {laysPath}: the ground leaves it to the paths exactly when they draw it" );
					Assert.AreEqual( !laysPath, pathsDraw, $"path cell ({x}, {y})" );
				}

				if ( ParkPaths.IsPath( cell ) )
					++paths;
			}
		}

		Assert.AreEqual( 78, paths, "the stock park's path cells" );
	}

	/// <summary>
	/// A path cell is drawn with the word its path wrote, its turn kept under the compositor (<c>0x80</c>), where the
	/// file's word would be stripped of the turn: some of the stock park's paths run turned, and for them the two differ.
	/// </summary>
	/// <remarks><b>Mutation:</b> <c>DrawnWordAt</c> reading the file's word instead of <c>WordOf</c> fails the turned
	/// cells; <c>WordOf</c> dropping <c>PathWord</c> fails them too.</remarks>
	[TestMethod]
	public void APathCellIsDrawnWithItsPathsWordAndItsTurn()
	{
		var data = GameData.Required();
		var world = StockWorld();

		HeightfieldFile field;
		using ( var stream = data.OpenRead( "levels/jungle/terrain/base.MD2" ) )
			field = new HeightfieldFile( stream );

		var turned = 0;

		for ( var y = 0; y < field.CellsY; ++y )
		{
			for ( var x = 0; x < field.CellsX; ++x )
			{
				var cell = ParkState.CellFor( world, x, y );

				if ( !ParkPaths.IsPath( cell ) )
					continue;

				var file = (ushort)field.Cells[(y * field.CellsX) + x];
				var drawn = ParkGround.DrawnWordAt( world, field, x, y, true );

				Assert.AreEqual( ParkGround.DrawnWord( ParkGround.PathWord( file, cell.TileAngle ), true ), drawn, $"({x}, {y})" );
				Assert.AreEqual( 0x80, drawn & 0x80, $"({x}, {y}) is a path cell" );

				if ( cell.TileAngle != 0 )
				{
					++turned;
					Assert.AreNotEqual( 0, drawn & 0x38, $"({x}, {y}) is turned {cell.TileAngle}" );
					Assert.AreNotEqual( ParkGround.DrawnWord( file, true ), drawn, $"({x}, {y}): the file's word is not the path's" );
				}
			}
		}

		Assert.IsTrue( turned > 0, "the stock park has paths that run turned" );
	}
}
