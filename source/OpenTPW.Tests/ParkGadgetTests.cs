using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The park gadget's research button, <c>0x2b</c>, which runs the research opener (<c>FUN_004aa480</c>,
/// <c>docs/exe/hud.md</c>, "The nine screens behind Info, Money and Research"). These read the gadget's meshes from the
/// shipped game and are skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class ParkGadgetTests
{
	private Point2 screenBefore;

	[TestInitialize]
	public void MountTheGame()
	{
		// The interface's own 2048x1536, so a point in the window is the same point on the gadget's layout. Before the game
		// is required, so that a skip leaves the cleanup the real size to put back.
		screenBefore = Screen.Size;
		Screen.Size = new Point2( 2048, 1536 );

		Log ??= new();
		FileSystem = GameData.Required();
	}

	[TestCleanup]
	public void PutTheScreenBack() => Screen.Size = screenBefore;

	/// <summary>
	/// <b>Each click on the research button runs the research opener</b>, which in Instant Action is its box; a click on
	/// the gauge beside it, which takes the press and has nothing to do, opens none.
	/// </summary>
	/// <remarks><b>Mutations:</b> the opener taken out opens nothing; one on every release opens on the gauge's click.</remarks>
	[TestMethod]
	public void EachClickOnResearchRunsTheOpener()
	{
		var stack = AStack();
		stack.Open( new UI.ParkGadget( stack ) );
		Assert.IsTrue( Level.InstantAction, "no player: Instant Action" );
		Assert.IsTrue( stack.ClickAt( 333, 1313 ), "the middle of 0x2b's (274,1254)-(392,1372)" );
		Assert.AreEqual( ParkResearchOpenerTests.InstantActionText, ParkResearchOpenerTests.TextOf( (UI.MessageBox)stack.Windows[^1] ) );
		stack.Close( stack.Windows[^1] );

		Assert.IsTrue( stack.ClickAt( 333, 1313 ) );
		Assert.IsInstanceOfType( stack.Windows[^1], typeof( UI.MessageBox ), "every click, not the first alone" );
		stack.Close( stack.Windows[^1] );

		Assert.IsTrue( stack.ClickAt( 115, 1200 ), "the gauge, 0x1e" );
		CollectionAssert.AreEqual( new[] { "ParkGadget" }, stack.Windows.Select( window => window.GetType().Name ).ToArray(),
			"the gauge's click opens nothing" );
	}

	/// <summary>
	/// <b>The corner shows the keys held and the tickets not spent, the ticket hidden at none</b> (<c>0x004a1092</c>..):
	/// two jungle tickets and the first key, one spent, read "1 x" and "1 x" as the original's did before its award
	/// (<c>q706/orig/O/g-05.png</c>); both spent hide the ticket and its count, the keys staying.
	/// </summary>
	/// <remarks><b>Mutations:</b> the tickets earned shown; the spent added; the ticket left shown at none.</remarks>
	[TestMethod]
	public void TheCornerShowsTheKeysAndTheTicketsNotSpent()
	{
		var file = new PlayerFile { InstantAction = false, KeysGiven = 1, TicketsSpent = 1 };
		file.Park( "jungle" ).Tickets[0] = file.Park( "jungle" ).Tickets[2] = 1;
		var current = typeof( Players ).GetProperty( nameof( Players.Current ) )!;

		try
		{
			current.SetValue( Players.Roster, new Player( 0, "Test", file ) );
			var gadget = new UI.ParkGadget( AStack() );

			Assert.AreEqual( ("1 x", true, true), (Find( gadget.Root, 0x37 ).Text, Find( gadget.Root, 0x37 ).Visible, Find( gadget.Root, 0x35 ).Visible) );
			Assert.AreEqual( "1 x", Find( gadget.Root, 0x36 ).Text, "two tickets and the key given: one key" );

			file.TicketsSpent = 2;
			gadget = new UI.ParkGadget( AStack() );

			Assert.AreEqual( (false, false), (Find( gadget.Root, 0x37 ).Visible, Find( gadget.Root, 0x35 ).Visible), "none unspent: hidden" );
			Assert.AreEqual( "1 x", Find( gadget.Root, 0x36 ).Text );
		}
		finally
		{
			current.SetValue( Players.Roster, null );
		}
	}

	private static UI.UiControl Find( UI.UiControl control, int id )
		=> control.Id == id ? control : control.Children.Select( child => Find( child, id ) ).FirstOrDefault( found => found != null )!;

	/// <summary>A window stack with no windows, made without its constructor.</summary>
	private static UI.WindowStack AStack()
	{
		var stack = (UI.WindowStack)RuntimeHelpers.GetUninitializedObject( typeof( UI.WindowStack ) );

		typeof( UI.WindowStack ).GetField( "_windows", BindingFlags.Instance | BindingFlags.NonPublic )!
			.SetValue( stack, new List<UI.UiWindow>() );

		return stack;
	}
}
