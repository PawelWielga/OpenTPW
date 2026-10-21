using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// When a park line's voice starts, its cue falls and its clips end (<see cref="Advisor.LineTimes"/>; Advisor_SayResponse
/// 0x00599050, Advisor_PlayGestureRow 0x00598bf0; <c>docs/exe/scenes.md</c>, "A row and the voice's start"), which clip of
/// the line is playing (<see cref="ClipSequence.IndexAt"/>) and how long the response takes in all. No game is needed.
/// </summary>
[TestClass]
public class AdvisorLineTests
{
	/// <summary>Lost Kingdom's clip lengths as the original's table holds them, as <see cref="AdvisorGestureTests"/> has them.</summary>
	private static readonly Dictionary<int, int> Jungle = new()
	{
		[14] = 0, [15] = 666, [16] = 28666, [17] = 633, [18] = 2966, [19] = 1300, [20] = 2000, [11] = 3333, [5] = 5000
	};

	private static int Length( int clip ) => Jungle.GetValueOrDefault( clip );

	/// <summary>
	/// Row 13 has no lead-in, so the voice starts at once and the clips and the key's cue count from the clock less 200:
	/// given at 10 s, the cue falls at 10 - 0.2 + 7.798 and the clips end at 10 - 0.2 + 8.798. Counting from the clock itself
	/// would put the cue 0.2 s late.
	/// </summary>
	[TestMethod]
	public void ARowWithNoLeadInCountsFromTheClockLessTwoHundred()
	{
		var row = AdvisorGestures.Read( 13, Length )!.Value;
		var (speakAt, cueAt, endAt) = Advisor.LineTimes( 10f, row.LeadIn, 0f, row.CueAt, row.Milliseconds );

		Assert.AreEqual( 10f, speakAt, 1e-4f );
		Assert.AreEqual( 17.598f, cueAt, 1e-3f );
		Assert.AreEqual( 18.598f, endAt, 1e-3f );
	}

	/// <summary>
	/// A line with a lead-in (the random chain, or a row opening on clip 14) speaks 0.8 s on, and its clips count from there:
	/// 5000 ms of clips given at 10 s end at 15.8, not at 15.
	/// </summary>
	[TestMethod]
	public void ALineWithALeadInCountsFromTheLeadInsEnd()
	{
		var (speakAt, cueAt, endAt) = Advisor.LineTimes( 10f, true, 0f, null, 5000 );

		Assert.AreEqual( 10.8f, speakAt, 1e-4f );
		Assert.AreEqual( 10f, cueAt, 1e-4f );
		Assert.AreEqual( 15.8f, endAt, 1e-3f );
	}

	/// <summary>A cue the message gave counts from the clock itself, and a row's own cue replaces it.</summary>
	[TestMethod]
	public void AMessagesCueCountsFromTheClockAndARowsReplacesIt()
	{
		Assert.AreEqual( 12.5f, Advisor.LineTimes( 10f, true, 2.5f, null, 5000 ).CueAt, 1e-4f );
		Assert.AreEqual( 12.5f, Advisor.LineTimes( 10f, false, 2.5f, null, 5000 ).CueAt, 1e-4f );
		Assert.AreEqual( 16.8f, Advisor.LineTimes( 10f, true, 2.5f, 6000, 7000 ).CueAt, 1e-3f );
	}

	/// <summary>
	/// The clip playing a given time into the line: a clip starts the millisecond the one before it ends, so at exactly a
	/// clip's start it is that clip, before the run it is the first, and past the end it is the last. Row 13 on Lost Kingdom's
	/// lengths: 17 four times from 0, 633, 1266 and 1899, 18 at 2532, 19 at 5498, 20 at 6798.
	/// </summary>
	[TestMethod]
	public void TheClipPlayingIsTheOneStartedAtOrBefore()
	{
		var row = AdvisorGestures.Read( 13, Length )!.Value;
		var sequence = new ClipSequence( row.Clips.Select( clip => clip.Clip ).ToList(), Length );

		Assert.AreEqual( 0, sequence.IndexAt( 0.632f ) );
		Assert.AreEqual( 1, sequence.IndexAt( 0.633f ) );
		Assert.AreEqual( 3, sequence.IndexAt( 2.0f ) );
		Assert.AreEqual( 4, sequence.IndexAt( 2.532f ) );
		Assert.AreEqual( 5, sequence.IndexAt( 5.498f ) );
		Assert.AreEqual( 6, sequence.IndexAt( 6.8f ) );
		Assert.AreEqual( 6, sequence.IndexAt( 60f ) );
		Assert.AreEqual( 0, sequence.IndexAt( -1f ) );
		Assert.AreEqual( -1, ClipSequence.Empty.IndexAt( 1f ) );
	}

	/// <summary>A response takes its clips, clip 15 and a second more (0x005994ac).</summary>
	[TestMethod]
	public void AResponseTakesItsClipsClipFifteenAndASecond()
		=> Assert.AreEqual( 8798 + 666 + 1000, Advisor.ParkResponseDuration( 8798, 666 ) );
}
