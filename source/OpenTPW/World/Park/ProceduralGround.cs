namespace OpenTPW;

/// <summary>
/// The procedural ground's tile maker: <c>FUN_0055f780</c>, which cuts one ground cell's tile out of a
/// <see cref="ProceduralGroundFile"/>. Every step and quirk below is docs/exe/park-engine.md, "The procedural ground";
/// the tests hold it to the original's own code run under an emulator (<c>lndtile.py</c>).
/// </summary>
public sealed class ProceduralGround
{
	/// <summary>The colour of a cell the map's byte 0 refuses: <c>0x7f7f7f00</c>.</summary>
	public const byte Grey = 0x7f;

	/// <summary>The pixels of a row the masks are classed over together (the original's MMX spans).</summary>
	private const int Span = 16;

	/// <summary>Pixels along a tile's side.</summary>
	public int TileSize { get; }

	/// <summary>Box-relative cells with a full 5 x 5 of samples: 95 x 85 in every park, <c>(w - 1) &gt;&gt; 2</c> and <c>(h - 1) &gt;&gt; 2</c>.</summary>
	public int Columns { get; }

	public int Rows { get; }

	private readonly ProceduralGroundFile.Image _map;
	private readonly ProceduralGroundFile.Image[] _textures;

	/// <summary>The five ramps, from the file's pairs 1 to 5 (<c>0x008795a8</c>, <c>0x008796a8</c>, <c>0x008793a8</c>, <c>0x008797a8</c>, <c>0x008794a8</c>).</summary>
	private readonly byte[][] _ramps;

	public ProceduralGround( ProceduralGroundFile file, int tileSize )
	{
		var set = file.SetFor( tileSize ) ?? throw new ArgumentException( $"base.lnd has no set for {tileSize}-pixel tiles." );

		TileSize = tileSize;
		_map = file.Map;
		_textures = set.Slots;
		Columns = (_map.Width - 1) >> 2;
		Rows = (_map.Height - 1) >> 2;

		_ramps = new byte[5][];

		for ( var i = 0; i < 5; ++i )
			_ramps[i] = Ramp( file.Ramps[i * 2], file.Ramps[(i * 2) + 1] );
	}

	/// <summary>
	/// <c>FUN_0055e710</c>: nought below <c>c - (w + 1) / 2</c>, 255 above <c>c + (w + 1) / 2</c>, and
	/// <c>(i x 255 - lo x 255) / (w + 1)</c> between, as a byte.
	/// </summary>
	internal static byte[] Ramp( int centre, int width )
	{
		var lo = centre - ((width + 1) >> 1);
		var hi = centre + ((width + 1) >> 1);
		var ramp = new byte[256];

		for ( var i = 0; i < 256; ++i )
			ramp[i] = i < lo ? (byte)0 : i > hi ? (byte)255 : (byte)(((i * 255) - (lo * 255)) / (width + 1));

		return ramp;
	}

	/// <summary><c>q + ((p - q) x w &gt;&gt; 8)</c> on one byte, the shift arithmetic.</summary>
	private static int Lerp( int q, int p, int w ) => (q + (((p - q) * w) >> 8)) & 0xff;

	/// <summary>
	/// The tile of box-relative cell (<paramref name="x"/>, <paramref name="y"/>): <see cref="TileSize"/> squared
	/// pixels of red, green and blue, row 0 first, as <c>FUN_00572440</c> leaves it in the surface. Row k lies
	/// toward the cell's y + 1 edge as k grows, and column j toward x + 1.
	/// </summary>
	public byte[] Tile( int x, int y )
	{
		var n = TileSize;
		var tile = new byte[n * n * 3];
		var r = Rows - y - 1;

		// The map is read flat, as the original reads it, so the box's last column (95) takes the next row's first.
		if ( (Sample( (4 * r) + 1, (4 * x) + 1, 0 ) & Sample( (4 * r) + 1, (4 * x) + 2, 0 )
			& Sample( (4 * r) + 2, (4 * x) + 1, 0 ) & Sample( (4 * r) + 2, (4 * x) + 2, 0 )) != 0xff )
		{
			Array.Fill( tile, Grey );
			return tile;
		}

		var u1 = Upsample( x, r, 1 );
		var u2 = Upsample( x, r, 2 );
		var u3 = Upsample( x, r, 3 );

		var mA = new byte[n * n];
		var mB = new byte[n * n];

		for ( var i = 0; i < n; ++i )
		{
			for ( var j = 0; j < n; ++j )
			{
				var k = (i * n) + j;
				mA[k] = Math.Max( _ramps[4][Mix( 12, x, r, i, j ) * u3[k] >> 8], _ramps[4][Mix( 11, x, r, i, j ) * u3[k] >> 8] );
				mB[k] = Math.Max( _ramps[2][Mix( 6, x, r, i, j ) * u2[k] >> 8], _ramps[2][Mix( 7, x, r, i, j ) * u2[k] >> 8] );
			}
		}

		for ( var i = 0; i < n; ++i )
		{
			for ( var s = 0; s < n; s += Span )
			{
				var a = Class( mA, (i * n) + s );
				var b = Class( mB, (i * n) + s );

				for ( var j = s; j < s + Span; ++j )
				{
					var k = (i * n) + j;

					// FUN_0055eaa0, both masks mixed, never steps its u1 pointer: the span's first serves all sixteen.
					var w1 = a == 1 && b == 1 ? u1[(i * n) + s] : u1[k];

					// The scratch is written from its last row up: weight row i is the tile's row n - 1 - i.
					var o = ((((n - 1 - i) * n) + j) * 3);

					var wA = Weight( 10, _ramps[3], x, r, i, j, w1 );
					var wB = Weight( 5, _ramps[1], x, r, i, j, w1 );
					var wC = Weight( 2, _ramps[0], x, r, i, j, w1 );

					for ( var c = 1; c <= 3; ++c )
					{
						var layerA = Layer( 9, 8, wA, x, r, i, j, c );
						var layerB = Layer( 4, 3, wB, x, r, i, j, c );
						var layerC = Layer( 1, 0, wC, x, r, i, j, c );

						// A span of one class takes its layer whole, never lerp( x, y, 255 ), which is one short.
						var under = b == 2 ? layerB : b == 0 ? layerC : Lerp( layerC, layerB, mB[k] );
						tile[o + c - 1] = (byte)(a == 2 ? layerA : a == 0 ? under : Lerp( under, layerA, mA[k] ));
					}
				}
			}
		}

		return tile;
	}

