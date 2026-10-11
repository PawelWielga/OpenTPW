using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OpenTPW.Tests;

/// <summary>Real arrivals and the decoded constructor's draw order, not a second implementation of its formulas.</summary>
[TestClass]
public class ParkGuestArrivalTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame()
	{
		FileSystem = data = GameData.Required();
	}

	[TestCleanup]
	public void LetThePeopleGo()
	{
		TestRun.DeleteEvery<ParkPeople>();
		ParkState.ForgetCurrent();
	}

	private sealed class Draws( params int[] values ) : Random
	{
		public int Taken { get; private set; }
		public override int Next()
		{
			Assert.IsTrue( Taken < values.Length, "an unexpected constructor draw" );
			return values[Taken++];
		}
		public override int Next( int maxValue ) => throw new AssertFailedException( "use the raw draw before modulo" );
	}

	private ParkPeople Open( Draws draws )
	{
		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var world = new ParkWorld( new SaveReader( stream ).ReadFile() );
		return new ParkPeople( world, new ParkBalance( "jungle", easyMode: true ),
			state: new ParkState( world ), random: draws, banks: new ParkSpriteBanks( 6, 3, 4 ) );
	}

	[TestMethod]
	public void TheRealConstructorInitializesEveryDecodedValueInDrawOrder()
	{
		// Speed, exit, kind, cash, thirst, hunger, toilet, discarded, prankery.
		var draws = new Draws( 4, 119, 3, 30, 49, 17, 29, 99, 4 );
		var people = Open( draws );
		var id = people.Admit( 42, 5 );
		var guest = people.Guests[id];

		Assert.AreEqual( 9, draws.Taken );
		Assert.AreEqual( Peep.BaseSpeeds[4], guest.BaseSpeed );
		Assert.AreEqual( 3, guest.PersonType );
		Assert.AreEqual( 179, guest.ExitLevel );
		Assert.AreEqual( 805, guest.Cash );
		Assert.AreEqual( 50f, guest.Happiness );
		Assert.AreEqual( 49f, guest.Thirst );
		Assert.AreEqual( 17f, guest.Hunger );
		Assert.AreEqual( 29f, guest.Toilet );
		Assert.AreEqual( 0f, guest.Vomit );
		Assert.AreEqual( 0f, guest.Litter );
		Assert.AreEqual( 100 + (id & 0xffff) % 3, guest.PrankeryIndex );
		Assert.AreEqual( new ParkSpriteBanks( 6, 3, 4 ).ChildOf( id ), guest.SpriteBank );
	}

	[TestMethod]
	public void LowerEndpointsAndPrankeryEqualityUseTheNextGuestsOwnDraws()
	{
		var draws = new Draws( 0, 0, 0, 0, 50, 100, 30, 0, 5,
			1, 1, 4, 15, 1, 2, 3, 99, 0 );
		var people = Open( draws );
		var first = people.Guests[people.Admit( 42, 5 )];
		var second = people.Guests[people.Admit( 42, 5 )];

		Assert.AreEqual( 18, draws.Taken );
		Assert.AreEqual( 60, first.ExitLevel );
		Assert.AreEqual( 255, first.Cash );
		Assert.AreEqual( 0f, first.Thirst );
		Assert.AreEqual( 0f, first.Hunger );
		Assert.AreEqual( 0f, first.Toilet );
		Assert.AreEqual( 0, first.PrankeryIndex, "5 is not below the shipped likelihood 5" );
		Assert.AreEqual( 50f, second.Happiness );
		Assert.AreEqual( 4, second.PersonType );
		Assert.AreEqual( 750, second.Cash );
		Assert.AreEqual( 61, second.ExitLevel );
		Assert.AreEqual( 100 + (second.ThingId & 0xffff) % 3, second.PrankeryIndex );
	}

	[TestMethod]
	public void TheExceptionalSignedReturnKeepsUnsignedNeedsAndSignedMoneyAndExit()
	{
		var draws = new Draws( int.MinValue, int.MinValue, int.MinValue, int.MinValue,
			int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue );
		var people = Open( draws );
		var guest = people.Guests[people.Admit( 42, 5 )];

		Assert.AreEqual( 9, draws.Taken );
		Assert.AreEqual( Peep.BaseSpeeds[3], guest.BaseSpeed );
		Assert.AreEqual( 0, guest.PersonType );
		Assert.AreEqual( 52, guest.ExitLevel );
		Assert.AreEqual( 249, guest.Cash );
		Assert.AreEqual( 48f, guest.Thirst );
		Assert.AreEqual( 48f, guest.Hunger );
		Assert.AreEqual( 8f, guest.Toilet );
		Assert.AreEqual( 0, guest.PrankeryIndex );
	}

	[TestMethod]
	public void AForcedKindStillConsumesItsDrawAndCashTruncates()
	{
		var draws = new Draws( 0, 60, 7, 14, 2, 3, 4, 99, 5 );
		var people = Open( draws );
		var guest = people.Guests[people.Admit( 42, 5, personType: 4 )];

		Assert.AreEqual( 9, draws.Taken );
		Assert.AreEqual( 4, guest.PersonType );
		Assert.AreEqual( 742, guest.Cash, "99 percent of 750 truncates toward zero" );
		Assert.AreEqual( 2f, guest.Thirst );
		Assert.AreEqual( 3f, guest.Hunger );
		Assert.AreEqual( 4f, guest.Toilet );
	}

	/// <summary>
	/// A cell off the 128 x 128 map takes nobody: <see cref="ParkPeople.Admit"/> "answers their thing id, or nought
	/// where the park cannot take one" (its doc comment), and it answers before the constructor's first draw, so the
	/// generator is where it was. One cell past each of the map's four sides; the generator here fails the test on any draw.
	/// </summary>
	[TestMethod]
	public void ACellOffTheMapTakesNobodyAndDrawsNothing()
	{
		var draws = new Draws();
		var people = Open( draws );
		var before = people.Peeps.Count;

		Assert.AreEqual( 0, people.Admit( -1, 5 ), "west of the map" );
		Assert.AreEqual( 0, people.Admit( 128, 5 ), "east of it: the cells are 0 to 127" );
		Assert.AreEqual( 0, people.Admit( 42, -1 ), "north of it" );
		Assert.AreEqual( 0, people.Admit( 42, 128 ), "south of it" );

		Assert.AreEqual( 0, draws.Taken, "nothing is drawn for a guest who is not made" );
		Assert.AreEqual( before, people.Peeps.Count, "and nobody joins the park" );
	}

	/// <summary>
	/// The exit level, its variation, the cash variation and the prankery likelihood are the balance file's, not
	/// constants: the constructor reads them from the globals the keys fill (<c>docs/exe/guest-arrivals.md</c>,
	/// "Construction and arrival": exit <c>ExitLevel + draw % (2 * ExitLevelVar) - ExitLevelVar</c>, cash
	/// <c>(draw % (2*v+1) - v + 100) * StartingCash[kind] / 100</c>, prankery <c>draw % 100 &lt; PrankeryLikelihood</c>).
	///
	/// <para>
	/// <b>Why a balance file of its own.</b> The shipped file's four values (120, 60, 15, 5) are the fallbacks, so the
	/// shipped game cannot tell a read key from a constant. Here the shipped stack is copied and the last layer says
	/// 200, 10, 40 and 50. So: exit 200 + 15 % 20 - 10 = 205; cash for kind 4, who starts with 750,
	/// (70 % 81 - 40 + 100) = 130 percent, 975; and a prankery draw of 20 is below 50, so they are a prankster.
	/// </para>
	/// </summary>
	[TestMethod]
	public void TheExitCashAndPrankeryNumbersAreReadFromTheBalanceFile()
	{
		var root = Directory.CreateTempSubdirectory( "opentpw-arrival-" );

		try
		{
			Directory.CreateDirectory( Path.Combine( root.FullName, "levels", "jungle" ) );

			File.WriteAllBytes( Path.Combine( root.FullName, "levels", "Standard.sam" ), data.ReadAllBytes( "levels/Standard.sam" ) );
			File.WriteAllBytes( Path.Combine( root.FullName, "levels", "jungle", "Standard.sam" ), data.ReadAllBytes( "levels/jungle/Standard.sam" ) );
			File.WriteAllBytes( Path.Combine( root.FullName, "levels", "jungle", "Easy_Standard.sam" ), data.ReadAllBytes( "levels/jungle/Easy_Standard.sam" ) );
			File.AppendAllText( Path.Combine( root.FullName, "levels", "jungle", "Easy_Standard.sam" ),
				"\nPeepInfo.ExitLevel 200\nPeepInfo.ExitLevelVar 10\nPeepInfo.StartingCashVarPc 40\nPeepInfo.PrankeryLikelihood 50\n" );

			FileSystem = new BaseFileSystem( root.FullName );

			var balance = new ParkBalance( "jungle", easyMode: true );

			FileSystem = data;

			Assert.AreEqual( 200, balance.Int( "PeepInfo.ExitLevel" ), "the last layer's own value is the one held" );

			using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
			var world = new ParkWorld( new SaveReader( stream ).ReadFile() );

			// Speed, exit, kind, cash, thirst, hunger, toilet, discarded, prankery.
			var draws = new Draws( 0, 15, 3, 70, 1, 2, 3, 0, 20 );
			var people = new ParkPeople( world, balance,
				state: new ParkState( world ), random: draws, banks: new ParkSpriteBanks( 6, 3, 4 ) );
			var guest = people.Guests[people.Admit( 42, 5, personType: 4 )];

			Assert.AreEqual( 9, draws.Taken );
			Assert.AreEqual( 205, guest.ExitLevel, "200 + 15 % (2 * 10) - 10" );
			Assert.AreEqual( 975, guest.Cash, "70 % 81 - 40 + 100 is 130 percent of 750" );
			Assert.AreEqual( 100 + (guest.ThingId & 0xffff) % 3, guest.PrankeryIndex, "20 is below a likelihood of 50" );
		}
		finally
		{
			FileSystem = data;
			root.Delete( recursive: true );
		}
	}

	/// <summary>
	/// What a new guest's mover, walk and sprite hold before anything sends them anywhere. Made on the gateway's own
	/// cell, which counts as the park's, they are deciding at once and no walk in is planned (<c>FUN_004faec0</c>,
	/// <c>0x004fb1d0</c> on), so each value is the constructor's own:
	/// <list type="bullet">
	/// <item>the mover's constructor (<c>FUN_0050ffe0</c>, which <see cref="ParkPeople.Admit"/> cites) writes 1 to the
	/// navigator's <c>+0x60</c>, the byte the serialiser names <c>path_finished</c> (<c>FUN_0050d150</c>,
	/// <c>0x0050d4c6</c>): on no route, they have finished it;</item>
	/// <item>their walk reads the running park's block stamps, the ground it notices changing
	/// (<see cref="PeepWalk.Ground"/>, <c>0x0050ed79</c>), as every saved guest's does;</item>
	/// <item>their sprite is scheduled as it is made, from nought, as <see cref="ParkPeople.Admit"/> says: due at
	/// 0 + the default interval <c>0x3e</c>, 62.</item>
	/// </list>
	/// </summary>
	[TestMethod]
	public void AGuestMadeOnAParkCellIsOnNoRouteOnThePatchOfGroundAndDueInOneInterval()
	{
		var draws = new Draws( 0, 0, 0, 0, 0, 0, 0, 0, 99 );
		var people = Open( draws );
		var gate = people.Admission!.EntranceA;
		var id = people.Admit( gate.X, gate.Y );
		var guest = people.Guests[id];

		Assert.AreEqual( 9, draws.Taken );
		Assert.AreEqual( PeepState.Deciding, guest.State, "made on a park cell, so no walk in was planned" );
		Assert.IsTrue( guest.Navigator.Finished, "the mover is made with its path finished" );
		Assert.AreSame( people.State, people.WalkFor( id )!.Ground, "the walk reads the running park's block stamps" );
		Assert.AreEqual( 62, people.SpriteFor( id )!.Due, "scheduled from nought: one default interval, 0x3e" );
	}
}
