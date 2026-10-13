using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTPW.Tests;

[TestClass]
public class ParkAdvisorMessageTests
{
	private static SettingsFile Settings( int open = 26, int close = 26, int threshold = 25, int worn = 100, int mechanic = 110 )
		=> new( new MemoryStream( Encoding.ASCII.GetBytes( $"GeneralAdvisor.MinScoreForConsideration {threshold}\n"
			+ $"MessageGroups[0].MinTimeSameMessage 120\nMessageGroups[0].SayOnlyOnce 0\n"
			+ $"ParkNowOpen.Score {open}\nParkNowClosed.Score {close}\nRidesBreakdownImminent.Score {worn}\n"
			+ "RidesBreakdownNoMechanics.Score 105\nRidesBreakdownBusyMechanics.Score 107\n"
			+ $"RidesBreakdownMechanicOnIt.Score {mechanic}\n"
			+ "MessageGroups[0].DiscardAfterSlaps 0\nMessageGroups[1].MinTimeSameMessage 0\nMessageGroups[1].SayOnlyOnce 1\nMessageGroups[1].DiscardAfterSlaps 3\nHelpMessage.Score 1000\nTutorialUIPanel.AfterTime 10\nTutorialUIPanel.CutoffTime 60\nTutorialUIPanel.Score 110\nTutorialExplainTooltips.AfterTime 20\nTutorialExplainTooltips.CutoffTime 80\nTutorialExplainTooltips.Score 100\nOpenPark.ScorePerWaitingPerson 20\nClosePark.Score 99\nRidesBuildRides.Score -50\nRidesBuildRides.AdditionalPerDay 3\n"
			+ "ShopsClosed.MoreThanThisPercentClosed 25\nShopsClosed.Score 75\nSideshowsClosed.MoreThanThisPercentClosed 25\nSideshowsClosed.Score 70\n"
			+ "ToiletsClosed.MoreThanThisPercentClosed 20\nToiletsClosed.Score 75\n"
			+ "StaffroomClosed.MoreThanThisPercentClosed 25\nStaffroomClosed.Score 90\n"
			+ "RidesClosed.AtLeastThisManyRides 3\nRidesClosed.MoreThanThisPercentClosed 50\nRidesClosed.Score 80\nQueueUnconnected.Score 87\n"
			+ "StaffHireMechanics1.ScorePerWornRide 5\nStaffHireMechanics1.PoorerStateThan 25\nStaffHireMechanics2.ScorePerBrokenRide 10\n"
			+ "StaffHireMechanics3.IdealNumberOfRidesPerMechanic 4\nStaffHireMechanics3.ScorePerExtraRide 10\nWaitingTimes.DaysForStaff 30\n"
			+ "RidesPoorSelection.PoorerThan 30\nRidesPoorSelection.MoreRidesThan 3\nRidesPoorSelection.Score 30\nWaitingTimes.DaysForObjects 30\n"
			+ "ShopsPoorSelection.PoorerThan 25\nShopsPoorSelection.MoreShopsThan 2\nShopsPoorSelection.Score 50\n"
			+ "ShopsBuildShops.Score 50\nShopsBuildShops.AdditionalPerDay 2\n"
			+ "SideshowsBuildSideshows.Score -70\nSideshowsBuildSideshows.AdditionalPerDay 2\n"
			+ "SideshowsPoorSelection.PoorerThan 15\nSideshowsPoorSelection.MoreSideshowsThan 2\nSideshowsPoorSelection.Score 45\n"
			+ "FeaturesPoorSelection.PoorerThan 20\nFeaturesPoorSelection.MoreFeaturesThan 5\nFeaturesPoorSelection.Score 20\n"
			+ "FeaturesBuildFeatures.Score -40\nFeaturesBuildFeatures.AdditionalPerDay 1\n"
			+ "FeaturesResearchMore.Score 60\nSideshowsResearchMore.Score 75\n"
			+ "MessageGroups[4].MinTimeSameMessage 900\nMessageGroups[4].SayOnlyOnce 0\nMessageGroups[4].DiscardAfterSlaps 0\n"
			+ "FeaturesNoToilets.Score 90\nFeaturesNoToilets.NeedingMoreThan 60\nFeaturesNoToilets.MorePeopleThanThisNeeding 25\n"
			+ "ShopsBuildMoreShops.ShopsPerRideRatio 0.15\nShopsBuildMoreShops.WealthMoreThan 20000\nShopsBuildMoreShops.Score 70\n"
			+ "SideshowsBuildMoreSideshows.SideshowsPerRideRatio 0.2\nSideshowsBuildMoreSideshows.WealthMoreThan 20000\nSideshowsBuildMoreSideshows.Score 80\n" ) ) );

	private static void Set( SettingsFile settings, string key, int value )
	{
		var index = settings.Entries.FindIndex( pair => pair.Key == key );
		settings.Entries[index] = new SettingsPair( key, value.ToString() );
	}

	private static void Set( SettingsFile settings, string key, string value )
		=> settings.Entries[settings.Entries.FindIndex( pair => pair.Key == key )] = new SettingsPair( key, value );

	/// <summary>
	/// A park file's waiting messages stand in the advisor's slots from the load on: the shipped park's opening
	/// at 20 waits unsaid under the threshold and refuses the door's next post as its one copy; a waiting message
	/// over the threshold is said on the first sweep with its file's subject; a message of no built topic, or one
	/// asking for a line, keeps its slot and is never said; and the slots are handed back to a save as they
	/// stand, a post made here with its thing's item.
	/// </summary>
	[TestMethod]
	public void AParkFilesWaitingMessagesStandInTheSlots()
	{
		var file = new Dictionary<int, ParkWorld.AdvisorSlot>
		{
			[0] = new( 13, 1100, ParkAdvisorMessages.MechanicsBusy, 107, true, -1, false, false ),
			[1] = new( 0, 0, 190, 1000, false, -1, false, false ),
			[2] = new( 0, 0, ParkAdvisorMessages.Opened, 20, true, -1, false, false ),
			[3] = new( 0, 0, 190, 1000, true, -1, false, false ),
			[5] = new( 14, 1140, ParkAdvisorMessages.RideWornOut, 100, true, 1, false, false )
		};
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 2, messages.RestoreSlots( index => file.TryGetValue( index, out var slot ) ? slot : null ) );
		Assert.AreEqual( 4, messages.PendingCount, "two to say and two carried" );
		StringAssert.Contains( messages.Census(), "slot=69/subject=13 slot=128/subject=0 " );

		var asRead = messages.WrittenSlots( _ => 7 );
		Assert.AreEqual( 8, asRead.Count );
		Assert.AreEqual( file[0], asRead[0] );
		Assert.AreEqual( default( ParkWorld.AdvisorSlot ), asRead[1] );
		Assert.AreEqual( file[2], asRead[2] );
		Assert.IsNull( asRead[3], "an unbuilt message's slot is the file's" );
		Assert.AreEqual( default( ParkWorld.AdvisorSlot ), asRead[4] );
		Assert.IsNull( asRead[5], "a line asked for by number is the file's" );

