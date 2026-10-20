using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenTPW.Tests;

/// <summary>
/// The end of research (<c>docs/exe/park-engine.md</c>, "The lab at work"): the last project's finish told once,
/// the player's park record marked all researched and kept in <c>gms.dat</c>, and advisor row 167 at 10000, which
/// follows in Instant Action always and in Full Simulation only once every park of the player's is marked.
/// </summary>
[TestClass]
public class ParkResearchEndTests
{
	[TestInitialize]
	public void Setup() => Log ??= new();

	private static PlayerFile FourParks( bool instantAction )
	{
		var file = new PlayerFile { InstantAction = instantAction };

		foreach ( var theme in new[] { "fantasy", "hallow", "jungle", "space" } )
			file.Park( theme );

		return file;
	}

	private static bool[] Marks( PlayerFile file ) => [.. file.Parks.Select( park => park.AllResearchCompleted )];

	/// <summary>
	/// The lab's end test runs after its pick (<c>0x00503e15</c>): the finish that leaves every project done tells the
	/// end once, with its tick, after the advisor and the analyser heard the finish and with the kind's current project
	/// already none; a finish that leaves one open tells nothing.
	/// </summary>
	/// <remarks><b>Mutations:</b> the end told on every finish; told before the pick; never told; told with no tick.</remarks>
	[TestMethod]
	public void TheLastFinishTellsTheEndOnceAfterThePick()
	{
		ParkWorld.ResearchProject[] projects = [new( 1150, 0, 1, 0, 600f, 576f ), new( 1414, 3, 1, 0, 250f, 249f )];
		var lab = new ParkResearch( [], new ParkItemCatalogue( "none", new BaseFileSystem( Path.GetTempPath() ) ), projects,
			[new( 100, 0, 0, 1 ), new( 0, 0, -1, 1 ), new( 0, 0, -1, 1 ), new( 100, 0, 1, 1 ), new( 0, 0, -1, 1 )], workLoad: 85 );
		var heard = new List<string>();
		lab.Researched = ( item, _, tick ) => heard.Add( $"finished {item} on {tick}" );
		lab.AllDone = tick => heard.Add( $"end on {tick}, kind 0 current {lab.Kinds[0].CurrentProject}, kind 3 current {lab.Kinds[3].CurrentProject}" );

		// 100 / 200 x 85 x 12 x 0.01 = 5.1 a kind, so the stone head finishes on the first; then the karts alone take 10.2,
		// 581.1, 591.3 and 601.5 on the third.
		var told = Enumerable.Range( 0, 5 ).SelectMany( handOut => lab.SpendPoints( 12, 760 + handOut * 20 ) ).ToArray();

		Assert.AreEqual( 2, told.Length );
		Assert.AreEqual( (false, true), (told[0].AllDone, told[1].AllDone) );
		CollectionAssert.AreEqual( new[] { "finished 1414 on 760", "finished 1150 on 800", "end on 800, kind 0 current -1, kind 3 current -1" }, heard );
	}

	/// <summary>
	/// Instant Action marks the current park's record alone and the rest follows; Full Simulation marks it and
	/// follows only once every record the player holds is marked; nobody playing marks nothing and follows.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> every record marked; Full Simulation always followed, or never; any record marked enough; the
	/// game type's test turned round; the record of another theme marked.
	/// </remarks>
	[TestMethod]
	public void ThePlayersRecordIsMarkedAndTheGameTypeSaysWhatFollows()
	{
		var instant = FourParks( instantAction: true );
		Assert.IsTrue( ParkResearch.MarkAllResearched( instant, "jungle", instantAction: true ) );
		CollectionAssert.AreEqual( new[] { false, false, true, false }, Marks( instant ) );

		var full = FourParks( instantAction: false );
		Assert.IsFalse( ParkResearch.MarkAllResearched( full, "jungle", instantAction: false ), "three parks are not" );
		CollectionAssert.AreEqual( new[] { false, false, true, false }, Marks( full ) );
		Assert.IsFalse( ParkResearch.MarkAllResearched( full, "space", instantAction: false ), "two are not" );
		full.Park( "fantasy" ).AllResearchCompleted = true;
		Assert.IsTrue( ParkResearch.MarkAllResearched( full, "hallow", instantAction: false ), "the last one marked" );
		CollectionAssert.AreEqual( new[] { true, true, true, true }, Marks( full ) );

		var empty = new PlayerFile();
		Assert.IsTrue( ParkResearch.MarkAllResearched( empty, "jungle", instantAction: false ), "a record made for the park, the only one" );
		Assert.AreEqual( ("jungle", true), (empty.Parks.Single().Name, empty.Parks.Single().AllResearchCompleted) );

		Assert.IsTrue( ParkResearch.MarkAllResearched( null, "jungle", instantAction: true ) );
	}

