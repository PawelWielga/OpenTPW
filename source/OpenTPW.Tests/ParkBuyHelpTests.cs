using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Linq;
using System.Reflection;

namespace OpenTPW.Tests;

/// <summary>
/// The buy screen's help rows (<c>docs/exe/hud.md</c>, "The buy screen's help rows"): <c>FUN_004abc80</c> posts the row of
/// the item the list last selected while the pointer is on the list's rectangle, and only on a change of the item or of
/// its state. The rows and the order of what is posted are the original's, measured in it (<c>q495/orig/</c>).
/// </summary>
[TestClass]
public class ParkBuyHelpTests
{
	private Level? _level;
	private Point2 _screen;
	private bool _popupHelp;

	[TestInitialize]
	public void Keep()
	{
		_level = Level.Current;
		_screen = Screen.Size;
		_popupHelp = GameOptions.Current.PopupHelp;
		GameOptions.Current.PopupHelp = true;
		Log ??= new();
		Input.Mouse = new();
		HelpState = ParkBuyScreen.HelpOffList;
	}

	[TestCleanup]
	public void PutBack()
	{
		Level.Current = _level!;
		Screen.Size = _screen;
		GameOptions.Current.PopupHelp = _popupHelp;
		Input.Mouse = new();
		HelpState = ParkBuyScreen.HelpOffList;
	}

	/// <summary>The poster's state, a static as the original's global is: put back so one test does not hand it to the next.</summary>
	private static int HelpState
	{
		set => typeof( ParkBuyScreen ).GetField( "_helpState", BindingFlags.NonPublic | BindingFlags.Static )!.SetValue( null, value );
	}

	private static void SetBalance( int balance )
		=> typeof( ParkState ).GetProperty( nameof( ParkState.Balance ) )!.SetValue( Level.Current!.ParkState, balance );

	private static UiList ListOf( ParkBuyScreen screen )
		=> (UiList)typeof( ParkBuyScreen ).GetField( "_list", BindingFlags.NonPublic | BindingFlags.Instance )!.GetValue( screen )!;

	/// <summary>The middle of a row's slot on the list, by the row's id.</summary>
	private static Vector2 RowAt( UiList list, int id )
	{
		var slot = Enumerable.Range( 0, list.Rows.Count ).Single( at => list.Rows[at].Id == id );
		Assert.IsTrue( slot < list.VisibleRows );

		return new Vector2( (list.RowArea.Left + list.RowArea.Right) / 2f, list.RowArea.Top + (slot * list.RowHeight) + (list.RowHeight / 2f) );
	}

	/// <summary>One frame of the stack with the pointer moved to <paramref name="at"/>: what the bar says after it.</summary>
	private static string Frame( WindowStack stack, Vector2 at, bool moved = true )
	{
		Input.Mouse = new() { Position = at, Delta = moved ? new Vector2( 1, 0 ) : Vector2.Zero };
		stack.Update();

		return stack.HelpBarText;
	}

	/// <summary>
	/// The state is the price against the bank, a price equal to it buying the item, and for a mystery ride the player's
	/// tickets against its cost, equal buying it; a ride the player has unlocked is priced as any other.
	/// </summary>
	/// <remarks><b>Mutations:</b> the bank compared strictly; the tickets compared strictly; the mystery's two states swapped; the unlocked ride taken as a mystery.</remarks>
	[TestMethod]
	public void TheStateIsThePriceAgainstTheBankAndAMysteryRidesCostAgainstTheTickets()
	{
		var (catalogue, _, _) = ParkFootprintPictureTests.BuyScreen();
		var belly = catalogue.All.Single( item => item.Name == "Belly Bounce" );
		var mystery = catalogue.All.First( item => item.GoldenTicketCost > 1 );

		Assert.AreEqual( 500, belly.BuildPrice );
		Assert.AreEqual( ParkBuyScreen.HelpToBuy, ParkBuyScreen.HelpStateOf( belly, 500, null, 0 ) );
		Assert.AreEqual( ParkBuyScreen.HelpDear, ParkBuyScreen.HelpStateOf( belly, 499, null, 0 ) );

		var cost = mystery.GoldenTicketCost;
		Assert.AreEqual( ParkBuyScreen.HelpMysteryBought, ParkBuyScreen.HelpStateOf( mystery, 0, null, cost ) );
		Assert.AreEqual( ParkBuyScreen.HelpMysteryShort, ParkBuyScreen.HelpStateOf( mystery, 1_000_000, null, cost - 1 ) );

		ushort[] unlocked = [(ushort)mystery.Id];
		Assert.AreEqual( ParkBuyScreen.HelpToBuy, ParkBuyScreen.HelpStateOf( mystery, mystery.BuildPrice, unlocked, 0 ) );
		Assert.AreEqual( ParkBuyScreen.HelpDear, ParkBuyScreen.HelpStateOf( mystery, mystery.BuildPrice - 1, unlocked, cost ) );
	}

