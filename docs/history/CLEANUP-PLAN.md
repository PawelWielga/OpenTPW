# Cleanup plan: nine things a player sees

Written 2026-09-21 against `alexah/96-laying-and-lifting-path` (`1fd83ad`). Line numbers are from that tip.

**How to use this file.** One item per session. Alexah picks the item. Each item says what is seen, where it lives, what is already known, the most likely cause, and how to confirm. "Confirm" always means the running game: a screenshot **and** the census or log line that goes with it, both, with the number predicted before it is read. A unit test is added for regression only; it is never the proof.

**Order.** 9, 6, 4 first: the cause is already known and each is one short session. Then 1, 5, 3, 8, 2: each needs a decode step before a build step, so it is two sessions. 7 last: it needs Alexah to say what the original shows before anyone opens Ghidra.

**>>> WHAT ACTUALLY HAPPENED TO THAT ORDER. <<<** Alexah picked **5 ahead of 1**, then **3 ahead of 1**, and both took **one** session rather than the two budgeted — each time because `docs/exe/` already held the neighbouring decode, so the session's own decode work was a corpus census or a read of one function. **Item 1 then closed on 2026-09-22, also in one session** — and for the third time the decode came cheap because `docs/exe/` already held the neighbouring work. **Item 2 then closed the same day, 2026-09-22**, and for the fourth time the decode came cheap because
the neighbouring work was already in `docs/exe/` — the camcorder's own section named `FUN_0042b1c0`, and
`CellEdge` already modelled the very function that branch calls.

**>>> ITEM 8 WAS THEN MEASURED THE SAME DAY AND CHANGED NOTHING. <<<** All three of its claims are
refuted: the wander box is the original's own field for field, the aim turns **no faster** near an
island change than away from one (median 5.31 against 5.59 °/s, with a *lower* maximum), and **zero**
frames of 16,407 stepped more than five times the median per-frame step. Its own "Already known"
blamed `LookAtRate`, which belongs to the **orbit** branch and which `Attract` never touches. So the
item is answered but **not closed by me**: what is left is a deviation request under rule 11, and that
is Alexah's to ask for. The branch carries the instrument and the decode record only.

**>>> ALL NINE ARE NOW CLOSED, 2026-09-22. 9, 6, 4, 5, 3, 1, 2, 8, 7. <<<** Item 8 closed as
*measured faithful* plus the deviation Alexah then chose; item 7 closed by asking Alexah first, which
is what its own "First step, Alexah's" line demanded and which overturned the decode this file carried.

**>>> AND THE TWO CONFIRM CLAUSES THAT WERE ONLY PARTLY MET ARE NOW MET IN FULL, 2026-09-22. <<<**
Branch `alexah/108-photograph-the-two-confirms`. Both were closed on census evidence with the
photographs owed; Alexah asked for them to be finished, and they are. **No code changed** - both were
capture tasks that the existing `camcorder x y [yaw]`, `walk` and `at=`/`type=` census already allowed.

**Item 2 - all four stops photographed, each with its census beside it** (only one approach had ever
been shot). Every target cell was printed from the game first and all four read `type 4`:

| approach | yaw | stopped at | `type` | stand |
|---|---|---|---|---|
| west, walking east | −1.57 | **(50,24)** | 0 | 510 (509.999) |
| east, walking west | +1.57 | **(54,24)** | 0 | 540 (540.001) |
| north, walking south | 0.00 | **(53,22)** | 0 | 230 (229.999) |
| south, walking north | 3.14 | **(51,27)** | 0 | 270 (270.001) |

**4 of 4 ended where predicted, 0 of 4 inside the footprint**, `pitch=0.0` and the yaw exactly as set
in every reading, `save/` unchanged. The heading differs per approach because the step is
facing-relative — at those four yaws `walk 1 0` (forward) walks into the ride *and* looks at it, which
is rule 108. **Honestly: the ride dominates the frame from the west and east, and reads as a nearer
edge from the north and south**, because from those sides the camera looks along the box rather than
at its broad face. All four are genuine stop shots.

**Item 7 - the burst from the click to the loading screen, matched against Alexah's description.**

| t | state | radius | what the frame shows |
|---|---|---|---|
| before | `No` | — | the island small and distant across the ocean |
| 0.2–5.7 s | **Homing** | — | the view swinging round onto the gate side |
| 7.4 s | **FlyingIn** | 21.09 | **square-on to the gate, "Lost KINGDOM" legible** on the sign, both carved posts and the path between them |
| 8.7 s | **FlyingIn** | 8.43 | hard against the gateway, the sign filling the top of the frame, the park's path visible through the arch |
| 11.6 s | `No` | 7.99 | `Loaded jungle in 1745 steps` |

`save/` unchanged. **Two caveats, stated rather than glossed:** the apparent approach between the first
two frames is the ordinary orbit ease from the attract position, **not** the leave sequence — the
leave's own approach is the `FlyingIn` frames; and the front-end panels are up throughout because the
console's `enter` does not close the front end the way `IslandPanel.EnterPark` does. Both are harness
artefacts. **Worth Alexah's eye:** at radius 8 the camera ends up *inside* the gateway with a good deal
of stonework across the frame. That is what the original's own threshold of 8.0 produces, so it is
faithful — but if it reads as too close in play, that is a rule 11 call and theirs to make.

**Two of the nine turned out not to be the defect they were written as**, and that is the record worth
keeping: **item 8's three claims were all refuted by measurement** (the box is the original's own, the
aim does not swing, nothing jerks), and **item 7's premise was right while this file's decode was
wrong** — the original does move the camera, and the entry had stopped at the first of five steps.
**A third, item 1, had both of its guessed causes refuted** on the way to being fixed. So of nine
items, **three were mis-diagnosed in writing and only measurement or Alexah caught it.**

---

## 9. Loading is slow, and hangs after "queue cells stand in the park"

**>>> DONE, 2026-09-21. A PARK LOAD WENT 23,298 ms → 2,488 ms, ABOUT 9.4x. <<<**

Two sessions. `alexah/97-load-time` measured it and landed three prepared fixes (23,298 → 18,820 ms);
`alexah/98-decode-each-texture-once` found and removed what was actually costing the time
(18,930 → **2,488 ms**). **The "Done when" is met**: the worst phase is now `terrain` at **718 ms**.

| phase | first measured | after the 3 fixes | after the cache moved |
|---|---|---|---|
| **terrain** | 19,627 / 20,333 | 16,335 / 16,348 | **718 / 736 / 720** |
| objects | 1,428 / 1,547 | 1,147 / 1,150 | **656** |
| staff pool | 187 / 191 | 5 / 4 | 4 |
| **TOTAL** | **22,888 / 23,709** | **18,819 / 18,823** | **2,488 / 2,520 / 2,502** |

**And it is not a jungle-only win.** All four themes were loaded and timed, which had never been done:
**fantasy 1,016 ms** (terrain 379), **hallow 1,124 ms** (terrain 527), **space 1,229 ms** (terrain 656).

**What it actually was, after two wrong readings were measured away.** Timing *inside* `ParkTerrain`
split `base.MD2`'s 16,465 ms into **read 9 ms, textures 16,390 ms, buildmodels 60 ms** - so 99.5% of
it was the texture slot loop, and the `BuildModels`/material-construction theory died at 60 ms.
Counting then gave the cause outright: **1,047 texture loads of 57 distinct**, 18.4:1.

**`Texture`'s path constructor decoded the file and *then* asked the cache.** `CreateTexture` consults
the cache at the END of the constructor, by which point the .wct has been decompressed and put
through the whole wavelet decode - and the result is thrown away in favour of the copy already in
memory. The cache saved the GPU upload and none of the work. Asking first is behaviour-identical:
the set of hits is unchanged, a hit already returned before the sampler, size and path were assigned,
and `PreprocessTextureData` and `IsGraded` only touch the local pixel array. Predicted 16,383 × 57/1047
≈ **892 ms** before running it; measured **683 ms**.

Also removed: a `.ToList()` in `TryGetCachedTexture` that built a list of every texture in the game on
every lookup. It stays a scan rather than becoming a dictionary because `Delete` removes from
`Asset.All`, which is the list being scanned - the cache self-invalidates, and a dictionary would need
invalidating by hand for no measurable gain.

**`TextureDecodeTests` is new, and the mutation check is the interesting part.** The buffer resize in
`8c74ead` had no cover at all. This decodes every texture `base.MD2` names and checks each against its
own header. **The first mutation it was written for SURVIVED** - dropping the row buffer's extra
`size*size/2` changed nothing - and the reason is a finding rather than a hole: that headroom is
indexed only when a texture is alpha **and** half-scale, and **of the 656 `.wct` files across the four
themes' terrain and shared directories, 0 are half-scale** (211 carry alpha, every one full-scale). It
is **dead by CONTENT**, so no test against shipped data can pin it, and it stays because it is correct
for a texture the decoder supports and the game never ships. Two mutations on paths the art *does*
reach - either buffer at `size*size/2` - both fail the test, so it has real teeth.

**The INTERMEDIATE measurement, after the three prepared fixes and before the cache moved.** Kept for
the per-phase detail; the final numbers are in the table at the top of this item.

