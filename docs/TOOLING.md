# Tooling recipes

The planned recipes file (`docs/README.md`). The recipes `docs/MEMORY-DIET.md` still owes it stay owed. Machine
paths are in `CLAUDE.local.md`.

## The tools every item reuses

`CLAUDE.md` rule 21: read this before writing a harness script, use what is here, and add a row for anything built
that a later item could use. The files are outside the repo, in the harness folder (`CLAUDE.local.md`): `lib/` is
shared by every item, `original/` drives the original, and a `q<N>/` folder is one item's own evidence, not a
place to look for tools. Each file's docstring is its manual.

| Tool | Use it for |
|---|---|
| `lib/parkrun.py` | The scaffold of every save-and-load confirm run: a private game folder, `enter()`, `save()`, `load()`, `reply()`, `gaps( pattern )`, `shot()`, `predict()` and `said()`, `done()`; `OFFSCREEN = True` for a run beside the desktop's; `GAME_FOLDER = "<path>"` for a game folder of the script's own (`lib/gamefolder.py`); `AUDIO_FILE = "<path>"` writes the game's own mix to a file, nothing to the speakers; `original_view( x, y )` for a saved camera the original's frame will show the point in |
| `lib/gamefolder.py` | A private game folder whose data differs from the real game's in the files named: `make( dest, { "data/Advisor/Advisor.sam": change } )` links everything else and writes a changed copy of each file (`once( old, new )` is a change that fails unless `old` is there once); hand the folder to `parkrun.py` as `GAME_FOLDER`, on the desktop or off it. For a balance key changed in this game without touching `/home/alex/Games/TPWorld` (Q338) |
| `lib/mixmatch.py` | One sample measured in OpenTPW's own mix (`AUDIO_FILE`): `template( sdt, index )` decodes a bank's entry, `window( file, from, to )` cuts the capture between two sizes, `match()` is the best match and its least-squares gain, `steady()` the gain of a looped sample under other sounds, from 0.3 s pieces in 2 s steps |
| `lib/parkscene.py` | exec'd after `parkrun.py`: the one-line census readers (`var`, `thing_line`, `staff_line`, `bumper_line`, `field`) and the Hot Pot scenes (`hot_pot`, `in_a_go`, `broken_pot`), so a confirm script does not write them again |
| `lib/stockscene.py <outdir> <bin> <dest.TPWS> <TICK> [command ...]` | A scene file made from the stock Lost Kingdom park, off-screen: entered, paused on the first tick at or past TICK, each console command given in turn (`sell 13`, `buy 1101 41 22 0`), the rides, shops and sideshows listed and the park saved to `<dest.TPWS>`. A stamp or a flag is then written into a copy with `parkfile.thing_set` |
| `lib/parkfile.py` | The one reader of a park file: `advisor()` (the advisor thing's message records not fresh; Q313), `advisor_forget( src, dst, message )` (a copy of a jungle file whose advisor has never said a message, its block packed again; Q316), `advisor_slots()` and `advisor_clocks()` (his eight slots, the messages waiting to be said, and his last action, kept message and message playing; `-a` prints all three; Q314), `clock()` (the clock module's two dwords, the clock and the second stopwatch; `-a` says how much of the advisor's wait the file was saved with; Q315), `Park` (things, cells, sprites), `track()` (the track-rides module, cars and riders), `models()` (every model record: item, flags, node words, lookup records, channels), `head()` (a sprite), `emitters()`, `emitter_words()` and `emitter_aim()` (the particles module's live emitters by their chains, where one is not its template, and its own velocity at `+0x38`; `parkfile.py -e <file>` prints them), `tags()` (the interface block that ends the body: each aerial tag's id, text, life, picture, buttons, thing and item, with the bytes the original leaves unwritten; `parkfile.py -t <file>...` prints them), `thing_set( src, dst, thing, offset, fmt, value )` (a copy of a file with one field of one thing's record written over and the block packed again, answering the old value: how a state no screen of either game reaches is put in a file; Q325), `analyser()` and `filetime( tick )` (the park analyser's last 76 bytes by the serialiser's names, its six calendar stamps among them, `mLastStaffHired` at the record's `+73524`, and the calendar on a tick as such a stamp; `parkfile.py -s <file>...` prints them beside the staff's newest `mTimeHired`; Q333), `vehicles()` (the arrival vehicles: the header's three slots and current vehicle, the object list, cell (0,0)'s chain and each slot's thing's record by field and raw; `parkfile.py -v <file>...` prints them; Q342). Add a module's reader here, not in an item's folder |
| `lib/sfxmap.py <data dir> [<cat_..SFX.map> [effect]]` | A sound category's effects and variation headers (flags, parameter, volume, pitch, wait, range), walked as the loader walks them; with the data folder alone, the spread of every variation's range over all 31 files. Importable (`Category`, `categories`) |
| `lib/responses.py [ID ...]` | The advisor's response table read from `testme.exe` on disk (610 rows: sample, lip file, gesture, model slot and bank, the two props, group): the spread of slots and props, or the rows named. Importable (`rows()`, `off( va )`, which turns any address of the image into a file offset) |
| `lib/advisorschema.py [OFFSET ... \| all]` | The advisor's `.sam` schema walked from `testme.exe` on disk: the `Advisor.sam` key behind each offset of the balance object `0x00fb3540`, which is `ECX` in every score function of the advisor's metadata table (`[ECX + 0x5e4]` is `TutorialUIPanel.Score`). The array's four extra bytes are anchored on three known offsets, not read. Importable (`walk()`) |
| `lib/tagtable.py [ID ... \| groups]` | The tag table read from `testme.exe` on disk (382 rows: id, key, picture, string, the bar's two buttons, life, text kind): the spread of each field, the rows named, or every response group with its row. The tag a said advisor line leaves on the gadget's aerial is the row of its response's group. Importable (`rows()`, `find( id )`) |
| `lib/hidelists.py <advisor.wad> <stem>` | Every clip's hide list of a model in a wad (`<stem>m1.md2` on), each node by name: what a clip's start hides |
| `lib/md2surf.py <wad> <member.md2> [x z]` / `--sweep <data dir>` | A model's header float, lookup records and the faces a point stands on, with the height under a model point; the sweep counts them over every wad. Importable (`Model`): a reader of a `.md2`'s header, lookup table, meshes, vertices and faces to build a model check on |
| `lib/parkrun-smoke.py <out> <bin> [off]` | The scaffold's own check: enters Lost Kingdom, saves under pause, loads the file back from the Load Park list and compares the three ticks; `off` runs it on the private display |
| `lib/mutate.py <mutations.py>` | Putting an item's bugs back, in parallel, in copies of the tree (`WORKFLOW.md`, "Verifying") |
| `lib/stryker.sh <project> <File.cs>...` | Mechanical bugs in a file, Stryker.NET |
| `original/loadfile.sh load <file> <outdir> <tag>` / `stop` | A park file loaded in the original from its Load Park list, off-screen, with frames; `POLL=` starts a memory reader just before the click. It starts the original only when it is not running, so a second file is 35 s where the first is 123 s |
| `original/heads.py PID SECS ITEM` | The original's memory: every model of an item with its lookup state (shared flags, heads attached, each record holding one) and each head's sprite, place, scale and frame, a line a change. A `POLL` for `loadfile.sh` |
| `original/emitters.py PID SECS` | The original's memory: the particle system's live emitters by the used chain (slot, count, handle, effect, place, aim, life, particles alive), a block a change and the particle counts a second. A `POLL` for `loadfile.sh` |
| `original/scriptvar.py PID THING [INDEX [VALUE]]` | A ride script's variables in the running original, by thing: read, or **written** (the one tool that writes its memory). It is how a ride is broken on demand |
| `original/object.py PID SECS THING [VAR ...]` | The original's memory: one object's `mState`, `mCanLoad`, the guest being loaded, the queue's head, the member assigned, the State of repair and the remaining life, and those variables of its script, a line a change with `mGameTick`. A `POLL` for `loadfile.sh` |
| `original/thingwords.py PID SECS THING:OFFSET[:FMT[:NAME]] ...` | The original's memory: any words of any things, a line a change with `mGameTick`. Use it for a field or two before writing a poller: the mechanics' search cursor is `2:0xc`, a mechanic's state, job and count `26:0x19c:I`, `26:0x218`, `26:0x214:I`. A `POLL` for `loadfile.sh` |
| `original/spritepass.py PID`, `particleheights.py PID SECS EFFECT [STEP]`, `particleoverlay.py PID OUT.png [EFFECT]` | The original's world particles: the sprite pass's matrix (`[0x0087b088]`) with every emitter's draw words and its particles projected to pixels; how high and wide an effect's particles go over a time; and a frame grabbed with each particle's sprite marked on it, which proves a projection rule against the picture |
| `original/particledump.py PID EFFECT [raw]` | The original's memory: EVERY particle of each live emitter of an effect (its place less the emitter's, velocity, size, frame, colour) and the spread of each, where `spritepass.py` prints an emitter's first six. OpenTPW's side is the console's `particles <slot>`; `q261/cloud.py <run.out>` prints the same spread from a run's output |
| `lib/spritesheet.py <bank.ESP> <out.png> <set>...` | A sprite bank's picture sets with their alpha, each frame over black and over white, and each frame's alpha spread and mean colour: what a particle's picture is before any blend. `wadcat --dump SPR_PA.ESP` and `--dump SPR_PA.TPC` on `esprites.wad` write the two files it reads |
| `original/voices.py PID [SECS]` | The original's memory: every live sound voice by the service's own list (its effect and flags word, the voice's flags, place, range and rectangle, how far it is from the listener), with the listener first; a block a change. Says which placed sounds are in range and sounding. A `POLL` for `loadfile.sh` |
| `original/mixer.py PID [SECS]` | The original's memory, inside `QMixer.dll`: the session and its listener, and every open channel's flags, place, distance mapping, volume, the mixer's own distance, and its cone, distance and final gains, with the gain the law gives beside the one read. How loud the mixer is playing a voice, where `voices.py` says only whether it sounds. A `POLL` for `loadfile.sh` |
| `original/crowdmix.py PID SECS` | The original's memory: the crowd's voice (level, handle, keys, values, the current variation with its weight, the sample record) beside every flat mixer channel's volume of 127, in ONE log stamped with the wall clock and `mGameTick`, a line a change, 20 ms a poll. Use it where `crowd.py` and `mixer.py` would have to be held together by eye; `mixer.py` prints nothing for a change of volume alone. A `POLL` for `loadfile.sh` |
| `original/mixfreq.py PID SECS [2d]` | The original's memory, inside `QMixer.dll`: every open 3D channel's place, far end, volume of 127 and FREQUENCY (`+0x1f4`), a stamped line each time one opens or changes, and at the end a count by far end, volume and frequency. Which pitch each sample of an effect plays at; `2d` reads the flat channels instead. A `POLL` for `loadfile.sh` |
| `original/rainmix.py PID SECS` | The original's memory: the weather thing's quality, forecast, drops, target and its own rain handle, the global rain handle, the two rain voices (controller keys and values, the current variation's volume bytes) and every flat mixer channel under 40 of 127 with its frequency, in ONE stamped log, a line a change. How loud the rain is and what sets it; `crowdmix.py` is the same for the crowd's voice. A `POLL` for `loadfile.sh` |
| `original/advisor.py PID SECS MESSAGE[,MESSAGE...] [THING]` | The original's memory: the park advisor (the thing `FUN_005194d0` answers): each named message's own record (the tick last said, the line, said, slaps), the eight slots (subject thing, line asked for, message, score), the last action's start and duration, and a thing's State of repair beside them, a line a change with `mGameTick`. Says whether and when he was told a message and whether he said it. A last argument `poll` ends each line with the poll's cursor (`[0x00fb3540]`, the row of the metadata table the next tick starts from; Q316). A `POLL` for `loadfile.sh` |
| `original/advisortable.py PID [meta_full.json]` | The original's memory: the advisor's metadata table as the running game holds it (the image holds zeros): the polled rows as ranges with their count, a few rows' places in a round, and every dword where it differs from the emulated table (two, both row 0's). A `POLL` for `loadfile.sh` |
| `original/bytehold.py PID SECS ADDR=VALUE [TICK ADDR=VALUE]` | The original's memory, **written**: holds one byte of the image at a value, and at another once `mGameTick` passes a tick, a line a write. The Options screen's Tutorial switch is `0x0078d90d`. Stop the original afterwards |
| `original/waiting.py PID SECS` | The original's memory: the park analyser's count of guests waiting for a shut park to open (its `+0x1f0d4`, written each time the advisor's row 1 is scored past its two early ends, -999 before), beside the same count taken live (things of type 1 in state 3 on the two ticket booths' cells, by id), `mParkClosed` and every guest's state, a line a change with the wall clock and `mGameTick`. A `POLL` for `loadfile.sh` |
| `original/tags.py PID SECS` | The original's memory: the tags on the gadget's aerial (how many are held, each one's control rectangle on the 2048 x 1536 layout and its text, the mast's rectangle, the tag on the bar), a line a change with the wall clock and `mGameTick`. Says when a line leaves a tag, where it stands and when it goes. A `POLL` for `loadfile.sh` |
| `original/screens.py PID SECS` | The original's memory: what the gadget's three category buttons are left on (`[0x007cb28c]`, `[0x007cb2a0]`, `[0x007cb290]`) and which park screen is up (the open screen `[0x007c24c8]`, the buy screen `[0x007cc1f8]`, the hire screen `[0x007ca308]`, each pointer printed, so a screen destroyed and built again shows) and the hire screen's tab (`[0x007ca30c]`, `hiretab`, 0 the cleaners' to 4 the scientists'), a line a change with the wall clock and `mGameTick`. Says what a click opened without a frame. A `POLL` for `loadfile.sh` |
| `original/bar.py PID SECS` | The original's memory: the message bar (its rectangle, shown bit, parent, its data's thing, button types, item and text, each child's rectangle), the tag on show and the one being put on show, the gadget's body with its children two deep, the arm's handle, and each tag's rectangle, shown bit, timer byte and whether a stream plays on it, the changed parts a change with the wall clock and `mGameTick`. Says what a click on a tag, on the bar or on the camcorder button does to the arm, step by step. A `POLL` for `loadfile.sh` |
| `original/breakq.py PID SECS THING [all]` | The original's memory, **written**: breaks a ride (`VAR_BREAKSTAT`) each time it is open with a queue that has stood four seconds, so every break puts a queue out, a line a break. With `all` it first writes nought over the mask byte at `0x0050133f`, so every guest dismissed from a queue plays kids `0x80`, not one in eight. Stop the original afterwards |
| `original/inside.py PID SECS [LEVEL]` | The original's memory: the guests inside the park as the analyser counts them (`FUN_004c9130`: a guest on a cell of type 0, 1, 3, 9 or 10) and those of them whose toilet need is over LEVEL (60 unless given), with their ids, the share in whole per cent the advisor's row 82 sets against 25, and the door, a line a change with `mGameTick`. Says who the advisor's and the gate's counts hold on a tick. A `POLL` for `loadfile.sh` (Q347) |
| `original/worldwords.py PID SECS OFFSET[:FMT[:NAME]] ...` | The original's memory: any words of the world (`[0x0080239c]` + OFFSET), a line a change with `mGameTick`: the arrival vehicle that is current and the three slots' things are `0x1da72a:H:current 0x1da72c:H:size1 0x1da72e:H:size2 0x1da730:H:size3`. A `POLL` for `loadfile.sh`; `thingwords.py` reads a thing's |
| `original/brk.sh ADDR [HITS] [SECS] [WORDS]` | Who CALLS a routine of the original: a hardware breakpoint through `gdb`, attached and detached again; at each hit `mGameTick`, the registers and the stack's words that point into the game's code, nearest first (the first is the caller's return address; read each in Ghidra, a real one follows a `CALL`). `watch.sh` says who writes an address, this says why. `BRK_PRINT='printf "fctrl %x st0 %.17g st1 %.17g\n", $fctrl, (double)$st0, (double)$st1'` adds one gdb line a hit: the x87's control word and stack at a compare |
| `original/thingpoke.py PID SECS KIND THING:OFFSET:FMT=VALUE ...` | The original's memory, **written**: holds words of things at given values, written again whenever they read otherwise, a line a write; refuses a thing whose type byte (`+2`) is not KIND. `150 15 7:0x38:B=1` holds the weather's `mLightning`, so bolts and thunder come on a clear day. `thingwords.py` reads the same words. Stop the original afterwards |
| `original/mixpairs.py [<mixer.py log>]` | A `mixer.py` log against the category files: every pair of a 3D channel's far end and volume of 127, with the variation volumes of that range that `round( v x 0.75 )` makes it from. Says which effects' channels a log holds, and that none plays at a number the files cannot make |
| `original/modelboxes.py PID [FIRST LAST]` | The original's memory: each thing's model record's cell rectangle (`+0xc0`, `+0xc4`), whether it has a `.hmp`, the box's middle (where its scream is sounded) and its base height. Read the objects' lines (the stock park's are things 13 to 24): a person's `+0x20` is no model handle, and its line is noise |
| `original/research.py <in> <out> <item> [0]` | A copy of a jungle park file with one item marked researched, so Instant Action's buy list offers it; with a last `0`, marked not researched, which both games keep for an item whose research costs nothing (Q335) |
| `original/original.sh`, `tpwmem.py`, `gmove.py`, `record.sh`, `watch.sh` | Starting, reading, clicking, filming and watchpointing the original ("The original under Proton", below) |
| `original/timens.py SECONDS CMD...` | A command run in a time namespace where the computer has been up SECONDS: the original's park clock without a restart (warning 1, below). **It fails outside the VS Code sandbox** (`PermissionError` on `/proc/self/setgroups`, tried 2026-10-10), so a session started from a plain terminal cannot use it as it is |
| `original/kwinwl-start.sh` | A private display on `:78` drawn by the graphics card (`kwin_wayland --virtual` with Xwayland) for the original, where Xvfb draws by software. `:78` is `parkrun.py`'s too. **Not run since it was written**: try it before leaning on it |
| `q119/lib.py` | What `parkrun.py` is built on: the launch, the console pipe, XTEST clicks and keys, the frame grab. Use it through `parkrun.py` |
| `gen_addresses.py` | Regenerating `docs/exe/addresses.md` |
| `wadcat`, `strdump`, `nodenames/` | A wad's entries, a string table's lines, a UI wad's node names (`CLAUDE.local.md`) |

**A poller of the original's memory is the one thing still written an item at a time** (`q257r/orig/cars.py`,
`q257x/orig/smoke.py` for a car's smoke handle, its emitter and its node's place, `q257t/orig/chan.py` for a car's model's channel stamps, `q253/orig/look.py`; a model's heads are `original/heads.py`, above): each reads its own structures. Start from the newest that reads the same
structure, and hand it to `loadfile.sh` through `POLL`.

## Recipes

Each was found by trial in the item named, and cost a run to find.

**In OpenTPW, through the console** (`OPENTPW_DEBUG_CONSOLE=1`; the `case` labels in `DebugConsole.cs` are the
whole list):

- **`camera x y zoom yaw` takes world units**, a cell times ten (Q257n).
- **Is a float product rounded before its compare** (Q343): break on the `FCOMPP` with `BRK_PRINT` (`brk.sh`'s
  row) as the `POLL` of a `loadfile.sh` load: `fctrl` 0x27f is 53-bit precision, so `FILD`, `FMUL float ptr` is a
  double of the float key (five times the float nearest 0.2 is more than one). The same hit gives the row's turn
  (its tick) and both sides of the compare.
- **A polled row whose shipped score is under the advisor's 25** (Q344, row 79's `Score` 20): neither game can
  say it as shipped, so both are run under a raised `Score` beside the key whose edge is asked (`q344/confirm.py`,
  `q344/orig/run.sh` with `POORER=`, `MORE=`, `SCORE=`); `Score` 25 is the control that the limit is strict
  (`result=under-threshold` here, a fresh record there). A whole-number share's edge is one key apart: 62 silent
  and 63 said pins five kinds of eight in both games. **The leavers' summons in this game is not on a fixed tick**
  (1093 in Q341's runs, 1139 in Q344's): a day-29 check asks whether a stamp came before the row's next turn.
