using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The park file's writer, a vehicle made at its summons (<c>docs/exe/saves.md</c>, "OpenTPW's writer, a vehicle
/// made at its summons"): the thing the summons makes is written whole, an object on the corner cell, and the
/// header names it in its size's slot and as the current vehicle.
/// </summary>
[TestClass]
public class ParkFileWriterMadeVehicleTests
{
	private const string Theme = "jungle";
	private const string ShippedPark = "levels/jungle/Easymode.TPWI";
	private const int FerryItem = 1604, Bus = 15, StoodFerry = 65531, StoodSeaplane = 65530;

	private BaseFileSystem data = null!;
	private BaseFileSystem oldSaves = null!;
	private ParkFixedItems? oldItems;
	private string? root;
	private ParkWorld shipped = null!;
	private ParkItemCatalogue catalogue = null!;
	private readonly List<Entity> made = [];

	private string Jungle => Path.Combine( root!, "users", "1Test", "jungle" );

	[TestInitialize]
	public void ReadTheShippedPark()
	{
		Log ??= new();
		data = FileSystem = GameData.Required();
		shipped = Read( data.ReadAllBytes( ShippedPark ), out _ );
		catalogue = new ParkItemCatalogue( Theme, data );

		oldSaves = SaveFileSystem;
		oldItems = ParkFixedItems.Current;
		root = Directory.CreateTempSubdirectory( "opentpw-made-vehicle-" ).FullName;
		Directory.CreateDirectory( Jungle );
		SaveFileSystem = new BaseFileSystem( root );
		Unimplemented.Forget();
	}

	[TestCleanup]
	public void PutAwayWhatWasMade()
	{
		foreach ( var entity in made )
			entity.Delete();

		made.Clear();
		Entity.ApplyDeletions();
		TestRun.DeleteEvery<ParkPeople>();

		// Without the game the set-up stops before anything is changed.
		if ( root != null )
		{
			typeof( Players ).GetProperty( nameof( Players.Current ) )!.SetValue( Players.Roster, null );
			typeof( ParkFixedItems ).GetProperty( nameof( ParkFixedItems.Current ) )!.SetValue( null, oldItems );
			SaveFileSystem = oldSaves;
			Directory.Delete( root, true );
		}

		ParkOrbitCameraMode.Forget();
		Unimplemented.Forget();
	}

	private static ParkWorld Read( byte[] file, out byte[] body )
	{
		using var stream = new MemoryStream( file );
		var reader = new SaveReader( stream );

		body = reader.ReadFile();

		return new ParkWorld( body, reader.Preamble );
	}

	private int ChannelsFor( int id ) => catalogue.TryGet( id, out var item ) ? item.AnimationChannels : 1;

	private static int Times( string gap ) => Unimplemented.Summary.Where( counted => counted.What == gap ).Sum( counted => counted.Times );

	/// <summary>The park's fixed items as a scene leaves them: the bus the file's thing, the two others under ids of the scene's own.</summary>
	private static void StandVehicles()
	{
		var items = (ParkFixedItems)RuntimeHelpers.GetUninitializedObject( typeof( ParkFixedItems ) );

		typeof( ParkFixedItems ).GetField( "_thingOf", BindingFlags.NonPublic | BindingFlags.Instance )!
			.SetValue( items, new Dictionary<string, int> { ["bus"] = Bus, ["seaplane"] = StoodSeaplane, ["ferry"] = StoodFerry } );

		typeof( ParkFixedItems ).GetProperty( nameof( ParkFixedItems.Current ) )!.SetValue( null, items );
	}

	/// <summary>One sweep of the park, through both clocks as a frame runs them.</summary>
	private static void Sweep( ParkPeople people )
	{
		var from = people.State.GameTick;

		for ( var frame = 0; frame < 10 && people.State.GameTick == from; ++frame )
		{
			Time.Update( 0.1f );
			GameClock.Update( paused: false, GameClock.ParkCatchUp );
			people.Update();
		}
	}

