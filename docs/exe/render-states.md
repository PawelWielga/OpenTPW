# Render states

How Theme Park World sets Direct3D render states. It never sets them per material: the engine compiles
a single 32-bit **state word** per draw and applies it lazily against a cached copy, so every visual
difference between opaque, cut-out, graded and additive geometry is one of four constants. Three facts
matter more than the rest. **CULLMODE is set to `D3DCULL_NONE` once at startup and never rewritten** —
nothing in the game is one-sided. **See-through geometry still writes depth** — none of the four
material words disables depth writes. And the choice between a cut-out (hard-edged) and a graded
(soft) alpha reference follows, for a `.wct`, **its alpha chunk and its header byte 1**, and for any other
format a **pixel classifier**, subject to material-load options and the material `0x800` opaque override
described below.

## Device and how a state reaches it

DirectDraw + Direct3D 6 immediate mode. The binary imports `DDRAW.DLL` only; the device comes back
from `QueryInterface`, so there is no `d3dim` import to find. The device vtable is `IDirect3DDevice3`. The executable also carries a software
renderer ("MMX Software Renderer, by Martin Griffiths, 1998/9", `0x006ef770`) and two refusals of the hardware path,
" -- Detected Voodoo1, failing HAL init." (`0x007689d0`) and " -- Detected PowerVR1, failing HAL init." (`0x0076637c`);
which code reads them is not traced (Q194, from the bytes). DirectDraw's two callers are `boot.md`'s step 3.3.

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `+0x58` | `SetRenderState` | Device vtable slot used for every state change | `IDirect3DDevice3` vtable, `call [reg+0x58]` sites |
| `+0x74` | `DrawIndexedPrimitive` | Device vtable slot | `IDirect3DDevice3` vtable |
| `+0x98` | `SetTexture` | Device vtable slot | `IDirect3DDevice3` vtable |
| `+0xa0` | `SetTextureStageState` | Device vtable slot | `IDirect3DDevice3` vtable |
| `FUN_00567620` | — | Applies a compiled state word, lazily, against a cached copy of the current states | Renderer vtable slot 2 |
| `0x00701330` | — | The renderer vtable holding `FUN_00567620` at slot 2 | Disassembly |

## The compiled 32-bit state word

| Bits | What it is | Evidence |
|---|---|---|
| `0x0f..0x00` | `DESTBLEND = word & 0xf`, `SRCBLEND = (word >> 4) & 0xf` | `FUN_00567620` |
| `0x100` | `ALPHABLENDENABLE` | `FUN_00567620` |
| `0x200` | Selects the **high** ALPHAREF of the pair | `FUN_00567620` |
| `0x400` | `ALPHATESTENABLE` | `FUN_00567620` |
| `0x800` | `ZWRITEENABLE = NOT this bit` — the only depth-write control in the engine | `FUN_00567620` |
| `0x8000` | `ZENABLE = NOT this bit` | `FUN_00567620` |
| `0x1000` / `0x2000` | Not render states: together the `0x3000` "exempt from sorting" mask | `FUN_00565590` tests the mask |
| `0x80000` | `SPECULARENABLE` | `FUN_00567620` |
| `0x100000` | `FOGENABLE` | `FUN_00567620`. Cleared on a batch the game fogs itself; the park's own view asks for no fog at all (`park-engine.md`, "The park view's colours") |
| `0x800000` | Overrides the per-batch depth key with 0 | `FUN_00565590` |

