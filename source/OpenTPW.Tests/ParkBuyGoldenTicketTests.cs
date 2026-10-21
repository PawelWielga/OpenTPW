using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Linq;
using System.Reflection;

namespace OpenTPW.Tests;

/// <summary>
/// The buy list's golden-ticket rows and their click (<c>docs/exe/hud.md</c>, "Spending golden tickets"), on the player
/// the original was measured with (<c>q709/gms-two.dat</c>: 4 tickets earned, 2 spent, no ride bought).
/// </summary>
[TestClass]
public class ParkBuyGoldenTicketTests
{
	private Level? _level;
	private Point2 _screen;
	private Player? _player;
	private int _sort;

	private static readonly PropertyInfo CurrentPlayer = typeof( Players ).GetProperty( nameof( Players.Current ) )!;

	private static readonly FieldInfo Sort = typeof( ParkBuyScreen ).GetField( "_sort", BindingFlags.Static | BindingFlags.NonPublic )!;

	[TestInitialize]
	public void Keep()
	{
		_level = Level.Current;
		_screen = Screen.Size;
		_player = Players.Roster.Current;
		_sort = (int)Sort.GetValue( null )!;
		Log ??= new();
		FileSystem = GameData.Required();
		Sort.SetValue( null, 1 );
		typeof( ParkBuyScreen ).GetField( "_lastTab", BindingFlags.Static | BindingFlags.NonPublic )!.SetValue( null, 0 );
	}

	[TestCleanup]
	public void PutBack()
	{
		ParkHand.LetGo();
		Level.Current = _level!;
		Screen.Size = _screen;
		CurrentPlayer.SetValue( Players.Roster, _player );
		Sort.SetValue( null, _sort );
	}

	/// <summary>The player of <c>gms-two.dat</c>: 4 tickets earned, 2 spent, no ride bought.</summary>
	private static PlayerFile TwoLeft()
	{
		var file = new PlayerFile { RecordTickets = [1, 1, 1, 1], TicketsSpent = 2 };
		CurrentPlayer.SetValue( Players.Roster, new Player( 0, "Test", file ) );

		return file;
	}

	private static void SetBalance( int balance )
		=> typeof( ParkState ).GetProperty( nameof( ParkState.Balance ) )!.SetValue( Level.Current!.ParkState, balance );

	private static UiList ListOf( ParkBuyScreen screen )
		=> (UiList)typeof( ParkBuyScreen ).GetField( "_list", BindingFlags.NonPublic | BindingFlags.Instance )!.GetValue( screen )!;

	/// <summary>The colour of the cell lettered <paramref name="text"/>.</summary>
	private static UiColour CellColour( UiList list, string text )
		=> list.Children.Single( cell => cell.Visible && cell.Text == text ).TextColour;

	/// <summary>
	/// <c>PriceSkin_SetValue</c>: "$ " before a number at or above nought, " gt" after the negation of one below, and a
	/// number past the bias lettered less it.
	/// </summary>
	/// <remarks><b>Mutations:</b> the negative not negated; nought taken as tickets; the bias's bound strict the other way.</remarks>
	[TestMethod]
	public void ThePriceCellLettersCashAndTickets()
	{
		Assert.AreEqual( "$ 2500", ParkBuyScreen.PriceText( 2500 ) );
		Assert.AreEqual( "$ 0", ParkBuyScreen.PriceText( 0 ) );
		Assert.AreEqual( "1 gt", ParkBuyScreen.PriceText( -1 ) );
		Assert.AreEqual( "3 gt", ParkBuyScreen.PriceText( -3 ) );
		Assert.AreEqual( "$ 250539758", ParkBuyScreen.PriceText( 0xeeeeeee ) );
		Assert.AreEqual( "250539758 gt", ParkBuyScreen.PriceText( -0xeeeeeee ) );
		Assert.AreEqual( "$ 1", ParkBuyScreen.PriceText( 0xeeeeeef ) );
		Assert.AreEqual( "-$ 5", ParkBuyScreen.PriceText( -(0xeeeeeee + 5) ) );
	}

