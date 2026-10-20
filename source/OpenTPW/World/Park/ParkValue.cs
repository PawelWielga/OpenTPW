namespace OpenTPW;

/// <summary>
/// What the park is valued at, one whole number - the original's <c>FUN_004c7db0</c>, the finances screen's "Park value"
/// and the analyser's 2nd ring (<c>+0x1f350</c>), sampled at each month's close (<see cref="ParkState.TurnTheMonth"/>).
/// <c>docs/exe/hud.md</c>, "The park value".
///
/// <para>
/// <b>Two sums.</b> Every cell not outside the park adds by its <c>mType</c> and its track record; every object on the
/// chain but a condemned one adds the <c>CostOfUpgrade</c> of each tier up to its own, times its State of repair cut to a
/// byte, over a hundred.
/// </para>
/// </summary>
/// <remarks>
/// The original keeps its last answer (<c>+0x1f0ec</c>) and gives it again to a caller asking within as many quarter
/// ticks as it names; the close names nought, so it takes the sum afresh unless the same quarter tick already took it.
/// This takes it afresh every time (a deviation of the same quarter tick at most).
/// </remarks>
public static class ParkValue
{
	/// <summary>What a cell of each <c>mType</c> adds (<c>0x004c7e24</c>..<c>0x004c7e7d</c>): 9 is both 3's and its own.</summary>
	public static int OfType( int type ) => type switch
	{
		0 => 2,
		1 => 3,
		2 => 1,
		3 => 5,
		9 => 6,
		10 => 1,
		_ => 0
	};

	/// <summary>What a cell whose track record is one of <see cref="CellEdge.TrackCounts"/> adds (<c>FUN_005363f0</c>).</summary>
	public const int TrackCell = 50;

	/// <summary>The <c>mState</c> whose object adds nothing - condemned (<c>FUN_004e0380</c>).</summary>
	public const int Condemned = 4;

	/// <summary>The sum, <paramref name="catalogue"/> naming each object's item; an item it does not name adds nought.</summary>
	public static int Of( ParkState state, ParkItemCatalogue? catalogue )
	{
		var cells = 0;
		var objects = 0;

		unchecked
		{
			for ( var y = 0; y < ParkWorld.MapSize; ++y )
			{
				for ( var x = 0; x < ParkWorld.MapSize; ++x )
				{
					var cell = state.Record( x, y );

					if ( (cell.Flags & ParkPathBuilding.OutsideThePark) != 0 )
						continue;

					cells += OfType( cell.Type );

					if ( TrackCounts( state, cell ) )
						cells += TrackCell;
				}
			}

			foreach ( var thing in state.ObjectsInChainOrder() )
				objects += Of( thing, catalogue );
		}

		Log.Info( $"Park: park value {cells + objects}, cells {cells}, objects {objects}" );

		return cells + objects;
	}

	/// <summary>
	/// One object's share: nought when condemned; otherwise the <c>CostOfUpgrade</c> of tiers nought to its
	/// <c>mUpgradeLevel</c>, times <c>__ftol( +0x44 ) &amp; 0xff</c>, over a hundred, a signed whole division
	/// (<c>0x004c7ef6</c>..<c>0x004c7f56</c>).
	/// </summary>
	public static int Of( ParkWorld.CatalogueObject thing, ParkItemCatalogue? catalogue )
	{
		if ( thing.State == Condemned )
			return 0;

		var costs = 0;

		unchecked
		{
			if ( catalogue != null && catalogue.TryGet( thing.CatalogueId, out var item ) )
			{
				for ( var tier = 0; tier <= (byte)thing.UpgradeLevel; ++tier )
					costs += item.UpgradeCostAt( tier );
			}

			return (((int)thing.StateOfRepair & 0xff) * costs) / 100;
		}
	}

	/// <summary>
	/// Whether a cell's track record is one that adds: its own, or for a type 12 or 17 its parent's, none where that names
	/// no parent (<c>FUN_005363f0</c>, <c>FUN_0053ad90</c>, <c>FUN_0053ad30</c>).
	/// </summary>
	internal static bool TrackCounts( ParkState state, ParkWorld.MapCell cell )
	{
		if ( !CellEdge.TrackDefersToParent( cell.TrackType ) )
			return CellEdge.TrackCounts( cell.TrackType );

		if ( cell.TrackParentId == 0 )
			return false;

		var (x, y) = MapStep.CellAt( cell.TrackParentId );

		return ParkState.OnMap( x, y ) && CellEdge.TrackCounts( state.Record( x, y ).TrackType );
	}
}
