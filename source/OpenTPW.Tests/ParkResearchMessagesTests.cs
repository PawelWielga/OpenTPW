using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTPW.Tests;

/// <summary>
/// What a finished project tells (<c>docs/exe/park-engine.md</c>, "The lab at work"): the advisor's line of its kind,
/// rows 93 to 97, the advisor's line of a group raised, rows 133 to 136, Full Simulation's alone, and the park
/// analyser's ring of the recently researched, which the buy list's row state 2 reads. The last tests read Lost
/// Kingdom and skip without the game.
/// </summary>
[TestClass]
public class ParkResearchMessagesTests
{
	[TestInitialize]
	public void Setup() => Log ??= new();

	/// <summary>The shipped Advisor.sam's keys these rows read, and the gate's.</summary>
	private static SettingsFile Settings()
		=> new( new MemoryStream( Encoding.ASCII.GetBytes( "GeneralAdvisor.MinScoreForConsideration 25\n"
			+ "MessageGroups[0].MinTimeSameMessage 120\nMessageGroups[0].SayOnlyOnce 0\nMessageGroups[0].DiscardAfterSlaps 0\n"
			+ "ResearchRideResearched.Score 150\nResearchAddonResearched.Score 130\nResearchShopResearched.Score 130\n"
			+ "ResearchSideshowResearched.Score 110\nResearchFeatureResearched.Score 90\n"
			+ "NewResearchGroupRide.Score 40\nNewResearchGroupShop.Score 40\nNewResearchGroupSideshow.Score 40\nNewResearchGroupFeature.Score 40\n" ) ) );

	/// <summary>A ring as the shipped park holds each: nothing pushed, the allocator's 0xCDCD in its slots.</summary>
	private static ParkWorld.ResearchedRing Shipped() => new( -1, 3, false, 0, [0xCDCD, 0xCDCD, 0xCDCD] );

	/// <summary>
	/// Each kind's finish posts its own row (Advisor_OnMessage's arm 0x11): rides 0x5d, shops 0x5f, sideshows 0x60,
	/// features 0x61, upgrades 0x5e, said at its key's score with its first response and sample, its group the tag's,
	/// and the item its second word; a kind past 4 posts nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the kinds in row order (shops 0x5e); a response or sample off by one; the item not handed on.</remarks>
	[TestMethod]
	public void EachKindsFinishIsItsOwnLine()
	{
		(int Kind, int Item, int Message, int Score, int Response, int Sample, int Group)[] rows =
		[
			(0, 1150, 0x5d, 150, 248, 282, 95), (1, 1212, 0x5f, 130, 251, 285, 97), (2, 1304, 0x60, 110, 253, 287, 98),
			(3, 1414, 0x61, 90, 255, 289, 99), (4, 1150, 0x5e, 130, 250, 284, 96)
		];

		foreach ( var row in rows )
		{
			var messages = new ParkAdvisorMessages( Settings(), instantAction: () => true );
			Assert.AreEqual( row.Message, ParkAdvisorMessages.ResearchedOf( row.Kind ) );
			Assert.IsTrue( messages.Post( row.Message, 820, secondary: row.Item ) );

			var slot = messages.WrittenSlots( _ => -1 ).First( written => written is { Valid: true } )!.Value;
			Assert.AreEqual( (0, row.Item, row.Message, row.Score), (slot.Thing, slot.Secondary, slot.Message, slot.Score), $"kind {row.Kind}'s slot" );

			var said = new List<(int, int)>();
			var heard = new List<(int, int, int?)>();
			messages.Tick( 820, 0, ( response, sample ) => { said.Add( (response, sample) ); return 2500; },
				( group, subject, second ) => heard.Add( (group, subject, second) ) );
			CollectionAssert.AreEqual( new[] { (row.Response, row.Sample) }, said, $"kind {row.Kind}" );
			CollectionAssert.AreEqual( new[] { (row.Group, 0, (int?)row.Item) }, heard, $"kind {row.Kind}" );
		}

		Assert.IsNull( ParkAdvisorMessages.ResearchedOf( 5 ) );
	}

