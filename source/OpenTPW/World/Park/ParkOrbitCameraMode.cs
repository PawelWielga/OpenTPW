using NeoVeldrid;
using System.Numerics;

namespace OpenTPW;

/// <summary>
/// The park's own camera, as the original flies it.
///
/// <para>
/// The original keeps this camera as a save module of its own ("SAD_CAMERA", chunk magic <c>KAME</c>,
/// code 0x0042a190-0x0042d130), and its persisted state is six globals: where it is looking, how far
/// round it has been spun, how far out it is zoomed, a flags word, and the look-at point and spin that
/// first person saves to put back. Everything else below is
/// derived from those each frame, which is why this has so few fields.
/// </para>
///
/// <para>
/// Zoom drives pitch, so there is only one control for "how close am I": pulling in also lowers the
/// camera towards the ground, and pushing out raises it towards a plan view.
/// </para>
///
/// <para>
/// The three pieces of state are static so that the debug console can drive the camera without holding
/// the instance, the same way <see cref="LobbyCameraMode"/> exposes its own.
/// </para>
/// </summary>
public sealed class ParkOrbitCameraMode : CameraMode
{
	/// <summary>
	/// A park under this camera is heard from halfway between the eye and the point it looks at, as the
	/// original's is while its camera-mode mask is clear (<c>0x0054f9e4</c>; <c>docs/exe/audio.md</c>, "The
	/// listener, and what a pause does to it").
	/// </summary>
	public override Vector3 Ears => (Position + PointOfInterest) * 0.5f;

	/// <summary>
	/// What the camera looks at, on the ground. The original's default is (475, 175) in the park's own
	/// axes, which is grid cell (47, 17) - the park entrance's <c>EntranceA</c> in <c>Standard.sam</c>, one
	/// cell west of the (48, 17) that <c>MapInfo.FixedItemOrigin</c> names. A cell is 10 units across and
	/// its centre is at <c>grid * 10 + 5</c>, which is exactly where those two numbers come from.
	/// </summary>
	public static Vector3 PointOfInterest { get; set; } = new( 475f, 175f, 0f );

	/// <summary>How far the camera has been spun about the point of interest, in radians.</summary>
	public static float Yaw { get; set; }

	/// <summary>
	/// Distance from the point of interest, and the only "how close" control - <see cref="Pitch"/> is
	/// derived from it. The original starts at 110 and clamps to 70..130.
	/// </summary>
	public static float Zoom { get; set; } = 110f;

	public const float MinZoom = 70f;
	public const float MaxZoom = 130f;

	/// <summary>Pitch pulled all the way in, in degrees.</summary>
	private const float PitchAtMinZoom = 45f;

	/// <summary>Pitch pushed all the way out, in degrees.</summary>
	private const float PitchAtMaxZoom = 65f;

	/// <summary>
	/// The original's own ceiling on pitch. An oddly precise number because it clamps the result of the
	/// interpolation rather than being a design value, and it is kept exactly as it reads there so that
	/// nothing here quietly rounds the camera somewhere the original would not put it. With the range
	/// above it is never actually reached; it will matter if zoom is ever driven outside 70..130.
	/// </summary>
	private const float MaxPitch = 84.2673f;

	/// <summary>
	/// Pitch for the current zoom: 45 degrees pulled in, 65 pushed out, straight line between, then
	/// clamped. Zoom and pitch are one control in the original and they stay one control here.
	/// </summary>
	public static float Pitch => PitchAt( Zoom );

	/// <summary>The pitch <see cref="Pitch"/> gives at a zoom, in degrees.</summary>
	internal static float PitchAt( float zoom )
	{
		var t = ((zoom - MinZoom) / (MaxZoom - MinZoom)).Clamp( 0f, 1f );
		var degrees = PitchAtMinZoom + ((PitchAtMaxZoom - PitchAtMinZoom) * t);

		return degrees.Clamp( 0f, MaxPitch );
	}

	/// <summary>
	/// How fast the eye catches up with the ground beneath it.
	///
	/// <para>
	/// The original eases it with a one-pole filter at alpha 0.5 - half the remaining distance every
	/// tick - and its park loop ticks at a fixed 31ms. That is a half-life of 31ms, so the rate here
	/// is <c>ln(2) / 0.031</c>. Written as a rate rather than as a per-frame fraction because a fixed
	/// fraction settles at a different speed on every machine; see <see cref="Time.SmoothingFactor"/>.
	/// </para>
	/// </summary>
	private const float GroundFollowRate = 22.36f;

