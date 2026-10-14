namespace OpenTPW.UI;

/// <summary>
/// What each kind of staff's training may spend a month - the original's <c>staffcosts</c>, headed "Staff Training
/// Budgets", screen 9 of the Money category and the screen the training tags' bar button opens.
///
/// <para>
/// <b>Where the layout comes from.</b> <c>FUN_004b2750</c> builds it from the compiled stream at <c>0x00756400</c>,
/// handler <c>0x004b24b0</c>. Every rect, id, help row and mesh below is that stream's
/// (<c>docs/exe/hud.md</c>, "The Staff Training Budgets screen").
/// </para>
///
/// <para>
/// <b>A row a kind.</b> Each is a spinner (control type 12) over nought to 10000 in steps of 25, on thing 1's
/// <c>mBudget</c> of the kind, with the kind's picture and a bar of the kind's skill: its members' grades as a
/// whole per cent of the top grade for each (<c>FUN_004c9c00</c>), set as the screen is built and not again. The
/// rows stand handymen, mechanics, entertainers, guards, researchers; the stream lists them out of that order.
/// A budget is written as its spinner moves (message <c>0x800</c>), not as the screen closes.
/// </para>
///
/// <para>
/// <b>Opening it is a change of every budget.</b> A spinner given its first value tells the handler as a press
/// does, so as the screen opens each budget is written back as it stands, a budget over nought sets
/// <c>mHaveEverTrained</c>, and the staff costs row holds the five budgets added up (<c>0x004b266a</c>), as it does
/// after every press (<c>docs/exe/hud.md</c>: measured in the original, not read).
/// </para>
///
/// <para>
/// <b>The month's account is not kept.</b> The panel's cash in, other costs and balance are the analyser's newest
/// month of cash in and of all costs, which nothing here keeps: they are left empty and counted,
/// <c>MONTHLY_ACCOUNT_HISTORY</c>, as the hire screen's are.
/// </para>
/// </summary>
internal sealed class ParkStaffCostsScreen : UiWindow
{
	/// <summary>The spinners' ends and step (FUN_0066a5c4 with 0 and 10000, FUN_0066a68d with 0x19).</summary>
	internal const int Lowest = 0;

	internal const int Highest = 10000;

	internal const int Step = 25;

	/// <summary>Font slot 6, which the builder gives every figure and label.</summary>
	private const int TextFont = 6;

	/// <summary>What the builder letters the two cost rows in, <c>FUN_0065c5d5( 0xff, 0, 0, 0xff )</c>.</summary>
	private static readonly UiColour Cost = new( 255, 0, 0 );

	/// <summary>
	/// One kind's row: its spinner's id, where the row lies, the picture's mesh, and the help rows of the spinner
	/// and of the skill bar. In <see cref="ParkStaffPool"/>'s order of kinds, which is thing 1's order of budgets.
	/// </summary>
	internal static readonly (int Id, int Top, string Picture, int Help, int BarHelp)[] Rows =
	[
		(0x2e7, 183, "h_handy", 164, 169),
		(0x2ec, 337, "h_mech", 165, 170),
		(0x2ea, 491, "h_enter", 166, 171),
		(0x2eb, 645, "h_guard", 167, 172),
		(0x2ed, 799, "h_resrcher", 168, 173)
	];

	private readonly UiControl[] _budgets = new UiControl[Rows.Length];
	private readonly UiControl _staffCosts;

