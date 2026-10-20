namespace OpenTPW;

/// <summary>
/// A ride's view with <c>UsageInfo.ConstrainCamera</c> set, the one the End feature puts the camera in (<c>FUN_0042a560</c>
/// through <c>FUN_0042ab20( 4, 1, model, 0, 0, 1 )</c>; <c>gui_CameraFlags</c> <c>0x4</c> with <c>0x800</c>): the eye on the
/// viewed model's own view node, id 1 under <c>0x1000</c> (<see cref="RideNodes.FindView"/>), with no look offset
/// (<c>docs/exe/park-engine.md</c>, "The End feature's view"). Nothing here turns it: the offsets are nought under
/// <c>0x800</c> and nothing forwards the pointer. A general ride view, the "Ride it!" one, is not built.
/// </summary>
/// <remarks>
/// <para>
/// Each frame the camera update (<c>FUN_0042b1c0</c>, <c>0x0042ca13</c>..<c>0x0042cb7d</c>) takes the record's turn, its
/// three rows of three, mixed with the last frame's record matrix's, <c>(1 - w) x now + w x last</c> element by element,
/// with <c>w = (1000 - t) x 0.0005</c>, <c>t</c> the milliseconds since the last update held to 100 past 200 and to 1 at
/// nought or below, and copies the eye from the record's own translation (<c>0x0042cb83</c>); it looks along the mixed
/// third row.
/// <b>Ours takes <c>t</c> from <see cref="Time.Delta"/></b> where the original reads its own clock
/// (<c>FUN_00402d80</c>), and the first frame mixes the matrix with itself, where the original's last matrix is whatever
/// the view last stored. The End's view node is still in all three of its clips, so the two give the same eye.
/// </para>
/// <para>
/// The view node's matrix is in the model's axes (y up) and the park's are z up, the same swap the model's meshes are
/// built with.
/// </para>
/// </remarks>
public sealed class ParkRideViewCameraMode : CameraMode
{
	/// <summary>
	/// The viewed thing's view node now, in the model's axes, or null when nothing is viewed. Set before
	/// <see cref="Camera.SetCameraMode{T}"/> by whoever puts the camera here (<see cref="ParkEndFeature"/>).
	/// </summary>
	internal static Func<System.Numerics.Matrix4x4?>? Viewed { get; set; }

	/// <summary>Whether the camera is in a ride's view, <c>gui_CameraFlags &amp; 0x4</c>.</summary>
	public static bool Active { get; private set; }

	/// <summary>The last frame's record matrix, unmixed (<c>[0x00790948]</c>).</summary>
	private static System.Numerics.Matrix4x4? _last;

	public ParkRideViewCameraMode()
	{
		FieldOfView = ParkOrbitCameraMode.Lens;
		_last = null;
		Active = true;
		Place( 0f );
	}

	/// <summary>Leaves the view: the next mode is set by whoever leaves it.</summary>
	internal static void Leave()
	{
		Active = false;
		Viewed = null;
		_last = null;
	}

	public override void Update()
	{
		Place( Time.Delta * 1000f );
	}

	private void Place( float milliseconds )
	{
		if ( Viewed?.Invoke() is not { } now )
			return;

		var mixed = Mix( now, _last ?? now, milliseconds );
		_last = now;

		Position = new Vector3( mixed.M41, mixed.M43, mixed.M42 );
		Rotation = Rotation.LookAt( new Vector3( mixed.M31, mixed.M33, mixed.M32 ) );
	}

	/// <summary>The weight of the last frame's matrix after <paramref name="milliseconds"/>, <c>(1000 - t) x 0.0005</c>.</summary>
	internal static float LastWeight( float milliseconds )
	{
		var t = milliseconds > 200f ? 100f : milliseconds <= 0f ? 1f : milliseconds;

		return (1000f - t) * 0.0005f;
	}

	/// <summary>
	/// The record's matrix <paramref name="now"/> with its turn mixed with <paramref name="last"/>'s, element by element;
	/// the rest, the translation with it, is <paramref name="now"/>'s.
	/// </summary>
	internal static System.Numerics.Matrix4x4 Mix( System.Numerics.Matrix4x4 now, System.Numerics.Matrix4x4 last, float milliseconds )
	{
		var w = LastWeight( milliseconds );
		var keep = 1f - w;

		var mixed = now;
		mixed.M11 = keep * now.M11 + w * last.M11; mixed.M12 = keep * now.M12 + w * last.M12; mixed.M13 = keep * now.M13 + w * last.M13;
		mixed.M21 = keep * now.M21 + w * last.M21; mixed.M22 = keep * now.M22 + w * last.M22; mixed.M23 = keep * now.M23 + w * last.M23;
		mixed.M31 = keep * now.M31 + w * last.M31; mixed.M32 = keep * now.M32 + w * last.M32; mixed.M33 = keep * now.M33 + w * last.M33;

		return mixed;
	}
}