	/// <summary>The eased ground height, and whether it has ever been sampled - see <see cref="GroundUnderPoi"/>.</summary>
	private float _groundHeight;

	private bool _groundSampled;

	/// <summary>The ground under the point of interest right now, without touching the easing.</summary>
	/// <remarks>See <c>ParkCamcorderCameraMode.PeekGround</c> - the first un-eased sample belongs to
	/// the first <see cref="Update"/>, not to a constructor.</remarks>
	private static float PeekGround()
		=> ParkGround.Current?.Heightfield?.HeightAtWorld( PointOfInterest.X, PointOfInterest.Y ) ?? 0f;

	/// <summary>
	/// The ground under the point of interest, eased. The original raises the eye by this so that
	/// scrolling over a cliff lifts the camera with the land rather than burying it.
	///
	/// <para>
	/// The first sample is taken outright rather than eased into, so that a park opens with the camera
	/// already at the right height instead of rising into place over the first second. Zero where
	/// there is no ground to ask - which is any scene that is not a park.
	/// </para>
	/// </summary>
	private float GroundUnderPoi()
	{
		var field = ParkGround.Current?.Heightfield;

		if ( field == null )
			return 0f;

		var sample = field.HeightAtWorld( PointOfInterest.X, PointOfInterest.Y );

		if ( !_groundSampled )
		{
			_groundHeight = sample;
			_groundSampled = true;
		}
		else
		{
			_groundHeight = _groundHeight.LerpTo( sample, Time.SmoothingFactor( GroundFollowRate ) );
		}

		return _groundHeight;
	}

	/// <summary>How fast the point of interest scrolls, in world units a second. Provisional - see Update.</summary>
	private const float ScrollSpeed = 200f;

	/// <summary>
	/// The angle the original's park camera hands its projection: pi/2 at its reset (<c>FUN_0042a990</c>,
	/// <c>0x0042aa2f</c>), near 0.1, far 1000.
	/// </summary>
	internal const float HandedAngle = 90f;

	/// <summary>
	/// The picture the park draws, 53.13 degrees top to bottom at 4:3 and 67.38 across: the handed 90
	/// through the original's screen scale (<see cref="Camera.DrawnAngle"/>). The camcorder's first person
	/// draws with the same lens.
	/// </summary>
	/// <remarks>
	/// The far plane stays Camera's own 10000 rather than the original's 1000. Nothing here depends on it -
	/// it costs only depth precision - so it is left alone rather than reaching into shared code to suit one
	/// camera.
	/// </remarks>
	public static float Lens => Camera.DrawnAngle( HandedAngle );

	public ParkOrbitCameraMode()
	{
		FieldOfView = Lens;

		// Already over the point of interest before anything reads the camera - see Place. The ground
		// is PEEKED rather than sampled, for the reason ParkCamcorderCameraMode.PeekGround gives: the
		// one un-eased sample belongs to the first Update, when the statics have settled.
		Place( PeekGround() );
	}

