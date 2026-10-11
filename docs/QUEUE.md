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
the original.

---

## A. Bugs first

**Ordered at Alexah's word (2026-10-08):** what a playable Lost Kingdom park needs most comes first (saving a
park, the calendar, the shop's window, the gauge, the research lab), then the rest of this section in its old order.

- [ ] **Q298. The writer: what else a made record leaves out.** The old Q297's other pieces, each counted or
  said in `saves.md`. Confirm: each piece's bytes beside a file of the original's own, and the file loaded in
  the original under Proton.
  From Q297 (`audio.md`, "A scream's child and its pitch"): a kids' one-shot is given no pitch here
  (`KIDS_ONE_SHOT_PITCH`: a single scream's bytes are -8 and 8, a guest put off's -5 and 5; the rule is
  `FUN_006bc410`'s, and a pitch below nought was in no run of either game). A scream child's pitch under
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
  From Q267 (`audio.md`, "A channel's volume"): a bumper ride's engine (68), thunder
  (`ThunderVolume`) and the interface's flat effects are not through `ChannelGain`. No scream was made
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
  From Q264 (`audio.md`, "The mixer's distance law"; `park.md`, "The land's own sounds"): the land's two type 3 records
  (gulls and the sea, a rectangle) and the level's own two type 1 are counted, not played
  (`SCAPE_OMP_SOUND_TYPE_3`, `SCAPE_OMP_SOUND_TYPE_1`): the level's own `cat_ambient` is not loaded, and the
  gulls' and sea's voice class (`0x0006`, `0x006be090`) is not decoded, nor why the level's ambient 181 had
  no voice in the original. A channel's volume read as a whole number of 127 (the fall 75, the music 60), so a
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
  `RIDE_UPGRADE_COMPLETION`: nothing here puts a ride in state 2), advisor message `0x46` is counted
  (`MECHANIC_ON_HIS_WAY_MESSAGE`), and a mechanic put down still goes idle where the original's searches at
  once (`MECHANIC_PUT_DOWN_JOB_SEARCH`); every repair's count is nought while the State of repair stays 100
  (the wear by use, Q157), so the count down was in no game run; his repairing picture was not held against
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
- [ ] **Q248. The writer's other callers.** From Q241 and Q241e (`saves.md`, "The other callers"). `gms.dat`
  written before every park file; `autosave.TPWS` on leaving a park, which entering then finds as the newest
  file; the quicksave and quickload keys (`<theme>.TPWS`); `restart.INTS` written when the folder has none, and
  read by Restart Park. After Q241j. Confirm: a park left and entered again at the tick it was left on.
