using System.Globalization;

namespace OpenTPW;

/// <summary>
/// The park's posted messages, selection mode 2: the door's two, a ride about to break down, a ride broken down
/// with no mechanic to come and a mechanic on his way to one, category 0, and the tutorial line of an opened tag
/// message, category 1, which is withdrawn as its bar leaves. The eight slots, strict tick threshold, helper
/// recheck and per-message histories follow FUN_0059a550, the gate FUN_0059abc0 and the withdrawal FUN_0059aa70;
/// docs/exe/advisor-park.md, Q90, "A ride about to break down", "A ride broken down and no mechanic to come",
/// "A mechanic on his way" and "The line of an opened tag message".
/// Other park messages and the polling analyser remain unbuilt.
/// </summary>
internal sealed class ParkAdvisorMessages
{
	internal const int Opened = 128;
	internal const int Closed = 129;
	/// <summary>Message 0x42, posted by a ride's wear (FUN_004df670, 0x004df8be) with the ride as its subject.</summary>
	internal const int RideWornOut = 66;
	/// <summary>Message 0x44, posted by the mechanics' base as a ride breaks down in a park with no mechanic (FUN_004dadc0, 0x004dae62).</summary>
	internal const int NoMechanics = 68;
	/// <summary>Message 0x45, posted by the mechanics' base as a ride breaks down with every mechanic busy (FUN_004dadc0, 0x004daff0).</summary>
	internal const int MechanicsBusy = 69;
	/// <summary>Message 0x46, posted by the mechanic's decide (FUN_004da5b0, 0x004da6bf) with the ride he sets off for as its subject.</summary>
	internal const int MechanicOnHisWay = 70;
	/// <summary>Message 0x11c, posted as a tag's message is put on the bar (FUN_0048d9c0) and withdrawn as the bar leaves (FUN_0048e2f0, 0x1001d8).</summary>
	internal const int TagMessage = 284;

	/// <summary>The tutorial messages' category (a row's +0x08): MessageGroups[1], refused while the Tutorial option is off (FUN_0059c610).</summary>
	internal const int Tutorial = 1;

	/// <summary>
	/// A row of the metadata table 0x0076e300: the message, its score's key, its first response and that
	/// response's sample, how many lines it rotates (+0x28) and how many copies the slots may hold (+0x20).
	/// <paramref name="GestureRowLine"/> is the one line whose response names a row of the gesture table
	/// (the response table's +0x0c), -1 where every line's is the random chain. <paramref name="Prop"/> is what
	/// he wears for every line of it (the response table's +0x14; each built row's +0x18 is nought).
	/// <paramref name="Group"/> is the group of every line of it (the response table's +0x1c), the id of the tag
	/// a said line leaves (<see cref="ParkTags"/>). <paramref name="Category"/> is the row's +0x08, the
	/// MessageGroups entry of Advisor.sam the gate reads for it.
	/// </summary>
	internal sealed record Topic( int Id, string ScoreKey, int FirstResponse, int FirstSample, int Lines = 2, int MaxInstances = 1,
		int GestureRowLine = -1, int Prop = 0, int Group = ParkTags.NoTag, int Category = 0 );

	/// <summary>The ticket hat, the door's lines' prop.</summary>
	internal const int TicketHat = 9;
	/// <summary>The hard hat, the prop of every line about a ride breaking down.</summary>
	internal const int HardHat = 5;

	internal static readonly Topic[] DoorTopics =
	[
		new( Opened, "ParkNowOpen.Score", 308, 342, Prop: TicketHat, Group: 150 ),
		new( Closed, "ParkNowClosed.Score", 310, 344, Prop: TicketHat, Group: 151 )
	];
	internal static readonly Topic[] ParkTopics =
	[
		.. DoorTopics,
		new( RideWornOut, "RidesBreakdownImminent.Score", 194, 191, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 68 ),
		new( NoMechanics, "RidesBreakdownNoMechanics.Score", 198, 196, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 70 ),
		new( MechanicsBusy, "RidesBreakdownBusyMechanics.Score", 201, 199, Lines: 3, MaxInstances: 255, Prop: HardHat, Group: 71 ),
		new( MechanicOnHisWay, "RidesBreakdownMechanicOnIt.Score", 204, 202, Lines: 3, MaxInstances: 255, GestureRowLine: 2, Prop: HardHat, Group: 72 ),
		new( TagMessage, "HelpMessage.Score", 544, 561, Lines: 1, Category: Tutorial )
	];

