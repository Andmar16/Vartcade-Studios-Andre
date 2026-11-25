# Ten Days to Sundown: UI/UX Wireframes

## Overview
Colored wireframes for the Unity version, made so the development team (Chua, Esteban) can see what the design team (Barros, Villa) has in mind before building. Each version lives in its own folder and is never edited after review; changes go into the next version.

| Version | Date | Status | Contents |
|---|---|---|---|
| [v0.1](v0.1/WIREFRAMES.md) | 2026-11-21 | Initial idea draft, waiting for review (checklist task W19) | 10 boards: read me, Main Menu, Lobby, Day table, Night table, Waiting, Cards, Rondo, Morning recap, Sundown results |

## Goals
- Every screen drawn before it is built, with numbered notes that say what each element does, which script owns it, its priority and its motion.
- One shared look: the merged palette, fonts and placeholder puppets below.
- Phone landscape first; PC uses the same layout, wider.

## Folder structure
```
Docs/assets/wireframes/
├── README.md            (this file)
└── v0.1/
    ├── WIREFRAMES.md    (all developer notes, board by board, with images)
    ├── png/             (one image per board, for quick viewing)
    └── html/            (the source of each board; open in a browser)
```

## Important decisions (from the team, Nov 21)
- **Platform:** phone and PC; design phone landscape first (reference 1920x1080, Canvas Scaler match 0.5, safe area for notches).
- **UI build:** hybrid; layout and art in prefabs and sprites, behavior stays in code (keep `IGameView`).
- **Look:** 60 Seconds!, Sally Face, Fallout Shelter, Fran Bow. Board framing like UNO (digital) and Buckshot Roulette (2.5D seated table).
- **Characters:** cutout puppets (Kitty Powers' Matchmaker style). Placeholder big and thin bodies for now; avatar creation is later and not the focus.
- **Cards:** illustrated; hover (PC) or long-press (phone) shows what a card does, like UNO.
- **Not your turn:** own waiting screen with a timer; Night shows a full blackout.
- **Backgrounds:** animated, layered environments for the menu and the game table.

## Merged palette (for the UITheme asset)
| Token | Hex | Use |
|---|---|---|
| Sky top / mid / glow | `#0B0E1A` / `#1A1C33` / `#6B331E` | Backgrounds only (sky gradient, card backs) |
| Sun | `#F28C3A` | Sun and warm light; not for text |
| Charcoal | `#13191D` | Panels, outlines, dark text |
| Brick | `#B53B32` | Primary buttons (cream text passes, 7:1) |
| Gold | `#E5A836` | Your turn, highlights |
| Orange | `#D9822B` | Day phase, Rondo, warnings (charcoal text) |
| Dark red | `#632B29` | Danger, losses, Doomsday |
| Teal | `#4E7C86` | Night, jumpsuits; cream text on it only for large text (3.8:1) |
| Teal dark (new) | `#3B5F67` | Night buttons with small cream text (5.6:1) |
| Cream | `#F4E9D8` | Text on dark, card faces |

Fonts: Anton (headings, buttons), Roboto (body, HUD), Special Elite (flavor text only; not yet in the project, add it from Google Fonts under the OFL license).

## Open questions that change these boards
- D-Q1: what happens when the turn timer reaches zero (drawn as: turn ends; a Peek is done for you at Night).
- D-Q2: is a Night thief ever revealed (drawn as: never).
- D-Q3: does Sundown reveal every seat (drawn as: yes).
- D-Q4: is your own character visible at the table (drawn as: no; a portrait in the HUD).

## Workflow
1. Design team edits the wireframes on the shared canvas.
2. When a version is reviewed, export it into a new `vX.Y` folder here (PNG + HTML + notes) and add a row to the table above.
3. Developers build from the newest reviewed version and reference note numbers in commits (e.g. `03-7 KindsTracker`).
