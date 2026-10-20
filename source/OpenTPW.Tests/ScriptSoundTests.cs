using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTPW.Tests;

/// <summary>
/// The sounds a ride script starts with <c>ADDOBJ</c> and <c>EVENT</c>, and stops with <c>KILLOBJ</c>, <c>FADEOBJ</c>
/// and its end (<c>docs/exe/audio.md</c>, "A script's sounds"), played with a device stood in as
/// <see cref="VoicePlacementTests"/> stands one in.
/// </summary>
[TestClass]
public class ScriptSoundTests
{
	private const string Theme = "jungle";

	/// <summary>
	/// The class an effect's flags word makes (<c>0x006b6774</c>..<c>0x006b67f2</c>): bit <c>0x4</c> clear is a one-shot
	/// whatever else is set, and with it set <c>0x10</c> wins, then <c>0x2</c> splits the rest by <c>0x400</c> and <c>0x100</c>.
	/// </summary>
	[DataTestMethod]
	[DataRow( 0x0, "OneShot", null )]
	[DataRow( 0x8, "OneShot", null )]
	[DataRow( 0x208, "OneShot", null )]
	[DataRow( 0x412, "OneShot", null )]
	[DataRow( 0x6, "Plain", null )]
	[DataRow( 0x206, "Plain", null )]
	[DataRow( 0x406, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BE610" )]
	[DataRow( 0x606, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BE610" )]
	[DataRow( 0x106, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BE330" )]
	[DataRow( 0x416, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BE680" )]
	[DataRow( 0x404, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BDD30" )]
	[DataRow( 0x104, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BDC60" )]
	[DataRow( 0x4, "NotBuilt", "RIDE_EFFECT_SOUND_CLASS_6BDC60" )]
	public void AnEffectsFlagsPickItsVoicesClass( int flags, string kind, string? gap )
	{
		var (made, named) = ParkAudio.ClassOf( flags );

		Assert.AreEqual( kind, made.ToString() );
		Assert.AreEqual( gap, named );
	}

	private static int Word( Opcode opcode ) => unchecked( (int)( 0x80000000u | (uint)opcode ) );

	/// <summary>A whole .RSE file of one variable, as <see cref="RideScriptEffectTests"/> builds one.</summary>
	private static RideScriptFile Build( params int[] body )
	{
		using var memory = new MemoryStream();
		using var writer = new BinaryWriter( memory );

		writer.Write( Encoding.ASCII.GetBytes( "RSSE" ) );
		writer.Write( 0x00010F51 );
		writer.Write( 1 );
		writer.Write( 8 );
		writer.Write( 50 );
		writer.Write( 0 );
		writer.Write( 0 );
		writer.Write( 0 );
		writer.Write( Encoding.ASCII.GetBytes( "Pad Pad Pad Pad " ) );
		writer.Write( body.Length );

		foreach ( var word in body )
			writer.Write( word );

		writer.Write( 0 );

		var name = Encoding.ASCII.GetBytes( "VAR_0\0" );

		writer.Write( name.Length );
		writer.Write( name );
		writer.Flush();

		return new RideScriptFile( new MemoryStream( memory.ToArray() ) );
	}

	private readonly List<Voice> _before = [];

	/// <summary>Runs <paramref name="play"/> with a device stood in and a park's audio up, and takes away every voice it made.</summary>
	private void WithAParksAudio( Action<ParkAudio> play )
	{
		FileSystem = GameData.Required();
		Log ??= new();
		Unimplemented.Forget();

		var ready = typeof( Audio ).GetProperty( nameof( Audio.Ready ) )!;

		Assert.IsFalse( Audio.Ready, "a test run has no audio device" );

		lock ( Audio.Lock )
			_before.AddRange( Audio.Voices );

		ParkAudio? park = null;

		try
		{
			ready.SetValue( null, true );
			park = new ParkAudio( Theme );
			play( park );
		}
		finally
		{
			park?.Delete();
			Entity.ApplyDeletions();

			lock ( Audio.Lock )
				Audio.Voices.RemoveAll( voice => !_before.Contains( voice ) );

			ready.SetValue( null, false );
			Unimplemented.Forget();
		}
	}

	private static bool Counted( string what ) => Unimplemented.Summary.Any( entry => entry.What == what );

	/// <summary>
	/// <b>Each type plays in its own category, and a stop takes what its tag names.</b> On the End's model: the global
	/// ambient's 209 looped (flags <c>0x8</c>) on node 1 over its variation's 100; the interface's 186 flat (<c>0x208</c>);
	/// the global rides' 200 of the plain class (<c>0x6</c>) on node 9 over 170, and 202 a one-shot that keeps no record.
	/// The level's ambient holds no 6, so its record is freed, while the global ambient holds a 6; its 177 is a held
	/// chain, counted and kept. A placed sound of no node, with no placed thing to stand on, is counted and kept.
	/// </summary>
	[TestMethod]
	public void EachTypePlaysInItsOwnCategoryAndAStopTakesItsTag()
	{
		WithAParksAudio( park =>
		{
			var nodes = RideNodes.Load( "levels/jungle/features/end", "End", (BaseFileSystem)FileSystem, false, [] )!;
			var script = new RideScript( Build(
				Word( Opcode.ADDOBJ ), 8, 1, 209, 1,
				Word( Opcode.ADDOBJ ), 9, -1, 186, 2,
				Word( Opcode.ADDOBJ ), 5, 9, 200, 3,
				Word( Opcode.ADDOBJ ), 4, -1, 6, 4,
				Word( Opcode.ADDOBJ ), 4, -1, 177, 5,
				Word( Opcode.ADDOBJ ), 5, -1, 201, 6,
				Word( Opcode.EVENT ), 5, 10, 202,
				Word( Opcode.END ) ) ) { Effects = new RideEffects(), Nodes = nodes, Id = 903 };

			script.Turn( 0f );

			var effects = script.Effects!;

			CollectionAssert.AreEqual( new[] { 6, 5, 3, 2, 1 }, effects.Records.Select( record => record.Tag ).ToArray(),
				"the level ambient's 6 is freed, every other record kept" );
			Assert.AreEqual( 1, effects.Freed );
			Assert.AreEqual( (4, 1, 0, 0, 0), park.ScriptSounds );
			Assert.IsTrue( Counted( "RIDE_EFFECT_SOUND_CLASS_6BDD30" ), "the gates' 177 is a held chain" );
			Assert.IsNull( effects.Records[1].Sound, "and has no voice" );
			Assert.IsTrue( Counted( "RIDE_EFFECT_SOUND_NO_PLACE" ), "201 has nowhere to stand" );
			Assert.IsNull( effects.Records[0].Sound );

			var looped = effects.Records.Single( record => record.Tag == 1 ).Sound!;
			var flat = effects.Records.Single( record => record.Tag == 2 ).Sound!;
			var plain = effects.Records.Single( record => record.Tag == 3 ).Sound!;

			Assert.IsTrue( looped.Looped && !looped.Plain );
			Assert.AreEqual( new Vector3( 0f, 271f, 61f ), looped.Place, "node 1 at (0.79, 61.63, 271.85) on the model" );
			Assert.AreEqual( 100f, looped.Voice!.Range );
			Assert.IsTrue( looped.Voice.Loop );

			Assert.IsNull( flat.Place, "cat_ui's 186 is flagged 0x200" );
			Assert.IsFalse( flat.Voice!.IsPlaced );

			Assert.IsTrue( plain.Plain && !plain.Looped );
			Assert.AreEqual( new Vector3( 21f, 57f, 16f ), plain.Place );
			Assert.AreEqual( 170f, plain.Voice!.Range );
			Assert.AreEqual( 1, plain.Samples );

			Assert.AreEqual( 4, park.ScriptVoices.Count, "the four started, the event's one-shot among them" );

			var kill = new RideScript( Build( Word( Opcode.KILLOBJ ), 1, Word( Opcode.FADEOBJ ), 3, Word( Opcode.END ) ) );

			// The two stops run on the first script's own list, as the engine's handlers walk the frame's +0xb0.
			script.Effects = null;
			kill.Effects = effects;
			kill.Turn( 0f );

			CollectionAssert.AreEqual( new[] { 6, 5, 2 }, effects.Records.Select( record => record.Tag ).ToArray() );
			Assert.AreEqual( (1, 1), (effects.Stopped, effects.Faded) );
			Assert.AreEqual( (4, 1, 1, 1, 0), park.ScriptSounds );
			Assert.IsTrue( looped.Stopped && plain.Stopped && !flat.Stopped );
			Assert.IsFalse( looped.Voice.Playing, "a kill is a hard stop" );
			Assert.IsTrue( plain.Voice.Ending, "a fade is under way" );

			// And the end of a script stops what is left.
			kill.StopEffects();

			Assert.IsTrue( flat.Stopped );
			Assert.AreEqual( (4, 1, 2, 1, 0), park.ScriptSounds );
		} );
	}

	/// <summary>
	/// <b>A sound on a node stands at the node and is moved only on its script's eighth tick.</b> The End's 201 on its
	/// sound node 10, at (-21.78, 16.59, 57.14) on its model: its height last in the world's axes and each coordinate cut
	/// toward nought; a node the model does not hold frees the record (the engine's "RSSE: Invalid Node ID").
	/// </summary>
	[TestMethod]
	public void ASoundOnANodeStandsThereAndIsMovedOnItsScriptsEighthTick()
	{
		WithAParksAudio( park =>
		{
			var nodes = RideNodes.Load( "levels/jungle/features/end", "End", (BaseFileSystem)FileSystem, false, [] )!;

			var script = new RideScript( Build(
				Word( Opcode.ADDOBJ ), 5, 10, 201, 1,
				Word( Opcode.ADDOBJ ), 5, 40, 201, 2,
				Word( Opcode.END ) ) ) { Effects = new RideEffects(), Nodes = nodes, Id = 904 };

			script.Turn( 0f );

			var record = script.Effects!.Records.Single();

			Assert.AreEqual( 1, record.Tag, "node 40 is not the End's, so its record is freed" );

			var sound = record.Sound!;
			var at = NodePlace;

			Assert.AreEqual( at, sound.Place );
			Assert.AreEqual( at, sound.Voice!.Place );

			var elsewhere = new Vector3( 1f, 2f, 3f );

			park.MoveScriptSound( sound, elsewhere );
			script.SweepEffects( soundsDue: false );

			Assert.AreEqual( elsewhere, sound.Place, "not moved on another tick" );

			script.SweepEffects( soundsDue: true );

			Assert.AreEqual( at, sound.Place, "moved back to its node on its own" );
			Assert.AreEqual( at, sound.Voice.Place );
			Assert.AreEqual( 2, park.ScriptSounds.Moved );
		} );
	}

	/// <summary>
	/// <b>The scheduler moves a script's sounds on that script's own eighth tick and no other</b> (<c>0x00551a1c</c>: the id
	/// against the tick, as a turn is given). Script 3 takes its turns on ticks 3 and 11; its 201 is put elsewhere after the
	/// first, stays there through ticks 4 to 10, and is back on its node on tick 11.
	/// </summary>
	[TestMethod]
	public void TheSchedulerMovesASoundOnlyOnItsScriptsOwnTick()
	{
		WithAParksAudio( park =>
		{
			var nodes = RideNodes.Load( "levels/jungle/features/end", "End", (BaseFileSystem)FileSystem, false, [] )!;
			var script = new RideScript( Build(
				Word( Opcode.ADDOBJ ), 5, 10, 201, 1,
				Word( Opcode.ENDSLICE ), Word( Opcode.BRANCH ), 0x20000005 ) ) { Effects = new RideEffects(), Nodes = nodes, Id = 3 };
			var scheduler = new RideScriptScheduler();

			scheduler.Add( 3, script );

			for ( var tick = 1; tick <= 3; ++tick )
				scheduler.Advance( 0f );

			var sound = script.Effects!.Records.Single().Sound!;
			var elsewhere = new Vector3( 1f, 2f, 3f );

			Assert.AreEqual( NodePlace, sound.Place, "started on its node on tick 3" );
			park.MoveScriptSound( sound, elsewhere );

			for ( var tick = 4; tick <= 10; ++tick )
			{
				scheduler.Advance( 0f );
				Assert.AreEqual( elsewhere, sound.Place, $"not moved on tick {tick}" );
			}

			scheduler.Advance( 0f );
			Assert.AreEqual( NodePlace, sound.Place, "moved back on tick 11" );
		} );
	}

	/// <summary>The End's sound node 10 on its model, in whole units of the world, the height last.</summary>
	private static readonly Vector3 NodePlace = new( -21f, 57f, 16f );
}
