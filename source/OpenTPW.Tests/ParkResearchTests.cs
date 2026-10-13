using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The researched flag the buy list lists by (<c>docs/exe/hud.md</c>, "What the buy list actually filters on"): the
/// save's record for an item it holds, <c>Upgrades[0].CostOfResearch</c> nought for one it lacks. The first tests build
/// a theme of their own on disk; the rest read Lost Kingdom and skip without the game.
/// </summary>
[TestClass]
public class ParkResearchTests
{
	private string _root = null!;

	[TestInitialize]
	public void Setup()
	{
		Log ??= new();
		_root = Path.Combine( Path.GetTempPath(), $"opentpw-research-{System.Guid.NewGuid():N}" );

		Write( "shops/Shops.sam", "Info.WhichUIType 1\n" );
		Write( "shops/free/free.sam", "Info.Id 1201\nInfo.Name \"Free\"\nUpgrades[0].CostOfResearch 0\n" );
		Write( "shops/dear/dear.sam", "Info.Id 1202\nInfo.Name \"Dear\"\nUpgrades[0].CostOfResearch 500\n" );
		Write( "shops/unsaved/unsaved.sam", "Info.Id 1203\nInfo.Name \"Unsaved\"\n" );
		Write( "shops/costly/costly.sam", "Info.Id 1204\nInfo.Name \"Costly\"\nUpgrades[0].CostOfResearch 300\n" );
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

	private ParkResearch Research( out ParkItemCatalogue catalogue )
	{
		catalogue = new ParkItemCatalogue( "test", new BaseFileSystem( _root ) );

		// The save says no to a free item and yes to a dear one; it holds nothing for the other two.
		return new ParkResearch(
		[
			new ParkWorld.ObjectControl( 1201, Researched: false, TierResearched: 0 ),
			new ParkWorld.ObjectControl( 1202, Researched: true, TierResearched: 2 ),
		], catalogue );
	}

	[TestMethod]
	public void TheSavesFlagWinsOverTheFile()
	{
		var research = Research( out _ );

		Assert.IsFalse( research.IsResearched( 1201 ), "the save holds it unresearched, though its file costs nothing" );
		Assert.IsTrue( research.IsResearched( 1202 ), "the save holds it researched, though its file costs 500" );
		Assert.AreEqual( 2, research.TierResearched( 1202 ) );
	}

	[TestMethod]
	public void AnItemTheSaveLacksIsSeededFromItsCost()
	{
		var research = Research( out _ );

		Assert.IsTrue( research.IsResearched( 1203 ), "no cost set reads nought: researched" );
		Assert.IsFalse( research.IsResearched( 1204 ), "a cost of 300: not researched" );
		Assert.AreEqual( 2, research.FromSave );
		Assert.AreEqual( 2, research.Seeded );
	}

	[TestMethod]
	public void TheBuyListListsOnlyResearchedItems()
	{
		var research = Research( out var catalogue );

		CollectionAssert.AreEqual( new[] { "Dear", "Unsaved" },
			ParkBuyScreen.Listed( catalogue, research, 1 ).Select( item => item.Name ).ToArray() );
	}

	[TestMethod]
	public void AGoldenTicketRideIsAMysteryUntilThePlayerUnlocksIt()
	{
		var ride = new ParkItemCatalogue.Item( 1105, "Ride", "", "ride", 1, 1, null, GoldenTicketCost: 5 );

		Assert.IsTrue( ParkBuyScreen.IsMystery( ride, null ), "nobody playing: nothing unlocked" );
		Assert.IsTrue( ParkBuyScreen.IsMystery( ride, new ushort[] { 1106 } ) );
		Assert.IsFalse( ParkBuyScreen.IsMystery( ride, new ushort[] { 1105 } ), "unlocked with tickets" );
		Assert.IsFalse( ParkBuyScreen.IsMystery( ride with { GoldenTicketCost = 0 }, null ), "no ticket cost" );
	}

	private static ParkWorld ShippedPark( BaseFileSystem data )
	{
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	[TestMethod]
	public void TheShippedParkHoldsFiftyRecordsTwentySixResearched()
	{
		var records = ShippedPark( GameData.Required() ).ObjectControlRecords;

		Assert.AreEqual( 50, records.Count );
		Assert.AreEqual( 26, records.Count( record => record.Researched ) );
		Assert.AreEqual( 17, records.Count( record => record.TierResearched == 2 ) );
		Assert.IsTrue( records.All( record => record.TierResearched == 0 || record.Researched ), "a tier only where researched" );
	}

	/// <summary>
	/// The control: in the shipped park the save's flag and the file's cost agree item for item, so
	/// this passes whichever rule is read; the tab tests below pass only if the filter is applied at all.
	/// </summary>
	[TestMethod]
	public void InTheShippedParkTheSaveAndTheFilesAgree()
	{
		var data = GameData.Required();
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var research = new ParkResearch( ShippedPark( data ).ObjectControlRecords, catalogue );

		Assert.AreEqual( 50, research.FromSave );
		Assert.AreEqual( 0, research.Seeded, "the save holds every item Instant Action catalogues" );

		foreach ( var item in catalogue.All )
			Assert.AreEqual( item.ResearchCost == 0, research.IsResearched( item.Id ), item.Name );
	}

	/// <summary>
	/// The lab's projects are read from the park file (<c>FUN_00502dc0</c>, <c>FUN_00504210</c>): the shipped
	/// Lost Kingdom park holds twenty-four, eleven of a feature's kind, and <see cref="ParkResearch.Unfinished"/>
	/// counts those of a kind whose points are under their cost (<c>FUN_00503b30</c>).
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the count read from another offset, or all 256 taken; a project's fields each read from
	/// its neighbour's place; a project of any kind counted; one with its points at or over its cost counted; the
	/// cost set against nought.
	/// </remarks>
	[TestMethod]
	public void TheShippedParksLabHoldsTwentyFourProjectsElevenOfThemFeatures()
	{
		var data = GameData.Required();
		var park = ShippedPark( data );
		var projects = park.ResearchProjects;

		Assert.AreEqual( 24, projects.Count );
		Assert.AreEqual( new ParkWorld.ResearchProject( 1140, 0, 3, 0, 700f, 0f ), projects[0] );
		Assert.AreEqual( new ParkWorld.ResearchProject( 1150, 0, 1, 0, 600f, 30f ), projects[1] );
		Assert.AreEqual( new ParkWorld.ResearchProject( 1414, 3, 1, 0, 250f, 7.5f ), projects[8] );
		Assert.AreEqual( new ParkWorld.ResearchProject( 1419, 3, 3, 0, 600f, 0f ), projects[18] );
		Assert.AreEqual( new ParkWorld.ResearchProject( 1307, 2, 3, 0, 800f, 0f ), projects[23] );
		CollectionAssert.AreEqual( new[] { 8, 3, 2, 11 }, Enumerable.Range( 0, 4 ).Select( kind => projects.Count( project => project.Category == kind ) ).ToArray() );
		Assert.IsTrue( projects.All( project => project.Done < project.Cost && project.UpgradeLevel == 0 ) );

		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var research = new ParkResearch( park.ObjectControlRecords, catalogue, projects );
		CollectionAssert.AreEqual( new[] { 8, 3, 2, 11, 0 }, Enumerable.Range( 0, 5 ).Select( research.Unfinished ).ToArray() );
		Assert.AreEqual( 0, new ParkResearch( park.ObjectControlRecords, catalogue ).Unfinished( ItemDescriptionFile.Feature ), "with no projects handed in" );

		// A project is unfinished while its points are under its cost, and is its own kind's alone.
		ParkWorld.ResearchProject Feature( float cost, float done ) => new( 1414, ItemDescriptionFile.Feature, 1, 0, cost, done );
		int Left( params ParkWorld.ResearchProject[] lab ) => new ParkResearch( [], catalogue, lab ).Unfinished( ItemDescriptionFile.Feature );
		Assert.AreEqual( 1, Left( Feature( 250f, 249.5f ) ) );
		Assert.AreEqual( 0, Left( Feature( 250f, 250f ) ), "done at its cost" );
		Assert.AreEqual( 0, Left( Feature( 250f, 300f ) ) );
		Assert.AreEqual( 1, Left( Feature( 0.5f, 0f ) ), "a cost under one point" );
		Assert.AreEqual( 0, Left( Feature( 0f, 0f ) ), "nought is not under nought" );
		Assert.AreEqual( 2, Left( Feature( 250f, 0f ), Feature( 250f, 250f ), Feature( 300f, 7.5f ), new( 1304, ItemDescriptionFile.SideShow, 1, 0, 700f, 9f ), new( 1140, ItemDescriptionFile.Ride, 3, 0, 700f, 0f ) ) );
	}

	[TestMethod]
	public void LostKingdomsTabsListWhatTheOriginalShowed()
	{
		var data = GameData.Required();
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var research = new ParkResearch( ShippedPark( data ).ObjectControlRecords, catalogue );

		string[] Tab( int tab ) => ParkBuyScreen.Listed( catalogue, research, tab ).Select( item => item.Name ).ToArray();

		// q178b/orig/s06-s09.png, the original under Proton.
		CollectionAssert.AreEqual( new[] { "Aztec Mayhem", "Belly Bounce", "Crazy Ape", "Rocky Racers" }, Tab( 0 ) );
		CollectionAssert.AreEqual( new[] { "Balloon Shop", "Burger Shop", "Drinks Shop" }, Tab( 1 ) );
		CollectionAssert.AreEqual( new[] { "Jungle Spray", "Strength Bird" }, Tab( 2 ) );
	}
}
