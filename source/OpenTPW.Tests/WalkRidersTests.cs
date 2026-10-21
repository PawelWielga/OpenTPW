using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Numerics = System.Numerics;

namespace OpenTPW.Tests;

/// <summary>
/// Where a visitor a walk holds is drawn (<see cref="WalkRiders"/>): the engine's placer <c>FUN_005580a0</c> as its
/// stepper <c>FUN_00557d80</c> hands it each slot (<c>docs/exe/ride-operation.md</c>, "Where a walk's rider is drawn").
/// The places are the ones the original's own saves hold its riders' sprites at (<c>walkriders</c>, sixteen park files).
/// </summary>
[TestClass]
public class WalkRidersTests
{
	/// <summary>A slot whose four node ids differ, so a placement taking the wrong one is seen.</summary>
	private static RideScript.WalkView Slot( RideScript.WalkState state, short action, short flags = 1 )
		=> new( state, action, flags, WalkNode: 1, HeadNode: 7, OffFrom: 9, OffTo: 16, Start: 1000f, Due: 3000f, Facing: 3 );

	/// <summary>
	/// <b>Each state hands the placer its own two nodes</b> (<c>FUN_00557d80</c>'s switch): walking on, the walk node to
	/// the head node, the head in space <c>0x80</c> under action 4 alone; walking off, off-from to off-to, off-from in
	/// <c>0x80</c> under action 4 alone; done, off-to both ends; carried under 3, 5 or 6, the head node both ends in
	/// <c>0x800</c>, under 4 too. Every one walking or done is put on the walk, animation 1.
	/// </summary>
	[TestMethod]
	public void EachStateHandsThePlacerItsOwnTwoNodes()
	{
		const uint walk = RideNodes.WalkSpace, head = RideNodes.HeadSpace;

		foreach ( var (state, action, from, fromSpace, to, toSpace, moving) in new (RideScript.WalkState, short, int, uint, int, uint, bool)[]
		{
			(RideScript.WalkState.WalkingOn, 6, 1, walk, 7, walk, true),
			(RideScript.WalkState.WalkingOn, 4, 1, walk, 7, head, true),
			(RideScript.WalkState.WalkingOff, 6, 9, walk, 16, walk, true),
			(RideScript.WalkState.WalkingOff, 4, 9, head, 16, walk, true),
			(RideScript.WalkState.Done, 4, 16, walk, 16, walk, false),
			(RideScript.WalkState.Done, 1, 16, walk, 16, walk, false),
		} )
		{
			var placed = WalkRiders.Of( Slot( state, action ) );
			var why = $"{state} under action {action}";

			Assert.IsNotNull( placed, why );
			Assert.IsFalse( placed.Value.Hidden, why );
			Assert.AreEqual( (from, fromSpace, to, toSpace, moving), (placed.Value.From, placed.Value.FromSpace, placed.Value.To,
				placed.Value.ToSpace, placed.Value.Moving), why );
			Assert.AreEqual( WalkRiders.WalkAnimation, placed.Value.Animation, why );
			Assert.AreEqual( 3, placed.Value.Facing, why );
		}
	}

