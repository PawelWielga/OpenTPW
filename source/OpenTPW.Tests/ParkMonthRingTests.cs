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
	/// <b>The shipped park's three money rings</b>, thing 5's record at 62,598 plus 589 a ring: one month closed, all
	/// costs 13175, staff costs 0, cash in 1500; the month so far 538, 538 and 200. Written back changed, they read back
	/// as written, and the other thirteen rings and the slots never filled (<c>0xcdcdcdcd</c>) are the file's bytes.
	/// </summary>
	/// <remarks><b>Mutations:</b> the rings' start a ring out; the costs and cash-in places swapped; the writer not handed the rings.</remarks>
	[TestMethod]
	public void TheShippedParksMoneyRingsReadAndWriteBack()
	{
		var shipped = Shipped();

		Assert.AreEqual( (13175, 538), (shipped.MonthCosts!.Newest, shipped.MonthCosts.Total) );
		Assert.AreEqual( (0, 538), (shipped.MonthStaffCosts!.Newest, shipped.MonthStaffCosts.Total) );
		Assert.AreEqual( (1500, 200), (shipped.MonthCashIn!.Newest, shipped.MonthCashIn.Total) );

		var costs = shipped.MonthCosts.Copy();
		var staff = shipped.MonthStaffCosts.Copy();
		var cash = shipped.MonthCashIn.Copy();

		costs.Add( 1 );
		costs.Close();
		staff.Add( 2 );
		cash.Add( 3 );

		var body = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value,
			MonthRings: (costs, staff, cash) ) );
		var written = new ParkWorld( body );

		Assert.AreEqual( (1, 539, 0), (written.MonthCosts!.Current, written.MonthCosts.Newest, written.MonthCosts.Total) );
		Assert.AreEqual( (0, 540), (written.MonthStaffCosts!.Newest, written.MonthStaffCosts.Total) );
		Assert.AreEqual( (1500, 203), (written.MonthCashIn!.Newest, written.MonthCashIn.Total) );

		var original = ParkFileWriter.Body( shipped, new ParkFileWriter.Running( shipped.GameTick, false, 13, 0, shipped.Camera.Saved!.Value ) );
		var differ = Enumerable.Range( 0, body.Length ).Where( at => body[at] != original[at] ).ToArray();
		var rings = shipped.AnalyserAt + 62598;

		Assert.IsTrue( differ.All( at => at >= rings + 2 * ParkMonthRing.Size && at < rings + 6 * ParkMonthRing.Size ), "only the costs, staff and cash-in rings' bytes" );
		Assert.AreEqual( unchecked((int)0xcdcdcdcd), System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( body.AsSpan( rings + 2 * ParkMonthRing.Size + 13 + 2 * 4 ) ), "a slot never filled stays the file's" );
	}
}
