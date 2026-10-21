using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Numerics = System.Numerics;

namespace OpenTPW.Tests;

/// <summary>
/// A coaster's highest track point, Global 0's measure - <see cref="ParkCoasterTrack"/>, <see cref="ParkCoasterPylon"/>
/// and <see cref="ParkCoasterHeights"/> (<c>docs/exe/ride-operation.md</c>, "A coaster's highest track point").
/// </summary>
/// <remarks>
/// The game ships no park with a coaster, so the Temple Of Gloom's 38 pieces are the ones Alexah's jungle <c>New Save</c>
/// holds (<c>parkfile.py -K</c>), stood on the shipped jungle ground with the shipped Minecart's own files; the numbers
/// asserted are the original's, read from its memory with that park loaded (<c>q706b/O1/full.txt</c>).
/// </remarks>
[TestClass]
public class ParkCoasterTrackTests
{
	private const string Minecart = "levels/jungle/rides/minecart";

	/// <summary>The Temple Of Gloom's pieces in the file's order: flags, cell, loft, bank, incline, yaw.</summary>
	private static readonly (int Flags, int X, int Y, float Loft, float Bank, float Incline, float Yaw)[] TempleOfGloom =
	[
		( 0x400, 26, 35, 0.09999999403953552f, 0.0f, -0.7168300747871399f, 1.5707963705062866f ),
		( 0x400, 29, 35, 0.9999998211860657f, 0.0f, -0.6028791069984436f, 1.5707963705062866f ),
		( 0x400, 32, 35, 0.9999998211860657f, -0.001859996933490038f, 0.5273827910423279f, 1.636208415031433f ),
		( 0x0, 37, 34, 0.009999990463256836f, 0.011096315458416939f, 0.5256524085998535f, 1.7311091423034668f ),
		( 0x0, 44, 33, 0.009999984875321388f, -0.19929911196231842f, -0.16539107263088226f, 2.1227593421936035f ),
		( 0x6, 48, 27, 0.25999999046325684f, 0.5406283736228943f, -0.11735115945339203f, 2.1942005157470703f ),
		( 0x6, 55, 25, 0.15999998152256012f, 1.157362699508667f, 0.34836146235466003f, 1.2427752017974854f ),
		( 0x6, 57, 28, 0.0f, 3.1414928436279297f, 0.148967444896698f, 0.2828045189380646f ),
		( 0x0, 57, 36, 0.0f, 0.26152172684669495f, -0.308637797832489f, -0.7344852089881897f ),
		( 0x0, 50, 36, 0.3699999749660492f, 0.0f, -0.16045336425304413f, -1.5707963705062866f ),
		( 0x0, 42, 36, 0.25999999046325684f, 0.0f, 0.3927750289440155f, -1.5707963705062866f ),
		( 0x0, 39, 36, 0.0f, 0.0f, 0.009462527930736542f, -1.5707963705062866f ),
		( 0x400, 36, 36, 0.25f, 0.0f, 0.0f, -1.5707963705062866f ),
		( 0x400, 33, 36, 0.0f, 0.0f, -0.009462527930736542f, -1.5707963705062866f ),
		( 0x400, 30, 36, 0.25999999046325684f, -0.05125555023550987f, -0.1171732246875763f, -1.4395655393600464f ),
		( 0x400, 26, 37, 0.0f, -0.11356727033853531f, 0.1503761112689972f, -1.0437666177749634f ),
		( 0x400, 21, 42, 0.054999999701976776f, -0.28525590896606445f, -0.22784890234470367f, -0.0037257822696119547f ),
		( 0x400, 24, 45, 0.10999999940395355f, -0.39635857939720154f, -0.2443021982908249f, 1.6749874353408813f ),
		( 0x400, 26, 42, 0.19499999284744263f, -0.3488209545612335f, -0.10423000901937485f, -2.9401674270629883f ),
		( 0x400, 23, 40, 0.22499997913837433f, -0.27655860781669617f, 0.023982541635632515f, -1.4726760387420654f ),
		( 0x400, 20, 43, 0.16499999165534973f, -0.24368542432785034f, 0.228028804063797f, -0.24882107973098755f ),
		( 0x400, 21, 46, 0.009999990463256836f, -0.259286493062973f, 0.18085065484046936f, 0.863943338394165f ),
		( 0x0, 26, 47, 0.009999984875321388f, -0.21669158339500427f, -0.13910920917987823f, 2.047205686569214f ),
		( 0x400, 29, 40, 0.3399999737739563f, -0.16632802784442902f, -0.11704082041978836f, -2.687201738357544f ),
		( 0x400, 22, 38, 0.2449999749660492f, -0.13717085123062134f, 0.1352119743824005f, -1.2239775657653809f ),
		( 0x400, 18, 44, 0.03499998152256012f, -0.11805621534585953f, 0.09506488591432571f, -0.2916187644004822f ),
		( 0x0, 18, 47, 0.0f, 0.42091235518455505f, 0.042002011090517044f, -0.6238080859184265f ),
		( 0x400, 15, 48, 0.0f, -0.3153851628303528f, 0.05297018587589264f, -0.785024106502533f ),
		( 0x400, 14, 51, 0.0f, 0.5069839358329773f, 0.09510775655508041f, -1.266157865524292f ),
		( 0x400, 10, 48, 0.0f, -0.4413507580757141f, 0.09823249280452728f, -1.4002759456634521f ),
		( 0x0, 8, 51, 0.0f, 0.5354068279266357f, 0.09963005036115646f, -1.542862892150879f ),
		( 0x0, 5, 47, 0.0f, 0.016281170770525932f, 0.05769294500350952f, -2.525843381881714f ),
		( 0x400, 3, 44, 0.0f, 0.5657631754875183f, -0.019515328109264374f, 2.9451539516448975f ),
		( 0x400, 6, 42, 0.0f, -0.5596330165863037f, -0.009795434772968292f, 3.043477773666382f ),
		( 0x400, 3, 39, 0.0f, 0.5849049091339111f, -0.009795434772968292f, 3.043477773666382f ),
		( 0x400, 6, 37, 0.0f, 0.0f, -0.02764235809445381f, 2.158798933029175f ),
		( 0x400, 9, 35, 0.0f, 0.1727573424577713f, 0.014485427178442478f, 1.8647394180297852f ),
		( 0x400, 14, 35, 0.0f, 0.0f, -0.02550358511507511f, 1.5707963705062866f ),
	];

