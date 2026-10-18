using Microsoft.VisualStudio.TestTools.UnitTesting;

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
}
