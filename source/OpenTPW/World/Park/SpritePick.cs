using System.Numerics;

namespace OpenTPW;

/// <summary>
/// The person under the pointer, taken the original's way: by the picture drawn there, not by the cell
/// (<c>docs/exe/park-engine.md</c>, "The thing under a sprite").
///
/// <para>
/// <b>The draw latches, the pick asks.</b> As each person's picture is drawn, <c>FUN_005890e0</c> latches it
/// (<c>0x0058931a</c>) when the pointer lies inside a circle about the picture's left edge at its origin's height, the
/// circle's radius squared a fifty-thousandth of the picture's span squared; of two, the nearer the eye wins. The pick
/// <c>FUN_00532bd0</c> then takes that person if they stand on the hovered cell or one of its eight neighbours, and
/// nobody otherwise. Here the people's draw records each picture it draws (<see cref="Drawn"/>) and the pick reads the
/// last frame's, as the original's pick reads the last draw's latch.
/// </para>
/// <para>
/// <b>The arithmetic is the original's, in its 4:3 frame.</b> The pointer and the picture's place are put in the -1 to 1
/// box of the 4:3 frame the lens draws the original's picture in (<see cref="Frame"/>), down the screen positive, so a
/// window of any shape takes the circle the original takes at the same picture: at 1280 x 720 Mike Cooper's 34 x 55
/// reaches 64 pixels across and 48 down, as the original's 1024 x 768 does at 720 high. A picture's place is its origin
/// put through this game's lens, which draws the original's (Q441); its span is the original's, its pixels times twenty
/// over 128, the across times 0.75 (<c>0x008bcbcc</c>, the original's screen's height over its width). The span is added
/// before the divide by the place's <c>w</c>, which in the original's projection is the sine of half the 90 degrees the
/// park hands it times the eye's depth (<c>FUN_00578be0</c>, <c>m23</c>), and the radius is not divided at all, so the
/// circle is the same size on the screen at every zoom.
/// </para>
/// </summary>
public static class SpritePick
{
	/// <summary>One person's picture as the people's draw last put it up: whose, where its origin stands in the world,
	/// its size and origin in pixels, and whether it is drawn reflected.</summary>
	internal readonly record struct Drawn( int Thing, Vector3 Origin, int Width, int Height, int OriginX, int OriginY, bool Mirrored );

	/// <summary>A picture's pixels to the original's span, in its clip space (<c>docs/exe/park-engine.md</c>, "The size a
	/// sprite is drawn"; the draw's world size is <see cref="ParkGuestSprites"/>'s).</summary>
	private const float Span = 20f / 128f;

	/// <summary>The radius squared over the span squared, <c>0x0070203c</c>.</summary>
	private const float Reach = 0.0002f;

	/// <summary>The original's screen's height over its width, <c>0x008bcbcc</c>: it runs only at 4:3.</summary>
	private const float HeightOverWidth = 0.75f;

	/// <summary>A place's <c>w</c> over the eye's depth in the original: <c>sin( 90 / 2 )</c>, the projection's
	/// <c>m23</c> for the angle the park hands it (<c>FUN_00578be0</c>; measured, a <c>w</c> of 77.78 at zoom 110).</summary>
	internal static readonly float Lens = MathF.Sin( MathF.PI / 4f );

	/// <summary>The cells <c>FUN_00532bd0</c> asks, in its order: the hovered cell, then its eight neighbours row by row
	/// (the pairs at <c>0x0081ed40</c>, filled at <c>0x00532b30</c>).</summary>
	internal static readonly (int X, int Y)[] Around =
		[(0, 0), (-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1)];