	/// <summary>
	/// The shipped park with its ferry stood and scripted under the scene's id, a load of sixty-one forced and swept
	/// until the ferry is summoned, and so made.
	/// </summary>
	private (ParkPeople People, ParkRides Rides, int Ferry) FerrySummoned()
	{
		typeof( Players ).GetProperty( nameof( Players.Current ) )!.SetValue( Players.Roster, new Player( 0, "Test", new PlayerFile { InstantAction = true } ) );

		var state = new ParkState( shipped );
		var rides = new ParkRides( Theme, shipped, catalogue, data );

		made.Add( rides );
		StandVehicles();

		Assert.IsTrue( catalogue.TryGet( FerryItem, out var item ), "the jungle's catalogue has the ferry" );
		Assert.IsTrue( rides.BindNew( ParkBuilding.Constructed( item, StoodFerry, 0, 0, 0, 1, 1, default ), item ), "the stood ferry runs its script" );

		var people = new ParkPeople( shipped, new ParkBalance( Theme, easyMode: true ), () => ParkRides.GateIsOpen, state, catalogue,
			thing => rides.Scheduler.Find( rides.ScriptFor( thing ) ), random: new Random( 5 ), behaviourRandom: new Random( 5 ),
			rideRandom: new Random( 5 ), staffRandom: new Random( 5 ) );

		Time.Paused = false;
		Time.StepFrames = 0;
		GameClock.Rebase();
		Time.Update( 0f );
		GameClock.Update( paused: false, GameClock.ParkCatchUp );

		Assert.AreEqual( 3, people.ForceArrival( 61 ), "sixty-one come by the third vehicle" );
		Sweep( people );
		Assert.AreEqual( 3, people.CurrentVehicle, "summoned on the sweep that finds none" );

		return (people, rides, people.VehicleThings[2]);
	}

	/// <summary>
	/// <b>The stood ferry's script goes under the thing the summons makes</b>, and nothing is left under the
	/// scene's id: the script's own thing is what its record in a park file names.
	/// </summary>
	/// <remarks><b>Mutations:</b> the script left under the scene's id; bound under both; its own thing left the scene's.</remarks>
	[TestMethod]
	public void TheStoodVehiclesScriptGoesUnderTheMadeThing()
	{
		var (_, rides, ferry) = FerrySummoned();

		Assert.AreEqual( 0, rides.ScriptFor( StoodFerry ), "nothing runs under the scene's id" );
		Assert.AreNotEqual( 0, rides.ScriptFor( ferry ), "the script runs under the made thing" );
		Assert.AreEqual( ferry, rides.Scheduler.Find( rides.ScriptFor( ferry ) )!.ThingId, "and names it" );
		Assert.AreEqual( ferry, ParkFixedItems.Current!.ThingFor( "ferry" ) );
		Assert.AreEqual( StoodSeaplane, ParkFixedItems.Current.ThingFor( "seaplane" ), "the seaplane, not summoned, is where it was" );
	}

