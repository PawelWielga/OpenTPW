namespace OpenTPW;

/// <summary>
/// A person's shadow on the ground: their picture's quad stood up in the world, laid flat along the sun's direction and
/// set on the land, drawn after the people (<c>SpriteList_Draw</c>'s second pass, <c>0x0058aa4f</c>; <c>SpriteShadow_Draw</c>
/// and <c>SpriteShadow_BuildQuad</c>; <c>docs/exe/park-engine.md</c>, "A person's shadow").
///
/// <para>
/// <b>Which picture.</b> The detail file's <c>GraphicalOptions.SPRITESHADOW</c> picks it: 0 draws none, 1 (medium) the
/// shipped <c>generic/shadow/alphkid</c> silhouette over every person, 2 and 3 the person's own picture
/// (<c>0x0044da97</c>). The own picture is not built: it is counted, <c>SPRITE_SHADOW_OWN_PICTURE</c>, and draws none.
/// </para>
/// <para>
/// <b>How dark.</b> The original keeps one shadow a light for up to four lights travelling downwards
/// (<c>ShadowLights_Add</c> (<c>0x00584be0</c>)); a park makes one, the sun. A light's darkness is its downward share times 127, truncated,
/// and the shadow multiplies what lies under it by <c>255 - darkness</c> over 255 (state word low byte <c>0x13</c>,
/// SRCBLEND ZERO, DESTBLEND SRCCOLOR), cut to the silhouette by the alpha test at 16, writing no depth.
/// </para>
/// </summary>
internal static class ParkPersonShadows
{
	/// <summary>The silhouette <c>0x00550950</c> loads into the sprite list's <c>+0x2400</c> (it names the <c>.tga</c>; the loader finds the <c>.wct</c>).</summary>
	internal const string Silhouette = "generic/shadow/alphkid.wct";

	/// <summary>A picture pixel's length in the world: the span <c>0x0070200c</c>, 20, over the 128 every picture stores.</summary>
	internal const float Span = 20f / 128f;

	/// <summary>
	/// The across times <c>DAT_008bcbcc</c>, the original's screen height over its width, 0.75 at every mode it offers
	/// (all 4:3). The shadow lies in the world, so here it is that constant, not this window's shape.
	/// </summary>
	internal const float Across = 0.75f;

	/// <summary>How far above the land a shadow's corners stand: <c>0x008bd4ec</c>, 0.6 (<c>0x00576a72</c>).</summary>
	internal const float Lift = 0.6f;

	/// <summary>The lift under render word <c>0x2000</c>, the first-person and ride views: 0.25 (<c>0x00576a81</c>).</summary>
	internal const float FirstPersonLift = 0.25f;

	/// <summary>The detail file's <c>GraphicalOptions.SPRITESHADOW</c>: 0 in low.sam, 1 in med.sam, 2 in high.sam. A file that will not read gives medium's 1.</summary>
	internal static int Tier( string detailFile )
	{
		try
		{
			if ( int.TryParse( new SettingsFile( $"/{detailFile}" )["GraphicalOptions.SPRITESHADOW"], out var value ) )
				return value;
		}
		catch ( Exception e )
		{
			Log.Warning( $"Shadows: {detailFile} would not load, so people cast medium's shadows - {e.Message}" );
		}

		return 1;
	}

	/// <summary>
	/// A light's darkness, 0 to 127, from the direction it travels (this engine's Z up): its downward share times 127,
	/// truncated (<c>0x00584d55</c>, the scale <c>0x00701ecc</c> -127), or null for a light that does not travel
	/// downwards, which casts none.
	/// </summary>
	internal static int? Darkness( Vector3 travels )
	{
		var scaled = travels.Z * -127f;
		return scaled > 0f ? (int)scaled : null;
	}

	/// <summary>The shadow's colour, 0xAARRGGBB: opaque, each channel 255 less the darkness (<c>0x00589ab5</c> to <c>0x00589acf</c>).</summary>
	internal static uint Colour( int darkness ) => 0xffffffffu - ((uint)darkness * 0x010101u);

	/// <summary>
	/// A person's shadow's four corners, each with the silhouette's u and v there: the picture's width across the
	/// light at the person's <paramref name="place"/> (reflected, both sides negated about it), its height stood up from
	/// there and laid flat along <paramref name="travels"/>, each corner then set <paramref name="lift"/> above
	/// <paramref name="ground"/>'s plane there. Corners 0 and 1 are the picture's top, at the far end; 2 and 3 its foot.
	/// False, writing nothing, for a light that does not travel downwards.
	/// </summary>
	internal static bool Corners( Span<Vertex> corners, Vector2 place, int width, int height, int originX, bool mirrored,
		Vector3 travels, Func<float, float, float> ground, float lift, uint colour )
	{
		if ( Darkness( travels ) is null )
			return false;

		// The across (Lz, 0, -Lx) of the original's Y-up axes (0x00584d0f to 0x00584d45), unnormalised: shorter as the sun
		// stands higher.
		var across = new Vector2( travels.Y, -travels.X );
		var left = -originX * Span * Across;
		var right = (width - originX) * Span * Across;

		if ( mirrored )
			(left, right) = (-left, -right);

		// The picture's height, stood up from the foot and laid along the light by the matrix ShadowLights_Add builds.
		var tall = height * Span;
		var laid = new Vector2( travels.X, travels.Y ) * (-tall / travels.Z);

		corners[0] = Corner( place + (across * left) + laid, new Vector2( 0f, 0f ) );
		corners[1] = Corner( place + (across * right) + laid, new Vector2( 1f, 0f ) );
		corners[2] = Corner( place + (across * right), new Vector2( 1f, 1f ) );
		corners[3] = Corner( place + (across * left), new Vector2( 0f, 1f ) );

		return true;

		Vertex Corner( Vector2 at, Vector2 uv )
			=> new( new Vector3( at.X, at.Y, ground( at.X, at.Y ) + lift ), uv ) { MatFlags = colour };
	}
}
