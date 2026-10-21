using NeoVeldrid;
using OpenTPW.UI;

namespace OpenTPW;

/// <summary>
/// The End feature, the executable's <c>StartWinningSequence</c> and <c>EndWinningSequence</c>: at the end of research the
/// catalogue item named <c>End</c> made as a thing at (0, 0, 0), its script and that script's child run, the camera put
/// in its ride view, and the screen taken until Escape or a right click ends it (<c>docs/exe/park-engine.md</c>, "The End
/// feature's view" and "The End feature's handler").
/// </summary>
/// <remarks>
/// <para>
/// <b>Made</b> (<c>World_MakeEndFeature</c>, <c>0x00516b00</c>, then <c>FUN_0048ae70</c>): first person left, the item's
/// model stood unturned at the world's origin (<c>Info.DontApplyOffset</c>), its <c>.RSE</c> bound from word 0, the
/// camera on the model's view node (<see cref="ParkRideViewCameraMode"/>), the full-screen view taken off where it was on,
/// the open park screen closed, and a control of its own put over the park's layer, which hides the gadget, the money
/// and the help bar and takes the keys and the presses (<see cref="WindowStack.Cover"/>); the pointer stays.
/// </para>
/// <para>
/// <b>Ended</b> (<c>EndFeature_OnMessage</c>, <c>0x0048a970</c>): Escape let go with no modifier, or a right click with
/// RMB cancel on more than 2000 ms after the control was made (<c>0x0048aaf8</c>). The thing is deleted
/// (<c>World_DeleteEndFeature</c>, <c>0x00516c10</c>), which ends its scripts and the music dip with them and stops the
/// particles their <c>ADDOBJ</c>s started, an <c>EVENT</c>'s left to burn out; the orbit comes back where it was, the
/// layer is shown and the keys given back, and in Instant Action UITEXT 471's box is put up with its cross alone.
/// </para>
/// <para>
/// <b>Not the original's:</b> the thing is not put in the park's object list, which only the save reads. The keys
/// reach no save while the End has them, but closing the window does: it writes <c>autosave.TPWS</c>
/// (<see cref="Level.WriteParkOnLeaving"/>) with no End in it, its scripts not written
/// (<c>SAVE_PARK_SCRIPT_MADE_SINCE_THE_LOAD</c>). Whether the original's save as it closes holds its End is not
/// measured. Its id is the park's next. <c>FUN_004862a0</c>, the re-focus at the end, is not decoded and has nothing
/// to do here: the cover gives the keys back.
/// </para>
/// </remarks>
internal sealed class ParkEndFeature
{
	/// <summary>The name the catalogue item is found by (<c>+0x390</c>, the string object <c>0x007cf9c0</c>).</summary>
	public const string ItemName = "End";

	/// <summary>How long after the control is made a right click ends it, strictly more (<c>0x0048aaf8</c>).</summary>
	public const long RightClickAfter = 2000;

	/// <summary>The End standing now, the original's handle <c>0x007cf4f4</c>; null when there is none.</summary>
	internal static ParkEndFeature? Current { get; private set; }

	/// <summary>The End's thing id.</summary>
	public int ThingId { get; }

	/// <summary>The End's own script, <c>End.RSE</c>.</summary>
	public RideScript Script { get; }

	/// <summary>When the control was made, by <see cref="Time.WallMilliseconds"/> (its message <c>0x15</c>, <c>0x0048ac16</c>).</summary>
	public long MadeAt { get; }

	private readonly ParkItemCatalogue.Item _item;
	private readonly RideNodes _view;
	private readonly WindowStack _stack;
	private readonly ParkRides _rides;
	private readonly ParkObjects _objects;
	private readonly ParkState _state;
	private readonly bool _instantAction;

	private ParkEndFeature( int thingId, RideScript script, ParkItemCatalogue.Item item, RideNodes view, WindowStack stack,
		ParkRides rides, ParkObjects objects, ParkState state, bool instantAction )
	{
		ThingId = thingId;
		Script = script;
		_item = item;
		_view = view;
		_stack = stack;
		_rides = rides;
		_objects = objects;
		_state = state;
		_instantAction = instantAction;
		MadeAt = Time.WallMilliseconds;
	}

	/// <summary>The catalogue's item named <see cref="ItemName"/>, the first by id.</summary>
	internal static ParkItemCatalogue.Item? Find( ParkItemCatalogue catalogue )
		=> catalogue.All.Where( item => item.Name == ItemName ).OrderBy( item => item.Id ).Cast<ParkItemCatalogue.Item?>()
			.FirstOrDefault();

