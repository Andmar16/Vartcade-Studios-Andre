# cards

Card illustrations: the team's originals and their vector versions.

## Plan (checklist A21)
1. The design team puts the card illustrations in the reference pack under
   `99_Our_Own_Art/cards_fronts` and `cards_backs` and sends the zip to Claude.
2. Claude vectorizes each one: clean line art with an even outline weight, flat
   colors snapped to the palette (`../palette`), one shadow tone per color.
3. The artists review; small fixes go in `_v2` files.
4. PNGs are exported at 2x into `Assets/_Project/Art/Sprites/Cards/` for Unity.

## Folders (created when the first files arrive)
```
cards/
├── originals/   the team's illustrations as received (never edited)
├── vector/      SVG masters, one per card side
└── export/      PNG at 2x, the files Unity uses
```

## Names
`card_<kind or action>_<front|back>[_v2].svg`, for example `card_food_front.svg`,
`card_reverse-turn_front.svg`, `card_flashlight_back.svg`, `card_key_back.svg`.

## Must stay true (game rules)
- The Bunker Key's FRONT must look exactly like a Supply front.
- Each Supply kind shows its shape symbol (Food circle, Water drop, Medicine plus,
  Entertainment diamond) so colorblind players can tell them apart.
- Art must read at 46 px wide (opponent hands): bold shapes, little fine detail.
