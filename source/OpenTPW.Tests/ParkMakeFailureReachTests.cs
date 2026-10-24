using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenTPW.Tests;

/// <summary>
/// The reach of a make that fails after it took its thing id (Q615): <see cref="ParkBuilding.Build"/> past
/// <c>PlaceNow</c> and <see cref="ParkEndFeature.Make"/> past <c>BindFixed</c>. Every Lost Kingdom item is put through the
/// script load <c>ParkRides.Load</c> makes; with the running game's census of <c>ParkObjects.Place</c> it decides whether
/// the two returns can be reached with content the game ships.
/// </summary>
[TestClass]
public class ParkMakeFailureReachTests
{
	private BaseFileSystem data = null!;

	[TestInitialize]
	public void MountTheGame() => FileSystem = data = GameData.Required();

	/// <summary>
	/// Every item a buy list shows (UI type 0 to 3, as <see cref="ParkBuilding"/> admits) has a script that reads, the
	/// load <c>ParkRides.Load</c> makes: so a purchase cannot meet a script that will not run, and the End, the one item
	/// <see cref="ParkRides.BindFixed"/> is asked for, cannot reach its "script would not run" return. The model half
	/// (<c>ParkObjects.Place</c>) needs a graphics device and is counted in the running game (<c>q615/</c>).
	/// </summary>
	[TestMethod]
	public void EveryBuyableItemsScriptReads()
	{
		var catalogue = new ParkItemCatalogue( "jungle", data );
		var failures = new List<string>();
		var seen = 0;

		foreach ( var item in catalogue.All.Where( i => i.UiType is >= 0 and <= ItemDescriptionFile.Feature ) )
		{
			++seen;

			using var stream = data.OpenRead( ParkRides.ScriptPathFor( item ) );

			if ( stream == null || !new RideScriptFile( stream ).IsValid )
				failures.Add( $"{item.Name} ({item.Id})" );
		}

		Console.WriteLine( "Q615 ids: " + string.Join( " ", catalogue.All.Where( i => i.UiType is >= 0 and <= ItemDescriptionFile.Feature ).Select( i => i.Id ).Order() ) );
		Console.WriteLine( $"Q615: {seen} buyable items, {failures.Count} scripts that do not read" );
		Assert.IsTrue( seen > 20, "the sweep saw the catalogue" );
		Assert.AreEqual( 0, failures.Count, string.Join( "; ", failures ) );
		Assert.IsNotNull( ParkEndFeature.Find( catalogue ), "the End is in the catalogue" );
	}

	/// <summary>The End's script reads: the one <see cref="ParkRides.BindFixed"/> is asked for.</summary>
	[TestMethod]
	public void TheEndsScriptReads()
	{
		var end = ParkEndFeature.Find( new ParkItemCatalogue( "jungle", data ) )!;

		using var stream = data.OpenRead( ParkRides.ScriptPathFor( end.Value ) );

		Assert.IsTrue( new RideScriptFile( stream ).IsValid );
	}
}
