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
Ghidra; `verify-<area>.md` and a lead id name its evidence (`CLAUDE.local.md` says where). A note marked "fork watch"
comes from the daily look at the same fork's new commits (the first, 2026-10-16:
`~/.cache/tpw-harnesses/fork-review-2/watch/2026-10-16/`, `verify-<area>.md`; `docs/TOOLING.md`, "Recipes", the daily
fork watch).

---

## A. Bugs first

**Ordered at Alexah's word (2026-10-20):** the ferry, seaplane and bus made on demand (Q26c, which Q557's rest waits
on), then the rides, shops and sideshows that run for guests (Q580, Q581), then the research screen and golden tickets
(Q705 to Q709), then the sounds; the optional sound measurements Q575, Q576 and Q578 stand at the queue's end.
**Before that, at Alexah's word (2026-10-08):** what a playable Lost Kingdom park needs most comes first (saving a
park, the calendar, the shop's window, the gauge, the research lab), then the rest of this section in its old order.
The review of the run of 2026-10-16/17 put Q444 to Q446 first (done; its findings are
`~/.cache/tpw-harnesses/handoff/review2/q<N>/FINDINGS.md`). Q452 to Q465 come from the week-end audit of 2026-10-17
(`~/.cache/tpw-harnesses/audit-2026-10-17/`, `LEDGER.md`; `CLAUDE.local.md` says where).
The review of the run of 2026-10-17/18 (its findings are `~/.cache/tpw-harnesses/handoff/review3/`, `VERIFY-<n>.md`)
filed the items that cite it.

- [ ] **Q582. A made vehicle takes the id last freed.** From Q26c (and Q238): the original's summons hands a made vehicle
  the id last freed (its ferry from `q340/late30.TPWS` was thing 14 in Q341's run, its seaplane 14 in `q26c/orig2`),
  and `ParkState.NextThingId` gives one past the highest used (43), said at `ParkPeople.MakeVehicle`. Decode the
  original's free list (`FUN_0050b350`, the id's source at the summons) and build it for every thing made, the guests of
  a load too, which part from the original's ids once anybody has gone; then confirm from late30, whose freed 14 the
  original reuses.
- [ ] **Q583. The seaplane floats elsewhere at its stop than the original's.** From Q26c's comparison
  (`q26c/side-by-side-seaplane.png`): at status 2, at the camera 540,-20 zoom 130 rotation 0, the original's seaplane
  lies level at the harbour's top middle, nose left (`q26c/orig3/o-t1141.png`), and ours at the harbour's left and
  turned (`q26c/run-C/C-docked.png`), as main's stood one did (`run-A2/A2-docked.png`). Its route and clips play through
  `TRIGWAITANIM` on the model's Bezier path (`ParkFixedItems`, `LobbyModel.Paths`): find which of the path, the clip's
  end frame or the root's turn differs, against the original's frames of its drive-in (`orig3/o-t11*.png`); the ferry
  was not seen docked in the original, so photograph it too.
- [ ] **Q585. The Mayan Spinner's riders stand in a heap at its steps.** From Q584 (`q580/explore5/v00..v29.png`,
  `q580/run-R1c/R1-spinner.png`). Ours: each rider walks on from node 1 to their own node (2 to 7) and is then Carried,
  and every Carried rider is drawn at the top of the steps, heaped, through the whole ride, while the drum rises and
  spins as the original's does; `rides` reads "heads 0/0" for thing 44 with five Carried. The original's riders stand
  apart round the drum and go up with it (`q580/orig/a-t869..907`, `a-t1265..1319`; the drum's rise `a-t915..985`).
  The Lookout's heads ride its lift (`ride-operation.md`, "Lost Kingdom's legs"); find why ours has no head node for
  them (the Aztec Mayhem's are drawn on its benches, Q257u) and draw a Carried rider where the original does; compare
  frames of both games at 580,240 zoom 70.
- [ ] **Q586. The Sun God's two brazier plates are drawn dark where the original's are gold.** From Q580
  (`q580fs/side-by-side-sungod.png`: ours `run-S2/S-heads.png`, the original `orig/S2/o-t813.png`, both at 412,240 zoom
  70 rotation 0). Ours draws each plate beside the steps as a dark square with its fire in it (`S-sun.png` and
  `S-heads.png`, two moments of the ride); the original's are bright gold diamonds with the fire's glow behind them
  (`o-t813`, `o-t922`, `o-t1120`). Find which differs (the plate's mesh or texture, a clip's state, or the light on it)
  against `incagod`'s model.
- [ ] **Q709. Spending golden tickets: the mystery ride's purchase.** From Q177c, carried from Q141 when it was ticked;
  decoded by Q604 (`hud.md`, "Spending golden tickets"). Build it as decoded: the player's tickets left (counted less
  spent, no floor) and their `RideIds` asked by the buy list, the click, the carry and the purchase; **the price cell's
  setter** (`PriceSkin_SetValue` `0x00490650`: UITEXT 449 "$ {P13}", 455 "{P23} gt" below nought; this takes the "$ "
  of Q238's note too) and **the rows' colours** (`BuyList_CellColourCallback` `0x004acb70`: gold or red for a mystery
  row by the tickets, white or red for any other by the bank, the land rows' (231, 239, 75)); the click on a mystery
  row carrying the ride when the tickets suffice, the bank not asked, and refusing with `cat_ui` `0x1D` when not
  (replacing `MYSTERY_RIDE_PURCHASE`); the carry's price nought and no refusal by money; the purchase spending the
  tickets in place of the price, inserting the id and adding the cost only when the id is new (replacing
  `PURCHASE_GOLDEN_TICKET_ARM`); `gms.dat` written at its next write. The original lists the two mystery rows
  Jurassic Tours' first where this game lists Eruption's first: find the tie's rule and follow it. Confirm on
  `q709/gms-two.dat` (4 tickets, 2 spent, none bought) beside `q709/orig/a` (`q709/confirm.py` and `orig/run.sh`).
- [ ] **Q596. The other topics' gesture rows.** From Q595. Rows 5 to 11 of the gesture table (`AdvisorGestures`;
  `scenes.md`, "Gesture table") are fixed clip lists opening on clip 14 (row 5: 14, 5, 11, 15), named by responses of
  built topics (`GestureRowLine`: 17, 37, 53, 82, 84, 90, 97, 129, 206, 258): hand them to `PlayParkResponse` as the
  ticket lines' are, replacing `ADVISOR_RESPONSE_GESTURE_ROW`; rows 2, 3, 4 and 8 are empty, so their lines keep the
  random chain. Confirm a line of each in the original.
- [ ] **Q598. Every clip's scale track posed.** From Q597. The scale track (`0x80|0x100`; FileFormats `models.md`,
  "Scale") is in 644 tracks of 192 clips, 128 of them in 54 Lost Kingdom clips of 19 wads (`lib/scalescan.py`):
  the shops `fries`, `steak`, `cost_shp`, the rides `lookout`, `monkey`, `porkpie`, `tourride`, `tvsim`, `volcano`,
  `wateride`, `bouncy`, the sideshows `hyenas`, `junspray`, `arc2x3`, `squark`, the features `bigpalm`, `supbog`, `toilet`.
  `AnimationFile` reads them (Q599) and only the advisor poses them; `LobbyModel.Pose` counts the rest, `ANIM_SCALE_TRACK`.
  Three clips carry nothing else and so read as no clip (`IsValid`): Lost Kingdom's `tvsime` and `tvsims`, fantasy's
  `sign1c`. Pose them on every model a clip moves (`NodePoser`, `MeshRotator`) and confirm one of each kind beside
  the original's frames.
- [ ] **Q602. The advisor's row 19, message 0x13, and its line in the hard hat.** From Q708 (`q707/orig/O2/line.log`,
  `q708/PREDICTION.txt` round G2): Alexah's jungle `New Save` loaded on 19004 by her Full Simulation profile with no ticket,
  the original says sample 57 in the hard hat (props 5, 0) on tick 19084, response 57 of row 19 (polled, first response 55,
  three lines, score `0x0059d180`); this game counts the row (`PARK_ADVISOR_POLLED_ROW_NOT_BUILT`), so its advisor is free
  where the original's is busy, and says the hints `0x13a` (19059) and `0x13c` where the original's wait in their slots.
  Decode the score (`0x0059d180`) and build it on the polled rows' machinery; confirm the 19084 line in both.
- [ ] **Q603. The gadget's corner reads "-1 x" tickets where the original's reads "0 x".** From Q708 (`q708/run2-F/F-hints.png`
  against `q707/orig/O2/g-05.png`): with `q707/gms-none.dat` (no ticket, one spent, one key given) this game's corner
  shows the tickets less those spent, -1, and the original's 0. A profile spending more than it won is not one play makes;
  decode the corner's count (`hud.md`, the gadget's update) for its floor and say at the site what it is.
- [ ] **Q587. The karts' and the water ride's track: laid, walked and opened.** From Q581 (`park.md`, "The karts' and
  the water ride's track, buoys and race"). Replace `PLACED_TRACK_RIDE_FIRST_TRACK_CELLS`: the purchase lays the station
  (0xd or 0x12) and both mouths (0xb or 0x10) on the track grid and arms the queue; a click on a piece ("Left-click to
  edit this track") arms mode 0xb or 0x10 with it the anchor; each click lays a straight run, sound 0xce; the walk sets
  `+0x2c` and makes the sections, their collision objects and buoys (`ParkTrackRideTable`); a closed loop with the open
  guard opens the ride, and a later queue edit opens a closed valid one (the tail `ride-operation.md` cites). Decode
  first what it leaves unread: which `gk_trck*`/`wr_trck*` model a piece draws, a piece's cost and refund,
  `FUN_0052c6f0`/`FUN_0052ccb0`. Confirm on `q581/karts.TPWS` by the same clicks as `q581/orig` (stations at (32,25)
  and (20,20), loops to (34,29)/(28,29) and (22,23)/(16,23)): sections and buoys against `q581/orig/A-track.log` and
  `D-track.log`, and the pieces photographed beside the original's (`B-5-closed.png`, `D-13-boats.png`).
- [ ] **Q588. Dino Karts race for guests.** After Q587. The -4 record (`Bumper_LayRide`'s grid and start buoys, the
  template's 3 laps, 2048 arrival), `BUMP 7` to state 1 and the race's first buoy, `BUMP 4` a kart a rider (mesh
  `cars % 4`), the retarget's grid and race arms with the thrust bonus, the buoy arm's laps, state 3 and the end at the
  unload point, `BUMP 14` and `BUMP 3`, the step's clips 5 and 0xc by speed, and the track's objects in the point test,
  the rim and the obstacles (quarters, rectangles, 0x40 circles). Not read, to decode or count: the seat a rider takes
  (`FUN_00549c60`), a kart's height over the track. Confirm against `q581/orig/C-track.log` (the grid, 9 to 16 and 8,
  the timer -3 to 0, state 3, the next kart at the centre) and a frame of a kart on the track (`C-4-race.png`). Moves the
  website's rides list.
- [ ] **Q589. Splish Splash runs for guests.** After Q588. The -5 record (one start buoy, unload point a third of the
  way to it, 512 arrival, 64 boats), `BUMP 7` straight to state 2, a boat carrying the whole boarding list straight to
  the race, one lap, removed at the unload point with the ride left at 2, and the boat's spin (`+0x58` set at a rim or
  an obstacle, × 0.998 a tick, turn `+0x58 / 128`; the term the contact sets is not read). Confirm against
  `q581/orig/D-track.log` and `D2-track.log` and a frame of a ring in the channel (`D-13-boats.png`). Moves the
  website's rides list.
- [ ] **Q592. The cheat table's twelve rows.** From Q591 (`park-engine.md`, "The cheat table"): each is counted on its key's
  release with Alt alone (`Level.CheatRows`: `CHEAT_RESEARCH`, `CHEAT_MONEY`, `CHEAT_LOCAL_TICKET`, `CHEAT_GLOBAL_TICKET`).
  Build them as the original's after Q720 settles whether the keypad's digits reach their rows and the cheat flag's
  first-key clear is run in the original; whether this game's R binding also opens the research screen under Alt
  (the original's row needs no modifier, matched by `==`) is to be checked in the same session.
- [ ] **Q594. A loaded coaster's track and pylons are drawn.** From Q706 (`q706c/side-by-side-coaster.png`: the
  original's `q706b/O2/o-t19061.png` beside ours `q706c/run3-A/A-coaster.png`, both at 310,355 zoom 130 rotation 0, Alexah's
  jungle `New Save`). The original draws the Temple Of Gloom's track over its 38 pylons; ours draws neither, though Q706
  rebuilds the track's knots, curve and samples from the save (`ParkCoasterHeights`, `ParkCoasterPylon`). Build the
  pylons (each piece's `StdPylon` posed by its loft, rotate, incline and bank clips at its cell, as `ParkCoasterPylon`
  poses the knot) and the track's mesh from the samples and the `coaster.sam` cross sections (FileFormats `sam.md`, "A
  `coaster.sam`'s tables"; the mesh builder not decoded: decode first). Confirm by the same two frames side by side.
- [ ] **Q557. A script's sounds of the other voice classes, and of a script with no nodes here.** From Q553, which built
  the one-shot and the plain class, Q558, which built the chain of `0x4` alone, Q559, the held chain `0x404`, Q561,
  the music's class `0x406`, Q563, a voice out of range, Q564, a ride's scream out of range, and Q565, a script's
  one-shot out of range (`audio.md`, "A script's sounds"). Counted and not started:
  `_6BE680` and `_6BE330` (none in Lost Kingdom), type 10 (`RIDE_EFFECT_SOUND_ITEM_BANK`, no shipped script) and a
  placed sound with nowhere to stand here (`RIDE_EFFECT_SOUND_NO_PLACE`: the ferry's 172 and the seaplane's 173, ADDOBJ 8
  on node 1). Both are of the plain class (flags `0x6`), already built: what they lack is nodes, as `ParkRides`' second
  pass bound the vehicles the file does not name with no `Nodes`. **Since Q26c** both are made at their first summons
  through `ParkRides.BindNew`, which gives them nodes: in `q26c/run-B2` and `run-C` the log reads "Park audio: script
  16's effect 172, sample 'mk_ferry_1.mp2' ... plays" and "effect 173, sample 'mk_plane_6.mp2' ... plays", and main's
  runs counted `RIDE_EFFECT_SOUND_NO_PLACE`; not heard and not set beside the original's: confirm that here.
- [ ] **Q567. The original sounds the global rides' 208 at the sideshows (decode first).** From Q565. In the stock park
  the original made 208 (flags `0x0`, one sample of 1158 ms) 30 to 60 times a run at (525, 0, 300), by the Jungle Spray,
  and (434, 0, 303) (`q565/orig/`, b to d, `oneshots.py`); no jungle script asks for 208 (`lib/soundsweep.py`), and ours
  never plays it. Find its caller (a `Sound_PlayEffect` of category `[0x00803a28]`, effect 208) and what it marks.
- [ ] **Q577. The sound commands 2 and 3: a script voice's drop suspended on focus loss and by the P key's pause.** From the
  run 6 review of Q565 (VERIFY-19, finding 3). The window losing focus (`0x0046b73c` -> `0x0051bcf0` -> message `0x00700b64`
  -> `0x006b5640`, type 2) and the P key's pause (`Game_TogglePause` `0x00409350`, its pause branch `0x004093af`) send
  the sound service command 2; their resumes (`0x0046b70a` -> `0x0051bd30` -> `0x006b5670`, type 3; `0x00409381`,
  `Game_Resume` `0x0040932b`) send 3 (`0x006bcab0`). Command 2 suspends a voice's drop only while its `+0x3c` is nought
  (`0x6bcafd`/`0x6bcb02`); command 3 leaves `+0x3c` and flag `0x2000` set, so the drop stays far out until a channel
  start rewrites `+0x3c` (`audio.md`, "A script's one-shot"). Ours sends neither: its script one-shots are dropped by
  their time through a focus loss and a P pause. Decode what each command does to the other voice classes, then build
  them where ours has the two paths (find first whether ours pauses on focus loss at all). Confirm: in the original,
  a one-shot away from the camera kept past its length + 250 ms across an alt-tab (`oneshots.py`).
- [ ] **Q560. A park file's live voices put back (decode first).** From Q559. Every park file carries the sound
  manager's voices after the sound module's 84 bytes (`Sound_SaveModule` `0x0051c350` through `0x006b5c10`), and the
  original's load puts them back (`Sound_LoadModule` `0x0051c400`, `0x0051c497` -> `0x006b5c30` -> `FUN_006ba150` ->
  `SoundManager_SerializeVoices` `0x006b6da0`, which only reads, a case per class: 6 a held chain; the writer is
  `0x006b6c30`). So the stock park's gates' held chain 177 sounds from the entry in the original (`q559/orig/` runs c
  to g: a saved child of variation 8's sample, and the chain's own variation drawn afresh at the load, v8, v7, v1 and v2,
  as its set-up redraws `+0x8` after the load restores it, `0x006bbfc5`), where ours makes none: its gate script is
  resumed open and reaches no `EVENT`. A held chain is to be put back with its saved child's sample, a fresh draw for its
  own variation and no flag `0x20`, so the first time its child list empties it restarts at variation 1, once
  (`audio.md`, "Which variation it plays"). Counted at every park read: `PARK_FILE_SOUND_VOICES`. First session: decode the records (FileFormats `saves.md`, "Sound": 1,349 bytes in
  `Easymode.TPWI`; `docs/exe/saves.md`'s `SOUN` row) and which voices they hold in the stock park and in Alexah's Full
  Simulation saves (the gates', the crowd's, the rain's, a scream's), and whether a voice's handle in a script's record
  or the weather's `mRainSoundHandle` is matched to one put back. Second: build what the jungle's files hold, and say what
  this game's writer leaves out.
- [ ] **Q719. Does a loaded park play at the save's sound volumes? (one game check)** From the fork watch, 2026-10-20
  (`verify-world.md` M2; `saves.md`, "Sound"). The loader `0x0051c400` overwrites the live sound words at `0x00803aa0` with
  the save's (the volumes and the three on-flags; all 558 park files on hand hold 100/75/60/75 and on/on/on) and sets
  nothing else; the tick `0x0051bd70` pushes them. The profile applier `0x00423dd0`, which would put the player's options
  back, is reached only from `GameOptions_Construct`, the options screen and two table references, and from no load
  path found. If so, a loaded park overrides the player's volume options until they are touched. Confirm in the original
  under Proton: set music off in the options, load `Easymode.TPWI` (or a copy), and listen or record the mix
  (`record.sh`); then decide whether OpenTPW's load should do the same (it keeps the player's options today) and say the
  result at the site. Nothing is built before the check.