	/// <summary>
	/// <b>Carried, the action decides</b> (<c>FUN_005580a0</c>): under 1, 2 or 4 the visitor is hidden
	/// (<c>FUN_004f9ed0</c>); under 3, 5 and 6 they stand on the head node on animations 3, 8 and 5 - the scripts 90, 172
	/// and 140 the original's saves hold its riders under 5 and 6 on; under an action it has no arm for, the placer does
	/// nothing.
	/// </summary>
	[TestMethod]
	public void CarriedTheActionHidesOrStandsTheVisitor()
	{
		foreach ( short action in new short[] { 1, 2, 4 } )
			Assert.IsTrue( WalkRiders.Of( Slot( RideScript.WalkState.Carried, action ) ) is { Hidden: true }, $"action {action}" );

		foreach ( var (action, animation) in new (short, int)[] { (3, 3), (5, 8), (6, 5) } )
		{
			var placed = WalkRiders.Of( Slot( RideScript.WalkState.Carried, action ) );

			Assert.IsTrue( placed is { Hidden: false, Moving: false }, $"action {action}" );
			Assert.AreEqual( (7, RideNodes.WalkSpace, 7, RideNodes.WalkSpace), (placed!.Value.From, placed.Value.FromSpace, placed.Value.To,
				placed.Value.ToSpace), $"action {action}" );
			Assert.AreEqual( animation, placed.Value.Animation, $"action {action}" );
		}

		Assert.AreEqual( 140, new SpriteScript( SpriteScript.None, 0, 0, 0 ) is var five && five.Start( 5 ) ? five.Script : -1 );
		Assert.AreEqual( 172, new SpriteScript( SpriteScript.None, 0, 0, 0 ) is var eight && eight.Start( 8 ) ? eight.Script : -1 );

		Assert.IsNull( WalkRiders.Of( Slot( RideScript.WalkState.Carried, 0 ) ) );
		Assert.IsNull( WalkRiders.Of( Slot( RideScript.WalkState.Carried, 7 ) ) );
		Assert.IsNull( WalkRiders.Of( Slot( RideScript.WalkState.Free, 6 ) ) );
	}

	/// <summary>
	/// <b>The share of the leg is the clock's, at most a thousand</b>: a leg from 1000 to 3000 ms is half walked at 2000,
	/// done at 3000 and held there after, a tenth walked at 1200; a slot not moving stands at nought.
	/// </summary>
	[TestMethod]
	public void TheShareOfTheLegIsTheClocksAtMostAThousand()
	{
		var slot = Slot( RideScript.WalkState.WalkingOn, 6 );
		var placed = WalkRiders.Of( slot )!.Value;

		Assert.AreEqual( 500, WalkRiders.Progress( slot, placed, 2000f ) );
		Assert.AreEqual( 100, WalkRiders.Progress( slot, placed, 1200f ) );
		Assert.AreEqual( 1000, WalkRiders.Progress( slot, placed, 3000f ) );
		Assert.AreEqual( 1000, WalkRiders.Progress( slot, placed, 9000f ) );

		var carried = Slot( RideScript.WalkState.Carried, 6 );

		Assert.AreEqual( 0, WalkRiders.Progress( carried, WalkRiders.Of( carried )!.Value, 2000f ) );
	}

	/// <summary>
	/// <b>The point is A + (B − A) × p / 1000 on all three axes under flag bit 0</b>; without it the height would be the
	/// surface's, which no shipped walk asks for, and nothing is placed. The Gift Shop of Alexah's jungle <c>New Save</c>
	/// held its customer walking on from (465, 275) to (465, 285) at (465, 279.070), 407 thousandths along.
	/// </summary>
	[TestMethod]
	public void ThePointIsAlongTheLineOnAllThreeAxes()
	{
		var placed = WalkRiders.Of( Slot( RideScript.WalkState.WalkingOn, 1 ) )!.Value;
		var point = WalkRiders.Point( placed, new Numerics.Vector3( 10f, 2f, 30f ), new Numerics.Vector3( 20f, 6f, 50f ), 250 );

		Assert.IsNotNull( point );
		Assert.AreEqual( 12.5f, point.Value.X, 1e-5f );
		Assert.AreEqual( 3f, point.Value.Y, 1e-5f );
		Assert.AreEqual( 35f, point.Value.Z, 1e-5f );

		var shop = WalkRiders.Point( placed, new Numerics.Vector3( 465f, 0f, 275f ), new Numerics.Vector3( 465f, 0f, 285f ), 407 )!.Value;

		Assert.AreEqual( 279.070f, shop.Z, 0.0005f );

		Assert.IsNull( WalkRiders.Point( WalkRiders.Of( Slot( RideScript.WalkState.WalkingOn, 1, flags: 0 ) )!.Value,
			new Numerics.Vector3( 10f, 2f, 30f ), new Numerics.Vector3( 20f, 6f, 50f ), 250 ) );
	}

