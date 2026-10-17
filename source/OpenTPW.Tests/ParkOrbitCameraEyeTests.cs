using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace OpenTPW.Tests;

/// <summary>
/// Where the park's orbit camera stands - <see cref="ParkOrbitCameraMode.EyeAt"/> - against the eye read in the running
/// original (its camera matrix <c>0x0087aff0</c>, Q435; <c>docs/exe/park-engine.md</c>, "The park camera"). No game files
/// needed.
/// </summary>
[TestClass]
public class ParkOrbitCameraEyeTests
{
	/// <summary>Zoom drives pitch: 45 degrees pulled in, 65 pushed out, 58.33 at the 110 the original starts at.</summary>
	[TestMethod]
	public void ZoomDrivesThePitch()
	{
		Assert.AreEqual( 45f, ParkOrbitCameraMode.PitchAt( 70f ), 1e-4f, "pulled in" );
		Assert.AreEqual( 65f, ParkOrbitCameraMode.PitchAt( 130f ), 1e-4f, "pushed out" );
		Assert.AreEqual( 58.3333f, ParkOrbitCameraMode.PitchAt( 110f ), 1e-3f, "the original's start" );
	}

	/// <summary>
	/// The original at point (484.442, 259.319), zoom 110, rotation nought, had its eye at (484.44, 93.62, 201.57) in its
	/// own axes, Y up: here (484.44, 201.57, 93.62), Z up.
	/// </summary>
	[TestMethod]
	public void TheEyeIsTheOriginalsAtItsOwnNumbers()
	{
		var eye = ParkOrbitCameraMode.EyeAt( new Vector3( 484.442f, 259.319f, 0f ), 0f, 110f, 0f );

		Assert.AreEqual( 484.442f, eye.X, 0.01f, "straight behind the point" );
		Assert.AreEqual( 201.572f, eye.Y, 0.01f, "57.75 back from it, on the near side" );
		Assert.AreEqual( 93.623f, eye.Z, 0.01f, "93.62 up" );
	}

	/// <summary>
	/// The original turned to 90 and 180 at point (495, 285), zoom 110, stood its eye at (437.253, 93.623, 285.000) and
	/// (495.000, 93.623, 342.747): on the point less (h sin r, h cos r). This yaw swings the other way, so the same eye is
	/// the rotation negated (Q440, <c>lib/bothcam.py</c>).
	/// </summary>
	[TestMethod]
	public void TheOriginalsRotationIsThisYawNegated()
	{
		var point = new Vector3( 495f, 285f, 0f );
		var ninety = ParkOrbitCameraMode.EyeAt( point, -MathF.PI / 2f, 110f, 0f );
		var half = ParkOrbitCameraMode.EyeAt( point, -MathF.PI, 110f, 0f );

		Assert.AreEqual( 437.253f, ninety.X, 0.01f, "at 90, 57.75 less in x than the point" );
		Assert.AreEqual( 285f, ninety.Y, 0.01f, "at 90, level with it" );
		Assert.AreEqual( 495f, half.X, 0.01f, "at 180, level with it" );
		Assert.AreEqual( 342.747f, half.Y, 0.01f, "at 180, 57.75 beyond it" );
	}

	/// <summary>The eased ground under the point raises the eye by as much (the original's <c>0x00790aa4</c>).</summary>
	[TestMethod]
	public void TheGroundUnderThePointRaisesTheEye()
	{
		var flat = ParkOrbitCameraMode.EyeAt( new Vector3( 484.442f, 259.319f, 0f ), 0f, 110f, 0f );
		var raised = ParkOrbitCameraMode.EyeAt( new Vector3( 484.442f, 259.319f, 0f ), 0f, 110f, 12f );

		Assert.AreEqual( flat.Z + 12f, raised.Z, 0.01f, "twelve higher" );
		Assert.AreEqual( flat.Y, raised.Y, 0.01f, "no nearer or further" );
	}
}
