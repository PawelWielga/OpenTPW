namespace OpenTPW;

/// <summary>
/// The golden tickets a Full Simulation park wins - <c>GoldenTicket_Tick</c> (<c>0x004d4a00</c>), which the sweep's tail
/// calls when <c>mGameTick</c> is a multiple of a hundred (<c>0x004d7b54</c>). The six Local tests, the four Global, then
/// the Secret; each tested and awarded alone, every compare a strict greater-than, each award written into the player's
/// progress in memory, which <c>gms.dat</c> takes at its next write (<c>docs/exe/ride-operation.md</c>, "Golden tickets";
/// <c>docs/exe/saves.md</c>, "Keys and tickets").
/// </summary>
/// <remarks>
/// The advisor's ticket lines are counted, not posted (<c>GOLDEN_TICKET_MESSAGE</c>, <c>GOLDEN_TICKET_FIRST_LINES</c>),
/// and so is Global 0's measure, the highest track point of a coaster, where one stands (<c>GOLDEN_TICKET_COASTER_HEIGHT</c>):
/// it is measured as nought there, so that ticket is never won.
/// </remarks>
public static class ParkGoldenTickets
{
	/// <summary>How often the tickets are tested: on every sweep whose <c>mGameTick</c> this divides.</summary>
	public const int EverySweeps = 100;

	/// <summary>The thresholds, the balance's <c>GoldenTicketLocal</c> and <c>GoldenTicketGlobal</c> keys (FileFormats <c>sam.md</c>).</summary>
	public readonly record struct Thresholds( int Visitors, int PeopleInPark, int Happiness, int AtLeastThisManyHappyPeople,
		int ProfitYear, int RecentVisitors, int RecentVisitorMonths,
		int CoasterHeight, int GokartExcitement, int WaterLength, int MinCellsCovered )
	{
		public static Thresholds From( ParkBalance balance )
		{
			int Local( string key ) => balance.Int( $"GoldenTicketLocal.{key}" );
			int Global( string key ) => balance.Int( $"GoldenTicketGlobal.{key}" );

			return new( Local( "Visitors" ), Local( "PeopleInPark" ), Local( "Happiness" ), Local( "AtLeastThisManyHappyPeople" ),
				Local( "ProfitYear" ), Local( "RecentVisitors" ), Local( "RecentVisitorMonths" ),
				Global( "CoasterHeight" ), Global( "GokartExcitement" ), Global( "WaterLength" ), Global( "MinCellsCovered" ) );
		}
	}

	/// <summary>
	/// What the tests read of the park. <paramref name="RecentVisitors"/> is <c>Analyser_RecentVisitors</c> asked for the
	/// <c>RecentVisitorMonths</c> threshold; <paramref name="Months"/> the whole months into the game.
	/// </summary>
	public readonly record struct Measures( int LifetimeVisitors, int PeopleInPark, float Happiness, bool EverythingDone,
		int ProfitThisYear, int RecentVisitors, int Months,
		int CoasterHeight, int KartExcitement, int WaterSections, int CellsCovered, int PercentSecured )
	{
		public override string ToString()
			=> $"visitors {LifetimeVisitors} inside {PeopleInPark} happiness {Happiness:0.###} everything {(EverythingDone ? 1 : 0)} " +
				$"profit {ProfitThisYear} recent {RecentVisitors} months {Months} coaster {CoasterHeight} karts {KartExcitement} " +
				$"water {WaterSections} covered {CellsCovered} secured {PercentSecured}%";
	}

	/// <summary>
	/// One ticket won: its kind, its index, what the award returned (1 to 4, <see cref="PlayerFile.AwardLocal"/>) and the
	/// tickets straight after it: <paramref name="Total"/> besides the secret ones, the count its log line gives, and
	/// <paramref name="Every"/> of every kind, which the first ticket's lines ask for.
	/// </summary>
	public readonly record struct Award( string Kind, int Index, int Result, int Total = 0, int Every = 0 );

