using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OpenTPW.Tests;

/// <summary>The sample draw of <c>FUN_006bc680</c> (<c>docs/exe/audio.md</c>, "The sample draw"), pinned without the game.</summary>
[TestClass]
public class SoundSampleDrawTests
{
	private static List<SoundCategoryFile.Sample> List( params int[] odds )
	{
		var list = new List<SoundCategoryFile.Sample>();

		for ( int i = 0; i < odds.Length; ++i )
			list.Add( new SoundCategoryFile.Sample( 0, i, odds[i], TimeSpan.Zero ) );

		return list;
	}

	[TestMethod]
	public void TheFirstSampleWhoseOddsReachTheRollWins()
	{
		var lobby = List( 21845, 43690, 65535 );

		Assert.AreEqual( 0, SoundCategory.Draw( lobby, 21845 ) );
		Assert.AreEqual( 1, SoundCategory.Draw( lobby, 21846 ) );
		Assert.AreEqual( 1, SoundCategory.Draw( lobby, 43690 ) );
		Assert.AreEqual( 2, SoundCategory.Draw( lobby, 65535 ) );
	}

	[TestMethod]
	public void ARollAboveTheLastOddsGivesNoSample()
	{
		var short65516 = List( 30000, 65516 );

		Assert.AreEqual( 1, SoundCategory.Draw( short65516, 65516 ) );
		Assert.AreEqual( -1, SoundCategory.Draw( short65516, 65517 ) );
		Assert.AreEqual( -1, SoundCategory.Draw( short65516, 65535 ) );
	}

	[TestMethod]
	public void AListOfOneIsGivenWithNoDraw()
	{
		Assert.AreEqual( 0, SoundCategory.Draw( List( 100 ), 65535 ) );
		Assert.AreEqual( -1, SoundCategory.Draw( List(), 0 ) );
	}

	[TestMethod]
	public void AvoidRepeatMovesOnOnlyPastTwoSamples()
	{
		var three = List( 21845, 43690, 65535 );

		Assert.AreEqual( 1, SoundCategory.Draw( three, 100, last: 0 ), "a repeat of 0 moves on to 1" );
		Assert.AreEqual( 0, SoundCategory.Draw( three, 65535, last: 2 ), "a repeat of the last wraps to 0" );
		Assert.AreEqual( 0, SoundCategory.Draw( three, 100, last: 2 ), "a pick that is not the last stays" );
		Assert.AreEqual( 0, SoundCategory.Draw( List( 30000, 65535 ), 100, last: 0 ), "two samples are left alone" );
		Assert.AreEqual( 0, SoundCategory.Draw( three, 100 ), "no step where the caller did not ask" );
	}

	[TestMethod]
	public void NoSampleStaysNoSampleUnderAvoidRepeat()
	{
		Assert.AreEqual( -1, SoundCategory.Draw( List( 100, 200, 65516 ), 65535, last: 2 ) );
	}

	[TestMethod]
	public void TheRollRunsZeroToSixtyFiveThousandFiveHundredThirtyFive()
	{
		// Random.Next( n ) stops at n - 1; the original's roll reaches 65,535, and so reaches a last odds of 65,535.
		Assert.AreEqual( 65536, SoundCategory.RollRange );
	}
}
