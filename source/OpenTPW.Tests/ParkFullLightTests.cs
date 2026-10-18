using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OpenTPW.Tests;

/// <summary>
/// Low quality's <c>GraphicalOptions.LIGHTING</c> 0 gives every vertex of the ground and of every model the full colour
/// in place of the sun and the ambient (docs/exe/park-engine.md, "What <c>GraphicalOptions.LIGHTING</c> 0 turns off").
/// </summary>
[TestClass]
public class ParkFullLightTests
{
	/// <summary>Only low.sam turns the light off; med.sam and high.sam keep it.</summary>
	/// <remarks><b>Mutation:</b> the value's test turned round, or the file's key ignored, fails one of the three.</remarks>
	[TestMethod]
	public void OnlyTheLowDetailFileTurnsTheLightOff()
	{
		FileSystem = GameData.Required();
		Log = new();

		Assert.IsFalse( Level.Lighting( "low.sam" ) );
		Assert.IsTrue( Level.Lighting( "med.sam" ) );
		Assert.IsTrue( Level.Lighting( "high.sam" ) );
	}

	/// <summary>The full light is asked only by a park whose light is off: never by the lobby, nor by a lit park.</summary>
	/// <remarks><b>Mutation:</b> the test on <c>Lit</c> dropped or turned round fails.</remarks>
	[TestMethod]
	public void OnlyAnUnlitParkAsksForTheFullLight()
	{
		var ambient = new Vector3( 0.333f, 0.333f, 0.408f );
		var travels = new Vector3( 0.4f, 0.4f, -0.8f );

		Assert.AreEqual( 0f, ParkLight.FullLight( null ) );
		Assert.AreEqual( 0f, ParkLight.FullLight( new ParkLight( ambient, travels ) ) );
		Assert.AreEqual( 1f, ParkLight.FullLight( new ParkLight( ambient, travels, Lit: false ) ) );
	}

	/// <summary>
	/// The model shader makes a corner white under the switch, inside the park light and ahead of the sum and the half
	/// light, so neither the sun, the ambient nor the halving reaches it.
	/// </summary>
	/// <remarks><b>Mutation:</b> the white made another colour, the early return dropped, or the branch moved after the
	/// sum, fails.</remarks>
	[TestMethod]
	public void TheModelShaderDrawsEveryCornerWhiteUnderTheSwitch()
	{
		var text = File.ReadAllText( Path.Combine( System.AppContext.BaseDirectory, "content", "shaders", "test.shader" ) );

		const string branch = "if (g_oUbo.g_flFullLight > 0.5)\n            {\n                vParkLight = vec3(1.0);\n                return;\n            }";
		StringAssert.Contains( text, branch );

		var at = text.IndexOf( branch );
		Assert.IsTrue( text.IndexOf( "if (g_oUbo.g_flParkLight > 0.5)" ) < at, "inside the park light" );
		Assert.IsTrue( at < text.IndexOf( "vec3 lit = g_oUbo.g_vAmbientColour" ), "ahead of the sum" );
		Assert.AreEqual( 2, text.Split( "float g_flFullLight;" ).Length - 1, "both stages' blocks carry it" );
	}
}
