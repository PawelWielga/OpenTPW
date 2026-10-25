using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OpenTPW.Tests;

/// <summary>
/// The pointer is held apart from the interface's children and drawn by its own call, which <see cref="Level"/> makes
/// after every other draw of the frame, the object windows' model previews and the advisor included (Q634; the
/// original's cursor library draws it just before the flip, <c>0x00564210</c>, <c>docs/exe/hud.md</c>, "The pointer is
/// drawn last"). That <see cref="Level"/> makes the call last is seen in the running game, not here: no test can build
/// a level.
/// </summary>
[TestClass]
public class PointerDrawnLastTests
{
	private sealed class Recorder : UI.Panel
	{
		private readonly List<string> _drawn;
		private readonly string _name;

		public Recorder( List<string> drawn, string name )
		{
			_drawn = drawn;
			_name = name;
		}

		protected override void OnRender() => _drawn.Add( _name );
	}

	private bool _hidden;

	[TestInitialize]
	public void Begin()
	{
		_hidden = UI.RootPanel.Hidden;
		UI.RootPanel.Hidden = false;
	}

	[TestCleanup]
	public void End() => UI.RootPanel.Hidden = _hidden;

	/// <summary>
	/// <b>The interface's pass draws its children and not the pointer; the pointer's call draws the pointer alone.</b>
	/// </summary>
	/// <remarks>
	/// <b>Mutations:</b> <see cref="UI.RootPanel.Render"/> drawing the pointer with the children (the pointer drawn
	/// twice, the first time under the previews), and <see cref="UI.RootPanel.RenderPointer"/> drawing nothing or
	/// drawing the children again, each fail this.
	/// </remarks>
	[TestMethod]
	public void ThePointerIsDrawnOnlyByItsOwnCall()
	{
		var drawn = new List<string>();
		var hud = new UI.RootPanel();
		hud.AddChild( new Recorder( drawn, "window" ) );
		hud.AddChild( new Recorder( drawn, "particles" ) );
		hud.Pointer = new Recorder( drawn, "pointer" );

		hud.Render();
		CollectionAssert.AreEqual( new[] { "window", "particles" }, drawn, "the interface's pass leaves the pointer out" );

		hud.RenderPointer();
		CollectionAssert.AreEqual( new[] { "window", "particles", "pointer" }, drawn, "the pointer's call draws it, once" );
	}

	/// <summary><b>F2's hidden interface hides the pointer too</b>, as it did when the pointer was the interface's last child.</summary>
	/// <remarks><b>Mutation:</b> <see cref="UI.RootPanel.RenderPointer"/> without its test of <see cref="UI.RootPanel.Hidden"/> fails this.</remarks>
	[TestMethod]
	public void AHiddenInterfaceDrawsNoPointer()
	{
		var drawn = new List<string>();
		var hud = new UI.RootPanel { Pointer = new Recorder( drawn, "pointer" ) };

		UI.RootPanel.Hidden = true;
		hud.Render();
		hud.RenderPointer();

		Assert.AreEqual( 0, drawn.Count, "nothing is drawn with the interface hidden" );
	}

	/// <summary>An interface with no pointer (none is made before the level's interface is set up) draws without one.</summary>
	[TestMethod]
	public void NoPointerDrawsNothing()
	{
		var hud = new UI.RootPanel();
		hud.RenderPointer();
		Assert.IsNull( hud.Pointer );
	}
}
