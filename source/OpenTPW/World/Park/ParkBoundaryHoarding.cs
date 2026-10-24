using System.Numerics;

namespace OpenTPW;

/// <summary>
/// The wall between the park and the land not yet bought: one model for each anchor of the boundary's hoarding track
/// (track type 25), made again from the park file's model records as the original's loader makes every model
/// (<c>FUN_004647a0</c>; <c>docs/exe/park.md</c>, "The boundary hoarding's piece").
///
/// <para>
/// A piece's record carries its item, 17300 plus its row of the hoarding table (<c>FUN_00522e40</c>, <c>0x00522ea0</c>), the
/// cell it was made on and its angle. That cell is the anchor moved by the turn and the angle is 360 less the piece's own
/// (<c>FUN_00522f20</c>), so the record is stood as it is, by the rule every made model stands by
/// (<see cref="ParkObjects.OriginFor(int, int, int)"/>), at the height its two cells square give it (flags <c>0x33a</c> with
/// the footprint at <c>0x008179f8</c>, 2 by 2, every cell counted).
/// </para>
/// </summary>
public sealed class ParkBoundaryHoarding : Entity
{
	/// <summary>The item of the hoarding table's row 0; row r is item <c>FirstItem + r</c> (<c>0x4394</c>).</summary>
	public const int FirstItem = 17300;

	/// <summary>
	/// The hoarding table at <c>0x00763608</c>, row by row, as <c>FUN_00522e40</c> fills it from the theme's
	/// <c>hoarding.wad</c>: a row with no name takes the row before's model (<c>0x00522ed2</c>), so its own item is never
	/// made and a record names the named row above it.
	/// </summary>
	private static readonly string?[] Rows =
	[
		"ho_fos1", "ho_end1", "ho_fos1", "ho_fob1", "ho_fos1", null, null, null, null, "ho_fos4", null, null, "ho_fos4",
		null, null, "ho_fos4", null, null, "ho_fos4", "ho_fob2", "ho_fos2", null, null, "ho_fos3", null, null, "ho_fob3",
		"ho_fob4"
	];

	/// <summary>The cells a piece stands its height on: two across and two down, every one counted.</summary>
	private static readonly ItemShape Square = new( 2, 2, [[1, 1], [1, 1]] );

	/// <summary>The model a row draws, or null for a row outside the table.</summary>
	public static string? ModelOf( int row )
	{
		if ( row < 0 || row >= Rows.Length )
			return null;

		while ( Rows[row] == null )
			--row;

		return Rows[row];
	}

	/// <summary>One piece standing: its anchor's model, the cell and angle its record was made at.</summary>
	public readonly record struct Piece( string Model, int Row, int CellX, int CellY, int Angle );

	/// <summary>The boundary of the park currently loaded, or null outside one.</summary>
	public static ParkBoundaryHoarding? Current { get; private set; }

	/// <summary>The pieces standing, in the file's order.</summary>
	public IReadOnlyList<Piece> Pieces => _pieces;

	/// <summary>The boundary's anchors, the cells of track type 25.</summary>
	public int Anchors { get; }

	private readonly List<Piece> _pieces = [];
	private readonly List<LobbyModel> _models = [];
	private readonly string _theme;

	public ParkBoundaryHoarding( string themeName, IParkInitialState? park, ParkItemCatalogue? catalogue )
	{
		_theme = themeName.ToLowerInvariant();
		Name = $"{themeName} boundary hoarding";

		Anchors = AnchorsOf( park );

		var pieces = Read( park, catalogue );

		foreach ( var piece in pieces )
			Place( piece );

		// A park made fresh writes its anchors into the cells (FreshParkBoundary) with no model record; the original makes
		// each piece there from its neighbour mask (FUN_0053a540), which is not built.
		for ( var unmade = Anchors - pieces.Count; unmade > 0; --unmade )
			Unimplemented.Report( "BOUNDARY_HOARDING_UNMADE" );

		if ( Anchors > 0 )
			Log.Info( $"{themeName}: {_pieces.Count} boundary hoarding pieces stand on {Anchors} anchors" );

		Current = this;
	}

