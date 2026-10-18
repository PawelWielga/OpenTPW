using System;
using System.IO;

namespace OpenTPW;

/// <summary>
/// A park's <c>base.lnd</c>, the source of its composited ground: a map of where the ground is composited and how much
/// of each layer shows there, five ramp pairs, and three sets of thirteen textures (for 64-, 32- and 16-pixel tiles).
/// Read as <c>FUN_00560ff0</c> and <c>FUN_0055f470</c> read it; the bytes are the FileFormats docs' <c>lnd.md</c>, and
/// what the game makes of them is docs/exe/park-engine.md, "The procedural ground".
/// </summary>
public sealed class ProceduralGroundFile
{
	/// <summary>The one version the game takes.</summary>
	public const byte Version = 3;

	/// <summary>The textures in a set.</summary>
	public const int TexturesPerSet = 13;

	/// <summary>
	/// The game's slot for each texture in the order the file stores them (<c>FUN_0055f470</c>): colour textures 0, 1,
	/// 3, 4, 8 and 9 (four bytes a texel), greys 2, 5, 10, 6, 7, 11 and 12 (one).
	/// </summary>
	public static ReadOnlySpan<int> SlotInFileOrder => [0, 1, 3, 4, 8, 9, 2, 5, 10, 6, 7, 11, 12];

	/// <summary>One texture: width, height, bytes a texel and the texels row by row.</summary>
	public sealed record Image( int Width, int Height, int Depth, byte[] Data );

	/// <summary>One set: the tile size it serves, its index byte, and its thirteen textures by the game's slot.</summary>
	public sealed record TextureSet( int TileSize, byte Index, Image[] Slots );

	/// <summary>The file's ten bytes after the version: five pairs of a ramp's centre and width.</summary>
	public byte[] Ramps { get; }

	/// <summary>The map: <see cref="Image.Width"/> x <see cref="Image.Height"/> samples of four bytes (384 x 344 in every park).</summary>
	public Image Map { get; }

	/// <summary>The three sets, for 64-, 32- and 16-pixel tiles in that order.</summary>
	public TextureSet[] Sets { get; }

	public ProceduralGroundFile( Stream stream )
	{
		using var reader = new BinaryReader( stream, System.Text.Encoding.ASCII, leaveOpen: true );

		var version = reader.ReadByte();

		if ( version != Version )
			throw new InvalidDataException( $"base.lnd version {version}, where the game takes only {Version}." );

		Ramps = reader.ReadBytes( 10 );
		Map = ReadImage( reader );

		Sets = new TextureSet[3];

		for ( var set = 0; set < Sets.Length; ++set )
		{
			var size = reader.ReadInt32();
			var index = reader.ReadByte();
			var slots = new Image[TexturesPerSet];

			for ( var i = 0; i < TexturesPerSet; ++i )
				slots[SlotInFileOrder[i]] = ReadImage( reader );

			Sets[set] = new TextureSet( size, index, slots );
		}
	}

	/// <summary>The set made for <paramref name="tileSize"/> pixels, or null where the file has none.</summary>
	public TextureSet? SetFor( int tileSize ) => Array.Find( Sets, set => set.TileSize == tileSize );

	private static Image ReadImage( BinaryReader reader )
	{
		var width = reader.ReadInt32();
		var height = reader.ReadInt32();
		var depth = reader.ReadInt32();

		if ( width <= 0 || height <= 0 || depth is not (1 or 4) || (long)width * height * depth > 1 << 24 )
			throw new InvalidDataException( $"base.lnd image {width} x {height} x {depth}." );

		var data = reader.ReadBytes( width * height * depth );

		if ( data.Length != width * height * depth )
			throw new EndOfStreamException( "base.lnd ends inside an image." );

		return new Image( width, height, depth, data );
	}
}