	private static List<SavedCoasterPiece> Pieces()
		=> [.. TempleOfGloom.Select( p => new SavedCoasterPiece( p.Flags, p.X, p.Y, 0, p.Loft, p.Bank, p.Incline, p.Yaw, 0f ) )];

	private static HeightfieldFile JungleGround( BaseFileSystem data )
	{
		using var md2 = data.OpenRead( "levels/jungle/terrain/base.MD2" );

		return new HeightfieldFile( md2 );
	}

	/// <summary>
	/// The Temple Of Gloom's chain, its station's model standing where the park's object stands: cell (19, 34), turned
	/// nought, its model at (190, 340) at height 2.5 (<c>ParkObjects.ModelOriginFor</c> on that park).
	/// </summary>
	private static List<ParkCoasterTrack.Knot> TempleChain( BaseFileSystem data, ParkCoasterPylon pylon, HeightfieldFile field,
		float x = 190f )
	{
		var station = RideNodes.Load( Minecart, "minecart", data, false, [] )!;
		station.Place( new Vector3( x, 340f, 2.5f ), 0 );

		return ParkCoasterHeights.Chain( Pieces(), pylon, field, 2.5f,
			id => station.Find( id, ParkCoasterPylon.TrackSpace, out var at ) == NodeEnd.Posed ? at : null )!;
	}