	/// <summary>The two props <paramref name="response"/>'s row names, nought and nought for one of no built topic.</summary>
	internal static (int First, int Second) PropsOf( int response )
	{
		foreach ( var topic in ParkTopics )
		{
			if ( response >= topic.FirstResponse && response < topic.FirstResponse + topic.Lines )
				return (topic.Prop, 0);
		}

		return (0, 0);
	}

	private sealed class History
	{
		public int LastTick;
		public int Line = -1;
		public bool Said;
		public int Slaps;
	}

	/// <summary>A taken slot. <paramref name="Secondary"/> is the record's second word where a park file gave it, null for a post made here.</summary>
	private sealed record Pending( Topic Topic, int Score, int Subject, int? Secondary = null );
	private readonly SettingsFile _settings;
	private readonly Dictionary<int, Topic> _topics;
	private readonly Dictionary<int, History> _histories;
	private readonly Pending?[] _slots = new Pending?[ParkWorld.AdvisorSlots];
	// The slots a park file's message this game cannot say stands in: taken, and never said or written over.
	private readonly bool[] _carried = new bool[ParkWorld.AdvisorSlots];
	private readonly Func<bool> _tutorial;
	// The last action (the advisor's +0xd8, +0xdc, +0x16ec): the moment the last line said began, how long he
	// waits from it, and the message it was. The moment is null until a line is said or a file's is taken.
	private long? _started;
	private int _duration;
	private int _playing;
	private long WaitUntil => (_started ?? 0) + _duration;
	internal int Attempts { get; private set; }
	internal int PendingCount => _slots.Count( slot => slot != null ) + _carried.Count( carried => carried );
	internal int Threshold => Value( "GeneralAdvisor.MinScoreForConsideration" );

	/// <param name="tutorial">Whether the Tutorial option is on (0x0078d90d), the player's own where none is given.</param>
	internal ParkAdvisorMessages( SettingsFile settings, IEnumerable<Topic>? topics = null, Func<bool>? tutorial = null )
	{
		_settings = settings;
		_tutorial = tutorial ?? (() => GameOptions.Current.Tutorial);
		_topics = (topics ?? ParkTopics).ToDictionary( topic => topic.Id );
		_histories = _topics.Keys.ToDictionary( id => id, _ => new History() );
	}

	/// <summary>
	/// Takes each built message's record from a park file's advisor (<paramref name="saved"/> answers null for
	/// one the file does not hold, which stays as it is), as the original's advisor is read back whole with its
	/// thing. Says how many records were taken that the constructor would not have left.
	/// </summary>
	internal int Restore( Func<int, ParkWorld.AdvisorHistory?> saved )
	{
		var taken = 0;
		foreach ( var (id, history) in _histories )
		{
			if ( saved( id ) is not { } record ) continue;
			history.LastTick = record.LastTick;
			history.Line = record.Line;
			history.Said = record.Said;
			history.Slaps = record.Slaps;
			if ( record == ParkWorld.AdvisorHistory.Fresh ) continue;
			++taken;
			Log.Info( $"Park advisor: restored message={id} line={record.Line} lastTick={record.LastTick} said={(record.Said ? 1 : 0)} slaps={record.Slaps}" );
		}

		return taken;
	}

	/// <summary>Each built message's record as a park file's advisor holds it (<see cref="ParkWorld.PutAdvisorHistories"/>).</summary>
	internal IReadOnlyDictionary<int, ParkWorld.AdvisorHistory> Written()
		=> _histories.ToDictionary( pair => pair.Key,
			pair => new ParkWorld.AdvisorHistory( pair.Value.LastTick, pair.Value.Line, pair.Value.Said, pair.Value.Slaps ) );

	/// <summary>
	/// Takes a park file's waiting messages into the slots they stand in (<paramref name="saved"/> answers null
	/// for a file with no advisor), as the original's advisor is read back whole with its thing: a built message
	/// with its file's score, subject and second word. A taken slot this game cannot say - a message of no built
	/// topic, a line asked for by number, a forced post or a score the poster gave - is counted and stays taken,
	/// said by nothing and written back as the file's. Says how many were taken to be said.
	/// </summary>
	internal int RestoreSlots( Func<int, ParkWorld.AdvisorSlot?> saved )
	{
		var taken = 0;
		for ( var index = 0; index < _slots.Length; ++index )
		{
			if ( saved( index ) is not { Valid: true } slot ) continue;
			if ( !_topics.TryGetValue( slot.Message, out var topic ) || slot.Line >= 0 || slot.OverrideOnlyOnce || slot.ScoreOverridden )
			{
				Unimplemented.Report( "PARK_ADVISOR_SAVED_SLOT_NOT_BUILT" );
				_carried[index] = true;
				Log.Info( $"Park advisor: carried slot={index} message={slot.Message} subject={slot.Thing} score={slot.Score}" );
				continue;
			}

			_slots[index] = new Pending( topic, slot.Score, slot.Thing, slot.Secondary );
			++taken;
			Log.Info( $"Park advisor: restored slot={index} message={slot.Message} subject={slot.Thing} secondary={slot.Secondary} score={slot.Score}" );
		}

		return taken;
	}

