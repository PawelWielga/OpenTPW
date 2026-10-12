# Park advisor (original executable)

The park advisor is a THING like the weather, ticking on the same beat. It hears in two ways: the park **posts a message** to it, and **each tick it polls one row** of the metadata table below, calling that row's score function (`FUN_0059a550` calls `FUN_0059c680( 1 )` on the static at `0x00fb3540`, built by `FUN_0059bea0`, whose `+0x20` is `MinScoreForConsideration` (`DAT_00fb3560`); it steps its cursor at `+0` over the 351 rows to the next whose `+0x00` is nought, 156 of them, calls that row's score function and keeps a score above nought that the accept gate `FUN_0059abc0` would take; the tick raises it when `FUN_0059bf20` answers above `MinScoreForConsideration`, 25). Either way the message is accepted or rejected by a gate, and an accepted message occupies one of **eight priority slots**. Each tick the advisor considers the highest-scoring slot, speaking only if its score passes the threshold (Q90 below), then picks a line within that response and plays a sample. Two separate tables sit behind this — a static response table at `0x00768fb8` that maps a response id to a sample, lip file and gesture, and a runtime-filled metadata table at `0x0076e300` that holds the per-message score function, category and line count. The scores' coefficients live in `data/Advisor/Advisor.sam`, whose schema is compiled into the executable as 60-byte descriptors. This page is about the park advisor's message and scoring system; the on-screen advisor model and his interruption are in `ui.md`.

## The thing

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_0050b360` | — | THING dispatcher; advisor is type `0x0b`, weather is `0x0f` | Same dispatcher, same construction pattern as the weather thing (`weather.md`, "Weather is a thing (model 15)") |
| `DAT_00fb3b7c` | game type | Global the advisor's case is gated on (`!= 1`) | Read by `Game_StateMachine`, `IslandPanel_Refresh`, `GameMenu_BuildPark` |
| `FUN_0059b060` | `CAdvisor::ReceiveMessage` | Maps an incoming park message to a response and raises a topic | The error string inside names the method |
| `FUN_0059b590` | — | Builds the 0x18-byte message/topic record | Writes its argument to byte offset `0x0c` |
| `FUN_0059a940` | — | Raises a record into one of the eight slots | Walks `advisor + 0x28` at stride `0x18`, 8 iterations |
| `FUN_0059a550` | — | The tick: picks the highest-scoring slot and says it | Compiled member names `mLastActionStarted`, `mMessagePlaying` |
| `FUN_0059abc0` | — | The accept gate; can drop a raise outright | Called by `FUN_0059a940` before a slot is taken |
| `FUN_0059aa70` | — | Withdraw: clears every slot with a matching id | Calls `Advisor_StopSpeaking()` and bumps a counter if that id is speaking now |
| `FUN_0059c500` | — | Routes some responses to a separate sticky slot at `+0x16d0` | |
| `FUN_005194d0` | — | Fetches the advisor THING; indexes a THING table at `DAT_007cfb90`, stride 5 dwords | Asserts "Thing has been allocated in Thing..."; the decompiler hides the call because the result is passed in ECX by `__fastcall` |

Concrete message-to-response pairs seen in the dispatch: message `8` → response `0x102`; message `0x0e` sub 0/1/2 → `0x7d`/`0x7e`/`0x7f`; message `0x11` (research) categories 0-4 → `0x5d`, `0x5f`, `0x60`, `0x61`, `0x5e`.

## Q90 — park opening and closing (decode only, 2026-10-03)

**The shipped settings suppress both announcements.** The door posts advisor messages, but their score is
**20** and the tick requires **strictly more than 25**. Silence alone therefore does not prove a missing
feature. Q90b implements this message/gate path while preserving the decoded stock silence (below).
No game was launched for Q90's decode and no speech was
confirmed on screen. The following runtime expectations are predictions from executable instructions and data.

### Door event, message, response and sample are different numbers

`FUN_00519ef0( open, 0 )` posts a type-`0x13` record through `FUN_0040f630` (vtable `0x006fd810`,
type accessor `0x0040f650` returns `0x13`): event **3** for open
(`0x0051a031`), **4** for close (`0x0051a1c1`), outside each state-change guard. `FUN_0059b060` case `0x13`
passes that event to `FUN_0059ae20`. Cases 3/4 construct messages **`0x80`/`0x81`**, with line index **−1**,
forced **0**, score override **−1**, then call `FUN_0059a940` (`0x0059af04`..`0x0059af53`).
`FUN_0059b590` derives the score from the message's metadata, marks it occupied, and leaves score-overridden
false. Thus repeating the same door state still posts; it does not force speech or reset its history.

Fresh reconstruction of all **5,280** initializer instructions at `0x005a0a50`..`0x005a86fb` gives:

| Event | Advisor message / metadata address | Score function | First response | Lines / selection |
|---|---|---|---|---|
| 3, open | `0x80` / `0x0076ff00` | `0x0059f390`: `[ECX + 0x3e0]` | 308 (`0x134`) | 2 / mode 2 |
| 4, close | `0x81` / `0x0076ff38` | `0x005b96a0`: `[ECX + 0x3e4]` | 310 (`0x136`) | 2 / mode 2 |

Both rows are posted-only (`+0 = 1`), category **0**, valid for either game mode (`+0xc = 2`), with at most
**one pending instance of each message** (`+0x20 = 1`). Neither is the category-1 sticky help message.
The first-response writers are `0x005a385d` and `0x005a3939`; the pointer writers are `0x005a3710` and
`0x005a3883`. Mode 2 advances the message's own history and wraps at its two-line count in `FUN_0059a550`.
Only a successful playback-helper return advances that history; the open and close messages have separate histories.

| Response / table address | Sample and lip number | Existing transcript's meaning |
|---|---|---|
| 308 / `0x0076b638` | **342** | The park is now open; the transcript's first word is uncertain |
| 309 / `0x0076b658` | **343** | Attention please: the park is open for business |
| 310 / `0x0076b678` | **344** | The park is closed; nearly time to turn out the lights and lock up |
| 311 / `0x0076b698` | **345** | Closed for business; time to end the day |

These are meanings from the existing `global-speech-transcripts.tsv`, not freshly heard quotations (its
342 and 345 rows contain apparent transcription errors). All four response records have gesture **−1**, model/bank
word **0** (global speech bank), props **9/0**, the ticket hat. Their trailing values **150/151** are not the message
IDs or `.sam` ordinals. The advice to open a closed park (samples 1–3) is a different message.

### Why the stock door is silent, and when these lines can speak

The score functions above run with `ECX = 0x00fb3540`. The compiled schema, not the text file's order,
resolves the three relevant keys:

| Key | Descriptor / runtime address | Shipped value |
|---|---|---|
| `GeneralAdvisor.MinScoreForConsideration` | `0x0072e1d4` / `0x00fb3560` | **25** |
| `ParkNowOpen.Score` | `0x00734e94` / `0x00fb3920` | **20** |
| `ParkNowClosed.Score` | `0x00734f48` / `0x00fb3924` | **20** |

`FUN_0059bea0` builds the balance sub-object at receiver **+0xc**, then reads `data/advisor/advisor.sam`.
Its vtable `0x006fd958` returns the schema through `0x00415e90` and the storage base (`this + 8`) through
`0x005b0d60`. `FUN_00401030` starts at slot 1. The ten-entry `MessageGroups` array consumes **30 value
slots plus one extra dword** (`0x0040115b`, `0x0040119b`); omitting that extra dword misidentifies both
score fields. The schema walk reproduces the direct readers' `+0x3e0`/`+0x3e4` exactly.

The accept gate `FUN_0059abc0` can admit score-20 messages: **it does not test the score threshold**.
They occupy ordinary slots. In the tick, `0x0059a6d5` compares the best occupied slot's stored score with
`[0x00fb3560]`; `JLE 0x0059a8e8` skips speaking for **20 and also 25**. That tail only reads elapsed time
through `FUN_0041a990` and returns. **This tick retains the low-score slot; it does not discard it.**
A second copy of that message then fails the instance cap. A higher-priority message can replace it when
all eight slots are full; explicit withdrawal `FUN_0059aa70` can clear it. No opposite-message withdrawal
is present in the two door-event arms.

If a message was posted with a score **above 25**, the tick can select it after any current action's wait
and the one-shot suppression flag. The non-overridden playback helper `FUN_0059b620` then clears occupancy
and re-evaluates its score: its separate check at `0x0059b6c3` / `JL 0x0059b6c9` requires **at least 25**.
That weaker second check does not rescue a newly posted score-25 message from the tick's first check.
A successful helper return sets the wait to the response's duration plus **1,000 ms** and records the history.
That return is not proof of audible output: the helper does not test `Advisor_SayResponse`'s return value.
Higher-scoring pending messages take precedence; pressing the door does not interrupt them immediately.

Category 0's shipped `MinTimeSameMessage` is **120**, `SayOnlyOnce` **0**, `DiscardAfterSlaps` **0**.
After a successful helper return, and only when `lastSaidTick >> 2` is nonzero, the accept gate compares the same message's elapsed
`(mGameTick >> 2) - (lastSaidTick >> 2)` with 120; this is **480 world sweeps** when both stamps are aligned
(about **119 seconds** at eight 31-ms ticks per sweep), not a wall-clock timer. Opening and closing do not
share this cooldown. The general five-second and 120-second keys are not read, as Q194 established in "The content" below.

### Evidence and next-session confirmation

A private copy of the Ghidra project, hash-checked against the original and not altered;
program `/testme.exe`, image base `0x00400000`, executable SHA-256
`cf0ffd955077eca146d75ee46c45b8a0786fb757a8f7d204b1aed8ec5a1ee4cb`, matching the reference executable.
The `q90/` harness directory holds `initial-decode.txt`, `filler.txt`, `loader-schema-door.txt`,
`allocation-and-clear.txt`, `tick-constructor-withdraw.txt`, `tail-xrefs.txt`, and the reproducible
`check_tables.py` / `table-check.json`. Both metadata rows also match the earlier Q177 extraction.
No new shipped-file layout was discovered; these are executable mappings of already documented data.

**Q90b must predict zero spoken door announcements with the stock scores**, then confirm the real door
with a screenshot and message/score/threshold log. To exercise the audible branch, use a clearly labelled,
private configuration fixture raising only both door scores to **26** before posting, leaving the threshold at 25.
Read or explicitly reset the two histories before predicting the response/sample sequence: event 10 in
`FUN_0059ae20` sets every rotation index to −1, so each message then selects line 0, then 1, then 0.
A saved history may begin elsewhere; Q90b traces the fresh constructor below. Confirm both the actual door action and
advisor on screen with its matching log. Cover alternating lines, same-state posting, duplicate suppression,
the strict 25 boundary, per-message cooldown and queue priority in regression tests; restore the missing
route and threshold defects separately and require the new tests to fail. Never hard-code an unconditional
`Say(342)`/`Say(344)` or change the shipped scores to satisfy the old confirmation wording.

## The message record (0x18 bytes)

Confirmed two ways: `FUN_0059a940` reads the response/subject as `param_2[3]` (byte `0x0c`), and `FUN_0059b620` reads the same byte as `param_1[6]` in *short* units.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `+0x00` | — | u16 lead field, written by `FUN_0059c0a0`. **Not** the id the gate uses | The gate's slot scan compares `+0x0c`, not this |
| `+0x04` | — | int lead field | |
| `+0x08` | — | int line index; `-1` means "choose one" | |
| `+0x0c` | — | dword subject / response id — **this** is the id the gate keys its per-message counters on | `FUN_0059a940` passes `param_2[3]`; `FUN_0059b590` writes its argument here |
| `+0x10` | — | int score / priority; defaults to `FUN_0059bf20(subject)` when the caller passes `< 0` | |
| `+0x14` | — | byte, occupied (`= 1`) | |
| `+0x15` | — | byte, "forced" — skips part of the accept gate | |
| `+0x16` | — | byte, `1` if `+0x10` was given, else derived from the subject | Really "did the caller override the priority" |

**Eight slots, ranked.** A raise fills the first free slot; when all eight are taken it finds the lowest-priority occupant and overwrites it **only if the newcomer's priority (`message[4]`) is higher**. Slots live at `advisor + 0x14 + slot * 0x18`; the scan walks `advisor + 0x28` at stride `0x18` and copies 6 dwords (24 bytes). The tick then says the **highest**.

**The tick** (`FUN_0059a550`) returns early while `now < lastSpoke + wait`, consumes a one-shot suppress byte at `+0x16e8`, scans the eight and says the best. On success **wait = the line's length + 1000 ms**. The compiled member names confirm the fields: `mLastActionStarted` = `+0xd8`, `mLastActionDuration` = `+0xdc`, plus `mMessagePlaying`, `mHistory[i]`, `mSoundId`.

## Line selection

`FUN_0059c490` gives the per-response selection mode:

| Mode | What it is |
|---|---|
| `0` | Cycle **and stop at the last line** — the accept gate refuses the response once the stored index reaches count-1 |
| `1` | Advance and clamp |
| `2` | Advance |
| `3` | Random, `FUN_00516330() % count` |

The response is `FUN_0059c240( message ) + index` (`0x0059b798`..`0x0059b79f`), so a message's lines are a **contiguous run of response ids**; `Advisor_SayResponse` finds the response by the table's `+0x00` and plays its `+0x04`, the sample (`0x005990a7`..`0x005990cb`). Response `0x266` (614) means "say nothing" (`0x0059b7a1`).

## The accept gate

`FUN_0059a940` calls `FUN_0059abc0( advisor, id, forced )` before taking a slot. **The gate does not consult world state.** It rejects for exactly five reasons, none of them about staff, visitors, rides, research or money:

| # | Reason | Where |
|---|---|---|
| 1 | The options screen's Tutorial switch off (`DAT_0078d90d` nought, written from control `0x1d4c2` at `0x004a3704`) refuses a message whose row's category is 1, the tutorial messages | `FUN_0059c610` |
| 2 | A per-message cooldown: elapsed vs `MessageGroups[category].MinTimeSameMessage` from `Advisor.sam`, tested only when `FUN_0041a980` is non-zero | `FUN_0059c390` / `FUN_0041a980` / `FUN_0041a990` |
| 3 | When NOT forced: the category's `SayOnlyOnce` is set and the message's said byte `+0xe8` is; then the category's `DiscardAfterSlaps` is not nought and is at or under the message's `+0xec`, how often a withdraw has cut him off saying it (category 10, a row not found, is never refused for it) | `FUN_0059c410`, `FUN_0059c570` |
| 4 | A cap on how many copies of one id may sit in the eight slots | `FUN_0059c320` |
| 5 | A rotation check against counter `+0xe4`, and mode 0 running out of lines | `FUN_0059c490` / `FUN_0059c2b0` |

Every test reads a per-message property, its category's `MessageGroups` settings in `Advisor.sam`, the advisor's own counters and slots, or (test 1) a flag the options screen writes. **So a screen-open line needs only the advisor, its tables and a HUD — nothing from the simulation.**

### Test 2 is a per-message cooldown, not a level check

The two helpers are tiny, and both are `__fastcall` with the receiver hidden by the decompiler (the same mangling that hides `FUN_005194d0`):

    FUN_0041a980( p )  =  *p >> 2
    FUN_0041a990( p )  =  (*(DAT_0080239c + 0x1da70c) >> 2) - (*p >> 2)

The disassembly settles what `p` is:

    LEA EAX,[EDI + 0xe]     ; EDI = message id
    SHL EAX,0x4             ; (id + 14) * 16
    LEA ESI,[EAX + EBP*1]   ; EBP = advisor  ->  ESI = advisor + id*0x10 + 0xe0
    MOV ECX,ESI
    CALL 0x0041a980         ; non-zero if rec[0] >> 2 is set
    MOV ECX,0xfb3540
    CALL 0x0059c390         ; EBX = this message's threshold
    MOV ECX,ESI
    CALL 0x0041a990         ; EAX = (global[+0x1da70c] >> 2) - (rec[0] >> 2)
    CMP EAX,EBX
    JNC accept              ; elapsed >= threshold accepts; less rejects

`+0xe0` holds the time he last said this message, so `FUN_0041a980` non-zero means "he has said it before" and the cooldown only applies after a first airing. The `>> 2` on both sides counts in fours of thing sweeps, about a second each: the running counter `DAT_0080239c + 0x1da70c` is `mGameTick`, one count a sweep, and the tick stores it at `+0xe0` when he says the message (`FUN_0041a960`, `0x0059a847`; `park.md`, "Arrivals: who comes, on what, and how often"). The counter is in the same large global structure as `FUN_005194d0`'s `+0x1da71c`. `DAT_0080239c` is zero in the image and filled at runtime.

### The per-message record: `advisor + id * 0x10`

The disassembly computes `advisor + id*0x10 + 0xe0` outright (`LEA EAX,[EDI + 0xe]; SHL EAX,0x4; LEA ESI,[EAX + EBP*1]`), so the record starts at `+0xe0`.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `+0xe0` | — | When he last said it: `mGameTick` then, stored by `FUN_0041a960` (`0x0059a847`) | Receiver of `FUN_0041a980` / `FUN_0041a990` |
| `+0xe4` | — | Rotation counter (gate test 5) | `FUN_0059c2b0` |
| `+0xe8` | — | Said-once flag (gate test 3) | `FUN_0059c410` |
| `+0xec` | — | Times a withdraw has cut him off saying it (gate test 3, against `DiscardAfterSlaps`) | `FUN_0059c570`; bumped by `FUN_0059aa70` (`0x0059ab34`) |

## The two tables — do not confuse them

### `AdvisorResponseTable` at `0x00768fb8` — static and walkable

610 rows, stride `0x20`, terminated by **9999** (walked to its terminator). Ghidra already names the symbol. `Advisor_SayResponse` walks it linearly and matches the row whose first dword is the response id — the table is **not** sorted by id.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `0x00768fb8` | `AdvisorResponseTable` | 610 rows, stride `0x20`, `9999` terminator | Walked to the terminator; Ghidra symbol |
| `+0x00` | — | Response id | Matched linearly by `Advisor_SayResponse` |
| `+0x04` | — | **Sample id** — what `Sound_PlayEffect` is handed | |
| `+0x08` | — | Lip file number → `\Speech\lips\sp_%03d.lip` | Format string in the exe |
| `+0x0c` | — | Gesture: `-1` builds a random animation sequence (`FUN_00598b20`); any other value is a row of the gesture table `0x0076dc18` at value `- 0x10` (`FUN_00598bf0`) | `0x005990e3`, `0x00599383` |
| `+0x10` | — | Model slot in the low half, bank in the high half (`slot \| bank << 16`). Slot 1 is the park's own advisor model (137 rows). Bank 0 plays through the global speech category (`DAT_00803a34`), any other value through the park's (`DAT_00803a40`); five rows carry bank 1 (ids 1 and 399-402) | `0x005990dd`, `0x00599317` |
| `+0x14` | — | First prop: a costume id of his model, put on for the line ("What he wears for a line", below) | `0x005990fa`, `0x00598e18` |
| `+0x18` | — | Second prop, nought for none | `0x00599106`, `0x00598e2d` |
| `+0x1c` | — | Grouping values — **see the refuted section below**; no instruction reads it through this table's base | xrefs |

### Response metadata at `0x0076e300` — filled before `WinMain`

Stride `0x38`, validated by `row+0x04 == id`, with a linear-scan fallback bounded at `0x772fcc` (351 rows). Ids `0x15f` and `0x160` are excluded outright. **The table reads as zeros in the image; its filler is straight-line code at `0x005a0a50`** (`MOV` and `XOR` between four `PUSH`es and `POP`s, ending in the `RET` at `0x005a86fb`; in no Ghidra function), a C++ static initializer the C runtime runs before `WinMain` through the thunk `0x005a0a40` listed at `.data` `0x0072bda8` (Q177's analyser decode; the complete emulated table is `~/.cache/tpw-harnesses/q177/skeptic-analyser/meta_full.json`, made by `emu2.py` there). Row 91, for one: polled, id `0x5b`, category 0, mode 2, score function `0x0059e730`, one instance, first response 244, two lines, selection mode 2.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0076e300` | — | Metadata table, stride `0x38`, filled by the static initializer at `0x005a0a50` | Zeros in the image |
| `+0x00` | — | Nought: the row is polled, one of 155, a turn every 155 ticks; otherwise posted only | `FUN_0059c680` |
| `0x772fcc` | — | Upper bound of the linear-scan fallback (351 rows) | |
| `+0x04` | — | Id (used to validate a row) | |
| `+0x08` | — | Category | |
| `+0x0c` | — | Game-mode filter: `2` always, `0` game type 0, `1` game type 2 | |
| `+0x10` | — | **Function pointer** that computes the score | Called via `FUN_0059c0a0` and `FUN_0059bf20`, subject to the `+0x0c` filter |
| `+0x20` | — | Max instances | |
| `+0x24` | — | First **response** id: `FUN_0059c240` returns it, the line's index is added, and `Advisor_SayResponse` matches the sum against the response table's `+0x00`, whose `+0x04` is the sample (response 244 is sample 242) | `0x0059c29f`, `0x0059b79f` |
| `+0x28` | — | Line count | |
| `+0x2c` | — | Selection mode (see line selection above) | |

