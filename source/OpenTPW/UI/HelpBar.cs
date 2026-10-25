namespace OpenTPW.UI;

/// <summary>
/// The strip along the bottom of the screen that says what the control under the pointer does -
/// "Left-click to view next island".
///
/// <para>
/// 0x00492180 builds it: from x 574 to 1474 on the virtual screen with its bottom at 1520, and as
/// tall as twice the help font's line at the game's resolution plus ten, worked out as
/// <c>height * 0xc00 / screen height + 10</c>. It wears f_helpbg.md2, a single quad of translucent
/// black, with the text centred on it in white in font 7, wrapped at the strip's width and standing in the
/// middle, or at the top where the text holds a line break of its own (docs/exe/hud.md, "The help bar's
/// lettering"). Each control carries the row of UIHELPTEXT.str it shows in its layout data (op 0x11), and a
/// control with none shows nothing.
/// </para>
/// <para>
/// It shows only while the options screen's Popup help is on (<see cref="GameOptions.PopupHelp"/>).
/// Ctrl+H turns that off and back on too, through the help bar's binding
/// (<see cref="InputButton.ToggleHelpBar"/>).
/// </para>
/// <para>
/// <b>Engine and content.</b> The strip is engine, the same under every scene's windows. Which row a control
/// shows is the content of the layout that made the control.
/// </para>
/// </summary>
internal sealed class HelpBar
{
	internal const int Left = 574;
	internal const int Right = 1474;
	private const int Bottom = 1520;
	private const int HelpFont = 7;

	private readonly UiControl _bar = new()
	{
		Rect = new UiRect( Left, Bottom - 40, Right, Bottom ),
		Mesh = UiMesh.Get( "f_helpbg" ),
		Font = HelpFont,
		TextWraps = true,
		Visible = false
	};

	/// <summary>
	/// Where its top stands instead of its bottom at 1520, or null for the bottom: the park map moves it to the top of
	/// the screen while it is up (<c>FUN_00492530</c>, which moves the label's top and keeps its left where handed -1).
	/// </summary>
	public int? Top { get; set; }

	/// <summary>Whether it is switched on - the original's Popup Help option, which the button glints also wait on.</summary>
	public bool Enabled => GameOptions.Current.PopupHelp;

	/// <summary>Shows row <paramref name="helpText"/>, or nothing for -1, its parameters filled with <paramref name="values"/>.</summary>
	public void Update( int helpText, params (int Parameter, string Value)[] values )
	{
		// On the press. The original toggles it on the release, through the system table its window procedure matches
		// on every key (0x0040c5d0; docs/QUEUE.md Q65).
		if ( Input.Pressed( InputButton.ToggleHelpBar ) )
			GameOptions.Current.PopupHelp = !GameOptions.Current.PopupHelp;

		var text = OurKeys( !Enabled || helpText < 0 ? ""
			: values.Length > 0 ? Localization.FormatHelp( helpText, values )
			: Localization.Help( helpText ) );
		_bar.Visible = text.Length > 0;

		if ( !_bar.Visible )
			return;

		_bar.Text = text;
		_bar.TextDown = text.Contains( '\n' ) ? TextAlign.Start : TextAlign.Centre;

		var lineHeight = UiFonts.Get( HelpFont )?.LineHeight ?? 13;
		var height = (lineHeight * 0xc00 / UiFonts.SetScreenHeight) + 10;
		_bar.Rect = Top is { } top ? new UiRect( Left, top, Right, top + height ) : new UiRect( Left, Bottom - height, Right, Bottom );
	}

	/// <summary>
	/// A row's text with the keys this game moved: the nine rows that end "(A)" list all the items and "(S)" all the
	/// staff, keys that pan the camera here, so they end "(O)" and "(E)" instead (<c>ParkFrontEnd.ShortcutRow</c>). Not
	/// the shipped text; no other row ends in either.
	/// </summary>
	internal static string OurKeys( string text )
		=> text.EndsWith( " (A)", StringComparison.Ordinal ) ? text[..^3] + "(O)"
			: text.EndsWith( " (S)", StringComparison.Ordinal ) ? text[..^3] + "(E)"
			: text;

	/// <summary>What the strip says now, or nothing while it is hidden.</summary>
	public string Text => _bar.Visible ? _bar.Text ?? "" : "";

	/// <summary>The lines the strip last set its text in, the width they were wrapped at in the font's pixels, and where they stand.</summary>
	public IReadOnlyList<string> Lines => _bar.TextLines;

	public int WrapWidth => _bar.TextWrapWidth;

	public TextAlign Down => _bar.TextDown;

	public void Draw() => _bar.Draw();

	/// <summary>Lets go of the text it has drawn, as its scene ends.</summary>
	public void ReleaseText() => _bar.ReleaseText();
}
