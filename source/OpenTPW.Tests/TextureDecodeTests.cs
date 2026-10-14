using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Bullfrog's .wct decoder, over the art the game actually ships. These read real game files and are
/// skipped where there is no installation - see <see cref="GameData"/>.
///
/// <para>
/// The decode's two scratch buffers are sized to the indices the decode really reaches, and this is
/// what pins that. The failure mode is not a quiet one: a buffer an element short throws out of the
/// list indexer, so decoding every texture a model names is enough to catch it.
/// </para>
///
/// <para>
/// <b>This is regression cover, not the proof.</b> What proves the decode right is the park on the
/// screen; all four themes were loaded and looked at. A green run here only says nothing threw.
/// </para>
///
/// <para>
/// <b>Mutation-checked:</b> undersizing either of the LZSS path's buffers (<c>DequantizationBuffer</c>
/// or <c>RowDecodeBuffer</c> at <c>size*size/2</c>) fails this test.
/// </para>
/// </summary>
[TestClass]
public class TextureDecodeTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame() => data = GameData.Required();

	/// <summary>
	/// Every distinct texture a model's materials name, as paths beside the model.
	///
	/// Taken from the model rather than written down here, so the set cannot drift away from what is
	/// really loaded - this is the same <c>{directory}/{name}.wct</c> rule
	/// <see cref="LobbyModel.LoadTexture"/> follows, names untrimmed exactly as it leaves them.
	/// </summary>
	private List<string> TexturesNamedBy( string modelPath, string textureDirectory )
	{
		var model = new ModelFile( new MemoryStream( data.ReadAllBytes( modelPath ) ) );

		return model.Meshes
			.SelectMany( mesh => mesh.Materials )
			.Select( material => material.Name )
			.Where( name => !string.IsNullOrWhiteSpace( name ) )
			.Distinct()
			.Select( name => $"{textureDirectory}/{name}.wct" )
			.Where( Exists )
			.ToList();
	}

	/// <summary>
	/// Whether the file system can offer this path at all.
	///
	/// <b>Asked with <c>GetSize</c> rather than <c>FileExists</c>, because that one does not look
	/// inside archives</b> and every one of these lives in a .wad - so <c>FileExists</c> answers no to
	/// all of them and the set comes back empty. This is the same helper, for the same reason, as
	/// <see cref="LobbyModel"/>'s own <c>Exists</c>.
	/// </summary>
	private bool Exists( string path )
	{
		try
		{
			return data.GetSize( path ) > 0;
		}
		catch ( System.Exception )
		{
			return false;
		}
	}

	/// <summary>
	/// Every texture the jungle's terrain names decodes, to the size its own header declares.
	///
	/// <para>
	/// <c>base.MD2</c> is the right model to do this with: at 272 meshes it is the largest in the
	/// game, it names more distinct textures than anything else, and decoding its textures once per
	/// mesh that uses them would make a park load take twenty seconds. Nothing is asserted about
	/// the PIXELS - what this pins is that every shipped texture survives the decode at all, and that
	/// the decode agrees with the header about how big it is.
	/// </para>
	/// </summary>
	/// <summary>
	/// <b>The header's scale at offset 10 is Cr's and the one at 12 is Cb's</b> (<c>0x00564e20</c> hands them to
	/// the decoder as Y, offset 12, offset 10; <c>docs/exe/hud.md</c>, "The stat bars' skin"). <c>ridestatbar.wct</c>
	/// is the file whose two differ most among those measured on the original's screen (10 and 2): its ramp runs
	/// red, orange, yellow, green there, (255, 14, 0) at texel 1, (255, 103, 0) at 4, (255, 206, 0) at 8 and (90, 251, 0)
	/// at 24. Texel 1 is a row's end, where the D4's wrap reaches round to the row's last values.
	/// </summary>
	/// <remarks><b>Mutations:</b> the two scales read the other way round; Cr's scale taken for both.</remarks>
	[TestMethod]
	public void TheStatBarsRampDecodesToTheOriginalsColours()
	{
		var texture = new TextureFile( new MemoryStream( data.ReadAllBytes( "ui/textures/ridestatbar.wct" ) ) ).Data;

		(int R, int G, int B) At( int x )
		{
			var i = (4 * texture.Width + x) * 4;
			return (texture.Data[i], texture.Data[i + 1], texture.Data[i + 2]);
		}

		foreach ( var (x, want) in new[] { (1, (255, 14, 0)), (4, (255, 103, 0)), (8, (255, 206, 0)), (24, (90, 251, 0)) } )
		{
			var got = At( x );

			Assert.IsTrue( Math.Abs( got.R - want.Item1 ) <= 15 && Math.Abs( got.G - want.Item2 ) <= 15 && Math.Abs( got.B - want.Item3 ) <= 15,
				$"texel {x} decoded {got}, the original's {want}" );
		}
	}

	/// <summary>
	/// <b>A type 0x12 texture decodes as the original's zlib codec does, to the byte</b> (<c>FUN_00593ab0</c>;
	/// <c>docs/exe/textures.md</c>, "The zlib codec"). Each channel's sum is <c>wctorig.py</c>'s, the codec written
	/// again in Python from the listing: <c>ridestatbar.wct</c> (32 x 32, no alpha) and <c>b_1person.wct</c> (51 x 51
	/// out of a 64 x 64 plane, its alpha 0 to 255).
	/// </summary>
	/// <remarks><b>Mutations:</b> each of the four D4 coefficients swapped with another; smooth and detail halves the
	/// other way round; the wrap to the line's first value; rows only; a value truncated, not rounded; the chroma
	/// taken from the next block; the 0x80 escape read big-endian; the alpha plane's multiplier the luma's.</remarks>
	[TestMethod]
	public void ATypeTwelveTextureDecodesAsTheOriginalsZlibCodecToTheByte()
	{
		foreach ( var (path, want) in new[]
		{
			("ui/textures/ridestatbar.wct", new long[] { 116101, 128086, 29670, 261120 }),
			("ui/stexture/b_1person.wct", new long[] { 328951, 386376, 432787, 278729 }),
		} )
		{
			var texture = new TextureFile( new MemoryStream( data.ReadAllBytes( path ) ) ).Data;
			var sums = Enumerable.Range( 0, 4 ).Select( c => texture.Data.Where( ( _, i ) => i % 4 == c ).Sum( b => (long)b ) ).ToArray();

			CollectionAssert.AreEqual( want, sums, $"{path}'s R, G, B, A sums {string.Join( ", ", sums )}" );
		}
	}

	[TestMethod]
	public void EveryTextureTheJungleTerrainNamesDecodes()
	{
		var wanted = TexturesNamedBy( "levels/jungle/terrain/base.MD2", "levels/jungle/terrain/textures" );

		Assert.IsTrue( wanted.Count >= 40,
			$"the jungle terrain should name dozens of textures; found {wanted.Count}" );

		foreach ( var path in wanted )
		{
			var texture = new TextureFile( new MemoryStream( data.ReadAllBytes( path ) ) );

			Assert.IsTrue( texture.IsValid, $"{path} did not decode" );

			Assert.IsTrue( texture.Data.Width > 0 && texture.Data.Height > 0,
				$"{path} decoded to {texture.Data.Width}x{texture.Data.Height}" );

			// Four bytes a texel, every one of them written. A buffer sized short of what the decode
			// indexes throws long before it reaches this line.
			Assert.AreEqual( texture.Data.Width * texture.Data.Height * 4, texture.Data.Data.Length,
				$"{path} decoded to the wrong number of bytes" );
		}
	}
}
