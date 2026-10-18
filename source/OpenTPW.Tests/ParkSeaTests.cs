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
}
