namespace OpenTPW;

public partial class TextureFile
{
	/// <summary>
	/// The plain D4 synthesis pair, built as the original builds them into <c>0x00c27208</c>..<c>0x00c27214</c>.
	/// </summary>
	static readonly float H0 = (float)((1 + Math.Sqrt( 3 )) / (4 * Math.Sqrt( 2 )));
	static readonly float H1 = (float)((3 + Math.Sqrt( 3 )) / (4 * Math.Sqrt( 2 )));
	static readonly float H2 = (float)((3 - Math.Sqrt( 3 )) / (4 * Math.Sqrt( 2 )));
	static readonly float H3 = (float)((1 - Math.Sqrt( 3 )) / (4 * Math.Sqrt( 2 )));

	/// <summary>
	/// A type <c>0x12</c> image decoded as the original's zlib codec decodes it (<c>FUN_00593ab0</c>;
	/// <c>docs/exe/textures.md</c>, "The zlib codec"): the stream's Y plane (<paramref name="n"/> square), then Cb's and
	/// Cr's (half that), each value times its channel's multiplier, every row and then every column inverted with plain
	/// D4; each chroma value taken for a 2 x 2 block, BT.601, rounded to nearest and held to 0..255. An image with no
	/// alpha chunk is opaque.
	/// </summary>
	/// <remarks>
	/// The decompressor is chosen by the chunk's magic, so the three type <c>0x12</c> files whose chunks are LZSS (the
	/// 8 x 8 <c>stexture</c> copies of <c>Bowler_Btie.wct</c>, <c>Fan_stalk.wct</c> and <c>s_disc.wct</c>) go to LZSS,
	/// which throws on all three. The original's zlib decompressor refuses a chunk that does not start "BILZ"
	/// (<c>0x006193d0</c>) and decodes whatever its buffer held.
	/// </remarks>
	static byte[] DecodeZlibCodec( TextureFileData fileData, byte[] color, byte[] alpha, int n )
	{
		var position = 0;
		var luma = Plane( color, ref position, n, ComputeDequantizationScaleY( fileData.YChannelQuantizationScale ) );
		var cb = Plane( color, ref position, n / 2, ComputeDequantizationScaleCbCr( fileData.CbChannelQuantizationScale ) );
		var cr = Plane( color, ref position, n / 2, ComputeDequantizationScaleCbCr( fileData.CrChannelQuantizationScale ) );

		float[]? opacity = null;
		if ( fileData.HasAlphaChannel )
		{
			var alphaPosition = 0;
			opacity = Plane( alpha, ref alphaPosition, n, ComputeDequantizationScaleA( fileData.AChannelQuantizationScale ) );
		}

		var pixels = new byte[fileData.Width * fileData.Height * 4];

		for ( int row = 0; row < fileData.Height; row++ )
		{
			for ( int column = 0; column < fileData.Width; column++ )
			{
				double y = luma[row * n + column];
				var block = (row / 2) * (n / 2) + column / 2;
				var i = (row * fileData.Width + column) * 4;

				pixels[i] = Held( y + 1.402f * cr[block] );
				pixels[i + 1] = Held( y - 0.344136f * cb[block] - 0.714136f * cr[block] );
				pixels[i + 2] = Held( y + 1.772f * cb[block] );
				pixels[i + 3] = opacity is null ? (byte)255 : Held( opacity[row * n + column] );
			}
		}

		return pixels;
	}

	/// <summary>Rounded to nearest, as the original's <c>+ 12582912</c> rounds it, then held to a byte.</summary>
	static byte Held( double value ) => (byte)Math.Clamp( Math.Round( value ), 0, 255 );

	/// <summary>
	/// One channel's <paramref name="n"/> x <paramref name="n"/> values read from <paramref name="position"/> on (a signed
	/// byte, or <c>0x80</c> and a little-endian int16), each times <paramref name="multiplier"/>, its rows and then its
	/// columns inverted.
	/// </summary>
	static float[] Plane( byte[] stream, ref int position, int n, float multiplier )
	{
		var values = new float[n * n];

		for ( int i = 0; i < values.Length; i++ )
		{
			int value = (sbyte)stream[position++];

			if ( value == sbyte.MinValue )
			{
				value = BitConverter.ToInt16( stream, position );
				position += sizeof( short );
			}

			values[i] = value * multiplier;
		}

		return Inverted( Inverted( values, n, 1, n ), n, n, 1 );
	}

	/// <summary>
	/// Every line of a plane inverted with plain D4: a line's first half is its smooth values, its second its detail,
	/// and the value before the first is the last (<c>FUN_00593ab0</c>). <paramref name="along"/> steps between a line's
	/// values, <paramref name="across"/> between lines: (1, n) is the rows, (n, 1) the columns.
	/// </summary>
	static float[] Inverted( float[] plane, int n, int along, int across )
	{
		var result = new float[plane.Length];
		var half = n / 2;

		for ( int line = 0; line < n; line++ )
		{
			var start = line * across;

			for ( int k = 0; k < half; k++ )
			{
				var p = k == 0 ? half - 1 : k - 1;
				double smoothBefore = plane[start + p * along];
				double smooth = plane[start + k * along];
				double detailBefore = plane[start + (half + p) * along];
				double detail = plane[start + (half + k) * along];

				result[start + 2 * k * along] = (float)(smoothBefore * H2 + smooth * H0 + detailBefore * H1 + detail * H3);
				result[start + (2 * k + 1) * along] = (float)(smoothBefore * H3 + smooth * H1 - detailBefore * H0 - detail * H2);
			}
		}

		return result;
	}
}
