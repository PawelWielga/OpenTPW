namespace OpenTPW;

/// <summary>
/// Which items the running park has researched, and to which upgrade tier - the researched flag and tier of each
/// of <c>CControlManager</c>'s records (<c>+0x10</c>, <c>+0x14</c>), which the buy list asks before it lists an item
/// (<c>0x004ab023</c>; <c>docs/exe/hud.md</c>, "What the buy list actually filters on").
///
/// <para>
/// <b>At a park load the save's word is the flag</b>: world setup seeds every catalogued item researched when its
/// <c>Upgrades[0].CostOfResearch</c> is nought (<c>0x004d3e92</c>), the save's <c>mObjectControls</c> is laid over
/// all of it (<c>0x005181e7</c>), and an item the save has no record of is seeded again (<c>FUN_00415140</c>). So an
/// item the save holds keeps the save's flag, whatever its file's cost.
/// </para>
///
/// <para>
/// <b>Research completing is not built</b> (<c>FUN_00504630</c>, which sets the flag and the tier): this game has no
/// research, and nothing here changes after the load or after a fresh world is made (<see cref="FreshLab"/>), the lab's projects (<see cref="Unfinished"/>) and its kinds' outputs (<see cref="Rate"/>) included. The points a researcher would hand the lab are counted where
/// the original hands them, <c>RESEARCH_POINTS_TO_THE_LAB</c> in <see cref="StaffBehaviour"/>.
/// </para>
/// </summary>
public sealed class ParkResearch
{
	private readonly Dictionary<int, (bool Researched, int Tier)> _items = [];

	/// <summary>How many items came from the save's records, and how many were seeded from their files.</summary>
	public int FromSave { get; }

	/// <inheritdoc cref="FromSave"/>
	public int Seeded { get; }

	private readonly IReadOnlyList<ParkWorld.ResearchProject> _projects;

	/// <param name="records">The save's records, <see cref="ParkWorld.ObjectControlRecords"/>.</param>
	private readonly IReadOnlyList<ParkWorld.ResearchKind> _kinds;

	/// <param name="projects">The save's lab's projects, <see cref="ParkWorld.ResearchProjects"/>; none where no file gave any.</param>
	/// <param name="kinds">The save's lab's kinds' records, <see cref="ParkWorld.ResearchKinds"/>; none where no file gave any.</param>
	public ParkResearch( IEnumerable<ParkWorld.ObjectControl> records, ParkItemCatalogue catalogue,
		IReadOnlyList<ParkWorld.ResearchProject>? projects = null, IReadOnlyList<ParkWorld.ResearchKind>? kinds = null )
	{
		_projects = projects ?? [];
		_kinds = kinds ?? [];

		foreach ( var record in records )
		{
			if ( _items.TryAdd( record.ItemId, (record.Researched, record.TierResearched) ) )
				++FromSave;
		}

		foreach ( var item in catalogue.All )
		{
			if ( _items.TryAdd( item.Id, (item.ResearchCost == 0, 0) ) )
				++Seeded;
		}

	}

	/// <summary>How many items are researched and where their flags came from, for the log.</summary>
	public string Summary => $"Research: {_items.Values.Count( entry => entry.Researched )} of {_items.Count} items researched, "
		+ $"{FromSave} from the save's records and {Seeded} seeded from their files";

	/// <summary>Whether the item is researched - the flag the buy list lists by. An item nobody knows is not.</summary>
	public bool IsResearched( int itemId ) => _items.TryGetValue( itemId, out var entry ) && entry.Researched;

	/// <summary>
	/// How many of the lab's projects of <paramref name="kind"/>, a project's <c>mCat</c>, are not done: the
	/// points put to them are under their cost (<c>FUN_00503b30</c>). They are the file's, as nothing here
	/// researches.
	/// </summary>
	public int Unfinished( int kind ) => _projects.Count( project => project.Category == kind && project.Done < project.Cost );

