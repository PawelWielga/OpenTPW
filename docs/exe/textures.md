# The `.wct` decode

What the original does with a `.wct` file's bytes (the bytes themselves: the FileFormats clone, `formats/texture.md`).
Read in Ghidra on `/testme.exe`, read only (Q354, Q355, Q357, Q358).

## The loader, `FUN_00564c00`, and the alpha chunk

It reads the 28-byte header, refuses a type other than `0x12` or `0x13`, reads the colour chunk, and reads the alpha
chunk **when the bit count (header byte 2) is 32** (`0x00564c72`) into `+0x20`; the byte at offset 1 is tested and
passed over there (a `JNZ` round a `NOP`, `0x00564cc0`), and the version is masked to its low two bits. The alpha chunk
is the codec's sixth argument (`0x00564eb3`): none, and the image is opaque.

**Byte 1 chooses the alpha reference.** The texture's caller `FUN_00574bd0`, on a `.wct` that loaded, sets the flags
`[0x0087a24c]` to `0x24`, to `0x25` when the alpha chunk was read (`+0x20` not nought), and to `0x27` when header byte
1 (`+5`) is set as well, then ORs in the masked version shifted 8; the pixel classifier `FUN_00575160` is not run for a
`.wct` (it is the other formats' path). `FUN_005755f0` turns bit `0x2` into the texture's `0x200` and, without it, bit
`0x1` into `0x100`, so a `.wct` with an alpha chunk draws **cut out** (state word `0x2756`, reference 240) under a byte 1
of nought and **graded** (`0x556`, reference 16) under one set, and one without a chunk opaque (`render-states.md`,
"The four state words"). Of the 236 files whose alpha chunk sits under a byte 1 of nought (FileFormats, `texture.md`),
every one draws cut out in the original.

**OpenTPW** (Q358) reads the alpha chunk by the bit count, so those 236 decode with their alpha (233 of type `0x13`)
and `stexture\b_NEWwin.wct` (byte 1 set, 24-bit, no chunk) decodes opaque where it threw; `Texture` logs each cut-out
texture as it loads (`Texture: '...' reads its alpha chunk by its bit count, header byte 1 nought: N of M texels clear,
K under 240`). Since Q359 the reference is the original's: `TextureFile.Alpha` is the three states above (opaque, cut
out, graded), `Texture.Alpha` takes it for a `.wct` and counts the pixels only for another format (`Texture.Classify`,
`FUN_00575160`'s rule), and a model's material is cut at 240 or graded at 16 by it; a material with `0x2` over a texture
with no chunk draws solid, as the original's state word gives it. **What still deviates: the material's `0x2` gates the
alpha** (`LobbyModel.SeeThrough`). A texture with an alpha under a material without `0x2` draws solid here, counted
once a material use a model load (`MATERIAL_ALPHA_WITHOUT_0X2`, 54 in the stock park with the lobby), because this
game draws no ground under a footprint (below, "What a floor's clear border shows"). Of the jungle's, the lobby's and
the global advisor's `0x2` uses, 99 of 555 change reference (`q359/flips.py`): in the stock park the Belly Bounce's sign
(`gk_sign1`, `gk_sign2`), the toilets' door (`j_wc2`), the Staff Room's roof (`strawroof`) and the advisor's four mouths
go from 240 to 16 (their byte 1 is set; their partly-clear texels are under a twentieth), and his Kiosk hat and the
lobby's `Sea` draw solid. Each model logs the materials whose texture's alpha and `0x2` disagree (`<model>: N
material(s) whose texture's alpha and 0x2 bit disagree - mesh:material Alpha, ...`), as `q359/predict.py` predicts
them from the files. The path tile `jpa_tju2` is drawn by `ParkPaths` with no flag, opaque. The interface keys a
texture's black out before any of this, so `ipan.wct` (the training screen's rows, the message box) draws the same
either way.

