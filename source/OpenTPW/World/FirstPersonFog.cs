namespace OpenTPW;

/// <summary>
/// The fog the original lays over first person: linear, worked out at each vertex and blended across the triangle,
/// none before <see cref="Start"/> and the theme's <c>FogColour</c> alone from <see cref="End"/> on, both read from
/// the detail file's <c>GraphicalOptions.FIRSTPERSONVIEWDISTANCE</c> (<c>DetailFile_Apply</c> <c>0x0044d8f0</c>). The
/// depth it is measured in is the original's clip z, the float at a batch vertex's <c>+8</c> that
/// <c>Fog_VertexSpecular</c> compares, which is <see cref="ClipScale"/> × (eye depth − <see cref="ClipNear"/>); the shaders
/// take the band in eye depth. Everything in the park's world is fogged but the sky (docs/exe/park-engine.md, "First
/// person's fog").
/// </summary>
/// <param name="Start">Where the fog begins, in the original's clip z.</param>
/// <param name="End">Where it is whole, in the original's clip z.</param>
public sealed record FirstPersonFog( float Start, float End )
{
	/// <summary>
	/// The original's draw matrix's z row over its w: clip z = 0.70718 (e − 0.1), the projection's sin 45 times
	/// 1000 / 999.9, read live from <c>[0x0087b088]</c> in first person (park-engine.md, "The pull").
	/// </summary>
	internal const float ClipScale = 0.70718f;

	/// <summary>The near distance in that z row, <c>0x0042aa4e</c>.</summary>
	internal const float ClipNear = 0.1f;

	/// <summary>
	/// The band for a value of <c>FIRSTPERSONVIEWDISTANCE</c>: 0 to 3 as <c>DetailFile_Apply</c> sets them, any other
	/// value the startup default 100 to 130 (<c>0x00582de5</c>), which the detail file then leaves alone.
	/// </summary>
	internal static FirstPersonFog ForViewDistance( int viewDistance ) => viewDistance switch
	{
		0 => new( 50f, 60f ),
		1 => new( 100f, 110f ),
		2 => new( 135f, 150f ),
		3 => new( 230f, 250f ),
		_ => new( 100f, 130f )
	};

	/// <summary>
	/// The band of a detail file: <c>low.sam</c> 0, <c>med.sam</c> 1, <c>high.sam</c> 2. A file that will not read
	/// gives medium's.
	/// </summary>
	internal static FirstPersonFog Read( string detailFile )
	{
		try
		{
			if ( int.TryParse( new SettingsFile( $"/{detailFile}" )["GraphicalOptions.FIRSTPERSONVIEWDISTANCE"], out var value ) )
				return ForViewDistance( value );
		}
		catch ( Exception e )
		{
			Log.Warning( $"First person: {detailFile} would not load, so its fog takes medium's band - {e.Message}" );
		}

		return ForViewDistance( 1 );
	}

	/// <summary>An eye depth's clip z, the depth the band is in.</summary>
	internal static float ClipDepth( float eyeDepth ) => ClipScale * (eyeDepth - ClipNear);

	/// <summary>A clip z's eye depth.</summary>
	internal static float EyeDepth( float clipDepth ) => clipDepth / ClipScale + ClipNear;

	/// <summary>
	/// How much fog a vertex at <paramref name="eyeDepth"/> takes, 0 to 1: <c>Fog_VertexSpecular</c>'s
	/// (<c>d</c> − start) / (end − start), none at or before the start, whole at or past the end.
	/// </summary>
	internal float At( float eyeDepth )
	{
		var d = ClipDepth( eyeDepth );

		if ( d <= Start )
			return 0f;

		return d >= End ? 1f : (d - Start) / (End - Start);
	}

	/// <summary>
	/// The shaders' <c>g_vLinearFog</c>: the band's start and end in eye depth and 1 to switch it on, all nought for
	/// a draw it does not reach.
	/// </summary>
	internal System.Numerics.Vector4 Uniform => new( EyeDepth( Start ), EyeDepth( End ), 1f, 0f );

	public override string ToString() =>
		$"{Start:0.##} to {End:0.##} (eye depth {EyeDepth( Start ):0.0} to {EyeDepth( End ):0.0})";
}
