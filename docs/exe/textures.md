# The `.wct` decode

What the original does with a `.wct` file's bytes (the bytes themselves: the FileFormats clone, `formats/texture.md`).
Read in Ghidra on `/testme.exe`, read only (Q354).

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

## The zlib codec, `FUN_00593ab0`, and where OpenTPW's differs

Not built. The multiplier of each value is the table's; nothing else scales it.

- **Values.** The decompressed stream is read straight through, Y's `n x n` values, then Cb's and Cr's `n/2 x n/2`
  (`n` the larger side rounded up to a power of two, at least 8): a signed byte, or `0x80` and a little-endian
  int16 (`[0x00702688]` 256 times the high byte plus the low one). Each times the channel's multiplier.
- **The transform**, per row of `n` values and then per column, into a plane of its own: plain D4, the coefficients
  `(1 + sqrt 3) / (4 sqrt 2)`, `(3 + sqrt 3) / ...`, `(3 - sqrt 3) / ...`, `(1 - sqrt 3) / ...` built at run time
  (`0x00591600`..`0x00591690` into `0x00c27208`..`0x00c27214`; the doubles 3, -1, 0.25, 2, -3, 1 at `0x00702610`).
  With `s` the row's first half and `d` its second, `k` from 0 to `n/2 - 1` and `p = k - 1` wrapping to the last:
  `out[2k] = s[p] h2 + s[k] h0 + d[p] h1 + d[k] h3`, `out[2k+1] = s[p] h3 + s[k] h1 - d[p] h0 - d[k] h2`.
- **Colour.** BT.601 with each chroma value taken for a 2 x 2 block (no blend), rounded (`+ 12582912.0` and the
  float's low bits, `[0x007026ac]`) and held to 0 to 255; stored B, G, R, A.

OpenTPW's `TextureFile` takes type `0x12` down its "half-scale" path: its rows are read by another index map and its
multiplier is times pi/6. Its brightness agrees with the original's; its chroma at a row's ends does not: the first
and last texels of `ridestatbar.wct`'s ramp decode (125, 84, 51) and (125, 218, 99) where the original draws red and
green. `wctorig.py` (`docs/TOOLING.md`) is the codec above in Python: it gives texel 1 (250, 0, 0), 4 (242, 97, 0),
8 (248, 208, 0) and 24 (93, 241, 1), the original's screen at each ((255, 14, 0), (255, 103, 0), (255, 206, 0),
(90, 251, 0)), and the track 56 where the original's is 49 on screen and OpenTPW's 58.

## A sign's artwork (`FUN_005e7990`)

A `.sgn`'s four floats at `+0x0C` are handed to the codec's slot 3 (`0x005987b0` -> `0x00592000` -> `FUN_00592730`,
the same transform and colour) **as they are**, as the multipliers: 6, 10, 2 and 6 in every shipped sign, so Y
times 6, Cb 10, Cr 2. OpenTPW's `SignFile` makes them 16-bit scales and puts them through the header's formulas (4,
3.5, 1.5, 7) and the half-scale path. Through `wctorig.py`, `lobby.wad`'s `Jun_isle.sgn` comes out about 1.4 times
brighter than OpenTPW draws it; neither has been compared with the original's lobby.

## Predicted first (`q354/PREDICTION.txt`)

| | Predicted | Measured |
|---|---|---|
| `ridestatbar.wct`, texels 4, 8, 24 of row 4 (`wctpng.py`) | red-orange to green | (248, 109, 0), (255, 217, 2), (91, 254, 0); were (184, 182, 0), (229, 255, 0), (140, 255, 0) |
| Texel 1 | red | **missed**: (125, 84, 51), the half-scale path (above) |
| The training screen's 80% bar at 0.1 .. 0.7 of its width against the original's frame (`q353/orig/a-train.png`) | within 40 a channel | within 18 at all seven (`run-fix1`, `run-fix2` the final build) |
| Its first 3% | this game's R under 200, the original's over | (82, 40, 8) and (221, 42, 0) |
| The log | `UI: the stat bars' skin ... its ramp (125,84,51) (248,109,0) (255,217,2) (91,254,0)` | as predicted |
