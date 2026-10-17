using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTPW.UI;

namespace OpenTPW.Tests;

/// <summary>
/// The person under the pointer's say over a click on the park, before the ground's categories (<c>FUN_00486d90</c>,
/// <c>0x00486fd0</c> to <c>0x0048722b</c>; <c>docs/exe/park-engine.md</c>, "Type 1, the idle mode, does nothing at all").
/// </summary>
[TestClass]
public class ParkPersonPickTests
{
	private const ParkLocator.Kind None = ParkLocator.Kind.None, Staff = ParkLocator.Kind.Staff, Visitors = ParkLocator.Kind.Visitors;
	private const Input.Modifiers Plain = Input.Modifiers.None, Shift = Input.Modifiers.Shift, Control = Input.Modifiers.Control;

	/// <summary>
	/// <b>The visitors' locator with no modifier, or Shift alone with none open, takes a guest or nothing</b>; otherwise a
	/// member is theirs, and an open locator leaves nothing to the ground.
	/// </summary>
	/// <remarks><b>Mutations:</b> either half of the guest test; Shift with Control taken as Shift alone; the locator's stop.</remarks>
	[TestMethod]
	public void TheLocatorAndTheModifiersDecideBeforeTheGround()
	{
		(ParkLocator.Kind Locator, Input.Modifiers Held, bool Guest, bool Member, Level.PersonPick Pick)[] table =
		[
			(None, Plain, false, false, Level.PersonPick.Further),
			(None, Plain, true, false, Level.PersonPick.Further),
			(None, Plain, false, true, Level.PersonPick.Member),
			(None, Shift, true, false, Level.PersonPick.Guest),
			(None, Shift, false, true, Level.PersonPick.Nothing),
			(None, Shift, false, false, Level.PersonPick.Nothing),
			(None, Shift | Control, true, false, Level.PersonPick.Further),
			(None, Control, false, false, Level.PersonPick.Further),
			(Staff, Plain, false, false, Level.PersonPick.Nothing),
			(Staff, Plain, true, false, Level.PersonPick.Nothing),
			(Staff, Plain, false, true, Level.PersonPick.Member),
			(Staff, Shift, true, false, Level.PersonPick.Nothing),
			(Visitors, Plain, true, false, Level.PersonPick.Guest),
			(Visitors, Plain, false, true, Level.PersonPick.Nothing),
			(Visitors, Plain, false, false, Level.PersonPick.Nothing),
			(Visitors, Shift, true, false, Level.PersonPick.Nothing),
			(Visitors, Control, false, true, Level.PersonPick.Member),
			(Visitors, Control, true, false, Level.PersonPick.Nothing),
		];

		foreach ( var row in table )
			Assert.AreEqual( row.Pick, Level.PickPerson( row.Locator, row.Held, row.Guest, row.Member ), row.ToString() );
	}
}
