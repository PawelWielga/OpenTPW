using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The park value - <c>docs/exe/hud.md</c>, "The park value": <see cref="ParkValue"/>, the original's
/// <c>FUN_004c7db0</c>, and the analyser's 2nd ring (<c>+0x1f350</c>) it is sampled into at each month's close. Reads
/// Lost Kingdom's save and catalogue, and is skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkValueTests
{
	private BaseFileSystem? data;

	private BaseFileSystem Data() => data ??= FileSystem = GameData.Required();

	[TestCleanup]
	public void LetThePeopleGo() => TestRun.DeleteEvery<ParkPeople>();

	private ParkWorld Park()
	{
		using var stream = new MemoryStream( Data().ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		return new ParkWorld( new SaveReader( stream ).ReadFile() );
	}

	private ParkItemCatalogue Catalogue() => new( "jungle", Data() );

	private const int BellyBounce = 13;

	/// <summary>
	/// <b>The shipped park is valued at its own ring's figure</b>, the original's at its one closed month, 23977: the cells
	/// 19527 (2362 cells of type 0 at 2, 78 path at 3, 4 queue at 5, 8 of type 9 at 6, one of type 10 at 1, 290 track cells
	/// at 50, none outside the park) and the objects 4450 (each at full repair: the fountain 150, three toilets 100, the
	/// staff room 500, two cameras 250, a bin 100, the Drinks Shop 650, the Jungle Spray 1750, the Belly Bounce 500; the
	/// gate and the two fixed items nought).
	/// </summary>
	/// <remarks><b>Mutations:</b> a type's weight; the outside flag not skipped; a deferring track cell read as its own; the parent not taken; a tier past the level or short of it; the repair not cut to a byte.</remarks>
	[TestMethod]
	public void TheShippedParkIsValuedAtItsOwnRingsFigure()
	{
		var world = Park();
		var state = new ParkState( world );

		Assert.AreEqual( 23977, world.MonthParkValue!.Newest, "the file's one closed month, the original's figure" );
		Assert.AreEqual( 4450, state.ObjectsInChainOrder().Sum( thing => ParkValue.Of( thing, Catalogue() ) ) );
		Assert.AreEqual( 23977, ParkValue.Of( state, Catalogue() ) );
	}

	/// <summary>
	/// <b>A cell adds by its type and its track record, and nothing outside the park</b>: (2,2) is type 0 with a track
	/// record of type 12 whose parent is a 25, so 2 + 50; with no parent 2; with the outside flag nought. (3,3) is type 0:
	/// as a path 3, as type 9 6, as a footprint (4) nought. A parent that is itself a 12 is read as it is: (2,2)'s, (1,1),
	/// made a 12 naming a 25, takes (2,2)'s 50 away.
	/// </summary>
	[TestMethod]
	public void ACellAddsByItsTypeAndTrackAndNothingOutsideThePark()
	{
		var state = new ParkState( Park() );
		var catalogue = Catalogue();
		var whole = ParkValue.Of( state, catalogue );
		var track = state.Record( 2, 2 );

		Assert.AreEqual( (0, 12, (ushort)130), (track.Type, track.TrackType, track.TrackParentId) );

		state.SetRecord( 2, 2, track with { TrackParentId = 0 } );
		Assert.AreEqual( whole - 50, ParkValue.Of( state, catalogue ), "a type 12 with no parent adds no track" );

		state.SetRecord( 2, 2, track with { TrackType = 25 } );
		Assert.AreEqual( whole, ParkValue.Of( state, catalogue ), "a 25 of its own" );

		state.SetRecord( 2, 2, track with { Flags = (ushort)(track.Flags | ParkPathBuilding.OutsideThePark) } );
		Assert.AreEqual( whole - 52, ParkValue.Of( state, catalogue ), "outside the park, nothing" );

		state.SetRecord( 2, 2, track );

		var parent = state.Record( 1, 1 );

		Assert.AreEqual( (25, ParkPathBuilding.OutsideThePark), (parent.TrackType, parent.Flags & ParkPathBuilding.OutsideThePark), "(2,2)'s parent, 130, outside" );

		state.SetRecord( 1, 1, parent with { TrackType = 12, TrackParentId = 3 * 128 + 25 + 1 } );
		Assert.AreEqual( whole - 50, ParkValue.Of( state, catalogue ), "a parent that defers again is read as it is, not followed" );

		state.SetRecord( 1, 1, parent );

		var bare = state.Record( 3, 3 );

		foreach ( var (type, adds) in new[] { (1, 3), (2, 1), (3, 5), (9, 6), (10, 1), (4, 0), (7, 0) } )
		{
			state.SetRecord( 3, 3, bare with { Type = type } );
			Assert.AreEqual( whole - 2 + adds, ParkValue.Of( state, catalogue ), $"type {type}" );
		}
	}

	/// <summary>
	/// <b>An object adds every tier up to its own, times its repair cut to a byte, over a hundred</b>: the Belly Bounce at
	/// 98.6 adds 500 x 98 / 100 = 490; upgraded once (400) 882; twice (500) 1372; condemned nought, whatever its tiers.
	/// </summary>
	[TestMethod]
	public void AnObjectAddsItsTiersCostsTimesItsRepair()
	{
		var state = new ParkState( Park() );
		var catalogue = Catalogue();
		var bounce = state.ObjectsInChainOrder().Single( thing => thing.ThingId == BellyBounce ) with { StateOfRepair = 98.6f };

		Assert.AreEqual( 490, ParkValue.Of( bounce, catalogue ) );
		Assert.AreEqual( 882, ParkValue.Of( bounce with { UpgradeLevel = 1 }, catalogue ) );
		Assert.AreEqual( 1372, ParkValue.Of( bounce with { UpgradeLevel = 2 }, catalogue ) );
		Assert.AreEqual( 0, ParkValue.Of( bounce with { UpgradeLevel = 2, State = ParkValue.Condemned }, catalogue ) );
		Assert.AreEqual( 0, ParkValue.Of( bounce, null ), "an item the catalogue does not name adds nought" );
	}

	/// <summary>
	/// <b>The month's close samples the park value into the 2nd ring, after the money rings, and the writer is handed
	/// it</b>: the shipped park's slot 0 [23977], then slot 1 the park value then, its month so far untouched.
	/// </summary>
	[TestMethod]
	public void TheMonthsCloseSamplesTheParkValue()
	{
		var world = Park();
		var state = new ParkState( world );
		var catalogue = Catalogue();
		var people = new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ), null, state );

		Assert.AreEqual( (0, 23977, 0), (state.MonthParkValue.Current, state.MonthParkValue.Newest, state.MonthParkValue.Total), "the file's" );

		state.ReplaceObject( state.ObjectsInChainOrder().Single( thing => thing.ThingId == BellyBounce ) with { UpgradeLevel = 1 } );
		ParkPeople.TurnTheMonth( people, state, catalogue );

		Assert.AreEqual( (1, 23977 + 400, 0), (state.MonthParkValue.Current, state.MonthParkValue.Newest, state.MonthParkValue.Total) );
		Assert.AreEqual( state.MonthCosts.Current, state.MonthParkValue.Current, "on the money rings' slot" );
		Assert.AreSame( state.MonthParkValue, state.MonthRings.ParkValue, "the writer handed it as itself" );
	}
}
