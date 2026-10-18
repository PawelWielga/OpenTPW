namespace OpenTPW;

/// <summary>
/// How a park is lit: the theme's ambient colour and the direction its one sun travels, both set once
/// as the park loads and never changed by anything time-driven. The sun's colour stays on
/// <see cref="Level.SunLight"/>. Handed to <c>content/shaders/test.shader</c> by <see cref="ModelEntity"/>;
/// see <c>docs/exe/park-engine.md</c>, "The lighting model".
/// </summary>
/// <param name="Ambient">ThemeEngine.AmbientLightLevel's R, G, B / 255.</param>
/// <param name="Travels">LightNormal, normalised and swapped into this engine's Z-up axes.</param>
/// <param name="Lit">The detail file's <c>GraphicalOptions.LIGHTING</c>. Off, at low quality, the original gives every
/// vertex of the ground and of every model the full colour in place of the light (<c>Light_ModelVertexColours</c>
/// <c>0x00574660</c>; <c>docs/exe/park-engine.md</c>, "What <c>GraphicalOptions.LIGHTING</c> 0 turns off").</param>
public sealed record ParkLight( Vector3 Ambient, Vector3 Travels, bool Lit = true )
{
	/// <summary>The shader's <c>g_flFullLight</c> for a draw under <paramref name="light"/>: 1 only in a park at low quality.</summary>
	internal static float FullLight( ParkLight? light ) => light is { Lit: false } ? 1f : 0f;
}