## States set once for the whole run

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0056695c` | `CULLMODE = 1 = D3DCULL_NONE` | **Set once at startup and never rewritten anywhere in the program.** Nothing in the game is one-sided; foliage is authored expecting it. Render a park's foliage or fences with backface culling on and they come out half-missing. | All 88 `call [reg+0x58]` sites scanned; no other site writes CULLMODE |
| `0x005669b7` | `ALPHAFUNC = 7 = D3DCMP_GREATEREQUAL` | Alpha test comparison, never changed | Disassembly |
| `0x00573614` | `ZFUNC = 4 = D3DCMP_LESSEQUAL` | Depth comparison, never changed. OpenTPW uses `Less` — **a difference, not yet shown to matter.** | Disassembly |
| `0x00573660` | — | The `EnumZBufferFormats` callback (`FUN_00573080`, `0x005731b3`): keeps the format offered and stops at the first of 16 bits, so the z-buffer is 16-bit wherever the card offers one. At a park's depth 110 a step is about 0.43 units (`park-engine.md`, "What hides a person below their feet") | Disassembly |
| `DAT_007012d8` | ALPHAREF pair table | `{0x1000, 0xf000, 0x10, 0xf0}` | Data at that address |
| `FUN_00567270` | — | Probes the card and selects index 2 of that table, so the two references in use are **16 and 240 of 255** | Disassembly |

## The four state words a `.md2` material can get

Built by `Texture_BlendStateWord` at `0x00590c40`, switching on the engine texture object's flags at
`+8`. `FUN_00581790` is what builds the word for a mesh material.

| `flags & 0x300` | State word | Alpha test | Blend | Depth write |
|---|---|---|---|---|
| none | `0x1256` | off | off | **ON** |
| `0x100` | `0x2756` | on, ref 240 | `SRCALPHA` / `INVSRCALPHA` | **ON** |
| `0x200` | `0x556` | on, ref 16 | `SRCALPHA` / `INVSRCALPHA` | **ON** |
| `0x300` | `0x200552` | on, ref 16 | `SRCALPHA` / `ONE` | **ON** |

**See-through geometry writes depth.** None of the four words sets bit `0x800`.

`0x100` on its own means a 32-bit source texture.

## Pixel classification and material-load overrides

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_00574bd0` | — | On a `.wct` that loaded (`FUN_00564c00`), sets the flags `[0x0087a24c]` without the classifier: `0x24`, `0x25` with an alpha chunk read (cut out: `0x100`), `0x27` with header byte 1 set as well (graded: `0x200`); `textures.md`, "The loader" (Q358) | Decompile |
| `FUN_00575160` | — | The other formats': counts texels whose alpha is strictly between `0` and `0xf0`, and sets the "graded" bit when that count reaches `truncate(float(0.05) * width * height)`; `__ftol` truncates the threshold before the comparison at `0x005755c7`. A 4-byte source sets the cut-out bit whatever its alpha | Disassembly |
| `0x00701720` | — | The `0.05` multiplier used by that test | Data at that address |
| `0x00581820` | — | The only bit of the `.md2` **material** flag word the draw path tests: `0x800`, which forces the opaque word | Disassembly |
| `ProceduralBase_SkirtFlags` (`0x0046aa10`) | — | Sets material `0x800` on each triangle's material of a procedural base's floor, and clears it for material 0 on a footprint cell whose edge mask is nought; on every load that runs it, `Mesh_SetMaterialsOpaque` follows and sets it on them all (`textures.md`, "Which materials the loader makes opaque") | Decompilation (Q361, Q362) |
| `Mesh_SetMaterialsOpaque` (`0x0046a550`) | — | The writer that decides: loader flag `0x2000` (`0x00462c71`) sets `0x800` on every material of the floor mesh `FUN_00469a80` picked, or of the root mesh, and of an item's `P` model's root (`0x00462ca7`) | Decompilation; the original's memory (Q362) |

The inspected draw routine `FUN_00581790` does not read source material `0x2` as an alpha gate.
This is a bounded observation, not proof that the bit is unused elsewhere. Texture flags are
also affected by material loading, so pixels alone do not determine every draw state.

### Independent load-to-draw trace, 2026-10-03

Observed with headless Ghidra on an isolated project copy, x86/Windows, image base `0x00400000`.
The loaded program and installed `testme.exe` report SHA256
`cf0ffd955077eca146d75ee46c45b8a0786fb757a8f7d204b1aed8ec5a1ee4cb`.
The existing live Ghidra project was not opened or changed; the copy closed with `unsaved=False`.
The trace used decompilation plus instruction listings and data reads at the addresses below.

The model loader chain inspected was `0046dcf0 -> 0046dd50 -> 00579e00 -> 00575c60 ->
00575e10 -> 00575870 -> 005755f0 -> 00574bd0 -> 00575160`. `00579e00` supplies source
material records from model `+0x50` in eight-byte steps and frame records from `+0x54` in
sixteen-byte steps. `00575c60` retains those pointers and invokes `00575e10` immediately or deferred.

| Observed operation | Instruction evidence | Limit |
|---|---|---|
| Source flags `0x1000`, `0x2000`, `0x4000` select load options `2`, `4`, `8` respectively | `00575e32`, `00575e49`, `00575e60`, each calling `00575b80` | Which installed or generated materials reach these options was not established |
| Selecting an option assigns cache `+0x10008`; later matching tests overwrite earlier ones | `00575b80`; consumed and cleared in `005755f0` | These options are not ORed together |
| Option `4` bypasses the ordinary alpha-classifier mapping | `00575725..00575734` | This does not name all effects of the option |
| Option `8` ORs texture flags `0x300`, selecting the additive state | `00575754..00575760`, then `00590c40` | Source material flags and runtime texture flags are different words |
| Source low bits `0x10` and `0x20` propagate to texture flags `0x10000` and `0x20000` | `00575ea1`, `00575eb3`; reuse in `00575c60` ORs `0x30000` if either is present | The final high-level purpose of those bits remains unknown |
| Material `0x800` forces state `0x1256`; otherwise texture flags select the table above | `00581820`, `00581826`, `00581830`, `00590c40` | Mesh `0x2` is a separate depth-write control |
| Partial-alpha count is compared with an integer-truncated threshold | `0057557e`, `00575584`, `005755c2` (`__ftol`), `005755c7`, `005755cb`; multiplier bytes `cd cc 4c 3d` at `00701720` | OpenTPW currently compares `partial * 20 >= texels`, which can disagree at the boundary |

