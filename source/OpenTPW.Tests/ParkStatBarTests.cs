using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park screens' type-9 bars and their skin, <c>ridestatbar.wct</c> (<c>FUN_00477ff0</c>, the skin's value at
/// <c>0x00478593</c>; <c>docs/exe/hud.md</c>, "The stat bars' skin"): what each screen hands its bar, and the two
/// quads the skin draws. None of these needs the game.
/// </summary>
[TestClass]
public class ParkStatBarTests
{
	/// <summary>
	/// <b>The skin is two quads over the whole box, each part cropped</b>: the filled part from the left edge to the
	/// value shows the colours from the texture's left edge to the value, and the rest shows the grey from the value
	/// to the right edge; nothing beyond the ends at nought or a full bar.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the halves swapped; the fill squashed (the whole texture's width at any value); the rest
	/// drawn from the left edge; the box inset; no clamp.
	/// </remarks>
	[TestMethod]
	public void TheSkinDrawsTheFilledPartInColourAndTheRestInGreyEachCropped()
	{
		var box = new PixelRect( 100, 40, 200, 20 );

		var parts = ParkObjectWindow.UiStatBar.Parts( box, 0.25f );

		Assert.AreEqual( new Rectangle( 100, 40, 50, 20 ), parts[0].Where, "the filled part" );
		Assert.AreEqual( new Rectangle( 0f, 0.5f, 0.25f, 0.5f ), parts[0].Uvs, "the colours, as far as the value" );
		Assert.AreEqual( new Rectangle( 150, 40, 150, 20 ), parts[1].Where, "the rest" );
		Assert.AreEqual( new Rectangle( 0.25f, 0f, 0.75f, 0.5f ), parts[1].Uvs, "the grey, from the value on" );

		var full = ParkObjectWindow.UiStatBar.Parts( box, 1.5f );

		Assert.AreEqual( new Rectangle( 100, 40, 200, 20 ), full[0].Where, "held at full" );
		Assert.AreEqual( 0f, full[1].Where.Width, "nothing left over" );

		var none = ParkObjectWindow.UiStatBar.Parts( box, -1f );

		Assert.AreEqual( 0f, none[0].Where.Width, "held at nought" );
		Assert.AreEqual( new Rectangle( 0f, 0f, 1f, 0.5f ), none[1].Uvs, "the whole grey track" );
	}

	/// <summary>
	/// <b>The bar takes the original's 0 to 1024</b>, held at both ends as the control's setter holds it
	/// (<c>FUN_0066d885</c>), and the hire screen hands it <c>((grade &amp; 0xff) &lt;&lt; 10) / 5</c>, so a grade 4 fills
	/// 819 of 1024.
	/// </summary>
	/// <remarks><b>Mutations:</b> over 1000 not 1024; no clamp; the grade over 4; a shift of 8.</remarks>
	[TestMethod]
	public void TheBarTakesTheOriginalsUnitsAndTheHireScreenHandsItTheGradeOverFive()
	{
		var bar = new ParkObjectWindow.UiStatBar { Units = 614 };

		Assert.AreEqual( 614 / 1024f, bar.At );

		bar.Units = 2000;
		Assert.AreEqual( 1f, bar.At );

		bar.Units = -3;
		Assert.AreEqual( 0f, bar.At );

		CollectionAssert.AreEqual( new[] { 0, 204, 409, 614, 819 }, new[] { 0, 1, 2, 3, 4 }.Select( ParkHireScreen.SkillUnits ).ToArray() );
	}
}
