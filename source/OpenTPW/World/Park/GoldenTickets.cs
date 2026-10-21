namespace OpenTPW;

/// <summary>
/// Spending golden tickets: the player's tests on a ride with a ticket cost, and its purchase -
/// <c>docs/exe/hud.md</c>, "Spending golden tickets".
///
/// <para>
/// A ride whose <c>UsageInfo.GoldenTicketCost</c> is above nought and whose id the player has not bought with tickets
/// is a <b>mystery</b>: the buy list names it "??? Mystery Ride! ???" and prices it in tickets, its click asks the
/// tickets and not the bank, carrying it is never refused for money, and putting it down spends the tickets in place
/// of its price. Once bought it is the ride's own row, paid in cash like any other. The bought ids and the tickets
/// spent are the player's, in their <c>gms.dat</c>, across every park and theme.
/// </para>
/// <para>
/// The original's spend does nothing in its online game type (<c>[0x00fb3b7c]</c> 1); this game has none, so that arm
/// is not reached.
/// </para>
/// </summary>
internal static class GoldenTickets
{
	/// <summary>The ids the current player has bought with tickets; null with nobody playing.</summary>
	public static IReadOnlyCollection<ushort>? Rides => Players.Roster.Current?.File.RideIds;

	/// <summary>The current player's tickets left (<c>PlayerProgress_TicketsLeft</c>); nought with nobody playing.</summary>
	public static int TicketsLeft => Players.Roster.Current?.TicketsUnspent ?? 0;

	/// <summary><c>GoldenTicket_RideBought</c> (<c>0x004d4b70</c>): the id is in the player's set.</summary>
	public static bool RideBought( IReadOnlyCollection<ushort>? rides, int id ) => rides?.Contains( (ushort)id ) == true;

	/// <summary>
	/// A ticket cost above nought, not yet bought: the test the list (<c>0x004ab023</c>), the click (<c>0x004ac5a9</c>),
	/// the carry (<c>0x005235e6</c>) and the purchase (<c>0x004db4a6</c>) each make.
	/// </summary>
	public static bool IsMystery( ParkItemCatalogue.Item item, IReadOnlyCollection<ushort>? rides )
		=> item.GoldenTicketCost > 0 && !RideBought( rides, item.Id );

	/// <inheritdoc cref="IsMystery(ParkItemCatalogue.Item, IReadOnlyCollection{ushort})"/>
	public static bool IsMystery( ParkItemCatalogue.Item item ) => IsMystery( item, Rides );

	/// <summary><c>GoldenTicket_CanBuyRide</c> (<c>0x004d4b90</c>): the cost at most the tickets left, which have no floor.</summary>
	public static bool CanBuyRide( ParkItemCatalogue.Item item, int ticketsLeft ) => item.GoldenTicketCost <= ticketsLeft;

	/// <summary>
	/// <c>GoldenTicket_SpendForRide</c> (<c>0x004d4ad0</c>): where the tickets suffice, <c>PlayerProgress_BuyRide</c>
	/// (<c>0x005af740</c>) puts the id into the set, kept in order as the original's set is, and only where it was not
	/// there adds the cost to the tickets spent. Answers whether the id went in. Nothing is written to disk here: the
	/// player's file is written at its next write.
	/// </summary>
	public static bool SpendForRide( PlayerFile? progress, ParkItemCatalogue.Item item, int ticketsLeft )
	{
		if ( progress is null || !CanBuyRide( item, ticketsLeft ) )
			return false;

		var id = (ushort)item.Id;
		var at = progress.RideIds.BinarySearch( id );

		if ( at >= 0 )
			return false;

		progress.RideIds.Insert( ~at, id );
		progress.TicketsSpent += item.GoldenTicketCost;

		return true;
	}
}
