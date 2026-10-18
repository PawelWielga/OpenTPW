using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
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
	/// <summary>The strip's width at 800 x 600, in the font's pixels: 900 of the virtual screen's 2048 across.</summary>
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
