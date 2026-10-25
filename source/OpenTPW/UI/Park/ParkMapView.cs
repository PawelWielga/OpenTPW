namespace OpenTPW.UI;

/// <summary>
/// Where the park map looks and how near: the map screen object's centre, <c>+0x54</c> and <c>+0x58</c>, in cells, and its
/// zoom <c>+0x5c</c> (the static at <c>0x00f86db8</c>; <c>docs/exe/hud.md</c>, "The map's view"), and the picture's
/// rectangle they come to.
///
/// <para>
/// <b>Opened on the camera's cell.</b> The opener takes the camera's point a tenth at a time, truncated, holds it below
/// <c>MapInfo</c>'s width and height and then to the heightfield's cells (<c>0x005f0ec2</c>..<c>0x005f0fa2</c>). The
/// zoom is the static's own and outlives the screen: only the constructor sets it, so it is kept from one opening to the
/// next until the game is quit, and here it is a static too.
/// </para>
/// <para>
/// <b>Moved a cell a pass.</b> The arrows, the arrow keys, the screen's edges and the right button's drag all go through
/// <c>0x005f35b0</c> (left takes from the centre across, right adds, up ADDS to the centre down, down takes), which holds
/// the centre to the heightfield's cells less one. A held arrow moves one cell each pass of the interface; here a pass is
/// the original's thirty a second (measured under its cap, <c>q624/orig/a-hold.log</c>) on <see cref="Time.Delta"/>:
/// a deviation in rate only where the original's frame rate was not thirty.
/// </para>
/// <para>
/// <b>Drawn at the original's 800 x 600.</b> <c>0x005f3230</c> sizes the picture by the screen's own pixels: one
/// picture pixel is <see cref="Zoom"/> screen pixels, so the original's map is smaller the finer its resolution. Here
/// the scale is the one it has at 800 x 600, the reference install's (2.56 layout units a picture pixel at zoom 1), at
/// every window size: a deviation, so the map is not a postage stamp on a modern screen.
/// </para>
/// <para>
/// <b>Upside down.</b> The original's 2D blitter puts the picture's first row at the bottom of the screen, so the park
/// gate, at the top of <c>2dmap.tga</c> as a viewer shows it, stands at the bottom of the map, and a larger
/// <see cref="Down"/> is further UP the screen. Every rule here is written in the picture's rows and turned over only
/// where the screen is reached (<see cref="Placed"/>).
/// </para>
/// </summary>
internal sealed class ParkMapView
{
	public const float MinZoom = 1f;
	public const float MaxZoom = 3f;

	/// <summary>What a zoom button or the keypad's + or - adds or takes (<c>0x00703030</c>, <c>0x00703028</c>).</summary>
	public const float ZoomStep = 0.5f;

	/// <summary>The cells a pass of the interface moves the view while an arrow is held or an arrow key goes down.</summary>
	public const float CellsAPass = 1f;

	/// <summary>The original's interface passes a second, by which a per-pass move is taken onto <see cref="Time.Delta"/>.</summary>
	public const float PassesASecond = 30f;

	/// <summary>The picture's field, <c>0x00703044</c> and <c>0x00703048</c>: the heightfield's cells span this many picture pixels.</summary>
	public const float FieldWidth = 480f;

	public const float FieldHeight = 425f;

	/// <summary>Where the field's first cell starts, <c>0x00703054</c> and <c>0x00703058</c> negated.</summary>
	public const float FieldLeft = 15f;

	public const float FieldTop = 42f;

	/// <summary>The screen the scale is taken from - see the class remarks.</summary>
	public const float ReferenceWidth = 800f;

	public const float ReferenceHeight = 600f;

	/// <summary>Layout units a reference screen pixel: the virtual screen is 2048 across.</summary>
	public const float LayoutPerPixel = 2048f / ReferenceWidth;

	/// <summary>Kept from one opening to the next - see the class remarks.</summary>
	internal static float KeptZoom { get; set; } = MinZoom;

	/// <summary>The heightfield's cells across and down (<c>+0x24</c>, <c>+0x28</c>): 96 by 85 in every theme.</summary>
	public int CellsAcross { get; }

	public int CellsDown { get; }

