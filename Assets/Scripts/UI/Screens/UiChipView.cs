using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIP.UI
{
    public sealed class UiChipView : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text label;
        [SerializeField] Image background;

        public void Wire(Button btn, TMP_Text text, Image bg)
        {
            button = btn;
            label = text;
            background = bg;
        }

        public void Bind(string text, bool selected, System.Action onClick)
        {
            if (label != null)
            {
                label.text = text;
                label.color = selected ? UiTheme.OnAccent : UiTheme.Text;
            }

            if (background != null)
            {
                background.color = selected ? UiTheme.Accent : UiTheme.BgCardAlt;
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClick != null)
                {
                    button.onClick.AddListener(() => onClick());
                }
            }

            SizeToText();
        }

        public void SizeToText()
        {
            if (label != null)
            {
                label.enableWordWrapping = false;
                label.overflowMode = TextOverflowModes.Overflow;
                label.ForceMeshUpdate();
            }

            var layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.flexibleWidth = 0;
            layoutElement.minHeight = 32;
            layoutElement.preferredHeight = 32;
            if (label == null)
            {
                return;
            }

            const float HorizontalPad = 20f;
            var width = Mathf.Max(48f, label.preferredWidth + HorizontalPad);
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
        }
    }
}
