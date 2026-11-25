# Environment v3.2 (Nov 26): the picked look

Andre's picks from the Environment Drafts canvas, combined and redrawn by Claude. Not in the game yet:
these are the approved-direction drafts, built in layers so the same pictures can go into Unity (checklist E05-E08).

## What was picked
- **Room:** R-B (Filipino home bunker) with R-A's steel shelf, poster and pipes and R-C's portrait, children's
  drawings and peeling wallpaper. Spider web, crows on the wire, tally marks, card-art labels (Millenium Tuna,
  Peak, First Aid), the shelf empties over the 10 days.
- **Outside / menu:** M-A Filipino street (sari-sari store, jeepney, rooster, Silong billboard, houses with laundry,
  water tank on a roof platform, bunker hatch).
- **Props:** P01-P20, redrawn (proper jeepney and tandang).  **Motion:** A01-A10.  **Light:** L1-L5 built in.
- **Pixel details:** X01-X12 at 3 px cells (1080p).  **Overlays:** OV3, OV7, OV8 + OV10 (new warm balance).
- **Rondo:** each of the 8 results changes both scenes.

## Style rules (v3.2)
- Thin, sketchy ink (2-3 px at 1080p) with a faint second pass; grime, drips, rust and paper grain.
- Paper cut-out 2.5D (Paper Mario feel): every object has a pale paper edge and a soft shadow on the surface
  behind it; seated characters are turned slightly toward the middle; print-dot (halftone) shading in dark areas.
  Things painted or scratched on a wall (web, crack, tally, stains) get no paper edge.
- Cast: dark grey long-sleeved top by default; brass paper-fastener at the shoulder joint.
- Table: smaller (far edge y = 650 at 1080p, x 420-1500), banig mat in the middle, scratches, rings, spill, candle.

## Files
- `renders/` room and street for Day 1 / 5 / 9, Night and each Rondo result (1280x720 JPG previews).
- `props/` P01-P20 as transparent PNGs (400x300).
- `src/` the Python that draws everything (SVG drawn in code, rendered with cairosvg, textures in numpy).
  Paths inside point at Claude's workspace; change `OUT`, `CAST` and `ROOT` to rebuild elsewhere.
  `python3 room.py d1 night tornado` renders states; `python3 room.py hero` exports the moving layers.

## Canvas
"10 Days to Sundown: Environment Drafts" (claude.ai artifact), right-hand v3 column: THE GAME (20 s loop),
Main menu, Dial + wheel, Room/Street (moving), states, props, pixel, overlays, cast.
