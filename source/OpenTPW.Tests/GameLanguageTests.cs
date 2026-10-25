using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

[TestClass]
public class GameLanguageTests
{
	[TestMethod]
	public void PolishCanBeRequestedWithoutAnEnglishFolder()
	{
		Assert.AreEqual( "Polish", GameLanguage.Select( new[] { "Polish" }, "pl", "en-US" ) );
		Assert.AreEqual( "Polish", GameLanguage.Select( new[] { "English", "Polish" }, "auto", "pl-PL" ) );
	}

	[TestMethod]
	public void AutomaticSelectionFallsBackDeterministically()
	{
		Assert.AreEqual( "English", GameLanguage.Select( new[] { "Polish", "English" }, null, "ja-JP" ) );
		Assert.AreEqual( "American", GameLanguage.Select( new[] { "American" }, "en", "en-GB" ) );
		Assert.AreEqual( "pOLiSh", GameLanguage.Select( new[] { "pOLiSh" }, "POLISH", "en-US" ) );
	}

	[TestMethod]
	public void ExplicitUnavailableLanguagesAreReported()
	{
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Select( new[] { "English" }, "pl", "en-US" ) );
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Select( Array.Empty<string>(), "auto", "pl-PL" ) );
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Select( new[] { "Polish" }, "../Polish", "pl-PL" ) );
	}

	[TestMethod]
	public void CommandLineOverridesEnvironmentAndRequiresAValue()
	{
		Assert.AreEqual( "pl", GameLanguage.Requested( new[] { "--language", "pl" }, "en" ) );
		Assert.AreEqual( "pl", GameLanguage.Requested( new[] { "--language=pl" }, null ) );
		Assert.AreEqual( "pl", GameLanguage.Requested( Array.Empty<string>(), "pl" ) );
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Requested( new[] { "--language" }, null ) );
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Requested( new[] { "--language=" }, null ) );
		Assert.ThrowsException<ArgumentException>( () => GameLanguage.Requested( new[] { "--language", "--game" }, null ) );
	}
}
