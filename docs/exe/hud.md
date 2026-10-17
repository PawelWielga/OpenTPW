# The park HUD, from the executable

Everything the park's interface is made of, read out of `testme.exe`: the 16-bit **layout stream** opcode
format that is compiled into the executable (there is no layout file), the mesh-name hash that binds a
control to a model, the assembly function that builds a park's interface, each park panel's stream with
its control ids, rects, artwork and help rows, the six management buttons' real dispatch, and the park map
screen. Read with the park-engine page, which holds the 17 shortcut actions, the screen map (54 calls, 41 rows) and the
interaction modes.

## The layout stream opcode format

`UI_LoadTree( stream, handler )` is `UI_ParseTreeStream( stream )` followed by `FUN_0065e526( handler )`.
The stream argument is **16-bit opcodes compiled into the executable**, not a filename.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `0x0065fd58` | `UI_ParseTreeStream` | Walks the opcode stream and builds the control tree | Disassembly; six streams walked clean |
| `FUN_0065e526` | — | Installs the handler after the tree is parsed | Second half of `UI_LoadTree` |
| `FUN_0065fc18` | — | Reader: one short | Operand reader |
| `FUN_0065fbf0` | — | Reader: one dword | Operand reader |
| `FUN_0065fc38` | — | Reader: four shorts (a rect) | Operand reader |
| `FUN_0065fcc5` | — | Reader: two shorts | Operand reader |
| `FUN_0065dd7b` | — | `( this, type, flags, id, x0, y0, x1, y1 )` constructs the control; **type** selects the class (1 = panel, 2 = button, 3 = slider, and ten more up to 0xd) | Disassembly |
| `FUN_00659c42` / `FUN_0065d070` | — | Look up a mesh by hash / set it on the control | op 1 |
| `FUN_0065d1c8` | — | Sets a skin | op 2 |
| `FUN_00661830` | — | Stores the UIHELPTEXT row at control `+0x90` | op 0x11 |
| `FUN_00661850` | — | Stores a second id at control `+0x94` | op 0x12 |

### The opcodes

    op 0     [short type][dword flags][dword id][4-short rect]  -> FUN_0065dd7b, then RECURSE
    op 1     [dword hash]   the control's mesh
    op 2     [dword hash]   a skin
    op 3     [4-short rect] the text rect
    op 4     [short sub]    1/3 = +rect, 2 = +6 bytes, 4 = [short n] then n*4 bytes
    op 5     END of the current level - pops one recursion
    op 6,7   [rect] implicit child, type 2 flags 0x21 id 1/2, RECURSE
    op 8     [rect] slider thumb, type 2 flags 1 id 3, installs SliderThumb_Callback, RECURSE
    op 9     [rect] CONDITIONAL - a type 4 or 7 parent with no child id 1: scrollbar, type 3 flags 0x10 id 1, RECURSE
    op 10    [rect] CONDITIONAL - read only under a type 4 or 7 parent
    op 0xb   [short n] then n pairs
    op 0xc   [short n][rect] type 2 flags 1 id n+0x10, RECURSE
    op 0xd   [short][short]
    op 0xe,f [rect] like 6/7
    op 0x10  [short n][rect] type 6, id n|0x11100000, RECURSE
    op 0x11  [dword] the UIHELPTEXT row
    op 0x12  [dword] a second id

### Operand sizes, read out of `UI_ParseTreeStream` rather than inferred

After the 2-byte opcode:

    0    18 then RECURSE     1  4     2  4     3  8     5  0 and RETURNS (this is the pop)
    6,7,8   8 then RECURSE   0xd  4   0x11  4  0x12  4
    0xc, 0x10   10 then RECURSE        0xe, 0xf   8 then RECURSE
    4    2 sub-op, then 8 / 6 / 8 / (2 + 4*n) for sub-ops 1 / 2 / 3 / 4
    0xb  2 count, then 4 * count
    9, 10   READ NOTHING WHEN THEIR CONDITION FAILS - the stream's length is state-dependent

**`0x10` is 10 bytes, not 4** — a walker that guesses 4 mis-walks the whole tail.

**Ops 9 and 10 are runtime-conditional.** They read their rect *only* under a type 4 or 7 parent (op 9 also
only while that parent has no child id 1, `0x0066043e`), so a static walk must track the enclosing control's
type. None of the park panels uses them; the buy and hire lists do ("Ops 9 and 10 are runtime-conditional" below).

### Which control an attribute binds to

`case 0` recurses with **ECX = the new child** (`0065fe4c: MOV ECX,[EBP-0x30]`), but `case 1`, `2`, `3`,
`0x11` and `0x12` all load **ECX = `[EBP+0xffffff60]`, the RUNNING invocation's own `this`**
(e.g. `0065fe75`). So a mesh, skin, text rect or help row binds to **whichever control's block encloses
it** — and after an `op 5` that is the **parent**, not the child that just closed.

**The trap:** a dumper that prints attributes without marking the closes makes a parent's mesh look like it
belongs to the child printed just above it. Such a dumper puts the `guage` mesh on the meter
`0x1f` when the bytes put it on its housing `0x1e` (`... 05 00 | 01 00 9f 57 d3 69 | 05 00` at
`0x007529a0` — close, THEN mesh, THEN close). A static walker **must** track op 5 to attribute
attributes, because `case 0` recurses and `case 5` returns: that is why `guage` lands on `0x1e` and `base`
on `0x1d` though each textually follows a later id. Always print CLOSE markers and bind by an explicit
stack; attributing by "the id that precedes it in the bytes" is wrong.

### The self-check that proves the decode

Every stream address from the screen table begins with **op 0** and the walk **terminates on a
balanced op 5**. Six streams walked, six clean. If a walk ends mid-record or hits an unknown opcode, the
decode is wrong — do not "fix" it by skipping bytes.

## The mesh-name hash — cracked, 39 of 39

    h = 0
    for each character c of the name:  h = ((c ^ h) * 47) & 0xFFFFFFFF

The name hashed is **the full, untruncated name of the model's FIRST mesh node** — not the member name,
not a clamped one. `w_dialog_wave.MD2`'s first node is `wdialog w` -> `wdialogw`; the truncation already
happened when the artist named the node, so **do not clamp it yourself**.

Hashing the 1,202 `ui.wad` **member names** instead gives **zero** hits across five inits and four
transforms — the wrong name source. Node names come from `wadcat --anim <wad>` (the C# tool in `CLAUDE.local.md`, not the `wadcat.py` harness below); its `--meshes` mode
lists each model's mesh node names too, first node first.

### Node name versus file name

The stream asks by the first node's name; the loader loads `ui/<FILE>.md2`. Mapped from
`wadcat --anim ui.wad`:

| Node name (hashed) | File | Note |
|---|---|---|
| `pan_money` | `panel.md2` | the arm |
| `pane_money` | `panelend.md2` | the arm's end |
| `messagel` / `messagem` / `messager` | `f_tagl.md2` / `f_tagm.md2` / `f_tagr.md2` | message bar slices |
| `base` | `mainpanel.md2` | the gadget body |
| `handle`, `b_retract`, `b_1person`, `b_postcard`, `mb_erase`, `aerial` | same as their node names | — |

**`pan_money` and `pane_money` differ by one letter and sit one control apart** — guess either and you
silently swap them. `panel.md2` and `panelend.md2` carry **four parts each**, and they are not animation
frames: `pan_money` / `pan_info` / `pan_buy` / `pan_staff` and the matching `pane_*`, so the arm wears the
colour of whichever category it carries.

## What builds a park's interface: `FUN_00489f50`

It is the assembly point. In order:

1. Installs the **interaction mode** if `DAT_007b05e8` is null — the default type-1 mode `FUN_0046c6a0`
   through the setter `FUN_0046c350`.
2. Creates **two full-screen layers**, `FUN_0065dd7b( 1, 0, id, 0, 0, 0x7ff, 0x5ff )` — **2047 x 1535, the
   same 2048x1536 virtual screen the lobby uses**. Layer id 0 gets handler `Park_MouseMessageProc` (`0x004881a0`) and is made
   visible; layer id 1 gets handler **`FUN_00488a00`**.
3. On layer 1: `FUN_0065f11d( 0x15 )` - the layer's **cursor**, `c_crosshair.ani` (`FUN_00489720` registers
   it as `0x15` at `0x004899a8`) - then `UI_LoadTree( 0x0074fa98, 0 )`, the camcorder viewfinder.
4. Then the six panels, each `UI_LoadTree` + `UI_SetVisible(0)` — **built hidden, except the gadget**:
   `FUN_004a1d70` loads its stream onto layer 0 (`0x004a1da4`) and hides only `0x30` (`0x004a22ba`), and `0x34` in
   Instant Action (`0x004a1fa8`). The other five load onto the UI root.

| Address | What it builds |
|---|---|
| `FUN_004a1d70` | management gadget |
| `FUN_00498b50` | camcorder / postcard |
| `FUN_004b3cf0` | locator |
| `FUN_00497b20` | finance |
| `FUN_0048d8b0` | message bar |
| `FUN_00498790` | coaster builder |

**`FUN_00488a00` is layer 1's handler**, and layer 1 has the focus only in first person (`FUN_004862a0`). Its key
messages run camera-table handlers only: `0x1000a` -> `FUN_0040c900( key, mods )` and `0x1000b` -> `FUN_0040c990`.
On a key-up it looks up two shortcuts actions without running them: 0x10 (camcorder) takes the same exit as key
0x1b, and **0xf = postcard** calls `FUN_004a9380`. The camera, game, shortcuts and cheat handler chain is layer 0's,
`Park_MouseMessageProc` (`0x004881a0`) - see `park-engine.md`, "How a key is matched" and "The hand's ways out", and
`scenes.md`, "The park Escape route". The coaster bar's `FUN_004982a0` is a third route, through the camera and
coaster tables.

## The management gadget — stream `0x00752940`

Built by `FUN_004a1d70` onto the park's layer 0, with no handler for the stream (`UI_LoadTree` is passed 0,
`0x004a1daa`); `FUN_004a0a20` is installed on the body `0x1d` alone (`0x004a231d`). Four root controls:

    0x1d  panel  ( 37, 984)-( 439,1507)   the bottom-left gadget body      mesh base
      0x1e  panel  help 477                                  mesh guage     (gauge housing)
        0x1f  t9   ( 85,1100)-( 144,1324) no mesh      + meter.wct skin
      0x20  panel  help 478  + text rect  mesh date    <- THE DATE READOUT
      0x21  panel  ( 331,1069)-(1006,1495)  THE ARM   mesh pan_money  = panel.md2
        0x22  panel (1006,1069)-(1071,1382)  its end   mesh pane_money = panelend.md2
          0x23  panel ( 976,1058)-(1129,1503) handle   mesh handle     = handle.md2, 16-point region
        0x24  button ( 349,1386)-( 429,1466)  help 481  mesh b_retract  (child of 0x21)
      0x25  panel  (170,1122)-( 392,1372)  THE SIX ROUND BUTTONS
        0x2c  panel  help 494  (child of 0x25)
    0x2d  panel ( 94, 918)-( 144, 984)  mesh aerial    0x2e  help 476, mesh aerialtop  (77,790)-(161,918)
    0x2f  panel (258,  60)-( 720, 260)    0x30 (458,273)-(720,363)   0x31 (728,109)-(831,211)
                                          0x32 ( 37, 58)-(242,262) mesh i_dollar
    0x33  panel (1688, 48)-(1963, 278)    0x34 gkey (1853,165)-(1955,268)  0x35 gtick (1853,54)-(1955,156)
                                          0x36 (1688,171)-(1829,261)  0x37 (1688,60)-(1829,150)

### How `FUN_004a1d70` populates the clusters

| Control | What the builder does | Evidence |
|---|---|---|
| `0x36`, `0x37` | **Text labels**, `LabelSkin_Construct`, colour **white** (0xff,0xff,0xff,0xff), font `FUN_00485a70(2)`, both seeded with the string at `0x00752f24`. They sit to the LEFT of the two icons, so the cluster is icon+count twice | Disassembly of `FUN_004a1d70` |
| `0x34` (`gkey`) | Takes **frame 3** (`FUN_0065d3a3(3)`) — what the four `gkey0-3.wct` textures are for — and **is hidden outright in Instant Action** (`DAT_00fb3b7c == 2` -> `UI_SetVisible(0)`) | Disassembly |
| `0x30` | **Yellow** (0xff,0xff,0,0xff), font 2, and **starts hidden** (`UI_SetVisible(0)`) — matches it being the price of the item in hand, absent when your hand is empty | Disassembly |
| `0x2f` | Its field is sized by **measuring the string `999999999`** (`UI_MeasureText` on the string at `0x00752f10`), white, font `FUN_00485a70(1)`, skin `FUN_0048fde0(0)`; `0x32`'s rect is then computed from that measurement rather than being fixed | Disassembly |
| `0x2d` / `0x2e` | Callbacks `LAB_004a11e0` and `FUN_004a1190`; the mast is shrunk to eight units and the top follows it down ("The arm and the aerial as built" below) | Disassembly |
| `0x1f` | Takes the `meter.wct` skin | Confirmed twice, independently |
| `0x20` | The date is **black** — `FUN_0065c5d5(0,0,0,0xff)` with font slot 3 (`_DAT_007cb2d4 = 3`) | Confirmed twice, independently |

### Depth

`FUN_0065f16b( this, n )` writes n to the control's `+0xe0`, clamps at `0x100`, and recurses into children
at n+2. The receivers are `__fastcall` and **Ghidra hides them** — read the disassembly:

    0x21 arm 4   0x22 end 4   0x24 b_retract 7   0x1d body 8   0x23 handle 9   0x25 six buttons 10

**Higher is nearer the front**, settled by screenshot: the six buttons sit at 10 inside a body at 8 and
plainly draw over it. So the arm draws **behind** the body, and the handle in front of it. Built the other way
the arm lays a hard seam across the panel. A control linked later takes its parent's depth and two when the caller asks for it (the second argument, `0x0065f4d6`)
(`FUN_0065f447`).

### The arm and the aerial as built

A control's flags `0x10000`, `0x20000`, `0x40000` and `0x80000` make it follow its parent's left, right, top and
bottom edge when the parent's rectangle is set to another size (`FUN_0065cc04` → `FUN_0065c9bd`, which moves the child
by that edge's change through `FUN_0065c7fe`; `0x30000`, `0xc0000` and `0xf0000` together stretch it instead).
`FUN_0065c7fe` moves a control's regions and then every child the same way. In the gadget's stream the arm's end
`0x22` and the handle `0x23` carry `0x20000`, and the aerial's top `0x2e` carries `0x40000`.

**The arm is built in.** `FUN_004a1d70` reads the arm's rectangle and the handle's, adds the arm's left less the
handle's (331 − 976 = −645) to the arm's right (`0x004a23da`) and sets it (`0x004a23e2`): the arm is
(331,1069)-(361,1495), 30 wide, and its end, following the right edge with the handle inside it, sits 645 left of the
stream: the end (361,1069)-(426,1382), the handle (331,1058)-(484,1503) with its 16 points moved as far. The handle is
at depth 9 over the body's 8, so with the arm in it is the blue grip that shows past the body's right edge (439);
every point of the narrowed arm and of its end is inside the body's outline or the handle's. The retract button `0x24`
(flags 1) stays at its stream rectangle, switched off.

**The aerial is built down.** The builder sets the mast `0x2d`'s top to its bottom less 8 (`0x004a2057`,
`0x004a205c`): (94,976)-(144,984). The top `0x2e` follows the mast's top edge, 58 down: (77,848)-(161,976), resting on
the body's neck. (The move at `0x004a2034` before it, the mast's top less the top's bottom, is nought in this stream.)
`LAB_004a11e0`, the mast's handler, answers `0x100001`, a message's icon handed to it: it links the icon as its child,
posts it message 6 with 1 to arrive 500 ms later (`FUN_00658df4`, `0x004a1292`), and grows the mast upward by the icon's height and 10 (`FUN_0065cd95( 0, -(h + 10), 0, 0 )`,
`0x004a12e3`), which lifts the top as far. `FUN_004a1190`, the top's handler, answers a right click (`0x10006`, button
1) by sending the mast `0x100003`, and the mast's `0x100003` arm (`0x004a13b7`) walks the icons from `[0x007cb2c8]`,
posting each message 5 (close) with a delay 100 ms longer than the last one's: "Right-click to delete ALL messages", help row 476. Seen in the original:
with no message the red top sits on the neck; with one, an icon block stands between them.

