using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The research screen (<see cref="UI.ParkResearchScreen"/>, <c>ResearchScreen_Open</c> and <c>ResearchScreen_OnMessage</c>)
/// over a fresh Full Simulation Lost Kingdom lab, the one the original makes (<c>restart.INTS</c>; Q349): its bars, labels,
/// sliders and arrows, and the lab's setters behind them. These read the item files and the interface's meshes from the
/// shipped game and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkResearchScreenTests
{
	private BaseFileSystem data = null!;
	private Point2 _screen;

	/// <summary>Full Simulation's most: <c>StaffPoolInfo.MaxResearchersInPark</c> 10 times the top grade's ability 6.</summary>
	private const int Most = 60;

	[TestInitialize]
	public void Mount()
	{
		_screen = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );
		Log ??= new();
		FileSystem = data = GameData.Required();
	}

	[TestCleanup]
	public void PutTheScreenBack() => Screen.Size = _screen;

	private ParkResearch FreshLab()
	{
		var catalogue = new ParkItemCatalogue( "jungle", data );
		var balance = new ParkBalance( "jungle", easyMode: false );
		var fresh = new FreshPark( "jungle", balance, catalogue, data );
		return new ParkResearch( fresh.ObjectControlRecords, catalogue, fresh.ResearchProjects, fresh.ResearchKinds,
			fresh.ResearchWorkLoad, ParkResearch.GroupOpensAt( balance ), fresh.ResearchMostExpensiveProject );
	}

	private static UI.ParkResearchScreen Open( UI.WindowStack stack, ParkResearch lab, int ability )
	{
		var screen = new UI.ParkResearchScreen( stack, lab, () => ability, Most );
		stack.Open( screen );
		return screen;
	}

	private static IEnumerable<UI.UiControl> Controls( UI.UiControl control )
		=> new[] { control }.Concat( control.Children.SelectMany( Controls ) );

	private static UI.UiSlider Slider( UI.ParkResearchScreen screen, int id ) => Controls( screen.Root ).OfType<UI.UiSlider>().Single( slider => slider.Id == id );

	/// <summary>Each kind's bar, by the panel it stands in.</summary>
	private static UI.ParkObjectWindow.UiStatBar Bar( UI.ParkResearchScreen screen, int panel )
		=> screen.Root.Children.Single( control => control.Id == panel ).Children.OfType<UI.ParkObjectWindow.UiStatBar>().Single();

	private static UI.UiButton Arrow( UI.ParkResearchScreen screen, int panel, int id )
		=> screen.Root.Children.Single( control => control.Id == panel ).Children.OfType<UI.UiButton>().Single( button => button.Id == id );

	/// <summary>
	/// <b>The fresh lab's dearest project is project 3, and the five bars open at the original's</b>: rides (Dino Karts,
	/// 600 of 1300 left) 46 per cent, so 552 of 1024; shops (1212, 450) 675; sideshows (1304, 700) 481; miscellaneous
	/// (Stone Head, 250) 829; upgrades (1100's tier 1, 250) 829 (<c>docs/exe/hud.md</c>, "The research screen's panels").
	/// Each label is UITEXT 446, the item and the price the original letters.
	/// </summary>
	/// <remarks><b>Mutations:</b> the dearest taken as the last strictly dearer, or the first at all; the per cent of the
	/// project's own cost; the fill of what is left rather than done; kind 4's price of tier nought.</remarks>
	[TestMethod]
	public void TheBarsOpenAtTheOriginals()
	{
		var lab = FreshLab();
		Assert.AreEqual( 3, lab.MostExpensiveProject );
		CollectionAssert.AreEqual( new[] { 46, 34, 53, 19, 19 }, Enumerable.Range( 0, 5 ).Select( lab.PercentLeftOfDearest ).ToArray() );

		var screen = Open( new UI.WindowStack(), lab, ability: 4 );
		CollectionAssert.AreEqual( new[] { 552, 829, 675, 481, 829 },
			UI.ParkResearchScreen.Rows.Select( row => Bar( screen, row.Panel ).Units ).ToArray(), "rides, upgrades, shops, sideshows, miscellaneous" );
		CollectionAssert.AreEqual( new[]
		{
			"DinoKarts\nEstimated price : $ 6500", "Belly Bounce\nEstimated price : $ 400", "Fries Shop\nEstimated price : $ 750",
			"Laughing Hyenas\nEstimated price : $ 1750", "Stone Head\nEstimated price : $ 100"
		}, UI.ParkResearchScreen.Rows.Select( row => Bar( screen, row.Panel ).Text ).ToArray() );
	}

	/// <summary>
	/// <b>The dearest project is the first strictly dearer than all before it</b> (the fill's compare, <c>0x00503243</c>):
	/// of 500, 1300, 1300 and 200 it is the first 1300, project 1; with none, -1.
	/// </summary>
	/// <remarks><b>Mutations:</b> the compare taken at or over, which answers the second 1300.</remarks>
	[TestMethod]
	public void TheDearestIsTheFirstOfTheDearest()
	{
		ParkWorld.ResearchProject Costing( float cost ) => new( 1150, 0, 1, 0, cost, 0 );

		Assert.AreEqual( 1, ParkResearch.MostExpensive( [Costing( 500 ), Costing( 1300 ), Costing( 1300 ), Costing( 200 )] ) );
		Assert.AreEqual( -1, ParkResearch.MostExpensive( [] ) );
	}

	/// <summary>
	/// <b>Opening the screen takes the work load round the knob</b>, as the original does (measured, Q590): with one
	/// researcher of ability 3 the limit is <c>(97 / 3 + 3) × 1024 / 60</c> = 597, the knob <c>15 × 597 / 30</c> = 298,
	/// and back <c>30 × 298 / 597 + 70</c> = 84; the next opening 83. Every output is worked again by it, 48, 6, 14, 6 and
	/// 4. Of ability 4 the round trip comes back: the limit 614, the knob 307, the work load 85.
	/// </summary>
	/// <remarks><b>Mutations:</b> the knob set without telling the handler; the limit's third taken of c; the knob's
	/// division rounded; the outputs left as they were.</remarks>
	[TestMethod]
	public void OpeningTakesTheWorkLoadRoundTheKnob()
	{
		var lab = FreshLab();
		var stack = new UI.WindowStack();
		var screen = Open( stack, lab, ability: 3 );

		StringAssert.StartsWith( screen.Census(), "workload 84 limit 597 knob 298 dial 4 gauge 597 ability 3 most 60;" );
		CollectionAssert.AreEqual( new[] { 48, 6, 14, 6, 4 }, lab.Kinds.Select( kind => kind.Output ).ToArray() );
		CollectionAssert.AreEqual( new[] { 100, 15, 30, 15, 10 }, lab.Kinds.Select( kind => kind.Effort ).ToArray(), "the efforts written back as they stand" );

		stack.Close( screen );
		Open( stack, lab, ability: 3 );
		Assert.AreEqual( 83, lab.WorkLoad );

		var other = FreshLab();
		StringAssert.StartsWith( Open( new UI.WindowStack(), other, ability: 4 ).Census(), "workload 85 limit 614 knob 307 dial 4 gauge 614 ability 4" );
		CollectionAssert.AreEqual( new[] { 49, 6, 14, 6, 4 }, other.Kinds.Select( kind => kind.Output ).ToArray() );
	}

	/// <summary>
	/// <b>The efforts' seeding works the outputs again by itself</b>: at work load 70 the knob stays at nought, so it tells
	/// nobody, and a lab whose outputs read 1 to 5 opens with every kind's worked from its effort, 40, 5, 11, 5 and 3.
	/// </summary>
	/// <remarks><b>Mutations:</b> the seeding's write-back taken out.</remarks>
	[TestMethod]
	public void TheEffortsSeedingWorksTheOutputs()
	{
		var catalogue = new ParkItemCatalogue( "jungle", data );
		var balance = new ParkBalance( "jungle", easyMode: false );
		var fresh = new FreshPark( "jungle", balance, catalogue, data );
		var stale = fresh.ResearchKinds.Select( ( kind, at ) => kind with { Output = at + 1 } ).ToArray();
		var lab = new ParkResearch( fresh.ObjectControlRecords, catalogue, fresh.ResearchProjects, stale, 70,
			ParkResearch.GroupOpensAt( balance ), fresh.ResearchMostExpensiveProject );

		StringAssert.StartsWith( Open( new UI.WindowStack(), lab, ability: 3 ).Census(), "workload 70 limit 597 knob 0 dial 0 " );
		CollectionAssert.AreEqual( new[] { 40, 5, 11, 5, 3 }, lab.Kinds.Select( kind => kind.Output ).ToArray() );
	}

	/// <summary>
	/// <b>The work-load slider runs up and down, held to its limit</b>: at the top, 1023, it is held to 597 and the work
	/// load is 100, the dial at frame 8; at its foot the work load is 70 and the dial 0; at 300, 85 and frame 4. The knob
	/// stands 513 down, <c>(1023 - 298) × (481 - 67) / 1023</c> under the track's top at 220.
	/// </summary>
	/// <remarks><b>Mutations:</b> the hold taken out; the dial's frame of the value rather than of its thirtieths; the
	/// slider laid across, or its top the lowest.</remarks>
	[TestMethod]
	public void TheWorkLoadSliderIsHeldToItsLimit()
	{
		var lab = FreshLab();
		var screen = Open( new UI.WindowStack(), lab, ability: 3 );
		var slider = Slider( screen, 0xd2d10 );

		Assert.AreEqual( (513, 580), (slider.Thumb!.Rect.Top, slider.Thumb.Rect.Bottom), "the knob at 298" );

		foreach ( var (value, workLoad, dial, held) in new[] { (1023, 100, 8, 597), (0, 70, 0, 0), (300, 85, 4, 300) } )
		{
			slider.SetValue( value );
			slider.Moved!();
			Assert.AreEqual( (held, workLoad), (slider.Value, lab.WorkLoad), $"{value}" );
			StringAssert.Contains( screen.Census(), $"knob {held} dial {dial} " );
		}

		lab.SetWorkLoad( 120 );
		Assert.AreEqual( 100, lab.WorkLoad, "held to a hundred" );
	}

	/// <summary>
	/// <b>An effort slider sets its kind's effort, and every output follows</b>: the rides to 50 of a sum of 120 at work
	/// load 84 give 34, 10, 21, 10 and 6. An effort over a hundred is held there, and so is one under nought, which the
	/// original takes unsigned.
	/// </summary>
	/// <remarks><b>Mutations:</b> the slider's kinds swapped; only the kind's own output worked again.</remarks>
	[TestMethod]
	public void AnEffortSliderSetsItsKind()
	{
		var lab = FreshLab();
		var screen = Open( new UI.WindowStack(), lab, ability: 3 );
		var rides = Slider( screen, 0xd2d0e );

		rides.SetValue( 50 );
		rides.Moved!();
		CollectionAssert.AreEqual( new[] { 50, 15, 30, 15, 10 }, lab.Kinds.Select( kind => kind.Effort ).ToArray() );
		CollectionAssert.AreEqual( new[] { 34, 10, 21, 10, 6 }, lab.Kinds.Select( kind => kind.Output ).ToArray() );

		Slider( screen, 0xd2d0b ).SetValue( 40 );
		Slider( screen, 0xd2d0b ).Moved!();
		Assert.AreEqual( 40, lab.Kinds[ItemDescriptionFile.Feature].Effort, "0xd2d0b is the miscellaneous" );

		Assert.AreEqual( (100, 100), (lab.SetEffort( 1, 150 ), lab.SetEffort( 2, -5 )) );
	}

	/// <summary>
	/// <b>The arrows pick the kind's next and previous project</b>: right from Dino Karts (project 4) is project 5, item
	/// 1106 (550, so 593 of 1024), left from there is 4 again, and left from 4, with no open ride project of group 1
	/// before it, is the last such after it, project 7.
	/// </summary>
	/// <remarks><b>Mutations:</b> the left arrow taking the first after; the bar left unset after an arrow.</remarks>
	[TestMethod]
	public void TheArrowsPickTheKindsProjects()
	{
		var lab = FreshLab();
		var screen = Open( new UI.WindowStack(), lab, ability: 3 );

		Arrow( screen, 0xd2d01, 0xd2d04 ).Clicked!();
		Assert.AreEqual( (5, 1106, 593), (lab.Kinds[0].CurrentProject, lab.CurrentItem( 0 ), Bar( screen, 0xd2d01 ).Units) );

		Arrow( screen, 0xd2d01, 0xd2d03 ).Clicked!();
		Assert.AreEqual( 4, lab.Kinds[0].CurrentProject );
		Arrow( screen, 0xd2d01, 0xd2d03 ).Clicked!();
		Assert.AreEqual( (7, 1180), (lab.Kinds[0].CurrentProject, lab.CurrentItem( 0 )) );
	}

	/// <summary>
	/// <b>A change in the researchers' ability sets the limit and the knob again</b>, on the next pass (message
	/// <c>0x1e</c>): ability 6 gives 631 and a knob of <c>14 × 631 / 30</c> = 294, whose round trip takes the 84 to 83.
	/// </summary>
	[TestMethod]
	public void AChangedAbilitySetsTheLimitAgain()
	{
		var lab = FreshLab();
		var ability = 3;
		var screen = new UI.ParkResearchScreen( new UI.WindowStack(), lab, () => ability, Most );
		Assert.AreEqual( 84, lab.WorkLoad );

		ability = 6;
		screen.Update();
		StringAssert.StartsWith( screen.Census(), "workload 83 limit 631 knob 294 " );
	}
}