	/// <summary>
	/// The person whose picture the draw latches for a pointer, or nought (<c>FUN_005890e0</c>, <c>0x00589259</c> to
	/// <c>0x0058931a</c>): the pointer inside their circle, the nearest by depth of those it is inside, the first drawn of
	/// two at one depth.
	/// </summary>
	/// <param name="pointer">The pointer in the 4:3 frame's -1 to 1 box, down the screen positive (<see cref="Box"/>).</param>
	/// <param name="viewProjection">The view times the projection, a row vector on the left.</param>
	/// <param name="frame">The window's -1 to 1 box in the 4:3 frame's, across and down (<see cref="Frame"/>).</param>
	internal static int Latch( IEnumerable<Drawn> drawn, Vector2 pointer, Matrix4x4 viewProjection, Vector2 frame )
	{
		var latched = 0;
		var nearest = float.MaxValue;

		foreach ( var picture in drawn )
		{
			if ( Circle( picture, viewProjection, frame ) is not { } circle )
				continue;

			var distance = ((pointer.X - circle.X) * (pointer.X - circle.X)) + ((pointer.Y - circle.Y) * (pointer.Y - circle.Y));

			if ( distance < circle.Reach && circle.Depth < nearest )
			{
				latched = picture.Thing;
				nearest = circle.Depth;
			}
		}

		return latched;
	}

	/// <summary>
	/// A picture's circle: its middle in the 4:3 frame's -1 to 1 box, its radius squared there, and the place's depth that
	/// decides between two; null behind the eye, where nothing is drawn.
	/// </summary>
	internal static (float X, float Y, float Reach, float Depth)? Circle( Drawn picture, Matrix4x4 viewProjection, Vector2 frame )
	{
		var place = System.Numerics.Vector4.Transform( new System.Numerics.Vector4( picture.Origin.X, picture.Origin.Y, picture.Origin.Z, 1f ), viewProjection );

		if ( place.W <= 0f )
			return null;

		var across = Span * HeightOverWidth;
		var left = -picture.OriginX * across;
		var right = (picture.Width - picture.OriginX) * across;

		// Reflected, the picture's own left edge is drawn on the right (0x005891e5, flag 0x10).
		if ( picture.Mirrored )
			(left, right) = (-left, -right);

		var high = picture.Height * Span;

		return ((place.X / place.W * frame.X) + (left / (place.W * Lens)), -place.Y / place.W * frame.Y,
			((high * high) + ((right - left) * (right - left))) * Reach, place.W);
	}

	/// <summary>
	/// The latched person if they stand on the hovered cell or one of its neighbours, by each cell's list, or nought
	/// (<c>FUN_00532bd0</c>).
	/// </summary>
	internal static int Taken( ParkState state, int latched, int cellX, int cellY )
	{
		if ( latched == 0 )
			return 0;

		foreach ( var (dx, dy) in Around )
		{
			var (x, y) = (cellX + dx, cellY + dy);

			if ( !ParkState.OnMap( x, y ) )
				continue;

			for ( int thing = state.CellAt( x, y ).Occupant; thing != 0; thing = state.NextOnCell( thing ) )
			{
				if ( thing == latched )
					return latched;
			}
		}

		return 0;
	}

	/// <summary>A pixel of the window in the 4:3 frame's -1 to 1 box, down the screen positive, as <c>FUN_00532bd0</c> puts
	/// the original's 2048 x 1536 pointer in its own (x / 1024 − 1 and y / 767.5 − 1).</summary>
	internal static Vector2 Box( Vector2 screen, float width, float height )
	{
		var frame = Frame( width, height );

		return new( ((2f * screen.X / MathF.Max( width, 1f )) - 1f) * frame.X, ((2f * screen.Y / MathF.Max( height, 1f )) - 1f) * frame.Y );
	}

	/// <summary>
	/// How far a window <paramref name="width"/> by <paramref name="height"/> reaches in the -1 to 1 box of the 4:3 frame
	/// its lens draws the original's picture in (<see cref="Camera.Lens"/>): a wider window holds that frame's height and
	/// reaches width / height × 0.75 of it across, a narrower one holds its width and reaches height / width / 0.75 of it
	/// down. One both ways at 4:3.
	/// </summary>
	internal static Vector2 Frame( float width, float height )
	{
		var aspect = MathF.Max( width, 1f ) / MathF.Max( height, 1f );

		return aspect >= 1f / HeightOverWidth
			? new( aspect * HeightOverWidth, 1f )
			: new( 1f, 1f / (aspect * HeightOverWidth) );
	}
}