**The hit test's order.** `FUN_0065db25` skips a control flagged `0x2` or with `+4` set, and one with flag `0x1` clear and none of `0xf0000000` set (`0x0065db55`), tests every child in turn
whatever the parent's own region says, keeps the LAST child that answers, and answers itself only when no child did.
Children are linked at the tail (`FUN_0065f447`), so in stream order. Under the body: the gauge `0x1e`, the date
`0x20`, the arm `0x21`, the six buttons' panel `0x25`. So the panel answers before the arm where they meet, the arm
before the date (the date's x 331 to 361 with the arm in), and any child before the body's outline.

**Measured in the original** with the path tool armed and RMB cancel on (`[0x0078d911]` 1), reading the mode object
`[0x007b05d8]`, which a right click that reaches the park replaces: a quick right click on the body's bare metal, the
red top, the mast and the handle past the body's edge left it alone, each tried once or twice; one on the grass, and
one in the corner of the body's rectangle outside its outline, replaced it, four of four. Over the red top the help
bar read row 476. The arm out was not tried there.

**OpenTPW.** `ParkGadget` builds the body's children in that order under a body that draws by depth
(`UiControl.DrawsByDepth`, `Depth`), reads both outlines into `UiControl.Outline`, and builds the arm in
(`BuildIn`) and the aerial down. The arm, its end, the mast and the buttons' panel stop the pointer over their
rectangles (`UiControl.StopsPointer`). The arm slides ("The arm's handler", Q427). Not the original's: at rest in, the arm
and its end draw nothing, where the original draws them fitted to their narrowed rectangles behind the body and the
handle (the film of Q427 shows the arm's mesh squeezed to its rectangle through a slide), and the retract button is
hidden while the arm is in or coming in. A tag stands
on the mast and lifts the top (`ParkGadget.PutTags`), and the top's right click takes every tag off.

### Op 4 sub-op 4 is a hit-test region, not a motion path

All four sub-ops end in `FUN_0065cfff(obj)` after building a shape — sub-op 1 from a rect, sub-op 4 from
n points via `FUN_0066c4a1(n, pts)`. So `handle`'s 16 points and the gadget body's 23 are **click masks**.
Reading them as a path would have animated the arm along nothing.

### The six round buttons, ids 0x26-0x2b

Handler **`FUN_004a0840`**:

| Id | Mesh | Help row | Dispatch |
|---|---|---|---|
| `0x26` | `b_buy` | 469 | `FUN_004a0940(1)` |
| `0x27` | `b_camera` | 474 | press `FUN_00498bb0` / release `FUN_00498bd0` (shows the sub-panel) |
| `0x28` | `b_info` | 470 | `FUN_004a0940(2)` |
| `0x29` | `b_map` | 473 | `FUN_005f0b40` (the map screen) |
| `0x2a` | `b_money` | 471 | `FUN_004a0940(3)` |
| `0x2b` | `b_resrch` | 472 | `FUN_004aa480` (research) |

**The HUD is six buttons, not 17, and three of them are category pickers.** `FUN_004a0940( n )` opens
*the screen that category was last left on*, from three globals seeded 1 / 3 / 10 in `FUN_004a1d70`:

    n=1  DAT_007cb28c  1 -> buy FUN_004acc70      2 -> hire FUN_0049bdd0
    n=2  DAT_007cb2a0  3 -> parkstatus  4 -> allstaff  5 -> allitems  6 -> allpeeps
    n=3  DAT_007cb290  7 -> financeinfo  8 -> loans  9 -> staffcosts  10 -> entryprice

It does nothing at all when `FUN_0048c8d0()` returns 1, which needs **both** `DAT_007c2534` non-zero
**and** `FUN_006ad810()` non-zero - that is, while the game menu exists and is on screen (see "The gate that looks
like a bug, and is not"). This is why the 17
shortcut actions and the gadget's button count never reconciled — they are not meant to.

**Who writes the three globals (Q330).** Besides the seed (`0x004a1db4`) their one writer is
`FUN_004a0810( category, screen )`, and its ten callers are the ten screens' openers, each for its own screen. The
buy opener's is its first act, `(1, 1)` at `0x004acc8f`, before it looks whether its screen is up; the hire
opener's is `(1, 2)` at `0x0049bdf5`, after `FUN_00485b40` has destroyed the park screen that is open. So a
category is left on a screen whoever opened it: the sibling's button, the message bar's, the gadget's own. The
hire opener asked for -1 always builds, so the gadget's button with the hire screen up destroys it and builds it
again, which withdraws `0xbd` and posts it again (a cut while he says it, and the line said again); the buy
opener with its screen up picks its tab again and no more.

**The seed is the entry from the front end and nothing else.** `FUN_004a1d70` is called by `FUN_00489f50`, that
by `FUN_004813e0`, and that by `Game_StateMachine` at `0x0054ed25`, state 9, alone. A park loaded from the
park's own Load Park list keeps what the park before it left, and so does Restart Park. Restart Park is
measured, not read: its confirm `FUN_0048b5b0` asks `FUN_005508b0( 1 )`, and what serves that was not read to
its end (my guess from the part read, that it passes state 9, was wrong).

Measured in the original, the stock file in four fresh programs, each predicted first (`q330/PREDICTION.txt`;
`q330/orig/run.sh <file> a|b|r|l`, `original/screens.py` beside `advisor.py`; nine of ten, the miss Restart
Park's, predicted again and run again):

| What was done | `DAT_007cb28c` | Screen up |
|---|---|---|
| the park entered | 1 | none |
| the gadget's button | 1 | buy |
| the buy screen's button to hire | 2 | hire; `0xbd` (55, 0, 0, 1) to (891, 0, 1, 1) |
| the gadget's button 4 s into his line | 2 | another hire screen (`0x46cee10` to `0x46cedf0`); `0xbd` (891, 0, 0, 2) on tick 919, (920, 0, 1, 2) on 920 |
| exit, the gadget's button | 2 | hire, nobody speaking (three cuts) |
| the hire screen's button to buy | 1 | buy |
| exit, the gadget's button | 1 | buy |
| hire, exit, the same file loaded from the list, the gadget's button | 2 | hire |
| hire, exit, Restart Park (the clock back from 916 to under 882), the gadget's button | 2 | hire |
| hire, exit, Exit To Lobby, the park entered again, the gadget's button | 1 (written as the clock read 0) | buy |

**OpenTPW** (`ParkCategoryScreens`): `ParkBuyScreen.Shown` and `ParkHireScreen.Shown` remember their screen,
the gadget's button opens the one remembered (`Open`), and `Game` seeds the three as a park is asked for from a
scene that is not a park (`ParkAsked`). Information is still seeded 4, the declared deviation. The stock park,
real clicks, predicted first (`q330/confirm.py fix|control|restart`): nine of nine and the control. `windows`
ends `; left on 1,4,10`; the button logs `Park gadget: category 1 is left on screen 2` after the hire screen
was up; clicked 3 s into his line with hire up, `withdraw message=189 cleared=0 cut=1 slaps=2`, a post on tick
815 and `attempt=3 message=189 ... tick=816`; left on hire it opens hire after `savepark` and a load from the
list, and after Restart Park; after `lobby` and the park again `left on 1,4,10` and it opens buy. Main's build
opened buy after hire and logged no such line. Sixteen bugs put back, sixteen caught
(`q330/mutations.py`); the one line no test reaches is `Game`'s own call, which the load, restart and lobby
runs hold. Not run: Full Simulation; the Information and Finance categories in either game (their four built
screens remember through `ParkCategoryScreens.Show` alone, so the message bar's entry-price button does, and
their seed is not put to the original); a key that opens a screen.

### The nine screens behind Info, Money and Research — censused 2026-09-21

Every one located, with its builder, its layout stream, its refresh callback, the size of the builder
and the control count parsed out of its stream. **The control counts are LOWER BOUNDS**: the stream
walk misses children of composite controls (a list's own column headers never appear).

| # | Screen | Builder | Stream | Callback | Builder bytes | Controls |
|---|---|---|---|---|---|---|
| 3 | parkstatus | `FUN_004a52a0` | `0x7537d0` | `0x4a4830` | 2926 | 28 |
| 4 | allstaff | `FUN_00496620` | `0x750e10` | `0x495da0` | 1438 | 37 |
| 5 | allitems | `FUN_00495aa0` | `0x7508e0` | `0x495290` | 342 + four sub-builders | 12 |
| 6 | allpeeps | `FUN_00493530` | `0x7506c8` | `0x493230` | 677 | 5 + 6 columns |
| 7 | financeinfo | `FUN_0049ac60` | `0x751ca8` | `0x49a0b0` | 2238 | 19 |
| 8 | loans | `FUN_0049fb30` | `0x7525a0` | `0x49f4e0` | 1905 | 18 |
| 9 | staffcosts | `FUN_004b2750` | `0x756400` | `0x4b24b0` | 2990 | 30 |
| 10 | entryprice | `FUN_00498d80` | `0x751798` | `0x498c60` | 721 | 9 |
| — | research | `FUN_004aa480` | `0x754490` | `0x4a96b0` | 2616 | 41, six sliders |

What each shows, from its own decoded labels:

- **parkstatus** — People in park / Arrival rate / Average happiness / Average time in park / Park
  rating, Top 3 Thoughts, Happiness Levels (`hap.wct` and `sad_undec.wct` gauges), Total park
  visitors, and a 1/3/12-year graph.
- **allstaff** — a five-column list: Name / Current Status / Monthly Wage / Skill / Happiness, plus
  average-happiness gauges.
- **allitems** — four tabs of placed objects, the same four categories the buy screen uses.
  **The four tabs do NOT share a list shape** — see below.
- **allpeeps** — a six-column guest list: Visitor Number / Cash Remaining / Time In Park / Rides
  Ridden / "?" / Happiness. Its row adder `FUN_00493800` fills them from the guest's `+0x1d8`, cash `+0x1a0`, a
  quarter of the park hours since the guest's arrival stamp `+0x1d4` (`mArrivalDate`, `mGameTick` at construction,
  `0x004fafdb`; `FUN_004fd950` shifts the ticks since right by two before `FUN_004f8730` turns them into park time,
  divided by 36,000,000,000 at `0x0049383e`),
  `mNumRides` `+0x1c4` (`0x00493850`), the last thought `+0x30` less one or 999 for none (`0x0049385a`), and happiness
  as a 0..1024 bar (`0x00493870`).
- **financeinfo** — Bank balance / Park value / Money in / Gate takings / Shop takings / Sideshow
  takings / Money out / Staff costs, over the same year graph.
- **loans** — "Available Loans": Lender Name / Amount / Interest Rate / Monthly Repayment.
- **staffcosts** — "Staff Training Budgets": Cash in / − Staff costs / − Other costs / − Loan
  payments / Balance.
- **entryprice** — one "Ticket Price" row. It writes the bank's `+0x118` (the thing `FUN_005195d0` answers, not the park object) through
  `FUN_004d05d0`, whose format string reads *"Admission fee set to %d"*. The setter does that and no more
  (`0x004d05e8`); the fee's two readers each take `+0x118` as they run, a guest's judgement (`FUN_004ff5b0`,
  `0x004ff602`, logged as *"GP: %d, AF: %d"*) and the charge (`FUN_004d0600`, `0x004d0609`), so a new price is the
  next judgement's, a sulking guest's second one included. OpenTPW's gate reads the running park's fee the same way
  (`ParkAdmission.Fee`, Q145).

### The four built screens, and what walking their streams alone could not give

**Column headings are in CODE, not in the layout stream.** Every `op 0xc` header child carries a rect
and nothing else; each builder then fetches it by id **`0x10 + index`** and hands it the text of a UITEXT row
(`FUN_00485b00( row )`) through `FUN_00486660` ("A list's order"). A stream walk therefore yields the right number of
unnamed boxes and no labels at all. The rows, read back from `UITEXT.str`:

| Screen | Header ids | UITEXT rows | Headings |
|---|---|---|---|
| allstaff | `0x10`–`0x14` | 101–105 | Name / Current Status / Monthly Wage / Skill / Happiness |
| allpeeps | `0x10`–`0x15` | 113–118 | Visitor Number / Cash Remaining / Time In Park / Rides Ridden / **`?`** / Happiness |
| allitems, rides | `0x10`–`0x14` | 82–86 | Name / Users Last Month / Excitement / State Of Repair / Remaining Life |
| allitems, shops | `0x10`–`0x14` | 87–91 | Name / Customers Last Month / Profit Last Month / Total Profit / Customer Satisfaction |
| allitems, sideshows | `0x10`–`0x15` | 92–97 | Name / Customers Last Month / Excitement / Profit Last Month / Total Profit / Customer Satisfaction |
| allitems, misc | `0x10`–`0x11` | 98–99 | Name / Number Owned |

**UITEXT row 117 is literally `"?"` in the shipped file.** The fifth visitor column has no heading in
the original either; it is not a decode failure.

**allitems loads THREE different list trees for its four tabs**, through four sub-builders that
`FUN_00495aa0` dispatches on the tab index:

    case 0 rides      FUN_00494ec0 -> tree 0x750ab8   5 columns
    case 1 shops      FUN_00494c70 -> tree 0x750ab8   5 columns (the same tree)
    case 2 sideshows  FUN_00494250 -> tree 0x750ba0   6 columns
    case 3 misc       FUN_00495110 -> tree 0x750ca0   2 columns

`FUN_006636b2( column, type )` sets each column's type, 0 text and 1 a number, which is what the list sorts it as
(`FUN_00663edc` reads it back through `FUN_006636f6`) and what the screens here take the alignment from: allstaff
passes 0,0,1,1,1 and allitems 0,1,1,1,1, but **allpeeps passes 1 for all six** — its name column is a visitor *number*.

**Tab ids are not in screen order.** allitems' switch takes case 1 to `0x12c4bc` (at x 1521–1623) and
case 2 to `0x12c4bb` (at 1402–1504), so laying the tabs out as a stride from the first one puts shops
and sideshows in each other's places while looking correct.

**`FUN_004a0810( category, screen )` is the seed setter**, called at the top of every builder —
`(2,4)` in allstaff, `(2,5)` allitems, `(2,6)` allpeeps, `(3,10)` entryprice. It confirms the
1 / 3 / 10 seeding above from the other direction.

**entryprice is not what its rects suggest.** Its three stacked right-hand buttons are **not** a
spinner: they resolve to `b_staffcost`, `b_loans` and `b_finance`, and `FUN_00498c60` sends them to
`FUN_004b2750`, `FUN_0049fb30` and `FUN_0049ac60` — this category's other three screens. The spinner
is `0x4f3ae`, control **type 12**, whose `op 0x0f` child is `b_minus` (left) and `op 0x0e` child is
`b_plus` (right); `FUN_0066a5c4(&0,&10000)` gives it a range of **0–10000** and `FUN_0066a68d(&1)` a
step of **1**. `0x4f3b0` is the black "Ticket Price" label (UITEXT 160), and `0x4f3b1` is `b_door`,
the park open/closed switch — `FUN_00519ef0( down != 1, 0 )` (`0x00498d33`), whose first argument is the OPEN
flag, so **down is closed**; the builder sets the switch down for a closed park (`Button_SetDown`, `0x00498fd9`),
once, when the screen is built. What the door does to the rides: `ride-operation.md`, "The closed ride". The fee is held in a global as the spinner moves (message `0x800`) and written on
dismissal (message `0x14`).

**allstaff's kind → happiness-label switch is deliberately out of order**: case 0→`0x6b`, 1→`0x6c`,
**2→`0x6e`**, **3→`0x6d`**, 4→`0x6f`. The string file lists guards (109) before entertainers (110)
while the kinds run cleaners, mechanics, entertainers, guards, scientists. Written as a plain 107–111
run, the guards tab draws a list of guards under the heading *"Entertainers' Happiness"*.

**Both list screens arm a 2000ms refresh timer** — `FUN_0065ef90( 0x80083, 2000 )`, the same timer id
the gadget's happiness gauge uses. Rebuilding such a list every frame instead is measurably wrong,
not merely untidy.

#### Resolving a mesh hash to a file name

The layout stream names a mesh by `h = (c ^ h) * 47` from zero over the model's **first node name**,
which is often not the file's name. Hashing every stem in `ui.wad` and matching resolves them — but
**only `.md2` entries are candidates**, because `UiMesh.Get` loads `ui/<name>.md2`. Matching against
all 1202 entries instead of the 278 models picks up textures and gives names that load nothing:
`b_pkinfo` and `b_sresrcher` are both `.wct` stems whose models ship as **`b_parkinfo.MD2`** and
**`b_sresrhcer.MD2`**. Validate the arithmetic against a known pair before trusting any match —
`b_buy` → `0x2135e13d` and `base` → `0x1beb0695`.

Resolved this way: `list_allstaff`, `list_kids` (allpeeps), `list_all` (the allitems tab group),
`b_finance`, `b_loans`, `b_staffcost`, `b_door`, `b_plus`, `b_minus`, `b_parkinfo`, `b_kids`,
`b_allstaff`, `b_allthings`, `b_sguard`, `b_sresrhcer`.

**The screen ROOT FRAMES resolve too.** Hashing file
stems leaves them unmatched, and scanning the models' raw bytes finds nothing either — every one of
ui.wad's 278 `.md2` members is **refpack-compressed**, so a byte scan reads compressed noise. Decompress
them (the tree's own `WadArchive` + `ModelFile` do it) and read the node names, and all three fall out:

| Hash | Node | File | Used by |
|---|---|---|---|
| `0xf76e4200` | `window4` | `w_big.MD2` | buy, hire, allstaff, allitems, allpeeps |
| `0xf76e42eb` | `window1` | `w_small.MD2` | entryprice |
| `0x257b71f9` | `varibox` | `f_varibox.MD2` | the entry-price spinner |

The two root hashes differ by `0xEB` = 235 = **47 × 5**, which is the signature of two names sharing
every character but the last — `window4` against `window1` — and that was visible before either was
identified. The ride window's `window2` in `w_med.MD2` is the third of the same family.

**A caution about every debug string quoted on this page and the next.** `FUN_005da3c0`, the logger
they are all handed to, is an **empty stub in the shipped build** — `void FUN_005da3c0(void) { return; }`.
So none of these lines is ever printed at runtime. They remain first-class evidence of **field names
and intent**, which is what this project uses them for and why they are quoted verbatim; they are
**not** evidence that the game emits anything, and "the original logs X" is wrong as a statement of
behaviour.
- **research** — six effort sliders. `FUN_004aa480` opens it only with the game menu down (`FUN_0048c8d0`) and the
  screen not already open (`DAT_007cc1d8`), and opens a message box instead in two cases: UITEXT `0x1d4` (*"Research is
  automatic in Instant Action mode."*) when the game type `DAT_00fb3b7c` is 2, and UITEXT `0x1d3` (*"You need to hire
  some scientists before you can carry out any research!"*) when `mFirstResearcher` (`+0x1da742`) is nought.

**Five are built and four are blocked on simulation this project does not have.** Built: **entryprice**,
**allpeeps**, **allstaff**, **allitems** and **staffcosts** ("The Staff Training Budgets screen", below). Blocked: **loans** (taking and repaying a loan: `ParkWorld.EconomyState.Loans` reads the save's `mLoans[]`
records, `ParkState` keeps them and the month's change pays the bought ones' instalments (`TurnTheMonth`); taking one is not built), **financeinfo** (the
money-in/out split and the graph history) and
**parkstatus** (Top 3 Thoughts, arrival rate, park rating, multi-year history), each counted when opened
(`LOANS_SCREEN`, `FINANCE_SCREEN`, `PARK_STATUS_SCREEN`), and **research** (`mResearchDone`,
`mResearchGroup`, `mFirstResearcher`, and per-group research points), whose screen is not built. The key reaches the
same function: the shortcuts' row 11 is `R` (`0x52`) with no modifier, handler `0x0040c5b0` (`MOV ECX, 0x007b51f0; CALL
0x004817e0`, a `JMP` to `FUN_004aa480`), heard where that table is run: over the park and under a park screen, not
under the game menu, a message box, the options, the map, the full-screen view or first person.

So the three buttons are not one job: **Info has 3 of 4 built, Money 2 of 4, and Research its two boxes and not its
screen.**

### The research opener and its two boxes (Q410)

`FUN_004aa480`, decompiled 2026-10-16, is what the gadget's Research button (`0x2b`), R and the message bar's button of
type `0x13` all run. In order: nothing with the game menu up (`FUN_0048c8d0`) or the screen open (`DAT_007cc1d8`); the
park screen that is open closed (`FUN_00485b40`); the mode object made once (`FUN_00550ca0`, behind `DAT_00fb34be` bit
1); then, when the game type `DAT_00fb3b7c` is 2 (Instant Action), `MessageBox_Open( &text, 2, 0, 0, 0, 0, 0, 0 )` with
UITEXT `0x1d4`, "Research is automatic in Instant Action mode.", white (`ff ff ff`) in font 4, and return; when
`mFirstResearcher` (`+0x1da742`) is nought, the same box with UITEXT `0x1d3`, "You need to hire some scientists before
you can carry out any research!"; else the screen's tree (`0x00754490`, `FUN_004a96b0`), its sounds `0x9a` and `0x9b`,
the effort gauge and the six sliders. `MessageBox_Open`'s second argument is the bottom place's button (`0x9873c8`; 1
`b_okay`, 2 `b_exit`, 0 hidden) and its third that button's act, and the same pairs follow for `0x9873c9` and
`0x9873ca`: here the cross alone, with no act, which the box's handler `FUN_0047eed0` answers by closing the box. The box
pauses the park (`g_ParkRunning`) and holds the key tables off (`FUN_00486b70`) until it closes.

**Built** (`ParkResearchOpener`, `MessageBox` with no tick): both boxes, at all three callers; the bar's type `0x13`
answers 2, the tag taken off. The screen is counted, `RESEARCH_SCREEN`. In the running game (`q410/run-fix2`): R over the
stock park logged `Park: research opener: Instant Action, so the box of UITEXT 0x1d4`, the box up with its cross alone
(`1-r.png`) and the park's clock at tick 769 for 3 s under it; the gadget's button the same; B then R closed the buy
screen and opened the box. A fresh Full Simulation park (`run-fs1`) gave the `0x1d3` box, and with a scientist hired
`RESEARCH_SCREEN` counted and no box. The original given the same park (`q410/orig/a-sheet.png`) showed the same box by
R, by the button and over the buy screen, its cross at (714, 326) of 1024 x 768, the bottom place.
In no game run: the bar's type `0x13` (its rows 76 and 80 score only in Full Simulation; the test holds it), a carried
hire that is a researcher, the other three themes.

### The Staff Training Budgets screen (Q352)

Screen 9 of the Money category, `staffcosts`. Read 2026-10-13: the builder `FUN_004b2750` (decompiled, its calls
read as disassembly), the handler at `0x004b24b0` (loose code, read by `disassemble`; nothing written or named),
the stream at `0x00756400` (walked to its balanced close; `lib/uistream.py`), its fourteen mesh hashes resolved
(`meshhash.py`), and `FUN_004c9c00`.

**The opener.** With its screen up (`DAT_007cc2ec`) it does nothing. Otherwise it destroys the park screen that is
open (`FUN_00485b40`), leaves the category on it (`FUN_004a0810( 3, 9 )`) and loads the tree. Its callers are the
category opener (`FUN_004a0940`), the entry-price, finance and loans screens' handlers (`0x00498d5f`, `0x0049a1a4`,
`0x0049f82e`), the message bar's button of type `0x15` (`FUN_0048e0f0`, `0x0048e206`) and the finance sub-panel's
`0x1b5b` (`FUN_00497a70`). No key was found for it.

**The stream.** Rects on the 2048 x 1536 layout.

| Id | Type | Rect | Mesh | Help | What it is |
|---|---|---|---|---|---|
| `0x2d9` | 1 | (208, 30)-(1840, 1007) | `w_park` (the node `window3`, `0xf76e4349`) | | the frame |
| `0x2da` | 1 | (805, 74)-(1277, 153) | | | the title, UITEXT 147 through `UI_SetTitle` (`0x004b3293`) |
| -1 | 2 | (1711, 820)-(1794, 903) | `b_okay` | 1 | close |
| `0x2db` | 2 | (1688, 575)-(1790, 677) | `b_entry` | 176 | to the entry price (`FUN_00498d80`) |
| `0x2dc` | 2 | (1688, 467)-(1790, 570) | `b_finance` | 175 | to the finances (`FUN_0049ac60`) |
| `0x2dd` | 2 | (1688, 360)-(1790, 462) | `b_loans` | 174 | to the loans (`FUN_0049fb30`); **disabled in game type 2**, Instant Action (vtable `+0x14` with 0, `0x004b32eb`) |
| `0x2de` | 1 | (267, 183)-(765, 411) | `!frame` | 177 | the month's account |
| `0x2e3`, `0x2e6`, `0x2e1`, `0x2e5` | 1, flags 3 | x 284-561; y 195, 246, 297, 349 | | | labels, UITEXT 357, 358, 359, 361: Cash in, - Staff costs, - Other costs, Balance |
| `0x2e2`, `0x2df`, `0x2e0`, `0x2e4` | 1, flags 3 | x 591-755; the same four rows | | | their figures, right-aligned (`FUN_0065c428( 2, 1 )`) |
| `0x2e7`, `0x2ec`, `0x2ea`, `0x2eb`, `0x2ed` | 12 | (770, y)-(1670, y + 149); y 183, 337, 491, 645, 799 | `f_train` | 164, 165, 166, 167, 168 | a spinner a kind: handymen, mechanics, entertainers, guards, researchers, top to bottom |

The stream lists the five spinners in the order `0x2e7`, `0x2ea`, `0x2eb`, `0x2ec`, `0x2ed`, not their order down
the screen. Each holds, 154 down a row (a unit more or less here and there): the text rect (996, 233)-(1273, 278);
op `0xf`, `b_minus`, (925, 224)-(985, 285); op `0xe`, `b_plus`, (1286, 224)-(1346, 285); `0x2e8`, type 9, the
skill bar, (1376, 235)-(1592, 274), help 169 to 173 down the screen; and `0x2e9`, the kind's picture, (800, 203)-(902, 306):
`h_handy`, `h_mech`, `h_enter`, `h_guard`, `h_resrcher`. The two cost rows are red (`FUN_0065c5d5( 0xff, 0, 0,
0xff )`), every figure font 6, a spinner's text black and centred.

**The builder.** Each spinner is given the range 0 to 10000 (`FUN_0066a5c4`), the step 25 (`FUN_0066a68d`), the
number skin that letters "$ 75" (`FUN_00490250`) and its kind's budget, thing 1's `mBudget` (`FUN_00519450`, `+0xc`
for `0x2e7`, `+0x10` `0x2ec`, `+0x14` `0x2ea`, `+0x18` `0x2eb`, `+0x1c` `0x2ed`). Each bar is given the skin
`ridestatbar.wct` and `( FUN_004c9c00( model ) << 10 ) / 100` of 1024, the models 5, 4, 6, 7, 8 in the rows' order:
`FUN_004c9c00` adds the grades (`+0x1e4`) of every thing of the model, times 100, over five a member, nought with
no member. The bars are set here and not again. The account's four figures are the hire screen's ("The mini-balance
is a MONTHLY ACCOUNT"): the analyser's newest month of cash in, of staff costs, of all costs less those, and cash in
less all costs, the three sums kept in `DAT_007cc2d4`, `DAT_007cc2e0` and `DAT_007cc2dc`, with `DAT_007cc2e4` the
other costs.

**The handler.**

- `0x800`, a spinner's value, ids `0x2e7` to `0x2ed`: the kind's budget is the value, **nought for a value below
  nought** (`SETL`/`DEC`/`AND`, `0x004b2544`), written at once; **a value over nought sets thing 1's `+0x20`,
  `mHaveEverTrained`** (`0x004b25d0`), which nothing here clears. Then, and for ids `0x2e8` and `0x2e9` too, the
  refresh: figure `0x2df` is **the five budgets added up** (`0x004b266a`), and `0x2e4` is the kept cash in less
  that sum less the kept other costs (`0x004b2695`).
- `0x100`, a click: -1 closes; `0x2db`, `0x2dc`, `0x2dd` open the entry price, the finances, the loans.
- `5` closes; `0x14` forgets the screen (`DAT_007cc2ec`).

**Opening it is a change of every budget (measured, not read).** A spinner given its first value tells the
handler as a press does. So the Staff costs row never shows the analyser's month: it reads the five budgets added
up from the opening, and a file whose budgets are over nought with `mHaveEverTrained` unset has it set by the
opening alone. The original, off-screen (`q352/orig/run2.sh`): given `q352/set75.TPWS` (the mechanics' 75) it read
"$ 75" and "-$ 11750" on 27 February, weeks before the month's change (run c); given `q351/bud2.TPWS` (25 and 50
written by hand, the flag nought) thing 1's `+0x20` went 0 to 1 on tick 1095, the tick the screen came up, the row
"$ 75" (run d). What reads `mHaveEverTrained` was not looked for.

**OpenTPW** (`ParkStaffCostsScreen`; `ParkCategoryScreens.StaffCosts`; `ParkPeople.SetTrainingBudget`,
`HaveEverTrained`, `TrainingOf`; `ParkGadget`'s bar button of type `0x15`; the console's `budgets`). The stream's
frame, title (lettered as `UI_SetTitle` letters one), buttons, account panel and five rows; a press of a spinner's
button writes the budget as the handler does; the opening writes each back; the bars are `ParkObjectWindow.UiStatBar`.
The month's change spends what is set (`ride-operation.md`, "The month's change"), the advisor's rows 28 to 32 read
it (`advisor-park.md`, "Staff nobody has trained"), and a saved park holds it (`saves.md`, "OpenTPW's writer, the
training budgets").

**Deviations, each said at its site.** The account is built (Q385, "The mini-balance is a MONTHLY ACCOUNT"). The bars wear
`ridestatbar.wct` (Q353, "The stat bars' skin", below). The lettering in the panel and the spinners is smaller than the original's at this window
size (not looked into). The finances and the loans are not built, so those two buttons close the screen and count
(`FINANCE_SCREEN`; `LOANS_SCREEN` in Full Simulation).

**Predicted first, both games** (`q352/PREDICTION.txt`, three rounds; `q352/confirm.py <out> <bin>
fix|money|bud2|month|control`, `q352/orig/run.sh` and `run2.sh`). Two predictions missed, both mine: the month's
change at the original's tick 1383 in this game, whose calendar counts from the park's making (Q241j), and the
Staff costs row empty at the opening, which this game matched and the original did not: found by the original's
run, then built as above and run again.

| | The original | OpenTPW |
|---|---|---|
| The training tag's bar button, `q351/all0.TPWS` | the category left on 9, the screen up, five "$ 0", the tag off (Q351's a2, run a) | `button type 21 opens the Staff Training Budgets screen and takes tag 30 off`, `left on 1,4,9`, `opened, budgets $ 0,$ 0,$ 0,$ 0,$ 0 skill 0,0,0,0,0 staffcosts $ 0 loans off` (`run-fix2`) |
| The mechanics' plus, three times | thing 1's `+0x10` 25, 50, 75, `+0x20` 0 to 1 at the first; "$ 75", Staff costs "$ 75", Balance "-$ 11750" (`orig/a-c3-mech-plus3.png`) | `budgets 0,25,0,0,0 evertrained 1`, `0,50,0,0,0`, `0,75,0,0,0`; `staffcosts $ 75` (`run-fix2/3-budgets.png`) |
| The handymen's plus, then minus twice | `+0xc` 25, then 0; `+0x20` still 1 | 25, 0, 0; `evertrained 1` |
| The stock staff's bars | four at 60 per cent, the researchers' at 40 (Q198's `o12.png`) | `skill 60,60,60,60,40` (`run-money2/1-screen.png`) |
| The loans button in Instant Action | grey | disabled: the click does nothing |
| The gadget's Money button, then the entry price's staff-costs button | the screen (runs c, d) | the same; the Money button afterwards opens it (`category 3 is left on screen 9`) |
| Budgets 25, 0, 50 and the flag unset, the screen opened | `+0x20` 0 to 1 at the opening, Staff costs "$ 75" (run d) | `evertrained 0` to `1`, `staffcosts $ 75` (`run-bud2`) |
| The month's change with the mechanics' 75 | thing 26's `+0x1e8` 0 to 15 on tick 1383 (run b; 0 to 10 with 50 set, run a) | `through 0` to `through 15`, a withdrawal of 75 then the wages 27, 69, 36, 45, 75, on this game's own month, tick 1879 or so (`run-month2`, the final build; `run-month1` the build before the opening's write-back, the same); photographed either side in `run-month3/before-after.png`: 1/31/2000 at $86657, 2/1/2000 at $86360, each the `money` line's balance |
| Main's build, the same tag and button | | `button type 21 is not built`, `TAG_BAR_BUTTON` (`run-control`) |

`side-by-side.png` holds both screens with the mechanics at 75. Fifty bugs put back, fifty caught at the first round
(`q352/mutations.py`); the one line no test reaches is `Level`'s hand-over of the budgets to the writer, which
`run-fix2`'s file holds. Not run: Full Simulation (the loans button enabled there; the test holds only
Instant Action's), a budget at 10000 in either game (the test holds the end), the screen in the three other themes.

### The stat bars' skin, `ridestatbar.wct` (Q353)

Every type-9 bar of the park's screens is given the same skin, `FUN_00477ff0( texture )`: 38 of its 46 calls hand it
`"ridestatbar.wct"` (`0x00750e00`: the object window's four, the training screen's five, the hire screen's one and
the others' bars) and 8 `"happygrad.wct"` (`0x007508c0`: the all-staff screen's three, "The all-staff rows", below,
and one each in `FUN_004939d0`, `FUN_004afc10`, `FUN_004b1040`, `FUN_004b53c0` and `FUN_004b6c20`). It is an
object of vtable `0x006feeb8` holding two quads in a 100 by 100 space, four vertices a block of `0x30` bytes (x, y
and z four apiece, `FUN_00477c00`). The control hands it its value through the skin's `+0xc` with flag 8
(`FUN_0066d885`, the value held to 0 to `0x400`); there (`0x00478593`) `v = value x 0.0009765625 x 100`, held to
0 to 100, and:

| Quad | x | u | v band |
|---|---|---|---|
| filled | 0 to v | 0 to v / 100 | 0.5 to 1 |
| the rest | v to 100 | v / 100 to 1 | 0 to 0.5 |

So each part is CROPPED, never squashed: a short bar shows only the ramp's left end, and only a full one its green
and its dark right edge; the empty part is drawn too, a bar at nought all track. The texture (`ui.wad`,
`textures\ridestatbar.wct`, 32 by 32; `stexture\` holds a copy) is a colour ramp in its first sixteen rows and a
grey track (56, `383838`) in the rest. The original's bar covers the
control's whole rectangle (rows 118 to 137 where the layout gives 117.5 to 137 at 1024 by 768), and the colour half
bleeds into the top rows of the track.

**OpenTPW** (`ParkObjectWindow.UiStatBar`, `Parts`; `Units` for the original's 0 to 1024; `Skin` the texture handed,
`ridestatbar.wct` unless the caller says otherwise; `ParkHireScreen.SkillUnits`).
The two parts over the whole rectangle as the table; the training screen hands its bars `(share << 10) / 100`, the
hire screen `((grade & 0xff) << 10) / 5` as the row under the pointer is picked (its list's flag `0x80`, stream
flags `0x291`, as the buy screen's); the object window's two built bars their percentage.

**Deviations, each said at its site.** The ramp runs red, orange, yellow, green as the original's (Q354, the header's scales; Q355, the original's zlib codec: `textures.md`), within 16 a channel along the 80% bar and red to its left end ((254, 4, 4), the original's (221, 42, 0); `q355/side-by-side.png`). The original's track is 49 grey where this game draws the texture's 56. The
hire list's first row is not picked as it fills (above), so the bar stands at nought until the pointer passes a row,
and keeps that pick through a change of tab where the original's is the new tab's first row.

**Predicted first, both games** (`q353/PREDICTION.txt`; `q353/confirm.py <out> <bin> fix|control`,
`q353/orig/run.sh`, `q353/measure.py <png> <rect>...` for a bar's coloured share). The file `q353/grades.TPWS` is
`q351/all0.TPWS` with the five grades 4, 1, 0, 3, 2 written (staff record `+398`). Round 1 missed three of four, all
mine: the halves swapped (this game's quad puts texture y 0 at the bottom of its rectangle) and a click on a hire row
that chose the candidate and closed the screen (the list did not pick under the pointer).

| | The original (`orig/a-*.png`) | OpenTPW (`run-fix4`, the final build; `run-fix2` and `run-fix3` the same) |
|---|---|---|
| The training screen's bars, grades 4, 1, 0, 3, 2 | coloured 0.811, 0.189, 0, 0.604, 0.406 of each bar, the rest grey | `skill 80,20,0,60,40`; 0.808, 0.202, 0, 0.606, 0.404 (`1-train.png`); `UI: the stat bars' skin ui/textures/ridestatbar.wct is 32x32` |
| The hire screen opened | the first row picked, the bar 0.617 | no row picked, the bar all grey (`2-hire.png`), `STAFF_SKILL_BAR` gone |
| The pointer over the first row | the bar unchanged | `Hire screen: candidate 11 grade 3, skill bar 614 of 1024`, 0.606 (`3-picked.png`) |
| The mechanics' tab | the bar 0.409, the first mechanic's | 0.606 kept (`4-tab.png`) |

The original's list held five handymen (Andy Buchanan, Chris Battson, Jan Svarovsky, Jon Taylor, Paul Carpenter)
where this game's, on the same file, held two (Chris Battson, Rajan Tande): not looked into. Not run: the object
window's bars in the original (no route there was driven), a full bar, the other three themes.

### How allpeeps keeps itself current: rows updated in place, never cleared (Q200)

Decoded 2026-09-30 (`wf_95649c5e-08a`: one Opus skeptic, three claims upheld and two amended, none refuted; the
amendments re-read first-hand). **The original never empties this list after it opens, so its scroll stays where the
player put it.** Three paths change it:

- **Opening.** `FUN_00493530` sets the sort from `DAT_007508bc` while the list is still empty (`FUN_006659af(v, 0)`),
  adds one row per guest (`FUN_00481bc0( 0x40, 0x004937e0 )`, thing type byte `+2` == 1, each through the row adder
  `FUN_00493800` → `FUN_0066403b` with no redraw), draws once (`FUN_00663324`, `0x00493632`), and, when handed a guest,
  selects that guest's row and scrolls to it (`FUN_00665739`, the one direct writer of the top row besides the
  constructor and the clear). Then it arms timer `0x80083` at 2000 ms (`FUN_0065ef90`, `FUN_0065f024`).
- **The timer, every two seconds** (message `0x10` with id `0x80083`, `0x00493270`). For each existing row, from the
  last to row 0, it fetches the row's guest (`FUN_00664c71`), works out the same six values the row adder does, but
  for one: **the time in park from the stay shifted back left by two** (`SHL EAX, 2` at `0x004932db`), four times the
  adder's hours ("The visitors rows", below), and writes them into that row (`FUN_006644ea( row, values, 0x3f, row == 0, -1 )`). Row 0's call redraws the visible
  rows from the top row `list+0x154`. **Nothing is added, removed, re-sorted or scrolled**: a row keeps its place from
  its last sort or insert even when the value it was sorted on has changed.
- **A guest arriving or going.** The guest constructor `FUN_004faec0` broadcasts message `0x1c` (`0x004fb2fd`) and the
  thing delete `FUN_0050b780` broadcasts `0x1b` for every thing (`0x0050b7f9`, before the free); `FUN_0050ca50` routes
  them. `0x1c` adds that guest's row (`FUN_00493c50`: the row adder, which inserts in sort order when the list's
  `+0x48 & 0x10` is set, then a redraw). `0x1b`, for type 1 only (`FUN_004818c0`), finds the row by its guest
  (`FUN_00664d2a`) and removes it (`FUN_00493c10` → `FUN_006647cb`). Neither writes the top row itself. Both finish in
  `FUN_00664495`, which sets the scrollbar's range to count − visible and puts its position back at the top row; the
  slider clamps the position, and if that moved it the list takes it back (`0x800` → `FUN_00664ea3`, `0x00664ee9`).
  So a removal near the bottom pulls the view up by the rows it lost, and nothing else moves it.

Three edges a copy must match, found by the skeptic:

- **With count ≤ visible rows the slider is disabled and never repositioned** (`0x006644d2`), so a view scrolled one
  row down that loses a guest down to thirteen stays one row down, row 0 unseen and the last slot blank, until the
  wheel clamps it (`0x1000d`).
- **Removing the selected row selects by visible slot, not by index**: `FUN_006647cb` passes the absolute index (the
  same, or one less if it was the last row) to `FUN_0066525c`, which adds the top row to it. At top 0 that is the
  next row; scrolled down, it lands further down the list.
- **The selection is not shifted** by an insert or removal above it, so the highlight moves onto a different guest.

The sort is remembered between openings: message `0x406` stores (column + 1), negated when descending, in
`DAT_007508bc` (`0x00493479`); its static default is 1, Visitor Number ascending.

Read first-hand for the build (Q200b, 2026-09-30):

- **The sorted insert goes after every row that is not greater.** `FUN_0066403b` walks from the head until
  `FUN_00663edc( new, row )` answers 1, which for a numeric ascending column is `new < row`, strictly. So equal
  values keep the order they came in, and an arrival not yet numbered (visitor 0) goes after the other noughts and
  before every numbered visitor.
- **An add that makes the count 1 selects the first row** (`FUN_0066525c( 0 )` in `FUN_0066403b`). The original's
  visitors list opens with a row highlighted, not always the first ("A right click on a list"). OpenTPW builds the
  pick for the hire, all-items and all-staff lists ("The first row is picked as a list fills") and counts it for the others
  (`LIST_FIRST_ROW_SELECTED`, `docs/QUEUE.md` Q232).
- **`FUN_0066525c( slot )`** selects `slot + top` when `slot < visible` and `slot < count`; otherwise, with
  `list+0x48 & 0x100` clear, it returns and the old index stands, and with it set it selects nothing. This list's
  flags are `0x291`, the bit clear ("A right click on a list"); OpenTPW still counts that branch
  (`LIST_RESELECT_PAST_THE_WINDOW`) and drops the selection (`docs/QUEUE.md` Q232).

**OpenTPW keeps it current as the original does** (Q200b): `ParkVisitorsScreen` fills the list once, rewrites each row
in place on the two-second timer, and adds and removes a row on `ParkPeople`'s `GuestArrived` and `GuestLeaving`;
`UiList` keeps its selection as an index and clamps the top row only as the slider does. The sort is always Visitor
Number ascending, the static default, because the headings do not sort yet. Confirmed in the game (`q200brun.py`,
`q200b-run1/`, silent, stock park, twenty admitted, 33 rows; `save/` unchanged), each predicted first: four rows down
the first row was cash 450 and still 450 three seconds on, with row values changed in place and not re-sorted; an
arrival's row went in at 1 (visitor 0, after the one nought already there) with the top row kept at 4, its going took
it out at 1; at the bottom (top row 20), the last row's going left 32 rows and the top row 19.

### A caution: `0x10`/`0x11`/`0x12` are column headers, not buttons

They are children of the buy list control `0x1f8`, numbered `0x10 + column`, and the UI library sorts
on them — labels UITEXT `0x7b` *Name*, `0x7c` *Price*, `0x8a` *"."*, help rows `0x90`/`0x91`/`0x92`
(*"sort the list by item name"* / *"by price"* / *"by items already owned or recently researched"*).
The buy stream itself contains ids `0x1e9`..`0x200` and **no** `0x10`/`0x11`/`0x12`, so they cannot be
top-level controls. The list widget sorts internally and the screen only remembers which column was
chosen. **Do not read them as gadget buttons** — the gadget's own six are `0x26`..`0x2b`.

### `FUN_00486b00` is a different id space from UIHELPTEXT

It goes to the advisor/message system (`FUN_0059b590` → `FUN_0059bf20`), not to the help table.
Reading its ids as UIHELPTEXT rows yields text that is plausible and wrong — the pylon button's
`0x125` decodes there as *"Left-click to view item's details"*. As an advisor message it
resolves through the metadata table at `0x0076e300`, filled by the static initializer at `0x005a0a50`: response 553,
sample 565, whose transcript is "looking this button let's you place normal pylons" (`advisor-park.md`, "A screen's line is a message id, resolved
through the metadata table").

### The arm is the gadget's panel carrier

`FUN_004a2590` puts a panel on the arm, `FUN_004a25f0` takes it off, and the message bar is only one of
the things it can carry. **Five callers** hand it a payload; the camcorder panel is `FUN_00498bb0`.
`FUN_004a25f0`'s argument lifts control **0x27** afterwards. Both are gated on `DAT_007cb2b8` (the gadget
is built, written by `FUN_004a1d70`) and `DAT_007cb2ac` (the interface is up, written by `FUN_00489f50`) —
**no simulation gate stands in front of the arm.**

## The camcorder viewfinder — stream `0x0074fa98`

Not a "park HUD root". Two top-level controls: the `viewfinder` panel (170,170)-(1878,1366) — which is
`f_viewfinder.MD2` — and a `b_eject` button (1916,1404)-(2018,1506), help 468 *"Left-click to exit
camcorder mode (C)"*.

**Confirmed against a screenshot of the original.** At 800x600 the virtual screen scales by 0.390625 with
no letterboxing, so `b_eject` lands at (748,548)-(788,588) — exactly the eject icon in the shot — and the
viewfinder's corner brackets sit on its rect. **The management gadget is NOT on screen in camcorder
mode**, which is why hiding it there is faithful and not a workaround.

### Anchors differ within one screen

Anchoring takes a rect's **centre** against the thirds: left third pins left, right third pins right,
middle centres — and the same down the screen. The gadget is bottom-**left**; the eject button's centre x
is 1967, past 2048*2/3, so it is bottom-**RIGHT**. Cropping it with the gadget's anchor missed it by the
whole 320px of horizontal slack at 1280x720 and returned a picture of grass. **Never assume one anchor for
a whole screen.**

### Four ways out of camcorder mode: Escape, the C key, the eject button, and a right click

`FUN_00488a00`'s key-up case treats `key == 0x1b` (VK_ESCAPE) and `action == 0x10` (camcorder, shortcut
16) the same way: it sets layer 1's cursor to 1 (`c_busy.ani`, `0x00489750`), leaves first person
(`FUN_0042ae70` or `FUN_0042a190`), and sets the cursor back to `0x15`. **`FUN_0065f11d` is a cursor setter, not a
page switch**: it stores the id at the control's `+0x50` and applies it at once if the pointer is over the
control (`FUN_006590f7`).

The same calls, from a copy at `0x00488ab7`..`0x00488adb` that joins Escape's code at `0x00488b5a` (at `0x00488b55` in a
ride view), are reached two more ways (decoded for `docs/QUEUE.md` Q59: three decoders, each put to a refuter, then a
critic; static only):

- **The eject button's left click**, message `0x100`, with no RMB cancel test and no button test (`0x00488a37`): the
  button class posts `0x100` with its id to its parent (`+0x60`) at the end of a left click (`0x006691d6`), and
  `b_eject`'s parent is layer 1.
- **A right click on the layer, with RMB cancel on**: the `0x10006` case (`0x00488aa1`..`0x00488adb`) reads
  `[0x0078d911]`, then the button (`1`: the user's secondary button, since the DirectInput library swaps the two when
  Windows has them swapped, `0x006342b9`), then `gui_CameraFlags & 0x16`. `0x10006` is the UI library's
  **single** click, not a double click - see "A click and a double click" below. A double click leaves on its first
  click, and its second press (`0x10007`) falls to the handler's tail.

**Where a right click leaves.** The viewfinder panel `0x89db52` is built with flags **3**, visible and disabled
(stream `0x0074fa98`: op 0, type 1, flags 3), and the hit test `FUN_0065db25` skips a control flagged `0x2` with all it
holds, so a press on the frame is layer 1's. `b_eject` (flags 1) takes a press on its own rect, and the button class
hands a right press or release (`param_3 != 0`) to the base proc on itself (`0x00669003`, `0x006690d6`), whose click
goes nowhere. So in 2048x1536 units a right click leaves anywhere on `0 <= x < 2047, 0 <= y < 1535` but
`1916 <= x < 2018, 1404 <= y < 1506`. With RMB cancel off, nowhere. Nothing passes a message to a parent: the queue
pump hands each entry to its own control's `+0x114` (`0x00669a84`, `FUN_0065f697`). The management screens are built
onto layer 0, which first person hides, and the help bar is disabled, so nothing else is under the pointer.

**What else a right press does in first person.** `FUN_00488a00` hands every message, before the base proc, to
`FUN_0042a760`, which sets `DAT_00790aac |= 4` on a right press and clears it on the release (`0x0042a8bd`,
`0x0042a8fb`). The park's own layer and the full-screen view's cover hand theirs to the same function (`0x00488290`,
`0x0048845c`, `0x004a287d`), so the bit is the right button held from a press on any of the three (with RMB cancel on, the park's own layer keeps the press back, `0x00488354`, and sends it once the pointer has moved more than 8 or 200 ms have passed), and a press the
eject button took never sets it. The camera update keeps bits 1, 2, 4 and `0x80` of the word each frame and rebuilds
the rest from the keys (`AND AL,0x87`, `0x0042b32b`). Walking (`gui_CameraFlags & 0x16` set, `& 0x3c` clear) it
builds the forward term from the keys, -0.1 for bit `0x40` then +0.1 for bit `0x20` (`0x0042b448`..`0x0042b468`), and
then, while bit 4 is set, adds another 0.1 (`EDI = word & 4` at `0x0042b716`; `0x0042b925`..`0x0042b93f`,
`[0x006fddac] = -0.1` subtracted), whatever RMB cancel is: **a held right button walks forward, as far as the Up arrow
does, and the two add**. The press and release also reach the camera table as key `0xfff1` (`button - 0x10`,
`0x00488a74`), which no row binds. The walk of a quick click is thrown away with the rest when leaving puts the saved
point back.

Measured in the original (Lost Kingdom, first person at (479.4,166.5), yaw 0, zoom word `0x007909ec` 110, RMB cancel
on, the camera read from memory): the right button held 1.84 s on the view, the word `0x84` while held and `0x80`
after, the position 40.4 along the facing, still in first person; the Up arrow held 1.87 s, the word `0xa0`, 40.4;
both for 0.81 s, `0xa4`, 34.1, twice the rate; Down with the button, `0xc4`, standing to a frame; held 2 s on the eject
button, the word `0x80` throughout and the position unmoved; a quick right click, out of first person. So one forward
term walks about 21.8 units a second there. The velocity that term feeds is not decoded to a speed (`0x0042bc2a` on:
the square root of the zoom word times `[0x006fddf8]` 0.02, halved with the last frame's).

**In a ride view** (`gui_CameraFlags` 4, "Ride it!", `FUN_004e15b0` → `FUN_0042a560`) the same exits run `FUN_0042a190`,
which goes back to walking first person when the ride was entered from it (`0x400`, `0x0042ac0b`) and to the orbit
otherwise. Flag `0x10` is tested and never set (dead by CODE). The two views of a park's end that `FUN_0048ac40` and `FUN_0048ae70` build
(world `+0x1da738 == 4`, where `FUN_004a2ac0` switches no layer) take the click in their own handlers, `0x0048a740` and
`0x0048a970`, and only 2000 ms after they open; the first then leaves the park in the full-screen view
(`park-engine.md`, "The full-screen view: F3").

**OpenTPW.** `WindowStack.RightClick` is the base proc's click for the right button; a press on the view asks
`ViewRightClick` what answers its click, and the park answers `ParkViewfinder.RightClicked` in first person, which leaves
it with RMB cancel on. The limit is timed on `Time.WallMilliseconds`, real time as the original's, and the way into first
person closes the open park screen, as the original's does (`FUN_00485b40` at `0x00481a2b`), so none is there to take the press. The eject button is `ParkViewfinder`'s, `Clicked = ParkCamcorderCameraMode.Leave`. A held right button walks:
`ParkCamcorderCameraMode.RightHeld` is the bit, read once a frame after the interface has had the press, and `Walk`
adds the forward key's amount for it. The walk's speed is a chosen 40 units a second for one forward term, where the
original's measures about 21.8, and its keys are W and S, where the original's are the zoom's, Up and Down
(`docs/QUEUE.md` Q25). There is no ride view: walking onto a ride's
entrance, which rides it in the original, is counted instead (`FIRST_PERSON_WALK_INTO_RIDE`, `park-engine.md`).

### A click and a double click: the UI library's `0x10006` and `0x10007`

The base control proc `FUN_0065f6d1` makes both, from the press (`0x10005`, case `0x0065f820`), the move (`0x10003`,
`0x0065fa33`) and the release (`0x10004`, `0x0065f913`). It keeps **one record per button, not per control**, at
`0x00faa598 + b * 0xc` (b = 0 left, 1 right, 2 middle): a state word (0 idle, 1 pressed and still a click, 2 spoiled),
the press point (`+2`, `+4`) and one time stamp (`+8`); and a mask at the control's `+0x11c` of the buttons pressed on
it. The clock is milliseconds of real time: `FUN_0065968e` calls slot 0 of the object at `[0x0077c490]`, which
`UI_Init` sets to the game's (`FUN_00659674` at `0x00489d9e`: `0x007b9864`, vtable `0x006ff178`, slot 0 `0x00489c80` →
`FUN_005f5fa0` → `FUN_005f5f10` on the timer at `0x00fa71e0`: QPC less its start, over its ticks a millisecond, plus
its start's milliseconds, with `timeGetTime` as the fallback). Nothing clamps or holds it. Measured in the original,
the game's process stopped 600 ms between a right press on the first-person view and its release, the press given 120
ms to be handled first: no click, twice, and a quick click after it left; the park's quick click the same way, stopped
400 ms, no click, three times. (A press sent 20 to 40 ms before the stop is handled after it, at 30 frames a second,
and then clicks: that measures the stop, not the clock.) OpenTPW's is `Time.WallMilliseconds`. A control's timer
runs on the same clock (`FUN_0065ef90` arms one, `FUN_00661fe5` fires it on `FUN_0065968e` differences): nothing
until a whole period has gone since its stamp, then message `0x10` once for each whole period gone, and the stamp
left the remainder behind now, so a late pass does not move its phase. `FUN_006622d2` runs every timer, and none
while `[0x00faa638]` is set: `FUN_00662411` sets it, called as a message box, the game menu and the options screen
open, and by three more (Q238): the park's map as it opens (`FUN_005f0b40`, `0x005f0b8a`, let go by its closer
`FUN_005f1130` at `0x005f121b`), and the pause overlay's two doors. `FUN_0048a6e0` (`0x0048a6e9`) holds and puts the
overlay up (`FUN_004a2790`, tree `0x00752e00`); its one caller is the pause `FUN_004092a0` when its first argument is
1, which only the window losing the focus passes (`WM_ACTIVATEAPP`, `0x0046b74c`); `FUN_0048a700` lets go and takes
it down on every resume (`FUN_00409300`, `0x00409315`). `FUN_0048a720` (`0x0048a72e`, `0x0048a733`) toggles the
overlay and holds or lets go with it, from `Game_TogglePause` (`0x0040935e`), which the overlay's own proc calls on a
key let go (`0x004a2706`). What the overlay draws and which key reaches the toggle are not read. `FUN_00662420` clears
the hold and stamps every timer afresh (`FUN_0066213e`). OpenTPW: `UiTimer`, held by `WindowStack.HoldsTimers` for the
message box, the game menu, the options screen and the map: the visitors list's 2000 ms, an object window's 4000 ms
and the buy screen's 1000 ms. The pause overlay is not built; the focus leaving is counted
(`FOCUS_LOSS_PAUSE_OVERLAY`).

**The buy screen's own timer** (Q238) is `0x80080` at 1000 ms, armed on the screen's message `0x15`
(`FUN_004ac270`, `0x004ac3ee`). Its tick, message `0x10` (`0x004ac2db`), draws the list's rows again
(`FUN_00663324` on control `0x1f8`) and letters the corner, control `0x200`, with the bank's balance: UITEXT `0x1ca`
("Cash  $ ") and the balance, or `0x1cb` ("Cash  -$ ") and the balance without its sign when it is under nought
(`0x004ac31e`). No other arm of the handler writes `0x200`, and the tab builder `FUN_004ab6b0` does not. The opener
`FUN_004acc70` letters it once itself, the same way (`0x004acdae`): measured in the original, the corner reads
"Cash  $ 88112" in the first frame its screen is drawn (`q238/orig-buy-sheet.png`). The timer is made stopped
(`FUN_00661ece`) and started and stamped by `FUN_0065f024` (`0x004ac403`), so its first tick is a second on. That
the number follows the string rests on the frame; the formatter `FUN_006acd60` is not decoded. OpenTPW:
`ParkBuyScreen.Tick` on a `UiTimer`, and the same lettering once as the screen is made. Not read: the two park's-end views' 2000 ms. The limit
`[0x0077c480] = 500`, and the points the interface's 2048x1536 units.

- **The press**, only from state 0: sets the mask bit, keeps the point, state 1. If the control's flags `+0x48` lack
  `0x8` and the press is less than 500 ms after the stamp, it posts **`0x10007`**, the double click, and sets state 2
  (`0x0065f8dd`). Then it stamps the press's time (`0x0065f908`).
- **A move** delivered to the control while its mask is set spoils (state 2) any button in state 1 whose press point is
  **more than 6** away across or down (`0x0065fab7`, `0x0065fadb`), for good. A move is posted to the capture if there is
  one, else to the control under the pointer, and a move onto another control is `0x10002`/`0x10001`, not a move: so
  without a capture only moves over the pressed control are judged.
- **The release**, delivered to the control that took the press wherever the pointer is: in state 1 with the mask bit, it
  posts **`0x10006`**, the click, with the press point, if less than 500 ms passed since the press (`0x0065f969`), and
  stamps the release's time either way (`0x0065f9af`); otherwise it clears the stamp (`0x0065f9bd`), to nought, which
  the next press compares signed: once the clock has passed `0x80000000`, about 25 days up, that press is a double
  click's second (read, not run; OpenTPW keeps no stamp instead). Mask bit and state
  are cleared.

So a click is one press and its release under 500 ms, never having strayed more than 6. A press within 500 ms of the
last clean release, **on any control**, since the stamp is the button's, is a double click's second, and its release
makes no click and clears the stamp, so a third press clicks again. Both messages are queued (`FUN_00658ccc` →
`FUN_006697e3`) to the control whose proc posted them and delivered in the same drain as the release. The window class
has no `CS_DBLCLKS` (style 0, `0x0044e0de`), and by default the buttons do not come from the window procedure at all:
the DirectInput cursor library is on (`FUN_0047e7d0` sets `DAT_007b4b88`, `0x0047e951`, unless `-bwcursor`), and its
callback `0x00486c70` posts the presses and releases in the same units. Its own double click bit (`0x0063446a`) never
reaches the UI, and `FUN_00658ab4`, which would post `0x10007` to the hover, has no caller (dead by CODE).

### Who acts on the click, and who on the release

Decoded for `docs/QUEUE.md` Q123b, first-hand, and run in the original.

**A button does not use the click.** The button class's proc `FUN_00668f9c` answers the left press and release
itself and never hands them to the base proc. The press, from state 0 only, stamps the button's record, sets state 1
and draws it down (`0x10005`). The pointer leaving the button sets state 2 and lifts it, coming back sets 1 and puts
it down again (`0x10002`, `0x10001`). The release posts **`0x100`** with the button's id to its parent when the state
is 1 and `FUN_00668eff` answers true, which is false only when a repeating button (flag `0x20`) has already repeated
(`+0x15a` 2; `FUN_00668dd9` on the tick `0x1e`, delays `+0x13c` then `+0x140`, posting `0x10009` then `0x10008`). No
clock is read at the release: **a button held any length of time is a click**, so long as the pointer is on it when
it comes up.

**Everything else gets the base proc's click**, `0x10006`: the release under 500 ms from the press, not strayed more
than 6. And for the release, the press, the click and the double click alike the base proc also calls
`FUN_0065dcaf`: if the message's own point is inside the control (slot 0 of the region object at `+0x2c`,
`0x0065dcc9`), it posts the same message with **`0x1000` set** to the control itself, carrying the record's press
point. For the click and the double click the two points are the same, so `0x11006` is "a click that began inside
me", and that is what turns `0x10006` into the message the list and the slider answer. (For a press the record still
holds the press before; nothing was found that reads `0x11004` or `0x11005`.)

**The stray is the base proc's, on the move message.** A class whose proc answers `0x10003` itself and returns
is never judged: the list's does (`0x00665d22`), so a press on a list is a click however far the pointer goes.

| Who | Message | What a click does |
|---|---|---|
| a list's rows, `FUN_00665c35` | `0x11006` | the left button: the row click `FUN_006655a2`; any other: `FUN_0066563d`, the `0x402` above; `0x11007`, the double click, has its own arm |
| the plain list class, `FUN_0065bb6b` (made by `FUN_0065ab64`) | `0x11006` | with flag `0x80`, posts `0x200` with the row; without, the row under the point (`FUN_0065b93f`, `0x201`) |
| a slider's track, `Slider_Callback` | `0x11006` | whatever the button: pages towards the point (`Slider_PageTowards`) |
| the lobby's player slots and Quit Game, `0x004a6000`, `0x004a61d0` (type 1 in the stream at `0x00753c68`, not buttons) | `0x10006` | whatever the button: the slot, or the quit question |
| an object window's title, `FUN_0048d2e0` (installed by `FUN_0048d290`, which eight windows call with their title's id; the ride window's is `0x3e28`) | `0x10006` | the left button: slot `+0x38` of the helper kept in the control's slot 0, the thing's rename box (`park-engine.md`, the name boxes). The move turns the title white (`FUN_0065c5d5`) and the leave puts it back (`FUN_00492d20`). OpenTPW counts the click, `RENAME_OBJECT` (Q28) |
| the entry price screen's heading `0x4f3af`, `FUN_00499060` (installed by `FUN_00498d80`) | `0x10006` | the left button: `FUN_004990f0`, the park's rename box; the same white on the move. OpenTPW counts the click, `RENAME_PARK` (Q28) |
| `FUN_004b3c10` (installed by `FUN_004b3cf0`, the staff and visitor locator) | `0x10006` | the left button: `FUN_004b79b0` or `FUN_004b6080` |
| the game menu's rows, `MenuList_ChoiceCallback` | `0x10006` | **whatever the button**: sound `0xc1`, then the choice (`0x100001`) |
| an object window's preview, `0x0048d1a0` | `0x10006` | whatever the button: the camera to the thing |
| `FUN_004b5c80` (installed by `FUN_004b6080`) | `0x10006` | whatever the button: `FUN_0048ce80`, the camera to a thing |
| the gadget's top, `FUN_004a1190` | `0x10006` | the right button only |
| the first-person layer, `FUN_00488a00` | `0x10006` | the right button, with RMB cancel |
| `FUN_004bed60` (two controls of `FUN_004bf460`) | `0x10006` | hides itself |
| `FUN_004c56e0` (five controls of `FUN_004c4fb0`) | `0x10006` | `FUN_005b7910` with its index; not identified |
| `0x005d58e0` (a control of `FrontEnd_Init`) | `0x10006` | `FUN_004bc710`, or message box `0x1d9`; not identified |
| the two park's-end views, `0x0048a740` | `0x10006` | after their 2000 ms |
| two lobby objects, `FUN_005dd530`, `FUN_005dd840` | `0x10006` | the middle button toggles a flag |
| a third of that kind, the handler at `0x005dced0` (in no function; the slot at `0x00702bdc`) | `0x10006` | the middle button toggles its flag `+0x58` (`FUN_004c57e0`, `FUN_004c5800`) |
| the message bar's ten entries (records of `0x208` bytes at `0x007c26b0`), a handler in no function, the test at `0x0048de67` | `0x10006` | the left button: sound `0xc0`, then the message is shown (`FUN_0048d9c0`), or, if it is the one on show (`[0x007ca090]`), the arm is retracted (`FUN_004a25f0( 0 )`); the right button: an entry other than `[0x007ca094]` is hidden (vtable `+0x14`), and the arm retracted if it was the one on show |

The table is every `CMP` or `SUB` against `0x10006` or `0x11006` in the image: every place the four bytes
`06 00 01 00` or `06 10 01 00` stand in code (a byte search, 14 and 3, the base proc's own post at `0x0065f996`
among them; it reaches the two handlers that sit in no function, `0x0048de67` and `0x005dced0`, which a walk by
function does not), the handlers that reach the message by
a chain of `SUB`s (`SUB 4` after `SUB 0x10002`, or after `SUB 0x10001` and `DEC` as in `FUN_0048d2e0`, `FUN_00499060` and `0x004a61d0`: the lobby's two and the three after them, found by a second scan), and
`FUN_00488a00`, which reaches its case through a jump table. A handler that switches on the message that last way is
not found by either scan, so the table can be short.

**In the original**, predicted first, 6 of 6, read from memory: the left button held 1.0 s on the gadget's Buy, the
buy screen opened as it came up (`[0x007c24c8]` 0 to `0x46bca50`); held 600 ms on a visitors row, nothing
(`0x46cc670` stayed), and a 200 ms click on it opened the visitor's window (`0x46cf4a0`); held 600 ms on the game
menu's Resume Game, the menu stayed (`[0x007c2534]` `0x46c1c30`); a quick right click on it chose it (0), and a quick
left click the same. In the lobby (Q123c, photographed): Quit Game held 600 ms asked nothing, and a quick right click
put the quit question up.

A visitor's window's preview, the same way (Q123c): held 600 ms, the window stayed (`0x46d3b20`) and the camera where
it was; a 150 ms click, the window closed and the camera went to the visitor, (475,175) to (490,280).

**OpenTPW** (Q123c): `WindowStack.LeftClick` keeps the left button's press and stamp as `RightClick` keeps the
right's, on `Time.WallMilliseconds`, and hands a click to the control the press landed on:
`UiControl.LeftClicked`, then `LeftClickedAt` with the press's point (`0x11006`); a double click's second press is
`LeftDoubleClicked` (`0x11007`). A `UiButton` takes no part: `Release` is its proc, with no limit, and its release
clears the stamp; a slider's thumb keeps its press too and leaves the stamp alone (`0x0066bb9b`).
`UiList.LeftClickedAt` selects the row under the point and activates the selected row (`FUN_006655a2` under flag
`0x80`; all seven list records carry it: buy `0x91`, the rest `0x291`, none `0x8` or `0x100`), and
`LeftDoubleClicked` activates it again (`FUN_006656fa`); `UiList.StraySpoilsAClick` is false, for either button.
The original's selected row is the one last hovered, so there a click that misses every row activates that one, and so
it does here, as every list follows the pointer (Q376). A `UiList` made without the flag would count its miss,
`LIST_CLICK_MISS_SELECTION_NOT_UNDER_POINTER`; no screen's is. A screen that answers no activation is counted,
`LIST_ROW_ACTIVATED_UNANSWERED`: the visitors, all-staff and all-items lists, the first of which opens a visitor's
window in the original. The game menu's rows, the lobby's player slots and Quit Game answer `LeftClicked` and
`RightClicked`, the preview `LeftClicked`, and a slider's track pages towards a click of either button
(`UiSlider.LeftClickedAt`, `RightClickedAt`), where it paged on the press. The console's `click` makes the click a
short press would. Not built: a list's column message `0x404`, which no screen answers; the middle button; a
repeating button; a control's flag `0x8`. A button that the pointer leaves and comes back to keeps its press here as
there, but the two are not compared.

## The other park streams

| Address | What it is | Contents |
|---|---|---|
| `0x00751530` | The **coaster builder** bar (859,916)-(1534,1190) | `cb_incline` 0x19, `cb_loft` 0x1a, `cb_move` 0x1b, `cb_rotate` 0x1c, `cb_swapdown` 0x1d, `cb_swapup` 0x1e, `cb_track` 0x1f, **0x20 with no mesh at all**, `cb_dellast` 0x21, framed by the nine-slice `!f_plain` |
| `0x007514c0` | Finance sub-panel (screen function `FUN_00497b20`) | `b_finance` 0x1b59, `b_loans` 0x1b5a, `b_staffcost` 0x1b5b |
| `0x00751720` | Camcorder / postcard | `b_1person` 0x63 help 479, `b_postcard` 0x64 help 480 |

`0x00751530` is the **coaster builder**, not a build toolbar: help rows 296-304 and 564-569 are *all*
pylons, track, loops and winches ("Left-click to move pylons", "...to build loop pylons", "Loop pylons
must always be placed in pairs").

## The message bar — stream `0x007501a0`

Built by `FUN_0048d8b0`. Walked in full: clean op 0, balanced op 5 at `0x007502ae`.

    0x10d2d  panel  (458,1101)-(1073,1442)   the bar itself
      0x10d2e  button (486,1325)-(589,1428)  mesh mb_erase
      0x10d2f  button (486,1218)-(589,1320)  NO mesh
      0x10d30  button (486,1110)-(589,1213)  NO mesh
      0x10d31  panel  (458,1101)-( 623,1442)  mesh messagel = f_tagl.md2
      0x10d32  panel  (623,1101)-( 835,1442)  mesh messagem = f_tagm.md2  + text rect (609,1118)-(1056,1422)
      0x10d33  panel  (835,1101)-(1073,1442)  mesh messager = f_tagr.md2

`FUN_0048d8b0` skins **0x10d32** only — label skin, **font 6** — so the middle slice carries the text. The
bar sits immediately right of the gadget, which ends at x 439.

### What feeds it

Every message arrives through `FUN_00481580`, whose **one** caller is `CTagSystem::ReceiveMessage`
(`FUN_00509bf0` - its own debug string names the class), and **every line the park advisor says feeds it**: a
said line sends an event of type `0x18` with its response's group, and the group's row in the tag table is a
tag on the gadget's aerial (`advisor-park.md`, "The tag a line leaves on the aerial", which has the table, the
list's rules and the mast's). The other two branches are an event of type `0x19` and the postcard's three ends
(ids `0x17b` / `0x17c` / `0x17d`, "Postcard sent", "Postcard to outbox", "Postcard failed"). `FUN_0048ef70` is
the **save-file loader**, not a live poster - it reads 0x21c-byte records through `FUN_005f5e20`. The cap is
**ten** (`DAT_00750610`, with the string `mb_erase` immediately after it at `0x00750614`). The tags are built
(`ParkTags`), and so is the bar a tag's left click opens (below).

**A screen's advisor line is not posted here.** `FUN_00486b00` runs into the advisor's own message system
(`FUN_0059b590`, `FUN_0059a940`); the bar keeps a separate list (`DAT_007cb2c8`, posted by
`FUN_0048eb20`). They talk in one direction only: showing a bar message (`FUN_0048d9c0`) posts advisor
message **0x11c** and calls `FUN_004a2590` to put the bar on the arm.

State globals: `DAT_007ca098` the bar's tree, `DAT_007ca090` the tag whose message is on show, `DAT_007ca094` the
tag being put on show, `DAT_007ca09c` how many tags are held.

### Showing a message, `FUN_0048d9c0`

A tag's left click (`FUN_0048dae0`, `0x10006` with button 0) plays sound `0xc0`, and then: the tag on show
(`[0x007ca090]`) folds the arm away (`FUN_004a25f0( 0 )`); any other is handed to `FUN_0048d9c0`:

1. Where the tag's byte `+0x134` is set (its timers run) and its second button is not `0x1b`, timers `0x80080`
   and `0x80081` are stopped and the byte cleared, and its particle killed. **A tag once opened does not time
   out.** A tag whose second button declines keeps its timers.
2. The bar's data words are written from the tag's: the text (the tag's word 2 to the bar's 1), the thing (3 to 2),
   the first button's type (5 to 3), the second's (6 to 4), the item (8 to 5).
3. `FUN_004a2590( bar, 1 )` asks the arm for the bar, `[0x007ca094]` is the tag, and advisor message `0x11c` is
   posted (`FUN_00486b00`).

### The bar's handler, `FUN_0048e2f0`

| Message | What it does |
|---|---|
| `0x1001d9`, the arm starts out with it | the middle panel's text is the bar's word 1; `FUN_0048e610` dresses `0x10d30` by word 3 and `0x10d2f` by word 4; `0x10d2e` wears `mb_erase` and is shown, unless word 4 is `0x1b`, when it has no mesh and is hidden; each button's depth goes up one; `[0x007ca090]` takes `[0x007ca094]`; and the stream at `0x007502b8` is played on that tag at 100 (`FUN_0065e72d`) |
| `0x1d` with 1, the arm fully out | `[0x007ca094]` is cleared |
| `0x1001d8`, the arm starts in | the tag's stream is stopped (`FUN_0065e897`), advisor message `0x11c` is withdrawn (`FUN_00486b40`), `[0x007ca090]` is cleared |
| `0x100` from `0x10d2e` | the tag on show is hidden, the mast is posted `0x100002` with it and nought (off half a second on), and the arm folded away |
| `0x100` from `0x10d30` or `0x10d2f` | `FUN_0048e0f0( type, thing, item )`: on an answer of 1 the arm is folded away; on 2 the mast is sent `0x100002` for the tag and the arm folded away; on nought nothing more |

**A button's look, `FUN_0048e610`** (the type, the mesh's node, the help row put in the control's `+0x90`): 3
`mb_locate` `0x204`; 4 `mb_locate` `0x203`; 9 to `0x11` `mb_buy`, `0x200` and the type; `0x12` `b_loan` `0x212`;
`0x13` `mb_resrch` `0x213`; `0x14` `b_pkinfo` `0x214`; `0x15` `b_staffcost` `0x215`; `0x17` `mb_callmech`
`0x217`; `0x18` `b_entry` `0xec`; `0x1a` `mb_accept` and `0x1b` `mb_decline`, no row; `0x16` leaves the button as
it was; any other type takes the mesh off and hides it. Rows 515 and 516 both read "Left-click to go there".

**A button's act, `FUN_0048e0f0`** (its answer in brackets): 3 `FUN_004867b0( thing )`, the camera to the thing's
cell (0: the bar stays); 4 `FUN_004868d0( &item )` (0; not read); 9 to `0xc` `FUN_004acc70( 0..3, item )` (2);
`0xd` to `0x11` `FUN_0049bdd0( 0..4 )` (2); `0x12` `FUN_0049fb30` (2); `0x13` `FUN_004aa480`, research (2); `0x14`
`FUN_004a52a0` (2); `0x15` `FUN_004b2750` (2); `0x16` `FUN_004b7b00` (2); `0x17` `FUN_004dfe30( 1 )` on the
thing, the call for a mechanic (1); `0x18` `FUN_00498d80`, the entry price screen (2); `0x1a` and `0x1b`
`FUN_004d2df0` and `FUN_004d2ec0` on the thing `FUN_00519610` answers (2). None but 3, 9 to `0xc` and `0x18` is
read further; 9 to `0xc`, the buy screen on a tab, are in `advisor-park.md`, "A park with no ride in it".
`FUN_00498d80` is the entry-price screen's builder: nothing where the screen is open already (`[0x007ca210]`),
else the screen with the Finance category's last screen noted as 10 (`FUN_004a0810( 3, 10 )`) and its own advisor
message, `0x12f`, posted. On its answer of 2 the bar's handler posts the mast `0x100002` for the tag with nought
(hidden, and off half a second on) and folds the arm away. **Built** (Q319, `ParkGadget.BarButton`): the tag of
the advisor's row 1 carries it (`advisor-park.md`, "A shut park people wait outside"), and in the original its
click read as that: a slot took `0x12f`, the handle went back to 331 and no tag was held 0.53 s on.

**The stream at `0x007502b8`** is seven rows of four shorts, a change of the control's left, top, right and bottom:
three of (-2, -2, 2, 2), three of (2, 2, -2, -2) and the end mark `0xd8f3`. The tag on show swells two a side a
step for three steps, shrinks the same, rests a step and goes round again.

### The arm's handler, `0x004a14f0`

In no function; a jump table over messages `0xa` to `0x100`. The arm's byte `+0x134` is its state: 0 at rest, 2
going out, 3 coming in, 4 coming in for a panel waiting.

- **`0xa`, out with a panel.** In any state but 0 the panel is kept in the arm's data word 2 and nothing more is
  done. At rest: timer `0x80080` is armed at 20 ms and the arm's `+0x130` set to ten; the panel is linked under
  the arm (`FUN_0065f447`) three depths under its own; its rectangle is put at the arm's left less its own width
  and fifty, its middle on the arm's (`0x004a1613`), and each of its children is shown once its right edge is past
  the arm's left (`FUN_004a1440`); **the step is a tenth of the panel's width, the body's right edge less the
  arm's left, and a hundred** (`0x004a16d7`, a whole division); the retract button `0x24` is switched on; sound
  `0x1c` is played (`FUN_0051bfc0`); and the panel is sent `0x1001d9`.
- **`0xb`, in.** Nothing with no panel carried. With one: the panel is sent `0x1001d8`, the timer is armed again
  (`FUN_0065ef90` answers 1 and resets one already running), the step is a tenth of the arm's left less the handle's
  (`[0x007cb2d0]`), a whole division from wherever the handle stands, so a slide out turns back where it is and an
  `0xb` while coming in starts the ten steps again; the retract button is switched off and sound `0x1c` played; the
  state is 3, or 4 where the message's word is -1. **No sender passes -1** (`FUN_004a2590`, `FUN_004a25f0` and the
  retract button's `0x100` pass nought; nothing else sends the arm `0xb`): state 4 is dead by CODE.
- **Timer `0x80080`.** While `+0x130` is over nought it goes down one and the arm's right edge moves a step; the
  arm's resize (message `0x12`, `0x004a1a0e`) moves the panel as far (`FUN_0065c7fe`) and shows it and each of its
  children where its right edge is past the arm's parent's, the body's 439 (`FUN_004a1440`, which sets every one,
  so a part `0x1001d9` hid is shown again: the bar's button of type nought and a decline's erase button, each with
  no mesh, stand shown and take the pointer once the arm is out). At nought the timer stops: coming in (state 3),
  **the arm is put back at its rest rectangle**, the one the builder stored with the narrowed arm (`FUN_0065cf17` at
  `0x004a23ee` writes the control's `+0x14`; `FUN_0065d4cb` reads it at `0x004a18ef`, `FUN_0065cc04` sets it at
  `0x004a1933`), the panel is hidden and unlinked, sent `0x1d` with nought, and sound `0xc2` played
  (`FUN_00485aa0`, cat_ui flat); in state 4 the same but the arm's parent is posted `0xb` in place of the `0x1d` and
  the sound; going out, the panel is sent `0x1d` with 1. The state is 0, and a panel waiting in word 2 is posted
  `0xa`. The ten steps fire 20 ms apart and an eleventh ends the slide: 0.22 s.
- **`0x100` from `0x24`,** the retract button: `0xb`, or with the arm already coming in the waiting panel is
  forgotten; then control `0x27`, the camcorder button, is put up.

`FUN_004a2590( panel )`: nothing with a panel already waiting; else the arm is sent `0xb` and posted `0xa` with
the panel, so a panel replaces the one carried by the arm coming in and going out again. `FUN_004a25f0( lift )`:
`0xb`, or the waiting panel forgotten while the arm comes in; with `lift`, the camcorder button is put up. The
camcorder panel's own handler (`FUN_00498ad0`) has no arm for `0x1001d8`, so a tag that takes the arm from it
leaves the camcorder button down, and that button's next click, up, folds away whatever the arm carries
(`FUN_00498bd0`).

### The original (`q311/orig/run.sh`, `original/bar.py`; predicted first, seven of seven)

Q265's `bounce.TPWS`, its Belly Bounce broken through its script 20 s after the load; the mechanic's tag came at
24.1 s. Left clicked seven seconds on: `[0x007ca090]` and `[0x007ca094]` read the tag's control; the bar's data
read thing 13, buttons 3 and 0, item 1100 and "Your Belly Bounce ride has broken down, and a mechanic is on his
way to fix it."; its rectangle went from (-334, 1112, 281, 1453) to **(486, 1112, 1101, 1453)** and the handle's
left from 331 to **1151**, in steps of 82 inside 0.2 s; `[0x007ca094]` cleared 20 ms after the last. The tag's
`+0x134` read 0 from the click, and **the tag was still held 70 s after it came**, where an unopened one goes at
30.5 s. Its rectangle went (84, 909, 154, 974), then 2, 4, 6, 4 and 2 wider a side and back, a round every 0.9 s
(a step is 100 ms rounded up to the game's 31 ms frames). `mb_locate` clicked: the view moved onto the Bounce,
the bar and the tag stayed (the frame's help bar reads "Left-click to go there"). The tag clicked again: on show
read 0, the bar and the handle went back in six polls, the tag stayed and stopped swelling. Clicked a third time
and `mb_erase` clicked: the tag's shown bit read 0 at once, none was held 0.53 s on, the arm in. **The camcorder
button** after it: the handle stopped at 851, 520 out, a tenth of (319 + 108 + 100) ten times, where the
stream's rectangles and this game's arm have it 645 out.

### The slide, measured (Q427)

**The original** (`q427/orig/run.sh`, Q265's `bounce.TPWS` off-screen, `bar.py` polling, `record.sh` filming; predicted
first, `q427/PREDICTION.txt`): the camcorder button put the handle from 331 to 851 in steps of 52 in 0.22 s, the arm's
state 2 while it moved, its panel at (482,1206)-(801,1359), and back by -52 in state 3. The mechanic's tag: 331 to
1151 by 82 in 0.23 s, the tag on show and the one being put on show both set as it started and the second cleared at
its end, the bar starting at (-334,1112,281,1453); every one of the bar's six children shown once out, its button of
type nought too; clicked again, back by -82, the tag on show cleared as it started. The film: the arm's green body
squeezed to its rectangle through the slide, the retract cross from its first frame, the postcard button shown a frame
before the camera button. **A turn** (the camcorder button and Ctrl+S within the slide, twice): one step out to 383,
steps of -5 back to 333, 331 at the end, then out by 67 to 1001 with the staff panel at (484,1127): the rest
rectangle, not the tenths.

**OpenTPW** (`ParkGadget.Carry`, `Fold`, `Slide`; `ParkTags.PuttingOnShow`, `Shown`, `FullyOut`; the console's `tags`
reads the arm's move, what it carries and what waits). `q427/confirm.py` on the stock park, paused and stepped 16.7 ms a
frame (`run-fix4`, the final build, nine of nine): the tag's click `Park gadget: arm: Out with panel 10d2d, step 82,
handle 331, sound 0x1c a voice`, the handle 413, 577, 741, 987, 1151 after 2, 4, 7, 10 and 14 frames, the bar hidden
until its right edge passed 439; again, `step -82`, 331 and `sound 0xc2 a voice`; the camcorder button 383, 591, 851 and
its panel at (482,1206); the camcorder button and Ctrl+S three frames on, `arm: In with panel 62, step -10, handle 435`,
331, then 1001 with the staff panel at (484,1127). Side by side with the original's film (`q427/side-by-side.png`) the
bar comes out alike. Main's build had the handle at 1151 at the click and counted `GADGET_ARM_SLIDE`. The camcorder
button's advisor line `0x131` is built (Q428, below). Not heard in either game. In no game run: a panel refused while
another waits (tested only), Full Simulation (not asked: the arm is either mode's), the other three themes.

### The camcorder button's line (Q428)

**The executable.** Three places name message `0x131`, all through `FUN_00486b00` (post) and `FUN_00486b40` (withdraw).
The camcorder button's down, `FUN_00498bb0` (from `FUN_004a0840`'s case `0x27` with 1): `FUN_004a2590( panel, 0 )`,
then the post, whether or not the arm took the panel. Its up, `FUN_00498bd0`: `FUN_004a25f0( 0 )`, then the withdraw.
The panel's own handler, `FUN_00498ad0`, on any `0x100` (99 the camera, `FUN_00481a10`; 100 the postcard,
`FUN_004a9380`): the button's act, the withdraw, then `FUN_004a25f0( 1 )`, which lifts the camcorder button. Its
`0x14` withdraws nothing, so a tag taking the arm from the panel leaves the line be. The fourth hit of the bytes
`PUSH 0x131`, `0x004c0211` in `FUN_004bff70`, is a string id for `FUN_00485b00`, not a message. Row 305 of the metadata
table (`meta_full.json`): posted only, category 1 (the Tutorial's: said once, gone after three cuts), one line,
response 565, sample 583 (9.0 s, "the buttons on this panel let you use camcorder mode ... send email postcards"),
group `0x17f` (no tag).

**The original** (`q428/orig/run.sh`, `q325/stock.TPWS` off-screen, `advisor.py 0x131`; predicted first, five of five):
the button down put `0x131` in a slot on 872 and said it on 873, line 0, beside the panel (`a-1-line.png`); up 3 s in
cut it, slaps 1; down said it on 895 and the camera button cut it, slaps 2; said on 941 and cut by the up, slaps 3;
the fourth down took no slot. The postcard button was left out: it pauses the game and writes a picture.

**OpenTPW** (`ParkAdvisorMessages.CamcorderPanel`; `ParkGadget.ShowArm`, `EnterCamcorder`, `SendPostcard`).
`q428/confirm.py` on the stock park, real clicks (`run-fix2`, the final build, five of five): `post message=305
subject=0 score=1000 threshold=25 tick=790 result=queued`, said at once, response 565, sample 583, beside the panel
(`1-line.png`; side by side with the original's, `q428/side-by-side.png`); the up `withdraw message=305 cleared=0
cut=1 slaps=1`; the postcard button `... slaps=2` with `POSTCARD_BUTTON` counted; the camera button `... slaps=3`; the
fourth down `result=slapped`. Main's build counted `CAMCORDER_PANEL_LINE` and posted nothing. In no game run: a tag
taking the arm from the panel and a panel refused (tested only), the line let end and said once, Full Simulation (not
asked: the button is either mode's), the other three themes. Not heard in either game.

### OpenTPW (Q311: `ParkGadget.OpenTag`, `BarButton`, `EraseTag`, `Carry`, `Swell`; `ParkTags.Open`, `Shut`, `OnShow`)

`q311/confirm.py`, the stock park's Belly Bounce broken 25 s in and the mechanic's tag clicked three seconds
after his line, seven predictions of seven twice and the control's one: the log read `Park gadget: message bar:
tag 72 subject 13 item 1100 bar: out=1 rect=486,1112,1101,1453 arm=331,1069,1181,1495
handle=1151,1058,1304,1503 first=mb_locate/help=516 second=hidden erase=mb_erase/help=-1 camcorder=0` and the
text; `tags` read `timers=0/open=1`; the text's box in the frame went from 0.6% black to 94.8% with white
lettering in its top third; 40 s after the tag came it stood with the bar out; with the camera sent away,
`mb_locate` logged `Camera: to thing 13 on cell (51,23), point of interest (510,230)` and left the bar; the tag
clicked again read `bar: out=0` and `handle=331,1058,484,1503` with the tag standing; opened a third time and
erased, `tags` read `hidden=1` and `out=0` at once and `standing=0` 0.8 s on. `side-by-side.png` holds the
original's frame, this game's and main's. Main's build counted `TAG_MESSAGE_BAR` at the click, drew nothing and
its tag was gone 33 s after it came.

| | The original | OpenTPW |
|---|---|---|
| The arm's way out and in | ten steps 20 ms apart | the same on the frame clock (Q427); a tag's right click refused while it is the one being put on show; a panel asked for while the arm moves waits, and a third is refused |
| The arm's travel | a tenth of the panel's width and 208, ten times: 820 with the bar, 520 with the camcorder panel | the same (Q427) |
| The arm's sounds (`0x1c` as it starts, `0xc2` as it is in) | played | played, cat_ui at the effects' own volumes (Q427); not heard in either game |
| The panel's children as it comes out | each shown as its right edge passes the body's | the same (Q427) |
| The bar's text | the middle panel's, over the right slice | the same lettering; the right slice is put in the tree before the middle one so that it is drawn under it. At 1280 x 720 font 6 is a smaller face than the original's at 1024 x 768, so the line breaks fall later |
| The two stacked buttons | 21 types | dressed for every type whose mesh is a file of `ui.wad` (not `b_loan` and `b_pkinfo`, nodes of files not looked up); types 3, 9 to `0x11` and `0x18` act (`advisor-park.md`, "A park with no ride in it", "Worn rides and no mechanic"), the rest counted `TAG_BAR_BUTTON` |
| Advisor message `0x11c` | posted with the bar, withdrawn as it leaves | the same (`advisor-park.md`, "The line of an opened tag message") |
| The tag on show | swells by the stream, a step every 100 ms on the frame's clock | the same steps on real milliseconds, so a round is 0.7 s where the original's frames make it 0.9 |
| The buttons' depths | one more at every showing | not kept |
| A tag opened, in a park file | written with its whole life, and a load starts its timers again | the same (`ParkTags.Restore`) |

## The help rows, from the game's own UIHELPTEXT.str

Independent confirmation of the whole decode, from a different source than the disassembly — the wording
matches `FUN_004a0840`'s dispatch button for button, and names both readouts:

    469  Left-click to buy and build attractions or to hire staff   b_buy
    470  Left-click to view information screens                     b_info
    471  Left-click to view finance screens                         b_money
    472  Left-click for research (R)                                b_resrch
    473  Left-click to view the map (Space)                         b_map
    474  Left-click to use camcorder mode or send a postcard        b_camera
    477  Happiness of the park visitors                             the `guage`
    478  Current date                                               the `date` readout
    481  Left-click to close this arm                               b_retract
    494  Research is not available in Instant Action mode
    468  Left-click to exit camcorder mode (C)                      b_eject
    479/480  camcorder (C) / send a postcard (Ctrl-P)

`UIHELPTEXT.str` has **589 rows**. Dump them with the local `strdump` harness (`dotnet run --project <strdump> -- <data dir> <file.str> <lo-hi|n>...`;
its path is in `CLAUDE.local.md`), which mounts the file system first.

### Help ids are UIHELPTEXT.str, not UITEXT

Op 0x11's value is a row of **`Language/English/UIHELPTEXT.str`**, read through `Localization.Help( id )`.
**It is a different file from `UITEXT.str`**, which is what the `UIStrings` enum numbers. "help 469 on
`b_buy` vs `UIStrings.ParkClosed = 469`" is **not** a contradiction: the two
tables simply share index space.

**Reading a `.str` outside the game needs the file system mounted FIRST.** `BFSTReader`'s lookup table is a
static `Lazy<BFMUReader>` over `Language/English/MBToUni.dat`, opened through `FileSystem.OpenRead` on the first
string decoded. A `StringFile( Stream )` constructor does **not** avoid it, because it decodes as it constructs.
Assign `FileSystem = new BaseFileSystem( <data dir> )` before constructing anything. Otherwise the first read
throws, and with stderr filtered that looks like "the rows are empty".

## The park map — `FUN_005f0b40`, stream `0x00774da0`

**It loads the shipped image *and* renders live over it** — both, not one or the other.

| Address / offset | Original name | What it is | Evidence |
|---|---|---|---|
| `FUN_005f0b40` | — | The map screen's own function, opened by `b_map` | Gadget dispatch |
| `DAT_00f86d20` | — | The static string `2dmap.tga`, constructed at `0x005f0ad0` with a destructor registered through atexit. **Not map logic** — reading it as part of the screen is an adjacency error (`FUN_005f09a0` ends at `0x005f0ac7`, then NOP padding) | Disassembly |
| `FUN_005f1ea0` | — | Builds a path (note the `0x5c` backslash), passes `&DAT_00f86d20` to a vtable method, stores the image's width and height into `+0x48` / `+0x4c` | Disassembly |
| `FUN_005777b0` | — | Rounds those dimensions up to 64 — `(w+0x3f)>>6` — and allocates `tilesX*tilesY` records of 0x60, each a 64x64 surface. For 512x512 that is **8x8 = 64 tiles** | Corroborates that the loaded image is the shipped TGA |
| `+0x60` | — | A **128x128 grid of classification codes**, 8 bytes a cell, in the screen's structure; `+0x60 + 0x20000 = +0x20060` is the selection field | Disassembly |
| `FUN_005f2050` | — | Fills the grid: **9** a cell of track type 25, **10/11/12** mType 1, mType 3 and `mFlags & 0x40` (below), **1-8** things by type (`+0x4ac`) and flags (`+0x32`) | Disassembly |
| `FUN_005f2380` | — | Paints it: a 9-dword (36-byte) blit descriptor per cell, then outlines footprints by comparing each cell with its neighbour above/below (vtable+8) and left/right (vtable+0xc), then draws a selection highlight for the item at `+0x20060` | Disassembly |

The base image is the park's own **`2dmap.tga`**, 512x512 24-bit, one per theme (fantasy, hallow, jungle,
space).

### It is a heat map, and that is the scope trap

`FUN_005f09a0` is **not** a sprite selector — it is a **three-stop colour ramp**: clamp a value to 0-100,
blend stop1->stop2 below 50 and stop2->stop3 above, patch R/G/B into the descriptor and force alpha 0xff.
The stops are `DAT_00f86bd8`, `DAT_00f86bfc`, `DAT_00f86c20` (runtime-filled — beyond `.data`'s raw bytes,
so a static read gives zeros). The value is a **per-thing metric** fetched differently per radio setting
(`FUN_004e0860`, `FUN_004e1e30`). The first is built (`ParkRideScore.ExcitementOf`); the second's days are kept
(`ParkObjectRings.Satisfaction`) and its average is not; the thing layers are unbuilt. The base image plus the cell codes 9-12 (below) is the honest first cut.

### Controls

Six toggles — **0x982, 0x987, 0x989, 0x993, 0x994, 0x983** — each restored from a bit in `+0x18`
(8, 1, 0x10, 2, 4, 0x20) and each gating one code in the painter: they are **layer filters**. Two radio
groups: **0x98b** with options 0x98e/0x98f/0x98c/0x98d from `+0x1c`, and **0x996** with 0x998/0x997/0x999
from `+0x20` — the **metric pickers**. **0x991** is a label, font 4, **yellow** (0xff,0xff,0,0xff).
**0x990** and **0x995** are display-only (vtable+0x14 with 0), and 0x990's depth+2 is handed to the six
toggles. **0x992** takes handler `LAB_005f0b20`; 0x98a/0x984/0x981/0x988 get `MoveToXY(0,10)`.

### Sprite sheets

Six, in `data\2DMap.wad` — a whole archive listed beside `data\ui.wad` in the loader's table:
`.\data\2dmap\` + `vsprite/rsprite/ssprite/lsprite/esprite/gsprite.tga`. They are constructed as static
globals — `v` at `0xf86d40`, then `r` `0xf86d54`, `s` `0xf86d68`, `l` `0xf86d7c`, `e` `0xf86d90`,
`g` `0xf86da4` (stride 0x14). `FUN_005f3710` walks `&DAT_00f86d48` to `0xf86dc0` by 0x14 calling
`FUN_0056b380`, which allocates 0x18, constructs, loads by name and caches the handle.

**UNRESOLVED:** that walk starts at `d48`, so it skips `vsprite` at `d34`, and its last record `dac` would
put a string at `0xf86db8` — which the park-engine page already records as the map screen's own ECX. So
the "six records each holding a path at +0xc" reading is wrong somewhere. **Do not build on it — re-derive
the table layout first.**

### It pauses

`FUN_005f0b40` calls `FUN_004092a0(0,0)`, clears `g_ParkRunning`, and calls `Advisor_StopQuietly(1)` — the
*quiet* stop, not the crying one. So unlike the gadget this screen legitimately pauses. It posts advisor
message **0x130** as it opens, and refuses to open at all if the game menu is up (`FUN_0048c8d0`) or if it is
already open.

### The map screen's stream, `0x00774da0` — walked in full

Clean op 0, balanced op 5, ending exactly where the `2dmap.tga` string starts at `0x007752e4`. Root
**0x980** is full screen (0,0)-(2047,1537). Every mesh named by its `ui.wad` **file** in brackets.

    0x992  THE MAP VIEWPORT  ( 49,  33)-(1877,1364)  28-point region, handler LAB_005f0b20
    0x98a  nuup   (b_mapup)   ( 80,1079)-( 237,1236) help 263 scroll up      7-pt region
    0x981  nudown (b_mapdn)   ( 80,1236)-( 237,1393) help 264 scroll down    7-pt region
    0x984  nuleft (b_mapleft) (  1,1157)-( 158,1315) help 265 scroll left    7-pt region
    0x988  nuright(b_mapright)( 158,1157)-( 315,1315) help 266 scroll right  7-pt region
    0x986  b_plus             ( 218,1327)-( 275,1384) help 261 zoom in   circle (246,1355) r28
    0x985  b_minus            (  32,1327)-(  90,1385) help 262 zoom out  circle ( 61,1356) r29
    0x990  buttpan1 (f_mapbut1) ( 35,1387)-( 681,1533)   left button panel backing
    0x995  buttpan2 (f_mapbut2) (1356,1387)-(1909,1533)  right button panel backing
    0x991  textpan  (f_maptext) ( 688,1395)-(1351,1525)  text rect (730,1424)-(1295,1503)
    (-1)   b_okay             (1941,1339)-(2024,1422) help 1 "close this screen"

**Six layer switches** (type 2, flags **0x11** = toggle), each with a show/hide help PAIR in op 0x11 and
op 0x12 — the captions for on and off:

    0x987 b_sride (b_srides) ( 64,1409)-( 167,1512)  267/268  rides
    0x994 b_sshow           ( 184,1409)-( 286,1512)  271/272  sideshows
    0x993 b_sshop           ( 303,1409)-( 406,1512)  269/270  shops
    0x982 b_sfeature        ( 423,1409)-( 525,1512)  273/274  miscellaneous items
    0x989 b_strack          ( 542,1409)-( 644,1512)  275/276  tracks
    0x983 b_svisit          (1390,1409)-(1493,1512)  277/278  visitors   <- apart from the cluster

**Two radio groups, both control type 8.** 0x98b is the STAFF overlay picker, a vertical stack on the
right edge (1883,443)-(1985,987); `FUN_005f0b40` restores it from `+0x1c` as 1/2/3/4:

    0x98e b_shandy (1883, 443)-(1985, 546)  279/280  cleaners and litter
    0x98f b_smech  (1883, 592)-(1985, 694)  281/282  mechanics and ride reliability
    0x98d b_senter (1883, 740)-(1985, 842)  285/286  entertainers and queue times
    0x98c b_sguard (1883, 885)-(1985, 987)  283/284  guards, camera coverage and pranks

0x996 is the METRIC picker, bottom right (1526,1409)-(1868,1512), restored from `+0x20` as 1/2/3. The mesh
**map_win (w_map.MD2)** is bound AFTER the group's own close (`0x007752da`), so by the op-5 rule it is the root
**0x980**'s, not the group's or 0x999's:

    0x999 b_ssatis (1526,1409)-(1629,1512)  291/292  customer satisfaction
    0x998 b_shap   (1646,1409)-(1748,1512)  287/288  happiness
    0x997 b_sexcit (1766,1409)-(1868,1512)  289/290  excitement ratings

The help rows confirm the heat map in the game's own words — "excitement ratings", "customer
satisfaction", "ride reliability", "queue times" are all simulation values.

### The cell codes, by exhaustion of the cell accessors

Each is a one-liner on a 0x44-byte runtime cell, except the first two, which read the 0x28-byte record the cell's first
word names (`FUN_004d0af0`), taking its parent's when it is type 12 or 17 (`FUN_0053ad90`):

| Address | What it tests |
|---|---|
| `FUN_00536450` | the record is type 25 (`FUN_0053ad60`) |
| `FUN_005363f0` | the record is type 11, 13, 16, 18 or 25 (`FUN_0053ad30`); false for a type 12 or 17 record with no parent |
| `FUN_00536310` | kind == 1 |
| `FUN_00536320` | kind == 3 or 9 |
| `FUN_00536340` | kind == 9 |
| `FUN_00536350` | kind == 10 |
| `FUN_00536380` | kind == 4 |
| `FUN_005363e0` | kind == 0x15 |
| `FUN_00535db0(cell,mask)` | `*(ushort*)(cell+0xe) & mask` |

Read against `FUN_005f2050`'s nesting, kind 9 is ruled out before code 11 is reached, so:

    code  9 = track type 25       code 10 = kind 1    code 11 = kind 3    code 12 = flags & 0x40

**The runtime kind at `+8` is the cell's `mType`, not the `.MAP` attribute byte.** The cell serialiser
`FUN_004d0b30` names `+0x08` `mType` (`0x004d0f19`; `park-engine.md`, "The runtime cell is not the save cell"), and
over the shipped park `mType` takes 7, 2 and 30, which jungle's attribute byte never does (below; `park.md`,
"`mType` over the shipped park"). So code 10 is a path (mType 1), code 11 a queue cell (mType 3) and code 12 land
outside the park (`mFlags & 0x40`; `park-engine.md`, "The path tool").

### The empirical check, and what it does and does not establish

Attribute values 1 and 3, drawn from `base.map`, reproduce the very features the shipped `2dmap.tga`
draws — the winding channel, its prongs, and the round blob bottom-right — with attribute **17** landing
on the entrance/bus structure at the top. **It is a falsifying test, not a flattering one:** four
orientations were rendered and three failed outright (transpose throws the bus slab to the left edge,
flipV to the bottom), and the full 128x128 extent matched *worse* than the playable crop, squashing every
feature up-left with dead space right and bottom.

    orientation  IDENTITY - x across, y down, indexed x*128 + y   (transpose/flipV rejected)
    extent       96 x 85 cells (x 0..95, y 0..84 INCLUSIVE), NOT the full 128x128

Counts (jungle `base.map`, 16384 cells): 0 x14889 (90.9%), **1 x689 (4.2%)**, 17 x490 (3.0%),
**3 x240 (1.5%)**, 144 x48, 148 x14, 8 x10, 128 x4. And a structural find: **`terrain.map` holds only TWO
values, 0 and 1** (805 cells) — a pure binary mask, not a second attribute grid.

What it proves is that the shipped `2dmap.tga` already draws the attribute map's value-1 and value-3
cells; the map's codes 10 and 11 are paths and queue cells (above), not these. **It does NOT license semantic
labels** — "3 is water, 1 is path" is a guess the pixels do not support, so name them by value, not by
meaning.

### The cell-to-pixel mapping is exact

The image carries a black frame, so the grid maps onto the INNER field, measured as **x 16..495 (480
wide), y 43..467 (425 tall)**:

    cell (x,y) -> pixel ( 16 + 5*x , 43 + 5*y )      jungle's 512x512 2dmap.tga, 96x85 cells
    480 = 96 * 5   and   425 = 85 * 5                EXACTLY, in both directions

The field yields **exactly 5.0000 px per cell both ways** and the aspect matches to four decimals
(480/425 = 1.1294 = 96/85), where the 95x84 extent would give 5.0526 / 5.0595 px and 1.1310 — a near-miss
that tidy is usually an off-by-one, not a measurement. Forcing the full 128x128 onto the same field would
demand **13% non-square** cells (3.750 vs 3.320), which no top-down map would have. Re-overlaid at the
exact mapping the extent border lands on the field edge on all four sides and the hatching covers the
channel, both prongs and the round blob, with attribute 17 over the entrance structure; the 128x128
version leaves the blob untouched.

So the extent is corroborated three independent ways — shape match, feature position, and whole-number
cell size. **Only the arithmetic one could not have been talked into.**

## Constraints on anything built against this

- **A HUD panel must leave `Pauses` false** or it stops `GameClock`, and with it the calendar, particles
  and every model animation. The map screen is the exception: the original genuinely pauses there.
- **The interface takes the mouse before the world does.** `WindowStack.WheelTaken` keeps a wheel a window used
  from also zooming the park (`ParkOrbitCameraMode`), and `WindowStack.PointerTaken` keeps a left press on a
  window, or any left press under a modal one, from reaching the world (`Level.WorldClick`).
  `WindowStack.RightPointerTaken` keeps a right press there from arming the quick click, except beside a park screen
  (`UiWindow.ParkScreen`), which the original builds onto the park's layer (`park-engine.md`, "Whose a right press
  is"). The camcorder key is guarded by `Level.Current?.PausedByWindow() != true`. A new HUD control that uses the
  wheel or a press must go through these, and a new park screen built onto layer 0 sets `ParkScreen`.
- **HUD work is verifiable by eye and capture, not by test** — it needs someone to look at the running
  game.

---

## The buy and hire screens, and the list control under them

Decoded 2026-09-20 by an eight-dimension pass, every claim then re-derived by a second agent that was
told to refute it.

### The scrolling list is UI control **type 7** - `UiList` here

Both screens are a single multi-column scrolling list class — ctor `FUN_00662562`, 0x178 bytes, vtable
`0x007059f0`, type getter returns **7**. OpenTPW's is `UiList` (`source/OpenTPW/UI/UiList.cs`); everything else on
these screens already had an analogue.

| Function | What it is |
|---|---|
| `FUN_006649d5` | Clear — frees each TEXT column's string, rebuilds the free-node chain, resets count/selection/scroll |
| `FUN_0066403b( &rec, id, insertAfter, commit )` | Add a row. **`rec` is `columnCount` dwords, not a fixed struct** |
| `FUN_00663324` | Commit/refresh — the only thing that moves data into widgets |
| `FUN_006639cb( factory, styler )` | Builds the reusable row widgets. **Neither argument is a draw or sort callback** |
| `FUN_006636b2( col, type )` | Column type: 0 = text (string, strcmp-sorted), 1 = numeric |
| `FUN_00664c71( rowIndex, &out )` | Row index → that row's stored id |
| `FUN_0066a295( memberId )` | Selects a member of a type-8 radio group (the tabs) |

**The record is sized by the column count.** Buy pushes **three** — `{char* name, int price, int state}`
— and hire pushes **two**, `{char* name, int wage}`. A fixed three-field struct is wrong for hire.

**The state column is a tick-box, not a number.** Buy's styler skins column 2 with the mesh
`i_boxtick` and calls SetValue, so the {0,1,2} picks one of three frames.

**`FUN_006636b2` must be called AFTER the tree is parsed**, because op `0xb` resets every column's type
to 1. A hard ordering constraint that fails silently.

**Row widgets are a fixed reused pool.** `FUN_006639cb` builds one chain per *visible slot*, and the
slot count is derived from the measured height of the factory's first widget — so the prototype row
must exist before the number of slots is knowable. Rows are never drawn; their values are pushed into
the pool on every refresh.

### The messages, and why one signature will not do

    0x400  row activated      (listId, selected ROW INDEX)
    0x401  selection changed  (listId, selected ROW INDEX)
    0x406  sort changed       (listId, signed column+1)
    0x100  button clicked     (CONTROL id, no row at all)

`param_3` means different things for `0x100` and for `0x400`/`0x401`. Index and id diverge as soon as
the list is sorted, which is what `FUN_00664c71` is for.

Three further messages are implemented by the class and handled by **neither** screen: `0x402`
right-click a row, `0x404` column hit, `0x405` visible range changed. By this project's own rule they
are dead by CONTENT, not by CODE. The all-staff, visitors and all-items screens do answer `0x402`: the camera goes to
the row's thing and the screen closes (`park-engine.md`, "The camera goes to a thing").

### A list's order: the sorted insert, and a heading's click (Q130d)

Decoded and measured 2026-10-06. **Every list whose flags `+0x48` hold `0x10` keeps itself in order**, and all seven
list records do (buy `0x91`, the rest `0x291`). The sort is two words on the list: the column `+0x168` and the
direction `+0x16a` (0 ascending, 1 descending).

- **The add is a sorted insert.** `FUN_0066403b` with an insert-after of -1 takes a free node, then walks from the
  head until `FUN_00663edc( new, row )` answers 1 and links the new row before that one, or after the last. For a
  numeric column (type 1) that is `new < row` ascending and `row < new` descending, both strict, so **equal rows stand
  in the order they were added**. For a text column (type 0) it is 1 when the row's text is null, else
  `FUN_0067c290( new, row ) < 0` ascending (the arguments swapped for descending): a compare of 16-bit characters by
  their plain values, with no case folding and no locale, so every capital stands before every small letter.
- **A heading's click re-sorts.** The headings are the list's children `0x10 + column`; a child's click reaches the
  list's proc `FUN_00665c35` as message `0x100` and goes to `FUN_00665a44( id )`. On the column already sorted on it
  flips the direction (`FUN_0066594d( -1 )`); on another it sets ascending, then the column (`FUN_0066590f`). Either
  way it sorts the whole list (`FUN_006628f4`), commits (`FUN_00663324`) and, with a row selected, posts `0x401` for
  the selected index, which now names a different row. Each setter that changed its word posts `0x406` with the column
  + 1, negated when descending (`FUN_006658b9`).
- **`FUN_006628f4` keeps equal rows as they stood.** It moves the first least row to the head, then takes each row
  from the third on and links it before the first row it is strictly less than. So a click on Monthly Wage from name
  order leaves equal wages in name order, where a fresh fill leaves them in the order added.
- **A screen remembers its sort in one word of its own**, for the session: its handler stores `0x406`'s value, and
  its opener hands the word to `FUN_006659af( word, 0 )` while the list is still empty, which sets both words and
  sorts nothing. Hire: `[0x007523e4]` (written `0x0049b85e`, read `0x0049c0f0`). Buy: `[0x00755140]` (`0x004ad057`).
  Visitors: `[0x007508bc]`. All three are 1 in the file: the first column, ascending.

**So the hire list stands in name order because its first column is the name.** `FUN_0049b5b0` clears the list and
adds the tab's candidates in slot order, 0 to 31, each through the sorted insert, the slot as the row's id; a
candidate who joins while the screen is open (`FUN_00481550`, `FUN_0049c970`) goes through the same insert. Nothing in
the hire screen's own code orders anything. The name handed over is `FUN_005074a0`'s text, held to 39 characters on a
fill and 40 on a join.

**In the original** (under Proton, the stock Lost Kingdom park, each predicted first; `q130d/orig`): the word read 1
with the screen open; a click on Candidate Name made it -1 and reversed the rows, on Monthly Wage 2 and by wage
rising, again -2 and falling; another tab, and the screen closed and opened again, kept -2 and the order. With the
word back at 1 all five tabs stood in name order, and four of them not in slot order, each row tied to its slot
through the pool's name index and the `american` tables (janitors: slots 19, 1, 17, 2, 14, 7). Ties, on six
entertainers: a click on Monthly Wage from name order gave Duncan Kershaw, Shintaro Kanaoya, Simon Carter at 48
(name order), and another tab and back gave Shintaro Kanaoya, Simon Carter, Duncan Kershaw (slots 0, 8, 13). The buy
list reversed on a click on Name and was still reversed when opened again.

**The hire and buy lists' headings** are the stream's `op 0xc` children, lettered and framed by `FUN_00486660(
control, text, font, r, g, b, help )`: a label skin, the nine-slice frame `!cbut`, the text centred both ways, the font
slot and colour handed over, and the help row at `+0x90`. Both openers hand font slot 8 (`[0x007523e0]`,
`[0x00755138]`) and the colour 255, 239, 0.

| List | Column | Rect | UITEXT | Help | Type |
|---|---|---|---|---|---|
| hire (`0x00751fa8`) | 0 | 1039, 331, 1460, 436 | 144 "Candidate Name" | 159 | text |
| | 1 | 1471, 331, 1731, 436 | 145 "Monthly Wage" | 160 | number |
| buy (`0x00754cf8`) | 0 | 1032, 317, 1454, 422 | 123 "Name" | 144 | text |
| | 1 | 1458, 317, 1664, 422 | 124 "Price" | 145 | number |
| | 2 | 1669, 317, 1729, 422 | 138 "." | 146 | number, the tick's state |

The types are `FUN_006636b2`'s (hire from `0x0049c0bf`, buy from `0x004ad027`); the visitors list leaves all six columns
numbers. The buy list sorts on the text its row shows, so a mystery ride's "??? Mystery Ride! ???" stands first, and
the two land rows are sorted in with the rest (`FUN_004aaf70` adds them through the same insert, Buy Land priced at
`[0x0078560c]`).

**OpenTPW** (Q130e): `UiList` keeps the column and direction (`SetSort`, `SortWord`), `Add` is the sorted insert
whenever a sort is set, `HeadingClicked` is `FUN_00665a44` and `Sort` `FUN_006628f4`; a heading is a button
(`AddHeading`). The hire, buy, visitors and all-staff screens each keep their word in a static for the session, the
all-items screen one a tab (Q374). A list built with headings and no sort, which only a test builds, counts a heading's
click (`LIST_HEADING_SORT_NOT_BUILT`).
Time In Park sorts on its hours (Q372), the "?" column on its thought's part (Q373). The buy list's rows are
handed over in name order, where the original walks its catalogue (`FUN_00412e60`): that shows only among rows equal
on the sorted column.

**Measured, both games side by side** (Q130e, 2026-10-07, each predicted first; `q130e/`). The original, on the save's
own sixteen candidates (every photograph before `mGameTick` 858, when the first times out), and OpenTPW with the park
paused, gave the same rows on all five tabs, apart from three names the `american` tables give differently, which
stand where their own letters put them. The entertainers (slots 5, 9, 13 at 60, slot 23 at 84): Monthly Wage, word 2,
the three at 60 in name order then 84; the mechanics' tab and back, word 2, the three in slot order; Monthly Wage
again, -2, the 84 first and the three as they stood; Candidate Name, 1; again, -1. The buy list's rides: Name -1,
Price 2, Price -2, the tick 3, the four rows the same in both at each step. The original's features tab at -1 reads
Small bathroom, Small Tree, Small Bush: a small letter after every capital.

### The pointer over a list

Under the list flag `0x80` the proc `FUN_00665c35` answers the pointer's move, `0x10001`, and `0x10003`, through
**`FUN_006656a0`**: when the list's `+0x11c` is nought and the point is inside the control's region (`+0x2c`), it
hands the point to `FUN_0066552e`, the same row test the clicks use. So the row under a moving pointer is selected
with no press, `FUN_0066525c` posts `0x401` when that changes the selected row, and a point over no row leaves the
selection where it was. The buy and hire lists carry the flag, as the five list trees below do. `+0x11c` is the
control's own record of the buttons pressed on it: the base proc sets a button's bit on its press and clears it on its
release (`FUN_0065f6d1`, `0x0065f871`, `0x0065f9e1`), and the button class and the slider's thumb keep theirs the
same way. So a list follows the pointer only while no button pressed on it is down. OpenTPW: `UiList.PointerMoved`,
told by the stack whether a press that landed on the list is still held; whether the original's bit outlives a
release let go off the control is not decoded. What sends `0x10003` is not traced.

**OpenTPW**: `UiList.SelectsUnderPointer` and `PointerMoved`; `WindowStack` sends the move to the control under the
pointer on a frame the pointer moved. Every list sets the flag: the buy list (Q233b), the park file screen's
(`ParkFileScreen`), the hire list (Q356), and the visitors, all-staff and all-items lists (Q376). A pick under the pointer
logs `List 0x<id>: row <id> at <index> picked under the pointer`.

**The visitors, all-staff and all-items lists follow the pointer** (Q376). Their tree records carry the flag
(`0x00750716`, `0x00750e5e`, `0x00750ab8`, `0x00750ba0`, `0x00750ca0`: type 7, flags `0x291`), and none of their
handlers has a `0x401` arm (the visitors' `0x00493230`: `0x400`, `0x402`, `0x406`; the all-staff's `FUN_00495da0`: the
same three; the all-items' `FUN_00495360`, "The miscellaneous tab", below), so the pick is the highlight alone, and a
right click on the strip under the rows names the row last hovered. **Measured in the original** (`q376/orig/a`,
`q373/thought.TPWS`; `rectcolour.py`'s teal share of each row's first cell): the visitors list opened with row 0
highlighted (0.934), and the pointer brought along row 2's band from the left of the list moved it there (0, 0, 0.917);
the misc tab, row 0 at its opening (0.949), row 2 under the pointer (0.811), still row 2 with the pointer brought round
outside the list to the strip under the rows (0.917), and a right click there shut the screen with the view on a
security camera. The all-staff list was not run in the original: every scene has one member a kind, which the add's
pick has highlighted already. **OpenTPW** (`q376/run-fix1`, the final game binary, the stock park paused, the pointer
put on a row by XTEST): `List 0x1e497: row 40 at 2`, `List 0x321: row 25 at 0`, `List 0x190: row 19 at 2`, each row's
teal 0.92 and every other row's nought, nothing picked on the strip, and the right click there `Camera: to thing` 40, 25
and 19, each screen shut. Main's build (`run-control1`): no highlight on the visitors or the staff, the misc tab's row 0
kept, and its right click to thing 17; the other two counted `LIST_RIGHT_CLICK_FIRST_ROW_NOT_SELECTED`.

### A frame worn at two sizes

A `!` mesh is a nine-slice frame grown to its control's size (`0x00477310`). `UiMesh` kept one set of vertices for
each frame part and re-wrote it for whichever control drew next, so two controls of different sizes wearing the same
frame on one screen were both drawn with the last size written: the buy screen's description panel, 414 by 414, was
drawn 712 by 281, the stats panel's shape, squeezed into its own rectangle's corner. Each size now has vertices of
its own (`UiMesh.Sized`). Photographed before and after (Q233b); no test, as a part needs a graphics device.

### A right click on a list

The list's proc `FUN_00665c35` answers the message `0x11006`, which carries a button and a point: the left button
(`param_3` nought) goes to `FUN_006655a2`, the row click above; any other goes to **`FUN_0066563d`** (`0x00665dbd`).
The base proc's click `0x10006` becomes `0x11006` in `FUN_0065dcaf` ("Who acts on the click, and who on the
release"). `FUN_0066563d` does nothing unless the list's
flags `+0x48` have `0x80`. Then `FUN_0066552e` hit-tests the point against the row region (`+0x138`), divides its
height above the rows' top (`+0x13e`) by the row height (`+0x170`), and hands the slot to `FUN_0066525c`, which
selects it only when it is under the visible count (`+0x172`) and the row count; and then, **if any row is selected**
(`+0x14c` not negative), it posts `0x402` to the parent with the list's id and the selected row.

So the right click names the selected row, having first selected the one under it: a click on the list that misses
every row still names the row selected before. All five trees of the three list screens carry flags `0x291`
(`0x00750716`, `0x00750e5e`, `0x00750ab8`, `0x00750ba0`, `0x00750ca0`): `0x80` set, and `0x100` clear, so a slot past
the rows leaves the selection where it was.

**In the original**: the visitors list opens with one row highlighted (the first on one opening, the second and the
tenth on two others; what chooses it is not decoded, `docs/QUEUE.md` Q232: the pointer resting over the list is one
candidate, as the list follows it, Q376), and a quick right click on the strip under
its tenth row went to that row's guest, not to an eleventh row's.

**OpenTPW**: `UiList.RightClickedAt`, called by `WindowStack.RightClick` with the press's point, and
`UiList.RowRightClicked`. The strip under the last whole row is no row's for either button. Every list follows the pointer ("The pointer
over a list"), so a miss names the row last hovered. The hire and all-items lists pick their first row as they fill
("The first row is picked as a list fills"), so there a miss before any hover names that row; the others open with no
row selected, and a miss before any row is hovered or chosen names nothing: counted,
`LIST_RIGHT_CLICK_FIRST_ROW_NOT_SELECTED`.

**With flag `0x80` set, `0x400` is posted once, on the click** (`FUN_006655a2`, from `0x11006`), and once more on a
double click's second press (`FUN_006656fa`); nothing posts it on a bare press ("Who acts on the click, and who on
the release", measured). `UiList.LeftClickedAt` and `LeftDoubleClicked`.

### The structural surprise: the tab group is a CHILD of the list

In both trees the tab button group, the column headers and the scrollbar are all children of the list
control. **Tab clicks reach the screen only by bubbling to the list's handler** — parenting the tabs to
the dialog instead would mean never seeing them.

### Ops 9 and 10 are runtime-conditional, and that is not optional detail

Op 9 creates the scrollbar (type 3 slider, flags 0x10, id 1) and op `0xa` writes the list's **row-area**
rect. **Both consume their operand only when the enclosing control's type is 4 or 7**; under any other
parent they read nothing. A walker that always eats 8 bytes for op `0xa` desynchronises the whole tail.
Op 3's text rect and op `0xa`'s row area happen to hold the same values on both these screens, so
conflating them looks correct here and breaks elsewhere.

### Both trees, walked to a balanced op 5

**BUY — `0x00754cf8`, 1080 bytes, 32 controls.** Handler `FUN_004ac270`.

    0x1e9  root (186,30)-(2018,1007)          mesh 0xf76e4200 = node "window4" in w_big.MD2
      0x1ea  panel  + text rect               !frame
        0x1eb, 0x1ec
      0x1ed  stats panel                      !frame
        0x1ee 0x1ef 0x1f0 (type 9 bars) ; 0x1f1 0x1f2 0x1f3 0x1f4 0x1f5 0x1f6 0x1f7 (labels)
      0x1f8  THE LIST (1009,179)-(1813,902)   mesh buyitem, 3 columns
        0x1f9  TAB GROUP
          0x1fa b_sshow help 142 | 0x1fb b_sride help 140
          0x1fc b_sfeature help 143 | 0x1fd b_sshop help 141
        0x10 0x11 0x12  column headers ; op 0xa row area (1037,427)-(1727,871)
        op 9 SCROLLBAR id 1  (!slider, b_up, b_down, b_scroller thumb)
      0x1fe  TITLE     0x200  MONEY
      id -2  EXIT b_exit help 2 ; 0x1ff -> HIRE  b_allstaff help 153

**HIRE — `0x00751fa8`, 1074 bytes, 31 controls.** Handler `FUN_0049b650`.

    0x247f root                                mesh 0xf76e4200 = node "window4" in w_big.MD2
      0x2480 TITLE ; id -2 EXIT
      0x2481 staffinfo   -> 0x2482 skill bar, 0x2483 label
      0x2484 PORTRAIT    staffpic
      0x2485 MINI-BALANCE help 163             mesh balance
        0x2486/0x2487  Balance      0x2488/0x2489  Cash in
        0x248a/0x248b  Other costs  0x248c/0x248d  Staff costs
      0x248e THE LIST                          mesh hirestaff, 2 columns
        0x248f TAB GROUP
          0x2490 b_shandy 154 | 0x2493 b_smech 155 | 0x2492 b_senter 156
          0x2494 b_sguard 157 | 0x2491 b_sresrcher 158
        0x11 then 0x10 column headers (reversed in the stream) ; op 9 SCROLLBAR
      0x2495 -> BUY  b_allthings help 162 ; 0x2496 MONEY

#### The mini-balance is a MONTHLY ACCOUNT, not the park's cash

From `FUN_0049bdd0`, corroborated by the `0x401` arm of `FUN_0049b650`. Three monthly ring buffers on
the park analyser (`mParkAnalyser`, `FUN_00519510`), each stored as samples / index / count / wrapped, each fed by the
month's running total 4 bytes before its samples (`+0x1fc90` cash in, `+0x1f7f0` staff, `+0x1f5a0` all costs, the cost
of goods among them), pushed on the month's change (`FUN_004c7720`; `ride-operation.md`, "The settle-up's bookkeeping"):

| Samples | Index | Count | Wrapped | Becomes |
|---|---|---|---|---|
| `+0x1fc94` | `+0x1fed4` | `+0x1fed8` | `+0x1fedc` | `DAT_007ca2f0`, cash in |
| `+0x1f7f4` | `+0x1fa34` | `+0x1fa38` | `+0x1fa3c` | `DAT_007ca2fc`, staff costs |
| `+0x1f5a4` | `+0x1f7e4` | `+0x1f7e8` | `+0x1f7ec` | `DAT_007ca2f8`, total costs |

    0x2489 Cash in     <- DAT_007ca2f0
    0x248c Staff costs <- DAT_007ca2fc              (on 0x401, plus the candidate's wage)
    0x248b Other costs <- DAT_007ca2f8 - DAT_007ca2fc
    0x2487 Balance     <- DAT_007ca2f0 - DAT_007ca2f8

A negative index reads `index + count`, and only when the wrapped byte is set - a ring that has not
filled once yet; the count is tested first, and a count of nought reads nought (`0x0049c54f`). The figures are read
once, as the screen opens; nothing rereads them while it stands. **A pick (`0x401`) moves two of them**: the staff costs
read `DAT_007ca2fc` plus the candidate's wage (`FUN_005077f0`) and the balance `DAT_007ca2f0` less the wage less
`DAT_007ca2fc` less `DAT_007ca300`, the kept other costs: cash in less all costs less the wage. Every figure is the
money skin's (`FUN_00490250`): "$ 1500", "-$ 11675". **The balance row is last month's net, so the park's cash does not belong in it**; a
build that puts it there shows the same figure twice, once correctly as `0x2496` and once mislabelled.

**The two cost rows are red.** `FUN_0065c5d5( 0xff, 0, 0, 0xff )` is called while `0x248d`/`0x248c` and
`0x248a`/`0x248b` are the current control, and never for `0x2488`/`0x2489` or `0x2486`/`0x2487`.

**What feeds the money months** (read 2026-10-15, Q385 and Q386; every writer of the offsets, a scan of the image's operands):

| Total | Added by |
|---|---|
| `+0x1fc90` cash in | every deposit (`FUN_004d0190`), the bank's month turn's batch (`FUN_004d0370`), the gate fee (`FUN_004d0600`, which adds to the gate's `+0x1fee0` too) and a loan taken (`FUN_004d0750`) |
| `+0x1f5a0` all costs | every withdrawal made (`FUN_004d01f0`, nothing while `+0x114` is nought), a loan's instalment (`FUN_004d0370`) and a loan paid off (`FUN_004d0850`) |
| `+0x1f7f0` staff costs | a member's monthly wage (`FUN_00504c70` on message `0xc`) and a dismissal's (`FUN_00505790`), each after its withdrawal and whether or not that was made |
| `+0x1fa40` training | a member's training share (`FUN_00505a10`, from thing 1's month turn), after its withdrawal and whether or not that was made; nothing at grade 4 |
| `+0x1fee0` gate takings | the gate fee alone (`FUN_004d0600`, which adds it to cash in first) |
| `+0x20130` shop takings | a shop's charge (`FUN_004e16b0` where the item's descriptor `+0x4ac` is 1, `0x004e1711`), after its deposit onto cash in |
| `+0x20380` sideshow takings | a sideshow's charge (the same, `+0x4ac` 2, `0x004e18a6`); a ride's (0) goes onto neither |

The close (`FUN_004c7720`, the analyser's turn of the month, after thing 1's training and before the bank's turn and
the wages) pushes each total into its ring (`FUN_004ce290` for all costs, staff costs, training and cash in; the same
steps written out in line for the gate, shop and sideshow takings: the slot one on, nought past the last with the
wrapped byte set, the total written there) and zeroes it; the constructor (`FUN_004c58c0`) leaves each slot -1, each
count 144. The same close samples the analyser's other nine rings, which no add feeds: **the bank's balance first**,
before any other ring (`FUN_006ad810` on the bank, `FUN_005195d0`'s thing `+0xc`, into `+0x1f100`'s next slot; its own
total `+0x1f100` is never written and reads 0 in every file), and after the seven money rings the other seven in this
order (read 2026-10-15, Q389, `0x004c788c`..`0x004c7a7a`; not ring order): the month total `+0x20cc0` closed as a money
ring is (`+0x20cc0`, added by `FUN_004cf3e0`; below, "The arrivals"), `FUN_004c7fa0` (`+0x205d0`; it and the 12th below, "The guests inside and the rides at a close"), the lifetime visitors `+0x21c08`
(`+0x20820`, below, "The lifetime visitors"), `FUN_004c8070` (`+0x20a70`), `FUN_004c7bb0`'s float (`+0x20f10`; below, "The guests' happiness at a close"),
`FUN_004c7db0`'s figure (`+0x1f350`; the graph's "Park value", below), a mean of `FUN_004cef30` (`+0x21160`; below,
"The leavers' stays") and `FUN_004cceb0` (`+0x213b0`; below, "The park score").

**The finances screen graphs eight of them** (`FUN_0049a2e0`, its range `DAT_007ca2ec` 12, 36 or 144 months; the rings
picked by the bits of `DAT_00751fa0`, 1 at the start: 1 the balance, 2 `+0x1f350`, 4 cash in, 8 gate takings, `0x10`
shop takings, `0x20` sideshow takings, `0x40` all costs, `0x80` staff costs; `FUN_0049a7b0` takes the largest month
of those picked for the scale through `FUN_004801d0`). Bits 1 and 2 and the other six do not stand together: a row of
one group ticked clears the other's. Training has no row.
The rings' place in a park file is `saves.md`, "The park analyser": in the shipped park one month is closed, cash in
1500, staff costs 0, all costs 13175, so the original's hire screen opens on "$ 1500", "$ 0", "$ 13175", "-$ 11675".

**OpenTPW** (Q385). `ParkMonthRing` is a ring; `ParkState.MonthCashIn`, `MonthStaffCosts` and `MonthCosts` are the
three, the file's at a load (`ParkWorld`) and empty in a park made with no file; `Deposit` and `Take` add to cash in,
`Spend` to all costs, `AddStaffCost` (from `PayTheWages` and `Fire`) to staff costs; `TurnTheMonth` closes them first;
the writer writes them back (`ParkWorld.PutMonthRings`). The hire screen (`ParkHireScreen.ShowAccount`) and the training
screen read the newest month as they open. **The other four money rings are kept the same way** (Q386):
`MonthTraining` (`AddTrainingCost`, from `ParkPeople.Train` after its `Spend`), `MonthGateTakings` (`Take`),
`MonthShopTakings` and `MonthSideshowTakings` (`AddShopTakings`, `AddSideshowTakings`, from the charge by the item's
`UiType`); `TurnTheMonth` closes the seven in the close's order and the writer writes them (`ParkWorld.MonthRingSet`).
Nothing here reads them: the finances screen is not built, and the original's reads them from a file this game wrote.
**The balance ring is kept too** (Q387): `MonthBalance`, the file's at a load, `ParkMonthRing.Push( Balance )` first in
`TurnTheMonth`, so after the training's withdrawal and before the bank's batch, the instalments and the wages, as
`Level`'s month turn calls them; written as the 1st ring. Measured in 579 park files: where its slot is the money
rings' (573; the six others are this game's own, written before it), each closed month is the last plus that month's
cash in less its all costs in 572; the one other (`q252/orig/ctl-written-by-the-original.TPWS`) holds as its second
month the shipped file's month so far unchanged where its balance fell 494 more, by every sign a month taken over from a
file this game wrote before Q385 kept the costs. **The park value ring is kept too** (Q388): `MonthParkValue`, the
file's at a load, `ParkMonthRing.Push( ParkValue.Of( ... ) )` after the seven money rings in `TurnTheMonth`, written as the
2nd ring (below, "The park value"). **And the lifetime visitors' ring** (Q389): `MonthVisitors`, the file's at a load,
`ParkMonthRing.Push( LifetimeVisitors )` after the money rings and before the park value, written as the 11th (below,
"The lifetime visitors"). **And the guests inside' and the rides' rings** (Q390): `MonthGuestsInside` and `MonthRides`,
written as the 10th and 12th (below, "The guests inside and the rides at a close"). **And the arrivals'** (Q391):
`MonthArrivals`, written as the 13th (below, "The arrivals"). **And the 14th and 15th** (Q392, Q393; below), **and the 16th** (Q394;
below, "The park score"): all sixteen are kept.

#### The park value

`FUN_004c7db0` (read 2026-10-15, Q388), a thiscall on the analyser with one argument, a number of quarter ticks. Its
answer is kept at `+0x1f0ec`, stamped at `+0x1f0e8` with `mGameTick` (`FUN_0041a960`) and flagged at `+0x1f0f0`: with
the flag set and the argument at or over the quarter ticks since the stamp (`FUN_0041a990`, `(tick >> 2) - (stamp >> 2)`,
unsigned) the kept answer is given again. Its two callers: the month's close (`0x004c79a7`, argument nought, so a fresh
sum unless one was taken in the same quarter tick), which pushes it into the 2nd int ring, `+0x1f350` (its slot
`+0x1f594`, its months `+0x1f354`), and the loans screen's test of a loan (`FUN_004d06b0`, argument `0x14`), which
weighs the value (times the float 1.0 at `0x00700370`) less what the bought loans still owe against the loan's record;
not read further. The analyser's serialiser
`FUN_004c5d70` writes the three kept fields too. The sum:

- **Every cell** of the 128 x 128 runtime array (world `+0x2d8`, `0x44` bytes, id `y*128+x+1`) whose `mFlags & 0x40`
  (outside the park) is clear adds by `mType`: 0 adds 2, 1 (path) 3, 2 adds 1, 3 (queue) 5, 9 adds 6 (both the `3 or 9`
  test and its own), 10 adds 1, every other nought (`0x004c7e24`..`0x004c7e7d`; `FUN_005d60c0`, which would add 10,
  answers nought). **And 50 where its track record counts** (`FUN_005363f0`): the record the cell's first word names,
  or for a type 12 or 17 its parent's (`+0x10`; none with no parent), of type 11, 13, 16, 18 or 25.
- **Every object on the chain** (`mFirstObject`, world `+0x1da746`, linked at `+0xc`) but a condemned one (`mState` 4,
  `FUN_004e0380`) adds the item's `CostOfUpgrade` (descriptor `+0x1b8 + 0x40 k`) for every tier `k` from nought to its
  `mUpgradeLevel` (`+0x50`), times `__ftol( State of repair, +0x44 ) & 0xff`, over 100, a signed whole division.

**Measured.** From the file alone (`lib/parkfile.park_value` with `lib/itemcosts`), the shipped `Easymode.TPWI` sums to
its own ring's one closed month, 23977: cells 19527 (2362 of type 0, 78 path, 4 queue, 8 of type 9, one of type 10, and
290 track cells, 216 of type 12 whose parent is a 25 and 74 of 25), objects 4450 at full repair (the fountain 150, three
toilets 100, the staff room 500, two cameras 250, a bin 100, the Drinks Shop 650, the Jungle Spray 1750, the Belly
Bounce 500; the gate and the two fixed items nought). Of 581 park files on hand 186 sum to their ring's newest month,
the shipped park and its untouched copies; the others were worn or built on after their last close.

**OpenTPW** (Q388). `ParkValue.Of` is the sum, from `ParkState.Record` and `ObjectsInChainOrder`, the tiers' costs
`ParkItemCatalogue.Item.UpgradeCostAt` (`ItemDescriptionFile.UpgradeCostAt` for tiers 1 and 2); it takes the sum afresh
every time where the original's close can reuse one of the same quarter tick (said at the site). Nothing here reads the
ring: the finances screen and the loans screen are not built.

#### The lifetime visitors

Read 2026-10-15 (Q389). **`mLifetimeVisitors` (`+0x21c08`) counts the admission fees taken**: `FUN_004c7520`, called by
the gate fee `FUN_004d0600` after its deposit and its gate takings, adds one to it whatever the fee, nought included,
and raises `mMostPaidForTicket` (`+0x21c40`) to the fee where the fee is above it (a signed compare, `0x004c754f`);
then it sends the challenge manager its message `0x1a` (`ride-operation.md`, the fee's row). Nothing else writes either
but the constructor `FUN_004c58c0` (nought, nought) and the serialiser `FUN_004c5d70`. It is not the header's
`mNumberOfVisitorsToDate`, which counts arrivals. **The month's close samples it into the 11th int ring** (`+0x20820`,
its slot `+0x20a64`, months `+0x20824`; `0x004c7904`..`0x004c792a`): the count as it stands, its month so far
`+0x20820` never written. **Its readers**: the park status screen (`FUN_004a52a0`, I) letters it as "Total park
visitors" (`0x4a68ca`, `FUN_004c94c0`), as it opens and on its one-second timer (`0x004a48b3`), beside "People in park"
(`FUN_004c9130`); the golden tickets' Local 0 and Local 5 (`FUN_004d4bc0`; Local 5 through `FUN_004c94d0`, the only
reader of the ring: the count less the month that many back); and a challenge's test at `0x005a0557` (the count
against `GoldenTicketLocal`'s `Visitors` times a float of its own, `+0x5b8`; not read further). **Nothing reads
`mMostPaidForTicket`**: the image's every operand `+0x21c40` is the constructor's, the serialiser's two and
`FUN_004c7520`'s.

**Measured** (`lib/ringsweep.py`, `q389/ratio.py`): the shipped `Easymode.TPWI` holds 8 and 25, the 11th ring one month
closed on 0, and its gate month so far is 200, eight fees of 25. Over all 584 park files on hand the 11th ring never
falls, its newest month is at most `mLifetimeVisitors` and its month so far is 0; in all 578 where it stands on the gate
ring's slot, the gate's month so far is `mLifetimeVisitors` less the ring's newest month, times one fee (25 in 561, 20
in 4, no admission since in 13), never above `mMostPaidForTicket`.

**OpenTPW** (Q389). `ParkState.LifetimeVisitors` and `MostPaidForTicket`, the file's at a load (`ParkWorld`) and nought
in a park made with no file; `Take`, the gate fee, raises them as `FUN_004c7520` does; `TurnTheMonth` pushes the count
into `MonthVisitors`; the writer writes all three (`ParkWorld.PutLifetimeVisitors`, `PutMonthRings`); `money` prints
them. The challenge message is counted (`GATE_FEE_CHALLENGE_POST`). Nothing here reads them: the park status screen is
not built (`PARK_STATUS_SCREEN`), nor the golden tickets (Q141). **Confirmed** (`q389/`): the stock park with its fee
written to 26 took 18 fees before its close on tick 1474 (`Park: lifetime visitors 9` .. `26, most paid for a ticket
26`), closed `visitors 26` with the gate's 668 = 200 + 26 x 18, and wrote 26, 26 and the ring [0, 26]; main's build
wrote the file's 8, 25 and [0]. The original given this game's file read 26, 26, [0, 26], closed 26 into its third slot
at once, and its park status screen lettered "Total park visitors 26" (`q389/orig/a-2-status.png`); given main's file,
"8" (`c-2-status.png`).

#### The guests inside and the rides at a close

Read 2026-10-15 (Q390). **The month's close samples two counts into the 10th and 12th int rings**, each a thiscall on
the analyser with one argument, a number of quarter ticks, asked with nought (`PUSH EBP`, `0x004c78b1` and
`0x004c7929`), so counted afresh unless the same quarter tick counted already; each keeps its answer as the park value
does (`FUN_004c7fa0`: stamp `+0x1f0b8`, count `+0x1f0bc`, flag `+0x1f0c0`; `FUN_004c8070`: `+0x1f0c4`, `+0x1f0c8`,
`+0x1f0cc`), and the serialiser writes them.

- **`FUN_004c7fa0`, the guests inside** (`+0x205d0`, its slot `+0x20814`, months `+0x205d4`; `0x004c78c1`..`0x004c78f1`):
  the used thing list (`FUN_00516130`, `FUN_00516160`) walked for things of kind 1 (`+2`, guests) on whose cell
  (`+5` x, `+7` y) `FUN_004fa990` passes, a type 1, 3, 9, 10 or 0 (`FUN_00536310`, `20`, `40`, `50`, `90`): the count
  the gate's close takes (`FUN_004c9130`, `park-gate.md`), the music's crowd (`FUN_004c81e0`) and the strike's look
  (`FUN_00508e70`, asked with 5).
- **`FUN_004c8070`, the rides standing** (`+0x20a70`, slot `+0x20cb4`, months `+0x20a74`; `0x004c7933`..`0x004c7964`):
  the same list walked for things of kind 3 whose item (`FUN_00412e90` on `+0xe`) has `+0x4ac`, `Info.WhichUIType`,
  nought; nothing else of the object is asked, so a ride shut, broken or condemned counts, and so does a placed upgrade
  (the `upgrades` folder's category is 0).

Neither month so far is written. **Readers**: the park status screen's graph (`FUN_004a4b90` and its draw, bit `8` of
`DAT_00753c48` from its fourth series button, `0x4a68bf`) plots the 10th beside the 13th (`+0x20cc0`), the 14th's float
(`+0x20f10`), the 15th (`+0x21160`) and the 16th (`+0x213b0`); its labels are not read. **Nothing reads the 12th**: its
every operand is the constructor's, the close's, the serialiser's and the load factory's (`FUN_005179c0`, which empties
every ring before the read, `FUN_0051ae30`).

**Measured** (`q390/sweep.py`): the shipped `Easymode.TPWI` holds the 10th one month closed on 0 and the 12th on 1, the
Belly Bounce; over all 587 park files on hand both months so far are 0; in all 188 whose park value sums to their 2nd
ring's newest month (untouched since their close) the 12th's newest is the file's kind-3 things of `WhichUIType` 0; the
10th's newest is at most the file's guests in 586 (the exception, `q253/orig/sold-by-the-original.TPWS`, closed on 30
and holds 27 guests).

**OpenTPW** (Q390). `ParkState.MonthGuestsInside` and `MonthRides`, the file's at a load and empty in a park made with
no file; `TurnTheMonth` pushes `ParkPeople.GateGuestCensus` and `ParkState.RidesStanding` (the chain's objects whose
catalogue item is `UiType` 0) after the money rings, around the lifetime visitors, before the park value; the writer
writes both (`PutMonthRings`); `money` prints them. Each count is taken afresh where the original's can be one of the
same quarter tick (said at `RidesStanding`). Nothing here reads them: the park status screen is not built
(`PARK_STATUS_SCREEN`). An upgrade placed in an Instant Action park is not catalogued here (no `Easy_` file), so it is
not counted (none ships placed in Lost Kingdom's file). **Confirmed** (`q390/`): the stock park with a Crazy Ape bought
closed on tick 1470 with `guests inside 24` (the `guests` census 29 a moment on) and `rides 2`, and wrote [0, 24] and
[1, 2]; main's build left the file's [0] and [1] a slot behind the money rings. The original given this game's file read
[0, 24] and [1, 2] and its own close at once pushed 24 and 2, its own counts of the same park (`q390/orig/a-load.log`,
`side-by-side.png`).

#### The arrivals

Read 2026-10-15 (Q391). **The 13th int ring (`+0x20cc0`, its slot `+0x20f04`, months `+0x20cc4`) counts the guests the
arrival vehicles drop off**: the arrival manager `FUN_004cf3e0` (`park.md`, "Arrivals"), while the vehicle answers 2
and some of the load are still to drop, makes one guest (`FUN_004cf720`, its answer not looked at), lowers the count
still to drop and adds one to the month so far (`0x004cf56d`..`0x004cf592`); nothing else adds to it (the image's every
operand `+0x20cc0`: the constructor's nought, the serialiser's, the load factory's, the close's and the graph's). The
month's close pushes the month so far into the ring and zeroes it, first after the seven money rings
(`0x004c788c`..`0x004c78bb`). **The same manager pushes each load's size, as the load is called, into a ring of the
last five loads** (read 2026-10-16, Q396): the analyser's ring shape, `mTemp` `+0x216d8` (nought, never written but by
the constructor and the load factory), the five loads `+0x216dc`, slot `+0x216f0` (-1 before the first), count
`+0x216f4` 5, wrapped `+0x216f8` (`FUN_004c58c0` `0x004c5b20`..`0x004c5b36`; the load factory's `FUN_0051ae90` the same).
The push (`0x004cf41f`..`0x004cf451`) takes the headcount `FUN_004cf5b0` answered, after the offloading flag is set and
before any vehicle is asked: the slot one on, round to nought and wrapped past the fifth, then the size into it. The
constructor fills no load, so a slot no load has reached holds whatever memory held (`0xcdcdcdcd` in the shipped park,
nought in the original's own fresh parks). The serialiser reads and writes it whole (`FUN_004ce660`, the stays' shape,
from `0x004c69e8` and `0x004c70de`). Its one reader is `FUN_005a0780`, a challenge's test in the table at `0x00772088`:
how many loads have been called (five once wrapped, else the slot plus one), and the challenge's `+0x5f4` when that is
exactly one; the challenges are not built. **Readers**: the park status screen's graph plots the 13th as its first series (bit
`1` of `DAT_00753c48`; `FUN_004a51d0`, `FUN_004a5e10` and the draw at `0x004a4750` take its month so far's address,
the ring's head); its labels are not read.

**Measured** (`q391/sweep.py`, `law.py`): the shipped `Easymode.TPWI` holds one month closed on 13, its thirteen guests,
and nothing so far. Over all 589 park files on hand the month so far is never below nought, and the ring stands on the
cash-in ring's slot in 574; the 15 others are this game's own files written past a month before this was kept. The
header's `mNumberOfVisitorsToDate` is no partner of it: the months and the month so far sum to it in 24 of the 42 files
whose ring has moved.

**OpenTPW** (Q391). `ParkState.MonthArrivals`, the file's at a load and empty in a park made with no file;
`ParkState.AddArrival` adds one where `ParkPeople.StepArrivals` drops a guest (`Admit` made one); `TurnTheMonth` closes it
after the sideshow takings; the writer writes it (`PutMonthRings`); `money` prints it as month so far / newest / slot.
Nothing here reads it: the park status screen is not built (`PARK_STATUS_SCREEN`). The ring of five loads is kept (Q396).
**Confirmed** (`q391/`): the stock park's one load of the month, 13 arriving on tick 1264 and all off on 1313, read
`arrivals 13/13 slot 0`, and a file saved then held 13 so far; the close on tick 1470 named `arrivals 13`, and the file
held [13, 13] on the money rings' slot; main's build left [13] a slot behind. The original given this game's file read
[13, 13] and its close at once pushed 0; given the mid-month file, 13 so far, pushed whole by its own close on tick 1383
(`q391/orig/a-load.log`, `b-load.log`, `side-by-side.png`).

**The ring of five loads, measured** (`q396/sweep.py`): in all 608 park files on hand its count is 5 and its `mTemp`
nought. In 584 of the 599 whose ring has not gone round, the loads called sum exactly to the guests the 13th ring has
counted dropped (its months and its month so far); the 15 others are this game's own files, written before it kept the
ring, which held the file's one load while the guests dropped. The nine that have gone round (Alexah's Full Simulation
jungle park) hold 35, 35, 46, 46, 50, the newest the 13th's month so far of 50. **OpenTPW** (Q396):
`ParkState.ArrivalLoads`, the file's at a load (its unreached slots as the file holds them) and five noughts in a park
made with no file; `ParkState.AddArrivalLoad` pushes where `ParkPeople.StepArrivals` calls a load, and the console's
`load` (`ForceArrival`) pushes the same; the writer writes it (`PutMonthRings`); `money` prints it as slot / wrapped /
newest. A load the original calls of nobody (world state 4, or the park full) is not called here, so not pushed
(`ParkPeople.StepArrivals`, its two refusals). Nothing here reads it. **Confirmed** (`q396/`): the stock park read
`loads 0/0 newest 13`; its load called on tick 1264 logged `Park: a load of 13 into the arrival loads' slot 1, 2 called`
before any guest dropped, and files saved then and at the load's all-off on 1313 held [13, 13] and the file's
`0xcdcdcdcd` beyond; main's build left [13] while the guests dropped. The original given the all-off file read [13, 13],
pushed its own load of 12 into slot 2 on tick 1916 before its first drop, the 13th's month so far rising to 12, and then
12 and 11 into slots 3 and 4; given the call file, it dropped the load's 13 and called none (`q396/orig/a2-load.log`,
`b-load.log`).

#### The guests' happiness at a close

Read 2026-10-15 (Q392). **The 14th ring (`+0x20f10`, its slot `+0x21154`, count `+0x21158`, wrapped `+0x2115c`, months
`+0x20f14`) is the analyser's one ring of floats**: the close (`0x004c796d`, after the 12th's push) calls `FUN_004c7bb0`
(a thiscall with one stack argument it never reads, `RET 4`; it takes the thing manager `[0x0080239c]` for itself) and
`FSTP`s its answer into the next slot (`0x004c79a0`); nothing writes the month so far, nought in every file.
`FUN_004c7bb0` answers `0.0` (`[0x0070031c]`) while the park is shut (`FUN_0051a280`, the world's `mParkClosed`
`+0x1da710`); otherwise it walks the used thing list for things of kind 1 (`+2`, guests) on whose cell `FUN_004fa990`
passes, the 10th's walk and test (above), and for each one `__ftol`s the happiness float `+0x19c`, keeps its low byte,
and adds it to a whole-number sum (`FILD`, `FIADD`, `__ftol`, `0x004c7c46`..`0x004c7c67`); the answer is the sum over
the count (`FIDIV`), `0.0` with none. The 10th's `FUN_004c7fa0` can answer its kept count of the same quarter tick, this
one walks every time. **Readers**: the park status screen's graph (above); its labels are not read.

**Measured** (`q392/sweep.py`, `law.py`): the shipped `Easymode.TPWI` holds one month closed on `0.0`, nothing so far.
Over all 593 park files on hand every month is a float from 0 to 255 and the month so far is nought; the ring stands on
the cash-in ring's slot in 576. Of the 270 closed months with guests inside (the 10th above nought), the mean times the
10th's count is a whole number in 243; the 27 others are three months of one Full Simulation park in nine copies
(`1FullSimCheated`, jungle), where the first fits only a count of 52 for the 10th's 49 (the 10th's kept answer, by my
reading; not looked into). Every closed month with nobody inside reads `0.0`.

**OpenTPW** (Q392). `ParkState.MonthHappiness`, the file's at a load and empty in a park made with no file, its floats
kept as their bits (`ParkMonthRing.Push( float )`, `NewestSingle`); `ParkPeople.GuestsInsideHappiness` is the mean
(the guests `GateGuestCensus` counts, nought with the park shut); `TurnTheMonth` pushes it after the rides, before the
park value; the writer writes it (`PutMonthRings`); `money` prints it as newest, slot and now. Nothing here reads it:
the park status screen is not built (`PARK_STATUS_SCREEN`). **Confirmed** (`q392/`): the stock park's close on tick
1470 named `guests inside 16` and `happiness 45.4375` (727 over 16), `money` read the same at slot 1, and the file held
[0.0, 45.4375] on the money rings' slot, its own sixteen guests' mean the same; main's build left [0.0] a slot behind.
The original given this game's file read [0.0, 45.438] and its own close at once pushed 44.875 with 16 inside (718
over 16); given main's file, its close pushed 46.542, the mean of that file's 24 guests to the last place
(`q392/orig/a-load.log`, `b-load.log`, `side-by-side.png`).

