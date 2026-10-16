using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park analyser's 14th ring - <c>docs/exe/hud.md</c>, "The guests' happiness at a close": its one ring of floats
/// (<c>+0x20f10</c>), the mean happiness of the guests inside the park as a month is closed (<c>FUN_004c7bb0</c>). Reads
/// Lost Kingdom's save and catalogue, and is skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkHappinessRingTests
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

	/// <summary>
	/// <b>The mean is of the guests inside, each one's happiness truncated to its low byte, summed whole</b>: every guest's
	/// cell made a path but one guest's, alone on hers; 70.9 counts 70, 600 counts 88, -1.25 counts 255 and the nine others
	/// 50, so 863 over 12, where the one outside is 200 and not counted. The close pushes it as a float onto the money rings'
	/// slot, after the rides, its month so far left at nought; nought with the park shut, and with nobody inside.
	/// </summary>
	/// <remarks><b>Mutations:</b> the happiness summed as a float, or not cut to its low byte, or rounded; every guest counted; the shut park not tested; the mean not pushed, or pushed as an int; the ring closed onto another slot; the writer handed another ring.</remarks>
	[TestMethod]
	public void TheCloseSamplesTheMeanHappinessOfTheGuestsInside()
	{
		var world = new ParkWorld( Body() );
		var state = new ParkState( world );
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );

		Assert.AreEqual( (0, 0f, 0), (state.MonthHappiness.Current, state.MonthHappiness.NewestSingle, state.MonthHappiness.Total), "the file's" );
		Assert.AreEqual( (0, 0f), (people.GateGuestCensus, people.GuestsInsideHappiness), "nobody inside: nought" );

		var guests = people.Peeps.ToList();
		var cells = guests.Select( peep => peep.Navigator.Position.Cell ).ToList();
		var outside = guests.First( peep => cells.Count( cell => cell == peep.Navigator.Position.Cell ) == 1 );

		foreach ( var (x, y) in cells.Distinct().Where( cell => cell != outside.Navigator.Position.Cell ) )
			state.SetRecord( x, y, state.Record( x, y ) with { Type = 1 } );

		var inside = guests.Where( peep => peep != outside ).ToList();
		Assert.AreEqual( 13, guests.Count, "the shipped park's thirteen guests" );
		Assert.AreEqual( 12, people.GateGuestCensus, "all but the one alone on her cell" );

		foreach ( var peep in inside )
			peep.Happiness = 50f;

		inside[0].Happiness = 70.9f;
		inside[1].Happiness = 600f;
		inside[2].Happiness = -1.25f;
		outside.Happiness = 200f;

		var mean = (float)((70 + 88 + 255 + 9 * 50) / 12.0);
		Assert.AreEqual( mean, people.GuestsInsideHappiness, "863 over 12" );

		ParkPeople.TurnTheMonth( people, state, null );

		Assert.AreEqual( (1, mean, 0), (state.MonthHappiness.Current, state.MonthHappiness.NewestSingle, state.MonthHappiness.Total) );
		Assert.AreEqual( state.MonthCosts.Current, state.MonthHappiness.Current, "on the money rings' slot" );
		Assert.AreSame( state.MonthHappiness, state.MonthRings.Happiness, "the writer handed the 14th as itself" );

		state.SetParkClosed( true );
		Assert.AreEqual( (12, 0f), (people.GateGuestCensus, people.GuestsInsideHappiness), "the park shut: nought, the guests still inside" );

		ParkPeople.TurnTheMonth( people, state, null );
		Assert.AreEqual( (2, 0f), (state.MonthHappiness.Current, state.MonthHappiness.NewestSingle) );
	}

	/// <summary>A park made with no file starts the ring empty.</summary>
	[TestMethod]
	public void AParkMadeWithNoFileHasNoHappinessRing()
	{
		var fresh = new ParkState( parkIsClosed: false, visitorsToDate: 0, balance: 0 );

		Assert.AreEqual( (-1, 0f, 0), (fresh.MonthHappiness.Current, fresh.MonthHappiness.NewestSingle, fresh.MonthHappiness.Total) );
	}

	/// <summary>
	/// <b>The 14th ring reads and writes back as floats</b>: thing 5's record at 62,598 + 13 x 589, one month closed on
	/// 0.0. Written back with a month of 66.75 closed, it reads back as written, its bits a float's, at its own place and
	/// nowhere else.
	/// </summary>
	/// <remarks><b>Mutations:</b> the ring read or written as its neighbour, the 13th or the 15th; the writer not handed it; the float pushed as an int.</remarks>
	[TestMethod]
	public void TheShippedParksHappinessRingReadsAndWritesBack()
	{
		var shipped = new ParkWorld( Body() );
		var happiness = shipped.MonthHappiness!.Copy();

		Assert.AreEqual( (0, 0f, 0), (happiness.Current, happiness.NewestSingle, happiness.Total) );

		happiness.Push( 66.75f );

		var same = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( shipped.MonthBalance!, shipped.MonthParkValue!, shipped.MonthCosts!, shipped.MonthStaffCosts!, shipped.MonthTraining!,
				shipped.MonthCashIn!, shipped.MonthGateTakings!, shipped.MonthShopTakings!, shipped.MonthSideshowTakings!, shipped.MonthGuestsInside!,
				shipped.MonthVisitors!, shipped.MonthRides!, shipped.MonthArrivals!, happiness, shipped.MonthMeanStay!, shipped.LeaverStays! ) ) );
		var written = new ParkWorld( same );

		Assert.AreEqual( (1, 66.75f), (written.MonthHappiness!.Current, written.MonthHappiness.NewestSingle) );

		var fourteenth = shipped.AnalyserAt + 62598 + 13 * ParkMonthRing.Size;
		Assert.AreEqual( 66.75f, System.BitConverter.ToSingle( same, fourteenth + 13 + 4 ), "the second month's bytes a float's" );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, same.Length ).Where( at => same[at] != original[at] ).ToArray();

		Assert.IsTrue( differ.Length > 0 && differ.All( at => at >= fourteenth && at < fourteenth + ParkMonthRing.Size ), "the 14th ring alone" );
	}
}
