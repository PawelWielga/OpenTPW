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
	/// <remarks><b>Mutations:</b> the total kept after a close; the wrap at 143 or 145; the wrap flag unset; nought read as the newest after a close.</remarks>
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
	/// <b>The shipped park's seven money rings</b>, thing 5's record at 62,598 plus 589 a ring: one month closed, all
	/// costs 13175, staff costs 0, training 0, cash in 1500, gate, shop and sideshow takings 0; the month so far 538,
	/// 538, 0, 200, 200, 0 and 0. Written back changed, each reads back as written in its own place, and the other nine
	/// rings and the slots never filled (<c>0xcdcdcdcd</c>) are the file's bytes.
	/// </summary>
	/// <remarks><b>Mutations:</b> the rings' start a ring out; two rings' places swapped, read and written alike; a ring not written; the writer not handed the rings.</remarks>
	[TestMethod]
	public void TheShippedParksMoneyRingsReadAndWriteBack()
	{
		var shipped = Shipped();
		ParkMonthRing[] Rings( ParkWorld park ) => [park.MonthCosts!, park.MonthStaffCosts!, park.MonthTraining!, park.MonthCashIn!,
			park.MonthGateTakings!, park.MonthShopTakings!, park.MonthSideshowTakings!];

		CollectionAssert.AreEqual( new[] { 13175, 0, 0, 1500, 0, 0, 0 }, Rings( shipped ).Select( ring => ring.Newest ).ToArray() );
		CollectionAssert.AreEqual( new[] { 538, 538, 0, 200, 200, 0, 0 }, Rings( shipped ).Select( ring => ring.Total ).ToArray() );

		var copies = Rings( shipped ).Select( ring => ring.Copy() ).ToArray();

		copies[0].Add( 1 );
		copies[0].Close();

		for ( var i = 1; i < copies.Length; ++i )
			copies[i].Add( i + 1 );

		var body = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: new( copies[0], copies[1], copies[2], copies[3], copies[4], copies[5], copies[6] ) ) );
		var written = Rings( new ParkWorld( body ) );

		Assert.AreEqual( (1, 539, 0), (written[0].Current, written[0].Newest, written[0].Total) );
		CollectionAssert.AreEqual( new[] { 0, 0, 1500, 0, 0, 0 }, written.Skip( 1 ).Select( ring => ring.Newest ).ToArray() );
		CollectionAssert.AreEqual( new[] { 540, 3, 204, 205, 6, 7 }, written.Skip( 1 ).Select( ring => ring.Total ).ToArray(), "each ring in its own place" );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, body.Length ).Where( at => body[at] != original[at] ).ToArray();
		var rings = shipped.AnalyserAt + 62598;

		Assert.IsTrue( differ.All( at => at >= rings + 2 * ParkMonthRing.Size && at < rings + 9 * ParkMonthRing.Size ), "only the seven money rings' bytes" );
		CollectionAssert.AreEqual( new[] { 0, 540, 3, 204, 205, 6, 7 },
			Enumerable.Range( 2, 7 ).Select( ring => System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + ring * ParkMonthRing.Size + 9 ) ) ).ToArray(),
			"each month so far at its ring's own place: the 3rd to the 9th" );
		Assert.AreEqual( unchecked((int)0xcdcdcdcd), System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + 2 * ParkMonthRing.Size + 13 + 2 * 4 ) ), "a slot never filled stays the file's" );
	}
}
