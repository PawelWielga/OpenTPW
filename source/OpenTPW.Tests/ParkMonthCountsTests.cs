using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park analyser's two month counts - <c>docs/exe/hud.md</c>, "The guests inside and the rides at a close": the 10th
/// int ring (<c>+0x205d0</c>), the guests inside the park as a month is closed (<c>FUN_004c7fa0</c>), and the 12th
/// (<c>+0x20a70</c>), the objects of a ride's kind (<c>FUN_004c8070</c>). Reads Lost Kingdom's save and catalogue, and
/// is skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkMonthCountsTests
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

	private const int BellyBounce = 13, JungleSpray = 14, CrazyApe = 1101;

	/// <summary>
	/// <b>A ride's kind is counted, whatever its state</b>: the shipped park's one, the Belly Bounce, and two with a feature made a Crazy Ape (the Jungle Spray is a
	/// sideshow, the Drinks Shop a shop, the rest features and fixed items); still two with the Bounce condemned and shut; nought with no
	/// catalogue to name the items.
	/// </summary>
	/// <remarks><b>Mutations:</b> the kind tested against a shop's or a sideshow's; a condemned or shut ride left out; every object counted.</remarks>
	[TestMethod]
	public void TheRidesStandingAreTheObjectsOfARidesKind()
	{
		var state = new ParkState( new ParkWorld( Body() ) );
		var catalogue = Catalogue();
		var bounce = state.ObjectsInChainOrder().Single( thing => thing.ThingId == BellyBounce );

		Assert.AreEqual( 1, state.RidesStanding( catalogue ) );

		var feature = state.ObjectsInChainOrder().First( thing => catalogue.TryGet( thing.CatalogueId, out var item ) && item.UiType == ItemDescriptionFile.Feature );
		state.ReplaceObject( feature with { CatalogueId = CrazyApe } );
		Assert.AreEqual( 2, state.RidesStanding( catalogue ), "a feature made a Crazy Ape: two rides, one shop, one sideshow" );

		state.ReplaceObject( bounce with { State = ParkValue.Condemned, CanLoad = 0 } );
		Assert.AreEqual( 2, state.RidesStanding( catalogue ), "condemned and shut, still standing" );
		Assert.IsTrue( state.ObjectsInChainOrder().Any( thing => thing.ThingId == JungleSpray ), "the sideshow stands, not counted" );
		Assert.AreEqual( 0, state.RidesStanding( null ) );
	}

	/// <summary>
	/// <b>The close samples the guests inside into the 10th ring and the rides into the 12th</b>, after the money rings,
	/// and the writer is handed both: the shipped file's [0] and [1] on slot 0; one guest's cell made a path, one close
	/// on, slot 1 holds the census's two guests on that cell, not all thirteen, and the one ride; their month so far untouched. A park made with no
	/// file starts both empty.
	/// </summary>
	/// <remarks><b>Mutations:</b> the census not passed, or every guest passed; either ring not pushed, or each pushed the other's count; the file's rings not read; the writer handed another ring.</remarks>
	[TestMethod]
	public void TheMonthsCloseSamplesTheGuestsInsideAndTheRides()
	{
		var fresh = new ParkState( parkIsClosed: false, visitorsToDate: 0, balance: 0 );

		Assert.AreEqual( (-1, 0, -1, 0), (fresh.MonthGuestsInside.Current, fresh.MonthGuestsInside.Newest, fresh.MonthRides.Current, fresh.MonthRides.Newest), "no file" );

		var world = new ParkWorld( Body() );
		var state = new ParkState( world );
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );
		var catalogue = Catalogue();

		Assert.AreEqual( (0, 0, 0, 1), (state.MonthGuestsInside.Current, state.MonthGuestsInside.Newest, state.MonthRides.Current, state.MonthRides.Newest), "the file's" );

		Assert.AreEqual( (0, 13), (people.GateGuestCensus, people.Peeps.Count), "the shipped park's thirteen guests, none on a counting cell" );

		var (x, y) = people.Peeps[0].Navigator.Position.Cell;
		state.SetRecord( x, y, state.Record( x, y ) with { Type = 1 } );
		var inside = people.GateGuestCensus;

		Assert.AreEqual( 2, inside, "the first guest's cell made a path: two guests stand on it" );

		ParkPeople.TurnTheMonth( people, state, catalogue );

		Assert.AreEqual( (1, inside, 0), (state.MonthGuestsInside.Current, state.MonthGuestsInside.Newest, state.MonthGuestsInside.Total) );
		Assert.AreEqual( (1, 1, 0), (state.MonthRides.Current, state.MonthRides.Newest, state.MonthRides.Total) );
		Assert.AreEqual( state.MonthCosts.Current, state.MonthRides.Current, "on the money rings' slot" );
		Assert.AreSame( state.MonthGuestsInside, state.MonthRings.GuestsInside, "the writer handed the 10th as itself" );
		Assert.AreSame( state.MonthRides, state.MonthRings.Rides, "and the 12th" );
	}

	/// <summary>
	/// <b>The 10th and 12th rings read and write back</b>: thing 5's record at 62,598 + 9 x 589 and + 11 x 589, one month
	/// closed on 0 and on 1. Written back changed, each reads back as written, at its own place and nowhere else.
	/// </summary>
	/// <remarks><b>Mutations:</b> either ring read or written as its neighbour; the two swapped; the writer not handed them.</remarks>
	[TestMethod]
	public void TheShippedParksGuestsInsideAndRidesRingsReadAndWriteBack()
	{
		var shipped = new ParkWorld( Body() );
		var inside = shipped.MonthGuestsInside!.Copy();
		var rides = shipped.MonthRides!.Copy();

		Assert.AreEqual( (0, 0, 0), (inside.Current, inside.Newest, inside.Total) );
		Assert.AreEqual( (0, 1, 0), (rides.Current, rides.Newest, rides.Total) );

		inside.Push( 1234 );
		rides.Push( 56 );

		var same = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( shipped.MonthBalance!, shipped.MonthParkValue!, shipped.MonthCosts!, shipped.MonthStaffCosts!, shipped.MonthTraining!,
				shipped.MonthCashIn!, shipped.MonthGateTakings!, shipped.MonthShopTakings!, shipped.MonthSideshowTakings!, inside,
				shipped.MonthVisitors!, rides, shipped.MonthArrivals!, shipped.MonthHappiness!, shipped.MonthMeanStay!, shipped.LeaverStays!, shipped.MonthParkScore! ) ) );
		var written = new ParkWorld( same );

		Assert.AreEqual( (1, 1234), (written.MonthGuestsInside!.Current, written.MonthGuestsInside.Newest) );
		Assert.AreEqual( (1, 56), (written.MonthRides!.Current, written.MonthRides.Newest) );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, same.Length ).Where( at => same[at] != original[at] ).ToArray();
		var tenth = shipped.AnalyserAt + 62598 + 9 * ParkMonthRing.Size;
		var twelfth = shipped.AnalyserAt + 62598 + 11 * ParkMonthRing.Size;

		Assert.IsTrue( differ.Length > 0 && differ.All( at => at >= tenth && at < tenth + ParkMonthRing.Size
			|| at >= twelfth && at < twelfth + ParkMonthRing.Size ), "the 10th and 12th rings alone" );
		Assert.AreEqual( 1234, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( same.AsSpan( tenth + 13 + 4 ) ), "the count at the 10th ring's slot 1" );
		Assert.AreEqual( 56, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( same.AsSpan( twelfth + 13 + 4 ) ), "and the 12th's" );
	}
}
