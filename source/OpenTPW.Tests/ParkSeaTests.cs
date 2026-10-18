using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// The harbour's moving sea, <see cref="ParkSea"/> (<c>docs/exe/park-engine.md</c>, "The park's sea"). The expected
/// numbers are worked from the decoded formula with every sine read from the engine's 4,096-entry table, the ten
/// degrees' too: entry 114 is 10.02°, so cos 10° is 0.98475 and sin 10° 0.17398, and exact maths answers s a few
/// hundredths away at these distances (25.704 for the first, 85.886 for the point). Each point asked has a column unlike its row and a phase that is not nought, so
/// a rule with the axes swapped, a sign turned or the phase's half dropped answers otherwise.
/// </summary>
[TestClass]
public class ParkSeaTests
{
	private const float Table = 0.005f;

	[TestMethod]
	public void TheWaveRunsTenDegreesOffTheGridAndHalfThePhaseBack()
	{
		// (2π/3)(20 × 0.98475 − 40 × 0.17398) − 0.5 × 2.
		Assert.AreEqual( 25.6734f, ParkSea.Wave( 20f, 40f, 2f ), 0.001f );
	}

	[TestMethod]
	public void APointHeavesByOnePointSevenTimesTheCosine()
	{
		Assert.AreEqual( 0.91851f, ParkSea.Height( 1f ), Table );
	}

	[TestMethod]
	public void APointsNormalLeansEquallyAlongBothGroundAxesByTheSine()
	{
		var normal = ParkSea.Normal( 0.5f );

		Assert.AreEqual( 0.39682f, normal.X, Table );
		Assert.AreEqual( 0.82769f, normal.Y, Table );
		Assert.AreEqual( 0.39682f, normal.Z, Table );
	}

	[TestMethod]
	public void APointStandsTwentyAColumnAcrossAndTwentyARowAlongFromTheCorner()
	{
		var corner = new Vector3( 380f, -180f, -5f );

		// Column 3, row 5, phase 3: s = (2π/3)(60 × 0.98475 − 100 × 0.17398) − 1.5 = 85.808, 1.7 cos s = −0.940.
		var point = ParkSea.Point( corner, 3, 5, 3f );

		Assert.AreEqual( 440f, point.X, 0.001f );
		Assert.AreEqual( -80f, point.Y, 0.001f );
		Assert.AreEqual( -5.9401f, point.Z, Table );
	}

	[TestMethod]
	public void TheCornerIsTheThemesWaterStartPosWithItsUpTurnedToThisEnginesZ()
	{
		Assert.AreEqual( new Vector3( 380f, -180f, -5f ), ParkSea.CornerOf( new Vector3( 380f, -5f, -180f ) ) );
	}

	[TestMethod]
	public void TwoBlocksLay134QuadsRowByRowEachCountingItsWordsFromNought()
	{
		var quads = ParkSea.Quads().ToArray();

		Assert.AreEqual( 134, quads.Length );

		// The first block, columns 0 to 9 by rows 0 to 10, along a row first.
		Assert.AreEqual( (0, 0, 0), quads[0] );
		Assert.AreEqual( (9, 0, 9), quads[9] );
		Assert.AreEqual( (0, 1, 10), quads[10] );
		Assert.AreEqual( (9, 10, 109), quads[109] );

		// The second, columns 10 to 13 by rows 0 to 5, takes the first 24 words again.
		Assert.AreEqual( (10, 0, 0), quads[110] );
		Assert.AreEqual( (10, 1, 4), quads[114] );
		Assert.AreEqual( (13, 5, 23), quads[133] );
	}

	[TestMethod]
	public void EachQuadFoldsAlongTheLineFromItsSecondCornerToItsThird()
	{
		CollectionAssert.AreEqual( new uint[] { 2, 1, 0, 1, 2, 3, 6, 5, 4, 5, 6, 7 }, ParkSea.Elements( 2 ) );
	}

	[TestMethod]
	public void EveryWordIsOneOfTheThreeTextures()
	{
		var words = ParkSea.DrawWords( new Random( 490 ) );

		Assert.AreEqual( 110, words.Length );
		CollectionAssert.AreEquivalent( new[] { 0, 1, 2 }, words.Distinct().ToArray() );
	}

	[TestMethod]
	public void ThePhaseGainsTwelveASecondAndNothingWhilePaused()
	{
		// 0.4 a frame at 30 frames a second; the build squares' 0.1 would give 2.5.
		Assert.AreEqual( 7f, ParkSea.Advance( 1f, 0.5f, paused: false ), 0.0001f );
		Assert.AreEqual( 1f, ParkSea.Advance( 1f, 0.5f, paused: true ), 0.0001f );
	}

	[TestMethod]
	public void OnlyTextureQualityNoughtAndOneTakeTheSmallCopies()
	{
		Assert.IsTrue( ParkSea.SmallTextures( 0 ) );
		Assert.IsTrue( ParkSea.SmallTextures( 1 ) );
		Assert.IsFalse( ParkSea.SmallTextures( 2 ) );
		Assert.IsFalse( ParkSea.SmallTextures( 3 ) );
	}

	[TestMethod]
	public void EveryParkLaysItsSeaAtTheCornerItsThemeFileGives()
	{
		FileSystem = GameData.Required();

		// The global Standard.sam says (375, −5, 0); each theme's own file says otherwise.
		foreach ( var theme in new[] { "jungle", "fantasy", "hallow", "space" } )
			Assert.AreEqual( new Vector3( 380f, -5f, -180f ), new ParkBalance( theme ).Vector( "WaterStartPos", new Vector3( -1f ) ), theme );
	}

