namespace OpenTPW;

/// <summary>
/// What the advisor wears for a line: the two props its response names (the response table's <c>+0x14</c> and
/// <c>+0x18</c>), put on after each clip of the line has hidden his add-on pieces (<c>FUN_00598dc0</c>;
/// <c>docs/exe/advisor-park.md</c>, "What he wears for a line"). A prop is a costume id of his model, found in the
/// lookup space <c>0x400</c>: 5 is the hard hat, 9 the ticket hat.
/// </summary>
internal static class AdvisorProps
{
	/// <summary>The lookup space a costume piece is found in (<c>FUN_00429ee0</c>, <c>FUN_00429f60</c>).</summary>
	internal const uint CostumeSpace = 0x400;

	/// <summary>The lowest costume id that is part of him rather than something he puts on.</summary>
	internal const int FirstOwnPart = 19;

	internal const int LeftAntenna = 19;
	internal const int RightAntenna = 20;
	internal const int RightHand = 21;

	/// <summary>The prop that asks for the theme's own costume, <c>ThemeAdvisorCostumes</c>.</summary>
	internal const int ThemeCostume = -1;

	/// <summary>
	/// The pieces one prop switches, in the engine's order (<c>FUN_00598ca0</c>): nought shows his antennae;
	/// a hat that covers them hides them; the spatula, 14, hides the hand that holds it; and any prop but
	/// nought is then shown itself. The theme's costume, -1, switches nothing here.
	/// </summary>
	internal static IEnumerable<(int Id, bool Shown)> Switched( int prop )
	{
		switch ( prop )
		{
			case 0:
				yield return (LeftAntenna, true);
				yield return (RightAntenna, true);
				yield break;

			case ThemeCostume:
				yield break;

			case 1 or 2 or 4 or 5 or 6 or 7 or 9 or 10 or 11 or 13:
				yield return (LeftAntenna, false);
				yield return (RightAntenna, false);
				break;

			case 14:
				yield return (RightHand, false);
				break;
		}

		yield return (prop, true);
	}

	/// <summary>
	/// Dresses <paramref name="model"/> for a line: every add-on piece hidden, as each of his clips' hide lists
	/// hides them, then <paramref name="first"/> and, where it is not nought, <paramref name="second"/> put on.
	/// His own parts stay as the last line left them unless a prop switches them. <paramref name="show"/> is
	/// handed each node switched.
	/// </summary>
	internal static void Wear( ModelFile model, int first, int second, Action<int, bool> show )
	{
		for ( var node = 0; node < model.Nodes.Count; ++node )
		{
			if ( model.Nodes[node].Id is > 0 and < FirstOwnPart )
				show( node, false );
		}

		Put( model, first, show );

		if ( second != 0 )
			Put( model, second, show );
	}

	private static void Put( ModelFile model, int prop, Action<int, bool> show )
	{
		// The theme's costume is worn by the park's own model, which is not built.
		if ( prop == ThemeCostume )
			Unimplemented.Report( "ADVISOR_THEME_COSTUME" );

		foreach ( var (id, shown) in Switched( prop ) )
		{
			var node = model.FindNode( id, CostumeSpace );

			if ( node >= 0 )
				show( node, shown );
		}
	}
}
