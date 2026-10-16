using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OpenTPW.Tests;

/// <summary>
/// The map's developed cells - <c>docs/exe/advisor-park.md</c>, "A park too big for its handymen":
/// <see cref="ParkState.DevelopedCells"/>, the original's <c>FUN_004c81b0</c>, which the advisor's row 10 reads. Reads Lost
/// Kingdom's save, and is skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkDevelopedCellsTests
{
	private ParkWorld Park()
	{
		using var stream = new MemoryStream( (FileSystem = GameData.Required()).ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	/// <summary>
	/// <b>The shipped park has 126 developed cells</b>: 78 of path, 4 of queue, 35 of footprint, 8 of type 9 and one of type
	/// 10, of the map's 16,384 (6,875 bare, 240 of type 2, 9,077 of type 7, 66 of the approach).
	/// </summary>
	[TestMethod]
	public void TheShippedParkHas126DevelopedCells()
		=> Assert.AreEqual( 126, new ParkState( Park() ).DevelopedCells() );

	/// <summary>
	/// <b>A cell counts by its type alone, inside the park or out, anywhere on the map</b>: a bare (3,3) made path, queue,
	/// footprint, or either end of a ride adds one; made type 2, 5, 7 or the approach adds none; (0,0) and (127,127), the
	/// map's corners, count, and a cell flagged outside the park counts as one inside does.
	/// </summary>
	/// <remarks><b>Mutations:</b> a type left out or one added; the outside flag skipped; a row or a column of the map missed.</remarks>
	[TestMethod]
	public void ACellCountsByItsTypeAloneAnywhereOnTheMap()
	{
		var state = new ParkState( Park() );
		var bare = state.Record( 3, 3 );

		Assert.AreEqual( (0, 0), (bare.Type, bare.Flags & ParkPathBuilding.OutsideThePark), "(3,3) is bare and inside the park" );

		foreach ( var (type, adds) in new[] { (1, 1), (3, 1), (4, 1), (9, 1), (10, 1), (0, 0), (2, 0), (5, 0), (7, 0), (30, 0) } )
		{
			state.SetRecord( 3, 3, bare with { Type = type } );
			Assert.AreEqual( 126 + adds, state.DevelopedCells(), $"type {type}" );
		}

		state.SetRecord( 3, 3, bare with { Type = 1, Flags = (ushort)(bare.Flags | ParkPathBuilding.OutsideThePark) } );
		Assert.AreEqual( 127, state.DevelopedCells(), "outside the park the same" );

		state.SetRecord( 0, 0, state.Record( 0, 0 ) with { Type = 1 } );
		state.SetRecord( 127, 127, state.Record( 127, 127 ) with { Type = 1 } );
		Assert.AreEqual( 129, state.DevelopedCells(), "the map's two far corners" );
	}
}
