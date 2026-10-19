namespace OpenTPW.UI;

/// <summary>
/// A control's text, drawn in one of the interface's fonts.
///
/// <para>
/// The original renders a control's text into a buffer of its own and puts that on the screen, and
/// this does the same: the lettering is laid out once into a texture at the font's own pixel size,
/// scaled onto the window from there, and only laid out again when the text, font, colour or wrap
/// width changes.
/// </para>
/// <para>
/// The purple skin every big button and dialog title wears (0x00491ab0) draws its text twice: first
/// in grey at half strength, two pixels to the left and two down, and then at full strength where it
/// belongs, which gives the lettering its dark edge. The plain label skin (0x0048f830) draws it once.
/// The original's first pass sets the glyph colour to (0x80, 0x80, 0x80, 0x80); this halves the text's own
/// colour instead, and whether the original's grey replaces the colour or combines with it is not read.
/// docs/exe/ui.md, "The two-pass text class".
/// </para>
/// </summary>
internal sealed class UiText
{
	private const int ShadowOffset = 2;

	private Texture? _texture;
	private int _width;
	private int _height;
	private Layout _built;
	private List<string> _lines = [];

	private readonly record struct Layout( string Text, int Font, int Set, UiColour Colour, bool Shadow, int WrapWidth, TextAlign Across );

	public void Draw( string? text, int font, UiColour colour, bool shadow, bool wrap, PixelRect area, TextAlign across, TextAlign down )
	{
		if ( string.IsNullOrEmpty( text ) || UiFonts.Get( font ) is not { } bitmap )
			return;

		var scale = UiFonts.Scale;
		var layout = new Layout( text, font, UiFonts.SetIndex, colour, shadow, wrap ? WrapWidthOf( area.Width ) : 0, across );

		if ( _texture == null || layout != _built )
			Build( bitmap, layout );

		if ( _texture == null )
			return;

		// Aligned by the text itself; the shadow hangs off its left and bottom.
		var margin = shadow ? ShadowOffset : 0;
		var textWidth = (_width - margin) * scale;
		var textHeight = (_height - margin) * scale;

		var x = Align( area.X, area.Width, textWidth, across ) - (margin * scale);
		var y = Align( area.Y, area.Height, textHeight, down );

		var width = _width * scale;
		var height = _height * scale;

		Material.UI.Set( "Color", _texture );

		using ( _ = new Graphics.Scope( Screen.Size ) )
			Graphics.Quad( new Rectangle( MathF.Round( x ), MathF.Round( Screen.Height - y - height ), width, height ), Material.UI );
	}

	/// <summary>
	/// The width a wrapping control's text breaks at, in the font's pixels: the control's drawn width over the font's scale,
	/// cut toward nought as the original cuts it ((x1 - x0) x the screen width / 2048, <c>0x0048f47b</c>; docs/exe/hud.md,
	/// "The help bar's lettering").
	/// </summary>
	internal static int WrapWidthOf( float areaWidth ) => (int)(areaWidth / UiFonts.Scale);

	/// <summary>The lines the text was last laid out in, and the width in the font's pixels they were wrapped at (0 for none).</summary>
	public IReadOnlyList<string> Lines => _lines;

	public int WrapWidth => _built.WrapWidth;

	/// <summary>Releases the texture. Drawing again lays the text out afresh.</summary>
	public void Delete()
	{
		_texture?.Delete();
		_texture = null;
	}

	private static float Align( float start, float space, float size, TextAlign align ) => align switch
	{
		TextAlign.Start => start,
		TextAlign.End => start + space - size,
		_ => start + ((space - size) * 0.5f)
	};

	private void Build( BitmapFont font, Layout layout )
	{
		_built = layout;

		var lines = _lines = Wrap( font, layout.Text, layout.WrapWidth );
		var margin = layout.Shadow ? ShadowOffset : 0;

		var textWidth = Math.Max( 1, lines.Max( line => font.Measure( line ) ) );
		var textHeight = Math.Max( 1, lines.Count * font.LineHeight );

		var width = textWidth + margin;
		var height = textHeight + margin;
		var pixels = new byte[width * height * 4];

		var shadow = new UiColour( (byte)(layout.Colour.R / 2), (byte)(layout.Colour.G / 2), (byte)(layout.Colour.B / 2) );

		for ( int i = 0; i < lines.Count; ++i )
		{
			var x = (int)Align( 0, textWidth, font.Measure( lines[i] ), layout.Across );
			var y = i * font.LineHeight;

			if ( layout.Shadow )
				font.Write( lines[i], pixels, width, height, x, y + margin, shadow, 0.5f );

			font.Write( lines[i], pixels, width, height, x + margin, y, layout.Colour, 1f );
		}

		if ( _texture != null && _texture.Width == width && _texture.Height == height )
		{
			_texture.UpdatePixels( pixels );
		}
		else
		{
			_texture?.Delete();
			_texture = new Texture( pixels, width, height, TextureFlags.OwnPixels );
		}

		_width = width;
		_height = height;
	}

	/// <summary>
	/// The text's lines: split where it says, and where a line is wider than <paramref name="wrapWidth"/> if that is not 0.
	/// The break is the renderer's word-wrapper's (0x006aec80): the pen's advances are added a character at a time, and
	/// the character that takes the sum past the width ends the line before the last space or tab at or after the line's
	/// second character, or before itself where there is none, a lone character keeping its line. Spaces and tabs before
	/// the break are left off the line and those after it off the next.
	///
	/// <para>
	/// <b>Where it differs from the original</b>, none of it reached by shipped text (dead by CONTENT: no row of either
	/// language folder ends in a line feed or holds a carriage return). The original lays each paragraph with its line
	/// feed (0x006aeb4b): blanks cut at a break that land before a line feed add one more, empty line (0x006aed53), a line
	/// feed that ends the text lays no further paragraph and an empty text lays no line (0x006aeb81, 0x006aeb32), a
	/// carriage return is a break of its own, so CR LF is two. Here the line feeds are split off first, a trailing empty
	/// paragraph and an empty text each lay one line, a lone carriage return is a character and CR LF is one break.
	/// </para>
	/// </summary>
	internal static List<string> Wrap( BitmapFont font, string text, int wrapWidth )
	{
		var lines = new List<string>();

		foreach ( var paragraph in text.Replace( "\r\n", "\n" ).Split( '\n' ) )
		{
			if ( wrapWidth <= 0 )
			{
				lines.Add( paragraph );
				continue;
			}

			var start = 0;
			var sum = 0;

			for ( int i = 0; i < paragraph.Length; ++i )
			{
				var advance = font.Measure( paragraph.AsSpan( i, 1 ) );

				// A line's first character stays on it however wide: the original gives it a line of its own, and the next
				// character, overflowing at once, then cuts before itself.
				if ( sum + advance <= wrapWidth || i == start )
				{
					sum += advance;
					continue;
				}

				var at = i;

				while ( at != start && !IsSpace( paragraph[at] ) )
					--at;

				var end = at == start ? i : at;
				var next = end;

				while ( at != start && end - 1 != start && IsSpace( paragraph[end - 1] ) )
					--end;

				while ( next < paragraph.Length && IsSpace( paragraph[next] ) )
					++next;

				lines.Add( paragraph[start..end] );
				start = next;
				sum = 0;
				i = next - 1;
			}

			if ( start < paragraph.Length || paragraph.Length == 0 )
				lines.Add( paragraph[start..] );
		}

		return lines;
	}

	private static bool IsSpace( char character ) => character is ' ' or '\t';
}

internal enum TextAlign
{
	Start,
	Centre,
	End
}
