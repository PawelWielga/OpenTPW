using System.Numerics;

namespace OpenTPW;

/// <summary>
/// Poses a model's whole node tree from a clip that moves nodes about: its rotation and position tracks, each node's
/// own transform within its parent, composed down from the root as the engine's pose walk does (<c>FUN_0044ab30</c>,
/// local x parent, <c>FUN_004702d0</c>). <see cref="LobbyModel.Pose"/> uses it for a clip carrying position tracks
/// (<see cref="Drives"/>) and <see cref="MeshRotator"/> for the rest.
///
/// <para>
/// <b>A position key replaces the node's translation within its parent</b> (<c>FUN_00471860</c>, <c>0x0047208b</c>),
/// as a rotation key replaces its orientation, so a node carries everything hanging off it, turned by its parents.
/// Before a track's first key the node is left where it is (<see cref="AnimationFile.PositionTrack.TrySample"/>). On a
/// model whose header word <c>0x30</c> carries <c>0x4</c> (the coaster parts) the point is added to the node's own
/// translation instead (<c>0x0047205d</c>), which the engine puts back every advance (<c>FUN_00473c70</c>), so it is
/// added to the rest.
/// </para>
///
/// <para>
/// <b>A route key puts the node at its route's point</b> (channel <c>0x200</c>): the model's route the node's path id
/// names, sampled at the key's percentage as <see cref="LobbyModel.SampleRoute"/> samples it, is the node's translation
/// within its parent, as the drawn model moves it (<see cref="LobbyModel.Pose"/>), so a node hanging off the bus rides
/// with it. Its facing along the route (<c>0x400</c>) is not built, as the drawn model's is not.
/// </para>
///
/// <para>
/// What a pose leaves is kept: the next pose starts from it, and only <see cref="Rest"/> puts nodes back, as the engine
/// keeps each node's matrix between advances and restores it only on a change of clip (<c>FUN_00472310</c>).
/// </para>
/// </summary>
public sealed class NodePoser
{
	/// <summary>The channels whose nodes a change of clip puts back (<c>FUN_00472310</c>'s mask).</summary>
	private const uint RestoredChannels = 0x289;

	private const uint PositionChannel = 0x1;
	private const uint RotationChannel = 0x8;
	private const uint PathChannel = 0x200;

	private readonly Matrix4x4[] _rest;
	private readonly Matrix4x4[] _restTurnInverse;
	private readonly int[] _parents;
	private readonly bool _adds;
	private readonly IReadOnlyList<ModelFile.ModelPath> _routes;
	private readonly int[] _routeIds;

	private readonly Matrix4x4[] _local;
	private readonly Matrix4x4[] _world;
	private readonly bool[] _composed;

	/// <param name="locals">Each node's own transform within its parent, in the file's axes.</param>
	/// <param name="parents">Each node's parent, or -1 for the root.</param>
	/// <param name="headerFlags">The model's header word at <c>0x30</c> (<see cref="ModelFile.HeaderFlags"/>).</param>
	/// <param name="routes">The model's routes (<see cref="ModelFile.Paths"/>), and <paramref name="routeIds"/> each node's path id.</param>
	public NodePoser( IReadOnlyList<Matrix4x4> locals, IReadOnlyList<int> parents, uint headerFlags,
		IReadOnlyList<ModelFile.ModelPath>? routes = null, IReadOnlyList<int>? routeIds = null )
	{
		_rest = [.. locals];
		_parents = [.. parents];
		_adds = (headerFlags & 0x4) != 0;
		_routes = routes ?? [];
		_routeIds = [.. routeIds ?? []];

		_local = [.. _rest];
		_world = new Matrix4x4[_rest.Length];
		_composed = new bool[_rest.Length];

		// The orientation a rotation key replaces is the node's own, taken out before the key goes in - see
		// MeshRotator.BuildRestInverses, which this matches so that both turn a node alike.
		_restTurnInverse = new Matrix4x4[_rest.Length];
		for ( int i = 0; i < _rest.Length; ++i )
		{
			_restTurnInverse[i] = Matrix4x4.Decompose( _rest[i], out _, out var turn, out _ )
				&& Matrix4x4.Invert( Matrix4x4.CreateFromQuaternion( turn ), out var inverse )
					? inverse
					: Matrix4x4.Identity;
		}

		Compose();
	}

	public NodePoser( ModelFile model )
		: this( [.. model.Nodes.Select( node => node.LocalTransform )], [.. model.Nodes.Select( node => node.ParentIndex )],
			model.HeaderFlags, model.Paths, [.. model.Nodes.Select( node => node.PathId )] )
	{
	}

