using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SVector3 = System.Numerics.Vector3;

namespace OpenTPW.Tests;

/// <summary>
/// The picker's object march (<see cref="ParkPickMarch"/>; <c>docs/exe/park-engine.md</c>, "The picker's object march").
/// The gate's figures are the original's own, read at the stock park with the camera at (475,155) 110 180
/// (<c>q494/orig/a-scan.log</c>): its eye at (475, 93.623, 212.747) and the point P its march wrote for each pixel, here
/// as our x, y, z (its x, z, height). The candidate a cell offers (the <c>.hmp</c> about the model's root, the box's
/// <c>BoxMin.Y</c>, the zero-opacity hide) is covered by the game run alone (<c>q494/</c>), not by a test here.
/// </summary>
[TestClass]
public class ParkPickMarchTests
{
	/// <summary>The original's eye at that view, in our axes.</summary>
	private static readonly SVector3 Eye = new( 475f, 212.747f, 93.623f );

	/// <summary>
	/// The gate's triangles as its file places them: <c>gates.MD2</c> carries the gate's place in the park in its own root.
	/// </summary>
	private static List<(SVector3 A, SVector3 B, SVector3 C, SVector3 Normal)> Gate()
	{
		var data = GameData.Required();
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/features/gates/gates.MD2" ) );
		var file = new ModelFile( stream );

		SVector3 Corner( int mesh, uint drawn )
		{
			var at = SVector3.Transform( file.Meshes[mesh].Vertices[drawn].Position.GetSystemVector3(), file.Meshes[mesh].WorldTransform );
			return new SVector3( at.X, at.Z, at.Y );
		}

		return ParkPickMarch.Triangles( file, _ => true, Corner ).ToList();
	}

	/// <summary>
	/// <b>The original's own rays meet the gate where its march met it.</b> Each ray, from its eye through its P, is
	/// handed to the walk: the first triangle met in the walk's order is the one the original answered, to a tenth.
	/// (489.7, 174.0, 41.7) is met only in the materials' order (the faces in the file's order meet (489.0, 175.8, 44.1)
	/// first); (469.9, 175.7, 44.0) only on each triangle's own plane (the file's face normals meet (469.7, 174.0, 41.8));
	/// (492.4, 170.5, 30.4) on neither.
	/// </summary>
	/// <remarks><b>Mutations:</b> the faces in the file's order; the node's own faces dropped.</remarks>
	[TestMethod]
	public void TheOriginalsOwnRaysMeetTheGateWhereItsMarchMet()
	{
		var gate = Gate();

		foreach ( var p in new SVector3[] { new( 489.7f, 174.0f, 41.7f ), new( 469.9f, 175.7f, 44.0f ), new( 492.4f, 170.5f, 30.4f ),
			new( 480.2f, 179.5f, 30.4f ), new( 494.2f, 179.2f, 15.7f ), new( 476.3f, 174.0f, 41.7f ) } )
		{
			var met = ParkPickMarch.MeetFirst( gate, Eye, SVector3.Normalize( p - Eye ) );

			Assert.IsNotNull( met, $"through {p}" );
			Assert.IsTrue( SVector3.Distance( met.Value, p ) < 0.15f, $"through {p}: met {met.Value}" );
		}
	}

	/// <summary>
	/// A triangle is met by the ray's LINE: a point inside it behind the eye counts, as <c>FUN_0045d090</c> never asks
	/// which side of the eye it is on; a point outside it does not.
	/// </summary>
	/// <remarks><b>Mutations:</b> the angle sum's threshold at 2π less a tenth.</remarks>
	[TestMethod]
	public void ATriangleIsMetByTheLineInsideIt()
	{
		var (a, b, c) = (new SVector3( 0, 0, 0 ), new SVector3( 10, 0, 0 ), new SVector3( 0, 10, 0 ));
		var up = SVector3.UnitZ;

		Assert.AreEqual( new SVector3( 2, 2, 0 ), ParkPickMarch.Meet( new SVector3( 2, 2, 5 ), -up, a, b, c, up ) );
		Assert.AreEqual( new SVector3( 2, 2, 0 ), ParkPickMarch.Meet( new SVector3( 2, 2, 5 ), up, a, b, c, up ), "behind the eye" );
		Assert.IsNull( ParkPickMarch.Meet( new SVector3( 8, 8, 5 ), -up, a, b, c, up ), "past the long side" );
		Assert.IsNull( ParkPickMarch.Meet( new SVector3( 5.05f, 5.05f, 5 ), -up, a, b, c, up ), "just past it: the angles sum to 6.243" );
		Assert.IsNotNull( ParkPickMarch.Meet( new SVector3( 4.99f, 4.99f, 5 ), -up, a, b, c, up ), "just inside it" );
	}