**The river's banks.** Each river mesh of `levels/jungle/terrain/base.MD2` draws `jri_bnk4` to `6` under `0x2`; their
river side is clear (`jri_bnk4`: 962 of 16,384 texels, its bottom row all clear, its top row opaque). Before Q358 the
banks were opaque and drew a black seam between the water and the near bank; the original under Proton, given the same
file, draws none, and neither does this game now (`q358/side-by-side.png`). This game binds a model's textures with
Wrap (`Material.Set`'s default) where the original's addressing is CLAMP (`render-states.md`): the bank's opaque top
edge blends in its clear bottom row and the cut-out test drops it, so **a thin pale line runs along each bank's outer
edge** here, which the original does not draw (filed, Q360).

## What a floor's clear border shows

Every ride's, shop's and sideshow's model opens with a floor plate as wide as its footprint (`park.md`), and most of
those plates' textures carry an alpha under a material without `0x2`: a run into the stock park loads 54 such uses,
six of them the lobby's Space island (`m_grass1` and `m_grass2` at the Belly Bounce, the Jungle Spray's `mj_grass*` and `ss_gplnk*`, the
toilets' `J_wc1`, the terrain's `dk_water3`...). Their border is painted grass at alpha nought (`m_grass1`: 3,328 of
16,384 texels clear, its top and left edges), so the original's state word drops it, and the original's frame of the
same file shows green there, the dirt meeting the park's ground (`q359/orig/a-2-full.png`, `orig-crops.png`). **Drawn
here by the texture alone, the same border shows the sky's clear colour** (`q359/cmp-bellyclose.png`): this game draws
no ground under a footprint (`ParkObjects.CoversGround`), so nothing lies under the clear texels. That is why the
`0x2` gate stays (Q359), and what the old note "removing the gate exposed holes" was.

**The original marks those cells too, and what it draws there is not read.** `FUN_005365d0` (and `FUN_005366e0`) ask
`FUN_00535dd0` for a cell's ground tile, which answers 8 for cell types 2, 4, 7, 9, 10, 21 and 30 (a built thing's
footprint is 4, 9 and 10); `FUN_00450470` hands that to `FUN_0046df50`, which ORs `0x101` into the heightfield cell's
flag word (`[[0x007a0854] + 0x6c] + 0x2c`, four bytes a cell, the flag word low and the ground's texture index high)
and clears `0x80`, leaving the texture index. Read in the running original (`original/groundcell.py`, `q359/orig/b-load.log`):
ten of the Belly Bounce's twelve cells hold flags `0x0303` to `0x034b` and texture words 11 down to 0, one a cell, and
its two middle cells `0x0103` over ground textures 27 and 57; open grass holds `0x0002` or `0x0102`. Whether the
ground's drawer skips a cell by `0x1` is not read: its drawer is not found (`FUN_0045bf90` is the terrain's hit test,
which skips a cell with bit `0x2`; `FUN_0044e5b0` sets a flag nothing reads), and a cell's words written into the running
original after a load changed nothing on screen, its path tile copied onto grass included (`orig/c`, `orig/d`): the ground
is built at a load. So either the original draws ground under a footprint or its floors' clear texels are drawn, and
the frames at this distance cannot tell green ground from the plate's own painted green.

## The header's scales, and the order they are handed on (`0x00564e20`)

The texture loader turns the header's four 16-bit scales into multipliers and hands them to the codec object at
`[0x00879a64]` (vtable `0x007026b8`, slot 2, `0x00592060`: type `0x12` to its primary vtable's slot 0, `FUN_00593ab0`,
the zlib codec; `0x13` to slot 2, `FUN_00595920`, LZSS). From the header (offset 0 the type byte):

| Offset | Multiplier | Handed on as |
|---|---|---|
| 8 | `1 + n x 0.5` (`[0x007011b8]` 1.0, `[0x007011c4]` -0.5) | the first channel's, Y |
| 12 | `1 + n x 0.25` (`[0x007011c0]` -0.25) | the second channel's, **Cb** |
| 10 | `1 + n x 0.25` | the third channel's, **Cr** |
| 14 | `n + 1` (`[0x007011bc]` -1.0) | the alpha's |

**The scale at offset 10 is Cr's and the one at 12 is Cb's.** Both codecs decode the second channel with the
second multiplier and convert it with 1.772 into blue (`FUN_00593ab0`: plane `0x00a667f8` times `[0x00702694]`;
`FUN_00595920`: `0x00f34a48`), the third with the third and 1.402 into red (`0x00caf228` times `[0x00702698]`;
`0x00cf0a28`). The same order is in the loader's second path (`0x00564ef0`, from `0x00564f21`).

