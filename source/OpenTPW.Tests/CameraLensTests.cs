using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace OpenTPW.Tests;

/// <summary>
/// The lens: every view hands the projection an angle, and the original's screen scale diag( 1.5, -2 ) narrows the picture
/// to <c>2 atan( tan(a/2) / 2 )</c> (<c>docs/exe/park-engine.md</c>, "The lens", Q260). Only the lobby's file and its isle need
/// the game.
/// </summary>
[TestClass]
public class CameraLensTests
{
	/// <summary>The park's handed 90 draws 53.13 high, the lobby's ISLANDFOV 100 draws 61.58 (Q260, measured live).</summary>
	[TestMethod]
	public void TheScreenScaleHalvesTheTangent()
	{
		Assert.AreEqual( 53.1301f, Camera.DrawnAngle( 90f ), 1e-3f, "the park" );
		Assert.AreEqual( 61.5795f, Camera.DrawnAngle( 100f ), 1e-3f, "the lobby" );
	}

	/// <summary>
	/// At 4:3 the park's projection is the original's draw matrix: x/w 1.5 and y/w 2 a unit of tangent off the eye's axis
	/// (<c>[0x0087b088]</c>, Q435), and the pointer's ray reaches 0.667 across and 0.5 down a unit ahead, its picking's own
	/// half-tangents (<c>FUN_0045bf90</c>).
	/// </summary>
	[TestMethod]
	public void TheParkDrawsTheOriginalsMatrixAt4By3()
	{
		var lens = Camera.Lens( ParkOrbitCameraMode.Lens, 4f / 3f );

		Assert.AreEqual( 1.5f, lens.Projection.M11, 1e-4f, "x/w per unit of tangent" );
		Assert.AreEqual( 2f, lens.Projection.M22, 1e-4f, "y/w per unit of tangent" );
		Assert.AreEqual( 0.6667f, lens.Across, 1e-4f, "across, a unit ahead" );
		Assert.AreEqual( 0.6667f, lens.ReferenceAcross, 1e-4f, "and at 4:3 itself" );
	}

	/// <summary>A wider window keeps the picture's height and sees further to the sides: at 16:9, 0.5 x 16/9 across.</summary>
	[TestMethod]
	public void AWiderWindowKeepsTheHeight()
	{
		var lens = Camera.Lens( ParkOrbitCameraMode.Lens, 16f / 9f );

		Assert.AreEqual( 2f, lens.Projection.M22, 1e-4f, "the same height" );
		Assert.AreEqual( 0.8889f, lens.Across, 1e-4f, "further across" );
		Assert.AreEqual( 0.6667f, lens.ReferenceAcross, 1e-4f, "against 4:3's" );
	}

	/// <summary>Both park cameras draw 53.13: nothing in a park hands the projection another angle.</summary>
	[TestMethod]
	public void BothParkCamerasDrawTheSameLens()
	{
		Assert.AreEqual( 53.1301f, new ParkOrbitCameraMode().FieldOfView, 1e-3f, "the orbit camera" );
		Assert.AreEqual( 53.1301f, new ParkCamcorderCameraMode().FieldOfView, 1e-3f, "the camcorder's first person" );
	}

	/// <summary>The lobby takes lobby.txt's ISLANDFOV, 100 in the shipped file, through the same law.</summary>
	[TestMethod]
	public void TheLobbyDrawsItsFilesAngle()
	{
		Log ??= new();
		FileSystem = GameData.Required();

		var settings = LobbyCameraMode.ReadSettings();

		Assert.AreEqual( 100f, settings.IslandFov, 1e-4f, "the file's" );
		Assert.AreEqual( 61.5795f, LobbyCameraMode.LensOf( settings ), 1e-3f, "drawn" );
	}

	/// <summary>
	/// The lobby's camera draws its file's angle both ways it runs: flying itself with nobody playing, and orbiting the
	/// island a picked player is on. A stand-in isle (<see cref="LobbyCountedGapsTests.AnIsle"/>) with the jungle's script,
	/// and an ISLANDFOV of 80, so no fixed number passes.
	/// </summary>
	[TestMethod]
	public void TheLobbyCameraDrawsItFlyingAndOrbiting()
	{
		Log ??= new();
		FileSystem = GameData.Required();

		var isle = LobbyCountedGapsTests.AnIsle();
		LobbyCountedGapsTests.SetField( isle, "<Script>k__BackingField", LobbyScript.Read( "jungle" ) );

		var camera = new LobbyCameraMode();
		LobbyCountedGapsTests.SetField( camera, "_islands", new List<LobbyIsland> { isle } );
		LobbyCountedGapsTests.SetField( camera, "_settings", (LobbyCameraMode.CameraSettings?)new LobbyCameraMode.CameraSettings( 70f, 20f, 80f ) );

		try
		{
			LobbyCountedGapsTests.SetCurrentPlayer( null );
			camera.Update();
			Assert.AreEqual( 45.5210f, camera.FieldOfView, 1e-3f, "flying, nobody playing" );

			camera.FieldOfView = 0f;
			LobbyCountedGapsTests.SetCurrentPlayer( new Player( 0, "Test", new PlayerFile() ) );
			camera.Update();
			Assert.AreEqual( 45.5210f, camera.FieldOfView, 1e-3f, "orbiting a picked player's island" );
		}
		finally
		{
			LobbyCountedGapsTests.SetCurrentPlayer( null );
			LobbyCameraMode.ForgetIsland();
		}
	}

	/// <summary>Another lobby.txt's ISLANDFOV is drawn by the same law, and one without it draws the shipped 100.</summary>
	[TestMethod]
	public void AnotherFilesAngleIsDrawnTheSameWay()
	{
		var other = LobbyCameraMode.ReadSettings( new StringReader( "SPINRADIUS(70)\n  ISLANDFOV(80)\nVERTICALOFFSET(20)\n" ) );
		var none = LobbyCameraMode.ReadSettings( new StringReader( "SPINRADIUS(70)\n" ) );

		Assert.AreEqual( 80f, other.IslandFov, 1e-4f, "read" );
		Assert.AreEqual( 45.5210f, LobbyCameraMode.LensOf( other ), 1e-3f, "2 atan( tan 40 / 2 )" );
		Assert.AreEqual( 61.5795f, LobbyCameraMode.LensOf( none ), 1e-3f, "the shipped 100" );
	}
}
