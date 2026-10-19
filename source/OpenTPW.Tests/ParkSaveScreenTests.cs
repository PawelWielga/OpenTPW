using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVeldrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The Save Park screen (<c>docs/exe/saves.md</c>, "One screen, two uses", "What the handlers answer" and "OpenTPW's
/// Save Park"): the menu row that opens it, the name box, a row's click, OK, Enter and Escape, the overwrite question,
/// and the save it makes of the running park. Over a disposable save folder.
/// </summary>
[TestClass]
public class ParkSaveScreenTests
{
	private const int NameBox = 0x23bce2;
	private const int Ok = -1;
	private const int Cross = -2;
	private const int QuestionCross = 0x9873c8;
	private const int QuestionTick = 0x9873c9;

	private static readonly Vector2 Grass = new( 1100, 800 );

	/// <summary>The middle of the name box, 605, 811, 1343, 895 on a screen the virtual one's size.</summary>
	private static readonly Vector2 InTheBox = new( 974, 853 );

	private string? root;
	private BaseFileSystem oldSaves = null!;
	private Point2 _screen;
	private Level? _level;

	private string Jungle => Path.Combine( root!, "users", "1Test", "jungle" );

	[TestInitialize]
	public void IsolateSaves()
	{
		Log ??= new();
		FileSystem = GameData.Required();

		oldSaves = SaveFileSystem;
		root = Directory.CreateTempSubdirectory( "opentpw-save-park-" ).FullName;
		Directory.CreateDirectory( Jungle );
		SaveFileSystem = new BaseFileSystem( root );

		_screen = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );
		_level = Level.Current;

		Input.ForgetHeldKeys();
		Input.Mouse = new();
		Input.TextCaptured = false;
		Unimplemented.Forget();
		Time.Now = 100f;
		SetCurrentPlayer( new Player( 0, "Test", new PlayerFile { InstantAction = true } ) );
	}

	[TestCleanup]
	public void RestoreSaves()
	{
		// Without the game the set-up stops before anything is changed.
		if ( root == null )
			return;

		SetCurrentPlayer( null );
		SaveFileSystem = oldSaves;
		Directory.Delete( root, true );

		ParkBuildMode.Forget();
		Level.Current = _level!;
		Input.ForgetHeldKeys();
		Input.Mouse = new();
		Input.TextCaptured = false;
		Unimplemented.Forget();
		Screen.Size = _screen;
	}

	/// <summary>
	/// <b>The menu's Save Game closes the menu and opens the Save Park screen</b>, modal and pausing, counted as
	/// nothing, titled UITEXT 201, its rows the player's saved parks, with an OK button and a name box of fifteen
	/// characters that reads "New Save" and has the keys.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the row left counting <c>SAVE_GAME</c> opens nothing; the menu left open stands under the
	/// screen; Load's title; the fill skipped leaves no rows; the box given no focus leaves the keys with the park;
	/// the box left empty.
	/// </remarks>
	[TestMethod]
	public void SaveGameOpensTheSaveParkScreenWithItsNameBox()
	{
		Put( "New Save.TPWS", new DateTime( 2026, 10, 8, 12, 28, 0, DateTimeKind.Local ) );
		Put( "Old Park.TPWS", new DateTime( 2026, 10, 1, 14, 18, 0, DateTimeKind.Local ) );
		Put( "easymode.TPWI", new DateTime( 2026, 10, 1, 14, 18, 0, DateTimeKind.Local ) );

		var stack = APark();
		var screen = OpenSavePark( stack );

		Assert.AreEqual( "ParkGadget, ParkViewfinder, ParkSaveScreen", Names( stack ), "the menu closes and the screen opens" );
		Assert.AreEqual( "", Counted(), "opening counts nothing" );
		Assert.IsTrue( screen.Modal );
		Assert.IsTrue( screen.Pauses );
		Assert.IsTrue( screen.ClosesParkScreen );

		Assert.AreEqual( "0 New Save 8.10.2026 12:28 | 1 Old Park 1.10.2026 14:18",
			string.Join( " | ", ListOf( screen ).Rows.Select( row => $"{row.Id} {row.Name} {row.Values![0]}" ) ) );

		var controls = Everything( screen.Root ).ToArray();

		Assert.AreEqual( "Save Park", controls.Single( control => control.Id == 0x23bce1 ).Text );
		Assert.AreEqual( Localization.Text( 201 ), controls.Single( control => control.Id == 0x23bce1 ).Text );
		Assert.IsInstanceOfType( controls.Single( control => control.Id == Ok ), typeof( UI.UiButton ) );

		var box = (UI.UiEdit)controls.Single( control => control.Id == NameBox );

		Assert.AreEqual( 15, box.MaxLength );
		Assert.AreEqual( "New Save", box.Value );
		Assert.AreEqual( Localization.Text( 206 ), screen.Name );
		Assert.AreSame( box, screen.Focus );

		Frame( stack );
		Assert.IsTrue( Input.TextCaptured, "the box has the keys, so the park's shortcuts hear none" );
	}

	/// <summary>
	/// <b>What is typed first replaces the name the box opened on; a character a file name cannot hold is dropped,
	/// and so is the sixteenth.</b> The original's box read "ab", then "aba!cdefghijklm" and no more.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the opening text not selected gives "New Saveab"; a limit of sixteen takes the p; the refused
	/// list left empty takes the nine.
	/// </remarks>
	[TestMethod]
	public void TheBoxTakesFifteenCharactersAndRefusesNine()
	{
		var stack = APark();
		var screen = OpenSavePark( stack );

		Type( stack, "ab" );
		Assert.AreEqual( "ab", screen.Name );

		Type( stack, "\\/*?:|<>\"" );
		Assert.AreEqual( "ab", screen.Name, "none of the nine is taken" );

		Type( stack, "a!cdefghijklm" );
		Assert.AreEqual( "aba!cdefghijklm", screen.Name );
		Assert.AreEqual( 15, screen.Name.Length );

		Type( stack, "nop q" );
		Assert.AreEqual( "aba!cdefghijklm", screen.Name, "the sixteenth is dropped" );

		Press( stack, Key.BackSpace );
		Press( stack, Key.BackSpace );
		Type( stack, " Z" );
		Assert.AreEqual( "aba!cdefghijk Z", screen.Name, "a space and a capital are taken where there is room" );
	}

	/// <summary>
	/// <b>A row's click puts that save's name in the box, unselected</b>, so the next letter goes on its end; the
	/// folder is read again at the click; the screen stays open and nothing is counted.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the name put in selected gives "x" alone; the first row's name whatever was clicked; the
	/// list as it stood at the opening kept names the deleted file; the click saving or closing.
	/// </remarks>
	[TestMethod]
	public void ARowsClickPutsItsNameInTheBoxUnselected()
	{
		Put( "a.TPWS", DateTime.Now );
		Put( "b park.TPWS", DateTime.Now );
		Put( "c is a name of twenty.TPWS", DateTime.Now );

		var stack = APark();
		var screen = OpenSavePark( stack );

		Type( stack, "qw" );
		ListOf( screen ).Activated!( 1 );
		Assert.AreEqual( "b park", screen.Name );

		Type( stack, "x" );
		Assert.AreEqual( "b parkx", screen.Name, "not selected: the letter is added" );

		ListOf( screen ).Activated!( 2 );
		Assert.AreEqual( "c is a name of ", screen.Name, "the box's fifteen" );

		File.Delete( Path.Combine( Jungle, "a.TPWS" ) );
		ListOf( screen ).Activated!( 0 );
		Assert.AreEqual( "b park", screen.Name, "the first entry now" );

		ListOf( screen ).Activated!( 1 );
		ListOf( screen ).Activated!( 2 );
		Assert.AreEqual( "c is a name of ", screen.Name, "a place the folder has lost changes nothing" );

		Assert.AreEqual( "ParkGadget, ParkViewfinder, ParkSaveScreen", Names( stack ) );
		Assert.AreEqual( "", Counted() );
	}

	/// <summary>
	/// <b>OK over a name the folder does not hold writes the running park under it at once and closes the
	/// screen</b>, the player's gms.dat with it. A name that differs from a save's only by a space at its end is such
	/// a name, and so is an empty one, written as <c>.TPWS</c>; one that differs only in its case is asked nothing
	/// and written over the save of that name, as the original under Wine wrote over <c>New Save.TPWS</c>.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the save not made writes nothing; the screen left open; a compare that ignores case asks,
	/// and so does a name trimmed; an empty name refused stays open; the player not saved leaves no gms.dat.
	/// </remarks>
	[TestMethod]
	[DataRow( "zz", "New Save.TPWS, zz.TPWS" )]
	[DataRow( "new save", "New Save.TPWS" )]
	[DataRow( "New Save ", "New Save .TPWS, New Save.TPWS" )]
	[DataRow( "", ".TPWS, New Save.TPWS" )]
	public void OkOverANewNameWritesTheParkAndCloses( string name, string files )
	{
		Put( "New Save.TPWS", DateTime.Now );
		RunningPark( 4321 );

		var stack = APark();
		var screen = OpenSavePark( stack );

		Press( stack, Key.BackSpace );
		Type( stack, name );
		Assert.AreEqual( name, screen.Name );

		Click( screen, Ok );

		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ), "the screen closes, with no question" );
		Assert.AreEqual( "", Counted() );
		Assert.IsFalse( stack.AnyPausing );
		Assert.AreEqual( files, Parks(), "the park is written under the name" );
		Assert.AreEqual( 4321, TickIn( Directory.GetFiles( Jungle ).Single( file => string.Equals( Path.GetFileName( file ), name + ".TPWS", StringComparison.OrdinalIgnoreCase ) ) ), "and is the running park" );
		Assert.IsTrue( File.Exists( Path.Combine( Jungle, "..", "gms.dat" ) ), "the player is written with it" );
	}

	/// <summary><b>Enter in the box is the OK button and Escape the cancel button; the cross closes too.</b></summary>
	/// <remarks>
	/// <b>Mutations:</b> Enter not answered leaves the screen; Escape saving writes one; the cross saving writes one.
	/// </remarks>
	[TestMethod]
	public void EnterIsOkAndEscapeIsCancel()
	{
		RunningPark( 900 );

		var stack = APark();

		OpenSavePark( stack );
		Press( stack, Key.Escape );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ), "Escape closes" );
		Assert.AreEqual( "", Parks(), "and saves nothing" );
		Frame( stack );
		Assert.IsFalse( Input.TextCaptured, "the keys are the park's again" );

		Click( OpenSavePark( stack ), Cross );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ), "the cross closes" );
		Assert.AreEqual( "", Parks() );

		OpenSavePark( stack );
		Type( stack, "mine" );
		Press( stack, Key.Enter );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ), "Enter closes" );
		Assert.AreEqual( "mine.TPWS", Parks(), "and is the save" );
	}

	/// <summary>
	/// <b>OK over a name the folder holds asks UITEXT 205 first</b>, the name its parameter; the question's tick is
	/// the save, written over the file, and closes both, and its cross leaves the screen open and the file as it was.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the question skipped writes at once; the name left out reads " exists"; the tick not saving
	/// leaves the old file; the cross saving; the folder as it stood at the opening kept asks nothing of a file written
	/// since.
	/// </remarks>
	[TestMethod]
	public void ANameInTheFolderIsAskedAboutFirst()
	{
		RunningPark( 1234 );

		var stack = APark();
		var screen = OpenSavePark( stack );

		// Written after the screen opened: the folder is read again at OK.
		Put( "New Save.TPWS", DateTime.Now );

		Press( stack, Key.Enter );
		Assert.AreEqual( "ParkGadget, ParkViewfinder, ParkSaveScreen, MessageBox", Names( stack ) );
		Assert.AreEqual( 4, new FileInfo( OldSave ).Length, "nothing is saved yet" );

		var question = stack.Windows.OfType<UI.MessageBox>().Single();

		Assert.AreEqual( "New Save exists\n\nOverwrite ?", Everything( question.Root ).Single( control => control.Id == 0x9873c7 ).Text );

		Click( question, QuestionCross );
		Assert.AreEqual( "ParkGadget, ParkViewfinder, ParkSaveScreen", Names( stack ), "the cross leaves the screen" );
		Assert.AreEqual( 4, new FileInfo( OldSave ).Length, "and the file" );

		Click( screen, Ok );
		Click( stack.Windows.OfType<UI.MessageBox>().Single(), QuestionTick );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ), "the tick closes both" );
		Assert.AreEqual( "New Save.TPWS", Parks() );
		Assert.AreEqual( 1234, TickIn( OldSave ), "the running park is written over the old save" );
	}

	/// <summary>
	/// <b>The question takes the keys from the box, and its closing does not hand them back</b>: no letter, Enter or
	/// Escape is heard until the box is clicked, as in the original, where "y" then read "New Savey".
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the focus kept through the question takes the first y and closes on Escape; a click on the
	/// box giving nothing back leaves "New Save".
	/// </remarks>
	[TestMethod]
	public void TheBoxLosesTheKeysToTheQuestionUntilItIsClicked()
	{
		Put( "New Save.TPWS", DateTime.Now );
		RunningPark( 900 );

		var stack = APark();
		var screen = OpenSavePark( stack );

		ListOf( screen ).Activated!( 0 );
		Press( stack, Key.Enter );
		Click( stack.Windows.OfType<UI.MessageBox>().Single(), QuestionCross );

		Type( stack, "y" );
		Press( stack, Key.Enter );
		Press( stack, Key.Escape );
		Assert.AreEqual( "New Save", screen.Name, "the letter is not heard" );
		Assert.IsFalse( ((UI.UiEdit)Everything( screen.Root ).Single( control => control.Id == NameBox )).HasFocus, "and the box shows no caret" );
		Assert.AreEqual( "ParkGadget, ParkViewfinder, ParkSaveScreen", Names( stack ), "nor Enter, nor Escape, and no menu opens" );
		Assert.IsFalse( Input.TextCaptured );

		Frame( stack, InTheBox, left: true );
		Frame( stack, InTheBox, left: false );
		Type( stack, "y" );
		Assert.AreEqual( "New Savey", screen.Name, "a click on the box gives it the keys again" );

		Press( stack, Key.Enter );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ) );
		Assert.AreEqual( "New Save.TPWS, New Savey.TPWS", Parks() );
	}

	/// <summary>
	/// <b>With nobody playing the list is empty, and OK writes nothing, there being no folder, and closes.</b> With no
	/// park running it writes nothing either.
	/// </summary>
	[TestMethod]
	public void WithNobodyPlayingTheListIsEmpty()
	{
		Put( "a.TPWS", DateTime.Now );
		SetCurrentPlayer( null );
		RunningPark( 900 );

		var stack = APark();
		var screen = OpenSavePark( stack );

		Assert.AreEqual( 0, ListOf( screen ).Rows.Count );

		Click( screen, Ok );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ) );
		Assert.AreEqual( "a.TPWS", Parks() );

		SetCurrentPlayer( new Player( 0, "Test", new PlayerFile { InstantAction = true } ) );
		Level.Current = null!;
		Click( OpenSavePark( stack ), Ok );
		Assert.AreEqual( "ParkGadget, ParkViewfinder", Names( stack ) );
		Assert.AreEqual( "a.TPWS", Parks(), "no park, no save" );
	}

	/// <summary>
	/// <b>The save installs the idle mode before it writes</b> (<c>0x00516d13</c>): a build tool armed is put away.
	/// The menu's Escape has already done it on the way to the screen, so this is the console's <c>savepark</c>.
	/// </summary>
	/// <remarks><b>Mutations:</b> the hand not let go leaves the path tool armed.</remarks>
	[TestMethod]
	public void TheSaveLetsGoOfTheHand()
	{
		RunningPark( 900 );
		ParkBuildMode.Arm( ParkBuildMode.Path );

		Assert.IsNotNull( Level.Current.WritePark( "held" ) );
		Assert.AreEqual( ParkBuildMode.None, ParkBuildMode.Current );
		Assert.AreEqual( 900, TickIn( Path.Combine( Jungle, "held.TPWS" ) ) );
	}

	/// <summary>
	/// <b>Leaving a park writes it as <c>autosave.TPWS</c></b> (state 0xb, <c>0x0054ff10</c>), at the tick it was left
	/// on, beside the menu's saves, and entering the theme again finds it as the newest park file. Leaving the lobby
	/// writes nothing.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> another name; the running park's tick not written (the file's 755); the guard on the scene
	/// turned round writes nothing from a park.
	/// </remarks>
	[TestMethod]
	public void LeavingAParkWritesTheAutosaveThatEnteringFindsNewest()
	{
		Put( "easymode.TPWI", new DateTime( 2026, 10, 1, 14, 18, 0, DateTimeKind.Local ) );
		Put( "New Save.TPWS", new DateTime( 2026, 10, 8, 12, 28, 0, DateTimeKind.Local ) );

		var lobby = (Level)RuntimeHelpers.GetUninitializedObject( typeof( Level ) );
		typeof( Level ).GetProperty( nameof( Level.Kind ) )!.SetValue( lobby, Level.Scene.Lobby );
		Assert.IsNull( lobby.WriteParkOnLeaving() );
		Assert.AreEqual( "New Save.TPWS", Parks() );

		RunningPark( 1234 );

		Assert.AreEqual( "users/1Test/jungle/autosave.TPWS", Level.Current.WriteParkOnLeaving() );
		Assert.AreEqual( "New Save.TPWS, autosave.TPWS", Parks() );
		Assert.AreEqual( 1234, TickIn( Path.Combine( Jungle, "autosave.TPWS" ) ) );
		Assert.AreEqual( "autosave.TPWS", Path.GetFileName( SaveFolder.NewestPark( 0, "Test", "jungle" ) ) );
		Assert.AreEqual( "", Counted() );
	}

	/// <summary>
	/// The shipped park running in a level of its own at a tick of its own, which is not the file's 755, so a save
	/// that wrote the file as loaded is told from one that wrote the running park.
	/// </summary>
	private void RunningPark( int tick )
	{
		using var stream = new MemoryStream( FileSystem.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var reader = new SaveReader( stream );
		var world = new ParkWorld( reader.ReadFile(), reader.Preamble );
		var state = new ParkState( world );
		var level = (Level)RuntimeHelpers.GetUninitializedObject( typeof( Level ) );

		state.SetGameTick( tick );
		typeof( Level ).GetProperty( nameof( Level.Kind ) )!.SetValue( level, Level.Scene.Park );
		typeof( Level ).GetProperty( nameof( Level.ThemeName ) )!.SetValue( level, "jungle" );
		typeof( Level ).GetProperty( nameof( Level.Park ) )!.SetValue( level, world );
		typeof( Level ).GetProperty( nameof( Level.ParkState ) )!.SetValue( level, state );
		Level.Current = level;
	}

	/// <summary>The park files in the player's jungle folder, by name.</summary>
	private string Parks() => string.Join( ", ", Directory.GetFiles( Jungle, "*.TPWS" ).Select( Path.GetFileName ).Order( StringComparer.Ordinal ) );

	private string OldSave => Path.Combine( Jungle, "New Save.TPWS" );

	/// <summary>The <c>mGameTick</c> a park file holds.</summary>
	private static int TickIn( string path )
	{
		using var stream = new MemoryStream( File.ReadAllBytes( path ) );
		var reader = new SaveReader( stream );

		return new ParkWorld( reader.ReadFile(), reader.Preamble ).GameTick;
	}

	/// <summary>What has been counted, less the list's own count of a first row it does not select.</summary>
	private static string Counted() => string.Join( ", ", Unimplemented.Summary
		.Where( entry => entry.What != "LIST_FIRST_ROW_SELECTED" )
		.Select( entry => $"{entry.Times}x {entry.What}" ) );

	private static UI.WindowStack APark()
	{
		var stack = new UI.WindowStack();

		_ = new UI.ParkFrontEnd( stack, "jungle" );

		return stack;
	}

	/// <summary>Opens the game menu with Escape and chooses its second row, Save Game, as a click let go on it does.</summary>
	private static UI.ParkSaveScreen OpenSavePark( UI.WindowStack stack )
	{
		Press( stack, Key.Escape );

		if ( !stack.Windows.OfType<UI.GameMenu>().Any() )
			Press( stack, Key.Escape );

		var menu = stack.Windows.OfType<UI.GameMenu>().Single();
		var choices = (IList)typeof( UI.GameMenu ).GetField( "_choices", BindingFlags.Instance | BindingFlags.NonPublic )!.GetValue( menu )!;

		((UI.UiControl)choices[1]!).LeftClicked!();

		return stack.Windows.OfType<UI.ParkSaveScreen>().Single();
	}

	private static UI.UiList ListOf( UI.UiWindow screen ) => Everything( screen.Root ).OfType<UI.UiList>().Single();

	private static void Click( UI.UiWindow window, int id )
		=> ((UI.UiButton)Everything( window.Root ).Single( control => control.Id == id )).Clicked!();

	private static IEnumerable<UI.UiControl> Everything( UI.UiControl control )
		=> control.Children.SelectMany( Everything ).Prepend( control );

	private static void Frame( UI.WindowStack stack, params KeyEvent[] keys )
	{
		Input.UpdateFrom( new Snapshot( keys, "", Grass, false ) );
		stack.Update();
	}

	private static void Frame( UI.WindowStack stack, Vector2 at, bool left )
	{
		Input.UpdateFrom( new Snapshot( [], "", at, left ) );
		stack.Update();
	}

	private static void Press( UI.WindowStack stack, Key key )
	{
		Frame( stack, new KeyEvent( key, true, ModifierKeys.None ) );
		Frame( stack, new KeyEvent( key, false, ModifierKeys.None ) );
	}

	/// <summary>The characters as one frame's text input, then a frame with none.</summary>
	private static void Type( UI.WindowStack stack, string text )
	{
		Input.UpdateFrom( new Snapshot( [], text, Grass, false ) );
		stack.Update();
		Frame( stack );
	}

	private static string Names( UI.WindowStack stack ) => string.Join( ", ", stack.Windows.Select( window => window.GetType().Name ) );

	private void Put( string name, DateTime written )
	{
		var path = Path.Combine( Jungle, name );
		File.WriteAllBytes( path, [1, 2, 3, 4] );
		File.SetLastWriteTime( path, written );
	}

	private static void SetCurrentPlayer( Player? player )
		=> typeof( Players ).GetProperty( nameof( Players.Current ) )!.SetValue( Players.Roster, player );

	/// <summary>SDL's events for one frame: the keys, the text typed, where the pointer is and whether the left button is down.</summary>
	private sealed class Snapshot( KeyEvent[] keys, string typed, Vector2 at, bool left ) : InputSnapshot
	{
		public IReadOnlyList<KeyEvent> KeyEvents => keys;
		public IReadOnlyList<MouseEvent> MouseEvents => [];
		public IReadOnlyList<char> KeyCharPresses => typed.ToCharArray();
		public System.Numerics.Vector2 MousePosition => new( at.X, at.Y );
		public float WheelDelta => 0f;
		public bool IsMouseDown( MouseButton button ) => button == MouseButton.Left && left;
	}
}
