using SVector3 = System.Numerics.Vector3;

namespace OpenTPW;

/// <summary>
/// The picker's object march (<c>FUN_0045bf90</c> after its terrain test, <c>FUN_0045cdc0</c>, <c>FUN_0045d090</c>;
/// <c>docs/exe/park-engine.md</c>, "The picker's object march"): the ray is stepped a cell at a time from the eye, each
/// cell's placed thing is asked whether the ray's height across the cell lies between its model's floor and its
/// <c>.hmp</c> height there, and a thing that passes has its model's triangles tested in the file's order. The first
/// triangle met gives the point, and the point gives the cell, where it is nearer the eye than the ground.
/// </summary>
internal static class ParkPickMarch
{
	/// <summary>The march's step along the ray's longer ground axis, one cell (<c>0x0045ca03</c>'s 10.0).</summary>
	internal const float Step = 10f;

	/// <summary>
	/// How far past a cell edge the ray is taken when it crosses one sideways between two steps, so that the cell
	/// it only clips is asked too (<c>0x006fe668</c>, 1.01).
	/// </summary>
	internal const float PastTheEdge = 1.01f;

	/// <summary>The least sum of the three angles a point inside a triangle makes with its corners (<c>0x006fe678</c>).</summary>
	internal const float InsideAngles = 6.2821855f;

	/// <summary>
	/// How many steps the march takes before giving up: the original's loop has no count, and ends only once the ray
	/// is below the ground's point or below nought, which a ray that does not fall never is. A deviation; the park is
	/// 128 cells across, so a march that has gone 400 has left it.
	/// </summary>
	internal const int MostSteps = 400;

	/// <summary>
	/// A cell's thing as the march asks it: how high its <c>.hmp</c> says it stands over the cell, the floor of its
	/// model's box, and the triangle test (null where none is met).
	/// </summary>
	internal readonly record struct Candidate( int Thing, float Top, float Floor, Func<SVector3, SVector3, SVector3?> Meet );

	/// <summary>
	/// Marches the ray from <paramref name="from"/> along <paramref name="direction"/> (z up) and answers the first
	/// thing met and where, or null. The march goes on while the ray stands at or above <paramref name="ground"/>'s
	/// height plus one step's fall, or above nought where the ray met no ground (<c>0x0045c9f0</c>..<c>0x0045ca17</c>).
	/// <paramref name="cell"/> answers a cell's thing, already held to the map (<see cref="Hold"/>).
	/// </summary>
	internal static (int Thing, SVector3 Point)? March( SVector3 from, SVector3 direction, SVector3? ground,
		Func<int, int, Candidate?> cell )
	{
		// The longer of the two ground axes is stepped by a whole cell; the other and the height follow
		// (0x0045c99a..0x0045c9e0). Axis 0 steps y, the original's z.
		var alongY = MathF.Abs( direction.X ) <= MathF.Abs( direction.Y );
		var major = alongY ? direction.Y : direction.X;
		var minor = alongY ? direction.X : direction.Y;

		if ( major == 0f )
			return null;

		var step = major > 0f ? Step : -Step;
		var sideways = minor / major * step;
		var fall = direction.Z / major * step;

		var floor = ground is { } g ? g.Z + fall : 0f;
		var at = from;

		for ( var steps = 0; floor <= at.Z && steps < MostSteps; ++steps )
		{
			var (cellX, cellY) = (Truncate( at.X ), Truncate( at.Y ));

			if ( Ask( cell, cellX, cellY, alongY, at, fall, step, direction, from ) is { } met )
				return met;

			var was = at;
			at += alongY ? new SVector3( sideways, step, fall ) : new SVector3( step, sideways, fall );

			// A ray that crosses a cell edge on the other axis between two steps clips a cell neither step stands in.
			var (wasSide, nowSide) = alongY ? (was.X, at.X) : (was.Y, at.Y);

			if ( Truncate( nowSide ) == Truncate( wasSide ) || at.X < 0f || at.Y < 0f )
				continue;

			var t = PastTheEdge * (nowSide <= wasSide
				? Fraction( wasSide * 0.1f ) / ((wasSide * 0.1f) - (nowSide * 0.1f))
				: 1f - (Fraction( nowSide * 0.1f ) / ((nowSide * 0.1f) - (wasSide * 0.1f))));

			var clipped = (was * (1f - t)) + (at * t);

			if ( Ask( cell, Truncate( clipped.X ), Truncate( clipped.Y ), alongY, clipped, fall, step, direction, from ) is { } edge )
				return edge;
		}

		return null;
	}