- [ ] **Q723. The sample draw as `FUN_006bc680` draws it.** From the upstream look, 2026-10-19 (`verify-sound-ui.md`, "The
  music roll rule"; `audio.md`, "The sample draw"). The original rolls 0 to 65,535 and walks the samples for the first whose
  odds are at least the roll, and plays NOTHING when none reaches it; with the avoid-repeat argument set and more than two
  samples a pick equal to the last moves on to the next. `SoundCategory.PickIn` rolls 0 to the last odds, so a list always
  plays, and has no avoid-repeat step. The first is a deviation of at most 20 in 65,536 (201 of the 1,592 lists in the 31
  SFX maps end below 65,535, the lowest at 65,516), said at the site and not worth a change on its own. The second matters
  for the lobby: all four `cat_locallobbymusic` effect 1 hold one variation of three samples (odds 21,845, 43,690, 65,535,
  flags `0x206`, the plain class that asks with 1), so the original's lobby music never plays a track twice running, and ours
  can. None of the voices that ask for it builds the step (the lobby's music, and the land's, the crowd's and the park music's voices; `audio.md`). Build the
  draw once (rolled 0 to 65,535, no sample past the last odds, the previous-pick step where the class asks) for every
  caller, or count the unbuilt step `SAMPLE_NO_REPEAT` (rule 4) until it is. Confirm: the lobby's music over ten
  selections, never the same track twice running, caught by the disk driver.

- [ ] **Q562. The bus drives facing along its route, and stands at the original's stops.** From Q561. Our bus drives its
  route turned as it was parked (the route key's facing `0x400`, counted `ANIM_PATH_FACING`; `park.md`, "Clip content"),
  where the original's faces along the road (`q561/side-by-side.png`); and it stands at (510, 65) and (422, 65), the route's
  points at its clips' last keys (42.437 and 55.996), where the original's bus voice stood at (529, 64) and (439, 65),
  19 and 17 back along the route. Decode the route sampler and the facing (`FUN_00474840` and its callers) against
  ours (`LobbyModel.SampleRoute`), measure the original's bus matrix at its stops, and build the facing for the drawn
  bus and its nodes (`NodePoser`). The seaplane's route too.
- [ ] **Q713. The advisor's shop and sideshow value and satisfaction rows, 238 to 257.** From the fork watch, 2026-10-19
  (`verify-advisor-ui.md` L1; `advisor-park.md`, "Shop and sideshow value and satisfaction, rows 238 to 257"). Twenty
  polled rows, counted in `PARK_ADVISOR_POLLED_ROW_NOT_BUILT`; rows 239 to 241 were not opened, so read those three
  callbacks (`0x0059fbd0`, `0x0059fc40`, `0x0059fcb0`) first. Build them on the built rows' machinery
  (`ParkAdvisorMessages` topics with `Polled` and `ScoreOf`): the helpers pick the best or worst shop (`UsageInfo.ShopType`
  2 food, 3 restaurant, 4 drinks, 1/5/6 shops) or sideshow (`Info.WhichUIType` 2) older than 30 days by
  `ParkObjectRings.Satisfaction` or by the walk-away percentage (the sum of the last 30 days' walk-aways x 100 / the sum of
  the customers, unsigned; the lowest for `0x004cc090`, the highest for `0x004caae0`; strict, the first of equals).
  **Said at the site as the original's quirk:** the "great value" rows 248 to 252 fire when walk-aways pass 80% of the
  paying customers (shipped line 80, 5 a point), not when value is good. The poor rows 238 to 247 score negative with the
  shipped settings and never play; build them anyway, as faithful noughts. Lost Kingdom reaches rows 248 to 257. The
  `Advisor.sam` key names against the settings words `+0x4d8`..`+0x574` are unproven (the settings reader's map was not
  read): match them with `lib/advisorschema.py` before naming a field. Confirm: a shop older than 30 days with a high
  walk-away share in a park file made for it, the row's poll line and score beside the original's.
  - **Note (Q708):** the original says row 257's sample 525 (props 8, 0; response 497) on tick 19040 of Alexah's jungle
    `New Save` loaded on 19004 by her Full Simulation profile with no ticket (`q707/orig/O2/line.log`): a ready case.
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
  `tp_sideshowwin_`, `mortar_2` and `mortar_3` in global `RideHD.sdt`. Their lengths come out 4/3 too long, which
  `SoundCategory.Length` and the scan a category falls back on (`SoundCategoryFile.ReadSamples`) read; since Q566 the
  categories take their records from the walk, so global lobby sfx 4 and global rides 202, 203, 211 and 212 play. Read the version and take MPEG-1's tables
  for it; the two `SoundCategoryFile` remarks that rest on the lengths ("every sample in every bank", "26 to 83ms")
  change with it, and the version split goes to FileFormats `sounds.md` (rule 14). Since Q566 the walk is what a
  category plays; the scan is only its fallback. The fix moves what Q566's test pins
  (`ParkScreamChainTests.EveryWalkedSampleRecordNamesASampleOfItsCategory`: 99 effects read otherwise and 7 left empty;
  global lobby sfx 4 and global rides 202, 203, 211 and 212 should then agree, hallow's and space's stay), and the
  `SoundCategoryFile` summary's "the two readers agree except in four categories" (a forecast, not measured). Confirm: a test over every shipped bank that each entry's length agrees
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
- [ ] **Q570. The calendar compared after each sweep, and the weather ticked inside it.** From the run 6 review of Q149
  (`review6/VERIFY-7.md`, Q149 1 and the hunt's new finding). The original's `Calendar_Update` (`0x004f8260`) runs at
  the tail of every sweep (`0x00516695` -> `0x004d7b3b`), so in a frame of two or three sweeps whose day turns before
  the last, the objects' rings roll and the month's bank and staff work turn between two sweeps; ours compares once a
  frame after the entity walk (`Level.Update`, `GameCalendar.Update`), so the later sweeps count into the closing day.
  And `ParkWeather` ticks off `GameCalendar.Counter`, which moves only after the walk, so its ticks for a frame's sweeps
  run in the next frame and read `GameCalendar.Days` as compared after the last of them; the original's
  `FUN_00512880` runs inside its sweep, before that sweep's compare. Both are said at the site. The sweep is split
  across the entities (`ParkPeople.OnUpdate`, `ParkRides`, `ParticleSystem` each loop over `GameClock.TicksDue`), so the
  fix is one sweep loop that runs every entity's sweep work, then the weather's tick, then the compare and
  `RollTheDay` / `TurnTheMonth` / `TurnTheYear`, per sweep. The weather's day phase is moot once Q529 schedules on
  instants; its one-frame lag is not. Test: a frame of three sweeps whose day turns on the second rolls the rings
  before the third sweep's settle-up (put the once-a-frame compare back and see it fail). Reached only in a frame
  owing 16 or more ticks (a hitch); no game run shows it unless a hitch is forced (the console's `pause`/`step` or a
  stopped process, as `park-engine.md`'s "+5, +5, +6" measurement did).
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
  original's way. Confirm: the staff screen at a known happiness, photographed. Note (Q244): `UiMeter` is now the
  gadget's gauge alone and its fill direction is read (`hud.md`, "The happiness gauge's two quads"); the remark cited
  above is gone.
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
  - **Note (fork watch 2026-10-18, `verify-other-a.md` H2, `verify-audio-fx.md` F2):** decoded, `audio.md`, "The fade, the SoundInfo
    settings and the reverb". `FUN_0051c8d0` is the reverb preset holder (`[0x00803ab4]`, posts `0x700b30` only on a
    change), and `[0x0078578c]` is the level key `Sound.Reverb` (0 in `Standard.sam`, 3 in hallow's, none in jungle's,
    so Lost Kingdom's is 0). A negative ride `+0xd8` sends nothing. The preset reaches EAX 2.0 hardware only; reverb
    itself stays cut (`audio.md`).
  - **Note (Q580, 2026-10-20):** the Sun God reaches `SETREVERB` (`incagod` at 162 and 188, in every run of it), counted
    at the dispatch default as `Inca God: SETREVERB at 162`. Building it is the store of its operand at the script's
    `+0xd8` for the preset holder above; with reverb cut nothing would read it, so a name of its own in the count is all
    it needs (`q580fs/run-S2`).
- [ ] **Q158. Five park-screen readouts are stand-ins, and nothing counts them.** Found by the 2026-09-26 staleness
  audit (`CLAUDE.md` rule 4). `ParkObjectWindow`'s Age row prints a bare number: its wording, `FUN_006acd60` with
  `0x1b1`, is not decoded. Its scrap row prints the build price, where `FUN_004e2400` scales it by the age bucket's
  percentage, `SCRAP_VALUE_DEPRECIATION`, counted only when something is sold. `ParkEntryPriceScreen`'s heading
  `0x4f3af` is empty, and its remark says "left empty and counted"; the original titles it with the name of the thing
  at world `+0x1da732`, `mParkGates` (`ParkWorld`), and its help row 191 reads "Park name - left-click to edit", so
  the title is the park's name and its edit is Q28's. `ParkGadget.ShowDate` prints the machine's short date (Q527). `ParkBuyScreen`'s description panel `0x1ea` draws its frame and the footprint picture, and neither the item's turning model nor its name row `0x1ec`. Count each
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
  From the run review (2026-10-17): three more callers whose tests stop at the helper. `ParkBuildMarkers` hands
  `RecordHeight( square.Lift )` to the strip's `Over`; put back `square.Lift` and every test passes, and the game's `strip`
  census prints `Over - Lift`: assert a built strip's `Over` for a lifted cell (21.5), or print `Over` (Q435).
  `ParkBuildMarkers.Fill` needs no GPU and runs before `Build`'s first GPU call, so Q436's mutation m12 (the vertex count)
  is not "game-only": move the count into a testable `VertexCount( squares, walls )`. `CameraLensTests.
  TheLobbyDrawsItsFilesAngle` asserts 100, which `LobbyCameraMode.ReadSettings( TextReader )` also falls back to, so a wrong
  path or an unread stream passes: open `lobby/lobby.txt` through `FileSystem`, assert it is found, parse that stream (Q441).
  `ParkStaffCellListTests` (Q446) demands a relink from a member in the Held state, where the original relinks only in
  its walk tick and a placement: drive a one-cell walk in Walking instead, so a relink inside `Walked` passes.
  `Level.cs`'s hand-over of the gadget's locator to the level (Q444) is reached by no test (`ParkLocatorTests` sets
  `level.Locator` by hand); build the test level through `ParkFrontEnd`, or add its deletion to Q444's mutations as
  caught by the game alone. `ParkStaffWindowTests`' refused-sound test (Q442) reaches the path and queue refusals beside a
  park screen by clicks the game refuses (`Level.WorldClick` returns on `KeptFromThePark`), so `RunPath( cellX, cellY, true )`
  at the `RunBuildMode` call survives: arm the tool before the window opens (`ParkBuildMode.Arm( ParkBuildMode.Path )`; the
  queue by `ParkPathBuilding.EditQueue( 13 )`), then click, reword the summary to "armed", and put that bug back.
  `Level.cs` hands `state.RandomSeed` to `ParkFileWriter.Running` with no test on the wiring (Q443; `q443/mutations.py` declares
  `LV` and uses it for no mutant): take the argument out and the suite passes; assert the written header's seed.
  The person windows (Q430 to Q432): Ctrl+V over the visitor's window (`ParkFrontEnd`'s case 14 branch; drive the action,
  assert the locator is on the guest and the window closed, put the branch back out); `Level.ClickWorldAt` with a staff
  thing (deleting its branch survives every test); `ParkStaffWindow.Open` with the window already up (it stays on its
  member and posts `0xc0` again); and PICK UP's "not drawn", shown by the census predicate alone (the draw loop's
  `InTheHand` skip in `ParkGuestSprites` survives; show his cell on screen).
- [ ] **Q212. The ride window's other statuses. Decode first.** Found by the 2026-10-04 review (`u7-ride-door-6`,
  `-7`, `-9`, `-10`). `FUN_00485f60` answers (80/100/255) 5..9 for a ride waiting for an upgrade, `0xb`..`0xf` for a toilet's
  call to the handyman and `0x11`..`0x15` for a mechanic called (`+0x64`), `0x15` (UITEXT 385, "REPAIRS IN PROGRESS")
  only while the worker is at work; 2 (UITEXT 366) for state 1 broken down with its assignee not at work, whether or
  not one is assigned; 24 for an exit not connected, asked whatever the open word, so an open ride with its exit cut
  off shows 24 (ours returns 0 for an open ride before that arm). The order and the tests are in
  `ride-window-door.md`, "The statuses behind Q212" (fork watch 2026-10-19, verify-advisor-ui.md L2, verify-world.md
  L1). `ParkClosedStatus` answers 1, 22 and 23 only, shows grey CLOSED for a mechanic call, and
  `ParkObjectWindow` shows BROKEN DOWN from `VAR_BROKEN` with no decode behind it. `OBJECT_WINDOW_EXIT_CONNECTION_STATUS`
  is reported every frame a window is open (9,631 in one run; `QUEUE_VERDICT_CORNER_RULE`, the other, is built since,
  Q538): report on a change. Confirm: each status photographed beside the original's.
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
  the original's fresh park. The stock park's boundary is not drawn either (Q469: the wall right of the gate in
  `q469/orig/a-4-cam180.png`, open grass in `q469/run1/2-hoarding.png`).
  From Q494: the picker's object march meets the boundary's hoarding in the original (ho_fos1.MD2, its track kind 25
  anchors (43,17) and (51,17), at 7 pixels right of the gate in `q494/orig/a-scan.log`); ours counts
  `PICK_OBJECT_MARCH_TRACK_THING` on every cell of kind 11, 12, 16, 17 or 25 and answers the ground there. Once the
  hoarding is drawn, `ParkPicking.CandidateOn` asks its model first (`FUN_0053bf30`'s `+4`).
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
  upgrade (`+0x284`) and model-failed (`+0x508`) tests or count them (`FUN_004aaf70`); the upgrade test is reached
  since Q349 (Alexah's Full Simulation saves), so it is Q452. (3) `ParkAudio.ScreamEffectFor`
  silences a negative band; `FUN_00551130` plays `0x48` for one. (4) `RideScript.NextRandom` answers 0 for every
  bound + 1 at or below nought; only -1 divides by nought (`0x00553994`). (5) `ParkItemHeights.Over` floors the
  cell's middle where `0x00452bb0` truncates corner offsets: measure it on raised ground. (6) The original rolls a
  rider's head sprite to its node (`FUN_0044b510`, `0x0044ba04`); `DrawHead` does not, and it is counted (`RIDER_HEAD_ROLL`): build
  it. (7) `Peep.Pace` works at 24 bits and `ShopCostOfGoods` at 53 under one control word: settle which, the pinned
  18 may be 19 (the shop window's amount slider now sets an amount off the steps of 50, Q31). (8) Two docs lines not yet re-measured: `park.md`'s selector table rows 8 and 9 leave out the
  `0x4000` and type gates of `FUN_00544c80` and `FUN_00544e50`; FileFormats `sound-categories.md` says the masks use
  bits 1, 2 and 4 beside 645 speech variations holding 100.
  - **Note on (7) (fork watch 2026-10-18, `verify-world-a.md` H2; `park-engine.md`, "Which rounding is live is not
    settled"):** the cause of the 24-bit reading is not settled, and our own 53-bit reading stands beside it: a breakpoint
    at `0x0059e17b` in the running original read the control word `0x27f` on ticks 970 and 1125
    (`advisor-park.md`, "The product is a double of the float key"), while the game-clock accumulator is added at 24 bits
    (measured). DirectDraw's cooperative level is set with `0x851` (`0x00563914`) and `0x808` (`0x00563b92`), both with
    FPUSETUP and neither with FPUPRESERVE; the renderer's own 24-bit switch is `0x00591220` (a failed frame leaves it in
    force, `0x00577433`). Which one a given calculation sees is still to be read, in the original, at that calculation.
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
  - **Note (fork watch 2026-10-18, `verify-other-b.md` L2):** `FUN_004d93b0` is read whole in `ride-operation.md`, "A cell's
    litter": it returns at once online and when the cell already holds litter; the kind picks one of eight sprite programs
    (kinds 1 to 5 their own, 6 and the "Dodgy litter type" log share one, 7 and 8 stamp region effects 2 and 5 first, 8 also
    particle `0x32`); the cell keeps the sprite (`+0x28`), the kind (`+0x2c`) and nought (`+0x30`); and **it reseeds the park
    generator with the cell id**, then draws twice for the place in the cell (`10 x cell + 1 + r % 9` on each axis), so a cell's
    litter always stands in the same spot and every drop resets the shared generator, which the generator mirror must copy.
- [ ] **Q226. Nobody is sick.** Found by Q111, arm (b). Illness exactly 100 on a third of deciding turns: spot
  animation 7, litter type 7 on the cell, event `0x12`, sound `0xcc`, illness nought, and the turn ends. After Q225,
  which builds a cell's litter. Build it where `DECIDE_VOMIT` is counted. Confirm: `need vomit 100`, a guest sick
  within a few turns, `peeps` showing illness 0; a screenshot.
  - **Note (fork watch 2026-10-18, `verify-other-b.md` L2):** the sick are litter kind 7: record `0x0074f140`, region effect 2
    stamped first, then the sprite placed by the cell-id reseed and two draws (`ride-operation.md`, "A cell's litter"). The drop
    does nothing on a cell that already has litter, so a second sickness there leaves the first.
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
  - **Note (fork watch 2026-10-16, `verify-advisor.md` lead B):** the advisor's half of the bus message is decoded
    (`advisor-park.md`, "Rows settled from the bytes"): `ReceiveMessage` arm `0x0059b221` takes the guest's id and
    packed cell and switches on the message's `+0xc` (the guest's id % 3): 0 posts advisor message `0x7d`, 1 `0x7e`, 2
    `0x7f` (rows 125-127, getters `0x0059f360`, `0x0059f370`, `0x0059f380`, keys `PrankSbomb`, `PrankLitter` and
    `PrankBalloon` `.Score`, 35 each, above the 25 cut). The prank done is `mPrankeryIndex` 100 + id % 3, so the
    advice names the prank done for a guest the constructor made.
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
  From Q495 (`hud.md`, "The buy screen's help rows"): the fill's pick is also the buy screen's help item, so in the
  original the first row's help row is posted as soon as the pointer comes onto the row area; ours posts nothing before
  a row is selected. After a tab's click the original's bar was hidden with the pointer on the tab (`q495/orig/`,
  O9), where ours keeps the tab's row: look at it with the pick.
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
  - **Note (fork watch 2026-10-19, `verify-audio-fx.md` M2; `hud.md`, "Which window the click sound is for"):** the bar's
    arrows are repeating buttons (flag `0x21`): first repeat after 500 ms, then every 125 ms (`+0x13c`, `+0x140` of every
    button). An arrow clicked without repeating plays `Select3` (`0xbd`, the bar's flags `0x10`); one held past 500 ms
    plays **nothing**, neither at the repeats nor on release (the repeat messages are not `0x100`, and the release posts
    none after a repeat). The thumb and the track play nothing.

- [ ] **Q249. A park that was never loaded from a file has nothing to carry.** From Q241e. The fresh world (a
  Full Simulation park, or a player whose folder holds no park file) is made by `FreshPark`, so the writer's
  carried modules have no source. Decode what each module holds in a `restart.INTS` written at tick nought
  (three are in Alexah's saves), make each afresh, and take the legal text from the level's shipped park file.
  After Q241j. Confirm: a fresh Full Simulation park saved and loaded by the original under Proton.
  From Q241f: `Level.WritePark` counts such a park as `SAVE_PARK_WITH_NO_FILE` and writes nothing.
- [ ] **Q569. Where the original puts down a worker held at a save, decode first.** From the run 6 review of Q241j (VERIFY-3,
  "anything the first reader missed"; unsettled). The world writer installs the idle mode over the hand
  (`FUN_0046c350` at `0x00516d13`), which runs the outgoing mode's uninstall: for a worker picked up, the type-6
  mode's vtable `+0x2c`. Ours puts them back on the middle of the cell they stand in, the one they were picked up
  from (`ParkHand.LetGo` -> `ParkPeople.PutBack` -> `DropStaff`), and writes them there. Neither `docs/exe/` nor the
  commit shows that the original does the same (`PutBack`'s comment names `0x0046cdc0` as that uninstall, unshown). Decode the type-6 mode's `+0x2c` in Ghidra (vtable `0x006fea70`):
  where the worker goes (their pick-up cell, the cell under the pointer, or left where they hung), whether they are
  set idle and whether a mechanic goes to his job search there too. Write it in `docs/exe/park-engine.md`, "The
  hand's ways out", and only then change `PutBack` if it differs. Confirm: the original under Proton saving with a
  worker in the hand, read back with `parkfile.py` (their position and state), against ours
  (`ParkSaveScreenTests.AWorkerInTheHandIsPutDownBeforeTheParkIsWritten` holds ours).
- [ ] **Q571. What the original's autosave holds when its window is closed under the End.** From the run 6 review of Q552
  (`handoff/review6/VERIFY-12.md`, Q552 3). Ours writes `autosave.TPWS` on a window close with the End up
  (`Game.cs` -> `Level.WriteParkOnLeaving`), with no End in it and its two scripts not written
  (`SAVE_PARK_SCRIPT_MADE_SINCE_THE_LOAD`), said at `ParkEndFeature` and in `park-engine.md`, "OpenTPW builds it". The
  original's End is a whole object (`0x004db090`, registered by `0x0050b350`, handle `0x007cf4f4`) and nothing found in
  objdump skips it in the state-`0xb` save. Measure: in the original under Proton, `q550/alldone576.TPWS` to tick 800
  (the End up), close the window, then read its autosave with `parkfile.py` (is thing 43, catalogue 1605, in the object
  list; are `End.RSE` and `AnimCtrl.RSE`'s states written). If it holds the End, decide with Alexah whether ours writes
  it too (and what loading a park with the End in it does in the original); otherwise say at the site that both leave
  it out.

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
- [ ] **Q572. An additive model's rotation key (`ANIM_ADDITIVE_ROTATION`).** From the run 6 review of Q554 (`handoff/review6/VERIFY-13.md`,
  Q554 5). On a model whose header word `0x30` carries `0x4` the engine composes a rotation key with the node's own turn
  (`FUN_00470d10`) rather than replacing it. Ours counts it, `ANIM_ADDITIVE_ROTATION`, in two places: `NodePoser.Pose`
  (one rotation track in the 22 additive clips that carry a position track) and `LobbyModel.Pose` for a clip that only
  turns, which `MeshRotator` would pose as a replacement (49 such clips, 50 tracks: the coaster pylons, Lost Kingdom's
  `coaster1` StdPylonI/R, `coaster3` looppylonr1/r2 and stdpylonb/i/r, `minecart` LoopPylonR1/R2 and StdPylonB/I/R among
  them). `ParkCoasterPylon` (Q706) loads the pylon models for Global 0's measure and poses its knot's turns itself, where
  the knot and its parent have no turn of their own. `FUN_00470d10` (`Anim_ComposeAdditiveTurn`) is a 3 x 3 product of the
  key's matrix (`Anim_QuatToMatrix3`, `0x00474120`) and the node's own rows; decode the order exactly and whether the
  additive restore `FUN_00473c70`/`0x00473d0c` puts the turn back every advance, then build it in both posers when the
  coaster pylons are drawn (Q594). Confirm by a pylon's turn beside the original's.
- [ ] **Q573. A node a clip's route moves, followed (`RIDE_NODE_ON_A_ROUTE`).** From the run 6 review of Q555 (`VERIFY-14.md`, Q555 5).
  `RideNodes.Follow` poses a thing's node tree for its particle records through `NodePoser`, which reads rotation and
  position tracks and no path track, where the drawn model moves a node along its route (`LobbyModel.Pose`,
  `MoveAlongRoute`); a carried find of a node on a route, or under one, reads it where it stands without the route,
  counted `RIDE_NODE_ON_A_ROUTE` in `RideNodes.Stored`. Lost Kingdom's route clips (`bus`, `Ferry`, `seaplane`) start no
  particle record; whether any other clip with a route carries one is not measured. Measure it across the shipped
  scripts and clips (`cliptracks`, the `.RSE` corpus); where one is reached, pose path tracks in `NodePoser` as
  `LobbyModel` does and drop the count.
- [ ] **Q574. One node tree for every channel of a drawn model.** From the run 6 review of Q554 (`VERIFY-13.md`, Q554 2). The engine
  keeps one matrix per node for all of a model's channels; ours poses a clip that moves nodes through `NodePoser` and
  places every mesh from it (`LobbyModel.PlacePosed`), and a clip that only turns through `MeshRotator`, so a
  rotation-only clip on a channel posed before a node-moving one is drawn at rest until it is posed again, the next frame
  (said at `LobbyModel.Pose`). No Lost Kingdom model shows it: the Inca Totem's rotation-only channels are 1 to 3, above
  its node-moving channel 0; other themes not swept. Sweep every multi-channel script's roles against its clips; where a
  model is reached, pose every clip of a model that has a node-moving clip through the one tree.
- [ ] **Q725. The Lost Kingdom speakers' raw vertex frames are skipped.** From the upstream look, 2026-10-19
  (`verify-models.md` L3; FileFormats `models.md`, "Raw vertex frames"). `AnimationFile` skips every track that sets bit
  `0x4000` (30 in the game); Lost Kingdom has the 8 of `speaker1c` to `speaker4c` (4 x 16 vertices and 4 x 8). The data is
  read: each track's positions are whole frames, the descriptor's `+0x0C` plus one of them (30 of 30), indexed by the track's
  index table. Classify first (rule 3): GAP if a placed Lost Kingdom thing plays a speaker construction clip, otherwise dead
  by CONTENT. **Not settled:** whether the original interpolates between the whole frames (the second fork review reads
  `0x00471aa1` and `0x00471943` as blending one address with t and 1 - t; not re-read here) and whether `+0x0C` or the
  block's last frame bounds the frames (equal in all 30). Re-read both in Ghidra into `docs/exe/` before building; until then
  count the unbuilt path `RAW_VERTEX_FRAMES` (rule 4). Confirm: a speaker's construction beside the original's.

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
  - Built by Q241j: `Level.WritePark` writes `gms.dat` first and installs the idle mode before the park, as the
    original's save does on every call; nothing of this bullet is left.
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
  - **Note (fork watch 2026-10-16, `verify-world.md` L14):** the separation pass is `0x005102f0`, called directly
    (`0x0050e166`) with radius 2 x `[thing+4]` and callback `0x0050dfc0`: a window of +-ceil( radius / `0x10000` )
    cells (3 x 3 only for a radius of one cell or less), rows then columns, one thing list per cell (`cell+0x2b8`,
    next `+10`) in its own order; kinds 1, 4, 5, 6, 7, 8 and `0x12`; itself excluded; distance non-zero and strictly
    inside the radius; it stops after the **fifth** neighbour. Whether guests reach it is not read.

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
  - **Note (upstream look 2026-10-19, `verify-images.md` L1):** the ink block is the letters' EDGE, not their colour. The
    letters are filled from the line's 16 x 128 fill (bytes 1 to 3 are R, G, B, tiled, `FUN_005eb8a0`; `park.md`), the ink
    shaped (radius, sigma, dx, dy) under them. Photo: the Wonder Land board's "Wonder" is lime and "Land" yellow-orange,
    both edged olive, and `Fan_gate.sgn`'s two ink blocks are the same olive over two different fills. FileFormats
    `sgn.md` "Line ink" and the `SignFile`/`SignTexture` comments now say so; `SignTexture` still letters in the flat ink
    colour. Settle in Ghidra first, in `FUN_005ecb40`, for modes 1 and 2: the call order of `0x5EBF10` (shape the ink mask),
    `0x5E7F60` (blend the ink through it) and `FUN_005eb8a0` (fill the unshaped glyph mask); whether `0x5EB8A0` blends by
    coverage or overwrites and what it writes into byte 0; how `+0x24`, `+0x28`, `+0x3c` map the 128 fill rows onto the line
    (its height or the whole board); and in mode 2 (`0x5E77C0`, `0x5E7EF0`) which line's fill and ink are used (19 of the 27
    `(2,2)` signs have two different fills; Space's photograph looks like fill 1, unproven). 162 ink blocks ship, 101 with a
    non-zero offset. A hypothesis only: 47 of the 168 fills have byte 0 = 0 throughout (the rest 255), among them both of the
    jungle park gate's (`gates.sgn`), `Jun_isle`'s and five jungle ride boards'; a fill byte 0 of 0 landing in the
    A4R4G4B4 alpha nibble would blank the letters, a rival to `textures.md`'s blame of Wine's GDI fonts for the original
    under Proton lettering no name on the jungle gate. No Proton frame of a byte-0-is-255 board's lettering was found to
    split the two; film one.
- [ ] **Q722. A park's name split over the sign's two lines at the space nearest the middle.** From the upstream look,
  2026-10-19 (`verify-images.md` L4). `SignTexture.SplitName` splits at the FIRST space; the original searches outward from
  the middle of the string for a space, alternating (`FUN_00467a80`; FileFormats `sgn.md`). Lost Kingdom reaches it: "Tom
  Tom Twister" is "Tom" / "Tom Twister" here and "Tom Tom" / "Twister" by the centre rule (the space at index 7 of 15). Q399
  covers where the text comes from and its widths, not the split. **Not settled:** the tie. For a name whose two nearest
  spaces are equally far from the middle ("The Hot Pot": spaces at 3 and 7 around 5) the left gives "The" / "Hot Pot" and
  the right "The Hot" / "Pot"; read `0x00467b41` in Ghidra for the start index (len / 2 or not) and which side is tested
  first. The comment on `SplitName` says the deviation. Confirm: a three-word name on a gate or a ride board beside the
  original's.

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
  - **Note (fork watch 2026-10-16, `verify-audio-fx.md` A3):** the list rows' click sound is built (`UiSounds`,
    `WindowStack`'s hook `0x00485780`): the control's message `0x100` handler (`0x004857b8`) plays `Select3` (189,
    `0xbd`) when style bit `0x10` of `[this+0x48]` is set (`0x004857cb`), else `0x1f`. Open: which park list rows the
    original's factories make with style `0x10` (sites `0x00493986`, `0x004939b0`, `0x00494628`, `0x0049463d`,
    `0x00496cea`, `0x00496cff`, `0x004b4c70`, `0x006604a1`, `0x0066051a`) and whether ours set `Toggles` for the same
    rows; which of them receive `0x100` is not read.
  - **Note (fork watch 2026-10-18, `verify-ui-hud.md` U3, `verify-world-b.md` L1):** the control of stream `0x00755750` is the ride
    window's **Buy Upgrades list** (`FUN_004ae560`, handler `0x004ae430`, rows from `FUN_004ae140`), and the `0x1D` it plays
    (`0x00486ae0`) is for an **add-on** row dearer than the balance only: a level row dearer than the balance gives no cue at
    all (`0x004af4e9`, strictly below). `ride-operation.md`, "The ride window's Buy Upgrades list". Q512 builds the list.
- [ ] **Q711. A list heading clicks Select3 (`0xbd`) in the original; ours clicks `BUTTON01` (`0x1f`).** From the fork watch,
  2026-10-19 (`verify-audio-fx.md` M2, "MISS (live in our code)"; `docs/exe/hud.md`, "Which window the click sound is for").
  **Insert next to Q402**, whose 2026-10-16 note asks which list rows make style `0x10` and whether ours set `Toggles`: this
  answers it for the headings. The click hook (`0x00485780`) plays on every message `0x100` by the RECEIVER's own style word,
  bit `0x10` (`0x004857cb`) giving `Select3` (`0xbd`), else `0x1f`. A list heading is a button (op `0xc`,
  `( 2, 1, n+0x10 )`, `0x006606a8`..`0x006606b2`) whose parent is the list, and every sorted list record carries `0x10` (walked
  `0x00750ab8`, `0x00750ba0`, `0x00750ca0`, type 7, flags `0x291`; the buy list `0x91`, the rest `0x291`). So a
  heading click plays `Select3`. Ours: the headings (`UiList.AddHeading`, `AddFramedHeading`; hire, buy, all-staff, all-items,
  visitors) are plain `UiButton`s with `Toggles` false and parent `UiList`, so `WindowStack.Release` plays
  `UiSounds.Click( toggle: false )` = `0x1f` (said at both sites). Build the rule the original has, not a per-button flag: toggle
  when the receiver (the parent) is a list flagged `0x10`. A radio group's children already toggle (`UiRadioGroup`). Not heard in
  either game yet. Confirm: a heading clicked on the hire list, caught by the disk driver, beside the original.

- [ ] **Q403. The balance's colour and the cost in hand.** From the fork review 2, 2026-10-16 (`verify-ui-hud.md` L5).
  The HUD letters the balance in one of six colours (table `0x00752e30`, index (balance + 999) / 1000 clamped to 0..5:
  red at nought or below, white over 4000), and control `0x30` shows the cost of the item in hand (`0x004a0d38`..`0x004a0d94`),
  only for mode types 3 and 4, and 8 when `[0x0079c658] & 1` is set (-9999 hides it, -999 resets it). The keys counter is skipped in Instant Action,
  the tickets counter runs in every mode. Read what this game does first. Confirm: the balance at 500 and 5000 beside
  the original.

- [ ] **Q419. The hire screen's candidate box is empty.** From the run review, 2026-10-16. On the same file the
  original's hire screen shows the chosen candidate's figure where this game's box is black (`q412/run-fix2/3-screen.png`
  beside `q412/orig/a-button.png`; the same in `q415`). The candidates' names differ too, which may be the pool's
  top-up (Q356's note); not looked into. Decode what the original draws there first. Confirm: the box beside the
  original's on the same file, a screenshot and the census of what is drawn.
  - **Note (fork watch 2026-10-16, `verify-audio-fx.md` O4):** the figure is made at `0x0049bb4a` to `0x0049bb64`:
    `0x00475a10( 0x0074f758, slot )` with the slot by role from `0x0049bb10` (5, 6, 4, 7, 8), the handle kept at
    `[0x007ca310]`, then `0x004755d0( handle, 8, 8.0f )`. The facing and frames 0 to 7 are inside the sprite script
    `0x0074f758`: not read.
- [ ] **Q404. A park six months in the red ends.** From the fork review 2, 2026-10-16
  (`verify-economy-guests-paths.md` L4, M2). Counted, `BANK_PARK_ENDS_IN_THE_RED`. What the end does is
  `ride-operation.md`, "The end of the park": world state 4, the end screen, `VAR_BREAKSTAT` 1 on every ride guests
  choose, the pending deletes flushed, the gate closed; the clock and the bank go on. The path tests no game type, so
  Lost Kingdom's Easymode reaches it. Confirm: a park held in the red to its end, beside the original.

- [ ] **Q452. Buy Rides lists a researched ride upgrade.** From the week-end audit, 2026-10-17 (`LEDGER.md` D1,
  `VERIFY-13a.md` Q349 1; `CLAUDE.local.md` says where). `ParkBuyScreen.Listed` lacks the original's `+0x284` skip
  (`0x004aaffc`), so a Full Simulation park with 1500 or 1501 researched lists Dino Karts Jump (750) and Dino Karts
  Tunnel (900), and a row puts one in the hand. Seen in the game on a copy of Alexah's Full Simulation autosave
  (`gp-fs.out`). It is Q221 (2), no longer dead by content since Q349. Confirm: the list without them, beside the original.
- [ ] **Q453. A park file's advisor poll cursor out of range stalls the game.** From the week-end audit (D2,
  `VERIFY-2a.md` Q316 1). `RestoreCursor` refuses only a negative cursor; at `0x7FFFFFFF` the first poll's `row + 1`
  overflows and walks 2^31 rows, about 87 s. The original sets any cursor of 351 or more to nought first (`0x0059c6a8`).
  Put the wrap back before the loop (the line removed as mutant r09) with an `int.MaxValue` test.
- [ ] **Q454. A new Instant Action player's first park clears the advisor's records.** From the week-end audit (D3,
  `VERIFY-1b.md` Q313 1). Event 10 (`RESET_FOR_EASY_MODE`, sent `0x0054f24e`, handled `0x0059af63`) sets every record's
  line to -1 and its said and slaps to nought, keeping the tick, on the player's first park only (the gms.dat byte
  read `0x005b0597`, written `0x005b0109`). Ours keeps Easymode.TPWI's said bytes, so 0xb9 and 0x11a are never said.
  `PlayerFile.FirstParkToCome` is never cleared here. Confirm: a new player's first park in both games, tick 877.
- [ ] **Q455. A click on the tab already down lifts it.** From the week-end audit (D4, `VERIFY-15a.md` Q364 1).
  `WindowStack` flips the radio button up and `Put` returns before setting it down; the original's group with style
  0x10 sets it down again (`0x0066a455`..`0x0066a468`; frames `q364/orig/b-*.png`). `ParkLocator` already works round
  it. Read each screen group's style first.
- [ ] **Q456. The entry price screen's Loans button in Instant Action.** From the week-end audit (D5, `VERIFY-16a.md`
  Q366 1). The original disables it in game type 2 (`0x00499008`..`0x00499030`), as staff costs' (`ParkStaffCostsScreen.cs`
  does it); ours takes the click, closes the screen and opens nothing.
- [ ] **Q457. The satisfaction bar is masked to a byte.** From the week-end audit (D6, `VERIFY-16a.md` Q367/Q368).
  `ParkItemsScreen` runs satisfaction through `Bar()`'s `& 0xff`; the original does not (`0x00494031`, `0x00494199`,
  `0x00495888`, `0x00495959`) and the setter clamps 0..0x400 (`0x0066d885`). A test with -1 and 256.
