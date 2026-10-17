namespace OpenTPW.UI;

/// <summary>
/// The park map: the screen behind the gadget's map button.
///
/// <para>
/// <b>Where it comes from.</b> The original's compiled layout stream at 0x00774da0
/// (<c>docs/exe/hud.md</c>, "The park map"). Every rectangle and every id below is that stream's, and
/// every mesh is named by the hash the stream carries.
/// </para>
/// <para>
/// <b>It pauses</b>, as the original's does, and quietens the advisor with the fading stop, not the
/// crying one. <see cref="ParkGadget"/> leaves <see cref="UiWindow.Pauses"/> false because it is up for
/// the whole of a park; this is the opposite case, and <see cref="ParkFrontEnd"/> holds the advisor for
/// any window that pauses.
/// </para>
/// <para>
/// <b>The map is the shipped picture.</b> Every park carries its own 512x512 <c>2dmap.tga</c>, and the
/// original loads the whole of it, as 8x8 tiles of 64x64. It is shown whole here too, the black border
/// the art has included, and is not cropped to its inner field: what the original then shows of it is
/// governed by a zoom and scroll that are not decoded. Cell (x,y) lands at pixel (16 + 5x, 43 + 5y) of
/// that image.
/// </para>
/// <para>
/// <b>The overlay is not built.</b> Over that picture the original paints a live 128x128 grid of
/// classification codes - rides, shops, sideshows, tracks, visitors and staff - each cell coloured by a
/// three-stop ramp over a per-thing metric from 0 to 100, and its six layer switches and two radio
/// groups (staff overlays and the metric picker) choose what it shows. Of its metrics excitement is
/// worked out (<see cref="ParkRideScore.ExcitementOf"/>) and satisfaction's days are kept
/// (<see cref="ParkObjectRings.Satisfaction"/>); reliability and queue times are not, so the switches
/// would sit over an overlay half empty. The two button panels that back the switches, buttpan1 and
/// buttpan2, are kept bare, as the screen's own backing art.
/// </para>
/// <para>
/// <b>No terrain colours are painted over the picture.</b> The colours the original gives its land
/// codes are immediates in its initialiser at <c>0x005f0700</c> (<c>docs/exe/hud.md</c>, "The map's
/// colours"; which record colours which code is not traced), not carried over, and the shipped picture
/// already shows the park's terrain.
/// </para>
/// <para>
/// <b>Engine and content.</b> The window, the controls and the help bar are engine. Which controls the
/// map has, where they sit and what each does is this file - the park's content.
/// </para>
/// </summary>
internal sealed class ParkMapScreen : UiWindow
{
	/// <summary>Slot 4, which is what FUN_005f0b40 gives the lettering panel before colouring it yellow.</summary>
	private const int LabelFont = 4;

	private readonly MapView _view;