- **A polled row whose first sayable turn is late** (Q345, row 81 on tick 1593): one scene run saves the park
  at each tick wanted (`q345/scene.py`: `save()` then a copy, as often as needed; a purchase and a sale between
  saves make the controls), so four files cost one run. **A spent bus is summoned again on a fixed tick of the
  stock park (1264, 1438, 1600 here) and both games may draw a ferry or seaplane there, which stamps the newest
  build**: read the scene run's `summoned` lines and save between two of them, never just before one that falls
  on the row's turn. A day-short file is seen said a turn later only in the original with its stamp held
  (`EXTRA="python3 -u .../thingpoke.py %PID% 150 13 5:0x21c30:Q=<stamp>"`). **A tag lives thirty seconds**: after
  `loadfile.sh` returns (some 20 s after the click) a `run.sh` that takes frames first clicks a tag that is
  ending; `WAIT60=0 SECS60=0 CLICKS=1` clicks in time, and `GRAB=1` has the frames of him speaking.
- **Four kinds of ride in an Instant Action park** (Q343): `stockscene.py ... 912 "buy 1101 41 22 0" "buy 1102 70
  40 0" "buy 1104 55 30 0"` (a Crazy Ape, a Rocky Racers and an Aztec Mayhem beside the stock Belly Bounce, each
  left shut as bought, which both games count as a kind standing). The Instant Action catalogue holds no fifth.
