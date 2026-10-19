using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The shop's window (<c>FUN_004b0e30</c>, its handler <c>FUN_004b0b30</c>; <c>docs/exe/hud.md</c>, "The shop's window"),
/// on the shipped park's Drinks Shop, thing 16. The window's tests are skipped without the game (<see cref="GameData"/>);
/// the local happiness's and apply to all's are not.
/// </summary>
[TestClass]
public class ParkShopWindowTests
{
	private const int DrinksShop = 16;
	private const int BellyBounce = 13;

	private Level? _level;
	private Point2 _screen;
	private float _now;
	private Vector3 _looksAt;
	private ParkPeople? _people;
	private ParkState _state = null!;
	private WindowStack _stack = null!;
	private Level _park = null!;

	private void LoadThePark()
	{
		Screen.Size = new Point2( 2048, 1536 );
		Log ??= new();
		FileSystem = GameData.Required();
		Input.Mouse = new();
		Unimplemented.Forget();

		using var stream = new MemoryStream( FileSystem.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", FileSystem );
		_state = new ParkState( world );

		_park = (Level)RuntimeHelpers.GetUninitializedObject( typeof( Level ) );
		typeof( Level ).GetProperty( nameof( Level.Park ) )!.SetValue( _park, world );
		typeof( Level ).GetProperty( nameof( Level.ParkState ) )!.SetValue( _park, _state );
		typeof( Level ).GetProperty( nameof( Level.Catalogue ) )!.SetValue( _park, catalogue );
		typeof( Level ).GetProperty( nameof( Level.Kind ) )!.SetValue( _park, Level.Scene.Park );
		Level.Current = _park;

		_people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, _state, catalogue );
		_stack = new WindowStack();
		typeof( Level ).GetField( "_windows", BindingFlags.NonPublic | BindingFlags.Instance )!.SetValue( _park, _stack );
	}

	[TestInitialize]
	public void KeepWhatTheTestsChange()
	{
		_level = Level.Current;
		_screen = Screen.Size;
		_now = Time.Now;
		_looksAt = ParkOrbitCameraMode.PointOfInterest;
	}

	[TestCleanup]
	public void PutItAllBack()
	{
		_people?.Delete();
		Entity.ApplyDeletions();
		Level.Current = _level!;
		Screen.Size = _screen;
		Time.Now = _now;
		Input.Mouse = new();
		ParkOrbitCameraMode.PointOfInterest = _looksAt;
		Unimplemented.Forget();
	}

	private ParkShopWindow? Window => _stack.Windows.OfType<ParkShopWindow>().FirstOrDefault();

	private ParkWorld.CatalogueObject Shop => _state.Objects.Single( placed => placed.ThingId == DrinksShop );

	private static int Times( string what ) => Unimplemented.Summary.FirstOrDefault( gap => gap.What == what ).Times;

	private void Click( UiControl root, int id )
	{
		var control = Find( root, id );
		Assert.IsTrue( _stack.ClickAt( (control.Rect.Left + control.Rect.Right) / 2, (control.Rect.Top + control.Rect.Bottom) / 2 ), $"{id:x} clicked" );
	}

	private ParkShopWindow Open()
	{
		_park.OpenObjectWindow( DrinksShop );
		Assert.IsNotNull( Window, "the shop's click opens the shop's window" );
		return Window!;
	}