	public ParkStaffCostsScreen( WindowStack stack ) : base( stack )
	{
		// Built onto the park's own layer, as the entry-price screen is: not modal, and nothing paused.
		ParkScreen = true;

		// w_park: the stream's hash 0xf76e4349 is the node "window3".
		Root = new UiControl
		{
			Id = 0x2d9,
			Rect = new UiRect( 208, 30, 1840, 1007 ),
			Mesh = UiMesh.Get( "w_park" )
		};

		// UITEXT 147, "Staff Training Budgets", lettered by UI_SetTitle on 0x2da (0x004b3293): font 5 in
		// (234, 239, 102), in the stream's rect (805 to 1277) widened by half its width either side.
		Root.Add( new UiControl
		{
			Id = 0x2da,
			Rect = new UiRect( 569, 74, 1513, 153 ),
			Font = 5,
			TextColour = new UiColour( 234, 239, 102 ),
			TextShadow = true,
			Text = Localization.Get( UIStrings.StaffTrainingBudgets )
		} );

		Root.Add( new UiButton
		{
			Id = -1,
			Rect = new UiRect( 1711, 820, 1794, 903 ),
			HelpText = 1,
			Mesh = UiMesh.Get( "b_okay" ),
			Clicked = () => Stack.Close( this )
		} );

		// This category's other three screens (the handler's 0x100: 0x2db FUN_00498d80, 0x2dc FUN_0049ac60, 0x2dd
		// FUN_0049fb30). The loans button is disabled in Instant Action (game type 2, 0x004b32eb).
		Root.Add( CrossLink( 0x2db, new UiRect( 1688, 575, 1790, 677 ), 176, "b_entry", 10 ) );
		Root.Add( CrossLink( 0x2dc, new UiRect( 1688, 467, 1790, 570 ), 175, "b_finance", 7 ) );
		Root.Add( CrossLink( 0x2dd, new UiRect( 1688, 360, 1790, 462 ), 174, "b_loans", 8 ) ).Enabled = !Level.InstantAction;

		var account = Root.Add( new UiControl
		{
			Id = 0x2de,
			Rect = new UiRect( 267, 183, 765, 411 ),
			HelpText = 177,
			Mesh = UiMesh.Get( "!frame" )
		} );

		// UITEXT 357, 358, 359 and 361: "Cash in", "- Staff costs", "- Other costs", "Balance".
		account.Add( Label( 0x2e3, new UiRect( 284, 195, 561, 240 ), UIStrings.CashIn ) );
		account.Add( Label( 0x2e6, new UiRect( 284, 246, 561, 291 ), UIStrings.StaffCosts, Cost ) );
		account.Add( Label( 0x2e1, new UiRect( 284, 297, 561, 342 ), UIStrings.OtherCosts, Cost ) );
		account.Add( Label( 0x2e5, new UiRect( 284, 349, 561, 393 ), UIStrings.Balance ) );

		account.Add( Figure( 0x2e2, new UiRect( 591, 195, 755, 240 ) ) );
		_staffCosts = account.Add( Figure( 0x2df, new UiRect( 591, 246, 755, 291 ), Cost ) );
		account.Add( Figure( 0x2e0, new UiRect( 591, 297, 755, 342 ), Cost ) );
		account.Add( Figure( 0x2e4, new UiRect( 591, 349, 755, 393 ) ) );

		var people = ParkPeople.Current;

		for ( var kind = 0; kind < Rows.Length; ++kind )
		{
			var row = Rows[kind];

			// Every rect of a row is the first row's, 154 down a row; the stream's own differ from that by a unit
			// here and there, which is under a screen pixel.
			var down = row.Top - Rows[0].Top;

			// f_train, the row's lozenge with its box. The budget is lettered in the stream's text rect (op 3),
			// black, in the middle (FUN_0065c428( 1, 1 ), FUN_0065c5d5( 0, 0, 0, 0xff )).
			var spinner = Root.Add( new UiControl
			{
				Id = row.Id,
				Rect = new UiRect( 770, row.Top, 1670, row.Top + 149 ),
				HelpText = row.Help,
				Mesh = UiMesh.Get( "f_train" ),
				TextRect = new UiRect( 996, 233 + down, 1273, 278 + down ),
				Font = TextFont,
				TextColour = UiColour.Black
			} );

			_budgets[kind] = spinner;

			// The skill bar, type 9 under ridestatbar.wct: (FUN_004c9c00( model ) << 10) / 100 of 1024.
			spinner.Add( new ParkObjectWindow.UiStatBar
			{
				Id = 0x2e8,
				Rect = new UiRect( 1376, 235 + down, 1592, 274 + down ),
				HelpText = row.BarHelp,
				Known = true,
				At = (people?.TrainingOf( kind ).GradeShare ?? 0) / 100f
			} );

			// The spinner's op 0xf and op 0xe children, minus on the left and plus on the right.
			spinner.Add( Nudge( new UiRect( 925, 224 + down, 985, 285 + down ), "b_minus", kind, -Step ) );
			spinner.Add( Nudge( new UiRect( 1286, 224 + down, 1346, 285 + down ), "b_plus", kind, Step ) );

			spinner.Add( new UiControl
			{
				Id = 0x2e9,
				Rect = new UiRect( 800, 203 + down, 902, 306 + down ),
				Mesh = UiMesh.Get( row.Picture )
			} );
		}

		Unimplemented.Report( "MONTHLY_ACCOUNT_HISTORY" );

		if ( people != null )
		{
			for ( var kind = 0; kind < Rows.Length; ++kind )
				people.SetTrainingBudget( kind, people.TrainingBudgets[kind] );

			_staffCosts.Text = Money( people.TrainingBudgets.Sum() );
		}

		Show();

		Log.Info( $"Staff Training Budgets: opened, {Census()}" );
	}