	/// <summary>
	/// The tests and the awards, in the original's order (<c>GoldenTicket_CheckLocal</c> <c>0x004d4bc0</c>,
	/// <c>GoldenTicket_CheckGlobal</c> <c>0x004d4e50</c>, the Secret's at <c>0x004d4a6e</c>): a Local ticket the park
	/// holds is skipped; a Global is always asked of the award, which decides; the Secret's index 0 alone, when it is
	/// not held and every counted cell is watched.
	/// </summary>
	public static List<Award> Check( Measures m, Thresholds t, PlayerFile player, string park,
		Func<string, bool> isPark, Func<string, int?> keysToEnter )
	{
		var won = new List<Award>();

		Award Made( string kind, int index, int result )
			=> new( kind, index, result, player.CountTicketsBesidesSecret( isPark ), player.CountTickets( isPark ) );

		void Local( int index, Func<bool> passes )
		{
			if ( player.ParkHoldsTicket( park, index ) || !passes() )
				return;

			if ( player.AwardLocal( park, index, isPark, keysToEnter ) is var result and not 0 )
				won.Add( Made( "Local", index, result ) );
		}

		void Global( int index, int value, int threshold )
		{
			if ( value <= threshold )
				return;

			if ( player.AwardGlobal( park, index, value, isPark, keysToEnter ) is var result and not 0 )
				won.Add( Made( "Global", index, result ) );
		}

		Local( 0, () => m.LifetimeVisitors > t.Visitors );
		Local( 1, () => m.PeopleInPark > t.PeopleInPark );
		Local( 2, () => m.Happiness > t.Happiness && m.PeopleInPark > t.AtLeastThisManyHappyPeople );
		Local( 3, () => m.EverythingDone );
		Local( 4, () => m.ProfitThisYear > t.ProfitYear );
		Local( 5, () => m.RecentVisitors > t.RecentVisitors && m.Months > t.RecentVisitorMonths );

		Global( 0, m.CoasterHeight, t.CoasterHeight );
		Global( 1, m.KartExcitement, t.GokartExcitement );
		Global( 2, m.WaterSections, t.WaterLength );
		Global( 3, m.CellsCovered, t.MinCellsCovered );

		if ( player.BonusTickets[0] == 0 && m.PercentSecured == 100
			&& player.AwardSecret( 0, isPark, keysToEnter ) is var secret and not 0 )
			won.Add( Made( "Secret", 0, secret ) );

		return won;
	}

	/// <summary>
	/// The log lines the original writes for an award (<c>GoldenTicket_TellLocal</c> <c>0x004d4fa0</c>,
	/// <c>GoldenTicket_TellGlobal</c> <c>0x004d5270</c>, <c>GoldenTicket_TellSecret</c> <c>0x004d5870</c>), in its own words.
	/// </summary>
	public static string Told( Award award )
	{
		var what = (award.Kind, award.Result) switch
		{
			(_, 1) => $"{award.Kind} ticket only",
			(_, 2) => $"{award.Kind} ticket and key",
			(_, 3) => $"{award.Kind} ticket, key and park",
			("Global", 4) => "Global ticket award moved here",
			_ => $"CGoldenTicketControl::TellTheWorld ({award.Kind}) is being passed an unrecognised type value ({award.Result})"
		};

		return $"Won a {award.Kind} golden ticket (type - {award.Result}, num - {award.Index}); {what}; You have {award.Total} total golden tickets";
	}

	/// <summary>The measures of the park as it stands now.</summary>
	internal static Measures Measure( ParkState state, ParkPeople people, ParkItemCatalogue? catalogue, ParkResearch? research,
		int recentVisitorMonths )
	{
		var kart = 0;
		var water = 0;
		var coaster = false;

		foreach ( var placed in state.Objects )
		{
			if ( catalogue == null || !catalogue.TryGet( placed.CatalogueId, out var item ) || item.UiType != ItemDescriptionFile.Ride )
				continue;

			switch ( item.TrackType )
			{
				case ItemDescriptionFile.CoasterTrack:
					coaster = true;
					break;

				// FUN_004cc3b0: the highest excitement, its low byte, of a ride on a car track.
				case ItemDescriptionFile.CarTrack:
					kart = Math.Max( kart, ParkRideScore.ExcitementOf( placed, item, state.TrackRides ) & 0xff );
					break;

				// FUN_004cc6f0( 2 ): the longest section list of a ride on a water track.
				case ItemDescriptionFile.WaterTrack:
					water = Math.Max( water, state.TrackRides.LayoutOf( placed.TrackRide )?.Sections ?? 0 );
					break;
			}
		}

		if ( coaster )
			Unimplemented.Report( "GOLDEN_TICKET_COASTER_HEIGHT" );

		var (covered, secured) = Cells( state );

		return new( state.LifetimeVisitors, people.GateGuestCensus, people.GuestsInsideHappiness,
			EverythingDone( state, catalogue, research ),
			state.ProfitThisYear,
			state.MonthVisitors.Back( recentVisitorMonths ) is { } then ? state.LifetimeVisitors - then : 0,
			ParkStrikes.MonthsSinceTheFirstSweep( state.GameTick ),
			0, kart, water, covered, secured );
	}