| phase | before (2 runs) | after the 3 fixes (2 runs) | saved |
|---|---|---|---|
| **terrain** | 19,627 / 20,333 | **16,335 / 16,348** | 3,638 |
| objects | 1,428 / 1,547 | 1,147 / 1,150 | 339 |
| fixed items | 369 / 357 | 214 / 213 | 150 |
| staff pool | 187 / 191 | **5 / 4** | 184 |
| queues | 266 / 270 | 227 / 225 | 42 |
| everything else (13 phases) | ≤ 221 ms each | ≤ 271 ms each | — |
| **TOTAL** | **22,888 / 23,709** | **18,819 / 18,823** | **4,479** |

Run-to-run spread on the untouched build was **821 ms**, so everything above ~1 s is signal.

**Three of this item's predictions were wrong, and the measurement is what corrected them:**
- *"ParkObjects took 9,400 ms"* was the item's illustrative example. **ParkObjects costs 1.4 s.**
  The stall is `ParkTerrain` - `base.MD2`, 272 meshes and its textures - at **86% of the load**.
- The BFMU/`ParkStaffPool` cost is real but **cannot matter here**: that phase ran **187 ms** before
  the fix and 4 ms after. It is a 40x win worth 0.18 s, not a load-time fix.
- **The three prepared fixes were NOT on a branch.** `claude/opentpw-code-review-e9vkqf` is absent
  from this clone and `9829691`/`564068d`/`a9add6d` are not valid objects here; `/home/alex/repos/game`
  is not a git repository. Alexah supplied them as a patch, applied with `git am`, so their original
  authorship and messages are preserved as `8c74ead`, `20ef69f`, `e543129`.

**>>> SUPERSEDED THE SAME DAY - this block described work that is now DONE. See the top of this item.
Kept because two of its three claims were WRONG and the record of that is worth more than tidiness. <<<**
It read: *"What is left, and it is the second half of this item."* What it got right was
`Texture.UpdateFromWct` decoding before `CreateTexture` consults the cache - that was the whole bug.
What it got wrong:
- It measured **≥2,000 decodes for ≥510 distinct** across a lobby-plus-park run, called that ~4:1, and
  concluded **"redundancy alone does not explain the 16 s"**. Wrong, because the figure mixed the
  lobby in. Measured per model, `base.MD2` alone is **1,047 loads of 57 distinct - 18.4:1**, and the
  redundancy explains essentially all of it.
- It concluded **"therefore the decode itself is simply slow"** and pointed at `DecodeChannel`'s
  delegates and `List<float>`. Also wrong: the decode is a reasonable **~16 ms per texture**. Nothing
  in `DecodeChannel` was touched, and the load still fell to a tenth.
- `TryGetCachedTexture`'s `.ToList()` was correctly called "milliseconds, not seconds". It was removed
  anyway, and it was indeed worth milliseconds.

**The lesson it teaches is the one it was written against:** it said *"time it before changing it"*,
and timing it is exactly what refuted it.

**Seen.** Entering Lost Kingdom stalls for a long time right after the log says `4 queue cells stand in the park`.

**Where.** `Level.SetupParkEntities` (`Level.cs:286-340`). The line after the queues message is `new ParkTerrain`, then `new ParkObjects` (eleven models and their textures), `ParkFixedItems`, `ParkRides`, `ParkGuestSprites` (the sprite atlas), `ParkStaffPool`, `ParkPeople`.

**Already known.** Three load-time costs were found on 2026-09-19 and are still in the tree:
- `TextureFile.ImageDecodeState.cs:12-13` allocates two lists of `size × size × size` floats for every texture decoded. For a 256-pixel texture that is about 134 MB. A park load decodes hundreds of textures.
- `Refpack.cs:32-38` reflects over every type in the assembly on every decompress call, which is every file read out of a `.wad`.
- `BFMUReader.GetCharacter` (`:95-104`) re-parses the whole lookup table for every character of every string. `ParkStaffPool` parses a string file on every name it asks for (`:195-208`, `:260-272`), twenty-five times at park load.

Fixes for all three exist as commits on the local branch `claude/opentpw-code-review-e9vkqf` (`9829691`, `564068d`, `a9add6d`), each built and tested alone. They can be cherry-picked onto the tip; they touch no park code.

**>>> CORRECTED 2026-09-21: that branch does not exist here.** It is absent from this clone, those
three SHAs are not valid objects, and `/home/alex/repos/game` is not a git repository. Alexah supplied
them as a **patch**, applied with `git am`, so their authorship and messages survive as `8c74ead`,
`20ef69f`, `e543129`. **All three are landed.** Do not go looking for the branch. <<<

**First step, before any fix: measure.** Wrap each `new Park*` line in `SetupParkEntities` with a stopwatch and log the milliseconds. That turns "it hangs after the queues message" into "ParkObjects took 9,400 ms". The log already carries a time per line, so a run's log gives a first reading with no code at all.

**Confirm.** Log the per-phase times before and after. Screenshot the loading bar at the moment it stalls, before and after. Say the time from `park jungle` to the first park frame, before and after.

**Done when** no single phase stalls the bar for more than a couple of seconds, and the total is written in `docs/STATUS.md` as a measured number.

**Both halves are met, 2026-09-21.** The total is in `docs/STATUS.md` as a measured number, and no
phase stalls the bar for more than a couple of seconds - the worst is `terrain` at **718 ms** of a
**2,488 ms** load. Confirmed on screen and by census in all four themes, with `save/` unchanged within
every run. **Item 9 is closed.**

---

## 6. Screams keep playing while the game is paused

**>>> DONE, 2026-09-21. THE MIX DROPS 34.96 dB WHEN THE MENU OPENS, AGAINST A 0.00 dB FLOOR. <<<**
Branch `alexah/99-pause-holds-sound`. Measured through the game's own mixer, before and after, with
the same harness both times (`~/.cache/tpw-harnesses/screampause.py`) and the game clock **proven
held** in each run (`ticks 2840->2840` before, `1975->1975` after, +0 in 1.2 s):

| | music only | floor (2 screaming windows, nothing done between) | A menu shut | B MENU OPEN | C shut again | A -> B |
|---|---|---|---|---|---|---|
| **before** | -38.3 | 0.04 dB | -24.1 | -24.1 | -23.9 | **-0.01 dB - no change** |
| **after** | -56.3 | **0.00 dB** | -25.2 | **-60.1** | -25.2 | **+34.96 dB** |

**The scream RESUMES rather than restarting**, which is the assertion that separates a hold from a
stop-and-replay: **0** new `screaming` lines after the menu opened, and **0** `stopped screaming`
lines in the whole run - the script is frozen one instruction short of `STOPSCREAM`, so the census
still reads `screaming True` either side of every window while the mix is silent. `save/` unchanged
within both runs.

**Seen.** Open the park menu; the kids keep screaming.

**Where.** Nothing in `Audio/` or `World/Park/ParkAudio.cs` reads `GameClock.Paused`. The only thing pause holds today is the advisor's voice (`ParkFrontEnd.cs:344`).

**Already known.** `GameClock.cs:39-42` records what the original does: pause sets `DAT_00803ad2`, which makes the listener update (`FUN_0051c1d0`) put its second coordinate at 10000. `Voice.Pause()` (`Voice.cs:159`) already exists with a 10 ms fade, built for the advisor.

**>>> CORRECTED 2026-09-21, MEASURED. THREE OF THIS ITEM'S PREMISES WERE WRONG. <<<**
- **"Every placed sound attenuates to nothing" is NOT ESTABLISHED, in either direction.** QMixer's
  distance model is not in `testme.exe`: `SetDistanceMapping` has exactly one call site (`0x006c581b`),
  gated on a request bit whose writer does not exist anywhere in the image, so QMixer's *default*
  mapping probably governs and it lives in `QMixer.dll`. How much a 10,000-unit lift attenuates is
  **undetermined**. Do not write "attenuates to nothing" anywhere; it is the claim three documents
  already assert without evidence.
- **"Music is not placed" is FALSE.** `FUN_0051e730` plays the park's music through
  `Sound_PlayEffect(..., 2, 0, 0, 0)` - *at the origin*. Nothing in the original is unplaced; a 2D
  sound is one played at `(0,0,0)`. The conclusion (music carries on) survives; **the stated reason
  does not**. What is safe to assert is structural: the original's mechanism acts **through the
  listener**, and a listener cannot reach a sound that was never placed against it.
- **The listener route is INERT in OpenTPW, twice over.** A park never sets `Audio.ReferenceDistance`
  (the only two writes are `LobbyAudio.cs:406` and `:265`), so `AudioListener.AttenuationTo` returns
  `1f` before it measures anything; and `Voice.Locate` returns immediately for a voice with no
  position. Porting the lift would silence **nothing** and would read exactly like a broken harness.

**Likely cause.** Nobody wired it. The mechanism is precise: a scream is a **looping** placed voice,
the ride VM is gated by `GameClock.TicksDue`, so a held park never reaches `STOPSCREAM` - and the
mixer runs on SDL's own thread, which knows nothing of any game clock.

