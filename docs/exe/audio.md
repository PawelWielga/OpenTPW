# exe/audio — positional audio

The original really had 3D sound. It delay-loads **QMixer.dll** (QSound's mixer, shipped in the install, dated Jan 2000) and resolves 27 named entry points, including `SetSourcePosition`, `SetListenerPosition`, `SetListenerOrientation`, `SetListenerVelocity`, `SetSourceCone`, `SetDistanceMapping`, `SetSpeedOfSound`, `SetSpeakerPlacement`, `SetPanRate`, `SetFrequency` and `GetSourcePosition`. Every sound in the game goes through one entry point, `Sound_PlayEffect(handle, category, effect, x, y, z)`, and a 2D sound is played at `(0,0,0)` — **but that does not mean a 2D sound is merely a 3D source sitting at the origin, and reading it that way is a trap.** The engine has a separate, genuinely listener-*independent* path (`QSWaveMixSetPosition`) chosen per voice by a flag whose writer was never found; see "The listener, and what a pause does to it". Positions come from `.MD2` nodes that the id table marks as emitters with the flag word `0x211`. The lobby used none of it: everything there played at `(0,0,0)`.

## QMixer

The import-by-name table is at `00725990`–`00725c52` (u16 hint + name, terminated by the string `QMIXER.dll`).

The delay-load IAT *can* be found, but not by searching for a string or a VA: the tables hold **RVAs of the `IMAGE_IMPORT_BY_NAME` records**, so search for the dword `nameHintAddress - 0x400000`. `SetDistanceMapping`'s record is at `00725bc0`, so a byte search for `c0 5b 32 00` finds it in **two** places — the delay-load signature, because the INT and the delay IAT start out identical. The pair sits at `006fd294`–`006fd2fc` and `007243ac`–`00724414`, 27 entries each, zero-terminated at both ends. `006fd2xx` is the live IAT: it has READ xrefs, the other has none. Ghidra leaves that whole region undefined as functions, which is why xrefs look empty until the records are defined.

**Deliberately not resolved:** every `QSWaveMixSetReverb*`, plus `SetRoomSize`, `SetPolarPosition`, `SetSourceVelocity`, `SetListenerRolloff`, `SetSourceCone2`. Reverb came from **EAX** instead — `data\sound.sam` carries `SoundInfo.QSOUND 15` and `SoundInfo.EAX 10`, and `SETREVERB` is a ride-script opcode, with `SndReverb.map` as its settings file.

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `00725990`–`00725c52` | — | QMixer import-by-name table (u16 hint + name), terminated by `QMIXER.dll` | Read from the exe |
| `00725bc0` | `SetDistanceMapping` | That import's `IMAGE_IMPORT_BY_NAME` record; its RVA is the search key | Byte search `c0 5b 32 00` hits exactly the INT and the delay IAT |
| `006fd294`–`006fd2fc` | — | The **live** delay IAT, 27 slots, zero-terminated | Only this copy has READ xrefs |
| `007243ac`–`00724414` | — | The INT, identical twin of the delay IAT at load time | Delay-load layout |
| `006fd2a4` | — | `SetDistanceMapping`'s IAT slot; called exactly once | `get_xrefs_to` on the slot |
| `006d2963` | — | The one call site of that slot | xref from `006fd2a4` |
| `006d2930` | — | Thin wrapper around it: builds a 16-byte struct `{cbSize=0x10, a, b, c}` and copies the three values straight from its caller, so the real parameters live one level further up | Decompiled |
| `00711980` | — | The only place the wrapper's address appears: slot `+0x60` of the QMixer wrapper vtable at `0x00711920` (see below), so the caller is virtual-dispatched | Address search |
| `007655d8` | — | Ride-script keyword table; holds `SETREVERB` alongside `SPARK`, `SCREAMLEVEL`, `SETLIGHT`, `WALKON` | Read from the exe |

`SndReverb.map` is 820 bytes and decoded to its last byte: the loader `0x006c5d20` reads a dword that must be 2000, a slot count (32), then per slot one byte (0 empty, the slot's first word set to -1) and, when it is non-zero, 13 dwords (52 bytes) into a `count x 0x34` array at `+0x68`; 8 + 32 + 15 x 52 = 820, slots 0 to 14 present. The layout is FileFormats' "Reverb presets (SndReverb.map)" page; what the 13 values are is not decoded (fork watch 2026-10-16, `verify-audio-fx.md` L7).

## The single entry point

    Sound_PlayEffect( handle, category, effectId, x, y, z )

The three ints become floats in a stack record `{vtable 00700b90, category, effect, x, y, z}`, virtual-called through `DAT_00802bcc`'s vtable+8. `DAT_00802bcc` is not the manager: it is an 8-byte forwarder (vptr `0x0070a288`, the manager at `+4`) whose `+8` (`0x006b5b40`) calls the manager's `+0x10`, `0x006b87d0`, which is the real play. See "How the engine plays an effect" below.

**`Sound_PlayEffect` has no 2D form: a caller that wants a flat sound passes (0,0,0).** `UI_PlaySound` is literally `Sound_PlayEffect(0, uiCat, id, 0, 0, 0)`. The engine gives such a voice no place against the listener and no distance mapping when its effect is flagged `0x200`, as every effect of `cat_uiSFX.map` is: it takes the `QSWaveMixSetPosition` path, and what the mixer makes of that path is not read (see "The listener, and what a pause does to it"). OpenTPW copies the shape: one call, optional position (`Audio.Play`, null for flat).

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0051bfc0` | `Sound_PlayEffect` | The single entry point for every sound, positional or not | Decompiled; all callers below reach audio through it |
| `00700b90` | — | vtable stored in the stack record it builds | Decompiled |
| `DAT_00802bcc` | — | An 8-byte forwarder to the sound manager, written through a pointer (`0x0051b6a6 PUSH 0x802bcc`, `0x006b5de6`), which is why it shows only reads | Disassembly |
| `00485aa0` | `UI_PlaySound` | 2D wrapper: `Sound_PlayEffect(0, uiCat, id, 0, 0, 0)` | Decompiled |

## How the engine plays an effect: priority, not a repeat delay

Decoded 2026-09-23 for `docs/QUEUE.md` Q9. Five decoders each had their claims checked by two refuters (workflow `wf_99a15f42-84d`). `0x006bc2d0`, `0x006c3e00` and `0x006c0676` were then re-read by hand, and the layout was re-measured over all 31 shipped `cat_*SFX.map`. The file layout itself is in the FileFormats docs clone, `formats/sound-categories.md`.

**The engine keeps no time per effect.** No part of the path from `Sound_PlayEffect` down to a voice reads a clock or writes one into an effect record or a category:
- the play entry `0x006b87d0` does not;
- neither does the device gate `0x006b88e9` → `0x006b9bd0`, which is `MOV EAX,1; RET 8` on both device vtables;
- neither does anything else before the voice is made.

A second caller of the same category and effect is always let through and gets a new voice with a new handle. **So a stop cannot change what another caller hears, except by freeing capacity** (see the pool below).

**The effect record's fourth int is a priority.** OpenTPW's `SoundCategoryFile` reads it as `RepeatDelay`: 2700 for every kids scream, 10000 for music, 6000 for speech. It has two readers:
- **Voice creation.** It is copied into the voice (`0x006bb9f9 MOV DX,[EAX+0xc]`, `0x006bb9fd MOV [ECX+0x22],DX`) as the high word of the sort key at `voice+0x20`. The low word is closeness to the listener (`0x006bba6c`).
- **The play entry.** When the caller passes in a live handle, the new effect replaces that handle's voice only if its priority is strictly higher (`0x006b88d3`..`0x006b88da`). STARTSCREAM and SINGLESCREAM pass handle 0 (`0x00551165`), so this never applies to them.

Every use of that int as a delay in OpenTPW is OpenTPW's own: `SoundCategory.Play`'s throttle, and every replay it times (the lobby's beds and one-shots, the park's weather, and SINGLESCREAM). These are filed as Q43.

**The voice class comes from the record's flags word**, the `u16` at `+0x10` (`0x006b6774`..`0x006b67f2`):

| Flags word | Voice class | Shipped users |
|---|---|---|
| no bit `0x4` (for example 0, `0x200`, `0x8`) | One-shot: plays one sample and dies; with bit `0x8` the sample is looped and the voice lives on ("A voice's range", below). Freed at start + length + 250 ms (`0x006bc01b`), or when its channel ends. | 1,067 of the 1,267 records, SINGLESCREAM's 75-90 and 105-109 among them; 30 more carry bit `0x1` and go through a deferred queue first (`0x006b66d0`) |
| `0x0404` | Held chain (`Voice_MakeHeldChain` `0x006bdd30`), final vtable `0x0070a2e0`. The constructor stores `0x0070a390`, then overwrites it at `0x006bdde5`. | 18 records: kids 71-74, staff 188, and the ambient beds with `0x120404`/`0x170404` |
| `0x4` with neither `0x2` nor `0x400` | The chain of any variation (`Voice_MakeAnyChain` `0x006bdc60`): the held chain's base class `0x0070a390` kept, whose children draw their variation among all of the effect's ("A script's sounds", below) | 17 records, each of one variation: Lost Kingdom's rides 89, 100, 147, 170 among them |
| bits `0x4` and `0x2` | Four constructors by the other bits (`0x006b677d`..`0x006b67c3`): with `0x10`, `0x006be680`; else with `0x400`, `FUN_006be610`, **the music's class**, below; else with `0x100`, `0x006be330`; else `0x006be090`, which draws its variation from all of the effect's and follows each later sample with the variation's wait ("The plain class: any variation, and a silence after a sample", below). The `0x10` and `0x100` ones are not decoded. | 135 records (`classes.py`): 106 to `0x006be090` (`0x0006`, `0x0206`), 27 to `FUN_006be610` (`0x0406` 16, `0x0606` 11: music 2 in every theme, kids 91, global ambient 33, jungle's rides 204, 216 and 217 among them), 2 to `0x006be680` (`0x0016`), none to `0x006be330` |

**The music's class: the parameter picks the variation, and nothing else** (`FUN_006be610`, constructor `FUN_006be660`, vtable `0x0070a498`; fix 4 of the 2026-10-06 review). The voice plays one sample after another. Its slot `+0x38` (`FUN_006be5e0`) reads its first controller's value, the byte at `[[voice+0x4c]+4]`, and hands it to `FUN_006be450`, which is the chain's rule (`0x006c3e00`) again:

- with no variation yet, the effect's first one, whatever the parameter, if it holds samples (`FUN_006bd7a0`);
- otherwise the current variation's zone records (count at `+4`, 8-byte records at `[+0x26]`: a variation pointer, then the range in bytes `+6` and `+7`) whose range holds the parameter. Their targets' weights (`+0x1e`) are summed, the sound library's own generator steps (`[0x00fb1f20]` × `0x19660d` + `0x3c6ef35f`, rotated right 13) and its value modulo the sum (`0x006be4fa`) picks one: the walk adds each in-range weight and stops at the first running total not below the draw (`0x006be52d`), so the first candidate has one draw in the sum more than its weight and the last one fewer;
- with no record in range, the controller's value is made `0x7f` and the voice's flag `0x40` set (`0x006be55e`, `0x006be565`): it is parked, and the next set of a controller to anything but `0x7f` (slot `+0x28`, `0x006be580`) picks a variation and a sample (`FUN_006bc680`) and clears the flag.

Nothing on this path writes a volume. A sample's volume comes from its variation's two bytes at `+0x0c`/`+0x0d`, by a controller only where the variation names one (`FUN_006bc090`). Music 2 names none and holds 100 and 100 in every variation of all four themes, so the crowd's level changes which variation plays and never how loud. Measured in the original, and what OpenTPW does with it: `park-engine.md`, "The music's level".

**A held voice is a chain.** The parent of a `0x0404` voice never plays a sample.
- **Its tick** (vtable `+0x14`, `0x006bdae0`) only extends and prunes a chain of one-shot children (`0x006bdb50`, `0x006bdc10`).
- **When a child is made.** Each pass, if the clock has passed the newest child's time (`0x006bdb88 CMP EBP,[ESI+0x1c]; JBE`), a new child is made. It is a 0x50-byte voice of class `0x0070a9e0` (`0x006bdbba` → `0x006b72f0` → `0x006c3ded`).
- **What a child gets.** `0x006c3a80` gives it:
  - a variation (`0x006c3e00`);
  - a fresh weighted sample (`0x006bc680`);
  - as it starts, a volume and a pitch of its own ("A scream's child and its pitch", below);
  - its time: now plus a wait drawn from that variation's header (`0x006c3abe`..`0x006c3ac5`).
- **How the wait is drawn.** The wait is the header's `u16` pair at `+0x10`/`+0x12` (`0x006bc2d0`):
  - nought when both are nought;
  - otherwise the pair is swapped if the second is the smaller, then `min + LCG % (max - min)`, which is min when they are equal.
  - Bit 4 of the header's `+0x18` takes the wait from the voice's parameter instead (`0x006c3d80`). No scream variation sets it.
  - It is counted from the child's start, and the sample's length does not come into it.
  - Shipped for the four screams: 71 1000-3000 ms, 72 500-2000, 73 100-1000, 74 0-500.
- **Which variation it plays.** The first child takes the effect's first variation, whatever the parameter (`0x006c3e26`..`0x006c3e33`): with no child yet, `HeldChain_HeadOrFirstChild` (`0x006bdb10`) noughts the chain's own variation, which its set-up drew among all (`0x006bbf60`), where the effect has a key (`+0x12`) and the voice's flag `0x20` is clear, and sets `0x20`. All 18 shipped records have a key (6, 18, 19 or 23); a chain put back from a park file keeps its saved child and never comes there first ("A script's sounds", below). Every later child draws, weighted by each target's `+0x1e`, among the current variation's zone records whose `lo..hi` holds the voice's parameter byte (`0x006c3e1c`, `0x006c3e89`..`0x006c3e97`). No match parks the chain until a new parameter arrives (`0x006c3f49`).
- **Where the parameter comes from.** For the screams it is parameter 6. STARTSCREAM sets it to `clamp((operand + speed) / 2, 0, 100)` straight after the play (`0x00551261`..`0x00551265`), and SCREAMLEVEL resets it. The zones send 0-25 to variation 1, 26-50 to 2, 51-75 to 3 and 76-100 to 4, so Lost Kingdom's Belly Bounce, at `(20 + 50) / 2 = 35`, screams variation 1 once and variation 2 ever after.
- **Pruning.** A child is pruned when the clock reaches its time (`0x006bdc23`). The prune deletes it without stopping its channel (`0x006b71f0` → `0x006bb9b0`), so its sample most likely plays out over the next one's start. Whether the device reclaims that channel early is **not traced**.
- **The clock.** It is wall time in milliseconds (`0x005f5fa0`: QueryPerformanceCounter, or timeGetTime), read once per service pass (`0x006b62c1`). It does not pause with the game.
- **The service.** The service runs whenever `Sound_ApplyGroupVolumes` posts message `0x700b6c` (`0x0051bfab`). How often that is has **not been measured**. A child made in one pass starts on the next (`0x006bdb79`).

**A stop takes down one chain.**
- `Sound_Stop` and `Sound_StopFading` find the voice by its exact handle (`0x006b6830`, generation-checked at `0x006b6876`).
- A held parent's stop (`0x006bd9b0`) hard-stops each child still in its chain, down to `QSWaveMixStopChannel` (`0x006d24d7`). It deletes the children, clears the chain and flags itself finished (`0x006bcade`).
- With fading on ("The fade", below), StopFading still ends in the same hard stop. The parent has no channel, so its first fade step takes the no-channel exit (`0x006bcc71`), and the next service stops it (`0x006b62fe` → `0x006b6320`).
- Nothing on any stop or free path writes an effect record, a category or a manager gate. STOPSCREAM zeroes the script's `+0xd0` itself (`0x00555f0a`).

**Where two callers of one effect do meet: the pool.**
- Each service keeps only the top N voices by that `(priority << 16) | closeness` key and ticks only those (`0x006b6358`, `0x006b637b`). N is 12 by default (`0x006b83b0`) and is reset from the options, up to 30 (`0x006b6140`).
- The hardware channels are taken by the same key (`0x006bad28`).
- A held chain that falls out of the top N makes no children, and a stop elsewhere can let it back in. Not built here (Q43).

**Two rides screaming the same band, therefore:**
- two parents, two handles, each chain starting at once and going on its own random clock;
- one ride's STOPSCREAM cuts that ride's newest child and changes nothing about the other.

OpenTPW's `ParkScreams` builds exactly this: `ParkAudio.StopScream` releases nothing, and `SoundCategory.PickFrom` picks a child's sample with no gate.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0070a288` | — | vtable of the 8-byte forwarder at `DAT_00802bcc`: `+8` play → manager `+0x10` | Disassembly (`0x006b5dd6`, `0x006b5b4f`) |
| `0x0070a2a0` | — | The sound manager's vtable: `+8` `0x006b8720` registers a category (→ `0x006bf330`), `+0x10` `0x006b87d0` plays, `+0x18` `0x006b89b0` stops a handle | Read from the image |
| `0x0051b530` | — | The game's side of registering a category: a `cat_*` name to a handle through the manager's vtable `+4` (`DAT_00802bcc`); 0 when the sound system is down (`DAT_00802bc8`/`DAT_00802bd4`) or `FUN_0041e8e0` does not answer 1. Called by `Sound_RegisterGlobalCategories`, `FUN_0051ec50`, `FUN_0051e890` and `FUN_0051e8f0` | Decompiled |
| `0x006bf330` | — | Registers a category: looks the name up and loads `<name>BANK.map` and `<name>SFX.map` into a 0x34-byte category. It does not play | Disassembly (`0x006bf3c2`, `0x006bf520`) |
| `0x006b87d0` | — | Play. It reads no clock | Disassembly |
| `0x006bb9f9` | — | The effect record's `+0xc` copied into the voice's priority | Disassembly |
| `0x006b88d7` | — | The only other read of `+0xc`: replace a live handle only for a higher priority | Disassembly |
| `0x006bdae0` | — | A held voice's tick: extend the chain, prune it | Disassembly |
| `0x006bdb88` | — | Make a child once the clock passes the newest child's time | Disassembly |
| `0x006c3a80` | — | A child's variation, sample and time | Disassembly |
| `0x006bc2d0` | — | A variation's wait: `min + LCG % (max - min)` | Disassembly, re-read by hand |
| `0x006c3e00` | — | A child's variation: the first variation first, then by the zones and the parameter | Disassembly, re-read by hand |
| `0x006bd9b0` | — | A held voice's stop: its own children only | Disassembly |
| `0x005f5fa0` | — | The sound clock: wall-time milliseconds | Disassembly |
| `0x00fb1f20` | — | The one random seed every draw above advances | Disassembly |
| `0x006b7e76` | — | Faults (a read through NULL) when no audio device exists: with Wine's pulse and ALSA drivers both disabled, mmdevapi finds no driver and the game crashes right after its DirectDraw set-up | Proton log, `EXCEPTION_ACCESS_VIOLATION` at this address |

## A voice's range, and the rectangle it follows the listener in

Read for Q263 (the land's own sounds, `park.md`) and held against the running original's voices
(`original/voices.py`).

**The play record carries a range.** The record `Sound_PlayEffect` builds is `{vtable 00700b90, category, effect,
x, y, z, range}`; it stores nought for the range, and `FUN_0051c130`, whose one caller is the level's placed
objects, stores its seventh argument. `FUN_006bbe90` makes the voice's flags from the effect record's flags word
and keeps a range that is not nought at the voice's `+0x40`:

| Effect flags (`+0x10`) | Voice flags (`+0x30`) | What it does |
|---|---|---|
| `0x8` | `0x6` | Bit `0x4`: the voice's channel is given 9999 (`FUN_006c5110`, `0x006bc410`'s end), its loop count. Measured: global ambient 8's voice held its channel through 14 s on a 1.8 s sample |
| `0x4` | `0x2` | |
| `0x200` | `0x8` | No range and no place: the service never silences it, and its place is written as the whole words 0, 1, 0 (`0x006bc51d`, `FUN_006bb220`; the slots are floats, so its y is the bit pattern 1, 1.4e-45, and not 1.0), with bit `0x1` at `params + 0xc`; it reaches `QSWaveMixSetPosition` (`0x006d272b`), where a placed voice, with bit `0x20` there, reaches `QSWaveMixSetSourcePosition` |
| `0x20` | `0x200` | |
| `0x4000` | `0x20000` | |
| a range given | `0x80` | The range at `+0x40` is the voice's own |

**Each service pass silences what is out of range** (`FUN_006bca10`, from `FUN_006b62b0`'s walk of the voice
list). The range is `FUN_006bc650`: the voice's own times the manager's float at `+0x2c` (1.0 in the running
game) with flag `0x80`, else the float at `+0x22` of the voice's variation, else nought. The distance is
`FUN_006bb9f0`'s, over x and z alone, from the listener at `[manager + 0x60]`. Past the range, and without
voice flag `0x8`, the voice is flagged `0x800` and its channel given up (`FUN_006bca70`); inside it the flag is
cleared and the voice plays on. Measured: a voice in range reads `0x00087`, one out of it `0x00886`.

**A voice given no range takes its variation's** (`FUN_006bc650`'s second arm: the voice's `+8` is its variation's
header, kept in memory as the file lays it out, and the float is at `+0x22`). Counted over all 1,595 variations in
the 31 category files (`lib/sfxmap.py`, each file walked to its last byte): 100 in 738, 95 in 34 (every scream of
`cat_kids` 71 to 90, and two of fantasy's ride variations), 90 in 80, 60 to 85 in 51 (the staff's are 60, 70, 75, 85, 90 and one 100), 10 to 50 in 13,
150 and 170 in four of the global rides', 1500 in one (the global ambient's thunder, 32, which the game sounds
from a bolt's top), 0.1 in 29 and nought in the 645 speech variations. All but two of the last 674 belong to effects flagged
`0x200`, whose voices are given no mapping at all. `FUN_006bc410` hands the same number to the mixer as the
mapping's far end, so every placed voice is turned down by the law of the next section over its own variation's
range. **Held in the running original** (Q265, `original/mixer.py` over 150 s of the shipped park with the view on
the Belly Bounce, `q265/orig/a-load.log`; predicted first, 2 of 2): the ride's scream channels read a mapping of
2.0, 95.0 and 0.80 at (525, 250, 0), and on all 206 lines off the listener the gain was the law's (0.5653 at
49.41); the staff's read 60, 75 and 90, others 80 and 100, and no channel read a number the files do not hold. A
scream's channel volume read 0.2756, 35 of 127: its variation's 47 times the sound effects' 75 over a hundred
("A channel's volume", below).
**Not read:** the channels that stood at the listener's own place with a mapping of 2.0, 95.0, 0.80 and a volume of
10 of 127 (a distance of nought, so at full gain): which effect they are.