	/// <summary>
	/// Local 3, <c>Park_EverythingResearchedAndStanding</c> (<c>0x004ca6b0</c>): no lab project of any kind left undone,
	/// and every item of the catalogue one the UI does not show (<c>WhichUIType</c> 4), one with at least one standing, or
	/// one that costs a golden ticket.
	/// </summary>
	internal static bool EverythingDone( ParkState state, ParkItemCatalogue? catalogue, ParkResearch? research )
		=> EverythingDone( research?.UnfinishedOfAnyKind ?? 0, catalogue?.All ?? [], id => state.BuiltOf( id ).Standing );

	/// <summary><see cref="EverythingDone(ParkState, ParkItemCatalogue?, ParkResearch?)"/> of the lab's undone projects, the items and how many of each stand.</summary>
	public static bool EverythingDone( int undone, IEnumerable<ParkItemCatalogue.Item> items, Func<int, int> standing )
	{
		if ( undone > 0 )
			return false;

		return items.All( item => item.UiType == 4 || standing( item.Id ) != 0 || item.GoldenTicketCost != 0 );
	}

	/// <summary>
	/// Global 3's cells and the Secret's percentage, one walk of the map: the cells of kind 9, 10, 4 or <c>0x15</c>, or
	/// whose track record counts (<c>FUN_004cd7f0</c>, <c>hud.md</c>'s cell predicates); and of the cells not of kind 2,
	/// 7 or <c>0x1e</c>, the whole percentage whose security word is above nought (<c>Park_PercentCellsSecured</c>
	/// <c>0x004cd720</c>), nought with none.
	/// </summary>
	internal static (int Covered, int PercentSecured) Cells( ParkState state )
	{
		int covered = 0, counted = 0, secured = 0;

		for ( var y = 0; y < ParkWorld.MapSize; ++y )
		{
			for ( var x = 0; x < ParkWorld.MapSize; ++x )
			{
				var cell = state.Record( x, y );

				if ( cell.Type is 9 or 10 or 4 or 0x15 || ParkValue.TrackCounts( state, cell ) )
					++covered;

				if ( cell.Type is 2 or 7 or 0x1e )
					continue;

				++counted;

				if ( state.EffectsAt( x, y )[SecurityWord] > 0 )
					++secured;
			}
		}

		return (covered, counted == 0 ? 0 : secured * 100 / counted);
	}

	/// <summary>The security word of a cell's five, its <c>+6</c>.</summary>
	private const int SecurityWord = 3;

	/// <summary>The measures and awards of the last test, for the console's <c>tickets</c>.</summary>
	internal static (int Tick, Measures Measures, List<Award> Won)? Last { get; private set; }

	/// <summary>
	/// The test on a sweep, from the sweep's tail after the staff pool's turn (<c>0x004d7b60</c>): nothing unless the
	/// park's clock is a multiple of <see cref="EverySweeps"/> and the game is Full Simulation (<c>DAT_00fb3b7c</c> 0).
	/// </summary>
	internal static void Sweep( int gameTick, ParkState state, ParkPeople people )
	{
		if ( gameTick % EverySweeps != 0 || Level.InstantAction || Level.Current is not { } level
			|| Players.Roster.Current is not { } player || level.Balance is not { } balance )
			return;

		var thresholds = Thresholds.From( balance );
		var measures = Measure( state, people, level.Catalogue, level.Research, thresholds.RecentVisitorMonths );
		var won = Check( measures, thresholds, player.File, level.ThemeName, SaveFolder.IsTheme, KeysToEnter );

		Last = (gameTick, measures, won);
		Log.Info( $"Golden tickets: tick {gameTick}, {measures}, {won.Count} won" );

		foreach ( var award in won )
		{
			Log.Info( $"Golden tickets: {Told( award )}" );
			Unimplemented.Report( "GOLDEN_TICKET_MESSAGE" );

			// GoldenTicket_TellFirst (0x004d5b30): the first ticket's two lines, 0x137 and 0x13b, when the tickets of every kind are one.
			if ( award.Every == 1 )
				Unimplemented.Report( "GOLDEN_TICKET_FIRST_LINES" );
		}
	}

	/// <summary>A park's <c>Keys.CostToEnter</c>, read once, or null where its global.sam does not load (<c>0x005b11a0</c>).</summary>
	private static int? KeysToEnter( string park )
	{
		if ( !_keysToEnter.TryGetValue( park, out var keys ) )
		{
			var (cost, loaded) = LobbyIsland.ReadKeysToEnter( park );
			_keysToEnter[park] = keys = loaded ? cost : null;
		}

		return keys;
	}

	private static readonly Dictionary<string, int?> _keysToEnter = [];
}
