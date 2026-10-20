namespace OpenTPW.UI;

/// <summary>
/// A meter - control type 9 in the layout stream, and the only one of the thirteen types that shows a
/// number as a quantity rather than as lettering.
///
/// <para>
/// <b>It carries no mesh.</b> <c>FUN_0066d750</c> builds it as the plain
/// control with three more fields, and the original hands it a <c>meter.wct</c> skin through
/// <c>Meter_Construct</c> rather than a model - so a control drawing only <see cref="UiControl.Mesh"/> can
/// never show one. The texture ships in ui.wad at <c>textures\meter.wct</c>, 128x128.
/// </para>
/// <para>
/// <b>The value arrives through the vtable, clamped.</b> <c>PTR_FUN_00705c28</c> slot <c>+0x1c</c> is
/// <c>FUN_0066d885</c>: it writes the value to the control's own <c>+0x140</c>, holds it to
/// <b>0 to 0x400</b>, and then passes it to two sub-objects at <c>+0x98</c> and <c>+0x9c</c> through
/// their own vtable+0xc. So the original's full scale is <b>1024</b>, not a hundred, and whoever sets it
/// scales first - which is why <see cref="Max"/> is taken here rather than assumed.
/// </para>
/// <para>
/// <b>The picture is two halves of the skin cropped at one boundary.</b> The sub-object built by <c>Meter_Construct</c>
/// holds two quads in a 100 by 100 space sharing the boundary p, the value as a percentage (<c>Meter_OnMessage</c>
/// <c>0x00477db0</c>): above it the texture's right half (u 0.5 to 1), the dark capsule, and below it the left half
/// (u 0 to 0.5), the lit one, each with v = y / 100 (<c>Meter_SetQuadGeometry</c> <c>0x00477cc0</c>,
/// <c>Meter_SetQuadUVs</c> <c>0x00477d20</c>), so neither is ever squashed. The space's y of 0 is the control's bottom
/// edge (<c>FUN_00476c50</c> is handed the bottom as its origin), and the original's frame agrees: lit below, dark
/// above, the sad face at the bottom (<c>docs/exe/hud.md</c>, "The happiness gauge's two quads"). At nought the dark
/// half covers the whole capsule, as the constructor builds it before any value arrives.
/// </para>
/// </summary>
internal sealed class UiMeter : UiControl
{
	/// <summary>
	/// The skin the original gives it: the gadget's gauge takes <c>meter.wct</c> (<c>Meter_Construct</c>). The
	/// all-staff screen's two meters are not this skin but the stat bars' (<see cref="ParkObjectWindow"/>'s
	/// <c>UiStatBar</c>, <c>FUN_00477ff0</c>).
	/// </summary>
	public string Skin { get; init; } = "ui/textures/meter.wct";

	/// <summary>Loaded once per skin and shared, as <see cref="UiEdit"/>'s selection highlight is.</summary>
	private static readonly Dictionary<string, Texture> Skins = [];

	/// <summary>
	/// What counts as full. The original's own clamp is 1024 (see the class remarks), and a caller
	/// working in percent says so by leaving this at a hundred rather than by scaling on the way in.
	/// </summary>
	public int Max { get; init; } = 100;

	/// <summary>How far up it stands, in the same units as <see cref="Max"/>. Out-of-range values are held, not refused.</summary>
	public int Value
	{
		get => _value;
		set
		{
			if ( value == _value && _set )
				return;

			_value = value;
			_set = true;
			Log.Info( $"UI: meter 0x{Id:x} at {value} of {Max}, the boundary {Percent:F2}% up" );
		}
	}

	private int _value;
	private bool _set;

	protected override void OnDraw()
	{
		// Whatever the control itself carries first - it has no mesh of its own, but a meter given one
		// should still draw it, and its text rectangle is honoured the same way every other control's is.
		base.OnDraw();

		if ( Max <= 0 )
			return;

		if ( !Skins.TryGetValue( Skin, out var skin ) )
			Skins[Skin] = skin = new Texture( Skin );

		foreach ( var (where, uvs) in Quads( Pixels, Percent ) )
			Paint( skin, where, uvs );
	}

	/// <summary>
	/// Where the boundary stands, 0 to 100: the original's <c>value x 1/1024 x 100</c>, held, in single precision
	/// (<c>0x006fee08</c>, <c>0x006fee0c</c>) when <see cref="Max"/> is its 1024, and the plain share otherwise.
	/// </summary>
	internal float Percent => Math.Clamp( Value, 0, Max ) * (1f / Max) * 100f;

	/// <summary>
	/// The skin's two quads over a box (top-left origin) for a boundary at <paramref name="percent"/>: the dark right
	/// half above it and the lit left half below it, each showing the band of the texture that stands where it is
	/// drawn (this game's v of 0 is the bottom of a quad, as the original's y of 0 is the bottom of the space).
	/// </summary>
	internal static (Rectangle Where, Rectangle Uvs)[] Quads( PixelRect box, float percent )
	{
		var t = Math.Clamp( percent, 0f, 100f ) * 0.01f;
		var lit = box.Height * t;

		return
		[
			(new Rectangle( box.X, box.Y, box.Width, box.Height - lit ), new Rectangle( 0.5f, t, 0.5f, 1f - t )),
			(new Rectangle( box.X, box.Y + box.Height - lit, box.Width, lit ), new Rectangle( 0f, 0f, 0.5f, t ))
		];
	}

	private static void Paint( Texture skin, Rectangle where, Rectangle uvs )
	{
		if ( where.Width <= 0f || where.Height <= 0f )
			return;

		Material.UI.Set( "Color", skin );

		using ( _ = new Graphics.Scope( Screen.Size ) )
			Graphics.Quad( OnScreen( where, Screen.Height ), uvs, Material.UI );
	}

	/// <summary>
	/// A quad's box (top-left origin) in the draw's space, whose y of 0 is the screen's bottom: the flip that makes the
	/// lit half, last in <see cref="Quads"/>, fill up from the control's bottom edge.
	/// </summary>
	internal static Rectangle OnScreen( Rectangle where, float screenHeight ) =>
		new( where.X, screenHeight - where.Y - where.Height, where.Width, where.Height );
}
