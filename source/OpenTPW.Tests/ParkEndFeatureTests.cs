using System;
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
	/// The last frame's weight is <c>(1000 - t) x 0.0005</c>, <c>t</c> held to 100 past 200 and to 1 at nought or below
	/// (<c>0x0042ca13</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> t held to 200 past 200; nought not raised to 1; the weight's sign turned.</remarks>
	[TestMethod]
	[DataRow( 16f, 0.492f )]
	[DataRow( 200f, 0.4f )]
	[DataRow( 250f, 0.45f )]
	[DataRow( 0f, 0.4995f )]
	[DataRow( -5f, 0.4995f )]
	public void TheLastFramesWeightIsHeldAsTheOriginalHoldsIt( float milliseconds, float weight )
		=> Assert.AreEqual( weight, ParkRideViewCameraMode.LastWeight( milliseconds ), 1e-6f );

	/// <summary>
	/// Only the turn, three rows of three, is mixed with the last frame's; the translation, the eye, is the record's own
	/// (<c>0x0042cb83</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the whole matrix mixed; the two weights swapped.</remarks>
	[TestMethod]
	public void OnlyTheTurnIsMixed()
	{
		var now = Numerics.Matrix4x4.CreateTranslation( 1f, 2f, 3f );
		var last = Numerics.Matrix4x4.CreateRotationY( MathF.PI / 2f ) * Numerics.Matrix4x4.CreateTranslation( 9f, 9f, 9f );

		var mixed = ParkRideViewCameraMode.Mix( now, last, 100f );

		// w = 0.45: (1 - w) x 1 + w x cos 90.
		Assert.AreEqual( 0.55f, mixed.M11, 1e-5f, "the turn is mixed, the record's own weighted 1 - w" );
		Assert.AreEqual( 0.45f * last.M13, mixed.M13, 1e-5f );
		Assert.AreEqual( new Numerics.Vector3( 1f, 2f, 3f ), mixed.Translation, "the eye is the record's own" );
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
