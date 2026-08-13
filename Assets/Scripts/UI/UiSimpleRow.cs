using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public sealed class UiSimpleRow : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text subtitle;
        [SerializeField] TMP_Text body;
        [SerializeField] TMP_Text badge;
        [SerializeField] Image fill;

        public void Wire(Button btn, TMP_Text titleLabel, TMP_Text subtitleLabel, TMP_Text bodyLabel, TMP_Text badgeLabel, Image progressFill)
        {
            button = btn;
            title = titleLabel;
            subtitle = subtitleLabel;
            body = bodyLabel;
            badge = badgeLabel;
            fill = progressFill;
        }

        public void Bind(string titleText, string subtitleText, string bodyText, string badgeText, float progress01, Action onClick)
        {
            if (title != null)
            {
                title.text = titleText ?? string.Empty;
                title.gameObject.SetActive(!string.IsNullOrEmpty(titleText));
            }

            if (subtitle != null)
            {
                subtitle.text = subtitleText ?? string.Empty;
                subtitle.gameObject.SetActive(!string.IsNullOrEmpty(subtitleText));
            }

            if (body != null)
            {
                body.text = bodyText ?? string.Empty;
                body.gameObject.SetActive(!string.IsNullOrEmpty(bodyText));
            }

            if (badge != null)
            {
                badge.text = badgeText ?? string.Empty;
                badge.gameObject.SetActive(!string.IsNullOrEmpty(badgeText));
            }

            if (fill != null)
            {
                var showFill = progress01 >= 0f;
                fill.gameObject.SetActive(showFill);
                if (showFill)
                {
                    fill.fillAmount = Mathf.Clamp01(progress01);
                }

                if (fill.transform.parent != null && fill.transform.parent != transform)
                {
                    fill.transform.parent.gameObject.SetActive(showFill);
                }
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
