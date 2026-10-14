# The `.wct` decode

What the original does with a `.wct` file's bytes (the bytes themselves: the FileFormats clone, `formats/texture.md`).
Read in Ghidra on `/testme.exe`, read only (Q354, Q355).

## The loader, `FUN_00564c00`, and the alpha chunk

It reads the 28-byte header, refuses a type other than `0x12` or `0x13`, reads the colour chunk, and reads the alpha
chunk **when the bit count (header byte 2) is 32** (`0x00564c72`); the byte at offset 1 is tested and passed over (a
`JNZ` round a `NOP`, `0x00564cc0`), and the version is masked to its low two bits. The alpha chunk is the codec's
sixth argument: none, and the image is opaque. **OpenTPW** decides by the byte at offset 1 (said at its site in
`TextureFile.ReadFromStream`): in 236 shipped files it is 0 over a 32-bit count and an alpha chunk, whose alpha this
game drops (233 of type `0x13`), and `stexture\b_NEWwin.wct` has it 1 with no chunk, which throws here (FileFormats,
`texture.md`). Not built (Q356).

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

A `.sgn`'s four floats at `+0x0C` are handed to the codec's slot 3 (`0x005987b0` -> `0x00592000` -> `FUN_00592730`,
the same transform and colour) **as they are**, as the multipliers: 6, 10, 2 and 6 in every shipped sign, so Y
times 6, Cb 10, Cr 2. OpenTPW's `SignFile` makes them 16-bit scales and puts them through the header's formulas (4,
3.5, 1.5, 7), and decodes them with the zlib codec above. Through `wctorig.py` the four lobby signs come out 1.47 to
1.52 times brighter than OpenTPW draws them, and yellower (`q355/sgn/Jun_isle-main-fix-orig.png`); neither has been
compared with the original's lobby (Q356). The codec moved each sign's mean by -2.5 to -3.9%.

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
