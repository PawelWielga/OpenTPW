namespace OpenTPW;

/// <summary>
/// The tags a said line leaves on the gadget's aerial: the tag system's hearing of the event a line sends
/// (FUN_00509bf0, type 0x18) and the messages control's list (FUN_0048eb20). A line's response group names a row
/// of the tag table; the row is the tag's picture, the tag it takes the place of and how long it stands.
/// docs/exe/advisor-park.md, "The tag a line leaves on the aerial".
/// <para>
/// The times are the interface's timers', real milliseconds, handed in by the caller.
/// </para>
/// <para>
/// <b>A park file holds the tags standing</b> (<see cref="Written"/>, <see cref="Restore"/>;
/// docs/exe/saves.md, "OpenTPW's writer, the aerial's tags"): each one's id, text, whole life, picture, buttons,
/// thing and item, and a load puts each back with its whole life before it.
/// </para>
/// <para>
/// <b>A tag's left click puts its message on the bar</b> (<see cref="Open"/>, FUN_0048d9c0), which stops the
/// tag's two timers: a tag once opened stands until it is taken off. docs/exe/hud.md, "The message bar".
/// </para>
/// </summary>
internal sealed class ParkTags
{
	/// <summary>
	/// A row of the tag table 0x0075eca8: its id, the id of the tag it takes the place of (+0x04), the part of
	/// msgtag.md2 it shows (+0x08), its string (+0x0c), the message bar's two buttons (+0x10, +0x14), its life in
	/// milliseconds (+0x18) and how its text is made (+0x1c).
	/// </summary>
	internal sealed record Row( int Id, int Key, int Picture, int String, int Button1, int Button2, int Life, int Kind );

	/// <summary>The id no row holds, the key of a tag that takes no other's place (FUN_0050af40).</summary>
	internal const int NoTag = 0x17f;

	/// <summary>The rows the built topics' response groups name, of the table's 382.</summary>
	internal static readonly Row[] Rows =
	[
		new( 3, 4, 2, 7, 24, 0, 30000, 0 ),
		new( 4, 3, 2, 8, 24, 0, 30000, 0 ),
		new( 7, NoTag, 2, 11, 14, 0, 30000, 0 ),
		new( 8, NoTag, 2, 12, 14, 0, 30000, 0 ),
		new( 9, NoTag, 2, 13, 14, 0, 30000, 0 ),
		new( 12, NoTag, 2, 16, 13, 0, 30000, 0 ),
		new( 30, NoTag, 2, 33, 21, 0, 30000, 0 ),
		new( 31, NoTag, 2, 34, 21, 0, 30000, 0 ),
		new( 32, NoTag, 2, 35, 21, 0, 30000, 0 ),
		new( 33, NoTag, 2, 36, 21, 0, 30000, 0 ),
		new( 34, NoTag, 2, 37, 21, 0, 30000, 0 ),
		new( 62, NoTag, 2, 65, 9, 0, 30000, 0 ),
		new( 64, NoTag, 2, 67, 9, 0, 30000, 0 ),
		new( 73, NoTag, 2, 75, 10, 0, 30000, 0 ),
		new( 75, NoTag, 2, 77, 10, 0, 30000, 0 ),
		new( 76, NoTag, 2, 78, 10, 0, 30000, 0 ),
		new( 77, NoTag, 2, 79, 11, 0, 30000, 0 ),
		new( 78, NoTag, 2, 80, 19, 0, 30000, 0 ),
		new( 79, NoTag, 2, 81, 11, 0, 30000, 0 ),
		new( 80, NoTag, 2, 82, 11, 0, 30000, 0 ),
		new( 81, NoTag, 2, 83, 12, 0, 30000, 0 ),
		new( 82, NoTag, 2, 84, 0, 0, 30000, 0 ),
		new( 83, NoTag, 2, 85, 12, 0, 30000, 0 ),
		new( 84, NoTag, 2, 86, 12, 0, 30000, 0 ),
		new( 68, NoTag, 1, 71, 3, 0, 30000, 2 ),
		new( 70, 68, 1, 72, 3, 0, 30000, 2 ),
		new( 71, 68, 1, 73, 3, 0, 30000, 2 ),
		new( 72, 68, 1, 74, 3, 0, 30000, 2 ),
		new( 135, NoTag, 0, 134, 3, 0, 30000, 0 ),
		new( 150, 151, 1, 149, 0, 0, 30000, 0 ),
		new( 151, 150, 1, 150, 0, 0, 30000, 0 )
	];

