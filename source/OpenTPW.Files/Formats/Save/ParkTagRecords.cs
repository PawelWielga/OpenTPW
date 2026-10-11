using System.Buffers.Binary;

namespace OpenTPW;

/// <summary>
/// The untagged block that ends a park save's body, the tags standing on the gadget's aerial: a count, then a
/// record of <c>0x21c</c> bytes a tag. FileFormats <c>saves.md</c>, "The interface block"; the writer is
/// <c>FUN_0048ee50</c> and the loader <c>FUN_0048ef70</c> (<c>docs/exe/saves.md</c>, "OpenTPW's writer, the
/// aerial's tags").
///
/// <para>
/// <b>Found by anchors that have to agree</b>: the block begins right after the advisor scoring's one dword and
/// its tag (<c>CSDA</c>), which follow the cheats' tag (<c>STHC</c>), and its count's records end where the body
/// does.
/// </para>
/// </summary>
public sealed class ParkTagRecords
{
	/// <summary>The tag closing the advisor scoring's module, little-endian - <c>ADSC</c> read backwards.</summary>
	private const string PrecedingTrailer = "CSDA";

	/// <summary>The tag closing the cheats' module before it, <c>CHTS</c> read backwards; the advisor scoring is the dword between the two.</summary>
	private const string CheatsTrailer = "STHC";

	private const int AdvisorScoringSize = 4;

	/// <summary>A record's size: the id, the text, then the six words the tag's control keeps.</summary>
	public const int RecordSize = 0x21c;

	/// <summary>How many characters a tag's text holds before its terminator (<c>FUN_0048eb20</c> copies 0xff).</summary>
	public const int TextLength = 0xff;

	/// <summary>How many tags the aerial holds (<c>DAT_00750610</c>).</summary>
	public const int Most = 10;

	private const int TextAt = 0x004;
	private const int LifeAt = 0x204;
	private const int PictureAt = 0x208;
	private const int Button1At = 0x20c;
	private const int Button2At = 0x210;
	private const int ThingAt = 0x214;
	private const int ItemAt = 0x218;

	/// <summary>
	/// One tag as the file holds it: its row's id in the tag table, its text, its whole life in milliseconds (not
	/// what was left of it), the part of <c>msgtag.md2</c> it shows, the message bar's two buttons, and the thing
	/// and item its line was about, nought for none.
	/// </summary>
	public readonly record struct Record( int Id, string Text, int Life, int Picture, int Button1, int Button2,
		int Thing, int Item );

	/// <summary>Why the block was refused, or null where it was not. A refused block has no tags.</summary>
	public string? Problem { get; }

	/// <summary>The tags as saved, in the file's order; none where the block was refused.</summary>
	public IReadOnlyList<Record> Saved { get; } = [];

	/// <summary>Reads the block out of the inflated payload, the same array <see cref="ParkWorld"/> is given.</summary>
	public ParkTagRecords( byte[] inflatedPayload )
	{
		ArgumentNullException.ThrowIfNull( inflatedPayload );

		try
		{
			var at = FindBlock( inflatedPayload, out var count );
			var saved = new Record[count];

			for ( var i = 0; i < count; ++i )
			{
				var record = inflatedPayload.AsSpan( at + 4 + (i * RecordSize), RecordSize );

				saved[i] = new Record(
					Id: BinaryPrimitives.ReadInt32LittleEndian( record ),
					Text: TextOf( record.Slice( TextAt, LifeAt - TextAt ) ),
					Life: BinaryPrimitives.ReadInt32LittleEndian( record[LifeAt..] ),
					Picture: BinaryPrimitives.ReadInt16LittleEndian( record[PictureAt..] ),
					Button1: BinaryPrimitives.ReadInt32LittleEndian( record[Button1At..] ),
					Button2: BinaryPrimitives.ReadInt32LittleEndian( record[Button2At..] ),
					Thing: BinaryPrimitives.ReadUInt16LittleEndian( record[ThingAt..] ),
					Item: BinaryPrimitives.ReadInt32LittleEndian( record[ItemAt..] ) );
			}

			Saved = saved;
		}
		catch ( Exception e ) when ( e is InvalidDataException or ArgumentException )
		{
			Problem = e.Message;
		}
	}