	/// <summary>The screen as it stands, for the log and the console: each row's budget and its bar's whole per cent (-1 for a bar not drawn), and the staff costs row.</summary>
	internal string Census()
		=> $"budgets {string.Join( ",", _budgets.Select( budget => budget.Text ) )} "
			+ $"skill {string.Join( ",", _budgets.Select( budget => budget.Children.OfType<ParkObjectWindow.UiStatBar>().First() ).Select( bar => bar.Known ? (int)MathF.Round( bar.At * 100f ) : -1 ) )} "
			+ $"staffcosts {_staffCosts.Text ?? "empty"} loans {(Root!.Children.OfType<UiButton>().First( button => button.Id == 0x2dd ).Enabled ? "on" : "off")}";

	/// <summary>One of a spinner's two buttons.</summary>
	private UiButton Nudge( UiRect rect, string mesh, int kind, int by )
		=> new()
		{
			Rect = rect,
			Mesh = UiMesh.Get( mesh ),
			Clicked = () =>
			{
				if ( ParkPeople.Current is not { } people )
					return;

				people.SetTrainingBudget( kind, Math.Clamp( people.TrainingBudgets[kind] + by, Lowest, Highest ) );

				// The handler's own refresh after a change: the five budgets added up into the staff costs row.
				_staffCosts.Text = Money( people.TrainingBudgets.Sum() );

				Show();
			}
		};

	/// <summary>One of the three buttons to this category's other screens.</summary>
	private UiButton CrossLink( int id, UiRect rect, int help, string mesh, int screen )
		=> new()
		{
			Id = id,
			Rect = rect,
			HelpText = help,
			Mesh = UiMesh.Get( mesh ),
			Clicked = () =>
			{
				Stack.Close( this );
				ParkCategoryScreens.Show( Stack, ParkCategoryScreens.Finance, screen );
			}
		};

	private static UiControl Label( int id, UiRect rect, UIStrings text, UiColour? colour = null )
		=> new()
		{
			Id = id,
			Rect = rect,
			Font = TextFont,
			TextColour = colour ?? UiColour.White,
			TextAcross = TextAlign.Start,
			Text = Localization.Get( text )
		};

	private static UiControl Figure( int id, UiRect rect, UiColour? colour = null )
		=> new()
		{
			Id = id,
			Rect = rect,
			Font = TextFont,
			TextColour = colour ?? UiColour.White,
			TextAcross = TextAlign.End
		};

	/// <summary>A figure as the screen's number skin letters it (FUN_00490250): "$ 25".</summary>
	internal static string Money( int amount ) => $"$ {amount}";

	/// <summary>The five budgets, as the running park holds them.</summary>
	private void Show()
	{
		var people = ParkPeople.Current;

		for ( var kind = 0; kind < _budgets.Length; ++kind )
			_budgets[kind].Text = people != null ? Money( people.TrainingBudgets[kind] ) : null;
	}

	protected internal override void Update() => Show();
}
