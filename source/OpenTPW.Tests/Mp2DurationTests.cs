using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// <see cref="MP2File.Duration"/> reads the bitrate from the stream's own frame header, so it has to read the
/// header's version as well as its layer: MPEG-1 and MPEG-2 do not share a bitrate table. Read against every
/// shipped bank, skipped where there is no installation - see <see cref="GameData"/>.
/// </summary>
[TestClass]
public class Mp2DurationTests
{
	private static List<(string Bank, MP2File Entry)> EveryEntry()
	{
		GameData.Required();

		return Directory.EnumerateFiles( GameDir.Data, "*.sdt", SearchOption.AllDirectories )
			.SelectMany( path =>
			{
				using var archive = new SdtArchive( path );
				return archive.soundFiles.Select( entry => (Path.GetRelativePath( GameDir.Data, path ), entry) ).ToList();
			} )
			.ToList();
	}

	/// <summary>What the bank's header says the game plays, in seconds: 16-bit PCM at the entry's rate.</summary>
	private static double Played( MP2File entry )
	{
		var channels = entry.SoundType == MP2File.SoundTypes.MP2_STEREO ? 2 : 1;
		return entry.DecodedSize / (2.0 * channels * entry.SampleRate);
	}

	[TestMethod]
	public void AMpeg1HeaderTakesMpeg1sTable()
	{
		// ffff22c0: MPEG-1 Layer I, bitrate index 2 = 64 kbit/s. 8000 bytes of it are one second.
		var entry = new MP2File( 0, "x", [0xFF, 0xFF, 0x22, 0xC0, .. new byte[7996]], 22050, 16, 36, 0, [] );
		Assert.AreEqual( 1.0, entry.Duration.TotalSeconds, 1e-9 );

		var mpeg2 = new MP2File( 0, "x", [0xFF, 0xF7, 0x22, 0xC0, .. new byte[5996]], 22050, 16, 36, 0, [] );
		Assert.AreEqual( 1.0, mpeg2.Duration.TotalSeconds, 1e-9, "MPEG-2 Layer I index 2 is 48 kbit/s" );
	}

	[TestMethod]
	public void EveryShippedEntrysDurationAgreesWithWhatTheGamePlays()
	{
		var entries = EveryEntry();
		var mpeg1 = entries.Where( e => e.Entry.SoundData.Length > 4 && ((e.Entry.SoundData[1] >> 3) & 3) == 3 )
			.Select( e => $"{e.Bank.Replace( '\\', '/' )}:{e.Entry.Name}" ).Order().ToList();
		var worst = double.MinValue;
		var least = double.MaxValue;
		var checkedCount = 0;

		foreach ( var (bank, entry) in entries )
		{
			if ( entry.Duration == TimeSpan.Zero || entry.DecodedSize <= 0 )
				continue;

			var gap = (entry.Duration.TotalSeconds - Played( entry )) * 1000.0;
			worst = Math.Max( worst, gap );
			least = Math.Min( least, gap );
			checkedCount++;
		}

		CollectionAssert.AreEqual( new[] { "global/sound/RideHD.sdt:mortar_2.mp2", "global/sound/RideHD.sdt:mortar_3.mp2",
			"global/sound/RideHD.sdt:tp_riderepaired", "global/sound/RideHD.sdt:tp_sideshowwin_", "global/sound/SfxHD.sdt:keyexplode.mp2" },
			mpeg1, "the MPEG-1 entries" );
		Assert.AreEqual( entries.Count, checkedCount, "every entry has a duration and a played length" );

		// Measured: the stream is always the longer, by 14.4 to 78.5 ms (an encoder's delay and the padding of its last frame).
		Assert.IsTrue( least >= 0 && worst < 90, $"the gaps run {least:F1} to {worst:F1} ms" );
	}
}