	/// <summary>
	/// <c>FUN_0045cdc0</c> on one cell: its thing is tested only where the ray's height across the cell, from where it
	/// enters along the stepped axis to where it leaves, reaches down to the thing's top and up to its floor.
	/// </summary>
	private static (int Thing, SVector3 Point)? Ask( Func<int, int, Candidate?> cell, int cellX, int cellY, bool alongY,
		SVector3 at, float fall, float step, SVector3 direction, SVector3 from )
	{
		if ( cell( cellX, cellY ) is not { } thing )
			return null;

		// How far into its cell the point is along the stepped axis, counted the way the ray goes (0x0045cee4..0x0045cf52).
		var along = alongY ? at.Y : at.X;
		var into = step > 0f ? Fraction( along * 0.1f ) : 1f - ((along % Step) * 0.1f);

		var entering = at.Z - (into * fall);
		var leaving = at.Z + ((1f - into) * fall);

		if ( MathF.Min( entering, leaving ) > thing.Top || MathF.Max( entering, leaving ) < thing.Floor )
			return null;

		return thing.Meet( from, direction ) is { } point ? (thing.Thing, point) : null;
	}

	/// <summary>A cell index of the march, <c>__ftol</c> of a tenth: truncated towards nought.</summary>
	internal static int Truncate( float world ) => (int)(world * 0.1f);

	/// <summary>
	/// The cell the march asks, held to nought and below <c>MapInfo</c>'s width and height (<c>0x0045cdd8</c>..<c>0x0045ce0a</c>);
	/// a limit of nought holds nothing, as <see cref="ParkPicking.CellOf"/>.
	/// </summary>
	internal static (int X, int Y) Hold( int x, int y, int mapWidth, int mapHeight )
		=> (mapWidth > 0 ? Math.Clamp( x, 0, mapWidth - 1 ) : Math.Max( x, 0 ), mapHeight > 0 ? Math.Clamp( y, 0, mapHeight - 1 ) : Math.Max( y, 0 ));

	/// <summary><c>modf</c>'s fractional part: the value less its truncation, with its sign.</summary>
	private static float Fraction( float value ) => value - MathF.Truncate( value );

	/// <summary>
	/// <c>FUN_0045d090</c>'s test of one triangle: the ray's line meets the plane through <paramref name="a"/> facing
	/// <paramref name="normal"/>, and the point is inside where the three angles it makes with the corners sum to
	/// <see cref="InsideAngles"/> or more. The line, not the ray: nothing asks which side of the eye the point is on.
	/// </summary>
	internal static SVector3? Meet( SVector3 from, SVector3 direction, SVector3 a, SVector3 b, SVector3 c, SVector3 normal )
	{
		var n = SVector3.Normalize( normal );
		var t = -(SVector3.Dot( n, from ) - SVector3.Dot( n, a )) / SVector3.Dot( n, direction );
		var point = from + (direction * t);

		var angles = Angle( a - point, b - point ) + Angle( b - point, c - point ) + Angle( c - point, a - point );

		return angles >= InsideAngles ? point : null;
	}

	private static float Angle( SVector3 u, SVector3 v )
		=> MathF.Acos( SVector3.Dot( SVector3.Normalize( u ), SVector3.Normalize( v ) ) );

