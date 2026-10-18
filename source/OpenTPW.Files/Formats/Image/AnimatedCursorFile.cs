namespace OpenTPW;

/// <summary>
/// A Windows animated cursor (.ani), read for its hotspot alone: the pixel of the picture that stands on the pointer.
///
/// <para>
/// The file is RIFF <c>ACON</c>; each frame is an <c>icon</c> chunk holding a whole .cur, whose directory entry keeps
/// the hotspot at bytes 4 and 6 where an .ico keeps its planes and bit count. The game draws its cursors from the
/// <c>.tga</c> strips beside these files, so only the hotspot is taken from here. The FileFormats docs, "Cursors",
/// give the layout and the shipped values: one 32x32 hotspot per file, the same in every frame.
/// </para>
/// </summary>
public sealed class AnimatedCursorFile : BaseFormat
{
	/// <summary>The cursor directory's six bytes, then the entry's width, height, colours and reserved byte.</summary>
	private const int HotspotOffset = 10;

	/// <summary>The first frame's hotspot, or (0, 0) when the file holds no frame that can be read.</summary>
	public (int X, int Y) Hotspot { get; private set; }

	public AnimatedCursorFile( Stream stream )
	{
		ReadFromStream( stream );
	}

	protected override void ReadFromStream( Stream stream )
	{
		using var memory = new MemoryStream();

		stream.CopyTo( memory );

		var bytes = memory.ToArray();

		if ( bytes.Length < 12 || bytes[0..4] is not [(byte)'R', (byte)'I', (byte)'F', (byte)'F']
			|| bytes[8..12] is not [(byte)'A', (byte)'C', (byte)'O', (byte)'N'] )
		{
			Log.Warning( $"Animated cursor: {bytes.Length} bytes is not a RIFF ACON" );
			return;
		}

		// Chunks follow the twelve-byte header; LIST chunks are entered, every other chunk is stepped over by its size.
		var at = 12;

		while ( at + 8 <= bytes.Length )
		{
			var id = System.Text.Encoding.ASCII.GetString( bytes, at, 4 );
			var size = BitConverter.ToInt32( bytes, at + 4 );

			if ( id == "LIST" )
			{
				at += 12;
				continue;
			}

			if ( id == "icon" && size >= HotspotOffset + 4 && at + 8 + size <= bytes.Length )
			{
				var cursor = at + 8;
				Hotspot = (BitConverter.ToUInt16( bytes, cursor + HotspotOffset ), BitConverter.ToUInt16( bytes, cursor + HotspotOffset + 2 ));
				return;
			}

			at += 8 + size + (size & 1);
		}

		Log.Warning( "Animated cursor: no frame found" );
	}
}
