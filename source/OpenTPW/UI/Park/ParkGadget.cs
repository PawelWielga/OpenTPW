namespace OpenTPW.UI;

/// <summary>
/// The park's management gadget - the arm along the bottom-left corner with the date, the visitors'
/// happiness gauge and the six round buttons.
///
/// <para>
/// <b>Where the layout comes from.</b> FUN_004a1d70 builds it from the compiled layout stream at
/// 0x00752940, one of six panels FUN_00489f50 loads when a park's interface is assembled. Every
/// rectangle and every id below is that stream's, walked opcode by opcode, and every mesh is named by
/// the hash the stream carries: the original files each ui.wad model under the name of its first mesh
/// node and asks for it as <c>h = (c ^ h) * 47</c> from zero, so 0x2135e13d is <c>b_buy</c> and
/// 0x1beb0695 is <c>base</c>, which is the model shipped as <c>mainpanel.md2</c>. The stream also gives
/// each control its row of UIHELPTEXT.str, which is what the help bar says over it.
/// </para>
/// <para>
/// <b>It is six category pickers, not seventeen buttons.</b> The gadget's handler FUN_004a0840 sends
/// three of the six into FUN_004a0940, which opens <i>the screen that category was last left on</i> out
/// of a remembered-tab global - buy or hire, four information screens, four finance screens. The other
/// three go straight to one thing each. That is why the seventeen keyboard shortcut actions never
/// reconciled with the gadget's button count: they were never meant to. The help rows say the same
/// thing in the game's own words - 469 is "buy and build attractions or to hire staff", 470 "view
/// information screens", 471 "view finance screens".
/// </para>
/// <para>
/// <b>What is here and what is not.</b> The gadget draws, the date is live and the buttons light and
/// click. <b>Five of the six open something</b>: buy and hire, the map, the camcorder arm, the
/// Information category's staff/items/visitors lists, and the Money category's entry price. <b>Only
/// Research does nothing</b> - it is not a category, and neither its six effort sliders nor its two
/// message boxes are built - so it says so in the log and counts
/// each click as <c>RESEARCH_BUTTON</c>. <b>The gauge is live.</b> There is a happiness on every guest, and
/// FUN_004c7bb0 is what the original does with them - average them, and read nought while the park is
/// shut, which is what still leaves it resting at its lowest part in a park nobody has entered.
/// </para>
/// <para>
/// <b>The golden keys and tickets in the top right are real.</b> The stream's fourth root control,
/// 0x33, holds two rows of icon-and-count - the ticket 0x35 with its number 0x37, the key 0x34 with its
/// number 0x36 - and both numbers already exist here, because a player's gms.dat has carried them since
/// the lobby was built. So this one is not chrome: it shows what the player actually has, in the format
/// the original seeds it with, "0 x". The key row goes away entirely in Instant Action, which is what
/// FUN_004a1d70 does with it, and is why <see cref="IslandPanel"/> hides its own.
/// </para>
/// <para>
/// <b>The arm is a carrier, not a lid.</b> 0x21, with 0x22 and 0x23 on its far end and the retract
/// button 0x24 beside them. Nothing reaches it except through FUN_004a2590, and every one of that
/// function's five callers hands it a panel of its own to hold - so the arm is how this gadget shows any
/// sub-panel at all, and folding it away is how each of them is put back. The one whose contents exist
/// today is the camcorder panel, stream 0x00751720, which the camcorder button puts on it and either of
/// that panel's two buttons takes off again.
/// </para>
/// <para>
/// <b>The message bar</b> has a stream of its own at 0x007501a0 - a messagel/messagem/messager three-slice with
/// two stacked buttons and an mb_erase button under them - and the arm carries it as it carries the camcorder
/// panel, 820 out where the bar is 615 wide. A tag's left click puts its message on it (FUN_0048d9c0): the tag's
/// text, its row's two buttons, of which mb_locate puts the camera on the tag's thing, and the erase button, which
/// takes the tag off. The same tag clicked again, its right click, its going or the retract button folds the arm
/// away (docs/exe/hud.md, "The message bar").
/// </para>
/// <para>
/// <b>The aerial (0x2d, 0x2e) is the messages control.</b> Each tag a said line leaves (<see cref="ParkTags"/>)
/// is a msgtag icon on the mast, which lifts the red top by the icon's height and ten (LAB_004a11e0, 0x100001); a
/// right click on a tag hides it and takes it off half a second on, and one on the top deletes them all
/// (FUN_004a1190) - its help row says so. A tag's left click opens the message bar, and the tag on show swells
/// and shrinks (the stream at 0x007502b8). The top and the tags above one taken off are put down at once, where
/// the original plays them down (the streams at 0x007502f0 and 0x00752e48).
/// </para>
/// <para>
/// <b>The body, the arm and the aerial stop the pointer</b>, as every shown control of the original's does: the body
/// inside its 23-point outline, the handle inside its 16-point one, the others over their rectangles. A press there
/// never reaches the park (docs/exe/park-engine.md, "Whose a right press is").
/// </para>
/// <para>
/// <b>The bank balance beside it IS built.</b> 0x2f is the lettering and 0x32 the currency icon, and the
/// number is <see cref="ParkState.Balance"/>: the save's own <c>mBalance</c>, moved by every admission
/// fee, charge, cost and refund - one number, as in the original (see <see cref="ShowMoney"/>). What is
/// still out is <b>0x30</b>, the cost of whatever is
/// in your hand, which the original starts hidden and paints yellow because an empty hand has no price;
/// and <b>0x31</b>, the trend arrow, which wants a history of the balance that nothing here keeps. Both
/// reasons are written out where the cluster is built.
/// </para>
/// <para>
/// <b>Engine and content.</b> The controls, the pointer and the help bar are engine. Which buttons a
/// park has, where they sit and what each one does is this file - the park's content, as
/// <see cref="FrontEnd.FrontEnd"/>'s island panel is the lobby's.
/// </para>
/// </summary>
internal sealed class ParkGadget : UiWindow
{
	/// <summary>DATETINY/DATESMALL/DATEBIG - slot 3 is the date's own font in all four resolution sets.</summary>
	private const int DateFont = 3;

	private readonly UiControl _date;

	/// <summary>The camcorder button, which stays down for as long as the arm it opens is out.</summary>
	private readonly UiButton _camcorder;

	/// <summary>The arm, and the panel it carries out of the gadget - see the class remarks.</summary>
	private readonly UiControl _arm;

	/// <summary>The arm's end 0x22, which carries the handle 0x23 and keeps to the arm's right edge.</summary>
	private readonly UiControl _armEnd;

	private readonly UiButton _retract;
	private readonly UiControl _carried;

	/// <summary>The message bar 0x10d2d, its text's panel 0x10d32 and its buttons 0x10d30, 0x10d2f and 0x10d2e.</summary>
	private readonly UiControl _bar;
	private readonly UiControl _barText;
	private readonly UiButton _barFirst;
	private readonly UiButton _barSecond;
	private readonly UiButton _barErase;