	public override void Update()
	{
		// Down to the ground and back, the way the original's 'C' does - see ParkCamcorderCameraMode.
		// Handled here rather than somewhere central because a camera mode is what knows when it should
		// give way, which is how LobbyCameraMode takes its own keys. Not while a menu holds the park,
		// or the key would swap the camera out from under an open window. On the key's release: C is the shortcuts
		// table's row 16 (0x0040c5c0), and the park runs its tables on a key-up (FUN_0040c990; docs/exe/scenes.md,
		// "The park Escape route"). Under the full-screen view only the camera's table is run (0x004a2918), and C is not
		// in it.
		if ( Level.Current?.PausedByWindow() != true && Level.Current?.FullScreenView != true
			&& Input.KeyUp( InputButton.CamcorderMode ) )
		{
			ParkCamcorderCameraMode.Enter();
			return;
		}

		// Spin and zoom. These reuse bindings that already exist rather than inventing new ones: what
		// the original binds is not traced yet, so the controls are provisional and the geometry below
		// is the part that matches it.
		//
		// Not while one of the park's screens is open: opening one switches the camera's table off and clears its
		// rows' latches (FUN_00485b70, 0x00485ccd), and the screen's handler runs the shortcuts' table alone, so no
		// camera key is heard until it closes. The mouse still reaches the camera from the park beside the screen.
		var keys = !UI.WindowStack.ParkScreenOpen;

		if ( keys && Input.Pressed( InputButton.RotateLeft ) )
			Yaw -= MathF.PI / 4f;

		if ( keys && Input.Pressed( InputButton.RotateRight ) )
			Yaw += MathF.PI / 4f;

		// Not if the interface already used it, or the wheel over a scrolling list would scroll the list
		// AND zoom the park behind it. Read in the same frame it is written: the HUD updates in Level.Update
		// and this runs from Level.Render, in that order, so there is no lag.
		//
		// Handled here rather than centrally for the reason the camcorder key above gives: a camera
		// mode is what knows when it should give way.
		if ( Input.Mouse.Wheel != 0 && !UI.WindowStack.WheelTaken )
			Zoom = (Zoom - (Input.Mouse.Wheel * 5f)).Clamp( MinZoom, MaxZoom );

		// Scroll the point of interest across the ground, in whatever direction the camera faces, so
		// that "forward" means forward on the screen rather than forward on the map.
		var (forward, right) = ScrollAxes( Input.Keyboard.KeysDown );

		if ( keys && (forward != 0f || right != 0f) )
		{
			var scroll = ScrollSpeed * Time.Delta;

			// The same basis the eye is placed from, and the same one ParkCamcorderCameraMode walks
			// along, so forward scrolls the view forward at every yaw - including the one camcorder mode
			// hands back, which need not be a multiple of pi/4.
			PointOfInterest += new Vector3(
				((forward * -MathF.Sin( Yaw )) + (right * MathF.Cos( Yaw ))) * scroll,
				((forward * MathF.Cos( Yaw )) + (right * MathF.Sin( Yaw ))) * scroll,
				0f );
		}

		Place( GroundUnderPoi() );
	}

	/// <summary>
	/// Which way the held keys scroll the view: W forward and D right, and nothing back or left. <b>Not the
	/// original's</b>, whose camera table binds the arrows and the keypad (<c>0x00748158</c>); and A and S are its
	/// shortcuts' all-items and all-staff rows (<c>ParkFrontEnd.ShortcutRow</c>), so here they scroll nothing.
	/// </summary>
	internal static (float Forward, float Right) ScrollAxes( IReadOnlyCollection<Key> down )
		=> (down.Contains( Key.W ) ? 1f : 0f, down.Contains( Key.D ) ? 1f : 0f);

	/// <summary>
	/// Swings the eye around the point of interest and looks back at it.
	/// </summary>
	/// <remarks>
	/// Called from the constructor as well as from <see cref="Update"/>, for the reason
	/// ParkCamcorderCameraMode.Place gives: the camera is swapped and its view matrix built in the
	/// same <see cref="Camera.Update"/>, so a mode that waited for its first update would have a
	/// frame drawn from the world origin.
	/// </remarks>
	private void Place( float ground )
	{
		Position = EyeAt( PointOfInterest, Yaw, Zoom, ground );

		// Only Forward is taken from this - see Rotation.LookAt, whose roll is arbitrary, and the note
		// on it in AudioListener. Camera builds its view from Forward and a fixed world up, so that is
		// all this has to be right about.
		Rotation = Rotation.LookAt( PointOfInterest - Position );
	}

	/// <summary>
	/// Where the eye stands for a point of interest, a spin, a zoom and the eased ground under the point. The original
	/// builds it as the point plus <c>rotateY( yaw, (0, sin(pitch) * zoom, -cos(pitch) * zoom) )</c>, a height and a
	/// horizontal distance swung about the vertical, in its own axes, where Y is up; this engine has Z up, so the height
	/// goes into Z and the swing happens about Z instead. The original's swing puts the eye at the point less
	/// <c>h sin r</c> across, this one at the point plus <c>h sin yaw</c>, so its rotation is this yaw negated: read in the
	/// running original at rotations 0, 90 and 180 (<c>docs/exe/park-engine.md</c>, "The park camera").
	/// </summary>
	internal static Vector3 EyeAt( Vector3 pointOfInterest, float yaw, float zoom, float ground )
	{
		var pitch = PitchAt( zoom ).DegreesToRadians();

		var horizontal = MathF.Cos( pitch ) * zoom;
		var height = (MathF.Sin( pitch ) * zoom) + ground;

		return pointOfInterest + new Vector3(
			horizontal * MathF.Sin( yaw ),
			-horizontal * MathF.Cos( yaw ),
			height );
	}