**Build.** When `GameClock` pauses, hold every voice **that has a position** and release them on
resume. That is **screams and thunder** - and *only* those two. **Rain is NOT placed** (`ParkAudio.cs:332`
passes no position) and **there is no fountain voice at all**, so this line's original parenthetical
"(screams, rain, thunder, the fountain)" was wrong on two of its four. The rule itself is kept: a
voice with a position is held. Not through the listener - see above, it cannot work here - but
through `Voice.Pause`, which exists, fades over 10 ms so it cannot click, keeps `Playing` true so
`StopScream` still works, and is already used this exact way by the advisor.

**Confirm.** Start a ride, wait for the scream, open the menu. The `rides` census still shows the script screaming; the mix goes silent. Capture the mix with `SDL_AUDIODRIVER=disk` (see `docs/VERIFYING.md`) and show the level drop; screenshot the menu open over the park. Close the menu; the scream returns without restarting.

**>>> MET, AND THE CENSUS CLAUSE COULD NOT BE MET AS WRITTEN. <<<** `rides` printed no `screaming`
field at all, so that half of this clause was unsatisfiable until one was added - a pure getter, which
is the kind of observable `docs/VERIFYING.md` rule 89 asks for over a log line the subject can switch
off. **Nothing existing could open a PAUSING window either**: `pause` is `Time.Paused`, a different
thing, and `ParkObjectWindow` sets `Pauses = false`, so a `menu` command was added, routed through
`ParkFrontEnd.MenuKey` so it takes the real Escape road rather than a private copy.

**Two instrument faults of my own, both caught before they became findings:**
- The first baseline's floor read **digital silence**, which looks exactly like an audio device that
  never opened. It is not: a park's music is **replayed, not looped** - about 8.5 s of arrangement
  then a ten-second wait - so a park's mix is genuinely silent for over half of every cycle and a 3 s
  music-only window fell in a gap. Worse, with no floor my own verdict code called +0.70 dB a DROP.
  Both fixed, and the baseline retaken, before anything was built.
- `pkill -f "OpenTPW.dll"` **kills the shell running it**, because `-f` matches that shell's own
  command line. It killed the mutation check at its first line. Match on `pgrep -x dotnet` instead.

**Mutation-checked** (rule 48): inverting `Voice.IsPlaced` fails **3 of the 4** new tests, and the
fourth survives by construction because it only asserts the two answers differ. **Deleting the wiring
survives all four**, which is expected and is written down at the test: `Audio.HoldPlaced` and
`Audio.Play` both need an audio device, which a test run has none of, so the wiring rests on the
capture and not on the suite.

---

## 4. The lobby ocean renders wrongly when you come back from a park

**>>> DONE, 2026-09-21. THE SEA COMES BACK WITH THE SAMPLER IT ASKED FOR. <<<**
Branch `alexah/100-cache-hit-keeps-its-sampler`. One harness (`~/.cache/tpw-harnesses/waterlobby.py`)
run identically on both builds, the camera pinned to `cam=452,353,32` in **all four** shots, `save/`
unchanged within both runs:

| the sea, by the `water` census | first lobby | back from a park, BEFORE | back from a park, AFTER |
|---|---|---|---|
| `requested` | Wrap | Wrap | Wrap |
| `sampler` | AnisotropicWrap | **AnisotropicRepeat** | **AnisotropicWrap** |
| `size` | 128x128 | **0x0** | **128x128** |
| `path` | `…/jri_lak3.wct` | **empty** | `…/jri_lak3.wct` |
| `adopted` | False | True | True |

**What the player was actually seeing.** `AnisotropicRepeat` is `SamplerAddressMode.Mirror`, so every
other tile of the sea was flipped in both axes and each wave line met its own reflection at the seam:
the ocean came back as a **diamond lattice** instead of parallel ripples. The island, the jetty, the
Dino and the sky are alike either side - they ask for `Repeat`, which already *is* the default - and
that is the two-sided control.

**The scalar could not see it and the picture could not miss it.** Same crop, mean 55.69 -> 55.62 and
variance 221.83 -> 222.79, under half a percent, because mirroring changes *where* texels land and not
*which* they are. A verdict resting on that percentage would have said "no change" about a defect
covering half the screen. Written up as `docs/VERIFYING.md` rule 99.

**It cost nothing.** `assets total=1731 distinct=535 texture=542` is identical before and after, so
refusing a cache hit whose flags differ from the request forced no second decode: no path in the game
is asked for under two different sets of flags. Predicted before the run, then measured.

**Mutation-checked, including one expected to survive** (rule 48). `SamplerFor(Wrap)` returning the
default fails **exactly** the three tests named for it; **removing the fix itself passes all 839**,
because every road to a cached texture runs through `CreateTexture`, which builds a GPU texture that a
test run has no device for. That is said at the test rather than papered over - the wiring rests on the
capture, as it did for item 6's audio.

**One instrument fault of my own, and it cost a run.** The first pass photographed two quite different
viewpoints while the console answered `island 0 'Lost Kingdom'` both times. With nobody playing the
lobby camera is in **attract** mode - `LobbyCameraMode.Update` returns early into `Attract()`, whose
wander is reseeded on every lobby build because `ForgetIsland` clears `_wandering` - so `island`,
`orbit` and `settle` are read by a branch that never runs, and the reply echoes the selection rather
than the camera. `lobbyshot.py`'s recipe predates the attract camera and had gone stale the same way.
An `attract off` command was added to force the orbit branch, which is the only way two lobbies in one
run can be shot from one spot. `docs/VERIFYING.md` rule 98.

**The second half of this item's Build line is DECIDED, and deliberately NOT built.** It says "then
decide whether a scene should release its textures on unload". The decision is **no, not here**:
textures stay cached by path and shared between scenes, which is what makes the second lobby cost 414
steps instead of 839 and what item 9's whole win rests on. Releasing them is a separate change with its
own risk - `Delete` schedules the GPU handles for disposal, and an adopted texture holds handles it does
not own, so a scene release would have to learn that difference first. Item 4's defect is fixed without
it. Per this file's own bookkeeping rule, that is a second item and it is not carried in this session.

**And that line's citation is WRONG against the tree.** It says `ParkGround.Rebuild` documents the leak;
`ParkGround.Rebuild` (`:80`) documents nothing of the kind and the word does not appear in the file. The
live statements are `Model.Delete` (`Model.cs:48`) and `Material.Delete` (`Material.cs:135-137`), both
saying textures are "cached by path and shared" and left alone on purpose, with the same sentence
repeated in `Sky`, `LobbyIsland`, `ParkObjects` and `ParkFixedItems`. What `ModelEntity.OnDelete`
(`:48`) records is the **models and materials** accumulation, which scene release already fixed.

**A fact for the record, which does NOT change this.** Of all 104 calls through the device vtable's
`+0xa0` (`SetTextureStageState`), the only two that write `D3DTSS_ADDRESS` both write
**`D3DTADDRESS_CLAMP`**, and nothing writes `ADDRESSU` or `ADDRESSV` - the original never wraps
anything. It cannot be ported here: OpenTPW's sea is a 10,000-unit plane whose UVs the shader computes
from position, so clamping would stretch one texel over the whole ocean, where the original's sea is a
mesh with authored UVs. Written up in `docs/exe/render-states.md`; it becomes live when the sea is
re-sourced from the original's mesh (lobby plan item 9).

**Seen.** First load looks right. Exit To Lobby, and the ocean texture is wrong (stretched, blocky, or clamped at the edges).

**Where.** `Water.Spawn` (`Water.cs:35`) asks for `jri_lak3.wct` with `TextureFlags.Wrap`.

**Already known.** **>>> SUPERSEDED BY THE FIX ABOVE - read this block as HISTORY, not as
instruction. Its line numbers went stale a second time the moment branch 100 changed this very
function. It is kept because it was RIGHT: the fault it predicted is exactly the fault measured. <<<**
**>>> RE-CHECKED AGAINST THE TREE 2026-09-21. THE DEFECT STANDS; EVERY LINE NUMBER
AND THE HELPER'S NAME WERE STALE, BECAUSE BRANCH 98 REWROTE THIS EXACT FUNCTION. <<<** The file is
`source/OpenTPW/Render/Assets/Texture.cs` (342 lines), the helper is **`TryAdoptCached`** - there is no
`TryGetCachedTexture` any more - and **there are now TWO cache-hit returns, not one**:

| | Where | What it skips |
|---|---|---|
| **the early one, NEW in branch 98** | the path constructor, **`:85`**, before the file is touched | *everything* - `UpdateFromWct`/`UpdateFromStb` are never called, so `CreateTexture` never runs at all |
| the original one | inside `CreateTexture`, **`:293-294`** | `PreprocessTextureData` (296), `SamplerType` (**300-307**), `Path`/`Width`/`Height` (**329-333**) |

