using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OpenTPW.Tests;

/// <summary>
/// The delete box's words: UITEXT 399, whose parameter 4 the original fills with the player's name (0x004a6056,
/// <c>docs/exe/saves.md</c>, "Delete").
/// </summary>
[TestClass]
public class FrontEndDeletePlayerTests
{
	/// <summary>
	/// <b>The box names the player, then asks.</b> Row 399 is parameter 4 and then the question, so the name stands
	/// first and alone on its line.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> the name handed as any other parameter leaves the line empty; <c>Localization.Get</c> in place
	/// of <c>Format</c> reads the row's first text part, which is empty.
	/// </remarks>
	[TestMethod]
	public void TheDeleteBoxNamesThePlayerThenAsks()
	{
		FileSystem = GameData.Required();

		Assert.AreEqual( "MARGOT\n\nAre you sure you want to\ndelete this player ?", UI.FrontEnd.DeleteQuestion( "MARGOT" ) );
	}

	/// <summary>
	/// <b>The box is handed the name of the slot whose delete button was pressed</b>, not the folder's, an empty name or
	/// another slot's: the real <c>AskToDeletePlayer</c> over a roster with two named slots, and the text of the box it opens.
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> <c>""</c> in place of the slot's name leaves the first line empty; <c>Roster[0]</c> in place of
	/// <c>Roster[slot]</c> names the other player.
	/// </remarks>
	[TestMethod]
	public void TheDeleteBoxIsHandedTheSlotsName()
	{
		FileSystem = GameData.Required();
		Log ??= new();

		var slots = (Player?[])typeof( Players ).GetField( "_slots", BindingFlags.Instance | BindingFlags.NonPublic )!.GetValue( Players.Roster )!;
		var before = slots.ToArray();

		try
		{
			slots[0] = new Player( 0, "ALICE", new PlayerFile() );
			slots[2] = new Player( 2, "MARGOT", new PlayerFile() );

			var stack = (UI.WindowStack)RuntimeHelpers.GetUninitializedObject( typeof( UI.WindowStack ) );
			typeof( UI.WindowStack ).GetField( "_windows", BindingFlags.Instance | BindingFlags.NonPublic )!.SetValue( stack, new List<UI.UiWindow>() );

			var frontEnd = (UI.FrontEnd)RuntimeHelpers.GetUninitializedObject( typeof( UI.FrontEnd ) );
			typeof( UI.FrontEnd ).GetField( "_stack", BindingFlags.Instance | BindingFlags.NonPublic )!.SetValue( frontEnd, stack );

			frontEnd.AskToDeletePlayer( 2 );

			var box = stack.Windows.OfType<UI.MessageBox>().Single();
			Assert.AreEqual( UI.FrontEnd.DeleteQuestion( "MARGOT" ), TextOf( box.Root! ) );
		}
		finally
		{
			for ( var i = 0; i < slots.Length; i++ )
				slots[i] = before[i];
		}
	}

	private static string? TextOf( UI.UiControl control )
		=> control.Text ?? control.Children.Select( TextOf ).FirstOrDefault( text => text != null );
}
