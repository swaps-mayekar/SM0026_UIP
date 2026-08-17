using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public static class UiFonts
    {
        public const string TitleFontResourcePath = "UI/Fonts/Inter_28pt-Black SDF";

        static TMP_FontAsset titleFont;

        public static TMP_FontAsset TitleFont =>
            titleFont ??= Resources.Load<TMP_FontAsset>(TitleFontResourcePath);

        public static bool IsTitleOrButtonText(TextMeshProUGUI tmp)
        {
            if (tmp == null)
            {
                return false;
            }

            var name = tmp.gameObject.name;
            if (name == "Title" || name == "Heading" || name.EndsWith("Title"))
            {
                return true;
            }

            if (tmp.GetComponent<Button>() != null)
            {
                return true;
            }

            return name == "Label" && tmp.GetComponentInParent<Button>() != null;
        }

        public static void ApplyTitleFont(TextMeshProUGUI tmp)
        {
            var font = TitleFont;
            if (tmp == null || font == null)
            {
                return;
            }

            tmp.font = font;
            tmp.fontStyle = FontStyles.Normal;
        }
    }
}
