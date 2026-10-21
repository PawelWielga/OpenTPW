namespace OpenTPW;

/// <summary>
/// The gesture table at <c>0x0076dc18</c>: fourteen rows of <c>0x50</c> bytes, a response's gesture less 16 naming its
/// row (<c>docs/exe/scenes.md</c>, "Gesture table"). A row is clip words, each a clip number in its low bits with flag
/// bits <c>0x7f0000</c>, ended by nought; a word carrying <c>0x400000</c> is a cue, the word after it its time in
/// milliseconds. Copied from the executable.
/// </summary>
internal static class AdvisorGestures
{
	/// <summary>The bits of a clip word that are its clip (<c>0xff80ffff</c>).</summary>
	internal const uint ClipBits = 0xff80ffff;

	/// <summary>The bits of a clip word that are its flags (<c>0x7f0000</c>).</summary>
	internal const uint FlagBits = 0x7f0000;

	/// <summary>A clip word's flag: the clip shows no add-on, no mouth and no head (<c>0x00598dc0</c>, <c>0x00599b8c</c>).</summary>
	internal const int FaceAway = 0x10000;

	/// <summary>A word that is a cue, its time the word after (<c>0x00598c25</c>).</summary>
	internal const uint Cue = 0x400000;

	private static readonly uint[][] _rows =
	[
		[0x10],
		[0x20011, 0x20011, 0x20011, 0x20011, 0x40012, 0x90013, 0x110014],
		[],
		[],
		[],
		[0xe, 0x5, 0xb, 0xf],
		[0xe, 0xb, 0x1, 0xc, 0x1, 0xf],
		[0xe, 0xb, 0xb, 0x5, 0xf],
		[],
		[0xe, 0xb, 0x4, 0xf],
		[0xe, 0x5, 0x1, 0xb, 0x6, 0xf],
		[0xe, 0xb, 0x9, 0xf],
		[Cue, 17000],
		[0x20011, 0x20011, 0x20011, 0x20011, 0x40012, 0x90013, Cue, 1000, 0x110014]
	];

	/// <summary>
	/// A row as Advisor_PlayGestureRow (<c>0x00598bf0</c>) reads it: its clips and each clip's flags, in order; their
	/// lengths added up by <paramref name="lengthOf"/>; the cue's time from the line's start (the clips before it plus
	/// its word), null for none; and whether the line keeps its lead-in, which only a row opening on clip 14 does
	/// (an empty row keeps it too). Null for a row the table does not hold.
	/// </summary>
	internal static (List<(int Clip, int Flags)> Clips, int Milliseconds, int? CueAt, bool LeadIn)? Read( int row, Func<int, int> lengthOf )
	{
		if ( row < 0 || row >= _rows.Length )
			return null;

		var clips = new List<(int, int)>();
		var total = 0;
		int? cueAt = null;
		var words = _rows[row];

		for ( var i = 0; i < words.Length; ++i )
		{
			if ( (words[i] & Cue) == Cue )
			{
				cueAt = total + (int)words[++i];
				continue;
			}

			var clip = (int)(words[i] & ClipBits);
			clips.Add( (clip, (int)(words[i] & FlagBits)) );
			total += lengthOf( clip );
		}

		return (clips, total, cueAt, clips.Count == 0 || clips[0].Item1 == 14);
	}
}
