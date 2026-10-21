using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The golden ticket's glints, UI particle channel 0, as the advisor's clip words set them (<see cref="Advisor.SetGlints"/>,
/// Advisor_ClipStarted 0x00598e7d) and UIParticles_PlaceChannels (0x005ed770) places them (<see cref="UI.UiParticleChannel"/>;
/// <c>docs/exe/scenes.md</c>, "Gesture table"). Places are in the units effects are moved in: across 50000 a half screen,
/// down 37500. No game is needed: without a particle system the channel keeps empty handles.
/// </summary>
[TestClass]
public class AdvisorGlintTests
{
	private const double Unit = 2;

	[TestInitialize]
	public void MakeTheLog() => Log ??= new();

	private static void AssertPlace( UI.UiParticleChannel glints, int index, float now, double x, double z )
	{
		var (px, pz) = glints.PlaceOf( index, now );
		Assert.AreEqual( x, px, Unit, $"glint {index} across at {now}" );
		Assert.AreEqual( z, pz, Unit, $"glint {index} down at {now}" );
	}

	/// <summary>
	/// Clip 17's flag starts one glint at 0.34, 0.62 on an upright ellipse 0.01 by 0.2, once round a clip, and its
	/// repeats start none and leave the clock where the first set it: at 0.75 the turn is 1.5, half way round, below
	/// the centre (a clock set again at 0.3 would put it at 0.9 of a turn).
	/// </summary>
	[TestMethod]
	public void ClipSeventeenStandsOneGlintAndItsRepeatsLeaveItsClock()
	{
		var glints = new UI.UiParticleChannel();

		Assert.IsTrue( Advisor.SetGlints( glints, 0x20000, 0.5f, 0f ) );
		Assert.IsFalse( Advisor.SetGlints( glints, 0x20000, 0.5f, 0.3f ) );
		Assert.AreEqual( 1, glints.Count );

		AssertPlace( glints, 0, 0.75f, 1.34 * 50000, 1.82 * 37500 );
	}

	/// <summary>
	/// Clip 18's flag plays <c>cat_ui</c> 197, tops the channel to two and eases it from clip 17's place to the middle
	/// as the radii grow to 0.6 by 0.4, twice round: half way through, the centre is 0.17, 0.31 and the radii 0.305 by
	/// 0.3, the first glint above the centre and the second below it.
	/// </summary>
	[TestMethod]
	public void ClipEighteenEasesTwoGlintsToTheMiddle()
	{
		var glints = new UI.UiParticleChannel();
		Advisor.SetGlints( glints, 0x20000, 0.5f, 0f );

		UI.UiSounds.LastAsked = null;
		Assert.IsTrue( Advisor.SetGlints( glints, 0x40000, 2f, 1f ) );

		Assert.AreEqual( (2, (int?)197), (glints.Count, UI.UiSounds.LastAsked) );
		Assert.AreEqual( (44, 49), (glints.EffectOf( 0 ), glints.EffectOf( 1 )), "clip 17's glint stays, clip 18's is the new one" );
		AssertPlace( glints, 0, 2f, 1.17 * 50000, 1.01 * 37500 );
		AssertPlace( glints, 1, 2f, 1.17 * 50000, 1.61 * 37500 );
	}

	/// <summary>
	/// Clip 19's word (0x90000, its face-away bit too) tops it to three, spaced a third of a turn apart round the
	/// middle on the grown ellipse: the first straight above, the others at 170 and 341 of the 512 steps.
	/// </summary>
	[TestMethod]
	public void ClipNineteenSpacesThreeGlintsAThirdApart()
	{
		var glints = new UI.UiParticleChannel();
		Advisor.SetGlints( glints, 0x20000, 0.5f, 0f );
		Advisor.SetGlints( glints, 0x40000, 2f, 1f );
		Advisor.SetGlints( glints, 0x90000, 1f, 3f );

		Assert.AreEqual( 3, glints.Count );
		Assert.AreEqual( (44, 49, 49), (glints.EffectOf( 0 ), glints.EffectOf( 1 ), glints.EffectOf( 2 )), "the third is clip 19's" );
		AssertPlace( glints, 0, 3f, 50000, 0.6 * 37500 );

		foreach ( var (index, angle) in new[] { (1, 170), (2, 341) } )
		{
			var sin = ParticleSystem.Sine[angle] * 4 / 1024.0;
			var cos = ParticleSystem.Sine[(angle + 0x80) & 0x1ff] * 4 / 1024.0;
			AssertPlace( glints, index, 3f, (1 + (0.6 * sin)) * 50000, (1 - (0.4 * cos)) * 37500 );
		}
	}

	/// <summary>
	/// Clip 20's word (0x110000) empties the channel and starts two golden ticket glints (44) afresh, closing in on the top right, 0.8, -0.8, the radii shrinking to
	/// nought; past the clip's time the radii stay nought and the centre runs on: at one and a half times through it is
	/// 1.2, -1.2.
	/// </summary>
	[TestMethod]
	public void ClipTwentyClosesOnTheTopRightAndRunsOn()
	{
		var glints = new UI.UiParticleChannel();
		Advisor.SetGlints( glints, 0x20000, 0.5f, 0f );
		Advisor.SetGlints( glints, 0x40000, 2f, 1f );
		Advisor.SetGlints( glints, 0x90000, 1f, 3f );
		Assert.IsTrue( Advisor.SetGlints( glints, 0x110000, 2f, 4f ) );

		Assert.AreEqual( (2, 44, 44), (glints.Count, glints.EffectOf( 0 ), glints.EffectOf( 1 )) );
		AssertPlace( glints, 0, 7f, 2.2 * 50000, -0.2 * 37500 );
		AssertPlace( glints, 1, 5f, 1.4 * 50000, 0.8 * 37500 );
	}

	/// <summary>
	/// A centre set with no time eases from where it was on the clock the last timed set started (0x005ed660 sets
	/// the clock only for a time that is not nought): half a second after a move at 0.5 that the clock reads from 0,
	/// the centre is three quarters of the way, 0.375 across.
	/// </summary>
	[TestMethod]
	public void ACentreSetWithNoTimeKeepsTheClock()
	{
		var glints = new UI.UiParticleChannel();
		glints.SetCount( 1 );
		glints.SetCentre( true, 0f, 0f, 1f, 0f );
		glints.SetRadius( true, 0f, 0f, 1f );
		glints.SetCentre( false, 0.5f, 0f, 0f, 0.5f );

		AssertPlace( glints, 0, 0.75f, 1.375 * 50000, 37500 );
	}

	/// <summary>
	/// An emitter with a pin down the screen keeps it wherever it moves; one with none takes the third of the virtual screen
	/// it stands in, as an effect with no pin of its own takes one across. A glint closing on the top right stands in the top
	/// third, which would pin it to the top of a window taller than 4:3 and leave the bottom-pinned advisor behind.
	/// </summary>
	[TestMethod]
	public void AnEmittersPinDownIsItsOwnOrWhereItStands()
	{
		var top = new Emitter { Z = -7500 };
		var middle = new Emitter { Z = 37500 / 16 };
		var bottom = new Emitter { Z = 75000 / 16 };

		Assert.AreEqual( (UI.VerticalAnchor.Top, UI.VerticalAnchor.Middle, UI.VerticalAnchor.Bottom),
			(UI.ScreenParticles.PinDown( top ), UI.ScreenParticles.PinDown( middle ), UI.ScreenParticles.PinDown( bottom )) );

		top.Down = UI.VerticalAnchor.Bottom;
		bottom.Down = UI.VerticalAnchor.Top;

		Assert.AreEqual( (UI.VerticalAnchor.Bottom, UI.VerticalAnchor.Top), (UI.ScreenParticles.PinDown( top ), UI.ScreenParticles.PinDown( bottom )) );
	}

	/// <summary>
	/// The glints the advisor starts are pinned to the bottom of the window with him (<see cref="Advisor.SetGlints"/>),
	/// wherever the channel moves them: clip 20's two close on the top right, in the top third, and are still bottom-pinned
	/// there. Needs the game's particle library.
	/// </summary>
	[TestMethod]
	public void TheGlintsKeepTheAdvisorsBottomPinWhereverTheyMove()
	{
		Log ??= new();
		FileSystem = GameData.Required();
		var system = new ParticleSystem( "Particle/Tp2.plb", 1000 );

		try
		{
			var glints = new UI.UiParticleChannel();
			Advisor.SetGlints( glints, 0x20000, 0.5f, 0f );
			Advisor.SetGlints( glints, 0x110000, 2f, 4f );
			glints.Place( 7f );

			var started = system.Emitters.Where( emitter => emitter.Active ).ToArray();

			Assert.IsTrue( started.Length >= 2, "both glints are running" );
			Assert.IsTrue( started.All( emitter => UI.ScreenParticles.ScreenY( emitter.Z ) < UI.VirtualScreen.Height / 3f ), "moved into the top third" );
			Assert.IsTrue( started.All( emitter => UI.ScreenParticles.PinDown( emitter ) == UI.VerticalAnchor.Bottom ) );
			Assert.IsTrue( started.All( emitter => emitter.Anchor == UI.Anchor.Right ) );
		}
		finally
		{
			system.Shutdown();
		}
	}

	/// <summary>0x200000 empties the channel; a word with two of the glint flags, or with none, leaves it as it was.</summary>
	[TestMethod]
	public void OnlyOneFlagActs()
	{
		var glints = new UI.UiParticleChannel();
		Advisor.SetGlints( glints, 0x80000, 1f, 0f );

		Assert.IsFalse( Advisor.SetGlints( glints, 0x60000, 1f, 0f ) );
		Assert.IsFalse( Advisor.SetGlints( glints, 0x10000, 1f, 0f ) );
		Assert.AreEqual( 3, glints.Count );

		Assert.IsTrue( Advisor.SetGlints( glints, 0x200000, 1f, 0f ) );
		Assert.AreEqual( 0, glints.Count );
	}
}