**OpenTPW** (`TextureFile.ReadFromStream`) reads offset 10 into Cr's scale and 12 into Cb's. In `ui.wad` this changes
442 of the 448 textures whose two scales differ (the six left alike are flat or grey) and none of the 473 whose two
are equal (`q354/sweep.sh`). `ridestatbar.wct` (Y 6, offset 10 = 10, offset 12 = 2) decodes red to green, as the
original draws it (`hud.md`, "The stat bars' skin").

## The zlib codec, `FUN_00593ab0`

Built (Q355: `TextureFile.DecodeZlibCodec`). The multiplier of each value is the table's; nothing else scales it.

- **Values.** The decompressed stream is read straight through, Y's `n x n` values, then Cb's and Cr's `n/2 x n/2`
  (`n` the larger side rounded up to a power of two, at least 8): a signed byte, or `0x80` and a little-endian
  int16 (`[0x00702688]` 256 times the high byte plus the low one). Each times the channel's multiplier.
- **The transform**, per row of `n` values and then per column, into a plane of its own: plain D4, the coefficients
  `(1 + sqrt 3) / (4 sqrt 2)`, `(3 + sqrt 3) / ...`, `(3 - sqrt 3) / ...`, `(1 - sqrt 3) / ...` built at run time
  (`0x00591600`..`0x00591690` into `0x00c27208`..`0x00c27214`; the doubles 3, -1, 0.25, 2, -3, 1 at `0x00702610`).
  With `s` the row's first half and `d` its second, `k` from 0 to `n/2 - 1` and `p = k - 1` wrapping to the last:
  `out[2k] = s[p] h2 + s[k] h0 + d[p] h1 + d[k] h3`, `out[2k+1] = s[p] h3 + s[k] h1 - d[p] h0 - d[k] h2`.
- **Colour.** BT.601 with each chroma value taken for a 2 x 2 block (no blend): B = Y + 1.772 Cb (`[0x00702694]`),
  G = Y - 0.344136 Cb - 0.714136 Cr (`[0x007026b4]`, `[0x007026a0]`), R = Y + 1.402 Cr (`[0x00702698]`), each sum
  rounded to nearest (`+ 12582912.0`, `[0x007026ac]`, and the float's low bits) and held to 0 to 255 (`packssdw`,
  `packsswb` round a bias of `0x80`); stored B, G, R, A. With no alpha chunk the alpha is `12583167.0`, 255.
- **The decompressor** (vtable `0x00703128` slot 2, `0x00619380`) refuses a chunk that does not start `BILZ`
  (`0x006193d0`), so the codec then decodes whatever its buffer held. Three type `0x12` files are LZSS (FileFormats,
  `texture.md`); OpenTPW sends them to its LZSS decompressor, which throws on all three, as on main.

**OpenTPW** matches it: of the 986 type `0x12` files with a zlib chunk, 983 decode within 1 a channel of `wctorig.py`
(`docs/TOOLING.md`, the codec in Python; 827 to the byte), and the other three differ in the alpha alone, the loader's
rule above (`q355/compare.py`). `ridestatbar.wct`'s texels 1, 4, 8 and 24 are (250, 0, 0), (242, 97, 0), (248, 208,
0) and (93, 241, 1), the original's screen at each ((255, 14, 0), (255, 103, 0), (255, 206, 0), (90, 251, 0)), and
its track 56 where the original's is 49 on screen (`hud.md`, "The stat bars' skin"). Its LZSS path (type `0x13`) is
unchanged: all 7,406 that decode are the same to the byte as before. Before Q355 type `0x12` went down a
"half-scale" path whose rows were read by another index map with the multiplier times pi/6: right in brightness,
wrong at a row's ends and blurred inside: every zlib texture changes (mean 11.7 a byte), and the gadget's buttons, smeared
and doubled before, are crisp as in the original's frame (`q355/gadget-orig-fix-main.png`).

## A sign's artwork (`FUN_005e7990`)

