using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Numerics;

namespace OpenTPW.Tests;

using Vector3 = System.Numerics.Vector3;

/// <summary>
/// A clip's position tracks, posed over the model's node tree - <see cref="NodePoser"/> (docs/exe/park.md, "Clip content";
/// FileFormats models.md, "Position (bit 0x1)"). The trees here are made so that a wrong rule lands somewhere else: a
/// turned root, so a key read in the world's axes rather than its parent's misses; a child, so a node that does not
/// carry what hangs off it leaves it behind; a first key past nought, so a sampler that clamps moves the node early.
/// </summary>
[TestClass]
public class NodePoserTests
{
	/// <summary>A root a quarter turn about y at (480, 0, 170), a plane under it at (0, 59, 273), a propeller under that at (6, 7, 3).</summary>
	private static NodePoser Tree( uint headerFlags = 0 )
	{
		var root = Matrix4x4.CreateRotationY( MathF.PI / 2 ) * Matrix4x4.CreateTranslation( 480, 0, 170 );

		return new NodePoser(
			[Matrix4x4.CreateTranslation( 0, 59, 273 ), Matrix4x4.CreateTranslation( 6, 7, 3 ), root],
			[2, 0, -1], headerFlags );
	}

	private static AnimationFile Clip( params AnimationFile.PositionTrack[] tracks )
	{
		var clip = (AnimationFile)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject( typeof( AnimationFile ) );

		foreach ( var property in new[] { "PositionTracks", "RotationTracks", "PathTracks" } )
		{
			var field = typeof( AnimationFile ).GetField( $"<{property}>k__BackingField",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic )!;
			field.SetValue( clip, field.FieldType.GetConstructor( Type.EmptyTypes )!.Invoke( null ) );
		}

		clip.PositionTracks.AddRange( tracks );
		return clip;
	}

	/// <summary>The plane, node 0, flown from (0, 59, 273) at frame 10 to (0, 59, -77) at frame 20 in a straight line.</summary>
	private static AnimationFile.PositionTrack Flight( int node = 0 ) => new()
	{
		TargetIndex = node,
		FrameIndices = [10, 20],
		Points = [new( 0, 59, 273 ), new( 0, 59, -77 )],
		IsBezier = false
	};

	private static void Near( Vector3 expected, Vector3 actual, string what, float within = 0.01f )
		=> Assert.IsTrue( Vector3.Distance( expected, actual ) < within, $"{what}: expected {expected}, got {actual}" );

	/// <summary>
	/// Halfway, the plane's key (0, 59, 98) is within its parent: the root's quarter turn takes z to x, so it stands at
	/// (578, 59, 170). Read in the world's axes it would stand at (480, 59, 268).
	/// </summary>
	[TestMethod]
	public void AKeyIsWithinTheParent()
	{
		var poser = Tree();

		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "at rest" );

		poser.Pose( Clip( Flight() ), 15 );

