using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;

namespace OpenTPW.Tests;

/// <summary>
/// The management gadget's happiness gauge as drawn: the <c>meter.wct</c> skin's two quads (<c>Meter_Construct</c>,
/// <c>Meter_SetQuadGeometry</c>, <c>Meter_SetQuadUVs</c>; <c>docs/exe/hud.md</c>, "The happiness gauge's two quads") and
/// the units the gadget hands it. None of these needs the game.
/// </summary>
[TestClass]
public class UiMeterTests
{
	/// <summary>
	/// <b>Two halves cropped at one boundary</b>: above it the texture's right half, the dark capsule, from v at the
	/// boundary to the top; below it the left half, the lit one, from the bottom to the boundary. 716 of 1024 is a
	/// boundary 69.92% up a box 100 high.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the halves swapped; u 0 to 1 on both (the two copies side by side); the lit half squashed
	/// (v 0 to 1); the fill from the top; the boundary as a share of 100 rather than 1024.
	/// </remarks>
	[TestMethod]
	public void TheDarkRightHalfStandsAboveTheBoundaryAndTheLitLeftHalfBelowEachCropped()
	{
		var box = new PixelRect( 10, 20, 40, 100 );
		var meter = new UiMeter { Max = 0x400, Value = 716 };

		Assert.AreEqual( 69.921875f, meter.Percent, 1e-4f, "716 x 1/1024 x 100" );

		var quads = UiMeter.Quads( box, meter.Percent );
		var t = 0.69921875f;

		Assert.AreEqual( 10f, quads[0].Where.X );
		Assert.AreEqual( 20f, quads[0].Where.Y, "the dark half from the box's top" );
		Assert.AreEqual( 40f, quads[0].Where.Width, "over the whole width" );
		Assert.AreEqual( 30.078125f, quads[0].Where.Height, 1e-3f, "down to the boundary" );
		Assert.AreEqual( new Rectangle( 0.5f, t, 0.5f, 1f - t ), quads[0].Uvs, "the right half, from the boundary to the top" );

		Assert.AreEqual( 50.078125f, quads[1].Where.Y, 1e-3f, "the lit half from the boundary" );
		Assert.AreEqual( 40f, quads[1].Where.Width, "over the whole width" );
		Assert.AreEqual( 69.921875f, quads[1].Where.Height, 1e-3f, "to the box's bottom" );
		Assert.AreEqual( new Rectangle( 0f, 0f, 0.5f, t ), quads[1].Uvs, "the left half, from the bottom to the boundary" );
	}

	/// <summary>
	/// <b>At nought the dark capsule fills the box</b>, as the original's constructor builds it before any value
	/// arrives, and the lit half has no height; a full meter is the reverse.
	/// </summary>
	/// <remarks><b>Mutations:</b> nothing drawn at nought; no clamp.</remarks>
	[TestMethod]
	public void AtNoughtTheDarkHalfFillsTheBox()
	{
		var box = new PixelRect( 10, 20, 40, 100 );

		var none = UiMeter.Quads( box, new UiMeter { Max = 0x400, Value = -5 }.Percent );

		Assert.AreEqual( new Rectangle( 10, 20, 40, 100 ), none[0].Where, "the whole box" );
		Assert.AreEqual( new Rectangle( 0.5f, 0f, 0.5f, 1f ), none[0].Uvs, "the whole right half" );
		Assert.AreEqual( 0f, none[1].Where.Height, "no lit half" );

		var full = UiMeter.Quads( box, 150f );

		Assert.AreEqual( 0f, full[0].Where.Height, "no dark half" );
		Assert.AreEqual( new Rectangle( 0f, 0f, 0.5f, 1f ), full[1].Uvs, "the whole left half" );
	}

	/// <summary>
	/// <b>The gadget hands the meter the mean times 1024 times 0.01f, truncated</b> (<c>0x004a1d44</c>): 0.01f is a
	/// shade under a hundredth, so 70 is 716, 50 is 511 and 100 is 1023 at 53-bit precision, where an exact scale gives
	/// 716.8, 512 and 1024 (as 24-bit precision does for 50 and 100).
	/// </summary>
	/// <remarks><b>Mutations:</b> 0.01 as a double; a rounding conversion; the mean handed as it is.</remarks>
	[TestMethod]
	public void TheGadgetHandsTheMeterTheMeanIn1024thsTruncated()
	{
		Assert.AreEqual( 716, ParkGadget.GaugeUnits( 70 ) );
		Assert.AreEqual( 511, ParkGadget.GaugeUnits( 50 ) );
		Assert.AreEqual( 1023, ParkGadget.GaugeUnits( 100 ) );
		Assert.AreEqual( 307, ParkGadget.GaugeUnits( 30 ) );
	}

	/// <summary>
	/// <b>The bar fills from the bottom on screen</b>: the draw's y of 0 is the screen's bottom, so a quad's box is
	/// flipped there, and the lit half, which <see cref="UiMeter.Quads"/> puts at the box's bottom, lands lowest.
	/// </summary>
	/// <remarks><b>Mutations:</b> the box's y passed through unflipped (the bar fills from the top).</remarks>
	[TestMethod]
	public void TheLitHalfIsDrawnLowestOnTheScreen()
	{
		var quads = UiMeter.Quads( new PixelRect( 10, 500, 40, 100 ), 30f );

		var dark = UiMeter.OnScreen( quads[0].Where, 768f );
		var lit = UiMeter.OnScreen( quads[1].Where, 768f );

		Assert.AreEqual( 168f, lit.Y, 1e-3f, "the box's bottom 30, from 768 - 600 up" );
		Assert.AreEqual( 30f, lit.Height, 1e-3f );
		Assert.AreEqual( 198f, dark.Y, 1e-3f, "the dark half from the lit half's top" );
		Assert.AreEqual( 70f, dark.Height, 1e-3f );
	}

	/// <summary>
	/// <b>The gadget's gauge is full at 1024</b>, the units <see cref="ParkGadget.GaugeUnits"/> hands it: a mean of 10
	/// stands about a tenth up, not full.
	/// </summary>
	/// <remarks><b>Mutations:</b> the gauge left at the meter's default full scale of a hundred (full from a mean of about 10).</remarks>
	[TestMethod]
	public void TheGadgetsGaugeIsFullAt1024()
	{
		var gauge = ParkGadget.HappinessGauge();
		gauge.Value = ParkGadget.GaugeUnits( 10 );

		Assert.AreEqual( 0x400, gauge.Max );
		Assert.AreEqual( 9.9609375f, gauge.Percent, 1e-4f, "102 of 1024" );
	}
}
