using System;

namespace OpenTPW.Tests;

/// <summary>
/// A <see cref="Random"/> for a <see cref="SoundCategory"/> whose sample roll (0 to 65,535) is the test's to set.
/// The category's variation pick (<c>Next( count )</c>) stays random, so a test that draws is deterministic in the one
/// thing a game-data test cannot leave to chance: a roll above a list's last odds gives no sample.
/// </summary>
internal sealed class FixedRoll : Random
{
	/// <summary>The roll the next sample draws take.</summary>
	public int Roll;

	public FixedRoll( int roll = 0 ) => Roll = roll;

	public override int Next( int maxValue ) => maxValue == SoundCategory.RollRange ? Roll : base.Next( maxValue );

	/// <summary>Runs <paramref name="body"/> with every category made inside it rolling <paramref name="roll"/>.</summary>
	public static void During( int roll, Action<FixedRoll> body )
	{
		var was = SoundCategory.RandomSource;
		var random = new FixedRoll( roll );

		SoundCategory.RandomSource = () => random;

		try
		{
			body( random );
		}
		finally
		{
			SoundCategory.RandomSource = was;
		}
	}
}

/// <summary>
/// Every category of a test run rolls this unless a test sets its own (<see cref="FixedRoll"/>): the roll stays at or
/// below 65,516, the lowest last odds of the 1,592 sample lists in the shipped maps, so no draw gives "no sample" (a roll
/// above a list's last odds, Q723) and a test of what a sample does is not left to a 20-in-65,536 chance. A test of the
/// no-sample case rolls above the last odds itself.
/// </summary>
internal sealed class NoneFreeRoll : Random
{
	public override int Next( int maxValue ) => maxValue == SoundCategory.RollRange ? base.Next( 65517 ) : base.Next( maxValue );
}