	/// <summary>
	/// The mark is the theme record's last byte in <c>gms.dat</c> (<c>0x005b1163</c>): a new player's 68 bytes with one
	/// record of "jungle" are 238, the record's <c>mNameChanged</c> at 193 and <c>mAllResearchCompleted</c> at 194, and
	/// it reads back.
	/// </summary>
	/// <remarks><b>Mutations:</b> the two bytes written the other way round; the mark not written.</remarks>
	[TestMethod]
	public void TheMarkIsKeptAsTheParkRecordsLastByte()
	{
		var file = new PlayerFile();
		ParkResearch.MarkAllResearched( file, "jungle", instantAction: true );

		using var written = new MemoryStream();
		file.Write( written );
		var bytes = written.ToArray();

		Assert.AreEqual( 238, bytes.Length );
		Assert.AreEqual( (0, 1), (bytes[193], bytes[194]) );
		Assert.IsTrue( PlayerFile.Read( new MemoryStream( bytes ) ).Park( "jungle" ).AllResearchCompleted );
	}

	/// <summary>
	/// Row 167 (message 0xa7, 0x0059f9a0) is posted about no thing at the constant 10000 in either game, read from no
	/// key, and said before a finished ride's line at 150: response 380, sample 452, on the theme's own advisor, its
	/// group 0x17f, which leaves no tag.
	/// </summary>
	/// <remarks><b>Mutations:</b> a key's score (nought, so refused); Full Simulation's alone; the response or sample off by
	/// one; the costume's prop left off (slot 0).</remarks>
	[TestMethod]
	public void RowOneSixtySevenIsSaidFirstAtTenThousandInEitherGame()
	{
		var settings = new SettingsFile( new MemoryStream( Encoding.ASCII.GetBytes( "GeneralAdvisor.MinScoreForConsideration 25\n"
			+ "MessageGroups[0].MinTimeSameMessage 120\nMessageGroups[0].SayOnlyOnce 0\nMessageGroups[0].DiscardAfterSlaps 0\n"
			+ "ResearchRideResearched.Score 150\n" ) ) );

		foreach ( var instantAction in new[] { true, false } )
		{
			var messages = new ParkAdvisorMessages( settings, instantAction: () => instantAction );
			Assert.IsTrue( messages.Post( ParkAdvisorMessages.RideResearched, 800, secondary: 1150 ) );
			Assert.IsTrue( messages.Post( ParkAdvisorMessages.AllResearched, 800 ) );

			var slot = messages.WrittenSlots( _ => -1 ).Where( written => written is { Valid: true } ).Select( written => written!.Value )
				.Single( written => written.Message == 0xa7 );
			Assert.AreEqual( (0, 10000), (slot.Thing, slot.Score), $"instant action {instantAction}" );

			var said = new List<(int, int)>();
			var heard = new List<int>();
			messages.Tick( 800, 0, ( response, sample ) => { said.Add( (response, sample) ); return 2500; }, ( group, _, _ ) => heard.Add( group ) );
			CollectionAssert.AreEqual( new[] { (380, 452) }, said, $"instant action {instantAction}" );
			CollectionAssert.AreEqual( new[] { ParkTags.NoTag }, heard, "its group is the tag table's none" );
		}

		Assert.AreEqual( Advisor.ThemeSlot, ParkAdvisorMessages.SlotOf( 380 ) );
	}
}
