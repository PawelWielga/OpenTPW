using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park's score out of a hundred (<c>FUN_004cceb0</c>) and the analyser's 16th ring (<c>+0x213b0</c>) it is pushed
/// into at each close - <c>docs/exe/hud.md</c>, "The park score". Reads Lost Kingdom's save and catalogue, and is skipped
/// where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkScoreRingTests
{
	private BaseFileSystem? data;

	private BaseFileSystem Data() => data ??= FileSystem = GameData.Required();

	[TestCleanup]
	public void LetThePeopleGo() => TestRun.DeleteEvery<ParkPeople>();

	private byte[] Body()
	{
		using var stream = new MemoryStream( Data().ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new SaveReader( stream ).ReadFile();
	}

	private ParkItemCatalogue Catalogue() => new( "jungle", Data() );

	private const int BellyBounce = 13;

	private static readonly ParkScore.StaffCounts NoStaff = default;

	/// <summary>
	/// <b>Each count is capped and divided whole, as <c>FUN_004cceb0</c> takes it</b>: 49 guests are nought points and 50
	/// one, 999 nineteen and 2,000 twenty; 1 ride one, 3 four, 13 nineteen and 14 twenty; 4 shops eight and 6 ten, and the
	/// sideshows the same; 9 features nine and 12 ten; 7 upgraded rides seven and 11 ten; each kind of staff its count to
	/// four. Every term at its top is a hundred.
	/// </summary>
	/// <remarks><b>Mutations:</b> any cap moved or dropped; the guests not capped at a thousand before the product; the rides rounded; a term weighed as another's; a kind of staff left out.</remarks>
	[TestMethod]
	public void EachCountIsCappedAndDividedWhole()
	{
		int Guests( int n ) => ParkScore.From( n, 0, 0, 0, 0, 0, NoStaff );
		int Rides( int n ) => ParkScore.From( 0, n, 0, 0, 0, 0, NoStaff );

		CollectionAssert.AreEqual( new[] { 0, 1, 19, 20, 20 }, new[] { 49, 50, 999, 1000, 2000 }.Select( Guests ).ToArray(), "the guests" );
		CollectionAssert.AreEqual( new[] { 1, 4, 19, 20, 20 }, new[] { 1, 3, 13, 14, 40 }.Select( Rides ).ToArray(), "the rides" );
		Assert.AreEqual( (8, 10), (ParkScore.From( 0, 0, 4, 0, 0, 0, NoStaff ), ParkScore.From( 0, 0, 6, 0, 0, 0, NoStaff )), "the shops" );
		Assert.AreEqual( (6, 10), (ParkScore.From( 0, 0, 0, 3, 0, 0, NoStaff ), ParkScore.From( 0, 0, 0, 7, 0, 0, NoStaff )), "the sideshows" );
		Assert.AreEqual( (9, 10), (ParkScore.From( 0, 0, 0, 0, 9, 0, NoStaff ), ParkScore.From( 0, 0, 0, 0, 12, 0, NoStaff )), "the features" );
		Assert.AreEqual( (7, 10), (ParkScore.From( 0, 0, 0, 0, 0, 7, NoStaff ), ParkScore.From( 0, 0, 0, 0, 0, 11, NoStaff )), "the upgraded rides" );
		Assert.AreEqual( 1 + 2 + 3 + 4 + 4, ParkScore.From( 0, 0, 0, 0, 0, 0, new( 1, 2, 3, 4, 9 ) ), "the staff, four of a kind at most" );
		Assert.AreEqual( 4 + 3 + 2 + 1 + 0, ParkScore.From( 0, 0, 0, 0, 0, 0, new( 6, 3, 2, 1, 0 ) ), "each kind its own" );
		Assert.AreEqual( 100, ParkScore.From( 5000, 99, 99, 99, 99, 99, new( 9, 9, 9, 9, 9 ) ) );
	}

	/// <summary>
	/// <b>The shipped park scores its file's 18</b>: no guest inside, one ride (1), one shop (2), one sideshow (2), eight
	/// features (8) and one of each kind of staff (5). The Belly Bounce at upgrade level 1 adds nothing; at 2 it adds one.
	/// A ride's level is the only one asked: the Jungle Spray, a sideshow, at 2 adds nothing. No catalogue counts no object.
	/// Five features made Drinks Shops: six shops reach their cap of 10, the one sideshow stays 2 and three features 3, the Bounce still upgraded: 22.
	/// </summary>
	/// <remarks><b>Mutations:</b> the level compared from 1 or 3, or for every kind; shops counted as sideshows; the fixed items (kind 4) counted as features; the catalogue's absence counted.</remarks>
	[TestMethod]
	public void TheShippedParkScoresItsFilesEighteen()
	{
		var world = new ParkWorld( Body() );
		var state = new ParkState( world );
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );
		var catalogue = Catalogue();

		Assert.AreEqual( new ParkScore.StaffCounts( 1, 1, 1, 1, 1 ), people.ScoreStaff );
		Assert.AreEqual( (0, 18), (state.MonthParkScore.Current, state.MonthParkScore.Newest), "the file's" );
		Assert.AreEqual( 18, ParkScore.Of( state, catalogue, 0, people.ScoreStaff ) );
		Assert.AreEqual( 5, ParkScore.Of( state, null, 0, people.ScoreStaff ), "no catalogue, the staff alone" );

		var bounce = state.ObjectsInChainOrder().Single( thing => thing.ThingId == BellyBounce );
		state.ReplaceObject( bounce with { UpgradeLevel = 1 } );
		Assert.AreEqual( 18, ParkScore.Of( state, catalogue, 0, people.ScoreStaff ), "level 1" );
		state.ReplaceObject( bounce with { UpgradeLevel = 2 } );
		Assert.AreEqual( 19, ParkScore.Of( state, catalogue, 0, people.ScoreStaff ), "level 2" );

		var spray = state.ObjectsInChainOrder().Single( thing => catalogue.TryGet( thing.CatalogueId, out var item ) && item.UiType == ItemDescriptionFile.SideShow );
		state.ReplaceObject( spray with { UpgradeLevel = 2 } );
		Assert.AreEqual( 19, ParkScore.Of( state, catalogue, 0, people.ScoreStaff ), "a sideshow's level is not asked" );

		var shop = state.ObjectsInChainOrder().Single( thing => catalogue.TryGet( thing.CatalogueId, out var item ) && item.UiType == ParkRideOperation.ShopUiType );
		foreach ( var feature in state.ObjectsInChainOrder().Where( thing => catalogue.TryGet( thing.CatalogueId, out var item ) && item.UiType == ItemDescriptionFile.Feature ).Take( 5 ).ToArray() )
			state.ReplaceObject( feature with { CatalogueId = shop.CatalogueId } );

		Assert.AreEqual( 1 + 10 + 2 + 3 + 1 + 5, ParkScore.Of( state, catalogue, 0, people.ScoreStaff ), "six shops capped, one sideshow, three features" );
	}

	/// <summary>
	/// <b>The close pushes the score onto the money rings' slot, with the guests inside and the staff it is handed</b>:
	/// two handymen and a mechanic hired make the staff's 5 into 8 (handymen 3, mechanics 2); the first guest's cell made a
	/// path puts two guests inside, under fifty, so nought more; the close pushes 21 into slot 1, its month so far untouched.
	/// </summary>
	/// <remarks><b>Mutations:</b> the score not pushed; the staff not passed, or every kind counted as one; the guests not passed; the ring closed onto another slot; the writer not handed it.</remarks>
	[TestMethod]
	public void TheClosePushesTheScore()
	{
		var world = new ParkWorld( Body() );
		var state = new ParkState( world );
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );
		var catalogue = Catalogue();

		Assert.AreNotEqual( 0, people.Hire( new ParkStaffPool.Candidate( Id: 997, Kind: 0, Name: "Ada Test", Grade: 0, Costume: 0, Wage: 69 ), 47, 21 ) );
		Assert.AreNotEqual( 0, people.Hire( new ParkStaffPool.Candidate( Id: 998, Kind: 0, Name: "Bea Test", Grade: 0, Costume: 0, Wage: 69 ), 47, 21 ) );
		Assert.AreNotEqual( 0, people.Hire( new ParkStaffPool.Candidate( Id: 999, Kind: 1, Name: "Cy Test", Grade: 0, Costume: 0, Wage: 69 ), 47, 21 ) );
		Assert.AreEqual( new ParkScore.StaffCounts( 3, 2, 1, 1, 1 ), people.ScoreStaff );

		var (x, y) = people.Peeps[0].Navigator.Position.Cell;
		state.SetRecord( x, y, state.Record( x, y ) with { Type = 1 } );
		Assert.AreEqual( 2, people.GateGuestCensus );

		ParkPeople.TurnTheMonth( people, state, catalogue );

		Assert.AreEqual( (1, 21, 0), (state.MonthParkScore.Current, state.MonthParkScore.Newest, state.MonthParkScore.Total) );
		Assert.AreEqual( state.MonthCosts.Current, state.MonthParkScore.Current, "on the money rings' slot" );
		Assert.AreEqual( 19, state.MonthParkScore.MeanOfNewest( 2 ), "the file's 18 kept before it: 39 over 2" );
		Assert.AreSame( state.MonthParkScore, state.MonthRings.ParkScore, "the writer handed the 16th as itself" );

		state.TurnTheMonth( null, 1000, 0f, people.ScoreStaff );
		Assert.AreEqual( (2, 28), (state.MonthParkScore.Current, state.MonthParkScore.Newest), "a thousand guests' 20 and the staff's 8, no catalogue to count the objects" );
	}

	/// <summary>A park made with no file starts it empty.</summary>
	[TestMethod]
	public void AParkMadeWithNoFileHasNoScores()
	{
		var fresh = new ParkState( parkIsClosed: false, visitorsToDate: 0, balance: 0 );

		Assert.AreEqual( (-1, 0, 0), (fresh.MonthParkScore.Current, fresh.MonthParkScore.Newest, fresh.MonthParkScore.Total) );
	}

	/// <summary>
	/// <b>The 16th ring reads and writes back at its own place</b>: thing 5's record at 62,598 + 15 x 589, one month closed
	/// on 18. Written back with a month of 31 pushed, it reads back as written, and no other byte moves; a park run from the
	/// file starts on it and closes the next month onto slot 2.
	/// </summary>
	/// <remarks><b>Mutations:</b> read or written as the 15th; the writer not handed it; the running park not given the file's.</remarks>
	[TestMethod]
	public void TheShippedParksScoreReadsAndWritesBack()
	{
		var shipped = new ParkWorld( Body() );
		var score = shipped.MonthParkScore!.Copy();

		Assert.AreEqual( (0, 18, 0, 144), (score.Current, score.Newest, score.Total, score.Length) );

		score.Push( 31 );

		var same = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( shipped.MonthBalance!, shipped.MonthParkValue!, shipped.MonthCosts!, shipped.MonthStaffCosts!, shipped.MonthTraining!,
				shipped.MonthCashIn!, shipped.MonthGateTakings!, shipped.MonthShopTakings!, shipped.MonthSideshowTakings!, shipped.MonthGuestsInside!,
				shipped.MonthVisitors!, shipped.MonthRides!, shipped.MonthArrivals!, shipped.MonthHappiness!, shipped.MonthMeanStay!, shipped.LeaverStays!, score ) ) );
		var written = new ParkWorld( same );

		Assert.AreEqual( (1, 31), (written.MonthParkScore!.Current, written.MonthParkScore.Newest) );
		Assert.AreEqual( (0, 0), (written.MonthMeanStay!.Current, written.MonthMeanStay.Newest), "the 15th left as it was" );

		var loaded = new ParkState( written );
		Assert.AreEqual( (1, 31), (loaded.MonthParkScore.Current, loaded.MonthParkScore.Newest), "a park run from the file starts on its 16th" );
		loaded.TurnTheMonth( null );
		Assert.AreEqual( (2, 0), (loaded.MonthParkScore.Current, loaded.MonthParkScore.Newest), "and closes the next month after it" );

		var sixteenth = shipped.AnalyserAt + 62598 + 15 * ParkMonthRing.Size;
		Assert.AreEqual( (1, 31), (System.BitConverter.ToInt32( same, sixteenth ), System.BitConverter.ToInt32( same, sixteenth + 13 + 4 )) );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, same.Length ).Where( at => same[at] != original[at] ).ToArray();

		Assert.IsTrue( differ.Length > 0 && differ.All( at => at >= sixteenth && at < sixteenth + ParkMonthRing.Size ), "the 16th ring alone" );
	}
}