		Assert.IsFalse( messages.PostDoor( closed: false, 900 ), "the file's copy is the one copy the slots may hold" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 900, 14 ), "the first free slot is 1" );
		var written = messages.WrittenSlots( thing => thing == 14 ? 1140 : 0 );
		Assert.AreEqual( new ParkWorld.AdvisorSlot( 14, 1140, ParkAdvisorMessages.RideWornOut, 100, true, -1, false, false ), written[1] );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 900, 15 ) );
		Assert.AreEqual( 15, messages.WrittenSlots( _ => 0 )[4]!.Value.Thing, "slot 3 is taken by the file's: the next free is 4" );

		var said = new List<(int Response, int Sample)>();
		var subjects = new List<(int Group, int Subject)>();
		for ( var sweep = 0; sweep < 6; ++sweep )
			messages.Tick( 901 + sweep, 100000L * (sweep + 1), ( response, sample ) => { said.Add( (response, sample) ); return 0; },
				( group, subject ) => subjects.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (204, 202), (201, 199), (194, 191) }, said, "by score: 110, 107, 100; the opening at 20 never" );
		CollectionAssert.AreEqual( new[] { (72, 15), (71, 13), (68, 14) }, subjects );
		Assert.AreEqual( 3, messages.PendingCount, "the opening and the two carried wait on" );
		var after = messages.WrittenSlots( _ => 0 );
		Assert.AreEqual( file[2], after[2] );
		Assert.AreEqual( default( ParkWorld.AdvisorSlot ), after[0] );

		var none = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 0, none.RestoreSlots( _ => null ) );
		Assert.IsTrue( none.PostDoor( closed: false, 900 ) );
		Assert.AreEqual( new ParkWorld.AdvisorSlot( 0, 0, ParkAdvisorMessages.Opened, 26, true, -1, false, false ), none.WrittenSlots( _ => 7 )[0],
			"a message about no thing has no item" );
	}

	/// <summary>With every slot taken and none of them one this game can say, a post has no slot to take.</summary>
	[TestMethod]
	public void APostTakesNoSlotACarriedMessageStandsIn()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 0, messages.RestoreSlots( _ => new ParkWorld.AdvisorSlot( 0, 0, 190, 1, true, -1, false, false ) ) );
		Assert.AreEqual( 8, messages.PendingCount );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.RideWornOut, 900, 14 ) );
		var forced = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 0, forced.RestoreSlots( index => new ParkWorld.AdvisorSlot( 0, 0, ParkAdvisorMessages.Opened, 30, true, -1, index % 2 == 0, index % 2 == 1 ) ) );
		Assert.AreEqual( 8, forced.PendingCount );
	}

	/// <summary>
	/// A park file's records are the advisor's own from the load on: a tutorial line said is not said again, one
	/// cut off three times is discarded, a line said inside its cooldown is refused and after it the next line
	/// of the rotation is said; a message the file has no record of keeps the constructor's; and the records are
	/// handed back to a save as they stand.
	/// </summary>
	[TestMethod]
	public void AParkFilesRecordsAreTheAdvisorsOwn()
	{
		var saved = new Dictionary<int, ParkWorld.AdvisorHistory>
		{
			[ParkAdvisorMessages.TagMessage] = new( 911, 0, true, 0 ),
			[ParkAdvisorMessages.MechanicOnHisWay] = new( 880, 0, true, 0 ),
			[ParkAdvisorMessages.RideWornOut] = new( 0, -1, false, 0 ),
			[ParkAdvisorMessages.Opened] = new( 0, 1, false, 0 )
		};
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 3, messages.Restore( id => saved.TryGetValue( id, out var record ) ? record : null ) );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=0/lastTick=911/said=1/slaps=0" );
		StringAssert.Contains( messages.Census(), "message=70/score=110/line=0/lastTick=880/said=1/slaps=0" );
		StringAssert.Contains( messages.Census(), "message=128/score=26/line=1/lastTick=0/said=0/slaps=0" );
		StringAssert.Contains( messages.Census(), "message=129/score=26/line=-1/lastTick=0/said=0/slaps=0" );

		Assert.IsFalse( messages.Post( ParkAdvisorMessages.TagMessage, 1079 ), "said once, in the file" );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 880 + 476, 13 ), "inside the 120 fours of sweeps" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 880 + 480, 13 ) );
		var said = new List<(int, int)>();
		messages.Tick( 1360, 0, ( response, sample ) => { said.Add( (response, sample) ); return 0; } );
		CollectionAssert.AreEqual( new[] { (205, 203) }, said, "the line after the file's" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.Opened, 1400 ) );
		messages.Tick( 1400, 100000, ( response, sample ) => { said.Add( (response, sample) ); return 0; } );
		Assert.AreEqual( (308, 342), said[1], "two lines: the one after line 1 is line 0" );

		var written = messages.Written();
		Assert.AreEqual( 34, written.Count );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 911, 0, true, 0 ), written[ParkAdvisorMessages.TagMessage] );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 1360, 1, true, 0 ), written[ParkAdvisorMessages.MechanicOnHisWay] );
		Assert.AreEqual( ParkWorld.AdvisorHistory.Fresh, written[ParkAdvisorMessages.Closed] );

		var cut = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 1, cut.Restore( id => id == ParkAdvisorMessages.TagMessage ? new ParkWorld.AdvisorHistory( 16604, 0, false, 3 ) : null ) );
		Assert.IsFalse( cut.Post( ParkAdvisorMessages.TagMessage, 19004 ), "cut off three times, in the file" );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 16604, 0, false, 3 ), cut.Written()[ParkAdvisorMessages.TagMessage] );

		var fresh = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 0, fresh.Restore( _ => null ) );
		Assert.IsTrue( fresh.Post( ParkAdvisorMessages.TagMessage, 1079 ) );
	}

	[TestMethod]
	public void StockDoorPostsBothMessagesButSpeaksNeitherAndRetainsSlots()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var state = new ParkState( null ) { AdvisorMessages = messages };
		state.SetParkClosed( true );
		state.SetParkClosed( false );
		// The two lines of the thousand sweeps are the poll's: the control panel's, a fresh park's on its day 11,
		// and the tips', on the first of its row's turns past day 20.
		var polled = new List<(int, int, int)>();
		for ( var i = 0; i < 1000; ++i )
			messages.Tick( i, i * 248, ( response, sample ) => { polled.Add( (i, response, sample) ); return response is 404 or 542 ? 15400 : throw new Exception( "stock speech" ); } );
		CollectionAssert.AreEqual( new[] { (264, 404, 377), (612, 542, 559) }, polled );
		Assert.AreEqual( 2, messages.PendingCount );
		Assert.AreEqual( 2, messages.Attempts );
		Assert.IsTrue( messages.Census().Contains( "message=128/score=20/line=-1" ) );
		Assert.IsTrue( messages.Census().Contains( "message=129/score=20/line=-1" ) );
	}

	[TestMethod]
	public void StockWornRideIsSpokenOfInThreeLinesThatWrap()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var said = new List<(int, int)>();
		for ( var i = 0; i < 4; ++i )
		{
			var tick = 1000 + i * 480;
			Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, tick, 13 ) );
			Assert.IsTrue( messages.Census().Contains( "pending=1 attempts=" + i + " threshold=25 slot=66/subject=13 " ), messages.Census() );
			messages.Tick( tick, i * 100000L, ( response, sample ) => { said.Add( (response, sample) ); return 0; } );
		}
		CollectionAssert.AreEqual( new[] { (194, 191), (195, 192), (196, 193), (194, 191) }, said );
		Assert.IsTrue( messages.Census().Contains( "message=66/score=100/line=0/lastTick=2440" ), messages.Census() );
	}

	/// <summary>
	/// Message 0x46's row: responses 204 to 206 are samples 202 to 204, three lines in turn, and the third's own
	/// gesture row is counted each time it is said.
	/// </summary>
	[TestMethod]
	public void StockMechanicOnHisWayIsSpokenOfInThreeLinesThatWrap()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var said = new List<(int, int)>();
		var rows = new List<int>();
		for ( var i = 0; i < 4; ++i )
		{
			var tick = 1000 + i * 480;
			var before = Times( "ADVISOR_RESPONSE_GESTURE_ROW" );
			Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, tick, 13 ) );
			Assert.IsTrue( messages.Census().Contains( "pending=1 attempts=" + i + " threshold=25 slot=70/subject=13 " ), messages.Census() );
			messages.Tick( tick, i * 100000L, ( response, sample ) => { said.Add( (response, sample) ); return 0; } );
			rows.Add( Times( "ADVISOR_RESPONSE_GESTURE_ROW" ) - before );
		}
		CollectionAssert.AreEqual( new[] { (204, 202), (205, 203), (206, 204), (204, 202) }, said );
		CollectionAssert.AreEqual( new[] { 0, 0, 1, 0 }, rows, "response 206 alone names a gesture row" );
		Assert.IsTrue( messages.Census().Contains( "message=70/score=110/line=0/lastTick=2440" ), messages.Census() );
	}

	/// <summary>
	/// Messages 0x44 and 0x45's rows: responses 198 to 200 are samples 196 to 198 and 201 to 203 are 199 to 201,
	/// three lines in turn each, scores 105 and 107, a slot a ride.
	/// </summary>
	[TestMethod]
	[DataRow( ParkAdvisorMessages.NoMechanics, 105, 198, 196 )]
	[DataRow( ParkAdvisorMessages.MechanicsBusy, 107, 201, 199 )]
	public void StockNoMechanicToComeIsSpokenOfInThreeLinesThatWrap( int message, int score, int response, int sample )
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var said = new List<(int, int)>();
		var rows = Times( "ADVISOR_RESPONSE_GESTURE_ROW" );
		for ( var i = 0; i < 4; ++i )
		{
			var tick = 1000 + i * 480;
			Assert.IsTrue( messages.Post( message, tick, 13 ) );
			Assert.IsTrue( messages.Post( message, tick, 14 ), "255 copies" );
			messages.Tick( tick, i * 100000L, ( r, s ) => { said.Add( (r, s) ); return 0; } );
			messages.Tick( tick, i * 100000L + 50000, ( r, s ) => 0 );
		}
		CollectionAssert.AreEqual( new[] { (response, sample), (response + 2, sample + 2), (response + 1, sample + 1), (response, sample) }, said );
		Assert.AreEqual( rows, Times( "ADVISOR_RESPONSE_GESTURE_ROW" ), "none of the six names a gesture row" );
		Assert.IsTrue( messages.Census().Contains( $"message={message}/score={score}/line=1/lastTick=2440" ), messages.Census() );
	}

	/// <summary>Busy mechanics, 107, are said before none, 105, and after a mechanic on his way, 110.</summary>
	[TestMethod]
	public void TheFourBreakdownMessagesAreSaidByScore()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		foreach ( var id in new[] { ParkAdvisorMessages.RideWornOut, ParkAdvisorMessages.NoMechanics, ParkAdvisorMessages.MechanicsBusy, ParkAdvisorMessages.MechanicOnHisWay } )
			Assert.IsTrue( messages.Post( id, 100, 13 ) );
		var said = new List<int>();
		for ( var i = 0; i < 4; ++i )
			messages.Tick( 100 + i, i * 2000, ( _, sample ) => { said.Add( sample ); return 0; } );
		CollectionAssert.AreEqual( new[] { 202, 199, 196, 191 }, said );
	}

	/// <summary>A mechanic on his way, 110, is said before a ride about to break down, 100, and each ride has its own slot.</summary>
	[TestMethod]
	public void AMechanicOnHisWayIsSaidBeforeAWornRideAndKeepsItsOwnHistory()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 100, 13 ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 100, 14 ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 100, 15 ), "255 copies" );
		var said = new List<int>();
		messages.Tick( 100, 0, ( _, sample ) => { said.Add( sample ); return 0; } );
		messages.Tick( 101, 2000, ( _, sample ) => { said.Add( sample ); return 0; } );
		messages.Tick( 102, 4000, ( _, sample ) => { said.Add( sample ); return 0; } );
		CollectionAssert.AreEqual( new[] { 202, 203, 191 }, said );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 579, 16 ), "last said on 101: 119 fours of sweeps ago" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicOnHisWay, 580, 16 ) );
	}

	private static int Times( string gap ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == gap ).Times;

	[TestMethod]
	public void EveryWornRideHasItsOwnSlotWhereADoorHasOne()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 100, 13 ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 100, 14 ) );
		Assert.IsTrue( messages.PostDoor( false, 100 ) );
		Assert.IsFalse( messages.PostDoor( false, 100 ) );
		Assert.AreEqual( 3, messages.PendingCount );
		var said = new List<int>();
		messages.Tick( 100, 0, ( _, sample ) => { said.Add( sample ); return 0; } );
		CollectionAssert.AreEqual( new[] { 191 }, said, "100 is over the door's 26" );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.RideWornOut, 579, 15 ), "said 479 sweeps ago" );
		Assert.IsTrue( messages.Census().Contains( "slot=66/subject=14 " ), "the second copy stays" );
		Assert.IsFalse( messages.Census().Contains( "subject=13" ) );
	}

	[TestMethod]
	public void RealStateRoutePostsUnchangedOpenAndClosedWithoutMovingRidesAgain()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		var moved = 0;
		var state = new ParkState( null ) { AdvisorMessages = messages, DoorMoved = _ => ++moved };
		var said = new List<int>();
		int Play( int response, int sample ) { said.Add( sample ); return 0; }
		state.SetParkClosed( false ); // Already open: still posts.
		messages.Tick( 0, 0, Play );
		state.SetParkClosed( true );
		messages.Tick( 1, 1000, Play );
		state.SetParkClosed( true ); // Already closed: still posts.
		messages.Tick( 2, 2000, Play );
		CollectionAssert.AreEqual( new[] { 342, 344, 345 }, said );
		Assert.AreEqual( 1, moved );
	}

	[TestMethod]
	public void EachMessageHasOnePendingCopyAndOppositeDoorDoesNotWithdrawIt()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.IsTrue( messages.PostDoor( false, 100 ) );
		Assert.IsFalse( messages.PostDoor( false, 100 ) );
		Assert.IsTrue( messages.PostDoor( true, 100 ) );
		Assert.IsFalse( messages.PostDoor( true, 100 ) );
		Assert.AreEqual( 2, messages.PendingCount );
		var said = new List<int>();
		messages.Tick( 100, 0, (r, s) => { said.Add( s ); return 0; } );
		messages.Tick( 104, 1000, (r, s) => { said.Add( s ); return 0; } );
		CollectionAssert.AreEqual( new[] { 342, 344 }, said );
	}

	[DataTestMethod]
	[DataRow( 20, 25, 0 )]
	[DataRow( 25, 25, 0 )]
	[DataRow( 26, 25, 1 )]
	[DataRow( 26, 26, 0 )]
	[DataRow( 27, 26, 1 )]
	public void TickUsesStrictDataThreshold( int score, int threshold, int expected )
	{
		var messages = new ParkAdvisorMessages( Settings( score, threshold: threshold ) );
		messages.PostDoor( false, 100 );
		var calls = 0;
		messages.Tick( 100, 0, (_, _) => { ++calls; return 0; } );
		Assert.AreEqual( expected, calls );
		Assert.AreEqual( 1 - expected, messages.PendingCount );
	}

	[DataTestMethod]
	[DataRow( 24, 0 )]
	[DataRow( 25, 1 )]
	public void HelperRechecksScoreInclusivelyAndConsumesRejectedSlot( int changedScore, int expected )
	{
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings );
		messages.PostDoor( false, 100 );
		Set( settings, "ParkNowOpen.Score", changedScore );
		messages.Tick( 100, 0, (_, _) => 0 );
		Assert.AreEqual( expected, messages.Attempts );
		Assert.AreEqual( 0, messages.PendingCount );
		Set( settings, "ParkNowOpen.Score", 26 );
		Assert.AreEqual( expected == 0, messages.PostDoor( false, 100 ) );
	}

	[TestMethod]
	public void PerMessageCooldownUsesShiftedParkSweepsAndIncludesBoundary()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		messages.PostDoor( false, 101 );
		messages.Tick( 101, 0, (_, _) => 0 );
		Assert.IsFalse( messages.PostDoor( false, 579 ) );
		Assert.IsTrue( messages.PostDoor( true, 102 ), "opposite door has its own stamp" );
		Assert.IsTrue( messages.PostDoor( false, 580 ), "(580 >> 2) - (101 >> 2) == 120" );
	}

	[TestMethod]
	public void EarlyStampDoesNotArmCooldownAndCategoryValueIsRead()
	{
		var settings = Settings();
		Set( settings, "MessageGroups[0].MinTimeSameMessage", 2 );
		var messages = new ParkAdvisorMessages( settings );
		messages.PostDoor( false, 3 );
		messages.Tick( 3, 0, (_, _) => 0 );
		Assert.IsTrue( messages.PostDoor( false, 4 ) );
		messages.Tick( 4, 1000, (_, _) => 0 );
		Assert.IsFalse( messages.PostDoor( false, 11 ) );
		Assert.IsTrue( messages.PostDoor( false, 12 ) );
	}

	[TestMethod]
	public void RotationIsSeparateWrapsAndZeroPlaybackStillAdvancesHistory()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		var said = new List<int>();
		var responses = new List<int>();
		for ( var i = 0; i < 3; ++i )
		{
			messages.PostDoor( false, 100 + i * 480 );
			messages.Tick( 100 + i * 480, i * 2000, (r, s) => { responses.Add( r ); said.Add( s ); return 0; } );
			messages.PostDoor( true, 101 + i * 480 );
			messages.Tick( 101 + i * 480, i * 2000 + 1000, (r, s) => { responses.Add( r ); said.Add( s ); return 0; } );
		}
		CollectionAssert.AreEqual( new[] { 342, 344, 343, 345, 342, 344 }, said );
		CollectionAssert.AreEqual( new[] { 308, 310, 309, 311, 308, 310 }, responses );
	}

	[TestMethod]
	public void ResponseWaitIncludesDurationPlusOneSecond()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		messages.PostDoor( false, 100 );
		messages.PostDoor( true, 100 );
		messages.Tick( 100, 50, (_, _) => 2000 );
		messages.Tick( 500, 3049, (_, _) => throw new Exception( "still waiting" ) );
		Assert.AreEqual( 1, messages.PendingCount );
		messages.Tick( 500, 3050, (_, _) => 0 );
		Assert.AreEqual( 2, messages.Attempts );
	}

	[TestMethod]
	public void HigherPriorityWinsAndEqualScoresKeepSlotOrder()
	{
		foreach ( var close in new[] { 26, 30 } )
		{
			var messages = new ParkAdvisorMessages( Settings( close: close ) );
			messages.PostDoor( false, 100 );
			messages.PostDoor( true, 100 );
			var said = 0;
			messages.Tick( 100, 0, (_, s) => { said = s; return 0; } );
			Assert.AreEqual( close == 26 ? 342 : 344, said );
		}
	}

	[TestMethod]
	public void EightSlotsReplaceOnlyTheFirstLowestWithStrictlyHigherPriority()
	{
		var settings = Settings( 20 );
		var topics = Enumerable.Range( 1, 10 ).Select( id => new ParkAdvisorMessages.Topic( id,
			id == 10 ? "ParkNowClosed.Score" : "ParkNowOpen.Score", 308, 342 ) ).ToArray();
		var messages = new ParkAdvisorMessages( settings, topics );
		for ( var id = 1; id <= 8; ++id ) Assert.IsTrue( messages.Post( id, 100 ) );
		Assert.IsFalse( messages.Post( 9, 100 ), "equal cannot replace" );
		Assert.IsTrue( messages.Post( 10, 100 ), "26 replaces the first 20" );
		Assert.AreEqual( 8, messages.PendingCount );
		messages.Tick( 100, 0, (_, _) => 0 );
		Assert.AreEqual( 7, messages.PendingCount );
		Assert.IsTrue( messages.Post( 1, 100 ), "first slot was replaced" );
		Assert.IsFalse( messages.Post( 2, 100 ), "other low slots remain" );
	}

	[TestMethod]
	public void WorldUpdateTicksMessagesOnTheIncrementedParkClock()
	{
		using var clock = new SimulationClockScope();
		FileSystem = GameData.Required();
		var state = new ParkState( null ) { AdvisorMessages = new ParkAdvisorMessages( Settings() ) };
		for ( var i = 0; i < 100; ++i ) state.AdvanceGameTick();
		var people = new ParkPeople( null, state: state );
		try
		{
			state.SetParkClosed( false );
			Time.Paused = false;
			Time.StepFrames = 0;
			GameClock.Rebase();
			Time.Update( 0 );
			GameClock.Update( false, GameClock.ParkCatchUp );
			for ( var i = 0; i < 9; ++i )
			{
				Time.Update( .031f );
				GameClock.Update( false, GameClock.ParkCatchUp );
				people.Update();
			}
			Assert.AreEqual( 1, state.AdvisorMessages.Attempts );
			Assert.IsTrue( state.AdvisorMessages.Census().Contains( "message=128/score=26/line=0/lastTick=101" ) );
		}
		finally
		{
			people.Delete();
			Entity.ApplyDeletions();
			Time.Paused = false;
			GameClock.Rebase();
		}
	}

	[TestMethod]
	public void ParkResponseDurationIncludesTheEndingClipTwiceOverall()
	{
		// A chain of 2400 ms already includes its 600 ms ending clip. The response adds it again.
		Assert.AreEqual( 4000, Advisor.ParkResponseDuration( 2400, 600 ) );
	}

	[TestMethod]
	public void CatchUpSweepsShareOneResponseClockAndDoNotConsumeTheNextMessage()
	{
		using var clock = new SimulationClockScope();
		FileSystem = GameData.Required();
		var state = new ParkState( null ) { AdvisorMessages = new ParkAdvisorMessages( Settings() ) };
		var people = new ParkPeople( null, state: state );
		try
		{
			state.SetParkClosed( false );
			state.SetParkClosed( true );
			SimulationClockScope.Frame( 2f );
			people.Update();
			Assert.AreEqual( 3, state.GameTick, "64 ticks owe eight world sweeps, and a frame runs three" );
			Assert.AreEqual( 1, state.AdvisorMessages.Attempts, "all three sweeps see one instant, so the one-second wait cannot expire" );
			Assert.AreEqual( 1, state.AdvisorMessages.PendingCount );
			Assert.IsTrue( state.AdvisorMessages.Census().Contains( "message=128/score=26/line=0/lastTick=1" ) );
		}
		finally
		{
			people.Delete();
			Entity.ApplyDeletions();
		}
	}

	/// <summary>
	/// <b>Message 0xbc, the buy screen's own line</b> (row 188 of 0x0076e300, row 284's twin: posted only, category
	/// 1, score 0x0059e8c0 = <c>HelpMessage.Score</c>, one copy, response 407 = sample 380, one line, no prop, group
	/// 383): said once, refused with the Tutorial option off, and a record apart from the tag message's.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the category nought; the score another's; the response or sample another's; two lines; a
	/// prop; a group that leaves a tag; two copies; the message's id the tag message's.
	/// </remarks>
	[TestMethod]
	public void TheBuyScreensOwnLineIsATutorialLineSaidOnce()
	{
		FileSystem = GameData.Required();
		var on = true;
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ), tutorial: () => on );
		var said = new List<(int, int)>();
		var groups = new List<(int, int)>();

		Assert.AreEqual( 0xbc, ParkAdvisorMessages.BuyScreen );
		on = false;
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.BuyScreen, 800 ), "a tutorial message, with the Tutorial option off" );
		on = true;
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.BuyScreen, 800 ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=188/subject=0 " );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.BuyScreen, 800 ), "one copy in the slots" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 800 ), "the tag message's is another message" );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 0, () => { } ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=188/subject=0 " );

		messages.Tick( 801, 5000, ( response, sample ) => { said.Add( (response, sample) ); return 18998; }, ( group, subject ) => groups.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (407, 380) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=188/score=1000/line=0/lastTick=801/said=1/slaps=0" );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=-1/lastTick=0/said=0/slaps=0" );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 407 ) );

		Assert.IsFalse( messages.Post( ParkAdvisorMessages.BuyScreen, 1400 ), "said once" );
		for ( var tick = 1401; tick < 1500; ++tick )
			messages.Tick( tick, tick * 248L, ( _, _ ) => throw new Exception( "said twice" ) );
		StringAssert.Contains( messages.Census(), "pending=0 attempts=1 " );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 801, 0, true, 0 ), messages.Written()[ParkAdvisorMessages.BuyScreen], "and kept in a park's file" );

		// Cut off and said again: its one line each time.
		var cut = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ), tutorial: () => true );
		said.Clear();
		Assert.IsTrue( cut.Post( ParkAdvisorMessages.BuyScreen, 800 ) );
		cut.Tick( 801, 5000, ( response, sample ) => { said.Add( (response, sample) ); return 18998; } );
		Assert.IsTrue( cut.Withdraw( ParkAdvisorMessages.BuyScreen, 8000, () => { } ) );
		Assert.IsTrue( cut.Post( ParkAdvisorMessages.BuyScreen, 813 ) );
		cut.Tick( 814, 8000, ( response, sample ) => { said.Add( (response, sample) ); return 18998; } );
		CollectionAssert.AreEqual( new[] { (407, 380), (407, 380) }, said );
		StringAssert.Contains( cut.Census(), "message=188/score=1000/line=0/lastTick=814/said=1/slaps=1" );
	}

	/// <summary>
	/// <b>Message 0xbd, the hire screen's own line</b> (row 189 of 0x0076e300, row 188's twin: posted only, category
	/// 1, score 0x0059e8c0 = <c>HelpMessage.Score</c>, one copy, response 408 = sample 381, one line, no prop, group
	/// 383): said once, refused with the Tutorial option off, a record apart from the buy screen's; and the shipped
	/// park's record of it, cut off once on tick 55, leaves it two more cuts.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the category nought; the score another's; the response or sample the buy screen's; two
	/// lines; a prop; a group that leaves a tag; two copies; the message's id the buy screen's.
	/// </remarks>
	[TestMethod]
	public void TheHireScreensOwnLineIsATutorialLineSaidOnce()
	{
		FileSystem = GameData.Required();
		var on = true;
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ), tutorial: () => on );
		var said = new List<(int, int)>();
		var groups = new List<(int, int)>();

		Assert.AreEqual( 0xbd, ParkAdvisorMessages.HireScreen );
		on = false;
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.HireScreen, 800 ), "a tutorial message, with the Tutorial option off" );
		on = true;
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.HireScreen, 800 ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=189/subject=0 " );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.HireScreen, 800 ), "one copy in the slots" );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.BuyScreen, 800 ), "the buy screen's is another message" );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.BuyScreen, 0, () => { } ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=189/subject=0 " );

		messages.Tick( 801, 5000, ( response, sample ) => { said.Add( (response, sample) ); return 18331; }, ( group, subject ) => groups.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (408, 381) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=189/score=1000/line=0/lastTick=801/said=1/slaps=0" );
		StringAssert.Contains( messages.Census(), "message=188/score=1000/line=-1/lastTick=0/said=0/slaps=0" );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 408 ) );

		Assert.IsFalse( messages.Post( ParkAdvisorMessages.HireScreen, 1400 ), "said once" );
		for ( var tick = 1401; tick < 1500; ++tick )
			messages.Tick( tick, tick * 248L, ( _, _ ) => throw new Exception( "said twice" ) );
		StringAssert.Contains( messages.Census(), "pending=0 attempts=1 " );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 801, 0, true, 0 ), messages.Written()[ParkAdvisorMessages.HireScreen], "and kept in a park's file" );

		// The shipped park's record: cut off once. Said and cut twice more, its one line each time, and then no more.
		var shipped = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ), tutorial: () => true );
		Assert.AreEqual( 1, shipped.Restore( id => id == ParkAdvisorMessages.HireScreen ? new ParkWorld.AdvisorHistory( 55, 0, false, 1 ) : null ) );
		said.Clear();
		Assert.IsTrue( shipped.Post( ParkAdvisorMessages.HireScreen, 800 ) );
		shipped.Tick( 801, 5000, ( response, sample ) => { said.Add( (response, sample) ); return 18331; } );
		Assert.IsTrue( shipped.Withdraw( ParkAdvisorMessages.HireScreen, 8000, () => { } ) );
		StringAssert.Contains( shipped.Census(), "message=189/score=1000/line=0/lastTick=801/said=0/slaps=2" );
		Assert.IsTrue( shipped.Post( ParkAdvisorMessages.HireScreen, 813 ) );
		shipped.Tick( 814, 8000, ( response, sample ) => { said.Add( (response, sample) ); return 18331; } );
		CollectionAssert.AreEqual( new[] { (408, 381), (408, 381) }, said );
		Assert.IsTrue( shipped.Withdraw( ParkAdvisorMessages.HireScreen, 11000, () => { } ) );
		StringAssert.Contains( shipped.Census(), "message=189/score=1000/line=0/lastTick=814/said=0/slaps=3" );
		Assert.IsFalse( shipped.Post( ParkAdvisorMessages.HireScreen, 826 ), "cut off three times" );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 814, 0, false, 3 ), shipped.Written()[ParkAdvisorMessages.HireScreen] );
	}

	/// <summary>
	/// <b>Message 0x11c, the line of an opened tag message</b> (row 284 of 0x0076e300: posted only, category 1, score
	/// 0x0059e8c0 = <c>HelpMessage.Score</c>, one copy, response 544 = sample 561, one line): said once, and never
	/// posted again once he has said it through (<c>FUN_0059abc0</c>, MessageGroups[1].SayOnlyOnce). Its response's
	/// group, 383, leaves no tag. A withdrawal after the line and its second cuts nothing.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the category nought; the score another's; the response or sample another's; two lines; a
	/// group that leaves a tag; a withdrawal after the wait cutting him off; the said flag not kept.
	/// </remarks>
	[TestMethod]
	public void TheLineOfAnOpenedTagMessageIsATutorialLineSaidOnce()
	{
		FileSystem = GameData.Required();
		var messages = new ParkAdvisorMessages( new SettingsFile( "Advisor/Advisor.sam" ) );
		var said = new List<(int, int)>();
		var groups = new List<(int, int)>();
		var stops = 0;

		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 1208 ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=284/subject=0 " );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.TagMessage, 1208 ), "one copy in the slots" );
		messages.Tick( 1210, 5000, ( response, sample ) => { said.Add( (response, sample) ); return 12332; }, ( group, subject ) => groups.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (544, 561) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=0/lastTick=1210/said=1/slaps=0" );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 544 ) );

		// 5000 + 12332 + 1000: the last millisecond he is saying it is 18331.
		Assert.IsFalse( messages.Withdraw( ParkAdvisorMessages.TagMessage, 18332, () => ++stops ), "the line and its second are over" );
		Assert.AreEqual( 0, stops );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.TagMessage, 1400 ), "said once" );
		for ( var tick = 1401; tick < 1500; ++tick )
			messages.Tick( tick, tick * 248L, ( _, _ ) => throw new Exception( "said twice" ) );
		StringAssert.Contains( messages.Census(), "pending=0 attempts=1 " );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=0/lastTick=1210/said=1/slaps=0" );
	}

	/// <summary>
	/// <b>The last action in a park file</b>: a file saved inside the wait after a line carries the wait on for
	/// what was left of it by the file's own second stopwatch, and nothing is said before it ends; the action is
	/// handed back to a save as readings of that stopwatch, the same until a line is said; a line said makes it
	/// that line's start, its length and a second, and its message. A wait long over lets the first sweep speak,
	/// and an advisor with no file's action and nothing said hands back noughts.
	/// </summary>
	[TestMethod]
	public void AParkFilesLastActionCarriesTheWaitOn()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( default( ParkWorld.AdvisorAction ), messages.WrittenAction( 123456, 50000 ) );
		StringAssert.Contains( messages.Census(), " action=none/playing=0" );

		// Saved 5,000 ms into a wait of 8,000: 3,000 are left from the load, at 50,000 here.
		var file = new ParkWorld.AdvisorAction( 115150244, 8000, ParkAdvisorMessages.MechanicOnHisWay );
		Assert.AreEqual( 3000, messages.RestoreAction( file, 115155244, 50000 ) );
		StringAssert.Contains( messages.Census(), " action=45000+8000/playing=70" );
		Assert.AreEqual( file, messages.WrittenAction( 115155244, 50000 ) );
		Assert.AreEqual( file, messages.WrittenAction( 115155244 + 2500, 52500 ), "the stopwatch and this game's clock move together" );

		Assert.IsTrue( messages.Post( ParkAdvisorMessages.MechanicsBusy, 900, 13 ) );
		var said = new List<int>();
		messages.Tick( 901, 50000, ( response, _ ) => { said.Add( response ); return 4000; } );
		messages.Tick( 912, 52999, ( response, _ ) => { said.Add( response ); return 4000; } );
		Assert.AreEqual( 0, said.Count, "inside the file's wait" );
		Assert.AreEqual( 0, messages.Attempts );
		messages.Tick( 913, 53000, ( response, _ ) => { said.Add( response ); return 4000; } );
		CollectionAssert.AreEqual( new[] { 201 }, said );
		Assert.AreEqual( new ParkWorld.AdvisorAction( 115155244 + 3000, 5000, ParkAdvisorMessages.MechanicsBusy ),
			messages.WrittenAction( 115155244 + 4000, 54000 ), "the line's start, its length and a second, and its message" );
		StringAssert.Contains( messages.Census(), " action=53000+5000/playing=69" );

		// A wait long over, the shipped park's: the first sweep speaks.
		var over = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( -45367, over.RestoreAction( new ParkWorld.AdvisorAction( 114820087, 10832, 0x11a ), 114876286, 700 ) );
		Assert.IsTrue( over.Post( ParkAdvisorMessages.MechanicsBusy, 900, 13 ) );
		over.Tick( 901, 700, ( response, _ ) => { said.Add( response ); return 0; } );
		Assert.AreEqual( 2, said.Count );
		Assert.AreEqual( new ParkWorld.AdvisorAction( 114876286, 1000, ParkAdvisorMessages.MechanicsBusy ), over.WrittenAction( 114876286, 700 ) );

		// A start the file's stopwatch has not reached yet waits the whole of it, as the original's compare does.
		var ahead = new ParkAdvisorMessages( Settings() );
		Assert.AreEqual( 8100, ahead.RestoreAction( new ParkWorld.AdvisorAction( 1100, 8000, 0 ), 1000, 0 ) );
	}

	/// <summary>
	/// A withdrawal inside a file's wait finds the message the file says he was saying: he is cut off, it is
	/// cut off once more, and the action's wait is written as nought with its start as it was
	/// (<c>FUN_0059aa70</c> stores nought at <c>+0xdc</c> alone). A withdrawal of any other message, or after the
	/// wait, cuts nothing.
	/// </summary>
	[TestMethod]
	public void AWithdrawalInsideAFilesWaitCutsTheMessageTheFileNames()
	{
		var messages = new ParkAdvisorMessages( Settings() );
		var stops = 0;
		messages.RestoreAction( new ParkWorld.AdvisorAction( 2000, 12000, ParkAdvisorMessages.TagMessage ), 7000, 100000 );
		Assert.IsFalse( messages.Withdraw( ParkAdvisorMessages.Opened, 100001, () => ++stops ), "another message's line" );
		Assert.AreEqual( 0, stops );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 106999, () => ++stops ) );
		Assert.AreEqual( 1, stops );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=-1/lastTick=0/said=0/slaps=1" );
		Assert.AreEqual( new ParkWorld.AdvisorAction( 2000, 0, ParkAdvisorMessages.TagMessage ), messages.WrittenAction( 14000, 107000 ) );
		Assert.IsFalse( messages.Withdraw( ParkAdvisorMessages.TagMessage, 107000, () => ++stops ), "the wait is over" );

		var late = new ParkAdvisorMessages( Settings() );
		late.RestoreAction( new ParkWorld.AdvisorAction( 2000, 12000, ParkAdvisorMessages.TagMessage ), 7000, 100000 );
		Assert.IsFalse( late.Withdraw( ParkAdvisorMessages.TagMessage, 107000, () => ++stops ), "the last millisecond of the wait is 106,999" );
		Assert.AreEqual( 1, stops );
	}

	/// <summary>
	/// <b>A withdrawal</b> (<c>FUN_0059aa70</c>): every slot holding the message is emptied; where he is saying it,
	/// inside the line's time and its second, he is cut off, the message is no longer said, it is cut off once more
	/// and the wait is over, so a post is taken again and said on the next sweep. After as many cuts as its
	/// category's DiscardAfterSlaps, 3 for a tutorial message, the gate refuses it; a category whose
	/// DiscardAfterSlaps is nought is never refused for them.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the slot left; a cut outside the wait, or of another message's line; no stop; the said flag
	/// left; the slaps not counted, or counted with no cut; the wait left running; the cap 2 or 4, or read for
	/// category 0's nought as a cap; the other messages' slots emptied too.
	/// </remarks>
	[TestMethod]
	public void AWithdrawnMessageLeavesTheSlotsAndCutsHimOffWhereHeIsSayingIt()
	{
		var messages = new ParkAdvisorMessages( Settings( worn: 2000 ) );
		var said = new List<int>();
		var stops = 0;
		int Play( int response, int sample ) { said.Add( response ); return 9000; }

		// Withdrawn before it is said: the slot alone, and the worn ride's stays.
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 100 ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 100, 13 ) );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 0, () => ++stops ) );
		StringAssert.Contains( messages.Census(), "pending=1 attempts=0 threshold=25 slot=66/subject=13 " );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=-1/lastTick=0/said=0/slaps=0" );
		Assert.IsFalse( messages.Withdraw( ParkAdvisorMessages.TagMessage, 0, () => ++stops ), "nothing to withdraw" );

		// While he says another message's line, a withdrawal cuts nothing.
		messages.Tick( 100, 1000, Play );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 101 ) );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 2000, () => ++stops ) );
		Assert.AreEqual( 0, stops );
		StringAssert.Contains( messages.Census(), "/said=0/slaps=0" );

		// Said from 11000 (the worn ride's wait ends at 1000 + 9000 + 1000), cut at 14000.
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 102 ) );
		messages.Tick( 103, 10999, Play );
		messages.Tick( 104, 11000, Play );
		CollectionAssert.AreEqual( new[] { 194, 544 }, said );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 14000, () => ++stops ) );
		Assert.AreEqual( 1, stops );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=0/lastTick=104/said=0/slaps=1" );
		Assert.IsFalse( messages.Withdraw( ParkAdvisorMessages.TagMessage, 14001, () => ++stops ), "the wait is over: nothing more to cut" );
		Assert.AreEqual( 1, stops );

		// Posted again and said on the next sweep, with no wait; the last millisecond of a line is still a cut.
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 105 ) );
		messages.Tick( 105, 14000, Play );
		Assert.AreEqual( 3, said.Count );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 23999, () => ++stops ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 106 ), "two cuts are under the three" );
		messages.Tick( 106, 23999, Play );
		Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.TagMessage, 24000, () => ++stops ) );
		Assert.AreEqual( 3, stops );
		StringAssert.Contains( messages.Census(), "message=284/score=1000/line=0/lastTick=106/said=0/slaps=3" );
		Assert.IsFalse( messages.Post( ParkAdvisorMessages.TagMessage, 107 ), "discarded after three" );
		CollectionAssert.AreEqual( new[] { 194, 544, 544, 544 }, said, "its one line each time" );
		StringAssert.Contains( messages.Census(), "pending=0 attempts=4 " );

		// Category 0's DiscardAfterSlaps is nought: cut off four times, the worn ride's message is still taken.
		for ( var i = 0; i < 4; ++i )
		{
			Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 2000 + i * 600, 13 ), $"post {i}" );
			messages.Tick( 2000 + i * 600, 100000 + i, Play );
			Assert.IsTrue( messages.Withdraw( ParkAdvisorMessages.RideWornOut, 100000 + i, () => ++stops ) );
		}
		Assert.AreEqual( 7, stops );
		StringAssert.Contains( messages.Census(), "/said=0/slaps=4" );
	}

	/// <summary>
	/// <b>The Tutorial option off refuses a tutorial message and no other</b> (<c>FUN_0059abc0</c>'s first test,
	/// the byte at 0x0078d90d and <c>FUN_0059c610</c>), read at each post.
	/// </summary>
	/// <remarks><b>Mutations:</b> the option not read; read once at the making; refusing category 0 too; the sense turned round.</remarks>
	[TestMethod]
	public void TheTutorialOptionOffRefusesATutorialMessageAlone()
	{
		var on = false;
		var messages = new ParkAdvisorMessages( Settings(), tutorial: () => on );

		Assert.IsFalse( messages.Post( ParkAdvisorMessages.TagMessage, 100 ) );
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideWornOut, 100, 13 ) );
		on = true;
		Assert.IsTrue( messages.Post( ParkAdvisorMessages.TagMessage, 101 ) );

		var before = GameOptions.Current;
		try
		{
			GameOptions.Current = new GameOptions { Tutorial = false };
			var players = new ParkAdvisorMessages( Settings() );
			Assert.IsFalse( players.Post( ParkAdvisorMessages.TagMessage, 100 ), "the player's own option where none is given" );
			GameOptions.Current = new GameOptions { Tutorial = true };
			Assert.IsTrue( players.Post( ParkAdvisorMessages.TagMessage, 100 ) );
		}
		finally
		{
			GameOptions.Current = before;
		}
	}

	[TestMethod]
	public void CategorySayOncePreventsSecondPosting()
	{
		var settings = Settings();
		Set( settings, "MessageGroups[0].SayOnlyOnce", 1 );
		var messages = new ParkAdvisorMessages( settings );
		messages.PostDoor( false, 100 );
		messages.Tick( 100, 0, (_, _) => 0 );
		Assert.IsFalse( messages.PostDoor( false, 580 ) );
	}

	/// <summary>
	/// The wait after a response is counted in milliseconds of the running clock (<c>FUN_0059a550</c>;
	/// <c>docs/exe/advisor-park.md</c>: the tick returns early while now is under the last response plus the wait, and
	/// the wait is the line's length + 1000 ms). With no speaker the length is nought. Frames of two ticks, 62 ms,
	/// are under the frame clamp, so a sweep falls every 248 ms: the sweep at 248 ms says the first of two posted
	/// messages, the sweeps at 496, 744, 992 and 1,240 ms are inside the wait that ends at 1,248, and the sweep at
	/// 1,488 ms says the second.
	/// </summary>
	[TestMethod]
	public void TheSecondMessageWaitsOutOneSecondOfTheClockInMilliseconds()
	{
		using var clock = new SimulationClockScope();
		FileSystem = GameData.Required();
		var state = new ParkState( null ) { AdvisorMessages = new ParkAdvisorMessages( Settings() ) };
		var people = new ParkPeople( null, state: state );
		try
		{
			state.SetParkClosed( false );
			state.SetParkClosed( true );
			Assert.AreEqual( 2, state.AdvisorMessages.PendingCount );
			void Sweep()
			{
				for ( var frame = 0; frame < 4; ++frame )
				{
					SimulationClockScope.Frame( GameClock.TickSeconds * 2 );
					people.Update();
				}
			}
			Sweep();
			Assert.AreEqual( (1, 1, 1), (state.GameTick, state.AdvisorMessages.Attempts, state.AdvisorMessages.PendingCount), "248 ms: the first is said" );
			for ( var i = 0; i < 4; ++i ) Sweep();
			Assert.AreEqual( (5, 1, 1), (state.GameTick, state.AdvisorMessages.Attempts, state.AdvisorMessages.PendingCount), "1,240 ms: still inside the wait that ends at 1,248" );
			Sweep();
			Assert.AreEqual( (6, 2, 0), (state.GameTick, state.AdvisorMessages.Attempts, state.AdvisorMessages.PendingCount), "1,488 ms: the second is said" );
		}
		finally
		{
			people.Delete();
			Entity.ApplyDeletions();
			ParkState.ForgetCurrent();
		}
	}

	private static int Counted( string gap ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == gap ).Times;

	/// <summary>
	/// <b>Message 0xb9, the control panel's line, is polled</b> (row 185 of 0x0076e300: polled, category 1, score
	/// 0x005a06c0, one copy, response 404 = sample 377, one line; <c>FUN_0059c680</c>): each sweep scores the first
	/// polled row from the cursor's, of 155, and this one, the 110th from row nought, scores
	/// <c>TutorialUIPanel.Score</c> while the park is more than AfterTime and less than CutoffTime whole days old.
	/// Posted, it is said on the same sweep, about no thing and with no tag, and never again.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a polled row more or fewer; the cursor not moved on, or not wrapped; the day's bounds at
	/// or past their ends; the day a sweep's; the score nought or under the threshold posted; the gate not asked;
	/// the category nought; the response or sample another's; the unbuilt rows not counted.
	/// </remarks>
	[TestMethod]
	public void TheControlPanelsLineIsPolledOnThePollsOwnRowAndSaidOnce()
	{
		var cursor = new ParkAdvisorMessages.PollCursor();
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var said = new List<(int, int, int)>();
		var groups = new List<(int, int)>();
		int Play( int response, int sample ) { said.Add( (cursor.Row, response, sample) ); return 15400; }
		var unbuilt = Counted( "PARK_ADVISOR_POLLED_ROW_NOT_BUILT" );

		Assert.AreEqual( 155, ParkAdvisorMessages.PolledRows );
		Assert.AreEqual( 351, ParkAdvisorMessages.Rows );
		Assert.IsFalse( ParkAdvisorMessages.IsPolled( 0 ), "row nought is posted only in the running table" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 1 ) && ParkAdvisorMessages.IsPolled( 185 ) && ParkAdvisorMessages.IsPolled( 350 ) );
		Assert.IsFalse( ParkAdvisorMessages.IsPolled( 184 ) || ParkAdvisorMessages.IsPolled( 186 ) || ParkAdvisorMessages.IsPolled( 284 ) );
		Assert.AreEqual( 109, Enumerable.Range( 0, 185 ).Count( ParkAdvisorMessages.IsPolled ), "109 polled rows stand before row 185" );
		Assert.AreEqual( 10, ParkAdvisorMessages.DaysSinceTheFirstSweep( 253 ) );
		Assert.AreEqual( 11, ParkAdvisorMessages.DaysSinceTheFirstSweep( 254 ) );
		Assert.AreEqual( 59, ParkAdvisorMessages.DaysSinceTheFirstSweep( 1382 ) );
		Assert.AreEqual( 60, ParkAdvisorMessages.DaysSinceTheFirstSweep( 1383 ) );
		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ControlPanel );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 253 ), "day 10 is not after 10" );
		Assert.AreEqual( 110, messages.ScoreOf( topic, 254 ) );
		Assert.AreEqual( 110, messages.ScoreOf( topic, 1382 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1383 ), "day 60 is not before 60" );

		// A whole round on day 11 from row nought: the 110th sweep's row is built, and is said; the 148th's, row
		// 282, is built too and scores nought on day 17; the round ends on the row past the table's last.
		for ( var i = 0; i < 155; ++i )
			messages.Tick( 254 + i, (254 + i) * 250L, Play, ( group, subject ) => groups.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (186, 404, 377) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		Assert.AreEqual( unbuilt + 130, Counted( "PARK_ADVISOR_POLLED_ROW_NOT_BUILT" ) );
		Assert.AreEqual( "no-score", messages.PollAnswer( ParkAdvisorMessages.OpenPark ), "row 1 with no park handed in" );
		Assert.AreEqual( "no-score", messages.PollAnswer( ParkAdvisorMessages.ExplainTooltips ) );
		Assert.AreEqual( 351, cursor.Row );
		Assert.AreEqual( 351, messages.PollRow );
		StringAssert.Contains( messages.Census(), "message=185/score=110/line=0/lastTick=363/said=1/slaps=0" );
		StringAssert.Contains( messages.Census(), "/playing=185 poll=351" );
		Assert.AreEqual( 185, messages.WrittenAction( 1000, 0 ).Playing );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 363, 0, true, 0 ), messages.Written()[185] );

		// Past the last row the poll begins again at row 1, the first polled.
		messages.Tick( 409, 409 * 250L, ( _, _ ) => throw new Exception( "said twice" ) );
		Assert.AreEqual( 2, cursor.Row );

		// Round after round, each said once: row 282's next turn is the 147th sweep from row 2, on day 28.
		said.Clear();
		for ( var i = 0; i < 400; ++i )
			messages.Tick( 500 + i, (500 + i) * 250L, Play );
		CollectionAssert.AreEqual( new[] { (283, 542, 559) }, said );
		StringAssert.Contains( messages.Census(), "message=282/score=100/line=0/lastTick=646/said=1/slaps=0" );
		Assert.AreEqual( 2, messages.Attempts );
		Assert.AreEqual( 0, messages.PendingCount );
	}

	/// <summary>
	/// Row 1, message 1, is polled and scored by the park (0x0059c8d0): nought with the door open, nought with
	/// neither a ride nor a sideshow standing, and otherwise <c>OpenPark.ScorePerWaitingPerson</c> for each guest
	/// waiting at the booths. One waiting is 20, under the threshold; two are 40, and the line is said on its
	/// poll's own sweep in the ticket hat with tag 3, then not again inside its category's 120 quarter-sweeps,
	/// then its next line.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the door not asked, or asked the other way; the rides or the sideshows not asked, or both
	/// needed; the count not multiplied, or one more; the row not polled; the lines, the response, the sample,
	/// the prop or the group another's; the park not handed on by the sweep.
	/// </remarks>
	[TestMethod]
	public void AShutParkPeopleWaitOutsideIsPolledByHowManyWait()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 1 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: true, Rides: 1, Sideshows: 0, WaitingToOpen: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		var tick = 0;
		void Sweep( int at )
		{
			tick = at;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}
		void Round( int from )
		{
			cursor.Row = 1;
			Sweep( from );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.OpenPark );
		Assert.AreEqual( (1, 2, 1, 3, 1, 0, true, 3, ParkAdvisorMessages.TicketHat),
			(topic.Id, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 100 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 2, 3, 4 } )
			Assert.AreEqual( (ParkAdvisorMessages.TicketHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 8 ), "row 3's first line is not built" );

		// One waiting: 20, scored and under the threshold of 25.
		facts = facts with { WaitingToOpen = 1 };
		Round( 1000 );
		Assert.AreEqual( 20, messages.ScoreOf( topic, tick ) );
		Assert.AreEqual( ("under-threshold", 0, 2), (messages.PollAnswer( 1 ), said.Count, cursor.Row) );

		// The score's three ways to nought.
		facts = new( Closed: false, Rides: 1, Sideshows: 1, WaitingToOpen: 5 );
		Round( 1001 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 1 )), "an open park" );
		facts = new( Closed: true, Rides: 0, Sideshows: 0, WaitingToOpen: 5 );
		Round( 1002 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 1 )), "nothing to ride" );
		facts = new( Closed: true, Rides: 0, Sideshows: 1, WaitingToOpen: 0 );
		Round( 1003 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 1 )), "nobody waiting" );
		Assert.AreEqual( 0, said.Count );

		// A sideshow alone will do, and three waiting are 60: said on the poll's own sweep.
		facts = facts with { WaitingToOpen = 3 };
		Round( 1004 );
		CollectionAssert.AreEqual( new[] { (1004, 2, 1) }, said );
		CollectionAssert.AreEqual( new[] { (3, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=1/score=60/line=0/lastTick=1004/said=1/slaps=0" );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 1004, 0, true, 0 ), messages.Written()[1] );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		facts = new( Closed: true, Rides: 2, Sideshows: 0, WaitingToOpen: 2 );
		Round( 1004 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 1 ), said.Count) );
		Round( 1004 + 480 );
		CollectionAssert.AreEqual( new[] { (1004, 2, 1), (1484, 3, 2) }, said );
		Assert.AreEqual( 40, messages.ScoreOf( topic, tick ) );

		// A sweep that hands no park in scores it nought again.
		cursor.Row = 1;
		messages.Tick( 3000, 3000 * 250L, ( _, _ ) => throw new Exception( "said with no park" ) );
		Assert.AreEqual( "no-score", messages.PollAnswer( 1 ) );
	}

	/// <summary>
	/// Row 2, message 2, is polled and scored by the park (0x0059c960): <c>ClosePark.Score</c> in an open park with
	/// neither a ride nor a sideshow standing, and nought with the door shut, with a ride or with a sideshow. The
	/// line is said on its poll's own sweep in the ticket hat with tag 4, then not again inside its category's 120
	/// quarter-sweeps, then its next line.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the door not asked, or asked the other way; the rides or the sideshows not asked, or one
	/// alone enough; the score another key's, or a count's; the row not polled; the lines, the response, the
	/// sample, the prop or the group another's; tag 4's row another's.
	/// </remarks>
	[TestMethod]
	public void AnOpenParkWithNothingToRideIsPolledAndToldToClose()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 2 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 0, Sideshows: 0, WaitingToOpen: 7, Shops: 1, Toilets: 3 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		var tick = 0;
		void Round( int at )
		{
			cursor.Row = 2;
			tick = at;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ClosePark );
		Assert.AreEqual( (2, "ClosePark.Score", 5, 4, 3, 1, 0, true, 4, ParkAdvisorMessages.TicketHat),
			(topic.Id, topic.ScoreKey, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 100 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 5, 6, 7 } )
			Assert.AreEqual( (ParkAdvisorMessages.TicketHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 4 );
		Assert.AreEqual( (3, 2, 8, 0x18, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// The score's three ways to nought.
		facts = facts with { Closed = true };
		Round( 1001 );
		Assert.AreEqual( (0, "no-score", 3), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 2 ), cursor.Row), "a shut park" );
		facts = facts with { Closed = false, Rides = 1 };
		Round( 1002 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 2 )), "a ride standing" );
		facts = facts with { Rides = 0, Sideshows = 1 };
		Round( 1003 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, tick ), messages.PollAnswer( 2 )), "a sideshow standing" );
		Assert.AreEqual( 0, said.Count );

		// Open with neither: the key's 99 whoever waits, said on the poll's own sweep with tag 4.
		facts = facts with { Sideshows = 0 };
		Round( 1004 );
		Assert.AreEqual( 99, messages.ScoreOf( topic, tick ) );
		CollectionAssert.AreEqual( new[] { (1004, 5, 4) }, said );
		CollectionAssert.AreEqual( new[] { (4, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=2/score=99/line=0/lastTick=1004/said=1/slaps=0" );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 1004, 0, true, 0 ), messages.Written()[2] );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		Round( 1004 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 2 ), said.Count) );
		Round( 1004 + 480 );
		CollectionAssert.AreEqual( new[] { (1004, 5, 4), (1484, 6, 5) }, said );
		Round( 1004 + 960 );
		Round( 1004 + 1440 );
		CollectionAssert.AreEqual( new[] { (1004, 5, 4), (1484, 6, 5), (1964, 7, 6), (2444, 5, 4) }, said, "three lines in turn, then the first again" );
		CollectionAssert.AreEqual( new[] { (4, 0), (4, 0), (4, 0), (4, 0) }, groups );

		// A sweep that hands no park in scores it nought.
		cursor.Row = 2;
		messages.Tick( 3000, 3000 * 250L, ( _, _ ) => throw new Exception( "said with no park" ) );
		Assert.AreEqual( "no-score", messages.PollAnswer( 2 ) );
	}

	/// <summary>
	/// Row 62, message 0x3e, is polled and scored by the park (0x0059dad0): nought where a ride stands, and
	/// otherwise <c>RidesBuildRides.Score</c>, -50, and <c>AdditionalPerDay</c>, 3, for each whole day of the
	/// park's age, whatever the door and the sideshows: 73 on tick 959, 94 on tick 1114, 25 on the park's 25th day,
	/// which is not above the threshold, and 28 on its 26th, which is. The line is said in the hard hat with tag
	/// 64, whose bar carries the buy button of type 9, then not again inside its category's 120 quarter-sweeps.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides not asked, or the sideshows or the door asked; the days left out, or counted
	/// from another tick; either key another's; the row not polled; the lines, the response, the sample, the prop
	/// or the group another's; tag 64's row another's.
	/// </remarks>
	[TestMethod]
	public void AParkWithNoRideIsPolledAndToldToBuildSome()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 62 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 1, Sideshows: 0, WaitingToOpen: 0, Shops: 1, Toilets: 3 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 62;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildRides );
		Assert.AreEqual( (0x3e, "RidesBuildRides.Score", "RidesBuildRides.AdditionalPerDay", 182, 179, 3, 1, 0, true, 64, ParkAdvisorMessages.HardHat),
			(topic.Id, topic.ScoreKey, topic.PerDayWithNoRide, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 959 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 182, 183, 184 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 64 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 67, 9, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// A ride standing: nought, however old the park.
		Round( 959 );
		Assert.AreEqual( (0, "no-score", 63, 0), (messages.ScoreOf( topic, 959 ), messages.PollAnswer( 62 ), cursor.Row, said.Count) );

		// None: the two keys and the days, whatever the door and the sideshows.
		facts = facts with { Rides = 0 };
		Assert.AreEqual( (73, 94), (messages.ScoreOf( topic, 959 ), messages.ScoreOf( topic, 1114 )), "days 41 and 48" );
		Assert.AreEqual( (-50, -47, 25, 28), (messages.ScoreOf( topic, 23 ), messages.ScoreOf( topic, 24 ), messages.ScoreOf( topic, 576 ), messages.ScoreOf( topic, 600 )),
			"day 0, day 1, day 25 and day 26" );
		facts = facts with { Closed = true, Sideshows = 2 };
		Assert.AreEqual( 73, messages.ScoreOf( topic, 959 ), "a shut park with sideshows" );

		// Under the threshold, at it, and over it.
		Round( 23 );
		Assert.AreEqual( "no-score", messages.PollAnswer( 62 ) );
		Round( 576 );
		Assert.AreEqual( ("under-threshold", 0), (messages.PollAnswer( 62 ), said.Count), "25 is not above 25" );
		Round( 960 );
		CollectionAssert.AreEqual( new[] { (960, 182, 179) }, said );
		CollectionAssert.AreEqual( new[] { (64, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=62/score=73/line=0/lastTick=960/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		Round( 960 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 62 ), said.Count) );
		Round( 960 + 480 );
		Round( 960 + 960 );
		Round( 960 + 1440 );
		CollectionAssert.AreEqual( new[] { (960, 182, 179), (1440, 183, 180), (1920, 184, 181), (2400, 182, 179) }, said, "three lines in turn, then the first again" );

		// A ride bought: nought again.
		facts = facts with { Rides = 1 };
		Round( 2400 + 480 );
		Assert.AreEqual( ("no-score", 4), (messages.PollAnswer( 62 ), said.Count) );
	}

	/// <summary>
	/// Row 5, message 5, is polled and scored by the park (0x0059ca30): nought where a mechanic is in the park, and
	/// otherwise <c>StaffHireMechanics1.ScorePerWornRide</c>, 5, for each ride worn or broken down: 25 for five,
	/// which is not above the threshold, and 30 for six, which is. The line is said in the hard hat with tag 7,
	/// whose bar carries the hire button of type 0xe, then not again inside its category's 120 quarter-sweeps. The
	/// limit a ride is worn under is the section's <c>PoorerStateThan</c>.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the mechanics not asked, or one needed; the worn rides not counted, or every ride counted;
	/// the key another's; the row not polled; the lines, the response, the sample, the prop or the group another's;
	/// tag 7's row another's; the limit another key's.
	/// </remarks>
	[TestMethod]
	public void WornRidesInAParkWithNoMechanicArePolledAndToldToHireOne()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 5 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 7, Sideshows: 1, WaitingToOpen: 0, Shops: 1, Toilets: 3, Mechanics: 1, RidesWorn: 6 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 5;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.HireMechanics );
		Assert.AreEqual( (5, "StaffHireMechanics1.ScorePerWornRide", true, 14, 13, 3, 1, 0, true, 7, ParkAdvisorMessages.HardHat),
			(topic.Id, topic.ScoreKey, topic.PerWornRideWithNoMechanic, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.PerWornRideWithNoMechanic ) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 5 ) );
		Assert.AreEqual( 25, messages.WornBelow, "StaffHireMechanics1.PoorerStateThan" );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 927 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 14, 15, 16 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 7 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 11, 0xe, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// A mechanic in the park: nought, however many rides are worn.
		Round( 927 );
		Assert.AreEqual( (0, "no-score", 6, 0), (messages.ScoreOf( topic, 927 ), messages.PollAnswer( 5 ), cursor.Row, said.Count) );

		// None: the key for each worn ride, whatever else the park holds.
		facts = facts with { Mechanics = 0, RidesWorn = 0 };
		Round( 928 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, 928 ), messages.PollAnswer( 5 )), "no mechanic and nothing worn" );
		facts = facts with { RidesWorn = 1 };
		Assert.AreEqual( 5, messages.ScoreOf( topic, 928 ) );
		facts = facts with { RidesWorn = 5 };
		Round( 929 );
		Assert.AreEqual( (25, "under-threshold", 0), (messages.ScoreOf( topic, 929 ), messages.PollAnswer( 5 ), said.Count), "25 is not above 25" );
		facts = facts with { RidesWorn = 6, Closed = true, Rides = 6 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 932 ) );
		Round( 932 );
		CollectionAssert.AreEqual( new[] { (932, 14, 13) }, said );
		CollectionAssert.AreEqual( new[] { (7, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=5/score=30/line=0/lastTick=932/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		Round( 932 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 5 ), said.Count) );
		Round( 932 + 480 );
		Round( 932 + 960 );
		Round( 932 + 1440 );
		CollectionAssert.AreEqual( new[] { (932, 14, 13), (1412, 15, 14), (1892, 16, 15), (2372, 14, 13) }, said, "three lines in turn, then the first again" );

		// A mechanic hired: nought again.
		facts = facts with { Mechanics = 1 };
		Round( 2372 + 480 );
		Assert.AreEqual( ("no-score", 4), (messages.PollAnswer( 5 ), said.Count) );
	}

	/// <summary>
	/// Row 6, message 6, is polled and scored by the park (0x0059ca80): nought where a mechanic is in the park, and
	/// otherwise <c>StaffHireMechanics2.ScorePerBrokenRide</c>, 10, for each ride broken down, a worn one not
	/// counted: 20 for two, which is not above the threshold, and 30 for three, which is. The line is said in the
	/// hard hat with tag 8, whose bar carries the hire button of type 0xe, then not again inside its category's 120
	/// quarter-sweeps; its second line's response names a gesture row, which is counted.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the mechanics not asked, or one needed; the broken rides not counted, the worn rides
	/// counted for them, or every ride; the key row 5's; the row not polled; the lines, the response, the sample,
	/// the prop, the group or the gesture line another's; tag 8's row another's; row 5 scored by the broken rides; the poll line naming the worn rides.
	/// </remarks>
	[TestMethod]
	public void BrokenRidesInAParkWithNoMechanicArePolledAndToldToHireOne()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 6 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 7, Sideshows: 1, WaitingToOpen: 0, Shops: 1, Toilets: 3, Mechanics: 1, RidesWorn: 3, RidesBroken: 3 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 6;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.HireMechanicsBroken );
		Assert.AreEqual( (6, "StaffHireMechanics2.ScorePerBrokenRide", true, false, 17, 16, 3, 1, 0, true, 8, ParkAdvisorMessages.HardHat, 1),
			(topic.Id, topic.ScoreKey, topic.PerBrokenRideWithNoMechanic, topic.PerWornRideWithNoMechanic, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.PerBrokenRideWithNoMechanic ) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 6 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1083 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 17, 18, 19 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 8 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 12, 0xe, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// A mechanic in the park: nought, however many rides are broken down.
		Round( 1083 );
		Assert.AreEqual( (0, "no-score", 7, 0), (messages.ScoreOf( topic, 1083 ), messages.PollAnswer( 6 ), cursor.Row, said.Count) );

		// None: the key for each broken ride, and nothing for a ride that is only worn.
		facts = facts with { Mechanics = 0, RidesBroken = 0, RidesWorn = 6 };
		Round( 1084 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, 1084 ), messages.PollAnswer( 6 )), "no mechanic, six worn and none broken down" );
		var worn = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.HireMechanics );
		facts = facts with { RidesBroken = 1, RidesWorn = 0 };
		Assert.AreEqual( (10, 0), (messages.ScoreOf( topic, 1084 ), messages.ScoreOf( worn, 1084 )), "row 5 does not read the broken count" );
		facts = facts with { RidesBroken = 2, RidesWorn = 2 };
		Round( 1085 );
		Assert.AreEqual( (20, "under-threshold", 0), (messages.ScoreOf( topic, 1085 ), messages.PollAnswer( 6 ), said.Count), "20 is not above 25" );
		facts = facts with { RidesWorn = 4 };
		Assert.AreEqual( (" broken=2 mechanics=0", " worn=4 mechanics=0"), (messages.PollFacts( topic ), messages.PollFacts( worn )), "each poll line names what its score read" );
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		facts = facts with { RidesBroken = 3, RidesWorn = 3, Closed = true, Rides = 3 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 1088 ) );
		Round( 1088 );
		CollectionAssert.AreEqual( new[] { (1088, 17, 16) }, said );
		CollectionAssert.AreEqual( new[] { (8, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=6/score=30/line=0/lastTick=1088/said=1/slaps=0" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "the first line's gesture is the random chain" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		Round( 1088 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 6 ), said.Count) );
		Round( 1088 + 480 );
		Assert.AreEqual( gestures + 1, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "the second line's response names a gesture row" );
		Round( 1088 + 960 );
		Round( 1088 + 1440 );
		Assert.AreEqual( gestures + 1, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ) );
		CollectionAssert.AreEqual( new[] { (1088, 17, 16), (1568, 18, 17), (2048, 19, 18), (2528, 17, 16) }, said, "three lines in turn, then the first again" );

		// A mechanic hired: nought again.
		facts = facts with { Mechanics = 1 };
		Round( 2528 + 480 );
		Assert.AreEqual( ("no-score", 4), (messages.PollAnswer( 6 ), said.Count) );
	}

	/// <summary>
	/// Row 7, message 7, is polled and scored by the rides over those the park's mechanics keep (0x0059cac0):
	/// nought until WaitingTimes.DaysForStaff whole days after the newest member of staff was made, nought where
	/// the rides are no more than StaffHireMechanics3.IdealNumberOfRidesPerMechanic for each mechanic, and
	/// ScorePerExtraRide for each ride over, whatever the door. Three rides and no mechanic are said, in the hard
	/// hat with tag 9, whose bar carries the hire button of type 0xe; two are not.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the days not asked, asked the other way, or 30 days not enough; the mechanics not taken
	/// off, taken off once each, or one mechanic silencing it; a park at its mechanics' number scored; the rides
	/// another count; the key row 6's; the row not polled; the lines, the response, the sample, the prop or the
	/// group another's; tag 9's row another's; the poll line naming another row's facts.
	/// </remarks>
	[TestMethod]
	public void MoreRidesThanTheMechanicsKeepArePolledThirtyDaysAfterTheNewestHire()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 7 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 1, WaitingToOpen: 0, Shops: 1, Toilets: 3, RidesWorn: 5, RidesBroken: 2, DaysSinceStaffHired: 29 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 7;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.HireMoreMechanics );
		Assert.AreEqual( (7, "StaffHireMechanics3.ScorePerExtraRide", "StaffHireMechanics3.IdealNumberOfRidesPerMechanic", 20, 20, 3, 1, 0, true, 9, ParkAdvisorMessages.HardHat, -1),
			(topic.Id, topic.ScoreKey, topic.RidesAMechanic, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( "WaitingTimes.DaysForStaff", ParkAdvisorMessages.DaysForStaff );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.RidesAMechanic != null ) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 7 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 928 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 20, 21, 22 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 9 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 13, 0xe, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Twenty-nine days after the newest hire: nought, whatever stands.
		Round( 928 );
		Assert.AreEqual( (0, "no-score", 8, 0), (messages.ScoreOf( topic, 928 ), messages.PollAnswer( 7 ), cursor.Row, said.Count) );
		Assert.AreEqual( " rides=3 mechanics=0 sincehire=29", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Thirty: the key for each ride over four a mechanic.
		facts = facts with { DaysSinceStaffHired = 30, Rides = 2 };
		Round( 929 );
		Assert.AreEqual( (20, "under-threshold", 0), (messages.ScoreOf( topic, 929 ), messages.PollAnswer( 7 ), said.Count), "20 is not above 25" );
		facts = facts with { Rides = 4, Mechanics = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 929 ), "four rides are one mechanic's" );
		facts = facts with { Rides = 7 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 929 ), "three over one mechanic's four" );
		facts = facts with { Rides = 11, Mechanics = 2 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 929 ), "three over two mechanics' eight" );
		facts = facts with { Rides = 3, Mechanics = 1 };
		Round( 930 );
		Assert.AreEqual( (0, "no-score"), (messages.ScoreOf( topic, 930 ), messages.PollAnswer( 7 )), "fewer than a mechanic keeps is nought, not a score below it" );
		facts = facts with { Mechanics = 0, Closed = true, DaysSinceStaffHired = 31 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 932 ) );
		Round( 932 );
		CollectionAssert.AreEqual( new[] { (932, 20, 20) }, said );
		CollectionAssert.AreEqual( new[] { (9, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=7/score=30/line=0/lastTick=932/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 932 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 7 ), said.Count) );
		Round( 932 + 480 );
		Round( 932 + 960 );
		Round( 932 + 1440 );
		CollectionAssert.AreEqual( new[] { (932, 20, 20), (1412, 21, 21), (1892, 22, 22), (2372, 20, 20) }, said, "three lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A member of staff hired: nought again for thirty days.
		facts = facts with { DaysSinceStaffHired = 0 };
		Round( 2372 + 480 );
		Assert.AreEqual( ("no-score", 4), (messages.PollAnswer( 7 ), said.Count) );
	}

	/// <summary>
	/// Row 60, message 0x3c, is polled and scored by the park (0x0059d990): nought where no kind of ride is on
	/// offer, nought until WaitingTimes.DaysForObjects whole days after the newest object was made, nought with
	/// fewer rides than RidesPoorSelection.MoreRidesThan, nought where the kinds standing are PoorerThan per cent
	/// or more of those on offer, in whole numbers, and Score otherwise, whatever the door. One kind of four with
	/// three rides is said, with no hat and tag 62, whose bar carries the buy button of type 9; two kinds are not.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the days not asked, asked the other way, 30 days not enough, or the days another key's or
	/// the staff's; the rides not asked, three not enough, or the kinds standing asked for them; the share the
	/// other way up, not in whole per cent, 30 per cent poor, or its key another's; no kind on offer scored or
	/// thrown on; the score another key's; the row not polled; the lines, the response, the sample, the prop or
	/// the group another's; tag 62's row another's; the poll line naming another row's facts.
	/// </remarks>
	[TestMethod]
	public void APoorSelectionOfRidesIsPolledThirtyDaysAfterTheNewestBuild()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 60 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 1, WaitingToOpen: 0, Shops: 1, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 40, RideKindsOnOffer: 4, RideKindsStanding: 1, DaysSinceObjectBuilt: 29 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 60;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSelection );
		Assert.AreEqual( (0x3c, "RidesPoorSelection.Score", "RidesPoorSelection", 176, 173, 3, 1, 0, true, 62, 0, -1),
			(topic.Id, topic.ScoreKey, topic.PoorSelection, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( "WaitingTimes.DaysForObjects", ParkAdvisorMessages.DaysForObjects );
		Assert.AreEqual( 4, ParkAdvisorMessages.ParkTopics.Count( one => one.PoorSelection != null ) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 60 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 951 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 176, 177, 178 } )
			Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 62 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 65, 9, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Twenty-nine days after the newest build: nought, whatever stands.
		Round( 952 );
		Assert.AreEqual( (0, "no-score", 61, 0), (messages.ScoreOf( topic, 952 ), messages.PollAnswer( 60 ), cursor.Row, said.Count) );
		Assert.AreEqual( " kinds=1/4 rides=3 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Thirty: three rides at least, and the kinds standing under 30 per cent of those on offer, in whole numbers.
		facts = facts with { DaysSinceObjectBuilt = 30, DaysSinceStaffHired = 0 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 953 ), "one kind of four is 25; the newest hire is not asked" );
		facts = facts with { Rides = 2 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two rides are fewer than three" );
		facts = facts with { Rides = 3, RideKindsStanding = 2 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two kinds of four are 50" );
		facts = facts with { Rides = 9, RideKindsOnOffer = 7 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 953 ), "two of seven are 28, whatever the rides over three" );
		facts = facts with { RideKindsStanding = 3, RideKindsOnOffer = 10 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "30 is not under 30" );
		facts = facts with { RideKindsStanding = 1, RideKindsOnOffer = 3 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "one of three is 33" );
		facts = facts with { RideKindsStanding = 299, RideKindsOnOffer = 1000 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 953 ), "29.9 is 29 in whole numbers" );
		facts = facts with { RideKindsStanding = 0, RideKindsOnOffer = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "no kind on offer is nought, and no division" );
		facts = facts with { RideKindsOnOffer = 4, Rides = 3 };
		Assert.AreEqual( 30, messages.ScoreOf( topic, 953 ), "three golden-ticket rides are no kind standing" );

		// The keys are each its own: the score's, the share's and the rides'.
		Set( settings, "RidesPoorSelection.Score", 31 );
		Assert.AreEqual( 31, messages.ScoreOf( topic, 953 ) );
		Set( settings, "RidesPoorSelection.Score", 30 );
		facts = facts with { RideKindsStanding = 1 };
		Set( settings, "RidesPoorSelection.PoorerThan", 25 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "25 is not under 25" );
		Set( settings, "RidesPoorSelection.PoorerThan", 30 );
		Set( settings, "RidesPoorSelection.MoreRidesThan", 4 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "three rides are fewer than four" );
		Set( settings, "RidesPoorSelection.MoreRidesThan", 3 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 99 );
		Assert.AreEqual( 30, messages.ScoreOf( topic, 953 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		facts = facts with { Closed = true, DaysSinceObjectBuilt = 31 };
		Round( 956 );
		CollectionAssert.AreEqual( new[] { (956, 176, 173) }, said );
		CollectionAssert.AreEqual( new[] { (62, 0) }, groups );
		Assert.AreEqual( " kinds=1/4 rides=3 sincebuild=31", messages.PollFacts( topic ) );
		StringAssert.Contains( messages.Census(), "message=60/score=30/line=0/lastTick=956/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the next line is said.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 956 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 60 ), said.Count) );
		Round( 956 + 480 );
		Round( 956 + 960 );
		Round( 956 + 1440 );
		CollectionAssert.AreEqual( new[] { (956, 176, 173), (1436, 177, 174), (1916, 178, 175), (2396, 176, 173) }, said, "three lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A thing built: nought again for thirty days.
		facts = facts with { DaysSinceObjectBuilt = 0 };
		Round( 2396 + 480 );
		Assert.AreEqual( ("no-score", 4), (messages.PollAnswer( 60 ), said.Count) );
	}

	/// <summary>
	/// Row 71, message 0x47, is the shops' twin of row 60 (0x0059dca0): nought where no kind of shop is on offer,
	/// nought until WaitingTimes.DaysForObjects whole days after the newest object was made, nought with fewer
	/// shops than ShopsPoorSelection.MoreShopsThan, nought where the kinds of shop standing are PoorerThan per cent
	/// or more of those on offer, in whole numbers, and Score otherwise, whatever the door. One kind of five with
	/// two shops is said, two lines in turn with no hat and tag 73, whose bar carries the buy button of type 10.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides' kinds, the rides' count or the rides' least key asked; the shops asked for
	/// row 60; the days not asked or the staff's; two shops not enough; 25 per cent poor; the share's or the
	/// score's key the rides' section's; no kind of shop on offer scored or thrown on; the row not polled; the
	/// lines, the response, the sample, the prop or the group another's; tag 73's row another's; the poll line
	/// naming the rides.
	/// </remarks>
	[TestMethod]
	public void APoorSelectionOfShopsIsPolledThirtyDaysAfterTheNewestBuild()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 71 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The rides' facts would score row 60 and silence row 71 if either were asked for the other: nine rides of one kind of ten, no kind of ride for the shops' sum.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 9, Sideshows: 1, WaitingToOpen: 0, Shops: 2, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 40, RideKindsOnOffer: 10, RideKindsStanding: 9, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 5, ShopKindsStanding: 1 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 71;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorShopSelection );
		Assert.AreEqual( (0x47, "ShopsPoorSelection.Score", "ShopsPoorSelection", ParkAdvisorMessages.Shut.Shops, 207, 205, 2, 1, 0, true, 73, 0, -1),
			(topic.Id, topic.ScoreKey, topic.PoorSelection, topic.PoorOf, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		var rides = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSelection );
		Assert.AreEqual( ParkAdvisorMessages.Shut.Rides, rides.PoorOf );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 71 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 951 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 207, 208 } )
			Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 73 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 75, 10, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );
		Assert.AreEqual( (5, 1, 2, "MoreShopsThan", "shops"), facts.Kinds( ParkAdvisorMessages.Shut.Shops ) );
		Assert.AreEqual( (10, 9, 9, "MoreRidesThan", "rides"), facts.Kinds( ParkAdvisorMessages.Shut.Rides ) );

		// Twenty-nine days after the newest build: nought, whatever stands.
		Round( 952 );
		Assert.AreEqual( (0, "no-score", 72, 0), (messages.ScoreOf( topic, 952 ), messages.PollAnswer( 71 ), cursor.Row, said.Count) );
		Assert.AreEqual( " kinds=1/5 shops=2 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );
		Assert.AreEqual( " kinds=9/10 rides=9 sincebuild=29", messages.PollFacts( rides ) );

		// Thirty: two shops at least, and the kinds standing under 25 per cent of those on offer, in whole numbers.
		facts = facts with { DaysSinceObjectBuilt = 30, DaysSinceStaffHired = 0 };
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "one kind of five is 20; the newest hire is not asked" );
		Assert.AreEqual( 0, messages.ScoreOf( rides, 953 ), "nine kinds of ride of ten are not poor, whatever the shops" );
		facts = facts with { Shops = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "one shop is fewer than two" );
		facts = facts with { Shops = 2, ShopKindsOnOffer = 4 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "one kind of four is 25, not under 25" );
		facts = facts with { ShopKindsOnOffer = 5, ShopKindsStanding = 2 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two kinds of five are 40" );
		facts = facts with { Shops = 9, ShopKindsOnOffer = 9 };
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "two of nine are 22, whatever the shops over two" );
		facts = facts with { ShopKindsStanding = 249, ShopKindsOnOffer = 1000 };
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "24.9 is 24 in whole numbers" );
		facts = facts with { ShopKindsStanding = 0, ShopKindsOnOffer = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "no kind of shop on offer is nought, and no division" );
		facts = facts with { ShopKindsOnOffer = 3, Shops = 2 };
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "two golden-ticket shops are no kind standing" );
		facts = facts with { ShopKindsOnOffer = 5, ShopKindsStanding = 1 };

		// The rides' poor selection is its own: three rides of one kind of ten score row 60 and leave row 71's sum alone.
		facts = facts with { Rides = 3, RideKindsStanding = 1 };
		Assert.AreEqual( (30, 50), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( topic, 953 )) );
		facts = facts with { Rides = 1 };
		Assert.AreEqual( (0, 50), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( topic, 953 )), "one ride silences row 60 alone" );

		// The keys are each its own: the score's, the share's and the shops'.
		Set( settings, "ShopsPoorSelection.Score", 51 );
		Assert.AreEqual( 51, messages.ScoreOf( topic, 953 ) );
		Set( settings, "ShopsPoorSelection.Score", 50 );
		Set( settings, "ShopsPoorSelection.PoorerThan", 20 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "20 is not under 20" );
		Set( settings, "ShopsPoorSelection.PoorerThan", 25 );
		Set( settings, "ShopsPoorSelection.MoreShopsThan", 3 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two shops are fewer than three" );
		Set( settings, "ShopsPoorSelection.MoreShopsThan", 2 );
		Set( settings, "RidesPoorSelection.PoorerThan", 1 );
		Set( settings, "RidesPoorSelection.MoreRidesThan", 99 );
		Set( settings, "RidesPoorSelection.Score", 1 );
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "the rides' section is another row's" );
		Set( settings, "RidesPoorSelection.PoorerThan", 30 );
		Set( settings, "RidesPoorSelection.MoreRidesThan", 3 );
		Set( settings, "RidesPoorSelection.Score", 30 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 99 );
		Assert.AreEqual( 50, messages.ScoreOf( topic, 953 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		facts = facts with { Closed = true, DaysSinceObjectBuilt = 31 };
		Round( 956 );
		CollectionAssert.AreEqual( new[] { (956, 207, 205) }, said );
		CollectionAssert.AreEqual( new[] { (73, 0) }, groups );
		Assert.AreEqual( " kinds=1/5 shops=2 sincebuild=31", messages.PollFacts( topic ) );
		StringAssert.Contains( messages.Census(), "message=71/score=50/line=0/lastTick=956/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 956 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 71 ), said.Count) );
		Round( 956 + 480 );
		Round( 956 + 960 );
		CollectionAssert.AreEqual( new[] { (956, 207, 205), (1436, 208, 206), (1916, 207, 205) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A thing built: nought again for thirty days.
		facts = facts with { DaysSinceObjectBuilt = 0 };
		Round( 1916 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 71 ), said.Count) );
	}

	/// <summary>
	/// Row 75, message 0x4b, is the sideshows' twin of rows 60 and 71 (0x0059df30): nought where no kind of
	/// sideshow is on offer, nought until WaitingTimes.DaysForObjects whole days after the newest object was made,
	/// nought with fewer sideshows than SideshowsPoorSelection.MoreSideshowsThan, nought where the kinds of
	/// sideshow standing are PoorerThan per cent or more of those on offer, in whole numbers, and Score otherwise,
	/// whatever the door. One kind of seven with two sideshows is said, two lines in turn with no hat and tag 77,
	/// whose bar carries the buy button of type 11.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides' or the shops' kinds, count or least key asked; the sideshows asked for row 60
	/// or row 71; the days not asked or the staff's; two sideshows not enough; 15 per cent poor; the share's or
	/// the score's key another section's; no kind of sideshow on offer scored or thrown on; the row not polled;
	/// the lines, the response, the sample, the prop or the group another's; tag 77's row another's; the poll line
	/// naming the shops.
	/// </remarks>
	[TestMethod]
	public void APoorSelectionOfSideshowsIsPolledThirtyDaysAfterTheNewestBuild()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 75 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The rides' and the shops' facts would score rows 60 and 71 and silence row 75 if any were asked for another: nine rides of one kind of ten, four shops of two kinds of five.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 9, Sideshows: 2, WaitingToOpen: 0, Shops: 4, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 40, RideKindsOnOffer: 10, RideKindsStanding: 9, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 5, ShopKindsStanding: 2,
			SideshowKindsOnOffer: 7, SideshowKindsStanding: 1 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 75;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSideshowSelection );
		Assert.AreEqual( (0x4b, "SideshowsPoorSelection.Score", "SideshowsPoorSelection", ParkAdvisorMessages.Shut.Sideshows, 215, 213, 2, 1, 0, true, 77, 0, -1),
			(topic.Id, topic.ScoreKey, topic.PoorSelection, topic.PoorOf, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		var rides = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSelection );
		var shops = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorShopSelection );
		Assert.AreEqual( (ParkAdvisorMessages.Shut.Rides, ParkAdvisorMessages.Shut.Shops), (rides.PoorOf, shops.PoorOf) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 75 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 951 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 215, 216 } )
			Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 77 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 79, 11, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );
		Assert.AreEqual( (7, 1, 2, "MoreSideshowsThan", "sideshows"), facts.Kinds( ParkAdvisorMessages.Shut.Sideshows ) );
		Assert.AreEqual( (5, 2, 4, "MoreShopsThan", "shops"), facts.Kinds( ParkAdvisorMessages.Shut.Shops ) );
		Assert.AreEqual( (10, 9, 9, "MoreRidesThan", "rides"), facts.Kinds( ParkAdvisorMessages.Shut.Rides ) );

		// Twenty-nine days after the newest build: nought, whatever stands.
		Round( 952 );
		Assert.AreEqual( (0, "no-score", 76, 0), (messages.ScoreOf( topic, 952 ), messages.PollAnswer( 75 ), cursor.Row, said.Count) );
		Assert.AreEqual( " kinds=1/7 sideshows=2 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );
		Assert.AreEqual( " kinds=2/5 shops=4 sincebuild=29", messages.PollFacts( shops ) );
		Assert.AreEqual( " kinds=9/10 rides=9 sincebuild=29", messages.PollFacts( rides ) );

		// Thirty: two sideshows at least, and the kinds standing under 15 per cent of those on offer, in whole numbers.
		facts = facts with { DaysSinceObjectBuilt = 30, DaysSinceStaffHired = 0 };
		Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), "one kind of seven is 14; the newest hire is not asked" );
		Assert.AreEqual( (0, 0), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 )), "nine kinds of ride of ten and two kinds of shop of five are not poor, whatever the sideshows" );
		facts = facts with { Sideshows = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "one sideshow is fewer than two" );
		facts = facts with { Sideshows = 2, SideshowKindsOnOffer = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "one kind of five, all Lost Kingdom has, is 20" );
		facts = facts with { SideshowKindsOnOffer = 20, SideshowKindsStanding = 3 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "three kinds of twenty are 15, not under 15" );
		facts = facts with { SideshowKindsOnOffer = 7, SideshowKindsStanding = 2 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two kinds of seven are 28" );
		facts = facts with { Sideshows = 9, SideshowKindsOnOffer = 14 };
		Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), "two of fourteen are 14, whatever the sideshows over two" );
		facts = facts with { SideshowKindsStanding = 149, SideshowKindsOnOffer = 1000 };
		Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), "14.9 is 14 in whole numbers" );
		facts = facts with { SideshowKindsStanding = 0, SideshowKindsOnOffer = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "no kind of sideshow on offer is nought, and no division" );
		facts = facts with { SideshowKindsOnOffer = 4, Sideshows = 2 };
		Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), "two golden-ticket sideshows are no kind standing" );
		facts = facts with { SideshowKindsOnOffer = 7, SideshowKindsStanding = 1 };

		// The others' poor selections are their own: three rides of one kind of ten score row 60, two shops of one kind of five row 71, and row 75's sum is left alone.
		facts = facts with { Rides = 3, RideKindsStanding = 1, Shops = 2, ShopKindsStanding = 1 };
		Assert.AreEqual( (30, 50, 45), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( topic, 953 )) );
		facts = facts with { Rides = 1, Shops = 1 };
		Assert.AreEqual( (0, 0, 45), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( topic, 953 )), "one ride and one shop silence rows 60 and 71 alone" );
		facts = facts with { Rides = 3, Shops = 2, Sideshows = 1 };
		Assert.AreEqual( (30, 50, 0), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( topic, 953 )), "one sideshow silences row 75 alone" );
		facts = facts with { Sideshows = 2 };

		// The keys are each its own: the score's, the share's and the sideshows'.
		Set( settings, "SideshowsPoorSelection.Score", 46 );
		Assert.AreEqual( 46, messages.ScoreOf( topic, 953 ) );
		Set( settings, "SideshowsPoorSelection.Score", 45 );
		Set( settings, "SideshowsPoorSelection.PoorerThan", 14 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "14 is not under 14" );
		Set( settings, "SideshowsPoorSelection.PoorerThan", 15 );
		Set( settings, "SideshowsPoorSelection.MoreSideshowsThan", 3 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two sideshows are fewer than three" );
		Set( settings, "SideshowsPoorSelection.MoreSideshowsThan", 2 );
		foreach ( var other in new[] { "RidesPoorSelection", "ShopsPoorSelection" } )
		{
			var least = other == "RidesPoorSelection" ? "MoreRidesThan" : "MoreShopsThan";
			var (poorer, more, score) = (int.Parse( settings[$"{other}.PoorerThan"] ), int.Parse( settings[$"{other}.{least}"] ), int.Parse( settings[$"{other}.Score"] ));
			Set( settings, $"{other}.PoorerThan", 1 );
			Set( settings, $"{other}.{least}", 99 );
			Set( settings, $"{other}.Score", 1 );
			Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), $"{other} is another row's section" );
			Set( settings, $"{other}.PoorerThan", poorer );
			Set( settings, $"{other}.{least}", more );
			Set( settings, $"{other}.Score", score );
		}
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 99 );
		Assert.AreEqual( 45, messages.ScoreOf( topic, 953 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		facts = facts with { Closed = true, DaysSinceObjectBuilt = 31, Rides = 1, Shops = 1 };
		Round( 956 );
		CollectionAssert.AreEqual( new[] { (956, 215, 213) }, said );
		CollectionAssert.AreEqual( new[] { (77, 0) }, groups );
		Assert.AreEqual( " kinds=1/7 sideshows=2 sincebuild=31", messages.PollFacts( topic ) );
		StringAssert.Contains( messages.Census(), "message=75/score=45/line=0/lastTick=956/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 956 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 75 ), said.Count) );
		Round( 956 + 480 );
		Round( 956 + 960 );
		CollectionAssert.AreEqual( new[] { (956, 215, 213), (1436, 216, 214), (1916, 215, 213) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A thing built: nought again for thirty days.
		facts = facts with { DaysSinceObjectBuilt = 0 };
		Round( 1916 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 75 ), said.Count) );
	}

	/// <summary>
	/// Row 79, message 0x4f, is the features' twin of rows 60, 71 and 75 (0x0059e1c0): nought where no kind of
	/// feature is on offer, nought until WaitingTimes.DaysForObjects whole days after the newest object was made,
	/// nought with fewer features than FeaturesPoorSelection.MoreFeaturesThan, nought where the kinds of feature
	/// standing are PoorerThan per cent or more of those on offer, in whole numbers, and Score otherwise. The
	/// shipped Score, 20, is not over the advisor's 25, so the row scores and is never posted; under a Score of
	/// 26 it is said, two lines in turn in the theme's costume and with tag 81, whose bar carries the buy button
	/// of type 12.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides', the shops' or the sideshows' kinds, count or least key asked; the features
	/// asked for row 60, 71 or 75; the days not asked or the staff's; five features not enough; 20 per cent poor;
	/// the share's or the score's key another section's; no kind of feature on offer scored or thrown on; a score
	/// of 20 or 25 posted; the row not polled; the lines, the response, the sample, the prop or the group
	/// another's; tag 81's row another's; the poll line naming the sideshows; the costume not counted.
	/// </remarks>
	[TestMethod]
	public void APoorSelectionOfFeaturesScoresUnderTheThresholdAndIsSaidOnlyUnderAHigherKey()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 79 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The others' facts would score rows 60, 71 and 75 and silence row 79 if any were asked for another: nine rides of one kind of ten, four shops of two kinds of five, two sideshows of one kind of seven.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 9, Sideshows: 2, WaitingToOpen: 0, Shops: 4, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 40, RideKindsOnOffer: 10, RideKindsStanding: 9, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 5, ShopKindsStanding: 2,
			SideshowKindsOnOffer: 7, SideshowKindsStanding: 4, Features: 5, FeatureKindsOnOffer: 11, FeatureKindsStanding: 2 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 79;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorFeatureSelection );
		Assert.AreEqual( (0x4f, "FeaturesPoorSelection.Score", "FeaturesPoorSelection", ParkAdvisorMessages.Shut.Features, 223, 221, 2, 1, 0, true, 81, -1, -1),
			(topic.Id, topic.ScoreKey, topic.PoorSelection, topic.PoorOf, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		var rides = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSelection );
		var shops = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorShopSelection );
		var sideshows = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.PoorSideshowSelection );
		Assert.AreEqual( (ParkAdvisorMessages.Shut.Rides, ParkAdvisorMessages.Shut.Shops, ParkAdvisorMessages.Shut.Sideshows), (rides.PoorOf, shops.PoorOf, sideshows.PoorOf) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 79 ) );
		Assert.AreEqual( AdvisorProps.ThemeCostume, -1 );
		foreach ( var response in new[] { 223, 224 } )
			Assert.AreEqual( (-1, 0), ParkAdvisorMessages.PropsOf( response ) );
		Assert.AreEqual( (0, 0), (ParkAdvisorMessages.PropsOf( 222 ).First - ParkAdvisorMessages.HardHat, ParkAdvisorMessages.PropsOf( 225 ).First - ParkAdvisorMessages.ResearchHat), "the costume is rows 223 and 224's alone" );
		var row = ParkTags.Rows.Single( one => one.Id == 81 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 83, 12, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );
		Assert.AreEqual( (11, 2, 5, "MoreFeaturesThan", "features"), facts.Kinds( ParkAdvisorMessages.Shut.Features ) );
		Assert.AreEqual( (7, 4, 2, "MoreSideshowsThan", "sideshows"), facts.Kinds( ParkAdvisorMessages.Shut.Sideshows ) );
		Assert.AreEqual( (5, 2, 4, "MoreShopsThan", "shops"), facts.Kinds( ParkAdvisorMessages.Shut.Shops ) );
		Assert.AreEqual( (10, 9, 9, "MoreRidesThan", "rides"), facts.Kinds( ParkAdvisorMessages.Shut.Rides ) );
		Assert.AreEqual( (0, 0), facts.Of( ParkAdvisorMessages.Shut.Features ), "no row counts the features shut" );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 951 ), "before any sweep has handed the park in" );

		// Twenty-nine days after the newest build: nought, whatever stands.
		Round( 952 );
		Assert.AreEqual( (0, "no-score", 80, 0), (messages.ScoreOf( topic, 952 ), messages.PollAnswer( 79 ), cursor.Row, said.Count) );
		Assert.AreEqual( " kinds=2/11 features=5 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );
		Assert.AreEqual( " kinds=4/7 sideshows=2 sincebuild=29", messages.PollFacts( sideshows ) );

		// Thirty: five features at least, and the kinds standing under 20 per cent of those on offer, in whole numbers.
		facts = facts with { DaysSinceObjectBuilt = 30, DaysSinceStaffHired = 0 };
		Assert.AreEqual( 20, messages.ScoreOf( topic, 953 ), "two kinds of eleven are 18; the newest hire is not asked" );
		Assert.AreEqual( (0, 0, 0), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( sideshows, 953 )), "the others are not poor, whatever the features" );
		facts = facts with { Features = 4 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "four features are fewer than five" );
		facts = facts with { Features = 5, FeatureKindsOnOffer = 10 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "two kinds of ten are 20, not under 20" );
		facts = facts with { FeatureKindsOnOffer = 8, FeatureKindsStanding = 5, Features = 8 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "the shipped park's five kinds of eight are 62" );
		facts = facts with { Features = 90, FeatureKindsOnOffer = 1000, FeatureKindsStanding = 199 };
		Assert.AreEqual( 20, messages.ScoreOf( topic, 953 ), "19.9 is 19 in whole numbers, whatever the features over five" );
		facts = facts with { FeatureKindsStanding = 0, FeatureKindsOnOffer = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "no kind of feature on offer is nought, and no division" );
		facts = facts with { FeatureKindsOnOffer = 4, Features = 5 };
		Assert.AreEqual( 20, messages.ScoreOf( topic, 953 ), "five golden-ticket features are no kind standing" );
		facts = facts with { FeatureKindsOnOffer = 11, FeatureKindsStanding = 2 };

		// The others' poor selections are their own, and row 79's sum is left alone.
		facts = facts with { Rides = 3, RideKindsStanding = 1, Shops = 2, ShopKindsStanding = 1, SideshowKindsStanding = 1 };
		Assert.AreEqual( (30, 50, 45, 20), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( sideshows, 953 ), messages.ScoreOf( topic, 953 )) );
		facts = facts with { Rides = 1, Shops = 1, Sideshows = 1 };
		Assert.AreEqual( (0, 0, 0, 20), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( sideshows, 953 ), messages.ScoreOf( topic, 953 )), "one of each silences rows 60, 71 and 75 alone" );
		facts = facts with { Rides = 3, Shops = 2, Sideshows = 2, Features = 4 };
		Assert.AreEqual( (30, 50, 45, 0), (messages.ScoreOf( rides, 953 ), messages.ScoreOf( shops, 953 ), messages.ScoreOf( sideshows, 953 ), messages.ScoreOf( topic, 953 )), "four features silence row 79 alone" );
		facts = facts with { Features = 5, Rides = 1, Shops = 1, Sideshows = 1 };

		// The keys are each its own: the share's and the features'.
		Set( settings, "FeaturesPoorSelection.PoorerThan", 18 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "18 is not under 18" );
		Set( settings, "FeaturesPoorSelection.PoorerThan", 20 );
		Set( settings, "FeaturesPoorSelection.MoreFeaturesThan", 6 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "five features are fewer than six" );
		Set( settings, "FeaturesPoorSelection.MoreFeaturesThan", 5 );
		foreach ( var (other, least) in new[] { ("RidesPoorSelection", "MoreRidesThan"), ("ShopsPoorSelection", "MoreShopsThan"), ("SideshowsPoorSelection", "MoreSideshowsThan") } )
		{
			var (poorer, more, score) = (int.Parse( settings[$"{other}.PoorerThan"] ), int.Parse( settings[$"{other}.{least}"] ), int.Parse( settings[$"{other}.Score"] ));
			Set( settings, $"{other}.PoorerThan", 1 );
			Set( settings, $"{other}.{least}", 99 );
			Set( settings, $"{other}.Score", 1 );
			Assert.AreEqual( 20, messages.ScoreOf( topic, 953 ), $"{other} is another row's section" );
			Set( settings, $"{other}.PoorerThan", poorer );
			Set( settings, $"{other}.{least}", more );
			Set( settings, $"{other}.Score", score );
		}
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 953 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 99 );
		Assert.AreEqual( 20, messages.ScoreOf( topic, 953 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		// The shipped score, 20, and 25 are not over the advisor's 25: scored on its turn and never posted.
		facts = facts with { Closed = true, DaysSinceObjectBuilt = 31 };
		Round( 954 );
		Assert.AreEqual( ("under-threshold", 0, 0), (messages.PollAnswer( 79 ), said.Count, groups.Count) );
		Set( settings, "FeaturesPoorSelection.Score", 25 );
		Round( 955 );
		Assert.AreEqual( (25, "under-threshold", 0), (messages.ScoreOf( topic, 955 ), messages.PollAnswer( 79 ), said.Count) );

		// Over it the line is said, in a shut park as in an open one.
		Set( settings, "FeaturesPoorSelection.Score", 26 );
		Round( 956 );
		CollectionAssert.AreEqual( new[] { (956, 223, 221) }, said );
		CollectionAssert.AreEqual( new[] { (81, 0) }, groups );
		Assert.AreEqual( " kinds=2/11 features=5 sincebuild=31", messages.PollFacts( topic ) );
		StringAssert.Contains( messages.Census(), "message=79/score=26/line=0/lastTick=956/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 956 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 79 ), said.Count) );
		Round( 956 + 480 );
		Round( 956 + 960 );
		CollectionAssert.AreEqual( new[] { (956, 223, 221), (1436, 224, 222), (1916, 223, 221) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A thing built: nought again for thirty days.
		facts = facts with { DaysSinceObjectBuilt = 0 };
		Round( 1916 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 79 ), said.Count) );
	}

	/// <summary>
	/// Row 73, message 0x49, is polled (0x0059dde0): nought where no kind of ride stands, nought until
	/// WaitingTimes.DaysForObjects whole days after the newest object was made, nought where a kind of shop
	/// stands, and otherwise ShopsBuildShops.Score, 50, and AdditionalPerDay, 2, for each whole day of the park's
	/// age, whatever the door: 132 on tick 965, 146 on tick 1120. Two lines in turn in the hard hat with tag 75,
	/// whose bar carries the buy button of type 10.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides or the shops not asked, or asked the other way; the things counted for the
	/// kinds; the days not asked, the staff's, or thirty not enough; the park's age not counted, or the days since
	/// the build counted for it; the two keys each other's or the rides' row's; row 62 scored by the shops; the row
	/// not polled; the lines, the response, the sample, the prop or the group another's; tag 75's row another's;
	/// the poll line's words.
	/// </remarks>
	[TestMethod]
	public void AParkWithRidesAndNoShopIsToldToBuildSomeThirtyDaysAfterTheNewestBuild()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 73 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The counts of things are the other way about from the kinds: shops stand that are no kind (a golden ticket's), and no ride is counted where a kind stands.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 0, Sideshows: 1, WaitingToOpen: 0, Shops: 2, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 0, RideKindsOnOffer: 4, RideKindsStanding: 1, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 3, ShopKindsStanding: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 73;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildShops );
		Assert.AreEqual( (0x49, "ShopsBuildShops.Score", "ShopsBuildShops.AdditionalPerDay", 211, 209, 2, 1, 0, true, 75, ParkAdvisorMessages.HardHat, -1),
			(topic.Id, topic.ScoreKey, topic.PerDayWithNoShop, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.PerDayWithNoShop != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 73 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 965 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 211, 212 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 75 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 77, 10, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Twenty-nine days after the newest build: nought.
		Round( 965 );
		Assert.AreEqual( (0, "no-score", 74, 0), (messages.ScoreOf( topic, 965 ), messages.PollAnswer( 73 ), cursor.Row, said.Count) );
		Assert.AreEqual( " ridekinds=1 shopkinds=0 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Thirty: the score and two for each whole day of the park's age, not of the days since the build.
		facts = facts with { DaysSinceObjectBuilt = 30 };
		Assert.AreEqual( (132, 146), (messages.ScoreOf( topic, 965 ), messages.ScoreOf( topic, 1120 )), "days 41 and 48" );
		Assert.AreEqual( (50, 52), (messages.ScoreOf( topic, 23 ), messages.ScoreOf( topic, 24 )), "the first day ends inside tick 24" );
		facts = facts with { DaysSinceObjectBuilt = 500 };
		Assert.AreEqual( 132, messages.ScoreOf( topic, 965 ), "the days since the build are a gate, not the sum" );
		facts = facts with { DaysSinceObjectBuilt = 30, Closed = true, Sideshows = 0, Toilets = 0, DaysSinceStaffHired = 99 };
		Assert.AreEqual( 132, messages.ScoreOf( topic, 965 ), "the door, the sideshows and the newest hire are not asked" );

		// A kind of ride must stand, and no kind of shop: the things themselves are not counted.
		facts = facts with { RideKindsStanding = 0, Rides = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 965 ), "rides that are no kind standing (a golden ticket's) are no ride here" );
		Assert.AreEqual( " ridekinds=0 shopkinds=0 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { RideKindsStanding = 3, Rides = 0, ShopKindsStanding = 1, Shops = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 965 ), "one kind of shop standing" );
		Assert.AreEqual( " ridekinds=3 shopkinds=1 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { ShopKindsStanding = 0, Shops = 7, ShopKindsOnOffer = 0, RideKindsOnOffer = 0 };
		Assert.AreEqual( 132, messages.ScoreOf( topic, 965 ), "seven golden-ticket shops are no kind standing, and what is on offer is not asked" );

		// Row 62, the park with no ride, is its own: a ride silences it and no shop does not move it.
		var rides = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildRides );
		Assert.IsNull( rides.PerDayWithNoShop );
		Assert.IsNull( topic.PerDayWithNoRide );
		Assert.AreEqual( (73, 132), (messages.ScoreOf( rides, 959 ), messages.ScoreOf( topic, 965 )), "no ride counted, a kind of ride standing" );
		Assert.AreEqual( " rides=0", messages.PollFacts( rides ), "row 62's poll line is its own" );
		facts = facts with { Rides = 1, DaysSinceObjectBuilt = 0 };
		Assert.AreEqual( (0, 0), (messages.ScoreOf( rides, 959 ), messages.ScoreOf( topic, 965 )) );
		facts = facts with { Rides = 0 };
		Assert.AreEqual( (73, 0), (messages.ScoreOf( rides, 959 ), messages.ScoreOf( topic, 965 )), "row 62 does not wait for the newest build" );
		facts = facts with { DaysSinceObjectBuilt = 30 };

		// The keys are each its own.
		Set( settings, "ShopsBuildShops.Score", 51 );
		Assert.AreEqual( 133, messages.ScoreOf( topic, 965 ) );
		Set( settings, "ShopsBuildShops.Score", 50 );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 3 );
		Assert.AreEqual( 173, messages.ScoreOf( topic, 965 ) );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 2 );
		Set( settings, "RidesBuildRides.Score", 1 );
		Set( settings, "RidesBuildRides.AdditionalPerDay", 9 );
		Assert.AreEqual( 132, messages.ScoreOf( topic, 965 ), "the rides' section is another row's" );
		Set( settings, "RidesBuildRides.Score", -50 );
		Set( settings, "RidesBuildRides.AdditionalPerDay", 3 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 965 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 999 );
		Assert.AreEqual( 132, messages.ScoreOf( topic, 965 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		Round( 964 );
		CollectionAssert.AreEqual( new[] { (964, 211, 209) }, said );
		CollectionAssert.AreEqual( new[] { (75, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=73/score=132/line=0/lastTick=964/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 964 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 73 ), said.Count) );
		Round( 964 + 480 );
		Round( 964 + 960 );
		CollectionAssert.AreEqual( new[] { (964, 211, 209), (1444, 212, 210), (1924, 211, 209) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A shop built: nought, by the kind standing and by the thirty days again.
		facts = facts with { ShopKindsStanding = 1, DaysSinceObjectBuilt = 0 };
		Round( 1924 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 73 ), said.Count) );
	}

	/// <summary>
	/// Row 82, message 0x52, is polled (0x0059e380): nought in a shut park, nought where a toilet stands, nought
	/// with no guest inside, and otherwise FeaturesNoToilets.Score, 90, where the guests inside whose toilet need
	/// is over NeedingMoreThan, 60, are more than MorePeopleThanThisNeeding, 25, per cent of them in whole numbers:
	/// four of thirteen, 30, score, and three, 23, do not. Three lines in turn, bare-headed, with tag 84, whose bar
	/// carries the buy button of type 12.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the door, the toilets or the guests inside not asked, or asked the other way; the share
	/// taken of the needing, or not in whole numbers, or 25 enough; the two keys each other's; the level's low
	/// byte not taken; the row not polled; two lines or four; the response, the sample, the prop or the group
	/// another's; tag 84's row another's; the poll line's words.
	/// </remarks>
	[TestMethod]
	public void AnOpenParkWithNoToiletIsToldToBuildSomeWhenEnoughOfItsGuestsNeedOne()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 82 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 1, WaitingToOpen: 0, Shops: 2, Toilets: 0,
			DaysSinceObjectBuilt: 0, Features: 4, FeatureKindsStanding: 3, GuestsInside: 13, GuestsNeedingToilet: 3 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 82;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.NoToilets );
		Assert.AreEqual( (0x52, "FeaturesNoToilets.Score", "FeaturesNoToilets", 227, 225, 3, 1, 0, true, 84, 0, -1),
			(topic.Id, topic.ScoreKey, topic.NeedingToiletWithNone, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.NeedingToiletWithNone != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 82 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "before any sweep has handed the park in" );
		Assert.AreEqual( ((0, 0), (0, 0), (0, 0)), (ParkAdvisorMessages.PropsOf( 227 ), ParkAdvisorMessages.PropsOf( 228 ), ParkAdvisorMessages.PropsOf( 229 )) );
		Assert.AreEqual( (0, 0, 0), (ParkAdvisorMessages.SlotOf( 227 ), ParkAdvisorMessages.SlotOf( 228 ), ParkAdvisorMessages.SlotOf( 229 )), "the lobby's model says it, not the theme's" );
		var row = ParkTags.Rows.Single( one => one.Id == 84 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 86, 12, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );
		Assert.AreEqual( 60, messages.ToiletNeedOver );

		// Three of thirteen need one: 23 per cent is not more than 25.
		Round( 1594 );
		Assert.AreEqual( (0, "no-score", 83, 0), (messages.ScoreOf( topic, 1594 ), messages.PollAnswer( 82 ), cursor.Row, said.Count) );
		Assert.AreEqual( " closed=0 toilets=0 needing=3/13", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Four of thirteen, 30, is; one of four, 25, is not, and two of seven, 28 in whole numbers, is.
		facts = facts with { GuestsNeedingToilet = 4 };
		Assert.AreEqual( 90, messages.ScoreOf( topic, 1594 ) );
		facts = facts with { GuestsInside = 4, GuestsNeedingToilet = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "25 per cent is not more than 25" );
		facts = facts with { GuestsInside = 103, GuestsNeedingToilet = 26 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "25.2 per cent is 25 in whole numbers" );
		facts = facts with { GuestsInside = 7, GuestsNeedingToilet = 2 };
		Assert.AreEqual( 90, messages.ScoreOf( topic, 1594 ) );
		Assert.AreEqual( 90, messages.ScoreOf( topic, 3 ), "the park's age is not asked" );
		facts = facts with { DaysSinceObjectBuilt = 500, Rides = 0, Sideshows = 0, Shops = 0, Features = 0, FeatureKindsStanding = 0, ToiletsShut = 5, WaitingToOpen = 9 };
		Assert.AreEqual( 90, messages.ScoreOf( topic, 1594 ), "what else stands, the newest build and who waits outside are not asked" );

		// A shut park, a toilet standing and a park with nobody inside are each nought.
		facts = facts with { Closed = true };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "a shut park" );
		Assert.AreEqual( " closed=1 toilets=0 needing=2/7", messages.PollFacts( topic ) );
		facts = facts with { Closed = false, Toilets = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "one toilet standing, shut or not" );
		Assert.AreEqual( " closed=0 toilets=1 needing=2/7", messages.PollFacts( topic ) );
		facts = facts with { Toilets = 0, GuestsInside = 0, GuestsNeedingToilet = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "nobody inside" );
		facts = facts with { GuestsInside = 0, GuestsNeedingToilet = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "nobody inside is asked before the share is taken" );
		facts = facts with { GuestsInside = 13, GuestsNeedingToilet = 4 };

		// Said on its turn, its three lines in turn and then the first again, each leaving tag 84.
		Round( 1594 );
		Assert.AreEqual( "post", messages.PollAnswer( 82 ) );
		// The wait is 120 fours of sweeps: tick 1594 is in four 398, so 2071, in four 517, is inside it and 2072 is not.
		Round( 2071 );
		Round( 2072 );
		Round( 2552 );
		Round( 3032 );
		CollectionAssert.AreEqual( new[] { (1594, 227, 225), (2072, 228, 226), (2552, 229, 227), (3032, 227, 225) }, said );
		CollectionAssert.AreEqual( new[] { (84, 0), (84, 0), (84, 0), (84, 0) }, groups );

		// The keys are each its own, and the level is its low byte.
		Set( settings, "FeaturesNoToilets.Score", 91 );
		Assert.AreEqual( 91, messages.ScoreOf( topic, 1594 ) );
		Set( settings, "FeaturesNoToilets.Score", 90 );
		Set( settings, "FeaturesNoToilets.MorePeopleThanThisNeeding", 30 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1594 ), "four of thirteen is 30" );
		Set( settings, "FeaturesNoToilets.MorePeopleThanThisNeeding", 29 );
		Assert.AreEqual( 90, messages.ScoreOf( topic, 1594 ) );
		Set( settings, "FeaturesNoToilets.MorePeopleThanThisNeeding", 25 );
		Set( settings, "FeaturesNoToilets.NeedingMoreThan", 61 );
		Assert.AreEqual( (61, 90), (messages.ToiletNeedOver, messages.ScoreOf( topic, 1594 )), "the level is the counter's, not the score's" );
		Set( settings, "FeaturesNoToilets.NeedingMoreThan", 0x13c );
		Assert.AreEqual( 0x3c, messages.ToiletNeedOver );
		Set( settings, "FeaturesNoToilets.NeedingMoreThan", 60 );
		Set( settings, "FeaturesBuildFeatures.Score", 77 );
		Set( settings, "ToiletsClosed.Score", 78 );
		Assert.AreEqual( 90, messages.ScoreOf( topic, 1594 ), "rows 81's and 262's keys are theirs" );
		Set( settings, "FeaturesBuildFeatures.Score", -40 );
		Set( settings, "ToiletsClosed.Score", 75 );
	}

	/// <summary>
	/// Row 80, message 0x50, is polled (0x0059e260): nought with no researcher on the staff, nought where no kind
	/// of feature stands, nought where the lab holds no project of a feature's kind left undone, nought unless the
	/// kinds of feature standing are as many as the kinds on offer, and otherwise FeaturesResearchMore.Score, 60.
	/// One line, in the research hat, with tag 82, whose bar carries no button; its category is the fourth, whose
	/// wait between two sayings is 900 fours of sweeps. The row is Full Simulation's alone (its +0x0c is nought):
	/// in Instant Action the poll (FUN_0059c680) does not score it, and answers -1.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the researchers, the kinds standing or the lab's projects not asked, or asked the other
	/// way; the kinds standing let be fewer, or more, than those on offer; the park's age, the newest build or
	/// the door asked; the row not polled; two lines; the response, the sample, the prop, the group or the
	/// category another's; tag 82's row another's; the wait category nought's; the poll line's words; the row
	/// scored in Instant Action, or not in Full Simulation; the console's hold not taken, or not let go.
	/// </remarks>
	[TestMethod]
	public void AParkWithEveryFeatureOnOfferBuiltIsToldToResearchMore()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 80 };
		var settings = Settings();
		var instant = false;
		var messages = new ParkAdvisorMessages( settings, cursor: cursor, instantAction: () => instant );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 1, WaitingToOpen: 0, Shops: 2, Toilets: 3,
			DaysSinceObjectBuilt: 0, Features: 11, FeatureKindsOnOffer: 8, FeatureKindsStanding: 7, Researchers: 1, FeatureProjectsUnfinished: 11 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 80;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ResearchMoreFeatures );
		Assert.AreEqual( (0x50, "FeaturesResearchMore.Score", true, 225, 223, 1, 1, 4, true, 82, 10, -1),
			(topic.Id, topic.ScoreKey, topic.EveryFeatureBuilt, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( (4, 10), (ParkAdvisorMessages.ResearchMore, ParkAdvisorMessages.ResearchHat) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.EveryFeatureBuilt ), "no other row is scored so" );
		Assert.AreEqual( (true, 1), (topic.FullSimulationOnly, ParkAdvisorMessages.ParkTopics.Count( one => one.FullSimulationOnly )), "every other built row is either game's" );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.Category == ParkAdvisorMessages.ResearchMore ), "no other built row is of its category" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 80 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 972 ), "before any sweep has handed the park in" );
		Assert.AreEqual( ((-1, 0), (10, 0), (-1, 0)), (ParkAdvisorMessages.PropsOf( 224 ), ParkAdvisorMessages.PropsOf( 225 ), ParkAdvisorMessages.PropsOf( 226 )), "the hat is response 225's alone" );
		Assert.AreEqual( 0, ParkAdvisorMessages.SlotOf( 225 ), "the lobby's model says it, not the theme's" );
		CollectionAssert.AreEqual( new[] { (19, false), (20, false), (10, true) }, AdvisorProps.Switched( 10 ).ToArray(), "the hat hides both antennae" );
		var row = ParkTags.Rows.Single( one => one.Id == 82 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 84, 0, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Seven kinds of the eight on offer stand: not every one.
		Round( 972 );
		Assert.AreEqual( (0, "no-score", 81, 0), (messages.ScoreOf( topic, 972 ), messages.PollAnswer( 80 ), cursor.Row, said.Count) );
		Assert.AreEqual( " researchers=1 kinds=7/8 unfinished=11", messages.PollFacts( topic ), "the poll line names what the score read" );

		// All eight score; nine of eight, which no park holds, do not: the two are asked to be equal.
		facts = facts with { FeatureKindsStanding = 8 };
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ) );
		facts = facts with { FeatureKindsStanding = 9 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 972 ), "more than are on offer is not as many" );
		facts = facts with { FeatureKindsStanding = 8 };
		Assert.AreEqual( 60, messages.ScoreOf( topic, 3 ), "the park's age is not asked" );
		facts = facts with { DaysSinceObjectBuilt = 500, Closed = true, Rides = 0, Sideshows = 0, Shops = 0, Toilets = 0, Features = 0, Balance = -90000, Mechanics = 0 };
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ), "the newest build, the door, what else stands, the bank and the mechanics are not asked" );
		facts = facts with { Researchers = 3, FeatureProjectsUnfinished = 1, Mechanics = 2 };
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ), "one project and three researchers are a project and a researcher" );

		// No researcher, no kind standing and no project left are each nought.
		facts = facts with { Researchers = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 972 ), "no researcher, whatever mechanics there are" );
		Assert.AreEqual( " researchers=0 kinds=8/8 unfinished=1", messages.PollFacts( topic ) );
		facts = facts with { Researchers = 1, FeatureProjectsUnfinished = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 972 ), "nothing left to research" );
		Assert.AreEqual( " researchers=1 kinds=8/8 unfinished=0", messages.PollFacts( topic ) );
		facts = facts with { FeatureProjectsUnfinished = 2, FeatureKindsOnOffer = 0, FeatureKindsStanding = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 972 ), "none standing of none on offer is not every one built" );
		facts = facts with { FeatureKindsOnOffer = 1, FeatureKindsStanding = 1 };
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ), "one kind of one" );
		facts = facts with { FeatureKindsOnOffer = 8, FeatureKindsStanding = 8 };

		// In Instant Action the row is not scored at all, whatever the park holds; the score function is not what
		// refuses it. The console's hold is taken before the player's own game, and let go again.
		instant = true;
		Round( 972 );
		Assert.AreEqual( ("other-game", 81, 0, 0), (messages.PollAnswer( 80 ), cursor.Row, said.Count, messages.PendingCount) );
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ), "the poll refuses the row, not its score" );
		Assert.IsTrue( messages.InstantAction );
		messages.InstantActionHeld = false;
		Assert.IsFalse( messages.InstantAction, "held to Full Simulation's" );
		instant = false;
		messages.InstantActionHeld = true;
		Assert.IsTrue( messages.InstantAction, "held to Instant Action's" );
		Round( 972 );
		Assert.AreEqual( ("other-game", 0), (messages.PollAnswer( 80 ), said.Count) );
		messages.InstantActionHeld = null;
		Assert.IsFalse( messages.InstantAction, "let go: the player's own" );

		// Said on its turn with its one line and tag 82, and again only 900 fours of sweeps on: tick 972 is in four
		// 243, so 4571, in four 1142, is inside the wait and 4572 is not; category nought's 120 would let 1452 by.
		Round( 972 );
		Assert.AreEqual( "post", messages.PollAnswer( 80 ) );
		Round( 1452 );
		Assert.AreEqual( "cooldown", messages.PollAnswer( 80 ) );
		Round( 4571 );
		Round( 4572 );
		CollectionAssert.AreEqual( new[] { (972, 225, 223), (4572, 225, 223) }, said );
		CollectionAssert.AreEqual( new[] { (82, 0), (82, 0) }, groups );

		// The key is its own.
		Set( settings, "FeaturesResearchMore.Score", 61 );
		Assert.AreEqual( 61, messages.ScoreOf( topic, 972 ) );
		Set( settings, "FeaturesResearchMore.Score", 60 );
		Set( settings, "SideshowsResearchMore.Score", 77 );
		Set( settings, "FeaturesPoorSelection.Score", 78 );
		Set( settings, "FeaturesBuildFeatures.Score", 79 );
		Assert.AreEqual( 60, messages.ScoreOf( topic, 972 ), "rows 76's, 79's and 81's keys are theirs" );
		Set( settings, "SideshowsResearchMore.Score", 75 );
		Set( settings, "FeaturesPoorSelection.Score", 20 );
		Set( settings, "FeaturesBuildFeatures.Score", -40 );
	}

	/// <summary>
	/// Row 81, message 0x51, is polled (0x0059e2e0): nought where no kind of sideshow stands, nought until
	/// WaitingTimes.DaysForObjects whole days after the newest object was made, nought where a kind of feature
	/// stands, and otherwise FeaturesBuildFeatures.Score, -40, and AdditionalPerDay, 1, for each whole day of the
	/// park's age, whatever the door: 22 on tick 1438, which is not above the threshold, and 29 on tick 1593, which
	/// is. One line in the theme's costume with tag 83, whose bar carries the buy button of type 12.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the sideshows or the features not asked, or asked the other way; the shops or the rides
	/// asked for the sideshows; the things counted for the kinds; the days not asked, the staff's, or thirty not
	/// enough; the park's age not counted, or the days since the build counted for it; the two keys each other's
	/// or row 77's; row 77 scored by the features; the row not polled; a second line; the response, the sample,
	/// the prop or the group another's; tag 83's row another's; the poll line's words; the costume not counted.
	/// </remarks>
	[TestMethod]
	public void AParkWithSideshowsAndNoFeatureIsToldToBuildSomeOnceItIsOldEnough()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 81 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The counts of things are the other way about from the kinds: features stand that are no kind (a golden ticket's), and no sideshow is counted where a kind stands.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 0, WaitingToOpen: 0, Shops: 0, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 0, RideKindsOnOffer: 4, RideKindsStanding: 0, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 3, ShopKindsStanding: 0,
			SideshowKindsOnOffer: 4, SideshowKindsStanding: 1, Features: 6, FeatureKindsOnOffer: 8, FeatureKindsStanding: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 81;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildFeatures );
		Assert.AreEqual( (0x51, "FeaturesBuildFeatures.Score", "FeaturesBuildFeatures.AdditionalPerDay", 226, 224, 1, 1, 0, true, 83, AdvisorProps.ThemeCostume, -1),
			(topic.Id, topic.ScoreKey, topic.PerDayWithNoFeature, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.PerDayWithNoFeature != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 81 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1593 ), "before any sweep has handed the park in" );
		Assert.AreEqual( (AdvisorProps.ThemeCostume, 0), ParkAdvisorMessages.PropsOf( 226 ) );
		Assert.AreEqual( (0, 0), (ParkAdvisorMessages.PropsOf( 225 ).First - ParkAdvisorMessages.ResearchHat, ParkAdvisorMessages.PropsOf( 227 ).First), "the costume is response 226's, not its neighbours'" );
		var row = ParkTags.Rows.Single( one => one.Id == 83 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 85, 12, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Twenty-nine days after the newest build: nought.
		Round( 1593 );
		Assert.AreEqual( (0, "no-score", 82, 0), (messages.ScoreOf( topic, 1593 ), messages.PollAnswer( 81 ), cursor.Row, said.Count) );
		Assert.AreEqual( " sideshowkinds=1 featurekinds=0 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Thirty: forty under nought and one for each whole day of the park's age, not of the days since the build.
		facts = facts with { DaysSinceObjectBuilt = 30 };
		Assert.AreEqual( (22, 29, 35), (messages.ScoreOf( topic, 1438 ), messages.ScoreOf( topic, 1593 ), messages.ScoreOf( topic, 1748 )), "days 62, 69 and 75" );
		Assert.AreEqual( (-40, -39, -1, 0, 25, 26), (messages.ScoreOf( topic, 23 ), messages.ScoreOf( topic, 24 ), messages.ScoreOf( topic, 921 ), messages.ScoreOf( topic, 922 ), messages.ScoreOf( topic, 1520 ), messages.ScoreOf( topic, 1521 )), "day 40 ends the debt, and day 66 is the first above the threshold" );
		facts = facts with { DaysSinceObjectBuilt = 500 };
		Assert.AreEqual( 29, messages.ScoreOf( topic, 1593 ), "the days since the build are a gate, not the sum" );
		facts = facts with { DaysSinceObjectBuilt = 30, Closed = true, Rides = 0, Toilets = 0, DaysSinceStaffHired = 99, RideKindsStanding = 3, ShopKindsStanding = 2 };
		Assert.AreEqual( 29, messages.ScoreOf( topic, 1593 ), "the door, the rides, the shops and the newest hire are not asked" );

		// Under the threshold the row is scored and not posted; over it the one line is said, and said again, never a second.
		Round( 1438 );
		Assert.AreEqual( ("under-threshold", 0), (messages.PollAnswer( 81 ), said.Count) );
		Round( 1593 );
		Assert.AreEqual( "post", messages.PollAnswer( 81 ) );
		CollectionAssert.AreEqual( new[] { (1593, 226, 224) }, said );
		CollectionAssert.AreEqual( new[] { (83, 0) }, groups );
		Round( 1593 + 480 );
		CollectionAssert.AreEqual( new[] { (1593, 226, 224), (1593 + 480, 226, 224) }, said, "the row has one line" );

		// A kind of sideshow must stand, and no kind of feature: the things themselves are not counted.
		facts = facts with { SideshowKindsStanding = 0, Sideshows = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1593 ), "sideshows that are no kind standing (a golden ticket's) are no sideshow here" );
		Assert.AreEqual( " sideshowkinds=0 featurekinds=0 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { SideshowKindsStanding = 3, Sideshows = 0, FeatureKindsStanding = 1, Features = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1593 ), "one kind of feature standing" );
		Assert.AreEqual( " sideshowkinds=3 featurekinds=1 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { FeatureKindsStanding = 0, Features = 7, FeatureKindsOnOffer = 0, SideshowKindsOnOffer = 0 };
		Assert.AreEqual( 29, messages.ScoreOf( topic, 1593 ), "seven golden-ticket features are no kind standing, and what is on offer is not asked" );

		// Row 77, the park with shops and no sideshow, is its own.
		var sideshows = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildSideshows );
		Assert.IsNull( sideshows.PerDayWithNoFeature );
		Assert.IsNull( topic.PerDayWithNoSideshow );
		Assert.AreEqual( (0, 29), (messages.ScoreOf( sideshows, 1124 ), messages.ScoreOf( topic, 1593 )), "a kind of sideshow stands" );
		Assert.AreEqual( " shopkinds=2 sideshowkinds=3 sincebuild=30", messages.PollFacts( sideshows ), "row 77's poll line is its own" );
		facts = facts with { SideshowKindsStanding = 0 };
		Assert.AreEqual( (26, 0), (messages.ScoreOf( sideshows, 1124 ), messages.ScoreOf( topic, 1593 )), "and none does" );
		facts = facts with { SideshowKindsStanding = 1, ShopKindsStanding = 0 };

		// The keys are each its own.
		Set( settings, "FeaturesBuildFeatures.Score", -39 );
		Assert.AreEqual( 30, messages.ScoreOf( topic, 1593 ) );
		Set( settings, "FeaturesBuildFeatures.Score", -40 );
		Set( settings, "FeaturesBuildFeatures.AdditionalPerDay", 2 );
		Assert.AreEqual( 98, messages.ScoreOf( topic, 1593 ) );
		Set( settings, "FeaturesBuildFeatures.AdditionalPerDay", 1 );
		Set( settings, "SideshowsBuildSideshows.Score", 1 );
		Set( settings, "SideshowsBuildSideshows.AdditionalPerDay", 9 );
		Set( settings, "FeaturesPoorSelection.Score", 77 );
		Assert.AreEqual( 29, messages.ScoreOf( topic, 1593 ), "the sideshows' section and row 79's are other rows'" );
		Set( settings, "SideshowsBuildSideshows.Score", -70 );
		Set( settings, "SideshowsBuildSideshows.AdditionalPerDay", 2 );
		Set( settings, "FeaturesPoorSelection.Score", 20 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1593 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 999 );
		Assert.AreEqual( 29, messages.ScoreOf( topic, 1593 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );
	}

	/// <summary>
	/// Row 77, message 0x4d, is polled (0x0059e070): nought where no kind of shop stands, nought until
	/// WaitingTimes.DaysForObjects whole days after the newest object was made, nought where a kind of sideshow
	/// stands, and otherwise SideshowsBuildSideshows.Score, -70, and AdditionalPerDay, 2, for each whole day of the
	/// park's age, whatever the door: 14 on tick 969, which is not above the threshold, and 26 on tick 1124, which
	/// is. Two lines in turn in the hard hat with tag 79, whose bar carries the buy button of type 11.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the shops or the sideshows not asked, or asked the other way; the rides asked for the
	/// shops; the things counted for the kinds; the days not asked, the staff's, or thirty not enough; the park's
	/// age not counted, or the days since the build counted for it; the two keys each other's or row 73's; row 73
	/// scored by the sideshows; the row not polled; the lines, the response, the sample, the prop or the group
	/// another's; tag 79's row another's; the poll line's words.
	/// </remarks>
	[TestMethod]
	public void AParkWithShopsAndNoSideshowIsToldToBuildSomeOnceItIsOldEnough()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 77 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// The counts of things are the other way about from the kinds: sideshows stand that are no kind (a golden ticket's), and no shop is counted where a kind stands.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 2, WaitingToOpen: 0, Shops: 0, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 0, RideKindsOnOffer: 4, RideKindsStanding: 0, DaysSinceObjectBuilt: 29, ShopKindsOnOffer: 3, ShopKindsStanding: 1,
			SideshowKindsOnOffer: 4, SideshowKindsStanding: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 77;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildSideshows );
		Assert.AreEqual( (0x4d, "SideshowsBuildSideshows.Score", "SideshowsBuildSideshows.AdditionalPerDay", 219, 217, 2, 1, 0, true, 79, ParkAdvisorMessages.HardHat, -1),
			(topic.Id, topic.ScoreKey, topic.PerDayWithNoSideshow, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.PerDayWithNoSideshow != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 77 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1124 ), "before any sweep has handed the park in" );
		foreach ( var response in new[] { 219, 220 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 79 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 81, 11, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// Twenty-nine days after the newest build: nought.
		Round( 1124 );
		Assert.AreEqual( (0, "no-score", 78, 0), (messages.ScoreOf( topic, 1124 ), messages.PollAnswer( 77 ), cursor.Row, said.Count) );
		Assert.AreEqual( " shopkinds=1 sideshowkinds=0 sincebuild=29", messages.PollFacts( topic ), "the poll line names what the score read" );

		// Thirty: seventy under nought and two for each whole day of the park's age, not of the days since the build.
		facts = facts with { DaysSinceObjectBuilt = 30 };
		Assert.AreEqual( (14, 26, 40), (messages.ScoreOf( topic, 969 ), messages.ScoreOf( topic, 1124 ), messages.ScoreOf( topic, 1279 )), "days 42, 48 and 55" );
		Assert.AreEqual( (-70, -68, -2, 0), (messages.ScoreOf( topic, 23 ), messages.ScoreOf( topic, 24 ), messages.ScoreOf( topic, 806 ), messages.ScoreOf( topic, 807 )), "day 35 ends the debt" );
		facts = facts with { DaysSinceObjectBuilt = 500 };
		Assert.AreEqual( 26, messages.ScoreOf( topic, 1124 ), "the days since the build are a gate, not the sum" );
		facts = facts with { DaysSinceObjectBuilt = 30, Closed = true, Rides = 0, Toilets = 0, DaysSinceStaffHired = 99, RideKindsStanding = 3 };
		Assert.AreEqual( 26, messages.ScoreOf( topic, 1124 ), "the door, the rides and the newest hire are not asked" );

		// A kind of shop must stand, and no kind of sideshow: the things themselves are not counted.
		facts = facts with { ShopKindsStanding = 0, Shops = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1124 ), "shops that are no kind standing (a golden ticket's) are no shop here" );
		Assert.AreEqual( " shopkinds=0 sideshowkinds=0 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { ShopKindsStanding = 3, Shops = 0, SideshowKindsStanding = 1, Sideshows = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1124 ), "one kind of sideshow standing" );
		Assert.AreEqual( " shopkinds=3 sideshowkinds=1 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { SideshowKindsStanding = 0, Sideshows = 7, SideshowKindsOnOffer = 0, ShopKindsOnOffer = 0 };
		Assert.AreEqual( 26, messages.ScoreOf( topic, 1124 ), "seven golden-ticket sideshows are no kind standing, and what is on offer is not asked" );

		// Row 73, the park with rides and no shop, is its own.
		var shops = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildShops );
		Assert.IsNull( shops.PerDayWithNoSideshow );
		Assert.IsNull( topic.PerDayWithNoShop );
		Assert.AreEqual( (0, 26), (messages.ScoreOf( shops, 965 ), messages.ScoreOf( topic, 1124 )), "a kind of shop stands" );
		Assert.AreEqual( " ridekinds=3 shopkinds=3 sincebuild=30", messages.PollFacts( shops ), "row 73's poll line is its own" );
		facts = facts with { ShopKindsStanding = 0 };
		Assert.AreEqual( (132, 0), (messages.ScoreOf( shops, 965 ), messages.ScoreOf( topic, 1124 )), "and none does" );
		facts = facts with { ShopKindsStanding = 1, RideKindsStanding = 0 };

		// The keys are each its own.
		Set( settings, "SideshowsBuildSideshows.Score", -69 );
		Assert.AreEqual( 27, messages.ScoreOf( topic, 1124 ) );
		Set( settings, "SideshowsBuildSideshows.Score", -70 );
		Set( settings, "SideshowsBuildSideshows.AdditionalPerDay", 3 );
		Assert.AreEqual( 74, messages.ScoreOf( topic, 1124 ) );
		Set( settings, "SideshowsBuildSideshows.AdditionalPerDay", 2 );
		Set( settings, "ShopsBuildShops.Score", 1 );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 9 );
		Assert.AreEqual( 26, messages.ScoreOf( topic, 1124 ), "the shops' section is another row's" );
		Set( settings, "ShopsBuildShops.Score", 50 );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 2 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1124 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 999 );
		Assert.AreEqual( 26, messages.ScoreOf( topic, 1124 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		// Day 42's fourteen is scored and not posted; day 48's twenty-six is said.
		Round( 969 );
		Assert.AreEqual( ("under-threshold", 0), (messages.PollAnswer( 77 ), said.Count) );
		Round( 1124 );
		CollectionAssert.AreEqual( new[] { (1124, 219, 217) }, said );
		CollectionAssert.AreEqual( new[] { (79, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=77/score=26/line=0/lastTick=1124/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 1124 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 77 ), said.Count) );
		Round( 1124 + 480 );
		Round( 1124 + 960 );
		CollectionAssert.AreEqual( new[] { (1124, 219, 217), (1604, 220, 218), (2084, 219, 217) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// A sideshow built: nought, by the kind standing and by the thirty days again.
		facts = facts with { SideshowKindsStanding = 1, DaysSinceObjectBuilt = 0 };
		Round( 2084 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 77 ), said.Count) );
	}

	/// <summary>
	/// Row 74, message 0x4a, is polled (0x0059de80): nought where no kind of shop stands, nought until
	/// WaitingTimes.DaysForObjects whole days after the newest object was made, nought unless the kinds of shop
	/// standing times ShopsBuildMoreShops.ShopsPerRideRatio are more than the kinds of shop standing, nought unless
	/// the bank holds more than WealthMoreThan, 20000, and otherwise Score, 70. The shipped ratio, 0.15, is never
	/// more than one, so the row scores nought whatever stands; under a ratio above one it is two lines in turn in
	/// the hard hat with tag 76, whose bar carries the buy button of type 10.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the rides, their kinds or the shops set against the ratio; the comparison turned, or let
	/// by at equal; the kinds of shop not asked; the days not asked, the staff's, or thirty not enough; the bank
	/// not asked, or let by at the limit; the keys each other's or row 73's; row 73 scored so; the row not polled;
	/// the lines, the response, the sample, the prop or the group another's; tag 76's row another's; the poll
	/// line's words.
	/// </remarks>
	[TestMethod]
	public void TooFewShopsForTheParksSizeSetsTheShopsAgainstThemselvesAndIsNeverSaidUnderTheShippedRatio()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 74 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// What the key's name wants is here: fourteen rides of four kinds beside one shop, and money.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 14, Sideshows: 1, WaitingToOpen: 0, Shops: 1, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 0, RideKindsOnOffer: 4, RideKindsStanding: 4, DaysSinceObjectBuilt: 30, ShopKindsOnOffer: 3, ShopKindsStanding: 1, Balance: 50000 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 74;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildMoreShops );
		Assert.AreEqual( (0x4a, "ShopsBuildMoreShops.Score", "ShopsBuildMoreShops", 213, 211, 2, 1, 0, true, 76, ParkAdvisorMessages.HardHat, -1),
			(topic.Id, topic.ScoreKey, topic.ShopsPerRide, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.ShopsPerRide != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 74 ) );
		foreach ( var response in new[] { 213, 214 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 76 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 78, 10, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// The shipped ratio: nought, whatever the rides, since one kind of shop times 0.15 is not more than one kind of shop.
		Round( 966 );
		Assert.AreEqual( (0, "no-score", 75, 0), (messages.ScoreOf( topic, 966 ), messages.PollAnswer( 74 ), cursor.Row, said.Count) );
		Assert.AreEqual( " shopkinds=1 ratio=0.15 balance=50000 sincebuild=30", messages.PollFacts( topic ), "the poll line names what the score read" );
		facts = facts with { ShopKindsStanding = 3, Shops = 40, Rides = 0, RideKindsStanding = 0 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "nor three kinds" );
		Set( settings, "ShopsBuildMoreShops.ShopsPerRideRatio", "1" );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "a ratio of one leaves the two sides equal, which is not more" );

		// A ratio above one: the score, with no ride and no kind of ride standing, and whatever the shops counted.
		Set( settings, "ShopsBuildMoreShops.ShopsPerRideRatio", "1.5" );
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ), "three kinds of shop times 1.5 are more than three" );
		Assert.AreEqual( 70, messages.ScoreOf( topic, 2000 ), "the park's age is not in the sum" );
		Assert.AreEqual( " shopkinds=3 ratio=1.5 balance=50000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { ShopKindsStanding = 1, Shops = 0, Rides = 99, RideKindsStanding = 4, Closed = true, Sideshows = 0, DaysSinceStaffHired = 99, ShopKindsOnOffer = 0 };
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ), "the rides, the shops counted, the door and the newest hire are not asked" );

		// A kind of shop must stand, thirty days after the newest build, and the bank over its limit.
		facts = facts with { ShopKindsStanding = 0, Shops = 5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "shops that are no kind standing (a golden ticket's)" );
		facts = facts with { ShopKindsStanding = 1, DaysSinceObjectBuilt = 29 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "day 29" );
		Assert.AreEqual( " shopkinds=1 ratio=1.5 balance=50000 sincebuild=29", messages.PollFacts( topic ) );
		facts = facts with { DaysSinceObjectBuilt = 30, Balance = 20000 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "20000 is not more than 20000" );
		Assert.AreEqual( " shopkinds=1 ratio=1.5 balance=20000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { Balance = 20001 };
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ) );
		facts = facts with { Balance = -5 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "a park in the red" );
		facts = facts with { Balance = 50000 };

		// Row 73, the park with no shop, is its own.
		var none = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildShops );
		Assert.IsNull( none.ShopsPerRide );
		Assert.IsNull( topic.PerDayWithNoShop );
		Assert.AreEqual( (0, 70), (messages.ScoreOf( none, 965 ), messages.ScoreOf( topic, 966 )), "a kind of shop stands" );
		Assert.AreEqual( " ridekinds=4 shopkinds=1 sincebuild=30", messages.PollFacts( none ), "row 73's poll line is its own" );
		facts = facts with { ShopKindsStanding = 0 };
		Assert.AreEqual( (132, 0), (messages.ScoreOf( none, 965 ), messages.ScoreOf( topic, 966 )), "and none does" );
		facts = facts with { ShopKindsStanding = 1 };

		// The keys are each its own.
		Set( settings, "ShopsBuildMoreShops.Score", 71 );
		Assert.AreEqual( 71, messages.ScoreOf( topic, 966 ) );
		Set( settings, "ShopsBuildMoreShops.Score", 70 );
		Set( settings, "ShopsBuildMoreShops.WealthMoreThan", 50000 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ) );
		Set( settings, "ShopsBuildMoreShops.WealthMoreThan", 49999 );
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ) );
		Set( settings, "ShopsBuildMoreShops.WealthMoreThan", 20000 );
		Set( settings, "ShopsBuildShops.Score", 1 );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 9 );
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ), "row 73's section is another row's" );
		Set( settings, "ShopsBuildShops.Score", 50 );
		Set( settings, "ShopsBuildShops.AdditionalPerDay", 2 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 966 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 999 );
		Assert.AreEqual( 70, messages.ScoreOf( topic, 966 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		Round( 964 );
		CollectionAssert.AreEqual( new[] { (964, 213, 211) }, said );
		CollectionAssert.AreEqual( new[] { (76, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=74/score=70/line=0/lastTick=964/said=1/slaps=0" );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 964 + 479 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 74 ), said.Count) );
		Round( 964 + 480 );
		Round( 964 + 960 );
		CollectionAssert.AreEqual( new[] { (964, 213, 211), (1444, 214, 212), (1924, 213, 211) }, said, "two lines in turn, then the first again" );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// The shipped ratio again: silent.
		Set( settings, "ShopsBuildMoreShops.ShopsPerRideRatio", "0.15" );
		Round( 1924 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 74 ), said.Count) );
	}

	/// <summary>
	/// Row 78, message 0x4e, is polled (0x0059e110): nought where no kind of sideshow stands, nought until
	/// WaitingTimes.DaysForObjects whole days after the newest object was made, nought unless the kinds of ride
	/// standing times SideshowsBuildMoreSideshows.SideshowsPerRideRatio, 0.2, are more than the kinds of sideshow
	/// standing, nought unless the bank holds more than WealthMoreThan, 20000, and otherwise Score, 80: two lines
	/// in turn in the hard hat with tag 80, whose bar carries the buy button of type 11. The ratio is a float and
	/// the product a double (the x87's control word reads 0x27f at the compare), so under the shipped ratio five
	/// kinds of ride to one of sideshow are enough: five times the float nearest 0.2 is a hair over one.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the sideshows set against themselves, as row 74 sets the shops; the rides or the sideshows
	/// counted in place of their kinds; the shops' kinds on either side; the comparison turned, or let by at
	/// equal; the kinds of sideshow not asked; the days not asked, the staff's, or thirty not enough; the bank
	/// not asked, or let by at the limit; the keys row 74's or row 77's; rows 74 and 77 scored so; the row not
	/// polled; the lines, the response, the sample, the prop or the group another's; tag 80's row another's; the
	/// poll line's words.
	/// </remarks>
	[TestMethod]
	public void TooFewSideshowsForTheParksSizeSetsTheKindsOfRideAgainstTheKindsOfSideshow()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 78 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		// Every count but the two the score sets against each other is a number of its own.
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 14, Sideshows: 9, WaitingToOpen: 0, Shops: 7, Toilets: 3, Mechanics: 1,
			DaysSinceStaffHired: 0, RideKindsOnOffer: 8, RideKindsStanding: 6, DaysSinceObjectBuilt: 30, ShopKindsOnOffer: 3, ShopKindsStanding: 2, Balance: 50000,
			SideshowKindsOnOffer: 5, SideshowKindsStanding: 1 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Round( int at )
		{
			cursor.Row = 78;
			messages.Tick( at, at * 250L, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		}

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildMoreSideshows );
		Assert.AreEqual( (0x4e, "SideshowsBuildMoreSideshows.Score", "SideshowsBuildMoreSideshows", 221, 219, 2, 1, 0, true, 80, ParkAdvisorMessages.HardHat, -1),
			(topic.Id, topic.ScoreKey, topic.SideshowsPerRide, topic.FirstResponse, topic.FirstSample, topic.Lines, topic.MaxInstances, topic.Category, topic.Polled, topic.Group, topic.Prop, topic.GestureRowLine) );
		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Count( one => one.SideshowsPerRide != null ), "no other row is scored so" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 78 ) );
		foreach ( var response in new[] { 221, 222 } )
			Assert.AreEqual( (ParkAdvisorMessages.HardHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		var row = ParkTags.Rows.Single( one => one.Id == 80 );
		Assert.AreEqual( (ParkTags.NoTag, 2, 82, 11, 0, 30000, 0), (row.Key, row.Picture, row.String, row.Button1, row.Button2, row.Life, row.Kind) );

		// The shipped ratio: five kinds of ride times the float nearest 0.2, 0.20000000298, are more than one kind of sideshow, and four are not.
		facts = facts with { RideKindsStanding = 4 };
		Round( 970 );
		Assert.AreEqual( (0, "no-score", 79, 0), (messages.ScoreOf( topic, 970 ), messages.PollAnswer( 78 ), cursor.Row, said.Count) );
		Assert.AreEqual( " sideshowkinds=1 ridekinds=4 ratio=0.2 balance=50000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { RideKindsStanding = 5 };
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "five kinds of ride to one of sideshow: the product is not rounded to a float, which would make it one" );
		Assert.AreEqual( 80, messages.ScoreOf( topic, 2000 ), "the park's age is not in the sum" );
		Assert.AreEqual( " sideshowkinds=1 ridekinds=5 ratio=0.2 balance=50000 sincebuild=30", messages.PollFacts( topic ), "the poll line names what the score read" );
		facts = facts with { RideKindsStanding = 10, SideshowKindsStanding = 2 };
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "ten to two" );
		facts = facts with { RideKindsStanding = 9 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "nine to two" );
		Set( settings, "SideshowsBuildMoreSideshows.SideshowsPerRideRatio", "0.25" );
		facts = facts with { RideKindsStanding = 4, SideshowKindsStanding = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "a quarter is a float exactly: four kinds are one, which is not more than one" );

		// The rides are counted, where row 74 counts its shops twice: under a ratio above one the sideshows' own kinds would be more than themselves.
		Set( settings, "SideshowsBuildMoreSideshows.SideshowsPerRideRatio", "1.5" );
		facts = facts with { RideKindsStanding = 1, SideshowKindsStanding = 2 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "one kind of ride times 1.5 is not more than two kinds of sideshow" );
		Assert.AreEqual( " sideshowkinds=2 ridekinds=1 ratio=1.5 balance=50000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { RideKindsStanding = 0, SideshowKindsStanding = 1 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "no kind of ride" );
		facts = facts with { RideKindsStanding = 1 };
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "one kind of each under 1.5" );
		Round( 970 );
		Assert.AreEqual( ("post", 79, 1), (messages.PollAnswer( 78 ), cursor.Row, said.Count) );
		CollectionAssert.AreEqual( new[] { (970, 221, 219) }, said );
		CollectionAssert.AreEqual( new[] { (80, 0) }, groups );
		StringAssert.Contains( messages.Census(), "message=78/score=80/line=0/lastTick=970/said=1/slaps=0" );
		facts = facts with { Closed = true, Rides = 0, Sideshows = 0, Shops = 0, ShopKindsStanding = 0, ShopKindsOnOffer = 0, RideKindsOnOffer = 0, SideshowKindsOnOffer = 0, DaysSinceStaffHired = 99 };
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "the things counted, the shops, what is on offer, the door and the newest hire are not asked" );
		Set( settings, "SideshowsBuildMoreSideshows.SideshowsPerRideRatio", "1" );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "a ratio of one leaves one kind against one, which is not more" );
		Set( settings, "SideshowsBuildMoreSideshows.SideshowsPerRideRatio", "1.5" );

		// A kind of sideshow must stand, thirty days after the newest build, and the bank over its limit.
		facts = facts with { SideshowKindsStanding = 0, Sideshows = 5, RideKindsStanding = 4 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "no kind of sideshow stands, though four kinds of ride times 1.5 are more than nought: row 77's park" );
		Assert.AreEqual( " sideshowkinds=0 ridekinds=4 ratio=1.5 balance=50000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { SideshowKindsStanding = 1, RideKindsStanding = 1, DaysSinceObjectBuilt = 29 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "day 29" );
		Assert.AreEqual( " sideshowkinds=1 ridekinds=1 ratio=1.5 balance=50000 sincebuild=29", messages.PollFacts( topic ) );
		facts = facts with { DaysSinceObjectBuilt = 30, Balance = 20000 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "20000 is not more than 20000" );
		Assert.AreEqual( " sideshowkinds=1 ridekinds=1 ratio=1.5 balance=20000 sincebuild=30", messages.PollFacts( topic ) );
		facts = facts with { Balance = 20001 };
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ) );
		facts = facts with { Balance = -50000 };
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "a park in the red by more than the limit: the compare is signed" );
		facts = facts with { Balance = 50000 };

		// Rows 74 and 77 are their own.
		var shops = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildMoreShops );
		var none = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.BuildSideshows );
		Assert.IsNull( shops.SideshowsPerRide );
		Assert.IsNull( none.SideshowsPerRide );
		Assert.IsNull( topic.ShopsPerRide );
		Assert.IsNull( topic.PerDayWithNoSideshow );
		facts = facts with { ShopKindsStanding = 1 };
		Assert.AreEqual( (0, 0, 80), (messages.ScoreOf( shops, 966 ), messages.ScoreOf( none, 969 ), messages.ScoreOf( topic, 970 )), "the shipped shops' ratio, and a kind of sideshow stands" );
		Assert.AreEqual( " shopkinds=1 ratio=0.15 balance=50000 sincebuild=30", messages.PollFacts( shops ), "row 74's poll line is its own" );
		Assert.AreEqual( " shopkinds=1 sideshowkinds=1 sincebuild=30", messages.PollFacts( none ), "and row 77's" );

		// The keys are each its own.
		Set( settings, "SideshowsBuildMoreSideshows.Score", 81 );
		Assert.AreEqual( 81, messages.ScoreOf( topic, 970 ) );
		Set( settings, "SideshowsBuildMoreSideshows.Score", 80 );
		Set( settings, "SideshowsBuildMoreSideshows.WealthMoreThan", 50000 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ) );
		Set( settings, "SideshowsBuildMoreSideshows.WealthMoreThan", 49999 );
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ) );
		Set( settings, "SideshowsBuildMoreSideshows.WealthMoreThan", 20000 );
		Set( settings, "ShopsBuildMoreShops.ShopsPerRideRatio", "0.01" );
		Set( settings, "ShopsBuildMoreShops.WealthMoreThan", 999999 );
		Set( settings, "ShopsBuildMoreShops.Score", 1 );
		Set( settings, "SideshowsBuildSideshows.Score", 1 );
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "rows 74 and 77's sections are other rows'" );
		Set( settings, "ShopsBuildMoreShops.ShopsPerRideRatio", "0.15" );
		Set( settings, "ShopsBuildMoreShops.WealthMoreThan", 20000 );
		Set( settings, "ShopsBuildMoreShops.Score", 70 );
		Set( settings, "SideshowsBuildSideshows.Score", -70 );
		Set( settings, "WaitingTimes.DaysForObjects", 31 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 970 ), "day 30 is before day 31" );
		Set( settings, "WaitingTimes.DaysForObjects", 30 );
		Set( settings, "WaitingTimes.DaysForStaff", 999 );
		Assert.AreEqual( 80, messages.ScoreOf( topic, 970 ), "the staff's days are another row's" );
		Set( settings, "WaitingTimes.DaysForStaff", 30 );

		// Inside 120 quarter-sweeps of it the gate refuses the same message; past them the other line is said, and then the first again.
		var gestures = Counted( "ADVISOR_RESPONSE_GESTURE_ROW" );
		Round( 1447 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 78 ), said.Count), "quarter-sweep 361 is 119 after 242" );
		Round( 1448 );
		Round( 1928 );
		CollectionAssert.AreEqual( new[] { (970, 221, 219), (1448, 222, 220), (1928, 221, 219) }, said, "two lines in turn, then the first again" );
		CollectionAssert.AreEqual( new[] { (80, 0), (80, 0), (80, 0) }, groups );
		Assert.AreEqual( gestures, Counted( "ADVISOR_RESPONSE_GESTURE_ROW" ), "every line's gesture is the random chain" );

		// The shipped ratio again, one kind of ride: silent.
		Set( settings, "SideshowsBuildMoreSideshows.SideshowsPerRideRatio", "0.2" );
		Round( 1928 + 480 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 78 ), said.Count) );
	}

	/// <summary>
	/// Rows 260 to 262, messages 0x104 to 0x106, are polled and scored by the share of the park's shops, of its
	/// sideshows and of its toilets that are shut (0x005a0370, 0x005a03d0, 0x005a0430): nought with none standing,
	/// nought at or under the section's MoreThanThisPercentClosed in whole per cent, and the section's Score over
	/// it, whatever the door. Each is three lines in the ticket hat with no tag. In a shut park the shops' line is
	/// said on its own sweep, the other two wait in the slots through it, and the higher score is said first.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a share at its bound scored; the share not a whole number's; the shut counted of another
	/// kind, or the standing; none standing scored; the door asked; a row's key, section, response, sample, lines,
	/// prop or group another's; a row not polled; the bound another section's.
	/// </remarks>
	[TestMethod]
	public void ShutShopsSideshowsAndToiletsArePolledByTheShareShut()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 260 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 1, Sideshows: 0, WaitingToOpen: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Sweep( int at, long milliseconds )
			=> messages.Tick( at, milliseconds, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );
		ParkAdvisorMessages.Topic Row( int id ) => ParkAdvisorMessages.ParkTopics.Single( one => one.Id == id );

		var (shops, sideshows, toilets) = (Row( ParkAdvisorMessages.ShopsClosed ), Row( ParkAdvisorMessages.SideshowsClosed ), Row( ParkAdvisorMessages.ToiletsClosed ));
		Assert.AreEqual( (260, 261, 262), (shops.Id, sideshows.Id, toilets.Id) );
		Assert.AreEqual( (504, 487, 507, 490, 510, 493), (shops.FirstResponse, shops.FirstSample, sideshows.FirstResponse, sideshows.FirstSample, toilets.FirstResponse, toilets.FirstSample) );
		foreach ( var row in new[] { shops, sideshows, toilets } )
		{
			Assert.AreEqual( (3, 1, 0, true, ParkTags.NoTag, ParkAdvisorMessages.TicketHat, -1),
				(row.Lines, row.MaxInstances, row.Category, row.Polled, row.Group, row.Prop, row.GestureRowLine), $"row {row.Id}" );
			Assert.AreEqual( 0, messages.ScoreOf( row, 100 ), "before any sweep has handed the park in" );
		}
		Assert.AreEqual( ("ShopsClosed.Score", "ShopsClosed", "SideshowsClosed.Score", "SideshowsClosed", "ToiletsClosed.Score", "ToiletsClosed"),
			(shops.ScoreKey, shops.ShutSection, sideshows.ScoreKey, sideshows.ShutSection, toilets.ScoreKey, toilets.ShutSection) );
		for ( var response = 504; response < 513; ++response )
			Assert.AreEqual( (ParkAdvisorMessages.TicketHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 513 ), "row 263's one line wears nothing" );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 500 ), "the response before row 259's first is of no built row" );

		// The first sweep hands the park in: an open park with nothing shut, and row 260 scores nought.
		Sweep( 1, 0 );
		Assert.AreEqual( ("no-score", 261), (messages.PollAnswer( 260 ), cursor.Row) );

		int Score( ParkAdvisorMessages.Topic row, ParkAdvisorMessages.ParkFacts park )
		{
			facts = park;
			return messages.ScoreOf( row, 1 );
		}

		// Each kind's own counts, and no other's; the share in whole per cent, strictly over the bound.
		var open = new ParkAdvisorMessages.ParkFacts( false, 1, 0, 0 );
		Assert.AreEqual( 0, Score( shops, open with { Shops = 0, ShopsShut = 3, Sideshows = 4, SideshowsShut = 4, Toilets = 4, ToiletsShut = 4 } ), "no shop standing" );
		Assert.AreEqual( 0, Score( shops, open with { Shops = 4, ShopsShut = 1, Sideshows = 4, SideshowsShut = 4, Toilets = 4, ToiletsShut = 4 } ), "25 per cent is not over 25" );
		Assert.AreEqual( 0, Score( shops, open with { Shops = 27, ShopsShut = 7 } ), "25.9 per cent is 25" );
		Assert.AreEqual( 75, Score( shops, open with { Shops = 50, ShopsShut = 13 } ), "26 per cent" );
		Assert.AreEqual( 75, Score( shops, open with { Shops = 1, ShopsShut = 1 } ) );
		Assert.AreEqual( 0, Score( sideshows, open with { Sideshows = 4, SideshowsShut = 1, Shops = 4, ShopsShut = 4, Toilets = 4, ToiletsShut = 4 } ) );
		Assert.AreEqual( 70, Score( sideshows, open with { Sideshows = 50, SideshowsShut = 13 } ) );
		Assert.AreEqual( 0, Score( sideshows, open with { Sideshows = 0, SideshowsShut = 2, Shops = 1, ShopsShut = 1 } ), "no sideshow standing" );
		Assert.AreEqual( 0, Score( toilets, open with { Toilets = 5, ToiletsShut = 1, Shops = 4, ShopsShut = 4, Sideshows = 4, SideshowsShut = 4 } ), "20 per cent is not over 20" );
		Assert.AreEqual( 75, Score( toilets, open with { Toilets = 100, ToiletsShut = 21 } ), "21 per cent, under the shops' bound" );
		Assert.AreEqual( 0, Score( shops, open with { Shops = 100, ShopsShut = 21 } ), "21 per cent of the shops" );
		Assert.AreEqual( 0, Score( toilets, open with { Toilets = 0, ToiletsShut = 1 } ), "no toilet standing" );
		Assert.AreEqual( 75, Score( toilets, new ParkAdvisorMessages.ParkFacts( true, 0, 0, 0, Toilets: 3, ToiletsShut: 3 ) ), "the door is not asked" );

		// The shipped park with its door shut: one shop, one sideshow and three toilets, all shut. The shops' line
		// is said on its poll's sweep; the other two are posted on theirs and wait through it.
		facts = new ParkAdvisorMessages.ParkFacts( true, 1, 1, 0, Shops: 1, Toilets: 3, ShopsShut: 1, SideshowsShut: 1, ToiletsShut: 3 );
		cursor.Row = 260;
		Sweep( 899, 100_000 );
		CollectionAssert.AreEqual( new[] { (899, 504, 487) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		Sweep( 900, 100_250 );
		Sweep( 901, 100_500 );
		Assert.AreEqual( ("post", "post", "post", 263), (messages.PollAnswer( 260 ), messages.PollAnswer( 261 ), messages.PollAnswer( 262 ), cursor.Row) );
		Assert.AreEqual( (2, 1), (messages.PendingCount, said.Count) );
		StringAssert.Contains( messages.Census(), "slot=261/subject=0 slot=262/subject=0 " );
		StringAssert.Contains( messages.Census(), "message=260/score=75/line=0/lastTick=899/said=1/slaps=0" );

		// The wait is the line's 5000 ms and a second: the toilets' 75 goes before the sideshows' 70.
		cursor.Row = 300;
		Sweep( 924, 105_999 );
		Assert.AreEqual( 1, said.Count );
		Sweep( 925, 106_000 );
		Sweep( 926, 106_250 );
		Assert.AreEqual( 2, said.Count, "one line a wait" );
		Sweep( 949, 112_000 );
		CollectionAssert.AreEqual( new[] { (899, 504, 487), (925, 510, 493), (949, 507, 490) }, said );
		Assert.AreEqual( 0, messages.PendingCount );

		// The same message is refused inside 120 quarter-sweeps of its line (899 is in quarter 224), and past them
		// says its next line.
		cursor.Row = 260;
		Sweep( 1375, 300_000 );
		Assert.AreEqual( ("cooldown", 3), (messages.PollAnswer( 260 ), said.Count) );
		cursor.Row = 260;
		Sweep( 1376, 300_250 );
		Assert.AreEqual( (1376, 505, 488), said[^1] );

		// The shop opened again, its next turn scores nought.
		facts = facts with { ShopsShut = 0 };
		cursor.Row = 260;
		Sweep( 3000, 900_000 );
		Assert.AreEqual( "no-score", messages.PollAnswer( 260 ) );
	}

	/// <summary>
	/// Row 263, message 0x107, is polled and scored by the share of the park's staff rooms that are shut
	/// (0x005a0490): nought with none standing, nought at or under StaffroomClosed.MoreThanThisPercentClosed in
	/// whole per cent, and StaffroomClosed.Score over it, whatever the door. It is one line, response 513, whose
	/// sample is 638, with no prop and no tag: said on its poll's sweep, refused inside its cooldown and said
	/// again past it.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the share at its bound scored; the shut or the standing counted of the toilets; the door
	/// asked; the bound or the score another section's; the row's response, sample, lines or prop another's; the
	/// row not polled, or not a topic.
	/// </remarks>
	[TestMethod]
	public void AShutStaffRoomIsPolledByTheShareShut()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 263 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 1, Sideshows: 1, WaitingToOpen: 0, StaffRooms: 1 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Sweep( int at, long milliseconds )
			=> messages.Tick( at, milliseconds, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );

		var room = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.StaffroomClosed );
		Assert.AreEqual( (263, 513, 638, 1, 1, 0, true, ParkTags.NoTag, 0, -1),
			(room.Id, room.FirstResponse, room.FirstSample, room.Lines, room.MaxInstances, room.Category, room.Polled, room.Group, room.Prop, room.GestureRowLine) );
		Assert.AreEqual( ("StaffroomClosed.Score", "StaffroomClosed", null, ParkAdvisorMessages.Shut.StaffRooms),
			(room.ScoreKey, room.ShutSection, room.OpenWithAtLeast, room.ShutShare) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 263 ) );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 513 ) );
		Assert.AreEqual( 0, messages.ScoreOf( room, 100 ), "before any sweep has handed the park in" );

		// The first sweep hands the park in: one staff room, open.
		Sweep( 1, 0 );
		Assert.AreEqual( ("no-score", 264), (messages.PollAnswer( 263 ), cursor.Row) );
		Assert.AreEqual( 0, said.Count );

		int Score( ParkAdvisorMessages.ParkFacts park )
		{
			facts = park;
			return messages.ScoreOf( room, 1 );
		}

		var open = new ParkAdvisorMessages.ParkFacts( false, 1, 1, 0, StaffRooms: 1 );
		Assert.AreEqual( 90, Score( open with { StaffRoomsShut = 1 } ) );
		Assert.AreEqual( 0, Score( open with { StaffRooms = 4, StaffRoomsShut = 1 } ), "25 per cent is not over 25" );
		Assert.AreEqual( 0, Score( open with { StaffRooms = 27, StaffRoomsShut = 7 } ), "25.9 per cent is 25" );
		Assert.AreEqual( 90, Score( open with { StaffRooms = 50, StaffRoomsShut = 13 } ), "26 per cent" );
		Assert.AreEqual( 0, Score( open with { StaffRooms = 0, StaffRoomsShut = 1, Toilets = 3, ToiletsShut = 3, Shops = 1, ShopsShut = 1 } ), "the standing are the staff rooms" );
		Assert.AreEqual( 0, Score( open with { RidesShut = 1, Shops = 1, ShopsShut = 1, SideshowsShut = 1, Toilets = 3, ToiletsShut = 3 } ), "the shut are the staff rooms" );
		Assert.AreEqual( 90, Score( open with { Closed = true, StaffRoomsShut = 1 } ), "the door is not asked" );
		Assert.AreEqual( 90, Score( new ParkAdvisorMessages.ParkFacts( false, 0, 0, 0, StaffRooms: 2, StaffRoomsShut: 1 ) ), "a park with nothing to ride" );

		// The bound and the score are the staff rooms' section's own.
		Set( settings, "StaffroomClosed.MoreThanThisPercentClosed", 50 );
		Assert.AreEqual( 0, Score( open with { StaffRooms = 2, StaffRoomsShut = 1 } ), "50 per cent is not over 50" );
		Set( settings, "StaffroomClosed.MoreThanThisPercentClosed", 25 );
		Set( settings, "ShopsClosed.MoreThanThisPercentClosed", 99 );
		Set( settings, "ToiletsClosed.MoreThanThisPercentClosed", 99 );
		Assert.AreEqual( 90, Score( open with { StaffRooms = 2, StaffRoomsShut = 1 } ), "the shops' and the toilets' bounds are not the staff rooms'" );
		Set( settings, "ShopsClosed.MoreThanThisPercentClosed", 25 );
		Set( settings, "ToiletsClosed.MoreThanThisPercentClosed", 20 );

		// One staff room, shut: the line is said on its poll's sweep and leaves no tag.
		facts = open with { StaffRoomsShut = 1 };
		cursor.Row = 263;
		Sweep( 902, 100_000 );
		CollectionAssert.AreEqual( new[] { (902, 513, 638) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		Assert.AreEqual( ("post", 264, 0), (messages.PollAnswer( 263 ), cursor.Row, messages.PendingCount) );
		StringAssert.Contains( messages.Census(), "message=263/score=90/line=0/lastTick=902/said=1/slaps=0" );

		// Refused inside 120 quarter-sweeps of its line (902 is in quarter 225), and past them it is said again: it
		// has no second line.
		cursor.Row = 263;
		Sweep( 1379, 300_000 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 263 ), said.Count) );
		cursor.Row = 263;
		Sweep( 1380, 300_250 );
		Assert.AreEqual( (1380, 513, 638), said[^1] );
		Assert.AreEqual( 2, said.Count );

		// The staff room opened again, its next turn scores nought.
		facts = facts with { StaffRoomsShut = 0 };
		cursor.Row = 263;
		Sweep( 3000, 900_000 );
		Assert.AreEqual( "no-score", messages.PollAnswer( 263 ) );
	}

	/// <summary>
	/// Row 259, message 0x103, is polled and scored by the share of the rides that are shut (0x005a02f0): nought in
	/// a shut park, nought with fewer rides standing than RidesClosed.AtLeastThisManyRides, nought at or under
	/// RidesClosed.MoreThanThisPercentClosed in whole per cent, and RidesClosed.Score over it. It is three lines in
	/// the ticket hat with no tag, said on its poll's sweep, refused inside its cooldown and saying its next line
	/// past it.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the door not asked, or turned round; the least count not asked, or one fewer let by; the
	/// share at its bound scored; the shut counted of another kind, or the standing; the bound or the least another
	/// section's; the row's key, response, sample, lines, prop or group another's; the row not polled.
	/// </remarks>
	[TestMethod]
	public void AnOpenParksShutRidesArePolledByTheShareShut()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 259 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 3, Sideshows: 1, WaitingToOpen: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Sweep( int at, long milliseconds )
			=> messages.Tick( at, milliseconds, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );

		var rides = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.RidesClosed );
		Assert.AreEqual( (259, 501, 484, 3, 1, 0, true, ParkTags.NoTag, ParkAdvisorMessages.TicketHat, -1),
			(rides.Id, rides.FirstResponse, rides.FirstSample, rides.Lines, rides.MaxInstances, rides.Category, rides.Polled, rides.Group, rides.Prop, rides.GestureRowLine) );
		Assert.AreEqual( ("RidesClosed.Score", "RidesClosed", "RidesClosed.AtLeastThisManyRides", ParkAdvisorMessages.Shut.Rides),
			(rides.ScoreKey, rides.ShutSection, rides.OpenWithAtLeast, rides.ShutShare) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 259 ) );
		for ( var response = 501; response < 504; ++response )
			Assert.AreEqual( (ParkAdvisorMessages.TicketHat, 0), ParkAdvisorMessages.PropsOf( response ) );
		Assert.AreEqual( 0, messages.ScoreOf( rides, 100 ), "before any sweep has handed the park in" );
		foreach ( var other in ParkAdvisorMessages.ParkTopics.Where( one => one.Id != ParkAdvisorMessages.RidesClosed ) )
			Assert.IsNull( other.OpenWithAtLeast, $"row {other.Id} asks neither the door nor a least count" );

		// The first sweep hands the park in: three rides, none shut.
		Sweep( 1, 0 );
		Assert.AreEqual( ("no-score", 260), (messages.PollAnswer( 259 ), cursor.Row) );

		int Score( ParkAdvisorMessages.ParkFacts park )
		{
			facts = park;
			return messages.ScoreOf( rides, 1 );
		}

		var open = new ParkAdvisorMessages.ParkFacts( false, 3, 0, 0 );
		Assert.AreEqual( 80, Score( open with { RidesShut = 2 } ), "66 per cent of three" );
		Assert.AreEqual( 80, Score( open with { RidesShut = 3 } ) );
		Assert.AreEqual( 0, Score( open with { RidesShut = 1 } ), "33 per cent" );
		Assert.AreEqual( 0, Score( open with { Rides = 4, RidesShut = 2 } ), "50 per cent is not over 50" );
		Assert.AreEqual( 0, Score( open with { Rides = 100, RidesShut = 50, Shops = 100, ShopsShut = 100 } ), "the shops are not counted" );
		Assert.AreEqual( 80, Score( open with { Rides = 100, RidesShut = 51 } ) );
		Assert.AreEqual( 0, Score( open with { Rides = 199, RidesShut = 101 } ), "50.7 per cent is 50" );
		Assert.AreEqual( 0, Score( open with { Rides = 2, RidesShut = 2 } ), "two rides are fewer than three" );
		Assert.AreEqual( 0, Score( open with { Rides = 0, RidesShut = 0 } ), "no ride standing" );
		Assert.AreEqual( 0, Score( open with { Rides = 0, RidesShut = 4, Shops = 4, ShopsShut = 4, Sideshows = 4, SideshowsShut = 4, Toilets = 4, ToiletsShut = 4 } ), "the standing are the rides" );
		Assert.AreEqual( 0, Score( open with { Closed = true, RidesShut = 3 } ), "a shut park" );
		Assert.AreEqual( 0, Score( open with { RidesShut = 0, Shops = 3, ShopsShut = 3, Sideshows = 3, SideshowsShut = 3, Toilets = 3, ToiletsShut = 3 } ), "the shut are the rides" );

		// The least count and the bound are the rides' section's own.
		Set( settings, "RidesClosed.AtLeastThisManyRides", 4 );
		Assert.AreEqual( 0, Score( open with { RidesShut = 3 } ), "three rides where four are asked" );
		Assert.AreEqual( 80, Score( open with { Rides = 4, RidesShut = 3 } ) );
		Set( settings, "RidesClosed.AtLeastThisManyRides", 3 );
		Set( settings, "RidesClosed.MoreThanThisPercentClosed", 66 );
		Assert.AreEqual( 0, Score( open with { RidesShut = 2 } ), "66 per cent is not over 66" );
		Set( settings, "RidesClosed.MoreThanThisPercentClosed", 50 );
		Set( settings, "ShopsClosed.MoreThanThisPercentClosed", 99 );
		Set( settings, "ShopsClosed.Score", 1 );
		Assert.AreEqual( 80, Score( open with { RidesShut = 2 } ), "the shops' bound and score are not the rides'" );
		Set( settings, "ShopsClosed.MoreThanThisPercentClosed", 25 );
		Set( settings, "ShopsClosed.Score", 75 );

		// An open park with three rides, two of them shut: the line is said on its poll's sweep and leaves no tag.
		facts = open with { RidesShut = 2 };
		cursor.Row = 259;
		Sweep( 898, 100_000 );
		CollectionAssert.AreEqual( new[] { (898, 501, 484) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		Assert.AreEqual( ("post", 260, 0), (messages.PollAnswer( 259 ), cursor.Row, messages.PendingCount) );
		StringAssert.Contains( messages.Census(), "message=259/score=80/line=0/lastTick=898/said=1/slaps=0" );

		// Refused inside 120 quarter-sweeps of its line (898 is in quarter 224), and past them it says its next line.
		cursor.Row = 259;
		Sweep( 1375, 300_000 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 259 ), said.Count) );
		cursor.Row = 259;
		Sweep( 1376, 300_250 );
		Assert.AreEqual( (1376, 502, 485), said[^1] );

		// One of the two opened, its next turn scores nought.
		facts = facts with { RidesShut = 1 };
		cursor.Row = 259;
		Sweep( 3000, 900_000 );
		Assert.AreEqual( "no-score", messages.PollAnswer( 259 ) );
	}

	/// <summary>
	/// Row 113, message 0x71, is polled and scored by a queue not joined to a path (0x0059ef60):
	/// QueueUnconnected.Score where the park hands in a ride whose queue is unjoined, and nought with none,
	/// whatever the door and the counts. It is two lines with no prop, said on its poll's sweep about no thing,
	/// which leaves tag 135; refused inside its cooldown and saying its second line past it.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the score given with no queue unjoined, or refused with one; the door or the rides
	/// asked; the row's key, response, sample, lines, copies, prop or group another's; the row not polled; the
	/// thing handed on as the line's subject.
	/// </remarks>
	[TestMethod]
	public void AQueueNotJoinedToAPathIsPolledAndSpokenOf()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 113 };
		var settings = Settings();
		var messages = new ParkAdvisorMessages( settings, cursor: cursor );
		var facts = new ParkAdvisorMessages.ParkFacts( Closed: false, Rides: 2, Sideshows: 1, WaitingToOpen: 0 );
		var said = new List<(int Tick, int Response, int Sample)>();
		var groups = new List<(int, int)>();
		void Sweep( int at, long milliseconds )
			=> messages.Tick( at, milliseconds, ( response, sample ) => { said.Add( (at, response, sample) ); return 5000; },
				( group, subject ) => groups.Add( (group, subject) ), () => facts );

		var queue = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.QueueUnconnected );
		Assert.AreEqual( (113, 288, 312, 2, 255, 0, true, 135, 0, -1),
			(queue.Id, queue.FirstResponse, queue.FirstSample, queue.Lines, queue.MaxInstances, queue.Category, queue.Polled, queue.Group, queue.Prop, queue.GestureRowLine) );
		Assert.AreEqual( ("QueueUnconnected.Score", true), (queue.ScoreKey, queue.QueueNotJoined) );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 113 ) );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 288 ) );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 289 ) );
		Assert.AreEqual( 0, messages.ScoreOf( queue, 100 ), "before any sweep has handed the park in" );
		foreach ( var other in ParkAdvisorMessages.ParkTopics.Where( one => one.Id != ParkAdvisorMessages.QueueUnconnected ) )
			Assert.IsFalse( other.QueueNotJoined, $"row {other.Id} asks no queue" );

		// The first sweep hands the park in: every queue joined.
		Sweep( 1, 0 );
		Assert.AreEqual( ("no-score", 114, 0), (messages.PollAnswer( 113 ), cursor.Row, said.Count) );

		int Score( ParkAdvisorMessages.ParkFacts park )
		{
			facts = park;
			return messages.ScoreOf( queue, 1 );
		}

		var joined = new ParkAdvisorMessages.ParkFacts( false, 2, 1, 0 );
		Assert.AreEqual( 87, Score( joined with { UnjoinedQueue = 44 } ) );
		Assert.AreEqual( 87, Score( joined with { UnjoinedQueue = 1 } ) );
		Assert.AreEqual( 87, Score( joined with { Closed = true, UnjoinedQueue = 44 } ), "the door is not asked" );
		Assert.AreEqual( 87, Score( new ParkAdvisorMessages.ParkFacts( false, 0, 0, 0, UnjoinedQueue: 44 ) ), "the counts are not asked" );
		Assert.AreEqual( 0, Score( joined ), "no queue unjoined" );
		Assert.AreEqual( 0, Score( joined with { Rides = 3, RidesShut = 3, Shops = 1, ShopsShut = 1, Toilets = 1, ToiletsShut = 1, WaitingToOpen = 9 } ), "a shut ride is not an unjoined queue" );
		Set( settings, "QueueUnconnected.Score", 31 );
		Set( settings, "RidesClosed.Score", 1 );
		Assert.AreEqual( 31, Score( joined with { UnjoinedQueue = 44 } ), "the score is its own key's" );
		Set( settings, "QueueUnconnected.Score", 87 );
		Set( settings, "RidesClosed.Score", 80 );

		// A ride's queue unjoined: the line is said on its poll's sweep, about no thing, and leaves tag 135.
		facts = joined with { UnjoinedQueue = 44 };
		cursor.Row = 113;
		Sweep( 996, 100_000 );
		CollectionAssert.AreEqual( new[] { (996, 288, 312) }, said );
		CollectionAssert.AreEqual( new[] { (135, 0) }, groups );
		Assert.AreEqual( ("post", 114, 0), (messages.PollAnswer( 113 ), cursor.Row, messages.PendingCount) );
		StringAssert.Contains( messages.Census(), "message=113/score=87/line=0/lastTick=996/said=1/slaps=0" );

		// Refused inside 120 quarter-sweeps of its line (996 is in quarter 249), and past them it says its second
		// line, and then its first again.
		cursor.Row = 113;
		Sweep( 1475, 300_000 );
		Assert.AreEqual( ("cooldown", 1), (messages.PollAnswer( 113 ), said.Count) );
		cursor.Row = 113;
		Sweep( 1476, 300_250 );
		Assert.AreEqual( (1476, 289, 313), said[^1] );
		cursor.Row = 113;
		Sweep( 2000, 600_000 );
		Assert.AreEqual( (2000, 288, 312), said[^1] );

		// The queue joined, its next turn scores nought.
		facts = joined;
		cursor.Row = 113;
		Sweep( 3000, 900_000 );
		Assert.AreEqual( ("no-score", 3), (messages.PollAnswer( 113 ), said.Count) );
	}

	/// <summary>
	/// The cursor is the park file's: the shipped park's row 277 has row 282 scored on the fifth sweep, tick 760
	/// of a park whose first sweep is 756, and row 185 on the 122nd, tick 877; a row below the table is refused.
	/// </summary>
	[TestMethod]
	public void AParkFilesPollCursorIsTakenAndGoneOnFrom()
	{
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 40 };
		var messages = new ParkAdvisorMessages( Settings(), cursor: cursor );
		Assert.IsTrue( messages.RestoreCursor( 277 ) );
		Assert.AreEqual( 277, cursor.Row );
		var said = new List<int>();
		for ( var tick = 756; tick < 1100; ++tick )
			messages.Tick( tick, tick * 250L, ( response, _ ) => { said.Add( tick ); return 1000; } );
		CollectionAssert.AreEqual( new[] { 760, 877 }, said );

		// A row past the table's end begins again at row 1.
		Assert.IsTrue( messages.RestoreCursor( 1000 ) );
		messages.Tick( 1100, 1100 * 250L, ( _, _ ) => throw new Exception( "said twice" ) );
		Assert.AreEqual( 2, cursor.Row );

		Assert.IsFalse( messages.RestoreCursor( -1 ) );
		Assert.AreEqual( messages.PollRow, cursor.Row );
		Assert.IsTrue( cursor.Row >= 0 );
	}

	/// <summary>
	/// The poll's bounds and its gate: on the row's sweep a park of ten days or of sixty is not posted, nor one
	/// whose file says the line was said, nor one with the Tutorial option off, nor a score not above
	/// MinScoreForConsideration; a second park takes the cursor where the first left it; and a row found while
	/// he waits out another line stands in a slot until the wait is over.
	/// </summary>
	[TestMethod]
	public void ThePollAsksTheAgeTheGateAndTheThreshold()
	{
		ParkAdvisorMessages On( int row, SettingsFile? settings = null, bool tutorial = true )
			=> new( settings ?? Settings(), tutorial: () => tutorial, cursor: new ParkAdvisorMessages.PollCursor { Row = row } );
		static int Never( int response, int sample ) => throw new Exception( $"said {response}" );

		var young = On( 185 );
		young.Tick( 253, 0, Never );
		Assert.AreEqual( 0, young.PendingCount, "day 10" );
		var old = On( 185 );
		old.Tick( 1383, 0, Never );
		Assert.AreEqual( 0, old.PendingCount, "day 60" );
		var beside = On( 173 );
		beside.Tick( 900, 0, Never );
		Assert.AreEqual( 0, beside.PendingCount, "the polled row before" );
		var after = On( 186 );
		after.Tick( 900, 0, Never );
		Assert.AreEqual( 0, after.PendingCount, "the polled row after, 238" );
		Assert.AreEqual( 239, after.PollRow );
		var from = On( 174 );
		var heard = 0;
		from.Tick( 900, 0, ( _, _ ) => { ++heard; return 1000; } );
		Assert.AreEqual( 1, heard, "rows 174 to 184 are not polled: the sweep's row is 185" );

		var off = On( 185, tutorial: false );
		off.Tick( 900, 0, Never );
		Assert.AreEqual( 0, off.PendingCount, "the Tutorial option off" );

		var low = Settings();
		Set( low, "TutorialUIPanel.Score", 25 );
		var under = On( 185, low );
		under.Tick( 900, 0, Never );
		Assert.AreEqual( 0, under.PendingCount, "25 is not above 25" );
		Set( low, "TutorialUIPanel.Score", 26 );
		var over = On( 185, low );
		heard = 0;
		over.Tick( 900, 0, ( _, _ ) => { ++heard; return 1000; } );
		Assert.AreEqual( 1, heard, "26 is" );

		// The shipped park's own record: said on tick 257.
		var shipped = On( 185 );
		shipped.Restore( id => id == ParkAdvisorMessages.ControlPanel ? new ParkWorld.AdvisorHistory( 257, 0, true, 0 ) : null );
		shipped.Tick( 900, 0, Never );
		Assert.IsNull( On( 0 ).PollAnswer( ParkAdvisorMessages.ControlPanel ) );
		Assert.AreEqual( 0, shipped.PendingCount, "said once, by the file" );
		Assert.AreEqual( "said-once", shipped.PollAnswer( ParkAdvisorMessages.ControlPanel ), "the poll asks the gate itself: the row is no answer, and nothing is posted" );
		Assert.AreEqual( "no-score", young.PollAnswer( ParkAdvisorMessages.ControlPanel ) );
		Assert.AreEqual( "tutorial-off", off.PollAnswer( ParkAdvisorMessages.ControlPanel ) );
		Assert.AreEqual( "under-threshold", under.PollAnswer( ParkAdvisorMessages.ControlPanel ) );
		Assert.AreEqual( "post", over.PollAnswer( ParkAdvisorMessages.ControlPanel ) );

		// One cursor, two parks: the second goes on from the first's row.
		var cursor = new ParkAdvisorMessages.PollCursor { Row = 172 };
		var first = new ParkAdvisorMessages( Settings(), cursor: cursor );
		first.Tick( 900, 0, Never );
		first.Tick( 901, 0, Never );
		var second = new ParkAdvisorMessages( Settings(), cursor: cursor );
		heard = 0;
		second.Tick( 900, 0, ( response, _ ) => { Assert.AreEqual( 404, response ); ++heard; return 1000; } );
		Assert.AreEqual( 1, heard );
		Assert.AreEqual( 186, cursor.Row );

		// Found inside another line's wait, it stands in a slot and is said as the wait ends.
		var busy = On( 173 );
		busy.Post( ParkAdvisorMessages.MechanicOnHisWay, 899, 13 );
		busy.Tick( 899, 1000, ( _, _ ) => 6000 );
		busy.Tick( 900, 1250, Never );
		StringAssert.Contains( busy.Census(), "pending=1 attempts=1 threshold=25 slot=185/subject=0 " );
		busy.Tick( 901, 7999, Never );
		heard = 0;
		busy.Tick( 902, 8000, ( response, _ ) => { Assert.AreEqual( 404, response ); ++heard; return 1000; } );
		Assert.AreEqual( 1, heard );

		// The score is the function's at the post and at the tick's second look, not the key's: a post outside
		// the days stands at nought and is never said, and one found on day 59 and still waiting on day 60 is let go unsaid.
		var early = On( 0 );
		Assert.IsTrue( early.Post( ParkAdvisorMessages.ControlPanel, 100 ) );
		early.Tick( 101, 0, Never );
		StringAssert.Contains( early.Census(), "pending=1 attempts=0 threshold=25 slot=185/subject=0 " );
		var late = On( 173 );
		late.Post( ParkAdvisorMessages.MechanicOnHisWay, 1381, 13 );
		late.Tick( 1381, 1000, ( _, _ ) => 6000 );
		late.Tick( 1382, 1250, Never );
		StringAssert.Contains( late.Census(), "pending=1 attempts=1 threshold=25 slot=185/subject=0 " );
		late.Tick( 1383, 8000, Never );
		StringAssert.Contains( late.Census(), "pending=0 attempts=1 " );

		// A score of nought is no answer whatever the threshold.
		var none = Settings( threshold: -1 );
		var nought = On( 185, none );
		nought.Tick( 253, 0, Never );
		Assert.AreEqual( 0, nought.PendingCount, "nought is not above nought" );

		Assert.AreEqual( 1, ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ControlPanel ).Lines );
		Assert.IsTrue( ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ControlPanel ).Polled );
		Assert.AreEqual( 25, ParkAdvisorMessages.ParkTopics.Count( one => one.Polled ) );
	}

	/// <summary>
	/// <b>Message 0x11a, the line about the tips, is polled</b> (row 282 of 0x0076e300: polled, category 1, score
	/// 0x005a0720, one copy, response 542 = sample 559, one line): the control panel's function on its own
	/// section, <c>TutorialExplainTooltips</c>, so it scores from day 21 to day 79 of a park. 147 polled rows stand
	/// before it; said, it leaves no tag, wears no prop and is never said again; past its last day a said record
	/// reads as no score, the score being asked before the gate.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the row another; the section the control panel's; the score key another's; the response
	/// or sample another's; the category nought; not polled; the day's bounds at or past their ends.
	/// </remarks>
	[TestMethod]
	public void TheTipsLineIsPolledOnItsOwnRowAndDaysAndSaidOnce()
	{
		ParkAdvisorMessages On( int row, SettingsFile? settings = null, bool tutorial = true )
			=> new( settings ?? Settings(), tutorial: () => tutorial, cursor: new ParkAdvisorMessages.PollCursor { Row = row } );
		static int Never( int response, int sample ) => throw new Exception( $"said {response}" );

		var topic = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ExplainTooltips );
		Assert.AreEqual( 282, topic.Id );
		Assert.AreEqual( new ParkAdvisorMessages.Topic( 282, "TutorialExplainTooltips.Score", 542, 559, Lines: 1, MaxInstances: 1,
			GestureRowLine: -1, Prop: 0, Group: ParkTags.NoTag, Category: ParkAdvisorMessages.Tutorial, Polled: true,
			AgeSection: "TutorialExplainTooltips" ), topic );
		Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( 542 ) );
		Assert.AreEqual( 147, Enumerable.Range( 0, 282 ).Count( ParkAdvisorMessages.IsPolled ), "147 polled rows stand before row 282" );
		Assert.IsTrue( ParkAdvisorMessages.IsPolled( 281 ) && ParkAdvisorMessages.IsPolled( 282 ) );
		Assert.IsFalse( ParkAdvisorMessages.IsPolled( 283 ) );

		Assert.AreEqual( 20, ParkAdvisorMessages.DaysSinceTheFirstSweep( 483 ) );
		Assert.AreEqual( 21, ParkAdvisorMessages.DaysSinceTheFirstSweep( 484 ) );
		Assert.AreEqual( 79, ParkAdvisorMessages.DaysSinceTheFirstSweep( 1843 ) );
		Assert.AreEqual( 80, ParkAdvisorMessages.DaysSinceTheFirstSweep( 1844 ) );
		var messages = On( 282 );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 483 ), "day 20 is not after 20" );
		Assert.AreEqual( 100, messages.ScoreOf( topic, 484 ) );
		Assert.AreEqual( 100, messages.ScoreOf( topic, 1843 ) );
		Assert.AreEqual( 0, messages.ScoreOf( topic, 1844 ), "day 80 is not before 80" );

		// The bounds are its own section's, not the control panel's: day 15 scores the panel's line and not this
		// one, and day 70 this one and not the panel's.
		var panel = ParkAdvisorMessages.ParkTopics.Single( one => one.Id == ParkAdvisorMessages.ControlPanel );
		Assert.AreEqual( (110, 0), (messages.ScoreOf( panel, 350 ), messages.ScoreOf( topic, 350 )) );
		Assert.AreEqual( (0, 100), (messages.ScoreOf( panel, 1620 ), messages.ScoreOf( topic, 1620 )) );
		var keys = Settings();
		Set( keys, "TutorialExplainTooltips.Score", 77 );
		Set( keys, "TutorialExplainTooltips.AfterTime", 40 );
		Set( keys, "TutorialExplainTooltips.CutoffTime", 42 );
		var own = On( 282, keys );
		Assert.AreEqual( (0, 77, 77, 0), (own.ScoreOf( topic, 944 ), own.ScoreOf( topic, 945 ), own.ScoreOf( topic, 967 ), own.ScoreOf( topic, 968 )), "days 40, 41, 41 and 42" );
		Assert.AreEqual( 110, own.ScoreOf( panel, 945 ) );

		// On its row's sweep it is posted and said on that sweep, about no thing, with no tag.
		var said = new List<(int, int)>();
		var groups = new List<(int, int)>();
		var before = On( 281 );
		before.Tick( 760, 0, Never );
		Assert.AreEqual( 0, before.PendingCount, "row 281 is polled and not built" );
		before.Tick( 761, 250, ( response, sample ) => { said.Add( (response, sample) ); return 7600; }, ( group, subject ) => groups.Add( (group, subject) ) );
		CollectionAssert.AreEqual( new[] { (542, 559) }, said );
		CollectionAssert.AreEqual( new[] { (ParkTags.NoTag, 0) }, groups );
		Assert.AreEqual( 283, before.PollRow );
		Assert.AreEqual( "post", before.PollAnswer( ParkAdvisorMessages.ExplainTooltips ) );
		Assert.AreEqual( new ParkWorld.AdvisorHistory( 761, 0, true, 0 ), before.Written()[282] );
		Assert.AreEqual( 282, before.WrittenAction( 1000, 0 ).Playing );
		StringAssert.Contains( before.Census(), "message=282/score=100/line=0/lastTick=761/said=1/slaps=0" );

		// The poll's answers: too young, the file's said record, the option off, and a said record past the last day.
		var young = On( 282 );
		young.Tick( 483, 0, Never );
		Assert.AreEqual( "no-score", young.PollAnswer( ParkAdvisorMessages.ExplainTooltips ) );
		Assert.AreEqual( 0, young.PendingCount );
		var shipped = On( 277 );
		shipped.Restore( id => id == ParkAdvisorMessages.ExplainTooltips ? new ParkWorld.AdvisorHistory( 605, 0, true, 0 ) : null );
		for ( var tick = 756; tick <= 760; ++tick )
			shipped.Tick( tick, 0, Never );
		Assert.AreEqual( "said-once", shipped.PollAnswer( ParkAdvisorMessages.ExplainTooltips ), "the shipped park's fifth sweep" );
		Assert.IsNull( shipped.PollAnswer( ParkAdvisorMessages.ControlPanel ) );
		Assert.AreEqual( 0, shipped.PendingCount );
		var old = On( 282 );
		old.Restore( id => id == ParkAdvisorMessages.ExplainTooltips ? new ParkWorld.AdvisorHistory( 605, 0, true, 0 ) : null );
		old.Tick( 1845, 0, Never );
		Assert.AreEqual( "no-score", old.PollAnswer( ParkAdvisorMessages.ExplainTooltips ), "the score is asked before the gate" );
		var off = On( 282, tutorial: false );
		off.Tick( 760, 0, Never );
		Assert.AreEqual( "tutorial-off", off.PollAnswer( ParkAdvisorMessages.ExplainTooltips ) );
		Assert.AreEqual( 0, off.PendingCount );
		var low = Settings();
		Set( low, "TutorialExplainTooltips.Score", 25 );
		var under = On( 282, low );
		under.Tick( 760, 0, Never );
		Assert.AreEqual( "under-threshold", under.PollAnswer( ParkAdvisorMessages.ExplainTooltips ) );

		// A park made with no file, the cursor at nought and the clock from nought: its turns are sweeps 148, 303,
		// 458 and 613, and the last is the first past day 20.
		var fresh = On( 0 );
		var heard = new List<(int Tick, int Response)>();
		for ( var tick = 1; tick <= 700; ++tick )
			fresh.Tick( tick, tick * 250L, ( response, _ ) => { heard.Add( (tick, response) ); return 2000; } );
		CollectionAssert.AreEqual( new[] { (265, 404), (613, 542) }, heard );
	}
}
