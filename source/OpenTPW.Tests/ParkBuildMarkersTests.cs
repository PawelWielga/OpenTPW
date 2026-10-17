using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// The queue tool's squares - <see cref="ParkBuildMarkers"/>. No game files needed.
/// </summary>
[TestClass]
public class ParkBuildMarkersTests
{
	/// <summary>
	/// The wave the queue tool's squares ride on: <c>sin( phase + x + z )</c> in world units, one unit either
	/// way - the 4,096-entry table <c>FUN_004708d0</c> fills, read at <c>(phase + x + z) * 4096 / 2π</c>.
	/// </summary>
	[TestMethod]
	public void TheQueueSquaresWaveOneUnitAlongTheDiagonal()
	{
		Assert.AreEqual( 0f, ParkBuildMarkers.Wave( 0f, 0f, 0f ), 1e-5f, "flat at the origin with no phase" );
		Assert.AreEqual( 1f, ParkBuildMarkers.Wave( System.MathF.PI / 2f, 0f, 0f ), 1e-5f, "one unit at the crest" );
		Assert.AreEqual( ParkBuildMarkers.Wave( 0f, 10f, 20f ), ParkBuildMarkers.Wave( 0f, 20f, 10f ), 1e-5f,
			"the two ground axes count alike, so the crests run corner to corner" );
		Assert.AreEqual( System.MathF.Sin( 30f ), ParkBuildMarkers.Wave( 0f, 10f, 20f ), 1e-5f, "in world units, not cells" );
	}

	/// <summary>
	/// A square's record sits 1.5 over its lift: the original's records read 21.5 over the Belly Bounce's lift of 20,
	/// 11.5 over a lift of 10 and 1.5 on bare ground (<c>docs/exe/park-engine.md</c>, "Placement feedback"; Q433, Q435).
	/// </summary>
	[TestMethod]
	public void ASquaresRecordSitsOneAndAHalfOverItsLift()
	{
		Assert.AreEqual( 1.5f, ParkBuildMarkers.RecordHeight( 0f ), 1e-5f, "bare ground" );
		Assert.AreEqual( 11.5f, ParkBuildMarkers.RecordHeight( 10f ), 1e-5f, "a lift of ten" );
		Assert.AreEqual( 21.5f, ParkBuildMarkers.RecordHeight( 20f ), 1e-5f, "over the Belly Bounce" );
	}

	/// <summary>
	/// A lifted square's walls, as the original queued them on the stock park (<c>q435/orig/b-5-squares.txt</c>): (52,26),
	/// lifted 20 beside (51,26) at 20, (53,26) at 10 and (52,27) on the ground, with (52,25) as high, laid its square at
	/// 21.5 and then records at 11.5, 1.5 and 11.5 - one wall toward the east and two toward the south, lowest first.
	/// </summary>
	[TestMethod]
	public void ALiftedSquareIsWalledDownToEachLowerNeighbourTenAtATime()
	{
		var lifts = new System.Collections.Generic.Dictionary<(int, int), float>
		{
			[(52, 25)] = 20f, [(53, 26)] = 10f, [(52, 27)] = 0f, [(51, 26)] = 20f
		};

		var walls = ParkBuildMarkers.WallsAt( 52, 26, 20f, ParkPathBuilding.MarkerBlue, ( x, y ) => lifts[(x, y)] );

		CollectionAssert.AreEqual(
			new[] { ParkBuildMarkers.FaceEast, ParkBuildMarkers.FaceSouth, ParkBuildMarkers.FaceSouth },
			walls.Select( wall => wall.Face ).ToArray() );
		CollectionAssert.AreEqual( new[] { 11.5f, 1.5f, 11.5f }, walls.Select( wall => wall.Over ).ToArray(),
			"the original's records after the square's 21.5" );
		Assert.IsTrue( walls.All( wall => (wall.X, wall.Y, wall.Marker) == (52, 26, ParkPathBuilding.MarkerBlue) ), "the square's own cell, blue" );
		Assert.IsTrue( ParkBuildMarkers.WallsAt( 52, 26, 20f, ParkPathBuilding.MarkerRed, ( x, y ) => lifts[(x, y)] )
			.All( wall => wall.Marker == ParkPathBuilding.MarkerRed ), "red under a red square" );
		Assert.IsTrue( ParkBuildMarkers.WallsAt( 52, 26, 20f, ParkPathBuilding.MarkerLink, ( x, y ) => lifts[(x, y)] )
			.All( wall => wall.Marker == ParkPathBuilding.MarkerBlue ), "blue under m_link" );
	}

	/// <summary>The four neighbours are asked north, east, south, west, and a missing or higher one gets no wall.</summary>
	[TestMethod]
	public void TheWallsGoNorthEastSouthWestAndNoneOffTheMapOrUpward()
	{
		var lifts = new System.Collections.Generic.Dictionary<(int, int), float?>
		{
			[(5, 4)] = 0f, [(6, 5)] = 0f, [(5, 6)] = 0f, [(4, 5)] = 0f
		};

		CollectionAssert.AreEqual(
			new[] { ParkBuildMarkers.FaceNorth, ParkBuildMarkers.FaceEast, ParkBuildMarkers.FaceSouth, ParkBuildMarkers.FaceWest },
			ParkBuildMarkers.WallsAt( 5, 5, 10f, 0, ( x, y ) => lifts[(x, y)] ).Select( wall => wall.Face ).ToArray() );

		Assert.AreEqual( 0, ParkBuildMarkers.Walls( 20f, null, 20f, 30f, null ).Count, "off the map, as high, higher" );
		Assert.AreEqual( 0, ParkBuildMarkers.WallsAt( 5, 5, 0f, 0, ( _, _ ) => -10f ).Count, "a square on the ground has none" );
		Assert.AreEqual( 2, ParkBuildMarkers.Walls( 20f, 0f, null, null, null ).Count, "ten at a time, not one" );
	}