	/// <summary>
	/// The model's triangles in the order <c>FUN_0045d090</c> walks them, each with its plane's normal: from the root
	/// node, a node's faces by its materials in order (each material's first face and count), then its children, each
	/// before the next. A node <paramref name="shown"/> refuses (the engine's runtime <c>0x10</c>) or flagged
	/// <c>0x8210</c> offers no faces; a node flagged <c>0x20</c> offers no children. <paramref name="corner"/> places a
	/// mesh's drawn vertex in the world.
	/// </summary>
	internal static IEnumerable<(SVector3 A, SVector3 B, SVector3 C, SVector3 Normal)> Triangles( ModelFile file,
		Func<int, bool> shown, Func<int, uint, SVector3> corner )
	{
		var root = file.Nodes.FindIndex( node => node.ParentIndex < 0 );

		if ( root < 0 )
			yield break;

		var pending = new Stack<int>();
		pending.Push( root );

		for ( var guard = file.Nodes.Count * 2; pending.Count > 0 && guard > 0; --guard )
		{
			var index = pending.Pop();
			var node = file.Nodes[index];

			if ( index < file.Meshes.Count && (node.Flags & 0x8210) == 0 && shown( index ) )
			{
				foreach ( var triangle in MeshTriangles( file.Meshes[index], drawn => corner( index, drawn ) ) )
					yield return triangle;
			}

			if ( (node.Flags & 0x20) != 0 )
				continue;

			// Children are pushed last first so that the first child comes off the stack first.
			var children = new List<int>();

			for ( int child = node.FirstChild, seen = 0; child >= 0 && child < file.Nodes.Count && seen < file.Nodes.Count;
				child = file.Nodes[child].NextSibling, ++seen )
				children.Add( child );

			for ( var i = children.Count - 1; i >= 0; --i )
				pending.Push( children[i] );
		}
	}

	private static IEnumerable<(SVector3 A, SVector3 B, SVector3 C, SVector3 Normal)> MeshTriangles( ModelFile.Mesh mesh,
		Func<uint, SVector3> corner )
	{
		var faces = mesh.Indices.Length / 3;

		foreach ( var material in mesh.Materials )
		{
			for ( var face = (int)material.a; face < material.a + material.b && face < faces; ++face )
			{
				if ( mesh.Indices[(face * 3) + 2] >= mesh.Vertices.Length )
					continue;

				var a = corner( mesh.Indices[face * 3] );
				var b = corner( mesh.Indices[(face * 3) + 1] );
				var c = corner( mesh.Indices[(face * 3) + 2] );

				// The triangle's own plane: the running game's table at mesh 0x64 holds each face's normal parallel to its
				// triangle, though the file's entries for faces are not (docs/exe/park-engine.md, "The picker's object march").
				yield return (a, b, c, SVector3.Cross( b - a, c - a ));
			}
		}
	}

	/// <summary>The first triangle met in <see cref="Triangles"/>' order, or null.</summary>
	internal static SVector3? MeetFirst( IEnumerable<(SVector3 A, SVector3 B, SVector3 C, SVector3 Normal)> triangles,
		SVector3 from, SVector3 direction )
	{
		foreach ( var (a, b, c, normal) in triangles )
		{
			if ( Meet( from, direction, a, b, c, normal ) is { } point )
				return point;
		}

		return null;
	}

	/// <summary>
	/// The first triangle of a standing model the line meets: each mesh as it is drawn now, at its entity's place and
	/// with its morph's positions where one poses it, a mesh drawn at nought opacity hidden.
	/// </summary>
	internal static SVector3? MeetModel( LobbyModel model, SVector3 from, SVector3 direction )
	{
		var animators = new Dictionary<int, MeshAnimator>();

		foreach ( var animator in model.Animators )
		{
			if ( animator.Morphing )
				animators.TryAdd( animator.TargetIndex, animator );
		}

		SVector3 Corner( int index, uint drawn )
		{
			var mesh = model.Source.Meshes[index];
			var file = animators.TryGetValue( index, out var animator ) && drawn < mesh.VertexOrder.Length
				? animator.SourcePosition( mesh.VertexOrder[drawn] ).GetSystemVector3()
				: mesh.Vertices[drawn].Position.GetSystemVector3();

			return SVector3.Transform( new SVector3( file.X, file.Z, file.Y ), model.Entities[index].ModelMatrix );
		}

		return MeetFirst( Triangles( model.Source, index => index < model.Entities.Length && model.Entities[index].Opacity > 0f,
			Corner ), from, direction );
	}
}
