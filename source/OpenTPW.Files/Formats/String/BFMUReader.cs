using System.Text;

namespace OpenTPW;
public sealed class BFMUReader : BaseFormat
{
	private ExpandedMemoryStream memoryStream;
	public byte[] buffer;

	public BFMUReader( string path )
	{
		ReadFromFile( path );
	}

	public BFMUReader( Stream stream )
	{
		ReadFromStream( stream );
	}
	public void Dispose()
	{
		memoryStream.Dispose();
	}

	protected override void ReadFromStream( Stream stream )
	{
		// Set up read buffer
		var tempStreamReader = new StreamReader( stream );
		var fileLength = (int)tempStreamReader.BaseStream.Length;

		if ( fileLength < 8 )
			throw new InvalidDataException( "The BFMU header is truncated." );

		buffer = new byte[fileLength];
		tempStreamReader.BaseStream.ReadExactly( buffer );
		tempStreamReader.Close();

		memoryStream = new ExpandedMemoryStream( buffer );

		var magicNumber = memoryStream.ReadString( 4 );

		if ( magicNumber != "BFMU" )
			throw new Exception( $"Magic number did not match: {magicNumber}" );

		//ReadFile();
		//CharacterArray();
	}

	/// <summary>The BFMU alphabet. Version 1 reserves the last single-byte code as an escape.</summary>
	public List<char> CharacterArray()
	{
		var version = BitConverter.ToUInt16( buffer, 4 );

		if ( version > 1 )
			throw new InvalidDataException( $"Unsupported BFMU version: {version}" );

		var count = version == 0 ? buffer[6] : buffer[6] - 1 + buffer[7];

		if ( (version == 1 && buffer[6] == 0) || buffer.Length < 8 + count * 2 )
			throw new InvalidDataException( "The BFMU alphabet is truncated." );

		var characters = new List<char>( count );

		for ( var i = 0; i < count; ++i )
			characters.Add( (char)BitConverter.ToUInt16( buffer, 8 + i * 2 ) );

		return characters;
	}

	/// <summary>Decodes the byte span of one BFST text part, including version 1 escape pairs.</summary>
	public string Decode( ReadOnlySpan<byte> encoded )
	{
		_characters ??= CharacterArray();
		var escaped = BitConverter.ToUInt16( buffer, 4 ) == 1;
		var text = new StringBuilder( encoded.Length );

		for ( var i = 0; i < encoded.Length; ++i )
		{
			var code = (int)encoded[i];

			if ( escaped && code == buffer[6] )
			{
				if ( ++i >= encoded.Length || encoded[i] == 0 || encoded[i] > buffer[7] )
					throw new InvalidDataException( "Invalid BFMU escape pair." );

				code = buffer[6] - 1 + encoded[i];
			}

			text.Append( GetCharacter( code ) );
		}

		return text.ToString();
	}

	public void ReadFile()
	{
		/*
		
		Header

		4 bytes: Magic number - "BFMU"
		2 bytes: Likely specifies the character encoding - usually 0x00
		2 bytes: Character count

		# For each character

		2 bytes - The character itself in either Unicode or multibyte form

		*/

		var encoding = memoryStream.ReadBytes( 2 );
		Log.Info($"Encoding: {encoding}", true );
		var charCount = memoryStream.ReadBytes( 2 );

		for ( int i = 0; i < memoryStream.Length - memoryStream.Position; i++ )
		{
			//Log.Info( $"Character: {GetCharacter( memoryStream.Position )} @ Position: {memoryStream.Position}", true );
			_= memoryStream.ReadByte();
		}

	}

	/// <summary>The table, decoded once: it never changes, and a string asks for it once per character.</summary>
	private List<char>? _characters;

	/// <summary>The number of characters in the table.</summary>
	public int Count => (_characters ??= CharacterArray()).Count;

	public char GetCharacter( int character )
	{
		_characters ??= CharacterArray();

		// Characters are offset by 0x01 in the BFMU!
		if ( character < 1 || character > _characters.Count )
			throw new InvalidDataException( $"BFMU character index is out of range: {character}" );

		return _characters[character - 0x01];
	}

}