	/// <summary>
	/// <b>A shop's click opens its window</b> (<c>FUN_00486920</c>'s arm 1), as the original's on the same shop
	/// (<c>q31/orig/b-1-window.png</c>): titled "Drinks Shop", the labels UITEXT 32 to 38, "0 of 0", "$ 20", "$ 0", "$ 0",
	/// satisfaction 50 as a bar of 512, the price box "$ 30", the quality's middle icon lit, "Amount of ice", and with
	/// one shop no arrows; nothing counted as <c>SHOP_WINDOW</c>.
	/// </summary>
	/// <remarks><b>Mutations:</b> the arm 1 route gone; a label's row; C and M swapped in UITEXT 460; the quality's
	/// divide by 50; the ingredient's row less one; the arrows shown under two shops.</remarks>
	[TestMethod]
	public void AShopsClickOpensItsWindow()
	{
		LoadThePark();
		var window = Open();

		Assert.AreEqual( 0, Times( "SHOP_WINDOW" ) );
		Assert.AreEqual( "Drinks Shop", Find( window.Root, 0xc06e ).Text );

		CollectionAssert.AreEqual(
			new[] { "Customers last month", "Cost of goods", "Takings last month", "Profit last month", "Customer satisfaction", "Scrap value", "Local happiness" },
			new[] { 0xc075, 0xc071, 0xc072, 0xc073, 0xc074, 0xc07a, 0xc07b }.Select( id => Find( window.Root, id ).Text ).ToArray() );

		CollectionAssert.AreEqual( new[] { "0 of 0", "$ 20", "$ 0", "$ 0", "$ 650" },
			new[] { 0xc070, 0xc076, 0xc077, 0xc078, 0xc07d }.Select( id => Find( window.Root, id ).Text ).ToArray() );

		Assert.AreEqual( 512, ((ParkObjectWindow.UiStatBar)Find( window.Root, 0xc079 )).Units, "satisfaction 50 with no day" );
		Assert.AreEqual( "$ 30", Find( window.Root, 0xc085 ).Text );
		Assert.AreEqual( "Amount of ice", Find( window.Root, 0xc084 ).Text );
		Assert.AreEqual( ("Quality of goods", "Sale price"), (Find( window.Root, 0xc07e ).Text, Find( window.Root, 0xc083 ).Text) );
		StringAssert.Contains( window.Census(), "quality 1 (icon 1) amount 50" );
		Assert.IsFalse( Find( window.Root, 0xc069 ).Visible || Find( window.Root, 0xc06a ).Visible, "one shop: no arrows" );
	}

	/// <summary>
	/// <b>The month's figures are the finished days'</b> (<c>FUN_004b01d0</c>): a day of three customers and two walk-aways,
	/// 90 taken and 60 spent, satisfaction 30, then a customer today: "3 of 5" (C the customers, M those and the
	/// walk-aways), "$ 90", "$ 30", and the shipped ring's thirty days with this one's 30 averaging 1, so 51 as
	/// (51 &lt;&lt; 10) / 100 = 522.
	/// </summary>
	/// <remarks><b>Mutations:</b> C and M swapped; M without the walk-aways; today's customer counted; profit as takings.</remarks>
	[TestMethod]
	public void TheMonthsFiguresAreTheFinishedDays()
	{
		LoadThePark();

		var rings = _state.RingsFor( DrinksShop );

		for ( var i = 0; i < 3; ++i )
			rings.CountCustomer();

		rings.CountWalkAway();
		rings.CountWalkAway();
		rings.Takings.Today = 90;
		rings.Costs.Today = 60;
		rings.Satisfy( 30 );
		rings.Roll();
		rings.CountCustomer();

		var window = Open();

		CollectionAssert.AreEqual( new[] { "3 of 5", "$ 90", "$ 30" },
			new[] { 0xc070, 0xc077, 0xc078 }.Select( id => Find( window.Root, id ).Text ).ToArray() );
		Assert.AreEqual( 522, ((ParkObjectWindow.UiStatBar)Find( window.Root, 0xc079 )).Units );
	}

