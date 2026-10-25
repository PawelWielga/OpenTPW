using OpenTPW;
using static OpenTPW.Common.GlobalNamespace;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
var checks = 0;
void Check( bool condition, string message )
{
	if ( !condition ) throw new Exception( "FAIL: " + message );
	++checks;
}
void Reject( Action action, string message )
{
	try { action(); } catch ( ArgumentException ) { ++checks; return; }
	throw new Exception( "FAIL: " + message );
}

Check( GameLanguage.Select( ["English", "Polish"], "pl", "en-US" ) == "Polish", "pl alias" );
Check( GameLanguage.Select( ["Polish"], null, "en-US" ) == "Polish", "single-language installation" );
Check( GameLanguage.Select( ["English", "Polish"], "auto", "pl-PL" ) == "Polish", "system language" );
Check( GameLanguage.Select( ["Polish", "English"], null, "ja-JP" ) == "English", "English fallback" );
Check( GameLanguage.Select( ["American"], "en", "pl-PL" ) == "American", "US English alias" );
Check( GameLanguage.Select( ["pOLiSh"], "POLISH", "en-US" ) == "pOLiSh", "preserve installed spelling" );
Check( GameLanguage.Requested( ["--language", "pl"], "en" ) == "pl", "CLI overrides environment" );
Check( GameLanguage.Requested( ["--language=pl"], null ) == "pl", "equals syntax" );
Check( GameLanguage.Requested( [], "pl" ) == "pl", "environment selection" );
Reject( () => GameLanguage.Requested( ["--language"], null ), "missing option value" );
Reject( () => GameLanguage.Requested( ["--language", "--game"], null ), "next option is not a value" );
Reject( () => GameLanguage.Requested( ["--language="], null ), "empty value" );
Reject( () => GameLanguage.Select( ["English"], "pl", "en-US" ), "missing requested language" );
Reject( () => GameLanguage.Select( [], "auto", "pl-PL" ), "no complete language" );
Reject( () => GameLanguage.Select( ["Polish"], "../Polish", "en-US" ), "folder traversal" );

byte[] Alphabet( ushort version, byte single, byte extended, string chars )
{
	using var bytes = new MemoryStream();
	using var writer = new BinaryWriter( bytes );
	writer.Write( Encoding.ASCII.GetBytes( "BFMU" ) );
	writer.Write( version );
	writer.Write( single );
	writer.Write( extended );
	writer.Write( Encoding.Unicode.GetBytes( chars ) );
	return bytes.ToArray();
}
var direct = new BFMUReader( new MemoryStream( Alphabet( 0, 3, 0, "AąZ" ) ) );
Check( direct.Decode( new byte[] { 1, 2, 3 } ) == "AąZ", "version zero compatibility" );
var extended = new BFMUReader( new MemoryStream( Alphabet( 1, 3, 2, "ABąŁ" ) ) );
Check( extended.Decode( new byte[] { 1, 3, 1, 2, 3, 2 } ) == "AąBŁ", "version one escape pairs" );
Check( extended.Count == 4, "extended alphabet size" );
foreach ( var invalid in new[] { new byte[] { 3 }, new byte[] { 3, 0 }, new byte[] { 3, 3 }, new byte[] { 0 } } )
{
	try { extended.Decode( invalid ); throw new Exception( "Invalid encoding was accepted" ); }
	catch ( InvalidDataException ) { ++checks; }
}
using ( var emptyBytes = new MemoryStream() )
{
	using ( var writer = new BinaryWriter( emptyBytes, Encoding.UTF8, leaveOpen: true ) )
	{
		writer.Write( Encoding.ASCII.GetBytes( "BFST" ) );
		writer.Write( 0 );
		writer.Write( 1 );
		writer.Write( 4 );
		writer.Write( 1 );
		writer.Write( 0 );
	}
	var empty = new StringFile( new MemoryStream( emptyBytes.ToArray() ) );
	Check( empty[0] == "" && empty.Parts[0][0].Text == "", "empty text does not need a mounted character table" );
}
var report = new Dictionary<string, object> { ["unitChecks"] = checks };
if ( args.Length == 0 )
{
	Console.WriteLine( JsonSerializer.Serialize( report ) );
	Console.WriteLine( "Real game data not tested: pass the data directory as the first argument." );
	return;
}