	/// <summary>
	/// A second ride said 120 sweeps on takes the row's second line (response 249, sample 283); the upgrades' row has
	/// one line, so it says 250 again.
	/// </summary>
	[TestMethod]
	public void TheRidesLinesTakeTurns()
	{
		var messages = new ParkAdvisorMessages( Settings(), instantAction: () => true );
		var said = new List<int>();

		foreach ( var (message, tick) in new[] { (0x5d, 820), (0x5d, 1400), (0x5e, 2000), (0x5e, 2600) } )
		{
			messages.Post( message, tick, secondary: 1150 );
			messages.Tick( tick, tick * 100L, ( _, sample ) => { said.Add( sample ); return 2500; } );
		}

		CollectionAssert.AreEqual( new[] { 282, 283, 284, 284 }, said );
	}

	/// <summary>
	/// A group raised posts 0x85 + kind (Lab_RaiseHighestGroup 0x00503c8e). Its row is Full Simulation's alone: in
	/// Instant Action FUN_0059bf20 answers -1, and the post is slotted at -1 all the same and never said, as the
	/// original's stock park held it (q702/orig a-advisor.log); in Full Simulation it is said at 40, sample 432.
	/// </summary>
	/// <remarks><b>Mutations:</b> the filter left out of the post (40 in Instant Action); the post refused at -1.</remarks>
	[TestMethod]
	public void AGroupRaisedIsSlottedAtMinusOneInInstantActionAndSaidInFullSimulation()
	{
		var instant = new ParkAdvisorMessages( Settings(), instantAction: () => true );
		Assert.IsTrue( instant.Post( ParkAdvisorMessages.NewRideGroup, 820 ) );
		var held = instant.WrittenSlots( _ => -1 ).First( written => written is { Valid: true } )!.Value;
		Assert.AreEqual( (0x85, -1, 0), (held.Message, held.Score, held.Secondary) );
		var said = new List<int>();
		instant.Tick( 820, 0, ( _, sample ) => { said.Add( sample ); return 0; } );
		Assert.AreEqual( 0, said.Count );
		Assert.AreEqual( 1, instant.PendingCount, "it stays in its slot" );

		var full = new ParkAdvisorMessages( Settings(), instantAction: () => false );
		for ( var kind = 0; kind < 4; ++kind )
			full.Post( ParkAdvisorMessages.NewRideGroup + kind, 820 );
		var lines = new List<(int, int)>();
		var groups = new List<int>();
		for ( var turn = 0; turn < 4; ++turn )
			full.Tick( 820 + turn, turn * 100000L, ( response, sample ) => { lines.Add( (response, sample) ); return 0; }, ( group, _, _ ) => groups.Add( group ) );
		CollectionAssert.AreEquivalent( new[] { (318, 432), (320, 434), (322, 436), (324, 438) }, lines );
		CollectionAssert.AreEquivalent( new[] { 155, 156, 157, 158 }, groups );
	}