	/// <summary>
	/// <b>The settings are buffered and written on the close</b> (<c>FUN_004b0aa0</c> from <c>0x0048d200</c>'s message
	/// <c>0x14</c>), as in the original (<c>q31/orig</c> rounds b and c): the quality to the right and the amount to the
	/// left show a cost of goods of "$ 30" at once and leave the shop as it was; five clicks on plus make "$ 35"; the
	/// close writes quality 100, amount 0 and price 35.
	/// </summary>
	/// <remarks><b>Mutations:</b> the apply on the close gone; the quality written as its index; the cost of goods read
	/// from the shop instead of the window; the price's step.</remarks>
	[TestMethod]
	public void TheSettingsAreBufferedAndWrittenOnTheClose()
	{
		LoadThePark();
		var window = Open();

		var quality = (UiSlider)Find( window.Root, 0xc06b );
		var amount = (UiSlider)Find( window.Root, 0xc086 );

		quality.SetValue( 2 );
		quality.Moved!();
		amount.SetValue( 0 );
		amount.Moved!();

		for ( var i = 0; i < 5; ++i )
			ClickPlus( window );

		Assert.AreEqual( "$ 30", Find( window.Root, 0xc076 ).Text, "ice at quality 100 and amount 0: 20 x 1.5" );
		Assert.AreEqual( "$ 35", Find( window.Root, 0xc085 ).Text );
		StringAssert.Contains( window.Census(), "quality 2 (icon 2)" );
		Assert.AreEqual( (50, 50, 30), (Shop.QualityOfGoods, Shop.AmountOfSpecialIngredient, Shop.PricePerUse), "buffered" );

		_stack.Close( window );

		Assert.AreEqual( (100, 0, 35), (Shop.QualityOfGoods, Shop.AmountOfSpecialIngredient, Shop.PricePerUse), "written on the close" );

		window = Open();
		StringAssert.Contains( window.Census(), "quality 2 (icon 2) amount 0" );
		Assert.AreEqual( "$ 35", Find( window.Root, 0xc085 ).Text );
	}

	/// <summary>The spinner's plus, its op 0xe child, clicked by its place: it has no id of its own.</summary>
	private void ClickPlus( ParkShopWindow window )
	{
		var plus = Find( window.Root, 0xc085 ).Children[1];
		Assert.IsTrue( _stack.ClickAt( (plus.Rect.Left + plus.Rect.Right) / 2, (plus.Rect.Top + plus.Rect.Bottom) / 2 ), "plus clicked" );
	}

	/// <summary>
	/// <b>The door shuts and opens the shop</b> (<c>FUN_0048ccf0</c>), the preview's box lettered UITEXT 365 while it is
	/// shut, as in the original (<c>q31/orig/c-3-door.png</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the door's state inverted; the status box not refreshed.</remarks>
	[TestMethod]
	public void TheDoorShutsAndOpensTheShop()
	{
		LoadThePark();
		var window = Open();

		Click( window.Root, 0xc082 );
		Assert.AreEqual( 0, Shop.CanLoad, "shut" );
		Assert.IsTrue( Find( window.Root, 0xc06d ).Visible );
		Assert.AreEqual( Localization.Text( 365 ), Find( window.Root, 0xc06d ).Text );

		Click( window.Root, 0xc082 );
		Assert.AreNotEqual( 0, Shop.CanLoad, "open again" );
		Assert.IsFalse( Find( window.Root, 0xc06d ).Visible );
	}

	/// <summary><b>b_allthings opens the all-items list on its shops tab with this shop's row picked</b> (<c>FUN_00495aa0( 1, shop )</c>).</summary>
	[TestMethod]
	public void TheListButtonOpensTheShopsTabOnThisShop()
	{
		LoadThePark();

		// A Balloon Shop, whose row sorts before the Drinks Shop's, so the pick is not the first row's.
		_state.AddObject( new ParkWorld.CatalogueObject( 60, 1209, 30 << 8, 30 << 8, 0 ) );

		var window = Open();

		Click( window.Root, 0xc067 );

		var list = _stack.Windows.OfType<ParkItemsScreen>().SingleOrDefault();
		Assert.IsNotNull( list );
		Assert.AreEqual( (1, DrinksShop, (ParkShopWindow?)null), (list!.ShownTab, list.Picked, Window) );
	}