#### The leavers' stays

Read 2026-10-15 (Q393). **The 15th int ring (`+0x21160`, its slot `+0x213a4`, count `+0x213a8`, wrapped `+0x213ac`,
months `+0x21164`) is the mean stay of the last fifty guests to go home, in whole park days.** A ring of the same shape
fifty long (`+0x21600`: `mTemp` `+0x21600`, never written, entries `+0x21604`, slot `+0x216cc`, count `+0x216d0` 50 from
the constructor, wrapped `+0x216d4`; serialised by `FUN_004ce2d0`, the int rings' `FUN_004cdf00` at another length)
holds the stays: a guest entering state `0x15`, at the stop to go home, whose `mPaidAdmission` (`+0x204`) is set hands
the analyser their stay (`FUN_00501db0`, `0x00502333`): `FUN_004fd950` (the ticks since `mArrivalDate`, unsigned,
shifted right by two) shifted back left by two (`SHL` at `0x0050234a`, a 32-bit register), and `FUN_004c7600` turns
that into park time with `FUN_004f8730` on the park calendar (times its rate `+0x1c`, over four, times 10,000,000 in 64
bits, which wraps) and `__alldiv`s it by 864,000,000,000, signed: whole days, the low half pushed into the next slot.
Nothing else writes it (the image's every operand `+0x21600` to `+0x216d4`: the constructor's, the serialiser's, the
load factory's empty and the close's read). **The month's close**, after the park value (`0x004c79d7`..`0x004c7a40`):
`FUN_004cef30` on the stays with fifty (`PUSH 0x32`) takes the newest min(50, filled) (filled the count once wrapped,
else slot + 1), newest first, sums them into a double and answers how many it took (`RET 8`, `EAX`); with any, the
close divides (`FDIVR`) and `__ftol`s (toward nought), else nought, and pushes that into the 15th. The stays are left as
they are. The same stretch zeroes `+0x21bfc` and `+0x21c00`, this month's misbehaving kids (below, "The misbehaving kids"). **Readers**: the
park status screen's graph (above, "The guests inside and the rides at a close"); that its series is the one lettered
"Average time in park" is not read from a label.

**Measured** (`q393/sweep.py`, `law.py`): the shipped `Easymode.TPWI` holds the stays empty (slot -1 of 50) and the 15th
one month closed on 0. Over all 596 park files on hand the stays' count is 50 and their `mTemp` 0, the 15th's month so far
0, on the cash-in ring's slot in 576. In all 34 files whose stays hold any and have not wrapped, every closed month of
the 15th is the truncated mean of the stays' oldest k for some k, the leavers before that close; the 553 with none read
0. The 9 wrapped (one Full Simulation jungle park's copies) cannot be checked, the window having moved. Stays seen run
from 18 to 587 days.

**OpenTPW** (Q393). `ParkMonthRing` takes a length (`Stays`, 50); `ParkState.LeaverStays` and `MonthMeanStay`, the file's
at a load and empty in a park made with no file; `PeepBehaviour`, where a guest walking outside reaches the stop, hands
a paying guest's `Peep.StayAt` to `ParkState.AddLeaverStay` (`StayDays`) after the state is set, where the original's
setter does it; `TurnTheMonth` pushes `MeanOfNewest( 50 )` after the park value; the writer writes both
(`PutMonthRings`); `money` prints them. Nothing here reads them: the park status screen is not built
(`PARK_STATUS_SCREEN`). **Confirmed** (`q393/`): in the stock park four guests reached the stop before the close, 125,
193, 194 and 199 fours of sweeps, logged 21, 33, 33 and 34 days; the close on tick 1470 named `mean stay 30` (121 over
4), and the file held the 15th [0, 30] on the money rings' slot and the stays [21, 33, 33, 34]; main's build left [0] a
slot behind and no stays. The original given this game's file read both and its own close at once pushed 30, then its
next leaver handed in 35 days; given main's file, its close pushed 0 (`q393/orig/a-load.log`, `b-load.log`,
`side-by-side.png`).

#### The park score

Read 2026-10-15 (Q394). **The 16th int ring (`+0x213b0`, its slot `+0x215f4`, count `+0x215f8`, wrapped `+0x215fc`,
months `+0x213b4`) is a score of the park out of a hundred**, the last thing the close pushes (`0x004c7a49`, a thiscall on
the analyser, then the ring's step written out to `0x004c7a7a`). `FUN_004cceb0` adds eleven counts, each capped, every
division whole:

| Count | Asked of | Points | Most |
|---|---|---|---|
| the guests inside | kind 1 (`+2`) on a cell `FUN_004fa990` passes, walked twice, 1,000 at most | n x 20 / 1000 | 20 |
| the rides | `FUN_004ca8e0( 0, 0 )`: type 3, item `+0x4ac` (`Info.WhichUIType`) 0 | n x 3 / 2 | 20 |
| the shops | `FUN_004ca8e0( 1, 0 )` | n x 2 | 10 |
| the sideshows | `FUN_004ca8e0( 2, 0 )` | n x 2 | 10 |
| the features | `FUN_004ca8e0( 3, 0 )` | n | 10 |
| the rides upgraded | `FUN_004ca8e0( 0, 2 )`: a ride whose `mUpgradeLevel` byte (`+0x50`) is 2 or over; the level is asked of rides alone | n | 10 |
| the handymen, the mechanics | the thing list walked inline (`FUN_004cd300`, `FUN_00481f80` a staff type), type 5, then 4, `+3` nought | n | 4 each |
| the entertainers, guards, researchers | `FUN_004c9960( 6 )`, `( 7 )`, `( 8 )`: type byte, `+3` nought | n | 4 each |

The fixed items (`WhichUIType` 4) count for nothing. **Readers**: the park status screen's graph (above, "The guests
inside and the rides at a close"); which of its labels names it is not read.

**Measured** (`q394/sweep.py`): the shipped `Easymode.TPWI` holds the 16th one month closed on 18: no guest inside, the
Belly Bounce (1), the Drinks Shop (2), the Jungle Spray (2), eight features (8) and one of each kind of staff (5). Over
all 598 park files on hand its month so far is 0, on the cash-in ring's slot in 576. In 180 of the 197 untouched files
(the park value its file's own, Q388) the newest month is the reckoning from the 10th ring's guests and the file's
things; each of the 17 others differs by the staff hired or gone since its close. Alexah's Full Simulation jungle park
reads [72, 72, 73, 74], its last month 74 where the file's twelve rides reckon 73: its 12th ring says thirteen stood at
that close.

**OpenTPW** (Q394). `ParkScore.Of` counts the objects by their item's `UiType` and level and takes the guests inside and
`ParkPeople.ScoreStaff` (a member in the hand counts; one fired here is gone at once); `ParkState.MonthParkScore`, the
file's at a load and empty in a park made with no file, is pushed last in `TurnTheMonth`; the writer writes it; `money`
prints it with the score now. Nothing here reads it: the park status screen is not built (`PARK_STATUS_SCREEN`).
**Confirmed** (`q394/`): the stock park with the Belly Bounce written to level 2 (`bounce2.TPWS`), loaded; `money` read
`park score 18 slot 0 now 19`; the close on tick 1470 (22 guests inside) named `park score 19`, and the file held the 16th
[18, 19] with every ring on slot 1; main's build left [18] a slot behind. The original given this game's file read
[18, 19] and its own close at once pushed 19; given main's, its close pushed 19 into slot 1 with its 15th on slot 2
(`q394/orig/c-load.log`, `b-load.log`, `side-by-side.png`).

#### The misbehaving kids

Read 2026-10-16 (Q395). **Three counts sit before `mLifetimeVisitors`**: `mMisbehavingKids` (`+0x21bfc`) and
`mMisbehavingKidsThatGotAwayWithIt` (`+0x21c00`), this month's, and `mMisbehavingKidsEver` (`+0x21c04`). Their one adder
is `FUN_004c74f0( caught )`: the first and the third one more, the second one more where `caught` is nought. Its one
caller is the guard's chase, `FUN_004d6790`: the catch ("Guard: Caught a kid", `0x004d691b`) hands 2, every other end
("Guard: abandoned chase", `0x004d6a1f`) hands nought. Nothing else writes them (the image's every operand `+0x21bfc` to
`+0x21c04`): the constructor `FUN_004c58c0` zeroes all three, the close zeroes the month's two (`0x004c79eb`,
`0x004c79f1`, `EBP` nought, after the park value and before the stays' mean), and the serialiser `FUN_004c5d70`.
**The serialiser's read never reads the third**: where it reads the first two with `FUN_004fae10` (the stream's read,
vtable `+8`, "File read failed"), it hands the third to `FUN_004ced70` (`0x004c719a`), the write (vtable `+0xc`, "File
write failed"), so a load writes the analyser's word over the buffer and the stream moves on four: the lifetime
visitors that follow read right. The load factory `FUN_005179c0` allocates the analyser (`0x21c50` bytes) without
zeroing these three, so after a load the third is whatever that memory held: 0 in two loads, -999 in a third. **The one
reader** is `FUN_004c94a0( got away )` (the second if non-nought, else the first), called from five places, all advisor
scores. With nought (this month's): `0x005a04f0`, the polled row 264's (message `0x108`, its first response 514):
`LotsOfPranks.Score` (100) when this month's count is over `LotsOfPranks.MorePrankstersThan` (2), else nought; and
`0x0059cd30`, row 12's, a park with no guard (`advisor-park.md`, "A park with no guard", Q413). With one (this month's
got away): `0x0059d3dd` in row 22's score `0x0059d3b0` (with a guard on the staff, a key's score past a key's count; its
lines 64 to 66 ask for security cameras; `advisor-park.md`, "Kids who got away from a guard", Q423), and `0x0059e853` and `0x0059e871` in row 98's `0x0059e840` (message `0x62`,
`advisor-park.md`, "Kids who got away", Q418). Nothing reads the third.

**Measured** (`q395/sweep.py`): of 608 park files on hand, 594 hold nought, nought and nought; nine hold 0, 0, 3 (Alexah's
Full Simulation jungle park and its copies); one the original wrote holds 1, 1, 1 (`q253/orig/sold-by-the-original.TPWS`,
a chase abandoned that month, counted since its load).

**OpenTPW** (Q395). `ParkWorld.MisbehavingKids` reads the three (record `+73468`), `PutMisbehavingKids` writes them;
`ParkState.MisbehavingKids` takes the file's month two at a load and nought for the third, where the original's is its
load's unzeroed memory (a deviation, said at the site); `TurnTheMonth` zeroes the month's two; `money` and the close's
line print them. Nothing counts into them: the guard's chase waits on the pranks (`DECIDE_PRANK_*`). Row 12 reads this
month's (Q413), and row 264 (Q414; `advisor-park.md`, "Lots of pranks"). With them the close is built whole (`ANALYSER_MONTH_CLOSE` gone).
**Confirmed** (`q395/`): the stock park with the three written 5, 2 and 9 (`kids.TPWS`), loaded; `money` read `misbehaving
kids 5 got away 2 ever 0`; the close on tick 1470 named `misbehaving kids 0 got away 0 ever 0`, the file held 0, 0, 0 and
`unimplemented` no longer listed the close; main's build kept 5, 2, 9 in the file and listed it. The original given
main's file read 5, 2 and 0 (not 9) and its close on tick 1471 zeroed the two on the tick its ring 1 moved; given this
game's, 0, 0, 0 throughout (`q395/orig/c-load.log`, `d-load.log`, `side-by-side.png`).

#### The analyser's other stamps

Read 2026-10-16 (Q397). **Of the four stamps after `mLifetimeVisitors`, one moves.** Every operand `+0x21c10` to
`+0x21c2c` in the image: the constructor `FUN_004c58c0` (all four the calendar's now), the load factory `FUN_005179c0`
(zeroed, then the file's), the serialiser `FUN_004c5d70` (read `FUN_00510240`, write `FUN_004cede0`, eight bytes
each), and `FUN_004c7370`, the analyser's message handler (the dispatcher `FUN_0050b550`, model 13). Nothing reads
any of the four. In that handler:

- **Message `0x13` carrying 3 stamps `mParkLastOpened` (`+0x21c28`)** with the calendar's now (`FUN_004f8690` on world
  `+0x2a0`, `0x004c7444`..`0x004c747b`). The park door `FUN_00519ef0` posts it from its open arm (`0x0051a031`)
  **whether or not the park was shut**: an open request to an open park jumps straight to the post (`0x00519f7c`).
  The close arm posts 4 (`0x0051a1c1`), which the handler ignores; the end of the park (second argument non-nought)
  posts nothing. The analyser joins set `0x13` in its constructor (`FUN_004c7a90`, with `0xc` and `0x11`), and set
  `0x13` is things 3, 4, 5, 8 and 9 in the shipped park, the advisor first.
- **Message `0x17` carrying 2 would stamp `mLastStrike` (`+0x21c10`), and never does**: the strike's message
  (`FUN_00508f70`) goes to set `0x17`, which the advisor joins (`0x00599f08`) and the analyser does not. A load takes
  the sets from the file (`FUN_0040fd60`), and set `0x17` is the advisor alone in all 612 park files on hand
  (`q397/sweep.py`, `parkfile.sets`). Dead by CODE in the original.
- **`mLastPuke` (`+0x21c18`) and `mLastPrank` (`+0x21c20`) have no case**: only the constructor writes them.

**Measured** (`q397/sweep.py`): in all 612 park files the strike, puke and prank stamps are the calendar's start;
`mParkLastOpened` is later than it in 609 (`Easymode.TPWI`'s is tick 15).

**OpenTPW** (Q397). `ParkWorld.ParkLastOpened` reads it (record `+73508`), `PutParkLastOpened` writes it;
`ParkState.ParkLastOpened` takes the file's at a load and the calendar's start with no file; `SetParkClosed( false )`
stamps the calendar's now every time, the park open or not, and logs `Park: mParkLastOpened stamped <date> on mGameTick
N`. The other three are not kept: the file's go out as they came in, as the original's would.
**Confirmed** (`q397/`): the stock park paused on tick 763, the door clicked shut (no line) and open: `Park:
mParkLastOpened stamped 3/2/2000 02:47:30 on mGameTick 763` (`run-fix1/1-opened.png`); the file held tick 763's, the
three others the calendar's start, and held it again loaded from the list and saved; main's build left tick 15's. The
original given that file read tick 763's, kept it as its door shut on tick 896, and stamped tick 917's exactly as it
opened (`q397/orig/a-load.log`, `a-o-3-opened.png`).

#### The first row is picked as a list fills

Read 2026-10-14 (Q356). **The list's own add picks it.** `FUN_0066403b`, once its count reaches 1, calls
`FUN_0066525c( 0 )` (`0x006643d4`), which selects slot 0 plus the top row (`+0x14c`) and, where that moves the
selection, hands `0x401` with the list's id and the selected INDEX to `FUN_00658ccc`, which QUEUES it (`FUN_006697e3`, a
ring of four-word messages): posted, not sent. The clear `FUN_006649d5` sets `+0x14c` to -1 and the top row to 0 and
posts nothing, so every fill after a clear picks again. `FUN_0049b5b0` (called by the opener `FUN_0049bdd0` and by the
tab handler `FUN_0049bc80`, which does nothing else but set the title) clears the list and adds the tab's candidates in
slot order through the sorted insert, and the selection stays an index, so when the screen's handler `FUN_0049b650`
takes the message (`0x0049b869`) the row at index 0 is the one sorted FIRST, read back by the index (`FUN_00664c71`,
`0x0049b894`): the skill bar, Staff costs (`DAT_007ca2fc` plus that wage) and the balance are that row's, and the
highlight is on it. Where the index names no row `FUN_00664c71` answers 0 and writes nothing, and the handler goes on
with a stale word. The row id pushed is the candidate's SLOT INDEX (0 to 0x1f), which is what `FUN_00507bd0` takes
back: the pool `[0x007cf83c]`, 32 records of 0x14 bytes, `+0` the kind, `+4` the name's row (`FUN_005074a0`), `+8` the
grade, `+10` in use.

**The label `0x2483` reads UITEXT 146, "Skill"**, put on once by the builder (`FUN_00485b00( 0x92 )`, `0x0049c2c8`, the
only `PUSH 0x2483`); nothing letters a candidate there.

**Measured in the original** (Q356, `q356/orig/`, `q353/grades.TPWS`, the pool read by `staffpool.py`): opened on the
janitors on tick 1106 with slots 0 (grade 1, $ 36), 11, 18 and 19 (grade 3): Chris Battson (slot 11) stood first and
was highlighted, the bar at 0.617 and Staff costs $ 63, not those of slot 0, handed over first and standing second. On
the mechanics' tab Alex Cullum stood first and was highlighted, $ 92, the bar 0.209 (also the first handed over). Q353's
`a-tab.png` highlights its fifth row because `gmove.py` glides diagonally, and its way down from the tab crossed rows 0
to 4 ("The pointer over a list"). The original's pool at the load was this game's exactly; slot 7 timed out on tick 1066
and the top-up on tick 1083 added ten, which is why Q353's original, clicked after it, showed five janitors to this
game's two.

**OpenTPW** (Q356): `UiList.PicksFirstRow` builds the add's pick, the top row selected as the first row goes in and its
index posted; `TakePosted` hands the row standing at that index to `SelectionChanged` when the screen's `Update` calls
it. The hire, all-items and all-staff (Q382) lists set it; the other lists count the pick (`LIST_FIRST_ROW_SELECTED`, Q232). The picked wage stays in
Staff costs until the next pick. On the same file (`q356/run-fix2`) the opening logged `Hire list: added 7,11, row 11
picked` and `Hire screen: candidate 11 grade 3, skill bar 614 of 1024, staff costs 502`, the bar 0.606 with Chris
Battson's row highlighted, and the mechanics' tab `added 3,8,10, row 8 picked`, candidate 8 grade 2, 409, staff costs
554 (Karl Fitzhugh's $ 115 for Chris's $ 63).

#### A tab put down by the opener fills nothing

Read and measured 2026-10-14 (Q364). Each tabbed screen's opener fills its list itself and then puts its tab down with
`FUN_0066a295` (hire `FUN_0049bdd0`, buy `FUN_004acc70`, all items `FUN_00495aa0`, all staff `FUN_00496620`). That
call lifts the old button and puts the new one down through `Button_SetDown( down, 1 )`, which posts the button only
message 1 (`FUN_00658ccc`); the tab handlers fill on `0x101` with a fourth word of 1 (`FUN_0049bc80`, `FUN_004aca60`),
which a click sends. **Measured in the original** (`q364/orig/a`, `brk.sh` on the hire fill `0x0049b5b0` and the buy
fill `0x004aaf70`, `q353/grades.TPWS`): the gadget's Buy one buy fill (return `0x004ad15d`), b_allstaff one hire fill
(`0x0049c20d`), the mechanics' tab one (`0x0049bcd6`), b_allthings one buy fill: four hits for four steps. A click on the tab
already down fills nothing (`q364/orig/b`): the janitors' tab as the screen opened, then the mechanics' tab a second
time, no hit either time.

**OpenTPW** (Q364): `UiRadioGroup.Put` is `FUN_0066a295`, a button put down with no handler told; `Select` is a click,
`Put` and then the handler. The hire, buy, all-items and all-staff screens' fills call `Put`, so each fills once as it
opens and once a tab, where `Select` there had the handler fill again inside the fill: twice on every opening. On the
same file and route (`q364/run-fix1`) the log held one fill line a step, `Hire list: added 7,11, row 11 picked` once
where main's build logged it twice (`run-control1`), and the all-staff and all-items screens one `filling` line on
opening and one on a tab.

**The all-staff and all-items screens, read and measured 2026-10-14 (Q365).** Their tab handlers fill on `0x101` with a
fourth word of 1 alone, as the hire and buy ones: `FUN_004960a0` (each tab one `FUN_00481bc0` walk with the row adder
`0x00496320`, either inline or through `FUN_00496400`, then the label `FUN_00496570` and the kind's meter
`FUN_004964d0`) and `FUN_00495360` (each tab one sub-builder: rides `FUN_00494ec0`, shops `FUN_00494c70`, sideshows
`FUN_00494250`, misc `FUN_00495110`). The all-items opener puts its tab down before it even installs its handler; the
all-staff opener installs its handler first, and the put-down reaches it as message 1 all the same. In the original
(`q365/orig/a`, `q353/grades.TPWS`, `brk.sh` on the four `CALL`s `0x00496870`, `0x004961cf`, `0x00495b4d`,
`0x004953f0`): **S** opened all-staff with one hit at its opener's fill (through the `allstaff` stub `0x0040c52a`), the
mechanics' tab one, the same tab again none; **A** opened all-items with one hit at its rides fill (the `allitems` stub
`0x0040c51a`, from the screen's own handler running the shortcuts alone, `0x00488c1e`), the shops' tab one, the same
tab again none (`q365/orig/a-six.png`). This game's fills were already these.

**The shortcuts' screen rows** (Q366; their keys and what each opener does first are in `park-engine.md`, "The park
management gadget"). Measured in the original under Proton (`q366/orig/a`..`c`, `q353/grades.TPWS`, Instant Action), a
frame a key: A all items, and the Information button after it all items again; S, B, H, V, T and G all staff, buy, hire,
all visitors, Staff Training Budgets and the entry price, each in the last one's place; Space the map, and A over the map
nothing; I park status, F finances; L nothing, and the Money button after F and L finances; Ctrl+S and Ctrl+V a locator
panel at the gadget's foot. Ctrl+P was not pressed (the postcard writes a picture out).

**OpenTPW** (Q366): `ParkFrontEnd.ShortcutRow` is the table, a key let go matched on its key and its modifiers alike, and
`ShortcutKey` runs rows 1 to 15 where the original's table is heard: over the park, and under a park screen; not under
the game menu, a message box, the options screen, the map or the full-screen view; in first person the postcard's row
alone. A screen row calls `ParkCategoryScreens.Show`, the opener the category buttons reach, so the category is left on
it; Space opens the map as its button does; R runs the research opener ("The research opener and its two boxes").
Park status, finances and the postcard are counted (`PARK_STATUS_SCREEN`, `FINANCE_SCREEN`, `POSTCARD_SHORTCUT`), and
the two locators open ("The staff and visitor locators", Q424), loans as `LOANS_SCREEN` outside Instant Action and not at all inside it; G with the entry price up
leaves it as it is. The orbit camera's W and D still scroll it, which the original's does not (its camera table binds the
arrows and the keypad); A and S scroll it no longer. On the stock park (`q366/run-fix2`, the final game binary): each
key's `Park keys: shortcut row N, name` line and the screen `windows` named, a frame a screen (`run-fix2/1-items-by-A.png`
to `9-map.png`); A held a second left the camera's point where it was, W held a second moved it 200 units.

#### The staff and visitor locators

Read 2026-10-16 (Q424; the fork review 2's lead L1, `verify-ui-hud.md`, checked again in the private Ghidra). **Built
once, carried by the arm.** `FUN_004b3cf0` builds both at the interface's assembly, hidden: the stream `0x00756950`
holds two panels with the handler `FUN_004b3790`, `0x95c` (474,1159)-(941,1470) for the staff and `0x963`
(-37,1159)-(429,1470) for the visitors, each a type-8 group `0x95d` framed in `!f_plain` whose buttons (type 2, flags
`0x11`) are `0x95e` `b_shandy` (help 482), `0x95f` `b_smech` (483), `0x960` `b_sresrhcer` (486, the node `b_sresrcher`),
`0x961` `b_senter` (484), `0x962` `b_sguard` (485) and `0x964` `b_skids` (489); the group's handler `LAB_004b3b90`
puts a clicked button down whether the click put it up or down, and posts the panel `0x101` with it and 1. The stream
`0x00756ad8` is the strip `0x146` (448,964)-(1074,1066), `!frame`, help 552, its handler `FUN_004b3c10`: the name
`0x147` (font slot 11, white), the status `0x148` (`i_staffstat`), the hover `0x149` (`hilight`, shown on `0x10001`,
hidden on `0x10002`) and the thought `0x14a` (`thoughts`). The panels go on the arm through `FUN_004a2590`, so each is
put where the arm's handler puts any panel ("The arm's handler"): its left the arm's left less its width and fifty,
then ten steps of `(width + 208) / 10`, 67, so 670 out; its middle on the arm's. The staff's stands at (484,1127), the
visitors' at (485,1127).

**The keys.** Ctrl+S is `FUN_004b3f60( [0x007c2658] )` with a member's window up (`[0x007cc3c0]`), else
`FUN_004b3f60( 0 )`; Ctrl+V the same with a visitor's (`[0x007cc418]`), else `FUN_004b4280`. `FUN_004b3f60( 0 )` sets
the locator `[0x007cc2f0]` to 1 FIRST, then hands the arm the staff's panel, then noughts the person `[0x007cc308]`
and the role `[0x007cc30c]`, lifts every button (`FUN_0066a295( -1 )`), shows the status and hides the thought, posts
advisor message `0xc6`, closes the open park screen (`FUN_00485b40`) and puts the idle mode over the camcorder's pick
mode. `FUN_004b4280` hands the arm the visitors' panel FIRST, then sets role 6 and the locator 2, lifts the button,
noughts the person, hides the status and shows the thought, closes the park screen and posts `0x11f`. Given a person
(a window up), `FUN_004b3f60` picks the role and button by its type byte (5 handyman 1 `0x95e`, 4 mechanic 2 `0x95f`, 7
guard 3 `0x962`, 6 entertainer 4 `0x961`, 8 researcher 5 `0x960`, 1 visitor 6 `0x964`) and shows the strip on them.

**The arm folds a locator closed.** The arm takes a panel by folding what it carries, which sends that panel
`0x1001d8`, and the panel's answer is the locator closed: nobody, the strip hidden, `[0x007cc2f0]` nought,
`gui_CameraFlags` bit `0x1000` cleared, advisor messages `0x11f` and `0xc6` withdrawn. Because Ctrl+S sets its locator
before the fold, **Ctrl+S over a locator already out leaves the panel out and closed**: its buttons go down and find
nobody until Ctrl+S or Ctrl+V again. Ctrl+V sets its own after and is not caught. Escape asks `FUN_004816b0` first
(the game table's row 0, `0x0040c1a0`): with a locator open it folds the arm (`FUN_004a25f0( 0 )`, the camcorder button
left as it is) and is spent, so no menu opens.

**A button** (the panel's `0x101`, nothing with no locator open): from role 0 it takes up its role and the role's first
(`FUN_004b3340`); another role's drops the person and takes its first; the same role's steps to the next, round to
the first after the last (`FUN_004824d0`: the five staff lists from world `+0x1da73c` to `+0x1da744`, each member's next
at `+0x210`, newest first; the visitors the used-thing list from its head, newest first, type byte 1). Somebody found:
the strip shown if it was not, lettered (`FUN_004b3490`: a member's name `FUN_00506860`, a visitor UITEXT 456 "Visitor
#{P24}" with `+0x1d8`, or 457 "New Visitor" at nought), the camera put on them, bit `0x1000` set. Nobody: from role 0
nothing more; else the strip hidden and the bit cleared. **Each frame** (`0x1e`) with the bit set, `FUN_004b3640` puts the
camera's look-at where the person is drawn (`FUN_00486860`: `FUN_004f9f00` blends `+0x190`/`+0x194` towards the
navigator's place at the frame's fraction of the sweep, held to 0..1, truncates, and scales 10/65536; `FUN_0042aab0`
writes it, a snap) and draws a fenced member's patrol edges ("The patrol edges" below); then
`FUN_004b3580` sets the corner: a member's status `FUN_00485ec0( FUN_005064f0() )` (groups 0 and 1 picture 0, 2 1, 3 2,
4 3, picked up 9, no part), a visitor's last thought less one, `0x29a` with none. The camera's mode change clears the bit
(`0x0042ae52`). The strip's left click opens the person's window (`FUN_004b6080`, `FUN_004b79b0`).

**The original** (`q420/orig/run.sh`, `q325/stock.TPWS` off-screen; predicted first, six of six, `q420/PREDICTION.txt`):
Ctrl+S read locator 1, role 0, nobody, bit clear, the panel on the arm (`orig/a-1-ctrl-s.png`); the mechanics' button,
role 2, a person of type 4, the bit set, the look-at (532.19, 212.05), his `+0x190`/`+0x194` times 10/65536 to the
hundredth, the strip "Mike Cooper" with his status, him mid-frame (`orig/a-2-mechanic.png`); again the same person;
Ctrl+S again, the locator 0 with the panel out, the button then nobody; Ctrl+V locator 2, role 6, its button a person of
type 1 whose `+0x1d8` read 13, "Visitor #13" with no thought (`orig/a-6-visitor.png`); Escape the locator 0, the arm in,
no menu. The advisor came up as the staff's opened (`0xc6`).

**OpenTPW** (Q424): `ParkLocator`, carried by `ParkGadget`'s arm (`CarryLocatorPanel`, `TravelFor`, `PlaceFor`,
`FoldLocator`), opened by `ParkFrontEnd`'s rows 13 and 14 and folded by Escape; `Staff.StateGroup`; the console's
`locator`. On the stock park (`q420/run-fix3`, the final game binary; seven of seven): Ctrl+S `locator: open=Staff
role=0 actor=0 following=0 strip=0 ... staffPanel=1@484,1127 down=none` (`1-ctrl-s.png`); the mechanics' button `role=2
actor=26 following=1 strip=1 name="Mike Cooper" status=0`, the camera on him to a fraction of a unit, paused
(`2-mechanic.png`), and 3 s on; Ctrl+S again `open=None` with the panel out, the button then `actor=0`, "with no locator
open" (`4-closed.png`); Ctrl+V `visitorPanel=1@485,1127`, its button `actor=42 name="Visitor #13" thought=666`
(`5-visitor.png`); Escape "Escape: the locator folded away", no menu (`6-escape.png`). Side by side with the original's
(`q420/side-by-side.png`) the panel, the buttons and the strip stand alike. Main's build counted `STAFF_LOCATOR` and
`VISITOR_LOCATOR` once each and put nothing out. The strip's click opens the person's window, a visitor's (Q430)
or a member's (Q431), and Ctrl+S or Ctrl+V with one up opens on that person ("The person's windows"). **Deviations, said at the site:** the
arm slides as the original's (Q427); a person gone from the
park is let go of, where the original keeps the freed thing's address. Not looked into: the bit `0x1000`'s other
readers. In no game run: a kind with nobody (tested only), a fenced member, a person leaving the park while followed
(tested only), the strip's hover and click, first person over a follow, Full Simulation (not asked: the locators are
either mode's), the other three themes.

**The advisor's two lines** (Q425). `FUN_004b3f60` posts its message after `FUN_004a2590` has folded what the arm
carried and before it closes the park screen: `0x11f` where it was given a visitor (role 6), else `0xc6`.
`FUN_004b4280` posts `0x11f` last, after the park screen is closed. The fold's `0x1001d8` withdraws `0x11f` and then
`0xc6` (`FUN_00486b40`), whichever locator it was, and the destroy (`0x14`) withdraws nothing. So Ctrl+S over its own
locator cuts its line and posts it again. Both rows are posted-only, one line, the Tutorial category, and have no tag
(group `0x17f`): `0xc6` is response 417, sample 389 ("those buttons on the control panel arm let you locate your
employees..."), and `0x11f` is response 547, sample 564 ("you can locate visitors by clicking the button on the control
panel arm..."). The original (`q425/orig/run.sh`, `q325/stock.TPWS`, `advisor.py` on both; five of five predicted,
`q425/PREDICTION.txt`): after Ctrl+S, `0xc6` took a slot on 863 and was said on 864, its line 0, for 19998 ms. Ctrl+V
3 s in left `0xc6` at said 0, slaps 1, and `0x11f` was said on 879. Ctrl+V again after the line took no slot (said
once). Said again on 972, cut to slaps 2 by a second Ctrl+S and said on 986, then cut to slaps 3 by Ctrl+V: the next
Ctrl+S took no slot. **OpenTPW** (`ParkLocator.OpenStaff`, `OpenVisitors`, `Folded`; `ParkAdvisorMessages.StaffLocator`,
`VisitorLocator`): on the stock park (`q425/run-fix6`, the final game binary): Ctrl+S `post message=198 ... tick=782
result=queued`, `attempt=1 message=198 ... response=417 sample=389` (`1-staff-line.png`). Ctrl+V 2 s in gave `withdraw
message=287 cleared=0 cut=0`, then `withdraw message=198 cleared=0 cut=1 slaps=1`, then `post message=287`, and sample 564
at once (`2-visitor-line.png`). After the line, Escape and Ctrl+V gave `result=said-once` (`3-said-once.png`). The third
cut gave `slaps=3`, then `post message=198 ... result=slapped` (`5-slapped.png`). Main's build counted
`LOCATOR_ADVISOR_LINE` once. Side by side, `q425/side-by-side.png`. Ctrl+S or Ctrl+V with a person's window up is in no
game run: no person's window is built.

**The patrol edges** (Q426). `FUN_004b3640`, with the bit set and the person of type 4 to 8, asks `FUN_00506eb0` whether
the member's patrol area is the whole map (`+0x20a` 1 and `+0x20c` `0x4000`: none drawn); otherwise it reads the two
cells (`FUN_00506e70`) and walks four lines of `FUN_00536100( 0x136, ... )`: the top row from the left corner's right
neighbour to the right corner's left (both corners where the area is one cell wide), the right column from the top
down, the bottom row as the top, and the left column from the bottom up. `FUN_00536100` walks the longer way, a tie
along Y, holding the other coordinate at the start's, and passes over an off-map cell (`FUN_004d8300`). So a row two
cells wide has no middle: its line runs from the right corner back to the left, and both corners are laid twice. Op
`0x136` is `0x100` (lay a square) with mode `0x36`, the patrol tool's own (`FUN_005234d0` draws the tool's rectangle
with the same four lines and latches its refusal). `FUN_00532fc0`'s `0x100` arm asks `FUN_00535670( 0x36 )`, which
answers the refusal latch `DAT_00816d50` and nothing else, and reads the placer's failure `DAT_0081b604` before it;
both are left by a tool's own pass (`FUN_005234d0` clears the latch first), so with the tools put away the square is
texture 0, blue, lifted over a built cell and walled as any tool's ("Placement feedback"). Every frame, since the marker
draw empties its list. **The original** (`q426/orig/run.sh`, `q325/stock.TPWS`, `original/squares.py`; four of four
predicted, `q426/PREDICTION.txt`): the mechanics' button, texture 0, 48 squares on the 48 cells of (39,21)-(56,28),
none twice, at height 1.50 (no lift), in the order above (`a-1-mechanic.png`); the scientists' (the whole map) none;
the entertainers' (47,18)-(48,25) 20 squares on 16 cells, (47,18), (47,25), (48,18) and (48,25) twice, in the same
order; Escape none. **OpenTPW** (`ParkLocator.PatrolEdgeCells`, queued each frame to `ParkBuildMarkers.QueuePatrolEdges`,
which lays them as the tools' blue squares and keeps none past its frame): on the stock park (`q426/run-fix2`, the final
game binary), the mechanics' button logged `Park locator: thing 26 patrols (39, 21)-(56, 28), its edges 48 squares a
frame`, `locator` ended `patrol=48`, and `strip`'s drawn census held `| patrol 48` in that order, each marker 0, lift 0
(`1-mechanic.png`, beside the original's in `q426/side-by-side.png`); the scientists' `patrol=0`; the entertainers'
20 on 16 cells, (48,18), (47,18), (48,18) first (`3-entertainer.png`); Escape none. Main's build counted
`LOCATOR_PATROL_EDGES`. In no game run: a square lifted over a built cell on an edge (none of the stock areas crosses
one; the lift is the tools', tested), a fence drawn in play (the patrol tool, built: "The patrol switch and the patrol tool" below), Full Simulation (not
asked: the locators are either mode's), the other three themes.

#### The person's windows: the visitor's and the staff member's (Q429, Q430, Q431)

Read 2026-10-16 (Q429, decode only; nothing is built). Two of the object windows' nine (`park-engine.md`, "The
per-object management screen is nine screens"), both on the shared base `FUN_0048cea0( stream, handler, preview,
stats, 0 )`, each a park screen (`FUN_00485b70`: the arm folds, so a locator out is put away; the camera and game
tables go off). **Who opens them**: the world click on a guest or a member (`FUN_004879d0`, categories 7 and 8 to `c`),
the locator strip's left click (`FUN_004b3c10`, `0x10006` button 0: the visitor's for any type but 4 to 8), the
object dispatcher `FUN_00486920`, and a row's `0x400` in the visitors list (`0x0049351e`), the all-staff list
(`0x00496088`) and the staff room's (`0x004b5267`). The windows are statics: the visitor's at `0x007cc408` (its tree
`[0x007cc418]`, which Ctrl+V tests), vtable `0x006fff40`; the staff member's at `0x007cc3b0` (tree `[0x007cc3c0]`), vtable
`0x006ffe50`. The person is `[0x007c2658]`, set by `FUN_0048ca00`, which also counts the things of the window's class
(visitor 1, staff `0x40`).

**The visitor's** (`FUN_004b79b0`; stream `0x007575e0`, handler `FUN_004b7720`). The builder posts advisor message
`0x12e` and plays `cat_kids` effect `0x81`; the destroy (`0x14`) withdraws `0x12e`. The stream (`lib/uistream.py`;
meshes by `meshhash.py`, help rows UIHELPTEXT), 2048 x 1536:

| Id | Rect | What |
|---|---|---|
| `0x371f` | (328,130)-(1720,901) | the root; `window1` in `w_small` |
| `0x3736` | (749,174)-(1221,253) | the title, "Visitor #{P24}" (UITEXT 456, `+0x1d8`) or "New Visitor" (457) at nought; not editable |
| `0x3722` | (456,267)-(720,531) | the preview, `!frame` |
| `0x3723` | (745,265)-(1521,709) | the stats, `!frame`: two columns of eight rows, the labels at x 778 and the figures at x 1170 |
| `0x3734`, `0x3735` | (641,720)-(1352,856), (676,739)-(780,819) | the thought: `Ethoughts3` in `ethoughts` with its text rect (817,760)-(1314,805), help 95; inside it a type-6 icon wearing `thoughts.MD2` |
| `0x3720`, `0x3721` | (1591,234), (1580,322) | `b_arup` (help 98, the previous visitor), `b_ardown` (97, the next) |
| `0x3737`, `0x3738` | (1568,411), (1568,518) | `b_locate` (99: `FUN_004b3f60( person )`, the visitors' locator on them), `b_kids` (100: the visitors list, `FUN_00493530`) |
| `-2` | (1591,714) | `b_okay` (1): the close |

The stats fill (vtable `+0xc`, `FUN_004b6c20`) letters the labels in font 6: UITEXT 70 Cash remaining, 71 Time in park,
72 Rides ridden, 75 Purchases made, 73 Sideshows played, 74 Sideshows won, 76 Happiness, 77 Current status. The figures
(the refresh, vtable `+0x14`, `FUN_004b72a0`, at the show and on the timer `0x80080` every 4,000 ms) are the visitors
list's cells ("The visitors rows", below): a money cell of `+0x1a0`; the time cell (`FUN_00490e60`) of **four times the
stay** (`FUN_004fd950` shifted left by two before `FUN_004f8730`, as the list's rewrite); numbers of `+0x1c4`, `+0x1c8`,
`+0x1cc` and `+0x1d0`; a `happygrad.wct` bar inset 10 of `((__ftol( +0x19c ) & 0xff) << 10) / 100`; and the status, row
`+0x220` of `KIDSTATES.str` (`[0x0078baa0]`, table `0x3ec`, `mUIKidStates`), row 16 "Using: {P0}" with the name of the
thing whose id is `+0x1dc` (`FUN_004e2780`). The thought is part `+0x30` less one, or 999 at nought (none shown), and row
`+0x30` of `THOUGHTS.str` (`[0x0078ba9c]`). The title is set at the show (vtable `+4`, `0x004b7660`) and again by the
refresh while the window's kept number `this[9]` is nought, so a new visitor's title turns to their number once given.
The preview (vtable `+0x18`, `0x004b6b90`) is `FUN_00486530`: a copy of the person's model (`FUN_00475cf0` on `+0xc`) set
in the panel's rect, a fourth word 12.0 kept at the copy's `+0xa4` (not followed), copied from the live model again
on every `0x1e` (`FUN_00486610`). Its click goes to the person and closes the window (the base's `0x0048d1a0`).

**The staff member's** (`FUN_004b6080`; stream `0x00756f48`, handler `FUN_004b5cb0`). The builder posts `0xc0` and
gives the preview `FUN_004b5c80` (the camera to the member, the window kept); the destroy withdraws `0xc0` and `0xd4`.

| Id | Rect | What |
|---|---|---|
| `0x50a` | (328,130)-(1720,901) | the root, `window1` |
| `0x520` | (749,174)-(1221,253) | the title, the member's name (`FUN_00506860`); help 85, a click to edit it (`FUN_0048d290`, vtable `+0x30` sets the name, `+0x34` reads it) |
| `0x511` | (386,280)-(650,544) | the preview |
| `0x512` | (665,280)-(1538,623) | the stats: six rows, labels at x 688, figures at x 1193 |
| `0x50c`, `0x50e`, `0x510` | (380,694), (539,694), (1401,694) | `b_fire` (89), `b_pick` (88), `b_route` (86, a toggle, flags `0x11`, second help 87) |
| `0x50d`, `0x50f` | (1591,234), (1580,322) | `b_arup` (92), `b_ardown` (91) |
| `0x521`, `0x50b` | (1568,411), (1568,518) | `b_locate` (93), `b_allstaff` (94: the all-staff list on the member's kind, `FUN_00496620( FUN_00506490() )`) |
| `-1` | (1591,714) | `b_okay` |

The labels (vtable `+0xc`, `FUN_004b53c0`): UITEXT 64 Employed for, 65 Monthly wage, 67 Skill, 69 Happiness, 68 Energy,
66 Current status. The refresh (`FUN_004b58b0`): "{P8} days" (UITEXT 443, the plural at any count) of `FUN_00505b70`, the
days since `mTimeHired` (`saves.md`); a money cell of the wage `FUN_00506870`; a `ridestatbar.wct` bar of the skill
`FUN_00506550` (`<< 10) / 100`); a `happygrad.wct` bar of `+0x1f8`; a `ridestatbar.wct` bar of `+0x1fc`, the rest (500
until the first refresh); the status picture `0x51f` (`staffstat`, `FUN_00485ec0`) and the `STAFFSTATES` word
(`FUN_00485f00`, `[0x0078baa4]`) of `FUN_005064f0`, its 1 read as 0 where the member has no patrol area
(`FUN_00506eb0`). The show (vtable `+4`, `FUN_004b5ab0`): the title, the preview (scale word 8.0), the refresh, the patrol
toggle down with advisor message `0xd4` posted where an area is set, and PICK UP enabled by `FUN_00505bd0` (again every
`0x1e`). The open (vtable `+0`, `0x004b5a60`) hides both arrows (`UI_SetVisible( 0 )`) with fewer than two staff. **The verbs**: FIRE is the
dismissal `FUN_00505790`, behind a message box on UITEXT `0x18d` when `[0x0078d90f]` is set (the box's yes,
`FUN_004b6070`, dismisses), then the window closes; PICK UP keeps the member (`[0x007c2650]`), picks them up
(`FUN_00505c50`) and closes; the patrol toggle down (`FUN_004b5b50( 1 )`) keeps the member, puts map tool `0x36` in the
hand, closes the window and posts `0xc1`; up clears the area (`FUN_00506e90`).

**The advisor's four lines** (the metadata table, `q177`'s `meta_full.json`, rows of 56 bytes from `0x0076e300`): `0x12e`,
`0xc0`, `0xc1` and `0xd4` are each posted-only, one line, the Tutorial category, no tag, responses 562, 411, 412 and
431, samples 580 (the visitor's screen and the buttons on its right edge), 384 (the employee's screen, its three verbs,
the name's edit and the buttons on the right edge), 385 (how to draw a patrol area) and 457 (how to cancel one), by
their transcripts in `global-speech-transcripts.tsv`.

**The original** (`q429/orig/run.sh`, `q325/stock.TPWS` off-screen; predicted first, twelve of twelve,
`q429/PREDICTION.txt`): Ctrl+V, the visitors' button, the strip: `[0x007cc418]` set, the locator put away, "Visitor
#13" ($ 684, "11 days" at tick ~917 and "12 days" after a refresh, where the stay alone reads "2 day"; four noughts; the
bar 0.567 for 56.0; "Lining up for a ride", row 11 of the reference install's `american` table), no thought, the
visitor's model in the preview (`orig/a-1-visitor.png`, `a-1b-visitor-4s.png`); `0x12e` said on 918 and cut by OK on 962.
Ctrl+S, the mechanics' button, the strip: "Mike Cooper", "45 days", "$ 161", bars 0.61, 0.841 (`+0x1f8` 84.97) and 0.652
(`+0x1fc` 65.3), "Patrolling" (`a-3b-staff-4s.png`); `0xd4` said first, on 1041, `0xc0` waiting behind it, both gone at
OK. Not run: a thought shown, the verbs, the arrows, the name's edit, a visitor's or member's window from the world click
or a list.

**Read for the build** (Q430). The open (vtable `+0`, `FUN_004b75a0`) keeps the person (`FUN_0048ca00`) and letters the
thought panel `0x3734` in font 6, black, centred both ways; its words are `FUN_006ace60( +0x30 )` through
`FUN_00485ac0`, row 0 (none) an empty row. **The arrows** walk mask `0x40`, kind byte 1: `b_arup` is `FUN_00482da0`,
the used-thing list's previous link (`FUN_005161b0`, the entry's `+0xc`) from the person, so towards the newest, and
with none round from the list's tail (`FUN_004831f0`), the oldest; `b_ardown` is `FUN_004824d0`, the next link, towards
the oldest, round from the head. **`b_kids`** hands the visitors list the person: `FUN_00664d2a` finds the row,
`FUN_00665739` picks it and, where it is out of view, makes it the top, held so the view ends at the last row; the
list's opener also posts advisor message `0x11e`. **The status's row 16** names the thing as `FUN_004e2780` joins its
two name lines (`+0x3c8`, `+0x40a`): the second alone where the first is empty, the first less its last character and
the second straight after where the first ends in `-` (`"%s%s"`, `0x00749850`), the first alone where the second is
empty, else `"%s %s"` (`0x0075c0c0`). Nothing clears `[0x007c2658]` as a guest goes (only the delete and move verbs,
`FUN_0048cd10`, `FUN_0048cfa0`, write nought), so what the window does then is not decoded.

**The original** again (`q430/orig/run.sh`, the stock park; `q430/PREDICTION.txt` round 1): the window on $684 #13;
`b_ardown` $510 (stamp 659), `b_arup` $684, again $570 stamp 648, the oldest (no guest made since the load);
`b_locate` and Ctrl+V over the window: the window gone, `[0x007cc2f0]` 2 on the guest, role 6, `0x11f` posted; `b_kids`:
the list up with #12 (row 11 of 13) brought in as row 3's top, #4 (`a-7-kids.png`; the lit row the pointer's, flag
`0x80`), `0x11e` posted; its first row's click: the window on #4 ($570, stamp 655), the list gone; the preview's click:
the window gone. `0x12e` was posted and said once (916/917); the later three opens put none in a slot (not read why).

**OpenTPW** (Q430): `ParkVisitorWindow`, opened by the strip's click on a visitor and a visitors row's `0x400`; Ctrl+V
with it up and its `b_locate` are `ParkLocator.OpenOnVisitor`; `b_kids` is `UiList.Pick`. Counted: the preview's model
(`VISITOR_WINDOW_PREVIEW`), a guest gone while it is up (`VISITOR_WINDOW_GUEST_GONE`, the window closed), the list's
`0x11e` (`VISITORS_LIST_ADVISOR_LINE`). A guest's world click reaches nothing (the pick finds only a cell's
occupant). Its run (`q430/confirm.py`, round 2) walked
the same guests as the original's: 42, 41, 42, 29, the list's top row 3 and its first row Visitor #4 ($570).

**Read for the staff member's build** (Q431). The stats' rows, label then figure (`FUN_004b53c0`, the refresh
`FUN_004b58b0`): `0x51a` 64 then `0x51b` the days; `0x51d` 65 then `0x51e` a money cell; `0x519` 67 then bar `0x514`
(`ridestatbar.wct`); `0x518` 69 then bar `0x513` (`happygrad.wct`); `0x51c` 68 then bar `0x515` (`ridestatbar.wct`);
`0x516` 66 then the picture `0x51f` (1130,553), type 6 in `i_staffstat`, part `FUN_00485ec0( FUN_005064f0 )` (0 and 1
to 0, 2 to 1, 3 to 2, 4 to 3, else 9) and the word `0x517` (1193,553). The labels at x 688 to 1085, the figures at 1193
to 1520, each bar inset 10 (`FUN_00485e30`). `FUN_00505b70` is `FUN_004c90d0`'s divide on `+0x1f0`: whole days of the
park's calendar. **The arrows** walk mask 1 (`FUN_0048ca00`'s class -1): `FUN_00482da0`'s case 1 takes the
used-thing list's previous link from the member, or from the list's tail, and keeps the first whose kind byte is 4 to
8 (`FUN_00481f80`); `FUN_004824d0` the other way. The open counts the staff the same way. **PICK UP** is enabled by
`FUN_00505bd0`: never in states 4, 5, 7, `0xd` or `0x12`, else where the cell the member stands in (bytes `+5`, `+7`)
has `mType` 0, 1, 3, 9 or 10 (`FUN_004fa990`). **`b_allstaff`** is `FUN_00496620( kind, member )`, two arguments: the
list on that kind with the member's row picked and brought into view (`FUN_00664d2a`, `FUN_00665739`), as the
visitor's `b_kids`; a kind of -1 is the one last shown. That opener's last act posts advisor message `0x11d`
(`0x00496b9e`). The preview's handler (`FUN_004b5c80`) takes only `0x10006`: `FUN_0048ce80`, the camera to the
member, and the window stays. `FUN_004b3f60( member )` sets the locator 1 first, then the arm, then the role and
button by the type byte, shows the status, hides the thought and posts `0xc6`.

**The original** (`q431/orig/run.sh`, the stock park; `q431/PREDICTION.txt` round 1): the window on Mike Cooper, "39
days" on 2.10.2000, "$ 161", "Patrolling", both arrows, PICK UP lit (`a-1-window.png`); `b_ardown` the handyman (25),
`b_arup` back, again the entertainer (27); `b_locate` and Ctrl+S over the window: the window gone, `[0x007cc2f0]` 1,
role 4 on him; `b_allstaff`: the entertainers' tab, "Pierre Hintze" lit (`a-7-allstaff.png`), `0x11d` posted; its
row: the window on him; the preview: the window kept. **The advisor's gate decides what is said, not the window**:
`0xd4` went in once (916), was said to its end and was refused at every later show (the Tutorial group's
`SayOnlyOnce` 1); `0xc0` was said on 972, 1033 and 1080, each cut by the close, and refused at the fourth build
(`DiscardAfterSlaps` 3).

**OpenTPW** (Q431): `ParkStaffWindow`, opened by the strip's click on a member, a member's world click and an
all-staff row's `0x400`; Ctrl+S with it up and its `b_locate` are `ParkLocator.OpenOnMember`; `b_allstaff` opens
`ParkStaffScreen` on the kind with the row picked. Counted: the preview's model (`STAFF_WINDOW_PREVIEW`), the name's edit (`STAFF_WINDOW_NAME_EDIT`), a member gone
while it is up (`STAFF_WINDOW_MEMBER_GONE`, the window closed), the all-staff list's `0x11d` (`STAFF_LIST_ADVISOR_LINE`).
The switch's second help row, 87, is not shown. Its run (`q431/confirm.py`, run1) walked 26, 25, 26, 27 as the
original's, "47 days" on tick 1109.

**FIRE and PICK UP, read for the build** (Q432; `FUN_004b5cb0`'s `0x50c` and `0x50e`, read as disassembly). FIRE, with
the Confirmations option `[0x0078d90f]` set, opens `MessageBox_Open( &text, 2, 0, 1, FUN_004b6070, 0, 0, 0 )`: UITEXT
`0x18d` (397, "DISMISS EMPLOYEE" and "Are you sure you want to dismiss this employee ?") white in font 4, the cross in
the bottom place with no act and the tick in the middle place with `FUN_004b6070`. The text record's fourth word is
the member as the box opened (`[0x007c2658]` at `0x004b5e96`); the box's handler calls an act with its kept word 3
(`FUN_0047eed0`), which `FUN_004b6070` hands in ECX to the dismissal `FUN_00505790`. With the option
off, `FUN_00505790` at once. Either way the window's vtable `+0x2c` closes it next (`0x004b5ecc`), behind the box. The
dismissal first installs the idle mode where the current one is type 6 (`0x00505830`: a carried worker is put back,
"The worker (type 6)" in `park-engine.md`), takes one month's wage by kind and grade from the bank and adds it to the
analyser's staff costs (`+0x1f7f0`), puts particle `0x13` at the member (`FUN_004fa470`: the model's place plus the
map's offset, the ground's height there), and deletes the thing (`FUN_0050b780`). PICK UP keeps the member
(`FUN_0048c950`, `[0x007c2650]`), picks them up (`FUN_00505c50`, ECX the member) and closes the window.

**The original** (`q432/orig/run.sh`, the stock park; `q432/PREDICTION.txt` rounds 1 and 1b): the option read 1. PICK UP
on Mike Cooper: the window gone, `+0x19c` 7, the hand's word (`mStaffHQ`'s `+0x60`) 26, `[0x007c2650]` his address, a
new mode object, and a figure of him hanging at the pointer (`orig/a-2-pickup.png`); a click in the middle of the view
put him down at (47,24), state 1 (the mechanic's search), the hand's word 0, the bank unmoved. FIRE: the window gone and
the box up (`orig/b-5-fire-box.png`); the cross kept him; FIRE again and the tick: the bank 88122 to 87961 and the staff
costs 538 to 699 in the same 1.2 s, exactly 161 (his wage, 7 x 23), his `+3` 1; the mechanics' locator then named nobody.

**OpenTPW** (Q432): FIRE opens `MessageBox` on UITEXT 397 where `GameOptions.Confirmations` is on, its tick
`ParkPeople.Fire` on the member kept; PICK UP is `ParkPeople.PickUp`; each closes the window. `ParkPeople.Fire` puts a
carried worker back first. A member in the hand is not drawn in the park (`ParkGuestSprites.InTheHand`; the `guests`
census says "IN THE HAND, not drawn"). Counted: the puff (`STAFF_DISMISSAL_PARTICLE`) and the figure at the pointer
(`STAFF_PICKED_UP_PREVIEW`), so our pointer stays the plain arrow while a worker is carried. Its run (`q432/confirm.py`)
took 161 off the bank and put 161 on the staff costs across the tick, the same box (`q432/side-by-side-box.png`).

**The patrol switch and the patrol tool, read** (Q433). The switch `0x510` calls `FUN_004b5b50` with its new state. Up
(0): `FUN_00506e90` on the member, the whole map (`+0x20a` 1, `+0x20c` `0x4000`), and the window stays. Down (1): the
member kept (`FUN_0048c950( [0x007c2658] )`, into `[0x007c2650]`), the build-tool shell `FUN_0046c580( 0x36 )` installed
(its install `FUN_0046d5a0` runs the setter `FUN_0052f200( 0x36, 1 )`), the window closed (vtable `+0x2c`), then advisor
message `0xc1` posted. **The setter withdraws ten messages on any change of mode** (`0x0052f245`..`0x0052f326`):
`0xb8`, `0xcb`, `0xd6`, `0xcd`, `0xc9`, `0xce`, `0xc1`, `0x159`, `0x15a` and `0x151`, so `0xc1` is cut as the tool is put
away, however it is. **The preview** (`FUN_005234d0`'s `0x36` arm) sets the run's price to -9999 (`FUN_0052fb90`), then
unanchored lays one square under the pointer and anchored the rectangle from the anchor to the pointer, its least and
greatest corners, in the four lines of "The patrol edges" above, after setting the refusal latch `DAT_00816d50` by
`FUN_00532a50`: **refused where every cell of the rectangle on the map is outside the park** (`mFlags & 0x40`) **or
answers track type `0x19`** (its own track record, `FUN_004d0af0`, or its parent's for types 12 and 17,
`FUN_0053ad90`), so a single cell of the park lets it through. The verdict `FUN_00535670( 0x36 )` is the latch alone
(`0x005356df`), so the outline is all blue or all red, and the anchored one-cell rectangle on land outside the park
is red already. The cursor (`FUN_0052f950`) has no case for `0x36`: only cursor 8, `c_noplace`, where a square was
red (`DAT_00816d48`). **The click** (`FUN_00524960`, on the release; the press `FUN_00524790` does nothing for it):
refused (`DAT_00816d48`), sound `0xaf` and the tool put away, nothing written; unanchored, the anchor set and the kept
member read again (`thunk_FUN_0048c920` into `DAT_008186b0`); anchored, where a member is kept, `FUN_00506e50` writes
the least corner's cell id to `+0x20a` and the greatest's to `+0x20c`, then the anchor is cleared and the tool put
away (`FUN_0052f200( 0, 0 )`). Neither click plays a sound.

**The original** (`q433/orig/run.sh` and `run2.sh`, the stock park; `q433/PREDICTION.txt` rounds 1 and 2): Mike Cooper's
switch drawn down (footprints crossed in red) on (39,21)-(56,28), 2728 and 3641; up: 1 and 16384, the window up, the
footprints alone and "Idle" at the next refresh (`orig/a-2-switch-up.png`); down: the window gone, `[0x007c2650]` him,
mode `0x36`, `0xc1` posted and said, `0xd4` cut by the close, one blue square at (51,27) and the plain arrow; a click
there anchored (51,27); the pointer at (54,26): 8 cells in texture 0, (52,26), (53,26), (54,26), (54,27), (52,27),
(53,27), (51,27), (51,26), row 26 lifted over the built cells (`orig/a-5-outline.png`); the click: 3380 and 3511,
(51,26)-(54,27), mode 0, the anchor -1, `0xc1` cut (slaps 1); the window again down and "Patrolling". On the band
outside the park, (45,15) anchored with the latch already 1 and (53,15) gave 16 squares on 9 cells, every one
texture 1, and the no-entry pointer (`orig/b-4-outline.png`); the click put the tool away and wrote nothing.

**OpenTPW** (Q433): the switch is `ParkStaffWindow.Patrol`; the tool is build mode `0x36`, `ParkBuildMode.Patrol` with the
member as its `Serves`, and `ParkPatrolTool` (its `Strip`, `Refuses` and `Run`) lays the preview through
`ParkPathBuilding.Strip` and takes the world click; `Staff.SetPatrolArea` and `ClearPatrolArea` write the two corners.
`ParkBuildMode.Arm` withdraws `0xc1` on a change of mode, the only one of the setter's ten posted here; the cursor is the
plain pointer, `NoGo` while the outline is red. Counted: the refused click's sound (`PATROL_TOOL_REFUSED_SOUND_0xAF`).
Its run (`q433/confirm.py`, run4, the final build; real clicks, each cell found by moving the pointer and reading `strip`'s drawn census, now `lib/cellprobe.py`): the same switch
words, the same 8 squares in the same order for (51,27) and (54,26), row 26 lifted 20 on (51,26) and (52,26) and 10 on
(53,26) as the original's (tops 21.5, 21.5 and 11.5 over 1.5), the plain pointer, "patrols (51, 26)-(54, 27), cells 3380 to 3511", the window again "Patrolling"; on the band
(45,15) four red squares on the anchored cell (each line lays it), 16 red at (53,15), `NoGo`, nothing written
(`q433/side-by-side.png`). **At one camera** (Q435) the lift is the original's: its camera after Mike's window
closed, read by `original/camera.py` (point (484.442, 259.319), zoom 110, rotation 0), given here as `camera 484.442
259.319 110 0`, puts the eye at the same (484.44, 93.62, 201.57) in both games, so the frames differ by the lens alone
(Q260); our frame's middle 480 x 360 scaled by 768/360 lays our lifted tops of (51,26) and (52,26), y 284 to 314 here,
on the original's (`q435/side-by-side.png`), and the cells the original's camera put at two pixels were the cells under
the same pixels here at the first probe. Q433's "far higher" was its two cameras (ours zoom 60, pitch 45; the
original's 110, 58.3) and the lens. The walls a lifted square drops to a lower neighbour are drawn in both (Q436;
`park-engine.md`, "Placement feedback", "The walls"): the same nine records, face for face, under the outline's tops of
(51,26) to (53,26) (`q436/side-by-side.png`).
Escape with the tool armed and anchored, in the original, puts it away and writes nothing (mode 0, the anchor -1, the
area 1 and 16384; `q435/orig/a-run.log`), the outline and the advisor gone from the next frame and no menu up
(`q435/orig/a-6-escaped.png`): the game table's row 0 installs the idle mode (`0x0040c368`), whose setter cuts `0xc1`.
**Here the same** (Q437; `ParkFrontEnd.MenuKey` to `ParkHand.LetGo`): unanchored and anchored on (51,27), each Escape
logged `withdraw message=193 cleared=0 cut=1` then `Escape: the build tool is put away`, mode 0 not anchored, Mike's area
still the whole map, (0, 0)-(127, 127), no square or wall drawn and no menu; the next Escape opened the game menu (`q437/run2`,
`q437/side-by-side.png`). **A quick right click with the tool armed** (RMB cancel on, `[0x0078d911]` 1) does the same in
both games (Q438): the armed release installs the idle mode with no type test (`0x0048842b`; `park-engine.md`, "The hand's
ways out"). The original's (`q438/orig/run.sh`, the right button held 0.08 s): unanchored and anchored on (54,17), mode
0x36 to 0, the anchor -1, Mike's area 1 and 16384, `0xc1` cut (slaps 1, then 2), no marker square left and no menu; the
camera unmoved across the click; the pointer the idle one over the ground, with the help bar "Left-click to build path"
(`orig/a-4-rclick.png`, `orig/b-8-rclick.png`). Ours (`Level.RightButton` to `Level.QuickRightClick` to `ParkHand.LetGo`;
`q438/run1`), unanchored and anchored on (51,27): `withdraw message=193 cleared=0 cut=1` then `world click: right click - the
build tool is put away`, mode 0 not anchored, Mike's area the whole map, `strip` none, the camera unmoved, `pointer`
"cursor Path, help row 441", no menu; a second quick click, over the idle mode, logs nothing (`q438/side-by-side.png`).
A member fenced where one path cell lies stays on it, rolling it again and standing
eleven sweeps with thought `0x16` on one roll in about 55, in both games (Q434; `ride-operation.md`, "A member fenced
where one path cell lies").

#### How allstaff and allitems keep their rows current

Read and measured 2026-10-14 (Q365). Both screens rewrite their rows in place every two seconds on timer `0x80083`,
as allpeeps does ("How allpeeps keeps itself current"), and nothing else between a fill and the next:

- **allstaff.** The opener arms the timer once (`FUN_0065ef90( 0x80083, 2000 )`, `0x00496b84`); a tab does not. On it
  the screen's callback (`FUN_00495da0`, message `0x10`) walks the rows from the last to the first and hands
  `FUN_006644ea` each member's status, wage, skill and happiness with the column mask **`0x1a`**: the status, the skill
  and the happiness are written, the name and the wage stay as the fill wrote them. `FUN_006644ea` writes the masked
  cells and nothing else, so a row's colour and place stand.
- **allitems.** Each tab loads its own list tree, whose handler `FUN_004955e0` arms the timer as the tree is shown
  (message `0x15`, `0x0049560c`): **a tab starts the timer again**. On it, by the remembered tab (`[0x007ca154]`, which each
  sub-builder writes), the rides' and shops' rows are rewritten under mask **`0x1e`** and the sideshows' under
  **`0x3e`**: every column but the name. The miscellaneous tab has no case and is never rewritten.
- **The meters** are set by the opener (both) and by a tab (the kind's, `FUN_004964d0`) and by nothing else.

Seen in the original: between `q365/orig/a-shops` and `a-shops2`, nine seconds with no fill, the Drinks Shop's row
went from 1 customer, $10 and $20 to 4, $40 and $50.

**OpenTPW** (Q365): `ParkStaffScreen.Update` and `ParkItemsScreen.Update` are the two callbacks, on a `UiTimer` of
2000 ms of real time: the staff one stamped at the opening, the items one again at each tab; each logs `Staff list:
kind K, N rows rewritten, meters K/A, row 0 ...` or `Items list: tab T, N rows rewritten, row 0 ...`. The staff
meters are set at the opening and a tab, where they were set every frame. On `q353/grades.TPWS` (`q365/run-fix2`, the
final game binary; frames `run-fix2/four.png`): ten rewrites of the one mechanic in the 21 s after the opening, his
happiness 85 then 84 on screen and in the line, his wage 92 and the meters 880/895 in all ten, his status and
happiness the `staff` census's; three rewrites of the Drinks Shop in the 7 s after the shops' tab, none on the
miscellaneous tab. Main's build (`run-control2`) wrote none. Since Q367 and Q368 the rewrite moves every column below.

#### The all-items rows: a number, then bars

Read 2026-10-14 (Q367). Each tab's sub-builder hands `FUN_006639cb` a row factory and a styler, which make every row's
cells by the remembered tab: rides (`FUN_004944d0` and `FUN_00494690`) a name label, a number painter
(`FUN_0048fde0`) and three type-9 bars; shops a number, two money cells (`FUN_00490250`) and a bar; sideshows a number,
a bar, two money cells and a bar; miscellaneous a number. Every bar wears `ridestatbar.wct` (`FUN_00477ff0`, "The stat
bars' skin") and is inset 10 units from its row's top and bottom (`FUN_00485e30( cell, 0, 10 )`). The fill's adder
(`FUN_00493cd0`, through `FUN_00481bc0( 0x80, ... )`) and the rewrite (`FUN_004955e0`) hand the same values:

| Column | Rides | Shops | Sideshows |
|---|---|---|---|
| 1 | customers | customers | customers |
| 2 | excitement bar | last month's profit `$` | excitement bar |
| 3 | State of repair bar | total profit `$` | last month's profit `$` |
| 4 | remaining life bar | satisfaction bar | total profit `$` |
| 5 | | | satisfaction bar |

The customers are the ring `+0x1a8`'s last min( filled, 30 ) finished days added (`FUN_00495c20`, `PUSH 0x1e` at
`0x004956ca`), `__ftol`'d, a number. The excitement is `FUN_004e0860( 0 )` (`ParkRideScore.ExcitementOf`), the State
of repair `+0x44` (`0x00495715`), the remaining life `+0x48` (`FUN_004dd6d0`), last month's profit `FUN_004e1c70`,
the total `FUN_004e1c60` and the satisfaction `FUN_004e1e30`, read 2026-10-14 (Q368) on the object's day rings
(`ride-operation.md`, "The object's six day rings": each `mTemp`, `mData[30]`, `mCurrentEntry`, `mNumEntries`,
`mWrappedAround`):

- **Last month's profit** (`FUN_004e1c70`): the takings ring's (`+0x70`, days at `+0x74`) last min( filled, 30 )
  finished days added, less the costs ring's (`+0xf8`, days at `+0xfc`), each day loaded as an unsigned 32-bit figure
  (`FILD qword` with a nought high half, `0x004e1d21`), the difference `__ftol`'d (`0x004e1de9`). Today's is not among them.
- **The total** (`FUN_004e1c60`): `mTotalTakings` `+0x180` less `mTotalCosts` `+0x184`, a 32-bit `SUB`.
- **The satisfaction** (`FUN_004e1e30`): 50 plus the satisfaction ring's (`+0x340`, days at `+0x344`) last min( filled,
  30 ) days added as signed figures (`FIADD`), over their count (`FDIVR`), `__ftol`'d toward nought (`0x004e1ef1`); 50 when
  the ring holds no finished day (`0x004e1f06`). Its bar is the bar rule below.
- **A money cell** (`FUN_00490250`, vtable `0x006ff3b0`, its setter slot 3 `0x004903e0`) letters UITEXT `0x1c1` ("$ ")
  and the figure, or under nought `0x1c2` ("-$ ") and the figure without its sign (`CDQ`, `XOR`, `SUB`), through
  `FUN_006acd60` on the table `[0x0078ba94]` with one `VARM` variable; the variable's type word (`0xd`, `0x13`) names the
  placeholder, and its writer (`0x006add40`) prints the value `"%d"` (`0x0074f9d8`, wide).

Measured in the original under Proton (`q368/orig/a`, `q368/money.TPWS`: `mkscene.py` wrote the Drinks Shop's and Jungle
Spray's days and totals into `q353/grades.TPWS`; `original/money.py` polling): at the load's tick 963 the Drinks Shop
165, 3766, 53; the Jungle Spray -120, -600, 48; the Strength Bird, its rings empty, 0, 0, 50. Its frames letter "$ 165",
"$ 3766", "-$ 100", "-$ 580", "$ 10" and "$ 10" (a sale each by then, as the poll read), right-aligned, and its
satisfaction bars measure 0.526, 0.491 and 0.509 of their width (542, 491 and 512 of 1024).

Every bar is `((value & 0xff) << 10) / 100` of 1024, a float truncated first (`__ftol`), so a value past 255 shows its
low byte. Measured in the original on `q353/grades.TPWS` (`q367/orig/a-*.png`, `object.py` on thing 13): the Belly
Bounce's row "5" with bars of 0.403, 0.925 and 0.993 of their width, its repair 92.32 and life 99.85 at the frame
(942 and 1013 of 1024); the sideshows' excitement bars 0.308 (Jungle Spray, 30) and 0.28 (Strength Bird, 27).

**OpenTPW** (Q367): `ParkItemsScreen.NumbersOf` is the row by column, `UiList.BarColumns` the bar cells (a
`ParkObjectWindow.UiStatBar` inset `BarInset`), made again at each tab since rides and shops share a tree here; each
fill logs `Items list: tab T, row N <name>: <columns>`, `-` for a column not worked out. On the same file, paused at
the load (`q367/run-fix4`, the final game binary): `row 13 Belly Bounce: 3 409 962 1013` beside `rings 13` last30 3,
`spend` excitement 40 and the ride window's repair 94.375 and life 99.887505; the bars 0.40, 0.944 and 1.0 of their
width; the Drinks Shop `1 - - -` with its satisfaction cell black; the sideshows `0 276 - - -` and `0 307 - - -`
(bars 0.27, 0.30). Main's build (`run-control1`) left the customers and excitement blank and lettered 94 and 99.
**OpenTPW** (Q368): `ParkObjectRings.ProfitOfLastDays` and `SatisfactionOfLastDays`, `ParkItemsScreen.Money` the
cell's text; `rings` now prints the total takings beside the total costs. On `q368/money.TPWS`, paused at the load
(`q368/run-fix2`, the final game binary): `row 16 Drinks Shop: 1 165 3766 542`, `row 43 Strength Bird: 0 276 0 0 512`,
`row 14 Jungle Spray: 0 307 -120 -600 491`, the frames lettering "$ 165", "$ 3766", "$ 0", "-$ 120", "-$ 600" and the
bars 0.532, 0.505, 0.485 (`q368/side-by-side.png`); main's build (`run-control2`) lettered nothing and counted
`ALL_ITEMS_MONTHLY_HISTORY`, which is gone. The original's sideshow rows stand Jungle Spray then Strength Bird by its
sort, the name rising (Q374, below). The rows' font and height are the factory's (Q377, "The rows' font and
height"); the headings are framed (Q370, "The list headings' frames").

#### The miscellaneous tab: a row an object, and each tab's sort

Read 2026-10-15 (Q374). The miscellaneous sub-builder `FUN_00495110` fills through `FUN_00481bc0( 0x400, 0x004941e0 )`:
bit `0x400` is `FUN_00482040( 3, adder )`, which walks every thing of type 3 whose `FUN_004e2710` word is nought and whose
item's `Info.WhichUIType` (`+0x4ac`) is 3, the rides' walk (`0x80`) with the kind changed. **The adder `0x004941e0` makes
a row an object**: the cells are the object's name (`FUN_00486280`, the name the other tabs' adder `FUN_00493cd0`
letters) and its item's standing count, the control record's `+0x18` (`FUN_004ca8c0` over `FUN_004d3d80`, keyed by the
thing's item word `+0xe`); the row's data is the thing (`FUN_0066403b( cells, thing, -1, 0 )`). So two Small Toilets are
two rows, each "3" where three stand. The list's right click (`0x00495554`) has no case by tab: a miscellaneous row
goes to its object as a ride's does.

**Each tab keeps its own sort**, as the hire, buy and visitors screens keep theirs ("A list's order"): the sub-builders
hand `FUN_006659af( word, 0 )` the words `[0x00750d5c]` (rides), `[0x00750d60]` (shops), `[0x00750d64]` (sideshows) and
`[0x00750d68]` (miscellaneous), each 1 in the file, the name rising; the screen's handler `FUN_00495360` stores `0x406`'s
value in the word of the remembered tab (`[0x007ca154]`, the jump table at `0x004955c8` from `0x004954f9`). Every
sub-builder makes column 0 text (`FUN_006636b2( 0, 0 )`).

Measured in the original under Proton (`q374/orig/a`, `q353/grades.TPWS`): the miscellaneous tab lettered Litter Bin 1,
Round Fountain 1, Security Camera 2 twice, Small Toilet 3 three times and Staff Room 1, the first row picked; a click on
Name stood them the other way, Staff Room first; the rides' tab and back kept that; the sideshows' tab stood Jungle Spray
above Strength Bird; a right click on the third row shut the screen with the view on a security camera.

**OpenTPW** (Q374): `ParkItemsScreen.Show`'s miscellaneous arm adds a row a placed object, keyed by its thing, with
`ParkState.BuiltOf( item ).Standing`; `_sorts` holds the four words, handed over as each tab fills, and the lists' column
0 is text. Each fill and heading click logs `Items order: tab T, sort W, N rows: ...`. On the stock park with a Giant
Puzzle and a Balloon Shop bought under pause (`q374/run-fix3`, the final game binary): `tab 3, sort 1, 8 rows: 17 Litter
Bin 1, 24 Round Fountain 1, 19 Security Camera 2, 18 Security Camera 2, 23 Small Toilet 3, 22 Small Toilet 3, 21 Small
Toilet 3, 20 Staff Room 1`, eight rows lettered (`4-misc.png`); the Name heading's click `sort -1`, Staff Room first
(`5-misc-falling.png`), kept across the rides' tab; `tab 1, sort 1: 44 Balloon Shop 0, 16 Drinks Shop 0` and `tab 2, sort
1: 43 Giant Puzzle 0, 14 Jungle Spray 0`, each bought row added last and standing first; the right click on row 2
`Camera: to thing 19 on cell (40,29)`, the screen shut (`7-camera.png`). Main's build (`run-control2`) lettered five rows,
a row a kind, and counted the right click (`ALL_ITEMS_MISC_ROW_RIGHT_CLICK`, which is gone).

**Every tab picks its first row as it fills** (Q375). The add's pick ("The first row is picked as a list fills") posts
`0x401`, and the handler `FUN_00495360` has no arm for it (it goes to the default, `FUN_0065f66e`): the pick is the
highlight alone. It is an index, so the row highlighted is the one sorted first, it stays at the top over a heading's
re-sort, and the right click (`0x402`, the selected index) on a point below every row goes to that row's thing.
Measured in the original (`q375/orig/a`, `q353/grades.TPWS`; `rectcolour.py`'s teal share of row 0's name cell, row 1's
nought each time): rides 0.935, shops 0.931, sideshows 0.93, misc 0.949, and 0.939 after the Name heading with the Staff
Room on top; the pointer brought up from under the list to below the rows and a right click shut the screen with the view
on the Staff Room. **OpenTPW**: `ParkItemsScreen.BuildList` sets `UiList.PicksFirstRow`; nothing takes the posted pick,
and the `Items order` line ends `; picked N`. On the stock park with the Giant Puzzle and the Balloon Shop bought
(`q375/run-fix1`, the final game binary): picked 13, 44 (the Balloon Shop, added after the Drinks Shop), 43, 17, and 20
after the heading; row 0's teal 0.959, 0.971, 0.965, 0.968, 0.965, row 1's nought; the right click below the rows
`Camera: to thing 20 on cell (58,16)`, the screen shut, neither `LIST_FIRST_ROW_SELECTED` nor
`LIST_RIGHT_CLICK_FIRST_ROW_NOT_SELECTED` counted. Main's build (`run-control1`): no highlight, the screen left open, the
miss counted once and the pick four times.

#### The all-staff rows: a money cell, then bars

Read 2026-10-14 (Q369). The opener `FUN_00496620` hands `FUN_006639cb` the row factory `0x00496c10` and the styler
`FUN_00496d20`, which make each row's five cells: the name and the status as labels, the wage a money cell
(`FUN_00490250`, "The all-items rows", above: "$ 81"), the skill a bar in `ridestatbar.wct` and the happiness a bar in
`happygrad.wct`, both inset 8 from the row's top and bottom (`FUN_00485e30( cell, 0, 8 )`). The row adder
`0x00496320` hands them:

- **the wage** `FUN_00506870`: a table by kind (`[0x00785380]`, types 5, 4, 6, 7, 8 as 0 to 4) times one by grade
  (`[0x0078532c + grade x 16]`), which this game reads as `PerTypeStaffConsts[kind].PayMultiplier` times
  `PerGradeStaffConsts[grade].BaseWage` (`ParkStaffPool.WageFor`): 81 for the grade-4 handyman in both games;
- **the skill's bar** `(skill << 10) / 100`, the skill `FUN_00506550`: `__ftol( (grade - pct x -0.01) x 20 )`, the
  grade `+0x1e4` and `mPercentageThroughGrade` the byte `+0x1e8` (the double `-0.01` at `0x00700838`, the float 20 at
  `0x00700840`). That is `(grade x 100 + pct) / 5` exactly, for every grade 0 to 4 and every byte, with the FPU at 24,
  53 or 64 bits (`q369/skillfp.py`): 0 to 100 through the grades, to 131 past it, which the control holds at 1024;
- **the happiness's bar** `((__ftol( +0x1f8 ) & 0xff) << 10) / 100`, as the all-items bars.

The screen's two meters (`0x32c`, `0x32d`) are the same skin in `happygrad.wct`, each inset 8 (`0x0049698d`,
`0x00496a49`), not the gauge's `meter.wct`: horizontal, the ramp to the value and the grey track after it. The
texture (`ui.wad`, `textures\happygrad.wct`, 32 by 32) is laid out as `ridestatbar.wct`, a ramp from cyan through
orange to yellow in one half and the grey track in the other. The factory measures the row's height from its font
(`[0x007ca170]`, as the items rows' `0x007ca158`), and so does this game (Q377, "The rows' font and height").

Measured in the original under Proton on `q369/skill.TPWS` (`q353/grades.TPWS` with the percentages 60, 45, 99, 0, 13
written at the staff record's `+480`; `q369/orig/b`, `thingwords.py` on `+0x1e4`, `+0x1e8` and `+0x1f8`): its rows
letter "$ 81" and draw the skill bars 0.925, 0.283, 0.192, 0.608 and 0.417 of their width (942, 296, 194, 614 and 430 of
1024), and the happiness bars 0.85, 0.817, 0.817, 0.858 and 0.817 against its polled happiness at the frames (0.850,
0.819, 0.829, 0.850, 0.819); its meters 0.828 with a grey rest. Measured by where the grey track starts
(`barsplit.py`): `barshare.py`'s colour count drops `happygrad.wct`'s greyish-teal texels and reads them 0.02 short.

**OpenTPW** (Q369): `ParkStaffScreen.NumbersOf` and `SkillOf`; `UiList.BarSkins` and `BarInset` the styler's skin and
inset; `ParkObjectWindow.UiStatBar.Skin` the texture handed; the meters are `UiStatBar`s. Each fill logs `Staff list:
kind K, row <thing> <name>: <status> <wage> <skill bar> <happiness bar>`, and the rewrite its row 0 the same way. On
`q369/skill.TPWS`, paused at the load (`q369/run-fix2`, the final game binary): `row 25 Duke Mighten: Walking 81 942
880`, `26 ... 92 296 880`, `27 ... 36 194 880`, `28 ... 105 614 901`, `30 ... 125 430 921`; the frames' skill bars 0.92,
0.292, 0.195, 0.602, 0.425 and happiness bars 0.866, 0.866, 0.866, 0.884, 0.902 (`barsplit.py`); the meters 0.846 and
0.863 with a grey rest; "$ 81" (`1-handymen.png`). Main's build (`run-control1`) lettered the grade and the happiness as
numbers and filled its meters from the bottom. The headings are framed (Q370, "The list headings' frames"); the
status's words are Q379's (below); the list's sort is Q381's ("The all-staff list's sort", below).

#### The all-staff list's sort

Read 2026-10-15 (Q381). The opener `FUN_00496620` sets the five columns' types (`FUN_006636b2`, `0x004966a9` to
`0x004966d5`): the name and the status 0, text; the wage, the skill and the happiness 1, numbers. After the handler
(`0x004966f6`) it hands the screen's own word `[0x0075118c]` to `FUN_006659af( word, 0 )` while the list is still
empty (`0x004966fb`), and the handler `FUN_00495da0` stores message `0x406`'s value there (`0x00495fe0`). The word is 1
in the file, the name rising, and is one word for all five tabs: each tab's fill goes through the sorted insert under it
("A list's order", above). The status sorts on the word the cell letters, the skill and the happiness on their bars'
units, the wage on the wage.

Measured in the original under Proton (`q381/orig/a`, `orig/a-rows.png`), given this game's file of the stock park with
the three cleaners of the pool hired in falling name order (`q381/run-fix2/q381-fix.TPWS`; the four walking, `mState`
1): the cleaners' tab opened Chris Battson, Duke Mighten, Paul Brooke, Rajan Tande; Monthly Wage, Rajan at $ 45 and the
three at $ 63 in name order; again, those three as they stood and Rajan last; Current Status, the three Idle as they
stood and Duke, Patrolling, last; Escape and S again, the same. Its list highlights row 0 throughout (the add's pick: "The all-staff list's first row", below).

**OpenTPW** (Q381): `ParkStaffScreen._sort`, handed to `UiList.SetSort` before the fill and kept by `SortChanged`;
`TextColumns` the name and the status. Each fill and heading click logs `Staff list: kind K, sort W, N rows: ...`. On
the same scene, paused (`q381/run-fix2`, the final game binary): `sort 1`: 45 Chris Battson, 25 Duke Mighten, 44 Paul
Brooke, 43 Rajan Tande; `sort 3`: 43 Rajan 45, 45 63, 25 63, 44 63; `sort -3`: 45, 25, 44, 43; `sort 2`: 45, 44, 43,
25, and `sort 2` again opened anew; no `LIST_HEADING_SORT_NOT_BUILT` (`q381/fix2-rows.png`). Main's build
(`run-control2`): 45, 44, 43, 25 as hired (a hire goes in at the staff list's head) in all five frames, and
`3x LIST_HEADING_SORT_NOT_BUILT`.

#### The all-staff list's first row

Read 2026-10-15 (Q382). The list's own add picks the top row as the first goes in ("The first row is picked as a list
fills"), and the screen's handler `FUN_00495da0` has no `0x401` arm: its arms are `0x10` (the rewrite), 5, `0x100`,
`0x14`, `0x15`, `0x400`, `0x402` and `0x406`, and its default `FUN_00488ba0` hands `0x401` on to `FUN_0065f66e`. So the
pick is the highlight alone, as the all-items screen's is: an index, the row sorted first, kept at the top over a
heading's re-sort, and the right click (`0x402`, the selected index, `0x00495feb`) on a point below every row goes to
that row's member. Measured in the original (`q382/orig/a`, given `q381/run-fix2/q381-fix.TPWS`; `rectcolour.py`'s teal
share of row k's name cell, virtual 300, 420 + 39k, 700, 452 + 39k): row 0 0.953 on the cleaners' tab, 0.956 on the
mechanics' (one row), 0.953 back on the cleaners', 0.953 after Current Status and 0.947 after it again, Duke Mighten then
first; rows 1 and 2 nought each time. A right click below the four rows shut the screen and moved the view.

**OpenTPW** (Q382): `ParkStaffScreen` sets `UiList.PicksFirstRow`; nothing takes the posted pick, and the `Staff list:
kind K, sort W, N rows: ...` line ends `; picked N`. On Q381's scene, paused (`q382/run-fix1`, the final game binary):
picked 45 (sort 1), 43 (sort 3), 45 (sort -3), 45 (sort 2), 26 (the mechanics' tab), 45 (the cleaners' again); row 0's
teal 0.941 to 0.956, rows 1 and 2 nought; the right click below the rows `Camera: to thing 45 on cell (47,21)`, the
screen shut, neither `LIST_FIRST_ROW_SELECTED` nor `LIST_RIGHT_CLICK_FIRST_ROW_NOT_SELECTED` counted. Main's build
(`run-control1`): no highlight, the screen left open, the miss counted once and the pick three times.

#### The all-staff status words

Read 2026-10-15 (Q379). The row adder `0x00496320` letters the status cell with a row of the table `[0x0078baa4]`
through `FUN_006ace60` (nothing for a row past the table's end or not a string) and `FUN_00485ac0`. That word is
`0x0078b530 + 0x574`: the boot's `FUN_00419710` (called from `WinMain_Main` `0x0045ab6a` with `ECX` `0x0078b530`)
loads table `0x3ed` into `+0x574` and names it `mUIStaffStates` / `STAFFSTATES::TableID` in its failure message
(`0x00419a57`; UITEXT, table `0x3ee`, at `+0x564` is `[0x0078ba94]`, the same `0x10` apart). The table is
`Language/English/STAFFSTATES.str`: 0 Idle, 1 Patrolling, 2 Working, 3 Resting, 4 On strike, 5 Picked up.

The row is `FUN_005064f0` on `mState` (`+0x19c`): 0 to 0, 1 to 1, 2 and 3 to 3, 4 and 5 to 4, 7 to 5, and every
other state (6, and each kind's own jobs from 8 up) to 2. A 1 is then read as 0 where `FUN_00506eb0` answers nought
(`0x0049636b`): the patrol area's two words `+0x20a` and `+0x20c` are 1 and `0x4000`, cells 1 to `0x4000`, the whole
map, which the constructor `FUN_00504b90` writes and the patrol button's clear (`FUN_004b5b50` with nought,
`FUN_00506e90`) writes back. So a member walking with no patrol area of their own reads Idle. The two-second rewrite
(`FUN_00495da0`, `0x00495e41`) reads the same table.

Measured in the original under Proton (`q379/orig/bA`, `cB`): `q369/skill.TPWS` with the five members' `mState`
written (`q379/mkscene.py`) and each held by `thingpoke.py` on `+0x19c`, the staff HQ's strike flag set for the kind on
strike and a resting member's rest held at 10. Scene A (3, 7, 5, 1, 1; the researcher's area 1 to 16384) letters
Resting, Picked up, On strike, Patrolling, Idle; scene B (4, 0, `0xe`, 3, `0xf`) On strike, Idle, Working, Resting,
Working. A member of state 2 or 3 with no rest area faults the original (`0x00505fe0`'s and `FUN_00506d10`'s read of
thing `[+0x208]`), so state 2 was not photographed there.

**OpenTPW** (Q379): `Staff.StatusRow` and `PatrolsWholeMap`, `ParkStaffScreen.StatusOf`; a hire carries the whole map
as the constructor writes it (Q380: `ParkPeople.Hire`, `Staff.WholeMapBottomLeft` and `WholeMapTopRight`), and a member
with no area at all, a record built with none, is taken as the whole map too: a deviation. The fill's
log line quotes the word: `Staff list: kind K, row <thing> <name>: "<status>" <wage> <skill bar> <happiness bar>`. On
the same scenes, paused at the load (`q379/run-fixA4`, `run-fixB4`, the final game binary): A `"Resting"`, `"Picked
up"`, `"On strike"`, `"Patrolling"`, `"Idle"` with the census Resting, Held, OnStrike, Walking, Walking; B `"On
strike"`, `"Patrolling"`, `"Idle"`, `"Resting"`, `"Working"` with GoingOnStrike, Walking, Idle, Resting, Researching (B's
mechanic and entertainer change state in the sweeps before the pause). Main's build (`run-controlA2`) lettered the
activities' names and counted `STAFF_STATUS_TEXT` five times.