	/// <summary>
	/// Puts the point of interest on a thing, as a right click on a list's row and a click on an object window's
	/// preview do (<c>FUN_004867b0</c>; <c>docs/exe/park-engine.md</c>, "The camera goes to a thing"): on the corner of
	/// the cell the thing stands in, ten times the cell's column and row, not on the thing's own position. The spin and
	/// the zoom are left as they are. False, and nothing moved, for nought or an id nothing in the park has.
	/// </summary>
	/// <remarks>
	/// The original's write (<c>FUN_0042aab0</c>) also zeroes the camera's three scroll terms
	/// (<c>0x00790a88</c>..<c>0x00790a90</c>); this camera keeps none. In first person the same two words are where
	/// the viewer stands, and no screen is open there in the original; here only the orbit's point moves.
	/// </remarks>
	public static bool GoToThing( int thingId )
	{
		if ( CellOfThing( thingId, ParkPeople.Current, Level.Current?.ParkState ) is not var (x, y) )
			return false;

		PointOfInterest = new Vector3( x * 10f, y * 10f, 0f );

		Log.Info( $"Camera: to thing {thingId} on cell ({x},{y}), point of interest ({PointOfInterest.X:F0},{PointOfInterest.Y:F0})" );

		return true;
	}

	/// <summary>
	/// The cell a thing stands in - the original's thing bytes <c>+5</c> and <c>+7</c>, the whole part of its
	/// position: a person's from where they walk, a placed object's from its own origin.
	/// </summary>
	internal static (int X, int Y)? CellOfThing( int thingId, ParkPeople? people, ParkState? state )
	{
		if ( thingId == 0 )
			return null;

		if ( people != null )
		{
			if ( people.Guests.TryGetValue( thingId, out var guest ) )
				return guest.Navigator.Position.Cell;

			foreach ( var member in people.Staff )
			{
				if ( member.ThingId == thingId )
					return member.Navigator.Position.Cell;
			}
		}

		if ( state != null && state.TryObject( thingId, out var placed ) )
			return (placed.CellX, placed.CellY);

		return null;
	}

	/// <summary>
	/// Drops where this camera was looking as a scene ends, so the next park does not open wherever
	/// the last one was left - see <see cref="Level.Unload"/>.
	///
	/// <para>
	/// The three pieces of state here are static so they survive <see cref="Camera.SetCameraMode{T}"/>
	/// building a fresh instance, which means they survive the scene too unless something says
	/// otherwise. It matters because coming back from camcorder mode writes wherever the
	/// player wandered to into <see cref="PointOfInterest"/>, so without this a second park opens
	/// looking at a spot that belonged to the first one.
	/// </para>
	/// </summary>
	public static void Forget()
	{
		PointOfInterest = new Vector3( 475f, 175f, 0f );
		Yaw = 0f;
		Zoom = 110f;
	}

	/// <summary>
	/// A one-line summary for the debug console. Cell comes back out of the world position by the same
	/// rule that put it in: <c>grid = (world - 5) / 10</c>.
	///
	/// <para>
	/// <c>ground</c> is sampled fresh rather than being the eased value the camera is actually riding,
	/// so it says what the land under the point of interest is, not where the eye has caught up to.
	/// That is the more useful of the two to read back: it is the thing being followed.
	/// </para>
	/// </summary>
	public static string State()
	{
		var ground = ParkGround.Current?.Heightfield?.HeightAtWorld( PointOfInterest.X, PointOfInterest.Y );

		// The cell the point of interest stands in, which is the world position divided by the cell
		// size - not the (world - 5) / 10 above, which recovers the cell a CENTRE belongs to and comes
		// back fractional anywhere else.
		var cellX = (int)MathF.Floor( PointOfInterest.X / 10f );
		var cellY = (int)MathF.Floor( PointOfInterest.Y / 10f );

		var attributes = ParkGround.Current?.Attributes;

		return $"poi=({PointOfInterest.X:F0},{PointOfInterest.Y:F0}) " +
			$"cell=({(PointOfInterest.X - 5f) / 10f:F1},{(PointOfInterest.Y - 5f) / 10f:F1}) " +
			$"yaw={Yaw:F2} zoom={Zoom:F0} pitch={Pitch:F1} " +
			$"ground={(ground.HasValue ? ground.Value.ToString( "F1" ) : "-")} " +
			$"attr={(attributes != null ? attributes.At( cellX, cellY ).ToString() : "-")}";
	}
}
