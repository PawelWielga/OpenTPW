using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park analyser's ring of the last fifty leavers' stays (<c>+0x21600</c>) and its 15th ring (<c>+0x21160</c>), the
/// mean of those at each close - <c>docs/exe/hud.md</c>, "The leavers' stays". Reads Lost Kingdom's save, and is skipped
/// where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkStayRingTests
{
	private BaseFileSystem? data;

	private BaseFileSystem Data() => data ??= FileSystem = GameData.Required();

	private byte[] Body()
	{
		using var stream = new MemoryStream( Data().ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new SaveReader( stream ).ReadFile();
	}

	/// <summary>
	/// <b>A stay is shifted back into sweeps and turned into whole park days</b>, 3,750 seconds a sweep at the shipped rate:
	/// 11 fours of sweeps are 44 sweeps, 1.9 days, so 1 (47 sweeps would be 2); 12 are 2.08, so 2. A guest stamped after
	/// the clock (a stay of 2<sup>30</sup> less one) wraps the 64-bit product to a negative day count. The shift is the
	/// original's <c>SHL</c> in a 32-bit register (<c>0x0050234a</c>): a stay of 2<sup>30</sup> and six loses its top bits to
	/// 24 sweeps, one day.
	/// </summary>
	/// <remarks><b>Mutations:</b> the shift left left out, or taken in 64 bits; the rate's quarter dropped; a day of another length; the product not wrapped or divided unsigned.</remarks>
	[TestMethod]
	public void AStayIsWholeParkDays()
	{
		Assert.AreEqual( GameCalendar.DefaultRate, GameCalendar.Rate );
		CollectionAssert.AreEqual( new[] { 0, 0, 1, 2, 4, 5, 3902581, 7805162, -9642654, -5740073, 1 },
			new uint[] { 0, 5, 11, 12, 24, 29, 0x10000000, 0x20000000, 0x30000000, 0x3fffffff, 0x40000006 }.Select( ParkState.StayDays ).ToArray() );
	}

	/// <summary>
	/// <b>The mean is of the newest fifty at most, truncated toward nought</b>: 11 over 3 is 3; -11 over 3 is -3; fifty tens
	/// pushed over a thousand leave the thousand out once the ring has gone round; an empty ring is nought.
	/// </summary>
	/// <remarks><b>Mutations:</b> the mean rounded or floored; every slot counted, not the filled ones; the oldest taken, not the newest; nought for none left out.</remarks>
	[TestMethod]
	public void TheMeanIsOfTheNewestFiftyTruncated()
	{
		var ring = new ParkMonthRing( ParkMonthRing.Stays );
		Assert.AreEqual( (-1, 50, 0), (ring.Current, ring.Length, ring.MeanOfNewest( 50 )) );

		foreach ( var days in new[] { 2, 4, 5 } )
			ring.Push( days );

		Assert.AreEqual( 3, ring.MeanOfNewest( 50 ), "11 over 3" );
		Assert.AreEqual( 4, ring.MeanOfNewest( 2 ), "the newest two, 9 over 2" );

		var negative = new ParkMonthRing( ParkMonthRing.Stays );
		foreach ( var days in new[] { -2, -4, -5 } )
			negative.Push( days );

		Assert.AreEqual( -3, negative.MeanOfNewest( 50 ), "toward nought" );

		var round = new ParkMonthRing( ParkMonthRing.Stays );
		round.Push( 1000 );

		for ( var i = 0; i < 49; ++i )
			round.Push( 10 );

		Assert.AreEqual( (49, false, 29), (round.Current, round.Wrapped, round.MeanOfNewest( 50 )), "fifty: 1490 over 50" );

		round.Push( 13 );
		Assert.AreEqual( (0, true, 10), (round.Current, round.Wrapped, round.MeanOfNewest( 50 )), "the thousand gone: 503 over 50" );
	}

	/// <summary>
	/// <b>The close pushes the mean of the leavers' stays onto the money rings' slot, and leaves the stays alone</b>: three
	/// leavers of 2, 4 and 5 days close as 3; a second close with nobody gone since closes as 3 again.
	/// </summary>
	/// <remarks><b>Mutations:</b> the mean not pushed; the stays emptied at the close; the ring closed onto another slot; the writer not handed them.</remarks>
	[TestMethod]
	public void TheCloseSamplesTheMeanStay()
	{
		var state = new ParkState( new ParkWorld( Body() ) );

		Assert.AreEqual( (0, 0, 0), (state.MonthMeanStay.Current, state.MonthMeanStay.Newest, state.MonthMeanStay.Total), "the file's" );
		Assert.AreEqual( (-1, 50), (state.LeaverStays.Current, state.LeaverStays.Length), "the file's, nobody gone" );

		foreach ( var stay in new uint[] { 12, 24, 29 } )
			state.AddLeaverStay( stay );

		Assert.AreEqual( (2, 5), (state.LeaverStays.Current, state.LeaverStays.Newest) );

		state.TurnTheMonth( null );
		Assert.AreEqual( (1, 3, 0), (state.MonthMeanStay.Current, state.MonthMeanStay.Newest, state.MonthMeanStay.Total) );
		Assert.AreEqual( state.MonthCosts.Current, state.MonthMeanStay.Current, "on the money rings' slot" );

		state.TurnTheMonth( null );
		Assert.AreEqual( (2, 3, 2), (state.MonthMeanStay.Current, state.MonthMeanStay.Newest, state.LeaverStays.Current), "the stays kept" );
		Assert.AreSame( state.MonthMeanStay, state.MonthRings.MeanStay );
		Assert.AreSame( state.LeaverStays, state.MonthRings.LeaverStays );
	}

	/// <summary>A park made with no file starts both empty.</summary>
	[TestMethod]
	public void AParkMadeWithNoFileHasNoStays()
	{
		var fresh = new ParkState( parkIsClosed: false, visitorsToDate: 0, balance: 0 );

		Assert.AreEqual( (-1, 0, -1, 50), (fresh.MonthMeanStay.Current, fresh.MonthMeanStay.Newest, fresh.LeaverStays.Current, fresh.LeaverStays.Length) );
	}

	/// <summary>
	/// <b>Both read and write back at their own places</b>: the 15th ring at 62,598 + 14 x 589, the stays straight after the
	/// sixteen at 72,022, 213 bytes, <c>mNumEntries</c> 50. Written back with two stays and a month of 7 closed, they read
	/// back as written, and no other byte moves.
	/// </summary>
	/// <remarks><b>Mutations:</b> either read or written as its neighbour; the stays read at 144; the writer not handed them; the running park not given the file's stays.</remarks>
	[TestMethod]
	public void TheShippedParksStaysReadAndWriteBack()
	{
		var shipped = new ParkWorld( Body() );
		var mean = shipped.MonthMeanStay!.Copy();
		var stays = shipped.LeaverStays!.Copy();

		Assert.AreEqual( (0, 0, 0), (mean.Current, mean.Newest, mean.Total) );
		Assert.AreEqual( (-1, 50, false), (stays.Current, stays.Length, stays.Wrapped) );

		stays.Push( 6 );
		stays.Push( 9 );
		mean.Push( 7 );

		var same = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( shipped.MonthBalance!, shipped.MonthParkValue!, shipped.MonthCosts!, shipped.MonthStaffCosts!, shipped.MonthTraining!,
				shipped.MonthCashIn!, shipped.MonthGateTakings!, shipped.MonthShopTakings!, shipped.MonthSideshowTakings!, shipped.MonthGuestsInside!,
				shipped.MonthVisitors!, shipped.MonthRides!, shipped.MonthArrivals!, shipped.MonthHappiness!, mean, stays ) ) );
		var written = new ParkWorld( same );

		Assert.AreEqual( (1, 7), (written.MonthMeanStay!.Current, written.MonthMeanStay.Newest) );
		Assert.AreEqual( (1, 9, 7), (written.LeaverStays!.Current, written.LeaverStays.Newest, written.LeaverStays.MeanOfNewest( 50 )) );

		var loaded = new ParkState( written );
		Assert.AreEqual( (1, 9, 1, 7), (loaded.LeaverStays.Current, loaded.LeaverStays.Newest, loaded.MonthMeanStay.Current, loaded.MonthMeanStay.Newest),
			"a park run from the file starts on its stays and its 15th" );
		loaded.TurnTheMonth( null );
		Assert.AreEqual( (2, 7), (loaded.MonthMeanStay.Current, loaded.MonthMeanStay.Newest), "and closes on the file's stays: 15 over 2" );

		var fifteenth = shipped.AnalyserAt + 62598 + 14 * ParkMonthRing.Size;
		var history = shipped.AnalyserAt + 72022;
		Assert.AreEqual( 50, System.BitConverter.ToInt32( same, history + 4 ), "the stays' mNumEntries" );
		Assert.AreEqual( (6, 9), (System.BitConverter.ToInt32( same, history + 13 ), System.BitConverter.ToInt32( same, history + 17 )) );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, same.Length ).Where( at => same[at] != original[at] ).ToArray();

		Assert.IsTrue( differ.Length > 0 && differ.All( at => at >= fifteenth && at < fifteenth + ParkMonthRing.Size
			|| at >= history && at < history + ParkMonthRing.SizeOf( ParkMonthRing.Stays ) ), "the 15th ring and the stays alone" );
	}
}
