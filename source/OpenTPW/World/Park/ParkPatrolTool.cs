namespace OpenTPW;

/// <summary>
/// The patrol tool, build mode <c>0x36</c>: what the staff member's window hands the player as its patrol switch goes
/// down (<c>FUN_004b5b50( 1 )</c>), so that two clicks on the map fence that member into a rectangle.
///
/// <para>
/// <b>The preview</b> (<c>FUN_005234d0</c>'s <c>0x36</c> arm): with nothing anchored one square under the pointer; anchored,
/// the rectangle's outline from the anchor to the pointer, in the same four lines a followed member's patrol edges are
/// laid in (<see cref="UI.ParkLocator.PatrolEdgeCells"/>). The tool's verdict for every square (<c>FUN_00535670( 0x36 )</c>)
/// is the refusal latch alone, which the rectangle's test sets (<see cref="Refuses"/>), so the outline is all blue or all
/// red. An unanchored square is always blue.
/// </para>
///
/// <para>
/// <b>A click</b> (<c>FUN_00524960</c>'s <c>0x36</c> arm, on the release): a red outline puts the tool away with sound
/// <c>0xaf</c> (<see cref="UI.UiSounds.Refused"/>) and leaves the area as it was; the first click anchors; the second writes the rectangle's two corners to
/// the member (<c>FUN_00506e50</c>: the least corner's cell id to <c>+0x20a</c>, the greatest's to <c>+0x20c</c>) and puts
/// the tool away. Neither click plays a sound.
/// </para>
/// </summary>
public static class ParkPatrolTool
{
	/// <summary>The squares the tool shows for the pointer's cell.</summary>
	public static List<ParkPathBuilding.QueueSquare> Strip( int toX, int toY ) => Strip( toX, toY, out _ );

	/// <summary>
	/// The squares the tool shows for the pointer's cell, and whether its pass leaves the refusal latch
	/// <c>DAT_00816d50</c> set: the rectangle's test sets it (<c>0x00523dd3</c>), and unanchored nothing does.
	/// </summary>
	public static List<ParkPathBuilding.QueueSquare> Strip( int toX, int toY, out bool latched )
	{
		latched = false;

		if ( !ParkState.OnMap( toX, toY ) )
			return [];

		if ( !ParkBuildMode.Anchored )
			return [new( toX, toY, ParkPathBuilding.MarkerBlue )];

		var (from, to) = Corners( ParkBuildMode.Anchor, (toX, toY) );
		var refused = Level.Current?.Park is { } park && Refuses( park, from, to );
		latched = refused;
		var marker = refused ? ParkPathBuilding.MarkerRed : ParkPathBuilding.MarkerBlue;
		var why = refused ? "takes in no cell of the park" : null;

		return UI.ParkLocator.PatrolEdgeCells( from, to )
			.Select( cell => new ParkPathBuilding.QueueSquare( cell.X, cell.Y, marker, why ) )
			.ToList();
	}

	/// <summary>The rectangle between two cells, its least corner first, as the preview and the click both take it.</summary>
	public static ((int X, int Y) From, (int X, int Y) To) Corners( (int X, int Y) a, (int X, int Y) b )
		=> ((Math.Min( a.X, b.X ), Math.Min( a.Y, b.Y )), (Math.Max( a.X, b.X ), Math.Max( a.Y, b.Y )));

	/// <summary>
	/// The rectangle's test, <c>FUN_00532a50</c>: refused where every cell of it on the map is outside the park
	/// (<see cref="ParkPathBuilding.OutsideThePark"/>) or answers track type <c>0x19</c> (its own track record, or its
	/// parent's for a type 12 or 17).
	/// </summary>
	public static bool Refuses( IParkInitialState park, (int X, int Y) from, (int X, int Y) to )
	{
		var counted = 0;
		var barred = 0;

		for ( var y = from.Y; y <= to.Y; ++y )
		{
			for ( var x = from.X; x <= to.X; ++x )
			{
				if ( !ParkState.OnMap( x, y ) )
					continue;

				var cell = ParkState.CellFor( park, x, y );

				if ( ParkPathBuilding.TrackRecord( park, cell ).TrackType == 0x19 || (cell.Flags & ParkPathBuilding.OutsideThePark) != 0 )
					++barred;

				++counted;
			}
		}

		return barred == counted;
	}

	/// <summary>One click of the tool, at the cell under the pointer.</summary>
	/// <param name="pressReachedTheTool">
	/// Whether the press went to the park rather than to an open park screen (<see cref="ParkBuildMode.RefusedSound"/>).
	/// </param>
	public static string Run( int clickX, int clickY, bool pressReachedTheTool = true )
	{
		if ( Level.Current?.Park is not { } park )
			return "patrol: a park has to be loaded";

		var member = ParkBuildMode.Serves;

		if ( !ParkBuildMode.Anchored )
		{
			ParkBuildMode.AnchorAt( clickX, clickY );

			return $"patrol: anchored at ({clickX},{clickY}) for thing {member} - click the far corner";
		}

		var (from, to) = Corners( ParkBuildMode.Anchor, (clickX, clickY) );
		ParkBuildMode.Disarm();

		if ( Refuses( park, from, to ) )
		{
			var sound = ParkBuildMode.RefusedSound( pressReachedTheTool );

			return $"patrol: {from}-{to} takes in no cell of the park, so no area is written and the patrol tool is put away - {sound}";
		}

		if ( ParkPeople.Current?.Staff.FirstOrDefault( staff => staff.ThingId == member ) is not { } fenced )
			return $"patrol: thing {member} is not on the staff, so no area is written and the patrol tool is put away";

		fenced.SetPatrolArea( MapStep.CellId( from.X, from.Y ), MapStep.CellId( to.X, to.Y ) );

		return $"patrol: thing {member} patrols {from}-{to}, cells {fenced.PatrolBottomLeft} to {fenced.PatrolTopRight} - the patrol tool is put away";
	}
}
