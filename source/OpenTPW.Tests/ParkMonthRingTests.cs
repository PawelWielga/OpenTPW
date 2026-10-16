using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park analyser's month rings (<see cref="ParkMonthRing"/>; <c>docs/exe/hud.md</c>, "The mini-balance is a MONTHLY
/// ACCOUNT"): the close at the month's change, the newest month as the screens read it, and the three money rings in a
/// park file (<c>saves.md</c>, "The park analyser"). The ring itself runs without the game; the file reads Lost
/// Kingdom's save and is skipped where there is no installation.
/// </summary>
[TestClass]
public class ParkMonthRingTests
{
	private static ParkWorld Shipped()
	{
		using var stream = new MemoryStream( GameData.Required().ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );

		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	/// <summary>
	/// <b>A close puts the month's total in the next slot and zeroes it, round to the first past the 144th</b>
	/// (<c>FUN_004ce290</c>); the newest is nought before any close, then the slot just written.
	/// </summary>
	/// <remarks><b>Mutations:</b> the total kept after a close; the wrap at 143 or 145; the wrap flag unset; nought read as the newest after a close; a push that zeroes the total.</remarks>
	[TestMethod]
	public void ACloseFillsTheNextSlotAndTheRingGoesRoundAfter144Months()
	{
		var ring = new ParkMonthRing();

		Assert.AreEqual( (-1, false, 0), (ring.Current, ring.Wrapped, ring.Newest) );

		ring.Add( 30 );
		ring.Add( 12 );
		ring.Close();

		Assert.AreEqual( (0, 42, 0), (ring.Current, ring.Newest, ring.Total) );

		for ( var month = 1; month < ParkMonthRing.Months; ++month )
		{
			ring.Add( month );
			ring.Close();
		}

		Assert.AreEqual( (143, false, 143), (ring.Current, ring.Wrapped, ring.Newest) );

		ring.Add( 7 );
		ring.Close();

		Assert.AreEqual( (0, true, 7), (ring.Current, ring.Wrapped, ring.Newest) );

		ring.Add( 5 );
		ring.Push( 900 );

		Assert.AreEqual( (1, 900, 5), (ring.Current, ring.Newest, ring.Total), "a push samples a figure and leaves the month so far" );
	}

	/// <summary>
	/// <b>A ring's 589 bytes</b>: <c>mCurrentEntry</c>, <c>mNumEntries</c>, <c>mWrappedAround</c>, <c>mTemp</c> and the
	/// months; a slot of -1 reads nought, or the last month once wrapped (<c>0x0049c572</c>); a length other than 144 or
	/// a slot outside the ring is not read.
	/// </summary>
	/// <remarks><b>Mutations:</b> the byte fields in another order; a wrapped -1 read as nought; a length of 143 taken.</remarks>
	[TestMethod]
	public void ARingReadsAndWritesItsFileBytesAndAWrappedMinusOneIsTheLastMonth()
	{
		var bytes = new byte[ParkMonthRing.Size];

		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes, -1 );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( 4 ), 144 );
		bytes[8] = 1;
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( 9 ), 55 );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( 13 + 143 * 4 ), 900 );

		var ring = ParkMonthRing.Read( bytes )!;

		Assert.AreEqual( (-1, true, 55, 900), (ring.Current, ring.Wrapped, ring.Total, ring.Newest) );

		var written = new byte[ParkMonthRing.Size];
		ring.Write( written );
		CollectionAssert.AreEqual( bytes, written );

		bytes[8] = 0;
		Assert.AreEqual( 0, ParkMonthRing.Read( bytes )!.Newest, "not wrapped: nothing closed yet" );

		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( 4 ), 143 );
		Assert.IsNull( ParkMonthRing.Read( bytes ) );

		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes.AsSpan( 4 ), 144 );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes, 144 );
		Assert.IsNull( ParkMonthRing.Read( bytes ) );
	}

	/// <summary>
	/// <b>The shipped park's eight money rings and its park value</b>, thing 5's record at 62,598 plus 589 a ring: one month
	/// closed, the bank's balance 88325, the park value 23977, all costs 13175, staff costs 0, training 0, cash in 1500,
	/// gate, shop and sideshow takings 0; the month so far 0 (the balance and park value rings have none), 538, 538, 0,
	/// 200, 200, 0 and 0. Written back changed, each reads back as written in its own place, and the other seven rings and
	/// the slots never filled (<c>0xcdcdcdcd</c>) are the file's bytes.
	/// </summary>
	/// <remarks><b>Mutations:</b> the rings' start a ring out; the balance or park value ring read or written as another; two rings' places swapped, read and written alike; a ring not written; the writer not handed the rings.</remarks>
	[TestMethod]
	public void TheShippedParksMoneyRingsReadAndWriteBack()
	{
		var shipped = Shipped();
		ParkMonthRing[] Rings( ParkWorld park ) => [park.MonthBalance!, park.MonthParkValue!, park.MonthCosts!, park.MonthStaffCosts!,
			park.MonthTraining!, park.MonthCashIn!, park.MonthGateTakings!, park.MonthShopTakings!, park.MonthSideshowTakings!];

		CollectionAssert.AreEqual( new[] { 88325, 23977, 13175, 0, 0, 1500, 0, 0, 0 }, Rings( shipped ).Select( ring => ring.Newest ).ToArray() );
		CollectionAssert.AreEqual( new[] { 0, 0, 538, 538, 0, 200, 200, 0, 0 }, Rings( shipped ).Select( ring => ring.Total ).ToArray() );

		var copies = Rings( shipped ).Select( ring => ring.Copy() ).ToArray();

		copies[0].Push( 4242 );
		copies[1].Push( 31337 );
		copies[2].Add( 1 );
		copies[2].Close();

		for ( var i = 3; i < copies.Length; ++i )
			copies[i].Add( i );

		var body = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( copies[0], copies[1], copies[2], copies[3], copies[4], copies[5], copies[6], copies[7], copies[8], shipped.MonthGuestsInside!.Copy(), shipped.MonthVisitors!.Copy(), shipped.MonthRides!.Copy(), shipped.MonthArrivals!.Copy() ) ) );
		var written = Rings( new ParkWorld( body ) );

		Assert.AreEqual( (1, 4242, 0), (written[0].Current, written[0].Newest, written[0].Total), "the balance pushed, its month so far untouched" );
		Assert.AreEqual( (1, 31337, 0), (written[1].Current, written[1].Newest, written[1].Total), "the park value pushed, its month so far untouched" );
		Assert.AreEqual( (1, 539, 0), (written[2].Current, written[2].Newest, written[2].Total) );
		CollectionAssert.AreEqual( new[] { 0, 0, 1500, 0, 0, 0 }, written.Skip( 3 ).Select( ring => ring.Newest ).ToArray() );
		CollectionAssert.AreEqual( new[] { 541, 4, 205, 206, 7, 8 }, written.Skip( 3 ).Select( ring => ring.Total ).ToArray(), "each ring in its own place" );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, body.Length ).Where( at => body[at] != original[at] ).ToArray();
		var rings = shipped.AnalyserAt + 62598;

		Assert.IsTrue( differ.All( at => at >= rings && at < rings + 9 * ParkMonthRing.Size ), "only the first nine rings' bytes" );
		Assert.AreEqual( 4242, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + 13 + 4 ) ), "the balance at the 1st ring's slot 1" );
		Assert.AreEqual( 31337, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + ParkMonthRing.Size + 13 + 4 ) ), "the park value at the 2nd ring's slot 1" );
		CollectionAssert.AreEqual( new[] { 0, 541, 4, 205, 206, 7, 8 },
			Enumerable.Range( 2, 7 ).Select( ring => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + ring * ParkMonthRing.Size + 9 ) ) ).ToArray(),
			"each month so far at its ring's own place: the 3rd to the 9th" );
		Assert.AreEqual( unchecked((int)0xcdcdcdcd), System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + 2 * ParkMonthRing.Size + 13 + 2 * 4 ) ), "a slot never filled stays the file's" );
		Assert.AreEqual( unchecked((int)0xcdcdcdcd), System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + 9 * ParkMonthRing.Size + 13 + 4 ) ), "the 10th ring the file's" );
	}

	/// <summary>
	/// <b>The shipped park's lifetime visitors, its dearest ticket and their ring</b>: thing 5's record at 73,480 and
	/// 73,532, 8 and 25, and the 11th int ring at 62,598 + 10 x 589, one month closed on nought, no month so far. Written
	/// back changed, each reads back as written, at its own place and nowhere else.
	/// </summary>
	/// <remarks><b>Mutations:</b> the count or the dearest read or written a dword out, or each as the other; the 11th ring read or written as the 10th or the 12th; the writer not handed the two.</remarks>
	[TestMethod]
	public void TheShippedParksLifetimeVisitorsReadAndWriteBack()
	{
		var shipped = Shipped();
		var visits = shipped.MonthVisitors!;

		Assert.AreEqual( (8, 25), (shipped.LifetimeVisitors, shipped.MostPaidForTicket) );
		Assert.AreEqual( (0, 0, 0), (visits.Current, visits.Newest, visits.Total) );

		var ring = visits.Copy();
		ring.Push( 4321 );

		var same = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( shipped.MonthBalance!, shipped.MonthParkValue!, shipped.MonthCosts!, shipped.MonthStaffCosts!, shipped.MonthTraining!,
				shipped.MonthCashIn!, shipped.MonthGateTakings!, shipped.MonthShopTakings!, shipped.MonthSideshowTakings!, shipped.MonthGuestsInside!, ring,
				shipped.MonthRides!, shipped.MonthArrivals! ), Visitors: (99, 31) ) );
		var written = new ParkWorld( same );

		Assert.AreEqual( (99, 31), (written.LifetimeVisitors, written.MostPaidForTicket) );
		Assert.AreEqual( (1, 4321, 0), (written.MonthVisitors!.Current, written.MonthVisitors.Newest, written.MonthVisitors.Total) );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, same.Length ).Where( at => same[at] != original[at] ).ToArray();
		var record = shipped.AnalyserAt;
		var eleventh = record + 62598 + 10 * ParkMonthRing.Size;

		Assert.IsTrue( differ.Length > 0 && differ.All( at => at >= eleventh && at < eleventh + ParkMonthRing.Size
			|| at >= record + 73480 && at < record + 73484 || at >= record + 73532 && at < record + 73536 ), "the 11th ring and the two dwords alone" );
		Assert.AreEqual( 99, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( same.AsSpan( record + 73480 ) ) );
		Assert.AreEqual( 31, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( same.AsSpan( record + 73532 ) ) );
		Assert.AreEqual( 4321, System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( same.AsSpan( eleventh + 13 + 4 ) ), "the count at the 11th ring's slot 1" );
	}
}