**The skill bar `0x2482`** ((722, 536)-(957, 575), type 9) is built at nought (`0x0049c335`) and written in one place,
message `0x401` with a row picked (`0x0049b8c6`; the only other `PUSH 0x2482` is the builder's): `((grade & 0xff) <<
10) / 5` of 1024 (`0x0049b8dc`), the grade the candidate record's byte `+8`. A grade 4 fills four fifths.

**The walk is corroborated three ways**: the tab help rows, the resolved mesh names, and each screen's
own tab-index switch all give the same ordering, and it matches UITEXT 119–122 and 139–143.

**The root frame mesh `0xf76e4200` is `w_big.MD2`, node `window4`.** The hash is over a node name, which is why
searching file stems never finds it; see the resolution table above for the method.

#### How a list's cells are aligned

Read 2026-10-15 (Q383). `FUN_0065c428( across, down )` posts `0xc00` to the control's text (`+0x98`) and skin (`+0x9c`)
with the two bytes at the message's `+4` and `+5`. A label's skin (vtable `0x006ff320`; the label `FUN_0048f6a0`, the
number painter `FUN_0048fde0` and the money cell `FUN_00490250` alike) takes it in its slot `+0xc`, `FUN_0048fc40`:
flag `0x400` stores the across byte at `+0xd8`, `0x800` the down byte at `+0xd9`. Each constructor writes 0 and 1 there,
so **a cell no styler aligns stands at ( 0, 1 ): from the left, in the middle**. `LabelSkin_Draw` (`0x0048f862`) hands
the across byte to the text renderer as 0 left, 1 centre, 2 right, after `FUN_004925a0` swaps 0 and 2 where the
language is `0x40d` (Hebrew; English never is), and the down byte as 0 top, 1 middle, 2 bottom (`FUN_006aeb10`: the
renderer's `+0xa4` and `+0xa0`).

The stylers, each call on the cell just made (`ESI`, the listing; the decompiler drops it):

| Styler | Name | Then |
|---|---|---|
| all items `FUN_00494690`, every tab | ( 0, 1 ) | **the count `( 1, 1 )`, centred** (`0x00494780`, `0x00494822`...); each money cell `( 2, 1 )`; the bars none |
| all staff `FUN_00496d20` | ( 0, 1 ) | the status ( 0, 1 ); the wage `( 2, 1 )` (`0x00496e39`); the bars none |
| visitors `FUN_004939d0` | | above: the number and the time `( 1, 1 )`, the cash `( 2, 1 )`, the rides ridden ( 0, 1 ) |
| hire `FUN_0049c880` | ( 0, 1 ) | the wage (the money cell `FUN_00490250`) `( 2, 1 )` (`0x0049c94d`) |
| buy `FUN_004ad340` | ( 0, 1 ), colour 255, 255, 255, 255 | the price (`FUN_004904c0`, vtable `0x006ff3e0`, built at ( 0, 1 )) `( 2, 1 )` (`0x004ad428`); the third cell the `i_boxtick` mesh alone, no skin and no font, so nothing lettered |

Measured in the original under Proton at 800 x 600 (`q383/orig/a`, `q353/grades.TPWS`; `listcells.aligns`, white 110):
the count's middle 0.477 of its column on the rides tab, 0.472 shops, 0.483 and 0.478 sideshows, 0.494 to 0.495 on all
eight misc rows; the money cells' middles 0.83 to 0.88; the names from 0.000 to 0.012.

**OpenTPW** (Q383): `ParkItemsScreen.BuildList` sets `ColumnAligns` Start, Centre, then End, and its shape line ends
`aligned Start Centre End End End` (5), `... End` (6) and `aligned Start Centre` (2). On the stock park with a shop and a
sideshow bought, paused (`q383/run-fix3`, the final game binary): every count centred, its middle 0.488 to 0.518 (rides,
shops, sideshows) and 0.495 to 0.498 (misc, eight rows); the money cells' middles 0.873 to 0.906 (`q383/side-by-side.png`:
the original, this, main). Main's build (`run-control1`) lettered every count from the right. `ParkStaffScreen` already
letters the name and status from the left and the wage from the right; this game's `TextDown` is the middle by default.
The Hebrew swap is not kept (no Hebrew data ships).

**The hire and buy lists** (Q384): `ParkHireScreen` and `ParkBuyScreen` set `ColumnAligns` Start, End, which
`UiList`'s default already gave, and their shape lines end `aligned Start End`. The stock park, paused, the buy screen
by the gadget's button and the hire screen by its `b_allstaff` (`q384/run-fix1`, the final game binary): four buy rows,
the names from 0.000 to 0.016 of their column, the prices ending 0.969 to 0.979; three hire rows, the names from 0.000
to 0.005, the wages ending 0.966 to 0.974. Main's build (`run-control1`) measured the same to the third place. The
original at 800 x 600 (`q384/orig/b`, `q353/grades.TPWS`): the names from 0.000 to 0.010, every price ending 0.951 and
every wage 0.968, a "$ " before each (Q158's "From Q238"; `q384/fix1-lists.png`, `orig/a-lists.png`). In no game run:
Full Simulation (not asked: the screens are either mode's), the other three themes.

#### The visitors rows: a number, a money cell, a bar

Read 2026-10-14 (Q371). The visitors opener `FUN_00493530` hands `FUN_006639cb` the factory `FUN_004938c0` and the
styler `FUN_004939d0`, which makes the six cells, each in the font `[0x007508b4]`:

| Column | Cell | Alignment | The adder `FUN_00493800` hands |
|---|---|---|---|
| Visitor Number | `FUN_00490c30` (vtable `0x006ff470`, setter `0x00490dc0`): UITEXT `0x1c9` (457, "New Visitor") at nought, else `"%d"` | `FUN_0065c428( 1, 1 )` | `+0x1d8` |
| Cash Remaining | a money cell `FUN_00490250` ("The all-items rows", above) | `( 2, 1 )` | `+0x1a0` |
| Time In Park | `FUN_00490e60` (vtable `0x006ff4a0`, setter `0x00490ff0`): by hours, below | `( 1, 1 )` | `FUN_004fd950`'s stay through `FUN_004f8730`, `__alldiv` 36,000,000,000: hours |
| Rides Ridden | the number painter `FUN_0048fde0` | none set | `+0x1c4` |
| ? | a type-6 cell wearing the mesh filed as `ti_hungry`, `thoughts.MD2`, inset `( 4, 0 )`: below | | `+0x30` less 1, or 999 at nought |
| Happiness | a bar in `happygrad.wct` (`FUN_00477ff0`), inset `( 0, 8 )` | | `((__ftol( +0x19c ) & 0xff) << 10) / 100` (`0x00493870`) |

`FUN_0065c428( across, down )` posts `0xc00` with the two bytes to the cell's text and skin ("How a list's cells are
aligned", above: 0 left, 1 centre, 2 right). **Measured in the original's frame as well** (`q371/measure.py`'s `aligns`, the lettering's extent in
its column): the number centred (its middle 0.495 of the column), the cash to the right (its end 0.976), the rides from
the left (its start 0.032), as `( 2, 1 )` reads right on the park status screen above.

**The thought** (read 2026-10-15, Q373). The factory `FUN_004938c0` makes the row's fifth cell a **type 6** control
(`FUN_0065dd7b( 6, 1, 4, ... )` at `0x0049399b`: `FUN_0066d9e0`, vtable `0x00705c58`), where the others are type 1. Its
value setter (vtable `+0x1c`, `0x0066da4d`) keeps the value at `+0x13c`, hands it to the text and the skin as the **state
word** (flag 1, data `+4`), then calls the base's (`0x00661525`, flag `0x80` alone, which a mesh skin passes over). The
styler's `FUN_00659bc9( "ti_hungry" )` clones a template from the list `[0x00faa610]`: `FUN_00489060` files every
`ui.wad` model whose name has no `!` under its first node's name as a mesh skin (`FUN_00476a90`, vtable `0x006fee10`),
so `ti_hungry` is **`thoughts.MD2`**, seventeen nodes, each wearing its own `ti_*.wct`:

| Part | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 | 15 | 16 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Node | hungry | thirsty | hungthir | toilet | good | costhigh | litter | superhappy | happy | normal | sad | bored | angry | sick | scared | queuetoolong | confused |

(each `ti_` and the node's name; `ti_costhigh` wears `ti_bad`, `ti_superhappy` `ti_suhappy`, `ti_queuetoolong`
`ti_queuebad`). The skin's `+0xc` (`0x00476f10`, `0x00476e80`) shows the node its state and frame (`+0xa`, nought here)
count to down the model's node list and hides the one before (`|= 0x8010`); a count past the last node shows nothing.
So thoughts 1 to 17 show parts 0 to 16, and 18 to 22 and none (999) show nothing. `FUN_00485e30( cell, 4, 0 )` is
`FUN_0065a43e( 4, 0, -4, 0 )`: four units in from the left and right, none from the top and bottom. The two-second
rewrite hands the same value (`0x0049330c`) and, as for every column, moves no row.

Measured in the original under Proton on `q373/thought.TPWS` (`q371/visitors.TPWS` with ten guests' `mLastThought`
written at the record's `+386`; `q373/orig/a` and `b`, `thingwords.py` on `+0x30`): 0.58 s after V the ten cells were
confused, bored, hungry, bad, thirsty, none, bored, toilet, hungthir and bad, each its guest's polled thought less one
(18 none), four of them already moved off the file's by its running park (`lib/texmatch.py`, the icon's pixels against
the 17 textures). The "?" heading's click sorted on the thought as it stood then: a guest whose thought went from 3 to 6
after the click stood between the thirsty and the toilet rows, drawn bad from the next rewrite on (`q373/orig/b`).

**The time in park** (read 2026-10-14, Q372). The stay is `FUN_004fd950`: `mGameTick` less the guest's `+0x1d4`, taken
unsigned and shifted right by two (`SHR` at `0x004fd963`). `FUN_004f8730` on the park calendar (world `+0x2a0`, its
`+0x1c` `mFunnySecsPerRealSec`, 15000) makes it 100 ns units: `__allmul( rate, stay )`, over four, `__allmul` by
10,000,000, a 64-bit product that wraps; the adder `__alldiv`s that by 36,000,000,000, signed (`0x0049383e`), and keeps the
low half: whole hours. **The two-second rewrite (`0x00493270`) shifts the stay back left by two first** (`SHL EAX, 2` at
`0x004932db`), so from the first rewrite on a row letters four times the hours its fill did; an arrival's row (`0x1c`,
`FUN_00493c50`) goes through the adder, on the tick the guest is stamped. The setter `0x00490ff0` (on a message with
`0x80`) letters through `FUN_006acd60` on `[0x0078ba94]` with one `VARM` variable, whose type word is the string's
parameter number:

| Hours | UITEXT | Parameter |
|---|---|---|
| past 48 | `0x1bb` (443, "{8} days") | 8: the hours / 24 (`MUL 0xaaaaaaab`, `SHR 4`) |
| past 24 | `0x1ba` (442, "{8} day") | 8: the hours / 24, so 48 hours is "2 day" |
| else | `0x1b9` (441, "{11} hours") | 11: the hours, a negative one `NEG`'d with the variable's sign word set |

The variable's writer (`0x006add40`, its vtable `0x006ff170` slot 0) prints the value word `"%d"` and never reads the
sign word, so a negative count letters its size. **A guest stamped after the clock** has a stay near 2^30: the adder
letters 93,661,950 hours four sweeps ahead ("3902581 days"), and the rewrite's four times that stay wraps past the sign,
-137,761,755, lettered "137761755 hours". The list sorts the column on the signed hours.

Measured in the original under Proton on `q372/time.TPWS` (`q371/visitors.TPWS`, tick 963, with eight guests' `+398`
written; `q372/orig/b`, `thingwords.py` polling its clock): 0.58 s after V, at its tick 1056, the ten rows read the
adder's words exactly ("4 days", "1 day", "1 day", "2 day", "3 days", "3 days", "9 days", "3902538 days", "23 hours",
"4 days"); 7 s on, the rewrite's at 1080 ("18 days", "5 days", "9 days", "9 days", "13 days", "13 days", "39 days",
".37765797 hours", "5 days", "18 days": a nine-digit count clipped at both ends of its cell); the Time In Park heading's
click sorted the rewrite's numbers, the wrapped guest first. The time is centred (0.317 to 0.65 of its column).

Measured in the original under Proton on `q371/visitors.TPWS` (`q353/grades.TPWS` with five guests' happiness written
at the guest record's `+422` and thing 42's `mArrivalIndex` at `+402` written 0; `q371/orig/b`, `thingwords.py` on
`+0x19c`): thing 42 letters "New Visitor" and sorts first, visitor 1 "$ 306"; its ten bars measure 0.5, 0.125, 0.3,
0.758, 1.0, 0.792, 0.525, 0.558, 0.583 and 0.5 of their width against its polled happiness's bars 0.5, 0.119, 0.289 to
0.31, 0.75, 1.0, 0.789, 0.52, 0.55, 0.569 to 0.59 and 0.5 (`barsplit.py`). A guest's happiness moves as its park runs:
run a, unpolled, read four bars off the file's by 0.06 to 0.14.

**OpenTPW** (Q371): `ParkVisitorsScreen.RowOf` and `NumberText`, the list's `ColumnAligns`, `BarColumns`, `BarSkins`
and `BarInset` 8. Each fill logs `Visitors list: row <thing> <number>: <cash> <rides> <happiness bar>`, and each
rewrite `N rows rewritten, row 0` the same way (the time added since, below). On the same file, paused at the load (`q371/run-fix1`, the final game
binary): `row 42 New Visitor: $ 684 0 512`, `row 38 1: $ 306 1 122`, ... `row 37 4: $ 570 0 1024`, sorted 42 first;
its bars 0.5, 0.125, 0.33, 0.902, 1.0, 0.714, 0.527 and 0.5 (the file's happiness); the number centred (0.497), the cash
right (0.974), the rides left (0.034) (`q371/side-by-side.png`: the original, this, main). Main's build (`run-control2`)
lettered 0 and the cash and happiness as bare numbers, every one from the right, and drew no bar. The rows are 56 units
apart, as the original's 55.8 (Q377, "The rows' font and height"). **Not built**: the first row picked as the list fills
(Q232); the scrollbar.

**OpenTPW** (Q372): `Peep.StayAt`, `ParkVisitorsScreen.HoursIn` and `TimeText`; `RowOf`'s `rewrite` shifts the stay.
The log's row reads `<cash> <time> (<hours> h) <rides> <bar>`. On `q372/time.TPWS`, paused at the load's tick 964
(`q372/run-fix3`, the final game binary): the fills `row 42 New Visitor: $ 684 3 days (79 h) 0 512`, `38 ... 5 hours
(5 h)`, `40 23 hours`, `34 1 day (25 h)`, `37 2 day (48 h)`, `36 2 days (50 h)`, `41 8 days`, `33 3902537 days
(93660909 h)`, `32 0 hours`; the rewrite's row 0 `13 days (316 h)`; the heading's click `sort 3`, `33 7 -137765918`
first, then 0, 20, 95, 100, 195, 200, 316, 316, 320, 325, 329, 833 hours (`2-by-time.png`). Main's build lettered no
time and counted `VISITOR_TIME_IN_PARK` and the sort. This game's smaller lettering fits the nine digits in the cell.

**OpenTPW** (Q373): `UiList.MeshColumns` and `MeshInset` (a mesh cell framed by the row's number for its column, nothing
past the mesh's last part), `ParkVisitorsScreen.ThoughtsMesh` and `ThoughtPart`; the log's row reads `... <rides> part
<part> <bar>`. On `q373/thought.TPWS`, paused at the load's tick 964 (`q373/run-fix1`, the final game binary): the fills
`row 42 New Visitor: ... 0 part 16 512`, `38 ... part 999`, `40 part 0`, `34 part 13`, `37 part 1`, `36 part 17`, `41 part
8`, `33 part 3`, `32 part 10`, `35 part 5`; the frame's cells confused, none, hungry, sick, thirsty, none, happy, toilet,
sad, bad and none on the last three; the "?" heading's click `sort 5`, `40 37 33 35 41 32 34 42 36 38 39 29 31`, drawn
hungry to confused, then five without (`q373/side-by-side.png`: the original's sorted column, then this game's). Main's
build (`run-control1`) drew no picture and counted `VISITOR_COLUMN_117_UNNAMED` and `VISITOR_SORT_ON_UNBUILT_COLUMN`.

#### The rows' font and height

Read 2026-10-15 (Q377; the hire and buy lists Q378). **The five lists' rows are lettered in font slot 7**, and their height is
the row factory's:

| List | Factory | Height | Kept at | Styler, every cell in |
|---|---|---|---|---|
| visitors | `FUN_004938c0` | `0x38`, 56: font `[0x007508b4]` is measured and its answer thrown away (`0x004938fa` writes 56 whatever it was) | `[0x007ca140]` | `FUN_004939d0`, font `[0x007508b4]` |
| all staff | `0x00496c10` (no function) | font `[0x00751184]`'s height x `0x600` / `[0x00faa5c0]` + 6 | `[0x007ca170]` | `FUN_00496d20`, font `[0x00751184]` |
| all items | `FUN_004944d0` | font `[0x00750d54]`'s height x `0x600` / `[0x00faa5c0]` + 6 | `[0x007ca158]` | `FUN_00494690`, font `[0x00750d54]` |
| hire | `FUN_0049bbb0` (the opener hands `0x0049c870`, a jump to it) | font `[0x007523dc]`'s height x `0x600` / `[0x00faa5c0]` + 6 | `[0x007ca314]` | `FUN_0049c880`, font `[0x007523dc]` |
| buy | `FUN_004ad240` | font `[0x00755134]`'s height x `0x600` / `[0x00faa5c0]` + 6 | `[0x007cc200]` | `FUN_004ad340`, font `[0x00755134]` (the third cell a tick-box mesh) |

The five font words are 7 and only read (the heading's word beside each is 8, Q370), and the rule is the park-file screens' own
(`FUN_0049f0b0`, `saves.md`). The list takes the height as handed (`FUN_00663718`: `+0x170`) and fits
`(bottom - top) / height` rows (`+0x172`, again in `FUN_00664ba8`), as `UiList.VisibleRows`. `[0x00faa5c0]` is the
screen's height and `[0x0078b534]` the font set (`FUN_00485a70`: `0x0078b54c + (slot + set x 13) x 0x18`).

**Measured in the original under Proton** (`q377/orig/a`, `q373/thought.TPWS` loaded, V, S, A, read with `tpwmem.py`):
`[0x00faa5c0]` 480 and `[0x00faa5c4]` 640, set 1, so **the reference original draws at 640 x 480**, its frames scaled to
1024 x 768; `[0x007ca140]` 56 once V is up, `[0x007ca170]` 41 once S is, `[0x007ca158]` 41 once A is (GAME8.bf4, 11 tall:
11 x 1536 / 480 + 6), `[0x007cb24c]` 41. Its frames: the visitors rows 27.9 pixels apart (55.8 units), ten shown; the
items rows 20.4 to 20.6 (41 units). At its own 800 x 600 and 1024 x 768 (`Data/Resolution.sam`, `q377/orig/c` and `b`): `[0x00faa5c0]` 600 and
768, sets 2 and 3, and the staff, items and Load Park heights 39 and 34 (GAME9.bf4, 13; GAME10.bf4, 14), the visitors'
56 in both; the rows 56 units apart in both frames.
At 800 x 600 with `q353/grades.TPWS` (Q378, `q378/orig/a`): `[0x007cc200]` and `[0x007ca314]` 0 after the load, 39 once
the gadget's Buy and then `b_allstaff` built each screen; the frames' wages and prices 38 and 39 units apart
(`listcells.ink` on the number column: a name's descender moves a band's middle).

**OpenTPW** (Q377): `UiList.RowHeightFor` and `RowHeightOf( font )`, the font of the current set; the staff and items
lists `RowFont` 7 at `RowHeightOf( 7 )`, the visitors `RowFont` 7 at `0x38`. At this game's 1280 x 720 window (set 2,
font 7 GAME9.bf4, 13 tall) that is 39 units, where the original at 640 x 480 has 41: the same rule at another size.
Each screen logs `<list>: rows of H units in font F, N visible` as it is built. In the stock park with every pool
candidate hired, paused (`q377/run-fix1`, the final game binary): `Visitors list: rows of 56 units in font 7, 10 visible`,
`Staff list: rows of 39 units in font 7, 9 visible`, `Items list: the 5-column shape, rows of 39 units in font 7, 11
visible` and the 6- and 2-column shapes the same; the frames' lettering 55.45 units apart in ten visitors rows and 38.4
in the four handymen's and the eight misc rows (`listcells.ink`), 1.17 to 1.25 times main's lettering in height; the
pointer at a slot's middle by the new height picked slot 2 in all three. Main's build (`run-control1`) lettered thirteen
visitors rows 44.8 apart, all in font 6.
The hire and buy lists the same (Q378): `RowFont` 7 at `RowHeightOf( 7 )`, logged `Hire list: rows of 39 units in font 7,
12 visible` and `Buy list: rows of 39 units in font 7, 11 visible` on the stock park (`q378/run-fix1`, the final game
binary); the three candidates' wages and the four rides' prices 38.4 units apart, 1.11 times main's lettering in height,
and the pointer at slot 2's middle by 39 picked candidate 8 and ride 1101. Main's build (`run-control2`): 44.8 apart.
No list takes `UiList`'s default of 44 now.

#### The list headings' frames

Read 2026-10-14 (Q370). **Every list's opener letters and frames its headings through `FUN_00486660`**, as the hire and
buy lists' ("A list's order", above): the label skin, the nine-slice frame `!cbut`, the text centred both ways, the font
slot and colour handed over and the help row at `+0x90`. The all-staff opener `FUN_00496620`, the four all-items tab
builders (`FUN_00494ec0` rides, `FUN_00494c70` shops, `FUN_00494250` sideshows, `FUN_00495110` misc) and the visitors'
opener `FUN_00493530` each hand font slot 8 from a word of their own that nothing writes (`[0x00751188]`, `[0x00750d58]`,
`[0x007508b8]`) and the colour 255, 239, 0. Only the help rows differ (`UIHELPTEXT.str`, each "Left-click to sort the
list by ..." its column):

| List | Help rows |
|---|---|
| all staff | `0x77` to `0x7b` (119 to 123) |
| visitors | `0x81` to `0x86` (129 to 134) |
| items, rides | `0x69` to `0x6d` (105 to 109) |
| items, shops | `0x69`, `0x248` to `0x24b` (105, 584 to 587) |
| items, sideshows | `0x69`, `0x248`, `0x6b`, `0x249` to `0x24b` (105, 584, 107, 585 to 587) |
| items, misc | `0x69`, `0x24c` (105, 588) |

Measured in the original under Proton (`q369/orig/b-1-handymen.png`, `q367/orig/a-*.png`, `q370/orig/a-visitors.png`
and `a-misc.png`, `rectcolour.py` on each heading's rectangle): teal 0.80 to 0.985 of each, yellow lettering in each.

**OpenTPW** (Q370): `UiList.AddFramedHeading`, `HeadingFont`, `HeadingColour`; the items screen sets each tab's help
rows with its headings' text. Each opening logs `Staff list: headings` or `Visitors list: headings`, and each items tab
`Items list: headings of tab N:`, every heading as `mesh font help "text"`. On the stock park (`q370/run-fix2`, the final
game binary): `Staff list: headings !cbut 8 119 "Name", ... !cbut 8 123 "Happiness"`, the visitors' `129` to `134`, and
the four tabs' rows as the table; all 29 headings teal 0.854 to 0.99 with yellow lettering and no white
(`q370/side-by-side.png`). Main's build (`run-control1`) lettered them white on black. **Not built**: the original's
lettering is larger and wraps "Monthly Wage" where this game's fits one line of its smaller font (the park screens'
lettering, a deviation Q352 filed); the all-items headings sort (Q374), and the all-staff ones (Q381).

### The node name is not the file name — measured

The stream hashes the model's first **node** name,
while the loader opens `ui/<file>.md2`. Where an artist named the two differently they diverge, and a
name taken from the hash table simply does not load.

Listed out of `ui.wad` itself rather than inferred:

    buyitem   -> f_buyitem        hirestaff -> list_hirestaff     balance  -> f_balance
    staffinfo -> f_staffinfo      staffpic  -> f_staffpic
    b_sride   -> b_srides         b_sresrcher -> b_sresrhcer

**`b_sresrhcer.MD2` is misspelled in the archive** — the h and c transposed — while its own textures are
spelled `b_sresrcher.wct`. That one cannot be guessed at from the hash or from the texture beside it.

The rest of the set (`b_sshop`, `b_sshow`, `b_sfeature`, `b_shandy`, `b_smech`, `b_senter`, `b_sguard`,
`b_scroller`, `b_allstaff`, `b_allthings`, `i_boxtick`, `!frame`, `!slider`) load as-is, because there
the node and the file happen to agree. **`b_up` and `b_down` agree too**: each is the first node of the `.MD2` of its own name, no other
mesh node in `ui.wad` hashes the same, and the scrollbar's two buttons wear them (buy, `0x0075507a` and `0x00755056`).

Read the archive with the `wadcat.py` harness (`<wad> list`; the harness folder is in `CLAUDE.local.md`) — a port of this tree's own
`WadArchive` and `Refpack`, validated against two values the codebase documents independently.

### The gate that looks like a bug, and is not

`FUN_004a0940` does nothing when `FUN_0048c8d0()` returns 1, which needs both `DAT_007c2534` and
`FUN_006ad810()` non-zero. Both halves are named:

- `DAT_007c2534` is the in-game **Escape menu object** — a 0x14-byte MenuList built by
  `GameMenu_BuildPark` / `GameMenu_BuildLobby`. Non-zero means the menu exists.
- **`FUN_006ad810` is not a money function at all.** It is a two-byte COMDAT-folded accessor,
  `MOV EAX,[ECX+0xc]; RET`, shared by unrelated classes across 29 references — its meaning is entirely
  the receiver's. Here the receiver is the MenuList, so it is `MenuList::IsShown()`.

So **`FUN_0048c8d0()` means "the game menu exists AND is on screen"**, and the buy button refusing
while the pause menu is up is correct behaviour rather than an unbuilt path. The same function on the
bank Thing is what reads the balance.

**A caveat about the teardown**: several menu arms do NOT clear
`DAT_007c2534` — those that open a confirm box leave it set until that box's own callback runs, so the
menu object outlives the menu on screen.

### What the buy list actually filters on

The list is built from a per-item descriptor array at `world+0x1d943c`, stride 0x20, 150 slots. The
four filter fields, as **byte** offsets:

    +0x508  the item's model failed to load
    +0x4AC  Info.WhichUIType      (4 = "not shown in the UI", the files' own words)
    +0x284  AddOn.UpgradesId      (must be 0 - a standalone item, not a ride's upgrade)
    desc+0x10  researched/available

`desc+0x10` is not a static "is this listed" flag - it is the **researched** flag, and at a park load it is the
**save's**, not the item file's (Q201). The record array is `CControlManager`'s, 150 records of 0x20 bytes looked up by
`FUN_004d3d10`, and it is saved and loaded raw as `mObjectControls[150]` (`FUN_004d3aa0`). Its writers, in the order a
park load meets them:

1. **World setup** (`FUN_005156a0`): the array is zeroed (`FUN_004d3de0`), then `FUN_004d3e00` appends one record per
   catalogued item with `+0x10` = `Upgrades[0].CostOfResearch` (`item+0x180`) is nought (`0x004d3e92`). The research
   manager's setup (`FUN_00502d60` -> `FUN_005031a0`, from `0x00515d2f`) sets `+0x10` to 1 again for every standalone
   item with `Info.WhichUIType` below 4 whose cost is nought (`0x0050320a`) and enrols the rest as research candidates.
2. **The park's save** (mode 2, `FUN_00414d40` -> `FUN_00415270` -> `FUN_005179c0`, the read at `0x005181e7`) lays the
   saved array over all of it. `FUN_00415140` then runs `FUN_004d3e00` again, which appends a record only for an item
   the save has none for; an item the save holds keeps the save's byte.
3. **Research completing** (`FUN_00504630`) sets `+0x10` to 1 (`0x005046a1`) and `+0x14` to the upgrade tier reached;
   `FUN_00504580` steps `+0x14` past a tier whose research costs nothing, so `+0x14` is the tier researched, 0 to 2.

**Measured in `Easymode.TPWI`** (`q201/ctrldump`, all 50 records): `+0x10` is 1 on 26, exactly the 26 whose item's own
file, with its `Easy_` layer, sets `Upgrades[0].CostOfResearch` 0 - the two rules agree in the shipped park, and differ
only in a played save, where research has set more. Among them rides Belly Bounce, Crazy Ape, Rocky Racers, Aztec
Mayhem; shops Balloon, Burger, Drinks; sideshows Jungle Spray, Strength Bird. **Photographed in the original's Lost
Kingdom** (Instant Action, under Proton, 2026-09-30, Q178b): exactly those, and features Buy Land, Clear Land and the
eight whose cost is nought, and no mystery row. **OpenTPW lists the same** (Q201b, `ParkResearch`: the save's
records, an item it lacks seeded from its cost; `q201b/run2/`: rides 4, sideshows 2, shops 3). Research completing is
not built; the points a researcher hands the lab are counted, `RESEARCH_POINTS_TO_THE_LAB` (`ride-operation.md`,
"The research, in both games").

**The row state is {0,1,2} and both non-zero values are pinned**: 1 = you already own at least one
(`desc+0x18`, incremented on placement and decremented on demolition), 2 = one of the three most
recently **RESEARCHED** items in that tab — a 3-entry ring per tab fed only by the research-complete
message, **not** by building. The game's own help row 146 says the same thing: "sort the list by items
already owned or recently researched".

**The mystery row** is inside the researched test, never an unresearched item's (`0x004ab023`): a researched item
whose `UsageInfo.GoldenTicketCost` (`item+0xC4`, a dword; `Research.Group` is `+0x178`) is above nought and whose id
the player has not unlocked (`FUN_004d4b70`, `0x004ab047`) is named UITEXT 137 "??? Mystery Ride! ???" and priced at
**the negated ticket cost** (`0x004ab050`..`0x004ab063`). The unlocks are `PlayerProgress`'s set of item ids
(`DAT_00f7c58c`, built lazily by `FUN_00409410`; `FUN_005af780` finds an id), inserted only by the purchase
(`FUN_004db090` -> `FUN_004d4ad0`) when the tickets suffice, the cost added to the tickets spent; in game type 1 it
inserts nothing. The set is the player's, saved in `gms.dat` (`FUN_005aff50`; `saves.md`, the "ride ids" rows),
across every park and theme. No researched item in Instant Action's Lost Kingdom has a ticket cost, which is why the
original showed no mystery row. OpenTPW draws the row from the current player's `RideIds` (none with nobody playing); buying one
with tickets is not built, counted `MYSTERY_RIDE_PURCHASE`.

**Buy Land is item 101 and Clear Land is 102**, both `WhichUIType` 4 — which is exactly *why*
`FUN_004aaf70` appends them as synthetic rows **-1** and **-2** on tab 3 rather than finding them in the
walk. The Buy Land price is `Costs.MapCell`; the executable ships no default for it and the four
sibling cell prices, so they are all zero in the image and filled from the balance file.