- [ ] **Q458. The staff screen's meters average as the original's.** From the week-end audit (D7, `VERIFY-16b.md`
  Q369 1). The original averages each member's `ftol(h) & 0xff`: the kind's meter an integer mean then
  `(m << 10) / 100` (`0x004c9cc0`), the all-staff meter a float mean times 1024 times 0.01f (`0x004c7cc0`, `0x00496a7b`),
  in double (`advisor-park.md`, the FPU's precision). Ours scales the raw mean: 896 against 892 on `q369/skill.TPWS`.
- [ ] **Q459. The happiness gauge keeps the mean's fraction.** From the week-end audit (D8, `VERIFY-4b.md` Q392 2).
  `FUN_004c7bb0` ends `fidiv` (`0x004c7c96`); the gauge is trunc(mean x 10.24) of 1024 (`0x004a1d3f`..`0x004a1d5a`).
  The units are built (Q244, `ParkGadget.GaugeUnits`); the mean handed to them is still whole, `AverageHappiness`'s
  comment calls `FIDIV` an integer division, and `TheMeanIsWholeAndRoundsTowardsNought` pins it.
  `GuestsInsideHappiness` is the exact routine.
- [ ] **Q460. Zlib textures round the chroma terms twice.** From the week-end audit (D9, `VERIFY-13b.md` Q355 1). The
  original stores the Cb and Cr terms as float32 at 1.5 x 2^23 (`0x00594f5e`, `0x00594f87`), integers before Y is added;
  `Held` rounds once: 933 of 986 files differ by 1. Green's rounding (the FPU mode) needs a breakpoint at `0x00594f72`.
  Q489 saw it through type `0x13` too: the running original's 16-bit surfaces of eleven jungle `pathtex` files hold
  each channel's top four bits, and where OpenTPW's byte ends in `0xf` about half (19,265 of 35,399) are one higher
  (`q489/PREDICTION.txt` a3, `conv3.py`); hold those files' nibbles to the original's once fixed.
- [ ] **Q724. A 0x13 image's channels rounded, as the original rounds them.** From the upstream look, 2026-10-19
  (`verify-images.md` L4). `textures.md` says `FUN_00595920` (the LZSS codec) indexes its channel tables "by the channel's
  rounded value"; `TextureFile.Decode` cuts each float channel with `(byte)Clamp( 0, 255 )`, a truncation, which is low by
  up to one level (mean 0.48 a channel, 2.6% of nibbles after the A4R4G4B4 cut, over 120 random 0x13 files). Reached by all 7,407 files
  that carry type `0x13`. **Settle first:** read the column pass and colour
  step of `FUN_00595920` in Ghidra (where the rounding falls: after the column pass, or on the colour step; the 12582912.0
  constant at `0x007026ac` is the codec's own), then match the original's decode of a few files, as Q483 matched the
  ground's. The comment at the cut says the deviation. Confirm: three Lost Kingdom `.wct` of type `0x13` beside the
  original's decode, pixel by pixel.
- [ ] **Q461. The tag bar's meshless buttons are disabled in the original.** From the week-end audit (D10,
  `TIEBREAK-Q311.md`). A type-0 tag's second button and a decline's erase button are shown and disabled (vtable +0x14
  with 0 at `0x0048e991`, `0x0048e43c`); the hit test skips a disabled control (`0x0065db3b`). Q427 made them take the
  pointer (`ParkGadget.cs`, `hud.md`, two tests). Needs Q66 (`HitTest` reads no `Enabled`) first, or the difference said.
- [ ] **Q462. The advisor's turn comes before the sweep's tail.** From the week-end audit (D11, `VERIFY-8b.md` Q318 1).
  The original walks every thing (his turn inside the walk), then `0x004d7b20` (`0x00516695`): arrivals, staff pool,
  calendar. Ours runs the arrivals and the pool before his `Tick`, so row 0x11 can be said a poll round early or late.
- [ ] **Q463. A tag's first show: half a second, and particle 0x5a.** From the week-end audit (D12, `VERIFY-7b.md`
  Q309 1). The mast posts message 6 (show) 500 ms after a tag is put on (`0x004a1286`); its first show starts effect
  0x5a (`0x0048dfa6`, not while paused). Ours shows it at once and counts nothing (rule 4): count it first.
  - **Note (fork watch 2026-10-19, `verify-audio-fx.md` M3; `hud.md`, "A tag's effects follow its visibility"):** the live
    effect `0x5b` (property 9) is shown and hidden with the tag (`0x0051ff10( h, 0 )` at `0x0048dff9`, `( h, 1 )` at
    `0x0048e02e`), and message `0x13` moves it onto the tag's middle (`0x0051fe30`, `0x0048df88`). Ours counts neither the
    hide / show nor the move (`ParkTags.cs` counts only the `0x5b` at the show): count them with `0x5a`.
