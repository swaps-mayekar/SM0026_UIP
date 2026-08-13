using UnityEngine;

namespace UIP.UI
{
    public static class UiTheme
    {
        public static readonly Color Bg = Hex("0B1C33");
        public static readonly Color BgElevated = Hex("122846");
        public static readonly Color BgCard = Hex("17345A");
        public static readonly Color BgCardAlt = Hex("1C3F6B");
        public static readonly Color Stroke = Hex("2B527F");
        public static readonly Color Text = Hex("F4F7FB");
        public static readonly Color TextMuted = Hex("A9BDD6");
        public static readonly Color Accent = Hex("F5C542");
        public static readonly Color AccentDark = Hex("D4A017");
        public static readonly Color Danger = Hex("FF6B6B");
        public static readonly Color OnAccent = Hex("1A1300");

        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
            {
                return color;
            }

            return Color.magenta;
        }
    }
}
