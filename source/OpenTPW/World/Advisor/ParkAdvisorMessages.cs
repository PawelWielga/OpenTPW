using System.Globalization;

namespace OpenTPW;

/// <summary>
/// The park's posted messages, category 0, selection mode 2: the door's two and a ride about to break down.
/// The eight slots, strict tick threshold, helper recheck and per-message histories follow FUN_0059a550;
/// docs/exe/advisor-park.md, Q90 and "A ride about to break down". Other park messages and the polling
/// analyser remain unbuilt.
/// </summary>
internal sealed class ParkAdvisorMessages
{
	internal const int Opened = 128;
	internal const int Closed = 129;
	/// <summary>Message 0x42, posted by a ride's wear (FUN_004df670, 0x004df8be) with the ride as its subject.</summary>
	internal const int RideWornOut = 66;

	/// <summary>
	/// A row of the metadata table 0x0076e300: the message, its score's key, its first response and that
	/// response's sample, how many lines it rotates (+0x28) and how many copies the slots may hold (+0x20).
	/// </summary>
	internal sealed record Topic( int Id, string ScoreKey, int FirstResponse, int FirstSample, int Lines = 2, int MaxInstances = 1 );
	internal static readonly Topic[] DoorTopics =
	[
		new( Opened, "ParkNowOpen.Score", 308, 342 ),
		new( Closed, "ParkNowClosed.Score", 310, 344 )
	];
	internal static readonly Topic[] ParkTopics =
	[
		.. DoorTopics,
		new( RideWornOut, "RidesBreakdownImminent.Score", 194, 191, Lines: 3, MaxInstances: 255 )
	];

	private sealed class History
	{
		public int LastTick;
		public int Line = -1;
		public bool Said;
	}

	private sealed record Pending( Topic Topic, int Score, int Subject );
	private readonly SettingsFile _settings;
	private readonly Dictionary<int, Topic> _topics;
	private readonly Dictionary<int, History> _histories;
	private readonly Pending?[] _slots = new Pending?[8];
	private long _waitUntil;
	internal int Attempts { get; private set; }
	internal int PendingCount => _slots.Count( slot => slot != null );
	internal int Threshold => Value( "GeneralAdvisor.MinScoreForConsideration" );

	internal ParkAdvisorMessages( SettingsFile settings, IEnumerable<Topic>? topics = null )
	{
		_settings = settings;
		_topics = (topics ?? ParkTopics).ToDictionary( topic => topic.Id );
		_histories = _topics.Keys.ToDictionary( id => id, _ => new History() );
	}

	// Missing or malformed settings are installation errors, not permission to invent scores.
	private int Value( string key ) => int.Parse( _settings[key], CultureInfo.InvariantCulture );

	internal bool PostDoor( bool closed, int gameTick ) => Post( closed ? Closed : Opened, gameTick );

	/// <summary>
	/// FUN_0059a940 after FUN_0059b590: a message through the accept gate into a slot. <paramref name="subject"/>
	/// is the thing the record's first word names, nought for none; nothing here reads it but the log.
	/// </summary>
	internal bool Post( int id, int gameTick, int subject = 0 )
	{
		var topic = _topics[id];
		var score = Value( topic.ScoreKey );
		var history = _histories[id];
		var reason = "queued";
		if ( (uint)history.LastTick >> 2 != 0
			&& unchecked(((uint)gameTick >> 2) - ((uint)history.LastTick >> 2))
				< (uint)Value( "MessageGroups[0].MinTimeSameMessage" ) )
			reason = "cooldown";
		else if ( history.Said && Value( "MessageGroups[0].SayOnlyOnce" ) != 0 )
			reason = "said-once";
		else if ( _slots.Count( slot => slot?.Topic.Id == id ) >= topic.MaxInstances )
			reason = "duplicate";
		else
		{
			var index = Array.FindIndex( _slots, slot => slot == null );
			if ( index < 0 )
			{
				index = 0;
				for ( var i = 1; i < _slots.Length; ++i )
					if ( _slots[i]!.Score < _slots[index]!.Score ) index = i;
				if ( score <= _slots[index]!.Score ) reason = "priority";
			}
			if ( reason == "queued" ) _slots[index] = new Pending( topic, score, subject );
		}
		Log.Info( $"Park advisor: post message={id} subject={subject} score={score} threshold={Threshold} tick={gameTick} result={reason} pending={PendingCount}" );
		return reason == "queued";
	}

	/// <summary>
	/// One world sweep. Playback returns the response duration (zero when the speaker cannot start).
	/// The helper still succeeds after a zero playback return; its score recheck alone can reject here.
	/// </summary>
	internal void Tick( int gameTick, long milliseconds, Func<int, int, int> play )
	{
		if ( milliseconds < _waitUntil ) return;
		var index = -1;
		for ( var i = 0; i < _slots.Length; ++i )
			if ( _slots[i] is { } slot && (index < 0 || slot.Score > _slots[index]!.Score) ) index = i;
		if ( index < 0 || _slots[index]!.Score <= Threshold ) return;

		var pending = _slots[index]!;
		_slots[index] = null;
		var score = Value( pending.Topic.ScoreKey );
		if ( score < Threshold ) return;

		var history = _histories[pending.Topic.Id];
		var line = (history.Line + 1) % pending.Topic.Lines;
		var response = pending.Topic.FirstResponse + line;
		var sample = pending.Topic.FirstSample + line;
		var duration = play( response, sample );
		_waitUntil = milliseconds + duration + 1000;
		history.LastTick = gameTick;
		history.Line = line;
		history.Said = true;
		++Attempts;
		// The helper then sends an event of type 0x18 carrying the response's group and the record's subject
		// (FUN_0059b620, FUN_0040f740, FUN_0040fb10); nothing here hears it.
		Unimplemented.Report( "ADVISOR_MESSAGE_SAID_EVENT" );
		Log.Info( $"Park advisor: attempt={Attempts} message={pending.Topic.Id} subject={pending.Subject} score={score} threshold={Threshold} response={response} sample={sample} line={line} tick={gameTick} duration={duration} waitUntil={_waitUntil}" );
	}

	internal string Census() => $"park advisor: pending={PendingCount} attempts={Attempts} threshold={Threshold} "
		+ string.Concat( _slots.Where( slot => slot != null ).Select( slot => $"slot={slot!.Topic.Id}/subject={slot.Subject} " ) )
		+ string.Join( " ", _topics.Values.Select( topic =>
			$"message={topic.Id}/score={Value( topic.ScoreKey )}/line={_histories[topic.Id].Line}/lastTick={_histories[topic.Id].LastTick}" ) );
}