- [ ] **Q260. The park's lens: 90 degrees or 53.** Decode first. From Q258 (`park.md`, "A park's particles,
  started and drawn"): the matrix the original draws by, read in its memory, is a 90 degree projection times a
  scale of 1.5 across and 2 down (`0x0087b0d0`, `FUN_005781d0`), and a point through it lands in the frame with
  the screen as -1 to 1: 53.13 degrees top to bottom. `ParkOrbitCameraMode` asks for 90, from the culling
  frustum. Find which the picture is (a known point's pixel in both games at one camera), whether the lobby and
  the camcorder are the same, and what `TOOLING.md`'s "its camera stands further back" then is. Confirm: the
  decode written down; the build is the next session.
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
  sideshows and the rest stop at `SHOP_WINDOW`, `SIDESHOW_WINDOW` and `FEATURE_WINDOW`, and a staff member at
  `STAFF_WINDOW` (`Level.ClickWorldAt`); a clicked visitor reaches nothing counted. `park-engine.md`, "The
  per-object management screen is nine screens", lists the nine. This item is the shop's; the other seven are Q31b.
  From Q177: the shop, sideshow and toilet windows' figures are decoded (`ride-operation.md`, "The settle-up's
  bookkeeping"): customer satisfaction, profit last month, customers as "C of M", winners last month, the cost of
  goods from the window's pending sliders (applied on close, on stepping to the next or by apply to all), and a
  toilet's users last month. `UIStrings` 37 to 39 are one row off: UITEXT 37 is "Scrap value", 38 "Local happiness",
  39 "Quality of goods", 40 "Sale price"; nothing uses the three members yet.
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
  `ParkRideOperation` makes no breakdown request and sets no worn flag (`ride-operation.md`, "The first half of the
  turn"), and `Invite` reads no `RunsContinuously` (`+0x33` bit 0). Count each where the original tests it, and
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
  From Q252: **OpenTPW's Belly Bounce carries one rider at a time.** Every `rides` census of every run to hand
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
- [ ] **Q230. The gadget's arm jumps out and in, where the original's slides. Decode first.** Found by Q113. The
  arm's handler `LAB_004a14f0` (installed at `0x004a2387`) is a jump table over messages `0xa` to `0x100` working a
  state in the control's own `+0x134`; `FUN_004a25f0` reads states 3 and 4 as "still moving", and `FUN_004a2590` is
  the one way a panel gets onto the arm. Neither how far a step moves nor how long the slide takes is traced. The two
  ends are known: in, the arm's right edge is 645 left of the stream's (`hud.md`, "The arm and the aerial as built").
  Here `ParkGadget.PutArm` sets it out or in at once, counted as `GADGET_ARM_SLIDE`; in, the arm and its end draw
  nothing and the retract button is hidden, where the original narrows the arm and switches the button off. Decode
  the states, the step and its clock, and how a mesh is drawn on a narrowed control; measure the slide's length in the
  original's frames; then build it on `Time.Delta` (rule 10). Confirm: frames every 50 ms of the arm going out, beside
  the original's.
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

- [ ] **Q290. The queue's stamp is taken before the queue is measured, where the original measures first.** From
  the week-end audit (stage 8, the older decode read a second time from the listing; verified once, not reproduced
  in the game). `ParkState` takes `NextCounter()` before `InvalidateQueue` and the back-cell lookup, and its
  comment says so, where the executable measures the queue first and takes the counter after (`ride-operation.md`,
  the row for the queue's remeasure). `OpenTPW/World/Park/ParkState.cs`, line 1688. Confirm: the back cell's stamp
  read in both games after one queue edit.

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
  by `LobbyModel` and wrap too (`SignTexture.TryBuild`). The original clamps at every site
  (`render-states.md`, "Texture addressing is CLAMP"). With wrap plus mipmaps, a tile's edge samples the far edge. Use clamp
  where the original does. Confirm: screenshot two paths in a column, a crossroads and a ride sign,
  before and after.
  Every model texture is unclamped too (the 2026-09-26 staleness audit): `LobbyModel` asks `TextureFlags.Repeat`,
  as `ParkPaths` and `ParkGround` do, but all three bind an array, which `Material.Set` samples as Wrap whatever
  the flag; `Texture.SamplerFor`'s Mirror is used only for a texture bound alone. Check that a model's UVs stay inside 0 to 1 before clamping it, since six
  register-pushed sites are not covered (`render-states.md`); the sea keeps its wrap.
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

- [ ] **Q239. Research's two message boxes.** From Q139. The gadget's Research button and R both reach
  `FUN_004aa480`, which with the game menu down closes the open park screen (`FUN_00485b40`) and opens a message box:
  UITEXT `0x1d4`, "Research is automatic in Instant Action mode.", when the game type is 2 (Lost Kingdom's Easymode),
  else UITEXT `0x1d3` when `mFirstResearcher` is nought (`hud.md`, "The nine screens behind Info, Money and
  Research"). The message box is built; both sites only count (`RESEARCH_BUTTON`, `RESEARCH_SHORTCUT`). Build the two
  boxes at both; the research screen behind them stays counted (blocked: `docs/PLAYER-GAPS.md`). Confirm: R and the
  button in Lost Kingdom, the box photographed beside the original's.

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
  From Q255: a ferry or a seaplane that is current at a save is written as none and counted
  (`SAVE_PARK_ARRIVAL_VEHICLE`) where the file names no thing for its size, as every Lost Kingdom file to hand
  does: the thing made on demand is what gives the writer a handle to write (`saves.md`, "OpenTPW's writer, the
  arrival vehicle").
- [ ] **Q27. Pushing the mouse at the screen edge does not scroll.** The "push scroll" option exists and
  is read by nothing. `ParkOrbitCameraMode.Update` scrolls from keys only. Decode the camera
  binding table at `0x00748158` (`park-engine.md`, "The park camera"), then build.
- [ ] **Q28. Renaming parks and rides.** The save carries a name per park (`saves.md`, the `gms.dat` theme record's park name). No rename
  control exists; signs read fixed names (`ParkFixedItems.BuildSign`, `ParkObjects.BuildSign`,
  `LobbyIsland.DisplayNames`). Decode where the original edits the name and what it writes, then build the
  control and make the three sign painters read the save's name.
- [ ] **Q29. Parity with the original's hardware renderer.** `render-states.md`, "Texture addressing is CLAMP", counts the
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

## E. Large
- [ ] **Q31b. The other seven object windows.** After Q31's shop window: the sideshow's, the toilet's and the rest
  of `park-engine.md`'s nine, one window per session. Q31 carries the decoded figures and the `UIStrings` rows.
  From Q251: the staff window's days employed are `FUN_00505b70`, the calendar's now less `mTimeHired` over a
  day, handed to UITEXT `0x1bb` by `FUN_004b58b0` (`saves.md`, "`mTimeHired`"); `Staff.TimeHired` holds it.
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