	/// <summary>
	/// The eight slots as a park file's advisor holds them (<see cref="ParkWorld.PutAdvisorSlots"/>): a waiting
	/// message as FUN_0059b590 builds an unforced post with no score given, its second word the file's or, for
	/// one posted here about a thing, that thing's item by <paramref name="itemOf"/>; an empty slot not taken;
	/// null for a slot a file's unbuilt message stands in.
	/// </summary>
	internal IReadOnlyList<ParkWorld.AdvisorSlot?> WrittenSlots( Func<int, int> itemOf )
	{
		var written = new ParkWorld.AdvisorSlot?[_slots.Length];
		for ( var index = 0; index < _slots.Length; ++index )
		{
			if ( _carried[index] ) continue;
			written[index] = _slots[index] is { } slot
				? new ParkWorld.AdvisorSlot( slot.Subject, slot.Secondary ?? (slot.Subject != 0 ? itemOf( slot.Subject ) : 0),
					slot.Topic.Id, slot.Score, true, -1, false, false )
				: default( ParkWorld.AdvisorSlot );
		}

		return written;
	}

	/// <summary>
	/// Takes a park file's last action, as the original's advisor is read back whole and its second stopwatch
	/// made to read the file's again (FUN_00402e80): the wait a file was saved inside goes on for what was left
	/// of it, and a withdrawal inside it still finds the message he was saying. <paramref name="stopwatch"/> is
	/// the file's second stopwatch and <paramref name="milliseconds"/> the load's moment here. Says how many
	/// milliseconds of the wait are left, nought or less for one that is over.
	/// </summary>
	internal long RestoreAction( ParkWorld.AdvisorAction saved, uint stopwatch, long milliseconds )
	{
		_started = milliseconds + unchecked((int)(saved.Started - stopwatch));
		_duration = unchecked((int)saved.Duration);
		_playing = saved.Playing;
		var left = WaitUntil - milliseconds;
		Log.Info( $"Park advisor: restored action started={saved.Started} duration={saved.Duration} playing={saved.Playing} stopwatch={stopwatch} at={milliseconds} left={left}" );
		return left;
	}

	/// <summary>
	/// The last action as a park file's advisor holds it (<see cref="ParkWorld.PutAdvisorAction"/>), its start a
	/// reading of the second stopwatch, which reads <paramref name="stopwatch"/> at <paramref name="milliseconds"/>
	/// here. Before any line is said and with no file's taken it is the constructor's, noughts (FUN_00599e70).
	/// </summary>
	internal ParkWorld.AdvisorAction WrittenAction( uint stopwatch, long milliseconds )
		=> _started is { } started
			? new ParkWorld.AdvisorAction( unchecked(stopwatch - (uint)(int)(milliseconds - started)), unchecked((uint)_duration), _playing )
			: default;

	// Missing or malformed settings are installation errors, not permission to invent scores.
	private int Value( string key ) => int.Parse( _settings[key], CultureInfo.InvariantCulture );

	internal bool PostDoor( bool closed, int gameTick ) => Post( closed ? Closed : Opened, gameTick );

	/// <summary>
	/// FUN_0059a940 after FUN_0059b590: a message through the accept gate into a slot. <paramref name="subject"/>
	/// is the thing the record's first word names, nought for none; nothing here reads it but the log. The gate
	/// (FUN_0059abc0, no post here forced) refuses in this order: a tutorial message with the Tutorial option off;
	/// one said before and inside its category's MinTimeSameMessage; one said, of a category said only once; one
	/// cut off as often as its category's DiscardAfterSlaps, where that is not nought; one with all its copies in
	/// the slots.
	/// </summary>
	internal bool Post( int id, int gameTick, int subject = 0 )
	{
		var topic = _topics[id];
		var score = Value( topic.ScoreKey );
		var history = _histories[id];
		var group = $"MessageGroups[{topic.Category}]";
		var slaps = Value( $"{group}.DiscardAfterSlaps" );
		var reason = "queued";
		if ( topic.Category == Tutorial && !_tutorial() )
			reason = "tutorial-off";
		else if ( (uint)history.LastTick >> 2 != 0
			&& unchecked(((uint)gameTick >> 2) - ((uint)history.LastTick >> 2))
				< (uint)Value( $"{group}.MinTimeSameMessage" ) )
			reason = "cooldown";
		else if ( history.Said && Value( $"{group}.SayOnlyOnce" ) != 0 )
			reason = "said-once";
		else if ( slaps != 0 && slaps <= history.Slaps )
			reason = "slapped";
		else if ( _slots.Count( slot => slot?.Topic.Id == id ) >= topic.MaxInstances )
			reason = "duplicate";
		else
		{
			var index = -1;
			for ( var i = 0; i < _slots.Length && index < 0; ++i )
				if ( _slots[i] == null && !_carried[i] ) index = i;
			if ( index < 0 )
			{
				for ( var i = 0; i < _slots.Length; ++i )
					if ( _slots[i] is { } held && (index < 0 || held.Score < _slots[index]!.Score) ) index = i;
				if ( index < 0 || score <= _slots[index]!.Score ) reason = "priority";
			}
			if ( reason == "queued" ) _slots[index] = new Pending( topic, score, subject );
		}
		Log.Info( $"Park advisor: post message={id} subject={subject} score={score} threshold={Threshold} tick={gameTick} result={reason} pending={PendingCount}" );
		return reason == "queued";
	}

