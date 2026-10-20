using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The golden tickets a Full Simulation park wins - <see cref="ParkGoldenTickets"/> and the awards of
/// <see cref="PlayerFile"/> (<c>docs/exe/ride-operation.md</c>, "Golden tickets"; <c>docs/exe/saves.md</c>, "Keys and
/// tickets"). The last two tests read Lost Kingdom and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkGoldenTicketTests
{
	private static readonly Dictionary<string, int> Costs = new() { ["fantasy"] = 3, ["hallow"] = 1, ["jungle"] = 1, ["space"] = 5 };

	private static bool IsPark( string name ) => Costs.ContainsKey( name );

	private static int? KeysToEnter( string name ) => Costs.TryGetValue( name, out var keys ) ? keys : null;

	/// <summary>A player of the four parks with the first key, as a new Full Simulation player stands, and no ticket.</summary>
	private static PlayerFile Player()
	{
		var player = new PlayerFile { KeysGiven = 1 };

		foreach ( var name in Costs.Keys )
			player.Park( name );

		return player;
	}

	private static readonly ParkGoldenTickets.Thresholds Jungle = new( 100, 200, 75, 150, 15000, 350, 6, 105, 90, 50, 2000 );

	/// <summary>Every measure one short of its test, or at it: nothing passes, each compare being strict.</summary>
	private static readonly ParkGoldenTickets.Measures AtEveryThreshold
		= new( 100, 200, 75f, false, 15000, 350, 6, 105, 90, 50, 2000, 99 );

	/// <summary>
	/// <b>A Local award brings a ticket, a key or a park by the count of every ticket</b>: Alexah's profile (record 0, jungle
	/// 0 and 2, one key given) holds three; jungle 1 makes four, a ticket only; then 3 five, 4 six, which with the key given
	/// is three keys, what fantasy costs to enter: ticket, key and park. A ticket held gives nought and stays.
	/// </summary>
	/// <remarks><b>Mutations:</b> the held test dropped; the key given left out; the count's remainder or the cost compare changed.</remarks>
	[TestMethod]
	public void ALocalAwardBringsATicketAKeyOrAPark()
	{
		var player = Player();
		player.RecordTickets[0] = 1;
		player.Park( "jungle" ).Tickets[0] = 1;
		player.Park( "jungle" ).Tickets[2] = 1;

		Assert.AreEqual( 0, player.AwardLocal( "jungle", 0, IsPark, KeysToEnter ), "held already" );
		Assert.AreEqual( 1, player.AwardLocal( "jungle", 1, IsPark, KeysToEnter ), "four: a ticket only" );
		Assert.AreEqual( 1, player.AwardLocal( "jungle", 3, IsPark, KeysToEnter ), "five" );
		Assert.AreEqual( 3, player.AwardLocal( "jungle", 4, IsPark, KeysToEnter ), "six and the key given: three keys, fantasy's price" );
		CollectionAssert.AreEqual( new byte[] { 1, 1, 1, 1, 1, 0 }, player.Park( "jungle" ).Tickets );
	}

	/// <summary>
	/// <b>A key no park costs is a ticket and key</b>: with no key given, the third ticket is one key, Lost Kingdom's price
	/// (3); with the key given, two keys, which no park costs (2). A park whose global.sam does not load counts no ticket.
	/// </summary>
	[TestMethod]
	public void AKeyNoParkCostsIsATicketAndKey()
	{
		var given = Player();
		var none = Player();
		none.KeysGiven = 0;

		foreach ( var player in new[] { given, none } )
		{
			player.Park( "unknown" ).Tickets[0] = 1;
			player.AwardLocal( "jungle", 0, IsPark, KeysToEnter );
			player.AwardLocal( "jungle", 1, IsPark, KeysToEnter );
		}

		Assert.AreEqual( 2, given.AwardLocal( "jungle", 2, IsPark, KeysToEnter ), "two keys: no park's price" );
		Assert.AreEqual( 3, none.AwardLocal( "jungle", 2, IsPark, KeysToEnter ), "one key: the jungle's and hallow's price" );
		Assert.AreEqual( 3, none.CountTickets( IsPark ), "the unknown park's ticket is not counted" );
	}

	/// <summary>
	/// <b>A record is won once, then moves to a park that beats its holder, and is never raised</b>: the jungle wins
	/// record 0 at 106 (the record ticket and the jungle's hold); asked again at 200 it gives nought and keeps 106; fantasy
	/// at 106 gives nought (not smaller), at 107 takes it (4) and the jungle lets go.
	/// </summary>
	/// <remarks><b>Mutations:</b> the held park raised; the compare not strict; the old holder kept; the record ticket given again.</remarks>
	[TestMethod]
	public void ARecordMovesToAParkThatBeatsItAndIsNeverRaised()
	{
		var player = Player();

		Assert.AreEqual( 1, player.AwardGlobal( "jungle", 0, 106, IsPark, KeysToEnter ) );
		Assert.AreEqual( 0, player.AwardGlobal( "jungle", 0, 200, IsPark, KeysToEnter ), "the holder asked again" );
		Assert.AreEqual( 106, player.Park( "jungle" ).RecordValues[0] );
		Assert.AreEqual( 0, player.AwardGlobal( "fantasy", 0, 106, IsPark, KeysToEnter ), "level with the holder" );
		Assert.AreEqual( 4, player.AwardGlobal( "fantasy", 0, 107, IsPark, KeysToEnter ), "moved here" );

		Assert.AreEqual( (byte)0, player.Park( "jungle" ).HoldsRecord[0] );
		Assert.AreEqual( (byte)1, player.Park( "fantasy" ).HoldsRecord[0] );
		Assert.AreEqual( 107, player.Park( "fantasy" ).RecordValues[0] );
		Assert.AreEqual( 1, player.CountTickets( IsPark ), "a move is no ticket" );
	}

	/// <summary>
	/// <b>Every test passed, in order, from a new player</b>: the six Locals, Globals 1 to 3 and the Secret, ten tickets
	/// with the results 1 1 2 1 1 3 1 1 2 1 (the third two keys; the sixth three, fantasy's; the ninth four), each counted
	/// as it was made, so only the first sees one ticket; Global 0, measured nought, never.
	/// </summary>
	/// <remarks><b>Mutations:</b> a test's order or index; Global 0 measured; the Secret's index.</remarks>
	[TestMethod]
	public void EveryTestPassedAwardsTenInOrder()
	{
		var player = Player();
		var all = new ParkGoldenTickets.Measures( 101, 201, 75.5f, true, 15001, 351, 7, 0, 91, 51, 2001, 100 );

		var won = ParkGoldenTickets.Check( all, Jungle, player, "jungle", IsPark, KeysToEnter );

		CollectionAssert.AreEqual( new[] { "Local 0", "Local 1", "Local 2", "Local 3", "Local 4", "Local 5", "Global 1", "Global 2", "Global 3", "Secret 0" },
			won.Select( w => $"{w.Kind} {w.Index}" ).ToArray() );
		CollectionAssert.AreEqual( new[] { 1, 1, 2, 1, 1, 3, 1, 1, 2, 1 }, won.Select( w => w.Result ).ToArray() );
		CollectionAssert.AreEqual( new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 9 }, won.Select( w => w.Total ).ToArray(), "each count as it was made, the secret's left out" );
		CollectionAssert.AreEqual( new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, won.Select( w => w.Every ).ToArray(), "the first is the first ticket" );
		CollectionAssert.AreEqual( new byte[] { 0, 1, 1, 1 }, player.RecordTickets );
		CollectionAssert.AreEqual( new byte[] { 1, 0 }, player.BonusTickets );
		Assert.AreEqual( 0, ParkGoldenTickets.Check( all, Jungle, player, "jungle", IsPark, KeysToEnter ).Count, "all held: nothing again" );
	}

	/// <summary>
	/// <b>Every compare is strict, and the two-part tests ask both</b>: at every threshold nothing; Local 2's happiness
	/// past 75 with 150 inside, nothing, 151 the ticket; Local 5's 351 recent visitors at six months nothing, seven the
	/// ticket; the Secret at 99 per cent nothing.
	/// </summary>
	[TestMethod]
	public void EveryCompareIsStrictAndTheTwoPartTestsAskBoth()
	{
		Assert.AreEqual( 0, ParkGoldenTickets.Check( AtEveryThreshold, Jungle, Player(), "jungle", IsPark, KeysToEnter ).Count );

		var happy = AtEveryThreshold with { Happiness = 75.5f, PeopleInPark = 150 };
		Assert.AreEqual( 0, ParkGoldenTickets.Check( happy, Jungle, Player(), "jungle", IsPark, KeysToEnter ).Count, "150 inside" );
		Assert.AreEqual( "Local 2", Only( happy with { PeopleInPark = 151 } ) );

		var recent = AtEveryThreshold with { RecentVisitors = 351 };
		Assert.AreEqual( 0, ParkGoldenTickets.Check( recent, Jungle, Player(), "jungle", IsPark, KeysToEnter ).Count, "six months" );
		Assert.AreEqual( "Local 5", Only( recent with { Months = 7 } ) );

		Assert.AreEqual( "Global 1", Only( AtEveryThreshold with { KartExcitement = 91 } ) );
		Assert.AreEqual( "Global 2", Only( AtEveryThreshold with { WaterSections = 51 } ) );
		Assert.AreEqual( "Global 3", Only( AtEveryThreshold with { CellsCovered = 2001 } ) );
		Assert.AreEqual( "Secret 0", Only( AtEveryThreshold with { PercentSecured = 100 } ) );
		Assert.AreEqual( "Global 0", Only( AtEveryThreshold with { CoasterHeight = 106 } ) );
	}

	private static string Only( ParkGoldenTickets.Measures measures )
		=> string.Join( ",", ParkGoldenTickets.Check( measures, Jungle, Player(), "jungle", IsPark, KeysToEnter ).Select( w => $"{w.Kind} {w.Index}" ) );

	/// <summary>
	/// The original's log words for each result and the count (<c>0x0075a554</c>.., <c>0x0075a5f8</c>, <c>0x0075a530</c>); a
	/// Global result 4 adds the ticket's own line by its index (<c>0x004d5512</c>).
	/// </summary>
	[TestMethod]
	public void TheAwardsAreLoggedInTheOriginalsWords()
	{
		Assert.AreEqual( "Won a Local golden ticket (type - 1, num - 4); Local ticket only; You have 5 total golden tickets",
			ParkGoldenTickets.Told( new( "Local", 4, 1, 5 ) ) );
		Assert.AreEqual( "Won a Global golden ticket (type - 4, num - 0); Global ticket award moved here; "
			+ "Global ticket award coaster height; You have 2 total golden tickets",
			ParkGoldenTickets.Told( new( "Global", 0, 4, 2 ) ) );
		Assert.AreEqual( "Won a Global golden ticket (type - 4, num - 2); Global ticket award moved here; "
			+ "Global ticket award water length; You have 3 total golden tickets",
			ParkGoldenTickets.Told( new( "Global", 2, 4, 3 ) ) );
		Assert.AreEqual( "Won a Global golden ticket (type - 2, num - 0); Global ticket and key; You have 3 total golden tickets",
			ParkGoldenTickets.Told( new( "Global", 0, 2, 3 ) ) );
		Assert.AreEqual( "Won a Secret golden ticket (type - 3, num - 0); Secret ticket, key and park; You have 8 total golden tickets",
			ParkGoldenTickets.Told( new( "Secret", 0, 3, 8 ) ) );
	}

	/// <summary>
	/// <b>The month ring reaches back only as far as it holds</b>: three months pushed, six back is nothing, two back the
	/// first; once round (145 pushed), one back from the first slot is the last.
	/// </summary>
	[TestMethod]
	public void TheMonthRingReachesBackOnlyAsFarAsItHolds()
	{
		var ring = new ParkMonthRing();
		ring.Push( 10 );
		ring.Push( 20 );
		ring.Push( 30 );

		Assert.AreEqual( 30, ring.Back( 0 ) );
		Assert.AreEqual( 10, ring.Back( 2 ) );
		Assert.IsNull( ring.Back( 3 ) );

		var round = new ParkMonthRing();

		for ( var month = 1; month <= ParkMonthRing.Months + 1; ++month )
			round.Push( month );

		Assert.AreEqual( 145, round.Back( 0 ) );
		Assert.AreEqual( 144, round.Back( 1 ), "round the end" );
		Assert.IsNull( round.Back( ParkMonthRing.Months ), "no further than the ring" );
	}

	/// <summary>
	/// <b>Local 3 asks for nothing left to research and one of every item standing</b>: a ride standing, an item no buy
	/// list shows (UI type 4) and a ride bought with tickets, none standing, pass; one project undone fails it, and so
	/// does a shop with none standing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the lab not asked; the UI type 4 or ticket items not let off; the standing count not asked.</remarks>
	[TestMethod]
	public void EverythingDoneAsksTheLabAndEveryItem()
	{
		var ride = new ParkItemCatalogue.Item( 1, "ride", "", "", 1, 1, null, UiType: ItemDescriptionFile.Ride );
		var unshown = new ParkItemCatalogue.Item( 2, "unshown", "", "", 1, 1, null, UiType: 4 );
		var mystery = new ParkItemCatalogue.Item( 3, "mystery", "", "", 1, 1, null, UiType: ItemDescriptionFile.Ride, GoldenTicketCost: 1 );
		var shop = new ParkItemCatalogue.Item( 4, "shop", "", "", 1, 1, null, UiType: 1 );
		int Standing( int id ) => id == 1 ? 1 : 0;

		Assert.IsTrue( ParkGoldenTickets.EverythingDone( 0, [ride, unshown, mystery], Standing ) );
		Assert.IsFalse( ParkGoldenTickets.EverythingDone( 1, [ride, unshown, mystery], Standing ), "a project undone" );
		Assert.IsFalse( ParkGoldenTickets.EverythingDone( 0, [ride, unshown, mystery, shop], Standing ), "a shop with none standing" );
	}

	/// <summary>
	/// <b>The lab's undone projects of every kind count</b> (<c>FUN_00503b30( 5 )</c>): one of kind 0 and one of kind 2 under
	/// their costs, one at its cost, is two.
	/// </summary>
	[TestMethod]
	public void TheLabCountsUndoneProjectsOfEveryKind()
	{
		FileSystem = GameData.Required();
		var lab = new ParkResearch( [], new ParkItemCatalogue( "jungle", FileSystem ),
			[new ParkWorld.ResearchProject( 1140, 0, 0, 0, 700, 10 ), new ParkWorld.ResearchProject( 1190, 2, 0, 0, 1300, 0 ),
				new ParkWorld.ResearchProject( 1150, 1, 0, 0, 500, 500 )] );

		Assert.AreEqual( 2, lab.UnfinishedOfAnyKind );
	}

	/// <summary>Lost Kingdom's thresholds, read by name from the two Standard.sam files (FileFormats <c>sam.md</c>), the trailing comment on <c>CoasterHeight</c> left out.</summary>
	[TestMethod]
	public void LostKingdomsThresholdsAreTheBalances()
	{
		FileSystem = GameData.Required();

		Assert.AreEqual( Jungle, ParkGoldenTickets.Thresholds.From( new ParkBalance( "jungle" ) ) );
	}

	/// <summary>
	/// <b>The shipped park covers 616 cells and watches 3 per cent</b>: 242 of the 7,001 cells not of mType 2, 7 or 30
	/// have a security word above nought, as Q141 counted the file, and 616 are of mType 9, 10, 4 or 0x15 or of a
	/// counting track record (<c>parkfile.py -T</c>, which reads the same file apart from OpenTPW).
	/// </summary>
	/// <remarks><b>Mutations:</b> a kind left out or added; the parent not taken; the cells left out counted; the word index.</remarks>
	[TestMethod]
	public void TheShippedParkCovers616CellsAndWatchesThreePerCent()
	{
		FileSystem = GameData.Required();
		using var stream = new MemoryStream( FileSystem.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var state = new ParkState( new ParkWorld( new SaveReader( stream ).ReadFile() ) );

		Assert.AreEqual( (616, 3), ParkGoldenTickets.Cells( state ) );
	}
}
