using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The gesture table's rows as the advisor plays them (<see cref="AdvisorGestures"/>; <c>docs/exe/scenes.md</c>, "Gesture
/// table" and "A row and the voice's start"), the clip lengths they add up, and what a clip of <c>0x10000</c> leaves off.
/// The numbers are the original's own, read from its memory saying Local 0's ticket and key (<c>q707/orig/O</c>). The
/// clip lengths read Lost Kingdom and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class AdvisorGestureTests
{
	/// <summary>Lost Kingdom's advisor's clip lengths as the original's table holds them (clip 14 nought).</summary>
	private static readonly Dictionary<int, int> Jungle = new()
	{
		[14] = 0, [15] = 666, [16] = 28666, [17] = 633, [18] = 2966, [19] = 1300, [20] = 2000, [11] = 3333, [5] = 5000, [12] = 5000,
		[1] = 1666, [6] = 7500, [4] = 4166, [9] = 10833
	};

	private static int Length( int clip ) => Jungle.GetValueOrDefault( clip );

	/// <summary>
	/// Row 13, a golden ticket's key: clips 17 four times, 18, 19, 20, the cue's two words read out of the clips; its
	/// clips end 8798 ms on and its cue 7798 ms on (the clips before it and 1000), as the original's; no lead-in, the
	/// first clip being 17; the flags each clip carries.
	/// </summary>
	[TestMethod]
	public void RowThirteenIsTheTicketsClipsAndTheKeysCue()
	{
		var row = AdvisorGestures.Read( 13, Length )!.Value;

		CollectionAssert.AreEqual( new[] { 17, 17, 17, 17, 18, 19, 20 }, row.Clips.Select( clip => clip.Clip ).ToArray() );
		CollectionAssert.AreEqual( new[] { 0x20000, 0x20000, 0x20000, 0x20000, 0x40000, 0x90000, 0x110000 }, row.Clips.Select( clip => clip.Flags ).ToArray() );
		Assert.AreEqual( (8798, 7798, false), (row.Milliseconds, row.CueAt, row.LeadIn) );
	}

	/// <summary>Row 1, the ticket alone: the same clips with no cue.</summary>
	[TestMethod]
	public void RowOneHasNoCue()
	{
		var row = AdvisorGestures.Read( 1, Length )!.Value;

		Assert.AreEqual( (7, 8798, (int?)null, false), (row.Clips.Count, row.Milliseconds, row.CueAt, row.LeadIn) );
	}

	/// <summary>
	/// A row opening on clip 14 keeps its lead-in (row 5: 14, 5, 11, 15); an empty row too, and adds up to nought, so the
	/// random chain is played; the lobby tour's row 12 is a cue alone; a row past the table is none.
	/// </summary>
	[TestMethod]
	public void OnlyARowOpeningOnClipFourteenKeepsItsLeadIn()
	{
		var five = AdvisorGestures.Read( 5, Length )!.Value;
		Assert.AreEqual( (true, 0 + 5000 + 3333 + 666), (five.LeadIn, five.Milliseconds) );

		var two = AdvisorGestures.Read( 2, Length )!.Value;
		Assert.AreEqual( (true, 0, 0), (two.LeadIn, two.Milliseconds, two.Clips.Count) );

		var twelve = AdvisorGestures.Read( 12, Length )!.Value;
		Assert.AreEqual( (true, 0, (int?)17000), (twelve.LeadIn, twelve.Milliseconds, twelve.CueAt) );

		var zero = AdvisorGestures.Read( 0, Length )!.Value;
		Assert.AreEqual( (false, 28666), (zero.LeadIn, zero.Milliseconds), "row 0 opens on clip 16" );

		Assert.IsNull( AdvisorGestures.Read( 14, Length ) );
		Assert.IsNull( AdvisorGestures.Read( -1, Length ) );
	}

	/// <summary>
	/// A clip lasts its declared span (FUN_00474070): Lost Kingdom's clip 20 declares 60 frames and keys 89, so it lasts
	/// 2000 ms, the original's figure, where the keys' span would say 2966.
	/// </summary>
	[TestMethod]
	public void AClipLastsItsDeclaredSpan()
	{
		var data = GameData.Required();
		int Of( int clip ) => AdvisorModel.MillisecondsOf( new AnimationFile( new MemoryStream( data.ReadAllBytes( $"levels/jungle/advisor/Advisor_JUNm{clip}.MD2" ) ) ) );

		foreach ( var clip in new[] { 15, 16, 17, 18, 19, 20, 1, 6, 11 } )
			Assert.AreEqual( Jungle[clip], Of( clip ), $"clip {clip}" );

		var twenty = new AnimationFile( new MemoryStream( data.ReadAllBytes( "levels/jungle/advisor/Advisor_JUNm20.MD2" ) ) );
		Assert.AreEqual( (60, 89), (twenty.DeclaredLastFrame, twenty.LastFrame) );
	}

	/// <summary>
	/// A clip of 0x10000 shows no add-on (FUN_00598ca0): the theme's costume hides its sub-offs and shows no helmet, a hat
	/// still hides the antennae and is not itself shown.
	/// </summary>
	[TestMethod]
	public void AFaceAwayClipShowsNoAddOn()
	{
		var jungle = new[] { (4, 19), (0, 20), (0, 0), (0, 0) };

		CollectionAssert.AreEqual( new[] { (19, false), (20, false) }, AdvisorProps.Switched( AdvisorProps.ThemeCostume, jungle, addOnsHidden: true ).ToArray() );
		CollectionAssert.AreEqual( new[] { (4, true), (19, false), (20, false) }, AdvisorProps.Switched( AdvisorProps.ThemeCostume, jungle ).ToArray() );
		CollectionAssert.AreEqual( new[] { (19, false), (20, false) }, AdvisorProps.Switched( 5, null, addOnsHidden: true ).ToArray() );
		CollectionAssert.AreEqual( new[] { (19, true), (20, true) }, AdvisorProps.Switched( 0, null, addOnsHidden: true ).ToArray() );
	}
}
