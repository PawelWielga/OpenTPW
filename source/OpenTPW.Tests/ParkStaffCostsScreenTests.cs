using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The Staff Training Budgets screen, the Money category's screen 9 (<c>FUN_004b2750</c>, handler <c>0x004b24b0</c>;
/// <c>docs/exe/hud.md</c>, "The Staff Training Budgets screen"), and the budgets it sets as a park file holds them.
/// These read the shipped game and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkStaffCostsScreenTests
{
	private BaseFileSystem data = null!;

	/// <summary>The spinners' buttons on the interface's 2048x1536: the minus and the plus, and each row's middle.</summary>
	private const float Minus = 955;
	private const float Plus = 1316;
	private static readonly float[] RowMiddles = [254, 409, 563, 717, 871];

	private Point2 _screen;
	private ParkWorld _shipped = null!;
	private readonly System.Collections.Generic.List<Entity> _made = [];

	/// <summary>A park's people, deleted as the test ends.</summary>
	private ParkPeople People( ParkWorld park )
	{
		var people = new ParkPeople( park );

		_made.Add( people );

		return people;
	}

	[TestInitialize]
	public void MountTheGame()
	{
		_screen = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );

		Log ??= new();
		data = FileSystem = GameData.Required();
		Input.Mouse = new();
		Input.ForgetHeldKeys();
		Unimplemented.Forget();
		ParkCategoryScreens.Seed();

		using var stream = new MemoryStream( data.ReadAllBytes( "levels/jungle/Easymode.TPWI" ) );
		var reader = new SaveReader( stream );
		var payload = reader.ReadFile();

		_shipped = new ParkWorld( payload, reader.Preamble );
	}

	[TestCleanup]
	public void PutItAllBack()
	{
		foreach ( var entity in _made )
			entity.Delete();

		_made.Clear();
		Entity.ApplyDeletions();

		ParkCategoryScreens.Seed();
		Input.Mouse = new();
		Input.ForgetHeldKeys();
		Unimplemented.Forget();
		Screen.Size = _screen;
	}

	/// <summary>
	/// <b>A row a kind, each a spinner over nought to 10000 in steps of 25 on that kind's budget</b>, in the order
	/// handymen, mechanics, entertainers, guards, researchers down the screen; the budget is the park's as the
	/// button is let go; a budget over nought sets <c>mHaveEverTrained</c>, which a budget back at nought leaves
	/// set. Each row's bar is its kind's grades as a whole per cent of the top grade (the shipped staff: four of
	/// grade 3, a researcher of grade 2). The staff costs row holds the five budgets added up, from the opening on.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> a step of 1, or of 50; the plus and the minus swapped; a row writing its neighbour's
	/// budget; the rows in the stream's order; no lower end; no upper end, or one of 9999; the flag set by any
	/// change; the flag cleared at nought; the staff costs row one budget and not the sum, or empty as the screen
	/// opens; the bars all full, not drawn, or the grades over 4; the figure lettered without its "$ ".
	/// </remarks>
	[TestMethod]
	public void EachRowsSpinnerSetsItsKindsBudgetInStepsOfTwentyFiveFromNoughtToTenThousand()
	{
		var people = People( _shipped );
		var stack = new WindowStack();
		var screen = new ParkStaffCostsScreen( stack );

		stack.Open( screen );

		Assert.AreEqual( "budgets $ 0,$ 0,$ 0,$ 0,$ 0 skill 60,60,60,60,40 staffcosts $ 0 loans off account $ 0 / $ 0 / $ 0 / $ 0", screen.Census() );
		Assert.IsFalse( people.HaveEverTrained, "five budgets of nought written back train nobody" );

		// A minus at nought.
		Assert.IsTrue( stack.ClickAt( Minus, RowMiddles[1] ) );
		CollectionAssert.AreEqual( new[] { 0, 0, 0, 0, 0 }, people.TrainingBudgets );
		Assert.IsFalse( people.HaveEverTrained );

		for ( var kind = 0; kind < 5; ++kind )
		{
			for ( var press = 0; press <= kind; ++press )
				Assert.IsTrue( stack.ClickAt( Plus, RowMiddles[kind] ) );
		}

		CollectionAssert.AreEqual( new[] { 25, 50, 75, 100, 125 }, people.TrainingBudgets, "a row a kind, top to bottom" );
		Assert.IsTrue( people.HaveEverTrained );
		Assert.AreEqual( "budgets $ 25,$ 50,$ 75,$ 100,$ 125 skill 60,60,60,60,40 staffcosts $ 375 loans off account $ 0 / $ 375 / $ 0 / -$ 375", screen.Census() );

		stack.ClickAt( Minus, RowMiddles[0] );
		stack.ClickAt( Minus, RowMiddles[0] );
		stack.ClickAt( Minus, RowMiddles[4] );

		CollectionAssert.AreEqual( new[] { 0, 50, 75, 100, 100 }, people.TrainingBudgets );
		Assert.IsTrue( people.HaveEverTrained, "nothing clears it" );
		Assert.IsTrue( screen.Census().Contains( "staffcosts $ 325 " ) );

		// The upper end.
		people.SetTrainingBudget( 2, ParkStaffCostsScreen.Highest - 25 );
		stack.ClickAt( Plus, RowMiddles[2] );
		Assert.AreEqual( 10000, people.TrainingBudgets[2] );
		stack.ClickAt( Plus, RowMiddles[2] );
		Assert.AreEqual( 10000, people.TrainingBudgets[2] );

		stack.Update();
		Assert.IsTrue( screen.Census().StartsWith( "budgets $ 0,$ 50,$ 10000,$ 100,$ 100 " ) );
	}

	/// <summary>
	/// <b>Opening the screen is a change of every budget</b>: a file whose budgets are over nought and whose
	/// <c>mHaveEverTrained</c> is not set has it set as the screen opens, the budgets as they were, and the staff
	/// costs row holds them added up (the original given <c>q351/bud2.TPWS</c>: thing 1's <c>+0x20</c> 0 to 1 on
	/// the tick the screen came up, "$ 75" in the row).
	/// </summary>
	/// <remarks><b>Mutations:</b> the opening writing nothing back; writing noughts; the row empty at the opening; the row one budget.</remarks>
	[TestMethod]
	public void OpeningTheScreenWritesEveryBudgetBackAndSoSetsEverTrained()
	{
		var body = ParkFileWriter.Body( _shipped, new ParkFileWriter.Running( _shipped.GameTick, false, 13, 0, _shipped.Camera.Saved!.Value,
			Training: (new[] { 25, 0, 50, 0, 0 }, false) ) );
		var people = People( new ParkWorld( body ) );

		Assert.IsFalse( people.HaveEverTrained );

		var screen = new ParkStaffCostsScreen( new WindowStack() );

		Assert.IsTrue( people.HaveEverTrained );
		CollectionAssert.AreEqual( new[] { 25, 0, 50, 0, 0 }, people.TrainingBudgets );
		Assert.AreEqual( "budgets $ 25,$ 0,$ 50,$ 0,$ 0 skill 60,60,60,60,40 staffcosts $ 75 loans off account $ 0 / $ 75 / $ 0 / -$ 75", screen.Census() );
	}

	/// <summary>
	/// <b>The handler's own write</b> (<c>0x004b2534</c>): a value below nought is nought, and only a value over nought
	/// sets <c>mHaveEverTrained</c>.
	/// </summary>
	/// <remarks><b>Mutations:</b> a value below nought kept; nought setting the flag; a value below nought setting it.</remarks>
	[TestMethod]
	public void ABudgetBelowNoughtIsNoughtAndOnlyOneOverNoughtIsEverTrained()
	{
		var people = People( _shipped );

		people.SetTrainingBudget( 3, -25 );
		Assert.AreEqual( (0, false), (people.TrainingBudgets[3], people.HaveEverTrained) );

		people.SetTrainingBudget( 3, 0 );
		Assert.IsFalse( people.HaveEverTrained );

		people.SetTrainingBudget( 3, 25 );
		Assert.AreEqual( (25, true), (people.TrainingBudgets[3], people.HaveEverTrained) );
	}

	/// <summary>
	/// <b>The Money category's screen 9 is this screen</b>: its opener leaves the category on 9 and does nothing with
	/// its screen up; its three buttons go to the entry price, the finances and the loans, the loans' disabled in
	/// Instant Action; the entry-price screen's staff-costs button comes back; and the OK button closes it.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> screen 9 counted as unbuilt; a second screen built over the first; the category not left on
	/// 9; the loans button enabled in Instant Action; the entry button going to the finances; the OK button dead.
	/// </remarks>
	[TestMethod]
	public void TheMoneyCategorysNinthScreenIsTheStaffTrainingBudgets()
	{
		_ = People( _shipped );
		var stack = new WindowStack();

		ParkCategoryScreens.Show( stack, ParkCategoryScreens.Finance, ParkCategoryScreens.StaffCosts );

		var screen = stack.Windows.OfType<ParkStaffCostsScreen>().Single();

		Assert.AreEqual( 9, ParkCategoryScreens.LeftOn( ParkCategoryScreens.Finance ) );
		Assert.AreEqual( 0, Times( "STAFF_COSTS_SCREEN" ) );
		Assert.AreEqual( 0, Times( "MONTHLY_ACCOUNT_HISTORY" ), "the month's account is built" );

		ParkCategoryScreens.Show( stack, ParkCategoryScreens.Finance, ParkCategoryScreens.StaffCosts );
		Assert.AreSame( screen, stack.Windows.OfType<ParkStaffCostsScreen>().Single(), "nothing with its screen up" );

		// The loans button, disabled in Instant Action: the click is nobody's.
		Assert.IsTrue( Level.InstantAction );
		stack.ClickAt( 1739, 411 );
		Assert.AreSame( screen, stack.Windows.Single() );
		Assert.AreEqual( 0, Times( "LOANS_SCREEN" ) );

		// b_entry, and the entry-price screen's b_staffcost back.
		stack.ClickAt( 1739, 626 );
		Assert.IsTrue( stack.Windows.Single() is ParkEntryPriceScreen );
		Assert.AreEqual( 10, ParkCategoryScreens.LeftOn( ParkCategoryScreens.Finance ) );

		stack.ClickAt( 1619, 354 );
		Assert.IsTrue( stack.Windows.Single() is ParkStaffCostsScreen );
		Assert.AreEqual( 9, ParkCategoryScreens.LeftOn( ParkCategoryScreens.Finance ) );

		// b_finance: the finances, which are not built and are counted.
		stack.ClickAt( 1739, 518 );
		Assert.AreEqual( (0, 1, 7), (stack.Windows.Count, Times( "FINANCE_SCREEN" ), ParkCategoryScreens.LeftOn( ParkCategoryScreens.Finance )) );

		ParkCategoryScreens.Open( stack, ParkCategoryScreens.Finance );
		Assert.AreEqual( 2, Times( "FINANCE_SCREEN" ) );

		ParkCategoryScreens.Show( stack, ParkCategoryScreens.Finance, ParkCategoryScreens.StaffCosts );
		stack.ClickAt( 1752, 861 );
		Assert.AreEqual( 0, stack.Windows.Count, "b_okay" );
	}

	/// <summary>
	/// <b>A written park holds the five budgets and <c>mHaveEverTrained</c></b>: five dwords at 82 of the staff HQ's
	/// record and the byte at 102, the file's own where the writer is handed none (FileFormats <c>saves.md</c>,
	/// "The staff HQ"). The park that loads the file trains with them.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the budgets not written; written a kind out of place; the flag not written, or always set;
	/// the flag not read; the writer handed nothing still writing noughts.
	/// </remarks>
	[TestMethod]
	public void AWrittenParkHoldsTheTrainingBudgetsAndWhetherOneWasEverSet()
	{
		Assert.IsTrue( _shipped.StaffHqAt > 0 );
		Assert.AreEqual( false, _shipped.StaffHq!.Value.HaveEverTrained );

		var running = new ParkFileWriter.Running( _shipped.GameTick, false, 13, 0, _shipped.Camera.Saved!.Value,
			Training: (new[] { 25, 75, 0, 10000, 50 }, true) );

		var body = ParkFileWriter.Body( _shipped, running );
		var at = _shipped.StaffHqAt;

		CollectionAssert.AreEqual( new[] { 25, 75, 0, 10000, 50 },
			Enumerable.Range( 0, 5 ).Select( kind => System.BitConverter.ToInt32( body, at + 82 + (4 * kind) ) ).ToArray() );
		Assert.AreEqual( 1, body[at + 102] );

		var written = new ParkWorld( body );

		Assert.IsNull( written.Problem );
		CollectionAssert.AreEqual( new[] { 25, 75, 0, 10000, 50 }, written.StaffHq!.Value.Budgets.ToArray() );
		Assert.IsTrue( written.StaffHq.Value.HaveEverTrained );

		var people = People( written );

		CollectionAssert.AreEqual( new[] { 25, 75, 0, 10000, 50 }, people.TrainingBudgets );
		Assert.IsTrue( people.HaveEverTrained );

		// A budget back at nought with the flag set, and the flag alone.
		var cleared = new ParkWorld( ParkFileWriter.Body( written, running with { Training = (new int[5], true) } ) );

		CollectionAssert.AreEqual( new int[5], cleared.StaffHq!.Value.Budgets.ToArray() );
		Assert.IsTrue( cleared.StaffHq.Value.HaveEverTrained );

		var unset = new ParkWorld( ParkFileWriter.Body( written, running with { Training = (new[] { 25, 75, 0, 10000, 50 }, false) } ) );

		Assert.IsFalse( unset.StaffHq!.Value.HaveEverTrained );

		// Handed nothing, the writer leaves the file's.
		var carried = new ParkWorld( ParkFileWriter.Body( written, running with { Training = null } ) );

		CollectionAssert.AreEqual( new[] { 25, 75, 0, 10000, 50 }, carried.StaffHq!.Value.Budgets.ToArray() );
		Assert.IsTrue( carried.StaffHq.Value.HaveEverTrained );
	}

	private static int Times( string what ) => Unimplemented.Summary.FirstOrDefault( entry => entry.What == what ).Times;
}
