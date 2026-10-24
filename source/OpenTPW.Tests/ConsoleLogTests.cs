using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

[TestClass]
public class ConsoleLogTests
{
	private static string Scratch() => Directory.CreateTempSubdirectory( "tpw-consolelog" ).FullName;

	[TestMethod]
	public void TheFirstLaunchWritesConsoleLogWithTheHeaderThenWhatWasHeld()
	{
		var folder = Scratch();

		try
		{
			using var sink = new ConsoleLog.Sink();
			var tee = new ConsoleLog.Tee( new StringWriter(), sink );

			tee.WriteLine( "before the folder is known" );
			Assert.IsTrue( sink.Open( [folder], "HEADER" ) );
			tee.WriteLine( "after" );

			var lines = File.ReadAllLines( Path.Join( folder, ConsoleLog.FileName ) );

			CollectionAssert.AreEqual( new[] { "HEADER", "before the folder is known", "after" }, lines );
			Assert.IsFalse( File.Exists( Path.Join( folder, ConsoleLog.PreviousName ) ) );
		}
		finally { Directory.Delete( folder, true ); }
	}

	[TestMethod]
	public void TheNextLaunchKeepsTheOldFileAsPreviousAndReplacesAnOlderOne()
	{
		var folder = Scratch();

		try
		{
			for ( var run = 1; run <= 3; run++ )
			{
				using var sink = new ConsoleLog.Sink();
				sink.Open( [folder], $"run {run}" );
				sink.Write( $"body {run}\n" );
			}

			Assert.AreEqual( "run 3\nbody 3\n", File.ReadAllText( Path.Join( folder, ConsoleLog.FileName ) ).ReplaceLineEndings( "\n" ) );
			Assert.AreEqual( "run 2\nbody 2\n", File.ReadAllText( Path.Join( folder, ConsoleLog.PreviousName ) ).ReplaceLineEndings( "\n" ) );
		}
		finally { Directory.Delete( folder, true ); }
	}

	[TestMethod]
	public void AFolderThatCannotBeWrittenFallsThroughToTheNext()
	{
		var folder = Scratch();

		try
		{
			// A file where the first folder should be: the folder cannot be made.
			var blocked = Path.Join( folder, "blocked" );
			File.WriteAllText( blocked, "x" );

			using var sink = new ConsoleLog.Sink();

			Assert.IsTrue( sink.Open( [blocked, Path.Join( folder, "own" )], "H" ) );
			Assert.AreEqual( Path.Join( folder, "own", ConsoleLog.FileName ), sink.Path );
			StringAssert.Contains( File.ReadAllText( sink.Path ), "cannot be written" );
			StringAssert.Contains( sink.Failures, "blocked" );
		}
		finally { Directory.Delete( folder, true ); }
	}

	[TestMethod]
	public void TheVariableNamesTheOnlyFolderAndOtherwiseTheGamesThenThePlayers()
	{
		CollectionAssert.AreEqual( new[] { "/run" }, ConsoleLog.Candidates( "/game", "/run", "/user" ) );
		CollectionAssert.AreEqual( new[] { "/game", "/user" }, ConsoleLog.Candidates( "/game", null, "/user" ) );
		CollectionAssert.AreEqual( new[] { "/game", "/user" }, ConsoleLog.Candidates( "/game", "  ", "/user" ) );
	}

	[TestMethod]
	public void EveryWayOfWritingReachesBothTheTerminalAndTheFile()
	{
		var folder = Scratch();

		try
		{
			var terminal = new StringWriter();
			using var sink = new ConsoleLog.Sink();
			var tee = new ConsoleLog.Tee( terminal, sink );

			sink.Open( [folder], "" );
			tee.Write( 'a' );
			tee.Write( "b" );
			tee.Write( new[] { 'c', 'd', 'e' }, 1, 2 );
			tee.Write( "f".AsSpan() );

			Assert.AreEqual( "abdef", terminal.ToString() );
			Assert.AreEqual( "abdef", File.ReadAllText( Path.Join( folder, ConsoleLog.FileName ) ).ReplaceLineEndings( "" ) );
		}
		finally { Directory.Delete( folder, true ); }
	}
}
