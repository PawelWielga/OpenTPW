using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Every sign the game ships, read the way the game reads them.
///
/// <para>
/// <b>These exist because the obvious test cannot tell a broken reader from a working one.</b>
/// A reader that demands a 17,373-byte header before it looks at whether the file declares artwork
/// reads the four lobby boards and most of the others that ship a picture, and refuses all sixty-one
/// that do not. Testing it against a lobby sign - the natural choice, and the one the format was worked
/// out on - is green either way. Only walking all of them catches it, so that is what this does.
/// </para>
/// <para>
/// The walk is deliberately a discovery rather than a list of names. A test naming the signs it expects
/// would still pass if the reader lost the ability to find the rest of them, and the counts below are
/// the point of the exercise: far more boards are bare than painted, and a bare board is a normal sign
/// and not a truncated one.
/// </para>
/// <para>
/// These read real game files and are skipped where there is no installation - see <see cref="GameData"/>.
/// </para>
/// </summary>
[TestClass]
public class SignFileTests
{
	[TestInitialize]
	public void MountTheGame()
	{
		FileSystem = GameData.Required();
	}

	/// <summary>The folders a theme keeps free-standing items in - the same four ParkItemCatalogue walks.</summary>
	private static readonly string[] Folders = ["features", "shops", "rides", "sideshow"];

	/// <summary>
	/// What the game actually holds, counted through its own file system rather than off a disk full of
	/// extracted copies: 23 boards with a picture on them and 61 with none.
	/// </summary>
	private const int ExpectedPainted = 23;
	private const int ExpectedBare = 61;
	private const int ExpectedSigns = ExpectedPainted + ExpectedBare;

	/// <summary>
	/// Parsed once and kept. Decoding the painted boards is wavelet work, and three tests each doing it
	/// from scratch would cost more than the rest of the suite put together.
	/// </summary>
	private static (string Path, SignFile Sign)[]? _signs;

	private static (string Path, SignFile Sign)[] Signs()
	{
		return _signs ??= [.. EverySignPath().Select( path => (path, new SignFile( path )) )];
	}

	/// <summary>
	/// Where every sign in the game lives: the lobby's four beside the models that carry their meshes,
	/// and a ride's beside its own model, under whichever of the four item folders it is sold from.
	/// </summary>
	private static List<string> EverySignPath()
	{
		var signs = new List<string>();

		void Collect( string directory )
		{
			foreach ( var entry in Entries( directory, directories: false ) )
			{
				var name = Path.GetFileName( entry );

				if ( !string.IsNullOrEmpty( name ) && name.EndsWith( ".sgn", StringComparison.OrdinalIgnoreCase ) )
					signs.Add( $"{directory}/{name}" );
			}
		}

		Collect( "lobby/terrain" );

		foreach ( var theme in Entries( "levels", directories: true ) )
		{
			var themeName = Path.GetFileName( theme );

			if ( string.IsNullOrEmpty( themeName ) )
				continue;

			foreach ( var folder in Folders )
			{
				foreach ( var item in Entries( $"levels/{themeName}/{folder}", directories: true ) )
				{
					var stem = Path.GetFileName( item );

					if ( !string.IsNullOrEmpty( stem ) )
						Collect( $"levels/{themeName}/{folder}/{stem}" );
				}
			}
		}

		return signs;
	}

	/// <summary>
	/// A theme that does not sell one kind of thing has no folder for it, and asking throws rather than
	/// answering nothing - so this answers nothing, exactly as ParkItemCatalogue does.
	/// </summary>
	private static string[] Entries( string path, bool directories )
	{
		try
		{
			return directories ? FileSystem.GetDirectories( path ) : FileSystem.GetFiles( path );
		}
		catch ( Exception )
		{
			return [];
		}
	}

	/// <summary>
	/// The one that would have caught it: not one sign in the game is refused.
	/// </summary>
	[TestMethod]
	public void EverySignInTheGameReads()
	{
		var signs = Signs();

		var refused = signs.Where( entry => !entry.Sign.IsValid ).Select( entry => entry.Path ).ToArray();

		Assert.AreEqual( 0, refused.Length,
			$"{refused.Length} of {signs.Length} signs would not read, starting with '{refused.FirstOrDefault()}'" );

		// Counted after the reading rather than before it, so a surprise in the count cannot fire first and
		// hide a sign that would not read. A walk that quietly found nothing - or half of them - would
		// otherwise pass every assertion here.
		Assert.AreEqual( ExpectedSigns, signs.Length, "signs found by the walk" );
	}