	/// <summary>How many the list holds, DAT_00750610.</summary>
	internal const int Most = 10;
	/// <summary>How long before its end a tag starts its particle (timer 0x80081, FUN_0048eb20).</summary>
	internal const int WarningBeforeTheEnd = 5000;
	/// <summary>How long after it is hidden a tag is taken off the mast (LAB_004a11e0, 0x100002).</summary>
	internal const int OffAfterHidden = 500;
	/// <summary>How long after the one before it each tag goes when the top is right clicked (LAB_004a11e0, 0x100003).</summary>
	internal const int DeleteAllStep = 100;
	/// <summary>The second button that declines an offer, mb_decline: a tag carrying it keeps its timers when opened (FUN_0048d9c0).</summary>
	internal const int DeclineButton = 0x1b;

	/// <summary>The table the game's running language keeps the tags' strings in (DAT_0078ba90).</summary>
	internal const string StringsPath = "Language/English/TAG_SYSTEM.str";

	internal sealed class Tag
	{
		internal required Row Row { get; init; }
		/// <summary>The thing the line was about, nought for none.</summary>
		internal int Subject { get; init; }
		/// <summary>That thing's item, nought for none.</summary>
		internal int Item { get; init; }
		/// <summary>What the message bar would read, written as the tag is made (FUN_00509e10).</summary>
		internal string Text { get; set; } = string.Empty;
		/// <summary>Which of the list's ten records holds it: the first free as it was made. A file is written in this order.</summary>
		internal int Slot { get; init; }
		internal long Born { get; init; }
		/// <summary>When timer 0x80080 hides it; never for a row with no life.</summary>
		internal long HideAt { get; set; } = long.MaxValue;
		/// <summary>When message 5 takes it off the mast.</summary>
		internal long OffAt { get; set; } = long.MaxValue;
		/// <summary>Hidden and still holding its place on the mast.</summary>
		internal bool Hidden { get; set; }
		internal bool Warned { get; set; }
		/// <summary>The control's +0x134: its two timers run. Set as a tag with a life is made, cleared as it is opened.</summary>
		internal bool TimersRun { get; set; }
		/// <summary>When its message was last put on the bar.</summary>
		internal long OpenedAt { get; set; }
	}

	private readonly List<Tag> _tags = [];
	private readonly Row[] _rows;

	internal ParkTags( IEnumerable<Row>? rows = null ) => _rows = (rows ?? Rows).ToArray();

	/// <summary>The tags' strings, each as its parts (<see cref="StringsPath"/>); with none a tag's text is empty.</summary>
	internal StringPart[][]? Strings { get; set; }

	/// <summary>The tags on the mast, the oldest first: the oldest stands lowest.</summary>
	internal IReadOnlyList<Tag> Standing => _tags;
	/// <summary>One up at every change of the list or of a tag's showing.</summary>
	internal int Changes { get; private set; }
	/// <summary>How many tags the list has taken, for the new tag's sound.</summary>
	internal int Made { get; private set; }
	/// <summary>The tag whose message the bar shows (DAT_007ca090), or none.</summary>
	internal Tag? OnShow { get; private set; }

