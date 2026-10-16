namespace OpenTPW.UI;

/// <summary>
/// The research opener, the original's <c>FUN_004aa480</c>: what the gadget's Research button (<c>0x2b</c>), R (the
/// shortcuts' row 11) and the message bar's button of type <c>0x13</c> all do.
///
/// <para>
/// With the game menu up it does nothing; <see cref="UiWindow.Modal"/> keeps all three callers' presses away from
/// it there, as for the category buttons (<see cref="ParkCategoryScreens"/>). Otherwise it first closes the park
/// screen that is open (<c>FUN_00485b40</c>), then in Instant Action, game type 2, opens a message box reading UITEXT
/// <c>0x1d4</c>, "Research is automatic in Instant Action mode.", and with no researcher on the staff
/// (<c>mFirstResearcher</c>, <c>+0x1da742</c>, nought) one reading UITEXT <c>0x1d3</c>. Each is the plain box in
/// white in font 4 with its cross in the bottom place and nothing else (<c>MessageBox_Open( text, 2, 0, 0, ... )</c>).
/// Past both it builds the research screen, six effort sliders, which is not built here: counted,
/// <c>RESEARCH_SCREEN</c> (<c>docs/exe/hud.md</c>, "The nine screens behind Info, Money and Research").
/// </para>
/// </summary>
internal static class ParkResearchOpener
{
	/// <summary>Opens what the original's opener would for the running park and the player's game.</summary>
	public static void Open( WindowStack stack )
		=> Open( stack, Level.InstantAction, ParkPeople.Current?.ScoreStaff.Researchers ?? 0 );

	/// <param name="instantAction">Whether the game is Instant Action, the original's game type 2.</param>
	/// <param name="researchers">The researchers on the staff; only whether there is one matters.</param>
	internal static void Open( WindowStack stack, bool instantAction, int researchers )
	{
		stack.CloseParkScreen();

		if ( instantAction )
		{
			Log.Info( "Park: research opener: Instant Action, so the box of UITEXT 0x1d4" );
			stack.Open( new MessageBox( stack, Localization.Get( UIStrings.ResearchInstantAction ), null ) );
			return;
		}

		if ( researchers == 0 )
		{
			Log.Info( "Park: research opener: no researcher on the staff, so the box of UITEXT 0x1d3" );
			stack.Open( new MessageBox( stack, Localization.Get( UIStrings.NeedToHireScientists ), null ) );
			return;
		}

		Unimplemented.Report( "RESEARCH_SCREEN" );
		Log.Info( $"Park: research opener: {researchers} researcher(s), so the research screen, which is not built" );
	}
}
