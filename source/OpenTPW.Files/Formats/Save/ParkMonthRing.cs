namespace OpenTPW;

/// <summary>
/// One of the park analyser's month rings: the running month's total and the totals of the last 144 months, each
/// closed into the ring at the month's change (<c>FUN_004c7720</c>, <c>FUN_004ce290</c>; <c>docs/exe/hud.md</c>, "The
/// mini-balance is a MONTHLY ACCOUNT"). In a park file, <see cref="Size"/> bytes: <c>mCurrentEntry</c>,
/// <c>mNumEntries</c>, <c>mWrappedAround</c> (a byte), <c>mTemp</c> (the running total) and the months
/// (<c>FUN_004c5d70</c>).
/// </summary>
public sealed class ParkMonthRing
{
	/// <summary>How many months a ring keeps: the constructor's <c>mNumEntries</c> (<c>FUN_004c58c0</c>), 144 in every file.</summary>
	public const int Months = 144;

	/// <summary>A ring's bytes in a park file.</summary>
	public const int Size = 4 + 4 + 1 + 4 + Months * 4;

	private readonly int[] _months = new int[Months];

	/// <summary>The slot of the newest month, -1 before any is closed.</summary>
	public int Current { get; private set; } = -1;

	/// <summary>Whether the ring has gone round once.</summary>
	public bool Wrapped { get; private set; }

	/// <summary>The month's total so far.</summary>
	public int Total { get; private set; }

	/// <summary>Adds to the month's total, as the bank's and the staff's adds do, wrapping as the original's <c>ADD</c>.</summary>
	public void Add( int amount ) => Total = unchecked(Total + amount);

	/// <summary>The month's change: the total into the next slot, round to the first past the last, and the total nought.</summary>
	public void Close()
	{
		if ( ++Current >= Months )
		{
			Current = 0;
			Wrapped = true;
		}

		_months[Current] = Total;
		Total = 0;
	}

	/// <summary>
	/// The newest month's total as the hire screen reads it (<c>0x0049c54f</c>): the current slot's; below nought,
	/// the slot that index plus the ring's length names once the ring has wrapped, and nought before.
	/// </summary>
	public int Newest
		=> Current >= 0 ? _months[Current]
			: Wrapped ? _months[Current + Months]
			: 0;

	/// <summary>
	/// The ring at the start of <paramref name="bytes"/>, or null where its length is not <see cref="Months"/> or its
	/// slot is outside the ring: the original would read past its months there, and no file the game writes does it.
	/// </summary>
	public static ParkMonthRing? Read( ReadOnlySpan<byte> bytes )
	{
		var current = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes );
		var entries = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[4..] );

		if ( entries != Months || current < -1 || current >= Months )
			return null;

		var ring = new ParkMonthRing
		{
			Current = current,
			Wrapped = bytes[8] != 0,
			Total = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[9..] )
		};

		for ( var i = 0; i < Months; ++i )
			ring._months[i] = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[(13 + i * 4)..] );

		return ring;
	}

	/// <summary>Writes the ring over the <see cref="Size"/> bytes at the start of <paramref name="bytes"/>.</summary>
	public void Write( Span<byte> bytes )
	{
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes, Current );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[4..], Months );
		bytes[8] = Wrapped ? (byte)1 : (byte)0;
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[9..], Total );

		for ( var i = 0; i < Months; ++i )
			System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[(13 + i * 4)..], _months[i] );
	}

	/// <summary>A copy to run on, so the file's own stays as read.</summary>
	public ParkMonthRing Copy()
	{
		var copy = new ParkMonthRing { Current = Current, Wrapped = Wrapped, Total = Total };

		_months.CopyTo( copy._months, 0 );

		return copy;
	}
}