	/// <summary>The cells a march asks, in order, each answering nothing.</summary>
	private static List<(int X, int Y)> Asked( SVector3 from, SVector3 direction, SVector3? ground = null )
	{
		var asked = new List<(int, int)>();
		ParkPickMarch.March( from, direction, ground, ( x, y ) => { asked.Add( (x, y) ); return null; } );
		return asked;
	}

	/// <summary>
	/// <b>The march steps a whole cell along the longer ground axis from the eye's own cell</b>, and goes on while the ray
	/// stands at or above nought where it met no ground: from (5, 5, 50) falling 5 a step along y, the cells (0,0) to
	/// (0,10). Where it met ground, it goes on to one step's fall below the ground's point: ground at 20 stops it after
	/// (0,7), the step at 15.
	/// </summary>
	/// <remarks><b>Mutations:</b> the step a half cell; the shorter axis stepped; the loop's test after the step; the
	/// floor the ground's height without the fall.</remarks>
	[TestMethod]
	public void TheMarchStepsACellFromTheEyesCellUntilBelowTheGround()
	{
		var down = new SVector3( 0.01f, 1f, -0.5f );

		CollectionAssert.AreEqual( Enumerable.Range( 0, 11 ).Select( y => (0, y) ).ToList(), Asked( new SVector3( 5, 5, 50 ), down ) );
		CollectionAssert.AreEqual( Enumerable.Range( 0, 8 ).Select( y => (0, y) ).ToList(),
			Asked( new SVector3( 5, 5, 50 ), down, new SVector3( 5, 70, 20 ) ) );
		CollectionAssert.AreEqual( new List<(int, int)> { (2, 0), (1, 0), (0, 0) }, Asked( new SVector3( 25, 5, 10 ), new SVector3( -1f, 0.2f, -0.5f ) ).Take( 3 ).ToList(),
			"x the longer axis, stepped down" );
	}

	/// <summary>
	/// <b>A ray that crosses a cell edge on the other axis between two steps asks the cell it clips</b>, at the crossing
	/// taken 1.01 of the way: from (8, 5) moving 5 across a step, the cells (0,0), then (1,0) where x passes 10 at y 9,
	/// then (1,1). Moving the other way from (12, 5), x passes 10 at y 9 too, and the crossing taken exactly there would
	/// stand on the edge and ask (1,0) again; 1.01 of the way it asks (0,0).
	/// </summary>
	[TestMethod]
	public void ASidewaysCrossingAsksTheCellItClips()
	{
		CollectionAssert.AreEqual( new List<(int, int)> { (0, 0), (1, 0), (1, 1), (1, 2), (2, 2) },
			Asked( new SVector3( 8, 5, 50 ), new SVector3( 0.5f, 1f, -0.5f ) ).Take( 5 ).ToList() );
		CollectionAssert.AreEqual( new List<(int, int)> { (1, 0), (0, 0), (0, 1) },
			Asked( new SVector3( 12, 5, 50 ), new SVector3( -0.5f, 1f, -0.5f ) ).Take( 3 ).ToList() );
	}

	/// <summary>
	/// <b>A thing is tried only where the ray's height across its cell reaches it</b>: from (5, 2, 50) falling 5 a step,
	/// the ray crosses cell (0,0) from 51 where it enters (y 0) to 46 where it leaves (y 10), so a thing standing 45.9 high
	/// is not tried and one 46.1 high is; a floor at 51.1 is not tried and one at 50.9 is.
	/// </summary>
	/// <remarks><b>Mutations:</b> the point's own height for the whole cell; the cell's fraction counted from its far
	/// side.</remarks>
	[TestMethod]
	public void AThingIsTriedOnlyWhereTheRaysHeightReachesIt()
	{
		bool Tried( float top, float floor )
		{
			var tried = false;
			ParkPickMarch.March( new SVector3( 5, 2, 50 ), new SVector3( 0, 1, -0.5f ), null, ( x, y ) => (x, y) == (0, 0)
				? new ParkPickMarch.Candidate( 7, top, floor, ( _, _ ) => { tried = true; return null; } )
				: null );
			return tried;
		}

		Assert.IsFalse( Tried( 45.9f, 0f ) );
		Assert.IsTrue( Tried( 46.1f, 0f ) );
		Assert.IsFalse( Tried( 100f, 51.1f ) );
		Assert.IsTrue( Tried( 100f, 50.9f ) );
	}