	[TestMethod]
	public void LowDetailTakesTheSmallTexturesAndMediumTheFullOnes()
	{
		FileSystem = GameData.Required();

		Assert.IsTrue( ParkSea.SmallTextures( int.Parse( new SettingsFile( "/low.sam" )["GraphicalOptions.TEXTUREQUALITY"]! ) ) );
		Assert.IsFalse( ParkSea.SmallTextures( int.Parse( new SettingsFile( "/med.sam" )["GraphicalOptions.TEXTUREQUALITY"]! ) ) );

		foreach ( var folder in new[] { "textures", "stexture" } )
			foreach ( var name in ParkSea.TextureNames )
				Assert.IsTrue( FileSystem.FileExists( $"generic/dynamic/{folder}/{name}.wct" ), $"{folder}/{name}" );
	}

	private static readonly Vector3 Corner = new( 380f, -180f, -5f );

	/// <summary>A sea without its model: every word 1 but the first block's 42nd, which is 2 and the first, which is 0.</summary>
	private static ParkSea Sea()
	{
		var words = Enumerable.Repeat( 1, ParkSea.Words ).ToArray();
		words[0] = 0;
		words[42] = 2;
		return new ParkSea( Corner, "textures", words );
	}

	[TestMethod]
	public void EachQuadIsLaidFourCornersWithTheirUvTextureAndFlags()
	{
		var sea = Sea();

		try
		{
			GameClock.Update( paused: true, GameClock.ParkCatchUp );
			sea.SetPhase( 3f );
			sea.Update();

			// Quad 42 is column 2, row 4 of the first block (word 4 × 10 + 2): [c][r], [c+1][r], [c][r+1], [c+1][r+1].
			// Phase 3, held by the paused clock, so the points are worked from the table at that phase.
			var quad = sea.Vertices.Skip( 42 * 4 ).Take( 4 ).ToArray();
			var expected = new[]
			{
				(new Vector3( 420f, -100f, -5.0183f ), new Vector2( 0f, 0f )),
				(new Vector3( 440f, -100f, -4.3087f ), new Vector2( 1f, 0f )),
				(new Vector3( 420f, -80f, -3.5750f ), new Vector2( 0f, 1f )),
				(new Vector3( 440f, -80f, -5.9401f ), new Vector2( 1f, 1f ))
			};

			for ( var corner = 0; corner < 4; ++corner )
			{
				var (position, uv) = expected[corner];
				Assert.AreEqual( position.X, quad[corner].Position.X, 0.001f, $"corner {corner} x" );
				Assert.AreEqual( position.Y, quad[corner].Position.Y, 0.001f, $"corner {corner} y" );
				Assert.AreEqual( position.Z, quad[corner].Position.Z, Table, $"corner {corner} z" );
				Assert.AreEqual( uv, quad[corner].TexCoords, $"corner {corner} uv" );
				Assert.AreEqual( 2, quad[corner].TexIndex, $"corner {corner} wears the quad's word" );
				Assert.AreEqual( 0x2u | LobbyModel.ClampFlag, quad[corner].MatFlags, $"corner {corner} flags" );
			}

			// The corner [c+1][r+1] at phase 9: s = 85.808 less 3, the sine 0.9027 along both ground axes.
			sea.SetPhase( 9f );
			sea.Update();

			var normal = sea.Vertices[(42 * 4) + 3].Normal;
			Assert.AreEqual( 0.5569f, normal.X, Table );
			Assert.AreEqual( 0.6169f, normal.Y, Table );
			Assert.AreEqual( 0.5569f, normal.Z, Table );
			Assert.AreEqual( -4.2684f, sea.Vertices[(42 * 4) + 3].Position.Z, Table, "laid at the phase now" );

			// The second block counts its words from nought again: its first quad, column 10, wears word 0.
			Assert.AreEqual( 580f, sea.Vertices[110 * 4].Position.X, 0.001f );
			Assert.AreEqual( 0, sea.Vertices[110 * 4].TexIndex );
		}
		finally
		{
			GameClock.Rebase();
			sea.Delete();
			Entity.ApplyDeletions();
		}
	}

	[TestMethod]
	public void AFrameAdvancesThePhaseByTheFramesTimeAndAPausedClockHoldsIt()
	{
		var sea = Sea();
		var delta = Time.Delta;

		try
		{
			Time.Delta = 0.5f;

			GameClock.Update( paused: false, GameClock.ParkCatchUp );
			sea.SetPhase( 3f );
			sea.Update();
			Assert.AreEqual( 9f, sea.Phase, 0.0001f, "12 a second for half a second" );
			Assert.AreEqual( -4.2684f, sea.Vertices[(42 * 4) + 3].Position.Z, Table, "and laid at the new phase" );

			GameClock.Update( paused: true, GameClock.ParkCatchUp );
			sea.Update();
			Assert.AreEqual( 9f, sea.Phase, 0.0001f, "held while the clock is" );
		}
		finally
		{
			Time.Delta = delta;
			GameClock.Rebase();
			sea.Delete();
			Entity.ApplyDeletions();
		}
	}

	[TestMethod]
	public void TheTextureQualityIsTheDetailFilesKeyAtEachGraphicsQuality()
	{
		FileSystem = GameData.Required();
		var quality = GameOptions.Current.GraphicsQuality;

		try
		{
			// low.sam, med.sam and high.sam say 1, 2 and 3; the options start at 1, medium.
			foreach ( var (graphics, expected) in new[] { (0, 1), (1, 2), (2, 3) })
			{
				GameOptions.Current.GraphicsQuality = graphics;
				Assert.AreEqual( expected, Level.TextureQuality(), $"graphics quality {graphics}" );
			}
		}
		finally
		{
			GameOptions.Current.GraphicsQuality = quality;
		}
	}
}