Type 4, the only one shipped, reads six dwords after width, height and type: four floats, then the two chunk sizes. The
colour chunk goes to the codec's slot 3 (`0x005987b0` -> `0x00592000`, its colour flag set -> `FUN_00592730( w, h,
data, out, v0, v1, v2 )`, `0x005e7cb9`) and the alpha chunk to the same slot with the flag clear and `v3`
(`0x005e7ced`). **The floats are the multipliers as they are**, no formula: `FUN_00592730` reads the Y plane times
`v0` into `+0x2c`, the second plane times `v1` into `+0x1c` and the third times `v2` into `+0xc`; `FUN_005935a0` takes
`+0x1c` times 1.772 into blue and `+0xc` times 1.402 into red. So Y times 6, Cb 10, Cr 2 and the alpha 6, in all 23
shipped boards. Its rows and columns go through `FUN_00592b80`, `FUN_00592c50` and `FUN_005933a0`, not compared with
`FUN_00593ab0`'s instruction by instruction; the decode below agrees with the screen.

**The board holds the artwork with red and blue exchanged.** `FUN_005935a0` writes each texel blue, green, red;
`FUN_005e7990` puts the alpha before them (`0x005e7d03`: A, B, G, R); the sign's board (`FUN_005ecb40`, its mode 2)
packs a texel as `byte0 << 12 | byte1 << 8 | byte2 << 4 | byte3`, A4R4G4B4, so the codec's blue is drawn as red and
its red as blue. The inks go through the same packer with their file byte 0 in byte 1, which is why a line's ink is
read red, green, blue, opacity. The board's mode 1 copies the bytes as they are; which mode a board takes is not read.
Witnesses: the Windows screenshot `content/ReferenceScreenshots/ParkPicker/ParkPickerReference.jpg` shows Wonder Land's
board pale cream, the exchanged decode (the codec's own order is pale cyan); the original under Proton draws Lost
Kingdom's park gate blue-grey (the codec's order is brown). Under Proton the original letters no name on that board
(the Windows screenshot's board has its name; Wine's GDI fonts are not looked into).

**OpenTPW** (Q357): `SignFile` hands the four floats on as the multipliers, and `SignTexture.CropBoard` exchanges red
and blue as it cuts the board. All 23 boards decode within 1 a channel of `wctorig.py` (`q357/allsgn/sweep.py`), and
all 8,392 `.wct` files that decode are the same to the byte as before. Each board's mean colour is logged as it is
painted: Lost Kingdom's gate and island (64.3, 67.5, 82.6), Halloween (41.7, 34.7, 38.8), Wonder Land (232.1, 240.1,
178.3), Space (51.1, 27.7, 98.0), where the header's formulas and the codec's order gave the gate (56.0, 44.1, 45.2).
Through the alpha's 6 where the formula gave 7, four boards (the three `sign1` boards and the Jelly's) lie a shade
under opaque throughout (232 to 254); the eleven other solid ones are 253 or 254 on 2.3% of texels where they read
down to 77. The gate seen from inside the park (`camera 480 140 80 180`) measures (24.4, 30.5, 42.4) on its board's
plain stone here, (22.3, 23.8, 29.2) on main, and (25.8, 27.1, 39.0) in the original (`q357/side-by-side.png`,
`board-close.png`). This game's slab top beside it is 1.27 to 1.34 times the original's (not looked into: the light or
the model's texture).

## Predicted first (`q354/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| `ridestatbar.wct`, texels 4, 8, 24 of row 4 (`wctpng.py`) | red-orange to green | (248, 109, 0), (255, 217, 2), (91, 254, 0); were (184, 182, 0), (229, 255, 0), (140, 255, 0) |
| Texel 1 | red | **missed**: (125, 84, 51), the half-scale path (above) |
| The training screen's 80% bar at 0.1 .. 0.7 of its width against the original's frame (`q353/orig/a-train.png`) | within 40 a channel | within 18 at all seven (`run-fix1`, `run-fix2` the final build) |
| Its first 3% | this game's R under 200, the original's over | (82, 40, 8) and (221, 42, 0) |
| The log | `UI: the stat bars' skin ... its ramp (125,84,51) (248,109,0) (255,217,2) (91,254,0)` | as predicted |

