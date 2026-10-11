using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// What the advisor wears for a line: the pieces a prop switches (<c>FUN_00598ca0</c>), the props the built
/// park topics' responses name, and his model dressed line after line. The last reads real game files and is
/// skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class AdvisorPropsTests
{
	private const string ModelPath = "global/advisor/Advisor.md2";

	private static (int, bool)[] Switched( int prop ) => AdvisorProps.Switched( prop ).ToArray();

	[TestMethod]
	public void NoughtShowsHisAntennaeAndNothingElse()
		=> CollectionAssert.AreEqual( new[] { (19, true), (20, true) }, Switched( 0 ) );

	[TestMethod]
	public void AHatThatCoversHisAntennaeHidesThemAndIsShown()
	{
		foreach ( var hat in new[] { 1, 2, 4, 5, 6, 7, 9, 10, 11, 13 } )
			CollectionAssert.AreEqual( new[] { (19, false), (20, false), (hat, true) }, Switched( hat ), $"prop {hat}" );
	}

	[TestMethod]
	public void TheSpatulaHidesTheHandThatHoldsIt()
		=> CollectionAssert.AreEqual( new[] { (21, false), (14, true) }, Switched( 14 ) );

	[TestMethod]
	public void AnyOtherPropIsShownAlone()
	{
		foreach ( var prop in new[] { 3, 8, 12, 15 } )
			CollectionAssert.AreEqual( new[] { (prop, true) }, Switched( prop ), $"prop {prop}" );
	}

	[TestMethod]
	public void TheThemesCostumeSwitchesNothing()
		=> Assert.AreEqual( 0, Switched( AdvisorProps.ThemeCostume ).Length );

	[TestMethod]
	public void ARidesLinesNameTheHardHatAndTheDoorsTheTicketHat()
	{
		foreach ( var response in Enumerable.Range( 194, 3 ).Concat( Enumerable.Range( 198, 9 ) ) )
			Assert.AreEqual( (5, 0), ParkAdvisorMessages.PropsOf( response ), $"response {response}" );

		foreach ( var response in Enumerable.Range( 308, 4 ) )
			Assert.AreEqual( (9, 0), ParkAdvisorMessages.PropsOf( response ), $"response {response}" );

		foreach ( var response in new[] { 193, 197, 207, 307, 312, 394 } )
			Assert.AreEqual( (0, 0), ParkAdvisorMessages.PropsOf( response ), $"response {response}" );
	}

	/// <summary>His model's pieces by name after each of three lines: a ride's, the door's, the lobby's.</summary>
	[TestMethod]
	public void HisModelIsDressedLineAfterLine()
	{
		var data = GameData.Required();
		var model = new ModelFile( new MemoryStream( data.ReadAllBytes( ModelPath ) ) );
		var shown = new Dictionary<string, bool>();

		void Show( int node, bool visible ) => shown[model.Meshes[node].Name.TrimEnd( '\0' ).Trim()] = visible;

		AdvisorProps.Wear( model, 5, 0, Show );
		Assert.IsTrue( shown["Hardhat"], "the hard hat on" );
		Assert.IsFalse( shown["Left Antennae"] || shown["Right Antennae"], "his antennae under it" );
		Assert.IsFalse( shown["Ticket Hat"] || shown["Spatula"] || shown["Bowler_BTie"], "no other add-on" );
		Assert.IsFalse( shown.ContainsKey( "Right hand" ), "his hands left alone" );

		AdvisorProps.Wear( model, 9, 0, Show );
		Assert.IsTrue( shown["Ticket Hat"], "the ticket hat on" );
		Assert.IsFalse( shown["Hardhat"], "the hard hat off again" );
		Assert.IsFalse( shown["Left Antennae"] || shown["Right Antennae"], "his antennae still away" );

		AdvisorProps.Wear( model, 0, 0, Show );
		Assert.IsTrue( shown["Left Antennae"] && shown["Right Antennae"], "his antennae back" );
		Assert.AreEqual( 0, shown.Count( piece => piece.Value && piece.Key.Contains( "Hat" ) ), "no hat" );
		Assert.IsFalse( shown["Hardhat"] || shown["Ticket Hat"], "neither hat" );

		AdvisorProps.Wear( model, 13, 14, Show );
		Assert.IsTrue( shown["Fast Food Hat"] && shown["Spatula"], "both props of a row with two" );
		Assert.IsFalse( shown["Right hand"], "the hand that holds the spatula" );
	}

	/// <summary>Every clip of his hides the same ten pieces as it starts: the model's add-ons, ids under 19.</summary>
	[TestMethod]
	public void EveryClipHidesTheTenAddOns()
	{
		var data = GameData.Required();
		var model = new ModelFile( new MemoryStream( data.ReadAllBytes( ModelPath ) ) );
		var addOns = Enumerable.Range( 0, model.Nodes.Count )
			.Where( node => model.Nodes[node].Id is > 0 and < AdvisorProps.FirstOwnPart ).ToArray();

		Assert.AreEqual( 10, addOns.Length );

		for ( var clip = 1; clip <= 15; ++clip )
		{
			var animation = new AnimationFile( new MemoryStream( data.ReadAllBytes( $"global/advisor/Advisorm{clip}.MD2" ) ) );
			CollectionAssert.AreEquivalent( addOns, animation.HideList.Select( node => (int)node ).ToArray(), $"clip {clip}" );
		}
	}
}