	/// <summary>What the arm carries out: the camcorder panel, the message bar, or nothing with the arm in.</summary>
	private UiControl? _onArm;
	private readonly UiMesh? _armMesh;
	private readonly UiMesh? _armEndMesh;

	/// <summary>
	/// How far the arm's right edge comes in: FUN_004a1d70 adds the arm's left less the handle's, 331 - 976, to the arm's
	/// right (0x004a23da) and sets that rectangle, so the arm is built 30 wide and its end and handle, which follow their
	/// parent's right edge (flag 0x20000, FUN_0065c9bd), sit 645 to the left of the stream's place for them.
	/// </summary>
	private const int ArmTravel = 645;

	/// <summary>
	/// How far the arm goes out with the message bar: ten steps of a tenth of the bar's width, the body's right edge
	/// less the arm's left, and a hundred (0x004a16d7): (615 + 439 - 331 + 100) / 10 is 82.
	/// </summary>
	private const int BarTravel = 820;

	/// <summary>How far out the arm's right edge stands now.</summary>
	private int _armTravel = ArmTravel;

	/// <summary>Whether the arm is out. See <see cref="PutArm"/>.</summary>
	internal bool ArmOut => _onArm != null;

	/// <summary>Whether the arm is out with the message bar on it.</summary>
	internal bool BarOut => _onArm == _bar;

	/// <summary>The body's outline - the stream's op 4 sub-op 4 on 0x1d, 23 points at 0x00752ac8.</summary>
	private static readonly UiPoint[] BodyOutline =
	[
		new( 167, 1019 ), new( 258, 1014 ), new( 327, 1015 ), new( 396, 1022 ), new( 415, 1032 ), new( 428, 1057 ),
		new( 439, 1311 ), new( 428, 1343 ), new( 413, 1365 ), new( 389, 1383 ), new( 358, 1391 ), new( 344, 1410 ),
		new( 344, 1499 ), new( 255, 1507 ), new( 150, 1500 ), new( 102, 1482 ), new( 75, 1461 ), new( 48, 1409 ),
		new( 37, 1311 ), new( 64, 1071 ), new( 72, 1046 ), new( 84, 984 ), new( 155, 984 )
	];

	/// <summary>The handle's outline with the arm out - the stream's op 4 sub-op 4 on 0x23, 16 points at 0x00752a42.</summary>
	private static readonly UiPoint[] HandleOutline =
	[
		new( 1060, 1069 ), new( 1071, 1058 ), new( 1093, 1106 ), new( 1113, 1183 ), new( 1129, 1310 ), new( 1120, 1405 ),
		new( 1090, 1461 ), new( 1056, 1486 ), new( 1015, 1503 ), new( 976, 1503 ), new( 976, 1406 ), new( 981, 1392 ),
		new( 992, 1382 ), new( 1024, 1373 ), new( 1045, 1358 ), new( 1062, 1336 )
	];

	/// <summary>
	/// The count beside each icon, in the font the original gives them - FUN_004a1d70 fetches 0x36 and
	/// 0x37 and hands both a label skin, white, and font slot 2. The lobby's key count uses the same
	/// slot for the same reason.
	/// </summary>
	private const int CountFont = 2;

	/// <summary>The key row of the top-right cluster, which Instant Action does without.</summary>
	private readonly UiControl _keyRow;

	private readonly UiControl _keyCount;
	private readonly UiControl _ticketCount;

	/// <summary>
	/// The gauge's moving part - the stream's 0x1f, control type 9. See <see cref="UiMeter"/> for why it
	/// takes a skin rather than a mesh, and <see cref="ParkPeople.AverageHappiness"/> for the number.
	/// </summary>
	private readonly UiMeter _happiness;

	/// <summary>
	/// The bank balance in the top-left corner - the stream's 0x2f, the lettering itself. Its field is
	/// sized in the original by measuring the string "999999999" (0x00752f10) rather than being fixed, so
	/// nine digits is what it is built to hold.
	/// </summary>
	private readonly UiControl _balance;

	/// <summary>
	/// The balance's own font slot - <c>FUN_004a1d70</c> gives 0x2f <c>FUN_00485a70(1)</c>, where the two
	/// counts in the opposite corner get slot 2 and the date gets slot 3.
	/// </summary>
	private const int BalanceFont = 1;

