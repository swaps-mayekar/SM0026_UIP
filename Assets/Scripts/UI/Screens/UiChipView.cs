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
        }
    }
}