	/// <summary>Whether a clip is posed here rather than by <see cref="MeshRotator"/>: one that moves a node.</summary>
	public static bool Drives( AnimationFile clip ) => clip.PositionTracks.Count > 0;

	/// <summary>Where node <paramref name="node"/> stands in the model, as last posed, in the file's axes.</summary>
	public Matrix4x4 World( int node ) => _world[node];

	/// <summary>Node <paramref name="node"/>'s own transform within its parent, as last posed.</summary>
	public Matrix4x4 Local( int node ) => _local[node];

	/// <summary>Poses every track <paramref name="clip"/> carries at <paramref name="frame"/>, then the whole tree.</summary>
	public void Pose( AnimationFile clip, float frame )
	{
		if ( _adds )
		{
			// FUN_00470d10 composes an additive model's rotation key with the node's own; not decoded here. One
			// rotation track in the 22 additive clips the game ships.
			if ( clip.RotationTracks.Count > 0 )
				Unimplemented.Report( "ANIM_ADDITIVE_ROTATION" );
		}
		else
		{
			foreach ( var track in clip.RotationTracks )
			{
				var node = track.TargetIndex;

				if ( !Holds( node ) || track.Rotations.Length == 0 )
					continue;

				// Sampled as MeshRotator samples it, held at the first key before that key's frame, where the engine's key
				// search answers nought and skips the track (Anim_FindKey) - not the original's, not measured here.
				var at = _local[node].Translation;
				var turned = Matrix4x4.CreateFromQuaternion( track.Sample( frame ) ) * _restTurnInverse[node] * _rest[node];

				turned.Translation = at;
				_local[node] = turned;
			}
		}

		foreach ( var track in clip.PositionTracks )
		{
			var node = track.TargetIndex;

			if ( !Holds( node ) || !track.TrySample( frame, out var point ) )
				continue;

			var local = _local[node];
			local.Translation = _adds ? _rest[node].Translation + point : point;
			_local[node] = local;
		}

		foreach ( var track in clip.PathTracks )
		{
			var node = track.TargetIndex;
			var which = node >= 0 && node < _routeIds.Length ? _routeIds[node] : 0;

			if ( !Holds( node ) || which < 0 || which >= _routes.Count )
				continue;

			var local = _local[node];
			local.Translation = LobbyModel.SampleRoute( _routes[which], track.Sample( frame ) );
			_local[node] = local;
		}

		Compose();
	}

	/// <summary>
	/// Puts back the nodes <paramref name="outgoing"/> moved, all but those <paramref name="incoming"/> moves on the
	/// same channel, which the engine leaves as the outgoing clip left them (<c>FUN_00472310</c>: the outgoing clip's
	/// channels per node, less the incoming's, against <c>0x289</c>; the node's whole transform comes back).
	/// </summary>
	public void Rest( AnimationFile outgoing, AnimationFile? incoming )
	{
		var leaving = Channels( outgoing );

		if ( incoming != null )
		{
			foreach ( var (node, channels) in Channels( incoming ) )
			{
				if ( leaving.ContainsKey( node ) )
					leaving[node] &= ~channels;
			}
		}

		foreach ( var (node, channels) in leaving )
		{
			if ( (channels & RestoredChannels) != 0 && Holds( node ) )
				_local[node] = _rest[node];
		}

		Compose();
	}

	/// <summary>The channels <paramref name="clip"/> drives on each node, of those this project reads.</summary>
	private static Dictionary<int, uint> Channels( AnimationFile clip )
	{
		var channels = new Dictionary<int, uint>();

		void Add( int node, uint channel ) => channels[node] = channels.GetValueOrDefault( node ) | channel;

		foreach ( var track in clip.RotationTracks )
			Add( track.TargetIndex, RotationChannel );

		foreach ( var track in clip.PositionTracks )
			Add( track.TargetIndex, PositionChannel );

		foreach ( var track in clip.PathTracks )
			Add( track.TargetIndex, PathChannel );

		return channels;
	}

	private bool Holds( int node ) => node >= 0 && node < _local.Length;

	private void Compose()
	{
		Array.Clear( _composed );

		for ( int node = 0; node < _local.Length; ++node )
			ComposeNode( node, _local.Length );
	}

	private Matrix4x4 ComposeNode( int node, int guard )
	{
		if ( _composed[node] )
			return _world[node];

		// A parent chain that loops or leaves the tree ends at the node's own transform, as ModelFile's walk does.
		var parent = _parents[node];

		_world[node] = guard <= 0 || parent < 0 || parent >= _local.Length || parent == node
			? _local[node]
			: _local[node] * ComposeNode( parent, guard - 1 );

		_composed[node] = true;
		return _world[node];
	}
}
