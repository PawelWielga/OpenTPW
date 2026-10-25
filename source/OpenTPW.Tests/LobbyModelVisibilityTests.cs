using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Which meshes a change of clip shows and hides (<see cref="LobbyModel.ShownAtChange"/>), on the shipped clips: the
/// engine's restore shows again what the outgoing clip tracked or listed, then the incoming clip's hide list is hidden.
/// These read real game files and are skipped where there is no installation: see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class LobbyModelVisibilityTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame() => data = GameData.Required();

	private AnimationFile Clip( string path )
	{
		using var stream = new MemoryStream( data.ReadAllBytes( path ) );

		return new AnimationFile( stream );
	}

	private System.Func<int, bool> TransformOnly( string path )
	{
		using var stream = new MemoryStream( data.ReadAllBytes( path ) );
		var model = new ModelFile( stream );

		return node => LobbyModel.IsTransformOnly( model, node );
	}

	/// <summary>
	/// The nodes left hidden: those <paramref name="outgoing"/>'s visibility tracks hide at its last frame, then the
	/// change to <paramref name="incoming"/> applied in its order.
	/// </summary>
	private static SortedSet<int> HiddenAfter( AnimationFile outgoing, AnimationFile incoming, System.Func<int, bool> transformOnly )
	{
		var hidden = new SortedSet<int>();

		foreach ( var track in outgoing.VisibilityTracks )
		{
			if ( track.VisibleAt( outgoing.LastFrame ) == false )
				hidden.Add( track.TargetIndex );
		}

		foreach ( var (node, visible) in LobbyModel.ShownAtChange( outgoing, incoming, transformOnly ) )
		{
			if ( visible )
				hidden.Remove( node );
			else
				hidden.Add( node );
		}

		return hidden;
	}

	/// <summary>
	/// The seaplane's take-off (role 5 entry 2, m3) hides its five meshes from frame 396 of 400 and its landing (m1)
	/// carries no visibility track, so only the restore shows it again: a later visit was drawn as nothing (Q622).
	/// </summary>
	[TestMethod]
	public void TheSeaplanesNextLandingShowsWhatItsTakeOffHid()
	{
		const string at = "levels/jungle/features/seaplane";
		var takeOff = Clip( $"{at}/seaplanem3.md2" );
		var landing = Clip( $"{at}/seaplanem1.md2" );

		var atTheEnd = takeOff.VisibilityTracks.Where( track => track.VisibleAt( takeOff.LastFrame ) == false ).Select( track => track.TargetIndex );

		CollectionAssert.AreEquivalent( new[] { 0, 1, 2, 3, 4 }, atTheEnd.ToArray(), "the take-off ends with all five meshes hidden" );
		Assert.AreEqual( 0, landing.VisibilityTracks.Count, "the landing shows nothing itself" );
		CollectionAssert.AreEqual( new int[0], HiddenAfter( takeOff, landing, TransformOnly( $"{at}/seaplane.md2" ) ).Where( node => node < 5 ).ToArray() );

		// Any track shows its node, not only a visibility track: the landing routes and turns 0 to 2 and shows nothing.
		var taxi = Clip( $"{at}/seaplanem2.md2" );

		CollectionAssert.IsSubsetOf( new[] { (0, true), (1, true), (2, true) },
			LobbyModel.ShownAtChange( landing, taxi, TransformOnly( $"{at}/seaplane.md2" ) ).ToArray() );
	}

	/// <summary>
	/// The Belly Bounce's construction (C) ends with its shell and egg, meshes 6 and 7, hidden, and its idle clip's hide
	/// list names both: the restore shows them and the bind hides them again at once, so they stay put away.
	/// </summary>
	[TestMethod]
	public void TheBellyBouncesShellAndEggStayPutAwayAfterItsBuild()
	{
		const string at = "levels/jungle/rides/bouncy";
		var build = Clip( $"{at}/bouncyc.md2" );
		var idle = Clip( $"{at}/bouncyi.md2" );

		var hidden = HiddenAfter( build, idle, TransformOnly( $"{at}/bouncy.md2" ) );

		Assert.IsTrue( hidden.Contains( 6 ) && hidden.Contains( 7 ), string.Join( " ", hidden ) );
		Assert.IsFalse( hidden.Contains( 5 ), "mesh 5, shown by the build's own track at 144, stays shown" );
	}

	/// <summary>
	/// The Belly Bounce's idle clip (I) lists its shell and egg, meshes 6 and 7, in its hide list and has no track for
	/// either, and its construction clip (C) lists neither: the restore shows both from the outgoing clip's list alone.
	/// </summary>
	[TestMethod]
	public void TheOutgoingClipsHideListIsShownAgain()
	{
		const string at = "levels/jungle/rides/bouncy";
		var transformOnly = TransformOnly( $"{at}/bouncy.md2" );
		var idle = Clip( $"{at}/bouncyi.md2" );
		var build = Clip( $"{at}/bouncyc.md2" );

		Assert.IsTrue( idle.HideList.Contains( (ushort)6 ) && !idle.Tracks.Exists( track => track.Node is 6 or 7 ) );
		Assert.IsFalse( build.HideList.Contains( (ushort)6 ) || build.HideList.Contains( (ushort)7 ) );

		var changes = LobbyModel.ShownAtChange( idle, build, transformOnly ).ToArray();

		CollectionAssert.IsSubsetOf( new[] { (6, true), (7, true) }, changes );
		Assert.IsFalse( changes.Any( change => change.Node is 6 or 7 && !change.Visible ) );
	}

	/// <summary>
	/// A speaker's construction ends with its fern (mesh 2) hidden, and its loop (m1) names none of its meshes: the
	/// restore shows the fern again, as the original's own saves hold every bought speaker on its loop (q622,
	/// meshes 3 and 4 hidden, 2 not). Nodes 3 and 4, in both clips' hide lists, are transform-only and are spared.
	/// </summary>
	[TestMethod]
	public void ASpeakersFernIsShownOnceItIsBuiltAndItsMarkersAreLeftAlone()
	{
		const string at = "levels/jungle/features/speaker1";
		var transformOnly = TransformOnly( $"{at}/speaker1.md2" );
		var build = Clip( $"{at}/speaker1c.md2" );
		var loop = Clip( $"{at}/speaker1m1.md2" );

		Assert.IsTrue( transformOnly( 3 ) && transformOnly( 4 ) && !transformOnly( 2 ) );
		Assert.IsFalse( HiddenAfter( build, loop, transformOnly ).Contains( 2 ) );

		var touched = LobbyModel.ShownAtChange( build, loop, transformOnly ).Select( change => change.Node ).ToArray();

		Assert.IsFalse( touched.Contains( 3 ) || touched.Contains( 4 ), string.Join( " ", touched ) );
	}
}