- [ ] **Q464. A written corner cell's made vehicles run newest first.** From the week-end audit (D13, `VERIFY-2b.md`
  Q342 1). With ferry and seaplane both made, `ParkWorld.People.cs` writes cell (0,0)'s chain oldest first; the
  original's file (`q257x/orig/fixed-by-the-original.TPWS`) and our running list run newest first. No effect on reading
  back was found.
- [ ] **Q465. A purchase's advisor post is not counted.** From the week-end audit (`VERIFY-10a.md` Q328 3). Every
  purchase reaches the original's 0xc9 or 0xce post (`0x004ac67e`..`0x004ac7d9`); ours neither builds nor counts it
  (`ParkBuyScreen.Chose`). The line is filed under Q420; add the `Unimplemented.Report` (rule 4).
- [ ] **Q501. A rider's head, a balloon and a thought bubble are drawn the original's size: shown.** From the review of Q470
  (`handoff/review3/VERIFY-6.md`, finding 4): Q470 drew every sprite through `Extent` (the span over
  `Camera.ClipPerWorld`), and the head (`HeadScale` 0.685), the balloon and the thought bubble go through the same
  `ParkGuestSprites.Corners`, but only a person's body was photographed on the fixed build, and no census of the
  original's sprite list held a head, balloon or bubble (they hold people alone; whether any of those carries `0x200` rests
  on the programs' static reading). Confirm in both games at one camera: a rider's head on a ride, a balloon and a thought
  bubble, their rows and columns against the decode (`original/spritepick.py` census with one of each on the list).
- [ ] **Q502. The lobby's fog is linear, 50 to 250, in the original; ours is `exp`. Decode first, then build.** From the review of Q481
  (VERIFY-9 Q481 1): the three lobby states that call `FUN_005d9680` (`OR AH,0x20` on render-word `0x008bcbc8`, at
  `0x005e1407`, `0x005e1980`, `0x005e24a2`) then `FUN_005d96a0( 0xff44ddff, 50.0, 250.0 )`, which writes `0x008bd47c`,
  `0x008bd490`, `0x008bd494`; the lobby's own initialisation (`0x005d8bef` to `0x005d8c10`) sets 50 to 300 with the bit
  clear. Ours is `Level.LobbyFogDensity` 0.025 through the shaders' `exp( depth * 0.01 )`, which has no start or end
  (`test.shader`, `water.shader`), and the lobby has not been photographed against the original's since parks stopped using
  the global fog. Decode which lobby states fog and which do not, photograph the lobby in both games at the same
  camera, then give the lobby the original's linear start and end (first person's fog, Q485, is the same linear fog,
  `FirstPersonFog` and the shaders' `g_vLinearFog`, worked out per vertex in the original's clip z: share that path).
- [ ] **Q503. The build markers load from `textures` at every texture quality.** The original's `FUN_0053d790` calls
  `FUN_0054dcc0` (0x0053d7b4) and takes `STexture` (`0x00763ee4`) for the three dynamic meshes where it answers 1
  (texture quality 0 or 1), as `ParkSea` and `ParkFlags` already do through `ParkSea.SmallTextures`. `ParkBuildMarkers.Load`
  always reads `generic/dynamic/textures/{name}.wct`; `blue.wct` (493 against 211 bytes) and `red.wct` (340 against 122)
  differ between the folders, `m_link` and `m_end` do not. Pass the quality in, load `stexture` at 0 and 1, and confirm
  it in the running game at low detail (a screenshot of a build square, before and after). Said at the site in
  `ParkBuildMarkers.MarkerNames`.
- [ ] **Q504. The sea's and the flags' phase is a float that grows without bound, and no one wrap keeps both seamless.**
  `ParkSea.Phase` gains 12 a second off `Time.Delta`; at a high refresh rate the step (12 x Delta) is rounded against the
  float's ulp after hours (numpy float32 at 144 Hz: rate x1.031 from 1.5 h, x0.9375 from 3 h, x1.125 from 6 h, x0.75
  from 12 h, x1.5 from 24 h, stopped at about 48 h). The original's is a float32 too (`DAT_00871f60`, `DAT_00871ed0`)
  but steps 0.4 a frame at 30 fps, so it is within 1.6% only to 12.1 h (phase 2^19) and stops at 2^23 (about 194 h).
  The sea's wave is periodic in the phase every 4 pi (s = ... - 0.5 phase; the table index moves by 4,096) but the flags'
  `Grid` is periodic every 20 (D = ... - 0.6 phase; the joint period of 3 and 4 in D is 12), and `ParkFlags` takes the
  sea's phase, so wrapping the shared phase at either breaks the other at the wrap. Fix: keep two phases (the sea's
  wrapped at 4 pi, the flags' at 20, both stepped alike from the same `Advance`), say it as a deviation at the site,
  and test that a phase either side of each wrap gives the same wave. Said at `ParkSea.Phase`.
- [ ] **Q505. The picker stands still in first person, a ride's view and the view of `0x10`.** From the review of Q472b (`handoff/review3/VERIFY-13.md`, q472b 1): the original skips the picker and the packer while
  `[0x790ab0] & 0x16` is set (`0x0054e2c0` `test al,0x16`, then `jne`; first person is `0x202`), so its cell stays where it
  was. Ours runs `ParkPicking.Update` every park frame, counted `PICK_IN_FIRST_PERSON` in `Level.Update` in first
  person and the End's ride view (`Level.InsideView`), so the crowd voice (`ParkAudio.cs`, the original's `0x0054f875`) counts guests round a cell that follows the pointer. Decode which
  modes set `0x4` and `0x10`, then hold the picker under all three bits. Confirm in both games: the crowd voice's count
  in first person against the cell before it.
