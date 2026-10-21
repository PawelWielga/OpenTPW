using Numerics = System.Numerics;

namespace OpenTPW;

/// <summary>
/// Where a visitor a walk holds is drawn, and on which animation: the engine's walk placer, <c>FUN_005580a0</c>, as its
/// stepper <c>FUN_00557d80</c> calls it for every slot in use, every frame (<c>docs/exe/ride-operation.md</c>, "Where a
/// walk's rider is drawn").
/// </summary>
/// <remarks>
/// <para>
/// <b>The stepper picks two nodes and a share of the way between them.</b> Walking on: the walk node to the head node, the
/// head in space <c>0x80</c> under action 4. Walking off: off-from to off-to, off-from in <c>0x80</c> under action 4.
/// Done: off-to, both ends. The share is the leg's progress, <c>( now - start ) × 1000 / ( due - start )</c>, at most a
/// thousand. Carried under action 3, 5 or 6: the head node, in <c>0x800</c>, both ends.
/// </para>
/// <para>
/// <b>Carried under action 1, 2 or 4 the visitor is not drawn</b>: the placer calls the person hide <c>FUN_004f9ed0</c>,
/// which frees the sprite and the thought bubble's. Every other placement goes to <c>FUN_004f9e60</c> with an animation:
/// 1, the walk, for a visitor walking or done; carried, 3 under action 3, 8 under 5 and 5 under 6. Measured in sixteen
/// park files the original wrote: every slot in use holds its visitor's sprite where this puts it, facing as the slot
/// does, or holds none (<c>walkriders</c>).
/// </para>
/// <para>
/// The script's shake (<c>WALKST_FLOAT</c>, <c>+0x84</c>), which turns and lifts a carried visitor under the placer's
/// animation 2, is not built; only Space Zone's Zero G carries it, and it is counted with that instruction.
/// </para>
/// </remarks>
internal static class WalkRiders
{
	/// <summary>
	/// What the placer does with one slot: hide the visitor, or put them between two nodes on an animation. Null for a
	/// slot carried under an action the placer has no arm for (0, or 7 and up), which it leaves where it was.
	/// </summary>
	internal readonly record struct Placement( bool Hidden, int From, uint FromSpace, int To, uint ToSpace, bool Moving,
		bool Along, short Facing, int Animation );

	/// <summary>The engine's walk animation, <c>FUN_004f9e60</c>'s 1 for every visitor not carried.</summary>
	internal const int WalkAnimation = 1;

	/// <summary>Bit 0 of a walk's flags: the height is taken along the line between the two nodes.</summary>
	private const short AlongTheLine = 1;

	/// <summary>The placement <c>FUN_00557d80</c>'s switch gives a slot, and <c>FUN_005580a0</c>'s choice of animation.</summary>
	internal static Placement? Of( RideScript.WalkView slot )
	{
		var head = slot.Action == 4 ? RideNodes.HeadSpace : RideNodes.WalkSpace;
		var along = (slot.Flags & AlongTheLine) != 0;

		switch ( slot.State )
		{
			case RideScript.WalkState.WalkingOn:
				return new Placement( false, slot.WalkNode, RideNodes.WalkSpace, slot.HeadNode, head, true, along, slot.Facing, WalkAnimation );

			case RideScript.WalkState.WalkingOff:
				return new Placement( false, slot.OffFrom, head, slot.OffTo, RideNodes.WalkSpace, true, along, slot.Facing, WalkAnimation );

			case RideScript.WalkState.Done:
				return new Placement( false, slot.OffTo, RideNodes.WalkSpace, slot.OffTo, RideNodes.WalkSpace, false, along, slot.Facing, WalkAnimation );

			case RideScript.WalkState.Carried when slot.Action is 1 or 2 or 4:
				return new Placement( true, slot.WalkNode, RideNodes.WalkSpace, slot.HeadNode, head, false, along, slot.Facing, 0 );

			case RideScript.WalkState.Carried when CarriedAnimation( slot.Action ) is var animation and not 0:
				return new Placement( false, slot.HeadNode, RideNodes.WalkSpace, slot.HeadNode, RideNodes.WalkSpace, false, along,
					slot.Facing, animation );

			case RideScript.WalkState.Carried:
				Unimplemented.Report( "WALK_RIDER_ACTION_UNPLACED" );
				return null;

			default:
				return null;
		}
	}

	/// <summary>
	/// The animation the placer queues for a visitor it places, or null where it queues none: <c>FUN_004f9e60</c> asks
	/// <c>FUN_004d4100</c> whether the sprite is already on it or has it queued, and only otherwise queues it; a visitor
	/// it hides is asked for nothing.
	/// </summary>
	internal static int? Asked( Placement placed, SpriteScript? playing, int queued )
		=> placed.Hidden || playing == null || playing.IsOn( placed.Animation ) || queued == placed.Animation
			? null
			: placed.Animation;

	/// <summary>The animation <c>FUN_005580a0</c> puts a carried visitor on (<c>FUN_005580a0</c>), or nought for none.</summary>
	private static int CarriedAnimation( short action ) => action switch
	{
		3 => 3,
		5 => 8,
		6 => 5,
		_ => 0
	};

	/// <summary>
	/// How far along their leg a visitor walking is at <paramref name="now"/>, in thousandths: <c>FUN_00557d80</c>'s
	/// <c>( now × 1000 − start × 1000 ) / ( due − start )</c>, held to a thousand by the placer. Nought for a slot not
	/// moving, whose two ends are one node.
	/// </summary>
	/// <remarks>
	/// The engine's division is unsigned, so a clock behind the start would wrap; this game's drawing clock is never
	/// behind a start the tick before it stamped, and the share is held at nought.
	/// </remarks>
	internal static int Progress( RideScript.WalkView slot, Placement placed, float now )
	{
		if ( !placed.Moving || slot.Due <= slot.Start )
			return placed.Moving ? 1000 : 0;

		var share = (now - slot.Start) * 1000f / (slot.Due - slot.Start);

		return (int)Math.Clamp( share, 0f, 1000f );
	}

	/// <summary>
	/// The point the placer puts the visitor at, from the two nodes as they stand (model axes, y up): <c>A + ( B − A ) ×
	/// p / 1000</c> as <c>( ( B − A ) × p − A × −1000 ) × 0.001</c> in floats (<c>0x00700fd4</c>, <c>0x00701020</c>), x
	/// and z, and y the same way under flag bit 0. Without it the height would be the surface's under the point
	/// (<c>FUN_004511a0</c>), which no shipped walk asks for: null, counted.
	/// </summary>
	internal static Numerics.Vector3? Point( Placement placed, Numerics.Vector3 from, Numerics.Vector3 to, int progress )
	{
		if ( !placed.Along )
		{
			Unimplemented.Report( "WALK_RIDER_HEIGHT_FROM_THE_SURFACE" );
			return null;
		}

		float Between( float a, float b ) => ((b - a) * progress - a * -1000f) * 0.001f;

		return new Numerics.Vector3( Between( from.X, to.X ), Between( from.Y, to.Y ), Between( from.Z, to.Z ) );
	}
}