	/// <summary>
	/// The analyser's ring (FUN_004c7370's arm 0x11, FUN_004caa70): a push takes the slot after the last, wrapping to
	/// the first past the third; the item n back is that slot, or before the first push and before the wrap nothing,
	/// never what the slot held from the file. A tab past the four takes nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the slot read before the wrap (0xCDCD); the wrap at the third slot; the newest
	/// pushed into slot 0 each time; the tabs mixed.</remarks>
	[TestMethod]
	public void TheRingKeepsTheThreeNewestOfItsTab()
	{
		var rings = new ParkResearchedRings( [Shipped(), Shipped(), Shipped(), Shipped()] );

		Assert.IsFalse( rings.IsRecent( 0, 0xCDCD ), "a slot never pushed is nobody's" );
		Assert.IsTrue( rings.Push( 0, 1150 ) );
		CollectionAssert.AreEqual( new[] { 1150, 0, 0 }, Enumerable.Range( 0, 3 ).Select( back => rings.Back( 0, back ) ).ToArray() );
		Assert.IsTrue( rings.IsRecent( 0, 1150 ) );
		Assert.IsFalse( rings.IsRecent( 3, 1150 ), "another tab's ring" );

		rings.Push( 0, 1108 );
		rings.Push( 0, 1180 );
		Assert.AreEqual( new ParkWorld.ResearchedRing( 2, 3, false, 0, [1150, 1108, 1180] ).ToString(), rings.Written[0].ToString() );
		CollectionAssert.AreEqual( new ushort[] { 1150, 1108, 1180 }, rings.Written[0].Data );

		rings.Push( 0, 1109 );
		var written = rings.Written[0];
		Assert.AreEqual( (0, true), (written.Current, written.Wrapped) );
		CollectionAssert.AreEqual( new ushort[] { 1109, 1108, 1180 }, written.Data );
		CollectionAssert.AreEqual( new[] { 1109, 1180, 1108 }, Enumerable.Range( 0, 3 ).Select( back => rings.Back( 0, back ) ).ToArray() );
		Assert.IsFalse( rings.IsRecent( 0, 1150 ), "pushed out by the fourth" );

		Assert.IsFalse( rings.Push( 4, 1150 ) );
		Assert.IsTrue( Enumerable.Range( 1, 3 ).All( tab => rings.Written[tab].Current == -1 ) );
	}