**Strong inference:** the inspected original path does not justify treating source material
`0x2` as the alpha-enable semantic used by `ModelFile.MaterialData.IsTranslucent` and
`LobbyModel.MaterialFlagsFor`. OpenTPW took the gate out in Q362 (below).
Prior observations that removing it makes holes do not prove the original bit's meaning.

**Settled since (Q362):** OpenTPW draws a material's alpha by its texture and `0x800` alone, `0x2` playing no part:
`0x800` is the file's or the loader's (`Mesh_SetMaterialsOpaque`, above; `textures.md`, "Which materials the loader makes
opaque"). The gate is gone; Q361's 174 of 174 floor materials are that loader's.

**Settled since (Q359):** the holes were this game's own: a floor plate's clear border shows what lies under the
footprint, and neither game draws ground there (`textures.md`, "What a floor's clear border shows"); the reference is the
texture's header since then.

**Unknown then:** why those holes occur. Texture decoding, another setup path, runtime mutation,
caching and the comparison scene remain possible explanations. This static trace did not
compare matching original/OpenTPW frames or live texture flags. It changes documentation and
comments only. Before altering rendering, independently capture the same disagreeing material's
source flags, runtime texture flags and decoded alpha pixels in both engines at the same draw.

## What actually disables depth writes

For a world sprite, its flag `0x80000`: `Sprite_BuildQuad` (`FUN_00589b50`) ORs `0x800` into the quad's word
(`0x00589e85`); `SpriteList_Draw` (`FUN_0058a770`) ORs one for `0x4000` too, but only into the key it groups by. For a
particle, its record's `0x4000`: `SpriteBatch_DrawParticles` ORs it (`0x0057c7fd`; `park.md`, "A park's particles, started
and drawn"). A person's flags carry neither (`park-engine.md`, "What hides a person below their feet"). For a mesh, bit `0x2` of a **mesh record's first dword**, not the material word of the same value. Conflating the
two sends you the wrong way, exactly like `Node.Flags` versus `Node.IdFlags`. `ModelFile.cs` skips
that dword as "initial mesh data".

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_00581790` | — | Builds the state word for a mesh material | Disassembly |
| `0x005817ba` | — | `TEST AL,0x2` then `OR ESI,0x800` — the mesh-record bit that clears `ZWRITEENABLE` | Disassembly |

Swept across 839 static models / 4924 mesh records: only four distinct flag words exist —
`0x1` (x4585), `0x401` (x331), `0x2` (x5), `0x81` (x3). Bit `0x2` is set on exactly five records, every
one of them a mesh named `heightfield` in a `base.md2`: the four park terrains and the lobby. Every one of those five
records is empty (no vertices, no faces), so no mesh that reaches the screen has depth writes off (a world particle does): `ParkTerrain` passes
the park `base.md2` through `LobbyModel`, and the ground itself is drawn by `ParkGround` from the file's heightfield block.

**Trap when sweeping `.md2`:** an animation file has `meshPtr` (`+0x70`) `== 0` and must be skipped, or
its matrices are read as flag words and come out as float bit patterns.

## Texture addressing: CLAMP, and WRAP where a frame asks for it

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `0x00566ce3`, `0x00566dae` | `D3DTSS_ADDRESS = 3 = D3DTADDRESS_CLAMP` | The device's init (`FUN_00566840`): stage 0 clamped | `PUSH 0x3` / `PUSH 0xc` / `PUSH 0x0` before `CALL [ECX + 0xa0]` (`IDirect3DDevice3::SetTextureStageState`) |
| `0x005678cf` → `FUN_00567a70` | — | The state word's applier (`FUN_00567620`): bit `0x400000` sets stage 0's `ADDRESS` to `1`, `D3DTADDRESS_WRAP`, and clear sets it back to `3`, CLAMP, whenever the bit changes | Disassembly: `FUN_00567a70( word, stage 0, mask 0x400000, state 0xc, set 1, clear 3 )` |
| `Texture_BlendStateWord` (`0x00590c40`), `FUN_00581790` | — | Put `0x400000` in a material's word when its texture's flags (`+8`) hold `0x10000` or `0x20000` (the first only where the renderer is `[0x008bd524]` 1) | Decompile |
| `FUN_00575e10` | — | Loads a frame's textures; after each, the frame's word `0x10` ORs `0x10000` and `0x20` ORs `0x20000` into the texture's flags; its byte 1's `0x20` asks the load for `0x8030000` | Decompile |
| `FUN_00575c60` | — | Called for a frame from a model's load (`FUN_00579e00`, `0x00579f7a`); a frame already loaded has `0x30000` ORed into all its textures where its word holds `0x30` | Decompile |
| `0x004626fb` | — | `FUN_00461f10` ORs `0x8030` into the frame word of every material of a mesh a UV track scrolls, before the same function loads the textures (`0x00462740`, `0x00462771`) | Disassembly |
| `FUN_00575870` | — | The texture cache: one texture per folder and name, counted by its users, so the bits above stay on it for every model using it | Decompile |
| `FUN_00590350` | — | A texture without `0x30000` is packed into a shared page with a half-texel inset (the reason it must clamp); one with it is not | Decompile |

So **a model's texture is clamped unless its frame carries `0x10` or `0x20`, or its mesh scrolls its UVs**, and then
it wraps in both directions, the stage having one address mode for both. The data agrees without exception: of the
12,951 material uses of the 838 static models, 2,573 reach a texture coordinate past 0 or 1 by more than 0.01, and
2,572 of those carry a wrap bit; the other names no texture (FileFormats `models.md`, "Bits 0x10 and 0x20"). The sweep
of every `SetTextureStageState` call: 98 push a literal state, two write `ADDRESS` (the init's CLAMP), none `ADDRESSU`
or `ADDRESSV`; the six through a register are the init block's counter (four) and `FUN_00567a70`; `SetRenderState`
(`+0x58`, 88 calls) is never handed the old address states 3, `0x2c` or `0x2d`.

**What this game does** (Q363): `LobbyModel.WrapFrames` and `Wraps` take the frames with a wrap bit and those of a
scrolled mesh; every other material's vertices carry `LobbyModel.ClampFlag`, and `test.shader` samples them with
`s_ColorClamp`, the clamped twin `Material.Set` binds beside `s_Color`. Lost Kingdom's outer river banks
(`jri_bnk4` to `6`, word `0x3`) now keep their top edge, where a wrapped sample took in their clear bottom row and the
cut-out dropped a thin pale line (`q363/side-by-side.png`: 451 line pixels on main, 38 here, none in the original).
**Deviation, counted:** each material here keeps its own way where the original's shared texture takes the bits any
of its users gave it; a texture path one model wraps and another clamps is counted `TEXTURE_WRAP_SHARED` (five in
the lobby and the stock park: the lobby's `hgt_rai1`, `sen_pnl1`, `sen_pnl2`, `Sen_pnl4` and the jungle's
`m_grass3`). The interface's meshes (`UiMesh`), the ground, the paths and the hoardings keep the sampler their
callers bind, which were not compared.

**The lobby sea.** OpenTPW's lobby sea is a 10,000-unit plane whose UVs the shader computes as `position * 0.05`, so
it runs hundreds of tiles wide; it keeps `TextureFlags.Wrap`. The original's sea is its own mesh with authored UVs.

## Three back-to-front sorts — which OpenTPW does not do

World geometry is FVF `0x1c4` (`XYZRHW`, stride `0x20`), so vertex `+8` is screen Z. The original sorts
at three levels, all back-to-front.

| Address / value | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_00565590` | — | 1. Per-batch depth key = mean vertex Z. State bit `0x800000` overrides it with 0. | Disassembly |
| `FUN_00565590` → `FUN_00565490` | — | 2. Per-triangle sort: `FUN_00565590` sums each triangle's `z0+z1+z2` and, **only when the state word misses the `0x3000` mask**, sorts them descending with the quicksort `FUN_00565490` — i.e. only for the graded (`0x556`) and additive (`0x200552`) words. The opaque and cut-out words carry `0x1000`/`0x2000` and skip it. | Disassembly |
| `FUN_00582ad0` → `FUN_005829e0` | — | 3. Per-batch sort, descending on the key at `+0x30` | Disassembly |

**OpenTPW does none of this sorting.** It does not need it for the lobby: the only graded surfaces
there are the shoreline ripples and the Space dish's cone, each a single layer that does not overlap
another. OpenTPW draws every see-through surface after every solid one (`Level`'s translucent pass), unsorted within
that pass. Parks load and have not been checked for it. **Whether the missing sorts show on screen, in either
scene, has never been measured.**