	/// <summary>
	/// <b>A park written after the summons holds the ferry as the original's own files hold one</b>
	/// (<c>q257x/orig/fixed-by-the-original.TPWS</c>, thing 32): the header's third slot and its current vehicle
	/// name the thing; its record is an object of item 1604 at the middle of cell (0,0), facing nought, state 3,
	/// its corner and both ends cell 1, its door open, named "Ferry", with its script and its model; it heads the
	/// object list and the corner cell's, the bus behind it. Nothing is counted. Read back, the park has the
	/// ferry current on that thing and makes nothing at its next summons.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the slot left nought; the second slot written; the current vehicle left nought; the object
	/// left out of the file; its record on another cell; its ends nought; the cell's list left the bus's; the script
	/// or the model not written; the bus's slot lost; a load of the file making the vehicle again.
	/// </remarks>
	[TestMethod]
	public void AParkWrittenAfterTheSummonsHoldsTheVehicleAsAThing()
	{
		var (people, rides, ferry) = FerrySummoned();
		var state = people.State;
		var stamp = state.LastObjectBuilt;

		Unimplemented.Forget();
		Assert.IsNotNull( Level.WritePark( shipped, state, Theme, "Ferried", people, null, rides, catalogue ) );

		var written = Read( File.ReadAllBytes( Path.Combine( Jungle, "Ferried.TPWS" ) ), out var body );

		Assert.IsNull( written.Problem );
		Assert.AreEqual( (Bus, 0, ferry, ferry),
			(written.ArrivalVehicleForSmallCrowd, written.ArrivalVehicleForMediumCrowd, written.ArrivalVehicleForLargeCrowd, written.CurrentArrivalVehicle),
			"the bus's slot the file's, the second none, the third and the current vehicle the made thing" );

		foreach ( var gap in new[] { "SAVE_PARK_ARRIVAL_VEHICLE", "SAVE_PARK_OBJECT_BOUGHT", "SAVE_PARK_SCRIPT_MADE_SINCE_THE_LOAD", "SAVE_PARK_MADE_MODEL_TABLES", "VEHICLE_MADE_NO_ITEM" } )
			Assert.AreEqual( 0, Times( gap ), gap );

		// The record, as the original's own.
		var record = written.Objects.Single( thing => thing.ThingId == ferry );
		var handle = rides.ScriptFor( ferry );

		Assert.AreEqual( (FerryItem, 0, 0, 0, 0), (record.CatalogueId, record.CellX, record.CellY, record.Angle, (int)record.Flags) );
		Assert.AreEqual( (3, 1, 1, 1, 1), (record.State, record.CanLoad, (int)record.TopLeft, (int)record.EntryPos, (int)record.ExitPos) );
		Assert.AreEqual( (50, 100, 50, 100f, 100f), (record.QualityOfGoods, record.ChanceOfWinning, record.AmountOfSpecialIngredient, record.StateOfRepair, record.RemainingLife) );
		Assert.AreEqual( handle, record.RideScript, "its script's handle" );
		Assert.IsTrue( record.MeshInstance > 0, "and a model's" );

		// Its place and its name's first line, where the record lies: the middle of the corner cell, and "Ferry".
		var at = body.AsSpan().IndexOf( ParkWorld.MadeObjectRecord( new ParkWorld.MadeObject( record, "Ferry", "" ), record.MeshInstance ).AsSpan( 16, 44 ) ) - 16;

		Assert.IsTrue( at > 0, "the record's angle, item, date, model and flags lie together" );
		Assert.AreEqual( (0x0080, 0x0080), ((int)BinaryPrimitives.ReadUInt16LittleEndian( body.AsSpan( at + 8 ) ), (int)BinaryPrimitives.ReadUInt16LittleEndian( body.AsSpan( at + 10 ) )) );
		Assert.AreEqual( "Ferry", new string( [.. Enumerable.Range( 0, 5 ).Select( i => (char)BinaryPrimitives.ReadUInt16LittleEndian( body.AsSpan( at + 60 + (i * 4) ) ) )] ) );
		Assert.AreEqual( 0, BinaryPrimitives.ReadUInt16LittleEndian( body.AsSpan( at + 60 + (5 * 4) ) ), "ended" );

		// The lists: the newest object and the newest thing on the corner cell, the bus next on that cell.
		Assert.AreEqual( ferry, written.FirstObject );
		Assert.AreEqual( ferry, written.CellAt( 0, 0 ).Occupant );
		Assert.AreEqual( Bus, BinaryPrimitives.ReadUInt16LittleEndian( body.AsSpan( at + 12 ) ), "the thing behind it on its cell is the bus" );

		// Its script and its model, whose record is a fixed item's: on (48,17), one cell square, flags 0x361.
		Assert.IsNotNull( written.ScriptStates.For( handle ), "the script's record" );

		var model = written.ThingStates( ChannelsFor ).ForScript( handle );

		Assert.AreEqual( (FerryItem, record.MeshInstance - 1), (model?.CatalogueId, model?.Slot), "its model's, by that script, in the slot its record names" );
		Assert.IsTrue( model?.NodeWords?.Length > 0, "with its node words" );
		Assert.IsTrue( body.AsSpan().IndexOf( Dwords( FerryItem, 48, 17, 1, 1, 0x361, handle ) ) > 0, "item, cell, footprint, flags and script lie together" );

		// Read back: the ferry is current on that thing, and a summons makes nothing more.
		TestRun.DeleteEvery<ParkPeople>();

		var again = new ParkPeople( written, new ParkBalance( Theme, easyMode: true ), () => ParkRides.GateIsOpen, new ParkState( written ), catalogue );

		Assert.AreEqual( 3, again.CurrentVehicle );
		Assert.AreEqual( (Bus, 0, ferry), (again.VehicleThings[0], again.VehicleThings[1], again.VehicleThings[2]) );
		Assert.AreEqual( stamp, again.State.LastObjectBuilt, "the file's stamp is the summons's" );

		again.ForceArrival( 61 );
		Sweep( again );
		Sweep( again );

		Assert.AreEqual( ferry, again.VehicleThings[2], "the file's thing still" );
		Assert.AreEqual( stamp, again.State.LastObjectBuilt, "and nothing is stamped" );
		Assert.AreEqual( written.Objects.Count, again.State.Objects.Count, "nor made" );
	}

