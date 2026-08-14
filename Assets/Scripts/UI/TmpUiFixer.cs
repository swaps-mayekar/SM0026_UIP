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
                var chip = tmp.GetComponentInParent<UiChipView>();
                tmp.enableWordWrapping = chip == null;
                if (chip != null)
                {
                    tmp.overflowMode = TextOverflowModes.Overflow;
                }

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
            PadScrollPanels(root);

            var chips = root.GetComponentsInChildren<UiChipView>(true);
            for (var i = 0; i < chips.Length; i++)
            {
                chips[i].SizeToText();
            }

            Canvas.ForceUpdateCanvases();
        }

        /// <summary>
        /// Turns a wrap-style chip row into a horizontal scroller, or returns the
        /// existing content transform when the row is already scrollable.
        /// </summary>
        public static Transform EnsureHorizontalChipScroll(Transform row)
        {
            if (row == null)
            {
                return null;
            }

            var selfScroll = row.GetComponent<ScrollRect>();
            if (selfScroll != null)
            {
                return selfScroll.content != null ? selfScroll.content : row;
            }

            var parentScroll = row.GetComponentInParent<ScrollRect>();
            if (parentScroll != null &&
                parentScroll.content == row &&
                parentScroll.horizontal &&
                !parentScroll.vertical)
            {
                return row;
            }

            var layout = row.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                return row;
            }

            var fitter = row.GetComponent<ContentSizeFitter>();
            if (fitter == null || fitter.horizontalFit != ContentSizeFitter.FitMode.PreferredSize)
            {
                return row;
            }

            var spacing = layout.spacing;
            layout.enabled = false;
            fitter.enabled = false;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(layout);
                UnityEngine.Object.Destroy(fitter);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(layout);
                UnityEngine.Object.DestroyImmediate(fitter);
            }

            var rowLe = row.GetComponent<LayoutElement>();
            if (rowLe == null)
            {
                rowLe = row.gameObject.AddComponent<LayoutElement>();
            }

            rowLe.minHeight = 36;
            rowLe.preferredHeight = 36;
            rowLe.flexibleWidth = 1;
            rowLe.minWidth = 0;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewport.transform.SetParent(row, false);
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = Vector2.zero;
            viewportRt.offsetMax = Vector2.zero;
            viewportRt.localScale = Vector3.one;
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 0);
            contentRt.anchorMax = new Vector2(0, 1);
            contentRt.pivot = new Vector2(0, 0.5f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = Vector2.zero;
            contentRt.localScale = Vector3.one;

            var contentLayout = content.AddComponent<HorizontalLayoutGroup>();
            contentLayout.spacing = spacing;
            contentLayout.childAlignment = TextAnchor.MiddleLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(0, 8, 0, 0);

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = row.gameObject.AddComponent<NestedScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.viewport = viewportRt;
            scroll.content = contentRt;
            return content.transform;
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

        /// <summary>
        /// ScrollRect overwrites content anchoredPosition, so edge offsets are lost.
        /// Layout padding keeps content off the top and bottom of each panel.
        /// </summary>
        static void PadScrollPanels(Transform root)
        {
            const int EdgePadding = 10;
            var scrolls = root.GetComponentsInChildren<ScrollRect>(true);
            foreach (var scroll in scrolls)
            {
                if (scroll == null || scroll.content == null)
                {
                    continue;
                }

                var group = scroll.content.GetComponent<VerticalLayoutGroup>();
                if (group == null)
                {
                    continue;
                }

                var padding = group.padding;
                var changed = false;
                if (padding.top < EdgePadding)
                {
                    padding.top = EdgePadding;
                    changed = true;
                }

                if (padding.bottom < EdgePadding)
                {
                    padding.bottom = EdgePadding;
                    changed = true;
                }

                if (changed)
                {
                    group.padding = padding;
                }
            }
        }
    }
}