	/// <summary>
	/// <b>The Temple Of Gloom peaks at 106.0722</b>, the original's <c>+0x110</c> with this park loaded, at sample 61 between
	/// the two tallest pylons, the curve 6.07 above them; Global 0 reads it as 106, the jungle's record in Alexah's profile.
	/// The chain is the station's three, the 38 pieces and the closing node, 42 in all, as the original's.
	/// </summary>
	[TestMethod]
	public void TheTempleOfGloomsTrackPeaksAt106()
	{
		var data = GameData.Required();
		var pylon = ParkCoasterPylon.Load( Minecart, data )!;
		var chain = TempleChain( data, pylon, JungleGround( data ) );
		var samples = ParkCoasterTrack.Samples( chain, ParkCoasterHeights.Curviness( Minecart, data )!.Value );
		var highest = ParkCoasterTrack.Highest( samples );

		Assert.AreEqual( 42, chain.Count );
		Assert.AreEqual( 106.0722f, highest, 0.0005f );
		Assert.AreEqual( 61, samples.FindIndex( sample => sample.Y == highest ) );
		Assert.AreEqual( 106, ParkCoasterTrack.Best( [highest] ) );

		// The original's 719 are these 718 and one past the closing knot, which the measure leaves out.
		Assert.AreEqual( 718, samples.Count );
	}

	/// <summary>
	/// The station's three nodes: the coaster model's track nodes 1 to 3, the first and third on pylons on the cells under
	/// them at the second's height, 12.042 over the model's root (loft 0.0227); the closing node at the first's place.
	/// </summary>
	[TestMethod]
	public void TheStationIsTheCoasterModelsTrackNodes()
	{
		var data = GameData.Required();
		var pylon = ParkCoasterPylon.Load( Minecart, data )!;
		var chain = TempleChain( data, pylon, JungleGround( data ) );

		AssertNear( new( 185f, 14.542f, 355f ), chain[0].Place );
		AssertNear( new( 210f, 14.542f, 355f ), chain[1].Place );
		AssertNear( new( 235f, 14.542f, 355f ), chain[2].Place );
		AssertNear( chain[0].Place, chain[^1].Place );
		Assert.AreEqual( 0.0227f, pylon.LoftFor( 14.542f - 2.5f ), 0.0001f );
	}

	/// <summary>
	/// <b>A station's end stands on the middle of the cell under its track node</b> (<c>trunc( x × 0.1 )</c>), its middle
	/// node where the track node is: the station two units along, its ends stay at 185 and 235 and its middle goes to 212.
	/// </summary>
	[TestMethod]
	public void AStationsEndsStandOnTheMiddleOfTheirCells()
	{
		var data = GameData.Required();
		var chain = TempleChain( data, ParkCoasterPylon.Load( Minecart, data )!, JungleGround( data ), 192f );

		AssertNear( new( 185f, 14.542f, 355f ), chain[0].Place );
		AssertNear( new( 212f, 14.542f, 355f ), chain[1].Place );
		AssertNear( new( 235f, 14.542f, 355f ), chain[2].Place );
	}

	/// <summary>
	/// <b>A pylon stands at the midpoint of its cell's lowest and highest corner</b> (<c>FUN_00466d70</c>): the first piece's
	/// cell (26, 35) has its near corner at 2.0 and stands at 2.5, so its knot is at 21.5, as the original's node 3 is.
	/// </summary>
	[TestMethod]
	public void APylonStandsAtTheMidpointOfItsCellsCorners()
	{
		var data = GameData.Required();
		var field = JungleGround( data );
		var chain = TempleChain( data, ParkCoasterPylon.Load( Minecart, data )!, field );

		Assert.AreEqual( 2.0f, field.HeightAt( 26, 35 ), 0.0001f, "the near corner, which the measure does not take" );
		Assert.AreEqual( 2.5f, ParkCoasterHeights.GroundAt( field, 26, 35 ), 0.0001f );
		AssertNear( new( 265f, 21.5f, 355f ), chain[3].Place );
	}

