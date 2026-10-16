namespace OpenTPW;

/// <summary>
/// The park's score out of a hundred - the original's <c>FUN_004cceb0</c>, pushed into the analyser's 16th ring
/// (<c>+0x213b0</c>) at each month's close (<see cref="ParkState.TurnTheMonth"/>), which the park status screen's graph
/// reads. <c>docs/exe/hud.md</c>, "The park score".
///
/// <para>
/// <b>Eleven counts, each capped.</b> The guests inside, a thousand at most, times 20 over a thousand; the rides times
/// three over two, 20 at most; the shops and the sideshows twice each, 10 at most; the features, 10 at most; the rides at
/// upgrade level 2 or over, 10 at most; and each of the five kinds of staff, 4 at most.
/// </para>
/// </summary>
/// <remarks>
/// The original counts the guests afresh (a guest on a cell <c>FUN_004fa990</c> passes), the objects by their item's
/// <c>Info.WhichUIType</c> (<c>FUN_004ca8e0</c>) and the staff by their type byte with <c>+3</c> nought
/// (<c>FUN_004c9960</c>, and the same walk written out for the handymen and the mechanics); all on the same close tick as
/// the 10th and 12th rings' counts, which hand theirs here.
/// </remarks>
public static class ParkScore
{
	/// <summary>The staff the score counts, a kind each (<c>FUN_004c9960</c>'s types 5, 4, 6, 7 and 8).</summary>
	public readonly record struct StaffCounts( int Handymen, int Mechanics, int Entertainers, int Guards, int Researchers );

	/// <summary>The upgrade level from which a ride counts as upgraded (<c>FUN_004ca8e0( 0, 2 )</c>, the thing's <c>+0x50</c>).</summary>
	public const int UpgradedFrom = 2;

	/// <summary>
	/// The score of <paramref name="state"/>'s objects, <paramref name="catalogue"/> naming each one's item (an item it does
	/// not name counts as nothing), <paramref name="guestsInside"/> and <paramref name="staff"/>.
	/// </summary>
	public static int Of( ParkState state, ParkItemCatalogue? catalogue, int guestsInside, StaffCounts staff )
	{
		var (rides, shops, sideshows, features, upgraded) = (0, 0, 0, 0, 0);

		if ( catalogue != null )
		{
			foreach ( var thing in state.ObjectsInChainOrder() )
			{
				if ( !catalogue.TryGet( thing.CatalogueId, out var item ) )
					continue;

				switch ( item.UiType )
				{
					case ItemDescriptionFile.Ride:
						++rides;
						if ( (byte)thing.UpgradeLevel >= UpgradedFrom )
							++upgraded;
						break;
					case ParkRideOperation.ShopUiType:
						++shops;
						break;
					case ItemDescriptionFile.SideShow:
						++sideshows;
						break;
					case ItemDescriptionFile.Feature:
						++features;
						break;
				}
			}
		}

		return From( guestsInside, rides, shops, sideshows, features, upgraded, staff );
	}

	/// <summary>The sum of the eleven capped counts, in <c>FUN_004cceb0</c>'s whole divisions.</summary>
	public static int From( int guestsInside, int rides, int shops, int sideshows, int features, int upgraded, StaffCounts staff )
		=> Math.Min( guestsInside, 1000 ) * 20 / 1000
			+ Math.Min( rides * 3 / 2, 20 )
			+ Math.Min( shops * 2, 10 )
			+ Math.Min( sideshows * 2, 10 )
			+ Math.Min( features, 10 )
			+ Math.Min( upgraded, 10 )
			+ Math.Min( staff.Handymen, 4 ) + Math.Min( staff.Mechanics, 4 ) + Math.Min( staff.Entertainers, 4 )
			+ Math.Min( staff.Guards, 4 ) + Math.Min( staff.Researchers, 4 );
}