	/// <summary>
	/// FUN_0059aa70: every slot holding <paramref name="id"/> is emptied, and where he is saying it (inside the
	/// last line's time and its second) <paramref name="stop"/> cuts him off (Advisor_StopSpeaking), the message
	/// is no longer said, it has been cut off once more and the wait is over. Says whether it did either.
	/// </summary>
	/// <remarks>
	/// The kept message the original also forgets here (the advisor's +0x16d0, a tutorial one of this id, raised
	/// again by FUN_0059ab50) is kept by nothing in this game.
	/// </remarks>
	internal bool Withdraw( int id, long milliseconds, Action stop )
	{
		var cleared = 0;
		for ( var i = 0; i < _slots.Length; ++i )
		{
			if ( _slots[i]?.Topic.Id != id ) continue;
			_slots[i] = null;
			++cleared;
		}

		var history = _histories[id];
		var cut = milliseconds < WaitUntil && _playing == id;
		if ( cut )
		{
			stop();
			history.Said = false;
			++history.Slaps;
			_duration = 0;
		}

		Log.Info( $"Park advisor: withdraw message={id} cleared={cleared} cut={(cut ? 1 : 0)} slaps={history.Slaps} pending={PendingCount}" );
		return cleared > 0 || cut;
	}

	/// <summary>
	/// One world sweep. Playback returns the response duration (zero when the speaker cannot start).
	/// The helper still succeeds after a zero playback return; its score recheck alone can reject here.
	/// <paramref name="said"/> hears the event a said line sends, with the response's group and the subject.
	/// </summary>
	internal void Tick( int gameTick, long milliseconds, Func<int, int, int> play, Action<int, int>? said = null )
	{
		if ( milliseconds < WaitUntil ) return;
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
		// A response with a gesture of its own plays that row of the gesture table (FUN_00598bf0); the speaker
		// here builds the random chain for every line.
		if ( line == pending.Topic.GestureRowLine )
			Unimplemented.Report( "ADVISOR_RESPONSE_GESTURE_ROW" );
		var duration = play( response, sample );
		_started = milliseconds;
		_duration = duration + 1000;
		history.LastTick = gameTick;
		history.Line = line;
		history.Said = true;
		_playing = pending.Topic.Id;
		++Attempts;
		// The helper then sends an event of type 0x18 carrying the response's group and the record's subject
		// (FUN_0059b620, FUN_0040f740, FUN_0040fb10); its one hearer is the tag system (FUN_00509bf0).
		said?.Invoke( pending.Topic.Group, pending.Subject );
		Log.Info( $"Park advisor: attempt={Attempts} message={pending.Topic.Id} subject={pending.Subject} score={score} threshold={Threshold} response={response} sample={sample} line={line} tick={gameTick} duration={duration} waitUntil={WaitUntil}" );
	}

	internal string Census() => $"park advisor: pending={PendingCount} attempts={Attempts} threshold={Threshold} "
		+ string.Concat( _slots.Where( slot => slot != null ).Select( slot => $"slot={slot!.Topic.Id}/subject={slot.Subject} " ) )
		+ string.Join( " ", _topics.Values.Select( topic =>
			$"message={topic.Id}/score={Value( topic.ScoreKey )}/line={_histories[topic.Id].Line}/lastTick={_histories[topic.Id].LastTick}/said={(_histories[topic.Id].Said ? 1 : 0)}/slaps={_histories[topic.Id].Slaps}" ) )
		+ $" action={(_started is { } started ? $"{started}+{_duration}" : "none")}/playing={_playing}";
}
