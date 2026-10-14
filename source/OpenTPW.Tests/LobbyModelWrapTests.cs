using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// A model texture wraps only where its frame carries a wrap bit (<c>0x10</c>, <c>0x20</c>) or its mesh scrolls its UVs;
/// every other texture is clamped to its edge, as the original's stage is (<c>LobbyModel.WrapFrames</c>,
/// <c>docs/exe/render-states.md</c>, "Texture addressing").
/// </summary>
[TestClass]
public class LobbyModelWrapTests
{
	private const string Terrain = "levels/jungle/terrain/base.MD2";

	[TestInitialize]
	public void MountTheGame() => FileSystem = GameData.Required();

	/// <summary>
	/// <b>Lost Kingdom's terrain: every texel a UV reaches past the texture is on a wrapping material, and the river's
	/// outer banks clamp.</b> The banks <c>jri_bnk1</c> to <c>3</c> tile their rocks along the river (frame <c>0x11</c>)
	/// and wrap; <c>jri_bnk4</c> to <c>6</c>, whose bottom row is clear, carry <c>0x3</c> and are clamped, so their top
	/// edge does not take in the clear row. Every one of the terrain's eleven scrolling surfaces wraps.
	/// </summary>
	/// <remarks><b>Mutations:</b> the wrap bits read as 0x10 alone; the clamp flag never set; set on every material;
	/// the UV tracks ignored.</remarks>
	[TestMethod]
	public void LostKingdomsTerrainWrapsWhereItsUvsLeaveTheTextureAndClampsTheOuterBanks()
	{
		var model = new ModelFile( Terrain );
		var clips = LobbyModel.LoadAnimations( Terrain );
		var wrapping = LobbyModel.WrapFrames( model, clips );

		int leaving = 0, clamped = 0, outerBanks = 0, tiledBanks = 0;

		foreach ( var mesh in model.Meshes )
		{
			var textures = Enumerable.Range( 0, Material.TextureSlots ).Select( _ => Opaque() ).ToList();
			var substituted = new bool[Material.TextureSlots];

			for ( int i = 0; i < mesh.Vertices.Length; ++i )
			{
				var uv = mesh.TexCoords[i];
				var material = mesh.Materials[(int)mesh.Vertices[i].TextureIndex];

				if ( uv.X < -0.01f || uv.X > 1.01f || uv.Y < -0.01f || uv.Y > 1.01f )
				{
					leaving++;
					Assert.IsTrue( LobbyModel.Wraps( material, wrapping ), $"{mesh.Name}:{material.Name} reaches {uv} and clamps" );
				}
			}

			for ( int m = 0; m < mesh.Materials.Length; ++m )
			{
				var name = mesh.Materials[m].Name.ToLowerInvariant();
				var flags = LobbyModel.MaterialFlagsFor( mesh, textures, substituted, m, null, wrapping );
				var clamps = (flags & LobbyModel.ClampFlag) != 0;

				if ( clamps )
					clamped++;

				if ( name.StartsWith( "jri_bnk4" ) || name.StartsWith( "jri_bnk5" ) || name.StartsWith( "jri_bnk6" ) )
				{
					outerBanks++;
					Assert.IsTrue( clamps, $"{mesh.Name}:{name}, an outer bank, is clamped" );
				}

				if ( name.StartsWith( "jri_bnk1" ) || name.StartsWith( "jri_bnk2" ) || name.StartsWith( "jri_bnk3" ) )
				{
					tiledBanks++;
					Assert.IsFalse( clamps, $"{mesh.Name}:{name}, a tiled bank, wraps" );
				}
			}
		}

		foreach ( var track in clips.SelectMany( clip => clip.UvTracks ) )
		{
			foreach ( var material in model.Meshes[track.TargetIndex].Materials )
				Assert.IsTrue( LobbyModel.Wraps( material, wrapping ), $"{model.Meshes[track.TargetIndex].Name} scrolls and wraps" );
		}

		Assert.IsTrue( leaving > 1000, $"the terrain's UVs leave the texture on {leaving} vertices" );
		Assert.IsTrue( outerBanks >= 27 && tiledBanks >= 27, $"outer banks {outerBanks}, tiled banks {tiledBanks}" );
		Assert.IsTrue( clamped > outerBanks, $"clamped materials {clamped}" );
	}

