using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Closing and opening rides, and what a closed ride does to its queue - the park's door, the close and open and
/// their guard, the closed ride's completion on its own turn (<c>FUN_004e0450</c>), and the reopen at the end of a
/// queue measured again. See <c>docs/exe/ride-operation.md</c>, "The closed ride".
///
/// <para>
/// Every park here is Lost Kingdom's own, with a Belly Bounce script standing in for every visitable object's:
/// each declares <c>VAR_RIDECLOSED</c> and <c>VAR_LETMEON</c> by name. The scripts are not run.
/// </para>
/// <para>
/// These read real game files and are skipped where there is no installation - see <see cref="GameData"/>.
/// </para>
/// </summary>
[TestClass]
public class ParkClosedRideTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame() => FileSystem = data = GameData.Required();

	private const string Theme = "jungle";

	private const int BellyBounce = 13;

	private const float Before = 50f;

	/// <summary>A head put out by a closed ride: <c>MediumHappinessChange</c>, 15, in <c>FUN_005012f0</c>.</summary>
	private const float TurnedAway = 35f;

	/// <summary>The Belly Bounce's back of queue as shipped, (49,22): <c>mNeighbours</c> 0x44, <c>mDirection</c> 0x04.</summary>
	private const int BackX = 49, BackY = 22;

	private sealed record Park( ParkWorld World, ParkState State, ParkPeople People,
		Dictionary<int, RideScript> Scripts, ParkItemCatalogue Catalogue );

	private Park Open()
	{
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );

		var state = new ParkState( world );
		var catalogue = new ParkItemCatalogue( Theme, data );
		var scripts = state.Objects.Where( thing => thing.IsVisitable )
			.ToDictionary( thing => thing.ThingId, _ => Script() );

		var people = new ParkPeople( world, new ParkBalance( Theme, easyMode: true ), gateStatus: null, state,
			catalogue, id => scripts.GetValueOrDefault( id ) );

		return new( world, state, people, scripts, catalogue );
	}

	private static void Close( Park park )
	{
		park.People.Delete();
		Entity.ApplyDeletions();
	}

	private RideScript Script()
	{
		using var stream = data.OpenRead( "levels/jungle/rides/bouncy/bouncy.RSE" );

		return new RideScript( new RideScriptFile( stream ) );
	}

	private static ParkWorld.CatalogueObject Thing( Park park, int id )
		=> park.State.Objects.Single( thing => thing.ThingId == id );

	private static ParkWorld.CatalogueObject[] Visitable( Park park )
		=> park.State.Objects.Where( thing => thing.IsVisitable ).ToArray();

	private static void Change( Park park, int id, Func<ParkWorld.CatalogueObject, ParkWorld.CatalogueObject> how )
		=> Assert.IsTrue( park.State.ReplaceObject( how( Thing( park, id ) ) ), $"thing {id} is in the park" );

	/// <summary>Puts the park's first <paramref name="count"/> guests in the Belly Bounce's queue, in order.</summary>
	private static Peep[] Queue( Park park, int count )
	{
		var queued = park.People.Peeps.Take( count ).ToArray();

		Assert.AreEqual( count, queued.Length, "the save has enough guests" );

		foreach ( var peep in queued )
		{
			peep.MajorDest = BellyBounce;
			peep.Happiness = Before;
			peep.SetState( PeepState.InQueue, tick: 1, new Random( 1 ) );
			peep.QueuePos = park.State.JoinQueue( BellyBounce, peep.ThingId );
		}

		return queued;
	}

	private static void AssertTurnedAway( Park park, Peep peep, string why )
	{
		Assert.AreEqual( PeepState.Deciding, peep.State, $"guest {peep.ThingId} thinks again: {why}" );
		Assert.AreEqual( 0, peep.MajorDest, $"guest {peep.ThingId} names the ride no longer: {why}" );
		Assert.AreEqual( TurnedAway, peep.Happiness, 0.001f, $"guest {peep.ThingId} loses MediumHappinessChange: {why}" );
		Assert.AreEqual( -1, park.State.PositionInQueue( BellyBounce, peep.ThingId ), $"guest {peep.ThingId} is out: {why}" );
		Assert.AreEqual( 0, park.State.NextInQueue( peep.ThingId ), $"guest {peep.ThingId} links forward to nobody" );
		Assert.AreEqual( 0, park.State.PreviousInQueue( peep.ThingId ), $"guest {peep.ThingId} links back to nobody" );
	}

	private static void AssertStillQueueing( Park park, Peep peep, string why )
	{
		Assert.AreEqual( BellyBounce, peep.MajorDest, $"guest {peep.ThingId} still names the ride: {why}" );
		Assert.AreEqual( Before, peep.Happiness, 0.001f, $"guest {peep.ThingId} loses nothing: {why}" );
		Assert.IsTrue( park.State.PositionInQueue( BellyBounce, peep.ThingId ) >= 0, $"guest {peep.ThingId} still queues: {why}" );
	}

	/// <summary>One turn of the closed ride's completion, as the ride's own turn runs it.</summary>
	private static void OneTurn( Park park )
		=> park.People.CompleteOrTurnAway( new ParkRideOperation( park.State, park.People.Guests ),
			park.Scripts[BellyBounce], Thing( park, BellyBounce ), thingTick: 10 );

	/// <summary>
	/// The door's close arm closes every object a guest may be offered, and nothing else: <c>mCanLoad</c>
	/// nought, the nominee let go of, <c>VAR_RIDECLOSED</c> set (<c>FUN_004df300</c> at <c>0x0051a1ae</c>).
	/// </summary>
	[TestMethod]
	public void ClosingTheParkClosesEveryObjectAGuestMayBeOffered()
	{
		var park = Open();

		try
		{
			var visitable = Visitable( park );
			var others = park.State.Objects.Where( thing => !thing.IsVisitable ).ToArray();

			Assert.AreEqual( 6, visitable.Length, "three toilets, the Drinks Shop, the Jungle Spray and the Belly Bounce" );
			Assert.IsTrue( park.State.Objects.All( thing => thing.CanLoad == 1 ), "the shipped park is saved with every object open" );

			park.State.NominateForLoading( BellyBounce, park.People.Peeps[0].ThingId );

			park.State.SetParkClosed( true );

			Assert.IsTrue( park.State.ParkIsClosed, "the park is shut" );

			foreach ( var thing in visitable )
			{
				Assert.AreEqual( 0, Thing( park, thing.ThingId ).CanLoad, $"thing {thing.ThingId} is closed" );
				Assert.AreEqual( 1, park.Scripts[thing.ThingId][ParkRideOperation.ClosedVariable],
					$"thing {thing.ThingId}'s script is told" );
				Assert.AreEqual( thing.State, Thing( park, thing.ThingId ).State, $"thing {thing.ThingId}'s state is left alone" );
			}

			Assert.AreEqual( 0, park.State.PersonBeingLoaded( BellyBounce ), "the nominee is let go of" );

			foreach ( var thing in others )
				Assert.AreEqual( thing.CanLoad, Thing( park, thing.ThingId ).CanLoad, $"thing {thing.ThingId} is not a ride anybody visits" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The door's open arm opens again every object its guard allows (<c>0x0051a013</c>..<c>0x0051a01e</c>),
	/// and in the shipped park that is all six: every back of queue and every entrance is joined to path.
	/// </summary>
	[TestMethod]
	public void OpeningTheParkOpensEveryObjectItClosed()
	{
		var park = Open();

		try
		{
			foreach ( var thing in Visitable( park ) )
				Assert.IsTrue( ParkRideOperation.MayOpen( park.World, thing, trackType: 0 ),
					$"thing {thing.ThingId} passes the guard as shipped" );

			park.State.SetParkClosed( true );
			park.State.SetParkClosed( false );

			Assert.IsFalse( park.State.ParkIsClosed, "the park is open" );

			foreach ( var thing in Visitable( park ) )
			{
				Assert.AreEqual( 1, thing.CanLoad, $"thing {thing.ThingId} is open again" );
				Assert.AreEqual( 0, thing.State, $"thing {thing.ThingId} is operating" );
				Assert.AreEqual( 0, park.Scripts[thing.ThingId][ParkRideOperation.ClosedVariable],
					$"thing {thing.ThingId}'s script is told" );
			}
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// Each arm acts only on a change (<c>0x00519f76</c>, <c>0x0051a09e</c>): shutting a shut park closes
	/// nothing, and opening an open one opens nothing.
	/// </summary>
	[TestMethod]
	public void TheDoorActsOnlyWhenItMoves()
	{
		var park = Open();

		try
		{
			var operation = new ParkRideOperation( park.State, park.People.Guests );

			operation.Close( park.Scripts[BellyBounce], BellyBounce );
			park.State.SetParkClosed( false );

			Assert.AreEqual( 0, Thing( park, BellyBounce ).CanLoad, "an open park's door opens nothing" );

			park.State.SetParkClosed( true );
			operation.Open( park.Scripts[BellyBounce], BellyBounce );
			park.State.SetParkClosed( true );

			Assert.AreEqual( 1, Thing( park, BellyBounce ).CanLoad, "a shut park's door closes nothing" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The guard refuses a ride with a mechanic called (<c>mRequestedService</c>), out of service (1, 2 or 4),
	/// or whose queue joins nothing, and the door leaves each closed.
	/// </summary>
	[TestMethod]
	public void TheDoorLeavesClosedWhatItsGuardRefuses()
	{
		var refusals = new (string Why, Action<Park> Make)[]
		{
			("a mechanic called", park => Change( park, BellyBounce, thing => thing with { RequestedService = 1 } )),
			("broken down", park => Change( park, BellyBounce, thing => thing with { State = 1 } )),
			("waiting for an upgrade", park => Change( park, BellyBounce, thing => thing with { State = 2 } )),
			("condemned", park => Change( park, BellyBounce, thing => thing with { State = 4 } )),
			("its back of queue joined to nothing", park => park.State.SetRecord( BackX, BackY,
				park.State.Record( BackX, BackY ) with { Neighbours = 0x04 } )),
		};

		foreach ( var (why, make) in refusals )
		{
			var park = Open();

			try
			{
				park.State.SetParkClosed( true );
				make( park );
				park.State.SetParkClosed( false );

				Assert.AreEqual( 0, Thing( park, BellyBounce ).CanLoad, $"{why}: the Belly Bounce stays closed" );
				Assert.AreEqual( 1, park.Scripts[BellyBounce][ParkRideOperation.ClosedVariable], $"{why}: and its script is not told" );
				Assert.IsTrue( Visitable( park ).Where( thing => thing.ThingId != BellyBounce ).All( thing => thing.CanLoad == 1 ),
					$"{why}: the other five open" );
			}
			finally
			{
				Close( park );
			}
		}
	}

	/// <summary>
	/// <c>FUN_004de4a0</c>. A thing with a queue path asks its back cell, which as shipped links on to the path
	/// behind it (0x44 against 0x04); any other thing asks the cell one step out of its entrance, which must be
	/// path linked both ways.
	/// </summary>
	[TestMethod]
	public void TheBackOfAQueueIsConnectedWhenItJoinsSomething()
	{
		var park = Open();

		try
		{
			var bounce = Thing( park, BellyBounce );

			Assert.IsTrue( bounce.HasQueuePath, "the Belly Bounce's queue is laid on the ground" );
			Assert.IsTrue( ParkRideOperation.BackOfQueueConnected( park.World, bounce ), "joined as shipped" );

			park.State.SetRecord( BackX, BackY, park.State.Record( BackX, BackY ) with { Neighbours = 0x04 } );

			Assert.IsFalse( ParkRideOperation.BackOfQueueConnected( park.World, bounce ),
				"a back cell linked only to the cell ahead joins nothing" );

			var entered = Visitable( park ).Where( thing => !thing.HasQueuePath ).ToArray();

			Assert.IsTrue( entered.Length > 0, "some visitable thing has no queue path" );

			foreach ( var thing in entered )
			{
				Assert.IsTrue( ParkRideOperation.BackOfQueueConnected( park.World, thing ), $"thing {thing.ThingId} joined as shipped" );

				var facing = thing.Angle switch { 0 => 0x10, 90 => 0x04, 180 => 0x01, _ => 0x40 };
				var (x, y) = MapStep.CellAt( thing.EntryPos );
				var (bx, by) = ParkBuilding.Step( x, y, CellEdge.Opposite( facing ) );
				var beyond = park.State.Record( bx, by );

				park.State.SetRecord( bx, by, beyond with { Type = 0 } );

				Assert.IsFalse( ParkRideOperation.BackOfQueueConnected( park.World, thing ),
					$"thing {thing.ThingId}: with no path before its entrance it joins nothing" );

				park.State.SetRecord( bx, by, beyond );
			}
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// A closed ride's turn puts its head out of the queue, one a turn (<c>0x004e0554</c>): off the queue, back
	/// to deciding, <c>MediumHappinessChange</c> off. Whoever was second is the head for the next turn.
	/// </summary>
	[TestMethod]
	public void AClosedRidePutsOutItsHeadOneATurn()
	{
		var park = Open();

		try
		{
			var queued = Queue( park, 3 );

			park.State.SetParkClosed( true );

			OneTurn( park );

			AssertTurnedAway( park, queued[0], "the head, on the first turn" );
			AssertStillQueueing( park, queued[1], "second, on the first turn" );
			AssertStillQueueing( park, queued[2], "third, on the first turn" );
			Assert.AreEqual( queued[1].ThingId, park.State.FirstInQueue( BellyBounce ), "the second is the head now" );

			OneTurn( park );

			AssertTurnedAway( park, queued[1], "the head, on the second turn" );
			AssertStillQueueing( park, queued[2], "third, on the second turn" );

			OneTurn( park );
			OneTurn( park );

			AssertTurnedAway( park, queued[2], "the last" );
			Assert.AreEqual( 0, park.State.FirstInQueue( BellyBounce ), "the queue is empty" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// A head in <see cref="PeepState.EnteringRide"/> whom the admit slot no longer names is forced on instead
	/// (<c>FUN_00500870</c>): off the queue, riding, nothing lost. One the slot still names is put out, and the
	/// slot is emptied as they go (<c>FUN_004ddd20</c>).
	/// </summary>
	[TestMethod]
	public void AnEnteringHeadIsForcedOnUnlessTheSlotStillNamesThem()
	{
		var park = Open();

		try
		{
			var queued = Queue( park, 2 );
			var script = park.Scripts[BellyBounce];

			park.State.SetParkClosed( true );

			queued[0].SetState( PeepState.EnteringRide, tick: 1, new Random( 1 ) );
			OneTurn( park );

			Assert.AreEqual( PeepState.Riding, queued[0].State, "forced onto the ride" );
			Assert.AreEqual( Before, queued[0].Happiness, 0.001f, "losing nothing" );
			Assert.AreEqual( -1, park.State.PositionInQueue( BellyBounce, queued[0].ThingId ), "and out of the queue" );
			AssertStillQueueing( park, queued[1], "second, while the first was forced on" );

			queued[1].SetState( PeepState.EnteringRide, tick: 1, new Random( 1 ) );
			script.Set( ParkRideOperation.AdmitVariable, queued[1].ThingId );
			OneTurn( park );

			AssertTurnedAway( park, queued[1], "entering, but the slot still names them" );
			Assert.AreEqual( 0, script[ParkRideOperation.AdmitVariable], "the slot is emptied" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// Through the ride's own turn: a closed ride invites nobody and puts one head out on each thing sweep.
	/// </summary>
	[TestMethod]
	public void AClosedRideTurnsAwayOneHeadEverySweep()
	{
		var park = Open();

		try
		{
			var queued = Queue( park, 4 );

			park.State.SetParkClosed( true );

			Time.Paused = false;
			Time.StepFrames = 0;
			GameClock.Rebase();
			Time.Update( 0f );
			GameClock.Update( paused: false, GameClock.ParkCatchUp );

			var sweeps = 0;
			var seenOut = new HashSet<int>();

			for ( var frame = 0; frame < 2000 && sweeps < 6; ++frame )
			{
				Time.Update( 1f / 60f );
				GameClock.Update( paused: false, GameClock.ParkCatchUp );

				var ticks = GameClock.TicksDue;

				park.People.Update();

				var swept = Enumerable.Range( 0, ticks )
					.Count( i => ((GameClock.Ticks - ticks + 1 + i) & (ParkPeople.ThingTickEvery - 1)) == 0 );

				if ( swept == 0 )
					continue;

				sweeps += swept;

				var gone = queued.Where( peep => peep.MajorDest != BellyBounce ).ToArray();

				Assert.AreEqual( Math.Min( sweeps, queued.Length ), gone.Length, $"after {sweeps} sweeps, one head out for each" );
				Assert.AreEqual( 0, park.State.PersonBeingLoaded( BellyBounce ), "nobody is invited" );

				// The ride takes its turn after the people take theirs, so whoever it put out this sweep has
				// not yet decided anything - a guest in a shut park then loses heart and heads for the exit.
				foreach ( var peep in gone.Where( peep => seenOut.Add( peep.ThingId ) ) )
					AssertTurnedAway( park, peep, $"put out on sweep {sweeps}" );
			}

			Assert.IsTrue( sweeps >= 5, "the park swept" );
			Assert.AreEqual( queued.Length, seenOut.Count, "every one was seen as they were put out" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The tail of <c>FUN_004de1f0</c>: a queue measured again opens its closed ride when the guard passes,
	/// whatever the park's door says, and the ride forgets its assigned member of staff either way.
	/// </summary>
	[TestMethod]
	public void AQueueMeasuredAgainOpensItsClosedRide()
	{
		var park = Open();

		try
		{
			// State 3, never offered, is one the guard does not refuse, and the open's SetState(0) moves it.
			park.State.SetParkClosed( true );
			Change( park, BellyBounce, thing => thing with { AssignedStaff = 5, State = 3 } );

			park.State.RemeasureQueue( BellyBounce );

			Assert.AreEqual( 1, Thing( park, BellyBounce ).CanLoad, "the Belly Bounce is open again" );
			Assert.AreEqual( 0, Thing( park, BellyBounce ).State, "and operating" );
			Assert.AreEqual( 0, park.Scripts[BellyBounce][ParkRideOperation.ClosedVariable], "and its script is told" );
			Assert.AreEqual( 0, Thing( park, BellyBounce ).AssignedStaff, "its member of staff is forgotten" );
			Assert.IsTrue( park.State.ParkIsClosed, "the park stays shut" );
			Assert.IsTrue( Visitable( park ).Where( thing => thing.ThingId != BellyBounce ).All( thing => thing.CanLoad == 0 ),
				"and nothing else opens" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>And not when the guard refuses: a mechanic called keeps it closed, and the staff is still forgotten.</summary>
	[TestMethod]
	public void AQueueMeasuredAgainLeavesClosedWhatTheGuardRefuses()
	{
		var park = Open();

		try
		{
			park.State.SetParkClosed( true );
			Change( park, BellyBounce, thing => thing with { RequestedService = 1, AssignedStaff = 5 } );

			park.State.RemeasureQueue( BellyBounce );

			Assert.AreEqual( 0, Thing( park, BellyBounce ).CanLoad, "the Belly Bounce stays closed" );
			Assert.AreEqual( 0, Thing( park, BellyBounce ).AssignedStaff, "its member of staff is forgotten all the same" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The flags a bought thing is given (<see cref="ParkItemCatalogue.Item.ObjectFlags"/>) are the save's own, the
	/// whole word, for every placed object: the Belly Bounce has a queue on the ground and its item
	/// <c>Info.HasQueue</c>, the Jungle Spray neither.
	/// </summary>
	[TestMethod]
	public void ABoughtThingIsFlaggedAsTheSaveFlagsItsOwn()
	{
		var park = Open();

		try
		{
			var compared = 0;

			foreach ( var thing in park.World.Objects.Where( thing => thing.IsPlaced ) )
			{
				if ( !park.Catalogue.TryGet( thing.CatalogueId, out var item ) )
					continue;

				Assert.AreEqual( thing.Flags, item.ObjectFlags, $"thing {thing.ThingId} '{item.Name}'" );
				++compared;
			}

			Assert.AreEqual( 11, compared, "every placed object" );
			Assert.IsTrue( Thing( park, BellyBounce ).HasQueuePath, "one of them has a queue path" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// A Belly Bounce as a purchase or a move builds it - its flags from the item, no saved back of queue - opens
	/// again when the park does: it takes the back-of-queue arm of <c>FUN_004de4a0</c>, where the entrance arm
	/// would find its own queue cell and never open it.
	/// </summary>
	[TestMethod]
	public void ABoughtQueuedRideOpensAgainWithThePark()
	{
		var park = Open();

		try
		{
			Assert.IsTrue( park.Catalogue.TryGet( Thing( park, BellyBounce ).CatalogueId, out var item ), "the item" );
			Change( park, BellyBounce, thing => thing with { Flags = (ushort)item.ObjectFlags, BackOfQueue = 0 } );

			park.State.SetParkClosed( true );

			Assert.AreEqual( 0, Thing( park, BellyBounce ).CanLoad, "closed with the park" );

			park.State.SetParkClosed( false );

			Assert.AreEqual( 1, Thing( park, BellyBounce ).CanLoad, "and open with it again" );
		}
		finally
		{
			Close( park );
		}
	}

	private static int Times( string what ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == what ).Times;

	/// <summary>
	/// A ride whose script reads <c>VAR_BROKEN</c> set is broken down on its own turn (<c>FUN_004e14e0</c>,
	/// <c>0x004e1508</c>, and <c>FUN_004e0e60( 1 )</c>): the broken picture on its hoardings, one head put out,
	/// shut as every close shuts it, the event counted and <c>mState</c> 1. The turns after are state 1's, a head
	/// each.
	/// </summary>
	[TestMethod]
	public void ARideWhoseScriptSaysBrokenIsBrokenDownAndShutOnItsTurn()
	{
		var park = Open();

		try
		{
			var queued = Queue( park, 3 );
			var hoarding = park.State.BindHoarding( BellyBounce );
			var counted = Times( "RIDE_BROKEN_DOWN_EVENT" );

			park.State.NominateForLoading( BellyBounce, queued[0].ThingId );
			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( (0, 1, 0u), (Thing( park, BellyBounce ).State, Thing( park, BellyBounce ).CanLoad, hoarding.Flags),
				"a ride whose script says nothing is left as it was" );

			Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.BrokenVariable, 1 ) );
			park.People.TakeTheRidesTurns( thingTick: 11 );

			var broken = Thing( park, BellyBounce );

			Assert.AreEqual( (ParkRideChoice.StateRefusedOne, 0), (broken.State, broken.CanLoad), "broken down, and its door shut" );
			Assert.AreEqual( 1, park.Scripts[BellyBounce][ParkRideOperation.ClosedVariable], "the script is told it is closed" );
			Assert.AreEqual( 0, park.State.PersonBeingLoaded( BellyBounce ), "the nominee is let go of" );
			Assert.AreEqual( 0x13u, hoarding.Flags, "the hoardings go up with the broken picture, kind 2, kept by the close's kind 1" );
			Assert.AreEqual( 1, hoarding.Texture );
			Assert.AreEqual( counted + 1, Times( "RIDE_BROKEN_DOWN_EVENT" ), "the post to the event bus is counted, once" );
			AssertTurnedAway( park, queued[0], "the completion SetState runs puts the head out" );
			AssertStillQueueing( park, queued[1], "one head a call" );

			foreach ( var other in Visitable( park ).Where( thing => thing.ThingId != BellyBounce ) )
				Assert.AreEqual( (0, 1), (other.State, other.CanLoad), $"thing {other.ThingId}, whose script says nothing, is left open" );

			park.People.TakeTheRidesTurns( thingTick: 12 );

			AssertTurnedAway( park, queued[1], "a broken ride's own turn puts out the next head" );
			AssertStillQueueing( park, queued[2], "and one a turn" );
			Assert.AreEqual( counted + 1, Times( "RIDE_BROKEN_DOWN_EVENT" ), "a ride already broken down is not set so again" );
			Assert.AreEqual( 0x13u, hoarding.Flags );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The remaining life (<c>+0x48</c>) cut to a byte decides between the two: at nought the ride is condemned,
	/// <c>mState</c> 4 and the hoardings' fourth-kind picture (<c>0x004e1556</c>); at one it is broken down.
	/// </summary>
	[TestMethod]
	[DataRow( 0.99f, ParkRideChoice.StateRefusedFour, 0x23u, "RIDE_CONDEMNED_EVENT" )]
	[DataRow( 0f, ParkRideChoice.StateRefusedFour, 0x23u, "RIDE_CONDEMNED_EVENT" )]
	[DataRow( 1f, ParkRideChoice.StateRefusedOne, 0x13u, "RIDE_BROKEN_DOWN_EVENT" )]
	[DataRow( 100f, ParkRideChoice.StateRefusedOne, 0x13u, "RIDE_BROKEN_DOWN_EVENT" )]
	public void ABrokenRideWithNoLifeLeftIsCondemned( float life, int state, uint flags, string gap )
	{
		var park = Open();

		try
		{
			var hoarding = park.State.BindHoarding( BellyBounce );
			var counted = Times( gap );

			Change( park, BellyBounce, thing => thing with { RemainingLife = life } );
			Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.BrokenVariable, 1 ) );
			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( (state, 0), (Thing( park, BellyBounce ).State, Thing( park, BellyBounce ).CanLoad) );
			Assert.AreEqual( flags, hoarding.Flags );
			Assert.AreEqual( counted + 1, Times( gap ) );
			Assert.AreEqual( 1, park.Scripts[BellyBounce][ParkRideOperation.ClosedVariable] );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// What row 5's score function reads of the stock park (<c>FUN_004c9960</c> asked for type 4,
	/// <c>FUN_004ca3a0</c> handed <c>StaffHireMechanics1.PoorerStateThan</c>, 25): its one mechanic, none once he is
	/// fired, and its rides worn: the Belly Bounce once its State of repair's whole number is under 25, or once it
	/// is broken down whatever its repair, and never the Jungle Spray, a sideshow, however worn. And what row 6's
	/// reads (<c>FUN_004ca310</c>): its rides broken down, the Belly Bounce in state 1 and in no other, however
	/// worn, and never the Jungle Spray.
	/// </summary>
	/// <remarks><b>Mutations:</b> a ride at the limit counted; the repair not cut to a whole number; a broken ride not counted, or counted twice; a condemned ride counted; a sideshow counted; the limit nought; every member of staff a mechanic; the fired mechanic still counted; a worn ride counted broken; a condemned ride or a broken sideshow counted broken; the broken ride not counted.</remarks>
	[TestMethod]
	public void TheAdvisorsParkFactsCountTheMechanicsAndTheWornRides()
	{
		var park = Open();

		try
		{
			(int Mechanics, int Worn) Read() => (park.People.AdvisorFacts().Mechanics, park.People.AdvisorFacts().RidesWorn);
			int Broken() => park.People.AdvisorFacts().RidesBroken;
			void Wear( int thing, float repair, int? state = null )
			{
				Assert.IsTrue( park.State.TryObject( thing, out var worn ) );
				park.State.ReplaceObject( worn with { StateOfRepair = repair, State = state ?? worn.State } );
			}

			Assert.AreEqual( (1, 0), Read(), "as shipped" );
			Wear( BellyBounce, 3f );
			Assert.AreEqual( (1, 0), Read(), "with no advisor there is no limit to be under" );
			park.State.AdvisorMessages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
			Assert.AreEqual( (1, 1), Read(), "the limit is the advisor's own" );
			Wear( BellyBounce, 25.0f );
			Assert.AreEqual( (1, 0), Read(), "25 is not under 25" );
			Wear( BellyBounce, 25.9f );
			Assert.AreEqual( (1, 0), Read(), "nor is 25.9, cut to 25" );
			Wear( BellyBounce, 24.99f );
			Assert.AreEqual( (1, 1), Read(), "24.99 is cut to 24" );
			Wear( 14, 3f );
			Assert.AreEqual( (1, 1), Read(), "the Jungle Spray is a sideshow" );
			Assert.AreEqual( 0, Broken(), "a worn ride is not a broken one" );
			Assert.IsTrue( park.State.TryObject( BellyBounce, out var bounce ) );
			Wear( BellyBounce, 24f, ParkRideChoice.StateRefusedOne );
			Assert.AreEqual( (1, 1), Read(), "worn and broken down is one ride" );
			Assert.AreEqual( 1, Broken() );
			Wear( BellyBounce, 100f );
			Assert.AreEqual( (1, 1), Read(), "broken down, whatever its repair" );
			Assert.AreEqual( 1, Broken() );
			Wear( 14, 3f, ParkRideChoice.StateRefusedOne );
			Assert.AreEqual( 1, Broken(), "a broken sideshow is no ride" );
			Wear( BellyBounce, 100f, ParkRideChoice.StateRefusedFour );
			Assert.AreEqual( (1, 0), Read(), "condemned is not broken down" );
			Assert.AreEqual( 0, Broken() );
			Wear( BellyBounce, 100f, bounce.State );
			Assert.AreEqual( (1, 0), Read() );

			Assert.IsTrue( park.People.Fire( Mechanic ) );
			Assert.AreEqual( (0, 0), Read(), "fired" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// What the advisor's row 1 reads of the shipped park (<c>FUN_004c9050</c>, <c>FUN_004ca8e0</c>,
	/// <c>FUN_004c91c0</c>): its door, its things of a ride's item and of a sideshow's, and the guests in state 3
	/// standing on either ticket booth's cell: not one beside a booth, and not one on a booth judging the fee.
	/// And what rows 260 to 262 read (<c>FUN_004ca9b0</c>, <c>FUN_004c9ee0</c>, <c>FUN_004c9f90</c>): its shops and
	/// toilets, and how many of the shops, sideshows and toilets are shut, none in the open park and all once the
	/// door has shut them. And what row 259 reads: how many of its rides are shut, the one ride once the door has
	/// shut it, and still that one with the sideshow and a toilet opened again. And what row 263 reads
	/// (<c>FUN_004ca030</c>, <c>FUN_004ca0c0</c>): its one staff room, which the door leaves open.
	/// </summary>
	/// <remarks><b>Mutations:</b> every waiting guest counted wherever they stand; every guest on a booth counted; one booth alone; the door's answer turned round; the shops counted as rides; the shut rides not counted, or counted with the sideshows'.</remarks>
	[TestMethod]
	public void TheAdvisorsParkFactsCountTheGuestsWaitingOnTheBoothCells()
	{
		var park = Open();

		try
		{
			var random = new Random( 1 );
			var guests = park.People.Peeps;
			foreach ( var guest in guests )
			{
				guest.Navigator.Position = FixedVector.AtCell( 60, 60 );
				guest.SetState( PeepState.Deciding, 800, random );
			}

			Assert.AreEqual( new ParkAdvisorMessages.ParkFacts( false, StockRides, StockSideshows, 0, StockShops, StockToilets, StaffRooms: StockStaffRooms, Mechanics: 1 ), park.People.AdvisorFacts() );

			void Put( int index, int x, int y, PeepState state )
			{
				guests[index].Navigator.Position = FixedVector.AtCell( x, y );
				guests[index].SetState( state, 800, random );
			}

			Put( 0, 47, 13, PeepState.WaitingForOpening );
			Put( 1, 48, 13, PeepState.WaitingForOpening );
			Put( 2, 48, 13, PeepState.WaitingForOpening );
			Put( 3, 48, 12, PeepState.WaitingForOpening );
			Put( 4, 47, 13, PeepState.JudgingTheFee );
			Assert.AreEqual( 3, park.People.WaitingToOpen );

			park.State.SetParkClosed( true );
			Assert.AreEqual( new ParkAdvisorMessages.ParkFacts( true, StockRides, StockSideshows, 3, StockShops, StockToilets,
				ShopsShut: StockShops, SideshowsShut: StockSideshows, ToiletsShut: StockToilets, RidesShut: StockRides, StaffRooms: StockStaffRooms, Mechanics: 1 ), park.People.AdvisorFacts() );

			// One toilet opened again, and the sideshow: the door's others stay shut.
			park.People.SetRideClosed( 22, closed: false );
			park.People.SetRideClosed( 14, closed: false );
			Assert.AreEqual( new ParkAdvisorMessages.ParkFacts( true, StockRides, StockSideshows, 3, StockShops, StockToilets,
				ShopsShut: StockShops, SideshowsShut: 0, ToiletsShut: StockToilets - 1, RidesShut: StockRides, StaffRooms: StockStaffRooms, Mechanics: 1 ), park.People.AdvisorFacts() );

			// The door left the staff room open; shut by itself, it alone is one more shut.
			var before = park.People.AdvisorFacts();
			park.People.SetRideClosed( StaffRoom, closed: true );
			Assert.AreEqual( before with { StaffRoomsShut = 1 }, park.People.AdvisorFacts() );
			StringAssert.EndsWith( park.People.GateDescription(), " waiting 3" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// What the advisor's row 113 reads of the park (<c>FUN_004ccb00</c> asked with nought): the first thing a
	/// guest may choose that has a queue whose back is not joined to a path, none in the shipped park, the Belly
	/// Bounce once its queue's back cell links only to the cell ahead, none while the queue tool is armed for it,
	/// and none for a thing with no queue whose entrance has no path before it.
	/// </summary>
	/// <remarks><b>Mutations:</b> the connected queue answered; the tool's thing not passed over, or every thing passed over while a tool is armed; a thing with no queue answered; a thing no guest may choose answered; the last found answered, not the first.</remarks>
	[TestMethod]
	public void TheAdvisorsParkFactsNameARideWhoseQueueIsNotJoined()
	{
		var park = Open();

		try
		{
			Assert.AreEqual( 0, park.People.AdvisorFacts().UnjoinedQueue, "every queue joined as shipped" );

			// A thing with no queue and no path before its entrance is not asked.
			var spray = Thing( park, 14 );
			Assert.IsFalse( spray.HasQueuePath );
			var facing = spray.Angle switch { 0 => 0x10, 90 => 0x04, 180 => 0x01, _ => 0x40 };
			var (x, y) = MapStep.CellAt( spray.EntryPos );
			var (bx, by) = ParkBuilding.Step( x, y, CellEdge.Opposite( facing ) );
			var beyond = park.State.Record( bx, by );
			park.State.SetRecord( bx, by, beyond with { Type = 0 } );
			Assert.IsFalse( ParkRideOperation.BackOfQueueConnected( park.World, spray ) );
			Assert.AreEqual( 0, park.People.AdvisorFacts().UnjoinedQueue );
			park.State.SetRecord( bx, by, beyond );

			var back = park.State.Record( BackX, BackY );
			park.State.SetRecord( BackX, BackY, back with { Neighbours = 0x04 } );
			Assert.AreEqual( BellyBounce, park.People.AdvisorFacts().UnjoinedQueue );
			Assert.AreEqual( park.People.AdvisorFacts() with { UnjoinedQueue = 0 },
				new ParkAdvisorMessages.ParkFacts( false, StockRides, StockSideshows, park.People.WaitingToOpen, StockShops, StockToilets, StaffRooms: StockStaffRooms, Mechanics: 1 ), "nothing else moved" );

			// The queue tool armed for it passes it over; armed for another thing, or put away, it does not.
			ParkBuildMode.Arm( ParkBuildMode.Queue, BellyBounce );
			Assert.AreEqual( 0, park.People.AdvisorFacts().UnjoinedQueue );
			ParkBuildMode.Arm( ParkBuildMode.Queue, 14 );
			Assert.AreEqual( BellyBounce, park.People.AdvisorFacts().UnjoinedQueue );
			ParkBuildMode.Arm( ParkBuildMode.Path );
			Assert.AreEqual( BellyBounce, park.People.AdvisorFacts().UnjoinedQueue );
			ParkBuildMode.Disarm();
			Assert.AreEqual( BellyBounce, park.People.AdvisorFacts().UnjoinedQueue );

			// With two unjoined, the first of the chain is the one named.
			var sprayFlags = Thing( park, 14 ).Flags;
			Change( park, 14, thing => thing with { Flags = (ushort)(thing.Flags | 0x08) } );
			Assert.IsTrue( Thing( park, 14 ).HasQueuePath );
			var (sprayBack, _) = ParkRideChoice.QueueCellsFor( park.World, Thing( park, 14 ) );
			Assert.AreNotEqual( 0, sprayBack );
			var (sx, sy) = MapStep.CellAt( sprayBack );
			var sprayCell = park.State.Record( sx, sy );
			park.State.SetRecord( sx, sy, sprayCell with { Neighbours = sprayCell.Direction } );
			Assert.IsFalse( ParkRideOperation.BackOfQueueConnected( park.World, Thing( park, 14 ) ), "the Spray, given a queue, joins nothing" );
			var chain = park.State.ObjectsInChainOrder().Where( thing => thing.ThingId is BellyBounce or 14 ).Select( thing => thing.ThingId ).ToArray();
			Assert.AreEqual( 2, chain.Length );
			Assert.AreEqual( chain[0], park.People.AdvisorFacts().UnjoinedQueue );
			ParkBuildMode.Arm( ParkBuildMode.Queue, chain[0] );
			Assert.AreEqual( chain[1], park.People.AdvisorFacts().UnjoinedQueue, "the tool's thing passed over, the next is named" );
			ParkBuildMode.Disarm();
			Change( park, 14, thing => thing with { Flags = sprayFlags } );
			park.State.SetRecord( sx, sy, sprayCell );

			// A thing no guest may choose is not asked.
			Change( park, BellyBounce, thing => thing with { Flags = (ushort)(thing.Flags & ~0x04) } );
			Assert.IsFalse( Thing( park, BellyBounce ).IsVisitable );
			Assert.AreEqual( 0, park.People.AdvisorFacts().UnjoinedQueue );

			park.State.SetRecord( BackX, BackY, back );
		}
		finally
		{
			ParkBuildMode.Disarm();
			Close( park );
		}
	}

	/// <summary>The shipped Lost Kingdom park's things of a ride's item and of a sideshow's: the Belly Bounce and the Jungle Spray.</summary>
	private const int StockRides = 1, StockSideshows = 1;
	/// <summary>Its things of a shop's item, the Drinks Shop, and its toilets, three Small Toilets; the door shuts each.</summary>
	private const int StockShops = 1, StockToilets = 3;
	/// <summary>Its staff rooms, the one Staff Room, thing 20, which the door does not shut.</summary>
	private const int StockStaffRooms = 1, StaffRoom = 20;

	private const int Mechanic = 26;

	private static Staff TheMechanic( Park park ) => park.People.Staff.Single( member => member.ThingId == Mechanic );

	/// <summary>Breaks the Belly Bounce on its own turn with the advisor listening; answers his messages.</summary>
	private static ParkAdvisorMessages BreakWithTheAdvisorListening( Park park, int gameTick = 2000, ParkAdvisorMessages? messages = null )
	{
		park.State.AdvisorMessages = messages ??= new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		park.State.SetGameTick( gameTick );
		Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.BrokenVariable, 1 ) );
		park.People.TakeTheRidesTurns( thingTick: 10 );
		Assert.AreEqual( ParkRideChoice.StateRefusedOne, Thing( park, BellyBounce ).State, "broken down" );

		return messages;
	}

	/// <summary>
	/// The mechanics' base hears a break (<c>FUN_004dadc0</c>): the shipped park's one mechanic, walking with no
	/// job, is free, and the advisor is told nothing.
	/// </summary>
	[TestMethod]
	public void ABreakWithAFreeMechanicTellsTheAdvisorNothing()
	{
		var park = Open();

		try
		{
			Assert.AreEqual( (0, false), (TheMechanic( park ).ObjectToRepair, TheMechanic( park ).Activity is StaffActivity.Resting) );
			Assert.AreEqual( 0, BreakWithTheAdvisorListening( park ).PendingCount );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>With no mechanic in the park the advisor is posted message 0x44 with the ride, on the park's tick.</summary>
	[TestMethod]
	public void ABreakWithNoMechanicInTheParkPostsMessage68WithTheRide()
	{
		var park = Open();

		try
		{
			Assert.IsTrue( park.People.Fire( Mechanic ) );

			var messages = BreakWithTheAdvisorListening( park );

			Assert.IsTrue( messages.Census().Contains( "pending=1 attempts=0 threshold=25 slot=68/subject=13 " ), messages.Census() );

			var said = new List<(int, int)>();

			messages.Tick( 2001, 0, ( response, sample ) => { said.Add( (response, sample) ); return 0; } );
			CollectionAssert.AreEqual( new[] { (198, 196) }, said );

			park.People.TakeTheRidesTurns( thingTick: 11 );
			Assert.AreEqual( 0, messages.PendingCount, "a ride already broken down is not spoken of again" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>The post carries the park's tick: one inside 480 sweeps of the line last said is refused.</summary>
	[TestMethod]
	public void ABreakInsideTheMessagesOwnWaitIsNotPosted()
	{
		var park = Open();

		try
		{
			Assert.IsTrue( park.People.Fire( Mechanic ) );

			var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );

			Assert.IsTrue( messages.Post( ParkAdvisorMessages.NoMechanics, 1600, 14 ) );
			messages.Tick( 1600, 0, ( _, _ ) => 0 );
			Assert.AreEqual( 0, BreakWithTheAdvisorListening( park, gameTick: 2079, messages ).PendingCount, "479 sweeps on" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// A mechanic with another job, or with none and resting, on his way to rest or striking, is busy: with
	/// nobody else, message 0x45. One with no job in any other state is free.
	/// </summary>
	[TestMethod]
	[DataRow( 14, StaffActivity.GoingToRide, 69 )]
	[DataRow( 14, StaffActivity.Idle, 69 )]
	[DataRow( 0, StaffActivity.GoingToRest, 69 )]
	[DataRow( 0, StaffActivity.Resting, 69 )]
	[DataRow( 0, StaffActivity.GoingOnStrike, 69 )]
	[DataRow( 0, StaffActivity.OnStrike, 69 )]
	[DataRow( 0, StaffActivity.Idle, 0 )]
	[DataRow( 0, StaffActivity.Walking, 0 )]
	[DataRow( 0, StaffActivity.Repairing, 0 )]
	[DataRow( 0, StaffActivity.Held, 0 )]
	public void ABreakWithEveryMechanicBusyPostsMessage69( int job, StaffActivity doing, int message )
	{
		var park = Open();

		try
		{
			TheMechanic( park ).ObjectToRepair = job;
			TheMechanic( park ).SetActivity( doing, 0 );
			Assert.AreEqual( message, ParkPeople.MessageForABreak( park.People.Staff, Thing( park, BellyBounce ) ) );

			var messages = BreakWithTheAdvisorListening( park );

			Assert.AreEqual( message == 0 ? 0 : 1, messages.PendingCount );
			Assert.AreEqual( message != 0, messages.Census().Contains( "slot=69/subject=13 " ), messages.Census() );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The advisor's turn is the sweep's last (<c>FUN_0050b360</c>'s arm <c>0xb</c>, thing 3 behind every thing
	/// made after him), so a message a ride posts on its turn is said on that same tick: a Belly Bounce worn
	/// under 10 on tick 64.
	/// </summary>
	[TestMethod]
	public void AMessageARidePostsOnItsTurnIsSaidOnTheSameSweep()
	{
		var park = Open();

		try
		{
			var messages = park.State.AdvisorMessages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );

			Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.OnRideVariable, 5 ) );
			Change( park, BellyBounce, thing => thing with { StateOfRepair = 11.5f } );
			park.State.SetGameTick( 63 );

			Time.Paused = false;
			Time.StepFrames = 0;
			GameClock.Rebase();
			Time.Update( 0f );
			GameClock.Update( paused: false, GameClock.ParkCatchUp );

			for ( var frame = 0; frame < 2000 && park.State.GameTick < 64; ++frame )
			{
				Time.Update( 1f / 60f );
				GameClock.Update( paused: false, GameClock.ParkCatchUp );
				park.People.Update();
			}

			Assert.AreEqual( 64, park.State.GameTick, "one sweep" );
			Assert.IsTrue( messages.Census().Contains( "pending=0 attempts=1 " ), messages.Census() );
			Assert.IsTrue( messages.Census().Contains( "message=66/score=100/line=0/lastTick=64/said=1/" ), messages.Census() );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The sweep hands the advisor the park: with the shipped park shut and three guests waiting on the booths'
	/// cells, the sweep whose poll stands on row 1 says message 1 at 60.
	/// </summary>
	[TestMethod]
	public void TheSweepHandsTheAdvisorTheParkSoAShutParksLineIsSaid()
	{
		var park = Open();

		try
		{
			var messages = park.State.AdvisorMessages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ),
				cursor: new ParkAdvisorMessages.PollCursor { Row = 1 } );
			var random = new Random( 1 );
			park.State.SetParkClosed( true );
			foreach ( var (guest, x) in new[] { (0, 47), (1, 48), (2, 48) } )
			{
				park.People.Peeps[guest].Navigator.Position = FixedVector.AtCell( x, 13 );
				park.People.Peeps[guest].SetState( PeepState.WaitingForOpening, park.State.GameTick, random );
			}

			var before = park.People.WaitingToOpen;
			Time.Paused = false;
			Time.StepFrames = 0;
			GameClock.Rebase();
			Time.Update( 0f );
			GameClock.Update( paused: false, GameClock.ParkCatchUp );
			var first = park.State.GameTick;

			for ( var frame = 0; frame < 2000 && park.State.GameTick < first + 1; ++frame )
			{
				Time.Update( 1f / 60f );
				GameClock.Update( paused: false, GameClock.ParkCatchUp );
				park.People.Update();
			}

			var waiting = park.People.WaitingToOpen;
			Assert.IsTrue( before >= 3 && waiting >= 3, $"{before} then {waiting} waiting" );
			Assert.IsTrue( messages.Census().Contains( $"message=1/score={20 * waiting}/line=0/lastTick={first + 1}/said=1/" ), messages.Census() );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// And one a member of staff posts on theirs: the mechanic's decide that takes the broken Bounce posts
	/// message 0x46, which is never left waiting at a sweep's end.
	/// </summary>
	[TestMethod]
	public void AMessageAMechanicPostsOnHisTurnIsSaidOnTheSameSweep()
	{
		var park = Open();

		try
		{
			var messages = park.State.AdvisorMessages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );

			Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.BrokenVariable, 1 ) );

			Time.Paused = false;
			Time.StepFrames = 0;
			GameClock.Rebase();
			Time.Update( 0f );
			GameClock.Update( paused: false, GameClock.ParkCatchUp );

			var first = park.State.GameTick;

			for ( var frame = 0; frame < 6000 && messages.Attempts == 0; ++frame )
			{
				Time.Update( 1f / 60f );
				GameClock.Update( paused: false, GameClock.ParkCatchUp );
				park.People.Update();
				Assert.AreEqual( 0, messages.PendingCount, $"left waiting at the end of sweep {park.State.GameTick}: {messages.Census()}" );
			}

			Assert.AreEqual( BellyBounce, TheMechanic( park ).ObjectToRepair, "the mechanic took it" );
			Assert.IsTrue( park.State.GameTick > first + 1, "on a later sweep than the break's" );
			Assert.IsTrue( messages.Census().Contains( $"message=70/score=110/line=0/lastTick={park.State.GameTick}/said=1/" ), messages.Census() );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// A mechanic whose job already names the ride ends the look: message 0x46 where nobody called him to it
	/// (<c>+0x64</c>), nothing where somebody did. A condemned ride's event is another, and posts nothing.
	/// </summary>
	[TestMethod]
	[DataRow( 0, 100f, 70 )]
	[DataRow( 1, 100f, 0 )]
	[DataRow( 0, 0f, 0 )]
	public void ABreakOfTheRideAMechanicAlreadyNamesPostsMessage70UnlessHeWasCalled( int called, float life, int message )
	{
		var park = Open();

		try
		{
			TheMechanic( park ).ObjectToRepair = BellyBounce;
			Change( park, BellyBounce, thing => thing with { RequestedService = called, RemainingLife = life } );

			var messages = park.State.AdvisorMessages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );

			Assert.IsTrue( park.Scripts[BellyBounce].Set( ParkRideOperation.BrokenVariable, 1 ) );
			park.People.TakeTheRidesTurns( thingTick: 10 );
			Assert.AreEqual( message == 0 ? 0 : 1, messages.PendingCount, messages.Census() );
			Assert.AreEqual( message != 0, messages.Census().Contains( $"slot={message}/subject=13 " ), messages.Census() );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>Two mechanics: a busy one before a free one says nothing, and two busy say message 0x45.</summary>
	[TestMethod]
	public void TheLookGoesOnPastABusyMechanicToAFreeOne()
	{
		var park = Open();

		try
		{
			var saved = park.World.People.Single( person => person.ThingId == Mechanic );
			var second = new Staff( 99, saved.Model, saved.Staff!.Value with { State = (int)StaffActivity.Walking }, saved.Navigator );
			var first = TheMechanic( park );

			first.ObjectToRepair = 14;
			Assert.AreEqual( 0, ParkPeople.MessageForABreak( new[] { first, second }, Thing( park, BellyBounce ) ) );
			Assert.AreEqual( 0, ParkPeople.MessageForABreak( new[] { second, first }, Thing( park, BellyBounce ) ) );
			second.ObjectToRepair = 15;
			Assert.AreEqual( 69, ParkPeople.MessageForABreak( new[] { first, second }, Thing( park, BellyBounce ) ) );
			Assert.AreEqual( 68, ParkPeople.MessageForABreak( park.People.Staff.Where( member => member.ThingId != Mechanic ), Thing( park, BellyBounce ) ),
				"the other four kinds are not mechanics" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// <c>mRequestedService</c> is read from file offset 1078, the dword after the three floats; the shipped
	/// park has no mechanic called on any of its fourteen objects. This pins the offset against the float
	/// before it only: the dword after it is nought here too, and the chain that closes on
	/// <c>mTotalTakings</c> at 1090 is what places it (<see cref="ParkWorld"/>).
	/// </summary>
	[TestMethod]
	public void NoObjectInTheShippedParkHasAMechanicCalled()
	{
		var park = Open();

		try
		{
			Assert.AreEqual( 14, park.World.Objects.Count, "fourteen objects" );
			Assert.IsTrue( park.World.Objects.All( thing => thing.RequestedService == 0 ), "none waits on a mechanic" );
			Assert.AreEqual( 100f, park.World.Objects.Single( thing => thing.ThingId == BellyBounce ).StateOfRepair,
				"the float before it reads as it should" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The last step of the turn's first half, <c>FUN_004e0b90</c> (<c>docs/exe/ride-operation.md</c>, "The first
	/// half of the turn", step 5): a head who is not queueing is cleared and nothing else is tidied, in every state
	/// but 3 and 4. So the head is nobody afterwards, and the guest is left as they were: still naming the ride and
	/// with their happiness of 50 whole, because in states 1 and 2 the completion that follows finds no head to put
	/// out and takes no <c>MediumHappinessChange</c> off anybody.
	/// </summary>
	[TestMethod]
	[DataRow( 0 )]
	[DataRow( ParkRideChoice.StateRefusedOne )]
	[DataRow( 2 )]
	public void AStaleQueueHeadIsDroppedOnTheTurnOfAThingNotInStateThreeOrFour( int state )
	{
		var park = Open();

		try
		{
			var head = Queue( park, 1 )[0];

			Change( park, BellyBounce, thing => thing with { State = state } );
			head.SetState( PeepState.Deciding, tick: 1, new Random( 1 ) );

			Assert.AreEqual( head.ThingId, park.State.FirstInQueue( BellyBounce ), "the head names a guest who queues no longer" );

			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( 0, park.State.FirstInQueue( BellyBounce ), $"state {state}: the stale head is cleared" );
			Assert.AreEqual( BellyBounce, head.MajorDest, $"state {state}: and nothing else is tidied" );
			Assert.AreEqual( Before, head.Happiness, 0.001f, $"state {state}: dropped, not put out, so nothing is lost" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// <c>FUN_004e0b90</c> returns for states 3 and 4 before its stale-head drop, and <c>FUN_004e0e00</c> sends
	/// state 3 straight to its return (<c>docs/exe/ride-operation.md</c>, "The first half of the turn", step 2:
	/// "a state-3 object does nothing at all on its turn"; "The second half", state 3: nothing). So a state-3
	/// thing whose head has stopped queueing still names that head after its turn.
	/// </summary>
	[TestMethod]
	public void AThingInStateThreeKeepsAStaleQueueHeadOnItsTurn()
	{
		var park = Open();

		try
		{
			var head = Queue( park, 1 )[0];

			Change( park, BellyBounce, thing => thing with { State = 3 } );
			head.SetState( PeepState.Deciding, tick: 1, new Random( 1 ) );

			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( head.ThingId, park.State.FirstInQueue( BellyBounce ), "state 3 returns before the stale-head drop" );
			Assert.AreEqual( BellyBounce, head.MajorDest, "and the guest is not touched" );
			Assert.AreEqual( Before, head.Happiness, 0.001f, "nor put out" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// The watchdog at the tail of <c>FUN_004e1220</c> (<c>0x004e1382</c>), through the ride's own turn: on a turn
	/// that invites nobody an open ride lets go of a nominee who is not being admitted, and keeps one who is. A
	/// closed ride (<c>mCanLoad</c> nought) runs the completion and returns at <c>0x004e13fc</c>, so the watchdog is
	/// not reached and the nominee stands (<c>docs/exe/ride-operation.md</c>, "The closed ride", the completion).
	/// </summary>
	[TestMethod]
	public void AnOpenRidesTurnDropsANomineeWhoIsNotComingToBoard()
	{
		var park = Open();

		try
		{
			var guest = park.People.Peeps[0];

			guest.SetState( PeepState.BeingAdmitted, tick: 1, new Random( 1 ) );
			park.State.NominateForLoading( BellyBounce, guest.ThingId );
			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( guest.ThingId, park.State.PersonBeingLoaded( BellyBounce ), "a nominee on their way is kept" );

			guest.SetState( PeepState.Deciding, tick: 1, new Random( 1 ) );
			Change( park, BellyBounce, thing => thing with { CanLoad = 0 } );
			park.People.TakeTheRidesTurns( thingTick: 11 );

			Assert.AreEqual( guest.ThingId, park.State.PersonBeingLoaded( BellyBounce ),
				"a closed ride's turn does not reach the watchdog" );

			Change( park, BellyBounce, thing => thing with { CanLoad = 1 } );
			park.People.TakeTheRidesTurns( thingTick: 12 );

			Assert.AreEqual( 0, park.State.PersonBeingLoaded( BellyBounce ),
				"an open ride's turn lets go of a nominee who is not being admitted" );
		}
		finally
		{
			Close( park );
		}
	}

	/// <summary>
	/// <c>FUN_004e0e00</c>'s arm for states 1, 2 and 4 is the completion <c>FUN_004e0450</c> and then the dismissal
	/// <c>FUN_004e1410</c> (<c>docs/exe/ride-operation.md</c>, "The second half"): a ride broken down, waiting for
	/// an upgrade or condemned still lets off the rider its script names in <c>VAR_LETMEOFF</c>, who is then
	/// leaving the ride, and the slot is emptied behind them.
	/// </summary>
	[TestMethod]
	[DataRow( ParkRideChoice.StateRefusedOne )]
	[DataRow( 2 )]
	[DataRow( ParkRideChoice.StateRefusedFour )]
	public void ARideOutOfServiceStillLetsOffWhoeverItsScriptReports( int state )
	{
		var park = Open();

		try
		{
			var rider = park.People.Peeps[0];
			var script = park.Scripts[BellyBounce];

			rider.MajorDest = BellyBounce;
			rider.QueuePos = 0;
			rider.SetState( PeepState.Riding, tick: 1, new Random( 1 ) );

			Change( park, BellyBounce, thing => thing with { State = state, CanLoad = 0 } );
			Assert.IsTrue( script.Set( ParkRideOperation.DismissVariable, rider.ThingId ), "the script reports its rider" );

			park.People.TakeTheRidesTurns( thingTick: 10 );

			Assert.AreEqual( PeepState.LeavingRide, rider.State, $"state {state}: the rider is let off" );
			Assert.AreEqual( 0, script[ParkRideOperation.DismissVariable], $"state {state}: and the slot is emptied behind them" );
		}
		finally
		{
			Close( park );
		}
	}
}