	/// <summary>
	/// Through the stack, as the original's round a went (<c>q495/PREDICTION.txt</c>, O2 to O8): the kind's name and the
	/// row's text with no space between, the heading's and the tab's own rows (both off the row area), and the row's line
	/// again on the way back from either; a change of the bank under a resting pointer posts the row again.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the row not taken from the selection; the off-list state not forgotten; the list's whole
	/// rectangle for its row area; the screen's post asked before the control's; the item's own name for its kind's; the
	/// dear and buy rows swapped.
	/// </remarks>
	[TestMethod]
	public void TheBuyListPostsItsItemsRowOnTheRows()
	{
		var (_, screen, stack) = ParkFootprintPictureTests.BuyScreen();
		var list = ListOf( screen );
		SetBalance( 1500 );

		var heading = new Vector2( 1243, 370 );
		var sideshows = new Vector2( 1453, 246 );
		var panel = new Vector2( 300, 300 );
		const string ape = "Pirate ShipYou can't afford to buy this item!";

		Assert.AreEqual( Localization.Help( 0x90 ), Frame( stack, heading ), "ours: no row is picked as the list fills (Q232)" );
		Assert.AreEqual( "Bouncy CastleLeft-click to buy", Frame( stack, RowAt( list, 1100 ) ) );
		Assert.AreEqual( Localization.Help( 0x90 ), Frame( stack, heading ), "the heading is off the rows: its own row (O2)" );
		Assert.AreEqual( ape, Frame( stack, RowAt( list, 1101 ) ) );
		Assert.AreEqual( ape, Frame( stack, RowAt( list, 1101 ), false ), "it stands" );
		Assert.AreEqual( Localization.Help( 142 ), Frame( stack, sideshows ), "the tab's own row (O5)" );
		Assert.AreEqual( ape, Frame( stack, RowAt( list, 1101 ) ), "back from the tab, which is off the rows: posted again (O6)" );
		Assert.AreEqual( "", Frame( stack, panel ) );
		Assert.AreEqual( ape, Frame( stack, RowAt( list, 1101 ) ), "from off the list: posted again (O8)" );

		SetBalance( 2000 );
		Assert.AreEqual( "Pirate ShipLeft-click to buy", Frame( stack, RowAt( list, 1101 ), false ), "the bank moved: the state did" );
		Assert.AreEqual( "help item 1101 posted 1101 state 3", screen.HelpCensus() );
	}

	/// <summary>
	/// The poster itself: nothing until the item or its state changes, and from off the rows the item spoken of again.
	/// </summary>
	/// <remarks><b>Mutations:</b> posted every frame; posted off the rows; the state not compared.</remarks>
	[TestMethod]
	public void ThePosterPostsOnAChangeOfTheItemOrItsStateAlone()
	{
		var (_, screen, _) = ParkFootprintPictureTests.BuyScreen();
		screen.RowSelected( 1100 );

		Assert.AreEqual( 0x94, screen.HelpPost( true, 500, null, 0 )?.Row );
		Assert.IsNull( screen.HelpPost( true, 500, null, 0 ), "the same item in the same state" );
		Assert.AreEqual( 0x96, screen.HelpPost( true, 499, null, 0 )?.Row, "the bank fell under the price" );
		Assert.IsNull( screen.HelpPost( false, 499, null, 0 ), "off the rows: nothing" );
		Assert.AreEqual( 0x96, screen.HelpPost( true, 499, null, 0 )?.Row, "back on them: spoken of again" );
		screen.RowSelected( 1101 );
		Assert.AreEqual( 0x96, screen.HelpPost( true, 499, null, 0 )?.Row, "another item in the same state" );
	}

	/// <summary>
	/// The name the row gives is the kind's, <c>Info.RideTypeStringIndex</c>'s row of <c>ITEMTYPES.str</c>: a sideshow's
	/// too, and a shop, which sets none, is named nothing (round b, B2 and B3).
	/// </summary>
	/// <remarks><b>Mutations:</b> the key not read; the item's own name; a missing index read as row 1.</remarks>
	[TestMethod]
	public void AnItemIsNamedByItsKind()
	{
		var (catalogue, _, _) = ParkFootprintPictureTests.BuyScreen();
		string Kind( string name ) => ParkBuyScreen.TypeNameOf( catalogue.All.Single( item => item.Name == name ) );

		Assert.AreEqual( "Bouncy Castle", Kind( "Belly Bounce" ) );
		Assert.AreEqual( "Simulator", Kind( "Aztec Mayhem" ) );
		Assert.AreEqual( "Shooting Gallery", Kind( "Jungle Spray" ) );
		Assert.AreEqual( "", Kind( "Drinks Shop" ) );
	}

	/// <summary>
	/// The land rows (O10 to O12): Buy Land's line and Clear Land's, each posted once for as long as it is the item, so
	/// back on Buy Land from off the list the bar stays empty, and the state is left forgotten.
	/// </summary>
	/// <remarks><b>Mutations:</b> the land rows' lines swapped; a land row posted again from off the list.</remarks>
	[TestMethod]
	public void ALandRowIsPostedOnceForAsLongAsItIsTheItem()
	{
		var (_, screen, stack) = ParkFootprintPictureTests.BuyScreen();
		var list = ListOf( screen );
		screen.PickTab( 3 );

		var panel = new Vector2( 300, 300 );

		Assert.AreEqual( Localization.Help( 0x21a ), Frame( stack, RowAt( list, -1 ) ) );
		Assert.AreEqual( "", Frame( stack, panel ) );
		Assert.AreEqual( "", Frame( stack, RowAt( list, -1 ) ), "a land row already posted is not posted again" );
		Assert.AreEqual( "help item -1 posted -1 state -1", screen.HelpCensus() );
		Assert.AreEqual( Localization.Help( 0x21b ), Frame( stack, RowAt( list, -2 ) ) );
		Assert.AreEqual( Localization.Help( 0x21a ), Frame( stack, RowAt( list, -1 ) ) );
	}
}