	/// <summary>The boundary's anchors in a park's cells: those of track type 25.</summary>
	public static int AnchorsOf( IParkInitialState? park ) => park?.Cells.Count( cell => cell.TrackType == 25 ) ?? 0;

	/// <summary>
	/// The pieces a park file's model records make, in the file's order: each record of an item from 17300 on, by its row's
	/// model, its cell and its angle. None where the park has no file or its model module does not read.
	/// </summary>
	public static List<Piece> Read( IParkInitialState? park, ParkItemCatalogue? catalogue )
	{
		var pieces = new List<Piece>();

		if ( park?.Save is not { } save )
			return pieces;

		var states = save.ThingStates( id => catalogue != null && catalogue.TryGet( id, out var item ) ? item.AnimationChannels : 1 );

		if ( states.Problem != null )
			return pieces;

		foreach ( var record in states.Things )
		{
			var row = record.CatalogueId - FirstItem;

			if ( row < 0 || row >= 100 )
				continue;

			// Items 17328 to 17399 are no row of the table, and no file to hand holds one.
			if ( ModelOf( row ) is not { } model )
			{
				Unimplemented.Report( "BOUNDARY_HOARDING_ROW" );
				continue;
			}

			pieces.Add( new Piece( model, row, record.CellX, record.CellY, record.Angle ) );
		}

		return pieces;
	}

	/// <summary>
	/// Where a piece's model stands and how it is turned: its record's cell and angle as every made model takes them
	/// (<see cref="ParkObjects.OriginFor(int, int, int)"/>), at the middle of the lowest and highest ground under its two
	/// cells square, or the ground at its cell where there is none to ask.
	/// </summary>
	internal static (Vector3 Origin, Quaternion Turn) StandOf( Piece piece, HeightfieldFile? field )
	{
		var origin = ParkObjects.OriginFor( piece.CellX, piece.CellY, piece.Angle, field );

		if ( field != null && ParkObjects.BaseHeight( Square, origin, piece.Angle, field ) is { } height )
			origin = origin with { Z = height };

		return (origin, ParkObjects.Turn( piece.Angle ));
	}

	private void Place( Piece piece )
	{
		var directory = $"levels/{_theme}/hoarding";

		try
		{
			var model = new LobbyModel( $"{directory}/{piece.Model}.MD2", $"{directory}/textures", Vector3.Zero,
				sharedTextureDirectory: $"levels/{_theme}/sharetex" );

			var (origin, turn) = StandOf( piece, ParkGround.Current?.Heightfield );

			model.SetTransform( origin, turn );

			_models.Add( model );
			_pieces.Add( piece );
		}
		catch ( Exception e )
		{
			Log.Warning( $"{_theme}: boundary piece '{piece.Model}' would not load, so ({piece.CellX},{piece.CellY}) is missing - {e.Message}" );
		}
	}

	/// <summary>The console's <c>boundary</c>: the pieces by model, the anchors, and with a cell the pieces made on or beside it.</summary>
	public IEnumerable<string> Census( (int X, int Y)? near = null )
	{
		yield return $"boundary pieces {_pieces.Count} anchors {Anchors} unmade {Math.Max( 0, Anchors - _pieces.Count )} "
			+ string.Join( " ", _pieces.GroupBy( piece => piece.Model ).OrderBy( group => group.Key ).Select( group => $"{group.Key} {group.Count()}" ) );

		if ( near is not { } cell )
			yield break;

		foreach ( var piece in _pieces.Where( piece => Math.Abs( piece.CellX - cell.X ) <= 2 && Math.Abs( piece.CellY - cell.Y ) <= 2 ) )
			yield return $"  {piece.Model} row {piece.Row} at ({piece.CellX},{piece.CellY}) angle {piece.Angle}";
	}

	/// <summary>Takes the pieces down and lets go of <see cref="Current"/> where it is still this one.</summary>
	protected override void OnDelete()
	{
		base.OnDelete();

		foreach ( var model in _models )
		{
			foreach ( var entity in model.Entities )
				entity.Delete();
		}

		_models.Clear();

		if ( Current == this )
			Current = null;
	}

	protected override void OnUpdate()
	{
		foreach ( var model in _models )
			model.Update( GameClock.Delta );
	}
}
