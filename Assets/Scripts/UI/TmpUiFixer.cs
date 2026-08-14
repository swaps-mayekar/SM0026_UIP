using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    /// <summary>
    /// Fixes UITK-to-uGUI leftovers that make screen content invisible:
    /// transparent Mask stencils clip everything, and TMP needs orthographic UI mode.
    /// </summary>
    public sealed class TmpUiFixer : MonoBehaviour
    {
        void Awake()
        {
            Fix(transform);
        }

        public static void Fix(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            var labels = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in labels)
            {
                if (tmp == null)
                {
                    continue;
                }

                tmp.isOrthographic = true;
                if (tmp.font == null && font != null)
                {
                    tmp.font = font;
                }
            }

            var masks = root.GetComponentsInChildren<Mask>(true);
            foreach (var mask in masks)
            {
                if (mask == null)
                {
                    continue;
                }

                var image = mask.GetComponent<Image>();
                if (image != null && image.color.a < 0.01f)
                {
                    // Mask uses image alpha as the stencil. Color.clear hides all children.
                    image.color = Color.white;
                }

                mask.showMaskGraphic = false;
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}
