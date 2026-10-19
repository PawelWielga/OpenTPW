namespace OpenTPW;

public static partial class Input
{
	public struct MouseInfo
	{
		public Vector2 Delta;
		public Vector2 Position;

		public bool Left;
		public bool Right;

		/// <summary>
		/// Whether the left button went down this frame: a press the window system sent, as the original acts on
		/// the press its cursor library's callback posts (0x00486c70, message 0x10005). A button already held when the frame began, or when the interface was last
		/// looking, is not one.
		/// </summary>
		public bool LeftWentDown;

		/// <summary>
		/// How many presses of the left button the window system sent this frame. <see cref="Left"/> is only where the
		/// button stands as the frame ends, so a press let go inside one frame shows here and in
		/// <see cref="LeftWentDown"/> alone.
		/// </summary>
		public int LeftPresses;

		/// <summary>Whether the right button went down this frame, as <see cref="LeftWentDown"/> is for the left.</summary>
		public bool RightWentDown;

		/// <summary>
		/// How many presses of the right button the window system sent this frame, as <see cref="LeftPresses"/> is for
		/// the left: the original's cursor library posts the right button's press and release as messages too
		/// (<c>0x00486c70</c>, button 1).
		/// </summary>
		public int RightPresses;

		public float Wheel;

		public override string ToString()
		{
			var str = "";

			foreach ( var field in typeof( MouseInfo ).GetFields() )
				str += $"{field.Name}: {field.GetValue( this )}\n";

			return str;
		}
	}
}