	/// <summary>
	/// <b>The placer queues its animation only where the sprite is neither on it nor has it queued</b>
	/// (<c>FUN_004d4100</c>), and asks nothing of a visitor it hides.
	/// </summary>
	[TestMethod]
	public void TheAnimationIsQueuedOnlyWhereTheSpriteLacksIt()
	{
		var carried = WalkRiders.Of( Slot( RideScript.WalkState.Carried, 6 ) )!.Value;
		var walking = new SpriteScript( SpriteScript.None, 0, 0, 0 );
		walking.Start( WalkRiders.WalkAnimation );

		Assert.AreEqual( 5, WalkRiders.Asked( carried, walking, 0 ) );
		Assert.IsNull( WalkRiders.Asked( carried, walking, 5 ), "already queued" );

		var on = new SpriteScript( SpriteScript.None, 0, 0, 0 );
		on.Start( 5 );

		Assert.IsNull( WalkRiders.Asked( carried, on, 0 ), "already on it" );
		Assert.IsNull( WalkRiders.Asked( carried, null, 0 ), "no sprite" );
		Assert.IsNull( WalkRiders.Asked( WalkRiders.Of( Slot( RideScript.WalkState.Carried, 1 ) )!.Value, walking, 3 ), "hidden" );
		Assert.AreEqual( 1, WalkRiders.Asked( WalkRiders.Of( Slot( RideScript.WalkState.WalkingOff, 6 ) )!.Value, on, 0 ) );
	}

	/// <summary>
	/// <b>The walk slot holding a visitor is the last in use with their handle</b>: the stepper places every slot in order,
	/// so the later's placement stands; a slot let go holds nobody, and nobody is found for a handle of nought, though a
	/// script can walk one on.
	/// </summary>
	[TestMethod]
	public void TheSlotHoldingAVisitorIsTheLastInUse()
	{
		var script = new RideScript( Build( walkSlots: 4, 0 ) );

		script.RestoreRiders( new SavedScript( 1, 0, 0, [], 0, 0, 0, [], 0, 0, 0, 0, 0, Walk:
		[
			new SavedWalkSlot( 1, 2, 2, 16, 5000, 5700, 42, 6, (short)RideScript.WalkState.Carried, 1, 1 ),
			new SavedWalkSlot( 1, 3, 3, 16, 5000, 5900, 42, 6, (short)RideScript.WalkState.WalkingOff, 1, 4 ),
			new SavedWalkSlot( 1, 4, 4, 16, 5000, 5800, 42, 6, 0, 1, 2 ),
			new SavedWalkSlot( 1, 5, 5, 16, 5000, 5800, 0, 6, (short)RideScript.WalkState.Carried, 1, 2 ),
		] ), reading => reading != 0 ? reading - 5000f : null );

		Assert.IsTrue( script.TryWalkOf( 42, out var view ) );
		Assert.AreEqual( (RideScript.WalkState.WalkingOff, (short)3, (short)4), (view.State, view.HeadNode, view.Facing) );
		Assert.IsFalse( script.TryWalkOf( 43, out _ ) );
		Assert.IsFalse( script.TryWalkOf( 0, out _ ) );
	}