**A voice may be given a rectangle** (`FUN_0051c5d0` → `FUN_006b64b0`): four whole numbers at
`[voice + 0x4c] + 8`, x, z, a second x and a second z, and voice flag `0x1000`. With it `FUN_006bca10` first
moves the voice (`FUN_006bd5f0`): to the listener's x held between the two x, the listener's own height, and
the listener's z held between the second z and the first. The voice stays as near the listener as the
rectangle lets it. Measured: the sea's at (the listener's x, its height, 21), the gulls' at (450, its height, 31).

**The distance mapping has a writer** (`FUN_006bc410`, the voice's channel set-up, for a voice without flag
`0x8`): request bit `0x200` at `params + 0x14`, and the three numbers at `+0x38`: 2.0, the voice's range (2.1
when the range is 2.0 or under), and the manager's slot `+0x2c` (`0x006b9bb0`): 0.2 with bit `0x40` of the
manager's `+0x28`, else 0.8. The running game read `0x508` there, so 0.8. These reach
`QSWaveMixSetDistanceMapping` through `0x006c581b`. In `QMixer.dll` (image base `0x18000000`) that export, at
`0x18003b10`, packs them into a command of type `0xc` (`0x1800c870`; a record of 12 bytes or under takes a
scale of 1.0) and posts it to the channel (`0x18001520`). What the mixer does with them is the next section.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0051c130` | — | `Sound_PlayEffect` with a range | Decompiled |
| `0x006bbe90` | — | A voice's flags from its effect's, and its own range | Decompiled |
| `0x006bca10` | — | A service pass on one voice: the rectangle's move, then in or out of range | Decompiled |
| `0x006bc650` | — | A voice's range | Decompiled |
| `0x006bb9f0` | — | The squared distance to the listener over x and z, and the sort key's low word | Disassembly |
| `0x006bca70` | — | Out of range: flag `0x800`, the channel given up | Decompiled |
| `0x0051c5d0`, `0x006b64b0` | — | A voice's rectangle and flag `0x1000` | Decompiled, disassembly |
| `0x006bd5f0` | — | The voice put at the listener's place inside its rectangle | Decompiled |
| `0x006bc410` | — | A voice's channel set-up: the distance mapping's three numbers and its request bit, the loop count | Decompiled |
| `0x006b9bb0` | — | The mapping's third number: 0.2 or 0.8 | Disassembly; `0x0070a624`, `0x0070a628` read |
| `[[0x00802bcc] + 4] + 0x20` | — | The voice container: its list's head at `+0x24`, a voice's next at `+0x34` (`FUN_006b62b0`) | Decompiled; walked in the running game |

## The mixer's distance law

Read for Q264 in `QMixer.dll` itself, imported into the Ghidra project as `/QMixer.dll` at Alexah's word
(2026-10-10; image base `0x18000000`, 1,277 functions; the names below are this project's), and held against
the running original's channels (`original/mixer.py`).

**A channel is turned down by its distance in a straight line, raised to the mapping's third number.** The
mapping's command, type `0xc`, is applied by the channel's command switch (`0x18007a20`): it stores min, max and
scale at channel `+0x1e4`, `+0x1e8` and `+0x1ec` and marks the channel's gains stale. They are worked out again
in `0x1800a980`, which reads the channel's parameter block at `+0x1a4`:

- The distance (`+0x24c`) is the length of the listener's place (session `+0x58`) to the channel's
  (`+0x1b4`), **over all three axes**. The game's own service silences a voice by x and z alone (above), so a
  voice the service counts in range can still be past the range for the mixer, the listener's height being part
  of its distance.
- The channel's flags word (`+0x200`, what `QSWaveMixEnableChannel`'s last argument is stored as) picks the arm.
  **With `0x1000`: nothing past max; all of it at min or nearer, or with a scale of nought; between them
  `((max - d) / (max - min)) ^ scale`** (a scale of 1.0 skips the power; the power is `2 ^ (scale x log2)`,
  `0x1800de70`). Without `0x1000` the arm is the inverse one, `min / (min + scale x (d - min))`
  (`0x1800cfd0`), nothing past max unless `0x800` holds it at max's gain.
- **The game sets `0x1000` on every channel it enables**: its flag maker `0x006d22d0` ends in `OR AH,0x10`,
  whatever it was handed, and its one caller `0x006d2320` (the wrapper's slot `+0x68`) passes the result to
  `QSWaveMixEnableChannel`. So a placed voice's law is the linear arm: with the game's 2.0, its range and 0.8,
  `((range - d) / (range - 2)) ^ 0.8`.
- The channel's final gain (`+0x260`) is its volume (`+0x1f0`) times the distance's gain (`+0x25c`) times the
  cone's (`+0x258`). The volume is `QSWaveMixSetVolume`'s number over 32,767 (command type 2).

**Measured in the original, 2026-10-10** (`original/mixer.py`, new: the session found in the DLL's own memory,
every open channel a line; four of Q263's camera files loaded, `q264/orig/mixer-*.txt`). Predicted first
(`q264/PREDICTION.txt`), 4 of 4: both placed channels read flags `0x1111` and a mapping of 2.0, 97.0 and 0.80;
the gain beside each distance was the law's to four places:

| Channel | The mixer's distance | Its gain | The law |
|---|---|---|---|
| the fall, listener (552, 52, 538) | 61.70 | 0.4529 | 0.4529 |
| the fall, listener (547, 57, 589) | 96.78 | 0.0077 | 0.0077 |
| the river, the same listener | 100.51 | 0.0000 | 0 |
| the river, listener (537, 57, 682) | 65.06 | 0.4181 | 0.4181 |
| the fall, listener (552, 48, 464) | 71.77 | 0.3462 | 0.3462 |

The first row's distance is the three-axis one: the service read the fall 34 off over x and z, and
`sqrt( 22^2 + 25^2 + 52^2 )` is 61.75. At the second listener the service had both voices in range (78 and 83
off) and the mixer played next to nothing of the fall and nothing of the river. The game hands the mixer a
place as (x, z, height), and the mixer keeps the third negated (`0x18001020`).

**Not predicted, found: a channel's volume is a whole number of 127.** The fall's read 0.5905, 75 of 127; the
river's 0.2992, 38; the music's 0.4724, 60. Those are each variation's volume (100, 50 and 100) times its
group's level, the options' defaults of 75 for sound effects and 60 for music (`sound.sam`, `DefaultVolume`),
rounded. The code that makes the number is read in "A channel's volume", below. A paused park's channel reads a distance of 10,000.32 and a gain of nought: the
pause's lift (below) is past any range.

**Not read:** how the mixer applies the final gain to a sample (taken as a plain factor); what the pan and
QSound's own processing make of a place (flag `0x1`); the two channels that read a place of (0, 0, 0), a
mapping of 2.0, 100.0 and 0.8 and a distance of 50.00, stale as they were read; the 2D path's distance
(`QSWaveMixSetPosition`, command type 7, a range given as a number).

**What OpenTPW does.** `AudioListener.RangeGain` is the linear arm, and a voice played with a range of its own
(`Audio.Play`'s `range`) is turned down by it, the distance over three axes. A park under the orbit camera is
heard from the midpoint of the eye and the point it looks at (`CameraMode.Ears`), as below. The land's own
sounds are given their record's range (`park.md`, "The land's own sounds"), and every other placed voice of a
park its variation's (`SoundCategory.RangeOf`, Q265): a ride's screams and single screams, a guest put off, a
yawn, the staff's voices, thunder and a bumper ride's engine. A voice out of range plays on unheard, where the
original gives its channel up, and a voice is still panned by a plain left and right. The lobby's placed voices
are OpenTPW's own and take no range.

**Measured here, 2026-10-10** (`q265/confirm.py`, predicted first in `q265/PREDICTION.txt`; fix 4 of 4 on the
first run, control 2 of 2). With the camera on the Belly Bounce `voices` read seven scream samples at range 95,
34.09 off, each at a gain of 0.7128, the law's; 800 units off, seventeen ranged voices (screams, staff, the fall
and the river) each read 0.0000 while 14 screams started, and the mix's loudest pieces fell from 0.0329 to
nought. In the build before, the same far window read 0.908 of the near one. The staff's 82 log lines read ranges
of 60, 70, 75 and 90. **Found, not built:** the original sounds the Belly Bounce's scream at (525, 250), and
OpenTPW at its thing's cell, (510, 230). A Hot Pot's engine in a go read range 100 and 0.8252 at 22.92 units,
and nought far off (`q265/engine.py`, 2 of 2).

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `QMixer 0x1800a980` | — | A channel's gains from its place and the listener's: distance, cone, and the product | Decompiled; read live |
| `QMixer 0x18007a20` | — | A channel's command switch; type `0xc` stores the mapping, type 1 the flags, type 2 the volume | Decompiled |
| `QMixer 0x1800cfd0` | — | The inverse arm's gain | Decompiled |
| `QMixer 0x1800de70`, `0x1800de40` | — | A power, by `2 ^ (y x log2 x)` | Decompiled |
| `QMixer 0x18009c50` | — | The session's channel n: the count at `+8`, the array at `+0xc` | Decompiled; walked live |
| `0x006d22d0` | — | The wrapper's channel flags from the game's, always with `0x1000` | Disassembly |
| `0x006d2320` | — | The wrapper's slot `+0x68`: `QSWaveMixSetPanRate`, then `QSWaveMixEnableChannel` with those flags | Decompiled |

## A channel's volume

Read for Q267 and held against the running original's channels (`q265/orig/a-load.log`, `original/mixpairs.py`).

**A voice's loudness is its variation's volume and nothing else of the play.** The channel's set-up
`FUN_006c5690` sends a volume when the request mask has bit `0x2`: it copies `params + 0x28` to the channel
record's `+0x18`, scales it (`FUN_006bb860`) and hands the whole number to the wrapper's slot `+0x80`.
`params + 0x28` and the bit are written by `FUN_006c5130`, and the voice's channel set-up (`0x006bc461`; under voice flag `0x8000` the word at the voice's `+0x48` instead, which `FUN_006bc900` stores for a voice with no channel, reached only from `FUN_0051c0e0`, whose three callers are the advisor's two stops) and its apply (`0x006bbbe0`, skipped under voice flag `0x400`) give it
`FUN_006bc090`'s answer: the variation's two volume bytes (`+0xc`, `+0xd`), by a controller of the variation's
own or else a draw between them ("A voice's two controllers", below); 100 for a voice with no variation.

- **The groups' levels** (`FUN_006bb860`): the channel record's mask at `+0x24` picks up to four floats at
  `[+0x34]`, multiplied together from 1.0, and the volume times that is rounded as the FPU rounds, to the
  nearest and a half to the even number. In the running game the sound effects' product is 0.75 and the music's
  0.60, the options' two defaults (`sound.sam`, `DefaultVolume`).
- **The wrapper** (`FUN_006d2980`, slot `+0x80`): a number from 0 to 127, looked up in the table of words at
  `0x00782e40`, which is `n x 258` and 32,767 at 127, and passed to `QSWaveMixSetVolume`. Anything outside 0 to
  127 is refused. The mixer keeps it over 32,767 ("The mixer's distance law"), so a channel's volume reads as
  `n / 127`.
- **A scream's parameter 6 is no volume.** It is slot 0 of the voice's controllers, the held chain's zone
  parameter, and a set of it applies a volume only on a match past slot 0 (`0x006bbb80`). All 32 variations of
  the kids' screams, effects 71 to 90, hold the bytes 47 and 47 and name no controller on the volume, so a
  scream is 47 whatever its script's level: `round( 47 x 0.75 )` is 35. That the Belly Bounce's parameter is
  also 35, `(20 + 50) / 2`, is a coincidence of the two numbers.

**Held in the running original** (predicted first, `q267/PREDICTION.txt`, 2 of 2). Its mixer's log of 150 s of the
shipped park holds twelve pairs of a 3D channel's far end and volume, and each is `round( v x 0.75 )`, a half to
the even, of a volume a variation of that range holds in the global or the jungle's category files: 35 of 127 at
95 (47, the screams, all 212 lines of them at (525, 250)); 18 and 12 at 75 (the staff's 24 and 16); 26 at 80
(34); 45, 64 and 16 at 90 (60, 85, 21); 32 at 60 (42); 75, 35, 21 and 10 at 100 (100, 47, 28 and 14). With
Q264's river, 38 from 50, the rounding is seen both ways: 37.5 up to 38 and 10.5 down to 10. **Not done:** no
scream was made at a level other than 20 in the original, so that the parameter moves no volume there is the
listing's and the files' alone.

**What OpenTPW does** (`ParkAudio.ChannelGain`): a sound effect's volume `v` plays at
`round( v x 75 / 100 ) / 60` of the music's own level, the same whole number beside the music's 60. A held
scream's child takes its variation's volume (`ScreamChildVolume`), and a single scream, a guest put off and a
yawn the picked variation's (`SoundCategory.Play`'s `gain`); the land's own sounds go through the same sum.
`SCREAMLEVEL` moves the playing child's volume only where its variation names the parameter on its volume, which
no shipped scream does. The staff's voices and the crowd's take it too (Q268, below), a bumper ride's engine (Q299, below), thunder (Q300, below) and the rain's two voices, 1 to 3 of 127 (`weather.md`, "Two rain voices, and how loud each is"). **Not through it yet:**
every flat effect of the interface and the lobby's own thunder.

**Measured here, 2026-10-10** (`q267/confirm.py`, 4 of 4): with the camera on the Belly Bounce `voices` read ten
scream samples at (525, 250), each at a volume of 0.1925 (35 over 60 of the music's 0.33), 24.26 off at a gain of
0.8034, and the log's line read parameter 35, volume 47; nine of the ten were found in the game's own mix at a
score of 0.99 or better, each at a least-squares gain of 0.0614. The build before (control, 2 of 2) read the
log's volume as 35, the parameter, and eight samples in its mix at 0.1116 each, predicted before its run: the
fix's is 0.550 of it, where 0.1925 over 0.35 is 0.55. Twenty bugs put back (`q267/mutations.py`): 18 fail a test,
one more after a test line was added, and one fails none, a `SCREAMLEVEL` that sets a flat volume behind the
test for a controller, which no shipped scream passes.

**The crowd's voice and the staff's, held in the running original** (Q268; `q268/orig/run.sh`, predicted first in
`q268/PREDICTION.txt`, 3 of 3). The mixer and the crowd's voice were polled together (`mixer.py`, `q236/orig/crowd.py`)
while the pointer was moved on and off the Belly Bounce's queue. The crowd's voice is the flat channel at the
listener's own place that Q265 left unnamed: it read 10 of 127 through every stretch the handle `[0x00803aa4]` was
held at a level of 12 or less (a volume of 14, 10.5 to the even 10), 11 at levels 13 to 15 (a volume of 15), fell to
1 and was gone through every stretch the handle was nought, five stretches of each in order; its distance gain read
1.0000, so its final gain is the volume. The staff's channels read 18 of 127 at a far end of 60 and of 75 (24), 12 at
75 (16), 22 at 70 (30) and 16 at 90 (21). The twin variation's draw between 14 and 22 was never
seen, only 10 and 11, because none is made ("A held voice's next sample", below). **Not explained:** flat channels at 1, 2 and 3 of 127
with a far end of 70 or 95 stayed on after the crowd's was let go, and one at 4 of 127 from the park's start.

**In OpenTPW** (`q268/confirm.py`, fix 3 of 4, control 3 of 3): with the pointer's cell held on the queue `crowd`
read a volume of 14 playing at 0.0550 and 15 at 0.0605, and the staff's 130 log lines 16 at 0.0660, 21 at 0.0880, 24
at 0.0990 and 30 at 0.1210, each `round( v x 0.75 ) / 60` of the music's 0.33. With the camera 750 units off, where
the mix is the crowd's flat voice alone, twenty samples were found at a score of 0.97 or better, the lowest at a gain
of 0.02719, half of 0.0550 by the master; the build before read 0.02074, half of its 0.0420, so the fix is 1.3107 of
it where 1.310 was owed. The one miss was the check's: `crowd` reads a volume of nought between two samples. Twelve
bugs put back (`q268/mutations.py`): all twelve fail a test.

**A bumper ride's engine, held in the running original** (Q299; `q299/orig/run.sh`, predicted first in
`q299/PREDICTION.txt`). The mixer's 3D channels were polled (`mixfreq.py`) for 150 s over a load of a Hot Pot in its
go (`q257o/orig/hotpot-later2.TPWS`). The lead boat's engine, jungle `cat_rides` 194 (volume 68 and 68, range 100),
is the channel at a far end of 100 that moves about inside the pot at height nought: it read **51 of 127**,
`round( 68 x 0.75 )`, on 2,276 lines over five channels as go followed go, and its frequency 41 values from 18408,
the sample's 22050 at a pitch of -24, to 24930. So the class 194 makes (vtable `0x0070a338`) sends its volume
through the same scale as a one-shot's. As a go ended the same channel read 30 and then 13 of 127 before it closed,
the fade. Every other line at a far end of 100 inside the pot was one fixed place, (435, 255) at 64 of 127 (a
volume of 85), not the engine's and not named here.

**In OpenTPW** (`ParkCarSounds.Gain`; `q299/confirm.py`, fix 5 of 5, control 3 of 3): through a bought Hot Pot's go
`voices` read the engine at a volume of 0.2805, 51 over 60 of the music's 0.33, sixteen reads of sixteen at twelve
rates and a range of 100, and `bumpers` and the log's line read volume 68 playing at 0.2805; the build before read
0.6800, the 68 as a plain hundredth. With the music off and the camera on the pot the game's own mix over the go's
first 12 s read an rms of 0.00853 against the build before's 0.02056: 0.415 of it, where 0.2805 over 0.68 is
0.4125. Nine bugs put back (`q299/mutations.py`): seven fail a test, and two fail none, a wrong volume handed to
the voice as it starts, which the apply that follows in the same call sets right before a test can read it.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x006c5690` | — | A channel's set-up from a voice's requests: bit `0x2` the volume | Decompiled |
| `0x006c5130` | — | Stores a volume at `params + 0x28` and sets request bit `0x2` | Disassembly |
| `0x006bb860` | — | A channel's volume times its groups' levels, rounded | Decompiled |
| `0x006d2980` | — | The wrapper's slot `+0x80`: 0 to 127 through the table to `QSWaveMixSetVolume` | Decompiled |
| `0x00782e40` | — | 128 words, `n x 258`, 32,767 at 127 | Read |
| `0x006bc090` | — | A voice's volume from its variation's bytes | Decompiled |

