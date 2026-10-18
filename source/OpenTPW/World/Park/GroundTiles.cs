namespace OpenTPW;

/// <summary>
/// The procedural ground's tiles of every cell inside its box, laid side by side in one texture in the cells' own
/// order, and where a cell finds its tile there. docs/exe/park-engine.md, "The procedural ground".
/// </summary>
/// <remarks>
/// The original keeps 1,500 to 2,500 tiles (<c>PROCEDURALTEXTURECACHE</c>), each its own texture, and composites a
/// visible cell's again when the cache has let it go. A tile is a function of its cell and the file alone, so making
/// every one once as the park loads draws the same pixels. Laid side by side, a tile's smaller mip levels take in its
/// neighbours, which are the cells beside it on the ground. The original's tiles are 16-bit (<c>0x0057257a</c>);
/// these keep the file's 8 bits a channel, as every texture of ours does.
/// </remarks>
public sealed class GroundTiles
{
	/// <summary>
	/// The tiles, laid bottom up: <c>content/shaders/test.shader</c> turns every texture's v over (<c>1 - v</c>), so the
	/// texture's last row is v nought, where cell (x0, y0)'s tile begins with its row 0.
	/// </summary>
	public Texture Texture { get; }

	/// <summary>The box, inclusive: cells (X0, Y0) to (X1, Y1).</summary>
	public int X0 { get; }

	public int Y0 { get; }

	public int X1 { get; }

	public int Y1 { get; }

	public int TileSize { get; }

	private readonly int _width;
	private readonly int _height;

	private GroundTiles( Texture texture, (int X0, int Y0, int X1, int Y1) box, int tileSize )
	{
		Texture = texture;
		(X0, Y0, X1, Y1) = box;
		TileSize = tileSize;
		_width = (X1 - X0 + 1) * tileSize;
		_height = (Y1 - Y0 + 1) * tileSize;
	}

	/// <summary>Whether cell (x, y) is inside the box, and so draws its tile.</summary>
	public bool Covers( int x, int y ) => x >= X0 && x <= X1 && y >= Y0 && y <= Y1;

	/// <summary>
	/// Where a cell corner's coordinate (0 or 1 each way) falls on its tile, v as before the shader turns it over: from the
	/// first texel's centre to the last's,
	/// as <c>FUN_00590350</c> places a texture (offset half a texel in, the size one texel short; <c>[0x008c1628]</c> 0.5).
	/// </summary>
	public Vector2 Uv( int x, int y, Vector2 corner ) => TileUv( x - X0, y - Y0, corner, TileSize, _width, _height );

	internal static Vector2 TileUv( int column, int row, Vector2 corner, int tileSize, int width, int height )
		=> new( ((column * tileSize) + 0.5f + (corner.X * (tileSize - 1))) / width,
			((row * tileSize) + 0.5f + (corner.Y * (tileSize - 1))) / height );

	/// <summary>
	/// The box <c>FUN_00457c30</c> sets as a park loads (<c>+0x234</c> to <c>+0x240</c>): from <c>MapInfo.XStart</c>,
	/// <c>YStart</c> to that plus <c>HeightfieldWidth</c>, <c>HeightfieldHeight</c> and
	/// <c>ThemeEngine.ProceduralTexturingX/YOffset</c>, an offset below 1 held to nought (<c>FUN_004080e0</c>).
	/// </summary>
	internal static (int X0, int Y0, int X1, int Y1) Box( int xStart, int yStart, int width, int height, int xOffset, int yOffset )
		=> (xStart, yStart, xStart + width + (xOffset < 1 ? 0 : xOffset), yStart + height + (yOffset < 1 ? 0 : yOffset));

	/// <summary>
	/// The tiles of a park, or null where it has no <c>base.lnd</c> that reads: its cells then draw their own textures.
	/// Without a balance the box is the field's own.
	/// </summary>
	public static GroundTiles? Make( string themeName, ParkBalance? balance, HeightfieldFile field, int tileSize )
	{
		ProceduralGround ground;

		try
		{
			using var stream = FileSystem.OpenRead( $"levels/{themeName.ToLowerInvariant()}/terrain/base.lnd" );
			ground = new ProceduralGround( new ProceduralGroundFile( stream ), tileSize );
		}
		catch ( Exception e )
		{
			Log.Warning( $"{themeName}: base.lnd would not read, so every ground cell draws its own texture - {e.Message}" );
			return null;
		}

		var box = balance == null
			? (0, 0, field.CellsX - 1, field.CellsY - 1)
			: Box( balance.Int( "MapInfo.XStart" ), balance.Int( "MapInfo.YStart" ),
				balance.Int( "MapInfo.HeightfieldWidth", field.CellsX - 1 ), balance.Int( "MapInfo.HeightfieldHeight", field.CellsY - 1 ),
				balance.Int( "ThemeEngine.ProceduralTexturingXOffset" ), balance.Int( "ThemeEngine.ProceduralTexturingYOffset" ) );

		var across = box.Item3 - box.Item1 + 1;
		var down = box.Item4 - box.Item2 + 1;
		var pixels = Lay( ground, across, down, out var grey );

		Log.Info( $"{themeName}: procedural ground - box {box.Item1},{box.Item2} to {box.Item3},{box.Item4}, " +
			$"{across * down} tiles of {tileSize} pixels, {grey} grey where the map's byte 0 refuses" );

		return new GroundTiles( new Texture( pixels, across * tileSize, down * tileSize ), box, tileSize );
	}

	/// <summary>
	/// The box's tiles side by side as RGBA, its rows bottom up: tile (column, row)'s row k on the texture's row
	/// <c>height - 1 - (row x size + k)</c>. <paramref name="grey"/> counts the tiles the map's byte 0 refuses.
	/// </summary>
	internal static byte[] Lay( ProceduralGround ground, int across, int down, out int grey )
	{
		var size = ground.TileSize;
		var width = across * size;
		var height = down * size;
		var pixels = new byte[width * height * 4];
		var refused = 0;

		// Each tile is its cell's and the file's alone, so the box's rows are made side by side.
		Parallel.For( 0, down, row =>
		{
			for ( var column = 0; column < across; ++column )
			{
				var tile = ground.Tile( column, row );

				if ( tile.All( b => b == ProceduralGround.Grey ) )
					Interlocked.Increment( ref refused );

				for ( var k = 0; k < size; ++k )
				{
					for ( var j = 0; j < size; ++j )
					{
						var from = ((k * size) + j) * 3;
						var to = (((height - 1 - ((row * size) + k)) * width) + (column * size) + j) * 4;
						pixels[to] = tile[from];
						pixels[to + 1] = tile[from + 1];
						pixels[to + 2] = tile[from + 2];
						pixels[to + 3] = 255;
					}
				}
			}
		} );

		grey = refused;
		return pixels;
	}
}