	public ParkMapScreen( WindowStack stack ) : base( stack )
	{
		// The original pauses the park here and quietens the advisor - see the class remarks. Modal so
		// that nothing behind it can be clicked, which is what putting the park's interface away
		// (message 6 to DAT_007cb2ac) comes to here. No dimming backdrop, and the root's own mesh,
		// map_win (w_map.MD2), is not drawn.
		Pauses = true;
		Modal = true;

		// FUN_005f0b40 closes the park screen that is open before it hides the layer (0x005f0bd1).
		ClosesParkScreen = true;

		Root = new UiControl { Id = 0x980, Rect = new UiRect( 0, 0, 2047, 1537 ) };

		// The map itself, and the one control with a handler of its own in the original (LAB_005f0b20).
		// It is added first so everything else draws over it: the scroll arrows and the zoom buttons
		// overlap its bottom-left corner, and the panels below start at y 1387 where it stops at 1364.
		_view = Root.Add( new MapView
		{
			Id = 0x992,
			Rect = new UiRect( 49, 33, 1877, 1364 )
		} );

		// The two button panels and the lettering panel between them.
		Root.Add( new UiControl
		{
			Id = 0x990,
			Rect = new UiRect( 35, 1387, 681, 1533 ),
			Mesh = UiMesh.Get( "f_mapbut1" )
		} );

		Root.Add( new UiControl
		{
			Id = 0x995,
			Rect = new UiRect( 1356, 1387, 1909, 1533 ),
			Mesh = UiMesh.Get( "f_mapbut2" )
		} );

		// Yellow, font 4 - FUN_005f0b40 fetches 0x991, hands it a label skin and calls the colour setter
		// with (0xff, 0xff, 0, 0xff). WHAT IT SAYS IS NOT TRACED: the original fills it from a resource
		// this has not read back, so it is drawn empty rather than given words it may not have.
		Root.Add( new UiControl
		{
			Id = 0x991,
			Rect = new UiRect( 688, 1395, 1351, 1525 ),
			Mesh = UiMesh.Get( "f_maptext" ),
			TextRect = new UiRect( 730, 1424, 1295, 1503 ),
			Font = LabelFont,
			TextColour = new UiColour( 255, 255, 0 )
		} );

		// The four scroll arrows. Each carries a seven-point hit region in the stream (op 4 sub-op 4,
		// what UiControl.Outline takes), which is not reproduced: each takes its rectangle, and the
		// rectangles overlap - up and down each share a square with left and right, where the
		// later-added left and right arrows take the press.
		Root.Add( Arrow( 0x98a, new UiRect( 80, 1079, 237, 1236 ), 263, "b_mapup", 0, -1 ) );
		Root.Add( Arrow( 0x981, new UiRect( 80, 1236, 237, 1393 ), 264, "b_mapdn", 0, 1 ) );
		Root.Add( Arrow( 0x984, new UiRect( 1, 1157, 158, 1315 ), 265, "b_mapleft", -1, 0 ) );
		Root.Add( Arrow( 0x988, new UiRect( 158, 1157, 315, 1315 ), 266, "b_mapright", 1, 0 ) );

		// Zoom. The stream gives these two a CIRCULAR hit region - op 4 sub-op 2, three shorts of centre
		// and radius, (246,1355) r28 and (61,1356) r29 - which is likewise left as the rectangle.
		Root.Add( new UiButton
		{
			Id = 0x986,
			Rect = new UiRect( 218, 1327, 275, 1384 ),
			HelpText = 261,
			Mesh = UiMesh.Get( "b_plus" ),
			Clicked = () => _view.Zoom( In: true )
		} );

		Root.Add( new UiButton
		{
			Id = 0x985,
			Rect = new UiRect( 32, 1327, 90, 1385 ),
			HelpText = 262,
			Mesh = UiMesh.Get( "b_minus" ),
			Clicked = () => _view.Zoom( In: false )
		} );

		// The stream gives this one id -1 rather than a number of its own, and UIHELPTEXT row 1 -
		// "Left-click to close this screen".
		Root.Add( new UiButton
		{
			Id = -1,
			Rect = new UiRect( 1941, 1339, 2024, 1422 ),
			HelpText = 1,
			Mesh = UiMesh.Get( "b_okay" ),
			Clicked = Close
		} );

		// The quiet stop, which is what the original takes here: Advisor_StopQuietly(1) fades his voice
		// and throws the line away, where Hush would have him cry out. OptionsScreen is the only other
		// thing in this game that takes it, and for the same reason.
		Advisor.Current?.StopQuietly();
	}

	/// <summary>One of the four scroll arrows, which nudge the view by a cell step - see <see cref="MapView"/>.</summary>
	private UiButton Arrow( int id, UiRect rect, int helpText, string mesh, int across, int down )
		=> new()
		{
			Id = id,
			Rect = rect,
			HelpText = helpText,
			Mesh = UiMesh.Get( mesh ),
			Clicked = () => _view.Scroll( across, down )
		};

	private void Close() => Stack.Close( this );

	// There is no Cancel() here: that is a box's Escape, and the map has no box. A plain Escape let go closes the map
	// as b_okay does, from ParkFrontEnd.MenuKey (the map's own key-up case, 0x005f17ef).