	/// <summary>The picture's own size (<c>+0x48</c>, <c>+0x4c</c>).</summary>
	public int PictureWidth { get; }

	public int PictureHeight { get; }

	/// <summary>The cell the view is centred on, across (<c>+0x54</c>).</summary>
	public float Across { get; private set; }

	/// <summary>The cell the view is centred on, down the park (<c>+0x58</c>); larger is further up the screen.</summary>
	public float Down { get; private set; }

	public float Zoom
	{
		get => KeptZoom;
		private set => KeptZoom = value;
	}

	private ParkMapView( int cellsAcross, int cellsDown, int pictureWidth, int pictureHeight, float across, float down )
	{
		CellsAcross = cellsAcross;
		CellsDown = cellsDown;
		PictureWidth = pictureWidth;
		PictureHeight = pictureHeight;
		Across = across;
		Down = down;
	}

	/// <summary>
	/// The view as the map opens over a camera looking at world point (<paramref name="x"/>, <paramref name="y"/>):
	/// its cell, held below the map's width and height (<c>MapInfo</c>'s 95 and 84) and then to the heightfield's cells.
	/// </summary>
	public static ParkMapView OpenAt( float x, float y, int mapWidth, int mapHeight, int cellsAcross, int cellsDown,
		int pictureWidth = 512, int pictureHeight = 512 )
	{
		var across = Math.Clamp( (int)(x * 0.1f), 0, Math.Max( mapWidth - 1, 0 ) );
		var down = Math.Clamp( (int)(y * 0.1f), 0, Math.Max( mapHeight - 1, 0 ) );

		return new ParkMapView( cellsAcross, cellsDown, pictureWidth, pictureHeight,
			Math.Min( across, cellsAcross ), Math.Min( down, cellsDown ) );
	}

	/// <summary>
	/// <c>0x005f35b0</c>: moves the centre by <paramref name="across"/> and <paramref name="down"/> cells, down being up
	/// the screen, and holds it to the heightfield's cells less one.
	/// </summary>
	public void Scroll( float across, float down )
	{
		Across = Math.Clamp( Across + across, 0f, CellsAcross - 1 );
		Down = Math.Clamp( Down + down, 0f, CellsDown - 1 );
	}

	/// <summary>A zoom button or the keypad's + or -: <see cref="ZoomStep"/> in or out, held to 1 to 3.</summary>
	public void ZoomBy( bool In ) => Zoom = Math.Clamp( Zoom + (In ? ZoomStep : -ZoomStep), MinZoom, MaxZoom );

	/// <summary>A picture pixel's width in layout units at this zoom.</summary>
	public float Scale => Zoom * LayoutPerPixel;

	/// <summary>The picture pixels a cell spans across and down.</summary>
	public float PixelsACellAcross => FieldWidth / CellsAcross;

	public float PixelsACellDown => FieldHeight / CellsDown;

	/// <summary>The picture pixel at the screen's centre: the first cell's corner plus the centre's cells.</summary>
	public (float X, float Y) CentrePixel => (FieldLeft + (PixelsACellAcross * Across), FieldTop + (PixelsACellDown * Down));

	/// <summary>
	/// <c>0x005f3230</c>'s source rectangle: what a reference screen holds at this zoom about the centre pixel, held to
	/// the picture, in its pixels and its rows (row 0 the top of the picture as a viewer shows it).
	/// </summary>
	public (float Left, float Top, float Right, float Bottom) Source
	{
		get
		{
			var (x, y) = CentrePixel;
			var halfWidth = ReferenceWidth / Zoom * 0.5f;
			var halfHeight = ReferenceHeight / Zoom * 0.5f;

			return (MathF.Max( x - halfWidth, 0f ), MathF.Max( y - halfHeight, 0f ),
				MathF.Min( x + halfWidth, PictureWidth - 1 ), MathF.Min( y + halfHeight, PictureHeight - 1 ));
		}
	}