	/// <summary>
	/// Makes the End and takes the screen, or says why not and answers null. An End standing already is left alone.
	/// </summary>
	internal static ParkEndFeature? Make( ParkItemCatalogue catalogue, ParkState state, ParkRides rides, ParkObjects objects,
		WindowStack stack, bool instantAction )
	{
		if ( Current is { } standing )
			return standing;

		if ( Find( catalogue ) is not { } item )
		{
			Log.Warning( $"End feature: Could not find the {ItemName} feature" );
			return null;
		}

		RideNodes? view;
		RideAnimations animations;
		LobbyModel model;

		try
		{
			view = RideNodes.Load( item.Directory, item.Stem, FileSystem, false, [] );
			animations = RideAnimations.Load( item.Directory, item.Stem, FileSystem, item.AnimationChannels );
			model = new LobbyModel( $"{item.Directory}/{item.Stem}.MD2", $"{item.Directory}/textures", Vector3.Zero,
				clips: animations.AllClips );
		}
		catch ( Exception e )
		{
			Log.Warning( $"End feature: '{item.Directory}/{item.Stem}.MD2' will not read, so no End is made - {e.Message}" );
			return null;
		}

		if ( view is null || view.FindView( out _ ) == NodeEnd.Missing )
		{
			Log.Warning( $"End feature: '{item.Directory}/{item.Stem}.MD2' has no view node, so no End is made" );
			foreach ( var entity in model.Entities )
				entity.Delete();
			return null;
		}

		view.PlaceFixed();

		var thingId = state.NextThingId();

		if ( thingId == 0 )
		{
			// Not asked for again this session: the End is owed once, when the last item is researched
			// (Level.OweTheEndFeature), and the owing is cleared before this runs.
			Log.Warning( "End feature: the thing table is full, so no End is made" );
			foreach ( var entity in model.Entities )
				entity.Delete();
			return null;
		}

		objects.Stand( thingId, model, animations, item.Id );

		if ( rides.BindFixed( thingId, item ) is not { } script )
		{
			// The id taken above is not given back: nothing here deletes the thing (a queued item).
			Log.Warning( $"End feature: thing {thingId}'s script would not run, so no End is made" );
			objects.Remove( thingId );
			return null;
		}

		// FUN_0048ae70: first person or a ride view left, then the End's own ride view.
		if ( ParkCamcorderCameraMode.Active )
			ParkCamcorderCameraMode.Leave();

		var end = Current = new ParkEndFeature( thingId, script, item, view, stack, rides, objects, state, instantAction );

		ParkRideViewCameraMode.Viewed = end.Eye;
		Camera.SetCameraMode<ParkRideViewCameraMode>();

		// The full-screen view off where it was on (FUN_004a2a90, FUN_004a29d0), the open park screen closed, the layers
		// hidden under the End's own control, which has the keys.
		if ( stack.Covered )
		{
			stack.Cover( false );
			Log.Info( "Full-screen view: off" );
		}

		var closed = stack.CloseParkScreen();
		stack.Cover( true );
		stack.CoverKeys = end.KeyReleased;
		stack.CoverRightClick = end.RightClickAnswer;

		view.FindView( out var eye );
		Log.Info( $"End feature: thing {thingId} ('{item.Name}', catalogue {item.Id}) made at (0, 0, 0), running "
			+ $"{item.Stem}.RSE as script {script.Id}; the camera "
			+ $"on its view node at ({eye.M41:F2}, {eye.M43:F2}, {eye.M42:F2}), its third row ({eye.M31:F3}, {eye.M33:F3}, "
			+ $"{eye.M32:F3}); the park's layer hidden{(closed ? ", the park screen closed" : "")}" );

		return end;
	}

	/// <summary>Lets go of an End left standing as its park is unloaded; the park's things go with the park.</summary>
	internal static void Forget() => Current = null;

	/// <summary>The view node's matrix now, for <see cref="ParkRideViewCameraMode.Viewed"/>.</summary>
	private System.Numerics.Matrix4x4? Eye()
		=> _view.FindView( out var world ) == NodeEnd.Missing ? null : world;

	/// <summary>A key let go under the End's control (message <c>0x1000b</c>): Escape with no modifier ends it.</summary>
	private void KeyReleased( Input.KeyRelease release )
	{
		if ( KeyEnds( release ) )
			Finish( "Escape" );
	}

	/// <summary>Whether a key let go ends the End: Escape with no modifier held (<c>0x0048a99e</c>..<c>0x0048a9af</c>).</summary>
	internal static bool KeyEnds( Input.KeyRelease release ) => release.Key == Key.Escape && release.Plain;

	/// <summary>
	/// Whether a right click <paramref name="since"/> ms after the control was made ends the End: with RMB cancel on and
	/// strictly more than <see cref="RightClickAfter"/> (<c>jle</c> at <c>0x0048aaf8</c>).
	/// </summary>
	internal static bool RightClickEnds( bool rmbCancel, long since ) => rmbCancel && since > RightClickAfter;

	/// <summary>What a right click on the End's control answers, asked at the press.</summary>
	private Action? RightClickAnswer() => RightClicked;

	/// <summary>
	/// A right click (message <c>0x10006</c>, button 1): with RMB cancel on and more than <see cref="RightClickAfter"/>
	/// since the control was made, it ends the End, both asked at the click.
	/// </summary>
	private void RightClicked()
	{
		var since = Time.WallMilliseconds - MadeAt;

		if ( !RightClickEnds( GameOptions.Current.RmbCancel, since ) )
		{
			Log.Info( $"End feature: a right click {since} ms after it was made, RMB cancel "
				+ $"{(GameOptions.Current.RmbCancel ? "on" : "off")}, so it stands" );
			return;
		}

		Finish( $"a right click {since} ms after it was made" );
	}

	/// <summary>The handler's one body for both ways out (<c>0x0048a9b5</c> on).</summary>
	private void Finish( string why )
	{
		if ( Current != this )
			return;

		// World_DeleteEndFeature: the thing deleted, its scripts with it, the handle noughted, the ride view left.
		var before = _rides.Scheduler.Count;
		_rides.Unbind( ThingId, _item );
		_objects.Remove( ThingId );
		_state.DeleteThing( ThingId );
		Current = null;

		ParkRideViewCameraMode.Leave();
		Camera.SetCameraMode<ParkOrbitCameraMode>();

		// Message 6 to both layers: layer 1 hidden, layer 0 shown; the keys back.
		_stack.Cover( false );

		Log.Info( $"End feature: ended by {why}: thing {ThingId} deleted, {before - _rides.Scheduler.Count} script(s) ended; "
			+ "the orbit and the park's layer back" + (_instantAction ? ", and in Instant Action UITEXT 471's box" : "") );

		if ( _instantAction )
			_stack.Open( new MessageBox( _stack, Localization.Get( UIStrings.CongratulationsInstantAction ), null ) );
	}
}
