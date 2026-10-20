using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The research opener, <c>FUN_004aa480</c> (<see cref="UI.ParkResearchOpener"/>): the park screen that is open closed
/// first, then in Instant Action the box of UITEXT <c>0x1d4</c>, with no researcher the box of UITEXT <c>0x1d3</c>, each
/// with its cross alone, else the research screen (<see cref="UI.ParkResearchScreen"/>). These read the interface's strings and meshes from the
/// shipped game and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkResearchOpenerTests
{
	/// <summary>UITEXT <c>0x1d4</c> as the shipped English table has it.</summary>
	internal static string InstantActionText => Localization.Get( UIStrings.ResearchInstantAction );

	private Point2 _screen;

	[TestInitialize]
	public void MountTheGame()
	{
		_screen = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );

		Log ??= new();
		FileSystem = GameData.Required();
	}

	[TestCleanup]
	public void PutTheScreenBack() => Screen.Size = _screen;

	/// <summary>
	/// <b>In Instant Action the opener closes the open park screen and opens the box of UITEXT <c>0x1d4</c></b>, whatever
	/// the staff: the plain dialog with the cross in the bottom place and no tick, which closes it and nothing more.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the park screen left open; the game type's test taken out or turned round; the two texts
	/// exchanged; the tick left on the box; the screen counted here.
	/// </remarks>
	[TestMethod]
	public void InInstantActionTheOpenerSaysResearchIsAutomatic()
	{
		var stack = new UI.WindowStack();
		var screen = new AScreen( stack );

		Assert.AreEqual( "Research is automatic in Instant Action mode.", InstantActionText, "UITEXT 0x1d4" );

		stack.Open( screen );
		UI.ParkResearchOpener.Open( stack, instantAction: true, researchers: 1 );

		Assert.IsFalse( stack.Windows.Contains( screen ), "the park screen closed first" );
		var box = (UI.MessageBox)stack.Windows.Single();
		Assert.AreEqual( InstantActionText, TextOf( box ) );
		CollectionAssert.AreEqual( new[] { "0x9873c8" }, Buttons( box ), "the cross alone, in the bottom place" );

		Assert.IsTrue( stack.ClickAt( 1428, 652 ), "the cross" );
		Assert.AreEqual( 0, stack.Windows.Count, "closed, and nothing more" );
	}

	/// <summary>
	/// <b>In Full Simulation with no researcher it opens the box of UITEXT <c>0x1d3</c></b>, the same box; with one it
	/// opens the research screen, the park screen closed the same, and with that screen up it does nothing more
	/// (<c>[0x007cc1d8]</c>).
	/// </summary>
	/// <remarks><b>Mutations:</b> the researchers' test taken out or turned round; the two texts exchanged; the screen
	/// left unopened; the test of a screen up taken out, which opens a second.</remarks>
	[TestMethod]
	public void InFullSimulationItAsksForScientistsOrOpensTheScreen()
	{
		var stack = new UI.WindowStack();

		Assert.AreEqual( "You need to hire some scientists before you can carry out any research!",
			Localization.Get( UIStrings.NeedToHireScientists ), "UITEXT 0x1d3" );

		UI.ParkResearchOpener.Open( stack, instantAction: false, researchers: 0 );
		var box = (UI.MessageBox)stack.Windows.Single();
		Assert.AreEqual( Localization.Get( UIStrings.NeedToHireScientists ), TextOf( box ) );
		CollectionAssert.AreEqual( new[] { "0x9873c8" }, Buttons( box ) );
		stack.Close( box );

		var screen = new AScreen( stack );
		stack.Open( screen );
		UI.ParkResearchOpener.Open( stack, instantAction: false, researchers: 2 );
		var research = stack.Windows.Single();
		Assert.IsInstanceOfType( research, typeof( UI.ParkResearchScreen ), "the park screen closed, the research screen open" );

		UI.ParkResearchOpener.Open( stack, instantAction: false, researchers: 2 );
		Assert.AreSame( research, stack.Windows.Single(), "with its screen up the opener does nothing" );
	}

	/// <summary>The message box's text, its dialog's (<c>0x9873c7</c>).</summary>
	internal static string? TextOf( UI.MessageBox box ) => Controls( box.Root ).Single( control => control.Id == 0x9873c7 ).Text;

	private static string[] Buttons( UI.MessageBox box )
		=> Controls( box.Root ).OfType<UI.UiButton>().Where( button => button.Visible ).Select( button => $"0x{button.Id:x}" ).ToArray();

	private static System.Collections.Generic.IEnumerable<UI.UiControl> Controls( UI.UiControl control )
		=> new[] { control }.Concat( control.Children.SelectMany( Controls ) );

	/// <summary>A bare park screen.</summary>
	private sealed class AScreen : UI.UiWindow
	{
		public AScreen( UI.WindowStack stack ) : base( stack )
		{
			Root = new UI.UiControl { Rect = new UI.UiRect( 600, 300, 1000, 600 ) };
			ParkScreen = true;
		}
	}
}