- [ ] **Q506. A frame's second press: acted on at the park, or not (`PARK_PRESSES_IN_ONE_FRAME`).** From the review of Q476
  (`handoff/review3/VERIFY-14.md`, finding 2): `Level.WorldClick` counts a frame's second left press and does not act on
  it (`Unimplemented.Report( "PARK_PRESSES_IN_ONE_FRAME" )`), because the first may open a window the second would land on
  and the interface has dealt the frame's presses out by then. Two presses inside one 31 ms frame is rare with a mouse. Decode
  what the original does with two presses delivered between two park ticks (the cursor callback `0x00486c70` posts each
  through `FUN_00658af1`; where the park's mouse proc `FUN_004879d0` runs against the drain), build it or record that one is
  enough, and drop the counter or say why it stays.
- [ ] **Q507. Does the original send a frame's second press to the control the first press's click left?** From the review of
  Q476 (VERIFY-14, finding 4; UNSETTLED). `WindowStack.Update` hit-tests the hover once and gives every step of
  `LeftSteps` (press, release, click) to that one control, so a second press in the same frame reaches the control the first
  press clicked even if that click closed or replaced it. The original's press poster `FUN_00658af1` addresses the hover current at
  posting (`0x00658b1a mov eax,[0x00faa5e0]`, then `0x00658ccc`), so if the cursor callback posts the second press before the
  drain handles the first click, the original's second press also goes to the old control. Settle by how `0x00486c70`'s events
  are delivered against the message drain (Ghidra), or by a two-press-in-one-frame run in the original on a button that closes
  its own window; then keep the comment at `WindowStack.Update` ("whether the original sends a frame's second press ... is not
  decided") or replace it with the answer.
- [ ] **Q508. A pulled body against an unpulled sprite's order (heads, bubbles, balloons).** From the review of Q500
  (VERIFY-14, finding 3; UNSETTLED, narrowed). The premise now holds: each group's flush (`0x0058aa3b call 0x5658c0`, ecx
  `[0x00879ab0]`, the batch object `0x00566220` adds to) reaches `FUN_00565590` (`0x00565917`), so sprite batches go through the
  per-triangle sort, which keys on the screen z the pull wrote. Still unread: the mask `[esi+0xc]` tested at `0x0056570b` (the
  doc's `0x3000`) and `[0x008bd524]` in play, and whether bodies share a group (texture) with heads, bubbles and balloons;
  across groups the batch key (mean z) also sees the pulled z. Settle by a live read of those two words in the original, or a
  photograph of a balloon's fringe against a body in both games; then say whether `FurthestFirst`, which orders every quad
  here by its unpulled depth, matches the original's order for a pulled body against an unpulled sprite, and whether the
  first-person view (render bit `0x80`, ungrouped; `docs/exe/park-engine.md`, "What hides a person below their feet") sorts at all.
- [ ] **Q509. Sound_StopFading: the original's fade law, and FADEOBJ as a cut at low detail.** From the fork watch,
  2026-10-18 (`verify-other-a.md` H1, `verify-audio-fx.md` F1; `docs/exe/audio.md`, "The fade, the SoundInfo settings
  and the reverb"). The original's `Sound_StopFading` (`0x0051c300`) and `FADEOBJ`'s sound arm are a **x0.6 volume step
  per fade step** (the 60 is a percentage kept, not milliseconds; a worker thread makes the steps, 100 ms apart, not
  the service pass) while sound detail is above
  `SoundInfo.FADETOGGLE` (30, `sound.sam`; stock detail is `DEFAULTQUALITY` 50, and 25 at boot on low graphics detail,
  `0x0054de17`), and a **hard stop** (the same as `KILLOBJ` / `Sound_Stop`) at 30 or below. At 0, or with no channel, the
  voice is stopped on the next service pass. OpenTPW stands a flat 0.06 s ramp in for both, said at the site in
  `Advisor.QuietenSeconds`, `ParkAudio`'s rain and crowd fades, `ParkCarSounds` and `FADEOBJ`'s (`ParkAudio.ScriptFadeSeconds`, Q553). Build: (1) the fade as a geometric
  decay on the voice's volume, one step every 100 ms (250 ms before the first when nothing was fading), each step
  `own = trunc( 60 x round( own x group product ) / 100 )` with the group gains folded in at each step, ending in a stop
  at 0 (the rate is read, not to be measured: `audio.md`, "The fade", fork watch 2026-10-19, `verify-other-b.md` H1); (2) `FADEOBJ` and `Sound_StopFading` as a cut when sound detail is
  30 or below, the detail being the options' sound quality slider (`0x00423850`), so the options screen's slider must
  reach it; (3) the stop-all at a level's end (`0x0051bcb0`, handed 90, `Level.StopAllSeconds`): trace whether the 90 is
  the same kind of percentage before changing it. Confirm: the advisor quietened (options screen) and the bumper cars'
  unload with a capture of the mix at detail 50 and at 30, beside the original at the same detail; predict the decay
  per pass first. Predicted per step: at group product 1.0 the volume runs 100, 60, 36, 21, 12, 7, 4, 2, 1, 0 (nine steps, about 0.9 s
  after the first step's wait of up to 250 ms); at the effects' measured 0.75 the device gets 75, 45, 20, 9, 4, 1, 0
  (six steps, about 0.6 s). (Fork watch 2026-10-19, `verify-other-b.md` H1; `audio.md` holds the chain.)
- [ ] **Q510. The hidden wide zoom: press it in the original, then build. Confirm first.** From the fork watch
  (2026-10-18, `verify-world-a.md` H1; `docs/exe/park-engine.md`, "A hidden wide zoom"). Camera row `0x11` (`0x00748298`,
  key `0x20`, modifier `0x3f`, handler `0x0042a520`) flips bit `0x200` of `[0x00790aac]`; by our key decode that is
  Shift+Ctrl+Alt+Space, and with the bit set the zoom clamp is [40, 300] (`0x0074c9d8`, `0x0074c9dc`) where it is [70, 130]
  otherwise. The clamp runs only while bit `0x80` is set; the park-load reset `FUN_0042a990` keeps `0x200` and sets `0x80`
  (`0x0042aa64`-`0x0042aa75`), so the wide zoom survives a load. The key table is the handler's only reference, and the press has never been
  made in a running original: this is decode, not observation. Confirm by that press in the original under Proton (read
  `[0x00790aac]` bit `0x200` with `tpwmem.py`, then zoom out with the mouse wheel or keys and read the zoom at its limit,
  predicting 300 and 40 first), also after a park load. Then build it: the key, the bit kept across a load, the two ranges.
  The pitch is computed from the zoom by the same expression and extrapolates past the range; read the camera's pitch clamp
  ([0, 84.27] in `park-engine.md`) at zoom 300 in the original before building it. Q23 is the camera's other work.
- [ ] **Q720. Do the keypad digits reach the cheat and camera rows? (one key press in the original)** From the fork watch,
  2026-10-20 (`verify-other-ui-audio.md`, "Miss found here"; `park-engine.md`, "The cheat table"). Both key edges
  (`0x0046b9c0`..`0x0046b9f5`, `0x0046bab8`..`0x0046baeb`) replace the vk by `MapVirtualKeyA( vk, 2 )` when it answers
  non-zero, excluding only the keypad's `* + - . /`; the digits `0x60`-`0x69` are not excluded. If the call answers `'1'`..`'9'`
  for `VK_NUMPAD1`..`9`, every row keyed `0x61`-`0x69` is unreachable (cheat rows 3-11, the Local 1-5 and Global 0-3 ticket
  cheats; camera rows 13-22, the keypad pan) and Alt+Num1 fires the ten-thousand cheat. If it answers 0 the rows work as the
  tables say. Confirm: in the original under Proton, in a park, NumLock on, hold keypad 4 (or 6) and see whether the camera
  pans; with `Easymode.TPWI`'s cheat flag possibly cleared by the first key (the same section), press a harmless first key, then
  Alt+Num1 and read the money. Then correct the keypad rows in `park-engine.md` (the camera's pan keys, near line 394, and the cheat table) and OpenTPW's keypad bindings if they differ.
- [ ] **Q511. A bouncing rider is lifted on a sine.** From the fork watch, 2026-10-18 (`verify-world-b.md` L4). The Belly
  Bounce's riders are drawn standing on their node here (`ParkGuestSprites.Seated`); the original's positioner
  `FUN_00557ab0` lifts each on a sine (`0x00557c75`): the phase is `( trunc( elapsed x ( 0.8 + 0.005 x the short +0xc0 ) )
  mod 1000 ) x 0.003125`, the index `trunc( phase x [0x007b095c] ) & [0x007b0960]` into the table `0x007b0968`, and the height
  `( ( visitor mod 7 ) + 12.0 ) x sine + the instance word + BOUNCESETBASE's word` (`+0x6e`), or the node's own y when that
  is larger; the sprite is placed by `FUN_004f9e60` with `elapsed / 500` and template 2 (`docs/exe/ride-operation.md`, "The
  bounce riders' height"). Counted, `RIDER_BOUNCE`, at the draw. Lost Kingdom's stock park carries the Belly Bounce, so placed
  content reaches it. Not read: whether a4 is a heading sector, and `[ [+0x1c] + 0x1c ]` (template 3 and `visitor mod 7`).
  Read those first. Confirm in both games: a rider's height over a few seconds on the same ride, predicted from the formula,
  beside the original (`record.sh`); a screenshot of each.
- [ ] **Q512. The ride window's Buy Upgrades list.** From the fork watch, 2026-10-18 (`verify-world-b.md` L1,
  `verify-ui-hud.md` U3). The ride window's upgrades button (`RIDE_WINDOW_UPGRADES`, counted) opens the original's list
  (stream `0x00755750`; `FUN_004ae560`, rows `FUN_004ae140`, choice `FUN_004ae430` and `FUN_004af480`): the add-on rows
  (types whose `+0x284` is the ride's type and whose control has `+0x10`), then one row for the next level, UITEXT 29 with
  the price `CostOfUpgrade[level+1]`, or UITEXT 30 "cancel" while an upgrade waits (state 2), else UITEXT 26 or 28; nothing in
  Instant Action (UITEXT 27). A level row is ordered only when its cost is strictly below the balance, with no error cue; an
  add-on row at most the balance builds a tool-4 carry shell and closes the window, and a dearer one plays `cat_ui` `0x1D`
  (`docs/exe/ride-operation.md`, "The ride window's Buy Upgrades list"). Full Simulation only: Lost Kingdom's stock park is
  Instant Action. Neighbours: Q272 (a mechanic and a waiting upgrade), Q452 (Buy Rides lists a researched upgrade), Q402 (the
  sound). Confirm in Full Simulation: a ride at level 0 and at level 2, the list's rows and prices, an order and its cancel,
  beside the original; a screenshot of each.
- [ ] **Q513. The add-on's snap and the window a click on an add-on opens.** From the fork watch, 2026-10-18
  (`verify-world-b.md` L2 and L3, `verify-world-a.md` M5). Lost Kingdom's add-ons (1500 to 1502, `AddOn.UpgradeType` 1) are
  the items. Hovered over one of the five track types, the tool-4 preview snaps the add-on to anchor + 2 x floor( d / 2 ) on
  both axes, turns it by the cell's flow (flow 1 gives 0, 4 `0x10e`, `0x10` `0xb4`, `0x40` `0x5a`), applies the item's
  North/West/South/East adjust and moves the cell by the ride's own angle; the placement stores the word `+0x30` as the
  builder's object (the ride the buy row was chosen on); a click on the add-on opens that ride's window when the word is
  non-zero (`docs/exe/park-engine.md`, "The add-on's snap, its open ends and the window a click opens"). Its open ends light
  the dummy nodes by a bit accumulator that is never cleared after the start. Read what this game does first (`HOVER_*` and
  the add-on hover counters; since Q478 a placed add-on of type 1 posts no hover row and keeps the thing, `Level.GroundArmAt`). Reached through Q512's add-on rows, so after it. Confirm in Full Simulation: an add-on dragged
  over a track piece at each flow, its snapped cell and angle, and a click on the placed add-on opening the ride's window,
  beside the original; a screenshot of each.
- [ ] **Q514. The hover's rows over a track and type `0x15`.** From Q478 (`park-engine.md`, "The rows over a queue and
  an object", "The arms that post nothing"): ours posts the empty row and counts `HOVER_TRACK_CELL_CATEGORY` (track types
  11 and 16, and 12 under a parent not 25) and `HOVER_TYPE_15_ROW` (map type `0x15`). Type `0x15` is read: category 6,
  UIHELPTEXT 448 (`0x1c0`, "Left-click to edit this track") posted on a change of category, the thing forgotten,
  `FUN_00469180`, pointer 11 `c_pylon` (`0x00487728` to `0x004877a2`). The track's arm (`0x004873d5` to `0x004874fe`) is
  category 5 and 446 (`0x1be`) with pointer 6 or 10 by `+0x2c` of the thing `FUN_004d99e0` answers for the track
  record's `+0x10` cell (none: the thing forgotten, nothing posted, `0x00487441`). On a cell of map type 4, 9 or 10
  (`FUN_00536380`, `FUN_00536350`, `FUN_00536340`) the arm returns with nothing when the cell has no thing
  (`0x004873f8`) and goes on to the map-type switch when its thing is no add-on (`0x0048740e`), so such a cell takes the
  object's 550, where ours counts the track first. The stock park reaches neither
  (map types 0, 1, 2, 3, 4, 7, 9, 10, 30 and track types 7, 12, 25, `parkfile.show_map` on `q325/stock.TPWS`); that a
  built track ride is what makes them is inferred from the rows' words and `c_pylon`: build after the track rides. Confirm in both games over a built coaster's track and its pylons.
- [ ] **Q517. Do the loading screen, the pointer and the letters pass the four-bit cut? Decode first.** From Q700
  (`textures.md`, "The texture cache's surface format"): ours keeps them at eight bits (`TextureFlags.OwnPixels`) because
  how the original puts them on the screen is not read: the loading screen's picture (`LoadingScreen_Begin`,
  `0x00587380`), the `.ani` pointer, and the `.bf4` letters. Find whether each is a texture of the cache (cut to
  A4R4G4B4) or a blit to the display, then cut what the original cuts. Confirm by a frame pair of the help bar's letters
  and the pointer in both games.
- [ ] **Q518. The 16-bit display.** From Q700 (`q700/PREDICTION.txt`, P3, P9): with the textures cut, the sea at camera
  B still reads red 3.1 against the original's 1.3, and the shipped presets hold `RENDER32 0`, a 16-bit display
  (`boot.md`, `0x0044e20b`), where ours draws eight bits a channel. Decode first what the original's frame keeps at 16
  bits (5-6-5, and whether the device dithers or cuts), measured on its own frames at its own resolution, then decide
  with Alexah whether ours draws the same.
- [ ] **Q519. A texture's mip levels: four, and how the original fills them.** From Q700 (`textures.md`, "Four levels a
  texture"): the original gives each texture four levels where the cache has room (`TexCache_MeasureSpace`,
  `0x0058d400`); ours makes every level down to one texel, each the rounded mean of the eight-bit level above, cut, a
  filter not read. Read the filler (what writes a level below the first: the codecs, or a downsample of the surface),
  then build it. Confirm by a far camera over the sea and the brick in both games.
  - **Note (run 6 review, Q244):** confirm by the happiness gauge too, a fixed screen-space target: its faces are
    sharp in ours and blurred in the original's frames (`q244/side-by-side.png`; `hud.md`, "The happiness gauge's two
    quads"). If the mip levels do not explain it, the filter states are Q29's.
- [ ] **Q520. A person's shadow under a tool's squares.** From Q480 (`park-engine.md`, "A person's shadow"): the
  original's patrol squares (and any tool's) are drawn over the people's shadows, which show through them
  (`q480/orig/a-2-on.png`: Mike's shadow on the squares darkens 0.556, 0.716, 0.835); ours draws `ParkBuildMarkers`
  before the people, writing depth 1.5 over the land, so a shadow 0.6 over it is hidden (`q480/run2/4-on.png`: 0 pixels
  changed with `shadows 0`). Decode where the original draws its marker squares against the sprite list's two passes
  (and whether they write depth over the people's feet, Q477), then order ours to match. Confirm by the locator open on
  Mike in both games, a frame pair with shadows on and off (`lib/shadowdiff.py`).
- [ ] **Q521. A person's shadow at high detail in the orbit view: their own picture, unfogged.** From Q480, narrowed by
  the run 4 review (first person's own-picture shadow, `FUN_00589770`, is built: `ParkPersonShadows.KindFor`). `SPRITESHADOW`
  2 and 3 (`high.sam` sets 2) have `SpriteShadow_Draw`'s non-`0x40000` arm (added with `0x566220`, unfogged) use the
  person's own picture with the darkness word `0x67000000` as its colour and the picture's own blend (no multiply),
  counted here `SPRITE_SHADOW_OWN_PICTURE` (`ParkGuestSprites.WriteShadow`) and drawing none. Build it on first person's:
  add the unfogged orbit kind to `KindFor` and reuse `OwnPictureShadowMaterial`. Confirm at high quality in both games
  (the original by its Options screen) by a frame pair at one camera.
- [ ] **Q522. The weather's drops in first person's fog.** From Q485 (`park-engine.md`, "First person's fog"): first person
  fogs every batch under render-word `0x2000`, but whether the rain's and snow's drops go through that batch (the callers
  of `SpriteBatch_AddQuad` include `FUN_0057be90`, `FUN_0057ed50`, `FUN_005860d0`, `FUN_00577ae0` and `FUN_0058a010`, none
  read) is not decoded; ours draws them unfogged and counts `FIRST_PERSON_FOG_WEATHER` (`WeatherSprites.OnRender`).
  Decode the weather's draw, then fog `weather.shader` the same way or take the count out. Confirm by rain in first
  person in both games with the band held at 20 to 40 (`memhold.py` there, the console's `fog 20 40` here).
- [ ] **Q525. The money's lettering: the entry fee's "$ " and a cash below nought.** From Q524 (`q524/side-by-side.png`):
  the original's entry-price spinner letters the fee "$ 28" where ours letters "28" (`ParkEntryPriceScreen.Show`), and
  its park cash at the top left letters a balance under nought "-$ 796" in red where ours letters "$ -1188" in the
  usual colours (`orig/a-fee.png` beside `run2/3-reloaded-fee.png`). Seen, not decoded: read the spinner's text
  (`hud.md` names the money skin `FUN_00490250`, "$ 75" and "-$ 11750" on the training screen) and the cash display's
  draw, then letter both as the original does. Confirm by the two frames at one fee and one balance under nought.
- [ ] **Q527. The gadget's date: the original letters it month.day.year with full stops.** From Q526
  (`q526/side-by-side.png`): on 10 January 2002 the original's gadget reads "1.10.2002" (`orig/a-2-loaded-gadget.png`,
  the poll's day word 10 at that moment), where ours letters .NET's short date, "1/6/2002" on 6 January
  (`ParkGadget.ShowDate`, `ToShortDateString`, so it follows the computer's culture). Decoded (run 6 review):
  `FUN_004a0e30` letters UITEXT row `0x1c0` (448, `{17}.{16}.{18}` in the american tables, `saves.md`, the Load Park
  rows) into a buffer of `0x1f` (the call at `0x004a0ea1`), token 16 the day, 17 the month (1-based, the +1 at
  `0x0048156c`) and 18 the year + 1900: month.day.year with full stops. It is redrawn on a day's change, from
  `0x00481570` inside the day-changed block (the call at `0x004f83ac`), and once at set-up (`0x0048166f`). Ours draws `GameCalendar.Now`,
  the date of the park's tick (Q149), so the two gadgets show one date in two letterings (`q149/side-by-side.png`). Confirm by both
  gadgets on one date.
- [ ] **Q529. A loaded park's weather runs on from the file's.** From Q528 (`saves.md`, "OpenTPW's writer, the weather
  thing"): the original's load reads all of the weather thing over its constructor's (`Weather_Serialise`,
  `0x00512010`) and runs on from it, the rain, the storm, a pending forecast and its due time; ours reads only
  `mWindDir` (`ParkWorld.WindDirection`) and rolls the rest fresh, though `ParkWorld.WeatherRecord` reads it all. Start
  `ParkWeather` from the record, and schedule as the original does, on the park calendar's instants (`mLastChanged` +
  3 days, the forecast's `mTime`; the first tick past each) rather than day edges of `GameCalendar` (Q149), so a
  forecast read from a file falls due on the original's tick. Look first at what the load leaves undrawn: the original
  sets the particles only on a step of the ramp, so a file whose drops stand at their target rains nowhere until the
  next change (filmed, `q528/orig/e-rain-film.png`), and its voice's handle is the saving process's. The constructor does not apply its roll
  (`docs/exe/weather.md`, `FUN_00511f70`), so a park with no file holds the roll unapplied until its first forecast
  arrives. Confirm: q528's `run3/q528-fix2.TPWS`
  loaded here and in the original, the quality, the drops and the forecast's delivery tick (17097) predicted, a frame
  of each.
- [ ] **Q712. The television static on entering or leaving first person.** From the fork watch of 2026-10-19
  (verify-advisor-ui.md L4; `park-engine.md`, "Entering or leaving first person shows 300 ms of television static"),
  with Q485's first person. On either edge of the camera flags `& 0x16` (a ride's view, first person) the original's
  `Game_StateMachine` (`0x0054fabb`..`0x0054fb63`) toggles the fog's render bits and sets a 300 ms timer (`0x0087502c`,
  stamped from `GetTickCount`); while it runs the frame is a 320 x 240 static (`FUN_0053e760`: one `rand()` a pixel, 75%
  black, 12.5% white, 12.5% grey 150) stretched to the screen **instead of** the frame renderer. Lost Kingdom reaches it
  from every ride view and the camcorder. Count `FIRST_PERSON_STATIC` where the swap would go, then build it timed on
  `Time.WallMilliseconds` (it is real time in the original), the frame replaced as the original's is. Not measured: the
  pixels in a running original; the screen the blit is stretched to is read as the screen size, not checked. Confirm:
  the static photographed in the original under Proton on entering first person, then ours beside it; predict the 300 ms
  from the frame count.

- [ ] **Q716. First person's own-picture shadow beside the original's.** From the run 4 review (Q480 1, Q485 1 and 2),
  built and confirmed here against main (`fix-run4/PREDICTION.txt` S1-S5: no streak at -2/-1, 0.592 of the ground under
  the picture's core, a figure where main drew a bar) but not beside the original: its first-person frames (`q485/orig`)
  have nobody near the camera. Put the original in first person with people within the band (`gmove.py`, `memhold.py`
  at the stock band and at -2/-1) and ours at the same stand; predict the shape (the person's own `.FPC` picture laid
  along the sun) and 0x67/255 darkening on opaque texels. Measure the order too: ours draws every shadow after every
  body, the original's 0x80 walk adds each straight after its own body (said at `BuildShadows`).
- [ ] **Q717. The cost of cutting every texture to four bits.** From the run 4 review (Q700 4): a park loads 0.76 s
  slower with the cut, and each switch into and out of first person packs the guest atlas again (about 0.45 s by the
  load figure, not measured). Measure a camcorder switch with and without the cut (`TEXTURE32 1` in a private game
  folder); if it is felt, make `CutLevels` cheaper (a 256-entry table, one row loop a level). Said at
  `Texture.LevelsFor` and in `docs/exe/textures.md`, "What the cut costs".
- [ ] **Q718. A first-person right click let go inside one frame reaches the park after first person is left.** From the
  run 4 review (Q499 2, latent). `Hud.Update` (`Level.cs`) runs the window stack before `WorldClick`: the stack's release
  step calls `ViewRightClick` (`ParkViewfinder.RightClickAnswer`), which leaves first person, and `WorldClick`'s press
  step then reads `RightPressTaken( false )`, arms, and its release runs `QuickRightClick`. The original posts the park
  the press first, in first person, and takes none of it. Harmless today (entering first person lets go of the hand and
  `LetGo` answers null); it bites if anything the quick click does is ever not a no-op in first person. Fix: record at
  the stack's press step whether `ViewRightClick` answered, in a static of its own, and read that in `RightPressTaken`.
  Said at the site (`Level.WorldClick`, "Latent").
- [ ] **Q535. An empty-hand path click takes the latch from its own pass.** From the run 5 review (Q532 1). Between
  the press and the release the original runs mode-1 hover passes over the clicked cell, so an empty-hand click on
  ground whose one cell is unaffordable leaves `DAT_00816d50` 1 (the money arm, `0x00535931`); ours arms (which clears
  the latch, as the install's pass does at `0x0046d5e8`) and leaves it 0. Give `PathStrip` an out `latched`, as
  `ParkPathBuilding.Strip( x, y, out latched )` has, and have `RunPath` store it in `ParkBuildMode.RefusalLatched`
  after `Arm`. Confirm with the original (the stock park, an unaffordable single cell, `d50` read at
  `q532/orig/a-run.out`'s addresses) and a test whose price a wrong rule gets wrong. Said at the site
  (`ParkBuildMode.Arm`).
- [ ] **Q536. A carried item's passes set the latch.** From the run 5 review (Q532 2). Mode 4's pass clears
  `DAT_00816d50` at `0x005234f2` and sets it and `d5c` at `0x0052365c`/`0x00523662` each tick when the item's
  BuildPrice (`+0x1b8`) is above the bank's balance, unless the golden-ticket cost `+0xc4` is positive and
  `FUN_004d4b70` says it is unpaid (then the pending price is 0 and d50 stays clear), and not under the replay
  (`[0x00785a2c]`). The placer (`FUN_00528a70`, `0x0052390e`) also sets d50 at `0x005295e8` and `0x00529730` for an
  entrance or exit facing off the map (trigger not re-derived). Build both in `ParkBuildMarkers.OnUpdate` while
  `ParkBuilding.Carrying != 0`; the player reaches it by a move with the price above the balance, which `Carry`
  refuses but `Move` does not. `Hold` clears the latch for now; said at the site (`ParkBuilding.Hold`).
  From Q539 (its last piece; counted as `CARRY_PREVIEW_MARKERS`): confirm beside the original's `[0x00816d50]` and the
  locator's edges; the bank can be held low in the original with `thingpoke.py` (`TOOLING.md`, "Recipes", Q542).
- [ ] **Q537. The patrol pointer after a re-arm, watched in the original.** From the run 5 review (Q534 1). Re-arming
  the patrol tool over itself keeps `c_noplace` here (`ParkBuildMode.Arm`), by the decode (`FUN_004b5b50` ->
  `FUN_0046c350` -> `FUN_0046d5a0`; the setter `FUN_0052f200` sets a cursor for modes 3 and 1 only); not watched in
  either game. In ours the route Ctrl+S, the mechanics' button, the strip did not reopen Mike's window with the tool
  in the hand (`fix-run5/c-control`): find the original's route first (the locator, or the gadget's staff screen),
  then patrol anchor outside the park, the pointer on a park cell (`c_noplace`), the switch down again, the pointer
  back on a park cell, and read the cursor; predict `c_noplace`. Also read the pointer over the gadget's camera
  control with the tool armed (`FUN_004981d0` writes the same control's cursor). Write the result into
  `docs/exe/hud.md`, "The patrol pointer", and drop the "not watched" sentence there and at `Arm`.
- [ ] **Q541. The build tools' run total, shown as Q403's cost in hand.** Seen in Q540's runs: with the queue tool
  anchored on the stock park the original shows a yellow "$ 375" under the balance (`q540/orig/a-16-join28-edges.png`,
  `b-1-before-click.png`), gone after the click; ours shows none (`q540/run2/3-join-edges.png`). Q403 already decodes
  that display: `FUN_004a0ab0` reads the total (`0x004a0d38`) into control `0x30`, for interaction-mode types 3 and 4
  (8 only when `[0x0079c658] & 1`), -999 skipping the show and -9999 (the patrol tool's `FUN_0052fb90`) hiding it
  (`0x004a0d68`..`0x004a0d74`); the build tools are type 3 (`park-engine.md`, "The build-tool interaction mode is
  object type 3"). Build it with Q403 or after it; what is left to read is how often message `0x1e` comes against the pass.
  From Q542: the total is `DAT_0081af4c` (`FUN_0052fba0` adds, `FUN_0052fbc0` reads); the path and queue passes never
  zero it, `FUN_004a0db0`'s message `0x1e` does (`0x004a0dee`), calling `FUN_004a0ab0` first on every third one (the
  counter `[0x007cb2e0]`, reset at `0x004a0de2`, which message `0x15` also zeroes, `0x004a0e02`). A red square's second
  verdict adds to it (`park-engine.md`, "The path tool"), so the label may show more than the click charges: read it
  beside `strip`'s `run total`.
- [ ] **Q568. The second charge's carry flag, settled by the original first.** From the review of run 6 (VERIFY-2, M1,
  UNSETTLED). The second verdict's charge also needs `[0x008186d4]` == 0 (`0x005358df`). The mode-4 and `0x3b` arms of
  the pass (`0x005235d4`, `0x00523967`) and of the click (`0x00524b4a`, `0x005253e9`) set it to 1; only the click
  (`0x005249c2`, `0x00525a06`) and `0x005239fc` clear it, and `FUN_0052f200` (the tool setter) does not touch it. So a
  path or queue tool armed from the toolbar straight after carrying an item might preview with no second charge (and so
  no `c_cash`) until the first world click. `ParkPathBuilding.JudgedAgain` never models the flag. Run the original
  first: carry an item, put it down or drop it, press the path button, and hold the pointer on bare ground past a red
  square with a balance under 20; watch `[0x0081af4c]`, `[0x008186d4]` and the cursor `[0x007cb2d8]`
  (`docs/TOOLING.md`, the path tool's recipe). Build it only if the run shows the flag held; otherwise write in
  `docs/exe/park-engine.md`, "The path tool", that it is cleared before a tool can be armed. Confirm: the same steps in
  ours beside the original's frames.
- [ ] **Q543. The welcome a park made with no file says on tick 1, decode first.** From Q327 (`advisor-park.md`, "A
  fresh Full Simulation park", and "Still unknown", `Welcome`). In both of the original's fresh Full Simulation parks
  message 0 (row 0, posted only; response 1, sample 1, slot 1, `Welcome.Score` by `0x0059c8c0`) stood in a slot at
  100,000 on tick 0 and was said on tick 1 for 31.3 s (the record (1, 0, 1, 0) in `q327/orig/FSFRESH.TPWS`, read with
  `parkfile.py -a`; the slots in `q327/orig/a-restart.INTS` and `q327/orig/o/fresh/proton-restart.INTS`;
  `q327/orig/a/fresh.log` holds the slot and the 31,332 ms action only); this game posts
  nothing there. Find the poster (a post of message 0 at the world's setup, or the park's first tick), whether an
  Instant Action park made with no file and a loaded park post it too, and what sample 1's 31 s is. Then build it.
  Confirm: a fresh park's first seconds in both games, the record and a frame.
- [ ] **Q547. The cursor Up and Down keys step an object window.** From Q31. The base window proc `0x0048d200` takes key
  down `0x2600` and `0x2800` to `0x0048cbe0` and `0x0048caf0` (`hud.md`, "The shop, sideshow and ride windows"), which
  apply and step to the next or the previous object of the class; no object window here hears a key. Build it for the
  ride's and the shop's windows (and the staff member's and the visitor's, if the same proc serves them: read first), and
  confirm in either game: the stock park with a second Drinks Shop bought, Down in the shop's window, the title and the
  `shopwindow` census naming the other shop.
- [ ] **Q548. The ride window's title is white; `UI_SetTitle` letters it yellow.** From Q31: the shop's window takes
  `UI_SetTitle`'s font 5 in (234, 239, 102) with its shadow over the widened rect (`hud.md`, "The shop's window"), as the
  visitor's and the staff member's do; `ParkObjectWindow` letters the ride's title white in the stream's own rect. Read the
  ride's load for the same call (`FUN_004ae940` or `0x004af030`), then photograph the Belly Bounce's window beside the original's.
- [ ] **Q549. The researcher tires about 80 sweeps sooner here and rests longer.** From Q701: stock Easymode, left
  alone, our researcher (thing 30) set off for the Staff Room on tick 2210 and 2199 in two runs and ended the rest on
  2553 and 2578, skipping 17 and 19 hand-outs; the original's last hand-out before its rest was on 2280 and it gave the
  next near 2590, skipping 14 (`q701/run1/run.log`, `run2/run.log`, `q701/orig/a-poll2.log`). So Dino Karts' 95th
  hand-out falls on 2980 or 3020 here and 2920 there. Read
  both games' rest level each sweep from tick 755 (`q134/orig/watch.py`'s reader; `staff`) and find where the drain
  or the walk to the room parts (`ride-operation.md`, "The staff turn"; Q326's search, arrival and rest).

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
  `GameCalendarTests.TheDaysKeepTheOriginalsSlightlyFastPace` (it asserts 10 January on tick 230 and 11 January with
  `DayRolled` on 231, the tick's date, and nothing of the 248 ms pace),
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
  Name them and make them functions in Ghidra as the reading finds them (Alexah, 2026-10-18); a name there agrees
  with its `docs/exe/` row. No game run.
  - **Note (fork watch 2026-10-18, `verify-advisor.md` A1):** the three loan helpers were read in the executable's listing, not
    in Ghidra: `FUN_004d0810` (some offer passes and is untaken), `FUN_004d0970` (a taken offer's remaining owed) and
    `FUN_004d0a00` (the total owed), and `ride-operation.md`'s rows on them are corrected ("The loan helpers and the money
    rows"). The Ghidra project was not touched: their functions and names are still to make there.
- [ ] **Q579. The voice list's reader and writer named in the Ghidra project.** From the review of run 6 (VERIFY-17,
  Q559 4; group O's notes). The project's `SoundManager_SerializeVoices` (`0x006b6da0`) only reads the voice list
  (stream slot `+0x8`), and its writer `0x006b6c30` (slot `+0xc`) is unnamed. Rename both in the project (a reader and
  a writer name), then use the new names in `audio.md`'s table and "A script's sounds", `saves.md`'s `SOUN` row and
  Q560's text, which keep the old name today. Docs and names only; nothing to confirm in the game.

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

- [ ] **Q466. The texture-alpha pages and two FileFormats pages, read again whole.** From the week-end audit,
  2026-10-17 (`VERIFY-14a.md`, `VERIFY-14b.md`, `VERIFY-15a.md`; `CLAUDE.local.md` says where). Q357 to Q362 rewrote the
  alpha rule in turn and left `textures.md` (:19-20, :34-38, :51-58, :86-97, :123-124, :234), `lobby.md` (:215-239) and
  `render-states.md` (:79, :161, :166) contradicting the code and each other: nothing keys black in the interface; the
  loader's `0x800` makes floors opaque; the skirt pass clears the current group's material when the edge byte is nought;
  mode 1 reverses each texel's bytes (`0x005ecf5e`). FileFormats `formats/sgn.md:228-230` keeps the old multiplier's alpha
  figures and `formats/models.md:747` calls bit 0x02 the only one established (rule 14). Rewrite each from the tip's
  code and the listing; no behaviour change.

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
  and the Right arrow and keypad 7 lower it, and the drawn rotation eases after it by min(dt, 140) / 140 a frame (min(dt, 200) / 200 with the 90 degree option
  `[0x0078d912]` set: this item's own option); the
  direction is already the original's (this game's yaw is its rotation negated, and the Left arrow,
  RotateLeft, lowers the yaw). The step and the easing are what differ.
  From the run review (2026-10-17, Q440): W and D scroll the orbit camera (`ParkOrbitCameraMode.cs`) without ending a
  locator's follow, where any pan in the original does (`0x0042ba37`, `0x0042ba92`; `ParkLocator.Following` is cleared only
  at its own fold, a mode change, a person gone and a step that finds nobody), so scrolling with the locator following
  does nothing here: the follow puts the point back each frame. Counted nowhere; clear it where a pan is built.
  From the fork watch (2026-10-18, `verify-world-a.md` M1, `verify-other-a.md` M1; `park-engine.md`, "A sprite's camera
  sector reads the asked rotation"): the original picks a sprite's camera sector from the **asked** rotation
  `[0x00790a38]`, not the eased one, as `8 - (trunc( (rotation + pi/8) x 8 / 2 pi ) & 7)` (`0x00542010`), and a draw
  with flag `0x40000` gets sector 0 (`0x005421cc`). `ParkGuestSprites.Facing` reads `Camera.Rotation.Forward` (about
  line 571); that is the same today because `ParkOrbitCameraMode` snaps and has no ease, and it becomes a difference when
  this item builds the ease: the sector must then follow the asked rotation. The rounding agrees; the sign against
  `0x00790a38` is read only from the code and its own note says the screen settles it.
- [ ] **Q24. Nothing highlights a thing under the mouse.** The idle pointer already follows the hover category over
  ground and path (`Level.IdleOverPath`). Picking is decoded (`park-engine.md`, "Picking is a real ray cast, not a
  grid lookup"): the hit point comes from a ray against the terrain and then object meshes, and the hovered THING is
  taken from the cell, not by hitting its model (`ParkPicking.ObjectUnderCursor`; a person by their picture, `PersonUnderCursor`). Decode what the hover updater
  `FUN_00486d90` highlights when the hovered thing is an object or a person, then build it. Include the park gate: its
  arm calls `FUN_00469180` (`0x004878ce`, the highlight swap on `[0x007afda0]`), which the ticket booth's arm does not
  (`park-engine.md`, "The gate and the ticket booths"); the hover calls it at six places. Seen in the original (Q478):
  the pointer on the Belly Bounce brightens its model (the frame's box (180,220) to (300,350) reads red 160 and 173 in
  `q478/orig/b-3-ride.png` and `b-4-ride-again.png`, 138 and 141 in `b-2-queue.png` and `b-5-shop.png`), the object
  arm's calls at `0x00487712` (a new object) and `0x004878ce` (the same one). From Q494: `[0x007afda0]` is the model the picker's object march met (`FUN_00469340`), which
  ours keeps as `ParkPicking.Marched` (the thing), whether or not its point beat the ground's.
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
  From Q496 and Q497 (`q496/orig/`, `q497/orig/`, `helpbar.py`): the pick wears `0x13`, `c_cam.ani`, and posts rows of
  its own, 580 "{P2}: Left-click to ride" over a ride and 579 "Left-click to use camcorder mode here" after it, and
  none over ground before (`hud.md`, "The help bar in first person"); build them with the pick. Its
  click on a hoarding (617, 378), and one at (700, 450) or (617, 500), left the pick up (the no-entry cursor, seen
  at (617, 500)); one at (512, 420) or (450, 450) dropped it back to the orbit's mode `0x6fea10` with no first person;
  one on the path inside the gate (290, 395) stood the viewer there.
- [ ] **Q27. Pushing the mouse at the screen edge does not scroll.** The "push scroll" option exists and
  is read by nothing. `ParkOrbitCameraMode.Update` scrolls from keys only. Decode the camera
  binding table at `0x00748158` (`park-engine.md`, "The park camera"), then build.
  From Q440 (`park-engine.md`, "The follow"): the original pans at the edge only with `[0x0078d910]` clear, an Options
  toggle (written at `0x004a3882`, control `0x1d4c9`); the update zeroes the pointer's terms when it is set. Which of the
  Options screen's rows it is, and whether it is the "push scroll" row, is not read.
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
  - **Note (fork watch 2026-10-19, `verify-audio-fx.md` M1, `verify-other-b.md` M5; `hud.md`, "Which window the click sound is
    for ... the rename boxes' keys"):** in the two-line boxes (park, object) Enter on the **first line moves the focus to
    the second with no click**, Tab swaps the lines, Enter on the second applies and clicks (`0x1f`), Escape closes silently
    (`0x00658d9f( w, 4 )`). Our generic Enter click (`WindowStack.Keyboard`, `UiSounds.Click( toggle: false )` before
    `front.Accept()`) would click on the first line's Enter: the box must move the focus without it. The **staff rename box**
    (`0x004b6110`, callback `0x004b6320`, table `0x007572e8`): one edit of 32 characters, title UITEXT 195, first text the
    thing's current name, write through `0x00506830` (no length or empty test), refused while a box is open or with no
    selected thing of class 4..8; a box does not open a second time while one is open.
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
  - **Note (fork watch 2026-10-16, `verify-audio-fx.md` A6):** particle caller ids read in the listing: `0x004d95bf`
    50, `0x00544dd8` 2, `0x005480fc` 96, `0x0055dba3` 39, `0x0055dc22` and `0x0055dca1` 40, `0x005999ad` 87,
    `0x0048de1a` 91; also `0x00546ac7` and `0x0054aed5` (15), `0x00549d04`, `0x0054ad24`, `0x00557334`, `0x00557f0e`
    (19), `0x004b8f6e` to `0x004b961c` (97, 98), `0x00548481`, `0x00548a4b` (1), `0x005488d4` (10). Which fire for a
    Lost Kingdom build or sale is not read.
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
  From the run review (2026-10-17, Q430): `ParkAudio.VisitorWindowOpened` (kids effect `0x81`, played on every build of the
  visitor's window in the original) keeps this throttle, now said at the site: a window opened again within the clip's
  length plus RepeatDelay is silent here. Pass `respectDelay: false` there, as `PutOff` and the yawn do, if the throttle
  stays when this item is done.
  - **Note (fork watch 2026-10-18, `verify-other-a.md` M3):** the pool's tie keeps the **newer** voice: `0x006b6a80` links a new voice at the
    list head, so the rebuild (`0x006b6bc0`, strictly greater than the lowest rank) sees the newest first. This corrects
    "the earlier voice keeping a tie" above (`audio.md`, "The fade, the SoundInfo settings and the reverb").
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
  - **Note (fork watch 2026-10-18, `verify-ui-hud.md` U6):** a put-down that reaches `0x005253b8` leaves the rotation at
    0 (`0x0081d7a4`, by `0x0052f1b0( 0, 0 )`) unless exactly Ctrl is held, which keeps it (`park-engine.md`, the paragraph
    on carrying a purchase). Ours already keeps the item in the hand on exactly Ctrl (`ParkBuilding.cs:1319-1324`). Still
    unread: which placements take the branch at `0x00524db1`, and the queue tool's commit.
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
  - **Note (fork watch 2026-10-16, `verify-ui-hud.md` M2 and M5):** the address check, inside the send handler
    (`0x004a7f36` to `0x004a7fa2`): the recipients are split on `;`, `,`, space and newline; each needs an `@` not at
    its start and a `.` after it more than one character on; any failure opens the box UITEXT `0xd6`, otherwise
    `0x004a8600` sends (online: skipped). The photo is 640 x 240 (`0x005d485f`), drawn into (`0x142`, `0x5e`, `0x27c`,
    `0xd7`). The alert queue `0x0047f620` (a deque, popped one at a time when no box is up) is fed only by the
    postcard and the online screens (33 call sites of `0x0047f410`): in an offline Lost Kingdom game only the postcard
    posts to it. Not by goals or challenges.
- [ ] **Q242. Go Online and Publish Park. Alexah's call.** From Q139. Both are counted (`GO_ONLINE`,
  `PUBLISH_PARK`) and close the menu; the original starts connecting (`0x005b5cc0`) to servers that are gone. Ask
  Alexah: leave them counted, say so in the game's own style (rule 11), or something else. Then build the answer.
  From Q496: the online visit's own interface (`FUN_0048a410`, stream `0x00758ae8`) and its help rows are decoded
  (`hud.md`, "The online visit's help rows"); they come with whatever Go Online becomes.
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
  From the run review (2026-10-17): both person windows are built (Q430, Q431), so five of the seven are left, and their
  leftovers belong here; Q420's notes from Q430 and Q431 list them: the visitor's preview (`VISITOR_WINDOW_PREVIEW`) and a
  guest gone (`VISITOR_WINDOW_GUEST_GONE`), the lists' lines `0x11e` and `0x11d`, the staff member's name edit, preview and
  member gone (`STAFF_WINDOW_NAME_EDIT`, `STAFF_WINDOW_PREVIEW`, `STAFF_WINDOW_MEMBER_GONE`), the staff room's row
  (`0x004b5267`) and the patrol switch's second help row (87). New: the base handler `FUN_0048d200` turns the cursor Up and
  Down keys (`0x2600`, `0x2800`) into `b_arup` and `b_ardown` (`hud.md`, "The person's windows"); decode the focus to see
  whether a key reaches the window, then build it or count it (`Unimplemented.Report( "OBJECT_WINDOW_CURSOR_KEYS" )`).
  Duplicates: `ParkLocator.OpenOnMember` copies `OpenOnVisitor`, where the original's is one function, `FUN_004b3f60`,
  branching on the type byte (`0x004b3f84`, table `0x004b4258`): make one `OpenOn( thing )`; and
  `ParkStaffWindow.DaysEmployed` is a third copy of the `( FileTimeOf( now ) - stamp ) / TicksPerDay` divide beside
  `ParkState.DaysSinceStaffHired` and `DaysSinceObjectBuilt`: one `DaysSince( stamp, now )`. Their tests that miss their
  bug are in Q211's run-review note.
  - **Note (fork watch 2026-10-16, `verify-ui-hud.md` H1):** the sideshow's and the ride's windows are decoded too
    (`hud.md`, "The shop, sideshow and ride windows"). Sideshow: price always, then chance of winning (`+0x190`, 0 to
    100) and prize (`+0x188`, 0 to 10000) only when `NonWinnableSideshow` (`+0x10c`) is 0, which no shipped `.sam`
    sets (that branch is dead by content); apply to all is `0xa0a9` with mask `0x100`, the same-item rule. Ride: the
    speed, duration and capacity controls and the apply `0x004af440`.
- [ ] **Q32. Graphics tiers.** `Level` reads the detail files (`low.sam`, `med.sam`, `high.sam`) for `GameOptions.PARTICLEDENSITY`
  and `NUMKIDS` (the kid and staff bank caps) and for three `GraphicalOptions.*` keys: `LIGHTING` (a counted no-op),
  `TEXTUREQUALITY` (the sea's and the flags' folder) and `PROCEDURALTEXTURING` (the ground's tile size). `SPRITESHADOW` is read too (Q480: medium's silhouette built, the own
  picture counted). Nothing reads the rest (texture filtering, sky, the sky's and models' shadows, fog, mipmaps, view distance); `SKYQUALITY` and `FIRSTPERSONVIEWDISTANCE` are named only in comments. The live detail file is loaded once, at boot
  (`FUN_0044dc60`, `lobby.md`'s `0x007858c8` row); `0x00423bc0` (`OptionsScreen`'s restart note) only reads a new
  quality's file into a local to compare it and flag a restart. Decode what it does with each key - the lobby plan's item 11 names three: the `stexture` set,
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
  - **Note (fork watch 2026-10-18, `verify-other-a.md` lead 3):** `DUCKINGLEVEL` is proven: global `0x00785914`, the table's
    seventh key (`docs/exe/audio.md`, "The fade, the SoundInfo settings and the reverb"), used as a percentage, the
    group level times 38 / 100 (`0x0051bdf9`). The "inferred, not proven" above is settled; read it from `sound.sam`.
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
  - **Note (fork review, 2026-09-30):** `restart.INTS` is the park as the theme was first entered (`saves.md`, "Restart
    Park: restart.INTS"), here the original's own fresh park. `FUN_00550b30` writes the new world's state once, on the first state-0xf frame, and Restart Park (`FUN_005ac5f0`) loads it back. Each of the three holds
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
  - **Note (fork watch 2026-10-16, `verify-world.md` L2 and L3, `verify-audio-fx.md` O1):** `0x00430ed0` is the
    **track knot pass**, not a car update (`addresses.md` said car). Per track node (list `[0x00790fe4]+0x18`, next
    `+0x64`) it works the tangent's yaw and pitch into `+0xd4` and `+0xe0`, poses the node's pylon model
    (`0x00430ca0`), advances its model (`0x00473c70( 0x7a4610[node+0x70], 1, 0 )`, `0x004311f0`) and, for each knot
    record (`node+0x90`, stride `0x58`), takes the posed tag's position and writes the node's roll at `+0xe4`; the
    second rail is the knot plus (0, 1, 0) (non-loop), plus 2.0 x the tag's row (a modelled loop, node flag `0x40`),
    `0x004321c0` last. The sampler `0x00440c10` turns (second rail point less the first) by -yaw and -pitch through
    the sine table `0x007b0968` and takes atan2( -x, y ); `0x00440ad0` walks the knots. The bend `0x00438800` runs
    from `0x0043d730` only for a car type whose flag bit 0 is set (`coaster1`'s `bBendCar 1`), its dot clamped to [0,
    1] before the acos. Not read: the bend's tables, `0x00432df0`, `0x00435570`, the per-pylon tag counts.

  - **Note (fork watch 2026-10-17, `~/.cache/tpw-harnesses/fork-review-2/watch/2026-10-17/verify-save.md` L1):** the coasters module holds each track piece as one 0x22-byte
    record: flags, the mesh instance (to node `+0x70` through `0x00463ab0`), cell x and y (u16), an ordinal (u8), five
    unaligned floats and a link count (u8). The floats go into the node through `0x00444150` (loft, `+0xe8`),
    `0x004441d0` (yaw, `+0xe0`) and `0x00444270` (bank and incline, `+0xd8`/`+0xdc`, or a loop's rotation, `+0xf8`).
    Measured on Temple Of Gloom in Alexah's jungle saves: the pieces end at the module's +1,344, leaving the 506 bytes
    for trains, cars and riders. No loop piece or link in the corpus: those branches are read from code only. The field
    table belongs in FileFormats `saves.md`, "The coasters module".
  - **Note (fork watch 2026-10-18, `verify-audio-fx.md` F3):** the crash sound. `0x0043cd60` exchanges two bumping trains' speeds and, when
    the difference passes 1.0 (`[0x006fe020]`), calls `0x00439840`, which **only while `[0x00785a2c]` is nought** plays
    global rides 12 (`0x0051bfc0( 0, [0x00803a28], 0xc, x, y, z )`, `0x004398cd`) at a place taken from the train's `+0x20`
    by the ride's sample count (`+0xa8`). Build it with that gate, the one `audio.md`, "What an EventMap's slots feed",
    names for slot 0.
  - **Note (fork watch 2026-10-18, `verify-world-b.md` L6 and L7, `verify-world-a.md` M2 and M4):** the rebuild `0x0043eb70` runs
    **every frame** for every track with flag bit 0 (`0x00435a30`, from `0x00435a95`, list `0x00790fe0`, next `+0x18`; event
    `0x43` once, flag `0x2000`), so `0x00435a30` is the per-frame rebuild as well as the motion tick; the edited track and an
    index-picked one run it too (`0x00447d49`, `0x00447d83`). Its order: `0x00430ed0` (knots), `0x00432b50`, `0x00440c10`
    (sampler), `0x0043c450` (roll), `0x00439920`, `0x00439ab0`, `0x0043eac0`, `0x004402d0`. **The winch mark** (node `+8` bit
    `0x800`): `0x00432b50` clears it at every rebuild (unless `0x1000`), `0x0043c5f0` sets it when the clamped value equals its
    limit; not read: that the limit is the floor speed, and "creation parameter 0x20 gives `0x1800`" (no such immediate is
    or-ed into `+8`). **Chop rounding**: `0x00439ab0` (the crossing pass) sets the x87 to chop (`0x00439ab9`..`0x00439ac8`) and
    restores it at `0x00439c30`; every conversion in its callees truncates. **The roll per sample** `0x0043c450` (gated by
    car-type flag 8, run in the rebuild, not per car): the target angle `0x0046f230`, the delta wrapped by (d + 3 pi) mod 2 pi less
    pi, weight |vector| x car-type `+0x6c` held to 1.0, the roll wrapped and stored at sample `+0x18`, past `[track+0x134]`
    scaled by 1 - clamp( (i - start) / (`+0xa8` - start), 0, 1 ); the vector is turned by three sample angles. Not read: that
    `+0x6c` is `ForceMultiplier`.
  - **Note (fork watch 2026-10-20, `verify-world.md` M1):** a loaded coaster's crossing lists hold **chain positions**: after the
    stack pass (`0x0043827b`..`0x004382a3`) `0x0043e520( ctl+0x3c, n )` walks `n` links (`+0x64`) from the head (`ctl+0x54`) and the
    node's `+0xb4+4i` takes the node found. The saver writes each partner's `node+0` (`0x004375ad`..`0x004375b7`). The loader stores
    the counts unbounded (`0x00437d19`, `0x00437f72`); the runtime holds eight (`0x00439fda`, `0x0043a018`). Not traced: who numbers
    `node+0`, and whether deleting a node renumbers the rest. No park file on hand holds a crossing (the two with a coaster have
    counts 0), and the offer gate refuses a self-crossing coaster, so Lost Kingdom meets none. FileFormats `saves.md`, "The coasters
    module", has it.
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
  - **Note (fork watch 2026-10-16, `verify-world.md` L1 and L9, `verify-audio-fx.md` O1):** the loop button's open
    question is answered: control `0x20` is enabled only when the mask's bit 2 holds and the thing's upgrade level
    (`+0x50`, `mUpgradeLevel`) is at least 1 (`0x004980c9`, refresh `0x0049880f`), so a fresh coaster has it off until
    its first upgrade (`coaster1` has `Upgrades[0..2]`); at level 1 or more the icon is `cb_loop`, or `cb_trough` when
    mask bit `0x200` (definition flag `0x100`) is set. `0x004df8f0` raises the level (`0x004df962`) only when thing
    `+0x19c` is 2: not decoded. The loop pair: `0x0079c664` = 2 at `0x00442cf4` (button) and `0x00444677` (key),
    zeroed at `0x004446dd`; `0x006fdeec` = 90000.0 compared at `0x00435e2a`. The closing click `0x00442880` is not
    read. The knot pass is Q405's note.
  - **Note (fork watch 2026-10-18, `verify-world-b.md` L5, `verify-world-a.md` M3):** Q406's H is the pylon height
    `0x004346a0` = ( `[p+0xc0]` - `[p+0xbc]` ) x t + `[p+0xbc]`, t the piece's `+0xe8`. At the cost (`0x00447830`) t is
    **unclamped**, p is `[[piece+0x100]+0x18]` and the result is x 0.1 (`0x006fe1f0`, `0x004478bf`); the cursor routine
    `0x00443f40` clamps t to [0, 1] and skips it when the control's flags & `0x4000`, storing the result at `+0x118`. The rebuild
    every frame, the chop rounding and the per-sample roll are Q405's note.

- [ ] **Q407. Buy Land.** From the fork review 2, 2026-10-16 (`verify-objects-signs-other.md` L1). Counted,
  `BUY_LAND_TOOL`; Lost Kingdom's Features tab offers it (`hud.md`, item 101), and 13,878 of the 16,384 cells carry
  the unowned flag `0x40`. The validator `0x00531a50` refuses a span over 2 cells, gate posts, cell types 7 and
  `0x1e`, a hoarding crossed and a pocket left (a flood fill: grid `0x00532960`, fill `0x005328e0`), and shows the
  can't-afford pointer `0x14` (`DAT_00816d5c`) unless a layout is replayed; the price is `Costs.MapCell` × the unowned
  cells inside; the purchase `0x005327e0` places the hoardings and clears the flag inside (`0x005311c0`); the debit is
  `0x004d01f0`. Confirm: a rectangle bought, beside the original. The purchase also re-marks the ground's half light
  (`FUN_00450d10` at `0x005328c3`; Q482): make `ParkGround`'s mask again, and confirm a bought cell's light doubling.

- [ ] **Q408. TOUR rides: Jurassic Tours.** From the fork review 2, 2026-10-16 (`verify-tours-advisor-rse.md` L1-L6).
  `tourride` (Id 1170), Lost Kingdom's golden-ticket ride, Full Simulation only (no `Easy_` `.sam`), runs 19 TOUR
  instructions, which fall to the counted default here. Decoded: the handlers (table `0x005569d0`; FileFormats
  `vm/instructions.md`), the flying car's tick `0x0055abf0` (nine states; take-off at 20000 and 14000; a lattice at
  max( 12000, ground + 5000 ); patrol corners at ±25000; 30000 near stations; bounds from the park, `FUN_0055ab50`), a
  rider's head ((car `+0xd8` + count) mod car `+0xdc`) + 1, TOUR 9's effects (worn particle 39, broken 40, a repair
  sound `0xbb` and clip 10), the draw `0x0055ce60`. Waits on Q706 and Q709.

- [ ] **Q409. Full Simulation's loans and challenges.** From the fork review 2, 2026-10-16
  (`verify-economy-guests-paths.md` L1, L5). A loan offer passes when its amount is at most the net worth less what
  the bought loans still owe, signed, kept in the offer and rebuilt on opening and each tab (`0x0049fa00`,
  `0x004a06b0`, `0x004d06b0`). A won challenge with a follow-up pays no prize, and the follow-up is marked won as it
  is offered, so one declined or lost never returns; declining a challenge that is not Independent pays half its
  prize; the pacing reads `DaysAfterCompletedChallenge` and `DaysAfterDeclinedChallenge`; nothing raises the declined
  count, so `DeclinesToForfeit` is never reached (`0x004d2b60`, `0x004d2fb0`, `0x004d1a00`, `0x004d3140`,
  `0x004d1ae0`, `0x004d2ec0`). Neither screen is built.
  - **Note (fork watch 2026-10-16, `verify-world.md` L13, `verify-ui-hud.md` M1):** a shop's sale feeds the challenges
    by its `ShopType` 1 to 6 (`0x004e18e4`: types 5, ingredient, 8, ingredient, 7, 6) and `SpecialIngredient` 1 to 4
    (`0x004e18fc`: types 2, 1, 3, 4), posted by `0x004d27a0`; `0x004ca160` is the mean over toilets of 100 less a
    float's low byte, only for `0x004d2860`. The loans window: row pitch `(h x 0x600) / [0x00faa5c0] + 6` from a font
    of 7; summary labels UITEXT `0x165`, `0x168`, `0x167`, `0x169`; take `0x0049f460` and repay `0x0049f4a0` refill
    only the list `0x0012c4bd`; the tab is remembered for the session (`[0x007cb260]`, control `0x0012c4bf` or
    `0x0012c4c0`).
  - **Note (fork watch 2026-10-18, `verify-advisor.md` A1):** the credit test `0x004d06b0` is read in `ride-operation.md`, "The
    loan helpers and the money rows": limit = net worth x 1.0 less the total still owed (instalments left x instalment over the
    taken offers), an offer passing when its amount is at most it, and the pass flag **written into the offer** on every call, so
    an advisor poll of row 102 rewrites the flags. A loan taken counts as cash in for the advisor's row 107. Rows 100 and 107
    are polled in Instant Action too (Lost Kingdom reaches them); 101 and 102 are Full Simulation's alone.
  From Q327: the original said row 107's `0x6b` (score 90) on tick 3633 of `q327/FSOPEN.TPWS` and `FSFRESH80.TPWS`,
  its fresh Full Simulation park saved on 3513 (no loan, $48,180, $1,400 spent); this game counts the row
  (`PARK_ADVISOR_POLLED_ROW_NOT_BUILT`).

- [ ] **Q448. Ride script: REPAIREFFECT and BUMP 17.** From the fork watch, 2026-10-16 (`verify-audio-fx.md` A1 and
  C1, `verify-world.md` L6). Both are counted by the dispatch's per-instruction report (`RideScript.cs`: "REPAIREFFECT
  at N", "Wateride: BUMP at N"; `BUMP_17_TRACK_PIECES` is the bumper rides' alone, and the water ride has no bumper
  record, `RideScript.cs:2337-2345`), both reached by Lost Kingdom's own rides. **REPAIREFFECT** (table entry 93,
  `0x005562a6`; FileFormats `vm/instructions.md`): the particle on for a non-zero value and off for nought
  (`0x00556de0`); **show** also plays global rides 111 and 211 at the model; **hide** plays the SINGLESCREAM picker
  `0x00551320` with (`VAR_ONRIDE`, 1, 1), so only column 0 (75, 79, 83, 87 by the number aboard, silent with none),
  and only when the script declares more than five variables. Our `park.md` has the polarity right; the other fork,
  which follows the Mac, has it backwards. Jungle Bouncy runs `REPAIREFFECT 1` and later `0`. **BUMP 17**
  (`0x005549ed`): the operand against `[script+0xe6]`; equal does nothing, otherwise it is stored and the circuit
  walks `0x0052a490` and `0x0052a700` run with f = 1 for an operand of 0 and 0 for any other; reset to -1 at load
  (`0x00559f29`); track edits run the same walks. Jungle's `Wateride.RSE` issues it four times (0, 1, 1, 0). Confirm:
  Bouncy's repair limb and the Wateride, beside the original. Decode `0x0052a490` and `0x0052a700` to `docs/exe/`
  first if they are not there. Building BUMP 17 for the Wateride also needs the water ride's track record, which the
  code does not have (the Q257r paragraph under Q420).
  - **Note (fork watch 2026-10-18, `verify-audio-fx.md` F5):** REPAIREFFECT's two sounds (global rides 111 and 211, `0x0051bfc0` at
    `0x0055631a` and `0x0055634c`) **drop both returned handles**: nothing stores them, so nothing can stop them with the
    owner. Do not tie them to the owner. Whether 111 and 211 loop (effect flag `0x8`) was not read; if they do, the
    original never stops them.

- [ ] **Q449. Building over scenery, and tearing down a water track.** From the fork watch, 2026-10-16
  (`verify-world.md` C2 and C3). (1) The placement check (`0x00535d66` to `0x00535d73`) passes a new thing over
  another's cells only when its `Info.OverwritePriority` is **strictly greater** than the owner's (`0x00818c10`,
  `0x00818c14`). Every category file declares 5; of the item files, features alone set it, 45 at 1 and 48 at 0
  (FileFormats `sam.md`, which lists Lost Kingdom's); the path and queue tools write 2 and 3 (`0x00527200`,
  `0x00527218`). Our code reads the key nowhere (`ItemDescriptionFile.cs` lists it as unread;
  `RideInfo.OverwritePriority` is unused): decide what a click on scenery with a ride or shop selected does here, then
  count it. (2) The demolisher's track arm (`0x0052801d`), for `WhichTrackType` 1 (karts) and 2 (the water ride):
  withdraws 2 x the cell cost x the scrap % / 100, signed, at `0x00528179`, after the per-tile refund of `0x0053b280`
  (cost x % / 100 unsigned; none for cell type `0x19`, a replayed layout or a station tile). `SALE_TRACK_TEARDOWN`
  (`ParkBuilding.cs`) counts it. Lost Kingdom's Wateride (`WhichTrackType` 2) and its go-karts (1) both reach this
  arm; its coasters (3) withdraw nought. Confirm: a Wateride sold with its track, and the go-karts, the bank beside
  the original's.

- [ ] **Q450. Park screens: the buy window's gauges, the map's colours, the hover latch.** From the fork watch,
  2026-10-16 (`verify-ui-hud.md` M3, M4, C3). (1) The buy screen's stats panel is unbuilt (`ParkBuyScreen.cs`); its
  gauges are in `hud.md`, "The shop, sideshow and ride windows" (`0x004abb00`: excitement, text from
  `Upgrades[0].RedLineCapacity`, a gauge from `Upgrades[0].WearRate`; label text not read). (2) The map overlay is
  unbuilt (`ParkMapScreen.cs`); its colours are immediates in `0x005f0700` (`hud.md`, "The map's colours"); the
  channel order is inferred, and which record colours which code is not traced. (3) The frame routine brackets the
  draws with `0x0046c100` (on `0x007b05a8`, at `0x0054e310`) and `0x0046c110` (at `0x0054e34f`), which sets `+4`
  (`0x007b05ac`) to the latched thing when its kind byte `+2` is 1, 4, 5, 6, 7, 8 or `0x12`, sets `+8` to the first
  thing on the picked cell (`+0x24`, `0x007b05cc`, by `0x00527e80`) and, when nothing was latched, copies `+8` into
  `+0`; `0x0048c960` snapshots `+0`, `+4` and `+8` (into `0x007c264c`, `0x007c2650`, `0x007c2654`). `ParkPicking` keeps all
  three apart: the raw latch (`Latched`, no copy of `+8` into it when nothing was latched), `PersonUnderCursor` (`+4`)
  and `ObjectUnderCursor` (`+8`; Q474); compare Q444 before building. Confirm each
  beside the original.
  - **Note (fork watch 2026-10-19, `verify-other-b.md` M2):** the map's thing codes 1 to 8 are coloured, by default, by the
    thing's state of repair through the three-stop ramp (`hud.md`, "The park map": the stops, the clamp, the eight sites);
    for codes 1, 2, 3 and 8 the radio's settings 2 and 3 use `FUN_004e0860` and `FUN_004e1e30` instead. Part (2)'s "which record colours which code" is
    answered for those eight codes.
- [ ] **Q714. The Park Status screen (`PARK_STATUS_SCREEN`), starting with the Top 3 Thoughts.** From the fork watch,
  2026-10-19 (`verify-other-b.md` H2; `hud.md`, "The Park Status screen's Top 3 Thoughts"). Counted when opened, never
  asked for by an item (the notes under Q420 list the rings it graphs). The screen shows people in the park, arrival rate,
  average happiness, average time, park rating, the Top 3 Thoughts, the happiness gauges (`hap.wct`, `sad_undec.wct`),
  total visitors and the 1/3/12-year graph (`hud.md`, the nine screens). Decode the rest first (labels, the graph's draw
  `0x004a47bf`, the series colours at `0x004a45e0`, the scale `0x004801d0`/`0x004802a0` read in `hud.md`), then build.
  The Top 3 is read: count the peeps' thought (`+0x30`) over those `0x004fa990` allows, the three largest (ties to the
  first, id 3 to `0x16`, none left is `0x17`), slot 0 the rightmost icon, the icon's frame the id less one, its help row
  of `THOUGHTS.str` the frame plus one, refreshed every 1000 ms. Unproven: whether an icon hidden for no thought is shown
  again. Confirm: the screen photographed beside the original's in the shipped park.
  - **Note (fork watch 2026-10-20, `verify-other-ui-audio.md` A-L1):** the graph's backdrop is decoded in `hud.md`, "The graph's
    backdrop": the plot box (the control's size less 20, from 4), the colours, ten grid rows, the columns' step of 1, 3 or 12 for 12,
    36 or 144 samples (any other count leaves the step unset, an original defect), the zero axis when ymin is below nought, and the
    `%d` labels. Not read: how the finance screen's 36 and 144 reach the count word, and the `0x7d00` added to each label's place.

## F. Then

Back to `docs/PLAYER-GAPS.md`: gap 4 (advisor in a park); gap 7 (saving a fresh park) is Q249, in section A. Also litter and the day ending, whose deferral reasons expired
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
- [ ] **Q715. The lobby's bolt inverts the screen for one frame (low priority: the lobby, not Lost Kingdom's park).** From
  the fork watch, 2026-10-19 (`verify-other-b.md` M1; `lobby.md`, "The lobby's bolt inverts the screen for one frame").
  The park's bolt has none, so only the lobby is affected. After the lobby bolt's call the original sets a flag, and the next
  frame draws a white full-screen quad through INVDESTCOLOR/ZERO blending (1 - destination) once. Not traced: the state
  word's path to the blend states, and no run of the original has shown the frame (the fork's capture saw none either), so
  film the original's lobby in a storm (`record.sh`) before building, and count the unbuilt flash `LOBBY_BOLT_FLASH` (rule 4) until it is. Pairs
  with Q152 (the bolt's lifetime).
- [ ] **Q721. Each lobby island and gate turned by its ISLAND heading.** From the upstream look, 2026-10-19
  (`verify-sound-ui.md` L2). The island constructor `0x005dfa60` stores `island[+0x14]` = the ISLAND line's heading field
  x pi / 180, hands that radian value twice to `FUN_005d8380` (the isle's model instance `[island+4]`, then the gate's
  `[island+8]`), and calls `FUN_005d8190` on both with ISLANDCAMERAPOSITION; the camera homes onto `+0x14 + pi`
  (`FUN_005e1210`, `lobby.md`, "The island's heading"). Every `*_gate` mesh in `lobby.wad` sits on its model's -Z side, and
  a yaw of `heading` puts the gate where the homing parks the camera, which is why the heading is read as the island's and
  gate's yaw. Ours: `LobbyCameraMode.GateHeading` is one fixed pi, and `LobbyIsland` and `LobbyGate` never turn (only a
  position, `Level.cs`), which agrees with the original only where the heading is 0 (Space Zone). Predicted, unproven: the
  original's Lost Kingdom gate stands at our -X (ours -Y), Wonder Land's at +X and Halloween's at +Y. The comments say the
  deviation now; FileFormats `lobby-scripts.md` and `lobby.md` are corrected to what is known. **Settle first, in Ghidra or
  under Proton:** what `FUN_005d8380` writes into the model instance and how its draw matrix (built by `0x005d8870`, flags
  `0xc0`) turns the float into a rotation (axis, sign, row or column convention), and that the stack argument `[ESP+0x48]`
  at `0x005dfb90` is the ISLAND line's sixth field; or, cheaper, read the island camera's `+0x18` at state 2 after Enter on
  Lost Kingdom with `tpwmem.py` (pi / 2 + pi = 4.712 if the heading is used, as predicted; ours pi) and film one frame of
  the opened gate. Then turn the isles and gates, set `GateHeading` from the island, and fix the sun in
  `Level.SetupEntities`, which stands in front of gates that all face one way. Lost Kingdom's island first. Confirm: the
  lobby with the island and gate of Lost Kingdom beside the original's.
- [ ] **Q575. A plain voice's first silence, measured in the original.** From the run 6 review of Q563 (`handoff/review6/VERIFY-18.md`,
  Q563 2). Read from the listing and built (fix group P): every start of a plain or music voice, the first and a start
  in range again included, draws its variation's silence (`FUN_006bc410` -> slot `+0x48` `0x006bdf00` -> `0x006c5220`),
  where Q302/Q553 had the first followed at once (`audio.md`, "The plain class"). Predict before the run: the land's gulls
  (global ambient 2, one variation, wait 0 to 10000 ms) in the stock park, with the camera on the harbour, put their
  first sample's channel and their second's 0 to 10 s apart beyond the first's length, not at once; and the bus's voice
  back in range (waits nought) unchanged. Confirm in both games (`mixer.py`, `voices.py`; ours `scriptsounds` and the
  `Park audio: the land's own ambient 2 ... ms of silence after it` lines, which now print on the first too).
- [ ] **Q576. A chain's range before its first child, and a chain of any variation's, by the set-up's draw** (optional; reach
  only the staff's 188). From the run 6 review of Q563 and Q564 (`VERIFY-18.md`, Q563 4, Q564 1). The original's chain of any
  variation ranges for its whole life by the variation its set-up drew by weight among all (`0x006bbfc5` ->
  `FUN_006bc810`), its children writing nothing into it, and a held chain does before its first child; ours ranges by the
  newest child's or the effect's first. Said at `ParkAudio.InRange` and `ParkScreams.InRange`. In Lost Kingdom only the
  staff's 188 (a held chain, its fourth variation 85 of weight 1354 of 65532, the rest 90) can differ, and only in its
  start window away. Build: draw the set-up's variation for range alone (kept apart from the first child's variation 1).
- [ ] **Q578. Whether the land's following voices are heard through a pause in the original.** From the run 6 review (group N, Q558 1's decode, `audio.md`, "The pause substitution"): a voice
  with a rectangle (flag `0x1000`) is moved by `FUN_006bd5f0` to the listener's own height from `[manager + 0x60]`
  (`0x006bd5ff`), and the service's range test ignores height. If `[manager + 0x60]` holds the listener the pause lifts
  (`FUN_0051c1d0` -> `0x006b5ba0` -> `0x006b8b60`), the gulls and the sea stay at the listener's distance and are heard
  through a park's menu, where ours holds them (`Audio.HoldPlaced`) and lets their samples pass unheard. Read what
  `[manager + 0x60]` points to and who writes it, then measure the original's mix in a paused Lost Kingdom park
  (`mixer.py`: the sea's channel volume and distance with the menu open).
- [ ] **Q601. The gadget's date is written with the machine's culture; the original's reads "4.13.2002".** Seen in
  Q707's runs (`q707/p707t/side-by-side.png`): ours letters `GameCalendar.Now.ToShortDateString()` (`ParkGadget.ShowDate`),
  "4/18/2002" on this machine, and would letter another order on a machine of another culture; the original's frames read
  "4.9.2002" to "4.14.2002" (`q707/p707t/orig/O/g-*.png`). Decode the original's date writer for the gadget, then write
  the date its way whatever the machine.