	/// <summary>Each wall stands on its own edge of the cell, from the corner the original lays its foot first.</summary>
	[TestMethod]
	public void EachWallStandsOnTheEdgeTowardItsNeighbour()
	{
		Assert.AreEqual( ((5, 5), (6, 5)), ParkBuildMarkers.Edge( 5, 5, ParkBuildMarkers.FaceNorth ) );
		Assert.AreEqual( ((6, 5), (6, 6)), ParkBuildMarkers.Edge( 5, 5, ParkBuildMarkers.FaceEast ) );
		Assert.AreEqual( ((6, 6), (5, 6)), ParkBuildMarkers.Edge( 5, 5, ParkBuildMarkers.FaceSouth ) );
		Assert.AreEqual( ((5, 6), (5, 5)), ParkBuildMarkers.Edge( 5, 5, ParkBuildMarkers.FaceWest ) );
		Assert.AreEqual( 10f, ParkBuildMarkers.WallHeight, "the -10.0 at 0x00700e1c" );
	}

	/// <summary>A wall's foot lies along its edge at the record's height and its top one wall higher; blinked off, it has no area.</summary>
	[TestMethod]
	public void AWallRisesTenFromItsRecordAlongItsEdge()
	{
		var corners = ParkBuildMarkers.WallCorners( new ParkBuildMarkers.Wall( 5, 5, ParkBuildMarkers.FaceEast, 0, 11.5f ), false );

		CollectionAssert.AreEqual( new[] { (6, 5, 11.5f), (6, 6, 11.5f), (6, 6, 21.5f), (6, 5, 21.5f) },
			corners.Select( corner => (corner.X, corner.Y, corner.Over) ).ToArray() );
		CollectionAssert.AreEqual( new[] { (0f, 0f), (1f, 0f), (1f, 1f), (0f, 1f) },
			corners.Select( corner => (corner.Uv.X, corner.Uv.Y) ).ToArray(), "u along the edge, v up it" );

		var gone = ParkBuildMarkers.WallCorners( new ParkBuildMarkers.Wall( 5, 5, ParkBuildMarkers.FaceEast, 1, 11.5f ), true );
		Assert.IsTrue( gone.All( corner => (corner.X, corner.Y, corner.Over) == (6, 5, 11.5f) ), "all on its first corner" );
	}

	/// <summary>A wall is red under a red square and blue under every other the tools draw.</summary>
	[TestMethod]
	public void AWallIsRedUnderRedAndBlueOtherwise()
	{
		Assert.AreEqual( ParkPathBuilding.MarkerRed, ParkBuildMarkers.WallMarker( ParkPathBuilding.MarkerRed ) );
		Assert.AreEqual( ParkPathBuilding.MarkerBlue, ParkBuildMarkers.WallMarker( ParkPathBuilding.MarkerBlue ) );
		Assert.AreEqual( ParkPathBuilding.MarkerBlue, ParkBuildMarkers.WallMarker( ParkPathBuilding.MarkerLink ) );
		Assert.AreEqual( ParkPathBuilding.MarkerBlue, ParkBuildMarkers.WallMarker( ParkPathBuilding.MarkerEnd ) );
	}

	/// <summary>
	/// <b>Nothing left to draw leaves no walls behind</b>: when the tool is put away the mesh goes, and the census that
	/// `strip` reads says "none" with no walls after it, as the original's list is emptied every frame.
	/// </summary>
	/// <remarks><b>Mutation:</b> the walls not cleared with the squares keep the last outline's walls in the census.</remarks>
	[TestMethod]
	public void NothingToDrawLeavesNoWallsInTheCensus()
	{
		var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
		var markers = (ParkBuildMarkers)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject( typeof( ParkBuildMarkers ) );
		var lifts = new System.Collections.Generic.Dictionary<(int, int), float> { [(52, 25)] = 20f, [(53, 26)] = 10f, [(52, 27)] = 0f, [(51, 26)] = 20f };
		var strip = typeof( ParkBuildMarkers ).GetField( "_strip", flags )!;
		strip.SetValue( markers, System.Activator.CreateInstance( strip.FieldType ) );
		typeof( ParkBuildMarkers ).GetField( "_walls", flags )!
			.SetValue( markers, ParkBuildMarkers.WallsAt( 52, 26, 20f, ParkPathBuilding.MarkerBlue, ( x, y ) => lifts[(x, y)] ) );
		StringAssert.Contains( markers.Census(), "| walls 3 " );

		typeof( ParkBuildMarkers ).GetMethod( "Build", flags )!.Invoke( markers,
			[new System.Collections.Generic.List<ParkPathBuilding.QueueSquare>(), new System.Collections.Generic.List<(int X, int Y)>(), new System.Collections.Generic.List<(int X, int Y)>()] );

		Assert.AreEqual( "none", markers.Census() );
	}
}
