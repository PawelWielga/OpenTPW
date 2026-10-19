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

	/// <summary>The park's light is lit by the detail file the park runs on: low.sam's LIGHTING 0 reaches <c>ParkLight.Lit</c>.</summary>
	/// <remarks><b>Mutation:</b> <c>Lighting( detailFile )</c> in <c>ParkLightFor</c> replaced by <c>true</c> fails.</remarks>
	[TestMethod]
	public void TheParkLightIsLitByTheDetailFileItRunsOn()
	{
		FileSystem = GameData.Required();
		Log = new();

		var ambient = new Vector3( 0.333f, 0.333f, 0.408f );
		var travels = new Vector3( 0.4f, 0.4f, -0.8f );

		Assert.IsFalse( Level.ParkLightFor( ambient, travels, "low.sam" ).Lit );
		Assert.IsTrue( Level.ParkLightFor( ambient, travels, "med.sam" ).Lit );
		Assert.AreEqual( ambient, Level.ParkLightFor( ambient, travels, "high.sam" ).Ambient );
	}

	/// <summary>The uniforms a draw hands the shader carry the park's light: the full light only from an unlit park, none outside a park.</summary>
	/// <remarks><b>Mutation:</b> <c>g_flFullLight</c> not written, or written from the wrong light, or <c>g_flParkLight</c> left
	/// at nought, fails.</remarks>
	[TestMethod]
	public void TheUniformsCarryTheParkLightAndItsFullLight()
	{
		var ambient = new Vector3( 0.333f, 0.333f, 0.408f );
		var travels = new Vector3( 0.4f, 0.4f, -0.8f );

		var lit = ModelEntity.WithParkLight( default, new ParkLight( ambient, travels ) );
		Assert.AreEqual( (1f, 0f, ambient, travels), (lit.g_flParkLight, lit.g_flFullLight, lit.g_vAmbientColour, lit.g_vLightTravels) );

		var low = ModelEntity.WithParkLight( default, new ParkLight( ambient, travels, Lit: false ) );
		Assert.AreEqual( (1f, 1f), (low.g_flParkLight, low.g_flFullLight) );

		var none = ModelEntity.WithParkLight( default, null );
		Assert.AreEqual( (0f, 0f, Vector3.Zero), (none.g_flParkLight, none.g_flFullLight, none.g_vAmbientColour) );
	}
}