	/// <summary>
	/// A board with no picture on it is a normal sign, and most of them are.
	///
	/// <para>
	/// The original clears such a board to transparent black and skips the artwork outright, leaving the
	/// ride's name lettered onto nothing with the ride showing through behind it. So a sign is expected
	/// to read with no image at all, and the reader must say valid-with-no-image rather than failing.
	/// </para>
	/// </summary>
	[TestMethod]
	public void MostBoardsCarryNoArtworkAndThatIsNotAFailure()
	{
		var signs = Signs();

		var bare = signs.Count( entry => entry.Sign.IsValid && entry.Sign.Image == null );
		var painted = signs.Count( entry => entry.Sign.IsValid && entry.Sign.Image != null );

		Assert.AreEqual( ExpectedPainted, painted, "boards with a picture on them" );
		Assert.AreEqual( ExpectedBare, bare, "boards with none" );
	}

	/// <summary>
	/// Every sign names two fonts and a file for each, painted or not.
	///
	/// <para>
	/// This is what makes a bare board worth reading at all: the lettering is the whole of its content.
	/// The two font records are read unconditionally by the engine, with no count anywhere in the file,
	/// and all 84 shipped signs fill both - including the three that set both line modes to zero, whose
	/// font records name a font the sign then inks with nothing.
	/// </para>
	/// </summary>
	[TestMethod]
	public void EverySignNamesTwoFontsAndAFileForEach()
	{
		var wrong = Signs()
			.Where( entry => entry.Sign.IsValid )
			.Where( entry => entry.Sign.Fonts.Count != 2
				|| entry.Sign.Fonts.Any( font => string.IsNullOrEmpty( font.FileName ) ) )
			.Select( entry => entry.Path )
			.ToArray();

		Assert.AreEqual( 0, wrong.Length,
			$"{wrong.Length} signs do not name two fonts with a file each, starting with '{wrong.FirstOrDefault()}'" );
	}

	/// <summary>
	/// A painted board decodes at the size its chunks are encoded at, which is not the size it states.
	///
	/// <para>
	/// The header of every painted sign says 256x128, and that is true of the artwork - but the chunks
	/// are encoded at the wavelet's aligned 256x256, and only the top half of the decoded image carries
	/// the board. The two readings cannot be told apart by the colour chunk, which inflates to 98304
	/// bytes either way, but the alpha chunk settles it at 65536: that is 256*256, not 256*128.
	/// </para>
	/// </summary>
	[TestMethod]
	public void APaintedBoardDecodesAtTheSizeItsChunksAreEncodedAt()
	{
		var painted = Signs().FirstOrDefault( entry => entry.Sign.IsValid && entry.Sign.Image != null );

		Assert.IsNotNull( painted.Path, "no painted board was found to check" );

		var image = painted.Sign.Image!.Value;

		Assert.AreEqual( SignFile.Size, image.Width, $"{painted.Path} decoded width" );
		Assert.AreEqual( SignFile.Size, image.Height, $"{painted.Path} decoded height" );
	}

	/// <summary>
	/// A board's four floats are the codec's multipliers as they are (Y 6, Cb 10, Cr 2, alpha 6), not the header
	/// scales a <c>.wct</c> turns into multipliers (which make them 4, 3.5, 1.5 and 7). Lost Kingdom's gate board's
	/// mean, and its texel at (100, 60), are what the original's codec gives (<c>wctorig.py</c>,
	/// <c>docs/exe/textures.md</c>, "A sign's artwork"); through the header's formulas the mean is
	/// (56.0, 44.1, 45.2) and the texel (60, 47, 48).
	/// </summary>
	[TestMethod]
	public void ABoardsFourFloatsAreTheCodecsMultipliersAsTheyAre()
	{
		var image = new SignFile( "levels/jungle/features/gates/gates.sgn" ).Image;

		Assert.IsNotNull( image, "Lost Kingdom's gate board" );

		var sums = new double[4];
		var pixels = SignFile.Size * SignFile.ArtworkHeight;

		for ( int i = 0; i < pixels * 4; ++i )
			sums[i % 4] += image.Value.Data[i];

		CollectionAssert.AreEqual( new[] { 82.6, 67.5, 64.3, 255.0 }, sums.Select( sum => Math.Round( sum / pixels, 1 ) ).ToArray(),
			"the board's mean red, green, blue and alpha" );
		var texel = ((60 * SignFile.Size) + 100) * 4;

		CollectionAssert.AreEqual( new byte[] { 88, 73, 67, 255 }, image.Value.Data[texel..(texel + 4)], "its texel at (100, 60)" );

		// The gate's board is opaque at either alpha multiplier; Lost Kingdom's sign1 is a shade under, 246 at 6
		// where 7 clamps it to 255.
		Assert.AreEqual( 246.0, MeanAlpha( new SignFile( "levels/jungle/features/sign1/sign1.sgn" ).Image!.Value.Data ), "sign1's mean alpha" );
	}

	/// <summary>The mean alpha of a board's first <see cref="SignFile.ArtworkHeight"/> rows, to one place.</summary>
	private static double MeanAlpha( byte[] data )
	{
		var pixels = SignFile.Size * SignFile.ArtworkHeight;
		var sum = 0.0;

		for ( int i = 3; i < pixels * 4; i += 4 )
			sum += data[i];

		return Math.Round( sum / pixels, 1 );
	}

