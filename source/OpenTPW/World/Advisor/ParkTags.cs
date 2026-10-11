namespace OpenTPW;

/// <summary>
/// The tags a said line leaves on the gadget's aerial: the tag system's hearing of the event a line sends
/// (FUN_00509bf0, type 0x18) and the messages control's list (FUN_0048eb20). A line's response group names a row
/// of the tag table; the row is the tag's picture, the tag it takes the place of and how long it stands.
/// docs/exe/advisor-park.md, "The tag a line leaves on the aerial".
/// <para>
/// The times are the interface's timers', real milliseconds, handed in by the caller. A tag's item word is not
/// kept: the list matches a tag by its thing and its item, and a thing has one item.
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
		new( 68, NoTag, 1, 71, 3, 0, 30000, 2 ),
		new( 70, 68, 1, 72, 3, 0, 30000, 2 ),
		new( 71, 68, 1, 73, 3, 0, 30000, 2 ),
		new( 72, 68, 1, 74, 3, 0, 30000, 2 ),
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

	internal sealed class Tag
	{
		internal required Row Row { get; init; }
		/// <summary>The thing the line was about, nought for none.</summary>
		internal int Subject { get; init; }
		internal long Born { get; init; }
		/// <summary>When timer 0x80080 hides it; never for a row with no life.</summary>
		internal long HideAt { get; set; } = long.MaxValue;
		/// <summary>When message 5 takes it off the mast.</summary>
		internal long OffAt { get; set; } = long.MaxValue;
		/// <summary>Hidden and still holding its place on the mast.</summary>
		internal bool Hidden { get; set; }
		internal bool Warned { get; set; }
	}

	private readonly List<Tag> _tags = [];
	private readonly Row[] _rows;

	internal ParkTags( IEnumerable<Row>? rows = null ) => _rows = (rows ?? Rows).ToArray();

	/// <summary>The tags on the mast, the oldest first: the oldest stands lowest.</summary>
	internal IReadOnlyList<Tag> Standing => _tags;
	/// <summary>One up at every change of the list or of a tag's showing.</summary>
	internal int Changes { get; private set; }
	/// <summary>How many tags the list has taken, for the new tag's sound.</summary>
	internal int Made { get; private set; }

	/// <summary>
	/// A line of response group <paramref name="group"/> about <paramref name="subject"/> was said. A tag of the
	/// row's key about the same thing is taken off at once, or with a row that is its own key is left as the
	/// line's tag; otherwise the list takes a new tag while it holds fewer than ten.
	/// </summary>
	internal Tag? Said( int group, int subject, long now )
	{
		var row = Array.Find( _rows, candidate => candidate.Id == group );
		if ( row == null )
		{
			// A group with a row the table holds and this list does not.
			if ( group != NoTag && group != NoTag - 1 )
				Unimplemented.Report( "TAG_ROW_NOT_LISTED" );
			return null;
		}

		var fresh = true;
		foreach ( var tag in _tags.ToArray() )
		{
			if ( tag.Row.Id != row.Key || tag.Subject != subject ) continue;
			if ( row.Id == row.Key )
				fresh = false;
			else
				TakeOff( tag );
		}

		if ( !fresh || _tags.Count >= Most ) return null;

		var made = new Tag { Row = row, Subject = subject, Born = now };
		if ( row.Life > 0 ) made.HideAt = now + row.Life;
		_tags.Add( made );
		++Changes;
		++Made;
		Log.Info( $"Park tags: tag {row.Id} picture {row.Picture} subject {subject} life {row.Life} standing {_tags.Count}" );
		return made;
	}

	/// <summary>The timers' turn: a tag's particle five seconds before its end, its hiding at its end, and its going.</summary>
	internal void Tick( long now )
	{
		foreach ( var tag in _tags.ToArray() )
		{
			if ( !tag.Warned && tag.Row.Life > 0 && now >= tag.Born + tag.Row.Life - WarningBeforeTheEnd )
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
		++Changes;
		Log.Info( $"Park tags: tag {tag.Row.Id} subject {tag.Subject} off, standing {_tags.Count}" );
	}

	internal string Census( long now ) => $"tags: standing={_tags.Count} "
		+ string.Join( " ", _tags.Select( tag =>
			$"tag={tag.Row.Id}/picture={tag.Row.Picture}/subject={tag.Subject}/age={now - tag.Born}/hidden={(tag.Hidden ? 1 : 0)}" ) );
}
