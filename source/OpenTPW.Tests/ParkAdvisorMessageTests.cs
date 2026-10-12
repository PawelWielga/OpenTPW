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
			+ "RidesClosed.AtLeastThisManyRides 3\nRidesClosed.MoreThanThisPercentClosed 50\nRidesClosed.Score 80\nQueueUnconnected.Score 87\n" ) ) );

	private static void Set( SettingsFile settings, string key, int value )
	{
		var index = settings.Entries.FindIndex( pair => pair.Key == key );
		settings.Entries[index] = new SettingsPair( key, value.ToString() );
	}

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
			[1] = new( 0, 0, 189, 1000, false, -1, false, false ),
			[2] = new( 0, 0, ParkAdvisorMessages.Opened, 20, true, -1, false, false ),
			[3] = new( 0, 0, 189, 1000, true, -1, false, false ),
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
		Assert.AreEqual( 0, messages.RestoreSlots( _ => new ParkWorld.AdvisorSlot( 0, 0, 189, 1, true, -1, false, false ) ) );
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
		Assert.AreEqual( 19, written.Count );
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
		Assert.AreEqual( unbuilt + 144, Counted( "PARK_ADVISOR_POLLED_ROW_NOT_BUILT" ) );
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
		Assert.AreEqual( 11, ParkAdvisorMessages.ParkTopics.Count( one => one.Polled ) );
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