	/// <summary>
	/// The board holds the artwork with red and blue exchanged, as the original's does: its codec writes blue, green,
	/// red and its board packs the first of them into a texel's red (<c>docs/exe/textures.md</c>, "A sign's
	/// artwork"). Lost Kingdom's gate board is then bluish, as the original draws it, where the codec's own order is
	/// brown; the texel is the one <see cref="ABoardsFourFloatsAreTheCodecsMultipliersAsTheyAre"/> reads.
	/// </summary>
	[TestMethod]
	public void TheBoardHoldsTheArtworkWithRedAndBlueExchanged()
	{
		var board = SignTexture.CropBoard( new SignFile( "levels/jungle/features/gates/gates.sgn" ).Image!.Value );

		var sums = new double[4];
		var pixels = SignFile.Size * SignFile.ArtworkHeight;

		for ( int i = 0; i < board.Length; ++i )
			sums[i % 4] += board[i];

		CollectionAssert.AreEqual( new[] { 64.3, 67.5, 82.6, 255.0 }, sums.Select( sum => Math.Round( sum / pixels, 1 ) ).ToArray(),
			"the board's mean red, green, blue and alpha" );

		var texel = ((60 * SignFile.Size) + 100) * 4;

		CollectionAssert.AreEqual( new byte[] { 67, 73, 88, 255 }, board[texel..(texel + 4)], "its texel at (100, 60)" );
		Assert.AreEqual( 246.0, MeanAlpha( SignTexture.CropBoard( new SignFile( "levels/jungle/features/sign1/sign1.sgn" ).Image!.Value ) ),
			"sign1's board keeps its alpha" );
	}

	/// <summary>
	/// A painted board is either solid art or a deliberate cut-out, and there is nothing in between.
	///
	/// <para>
	/// A material whose texture was substituted is forced see-through, because that is the only way a
	/// ride's bare board can show the ride behind it rather than filling with black - see
	/// <c>LobbyModel.MaterialFlagsFor</c>, and the original ORs the same bit in as it swaps the
	/// texture. That reaches the boards that <b>do</b> carry artwork too, which the world's shader would
	/// otherwise draw with their alpha ignored, since it only honours alpha on a see-through material. So
	/// what their alpha holds decides whether that is right for them.
	/// </para>
	/// <para>
	/// It is. Fifteen of the twenty-three are solid art that never goes below alpha 232: eleven are opaque
	/// but for 2.3% of texels at 253 or 254, the codec's ringing round a hard edge, and four (the
	/// <c>sign1</c> boards of three themes and the Jelly's) lie a shade under opaque throughout, as the
	/// original's codec decodes them with the sign's alpha multiplier of 6. The other eight reach alpha
	/// <b>0</b>: they are shaped art - the Bumper Cars, Candy Cabin, Cat Coaster, Ferris Wheel and the
	/// rest - that without the bit draws as an opaque rectangle with the cut-out it was authored with ignored.
	/// </para>
	/// <para>
	/// Only the rows the board is cut from are measured. The image decodes into a 256x256 buffer
	/// because that is the size its chunks are encoded at, but just the top <see cref="SignFile.ArtworkHeight"/>
	/// of them are ever drawn, and what the unused remainder holds does not matter.
	/// </para>
	/// </summary>
	[TestMethod]
	public void APaintedBoardIsEitherSolidOrADeliberateCutOut()
	{
		// Solid art never goes far below opaque; shaped art reaches alpha 0. A board sitting between the
		// two would be artwork quietly fading out, which nothing in the game does.
		const int SolidFloor = 224;

		var between = new List<string>();
		var solid = 0;
		var cutOut = 0;

		foreach ( var (path, sign) in Signs() )
		{
			if ( sign.Image is not { } image )
				continue;

			var lowest = 255;

			for ( int y = 0; y < SignFile.ArtworkHeight && y < image.Height; ++y )
			{
				var row = y * image.Width * 4;

				for ( int x = 0; x < image.Width; ++x )
					lowest = Math.Min( lowest, image.Data[row + (x * 4) + 3] );
			}

			if ( lowest == 0 )
				++cutOut;
			else if ( lowest >= SolidFloor )
				++solid;
			else
				between.Add( $"{Path.GetFileName( path )} min={lowest}" );
		}

		Assert.AreEqual( 0, between.Count,
			$"{between.Count} painted boards are neither solid nor a cut-out: {string.Join( " | ", between.Take( 25 ) )}" );

		Assert.AreEqual( 15, solid, "painted boards that never go below alpha 224" );
		Assert.AreEqual( 8, cutOut, "painted boards shaped by a real cut-out" );
	}
}