	/// <summary>
	/// <b>A vehicle's model record is a fixed item's</b> (<c>FUN_004dd500</c>): flags <c>0x361</c>, on cell (48,17)
	/// and one cell square whatever its object's cell and its item's footprint, as the bus's, the seaplane's and
	/// the ferry's records are in the original's own files; a thing bought keeps its own cell, footprint and
	/// <c>0x32f</c>.
	/// </summary>
	/// <remarks><b>Mutations:</b> a vehicle's record with a bought thing's flags, on its object's cell, of its item's footprint; a bought thing's a fixed item's.</remarks>
	[TestMethod]
	public void AVehiclesModelRecordIsAFixedItems()
	{
		var vehicle = ParkThingStates.MadeRecord( FerryItem, 0, 0, 3, 4, 18, 0, 0f, 0, [], vehicle: true );
		var bought = ParkThingStates.MadeRecord( FerryItem, 5, 6, 3, 4, 18, 0, 0f, 0, [] );

		CollectionAssert.AreEqual( Dwords( FerryItem, 48, 17, 1, 1, 0x361, 18 ), vehicle[1..29] );
		CollectionAssert.AreEqual( Dwords( FerryItem, 5, 6, 3, 4, 0x32f, 18 ), bought[1..29] );
		Assert.AreEqual( bought.Length, vehicle.Length );
		CollectionAssert.AreEqual( bought[29..], vehicle[29..], "the rest is the same" );
	}

	private static byte[] Dwords( params int[] values ) => [.. values.SelectMany( BitConverter.GetBytes )];

	/// <summary>
	/// <b>A park with no item for the vehicle makes nothing</b>: the summons is counted, no thing is made, nothing is
	/// stamped, and the file's slot stays nought.
	/// </summary>
	/// <remarks><b>Mutations:</b> a thing made of no item; the stamp moved all the same; nothing counted.</remarks>
	[TestMethod]
	public void WithNoItemForTheVehicleNothingIsMade()
	{
		var state = new ParkState( shipped );
		var people = new ParkPeople( shipped, new ParkBalance( Theme, easyMode: true ), () => ParkRides.GateIsOpen, state );
		var stamp = state.LastObjectBuilt;

		Time.Paused = false;
		Time.StepFrames = 0;
		GameClock.Rebase();
		Time.Update( 0f );
		GameClock.Update( paused: false, GameClock.ParkCatchUp );

		people.ForceArrival( 61 );
		Sweep( people );

		Assert.AreEqual( 3, people.CurrentVehicle );
		Assert.AreEqual( 0, people.VehicleThings[2] );
		Assert.AreEqual( stamp, state.LastObjectBuilt );
		Assert.AreEqual( shipped.Objects.Count, state.Objects.Count );
		Assert.AreEqual( 1, Times( "VEHICLE_MADE_NO_ITEM" ) );
	}
}