	/// <summary>
	/// <b>A piece's knot is pitched by its pylon's incline clip and turned by its rotate clip</b>, each blended on the
	/// engine's sine table, so the original's pitch and heading are whole multiples of π/1024: the second piece (incline
	/// -0.6029) pitches 246 of them, where the straight fit -1.25 × incline gives 0.7536; the third (yaw 1.6362) heads 533 of
	/// them.
	/// </summary>
	[TestMethod]
	public void APiecesFacingIsItsPylonsClipsOnTheEnginesTable()
	{
		var pylon = ParkCoasterPylon.Load( Minecart, GameData.Required() )!;
		var climbing = pylon.DirectionAt( 1.5707964f, -0.6028791f );
		var turning = pylon.DirectionAt( 1.6362084f, 0.5273828f );

		Assert.AreEqual( 246 * MathF.PI / 1024, MathF.Asin( climbing.Y ), 2e-5f );
		Assert.AreEqual( 533 * MathF.PI / 1024, MathF.Atan2( turning.X, turning.Z ), 2e-5f );
		Assert.AreEqual( -215 * MathF.PI / 1024, MathF.Asin( turning.Y ), 2e-5f );
	}

	/// <summary>
	/// A pylon type's knots come from its model's records: the Minecart's and the Gorilla Thrilla's one knot each, 10 to 100
	/// and 15.56 to 95.56 high; Chak Atak's three a node, not built, so it gives no height.
	/// </summary>
	[TestMethod]
	public void APylonTypesKnotsComeFromItsModel()
	{
		var data = GameData.Required();
		var minecart = ParkCoasterPylon.Load( Minecart, data )!;
		var flume = ParkCoasterPylon.Load( "levels/jungle/rides/coaster1", data )!;
		var gorilla = ParkCoasterPylon.Load( "levels/jungle/rides/coaster3", data )!;

		Assert.IsTrue( minecart.Posable );
		Assert.AreEqual( (10f, 100f), (minecart.Lowest, minecart.Highest) );
		Assert.AreEqual( 3, flume.Knots );
		Assert.IsFalse( flume.Posable );
		Assert.IsTrue( gorilla.Posable );
		Assert.AreEqual( 15.558f, gorilla.Lowest, 0.001f );
		Assert.AreEqual( 95.557f, gorilla.Highest, 0.001f );
		Assert.AreEqual( 15.558f, gorilla.MinHeight, 0.001f, "fMinHeight 10, raised to the loft-0 height (0x004309a4)" );
		Assert.AreEqual( 10f, minecart.MinHeight );
		Assert.AreEqual( 1.25f, ParkCoasterHeights.Curviness( Minecart, data ) );
	}

	/// <summary>
	/// <b>The loft that stands a knot at a height holds the height to the type's range and its share to 0 and 1</b>
	/// (<c>Coaster_LoftForHeight</c> <c>0x00433790</c>): the Minecart's 5 and 150 give 0 and 1, 55 gives a half; the Gorilla
	/// Thrilla's 150 gives 1, not 1.056 (its share's own hold: its fMaxHeight 100 is above its highest knot 95.557); a type whose
	/// fMaxHeight is 50 over knots 10 to 100 gives 80 the share of 50, 0.444, not 0.778 (the height's own hold; no shipped file
	/// has such a type, so the rule is given its numbers).
	/// </summary>
	/// <remarks><b>Mutations:</b> both holds removed; the share's hold alone (the Gorilla's 150); the height's hold alone (the 50 type).</remarks>
	[TestMethod]
	public void TheLoftForAHeightIsHeldToTheTypesRangeAndToNoughtAndOne()
	{
		var data = GameData.Required();
		var minecart = ParkCoasterPylon.Load( Minecart, data )!;
		var gorilla = ParkCoasterPylon.Load( "levels/jungle/rides/coaster3", data )!;

		Assert.AreEqual( 0f, minecart.LoftFor( 5f ) );
		Assert.AreEqual( 1f, minecart.LoftFor( 150f ) );
		Assert.AreEqual( 0.5f, minecart.LoftFor( 55f ), 1e-5f );
		Assert.AreEqual( 0f, gorilla.LoftFor( 5f ) );
		Assert.AreEqual( 1f, gorilla.LoftFor( 150f ) );

		Assert.AreEqual( 40f / 90f, ParkCoasterPylon.LoftShare( 80f, 10f, 50f, 10f, 100f ), 1e-5f );
		Assert.AreEqual( 0f, ParkCoasterPylon.LoftShare( 30f, 10f, 50f, 40f, 40f ), "no range" );
	}

