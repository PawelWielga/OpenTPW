using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Numerics = System.Numerics;

namespace OpenTPW.Tests;

/// <summary>
/// The End feature's parts that stand without a running park: its ride view's camera (<see cref="ParkRideViewCameraMode"/>),
/// End.MD2's view node as a fixed item stands (<see cref="RideNodes.FindView"/>, <see cref="RideNodes.PlaceFixed"/>) and the
/// catalogue item it is made from (<see cref="ParkEndFeature.Find"/>). The original's eye, (480.53, 10.00, 200.00), was read
/// from its memory through the End (<c>docs/exe/park-engine.md</c>, "The End feature's view"). The rest is the running
/// game's to show (<c>q703/</c>).
/// </summary>
[TestClass]
public class ParkEndFeatureTests
{
	/// <summary>
	/// The last frame's weight is <c>(1000 - t) x 0.0005</c>, <c>t</c> held to 100 past 200 and raised to 1 below 1
	/// (<c>0x0042b2c7</c>, <c>0x0042ca44</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> t held to 200 past 200; nought not raised to 1; half a millisecond kept; the weight's sign
	/// turned.</remarks>
	[TestMethod]
	[DataRow( 16f, 0.492f )]
	[DataRow( 200f, 0.4f )]
	[DataRow( 250f, 0.45f )]
	[DataRow( 0f, 0.4995f )]
	[DataRow( 0.5f, 0.4995f )]
	[DataRow( -5f, 0.4995f )]
	public void TheLastFramesWeightIsHeldAsTheOriginalHoldsIt( float milliseconds, float weight )
		=> Assert.AreEqual( weight, ParkRideViewCameraMode.LastWeight( milliseconds ), 1e-6f );

	/// <summary>
	/// Only the turn, three rows of three, is mixed with the last frame's; the translation, the eye, is the record's own
	/// (<c>0x0042cb83</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the whole matrix mixed; the two weights swapped; the third row, the one the camera looks
	/// along, left unmixed; a second-row entry mixed into the third.</remarks>
	[TestMethod]
	public void OnlyTheTurnIsMixed()
	{
		var now = Numerics.Matrix4x4.CreateTranslation( 1f, 2f, 3f );
		var last = Numerics.Matrix4x4.CreateRotationX( MathF.PI / 6f ) * Numerics.Matrix4x4.CreateRotationY( MathF.PI / 2f )
			* Numerics.Matrix4x4.CreateTranslation( 9f, 9f, 9f );

		var mixed = ParkRideViewCameraMode.Mix( now, last, 100f );

		// w = 0.45: (1 - w) x 1 + w x cos 90.
		Assert.AreEqual( 0.55f, mixed.M11, 1e-5f, "the turn is mixed, the record's own weighted 1 - w" );
		Assert.AreEqual( 0.45f * last.M13, mixed.M13, 1e-5f );
		Assert.AreEqual( 0.55f + 0.45f * last.M22, mixed.M22, 1e-5f, "the second row" );

		// The third row, which the camera looks along: last's is (0.5, -0.866, 0) turned, its M32 not its M23.
		Assert.AreEqual( 0.45f * last.M31, mixed.M31, 1e-5f, "the third row" );
		Assert.AreEqual( 0.45f * last.M32, mixed.M32, 1e-5f );
		Assert.AreEqual( 0.55f + 0.45f * last.M33, mixed.M33, 1e-5f );
		Assert.AreNotEqual( last.M23, last.M32, 1e-3f, "a slip between the second row and the third shows" );
		Assert.AreEqual( new Numerics.Vector3( 1f, 2f, 3f ), mixed.Translation, "the eye is the record's own" );
	}

	/// <summary>
	/// <b>In the End's ride view no body is pulled toward the eye and the world is fogged</b>, as in first person: the
	/// original's render word <c>0x2000</c> is set for <c>gui_CameraFlags &amp; 0x16</c> (<c>0x0054fae2</c>), whose
	/// <c>0x4</c> the ride view sets, and the pull is skipped under it (<c>0x00589f24</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the pull keyed to first person alone; the inside view not holding the ride view.</remarks>
	[TestMethod]
	public void InTheRideViewNoBodyIsPulledAsInFirstPerson()
	{
		Assert.IsTrue( ParkGuestSprites.PullsBodies, "over the orbit the bodies are pulled" );

		try
		{
			ParkRideViewCameraMode.Viewed = () => Numerics.Matrix4x4.Identity;
			Camera.SetCameraMode<ParkRideViewCameraMode>();

			Assert.IsTrue( Level.InsideView );
			Assert.IsFalse( ParkGuestSprites.PullsBodies, "in the ride view none is" );
		}
		finally
		{
			ParkRideViewCameraMode.Leave();
			Camera.SetCameraMode<CameraMode>();
		}

		Assert.IsTrue( ParkGuestSprites.PullsBodies, "and once it is left they are again" );
	}