So a texture served from the cache still has the **default sampler** (`AnisotropicRepeat`, set at the
field's declaration on `:9`), not the wrap sampler, and no size - which is the fault this item is
about. **Branch 98 made it MORE reachable, not less:** the early return at `:85` is now the common
path for any repeated texture, and it skips the sampler assignment just as completely.
`Material.Delete` (`Material.cs:146`) still does not release its textures, so the first lobby's water
texture is still in `Asset.All` when the second lobby builds, and the second `Water` hits the cache.

*(Cited line numbers in this file go stale the moment anyone touches the function. Re-grep before
trusting any of them - this block was written against `1fd83ad` and was already wrong by `ce3de6c`.)*

**Likely cause.** That, exactly. The first load misses the cache and gets a wrap sampler. The return hits it and gets the default.

**Build.** A cache hit carries `SamplerType`, `Width`, `Height`, `Path` and the requested flags across, or refuses to serve a hit whose flags differ from the request. Then decide whether a scene should release its textures on unload; `ParkGround.Rebuild` documents the leak that comes from not doing so.

**Confirm.** Log at `Water.Spawn` whether the texture came from the cache and what its `SamplerType` is. Predict: first load `AnisotropicWrap`, return `Point` (or whatever the default is). Screenshot the ocean at the same camera position on first load and on return, before and after.

---

## 1. A ride plays its "being built" clip when a saved park loads

**>>> DONE, 2026-09-22. THE BELLY BOUNCE NO LONGER HATCHES: role 0 NEVER appears. <<<**

One session, on `alexah/104-built-not-building`. **Both of the causes this entry guessed at were
wrong, and the real one was measured.** `PoseAsBuilt` was already hiding the egg correctly; what put
it back was the script. `Bouncy.RSE`'s **second** instruction (body word 4; word 0 is `NAME`) is
`WAITANIM 0 0` — role 0 entry 0, the construction clip — and `WAITANIM` starts a clip as well as
waiting on it, so `Sweep` posed it every frame and `LobbyModel.Pose` re-applied its visibility tracks
over the built pose.

**And the original has no guard at all** — it restores each script's saved program counter
(`FUN_005597a0`) and every animation channel (`FUN_004647a0`), so a loaded park's scripts resume
mid-flight and never re-run that instruction. Built: a reader for the save's `RSSE` module, restoring
the counter, the variables and the declared name. `docs/exe/ride-operation.md` has the decode; the
byte layout went to the FileFormats clone's `saves.md`.

**THREE ADVERSARIAL REVIEWS, AND EVERY ONE FOUND SOMETHING REAL — none was caught by the suite or by
the game.** The third found the restore still discarding a field: dword 6 of a saved channel is its
playback **speed**, saved at **1.1** for the Belly Bounce and 1 for everything else, and it is read now.
Its worth is bounded and the bound was measured: a restored speed survives loop wraps and holds, but the
script's next trigger replaces it with 1, and Bouncy's script gets back to its `LOOPANIM`s about twenty
seconds in — so it fixes that window, not the session. The same review also showed the channel half had
**no test that could see its own flag translation** (reverting it left all 858 green), and that a script-reader
failure degraded in total silence. Both are fixed, and **both mutations now turn the suite red**. The first found the regression below. The second found that the saved channel flag word is the
engine's **internal** field and was being passed in as a **caller** flag: they collide on `0x4`, which
internally means "held on the last frame" and to a caller means "keep the pose", and `Start` clears
`0x6` on the way in — so the hold was dropped for **eleven of the fifteen** restored channels and the
clips restarted. It looked right only because starting every channel at time nought backdates it into
the held state anyway; correct by accident, resting on a second oddity. Now translated properly: `0x1`
and `0x8` carry across, and held/frozen are re-entered through roles 14 and 13 the way the engine does
it. The second review also found the channel half had **no test at all** — gutting it left all 857
green — and that any reader failure degrades silently back to the frozen park.

**IT TAKES BOTH HALVES, AND THE FIRST ATTEMPT AT THIS WAS A NET REGRESSION.** Restoring only the
script counter froze **ten of the fourteen** placed things for the whole session — a thing whose
steady-state loop holds no animation instruction never reaches the `LOOPANIM` in its prologue again.
An adversarial review caught it; the whole-park census measured it. `RSYS` (`FUN_004647a0`) is the
other half, and its walk only closes once the channel count comes from each **item** (Jungle Spray 3,
the rest 1) rather than being assumed uniform.

| `rides` census, whole park | before | script only | **both halves** |
|---|---|---|---|
| 13 Belly Bounce | role **0**, 150 frames | role 2 | **role 2 at once** |
| 24 Fountain | role 5 | **idle for ever** | **role 5** |
| 21–23 Toilets, 14 Jungle Spray ×3 | role 5 / role 2 HELD | **idle for ever** | **restored** |
| 12 Traffic Lights, 11 Gates, 16 Coconut, 17 Litter Bin | animating | **idle for ever** | **animating** |

Screenshots either side at 2.0s: an egg, against the built dinosaur with its fence and name board.
**The mutation was run and caught it** — disabling the one call site turned the suite red on
`Expected:<46>. Actual:<0>`, while the nine format tests stayed green, which is what says they cover
the reader and not the wiring. 858 tests, 0 skipped, 125 warnings.

**Seen.** On entering Lost Kingdom, the Belly Bounce hatches from its egg again instead of standing built.

**Where.** `ParkObjects.PoseAsBuilt` (`ParkObjects.cs:351-400`) and `ParkObjects.Sweep` (`:680-712`).

**Already known.** `docs/exe/park.md:253`: "Role 0 is played once, when the player builds the thing. A park loaded from a save does not replay it, so the last frame of the construction clip is what a built item looks like." Two things in the code do not match that:
- `PoseAsBuilt` applies only the construction clip's **visibility** tracks (which meshes are shown at the end). It does not apply its rotation, morph or position tracks, so a mesh the clip turns or moves is left at its rest pose.
- `Sweep` poses the model from whatever clip the script's animation player holds. If the script's opening `TRIGANIM` names animation 0 (the construction clip) on its first turn, the player starts it from frame 0 and the built pose is overwritten.

**Decode first.** Which of the two is it? The `rides` census at tick 0 says which clip each player holds. If it is role 0 entry 0, find how the original suppresses the replay on load: the object's state at load, or a guard in `TRIGANIM`, or the script itself reading a variable. Write the answer to `docs/exe/ride-operation.md`.

**Build.** Whichever guard the original has. And `PoseAsBuilt` should pose the whole end frame, not only visibility.

**Confirm.** Screenshot the Belly Bounce in the first park frame, before and after. The `rides` census at tick 0 and tick 100 beside each. Predict what clip the player holds at tick 0.

---

## 5. One scream, on a loop, at one volume

**>>> DONE, 2026-09-21. A HELD SCREAM WENT FROM 1 SAMPLE TO 8, AND BOTH MISSING OPCODES LANDED. <<<**
Branch `alexah/101-screams-vary-and-single-scream`. One harness
(`~/.cache/tpw-harnesses/screamvary.py`) run on two builds differing in **one line**, `save/` unchanged
within both runs:

| the Belly Bounce, by the new `rides` census | CONTROL (one clip looped) | AFTER (replayed) |
|---|---|---|
| effect 71, passes | **1** | **8** |
| effect 72, passes | **1** | **6** |
| distinct scream samples heard, whole park | 7 | **22** |
| `unimplemented` naming either opcode | no | no |

**`plays` is the statistic, not `distinct`.** The control still reaches 7 distinct samples, because the
ride starts and stops screaming about seven times in four minutes and each *start* picks a fresh clip -
so a verdict resting on "distinct went up" would have been satisfied by the broken build. What
separates them is passes **within one held scream**: 1 against 8. Predicted before the run.

**Both effects appear in one run, which confirms the band mechanism independently.** 71 and 72 are the
bands for one rider and for two-to-three, so the rider count crossed a band edge mid-run and
`Bouncy.RSE`'s `JSR ->193` tore the scream down and started another - the decode seen happening.

**The mix corroborates it, and the FIRST version of that measurement was a lie.** Against **fixed**
thresholds the control sounds 93.3% of the time above -60 dBFS with a 10th percentile of -55.1 dBFS -
never actually silent - where the replayed build sounds 52.4% with a 10th percentile at **-180 dBFS**,
true digital silence between screams. Peaks are alike (90th percentile -23.6 against -25.3); it is the
*floor* that differs, exactly as "looped" against "replayed with gaps" predicts. **The first pass used a
threshold taken from each run's own 35th percentile**, which put the two baselines 39 dB apart and made
the derived duty cycles meaningless - `docs/VERIFYING.md` rule **100**. These are two different runs with
different park state, so this corroborates the census rather than proving anything on its own.

**Mutation-checked, including one expected to survive** (rule 48): `0x6c -> 0x6b` in the negative branch
fails exactly `ANegativeLevelPicksAOneShotOnTheBandAlone`; the grid base `0x4f -> 0x4e` fails two tests,
the second because `0x4f` then becomes reachable as a first-column id. **Removing the replay itself
passes all 842**, which was said in advance and is the same shape as items 4 and 6: the pump needs an
audio device and a loaded category, which a test run has neither of, so the replay rests on the capture.

**What the decode changed, all of it written to `docs/exe/ride-operation.md`:**
- **Four corpus counts were wrong and are corrected.** Measured over all 306 wads / 308 scripts:
  STARTSCREAM **40**, STOPSCREAM **80**, SINGLESCREAM **46**, SCREAMLEVEL **81 uses in 36 scripts** -
  against the 7 / 7 / 9 / 6 that stood there. They were **jungle-only**, and short even for jungle,
  because **`Monkey.rse` has a lower-case extension** that a case-sensitive `.RSE` match drops.
- **The variety is the SCRIPT's, not the engine's**: `Bouncy.RSE` calls a subroutine every pass that
  re-issues `STOPSCREAM` + `STARTSCREAM` whenever the rider count changes band.
- **`Sound_PlayEffect` has no loop parameter at all** - `STARTSCREAM` and `SINGLESCREAM` make the
  identical call - so looping is decided below it, in `QMixer.dll`, which is not in the Ghidra project.
  The shipped data settles it instead: the four scream effects declare **4 variations over 25/50/60/59
  samples behind a 2700 ms repeat delay**, with samples averaging ~800 ms. A repeat delay is meaningless
  for a seamless loop, and a loop leaves 24 of effect 71's 25 samples unreachable.
- **A dead end recorded so nobody walks it twice**: the effect record's fifth int looked like a loop flag
  in `cat_kids` alone. Swept across all **1267** records in all **31** categories it is not - `music`
  effect 2 carries one and is replayed, and so do two `ui` button sounds.
- **The grid's first column is unreachable by arithmetic** - step is `(level + 50) / 50`, so step 0 needs
  a negative level, and a negative level takes the *other* branch. Pinned by a test.

**Two clauses of "Confirm" could not be met as written, and neither is a gap in the work.**
"Show the level moving" is `SCREAMLEVEL`, and **Bouncy never calls it** - no placed ride in Lost Kingdom
does, so the level cannot move in the shipped park. It is built because it is the third-most-used member
of the family corpus-wide. And "count distinct scream samples" from the mix would mean correlating
against 25 candidate clips; the game now reports the count itself, which rule 89 prefers anyway.

**Seen.** A rider boards, one scream sample starts, and it repeats unchanged until the ride ends. The original varies the sample, the timing and the volume.

**Where.** `ParkAudio.Scream` (`:340-385`) plays one effect looped. `SoundCategory.Play` (`:115-130`) picks a sample **once** and then loops that one clip.

**Already known.** `docs/exe/ride-operation.md` decodes the whole family under "## The SCREAM family" — **`:585` onwards**, with the opcode rows at `:593-598`. *(This cited `:534-545`; item 3's own +53 lines pushed it, and `:534` is now the heading "Where a WALKING peep is drawn".)*
- `STARTSCREAM` (opcode 86) holds one **looping** effect, 0x47 to 0x4a by band. Built.
- `SINGLESCREAM` (opcode 88) plays a **one-shot** from a 4×4 grid, ids 0x4b to 0x5a, chosen by band and by `(a+b)/50`. **Not built**: no case in `RideScript.cs`.
- `SCREAMLEVEL` (opcode 89) changes the held scream's volume. **Not built.**
- Each effect in `cat_kids` can hold several weighted samples (`SoundCategory.cs:19`).

**Decode first.** Two questions, both answerable from the data and the exe:
1. Do the placed scripts call `SINGLESCREAM` and `SCREAMLEVEL`? Count them across all 308 scripts with the `rse-listing` harness, and in `Bouncy.RSE` and the Jungle Spray's script in particular.
2. When the original loops effect 0x47, does it re-pick a sample each pass, or loop one clip? Trace what `FUN_0051bc40` and the looping play do with an effect that has more than one sample.

**Build.** `SINGLESCREAM` and `SCREAMLEVEL`, and whichever of the two loop behaviours the original has. Anything not reached by the placed scripts is counted, not guessed.

**Confirm.** Capture the mix for one full ride cycle, before and after; count distinct scream samples and show the level moving. The `unimplemented` census must no longer list either opcode. Screenshot the ride mid-cycle with the `rides` census beside it.

---

## 3. Guests move in jumps, not smoothly

**>>> DONE, 2026-09-21. A GUEST'S DRAWN POSITION WENT FROM 2 PLACES PER THING TICK TO 30. <<<**
Branch `alexah/103-guests-walk-smoothly`. One harness
(`~/.cache/tpw-harnesses/walkburst.py`) run on two builds differing in **one line**, the same guest
(thing 35) and the same 18 s window in both, `save/` unchanged within each:

| | CONTROL (`ThingTickFraction => 1f`) | AFTER |
|---|---|---|
| distinct drawn positions **within one 248 ms tick** | median **2** | median **30**, max 32 |
| `alpha` distinct values | **1** (constant 1.000) | **101** (0.000 → 1.000) |
| distinct drawn positions, whole run | 48 | 1498 |
| tick period from alpha wraps | — | **247 ms** (73 wraps in 18.0 s) |
| navigator `peeps at`, first twelve | identical | identical |

**The navigator matched wherever the two runs were compared, and that is the two-sided control.** The
interpolation lives entirely in the drawing, so the simulation had to come out unchanged — 111 samples,
48 against 47 distinct cells, the first twelve positions the same to three decimals. A build that had
smoothed the *simulation* instead, which is the wrong fix, would fail exactly there.

**The whole-run figure would have lied, as it did for item 5.** 48 against 1498 looks like the headline
and is not the claim: a guest really does move four times a second on either build, so distinctness
accumulates regardless. Only positions **within one tick** separate a slide from a jump.

**`alpha` is the cleaner discriminator than the position count.** 1 distinct value against 101, and its
wrap rate independently measures the period at **247 ms** — against the decode's `1/248.000007` read out
of `.rdata`. Two unrelated routes to the same number.

**What the decode changed, all of it in `docs/exe/ride-operation.md` under "Where a WALKING peep is
drawn":**
- **The answer is (b): the original interpolates, per FRAME.** `FUN_004f9f00` is `prev + (cur - prev) * t`
  per axis (`0x004f9f89` and `0x004f9fb6`), driven by `FUN_00518f90` at `0x0054fa85` — **past the 31 ms
  catch-up loop's back edge**, so once per frame and not once per tick.
- **`t` is time since the last thing sweep over 248 ms.** `0x0054fa5c` subtracts baseline
  `[0x00878a1c]` from now `[0x008786bc]` and multiplies by `[0x00700f94]` = **1/248.000007**; the
  baseline is re-stamped at `0x0054f683`, *inside* the every-8th gate. Clamped [0, 1].
- **X and Z only. Height is forced to nought** (`0x004f9ffd`) and **facing is NOT blended** — copied raw
  from `[ESI+0x1c]` at `0x004fa015` — so a peep glides and its octant snaps. Both are reproduced.
- **The stamp is unconditional**: `FUN_004fa870` is the **first call of every person kind's tick
  handler** (`0x00501658` guests, `0x00505495` staff), ahead of the guest handler's `(id & 3)` stagger.
  That placement is load-bearing, not incidental — see the caveat below. A teleport re-stamps
  (`0x004fa95d`), which is why both of OpenTPW's placement sites now do.
- **Three rates, one shared "now"**: 1/31 placed objects, 1/62 particles, **1/248 peeps**, each with its
  own baseline global.
- **A clean negative**: the every-2nd-tick sprite step moves nobody. All 698 instructions of the
  handler region `0x00476000`–`0x00476a40` plus four helpers touch instance `+0x88`/`+0x8c`/`+0x90`
  nowhere, so `SpriteTickEvery = 2` is right for animation and must never be made to move anyone.

**>>> TWO OF THIS ITEM'S OWN "Already known" CLAIMS WERE WRONG. <<<**
- **`mLastPosX`/`mLastPosY` are NOT the interpolation source**, and the item's "read and stored but used
  by nothing here" is wrong twice over. They are a *trailing sprite's* own last placement (live
  `+0x218`/`+0x21c`, only reader `FUN_004fe900` behind a gate), and in OpenTPW they are neither read nor
  stored — `grep` finds the names only inside two doc comments. The real pair is **`mPreviousX`/
  `mPreviousY` at `+0x190`/`+0x194`**.
- **The period is 248 ms, not 31 ms.** The item is right that "the original also runs the thing engine
  every eighth tick, so the smoothness must come from somewhere else" — and the somewhere else is a
  per-frame pass the item did not suspect.

**One caveat, recorded rather than smoothed over.** The stamp had to go in `ParkPeople.OnUpdate`'s peep
and staff loops, **not** inside `PeepWalk.Step()` — which is the obvious place and the wrong one. The
original stamps for every peep whatever state they are in; stamping only walkers leaves a guest who has
**stopped** holding two different positions for ever, and the drawing would swing them between the two
on every frame, about a quarter of a cell, for as long as they stood there. There is a unit test for it,
and **deleting the wiring still passes all 848** because nothing in the suite drives a stopped guest
through a running park. Said at the test, per rule 48.

**Mutation-checked, six of them, both survivals called in advance** (rule 48). M1 `StampPrevious` no-op
→ 1 failure; M2 blend drops alpha → 4; M3 heading blended → 1; **M4 no stamp in the peep loop →
SURVIVED all 848**; M5 `ThingTickFraction = 1f` → 1; **M6 the renderer passes `1f` → SURVIVED all 848**,
because the render path needs a graphics device a test run has none of — the same division items 4, 5
and 6 each recorded. Every restore md5-verified. **M2 caught 4 rather than the 2 predicted**, and the
reason is worth keeping: dropping `* alpha` makes the expression wholly `int`, so `x / FixedVector.One`
becomes **integer** division and truncates. The shipped code is float only because `* alpha` promotes it.

**Two instrument faults of my own, both caught before they became findings, both now rules.**
- **Rule 102**: `run_python`'s `memory.getBytes`, `api.getBytes` **and** `memory.getBlock` all reported
  **no block** at `0x00700f94`, while the `read_memory` tool read it at once and the file on disk agreed
  byte for byte. `list_segments` compounds it by printing PE section headers rather than Ghidra blocks.
  I was one step from recording "the delegated probes fabricated these constants"; they had not.
- **Rule 103**: the first harness segmented thing ticks by watching `alpha` **wrap** — which cannot work
  on a control whose alpha never moves. The whole run collapses to one segment and the broken build
  scores ~70 against the good build's 31. **The verdict would have inverted.** Segmenting on the 248 ms
  period off the timestamps depends on nothing the subject prints.

**The item's own "Confirm" clause could not be met as written**, for a reason the plan could not have
known: it asks for a paused eight-frame burst, and **`docs/VERIFYING.md` rule 91 forbids grabbing while
paused** — `pause` plus `step` hands the renderer a frame budget and it stops presenting once spent, so
a paused burst measures a per-frame effect as absent. The clock was left running throughout and the
measurement taken from the game's own getters at frame rate, which rule 89 prefers anyway.

**Seen.** Guests step rather than walk. The original eases them.

**Where.** *(Line numbers as they were when this was written; after the fix `ParkPeople.OnUpdate` is `:1107-1238` — the peep loop `:1132` with its stamp at `:1140`, the staff loop `:1171` with its stamp at `:1176`, and `ThingTickFraction` at `:1091` — while `ParkGuestSprites.Standing` is `:658` and no longer draws at the current position: it returns `prev + (cur - prev) * alpha`.)* `ParkPeople.OnUpdate` (`:1082-1110`) steps a guest's behaviour and walk every eighth tick (`ThingTickEvery = 8`, about four times a second). `ParkGuestSprites.Standing` (`:646`) draws the sprite at the walk's current position every frame, so the picture jumps four times a second.

**Already known.**
- The original also runs the thing engine every eighth tick (`0054f668`, `TEST byte ptr [0x00877d34],0x7`), so its navigator moves at the same rate. The smoothness must come from somewhere else.
- The guest record carries **`mLastPosX` at 430 and `mLastPosY` at 434** (`ParkWorld.cs:396`), read and stored but used by nothing here. A "last position" beside a current one is what a per-frame interpolation needs.
- Sprites step every second tick (`SpriteTickEvery = 2`, about sixteen times a second).
- `docs/exe/ride-operation.md:468`: `FUN_005580a0` interpolates between two node positions and calls `FUN_004f9e60` to place a sprite. That is for riders on a ride; the walking case may go through the same placer.

**Decode first.** How does the original draw a walking guest between two navigator positions? Trace `FUN_004f9e60` and its callers, and who writes `mLastPosX`. Write it to `docs/exe/ride-operation.md` beside the rider case.

**Build.** Whatever the original does. If it interpolates from last to current over the eight ticks, do that in `Standing`, driven by `GameClock`, so it pauses with the game.

**Confirm.** Screenshot burst of one guest over one thing tick (eight frames), before and after: before shows one jump, after shows eight small steps along the same line. The `peeps` census shows the same navigator positions in both runs.

---

## 8. The lobby's attract camera is too far away and moves in jerks

**>>> MEASURED 2026-09-22, AND ALL THREE OF THIS ITEM'S CLAIMS ARE REFUTED. NOTHING WAS CHANGED.
IT IS NOW ALEXAH'S CALL, BECAUSE WHAT IS LEFT IS A DEVIATION REQUEST AND NOT A DEFECT. <<<**

Branch `alexah/106-attract-camera-aim`, which carries the **instrument** and the decode record, and no
change to the camera at all.

**1. "Too far away" — the box is the original's own, field for field.** Every constant of `Attract`
was read out of `.rdata` and `FUN_005dfcd0` and every one already matched: centre **500/75/500**,
extents **400/50/400**, wander speed **1.0** a tick, arrival **100.0** squared, look cap **2.0**, look
threshold **50.0** squared, both direction eases **0.1**, the ramp **±0.05**. The item's own first
decode question — "is the extent a full width or a half width, that alone doubles or halves the
distance" — is answered by `_DAT_00702c5c` = **0.5**: the roll is `centre + rand*extent − extent/2`, so
it is the **full** width and `RandomPointInBox` was already right. Measured in flight over 60 s: inside
the box on all three axes (z 55–79 of 50–100), a median of **101.5 units** from the nearest island,
max 143.5. **Do not "fix" this box** — it would be a departure from the original, not a correction.

**2. "The view swings when it changes island" — it does not, and it turns SLOWER there if anything.**
Over 150 s with **8** island changes, differencing the aim over a fixed 100 ms window:

| turn rate of the view | near a change (±1.5 s) | away from any change |
|---|---|---|
| samples | 3,404 | 17,305 |
| median | **5.31 °/s** | **5.59 °/s** |
| p99 | 15.36 | 17.35 |
| **max** | **15.62** | **19.07** |

Ratio of medians **0.95x**. The fastest turning in the whole run happens *away* from a change, and the
whole-run maximum is **19 °/s** — a slow pan, not a swing.

**3. "Moves in jerks" — no frame steps out of line at all.** Over 120 s and 16,407 distinct frames, the
per-frame step of the aim is median **0.0688°**, p99 0.2589°, max 0.3258° — **max/median 4.7x**, with
**zero** frames stepping more than **5x** the median (and so none at 10x or 20x). The camera's own
per-frame movement is median 0.0694 units against a max of 0.0751 — **max/median 1.1x**.

**The item's "Already known" was wrong about the mechanism.** It says the target jumps "and `_lookAt`
then chases it at `LookAtRate = 2` per second". `_lookAt` and `LookAtRate` belong to the **orbit**
branch and `Attract` never touches either. The attract path keeps a look **point** with its own ramped
`_lookSpeed`, eased exactly as the original eases it — which is why there is no snap to find.

**A competing explanation was tested and excluded twice.** The camera and its look point share one box
and could in principle pass close, and a direction between two nearly coincident points is
ill-conditioned however smoothly each moves. Measured, `aimDist` never fell below **29.2** in one run
or **32.8** in the other, so it never happens.

**Three instrument faults of mine, all caught before they became findings, two now rules.**
- **Rule 109**: polling at ~144 Hz a value the game recomputes once a frame turned frame pacing into a
  rate. The first headline, "max 143.12 °/s", is exactly **1.0° in one 7 ms sample**. Worse, that
  harness stored only its own derived statistic, so when the derivation proved aliased the data could
  not be re-analysed and the run had to be repeated.
- **Rule 110**: the **start of the run counted as an island change**, so the window around t=0 landed
  in the treatment bucket and **20 of 20** fastest turns were inside the first 5 s. Discarding a
  warm-up **inverted the verdict**: near/away medians went from 13.48/10.18 (treatment looks worse) to
  6.66/8.99 (treatment looks better).
- And a discrepancy I could not explain was **recorded rather than resolved by assertion** until it
  was measured away: two runs disagreed about how often consecutive polls repeated (42.9% against 4%).
  The jitter run settled it — **0.0% repeats** at a 6.95 ms median frame gap, so the game really does
  render at ~144 Hz and the first figure was my own formatted output, values under 0.0005 printing as
  `0.000`.

**Built:** `LobbyCameraMode.AttractState()` and the console's **`aim`** — a pure getter, because
`attract` is a **setter** and must never be polled (rule 88) and `state` carries `cam=` but nothing at
all about where the camera is *looking*. Without it "the view swings" was not a measurable claim.

**ALEXAH WAS ASKED, AND CHOSE "FLY CLOSER TO THE ISLANDS" (2026-09-22).** So a deviation was taken
under `CLAUDE.md` rule 11, knowingly and marked at the site, with the original's own numbers kept
beside it. The "Decode first" and "Build" lines below are answered and superseded by the above.

**>>> AND THE OBVIOUS WAY TO FLY CLOSER MADE IT FARTHER. MEASURED, TWICE. <<<**

| the camera's distance to the nearest island | median | max | horizontal median |
|---|---|---|---|
| the original's box (300-700, height 50-100) | **101.5** | 143.5 | 69.3 |
| **attempt 1** - shrink the box to 360-640 | **104.3** | 147.1 | 82.6 |
| **attempt 2** - original box, height 30-60 | **74.0** | 128.2 | 64.1 |

Attempt 1 was predicted at 65 and measured at **104.3** - very slightly *farther* than the thing it was
meant to improve. The box really did shrink (the flight was measured inside it), so the change took
effect and the reasoning was simply wrong: **the islands stand at the CORNERS of the square**, so the
original's box already centres each island in its own quadrant, and pulling the camera toward (500,500)
moves it to the one point farthest from all four - 141 units from each. **A smaller box is not a closer
one.** That is `docs/VERIFYING.md` rule 60 happening: a correction is not automatically more accurate
than the thing it replaces.

The lever is the **height**, which the first measurement had already said if it had been read properly:
horizontal median 82.6 against a 3-D median of 104.3 leaves about 64 units of purely vertical distance.
Attempt 2 puts the horizontal box back exactly as the original has it and lowers only the band, 50-100
to **30-60**. Predicted 72-78 before running; **measured 74.0**, which is 27% closer than the original
and lands within a unit of `NominalDistance` (72.8) - the `sqrt(SPINRADIUS^2 + VERTICALOFFSET^2)` the
lobby was actually composed at. The islands' own camera targets sit 12.5 to 38 up, so this leaves
roughly 20 units of height over the thing being looked at.

**Only the box moved.** The aim mechanism, the speeds, the arrival radii and both eases are still the
original's, and the "ease the aim change" half of the Build line below is **not** built and should not
be: the aim was measured and it already eases - there is no swing to remove.

**Seen.** With nobody playing, the camera wanders far from the islands, and when it changes which island it looks at, the view swings.

**Where.** `LobbyCameraMode.Attract` (**`:343-392`** — this said `:318-360`), `RandomPointInBox` (**`:395-399`**, not `:376`).

**Already known.** The numbers were read from the lobby constructor `FUN_005dfcd0`: centre (500, 500, 75), extent (400, 400, 50), speed, arrival radius, look cap. `RandomPointInBox` uses `± extent / 2`, so the box is 300 to 700 on each axis. The islands sit at 400 to 600. The aim is "whichever island is nearest", so the target **jumps** when the nearest changes, and `_lookAt` then chases it at `LookAtRate = 2` per second. The look-speed ramp was **chosen, not read** (`docs/exe/lobby.md`, "one number was chosen rather than read").

**Decode first.** Two checks against `FUN_005e0470`: is the extent a full width or a half width (that alone doubles or halves the distance), and does the original ease the aim when the nearest island changes, or snap it? Also whether the original's wander target is inside the island square or the whole box.

**Build.** Match the original where the decode is clear. Where it is not, this is a rule 11 case: a smaller wander box around the island cluster and easing on the aim change through `Time.SmoothingFactor`, declared at the site as a deviation.

**Confirm.** Screenshot burst across an island change, before and after: before shows the swing, after shows the ease. Log the camera's distance to the nearest island every second for a minute; predict the range before reading it.

---

## 2. Camcorder mode walks through objects and walls

**>>> DONE, 2026-09-22. FOUR APPROACHES, 4 OF 4 INTO THE RIDE BEFORE, 0 OF 4 AFTER. <<<**

Branch `alexah/105-camcorder-stops-at-objects`. One harness
(`~/.cache/tpw-harnesses/camcorderwall.py`) run on two builds differing in **one line**, walking into
the Belly Bounce's footprint from all four sides, `save/` unchanged within both runs:

| approach, 40 frames of walking | CONTROL (step taken whole) | AFTER (swept) |
|---|---|---|
| from the west, walking east | **53,24 type 4** | 50,24 — where it started |
| from the east, walking west | **51,24 type 4** | 54,24 — where it started |
| from the north, walking south | **53,25 type 4** | 53,22 — where it started |
| from the south, walking north | **51,24 type 4** | 51,27 — where it started |
| ended INSIDE the footprint | **4 of 4** | **0 of 4** |
| ended where predicted | 0 of 4 | **4 of 4** |

**All four control endpoints were predicted before the run and all four matched.** The four target
cells are printed by the harness from the game's own `cell` command and every one reads **type 4**, so
an approach aimed at the wrong place cannot pass unnoticed.

**The decode is the whole of this item, and it is exactly what the Build line hoped for.** The
camcorder *interaction mode* (`FUN_0046cff0`) is only `{vptr, 0}` and holds no position at all — the
movement is in the camera update, `FUN_0042b1c0`, whose **first-person** branch does not integrate the
step the way the orbit branch above it does. It sweeps the step cell by cell and at each boundary calls
**`FUN_004d8750`** — the very function `CellEdge` already models — at **mode 2**, the strict one:

    0042c093   PUSH 2 / PUSH dir / PUSH EBP=y / PUSH EBX=x / CALL 004d8750   dir = 3 if velX<0 else 1
    0042c290   PUSH 2 / PUSH dir / PUSH EBP=y / PUSH EBX=x / CALL 004d8750   dir = 0 if velZ<0 else 2

Read as **disassembly**, because the decompiler drops the arguments — the same warning `CellEdge`'s own
remarks already carry about this function. Directions match `MapStep` exactly (0 = −y, 1 = +x, 2 = +y,
3 = −x). Constants: `_DAT_006fdd58` 0.1 (ten units to a cell), `0x0074c9c4` **9.999** and `0x0074c9d0`
**0.001** the boundary epsilons, `_DAT_006fde00/04` ±1e-4 the dead band. Full write-up in
`docs/exe/park-engine.md`.

**One guard is load-bearing and was missed on the first attempt.** Having advanced to a boundary the
original re-derives the cell and, *only where that cell has not changed*, nudges 0.001 along the
direction of travel. Without it a negative step lands exactly on `cell * 10`, whose floor is still the
cell being left, so the next pass measures nought distance to the same side and never arrives — the
sweep stalls at every boundary going north or west.

**Mutation-checked, three of them, the survival called in advance** (rule 48). **M1, the wiring —
`EdgeTest()` returning null — SURVIVED all 865**, which is expected and is said at the test: the seven
new tests call `Slide` directly with a park's own edge test, so they pin the arithmetic and cannot pin
the wiring, which rests on the capture. **M2** inverting the Y direction and **M3** removing the nudge
each turned the suite **red**, M3 through the anti-vacuity control rather than the main assertion.
Every restore md5-verified.

**Photographed, and the picture is corroboration rather than the proof.** Standing one cell west facing
the ride, then walking forward: `stand` 505 → **510** (509.999 to the printed precision), `cell=(51.0,
24.5)`, `at=(50,24)` — 26.7 units of intent, 5 units travelled, stopped on the footprint boundary. On
screen the dinosaur goes from standing clear ahead behind its fence to filling the left of the frame at
point-blank range.

**Two instrument faults of my own, both caught before they became findings, both now rules.**
**Rule 107**: the view is steered by where the pointer **is**, and its two axes differ — yaw
*accumulates*, so it drifts for as long as the clock runs, while pitch is *assigned*, so a console
cannot set it at all. The first four-sided run had "east" moving the viewer three cells **north**,
because the heading spun between approaches; the first screenshot was 41.3 degrees of empty sky. Fixed
with `pause` for the walk (which zeroes `Time.Delta` — this is not rule 91, which forbids *grabbing*
while held) and XTEST motion to park the pointer in the dead zone for the picture. **Rule 108**:
`walk <forward> <right>` is facing-relative, so aiming the camera silently re-aims the controls — at
yaw 0 `right` is +x, but facing the ride at yaw −π/2 it is −y, and the identical command walked the
viewer away along a different axis while printing a perfectly consistent census.

**Built:** `ParkCamcorderCameraMode.Slide` (pure, so the arithmetic is testable without a device),
`Step`/`DebugWalk`, the mode-2 edge test cached per park, `at=`/`type=` on the `camcorder` census, a
`walk` console command, and an optional heading on `camcorder x y [yaw]` — the last two because the
capture this item asks for could not be taken with what existed: `camcorder x y` **teleports**, and so
crosses no cell edge at all.

**One branch is decoded and deliberately not built**: after moving, the original calls `FUN_0042a340`,
which finds a thing of kind 3 on the cell and runs `FUN_004e15b0`. What that does to the viewer is not
traced, so it is counted rather than guessed.

**Seen.** On the ground, you can walk straight through a ride.

**Where.** `ParkCamcorderCameraMode` samples only the heightfield. It never asks whether the cell it is entering is occupied. **The movement step is `Walk()` at `:358-369`, and the `Stand` write at `:368` is where the occupancy test belongs** — this said `:379-412`, which is now `PeekGround` (`:379`), `GroundUnderStand` (`:386-407`) and `State` (`:410-419`), i.e. the ground easing rather than the step.

**Already known.** The original's camcorder handler is `FUN_00481a10` (`docs/exe/park-engine.md:552` for the row, `:576-583` for what it does — this cited `:547`, which is a different row of the same table, `staffloc`), and the mode's own header cites `FUN_0042b1c0`. The park already knows which cells are blocked: `CellEdge.Blocked`, and `ParkState.CellAt( x, y ).Occupant`, both used by the guests.

**Decode first.** Does the original's camcorder stop at an occupied cell, at a footprint, or at a mesh? Trace the movement step in `FUN_0042b1c0` / `FUN_00481a10` and write it to `docs/exe/park-engine.md`.

**Build.** The same test the original makes, reusing the guests' edge and occupancy checks rather than a new one.

**Confirm.** Walk into the Belly Bounce from four sides. Screenshot each stop, with the `camcorder` census showing the stand position not entering the footprint cells.

---

## 7. There is no "entering the park" animation in the lobby

**>>> DONE, 2026-09-22. ALEXAH WAS RIGHT AND THE DECODE IN THIS FILE WAS WRONG. <<<**

Branch `alexah/107-camera-flies-into-the-park`. Asked what the original shows, Alexah answered **"the
camera moves toward the gate"** — which contradicted the entry below, and the entry was the one at
fault. It had stopped at the **first of five steps**.

**The chain, and the one identity that unlocks it.** `IslandLobby_LeaveForPark` (`0x005e1e30`) sets
`*(param_1 + 0x14) = 1`, and the camera update takes `int *`, so **`+0x14` IS `param_1[5]`** — the lobby
camera's own **state machine**. Closing the panel therefore *starts* the beat:

| | |
|---|---|
| `+0x40` `IslandLobby_EnterPark` | checks `[5] == 0` and the key count, calls `+0x44` |
| `+0x44` `IslandLobby_LeaveForPark` | sets state **1**, key puff and sound, closes the panel |
| state 1 | turns the orbit onto `island[+0x14] + π` at `0.05 × delta`, arriving within half a step |
| state 2 | locks it and decays radius **0.07** and vertical **0.6** per delta — the camera flies in |
| radius < **8.0** | calls `+0x48` = `FUN_005e1e50`, which ends `MOV [EAX+0x14], 2` on the scene |
| `FUN_005d5cf0` | the state-3 teardown **returns that field** — the documented "choice 2 means play a park" |

**The derived vtable is at `0x00702ec0`**, which `docs/exe/lobby.md` had recorded as "not found"; it was
found by searching for `IslandLobby_EnterPark`'s pointer bytes rather than the base's update. Two
corroborations fell out of it: `FUN_005e1830` (derived `+0x08`) opens `CALL 0x005e0470`, which is what
proves the base camera update really runs in the island lobby; and `FUN_005d83f0` — called on the state
transitions, and the obvious candidate for a gate animation — is **not** one: it is `__thiscall` on the
island and plays the ISLE model's clip 0 or 1, the same pair the update's tail loop picks between.

**Measured in Lost Kingdom, and both phases match the decode to two decimals:**

| | predicted | measured |
|---|---|---|
| homing, 2.814 rad at 0.5 rad/s | **5.63 s** | **5.63 s** |
| flying in, radius 64.75 → 8.60 at 0.7/s | **2.88 s** | **2.88 s** |
| heading held during the fly-in | π = 3.1416 | **3.1420** |
| vertical offset | collapses ~10x faster | 10.12 → **0.00** |

then `Loaded jungle in 1745 steps`. `cam=` travels 417,343,27 → 401,380,14 over the run. `save/`
unchanged within it.

**Mutation-checked, four of them, both survivals called in advance** (rule 48). **M1, the wiring —
`LeaveForPark` doing nothing — SURVIVED all 873**, because the eight new tests reach only the extracted
arithmetic. **M4 — `RadiusDecay` 0.7 → 0.07, the per-second conversion forgotten — ALSO survived**, and
that names a real unpinned claim rather than an oversight: the tests pass their own rates instead of
reading the constants, because an instrument that reads the same state as its subject cannot falsify it
(rule 2). Both rest on the capture. M2 (always turning one way) failed 2 tests and M3 (arriving at a
whole step rather than half) failed 1.

**One guard is OURS and it prevents a hang, not a wrong picture.** `Update` returns early into the
attract flight whenever no player is selected, and the leave sequence lives in the orbit branch — so
without it a console-driven entry with nobody playing would wander for ever and **the park would never
load at all**. The original cannot reach that case, because its own EnterPark needs a player.

**The gate's swing stays a deviation**, but it now plays *over* a move the original really makes rather
than standing in for nothing — `LobbyGate`'s remark that "the original is blank at the one moment the
player is looking straight at the gate" was **wrong** and is corrected at the site.

**Two harness faults of mine, both caught, neither a new rule.** The load check matched the literal
strings "Loaded jungle"/"Loaded the park" while the game had written `Loaded hallow`, so it printed
**THE PARK NEVER LOADED** about a park that had loaded perfectly — rule 43, a failure mode that is a
substring miss. It also entered **Halloween World**, outside this project's Lost Kingdom scope, because
`attract off` keeps whichever island was selected. Both fixed and the run redone.

**Seen.** Alexah remembers the original showing something between clicking Enter and the loading screen. Here the panel closes and the load begins.

**Where.** `IslandPanel.EnterPark` (`:280-328`): spawns the key-puff particle (effect 98), opens the gate (`LobbyGate.Open`, a declared deviation), then asks for the park.

**Already known.** `docs/exe/lobby.md:264`: the original's `IslandLobby_LeaveForPark` (`0x005e1e30`) is three calls: set the leaving flag, `IslandPanel_KeyPuffAndEnterSound`, and a UI message that closes the panel. The state-3 teardown after it "only tears down". Effect 4's sound ships with no samples, so the cue is silent. **The decode and Alexah's memory disagree, so the first step is to find out which is right.**

**First step, Alexah's.** Describe what the original shows: does the camera move toward the gate, does the screen fade, does the key puff appear large, is there a sound? A video of the original lobby would settle it. Until then nobody opens Ghidra for this.

**Then decode.** With that description, find it: the state-3 teardown, the loading-screen entry, or something the panel's own tree does on message 6.

**Confirm.** Screenshot burst from the click to the loading screen, matched against the description.

---

## Bookkeeping for every item

- The branch is named for the item — for what it CHANGES, not for the item number. One item, one
  branch, one session. **Taken so far: `alexah/97-load-time` and `alexah/98-decode-each-texture-once`
  (item 9, two sessions), `alexah/99-pause-holds-sound` (item 6, one session),
  `alexah/100-cache-hit-keeps-its-sampler` (item 4, one session) and
  `alexah/101-screams-vary-and-single-scream` (item 5, one session) and
  `alexah/103-guests-walk-smoothly` (item 3, one session) and
  `alexah/104-built-not-building` (item 1, one session) and
  `alexah/105-camcorder-stops-at-objects` (item 2, one session) and
  `alexah/106-attract-camera-aim` (item 8, one session, **two commits**: the first changed the camera
  not at all and carries the instrument and the decode record, the second is the deviation Alexah then
  asked for) and `alexah/107-camera-flies-into-the-park` (item 7, one session). The next free number is
  **108** — 102 is taken by `alexah/102-main-is-the-tip`, which is not a cleanup item, 103 by item 3,
  104 by item 1, 105 by item 2, 106 by item 8 and 107 by item 7.**
  **Item 7 was the one item nobody could start without Alexah**, and their answer - "the camera moves
  toward the gate" - **contradicted the decode standing in `docs/exe/lobby.md`**, which said the
  original does nothing between Enter and the loading screen. Alexah was right and the decode was
  short: it had stopped at the first of five steps. Asking was worth more than any amount of reading.
  **Item 2 was budgeted at two sessions and took one, for the fourth time running and the same reason:**
  `docs/exe/park-engine.md` already carried the camcorder's own section naming `FUN_0042b1c0`, and
  `CellEdge` already modelled `FUN_004d8750` — the function that branch turns out to call. The session's
  own decode was reading two call sites as disassembly and six constants. **Check `docs/exe/` before
  opening a program**, which is now four for four.
  **>>> THE BRANCH MODEL CHANGED 2026-09-21: `main` has everything and IS the tip.** A task branches
  off `main` and is **fast-forward merged back into `main`** once it is confirmed in the game — there
  is no stacked chain any more. See `CLAUDE.md`'s Branches section. <<<
  **Item 5 was budgeted at two sessions here and took one.** The decode step is what the budget was
  for, and it came in cheap for a reason worth keeping: `docs/exe/ride-operation.md` already held the
  whole handler family, so the session's own decode work was a corpus census and a read of
  `Bouncy.RSE` itself — neither of which needs Ghidra. **Check `docs/exe/` before opening a program.**
  **Item 3 was budgeted at two sessions here and also took one, for the same reason** — the decode came
  cheap because `docs/exe/ride-operation.md` already held the rider case next door, so the session's own
  work was reading `FUN_004f9f00` and three constants. **Item 1 was budgeted at two sessions and took
  one as well**, for the third time and the same reason — `docs/exe/park.md` already held the role
  table and the restore chain, so the session's own decode was a walk of two save modules. **What it
  cost instead was verification**: the first build fixed the clip and froze ten of the fourteen things,
  and three adversarial reviews each found something real that the green suite and the screenshots both
  missed. **Item 8 is now the only untouched item left before 7** — but Alexah picks.
- Facts found go in `docs/exe/`. The commit message says why and what was measured. The code comment says what the code does now.
- Tick the item here in the commit that lands it, with the measured number that proves it.
- If an item turns out to be two, split it and stop. Do not carry the second half in the same session.