	/// <summary>The measure leaves out the first sample and starts from nought, and takes the last.</summary>
	[TestMethod]
	public void TheHighestLeavesOutTheFirstSampleAndStartsFromNought()
	{
		Assert.AreEqual( 0f, ParkCoasterTrack.Highest( [new( 0, 50, 0 ), new( 0, -3, 0 ), new( 0, -1, 0 )] ) );
		Assert.AreEqual( 7f, ParkCoasterTrack.Highest( [new( 0, 1, 0 ), new( 0, 2, 0 ), new( 0, 7, 0 )] ) );
	}

	/// <summary>Global 0 takes the highest coaster, truncated, and nought with none.</summary>
	[TestMethod]
	public void TheBestCoasterIsTruncated()
	{
		Assert.AreEqual( 0, ParkCoasterTrack.Best( [] ) );
		Assert.AreEqual( 106, ParkCoasterTrack.Best( [106.9f] ) );
		Assert.AreEqual( 106, ParkCoasterTrack.Best( [3.9f, 106.2f, 50f] ) );
	}

	/// <summary>
	/// The coasters module's pieces are read after the third node's crossing partners, each piece's own partners skipped:
	/// the fields at their offsets, unaligned (FileFormats <c>saves.md</c>, "The coasters module").
	/// </summary>
	[TestMethod]
	public void ASavedCoastersPiecesAreRead()
	{
		var header = new byte[32];
		BitConverter.GetBytes( 0x101 ).CopyTo( header, 0 );
		BitConverter.GetBytes( (short)330 ).CopyTo( header, 0x0a );
		BitConverter.GetBytes( (short)2 ).CopyTo( header, 0x16 );

		static byte[] Piece( int flags, int x, int y, float loft, float incline, float yaw, params short[] partners )
		{
			var piece = new byte[0x22];
			BitConverter.GetBytes( flags ).CopyTo( piece, 0 );
			BitConverter.GetBytes( (short)x ).CopyTo( piece, 0x08 );
			BitConverter.GetBytes( (short)y ).CopyTo( piece, 0x0a );
			BitConverter.GetBytes( loft ).CopyTo( piece, 0x0d );
			BitConverter.GetBytes( incline ).CopyTo( piece, 0x15 );
			BitConverter.GetBytes( yaw ).CopyTo( piece, 0x19 );
			piece[0x21] = (byte)partners.Length;

			return [.. piece, .. partners.SelectMany( BitConverter.GetBytes )];
		}

		byte[] module = [.. Encoding.ASCII.GetBytes( "EMAK" ), .. new[] { 1, 1, 0, 2 }.SelectMany( BitConverter.GetBytes ), .. header,
			.. BitConverter.GetBytes( 1 ), .. BitConverter.GetBytes( (short)7 ),
			.. Piece( 0x400, 26, 35, 0.1f, -0.7168f, 1.5708f, 3, 4 ), .. Piece( 0x6, 29, 35, 1f, 0.25f, -1.5708f ),
			.. new byte[16], .. Encoding.ASCII.GetBytes( "SAOC" )];

		var pieces = new ParkCoasters( module ).FirstPieces;

		Assert.AreEqual( 2, pieces.Count );
		Assert.AreEqual( new SavedCoasterPiece( 0x400, 26, 35, 0, 0.1f, 0f, -0.7168f, 1.5708f, 0f ), pieces[0] );
		Assert.AreEqual( new SavedCoasterPiece( 0x6, 29, 35, 0, 1f, 0f, 0.25f, -1.5708f, 0f ), pieces[1] );
	}

	private static void AssertNear( Numerics.Vector3 expected, Numerics.Vector3 actual )
		=> Assert.IsTrue( Numerics.Vector3.Distance( expected, actual ) < 0.001f, $"expected {expected}, was {actual}" );
}