	/// <summary><c>FUN_004e2200</c>: <c>SpecialIngredient</c> 1 to 4 are rows 0 to 3 of <c>INGREDIENT.str</c>, any other none.</summary>
	[TestMethod]
	public void TheIngredientsAreTheirTablesRows()
	{
		FileSystem = GameData.Required();

		CollectionAssert.AreEqual( new[] { "", "Amount of fat", "Amount of salt", "Amount of ice", "Amount of sugar", "" },
			Enumerable.Range( 0, 6 ).Select( ParkShopWindow.IngredientName ).ToArray() );
	}

	/// <summary>
	/// <b>The local happiness</b> (<c>FUN_004cd880( cell, 1 )</c>): the guests on the nine cells round the shop's, each
	/// one's happiness truncated to its low byte, averaged; a guest two cells off and anyone not a guest are not counted;
	/// at x 0 the cell west of the shop's is the row above's last, as the packed id wraps.
	/// </summary>
	/// <remarks><b>Mutations:</b> a radius of 2; the low byte dropped; a member counted; the wrap clamped.</remarks>
	[TestMethod]
	public void TheLocalHappinessIsTheNineCellsGuests()
	{
		var state = new ParkState( parkIsClosed: false, visitorsToDate: 0 );
		var happiness = new Dictionary<int, float> { [1] = 80.7f, [2] = 61f, [3] = 300f, [4] = 99f, [6] = 20f };

		state.StandOn( 1, 10, 10 );
		state.StandOn( 2, 11, 11 );
		state.StandOn( 3, 9, 11 );
		state.StandOn( 4, 12, 10 );
		state.StandOn( 5, 10, 9 );

		float? Of( int thing ) => happiness.TryGetValue( thing, out var h ) ? h : null;

		Assert.AreEqual( (80 + 61 + 44) / 3, ParkShopWindow.LocalHappiness( state, 10, 10, Of ), "300 is 44 in a byte; thing 4 two off, 5 no guest" );
		Assert.AreEqual( 0, ParkShopWindow.LocalHappiness( state, 30, 30, Of ), "nobody" );

		state.StandOn( 6, 127, 4 );
		Assert.AreEqual( 20, ParkShopWindow.LocalHappiness( state, 0, 5, Of ), "(-1, 5) is (127, 4)" );
	}

	/// <summary>
	/// <b>Apply to all reaches the shops of the shown one's item</b> (<c>0x00482040</c> on mask <c>0x200</c>, then
	/// <c>0x004b0ae0</c>'s item test), not every shop; and the apply's setters write the quality index times 50.
	/// </summary>
	[TestMethod]
	public void ApplyToAllReachesTheSameItemsShops()
	{
		var catalogue = new ParkItemCatalogue( "jungle", GameData.Required() );
		var unplaced = ParkWorld.CatalogueObject.Unplaced;

		ParkWorld.CatalogueObject[] objects =
		[
			new( DrinksShop, 1203, 43 << 8, 31 << 8, 0 ),
			new( 40, 1203, 50 << 8, 20 << 8, 0 ),
			new( 41, 1206, 52 << 8, 20 << 8, 0 ),
			new( BellyBounce, 1100, 52 << 8, 24 << 8, 0 ),
			new( 42, 1203, unplaced, unplaced, 0 ),
		];

		CollectionAssert.AreEqual( new[] { DrinksShop, 40 }, ParkShopWindow.ShopsOfItem( objects, catalogue, 1203 ) );

		var applied = ParkShopWindow.Applied( objects[0], quality: 1, amount: 7, price: 45 );
		Assert.AreEqual( (50, 7, 45), (applied.QualityOfGoods, applied.AmountOfSpecialIngredient, applied.PricePerUse) );
	}

	private static UiControl Find( UiControl root, int id )
		=> FindOrNull( root, id ) ?? throw new AssertFailedException( $"no control {id:x}" );

	private static UiControl? FindOrNull( UiControl root, int id )
		=> root.Id == id ? root : root.Children.Select( child => FindOrNull( child, id ) ).FirstOrDefault( found => found != null );
}
