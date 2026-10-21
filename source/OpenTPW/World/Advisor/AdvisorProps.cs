namespace OpenTPW;

/// <summary>
/// What the advisor wears for a line: the two props its response names (the response table's <c>+0x14</c> and
/// <c>+0x18</c>), put on after each clip of the line has hidden the pieces its hide list names
/// (<c>FUN_00598dc0</c>; <c>docs/exe/advisor-park.md</c>, "What he wears for a line"). A prop is a costume id of
/// the line's model, found in the lookup space <c>0x400</c>: 5 is the hard hat, 9 the ticket hat, and -1 the
/// theme's own costume, whose pieces the theme's <c>Standard.sam</c> names.
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

	/// <summary>How many pairs of pieces a theme's costume holds (<c>FUN_004080e0</c> copies four).</summary>
	internal const int CostumePairs = 4;

	/// <summary>
	/// A theme's costume: the pairs <c>ThemeAdvisorCostumes[0]</c> to <c>[3]</c> of the park's balance, each an
	/// <c>AddOns</c> piece to show and a <c>SubOffs</c> piece to hide, nought where the pair names none.
	/// </summary>
	internal static (int AddOn, int SubOff)[] CostumeOf( Func<string, int> key )
	{
		var pairs = new (int, int)[CostumePairs];

		for ( var pair = 0; pair < CostumePairs; ++pair )
			pairs[pair] = (key( $"ThemeAdvisorCostumes[{pair}].AddOns" ), key( $"ThemeAdvisorCostumes[{pair}].SubOffs" ));

		return pairs;
	}

	/// <summary>
	/// The pieces one prop switches, in the engine's order (<c>FUN_00598ca0</c>): nought shows his antennae;
	/// a hat that covers them hides them; the spatula, 14, hides the hand that holds it; and any prop but
	/// nought is then shown itself. The theme's costume, -1, shows each pair's add-on and hides its sub-off,
	/// pair by pair; the piece -1 the engine then asks for is no model's. With <paramref name="addOnsHidden"/>, the clip
	/// word's <c>0x10000</c>, neither a costume's add-on nor the prop itself is shown; what a prop hides is still hidden.
	/// </summary>
	internal static IEnumerable<(int Id, bool Shown)> Switched( int prop, IReadOnlyList<(int AddOn, int SubOff)>? costume = null,
		bool addOnsHidden = false )
	{
		switch ( prop )
		{
			case 0:
				yield return (LeftAntenna, true);
				yield return (RightAntenna, true);
				yield break;

			case ThemeCostume:
				foreach ( var (addOn, subOff) in costume ?? [] )
				{
					if ( addOn != 0 && !addOnsHidden )
						yield return (addOn, true);

					if ( subOff != 0 )
						yield return (subOff, false);
				}

				yield break;

			case 1 or 2 or 4 or 5 or 6 or 7 or 9 or 10 or 11 or 13:
				yield return (LeftAntenna, false);
				yield return (RightAntenna, false);
				break;

			case 14:
				yield return (RightHand, false);
				break;
		}

		if ( !addOnsHidden )
			yield return (prop, true);
	}

	/// <summary>
	/// Dresses <paramref name="model"/> for a line: every node of <paramref name="hidden"/> hidden, the hide list
	/// of the clip the line starts on, then <paramref name="first"/> and, where it is not nought,
	/// <paramref name="second"/> put on. Parts in no hide list stay as the last line left them unless a prop
	/// switches them. <paramref name="show"/> is handed each node switched.
	/// </summary>
	internal static void Wear( ModelFile model, IEnumerable<ushort> hidden, int first, int second,
		IReadOnlyList<(int AddOn, int SubOff)>? costume, Action<int, bool> show, bool addOnsHidden = false )
	{
		foreach ( var node in hidden )
			show( node, false );

		Put( model, first, costume, show, addOnsHidden );

		if ( second != 0 )
			Put( model, second, costume, show, addOnsHidden );
	}

	private static void Put( ModelFile model, int prop, IReadOnlyList<(int AddOn, int SubOff)>? costume, Action<int, bool> show,
		bool addOnsHidden )
	{
		foreach ( var (id, shown) in Switched( prop, costume, addOnsHidden ) )
		{
			var node = model.FindNode( id, CostumeSpace );

			if ( node >= 0 )
				show( node, shown );
		}
	}
}