	/// <summary>
	/// A line of response group <paramref name="group"/> about <paramref name="subject"/>, a thing of item
	/// <paramref name="item"/> called <paramref name="itemName"/>, was said. A tag of the row's key about the same
	/// thing and item is taken off at once, or with a row that is its own key takes the new text and is left as
	/// the line's tag; otherwise the list takes a new tag while it holds fewer than ten.
	/// </summary>
	internal Tag? Said( int group, int subject, long now, int item = 0, string? itemName = null )
	{
		var row = Array.Find( _rows, candidate => candidate.Id == group );
		if ( row == null )
		{
			// A group with a row the table holds and this list does not.
			if ( group != NoTag && group != NoTag - 1 )
				Unimplemented.Report( "TAG_ROW_NOT_LISTED" );
			return null;
		}

		return Take( row, subject, item, TextOf( row, item, itemName ), now );
	}

	/// <summary>
	/// The messages control's list taking a tag (FUN_0048eb20): the standing tags of <paramref name="row"/>'s key
	/// about the same thing and item, then the new tag in the first free record.
	/// </summary>
	private Tag? Take( Row row, int subject, int item, string text, long now )
	{
		var fresh = true;
		foreach ( var tag in _tags.ToArray() )
		{
			if ( tag.Row.Id != row.Key || tag.Subject != subject || tag.Item != item ) continue;
			if ( row.Id == row.Key )
			{
				tag.Text = text;
				fresh = false;
			}
			else
				TakeOff( tag );
		}

		if ( !fresh || _tags.Count >= Most ) return null;

		var slot = Enumerable.Range( 0, Most ).First( free => _tags.All( tag => tag.Slot != free ) );
		var made = new Tag { Row = row, Subject = subject, Item = item, Text = text, Slot = slot, Born = now };
		if ( row.Life > 0 )
		{
			made.HideAt = now + row.Life;
			made.TimersRun = true;
		}

		_tags.Add( made );
		++Changes;
		++Made;
		Log.Info( $"Park tags: tag {row.Id} picture {row.Picture} subject {subject} life {row.Life} standing {_tags.Count}" );
		return made;
	}

	/// <summary>
	/// A tag's text by its row's kind (FUN_00509e10): kind 0 the row's string alone, kind 2 the string with the
	/// item's name as its parameter 0. Any other kind, and a kind 2 about no item (which the original logs and
	/// leaves its buffer as it stood), is counted and written empty.
	/// </summary>
	internal string TextOf( Row row, int item, string? itemName )
	{
		if ( Strings is not { } strings || row.String < 0 || row.String >= strings.Length )
			return string.Empty;

		if ( row.Kind != 0 && (row.Kind != 2 || item == 0) )
		{
			Unimplemented.Report( "TAG_TEXT_KIND" );
			return string.Empty;
		}

		var text = new System.Text.StringBuilder();

		foreach ( var part in strings[row.String] )
		{
			if ( part.Text != null )
				text.Append( part.Text );
			else if ( row.Kind == 2 && part.Parameter == 0 )
				text.Append( itemName );
		}

		return text.ToString();
	}

	/// <summary>
	/// The tags as a park file holds them (FUN_0048ee50): every record in use, a hidden tag's too, in the order of
	/// the list's ten records, each with its whole life and not what is left of it.
	/// </summary>
	internal IReadOnlyList<ParkTagRecords.Record> Written()
		=> [.. _tags.OrderBy( tag => tag.Slot ).Select( tag => new ParkTagRecords.Record( tag.Row.Id, tag.Text,
			tag.Row.Life, tag.Row.Picture, tag.Row.Button1, tag.Row.Button2, tag.Subject, tag.Item ) )];