	/// <summary>
	/// <b>A mesh that scrolls its UVs wraps whatever its frame says</b>, as the loader ORs <c>0x8030</c> into its frames
	/// before the textures load (<c>0x004626fb</c>); without the track the same frame clamps. Another mesh's material on
	/// the same frame wraps too: the bit is the frame's.
	/// </summary>
	/// <remarks><b>Mutations:</b> the UV tracks ignored; Wraps ignoring the frames; MaterialFlagsFor dropping them.</remarks>
	[TestMethod]
	public void AScrollingMeshWrapsWhateverItsFrameSays()
	{
		var model = new ModelFile( Terrain );
		var index = model.Meshes.FindIndex( mesh => mesh.Materials.Any( m => m.Name.ToLowerInvariant().StartsWith( "jri_bnk4" ) ) );
		Assert.IsTrue( index >= 0, "a mesh with an outer bank" );

		var bank = model.Meshes[index].Materials.First( m => m.Name.ToLowerInvariant().StartsWith( "jri_bnk4" ) );
		Assert.AreEqual( 0u, bank.Flags & LobbyModel.WrapBits, "the bank's frame has no wrap bit" );

		Assert.IsFalse( LobbyModel.Wraps( bank, LobbyModel.WrapFrames( model, [] ) ), "no track: clamped" );

		var scrolled = LobbyModel.WrapFrames( model, [Scrolling( index )] );
		Assert.IsTrue( LobbyModel.Wraps( bank, scrolled ), "its mesh scrolls: wrapped" );

		var mesh = model.Meshes[index];
		var textures = Enumerable.Range( 0, Material.TextureSlots ).Select( _ => Opaque() ).ToList();
		var material = System.Array.IndexOf( mesh.Materials, bank );
		uint Flags( IReadOnlySet<uint> wrapping )
			=> LobbyModel.MaterialFlagsFor( mesh, textures, new bool[Material.TextureSlots], material, null, wrapping );

		Assert.AreNotEqual( 0u, Flags( LobbyModel.WrapFrames( model, [] ) ) & LobbyModel.ClampFlag, "its vertices clamp" );
		Assert.AreEqual( 0u, Flags( scrolled ) & LobbyModel.ClampFlag, "scrolled, its vertices wrap" );

		var elsewhere = model.Meshes
			.Where( ( _, m ) => m != index )
			.SelectMany( mesh => mesh.Materials )
			.FirstOrDefault( m => m.FrameOffset == bank.FrameOffset );

		if ( elsewhere != null )
			Assert.IsTrue( LobbyModel.Wraps( elsewhere, scrolled ), "the same frame on another mesh: wrapped" );
	}

	/// <summary>An animation scrolling one mesh, made without a file.</summary>
	private static AnimationFile Scrolling( int target )
	{
		var animation = (AnimationFile)RuntimeHelpers.GetUninitializedObject( typeof( AnimationFile ) );
		var tracks = new List<AnimationFile.UvTrack> { new() { TargetIndex = target } };
		typeof( AnimationFile ).GetField( "<UvTracks>k__BackingField",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic )!.SetValue( animation, tracks );
		return animation;
	}

	/// <summary>A texture as a load would leave it, opaque, made without a device.</summary>
	private static Texture Opaque()
	{
		var texture = (Texture)RuntimeHelpers.GetUninitializedObject( typeof( Texture ) );
		typeof( Texture ).GetProperty( nameof( Texture.Alpha ) )!.SetValue( texture, TextureAlpha.Opaque );
		return texture;
	}
}
