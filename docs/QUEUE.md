# Work queue

Begun 2026-09-22 against `main` at `9b0ebab`; items from Q35 on were filed against later tips. File and doc line
numbers in Q13-Q34 are from `9b0ebab` and have drifted (`park-engine.md` by up to ~1,100 lines): find the member or
the heading, never the line.

**How to use this file.** One item per session. Take the first unticked item and do only that one.
Tick it in the commit that lands it, with the number that proves it; the pre-commit hook then moves it to the end of
its section in `history/queue-done.md`, which holds every finished item. A page citing a Q-number that is not here
means that finished entry. If an item turns out to be two,
split it into two lines here and stop after the first. Alexah may reorder; nobody else does.
A line **Needs Alexah (date):** at the end of an item is a question an unattended run left (`WORKFLOW.md`,
"Unattended runs"): the item waits for the answer, and the session Alexah answers takes the line out.

**Every item, no exceptions.**

- Start from the repo root. Read `CLAUDE.md`, `docs/STATUS.md`, then this file's head and its first unticked item:
  the first hundred lines or so. The rest of the queue is other sessions'; grep it for a Q-number when a note
  is to be filed under one.
- Branch from `main`, named for the item, next free number: `alexah/N-<what-it-changes>`.
- "Confirm" means the running game: a screenshot **and** the census or log line that goes with it,
  both. Predict the number before reading it. A test is added for regression only. It is never the proof.
- Put the bug back and re-run the new test. If it stays green, the test is hollow. Fix the test.
- Build and test the commit alone in a throwaway worktree.
- Facts found go in `docs/exe/`, and bytes in a shipped file in the FileFormats clone (rule 14). The commit
  message says why. The code comment says what the code does now. No "used to say", no dates, no `>>>`
  banners in code.
- Update `docs/STATUS.md` in the same commit. It stays under 120 lines.
- When confirmed, fast-forward merge into `main`. Commit locally. **Pushing `main` still needs Alexah's
  yes** (rule 1). Say what is ready and stop. Do not start the next item.
- End with four lines: what is ready, what was confirmed on screen, what was not, what the next item is.

**Where the detail is.** Q72-Q75 come from the 2026-09-12 review, whose three
artifacts are listed in `docs/history/README.md`. Q208-Q217 come from the 2026-10-04 review of the commits of
2026-10-03 and 2026-10-04; an id such as `u6-hoardings-1` names a finding in its findings file (`CLAUDE.local.md`
says where). Q219-Q221 come from the 2026-10-04 effort audit of the commits of 2026-09-29 to 2026-10-01; an id such
as `r-f1` names a verdict in its results file (`CLAUDE.local.md` says where). Q222 comes from Q107's comparison with
the original. A note marked "fork review 2" comes from the 2026-10-16 review of another fork, every lead checked in
Ghidra; `verify-<area>.md` and a lead id name its evidence (`CLAUDE.local.md` says where).

---

## A. Bugs first

**Ordered at Alexah's word (2026-10-08):** what a playable Lost Kingdom park needs most comes first (saving a
park, the calendar, the shop's window, the gauge, the research lab), then the rest of this section in its old order.
Q444 and Q445 come first: defects a player meets, found by the review of the run of 2026-10-16/17 (its findings are
`~/.cache/tpw-harnesses/handoff/review2/q<N>/FINDINGS.md`).

- [ ] **Q447. A person's hover rows and pointer.** From Q444. The hover over a guest it would pick shows UIHELPTEXT 0x229
  (with their number, `+0x1d8`) or 0x22a, over a member 0x22b with `FUN_00506860`'s string, and puts up pointer 2
  (`FUN_004a2aa0( 2 )`; `FUN_00486d90`); ours shows neither, counted `PERSON_HOVER_HELP`. Q444's original frames show a
  member's row standing at the foot of the screen with the staff locator open (`q444/orig/a-2-P1.png`): read why first.

