using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// The pixel of each cursor's picture that stands on the pointer, read from the shipped <c>.ani</c> files. The
/// crosshair first person wears is the one cursor drawn about its middle (Q498's review: drawn by its top-left
/// corner, its cross stood 16 px right of and below the pointer).
/// </summary>
[TestClass]
public class CursorHotspotTests
{
	/// <summary>Every <c>.ani</c> the game ships reads, and only <c>c_crosshair.ani</c>'s hotspot is not (0, 0).</summary>
	[TestMethod]
	public void OnlyTheCrosshairStandsByItsMiddle()
	{
		GameData.Required();

		var folder = GameDir.GetPath( "/data/ui/cursors" );
		var files = Directory.GetFiles( folder, "*.ani" ).Order().ToArray();

		Assert.AreEqual( 21, files.Length, "the shipped cursors" );

		foreach ( var file in files )
		{
			using var stream = File.OpenRead( file );
			var expected = Path.GetFileName( file ) == "c_crosshair.ani" ? (16, 16) : (0, 0);

			Assert.AreEqual( expected, new AnimatedCursorFile( stream ).Hotspot, Path.GetFileName( file ) );
		}
	}

	/// <summary>Each cursor the game draws finds its own file: the crosshair's middle, the rest their corner.</summary>
	[TestMethod]
	public void EachCursorTakesItsOwnFilesHotspot()
	{
		GameData.Required();

		foreach ( var cursor in Enum.GetValues<Input.CursorTypes>() )
		{
			var expected = cursor == Input.CursorTypes.Crosshair ? (16, 16) : (0, 0);

			Assert.AreEqual( expected, UI.Cursor.HotspotOf( cursor ), cursor.ToString() );
		}
	}
}
