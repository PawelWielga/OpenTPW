# What a player notices

What a player notices, **ordered by when a player meets it**, not by size or by how interesting it is to build.
**This is not the work queue: `docs/QUEUE.md` is**. Its section F returns to gap 4; gap 7 is its Q249 (a fresh park saved), in its section A.
Drawn up 2026-09-20 from `docs/STATUS.md`, the 42-thing Lost Kingdom census
(`docs/history/current-task-archive.md`, grep `THE PARK IS FULLY DECODED`) and the live plan.

`docs/history/CLEANUP-PLAN.md` was a second list of nine things a player sees, handed over 2026-09-21. All nine are
closed; `docs/QUEUE.md` Q13 moves it into `docs/history/`. Until then it is untracked and exists on this machine only.

**Alexah sets which item is the goal. One item per session** (`CLAUDE.md`, "How a session runs" 4).
Tick an item here in the same commit that lands it, and move its detail to the page that owns it.

Scope is **Lost Kingdom only**. Anything that changes nothing in `data/levels/jungle` is not on this list,
however large it looks — that trap has been hit twice.

## Deferred in item 2: research and park status

**ONLY THE RESEARCH SCREEN REMAINS.** It is not a category
(`FUN_004a0840`'s case `0x2b` goes straight to `FUN_004aa480`, as the map does) and its screen is six
effort sliders over research groups, or a message box in Instant Action or with no researcher hired. This game has no
research screen; Lost Kingdom's one researcher (thing 30) walks and researches as the original's does (Q134), and
the lab spends the points and finishes projects (Q701). Its two message boxes are built (Q410): in Lost Kingdom's
Instant Action the button and R open "Research is automatic in Instant Action mode.", as the original's; the screen
behind them, Full Simulation only, is built (Q590: the bars, labels, arrows, effort and work-load sliders; its pumps' motion and sounds are Q705). The lab at work is built (Q701), and a finished project is told to the advisor and starred on the buy list (Q702); the end of research marks the park and posts row 167 (Q550), and its End feature is built (Q703).

**Park status (screen 3) is deferred by Alexah, 2026-09-21** — *"I'm okay delaying work on the Info
screen. It's not important at the moment."* The Information category is therefore seeded to screen **4**
where the original seeds **3**, a declared deviation that reverts the day park status is built.

**Setting a patrol area, deferred by Alexah "for now" on 2026-09-21, is built** (Q433): the staff member's window's
patrol switch hands over the patrol tool, and two clicks on the map fence the member (`docs/exe/hud.md`).

## How the vehicles move

The `.RSE` runtime is sufficient: the bus's script binds and runs to completion,
reaching nothing unbuilt. The movement is a **Bézier route in the model file** (the array at `0xac`, indexed by a
node's `+0x52`) driven by a **per-frame percentage in the animation** (channel `0x200`), which `LobbyModel.Pose`
applies for all three vehicles.

Confirmed from the engine as well as the data: `FUN_00471860` indexes the table as
`*(model + 0xac) + node[+0x52] * 0x10`, and `FUN_00474840` is the cubic Bézier over it.

## The eight gaps

| # | What a player sees | Depends on |
|---|---|---|
| ~~1~~ | ~~The gates never open when you enter a park~~ **DONE 2026-09-20** | — |
| ~~2~~ | ~~Four of the six gadget buttons do nothing~~ **DONE 2026-09-21** — five of six now open something | — |
| ~~3~~ | ~~Nobody new ever arrives~~ **DONE 2026-09-20** — and on all three vehicles | — |
| 4 | The advisor says one line in a park, and nothing after it | — |
| ~~5~~ | ~~The happiness gauge reads wrong~~ **DONE 2026-10-19** (Q244) | — |
| ~~6~~ | ~~Nobody goes home and no day ever ends~~ **DONE 2026-09-20** — same loop as 3 | — |
| 7 | A park cannot be saved | — |
| ~~8~~ | ~~Guests cannot buy anything from a shop, and barely from a sideshow~~ **DONE 2026-09-20** | — |

**Two remain: 4 and 7.** Each is independent of the other — neither is blocked on
anything now built, and neither blocks the other.

---

## 1. The gates never open when you enter a park — DONE, 2026-09-20

- [x] The gate idles **shut**, opens **once** as the park-entry flight's camera finishes homing onto the
      gate side (`LobbyCameraMode.StepLeaving`, `0x005e06e4`), and shuts again if Escape cancels the flight
      from state 2 (`0x005e18ab`). The park is asked for when the camera arrives, not when the doors finish.
- That is the original's: see `docs/exe/lobby.md`, "Escape cancels the fly-in, and the gate is the flight's".
- **Not every gate swings.** Fantasy's is a worm with **no rotation tracks at all**, so it gets no
  `MeshRotator`. Each clip plays over the span it declares; the table is in `docs/exe/lobby.md`.
- **Confirmed in the running game** by Q41's run (`q41confirm.py`): the console's `state` read the gate playing its opening clip during the flight, and the cancel from state 2 shut it.
- **Two traps worth keeping**, both of which cost runs: the debug console's `pause` sets `Time.Paused`
  and `Time.Delta` then reads **nought**, so anything driven by it freezes and never finishes — use
  `step <n>`, each one frame of ¹⁄₆₀ s. And the lobby camera orbits, so `settle` alone lands on
  whichever side it had reached: the gates face their island's **−Y** side, which is `orbit π`.

## 2. Four of the six gadget buttons do nothing - DONE 2026-09-21, five of six now open something

**TICKED. RESEARCH was the last to do nothing; its screen is built in Full Simulation (Q590).**
Buy carries the whole purchase/hire/management half; paths, queues and building by pointing are all
laid and verified; and Info and Money now open real screens - **all four of their buildable screens
drew in a running park and were photographed**. Research is not a category at all (`FUN_004a0840`'s
case `0x2b` goes straight to `FUN_004aa480`, as the map does), and its one screen is six effort
sliders over research groups. Instant Action has no research screen (the lab spends its researchers' points and
finishes projects, Q701; the advisor's lines on it and the end of research are Q702 and Q703). The button and R, the
shortcut to the same function, open its two message boxes (Q410) and, in Full Simulation with a researcher, the screen
(Q590).

- [x] **Buy** opens `ParkBuyScreen` (stream `0x00754cf8`), which cross-links to `ParkHireScreen`
      (stream `0x00751fa8`). Both are built on the original's control **type 7**, a scrolling
      multi-column list that nothing in this tree had - `UiList`. A row's payload is sized by the
      COLUMN COUNT, which is why buy pushes three and hire two.
- [x] **Clicking a placed ride opens its management window** - `ParkObjectWindow`, stream
      `0x00755150`. **There are nine such windows**, dispatched by `FUN_00486920` on the thing's kind
      byte and the item's `WhichUIType`; the shop's is built (`ParkShopWindow`, Q31), and the others stop at counted names
      (`SIDESHOW_WINDOW`, `FEATURE_WINDOW` for toilets, staff rooms, misc items and upgrades alike); the staff member's is built (Q431),
      its FIRE and PICK UP too (Q432), and so is the visitor's (Q430), which a visitor's world click does not reach. Close, delete, move and
      the two cycle arrows work.
- [x] **The rides panel - 2026-09-21; two rows still counted.** All three sliders take their
      range from the item and their value from the ride, and **save**: capacity 5 of 1..10 clicked to
      6 read back as 6 after the window was closed and reopened.
      **The stats table fills five of its seven rows.** The four condition rows are GAUGES, not text -
      `FUN_004ade40` hands each `((v & 0xff) << 10) / 100`, a 0..100 percentage onto a 0..1024 bar -
      which is why they rendered their labels perfectly and showed no values at all. State of repair
      and Remaining life are now drawn, from two floats at save **1074** and **1070** that nothing was
      reading; Belly Bounce measures **repair 100, life 100**. Age and Scrap value were already there.
      **Excitement and Reliability stay counted**, and not for want of a control: their inputs are named now
      (`park-engine.md`, the compiled `.sam` schema) and the guests' choice computes the first
      (`ParkRideScore.ExcitementOf`), but neither bar is built. **Users last month** is the object's customers
      over its last thirty finished game days (Q177b), read from the save's rings and kept by the settle-up.
      **The preview draws a fresh instance of the item's `P` model**, its own where it ships none, fitted,
      tipped and turned as the original's (Q188; `docs/exe/park-engine.md`, "The object window's preview").
      The light is OpenTPW's own, and the turn stops while the debug console holds the clock.
- [x] **The verbs underneath**: `ParkBuilding` buys, sells, moves and carries; `ParkStaffPool` and
      `ParkPeople` hire, fire, pick up and put down. Money is taken at PLACE time, from the item's
      `+0x1b8`, which is why cancelling a carry needs no refund.
- [x] **INFO and MONEY - DONE 2026-09-21, four screens, all confirmed on screen.** The HUD is six
      CATEGORY pickers, not seventeen buttons: `FUN_004a0940( n )` opens whichever screen a category
      was last left on, and `FUN_004a0810( category, screen )` in each builder confirms the seeding
      from the other direction. Built: **allstaff** (`0x750e10`), **allitems** (`0x7508e0`),
      **allpeeps** (`0x7506c8`) and **entryprice** (`0x751798`).
      **The Info and Money buttons were pressed for real** - through `click`, the genuine hit test and
      both handlers, only SDL skipped - and each opened its screen: `the interface took (162,558)`
      and `(100,609)`.
      **The numbers were predicted before they were read.** The fee reads **25** and the spinner moved
      it **25 -> 26 -> 24**; `b_door` took the gate **open -> shut -> open**; the cleaner's wage read
      **63** and the scientist's **125**, which are `PerGradeStaffConsts[grade].BaseWage` times
      `PerTypeStaffConsts[kind].PayMultiplier` on the EASY numbers (7x9 and 5x25). All Miscellaneous
      Items counted **Small Toilet 3**, which the save's own `VisitableFlag` census independently
      records as three.
      **Three of the four screens' columns are filled only where this game has the number.** Rides
      fill State Of Repair and Remaining Life; miscellaneous items fill both columns; visitors' Rides Ridden
      is the guest's `mNumRides` (Q177b); shops, sideshows and visitors' Time In Park stay blank and counted
      rather than carrying a plausible wrong quantity. **UITEXT row 117 is literally `"?"`** - the original ships that visitor
      column unnamed too.
- [x] **RESEARCH** - built: its two boxes (Q410), its screen (Q590), and the screen's pumps and three sounds (Q705),
      each confirmed beside the original.
- [x] **PATHS - DONE 2026-09-21, and verified in the running game.** `path 10 10` lays one for
      **20** (`Costs.PathCell`, measured from `Standard.sam` and confirmed through the game's own
      balance reader); `delpath` lifts one and **refunds nothing**, which is the original's own
      asymmetry - only a queue cell credits anything back. A laid cell joins itself up by
      `FUN_005348d0`'s rule and takes its art from the executable's own two tile tables.
      **Photographed:** a plus of five cells drew end pieces at all four tips and a **crossroads** at
      the centre, and the HUD money went **87987 -> 87887** - exactly 100 for five cells, on screen.
      A laid cell retiles itself and its neighbours.
- [x] **QUEUES - DONE 2026-09-21, and the `FUN_004de1f0` HOOK IS PROVEN.** `queue <x> <y> <thing>
      <fromX> <fromY>` lays one for **75**; `delqueue` lifts one and **refunds**, where a path does
      not - the original's own asymmetry. A cell records the object it serves and a flow direction
      that is the **opposite of the step taken into it** (first writer wins), which is what makes the
      queue measurable: the walk accepts a neighbour only when its flow points back the way the walk
      came.
      **The hook this item has flagged from the beginning now works, and the numbers were predicted
      before they were read.** Deleting a middle cell took the Belly Bounce from **4 cells ending
      2866** to **2 ending 2868**, and the capacity readout followed it from 0/16 to 0/8 - while the
      save's own `mQueueSizeInCells` sat unchanged at 4, correctly ignored. Without
      `ParkState.InvalidateQueue` that number could not have moved at all, because `QueueCellsFor`
      returns the cached pair whenever the save sets it - and this park sets it on exactly that ride.
      **Deleting a queue cell ORPHANS the remainder and that is correct** - the original has no
      trimming loop anywhere.
- [x] **Building by POINTING - DONE 2026-09-21, confirmed by console.** `ParkBuildMode` holds the
      original's single MODE global and its anchor pair. There is **no drag** - both drag vtable
      slots are bare `RET` stubs - so a run is click-to-anchor then click-to-commit, the target
      snapped to the dominant axis, **and the anchor then advances to that snapped target rather than
      to where the run reached**, which is exactly what lets an L be laid click by click.
      Measured: click one anchored and laid nothing; click two laid **6 cells (8,9)→(13,9)**; click
      three laid **4 more** down the other axis, and the corner came out as tile index 3 at 90 - a
      corner piece - with end pieces at both tips. Money fell **87987 → 87787**, exactly 200 for ten
      cells. **An armed mode consumed a click on the Belly Bounce instead of opening its window, and
      once disarmed the identical click opened thing 13's window** - the original's rule, shown both
      ways. A run **aborts entirely on the first cell that refuses**, as the original's line walker
      does.
      Seen on screen since, through the player's own route: a queue laid by two clicks was photographed
      standing and joined (`docs/QUEUE.md` Q3).
      **THE QUEUE TOOL IS ARMED FROM THE INTERFACE TOO, 2026-09-22.** The ride window's own queue button
      arms it against that ride (`ParkObjectWindow`, verb `0x3e34`), and clicking a queue cell re-arms
      it for the thing that queue serves - the original's mode `0x14`. Both were driven through the
      real interface and the armed mode read back as `mode 3`.
      **THE PATH TOOL IS PICKED UP FROM THE PARK TOO, 2026-09-22 (Q35).** A click on grass or path arms and
      anchors it, Backspace takes the last run back up, and Escape puts it away - confirmed in the
      running game with the real keys.
- [x] **Placing by pointing - DONE 2026-09-21.** Both screens put the item or the person in the hand,
      and clicking the park puts them down. `Level.WorldClick` takes the click only when the interface
      did not, and **anything in the hand goes down before any window opens** - the original's own
      order, since a place mode consumes the click and only an idle mode opens windows. The console's
      `put` and `hire` still do it in one step, for a test that cannot move the pointer.
      **A quick right click cancels, with RMB cancel on, and so does Escape** - the original's own ways
      out of a place mode. Nothing is charged for picking something up, so putting it back gives nothing
      back and takes nobody out of the pool. The console's `drop` reaches the same `ParkHand.LetGo`
      rather than a copy (`docs/exe/park-engine.md`, "The hand's ways out").

### The original section, for the part still open

- [x] **Was seen:** Info, Money and Research do nothing. **Five of the six now open something**, as of
      2026-09-21 — Buy, Camcorder, Map, Info and Money; Research's boxes since Q410, its screen since Q590.
- **Lives:** `ParkGadget.cs` — `b_resrch` runs `ParkResearchOpener`, which opens `ParkResearchScreen`. `b_info` and `b_money` go through
  `ParkCategoryScreens.Open`, which is the original's own remembered-tab picker.
- **The real dispatch is decoded** in `docs/exe/hud.md`: `FUN_004a0940( n )` opens *the screen that
  category was last left on*, from three globals seeded 1 / 3 / 10. **The HUD is six category pickers,
  not 17 buttons.**
- **Two jobs, and they are very different sizes.** Making the buttons honest is small. Making them *work*
  needed buying, building and hiring, which now exist (item 2 above), and finances and research, which still do not — the single
  largest missing system in the project. Do not start the second one by accident.
- **Gate:** the `unimplemented` census in the debug console.

## 3. Nobody new ever arrives — DONE, 2026-09-20

- [x] **Was seen:** the park is permanently the save's 13 guests and 5 staff. Once they have ridden the
      one ride, nothing changes again, ever.
- **DONE, 2026-09-20: GUESTS ARRIVE BY THEMSELVES AND GO HOME BY THEMSELVES.** A park left
  alone takes a load on the park's own clock with nothing typed — the first 126 s in, the next about
  150 s after each — and the saved guests go home once miserable, or caught deciding as their day runs out (Q109). `ParkPeople.StepArrivals` is the manager
  and `ParkPeople.Depart` the other half; `ExitLevel` is what sends them home, a countdown nothing had
  ever read. **And the vehicles carry them now — 2026-09-20, all three drive.** The one thing missing
  was an opcode: `Ferry.RSE` and `seaplane.RSE` start every animation with `TRIGWAITANIM` where
  `bus.RSE` uses plain `TRIGANIM`, so while that had no case in `RideScript` those two stood still and
  only the bus moved — the whole of the difference. With it built, `load 40` flies the seaplane in and
  `load 70` sails the ferry in, and the guests step off once it has landed rather than before: `peeps`
  holds at 13 through about nine seconds of approach, then 16 → 21 → 25 → 30.
- **And a second half to it: releasing the vehicle three times a circuit, not once.** Each vehicle
  script parks at three `TEST VAR_TRIGGER` spins; releasing only the first left the bus stopped at
  `VAR_STATUS` 4 for ever and, because arrivals are gated on the vehicle reporting 2, **the park
  drained to nought after one guest**. `ParkPeople.StepVehicle` is the tail of `FUN_004cf3e0`, which
  runs every thing sweep and re-triggers on status -1, 0, 4 and on 2 once its load is let go. Unattended, the
  population moves both ways: a load is called about 150 s after the last one got off, and the saved
  guests go home once miserable, or caught deciding as their day runs out (Q109).
  A `vehicles` console census was added to see this at all - `paths` reports where a vehicle is drawn,
  which reads identically whether its script is running or parked, and only the pc separates them.
- **Lives:** `ParkPeople.PeepsIn` builds the save's guests; `ParkPeople.Admit` builds every arrival, at a bus
  stop cell (`ParkAdmission.BusStopA/B`), in `PeepState.AtGate`; the walk in through
  the original's outside states is not built (`docs/exe/guest-arrivals.md`); `PeepState.AtTheBusStop` (21) is built and
  holds leavers until a vehicle stands for them.
- **Census:** ids 29, 31-41 **and 42** are the 13 guests, on the bus road at x 47-48, y 9-15, already
  walking in. (Thing 42 is a guest too. Compute a free id, never take one from here.)
- **The gate-admission path it feeds** is built and measured — guests pay at the gate.
- **Gate:** `park jungle`, then the `guests` census over time. **Predict the count before reading it.**
- **What remained for the loop — all of it done, 2026-09-20:** an arrival manager (its timer now counts fours of
  the thing sweep from the save's mark, as the original's does: the first load 126 s in and the next about 150 s
  after each, decoded by Q68 and built by Q68b); guests created and walked in from the stop; guests walked out and
  removed; and the ferry and seaplane **driving** alongside the bus.
- **The whole mechanism is decoded — see `docs/exe/park.md`, "Arrivals".** Its shape
  is not the one this list assumed:
  - `FUN_004cf3e0` waits out a timer, asks `FUN_004cf5b0` for a headcount, summons a vehicle, and then
    makes **one guest per thing sweep** through `FUN_004cf720` until the load is spent.
  - **The vehicle is chosen by crowd size** — under 36 the bus, up to 60 the seaplane, beyond that the
    ferry. At random only when none is current and the manager's tail summons one.
  - **The vehicle thing is created on demand** (`FUN_0051a2f0`), which is why this park places a bus
    and neither of the others: its `mArrivalVehicle_Size1` slot holds the bus (thing 15, measured) and
    the other two slots are nought because no crowd that big has ever arrived.
  - **Nobody rides in anything.** The guest is constructed at a cell near the stop, so the vehicles are
    mechanism rather than transport — and state 21 deletes one when it reads 4.
- **The rate** is `Arrival.TimeBetweenArrivals` 150, proven by the balance loader's slot table. OpenTPW counts it as
  the original does, in `mGameTick >> 2`, fours of thing sweeps, and carries its wait over from the save: measured
  in Q68b's run, the first load on sweep 509 (126 s), and the next 604 sweeps (149.5 s) after the last guest got off
  (`docs/exe/park.md`, "Arrivals"). The vehicle's drive in comes on top, and the wait starts again only once the
  vehicle has put its load down. Between loads the bus waits at the stop where the original's waits out of sight,
  so a later load's guest comes on the sweep that calls it (Q131).

### The three vehicles they arrive and leave on

Alexah wants them dropped off and picked up at the front of the park by a **cruise ship, a sea plane
and a bus**. All three are shipped fixed items, and **none of them is missing art**:

| Item | `Info.Id` | Where it sits now |
|---|---|---|
| Bus | **1600** | **Drives.** Spawns at cell (29.7, −11.5), runs its route and stops at its two shelters, world (529.1, 64.4) and (439.9, 64.8), where `BusStopA` and `BusStopB` are (Q583) |
| Seaplane | **1602** | **Flies, lands and unloads.** Photographed on the water in the bay |
| Ferry — **this is the cruise ship**, confirmed by Alexah 2026-09-20 | **1604** | **Sails and docks.** Photographed alongside the quay |
| (Gates 1601, Lights 1603, End 1605 — End is three aircraft 59 units up) | | |

**`ParkFixedItems` now loads gates, lights and the vehicles** (the ferry and the seaplane only at their first summons, where the file names no thing for them; Q26c). Only the End sign is still held
back. All six carry `DontApplyOffset 1` and `WhichUIType 4`.

**Routes:** ferry 33 points, seaplane 33, bus 45, all open Bézier routes (type 2): 10, 10 and 14 segments, the last
two points never sampled; each stands on its own route.

**A vehicle turns to face its direction of travel** (Q583): the route's first derivative (`Route_BezierTangent`)
normalised, and a basis built from it (`Matrix_FaceAlongTangent`; `exe/park.md`, "Where a route key puts its node").

**The bus runs its circuit repeatedly** (`ParkPeople.StepVehicle`), watched through statuses 1, 2, 3, 4 and 5, and
the seaplane and ferry both loop back to the start of theirs.

**The stop geometry is already decoded**, from the theme's own `Standard.sam` — do not re-derive it:

    BusStopA/B        (42,5)  (53,5)
    CrossingBSSideA/B (47,5)  (48,5)      <- bus-stop side of the crossing
    CrossingParkSideA/B (47,9) (48,9)     <- park side
    TicketBoothA/B    (47,13) (48,13)
    EntranceA/B       (47,17) (48,17)
    MapInfo.FixedItemOrigin (48,17)

The census places the save's 13 guests on the bus road at x 47–48, y 9–15 — i.e. already walking in
from the crossing. So the arrival path they would take is the one the shipped save shows mid-use.

## 4. The advisor says one line in a park, and nothing after it

- [ ] **Seen:** the advisor is a constant presence in the original and says nothing here while you play.
- **Lives:** `docs/exe/advisor-park.md`; a park's lines are `World/Advisor/ParkAdvisorMessages.cs`'s: the door's opened and closed messages (Q90b), silent with
  the shipped `Advisor.sam`, whose score of 20 for each is under the threshold of 25; the breakdown lines (Q305 to Q307), the
  message bar's (Q312) and the control panel's, which is polled and which the stock park's file holds as said (Q316).
- **The "needs simulation" blocker was refuted:** his acceptance gate consults no world state, and the HUD
  it waited on exists. This is buildable today.
- **Do not transcribe anything first:** every global speech line is already transcribed in
  `global-speech-transcripts.tsv` in the Ghidra notes (where they live is in `CLAUDE.local.md`). Grep it.
- **Gate:** audio capture, cross-correlated against the game's own mix (the memory note `verifying-audio-by-capture.md`).
- **NOT confirmed in a run** — this rests on code reading alone.
- **He also polls** (Q177): each tick scores one of 156 rows of his table, so lines such as the ticket price (messages
  91 and 92, over the gate's opinion ring, which OpenTPW does not keep) and a shop's great satisfaction come from the
  park's own figures (`advisor-park.md`; `ride-operation.md`, "The settle-up's bookkeeping").

## 5. The happiness gauge reads wrong — DONE, 2026-10-19 (Q244)

- [x] **Seen:** the bar drew in the wrong place, as two copies split down the middle. *"The bar image just appears to
      not be rendering within the actual location properly, it looks like two copies of the bar split down the middle
      like it's repeating"* (Alexah, 2026-09-21).
- **The cause:** `meter.wct` holds two capsules side by side, a lit one and a dark one, and the original draws the lit
  left half below the boundary and the dark right half above it, each cropped; `UiMeter` drew the whole texture's top
  slice squeezed into the bottom of the capsule, both halves side by side. Now it draws the original's two quads, the
  dark capsule whole at nought (`docs/exe/hud.md`, "The happiness gauge's two quads").
- **Confirmed beside the original** on three files whose every guest is at 70, 30 and 0: 69.5%, 29.5% and 0% in
  OpenTPW against 67.9%, 28.6% and 0% in the original, whose clock ran on through the load (`q244/side-by-side.png`).
- **Still open, and not this gap:** which guests the mean counts (Q148) and its fraction (Q459).

## 6. Nobody goes home and no day ever ends — DONE, 2026-09-20

- [x] Guests go home when their `ExitLevel` runs out — a countdown that had been ticking since the save
      was first read with **nothing anywhere reading it**. `ParkPeople.Depart` reverses everything
      `Admit` wires, including `ParkState.Forget`; a guest a ride or a queue is holding is refused,
      which is the original's own condition. Confirmed unattended: fourteen departures by name, the
      population moving both ways rather than only down.
- **It was built with 3, not after it** — exactly as the note below predicted. Built alone, departures
  drain the park and leave it that way, and that is not hypothetical: a defect in the vehicle handshake
  did drain it to nought for a while, which is what caught the fault.
- **A day's end rolls only the objects' day rings** (`Level.Update` → `ParkState.RollTheDay`, Q177b); nothing
  else is paced by it yet. That half of this line stands.
- **Lives:** `GameCalendar.DayRolled`, which only the rings' roll reads.
- **Gate:** the `peeps` census, with `pause` and `step <n>` to make a short-lived state observable.
- **Same vehicle loop as item 3**, which is why they are now one job: a bus that drops off must also
  pick up, and a guest who goes home has to leave by something.

## 7. A park cannot be saved

- [ ] **Seen:** Publish does nothing, and a park made fresh (a Full Simulation park entered with no file) cannot be
  saved, which a player meets at the moment they try to stop. Load opens the Load Park screen and loads the row
  clicked (Q241c); Save opens the Save Park screen and its OK writes the running park under the typed name (Q241d,
  Q241j), which OpenTPW and the original both load back. Leaving a park writes it as `autosave.TPWS`, which the next
  entry finds, as the original's (Q544).
- **Lives:** `Level.WritePark`, which counts a park with no file as `SAVE_PARK_WITH_NO_FILE` (Q249), and
  `ParkFrontEnd`'s game menu, where Publish Park calls `NotYet(...)`, counted as `PUBLISH_PARK`.
- **Decoded (Q241):** the two screens, the folder's list, the load in place and the writer's container are in
  `docs/exe/saves.md`, "Load Game and Save Game"; Q241b to Q241j and Q544 are built, and `QUEUE.md` holds Q248 and Q249.
- **Nine park files agree with the traced preamble** (`docs/exe/saves.md`, "Preamble"): the shipped `Easymode.TPWI` and the eight of
  Alexah's Full Simulation play. The original under Proton has loaded the files the console's `savepark` and the menu's Save Game wrote.
- **Full Simulation now starts fresh** (Q197): twelve things, closed, fee 20 and the regular balance/loans;
  Instant Action keeps `Easymode.TPWI`. Where the player's folder holds a park file for the theme, entering loads the newest instead (Q241b).
  The initialization and its reference comparison are in `exe/park-engine.md`, "A fresh Full Simulation world".

---

## 8. Guests cannot buy anything from a shop, and barely from a sideshow — DONE, 2026-09-20

- [x] **Guests buy from both.** Confirmed in the running game: a filled park took **1110 at the Drinks
      Shop** (37 sales at 30) and **900 at the Jungle Spray** (45 at 20) across two runs, with the
      `spend` census showing the cause before the effect — guests `heading {shop:16}` and then the till
      moving. `save/` unchanged within every run.
- **A queue's length is walked off the map, not read from the save.** `FUN_004de130` (`GetBackOfQueue`)
  rewrites the object's `+0x40` from the type-3 cells whenever `mBackOfQueue` is nought, so a shop and a toilet
  whose saved count is nought still take a queue. **A shop is offered and paid like a ride**: `Coconut.RSE` runs the
  same `VAR_LETMEON` → `WAIT 1000` → `VAR_LETMEOFF` handshake. The decode is `docs/exe/ride-operation.md`.
- **What made the visit do anything: the win roll.** `FUN_004e2670` draws `r` from the park's own generator as a
  guest enters and wins when `r % 100 <= chance`; SetState stores it in `mQueuePos` (`0x00501f41`), which the settle-up splits on. Nothing here ever wrote
  that byte, so **every visit in the park took the losing arm** — which is why a sideshow charged 20 and
  did nothing else. Chance of winning is `100 - UsageInfo.InitChanceOfLoosing`: a shop declares none, so
  its chance is **100** and a drink is always served; the Jungle Spray declares 75, so **25**.
- **Built with it:** the losing arm (happiness down by `MediumHappinessChange`), the sideshow's prize
  (`+50`, its `InitCostOfGoods`) and the win's happiness, `log2( cost / price ) * 15` = **+19**.
- **Still unbuilt, and named rather than quietly skipped:** the park analyser's shop and sideshow month
  accumulators (`+0x20130` / `+0x20380`), and of the settle-up's bookkeeping the
  event history, the analyser's sample and the sideshow's thoughts, counted. The visit counts, the object's six day
  rings, the happiness gained since joining and the served count are kept (Q177b); the charge is banked, and a shop's
  and a won sideshow's cost of goods booked and withdrawn, so a drink nets the park 10 and a won play costs it 30
  (Q177c). A shop's ingredient terms are built (Q177d): a drink gives 20 of its 40 thirst back and 2 more happiness,
  and an ice cream quickens the walk for a few sweeps. A Balloon Shop's winner is given a balloon in their own colour,
  with a life from the shop's quality, held over them until it bursts (Q177e); a Costume Shop dresses a guest in the
  theme's costume, and a second visit gives back the child they arrived as (Q177f). `FUN_004fdcc0`'s excitement match is
  built (Q169).
- **One honest limit.** A park left entirely alone still rarely buys a *drink*: only a quarter of guests
  ever grow thirsty (`Peep.Tick` shares the drift by thing id) and by then their exit countdown has
  usually run out — measured, of 148 samples at thirst 50+, **73 were HeadingForExit and only 11
  Deciding**. The shop is chosen the moment a thirsty guest *is* deciding, which
  `ParkRideChoiceTests` pins from four cells across the park. The `thirst` console command exists to
  create that condition, for the same reason `load` exists for the seaplane.

---

## Just behind these

- **29 of 106 opcodes are unimplemented** (77 built, `case Opcode.` labels against the enum, measured 2026-10-10).
  How many of the 29 shipped Lost Kingdom content reaches is answered by the `unimplemented` census.

## Deliberately not on this list

Each was deferred for a measured reason, not for want of interest: the `BUMP` / `COAST` / `HOP` families
(**their rides are not placed in Lost Kingdom**) and the other three themes. Litter's reason expired once the
shops served; it is queued in `docs/QUEUE.md` section F.
