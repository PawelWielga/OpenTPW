using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Linq;
using System.Reflection;

namespace OpenTPW.Tests;

/// <summary>
/// The help bar's lettering (<c>docs/exe/hud.md</c>, "The help bar's lettering"): the strip's label wraps at its own width,
/// by the renderer's word-wrapper <c>0x006aec80</c>, and stands in the middle, or at the top where the text holds a line
/// break. The rows' breaks are the original's, read in its frames at 800 x 600, the font set ours draws with at
/// 1280 x 720 (<c>q471/orig/b-2-gate.png</c>, <c>q478/orig/b-3-ride.png</c>).
/// </summary>
[TestClass]
public class HelpBarWrapTests
{
	/// <summary>The strip's width at 800 x 600, in the font's pixels: 900 of the virtual screen's 2048 across. The live width is tested apart.</summary>
	private const int StripWidth = 900 * 800 / 2048;

	private bool _popupHelp;

	[TestInitialize]
	public void Keep()
	{
		_popupHelp = GameOptions.Current.PopupHelp;
		GameOptions.Current.PopupHelp = true;
		Log ??= new();
	}

	[TestCleanup]
	public void PutBack() => GameOptions.Current.PopupHelp = _popupHelp;

	/// <summary>Font 7 of the set drawn at 800 x 600 (the third), which the strip letters in.</summary>
	private static BitmapFont Font()
	{
		FileSystem = GameData.Required();

		return BitmapFont.Get( "GAME9.bf4" ) ?? throw new AssertFailedException( "GAME9.bf4 did not load" );
	}

	[TestMethod]
	public void TheGatesRowBreaksBeforeTicketAsTheOriginals()
	{
		var font = Font();
		var lines = UiText.Wrap( font, Localization.Help( 459 ), StripWidth );

		CollectionAssert.AreEqual( new[] { "Park gates - left-click to open/close the park or change the", "ticket price (G)" }, lines );
	}

	[TestMethod]
	public void AnObjectsRowBreaksBeforeCtrlAsTheOriginals()
	{
		var font = Font();
		var lines = UiText.Wrap( font, Localization.FormatHelp( 550, (2, "Belly Bounce") ), StripWidth );

		CollectionAssert.AreEqual( new[] { "Belly Bounce: Left-click to view/edit this item's details. Hold", "CTRL and click to buy another" }, lines );
	}

	[TestMethod]
	public void ALandRowKeepsItsOwnTwoLines()
	{
		var font = Font();
		var lines = UiText.Wrap( font, Localization.Help( 0x21a ), StripWidth );

		CollectionAssert.AreEqual( new[] { "Left-click to buy land", "The cost shown is per tile of land" }, lines );
	}

	/// <summary>A line as wide as the width fits; one pixel less and its last word goes down, the space between left off both.</summary>
	[TestMethod]
	public void ALineAsWideAsTheWidthFits()
	{
		var font = Font();
		var width = font.Measure( "Left-click to build path" );

		CollectionAssert.AreEqual( new[] { "Left-click to build path" }, UiText.Wrap( font, "Left-click to build path", width ) );
		CollectionAssert.AreEqual( new[] { "Left-click to build", "path" }, UiText.Wrap( font, "Left-click to build path", width - 1 ) );
	}

	/// <summary>Spaces run together are left off the end of the line and the start of the next, a tab with them.</summary>
	[TestMethod]
	public void SpacesAtABreakAreLeftOffBothLines()
	{
		var font = Font();
		var width = font.Measure( "Left-click  " );

		CollectionAssert.AreEqual( new[] { "Left-click", "to" }, UiText.Wrap( font, "Left-click  \t to", width ) );
	}

	/// <summary>A word wider than the width is cut where it overflows; a character wider than the width stands alone.</summary>
	[TestMethod]
	public void AWordWiderThanTheWidthIsCut()
	{
		var font = Font();

		CollectionAssert.AreEqual( new[] { "WW", "WW", "W" }, UiText.Wrap( font, "WWWWW", font.Measure( "WW" ) + 1 ) );
		CollectionAssert.AreEqual( new[] { "W", "W" }, UiText.Wrap( font, "WW", 1 ) );
	}

	/// <summary>
	/// <b>The strip's width is the drawn width over the font's scale, cut toward nought</b>, at each of the four sets'
	/// screens (512 x 384 to 1024 x 768, where the scale is 1) and on two windows drawn larger than their set (1280 x 720, 1920 x 1080): the width <c>UiText.Draw</c> hands the wrapper, not a constant of the test's own.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the width rounded to nearest gives 352 at the third set; the scale left out
	/// gives 421 at 1280 x 720.
	/// </remarks>
	[TestMethod]
	public void TheStripsWidthIsTheDrawnWidthOverTheScaleCutTowardNought()
	{
		var screen = Screen.Size;

		try
		{
			foreach ( var (width, height, expected) in new[] { (512, 384, 225), (640, 480, 281), (800, 600, 351), (1024, 768, 450), (1280, 720, 351), (1920, 1080, 450) } )
			{
				Screen.Size = new Point2( width, height );

				var strip = VirtualScreen.ToPixels( new UiRect( HelpBar.Left, 0, HelpBar.Right, 1 ), Anchor.Centre, VerticalAnchor.Middle );

				Assert.AreEqual( expected, UiText.WrapWidthOf( strip.Width ), $"{width} x {height}" );
			}
		}
		finally
		{
			Screen.Size = screen;
		}
	}

	/// <summary>
	/// A line that starts with blanks and overflows keeps its first blank: the trimming of the blanks before a break stops at
	/// the line's second character (0x006aecfb), where trimming them all would leave an empty line.
	/// </summary>
	/// <remarks><b>Mutations:</b> the trim without its <c>end - 1 != start</c> stop gives an empty first line.</remarks>
	[TestMethod]
	public void ALineThatStartsWithBlanksKeepsItsFirstOne()
	{
		var font = Font();
		var lines = UiText.Wrap( font, "   WWWWWW", font.Measure( "   WW" ) );

		Assert.AreEqual( " ", lines[0] );
		Assert.AreEqual( "WWWWWW", string.Concat( lines.Skip( 1 ) ), "the rest is cut where it overflows" );
	}

	/// <summary>A row with a break of its own stands at the top of the strip, any other in the middle; both wrap.</summary>
	[TestMethod]
	public void ARowWithABreakStandsAtTheTop()
	{
		Font();
		var bar = new HelpBar();
		var control = (UiControl)typeof( HelpBar ).GetField( "_bar", BindingFlags.NonPublic | BindingFlags.Instance )!.GetValue( bar )!;

		Assert.IsTrue( control.TextWraps, "the strip wraps (its skin's 0x3000 ( 1, 1 ))" );

		bar.Update( 0x21a );
		Assert.AreEqual( TextAlign.Start, bar.Down, "538 holds a line break" );

		bar.Update( 459 );
		Assert.AreEqual( TextAlign.Centre, bar.Down, "459 holds none" );

		bar.Update( 0x21b );
		Assert.AreEqual( TextAlign.Start, bar.Down, "539 holds a line break" );
	}
}
