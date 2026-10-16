using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The share of the map's walked cells that hold litter - <c>docs/exe/advisor-park.md</c>, "Litter and no handyman":
/// <see cref="ParkState.LitterShare"/>, the original's <c>FUN_004ccdf0</c>, which the advisor's rows 8 and 9 read. Reads
/// Lost Kingdom's save, and is skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkLitterShareTests
{
	[TestCleanup]
	public void LetThePeopleGo() => TestRun.DeleteEvery<ParkPeople>();

	private ParkWorld Park()
	{
		using var stream = new MemoryStream( (FileSystem = GameData.Required()).ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	private static List<(int X, int Y)> CellsOf( ParkState state, int type )
	{
		var cells = new List<(int, int)>();

		for ( var y = 0; y < ParkWorld.MapSize; ++y )
		{
			for ( var x = 0; x < ParkWorld.MapSize; ++x )
			{
				if ( state.Record( x, y ).Type == type )
					cells.Add( (x, y) );
			}
		}

		return cells;
	}

	/// <summary>
	/// <b>The shipped park's 90 walked cells</b> (78 of path, 4 of queue, 8 of type 9) hold no litter, so its share is
	/// nought; twelve of its path cells littered are 1,200 over 90, 13, and fourteen 15, cut towards nought.
	/// </summary>
	/// <remarks><b>Mutations:</b> the share rounded rather than cut; the littered not times 100; path, queue or type 9 left out
	/// of the walked; type 4 or 10 let in.</remarks>
	[TestMethod]
	public void TheShippedParksWalkedCellsAreNinetyAndTheShareIsCut()
	{
		var state = new ParkState( Park() );
		var path = CellsOf( state, 1 );

		Assert.AreEqual( (78, 4, 8), (path.Count, CellsOf( state, 3 ).Count, CellsOf( state, 9 ).Count) );
		Assert.AreEqual( 0, state.LitterShare(), "a park nobody has played holds no litter" );

		foreach ( var (x, y) in path.Take( 12 ) )
			state.CellAt( x, y ).Litter = 1;
		Assert.AreEqual( 13, state.LitterShare(), "1,200 over 90" );

		foreach ( var (x, y) in path.Skip( 12 ).Take( 2 ) )
			state.CellAt( x, y ).Litter = 7;
		Assert.AreEqual( 15, state.LitterShare(), "1,400 over 90 is 15.6, cut; any kind of litter counts" );

		foreach ( var (x, y) in path.Take( 14 ) )
			state.CellAt( x, y ).Litter = 0;
		var (qx, qy) = CellsOf( state, 3 ).First();
		state.CellAt( qx, qy ).Litter = 2;
		var (ex, ey) = CellsOf( state, 9 ).First();
		state.CellAt( ex, ey ).Litter = 2;
		Assert.AreEqual( 2, state.LitterShare(), "a queue cell and an entrance littered: 200 over 90, the same 90" );
	}

	/// <summary>
	/// <b>A littered cell counts whatever its type, and an unlittered one only as path, queue or an entrance</b>: litter
	/// on five type 7 cells makes them walked too (twelve path cells and those five are 1,700 over 95, 17, where over the
	/// 90 alone it would be 18); a footprint or a ride's far end with no litter adds nothing; the map's corners (0,0) and
	/// (127,127) count (1,900 over 97, 19).
	/// </summary>
	/// <remarks><b>Mutations:</b> the littered left out of the walked; a row or a column of the map missed; a footprint or a
	/// far end counted.</remarks>
	[TestMethod]
	public void ALitteredCellCountsWhateverItsType()
	{
		var state = new ParkState( Park() );

		foreach ( var (x, y) in CellsOf( state, 1 ).Take( 12 ) )
			state.CellAt( x, y ).Litter = 1;
		foreach ( var (gx, gy) in CellsOf( state, 7 ).Take( 5 ) )
			state.CellAt( gx, gy ).Litter = 3;
		Assert.AreEqual( 17, state.LitterShare(), "1,700 over 95" );

		var bare = state.Record( 3, 3 );
		Assert.AreEqual( 0, bare.Type, "(3,3) is bare" );
		foreach ( var type in new[] { 4, 10 } )
		{
			state.SetRecord( 3, 3, bare with { Type = type } );
			Assert.AreEqual( 17, state.LitterShare(), $"type {type} unlittered is not walked" );
		}

		state.SetRecord( 3, 3, bare );
		state.CellAt( 0, 0 ).Litter = 1;
		state.CellAt( 127, 127 ).Litter = 1;
		Assert.AreEqual( 19, state.LitterShare(), "1,900 over 97: the map's two far corners" );
	}

	/// <summary>
	/// <b>The park hands the advisor the share as its cells hold it at the ask</b>: nought, then 13 with twelve path cells
	/// littered, the same <see cref="ParkState"/>'s.
	/// </summary>
	/// <remarks><b>Mutations:</b> the share not handed in, or taken once and kept.</remarks>
	[TestMethod]
	public void TheParkHandsTheAdvisorItsLitterShare()
	{
		var world = Park();
		var state = new ParkState( world );
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );

		Assert.AreEqual( 0, people.AdvisorFacts().LitterShare );
		foreach ( var (x, y) in CellsOf( state, 1 ).Take( 12 ) )
			state.CellAt( x, y ).Litter = 1;
		Assert.AreEqual( 13, people.AdvisorFacts().LitterShare );
	}

	/// <summary><b>With no walked cell and no litter the share is nought</b>, where the original answers nought before it divides.</summary>
	[TestMethod]
	public void NoWalkedCellIsNought()
		=> Assert.AreEqual( 0, new ParkState( parkIsClosed: false, visitorsToDate: 0 ).LitterShare() );
}