	/// <summary>
	/// A copy of <paramref name="body"/> with <paramref name="tags"/> as its block, in place of the one it ends on.
	/// The block is found afresh, so the body may be one whose other modules have changed length.
	///
	/// <para>
	/// <b>A deviation: the bytes the original leaves unwritten are nought here.</b> Its record is built on the
	/// stack, so what follows the text's terminator and the two bytes after each of the picture and the thing are
	/// whatever stood there; its loader reads none of them.
	/// </para>
	/// </summary>
	/// <exception cref="InvalidOperationException">The body ends on no such block, or more tags are given than the aerial holds.</exception>
	public static byte[] Put( byte[] body, IReadOnlyList<Record> tags )
	{
		if ( tags.Count > Most )
			throw new InvalidOperationException( $"{tags.Count} tags are more than the aerial's {Most}" );

		int at;

		try
		{
			at = FindBlock( body, out _ );
		}
		catch ( InvalidDataException e )
		{
			throw new InvalidOperationException( $"the interface block was not found: {e.Message}" );
		}

		var written = new byte[at + 4 + (tags.Count * RecordSize)];
		body.AsSpan( 0, at ).CopyTo( written );
		BinaryPrimitives.WriteInt32LittleEndian( written.AsSpan( at ), tags.Count );

		for ( var i = 0; i < tags.Count; ++i )
		{
			var tag = tags[i];
			var record = written.AsSpan( at + 4 + (i * RecordSize), RecordSize );
			var text = tag.Text.Length > TextLength ? tag.Text[..TextLength] : tag.Text;

			BinaryPrimitives.WriteInt32LittleEndian( record, tag.Id );

			for ( var index = 0; index < text.Length; ++index )
				BinaryPrimitives.WriteUInt16LittleEndian( record[(TextAt + (2 * index))..], text[index] );

			BinaryPrimitives.WriteInt32LittleEndian( record[LifeAt..], tag.Life );
			BinaryPrimitives.WriteInt16LittleEndian( record[PictureAt..], (short)tag.Picture );
			BinaryPrimitives.WriteInt32LittleEndian( record[Button1At..], tag.Button1 );
			BinaryPrimitives.WriteInt32LittleEndian( record[Button2At..], tag.Button2 );
			BinaryPrimitives.WriteUInt16LittleEndian( record[ThingAt..], (ushort)tag.Thing );
			BinaryPrimitives.WriteInt32LittleEndian( record[ItemAt..], tag.Item );
		}

		return written;
	}

	/// <summary>The text up to its terminator; what follows one is the writer's stack.</summary>
	private static string TextOf( ReadOnlySpan<byte> field )
	{
		var text = new System.Text.StringBuilder();

		for ( var at = 0; at + 2 <= field.Length; at += 2 )
		{
			var character = BinaryPrimitives.ReadUInt16LittleEndian( field[at..] );

			if ( character == 0 )
				break;

			text.Append( (char)character );
		}

		return text.ToString();
	}

	/// <summary>The block's start: right after a <c>STHC</c> tag, a dword and a <c>CSDA</c> tag, where the count's records end the body.</summary>
	private static int FindBlock( byte[] data, out int count )
	{
		var cheats = System.Text.Encoding.ASCII.GetBytes( CheatsTrailer );
		var before = System.Text.Encoding.ASCII.GetBytes( PrecedingTrailer );
		var lead = cheats.Length + AdvisorScoringSize + before.Length;

		for ( var at = 0; at + lead + 4 <= data.Length; ++at )
		{
			if ( !data.AsSpan( at, cheats.Length ).SequenceEqual( cheats )
				|| !data.AsSpan( at + cheats.Length + AdvisorScoringSize, before.Length ).SequenceEqual( before ) )
				continue;

			var block = at + lead;
			count = BinaryPrimitives.ReadInt32LittleEndian( data.AsSpan( block ) );

			if ( count >= 0 && count <= Most && block + 4 + ((long)count * RecordSize) == data.Length )
				return block;
		}

		count = 0;
		throw new InvalidDataException(
			$"no {CheatsTrailer} tag, dword and {PrecedingTrailer} tag are followed by a count whose {RecordSize}-byte records end the {data.Length} bytes" );
	}
}
