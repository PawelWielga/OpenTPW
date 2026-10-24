using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The wall between the park and the land not bought (<see cref="ParkBoundaryHoarding"/>; <c>docs/exe/park.md</c>, "The
/// boundary hoarding's piece"): the hoarding table's rows, the pieces Lost Kingdom's file makes, and where each stands.
/// The game's files are read where there is an installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkBoundaryHoardingTests
{
	/// <summary>A row with no name takes the named row above it (<c>0x00522ed2</c>); none past the table's 28.</summary>
	[TestMethod]
	[DataRow( 0, "ho_fos1" )]
	[DataRow( 1, "ho_end1" )]
	[DataRow( 3, "ho_fob1" )]
	[DataRow( 8, "ho_fos1" )]
	[DataRow( 11, "ho_fos4" )]
	[DataRow( 19, "ho_fob2" )]
	[DataRow( 22, "ho_fos2" )]
	[DataRow( 25, "ho_fos3" )]
	[DataRow( 26, "ho_fob3" )]
	[DataRow( 27, "ho_fob4" )]
	[DataRow( 28, null )]
	[DataRow( -1, null )]
	public void ARowDrawsTheNamedRowAtOrAboveIt( int row, string? model )
		=> Assert.AreEqual( model, ParkBoundaryHoarding.ModelOf( row ) );

	private static (ParkWorld World, ParkItemCatalogue Catalogue) Stock()
	{
		var data = GameData.Required();
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );

		return (new ParkWorld( new SaveReader( stream ).ReadFile() ), new ParkItemCatalogue( "jungle", data ));
	}

	/// <summary>The stock park's 143 records of items 17302 to 17323, one on each of its 143 anchors.</summary>
	[TestMethod]
	public void LostKingdomsFileMakesAPieceForEveryAnchor()
	{
		var (world, catalogue) = Stock();
		var pieces = ParkBoundaryHoarding.Read( world, catalogue );

		Assert.AreEqual( 143, ParkBoundaryHoarding.AnchorsOf( world ) );
		Assert.AreEqual( 143, pieces.Count );
		Assert.AreEqual( "ho_fob1 12, ho_fob2 8, ho_fos1 90, ho_fos2 19, ho_fos3 13, ho_fos4 1",
			string.Join( ", ", pieces.GroupBy( piece => piece.Model ).OrderBy( group => group.Key ).Select( group => $"{group.Key} {group.Count()}" ) ) );
		Assert.AreEqual( new ParkBoundaryHoarding.Piece( "ho_fos1", 2, 52, 17, 270 ), pieces[0] );
	}

	/// <summary>
	/// Each piece's two cells square, stood from its record's cell and angle, lands on its anchor's own: the type 25 cell
	/// and the three type 12 cells that name it (<c>FUN_00522f20</c> moves the cell with the turn).
	/// </summary>
	[TestMethod]
	public void EveryPieceStandsOnItsAnchorsFourCells()
	{
		var (world, catalogue) = Stock();
		var square = new ParkItemCatalogue.Item( 17302, "ho", "", "", 2, 2, null );
		var anchors = new System.Collections.Generic.HashSet<(int, int)>();

		foreach ( var piece in ParkBoundaryHoarding.Read( world, catalogue ) )
		{
			var (origin, turn) = ParkBoundaryHoarding.StandOf( piece, null );
			var (left, top, right, bottom) = ParkObjects.FootprintOf( square, origin, turn );

			Assert.AreEqual( (left + 1, top + 1), (right, bottom), $"{piece}" );
			Assert.AreEqual( 25, world.CellAt( left, top ).TrackType, $"{piece} on ({left},{top})" );

			foreach ( var (x, y) in new[] { (right, top), (left, bottom), (right, bottom) } )
			{
				Assert.AreEqual( 12, world.CellAt( x, y ).TrackType, $"{piece} on ({x},{y})" );
				Assert.AreEqual( MapStep.CellId( left, top ), world.CellAt( x, y ).TrackParentId, $"{piece} on ({x},{y})" );
			}

			anchors.Add( (left, top) );
		}

		Assert.AreEqual( 143, anchors.Count );
	}

	/// <summary>
	/// A piece stands halfway between the lowest and the highest ground vertex of its two cells square (flags <c>0x33a</c>,
	/// the 2 by 2 footprint at <c>0x008179f8</c>, <c>FUN_00466d70</c>), which is not its cell's corner where the ground falls.
	/// </summary>
	[TestMethod]
	public void APieceStandsMidwayUpTheGroundUnderItsSquare()
	{
		var (world, catalogue) = Stock();
		using var terrain = GameData.Required().OpenRead( "levels/jungle/terrain/base.MD2" )!;
		var field = new HeightfieldFile( terrain );
		var square = new ParkItemCatalogue.Item( 17302, "ho", "", "", 2, 2, null );
		var sloped = 0;

		foreach ( var piece in ParkBoundaryHoarding.Read( world, catalogue ) )
		{
			var (left, top, _, _) = ParkObjects.FootprintOf( square, ParkBoundaryHoarding.StandOf( piece, null ).Origin,
				ParkBoundaryHoarding.StandOf( piece, null ).Turn );
			var heights = Enumerable.Range( 0, 9 ).Select( i => field.HeightAt( left + (i % 3), top + (i / 3) ) ).ToArray();
			var (origin, _) = ParkBoundaryHoarding.StandOf( piece, field );

			Assert.AreEqual( (heights.Min() + heights.Max()) * 0.5f, origin.Z, 0.001f, $"{piece}" );

			if ( heights.Min() != heights.Max() && MathF.Abs( origin.Z - field.HeightAt( piece.CellX, piece.CellY ) ) > 0.01f )
				++sloped;
		}

		Assert.IsTrue( sloped > 0, "no piece stands where its square's middle and its cell's corner differ" );
	}

	/// <summary>A park made fresh writes its anchors with no model record, so it makes no piece and every anchor is unmade.</summary>
	[TestMethod]
	public void AFreshParkMakesNoPieceOnItsAnchors()
	{
		var data = GameData.Required();
		FileSystem = data;
		var catalogue = new ParkItemCatalogue( "jungle", data );
		var fresh = new FreshPark( "jungle", new ParkBalance( "jungle", easyMode: false ), catalogue, data );

		var anchors = ParkBoundaryHoarding.AnchorsOf( fresh );

		Assert.IsTrue( anchors > 0 );
		Assert.AreEqual( 0, ParkBoundaryHoarding.Read( fresh, catalogue ).Count );

		// And the wall counts each one.
		var before = Counted( "BOUNDARY_HOARDING_UNMADE" );
		var wall = new ParkBoundaryHoarding( "jungle", fresh, catalogue );

		try
		{
			Assert.AreEqual( anchors, Counted( "BOUNDARY_HOARDING_UNMADE" ) - before );
			Assert.AreEqual( 0, wall.Pieces.Count );
		}
		finally
		{
			wall.Delete();
		}
	}

	private static int Counted( string gap ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == gap ).Times;
}
