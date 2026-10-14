using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// A model material's alpha: its reference is its texture's (<c>docs/exe/textures.md</c>, "The loader"), and whether
/// it is drawn at all is still the material's bit <c>0x2</c>, a deviation (<c>LobbyModel.SeeThrough</c>).
/// </summary>
[TestClass]
public class LobbyModelAlphaTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame() => data = GameData.Required();

	private const uint Translucent = 0x2;
	private const uint CutOut = 0x10000;

	/// <summary>
	/// <b>The reference is the header's byte 1, not a count of the pixels.</b> <c>gk_sign1</c>, the Belly Bounce's sign,
	/// has byte 1 set and 753 partly-clear texels of 16,384, under the twentieth a count calls graded: it is drawn graded
	/// (no cut-out mark). <c>jri_bnk4</c>, a river bank, has byte 1 nought: cut out. <c>tp_top</c> has no alpha chunk:
	/// solid under its <c>0x2</c>. <c>m_grass1</c>, a floor's border, has an alpha under a material without <c>0x2</c>:
	/// solid here, the counted deviation. A sign's texture is see-through whatever the material says.
	/// </summary>
	/// <remarks><b>Mutations:</b> the reference by the pixel count; the 0x2 gate dropped; a 0x2 over an opaque texture
	/// left see-through; the file's own 0x2 kept on a solid material; the sign's exception dropped.</remarks>
	[TestMethod]
	public void AMaterialsAlphaReferenceIsItsTexturesHeaderAndItsBitStillGatesIt()
	{
		var sign = Read( "levels/jungle/sharetex/gk_sign1.wct" );
		var bank = Read( "levels/jungle/terrain/textures/jri_bnk4.wct" );
		var top = Read( "levels/jungle/rides/totem/textures/tp_top.wct" );
		var floor = Read( "levels/jungle/sharetex/m_grass1.wct" );

		Assert.AreEqual( TextureAlpha.Graded, sign.Alpha, "gk_sign1: byte 1 set" );
		Assert.IsTrue( Partial( sign ) * 20 < sign.Data.Width * sign.Data.Height, "and too few partly-clear texels for a count to call it graded" );
		Assert.AreEqual( TextureAlpha.CutOut, bank.Alpha, "jri_bnk4: byte 1 nought" );
		Assert.AreEqual( TextureAlpha.Opaque, top.Alpha, "tp_top: no alpha chunk" );
		Assert.AreEqual( TextureAlpha.Graded, floor.Alpha, "m_grass1: byte 1 set" );

		var mesh = new ModelFile.Mesh
		{
			Materials = [Mat( 0x33 ), Mat( 0x33 ), Mat( 0x33 ), Mat( 0x31 ), Mat( 0x31 )]
		};
		var textures = new List<Texture> { Of( sign.Alpha ), Of( bank.Alpha ), Of( top.Alpha ), Of( floor.Alpha ), Of( TextureAlpha.Opaque ) };
		var substituted = new bool[OpenTPW.Material.TextureSlots];
		substituted[4] = true;

		uint Flags( int material ) => LobbyModel.MaterialFlagsFor( mesh, textures, substituted, material );

		Assert.AreEqual( 0x31u | Translucent, Flags( 0 ), "the sign: see-through, graded" );
		Assert.AreEqual( 0x31u | Translucent | CutOut, Flags( 1 ), "the bank: see-through, cut out" );
		Assert.AreEqual( 0x31u, Flags( 2 ), "tp_top: solid, its 0x2 taken out" );
		Assert.AreEqual( 0x31u, Flags( 3 ), "the floor's border: solid, the deviation" );
		Assert.AreEqual( 0x31u | Translucent | CutOut, Flags( 4 ), "a substituted sign: see-through, cut out" );
	}

	/// <summary>
	/// <b>A .wct texture takes its file's alpha</b>, read as a texture is (<c>Texture.ReadWct</c>, the whole of its load
	/// but the upload): <c>gk_sign1</c> graded where its pixels alone would be cut out, <c>jri_bnk4</c> cut out,
	/// <c>tp_top</c> opaque; a texture the game does not have is the original's stand-in, opaque as its pixels are.
	/// </summary>
	/// <remarks><b>Mutations:</b> the load by the pixel count; a missing file taken as cut out.</remarks>
	[TestMethod]
	public void AWctTextureTakesItsFilesAlpha()
	{
		var was = FileSystem;
		FileSystem = data;

		try
		{
			Assert.AreEqual( TextureAlpha.Graded, Texture.ReadWct( "levels/jungle/sharetex/gk_sign1.wct" ).Alpha, "gk_sign1: the file's, not the count's" );
			Assert.AreEqual( TextureAlpha.CutOut, Texture.ReadWct( "levels/jungle/terrain/textures/jri_bnk4.wct" ).Alpha, "jri_bnk4" );
			Assert.AreEqual( TextureAlpha.Opaque, Texture.ReadWct( "levels/jungle/rides/totem/textures/tp_top.wct" ).Alpha, "tp_top" );

			var missing = Texture.ReadWct( "levels/jungle/sharetex/opentpw-no-such.wct" );
			Assert.AreEqual( TextureAlpha.Opaque, missing.Alpha, "a missing file: opaque" );
			Assert.AreEqual( TextureAlpha.Opaque, Texture.Classify( missing.Image.Data ), "as its stand-in's pixels are" );
		}
		finally
		{
			FileSystem = was;
		}
	}

	/// <summary>
	/// A texture that is not a .wct is classified by its pixels, as <c>FUN_00575160</c> does: graded at a twentieth of
	/// the area partly clear, cut out under it, opaque when every texel is.
	/// </summary>
	/// <remarks><b>Mutations:</b> the twentieth moved either way; an opaque texture called cut out.</remarks>
	[TestMethod]
	public void ATextureThatIsNotAWctIsClassifiedByItsPixels()
	{
		static byte[] Texels( int count, int partial, byte clear = 255 )
			=> Enumerable.Range( 0, count ).SelectMany( i => new byte[] { 1, 2, 3, i < partial ? (byte)100 : clear } ).ToArray();

		Assert.AreEqual( TextureAlpha.Opaque, Texture.Classify( Texels( 40, 0 ) ), "every texel opaque" );
		Assert.AreEqual( TextureAlpha.Graded, Texture.Classify( Texels( 40, 2 ) ), "a twentieth partly clear" );
		Assert.AreEqual( TextureAlpha.CutOut, Texture.Classify( Texels( 40, 1 ) ), "under a twentieth" );
		Assert.AreEqual( TextureAlpha.CutOut, Texture.Classify( Texels( 40, 0, 0 ) ), "only clear and opaque texels" );
	}

	private TextureFile Read( string path ) => new( new MemoryStream( data.ReadAllBytes( path ) ) );

	private static int Partial( TextureFile file ) => file.Data.Data.Where( ( b, i ) => i % 4 == 3 && b > 0 && b < 240 ).Count();

	private static ModelFile.MaterialData Mat( uint flags ) => new() { Flags = flags, Name = "m" };

	/// <summary>A texture as a load would leave it, made without a device.</summary>
	private static Texture Of( TextureAlpha alpha )
	{
		var texture = (Texture)RuntimeHelpers.GetUninitializedObject( typeof( Texture ) );
		typeof( Texture ).GetProperty( nameof( Texture.Alpha ) )!.SetValue( texture, alpha );
		return texture;
	}
}