	/// <summary>2 where a span's sixteen are all 255, 0 where all nought, 1 where mixed.</summary>
	private static int Class( byte[] mask, int start )
	{
		bool full = true, empty = true;

		for ( var j = start; j < start + Span; ++j )
		{
			full &= mask[j] == 255;
			empty &= mask[j] == 0;
		}

		return full ? 2 : empty ? 0 : 1;
	}

	/// <summary>A layer's weight at a pixel: <c>R[ Tm x u1 &gt;&gt; 8 ]</c>.</summary>
	private int Weight( int mix, byte[] ramp, int x, int r, int i, int j, int u1 ) => ramp[Mix( mix, x, r, i, j ) * u1 >> 8];

	/// <summary>One layer's byte <paramref name="channel"/>: <c>lerp( Tq, Tp, w )</c>.</summary>
	private int Layer( int q, int p, int w, int x, int r, int i, int j, int channel )
		=> Lerp( Texel( q, x, r, i, j, channel ), Texel( p, x, r, i, j, channel ), w );

	/// <summary>One map byte, the map read as one run of rows.</summary>
	private int Sample( int row, int column, int channel ) => _map.Data[(((row * _map.Width) + column) * 4) + channel];

	/// <summary>
	/// The texel at (i, j) of a texture's window for the cell: N x N from column <c>(N x x) mod (w - N)</c> and row
	/// <c>(N x r) mod (h - N)</c>.
	/// </summary>
	private int Texel( int slot, int x, int r, int i, int j, int channel )
	{
		var t = _textures[slot];
		var n = TileSize;
		var row = ((n * r) % (t.Height - n)) + i;
		var column = ((n * x) % (t.Width - n)) + j;
		return t.Data[(((row * t.Width) + column) * t.Depth) + channel];
	}

	private int Mix( int slot, int x, int r, int i, int j ) => Texel( slot, x, r, i, j, 0 );

	/// <summary>
	/// <c>FUN_0055e780</c>: one of the map's bytes over the cell's 5 x 5 samples to N x N, each 4 x 4 square bilinear in
	/// 16.16 fixed point, its value starting at <c>a &lt;&lt; 16</c> and stepping <c>(c - a) &lt;&lt; s</c> a row and
	/// <c>dx</c> along it, <c>dx</c> starting at <c>(b - a) &lt;&lt; s</c> and growing by
	/// <c>(((d - c) &lt;&lt; s) - ((b - a) &lt;&lt; s)) &gt;&gt; log2(N / 4)</c> a row.
	/// </summary>
	private int[] Upsample( int x, int r, int channel )
	{
		var n = TileSize;
		var q = n / 4;
		var shift = System.Numerics.BitOperations.Log2( (uint)q );
		var s = 16 - shift;
		var weights = new int[n * n];

		for ( var i = 0; i < 4; ++i )
		{
			for ( var j = 0; j < 4; ++j )
			{
				var a = Sample( (4 * r) + i, (4 * x) + j, channel );
				var b = Sample( (4 * r) + i, (4 * x) + j + 1, channel );
				var c = Sample( (4 * r) + i + 1, (4 * x) + j, channel );
				var d = Sample( (4 * r) + i + 1, (4 * x) + j + 1, channel );
				var dx = (b - a) << s;
				var ddx = (((d - c) << s) - dx) >> shift;

				for ( var yy = 0; yy < q; ++yy )
					for ( var xx = 0; xx < q; ++xx )
						weights[(((i * q) + yy) * n) + (j * q) + xx] = (((a << 16) + (yy * ((c - a) << s)) + (xx * (dx + (yy * ddx)))) >> 16) & 0xff;
			}
		}

		return weights;
	}
}