	/// <summary>
	/// How much of the lab's work a kind of research is getting from the researchers on the staff
	/// (<c>FUN_00503980</c>; <c>docs/exe/advisor-park.md</c>, "Every sideshow built, and more to research"): the
	/// kind's output times the share, in whole per cent, that <paramref name="ability"/>, the staff's researchers'
	/// <c>ResearchAbility</c> added up, is of <paramref name="most"/>, the most researchers a park may employ
	/// times the top grade's ability, over a hundred; each division a whole number's. Nought with no such most.
	/// The outputs are the file's or the fresh world's, as no screen here changes a kind's effort.
	/// </summary>
	public int Rate( int kind, int ability, int most )
		=> most == 0 || kind < 0 || kind >= _kinds.Count ? 0
			: (int)((uint)_kinds[kind].Output * ((uint)(ability * 100) / (uint)most) / 100);

	/// <summary>
	/// A fresh world's five kinds' records (<c>FUN_00503330</c>): each kind's effort is its
	/// <c>ResearchCategories[kind].Effort</c> held to 100, and its output that effort's share of the five in whole
	/// per cent, times <paramref name="workLoad"/>, <c>Research.StartingWorkLoad</c>, over a hundred; nought
	/// where the efforts add up to nothing.
	/// </summary>
	public static IReadOnlyList<ParkWorld.ResearchKind> FreshKinds( IEnumerable<int> efforts, int workLoad )
	{
		var held = efforts.Select( effort => (uint)effort > 99 ? 100u : (uint)effort ).ToArray();
		var sum = (uint)held.Sum( effort => effort );

		return held.Select( effort => new ParkWorld.ResearchKind( (int)effort,
			sum == 0 ? 0 : (int)(effort * 100 / sum * (uint)workLoad / 100) ) ).ToArray();
	}

	/// <summary>The kind of research an upgrade's project is, whatever its item's own (<c>0x005045fc</c>).</summary>
	public const int Upgrade = 4;

	/// <summary>
	/// The lab of a world just made, and the upgrade tier each item's control record is left at: world setup's fill
	/// (<c>FUN_005031a0</c>; <c>docs/exe/park-engine.md</c>, "A fresh world's research lab"). Every item a buy list
	/// can show (<c>Info.WhichUIType</c> under 4), in the catalogue's order, gives at most one project: its own
	/// research where that costs anything, and otherwise the first later tier whose research costs anything, as a
	/// project of kind 4. A tier costing nothing is passed over and counted researched (<c>FUN_00504580</c>), so an
	/// item with no upgrade is left at tier 2; an item of kind 4 has no upgrade and is left at 0.
	/// </summary>
	public static (IReadOnlyList<ParkWorld.ResearchProject> Projects, IReadOnlyDictionary<int, int> Tiers) FreshLab(
		IEnumerable<ParkItemCatalogue.Item> items )
	{
		var projects = new List<ParkWorld.ResearchProject>();
		var tiers = new Dictionary<int, int>();

		foreach ( var item in items.Where( item => item.UiType < Upgrade ) )
		{
			if ( item.ResearchCost != 0 )
			{
				projects.Add( new( item.Id, item.ResearchCategory, item.ResearchGroup, 0, item.ResearchCost, 0 ) );
				continue;
			}

			if ( item.ResearchCategory == Upgrade )
				continue;

			int[] later = [item.UpgradeResearchCost1, item.UpgradeResearchCost2];
			var next = Array.FindIndex( later, cost => cost > 0 );
			tiers[item.Id] = next < 0 ? later.Length : next;

			if ( next >= 0 )
				projects.Add( new( item.Id, Upgrade, 0, next + 1, later[next], 0 ) );
		}

		return (projects, tiers);
	}

	/// <summary>The upgrade tier researched, 0 to 2 (<c>+0x14</c>); nothing reads it yet.</summary>
	public int TierResearched( int itemId ) => _items.TryGetValue( itemId, out var entry ) ? entry.Tier : 0;
}
