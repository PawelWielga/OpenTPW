using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVeldrid;
using System.IO;
using System.Linq;

namespace OpenTPW.Tests;

/// <summary>
/// Which faces of a model are drawn: the original culls every model face itself, keeping those whose screen corners
/// wind clockwise (<c>Face_KeptByWinding</c>, <c>0x00581320</c>; <c>docs/exe/render-states.md</c>, "The software
/// back-face test"), and tests depth less or equal (<c>0x00573614</c>). So of a sheet authored two-sided, the same three
/// points twice wound opposite ways, only the face turned to the eye is drawn, and lit its own way (Q586).
/// </summary>
/// <remarks><b>Mutations:</b> the model's flags given <see cref="MaterialFlags.DisableCulling"/> back; the pipeline culling
/// nothing, or the front faces; the depth test less.</remarks>
[TestClass]
public class ModelFaceCullingTests
{
	/// <summary>A model's materials cull, whatever the caller asked: <c>DisableCulling</c> is taken off, the rest kept.</summary>
	[TestMethod]
	public void AModelsMaterialsCull()
	{
		Assert.AreEqual( MaterialFlags.None, LobbyModel.DrawFlags( MaterialFlags.None ) );
		Assert.AreEqual( MaterialFlags.DisableDepthWrite, LobbyModel.DrawFlags( MaterialFlags.DisableCulling | MaterialFlags.DisableDepthWrite ) );
	}

	/// <summary>A material that culls drops the faces wound against clockwise; one that asks for both faces drops none.</summary>
	[TestMethod]
	public void ThePipelineKeepsClockwiseFaces()
	{
		Assert.AreEqual( FaceCullMode.Back, Material.CullModeFor( MaterialFlags.None ) );
		Assert.AreEqual( FaceCullMode.Back, Material.CullModeFor( MaterialFlags.DisableDepthWrite ) );
		Assert.AreEqual( FaceCullMode.None, Material.CullModeFor( MaterialFlags.DisableCulling ) );
		Assert.AreEqual( FrontFace.Clockwise, Material.KeptWinding );
	}

	/// <summary>Less or equal, so a face at the depth of one drawn before it replaces it.</summary>
	[TestMethod]
	public void TheLaterOfTwoFacesAtOneDepthIsDrawn()
	{
		Assert.AreEqual( ComparisonKind.LessEqual, Material.DepthComparisonFor( MaterialFlags.None ) );
		Assert.AreEqual( ComparisonKind.LessEqual, Material.DepthComparisonFor( MaterialFlags.DisableCulling ) );
		Assert.AreEqual( ComparisonKind.LessEqual, Material.DepthComparisonFor( MaterialFlags.DisableDepthWrite ) );
	}

	/// <summary>A material that neither tests nor writes depth compares nothing.</summary>
	[TestMethod]
	public void NoDepthComparesNothing()
		=> Assert.AreEqual( ComparisonKind.Always, Material.DepthComparisonFor( MaterialFlags.DisableDepth ) );

	/// <summary>
	/// <b>The Sun God's plates are eight two-sided pairs, each underside first</b>: <c>incagod.MD2</c>'s <c>Body</c>,
	/// material <c>Inc_Saucer</c>, faces 124 to 139. Each pair stands on one set of three points wound opposite ways;
	/// the first face's normal (the table entry it names) points down the mesh's z, the second's up it, and the node's
	/// turn (120 degrees about -(1, 1, 1)) takes the mesh's z to the park's up. Drawn both, the first at a depth wins
	/// under a less test and the plates are dark; culled, the lit top is the one seen from above.
	/// </summary>
	[TestMethod]
	public void TheSunGodsPlatesAreTwoSidedUndersideFirst()
	{
		var data = GameData.Required();
		var model = new ModelFile( new MemoryStream( data.ReadAllBytes( "levels/jungle/rides/incagod/incagod.MD2" ) ) );
		var body = model.Meshes.Single( mesh => mesh.Name.TrimEnd( '\0', ' ' ) == "Body" );
		var saucer = body.Materials.Single( material => material.Name.StartsWith( "Inc_Saucer", System.StringComparison.OrdinalIgnoreCase ) );

		Assert.AreEqual( 124, (int)saucer.a, "the plates' faces start at 124" );
		Assert.AreEqual( 16, (int)saucer.b, "the two plates are sixteen faces" );

		for ( int face = saucer.a; face < saucer.a + saucer.b; face += 2 )
		{
			var under = Corners( body, face );
			var over = Corners( body, face + 1 );

			CollectionAssert.AreEquivalent( under, over, $"faces {face} and {face + 1} stand on the same three points" );
			Assert.IsTrue( Facing( under ).Dot( Facing( over ) ) < 0f, $"faces {face} and {face + 1} are wound opposite ways" );

			var first = body.FaceNormals[body.FaceNormalIndices[face]];
			var second = body.FaceNormals[body.FaceNormalIndices[face + 1]];

			Assert.IsTrue( first.Z < -0.8f, $"face {face}'s normal {first} points down the mesh's z" );
			Assert.IsTrue( second.Z > 0.8f, $"face {face + 1}'s normal {second} points up it" );
		}
	}

	private static Vector3[] Corners( ModelFile.Mesh mesh, int face )
		=> [.. Enumerable.Range( 0, 3 ).Select( corner => mesh.Vertices[mesh.Indices[(face * 3) + corner]].Position )];

	private static Vector3 Facing( Vector3[] corners )
		=> Vector3.Cross( corners[1] - corners[0], corners[2] - corners[0] );
}