- [ ] **Q420. The writer: what else a made record leaves out.** The old Q310's other pieces, each counted or
  said in `saves.md`. Confirm: each piece's bytes beside a file of the original's own, and the file loaded in
  the original under Proton.
  From Q443 (`park.md`, "Arrivals: who comes, on what, and how often"): the leavers' draw is on the save's seed, which
  is written back, so the nearest piece is the staff strike's five records and `mForceStrike` (`ParkWorld.StaffHqState`;
  a strike changes them, Q138b, and the writer leaves them the file's, `saves.md`, the staff HQ), then the economy
  thing's rings and the loans. In no game run: the original's own vehicle at the same summons (its generator is drawn
  by every system and reseeded at each arrival, ours by the leavers alone: moving those onto `ParkState.RandomSeed` is
  not filed as a piece of this item), Full Simulation (not asked: the summons is either mode's), the other three themes.
  From Q442 (`hud.md`, "The patrol switch and the patrol tool"): the refused click's sound is built at every build
  tool, so the nearest piece is the leavers' draw on the save's seed (`park.md`; it needs the park's one shared
  generator). In no game run: a move's put-down refused (`ParkBuilding.Move`; tested only), a refusal for money, the
  original's placement and queue refusals (its path refusal heard; the one call is read in the listing), Full
  Simulation (not asked: the tools are either mode's), the other three themes.
  From Q439 (`hud.md`, "The patrol switch and the patrol tool"): the refused click's sound is built for the patrol
  tool, so the nearest piece is the same call at the other three refusals it serves, one helper away
  (`UiSounds.Refused`; `0x00524aa7` is ahead of the commit's mode switch): a placement on a red cell
  (`PLACEMENT_REFUSED_SOUND_0xAF`), the path tool (`PATH_TOOL_REFUSED_SOUND_0xAF`, two sites) and the queue tool
  (`QUEUE_TOOL_REFUSED_SOUND_0xAF`); then the leavers' draw on the save's seed (`park.md`). Not shown in the original:
  its sound under the music (0.318; run it with the Music switch off). In no game run: a refused click beside an open
  park screen (tested only), Full Simulation (not asked: the tool is either mode's), the other three themes.
  From Q438 (`hud.md`, "The patrol switch and the patrol tool"): a quick right click with the tool armed is the
  original's, so the nearest piece is the patrol tool's refused click's sound `0xaf` (counted,
  `PATROL_TOOL_REFUSED_SOUND_0xAF`; heard by the game's own mix; built, Q439), then the leavers' draw on the save's seed
  (`park.md`; it needs the park's one shared generator). In no game run: a right drag or a held right press with the
  tool armed (the original hands the camera the press late; what its orbit camera does with bit 4 of `DAT_00790aac`,
  held, is not decoded, and ours reads no right button), the click with RMB cancel off, Full Simulation (not asked: the
  tool is either mode's), the other three themes. Not heard.
  From Q437 (`hud.md`, "The patrol switch and the patrol tool"): Escape with the tool armed is the original's, so the
  nearest piece is a quick right click with the tool armed (RMB cancel on; `park-engine.md`, "The hand's ways out"), run
  in neither game (run in both, Q438). In no game run: Escape with a modifier held while the tool is armed (the shortcuts' row names none),
  Escape unanchored in the original, Full Simulation (not asked: the tool is either mode's), the other three themes. Not
  heard.
  From Q436 (`park-engine.md`, "Placement feedback", "The walls"): a lifted square's walls are the original's, so the
  nearest piece is Escape with the patrol tool armed (the original's puts it away unwritten, Q435; run here, Q437).
  Counted, not built: the latches a wall zeroes under a path or queue strip (`MARKER_WALL_CLEARS_REFUSAL_LATCH`). In no
  game run: a red wall and its blink, the walls under the locator's patrol edges, a rotation other than nought, Full
  Simulation (not asked: the outline is either mode's), the other three themes.
  From Q435 (`hud.md`, "The patrol switch and the patrol tool"): at one camera the lifted squares stand where the
  original's do, so the nearest piece is the walls a lifted square drops to a lower neighbour (`MARKER_LIFT_SIDE_FACES`,
  `FUN_0053df30`'s other faces; the original's records at 11.5 and 1.5 under a 21.5 top, `park-engine.md`, "Placement
  feedback"), drawn there and not here (built, Q436). In no game run: Escape with the tool armed here (the original's puts it away
  unwritten; run, Q437), a rotation other than nought at one camera, Full Simulation (not asked: the outline is either mode's),
  the other three themes.
  From Q434 (`ride-operation.md`, "A member fenced where one path cell lies"): the fenced member's walk on one path cell
  is the original's, so the nearest piece is the patrol tool's lifted squares at one camera in both games (ours looked
  far higher at Q433's two cameras; Q260, the lens, may be the cause; compared, Q435: the original's). In no game run: two misses in a row in the
  original (its 200 s had none; ours had one, deciding the next sweep as `0x00505542` says), another kind of member or
  grade, Full Simulation (not asked: the walk is either mode's), the other three themes.
  From Q433 (`hud.md`, "The patrol switch and the patrol tool"): the patrol switch and its tool are built. Nearest: a
  member fenced where one path cell lies stands idle on it with thought `0x16` seconds after reaching it here (the patrol
  roll finds only his own cell); watch the original's past nineteen seconds before changing `StaffBehaviour`. Seen: at
  the two runs' cameras our lifted squares look far higher above the ground than the original's, at the same lift
  numbers; compare at one camera (compared, Q435: the same at one camera). Counted, not built: the refused click's sound `0xaf` (`PATROL_TOOL_REFUSED_SOUND_0xAF`; built, Q439).
  In no game run: `0xc1` cut by the click here (the line ended first; the original's was cut; tested), Escape (run, Q437) or a
  right click with the tool armed (run, Q438), Full Simulation (not asked: the window is either mode's), the other three themes.
  Not heard.
  From Q432 (`hud.md`, "The person's windows"): FIRE and PICK UP are built, so the staff window's nearest piece is the
  patrol switch (`STAFF_WINDOW_PATROL`: `FUN_004b5b50`, map tool `0x36` and advisor line `0xc1`, or the area cleared,
  `FUN_00506e90`). Counted, not built: the puff of particle `0x13` at a dismissal (`STAFF_DISMISSAL_PARTICLE`,
  `FUN_004fa470`) and the figure of a carried worker at the pointer (`STAFF_PICKED_UP_PREVIEW`, the type-6 mode's
  sprite). In no game run: FIRE with the Confirmations option off and a dismissal with another worker carried (tested
  only), a drop refused, Full Simulation (not asked: the window is either mode's), the other three themes. Not heard.
  From Q431 (`hud.md`, "The person's windows"): the staff member's window is built, so the nearest piece is its verbs:
  FIRE (`STAFF_WINDOW_FIRE`: `FUN_00505790`, behind a box on UITEXT `0x18d` where the option `[0x0078d90f]` is set;
  `ParkPeople.Fire` exists), PICK UP (`STAFF_WINDOW_PICK_UP`: `FUN_0048c950` then `FUN_00505c50`; `ParkPeople.PickUp`
  exists), the patrol switch (`STAFF_WINDOW_PATROL`: `FUN_004b5b50`, map tool `0x36` and advisor line `0xc1`, or the
  area cleared, `FUN_00506e90`). Counted, not built: the name's edit (`STAFF_WINDOW_NAME_EDIT`, `FUN_0048d290`), the
  preview's model (`STAFF_WINDOW_PREVIEW`), a member gone while it is up (`STAFF_WINDOW_MEMBER_GONE`, not decoded),
  the all-staff list's advisor line `0x11d` (`STAFF_LIST_ADVISOR_LINE`), the staff room's list's row (`0x004b5267`; no
  staff room window). Not shown: the patrol switch's second help row, 87. Settled: Q430's "`0x12e` in no slot on the
  second to fourth opens" is the accept gate (Tutorial lines said once, and refused after three cuts). In no game run:
  a member's world click (tested only through `ParkStaffWindow.Open`; the pick may not find a walking member), PICK UP
  unlit (tested only), Full Simulation (not asked: the window is either mode's), the other three themes. Not heard.
  From Q430 (`hud.md`, "The person's windows"): the visitor's window is built, so the nearest piece is the staff
  member's window (built, Q431; its verbs and the patrol tool are not; Q31b lists it among the seven). Counted, not built:
  the visitor's preview model (`VISITOR_WINDOW_PREVIEW`, `FUN_00486530`, a copy of the guest's sprite turning in the
  panel), a guest gone while the window is up (`VISITOR_WINDOW_GUEST_GONE`; the original clears nothing, not decoded),
  the visitors list's own advisor line `0x11e` (`VISITORS_LIST_ADVISOR_LINE`), a guest's world click (the pick finds only
  a cell's occupant, so no guest: `FUN_004879d0` category 7 to `c`). Open: the original put `0x12e` in no slot on its
  second to fourth opens (not read why). Not run in the original: a thought shown in the window (ours drew thought 12,
  "Bored", `q430/run-fix2/8-row.png`). In no game run: a guest leaving while it is up (tested only), Full Simulation
  (not asked: the window is either mode's), the other three themes. Not heard.
  From Q429 (`hud.md`, "The person's windows"): both windows are decoded, so the nearest piece is building the
  visitor's window (`VISITOR_WINDOW`; from the strip's click, a guest's world click and a visitors row's `0x400`, which
  counts `LIST_ROW_ACTIVATED_UNANSWERED`), then the staff member's (`STAFF_WINDOW`: its verbs and the patrol tool make it
  the larger; Q31b lists it among the seven). Not run in the original: a thought shown, either window's verbs, arrows
  or name edit, either opened from the world or a list, Full Simulation (not asked: the windows are either mode's), the
  other three themes. Not heard.
  From Q428 (`hud.md`, "The camcorder button's line"): the line is built. The locators' last piece is still the strip's
  click, the person's window (`STAFF_WINDOW`, `VISITOR_WINDOW`: `FUN_004b6080`, `FUN_004b79b0`, not yet decoded; the
  employee window is Q31's), the nearest. In no game run: a tag taking the arm from the camcorder panel and a panel
  refused (tested only), the line let end and said once, Full Simulation (not asked: the button is either mode's), the
  other three themes. Not heard in either game.
  From Q427 (`hud.md`, "The slide, measured"): the arm's slide is built. The locators' last piece is the strip's click,
  the person's window (`STAFF_WINDOW`, `VISITOR_WINDOW`), the nearest. Counted, not built: the camcorder button's
  advisor line `0x131` (`FUN_00498bb0` posts it, `FUN_00498bd0` withdraws it; built, Q428). In no game run:
  a panel refused while another waits (tested only), Full Simulation (not asked: the arm is either mode's), the other
  three themes. Not heard in either game.
  From Q426 (`hud.md`, "The patrol edges"): a fenced member's patrol edges are built. The locators' last pieces are the
  strip's click, which opens the person's window (`STAFF_WINDOW`, `VISITOR_WINDOW`; the employee window is Q31's, and
  the patrol tool, `0x510`, map tool `0x36`, is on it), and the arm's slide (built, Q427), the nearest. In no
  game run: a patrol edge over a built cell (no stock area crosses one; the lift is the tools', tested), Full
  Simulation (not asked: the locators are either mode's), the other three themes.
  From Q425 (`hud.md`, "The staff and visitor locators"): the locators' two advisor lines are built, so the locators'
  nearest piece is a fenced member's patrol edges (built, Q426). In no game run:
  Ctrl+S or Ctrl+V with a person's window up (none is built; `FUN_004b3f60` given a visitor posts `0x11f`), the lines
  through a save and a load (written with every row's record; not run), the Tutorial option off (tested only), Full Simulation (not asked: the
  locators are either mode's), the other three themes. Not heard in either game.
  From Q424 (`hud.md`, "The staff and visitor locators"): the locators are built. Counted, not built: the advisor's
  two lines as they open, `0xc6` (staff) and `0x11f` (visitors) (built, Q425); a fenced member's patrol edges
  (built, Q426); the strip's click, the person's window (`STAFF_WINDOW`,
  `VISITOR_WINDOW`), and with it Ctrl+S or Ctrl+V over a person's window opening on that person. The arm puts the panel
  out at once (built to slide, Q427). In no game run: a kind with nobody, a person leaving while followed (tested
  only), the strip's hover and click, first person over a follow, Full Simulation (not asked: the locators are either
  mode's), the other three themes.
  From Q423 (`advisor-park.md`, "Kids who got away from a guard"): row 22 is built, so every guards' row is. In no game
  run: lines 2 and 3 (the test holds the turn), a shut park (the test holds it scoring), a kid getting away in play
  (nothing here counts one: the pranks), two guards or more (tested only), Full Simulation (not asked: the row is either
  mode's), the other three themes. Not heard in either game.
  From Q422 (`advisor-park.md`, "Research too slow, and researchers to hire"): row 17 is built, so every staff row, 5 to
  17, is. Row 22 (`0x0059d3b0`) reads the got-away count with a guard on the staff, the nearest of the guards' rows (built, Q423). In no
  game run: the line with the shipped fifteen months (the clock would pass every candidate's time in the pool; tested
  only), research at 14 per cent or more, a most of nought (tested only), lines 2 and 3 (the test holds the turn), a
  researcher hired after the line, Full Simulation (not asked: the row is either mode's), the other three themes. Not
  heard in either game.
  From Q421 (`advisor-park.md`, "Most or every kind of ride built, and no researcher"): rows 15 and 16 are built. The
  last staff row, 17 (`0x0059cfe0`, `StaffHireResearchers3`, category 4, its keys `MonthsIntoGameForConsideration` and
  `SlowerThan`), is not (built, Q422); row 22 (`0x0059d3b0`) reads the got-away count with a guard on the staff, the nearest of the guards'
  rows (built, Q423). In no game run: row 15 said with the shipped keys (Instant Action's four kinds cannot pass 25: tested, and seen
  under the threshold), lines 2 and 3 of row 15 and line 2 of row 16 (the test holds the turn), none on offer (row 16's
  90, tested only), a researcher hired after the line, Full Simulation (not asked: the rows are either mode's), the
  other three themes. Not heard in either game.
  From Q418 (`advisor-park.md`, "Kids who got away"): row 98 is built. The other staff rows, 15 to 17, are not: rows 15
  and 16 the kinds of ride on offer and standing with no researcher (`0x004ca450`, `0x004ca7b0`; the nearest; built, Q421), row 17 the
  months into the game (built, Q422). Row 22 (`0x0059d3b0`, built, Q423) reads the same got-away count with a guard on the staff (its lines ask for
  security cameras), the nearest of the guards' rows. In no game run: the second line (sample 292; its gesture row is
  counted, the test holds the turn), a kid getting away in play (nothing here counts one: the pranks), a shut park (the
  test holds it scoring), Full Simulation (not asked: the row is either mode's), the other three themes. Not heard in
  either game.
  From Q417 (`advisor-park.md`, "Litter and no handyman"): rows 8 and 9 are built. The other staff rows, 15 to 17, are
  not: rows 15 and 16 the kinds of ride on offer and standing with no researcher (`0x004ca450`, `0x004ca7b0`; the
  nearest; built, Q421), row 17 the months into the game (built, Q422). Row 98 is built (Q418). Nothing here drops or sweeps litter (`DECIDE_LITTER_DROPPED`,
  `HANDYMAN_LITTER_SEARCH`), so the share is the file's; the original's handyman swept a cell every 15 or 16 ticks. In
  no game run: lines 2 and 3 (the test holds the turn), a share of nought or at the cut-offs themselves (tested only),
  two handymen, a handyman hired after the line, Full Simulation (not asked: the rows are either mode's), the other
  three themes. Not heard in either game.
  From Q416 (`advisor-park.md`, "A busy park with no guard"): row 11 is built, the first row of category 9 (said only
  once). The other staff rows, 8, 9 and 15 to 17, are not (rows 8 and 9 are built, Q417): rows 8 and 9 the litter (`FUN_004ccdf0`, with no handyman;
  the nearest), rows 15 and 16 the kinds of ride on offer and standing with no researcher, row 17 the months into the
  game (built, Q422). In no game run: lines 2 and 3 (said once, they come only after a withdraw cuts the first; the test holds the
  second with the category's key off), a crowd past 100 or under 70 (tested only), a guard hired after the line, the
  said byte through a save and a load, Full Simulation (not asked: the row is either mode's), the other three themes.
  Not heard in either game.
  From Q415 (`advisor-park.md`, "A busy park with no entertainer"): row 14 is built. The other staff rows, 8, 9, 11 and
  15 to 17, are not (row 11 is built, Q416; rows 8 and 9, Q417); their scores are read there: rows 8 and 9 the litter (`FUN_004ccdf0`, with no handyman), row 11 the
  crowd over 70 with no guard after `DaysForStaff` (the nearest: it asks only what row 14 and row 13 already read),
  rows 15 and 16 the kinds of ride on offer and standing with no researcher, row 17 the months into the game (built, Q422). In no game
  run: lines 2 and 3 (the test holds them), a crowd under 35 (a score below nought, tested only), a crowd past 100 (tested
  only), an entertainer hired after the line, Full Simulation (not asked: the row is either mode's), the other three
  themes. Not heard in either game.
  From Q414 (`advisor-park.md`, "Lots of pranks"): row 264 is built. The other staff rows, 8, 9, 11 and 14 to 17, are
  not (rows 8, 9, 11 and 14 are built, Q415 to Q417). The original, given a file with no guard (Q413's `three.TPWS`,
  `q414/two.TPWS`), said a line and raised its tag on tick 828, "Some of the visitors are misbehaving and your security
  hasn't seen it...": the polled row 98, raised by the files' nine kids who got away (built, Q418). In no game run: a shut park (the
  test holds it), the line said twice, a kid counted in play, Full Simulation (not asked: the row is either mode's),
  the other three themes. Not heard in either game.
  From Q413 (`advisor-park.md`, "A park with no guard"): row 12 is built. The other staff rows, 8, 9, 11 and 14 to 17,
  are not (rows 8, 9, 11 and 14 are built, Q415 to Q417); row 264 (`LotsOfPranks`, `0x005a04f0`, more than two kids this month), which the original posted on tick
  903 in all three of Q413's files, is the nearest (it reads the same count; built, Q414). In no game run: lines 2 and 3 (the test
  holds them; the third's gesture row is counted), a kid counted in play (nothing here counts one: the pranks), two
  guards or more, a guard hired after the line, Full Simulation (not asked: the row is either mode's), the other three
  themes. Not heard in either game.
  From Q412 (`advisor-park.md`, "A park too big for its guards"): row 13 is built. The other staff rows, 8, 9, 11, 12
  and 14 to 17, are not (rows 8, 9, 11, 12 and 14 are built, Q413 to Q417). In no game run: lines 2 and 3 (the test holds them), a guard over his 750 (776 cells; the
  test holds it), two guards or more, a guard hired after the line, Full Simulation (not asked: the row is either
  mode's), the other three themes. Not heard in either game.
  From Q411 (`advisor-park.md`, "A park too big for its handymen"): row 10 is built. The other staff rows, 8, 9 and 11
  to 17, are not (rows 8, 9 and 11 to 14 are built, Q412 to Q417); row 13's keys name developed cells for the guards (`StaffHireGuards3`), the nearest (built, Q412). In no game run:
  lines 2 and 3 (the test holds them), two handymen or more, a handyman hired after the line, the minimum of 300
  (it asks nothing a handyman's 400 does not, at the shipped values: tested only), a region's kept count reused in
  the same quarter tick (counted afresh here, said at `ParkState.DevelopedCells`), Full Simulation (not asked: the row
  is either mode's), the other three themes. Not heard in either game.
  From Q410 (`hud.md`, "The research opener and its two boxes"): the boxes are built; the research screen (six effort
  sliders, `FUN_004a96b0`) is counted and waits on the lab (Q245). In no game run: the bar's type `0x13` (its rows
  score only in Full Simulation; the test holds it), a carried hire that is a researcher, the other three themes.
  From Q397 (`hud.md`, "The analyser's other stamps"): `mParkLastOpened` is kept and nothing in either game reads it. In
  no game run: an open from a shortcut key (`InputButton.OpenPark` is not bound), a park made with no file, Full
  Simulation (not asked: the door is either mode's), the other three themes.
  From Q396 (`hud.md`, "The arrivals"): the arrival manager's ring of five loads is kept. Nothing here reads it: the
  challenge whose test it is (`FUN_005a0780`, in the table at `0x00772088`; the challenge's `+0x5f4` once exactly one
  load is called) is not built, nor are the challenges. A load the original calls of nobody (world state 4, the park
  full) is not called here, so not pushed. In no game run: the ring gone round (tested only), a load called in a
  file loaded mid-load, Full Simulation (not asked: the arrivals are either mode's), the other three themes.
  From Q395 (`hud.md`, "The misbehaving kids"): the three counts are kept. Nothing counts into them: the guard's chase
  (`FUN_004d6260` to `FUN_004d6790`) waits on the pranks (`DECIDE_PRANK_*`), and its catch's sound `0x89` with it. The
  advisor's row 264 (`LotsOfPranks`, message `0x108`, score `0x005a04f0`: 100 past two this month) reads the month's count
  (built, Q414). The original's third count after a load is its memory's
  (nought, or -999 once); nought here (said at `ParkState`). Why the original's park loaded at tick 755 ran no close in
  120 s (`q395/orig/a3-load.log`: its words stood still past tick 788 with the clock true) is not looked into. In no
  game run: a kid counted, Full Simulation (not asked: the counts are either mode's), the other three themes.
  From Q394 (`hud.md`, "The park score"): the 16th ring is kept, so all sixteen are. Nothing here reads it: the park
  status screen (`PARK_STATUS_SCREEN`), which graphs it, is not built. The misbehaving kids' two counts the close zeroes
  are kept (Q395). In no game run: a park past 50 guests
  inside or past fourteen rides (the caps, tested only), a sideshow, shop or feature bought or sold before a close, a
  member hired or fired before a close (tested only), Full Simulation (not asked: the ring is either mode's), the
  other three themes.
  From Q393 (`hud.md`, "The leavers' stays"): the stays and the 15th ring are kept. Nothing here reads them: the park
  status screen (`PARK_STATUS_SCREEN`), which graphs the 15th, is not built. The 16th is kept (Q394), and the misbehaving
  kids' two counts the same close zeroes are kept (Q395). A stay is handed where `PeepBehaviour` puts a guest walking outside
  at the stop, the one way in here; the original hands it on any entry to state `0x15`. In no game run: a guest who
  never paid at the stop (tested only), a stay ring gone round fifty, a stay of a guest stamped after the clock
  (negative, tested only), Full Simulation (not asked: the rings are either mode's), the other three themes.
  From Q392 (`hud.md`, "The guests' happiness at a close"): the 14th ring is kept. Nothing here reads it: the park
  status screen (`PARK_STATUS_SCREEN`), which graphs it, is not built. The 15th is kept (Q393), and the 16th
  (Q394). In no game run: a close with the park shut
  (tested only), a guest's happiness below nought or past 255 (tested only), a ring gone round 144 months, Full
  Simulation (not asked: the ring is either mode's), the other three themes.
  From Q391 (`hud.md`, "The arrivals"): the 13th ring is kept. Nothing here reads it: the park status screen
  (`PARK_STATUS_SCREEN`), whose first series it is, is not built. The arrival manager's ring of five loads is kept
  (Q396). In no game run: a load dropped across a close, a stop that refuses a guest (this
  game's own end of a load; nothing added), a ring gone round 144 months, Full Simulation (not asked: the ring is either
  mode's), the other three themes.
  From Q390 (`hud.md`, "The guests inside and the rides at a close"): the 10th and 12th rings are kept. Nothing here
  reads them: the park status screen (`PARK_STATUS_SCREEN`), which graphs the 10th beside the 13th to 16th, is not
  built; nothing reads the 12th. The 13th to 16th are kept (Q391 to Q394); the two counts' kept answers (`+0x1f0b8` to
  `+0x1f0cc`) are still the file's bytes. An upgrade placed
  in an Instant Action park is not catalogued here, so not counted as a ride. In no game run: a ride condemned or
  broken at a close (tested only), an upgrade placed, a ring gone round 144 months, Full Simulation (not asked: the
  rings are either mode's), the other three themes.
  From Q389 (`hud.md`, "The lifetime visitors"): the count, the dearest and the 11th ring are kept. Nothing here reads
  them: the park status screen (`PARK_STATUS_SCREEN`, which letters the count) and the golden tickets (Q141) are not
  built; the gate fee's challenge message is counted (`GATE_FEE_CHALLENGE_POST`). The analyser's other six int rings
  are still the file's bytes (`ANALYSER_MONTH_CLOSE`); the 10th (`FUN_004c7fa0`) and the 12th (`FUN_004c8070`) stand
  beside it in the close. In no game run: a fee of nought, a fee lowered below the dearest, a ring gone round 144
  months, Full Simulation (not asked: the gate fee is either mode's), the other three themes.
  From Q388 (`hud.md`, "The park value"): the park value ring is kept. Nothing here reads it (no finances or loans
  screen); the original's kept answer (`+0x1f0e8` to `+0x1f0f0`) is the file's bytes, and this game sums afresh where
  the original's close can reuse a sum of the same quarter tick. In no game run: an upgraded or condemned object, a
  cell bought outside the park, a ring gone round 144 months, Full Simulation (not asked: the ring is either mode's),
  the other three themes.
  From Q387 (`hud.md`, "The mini-balance is a MONTHLY ACCOUNT"): the balance ring is kept. The analyser's other
  int rings are not (`ANALYSER_MONTH_CLOSE`; the lifetime visitors' is kept, Q389), so a file this game writes past a
  month holds them a slot behind the money rings. The finances screen is not built here. In no game run: a balance below
  nought at a close, a loan's instalment in the month (no screen), a ring gone round 144 months, Full Simulation (not
  asked: the ring is either mode's), the other three themes.
  From Q386 (`hud.md`, "The mini-balance is a MONTHLY ACCOUNT"): the finances screen (`FUN_0049a2e0`, F or the Money
  button's `b_finance`) is not built, so nothing here reads the training, gate, shop and sideshow rings; the original
  reads them from this game's file. In no game run: a training share with withdrawals off (tested only), a grade-4
  member's training, a ring gone round 144 months, Full Simulation (not asked: the rings are either mode's), the other
  three themes.
  From Q385 (`hud.md`, "The mini-balance is a MONTHLY ACCOUNT"): the analyser's other nine int rings (the month-end
  balance, `+0x1f350`, the lifetime visitors and more; Q386 named each; Q387, Q388 and Q389 keep three, and the
  lifetime visitors and dearest ticket) are not kept, and a written file keeps the file's bytes for them
  (`ANALYSER_MONTH_CLOSE`). A park made with no file starts the rings empty and is not written
  (`SAVE_PARK_WITH_NO_FILE`). The original closes a month on the first tick after loading a file this game saved past
  its own first month (this game's calendar, Q149). In no game run: a loan taken or paid (no screen), a month with
  withdrawals off, a ring gone round 144 months, Full Simulation (not asked: the account is either mode's), the other
  three themes.
  From Q384 (`hud.md`, "How a list's cells are aligned"): the hire and buy lists' alignments are compared cell by cell.
  In no game run: Full Simulation (not asked: the screens are either mode's), the other three themes.
  From Q383 (`hud.md`, "How a list's cells are aligned"): every all-items count is centred. The language `0x40d`
  (Hebrew) swap of left and right is not kept (no Hebrew data ships). In no game run: Full Simulation (not asked: the
  screens are either mode's), the other three themes.
  From Q382 (`hud.md`, "The all-staff list's first row"): the list picks its first row. The original's right click
  below the rows shut the screen and moved the view, onto which member is not read (no reader of its camera). In no
  game run: a member hired or gone with the screen open (where the index then stands), Full Simulation (not asked:
  the screen is either mode's), the other three themes.
  From Q381 (`hud.md`, "The all-staff list's sort"): the list sorts; its first-row pick is Q382's. In no game run: the
  skill or the happiness heading clicked, a member hired or gone with the screen open, a status changed by the rewrite after a sort (tested only in the original's frames, where none
  changed), Full Simulation (not asked: the screen is either mode's), the other three themes.
  From Q380 (`hud.md`, "The all-staff status words"; `saves.md`, "What a made record leaves at nought"): a hire carries
  the whole map. A record with no area, nought and nought (a test's, or a file this game wrote before Q380), is still
  taken as the whole map here where the original's `FUN_00506ed0` puts it outside every cell (a deviation said at
  `Staff.Patrols` and `PatrolsWholeMap`). In no game run: a patrol roll that routes from a dead end (no stock cell
  found), a hire in Full Simulation (not asked: the hire is either mode's), the other three themes.
  From Q379 (`hud.md`, "The all-staff status words"): the hire's patrol area is Q380's. A member of state 2 or 3 with no rest area faults the original (`FUN_00505fe0`,
  `FUN_00506d10` read thing `[+0x208]`), and this game takes it on (not looked into). In no game run: state 6, a kind's
  own job states but `0xe` and `0xf`, a status changed under the open screen's rewrite (tested only), Full Simulation
  (not asked: the screen is either mode's), the other three themes.
  From Q378 (`hud.md`, "The rows' font and height"): the hire and buy rows are the factories'. Seen beside the
  original's and filed already: the "$ " before a price and a wage and the empty box of a row with no state (Q158's
  "From Q238", "From Q130e"), the buy list's first row picked (Q232). In no game run: a hire or buy tab past its visible rows, this game at
  another font set than 2, Full Simulation (not asked: the screens are either mode's), the other three themes.
  From Q377 (`hud.md`, "The rows' font and height"): the rows are the factories'; the hire and buy lists' are Q378's.
  The original stands at 800 x 600 since, where its rows are this game's 39; in no game run: this game at another font set than 2, the original at 1024 x 768, Full Simulation (not asked: the
  screens are either mode's), the other three themes.
  From Q376 (`hud.md`, "The pointer over a list"): every list follows the pointer. In no game run: the original's
  all-staff list with two members of a kind, a row bought, sold, come or gone under a pointer at rest (no move, no
  pick, in this game as `WindowStack` sends a move only on a frame the pointer moved), Full Simulation (not asked: the
  screens are either mode's), the other three themes.
  From Q375 (`hud.md`, "The miscellaneous tab: a row an object, and each tab's sort"): the all-items lists pick their
  first row, and follow the pointer (Q376). In no game run: an empty tab, a row bought or sold with the tab open, Full Simulation (not
  asked: the screen is either mode's), the other three themes.
  From Q374 (`hud.md`, "The miscellaneous tab: a row an object, and each tab's sort"): the misc list picks no first row
  as it fills, where the original's picks it (the other lists' pick, Q232). In no game run: a misc object bought or sold
  with the tab open, Full Simulation (not asked: the screen is either mode's), the other three themes.
  From Q373 (`hud.md`, "The thought"): the thought cell is built. In no game run: thoughts 19 to 22, a thought changed
  under this game's open screen (its two-second rewrite is tested, not run), Full Simulation (not asked: the screen is
  either mode's), the other three themes.
  From Q372 (`hud.md`, "The visitors rows", "The time in park"): the time in park is built. Its rate is
  `GameCalendar.Rate`, the clock constructor's, where the original's is the park calendar's own `+0x1c` (the save's
  clock block is not read: Q149). The original's larger lettering clips a nine-digit count at both ends of its cell,
  where this game's fits. In no game run: an arrival's row with the screen open (its stay nought in either game), Full
  Simulation (not asked: the screen is either mode's), the other three themes.
  From Q371 (`hud.md`, "The visitors rows: a number, a money cell, a bar"): the visitors' number, cash and happiness are
  built, and the time in park (Q372) and the thought (Q373), and the row height (Q377). The alignment message `0xc00` and the all-staff styler's alignments are read (Q383). In no game run: a guest with negative cash,
  a happiness past 255, Full Simulation (not asked: the screen is either mode's).
  From Q370 (`hud.md`, "The list headings' frames"): the headings are framed; the original's lettering is larger and
  wraps "Monthly Wage". Seen beside the original in `q370/side-by-side.png`, not read and not counted: the misc tab's
  row an object is built (Q374); the visitors list has no scrollbar (the original's has); its number, cash and
  happiness cells are built (Q371).
  From Q369 (`hud.md`, "The all-staff rows: a money cell, then bars"): the row height is built (Q377); the status's own words are Q379's; the list's sort is Q381's. Five other callers hand the stat bars'
  skin `happygrad.wct` (`FUN_004939d0`, `FUN_004afc10`, `FUN_004b1040`, `FUN_004b53c0`, `FUN_004b6c20`): which
  screens' bars these are and what this game draws there is not looked at. The all-staff meters' every-member value is
  this game's mean `x 1024 / 100` where the opener `__ftol`s `FUN_004c7cc0`'s answer with no scaling in the decompiler's
  output (the listing not read; both games' meters read about 0.83 to 0.86). In no game run: a grade-4 member past 100,
  a member with no name, Full Simulation (not asked: the screen is either mode's), the other three themes.
  From Q368 (`hud.md`, "The all-items rows: a number, then bars"): in no game run: a figure moved by a sale in this
  game's frame, a ring of fewer than thirty days with a satisfaction that is not nought, a satisfaction past 255.
  From Q367 (`hud.md`, "The all-items rows: a number, then bars"): the shops' and sideshows' money and satisfaction
  are built (Q368); the sideshow rows stand by name, as the original's (Q374); the items rows are the factory's font-7 height (Q377); its headings are framed
  (Q370). The all-staff list's skill and happiness
  are bars (Q369). In no game
  run: a track ride's row (the fill's `0x2000` walk, `0x00493eb0`), a value past 255, Full Simulation (not asked:
  the screen is either mode's), the other three themes.
  From Q366 (`hud.md`, "The shortcuts' screen rows"): the staff and visitor locators (`FUN_004b3f60`, Ctrl+S and
  Ctrl+V, a panel at the gadget's foot in the original) are built (Q424); the postcard row is counted
  (`POSTCARD_SHORTCUT`). W and D still scroll the orbit camera, the original's camera table binding the
  arrows and the keypad (`ParkOrbitCameraMode.ScrollAxes`; Alexah kept them, 2026-10-14). In no game run: L in Full
  Simulation (counted there), a shortcut under an object window, Ctrl+P in the original.
  **Note (fork review 2, 2026-10-16, `verify-ui-hud.md` L1; built, Q424):** the locators are decoded whole. The panel's procedure
  is `FUN_004b3790`: radio buttons `0x95e`..`0x964` pick roles 1 to 6 (handymen, mechanics, guards, entertainers,
  researchers, visitors); a role walks its staff chain (world `+0x1da73c`..`+0x1da744`, next at `+0x210`) or the
  visitors newest first, and "next" wraps. The strip (`FUN_004b3490`) letters the staff name or the visitor number
  (UITEXT `0x1c8`, `0x1c9`) with a state or thought icon; each frame the camera's look-at snaps to the actor
  (`FUN_00486860`, `FUN_0042aab0`) and a staff member's patrol edges are drawn. Advisor lines `0x11f` and `0xc6` are
  posted and withdrawn.
  From Q365 (`hud.md`, "How allstaff and allitems keep their rows current"): A row's status is the game's own word (Q379); its skill and happiness are bars (Q369);
  the items columns are Q367's. What the original does to these two lists as a member or an object comes or
  goes with the screen open is not read (allpeeps' `0x1b`/`0x1c`). In no game run: the sideshows' tab rewritten, the
  timer held under a message box, Full Simulation (not asked: the screens are either mode's).
  From Q363 (`render-states.md`, "Texture addressing"): **a texture one model wraps and another clamps keeps each
  material's own way here** (`TEXTURE_WRAP_SHARED`, five in the lobby and the stock park), where the original's one
  texture per folder and name takes every user's bits for as long as it is loaded. The interface's meshes (`UiMesh`
  picks its own sampler), the ground, the paths and the hoardings were not compared (paths and signs: Q15). The
  renderer's mode `[0x008bd524]` is taken as 1 (hardware), not read in the running original. In no game run: the
  lobby's islands up close (the Space island's `sen_pnl` panels now clamped, its gate's wrapped), an item outside
  the stock park, the object window's preview, the other three themes, Full Simulation (not asked: the models are
  the level's).
  From Q362 (`textures.md`, "Which materials the loader makes opaque"): seen only in the stock park. **Not photographed
  in either game**: the lobby's Space island (`sfl_cnr3`, `sho_top1`..`4`, now see-through), the floors newly opaque
  (`totem`, `puzzle`, `minecart`, `mumbo`'s planks, `s_plant`), the `P` models' roots in the object window (`Pgokarts`,
  `Pwateride`, `Pcoaster3`'s leaves), the alphas newly drawn on no floor (`jfe_rck1`/`2`, `mumbo`'s tongue, `coaster1`'s
  leaves). Behind the bridge's see-through waterfall this game shows greener ground than the original's dark (not
  looked into). Type-3 items' pieces (`FUN_0042fad0`, flags `0x1013405`) are not loaded here. The original's stop of the
  floor walk past the ground's last cell is not kept (no Lost Kingdom mesh reaches past 502 units of its 850). Material `0x2`'s depth-write role
  (`render-states.md`, the 2026-10-03 trace) is not looked into. The hoardings still take mesh 0 for the floor
  (`ParkObjects`), which `ProceduralBase.FloorMesh` matches on every Lost Kingdom item. Which routine draws a
  procedural base each frame and which items take flag `0x1` beyond the table are not read further. Not run: the other
  three themes, Full Simulation (not asked: the models are the level's).
  From Q358 (`textures.md`, "The loader"): the reference is byte 1 since Q359. The thin pale line along each river bank's outer edge is gone (Q363). In no game
  run: the lobby's islands up close (the reference player opens only Lost Kingdom), the message box (`ipan`), the
  Hot Pot, the Jungle Spray's bench and the other shops' alpha, Full Simulation (not asked: the textures are the
  level's).
  From Q357 (`textures.md`, "A sign's artwork"): **the original under Proton letters no name on Lost Kingdom's
  park gate**, where this game letters "Lost Kingdom" (the Windows screenshot's Wonder Land board has its name:
  Wine's GDI fonts are not looked into). This game's gate slab top is 1.27 to 1.34 times the original's (the light
  or the model's LZSS texture, not looked into). Which of the board's two modes the original takes is not read
  (mode 1 copies the bytes as they are). `FUN_00592730`'s transform is not compared with `FUN_00593ab0`'s
  instruction by instruction. The lobby's boards were not photographed in either game (the player buttons cover
  them in the original's orbit). In no game run: the ride boards (the coasters', `sign1`), the other three themes'
  parks, Full Simulation (not asked: the boards are the level's).
  From Q355 (`textures.md`, "The loader"): the alpha chunk by the bit count is built (Q358). The LZSS codec
  (`FUN_00595920`, type `0x13`) is not compared with this game's full-scale path, nor its rounding (this game's
  truncates). Three type `0x12` files are LZSS and Fan_stalk's, Bowler_Btie's and s_disc's 8 x 8 copies throw in
  this game's LZSS reader.
  From Q356 (`hud.md`, "The first row is picked as a list fills"): the other lists' pick is Q232's. The opening fills it once (Q364). A list emptied before
  its pick is taken is handed -1, where the original's handler goes on with a stale word. In no game run: an empty
  tab, a candidate joining an empty list with the screen open, Full Simulation (not asked).
  From Q353 (`hud.md`, "The stat bars' skin"): the `.wct` reader's red is Q354's (the header's scales); the hire
  list's first-row pick and its "Skill" are built (Q356), and the original's five handymen were its pool's top-up on
  tick 1083, after this game's clicks (Q356). The track is 49 grey there and 58 here. In no game run: the object window's
  bars in the original, a full bar, the other three themes.
  From Q352 (`hud.md`, "The Staff Training Budgets screen"): the account's Cash in, Other costs and Balance are
  built, on it and on the hire screen (Q385).
  The skill bars wear `ridestatbar.wct` (Q353). The screen's lettering is smaller than the original's at this window size (not looked into). The
  finances and loans screens its buttons go to are not built. What reads `mHaveEverTrained` was not looked for.
  No file of the original's own with a budget was made (its memory was read). **This game's month does not
  turn on the file's calendar** (Q241j: 715 ticks after a load, where the original's turned on 1383). In no
  game run: Full Simulation (the loans button enabled there), a budget at 10000, the three other themes.
  From Q351 (`advisor-park.md`, "Staff nobody has trained"): the bar's staff-costs button is built (Q352).
  The gesture rows of responses 82, 84 and 90 are counted
  (`ADVISOR_RESPONSE_GESTURE_ROW`). **The handymen's cap is paler than the original's**, as the other hats
  are (the advisor's light, below). In no game run: lines 2 and 3 of any row, a line again past its 120
  quarter-ticks, two members of a kind, a park made with no file, Full Simulation (not asked: the rows are
  either game's and read neither). Not heard in either game.
  From Q350 (`advisor-park.md`, "Every sideshow built, and more to research"): the bar's research button (type
  `0x13`, `FUN_004aa480`) runs the opener, its boxes built (Q410). The training lines are
  built (Q351), so row 76's line now waits for the researchers' here as there: not run again. A kind's current project and highest group, `mMostExpensiveProject` and
  `mLabActive` are not made; the lab's own turn (`FUN_00503430`: 0.9 points every 20 ticks to a sideshow from
  ability 6) is not read. In no game run: line 2, the line again past its 900 quarter-ticks, the row in the
  original's own Full Simulation park. Not heard in either game.
  From Q349 (`park-engine.md`, "A fresh world's research lab"): **a fresh park's lab has its projects and
  the kinds' efforts and outputs** (Q350): each kind's current project and highest group,
  `mMostExpensiveProject` and `mLabActive` are decoded (`FUN_00502d60`, `FUN_00503330`) and not made, as no
  reader of them is built; a finished project's next upgrade (event `0x11`, `FUN_00503d40`) is not made; an
  upgrade item is catalogued and nothing builds one; every item is taken as loaded (`+0x508`). In no game run:
  row 80 in the original's own fresh park (it was not driven to hire and build), the other three themes' fresh
  labs, a fresh park saved (`SAVE_PARK_WITH_NO_FILE`). Not heard in either game.
  From Q348 (`advisor-park.md`, "Every feature built, and more to research"): nothing here researches, so a
  park's projects never finish. Of the lab's record a kind's current project and highest group,
  `mWorkLoad`, `mMostExpensiveProject` and `mLabActive` are named and not read. **Twenty-nine polled rows of mode 0
  are unbuilt** (18 to 21, 23 to 27, 61, 63 to 65, 72, 101, 102, 171, 274 to 276, 278 to 281, 314, 316, 317,
  347, 348); row 76 is built (Q350). **Rows 83 and 84 have no
  poster found** (row 83's score is a stub answering nought): not proven dead. In no game run: a Full
  Simulation park (Q327), the line again past its 900 quarter-ticks, a project finished by the original's own
  research. Not heard in either game.
  From Q347 (`advisor-park.md`, "A park with no toilet"): **left alone, this game's guests and the original's
  do not come and go alike**: of four loads of `q347/lvl60.TPWS` the original said row 82's line on tick 1284
  in one (three of eleven or fewer inside over 60: not counted in that run) and this game's one run was silent
  through tick 1304; only the turn straight after a load was compared. Rows 83 and 84 (`FeaturesFarToToilets`,
  score `0x0059e410`) are posted and who posts them is not read; row 80 is built (Q348). In no game
  run: lines 2 and 3, the line again past its cooldown, a share of exactly 25 in this game (3 of 12 was silent
  in the original; the test holds 1 of 4), a guest outside the park, a park made with no file, Full Simulation
  (not asked: the row reads neither mode). Not heard in either game.
  From Q346 (`advisor-park.md`, "The theme's own advisor and its costume"): **the pith helmet is paler than the
  original's**, yellow with a white band where the original's is tan with a grey one, as the hard hat is paler:
  `AdvisorModel`'s light is set against the lobby model's gloves, and the original's light (26.67, -26.67, 800
  at `FUN_00429ba0`) is not decoded into a space. Clips 16 to 20 of the theme's model (the map, the tickets, the
  mutant eye, the golden ticket's gesture rows) are not built. Which model a line plays on, the costume handed
  to it and the chain's lengths are `Advisor`'s, which no test reaches: the game run is their proof. Not run:
  the original this session (its frames are Q344's and Q345's), a slot 0 line straight after a slot 1 line in
  the original, the three other themes' models and costumes, Full Simulation (not asked: the model is the
  level's in either mode). Not heard in either game.
  From Q345 (`advisor-park.md`, "A park with sideshows and no feature"): row 80
  (`0x0059e260`, `FeaturesResearchMore`) is not read; row 76 is built (Q350). In no game run: the line
  again past its cooldown, score 22 on tick 1438 (a summons falls on that tick in both games), day 29 said a
  turn later in this game (a vehicle's stamp came first; the original's was held by a write), a golden-ticket
  sideshow or feature, a feature built after the line, the bar's button with the buy screen open, a park made
  with no file, Full Simulation (not asked: the row reads neither mode). Not heard in either game.
  From Q344 (`advisor-park.md`, "A poor selection of features"): **row 79's line is the theme's own
  advisor's** (model slot 1, prop -1; built, Q346). Row 81 is built (Q345),
  and row 76 is built (Q350). In no game run: line 2, the line again past its cooldown, day 29
  in the original, no kind of feature on offer, a golden-ticket feature, a feature built after the line, the
  bar's button with the buy screen open, a park made with no file, Full Simulation (not asked: the row reads
  neither mode). Not heard in either game.
  From Q343 (`advisor-park.md`, "Too few sideshows for the park's size"): **the one park in which the shipped
  game says row 78's line was run in neither game**: five kinds of ride to one of sideshow, which needs Full
  Simulation and its research (the test holds it; Q327's catch-up is where it belongs). Rows 79 (`0x0059e1c0`)
  and 81 (`0x0059e2e0`) are not read, and row 76 is built (Q350). In no game run: line 2, the line again
  past its cooldown, day 29 in the original, a golden-ticket ride or sideshow, a sideshow built after the line,
  the bar's button with the buy screen open, a park made with no file. Not heard in either game.
  From Q342 (`saves.md`, "OpenTPW's writer, a vehicle made at its summons"): the made vehicle's id is one past
  the highest used, the original's the id last freed (14 where this game gave 43; Q26c). Its model and script
  stand from the load under an id of the scene's own and its script's handle is taken then, where the original
  makes all three at the summons and starts the script from its top (Q26c). **The original's ferry's rings
  hold numbers** (`q257x/orig/fixed-by-the-original.TPWS`, thing 32, `+289` on); what counts into a vehicle's
  rings is not read, and this game's are nought. The default cell a fixed item's model is made on
  (`DAT_00785594`, `DAT_00785598`: 48 and 17 in every Lost Kingdom record) is not traced to its key, and no
  other theme's is measured. In no game run: the second vehicle made in one park (the test holds it), a load
  of sixty-one's own summons, a file with both made loaded in the original, a park made with no file, Full
  Simulation (the summons is the arrival manager's in both modes: not asked).
  From Q341 (`advisor-park.md`, "What moves it with nothing bought"): **the leavers' draw is this game's own
  generator**, the original's `% 3` of the save's seed (`+0x1da708`):
  the original drew the third vehicle on tick 1094 in every load of `q340/late30.TPWS`, this game the
  seaplane, the ferry or the bus on tick 1093 (a tick early, not looked into), and the bus stamps nothing, so
  one run in three still says rows 60, 71, 73, 75 and 77 early; what else draws on that seed between a load
  and the summons is not read. In no game run: the second vehicle's stamp in this game, a load of sixty-one's
  summons, a park made with no file, Full Simulation.
  From Q340 (`advisor-park.md`, "A park with shops and no sideshow"): the original's `mLastObjectBuilt` moved with nothing bought in all eight loads, near tick 1094 (1069 to 1139) and
  again 270 to 340 ticks on (1408, 1434), twelve ticks after a load of tick 1082, so an event of the park
  (read since, Q341: the arrival vehicle made at its first summons, above). Row 76 (`0x0059dfd0`, `SideshowsResearchMore`, Score 75, tag row 78 with the
  bar's button of type 19) is read and waits for the research: `FUN_004c9960( 8 )`, `0x00503b30` and
  `FUN_00503980` on `FUN_00519550( 2 )` are not read. Rows 79 and 81 are not read (row 78 is built, Q343). In no game run: line 2, the line again past its cooldown, a golden-ticket shop or sideshow, a
  sideshow built after the line (the test holds each), the bar's button with the buy screen open, a park made
  with no file, Full Simulation. Not heard in either game.
  From Q339 (`advisor-park.md`, "A poor selection of sideshows"): the four other rows that read the newest
  build are not built or read (77 to 79 and 81), and row 76 is built since, Q350 (`0x0059dfd0`, not a reader of it by Q334's
  list; `SideshowsResearchMore` by its key's place, `+0x288`, Score 75: `disassemble_at` first). Row 77's keys
  are next in the schema (`SideshowsBuildSideshows`, Score -70: probably row 62's or 73's shape of the
  sideshows; name them with `advisorschema.py` from `+0x28c` on). Whether the original's `mLastObjectBuilt`
  moved in Q339's runs was not polled. In no game run: the line under the shipped key (it cannot score in Lost
  Kingdom), line 2, the line again past its cooldown, a golden-ticket sideshow, the Arcade standing or on offer,
  a thing built after the line, the bar's button with the buy screen open, a park made with no file, Full
  Simulation. Not heard in either game.
  From Q338 (`advisor-park.md`, "Too few shops for the park's size"): the five other rows that read the newest
  build are not built or read (75, 77 to 79 and 81; row 75's score is `0x0059df30`: `disassemble_at` first, and
  name its keys with `advisorschema.py` from `+0x27c` on, `SideshowsPoorSelection`). **Row 74 counts no ride**:
  whether to mend it (the rides counted, as its key names them) is Alexah's call, not asked; as built it is the
  original's and never speaks. In no game run: line 2, the line again past its cooldown, day 29 in the original,
  a ratio of exactly one, a golden-ticket shop standing, a park made with no file, Full Simulation. Not heard in
  either game.
  From Q337 (`advisor-park.md`, "A park with rides and no shop"): **the original's `mLastObjectBuilt` moved
  twice more with nothing bought**, to tick 1158's on tick 1158 in `q337/shop30.TPWS` and to tick 1170's on
  tick 1170 in `q337/noride30.TPWS` (`q337/orig/c-load.log`, `d-load.log`); still not read (below). The six
  other rows that read the newest build are not built or read (74, 75, 77 to 79 and 81; row 72, `0x0059dd40`,
  not a reader of it, is undefined bytes whose first test is a count asked with 8). In no game run: line 2,
  the line again past its cooldown, a golden-ticket shop or ride standing, a shop built after the line (the
  test holds each), the bar's button with the buy screen open, a park made with no file, Full Simulation. Not
  heard in either game.
  From Q336 (`advisor-park.md`, "A poor selection of shops"): **the original's `mLastObjectBuilt` moved again
  with nothing bought**, to tick 1085's on tick 1085 in `q336/shops1.TPWS` (`q336/orig/e-load.log`), and not
  in `offer4` or `two30` through tick 1135; still not read (below). The seven other rows that read the newest
  build are not built or read (73 to 75, 77 to 79 and 81; by their keys' places `ShopsResearchMore` and
  `ShopsBuildShops` are near: name each with `advisorschema.py` after `disassemble_at`). In no game run: line
  2, the line again past its cooldown, a golden-ticket shop (none in Lost Kingdom; the test holds both of its
  rules), a thing built after the line, the bar's button with the buy screen open, a park made with no file,
  Full Simulation. Not heard in either game.
  From Q335 (`advisor-park.md`, "A poor selection of rides"): **the original's `mLastObjectBuilt` moved to
  tick 1400's on tick 1400 with nothing bought**, in `q335/poor30.TPWS` left running (`q335/orig/b-load.log`,
  before its load); what was made is not read (the constructor's callers are the vehicle summons and the "End"
  feature: watch `+0x21c30` with `watch.sh`), and here nothing moves it, so past it this game's row 60 would
  speak where the original's waits thirty days more. In no game run: lines 2 and 3, the line again past its
  cooldown, a golden-ticket ride standing or on offer, a thing built after the line, the bar's button with the
  buy screen open (the tests hold each but the last), a park made with no file, Full Simulation. Not heard in
  either game.
  From Q334 (`advisor-park.md`, "The newest build"): seven advisor rows that read the days since
  `mLastObjectBuilt` are not built or read (rows 73 to 75, 77 to 79 and 81; score functions
  `0x0059dde0` to `0x0059e2e0`, all undefined bytes: disassemble first; `0x004c9070`, row 60's
  `0x0059d990` and row 71's `0x0059dca0` are read); a
  ferry or seaplane made at its first summons moves the stamp since Q341 (its own thing is not made: above,
  and Q26c). In no game run: a move's put-down, a purchase or a sale
  made in the original by hand, a park made with no file, Full Simulation.
  From Q333 (`advisor-park.md`, "More rides than the mechanics can keep"): the analyser's four other stamps are
  read (Q397: `mParkLastOpened` kept, the other three never move), `mLifetimeVisitors` and `mMostPaidForTicket` are
  kept (Q389); `mLargestBusLoad` and `mLongestStay` are not kept and go out as the file's (what reads and writes each
  is not read: search the image for each one's displacement, as Q334 did). Whether anything but a hire runs the staff constructor (a
  candidate made for the hire screen) is not read. In no game run: lines 2 and 3, the line said again past its
  cooldown, seven rides and one mechanic, a mechanic hired from the screen after the line (the tests hold
  each), a hire made in the original by hand, a park made with no file, Full Simulation. This game said the
  line of `days29` on tick 1094 and the original on 1096. Not heard in either game.
  From Q332 (`advisor-park.md`, "Broken rides and no mechanic"): row 124 is not built (`0x0059f220`, not read;
  responses 300 and 301, sample 334, "the buzz about your park has started to die down": the original said it
  on tick 1317 of the scene's file, twice; both responses name model slot 1 and prop -1, the theme's own
  advisor and costume, which are not built either). Response 18's gesture row, 21, is counted, not played. In no game run:
  lines 2 and 3, the line said again past its cooldown, a mechanic kept or hired after the line, a broken
  sideshow beside the rides (the tests hold each), Full Simulation. Not heard in either game.
  From Q331 (`advisor-park.md`, "Worn rides and no mechanic"): the other staff rows, 8, 9 and 11 to 17, are not built
  (tags 10, 11 and 13 to 19, each with a hire button; row 10 is built, Q411, row 13, Q412, and row 12, Q413). In no game run: lines 2 and 3, the line said again past its cooldown, a broken ride counted, a
  ride worn under 25 by running, button types `0xd` and `0xf` to `0x11`, the button with the hire screen open
  (the tests hold each), a mechanic hired after the line, Full Simulation. Not heard in either game.
  From Q330 (`hud.md`, "The six round buttons"): what serves Restart Park's request (`FUN_005508b0( 1 )`) is
  not read; it keeps the category in both games, measured. In no game run: Full Simulation, the Information
  and Finance categories (their seed and their openers' notes are read, not put to the original; Information
  is seeded 4 here, the declared deviation), a key that opens a screen. Exit To Lobby in the original writes
  `autosave.TPWS`; this game writes none.
  From Q329 (`advisor-park.md`, "The hire screen's own line"): in no game run: a candidate chosen while he speaks (what the
  original's handler does to the screen at a row's click was not read), the opener asked for the tab shown
  with the screen open, a park left with the screen open, Full Simulation. Not heard in either game.
  From Q328 (`advisor-park.md`, "The buy screen's own line"): the buy screen's handler's four other posts are
  not built (`0xc9` or `0xce` at a purchase, `0x159` and `0x15a` at the land rows; read in the decompiler
  alone), nor the other 37 screens' lines (`0xbe` to `0xd5`, `0x11d` to `0x135`; find each opener's post and
  its handler's withdrawal). In no game run: a purchase cutting the line, the message bar's buy button
  posting it, a park left with the screen open, Full Simulation. He stands at the lower right with a park
  screen open in both games; where he stands with none was not compared. Not heard in either game.
  From Q326 (`ride-operation.md`, "A shut staff room and a tired member"): in no game run: a member of
  another kind, a second staff room nearer and open, a room shut while a member rests in it, Full Simulation.
  The original's frame does not show the room (its saved view); `VAR_STAFFIN` and the resting-staff list stay
  unbuilt (`REST_AREA_OCCUPANCY`).
  From Q325 (`advisor-park.md`, "A shut staff room"): a staff room's own window is not built (the original's,
  `FUN_004b4fc0`, has no door); response 513's lip file 0 is not read. Thirteen other responses name
  `z_error.mp2` (0, 429, 517 to 519, 550, 572, 573, 588 to 591, 599): their rows are not read. In no game run:
  a share between nought and the whole, the line again past its cooldown (the test holds each), a staff room
  bought, Full Simulation. The original's sound was not measured; neither game was heard by ear.
  From Q324 (`advisor-park.md`, "A queue not joined to a path"): **the object the line passes over is not the
  original's**: here the thing the queue tool is armed for, there the builder's object `DAT_008186ac`, set from
  the interface's selected thing too (`FUN_005276d0`, nineteen callers not read) and perhaps kept after the
  tool is put away; buy a ride with the mouse in the original, put the tool away and watch the word and row
  113. What reads the balance object's `+4` is not found. In no game run: line 2, the line said again past its
  cooldown (the test holds each), a queue cut off from an open ride, two rides unjoined, the tag's button
  clicked in the original, the line posted while he speaks, Full Simulation. Not heard in either game.
  From Q323 (`advisor-park.md`, "An open park's shut rides"): in no game run: row 259's second and third lines, the line said again past its cooldown (the test holds
  each), a ride shut from its own window, a share of exactly half, four rides or more, Full Simulation's
  rates. Not heard in either game.
  From Q322 (`advisor-park.md`, "A park with no ride in it"): the row of a tag's item is not selected on the buy screen
  (`TAG_BAR_BUY_ITEM_ROW`, counted; no listed tag names an item); what writes an item's control record's
  standing count (`+0x18`) is not read. In no game run: lines 2 and 3, the line said again past its cooldown,
  a shut park with no ride, a park under its 26th day, Full Simulation's empty park, button types `0xa` to
  `0xc`, the button with the buy screen open (the tests hold each), a ride bought after the line. Not heard in
  either game.
  From Q321 (`advisor-park.md`, "An open park with nothing to ride"): in no game run: row 2's second and
  third lines, the line said again past its cooldown, a tag 4 and a tag 3 taking each other's place, the park
  shut while he speaks (whether anything withdraws message 2 was not looked for), Full Simulation's empty park
  opened, a sale through the player's own window. Not heard in either game.
  From Q320 (`advisor-park.md`, "Shut shops, sideshows and toilets"): The toilets' count's place in the analyser (`+0x1f0e0`) is
  not kept. In no game run: a shop, sideshow or toilet shut by itself in an open park (no window shuts a shop
  or a toilet), a share between nought and the whole, each row's second and third lines, a row said again past
  its cooldown, four waiting (row 1 before the toilets'). What the first run did with row 1's slot is not
  known (the `dropped` log line came after it). Not heard in either game.
  From Q319 (`advisor-park.md`, "A shut park people wait outside"): The count's place in the analyser (`+0x1f0d4`) is not
  kept. The entry-price screen's own line (`0x12f`) is not posted. In no game run: lines 3 and 4, the refusal
  inside 480 ticks, a bus at a shut park, the park opened while he speaks (whether anything withdraws message 1
  was not looked for), a tag 3 taking a tag 4's place, Full Simulation's empty park. Not heard in either game.
  From Q318 (`advisor-park.md`, "The advisor's turn in the sweep"): in no game run: a ride's own post said on
  its tick (the wear's `0x42`, the break's `0x44` and `0x45`; a test holds the wear's), a post from the staff
  pool's or the arrivals' turn (none is built). Whether `0x45`'s 31 sweeps after the line before it (the
  original's 43) moved was not measured again. Not heard in either game.
  From Q317 (`advisor-park.md`, "The tips' line"): in no run of the original: the Tutorial switch off at row
  282's turn, a park past day 79, a park made with no file. This game's wait was 11,665 ms in each run and the
  original's 11,665 and 12,498: what the second second is was not read. The line's end was not seen. Not heard
  in either game.
  From Q316 (`advisor-park.md`, "The poll, and the control panel's line"): 146 of the 155 polled rows are not
  built (`PARK_ADVISOR_POLLED_ROW_NOT_BUILT`; row 281, `GoldTicketExplanation.Score` on a count of one, is the
  nearest); row 0's poster (`Welcome`, posted only in the running table)
  is not read; `meta_full.json`'s row 0 is wrong in two dwords and the file is not corrected; the line's length
  differs a run in both games and was not compared; a file's cursor below nought is refused here, where the
  original would read before its table. In no game run: a line polled inside another's wait (the test holds
  it). `PLAYER-GAPS.md` gap 4's text is older than the breakdown lines. Not heard in either game.
  From Q315 (`advisor-park.md`, "The last action in a park file"): a park screen's own line is not posted, so it
  starts no action. In no game run: a withdrawal inside a file's wait (the tests
  hold it), a file with a wait of nought, a park made fresh saved before any line, Alexah's own parks. The two
  calls in `Level` are held by the game runs alone. Not heard in either game.
  From Q314 (`advisor-park.md`, "The slots in a park file"): the kept message, `mRequestedLastTutorial`,
  `mTimeSig`, `mSoundId` and `mNextAvailableQueuePos` are carried and what reads them is not described; a slot
  let go keeps the file's older bytes, not the said message's; a taken slot of an unbuilt message is counted and
  never said (`PARK_ADVISOR_SAVED_SLOT_NOT_BUILT`; no stored file holds one). In no game run: the door
  shut and opened after a load (the test holds `duplicate`), eight slots full, Alexah's own parks. The two calls
  in `Level` are held by the game runs alone. Not heard in either game.
  From Q313 (`advisor-park.md`, "The records in a park file"): in no game run: a rotation or a cooldown carried
  across a load, a file with cuts, Alexah's own parks. The two calls in `Level` are held by the game runs alone.
  From Q312 (`advisor-park.md`, "The line of an opened tag message"): the kept message (`+0x16d0`, raised again by `FUN_0059ab50` from
  `FUN_00488ba0`) is not built; the other 37 screens' lines
  (`0xbe` to `0xd5`, `0x11d` to `0x135`) are not posted; whether the original's advisor cries out when a
  withdrawal cuts him off was not heard (ours does, `Advisor.StopSpeaking`). In no game run: a second tag
  opened over the first, the erase button with him speaking (the tests hold each), the options screen's own
  switch (the console's set it). Not listened to by a person in either game.
  From Q311 (`hud.md`, "The message bar"): the bar's buttons of every type but 3 are not built
  (`TAG_BAR_BUTTON`; `FUN_0048e0f0`'s nineteen other arms, of which only the camera's is read), and types `0x12`
  and `0x14` are not dressed (the files behind the nodes `b_loan` and `b_pkinfo` are not looked up); the bar's
  children are shown all at
  once; the buttons' depths are not kept; the tag on show swells on real milliseconds, a round in 0.7 s where
  the original's frames make it 0.9. At 1280 x 720 the bar's text is in a smaller face than the original's at
  1024 x 768 and breaks its lines later (which face a font slot takes at a window's size is not this item's).
  In no game run: a tag's right click with the bar out, a tag taken off by a later line with the bar out, the
  camcorder button or a park screen with the bar out, two tags, a tag that declines (the tests hold each). The
  arm's slide, its sounds and the camcorder panel's travel are built (Q427, which was Q230's ask).
  From Q310 (`saves.md`, "OpenTPW's writer, the aerial's tags"): a tag's text is written for kinds 0 and 2
  alone (`TAG_TEXT_KIND`; `FUN_00509e10` has 37), its name the item's `Info.Name`, held on items 1100 and 1140
  (what fills the item record's `+0x410` is not read); what item 2984 is on a guest's tag is not decoded; a
  loaded tag's life runs from the state's making, not from the read; the bytes the original leaves unwritten
  are nought. The call from the said line with the subject's item and name, and the strings' load, are held by
  the game runs alone. In no game run: two tags in a file, a hidden tag written, a tag of a row not listed put
  back and taken off by a later line. The new tag's sound at a load was not heard.
  From Q309 (`advisor-park.md`, "The tag a line leaves on the aerial"): the particle five seconds before a
  tag's end is not built (`TAG_ENDING_PARTICLE`,
  effect `0x5b`: the interface draws no particle in a park). Only the six rows the built topics name are listed
  (`TAG_ROW_NOT_LISTED`; the table is the executable's, 382 rows). The red top and the later tags are put down at once, not played down (the streams at
  `0x007502f0` and `0x00752e48`). The two sounds play at their variations' own volume, not measured against the
  original's mix. Events `0x19` and the postcard's are sent by nothing here. Whether a tag's timers run under
  the original's menu pause is not measured. In no game run: a second tag, a tag taking another's place, the
  door's tags, either right click (the tests hold each). Not heard in either game.
  From Q308 (`advisor-park.md`, "What he wears for a line"): the theme's own advisor model (slot 1, `Advisor_JUN.md2`, 137 rows) and its costume (prop -1,
  `ThemeAdvisorCostumes`) are built (Q346). A clip word's `0x10000` and its particles are a gesture row's, not built. The hat is paler than the
  original's (his light is set against his gloves, not decoded). In no game run: the ticket hat (the door's
  lines; the tests hold it), a second line after a hat, any other prop. The call from the speaker to the model
  is held by the game run alone.
  From Q307 (`advisor-park.md`, "A ride broken down and no mechanic to come"): the event's other two hearers
  are not read or built (the kind-17 relay's `FUN_00481530`, for types 7 and 8), nor a condemned ride's event
  (`RIDE_BROKEN_DOWN_EVENT`, `RIDE_CONDEMNED_EVENT`, counted). The mechanics are looked at in the staff
  list's order, not the chain's from `mFirstMechanic`. The original said `0x45` 43 ticks after the line before
  it began, this game 31 sweeps: not read. Row 67 (message `0x43`) scores nought and has no constant poster:
  not looked at further. In no game run: lines 199, 200, 202 and 203, a mechanic resting or striking, a
  mechanic already naming the ride that breaks, two mechanics, a condemned ride, a hired mechanic after
  "you don't have any mechanics". Not heard in either game.
  From Q306 (`advisor-park.md`, "A mechanic on his way"): response 206's own gesture row is not played
  (`ADVISOR_RESPONSE_GESTURE_ROW`, counted; no other built response names one). In no game run: lines 205 and
  206, two rides broken at once, a called ride, a post refused inside the 480 sweeps, a break with the advisor
  busy, a bought ride. Rows 67 to 69's posters are still not read. Not heard in either game.
  From Q305 (`advisor-park.md`, "A ride about to break down"): the record's item word is not kept. The same shape and not built: the two
  no-mechanic rows (105, 107), whose posters are not read. In no game run: lines 195 and 196, two worn rides at
  once, a post refused inside the 480 sweeps, a worn ride with the advisor busy (the message is then spent
  unsaid, as the door's), a bought ride. The original's line was not heard (its sound off-screen is a null
  device), and ours not listened to by a person.
  From Q304 (`ride-operation.md`, "Opening a ride", "The running test"): how fast a queue boards is not
  compared (a rider about every five seconds here with the polls' pauses in them, one every three in the
  original, `q252/orig/a-load.log`), nor how long each rides, nor where on the ride each of the five stands;
  the original's riders were in its poll, not its picture (its camera stood by the gate). In no game run: a
  bought Belly Bounce, another item with `Info.RunsContinuously` (which items carry it was not counted), a
  ride broken with five on. A full Bounce now wears to a break in about 50 steps of 64 sweeps.
  From Q303 (`ride-operation.md`, "A ride's wear"):
  what a ride's script does on `VAR_WORN` was not looked at; the wear's speed is the thing's
  `mOperatingSpeed`, not a script's word (Q155); `FUN_004df640`'s callers (the flag's arm: a ride window's
  reliability) were not read; the sum's stores are a float's where the listing stores one, and were not held
  bit for bit against the original's (its poller reads two places). In no game run: Full Simulation's rates, a
  second tier, a ride with no life left (condemned), a bought ride, a worn ride from a file, a repair's count
  between nought and the whole. The call in `ParkPeople.TakeTheRidesTurns` is held by a test.
  From Q302 (`audio.md`, "The plain class: any variation, and a silence after a sample"; `park.md`, "The land's
  own sounds"): how long the silence after a gull is, and which sample follows which, are not measured in the
  original (its sound off-screen is not paced); the silence here is timed on the frame clock from the frame a
  sample is found ended, where the original's is bytes in the channel's own stream; the sample draw's rule
  against the sample before (`FUN_006bc680` asked with 1, a variation of more than two samples) is built for no
  held voice; the classes of flags `0x10` and `0x100` (`0x006be680`, `0x006be330`) are not decoded; the
  original's channels at (450, 160, 0) with a far end of 100 and a new volume and frequency every few seconds
  are not named; the sea of the other three themes is started by the same code and was in no game run; the
  call in `Level` is held by the game run alone. Not listened to by a person.
  From Q301 (`weather.md`, "Two rain voices, and how loud each is"): what a level set on the bed's stale handle
  answers after a stop is not read (`FUN_006b5b80`), and here the next shower's first step starts the bed at
  once; the two rain channels read a far end of 95, not their variations' 100: a flat channel's far end is not
  looked into; the flat channel at 4 of 127 from a park's start is not the rain's and is still not named; the
  wiring from `ParkWeather` to the rain's calls is held by the game run and no test (a test cannot stand a
  `ParkWeather`: its drops need the graphics device); snow, which stops the weather's own voice and keeps the
  bed, was in no game run (no Lost Kingdom snow); the fix's two voices were not told apart in the mix, only
  their sum; that the original's two start on one mixer buffer is the listing's order and one poll line, not a
  measurement of its mix; how closely one rain sample follows another in the
  original is not measured (its sound off-screen is not paced). Not listened to by a person.
  From Q300 (`audio.md`, "A channel's volume"): a thunder is still held back by the per-effect wait (one bolt of
  fourteen had none; the original sounded two 12 ticks apart: Q43); the lobby's own thunder
  is not through `ChannelGain`; one of the original's thirty thunders opened at 24 of 127 for a second, by what
  was not read; a real storm was in no run of either game (the original's bolts were a held flag's, these the
  console's), and the bolt is not made out in this game's frame. Not listened to by a person.
  From Q299 (`audio.md`, "A channel's volume"): the engine's sample was not found in the mix (its pitch moves each
  tick; the mix was measured whole); a wrong volume at the voice's start is seen by no test (the apply follows in
  the same call); the original's channel at (435, 255), 64 of 127 at a far end of 100, is not named. In no game
  run: a go's end here, another bumper ride, a loaded park's boats. Not listened to by a person.
  From Q298 (`audio.md`, "A one-shot's pitch"): in no game run of either game: a single scream
  (the tests alone hold its -8 to 7), a yawn's rate, a sale's put-off of a rider. The original's put-off channels
  are turned down to 4 of 127 as they play, by what was not read. The mix check missed its own bar (0.641 and 0.679
  at two of six logged pitches; why those two score low was not looked at). Not listened to by a person.
  From Q297 (`audio.md`, "A scream's child and its pitch"): a scream child's pitch under
  `SCREAMLEVEL`'s apply (`0x006bbbe0`) is not built: no shipped scream names its parameter. In no game run:
  bands 2 to 4 (effects 72 to 74), a bought ride. Not listened to by a person. The one-shot's slot `+0x40`
  (`0x006c3d80`) was read for the wait alone.
  From Q269 (`audio.md`, "A held voice's next sample"): How
  closely one sample follows another is not measured: the original's null device off-screen is not paced. The
  original's crowd channel read a far end of 95 in one run and 100 in the next, on a flat channel: not read.
  A level over 13 was in no game run of either game. Each beat's set draws the pitch again on the playing
  sample, as the listing and the original's channel say; what that sounds like was not listened to.
  From Q268 (`audio.md`, "A channel's volume"):
  Flat channels at 1, 2, 3 and 4 of 127 in the original are not named. The staff's voices were not measured in
  the mix (their volumes are `voices`' and the log's). A crowd over 15 was in no game run of either game.
  From Q267 (`audio.md`, "A channel's volume"): the interface's flat effects are not through `ChannelGain`. No scream was made
  at a level other than 20 in the original. In no game run: a single scream, a guest put off (10 of 127), a yawn; the options' two volume sliders do not move `ChannelGain`'s 75 and 60. The
  request mask's bit `0x80000` arm of `FUN_006c5690` (a volume scaled again by `params + 0x9c` over a hundred)
  is not read.
  From Q266 (`ride-operation.md`, "Where a scream is sounded"): a fixed item's scream is counted and sounded
  where its model stands (`SOUND_PLACE_NO_CELL_RECTANGLE`; the original's record holds cell (0,0) alone,
  so (5, 5)); the wiring from the script to the place is held by the game run alone; a single scream, a
  bought ride's scream and a turned ride's were in no game run; the height is the anchor cell's ground,
  where the original's is its model's base height, nought on the stock park's flat land and not read on a
  hill; a script's own started sound with no node (`FUN_00556b90`'s other arm, the same middle) was not
  looked at here; the scream was not measured in the mix, its place and gain read from `voices` alone.
  From Q265 (`audio.md`, "A voice's range, and the rectangle it follows the listener in"): Not identified: the original's channels at the listener's own place with a
  mapping of 2.0, 95.0, 0.80 and a volume of 10 of 127. Held by the tests alone, in no game run: thunder, a
  single scream, a guest put off, a yawn; a bumper ride's engine's range is held by a game run and no test (no test
  stands a track ride's script), and its voice is at height nought where its boat floats 30 up. A script's own started sounds (`ADDOBJ`) were not looked at.
  From Q264 (`audio.md`, "The mixer's distance law"; `park.md`, "The land's own sounds"): the level's own two type 1
  records are counted, not played (`SCAPE_OMP_SOUND_TYPE_1`): the level's own `cat_ambient` is not loaded, and
  why the level's ambient 181 had no voice in the original is not decoded. A channel's volume read as a whole number of 127 (the fall 75, the music 60), so a
  sound effect at a hundred is 1.25 of the music. A voice out of range plays on unheard here, where the original gives its channel up:
  where its sample stands when it is heard again is not decoded. The pan is a plain left and right, not
  QSound's. The orbit camera's point is left at height nought where the ground is high, so the listener's
  height over a hill was not held against the original's. In no game run: the river in the mix, another
  theme, a park loaded from the menu. `docs/exe/addresses.md` not regenerated.
  From Q262: a park entered from a file starts the spray afresh
  and does not read the file's emitter. In no game run: another theme, a Full Simulation park with no file.
  **Do not take the jet's move as a piece for its picture:** the original's own Jungle Spray jet reads the same
  place and aim on every poll of its four seconds (`q257w/orig/a-load.log`), so an emitter moved to its node
  each tick shows nothing there; find a Lost Kingdom particle node a clip really moves before building it.
  From Q261 (`park.md`, "The sprite pass, and the Hot Pot's own smoke"): the original's cloud at the pot's
  doorway is wider and blacker than this one, the lens (Q260) and the blend not told apart: read which state
  word set 10's texture carries in the running original. Nothing here sorts see-through surfaces and sprites by
  distance, and the people's sprites are still drawn in the see-through pass, in the order things were made
  (not looked at: a guest standing in front of a bought thing's see-through half). The far cloud, at the fire
  arch, was a wisp in the frame.
  From Q259 (`park.md`, "A broken boat's smoke"): a boat's smoke is started and moved a frame at a time, not on
  the track tick; a boat with no emitter node is counted (`BUMPER_CAR_NO_SMOKE_NODE`). The wiring in
  `ParkBumperBoats.Sync` is held by the game run alone (no test stands a boat). In no game run: a boat taken off
  while it smokes, a second break after a fix, a full particle system.
  From Q258 (`park.md`, "A park's particles, started and drawn"): nothing moves or aims a running emitter to
  its node each tick, so the Jungle Spray's jet stays where its node rests (`PARK_PARTICLE_NODE_ON_A_CLIP`);
  counted and started nowhere: `PARK_PARTICLE_ITEM_EFFECT`, `PARK_PARTICLE_NO_PLACE`,
  `PARK_PARTICLE_NO_DIRECTION`, `PARK_PARTICLE_NOT_STARTED`; a loaded emitter begins again
  (`LOADED_EMITTER_LIFE_PART_RUN`); an effect added without its alpha (`WORLD_PARTICLE_ADDED_WITHOUT_ALPHA`).
  Not started, each counted: the riders' and leavers' puffs
  (`BUMPER_RIDER_PARTICLE`, `LEAVER_BOARDING_PARTICLE`), a sold thing's (`DESTROY_PARTICLE_EFFECT`). The writer
  still writes an emitter as it starts, not as the running one stands. A sprite's width on the screen was not
  settled by the original's picture (the listing's rule is built). An effect with no picture is 0.75 wide for
  its height in the pass (`DAT_008bcbcc`), and `ScreenParticles` draws its own as 1. In no game run: the jet,
  an `EVENT`'s puff, a `KILLOBJ` (the tests alone); the toilet's stink and flies were not held against the
  original's picture.
  From Q257z (`saves.md`, "OpenTPW's writer, the mechanic's job"; `ride-operation.md`, "The mechanic's
  repair"): an upgrade job is passed over and its completion left (`MECHANIC_UPGRADE_JOB`,
  `RIDE_UPGRADE_COMPLETION`: nothing here puts a ride in state 2), and a mechanic put down still goes idle where the original's searches at
  once (`MECHANIC_PUT_DOWN_JOB_SEARCH`); his repairing picture was not held against
  the original's frames; a called ride, two broken rides, a second mechanic, a ride with no route and a ride
  sold under him were in no game run.
  From Q257y (`saves.md`, "OpenTPW's writer, a broken ride's state"): SetState's post to the event bus is
  counted (`RIDE_BROKEN_DOWN_EVENT`, `RIDE_CONDEMNED_EVENT`); a ride condemned, one broken with its door shut
  and any ride but the Hot Pot were in no game run; whether nobody is seated inside a second of a break here
  was not resolved (a poll each half second; the original's is 0.3 s).
  From Q257x (`saves.md`, "OpenTPW's writer, a smoking car"): a smoking car's emitter is put
  where its boat is drawn, not where its last tick left it, and dealt after the scripts'; with no boat drawn
  it is counted and written as none (`SAVE_PARK_CAR_SMOKE`).
  **An `EVENT`'s puff needs no writing:** the original's own file of a bought Loudspeaker holds its two puffs
  spent (lives of -38 and -62, a particle each), and a load frees an emitter with a life under nought on its
  first tick (`Particles_Tick`, `0x00520130`); only a puff of an effect with a life still running would show,
  and no Lost Kingdom script's `EVENT` was found with one (not swept: sweep the corpus before building it).
  From Q257w (`saves.md`, "OpenTPW's writer, a particle with a direction"): a type 2's node that rides a clip is
  aimed as it rests; one whose matrix is never stored is counted (`SAVE_PARK_EMITTER_DIRECTED`, no Lost Kingdom
  node); the file's own jet was not seen in the original's picture (its camera is elsewhere for the load's first
  seconds); a Spray turned and the Laughing Hyenas were in no game run.
  From Q257v (`saves.md`, "OpenTPW's writer, an emitter started"): counted and written with no emitter: an
  item's own effect (`SAVE_PARK_EMITTER_ITEM_EFFECT`), a record with no node (`SAVE_PARK_EMITTER_NO_PLACE`) and
  an effect that links an effector (`SAVE_PARK_EMITTER_NOT_STARTED`). An emitter is written as it starts, its
  life begun again; an `EVENT`'s puff alive at the save is not written; a burst's draws are not made. In no
  game run: an emitter killed (a record gone, a thing sold).
  The Huge Hollow Rock's two particle nodes stand a cell out in one axis against the original's files.
  From Q257u (`saves.md`, "OpenTPW's writer, a walk's head"): the Inca Totem, a Mayhem sold or moved with riders
  carried and a head in a costume were in no game run; which of a head's 56 pictures each game shows is not
  compared; the rider's body is not drawn where the original leaves it standing.
  From Q257t (`saves.md`, "OpenTPW's reader, a car's model record"): a loaded car's wake's record is not read (no
  wake is drawn, `BUMPER_CAR_WAKE`); a handle naming no record of the boat's item is counted and left
  (`SAVED_TRACK_CAR_MODEL_NOT_ITS_OWN`), where the original takes whatever record lies there; with no boat drawn
  (the tests) a loaded car's clip is still started as the file is written.
  From Q257r (`saves.md`, "OpenTPW's writer, a track ride's cars"): a car's held sound is written as none.
  A loaded rider's sprite is made at the load, not as the ride lets them go, and a live rider's is never
  destroyed (the original's admission, `0x00502147`). The stepping agrees with the original's for three ticks
  and is not compared past a car's first draw: the cars draw their own generator, the original's the world's
  (`ParkBumperCars`, `_random`). The karts' and the water ride's BumperTypes have no record here, and a
  tracked ride (its `TrackType`) is not written at all (`SAVE_PARK_OBJECT_BOUGHT`, `SAVE_PARK_OBJECT_SOLD`,
  `SAVE_PARK_TRACK_RIDE_AS_THE_FILE`). Which of the three writers puts node word `0x40` on a car's first node
  was not traced, nor a car's `+0x14` to `+0x1c`. In no game run: a ride sold with boats out, a peep on the
  leaving or boarding list at the save, a turned track ride.
  From Q257n (`saves.md`, "OpenTPW's writer, a thing with emitter files"): a script's `EVENT` puff alive at
  the save is not written (the original's file of a bought Loudspeaker holds two of template 36). A Loudspeaker
  sold was in no game run.
  From Q257c: the offset's
  arms at 180 and 270 degrees stand on the listing alone: no file holds such an object.
  From Q254: a queue cell on a tile outside the eight pieces, and every changed queue cell of a park written
  with its things as the file's, is still left naming the file's model and counted
  (`SAVE_PARK_QUEUE_CELL_MODEL`). The model slots are dealt as the file is written (objects, then cells in the
  map's order), where the original's are in the order things were made: keep a running table if that order is
  ever read. And not decoded: the original, loading a file whose queue cell names a record of another tile
  (or a bare cell naming one), reads no handle on that cell afterwards and still draws the piece
  (`saves.md`, "OpenTPW's writer, a queue cell's model", the control).
  From Q257l (`saves.md`, "OpenTPW's writer, a person's sprite inside a loop"): a saved sprite's
  local 0 (`+0x84`) is not read at a load; it is memory the constructor `FUN_004758f0` never writes, and only
  the state script's hold loop reads it, with local 18 above nought, which no file has (`q257u/locals.py`).
  And a made person's sprite inside a loop (a hired entertainer performing) was in no game run: the tests alone.
  From Q257m (`saves.md`, "OpenTPW's writer, a person's sprite's drawing flags"): what bit `0x200` of a
  sprite's flags draws is not decoded (`0x005422fb`), and the drawing here reads none of the word.
  From Q257d: a head on a coaster's or a tour's car, which the engine hangs with no head table, is hung by
  nothing here, is not counted, and a written park holds only the file's: count it where `COAST` and `TOUR`
  reach `FUN_0044b410`, then write it as a table's head is (a walk's is done, Q257u). And a new head's slot is the
  lowest the FILE leaves empty, not the lowest empty as it was hung: keep the slots as heads are hung if that
  order is ever read.
  From Q257e (`saves.md`, "OpenTPW's writer, the region effects"; `ride-operation.md`, "The region effects"):
  check a written park with `q257f/fx.py`, which wants 0 cells wrong. Fireworks' effect is not taken off on the turn their script's
  variable 0 reads 1 (`FIREWORKS_SPENT_REGION_EFFECT`, counted at their purchase). The chooser divides by the
  FILE's attraction word, not the running grid's (`ParkRideChooser.ScoreOf`). And not read in the listing: that
  a load over a running park leaves a cell with no effects record holding what memory held.
  From Q257g (`saves.md`, "OpenTPW's writer, a channel's `0x8`"): as the mark comes off, the engine clears node
  flag `0x800` on the model's nodes holding `0x100`, once (`0x00473d3c`): counted
  (`MODEL_NODES_AFTER_ROLE_NOUGHT_ENDED`), the two flags not read. And an item whose only clips are role 0's is
  held on role 0 here where the engine parks it idle, so a bought one is written held with the mark.
  From Q257h (`saves.md`, "OpenTPW's writer, a walk slot's facing"): a rider carried on a node a clip moves
  faces as the node does at rest (`WALK_FACING_REST_POSE`, counted). The shake's turn of a carried rider is not built, with its
  instruction (`WALKST_FLOAT`, Space Zone's Zero G alone: after Lost Kingdom).
  From Q257i (`saves.md`, "OpenTPW's writer, a script's started effects"): a sound's record started here is
  written with a handle of nought. The engine frees a record whose spawn answers nought (`FUN_00557970`);
  every `ADDOBJ` of a known type keeps one here: decode when `Particles_Spawn` and `Sound_PlayEffect` answer
  nought (a category without the id, `park.md`, "ADDOBJ"), then keep no record there. `SETOBJPARAM`'s handle
  stored back is not kept. And not seen in the original: what its scripts do with a record whose handle is nought.
  From Q257s (`park.md`, "The scene's height under a point"): the scene's height is built for a model whose
  header float is not nought (the Hot Pot). Counted and left at the last height: a model with meshes to stand on
  (`SCENE_HEIGHT_SURFACE_MESHES`: the karts' bridges, the water ride's raised pieces) and one with neither
  (`SCENE_HEIGHT_OWN_MESH`: the mesh at the instance's `+0x2c`, whose setter is not read). Not read: a track
  piece's cell (`FUN_0053bf30`), header flag `0x80`'s setter, and a loaded car's own `+0xa0` as the height a
  corner off its ride keeps. A boat's rocking from its corners is still `BUMPER_CAR_ROCK`.
- [ ] **Q241j. The writer: the managers, then Save Game saves.** From Q241e. The economy thing's loans and rings,
  the staff HQ's strikes, the calendar's and the weather's fields; then `ParkSaveScreen.Save` writes
  (`SAVE_GAME_WRITER` gone), an overwrite replaces the file, and the Load Park list shows it. Confirm: a park
  played, saved from the menu under a typed name, loaded by OpenTPW and by the original under Proton; the tick,
  the cash and the guest count predicted; a screenshot of each.
  From Q241f: `Level.WritePark` is the call to make. Of the economy thing only `mBalance` is written, so a loaded
  park reads the first file's profit under the new balance (`money`: profit -12013 beside 88112). The original puts
  the pointer back to its default mode before it writes (`FUN_00516c80`, step 1); `WritePark` does not.
  From Q241h: a member of staff in the hand is written idle where they were picked up, and stays in the
  running park's hand; the default mode's install is what puts them down in the original.
- [ ] **Q327. Full Simulation catch-up: the built advisor lines in a Full Simulation park.** Filed at Alexah's
  word, 2026-10-12, and placed here by it: after Q241j, whose economy fields a Full Simulation file needs. One
  session. A Full Simulation park made with no file, saved, and run through each built polled row's scene in
  both games (rows 1, 2, 62, 113, 185, 259 to 263, 282, and row 80, which is Full Simulation's alone and needs
  the fresh park's lab filled first, Q349: `advisor-park.md` has each scene and its turn; the
  cursor is the program's, not a file's, so predict each turn from it). Say first, from each score function,
  whether the mode is asked; run those where it could matter (the regular catalogue, the empty park's age and
  rates), and say in one line why for the rest. Confirm: each line's `poll` and `attempt` lines with a
  screenshot, and the original's record on the same file. It answers the "Full Simulation" left in Q331's notes
  From Q319 to From Q325; take each out of those notes as it is run.
- [ ] **Q248. The writer's other callers.** From Q241 and Q241e (`saves.md`, "The other callers"). `gms.dat`
  written before every park file; `autosave.TPWS` on leaving a park, which entering then finds as the newest
  file; the quicksave and quickload keys (`<theme>.TPWS`); `restart.INTS` written when the folder has none, and
  read by Restart Park. After Q241j. Confirm: a park left and entered again at the tick it was left on.
- [ ] **Q149. The calendar keeps its own game tick, from nought, and makes up the advances the original loses.** Found
  by the 2026-09-26 staleness audit. The original's calendar counter is `mGameTick` (`+0x1da70c`, `weather.md`, "The
  calendar"), which a loaded save sets to its own (755 in `Easymode.TPWI`, as `GameCalendar.Rebase` says);
  `GameCalendar.Counter` starts at nought beside `ParkState.GameTick`, which carries the 755, so the date and the
  weather's days run about 33 days behind the original's. And `GameCalendar.Update` adds at most three advances a
  frame but carries the rest to later frames (`wanted - Counter`), where `0x0054f680` drops them for good, as
  `LongestCatchUp`'s own remark says. Check that the published calendar (`0x007ced58`) adds the counter to
  `mFunnyTimeStart` alone, then drive the calendar from `ParkState.GameTick`, so Q126's cap holds the date too.
  Confirm: the gadget's date on entering Lost Kingdom, predicted from 755 advances; a screenshot.
  Q165c gave the ride score and the object window's Age the original's calendar (`ParkState.CalendarNow`, from the
  save's `mGameTick`); the gadget's date and the weather's days still count from nought.
  From Q177b: the objects' day rings roll on `GameCalendar.DayRolled` too, so their days turn about 18 world ticks
  later than the original's after Lost Kingdom's load (its first changes at ticks 761, 784, 807), and a load never
  rolls on its first tick where the file's `mDayAtLastUpdate` differs from the loaded date.
  From Q198: the month's change comes to the bank and the staff on `GameCalendar.MonthRolled`, so here on 2/1/2000,
  177 s in, where the original's first after the load is at tick 1383, 3.1 (measured). A load reads the month and the
  day but not the year, which keeps the entry's seed, 2000 (`weather.md`), so a park saved in another year gets `0xd`
  on its first sweep and zeroes `mProfitThisYear` (decoded, not measured).
  From Q126: the thing sweep is capped at three a frame now and `ParkState.GameTick` with it, so after a 2 s stall
  `GameCalendar` (which carries its advances over) runs five ahead of `ParkState.CalendarNow`, and the weather's
  tick and the day's, month's and year's work with it; the original drops all of them with the sweep
  (`FUN_004d7b20`, `FUN_00512880`). `LongestCatchUp`'s remark says so and calls it a deviation.
- [ ] **Q31. The shop's object window.** `Level.OpenObjectWindow` opens only a ride's (`UiType 0`). Shops,
  sideshows and the rest stop at `SHOP_WINDOW`, `SIDESHOW_WINDOW` and `FEATURE_WINDOW`; a staff member's click
  opens their window (Q431); a clicked visitor reaches nothing counted. `park-engine.md`, "The
  per-object management screen is nine screens", lists the nine. This item is the shop's; the other seven are Q31b.
  From Q177: the shop, sideshow and toilet windows' figures are decoded (`ride-operation.md`, "The settle-up's
  bookkeeping"): customer satisfaction, profit last month, customers as "C of M", winners last month, the cost of
  goods from the window's pending sliders (applied on close, on stepping to the next or by apply to all), and a
  toilet's users last month. `UIStrings` 37 to 39 are one row off: UITEXT 37 is "Scrap value", 38 "Local happiness",
  39 "Quality of goods", 40 "Sale price"; nothing uses the three members yet.
  - **Note (fork review 2, 2026-10-16, `verify-ui-hud.md` L6):** the shop window `FUN_004b01d0` and the sideshow's
    `FUN_004b16c0` bind their rows: the 30-day takings (`0xc077`, `0xa093`), the satisfaction gauges, the scrap rows
    and the cell's happiness (`FUN_004cd880( cell, 1 )`). The row labels need the streams `0x00755908` and
    `0x00755e80` parsed.
- [ ] **Q244. The happiness gauge draws two copies of its bar.** `docs/PLAYER-GAPS.md` gap 5, found by Alexah playing:
  the bar draws in the wrong place, split down the middle. That page holds a lead read from the code and not
  measured (`UiMeter.OnDraw`'s sampler); measure before touching the meter's arithmetic. The number it shows is
  not the fault, and which guests it counts is Q148's. Confirm: the gauge photographed beside the original's.
- [ ] **Q245. The research lab. Decode first.** Nothing spends the researcher's points (`RESEARCH_POINTS_TO_THE_LAB`,
  Q134, which read no further than `FUN_00503430`'s share-out) and nothing completes an item (`FUN_00504630`, which
  sets the researched flag and the tier; `ParkResearch` says so). UITEXT `0x1d4` says research is automatic in
  Instant Action, Lost Kingdom's Easymode; what that does is not decoded. Decode the lab's spending, the automatic
  mode and the completion to `docs/exe/`, then split the build into its sessions. The research screen (six effort
  sliders, `FUN_004aa480`) waits on it; Q239 is its two message boxes.
  - **Note (fork review 2, 2026-10-16, `verify-economy-guests-paths.md` M1 and L2, `verify-ui-hud.md` L2):**
    **finishing the last project ends an Instant Action park**: `0x00503d40` plays advisor record `0xa7` and, in game
    type 2, calls `0x00516b00`, the End feature's only builder; in Full Simulation only once every entry of the set at
    `0x00f7c58c` is flagged. A completion runs message `0x11`, the lab, message `0x12`, then `0x004aaec0`, whose
    forward picker `0x005034f0` takes the first eligible project after the current one, else the first before it, else
    none, screen open or not; a finished project's next level goes on the table's end. The screen (Full Simulation
    only): fluid `0x004a95b0` and `0x004a9fc0` stepped once a frame message `0x1e`, refresh `0x004a9c70`, setters
    `0x00503870` and `0x00503900`, the workload knob's limit, a progress fill per panel (`0x004a9bd0`).
- [ ] **Q141. Golden tickets are never awarded. Decode first, and Alexah's call on when.** Found by Q69. What reaches
  the advisor's glints in the original is a golden-ticket award: only gesture rows 1 and 13 carry the glint flags, and
  only its lines use them (`docs/exe/scenes.md`, "Gesture table"). Those lines play on the park's own advisor, model slot
  1 from the level's `advisor.wad` with clips 16 to 20, which OpenTPW does not load. The sweep also read that a row whose
  first clip is not 14 skips the lead-in (`FUN_00598bf0`, not checked). No site reaches any of it, so nothing is counted.
  - **Note (fork review, 2026-09-30):** the automatic awards run only in game type 0 (`FUN_004d4a00`'s gate covers all eleven
    tickets), so in Lost Kingdom's Easymode, type 2 (Q178's note), the original never awards a ticket by play; this
    waits for a Full Simulation park (Q197). The cheat table can still award one in any type (rows
    `0x0040c6c0`..`0x0040c7e0` call `0x005af810`/`0x005af940`), behind the cheats byte the `CHTS` module loads, which
    Easymode ships set. The checker's decode is Q194's. Review items economy-8, gap3-8.
  - **From Q177c:** the buy list and its click read the same list of ticket-bought items as the purchase: a row whose
    item has a `GoldenTicketCost` not yet bought with tickets is named "??? Mystery Ride! ???" with its negated ticket
    cost for a price (`0x004ab02e`), and a click on it is refused when the player holds too few tickets
    (`FUN_004d4b90`, `0x004ac5a9`). The row is built, from the player's `RideIds` (`docs/exe/hud.md`, "The mystery
    row"); a click on it is refused always and counted, `MYSTERY_RIDE_PURCHASE`. The ticket test and the spending of
    tickets are not built, and the purchase's own arm is counted (`PURCHASE_GOLDEN_TICKET_ARM`). Lost Kingdom's
    `tourride` (1) and `volcano` (3) carry a ticket cost.
  - **Note (fork review 2, 2026-10-16):** Lost Kingdom's golden-ticket ride, Jurassic Tours (`tourride`, ticket cost
    1), runs the TOUR script, which is not built: Q408.
- [ ] **Q142. The staff pool reads a string table again for every name.** Found by Q70's game run. `ParkStaffPool`
  opens and parses a name table for each candidate it rolls (`RollName`) and STAFF_TYPES for each kind's name
  (`NameOfKind`): 27 of a park load's 28 table reads are six files read over and over, and the staff screen and the
  `candidates` census read STAFF_TYPES once a row. Read each once, as `Localization` does. Confirm: `park jungle` logs
  seven `String table:` lines (THEMENAMES too), two of them `35 strings in 904 bytes` (ENTERTAINER_NAMES and GUARD_NAMES alike), and the
  `candidates` census and the hire screen show the same names as before.
- [ ] **Q143. `BFSTReader` takes a string's length from one byte, and four shipped strings are longer.** Found by
  Q70's review. A record is `01`, a length of at least two bytes, little-endian, in a three-byte field, then the
  characters (FileFormats `strings.md`);
  `ReadFile` reads the length's first byte and skips the other two. Of the 4,730 records in the 42 shipped files, four
  are over 255, all in UITEXT.str: row 400 (`UIStrings.Change`, 262 characters in English, which reads "CHANGE"; 256 in
  american, which reads empty) and row 417 (`SoftwareCopyright`, 615 and 614, cut to 103 and 102). Nothing reads either
  row yet, so no screen shows the cut. Read all three bytes; the reader's copy of the layout, which says "3 bytes" and
  "Unknown", becomes a pointer to `strings.md`. A test on rows 400 and 417.
  From Q241: the reader also takes a string's first text part alone, so each of the 474 parameter strings is cut
  at its first parameter (FileFormats `strings.md`, "The note on parts"). UITEXT 399, the delete-player question,
  reads empty that way, and `FrontEnd.AskToDeletePlayer` fills it with words of its own in the belief that the
  original ships it blank; it is parameter 4, then "Are you sure you want to / delete this player ?". UITEXT 205
  (the overwrite question) and 448 (a save's date) are two more. Read every part, give `Localization` a way to be
  handed the parameters, and let the delete box say the original's words. `q241/strraw.py` prints a row's parts.
  From Q241c: every part is read now (`BFSTReader.ReadParts`, `StringFile.Parts`, with the three-byte length) and
  `Localization.Format( row, (parameter, value)... )` fills them; `ReadFile` and `Localization.Get` still give the
  first part by the one-byte length, and only the Load Park screen's date (448) and the Save Park screen's overwrite
  question (205) use the parts.
- [ ] **Q144. Three hollow ride-script tests.** Found by the 2026-09-26 staleness audit, whose comment fixes now say
  what each asserts. `RideScriptWalkTests.AWalkIsCollectedOnlyOnceItHasFinished`: its second script ends on its first
  turn, so the stepper never runs and neither `WALKGET` is checked. `MorePeopleThanLanesCannotAllBeWalkedOn` asserts a
  floor, not the ceiling its name claims. `RideScriptRunTests.WhatIsNotImplementedIsCountedRatherThanGuessed` asserts a
  count is not below nought, which cannot fail. Make each assert its claim, and put the bug back to prove it (`CLAUDE.md`
  rule 6). No game run.
- [ ] **Q147. `MP2File.Duration` reads every sample as MPEG-2, and five shipped samples are MPEG-1.** Found by the
  2026-09-26 staleness audit. The getter never reads the header's version bits (`(SoundData[1] >> 3) & 3`) and always
  takes the MPEG-2 bitrate tables. By the audit's census of all 47 `.sdt`, five entries are MPEG-1 Layer I (header
  `ffff22c0`, bitrate index 2, 64 kbit/s read as 48): `keyexplode` in global `SfxHD.sdt`, and `tp_riderepaired`,
  `tp_sideshowwin_`, `mortar_2` and `mortar_3` in global `RideHD.sdt`. Their lengths come out 4/3 too long,
  `SoundCategoryFile.ReadSamples` turns away every map record that names one (its class summary says so), and the
  effects after it in its category are shifted along: by the audit's reading global lobby sfx 4 (the park entry) and
  global rides 203, 211 and 212 play nothing, and rides 202 the wrong list. Read the version and take MPEG-1's tables
  for it; the two `SoundCategoryFile` remarks that rest on the lengths ("every sample in every bank", "26 to 83ms")
  change with it, and the version split goes to FileFormats `sounds.md` (rule 14). Q43's structural walk is the other
  half, for the variations with no samples. Confirm: a test over every shipped bank that each entry's length agrees
  with its decode, `ReadSamples` and `ReadVariations` agreeing in both global categories, and the park-entry effect
  heard by capture.
- [ ] **Q148. The music and the happiness gauge count guests the original leaves out.** Found by the 2026-09-26
  staleness audit. The original's crowd count behind the music (`FUN_004c7fa0`) and its happiness average both take a
  guest only where `FUN_004fa990` passes, and that is decoded: it packs the guest's cell from bytes `+5` and `+7`
  (`0x004fa994`) and passes when the cell's kind is 0, 1, 3, 9 or 10 (`FUN_00536390`, read first-hand, and
  `FUN_00536310` to `FUN_00536350`, `hud.md`, "The cell codes"; `park-engine.md`, the PICK UP button), so a guest on a
  ride's footprint (kind 4) or outside the park is left out. `ParkAudio.CrowdLevel` and `ParkPeople.AverageHappiness`
  count every guest, and `CrowdLevel`'s remark reads the test as a dword at `thing + 8` whose meaning is not
  established, which the disassembly does not bear out. Build the filter once for both, and for `HeadingForExit`'s
  change of mind when it is built. Confirm: the Belly Bounce full, the music level and the gauge read before and
  after, predicted first; a screenshot of the gauge.
- [ ] **Q150. Scripts and the thing sweep take a frame's ticks in two loops, where the original takes both per tick.**
  Found by the 2026-09-26 staleness audit. The original's park loop runs the scripts (`0x0054f56b`) and the thing
  sweep (`0x0054f7bb`) inside one loop over the frame's ticks. `ParkRides` and `ParkPeople` each loop over
  `GameClock.TicksDue` on their own, rides first (`ParticleSystem` is a third), so on a frame with two ticks due every
  script takes both before any guest takes one. `GameClock.TicksDue`'s remark says this must become one loop once
  guests react to what rides did in the same tick, and they do now: a shut ride turns its queue away (`c54e848`). Run
  the park's systems from one loop, one tick at a time, in the original's order, keeping the three-sweep cap
  (`ParkPeople.SweepsAFrame`). Confirm: a long frame
  forced, the log's ride and guest lines interleaved tick by tick.
  From Q238: inside one sweep the original walks ONE list, every kind of thing in it, the newest first
  (`FUN_00516270`; `ride-operation.md`, "Where a ride's turn comes from"). `ParkPeople` sweeps the guests, then the
  staff, then the rides, each in that order among its own, so a hire or a thing bought after a guest still takes
  its turn after every guest.
- [ ] **Q151. A park's sky draws the lobby's horizon band. Decode first.** Found by the 2026-09-26 staleness audit.
  `FUN_005863c0` gives the band's four rings, per half, 0.99, `c08` + 0.5, `c0c` + 0.5, `v` + 0.5 and then 0.49,
  `c08`, `c0c`, `v`, with `c08`, `c0c`, `v` 0.34, 0.17, 0.01 (`0x00701f88`) by default and 0.19, 0.01, 0.49
  (`0x00701f94`) while `DAT_008bcbc8 & 0x2000000` is set (read first-hand). `Sky.UpperV` and `LowerV` hold the second
  set, 0.99, 0.69, 0.51, 0.99 over 0.49, 0.19, 0.01, 0.49, which is the lobby's, and `Sky.Band` draws it for every
  sky; `_tintRamp`'s remark, "the whole difference between the two skies", is true of the code only. Settle which
  scene sets the flag (it also gates a mirrored copy of the dome), write the draw to `docs/exe/`, then give a park the
  default set, 0.99, 0.84, 0.67, 0.51 over 0.49, 0.34, 0.17, 0.01. Confirm: a camcorder frame of Lost Kingdom's
  horizon before and after.
- [ ] **Q152. Every lightning bolt lasts 0.42 s.** Found by the 2026-09-26 staleness audit. `Lightning.Duration` is a
  constant, and neither `LobbyWeather` nor `ParkWeather.PlaceBolt` passes a lifetime to `Strike`. The original's bolt
  draw takes one (`FUN_00580320`, argument 7, in milliseconds): 500 in the lobby and `LightningTime` in a park, 2000
  in the balance file, which nothing here reads (`weather.md`, "The bolt"). Pass each scene's own. Confirm: `bolt` in
  the lobby and in Lost Kingdom, frames grabbed every 100 ms, the bolt up for 0.5 s and 2 s.
  - **Note (fork review 2, 2026-10-16, `verify-audio-lips-video.md` lead 3):** the bolt's shape is the PC's own: the
    constructor (`0x0057d2ab`..`0x0057d2e9`) sets roughness 0.6, a stop test of 6 ± 2, a ribbon half-width of 1.3 /
    (depth + 1) in screen units and branch odds of 9 in 512; the midpoint step is `0x0057e4b0` (its offset is not
    perpendicular: it uses |mid|², and its cross term is × 0.0), the branch heading `0x0057e810`, the colour pulse
    `0x0057ee30`, whose grey can pass 255 and wrap to near black. `Lightning.cs` draws one quad 9 wide.
- [ ] **Q153. Editing a queue parts from the original's walk twice.** Found by the 2026-09-26 staleness audit.
  `ParkPathBuilding.EditQueue` and `QueueEnds` both cite `FUN_00530120`, and they differ. With no link on the entry
  cell, `EditQueue` steps along the entry cell's raw `Direction` byte, where the original takes the side the thing's
  angle names (`0x005303f1`), as `QueueEnds` does with `AngleSide`; and its no-queue arm cuts nothing, where the
  original takes the path's bit when the entry cell faces a path directly (`ride-operation.md`, "The sale's drain",
  the list). Neither is said at the site. Build `EditQueue` on `QueueEnds`' walk. Confirm: a Belly Bounce bought and
  turned, its window's queue button pressed before any queue is laid, `tool` and `cell` on the anchor, predicted
  first; a screenshot.
- [ ] **Q154. The queue retile does not ask the track layer.** Found by the 2026-09-26 staleness audit.
  `ParkPathBuilding.PathLinks`, the original's `FUN_00535dd0` counting a queue cell's mutual path links, leaves out
  its fourth test, `FUN_0053ad20` of the track record beside the cell, and counts `QUEUE_TILE_TRACK_FLAGS_GATE` on
  every link instead. The save's track layer is read now (`ParkWorld.MapCell`'s `Track` fields), and
  `CellEdge.TrackCloses` answers `FUN_0053ad20` for the walk. Whether the retile's test is exactly `TrackCloses`,
  which also applies `TrackCounts`' five types (`FUN_005363f0`), is not established: read the call first, then ask it
  and write it to `park-engine.md`. Confirm: the Belly Bounce's queue relaid, `cell` on each queue cell and the
  counter's absence in `unimplemented`, the tiles predicted first.
- [ ] **Q155. Every script's speed word is 50, where a placed thing's is its own. Decode first.** Found by the
  2026-09-26 staleness audit. The object constructor pushes the item's operating speed into its script (`FUN_0055a300`
  at `0x004db534`), and the ride window's speed slider writes it (`FUN_004dd6e0`); `docs/exe/park.md`, "The clock, the
  speed word, and WAIT". `RideScript` keeps no speed word: `WAIT`'s divisor is counted as `RIDE_SPEED_SCALES_WAITS`,
  and `ParkAudio.ScriptSpeed` is a constant 50 in `ScreamVolume` and `ScreamGridIndex`, where the speed is half the
  answer. `ParkScreamTests` calls the Belly Bounce's 35 "the real park's number", which holds only if its speed is 50.
  Decode what a loaded object's script holds (the constructor's push from descriptor `+0x1a8`, and whether the save's
  `mOperatingSpeed` reaches the script after it), measure the Belly Bounce's, then keep the word per script and read
  it in `WAIT` and in both scream readings. Confirm: `rides` showing the Belly Bounce's word, and the scream's level
  by capture.
  From Q166: every trigger hands the divisor (`[ESP+0x14]`, 0.5 + 0.01 × speed, `0x00551cdc`) to the channel as its
  play rate - `TRIGANIM` `0x00552952`, `WAITANIM` `0x00552a9f`, `LOOPANIM` `0x00552be4`, `TRIGWAITANIM` `0x00552d2f`,
  `TRIGANIM_CH` `0x00553190`, `WAITANIM_CH` `0x00553325`, `LOOPANIM_CH` `0x00553465`, `TRIGWAITANIM_CH` `0x00553624` -
  where `StartAnimation` passes 1.0 (said at the site); the deadlines of `TRIGANIM` `0x005529ac`, `WAITANIM`
  `0x00552acd`, `TRIGWAITANIM` `0x00552d94`, `TRIGANIM_CH` `0x005531ee` and `TRIGWAITANIM_CH` `0x00553682` divide by
  it, and none of these divisions is counted (`RIDE_SPEED_SCALES_WAITS` fires only from the ride window). The unbuilt
  `TRIGANIMSPEED` passes its rate / 1000 × the divisor (`0x00552f6e`), and its deadline, the length × 1000 / its rate,
  does not divide by it. The length a trigger answers does not depend on it (`FUN_00472f60`,
  `0x0047323e`..`0x00473258`).
  From Q174: `+0xe4` is the play rate in thousandths. After every turn of a script with a model the scheduler writes
  `(0.5 + 0.01 × speed) × +0xe4 / 1000` into every channel's queued speed `+0x30` (`0x00551789`), which a promotion
  and a loop's replay start at; OpenTPW's promotion keeps the speed the clip was queued at. `TRIGANIMSPEED` leaves its rate
  there (jungle `Gates` 4000). The Easymode Belly Bounce was saved looping at 1.1, and OpenTPW's second trigger at its
  word 43 (`park.md`, "Where OpenTPW's animation state parts from the engine's", difference 4) replaces it at 1.0. With a divisor other than 1, a clip under 300 ms or no model makes
  `WAITANIM`'s qword sum wrap `+0xa0` (`park.md`, "`WAITANIM` is NOT `TRIGANIM` with a wait").
- [ ] **Q156. The staff screen's two happiness meters fill upward. Decode first.** Found by the 2026-09-26 staleness
  audit. `UiMeter` fills every meter from the bottom, a choice its remarks justify by the gadget's gauge housing being
  taller than wide (59 by 224); the staff screen's two `happygrad.wct` meters are 376 by 45 (`ParkStaffScreen`,
  `0x32d` and `0x32c`). What paints a meter is the two sub-objects `FUN_0066d885` hands the value to (`+0x98`,
  `+0x9c`, their vtable `+0xc`), untraced. Trace their geometry, write it to `hud.md`, and fill each meter the
  original's way. Confirm: the staff screen at a known happiness, photographed.
- [ ] **Q157. Guests' and rides' reached arms that count nothing.** Found by the 2026-09-26 staleness audit. Each is
  reached in Lost Kingdom, unbuilt, and calls no `Unimplemented.Report` (`CLAUDE.md` rule 4). In `PeepBehaviour`:
  `Entering` lacks `FUN_004ffb20`'s guard (park shut or gate not open, back to the gate), reachable since the
  entry-price door shuts a running park; `HeadingForExit` has no
  change of mind (`FUN_00500a50`), which a saved guest can take. `CellReroute` never
  runs `FUN_005108a0`'s diagonal pass after a scan that splices nothing, which is how most routes end.
  `Peep.Tick` leaves out the cell's `RegionFX` term (`FUN_00501650`) on every needs turn.
  (`ParkRideOperation.Invite`'s `RunsContinuously` is built, Q304.) Count each where the original tests it, and
  put each build without an item in the queue. Confirm: `unimplemented` over a timed run, each name present, predicted
  first.
  - **Note (fork review, 2026-09-30):** `EntertainerConstsPerGrade[g].HappinessEffectOnCell` (4, 8, 12, 16, 20; `Standard.sam`)
    has no reader in our decode; find it before building the entertainer's half of the region term. Review item peeps-v3.
  From Q126: `FUN_0055a470` (`0x0054f828`, every eighth step, a dropped sweep's too) finds the ride the camera is
  on (`FUN_0042a4e0`, the list at `[0x008791b0]`) and hands its `+0xd8`, or the default `[0x0078578c]`, to
  `FUN_0051c8d0`, a sound setting not decoded. Nothing here has it, counted or not.
- [ ] **Q158. Five park-screen readouts are stand-ins, and nothing counts them.** Found by the 2026-09-26 staleness
  audit (`CLAUDE.md` rule 4). `ParkObjectWindow`'s Age row prints a bare number: its wording, `FUN_006acd60` with
  `0x1b1`, is not decoded. Its scrap row prints the build price, where `FUN_004e2400` scales it by the age bucket's
  percentage, `SCRAP_VALUE_DEPRECIATION`, counted only when something is sold. `ParkEntryPriceScreen`'s heading
  `0x4f3af` is empty, and its remark says "left empty and counted"; the original titles it with the name of the thing
  at world `+0x1da732`, `mParkGates` (`ParkWorld`), and its help row 191 reads "Park name - left-click to edit", so
  the title is the park's name and its edit is Q28's. `ParkGadget.ShowDate` prints the machine's short date in place
  of a format not read back. `ParkBuyScreen`'s description panel `0x1ea` draws its frame and the footprint picture, and neither the item's turning model nor its name row `0x1ec`. Count each
  where it is drawn. Confirm: `unimplemented` after opening a ride's window, the entry-price screen and the buy
  screen.
  From Q238: the buy list's prices are bare numbers where the original letters "$ 2500" (UITEXT `0x1c1`,
  `UIStrings.Dollar`; `q238/fix/b-buy-screen.png` beside `rv8/orig/b1.png`), and nothing counts it.
  From Q130e, seen beside the original's lists: the hire list's wages are bare numbers too ("$ 63" there); a buy
  row with no state shows an empty box there and nothing here; Buy Land's price is `[0x0078560c]`, 10 there and 0
  here; and the headings' lettering is larger there than the rows', here the same.
  From Q188: the buy screen's panel is the object window's preview (`ParkObjectPreview`, which wants a placed thing
  today: give it an item); in the original it is (437,162) 360 by 363. The footprint picture at its lower left is
  built (Q233b) and is drawn in front of the model; the name row `0x1ec` above it is not built.
- [ ] **Q159. `SdtArchive.GetFile` matches a truncated name the wrong way round.** Found by the 2026-09-26 staleness
  audit. It tests `x.Name.StartsWith( name )`, the stored name against the one asked for. A `.sdt` name field is 16
  bytes and a longer name is cut to fit, so asking for "TP SCREECH 11.mp2", stored as "TP SCREECH 11.m", never
  matches, which is what its summary says `StartsWith` is there for; and "TP SCREECH 1" would find "TP SCREECH 11.m".
  Jungle's `LobbySfxHD.sdt` and `AmbientHD.sdt` store eight such names. No caller opens a `.sdt` entry by name yet
  (the archive handler `Game` registers reaches it through `OpenFile`). Match an equal name, or, for a stored name
  that fills its field, the request's prefix of that length. A test on the eight. No game run.
- [ ] **Q160. Two park tests do not test what their names say.** Found by the 2026-09-26 staleness audit.
  `ParkFootprintOccupancyTests.LeavingACellTheThingIsNotOnKeepsWhoeverStandsBehindIt` passes whatever `LeaveCell`
  does: `LeaveCell( 58, 15, Ride )` already unlinks the ride from (58,16)'s list, `LeaveCell( 58, 16, Guest )` then
  leaves its `Occupant` nought, and `ParkPicking.ThingOn` falls back to the cell's parent and answers the ride anyway.
  Assert the cell's `Occupant`; if it then fails, check `FUN_004d9280` before making `LeaveCell` ignore a thing not on
  the cell asked about. `ParkRideJoinTests.NoQueueGrowsPastTheRoomItHas` takes the room as the save's
  `QueueSizeInCells` times four, nought on the Drinks Shop and the three toilets, where the gate measures their walked
  cells (`ParkRideChoice.QueueCellsFor`); a run that queues a guest at a toilet fails it falsely. Use the walked
  count. Put the bug back for each (`CLAUDE.md` rule 6). No game run.

- [ ] **Q210. Hoardings: the original's terrain rectangle and its base-vertex gate.** Found by the 2026-10-04 review
  (`u6-hoardings-1`, `-2`, `-4`; `docs/exe/ride-hoardings.md`). `ParkRideHoarding.Ground` samples `x0 + gx` where the
  original samples `x0 + trunc( gx * (w - 1) / w + 0.1 )` inside the model's inclusive rectangle (`+0xc0`, `+0xc4`,
  `FUN_00467030` `0x004670c1`, `FUN_00454190`); counted `HOARDING_TERRAIN_RECTANGLE`. `ParkObjects` gates on
  `Nodes.Count > 1` and reads `Meshes[0]` where the original counts base vertices (`FUN_00469a80`, `0x0046a838`) and
  reads the mesh with the most (`0x0046a82c`). Port both, and write the test's expected value from the executable's
  formula, not from the code. Confirm: a ride bought on sloping ground, closed, photographed with `hoardings`; then
  the same panels beside the original's under Proton (their height, which rises first, where the sign sits).
- [ ] **Q211. Tests that run the callers, not the helpers.** Found by the 2026-10-04 review: fifteen bugs put back
  left all 1,764 tests green. Nothing runs `ParkBuilding.Buy` (taking `BindOperation` out of `Build`); `ParkObjects`'
  hoarding gate, its saved-slot `Restore`, `ParkRideHoarding.Fill`'s sliding V, the panels' `OrderBy`, the `+ 0.1`
  truncation; `Advisor.PlayParkResponse` (its `Busy` guard, the end clip's length); `SayOnlyOnce`'s `history.Said`;
  `Level`'s `DrawStatus`; `SetupParticles` before `SetupParkEntities`; `FreshPark`'s goods and chance on the two fixed
  objects; the prankery id's `& 0xffff`; the cash's `Math.Max( 0, ... )`; `Players.Load`'s read-only stand-in. And
  `ArchiveDamageTests.EveryInstalledWadEntryStillDecodes` prints its hash and count without asserting either. Write
  each test, put its bug back, see it fail (`CLAUDE.md` rule 6). No game run.
- [ ] **Q212. The ride window's other statuses. Decode first.** Found by the 2026-10-04 review (`u7-ride-door-6`,
  `-7`, `-9`, `-10`). `FUN_00485f60` answers `0xb`..`0x14` (80/100/255) for a ride with a mechanic called (`+0x64`),
  2 (UITEXT 366) for state 1 with nobody assigned and `0x15` (UITEXT 385, "REPAIRS IN PROGRESS") with one, 24 for an
  exit not connected; `ParkClosedStatus` answers 1, 22 and 23 only, shows grey CLOSED for a mechanic call, and
  `ParkObjectWindow` shows BROKEN DOWN from `VAR_BROKEN` with no decode behind it. `OBJECT_WINDOW_EXIT_CONNECTION_STATUS`
  and `QUEUE_VERDICT_CORNER_RULE` are reported every frame a window is open (9,631 and 24,273 in one run): report on
  a change. Confirm: each status photographed beside the original's.
- [ ] **Q213. The status box's lettering is smaller than the original's.** Seen 2026-10-04 with the two side by side
  (Q208): the original's "LINE NOT CONNECTED" fills the striped box in two lines of large type, OpenTPW's is small.
  Decode the font and size `FUN_004ade40` draws with. Confirm: the box photographed beside the original's.
- [ ] **Q214. A fresh Full Simulation park's four gaps.** Found by the 2026-10-04 review (`u1-startup-1` to `-4`).
  `ParkStaffPool.Roll` gives every candidate costume 0 where `FUN_00507600` stores `FUN_00541f70( kind )` at record
  `+9`, so every hired entertainer and mechanic wears bank 0 (`guests`: `bank 0+0` on all five kinds). The land
  boundary (track types 25 and 12, from `Hoardings.sam`) is written into the cells and neither drawn nor counted.
  The catalogue's emitters are registered and the per-item slot table (item `+0x4dc`, `0x0041455a`) is thrown away.
  `FreshParkBoundary.Apply` throws where the original stops at its 64th entry (`0x00532517`), and a test pins the
  throw. Confirm: several entertainers hired in a fresh park, more than one costume on screen; the boundary beside
  the original's fresh park.
- [ ] **Q215. The park advisor's tick: three paths neither built nor counted, and the original never watched.** Found
  by the 2026-10-04 review (`u5-advisor-door-2`, `-3`, `-9`). `FUN_0059a550` polls a metadata row each tick
  (`FUN_0059c680( 1 )`); `FUN_0059b620` posts a message after `Advisor_SayResponse` (`FUN_0040f740`, `FUN_0040fb10`,
  response groups 150 and 151 for the door); `Advisor_SayResponse` answers 0 for a sample of no length; event 10 keeps
  the cooldown stamp. Count each, then build. Confirm: press the door in the original under Proton and hear whether
  he speaks (Q90 predicted silence from the scores and never ran it), then the same in OpenTPW.
- [ ] **Q216. The save-preservation policy. Alexah's call first.** Found by the 2026-10-04 review (`u9-foundation-2`,
  `-5`). OpenTPW will not rewrite a gms.dat or Config.tcf it did not read whole; the original overwrites. A Config.tcf
  of version 0, of another version or with a tail blocks saving the options for good, and nothing on screen says a
  save was refused (rule 11: say it in the game's own style). Decide whether to keep it. Either way, measure the
  stricter `ExpandedMemoryStream.ReadBytes` over all 47 `.sdt`, the 42 `.str` and the saves: a short read it used to
  pad now throws.
- [ ] **Q217. Three leftovers among the park's people.** Found by the 2026-10-04 review (`u3-staff-paths-1`,
  `u4-gate-1`, `u2-arrivals-4`). Staff are kept on paths by a fence the original does not have; its own recovery is
  the no-links arm (`0x004f95c0`), built for staff (Q112): ask Alexah whether the fence stays. `ParkPeople` holds two
  versions of `FUN_004fa990`: `InsideGateCensus` reads the position's cell as the original does (bytes `+5`, `+7`),
  `OnACountingCell` reads the occupancy link first; make them one. `ParkCatchUpOrderTests`' row (16, 1) asserts the
  open Q150 defect as the expected result and will go red when Q150 is fixed: say so in the test.
- [ ] **Q218. Five gaps left by the Q208 and Q209 corrections.** Found 2026-10-04 by a second read of those four
  commits, each checked against the code. (1) `RideHoardingTests.TerrainEndpointsFollowTheOriginalNormalizedQuarterTurns`
  still says by its name that it follows the original, while `docs/exe/ride-hoardings.md` says it checks OpenTPW's
  larger rectangle: rename it now, and Q210 writes the real one. (2) `ride-hoardings.md` says `Meshes[0]` is the mesh
  with the most base vertices in every shipped outline; the corpus test reads `Meshes[0]` and never counts them.
  Measure it over all 129 outlines and assert it, or take the sentence out. (3) `ProfilePreservationTests` has no
  profile that reads at `Load` and fails at `Select`: the unreadable one is unreadable before `Load`, the cached one
  is deleted. Write that case (readable, then mode 0, then restored) and put the `CanWrite` revoke back out to see it
  fail. (4) `ParkBoughtClosedTests.ALoadedDisconnectedRideKeepsItsSavedDoor` builds a new ride and rebinds it; no save
  is read. Load one, or name the test for what it does. (5) `docs/exe/ride-operation.md` says the remeasure tail
  `FUN_004de1f0` leaves at once when `+0x68` is non-zero; `ParkRideOperation.ReopenAfterRemeasure` clears
  `AssignedStaff` whatever was decided and cites `0x004de48c`. Read the tail in Ghidra and make the two agree. Also
  two sentences: `docs/exe/saves.md`'s "None of this is the original's behaviour" follows a sentence that says the
  first save is the original's, and `STATUS.md`'s "cached profiles stay read-only" is true of an unreadable file
  only. Put the bug back for each test (`CLAUDE.md` rule 6). No game run.

- [ ] **Q219. Light the models from the normals the file stores. Decode first.** Found by the 2026-10-04 effort
  audit (`r-f1`, `r-c2`). The original's model lighter `FUN_00574660` takes one normal per vertex-order entry from the
  table at mesh `+0x64` (stride 12, `+0x5e` entries); OpenTPW lights every placed model from smoothed normals it
  computes, and `ModelFile.ReadFaceNormals` reads only as many entries as the faces index. Count the table's entries
  across all 2,129 `.md2`, compare the stored normals with the computed ones, then read and use the stored ones. The
  FileFormats models page gets the count. Confirm: one ride and one shop photographed beside the original's under
  Proton, the same sun.
- [ ] **Q220. Tests for the wiring the effort audit found untested.** Found by the 2026-10-04 effort audit: seventeen
  bugs put back left all 1,766 tests green. `Level.Update`'s day roll, month turn (its order: training, the bank,
  wages) and year turn; `ParkRides.Resume`'s `MoveKeptReadings` and `BindNew`'s `RideNodes`; the walk leg's x/z form
  (a leg a hair from a whole unit); the Hot Pot's `ParkBumperBoats.Put` and its drawn heading, the speed word's
  placement wiring, the seating friction 7/10 and the broken ride's 1000, the car's turn, the engine fade's length,
  `TowardsCamera`; `ParkGuestSprites.DrawBalloons` (the bob's rate and write-back); the camcorder's
  `UseFirstPersonPictures` on enter and leave; `ParkObjectWindow`'s 30 days and 4 s; the buy list's features tab.
  Write each test, put its bug back, see it fail (`CLAUDE.md` rule 6). No game run.
- [ ] **Q221. Eight leftovers from the effort audit, each dead by content today.** Found by the 2026-10-04 effort
  audit, each read in the listing. (1) `ParkScriptStates` reads the tick and next handle with `header > dword * 4`; it
  should be `>= (dword + 1) * 4`. (2) `ParkBuyScreen.Listed` applies two of the original's four filters: add the
  upgrade (`+0x284`) and model-failed (`+0x508`) tests or count them (`FUN_004aaf70`). (3) `ParkAudio.ScreamEffectFor`
  silences a negative band; `FUN_00551130` plays `0x48` for one. (4) `RideScript.NextRandom` answers 0 for every
  bound + 1 at or below nought; only -1 divides by nought (`0x00553994`). (5) `ParkItemHeights.Over` floors the
  cell's middle where `0x00452bb0` truncates corner offsets: measure it on raised ground. (6) The original rolls a
  rider's head sprite to its node (`FUN_0044b510`, `0x0044ba04`); `DrawHead` does not, and it is counted (`RIDER_HEAD_ROLL`): build
  it. (7) `Peep.Pace` works at 24 bits and `ShopCostOfGoods` at 53 under one control word: settle which, the pinned
  18 may be 19. (8) Two docs lines not yet re-measured: `park.md`'s selector table rows 8 and 9 leave out the
  `0x4000` and type gates of `FUN_00544c80` and `FUN_00544e50`; FileFormats `sound-categories.md` says the masks use
  bits 1, 2 and 4 beside 645 speech variations holding 100.
- [ ] **Q222. The original's guests come up empty-handed, and ours do not. Measure first.** Found by Q107's
  comparison with the original (`ride-operation.md`, "Q107"). The same stock Lost Kingdom park left alone for 240 s:
  the original's chooser named nothing 112 times and a thing 72 times (Jungle Spray 27, Drinks Shop 18, Belly Bounce
  16, toilets 11), with at least fifteen arrivals; OpenTPW's named nothing never and a thing 16 times (Belly Bounce 14,
  Jungle Spray 1, Drinks Shop 1), with one arrival (Q26). In the original seven of the empty hands were at (52,29),
  where the Jungle Spray turned guests away fourteen times; here `why` scores the Belly Bounce 10 or more for a fresh guest anywhere inside
  the park. So a score, an offer gate or the clock the chooser reads (Q132) differs, and Q104's and Q105's scores were
  never compared with the original. Read one guest's scores in the original at a known cell (a break in
  `FUN_004fcc30`, or its inputs from memory) beside `why` for the same guest and cell, find the term that differs,
  then file its build. Instruments: `q107/orig/watch.py` and `watch1.log`, `q107/base`.
  From Q252 (built since, Q304: `Invite` reads `RunsContinuously`, and it carries five): **OpenTPW's Belly Bounce carried one rider at a time.** Every `rides` census of every run to hand
  reads bouncing 0 or 1, with fifteen queueing (`q252/run2`); the original, given the same file, had five aboard
  twelve seconds after its load, a guest boarding every three (`q252/orig/a-load.log`), and its riders ride 121
  sweeps, or 32 more. So the crowd on the queue is boarding's, not the chooser's alone: read the boarding turn first.
  From Q26b: the crowd on the Belly Bounce now sizes the loads. With thirteen arrivals its sixteen places fill, the
  offer gate refuses it, and a load called on that sweep is 4, not 12 (one run of two, `q26b/gate`); the original's
  passed the gate on every sweep of two runs.

- [ ] **Q224. A happy guest does not jump on their deciding turn.** Found by Q111 (`ride-operation.md`, "The state-6
  turn, in order", (a)). More than 100 sweeps past `+0x208` and happiness above 80: spot animation 5 and the turn ends
  (`0x004fecd2`..`0x004fecea`). Every part is built (`PeepBehaviour.PlaySpotAnimation`, `SpotHappy`); build it where
  `DECIDE_HAPPY_SPOT_ANIMATION` is counted. The original's stock park made one in 964 sweeps (guest 33 at 83).
  Confirm: a guest at happiness 90 deciding, set 5 for eleven sweeps and no sooner than 101 sweeps again; a screenshot
  of the jump, beside the original's.
- [ ] **Q225. A guest full of litter neither walks to a bin nor drops it. Decode first.** Found by Q111, arm (c).
  Litter 90 or more: the nearest `HoldsLitter` thing within three cells that routes, `MajorDest` and state 9, whose
  turn `FUN_004fff20` walks there, sets the bin's script variable 0 and zeroes the litter; none, litter of a drawn
  type on the cell (`FUN_004d93b0`) and the level zeroed. Decode `FUN_004d93b0` and what a cell's litter is drawn as,
  and check `FUN_004fff20` whole; then build both where `DECIDE_LITTER_BIN_ERRAND` and `DECIDE_LITTER_DROPPED` are
  counted. Confirm: a guest at litter 90 beside the bin at (44,29) walks to it, `peeps` showing litter 0; one far
  from it drops, `cell` showing the litter; a screenshot of each.
- [ ] **Q226. Nobody is sick.** Found by Q111, arm (b). Illness exactly 100 on a third of deciding turns: spot
  animation 7, litter type 7 on the cell, event `0x12`, sound `0xcc`, illness nought, and the turn ends. After Q225,
  which builds a cell's litter. Build it where `DECIDE_VOMIT` is counted. Confirm: `need vomit 100`, a guest sick
  within a few turns, `peeps` showing illness 0; a screenshot.
- [ ] **Q227. Nobody stops to watch the entertainer.** Found by Q111, arm (e). An entertainer on the nine cells
  around a deciding guest, and the nearest one performing (staff state `0xe`): event `0xe`, the guest turned to face
  them, and the turn ends. The original's stock park did it eight times in 964 sweeps. After Q133d, which builds the
  performance; counted meanwhile as `DECIDE_ENTERTAINER_BESIDE`, whoever the entertainer is doing. The fireworks half
  (`DECIDE_WATCH_FIREWORKS`) is dead by content in Lost Kingdom. Confirm: a guest beside the performing entertainer
  facing them; `facing 1` and a screenshot.
- [ ] **Q228. A prankster plays no pranks.** Found by Q111, arm (f). Below happiness 15, on a draw under the
  prankery: a stink bomb (litter type 8, sound `0xcf`) on `PeepInfo.StinkbombLikelihood` of a hundred, litter, or
  the balloon of another guest on the cell let go; event `0xf`, a type-`0xe` bus message and happiness up one. After
  Q225. Decode what the bus message's receivers do with it before building. Counted as
  `DECIDE_PRANK_STINK_BOMB`, `DECIDE_PRANK_LITTER` and `DECIDE_PRANK_BALLOON`. Confirm: `need prankery 102` beside a
  guest holding a balloon, the balloon let go; a screenshot.
  - **Note (fork review 2, 2026-10-16, `verify-economy-guests-paths.md` L3):** the receiver: the guard sent is the
    idle or walking one (state 0 or 1) nearest the staff HQ's own cell; `0x005099c0` never reads the prankster's cell,
    and a tie goes to the newest hire. The HQ sits at (0, 0) in Easymode and in all five Full Simulation saves, so it
    is the guard nearest the map's corner wherever the prank is. Not read: anything else acting on message `0xe`, the
    escort `0x004d6a50`'s arms.
- [ ] **Q229. A member of staff put down sets off on the next sweep, where the original's stands eleven. Decode
  first.** Found by Q112's measurement of the original (`ride-operation.md`, "OpenTPW takes the arm"). Its guard, put
  down from the staff window's PICK UP, reads state 7 to 0 on the click, 0 to 1 with a destination a sweep later, 0
  again inside that same tick, and walks eleven or twelve sweeps on (ticks 1206, 1207, 1207, 1218; 1732, 1733, 1733,
  1745). `ParkPeople.DropStaff` sets Idle from Carried, which stamps 0, so ours decides on the next sweep and keeps
  walking. What writes the second 0 is not read: the hand mode's uninstall puts a worker it still names down again
  (`0x0046cdc0`, `FUN_00505ea0`), and state 0 from a walk stamps the clock, which would fit. Decode the drop's order,
  then build it. In the second run the walk also stopped on grass at (43,27) eight sweeps in and the no-links arm ran
  again from there: read why. Confirm: `staff` after `putstaff`, `idleSince` and the sweep the walk starts, predicted
  first; a screenshot.
  From Q133b: the original's handyman, put down two cells from a dirty toilet, read `0xa` on the tick after the
  put-down and the toilet's stamp was written again a tick later (1389, then 1390: `q133b/orig/a.log`), so the
  second decide runs for him too; from `0xa` the idle between stamps nought, and he does not stand eleven.
- [ ] **Q231. Under an open park screen: F1, the wheel and the release. Decode first.** Found by Q115
  (`park-engine.md`, "A park screen is open"). The screens' key handler answers a plain F1 let go by calling
  `FUN_005194d0` on the world and `FUN_0059ab50` on what it answers (`0x00488bfb`), not traced; nothing here reads F1 in
  a park. Where the wheel's message goes with the pointer over a screen's body is not decoded: here the wheel zooms
  the camera wherever the pointer is, unless a list or a slider takes it. And an armed tool's click beside a screen is
  taken on the press here, where the original skips the press and commits on the release (`Level.KeptFromThePark`),
  which is `RunBuildMode`'s standing difference. Decode the two, count each where it is reached, then build. Confirm:
  F1 and the wheel over the buy screen beside the original's, read from memory.
- [ ] **Q232. A list opens with no row selected, where the original's opens with one highlighted. Decode first.**
  Found by Q117 (`hud.md`, "A right click on a list"). The original's add selects the first row as a list fills
  (`FUN_0066525c( 0 )` in `FUN_0066403b`), and its visitors list opened with a row highlighted every time: the first
  on one opening, the second and the tenth on two others, so something else moves it (the all-items builders call
  `FUN_00664d2a` and `FUN_00665739` with a thing; the two-second rewrite and the sorted insert are the other
  candidates). `UiList.FirstRow` counts the add's selection and builds none (`LIST_FIRST_ROW_SELECTED`), so a right
  click that misses every row before one is chosen goes nowhere here (`LIST_RIGHT_CLICK_FIRST_ROW_NOT_SELECTED`),
  and `Remove` drops the selection on the `0x100` branch though all five list trees have the bit clear
  (`LIST_RESELECT_PAST_THE_WINDOW`). Decode which row each of the five lists opens on and what the buy and hire
  lists do, then build it. Confirm: the visitors list opened beside the original's, the highlighted row predicted
  first; a screenshot.
  From Q233b: the buy list now selects the row under a moving pointer (`UiList.SelectsUnderPointer`, the flag `0x80`);
  the hire list carries the flag too and does not set it, and the three list screens' trees carry it (`0x291`). With
  no first row selected, the buy screen's panel stays empty until the pointer has been over a row, where the
  original's shows the top row half a second after the list fills.
  From Q356 (`hud.md`, "The first row is picked as a list fills"): the pick is built for the hire list
  (`UiList.PicksFirstRow`, its `0x401` posted and taken by the screen's `Update`); a list sets it once its screen's
  handler of `0x401` is read. `gmove.py` glides diagonally and the pointer over a list picks: check the pointer's
  path in the original's visitors runs (the second and tenth rows) before looking for anything else.
  From Q375 (`hud.md`, "The miscellaneous tab: a row an object, and each tab's sort"): the pick is built for the
  four all-items lists too (their handler has no `0x401` arm); the all-staff and visitors lists still count it.

- [ ] **Q247. A loaded park is built again behind the loading screen, and the list has no scrollbar.** Left by
  Q241c (`saves.md`, "OpenTPW's Load Park"). The original reads a park file over the running park, with no loading
  screen and no change of scene (`FUN_00414d40`: twelve teardown calls, the modules, `FUN_00415140`), where
  `ParkLoadScreen.Load` asks `Game.RequestParkLoad` for the whole scene again; Restart Park is the same
  (`ParkFrontEnd.RestartPark`). And the stream's scrollbar (control 1: `!slider`, `b_up`, `b_scroller`, `b_down`)
  is built on no list here: a list longer than its window is counted as `LOAD_PARK_LIST_SCROLLBAR` on this screen
  and the wheel scrolls it. Decode the teardown first, then decide with Alexah whether the park is rebuilt in
  place. Confirm: a load timed against the original's 1.3 s, and a folder of fourteen saves scrolled by the bar.
  From Q241f: no load here reads the camera module, which the writer now writes (`ParkWorld.Camera` holds it,
  `FUN_0042cec0`; `saves.md`, "Loading the modules", Camera): a loaded park opens on the default view, where the
  original's opens where the save's camera stood. The file's rotation is the orbit camera's yaw negated.

- [ ] **Q249. A park that was never loaded from a file has nothing to carry.** From Q241e. The fresh world (a
  Full Simulation park, or a player whose folder holds no park file) is made by `FreshPark`, so the writer's
  carried modules have no source. Decode what each module holds in a `restart.INTS` written at tick nought
  (three are in Alexah's saves), make each afresh, and take the legal text from the level's shipped park file.
  After Q241j. Confirm: a fresh Full Simulation park saved and loaded by the original under Proton.
  From Q241f: `Level.WritePark` counts such a park as `SAVE_PARK_WITH_NO_FILE` and writes nothing.

- [ ] **Q270. The park music's level counts every guest, where the original counts only those on five kinds of cell.**
  From the week-end audit (stage 4; read twice from the listing, not yet measured in the original). `FUN_004c7fa0`
  (`0x004c801f`) asks `FUN_004fa990`, which takes the guest's cell and passes only a cell of type 0, 1, 3, 9 or 10
  (`scenes.md`, "The level"). `ParkAudio`'s level takes `Peeps.Count` with no cell test, and its comment says the
  five tests read the thing's `+8` and that the field is not established; `Peep.CountsOn` is already that test.
  Build the count through it. Q148 and Q217 touch the same filter. Confirm: `music` read with guests standing on a
  queue cell and off one, beside the original's level for the same file.

- [ ] **Q271. The model under a point misses the park gate's row.** From the week-end audit (stage 4; read twice
  from the listing). `FUN_00527e80` (called at `0x00452a82`) answers the gate thing the world keeps at `+0x1da732`
  for every cell of the entrance's row, from one cell before EntranceA's x to one past EntranceB's, without reading
  the cell, and only otherwise goes to `FUN_00527d60` (`park.md`, "The scene's height under a point").
  `ParkSceneHeight.StandingOn` answers null there. Measure first whether the gate thing's type byte is 3
  (`0x00452ac8`): without it the original answers nought there too. Confirm: the height read under a point on the
  gate's row in both games.

- [ ] **Q272. A mechanic arriving at a ride that waits for an upgrade is sent away.** From the week-end audit
  (stage 4; read twice from the listing). `FUN_004da740` puts him to work on any of three: the ride's state 1, its
  state 2 (`FUN_004e03e0`, `0x004da7ca`) or the ride's `+0x64` call, each going to `FUN_004da370` with `0xd`
  (`0x004da7fa`). `StaffBehaviour.ArriveAtTheRide` tests state 1 and the call only, sends a state 2 ride's mechanic
  back to his decide and counts nothing; its summary gives the two tests as the original's. Build the third, or
  count it. Confirm: a ride put to state 2 with a mechanic sent to it, his state read in both games.

- [ ] **Q273. A node's direction is normalised in another order and width than the original's.** From the week-end
  audit (stage 4; read twice from the listing). `FUN_00556b90` sums (x*x + y*y) + z*z, keeps the root as a float
  and divides each element by that float (`0x00556cba`, `0x00556cc6`, `0x00556ccc`). `RideNodes` sums
  (z*z + y*y) + x*x and divides by the double root, and its comment says the build takes the third element's
  square first. The last bit of a saved direction can differ. Confirm: a written model record's direction words
  compared bit for bit with the original's save of the same park.

- [ ] **Q274. A sprite the writer makes takes its first turn at once, where the original's waits one interval.**
  From the week-end audit (stage 4; read twice from the listing). The constructor `FUN_004758f0` writes `0x3e` to
  `+0x80` (`0x004759c4`) and the clock plus that `0x3e` to `+0x7c` (`0x004759df`). `ParkWorld.People`'s made sprite
  leaves `+0x7c` nought, says the constructor does, and writes no `+0x80`. Confirm: a guest made here and saved,
  the two words read in the file and beside the original's for a guest it made.

- [ ] **Q275. A lookup record's flag `0x10` is set on records that have no position.** From the week-end audit
  (stage 4; read twice from the listing). `FUN_0044a870` tests the node word's `0x400` (`0x0044a924`) only inside
  the branch its file flags' `0x30` opens (`0x0044a904`). `ParkModelTables` sets `0x10` from the node's `0x400`
  alone. Confirm: the records of every model in the nine park files compared with the tables built here, and a
  node with `0x400` and no position named.

- [ ] **Q276. A rider's head is always drawn from their own bank.** From the week-end audit (stage 4; read twice
  from the listing). `FUN_004fcac0` gives the rider's own `mSpriteID` only where a costume bank (kind 3) is loaded
  (`FUN_00541f60( 3 )`, `0x004fcac6`); where none is, it stores the rider's id at the world's `+0x1da708` and
  answers a draw among the kind 1 banks (`0x004fcae5`..`0x004fcb04`). `ParkGuestSprites` and `ParkFileWriter` give
  the rider's own bank in every case. Confirm: a park with no costume bank loaded, a rider's head bank read in
  both games.

- [ ] **Q277. Starting a clip binds every other channel's clip again.** From the week-end audit (stage 4; read
  twice from the listing). `FUN_00472f60` takes the outgoing clip's marks off (`FUN_00472310`, `0x004730a9`) and
  binds the new one (`FUN_00472d70`, `0x00473117`), nothing else. `ParkModelTables`' `Running.Started` also binds
  every other channel's clip again, so a track bit two channels' clips share stays set here where the original
  drops it; its summary gives the rebinding as the original's. Confirm: a model with two channels sharing a track,
  its node words after one clip is replaced, against the original's save.

- [ ] **Q278. An entertainer whose bank has no state group performs on animation 0.** From the week-end audit
  (stage 4; read twice from the listing). `FUN_00541fa0`'s count is tested for not 1 (`0x004d47cc`), so nought
  takes the modulo arm and queues `0xd` plus a draw modulo `0xffffffff` (`0x004d47d8`..`0x004d47e1`).
  `StaffBehaviour` takes that arm only above 1, counts `ENTERTAINER_BANK_WITHOUT_A_STATE_GROUP` for nought and
  starts on animation 0, which its remarks give as a deviation no shipped bank reaches. Dead by content today:
  build it as the original's or leave the count, on Alexah's word.

- [ ] **Q279. `Level.WrittenCells` writes a footprint cell on tile 8 only where the cell joined a footprint or
  changed its type or parent.** So a file's thing moved and put back on its own anchor (sell then buy) goes out
  with its unchanged cells on bare ground's tile 55, which the sale left and `ParkBuilding.Stamp` keeps. Every
  footprint cell should go out on tile `(0, 8, 0)` as every footprint cell of seven park files is
  (`docs/exe/saves.md`, "OpenTPW's writer, a thing bought and a thing sold"); a test that sells the Drinks Shop,
  buys item 1203 on (43,30) again and reads `WrittenCells` for that cell's `TileIndex` would show 55 From the
  week-end audit (stage 6, the second reading of commit `8422bffa`; read from the code, not reproduced in the
  game). `OpenTPW/World/Level.cs`, line 907.

- [ ] **Q280. A file's bumper ride with a peep on its leaving or boarding list and no car is written over where it
  lies once the running lists are empty (`Level.WritePark`.** `Take`: only `CarsOf` is asked of the file), so the
  file's type 7 and 8 chunks stay and the next load puts those peeps back on the lists. It should be taken out and
  written whole whenever the file holds any chunk but the record and the close under its handle, as `FUN_005428e0`
  writes the lists as they stand; a test that loads the commit's own "Boarder" file, empties the list and writes
  again would show the stale chunk From the week-end audit (stage 6, the second reading of commit `05e54382`; read
  from the code, not reproduced in the game). `OpenTPW/World/Level.cs`, line 587.

- [ ] **Q281. A member of staff who reaches the picket keeps the facing their walk ended on, and turns on the spot
  from there.** The original's state setter writes nought to the facing as state 5 is entered (FUN_005054d0 case
  5, 0x00505582, beside the speed and animation 0x13 that Staff.SetActivity already takes from it), so a striker's
  turns start from 0; a test that walks a member to the picket and reads PeepWalk.Heading would show it. From the
  week-end audit (stage 6, the second reading of commit `0db41100`; read from the code, not reproduced in the
  game). `OpenTPW/World/Park/StaffBehaviour.cs`, line 432.

- [ ] **Q282. A handyman's thought bubble is taken away only on his own sweep in four, as the other kinds' are, so
  it lasts 13 to 16 sweeps.** The original's handyman pre-step FUN_004d7060 ends by calling the expiry
  FUN_0050be40 ungated on every sweep (0x004d70ee; docs/exe/ride-operation.md, the pre-step table's note on
  0x004d70f1), so his lasts exactly 13: a test that gives handyman 25 a bubble on mGameTick 1001 and steps the
  park to 1014 would show it still there. From the week-end audit (stage 6, the second reading of commit
  `738789d6`; read from the code, not reproduced in the game). `OpenTPW/World/Park/ParkPeople.cs`, line 2144.

- [ ] **Q283. `ParkParticles.Edit.Start` passes a directed start's `Direction` on to the linked effect when the
  link does not follow (`its = spawn`, then `Start( its with { Template = linked } )`).** So the linked emitter is
  written aimed and with no draw of the generator. `Particles_SpawnFull` starts its linked emitter with the plain
  `Particles_Spawn` (`0x00521cf2`), so the linked start must carry no direction; a test starting effect 51 (links
  54, not following) with a direction and holding emitter 54's +0x38 to the template's (0, 31, 0) and the seed
  three draws on would show it From the week-end audit (stage 6, the second reading of commit `d490bc90`; read
  from the code, not reproduced in the game). `OpenTPW.Files/Formats/Save/ParkParticles.cs`, line 376.

- [ ] **Q284. The console's `particles <slot>` reads a slot that is not a number as slot 0 and prints that
  emitter's cloud without saying so (`DebugConsole.cs`, `Argument( 1 )` falls back to 0).** It should reply that
  the slot was not understood, as it does for a slot out of range; `particles abc` in a park with an emitter in
  slot 0 shows it. From the week-end audit (stage 6, the second reading of commit `03326e26`; read from the code,
  not reproduced in the game). `OpenTPW/Client/Diagnostics/DebugConsole.cs`, line 1804.

- [ ] **Q285. Seven unbuilt paths the game reaches count nothing.** From the week-end audit (stage 6, the second
  reading of each commit; read from the code). `CLAUDE.md` rule 4 asks each for an `Unimplemented.Report`; build
  the path where it is small. One bullet a path.
  - A park loaded from a file with a camera module opens on the default view and counts nothing:
    `ParkWorld.Camera.Saved` is read from the file and used by no load, and `Level` reports only
    `PARK_ADVISOR_SAVED_HISTORY` beside it. The original's loader `FUN_0042cec0` puts the zoom, rotation and point
    back (`saves.md`, "Loading the modules", Camera), so until Q247 builds it the load should call
    `Unimplemented.Report` under a name of its own, shown by the `unimplemented` census after a Load Park.
    (`OpenTPW/World/Level.cs`, line 282; commit `2bdbd7fd`)
  - `Level.WritePark` writes a park file without writing `gms.dat` first and without putting the pointer back to
    its default mode, and counts neither: its summary names both as deviations and the body calls no
    `Unimplemented.Report` for them. The original does both on every save (`saves.md`, "The save", step 1,
    for `gms.dat`, and `FUN_00516c80` step 1 through `FUN_0046c350` for the pointer), so each should be
    counted where the file is written until Q248 and Q241j build them, shown by `savepark` followed by
    `unimplemented`. (`OpenTPW/World/Level.cs`, line 505; commit `2bdbd7fd`)
  - `Peep.Tick` leaves the cell's region-effects term out of every needs turn and counts nothing, though the park
    has kept the cells since Q257e (`ParkState.Effects`). The original adds the cell's happiness, illness and
    hunger words there (`FUN_00501650` through the getter `FUN_004d8410`), so until it is built the turn should
    call `Unimplemented.Report`, as rule 4 asks and Q157 already lists. (`OpenTPW/World/Park/Peep.cs`, line 716;
    commit `8b70a8d8`)
  - A handyman loaded from a park file in state 8 or 9 (the walk to litter and the sweep, FUN_004d73c0's cases 8
    and 9, set by FUN_004d7330's table at 0x004d73a4) takes `StaffBehaviour.Step`'s `default:` arm, stands for the
    rest of the session and is counted nowhere. It should at least call `Unimplemented.Report` and fall to idle so
    he decides again, until the litter job is built; Alexah's `jungle/New Save.TPWS` and `jungle/autosave.TPWS`
    each hold four such handymen, which a load and the `staff` census would show stuck at `st 0x8` and `st 0x9`.
    (`OpenTPW/World/Park/StaffBehaviour.cs`, line 579; commit `757c46a6`)
  - The name box of Save Park and of the new player dialog ignores Delete, Left, Right, Home and End and counts
    nothing (`WindowStack.Keyboard` hands a `UiEdit` typed text and Backspace alone, and the caret is always at
    the end). The original's box answers each in `FUN_00667a72` (`0x2e00`, `0x2500`, `0x2700`, `0x2400`, `0x2300`,
    Shift extending the selection), so either they are built or the keys are counted where they are dropped.
    (`OpenTPW/UI/WindowStack.cs`, line 745; commit `d2052d84`)
  - A load drops the cars, riders and two lists of a saved track ride whose BumperType has no record here (every
    type but the Hot Pot's -1) and counts none of them (`Open` counts the ride alone, as `BUMPER_TEMPLATE_n`, and
    only for the family's -3, -6, -11 and -14), where the build before Q257p counted each car chunk as
    SAVED_TRACK_RIDE_CARS. It should count each dropped car, rider and listed peep, since the loader's car arm
    (FUN_00543560, chunk 5, docs/exe/saves.md "OpenTPW's reader, a track ride's cars") restores a car for any ride
    its handle names; a test with a type -4 ride and one car chunk would show the census unmoved.
    (`OpenTPW/World/Park/ParkBumperCars.cs`, line 522; commit `a96db38d`)
  - A guest's judgement of the entry fee pushes no sample to the park analyser and counts nothing:
    `ParkAdmission.OpinionOf` returns its band and `PeepBehaviour.Judge` acts on it. The original's `FUN_004ff5b0`
    calls `FUN_004c74b0( 0, byte )` on each of the four compared arms (`0x004ff693`, `0x004ff6b5`, `0x004ff6f0`,
    `0x004ff715`; not on the early-out), and kind 0 is the one ring anything reads, the advisor's messages 91 and
    92 (`ride-operation.md`, "The analyser's samples"), so it should be built or call `Unimplemented.Report`.
    (`OpenTPW/World/Park/ParkAdmission.cs`, line 303; commit `542b67e4`)

- [ ] **Q286. Seven tests cannot fail for what their names say.** From the week-end audit (stage 6, the second
  reading of each commit). Each passes with the bug it is named for put back; strengthen each and prove it with
  `mutate.py`. One bullet a test.
  - ASavedStampIsKept reads a stamp of nought from the shipped park and then checks only that the Staff
    constructor copies a value handed to it, so the entertainer's read of mTimeStartedEntertaining at record +503
    can be moved and the suite stays green. It should write a park with a performing entertainer whose stamp is
    not nought and read it back, as ParkMechanicRepairTests does for the mechanic's +503, the layout being
    FileFormats saves.md's entertainer row (mTimeStartedEntertaining 4, mNext 2).
    (`OpenTPW.Tests/ParkEntertainerPerformanceTests.cs`, line 263; commit `63660f76`)
  - No test reaches a whole purchase, so the one line that stamps a bought toilet's, camera's or fireworks' region
    effect (`ParkBuilding.Build`, the `StampEffects` call after `AddObject`) can be deleted with the suite green;
    `ParkRegionEffectsTests.Bought` calls `StampEffects` itself. A test should buy a Security Camera through
    `ParkBuilding.Build`'s own path with objects that place, and read security 20 on its cell, which is what
    `0x004db458` does and what q257e's mutation run showed only the game run catches.
    (`OpenTPW.Tests/ParkRegionEffectsTests.cs`, line 143; commit `8b70a8d8`)
  - TheSavedStampIsReadAndKept asserts that no other kind of staff reads a researcher's stamp, but the shipped
    save holds nought at +503 for all four other members, so the assertion passes with the model guard in
    ParkWorld.ReadStaff removed (mutation m38 and m38b of q134/mutations.txt survived). It should read a record
    built with a non-zero +503 for a mechanic, a handyman, an entertainer and a guard (bytes patched in a copy of
    Easymode.TPWI, or a park written by ParkWorld.People's writer with an entertainer's stamp set) and assert
    nought, which m38 would then fail. (`OpenTPW.Tests/ParkResearcherResearchTests.cs`, line 109; commit
    `50516f7c`)
  - No test holds the range a bumper ride's engine voice is started with: taking `range: rides.RangeOf( effect, 0
    )` off `ParkCarSounds.Play` fails nothing (`q265/mut-run1.out`, m21 survives), and only the game run
    `q265/engine.py` showed it. A test should stand a track ride's script far enough to start the engine voice and
    read its range as the variation's own, 100 for the Hot Pot's (`docs/exe/audio.md`, "Measured here,
    2026-10-10"). (`OpenTPW/World/Park/ParkCarSounds.cs`, line 92; commit `a069af13`)
  - `TheBootsSoundIsPlayedAtItsVariationsVolume` asserts `UiSounds.BootVolume` alone, and `UiSounds.Boot`, which
    hands that volume to `SoundCategory.Play` for effect 210, is called by no test, so a `Boot` that plays at
    volume 1 or plays the click's effect passes the suite. `Boot` answers nothing and plays only once sound is up
    (`Audio.Ready`), so it wants a seam first and then a test through it (the voice played: its clip and its
    volume of nought), which is known by putting `1f` in the `Play` call at
    `UiSounds.cs:73` and seeing the suite stay green. (`OpenTPW.Tests/ParkCountedPathsTests.cs`, line 205; commit
    `da9443ff`)
  - The test `ARideBoughtFacingAPathKeepsItsLinkToItsQueue` builds its purchase by hand and calls
    `ParkBuilding.LayEnds` itself, so no test fails when `Build` stops calling `LayEnds` (`ParkBuilding.cs:164`)
    or calls it before `Stamp`. A test should drive `Build` (or `Buy`/`PlaceCarried` on a level) for a Crazy Ape
    on (41,22) and read cell (42,22) links `0x01` and a queue of one cell; putting the bug back at line 164 shows
    the gap. (`OpenTPW.Tests/ParkFileWriterBoughtAndSoldTests.cs`, line 647; commit `2dec6b66`)
  - The test `AThingsVoiceIsSoundedAtTheMiddleOfItsCells` checks `ParkObjects.BoxMiddle` alone, so putting the
    scream back at the thing's corner in `TrySoundPlace`, or not keeping the footprint in `Place`, fails no test
    (q266/mutations.py m08 to m10 fail none). A test should stand a thing in a `ParkObjects` and read
    `TrySoundPlace` for it, the Belly Bounce at (51,23) answering (525, 250), so the wiring is held by the suite
    and not by one game run. (`OpenTPW.Tests/ParkFootprintTests.cs`, line 141; commit `9ec9a7a9`)

- [ ] **Q287. ParkHireScreen.Selected previews a highlighted candidate's wage with ShowMoney( person.Wage ), and
  Update() calls ShowMoney() with no wage on every frame (WindowStack.OnUpdate).** So the staff-costs preview is
  overwritten a frame later and the player never sees it. ShowMoney also never takes the previewed wage off the
  balance cell, which its own parameter remark says the original does (message 0x401); known by reading lines
  351-417 and WindowStack.cs:330, not run, and no test covers the cell From the week-end audit (stage 6, a sweep
  of the source settled by a verifier; read from the code, not seen in the game).
  `OpenTPW/UI/Park/ParkHireScreen.cs`, line 417. Confirm: a candidate highlighted on the hire screen, the
  staff-costs figure photographed.

- [ ] **Q288. Names and log lines that say what the code does not.** From the week-end audit (stage 6, a sweep of
  the source settled by a verifier). The audit changes no code, so each is left as it is: rename or reword, one
  bullet each. No game run.
  - ModelFile.Mesh.VertCnt is assigned the mesh's material count (the ushort at mesh +0x5A), while the real vertex
    count is in VertexCount, and nothing in the source reads VertCnt. Rename it MaterialCount or label it dead by
    CODE; known from ModelFile.cs lines 197, 414 and 490 and a grep of source/.
    (`OpenTPW.Files/Formats/Model/ModelFile.cs`, line 197)
  - ParkParticles.OnScreenAt (0xc1) names the template byte Particles_Spawn tests at 0x00521e98 to answer nought
    while the header's +0x08 is set; it is not the on-screen flag, which is +0x05, and ParticleLibraryFile already
    calls the same byte Suppressible. Tp2.plb sets it on 93 of 101 named effects, world and screen alike, so
    rename it to match (and OnScreenOnlyAt, the header's +0x08 the same test reads, with it).
    (`OpenTPW.Files/Formats/Save/ParkParticles.cs`, line 76)
  - ParkParticles.HoldsParticlesAt (+0x05) is the emitter's drawn-on-the-screen byte, which Start reads only to
    give a world emitter an empty box and a screen one six noughts. The name says something else (particles are
    held at +0x06/+0x08); FileFormats particles.md 0x05 and ParticleLibraryFile.OnScreen name the byte, so rename
    it. (`OpenTPW.Files/Formats/Save/ParkParticles.cs`, line 52)
  - SaveReader.ReadFile throws 'File version is not 133.' when the byte at 0x608 is not 0x85, and that byte is the
    last of the big-endian magic 0x01221985 at 0x605, not a version. The file's own comment above the read and the
    FileFormats saves page (header table, 0x605) both say so, so a bad file is reported under the wrong label and
    three of the magic's four bytes are never checked. (`OpenTPW.Files/Formats/Save/SaveReader.cs`, line 94)
  - SaveReader.ReadFile names the dword at 0x604 `fileType` and the byte at 0x608 `fileVersion`, and never uses
    the first. The comment above them and the FileFormats saves page say they are the zero byte with the magic's
    first three bytes and the magic's last byte, so the names contradict the file's own account; one piece of work
    with the bullet above (read the magic whole at 0x605 and check it, or rename).
    (`OpenTPW.Files/Formats/Save/SaveReader.cs`, line 90)
  - PutPeople throws 'thing N, an object bought, is in the file or the park already' for four conditions, one of
    which is a made record whose length is not the object record's size. That is read from the one `if` at
    ParkWorld.People.cs 400-401; the length test wants its own message (MadeObjectRecord always makes the right
    length, so it is a guard that cannot name itself when it fires).
    (`OpenTPW.Files/Formats/Save/ParkWorld.People.cs`, line 402)
  - SaveFolder.LoadConfig logs "Saves: read the options from <path>" after ConfigFile.Read returns null for a
    version-0 Config.tcf, when nothing was applied and the defaults stand, and equally when a file of an unknown
    version was applied but left _unreadConfig set. In both cases a later SaveConfig refuses to write and the
    load's log never says why; known from SaveFolder.cs:57-75 and ConfigFile.cs:61-66, by code only.
    (`OpenTPW/Client/SaveFolder.cs`, line 75)
  - SaveFolder.DeletePlayer logs "Saves: deleted the player '<name>' in slot N" whether the folder was absent or
    DeleteFolder left it behind because it held a dot-named entry, which its own doc comment says happens. Known
    from SaveFolder.cs:477-480 and 509-530, by code only; the log should say when the folder was left and what was
    in it. (`OpenTPW/Client/SaveFolder.cs`, line 480)
  - The console's `bumpers` reply heads its report with `bumpers N`, where N is `report.Count`: one line per
    bumper ride plus one per live car (DebugConsole.cs:807, :818, :833). The label reads as a count of rides and
    is not one; known by reading the code, not run. (`OpenTPW/Client/Diagnostics/DebugConsole.cs`, line 833)
  - The debug console's camcorder reply (ParkCamcorderCameraMode.State) prints `eye=` as the raw ground under the
    stand plus EyeHeight, while the camera is placed at the eased _groundHeight plus EyeHeight. The label names a
    height the eye is not at while the ease catches up or the clock is paused, known from reading State() against
    Place( GroundUnderStand() ), not from a run. (`OpenTPW/World/Park/ParkCamcorderCameraMode.cs`, line 830)
  - SoundCategoryFile.Variation.GapByParameter holds the first controller's whole mask (the u16 at +0x18: 1
    volume, 2 pitch, 4 the wait), and ParkAudio.Controlled, ParkAudio.NamesOnVolume and ParkCarSounds test its
    volume and pitch bits beside SecondMask, so the name describes one bit of three. This is known from
    SoundCategoryFile.cs's own FirstKey parameter note and from 0x006c3d8a, which tests the word against a passed
    bit; a rename (to pair with SecondMask) touches ParkAudio, ParkCarSounds, ParkScreams and four test files.
    (`OpenTPW/World/Park/ParkAudio.cs`)
  - NodesFor's catch branch warns that a thing "walks or seats riders ... every leg is the shortest and no head is
    hung" for any script whose model will not read, including one that reaches it only through UsesEffectNodes
    (ADDOBJ), which walks nobody and hangs no head. That is known from the code: the early return lets an
    ADDOBJ-only script through, and the null-model branch below guards the same message with `WalkSlots != 0 ||
    UsesHeads` while the catch does not. (`OpenTPW/World/Park/ParkRides.cs`)
  - PaceLocal (local 16, the instance's +0xc4) and PaceValue (0x1200) hold the draw's flags word, which the
    DrawFlags property returns and line 508 writes from a parameter called flags. The names say pace, which
    nothing in the file decodes or uses; this is known from the constants' own comments and the three uses at
    lines 321, 373, 468 and 508, so they should be renamed DrawFlagsLocal and DrawFlagsValue.
    (`OpenTPW/World/Park/SpriteScript.cs`)

- [ ] **Q289. Two members only tests call, which a built path needs.** From the week-end audit (stage 6, the
  dead-code census). Neither is dead by code: placed content reaches the path each belongs to, and the path is
  unbuilt.
  - The constant is read only by PeepSteeringTests:29 (whose message is "a tenth, for crowding - not built"); no
    production code applies it and no other 0x1999 literal exists. Its doc and PeepWalk.cs:36-42 say the original
    registers a third steering behaviour, separation, at FUN_0050fdf0 and that leaving it out "makes crowds walk
    through one another" - every walking peep in a placed park reaches that path, so the unbuilt thing is a GAP,
    not a dead member. Not checked in Ghidra here. Two things the finder may want: (1) both comments give as the
    reason that the project keeps no per-cell lists of who stands where, but ParkState now keeps exactly that
    (_onCellNext, ParkState.cs:1294-1369, "the original's thing +10"), so the stated reason looks stale; (2) grep
    of docs/QUEUE.md and docs/PLAYER-GAPS.md (held copies) finds no line for separation or crowding.
    (`OpenTPW/World/Park/PeepSteering.cs`, line 47, `SeparationWeight`)
  - Called only by RideStateTests:85 and RideScriptRunTests:280, and it is the only writer of _finished. The
    reader is live: RideScript.cs:2291 stores Ride.TakeRider() for COAST 3. ParkRideOperation.cs:335-338 says so
    at the site: "COAST 3 is implemented and its coasters still never dismiss anybody, because it drains the
    finished-rider queue that nothing fills ... a missing simulation, not a missing opcode", and
    docs/exe/ride-operation.md:4263 lists COAST 3 as how Coaster1, Coaster3 and Minecart dismiss - all three are
    Lost Kingdom rides (levels/jungle/rides in the .RSE corpus). So content reaches the path and the filler is
    unbuilt. Unproven: I did not check whether Easymode.TPWI places one of the three or whether they are only
    buyable. FinishRider's own doc calls it "the seam where a ride simulation will connect" but does not say only
    tests call it. (`OpenTPW/World/Ride/RideState.cs`, line 138, `FinishRider`)
  - **Note (fork review 2, 2026-10-16, `verify-coasters-run.md` L16):** the filler is the coaster's station step
    `0x0043b8b0`. The ring COAST 3 drains has three writers: `0x0043b480` unloads a train's riders as it arrives in
    the station (from the station step, from the state setter `0x00435730` on 4 or 8 to 2, and from `0x0043ba20`),
    only in state 4 or 8 with mode `0x40` clear; `0x0043b390` moves the admission queue across when the ride closes;
    `0x0043b130` refills it from a loader. Seating draws the game's own generator at `DAT_007b0958`; a rider the full
    ring cannot take is taken off the car and queued nowhere. The rest of the coaster's running is Q405.

- [ ] **Q290. The queue's stamp is taken before the queue is measured, where the original measures first.** From
  the week-end audit (stage 8, the older decode read a second time from the listing; verified once, not reproduced
  in the game). `ParkState` takes `NextCounter()` before `InvalidateQueue` and the back-cell lookup, and its
  comment says so, where the executable measures the queue first and takes the counter after (`ride-operation.md`,
  the row for the queue's remeasure). `OpenTPW/World/Park/ParkState.cs`, line 1688. Confirm: the back cell's stamp
  read in both games after one queue edit.

- [ ] **Q398. Each ground cell is split on its own diagonal.** From the fork review 2, 2026-10-16 (`verify-formats.md`
  H1, checked again in Ghidra by the main session). `ParkGround` splits every cell from (x, y) to (x + 1, y + 1) and
  says so at the site; the original draws each cell by its flag `0x2` and takes heights by `0x4`, the same split
  (`park-engine.md`, "The triangle diagonal"; FileFormats `models.md`, "A cell's diagonal"). In Lost Kingdom 2,708 of
  the 7,001 ground cells take the other diagonal. Fix the mesh and this game's height under a point (the camera's, a
  draped model's, and `FUN_004527f0`'s users, which split only the cells the load marks `0x800`). Confirm: a slope
  cell carrying `0x4` shot beside the original's, the same camera.

- [ ] **Q399. Ride boards lettered as the original letters them.** From the fork review 2, 2026-10-16
  (`verify-objects-signs-other.md` H1, H3, M1). `ParkObjects.BuildSign` letters the `.sam`'s `Name`, split; the
  original letters the item's `OBJECT_NAMES` rows (FileFormats `sgn.md`): in English Lost Kingdom only Dino Karts
  differs, "DinoKarts" on one line. `SignFile.cs` reads neither the width percent (`+0x144`, not 100 on 16 of Lost
  Kingdom's 17 boards) nor the line's y (`+0x148`); the original fits a line by a binary search on `LOGFONT.lfWidth`
  (`0x005eab70`). The ink block's four unknown fields are the grow or shrink radius, the blur's sigma, dx and dy (a
  Gaussian of 3 + trunc( 2.5σ ) taps). Confirm: Dino Karts' board and one other beside the original's.

- [ ] **Q400. Animated textures.** From the fork review 2, 2026-10-16 (`verify-formats.md` H2). 19 texture slots with
  several frames in 9 jungle wads (the queue fire, the 5x5 rock's waterfall, the monkey's face, `tvsim`, the hyenas,
  the puzzle, `squark`) and 49 jungle clips with frame tracks; nothing here shows a frame past the first. The layout
  is FileFormats `models.md` (the frame count at `+0x0A`, "Texture frame tracks"). The engine: sampler `FUN_00470c90`
  from the clip update `FUN_004721f0` and the channel advance `FUN_004735d0`, when the block's flag `0x2` is set and
  `[0x007b4980] & 8` clear; loader `FUN_00575c60`, `FUN_00575e10`; the draw reads the frame at `0x0057a47e`,
  `0x005805ed`, `0x005818c5` and `0x00581e48` while `DAT_008bd550 & 0x40`, which comes from `DAT_00785884`, very
  likely `GraphicalOptions.ANIMATINGTEXTURES` (inferred from the order in `FUN_0044d8f0`: read it first). Confirm: the
  queue fire burning beside the original's.

- [ ] **Q401. Self-lit materials.** From the fork review 2, 2026-10-16 (`verify-formats.md` M3). A material whose word
  `+0xC` carries `0x1` (tested at `0x00580c51`) gives every vertex the colour mask itself, white in play, in place of
  its lit colour: 929 of 12,960 materials, on signs, water, fire and lava. Nothing here reads it. Confirm: a sign and
  the waterfall shot beside the original's.

- [ ] **Q402. A refused purchase makes the original's error sound.** From the fork review 2, 2026-10-16
  (`verify-audio-lips-video.md` lead 5). The original plays `cat_ui` `0x1D` when the buy screen refuses a row
  (`FUN_004ac270` at `0x004ac7f6`: a mystery row short of tickets, a row dearer than the balance), when a Ctrl+click
  Clone is dearer than the balance (`FUN_004879d0`, `0x00487b46`), and from `FUN_004af480` (`0x004af5d6`, a control of
  window stream `0x00755750` not identified). This game refuses a row silently and counts nothing, and Clone has
  nothing behind it. Confirm: the sound caught by the disk driver on a refused row.

- [ ] **Q403. The balance's colour and the cost in hand.** From the fork review 2, 2026-10-16 (`verify-ui-hud.md` L5).
  The HUD letters the balance in one of six colours (table `0x00752e30`, index (balance + 999) / 1000 clamped to 0..5:
  red at nought or below, white over 4000), and control `0x30` shows the cost of the item in hand, only for mode types
  3, 4 and 8 with `[0x0079c658] & 1` (-9999 hides it, -999 resets it). The keys counter is skipped in Instant Action,
  the tickets counter runs in every mode. Read what this game does first. Confirm: the balance at 500 and 5000 beside
  the original.

- [ ] **Q419. The hire screen's candidate box is empty.** From the run review, 2026-10-16. On the same file the
  original's hire screen shows the chosen candidate's figure where this game's box is black (`q412/run-fix2/3-screen.png`
  beside `q412/orig/a-button.png`; the same in `q415`). The candidates' names differ too, which may be the pool's
  top-up (Q356's note); not looked into. Decode what the original draws there first. Confirm: the box beside the
  original's on the same file, a screenshot and the census of what is drawn.
- [ ] **Q404. A park six months in the red ends.** From the fork review 2, 2026-10-16
  (`verify-economy-guests-paths.md` L4, M2). Counted, `BANK_PARK_ENDS_IN_THE_RED`. What the end does is
  `ride-operation.md`, "The end of the park": world state 4, the end screen, `VAR_BREAKSTAT` 1 on every ride guests
  choose, the pending deletes flushed, the gate closed; the clock and the bank go on. The path tests no game type, so
  Lost Kingdom's Easymode reaches it. Confirm: a park held in the red to its end, beside the original.

## B. Docs and comments

- [ ] **Q14. Comment sweep of the cleanup commits.** Replace history-voice comments with what the code does now,
  found by member because line numbers go stale. Left: `AudioListener.AttenuationTo`'s summary nests a `<para>`
  inside a `<para>`, and `ParkCamcorderCameraMode.DebugWalk` steps a per-frame `WalkSpeed / 60f`; make it use
  `Time.Delta` (rule 10). No game run.
- [ ] **Q81. The FileFormats docs still disagree with themselves and with the game.** From the clone's 2026-09-18
  audit, as the 2026-09-24 staleness audit found it (`docs/history/fileformats-docs-ledger.md`). Most of it is fixed,
  and every branch that held a fix is merged into the clone's master, so no page exists in two versions and the
  `.md2` format has one page (`models.md`). Still wrong: `saves.md`'s `mFlags` `0x8` bit, its "version 85" (hex), and "the sixth
  person model is the visitor"; and `wad.md`'s name length, which includes the NUL. Open on both sides: what `0x14E`
  means (`SpriteBankFile`'s note). The Bumper enum is not a page error: the page's 1-17 is probably right, and it is
  `ScriptDefs.cs`, a labelled leftover left as it is, that looks corrupt. Fix each on one short
  branch off the clone's master (`docs/README.md`, "The FileFormats docs clone"). No game run; the
  site builds.
  Read by the week-end audit (2026-10-11), not re-measured: `wad.md`'s name length and the `mFlags` `0x8` bit read
  corrected. Still to judge: `saves.md`'s version `85`, its "sixth person model is the visitor", `0x14E`, and
  `vm/info.md`'s per-script variable ids.
- [ ] **Q75. The 2026-09-12 review's Phase F, what is left.** Opportunistic, never alone. Left: `SixLabors.ImageSharp` 3.1.6 in
  `OpenTPW.ModKit.csproj`; `content/textures/test.png`; `.editorconfig`'s `end_of_line = crlf` with no
  `.gitattributes`; and, to LABEL as dead by CODE rather than delete (rule 3), `Public/ModelFile.cs` and
  `Public/MapFile.cs` (both unreferenced), `Render/Primitives/Cube.cs` and `ReadUIntN`. `Singleton.cs` is used by
  `UI/Cursor.cs`, though nothing reads its `Instance`. Left to Alexah's call, not to be started: `.gitignore` for `.claude/`,
  `.vscode/` and `.mcp.json`, ModKit as a whole, and the nullable warnings except in passing.
  Also left (the 2026-09-26 staleness audit): the `Zio` 0.17.0 package in `OpenTPW.Common.csproj`, which no source
  file uses and which carries a low advisory (`DECISIONS.md`, "Package advisories"), and the `PreferredBackend`
  setting in `App.config` and `Settings.settings`, which nothing reads (`Settings.Default` is read for `GamePath` and
  `GameWindowSize` only). Label both as dead by CODE at their sites; remove either only on Alexah's word.
- [ ] **Q161. Stale text in code strings, test names and one census key.** Found by the 2026-09-26 staleness audit,
  whose edits were to comments only. `Extensions.GetColor`'s `[Obsolete( "Use Color.Parse" )]` names a method that
  does not exist, and nothing calls `GetColor` (dead by CODE: label it, rule 3). `RideEffects.Trigger` reports
  `effect type {type} (TRIGGEREVENT)`; the instruction is `EVENT` (`Opcode`), beside `(ADDOBJ)`. The console's `load`
  replies "they come one a tick"; `StepArrivals` admits one a thing sweep. Assert messages: `AnimationEasingTests`
  ("an older note here quoted"), `ParkRideJoinTests` ("only 13 and 14 can be"; six pass the filter),
  `ParkStateTests` ("the handyman" of thing
  26, the mechanic), `ParkTickTests` ("neither of which this tree had"), `ParkPathBuildingTests` ("the loader's
  runtime flag is a different thing", which `park-engine.md` calls not established), `PeepWalkTests` ("at 31ms a
  tick"; a step is a thing sweep), `RideAnimationsTests` ("this branch"), `PeepNeedsTests` ("nothing else makes a
  guest hurry"; `HurriesToTheGate`), `RideScriptModelTests` ("where the advance sat"), `PeepNavigatorTests` ("the
  longest route in the shipped park", true of the save only) and `SoundCategoryTests` ("happened to work here"). Test
  names: `AnimationUvTests.ATwoKeyTrackReadsAsItAlwaysDid`,
  `BalanceFieldTests.AKeyThatNamesOneFieldIsReadExactlyAsBefore`,
  `GameCalendarTests.TheDaysKeepTheOriginalsSlightlyFastPace` (it asserts only a count of 231),
  `ParkEvictionTests.EveryQueuerIsPutOutOfTheQueueAndLosesBothChanges` (its summary: the drain has already put out all
  from the fifth place back) and `ParkWeatherTests.TheWeatherTurnsEverySevenDaysAndIsDecidedThreeDaysBefore` (four
  days, `weather.md`). `ParkPathTests`' summary says "SEVEN tests fail" without the `ReferenceEquals` guard, measured
  when sixteen classes built an edge test; twenty do now: take the guard out again in a worktree and write the count
  taken, or drop it. No game run.
- [ ] **Q204. Load the area-only notes only in their area.** Claude Code loads a `.claude/rules/*.md` file whose
  `paths:` front matter matches an open file, and no other time. Move the sections of `CLAUDE.local.md` (and any of
  `CLAUDE.md`) that serve one area only - the capture recipe, the original under Proton, the 2.0 patch's evidence - into
  such files, keyed to the source and harness paths they serve, so a session that never opens them never loads them.
  Measure both files' size before and after, and confirm in a fresh session that a matching file brings its rule in
  and a non-matching one does not. Borrowed from Rootstock (github.com/Mazhron/rootstock-os); nothing else of it is
  taken. Alexah's yes 2026-10-01, for after the weekly reset. No game run.

- [ ] **Q292. Cut `docs/VERIFYING.md` to its rules.** At Alexah's word (2026-10-11). The page is 770 lines, each
  rule with the story of the mistake behind it; `MEMORY-DIET.md` set it 85. Each rule keeps its number and one
  line of why, and its story moves, as written, to `docs/history/`. No rule is dropped or renumbered: every page
  and note that cites a rule by number must still find it (grep `rule [0-9]` first). No game run.

- [ ] **Q293. Three `FUN_` labels Ghidra has not disassembled, and nineteen names the docs do not use.** From the
  week-end audit (stage 1). `ride-operation.md`'s rows on the loans name `FUN_004d0970`, `FUN_004d0a00` and
  `FUN_004d0810`, which are undefined bytes in the Ghidra project, so nobody has read them there: disassemble at
  each, read what it does, and correct the rows. And nineteen functions the docs call `FUN_` have a name in the
  project (the harness folder's `audit-2026-10-10/findings/s1-ghidra-named.tsv`): write the names in the docs.
  Nothing in Ghidra is named or made a function without Alexah's word. No game run.

- [ ] **Q294. The FileFormats pages changed since 2026-09-26, against the game's files.** Left by the week-end
  audit's stage 8. 1,557 layout statements on 22 pages, `saves.md` and `models.md` most of them, packed into 56
  areas (the harness folder's `audit-2026-10-10/ev/s8a-00.md` to `s8a-55.md`). One `tpw-verify` an area checks
  each against every file of its kind with the readers (`lib/parkfile.py`, `lib/sfxmap.py`, `lib/md2surf.py`,
  `wadcat`, the `.RSE` corpus); then the site is built. No game run.

- [ ] **Q296. Three statements Q291's verifiers could not settle.** Each is said as fact with no measurement
  beside it. `park-engine.md`, the object window's preview: "each with one clip in role M" (the Space zone's
  `megacost.wad` carries `Pmegacostm.MD2` and `PMEGACOSTM1.MD2` both; read which the loader takes). The same
  page, the footprint picture: "the lowest row of pixels stays clear" (rests on whether `FUN_005f9a80`'s
  rectangle takes its far edges: read the fill behind `[0x00fa76d0]`'s slot `+0xc`, or measure a frame of the
  original). `park.md`: "`WALKGET` returns only a rider `WALKOFF` let off (`FUN_00557110`)" (`FUN_00557110`
  answers a slot in state 4 and `WALKOFF` writes 3: trace the walk update's writes at `0x00557e6c` and
  `0x00558018`). Confirm: each settled from the listing, the page saying what it shows. No game run.

## C. Alexah's list: cause known, one session each

- [ ] **Q15. Green line between path cells and on ride signs.** Path textures get the default sampler,
  `AnisotropicWrap` = Wrap (`ParkPaths.Build`, through `Material.Set`'s array form). Sign halves are bound the same way
  by `LobbyModel` and wrap or clamp by their frame's bits (`SignTexture.TryBuild`). The original clamps every texture
  whose frame carries no wrap bit (`render-states.md`, "Texture addressing"). With wrap plus mipmaps, a tile's edge
  samples the far edge. Use clamp where the original does: the path tiles' and the ground's textures are not models,
  so read first what the original binds for them. Confirm: screenshot two paths in a column, a crossroads and a ride sign,
  before and after.
  A model's texture clamps unless its frame wraps (Q363). `ParkPaths` and `ParkGround` bind an array, which
  `Material.Set` samples as Wrap whatever the flag; `Texture.SamplerFor`'s Mirror is used only for a texture bound
  alone; the sea keeps its wrap.
- [ ] **Q17. Camcorder mode can strafe.** `ParkCamcorderCameraMode.Walk` passes
  `Input.Right` into `Step`. `park-engine.md`, "Walking on the ground is swept against the cell edges", decodes the sweep by two velocities but not the
  keys. Remove the strafe. Confirm: press the strafe key, the stand position in the `camcorder` census
  does not move sideways, screenshot.
- [ ] **Q18. The advisor draws in front of the dimmed screen.** `Level.Render` draws the HUD, then
  the overlay pass with the advisor. Pausing does not hide him either; only the options screen's quiet stop takes him off (`ui.md`, "Pausing him"). Draw the
  dimmer over him, or hide him while such a window is open. Confirm: screenshot with the buy screen open.
- [ ] **Q19. VSync and a frame limiter.** `Renderer.CreateGraphicsDevice` hard-codes VSync on. `Display.cs` already
  carries each mode's refresh rate. Add an Options row: VSync mode, and a frame limit from 30 up to
  Unlimited, default the monitor's refresh rate on first launch. Confirm: screenshot the options row;
  log the measured frame time at two settings.
- [ ] **Q20b. No bubbles on the drinks shop, no chimney smoke on the staff room.** The scripts ask for the effects and
  `RideEffects` records the request; nothing consumes the records. Join them to `ParticleSystem.Spawn` through Q20a's
  pass (`ParLib.cs` names `Bubbles = 58`, `Smoke = 2`, `SmallSmoke = 13`; `park.md` decodes the node and the per-tick
  push). Confirm: screenshot the drinks shop with bubbles and the staff room with a staff member inside and smoke
  rising; log lines for both spawns.
  - **Note (fork review, 2026-09-30):** part of what Lost Kingdom's effects need comes from the save, not from new script
    requests. The `PART` module (written by `FUN_0051f680`, read by `FUN_0051f7a0` at `0x00415437`) restores the live
    emitters, and `Particles_KillAllOnScreen` (`0x005200b0`) then kills every screen-drawn one, so after loading
    `Easymode.TPWI` two world emitters live on: WaterFall at slot 0 and Bubbles at slot 20. A saved script's `ADDOBJ`
    handle points at them through the `RSSE` module's `OBJ ` records, which `ParkScriptStates` skips today: seed the
    emitters only together with those records, or `Coconut.RSE`'s `KILLOBJ 1` can never stop the bubbles. The theme's
    `.emt` files go into the first empty template slots, 101 and 102 in jungle, never past 104. Layout: Q193. Review
    items fmt-assets-v1, gap3-6, gap5-1..gap5-4.
  Read by the week-end audit (2026-10-11): the bubbles and the seeding with the OBJ records are done (Q257i,
  Q258). What is left is the staff room's chimney smoke, which has no trace in the tree.
- [ ] **Q21. Entering a park: hide the front end for the fly-in.** `IslandPanel.EnterPark` closes only the
  island panel, and the rest of the front end stays up while the camera flies in. Hide it for the flight, and
  say what Escape's cancel (`FrontEnd.MenuKey`, `LobbyCameraMode.CancelLeave`) brings back - today only the
  island panel. The gate half landed with Q41: the flight plays M1 as the homing ends and M2 on a cancel
  (`docs/exe/lobby.md`, "Escape cancels the fly-in"), so do not re-pace it. Confirm: screenshot burst from the
  click to the loading screen.

- [ ] **Q295. An object window's preview starts from the angle the last one left.** From Q291. The first angle is
  `fmod( [0x007afd50], 6.2831855 )` (`0x00468e2f`; `park-engine.md`, the object window's preview): the record is
  static and its angle nought in the image, so only the first panel of a run starts from nought. The preview here
  starts every one from nought and counts it (`OBJECT_PREVIEW_START_ANGLE`, `ParkObjectPreview`). Build: keep the
  last panel's angle for the session and start the next from it. Confirm: two ride windows opened one after the
  other in both games, the second's first frame photographed, and the angle in the log.

## D. Alexah's list: decode first, then build (two sessions each)

The decode session writes the finding to `docs/exe/` and stops. The build is the next session.

- [ ] **Q22. Riders sit still on the Belly Bounce.** Seat positions are read once from the model's rest
  pose (the `LobbyModel` constructor) and never from the animated pose. A riding peep's sprite is `None`
  (`Peep.AnimationFor`). Decode: which frame a rider shows, and how the original re-resolves the seat
  node each frame (`ride-operation.md`, "Where a rider is drawn", and `FUN_005580a0` under "The WALK family").
  Then build both. Q175 re-read the lookup by id and flag (`audio.md`, "Node lookup is by id AND a capability flag")
  and decoded where a node's position comes from (the posed matrix, refreshed on screen; `ride-operation.md`, "How
  long a leg lasts, and where its ends are"), and `FUN_005580a0`'s interpolation; which frame a rider shows is still
  open. The engine finds a bounce rider's node by id in `0x800` ("Where a rider is drawn"); `ModelFile.FindNode` is
  that lookup and `RideNodes` the stored positions at rest (Q175b), not the animated pose a rider needs.
- [ ] **Q23. Camera rotation snaps by 45 degrees.** `ParkOrbitCameraMode.Update`, the two rotate keys. The 90-degree
  option exists (`GameOptions.NinetyDegreeRotation`, set on the Options screen and kept in the player file) and the camera does not read it. Decode the original's step
  and its easing (`park-engine.md`, "The park camera", has the saved and required rotation, not the rate). Build what
  the decode says, driven by `Time.SmoothingFactor`. Alexah: match the original, do not invent.
  From Q241f: with the options byte `0x0078d912` set the original's update turns a quarter turn a press
  (`0x0042b489`, `0x0042b4a4`) and its loader rounds a saved rotation to a quarter (`saves.md`, "Loading the
  modules", Camera), so a park saved here at an eighth turn opens a quarter away there. Seen beside the original
  at the same zoom and point (`q241f/sheet-ours-beside-original.png`): OpenTPW's view takes in far more ground.
  From Q440 (`park-engine.md`, "The turn"): held, the Left arrow and keypad 9 raise the rotation 0.002 a millisecond
  and the Right arrow and keypad 7 lower it, and the drawn rotation eases after it by min(dt, 140) / 140 a frame; the
  direction is already the original's (this game's yaw is its rotation negated, and the Left arrow,
  RotateLeft, lowers the yaw). The step and the easing are what differ.
- [ ] **Q24. Nothing highlights a thing under the mouse.** The idle pointer already follows the hover category over
  ground and path (`Level.IdleOverPath`). Picking is decoded (`park-engine.md`, "Picking is a real ray cast, not a
  grid lookup"): the hit point comes from a ray against the terrain and then object meshes, and the hovered THING is
  taken from the cell, not by hitting its model (`ParkPicking.ThingUnderCursor`). Decode what the hover updater
  `FUN_00486d90` highlights when the hovered thing is an object or a person, then build it.
- [ ] **Q25. The camcorder button should give a crosshair and place the camera where you click.**
  `ParkGadget.EnterCamcorder` enters the mode at once. The original installs a mouse-interaction mode
  (`FUN_00481a10`, `park-engine.md`, "Camcorder mode — the first-person view"). Its click handler was decoded by Q48
  (`park-engine.md`, "Entering and leaving first person"): cursor `0x13`, a left click on a cell of type 0, 1, 3, 9 or
  30 stands the viewer at the picked point, and leaving puts the saved point of interest and yaw back, where ours keeps
  the walk. It is also Q48's hole (3). Left to decode: the click's own cell tests and its thing branch. Then build.
  From Q121: measured in the original, one forward term (the Up arrow, or a held right button) walks about 21.8 units a
  second at zoom word 110, both together twice that; `WalkSpeed` is a chosen 40. The velocity is not decoded to a speed
  (`hud.md`, "Four ways out of camcorder mode"): decode it with this, then set the speed. The original's walk keys
  are the zoom's, Up and Down (bits `0x20` and `0x40` of the button word, measured); here W and S walk and the arrows
  do not.
- [ ] **Q26c. Ferry, seaplane and bus are always there.** `ParkFixedItems.Items` stands all three
  permanently, and `VehicleFor` gives a load under 36 the bus, so only the bus is called in the stock park (its
  loads are 13 and fewer since Q26b; three new rides make 30).
  The original creates the vehicle on demand (`FUN_0051a2f0`,
  `park.md`, "Arrivals: who comes, on what, and how often").
  From Q238: the original hands a new thing the id last freed (its load's first guest took 38, a leaver's, the
  rest 43 on); `ParkState.NextThingId` never uses one twice, said at the site, so a load's ids part from the
  original's once anybody has gone.
  Build create-on-demand and the bus / ferry / plane ordering; the pause between loads is built (Q68b).
  From Q342: the THING is made on demand now, an object on cell (0,0) the writer writes (`saves.md`,
  "OpenTPW's writer, a vehicle made at its summons"); the model and the script still stand from the load, and
  the id is not the original's.
- [ ] **Q27. Pushing the mouse at the screen edge does not scroll.** The "push scroll" option exists and
  is read by nothing. `ParkOrbitCameraMode.Update` scrolls from keys only. Decode the camera
  binding table at `0x00748158` (`park-engine.md`, "The park camera"), then build.
- [ ] **Q28. Renaming parks and rides.** The save carries a name per park (`saves.md`, the `gms.dat` theme record's park name). No rename
  control exists; signs read fixed names (`ParkFixedItems.BuildSign`, `ParkObjects.BuildSign`,
  `LobbyIsland.DisplayNames`). Decode where the original edits the name and what it writes, then build the
  control and make the three sign painters read the save's name.
  - **Note (fork review 2, 2026-10-16, `verify-objects-signs-other.md` H2):** `0x004990f0` opens the box (two
    14-character edits, title UITEXT `0xc2`); its OK, `0x004993e0`, sets the gate thing's two lines (`0x004e2950`,
    `0x004e2a50`, 33 wide characters each), repaints the sign (`0x004682b0`, the hardware path only), writes the
    player's `gms.dat` theme record (`0x005b0940`, `0x005b0970`: `mSignNameA`, `mSignNameB`, `mNameChanged`), and
    records the rename as action-recorder entries `0x50` and `0x55` (`0x00403fd0`, replayed by `0x00404f70`), which is
    how it outlives a save. Lines reading "Kevin the" and "Cosmic Cow" turn the cheats on (`0x004053c0( 1 )`). A ride
    board takes its name from `OBJECT_NAMES` until a load (Q399).
- [ ] **Q29. Parity with the original's hardware renderer.** `render-states.md`, "Texture addressing", counts the
  MIN/MAG/MIPFILTER and LOD-bias sites but does not decode their values; no shade-mode row exists; ZFUNC
  is LESSEQUAL there and Less here. The renderer lights per pixel with 16x anisotropic filtering and
  full mipmaps (`test.shader`'s fragment `main`, `Material.CreateSampler`). Decode the values, then match them. Confirm:
  side-by-side screenshots of the same view.
- [ ] **Q30. Waving flags at the bus stop.** Nothing found in code or docs. Decode what the original
  draws at `BusStopA/B` (`park.md:54, 98`), then build.
- [ ] **Q37. The placement terrain rule.** `ParkBuilding.Refusal` counts `PLACEMENT_TERRAIN_RULE` and lets a
  thing stand on types 2, 7 and 30 and on land flagged `0x40`, outside the park. The original allows a
  footprint only over in-park bare ground or path (`FUN_00535600`, `FUN_00535670` refusing `0x40` at
  `0x005357c7`); whether every placement route goes through that verdict (its op-`0x104` branch) is
  still open. When it is built, `Unstamp`'s two declared deviations go: keeping `0x40` where the original
  clears the whole flag word, and giving terrain back its save's record rather than clearing it.
  - **Note (fork review, 2026-09-30):** the `.hmp` adds nothing here: its mark plane equals `Info.Shape` in all 274 pictured items
    (Q191).
- [ ] **Q38. What building and selling look and sound like.** Counted, not built: the death particle
  (`DESTROY_PARTICLE_EFFECT`, `Info.DestroyParticleEffect`, 75-78, a world effect spread over the
  footprint) and the demolish sound (`DEMOLISH_SOUND`, `0x96`-`0x99` by size). Not yet counted: the
  build side - `Info.CreateParticleEffect` and the `+0xd4` sound zone the script loader makes when an
  item has one (`park.md`, "What selling a thing does to its script"). The world pass to draw them in is `WorldParticles`.
- [ ] **Q40. The place-staff mode's cell rule and its carry preview.** Found by Q6's decode (`park-engine.md`,
  "Putting a candidate down: the type-5 mode"), so this is a build. The original takes a worker only on a
  cell of `mType` 0, 1, 3 or 9, not flagged `0x40`, whose track record after the 12/17 parent redirect is
  not track type 11, 13, 16, 18 or 25; here any cell on the map takes one (`STAFF_PLACEMENT_CELL_RULE`).
  While a candidate is carried the original shows cursor 9 (`c_carry.ani`), hangs a sprite of their kind
  in their costume under the pointer and puts a red square on a cell the click would refuse; here the
  pointer is plain (`STAFF_CARRY_PREVIEW`). Confirm: carry a candidate over a cell the rule refuses, click,
  still in the hand and the red square photographed; then a path cell, hired.
- [ ] **Q43. The number read as a repeat delay is a priority, and the original throttles nothing.** Found by
  Q9's decode (`docs/exe/audio.md`, "How the engine plays an effect: priority, not a repeat delay").
  `SoundCategoryFile.Effect.RepeatDelay` is the effect record's `+0x0c`, which the original reads only as a voice
  priority, and nothing on its play path reads a clock. So `SoundCategory.Play`'s per-effect throttle has no
  counterpart, and neither have the replays it times: the lobby's beds and one-shots, the park's weather
  and SINGLESCREAM. The original's repeating voices come from the flags word instead. There are two kinds:
  - `0x0404` chains, built by Q9 for the screams only (staff 188 and the ambient beds are not);
  - the `0x4|0x2` voices whose constructor is not the music's `0x400` one: `0x006be090` (106 records), which hands a variation's wait to the mixer, and `0x006be680` (2). They are undecoded; music 2, kids 91 and ambient 33 are the music's class, which is decoded.

  Also owed:
  - the top-N voice pool, keyed by priority and closeness (N is 12, at most 30);
  - weighted variation picks (4 of 63 effects are uneven, and `SoundCategory.Pick` is even);
  - whether parameter 6 is also a level (OpenTPW hears it as the scream's gain);
  - replacing `ReadSamples`' scan with the structural walk.

  From Q135: in the original's mix the performance's end (`TADA.mp2`, staff `0x87`) was found on five of ten ends
  and two of the others fell where the mix was near silent (`ride-operation.md`, "The staff's sounds, in both
  games"): the voice pool may be why, and it is not read. A placed voice there matches its sample at 0.24 at best.
  Decode the `0x4|0x2` class's `0x006be090` and `0x006be680` constructors first. Confirm: capture the lobby's mix and a park's mix, before and after.
  The class's `0x400` constructor is decoded and the park's music follows it (the 2026-10-06 review's fix 4;
  `audio.md`, "The music's class"): parameter 4 picks the variation and an empty park is not silent. Its other
  constructors are not: `0x006be090` (106 records) and `0x006be680` (2).
  - **Note (fork review 2, 2026-10-16, `verify-audio-lips-video.md` leads 4 and 1):** the pool is rebuilt every
    service pass (`0x006b6ba0`): a voice is admitted when not finished, expired, replaced (`0x100`) or waiting
    (`0x40`), passing the device gate `0x006bc760`, in range or flagged `0x8` (`0x006bca10`), and keyed strictly above
    the pool's minimum, by insertion sort, the earlier voice keeping a tie. The key's low word is `0xFFFF` less round(
    0.01 d² ) over x and z (`0x006bb9f0`), left stale past 65535; only pooled voices are ticked. Screams carry
    priority 2700, the crowd 5300. A script's sound id its category lacks is silent with no fallback (`park.md`):
    check this game plays nothing for jungle `incagod`'s 43 and `Speaker2`-`4`'s 6.
- [ ] **Q51. Rest-area occupancy is unbuilt at both ends.** Found by Q36's decode. Arriving to rest
  (`FUN_00505fe0`) adds one to the rest area's script variable 0 (`VAR_STAFFIN` in every staff room) and sends
  message 15 to the resting-staff list (UI control `0x1e7b`); leaving (`FUN_00506d10`, from the normal end of a rest
  and from a sale) takes the one back. Neither half is built, and all three sites count `REST_AREA_OCCUPANCY`. Decode
  what the staff-room script does with `VAR_STAFFIN`, and whether a resting member of staff is hidden (entering state
  3 frees the sprite). Confirm: a guard resting, `rides` showing the room's variable.
- [ ] **Q55. A member of staff going idle queues no stand.** Found by Q36's review. The original's state-0 setter
  queues animation 3 every time (`FUN_005054d0` case 0, through `FUN_004fa460`: `PUSH 3`, `FUN_004217f0`);
  `Staff.AnimationFor` answers nothing for `Idle`, so a member who stops keeps the cycle they had - a guard put out of
  a sold rest area walks on the spot. Check every state-0 entry the original makes against the kinds' own sprite
  scripts before adding it. Confirm: a guard walking, then idle, `guests` showing the set change, photographed.
- [ ] **Q52. A rider on a thing without flag `0x20` is hidden in the original.** Found by Q36's decode. Admission
  tests the object's flag bit `0x20` (`0x0050212b`) and, without it, destroys the rider's sprite (`0x00502147`);
  OpenTPW never hides a rider. Every visitable thing in Lost Kingdom's save carries the bit, so nothing there shows
  it; a thing bought this session carries the bit its item gives it (`UsageInfo.RideHandlesSprite`, read since
  Q257c), so a bought Crazy Ape, whose item leaves it clear, is the case to run. Then
  build both. Confirm: a guest riding a bought Belly Bounce, `guests`, photographed.
  From Q252: **a rider on a thing WITH the bit is not standing.** In two files of the original's (Alexah's played
  jungle park and `q252/orig/ctl-written-by-the-original.TPWS`) all nine bounce riders' sprites are on program 66,
  the second state script, picture set 2; OpenTPW's rider stands on program 90, and a park file is written so.
  `Peep.AnimationFor`'s remark reads the setter's case `0x10` as "the stand": read it again with these files.
- [ ] **Q54. The `.sam` reader against the original's parser.** Found by Q36's decode (`park-engine.md`, "How a key
  finds its global"). In the original the first bad line ends the file - an unknown key, a bounded value out of
  range, or a negative in a non-negative field - and an array's count is the highest index written plus one, so
  Lost Kingdom's guest type is `rand % 8`. Compare `ParkBalance` with both. Confirm: a test per behaviour against the
  shipped files, and the guest types a loaded park draws from.
- [ ] **Q58. A purchase faces whatever the hand last held. Decode first.** Found by Q39's decode. The original keeps
  one rotation, `DAT_0081d7a4`, and `FUN_0052f200` writes it only for tool 0 (`0x0052f3b6`), so an item bought over a
  carried move faces the moved thing's way; a candidate's or worker's pickup leaves it too. Here a purchase always
  starts at nought, said at `ParkBuilding.CarryingAngle`. Decode what a successful put-down leaves in it (the light
  setter `FUN_0052f580( 0, 0 )`) before building one rotation. Confirm: move a ride turned 90, buy over it, put the
  purchase down, `objects` showing its angle.
- [ ] **Q60. A mechanic put down should look for work at once. Decode first.** Found by Q39's decode. The type-6
  put-down (`FUN_00505ea0`) sets every kind idle but the mechanic, whose claims are cleared (`FUN_004dad20`) and whose
  job search runs there and then (`FUN_004da5b0`); here a mechanic goes idle like the rest, counted as
  `MECHANIC_PUT_DOWN_JOB_SEARCH`. Decode whether idle here reaches the same search a tick later, and what differs.
  Confirm: pick up a mechanic, put them down, `staff` on the next few ticks.
- [ ] **Q61. The park-entry fly-in darkens the screen, and nothing here draws it.** Found by Q41's decode
  (`docs/exe/lobby.md`, "Escape cancels the fly-in"). State 2 sets the render camera's `+0x60` to
  `clamp( (1 - (r - 8) / (SPINRADIUS - 8)) x 65536, 0, 65535 )` (`0x005e052f`), states 0 and 1 set it to 0, and
  `0x005d8cb0` eases `+0x5c` toward it by an eighth of the gap per lobby pass and draws a black rectangle over the
  view with alpha `min( +0x5c >> 8, 255 )` (`0x008bd4e4`, drawn at `0x00576e54`). So the flight fades to black as it
  closes on the gate, and a cancel lifts it. Counted as `LOBBY_FLY_IN_FADE`. The ease is per pass, not per second:
  convert it through `Time.SmoothingFactor` and say so (rule 10). Confirm: `step` into the flight, screenshots at
  three radii with mean brightness falling, then Escape and the brightness back within a second.
- [ ] **Q62. What View the online world does offline. Decode first.** Found by Q41's decode. `IslandPanel_Callback`'s
  case for button `0x1e0e9` (`0x004b8c18`-`0x004b8c2f`) calls the island camera's `+0x20`, its deactivate
  (`0x005e1bd0`, which Ghidra names `IslandLobby_ViewOnlineWorld`), and then the online-world child's `+0x1c`, with no
  online test read yet; `IslandPanel.ViewOnlineWorld` does nothing, saying the original does nothing offline. Decode
  whether a test sits in front, and what the online child shows with no connection. Confirm: the button pressed,
  `windows` and a screenshot.
- [ ] **Q63. Does the engine's idle default play a lobby gate by itself? Decode first.** Found by Q41's decode. The
  lobby loop's `FUN_0044e410( 3 )` calls `FUN_00473c70( model, 0, 8 )` on every model with `[+0x14] != 0` and
  `!(+4 & 0x3000)`, and that replays role M entry 0 whenever channel 0 has finished, unless `(model+4 & 0x8004) != 0`.
  If the lobby's gate instances pass, the original's gates open by themselves - at the start, and again after a
  cancel's M2 - which would change what `LobbyGate` idles on. Decode the lobby instances' `+4` and `+0x14` (they are
  built through `0x005d8870` with flags `0xc0`). Confirm: a test on the flags, and the gate's `state` over 30 s idle.
- [ ] **Q64. Escape over the player slots opens the game menu.** Found by Q42's decode (`docs/exe/lobby.md`, "The
  lobby's keys act on the release"). `FrontEnd_ShowPlayerSlots` hides the lobby's root control (`0x004a65a8`) and gives
  the slots the focus (`0x004a65d6`), and their callback `0x004a5fc0` drops every key, so with the slots up no key acts
  at all: Escape opens no menu, and the slots' own Quit is the way out. A message box closed over the slots moves the
  focus to the hidden root (`0x004862a0`), where keys still reach nothing. Here `FrontEnd.MenuKey` opens the lobby's
  menu over the slots, said at the site. Confirm: nobody playing, Escape let go, `windows` still `PlayerSlots`; a
  screenshot.
- [ ] **Q65. The system table's keys: Ctrl+H on the release, and F8.** Found by Q42's decode (`lobby.md`, "The
  lobby's keys act on the release", its paragraph "The system table's keys, in either scene"; `park-engine.md`, "The original fires its shortcuts on key RELEASE"). The
  window procedure matches the system table `[0x0078718c]` itself on every key, in both scenes, and runs its handlers
  on the release: `P` (pause, `0x0040bf70`), Ctrl+H (Popup Help, `0x0040c5d0`), F8 (a `Scr%05ld.tga` screenshot,
  `0x0040c470` and `0x00550460`) and Ctrl+Shift+Alt+F8 (`0x0040c480`, a flag read only at `0x0054f455` and
  `0x0054fb93`). The player slots switch the tables off (`0x0040cfa0`) and their close back on (`0x0040cf60`). Here
  Ctrl+H toggles on its press (`HelpBar.Update`, said at the site), F8 is not built, and nothing switches the keys off
  under the slots. Decode what the F8 chord's flag does, and where a screenshot may be written without touching the
  game's folder, before building either. Confirm: Ctrl+H held, the help bar unchanged until it is let go; a screenshot.
  Until F8 and the chord are built, count each where it is let go (`CLAUDE.md` rule 4): nothing binds either, so the
  press reaches nothing and nothing says so (the 2026-09-26 staleness audit).
- [ ] **Q66. A disabled button takes the pointer.** Found by Q42's review. The original's hit test (`0x0065db25`) skips a
  control flagged `0x2` - a disabled button, `0x0065da8d` - with everything under it, so the pointer passes over it to
  whatever is behind: no hover, no help row, no glint. Here `UiButton.TakesMouse` is true whatever `Enabled` is, said at
  the site; the island panel's greyed arrows for an Instant Action player are the case in the lobby, and a press there
  still ends at the panel's outline, so nothing but the hover differs. Before changing the hit test, check every park
  window whose root takes no pointer: a press on a greyed button there would fall through to the world. Confirm: an
  Instant Action player's pointer on a grey arrow, no help row; a screenshot.

- [ ] **Q240. The postcard. Decode first.** From Q139. The gadget's postcard button is counted
  (`POSTCARD_BUTTON`), Ctrl+P under the full-screen view is counted (`FULL_SCREEN_VIEW_POSTCARD`), and Ctrl+P over
  the park (the shortcuts' row 15) and in first person (`FUN_00488a00`, action 15) is read by nothing: count those
  two first. All four reach `FUN_004a9380`: sound `0x95`, the game paused, the interface hidden, a screen chosen by
  `DAT_007cc150 = 3`, and a picture written out with `Postcard.wad`, `postcard.jpg` and an HTML template
  (`park-engine.md`). Decode the screen and what is written, to `docs/exe/`; the build is the next session.
- [ ] **Q242. Go Online and Publish Park. Alexah's call.** From Q139. Both are counted (`GO_ONLINE`,
  `PUBLISH_PARK`) and close the menu; the original starts connecting (`0x005b5cc0`) to servers that are gone. Ask
  Alexah: leave them counted, say so in the game's own style (rule 11), or something else. Then build the answer.
- [ ] **Q243. Every counted name against this queue.** Asked by Alexah, 2026-10-08: a thing "counted, not built"
  must have an item here, or it may never be come back to. List every `Unimplemented.Report` name in the source by
  script, find each in this file, and file an item (or add the name to the item it belongs to) for each one that has
  none; names dead by content say so in their item. No game run.
- [ ] **Q246. The ride view from first person, and its three other cameras. Decode first.** From Q140b. Walking
  into an open entrance starts the ride's view in the original (`FUN_0042a340`, then `FUN_004e15b0( 1 )`, the ride
  window's "Ride it!"; `park-engine.md`, "An entrance is shut to the viewer") and ends the walk; here each pass in the
  cell is counted (`FIRST_PERSON_WALK_INTO_RIDE`) and the viewer walks on, out of the Belly Bounce's entrance into its
  queue. `ParkEntranceGate.HasView` builds the fourth of `FUN_0042a440`'s views, the model's node, and counts the
  other three as not holding: `RIDE_VIEW_TRACK_RIDE_LEAD_CAR`, `RIDE_VIEW_TOUR_CAR`, `RIDE_VIEW_COASTER_NODE`, each
  dead by content in Lost Kingdom's stock park (no placed thing has a track-ride handle, a `TOUR` script or a
  coaster's track; which bought things would meet them was not looked at, but jungle's `coaster1`, `Coaster3`,
  `Tourride` and `wateride` models carry no view node, so their entrances are shut here until this is built). Decode the ride view's camera and what a
  coaster node's `+0x124` holds, to `docs/exe/`; the build is the next session.
  - **Note (fork review 2, 2026-10-16, `verify-tours-advisor-rse.md` L12, `verify-ui-hud.md` L13):** the ride view's
    smoothing in `FUN_0042b1c0` weighs the last view by (1000 - dt) × 0.0005, a dt over 200 ms taken as 100 first;
    leaving puts the camera on the ride's cell's centre, its yaw from `0x00522850 & 0x55` (`0x0042a1d0`). The tour
    car's view is Q408's.

## E. Large
- [ ] **Q31b. The other seven object windows.** After Q31's shop window: the sideshow's, the toilet's and the rest
  of `park-engine.md`'s nine, one window per session. Q31 carries the decoded figures and the `UIStrings` rows.
  From Q251: the staff window's days employed are `FUN_00505b70`, the calendar's now less `mTimeHired` over a
  day, handed to UITEXT `0x1bb` by `FUN_004b58b0` (`saves.md`, "`mTimeHired`"); `Staff.TimeHired` holds it.
  - **Note (fork review 2, 2026-10-16, `verify-ui-hud.md` L3):** the staff window is decoded whole: handler
    `FUN_004b5cb0` (fire, with UITEXT `0x18d`'s box when the options ask; pick up; the patrol toggle, tool `0x36`;
    locate; all staff), arrows walking all staff newest first and wrapping (hidden under two staff), stats refreshed
    every 4000 ms (`FUN_004b58b0`), advisor lines `0xc0`, `0xc1` and `0xd4` posted and withdrawn.
- [ ] **Q32. Graphics tiers.** Only `Level.SetupParticles` and `Level.NumKids` read the detail files (`low.sam`,
  `med.sam`, `high.sam`), and only `GameOptions.PARTICLEDENSITY` and `NUMKIDS` (the kid and staff bank caps) from them; nothing reads their `GraphicalOptions.*` keys (texture quality and
  filtering, sky, shadows, fog, mipmaps, view distance). The detail-file loader is `0x00423bc0` (`OptionsScreen`'s
  restart note). Decode what it does with each key - the lobby plan's item 11 names three: the `stexture` set,
  `SKYQUALITY` and the particle low-detail byte - then build low / medium / high.
- [ ] **Q33. UI scale.** The UI has one fixed virtual size (`VirtualScreen`, 2048 by 1536). Add Auto / small / medium
  / large in the dead Video Card row (`OptionsScreen.cs:71-72`).
- [ ] **Q34b. Pictures for the README.** Animated pictures need a capture tool; none exists in the repo.
- [ ] **Q72. Values from data, not constants (the 2026-09-12 review's Phase B).** The lobby's four island names
  from `THEMENAMES.str` through the reader the gate already uses (`ParkFixedItems.ParkDisplayName`) in place of
  `LobbyIsland.DisplayNames`; the `ISLAND()` directory and model names in place of `themeName[0..3]` (`LobbyIsland`,
  `LobbyGate`); and `DUCKINGLEVEL` from `sound.sam` for the advisor's 0.38 duck - but that mapping to `0x00785914` is
  inferred, not proven (`Advisor`'s note), so decode the global's writer first. For the names and the `ISLAND()`
  fields the data equals the constant, so "nothing looks different" is the regression check (`docs/exe/lobby.md`,
  "Park names and the locale tables"). Confirm: a lobby screenshot before and after, identical.
- [ ] **Q73. Cache pipelines and assets by key (the rest of the review's Phase D).** One pipeline per material
  (`Material`) where one per (shader, flags, output) would do, and texture and shader caches that scan `Asset.All`
  linearly (`Texture.Cache`, `Shader.Cache`). The shared blank texture landed (`bb897f8`); measure the load before
  and after, as that step did, and do not re-take its measurement.
  - **Note (fork review, 2026-09-30):** add `Renderer.cs`'s per-frame dirty-shader scan of `Asset.All` to the list; time it with the
    frame profiler before changing it. Review item ui-render-platform-2.
- [ ] **Q74. Lift the lobby out of `Level`, or record it as dropped (the review's Phase E). Alexah's call.** Park
  entry shipped without it: `Level` builds the lobby (`SetupEntities`, islands hard-coded, an unread `Global`) beside
  the park (`SetupParkEntities`). Ask Alexah before starting.
- [ ] **Q167. Read Alexah's Full Simulation saves.** Given 2026-09-28: a real player's saves, written by the
  original, in `~/repos/game/saves-fullsim/save/` (read-only; `CLAUDE.local.md` has the path). Jungle has
  `autosave.TPWS`, `New Save.TPWS` and `restart.INTS`; fantasy and hallow have theirs, space is empty. Made with
  `global.sam` edited (starting cash all 9s, one key per park); that changes the data folder, not the save's layout.
  The game crashed during the hallow park and would not load it again, so a hallow file may be damaged. Jungle first:
  load each in OpenTPW, count what reads and what does not, and put every layout fact in the FileFormats docs.
  Q165c read the jungle `autosave.TPWS` and `New Save.TPWS` through `ParkWorld` (`q165cprobe`, read-only): no problem
  reported, and the guests' two histories check out (FileFormats `saves.md`, 470).
  - **Note (Q174c's probe):** the `RSYS` channel module does not walk to its end in the jungle `New Save.TPWS` and
    `autosave.TPWS` (206 channels read, the cursor off the module's end), so `ParkThingStates` refuses it and none of
    their channels is restored; the fantasy and hallow saves and the shipped park walk closed (`q174cprobe/out.txt`).
  - **Note (Q181b):** a park built from the jungle `New Save.TPWS` throws in `ParkGuestSprites` (`People.ToDictionary` by
    `SpriteSlot`: two people share slot 0), after the scripts are bound; the game exits (`q181b/run1/run.log`).
  - **Note (fork review, 2026-09-30):** `restart.INTS` is the original's own fresh park. `FUN_00550b30` writes the new world's
    state once, on the first state-0xf frame, and Restart Park (`FUN_005ac5f0`) loads it back. Each of the three holds
    twelve things (the ten managers, then the unplaced gates and lights, catalogue objects x601 and x603), no people
    and no placed objects, `mGameTick` 0, the park closed, fee 20 and the shipped `Standard.sam`'s loans (APR 18-23);
    only the cash shows the edited data folder. It is Q197's reference. Review items gap3-2..gap3-11.
  Read by the week-end audit (2026-10-11): the jungle half looks done by Q165c, Q174c, Q181b and Q197. Left, and
  not said above: the RSYS module that does not walk closed in the jungle saves, the `ParkGuestSprites`
  duplicate-slot throw (not run again), and the fantasy and hallow saves, which are outside the Lost Kingdom
  scope.

- [ ] **Q405. Coasters run: their trains, motion and station.** From the fork review 2, 2026-10-16
  (`verify-coasters-run.md` L12-L17, L21). Lost Kingdom's `coaster1`, `Coaster3` and `Minecart`. The train count is
  clamp( trunc( lap / ( `uiTrainReleaseRate` × 0.001 ) ), 1, `uiMaxTrains` ) (`0x0043ca74`), the trains placed by
  release time; the motion tick `0x00435a30` runs once a frame after the 31 ms loop (a hop of half a sample, the (T +
  2O) / 3 collision exchange, f⁴ braking, floors from `fWinchSpeed` and `fMinSpeed`, decay 0.95 below speed 2);
  `coaster1`'s CrocCar is a bend car (`fBendElevationLow` 0.4, `High` 1.4 on samples flagged 8); the station step is
  Q289's note; a loaded coaster keeps its saved trains (flag `0x4000`, `0x00437cd5`) and riders (FileFormats
  `saves.md`, the coasters module; `ParkCoasters` reads only the first header); a car's seats are the capacity split
  over the centre-type cars. The PC's rating step is `ride-operation.md`'s, not the other fork's, which follows the
  Mac. Decode to `docs/exe/` first, then split. Confirm: Temple Of Gloom loaded from Alexah's park, its trains running
  and its riders let off, beside the original.

- [ ] **Q406. Coaster building: the track tool.** From the fork review 2, 2026-10-16 (`verify-coasters-build.md`
  L1-L11). A piece costs `fCostPerHeight` × H × 0.1 + `fCostPer2DLength` × length × 0.1, plus `fCostPerUnit` only on
  placing a pylon (`0x00447830`, `0x00435020`; the cursor's preview `0x00435d50`); placing takes trunc( cost ), a
  height or move release gives back trunc( old ) and takes ceil( new ); finishing, removing and deleting the last move
  no money, and nothing tests the balance. The edit handlers (height 0.005 a unit, rotate 0.02 within
  ±`fMaxRotnAngleDeviance`, bank and incline 0.02, loop tilt 0.01 in [0.15, 0.85]), auto-bank ((±π - turn) × v / 4.0
  over the neighbours' distances; `fAutoBankDivisor` is read by nothing), the sampler (0.05 steps, spacing 3.0) and
  the closing pair, the end node, the place distance (3 to 8 cells, 3 to 9 for a loop pylon, `0x0042fd90`). The
  validator `0x00434740` never refuses a click. Open: what hides the loop button on `coaster1`; whether Backspace
  reaches delete-last `0x00444f30` (`park-engine.md` has Backspace calling `0x0052fe50( 0, 1 )`, its only caller).
  Decode to `docs/exe/` first, then split.

- [ ] **Q407. Buy Land.** From the fork review 2, 2026-10-16 (`verify-objects-signs-other.md` L1). Counted,
  `BUY_LAND_TOOL`; Lost Kingdom's Features tab offers it (`hud.md`, item 101), and 13,878 of the 16,384 cells carry
  the unowned flag `0x40`. The validator `0x00531a50` refuses a span over 2 cells, gate posts, cell types 7 and
  `0x1e`, a hoarding crossed and a pocket left (a flood fill: grid `0x00532960`, fill `0x005328e0`), and shows the
  can't-afford pointer `0x14` (`DAT_00816d5c`) unless a layout is replayed; the price is `Costs.MapCell` × the unowned
  cells inside; the purchase `0x005327e0` places the hoardings and clears the flag inside (`0x005311c0`); the debit is
  `0x004d01f0`. Confirm: a rectangle bought, beside the original.

- [ ] **Q408. TOUR rides: Jurassic Tours.** From the fork review 2, 2026-10-16 (`verify-tours-advisor-rse.md` L1-L6).
  `tourride` (Id 1170), Lost Kingdom's golden-ticket ride, Full Simulation only (no `Easy_` `.sam`), runs 19 TOUR
  instructions, which fall to the counted default here. Decoded: the handlers (table `0x005569d0`; FileFormats
  `vm/instructions.md`), the flying car's tick `0x0055abf0` (nine states; take-off at 20000 and 14000; a lattice at
  max( 12000, ground + 5000 ); patrol corners at ±25000; 30000 near stations; bounds from the park, `FUN_0055ab50`), a
  rider's head ((car `+0xd8` + count) mod car `+0xdc`) + 1, TOUR 9's effects (worn particle 39, broken 40, a repair
  sound `0xbb` and clip 10), the draw `0x0055ce60`. Waits on Q141.

- [ ] **Q409. Full Simulation's loans and challenges.** From the fork review 2, 2026-10-16
  (`verify-economy-guests-paths.md` L1, L5). A loan offer passes when its amount is at most the net worth less what
  the bought loans still owe, signed, kept in the offer and rebuilt on opening and each tab (`0x0049fa00`,
  `0x004a06b0`, `0x004d06b0`). A won challenge with a follow-up pays no prize, and the follow-up is marked won as it
  is offered, so one declined or lost never returns; declining a challenge that is not Independent pays half its
  prize; the pacing reads `DaysAfterCompletedChallenge` and `DaysAfterDeclinedChallenge`; nothing raises the declined
  count, so `DeclinesToForfeit` is never reached (`0x004d2b60`, `0x004d2fb0`, `0x004d1a00`, `0x004d3140`,
  `0x004d1ae0`, `0x004d2ec0`). Neither screen is built.

## F. Then

Back to `docs/PLAYER-GAPS.md`: gap 4 (advisor in a park); gap 5 (happiness gauge) is Q244 and gap 7 (saving a
park) is Q241j, both in section A. Also litter and the day ending, whose deferral reasons expired
(`docs/REVIEW-2026-09-21.md` section 6).

## G. The lobby plan's open items

From the lobby plan Alexah agreed on 2026-09-12, "Finishing the Lobby" (its artifacts are listed in
`docs/history/README.md`). Alexah asked that its report be kept in use: when an item here lands or the order changes,
update it with the Artifact tool's `url` (https://claude.ai/code/artifact/48cf6fc2-0874-404c-b7bf-a05fe4b16470), never
a second copy. Items 1-4,
6-8 and the gate half of 5 landed; item 11 is Q32. Cut from the lobby that day and still cut: the intro movies and
splash, the three online UI trees, cones, reverb and Doppler, the message box's third button, and the original's full
state machine.

- [ ] **Q76. The isle's clips loop; the original picks one at random when idle (plan item 5).** The camera update's
  tail loop (`0x005e11f7`) starts the isle's M1 or M2 at random whenever it is idle (`docs/exe/lobby.md`, "Not
  sound"); here the isle's clips loop. Counted as `LOBBY_ISLE_RANDOM_CLIP`, once an isle (Q69); building it takes the counter
  out. The gate half landed (`3eed966`, `9b0ebab`, Q41); whether the gate also idles
  by itself is Q63. Confirm: `sound`/`state` over a minute of lobby, and a capture of the isle between clips.
- [ ] **Q77. The advisor's 90-second idle nag in the lobby (plan item 10).** Responses 394/395 (`0x18a`/`0x18b`,
  armed at `0x005e184c`, `docs/exe/ui.md`); sample 470 is never heard today. The arm and the queue's timer are read
  (`ui.md`, "The lobby's idle repeat"), and the arm is counted as `LOBBY_ADVISOR_IDLE_REPEAT` (Q69); when the first nag
  comes wants one sitting with the original first. Confirm: the line heard after 90 idle seconds, by capture.
- [ ] **Q78. The lobby's sea from `lobby/Terrain/base.md2` (plan item 9). Decode first.** The lobby has a sea: a
  `Water` entity (`Level`) that `Sky` seals the horizon against. The item is re-sourcing it from `base.md2` through the
  park terrain loader - but `Sky`'s own note says the original's lobby has no sea below its islands, which contradicts
  the premise. Settle whether the original draws `base.md2`'s sea before building anything.
- [ ] **Q79. Robustness (plan item 12, the rest).** Atomic save writes exist (`BaseFileSystem.WriteAllBytes`) and
  input is dropped while unfocused (Q42). Open: top-level error containment (nothing in `Program.Main` or `Game.Run` catches a failure of the load or the run loop),
  and easing the render rate and music while the window is unfocused.
- [ ] **Q80. Two tests the lobby audit asked for (plan item 13, the rest).** A save round trip against a real
  game-written file (it wants a sitting with the original to make one), and `LobbyScript` over the four shipped island
  scripts. Sound categories are covered by `SoundCategoryTests`.