	/// <summary>
	/// The shipped park's analyser holds four rings never pushed, -1, 3, unwrapped, mTemp 0, each slot 0xCDCD (FileFormats
	/// saves.md, "The four small rings"), and a park written after a push holds it, the other three the file's.
	/// </summary>
	[TestMethod]
	public void TheShippedParksRingsAreReadAndWrittenBack()
	{
		var data = GameData.Required();
		var body = new SaveReader( new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) ) ).ReadFile();
		var park = new ParkWorld( body );

		Assert.AreEqual( 4, park.RecentlyResearched!.Count );
		foreach ( var ring in park.RecentlyResearched )
		{
			Assert.AreEqual( (-1, 3, false, (ushort)0), (ring.Current, ring.Entries, ring.Wrapped, ring.Temp) );
			CollectionAssert.AreEqual( new ushort[] { 0xCDCD, 0xCDCD, 0xCDCD }, ring.Data );
		}

		var rings = new ParkResearchedRings( park.RecentlyResearched );
		rings.Push( 2, 1304 );
		var copy = body.ToArray();
		park.PutRecentlyResearched( copy, rings.Written );
		var again = new ParkWorld( copy ).RecentlyResearched!;

		Assert.AreEqual( (0, false), (again[2].Current, again[2].Wrapped) );
		CollectionAssert.AreEqual( new ushort[] { 1304, 0xCDCD, 0xCDCD }, again[2].Data );
		Assert.AreEqual( -1, again[0].Current );
		Assert.AreEqual( body.Length, copy.Length );
		Assert.AreEqual( 6, Enumerable.Range( 0, body.Length ).Count( at => body[at] != copy[at] ),
			"mCurrentEntry's four bytes, -1 to 0, and the item's two, 0xCDCD to 1304: nothing else" );
	}

	/// <summary>
	/// A played ring's <c>mWrappedAround</c> and <c>mTemp</c> go through the file in <c>FUN_004cdb80</c>'s order: the
	/// wrapped byte at +8, then <c>mTemp</c>'s two bytes at +9. A body whose rides ring holds (2, 3, wrapped, mTemp 7,
	/// 1102, 1104, 1108), as a played park's can, reads so; a push wraps it to slot 0 and keeps <c>mTemp</c>; and the
	/// written bytes are the same three, 1, 7, 0.
	/// </summary>
	/// <remarks><b>Mutations:</b> the wrapped byte and <c>mTemp</c> swapped in the reader, or in the writer.</remarks>
	[TestMethod]
	public void AWrappedRingAndItsTempAreReadAndWrittenInTheirPlaces()
	{
		var data = GameData.Required();
		var body = new SaveReader( new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) ) ).ReadFile();
		var shipped = new ParkWorld( body );
		// The rides ring's place in the analyser's record (FUN_004c5d70).
		var at = shipped.AnalyserAt + 62500;

		var played = body.ToArray();
		byte[] ring = [2, 0, 0, 0, 3, 0, 0, 0, 1, 7, 0, 0x4e, 0x04, 0x50, 0x04, 0x54, 0x04];
		ring.CopyTo( played, at );
		var read = new ParkWorld( played ).RecentlyResearched!;

		Assert.AreEqual( (2, 3, true, (ushort)7), (read[0].Current, read[0].Entries, read[0].Wrapped, read[0].Temp) );
		CollectionAssert.AreEqual( new ushort[] { 1102, 1104, 1108 }, read[0].Data );

		var rings = new ParkResearchedRings( read );
		rings.Push( 0, 1150 );
		var written = body.ToArray();
		shipped.PutRecentlyResearched( written, rings.Written );

		CollectionAssert.AreEqual( new byte[] { 0, 0, 0, 0, 3, 0, 0, 0, 1, 7, 0, 0x7e, 0x04, 0x50, 0x04, 0x54, 0x04 }, written[at..(at + 17)],
			"slot 0 takes 1150, wrapped stays 1 at +8, mTemp 7 at +9" );
		var again = new ParkWorld( written ).RecentlyResearched!;
		Assert.AreEqual( (0, true, (ushort)7), (again[0].Current, again[0].Wrapped, again[0].Temp) );
		CollectionAssert.AreEqual( new[] { 1150, 1108, 1104 }, Enumerable.Range( 0, 3 ).Select( back => new ParkResearchedRings( again ).Back( 0, back ) ).ToArray() );
	}

	/// <summary>
	/// The level's hooks on the lab (Level.HearResearch) over the stock park: a finish goes into the ring of the item's
	/// buy list tab, not its project's kind, so an upgrade of the Dino Karts (kind 4) stars the rides tab (0x004c73c3);
	/// a group raised posts its advisor line; and the end marks the player's jungle record and posts row 167.
	/// </summary>
	/// <remarks><b>Mutations:</b> the ring's tab the project's kind (an upgrade pushes nothing); GroupOpened not hooked; AllDone
	/// not hooked, or the end with another theme.</remarks>
	[TestMethod]
	public void TheLevelHooksTheLabToTheItemsTabTheAdvisorAndTheEnd()
	{
		var data = GameData.Required();
		FileSystem = data;
		var park = new ParkWorld( new SaveReader( new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) ) ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var state = new ParkState( park ) { AdvisorMessages = new ParkAdvisorMessages( Settings(), instantAction: () => true ) };
		var file = new PlayerFile { InstantAction = true };
		LobbyCountedGapsTests.SetCurrentPlayer( new Player( 0, "Test", file ) );

		try
		{
			Assert.IsTrue( catalogue.TryGet( 1150, out var karts ) );
			Assert.AreEqual( 0, karts.UiType );

			// The kart's upgrade alone, kind 4, a hand-out from done: 100 / 100 x 50 x 12 x 0.01 = 6.
			var lab = new ParkResearch( [], catalogue, [new( 1150, ParkResearch.Upgrade, 0, 1, 6f, 0f )],
				[new( 0, 0 ), new( 0, 0 ), new( 0, 0 ), new( 0, 0 ), new( 100, 0, 0, 0 )], workLoad: 50 );
			var ends = 0;
			Level.HearResearch( lab, state, catalogue, "jungle", () => ++ends );

			Assert.AreEqual( (1150, true), (lab.SpendPoints( 12, 820 ).Single().ItemId, lab.Projects.All( project => project.Done >= project.Cost )) );
			Assert.AreEqual( "[1150,0,0] current 0 wrapped 0", state.RecentlyResearched.Census( 0 ), "the rides tab's ring" );
			Assert.IsTrue( Enumerable.Range( 1, 3 ).All( tab => state.RecentlyResearched.Written[tab].Current == -1 ) );

			var slots = state.AdvisorMessages!.WrittenSlots( _ => -1 ).Where( written => written is { Valid: true } ).Select( written => written!.Value );
			CollectionAssert.AreEquivalent( new[] { (ParkAdvisorMessages.UpgradeResearched, 1150), (ParkAdvisorMessages.AllResearched, 0) },
				slots.Select( slot => (slot.Message, slot.Secondary) ).ToArray(), "the upgrade's line and row 167" );
			Assert.IsTrue( file.Park( "jungle" ).AllResearchCompleted );
			Assert.AreEqual( 1, ends, "the End feature follows" );

			lab.GroupOpened!( 1, 840 );
			Assert.IsTrue( state.AdvisorMessages.WrittenSlots( _ => -1 ).Any( written => written is { Valid: true } && written.Value.Message == ParkAdvisorMessages.NewRideGroup + 1 ),
				"the shops' new group" );
		}
		finally
		{
			LobbyCountedGapsTests.SetCurrentPlayer( null );
		}
	}

	/// <summary>
	/// The level's hearing of a finish over the stock park (ParkState.HearResearched): the advisor's slot holds the
	/// rides' line about no thing with the item its second word, the rides ring holds the item, and the buy list marks
	/// it 2; the Belly Bounce, which the park owns, stays 1 though pushed too; the Crazy Ape stays 0; a shop's finish
	/// stars the shop's row from the shops' ring.
	/// </summary>
	/// <remarks><b>Mutations:</b> recent before owned; the ring's tab not the item's (a shop read from the rides'); the item
	/// not handed on.</remarks>
	[TestMethod]
	public void TheStockParkHearsAFinishAndTheBuyListStarsIt()
	{
		var data = GameData.Required();
		FileSystem = data;
		var park = new ParkWorld( new SaveReader( new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) ) ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: true );
		var state = new ParkState( park ) { AdvisorMessages = new ParkAdvisorMessages( Settings(), instantAction: () => true ) };

		Assert.IsTrue( catalogue.TryGet( 1150, out var karts ) );
		Assert.IsTrue( catalogue.TryGet( 1100, out var bounce ) );
		Assert.IsTrue( catalogue.TryGet( 1101, out var ape ) );
		Assert.AreEqual( 0, ParkBuyScreen.StateOf( karts, state ) );

		state.HearResearched( 1150, 0, karts.UiType, 820 );
		state.HearResearched( 1100, 0, bounce.UiType, 840 );

		var slots = state.AdvisorMessages!.WrittenSlots( _ => -1 ).Where( written => written is { Valid: true } ).Select( written => written!.Value ).ToArray();
		CollectionAssert.AreEqual( new[] { (0, 1150, 0x5d), (0, 1100, 0x5d) }, slots.Select( slot => (slot.Thing, slot.Secondary, slot.Message) ).ToArray() );
		Assert.AreEqual( "[1100,1150,0] current 1 wrapped 0", state.RecentlyResearched.Census( 0 ) );

		Assert.AreEqual( 2, ParkBuyScreen.StateOf( karts, state ) );
		Assert.AreEqual( 1, ParkBuyScreen.StateOf( bounce, state ), "owned comes first" );
		Assert.AreEqual( 0, ParkBuyScreen.StateOf( ape, state ) );

		// A shop finished goes into the shops' ring, which its row reads.
		Assert.IsTrue( catalogue.TryGet( 1212, out var fries ) );
		Assert.AreEqual( (1, 0), (fries.UiType, ParkBuyScreen.StateOf( fries, state )) );
		state.HearResearched( 1212, 1, fries.UiType, 860 );
		Assert.AreEqual( "[1212,0,0] current 0 wrapped 0", state.RecentlyResearched.Census( 1 ) );
		Assert.AreEqual( 2, ParkBuyScreen.StateOf( fries, state ) );
	}
}
