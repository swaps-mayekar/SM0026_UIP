using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    /// <summary>
    /// Runtime UI repairs: TMP orthographic mode, transparent Mask stencils, and
    /// nested vertical columns that otherwise stay at Unity's default 100px width.
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
                tmp.enableWordWrapping = true;
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

            StretchVerticalColumns(root);
            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Nested VerticalLayoutGroups default to childControlWidth=false, so children
        /// keep Unity's 100px RectTransform and render as a skinny left column.
        /// </summary>
        static void StretchVerticalColumns(Transform root)
        {
            var groups = root.GetComponentsInChildren<VerticalLayoutGroup>(true);
            foreach (var group in groups)
            {
                if (group == null)
                {
                    continue;
                }

                group.childControlWidth = true;
                group.childForceExpandWidth = true;
                group.childForceExpandHeight = false;
            }
        }
    }
}
