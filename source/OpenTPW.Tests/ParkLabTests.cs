using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The lab at work (<c>docs/exe/park-engine.md</c>, "The lab at work"): a researcher's hand-out shared among the kinds
/// with an open project, a project reaching its cost, the item's next upgrade appended, the kind's group raised and its
/// next project picked. The first tests build a theme of their own on disk; the last reads Lost Kingdom and skips
/// without the game.
/// </summary>
[TestClass]
public class ParkLabTests
{
	private string _root = null!;

	[TestInitialize]
	public void Setup()
	{
		Log ??= new();
		_root = Path.Combine( Path.GetTempPath(), $"opentpw-lab-{System.Guid.NewGuid():N}" );

		Write( "rides/Rides.sam", "Info.WhichUIType 0\n" );
		Write( "rides/kart/kart.sam", "Info.Id 1150\nInfo.Name \"Kart\"\nUpgrades[0].CostOfResearch 600\nUpgrades[1].CostOfResearch 0\nUpgrades[2].CostOfResearch 300\n" );
		Write( "rides/plain/plain.sam", "Info.Id 1108\nInfo.Name \"Plain\"\nUpgrades[0].CostOfResearch 500\n" );
		Write( "rides/kind4/kind4.sam", "Info.Id 1140\nInfo.Name \"Kind4\"\nResearch.Category 4\nUpgrades[0].CostOfResearch 700\nUpgrades[1].CostOfResearch 100\n" );
		Write( "rides/ticket/ticket.sam", "Info.Id 1190\nInfo.Name \"Ticket\"\nUsageInfo.GoldenTicketCost 5\nUpgrades[1].CostOfResearch 100\n" );
		Write( "rides/unready/unready.sam", "Info.Id 1101\nInfo.Name \"Unready\"\nUpgrades[0].CostOfResearch 200\n" );
		Write( "rides/ready/ready.sam", "Info.Id 1102\nInfo.Name \"Ready\"\nUpgrades[1].CostOfResearch 100\n" );
		Write( "upgrades/Upgrades.sam", "Info.WhichUIType 0\n" );
		Write( "upgrades/addon/addon.sam", "Info.Id 1501\nInfo.Name \"Addon\"\nResearch.Category 4\nAddOn.UpgradesId 1101\n" );
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists( _root ) )
			Directory.Delete( _root, recursive: true );
	}

	private void Write( string path, string text )
	{
		var full = Path.Combine( _root, "levels", "test", path );
		Directory.CreateDirectory( Path.GetDirectoryName( full )! );
		File.WriteAllText( full, text );
	}

	private ParkItemCatalogue Catalogue() => new( "test", new BaseFileSystem( _root ) );

	private static int Counted( string name ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == name ).Times;

	private static ParkWorld.ResearchProject Project( int item, int kind, int group, float cost, float done = 0f, int level = 0 )
		=> new( item, kind, group, level, cost, done );

	/// <summary>
	/// A hand-out is effort / the open kinds' efforts' sum × the work load × the ability × 0.01, a float: a kind whose
	/// current project is done is left out of the sum and given nothing, a kind with no current project or no effort
	/// nothing either.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the done kind's effort kept in the sum (100 / 145 makes 7.03); the share given to a kind with
	/// no current project; the work load or the ability left out; the 0.01 a double's (a different float); the share
	/// not a float before it is added.
	/// </remarks>
	[TestMethod]
	public void AHandOutIsSharedByTheOpenKindsEfforts()
	{
		ParkWorld.ResearchProject[] projects = [Project( 1150, 0, 1, 600f, 30f ), Project( 1212, 1, 1, 450f, 4.5f ), Project( 1304, 2, 1, 700f, 700f ),
			Project( 1414, 3, 1, 250f, 7.5f )];
		ParkWorld.ResearchKind[] kinds = [new( 100, 0, 0, 1 ), new( 15, 0, 1, 1 ), new( 30, 0, 2, 1 ), new( 25, 0, -1, 1 ), new( 0, 0, 3, 1 )];
		var lab = new ParkResearch( [], Catalogue(), projects, kinds, workLoad: 85 );

		Assert.AreEqual( 0, lab.SpendPoints( 12, 760 ).Count );

		CollectionAssert.AreEqual( new[] { 38.869564f, 5.830435f, 700f, 7.5f }, lab.Projects.Select( project => project.Done ).ToArray() );
		Assert.AreEqual( 1, lab.HandOuts );

		// The scale is the float nearest 0.01: 1 / 100 x 85 x 2 times it is 0.016999999, where 0.01 itself makes 0.017.
		var small = new ParkResearch( [], Catalogue(), [Project( 1150, 0, 1, 600f ), Project( 1108, 0, 1, 600f )], [new( 1, 0, 0, 1 ), new( 99, 0, 1, 1 )],
			workLoad: 85 );
		small.SpendPoints( 2, 20 );
		Assert.AreEqual( 0.016999999f, small.Projects[0].Done );
		Assert.AreNotEqual( 0.017f, small.Projects[0].Done );
	}

	/// <summary>With no kind's current project open the hand-out spends nothing.</summary>
	[TestMethod]
	public void WithNoOpenProjectNothingIsSpent()
	{
		var lab = new ParkResearch( [], Catalogue(), [Project( 1150, 0, 1, 600f, 600f )], [new( 100, 0, 0, 1 ), new( 0, 0, -1, 1 )], workLoad: 85 );

		Assert.AreEqual( 0, lab.SpendPoints( 12, 760 ).Count );
		Assert.AreEqual( 600f, lab.Projects[0].Done );
	}

	/// <summary>
	/// The sum is compared with the cost before it is stored as a float, at the sweep's 53-bit precision: 449.99997
	/// and 0.00002 make a sum under 450 that rounds onto it, so the project is kept at its cost, done, and its item is
	/// never researched. The compare on the stored float would have finished it.
	/// </summary>
	[TestMethod]
	public void ASumThatRoundsOntoTheCostIsKeptThereAndNeverFinishes()
	{
		var before = Counted( "RESEARCH_FINISHED_TO_THE_ADVISOR" );
		var lab = new ParkResearch( [], Catalogue(), [Project( 1150, 0, 1, 450f, 449.99997f ), Project( 1108, 0, 1, 1e9f )],
			[new( 1, 0, 0, 1 ), new( 499, 0, 1, 1 )], workLoad: 1 );

		Assert.AreEqual( 0, lab.SpendPoints( 1, 20 ).Count );
		Assert.AreEqual( 450f, lab.Projects[0].Done );
		Assert.IsFalse( lab.IsResearched( 1150 ) );
		Assert.AreEqual( 0, lab.Kinds[0].CurrentProject, "no finish, so no pick" );
		Assert.AreEqual( 1, lab.Unfinished( 0 ), "the other ride; the kept one counts as done" );

		Assert.AreEqual( 0, lab.SpendPoints( 1, 40 ).Count );
		Assert.IsFalse( lab.IsResearched( 1150 ), "a done project takes no more points" );
		Assert.AreEqual( before, Counted( "RESEARCH_FINISHED_TO_THE_ADVISOR" ) );
	}

	/// <summary>
	/// A finished project marks its item researched at the project's tier, raises the kind's highest group against
	/// the table at the highest itself (<c>ResearchTech[highest + 1]</c>), and picks the kind's next project: the
	/// first open one after the current at or under the group, else the first before it, else none. Each finish is
	/// counted for the advisor and the analyser, a ride's for the challenges, a raise with work left for the advisor,
	/// and the last for the end of research.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the table read at highest + 1 (no raise on the first finish); the raise's share counted over
	/// every group; the pick taking the first open project from the start; the pick ignoring the group; the pick keeping
	/// a done project; the tier not the project's; the raise's advisor count with no work left.
	/// </remarks>
	[TestMethod]
	public void AFinishedProjectMarksItsItemRaisesTheGroupAndPicksTheNext()
	{
		var counts = new[] { "RESEARCH_FINISHED_TO_THE_ADVISOR", "RESEARCH_FINISHED_TO_THE_ANALYSER", "RESEARCH_FINISHED_TO_THE_CHALLENGES",
			"RESEARCH_GROUP_TO_THE_ADVISOR", "RESEARCH_ALL_DONE" };
		int[] Since( int[] before ) => [.. counts.Select( ( name, i ) => Counted( name ) - before[i] )];
		var before = counts.Select( Counted ).ToArray();

		// Six points a hand-out: 100 / 100 x 50 x 12 x 0.01.
		ParkWorld.ResearchProject[] projects = [Project( 1101, 0, 1, 10f ), Project( 1108, 0, 3, 10f ), Project( 1150, 0, 1, 6f, level: 0 ),
			Project( 1102, 0, 1, 10f )];
		var lab = new ParkResearch( [], Catalogue(), projects, [new( 100, 0, 2, 1 )], workLoad: 50, groupOpensAt: [0, 30, 100, 80, 85] );

		var first = lab.SpendPoints( 12, 20 ).Single();
		Assert.AreEqual( new ParkResearch.Finished( 2, 1150, 0, 0, 6f, Raised: 2, Next: 3, Appended: 3 + 1, AllDone: false ), first,
			"a third done of group 1 is at least 30: group 2 opens; the kart's second tier is appended" );
		Assert.IsTrue( lab.IsResearched( 1150 ) );
		CollectionAssert.AreEqual( new[] { 1, 1, 1, 1, 0 }, Since( before ) );

		Assert.AreEqual( 0, lab.SpendPoints( 12, 40 ).Count );
		var second = lab.SpendPoints( 12, 60 ).Single();
		Assert.AreEqual( (3, -1, 0), (second.Project, second.Raised, second.Next), "none after, so the first before" );

		lab.SpendPoints( 12, 80 );
		var third = lab.SpendPoints( 12, 100 ).Single();
		Assert.AreEqual( (0, 3, 1), (third.Project, third.Raised, third.Next), "group 1 and 2 all done opens 3; three of four is under 80" );
		CollectionAssert.AreEqual( new[] { 3, 3, 3, 2, 0 }, Since( before ) );

		lab.SpendPoints( 12, 120 );
		var last = lab.SpendPoints( 12, 140 ).Single();
		Assert.AreEqual( (1, ParkResearch.MostGroup, -1, false), (last.Project, last.Raised, last.Next, last.AllDone),
			"every ride done: raised to the most, nothing left to pick; the kart's upgrade is still open" );
		CollectionAssert.AreEqual( new[] { 4, 4, 4, 2, 0 }, Since( before ), "a raise with no ride left is not counted" );
		Assert.IsTrue( lab.IsResearched( 1108 ) );
		Assert.AreEqual( 0, lab.SpendPoints( 12, 160 ).Count, "kind 4 has no effort here" );
	}

	/// <summary>
	/// A finished item's next upgrade (<c>LabProject_MakeNextUpgrade</c>): from the tier after its record's, a tier
	/// that costs nothing is marked researched and passed over, and the first that costs anything is appended as a
	/// project of kind 4, group 0, at that tier and cost; an item of kind 4 gets none.
	/// </summary>
	[TestMethod]
	public void AFinishedItemAppendsItsFirstUpgradeThatCostsAnything()
	{
		var lab = new ParkResearch( [], Catalogue(), [Project( 1150, 0, 1, 6f ), Project( 1108, 0, 1, 6f ), Project( 1140, 0, 1, 6f )],
			[new( 100, 0, 0, 1 )], workLoad: 50 );

		var kart = lab.SpendPoints( 12, 20 ).Single();
		Assert.AreEqual( 3, kart.Appended );
		Assert.AreEqual( Project( 1150, ParkResearch.Upgrade, 0, 300f, level: 2 ), lab.Projects[3] );
		Assert.AreEqual( 1, lab.TierResearched( 1150 ), "its first upgrade costs nothing" );

		var plain = lab.SpendPoints( 12, 40 ).Single();
		Assert.AreEqual( (-1, 2), (plain.Appended, lab.TierResearched( 1108 )), "no upgrade costs anything" );

		var kind4 = lab.SpendPoints( 12, 60 ).Single();
		Assert.AreEqual( (-1, 0, 4), (kind4.Appended, lab.TierResearched( 1140 ), lab.Projects.Count), "an item of kind 4 has none" );
		Assert.IsTrue( kind4.AllDone is false, "the kart's upgrade is open" );

		// An upgrade's project marks its item at the project's tier.
		var tier = new ParkResearch( [], Catalogue(), [Project( 1140, ParkResearch.Upgrade, 0, 6f, level: 1 )],
			[new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 100, 0, 0, 1 )], workLoad: 50 );
		Assert.AreEqual( (1140, 1, true), (tier.SpendPoints( 12, 20 ).Single().ItemId, tier.TierResearched( 1140 ), tier.IsResearched( 1140 )) );
	}

	/// <summary>
	/// A project of kind 4 is picked only while the item it upgrades (<c>AddOn.UpgradesId</c>, or the item itself)
	/// is researched and, where that item has a golden-ticket cost, unlocked.
	/// </summary>
	[TestMethod]
	public void AnUpgradeIsPickedOnlyForAResearchedAndUnlockedItem()
	{
		ParkWorld.ObjectControl[] records = [new( 1190, true, 0 ), new( 1102, true, 0 ), new( 1501, true, 0 ), new( 1101, false, 0 ), new( 1150, true, 0 )];
		ParkWorld.ResearchProject[] projects = [Project( 1150, 4, 0, 6f, level: 1 ), Project( 1501, 4, 0, 100f ), Project( 1190, 4, 0, 100f, level: 1 ),
			Project( 1102, 4, 0, 100f, level: 1 )];

		ParkResearch Lab() => new( records, Catalogue(), projects, [new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ),
			new( 100, 0, 0, 1 )], workLoad: 50 );

		Assert.AreEqual( 3, Lab().SpendPoints( 12, 20 ).Single().Next, "the add-on's ride is not researched, the ticket ride not unlocked" );

		var unlocked = Lab();
		unlocked.Unlocked = id => id == 1190;
		Assert.AreEqual( 2, unlocked.SpendPoints( 12, 20 ).Single().Next );
	}

	/// <summary>
	/// The shipped park run forward by its one researcher's hand-outs, grade 2's twelve points each, finishes its 24
	/// projects on the hand-outs <c>lib/labsim.py</c> gives: Dino Karts on the 95th, Stone Head on the 162nd, Super
	/// Toilet last on the 1,177th. A fresh Lost Kingdom world's kinds start where the original's file has them.
	/// </summary>
	[TestMethod]
	public void TheShippedParksLabFinishesOnTheHandOutsTheOriginalsRulesGive()
	{
		var data = GameData.Required();
		FileSystem = data;
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var park = new ParkWorld( new SaveReader( stream ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var balance = new ParkBalance( "jungle", easyMode: true );

		Assert.AreEqual( 85, park.ResearchWorkLoad );
		CollectionAssert.AreEqual( new ParkWorld.ResearchKind[] { new( 100, 49, 1, 1 ), new( 15, 6, 20, 1 ), new( 30, 14, 22, 1 ), new( 25, 11, 8, 1 ),
			new( 0, 0, -1, 1 ) }, park.ResearchKinds.ToArray() );
		CollectionAssert.AreEqual( new[] { 0, 80, 85, 85, 0, 0, 0 }, ParkResearch.GroupOpensAt( balance ), "ResearchTech[1..7]" );

		var lab = new ParkResearch( park.ObjectControlRecords, catalogue, park.ResearchProjects, park.ResearchKinds, park.ResearchWorkLoad,
			ParkResearch.GroupOpensAt( balance ) );
		var finished = new System.Collections.Generic.List<(int HandOut, int Item)>();

		while ( lab.HandOuts < 2000 && finished.Count < 24 )
			finished.AddRange( lab.SpendPoints( 12, 760 + lab.HandOuts * 20 ).Select( done => (lab.HandOuts, done.ItemId) ) );

		(int, int)[] labsim = [(95, 1150), (162, 1414), (179, 1108), (313, 1180), (362, 1429), (384, 1304), (430, 1109), (496, 1212), (497, 1113),
			(562, 1424), (631, 1160), (748, 1140), (762, 1423), (829, 1307), (859, 1110), (890, 1400), (903, 1206), (938, 1418), (986, 1417),
			(1041, 1407), (1047, 1202), (1073, 1422), (1118, 1431), (1177, 1419)];
		CollectionAssert.AreEqual( labsim, finished.ToArray() );
		Assert.AreEqual( 24, lab.Projects.Count, "no ride's upgrade costs anything in the Easy layer" );
		Assert.AreEqual( 2, lab.TierResearched( 1150 ) );
		Assert.AreEqual( 0, lab.SpendPoints( 12, 0 ).Count, "nothing left" );

		var fresh = new FreshPark( "jungle", new ParkBalance( "jungle", easyMode: false ), new ParkItemCatalogue( "jungle", data ), data );
		CollectionAssert.AreEqual( new[] { (4, 1), (41, 1), (46, 1), (17, 1), (0, 1) },
			fresh.ResearchKinds.Select( kind => (kind.CurrentProject, kind.HighestGroup) ).ToArray() );
		Assert.AreEqual( 85, fresh.ResearchWorkLoad );
	}
}