	/// <summary>
	/// The rides tab as the original drew it (<c>q709/orig/a-lists.png</c>): the two mystery rows first, Jurassic Tours'
	/// (1170) before Eruption's (1112) as the catalogue walks them, "1 gt" in gold and "3 gt" in red; a cash row white, red
	/// once its price is above the bank on the next tick; and a land row (231, 239, 75).
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rows handed over in name order; the colour not asked as the rows are drawn; the tickets
	/// compared strictly; a mystery row coloured by the bank; the price lettered bare.
	/// </remarks>
	[TestMethod]
	public void TheMysteryRowsStandInTheCataloguesOrderLetteredAndColouredByTheTickets()
	{
		TwoLeft();
		var (_, screen, _) = ParkFootprintPictureTests.BuyScreen();
		var list = ListOf( screen );
		SetBalance( 50000 );
		list.Refresh();

		Assert.AreEqual( "1170 1112", string.Join( ' ', list.Rows.Take( 2 ).Select( row => row.Id ) ) );
		Assert.AreEqual( new UiColour( 255, 224, 0 ), CellColour( list, "1 gt" ) );
		Assert.AreEqual( new UiColour( 255, 0, 0 ), CellColour( list, "3 gt" ) );
		Assert.AreEqual( UiColour.White, CellColour( list, "$ 500" ), "the Belly Bounce" );

		SetBalance( 500 );
		list.Refresh();
		Assert.AreEqual( UiColour.White, CellColour( list, "$ 500" ), "a price equal to the bank" );

		SetBalance( 499 );
		list.Refresh();
		Assert.AreEqual( ParkBuyScreen.ShortColour, CellColour( list, "$ 500" ) );
		Assert.AreEqual( ParkBuyScreen.TicketsColour, CellColour( list, "1 gt" ), "the bank not asked" );

		screen.PickTab( ItemDescriptionFile.Feature );
		Assert.AreEqual( new UiColour( 231, 239, 75 ), list.RowColour!( list.Rows.Single( row => row.Id == -1 ) ) );
	}

	/// <summary>
	/// The click (<c>BuyScreen_OnMessage</c>, <c>0x004ac5a9</c>): with an empty bank, the row the tickets buy goes into the
	/// hand and the screen closes, tickets equal to its cost buying it; the one they do not is refused with <c>cat_ui</c>
	/// <c>0x1D</c> and the screen stays.
	/// </summary>
	/// <remarks><b>Mutations:</b> the bank asked of a mystery ride; the refusal silent; the tickets compared strictly.</remarks>
	[TestMethod]
	public void AMysteryRowIsCarriedByTheTicketsAndRefusedWithout()
	{
		var file = TwoLeft();
		var (_, screen, stack) = ParkFootprintPictureTests.BuyScreen();
		var list = ListOf( screen );
		SetBalance( 0 );
		UiSounds.LastAsked = null;

		list.Activated!( 1112 );

		Assert.AreEqual( (0, ParkBuyScreen.RefusedSound), (ParkBuilding.Carrying, UiSounds.LastAsked) );
		Assert.IsTrue( stack.Windows.Contains( screen ), "the screen stays" );

		// One left, Jurassic Tours' cost: equal buys it.
		file.TicketsSpent = 3;
		UiSounds.LastAsked = null;
		list.Activated!( 1170 );

		Assert.AreEqual( (1170, (int?)null), (ParkBuilding.Carrying, UiSounds.LastAsked) );
		Assert.IsFalse( stack.Windows.Contains( screen ), "closed behind it" );
	}

	/// <summary>
	/// The carry's preview never refuses a mystery ride for money (<c>BuildTool_PreviewPass</c>, <c>0x005235e6</c>): at a
	/// cell where the same ride, once bought, is refused because the bank is below its price, the mystery ride passes that
	/// test and goes on to the next.
	/// </summary>
	/// <remarks><b>Mutation:</b> the money test made of a mystery ride too.</remarks>
	[TestMethod]
	public void AMysteryRideIsNotRefusedForMoneyWhereItIsPutDown()
	{
		var file = TwoLeft();
		var (catalogue, _, _) = ParkFootprintPictureTests.BuyScreen();
		var state = Level.Current!.ParkState!;
		SetBalance( -1000 );

		var build = typeof( ParkBuilding ).GetMethods( BindingFlags.NonPublic | BindingFlags.Static )
			.Single( method => method.Name == "Build" && method.GetParameters().Length == 9 );

		string Answer( int x, int y )
		{
			var built = build.Invoke( null, [state, Level.Current.Park, catalogue, null, null, 1170, x, y, 0] )!;
			return (string)built.GetType().GetProperty( "Answer" )!.GetValue( built )!;
		}

		file.RideIds.Add( 1170 );
		var cell = Enumerable.Range( 0, 128 * 128 ).Select( at => (X: at % 128, Y: at / 128) )
			.First( at => Answer( at.X, at.Y ).Contains( "and the park has -1000" ) );

		file.RideIds.Clear();
		var mystery = Answer( cell.X, cell.Y );

		Assert.IsFalse( mystery.Contains( "and the park has" ), mystery );
		Assert.AreEqual( (2, 0), (file.TicketsSpent, state.Balance + 1000), "nothing paid where nothing was built" );
	}
}