	public ParkGadget( WindowStack stack ) : base( stack )
	{
		// Not modal and not pausing: the park carries on behind it, as the lobby's island panel lets the
		// lobby carry on. A window that paused here would stop the clock, the calendar and every model.
		//
		// The root is a bare container for the stream's four roots: the body, the aerial, the balance and the
		// earned cluster.
		Root = new UiControl { Rect = VirtualScreen.Whole };

		// The gadget body, 0x1d. It takes the pointer inside its outline, and everything under it is drawn in
		// order of depth, because the arm lies BEHIND the body it is a child of. FUN_004a1d70 gives each control a
		// depth through FUN_0065f16b, which writes it to the control's own +0xe0 and hands children two more; the
		// receivers are legible only in the disassembly, because the setter is __fastcall:
		//
		//     0x21 arm 4   0x22 end 4   0x24 b_retract 7   0x1d body 8   0x23 handle 9   the rest 10 and up
		//
		// Higher is nearer the front, which a screenshot settles: the six buttons sit at 10 inside a body at 8 and
		// plainly draw over it. The children are in the stream's order, which is the hit test's: the last child the
		// point is in answers (FUN_0065db25), so the six buttons' panel before the arm, the arm before the date and
		// the gauge, and any of them before the body.
		var body = Root.Add( new UiControl
		{
			Id = 0x1d,
			Rect = new UiRect( 37, 984, 439, 1507 ),
			Mesh = UiMesh.Get( "mainpanel" ),
			Outline = BodyOutline,
			Depth = 8,
			DrawsByDepth = true
		} );

		// gauge.md2 belongs to THIS control, not to the meter inside it. In the stream the mesh record
		// comes after the op that closes 0x1f's block, and UI_ParseTreeStream binds a mesh to the
		// control whose block is running - its case 1 loads ECX from the invocation's own `this`
		// (0x0065fe75), where case 0 recursed with the new child instead. So once 0x1f has closed, the
		// running block is 0x1e's again.
		var gaugeHousing = body.Add( new UiControl
		{
			Id = 0x1e,
			Rect = new UiRect( 67, 1080, 163, 1345 ),
			HelpText = 477,
			Mesh = UiMesh.Get( "gauge" )
		} );

		// Control type 9 in the stream, which is a meter: FUN_0066d750 is the plain control with three
		// more fields, and FUN_004a1cd0 arms a 2000ms timer - id 0x80083 - that fills it. It carries no
		// mesh at all, because the original hands it a meter.wct skin instead (FUN_00477870, from
		// FUN_004a1d70), which is why a control that draws only a mesh shows nothing here.
		//
		// What the timer asks for is
		// FUN_004c7bb0 - the mean happiness of the park's guests, and nought while the park is shut. See
		// ParkPeople.AverageHappiness for the arithmetic and for the one term of it that is deliberately
		// not reproduced, and UiMeter for how far up the skin is drawn and why that part is a choice.
		_happiness = gaugeHousing.Add( new UiMeter
		{
			Id = 0x1f,
			Rect = new UiRect( 85, 1100, 144, 1324 )
		} );

		_date = body.Add( new UiControl
		{
			Id = 0x20,
			Rect = new UiRect( 153, 1044, 404, 1121 ),
			HelpText = 478,
			Mesh = UiMesh.Get( "date" ),

			// The stream gives the lettering its own rectangle inside the panel.
			TextRect = new UiRect( 182, 1061, 383, 1103 ),
			Font = DateFont,

			// Black, which is what the original sets and not the interface's usual white: having
			// fetched control 0x20, FUN_004a1d70 calls the font setter with slot 3 and then the colour
			// setter with (0, 0, 0, 0xff) - `PUSH 0xff` over three pushes of a register holding zero,
			// at 0x004a2529. The panel's white labels go through the same call with 0xffffffff.
			TextColour = UiColour.Black
		} );

		// The arm, which the gadget carries a sub-panel out on: 0x21 with 0x22 inside it, 0x23 inside that
		// and the retract button 0x24 beside them. It is a carrier, not a lid - FUN_004a2590 is the only
		// way anything gets onto it, and each of its five callers hands it a panel of its own to hold.
		//
		// Built here at the stream's rectangles, the arm out, and put in below as the original's builder does.
		_arm = body.Add( new UiControl
		{
			Id = 0x21,
			Rect = new UiRect( 331, 1069, 1006, 1495 ),
			Mesh = _armMesh = UiMesh.Get( "panel" ),
			Depth = 4,
			StopsPointer = true,

			// It reaches out past the body, so UiControl.Add will not let it follow the body's corner and
			// VirtualScreen.AnchorFor would work its own out from its middle instead. That middle is 668
			// across and the left third ends at 683, so it would come out left anyway - but by fourteen
			// units, and an arm bolted to the body should not be held on by a rounding. Its two end pieces
			// are worse: both their middles fall in the middle third, so each would centre itself and the
			// arm would come apart across a wide window.
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		// The far end of the arm, and the handle on the end of that. panel.md2 and panelend.md2 carry FOUR
		// parts each and they are not animation frames: they are pan_money/pan_info/pan_buy/pan_staff and
		// the matching pane_*, so the arm wears the colour of whichever category it is carrying. Nothing on
		// the camcorder path sets a part, so both stay on part 0 - which is the one the layout stream names
		// each model by, and so the one its mesh hash resolves to.
		_armEnd = _arm.Add( new UiControl
		{
			Id = 0x22,
			Rect = new UiRect( 1006, 1069, 1071, 1382 ),
			Mesh = _armEndMesh = UiMesh.Get( "panelend" ),
			Depth = 4,
			StopsPointer = true,
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		// The handle, in front of the body: with the arm in it is what shows past the body's right edge.
		_armEnd.Add( new UiControl
		{
			Id = 0x23,
			Rect = new UiRect( 976, 1058, 1129, 1503 ),
			Mesh = UiMesh.Get( "handle" ),
			Outline = HandleOutline,
			Depth = 9,
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		// "Left-click to close this arm" - UIHELPTEXT row 481. It goes away with the arm, because it is
		// inside it, which is also how it comes to follow the arm's corner without being told to.
		//
		// With the arm in the original switches this button off, calling the control's vtable+0x14 with 0
		// exactly as it does for the readouts that are shown but never clicked, and a control switched off
		// takes no pointer; here it is hidden with what the arm carries (see PutArm). The red cross it shows is not the disabled part of b_retract: b_retract.md2's six
		// nodes are b_retract, disable, hilite, hidown, helddown and down - UiButton's own order - so part
		// 0 is the NORMAL look, and a button that closes something simply looks like a cross.
		_retract = _arm.Add( new UiButton
		{
			Id = 0x24,
			Rect = new UiRect( 349, 1386, 429, 1466 ),
			HelpText = 481,
			Mesh = UiMesh.Get( "b_retract" ),
			Depth = 7,
			Clicked = () => CloseArm()
		} );

		// What the arm carries, out of the camcorder panel's own stream at 0x00751720, which FUN_00498b50
		// builds and parks hidden until FUN_00498bb0 hands it over. Both its buttons close the arm behind
		// them: FUN_00498ad0 acts on the click, withdraws the advisor line the panel posted, and then calls
		// FUN_004a25f0(1) - and that argument is what lifts the camcorder button again.
		var carried = _carried = _arm.Add( new UiControl
		{
			Id = 0x62,
			Rect = new UiRect( 466, 1316, 785, 1469 )
		} );

		carried.Add( new UiButton
		{
			Id = 0x63,
			Rect = new UiRect( 474, 1316, 627, 1469 ),
			HelpText = 479,
			Mesh = UiMesh.Get( "b_1person" ),
			Clicked = EnterCamcorder
		} );

		carried.Add( new UiButton
		{
			Id = 0x64,
			Rect = new UiRect( 632, 1316, 785, 1469 ),
			HelpText = 480,
			Mesh = UiMesh.Get( "b_postcard" ),
			Clicked = SendPostcard
		} );

		// The message bar, stream 0x007501a0 (FUN_0048d8b0), at the place the arm's handler gives it out: its left
		// the arm's left less its own width and fifty, then the arm's 820, and its middle on the arm's (0x004a1613),
		// so (486,1112) where the stream has (458,1101). Three slices, the middle one a label in font 6 whose text
		// rectangle is the stream's; the two stacked buttons carry no mesh until a message is shown.
		_bar = _arm.Add( new UiControl
		{
			Id = 0x10d2d,
			Rect = new UiRect( 486, 1112, 1101, 1453 ),
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		// Each is pinned to the body's corner, as the arm's own pieces are: half the bar lies in the screen's middle third.
		_bar.Add( new UiControl { Id = 0x10d31, Rect = new UiRect( 486, 1112, 651, 1453 ), Mesh = UiMesh.Get( BarLeftMesh ), PinAcross = Anchor.Left, PinDown = VerticalAnchor.Bottom } );

		// The right slice goes in before the middle one, whose lettering runs on over it: the stream has it after.
		_bar.Add( new UiControl { Id = 0x10d33, Rect = new UiRect( 863, 1112, 1101, 1453 ), Mesh = UiMesh.Get( BarRightMesh ), PinAcross = Anchor.Left, PinDown = VerticalAnchor.Bottom } );

		_barText = _bar.Add( new UiControl
		{
			Id = 0x10d32,
			Rect = new UiRect( 651, 1112, 863, 1453 ),
			Mesh = UiMesh.Get( BarMiddleMesh ),
			TextRect = new UiRect( 637, 1129, 1084, 1433 ),
			Font = BarFont,
			TextAcross = TextAlign.Start,
			TextDown = TextAlign.Start,
			TextWraps = true,
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		_barFirst = _bar.Add( new UiButton { Id = 0x10d30, Rect = new UiRect( 514, 1121, 617, 1224 ), Clicked = () => BarButton( first: true ), PinAcross = Anchor.Left, PinDown = VerticalAnchor.Bottom } );
		_barSecond = _bar.Add( new UiButton { Id = 0x10d2f, Rect = new UiRect( 514, 1229, 617, 1331 ), Clicked = () => BarButton( first: false ), PinAcross = Anchor.Left, PinDown = VerticalAnchor.Bottom } );
		_barErase = _bar.Add( new UiButton { Id = 0x10d2e, Rect = new UiRect( 514, 1336, 617, 1439 ), Mesh = UiMesh.Get( BarEraseMesh ), Clicked = EraseTag, PinAcross = Anchor.Left, PinDown = VerticalAnchor.Bottom } );

		_onArm = carried;
		PutArm( null );

		// The six round buttons' panel, 0x25: the last of the body's children, so it answers before the arm where
		// the two meet.
		var buttons = body.Add( new UiControl
		{
			Id = 0x25,
			Rect = new UiRect( 170, 1122, 392, 1372 ),
			StopsPointer = true
		} );

		// The six, in the stream's own order. Each one's help row is the game's description of it.
		// In the original FUN_004a0940( 1 ) opens whichever of buy and hire that category was last left
		// on, from a remembered-tab global seeded to 1 - so buy is what a park opens on, and the hire
		// screen is its sibling rather than a second button. See docs/exe/hud.md. A deviation: this
		// button always opens buy. With the buy screen already up the opener only picks its tab again
		// (0x004acca0), so nothing is closed or opened.
		buttons.Add( new UiButton
		{
			Id = 0x26,
			Rect = new UiRect( 170, 1122, 288, 1240 ),
			HelpText = 469,
			Mesh = UiMesh.Get( "b_buy" ),
			Clicked = () =>
			{
				if ( !Stack.Windows.Any( window => window is ParkBuyScreen ) )
					Stack.Open( new ParkBuyScreen( Stack ) );
			}
		} );

		// It does not enter first person itself. FUN_004a0840's case 0x27 splits on whether the button
		// has just gone down: down calls FUN_00498bb0, which puts the camcorder panel on the arm, and up
		// calls FUN_00498bd0, which takes it off again. The button id 99 that actually enters the mode is
		// on that panel, not here; the C key goes straight there.
		_camcorder = buttons.Add( new UiButton
		{
			Id = 0x27,
			Rect = new UiRect( 202, 1355, 320, 1474 ),
			HelpText = 474,
			Mesh = UiMesh.Get( "b_camera" ),
			Toggles = true,
			Clicked = ShowArm
		} );

		buttons.Add( new UiButton
		{
			Id = 0x28,
			Rect = new UiRect( 287, 1133, 405, 1252 ),
			HelpText = 470,
			Mesh = UiMesh.Get( "b_info" ),
			Clicked = () => ParkCategoryScreens.Open( Stack, ParkCategoryScreens.Information )
		} );

		// The map. FUN_004a0840's case 0x29 goes straight to
		// FUN_005f0b40 rather than through the remembered-tab picker, because the map is not a category
		// - see <see cref="ParkMapScreen"/>, which pauses the park as the original does.
		buttons.Add( new UiButton
		{
			Id = 0x29,
			Rect = new UiRect( 81, 1340, 200, 1458 ),
			HelpText = 473,
			Mesh = UiMesh.Get( "b_map" ),
			Clicked = () => Stack.Open( new ParkMapScreen( Stack ) )
		} );

		// Of this category's four screens only ENTRY PRICE is within reach, and it is the one the
		// category is seeded to - FUN_004a0810(3,10) - so the button opens on it. Finances, loans and
		// staff costs all rest on monthly ring buffers and on loans, which ParkWorld reads and nothing runs, and
		// the entry-price screen carries a button to each of the three regardless, because the original's
		// own screen does. See docs/exe/hud.md.
		buttons.Add( new UiButton
		{
			Id = 0x2a,
			Rect = new UiRect( 156, 1241, 274, 1360 ),
			HelpText = 471,
			Mesh = UiMesh.Get( "b_money" ),
			Clicked = () => ParkCategoryScreens.Open( Stack, ParkCategoryScreens.Finance )
		} );

		// THE LAST ONE STILL DOES NOTHING, and it is not a category: FUN_004a0840's case 0x2b goes straight to
		// FUN_004aa480 rather than through the remembered-tab picker, exactly as the map does. That opens a message
		// box in Instant Action or with no researcher hired, and otherwise six effort sliders over research groups
		// (docs/exe/hud.md, "The nine screens behind Info, Money and Research"). None of the three is built, and
		// research itself waits on a research system this game lacks (docs/PLAYER-GAPS.md). Counted each click.
		buttons.Add( new UiButton
		{
			Id = 0x2b,
			Rect = new UiRect( 274, 1254, 392, 1372 ),
			HelpText = 472,
			Mesh = UiMesh.Get( "b_resrch" ),
			Clicked = () =>
			{
				Unimplemented.Report( "RESEARCH_BUTTON" );
				Log.Info( "Park gadget: Research - neither its screen nor its message boxes are built, so nothing more happens" );
			}
		} );

		// The aerial, the stream's second root: the mast 0x2d (94,918)-(144,984) with the red top 0x2e
		// (77,790)-(161,918) over it. FUN_004a1d70 sets the mast's top eight above its bottom (0x004a2057), and the
		// top follows its parent's top edge (flag 0x40000, FUN_0065c9bd), 58 down. A tag lifts both again: PutTags.
		// Each is pinned to the corner the body keeps to, since neither sits in the body's third of the screen.
		_mast = Root.Add( new UiControl
		{
			Id = 0x2d,
			Rect = new UiRect( MastLeft, MastBottom - 8, MastRight, MastBottom ),
			Mesh = UiMesh.Get( "aerial" ),
			StopsPointer = true,
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom
		} );

		// "Right-click to delete ALL messages" - UIHELPTEXT row 476. FUN_004a1190 answers a right click (0x10006,
		// button 1) by sending the aerial 0x100003, which takes every tag off, a tenth of a second apart.
		_mastTop = _mast.Add( new UiControl
		{
			Id = 0x2e,
			Rect = new UiRect( 77, 848, 161, 976 ),
			HelpText = 476,
			Mesh = UiMesh.Get( "aerialtop" ),
			PinAcross = Anchor.Left,
			PinDown = VerticalAnchor.Bottom,
			RightClicked = () => Tags?.DeleteAll( Time.WallMilliseconds )
		} );

		// The bank balance, in the OPPOSITE corner - the stream's 0x2f and 0x32. Root controls like the
		// cluster below rather than children of the gadget body, so each anchors top-left on its own.
		//
		// The save's economy thing (model 16) carries mBalance and mAdmissionFee, and ParkState's balance
		// starts from mBalance and moves with every admission fee, charge, cost and refund - see ShowMoney.
		_balance = Root.Add( new UiControl
		{
			Id = 0x2f,
			Rect = new UiRect( 258, 60, 720, 260 ),
			Font = BalanceFont,

			// Against the icon rather than adrift in the middle of a wide box, and that is a CHOICE. The
			// original sizes this field by measuring "999999999" and then computes 0x32's rect from the
			// width it got, so the two are placed against each other rather than at fixed points. Nothing
			// here re-flows either, so the stream's own rects are kept and the lettering is pulled to the
			// side the icon is on.
			TextAcross = TextAlign.Start
		} );

		// The currency icon - the one piece of artwork this cluster has of its own.
		Root.Add( new UiControl
		{
			Id = 0x32,
			Rect = new UiRect( 37, 58, 242, 262 ),
			Mesh = UiMesh.Get( "i_dollar" )
		} );

		// TWO OF THE CLUSTER ARE DELIBERATELY NOT BUILT, each for a reason rather than for want of a rect.
		//
		// 0x30, the price of whatever is in your hand, (458,273)-(720,363): yellow, font 2, and started
		// HIDDEN by FUN_004a1d70, which is exactly what an empty hand should show. The hand carries
		// things (ParkBuilding.Carry), but what makes the original show 0x30 is not traced.
		//
		// 0x31, the trend arrow, (728,109)-(831,211) - and it is a CHILD of 0x2f rather than a root
		// control, which only the disassembly shows: 0x004a0f05 fetches 0x2f from the interface root and
		// then MOV ECX,EAX fetches 0x31 from THAT. FUN_004a0e30 sets its frame to 0 or 1 by comparing the
		// newest sample of two ring buffers - base, index, count and a wrapped flag - at +0x1f5a4 (total
		// costs) and +0x1fc94 (cash in), frame 1 when cash in is lower (0x004a0f8a), on the thing FUN_00519510 fetches from the world at +0x1da720.
		//
		// THOSE HISTORIES ARE NOT ON THE ECONOMY. They are on the
		// park's STATISTICS manager: FUN_004c5d70 is the serialiser carrying those exact ring offsets,
		// and it writes mLifetimeVisitors, mMisbehavingKids, mMostPaidForTicket, mLongestStay and
		// mParkLastOpened beside them. The offsets are what joins the two functions; the header field is
		// mParkAnalyser (docs/exe/park.md, "Arrivals"), and the thing is named here by what it serialises.
		//
		// Nothing in this tree keeps a history of anything, so the arrow would have no two numbers to
		// compare and would sit on one frame for ever.

		// The stream's fourth root control, away in the top right corner of the screen rather than on the
		// panel - so it is added to the window's root, not to the gadget body, and it anchors top-right on
		// its own. Two rows: the ticket above, the key below, each an icon with its count to the left.
		var earned = Root.Add( new UiControl
		{
			Id = 0x33,
			Rect = new UiRect( 1688, 48, 1963, 278 )
		} );

		// The ticket, which every player has whether or not they have earned any.
		earned.Add( new UiControl
		{
			Id = 0x35,
			Rect = new UiRect( 1853, 54, 1955, 156 ),
			Mesh = UiMesh.Get( "gtick" )
		} );

		_ticketCount = earned.Add( Count( 0x37, new UiRect( 1688, 60, 1829, 150 ) ) );

		// The key row, which FUN_004a1d70 hides outright while the game type is 2 - Instant Action, where
		// keys mean nothing because every park is already open.
		_keyRow = earned.Add( new UiControl
		{
			Id = 0x1e0f0,
			Rect = new UiRect( 1688, 165, 1963, 268 )
		} );

		_keyRow.Add( new UiControl
		{
			Id = 0x34,
			Rect = new UiRect( 1853, 165, 1955, 268 ),
			Mesh = UiMesh.Get( "gkey" ),

			// gkey.md2 carries four frames - gkey0 to gkey3 in ui.wad - and the original asks for the
			// last of them here (FUN_0065d3a3 with 3), as the lobby's island panel does with the same
			// model.
			Frame = 3
		} );

		_keyCount = _keyRow.Add( Count( 0x36, new UiRect( 1688, 171, 1829, 261 ) ) );

		ShowDate();
		ShowEarned();
		ShowMoney();
		ShowHappiness();
	}

	/// <summary>
	/// One of the two counts in the top right corner: white, font slot 2, and pushed up against the icon
	/// it belongs to, which is what <see cref="IslandPanel"/> does with the lobby's.
	/// </summary>
	private static UiControl Count( int id, UiRect rect )
		=> new()
		{
			Id = id,
			Rect = rect,
			Font = CountFont,
			TextAcross = TextAlign.End,
			TextWraps = true
		};

	/// <summary>
	/// The camcorder button was clicked, and the arm follows it out or in.
	///
	/// <para>
	/// It reads the button rather than toggling anything: <see cref="WindowStack"/> has already turned a
	/// switch over by the time the click arrives, so asking whether it is down now is asking what the
	/// click meant. Toggling here as well would put the arm back where it started.
	/// </para>
	/// </summary>
	private void ShowArm() => SlideArm( _camcorder.IsDown ? _carried : null );

	/// <summary>
	/// The arm sent out with a panel or in while the park runs, which the original slides and this does at once: each
	/// is counted as <c>GADGET_ARM_SLIDE</c>. The build's own <see cref="PutArm"/> is no slide and counts none. The
	/// message bar leaving the arm has no tag on show (the bar's 0x1001d8).
	/// </summary>
	private void SlideArm( UiControl? panel )
	{
		if ( panel == _onArm )
			return;

		Unimplemented.Report( "GADGET_ARM_SLIDE" );

		if ( _onArm == _bar )
		{
			Tags?.Shut();
			Log.Info( "Park gadget: message bar: off the arm" );
		}

		PutArm( panel );
	}

	/// <summary>
	/// Puts the arm out with a panel, or in, where FUN_004a1d70 builds it: its right edge <see cref="ArmTravel"/> out
	/// with the camcorder panel, the stream's rectangles, and <see cref="BarTravel"/> with the message bar, its end and
	/// handle moved as far; in, the handle hugs the body's right edge and the rest lies behind the body.
	/// </summary>
	/// <remarks>
	/// <b>Not the original's in four ways.</b> It slides: the handler on the arm, 0x004a14f0, moves its right edge a
	/// step on a 20 ms timer, ten steps, and here the arm is out or in at once (<see cref="SlideArm"/> counts it). Its
	/// travel there is a tenth of the panel's width and 208, ten times: the bar's 820 is that, and the camcorder
	/// panel's is the stream's 645 where the original's handle was measured 520 out. In, the arm and its end draw
	/// nothing here, since how the original draws a mesh on a narrowed control is not traced; every point of both
	/// rectangles is inside the body's outline or the handle's, so they are behind those two either way. And the
	/// retract button is hidden with what the arm carries, where the original switches it off and keeps it behind the
	/// handle (docs/exe/hud.md, "The arm's handler").
	/// </remarks>
	private void PutArm( UiControl? panel )
	{
		if ( panel == _onArm )
			return;

		_onArm = panel;

		var travel = panel == null ? 0 : panel == _bar ? BarTravel : ArmTravel;
		var move = travel - _armTravel;

		_armTravel = travel;
		_arm.Rect = _arm.Rect with { Right = _arm.Rect.Right + move };
		_armEnd.Move( move, 0 );

		_arm.Mesh = panel != null ? _armMesh : null;
		_armEnd.Mesh = panel != null ? _armEndMesh : null;
		_retract.Visible = panel != null;
		_carried.Visible = panel == _carried;
		_bar.Visible = panel == _bar;
	}

	/// <summary>
	/// Folds the arm away - FUN_004a25f0, whose one argument lifts the camcorder button with it: it fetches control
	/// 0x27 and puts it back up. The message bar's own ways in pass nought, and the button stays as it is.
	/// </summary>
	private void CloseArm( bool liftsCamcorder = true )
	{
		SlideArm( null );

		if ( liftsCamcorder )
			_camcorder.IsDown = false;
	}

	/// <summary>The bar's three slices, the stream's messagel, messagem and messager by their files, and its erase button's.</summary>
	internal const string BarLeftMesh = "f_tagl";
	internal const string BarMiddleMesh = "f_tagm";
	internal const string BarRightMesh = "f_tagr";
	internal const string BarEraseMesh = "mb_erase";

	/// <summary>FONTTINY/SMALL/BIG slot 6, the label skin FUN_0048d8b0 gives the bar's middle panel.</summary>
	private const int BarFont = 6;

	/// <summary>The bar's button that puts the camera on the tag's thing, mb_locate: types 3 and 4 (FUN_0048e610).</summary>
	private const int LocateThing = 3;
	private const int LocateItem = 4;

	/// <summary>
	/// A bar button's mesh and help row by its type, the tag row's +0x10 or +0x14 (FUN_0048e610). A type not here
	/// shows no button; mb_accept and mb_decline are given no help row. Types 0x12 and 0x14 name the nodes b_loan and
	/// b_pkinfo, which no file of ui.wad is called: they are not here, and no listed row carries them.
	/// </summary>
	internal static (string Mesh, int Help)? BarButtonLook( int type ) => type switch
	{
		LocateThing => ("mb_locate", 0x204),
		LocateItem => ("mb_locate", 0x203),
		>= 9 and <= 0x11 => ("mb_buy", 0x200 + type),
		0x13 => ("mb_resrch", 0x213),
		0x15 => ("b_staffcost", 0x215),
		0x17 => ("mb_callmech", 0x217),
		0x18 => ("b_entry", 0xec),
		0x1a => ("mb_accept", -1),
		ParkTags.DeclineButton => ("mb_decline", -1),
		_ => null
	};

	/// <summary>
	/// A tag's left click (FUN_0048dae0, 0x10006 with the left button): sound 0xc0, then the arm folded away where
	/// this tag's message is the one on show, or its message put on the bar (FUN_0048d9c0): the tag's timers
	/// stopped, the bar handed the text, the two buttons' types, the thing and the item, and the arm sent out with
	/// it. The bar's own turn as the arm starts out (0x1001d9) writes the text, dresses the two buttons and shows
	/// the erase button unless the second button declines.
	/// </summary>
	/// <remarks>
	/// The advisor's message 0x11c the original posts with it (FUN_00486b00) and withdraws as the bar leaves is a
	/// screen's line, which no screen here says: counted, <c>TAG_MESSAGE_BAR_ADVISOR_LINE</c>. With the arm already
	/// carrying something the original folds it away and sends it out again; here the new panel is on it at once.
	/// </remarks>
	private void OpenTag( ParkTags tags, ParkTags.Tag tag )
	{
		UiSounds.TagClick();

		if ( tags.OnShow == tag )
		{
			CloseArm( liftsCamcorder: false );
			return;
		}

		if ( !tags.Open( tag, Time.WallMilliseconds ) )
			return;

		Unimplemented.Report( "TAG_MESSAGE_BAR_ADVISOR_LINE" );

		_barText.Text = tag.Text;
		Dress( _barFirst, tag.Row.Button1 );
		Dress( _barSecond, tag.Row.Button2 );
		_barErase.Visible = tag.Row.Button2 != ParkTags.DeclineButton;

		if ( _onArm != _bar )
		{
			Unimplemented.Report( "GADGET_ARM_SLIDE" );
			PutArm( _bar );
		}

		Log.Info( $"Park gadget: message bar: tag {tag.Row.Id} subject {tag.Subject} item {tag.Item} {BarCensus()}" );
	}

	private static void Dress( UiButton button, int type )
	{
		var look = BarButtonLook( type );

		button.Mesh = look is { } found ? UiMesh.Get( found.Mesh ) : null;
		button.HelpText = look?.Help ?? -1;
		button.Visible = look != null;
	}

	/// <summary>
	/// One of the bar's two stacked buttons (FUN_0048e2f0, 0x10d30 and 0x10d2f, to FUN_0048e0f0 with the button's
	/// type, the tag's thing and its item). Type 3 puts the camera on the thing's cell (FUN_004867b0) and leaves the
	/// bar out. Every other type opens a screen or answers an offer, none of it built: counted,
	/// <c>TAG_BAR_BUTTON</c>, and nothing more.
	/// </summary>
	private void BarButton( bool first )
	{
		if ( Tags?.OnShow is not { } tag )
			return;

		var type = first ? tag.Row.Button1 : tag.Row.Button2;

		if ( type == LocateThing )
		{
			ParkOrbitCameraMode.GoToThing( tag.Subject );
			return;
		}

		Unimplemented.Report( "TAG_BAR_BUTTON" );
		Log.Info( $"Park gadget: message bar: button type {type} is not built, so nothing more happens" );
	}

	/// <summary>
	/// The bar's erase button (0x10d2e): the tag on show is hidden at once and taken off the mast half a second on
	/// (the mast's 0x100002 with nought), and the arm folded away.
	/// </summary>
	private void EraseTag()
	{
		if ( Tags is not { OnShow: { } tag } tags )
			return;

		tags.Hide( tag, Time.WallMilliseconds );
		CloseArm( liftsCamcorder: false );
	}

	/// <summary>
	/// How far the tag on show stands out past its own rectangle each way: the stream at 0x007502b8, played on it
	/// while its message is on the bar (FUN_0065e72d with 100): two more a step for three steps, two fewer for
	/// three, a step at rest, and round again, a step every 100 ms.
	/// </summary>
	internal static int Swell( long sinceOpened ) => (int)(sinceOpened / 100 % 7) switch { 1 or 5 => 2, 2 or 4 => 4, 3 => 6, _ => 0 };

	/// <summary>The bar, its text and buttons and the arm's handle, for the debug console's <c>tags</c>.</summary>
	internal string BarCensus() => $"bar: out={(BarOut ? 1 : 0)} rect={Edges( _bar.Rect )} arm={Edges( _arm.Rect )} handle={Edges( _armEnd.Children[0].Rect )}"
		+ $" first={Button( _barFirst )} second={Button( _barSecond )} erase={Button( _barErase )} camcorder={(_camcorder.IsDown ? 1 : 0)} text=\"{(BarOut ? _barText.Text : null)}\"";

	private static string Button( UiButton button ) => button.Visible ? $"{button.Mesh?.Name ?? "none"}/help={button.HelpText}" : "hidden";

	/// <summary>
	/// b_1person, the panel's id 99: into first person through FUN_00481a10, and the arm closes behind it.
	/// The C key reaches the same mode without any of this, which is why
	/// <see cref="ParkCamcorderCameraMode"/> is still the only record of whether it is running.
	/// </summary>
	private void EnterCamcorder()
	{
		ParkCamcorderCameraMode.Enter();
		CloseArm();
	}

	/// <summary>
	/// b_postcard, the panel's id 100: FUN_004a9380, which pauses the game and writes a picture out - the
	/// game ships Postcard.wad, postcard.jpg and an HTML template for it. Nothing here writes one yet, and
	/// it is deliberately out of scope, so it is counted, says so and closes the arm as the other one does.
	/// </summary>
	private void SendPostcard()
	{
		Unimplemented.Report( "POSTCARD_BUTTON" );
		Log.Info( "Park gadget: Postcard - nothing writes a postcard out yet, so nothing more happens" );
		CloseArm();
	}

	private const int MastLeft = 94;
	private const int MastRight = 144;
	/// <summary>The mast's bottom edge, where its top stands eight above with no tag on it.</summary>
	private const int MastBottom = 984;
	/// <summary>A tag's control, FUN_0048ea00: 0x46 across and 0x41 down.</summary>
	private const int TagWidth = 0x46;
	private const int TagHeight = 0x41;
	/// <summary>What each tag lifts the mast's top by: its height and ten (LAB_004a11e0, 0x100001).</summary>
	private const int TagStep = TagHeight + 10;

	private readonly UiControl _mast;
	private readonly UiControl _mastTop;
	private readonly List<UiControl> _tagControls = [];
	/// <summary>Each tag control's own rectangle, which the tag on show swells past.</summary>
	private readonly List<UiRect> _tagRects = [];
	private ParkTags? _tagsGiven;
	private ParkTags? _tagsShown;

	/// <summary>The tags the mast shows: the running park's, or the list a test hands the gadget.</summary>
	internal ParkTags? Tags { get => _tagsGiven ?? Level.Current?.ParkState?.Tags; set => _tagsGiven = value; }
	private int _tagChangesShown = -1;
	private int _tagsHeard;

	/// <summary>
	/// The tags' timers, then the mast as the list stands: the oldest tag lowest, each eight below where the mast's
	/// top stood as it was put on and across its middle, the top lifted <see cref="TagStep"/> a tag. A hidden tag
	/// keeps its place until it is taken off.
	/// </summary>
	private void PutTags()
	{
		var tags = Tags;
		if ( tags == null ) return;

		tags.Tick( Time.WallMilliseconds );

		// The tag on show gone, by its own going or another park's list: the bar leaves with it (FUN_0048dae0, message 5).
		if ( BarOut && tags.OnShow == null )
			CloseArm( liftsCamcorder: false );

		if ( tags != _tagsShown || tags.Changes != _tagChangesShown )
			BuildTags( tags );

		// The tag on show, swelling and shrinking about its own rectangle.
		for ( var i = 0; i < _tagControls.Count; ++i )
		{
			var tag = tags.Standing[i];
			var swell = tag == tags.OnShow ? Swell( Time.WallMilliseconds - tag.OpenedAt ) : 0;
			var own = _tagRects[i];

			_tagControls[i].Rect = new UiRect( own.Left - swell, own.Top - swell, own.Right + swell, own.Bottom + swell );
		}
	}

	private void BuildTags( ParkTags tags )
	{

		// The new tag's sound, effect 0x1e of the interface's own category (FUN_0048eb20).
		if ( tags == _tagsShown && tags.Made > _tagsHeard ) UiSounds.NewTag();
		_tagsHeard = tags.Made;

		foreach ( var control in _tagControls )
		{
			control.Visible = false;
			_mast.Children.Remove( control );
		}

		_tagControls.Clear();
		_tagRects.Clear();
		_tagsShown = tags;
		_tagChangesShown = tags.Changes;

		var standing = tags.Standing;
		var top = MastBottom - 8 - TagStep * standing.Count;
		_mastTop.Move( 0, top - _mast.Rect.Top );
		_mast.Rect = new UiRect( MastLeft, top, MastRight, MastBottom );

		for ( var i = 0; i < standing.Count; ++i )
		{
			var tag = standing[i];
			var left = (MastLeft + MastRight - TagWidth) / 2;
			var tagTop = MastBottom - 8 - TagStep * (i + 1) + 8;
			_tagRects.Add( new UiRect( left, tagTop, left + TagWidth, tagTop + TagHeight ) );
			_tagControls.Add( _mast.Add( new UiControl
			{
				Id = 900 + i,
				Rect = new UiRect( left, tagTop, left + TagWidth, tagTop + TagHeight ),
				// "Message - left-click to open, right-click to delete", row 475 (FUN_0048ea00, +0x90).
				HelpText = 475,
				Mesh = UiMesh.Get( "msgtag" ),
				Frame = tag.Row.Picture,
				Visible = !tag.Hidden,
				PinAcross = Anchor.Left,
				PinDown = VerticalAnchor.Bottom,
				LeftClicked = () => OpenTag( tags, tag ),
				// Hidden, the arm folded away where its message is on show, and off the mast half a second on.
				RightClicked = () =>
				{
					tags.Hide( tag, Time.WallMilliseconds );

					if ( tags.OnShow == tag )
						CloseArm( liftsCamcorder: false );
				}
			} ) );
		}

		Log.Info( $"Park gadget: {AerialCensus()}" );
	}

	/// <summary>Where the mast, its top and each tag stand on the virtual screen, for the debug console's <c>tags</c>.</summary>
	internal string AerialCensus() => $"aerial: mast={Edges( _mast.Rect )} top={Edges( _mastTop.Rect )} "
		+ string.Join( " ", _tagControls.Select( ( control, i ) => $"tag{control.Id}={Edges( _tagRects[i] )}/frame={control.Frame}/shown={(control.Visible ? 1 : 0)}" ) );

	private static string Edges( UiRect rect ) => $"{rect.Left},{rect.Top},{rect.Right},{rect.Bottom}";

	/// <summary>The park screen that was open when the gadget last looked - see <see cref="Update"/>.</summary>
	private UiWindow? _screenSeen;

	protected internal override void Update()
	{
		// A park screen opening folds the arm away and lifts the camcorder button (FUN_004a25f0( 1 ) from
		// FUN_00485b70, 0x00485cf3).
		var screen = Stack.Windows.LastOrDefault( window => window.ParkScreen );

		if ( screen != null && screen != _screenSeen )
			CloseArm();

		_screenSeen = screen;

		PutTags();
		ShowDate();
		ShowEarned();
		ShowMoney();
		ShowHappiness();

		// Put the gadget away in first person, because camcorder mode steers the view from where the
		// pointer IS rather than from how it moves: outside a dead zone of 0.4 the view turns at up to
		// 1.2 radians a second, and every one of this panel's controls is inside that band at the bottom
		// left of any window. Left up, reaching for a button would sweep the view round - about a
		// quarter turn during a two-second reach - and leaving the mode hands that yaw to the orbit
		// camera, so the park would come back facing somewhere nobody chose.
		//
		// The original does not solve this by suppressing the steering: its entry to first person hides
		// layer 0, which holds the gadget, and shows the viewfinder's layer 1 (0x004a2ac0) - its own layout
		// stream at 0x0074fa98, a full-frame surround with an eject button in the far corner. Here that
		// layer is ParkViewfinder, and this hides: hidden rather than closed, which is the stack's own word
		// for a window that is neither drawn nor pointed at.
		//
		// It also goes away while the map is up, for the same reason in a different form: the original
		// does not leave the two on top of each other. FUN_005f0b40 sends message 6 - put away - to the
		// interface root (DAT_007cb2ac) before it loads the map's own tree. Without this the map's
		// scroll arrows and zoom buttons land exactly on this panel's six, because both live in the
		// bottom-left corner; a screenshot showed them drawn one over the other while every measured
		// check still passed.
		//
		// The map is NAMED rather than asked for as "is anything modal", because GameMenu, MessageBox
		// and OptionsScreen all set Modal as well, and what the gadget should do underneath those is a
		// separate question nobody has asked.
		//
		// Both conditions belong in ONE assignment: written as two, one after the other, the second
		// silently overwrites the first - a line that compiles, measures clean and does nothing.
		Hidden = ParkCamcorderCameraMode.Active || Stack.Windows.Any( window => window is ParkMapScreen );
	}

	/// <summary>
	/// The date the park's calendar has reached.
	///
	/// <para>
	/// <b>The format is an approximation, and knowingly one.</b> The original fills this control in
	/// FUN_004a0e30: it builds the text through FUN_006acca0 from a compiled resource - entry 0x1c0 of
	/// the bundle at DAT_0078ba94, into a 31-byte buffer - handed three numbered arguments typed 0x10,
	/// 0x11 and 0x12, which
	/// are the day, the month and the year. So the layout of the date is <b>the game's own localised
	/// resource</b>, not the machine's setting: the executable imports no GetDateFormat at all, and
	/// GetLocaleInfoA/W appear only inside CRT locale code.
	/// </para>
	/// <para>
	/// That entry of that bundle is UITEXT 448, the row the Load Park list writes a save's date with
	/// (<see cref="ParkFileScreen.DateOf"/>; docs/exe/saves.md). This does not use it: it shows the machine's
	/// short date, a stand-in and not a match.
	/// </para>
	/// </summary>
	private void ShowDate() => _date.Text = GameCalendar.Now.ToShortDateString();

	/// <summary>
	/// The golden keys and tickets the player has earned, read from their gms.dat the way the lobby's
	/// island panel reads them - see <see cref="IslandPanel.ShowKeys"/>.
	///
	/// <para>
	/// The format is the original's own: the string FUN_004a1d70 seeds both counts with, at 0x00752f24,
	/// is "0 x", so the number leads and the multiplication sign follows it. Nobody being picked yet -
	/// a park reached from the debug console rather than through the lobby - reads as none rather than
	/// as an error, because the count is a fact about a player and there is no player.
	/// </para>
	/// </summary>
	private void ShowEarned()
	{
		var player = Players.Roster.Current;

		_keyRow.Visible = player is not { InstantAction: true };
		_keyCount.Text = $"{player?.Keys ?? 0} x";
		_ticketCount.Text = $"{player?.Tickets ?? 0} x";
	}

	/// <summary>
	/// What the park is worth, in the corner the original keeps it - the balance the save was left with,
	/// moved by every admission fee, charge, cost and refund since it was loaded.
	///
	/// <para>
	/// <b>It is ONE number.</b> <see cref="ParkWorld"/> describes a file and a fee cannot move it;
	/// <see cref="ParkState"/> is the park as it is played, the level owns it, and taking a fee moves its
	/// balance directly - which is what the original does too (<c>FUN_004d0600</c> adds a fee straight
	/// onto <c>mBalance</c> and <c>mProfitThisYear</c>).
	/// </para>
	/// <para>
	/// Plain digits, with no thousands separator, because the original sizes this field by measuring the
	/// string "999999999" (0x00752f10) - nine digits and nothing between them.
	/// </para>
	/// <para>
	/// A park whose file carries no economy shows <b>nothing at all</b> rather than a nought, for the
	/// reason the key count shows none rather than an error: having no economy is a fact about the file,
	/// where a nought would be a claim that the park is broke.
	/// </para>
	/// </summary>
	private void ShowMoney()
	{
		var level = Level.Current;

		// The number comes from the running state, but whether the park HAS an economy is still a fact
		// about the file - so a theme shipping no economy shows nothing rather than a nought, which would
		// be a claim that it is broke.
		if ( level?.ParkState is not { } park || level.Park?.Economy is null )
		{
			_balance.Text = null;
			return;
		}

		_balance.Text = $"{park.Balance}";
	}

	/// <summary>
	/// How happy the park's visitors are, which is the one thing the gauge has ever been for - see
	/// <see cref="ParkPeople.AverageHappiness"/> for the number, and <see cref="UiMeter"/> for the drawing
	/// and for which part of it is a choice.
	///
	/// <para>
	/// <b>Read every frame where the original reads it every two seconds, and that is a departure.</b> Its
	/// meter arms a 2000ms timer (<c>FUN_004a1cd0</c>, id 0x80083) because the value costs a walk over
	/// every thing in the park; this walks the park's guests alone. What differs on screen is a gauge
	/// that moves smoothly rather than in steps, and putting the timer back would mean keeping a clock
	/// here in order to make the reading worse.
	/// </para>
	/// </summary>
	private void ShowHappiness() => _happiness.Value = ParkPeople.Current?.AverageHappiness() ?? 0;
}
