using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenTPW.Tests;

using Vector3 = System.Numerics.Vector3;

/// <summary>
/// The scale channel (0x80 with 0x100; FileFormats models.md, "Scale") and how the advisor poses it: Lost Kingdom's golden
/// ticket grows twelvefold through clip 18, holds in 19 and falls to three times in 20, and clips 17 and 18 squash his
/// body. Anim_PoseTrack lerps the two keys about the frame (0x00471d8d) and sets the lengths of the node's three axes to
/// them (Matrix_SetAxisLengths, 0x0046f910). docs/exe/ui.md, "The big ticket is a scale track".
/// </summary>
[TestClass]
public class AdvisorScaleTests
{
	private const string Folder = "levels/jungle/advisor";

	/// <summary>Ticket2, the big ticket's node, in Advisor_JUN.md2.</summary>
	private const int Ticket = 12;

	private const int Body = 13;

	private static AnimationFile Clip( int clip )
		=> new( new MemoryStream( GameData.Required().ReadAllBytes( $"{Folder}/Advisor_JUNm{clip}.MD2" ) ) );

	private static AnimationFile.ScaleTrack TrackOn( AnimationFile clip, int node )
		=> clip.ScaleTracks.Single( track => track.TargetIndex == node );

	private static void AreClose( Vector3 expected, Vector3 actual, string what )
		=> Assert.IsTrue( Vector3.Distance( expected, actual ) < 1e-3f, $"{what}: expected {expected}, got {actual}" );

	private static Vector3 Lengths( Matrix4x4 m )
		=> new( new Vector3( m.M11, m.M12, m.M13 ).Length(), new Vector3( m.M21, m.M22, m.M23 ).Length(), new Vector3( m.M31, m.M32, m.M33 ).Length() );

	/// <summary>Clip 18's four tracks and the ticket's four keys, read as the file gives them.</summary>
	[TestMethod]
	public void ClipEighteenKeysTheTicket()
	{
		var eighteen = Clip( 18 );

		CollectionAssert.AreEqual( new[] { 13, 0, 12, 28 }, eighteen.ScaleTracks.Select( track => track.TargetIndex ).ToArray() );

		var ticket = TrackOn( eighteen, Ticket );
		CollectionAssert.AreEqual( new ushort[] { 0, 38, 70, 89 }, ticket.FrameIndices );
		AreClose( new Vector3( 0.7484f, 0.9635f, 1.1578f ), ticket.Scales[0], "key 0" );
		AreClose( new Vector3( 12.0933f, 11.486f, 8.9577f ), ticket.Scales[2], "key 70" );

		Assert.AreEqual( 2, Clip( 19 ).ScaleTracks.Count );
		Assert.AreEqual( 2, Clip( 17 ).ScaleTracks.Count );
		Assert.AreEqual( 0, Clip( 14 ).ScaleTracks.Count );
	}

	/// <summary>
	/// The two keys about the frame, lerped, the frame's fraction included: half way from 38 to 70 is the mean of their
	/// keys, not either key, and clip 20 halves its fall at frame 12.5.
	/// </summary>
	[TestMethod]
	public void AFrameBetweenKeysLerpsThem()
	{
		var ticket = TrackOn( Clip( 18 ), Ticket );

		Assert.IsTrue( ticket.TrySample( 54f, out var middle ) );
		AreClose( new Vector3( 7.68305f, 7.8499f, 7.0106f ), middle, "frame 54" );

		Assert.IsTrue( ticket.TrySample( 19f, out var early ) );
		AreClose( new Vector3( 2.0106f, 2.58865f, 3.11065f ), early, "frame 19" );

		Assert.IsTrue( ticket.TrySample( 89f, out var last ) );
		AreClose( new Vector3( 12.0933f, 11.486f, 8.9577f ), last, "frame 89" );

		Assert.IsTrue( TrackOn( Clip( 20 ), Ticket ).TrySample( 12.5f, out var falling ) );
		AreClose( new Vector3( 7.5583f, 7.17875f, 5.59855f ), falling, "clip 20 frame 12.5" );

		Assert.IsTrue( TrackOn( Clip( 20 ), Ticket ).TrySample( 40f, out var fallen ) );
		AreClose( new Vector3( 3.0233f, 2.8715f, 2.2394f ), fallen, "clip 20 frame 40" );
	}