		Near( new( 480 + 98, 59, 170 ), poser.World( 0 ).Translation, "halfway" );
		Near( new( 0, 59, 98 ), poser.Local( 0 ).Translation, "its own translation" );
	}

	/// <summary>The propeller rides 6, 7 and 3 off the plane wherever the plane flies: at the end of the flight (-77 + 3) turned to x.</summary>
	[TestMethod]
	public void ANodeCarriesWhatHangsOffIt()
	{
		var poser = Tree();

		poser.Pose( Clip( Flight() ), 20 );

		Near( new( 480 - 77, 59, 170 ), poser.World( 0 ).Translation, "the plane" );
		Near( new( 480 - 77 + 3, 66, 170 - 6 ), poser.World( 1 ).Translation, "the propeller" );
	}

	/// <summary>
	/// Before the first key (frame 10) the node is left where it is: at rest on a fresh pose, and where the last pose put it
	/// after one - frame 9.9 floors to 9, still before. A sampler that clamps to the first point would put it at 273 on
	/// both, which is the rest here only by chance, so the second pose is the one that tells.
	/// </summary>
	[TestMethod]
	public void BeforeTheFirstKeyTheNodeStaysWhereItIs()
	{
		var poser = Tree();
		var clip = Clip( Flight() );

		poser.Pose( clip, 5 );
		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "fresh" );

		poser.Pose( clip, 20 );
		poser.Pose( clip, 9.9f );
		Near( new( 480 - 77, 59, 170 ), poser.World( 0 ).Translation, "after the clip came round again" );

		poser.Pose( clip, 10 );
		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "on the first key" );
	}

	/// <summary>
	/// A change of clip puts the node back, unless the incoming clip moves it too, when it stays where the outgoing left
	/// it until the incoming's first key (FUN_00472310).
	/// </summary>
	[TestMethod]
	public void AChangeOfClipPutsBackWhatTheIncomingDoesNotMove()
	{
		var poser = Tree();
		var outgoing = Clip( Flight() );

		poser.Pose( outgoing, 20 );
		poser.Rest( outgoing, Clip( Flight() ) );
		Near( new( 480 - 77, 59, 170 ), poser.World( 0 ).Translation, "incoming moves it too" );

		poser.Rest( outgoing, Clip( Flight( 1 ) ) );
		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "incoming moves another node" );

		poser.Pose( outgoing, 20 );
		poser.Rest( outgoing, null );
		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "no incoming" );
	}

	/// <summary>
	/// A rotation key replaces the node's own turn and carries what hangs off it. The plane rests turned a half turn,
	/// keyed a quarter: the propeller's (6, 7, 3) turns to (3, 7, -6) about the plane, which still stands at its rest place
	/// (frame 5, before the flight's first key), and the root's quarter turn takes the whole to (747, 66, 167); the turn
	/// leaves the plane's place alone. Keyed on
	/// top of the rest turn it would land at (753, 66, 173); left unturned, at (756, 66, 164).
	/// </summary>
	[TestMethod]
	public void ARotationKeyTurnsTheNodeAndWhatHangsOffIt()
	{
		var root = Matrix4x4.CreateRotationY( MathF.PI / 2 ) * Matrix4x4.CreateTranslation( 480, 0, 170 );
		var poser = new NodePoser(
			[Matrix4x4.CreateRotationY( MathF.PI ) * Matrix4x4.CreateTranslation( 0, 59, 273 ), Matrix4x4.CreateTranslation( 6, 7, 3 ), root],
			[2, 0, -1], 0 );

		var quarter = new AnimationFile.RotationTrack
		{
			TargetIndex = 0,
			FrameIndices = [0],
			Rotations = [Quaternion.CreateFromAxisAngle( Vector3.UnitY, MathF.PI / 2 )]
		};

		var clip = Clip( Flight() );
		clip.RotationTracks.Add( quarter );

		poser.Pose( clip, 5 );

		Near( new( 480 + 273, 59, 170 ), poser.World( 0 ).Translation, "the plane" );
		Near( new( 747, 66, 167 ), poser.World( 1 ).Translation, "the propeller" );

		// The turn keeps whatever place the node holds: flown to the end and come round before the first key again.
		poser.Pose( clip, 20 );
		poser.Pose( clip, 5 );
		Near( new( 480 - 77, 59, 170 ), poser.World( 0 ).Translation, "the plane where the flight left it" );

		var turnsOnly = Clip();
		turnsOnly.RotationTracks.Add( quarter );

		Assert.IsTrue( NodePoser.Drives( clip ) );
		Assert.IsFalse( NodePoser.Drives( turnsOnly ), "a clip that only turns is MeshRotator's" );
	}

	/// <summary>A model whose header carries 0x4 adds the key to the node's own translation: 273 + 98 halfway, from the rest every time.</summary>
	[TestMethod]
	public void AnAdditiveModelAddsTheKeyToTheRest()
	{
		var poser = Tree( 0x5 );
		var clip = Clip( Flight() );

		poser.Pose( clip, 15 );
		poser.Pose( clip, 15 );

		Near( new( 0, 118, 273 + 98 ), poser.Local( 0 ).Translation, "added once, not twice" );
	}

	/// <summary>
	/// The End's first clip flies its first plane, <c>_f_endplane02</c> (mesh 0), from where it rests, (480.3, 59, 443.4),
	/// down the park to the clip's last key, 353 nearer the gate along z; its propeller <c>prop08</c> keeps its distance
	/// from it, as it does at rest. Read from the game's own <c>end.wad</c>.
	/// </summary>
	[TestMethod]
	public void TheEndsFirstPlaneFliesAndTakesItsPropellers()
	{
		var data = GameData.Required();

		using var modelStream = data.OpenRead( "levels/jungle/features/end/End.MD2" )!;
		using var clipStream = data.OpenRead( "levels/jungle/features/end/Endm1.MD2" )!;

		var model = new ModelFile( modelStream );
		var clip = new AnimationFile( clipStream );
		var poser = new NodePoser( model );
		var track = clip.PositionTracks.Single( track => track.TargetIndex == 0 );
		var last = track.FrameIndices[^1];

		Assert.IsTrue( NodePoser.Drives( clip ) );

		var rest = poser.World( 0 ).Translation;
		var apart = Vector3.Distance( poser.World( 1 ).Translation, rest );

		Near( new( 480.3f, 59f, 443.4f ), rest, "the plane at rest", 0.1f );
		Near( model.Nodes[0].WorldTransform.Translation, rest, "as the file composes it" );

		poser.Pose( clip, last );

		var flown = poser.World( 0 ).Translation;

		Near( rest + new Vector3( 0, 0, -353 ), flown, $"the plane on frame {last}", 0.1f );
		Assert.AreEqual( apart, Vector3.Distance( poser.World( 1 ).Translation, flown ), 0.01f, "the propeller keeps its distance" );
	}
}
