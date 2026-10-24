namespace OpenTPW;

/// <summary>
/// Program entry point
/// </summary>
public class Program
{
	public static void Main( string[] args )
	{
		// What is printed from here on is kept in console.log once the game's folder is found - see ConsoleLog.
		ConsoleLog.Start();

		// Before anything can open a native library - see NativeLibraries.
		NativeLibraries.Register();

		Game.Run( args );
	}
}
