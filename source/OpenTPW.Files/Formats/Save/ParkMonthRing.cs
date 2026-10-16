namespace OpenTPW;

/// <summary>
/// One of the park analyser's month rings: the running month's total and the totals of the last 144 months, each
/// closed into the ring at the month's change (<c>FUN_004c7720</c>, <c>FUN_004ce290</c>; <c>docs/exe/hud.md</c>, "The
/// mini-balance is a MONTHLY ACCOUNT"). In a park file, <see cref="Size"/> bytes: <c>mCurrentEntry</c>,
/// <c>mNumEntries</c>, <c>mWrappedAround</c> (a byte), <c>mTemp</c> (the running total) and the months
/// (<c>FUN_004c5d70</c>). The analyser's ring of the last <see cref="Stays"/> leavers' stays is the same shape, shorter
/// (<c>+0x21600</c>, <c>FUN_004ce2d0</c>).
/// </summary>
public sealed class ParkMonthRing
{
	/// <summary>How many months a ring keeps: the constructor's <c>mNumEntries</c> (<c>FUN_004c58c0</c>), 144 in every file.</summary>
	public const int Months = 144;

	/// <summary>How many stays the ring of leavers' stays keeps: the constructor's <c>mNumEntries</c> at <c>+0x216d0</c>, 50 in every file.</summary>
	public const int Stays = 50;

	/// <summary>A month ring's bytes in a park file.</summary>
	public const int Size = 4 + 4 + 1 + 4 + Months * 4;

	/// <summary>The bytes in a park file of a ring of <paramref name="length"/> entries.</summary>
	public static int SizeOf( int length ) => 4 + 4 + 1 + 4 + length * 4;

	private readonly int[] _months;

	/// <summary>A month ring, empty: <see cref="Months"/> slots.</summary>
	public ParkMonthRing() : this( Months ) { }

	/// <summary>An empty ring of <paramref name="length"/> slots, as the analyser's constructor makes each (<c>FUN_004c58c0</c>).</summary>
	public ParkMonthRing( int length ) => _months = new int[length];

	/// <summary>The ring's slots, its <c>mNumEntries</c>.</summary>
	public int Length => _months.Length;

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
		Push( Total );
		Total = 0;
	}

	/// <summary>
	/// The month's change for a ring that samples a figure rather than adding up a month: <paramref name="value"/> into
	/// the next slot, round to the first past the last, the total left as it is (the bank's balance, <c>FUN_004c7720</c>).
	/// </summary>
	public void Push( int value )
	{
		if ( ++Current >= Length )
		{
			Current = 0;
			Wrapped = true;
		}

		_months[Current] = value;
	}

	/// <summary>
	/// The month's change for the ring of floats, the 14th (<c>+0x20f10</c>): <paramref name="value"/>'s bits into the
	/// next slot, as <see cref="Push(int)"/> (<c>FSTP</c> at <c>0x004c79a0</c>).
	/// </summary>
	public void Push( float value ) => Push( BitConverter.SingleToInt32Bits( value ) );

	/// <summary>The newest month as a float, for the ring of floats: <see cref="Newest"/>'s bits.</summary>
	public float NewestSingle => BitConverter.Int32BitsToSingle( Newest );

	/// <summary>
	/// The newest month's total as the hire screen reads it (<c>0x0049c54f</c>): the current slot's; below nought,
	/// the slot that index plus the ring's length names once the ring has wrapped, and nought before.
	/// </summary>
	public int Newest
		=> Current >= 0 ? _months[Current]
			: Wrapped ? _months[Current + Length]
			: 0;

	/// <summary>
	/// The mean of the newest <paramref name="count"/> entries, or of as many as the ring has, as the month's close takes
	/// it for the 15th ring (<c>0x004c79d7</c>..<c>0x004c7a13</c>): <c>FUN_004cef30</c> sums them newest first as a double,
	/// and the close divides by how many it took and truncates (<c>__ftol</c>); nought with none.
	/// </summary>
	public int MeanOfNewest( int count )
	{
		var taken = Math.Min( count, Wrapped ? Length : Current + 1 );
		var sum = 0.0;

		for ( var i = 0; i < taken; ++i )
			sum += Current >= i ? _months[Current - i] : _months[Current - i + Length];

		return taken > 0 ? (int)(sum / taken) : 0;
	}

	/// <summary>
	/// The ring at the start of <paramref name="bytes"/>, or null where its length is not <paramref name="length"/> or its
	/// slot is outside the ring: the original would read past its months there, and no file the game writes does it.
	/// </summary>
	public static ParkMonthRing? Read( ReadOnlySpan<byte> bytes, int length = Months )
	{
		var current = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes );
		var entries = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[4..] );

		if ( entries != length || current < -1 || current >= length )
			return null;

		var ring = new ParkMonthRing( length )
		{
			Current = current,
			Wrapped = bytes[8] != 0,
			Total = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[9..] )
		};

		for ( var i = 0; i < length; ++i )
			ring._months[i] = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian( bytes[(13 + i * 4)..] );

		return ring;
	}

	/// <summary>Writes the ring over the <see cref="SizeOf"/> bytes at the start of <paramref name="bytes"/>.</summary>
	public void Write( Span<byte> bytes )
	{
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes, Current );
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[4..], Length );
		bytes[8] = Wrapped ? (byte)1 : (byte)0;
		System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[9..], Total );

		for ( var i = 0; i < Length; ++i )
			System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian( bytes[(13 + i * 4)..], _months[i] );
	}

	/// <summary>A copy to run on, so the file's own stays as read.</summary>
	public ParkMonthRing Copy()
	{
		var copy = new ParkMonthRing( Length ) { Current = Current, Wrapped = Wrapped, Total = Total };

		_months.CopyTo( copy._months, 0 );

		return copy;
	}
}
