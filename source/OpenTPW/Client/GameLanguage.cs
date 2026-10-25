using System.Globalization;

namespace OpenTPW;

/// <summary>Selects one installed language before any localized assets are loaded.</summary>
public static class GameLanguage
{
	public static string Current { get; private set; } = "English";
	public static string Folder => $"Language/{Current}";

	/// <summary>A file in the selected language, including its own character table and fonts.</summary>
	public static string PathFor( string fileName ) => $"{Folder}/{fileName}";

	public static bool Configure( string[] args )
	{
		try
		{
			var requested = Requested( args, Environment.GetEnvironmentVariable( "OPENTPW_LANGUAGE" ) );
			var installed = FileSystem.GetDirectories( "Language" )
				.Select( path => Path.GetFileName( path.TrimEnd( '/', '\\' ) ) )
				.Where( name => FileSystem.FileExists( $"Language/{name}/UITEXT.str" )
					&& FileSystem.FileExists( $"Language/{name}/UIHELPTEXT.str" )
					&& FileSystem.FileExists( $"Language/{name}/MBToUni.dat" ) )
				.ToArray();

			Current = Select( installed, requested, CultureInfo.CurrentUICulture.Name );
			Log.Info( $"Language: {Current} ({Folder})" );
			return true;
		}
		catch ( Exception e ) when ( e is ArgumentException or IOException )
		{
			Log.Error( $"Language: {e.Message}" );
			return false;
		}
	}

	internal static string? Requested( string[] args, string? environment )
	{
		string? requested = environment;

		for ( var i = 0; i < args.Length; ++i )
		{
			if ( args[i] == "--language" )
			{
				if ( ++i >= args.Length || string.IsNullOrWhiteSpace( args[i] ) || args[i].StartsWith( "--", StringComparison.Ordinal ) )
					throw new ArgumentException( "--language requires a language, for example pl, en or auto." );

				requested = args[i];
			}
			else if ( args[i].StartsWith( "--language=", StringComparison.Ordinal ) )
			{
				requested = args[i]["--language=".Length..];

				if ( string.IsNullOrWhiteSpace( requested ) )
					throw new ArgumentException( "--language requires a language, for example pl, en or auto." );
			}
		}

		return requested;
	}

	/// <summary>Explicit selection must exist; automatic selection prefers the system language, then English.</summary>
	internal static string Select( IEnumerable<string> installed, string? requested, string culture )
	{
		var names = installed.Distinct( StringComparer.OrdinalIgnoreCase )
			.OrderBy( name => name, StringComparer.OrdinalIgnoreCase ).ToArray();

		if ( names.Length == 0 )
			throw new ArgumentException( "No complete language folder was found: UITEXT.str, UIHELPTEXT.str and MBToUni.dat are required." );

		if ( !string.IsNullOrWhiteSpace( requested ) && !requested.Equals( "auto", StringComparison.OrdinalIgnoreCase ) )
			return Match( names, requested ) ?? throw new ArgumentException(
				$"Language '{requested}' is not installed. Available: {string.Join( ", ", names )}." );

		return Match( names, culture ) ?? Match( names, "en" ) ?? names[0];
	}

	private static string? Match( string[] names, string requested )
	{
		var exact = names.FirstOrDefault( name => name.Equals( requested, StringComparison.OrdinalIgnoreCase ) );

		if ( exact != null )
			return exact;

		var candidates = requested.ToLowerInvariant() switch
		{
			"pl" or "pl-pl" => new[] { "Polish" },
			"en-us" => new[] { "American", "English" },
			"en" or "en-gb" => new[] { "English", "American" },
			"de" or "de-de" => new[] { "German" },
			"fr" or "fr-fr" => new[] { "French" },
			"it" or "it-it" => new[] { "Italian" },
			"nl" or "nl-nl" => new[] { "Dutch" },
			"es" or "es-es" => new[] { "Spanish" },
			_ => Array.Empty<string>()
		};

		return candidates.Select( candidate => names.FirstOrDefault(
			name => name.Equals( candidate, StringComparison.OrdinalIgnoreCase ) ) ).FirstOrDefault( name => name != null );
	}
}