	/// <summary>
	/// <b>A rider the shipped Jungle Spray carries stands where the original put its sprite</b>: Alexah's jungle
	/// <c>New Save</c>'s Jungle Spray on (42, 25) turned 270 held its riders carried under action 6 on lane nodes 2 and 1
	/// with their sprites at (421.995, 265.127) and (421.995, 255.931); its Laughing Hyenas on (30, 27) turned 180, on
	/// node 2 at (295.052, 274.420); its Strength Bird on (41, 29), under action 5 on node 2 at (420.000, 297.500).
	/// </summary>
	[TestMethod]
	public void ACarriedRiderStandsWhereTheOriginalsSaveHoldsTheSprite()
	{
		var data = GameData.Required();

		foreach ( var (directory, stem, cellX, cellY, angle, action, node, x, z) in new (string, string, int, int, int, short, short, float, float)[]
		{
			("levels/jungle/sideshow/junspray", "junspray", 42, 25, 270, 6, 2, 421.995f, 265.127f),
			("levels/jungle/sideshow/junspray", "junspray", 42, 25, 270, 6, 1, 421.995f, 255.931f),
			("levels/jungle/sideshow/hyenas", "hyenas", 30, 27, 180, 6, 2, 295.052f, 274.420f),
			("levels/jungle/sideshow/squark", "squark", 41, 29, 0, 5, 2, 420.000f, 297.500f),
		} )
		{
			var nodes = RideNodes.Load( directory, stem, data, false, [] )!;
			nodes.Place( ParkObjects.OriginFor( cellX, cellY, angle ), angle );

			var slot = new RideScript.WalkView( RideScript.WalkState.Carried, action, 1, WalkNode: 4, HeadNode: node, OffFrom: node,
				OffTo: 4, Start: 0f, Due: 0f, Facing: 0 );
			var placed = WalkRiders.Of( slot )!.Value;

			nodes.Find( placed.From, placed.FromSpace, out var from );
			nodes.Find( placed.To, placed.ToSpace, out var to );

			var at = WalkRiders.Point( placed, from, to, WalkRiders.Progress( slot, placed, 0f ) )!.Value;

			Assert.AreEqual( x, at.X, 0.0005f, $"{stem} node {node}" );
			Assert.AreEqual( z, at.Z, 0.0005f, $"{stem} node {node}" );
		}
	}

	/// <summary>
	/// <b>The Mayan Spinner's first rider stands on its own node, not at the steps</b>: <c>Lookout.RSE</c> walks a rider
	/// on under action 6 to node <c>VAR_ONRIDE</c> plus 2, so the first carried rider of a Spinner on (57, 23) is placed
	/// on node 2 at (581.190, 242.909), on animation 5, facing 1, the way the node points from the step after it arrives.
	/// </summary>
	[TestMethod]
	public void TheSpinnersFirstRiderStandsOnItsOwnNode()
	{
		var data = GameData.Required();
		var nodes = RideNodes.Load( "levels/jungle/rides/lookout", "lookout", data, false, [] )!;
		nodes.Place( ParkObjects.OriginFor( 57, 23, 0 ), 0 );

		var script = new RideScript( Build( walkSlots: 2, Word( Opcode.WALKON ), 42, 1, 2, 2, 16, 6, 1, Word( Opcode.END ) ) ) { Nodes = nodes };

		script.Turn( 0f );
		script.StepTheWalks( 100_000f );

		// The step that finds the rider arrived changes the state alone; the next takes the node's facing.
		script.StepTheWalks( 100_031f );

		Assert.IsTrue( script.TryWalkOf( 42, out var slot ) );
		Assert.AreEqual( RideScript.WalkState.Carried, slot.State );

		var placed = WalkRiders.Of( slot )!.Value;

		nodes.Find( placed.From, placed.FromSpace, out var from );
		nodes.Find( placed.To, placed.ToSpace, out var to );

		var at = WalkRiders.Point( placed, from, to, WalkRiders.Progress( slot, placed, 100_000f ) )!.Value;

		Assert.AreEqual( (2, 5, (short)1), (placed.To, placed.Animation, placed.Facing) );
		Assert.AreEqual( 581.190f, at.X, 0.0005f );
		Assert.AreEqual( 242.909f, at.Z, 0.0005f );
	}

