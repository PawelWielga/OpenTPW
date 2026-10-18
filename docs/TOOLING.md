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
| `lib/parkrun.py` | The scaffold of every save-and-load confirm run: a private game folder, `enter()`, `save()`, `load()`, `reply()`, `gaps( pattern )`, `shot()`, `predict()` and `said()`, `done()`; `OFFSCREEN = True` for a run beside the desktop's, on `:78` or `OFFSCREEN_DISPLAY` (a two-lane run's lane B takes `:79` and `GAME_FOLDER = "~/.cache/tpw-harnesses/lib/game-off-B"`); `GAME_FOLDER = "<path>"` for a game folder of the script's own (`lib/gamefolder.py`); `AUDIO_FILE = "<path>"` writes the game's own mix to a file, nothing to the speakers; `original_view( x, y )` for a saved camera the original's frame will show the point in |
| `lib/gamefolder.py` | A private game folder whose data differs from the real game's in the files named: `make( dest, { "data/Advisor/Advisor.sam": change } )` links everything else and writes a changed copy of each file (`once( old, new )` is a change that fails unless `old` is there once); hand the folder to `parkrun.py` as `GAME_FOLDER`, on the desktop or off it. For a balance key changed in this game without touching `/home/alex/Games/TPWorld` (Q338) |
| `lib/mixmatch.py` | One sample measured in OpenTPW's own mix (`AUDIO_FILE`): `template( sdt, index )` decodes a bank's entry, `window( file, from, to )` cuts the capture between two sizes, `match()` is the best match and its least-squares gain, `steady()` the gain of a looped sample under other sounds, from 0.3 s pieces in 2 s steps |
| `lib/parkscene.py` | exec'd after `parkrun.py`: the one-line census readers (`var`, `thing_line`, `staff_line`, `bumper_line`, `field`), the Hot Pot scenes (`hot_pot`, `in_a_go`, `broken_pot`), an advisor tag's rect and the window point to click it from `tags`' reply (`tag_rect`, `tag_click`; Q415), and the pointer's: `pointer()` and `pick()`'s lines, `at( fx, fy )` a share of the window, `find_person( want )` the pointer stepped round the middle until `pick` names a person (a locator's camera keeps its person there), `ctrl( k )` (Q451), a person's pick circle in window pixels, `circle( thing )` and every drawn person's `circles()` (the console's `circle`, Q468), `wait_log( pattern )` for a game line that reaches run.log a moment late (Q468), and `find_cell( (x, y), sx, sy )`, the window pixel near a start that `pick` answers with a named map cell, the hover's own (Q471, Q478), so a confirm script does not write them again |
| `lib/stockscene.py <outdir> <bin> <dest.TPWS> <TICK> [command ...]` | A scene file made from the stock Lost Kingdom park, off-screen: entered, paused on the first tick at or past TICK, each console command given in turn (`sell 13`, `buy 1101 41 22 0`), the rides, shops and sideshows listed and the park saved to `<dest.TPWS>`. A stamp or a flag is then written into a copy with `parkfile.thing_set` |
| `lib/pathscene.py <outdir> <bin> <dest.TPWS> <TICK> <N> [X0 Y0 W [H]] [-- command ...]` | A scene file of the stock Lost Kingdom park with more developed cells, off-screen: paused on the first tick at or past TICK, `path X Y` for the first N cells of the W x H block at (X0,Y0) (default 19 x 20 at (60,15), bare grass; its column 79 holds track cells), each with a 0.15 s wait, then each command after `--` (`fire 28`, the guard), saved: 126 + N developed (`parkfile.py -d`). Count the file, not the replies; never run it beside `mutate.py` (late replies cross) |
| `lib/cellprobe.py` | exec'd after `parkrun.py`: the window point over a named map cell, found by moving the real pointer and reading `strip`'s drawn census while a tool shows one square under it (`find( (x, y), start )`, `probe()`, `drawn()`, `squares()`), so a real click lands on a cell the original's run used (Q433) |
| `lib/cropgrid.py IMG CX CY HALFW HALFH OUT [--scale 8] [--step 10] [--mark Y,...] [--markx X,...]` | A frame's few pixels made countable: the box cut, enlarged without smoothing, a red rule every STEP frame pixels labelled with the frame's own coordinates, and a cyan line at each predicted row and column, to read a sprite's drawn top, bottom and edges against the marks in either game's frame (Q470, `q470/confirm.py`) |
| `lib/bothcam.py X,Z ZOOM ROT [PID]` | One camera in both games, by the original's numbers (`camera.py`'s point, zoom and rotation in degrees): `console()` is this game's `camera X Z ZOOM -ROT` (its yaw is the original's rotation negated), `ours( reply, ... )` sends it from a confirm script after `parkrun.py`, `original( pid, ... )` runs `setcam.py` with the follow stopped, `both()` does the two; the command line prints the console line and, given a PID, sets the original. At the same numbers this game's 1280 x 720 pixel is (640, 360) + (the original's - (512, 384)) x 720 / 768 (Q440, Q441); `to_original( x, y )` is the way back (Q478) |
| `lib/parkfile.py` | The one reader of a park file: `cells( p )`, `hoarding( p, x, y )` and `show_map( p, X0, Y0, X1, Y1 )` (every cell's map type and track record, whether a cell is a hoarding, a character map of the cells by type, the hoardings marked; `parkfile.py -c <file> [X0 Y0 X1 Y1]`; Q469), `object_days( p, thing )` (an object's customers ring, State of repair and remaining life, and the all-items row's customers and bars of them, Q367; and its shops' and sideshows' columns: the takings less the costs rings' last thirty, the totals' difference, the satisfaction and its bar, Q368; `-o` prints every object's), `advisor()` (the advisor thing's message records not fresh; Q313), `advisor_forget( src, dst, message )` (a copy of a jungle file whose advisor has never said a message, its block packed again; Q316), `advisor_slots()` and `advisor_clocks()` (his eight slots, the messages waiting to be said, and his last action, kept message and message playing; `-a` prints all three; Q314), `clock()` (the clock module's two dwords, the clock and the second stopwatch; `-a` says how much of the advisor's wait the file was saved with; Q315), `Park` (things, cells, sprites), `track()` (the track-rides module, cars and riders), `models()` (every model record: item, flags, node words, lookup records, channels), `head()` (a sprite), `emitters()`, `emitter_words()` and `emitter_aim()` (the particles module's live emitters by their chains, where one is not its template, and its own velocity at `+0x38`; `parkfile.py -e <file>` prints them), `tags()` (the interface block that ends the body: each aerial tag's id, text, life, picture, buttons, thing and item, with the bytes the original leaves unwritten; `parkfile.py -t <file>...` prints them), `thing_set( src, dst, thing, offset, fmt, value )` (a copy of a file with one field of one thing's record written over and the block packed again, answering the old value: how a state no screen of either game reaches is put in a file; Q325), `analyser()` and `filetime( tick )` (the park analyser's last 76 bytes by the serialiser's names, its six calendar stamps among them, `mLastStaffHired` at the record's `+73524`, and the calendar on a tick as such a stamp; `parkfile.py -s <file>...` prints them beside the staff's newest `mTimeHired`; Q333), `sets()` (the message centre's 29 listener sets, who hears each message type, as a load puts them back; `parkfile.py -x <file>...` prints them; Q397), `vehicles()` (the arrival vehicles: the header's three slots and current vehicle, the object list, cell (0,0)'s chain and each slot's thing's record by field and raw; `parkfile.py -v <file>...` prints them; Q342), `lab()` (the research lab's record: the used projects, the five kinds' records, `mNumProjects`, `mWorkLoad`, `mMostExpensiveProject`, `mLabActive`; `parkfile.py -l <file>...` prints them, a `restart.INTS` as well; Q349), `staff_patrols()` (every member's patrol corners, `+476` and `+478`, with the whole map and no area named; `parkfile.py -p <file>...` prints them; Q380), `pool()` (the staff pool's 32 slots by field; `parkfile.py -P <file>...` prints those in use; Q422) and `pool_set( src, dst, kind, valid=0 )` (a copy with `mValid` written on every slot of that kind: a pool with nobody of a kind to hire, which the advisor's row 17 asks for; Q422), `money_rings()` (the analyser's eight month money rings, the bank's balance at each close (no month so far), all
costs, staff costs, training, cash in, gate, shops' and sideshows' takings: slot, length, wrapped, the month so far, the newest
and the closed months, from the record's `+62598`, 589 bytes a ring; `parkfile.py -m <file>...` prints them; Q385,
Q386, Q387), `park_value( p, prices )` (the park value as `FUN_004c7db0` sums it from the file alone: the cells by type
and track record, the objects by their tiers' costs and repair; with `itemcosts.prices()` it is the analyser's 2nd ring's
figure; `parkfile.py -w <file>...` prints the cells and objects; Q388); `money_rings` also answers the 2nd ring as
`value` and the 11th, the lifetime visitors at each close, as `visits`, beside `analyser()`'s `mLifetimeVisitors` (Q389), and the 10th and 12th, the guests inside and the rides standing at each close, as `inside` and `rides` (Q390), and the 13th, the guests the arrival vehicles dropped, with its month so far, as `arrived` (Q391), and the 14th, the mean happiness of the guests inside at each close, its months read as FLOATS, as `happy` (Q392), and the 15th, the mean of the last fifty leavers' stays at each close, as `stay`, beside `stays()` (the ring of fifty stays itself at the record's `+72022`, oldest first, and the mean the close would push; `-m` prints it as `leavers`; Q393), and the 16th, the park score out of 100 at each close, as `score` (Q394; `q394/sweep.py`'s `score()` and `facts()` reckon it from a file); `loads()` (the arrival manager's ring of five loads at the record's `+72235`, oldest first, with how many are called and the raw five, `0xcdcdcdcd` where none reached; `-m` prints it as `loads`; Q396); `developed()` (the map's developed cells as `FUN_004c81b0` counts them, every cell of `mType` 1, 3, 4, 9 or 10 inside the park or out, with the cells by type; `parkfile.py -d <file>...` prints them; 126 in the shipped jungle park; Q411) and `bare_blocks( p, w, h )` (every block of bare grass inside the park, nearest a path first: where a scene can lay path); `guests_inside( p, below=30 )` (the guests the analyser counts inside, model 1 on a cell of `mType` 0, 1, 3, 9 or 10, and those under `below` happiness, the record's `+422` cut to its low byte, as the advisor's row 14 asks; with the entertainers; `parkfile.py -g <file>...` prints them with the share and the crowd; Q415). `litter()` (the litter share as the advisor's `FUN_004ccdf0` takes it: the littered cells over the cells of path, queue or type 9 or littered; `parkfile.py -L <file>...` prints it with the littered cells) and `litter_set( src, dst, cells, kind )` (a copy with `mLitter` written on each cell, the sprite handle left nought, so neither game draws it; Q417). Add a module's reader here, not in an item's folder |
| `lib/itemcosts.py [theme...]` | Every item's three `Upgrades[k].CostOfUpgrade` from the game's own `.sam` files (`wadcat --cat .sam` over each theme's wads), `{Info.Id: [tier 0, 1, 2]}`, cached in `itemcosts.json` beside it; `prices()` answers it (Q388) |
| `lib/uitypes.py` | Every item's `Info.WhichUIType` across the four themes, `{Info.Id: kind}` (0 rides and upgrades, 1 shops, 2 sideshows, 3 features, 4 the items no buy list shows): the folder's category unless the item's own `.sam` sets one, cached in `uitypes.json` beside it; `types()` answers it. A park file's rides, as the analyser's `FUN_004c8070` counts them, are its model-3 things of kind 0 (`q390/sweep.py`; Q390) |
| `lib/ringsweep.py [<list of files>]` | The park analyser's sixteen int rings across every park file on hand (it finds them, or takes a list): per ring, the files holding it on the cash-in ring's slot, those with a month so far, the commonest first months; then the balance law (each closed month of the 1st = the one before + the 6th's month - the 3rd's) and every file that breaks either (Q387); then the visitors law (the 11th never falls, its newest at most
`mLifetimeVisitors`, no month so far) and the files that break it (Q389). One file's rings: `parkfile.money_rings` |
| `lib/freshpark.py` | exec'd before `parkrun.py` with `FRESH_GAME = "<folder>"`: a game folder of the run's own whose only player has no `gms.dat`, so `enter()` makes a **fresh Full Simulation park** (no park file, 70 items, tick nought) in place of the stock Instant Action one. Pause straight after `enter()`; `hire` and `buy` work under pause (Q349) |
| `lib/labsim.py <park file> [--ability 12] [--researchers 1] [--first 760] [--tech 0,0,80,85,85]` | The original's research lab run forward from a park file's own lab (`parkfile.lab`), by `park-engine.md`, "The lab at work": one line per project finished, its hand-out, tick, kind, item, a group raised and the kind's next project, then whether all are done. Stock Easymode: Dino Karts on hand-out 95, tick 2640, the last on 1177, tick 24280 (Q245). Not modelled: an appended upgrade (none in Lost Kingdom's Easy layer), a resting researcher, hires, `ResearchTech[5..]` |
| `original/freshfs.sh make <outdir>` / `stop` | A fresh Full Simulation park made by the ORIGINAL, off-screen: a player folder with Alexah's Full Simulation `gms.dat` and no park file, picked in the lobby; copies the `restart.INTS` the original writes and leaves it running in that park for a memory reader. **Written from the steps as they were run by hand (Q349); the script itself has not been run whole** |
| `lib/sfxmap.py <data dir> [<cat_..SFX.map> [effect]]` | A sound category's effects and variation headers (flags, parameter, volume, pitch, wait, range), walked as the loader walks them; with the data folder alone, the spread of every variation's range over all 31 files. Importable (`Category`, `categories`) |
| `lib/responses.py [ID ...]` | The advisor's response table read from `testme.exe` on disk (610 rows: sample, lip file, gesture, model slot and bank, the two props, group): the spread of slots and props, or the rows named. Importable (`rows()`, `off( va )`, which turns any address of the image into a file offset) |
| `lib/advisorrow.py MESSAGE...` | An advisor message's metadata row (`0x0076e300 + id x 0x38`, read from the emulated fill `q177/skeptic-analyser/meta_full.json`): posted only or polled, category, game mode, score function, copies, first response and lines, then each line's response row. Importable, `row( id )`. Q141 |
| `lib/gmsfile.py <gms.dat>...` | A player profile's progress (`saves.md`, "gms.dat"): the four record and two secret ticket flags, spent, keys given outright, each theme's six park tickets and four records, the ticket-bought ride ids, and the tickets and keys counted as the game counts them. Importable, `read( path )`. What the original awarded: Alexah's Full Simulation profile reads jungle Locals 0 to 2 and record 0 at 106. Q141 |
| `lib/advisorschema.py [OFFSET ... \| all]` | The advisor's `.sam` schema walked from `testme.exe` on disk: the `Advisor.sam` key behind each offset of the balance object `0x00fb3540`, which is `ECX` in every score function of the advisor's metadata table (`[ECX + 0x5e4]` is `TutorialUIPanel.Score`). The array's four extra bytes are anchored on three known offsets, not read. Importable (`walk()`) |
| `lib/tagtable.py [ID ... \| groups]` | The tag table read from `testme.exe` on disk (382 rows: id, key, picture, string, the bar's two buttons, life, text kind): the spread of each field, the rows named, or every response group with its row. The tag a said advisor line leaves on the gadget's aerial is the row of its response's group. Importable (`rows()`, `find( id )`) |
| `lib/hidelists.py <advisor.wad> <stem>` | Every clip's hide list of a model in a wad (`<stem>m1.md2` on), each node by name: what a clip's start hides |
| `lib/md2surf.py <wad> <member.md2> [x z]` / `--sweep <data dir>` / `--uvs <data dir>` | A model's header float, lookup records and the faces a point stands on, with the height under a model point; the sweep counts them over every wad. Importable (`Model`): a reader of a `.md2`'s header, lookup table, meshes, vertices and faces to build a model check on; each mesh's `.uvs` (one per vertex-order entry) and `uv_runs( model )`, and `--uvs` lists every material run whose UVs leave 0..1, which `q363/wrapflags.py` sets beside its frame's wrap bits (Q363) |
| `lib/parkrun-smoke.py <out> <bin> [off]` | The scaffold's own check: enters Lost Kingdom, saves under pause, loads the file back from the Load Park list and compares the three ticks; `off` runs it on the private display |
| `lib/uistream.py <VA>` | A compiled UI layout stream of `testme.exe` on disk walked to its balanced close (`hud.md`, "The opcodes"): every control's type, flags, id and rect, its mesh hash, text rect, help row and outline, indented by its parent, each close printed. It does not read ops 9 and 10 (a list's scrollbar): a stream with a list stops there |
| `meshhash.py <hash> ...` | A layout stream's mesh hashes turned back into the `ui.wad` model to load, by file stem or by a model's first node name (`w_park` is the node `window3`) |
| `lib/wctpng.py <wad> <member> <out.png>` | One `.wct` texture out of a wad as this game's reader decodes it (`wct2raw`), a PNG and an 8x copy, with its header's version, size and scales (it prints offset 10 as "Cb" and 12 as "Cr": offset 10 is Cr's, `exe/textures.md`); the member matched by the end of its path (`'textures\\ridestatbar.wct'`). The colours are the reader's, the original's codec since Q355 |
| `lib/wctorig.py <file.wct\|file.sgn> [out.png]` | A type `0x12` `.wct`, or a `.sgn`'s artwork, decoded the ORIGINAL's way (`FUN_00593ab0`, `FUN_00592730`; `exe/textures.md`): the reference to check `TextureFile` against (983 of the 986 zlib files within 1, Q355). Writes an RGBA PNG and an 8x copy. Takes the alpha chunk by its size, which in every shipped file is the bit count's rule (FileFormats `texture.md`). A sign comes out in the codec's order: the original's board holds it with red and blue exchanged (Q357). LZSS (`0x13`) is not done (Q354) |
| `wct2raw/` (a project: `dotnet wct2raw/bin/Debug/net10.0/wct2raw.dll`) `<in.wct\|in.sgn\|in.md2> <out.rgba>` / `--batch <list> <outdir>` | `.wct` textures or a sign's artwork through the repo's own `TextureFile`/`SignFile`: width, height, RGBA. `--batch` decodes every path in a list in parallel to `<outdir>/<line>.rgba` (8,396 in 6 s), failures on stderr; given a `.md2`, each mesh's materials with their flag words; copy the project beside a worktree of main (its `ProjectReference`) for the control (`q355/wct2raw-main`) |
| `lib/wctcensus.py <dir>` | Every `.wct` under a folder (`wadcat --dump .wct` run inside one folder a wad, as names repeat across wads: `q355/alldump`, 8,375, and its `loose/`, the 41 loose files' 21 names; `all.list`) counted by type, colour codec, alpha byte, bit count and alpha chunk, an example of each. `q355/compare.py` is the decode against `wctorig.py` and main to copy |
| `lib/alphauses.py <materials.txt> [<dump> <wads.txt>]` | Every model material of the jungle's wads, `lobby.wad` and `ui.wad` naming a `.wct` whose alpha chunk sits under a header byte 1 of nought (the original draws it cut out), split by the material's `0x2` bit, the one this game's world shader lets alpha through on. `materials.txt` is `wct2raw <model.md2>` over a model dump, `q358/materials.txt` the 751 (Q358) |
| `lib/seam.py <frame.png> x0 x1 y0 y1` | A dark seam between water and its near bank, column by column in a window: the share of seam columns, the median ratio of the darkest pixel below the water to the water's and the bank's, and the longest run of neighbouring seam columns. Compares frames of two games lit differently, where a fixed level does not (Q358) |
| `lib/screendiff.py <a.png> <b.png> [x0 y0 x1 y1]` | Pixels differing by more than 40 between two frames inside a box, and where: a screen that should not change, checked against the control's frame (Q358) |
| `lib/diffpic.py <a.png> <b.png> <out.png> [x0 y0 x1 y1]` | Two frames side by side and a third, the second dimmed with every pixel differing by more than 40 a channel in red: where a change lies and its shape, and the count (Q359) |
| `lib/patchmean.py IMG NAME=X0,Y0,X1,Y1 ... [--vs IMG2]` | Named pixel boxes' mean colour, spread and brightness in a frame of either game, and with `--vs` the same boxes in a second frame, the difference and `mad`, the per-pixel mean absolute difference (how much moved where the means agree, Q486); `mean( img, box )` and `mad( img, img2, box )` importable. A colour compared across the two games or two runs at one camera (Q479) |
| `lib/lndtile.py <base.lnd> <N> <x> <y> [out.png]`, `... sheet X0 Y0 X1 Y1 out.png` | The original's procedural ground tile made by its OWN code, `FUN_0055f780` run under unicorn on the file (`Tiler( path, N ).tile( x, y )`, `.rgb()`): the bit-exact oracle for a port (Q483; `park-engine.md`, "The procedural ground"). `tile( x, y, rec )` with rec = (texels, w, h, depth, word) runs the path laid over too, the texels as the original's surface holds them (Q489). Needs `~/.cache/tpw-emu312/bin/python` |
| `lib/exeleaf.py` (import: `Exe()`, `alloc`, `write`, `read`, `call( fn, *args, ecx= )`, `reads`, `signed`) | Any leaf routine of `testme.exe` run unchanged under unicorn on memory the caller lays out: the image mapped at its base, a stack, a heap, a `HLT` sentinel, FPCW `0x027F`; `call` answers EAX, `reads` lists the globals a call touched. The oracle for a port's arithmetic (Q704's `q704/fill.py` runs `Lab_PercentLeftOfDearest` on a made-up lab and on a park's, `fslab.py`). `lndtile.py` keeps its own copy of the set-up. Needs `~/.cache/tpw-emu312/bin/python` |
| `lib/lndsheet.py <base.lnd> <N> <out.rgb> [--paths CELLS RGBADIR]` | Every procedural ground tile of a park (cells 0..95 x 0..84) by `lndtile.py`, in one file, and its SHA-256: what OpenTPW's `ProceduralGroundTests` hold `ProceduralGround` to (Q488). `--paths` lays a path over each cell CELLS names ('X Y NAME WORD', NAME a `wct2raw` `.rgba` in RGBADIR), as the original's 16-bit surface holds it (Q489; `q489/paths.cells` is the shipped park's 78). Read back `numpy.fromfile( f, uint8 ).reshape( 85, 96, N, N, 3 )[y, x]`. Needs `~/.cache/tpw-emu312/bin/python` |
| `lib/cellmatch.py AIM SHEET X0,X1,Y0,Y1 FRAME...` | Which procedural ground tile each game draws on a cell: a homography from the original's `camera.py --aim` pixels of named cells (`q488/orig/run.sh` writes them) cuts each cell out of a frame of either game at one camera (this game's mapped as `bothcam.py` says) and matches it against `lndsheet.py`'s tile of the cell and its eight neighbours, with the cells' mean colour. The original at Q479's camera A: its own tile on 84 of 122 cells (Q488). `ONLY=FILE` takes only the cells a file names; `PAIR=FRAME0` instead correlates each cell's luminance with the same cell in another frame, game against game (Q489: on path cells, where neighbours look alike and the tile match cannot tell them apart) |
| `lib/barshare.py <png> <x0> <y0> <x1> <y1> ...` | A type-9 bar measured in a frame of either game, by its layout rectangle: the share of its middle row in colour, the share grey, the colour's mean r - g, the last coloured pixel, the grey's mean, and the pixel an eighth of the way. **A black cell counts as grey** (spread under 12): judge "nothing drawn" by the grey's mean, under 5 there and 49 to 58 on a drawn track (Q367). **It reads a
`happygrad.wct` bar about 0.02 short** (its greyish-teal texels spread 30 or less): use `barsplit.py` for that one (Q369) |
| `lib/barsplit.py <png> <x0> <y0> <x1> <y1> ...` | A type-9 bar's fill by where its grey track starts (the first pixel of the middle row, from the left, with a spread of 12 or less and a mean of 40 to 75), as a share of the width: right for any ramp, `happygrad.wct`'s too, and blind to a highlighted row's band past the bar's end (Q369). **A lettered number's anti-aliased edge passes its track test**: where a cell may hold text, not a bar, judge "no bar" by `listcells.colour` (Q371) |
| `lib/listcells.py <png> <y0> <y1> <x0,x1> ...` | A list row's cells in a frame of either game, virtual units: `aligns( png, y0, y1, cols )` where each column's white lettering stands in the row band (start, end, middle as shares of the column, and `left`/`centre`/`right`), `bars( png, ys, x0, x1, h )` `barsplit.py` down a column of bars, `colour( png, ys, x0, x1, h )` each bar rect's middle row in colour (spread over 60: a ramp is, lettering is not). The visitors list's alignments in both games (Q371); `ink( png, x0, x1, y0, y1, white )` the bands of lettering down one column and `pitch( bands )` the median step between them: a list's row height and its font's size in either game's frame (use a column with `$` or digits and white 110; Q377: a name's descender, the `j` of "Rajan", drops its band's middle 4 units and moved a three-row pitch to 49, Q378). **Pass `white=110` to `aligns` and `ink` on this game's 1280 x 720 frames**: at the default 150 a thin `2` reads None and `$ 0` only its brightest pixels, ending 0.86 of a right-aligned column (Q383). **Judge a figure from the right by its END, common to every row**, not its middle: the original's "$ 2500" stands its middle at 0.72 of the buy list's price column and ends at 0.951 on every row (Q384) |
| `lib/texmatch.py <png> <x0> <y0> <x1> <y1> <texture.png>...` | Which of a set of textures a frame of either game draws in a cell, virtual units: the box round the cell's light pixels, each texture's opaque box stretched onto it, scored by the mean colour difference on the texture's darker (icon) pixels; `match()` answers the best, its score and the next best's, or None for no picture. **Read the margin**: a thought's thumbs-up and thumbs-down differ by 2 to 4 in the original's frames, every other pair by 8 or more. The visitors' thought cells in both games, the textures from `wctpng.py` (Q373) |
| `lib/rectcolour.py <png> <x0> <y0> <x1> <y1> ...` | What fills a framed control in a frame of either game, by its layout rectangle (its outer 4 per cent left out): the shares of yellow lettering, white, teal (`!cbut`'s body) and black, and the mean colour, a line a rectangle. A heading framed or bare, its lettering yellow or white (Q370) |
| `lib/mutate.py <mutations.py>` | Putting an item's bugs back, in parallel, in copies of the tree (`WORKFLOW.md`, "Verifying") |
| `lib/stryker.sh <project> <File.cs>...` | Mechanical bugs in a file, Stryker.NET |
| `handoff/` (`README.md`, `prompt.md`, `wait.sh`, `mkev.sh`) | A coordinated queue run (Alexah's word only, CLAUDE.md rule 20): a coordinating session starts one session per item with `create_session`, whose first prompt is `prompt.md`'s `/goal` (under 4,000 characters; a slash command sent later arrives as plain text), `wait.sh <status file> <s> [quiet min]` wakes it when the item ends or goes quiet, and `mkev.sh <out> <sha> <qN>` builds one item's review evidence for a `tpw-verify` reviewer. The README holds the lessons (2026-10-16) |
| `original/loadfile.sh load <file> <outdir> <tag>` / `stop` | A park file loaded in the original from its Load Park list, off-screen, with frames; `POLL=` starts a memory reader just before the click. It starts the original only when it is not running, so a second file is 35 s where the first is 123 s |
| `original/screenpark.sh <file> <outdir> <tag>` | A park file entered by the original on the REAL screen (`VERIFYING.md` rule 128: off-screen draws no sign), for a picture: the file made the newest in the reference player's jungle folder, the welcome and the player clicked, frames `<tag>-1..3` at 1024 x 768 and `-N-full` at 1920 x 1440 from 35 s on, then the original stopped, the file taken out and `save/` checked. 2.5 minutes a file; the camera is the file's own (Q357). Its two player clicks are 3 s apart: at 1.2 s the second fell inside the lobby's island flight at 1024 x 768 and the park was never entered (Q377) |
| `original/resolution.sh 800\|1024\|640\|...\|off\|show` | The reference original's resolution, by writing `Data/Resolution.sam` (the disc's `_Resolution.sam` renamed turns the override on; `off` removes it and the Options screen's 640 x 480 rules). **It stands at 800 x 600** (Q377). Run it with the original stopped; check with `tpwmem.py read 00faa5c0:h` |
| `original/groundcell.py PID SECS X,Y[:X2,Y2]=+MASK\|-MASK\|t@X,Y ...` | The original's memory, WRITTEN: the park ground's heightfield cell words (flags low, texture index high) ORed, cleared or given another cell's texture, held; prints the base model's and the terrain pass's heightfields (the same one), the render word `[0x008bcbc8]`, the size and each write. `HF=engine` writes the terrain pass's own pointer, `READ=X,Y[:X2,Y2]` prints those cells' words first, `AFTER=FILE` holds the writes until a frame is taken. A `POLL` for `loadfile.sh`, or run beside a park already loaded. The ground is rebuilt every frame: `+0x1` opens a black hole on the next one (Q360) |
| `original/heads.py PID SECS ITEM` | The original's memory: every model of an item with its lookup state (shared flags, heads attached, each record holding one) and each head's sprite, place, scale and frame, a line a change. A `POLL` for `loadfile.sh` |
| `original/emitters.py PID SECS` | The original's memory: the particle system's live emitters by the used chain (slot, count, handle, effect, place, aim, life, particles alive), a block a change and the particle counts a second. A `POLL` for `loadfile.sh` |
| `original/scriptvar.py PID THING [INDEX [VALUE]]` | A ride script's variables in the running original, by thing: read, or **written** (the one tool that writes its memory). It is how a ride is broken on demand |
| `original/object.py PID SECS THING [VAR ...]` | The original's memory: one object's `mState`, `mCanLoad`, the guest being loaded, the queue's head, the member assigned, the State of repair and the remaining life, and those variables of its script, a line a change with `mGameTick`. A `POLL` for `loadfile.sh` |
| `original/money.py PID SECS THING...` | The original's memory: each object's all-items columns, its customers last thirty days, last month's profit, total profit, satisfaction and bar, worked out from its takings, costs, customers and satisfaction rings and its two totals as the original's getters do, a line a change with `mGameTick`. A `POLL` for `loadfile.sh` (Q368) |
| `original/thingwords.py PID SECS THING:OFFSET[:FMT[:NAME]] ...` | The original's memory: any words of any things, a line a change with `mGameTick`. Use it for a field or two before writing a poller: the mechanics' search cursor is `2:0xc`, a mechanic's state, job and count `26:0x19c:I`, `26:0x218`, `26:0x214:I`. A `POLL` for `loadfile.sh` |
| `original/spritepass.py PID`, `particleheights.py PID SECS EFFECT [STEP]`, `particleoverlay.py PID OUT.png [EFFECT]` | The original's world particles: the sprite pass's matrix (`[0x0087b088]`) with every emitter's draw words and its particles projected to pixels; how high and wide an effect's particles go over a time; and a frame grabbed with each particle's sprite marked on it, which proves a projection rule against the picture |
| `original/camera.py PID [X,Y;...]`, `--aim X,Y` | The original's park camera now: the point, zoom and rotation, the camera's matrix (its last row the eye) and the draw matrix, and named cells' corners at 1.5 and 21.5 put through it to pixels of the 1024 x 768 frame; `--aim` prints the pixel of a cell's middle, so a run's clicks follow whatever camera it has (Q435) |
| `original/lens.py PID [SECS]` | The original's lens now: the angle handed to the projection builder, its near and far, the projection, the screen scale, the lobby render camera's own angle and `gui_CameraFlags`, then the picture's angles top to bottom and across; with SECS a line a change (Q260) |
| `original/camto.py PID X,Y [TOL]` | The original's park camera panned to a cell by resting the pointer on the 1024 x 768 frame's edges (its arrow keys turn the view), each edge's sense and rate learnt as it goes; with Mike's window opened before, the camera stayed where it was put for the 20 s measured (Q439) |
| `original/cellfind.py PID X,Y [PX,PY]` | The original's side of `lib/cellprobe.py`: the pixel over a map cell, found by moving the real pointer and reading the build preview's cell (`[0x008186b8]`, `[0x008186b4]`), Newton steps from `camera.py --aim`'s pixel (Q439). The preview reads (0, 0) under a park screen: `PICK=1` reads the picker's cell `[0x007b05cc]` instead, which a member's window leaves live. Its probes stay inside 0.95 of the way to each edge, past which the pointer pans the camera. With the camera still and no panel or object mesh under the pixel, `camera.py --aim`'s pixel was the cell on the first probe, six of six at rotations 0, 90 and 180 (Q440) |
| `original/spritepick.py PID SECS [census]`, `spritepick.py PID where THING [depth]`, `spritepick.py PID census` | The original's sprite pick, read-only (a `POLL` for `loadfile.sh`): a line a change with the pointer (raw and as -1 to 1), the latch `[0x008bd4fc]` and its owner, the latch the decode PREDICTS from every sprite carrying `0x200` (`park-engine.md`, "The thing under a sprite"), the pick's answer and the hover's person as thing ids, the cell, and the latched sprite's d2, r2, w, size and flags; `census` lists the sprite list by flags and owner kind once. `where THING`: that thing's circle's middle as a pixel of the 1024 x 768 frame (aim the pointer with it), its w, and the draw matrix times its place beside its stored place (Q468); `depth` adds its picture's texture flags and state word, the pull `Sprite_BuildQuad` gives its z, the buffer's z either side and the pull in eye units (Q477). `PID census` alone: the sprite list now, by flags and owner kind of any kind (a person's claim to a script first: a manager's `+0xc` can equal a person's script index), with the sizes and scales and whether `0x200` is set (Q470) |
| `original/cellhead.py PID [THING ...] [@X,Y ...]` | The original's cell lists, read once: the cell a thing is linked into (its bytes `+5`, `+7`) with its address (what `[0x007cc308]` and `[0x007b05ac]` hold), and a cell's chain head first (the cell's `+0x24`, then each thing's `mMapChild` `+0xa`), with `mGameTick`. Here the console's `cell X Y` (`occupant`) and `staff` (`cell`) (Q446) |
| `original/hoverscan.py PID CATS [person\|noperson\|any] [click] [grid X0,X1,DX,Y0,Y1,DY]` | The original's hover found by stepping the pointer (xdotool jumps on `$DISPLAY`, default `:77`, over 1024 x 768 points): after each jump the category `[0x007c24f4]`, the cell `[0x007b05cc]` and the sprite pick's person `[0x007b05ac]` as id and kind; it stops at the first point of a category in CATS with a person there, not there or either, and with `click` presses at once and reads what the click keeps, the person `[0x007c2650]` and the object `[0x007c2654]` (Q474, `q474/orig/run.sh`). Exits 1 when no point matched: run it again for walkers to move |
| `original/setcam.py PID X,Z ZOOM ROT [off\|on]`, `setcam.py PID off\|on` | The original's park camera WRITTEN: the point (world units), the zoom and the rotation in degrees, the rotation asked for (`0x00790a38`) and, setcam's own choice, the drawn one (`0x007909e0`; the game's snap writes only the first), and the scroll terms as the snap writes them, so the next frame is drawn there; `off` stops the follow first, `on` starts it on the locator's person. Reads back the point and the eye against the formula; exit 2 where the point did not stay (a follow or a pan moved it) (Q440) |
| `original/outside.py PID [X,Y] [N]` | The original's map cells outside the park (`mFlags`, the word at `+0xe` of each 0x44-byte cell, bit `0x40`) nearest a cell or the camera's point, as runs along a row: where a refused rectangle of the patrol tool lies (Q439) |
| `original/capmatch.py CAPTURE SDT ENTRY FROM TO ...`, `... --scan FROM [SECS]` | One sample of a bank measured in the original's own mix (an `ORIG_ASOUND` file capture, its byte marks taken with `stat -c %s` either side of what is measured): each window's score, gain and rms; `--scan` scores every 2 s window to the end, so a click's window can be shown the capture's only match (Q442) |
| `original/particledump.py PID EFFECT [raw]` | The original's memory: EVERY particle of each live emitter of an effect (its place less the emitter's, velocity, size, frame, colour) and the spread of each, where `spritepass.py` prints an emitter's first six. OpenTPW's side is the console's `particles <slot>`; `q261/cloud.py <run.out>` prints the same spread from a run's output |
| `lib/spritesheet.py <bank.ESP> <out.png> <set>...` | A sprite bank's picture sets with their alpha, each frame over black and over white, and each frame's alpha spread and mean colour: what a particle's picture is before any blend. `wadcat --dump SPR_PA.ESP` and `--dump SPR_PA.TPC` on `esprites.wad` write the two files it reads. `spritesheet.py <bank.TPC>... --find W H OX OY`: the pictures of that size and origin, with the columns and rows their alpha reaches and those of alpha 128 or more over the origin's row: a figure's own edges, to predict a frame's from (Q475; Mike's are `Generic\Mechanics\SPR_OM`) |
| `original/voices.py PID [SECS]` | The original's memory: every live sound voice by the service's own list (its effect and flags word, the voice's flags, place, range and rectangle, how far it is from the listener), with the listener first; a block a change. Says which placed sounds are in range and sounding. A `POLL` for `loadfile.sh` |
| `original/mixer.py PID [SECS]` | The original's memory, inside `QMixer.dll`: the session and its listener, and every open channel's flags, place, distance mapping, volume, the mixer's own distance, and its cone, distance and final gains, with the gain the law gives beside the one read. How loud the mixer is playing a voice, where `voices.py` says only whether it sounds. A `POLL` for `loadfile.sh` |
| `original/crowdmix.py PID SECS` | The original's memory: the crowd's voice (level, handle, keys, values, the current variation with its weight, the sample record) beside every flat mixer channel's volume of 127, in ONE log stamped with the wall clock and `mGameTick`, a line a change, 20 ms a poll. Use it where `crowd.py` and `mixer.py` would have to be held together by eye; `mixer.py` prints nothing for a change of volume alone. A `POLL` for `loadfile.sh` |
| `original/mixfreq.py PID SECS [2d]` | The original's memory, inside `QMixer.dll`: every open 3D channel's place, far end, volume of 127 and FREQUENCY (`+0x1f4`), a stamped line each time one opens or changes, and at the end a count by far end, volume and frequency. Which pitch each sample of an effect plays at; `2d` reads the flat channels instead. A `POLL` for `loadfile.sh` |
| `original/rainmix.py PID SECS` | The original's memory: the weather thing's quality, forecast, drops, target and its own rain handle, the global rain handle, the two rain voices (controller keys and values, the current variation's volume bytes) and every flat mixer channel under 40 of 127 with its frequency, in ONE stamped log, a line a change. How loud the rain is and what sets it; `crowdmix.py` is the same for the crowd's voice. A `POLL` for `loadfile.sh` |
| `original/advisor.py PID SECS MESSAGE[,MESSAGE...] [THING]` | The original's memory: the park advisor (the thing `FUN_005194d0` answers): each named message's own record (the tick last said, the line, said, slaps), the eight slots (subject thing, line asked for, message, score), the last action's start and duration, and a thing's State of repair beside them, a line a change with `mGameTick`. Says whether and when he was told a message and whether he said it. A last argument `poll` ends each line with the poll's cursor (`[0x00fb3540]`, the row of the metadata table the next tick starts from; Q316). A `POLL` for `loadfile.sh` |
| `original/advisortable.py PID [meta_full.json]` | The original's memory: the advisor's metadata table as the running game holds it (the image holds zeros): the polled rows as ranges with their count, a few rows' places in a round, and every dword where it differs from the emulated table (two, both row 0's). A `POLL` for `loadfile.sh` |
| `original/bytehold.py PID SECS ADDR=VALUE [TICK ADDR=VALUE]` | The original's memory, **written**: holds one byte of the image at a value, and at another once `mGameTick` passes a tick, a line a write. The Options screen's Tutorial switch is `0x0078d90d`. Stop the original afterwards |
| `original/helpbar.py PID [SECS]` | The original's memory: the help bar's label shown or not, the help sink's switches and the control the pointer came onto last (the park's view among them), the hover's category and cell and the entry-price screen; with SECS a poll at about 110,000 reads a second, a line in ms a change, fast enough to see the hover's own steps (each zeroes the category for a moment). The text is read from the frame (`560x50+232+712` at 1024 x 768). Made for Q473 (`docs/exe/hud.md`, "The help bar holds one posted row"); since Q495 also the buy screen's poster: its item `[0x007cc1e8]`, the item last posted `[0x007cc1e0]`, the state `[0x0075513c]` and the screen `[0x007cc1f8]` (`hud.md`, "The buy screen's help rows"); since Q496 `cam`: the game type, the online visit's tree, control, state and thing (nought in every offline park), the current mode's vtable and `gui_CameraFlags` (`hud.md`, "The online visit's help rows"); since Q497 `fp`: the hover's on-the-view flag and thing, layer 1, the library's pointed control and the cursor id applied, in the poll too (`hud.md`, "The help bar in first person"). |
| `original/memhold.py PID SECS ADDR[:fmt]=VALUE ...` | The original's memory, **written**: holds static words of the image (`I i H h B f`, `tpwmem.py`'s kinds) at values for SECS, a line a rewrite, then puts each back as it was. The fog's start and end `0x008bd490:f`/`0x008bd494:f`, the compositor's word `0x008bd550` (Q479); the game clock's paused flag `0x007859a0:I=1` (`0x00785970` + 0x30, not a real pause) holds the park still, every person in one pose, while the frame goes on drawing (Q477) |
| `original/compstate.py PID`, `tilecap.sh OUTDIR [HITS] [SECS]`, `lndcmp.py <base.lnd> OUTDIR` | The original's procedural ground compositor: its state read once (the ready bit, the box, the cache's tile size and count, the lnd object), every tile it makes caught by `gdb` as it is made (move the camera while it runs), with a path tile's record and texels (`rec-*`, `tex-*`, Q489), and each caught tile against `lib/lndtile.py`'s (Q483) |
| `original/waiting.py PID SECS` | The original's memory: the park analyser's count of guests waiting for a shut park to open (its `+0x1f0d4`, written each time the advisor's row 1 is scored past its two early ends, -999 before), beside the same count taken live (things of type 1 in state 3 on the two ticket booths' cells, by id), `mParkClosed` and every guest's state, a line a change with the wall clock and `mGameTick`. A `POLL` for `loadfile.sh` |
| `original/tags.py PID SECS` | The original's memory: the tags on the gadget's aerial (how many are held, each one's control rectangle on the 2048 x 1536 layout and its text, the mast's rectangle, the tag on the bar), a line a change with the wall clock and `mGameTick`. Says when a line leaves a tag, where it stands and when it goes. A `POLL` for `loadfile.sh` |
| `original/staffpool.py PID SECS` | The original's memory: the staff pool, each slot in use as slot:kind/name row/grade (`[0x007cf83c]`, 32 records of 0x14 bytes), beside the hire screen's tab and whether it is up, a block a change with `mGameTick`. The hire list fills in slot order, so the first of a kind is the first handed over; it shows each timeout and top-up. Resolve a name row with `strdump`. A `POLL` for `loadfile.sh` (Q356) |
| `original/bank.py PID` | The original's money, read once: the bank balance, the analyser's staff costs this month, the hand's worker (its low word), the current mode, the staff window's kept member and `mGameTick`, one line. Read it either side of a click to measure what the click cost (Q432: a dismissal's wage, 161) |
| `original/screens.py PID SECS` | The original's memory: what the gadget's three category buttons are left on (`[0x007cb28c]`, `[0x007cb2a0]`, `[0x007cb290]`) and which park screen is up (the open screen `[0x007c24c8]`, the buy screen `[0x007cc1f8]`, the hire screen `[0x007ca308]`, each pointer printed, so a screen destroyed and built again shows) and the hire screen's tab (`[0x007ca30c]`, `hiretab`, 0 the cleaners' to 4 the scientists'), a line a change with the wall clock and `mGameTick`. Says what a click opened without a frame. A `POLL` for `loadfile.sh` |
| `original/bar.py PID SECS` | The original's memory: the message bar (its rectangle, shown bit, parent, its data's thing, button types, item and text, each child's rectangle), the tag on show and the one being put on show, the gadget's body with its children two deep, the arm's handle, and each tag's rectangle, shown bit, timer byte and whether a stream plays on it, the changed parts a change with the wall clock and `mGameTick`. Says what a click on a tag, on the bar or on the camcorder button does to the arm, step by step. A `POLL` for `loadfile.sh` |
| `original/breakq.py PID SECS THING [all]` | The original's memory, **written**: breaks a ride (`VAR_BREAKSTAT`) each time it is open with a queue that has stood four seconds, so every break puts a queue out, a line a break. With `all` it first writes nought over the mask byte at `0x0050133f`, so every guest dismissed from a queue plays kids `0x80`, not one in eight. Stop the original afterwards |
| `original/inside.py PID SECS [LEVEL [BELOW]]` | The original's memory: the guests inside the park as the analyser counts them (`FUN_004c9130`: a guest on a cell of type 0, 1, 3, 9 or 10) and those of them whose toilet need is over LEVEL (60 unless given), with their ids, the share in whole per cent the advisor's row 82 sets against 25, and the door, a line a change with `mGameTick`. Says who the advisor's and the gate's counts hold on a tick. A `POLL` for `loadfile.sh` (Q347). `inside.py PID SECS LEVEL BELOW` also counts those under BELOW happiness (`+0x19c`), their share and the crowd, the advisor's row 14 (Q415) |
| `original/worldwords.py PID SECS OFFSET[:FMT[:NAME]] ...` | The original's memory: any words of the world (`[0x0080239c]` + OFFSET), a line a change with `mGameTick`: the arrival vehicle that is current and the three slots' things are `0x1da72a:H:current 0x1da72c:H:size1 0x1da72e:H:size2 0x1da730:H:size3`, and the park's generator, `mRandomSeed`, is `0x1da708:I:seed` (a file's own seed is the first read after its load, Q443). A `POLL` for `loadfile.sh`; `thingwords.py` reads a thing's |
| `original/litter.py PID SECS` | The original's memory: its litter share as the advisor's rows 8 and 9 take it (`FUN_004ccdf0`: every map cell, `+0x8` the type, `+0x2c` `mLitter`), with the littered cells, a line a change with `mGameTick`. A `POLL` for `loadfile.sh`; it shows the original's handyman sweeping where this game's does not (Q417) |
| `original/texcache.py PID SECS [NAME=+MASK\|-MASK\|=VALUE\|@ADDR=...]` | The original's texture cache (`[0x0087a268]`): every texture with its folder, refs and each record's flags word (`0x100` cut, `0x200` graded, `0x300` additive), `LIST=substr` to narrow; given NAME (`m_grass*` a prefix) or a raw record `@0x...`, holds the `0x300` bits (`=`) or ORs/clears a mask for SECS and restores them, `DONE=FILE` touched once the writes stand. A texture whose flags no draw reads (Q361: a procedural base's floor) changes nothing |
| `original/floormats.py PID NAME...` | The original's memory: every model material whose texture is a named cache entry (its source record, found by walking texture -> slot -> material object -> record), its flag word (`0x800` drawn opaque), counted by texture. Keep only words with bit `0x1` and nothing above `0x8f3`: the rest are other structures (Q361: 174 of 174 floor materials with `0x800`) |
| `original/floormesh.py PID` | The original's memory: every loaded model instance with its procedural base as the loader left it (`[model + 8] + 0xc` the base vertices `FUN_00469a80` counted, `+0x2c` the floor mesh it picked, by index and name) and its root mesh's material words (`0x800` = opaque). The original loads every catalogue item on offer, so one park checks them all; ignore the instances counting 0 beside one that counts (other references to the same header) (Q362) |
| `original/squares.py PID SECS [N]` | The original's marker squares, read-only: the fullest face list (`0x00820b00`, count `[0x00871ee0]`) seen in SECS, a record a line (cell, height, texture: 0 blue, 1 red, 8 `m_link`, 11 `m_end`; face `+0x0c`: 0 the square, 1/4/`0x10`/`0x40` a wall on its N/E/S/W edge rising 10 from the height, Q436; orientation `+0x10`), then each texture's cells and those laid more than once. The draw empties the list every frame, so it samples; textures 15 and up are other meshes' polygon faces after the squares. A tool's strip, a stranded person's red square, the locator's patrol edges (Q426), the walls under a lifted square (Q436) |
| `original/dynfaces.py PID MESH [out.json] [N]` | The original's dynamic-faces meshes, read-only: every record of the face list (`0x00820b00`, `0xa0` bytes) whose mesh id (`+0x90`) is MESH, read at once - 1 the sea's 134, 3 the gates' flags' 384 - with their count, words and box, and into `out.json` each one's four corners, UVs and normals (the original's axes, y up). The count is nought between frames; the records stay. Q491's flags against `q491/flagmodel.py` |
| `original/pointsat.py PID HEX...` | Every dword of the original's writable memory pointing at the given addresses (`OFFS=N`: or up to N bytes before them), ~10 s: run it on its own output to walk a pointer chain up |
| `original/brk.sh ADDR[,ADDR...] [HITS] [SECS] [WORDS]` | Who CALLS a routine of the original: a hardware breakpoint through `gdb` (up to four addresses joined by commas, each hit naming its own; Q364), attached and detached again; at each hit `mGameTick`, the registers and the stack's words that point into the game's code, nearest first (the first is the caller's return address; read each in Ghidra, a real one follows a `CALL`). `watch.sh` says who writes an address, this says why. `BRK_PRINT='printf "fctrl %x st0 %.17g st1 %.17g\n", $fctrl, (double)$st0, (double)$st1'` adds one gdb line a hit: the x87's control word and stack at a compare |
| `original/thingpoke.py PID SECS KIND THING:OFFSET:FMT=VALUE ...` | The original's memory, **written**: holds words of things at given values, written again whenever they read otherwise, a line a write; refuses a thing whose type byte (`+2`) is not KIND. `150 15 7:0x38:B=1` holds the weather's `mLightning`, so bolts and thunder come on a clear day. `thingwords.py` reads the same words. Stop the original afterwards |
| `original/mixpairs.py [<mixer.py log>]` | A `mixer.py` log against the category files: every pair of a 3D channel's far end and volume of 127, with the variation volumes of that range that `round( v x 0.75 )` makes it from. Says which effects' channels a log holds, and that none plays at a number the files cannot make |
| `original/modelboxes.py PID [FIRST LAST]` | The original's memory: each thing's model record's cell rectangle (`+0xc0`, `+0xc4`), whether it has a `.hmp`, the box's middle (where its scream is sounded) and its base height. Read the objects' lines (the stock park's are things 13 to 24): a person's `+0x20` is no model handle, and its line is noise |
| `original/research.py <in> <out> <item> [0]` | A copy of a jungle park file with one item marked researched, so Instant Action's buy list offers it; with a last `0`, marked not researched, which both games keep for an item whose research costs nothing (Q335) |
| `original/original.sh`, `tpwmem.py`, `gmove.py`, `record.sh`, `watch.sh` | Starting, reading, clicking, filming and watchpointing the original ("The original under Proton", below) |
| `original/timens.py SECONDS CMD...` | A command run in a time namespace where the computer has been up SECONDS: the original's park clock without a restart (warning 1, below). **It fails outside the VS Code sandbox** (`PermissionError` on `/proc/self/setgroups`, tried 2026-10-10), so a session started from a plain terminal cannot use it as it is |
| `original/kwinwl-start.sh` | A private display on `:78` drawn by the graphics card (`kwin_wayland --virtual` with Xwayland) for the original, where Xvfb draws by software. `:78` is `parkrun.py`'s too. **Not run since it was written**: try it before leaning on it |
| `q119/lib.py` | What `parkrun.py` is built on: the launch, the console pipe, XTEST clicks and keys, the frame grab. Use it through `parkrun.py` |
| `fork-review-2/forkwatch.py [--from SHA] [--keep]` | The daily fork watch's first step (read-only): fetches the meneerkrabs fork's clone (`~/.cache/tpw-fork-krabs/repo`), takes the PC addresses in the lines added since the last look, maps each to its function by `funcs-all.tsv` (every function's ranges, dumped from Ghidra 2026-10-16), drops the functions our tree and the FileFormats clone cite now and those an earlier look handed on, and writes `watch-<sha>.json`; `--keep` records the new tip in `watch-state.json` once the leads go to a sweep |
| `fork-review-2/mkwatchev.py <watch-SHA.json> <dir>` | The watch's evidence builder: splits the leads into areas (advisor, ui-hud, audio-fx, world, other) by the fork file citing each most, and writes `<dir>/claims-<area>-N.txt` (the fork's citing lines, +-2, read from the watched tip, for the sweeps) and `<dir>/code-<area>-N.txt` (our Ghidra decompile of each function, for the verifiers) from `<dir>/decomp.json`, which a Ghidra dump script (`watch/2026-10-16/dump.py`, a model for the next) writes |
| `gen_addresses.py` | Regenerating `docs/exe/addresses.md` |
| `wadcat`, `strdump`, `nodenames/` | A wad's entries, a string table's lines, a UI wad's node names (`CLAUDE.local.md`). `strdump` prints every part of a row, a parameter as `{Pn}` (since Q372: a row that starts with one, as UITEXT 441 "{P11} hours", read blank before); name the table under the data folder, `Language/English/UITEXT.str` |

**A poller of the original's memory is the one thing still written an item at a time** (`q257r/orig/cars.py`,
`q257x/orig/smoke.py` for a car's smoke handle, its emitter and its node's place, `q257t/orig/chan.py` for a car's model's channel stamps, `q253/orig/look.py`; a model's heads are `original/heads.py`, above): each reads its own structures. Start from the newest that reads the same
structure, and hand it to `loadfile.sh` through `POLL`.

## Cameras in both games

Read this before aiming a click or comparing two pictures. Each fact names the item that found it.

| To do this | Use |
|---|---|
| Set this game's park view | console `camera X Y ZOOM YAW` |
| Read the original's view | `original/camera.py PID` (`--aim X,Y` gives a cell's pixel) |
| Read the original's lens | `original/lens.py PID` |
| Set the original's view | `original/setcam.py PID X,Z ZOOM ROT off` |
| Put both games on one camera | `lib/bothcam.py` (`both()`, or `X,Z ZOOM ROT PID`) |
| Stop or restart the original's follow | `original/setcam.py PID off` / `on` |
| Pan the original to a cell | `original/camto.py PID X,Y` (pointer on the screen edge; it stops a follow) |
| Click a named cell in the original | `original/cellfind.py PID X,Y`, straight before the click |
| Click a named cell here | `lib/cellprobe.py` `find( (x, y), start )` |
| Find cells outside the park in the original | `original/outside.py PID` |

**This game** (`ParkOrbitCameraMode`):
- `camera x y zoom yaw` takes world units, a cell times ten, and the yaw in degrees; its reply is the state (`poi=... yaw= zoom= pitch= ground=`). Q257n
- Both games at one camera: copy the original's point, zoom and rotation from `camera.py` as `camera X Z ZOOM -DEGREES` (the original's third coordinate is this game's second, and its rotation is this game's yaw negated: at 90 the same eye is -90); `lib/bothcam.py` does it. Q435, Q440
- The eye is lifted by the ground's height under the point, so over high ground it is further than `zoom` from it. Read `placed`'s listener, do not work it out. Q264
- The camera aims at the ground: it cannot close in on water 30 units up. Q257s
- A person's drawn size in both games: the mechanics' locator (Ctrl+S, then the button `95f`, the original's at (360, 613)) puts Mike Cooper's place at the frame's middle at zoom 110 in both (w 77.782 there); read his picture by `circle 26` here and `spritepick.py PID where 26` there, and mark the frame with `lib/cropgrid.py`. Since Q500 his whole picture is drawn here with the locator open or folded, as there; `circle` ends with the depth he is tested at ("tested 106.19" at 110, his depth itself in first person). Q470 (`q470/confirm.py`, `q470/orig/run.sh`)
- **A person's own pixels and sides in both games** (Q475): here, under `pause`, a frame, then `sprites 0` (the people's pictures, balloons and bubbles not drawn, the pick's record kept), a frame, `sprites 1`: two paused frames are the same to the bit off the people, so every pixel differing by more than 2 is theirs; `circle THING` gives the origin's own pixel (`origin pixel`) and the picture's size, origin and reflection, and the sides are the origin's pixel minus and plus (origin x, width − origin x) × 60 / w × the frame's height / 768 (here w is `circle`'s depth × sin 45), swapped when reflected (`q475/confirm.py`, a `[W H]` for `size`). The original cannot hide its people: freeze the frame and its memory as one with `kill -STOP` on the game's pid, read `spritepick.py PID where THING` before and after the `import` grab, then `kill -CONT` (`q475/orig/run.sh`: before and after equal at 10 of 10), and read the sides by eye on a `cropgrid.py` crop with both rules' marks. **Never let the command that starts an off-screen run hold the words of `original.sh`'s display test** (`Xvfb` and the display number, in a note or a check in the same shell line): its `pgrep -f` finds that shell, the private display is never started, and KWin, `gmove.py` and `xdotool` then fail with "Can't open display: (null)". Q475
- **What a constant of the original's draw does, seen in one pose** (Q477): hold the clock with `memhold.py PID 14 0x007859a0:I=1` (the people stop, the frame keeps drawing; this sets only the clock's paused flag, not a real pause: while held the clock reads the capture of the last real pause, which can be earlier than now, so it may step back and then stand, and when the flag is put back the held span falls due at once, so take nothing timed just after it), grab a frame, hold the constant at another value with a second `memhold.py` for a few seconds and grab again, then once it is put back; the first and last agree off the corner the pause leaves moving, so the middle frame's difference is the constant's alone. Mike's depth pull `0x00702044` held at 0, -0.02, -0.05 and -0.2 under the mechanics' follow: `q477/orig/run.sh` (`KS="K K K"`, a cycle a value) and `q477/orig/trio.py` (the three crops with the origin's row marked, and the rows that differ). Our side's lowest drawn row of a person against the origin's row: `q477/ctl.py`. Q477
- **Who in the view is hidden by what is nearer** (Q500): under `pause`, `circle` with no thing lists every drawn person (window pixels, depth, picture, "tested" depth), and a frame with and one without people (`sprites 0`) give each one's own pixels; set against their picture's rectangle, the fewest are the ones something nearer covers. `q500/more.py` (`cut()`: the census, the eight most covered, a crop sheet of four; at the start view the park gate's stone hides two), with first person's census (`camcorder`) after. Q500
- `freeze` stops only the lobby's orbit. `pause` stops the clock, and nothing eased on `Time.Delta` moves under it: `step` before a grab (`VERIFYING.md` rule 91). `camera` then `step 2` moves every walker two ticks, so take a census last. Q257r
- `cellprobe.py` needs the camera set first (`camera X Y 60 0`): at zoom 25 a 50 to 90 px step stays inside one cell and the slope is nought. Anchor nothing before both corners are found. Q433

**The original** (1024 x 768 off-screen frame at its origin):
- `camera.py` reads the point `0x007908f0` (x, ground, z; a cell is 10), zoom `0x007909ec`, rotation `0x00790a38` and the drawn rotation `0x007909e0` (eased after it), the camera matrix `0x0087aff0` (its last row the eye) and the draw matrix `[0x0087b088]`. Q435
- Its eye is the formula's: point + (-h sin r, sin p x zoom, -h cos r), pitch from the zoom (`park-engine.md`, "The park camera"), measured at rotations 0, 90 and 180. Q435, Q440
- Its arrow keys turn the view and do not pan. The pointer past 0.95 of the way to a screen edge pans (only with `[0x0078d910]` clear, an Options toggle), about 8 cells a second, and stops a follow: `camto.py` learns the sense and rate, then rests the pointer in the middle. A pointer left there by a script keeps panning (Q440's first round lost two rotations to it). Q433, Q439, Q440
- `setcam.py` writes the camera words (with `off`, or a follow puts the point back the next frame) and reads the eye back: 0.000 units off the formula at every rotation tried. Q440
- **Its zoom stands between 70 and 130**: its update clamps it, so `setcam.py` given 150 reads back 130 while this game's `camera` takes 150, and the two pictures differ by 150 / 130 (pixels mapped from one land elsewhere in the other: Q478's first round met the Litter Bin for the Drinks Shop). Keep a shared camera's zoom inside 70..130. Q478
- A file entered by `screenpark.sh` is framed on the camera's own point; the Load Park list's view is 65 units on. `parkrun.original_view( x, y )` allows for the list. Q359
- For the first seconds after a load the camera is not on the saved view: poll a short-lived thing, do not film it. Q257w
- A camera file's listener is not where the camera was aimed: read `voices.py`'s first line. Q263

**The follow**:
- It is `gui_CameraFlags` `[0x00790ab0]` bit `0x1000` (`park-engine.md`, "The follow"). The original starts it when the locator's button finds somebody, on Ctrl+S or Ctrl+V with a person's window up (with nobody it opens a locator and leaves the bit clear), and on a window's binoculars, which put the window away; the point then trails the member as he walks, the arrow keys only turning the view. Q435, Q440
- A person's window, from the strip or the world, folds the locator and stops it: with a window open the camera stands still (Mike walked 12 units in 2 s, the point did not move). Q440
- It ends on any pan (the keypad, the pointer past 0.95 of the way to an edge: a second at the right edge left the point 83 units from Mike), the locator's fold, a mode change, and a person's window opening (the strip's click folds the locator, so the camera stands still while the window is up; a frame from before that click is not the view at a PICK UP). `setcam.py PID off` ends it from outside, `on` starts it again. Q432, Q435, Q440
- This game: `ParkLocator.Press` sets `Following`; each frame `Camera` puts the point on the member's blended place. A change to the camcorder's first person, a person gone, or a step that finds nobody clears it. A staff window's preview click does `GoToThing` once. Whether opening a window from the world click follows here is not checked.
- A walker's place against the camera is read under `pause`: two replies apart he has walked most of a cell. Q424

**The lens** (Q260):
- Every view hands the projection an angle (the park 90, the lobby `ISLANDFOV`), and the draw matrix's fixed screen scale makes the picture `2 atan( tan(a/2) / 2 )` high: the park 53.13 x 67.38, the camcorder the same, the lobby at 100 61.58 x 76.93 (`park-engine.md`, "The lens"). `original/lens.py PID [SECS]` reads it live; this game logs it once a mode (`Camera: ParkOrbitCameraMode lens 53.13 high at 4:3, ...`). Q260, Q441
- This game draws the same lens (Q441): its 1280 x 720 pixel is (640, 360) + (the original's - (512, 384)) x 720 / 768 at the same camera, six named cells on the first probe at three rotations (`q441/confirm.py`), and its middle 960 x 720 scaled to 1024 x 768 lays on the original's whole frame (`q441/sidebyside.py`, `q441/side-by-side.png`). The scripts of Q260 to Q440 (`q260/confirm.py`, `q435/confirm.py`, `q436/frames.py`, `q440/confirm.py`) were written for the old 90 degree lens and use x 360 / 768 and the middle 480 x 360: change both before running one again. Q260, Q435, Q441
- A change drawn at one camera is shown by the box difference against an earlier run's frame at the same numbers. Q436
- The original's records do not depend on its camera, so a run whose camera went elsewhere still gives them. Q436
- The original was once read as standing "further back" (Q257r, repeated by Q264) and once "closer" (Q357) at the same numbers. "Further back" was the Load Park list's view, 65 units on (Q359); "closer" was the lens: the eye is the same (Q435) and the picture showed the middle half each way against OpenTPW's old 90 degree lens, so everything stood twice as large in it (until Q441). Q260
- `camera.py --aim`'s pixel was cellfind.py's first probe at both of Q260's cells, with the camera still: the cells out at Q439 came from something other than the lens. Q260

**Aiming a click** (every one of these cost a void):
- Set the camera first (`setcam.py ... off`, or `bothcam.py`), then aim with `camera.py --aim` and click through `cellfind.py` (`PICK=1` with a window open): six of six on the first probe, and a click on a named cell with Mike's window open landed on it (the picker 4018 = (49,31) at the press). Q440
- Read the camera in the same run as the click, straight before it. A fixed pixel or cell from an earlier run named other cells (Q435 O1, Q438 O3).
- Aim relative to the camera's own point, not to fixed cells (`q438/orig/run.sh` `AIMREL=1`); better, take any cell of the right kind found live (`outside.py`, `cellfind.py`) and never hard-code Q433's (51,27), (54,26), (45,15).
- A cell off the frame stops the run unclicked. Q439 stopped at 877,38 and at 2003,686.
- A frame and its camera read seconds apart are not the same view while a follow runs. Q438
- Stop the original on any exit of the run script, or the next run loads into it. Q439
- The original's picker reads straight off the pointer: `camera.py --aim`'s pixel, one `xdotool mousemove` on `:77`, and 1.5 s later the cell `[0x007b05cc]` and its hit point `[0x007a1c70]` (three floats, x ground z), which stand unchanged where the ray meets nothing (`q472/orig/run.sh`). Q472
- The pixel map to this game holds only where both points stand on the same ground: at (400,780) this game's camera stood on ground 7.0 and the original's point slid to (400, 781.6) after `setcam.py`, and the mapped pixel fell two rows short. There, find this game's pixel by `pick X Y` down a column (`q472/confirm.py`, G6b). Q472

## Recipes

Each was found by trial in the item named, and cost a run to find.

- **The daily fork watch** (2026-10-16, `fork-review-2/watch/2026-10-16/`): what the meneerkrabs fork newly reads from
  the PC executable since the last look, handed to Alexah as a report. (1) `forkwatch.py` writes `watch-<sha>.json`.
  It re-fetches and records whatever the fork's tip is when `--keep` runs, so run it with `--keep` in this same run
  (the json is then the record handed on and nothing between two looks is skipped), or write the json's `to` sha into
  `watch-state.json` by hand. (2) Make `funcs.json` by hand, a list of the leads' `func` keys (no script writes it;
  `watch/2026-10-16/dump.py` reads it), run a dump script like that one through the private Ghidra
  (`ghidra-private/gd`, never the `ghidra` MCP server while a session holds it) to write `decomp.json`, then
  `mkwatchev.py watch-<sha>.json <dir>`. (3) One sweep agent per area (`tpw-sweep`, Sonnet) reads
  `claims-<area>-N.txt` and writes `sweep-<area>.md` from a `BRIEF.md` (who we are, the fork, its Mac and Theme Park
  Inc traps: copy the last day's). (4) One `tpw-verify` agent per area settles every high and medium lead and every
  conflict from `objdump` of `testme.exe` and at most fifteen private `gd` calls, reading our facts from committed
  `main` by `git grep` (`VERIFY.md` is its brief), and writes `verify-<area>.md`; only CONFIRMED, or the confirmed
  part of PARTLY, is used. (5) Write `REPORT.md` (leads that held and what they serve, our doc lines found wrong) and
  report to Alexah. (6) **File only on Alexah's word** (hers, 2026-10-16, in the coordinator session: "file and fix"):
  notes under the items the leads serve, new items after the queue's last, our wrong doc lines fixed, each citing
  "fork watch <date>" and the verify file's lead id. Never a sweep-only claim.
- **A member's walk watched in the original, and a patrol area set without the tool** (Q434, `q434/orig/run-b.sh`):
  the person's own `+0x8`/`+0xc` are not its place. The navigator is at `+0xd4`, so `thingwords.py ... 26:0xdc:I:px
  26:0xe0:I:py` (16.16: the cell is the value `>> 16`), `26:0x18:H 26:0x1a:H` the destination (cell x 256 + 128 at a
  cell's centre), `26:0x11c:I` the stamp of the last route planned (it moves on every successful roll), `26:0x30:I` the
  thought and `26:0x19c:I` the state. The patrol tool's click writes `+0x20a`/`+0x20c` and nothing else
  (`FUN_00506e50`), so `thingpoke.py %PID% 0.5 4 26:0x20a:H=3380 26:0x20c:H=3511` (kind 4) fences a member exactly,
  where Q433's clicks P1/P2 land on other cells as the camera follows Mike between runs.
- **The patrol tool in either game** (Q433, `q433/orig/run.sh` and `run2.sh`, `q433/confirm.py`): on the original's
  1024 x 768 frame the staff window's patrol switch is (739,385); with the tool armed `[0x008186b8]`, `[0x008186b4]` is
  the preview's cell, so moving the pointer over a grid and reading them maps the view (`run2.sh`). The original's
  camera moves with Mike between runs, so read the cells in the same run as the clicks. **Its Left key turned the view
  and Down drew it back; neither panned**: the pointer resting on a screen edge pans, about 8 cells a second;
  `KEYS="Left:5" EDGE="1022 300 1.2;512 766 1.0"` brought the band outside the park (x 42..53, y 14..16) into view.
  Here `camera X Y 60 0` (10 units a cell) and `lib/cellprobe.py` find the cells; the console's `strip` is the outline
  and `pointer` its cursor. **An anchored one-cell rectangle is four squares** on that cell (each line lays it):
  measured here, read in the original's code. A run that waits longer than a line (0xc1, 18 s) before the click that
  puts the tool away shows no cut. **Here `0xc1` leaves its slot a tick after it is posted**, as the advisor starts
  saying it: read `advisor`'s `message=193/.../said=1`, not `slot=193/` (Q437's first miss); a way out's cut is the
  log's `withdraw message=193 ... cut=1` just before its own line (`q437/confirm.py`). **A quick right click** (Q438): in
  the original `xdotool mousedown 3`, 0.08 s, `mouseup 3` on `:77` (under the 200 ms limit); here `button(3, ...,
  held=0.06)`. Opening Mike's window again snaps the original's camera to wherever he has walked, so aim that phase's
  cells at the camera's own point (`q438/orig/run.sh`, `AIMREL=1`). **A refused click in the original** (Q439,
  `q439/orig/run.sh` with `AIMB=1`): `camto.py %PID% 48,18`, then `outside.py` for the nearest band of outside cells and
  `cellfind.py` for each corner, read straight before its click. Its sound under the music scored 0.32 only: switch the music off first (`q439/PREDICTION.txt`). With it off
  (Q442, `q442/orig/run.sh`): `setcam.py PID 435,165 110 0 off` draws (43,16)'s middle at the frame's middle (512,384),
  and a press there with the hand empty arms the path tool red (mode 1, `d48` 1, read with the button held); its
  release refuses, and `original/capmatch.py` found `blnl_error1` at 0.558 there, the capture's only window over 0.3.
  With nothing armed after the menu the preview's cell reads (0, 0), so
  `cellfind.py` finds nothing there: aim with the camera instead.

- **The staff window's FIRE and PICK UP in either game** (Q432, `q432/orig/run.sh`, `q432/confirm.py`): on the
  original's 1024 x 768 frame `b_fire` is (228,385) and `b_pick` (308,385); FIRE's box puts its tick at (710,282) and
  its cross at (714,326); here they are `control 50c`, `50e`, `9873c9` and `9873c8`. Fire the stock park's one mechanic
  last: with him gone the mechanics' button opens a locator with nobody, and the strip opens no window. The gadget's
  money readout moves with income every second, so a cost is read by `original/bank.py` either side of the click, and
  here by `money`'s `staff` total. The strip's click folds the locator and ends the follow, so the camera stays put while
  the member walks on with the window up: a frame from before the strip's click is not the view at the pickup. Shoot
  the window, the carry and the drop, which share a camera.

- **The staff member's window in either game** (Q431, `q431/orig/run.sh`, `q431/confirm.py`): Ctrl+S, the mechanics'
  button (360,613), then the strip (380,505) of the original's 1024 x 768 frame opens it on Mike Cooper; its `b_ardown`
  is (810,182), `b_arup` (816,138), `b_locate` (809,231), `b_allstaff` (809,284), the preview (259,206), OK (816,378),
  and the all-staff list's first row (500,222). Here the console's `staffwindow` is its census and `control 50f` (the
  stream's ids) its places; OK's id is -1, which `control` does not parse (and the gadget holds -1s too), so click it by the root `50a`. **An advisor
  line's fate depends on how long a window stays up**: a Tutorial line said to its end is refused for ever after
  (`SayOnlyOnce`), one cut three times too (`DiscardAfterSlaps`); hold a window longer than the original's run did and
  the later posts differ (Q431's round 2): `confirm.py`'s `WAIT=0` keeps the original's pace.

- **The visitor's window in either game** (Q430, `q430/orig/run.sh`, `q430/confirm.py`): opened from the strip as below;
  in the original's 1024 x 768 frame its `b_arup` is (816,138), `b_ardown` (810,182), `b_locate` (809,231), `b_kids`
  (809,284), the preview (294,199), and the visitors list's first visible row (503,171); here the console's
  `visitorwindow` is its census and `control 3720` (and the rest of the stream's ids) its places. **The run log writes a
  line's time and "PM" with U+202F, a narrow no-break space**: a pattern on the log's clock needs `\s`, not a space
  (two of Q430's checks found nothing for it). A guest who has left changes which row of the list is first: read the
  `Visitors list: sort` line before naming one.

- **A person's window in the original, and the locators in either game** (Q429, `q429/orig/run.sh`, `q429/confirm.py`):
  off-screen after `loadfile.sh load`, Ctrl+V, the visitors' button (359, 641), then the strip (380, 505) of the
  1024 x 768 frame opens the visitor's window; its OK is (816, 378), as the staff member's (Ctrl+S, the mechanics'
  button (360, 613), the strip). The window folds the arm, so the locator is closed after it. In either game, Ctrl+S
  with the visitors' locator still out leaves the staff locator closed and its buttons find nobody: press Escape first.
  An advisor message's row in `q177/skeptic-analyser/meta_full.json` is at `0x76e300 + id x 56` (the response at
  `+0x24`), then `lib/responses.py <response>` for its sample; `lib/advisorrow.py <id>` does both.

- **The pointer on a person in either game** (Q447, `q447/confirm.py`, `q447/orig/run.sh`): a locator's follow keeps its
  person at the middle of the picture, so the pointer there is on them (here 0.5, 0.5 of the window, `pick` answering
  "standing thing N"; in the original (512, 384), scanned outward until the hover's category `[0x007c24f4]` reads 7 for
  a guest or 8 to 12 for a member, the thing `[0x007b05ac]`). Here `pointer` replies the cursor, the help row and its
  text filled; pause first so the person stays on the cell the pick asks. **The original's row is the foot bar** (288 to 736 across, 722 to 758 down of
  1024 x 768; crop it from the frame), and a modifier is held through its hover with `xdotool keydown Control_L` and
  `keyup` seconds apart, here with `key_down`/`key_up` (Q467, `q467/orig/run.sh`, `q467/confirm.py`).

- **The pointer over a ride, a shop and a queue in either game** (Q478, `q478/confirm.py`, `q478/orig/run.sh`): the
  stock park at camera (480,265) 130 180 (`bothcam`); here `parkscene.find_cell` finds the queue (50,22), the Belly
  Bounce (52,24) and the Drinks Shop (43,31), a real move, then `pointer` (cursor, row and text) and the log's `Park:
  hover row 444`/`550` lines; `bothcam.to_original` gives the original's pixels, where one `xdotool` jump on `:77` and
  `tpwmem.py read` of the category `[0x007c24f4]`, the cell `[0x007b05cc]` and the thing `[0x007c24f8]` read the
  same cells, and the frame the bar and the pointer. The shop is aimed at (43,31), its cell furthest from the Litter Bin
  (44,29), which the first round's pixel (at the clamped zoom) met. The reference install's rows are `Language/american`'s ("edit this line").
- **A message box in either game, and its cross** (Q410, `q410/confirm.py`, `q410/orig/run.sh`): here the console's
  `windows` ends "MessageBox" while one is up (the reply begins `[dbg] windows:`), and `control 9873c8` finds its
  bottom place to click; R and the gadget's Research button (`control 2b`) open one in an Instant Action park. In the
  original, off-screen after `loadfile.sh load`: R (`xdotool keydown r`/`keyup`), the Research button at (166, 656) and
  the box's cross at (714, 326) of the 1024 x 768 frame close it again; B opens the buy screen for a park screen to close.
- **The hire screen's account in both games, and a month turned** (Q385): this game's `money` prints the analyser's
  money months (`month balance N slot S cashin T/N staff T/N costs T/N slot S ...`, the month so far over the newest
  closed; the balance ring has no month so far, Q387); the log's
  `Hire screen: opened, ...` and `Hire screen: candidate ...` lines and the training screen's `account a / b / c / d`
  are the figures. The park sweeps four times a second, so the stock park's first month here (near tick 1470) is
  about three minutes after the pause is lifted: wait for `Park: analyser month closed` up to eight minutes. **The
  gadget's Buy button reopens the screen it was left on**: after the hire screen it opens the hire screen itself, and
  a b_allstaff click there lands on its b_allthings. The original's figures are `tpwmem.py read 7ca2f0:i 7ca2fc:i
  7ca2f8:i 7ca300:i` (cash in, staff, all costs, other) and its rings `thingwords.py ... 5:0x1fc90:i 5:0x1fed4:i
  5:0x1f7f0:i 5:0x1f5a0:i 5:0x1f7e4:i` (`q385/orig/run.sh`). **A file this game saves after its own first month is past
  the original's month too**: the original closes a month on the load's first tick, so its screen shows what the file
  held as the month so far.

- **Does the original's draw use this at all** (Q361): before poking a texture's or a material's flags to see what
  changes, read-watch the word (`WATCH=rwatch watch.sh ADDR 4 4 15`) beside a control that is read (`mGameTick`, or a
  texture of the same model that is drawn). A poke of a word nothing reads changes nothing and proves nothing; the
  floors' did, two rounds running. Find the word by its pointers: `texcache.py` for the texture record, `pointsat.py`
  up to its material slot, material object and source record (`floormats.py` does all three).

- **A sign's board beside the original's** (Q357): Lost Kingdom's park gate, from inside the park. This game:
  `camera 480 140 80 180` and `step 2` under `pause`, saved; the original given that file through
  `original/screenpark.sh` frames the board alike since Q441 (one lens: "Cameras in both games"); any difference left is the script's own framing (`screenpark.sh` frames the camera's own point, Q359). Re-check `q357/measure.py`'s patches, which were placed for the two scales, before reusing it.
  `q357/measure.py <frame> orig|ours` reads two lettering-free patches of its face and the slab's top. The lobby's
  boards are no use: in the original's orbit the player buttons cover them, and this game's `island N` frames
  them small and mid-flight.

**In OpenTPW, through the console** (`OPENTPW_DEBUG_CONSOLE=1`; the `case` labels in `DebugConsole.cs` are the
whole list):

- **A fresh Full Simulation park, in both games** (Q349). This game: `lib/freshpark.py` (a player folder with no
  `gms.dat` is a new Full Simulation player); the park's first log line is `jungle: fresh park: ...`, the poll
  starts on tick 1, so pause at once (tick 8 on the desktop and off it), and row N's first turn is its place
  among the polled rows (row 80: tick 50). The original: a folder in the reference install's `save/users/` under
  a free slot digit, holding a Full Simulation player's `gms.dat` and empty theme folders; two clicks on its row
  of the player list (660, 158 for the second row) go straight into Lost Kingdom, which it makes fresh and
  writes as `jungle/restart.INTS` within the minute. Remove the folder afterwards (`original/freshfs.sh`).
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
  left shut as bought, which both games count as a kind standing). The Instant Action catalogue holds twelve kinds; four are on offer at the start and the lab can bring the rest. Made at
  765 (paused on 768) with `fire 30` first, the stock researcher dismissed, the file keeps the lab's eight ride
  projects and both games say the advisor's row 16 on tick 783 (Q421, `q421/all4.TPWS`).
- **A member of staff's grade written in a file, and the second tag's click** (Q350): `parkfile.thing_set( src,
  dst, 30, 398, '<i', grade )` writes the stock researcher's `mCurrentPayGrade`, which the original reads back at
  his `+0x1e4` (`thingwords.py ... 30:0x1e4:I`). **A stock park whose researcher is of grade 0 hears a training
  line on tick 1109 in the original**, so a line said after it waits some twenty ticks and its tag is the second
  on the mast: click it at game point (59, 433), not the first tag's (59, 471). A run that ends with a box or a
  screen open (the bar's research button leaves one) breaks the next `loadfile.sh` load: make it the chain's last.
- **A park bigger than its handymen, or its guards** (Q411, Q412; `lib/pathscene.py`, Q411's `q411/mkscene.py` made it
  first): the stock park paused on 765 and `path X Y` given for the first N cells of the 17 x 17 bare block at (60,15) (`parkfile.bare_blocks`), each
  with a 0.15 s wait (`stockscene.py`'s 2.5 s a command is twelve minutes for 289), then saved: 126 + N developed
  cells (`parkfile.py -d`). **A reply can come after its 0.15 s window**, so count the cells in the file, not the
  replies. The advisor's row 10 then turns on tick 777 in both games; one handyman keeps 400, so 289 cells (415) say
  his line and 286 (412) do not. Row 13 (Q412): `pathscene.py <out> <bin> big.TPWS 1230 374 -- "fire 28"` (500 cells,
  the shipped guard dismissed); it cannot score before tick 1400 (30 days since the researcher's hire on 648), and
  both 500-cell files make row 10 speak first, on 1242. **The poll writes a row's line only when its answer
  changes**: a control that stays `no-score` logs its first turn alone; read the `advisor` census for the rest.
- **A park with no guard and misbehaving kids** (Q413): `pathscene.py <out> <bin> noguard.TPWS 760 0 -- "fire 28"`
  (N may be nought: the guard alone dismissed, paused on 765), then this month's kids, caught and not, and ever written
  with `parkfile.thing_set( src, dst, 5, 73468 + 4 * i, '<i', v )` (`q413/mkkids.py`; `parkfile.py -s` prints them).
  Both games read the month's two at a load; the advisor's row 12 then turns on tick 779, four kids saying it and
  three not, and the original also posts row 264 on 903 for more than two. **A still taken after `loadfile.sh` comes
  too late for a line said a few seconds after the load** (the original off-screen): start `record.sh` as the load's
  log appears and pull the frames. Row 264 (Q414) takes the same files, both games saying it on 903 in the guards' cap
  for three kids and silent for two (`q414/two.TPWS`). **The original's pollers (`advisor.py`, `tags.py`) log a line only
  on a change**, so a wait on a logged tick past a quiet turn never ends: wait on wall time (about 3.8 ticks a second
  off-screen, the load's clock starting about 0.7 s into its log) or read the date on the end frame.
- **An object's money and satisfaction days written in a file** (Q368, `q368/mkscene.py`): in an object's record the
  costs ring is at 228, the takings ring at 361 and the satisfaction ring at 901, each the entry, the count, the wrap
  byte and today (13 bytes) then thirty days, so day `k` is `thing_set( src, dst, t, ring + 13 + 4 * k, '<i', v )`;
  `mTotalCosts` is 1086 and `mTotalTakings` 1090. Write only slots the ring has filled (`parkfile.object_days`'s
  entry and wrap), and slots the original's day rolls will not reach before the frame: it rolls a day about every
  five seconds after a load. `original/money.py` reads the result in the running original.
- **Every kind of staff written untrained, a training budget in a file, and the top tag of several** (Q351):
  the stock staff are things 25 (handyman), 26 (mechanic), 27 (entertainer), 28 (guard) and 30 (researcher);
  `thing_set( src, dst, 1, 82 + 4 * kind, '<i', budget )` writes the staff HQ's `mBudget[kind]` (0 handymen, 1
  mechanics, 2 entertainers, 3 guards, 4 researchers), which the original reads back at thing 1's `+0xc + 4 *
  kind` (`thingwords.py ... 1:0xc:i`). With all five at grade 0 both games say five lines from tick 1105 to
  about 1269 (41 s), and **a tag lives thirty seconds, so the first is gone before the fifth is said**: click
  within a second of reading where the tags stand, never after another console call. The newest tag is the
  topmost: in the original at game point (59, 471 - 37.5 x (held - 1)), here from `tags`' topmost `tagNNN=`
  rectangle, its middle over four times 0.9375 (`q351/confirm.py`, `q351/orig/run.sh`). Take a tag's words
  from `strdump` on the folder the game under test reads: strings 34 and 37 differ ("cleaners" and
  "scientists" in English, "janitors" and "researchers" in american). A run whose pollers see no change prints
  no tick after the load: give `thingwords.py` a word that moves (a mechanic's state, `26:0x19c:I`).
- **The original's Financial Information screen, and a ring's months on it** (Q386, `q386/orig/run.sh`): F in a park
  opens it on the bank balance alone, 1 Year (12 points). Its rows at game points (252, y) on the 1024 x 768 frame: Bank
  balance 122, Park value 159, Money in 208, Gate takings 245, Shop takings 283, Sideshow takings 320, Money out 358,
  Staff costs 396; a row of the money group clears the balance and park value. The graph labels only each line's
  NEWEST point; read a month by its height over the baseline (y 389), the tallest line's peak at y 150. Thing 5's rings
  poll with `thingwords.py` (`5:0x1fee0:i` a total, `+0x244` its slot, `+4 + 4 n` month n).
- **The Staff Training Budgets screen, in both games, with no tag** (Q352): the gadget's Money button, then the
  entry-price screen's staff-costs button. Here: `control 2a` gives the Money button's place, then points of the
  2048 x 1536 layout through the screen's own scale (`q352/confirm.py`'s `topix`: a wide window centres a
  screen): the staff-costs button (1619, 354); on the screen a row's minus at x 955 and plus at x 1316, the rows'
  middles y 254, 409, 563, 717, 871; the entry button (1739, 626), the tick (1752, 861). The original
  (`q352/orig/run2.sh`): the Money button at game point (107, 645), the staff-costs button (810, 177), a row's
  minus (478, y) and plus (658, y) with y 127, 204, 281, 358, 435. `budgets` reads this game's five, the flag and
  the screen; `thingwords.py ... 1:0xc:i 1:0x10:i 1:0x14:i 1:0x18:i 1:0x1c:i 1:0x20:B` the original's. **Opening
  the screen writes every budget back, so it sets `mHaveEverTrained` by itself**: read the flag before it. A
  tag's count taken from the log before the load's click is stale (`run.sh`'s `held`): click a tag only after a
  fresh `tags.py` line. This game's month turns about 715 ticks after a load, not on the file's calendar (Q149).
- **The harbour's sea at one phase in both games** (Q490): this game `pause`, then `sea phase P` (the console's `sea`
  prints the corner, phase, folder and words; `sea y H` sinks it to read `Line01` alone); the original
  `memhold.py %PID% 4 0x00871f60:f=P`, a frame grabbed 2 s in (`q490/orig/run.sh`). The same P draws the same wave in
  both (the words differ, being random); P and P + 4π are one wave apart (mad 0.00 in both), P + 2π half. At camera B
  measure beside the seaplane, which parks over the first sea box in ours: the original's (100,150)-(250,300), ours
  (254,141)-(394,281). Two frames meant half a wave apart are grabbed by wall clock, 0.52 s grab to grab: `shot()`'s
  settles put them about a whole wave apart.
- **The gates' flags at one wind and one phase** (Q491): `thingpoke.py %PID% S 15 7:0x5c:f=W` holds the weather's
  `mWindDir` (every flag turns with it; 0 streams them to +x), `memhold.py %PID% S 0x00871ed0:f=P 0x00871f60:f=P` both
  phases. **A read under a held phase can be a frame late**: the frame steps the phase 0.4 before `memhold.py` writes
  it back, so a `dynfaces.py` dump is laid at P + 0.4 now and then (`q491/orig/b-p15.0.json`): take five reads 0.1 s
  apart and keep one the model fits at P (`q491/orig/run-c.sh`). The flags' geometry repeats every 20 of phase; P and
  P + 10 differ in y alone. At camera B the nearer flags' boxes (395,435,500,485) and (750,435,870,490) are clear of the
  sea; this game's are (530,408,629,455) and (863,408,976,459). This game (Q493): `pause`, `sea phase P` (the flags wave
  at the sea's phase, so it moves both), `flags wind W` holds the wind (`flags wind free` lets it go), `flags` prints
  the count, phase, wind and folder (`q493/ours.py`).
- **This game at another graphics quality** (Q482): `gamefolder.make( dest, {} )`, then write the quality into the
  folder's own `save/Config.tcf`, the dword at byte 12 (0 low, 1 medium, 2 high; `ConfigFile.Record`'s order after the
  version: card rendering, resolution, quality), and hand the folder to `parkrun.py` as `GAME_FOLDER`. The level reads
  `low.sam`, `med.sam` or `high.sam` by it as the park loads (`q482/ours.py`, `GAME_FOLDER=... run2-low`).
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
- **The buy list's help rows in both games** (Q495): a file with the bank written (`parkfile.thing_set( src, dst, 8,
  20, '<i', 1500 )`: the stock park's economy is thing 8, `mBalance` at its record's 20) makes some rows dear. In the
  original (1024 x 768, `q495/orig/run.sh`): Buy (113, 590), the tabs at y 123 (sideshows 726, shops 786, features 845),
  the Name heading (621, 184), off the list (300, 200); a row's y by moving down x 620 from y 205 and reading
  `helpbar.py`'s `buy item` (rows about 20 apart from 220). In this game (`q495/confirm.py`) the same layout points
  through the centring above, a row's slot from the `Buy list: tab N, ... rows:` log line and the list's `rows of N
  units`; `pointer` reads the bar and `footprint` the poster's words. The text is in the frame: crop the bar.
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
- **A crowd at a happiness, in a file both games load:** `lib/stockscene.py ... 760 "fire 27" "admit 52 26"` x45
  `"admit 52 34"` x45 (thing 27 is the stock entertainer; each command takes about 2.5 s) pauses on 765 with 93 inside,
  then `parkfile.thing_set( ..., guest, 422, '<f', h )` writes each happiness (`q415/mkscene.py`; `parkfile.py -g` counts
  them). **Five of the stock guests on the road walk in by tick 781**, in both games (the original's `inside.py ... 60 30` reads 98 on 773): count 98 inside there. The original says a line before its stills: film it (`record.sh start` in the `POLL`, `q415/orig/run.sh` with `RECORD=`; Q415).
- **Two crowds from one run, past a hire's 30 days:** a script on `lib/parkrun.py` (`q416/mkscenes.py`) pauses the stock
  park on 1233 (it stops on 1236; 12 inside), gives `fire 28` (the guard), admits 168 and calls `save()`, then admits 32
  more and calls `save()` again: one game, two files, 178 and 210 inside (each `admit` about 2.5 s, so ~9 minutes). The
  newest hire's 30 days come on 1340, so the advisor's turns on 1243 score nought and 1398's are the test (Q416). **`wait_for`
  takes plain words, not a pattern**: a regex there waits out its time and the run judges nothing.
- **A littered park:** `lib/stockscene.py` pauses the stock park (on 768 from 765) with `"fire 25"` (thing 25 is the
  stock handyman), then `parkfile.litter_set( base, out, path_cells[:N] )` writes litter on the first N path cells
  (`q417/mkscenes.sh`): the stock park walks 90 cells, so 12 littered is a share of 13 and 27 of 30. Both games load
  it; the original's handyman sweeps a cell every 15 or 16 ticks, so keep him dismissed or read the share with
  `original/litter.py`. **A filmed run (`RECORD=`) waits 6 s more before its clicks**: past a tag's 30 s life, so film
  one run and click in another (Q417).
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
- **A list picks the row the pointer passes over, and `gmove.py` glides diagonally first** (Q356): from the hire
  screen's tab (667,123) to (900,700) it crosses rows 0 to 4 and leaves the fifth picked. To put the pointer away
  from a list, go straight across, then straight down, outside it: `gmove.py 1000 123`, then `gmove.py 1000 700`.
  To hover ONE row (Q376, `q376/orig/run.sh`'s `row`): from there across to (105,700), up to the row's band at x 105,
  left of every list, then straight in to x 250; the visitors' rows stand at y 170 + 27.9k, all items' at 222 + 20.4k.
  `rectcolour.py` reads the highlight on a 1024 x 768 frame in virtual units: (300, 2y-16, 680, 2y+16) for a visitors
  row centred at y, (275, 432 + 40.8k, 640, 468 + 40.8k) for an items row, (300, 420 + 39k, 700, 452 + 39k) for an
  all-staff row in either game's frame (Q382). A right click on the all-staff list below its rows: (300,700), then up to
  (300,360), crossing no row of four (Q382, `q382/orig/run.sh`).
  The hire screen: the gadget's Buy (113,590), then the buy screen's b_allstaff (958,317); the mechanics' tab (667,123).
  All staff and all items (Q365): the keys S and A (`xdotool keydown`/`keyup` on `:77`; they act on the release, from
  the park or over a screen); all staff's mechanics' tab (665,121), all items' shops' tab (786,123). All staff's five
  tabs (Q369, `q369/orig/run.sh`): (605,121), (665,121), (724,121), (783,121), (843,121). Its headings (Q381, `q381/orig/run.sh`): Name (252,180),
  Current Status (432,180), Monthly Wage (558,180), reached straight down from (x,123); this game's are controls
  `0x10` to `0x14`. To hire several of a kind here, read `candidates`: it letters handymen **"Cleaner"**. A member's skill and
  happiness in its memory: `thingwords.py` on `T:0x1e4:I` (grade), `T:0x1e8:B` (percentage), `T:0x1f8:f`
  (happiness), started after the load with its start stamped, since its times count from its own start. All items' four tabs (Q370, `q370/orig/run.sh`): rides (667,123), sideshows (726,123), shops
  (786,123), misc (845,123); V opens the visitors over a screen. All items' Name heading (Q374, `q374/orig/run.sh`):
  (341,185), reached straight across from (341,123); its rows from y 223, 20.6 apart, a right click on the row the
  pointer was last brought down to. The stock park holds one shop and one sideshow: `buy` the catalogue's (`catalogue`)
  smallest name below the stock one to make a row that is added last and must sort first (`q374/confirm.py`). A guest's row in the original's memory (Q371, `q371/orig/run.sh` with `POLL=1`): the number `+0x1d8`, the cash
  `+0x1a0`, the rides `+0x1c4`, the happiness the float `+0x19c` (`thingwords.py ... 38:0x19c:f`); in a park file the
  guest record's `+402` (`mArrivalIndex`), `+414` (cash), `+422` (happiness, a float), for `parkfile.thing_set`. The
  original's visitors rows stand 55.8 units apart from (virtual) y 340. **The reference original's resolution
  sets its font set** ("The original under Proton", "Resolution"; Q377): at its standing 800 x 600 a list's row height
  and a font's size are this game's at its 1280 x 720 window. **Its rows change two seconds after V**: the
  rewrite is not the adder (the time in park four times, Q372), so a frame of what the opening fill letters is taken
  within 2 s of the key, before moving the pointer away (`q372/orig/run.sh` with `EARLY=1`: 0.58 s). The guest's
  `mArrivalDate` is `+0x1d4` live and `+398` in a file; poll a happiness beside it, which moves, for the clock.
- **The shortcuts' keys in the original** (Q366, `q366/orig/run.sh`): B H I A S V F L T G and Space plain, S and V with
  Ctrl held (`xdotool keydown Control_L` around the key). **L opens nothing in Instant Action**, so an Escape sent after it
  opens the game menu and the next key falls on the menu: close a screen only where one opened. The gadget's
  Information button (173,596) and Money button (107,650) of 1024 x 768 (halve both the stream's x and y).
- **The locators** (Q424, `q420/orig/run.sh`): Ctrl+S or Ctrl+V as above, then the buttons on the arm at handymen
  (303,613), mechanics (360,613), guards (416,613), entertainers (332,670), scientists (388,670), visitors (359,641)
  (the mechanics' and the visitors' clicked, the rest the same layout's); its words are the locator `[0x007cc2f0]`,
  the role `[0x007cc30c]`, the person `[0x007cc308]` (a thing's address: its type byte `+2`, `+0x190`/`+0x194`, a
  visitor's number `+0x1d8`) and `gui_CameraFlags` `[0x00790ab0]` bit `0x1000`, read with `tpwmem.py read`. In this
  game the console's `locator`, and `control 95f` for a button's place. A camera against a walker's place is read
  under `pause`: two replies apart, he has walked most of a cell. **After Ctrl+S over its own locator, Escape opens the game
  menu** (seen here; the original's `FUN_004816b0` asks only an open locator): fold the panel with Ctrl+V instead. The advisor's
  lines `0xc6` and `0x11f` are `advisor.py %PID% SECS 0xc6,0x11f` in the original, the log's `message=198` and `287` here
  (Q425, `q425/confirm.py`, `q425/orig/run.sh`). A fenced member's patrol edges are `squares.py %PID% 4` after the
  kind's button in the original, and here `locator`'s `patrol=` with `strip`'s drawn census, `| patrol N x,y,marker^lift`,
  read under `pause`; the stock mechanic and entertainer are fenced, the researcher not (Q426, `q426/`).
- **What a world click is, in either game** (Q444, `q444/orig/run.sh`, `q444/confirm.py`): the original's hover category is
  `[0x007c24f4]` (`park-engine.md`'s table: 0 nothing, 1 path, 2 ground, 7 a guest, 8 to 12 a member) and the person under
  the pointer `[0x007b05ac]`, read with `tpwmem.py read` after a `gmove.py` glide; with a locator's follow on, the person
  is near (512,384) of 1024 x 768. **Click at once on the point that reads the category wanted**: a few seconds of reads
  and a frame between let a walker go (round a). Here `pick` names the cell, the person taken by their picture
  (`standing thing N`) and the cell's object apart (`object thing M`; Q474), `cell X Y` its `occupant` and `parent` (a
  queue cell's parent is its ride too: test the type), `tool` the mode as a number (`mode 0,` none, `mode 1,` the path
  tool); a press of any length lands, one let go inside a slow frame too (Q476; `button(1, what, held=0)` is a press let
  go at once, the original's `xdotool mousedown 1 mouseup 1`); for a guest or a member to stay put, `pause` once the arm has stopped (its slide is eased on `Time.Delta`), click, `resume`. A window's
  OK on a 16:9 window is the 2048 x 1536 rect scaled by the height and moved in by the side bars. **Where a member stood** (Q446, `q446/confirm.py`, `q446/orig/run.sh`): the stock park's guard 28 is alone on (39,28) in the file and walks off it within a minute in both games (the original's paces to (39,25) and back); the guards' button opens his locator, and the original's pixel over a cell with a locator up is `PICK=1 cellfind.py` (its build preview reads (0, 0) then); wait with `cellhead.py` until he is near the cell and off it, or the pixel can land at the frame's edge under the HUD.
- **The camcorder button in the original** (Q428, `q428/orig/run.sh`): the button (130,707) and, once its panel is out,
  the camera button (283,641), of 1024 x 768; its line `0x131` is `advisor.py %PID% SECS 0x131`, here the log's
  `message=305` (`q428/confirm.py` clicks `control 27`, `64` and `63`). **The original's camera button installs a pick**
  (a camcorder cursor over the orbit view, a right click cancels it), not first person. **Leave its postcard button
  out**: it pauses the game and writes a picture. **Into first person** (Q496, `q496/orig/run.sh`): the stock park with
  `setcam.py %PID% 450,165 110 180 off`, the pick, then a left click on the red path inside the gate (290, 395):
  `helpbar.py`'s `flags` reads `0x202`. A click on the hoarding (617, 378), or on the land at (700, 450) or (617, 500),
  is refused (the no-entry cursor, seen at (617, 500)) and leaves the pick up; one at (512, 420) or (450, 450) drops the
  pick back to the orbit's mode `0x6fea10` without first person (`q496/orig/a-run.out`); C puts the pick back. **The pick over a ride** (Q497, `q497/orig/run.sh`):
  Q265's `bounce.TPWS` (`q265/run-fix1/`) in its saved view, no `setcam.py`: the Belly Bounce at (600, 470), path at
  (300, 300) (the pick's click there enters first person), ground at (400, 440), though in Q498's round c that point
  read the Belly Bounce (cat 4) in orbit: read A1 before trusting it; `SCAN=1 run.sh` reads the hover's
  category over a grid in orbit to find such points in another file. **Ours into first person** (`q498/confirm.py`):
  `control 27` then `63` enter it at once (no pick, Q25); the eject button is `control 89db53`; `pointer` reads the
  cursor and the row.
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
- **Its music off:** Escape for the game menu, its Options (510,365 at the install's 800 x 600), the Music switch
  (445,533), OK (898,688), which goes straight back to the park (an Escape after it opens the menu again, and
  (445,533) on the menu is Exit To Lobby, which writes an autosave: Q442); the same clicks put it back, and
  `save/Config.tcf` then holds the same bytes. A park's mix still reads 0.04 rms with it off and every placed voice
  silenced, so one placed sound's loudness is not read from the mix: neither a match of its sample (0.04) nor its
  loop's length in the autocovariance found a waterfall 21 units off (Q263).
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
- **The arm's slide, step by step** (Q427): here, click under `pause` and `step` the frames (16.7 ms each against the
  arm's 20 ms timer; 2, 4, 7, 10 and 14 frames are 1, 3, 5, 8 and 10 steps, clear of the boundaries), `tags` reading
  `handle=`, `move=`, `carried=` and `waiting=` after each (`q427/confirm.py`); `carried=` ends `/depth=N/behind body
  N retract N of N` (how many of the panel's shown parts draw before the body and before the cross) and `retract=` reads
  `on`, `off` or `hidden` (Q445, `q445/confirm.py`). The console's `control` takes the id in
  hex (`control 27` is the camcorder button). In the original, `bar.py` polls each step and `record.sh` films it; a
  second action inside the 0.22 s slide is one `xdotool` call after the click's release (`mouseup 1 sleep 0.03 keydown
  Control_L keydown s ...`, `q427/orig/run.sh` with `C=1`). Break the ride AFTER `loadfile.sh` comes back, not inside
  its `POLL`: it came back 85 s into the poll there, past the tag's 30.5 s life.
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
- **Both games at one camera, aiming, the follow and the lens:** "Cameras in both games", above (Q435, Q436).
- **`tpwmem.py read` and `watch` take every address as hex**, with or without `0x`: a decimal one is silently
  another address, so work a sum out first.
- **A ferry or seaplane made, and a file that holds it:** load `q340/late30.TPWS` (a leaver at the stop, no
  vehicle current): the leavers' summons comes on tick 1093 and, since Q443, draws from the file's seed 0x3f416bba: the bus first in every run, which makes nothing (the seaplane is the next summons, 0x3e8846bc -> 0x975cef2b); for the ferry, write a seed whose first draw `% 3` is 2 into a copy. Save while the vehicle is current (before tick 1200);
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
- **The skill bars and the hire screen's, in both games, at chosen grades** (Q353): write the five stock staff's
  grades into a file (`parkfile.thing_set( src, dst, thing, 398, '<i', grade )` for things 25, 26, 27, 28, 30;
  `q353/grades.TPWS` is 4, 1, 0, 3, 2). In the original after `loadfile.sh` (1024 x 768 points): the Money button
  (107, 645), the entry-price screen's staff-costs button (810, 177): the training screen; the Buy button (113,
  590), the buy screen's b_allstaff (958, 317): the hire screen; the hire list's first row (693, 231), with no
  press, picks it; the mechanics' tab (667, 123) (`q353/orig/run.sh`). In this game the same through
  `q353/confirm.py`, the control's place by the console's `control 26`; a click on a hire row CHOOSES the
  candidate and closes the screen, so hover. `lib/barshare.py` measures each bar.
- **A park file entered through `screenpark.sh` (the player's click) is framed on the camera's own point**, not 65
  units on as `parkrun.original_view` gives for the Load Park list: `camera 585 235 110 180` put world (585, 241)
  mid-frame and `camera 595 175 110 180` the Staff Room (Q359, `q359/orig/e-2.png`, `f-2.png`); screen x runs against
  world x there. Under `loadfile.sh` the view is the list's, nearer again.
- **Aiming a poke of the original's ground cells** (Q360): the view `loadfile.sh` leaves on Q359's file is about
  ninety pixels a cell, and screen x runs AGAINST the cell's x (the Belly Bounce at x 51..53 mid-frame, the path at x 56
  left of it). Read the visible range first: the terrain object `[0x00879d60]`, rows `+0x128`..`+0x12c`, a row's cells
  `+0x130 + r`..`+0x1b0 + r` (signed bytes); then `groundcell.py` with `READ=` over the box, and poke inside both. Grab
  the off-screen original's frames as `loadfile.sh` does (`import -window root -display :77`), a pair before for noise.
- **A model's materials, their textures' alpha and their `0x2` bit, predicted from the files:** `q359/predict.py
  [model ...]` (from `q359/materials.txt`, `wct2raw` run over every jungle, lobby and global advisor model ONE AT A
  TIME: parallel runs abort and thin the list) gives the line each model logs at its load; `q359/flips.py` lists the
  `0x2` uses whose reference the header moves (Q359).
- **Lost Kingdom's river in both games** (Q358): `camera 480 465 110 180` (`parkrun.py`'s `original_view( 480, 400 )`),
  `step 2`, then `save()`; `original/screenpark.sh` on that file. The original's saved view stands nearer than this
  game's and frames the long reach (78 water pixels across at x 300 of its 1024 x 768 frame, this game's 36 at x 530; measured before Q441's lens, so re-measure before relying on the pixel counts),
  not the left bend. Measure a bank by `lib/seam.py` (this game's window x 430..630, y 230..330; the original's
  x 100..450, y 200..420), never by a fixed darkness level: the original's terrain is lit darker and its rocks'
  outlines touch the water. Every saved file of the same scene differs in bytes (the park runs before the pause).
  The lobby's player slot opens only Lost Kingdom for the reference player: the other islands are seen far off.
- **A list sorted in the original, whose values move as its park runs** (Q373): stamp the heading's click in the run's
  log (`echo "== click $(date +%s.%N)"`) beside `thingwords.py`'s poll, and judge the ORDER by the values polled at the
  click and each row's picture by those at the last two-second rewrite before the frame: the rewrite changes a row's
  value in place and moves nothing, so a thought that changed after the click stands out of order (`q373/orig/b`). The
  "?" heading is the 1024 x 768 point (711, 128).
- **A member of staff held in a state, in both games** (Q379): write the state into a copy of the file
  (`parkfile.thing_set( src, dst, thing, 483, '<i', state )`, `q379/mkscene.py`), load it, and in the original hold
  `+0x19c` with `thingpoke.py PID 60 <type> <thing>:0x19c:I=<state>`, one call a kind (type 5 handyman, 4 mechanic, 6
  entertainer, 7 guard, 8 researcher). Three traps, each a run: a strike (4, 5) ends at once unless the staff HQ's flag
  for the kind is set (thing 1, `+22 + 12 x kind + 4`); **state 2 or 3 with no rest area faults the original**
  (`FUN_00505fe0`, `FUN_00506d10` read thing `[+0x208]`): use 3 with the rest written (`+499`) and held (`+0x1fc:f`) at 10
  so the rest never ends; and this game's sweeps before `load()`'s pause end an idle or a job whose stamp is long past.
- **A member of staff at a linked walk's dead end, in this game** (Q380): paused in the stock park, `delpath 48 22`
  cuts the Belly Bounce's queue tail and leaves its back cell (49,22) one link, the one its own direction strikes out;
  `candidates` lists the pool, and a guard hired there with `hire <candidate> 49 22` on a tick whose low two bits are
  not nought walks, meets the dead end and takes the patrol roll (`q380/confirm.py`). From that cell no try routes: the
  log says `Staff N: thought 0x16` and the frame shows the "?" over him on the queue's fence.

- **The park gate and the ticket booths in both games** (Q471): camera (475,155) 110 180 (`lib/bothcam.py`) shows both.
  The original's 1024 x 768 pixels: booth (47,13) (520,280), gate (47,17) (500,520), its sides (46,17) (580,500) and
  (49,17) (320,540), past them (45,17) (680,520) and (50,17) (300,520); this game's 1280 x 720 at the mapped pixels
  (648,262), (629,488), (704,469), (460,506), (798,488), (441,488), each checked by `pick X Y` first. The original's
  picker answers row 17 over the arch's face, where this game's picks the ground behind it, so take a gate cell's pixel
  at the arch's foot (`q471/confirm.py`'s `find()`; `q471/orig/run.sh` reads the category, the cell and `[0x007ca210]`,
  the entry price screen).

- **The original's picture with one cause held off** (Q479, `q479/orig/run.sh`, `runc.sh`, `rund.sh`): park the pointer
  inside the frame (880,560 on `:77`), never past 0.95 of it, or the camera pans between shots. The fog:
  `memhold.py %PID% 5 0x008bd490:f=10000 0x008bd494:f=20000` (the park view takes none, so nothing changes). The
  compositor's tiles: `memhold.py %PID% 6 0x008bd550=<the word less 4>`. Land outside the park at full light:
  `HF=engine groundcell.py %PID% 6 X0,Y0:X1,Y1=-0x400` (not restored: stop the original after). This game's park
  draws no fog since Q481; the lobby's is taken off in a copy of the bin folder with `fogFactor = 0.0;` in its
  `content/shaders/test.shader` and `water.shader` (the shaders are compiled as it starts), `~/.cache/dn-tmp/q479/nofog`.

- **Where a ground pattern stands in both games** (Q488): the fence along camera A's top cannot tell a camera a cell off
  from none (its posts stand about one a cell; `q488/fenceshift.py` 0.82 at no shift, 0.78 at one post). Ask each game
  which cell stands under the same pixels instead: the original's `camera.py PID --aim X,Y` for a dozen cells straight
  after `setcam.py` (`q488/orig/run.sh`), then this game's `pick X Y` at the mapped pixels (`q488/ours.py` with
  `PICKS='X,Y;...'`): both named the same five cells. `lib/cellmatch.py` then says which tile is drawn there.
- **A texture of ours made from bytes for `test.shader`** (Q488): the shader turns every v over (`1.0 - y`), so a
  texture laid top down is drawn mirrored about its middle row; lay its rows bottom up (`GroundTiles.Lay`).
- **The original's path tiles, caught** (Q489, `q489/orig/run.sh`): the park opens with the paths in view and their
  tiles cached, so a pan catches none. Start `tilecap.sh` with the camera over them, then set the stale bit once:
  `HF=engine groundcell.py %PID% 0.3 X0,Y0:X1,Y1=+0x100` (`READ=X0,Y0:X1,Y1` with 0.1 s prints the words first); every
  cell of the box is composited again on the next frame, each path cell with its record. A texture the original holds
  in a 16-bit surface is `TextureFile`'s rows bottom up, each channel's top four bits (`q489/layout.py`, `conv3.py`).
- **`wadcat --cat` is for text**: a binary entry comes out with its bytes turned to `?`. Take a binary with `--dump`
  (`wadcat --dump base.MD2 terrain.wad` writes it to the working folder; `q488/wordcmp.py` reads its cell words).
- **The original's sea, still or gone** (Q486, `q486/orig/run.sh`): `memhold.py %PID% 5 0x00871f60:f=1.0` holds the
  wave's phase (the quilt stops; it still creeps 0.4 a frame between writes, under one level); `0x00870ecc:f=-50` sinks
  the quilt under the lake, leaving `Line01` alone, OpenTPW's picture. The period of a moving patch: film it
  (`record.sh`), then the box's autocorrelation over the frames, not an FFT of a few seconds, whose bins are too coarse.

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
| `watch.sh 'EXPR' SIZE [HITS] [SECS]` | Which code writes a piece of memory: a hardware watchpoint through `gdb`, attached and detached again. `WATCH=rwatch` asks who READS it instead (`awatch` both): a silence is evidence only beside a control that fires, `mGameTick` (`'*(unsigned*)0x80239c + 0x1da70c'`) is read at once (Q361) |

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

**Resolution** (Q377). `Data/Resolution.sam` in the reference install sets the game's own resolution (1 512 x 384 to
7 2048 x 1536; absent, the Options screen's, which is 640 x 480 for the reference player); `resolution.sh` writes it.
**It stands at 800 x 600**, Res 3, from 2026-10-15: font set 2 (`[0x0078b534]`), the set this game draws with at the
harness's 1280 x 720 window, so lettering and every font-measured size (a list's rows: 39 units in both) compare like
with like. Measured at 640, 800 and 1024 (`q377/orig`): the clock TRUE and 30 fps at 1024 off-screen; `loadfile.sh`,
`gmove.py` hovers and clicks, a right click and `screenpark.sh` (after its 3 s fix) all work at each; the frames stay
1024 x 768 off-screen and the 3D view's framing is the same (the gate's box at the file's camera, 1024 against 640:
383 pixels differ, a running park's own). **What differs**: the fonts (640: set 1; 800: 2; 1024: 3, so lettering and
a list's rows are larger or smaller than this game's), the frame's sharpness (640 and 800 are scaled up to
1024 x 768, which blurs a glyph's edges: `listcells.ink` reads a band's height only on a native frame), and at 1024 the
game's cursor is drawn MAGENTA. Take `resolution.sh 1024` for a native, sharp frame (a bar's edge, a texture), and
put it back to 800 after. A number of the original's measured before 2026-10-15 that rests on its fonts is the
640 x 480 one's.

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
`tqi/` folder, with their own venv (`CLAUDE.local.md`); a three-frame run takes under a second. Those venvs were made inside the VS Code sandbox (Python
3.13) and do not run outside it (3.12): `~/.cache/tpw-emu312` (unicorn, pefile, numpy, Pillow) does, and `lib/lndtile.py` uses it.

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