	/// <summary>
	/// <b>The thing met is kept whichever point wins, and gives the point only where it is strictly nearer the eye than
	/// the ground's</b> (<c>0x0045cccf</c> stores the record before the nearer test, <c>0x0045ccf7</c>): the ground wins a
	/// tie, and with no ground the thing's point answers.
	/// </summary>
	/// <remarks><b>Mutations:</b> the thing dropped where the ground wins; the tie to the thing (<c>&lt;</c> to
	/// <c>&lt;=</c>); the ground ignored.</remarks>
	[TestMethod]
	public void TheThingMetIsKeptAndGivesThePointOnlyWhenNearer()
	{
		var from = new Vector3( 5, 5, 50 );
		var direction = new Vector3( 0, 1, -0.5f ).Normal;
		ParkPickMarch.Candidate? Meets( int x, int y ) => (x, y) == (0, 2)
			? new ParkPickMarch.Candidate( 9, 100f, -1f, ( _, _ ) => new SVector3( 5, 25, 40 ) )
			: null;

		Assert.AreEqual( (9, true), Pick( ParkPicking.March( from, direction, null, 95, 84, Meets ) ) );
		Assert.AreEqual( (9, true), Pick( ParkPicking.March( from, direction, new Vector3( 5, 26, 39.5f ), 95, 84, Meets ) ) );
		Assert.AreEqual( (9, false), Pick( ParkPicking.March( from, direction, new Vector3( 5, 25, 40 ), 95, 84, Meets ) ), "a tie: the ground's point, the thing kept" );
		Assert.AreEqual( (9, false), Pick( ParkPicking.March( from, direction, new Vector3( 5, 24, 41 ), 95, 84, Meets ) ), "the ground nearer" );
	}

	private static (int Thing, bool Nearer) Pick( (int Thing, Vector3 Point, bool Nearer)? met )
	{
		Assert.IsNotNull( met );
		return (met.Value.Thing, met.Value.Nearer);
	}

	/// <summary>
	/// <b>The cell the march asks is held to <c>MapInfo</c>'s width and height</b> (<c>0x0045cdd8</c>): a ray from past
	/// the last column asks column 94.
	/// </summary>
	[TestMethod]
	public void TheCellAskedIsHeldToTheMap()
	{
		var asked = new List<(int, int)>();
		ParkPicking.March( new Vector3( 990, 5, 50 ), new Vector3( 0, 1, -0.5f ).Normal, null, 95, 84,
			( x, y ) => { asked.Add( (x, y) ); return null; } );

		Assert.AreEqual( (94, 0), asked[0] );
	}

	/// <summary>
	/// <b>Which thing a cell answers</b> (<c>FUN_00527e80</c>): on the stock park every cell of the entrance's row from
	/// one before <c>EntranceA</c> (47,17) to one past <c>EntranceB</c> (48,17) answers the gate, the path cells where a
	/// guest stands as much as the gate's own; the next cell either side does not; a footprint cell of the Belly Bounce,
	/// (52,24), answers it by its parent; a cell of the gate's own type off the row, (46,16), answers nothing, and so does
	/// the Belly Bounce's queue cell (49,22), which names the ride as its parent but is of type 3.
	/// </summary>
	/// <remarks><b>Mutations:</b> the row's ends at the entrances themselves; the parent asked for any cell.</remarks>
	[TestMethod]
	public void TheEntrancesRowAnswersTheGate()
	{
		using var stream = new MemoryStream( (FileSystem = GameData.Required()).ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var state = new ParkState( new ParkWorld( new SaveReader( stream ).ReadFile() ) );
		int On( int x, int y ) => ParkItemHeights.ThingAnswering( state, (47, 17), (48, 17), 11, x, y );

		CollectionAssert.AreEqual( new[] { 0, 11, 11, 11, 11, 0 }, Enumerable.Range( 45, 6 ).Select( x => On( x, 17 ) ).ToArray() );
		Assert.AreEqual( 13, On( 52, 24 ), "the Belly Bounce's footprint" );
		Assert.AreEqual( 0, On( 46, 16 ), "the gate's type off the row" );
		Assert.AreEqual( 0, On( 49, 22 ), "a queue cell with a parent" );
	}
}
