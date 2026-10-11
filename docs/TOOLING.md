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
| `lib/parkrun.py` | The scaffold of every save-and-load confirm run: a private game folder, `enter()`, `save()`, `load()`, `reply()`, `gaps( pattern )`, `shot()`, `predict()` and `said()`, `done()`; `OFFSCREEN = True` for a run beside the desktop's; `AUDIO_FILE = "<path>"` writes the game's own mix to a file, nothing to the speakers; `original_view( x, y )` for a saved camera the original's frame will show the point in |
| `lib/mixmatch.py` | One sample measured in OpenTPW's own mix (`AUDIO_FILE`): `template( sdt, index )` decodes a bank's entry, `window( file, from, to )` cuts the capture between two sizes, `match()` is the best match and its least-squares gain, `steady()` the gain of a looped sample under other sounds, from 0.3 s pieces in 2 s steps |
| `lib/parkscene.py` | exec'd after `parkrun.py`: the one-line census readers (`var`, `thing_line`, `staff_line`, `bumper_line`, `field`) and the Hot Pot scenes (`hot_pot`, `in_a_go`, `broken_pot`), so a confirm script does not write them again |
| `lib/parkfile.py` | The one reader of a park file: `Park` (things, cells, sprites), `track()` (the track-rides module, cars and riders), `models()` (every model record: item, flags, node words, lookup records, channels), `head()` (a sprite), `emitters()`, `emitter_words()` and `emitter_aim()` (the particles module's live emitters by their chains, where one is not its template, and its own velocity at `+0x38`; `parkfile.py -e <file>` prints them). Add a module's reader here, not in an item's folder |
| `lib/sfxmap.py <data dir> [<cat_..SFX.map> [effect]]` | A sound category's effects and variation headers (flags, parameter, volume, pitch, wait, range), walked as the loader walks them; with the data folder alone, the spread of every variation's range over all 31 files. Importable (`Category`, `categories`) |
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
| `original/breakq.py PID SECS THING [all]` | The original's memory, **written**: breaks a ride (`VAR_BREAKSTAT`) each time it is open with a queue that has stood four seconds, so every break puts a queue out, a line a break. With `all` it first writes nought over the mask byte at `0x0050133f`, so every guest dismissed from a queue plays kids `0x80`, not one in eight. Stop the original afterwards |
| `original/thingpoke.py PID SECS KIND THING:OFFSET:FMT=VALUE ...` | The original's memory, **written**: holds words of things at given values, written again whenever they read otherwise, a line a write; refuses a thing whose type byte (`+2`) is not KIND. `150 15 7:0x38:B=1` holds the weather's `mLightning`, so bolts and thunder come on a clear day. `thingwords.py` reads the same words. Stop the original afterwards |
| `original/mixpairs.py [<mixer.py log>]` | A `mixer.py` log against the category files: every pair of a 3D channel's far end and volume of 127, with the variation volumes of that range that `round( v x 0.75 )` makes it from. Says which effects' channels a log holds, and that none plays at a number the files cannot make |
| `original/modelboxes.py PID [FIRST LAST]` | The original's memory: each thing's model record's cell rectangle (`+0xc0`, `+0xc4`), whether it has a `.hmp`, the box's middle (where its scream is sounded) and its base height. Read the objects' lines (the stock park's are things 13 to 24): a person's `+0x20` is no model handle, and its line is noise |
| `original/research.py <in> <out> <item>` | A copy of a jungle park file with one item marked researched, so Instant Action's buy list offers it |
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
  after the park is entered**: the advisor's opening line turns the effects down (a waterfall read 0.05 to 0.07
  where 0.176 was owed). Take byte marks either side of an 8 s sleep and use `mixmatch.steady`: a 1.5 s
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