	/// <summary>
	/// Where <see cref="Source"/> lands on the virtual screen, its centre pixel on the screen's centre, in layout units,
	/// and the texture rectangle to draw into it as <see cref="Graphics.Quad(Rectangle, Rectangle, Material)"/> takes it:
	/// that quad's window is y-up and the texture's rows run from the picture's bottom (v 0 is its last row as a viewer
	/// shows it), so the rectangle starts at <see cref="Source"/>'s top row and runs DOWN the texture (its height is
	/// negative), which puts the picture's first row at the bottom of the screen.
	/// </summary>
	public (UiRect Layout, Rectangle Texture) Placed
	{
		get
		{
			var (x, y) = CentrePixel;
			var (left, top, right, bottom) = Source;
			var scale = Scale;

			var layout = new UiRect(
				(int)MathF.Round( VirtualScreen.Width * 0.5f + ((left - x) * scale) ),
				(int)MathF.Round( VirtualScreen.Height * 0.5f + ((y - bottom) * scale) ),
				(int)MathF.Round( VirtualScreen.Width * 0.5f + ((right - x) * scale) ),
				(int)MathF.Round( VirtualScreen.Height * 0.5f + ((y - top) * scale) ) );

			var texture = new Rectangle( left / PictureWidth, 1f - (top / PictureHeight),
				(right - left) / PictureWidth, (top - bottom) / PictureHeight );

			return (layout, texture);
		}
	}

	/// <summary>
	/// A left click on the viewport at layout point (<paramref name="x"/>, <paramref name="y"/>) (<c>0x005f1d19</c>):
	/// the centre moves to the cell under it, measured from the SCREEN's centre, not the viewport's; not held here, as
	/// the close that follows holds it.
	/// </summary>
	public void CentreOn( float x, float y )
	{
		Across += (x - (VirtualScreen.Width * 0.5f)) / (PixelsACellAcross * Scale);
		Down += ((VirtualScreen.Height * 0.5f) - y) / (PixelsACellDown * Scale);
	}

	/// <summary>
	/// <c>0x005f1050</c>, as OK or a click on the map closes it: the centre held to the heightfield's cells less one, and
	/// the world point of its cell's corner, which the camera is put on (<c>FUN_0042aab0</c>).
	/// </summary>
	public (float X, float Y) CameraPoint()
	{
		Across = Math.Clamp( Across, 0f, CellsAcross - 1 );
		Down = Math.Clamp( Down, 0f, CellsDown - 1 );

		return ((int)Across * 10f, (int)Down * 10f);
	}

	/// <summary>
	/// The root's frame message with Pushscroll on (<c>0x005f126b</c>): the pointer within a hundredth of the screen's
	/// width or height of an edge moves the view <c>250</c> cells a pass for each whole of the screen past that line,
	/// <paramref name="across"/> and <paramref name="down"/> being the pointer's place on the screen from 0 to 1.
	/// Answers the cells a pass, across and down.
	/// </summary>
	public static (float Across, float Down) EdgePush( float across, float down )
	{
		across = Math.Clamp( across, 0f, 1f );
		down = Math.Clamp( down, 0f, 1f );

		var x = across < 0.01f ? -(0.01f - across) * 250f : across > 0.99f ? (across - 0.99f) * 250f : 0f;
		var y = down < 0.01f ? (0.01f - down) * 250f : down > 0.99f ? -(down - 0.99f) * 250f : 0f;

		return (x, y);
	}

	/// <summary>
	/// The viewport's frame message with Right button scroll on and the right button held from a press on it
	/// (<c>0x005f192e</c>): the cells a pass, from the pointer's place against the screen's centre in layout units, by
	/// <c>0x00703034</c> across and <c>0x00703038</c> down.
	/// </summary>
	public static (float Across, float Down) RightDrag( float x, float y )
		=> ((x - (VirtualScreen.Width * 0.5f)) * (1f / 409.6f), -((y - (VirtualScreen.Height * 0.5f)) * (1f / 307.2f)));

	public override string ToString()
	{
		var (layout, _) = Placed;
		var (left, top, right, bottom) = Source;
		return $"map view: centre ({Across:0.###},{Down:0.###}) zoom {Zoom:0.0} cells {CellsAcross}x{CellsDown} picture {PictureWidth}x{PictureHeight}"
			+ $" source ({left:0.#},{top:0.#})-({right:0.#},{bottom:0.#}) placed ({layout.Left},{layout.Top})-({layout.Right},{layout.Bottom})";
	}
}