	/// <summary>
	/// <b>A sweep hands a carried guest the placer's animation, and frees a hidden one's bubble</b>
	/// (<c>FUN_004f9e60</c>, <c>FUN_004f9ed0</c>): in the stock park, guest 29 riding thing 13 carried under action 6 is on
	/// animation 5 (script 140) after eight sweeps; carried under action 1, the bubble a thought gave them on the sweep
	/// before is gone after one, which the needs turn's expiry would not take for twelve.
	/// </summary>
	[DataTestMethod]
	[DataRow( (short)6 )]
	[DataRow( (short)1 )]
	public void ASweepHandsACarriedGuestThePlacersAnimation( short action )
	{
		var data = GameData.Required();
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );
		using var clock = new SimulationClockScope();
		var previousState = ParkState.Current;
		var previousPeople = ParkPeople.Current;
		ParkPeople? people = null;

		try
		{
			var state = new ParkState( world );
			var script = new RideScript( Build( walkSlots: 2, 0 ) );

			people = new ParkPeople( world, state: state, scriptFor: id => id == 13 ? script : null, random: new Random( 1 ),
				behaviourRandom: new Random( 2 ), rideRandom: new Random( 3 ), staffRandom: new Random( 4 ) );

			// After the people are made, which puts the park file's own riders back into the scripts.
			script.RestoreRiders( new SavedScript( 1, 0, 0, [], 0, 0, 0, [], 0, 0, 0, 0, 0, Walk:
			[
				new SavedWalkSlot( 1, 2, 2, 16, 5000, 5700, 29, action, (short)RideScript.WalkState.Carried, 1, 0 ),
				default,
			] ), reading => reading != 0 ? reading - 5000f : null );

			var guest = people.Peeps.Single( peep => peep.ThingId == 29 );
			guest.SetState( PeepState.Riding, 0, new Random( 1 ) );

			var sprite = people.SpriteFor( 29 );
			Assert.IsNotNull( sprite );
			Assert.IsFalse( sprite.IsOn( 5 ), "not on it before" );
			Assert.IsTrue( people.TryWalkOf( 29, out var holder, out _ ) && holder == script );

			Assert.IsTrue( guest.Thoughts.Set( 1, state.GameTick ), "a bubble to free" );

			for ( var sweep = 0; sweep < (action == 6 ? 8 : 1); ++sweep )
			{
				SimulationClockScope.Frame( 8 * GameClock.TickSeconds );
				people.Update();
			}

			if ( action == 6 )
			{
				Assert.IsTrue( sprite.IsOn( 5 ), $"script {sprite.Script}" );
				Assert.AreEqual( 140, sprite.Script );
			}
			else
			{
				Assert.IsNull( guest.Thoughts.Bubble, "the hidden rider's bubble is freed" );
			}
		}
		finally
		{
			people?.Delete();
			Entity.ApplyDeletions();
			typeof( ParkPeople ).GetProperty( "Current", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic )!.SetValue( null, previousPeople );
			typeof( ParkState ).GetProperty( nameof( ParkState.Current ) )!.SetValue( null, previousState );
		}
	}

	private static int Word( Opcode opcode ) => unchecked( (int)(0x80000000u | (uint)opcode) );

	/// <summary>A script declaring <paramref name="walkSlots"/> walk slots, two variables, and <paramref name="body"/>.</summary>
	private static RideScriptFile Build( int walkSlots, params int[] body )
	{
		using var memory = new MemoryStream();
		using var writer = new BinaryWriter( memory );

		writer.Write( Encoding.ASCII.GetBytes( "RSSE" ) );
		writer.Write( 0x00010F51 );
		writer.Write( 2 );
		writer.Write( 8 );
		writer.Write( 50 );
		writer.Write( 0 );
		writer.Write( 0 );
		writer.Write( walkSlots );
		writer.Write( Encoding.ASCII.GetBytes( "Pad Pad Pad Pad " ) );
		writer.Write( body.Length );

		foreach ( var word in body )
			writer.Write( word );

		writer.Write( 0 );

		for ( var i = 0; i < 2; ++i )
		{
			var name = Encoding.ASCII.GetBytes( $"VAR_{i}\0" );

			writer.Write( name.Length );
			writer.Write( name );
		}

		writer.Flush();

		return new RideScriptFile( new MemoryStream( memory.ToArray() ) );
	}
}