	/// <summary>
	/// <b>Escape under the End ends it once and gives the screen back</b> (<c>0x0048a9b5</c> on): its script ended, the
	/// handle noughted, the ride view left for the orbit, the cover and its keys off, and UITEXT 471's box put up in
	/// Instant Action alone (<c>[0x00fb3b7c] == 2</c>). A second ending does nothing.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the box in either mode; the box not put up; the script left running; the ride view not left;
	/// the cover left on; the handle not noughted (a second Escape opens a second box); the thing not deleted.
	/// </remarks>
	[TestMethod]
	[DataRow( true )]
	[DataRow( false )]
	public void EscapeEndsItOnceAndOnlyInstantActionGetsTheBox( bool instantAction )
	{
		var data = GameData.Required();
		FileSystem = data;
		Log ??= new();
		var world = new ParkWorld( new SaveReader( new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) ) ).ReadFile() );
		var catalogue = new ParkItemCatalogue( "jungle", data, instantAction: instantAction );
		var item = ParkEndFeature.Find( catalogue )!.Value;
		var rides = new ParkRides( "jungle", world, catalogue, data );
		var objects = new ParkObjects( "jungle", null, null );
		var stack = new UI.WindowStack();

		try
		{
			var scripts = rides.Scheduler.Count;
			var script = rides.BindFixed( 43, item );
			Assert.IsNotNull( script );
			Assert.AreEqual( scripts + 1, rides.Scheduler.Count );

			var view = RideNodes.Load( item.Directory, item.Stem, data, false, [] )!;
			view.PlaceFixed();

			var state = new ParkState( world );
			var end = (ParkEndFeature)Activator.CreateInstance( typeof( ParkEndFeature ), BindingFlags.NonPublic | BindingFlags.Instance,
				null, [43, script, item, view, stack, rides, objects, state, instantAction], null )!;
			typeof( ParkEndFeature ).GetProperty( nameof( ParkEndFeature.Current ), BindingFlags.NonPublic | BindingFlags.Static )!
				.SetValue( null, end );

			ParkRideViewCameraMode.Viewed = () => Numerics.Matrix4x4.Identity;
			Camera.SetCameraMode<ParkRideViewCameraMode>();
			stack.Cover( true );
			var keys = typeof( ParkEndFeature ).GetMethod( "KeyReleased", BindingFlags.NonPublic | BindingFlags.Instance )!
				.CreateDelegate<Action<Input.KeyRelease>>( end );
			stack.CoverKeys = keys;

			stack.CoverKeys( new Input.KeyRelease( NeoVeldrid.Key.Escape, Input.Modifiers.None ) );

			Assert.IsNull( ParkEndFeature.Current, "the handle noughted" );
			CollectionAssert.AreEqual( new[] { 43 }, state.DeletedThingIds.ToArray(), "its thing deleted, its id freed at the sweep's end" );
			Assert.AreEqual( scripts, rides.Scheduler.Count, "End.RSE ended" );
			Assert.IsFalse( ParkRideViewCameraMode.Active, "the ride view left" );
			Assert.IsInstanceOfType( typeof( Camera ).GetField( "CameraMode", BindingFlags.NonPublic | BindingFlags.Static )!
				.GetValue( null ), typeof( ParkOrbitCameraMode ), "for the orbit" );
			Assert.IsFalse( stack.Covered, "the layer shown" );
			Assert.IsNull( stack.CoverKeys, "the keys given back" );
			Assert.AreEqual( instantAction ? 1 : 0, stack.Windows.OfType<UI.MessageBox>().Count(), "UITEXT 471's box in Instant Action alone" );

			keys( new Input.KeyRelease( NeoVeldrid.Key.Escape, Input.Modifiers.None ) );
			Assert.AreEqual( instantAction ? 1 : 0, stack.Windows.OfType<UI.MessageBox>().Count(), "a second ending does nothing" );
		}
		finally
		{
			ParkEndFeature.Forget();
			ParkRideViewCameraMode.Leave();
			Camera.SetCameraMode<CameraMode>();
			rides.Delete();
			Entity.ApplyDeletions();
		}
	}

	/// <summary>
	/// The camera stands at the view matrix's translation and looks along its third row, both swapped from the model's
	/// axes (y up) to the park's (z up).
	/// </summary>
	/// <remarks><b>Mutations:</b> y and z not swapped; the second row looked along.</remarks>
	[TestMethod]
	public void TheEyeAndTheLookAreTheNodesSwappedToZUp()
	{
		var turned = Numerics.Matrix4x4.CreateRotationX( -16f * MathF.PI / 180f ) * Numerics.Matrix4x4.CreateTranslation( 480.5f, 10f, 200f );

		try
		{
			ParkRideViewCameraMode.Viewed = () => turned;
			Camera.SetCameraMode<ParkRideViewCameraMode>();

			Assert.IsTrue( ParkRideViewCameraMode.Active );
			Assert.AreEqual( 480.5f, Camera.Position.X, 1e-3f );
			Assert.AreEqual( 200f, Camera.Position.Y, 1e-3f );
			Assert.AreEqual( 10f, Camera.Position.Z, 1e-3f );

			var look = Camera.Rotation.Forward;
			Assert.AreEqual( 0f, look.X, 1e-3f );
			Assert.AreEqual( MathF.Cos( 16f * MathF.PI / 180f ), look.Y, 1e-3f, "towards the planes, +y in the park" );
			Assert.AreEqual( MathF.Sin( 16f * MathF.PI / 180f ), look.Z, 1e-3f, "sixteen degrees up" );
		}
		finally
		{
			ParkRideViewCameraMode.Leave();
			Camera.SetCameraMode<CameraMode>();
		}

		Assert.IsFalse( ParkRideViewCameraMode.Active );
	}

	/// <summary>
	/// End.MD2's view node, id 1 under <c>0x1000</c>, stands where the original's eye stood, (480.53, 10, 200), only when
	/// the model stands as its file holds it: placed at a cell, its root's own (480, 0, 170) would be lost.
	/// </summary>
	/// <remarks><b>Mutations:</b> the fixed placement keeping the cell's root; the view looked for in another space.</remarks>
	[TestMethod]
	public void EndsViewNodeStandsWhereTheOriginalsEyeStood()
	{
		var data = GameData.Required();
		var nodes = RideNodes.Load( "levels/jungle/features/end", "end", data, false, [] );

		Assert.IsNotNull( nodes );
		nodes!.PlaceFixed();
		Assert.AreEqual( NodeEnd.Posed, nodes.FindView( out var eye ) );

		Assert.AreEqual( 480.53f, eye.M41, 0.01f );
		Assert.AreEqual( 10f, eye.M42, 0.01f );
		Assert.AreEqual( 200f, eye.M43, 0.01f );
		Assert.AreEqual( 0f, eye.M31, 1e-3f );
		Assert.AreEqual( MathF.Sin( 16f * MathF.PI / 180f ), eye.M32, 1e-3f, "the third row turned sixteen degrees up" );
		Assert.AreEqual( MathF.Cos( 16f * MathF.PI / 180f ), eye.M33, 1e-3f );

		nodes.Place( Vector3.Zero, 0 );
		nodes.FindView( out var atACell );
		Assert.AreEqual( 30f, atACell.M43, 0.01f, "at a cell the root's own translation goes" );
	}

	/// <summary>A model with no view node has none to find.</summary>
	[TestMethod]
	public void AModelWithoutAViewNodeHasNone()
	{
		var nodes = RideNodes.Load( "levels/jungle/shops/giftshop", "giftshop", GameData.Required(), false, [] );

		Assert.AreEqual( NodeEnd.Missing, nodes!.FindView( out _ ) );
	}

	/// <summary>Escape let go with no modifier ends the End; with one held, or another key, it stands.</summary>
	/// <remarks><b>Mutations:</b> a modifier allowed; another key heard.</remarks>
	[TestMethod]
	public void EscapeWithNoModifierEndsIt()
	{
		Assert.IsTrue( ParkEndFeature.KeyEnds( new Input.KeyRelease( NeoVeldrid.Key.Escape, Input.Modifiers.None ) ) );
		Assert.IsFalse( ParkEndFeature.KeyEnds( new Input.KeyRelease( NeoVeldrid.Key.Escape, Input.Modifiers.Shift ) ) );
		Assert.IsFalse( ParkEndFeature.KeyEnds( new Input.KeyRelease( NeoVeldrid.Key.F3, Input.Modifiers.None ) ) );
	}

	/// <summary>A right click ends it with RMB cancel on and strictly more than 2000 ms after it was made.</summary>
	/// <remarks><b>Mutations:</b> 2000 itself enough; RMB cancel not asked.</remarks>
	[TestMethod]
	[DataRow( true, 2001L, true )]
	[DataRow( true, 2000L, false )]
	[DataRow( true, 150L, false )]
	[DataRow( false, 5000L, false )]
	public void ARightClickEndsItPastTwoSecondsWithRmbCancel( bool rmbCancel, long since, bool ends )
		=> Assert.AreEqual( ends, ParkEndFeature.RightClickEnds( rmbCancel, since ) );

	/// <summary>The item named <c>End</c> is jungle's 1605, in either mode's catalogue.</summary>
	[TestMethod]
	[DataRow( true )]
	[DataRow( false )]
	public void TheEndIsTheItemNamedEnd( bool instantAction )
	{
		var catalogue = new ParkItemCatalogue( "jungle", GameData.Required(), instantAction: instantAction );

		Assert.AreEqual( 1605, ParkEndFeature.Find( catalogue )?.Id );
	}
}