**Thunder, named in the original's mixer and built** (Q300; predicted first, `q300/PREDICTION.txt`, 1 of 1). The
global ambient's effect 32 is a plain one-shot (flags nought) of one variation: volume bytes 85 and 85, pitch bytes
-4 and 4, range 1500. `FUN_00512880` plays it by `Sound_PlayEffect` at the bolt's top and sets nothing after it, so
the volume is `round( 85 x 0.75 )`, 64 of 127, and the pitch a draw of -4 to 3 ("A one-shot's pitch", below). With
the weather thing's `mLightning` held at 1 in the running original (`original/thingpoke.py`; quality 59, a clear
day, so the bolts are the flag's and no storm's), `mixfreq.py` over 150 s read **30 thunders, each a 3D channel
with a far end of 1500 at 64 of 127 and a height of 300**, anywhere on the map, at eight frequencies and no other:
21268 (2), 21422 (4), 21578 (6), 21734 (4), 22050 (5), 22371 (1), 22533 (6), 22696 (2); none at 21115 or 22861
(`q300/orig/a-load.log`). One of the thirty opened at 24 of 127 and read 64 a second later, at the same frequency:
what turned it down was not read. Two thunders sounded 12 ticks apart, so nothing there holds one back for another.

Here (`ParkAudio.Thunder`, through `OneShotGain` and `OneShotRate`): `q300/confirm.py`, fix 5 of 5 and control 4
of 4, fourteen bolts aimed at the Belly Bounce nine seconds apart. Thirteen thunders' log lines and `voices` read a
volume of 0.3520 (64 over 60 of the music's 0.33) over 1500, at pitches -4, -3, -2, -1, 0, 2 and 3, and each one's
sample was in the game's own mix at a score of 0.946 to 1.000, its own pitch the best-matching of twelve tried for
all thirteen. The mix's gain, a channel at a time, over the volume times the distance gain read a median of 0.6654
(0.6464 to 0.7038); the build before read 0.4000 and a rate of 1.0000 on every one of eleven, and the same number
at 0.6636 (0.6533 to 0.6882), so a thunder is 0.88 of what it was. Eleven bugs put back (`q300/mutations.py`): all
eleven fail a test. **Not done:** one bolt of the fix's fourteen and three of the control's had no thunder, which
the per-effect wait still holds back here ("How the engine plays an effect", above); a real storm was in no run
of either game (the original's bolts were the held flag's, these the console's); not listened to by a person.

## Where positional audio actually lived

In parks, in the ride-script engine. `FUN_005573d0` (its own error strings say `RSSE:`) resolves a location via `FUN_00556b90`, then calls `Sound_PlayEffect` with real x, y, z for object types 3–10, and feeds the *same* resolved location to `Particles_Spawn` for types 1–2.

`FUN_00556b90` resolves the location two ways:

- **Given a node id**: `FUN_0044b220` finds the lookup record; position is the translation row (`+0x30` / `+0x34` / `+0x38`) of the world matrix the pose walk last stored for that record, (0, 0, 0) for one never stored; a miss returns 0 having written nothing (`ride-operation.md`, "How long a leg lasts, and where its ends are"). Optionally direction is `+0x20` / `+0x24` / `+0x28`, normalised and sign-flipped on a flag bit — that direction is the source cone.
- **Otherwise**: the centre of the thing's cell rectangle ×10 in x and z, at the model node's base height (`0x00556d52`): it takes `FUN_00466b70`'s box and drops its heights. `(0, 0, 0)` with no model or no `.hmp` (`park.md`, "The effect subsystem").

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_005573d0` | — | Ride-script sound/particle spawn; object types 3–10 → `Sound_PlayEffect`, types 1–2 → `Particles_Spawn`, same resolved location | Its error strings are prefixed `RSSE:` |
| `FUN_00556b90` | — | Resolves a script location: node id → the record's stored world matrix, else the centre of the thing's cell rectangle ×10 in x and z at the node's base height, or `(0, 0, 0)` with no model or no `.hmp` | Decompiled |
| `FUN_0044b220` | — | Node lookup by id **and** capability flag (see below) | Decompiled |
| `+0x30` / `+0x34` / `+0x38` | — | Translation row of that stored matrix = emitter position | Decompiled |
| `+0x20` / `+0x24` / `+0x28` | — | Direction row, normalised and sign-flipped on a flag bit = source cone | Decompiled |

## What an EventMap's slots feed

A track ride's or coaster's own sounds do not come from its script's `ADDOBJ` or `EVENT`: the ride engine reads them
from the script's `SPAWNSOUND` child, its `EventMap.rse` (`park.md`, "The script-to-script family", has the two
layouts and the jungle values). `FUN_0055a3e0( script, slot )` answers the child's variable at that index, and
`FUN_0051eeb0( script, flags, category, slot, x, y, z )` plays it as an effect when it is not 0. **Every caller passes
`[0x00803a3c]`, the park's `cat_rides`.**

| Slot | Read by | Ride | What it does |
|---|---|---|---|
| 0 | `FUN_004392a0` (`0x0043932f`) | coaster, per train (list `DAT_00790fe0`) | plays it into the train's `+100`, after stopping the old voices; skipped while `DAT_00785a2c` is set |
| 1 | `FUN_004392a0` (`0x00439363`) | the same | plays it into the train's `+0x68` |
| 5-8 | `FUN_004392a0`, `FUN_004396e0` | the same | stored in the train at `+0x54`..`+0x60` (use not read) |
| 0 | `Bumper_Retarget` `FUN_0054a040` (`0x0054a366`, `0x0054a682`, `0x0054a909`) | bumper, go-kart, water arms | plays it into the car's held voice `+0x20` |
| 1 | `FUN_0054ae50` (`0x0054b01d`) | go-karts only (types -4, -7, -9, -10), as a car is taken off | plays it; every other type fades its voice |
| 3 | `Bumper_PlayCarSound` `FUN_00547170` (`0x00547311`) | bumper, go-kart, water | case 2, the toot (logs "TOOT") |
| 4 | the same (`0x00547425`) | the same | case 4 |
| 0 | `FUN_0055a720` (`0x0055a944`) | tour ride, as a car is added | plays it at the car's position / 300 into `car+0xe0` |
| 10 | `Bumper_StepCar` (×3) and `FUN_0055abf0` (×17) | track rides; the flying cars | a **parameter id**, not an effect: `FUN_0051bc40( voice, slot 10, level )`, levels 60/30/80/15 for the flying cars and computed for the track cars |

So slot 10 is `VAR_PAR0` of the 11-variable layout and slots 5-8 are `VAR_PAR0`-`VAR_PAR3` of the 10-variable one. **No
caller reads slot 2**, so the coasters' `VAR_EVT2` (69, not an effect in jungle's `cat_rides`) is never played. Not read:
what sets the car's case to 2 or 4, the gates on the go-kart and water arms of `Bumper_Retarget`, and what
`DAT_00785a2c` is. `FUN_0055a3e0` answers 0 for a script with no sound script or a slot past its variable count.

### The bumper arm's engine (read for Q202, measured in the original)

- **The start** (`Bumper_Retarget`, `0x0054a250`..`0x0054a3a2`): only when the car's `+0x20` is 0, its flags hold both
  `0x4000` and `0x400000` (active and the lead), its ride's `+0x50` is 2 (running) and it lacks `0x20` (unloading). The
  thing is found by walking the things for one of type 3 whose `+0x28` is the ride's handle; its script's slot 0 is
  played (`FUN_0051eeb0( script, 0, cat_rides, 0, 0, 0, 0 )`) and the answer put at the car by `FUN_0051c270( voice,
  x × f, 0, z × f )`, `f` the float at `0x00700f20` (0.0033036, so 302.7 record units to a world unit, where the boats
  are drawn at 307.2), height 0. Both answers go into `+0x20`.
- **Each step** (`Bumper_StepCar`'s bumper arm, `0x00548499`..`0x00548584`, every type of it, not only the Hot Pot):
  while `+0x20` is not 0, `FUN_0051c270` moves it to the car, and `FUN_0051bc40( voice, slot 10, speed / 3 )` sets the
  parameter slot 10 names (16 for the jungle `bumper`) to `+0x4c / 3`. Each answers the handle, or 0 once the voice
  has gone (`FUN_006b6620`), and the answer goes back into `+0x20`, so a voice that has ended empties it and the next
  retarget starts another. No ride state gates the step.
- **The go's end** (`Bumper_CarTick`'s unloading arm for types -1, -3, -6, -11, -14, `0x0054788e`, by the byte map at
  `0x00547c30`): after the unload, `Sound_StopFading( +0x20 )` (the 60 is a per-pass volume step, "The fade", below), every unloading tick, **without clearing
  `+0x20`**; the handle goes when the voice has. **Taking a car off** (`FUN_0054ae50`, `0x0054b065`) fades it the same
  way. The cars stay when a go ends; only `BUMP 10`, a sale or a capacity cut takes them off.
- **Measured in the original** (Q202, `q202/voicelog.py`, the reference park patched to research the Hot Pot, bought
  and queued; two goes logged every track tick): the lead boat's `+0x20` was non-zero from the go's first tick, the
  same handle for all 750 ticks of it (28825 to 29575); the ride went back to loading at tick 29575 and `+0x20` read 0
  from 29583, 8 ticks and 135 ms later (that run stepped two ticks a frame); the next go's first tick held a new handle. No other boat held one.

The effect is jungle 194, `Engine.mp2` (447 ms by the map), flagged `0x6` (below): a voice with no timer, so its own tick plays
the sample again each time its channel ends and the handle lives for the whole go.

## A voice's two controllers

**Which class an effect's flags word makes** (`0x006b6774`..`0x006b67f2`, the play entry): bit `0x4` clear, a
one-shot; `0x4` with `0x10`, `FUN_006be680`; `0x4` with `0x2`, `FUN_006be610` with `0x400`, `FUN_006be330` with
`0x100`, else `FUN_006be090` (vtable `0x0070a338`, 194's class); `0x4` alone, `FUN_006bdd30` with `0x400` (the held
chain), else `Voice_MakeAnyChain` (`0x006bdc60`, the chain of any variation). **The voice's own flags** (`FUN_006bbe90`, `+0x30`): effect `0x8` gives `6`, effect `0x4`
gives `2`, `0x200` gives `8`, `0x20` gives `0x200`, `+0x11` bit `0x40` gives `0x20000`. **A voice with `2` never
times out** (`FUN_006bcb60`); one without it is freed at start + length + 250 ms. When its channel ends
(`0x006bbe40`) it clears `1` and, unless it holds `0x40` or `0x4800`, is marked finished (`0x10`) and freed by the
next service (`0x006b62d5`). Effect `0x8`'s voice bit `4` asks the mixer for a loop (`0x006be016`, request `0x40`).
Which of these keeps 194's voice alive across its channel's end was **measured, not read**: the handle lived 758
ticks over a 447 ms sample.

**The controllers** (`[voice+0x4c]`, eight bytes, `FUN_006b7150`): four keys then four values. Key 0 is the effect
record's `+0x12`; keys 1 and 2 are the variation header's bytes `+0x16` and `+0x1a` (`FUN_006bbf00`); key 3 is 0.
`FUN_0051bc40( voice, id, level )` goes to the voice's vtable `+0x28` (`0x006bbb80`; the held chain's `0x006bda90` and class `0x0070a498`'s `0x006be580` call it first): every slot whose key is `id`'s
low byte takes `level`'s low byte, and a match past slot 0, unless the voice holds `0x4000`, applies them
(`0x006bbbe0`). Slot 0 is the held chains' zone parameter (above).

**What they drive** (`0x006bc040`, asked with bit 1 for the volume and bit 2 for the pitch): the header's `u16` at
`+0x18` with that bit takes slot 1's value; else `+0x1c` with that bit takes slot 2's; else none. With a value `v`,
the volume is `v × (hi − lo) / 100 + lo` over the bytes `+0x0c`/`+0x0d` (`FUN_006bc090`) and the pitch the same over
the signed bytes `+0x0e`/`+0x0f` (`FUN_006bc170`), the pair swapped first if out of order; with none, a draw from `lo`
up to, not including, `hi` (`0x00fb1f20`'s generator). Bit 4 of the same masks is the held chain's wait (above).
Measured over all 1,595 shipped variations: the masks use only bits 1, 2 and 4.

**The pitch** goes to the mixer as a frequency (`FUN_006d2820`, request `0x20`): the channel's own rate times a
table entry, `0x00782f40 + 4p` above nought and `0x00783540 + 4|p|` below, and those tables hold 2^(k/96) from k = 1,
so a pitch `p` plays at 2^((p + 1) / 96) of the sample's rate above nought and 2^((p − 1) / 96) below, then
`QSWaveMixSetFrequency`. **For 194**: key 16 at `+0x16`, mask `+0x18` = 2, pitch -24 to 36, volume 68 to 68: the speed
/ 3 the step sets moves the pitch from -24 (0.835 of the sample's rate) at rest to +25 (1.207) at a boat's top speed of
247, at volume 68.

**OpenTPW** builds this for the bumper arm (`ParkBumperCars`, `ParkCarSounds`): the lead's engine looped at 68 / 100,
moved and pitched each tick, stopped with a 0.06 s fade of its own (**a deviation**: the original's `Sound_StopFading` is a x0.6 step, stepped by a worker thread every 100 ms, at detail above 30, a cut otherwise, "The fade", below; Q509); its volume is the channel's own number, 51 of 127
beside the music's 60 ("A channel's volume"). The coasters', tour ride's and go-karts' slots are not
built.

## A held voice's next sample

Read for Q269 and measured in the running original (`original/crowdmix.py`, `q269/orig/`).

**A sample that follows another on a held voice is queued with its sample alone.** The class's slot 0
(`0x006bde00`; vtable `0x0070a498`, the crowd's and the music's) picks the next variation (slot `+0x38`,
`FUN_006be5e0`: `FUN_006be450` by the zone parameter, then `FUN_006bbf00` for the keys), draws its sample (slot
`+0x3c`, `FUN_006bc680`), fills a request block through slot `+0x48` (`0x006bdf00`) and queues it on the voice's
channel (`FUN_006b9a60`). Slot `+0x48` calls `FUN_006bc240`, which sets the sample and its bank (`FUN_006c5190`,
`FUN_006c51b0`) and two flag bits, then the variation's wait where it has one: **no volume and no pitch**. Those are
requested in two places only: the voice's first start (slot `+0x14`, `0x006bdf50`, whose slot `+0x44` is
`FUN_006bc410`: `FUN_006bc090` and `FUN_006bc170`), and the apply (`0x006bbbe0`) a parameter set reaches on a match
past slot 0. So the channel plays every later sample at the volume and frequency it last had. `FUN_006bbf00` zeroes
keys 1 to 3 for a variation naming no controller, so while one is current a set matches slot 0 alone and applies
nothing; `FUN_006bc090`'s draw is reached for such a variation only where it is a voice's first, or a one-shot's.

**Measured** (predicted first, `q269/PREDICTION.txt`; the pointer held on the Belly Bounce's queue, 20 ms a poll):

- The first prediction, from the listing as it was read then, was that a twin draws 14 to 21, and it **failed**:
  through 242 stretches with the crowd's twin current its channel read 10 of 127 every time, as it did with the
  first variation current (levels 1 to 12), 427 lines of 427 in a second run.
- The channel record's word at `+0x1f4` is its frequency: six values, 22050 and 22371, 22533, 22696, 22861, 23026
  (`22050 x 2^((p + 1) / 96)` for a pitch of 1 to 5), changed 37 times over 449 new samples in 31 s, each time four
  of `mGameTick` after the last (30 gaps), eight (5) or sixteen (1): the beat, or more than one where a draw
  repeated or the twin was current, and never with the twin current: predicted before the run.
- The variations' weights in memory are 5904 and 590, the file's running totals taken apart by the loader
  (`FUN_006c0610`, under its byte `+0xcc`); the twin was 12% of 423 samples.

**The pace is not in these logs.** Off-screen the original plays into a null device that is not paced: a sample was
current for 69 ms where its clip's length is 208 on average, so how closely one sample follows another is still not
measured.

**What OpenTPW does** (`ParkAudio.HeldSample`, `NextCrowdSample`): the crowd's voice keeps one volume and one pitch;
its first sample sets them, a later one plays at them, and a beat's set draws them again on a variation that names
the parameter. Held in the running game (`q269/confirm.py`, fix 2 of 4, control 2 of 2): every pair of samples
started inside one beat shared volume and pitch, 314 of 314; the 27 twin samples' log lines read 14 playing at
0.0550 and 15 at 0.0605, the voice's own (the level reached 13, which the prediction had not allowed for: its two
misses), and six twin samples were found in the game's own mix at 0.0273 to 0.0275; the build before read a twin at
every volume from 14 to 21 and in its mix at up to 0.0413. Eleven bugs put back (`q269/mutations.py`): all eleven fail a test.
The music is of the same class, and its variations hold volume bytes 100 and 100 and pitch bytes 0 and 0 in all four
themes (`sfxmap.py`), so its first sample's draw is no draw and a later sample has nothing to keep: OpenTPW plays it at
one volume and no pitch. A held scream is another class, and each of its samples is a voice of its own: "A scream's
child and its pitch", below.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x006bde00` | — | Slot 0 of class `0x0070a498`: the next variation, its sample, the requests of slot `+0x48`, queued | Disassembly |
| `0x006bdf00` | — | Slot `+0x48`: `FUN_006bc240`, then the wait; no volume, no pitch | Disassembly |
| `0x006bc240` | — | A request block's sample and bank from the voice's sample record | Decompiled |
| `0x006bdf50` | — | Slot `+0x14`, a voice's start: slot `+0x44` (`FUN_006bc410`) fills the requests, volume and pitch among them | Disassembly |
| `0x006be5e0` | — | Slot `+0x38`: `FUN_006be450` with controller value 0, then the keys | Disassembly |
| `0x006c0610` | — | The loader's pass over an effect's variations: running totals to weights, zone indices to pointers | Decompiled |

## The plain class: any variation, and a silence after a sample

Read for Q302 (the gulls and the sea, `park.md`, "The land's own sounds") and held in the running original
(`original/voices.py`, `mixer.py`, `mixfreq.py`; `q302/orig/`).

**The class of flags `0x4` and `0x2` with none of `0x10`, `0x400` and `0x100`** (`FUN_006be090`, constructor
`FUN_006be0e0`, vtable `0x0070a338`) **is the music's class with two slots changed.** Its slot 0 is the same
`0x006bde00`, so it plays one sample after another and queues each later one with its sample alone ("A held voice's
next sample", above): the volume and the pitch are drawn once, as the voice starts (`FUN_006bc410`), and kept.

- **Slot `+0x38` is `FUN_006bc810`: a variation drawn by weight among all of the effect's**, with no zone and no
  parameter. An effect of one variation takes it. Otherwise the generator steps, and the walk adds each
  variation's weight (`+0x1e`) and stops at the first running total not below the draw's high word. Asked with 1
  (slot 0, a sample after the first), on an effect of more than two variations, a draw of the variation before
  (`+0x45`) is moved on to the next, round to the first. The voice's own start (`FUN_006bbf60`) asks with 0.
- **Slot `+0x28` is the base `0x006bbb80`**: a parameter set finds its key as any voice's does.
- **The wait is a silence after the sample** (`0x006bdf00`, slot `+0x48`): with either of the variation's wait
  words not nought, slot `+0x4c` (`FUN_006bc2d0`, "min + LCG % (max - min)") is drawn and stored in the request at
  `+0x58` under bit `0x200000` (`FUN_006c5220`, its one caller). The channel's source builder (`FUN_006ba460`)
  then chains a stage of class `0x0070ab58` after the sample's (`FUN_006c6730`): the wait in milliseconds times the
  format's rate over a thousand, in bytes, and its read (`0x006c67f0`) passes the sample through to its end and
  then writes that many bytes of nought before it answers that it has ended. The voice's first sample goes
  through `FUN_006bc410`, which sets no wait, so the first is followed at once.

**The two shipped users a park's land places** (`lib/sfxmap.py`): global ambient 2, the gulls, one variation of
two samples at volume 49, a pitch of -2 to 2 and a wait of 0 to 10000; and 3, the sea, three variations of one
sample each at 33, 55 and 55, no pitch and no wait. Neither is given a range, so each takes its variation's, 100.

**Measured in the original, 2026-10-11** (predicted first, `q302/PREDICTION.txt`, 4 of 4; the shipped park loaded
from four files whose cameras stand at stepped distances from the sea, `q302/mkfiles.py`, a minute each):

| Listener | The gulls' voice and channel | The sea's |
|---|---|---|
| (417, 47, 18), inside both rectangles | at the listener's place; 37 of 127, frequency 21734, gain 1.0000 | at the listener's place; 25 of 127, 22050, gain 1.0000 |
| (415, 47, 46) | at (415, 47, 31), 15.06 off, gain 0.8919; 37 of 127, 22050 | at (415, 47, 21), 25.06 off, gain 0.8068; 41 of 127, 22050 |
| (410, 47, 106) | at (410, 47, 31), 75.21 off, gain 0.3330; 37 of 127, 21578 | at (410, 47, 21), 85.21 off, gain 0.2202; 25 of 127, 22050 |
| (411, 47, 167) | 136 off: flag `0x800`, no channel | 146 off: flag `0x800`, no channel |

Each channel read a far end of 100 and one volume and one frequency for its whole minute, whatever samples went
by: 37 is the gulls' 49 through the channel's sum, 25 and 41 the sea's 33 and 55, the variation its start drew;
21578, 21734 and 22050 are a pitch of -2, -1 and 0 ("A voice's two controllers": `22050 x 2^((p - 1) / 96)` below
nought). Every gain is the law's, `((100 - d) / 98) ^ 0.8`. **Not measured:** how long the silence after a gull is
(the original's sound off-screen is not paced, and the channel stays open through it), and which sample follows
which.

**What OpenTPW does** (`ParkAudio.StartFollowing`, `ServeFollowing`, `AnyVariation`, `PlaysFollowing`): a voice
of this class is a variation drawn as `FUN_006bc810` draws it, a sample of it, the volume and pitch of the first
kept, and after every sample but the first a silence drawn from its variation's wait, timed on the frame clock
from the frame the sample is found ended. Built for the land's own type 3 sounds alone; the run is in `park.md`,
"The land's own sounds". The sample draw's own rule against the sample before (`FUN_006bc680` asked with 1, for a
variation of more than two samples) is not built, here or for the crowd's and the music's voices.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x006be090`, `0x006be0e0`, `0x0070a338` | — | The plain class's maker, constructor and vtable | Disassembly; the vtable read beside `0x0070a498` |
| `0x006bc810` | — | Slot `+0x38`: a variation by weight among all, never the one before of more than two | Decompiled |
| `0x006bbf60` | — | A voice's set-up: slot `+0x38` asked with 0, the keys, the first sample | Decompiled |
| `0x006c5220` | — | The request's wait: bit `0x200000`, the value at `+0x58` | Decompiled; one caller, `0x006bdf35` |
| `0x006ba460` | — | The channel's source builder: a loop stage under bit `0x40`, a silence stage under `0x200000` | Decompiled |
| `0x006c6730`, `0x0070ab58`, `0x006c67f0` | — | The silence stage: its length from milliseconds, its vtable, its read | Decompiled, disassembly |

## A scream's child and its pitch

Read for Q297 and measured in the running original (`original/mixfreq.py`, `q297/orig/`).

**Each child of a held scream is given a volume and a pitch as it starts.** A child is a voice of its own (class
`0x0070a9e0`, "A held voice is a chain", above), not a sample queued on its parent's channel, so it starts as a
one-shot does: its slot `+0x14` (`0x006bd800`) zeroes a request block and has slot `+0x44` (`0x006c3ae0`) fill it,
then opens a channel with it (`FUN_006b9ab0`). The fill sets the sample (slot `+0x48`, `FUN_006bc240`), the volume
(`FUN_006bc090` into `FUN_006c5130`), the group (`FUN_006c51f0`) and the pitch (`FUN_006bc170` into `FUN_006c5170`,
the request's `+0x24` under flag `0x20`), then the distance mapping from the variation's range. `FUN_006bc170` is the
one-shot's own: a controller's value over the variation's signed bytes `+0x0e`/`+0x0f` where the voice has a
controller for bit 2 (slot `+0x40`, `0x006c3d80` in this class), else a draw from the low byte up to, not including,
the high one. Kids 71 to 74 hold 0 and 6 in every one of their sixteen variations and name no controller on the
pitch, so **every child draws 0 to 5**, afresh, where the crowd's voice draws once and keeps it.

**Measured** (predicted first, `q297/PREDICTION.txt`; Q265's `bounce.TPWS`, the listener about 50 units from the
Belly Bounce, 150 s at 20 ms a poll): the scream channels (3D, a far end of 95, 35 of 127, at (525,250,0)) made 180
lines, and their frequency word `+0x1f4` read 22050 (28 lines), 22371 (40), 22533 (24), 22696 (23), 22861 (31) and
23026 (34): the six of "A held voice's next sample" and no other, 22050 in 15.6% where a sixth was owed. In the same
log the channels with a far end of 75 at 18 of 127 read frequencies below 22050 too (20366 to 23701): a variation
whose pitch bytes run either side of nought, not looked up.

**What OpenTPW does** (`ParkAudio.PlayScream`, `ScreamChildPitch`; `ParkScreams.Chain.Pitch`): each child draws its
pitch from its variation as its volume is read, and its voice is set to the rate the pitch stands for
(`ParkCarSounds.Rate`). Held in the running game (`q297/confirm.py`, fix 3 of 4, control 3 of 3): 38 children of the
Belly Bounce's scream in 75 s, each log line a pitch of 0 to 5 and all six seen, `voices` reading the six rates
(1.0000, 1.0145, 1.0219, 1.0293, 1.0368, 1.0443) at a volume of 0.1925 as before. In the game's own mix
(`q297/pitchscan.py`, each sample matched at each of six pitches over the whole capture): a pair of sample and pitch
that a log line names scored 0.895 to 1.000, 33 of 34 (the other, 0.301, was the first child, under the music), and a
pair no line names 0.742 at most, 32 of 32; the build before read every sample at 1.000 on pitch 0 and 0.772 at
most on another. The two checks written first each missed on their own terms: one wanted five samples played once
(two were, both right), the other a score of 0.97, which a pitched template, resampled by a straight line where the
mixer steps to the nearest sample, reaches in 22 of 34. Eleven bugs put back (`q297/mutations.py`): all eleven fail
a test.

**Not built:** the parameter set's apply on a playing child (`0x006bbbe0`), which no shipped scream's variation
asks for. A kids' one-shot's pitch is "A one-shot's pitch", below.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0070a9e0` | — | vtable of a held scream's child: `+0x14` `0x006bd800`, `+0x40` `0x006c3d80`, `+0x44` `0x006c3ae0`, `+0x48` `FUN_006bc240` | Read from the image |
| `0x006bd800` | — | The child's start: a request block zeroed, filled by slot `+0x44`, a channel opened with it | Disassembly |
| `0x006c3ae0` | — | The child's request fill: sample, volume, group, pitch, the range's mapping | Disassembly |
| `0x006bc170` | — | A voice's pitch: by a controller over the variation's bytes `+0x0e`/`+0x0f`, or a draw between them | Decompiled |
| `0x006c5170` | — | A request's pitch: `+0x24`, flag `0x20` | Decompiled |

## A one-shot's pitch

Read for Q298 and measured in the running original (`original/mixfreq.py`, `q298/orig/`).

**A plain one-shot draws a pitch as it starts.** An effect whose flags word has bit `0x4` clear is made by
`FUN_006bd7b0` ("A voice's two controllers", above), whose voice (`FUN_006b71d0`) wears vtable `0x0070a3e8`: slot
`+0x14` is `0x006bd800`, the start a scream's child shares, and slot `+0x44` is `FUN_006bc410`, the channel's set-up.
That asks for the volume (`FUN_006bc090`, unless the voice holds `0x8000`) and then the pitch (`FUN_006bc170`, unless
it holds `0x10000`; `FUN_006bbe90`, which makes the voice's flags from the effect's, sets neither). With no
controller on the pitch the draw is the generator at `0x00fb1f20` modulo the high byte less the low, plus the low:
from the low byte up to, not including, the high one. `Sound_PlayEffect` with no handle sets nothing afterwards
(`FUN_004faa00`, a person's sound, is the place and the call alone), so the kids' one-shots play as their variations
say: **a guest put off (`0x80`, bytes -5 and 5) at a pitch of -5 to 4, a single scream of effects 75 to 90 (-8 and 8)
at -8 to 7**, and a yawn (`0x7e`) and effects 105, 106, 108 and 109, whose two bytes are both nought, at the sample's
own rate. Thunder is the same voice in the ambient category (effect 32, bytes -4 and 4: -4 to 3), held in
the original's mixer and built by Q300 ("A channel's volume", above).

**Measured** (predicted first, `q298/PREDICTION.txt`; Q265's `bounce.TPWS`, the Belly Bounce broken through its
script's `VAR_BREAKSTAT` so its queue is put out). The put-off's channels are 3D with a far end of 100 at 10 of 127,
at the queue's cells; no other placed effect of the global or the Lost Kingdom's categories makes that pair. The ten
frequencies the rule gives for a 22050 Hz sample are 21115, 21268, 21422, 21578 and 21734 (-5 to -1), 22050, and
22371, 22533, 22696 and 22861 (1 to 4).

- Left alone, with the ride broken every 30 s, two put-offs sounded in 270 s, both at 22050 (`mixfreq.log`): the
  queue stays empty for a while after a break, and only a guest whose id divides by eight sounds (`0x0050133d`).
- With the ride broken only once its queue had stood four seconds, and the id's mask byte at `0x0050133f` written
  nought in the running game so that every guest dismissed sounds (`original/breakq.py`; the sound's own path is
  not touched), nine breaks in 330 s made **39 lines, at all ten frequencies and no other** (`mixfreq2.log`): 21115
  (2), 21268 (2), 21422 (4), 21578 (2), 21734 (1), 22050 (1), 22371 (7), 22533 (6), 22696 (7), 22861 (7). None read
  20963 or 23026, a pitch of -6 or 5. Eleven more lines are the same channels turned down to 4 of 127 as they
  played, at the same frequencies; what turns them down was not read.

**What OpenTPW does** (`ParkAudio.OneShotRate`, `OneShotPitch`; `SoundCategory.Play`'s `rate`): a single scream, a
guest put off and a yawn each draw their pitch from the picked variation after their volume, and the voice is set to
the rate it stands for (`ParkCarSounds.Rate`). Held in the running game (`q298/confirm.py`, fix 4 of 5, control 5 of
5): twelve put-offs in three breaks of the Belly Bounce, each log line a pitch of -5 to 4 (six values, three below
nought and nine above), `voices` reading only rates of the ten (0.9576, 0.9786, 0.9857, 1.0219, 1.0293, 1.0368) at a
volume of 0.0550 as before, and `KIDS_ONE_SHOT_PITCH` counted no more. In the game's own mix the put-off's one sample
(`bootout.mp2`), matched at every pitch from -8 to 7 over the whole capture, scored 0.948 to 0.951 at four of the
six pitches the log names, 0.641 and 0.679 at the other two (-5 and 4), and 0.304 at most at the ten it does not:
the check written first wanted 0.85 at four in five of the named, and missed. The build before read no pitch, a rate
of 1.0000 every time, the sample at 1.000 on pitch 0 and 0.297 at most on any other, and the gap counted twelve
times. Twelve bugs put back (`q298/mutations.py`): all twelve fail a test.

**In no game run of either game:** a single scream (the tests alone hold its -8 to 7 here), a yawn's rate, a sale's
put-off of a rider.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0070a3e8` | — | vtable of a plain one-shot's voice: `+0x14` `0x006bd800`, `+0x40` `0x006bc040`, `+0x44` `FUN_006bc410`, `+0x48` `FUN_006bc240` | Read from the image |
| `0x006bd7b0` | — | The play entry's one-shot arm: the voice made (`FUN_006b71d0`) and started through slot `+0x10` | Decompiled |
| `0x0050133d` | — | `FUN_005012f0`'s test of the dismissed guest's id against 7, before kids `0x80` | Disassembly |

## The crowd's voice

Decoded and measured in the original by Q236; built by Q236b (`ParkAudio`, "OpenTPW" below).

**It is the crowd under the pointer.** On every 32nd step, after the music's level (`park-engine.md`, "The music's level"), the park loop reads the word at `[0x007b05cc]` (`0x0054f875`), which is the cell the pointer is over, `y × 128 + x + 1`: the ground pick `FUN_0045d560` writes it each time it runs, the point held to an edge cell of the map and nought only where its clamps fail (`park-engine.md`, "The picker never hands the mode a cell off the map"), and `FUN_0046c0b0` resets it to 1. `FUN_004c8d30( 1, cell, 4, 0 )` then counts the things of kind 1, guests, on the lists of the cells from four before to four after it each way, a 9 by 9 square cut at the map's edges; cell nought counts nothing. The count is held to 100 (`0x0054f89e`), made nought while the world's state `+0x1da738` is 4 (`0x0054f8b0`), and handed to `FUN_0051e7b0`.

**`FUN_0051e7b0( level )`** keeps the level at `[0x00803ab0]`. While sound is up (`[0x00803aa8]`, set by `FUN_0051b660`): above nought, with no handle at `[0x00803aa4]` it plays kids 91 at (0,0,0) (`Sound_PlayEffect( 0, [0x00803a24], 0x5b, 0, 0, 0 )`), then sets the voice's parameter 7 to the level (`FUN_0051bc40`) and keeps what that answers as the handle, nought once the voice is gone, so a voice that ends is started again on the next beat; at nought it stops a live voice with `Sound_StopFading` and clears the handle.

**What parameter 7 does to kids 91** (global `cat_kidsSFX.map`; flags `0x0606`, the music's class, above; the effect's own parameter id, `+0x12`, is 7). Twelve variations, every one with volume bytes 14 and 22 and pitch bytes 0 and 6:

| Variations | Key `+0x16`, mask `+0x18` | Weight | Zones (parameter range to variation) |
|---|---|---|---|
| 1 to 6 | 7, 1 (the volume) | 5904 each | 0-15 to 1, 16-31 to 2, 32-47 to 3, 48-63 to 4, 64-79 to 5, 80-100 to 6, and its own band again to its twin, 7 to 12 |
| 7 to 12 | 0, 0 | 590 for 7, 5904 for 8 to 12 | the six bands to 1 to 6 only |

So the level does two things. As the class's zone parameter (controller slot 0) it picks each next sample's variation: the band's own variation, or from that one its twin, by weight (5904 to 590 in the first band, even in the others), and from a twin always back. As controller slot 1 of variations 1 to 6 it is the volume: `level × (22 − 14) / 100 + 14` in whole numbers (`FUN_006bc090`, `0x006bc040`), 14 from 1 to 12, 15 from 13, up to 22 at 100, of 100; and because the match is past slot 0 the set applies it to the playing sample at once (`0x006bbb80`, `0x006bbbe0`). A twin names no controller, so a set applies nothing while one is current, and no draw is made for it either: a later sample is queued with neither volume nor pitch ("A held voice's next sample", above), so a twin's plays at what the voice had. The pitch is a draw from 0 to 5, made again by each set that applies.

**Measured in the original** (Proton, off-screen, memory only; `q236/orig/crowd.py`, `a.log`, 75 s), predicted first and as predicted: the level word followed this script's own count of guests within four cells of the pointer's cell at every beat (12 over the Belly Bounce's queue, 0 on empty ground, 1 and 2 as a walker came by); the handle was nought exactly while the level was; the voice is of class `0x0070a498` with keys 7, 7 and both values the level; its variation went between 1 (key 7, mask 1) and 7 (key 0), both with volume bytes 14 and 22. **Not predicted:** once, at level 11, the voice ended by itself and was started again a beat later with a new handle; what ended it was not read. Not measured: the sound itself (no capture was made), state 4's nought, and a level past 15 (the stock park has thirteen guests), so the other five bands are the file's and the listing's alone.

**How loud it is, measured** (Q236b; `q236b/orig/`, `crowdx.py`, `musicx.py`). The scale past the variation's volume was not decoded then (it is now: "A channel's volume", above, which is what is built), so it was taken from the original's mix, written to a file by ALSA's `file` plugin. The crowd's clips are 53 to 724 ms long by the map and cannot be picked out under the music, so the music was switched off in the original's own options for the crowd's windows and measured by itself in another: with the pointer over the Belly Bounce's queue (level 9 in memory) 53 samples of variations 1 and 7 matched at a score of 0.6 or better, a first-variation sample at a least-squares gain of 0.036 to 0.046 of the decoded clip; over empty ground (level 0, no handle) one matched. The music's samples read 0.18 to 0.22 against a mono template, which ffmpeg makes 1.41 times the mean of a stereo sample's two channels (checked in OpenTPW's own mix, where a music sample known to play at 0.165 reads 0.117), so 0.26 to 0.31. By sample, the first variation's medians run from 0.022 to 0.046 with a middle of 0.034, where OpenTPW's are one value: **that spread is not explained** (the original may cut a sample at the map's length, which is 35 ms or so short of the decoded clip, or weigh each sample; not read). So a crowd sample at volume 14 is about 0.12 of a music sample at volume 100 (0.09 to 0.17). The options screen's two default volumes taken as plain factors, 75 for sound effects and 60 for music (`sound.sam`, `DefaultVolume`), would give 0.175, just outside that; the channel's own number, 10 of 127 beside the music's 60, is 0.167, and the mixer reads exactly that, so the capture's middle of 0.12 was low and is not explained. The matched samples carry all six pitches. The file is not paced (it runs about 3.6 times faster than the game), so how closely one sample follows another in the original was not measured.

**OpenTPW** (`ParkAudio.SetCrowdVoice`, `NextCrowdSample`, `ParkPeople.GuestsNear`): on the music's beat the guests on the cells within four of `ParkPicking.Cell` are counted (nought with no cell), held to 100 and made nought in world state 4. Above nought the voice is held: a flat sample of kids 91 from the first variation, then, as each ends, one from the variation the level's zone gives (`ParkScreams.NextVariation`, the music's rule), at the voice's own volume and pitch (`ParkAudio.HeldSample`): the voice's first sample takes them from `ParkAudio.Controlled` (the level, and a drawn pitch), and a later one keeps them, a twin's too. Each beat's set gives the playing sample the level's volume and a pitch drawn again, at once, where its variation names parameter 7, and nothing on a twin. At nought the sample is faded over 0.06 s (**a deviation**: the original's `Sound_StopFading` is a x0.6 step, stepped by a worker thread every 100 ms, at detail above 30 and a cut otherwise, "The fade", below; Q509) and the voice let go; started again it begins from the first variation. A sample's volume plays at the channel's own number (`ParkAudio.ChannelGain`): 14 is 10 of 127 beside the music's 60, 0.0550 beside 0.33. **Said:** the next sample is started on the frame after the last has ended, where the original's service pass starts it (rate not measured); the voice that ended by itself in Q236's log is not modelled, a held voice here never ends; the count reads each guest's linked cell (`ParkState.CellOf`), the original the cells' own lists. Console: `crowd`.

## The staff's voices

Fourteen effects of the global `cat_staff` (`cat_staffSFX.map`, bank `Sound\Staff`, `[0x00803a30]`) are named by the
staff's code; where each is asked for is `ride-operation.md`, "Drawn on the way". Each has one variation, no
controller on its volume or pitch (both masks nought), so the volume is the variation's byte and the pitch a draw
from its range (`0x006bbbe0`, "A voice's two controllers" above). Read from the shipped file (`q98/fx`), the weights
turned into shares of 65,535:

| Effect | Asked by | Volume | Pitch | Samples |
|---|---|---|---|---|
| `0xa0` | handyman walking | 24 | -10..10 | `cl_wk03`..`cl_wk06` 8.7% each, `blank44` 65% |
| `0xa1` | handyman idle | 24 | -10..10 | `cl_st01`, `03`, `04`, `05`, `06` 8% each, `blank44` 60% |
| `0xa2` | mechanic walking | 24 | -10..10 | `blank44` 60%, `mc_wk01`..`04`, `mc_wk08` 8% each |
| `0xa3` | mechanic idle | 24 | -10..10 | `mc_st04`, `07`, `08`, `11`, `12` 8% each, `blank44` 60% |
| `0xa4` | entertainer walking | 24 | -10..10 | `blank44` 66.7%, `jj_wk05`, `07`, `08`, `09`, `10` 6.7% each |
| `0xa5` | entertainer idle | 24 | -10..10 | `blank44` 66.7%, `jj_st01`..`jj_st05` 6.7% each |
| `0xa6` | guard walking | 24 | -10..10 | `blank44` 60%, `gd_wk01`..`gd_wk05` 8% each |
| `0xa7` | guard idle | 24 | -10..10 | `blank44` 79%, `gd_st01`, `gd_st02` 10.5% each |
| `0xa8` | researcher walking | 16 | -10..10 | `blank44` 60%, `sc_wk02`, `04`, `06`, `08`, `09` 8% each |
| `0xa9` | researcher idle | 16 | -10..10 | `blank44` 60%, `sc_st02`, `04`, `08`, `09`, `12` 8% each |
| `0x87` | a performance's end | 21 | 0 | `TADA` |
| `0x88` | a guard's chase starting | 30 | -5..5 | `Oi` |
| `0x89` | a guard's catch | 30 | -5..5 | `blank44` |
| `0x8a` | a researching turn | 30 | -5..5 | `blank44` |

`blank44.mp2` is 9 ms of silence, so with the draw's one in sixteen a kind's turn is heard about once in forty.
**What the samples say**: nothing in words. All 46 were put through an offline transcriber (Vosk's small English
model, `q135/listen/transcripts.tsv`), which made no sentence of any: the `_wk` ones run 0.2 to 2.9 s and come out
as strings of syllables ("lam lam lam", "do do do", "whoa whoa whoa"), the `_st` ones 0.2 to 2.3 s and mostly as
nothing or one syllable. No person has listened to them.

**OpenTPW** (`ParkAudio.StaffSound`): a variation evenly, a sample by weight, one placed one-shot on the effects
group at the member's feet, its rate from the drawn pitch (`ParkCarSounds.Rate`), the blank played as any other.
The byte plays at the channel's own number (`ParkAudio.ChannelGain`, "A channel's volume" above): 24 is 18 of 127
beside the music's 60, as the original's staff channels read, and the voice is turned down over its variation's own
range ("The mixer's distance law").

**Measured before the channel's number was built** (`ride-operation.md`, "The staff's sounds, in both games"): in
OpenTPW's mix `TADA.mp2` sat at 0.027 to 0.031, the byte's 0.21 × 0.30 × the master's 0.5, with no distance law. In the original's, with the camera on the entertainer's patrol,
its envelope is 0.022 to 0.032 of the sample's, where Q236b's crowd samples read 0.022 to 0.046 at bytes 14 to 22:
the same scale to within the spread, at that one camera. How the original's level falls with distance was not
measured, and its placed voice matches the sample at 0.24 at best, so the library changes it on the way.

### A script's sounds

Read for Q553 (`park.md`, "The effect subsystem") and held against the running original's End (`q553/orig/run.sh`:
`voices.py`, `mixer.py`; `q553/PREDICTION.txt`).

**`FUN_005573d0`'s types 3 to 9 are one `Sound_PlayEffect( 0, category, effect, x, y, z )` each, with no level set
after it**, the place `FUN_00556b90`'s under `0x200` cut to whole numbers (`__ftol`):

| Type | Category | Global |
|---|---|---|
| 3 | the level's `cat_rides` | `[0x00803a3c]` |
| 4 | the level's `cat_ambient` | `[0x00803a38]` (`FUN_0051ec50`) |
| 5 | the global `cat_rides` | `[0x00803a28]` |
| 6 | `cat_kids` | `[0x00803a24]` |
| 7 | `cat_staff` | `[0x00803a30]` |
| 8 | the global `cat_ambient` | `[0x00803a20]` |
| 9 | `cat_ui` | `[0x00803a2c]` |
| 10 | the thing's own bank, `FUN_004145e0`, nought with none ("RSSE: Tried to trigger a custom e...") | — |

The effect's flags word makes the voice's class as for any caller (`0x006b6774`..`0x006b67f2`, "How the engine plays an
effect"): no `0x4` the one-shot `0x006bd7b0`, looped under `0x8`; `0x4` with `0x10` `0x006be680`; with `0x2`, `0x400` the
music's `0x006be610`, `0x100` `0x006be330`, else the plain class `0x006be090`; without `0x2`, `0x400` the held chain
`0x006bdd30`, else the chain of any variation `0x006bdc60` (below). Lost Kingdom's 81
scripts ask for a sound 198 times (`ADDOBJ` 82, `EVENT` 116; `rselisting` over the `.RSE` corpus, each effect's flags
from its category's file): 116 one-shots with no flag, 45 looped, 2 flat (`cat_ui` 185, and 186 looped too), 20 of the
plain class, 6 held chains (the level ambient's 177 to 180, the staff's 188; below), 4 chains of any variation (the Crazy Ape's 89,
the Lava Fountain's 100 and Eruption's two 100s), 1 of the music's class
(the bus's global ambient 6) and 4 ids their category lacks (`park.md`).

**Measured in the original's End** (q550's scene, `a-voices.log`, `a-mixer.log`): the listener at (481, 200, 10) in the
world's axes; `End.RSE`'s 200 on its sound node 9 at (501, 227, 16) and 201 and each second's 202 on node 10 at (458,
227, 16), 34.41 and 35.68 off, gains 0.8424 and 0.8361 on a far end of 170, channels of 75, 38 and 75 of 127 (100, 50
and 100 times the effects' 0.75; 28, 14 and 28 for the End's first seven seconds or so, the effects turned down for a
reason not read); 186 flat (voice flags `0x0f`) on a 2D channel
at 75; AnimCtrl's three 209 on Endm1's planes, first at (480, 441, 61), (453, 468, 60) and (505, 468, 61), each found
moved at every 0.25 s read and silenced (`0x800`) while more than 100 off; the music's channel at nought (`DIPMUSIC`).

**The chain of any variation** (flags `0x4` with neither `0x2` nor `0x400`; read for Q558). `Voice_MakeAnyChain`
(`0x006bdc60`) builds its voice with `AnyChain_Construct` (`0x006bdcb0`), which stores the vtable `0x0070a390` and keeps
it: the held chain's maker `0x006bdd30` runs the same constructor and then overwrites it with `0x0070a2e0` (`0x006bdde5`).
The two vtables differ only in the destructor, slot `+0x50` (4 against 6) and the child maker, slot `+0x54`: here
`AnyChain_MakeChild` (`0x006b7250`), a 0x50-byte child built by `AnyChainChild_Construct` (`0x006c3a00`, vtable
`0x0070a970`), where the held chain's `0x006b72f0` builds `0x0070a9e0` (`FUN_006c3de0`). The tick (`0x006bdae0`), the
stop (`0x006bd9b0`) and the move are the held chain's ("A held voice is a chain", above), and so is the child's every slot
but `+0x38`, the variation: `FUN_006bc810`, the plain class's draw by weight among all of the effect's, where the scream's
child takes `0x006c3e00`'s zones. `0x006c3a80` asks it with 0, so the rule against the variation before is never applied.
A child is therefore a one-shot of any variation at a volume and a pitch of its own (`0x006c3ae0`), the next made once the
clock passes this one's start plus its variation's wait (`0x006bc2d0`); the child before is pruned without a stop and plays
out. A child sounds where its chain stands: its request's place is a pointer to the parent's `+0x10` (`0x006c3bea`), and
the parent's move, slot `+0x24` (`0x006bda20`), moves every child still in the chain (`FUN_006c3c70`). The parent has no
channel, so `Sound_StopFading` cuts it as `Sound_Stop` does (`0x006bcc71`). All 17 shipped records of this class hold one
variation (FileFormats `sound-categories.md`), so the draw among all is never heard.

**Measured in the original** (`q558/orig/`; uptime 8.2 days, so its park clock ran 1.46 to 1.55 times real time and
the loops were short; the sound clock is wall time and was not touched): the Crazy Ape bought in the stock park, its queue
joined and six guests sent in this game, saved, and loaded by the original (`chains.py`, `mixer.py`). Each loop of its go
made a chain of 89 (`0x0004`) at (430, 0, 240), the place's height second, gone at the loop's `KILLOBJ 10`. Its children
came 513 to 1001 ms apart over 32 gaps, 8 to 10 to a loop, all nine samples heard; the channels at that place read a far
end of 100 and 64 of 127 (85 x 0.75). With the camera away from it the chain stood flagged `0x800`, out of range, and
made no child at all through six loops: where the service leaves such a voice unticked is not read.

**OpenTPW** (`ParkAudio.StartScriptSound`, `RideScript.StartSound`, `SoundPlace`, `SweepEffects`, `StopEffect`): the
seven categories by type, the one-shot (looped under `0x8`), the plain class, the chain of any variation and the held chain played (`ServeScriptSounds`, the land's
own way, "The plain class"), a chain a child at a time (`ServeScriptSounds`, `NextChainChild`: each child of a chain of any variation a variation by
`AnyVariation` with no variation before, each of a held chain by `ParkScreams.NextVariation` at the parameter nought, its own volume and pitch, the next once the frame clock passes its start plus its
wait, a `FADEOBJ` a cut), flat under `0x200`, else placed over the variation's range; each at its variation's own
volume and a pitch drawn from it, a plain voice keeping its first sample's; moved by the sweep on its script's eighth
tick; stopped by `KILLOBJ` and the script's end, faded by `FADEOBJ` (0.06 s, the deviation of "The fade"). An effect its
category lacks, or a node its model lacks, frees an `ADDOBJ`'s record. Counted and not started:
`RIDE_EFFECT_SOUND_CLASS_<constructor>` for the other three classes, `RIDE_EFFECT_SOUND_HELD_CHAIN_NO_KEY` for a held chain whose effect has no key (none shipped), `RIDE_EFFECT_SOUND_ITEM_BANK` for type 10, and
`RIDE_EFFECT_SOUND_NO_PLACE` for a placed sound of a script with no model here, or of no node with no placed thing
(the ferry's 172 and the seaplane's 173, whose scripts run with no nodes). The End here: the same places, distances,
gains and volumes (`q553/PREDICTION.txt`; the console's `scriptsounds`). **Said:** a record a load put back starts
nothing, as the loader `FUN_005597a0` copies it raw; whether a park entered fresh from a file hears its looped script
sounds in the original is not measured (its run loaded into a park whose fountain was already sounding). There is no
voice pool here (Q43): every script voice sounds, where the original's service keeps twelve. A chain out of range makes
its children here and they play unheard, where the original's makes none (Q557). The Crazy Ape's chain here
(`q558/confirm.py`, run4, the final binary): 14 children a loop, each gap the stated wait within 0.007 s, waits 512 to 976
ms, volume 0.3520 (64 of 127) at (430,240,0) over 100, stopped after 14 by `KILLOBJ 10`.

**The held chain of a script** (flags `0x404` without `0x2`; read for Q559, `q559/`). `Voice_MakeHeldChain`
(`0x006bdd30`) is the scream's maker ("A held voice is a chain", above): its first child the effect's first variation,
each later one drawn by weight among the zones of the child before that hold the chain's parameter
(`HeldChainChild_PickVariation`, `0x006c3e00`), a wait after its start, a volume and a pitch of its own. A script's
`Sound_PlayEffect` sets no parameter after it, the constructor zeroes the controllers (`HeldChain_Construct`,
`0x006bdd80`), and the one setter, `FUN_0051bc40`, is reached from a script only by `SETOBJPARAM`, whose 20 uses are the
bus's (`park.md`): so a script's held chain draws at the parameter nought, and every Lost Kingdom zone runs 0 to 100.
Lost Kingdom's users (`rselisting` over the corpus): the gates' `EVENT 4 1 177` as they open (never stopped: each opening
adds a chain, which only the pool's twelve bound in the original, Q43), the four Loudspeakers' `ADDOBJ 4 -1 177..180 10`
for their lives, and the Staff Room's `ADDOBJ 7 -1 188 1` while `VAR_STAFFIN` holds anyone, `KILLOBJ 1` after.
The zones of 177, 178 and 180 name every variation but their own; 179's first two name 3 to 7 alone, and each of its third to
seventh names 1 and 2 once and the others of 3 to 7 twice, so those are drawn twice as often.

**Every park file carries the live voices, and the original puts them back.** `Sound_SaveModule` (`0x0051c350`) writes
the manager's voices after the 84 bytes, and `Sound_LoadModule` (`0x0051c400`, `0x0051c497`) reads them back through
`0x006b5c30`, `FUN_006ba150` and `SoundManager_SerializeVoices` (`0x006b6da0`), whose case 6 rebuilds a held chain
(vtable `0x0070a2e0`). So the stock park's gates' 177 is not started at the entry: it comes back from `Easymode.TPWI`
with its own variation and its saved child, whose sample was variation 8's in all four runs, and its flags never hold
`0x20`. The gate's script itself is resumed open, so it reaches no `EVENT`.

**Measured in the original** (`q559/orig/`, the real screen, `chains.py` and `brk.sh`; predicted first,
`q559/PREDICTION.txt`): the stock park entered, the gates' chain of 177 at (450, 0, 160), key 18, parameter 0, its
children each drawn from the one before (`0x006c3aba`, 14 in runs e and f: never a repeat, gaps 1080 to 3676 ms). A
Loudspeaker bought on (45,20) here and saved before its script turned, loaded by the original (run g): its chain at
(455, 0, 205) came through `0x006bdb1d` with key 18, flags `0x2` and its own variation 2, and its first child was
variation 1 with variation 1's sample, the chain's flags `0x22` after; its next six v4 v5 v6 v1 v4 v2, beside the
gates' restored chain (v6 v1 v7 v5 v3 v8). **A breakpoint set before the sound service's thread exists never reaches
it** (run d): set it once the lobby is up.

**OpenTPW** builds the held chain for a script (`ScriptVoiceClass.HeldChain`, `ScriptVoice.Held`, `Parameter`):
the same children as the chain of any variation, the variation by the zones. It puts back no voice from a park file
(`PARK_FILE_SOUND_VOICES`, at every park read), so the stock park's gates make no chain here where the original's restored
one makes children (Q560). The Loudspeaker here (`q559/confirm.py`, run2): its chain at (455,205,0), first child variation 1,
27 children with no repeat, waits 1079 to 3819 ms, each gap its stated wait within 0.006 s.

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `0x006bdc60` | `Voice_MakeAnyChain` | The maker for flags `0x4` with neither `0x2` nor `0x400` | Decompiled |
| `0x006bdcb0`, `0x0070a390` | `AnyChain_Construct` | The chain's constructor and the vtable it keeps (the held chain's base) | Decompiled; the vtables read side by side |
| `0x006b7250` | `AnyChain_MakeChild` | Slot `+0x54`: a 0x50-byte child, `AnyChainChild_Construct` | Decompiled |
| `0x006c3a00`, `0x0070a970` | `AnyChainChild_Construct` | The child's constructor and vtable: the scream child's but slot `+0x38`, `FUN_006bc810` | Decompiled; the vtables read side by side |
| `0x006bda20` | — | The chain's move, slot `+0x24`: its own place, then every child in the chain (`FUN_006c3c70`) | Disassembly |
| `0x006c3bea` | — | A child's request takes a pointer to its parent's place, `+0x10` | Disassembly |
| `0x006bdd30` | `Voice_MakeHeldChain` | The maker for flags `0x404` without `0x2`: `HeldChain_Construct`, then the set-up (slot `+0x10`, `0x006bbf60`) | Decompiled |
| `0x006bdd80` | `HeldChain_Construct` | The chain of any variation's constructor, its controllers zeroed, then vtable `0x0070a2e0` | Decompiled |
| `0x006bdb10` | `HeldChain_HeadOrFirstChild` | The chain's first child: with a key and no `0x20`, its own variation noughted first (`0x006bdb1d`) | Disassembly; `brk.sh`, run g |
| `0x006bdb50`, `0x006bdbb0`, `0x006bdc10` | `HeldChain_Extend`, `HeldChain_AppendChild`, `HeldChain_Prune` | The tick's two halves and the child appended after the newest | Disassembly |
| `0x006c3a80` | `ChainChild_Start` | A child's variation (slot `+0x38`), sample (`+0x3c`) and time (`+0x4c` plus now) | Disassembly; `brk.sh` at `0x006c3aa3`/`0x006c3aba` |
| `0x006c3e00` | `HeldChainChild_PickVariation` | The held chain's child: the effect's first, else a weighted draw among the zones of the chain's own that hold its parameter | Decompiled; `brk.sh`, runs e to g |
| `0x0051c350`, `0x0051c400` | `Sound_SaveModule`, `Sound_LoadModule` | A park file's sound module: the 84 bytes, then the live voices (`0x006b5c10`, `0x006b5c30`) | Decompiled; runs c to g |
| `0x006b6da0` | `SoundManager_SerializeVoices` | The voice list written or read; on a read, case 6 rebuilds a held chain | Decompiled |

## Node lookup is by id AND a capability flag

`FUN_0044b220` walks the `.MD2` id table for a record whose **id matches** *and* whose **flag word shares a bit** with a given mask, and answers the first such record. Not by id alone. The masks include `0x200` sound, `0x100` particles and `0x400` costume; walk nodes take `0x800` and heads `0x80`. A mask sharing no bit with `0x3da1f83` is swapped for `0x3da1f82` first (`0x0044b226`..`0x0044b22e`); none of the masks above is.

The flag words in the shipped model data agree with those masks bit-for-bit — the masks were derived from the exe and the flag words from the models independently:

| Value | Original name | What it is | Evidence |
|---|---|---|---|
| `0x200` | — | Mask passed for **sound** | Pushed by `FUN_005573d0` as `FUN_00556b90`'s fifth argument, which it hands to `FUN_0044b220` (`0x00556bc0`) |
| `0x100` | — | Mask passed for **particles** | Pushed the same way by `FUN_005573d0`'s particle arms |
| `0x400` | — | Mask passed for **costume** | Pushed by `FUN_00429ee0` and `FUN_00429f60`, which show and hide a node on an advisor model |
| `0x211` | — | id-table flag word marking a **sound node** | 65 uses in shipped models, 20 of them on explicitly sound-named nodes |
| `0x111` | — | id-table flag word marking a **particle emitter** | 454 uses in shipped models: most on effect-named nodes (`smoke`, `steam`, `particle emitter01`), but 108 on `Dummy` nodes, 64 on `Head` nodes and 12 on the park advisors' `Rotate1`-`3` |
| `0x1031` | — | id-table flag word of the `1stperson` camera node | Shipped models |

OpenTPW parses this table (`ModelFile.ReadNodeIds`, `Node.Id`, `Node.IdFlags`) and looks a node up in it as the engine does (`ModelFile.FindNode`, the walk family's through `RideNodes`). Its flag words are the capability bits above.

### Trap: two different meanings for `0x200`

`Node.Flags` (record `+0x00`) uses `0x200` for **"transform-only node"**. The mask tested by `FUN_0044b220` is `Node.IdFlags`, a **different word that reuses the value** and means **"sound node"**. Conflating the two sends you the wrong way.

Related node-record facts: the node **name** is at `+0x54` in every record, the 88-byte transform-only ones included. `+0x50` is **NOT** a second pointer; its low u16 is the record's own index, so do not try to read it as one.

## Emitters in the shipped data

- In-park `levels/space/features/gates.wad` → `gates.MD2`: mesh 1 `Antennae01` (rotated by `gatesm3`), mesh 2 `hatch`, node 7 `ant_emitter` idFlags `0x111` id 3, node 9 **`sound node` idFlags `0x211` id 1**.
- The pattern recurs: `Sound emitter01` on fantasy's speaker wads, `sound node` on every theme's `features/gates.wad`, `sound` on ride carts, `sound nodetemp` on every `Bus.MD2`.

**Nothing in any data file binds a category+effect to an emitter node.** The node says "a sound goes here"; *which* sound was compiled into the exe. That last hop must be chosen, not recovered.

## The lobby was entirely non-positional

`Lobby_Start` plays global effects 2 and 3 at `(0,0,0)`; `FUN_005e1640` plays each park's music and bed (effects 1 and 2) at `(0,0,0)`. **So positional lobby audio in OpenTPW is a deliberate deviation — an improvement on the original, not a restoration. Always say so when lobby audio is reported on.**

| Address | Original name | What it is | Evidence |
|---|---|---|---|
| `005dcfe0` | `Lobby_Start` | Plays global effects 2 and 3, both at `(0,0,0)` | Decompiled |
| `FUN_005e1640` | — | Plays each park's music and bed (effects 1 and 2), both at `(0,0,0)` | Decompiled |

The lobby island `Spa_isle.MD2` has nodes 26 `ant_emitternew` and 27 `ant_emitter`, parented to mesh 9 `Antennae01` (the only mesh either space lobby model rotates) — but **no id record**, so in the lobby they are named positions only. It is the sole model in `lobby.wad` with an emitter node, verified across all eight lobby models: the other three islands carry only `Dummy0..09` (idFlags `0x111`) plus `Spline path`, jungle adds `l_tree1` / `l_tree2`, and all four lobby gates carry nothing but a root `Dummy01`. That is why OpenTPW looks the name up rather than naming Space in code.

## The listener, and what a pause does to it

**`FUN_0051c1d0` is the per-frame listener update.** It was found by following what a pause writes.

It takes **nine dwords — three vectors**. The first is the listener **position**; the other two are orientation (forward and top). It hands them to `FUN_006b5ba0` → `FUN_006b8b60`, which compares the incoming position and both orientation vectors against the stored copies at `obj+0x28` and `obj+4`/`obj+0x10` and acts **only when something changed** — so the pause's write sticks without needing to be repeated.

| Address | What it is | Evidence |
|---|---|---|
| `FUN_0051c1d0` | The per-frame listener update: `(position, forward, top)` | Three call sites, all in `Game_StateMachine` |
| `0x0054e70e` | The **lobby's** call — nine literals: `(0,0,-50)`, `(0,0,1)`, `(0,1,0)` | Disassembled; `0xc2480000` = −50.0f |
| `0x0054f958` | A **park's**, camera-mode mask `0x16` set: the camera globals | Disassembled |
| `0x0054f9e4` | A **park's**, mask clear: the **midpoint** camera↔`0x00790ab8`, lerped by `0.5` at `0x00700f88` | Disassembled |
| `FUN_006b8b60` | Stores it, and only acts on a change | Decompiled |

### The pause substitution

    if ( DAT_00803ad2 != 0 )  position.second = _DAT_00700b28;

`0x00700b28` reads `00 40 1c 46` = `0x461C4000` = **10000.0f**. The original is **Y-up** — the lobby call site's own top vector is `(0,1,0)` — so the coordinate replaced is **height**: a pause lifts the listener **10,000 units straight up**, about eight times the whole map, since a park spans 0..1280 and coordinates reach QMixer unscaled.

`DAT_00803ad2` has **exactly one reader** (`0x0051c1e9`, inside `FUN_0051c1d0`) and one writer, the one-line setter `FUN_0051c1c0`:

| Call site | Passes | In |
|---|---|---|
| `0x004092e8` | 1 | `Game_Pause` |
| `0x00409339` | 0 | `Game_Resume` |
| `0x00409391` | 0 | `Game_TogglePause`, resume branch |
| `0x004093bf` | 1 | `Game_TogglePause`, pause branch |
| `0x00415254` | 0 | `FUN_00415140`, the **savegame loader's post-load rebuild** (`FUN_00414d40`, "Loaded savegame: %s") |

That last one is why the flag is better read as **"the world is not being heard right now"** than strictly as "paused": a load started from the paused menu clears it as it rebuilds the world.

### Which voices the lift takes, and how much

**How much a sound is turned down by that lift is the mixer's law.** The lift decides **which** sounds a pause takes: a listener cannot reach a sound that was never placed against it. The mixer decides **how much**: a listener 10,000 units up is past every voice's range, and the mixer plays nothing of a channel past its range ("The mixer's distance law"; a paused park's channel read a distance of 10,000.32 and a gain of nought). A voice with no range, flag `0x8`, is not shown to be silenced, and neither is a voice with a rectangle, flag `0x1000`, which the service keeps at the listener's own height (`FUN_006bd5f0`).

There is also a genuine **2D path**, so "a 2D sound is just one played at (0,0,0)" is true of `Sound_PlayEffect`'s shape but misleading about the engine: wrapper slot `+0x74` (`0x006d2640`) takes a flag — non-zero calls `QSWaveMixSetSourcePosition` (listener-relative), zero calls `QSWaveMixSetPosition` (**listener-independent**). The choice is made per voice at one place, `FUN_006b7fa0`, from **bit `0x20` of `params+0xc`**, which `FUN_006bc410` sets for a placed voice (`0x006bc5d7`) and leaves clear for a voice with flag `0x8`, which is given bit `0x1` instead (`0x006bc52d`). The music's and the UI's effect records hold `0x200` (music 2 is `0x0606` in all four themes, and all 45 effects of `cat_uiSFX.map` carry the bit), so their voices take the `QSWaveMixSetPosition` path; what the mixer makes of that path under the lift is not read.

OpenTPW reproduces the *which* and the *how much* by another means: `Audio.HoldPlaced` holds every voice that has a place to silence, where the original lifts the listener past their ranges.

### The QMixer wrapper vtable

At **`0x00711920`** — not `0x00711980`, which appears nowhere in the image. The vptr is installed by `MOV dword ptr [ESI],0x711920` at `0x006d1e64` in `FUN_006d1e30`, whose only caller is `FUN_006c4190` at `0x006c423b`. Each slot's target ends in `CALL dword ptr [0x006fd2xx]`, an entry of the delay-load IAT above, which is how the mapping was recovered.

| Slot | Import |
|---|---|
| `+0x18` | `InitEx` / `Activate` |
| `+0x24` | `OpenWaveEx` / `PlayEx` / `ConfigureChannel` / `FreeWave` |
| `+0x28` | `StopChannel` |
| `+0x2c` | `PauseChannel` |
| `+0x30` | `Restart` / `Stop` |
| `+0x60` | **`SetDistanceMapping`** |
| `+0x64` | `SetSpeakerPlacement` |
| `+0x68` | `SetPanRate` |
| `+0x6c` | `SetSpeedOfSound` |
| `+0x74` | **`SetSourcePosition` / `SetPosition`** (3D or 2D, per the flag above) |
| `+0x78` | `GetSourcePosition` |
| `+0x7c` | `SetSourceCone` |
| `+0x80` | `SetVolume` |
| `+0x88`, `+0x98` | `SetListenerPosition` |
| `+0x94` | `SetFrequency` |

## How a voice is decoded

The MPEG voice class's constructor `0x006c82c0` sets its vtable `0x0070ae30`. Its read, slot `+0x1c` (`0x006c83c0`),
calls slot `+0x5c` (`0x006c8630`, at `0x006c8429`), which goes to the decoder's `0x006d1ab0`, a byte copy. Frames
are decoded in `0x006d17b0`: Layer II by `0x006cdf50`, Layer I by `0x006ce5a0`. Both use one of two synthesis
routines, picked by `DAT_00fb2704`, and both write 16-bit samples.

**Each voice is clamped to 16 bits as it is decoded, never wrapped.** The x87 routine `0x006c9270` makes its three
word stores (`0x006c9415`, `0x006c947f`, `0x006c953e`) after `FISTP` and an explicit clamp to `0x7fff` and
`0xffff8000`. Every one of the MMX routine `0x006ca580`'s 96 word stores stores a `PACKSSDW`, which saturates. 336
shipped entries peak past full scale in a floating-point decode (FileFormats `sounds.md`, "Decoding"), so the clamp
takes effect on those entries: ffmpeg's 16-bit decode clips them the same way. How QMixer adds 16-bit voices
together is not in the executable.

OpenTPW does not clamp a voice: `AudioClip` keeps NLayer's floats, up to 1.40, and only the final mix is clamped
(`Audio.Mix`), so a clip past full scale played below full volume is louder at its peaks than the original's.

## The fade, the SoundInfo settings and the reverb

(Fork watch 2026-10-18, `verify-other-a.md` H1/H2/M3 and `verify-audio-fx.md` F1/F2, both read from the disassembly of `testme.exe`.)

**`sound.sam`'s `SoundInfo.*` and `DefaultVolume.*` keys are globals.** `0x007858f0` is a settings object (vtable `0x006fd640`, built at `0x00401030`
from `0x00402ca0`, key table `0x00746a20`, base `this+8` from `0x005b0d60`), filled by `0x00401650` from the file `"sound.sam"` (`0x0045b277`, the
name set at `0x00458da0`). Each key's global is `0x007858f8 + 4n`, n counting from 1 in the **table's** order (`park-engine.md`, "How a key finds
its global"), which is not the file's order:

| Global | Key | Shipped | Use read |
|---|---|---|---|
| `0x007858fc` | `QSOUND` | 15 | `0x0051b920`: detail above it sends `0x700b54` (also REAR 30 and CROSSOVER 40) |
| `0x00785900` / `0x00785904` | `REAR` / `CROSSOVER` | 30 / 40 | the same send |
| `0x00785908` | `EAX` | 10 | detail above it sends `0x700b50`; at or below it the reverb preset is forced to 0 |
| `0x0078590c` | `GEOMETRY` | 20 | not read here |
| `0x00785910` | `MPEGRATE` | 35 | above it sends 0, else 1 (`0x700b4c`) |
| `0x00785914` | `DUCKINGLEVEL` | 38 | **a percentage**: the group level times 38 / 100 (`0x0051bdf9`) |
| `0x00785918` | `DROPPABLELEVEL` | 20 | above it sends `0x700b44` |
| `0x0078591c` | `DEFAULTQUALITY` | 50 | the options constructor's sound detail (`0x004236e9`), passed on at `0x00423850` |
| `0x00785920` | `FADETOGGLE` | 30 | the fade bit, below |
| `0x00785924` | `SOUNDON` | 1 | |
| `0x00785928` to `0x00785934` | `DefaultVolume.SFX`, `MUSIC`, `SPEECH`, `MOVIE` | | the options constructor (`0x004236a5`) |

At boot (`0x0054de0a`..`0x0054de22`) the library's detail is `DEFAULTQUALITY`, or half of it (25) when the options' graphics detail (`+8`) is
nought. So a low-detail start has fading off until the options are applied. Whether a saved `SoundQuality` is applied at boot was not traced.

**The fade.** `FADEOBJ`'s sound arm and `Sound_StopFading` (`0x0051c300`) post `{0x00700bac, handle, 0x3c}` (`0x006b5930` -> `0x006b8930`). `KILLOBJ`
and `Sound_Stop` (`0x0051c2c0`) post `{0x00700ba8, handle}` to the same library slot `+0x18` (`0x006b89b0`, the hard stop).
- `0x006b8930` tests the fade bit `0x400` of `[[this+0x24]+0x28]`. Clear: it calls slot `+0x18`, so the fade **is** the plain stop.
- The bit is written by `0x0051b920( detail )` (`0x0051ba6b`): detail **above** `SoundInfo.FADETOGGLE` (`[0x00785920]`, 30) sends `0x700b48` with 1
  (`0x006b85a0` ORs the bit, `0x006b85b3`), else 0 (`0x006b85bb`). Clearing it turns every fading voice into a stop (`0x006b6240`).
  Stock detail is 50, so **fades are on by default and `FADEOBJ` cuts at detail 30 or below**.
- Set: `0x006b65a0` finds the voice (`0x006b6830`) and calls its slot `+0x30` (`0x006b7130`), which stores the byte 60 at voice `+0x44` and sets `0x400` in `+0x30`.
- A worker thread steps a flagged voice, not the service pass (fork watch 2026-10-19, `verify-other-b.md` H1). Sound init (`0x0051b6b1` -> `0x006b5d00` -> `0x006b5ad0`) calls the library's `0x006b9f40`, which clears the manager byte `+0x50` (`0x006b9f66`) and starts the worker `0x006b9d90` -> `0x006b9e30` (priority -1, `0x006b9f2b`). The worker loops until its stop byte: `0x006ba080` -> `0x006b6260`, which walks the voices and calls each flagged one's slot `+0x2c` (`0x006bcb80`), then `Sleep`s **100 ms if any voice was stepped, 250 ms if none** (scan first, sleep after, so the first step can wait up to 250 ms). The service pass (`0x006b62b0`, one caller `0x006b55e3`) calls slot `+0x2c` only while `+0x50` is set (`0x006b6334`..`0x006b6347`), which `0x006b9f40` has switched off: **only the worker steps a fade.** A byte other
  than 100 becomes a device command (`0x006c50d0`, flag `0x80000`), and the device sets the channel volume to `trunc( byte x eff / 100 )`, `eff` being the
  channel's current volume folded through `0x006bb860` (`0x006c5709`..`0x006c5742`; written back to the channel's `+0x18`).
- So the 60 is **a x0.6 step per worker step, with the group gains folded in at each step**: `own = trunc( 60 x round( own x group product ) / 100 )`, where
  `0x006bb860` multiplies the channel's `+0x18` by 1.0 (or gain[0] under mask bit 1) and by gain[1..3] under bits 2, 4, 8 (the four floats at `[ch+0x34]`; round
  half to even by `fistp`; mask 0 returns the number unchanged). The product compounds every step, so the step count follows the sliders: at product 1.0 the
  volume runs 100, 60, 36, 21, 12, 7, 4, 2, 1, 0 (**nine steps, about 0.9 s** after the first step's wait of up to 250 ms); at the effects' measured 0.75 the
  device gets 75, 45, 20, 9, 4, 1, 0 (**six steps, about 0.6 s**). At 0, or
  with no channel (a held parent, line 98 above), the voice swaps `0x400` for `0x100` (`0x006bcc6e`) and the next service stops it (`0x006b62fe`).
  The names master / music / speech / effects for mask bits 1, 2, 4, 8 and the category masks 9 / 3 / 5 were not re-derived there (unproven).
- **OpenTPW's fade is a deviation:** a flat 0.06 s ramp at every detail (`Advisor.QuietenSeconds`, `ParkAudio`'s rain and crowd fades, `ParkCarSounds`). The
  original's law and the cut at detail 30 or below are Q509. The 90 handed to stop-all (`0x0051bcb0`) when a level ends is probably the same kind of
  percentage; it was not traced.

**The voice pool's tie.** `0x006b6ba0` clears the 31 slots at the start of each pass; `0x006b6bc0` admits a voice only when its rank (`+0x20`) is strictly greater than the lowest,
and places it before the first slot of strictly lower rank. `0x006b6a80` links a new voice at the list head (`+0x24`), so the rebuild sees the newest first
and **a tie keeps the newer voice** (Q43).

**The reverb.** `SETREVERB` (`0x0055649c`) only stores its operand at script `+0xd8`. Every eighth step `0x0055a470` (`0x0042a4e0` gives the ridden
object) finds the script with `+0xc8` equal to it, `+0x10` nought and `+0xd8` set, in the list `[0x008791b0]`, and hands a level of 0 or more to the preset holder
`0x0051c8d0` (a negative one sends nothing); with no match it hands `[0x0078578c]`. The holder keeps the preset at `[0x00803ab4]` (-1 at sound init, `0x0051b700`) and
posts `0x700b30` (`0x006b5c90` -> `0x006ba1b0`) only on a change. `0x006c6000` refuses an index past the count or a record whose word 0 is -1. At detail at or
below `SoundInfo.EAX` (10), `0x0051b920` resets the holder to 0 (`0x0051b9c7`..`0x0051b9f1`).
- **`[0x0078578c]` is the level key `Sound.Reverb`**, entry 255 of the `PeepInfo` object `0x00785040`'s table (`0x0073fc70`; `0x005157cb` loads the theme's `Standard.sam` into it).
  `data/levels/Standard.sam` sets 0 and `hallow/Standard.sam` sets 3; jungle sets none, so Lost Kingdom's reverb off a ride is 0.
- **A slot goes to the listener, not to a voice, and only through EAX 2.0 hardware.** `0x006c46b0` copies the record's 13 words in the order [3], [4], [11], [5], [6], [7],
  [8], [9], [10], [0], [1], [2], [12], then flags 9 (0x38 bytes), and sends them through `IKsPropertySet::Set` (vtable `+0x10`) with the GUID at `0x0070aa50`,
  `{0306A6A8-B224-11D2-99E5-0000E8D8C722}` = `DSPROPSETID_EAX20_ListenerProperties`, property 1. That is the `EAXLISTENERPROPERTIES` layout, so the words are, by index:
  0 environment, 1 environment size, 2 environment diffusion, 3 room, 4 room HF, 5 decay time, 6 decay HF ratio, 7 reflections, 8 reflections delay, 9 reverb,
  10 reverb delay, 11 room rolloff factor, 12 air absorption HF. All three audio backends' slot `+0x50` (`0x0061add0`) answers -1, and the image imports no
  `QSWaveMix` reverb entry, so there is no software reverb. Not checked: which levels shipped scripts pass (the fork lists 0, 1, 3, 8, 9).

## Unknowns

- **`FUN_0051c700` is NOT the distance-mapping feed.** It is the sound-detail ladder that interpolates the three `RadiusInfo[n].MINRADIUS` values (100.0 / 0.5 / 0 at SWITCH 25 / 50 / 75) from `data\sound.sam` and posts them to `0x006b5890` → `FUN_006b99c0` → `FUN_006b8180`, which latches `{on/off, radius}` and walks the voice array setting a per-voice LEVEL. That level comes from `FUN_006c4c80`, a **segment-versus-circle occlusion test that uses only the X and Z components** and ignores height entirely — so the pause's Y-lift cannot touch it either way. It never reaches `SetDistanceMapping`.
- `SndReverb.map`'s slots and 52-byte records are decoded (see the page's first lines); what the 13 values of a record mean and how a slot is applied are in "The fade, the SoundInfo settings and the reverb", above (they are EAX 2.0 listener properties, applied to the listener and not to a voice).
- **What `QMixer.dll` does with the distance mapping is read**: "The mixer's distance law". Its three numbers and their writer are in "A voice's range, and the rectangle it follows the listener in": 2.0, the voice's range and 0.8, from `FUN_006bc410`, through the one call site `0x006c581b` in `FUN_006c5690` (`if ( params->flags_at_0x14 & 0x200 ) vtbl[0x60]( out, channel, params + 0x38 )`).
- **`params+0x14` is a request mask, not a flag pair.** `FUN_006c5690` tests at least nine bits of it, each gating one setter: `0x1` the position path (`FUN_006b7fa0`), `0x2` volume, `0x20` frequency, `0x40`, `0x100` source cone, `0x200` distance mapping, `0x400`, `0x20000`, `0x80000`.
- **A pause silences every voice with a range**: lifted 10,000 units, the listener is past any range, and the mixer plays nothing of a channel past its max ("The mixer's distance law"; a paused park's channel read a distance of 10,000.32 and a gain of nought). OpenTPW holds placed voices to silence, in `Audio.HoldPlaced`. A voice with no range, flag `0x8`, is not shown to be silenced.
