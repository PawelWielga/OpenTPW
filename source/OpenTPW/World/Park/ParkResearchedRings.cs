namespace OpenTPW;

/// <summary>
/// The park analyser's four rings of the recently researched, one a tab of the buy list in <c>Info.WhichUIType</c>'s
/// order, three item ids each (<see cref="ParkWorld.ResearchedRing"/>). Message <c>0x11</c>, a project finished, pushes
/// its item into its tab's ring (<c>FUN_004c7370</c>, <c>0x004c73ab</c>), and the buy list marks a row of one of the
/// three newest with the recently researched star, row state 2 (<c>0x004ab0f7</c>). docs/exe/park-engine.md, "The lab
/// at work"; docs/exe/hud.md, "What the buy list actually filters on".
/// </summary>
public sealed class ParkResearchedRings
{
	private readonly ParkWorld.ResearchedRing[] _rings;

	/// <summary>The file's rings, or a park made with no file's: nothing pushed.</summary>
	public ParkResearchedRings( IReadOnlyList<ParkWorld.ResearchedRing>? saved = null )
	{
		_rings = saved is { Count: ParkWorld.ResearchedRings }
			? saved.Select( ring => ring with { Data = ring.Data.ToArray() } ).ToArray()
			: Enumerable.Range( 0, ParkWorld.ResearchedRings ).Select( _ => ParkWorld.ResearchedRing.Fresh ).ToArray();
	}

	/// <summary>The rings as a park file holds them.</summary>
	public IReadOnlyList<ParkWorld.ResearchedRing> Written => _rings.Select( ring => ring with { Data = ring.Data.ToArray() } ).ToArray();

	/// <summary>
	/// The analyser's arm of message <c>0x11</c>: the slot after the last pushed, back to the first past the last
	/// (<c>mWrappedAround</c> set then), takes <paramref name="item"/>. A tab past the four, which the original writes
	/// over the analyser's next fields with, takes nothing here: no researched item is of one.
	/// </summary>
	public bool Push( int tab, int item )
	{
		if ( tab < 0 || tab >= _rings.Length )
		{
			Log.Warning( $"Research: item {item} is of buy list tab {tab}, which has no ring of the recently researched" );
			return false;
		}

		var ring = _rings[tab];
		var current = ring.Current + 1;
		var wrapped = ring.Wrapped;

		if ( current >= ring.Entries )
		{
			current = 0;
			wrapped = true;
		}

		ring.Data[current] = (ushort)item;
		_rings[tab] = ring with { Current = current, Wrapped = wrapped };
		Log.Info( $"Research: the analyser's tab {tab} ring takes item {item} in slot {current}: {Census( tab )}" );
		return true;
	}

	/// <summary>
	/// The item pushed <paramref name="back"/> pushes before the last, or nought where the ring holds none so far back
	/// (<c>FUN_004caa70</c>): at or before the last slot pushed, that slot; past it, only where the ring has wrapped.
	/// </summary>
	public int Back( int tab, int back )
	{
		if ( tab < 0 || tab >= _rings.Length )
			return 0;

		var ring = _rings[tab];

		if ( back >= ring.Entries )
			return 0;

		if ( back <= ring.Current )
			return ring.Data[ring.Current - back];

		return ring.Wrapped ? ring.Data[ring.Current - back + ring.Entries] : 0;
	}

	/// <summary>Whether <paramref name="item"/> is one of the three newest of its tab (<c>0x004ab0aa</c>..<c>0x004ab0f7</c>).</summary>
	public bool IsRecent( int tab, int item )
		=> item != 0 && Enumerable.Range( 0, ParkWorld.ResearchedRing.Slots ).Any( back => Back( tab, back ) == item );

	/// <summary>One ring, the newest first.</summary>
	public string Census( int tab )
		=> $"[{string.Join( ",", Enumerable.Range( 0, ParkWorld.ResearchedRing.Slots ).Select( back => Back( tab, back ) ) )}] "
			+ $"current {_rings[tab].Current} wrapped {(_rings[tab].Wrapped ? 1 : 0)}";
}