	/// <summary>
	/// The map's own picture goes when the screen does. It is built from a stream rather than a path, so
	/// nothing caches it and nothing else shares it - left alone it would be one more pathless texture a
	/// scene never lets go of.
	/// </summary>
	protected internal override void Closed() => _view.Release();

	/// <summary>
	/// The map picture, drawn into the viewport with the part of it that the scrolling and zooming have
	/// arrived at.
	///
	/// <para>
	/// The image is the park's own <c>levels/&lt;theme&gt;/2dmap.tga</c>, decoded by
	/// <see cref="Texture"/>'s stream constructor - the same StbImageSharp path the sky's art takes. It
	/// is drawn as one quad with a texture-coordinate rectangle, which is all the scrolling and zooming
	/// amount to.
	/// </para>
	/// <para>
	/// <b>The picture keeps its shape.</b> The viewport is 1828x1331 and the image is square, so filling
	/// the viewport would stretch the park sideways by a third. It is fitted by whichever side runs out
	/// first instead, which is what <see cref="VirtualScreen"/> already does with the whole interface.
	/// </para>
	/// <para>
	/// <b>How far a step and a zoom go is chosen, and marked as chosen.</b> The original has these six
	/// buttons and nothing traced says what one press is worth, so a press moves an eighth of the
	/// visible width and the zoom doubles between 1x and 4x.
	/// </para>
	/// </summary>
	private sealed class MapView : UiControl
	{
		/// <summary>How much of the picture a press moves, as a fraction of what is on screen - a choice.</summary>
		private const float Step = 0.125f;

		private const float MinZoom = 1f;
		private const float MaxZoom = 4f;

		private Texture? _map;
		private bool _tried;

		private float _zoom = MinZoom;
		private float _x = 0.5f;
		private float _y = 0.5f;

		internal void Scroll( int across, int down )
		{
			var visible = 1f / _zoom;

			_x = Math.Clamp( _x + (across * Step * visible), visible * 0.5f, 1f - (visible * 0.5f) );
			_y = Math.Clamp( _y + (down * Step * visible), visible * 0.5f, 1f - (visible * 0.5f) );
		}

		internal void Zoom( bool In )
		{
			_zoom = Math.Clamp( In ? _zoom * 2f : _zoom * 0.5f, MinZoom, MaxZoom );

			// Re-centre inside the new window, so zooming out at an edge does not leave the view off the
			// picture.
			Scroll( 0, 0 );
		}

		internal void Release()
		{
			_map?.Delete();
			_map = null;
		}

		protected override void OnDraw()
		{
			if ( Load() is not { } map )
				return;

			var visible = 1f / _zoom;
			var uvs = new Rectangle( _x - (visible * 0.5f), _y - (visible * 0.5f), visible, visible );

			// Fitted rather than stretched - see the class remarks. The picture is square, the viewport
			// is not, so the shorter side decides and the rest is left as it was.
			var area = Pixels;
			var side = MathF.Min( area.Width, area.Height );
			var placed = new Rectangle(
				area.X + ((area.Width - side) * 0.5f),
				area.Y + ((area.Height - side) * 0.5f),
				side, side );

			Material.UI.Set( "Color", map );

			using ( _ = new Graphics.Scope( Screen.Size ) )
				Graphics.Quad( placed, uvs, Material.UI );
		}

		/// <summary>
		/// The park's map picture, read once. A park without one draws nothing rather than complaining
		/// every frame, which is why the attempt is remembered as well as the result.
		/// </summary>
		private Texture? Load()
		{
			if ( _tried )
				return _map;

			_tried = true;

			if ( Level.Current?.ThemeName is not { } theme )
				return null;

			var path = $"levels/{theme}/2dmap.tga";

			try
			{
				using var stream = FileSystem.OpenRead( path );

				if ( stream == null )
				{
					Log.Warning( $"Park map: {path} is not there, so the map has no picture" );
					return null;
				}

				_map = new Texture( stream );
			}
			catch ( Exception e )
			{
				Log.Warning( $"Park map: {path} would not load - {e.Message}" );
			}

			return _map;
		}
	}
}