	/// <summary>
	/// Before the first key the key search answers nought and no scale is set (Anim_FindKey 0x00470b60), so the node keeps the
	/// lengths its matrix holds after the turn (models.md, "Scale").
	/// 21 of the game's 644 tracks start past frame 0; Lost Kingdom's wateridec keys nodes 15 and 16 from 69.
	/// </summary>
	[TestMethod]
	public void BeforeTheFirstKeyNothingIsSet()
	{
		var track = new AnimationFile.ScaleTrack
		{
			TargetIndex = 3,
			FrameIndices = [25, 50],
			Scales = [new Vector3( 2f, 3f, 4f ), new Vector3( 4f, 5f, 6f )],
		};

		Assert.IsFalse( track.TrySample( 24.9f, out _ ) );
		Assert.IsTrue( track.TrySample( 25f, out var first ) );
		AreClose( new Vector3( 2f, 3f, 4f ), first, "frame 25" );

		var local = new[] { Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.Identity, Matrix4x4.CreateScale( 7f ) };
		var clip = new AnimationFile( new MemoryStream( new byte[0x9c] ) );
		clip.ScaleTracks.Add( track );

		AdvisorModel.Scale( clip, 10f, local );
		AreClose( new Vector3( 7f, 7f, 7f ), Lengths( local[3] ), "before the first key" );

		AdvisorModel.Scale( clip, 37.5f, local );
		AreClose( new Vector3( 3f, 4f, 5f ), Lengths( local[3] ), "half way" );
	}

	/// <summary>
	/// The keyed lengths replace the axes' lengths, they do not multiply them: a node authored at twice its size and turned
	/// comes out at the keyed lengths, its axes still pointing the way they did and its place kept.
	/// </summary>
	[TestMethod]
	public void TheKeysSetTheAxesLengthsAndKeepTheirDirections()
	{
		var local = Matrix4x4.CreateScale( 2f ) * Matrix4x4.CreateRotationZ( 0.7f ) * Matrix4x4.CreateTranslation( 5f, -3f, 9f );

		var scaled = AdvisorModel.WithAxisLengths( local, new Vector3( 3f, 0.5f, 12f ) );

		AreClose( new Vector3( 3f, 0.5f, 12f ), Lengths( scaled ), "lengths" );
		AreClose( Vector3.Normalize( new Vector3( local.M11, local.M12, local.M13 ) ), new Vector3( scaled.M11, scaled.M12, scaled.M13 ) / 3f, "x direction" );
		AreClose( Vector3.Normalize( new Vector3( local.M21, local.M22, local.M23 ) ), new Vector3( scaled.M21, scaled.M22, scaled.M23 ) / 0.5f, "y direction" );
		AreClose( new Vector3( 5f, -3f, 9f ), scaled.Translation, "place" );
	}

	/// <summary>
	/// The advisor's own nodes: at frame 54 of clip 18 the ticket's axes are the keys' mean and his body is back at one
	/// (its keys reach 1 at frame 9), at frame 0 of clip 17 his body is squashed to 1.243, 0.838, 0.838, and a node no scale
	/// track keys keeps its rest lengths.
	/// </summary>
	[TestMethod]
	public void TheAdvisorsNodesTakeTheirKeys()
	{
		var model = new ModelFile( new MemoryStream( GameData.Required().ReadAllBytes( $"{Folder}/Advisor_JUN.md2" ) ) );
		var rest = model.Nodes.Select( node => node.LocalTransform ).ToArray();

		Assert.AreEqual( "Ticket2", model.Meshes[Ticket].Name.TrimEnd( '\0' ).Trim() );

		var local = (Matrix4x4[])rest.Clone();
		AdvisorModel.Scale( Clip( 18 ), 54f, local );

		AreClose( new Vector3( 7.68305f, 7.8499f, 7.0106f ), Lengths( local[Ticket] ), "the ticket at 54" );
		AreClose( Vector3.One, Lengths( local[Body] ), "the body at 54" );
		AreClose( rest[Ticket].Translation, local[Ticket].Translation, "the ticket's place" );
		Assert.AreEqual( rest[25], local[25], "the head pivot, which no scale track keys" );

		// The original's own node at the end of clip 18 (advnodes.py, q707/p597/orig/O/load.log line 279): lengths
		// 12.0933, 11.486, 8.9577 and its place -38.226, 0, 42.049, posed with its turn and its position key.
		local = (Matrix4x4[])rest.Clone();
		AdvisorModel.PoseNodes( Clip( 18 ), 89f, rest, local );
		AreClose( new Vector3( 12.0933f, 11.486f, 8.9577f ), Lengths( local[Ticket] ), "the ticket at 89, posed" );
		AreClose( new Vector3( -38.226f, 0f, 42.049f ), local[Ticket].Translation, "the ticket's place at 89" );

		local = (Matrix4x4[])rest.Clone();
		AdvisorModel.Scale( Clip( 17 ), 0f, local );
		AreClose( new Vector3( 1.243f, 0.838f, 0.838f ), Lengths( local[Body] ), "the body at 0 of clip 17" );
	}
}
