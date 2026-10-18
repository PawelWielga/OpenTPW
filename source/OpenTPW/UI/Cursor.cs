namespace OpenTPW.UI;

public class Cursor : Panel, Singleton<Cursor>
{
	private Texture? _texture;

	/// <summary>The pixel of the picture that stands on the pointer, from the cursor's own <c>.ani</c>.</summary>
	private (int X, int Y) _hotspot;

	private Input.CursorTypes _cursorType;

	public Input.CursorTypes CursorType
	{
		get => _cursorType;
		set
		{
			_cursorType = value;
			LoadTexture();
		}
	}

	public Cursor()
	{
		// TODO: Move this to Input
		CursorType = Input.CursorTypes.Normal;
	}

	private string GetImageName( Input.CursorTypes cursorType )
	{
		// bullfrog PLEASE...
		if ( cursorType == Input.CursorTypes.Cash )
			return "Cash";

		return "C" + cursorType.ToString()[..3].ToLower();
	}

	private void LoadTexture()
	{
		var cursorPath = $"/data/ui/cursors/{GetImageName( _cursorType )}.tga";
		_texture = new Texture( GameDir.GetPath( cursorPath ), TextureFlags.PinkChromaKey );
		_hotspot = HotspotOf( _cursorType );
	}

	/// <summary>
	/// The hotspot of the <c>.ani</c> the original loads for this cursor: the file whose name after <c>c_</c> starts
	/// with the picture's three letters (<c>c_crosshair.ani</c> for <c>Ccro.tga</c>). Only the crosshair's is not
	/// (0, 0) (FileFormats docs, "Cursors"); a picture with no such file (<c>Csdn</c>, <c>Csup</c>, <c>Cnog</c>, <c>Cash</c>)
	/// stands by its top-left corner.
	/// </summary>
	internal static (int X, int Y) HotspotOf( Input.CursorTypes cursorType )
	{
		var folder = GameDir.GetPath( "/data/ui/cursors" );
		var tag = cursorType.ToString()[..3].ToLower();

		if ( cursorType == Input.CursorTypes.Cash || !Directory.Exists( folder ) )
			return (0, 0);

		var file = Directory.GetFiles( folder, $"c_{tag}*.ani", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive } )
			.Order().FirstOrDefault();

		if ( file == null )
			return (0, 0);

		using var stream = File.OpenRead( file );
		return new AnimatedCursorFile( stream ).Hotspot;
	}

	protected override void OnRender()
	{
		if ( _texture == null )
			return;

		var material = Material.UI;
		material.Set( "Color", _texture );

		var size = new Vector2( 32 );

		var position = Input.Mouse.Position;
		position.X -= _hotspot.X;
		position.Y = Screen.Height - position.Y;
		position.Y -= size.Y - _hotspot.Y;

		var frame = ((Time.Now * 3).CeilToInt()) % 4;
		var uv0 = new Vector2( frame * 0.25f, 0f );
		var uv1 = uv0 + new Vector2( 0.25f, 1f );

		var rect = new Rectangle( position, size );
		var uvRect = new Rectangle( uv0, uv1 - uv0 );

		using ( _ = new Graphics.Scope( Screen.Size ) )
		{
			Graphics.Quad( rect, uvRect, material );
		}
	}
}
