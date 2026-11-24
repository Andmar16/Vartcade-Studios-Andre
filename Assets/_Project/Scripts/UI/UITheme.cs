using UnityEngine;

namespace TDTS.UI
{
    /// <summary>
    /// The merged palette from the UI/UX wireframes (v0.1, Nov 21). New screens take their colours
    /// from here so the whole game keeps one look.
    /// </summary>
    public static class UITheme
    {
        // Backgrounds only (sky gradient, card backs)
        public static readonly Color SkyTop = new Color32(0x0B, 0x0E, 0x1A, 255);
        public static readonly Color SkyMid = new Color32(0x1A, 0x1C, 0x33, 255);
        public static readonly Color SkyGlow = new Color32(0x6B, 0x33, 0x1E, 255);

        public static readonly Color Sun = new Color32(0xF2, 0x8C, 0x3A, 255);       // warm light, never text
        public static readonly Color Charcoal = new Color32(0x13, 0x19, 0x1D, 255);  // panels, outlines, dark text
        public static readonly Color Brick = new Color32(0xB5, 0x3B, 0x32, 255);     // primary buttons (cream text)
        public static readonly Color Gold = new Color32(0xE5, 0xA8, 0x36, 255);      // your turn, highlights
        public static readonly Color Orange = new Color32(0xD9, 0x82, 0x2B, 255);    // Day, Rondo, warnings (charcoal text)
        public static readonly Color DarkRed = new Color32(0x63, 0x2B, 0x29, 255);   // danger, losses, Doomsday
        public static readonly Color Teal = new Color32(0x4E, 0x7C, 0x86, 255);      // Night, jumpsuits (large cream text only)
        public static readonly Color TealDark = new Color32(0x3B, 0x5F, 0x67, 255);  // Night buttons with small cream text
        public static readonly Color Cream = new Color32(0xF4, 0xE9, 0xD8, 255);     // text on dark, card faces

        public static readonly string GoldHex = "#E5A836";
        public static readonly string CreamHex = "#F4E9D8";

        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