FileSystem = new BaseFileSystem( Path.GetFullPath( args[0] ) );
Log = new Logger();
Check( GameLanguage.Configure( ["--language", "pl"] ), "configure Polish" );
Check( GameLanguage.Current == "Polish", "selected Polish folder" );
Check( GameLanguage.PathFor( "UITEXT.str" ) == "Language/Polish/UITEXT.str", "selected asset path" );

var output = Console.Out;
var tables = new Dictionary<string, int>();
var allText = new StringBuilder();
string firstTheme;
string sampleUi;
try
{
	Console.SetOut( TextWriter.Null );
	foreach ( var path in FileSystem.GetFiles( GameLanguage.Folder ).Where( path => path.EndsWith( ".str", StringComparison.OrdinalIgnoreCase ) ) )
	{
		var relative = FileSystem.GetRelativePath( path ).TrimStart( '/' );
		var strings = new StringFile( relative );
		Check( strings.Entries.Length == strings.Parts.Length, "complete table " + path );
		tables[Path.GetFileName( path )] = strings.Entries.Length;
		foreach ( var row in strings.Parts )
			foreach ( var part in row )
				if ( part.Text != null ) allText.AppendLine( part.Text );
	}
	firstTheme = new StringFile( GameLanguage.PathFor( "THEMENAMES.str" ) )[0];
	sampleUi = Localization.Get( UIStrings.Options );
	Check( sampleUi == "Opcje", "production Localization decodes Options exactly" );
	Check( firstTheme == "Zaginione Królestwo", "Polish theme name decodes exactly" );
	Check( Localization.Help( -1 ) == "", "out of range help" );
	Check( Localization.Text( int.MaxValue ) == "", "out of range UI" );
}
finally { Console.SetOut( output ); }

var text = allText.ToString();
var lowerPolish = "ąćęłńóśźż";
foreach ( var character in lowerPolish )
	Check( text.Contains( character ), "decoded Polish character " + character );

var fontCoverage = new Dictionary<string, string>();
foreach ( var name in new[] { "GAME9.bf4", "GAME10.bf4", "GAME12.bf4", "GAME12AA.bf4", "TITLEBIG.bf4", "MENUBIG.bf4" } )
{
	var bytes = FileSystem.ReadAllBytes( GameLanguage.PathFor( name ) );
	Check( bytes.Length >= 8 && Encoding.ASCII.GetString( bytes, 0, 4 ) == "F4FB", "font header " + name );
	var glyphs = new HashSet<int>();
	var count = BitConverter.ToUInt16( bytes, 6 );
	for ( var i = 0; i < count; ++i )
	{
		var at = BitConverter.ToInt32( bytes, 8 + i * 4 );
		Check( at >= 0 && at <= bytes.Length - 24, "font glyph bounds " + name );
		glyphs.Add( BitConverter.ToInt32( bytes, at ) );
	}
	var missing = new string( lowerPolish.Where( ch => !glyphs.Contains( ch ) ).ToArray() );
	fontCoverage[name] = missing;
	Check( missing.Length == 0, "font Polish glyphs " + name + ": " + missing );
}

report["checks"] = checks;
report["language"] = GameLanguage.Current;
report["tables"] = tables;
report["totalRows"] = tables.Values.Sum();
report["firstTheme"] = firstTheme;
report["firstUiString"] = sampleUi;
report["polishCharacters"] = lowerPolish;
report["fontMissingCharacters"] = fontCoverage;
report["renderingTested"] = false;
report["speechPlaybackTested"] = false;
Console.WriteLine( JsonSerializer.Serialize( report, new JsonSerializerOptions { WriteIndented = true } ) );
