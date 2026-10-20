using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OpenTPW.Tests;

/// <summary>
/// A research panel's pump (<c>ResearchPump_SetRate</c> <c>0x004a95b0</c>, <c>ResearchPump_Tick</c> <c>0x004a9fc0</c>;
/// <c>docs/exe/hud.md</c>, "The pump"), and the slider thumb's press and release that start and fade the effort's sound.
/// </summary>
[TestClass]
public class ResearchPumpTests
{
	/// <summary>
	/// <b>A rate starts the pump, which fills and then runs</b>: a rate of 2 is a speed of <c>22 × 0x10000 / 100</c> = 14417,
	/// so 28834 a pass; from <c>0x30</c> it passes <c>0x3f</c> on the 37th pass (36 reach 0x3f's 0x3fd6d0) and runs from
	/// nought, and the run passes <c>0x1f</c> on its 73rd. A rate of nought drains it from <c>0x20</c>, and after 37 passes
	/// it rests at <c>0x30</c>. A new rate while it runs changes only its speed; nought on a stopped pump changes nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> one speed a pass rather than two; the run's wrap at 0x20 kept as a frame; a running pump
	/// restarted by a new rate; the stop resting at 0x2f; a stopped pump moved.</remarks>
	[TestMethod]
	public void ARateStartsThePumpWhichFillsThenRuns()
	{
		var pump = new UI.ResearchPump();

		pump.Pass();
		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopped, 0x30, 0), (pump.State, pump.Frame, pump.Passes), "a stopped pump does not move" );

		pump.SetRate( 2 );
		Assert.AreEqual( (UI.ResearchPump.PumpState.Starting, 0x30, 0x300000, 14417), (pump.State, pump.Frame, pump.Position, pump.Speed) );

		for ( var pass = 0; pass < 36; ++pass )
			pump.Pass();

		Assert.AreEqual( (UI.ResearchPump.PumpState.Starting, 0x3f, 0x300000 + (36 * 28834)), (pump.State, pump.Frame, pump.Position) );

		pump.Pass();
		Assert.AreEqual( (UI.ResearchPump.PumpState.Running, 0, 0), (pump.State, pump.Frame, pump.Position) );

		for ( var pass = 0; pass < 72; ++pass )
			pump.Pass();

		Assert.AreEqual( (0x1f, 72 * 28834), (pump.Frame, pump.Position) );
		pump.Pass();
		Assert.AreEqual( (UI.ResearchPump.PumpState.Running, 0, 0), (pump.State, pump.Frame, pump.Position), "the run comes round" );

		pump.Pass();
		pump.SetRate( 5 );
		Assert.AreEqual( (UI.ResearchPump.PumpState.Running, 0, 28834, 16384), (pump.State, pump.Frame, pump.Position, pump.Speed), "only the speed" );

		pump.SetRate( 0 );
		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopping, 0x20, 0x200000, 16384), (pump.State, pump.Frame, pump.Position, pump.Speed) );

		for ( var pass = 0; pass < 31; ++pass )
			pump.Pass();

		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopping, 0x2f), (pump.State, pump.Frame), "31 passes of 32768 reach 0x2f" );
		pump.Pass();
		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopped, 0x30, 0x300000), (pump.State, pump.Frame, pump.Position) );

		var passes = pump.Passes;
		pump.SetRate( 0 );
		pump.Pass();
		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopped, 0x30, 16384, passes), (pump.State, pump.Frame, pump.Speed, pump.Passes) );
	}

	/// <summary>
	/// <b>A rate given to a stopping pump starts it again from <c>0x30</c></b>, and nought given to a starting one drains it.
	/// </summary>
	[TestMethod]
	public void AStoppingPumpStartsAgain()
	{
		var pump = new UI.ResearchPump();

		pump.SetRate( 80 );
		pump.Pass();
		pump.SetRate( 0 );
		Assert.AreEqual( (UI.ResearchPump.PumpState.Stopping, 0x20), (pump.State, pump.Frame), "a starting pump drains" );

		pump.Pass();
		pump.SetRate( 1 );
		Assert.AreEqual( (UI.ResearchPump.PumpState.Starting, 0x30, 0x300000, 13762), (pump.State, pump.Frame, pump.Position, pump.Speed) );
	}

	/// <summary>
	/// <b>A slider's thumb says when it is taken hold of and let go</b> (messages <c>0x801</c> and <c>0x802</c>), the release
	/// after the thumb is put where its value belongs: dragged to 95, the value 52, and put back at 93 before it is told.
	/// A click on the track says neither.
	/// </summary>
	[TestMethod]
	public void ASlidersThumbSaysItsPressAndRelease()
	{
		var screen = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );

		try
		{
			ThumbSays();
		}
		finally
		{
			Screen.Size = screen;
		}
	}

	private static void ThumbSays()
	{
		var said = new List<string>();
		var slider = new UI.UiSlider { Rect = new UI.UiRect( 0, 0, 200, 40 ), HitRect = new UI.UiRect( 0, 0, 200, 40 ), Track = new UI.UiRect( 0, 0, 200, 40 ), Maximum = 100 };
		var thumb = slider.AddThumb( new UI.UiSliderThumb { Rect = new UI.UiRect( 0, 0, 20, 40 ) } );

		slider.ThumbPressed = () => said.Add( $"pressed {slider.Value}" );
		slider.ThumbReleased = () => said.Add( $"released {thumb.Rect.Left}" );
		slider.SetValue( 50 );

		slider.LeftClickedAt( 190, 10 );
		CollectionAssert.AreEqual( Array.Empty<string>(), said, "a page click" );

		thumb.PointerPressed( 100, 10 );
		thumb.PointerDragged( 104, 10 );
		Assert.AreEqual( (95, 52), (thumb.Rect.Left, slider.Value) );
		thumb.PointerReleased();
		CollectionAssert.AreEqual( new[] { "pressed 51", "released 93" }, said );
	}
}