	/// <summary>
	/// A park file's tags put on the aerial (FUN_0048ef70): each handed to the list in the file's order as a new
	/// tag whose key is <see cref="NoTag"/>, so it takes no other's place, with the file's text, picture, buttons,
	/// thing and item, and its whole life from <paramref name="now"/>. The picture and the life are the file's, so
	/// a tag of a row this list does not hold stands as any other.
	/// </summary>
	internal void Restore( IReadOnlyList<ParkTagRecords.Record> saved, long now )
	{
		foreach ( var record in saved )
		{
			var row = new Row( record.Id, NoTag, record.Picture, -1, record.Button1, record.Button2, record.Life, 0 );
			Take( row, record.Thing, record.Item, record.Text, now );
		}

		if ( saved.Count > 0 )
			Log.Info( $"Park tags: {saved.Count} of the file's put back, standing {_tags.Count}" );
	}

	/// <summary>
	/// A tag's message put on the bar (FUN_0048d9c0): its two timers are stopped, unless its second button is
	/// <see cref="DeclineButton"/>, so it no longer hides at its life's end; it is the tag on show until
	/// <see cref="Shut"/> or its going. False, and nothing done, for the tag already on show or one not standing.
	/// </summary>
	internal bool Open( Tag tag, long now )
	{
		if ( tag == OnShow || !_tags.Contains( tag ) ) return false;

		if ( tag.TimersRun && tag.Row.Button2 != DeclineButton )
		{
			tag.TimersRun = false;
			tag.HideAt = long.MaxValue;
		}

		OnShow = tag;
		tag.OpenedAt = now;
		++Changes;
		Log.Info( $"Park tags: tag {tag.Row.Id} subject {tag.Subject} opened, timers {(tag.TimersRun ? 1 : 0)}" );
		return true;
	}

	/// <summary>The bar taken off the arm (the bar's 0x1001d8): no tag is on show.</summary>
	internal void Shut()
	{
		if ( OnShow == null ) return;
		OnShow = null;
		++Changes;
	}

	/// <summary>The timers' turn: a tag's particle five seconds before its end, its hiding at its end, and its going.</summary>
	internal void Tick( long now )
	{
		foreach ( var tag in _tags.ToArray() )
		{
			if ( tag.TimersRun && !tag.Warned && now >= tag.Born + tag.Row.Life - WarningBeforeTheEnd )
			{
				tag.Warned = true;
				// Effect 0x5b over the tag's middle (FUN_0048dae0, timer 0x80081); the interface draws no particle here.
				Unimplemented.Report( "TAG_ENDING_PARTICLE" );
			}

			if ( !tag.Hidden && now >= tag.HideAt ) Hide( tag, now );
			if ( now >= tag.OffAt ) TakeOff( tag );
		}
	}

	/// <summary>A tag's own right click, and its life's end: hidden at once, off the mast half a second on.</summary>
	internal void Hide( Tag tag, long now )
	{
		if ( tag.Hidden || !_tags.Contains( tag ) ) return;
		tag.Hidden = true;
		tag.OffAt = Math.Min( tag.OffAt, now + OffAfterHidden );
		++Changes;
	}

	/// <summary>The top's right click: every tag goes, the oldest first, a tenth of a second apart.</summary>
	internal void DeleteAll( long now )
	{
		for ( var i = 0; i < _tags.Count; ++i )
			_tags[i].OffAt = Math.Min( _tags[i].OffAt, now + DeleteAllStep * (i + 1) );
	}

	private void TakeOff( Tag tag )
	{
		_tags.Remove( tag );
		if ( tag == OnShow ) OnShow = null;
		++Changes;
		Log.Info( $"Park tags: tag {tag.Row.Id} subject {tag.Subject} off, standing {_tags.Count}" );
	}

	internal string Census( long now ) => $"tags: standing={_tags.Count} "
		+ string.Join( " ", _tags.Select( tag =>
			$"tag={tag.Row.Id}/picture={tag.Row.Picture}/subject={tag.Subject}/age={now - tag.Born}/hidden={(tag.Hidden ? 1 : 0)}"
			+ $"/item={tag.Item}/slot={tag.Slot}/life={tag.Row.Life}/timers={(tag.TimersRun ? 1 : 0)}/open={(tag == OnShow ? 1 : 0)}/text=\"{tag.Text}\"" ) );
}