**The score is code; the coefficients are data.** Because `+0x10` is a called function pointer, the triggers are engine behaviour and cannot be driven purely from a file.

## The content: `data/Advisor/Advisor.sam`

19,863 bytes, ASCII, **382 keyed lines, 205 sections**, commented by the original developers ("was 30 - annoying"). It needs no new format work — `.sam` is already read everywhere, and subscripted keys were proven by the weather work.

    GeneralAdvisor.MinTimeAnyMessage         5
    GeneralAdvisor.MinTimeSameMessage        120
    GeneralAdvisor.MinScoreForConsideration  25     <- this IS DAT_00fb3560
    MessageGroups[0..9].MinTimeSameMessage / SayOnlyOnce / DiscardAfterSlaps

`MinScoreForConsideration` being a file key is why `DAT_00fb3560` has three readers and no writer. **The other two
`GeneralAdvisor` keys are loaded and never read** (Q194). The balance object is the sub-object at `0x00fb354c` (vtable
`0x006fd958`; built by `FUN_00401030`; `0x00415e70` is its destructor thunk), its member calls made with `ECX = 0x00fb3540`, and `FUN_00401030`'s rule puts
`MinTimeAnyMessage` at `0x00fb3558` and `MinTimeSameMessage` at `0x00fb355c`. No operand of the program falls in
`0x00fb3554`..`0x00fb355f` and no byte search finds either address, where the same search finds `0x00fb3560` at exactly
its three reads; none of the 15 member functions nor any of the 201 score functions (thiscall on `0x00fb3540`, every
row's adjustment `+0x14` 0; 433 this-relative accesses) touches `+0x18` or `+0x1c`. So the only pacing is the tick's
wait ("The tick", above) and each category's own `MinTimeSameMessage` in the gate; **there is no
global gap between lines.** `MessageGroups` is set for groups 0-5 and 9 only; 6-8 keep the loader's zeros. "Slaps" is the player slapping him. Field names across all messages are dominated by `Score` (160 uses) and `ScorePerPoint`, then per-message conditions: `ValueBetterThan`, `SatisfactionWorseThan`, `FilthierThan`, `ScorePerUnpatrolledPct`, `DontBotherIfPatrolledUp`, `MonthsIntoGameForConsideration`.

The golden-ticket keys are score coefficients, not lines: `GoldTicketNearToFirstOne`, `NearToXPeeps`,
`NearToXHappiness` and `NearToXProfit` (multipliers 0.65, 0.9, 0.85, 0.9) with `GeneralWonGoldenTicket`,
`GoldTicketExplanation`, `WonFirstGoldenTicket` and `GoldTicketCluesForRemainder`; no other shipped `.sam` has them.

### The schema is compiled into the exe

60-byte descriptors — kind at `+0`, name at `+4` in a 32-byte field, bounds at `+0x24`/`+0x28`, array count at `+0x34` (`MessageGroups` reads 10) — the same pattern the weather schema uses.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0072e120` | — | Table of 60-byte `.sam` schema descriptors, returned by the advisor balance object's slot 0 (`0x00415e90`); 205 sections, ordinals 0-204, the first, `GeneralAdvisor`, closing at `0x0072e210` | Walked: `GeneralAdvisor` is 0, `WaitingTimes` is 204 |
| `0x0073964c` | — | The `WaitingTimes` descriptor, the last section; the next record, kind `0xc`, ends the table | End of the walk |
| kind `0` | — | Section start, unnamed (204) | `park-engine.md`, "How a key finds its global", has every kind |
| kind `1` | — | Section end, carrying the section's name (204) | Same |
| kind `2` | — | Array start, unnamed (1, at `0x0072e24c`); `FUN_00401030` scans on to its kind `3` and logs "Missing TABLE_END" without one | Q194, walked |
| kind `3` | — | Array end, carrying the name (1: `MessageGroups`, `0x0072e33c`) | Same |
| kind `4` | — | Int (290): every `Score` and `ScorePerPoint`, and most conditions | `park-engine.md`, "How a key finds its global" |
| kind `5` | — | Int ≥ 0 (15): `MinTimeSameMessage`, `MinScoreForConsideration` and `DiscardAfterSlaps` among them | Same |
| kind `6` | — | Bounded int, bounds at `+0x24`/`+0x28` (50): `ThirstierThan`, `WorseThan` and `SayOnlyOnce` among them | Same |
| kind `7` | — | Float (9): `ShopsPerRideRatio`, `SideshowsPerRideRatio`, `AverageQueueLongerThan`, `Margin`, `WagesTimesHigherThanIncome`, `PeopleMultiplier` (twice), `HappinessMultiplier`, `ProfitMultiplier`; the file sets all nine (0.15, 0.2, 2.5, 1.2, 2, 0.65, 0.9, 0.85, 0.9) | Q194, walked: 774 records to the terminator |

205 section descriptors match the file's 205 distinct prefixes exactly — two independent sources agreeing.

**The file's order is NOT the exe's order.** `ClosePark` is ordinal 4 in the exe but sits at line 575 of the file, far from its neighbours at lines 47-57. Nothing is missing from either side. **Ordinals must come from the exe, never from file order.**

## Verified by transcript

Speech transcripts already exist — grep the transcripts TSV named in `CLAUDE.local.md` before transcribing anything. Each row below is also corroborated by the message's own `.sam` field names.

| Group | Samples | Transcript | Section |
|---|---|---|---|
| 3 | 1-3 | "your park is closed... open up" | `OpenPark` |
| 4 | 4-6 | "not much to do... close it" | `ClosePark` |
| 5 | 7-9 | "thirsty visitors... drink shops" | `VisitorsThirsty` |
| 6 | 10-12 | "getting hungry... food shops" | `VisitorsHungry` |
| 7 | 13-15 | "rides wearing out, no mechanics" | `StaffHireMechanics1` |
| 8 | 16-18 | "rides breaking down" | `StaffHireMechanics2` |
| 9 | 20-22 | "not enough mechanics" | `StaffHireMechanics3` |
| 10 | 23-25 | "getting messy, no janitors" | `StaffHireHandymen1` |

Lines come in **threes** (three takes). Sample 19 is skipped — consistent with roughly 80 silent stub entries.

## REFUTED — do not reuse these

### `+0x1c` is NOT the descriptor ordinal

The response table's `+0x1c` matches the section ordinal early and then **drifts in accumulating steps**. The section list was walked out of the exe (205 sections at `0x0072e210`), the response table was grouped by `+0x1c`, and the two were joined against the transcripts:

- The match holds far further than the first handful — all the way to **message group 24**, which really is `StaffGuardBuildCamera` ("if you build some security cameras your guards would be more efficient"). Two dozen exact matches.
- **The offset is not constant and it is not +5.** It accumulates: **+1** by group 31 (`StaffTrainHandymen`, section 30), **+3** by group 64, **+4** by 68, and only at group **70** does it reach **+5** (`RidesBreakdownNoMechanics`, section 65). It stays +5 through group 82 — sampling it in that band mistakes a waypoint for the rule.
- Groups **20-23 and 25-29 have no responses at all**, which is the shape of the gaps.

**There is no arithmetic that recovers `+0x1c`. Do not derive a sample from a section name by counting — in either direction.** This has been got wrong twice, in both directions. Take the sample from the response table and confirm the line by its transcript.

### A screen's line is a message id, resolved through the metadata table

`FUN_00486b00( id )` makes an advisor message with this id and posts it; `FUN_00486b40( id )` withdraws it. Both first call `FUN_005194d0` to fetch the advisor THING.

**That id is a *message* id, not a response id.** `FUN_00486b00` does `MOV EAX,[ESP+0x30]; LEA ECX,[ESP+0x14]; PUSH EAX; CALL 0x0059b590`, and `FUN_0059b590` writes that argument to `*(undefined4 *)(param_1 + 6)` on an `undefined2 *` — byte offset `0x0c`, the subject, the very field the accept gate keys its per-message counters on. It is **not** the response id that indexes `0x00768fb8`.

Checked against the data: the map screen (`FUN_005f0b40`) pushes `0x130` = 304, and response 304's sample is 338, "there's a litterbug running amok" — plainly not a map line. Message ids resolve through the metadata table at `0x0076e300`, filled by the static initializer at `0x005a0a50`: the row whose id is the message gives the first response at `+0x24`, the line's index is added, and the response table's `+0x04` is the sample. The map's `0x130` is response 564, sample 582 ("the map shows an overview of the park"); buy's `0xbc` is 407, sample 380; hire's `0xbd` is 408, sample 381; the pylon button's `0x125` is 553, sample 565. The 40 call sites' constants sit in `0xbc`-`0xd5` / `0x11c`-`0x135`, every one a posted-only row (`+0x00` 1) of one line (`+0x28` 1) (Q177's analyser skeptic, `~/.cache/tpw-harnesses/q177/skeptic-analyser/meta_full.json`; each sample checked against its transcript).

### The message-to-sample rule

Message → its metadata row's `+0x24` (the first response) + the line's index → the response table's `+0x04` (the sample); see both sections above.

**The golden-ticket messages** (`ride-operation.md`, "Golden tickets", posts them; Q194, from `meta_full.json` and the
transcripts): every row posted-only, model slot 1, bank 0. `0xd7`-`0xd9` (Local) have 6 lines each from responses 434,
446, 458; `0xdc`-`0xde` (Global) 4 from 440, 452, 464; `0xe1`-`0xe3` (Secret) 2 from 444, 456, 468; `0xea`-`0xed` one
each, 474-477. The lines rotate three groups of samples: "ticket only" rows samples 293-295 ("and the winner is... you've won a
golden ticket"), "ticket and key" sample 446 ("...and now you've got another golden key"), "ticket, key and park" samples 604 and 622
("...now you can open a new park"). The "moved" rows are samples 533-536: the rollercoaster, go-kart, water-ride and park-size
awards moved here.

## Every HUD screen posts a message

The ids seen so far: `0xbc` when buy opens, `0xbd` when hire opens, `0xc6` and `0x11f` in the staff/visitor locator, and `0x131`, which the camcorder/postcard sub-panel **withdraws**. So the advisor comments on the screen you just opened.

## Still unknown

| What | State |
|---|---|
| The loader that fills `0x0076e300` | **Found** and emulated: the static initializer at `0x005a0a50` (above); the 40 screen-posted ids are posted-only rows of one line each |
| The u16 at message `+0x00` | Unknown — it is written by `FUN_0059c0a0` but is not the gating id |
| `Welcome` (section 2) | Row 0 of the metadata table is posted only in the running game (the emulated table has it polled), its score function (`0x0059c8c0`, `MOV EAX,[ECX + 0xa0]`) is `Welcome.Score`, and the shipped park's file holds message 0 as said on tick 1; its poster is not read. Its line looks like a **stub**: group 2 holds four rows, all `sample=1, anim=16`, and sample 1 is an *OpenPark* line. The park welcome probably has no audio of its own (read from the response table alone; no code that posts `Welcome` was traced) |
| Responses 399-402 | All carry sample 1, an `OpenPark` line — the same stub shape |

## Q90b — the posted door path (2026-10-03)

`ParkState.SetParkClosed` posts on every request, including an unchanged state. `ParkAdvisorMessages`
reads the two scores, strict consideration threshold and category-0 cooldown/say-once setting from
`Advisor/Advisor.sam`. Its eight slots retain low scores, cap each door message at one pending copy,
replace only a strictly lower minimum when full, and select the first highest slot. Opening does not
withdraw a pending close, or vice versa. Each message rotates its own two responses only after the
helper succeeds; a busy/disabled speaker returning zero still consumes the message and records history.
The existing speaker owns the model, voice, gestures and pause. No arbitrary sample is queued at the door.

Fresh Ghidra follow-up in the same private, identity-checked Q90 project resolves two implementation details:

- `FUN_00599e70` clears eight occupied flags and all 351 histories: last-said tick 0, rotation −1,
  said flag 0, slap count 0. The implementation starts the door histories this way.
- `FUN_005989c0` fills the clip-length array through indexed writes starting at `0x00f796c8`, then zeros
  **clip 14 only** (`0x00f796fc`). `0x00f79700` holds **clip 15's duration**, despite being zero in the image.
  `FUN_00598b20` includes it in the gesture-chain sum, and `Advisor_SayResponse` adds it **again** at
  `0x005994ac`, plus 1,000 ms. The scheduler adds another 1,000 ms. The park speaker preserves both additions.
- `FUN_0059c610` tests metadata category 1, so the global class gate does not suppress the category-0 doors.

The per-message cooldown reads the incremented `ParkState.GameTick`, shifted by two. The response wait
reads one `GameClock.Now` instant for all catch-up sweeps in a frame. That clock inherits OpenTPW's
existing clamped-frame timing deviation; it is not the synthetic tick number multiplied by 31 ms.
A park file's records are read at a load ("The records in a park file", below). Other posted park
messages, the analyser and withdrawal/slap paths are outside Q90b; no claim of a complete park advisor.

Evidence is in `~/.cache/tpw-harnesses/q90b/`; fresh executable queries remain in `q90/q90b-*.txt`.
The private stock and score-26 game views use copied saves. Only two digits differ in the latter's
advisor configuration: both door scores 20 → 26. The shipped configuration and original saves remain
unchanged. Each confirmation presses the entry-price door with XTEST pointer input, after predicting
its result; the scene and speaker use the real game paths and `SDL_AUDIODRIVER=dummy`.

Predicted and observed in the running game (`runtime-stock-final/`, `runtime-score26-final/`):

| Fixture / real door press | Message / score / threshold | Response / sample | Result |
|---|---|---|---|
| Stock open, then close | 128 / 20 / 25; 129 / 20 / 25 | none | 0 attempts, 2 pending |
| Score-26 open | 128 / 26 / 25 | 308 / 342 | world tick 99, history 0 |
| Score-26 close | 129 / 26 / 25 | 310 / 344 | world tick 153, history 0 |
| Score-26 open after cooldown | 128 / 26 / 25 | 309 / 343 | world tick 711, history 1 |
| Score-26 close after cooldown | 129 / 26 / 25 | 311 / 345 | world tick 757, history 1 |

The six PNGs show the real door states; all four score-26 frames show the advisor. Each has its
message/score/threshold census and the four have `voicePlaying=True` plus `sp_342/344/343/345.mp2`
speech voices in `run.log`. No sound was sent to the desktop speakers. The stock control waits for the
unrelated screen-help line to finish before pressing the door. `runtime-results.json` independently
checks all four actual response durations against each logged gesture chain plus the shipped ending
clip's 666 ms plus 1,000 ms. Example: 8,166 → 9,832 ms, then the scheduler adds 1,000 ms.

Regression: 20 new cases cover stock data, actual `SetParkClosed` routing and unchanged states,
duplicate retention, strict and configurable thresholds, the helper's inclusive recheck, shifted
cooldown boundaries, independent rotating histories, zero speaker returns, priority/ties/eight-slot
replacement, waits and real catch-up ticking. All 14 restored defects fail (`mutations.json`,
`review-mutations.json`); restored full suite: **1,699 passed, 0 failed, 0 skipped** with game data.
Independent applied-code review found the extra end-clip duration and catch-up-clock errors; both
were corrected and their restored defects fail. `SimulationClockScope` isolates the new tick tests.

Not confirmed on screen: full-slot eviction, the exact cooldown boundary,
rotation back to line 0, and zero playback-return consumption. Those applicable to the new path are
regression-tested; the actual busy/disabled speaker guards are code-reviewed only. The first score-26 capture was partial because
its harness deadline was too short for the cooldown step; the completed final run replaces that proof.

## A ride about to break down (message `0x42`, Q305)

**Who posts it.** A ride's wear, `FUN_004df670`, on the turn its State of repair's byte falls below 10 with the
byte plus the wear not below it (`ride-operation.md`, "A ride's wear"). The call is `0x004df8be`:
`FUN_0059b590( 0x42, [thing + 0], [thing + 0xe], -1, 0, -1 )`, so the record's `+0x00` is the thing's id, its
`+0x04` the thing's item, no line is asked for, it is not forced and the score is the message's own; then
`FUN_005194d0` and `FUN_0059a940`, the same gate and slots as the door's.

**Its row** (`0x0076e300 + 66 × 0x38`, from the emulated table, `meta_full.json`):

| Field | Value |
|---|---|
| `+0x00` | 1: posted only, never polled |
| `+0x08` category, `+0x0c` game mode | 0, 2 (either) |
| `+0x10` score | `0x0059dc60`: `MOV EAX,[ECX + 0x248]`, so `0x00fb3788`, which the schema walk names `RidesBreakdownImminent.Score` (descriptor `0x0073199c`); the file sets **100** |
| `+0x20` copies in the slots | 255, where a door's is 1: every worn ride has a slot of its own |
| `+0x24` first response, `+0x28` lines, `+0x2c` mode | 194, 3, 2 |

Responses 194, 195 and 196 (`0x0076a7f8`, read in the image) are samples and lip files **191, 192, 193**, gesture
-1, model and bank word 0, props 5 and 0, the hard hat. The transcripts: "we've got trouble boss, a ride is about to break
down", "look out, she's gonna blow...", "one of your rides is about to break down, take cover...". Mode 2 takes the
line after the last and starts again past the count (`0x0059a796`, `0x0059a7fa`). The three rows after it in the
`.sam` are the same shape: `RidesBreakdownNoMechanics` 105, `BusyMechanics` 107, `MechanicOnIt` 110 at
`0x00fb378c` to `0x00fb3794`. The last is message `0x46`'s ("A mechanic on his way", below); the first two are
messages `0x44` and `0x45` ("A ride broken down and no mechanic to come", below).

**What follows a line said.** `FUN_0059b620`, after `Advisor_SayResponse`, builds an event of type `0x18`
(`FUN_0040f740`, vtable `0x006fd828`, whose first slot answers `0x18`) from the response's group, the record's
thing id and its `+0x04`, and sends it (`FUN_0040fb10`), for every message, a door's too. Its one hearer is the
tag system: "The tag a line leaves on the aerial", below.

**The original** (`q305/orig/`, `original/advisor.py`, Q265's `bounce.TPWS` left alone, the computer on seven
days so ticks alone are counted): the advisor is thing 3. Message `0x42`'s record read last said 0, line -1, said
0 from the load until tick **4800**, the tick the Belly Bounce's State of repair fell from 12.025 to 9.97 (the
byte 9, and 9 + 2.055 is not under 10); on that poll it read last said **4800**, line **0**, said 1, and the last
action's duration 6665 ms. It stayed so through five more falls to 2.4. His slots held message `0x80` at score 20
the whole run, unsaid, as "Why the stock door is silent" says. In the frame taken 1.5 s on (`a-told.png`) he is up
at the bottom right **wearing a yellow hard hat**. The small arrow beside him in that frame is the game's
pointer, holding the path tool's plank; it is there with no line said (`q306/orig/a-end.png`).

**OpenTPW** (`ParkAdvisorMessages.RideWornOut`, `ParkRideOperation.WearByRunning`; `q305/confirm.py`, the stock
park left alone on the desktop, 4 predictions of 4): the wear line of tick 5504 took the Bounce from 11.89 to
9.83, and the log read `post message=66 subject=13 score=100 threshold=25 tick=5504 result=queued pending=1`, then
on tick 5505 `attempt=1 ... response=194 sample=191 line=0` with `playback=started` and a duration of 5665 ms,
6665 with the scheduler's second, the original's. `advisor` read `saying sp_191.mp2` and
`message=66/score=100/line=0/lastTick=5505`; he is up in `run-fix1/2-told.png`. The five later lines under 10
posted nothing. Main's build, off-screen, posted nothing and counted `RIDE_WORN_OUT_MESSAGE` once on its own line
of the rule (`run-control`).

| | The original | OpenTPW |
|---|---|---|
| The post | from the wear, with the thing and its item | built; the item is not kept, and nothing reads the subject but the log and `advisor` |
| When it is said | the tick of the post | the same: "The advisor's turn in the sweep", below |
| The hard hat | drawn | built: "What he wears for a line", below |
| The event of type `0x18` after a line | sent, and heard by the tag system | built: "The tag a line leaves on the aerial", below |
| Lines 195 and 196, a second ride's own slot, the 480 sweeps before the same message again | the listing's | built and held by the tests alone |

## A mechanic on his way (message `0x46`, Q306)

**Who posts it.** The mechanic's decide, `FUN_004da5b0`, once the search has named a ride and a route to its
entry is found (`ride-operation.md`, "The mechanic's repair"): after the assignment (`FUN_004e01f0`), when the
ride's `+0x64` (called for, `FUN_004e03d0`) is nought and it is not waiting for an upgrade (`FUN_004e03e0`). The
call is `0x004da6bf`: `FUN_0059b590( 0x46, [mechanic + 0x218], [ride + 0xe], -1, 0, -1 )`, the same shape as
message `0x42`'s: the record's `+0x00` is the ride's id, its `+0x04` the ride's item, no line asked for, not
forced, the score the message's own; then `FUN_005194d0` and `FUN_0059a940`.

**Its row** (`0x0076e300 + 70 x 0x38` = `0x0076f250`, `meta_full.json`): posted only, category 0, either game
mode, score `0x0059dc90` (`MOV EAX,[ECX + 0x254]`, so `0x00fb3794`, `RidesBreakdownMechanicOnIt.Score`; the file
sets **110**), 255 copies in the slots, first response 204, 3 lines, mode 2. Rows 67 to 69 beside it are posted
only too, 255 copies each: 67 one line from response 197, 68 three from 198, 69 three from 201 (68 and 69: "A
ride broken down and no mechanic to come", below).

Responses 204, 205 and 206 (`0x0076a938`, read in the image) are samples and lip files **202, 203, 204**, model
and bank word 0, props 5 and 0, group `0x48`. The gesture of 204 and 205 is -1, the random chain; **206's
is 26**, row 10 of the gesture table. The transcripts: "one of your rides is broken down but a mechanic is on the
way to fix it", "don't be alarmed ... a mechanic is on the way", "don't panic ... mechanics are on the way".

**The original** (`q306/orig/run.sh`: Q265's `bounce.TPWS`, `scriptvar.py PID 13 4 1` twenty seconds after the
load, `advisor.py` and `object.py` polled together; ticks alone are counted): `VAR_BREAKSTAT` written on tick
1194, the Bounce's state 1 on 1198 with nobody assigned, and on the poll of tick **1201** its assigned member
read 26 and message `0x46`'s record `(1201, 0, 1, 0)`: last said that tick, line 0, said. The last action's
duration read 7498 ms. Message `0x42`'s record stayed never said. The frame 1.5 s on (`a-told.png`) has him up
in the hard hat, as for `0x42`. The ride was open again on tick 1313.

**OpenTPW** (`ParkAdvisorMessages.MechanicOnHisWay`, `StaffBehaviour.Decide`; `q306/confirm.py`, the stock
park's Belly Bounce broken by `scriptvar 13 VAR_BREAKSTAT 1` 25 s in, 4 predictions of 4): the log read `post
message=70 subject=13 score=110 threshold=25 tick=887 result=queued pending=1`, then `mechanic 26 found ride 13`
on the same tick, and on tick 888 `attempt=1 ... response=204 sample=202 line=0` with `playback=started` and a
duration of 6498 ms, 7498 with the scheduler's second, the original's. `advisor` read `saying sp_202.mp2` and
`message=70/score=110/line=0/lastTick=888`; he is up in `run-fix1/2-told.png` as the mechanic walks to the shut
ride. Main's build, off-screen, posted nothing and counted `MECHANIC_ON_HIS_WAY_MESSAGE` once (`run-control`).

| | The original | OpenTPW |
|---|---|---|
| The post | from the mechanic's decide, with the ride and its item | built; the item is not kept |
| When it is said | the tick of the post | the same: "The advisor's turn in the sweep", below |
| Response 206's own gesture row | played (`FUN_00598bf0`) | the random chain, counted `ADVISOR_RESPONSE_GESTURE_ROW` |
| The hard hat | drawn | built: "What he wears for a line", below |
| Lines 205 and 206, a second ride's own slot, a called ride's silence | the listing's | built and held by the tests alone |

## A ride broken down and no mechanic to come (messages `0x44` and `0x45`, Q307)

**Who posts them.** Not the ride: the mechanics' base, hearing the ride's event. SetState's arm 1
(`FUN_004e0e60`, `ride-operation.md`, "A ride broken down, in both games") builds an event of vtable
`0x006fd7c8`, whose first slot (`0x0040f3a0`) answers **7**, with the ride's id at `+8`, and sends it
(`FUN_0040fb10`) before the state is stored. The send is a call of `FUN_0050b550` on each subscriber of the type
in turn, then and there. The condemned arm's event is type 8 (`0x006fd7d0`), the upgrade arm's 9 (`0x006fd7d8`).
Three things subscribe to 7 (`FUN_0040f9e0`): the mechanics' base (`FUN_004da920`, the constructor of kind 10,
thing 2, the thing whose `+0xc` is the search's cursor), the tag system (`FUN_00509b40`, whose hearer
`FUN_00509bf0` only logs a type it does not know) and the kind-17 relay (`FUN_0050c9b0`, whose hearer hands 7
and 8 to `FUN_00481530`, not read).

**The base's hearer, `FUN_004dadc0`** (`FUN_0050b550`'s case 10):

1. `mFirstMechanic` (world `+0x1da73e`) nought: **message `0x44`** (`0x004dae62`).
2. Otherwise along the mechanics' chain (each one's `+0x210`):
   - a mechanic whose `+0x218` (`mObjectToRepair`) is the ride ends the look: **message `0x46`** when the ride's
     `+0x64` (called for, `FUN_004e03d0`) is nought (`0x004daf78`), nothing when it is set;
   - one whose `+0x218` is nought and whose state class (`FUN_005064f0`) is neither 3 (states 2 and 3, going to
     rest and resting) nor 4 (states 4 and 5, going on strike and on strike) is free, and ends the look with
     nothing said (`0x004daec5`);
   - any other (another job, or none and resting or striking) is passed.
3. The chain's end: **message `0x45`** (`0x004daff0`).

Each post is `FUN_0059b590( message, the event's thing id, [ride + 0xe], -1, 0, -1 )`, then `FUN_005194d0` and
`FUN_0059a940`: the shape of `0x42`'s and `0x46`'s. So a break with a free mechanic says nothing at the break,
and "a mechanic is on his way" follows from his own decide.

**Their rows** (`meta_full.json`; the score functions read in the image):

| | Row 68, `0x44` (`0x0076f1e0`) | Row 69, `0x45` (`0x0076f218`) |
|---|---|---|
| `+0x00`, `+0x08`, `+0x0c` | 1 posted only, category 0, either game mode | the same |
| `+0x10` score | `0x0059dc70`: `MOV EAX,[ECX + 0x24c]`, `RidesBreakdownNoMechanics.Score`, **105** in the file | `0x0059dc80`: `[ECX + 0x250]`, `RidesBreakdownBusyMechanics.Score`, **107** |
| `+0x20` copies | 255 | 255 |
| `+0x24`, `+0x28`, `+0x2c` | first response 198, 3 lines, mode 2 | 201, 3, 2 |
| Responses (`0x0076a878`, `0x0076a8d8`) | 198 to 200 = samples **196, 197, 198**, gesture -1, props 5 and 0, group 70 | 201 to 203 = samples **199, 200, 201**, gesture -1, 5 and 0, group 71 |
| The transcripts | "a ride is broken down and you don't have any mechanics, you should hire some right away" and two more | "... has broken down and the mechanics are all busy, maybe you should hire some more" and two more |

Row 67 (message `0x43`, one line, response 197, sample 193) has a score function that answers nought
(`0x005d60c0`: `XOR EAX,EAX`), and no call of `FUN_0059b590` names `0x43` as a constant: it cannot be said.

**The original** (`q307/orig/run.sh`, `advisor.py`, `object.py`, `thingwords.py` polled together; ticks alone
are counted):

- *No mechanic* (`none`: this game's own file, saved after the stock park's one mechanic was fired; the Bounce
  broken by `scriptvar.py PID 13 4 1` 20 s in): `VAR_BREAKSTAT` written on tick 950, and on the poll of tick
  **951** the Bounce's state read 1 and message `0x44`'s record `(951, 0, 1, 0)`: last said that tick, line 0,
  said. `0x45` and `0x46` stayed never said. He is up in the hard hat in `none-told.png`.
- *Every mechanic busy* (`busy`: the original's own `hotpot-later2.TPWS`, its pot, thing 43, broken 3.5 s in
  and its Bounce six seconds on): the pot's break said nothing; on tick 1827 mechanic 26's state read `0xc`,
  his job 43, and `0x46`'s record `(1827, 0, 1, 0)`. On the poll after the Bounce's break (tick 1853) a slot
  held **`(13, 1100, -1, 0x45, 107)`**: the ride, its item, no line asked for, the message, the score; on tick
  1870 `0x45`'s record read `(1870, 0, 1, 0)` and the action's duration 8331 ms. `0x44` stayed never said. He
  mended the pot, took the Bounce on tick 1907, and `0x46` was not said again.

**OpenTPW** (`ParkPeople.MessageForABreak`, called from `ParkPeople.BreakDown`; `q307/confirm.py`, 5
predictions of 5):

- `none` (the stock park, `fire 26`, `scriptvar 13 VAR_BREAKSTAT 1`): `post message=68 subject=13 score=105
  threshold=25 tick=876 result=queued pending=1` beside the break's own line, then on tick 877 `attempt=1 ...
  response=198 sample=196 line=0`, `playback=started`, duration 9832 ms; `advisor` read `saying sp_196.mp2` and
  `message=68/score=105/line=0/lastTick=877`. He is up in `run-none1/2-told.png`.
- `busy` (a Hot Pot bought and broken in its go, the Bounce broken as mechanic 26 walked to the pot): no post
  at the pot's break; `post message=70 subject=43` on tick 912, the tick he found it; `post message=69
  subject=13 score=107 ... tick=924 result=queued` at the Bounce's break; on tick 944, the first line over,
  `attempt=2 ... response=201 sample=199 line=0`, duration 7331 ms, the original's 8331 less the scheduler's
  second. He is up beside both shut rides in `run-busy1/2-told-busy.png`.
- Main's build (`control`, off-screen): the same break with no mechanic posted nothing.

| | The original | OpenTPW |
|---|---|---|
| The post | from the mechanics' base, hearing the break's event, with the ride and its item | from `BreakDown`, where the event is sent; the item is not kept |
| The order mechanics are looked at | the chain from `mFirstMechanic` | the staff list's; it decides only between a mechanic already naming the ride and a free one |
| When it is said | `0x44` on the post's tick; `0x45` 43 ticks after the line before it began | on the post's tick ("The advisor's turn in the sweep", below); `0x45` was 31 sweeps after the line before it began with his turn taken first, not measured again (why the original waits twelve more is not read) |
| The event's other two hearers, and a condemned ride's event | the tag system's log, the relay's `FUN_00481530` | counted, `RIDE_BROKEN_DOWN_EVENT`, `RIDE_CONDEMNED_EVENT` |
| The hard hat | drawn | built: "What he wears for a line", below |
| The second and third lines of each, a mechanic resting or striking, a mechanic already naming the ride | the listing's | built and held by the tests alone |

## The advisor's turn in the sweep (Q318)

The thing sweep (`FUN_00516380`) walks the used list from its head and hands each thing to `FUN_0050b360`, whose
arm for model `0xb` is the advisor's tick, `FUN_0059a550`: the poll's one row, then the best slot. A thing made is
linked in at the list's head (`FUN_00516270`), so the sweep runs newest first, and the advisor is thing 3. The
shipped park's list is 42 to 29, 15, 28 to 4, **3**, 2, 1; things 2 and 1 are models 10 (the mechanics' base) and
9, and the dispatcher has no arm for either. **His turn is the last a sweep takes**: every guest, member of staff,
placed object and manager has had its own, so a message any of them posts is said on the tick of the post. A park
made with no file makes its twelve reserved things in the same order, and whatever is bought or hired later goes
in at the head.

**The original** (`q318/orig/run.sh`, Q306's run again, twice): Q265's `bounce.TPWS`, its Belly Bounce broken
through its script 20 s after the load. The advisor's record for `0x46` read (0, -1, 0, 0) and then
(1165, 0, 1, 0), and the Bounce's assigned member read 0 on tick 1163 and 26 on 1165; the second run read
(1164, 0, 1, 0) and 26 on 1164. That the two ticks are one was predicted first and held in both; the tick itself
was guessed at 1190 to 1215 for the first run and missed, and reasoned at 1159 to 1205 for the second
(`q318/PREDICTION.txt`). The run of 2026-10-11 read 1201 and 1201.

**OpenTPW** (`ParkPeople.OnUpdate`): the messages' turn is the sweep's last call, after the guests', the staff's,
the arrivals', the staff pool's, the rides' and the gate's retry. `q318/confirm.py`, the stock park's Belly Bounce
broken 25 s in, predicted first (`q318/PREDICTION.txt`; the first round's `pending` counts were Q306's, wrong since
Q314, and were predicted again before the second run): the log read `mechanic 26 found ride 13 ... on mGameTick
885`, `post message=70 subject=13 score=110 threshold=25 tick=885 result=queued pending=2` and `attempt=1
message=70 subject=13 ... response=204 sample=202 line=0 tick=885`; `advisor` read `pending=1 attempts=1` and
`message=70/score=110/line=0/lastTick=885`, and he is up in the hard hat in `run-fix2/2-told.png` (the first run
read 886 three times). Main's build, off-screen, found and posted on 894 and said it on 895 (`run-control`).

| | The original | OpenTPW |
|---|---|---|
| A message posted on a thing's turn | said on that tick | the same |
| A message posted between sweeps (the door's, a tag's opening) | said at the next tick's advisor turn | the same, at that sweep's end |
| The poll's row | asked on his turn, after every other thing's | the same; the two built rows score by the park's age alone |
| Things 2 and 1 | after him, with no turn of their own | not things here |

Not run in the game: a ride's own post (the wear's `0x42`, the break's `0x44` and `0x45`; the tests hold the
wear's on its tick), and whether `0x45`'s 31 sweeps after the line before it moved. Not listened to in either game.

## What he wears for a line (Q308)

**The response's `+0x14` and `+0x18` are props, not face nodes.** `Advisor_SayResponse` keeps them at `0x00f79684`
and `0x00f79688`, each read once, by `FUN_00598dc0`. That routine is called from `FUN_00599800` after the line's
first clip is started (`0x00f79690`, set by the say) and again each time channel 0 takes the next clip
(`0x00f7968c`, set by `Advisor_Update` at `0x00599a73`), because **every clip of his hides the model's ten add-on
pieces as it starts**: all fifteen clips of `Advisor.md2` carry the same hide list, nodes 14 and 16 to 24, the
spatula, the eight hats and the bow tie (`hidelists.py`). `FUN_00598dc0` on the line's model slot (the response's
`+0x10` low half):

1. hides his five mouths (`0x00f796a0`, found by name by `FUN_0044b2e0`);
2. calls `FUN_00598ca0` with the first prop, and with the second where it is not nought;
3. where the clip's word carries `0x10000`, hides the node named `bug head`; and starts the clip word's
   particles (`0x3e0000`). The random chain's words carry no flag (`FUN_00598b20` stores bare clip numbers).

`FUN_00598ca0( n )` shows and hides pieces by costume id, through `FUN_00429ee0` (show: clears the node's `0x10`)
and `FUN_00429f60` (hide), which find the node with `FUN_0044b220( model, 0x400, id )`:

| `n` | What it switches |
|---|---|
| 0 | shows 19 and 20, his antennae |
| 1, 2, 4, 5, 6, 7, 9, 10, 11, 13 | hides 19 and 20 |
| 14 | hides 21, his right hand |
| -1 | the theme's costume: for each of four pairs filled by `FUN_004080e0` from `ThemeAdvisorCostumes[i]` (`0x00f797a4` the `AddOns`, `0x00f797b4` the `SubOffs`), shows the add-on and hides the sub-off |
| any but 0 | then shows the piece with id `n` itself, unless the clip's word carries `0x10000` |

His own parts are in no hide list, so antennae hidden for a hat stay hidden until a line whose prop is nought.

**The model's ids** (`md2surf.py`, the lookup records of `Advisor.md2`): 5 `Hardhat`, 6 `Security Hat`, 7
`Handyman Hat`, 8 `Kiosk Hat`, 9 `Ticket Hat`, 10 `Research Hat`, 11 `Bowler Hat`, 12 `Bowler_BTie`, 13 `Fast
Food Hat`, 14 `Spatula`, 19 and 20 the antennae, 21 and 22 the hands. The kiosk hat, 8, is the one hat that leaves
his antennae on.

**The table's props** (`responses.py`, all 610 rows read from the executable on disk):

| Slot | Props | Rows |
|---|---|---|
| 0, `Advisor.md2` | 0, 0 | 151 |
| 0 | 5, 0 the hard hat | 77 |
| 0 | 6, 0 | 47 |
| 0 | 7, 0 | 48 |
| 0 | 8, 0 | 8 |
| 0 | 9, 0 the ticket hat | 34 |
| 0 | 10, 0 | 58 |
| 0 | 11, 12 the bowler and its tie | 32 |
| 0 | 13, 14 the fast-food hat and the spatula | 18 |
| 1, the theme's `Advisor_JUN.md2` | -1, 0 | 129 |
| 1 | 0, 0 | 8 |

Slot 1 is a second model, built by `FUN_00457a90` from the level's own `advisor.wad` (`Advisor_` and the first
three letters of the theme). Every built park topic's rows are slot 0: the door's 308 to 311 carry (9, 0), the
rides' 194 to 206 (5, 0). The lobby's 394 carries (0, 0).

**OpenTPW** (`AdvisorProps`, `AdvisorModel.Wear`, `ParkAdvisorMessages.Topic.Prop` and `PropsOf`): as a line
begins the add-ons are hidden and the response's props put on, once a line rather than once a clip, which comes
to the same pieces since no clip of his keys an add-on. The speaker's queue, the lobby's lines and the console's
`say` wear (0, 0). `q308/confirm.py`, the stock park's Belly Bounce broken through its script: the line the
mechanic's decide posts began `Advisor: sample 202 ... wearing 5,0`, `advisor` read `wearing=5,0`, and in
`run-fix1/2-told.png` he is up in the yellow hard hat with no antennae, as the original's in `q306/orig/a-told.png`
(`side-by-side.png`); main's build read no `wearing` and drew him bare-headed with both antennae.

| | The original | OpenTPW |
|---|---|---|
| The props of a line | put on after every clip's start | put on as the line begins |
| The hat's colour | a full yellow | paler: he is lit past his textures (`AdvisorModel`'s light, set against his gloves) |
| The theme's costume, prop -1, and the theme's own model | slot 1's 137 rows | not built: no built topic names one; counted `ADVISOR_THEME_COSTUME` if one ever does |
| A clip word's `0x10000` (no prop, the head hidden) and its particles | a gesture row's | not built: the gesture rows are counted, `ADVISOR_RESPONSE_GESTURE_ROW` |

## The tag a line leaves on the aerial (Q309)

**Who hears a said line.** The event of type `0x18` (`FUN_0059b620`, above) carries the response's group at `+8`,
the record's thing at `+0xc`, -1 at `+0x10` and the record's `+0x04` (the thing's item) at `+0x14`. One thing
subscribes to `0x18`: the tag system, kind `0xc`, whose constructor `FUN_00509b40` subscribes it to types 7, `0xc`,
`0x14`, `0x18`, `0x19` and `0x13` (each `PUSH type`, `FUN_00415e00`, `FUN_0040f9e0`). Its hearer `FUN_00509bf0`
(`FUN_0050b550`'s case `0xc`) acts on three: `0x13` with a detail of 7, 8 or 9 (the postcard's three ends), `0x18`
and `0x19`; any other only logs. For `0x18`:

1. `FUN_0050af40( group )` finds the group's row in the **tag table**, `0x0075eca8` to `0x00761c68`, 382 rows of
   `0x20` bytes: nothing for `0x17e` and `0x17f`, else the row at the id's own place where it holds the id, else
   the first holding it. No row, no tag.
2. The tag's life is the event's `+0x10`, or the row's `+0x18` where that is under nought (always, from a line).
3. `FUN_00509e10` writes the tag's text from the row's string (`+0x0c`) by the row's kind (`+0x1c`): kind 0 the
   string alone, kind 2 the string with the item's name (`FUN_00412f50`).
4. `FUN_00481580` hands the messages control the tag: its id, the row's `+0x04`, the text, the life, the row's
   picture (`+0x08`), the row's two button types (`+0x10`, `+0x14`), the thing and the item.

**A row** (`lib/tagtable.py`, read from the executable on disk):

| Offset | What | Over the 382 rows |
|---|---|---|
| `+0x00` | id | 114 stand at their own place |
| `+0x04` | key: the id of the tag this one takes the place of | 107 name themselves |
| `+0x08` | picture, a short: the part of `msgtag.md2` shown | 0 to 5 |
| `+0x0c` | string | |
| `+0x10`, `+0x14` | the message bar's two buttons (`FUN_0048e610`: 3 and 4 `mb_locate`, 9 to `0x11` `mb_buy`, `0x12` `b_loan`, `0x13` `mb_resrch`, `0x14` `b_pkinfo`, `0x15` `b_staffcost`, `0x17` `mb_callmech`, `0x18` `b_entry`, `0x1a` `mb_accept`, `0x1b` `mb_decline`) | the second is 0 or 27 |
| `+0x18` | life, ms | 0 (121 rows), 30000 (228), 90000 (33) |
| `+0x1c` | text kind | 0 to 36 |

Of the response table's 149 groups, 148 have a row; the one without is `0x17f`, the lobby's lines'. The built
topics' rows:

| Group | Lines | Key | Picture | String | Buttons | Life | Kind |
|---|---|---|---|---|---|---|---|
| 68 | a ride about to break down (194 to 196) | 383, none | 1 | 71 | 3, 0 | 30000 | 2 |
| 70 | no mechanics (198 to 200) | 68 | 1 | 72 | 3, 0 | 30000 | 2 |
| 71 | the mechanics all busy (201 to 203) | 68 | 1 | 73 | 3, 0 | 30000 | 2 |
| 72 | a mechanic on his way (204 to 206) | 68 | 1 | 74 | 3, 0 | 30000 | 2 |
| 3 | a shut park people wait outside (2 to 4) | 4 | 2 | 7 | 24, 0 | 30000 | 0 |
| 150 | the park open (308, 309) | 151 | 1 | 149 | 0, 0 | 30000 | 0 |
| 151 | the park shut (310, 311) | 150 | 1 | 150 | 0, 0 | 30000 | 0 |

**The messages control's list, `FUN_0048eb20`** (through `FUN_0048ede0`; nothing is done with the interface down,
`[0x007cb2ac]` nought). The ten tags are controls made once by `FUN_0048eaa0` (`FUN_0048ea00`: 70 by 65, mesh
`msgtag`, handler `FUN_0048dae0`, help row 475, "Message - left-click to open, right-click to delete"), with ten
records of `0x208` bytes at `0x007c26b0` (the control, in use, the text).

1. Every tag standing whose id is the new one's **key**, about the same thing and item: where the new id is its
   own key the standing tag takes the new text and nothing more is made; otherwise it is taken off at once
   (`0x100002` with 1). So a mechanic's line takes off the about-to-break tag of its ride, and the door's two take
   each other's.
2. Otherwise, with fewer than ten held (`[0x007ca09c]` against `[0x00750610]`), the first free record's control
   takes the picture (`FUN_0065d3a3`), the text, timer `0x80080` at the life and timer `0x80081` five seconds
   before it (both only for a life over nought), and its data; the mast is sent `0x100001` with it, and the
   interface's sound `0x1e` plays.

**The mast, `LAB_004a11e0`** (control `0x2d`, `[0x007cb2c4]`; `hud.md`, "The management gadget"):

- `0x100001`, a tag put on: the tag is shown, the mast's top edge goes up by the tag's height and ten
  (`FUN_0065cd95( 0, -h, 0, 0 )`), the tag is put across the mast's middle with its top eight under the mast's
  new top, and the red top `0x2e`, which follows the mast's top edge, plays the stream at `0x00752e48`.
- `0x100002`, a tag taken off: hidden, then message 5 to it, at once with a 1 and half a second on otherwise.
- `0x100003`, the top's right click (`FUN_004a1190`): message 5 to each tag from the first, 100 ms after the one
  before, the first 100 ms on.
- A tag's message 5 (`FUN_0048dae0`): its particle killed, its timers stopped, its record freed, the mast's top
  put down by its height and ten, each later tag moved down the same and played the stream at `0x007502f0`, the
  red top the same, and the message bar retracted where it showed this tag.

**A tag's own handler, `FUN_0048dae0`:** timer `0x80080` hides it and sends the mast `0x100002` with nought, so
it is off half a second after its life ends; timer `0x80081` starts particle effect `0x5b` over its middle, once;
a left click plays sound `0xc0` and shows its message on the bar (`FUN_0048d9c0`, which posts advisor message
`0x11c`), or retracts the arm where it is the one on show; a right click, unless its message is the one being
shown (`[0x007ca094]`), hides it and sends `0x100002` with nought. The tags are written to a park file
(`FUN_0048ee50`, records of `0x21c` bytes) and read back (`FUN_0048ef70`): `saves.md`, "OpenTPW's writer, the
aerial's tags".

**The text, `FUN_00509e10`** (the tag system's buffer at `+0xc`, 0x400 characters; its answer is not looked at by
its caller): a switch on the row's kind. Kind 0 formats the row's string alone from the table at `[0x0078ba90]`,
the running language's `TAG_SYSTEM.str` (332 rows). Kind 2 formats it with one argument, number 0, the item's
name: `FUN_00412f50` on the item manager `0x007890a0`, the wide string at the item's record `+0x410` (what fills
it is not read: it reads "Belly Bounce" for 1100 and "The Hot Pot" for 1140, each item's `Info.Name`); with the
event's item nought it logs "Object id is IDNULL in CTagSystem" and leaves the buffer as it stood. Kind 1 writes
"A string needs to be put in here" for a thing that exists, kind 4 "String and number goes here"; kinds 3 and 5
to `0x24` format numbers and names from the event and the park's globals, not read further.

**The original** (`q309/orig/run.sh`: Q265's `bounce.TPWS`, `scriptvar.py PID 13 4 1` twenty seconds after the
load, `original/tags.py`, `advisor.py` and `object.py` polled together): with no tag the mast's rectangle read
(94, 976, 144, 984) and none held. On the poll of tick 1198, 21.47 s in, where message `0x46`'s record read said,
one was held: its control at **(84, 909, 154, 974)**, the mast **(94, 901, 144, 984)**, its text "Your Belly Bounce
ride has broken down, and a mechanic is on his way to fix it." At 52.01 s, tick 1368, none was held and the mast
read (94, 976, 144, 984) again: **30.54 s** on, the life and the half second. The frames (`a-told.png`,
`a-end.png`) show the red top lifted over a red block with a white picture, then back down.

**OpenTPW** (`ParkTags`, `ParkAdvisorMessages.Topic.Group`, `ParkGadget.PutTags`; `q309/confirm.py`, the stock
park's Belly Bounce broken through its script 25 s in, 5 predictions of 6 and the control's one): the line began
and the log read `Park tags: tag 72 picture 1 subject 13 life 30000 standing 1` and `Park gadget: aerial:
mast=94,901,144,984 top=77,773,161,901 tag900=84,909,154,974/frame=1/shown=1`, the original's rectangles; `tags`
read `standing=1 tag=72/picture=1/subject=13/age=1945/hidden=0`; the red top's upper edge stood at window row 363
where it had stood at 398, the 35 rows 75 units are at 720 down; 28 s on the tag stood, and 32.5 s on `tags` read
`standing=0` and `mast=94,976,144,984 top=77,848,161,976`. `side-by-side.png` holds both games' aerial before,
with the tag and after. The prediction that missed was a pixel rule set without the picture in mind: the red top
at rest covers the tag's box, so the box was 14.6% red before the line where under 5% was predicted, and 19.3%
with the tag where over 30% was. Main's build said the same line, moved nothing and counted
`ADVISOR_MESSAGE_SAID_EVENT`.

| | The original | OpenTPW |
|---|---|---|
| The rows | 382 in the executable | the six the built topics' groups name; any other group with a row is counted, `TAG_ROW_NOT_LISTED` |
| The match of a standing tag | id, thing and item | the same; the item is the subject's catalogue id |
| The times | the interface's timers, real milliseconds | the same clock (`Time.WallMilliseconds`), from the gadget's update |
| The red top and the later tags as one goes | played down (streams `0x007502f0`, `0x00752e48`) | put down at once |
| The particle five seconds before the end | effect `0x5b` over the tag | counted, `TAG_ENDING_PARTICLE` |
| A left click | sound `0xc0`, the message bar on the arm with the text and the buttons | the same (`hud.md`, "The message bar"), and an opened tag's timers stop there as here |
| The tag's text | written from the row's string by its kind (`FUN_00509e10`), 37 kinds | kinds 0 and 2, the two the six rows use: the row of `TAG_SYSTEM.str`, and for kind 2 the item's `Info.Name` where its parameter 0 stands; any other kind, and a kind 2 about no item, is counted, `TAG_TEXT_KIND`, and empty. Shown on the message bar, written to a park file and read by `tags` |
| The sounds' loudness | not measured | each at its variation's own volume, as the boot's |
| The tags in a park file | written and read | written and read (`saves.md`, "OpenTPW's writer, the aerial's tags") |
| The other two events (`0x19`, the postcard's) | heard | nothing sends them here |

## The line of an opened tag message (message `0x11c`, Q312)

**The post.** Showing a tag's message on the bar (`FUN_0048d9c0`, `hud.md`, "The message bar") calls
`FUN_00486b00( 0x11c )`: `FUN_0059b590( 0x11c, thing 0, 0, line -1, not forced, score -1 )`, then `FUN_005194d0`
and `FUN_0059a940`, the same gate and slots as every post. The bar's turn as the arm starts in with it
(`FUN_0048e2f0`, `0x1001d8`) calls `FUN_00486b40( 0x11c )`, the withdrawal. `FUN_004a2590`, which sends the arm
in before it sends it out, comes before the post in `FUN_0048d9c0`, so a second tag opened over the first
withdraws and then posts.

**Row 284** (`0x00772120`, from the emulated table): posted only, category **1**, either game mode, score
`0x0059e8c0` (`MOV EAX,[ECX + 0x328]`, so `0x00fb3868`), one copy in the slots, first response 544, one line, mode
2. Response 544 is sample 561, lip 561, the random gesture chain, model slot 0, no prop, group 383 (no tag):
"these messages help you stay on top of new developments; to look at related information, or to close or delete
the message, click on the buttons".

**The score is `HelpMessage.Score`, 1000 in the file.** In the schema's descriptor list (`0x0072e210`, 60 bytes
a row) a section's fields come BEFORE its type 1 row, a type 0 row after it; counting four bytes a field from
`RidesBreakdownImminent.Score` at `+0x248` (descriptor `0x0073199c`) puts `HelpMessage.Score` at `+0x328` and
`HelpMessage.ContextHelpScore` at `+0x32c`, and `FUN_0059ab50` reads the second as `DAT_00fb386c`. Both are 1000
in the file, so the slot's score in the running original does not tell the two apart; the walk does.

**The category** is `MessageGroups[1]`, "Tutorial messages": `MinTimeSameMessage` 0, `SayOnlyOnce` 1,
`DiscardAfterSlaps` 3. The gate refuses it with the Tutorial option off, once he has said it, and once he has
been cut off saying it three times (the table under "The accept gate").

**The withdrawal, `FUN_0059aa70( id )`.** (1) Where the advisor's byte `+0x16e4` is set, the message's category is
1 and `+0x16dc` is the id, the byte is cleared: the kept message at `+0x16d0`, which `FUN_0059ab50` raises again,
forced, at `HelpMessage.ContextHelpScore` (its one caller `FUN_00488ba0`). The byte is the kept record's own `mValid`: a raise of a message `FUN_0059c500` answers 1 for copies the whole record there before the gate is asked (`FUN_0059a940`). (2) Every
occupied slot whose message is the id is emptied. (3) The message being said is `+0x16ec` while the clock
(`FUN_00402d80`) is under `+0xd8 + +0xdc`, the last line's start and its length and second, and `0x160`
otherwise; where that is the id: `Advisor_StopSpeaking`, the message's said byte `+0xe8` cleared, its `+0xec` one
up, and `+0xdc` nought, so the next sweep may say a line. The tick stores `+0x16ec` with each line said
(`0x0059a86a`).

So he explains the message bar the first time a tag is opened; let finish, he never says it again in that park;
shut up by the bar folding away, he starts again at the next opening, and after three such cuts not at all.

### The original (`q312/orig/run.sh a|b`, `original/advisor.py`; predicted first, five of five)

Q265's `bounce.TPWS`, its Belly Bounce broken 20 s after the load, the mechanic's tag clicked four seconds after
it came. **Let finish (a):** 7.2 s after the mechanic's line began slot 0 read (thing 0, line -1, `0x11c`, score
**1000**); 0.47 s on, as that line's 7498 ms ended, the record read (tick 1210, line 0, said 1, slaps 0) and the
action 13332 ms; the tags held stayed one. The bar folded 25 s after the click and opened again: no slot held
`0x11c` in the 14 s after and the record did not change. **Cut off (b):** the bar folded 10 s into the line:
record (1202, 0, **said 0, slaps 1**), the action's length **0**. Opened again: in a slot and said 0.1 s on (tick
1262, said 1, slaps 1); folded: slaps 2; opened, said on tick 1303, folded: slaps 3; a fourth opening: no slot
held it in 12 s. In the frame he is up beside the bar with no hat (`q312/side-by-side.png`).

### OpenTPW (Q312: `ParkAdvisorMessages.TagMessage`, `Withdraw`, a topic's `Category`; `ParkGadget.WithdrawBarLine`)

`q312/confirm.py a|b|off|control`, the stock park's Bounce broken 25 s in and the tag clicked four seconds after
the mechanic's line began; predicted first, seven of seven (a twice, the first run's third lost to the script's
own log window, the log itself as predicted). The click logged `Park advisor: post message=284 subject=0
score=1000 threshold=25 tick=891 result=queued pending=1`; 2.8 s on, `attempt=2 message=284 subject=0 score=1000
threshold=25 response=544 sample=561 line=0 tick=905 duration=12332`, a second under the original's 13332 as
every line's is (the second is added to the wait); `advisor` read `saying sp_561.mp2` and
`message=284/.../said=1/slaps=0`. Folded after the line: `withdraw message=284 cleared=0 cut=0 slaps=0`; opened
again: `result=said-once`. **Cut (b):** folded three seconds into the line: `withdraw message=284 cleared=0 cut=1
slaps=1`, `Advisor: cut off mid-line`, `advisor` reading `quiet` and `said=0/slaps=1` a second on; each of the
next two openings said it again on the next sweep (attempts 3 and 4, `playback=started`), cut to slaps 2 and 3;
the fourth opening logged `result=slapped`. **`tutorial off`** in the console: `result=tutorial-off` and no
attempt. Main's build named message 284 nowhere and counted `TAG_MESSAGE_BAR_ADVISOR_LINE`.

| | The original | OpenTPW |
|---|---|---|
| The said byte and the cuts | the advisor thing's records, in the park file | the same, for the built messages ("The records in a park file") |
| The kept message (`+0x16d0`, `FUN_0059ab50`) | forgotten by a withdrawal of its id | kept by nothing here |
| The Tutorial option | read at each post | the same; the console's `tutorial on|off` sets it in memory |
| The cut | `Advisor_StopSpeaking` | the same call (`Advisor.StopSpeaking`), with its cry where his voice had started |
| The other 39 screens' lines (`0xbc` to `0xd5`, `0x11d` to `0x135`) | posted as each opens | not posted |

## The records in a park file (Q313)

The advisor is a thing: `FUN_005156a0` (`0x00515920`) allocates `0x16f0` bytes, constructs it with `FUN_00599e70`
and keeps its id at the world's `+0x1da71c`, which `FUN_005194d0` answers. So every message's record ("The
per-message record", above) is written with the thing and read back with it, and what he has said, the line he is
on, the tick and the cuts all last across a save. In a park file he is thing 3, model 11, 5,846 bytes, and message
`id`'s record is at the thing record's `+0xcc + id * 0x10`, memory's `+0xe0` (FileFormats `saves.md`, "The advisor
(model 11)"): 6,318 records in eighteen stored files of the original, none odd (`lib/parkfile.py -a`).
`q257x/orig/broken-by-the-original.TPWS`, saved by the original after its mechanic's line, holds `0x46` as
(1815, line 0, said 1, slaps 0); Alexah's Lost Kingdom park holds `0x11c` as said 0, slaps 3: cut off three times
and never to be said again there.

### OpenTPW (`ParkWorld.AdvisorHistoryOf`, `PutAdvisorHistories`; `ParkAdvisorMessages.Restore`, `Written`)

A load hands each built message's record to the park's messages (`Level`, logged `Park advisor: restored
message=...`), and `savepark` writes the seven built messages' records over the file's, the three bytes after a
said byte and every other message's record left the file's. `PARK_ADVISOR_SAVED_HISTORY` is gone.

`q313/confirm.py fix|control`, predicted first, four of four twice and the control one of one twice: the stock
park's Bounce broken, the mechanic's line said on tick 880 and the tag's line on 911, the bar folded, `savepark`.
The save logged `advisor message 70 written: line 0 lastTick 880 said 1 slaps 0` and `advisor message 284 written:
line 0 lastTick 911 said 1 slaps 0`; the file's thing 3 held those two records and every other byte of its 5,846
was the shipped file's. Loaded from the Load Park list: `restored message=70 line=0 lastTick=880 said=1 slaps=0`,
the same for 284, `advisor` reading both, and the tag clicked 23 s on logged `post message=284 ...
result=said-once` with the bar out and nobody beside it. Main's build wrote both records fresh, counted
`PARK_ADVISOR_SAVED_HISTORY`, and after its load said the line again, the advisor up beside the bar
(`q313/ours-side-by-side.png`).

**The original, given that file** (`q313/orig/run.sh`, predicted first, two of two): `advisor.py`'s one line in
90 s read `'0x11c': (911, 0, 1, 0), '0x46': (880, 0, 1, 0)` from the load on; the tag clicked 27 s on put the bar
out, no slot ever held `0x11c` and no advisor came up (`orig/fix-clicked.png`). Its control is Q312's run a, a file
with no record of `0x11c`: a slot held it at the click and he said it.

| | The original | OpenTPW |
|---|---|---|
| The records of the seven built messages | read and written with the thing | the same |
| The other 344 records | the same | carried: the file's own bytes are written back |
| The eight slots | written with the thing | the same ("The slots in a park file", below) |
| The last action's start and length, the message playing, the kept message | written with the thing | carried as the file's |
| A park made with no file | the constructor's | the same |

Not in a game run: a rotation carried across a load (the test holds line 1 after the file's 0), the cooldown from a
file's tick, a file with cuts, Alexah's own parks. The two calls in `Level` are held by the game runs alone.

## The slots in a park file (Q314)

**The serialiser, `FUN_00599f60`,** names every field it writes, and writes them in this order after the thing's
sixteen bytes (`FUN_0050b090`): `mTimeSig` (`+0x0c`, by `FUN_0041a860`), `mSoundId` (`+0x10`), the eight slots
(`+0x14` on, each by `FUN_0059ba30`), `mNextAvailableQueuePos` (`+0xd4`), `mLastActionStarted` (`+0xd8`),
`mLastActionDuration` (`+0xdc`), the 351 `mHistory[i]` (`+0xe0`, sixteen bytes each, whole), the kept message
(`+0x16d0`, by `FUN_0059ba30` again), `mRequestedLastTutorial` (`+0x16e8`, one byte) and `mMessagePlaying`
(`+0x16ec`). `FUN_0059ba30` writes a message record ("The message record", above) as twenty-one bytes, not
memory's twenty-four: `mThingId` (two bytes), `mSecondary`, `mMessage`, `mScore`, `mValid` (one byte),
`mResponseNo`, `mOverrideOnlyOnce` and `mScoreOverridden` (a byte each). So the file's record is 5,846 bytes
where memory's is `0x16f0`, a slot is at `24 + 21 n` of it, and the histories begin at 204 (FileFormats `saves.md`,
"The advisor (model 11)").

**The two times are readings of the second stopwatch** (`FUN_00402d80` is `FUN_00403150`), which the clock module
saves and a load sets again (`park-engine.md`, the clock's table): `Easymode.TPWI`'s action began at 114,820,087
and lasted 10,832 ms, and its file's stopwatch reads 114,876,286, so the wait a file holds is over or not by the
file's own clock, whatever the computer's.

**What the files hold** (`q314/sweep.py`, 21 stored files of the original): twenty hold one taken slot and one
holds none; the taken slot is always message `0x80`, the park's opening, at a score of 20, line -1, neither byte
set, in slot 2 (sixteen files) or slot 0 (four). It is under `MinScoreForConsideration`, 25, so the tick never
says it and never lets it go, and while it stands the gate's count of copies (test 4, one copy for `0x80`)
refuses every later post of the park's opening. A slot let go keeps its other bytes (the shipped file's slots 0
and 1 read messages `0xbc` and `0x15a` at 1000 with `mValid` nought); slots never used are `CD` throughout.
`mTimeSig`, `mSoundId` and `mNextAvailableQueuePos` are nought in all 21.

### OpenTPW (`ParkWorld.AdvisorSlotOf`, `PutAdvisorSlots`; `ParkAdvisorMessages.RestoreSlots`, `WrittenSlots`)

A load puts each taken slot's message into the same slot of the park's messages with its file's score, subject and
second word (`Level`, logged `Park advisor: restored slot=...`), and `savepark` writes the eight slots back: a
waiting message whole, as `FUN_0059b590` builds an unforced post with no score given, its second word the
subject's item; of an empty slot the `mValid` byte alone. A taken slot this game cannot say (a message of no built
topic, a line asked for by number, a forced post, a score the poster gave) is counted
`PARK_ADVISOR_SAVED_SLOT_NOT_BUILT`, keeps its slot, is said by nothing and is written back as the file's: no
stored file holds one.

`q314/confirm.py fix|control`, predicted first, four of four and the control one of one. Entering the stock park
logged `restored slot=2 message=128 subject=0 secondary=0 score=20` and `advisor` read `pending=1`. A Hot Pot
bought and broken, the Belly Bounce was broken as the mechanic's line for the pot was being said: `post message=69
subject=13 score=107 ... result=queued pending=2`. `savepark` under the pause logged `advisor slot 0 written:
message 69 thing 13 secondary 1100 score 107` and `advisor slot 2 written: message 128 ...`; the file's slot 0 read
`0d00 4c040000 45000000 6b000000 01 ffffffff 00 00`, the record the original's own memory held in Q307's run
(thing 13, 1100, line -1, `0x45`, 107), and every other byte of the eight slots was the shipped file's. Loaded
from the Load Park list: both slots restored, and on the first sweep `attempt=1 message=69 subject=13 score=107
... response=201 sample=199 line=0 tick=861`, tag 71 on the aerial at (84,834,154,899) beside the pot's
(`run-fix1/2-loaded.png`); the speaker refused the sample, the gadget's opening line having him. Main's build read
`pending=0` on entry, wrote no slot (its file's slot 0 the shipped file's) and after its load made no attempt and
left one tag.

**The original, given that file** (`q314/orig/run.sh`, predicted first, three of three): from the load, tick 860,
its slots read `(0, 13, 1100, -1, '0x45', 107)` and `(2, 0, 0, -1, '0x80', 20)` and its action the file's; a
quarter of a second on, tick 861, the record of `0x45` read `(861, 0, 1, 0)`, slot 0 was let go, the action was
8,331 ms long and a second tag stood at (84,834,154,899) reading "The Belly Bounce ride has broken down and all
your mechanics are busy. Maybe you should hire some more?". Its control is Q313's load of a file with slot 2
alone: no `0x45` in any slot or record.

| | The original | OpenTPW |
|---|---|---|
| A waiting message of a built topic | read and written with the thing | the same |
| A taken slot of any other kind | the same | counted, kept taken, written back as the file's |
| A slot let go | its other bytes stand as they were | the file's bytes stand; a message said here leaves the file's older bytes, not its own |
| The shipped park's opening message at 20 | waits for ever and refuses the next | the same |
| The last action's start and length, `mMessagePlaying` | on the second stopwatch, saved and set again | read and written: "The last action in a park file", below |
| The line said at once after a load | heard | heard: no line is said at a park's entry any more ("The poll, and the control panel's line", below) |

Not in a game run: the door shut and opened again after a load (the test holds `duplicate`), a taken slot of an
unbuilt message, a full eight slots. The two calls in `Level` are held by the game runs alone.

## The last action in a park file (Q315)

**Three fields time his wait, and all three are saved.** The tick (`FUN_0059a550`) returns while the second
stopwatch (`FUN_00402d80`) reads under `mLastActionStarted + mLastActionDuration` (`+0xd8`, `+0xdc`), an unsigned
compare; a line said stores the stopwatch's reading, the line's length and 1000, and the message's id in
`mMessagePlaying` (`+0x16ec`). The withdrawal (`FUN_0059aa70`) cuts him off only inside that wait and only where
`mMessagePlaying` is its message, and then stores nought in the duration alone: the start stands. The constructor
(`FUN_00599e70`) stores nought in the start and the duration and does not write `mMessagePlaying`. The serialiser
writes the three with the thing ("The slots in a park file", above), and the clock module holds the stopwatch
they are read against, which a load makes read the file's again (`park-engine.md`, the clock's table,
`FUN_00402e80`): **a park saved as he speaks goes on waiting after its load for what was left of the line.**

**What the files hold** (`q315/sweep.py`, the same 21 stored files of the original): two were saved inside the
wait, `q257x/orig/broken-by-the-original.TPWS` (start 115,150,244, wait 7,498, playing `0x46`, stopwatch
115,156,783: 959 ms left) and a Full Simulation autosave (28,196 ms left, playing 0); in the other nineteen the
wait was over, by 134 ms to fifteen minutes. Five hold a wait of nought, a withdrawal's cut.

### OpenTPW (`ParkClock.Second`, `SecondAt`; `ParkWorld.AdvisorActionOf`, `PutAdvisorAction`; `ParkAdvisorMessages.RestoreAction`, `WrittenAction`)

The messages keep the start as a moment of this game's clock, the wait and the message playing. A load moves the
file's start onto this clock by its distance from the file's stopwatch (`Level`, logged `Park advisor: restored
action started=... left=...`), as a saved script's deadline is moved (`ParkRides.Moved`); `savepark` writes the
start back as a reading of the stopwatch the file is written with, which `ParkClock.Put` moves with the clock.
An advisor with no file's action who has said nothing writes the constructor's noughts.

`q315/confirm.py theirs|fix|control`, predicted first: one of one, three of three and the control one of one.
The original's own file entered: `restored action started=115150244 duration=7498 playing=70
stopwatch=115156783 at=6556 left=959`. The stock park entered: `started=114820087 duration=10832 playing=282 ...
left=-45367`. Q314's scene (a Hot Pot broken, and the Belly Bounce broken as the mechanic's line for the pot is
said), saved under the pause 1,507 ms into the line: `Save: advisor action written: started 114902772 duration
7498 playing 70 stopwatch 114904279`, the file's three fields and its `KOLC` second dword the same, and no other
byte of thing 3 outside the slots and the records moved. Loaded from the Load Park list: `restored action ...
left=5991`; a second in one tag stood (`run-fix1/2-inside-the-wait.png`); the attempt of `0x45` came 6,206 ms
after the load's moment, on tick 893, 25 ticks after the load's 868, and tag 71 stood beside the pot's
(`3-after-the-wait.png`). Main's build wrote the stock file's action, attempted `0x45` on the first tick after
its load and had both tags a second in.

**The original, given that file** (`q315/orig/run.sh`, predicted first; the length was predicted 8,331 and read
8,332): from the load, tick 868, its action read `(114902772, 7498)`, its slots `(0, 13, 1100, -1, '0x45', 107)`
and the opening's, the record of `0x45` `(0, -1, 0, 0)`, one tag held. 6.21 s of wall time on, tick 893, the
record read `(893, 0, 1, 0)`, the action `(114910475, 8332)`, a start 205 ms after the saved wait's end, and the
second tag stood at (84,834,154,899). Q314's file, saved with the stock's action, was said a quarter second after
its load.

| | The original | OpenTPW |
|---|---|---|
| A file saved inside the wait | waits what was left, then says the best slot | the same; the same tick in the run above |
| The message playing | read by a withdrawal inside the wait | the same |
| A withdrawal's cut | the duration nought, the start as it was | the same |
| The stopwatch | its own, unscaled; parts from the clock when the clock's rate is not 1 | the file's second dword moved with the clock: this game's clock has one rate |
| The control panel's line | a polled message through the slots, so it starts an action | the same ("The poll, and the control panel's line", below) |

Not in a game run: a withdrawal inside a file's wait (the tests hold it), a file with a wait of nought, a park
made fresh saved before any line, Alexah's own parks. The two calls in `Level` are held by the game runs alone.
Not heard in either game.

## The poll, and the control panel's line (message `0xb9`, Q316)

**The poll.** The tick's first act, before its wait is looked at, is `FUN_0059c680( 1 )` on the balance object
(`0x0059a58b`, `ECX = 0x00fb3540`): from the row its first dword names, the cursor, nought again past row `0x15e`,
it walks the metadata table until it has scored ONE row whose `+0x00` is nought, and leaves the cursor on the row
after. A row is scored by its function (`+0x10`) where its mode (`+0x0c`) fits the game, and -1 otherwise; a score
above nought of a message the gate (`FUN_0059abc0`, not forced) lets by makes that message the answer, and
anything else answers `0x160`, no message. The tick then scores the answer again (`FUN_0059bf20`, -1 for `0x160`)
and, where that is above `MinScoreForConsideration` (`0x0059a5f5`, `JLE` past), posts it as any post: about thing
nought, line -1, not forced, no score given (`FUN_0059b590`, `FUN_0059a940`). So a polled message waits in the
slots with the posted ones, and is said on the same tick where no wait is on.

**155 of the 351 rows are polled**, read from the running original (`original/advisortable.py`): 1-32, 58-65,
71-82, 85-92, 98, 100-105, 107-124, 140-141, 144-160, 169-173, 185, 238-257, 259-265, 267-269, 274-276, 278-282,
314, 316-317 and 347-350. A round is 155 ticks. **The emulated table is wrong in one row:** `meta_full.json` has
row 0's `+0x00` and `+0x0c` nought, and the live table holds 1 and 2 there (posted only, either mode); its other
4,912 dwords are the live table's. Every row's id is its row.

**The cursor is in the park file.** `FUN_0059bea0` stores nought in it as the balance object is first built; the
save's advisor-scoring module is that one dword (`FUN_0059c890`, between the cheats' `STHC` and its own `CSDA`),
and the load reads it back into the same place (`FUN_0059c860`, from `FUN_00415270` at `0x00415d21`). So a park
goes on from the row its file was saved on, and a park made with no file from wherever the program's cursor
stands. `Easymode.TPWI` holds 277, the row after the last its makers' park scored: its record of `0x11a`, row
282, is said on tick 605, and 150 ticks on, the file's tick 755, the 150th polled row after it is row 276.

**Row 185, message `0xb9`:** polled, category 1 (the tutorial messages: said once, refused with the Tutorial
option off, dropped after three cuts), either game mode, one copy, first response 404, one line. Response 404 is
sample 377, lip 377, the random gesture chain, model slot 0, no prop, group 383 (no tag): "see the control panel
over there at the bottom left part of the screen...". 109 polled rows stand before it. Nothing withdraws it: each
of the 53 calls of the withdrawal (`FUN_00486b40`, `FUN_0059aa70`) pushes a constant, and none is `0xb9`.

**Its score, `0x005a06c0`** (in no Ghidra function): `FUN_004f8800`'s time since `mGameTick` nought
(`ride-operation.md`, "The 24 months") over `0xC92A69C000` by `__alldiv`, whole days; `JLE` to nought against
`[ECX + 0x5dc]`, `JGE` to nought against `[ECX + 0x5e0]`, else `[ECX + 0x5e4]`. Walked from the schema
(`lib/advisorschema.py`) those are `TutorialUIPanel.AfterTime`, `.CutoffTime` and `.Score`: 10, 60 and 110 in
`Advisor.sam`. A tick is 3,750 funny seconds, a day 23.04 ticks: **the line is owed from tick 254 to tick 1382 of
a park**, and is said on the first of those ticks the cursor stands on row 185. Row 282's (`0x005a0720`, message
`0x11a`) is the same function on `TutorialExplainTooltips` (20, 80, 100), built in "The tips' line", below, and
row 281's (`0x005a0780`) is `GoldTicketExplanation.Score` on a count of one, not built.

**What the shipped park holds.** `Easymode.TPWI`'s record of `0xb9` is (257, line 0, said 1, slaps 0): its makers'
park said the line on its eleventh day, and the file begins on tick 755. So **Instant Action's Lost Kingdom never
says it**: every poll of row 185, the first on tick 877, scores 110 and is refused by the gate as said once.

### OpenTPW (`ParkAdvisorMessages.Poll`, `ScoreOf`, `Refusal`, `PollCursor`, `RestoreCursor`; `ParkWorld.AdvisorPollCursorOf`, `PutAdvisorPollCursor`; topic `ControlPanel`)

Each sweep's `Tick` polls first, from the cursor's row, which is the program's (`ParkAdvisorMessages.ProgramCursor`,
handed to every park by `Level`), set from a loaded file's dword (`Park advisor: restored poll cursor row=...`)
and written to `savepark`'s file. The built polled rows are 185, 282 ("The tips' line", below) and 1 ("A shut park people wait outside", below); each of the other 152 is counted
`PARK_ADVISOR_POLLED_ROW_NOT_BUILT`. A topic's score is its function's (`ScoreOf`: the age's bounds for the
control panel's, the key's value for the rest), asked at the poll, at the post and at the tick's recheck
(`FUN_0059b620` calls `FUN_0059c0a0`). The poll logs a line a change of its answer: `Park advisor: poll
message=185 row=185 days=... score=... tick=... result=...`. **The line a park was entered with is gone:**
`UI.ParkLines.ExplainGadget` said sample 377 past the messages at every entry and every load, which the original
never does.

**How this was reached** (`q316/PREDICTION.txt`, five rounds, each written before its runs). The first build
counted the cursor in polled rows from nought at the program's start, of 156, and said the fresh-record file's
line on tick 866. Its predictions of that build held, and two things failed: the line's length (guessed under
18.5 s, read 19 to 21 s), and, in the fourth round, the original's tick after its Tutorial switch was held off
through one turn (`orig/run5.sh`, `original/bytehold.py`): predicted 1033, read **1032**. That one tick was the
round, 155 and not 156, which the live table then showed (row 0), and the same reading explained why the original
had said the fresh-record file's line on tick 877 after every load: its cursor came from the file. The build was
corrected and everything run again.

**The corrected build** (`q316/confirm.py stock|fresh|nofile|cutoff|early`, the fifth round, thirteen of
thirteen; `run5-*`). **The stock park** (four of four): `restored poll cursor row=277`, `restored message=185
line=0 lastTick=257 said=1 slaps=0`, one poll line, `row=185 days=38 score=110 threshold=25 tick=877
result=said-once`; no `sample=377`, `advisor` quiet and nobody beside the gadget four seconds in
(`run5-stock/1-four-seconds-in.png`); the Bounce broken at four seconds, the mechanic's line was heard,
`duration=6498` and `playback=started`. Main's build before the item (the control, `run-control`): `saying
sp_377.mp2` four seconds in, him up, and the same mechanic's line `duration=0`, `playback=refused`. **The stock
file with the record of `0xb9` made fresh** (`parkfile.advisor_forget`; four of four): the post and `attempt=1
message=185 ... response=404 sample=377 line=0` on tick **877**, the original's own tick for that file, the
length 20,664 ms; him up with no hat and no tag (`run5-fresh/1-saying-it.png`); saved under the pause, the
record `(877, 0, 1, 0)`, the message playing 185, the advisor-scoring dword 257, the census's `poll=`, and no
other byte of thing 3 moved; loaded back, the record and the cursor restored and the next poll of it `tick=1032
days=44 result=said-once`. **A park made with no file** (the second player, whose jungle folder is empty; one of
one): `row=185 days=4 score=0 tick=110 result=no-score`, then `days=11 score=110 tick=265 result=post` and the
line, him up (`run5-nofile/1-saying-it.png`). **The Tutorial option off, and the cutoff** (three of three):
`tutorial off` two seconds in, `tick=877 result=tutorial-off`, nobody beside the gadget; no line through the
turns on 1032, 1187 and 1342; `tutorial on`, and `days=64 score=0 tick=1497 result=no-score`, no attempt of 185
in the run (`run5-cutoff/2-past-the-cutoff.png`). **Saved before the line** (one of one): paused on tick 800,
the file's dword 59, the census's.

**The original** (`q316/orig/`, `advisor.py ... 0xb9,0x11a poll`). Its stock park (`run.sh`): the record `(257,
0, 1, 0)` and no slot holding `0xb9` for 85 s; its cursor read 59 on tick 800, as this game's does. The
fresh-record file: said on tick 877 in each of three programs (`run.sh`, `run2.sh`, `run3.sh`; the third
predicted before its run), the wait 19,164 to 20,832 ms, him up at the lower right with his antennae and no hat
(`fresh2-t05.png`), as here. The Tutorial switch held off (`run5.sh`): no line on tick 877 and no slot; switched
on on tick 901, the line on tick 1032. A file of this game's saved on tick 1506 with the record fresh
(`run4.sh`): silent for 85 s, past day 60. **This game's file saved on tick 800 before the line** (`run7.sh`,
predicted before its run): said on tick **877** exactly; with the stale 277 the build before wrote, it would
have come 122 ticks after the save.

| | The original | OpenTPW |
|---|---|---|
| The poll | one polled row a tick, of 155, from the cursor's row | the same |
| The cursor | the program's dword, saved with a park and read back | the same: `ProgramCursor`, the file's advisor-scoring dword |
| The control panel's line | row 185: days 11 to 59 of a park, said once | the same |
| The stock park | never said: its file holds it as said | the same; the row's first turn on tick 877 in both |
| A file with the record fresh | said on tick 877 | said on tick 877 |
| The Tutorial option off | not posted; said a round on once it is on | not posted (the turn after was run past the cutoff) |
| Past day 60 | not said | not said |
| The other polled rows | scored each round | rows 282 and 1 built (below); 152 counted, `PARK_ADVISOR_POLLED_ROW_NOT_BUILT` |
| The line's wait | the response's length and a second | the same; the length differs a run in both, as the gesture chain does |

Not in a game run: a line found by the poll inside another's wait (the test holds it). No case in either game:
the line cut off. Not heard in either game.

## The tips' line (message `0x11a`, Q317)

**Row 282, message `0x11a`:** polled, category 1 (the tutorial messages), either game mode, one copy, first
response 542, one line. Response 542 is sample 559, lip 559, the random gesture chain, model slot 0, no prop, group
383 (no tag): "if you're curious about a button or an icon just point at it with the cursor and check out the tip
that appears at the bottom of the screen". 147 polled rows stand before it in a round. The image holds no
`PUSH 0x11a` (`68 1a 01 00 00`; the same search for `0x11c` finds its three), so nothing posts or withdraws it by
a constant: the poll is its one way in.

**Its score, `0x005a0720`** (in no Ghidra function), is the control panel's listing byte for byte but its three
offsets: whole days since `mGameTick` nought, `JLE` to nought against `[ECX + 0x5e8]`, `JGE` to nought against
`[ECX + 0x5ec]`, else `[ECX + 0x5f0]`: `TutorialExplainTooltips.AfterTime`, `.CutoffTime` and `.Score`
(`lib/advisorschema.py`), 20, 80 and 100 in `Advisor.sam`. **The line is owed from tick 484 to tick 1843 of a
park**, days 21 to 79.

**What the shipped park holds.** `Easymode.TPWI`'s record of `0x11a` is (605, line 0, said 1, slaps 0), and it is
the message its last action names as playing. From the file's cursor, 277, row 282 is the fifth row scored: tick
760, then every 155 ticks. So Instant Action's Lost Kingdom never says it either: it scores 100 and is refused as
said once, and from tick 1845 scores nought.

### OpenTPW (`ParkAdvisorMessages.ExplainTooltips`, a second topic with `Polled` and an `AgeSection`)

A row of `ParkTopics` and nothing else: the poll, the score by the park's age, the gate, the records, the slots
and the action are Q312's to Q316's. `PARK_ADVISOR_POLLED_ROW_NOT_BUILT` was counted for 153 rows a round then, and is for 152 since row 1 was built.

**Predicted first** (`q317/PREDICTION.txt`, `q317/confirm.py stock|control|fresh|nofile|cutoff`; twelve of
twelve, every number written before any build or run). **The stock park** (four of four): `restored message=282
line=0 lastTick=605 said=1 slaps=0`, `poll message=282 row=282 days=32 score=100 threshold=25 tick=760
result=said-once`, row 185's line on tick 877 as before, no `sample=559`, `advisor` quiet and nobody beside the
gadget four seconds in (`run-stock1/1-four-seconds-in.png`), and the gap's count 43 at a pause on tick 799, one a
sweep less the one turn of row 282. **Main's build** (the control): no poll line of 282, and 44 on tick 799.
**The stock file with the record of `0x11a` made fresh** (`parkfile.advisor_forget`; four of four): `result=post`,
`post ... result=queued pending=2` and `attempt=1 message=282 subject=0 score=100 threshold=25 response=542
sample=559 line=0 tick=760 duration=10665` on one tick; three seconds on `saying sp_559.mp2`, `playing=282`, no
tag, and him up with his antennae and no hat (`run-fresh1/1-saying-it.png`); saved under the pause on tick 782,
the record (760, 0, 1, 0), the action's duration 11,665, playing 282, no slot holding it, no other byte of thing
3 moved; loaded back, the record restored and `days=39 ... tick=915 result=said-once`, no second attempt. **A park
made with no file** (one of one): `days=6 score=0 tick=148 result=no-score`, nothing logged on its turns of ticks
303 and 458, then `days=26 score=100 tick=613 result=post` and the line, after the control panel's on tick 265.
**The Tutorial option off, and the cutoff** (two of two, on the private display): after `tutorial off` on tick
772, `tick=915 result=tutorial-off` and nothing through tick 1705; switched on, `days=80 score=0 tick=1845
result=no-score`, the score asked before the gate.

**The original** (`q317/orig/run.sh`, `run2.sh`; `advisor.py ... 0x11a,0xb9 poll`). The fresh-record file, in two
programs: the record (0, -1, 0, 0) and the cursor 277 at the load, tick 755, and **(760, 0, 1, 0) on tick 760**,
as predicted, the cursor then 283; the action's duration 11,665 in one program, this game's own number, and 12,498
in the other. Him up at the lower right with his antennae and no hat 3.3 s after the line began
(`orig/pic-g05.png`). This game's file saved as he said it (`saying-it.TPWS`): the record (760, 0, 1, 0), the
action (114877440, 11665) and the cursor 16 from the load on, tick 782, and unchanged with no slot holding
`0x11a` for 75 s.

| | The original | OpenTPW |
|---|---|---|
| Row 282 | polled; days 21 to 79 of a park; said once | the same |
| The stock park | never said: its file holds it as said on tick 605 | the same; the row's first turn on tick 760 |
| A file with the record fresh | said on tick 760 | said on tick 760 |
| The line's wait | 11,665 and 12,498 ms in two programs | 11,665 ms in each of three runs |
| Past day 79 | not read in the original | no score, said or not |

Not in a game run of the original: the Tutorial switch off at this row's turn, a park past day 79, a park made
with no file. Not heard in either game. The line's end was not seen.

## A shut park people wait outside (message 1, Q319)

**Row 1, message 1:** polled, category 0 (the same message no sooner than 120 quarter-ticks on), either game mode,
one copy, first response 2, three lines in turn. Responses 2 to 4 are samples 1 to 3, the random gesture chain,
model slot 0, prop 9 (the ticket hat), group 3. It is the first polled row of a round: from the shipped park's
cursor, 277, its turns are ticks 768, 923, 1078 and every 155 on.

**Its score, `0x0059c8d0`** (a function made in Ghidra; `ECX` the balance object, `0x00fb3540`):

1. The park open (`FUN_004c9050`, `mParkClosed` nought): nought.
2. `FUN_004ca8e0( 0, 0 )` and `FUN_004ca8e0( 2, 0 )` both under 1: nought. The routine walks the world's thing
   list and counts the things of type 3 whose item's `+0x4ac` (`Info.WhichUIType`) is the kind asked for, 0 a
   ride and 2 a sideshow (for a ride, only at the level asked for or over: the thing's byte `+0x50`, against
   nought here). So a shut park with nothing to ride is told nothing.
3. Otherwise `FUN_004c91c0()` times `[ECX + 0xa4]`, `OpenPark.ScorePerWaitingPerson` (`lib/advisorschema.py`), 20
   in `Advisor.sam`. `FUN_004c91c0` takes the two ticket booths' cells (`FUN_004d8610( 0 )` and `( 1 )`), walks
   each cell's own list of things (the cell's `+0x24`, then each thing's `+0xa`) and counts those of type 1 whose
   `mState` (`+0x220`) is 3, waiting for the opening; it leaves the count in the analyser's `+0x1f0d4` and sets
   the byte after it. A guest who has paid and one who has not are both in state 3 there.

`MinScoreForConsideration` is 25, so **one guest waiting is not enough and two are**. The tag its line leaves is
row 3 of the tag table: key 4 (it takes the place of row 2's tag, "close your park"), picture 2, string 7 ("People
want to come in but your park is closed. You should think about opening up."), the bar's first button of type
`0x18`, no second, thirty seconds.

**Row 2, message 2, `ClosePark`** (`0x0059c960`, read and not built): with the park open and neither a ride nor a
sideshow standing, `[ECX + 0xa8]`, `ClosePark.Score`, 99; responses 5 to 7, group 4.

**Rows 259 to 263 score in a shut park too, and are not built** (functions made in Ghidra, read from their
listings alone): row 259 (`0x005a02f0`) wants the park open, at least `[ECX + 0x57c]` rides, and more than
`[ECX + 0x580]` per cent of them answered for by `FUN_004ca9b0( 0 )`; rows 260 and 261 (`0x005a0370`,
`0x005a03d0`) the same share of the things of kind 1 (shops) and of kind 2 (sideshows), with no test of the door;
rows 262 and 263 (`0x005a0430`, `0x005a0490`) a share of two other counts (`FUN_004c9ee0` and `FUN_004c9f90`,
`FUN_004ca030` and `FUN_004ca0c0`, not read). What `FUN_004ca9b0` counts was not read; the door shuts every ride,
shop and sideshow, and in the original's shut park rows 261 and 262 were posted at 70 and 75 on ticks 900 and 901
and a line was begun on tick 899, row 260's turn (below).

### OpenTPW (`ParkAdvisorMessages.OpenPark`, `ParkFacts`, a topic's `PerWaitingGuest`; `ParkPeople.WaitingToOpen`, `AdvisorFacts`; tag row 3; `ParkGadget.BarButton`'s type `0x18`)

A third polled topic, whose score asks the park: `ParkPeople` hands each sweep's `Tick` its `AdvisorFacts`, the
door, the things standing of a ride's item and of a sideshow's (`ParkState.ObjectsInChainOrder`, the catalogue's
`UiType`), and `WaitingToOpen`, the guests in `PeepState.WaitingForOpening` whose position's cell is either
booth's. The console's `gate` ends with `waiting N`. `ParkTags.Rows` lists row 3. **The bar's button of type
`0x18` is built** (`hud.md`, "The message bar"): it opens the entry-price screen and takes its tag off as the
erase button does.

**The original first** (`q319/PREDICTION.txt`; `q319/orig/run.sh`, `original/waiting.py` beside `advisor.py`,
`tags.py` and `bar.py`). This game's file of the stock park with its door shut on the entry-price screen on tick
787 and saved on tick 803 (`q319/scene1/shut.TPWS`): on **tick 923** the analyser's word went from -999 to 4, the
live count of state 3 on the booths' cells read 4, and a slot took message 1 at **80**. It was **said on tick
942**, not 923: the action's start had moved on tick 899 with a length of 10,832 ms, row 260's turn, and that
wait ended as tick 942 began. The record then read (942, 0, 1, 0), one tag stood at (84, 909, 154, 974) with the
mast at (94, 901, 144, 984) and string 7 word for word, and he was up in the green ticket cap
(`orig/a-told.png`). The tag clicked: the bar's data read thing 0, buttons 24 and 0, item 0 and the text, the
handle 331 to 1151 (`orig/a-bar.png`). The bar's first button clicked: a slot took `0x12f` at 1000 (the
entry-price screen's own line), the screen is in the frame (`orig/a-button.png`), the handle went back to 331 and
no tag was held 0.53 s on. **The first file twice more** (`orig/c-load.log`, `d-load.log`; my first prediction had missed the count, 5 or
6 for 4, and the tick, so both were made again before each run): the count 4 and the same four guests each time,
rows 261 and 262 on ticks 900 and 901 each time, and the record said on **tick 939** in both, where the line
begun on tick 899 read 9,998 ms long and round 1's 10,832 (a line's length is drawn). A fixed tick, 940 to 944,
missed by one; the relation held on the third run: the tick is 899 and the line's length over 252 ms a tick, and
the new action began 10,066 ms after the old. **The same scene saved past row 262's turn** (this game's file saved on tick 906,
`q319/run-fix1/shut.TPWS`; the original meets none of rows 260 to 262 before row 1): the record went from
(0, -1, 0, 0) to **(923, 0, 1, 0) on tick 923**, the analyser's word to 3 and the live count 3 (guests 29, 35
and 42), the tag on the same poll line.

**This game** (`q319/confirm.py fix|control`; five of six by the script on the first run, the sixth its own
fault: it read `advisor`'s last line where `wearing=9,0` is on the line before; six of six with the same numbers
on the final code, `run-fix2`; `side-by-side.png` holds the original's three frames over this game's). The stock park, the door shut on tick 785: the
poll of tick 768 read `score=0 ... result=no-score`; saved under a pause on tick 906 with `waiting 3`; then `poll
message=1 row=1 days=40 score=60 threshold=25 tick=923 result=post`, `post ... result=queued` and `attempt=1
message=1 subject=0 score=60 threshold=25 response=2 sample=1 line=0 tick=923` on one tick, `saying sp_001.mp2
... wearing=9,0`, him up in the green cap (`run-fix1/1-told.png`), `tags` reading `tag=3/picture=2/subject=0`
with `mast=94,901,144,984` and `tag900=84,909,154,974/frame=2`. The tag clicked (`click 56 441`): `message bar:
tag 3 subject 0 item 0 bar: out=1 rect=486,1112,1101,1453 ... first=b_entry/help=236 second=hidden
erase=mb_erase` and the text (`2-bar.png`). The button clicked (`click 265 549`): `button type 24 opens the
entry-price screen and takes tag 3 off`, `windows` listing `ParkEntryPriceScreen`, `standing=0` and `out=0` 0.7 s
on, no `TAG_BAR_BUTTON` counted (`3-screen.png`). **Main's build** (one of one, on the private display): no poll
line of message 1, no attempt, no tag at tick 941.

| | The original | OpenTPW |
|---|---|---|
| Row 1 | polled; nought with the park open or nothing to ride; 20 a guest waiting at the booths | the same |
| The same file, three waiting | queued and said on tick 923 | the same |
| The tag and the bar | row 3, part 2, string 7; button `0x18` opens the entry-price screen, tag off, arm in | the same |
| A shut park's other lines | rows 260 to 262 score, and a line of row 260's delayed row 1's by 19 ticks | not built: counted with the other polled rows, so row 1's line is said at once |
| Row 2, `ClosePark` | 99 in an open park with no ride and no sideshow | not built, counted |
| The count's place | left in the analyser's `+0x1f0d4` | not kept |

**Not built or not seen.** Rows 2 and 259 to 263. Lines 3 and 4 (samples 2 and 3) were in no game run, nor the
refusal inside 480 ticks, a second load of guests (whether a bus comes to a shut park was in no run),
a park opened while he says it (whether anything withdraws message 1 was not looked for), a tag 3 taking the place of a tag 4, or Full Simulation's empty park. The entry-price screen's own
line, `0x12f`, is not posted here (`hud.md`). Not heard in either game.