- **A balance key changed in both games** (Q338): this game from a private folder (`lib/gamefolder.py`, then
  `GAME_FOLDER` before `parkrun.py`; an off-screen run wants a folder of its own beside the desktop's). The
  original reads `~/Games/TPW-original/Data/Advisor/Advisor.sam` as it starts: stop it, check the file's sum is
  the shipped one, keep a copy, write the key, run, stop, put the copy back in a `trap` and print the sum
  (`q338/orig/run.sh`, `SAM=`). The loader takes a float as written (`ShopsPerRideRatio 1.5`). A run in which
  nothing is said prints no line after its first: add `EXTRA='python3 -u .../thingwords.py %PID% 60 29:0x8:H
  35:0x8:H 31:0x8:H'` so guests' places stamp its ticks.
- **A row that waits thirty days from the newest build, in the original** (Q340): its stamp
  (`thingwords.py ... 5:0x21c30:Q:lastbuilt`) moves with nothing bought near tick 1070 to 1140 and again some
  340 ticks on, in every load of a stock-park file, so a turn after tick 1069 is not said there. Either put the
  turn before it (Q335 to Q339's turns, 957 to 967, are), or hold the stamp at the file's value for a
  like-for-like and say so: `EXTRA='python3 -u .../thingpoke.py %PID% 110 13 5:0x21c30:Q=<stamp>'` in
  `q340/orig/run.sh` (the analyser, thing 5, is of type 13; `Q` works). Left alone it speaks on its first turn
  692 ticks past the last move (tick 2209 in `q340/late30.TPWS`: `SECS=370 WAITN=720`). What moves it is the
  ferry or seaplane made at its first summons (Q341), so twice a stock-park file and never after.
- **Who calls a routine, and when** (Q341): after a `loadfile.sh` load, `brk.sh 0x004c7680 2 240` waits for the
  routine and prints the tick and the return addresses on the stack; look each up with Ghidra's
  `decompile_function_by_address` (the one after a `CALL` is real). What happens in the first seconds after a
  load is missed by the attach: read the next occurrence, or the words it changes with a poller in `POLL=`.
- **A row no shipped park can score** (Q339): change its key in both games (above) AND, where the count is of
  kinds on offer, mark items researched in the file with `original/research.py`: the original keeps a
  sideshow's researched byte as written, as a shop's (four on its "Buy Sideshow" list). Each run of the
  original on a changed `Advisor.sam` is a program of its own (about three minutes): put the silent ones in one
  background chain with `EXTRA=` `thingwords.py` on three guests' places, and read each log's last tick.
- **A park with a ride and no shop is the stock park with thing 16 sold** (Q337): `sell 16` under a pause takes
  the Drinks Shop, its only shop; `sell 13` takes the Belly Bounce, its only ride. A sale leaves
  `mLastObjectBuilt`, so the days since the build are still set by writing the stamp into the file
  (`parkfile.thing_set` on thing 5, `+73516`; `q337/mk.py`).
- **`step N` steps frames, not ticks** (Q334): twenty of them moved the park's clock two ticks. To let ticks pass
  under a script, `resume`, wait on `tick()`, and `pause` again.
- **Who writes and who reads a field of a big thing** (Q334): `search_bytes` on its displacement's four bytes
  (`30 1c 02 00` for the analyser's `+0x21c30`) lists every instruction that names it, in a function or not; a
  routine nothing is seen to call is called from undefined bytes, found by scanning `testme.exe` on disk for
  `e8` and a relative target (`responses.off( va )` turns an address into a file offset), and the caller's row
  of the advisor's table is the one whose score address is the nearest at or below the call.
- **A tired member of staff, from the first sweeps of a load** (Q326): write the member's rest into the file, the
  float at the staff record's `+499` (`parkfile.thing_set( src, dst, 28, 499, '<f', 1.5 )`), where the console's
  `staffrest` and a write to the original's memory each need a run already going. The stock file's guard 28 is
  idle on stamp 752, so he is tired on tick 763 in both games, is in the Staff Room on tick 891 and up again on
  1088 (grade 3, 0.5 a tick). His walk is 128 ticks, half a minute: a script that watches it needs more than
  150 s from the load to the rest's end. `thingwords.py %PID% 170 28:0x19c:i:state 28:0x208:H:restarea
  28:0x1fc:f:rest` and `object.py %PID% 170 20 0` (the room's `VAR_STAFFIN`) read the original's side
  (`q326/orig/run.sh`).
- **The speaker is free from the entry to a park or a load** (Q316): the stock park says no opening line, its file
  holding the control panel's line, message `0xb9`, as said, so a line posted in the first seconds is heard. The
  older scripts' 23 s waits after a load are no longer needed.
- **The control panel's line, on demand:** enter a file whose record of `0xb9` is fresh
  (`parkfile.advisor_forget( stock, out, 185 )`, copied into the player's jungle folder as the newest): he says
  sample 377 on tick 877 in both games, 30 s in, for 19 to 21 s (the log's `Park advisor: poll message=185 ...
  result=post`): the poll's cursor is the file's (row 277 in the stock file; `advisor.py ... poll` prints the
  original's). A park made with no file, in a fresh program, says it on tick 265:
  `SLOT_CONTROL = "7a19"` before `parkrun.py` enters as the second player, whose jungle folder is empty (`7a15` is
  the first player's name, not a slot); `tutorial off` in the console has the poll answer `tutorial-off`
  (`q316/confirm.py fresh|nofile|cutoff|early`, `q316/orig/run.sh`). In the original the Tutorial switch is the byte
  `0x0078d90d`: `bytehold.py %PID% 110 0x0078d90d=0 900 0x0078d90d=1` beside `advisor.py` holds it off through a
  load and until tick 900 (`q316/orig/run5.sh`).
- **The tips' line, on demand, a second after a load** (Q317): a file whose record of `0x11a` is fresh
  (`parkfile.advisor_forget( stock, out, 282 )`) has him say sample 559 on tick 760 in both games, the fifth sweep
  after the stock file's load, for 10.7 s. `loadfile.sh`'s own frames come 12 s and 21 s after its click, too late
  for a line that short: start a grabber of `:77` that waits for the `-1-load-list.png` frame and then grabs every
  1.2 s (`q317/orig/run2.sh`).
- **A message waiting in the advisor's slots, on demand:** `broken_pot()`, wait for the log's `attempt=N message=70`
  (the mechanic's line for the pot, 6.5 s long), then `scriptvar 13 VAR_BREAKSTAT 1`: the Bounce's `post message=69
  ... result=queued` waits until that line ends. `pause` and `savepark` inside it write a file with the slot taken;
  the original says it a tick after loading such a file, and `loadfile.sh`'s log shows it twice (it starts the
  original on the file, then loads it from the list) (Q314). Since Q315 the same file holds the line's wait too:
  both games then say the waiting message when the wait ends, about six seconds after the load, not a tick after
  it. `parkfile.py -a <file>` says how many milliseconds of the wait a file was saved with, and
  `q257x/orig/broken-by-the-original.TPWS` is a file of the original's own saved inside one (959 ms) (Q315).
- **A shut park, and the advisor's line about it:** `screen 10` opens the entry-price screen, `control 4f3b1`
  answers the door's place, and a real click there shuts the park (no console command does); Escape closes the
  screen. Done inside eight seconds of the stock park's entry (by tick 800), the guests still at the booths wait
  there for 200 to 349 sweeps, `gate` ends with `waiting N`, and the advisor's row 1 is polled on tick 923 (768
  and every 155 on): with two or more waiting he says sample 1 in the ticket hat and leaves tag 3, whose bar's
  first button (`click 265 549` with the bar out, the tag at `click 56 441`) opens the entry-price screen
  (`q319/confirm.py`). **A file of that park saved before tick 899 has both games say a line about shut shops
  first** (rows 260 to 262 are polled on 899 to 901: sample 487 on tick 899, then the toilets' and the sideshows'
  lines and row 1's in the order of their scores, each as the wait before it ends), which holds row 1's line back
  to tick 942 or later: save past tick 902 (`pause`, `savepark`) for a file both games say row 1's on tick 923 from
  (`q319/orig/run.sh <file>`, with `CLICKS=0` for no clicks; Q319). The four lines in both games:
  `q320/confirm.py`, `q320/orig/run.sh <file>` (Q320).
- **An open park with nothing to ride, and the advisor's line about it:** in the stock park `pause` on a tick
  from 912 to 919, `sell 13` (the Belly Bounce), `sell 14` (the Jungle Spray), `resume`: row 2 is polled on tick
  924 (769 and every 155 on) and he says sample 4 in the ticket hat and leaves tag 4, whose bar's first button
  opens the entry-price screen as tag 3's does. `savepark` under that pause writes a file the original says the
  line on tick 924 from too, and then row 62's (on tick 968; this game as row 2's wait ends, 961). The
  original's line lasts from 2.5 s to 13 s after the load's click, before `loadfile.sh`'s own frames: put
  `(sleep 6; import -window root -display :77 out.png) &` in its `POLL` (`q321/confirm.py`, `q321/orig/run.sh`
  with `GRAB=1`; Q321).
- **Worn rides and no mechanic, and the advisor's line about it** (Q331): the line needs six rides, so the scene
  is a file: `q331/scene.py <out> <bin> fired` (the stock park paused on tick 915, `fire 26`, `buy 1101 41 22 0`,
  `buy 1104 57 23`, `buy 1101 70 40 0`, `buy 1101 30 60 0`, `buy 1101 60 60 0`, `savepark`), then
  `q331/worn.py <src> <dst> 13=24 43=24 ...`, which writes each ride's State of repair (the float at an object's
  record's `+1074`) through `parkfile.thing_set`. Row 5 is polled on tick 927 (772 and every 155 on): with six
  rides under 25 he says sample 13 in the hard hat and leaves tag 7, whose bar's first button (`click 265 549`,
  the tag at `click 56 441`) builds the hire screen on its mechanics' tab (`windows` names the hire screen's
  tab). A ride at exactly 25, or the mechanic kept, and nothing is said. In the original `screens.py` prints the
  tab (`hiretab`); `loadfile.sh` comes back some twenty seconds after its click, so a frame of a line said three
  seconds in is grabbed from the `POLL` (`q331/orig/run.sh` with `GRAB=1`; `STOP=0` keeps the original up for
  the next file). The same file has both games say row 113's line, the unjoined queue's, on tick 996.
- **More rides than the mechanics keep, and the advisor's line about it** (Q333): row 7 is nought until thirty
  days after the newest hire, and the shipped park's newest is tick 648, so in a live park the line cannot come
  before tick 1340. To have it on a turn soon after a load, write the stamp into the file: the analyser is thing
  5 and its `mLastStaffHired` the eight bytes at its record's `+73524`, a `FILETIME` (`parkfile.thing_set( src,
  dst, 5, 73524, '<q', parkfile.filetime( tick ) )`; `parkfile.py -s <file>...` prints a file's stamps beside
  the staff's newest `mTimeHired`; `q333/mk.py` made the item's files). 692 ticks after the
  stamp is day 30 and 691 is day 29. The scene is `q333/scene.py` (the stock park paused on tick 915, `fire 26`,
  `buy 1101 41 22 0`, `buy 1101 70 40 0`, `savepark`): with the stamp of tick 237 both games say sample 20 in
  the hard hat on tick 929 (774 and every 155 on) and leave tag 9, whose bar's first button builds the hire
  screen on its mechanics' tab. The same park says row 113's line on tick 996 and row 259's on 1053 in both
  games. In the original `thingwords.py %PID% SECS 5:0x21c38:Q:lasthire` reads the stamp; `hire <candidate> <x>
  <y>` in this game's console stamps it (`candidates` lists the pool), and the log says so.
- **A poor selection of shops, and the advisor's line about it** (Q336): row 71 wants two shops, under 25 per
  cent of the kinds of shop on offer standing, and thirty days since the newest build. **The shipped park
  offers three kinds, so it cannot score there**: mark two more researched in the file first
  (`original/research.py <in> <out> 1206`, then `1212`: five on offer, one kind is 20; both games keep a byte
  written 1 for a shop whose research costs money). `q336/scene.py <out> <bin> base1 1203` is the stock park
  paused on tick 917 with `buy 1203 41 22 0` (a shop on that cell stands open); `q336/mk.py` marks the shops and
  writes the stamp of tick 271 (day 30 on the row's turn, tick 963; 808 and every 155) and of 272 (day 29).
  Both games say sample 205 with no hat on tick 963 and leave tag 73, whose bar's first button builds the buy
  screen on its shops tab ("Buy Shop"). **In the original the frame of him is taken with `GRAB=1`**
  (`a-g10.png`): `run.sh`'s own `told` shot comes after `loadfile.sh` returns, twenty seconds on, when a
  fourteen-second line is over. **A silent file's proof of its tick is the next load's log**, whose first lines
  are the park before it (`c`'s park is read at tick 1136 in `d-load.log`).
- **A poor selection of rides, and the advisor's line about it** (Q335): row 60 wants three rides, under 30 per
  cent of the kinds on offer standing (the shipped park offers four: one kind is 25, two are 50) and thirty days
  since the newest build, the analyser's `mLastObjectBuilt` at thing 5's record's `+73516`
  (`parkfile.thing_set( src, dst, 5, 73516, '<q', parkfile.filetime( tick ) )`; a purchase stamps it with now, so
  it is written back after the scene is saved). `q335/scene.py <out> <bin> base1 2` is the stock park paused on
  tick 916 with `buy 1100 41 22 0` and `buy 1100 70 40 0`; `q335/mk.py` writes the stamp of tick 265 (day 30 on
  the row's turn, tick 957; 802 and every 155) and of 266 (day 29), and `offer3`, the Aztec Mayhem not
  researched (`research.py ... 1104 0`: three on offer, 33 per cent, silent). Both games say sample 173 with no
  hat on tick 957 and leave tag 62, whose bar's first button builds the buy screen on its rides tab. **Load a
  second file into the original only after closing the screen a click opened, or stop it first**: with the buy
  screen up the list's click loads nothing, and the next run's pollers then log the wrong park. **A log of
  several loads is read a stretch at a time**, cut wherever `mGameTick` falls back.
- **Broken rides and no mechanic, and the advisor's line about it** (Q332): **a ride bought and never opened
  does not break** when `scriptvar <thing> VAR_BREAKSTAT 1` is written, so break open ones: the stock Bounce
  (13), a Hot Pot (`parkscene.hot_pot()`) and an Aztec Mayhem with its queue joined and guests sent. Each break
  with no mechanic posts `0x44` (one line, the others inside its cooldown), and three shut rides of three have
  row 259 speak on its turn (898 and every 155): to keep both out of the way, buy two Crazy Apes before tick
  895 so row 259 is said then, sell them, break the rides past tick 1086 and save a few ticks before row 6's
  turn on 1238 (928 and every 155). With three broken he says sample 16 in the hard hat and leaves tag 8, whose
  bar's first button builds the hire screen on its mechanics' tab; with two, nothing. The original, given the
  file, says it on tick 1238 too, then row 124's line on tick 1317 and row 7's from tick 1394 (`q332/scene.py
  <out> <bin> <name> <N>`, `q332/confirm.py`, `q332/orig/run.sh <file>`). **`loadfile.sh` cannot load a second
  file into an original left with a park screen open** (its clicks open no menu and the log holds only the old
  park's first lines): close the screen, or `loadfile.sh stop` and start fresh.
- **A park with no ride, and the advisor's line about it:** the same pause with `sell 13` alone: row 62 is
  polled on tick 959 (804 and every 155 on) and he says sample 179 in the hard hat and leaves tag 64, whose
  bar's first button opens the buy screen on its rides tab (`windows` names the buy screen's tab; open it with
  `control 0x26`'s middle and click `control 0x1fd`'s for the shops tab first, so the button has a tab to
  change). The score grows by 3 a day, so sell later for another number (both sold on tick 1074: 94 in the
  original's slot on tick 1114). `advisor` scores each topic on the tick it is ASKED, not the tick it was said.
  **The original's words for a tag are the american folder's**, which can differ from the English one's that
  this game reads (string 67: "now is" and "now's"): take the words to predict from `strdump` on the folder
  the game under test reads. **Waiting on `loadfile.sh`'s log for a record**, leave out the first start's
  lines, which hold the same record: `grep -v "^ *0\.00 s" log | grep -q ...` (`q322/confirm.py`,
  `q322/scene.py <out> <bin> <name> <tick> <things>`, `q322/orig/run.sh <file>`; Q322).
- **The buy screen opened and closed by real clicks, in both games, and its advisor line:** the gadget's buy
  button is layout point (229, 1181) and the screen's exit button (1930, 859). In the original (1024 x 768) they
  are `gmove.py 114 590` and `965 430`. In OpenTPW the gadget keeps to the window's left, but **a park screen is
  centred in a window wider than 4:3**: scale the layout by the window's height over 1536, then add half of the
  window's width less 2048 times that scale to a screen's x (at 1280 x 720 the exit button is `click 1065 403`,
  not 905). The stock park says the screen's own line (sample 380, about 19 s) a tick after its first opening;
  close inside it and he is cut off, and after three cuts he is asked no more (`q328/confirm.py a|b|off|saved`,
  `q328/orig/run.sh <file> a|b|c`; Q328).
- **The hire screen reached by real clicks, in both games, and its advisor line:** the buy screen's button to
  hire is layout point (1917, 634) and the hire screen's exit button (1930, 859), both a park screen's (centred,
  the recipe above); in the original `gmove.py 958 317` and `965 430`. The same point on the hire screen is its
  button back to buy, so know which screen is up before clicking it. **The gadget's button opens whichever of
  buy and hire was last left on, in both games** (Q330; in the original a global of the running program, kept
  over a load from the list and over Restart Park: stop the original between runs, or leave it on buy). The
  stock file holds the hire line cut off once, so two cuts silence it
  (`q329/confirm.py a|b|off|saved`, `q329/orig/run.sh <file> a|b|c`; Q329).
- **Restart Park and Exit To Lobby by real clicks, in both games:** in the original Escape, then the menu's
  Restart Park row at `gmove.py 510 228` and the question's tick at `708 283` (25 s to the restarted park), or
  Exit To Lobby at `510 573` (no question; 30 s to the lobby, then `660 58` clicked twice enters the park as
  `loadfile.sh` does). **Exit To Lobby writes `autosave.TPWS` into the reference player's jungle folder and
  rewrites `gms.dat` with the same bytes**: `loadfile.sh stop` then says the sum changed; take the autosave out
  (it is the newest file there, so the next start would enter the park on it) and the sum is the one before.
  In OpenTPW `menu`, a click on `control 3`'s middle, then on `control 9873c9`'s, the question's tick (Return
  through XTEST does not answer it); `lobby` and `park jungle` are the console's way out and in
  (`q330/confirm.py fix|restart`, `q330/orig/run.sh <file> r|l`; Q330).
- **The count of unbuilt polled rows is exact, so predict it exactly:** under a `pause` on tick T the stock
  park's `PARK_ADVISOR_POLLED_ROW_NOT_BUILT` is T - 755 less the turns of built rows by then, and a built row
  comes round every 155 ticks with no log line where its answer has not changed (282 on 760 and 915; 1 on 768
  and 923; 2 on 769 and 924; 62 on 804 and 959; 113 on 841 and 996; 185 on 877 and 1032; 259 to 262 on 898 to 901
  and 1053 to 1056; 263 on 902 and 1057). **List a new row's turn BEFORE the file's last one too** (996 less 155 is 841, past the
  file's tick 755: left out of Q324's first prediction).
  `q320/probe.py` reads tick against count at a list of pauses (Q320).
- **An open park with most of its rides shut:** under a `pause` on a tick from 893 to 896, `buy 1101 41 22 0` (a
  Crazy Ape) and `buy 1104 57 23` (an Aztec Mayhem), their queues left unjoined: three rides, two shut. Saved
  there, the original starts a second before the advisor's row 259 on tick 898, before any of its other rows has
  seen the new rides; its row 113 then speaks of the unjoined queue on tick 996. **A line said a second after a
  load is photographed by a grab loop** started from the Load Park list's frame, every 1.2 s for twenty frames
  (`q323/orig/run.sh`; frames 3 to 9 hold the line), since `loadfile.sh`'s own come 6 s and 15 s on (Q323).
- **A queue not joined to a path, and the advisor's line about it:** the same pause with `buy 1104 57 23` alone
  (thing 43, its queue left unjoined): row 113 is polled on tick 996 (841 and every 155 on) and he says sample
  312 with no hat and leaves tag 135, whose bar's first button, the camera's, does nothing (the tag is about no
  thing). `tool queue 43` under the pause keeps the row at nought until `tool off`. The original, given the file,
  says it on tick 996 too; `tpwmem.py --pid %PID% watch 008186ac <secs>` in the `POLL` reads the builder's
  object beside `advisor.py ... 0x71 poll` and `tags.py` (`q324/confirm.py fix|control|joined|armed`,
  `q324/orig/run.sh <file>`; Q324).
- **A shut staff room, and the advisor's line about it:** no screen of either game shuts one, so the scene is a
  file: `parkfile.thing_set( Easymode.TPWI, room-shut.TPWS, 20, 214, '<i', 0 )` (thing 20 is the Staff Room,
  214 an object's `mCanLoad`), copied into the private jungle folder and loaded with `load( 1 )`. Row 263 is
  polled on tick 902 (and every 155 on) and he plays sample 638, the bank's `z_error.mp2`, with no hat and no
  tag, for 3,998 ms; the original, given the file, says it on tick 902 too. **To find a short sound in the
  game's own mix, open the window four ticks before the tick it is owed on**: `tick()` is a console reply and
  comes back up to a second late. **A control that must stay silent in the original needs a tick stamp past
  the turn**: `thingwords.py %PID% <secs> 29:0x8:H 35:0x8:H` (two guests' places) beside `advisor.py` stamps
  the log as they walk (`q325/confirm.py fix|control|open`, `q325/orig/run.sh <file>` with `EXTRA=`; Q325).
- **Open a bought ride:** `buy <item> <x> <y> 0` answers the thing's id and its queue node's cell; then
  `tool queue <thing>`, `worldclick <the node's cell>`, `worldclick <a path cell beside it>`, `tool off`. For a Hot
  Pot on (41,23): `buy 1140 41 23 0`, node (43,22), path (43,21); four boats are out six seconds on (Q257r).
- **Fill a ride:** `admit <x> <y>` on a path cell answers a new guest's id, `send <guest> <thing>` sends them; poll
  the ride's census (`bumpers`, `rides`) until it counts them (Q179d, Q257r).
- **An Aztec Mayhem with riders:** `buy 1104 57 23`, then the queue: `tool queue <thing>`, `worldclick 58 22`,
  `worldclick 56 22`, `tool off`. Two riders are carried about 40 s on, up to five before the ride starts.
  `camera 585 245 25 90` looks down into its pit, where the riders' heads are; from further off its walls hide
  them. In the original the lid is shut over them while the ride runs, so its frame shows no head (Q257u).
- **A Balloon Shop a guest can reach:** `buy 1209 43 22 0` (the review of 2026-10-06, fix 2).
- **Particles a script starts:** `buy 1203 44 33 0` is a second Drinks Shop, whose script opens on its bubbles
  (effect 58 on node 1); `scriptvar 21 VAR_WORN 1` makes the file's Small Toilet 21 start two (9, then 69).
  `emitters` prints the LOADED FILE's emitters; `particles` the running park's, with how many were drawn.
  `camera 445 335 45 180` shows both Drinks Shops' bubbles, `camera 555 170 25 180` the toilet's stink (Q258).
  In the original's frame the bubbles are a few pixels: crop the shop and enlarge it four times (Q257v).
- **The Jungle Spray's jet:** `admit 52 34` four times, each `send <guest> 14`; poll `rides` for a `2:37@`
  record on thing 14 and `pause` at the first: one jet (node 2) is running, for about four seconds (Q257w).
- **A broken Hot Pot with smoking boats:** fill the pot (above), and once `bumpers` reads `state Running` with
  riders seated, `scriptvar <thing> VAR_BREAKSTAT 1`; every boat reads `smoke started`. A break between goes
  smokes no boat (Q257x). `particles` then reads an emitter of effect 2 a boat; `camera 435 320 110 180` shows
  the smoke over the pot, cropped at (560,90)-(730,260) of the 1280 x 720 frame (Q259).
- **The Hot Pot's own smoke** (effect 16, two emitters, started by a break): `broken_pot()`, then
  `camera 435 250 110 0` shows the cloud at its doorway from the side the original's file `hotpot-later2.TPWS`
  looks from (OpenTPW yaw 0 looks toward higher z; yaw 180, Q259's, is the pot's far side), and
  `camera 435 228 35 0` its sign close up. `particles <slot>` prints every particle of one emitter (Q261).
- **The waterfall's spray** (effect 20, the land's own, running from the park's first frame):
  `camera 532 526 25 0` looks at the foot of the fall from the side the water comes down; `camera 532 591 110 180`
  (`parkrun.original_view( 532, 526 )`) saves a view the original's frame shows it in, from the far side (Q262).
- **The mechanic mending a pot:** `broken_pot()` (`lib/parkscene.py`), then wait for the log's `mechanic 26 found
  ride`, `starts repairing` and `finished repairing`: under a minute in all. He stands on (43,23), inside the
  pot's doorway, hidden from yaw 180: `camera 435 235 30 0` looks in at him (Q257z).
- **A placed sound measured in the mix:** `AUDIO_FILE` before `parkrun.py`, `bus music 0`, then **wait 16 s
  after the park is entered**: before Q316 an opening line turned the effects down there (a waterfall read 0.05 to
  0.07 where 0.176 was owed); the stock park now says none, and the wait was not measured again. Take byte marks either side of an 8 s sleep and use `mixmatch.steady`: a 1.5 s
  template scores 0.2 under the park's screams, a 0.3 s piece 1.000 in the gaps. `placed` prints the listener
  and each of the land's sounds with its distance and gain (Q264).
- **A ride that screams, and nothing heard:** the Belly Bounce (thing 13, cell (51,23)) screams inside a minute
  of the stock park's start; `camera 515 235 60 180` puts the listener 34 units off it, and `camera 1100 1100 110
  180` is 750 and more from every sound in the park, where the mix with `bus music 0` is exact nought. `voices`
  prints each ranged voice's place, range, distance and gain; a scream's child lives a second or so, so poll it
  every 0.4 s and gather (Q265).
- **A one-shot measured in the mix:** `AUDIO_FILE`, `bus music 0`, 16 s, then poll `voices` for 40 s between
  two byte marks and `mixmatch.match( window, template, 0.5 )` each sample named: a scream child scores 0.99 to
  1.000 and reads one gain to four places (`q267/confirm.py`; `find( name )` there looks a sample up in
  `KidsHD.sdt` and `xKidsHD.sdt`). The gain is the voice's volume times its distance gain times 0.397 with the
  camera at `515 235 60 180`, the master and the pan together, so compare two builds, not a build with a sum.
  `voices` prints each ranged voice's volume (Q267).
- **A pitched sample found in the mix:** match its template at each pitch (`q297/pitchscan.py`: read at a rate of
  `2 ^ ((p + 1) / 96)`, 1 at pitch 0) over the whole capture. At pitch 0 a hit is 1.000; at another pitch 0.90 to
  0.99, because the template is resampled by a straight line and the mixer steps to the nearest sample, and a miss
  is 0.77 at most: take 0.85 as the bar, not 0.97. A ride's variation repeats its few samples, so hold each
  (sample, pitch) pair of the log against the mix, not each sample once (Q297).
- **Guests put off, each heard:** `admit 52 26` until eight answer an id that divides by eight (only those
  sound), `send <guest> 13` each, wait 30 s, then `scriptvar 13 VAR_BREAKSTAT 1`: the Belly Bounce's queue is put
  out a head a sweep and the log reads a `put off` line each, three to seven a break; the mechanic has it open
  again inside a minute, so send and break again. In the mix the put-off's one sample (`bootout.mp2`) scored 0.95
  at four of six logged pitches and 0.64 and 0.68 at the other two, 0.30 at a pitch not played: take 0.5 as the bar
  (`q298/confirm.py`, Q298).
- **A Hot Pot's engine, read and measured:** `hot_pot(5)` and `in_a_go(120)` (`lib/parkscene.py`), then
  `camera 435 250 60 180`; `voices` names it `Engine.mp2 Effects placed` with its volume, rate and range, and
  `bumpers` prints the lead car's `volume N playing at G`. Its pitch moves each tick, so no template finds it in
  the mix: with `bus music 0` compare the mix's rms over the go's first 12 s between two builds (`q299/confirm.py`,
  Q299).
- **Thunder, read and measured:** `bolt 515 235` aims a bolt and its thunder follows inside three seconds, at the
  bolt's top, which leans up to 50 off: leave nine seconds between bolts or the effect's wait drops one. Its place
  moves the pan, so match its sample in each channel of the capture apart and take the root of the two gains'
  squares: over the volume times `voices`' distance gain that is one number, 0.66, for every thunder
  (`q300/confirm.py`, Q300).
- **The rain alone in the mix:** `bus music 0`, 16 s, `camera 1100 1100 110 180`, then `weather 16`, not 1: a
  quality of 15 or less arms the lightning, and thunder (range 1500) is then the mix. 16 is 360 drops, a level of
  60, reached in 4.5 s. `weather` prints both rain voices (level, variation, volume, what each plays at); the log
  has a line a set and a line a sample. Each `weather N` holds off the park's own next change for some 40 s. The
  two voices are the same sample, a part of a second apart unless the command lands on a weather tick's frame: apart,
  `mixmatch.steady` scores 0.77 and tells no gain and the rms is the two powers' sum; together it scores 1.00 at
  twice a voice's gain. Compare the mix's rms between two builds and allow for both (`q301/confirm.py`, Q301).
- **The gulls and the sea, read and measured:** `camera 400 0 110 180` puts the listener at (400,29,47), inside
  the gulls' rectangle and 8 from the sea's, with every other placed sound out of range; `camera 400 60 110 180`
  is 58 and 68 off, and shows the harbour. `placed` prints each following voice's place, distance, law and gain
  (`gain none` between two samples: poll until it reads `sounding`). The listener is not on whole units, so read a
  distance before predicting its law. A sea sample opens quietly: match its 1.5 s from 3 s in
  (`t[3 * 22050:]`), which scores 1.00 where its first second and a half scored 0.20 under a gull
  (`q302/confirm.py`, Q302).
- **A ride worn out by itself:** enter the stock park and leave it: the Belly Bounce (thing 13) logs `Object 13:
  wear 1.515 with 1 on` every 64 sweeps (16 s), is told it is worn after 53 of them, asks to break down after 66
  (17.5 minutes) and is mended and open two minutes on. `openthing 13` shows its State of repair's bar, `objects`
  prints the number (`q303/confirm.py`, Q303).
- **The crowd's voice alone in the mix:** `hover 50 24` holds the pointer's cell on the Belly Bounce's queue
  whatever the camera does, so with `bus music 0` and `camera 1100 1100 110 180` the mix is the crowd's flat
  voice and nothing else. `voices` names each sample as `<name> Effects flat`; match each at its six pitches
  (the template read at a rate of `2 ^ ((p + 1) / 96)`, 1 at pitch 0): a hit scores 0.97 to 1.000 and its gain
  is the voice's volume times the master's 0.5. `crowd` reads a volume of nought between two samples
  (`q268/confirm.py`, Q268).
- **The orbit camera's eye is not `zoom` from the point it looks at where the ground is high:** the eye is
  lifted by the ground's height under the point and the point stays at height nought. Over the waterfall
  (ground about 12) `camera 530 513 30 180` puts the eye 38.5 off, not 30. Read `placed`'s listener, do not
  work it out (Q264).
- **A harness run needs a python with `Xlib` and `PIL`:** `~/.cache/tpw-harnesses/review-codex/venv/bin/python3`
  where the system's has no `Xlib`. A run that dies before `done()` leaves its Xvfb on `:78`: stop it (Q257w).
- **The camera aims at the ground, so it cannot close in on a Hot Pot's water**, 30 units up: `camera 435 275 45 180`
  and anything nearer shows the pot's wall. Take `camera 435 320 110 180` and crop the pot from the frame (Q257s).
- **`pkill -f 'Xvfb :78'` kills the shell that runs it** (its own command line matches): write
  `pkill -f 'bin/[X]vfb :78'` (Q257x).
- **Take the census last.** `step 2` after a `camera` moves every car and guest two ticks: a census held against a
  file must be taken after the last step before `savepark` (Q257r).
- **A load runs on before `pause` lands**, a dozen ticks: hold a loaded park against the load's own log lines, not
  against a census (Q257r).
- **A run that needs changed options** uses a private game folder, links to the real one and a `save/` of its own
  (`parkrun.py` makes one; the review of 2026-10-06, fix 3).
- **In a test, a boat is launched empty and then filled:** `Launch`, `Board`, `Fill`, as the ride's script does
  (`BUMP 4`, `1`, `12`); `Board` before `Launch` leaves the car not driven (Q257r).

- **`parkrun.gaps( pattern )` needs its pattern** (`gaps( "SOUND_PLACE" )`): called bare it raises, after the
  run's readings are taken.
- **A sound's place is printed as (x, z, height)**, the height last, by the audio log and by `voices`: the Belly
  Bounce's scream reads (525,250,0).
- **`bumpers` prints a line a car**, `car N at (x,z) ...`, each behind `[dbg]`: match `\bcar \d+ at \(`, never from
  the line's start. `parkscene.bumper_line()` is the ride's line only.
- **Read a category's effects one effect a call** (`sfxmap.py <data dir> <map> <effect>`): a shell loop's output
  runs two effects together.

**In the original, off-screen at 1024 x 768** (points for `gmove.py X Y`; `gmove.py align` first, with `GW=1022
GH=766` there, or the pointer is off):

- **It enters a park on the newest file in the player's folder**, so a file copied in before the start is loaded at
  the first frame, and again by the list's click (Q257n). Its track tick is not reset by a load (Q257p).
- **The player screen:** the slot (660,58), clicked twice: the first click is ignored.
- **The game menu:** Escape with no tool in hand opens it **and pauses the game**; never sleep under it. Load
  (510,50) then the first row (400,123), the newest file; Save (510,140), type the name, Return; Options
  (510,400), the quality track (512,276), OK (898,688).
- **Buying:** Buy (117,588), a ride's row such as (560,305), the features tab (843,123). A blue footprint is a
  legal place and a red one is not. After the placing click the queue tool is in the hand: a click on the path
  joins the queue and puts the tool away, and a later click on the queue's cell picks it up again (Q257o).
- **Hiring:** Buy (117,588), the side tab (957,316), mechanics (665,123).
- **A clip's phase is held by its start stamp**, a reading of the saved clock that steps by the clip's length each
  loop (a boat's: 534 ms): compare a stamp with a file's less whole loops, never a frame number (Q257t).
- **Breaking a ride:** `scriptvar.py PID <thing> 4 1` writes `VAR_BREAKSTAT` (variable 4 of every ride's
  common twelve). Write it inside a go: only a car carrying riders smokes, and the go ends at once. Its
  mechanic mends the ride about 25 s later, so save (the menu pauses the game) before then; the mended file is
  evidence too. A script's variables are the dwords at `[frame + 0x1c]`, not `+0x20` (Q257x). As a `POLL` of
  `loadfile.sh` on `q257o/orig/hotpot-later2.TPWS`, `sleep 3.5; scriptvar.py %PID% 43 4 1` lands inside its go,
  and the load's own frame at 15 s shows the pot broken, on the saved view (Q259).
- **A broken ride is mended fast:** loaded from a file, a broken Hot Pot had its mechanic assigned two ticks on and
  was open again 32 ticks (8 s) after the load. Poll from before the click (`loadfile.sh`'s `POLL`); the poll's first
  line is the park before the load, its second the file's (Q257y).
- **Its object chain is not in id order:** the shipped park's runs 15, 24, 23 down to 16, then 14 to 11, and a
  thing bought goes on the front. Read an order from the file's `mNext` links, never from the ids (Q257z).
- **Instant Action offers only researched items:** patch a copy with `research.py`, never the reference
  `easymode.TPWI` (Q257n).
- **Its music off:** the game menu's Options (510,400), the Music switch (445,533), OK (898,688), Escape; the
  same clicks put it back, and `save/Config.tcf` then holds the same bytes. A park's mix still reads 0.04 rms
  with it off and every placed voice silenced, so one placed sound's loudness is not read from the mix: neither
  a match of its sample (0.04) nor its loop's length in the autocovariance found a waterfall 21 units off (Q263).
- **How loud its mixer plays a voice** is read, not captured: `mixer.py PID` after a load prints each channel's
  distance and gain (Q263's camera files `q263/files/aim-*.TPWS` step the listener away from the waterfall). A
  channel marked `dirty` has not been worked out since it last moved: read the ones that are not (Q264).
- **Its mixer over a ride's screams:** a file saved under `parkrun.original_view( 515, 235 )` puts its listener
  about 50 units from the Belly Bounce, and `POLL='python3 .../mixer.py %PID% 150'` reads the scream channels (a
  mapping of 2.0, 95.0, 0.80 at (525, 250, 0)) and the staff's as they come and go (Q265).
- **Its crowd's voice in the mixer:** start `mixer.py` and `q236/orig/crowd.py` together after a load and move
  the pointer on and off the Belly Bounce's queue (`gmove.py 512 384` mid-frame on Q265's `bounce.TPWS`, then
  `930 200`): the crowd's is the 2D channel at the listener's own place, there while the handle is held and
  gone while it is nought. Neither log is stamped, so hold them together by the order of their stretches
  (`q268/orig/run.sh`, Q268).
- **Its screams' pitches:** `mixfreq.py PID 150` after a load of Q265's `bounce.TPWS` reads the Belly Bounce's
  scream channels (far end 95, 35 of 127, at (525,250,0)), 180 lines in 150 s (`q297/orig/run.sh`, Q297).
- **Its guests put off, and their pitches:** `breakq.py PID 330 13 all` beside `mixfreq.py PID 330` on Q265's
  `bounce.TPWS`: the put-off's channels are the 3D ones with a far end of 100 at 10 of 127, 39 lines in nine
  breaks. Broken on a timer instead, the Bounce's queue is empty at most breaks, and without `all` only a guest
  whose id divides by eight sounds: two lines in 270 s (`q298/orig/run2.sh`, Q298).
- **Its Hot Pot's engine in the mixer:** `mixfreq.py PID 150` as the `POLL` of a load of
  `q257o/orig/hotpot-later2.TPWS` (a pot in its go): the engine is the 3D channel at a far end of 100 that moves
  inside the pot (x 426 to 456, z 244 to 252), 51 of 127, some 2,000 lines as its frequency follows the boat's
  speed; a fixed channel at (435, 255) is another sound (`q299/orig/run.sh`, Q299).
- **Its thunder, without a storm:** `thingpoke.py PID 150 15 7:0x38:B=1` beside `mixfreq.py PID 180` on Q265's
  `bounce.TPWS`: a bolt every five seconds or so, each thunder a 3D channel at a far end of 1500 and a height of
  300, 30 in 150 s. The game writes the flag back to nought every 163 ticks, as its weather changes, so it must be
  held, not written once (`q300/orig/run.sh`, Q300).
- **Its weather, chosen:** hold the weather thing's FORECAST, not its quality: `thingpoke.py PID 50 15 7:0x28:i=1`
  makes its next change the heaviest rain (and lightning), `=20` and `=35` lighter rain with none, `=100` dry. A
  quality written straight to `+0xc` changes nothing, since only a delivered forecast is turned into drops
  (`FUN_005127a0`). A change came inside 35 s of each hold, the computer on seven days; `rainmix.py` beside it
  reads the drops, both rain voices and their channels (`q301/orig/run.sh`, Q301).
- **Its gulls and its sea:** a file saved here under `camera 400 60 110 180` puts its listener at (417, 47, 18),
  inside both rectangles; `camera 400 100`, `400 140` and `400 200` step it to 15, 75 and 136 from the gulls'
  (`q302/files/sea-*.TPWS`). `voices.py` shows effects 2 and 3 (flags word `0x0006`, voice flag `0x1000`) with
  their rectangles, and `mixfreq.py` their channels at the voices' places: 37 of 127 the gulls, 25 or 41 the
  sea, a far end of 100. A second effect 2, flagged `0x0606` at (0,0,0), is the music (`q302/orig/run.sh`, Q302).
- **Its Belly Bounce worn out by itself:** `object.py PID 840 13 5 4 7 8 9` as the `POLL` of a load of Q265's
  `bounce.TPWS`, left alone: a fall of the State of repair every 64 ticks, `VAR_WORN` under 20, the break some
  4,500 ticks after the load (13 minutes at seven days' uptime) and the mend 126 ticks later.
  `q303/steps.py <log>` prints a line a fall with the riders that give it (`q303/orig/run.sh`, Q303).
- **Its advisor told of a worn ride:** `advisor.py PID 1100 0x42 13` as the `POLL` of a load of Q265's
  `bounce.TPWS`, left alone: message `0x42`'s record goes from `(0, -1, 0, 0)` to `(tick, 0, 1, 0)` on the fall
  of the State of repair that leaves its whole number under 10, about 13 minutes in; grab `:77` a second later
  for him in his hard hat (`q305/orig/run.sh`, Q305). Here the stock park left alone does the same in 15 to 22
  minutes: grep the log for `Park advisor: post message=66` (`q305/confirm.py`).
- **Its advisor told a mechanic is on his way, inside a minute:** as the `POLL` of a load of Q265's
  `bounce.TPWS`, `advisor.py %PID% 90 0x46,0x42 13 & object.py %PID% 90 13 4 7 & sleep 20; scriptvar.py %PID% 13
  4 1; wait`: one log, both pollers' lines stamped with the tick. Seven ticks after the write the Bounce's
  `staff` reads 26 and message `0x46`'s record `(tick, 0, 1, 0)` (`q306/orig/run.sh`, Q306). Here
  `scriptvar 13 VAR_BREAKSTAT 1` does the same in two seconds: grep the
  log for `Park advisor: post message=70` (`q306/confirm.py`).
- **Its advisor told no mechanic can come:** *none in the park*: fire the mechanic HERE (`fire 26`), `savepark`,
  and load that file in the original (it reads a park with no mechanic as its own); the same `POLL` with
  `0x44,0x45,0x46` has `0x44`'s record said on the tick the Bounce's state reads 1. *Every mechanic busy*: the
  original's own `q257o/orig/hotpot-later2.TPWS`, `scriptvar.py %PID% 43 4 1` 3.5 s in and `scriptvar.py %PID%
  13 4 1` six seconds on, `thingwords.py %PID% 80 26:0x19c:I:state 26:0x218:H:job` beside the pollers: a slot
  holds `0x45` with the Bounce as the first line is still said (`q307/orig/run.sh none|busy`, Q307). Here
  `parkscene.broken_pot()` then `scriptvar 13 VAR_BREAKSTAT 1` once the log reads `mechanic 26 found ride`
  (`q307/confirm.py`).
- **The advisor in a hat, inside a minute:** the same break (`scriptvar 13 VAR_BREAKSTAT 1` 25 s into the stock
  park) has him say response 204 in the hard hat two seconds on; grep the log for `Advisor: sample 20` and read
  `wearing=` in `advisor`. His head is in the frame's lower right: `q308/confirm.py` counts the hat's pixels in
  the box from 0.77 to 0.93 across and 0.55 to 0.78 down, where R > 200, G > 180, B < 170 and R - B > 60 read
  6307 with the hat and nought without (Q308).
- **The theme's own advisor in his costume, inside a minute:** load `q345/nofeat30.TPWS` (the stock park with no
  feature, saved on tick 1584): row 81's line, a slot 1 response with the prop -1, is said on tick 1593, nine
  ticks in, in both games; grep the log for `on model slot 1` and read `wearing=-1,0 slot=1` in `advisor`. Break
  the Bounce once `advisor` reads `shown=False` for a slot 0 line straight after it (`q346/confirm.py
  fix|control`; the original's frames of the file are `q345/orig/a-g*.png`). `q346/confirm.py` counts the
  helmet's pixels in the box from 0.70 to 0.95 across and 0.62 to 0.85 down, where R > 150, G > 110, B < 170 and
  R - B > 50 read 5862 with the helmet and nought with the lobby's fly bare-headed; do not look for `slot=` alone
  in `advisor`, whose park half prints the advisor's eight slots (Q346).
- **A park file with a tag in it, and what a load does with it:** break the Bounce as above, wait for the
  line, `savepark`: `parkfile.py -t <file>` prints the block's records, and five of the original's own files hold
  one to set beside it (`q257x/orig/broken-by-the-original.TPWS`, id 72 about a Hot Pot; `q253/orig/
  sold-by-the-original.TPWS`; Alexah's two jungle saves). **Start the original on a file with NO tag, then
  `loadfile.sh` the tagged one with `POLL` `tags.py`:** it then prints none, one, none. Started on the tagged
  file itself, the park it enters already holds the tag, and the poll's lines do not change at the load (Q310).
- **A tag on the aerial, inside a minute:** the same break has the mechanic's line leave tag 72 on the gadget's
  mast for 30.5 s of real time; `tags` reads the list and the mast's, the top's and each tag's rectangle, and
  `q309/confirm.py`'s `topedge` finds the red top's upper edge in a frame (35 rows up at 720 down). In the
  original `tags.py` beside `advisor.py` reads the same rectangles. **`loadfile.sh` comes back some twenty
  seconds after its click**, so a frame taken as it returns is not a "before": use its own `-2-loaded.png`. And
  with no tag the red top rests over the lowest tag's place, so a colour count in that box says little (Q309).
- **The advisor cut off his line, in both games:** the same break and click post message `0x11c` (the message
  bar's own tutorial line, 13 s long), and the tag clicked again while he says it withdraws it. In the original
  `advisor.py %PID% 150 0x11c,0x46` reads the slot, then the record's said byte and its slaps, and the action's
  length going to nought (`q312/orig/run.sh b`); here the log's `Park advisor: post`, `attempt` and `withdraw`
  lines and `advisor`'s `said=/slaps=` (`q312/confirm.py`). The console's `tutorial on|off` is the Options
  switch in memory. **Take a log mark BEFORE the click whose line is wanted**, not after its reply (Q312).
- **A tag opened on the message bar:** in the original, the tag stands at (84, 909, 154, 974) of the 2048 x 1536
  layout, so at 1024 x 768 `gmove.py 59 471` and a click opens it; with the bar out its first button is at
  (283, 586) and its erase button at (283, 694), and the camcorder button at (130, 707). `bar.py` beside
  `tags.py` reads every step (`q311/orig/run.sh`: the break at 20 s, the tag four seconds after it comes). Here
  the console's `click X Y` takes window pixels: a layout point is `x * h / 1536` across and
  `h - (1536 - y) * h / 1536` down, the tag (56, 441) at 1280 x 720, the bar's buttons (265, 549) and
  (265, 650); `tags` prints the bar's line. **A `reply` waits its whole time before the next command**, so to
  read what a click did at once give the click 0.02 s (`q311/confirm.py`, Q311).
- **Its sound is not paced off-screen:** the null device takes the mix as fast as it is made, so a held voice's
  samples follow one another in a third of their clips' length. Read volumes, pitches and variations from its
  mixer, never a pace or a sample's length in time (Q269).
- **A mixer channel's frequency** is the word at `+0x1f4` of its record (22050 for the kids' samples at pitch 0);
  `q269/orig/probe.py` pickles a channel's whole record a poll, which is how a word of it is found (Q269).
- **A camera file's listener is not where the camera was aimed:** `camera 531 513 110 180` saved here reads a
  listener of (546, 50, 476) there. Read it (`voices.py`'s first line) rather than work it out (Q263).
- **A point's place on its screen** is its park place (x, the height, z) through the matrix at `[0x0087b088]`,
  then `x / w` and `y / w` as the screen's -1 to 1 (`spritepass.py` does it): how to find a thing in its frame, or
  to prove a drawing rule on it (Q258).
- **A highest or widest reading grows with the time looked.** Compare the two games over the same time, or
  predict a count, never a maximum (Q258: a bubble's height read 31 in ten seconds and 42 in forty).
- **For the first seconds after a load its camera is not on the saved view**, so a thing that lasts under five
  seconds from the load is in its memory and not in its picture: poll it, do not film it (Q257w).
- **Its camera stands further back than OpenTPW's for the same numbers.** A file saved under
  `parkrun.original_view( x, y )` shows the point mid-frame there; measured at zoom 110 and yaw 180 alone (Q257r).
- **`tpwmem.py read` and `watch` take every address as hex**, with or without `0x`: a decimal one is silently
  another address, so work a sum out first.
- **A ferry or seaplane made, and a file that holds it:** load `q340/late30.TPWS` (a leaver at the stop, no
  vehicle current): the leavers' summons comes on tick 1093 and draws the ferry, the seaplane or the bus, the
  bus one run in three, which makes nothing: run again. Save while the vehicle is current (before tick 1200);
  `parkfile.py -v <file>` prints the three slots, the object list, cell (0,0)'s chain and each vehicle's record,
  and the original's own to set beside it are in `q257x/orig/fixed-by-the-original.TPWS`. In the original,
  `worldwords.py` on the four vehicle words and `object.py %PID% SECS <thing> 0 1` (the vehicle script's
  status and trigger) say whether it kept the vehicle: `q342/orig/run.sh` (Q342).
- **`parkfile.models()` is keyed by a model's HANDLE**, the object record's `+54`, which is its slot plus one:
  taking one off reads the record before it (Q342).
- **A park whose guests need a toilet, with none standing:** `lib/stockscene.py ... 960 "sell 21" "sell 22" "sell
  23" "toilet 70"` (the stock park's toilets are things 21 to 23, its thirteen guests 29 and 31 to 42, all inside
  on tick 962; `toilet <n> <guest>` sets one). The advisor's row 82 is polled on tick 974 (819 and every 155 on)
  and says sample 225 where over a quarter of the guests inside are over 60. Only guests whose id divides by
  four (32, 36, 40) gain a point every sixteen ticks, in both games, and guests come and go, so the share
  moves by itself in a long run: read it with `original/inside.py` or
  this game's poll line. A runtime cell's record in the original is `[0x008023a0] + (y * 128 + x) * 68`, the
  packed id LESS ONE (Q347).
- **An advisor line that is Full Simulation's alone, said in the stock Instant Action park, in both games**
  (Q348): read the row's `+0x0c` in `meta_full.json` FIRST (nought: the poll scores it in game type 0 alone, and
  31 polled rows are so). This game: the console's `advisorgame full` given paused after the load, before the
  row's turn (`advisorgame own` lets go). The original: `bytehold.py %PID% 47 0x00fb3b7c=2 1112 0x00fb3b7c=0`,
  then `bytehold.py %PID% 4 0x00fb3b7c=2` to put it back, holds the game type at 0 from tick 1113 over the row's
  turn; never through the load, which reads the game type for its catalogue, so use the row's second turn after
  the load (155 ticks on, 41.5 s from the click). Every Full Simulation row polled inside the hold is scored too:
  name them to `advisor.py`. The stock park's eight features on offer all standing: `lib/stockscene.py ... 960
  "buy 1401 45 25 0" "buy 1425 46 25 0" "buy 1428 45 26 0"`; its researcher is thing 30 and its lab thing 6,
  whose projects `thingwords.py %PID% SECS 6:0x145c:I:projects 6:0xbc:f:done8` reads (project n's `mCost` is at
  `0x18 + 20 n`, its `mResearchDone` four on) and `parkfile.thing_set( src, dst, 6, 16 + 16 * n + 12, '<f', cost
  )` finishes in a file.

## The original under Proton

The original game (`testme.exe`, the 2.0 build Ghidra holds) runs on this machine through GE-Proton, as a
reference instrument (Q168, `docs/history/queue-done.md`). It is Linux only: Proton is Wine for Linux. It is not an emulator and it
does nothing on Windows or macOS.

### What it gives you

- **The original's frames** beside OpenTPW's: the same park and state, captured from the real screen.
- **Live memory at Ghidra's addresses.** `testme.exe` has image base `0x400000` and no relocations, so a Ghidra
  address is the address in the running process. `tpwmem.py read` and `tpwmem.py watch`; reading is safe.
- **Timings** (a tick rate, a walk, an animation), **only after `tpwmem.py clock` says TRUE.** See the first
  warning.
- **Traces of what the game asks Windows for**: `ORIG_WINEDEBUG=+file original.sh start` logs every file it opens,
  `+reg` every registry read, `+dsound` its sound calls. `+ddraw` and `+d3d` are heavy. Logs go to the harness's
  `logs/`.
- **The game's own logs**: `enginedebug.txt` and `fallback.txt` in the install folder, rewritten at every start
  (display mode, renderer, texture cache).
- **Files written by the original**: `save\Config.tcf`, players and parks in the install's `save/`. They are ground
  truth for the FileFormats docs.

### Running it

The harness, named by file here (`CLAUDE.local.md` says where it lives):

| Command | What it does |
|---|---|
| `original.sh start` | Starts the reference install on the real screen, silent (null ALSA), capped at 30 fps, with the Proton log and a frame-rate log |
| `original.sh start --offscreen` | The same on a private Xvfb `:77`, with a private KWin (own D-Bus and config folders) so the picture is scaled |
| `original.sh stop [--offscreen]` | `wineserver -k` for the reference prefix only; with the flag, also its private KWin and Xvfb |
| `original.sh pid` | The game's Linux pid |
| `loadfile.sh load FILE OUTDIR TAG` / `stop` | Loads a park file from the Load Park list, starting the original only if it is not running; frames, and a reader through `POLL=` |
| `tpwmem.py clock [s]` | Tick rate and park-clock speed against real time; exits 2 unless both are within 5% |
| `tpwmem.py read 0x00877d34 0x00785988:d` | Reads addresses (`I` u32 default, `i h H B f d`, `sN` raw bytes) |
| `tpwmem.py watch ADDR [s]` | Prints each change |
| `gmove.py align` / `gmove.py X Y [click]` | Lines the game's cursor up with the pointer, and glides in small steps |
| `record.sh start NAME` / `stop` | Films the off-screen display, lossless, 30 frames a second, with the wall clock at its start |
| `record.sh frames NAME FROM TO` / `sheet NAME FROM TO` | Every frame of the film between two of its seconds, each with its wall-clock time, or the same as one contact sheet |
| `watch.sh 'EXPR' SIZE [HITS] [SECS]` | Which code writes a piece of memory: a hardware watchpoint through `gdb`, attached and detached again |

**Film what lasts under a second.** A timed still misses it: a balloon's burst lasts a second there, and Q255's
three stills caught none of it. Start `record.sh` before the click and pull the frames after; a frame's wall-clock
time (good to 0.2 s) lines it up with a memory poll that prints its own. Tried 2026-10-09: 158 frames in 5.3 s.

**Ask who wrote it, do not infer it.** `watch.sh` prints the program counter after each write to an address and
the value written; the instruction before that counter is the writer, at the listing's own address (the game is
a 32-bit process, loaded at its image base). Tried 2026-10-09 on `mGameTick`
(`'*(unsigned*)0x7cf83c + 0x1da70c'`): five writes, 806 to 810, each from `0x0051639e`, after the
`MOV [EDI+0x1da70c],ECX` at `0x00516398` in `FUN_00516380`, and the game ran on after the detach. The game is
held while `gdb` attaches (about a second) and for a few milliseconds a hit, so a count of sweeps across a watch
is the park clock's, not the wall's. Four watchpoints at most, the processor's.

**Getting to a park:** "Welcome to Sim Theme Park" (click once if it waits) → intro films on a fresh `save/` (click
through) → player screen. **The first click on the player screen is ignored**, so click again. Then type a name,
click the tick, click the island. The Lost Kingdom park loads in about 30 s.

**Mouse:** the game reads relative motion (DirectInput). Run `gmove.py align` first, which sweeps corner to corner so
the game's cursor clamps onto the pointer. Glide rather than jump, and hold the button about 0.2 s.
**Coordinates:** the game's 4:3 picture fills the screen's height and is centred. On a screen S_w × S_h, with the game
at height H, a game point (x, y) is at screen ((S_w − S_h·4/3)/2 + x·S_h/H, y·S_h/H). Off-screen at 1024 × 768 it
fills the screen.

### Warnings, most important first

1. **The park clock is wrong once the computer has been on for days.** The park clock is a double of
   "milliseconds since boot", added to each frame at 24-bit precision (`docs/exe/park-engine.md`, "The park clock
   loses precision with uptime"). Measured at 6.5 days of uptime: uncapped, the park froze (0 ticks in 30 s); at 30 fps
   it ran 1.38 to 1.54 times real time; at about 4.5 fps it ran at real time. **Run `tpwmem.py clock` before any
   timing, and discard timings it calls NOT TRUE** (`VERIFYING.md` rule 127). The fix is a restart, and the ask is
   Alexah's. The 30 fps cap stays on.
2. **Never point it at `~/Games/TPWorld`.** The original writes `save/`, `Config.tcf`, `enginedebug.txt` and
   `fallback.txt` into its own folder. The reference install is its own copy.
3. **On the real screen it takes the whole display and the mouse.** Start it only when the other session is idle;
   the run needs no question first (Alexah's standing word). Your monitor's mode is not changed: Proton scales the
   game's mode.
4. **Off-screen is for memory and logic, not pictures.** llvmpipe drew Lost Kingdom's gate without its sign
   (`VERIFYING.md` rule 128). Without a window manager, the picture is a cropped corner; `original.sh` starts one.
5. **The first frame grab after a scene change can repeat the old scene.** Take two, a few seconds apart.
6. **One copy per prefix.** The game's `CreateMutexA` lock (`docs/exe/boot.md`) makes a second start quit silently.
   `original.sh stop` first.
7. **Sound:** silent by default. With no audio device at all, the game crashes right after its sound start-up
   (`docs/exe/audio.md`). An audible run needs Alexah's word.
8. **Registry writes** (`VERIFYING.md` rule 130): with the game closed; with `/reg:32` or `/reg:64` stated; checked
   in `system.reg` afterwards. The game needs the installer's `Bullfrog Productions Ltd\Theme Park World` values,
   which `tpw-setup.sh` writes, or it quits silently at boot.
9. **Kill by pid, never `pkill -f PATTERN`** when the pattern is also in your own command line
   (`VERIFYING.md` rule 129).
10. **Disassembly is Ghidra's job** (`CLAUDE.md` rule 7). If the Ghidra server is down, say so rather than reaching
    for `objdump`.
11. **A big park off screen draws at about a hundredth of a frame a second** under llvmpipe. With
    `GALLIUM_DRIVER=zink MESA_VK_WSI_DEBUG=sw` in front of `original.sh start --offscreen` it draws on the GPU through
    Vulkan at 30 frames a second, but its frame grabs read black: memory only.
12. **A player's saved park** loads from a copy of that player's folder in the reference install's own `save/users/`,
    under a slot digit no other folder there has (a folder's leading digit is its slot, and two alike show one).
    Entering the island loads it. An Instant Action player loading a Full Simulation park crashes the load (a page
    fault at `0x00464955`, where a rebuilt thing's model handle reads empty; the cause is inferred). Delete the copy
    afterwards.
13. **Alexah's Full Simulation jungle park stalls at park tick 43**, its main thread busy in the track rides' stepper
    `FUN_0043ce20` (return addresses on its stack, sampled with `gdb -p` on the host): its stored matrices are readable,
    its clock never runs. Seen at 6.9 days of uptime; whether a restart cures it is not tested.

### What it is not

- **Not a Windows or macOS route.** On Windows the game runs natively, with a no-CD exe and the same clock bug. On
  macOS, CrossOver (Wine) is the nearest thing. Neither has been tried.
- **Not the retail `TP.exe`.** SafeDisc 1.41's driver will not start under Proton (`docs/exe/boot.md`). Every run
  uses a no-CD exe; the reference install uses `testme.exe`.
- **Not a player's install.** Players use `tools/play-the-original/` (the guide and `tpw-setup.sh`). The reference
  install was made with that same script, with `--exe testme.exe --no-menu`.

## The executable's own routines under unicorn

A leaf routine that touches no Windows API, no thread and no object built at run time can be run unchanged under the
unicorn CPU emulator, on the game's real data, without launching the game. Its output is the original's own, so it is
a bit-exact test oracle for a port. The fork review (2026-09-30) did this for the movie player (`docs/exe/boot.md`,
"The movie player"): the dequant table `FUN_006710c0`, the TQI macroblock decoder `FUN_006747d0` on all 9,412 frames
of the nine movies, and the stereo ADPCM `FUN_00672210` on every audio chunk. Q195 re-ran the table and the audio: the
table equals the formula read in Ghidra, and the audio equals ffmpeg's `adpcm_ea` sample for sample in all nine.

**How.** Map `testme.exe`'s headers and sections at its image base (`pefile`), map a stack, a scratch area for the
arguments, and a sentinel address holding `HLT`. Push the arguments and the sentinel as the return address, then
`emu_start( function, sentinel )`. The scripts are `emu.py` (video), `audio_emu.py` and `full.py` in the review's
`tqi/` folder, with their own venv (`CLAUDE.local.md`); a three-frame run takes under a second.

**Caveats.**
- **Set the x87 control word.** Code that rounds through the FPU depends on its precision. The movie runs used
  `0x027F` (53-bit, the Windows default); 64-bit gives the same pixels, 24-bit changes 37.5% of one frame's samples.
  Which one the original runs under is a measurement in the original, not an assumption.
- **Read the entry convention from the disassembly.** Many leaves take registers, not the stack
  (`FUN_006747d0`: `ESI`, `EBP`, `EDI`), and write into their caller's stack frame; the harness plays the caller.
- **Tables built at run time are not in the image.** They read as zero, so run their builder first (the colour tables
  of `FUN_00670350`), or the result is silently wrong.
- **A routine that calls the C runtime** (`__ftol` here) works, because that code is in the image too. One that calls
  an import does not, unless the harness stubs it.
