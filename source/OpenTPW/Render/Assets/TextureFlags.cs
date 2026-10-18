namespace OpenTPW;

[Flags]
public enum TextureFlags
{
	None = 0,

	/// <summary>
	/// If this is set, then the pixel value (255, 0, 255) will be treated as a transparent pixel.
	/// </summary>
	PinkChromaKey = 1,

	/// <summary>
	/// Force point filtering for this texture's sampler
	/// </summary>
	PointFilter = 2,

	/// <summary>
	/// Force wrap for this texture's sampler
	/// </summary>
	Wrap = 4,

	Repeat = 8,

	/// <summary>
	/// Kept at eight bits a channel where the game's art is cut to four (<see cref="Texture.Cut"/>): the one-colour
	/// stand-ins and the sky's tint, which are OpenTPW's own, and the loading screen, the pointer and the letters
	/// <see cref="UiText"/> sets, whose way to the screen in the original (its texture cache or a blit) is not read.
	/// </summary>
	OwnPixels = 16
}