## Predicted first (`q355/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| The 986 type `0x12` zlib files against `wctorig.py` | within 1 a channel | 983; 3 off in the alpha (**missed**: the loader's rule) |
| The 7,407 type `0x13` against main | the same bytes | 7,406; `b_NEWwin.wct` throws in both |
| The 3 type `0x12` LZSS files | decode | **missed**: they throw in both builds |
| The lobby's signs | under 5% darker or brighter | -2.5 to -3.9% |
| The training screen's log line | `its ramp (250,0,0) (242,97,0) (248,208,0) (93,241,1)` | as predicted (`run-fix2`, the final build) |
| Its 80% bar's left end | R over 200, within 45 of the original's (221, 42, 0) | (254, 4, 4); main's (82, 40, 8) (`run-control`) |
| Each bar's mean r - g | nearer the original's than Q354's | 16.3, 183.2, 60.3, 110.2 (original 9.4, 179.4, 56.4, 104.9) |

## Predicted first (`q357/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| The five boards' log lines (round 2) | `wctorig.py`'s means, red and blue exchanged | as predicted, each exact (`run-fix2`, the final build) |
| The gate's board, round 1 (the floats alone) | (32.9, 36.4, 41.5), the whole board's ratio | **missed**: (29.8, 30.5, 35.7): a patch moves by its own texels, and the screen adds a constant |
| Against the original, round 1 | R and G within 10%, B 10 to 25% under | **missed**: B 30% under: the board's exchange, not foreseen |
| The gate's board, round 2 | (24.7, 30.5, 41.8) within 8% | (24.4, 30.5, 42.4) |
| Against the original, round 2 | each channel within 15%, B / R over 1.4 | (0.946, 1.125, 1.087), B / R 1.74 (the original's 1.51) |
| Main's build | no board line, (22.3, 23.8, 29.2) within 5% | as predicted (`run-control`) |

## Predicted first (`q358/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| `wct2raw` over the 8,396 shipped `.wct` | 236 change, in alpha alone; `b_NEWwin.wct` decodes | as predicted; 8,393 decode where main's 8,392 |
| The log's lines in the stock park | `jri_bnk4` 962 clear and 1,230 under 240, `jri_bnk5` 1,888 and 2,145, `jri_bnk6` 1,033 and 1,258, `ipan` 5,273 and 6,008, `Sen_ant1` 3,199 and 3,682; none from main | as predicted, and eight more the census names (`run-fix-7`, the final build) |
| The river's left bend, `camera 480 465 110 180`, share under 45 a channel | under 0.012; main's over 0.025 | 0.0058; main's 0.0363 |
| The Staff Training Budgets screen against main's | under 60 pixels differ | 28, at its bottom edge |
| The near bank's seam, the original's frame of the same file (`seam.py`) | round 1: the bend; rounds 2 to 5: a level, a share, a run under 15 | **missed four times**: its view frames the long reach; it is lit darker; its rocks' outlines touch the water; its run was 15 |
| The same, round 6: longest run of seam columns | the original under 25 (its four longest under 60); the fix under 10, main over 25 | 15 (6 + 8 + 9 + 15 = 38); 4; 51; on the final build (`run-fix-7`, `orig/g`) 15 (38) and 4 |

## Predicted first (`q359/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| The log's disagreeing materials in the stock park, against `predict.py` | round 1: every line; round 2: 19 of 19 | missed 3 of 19 (the predictor's list lacked two terrain meshes and the global advisor); then 19 of 19, three times |
| The texture by itself, the floors (`cmp-bellyclose.png`) | the park's ground shows through the borders | **missed**: the sky shows; the original's frame shows green (P6, held) |
| Two pokes of the original's cell words (`orig/b`, `orig/c`, `orig/d`) | a hole or a path tile | void twice: the ground is built at a load |
| The narrowed rule: `MATERIAL_ALPHA_WITHOUT_0X2` | 54 | 54 (my check's regex missed it once) |
| Fix against main, two close views, guests boxed out | no floor border; under 1,500 pixels | 681 (the sign), 629 (the roof, the doors) |
| The Staff Room's eaves in the original (`cmp-roof.png`) | a smooth fringe, as the fix's, not main's sawtooth | as predicted, at a quarter of this game's resolution |
