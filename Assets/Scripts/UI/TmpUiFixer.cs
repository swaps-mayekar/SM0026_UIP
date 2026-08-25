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

            var bodyFont = TMP_Settings.defaultFontAsset;
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

                if (UiFonts.IsTitleOrButtonText(tmp))
                {
                    UiFonts.ApplyTitleFont(tmp);
                }
                else if (bodyFont != null)
                {
                    tmp.font = bodyFont;
                }
            }

            var inputs = root.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var input in inputs)
            {
                if (input != null && bodyFont != null)
                {
                    input.fontAsset = bodyFont;
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
            FixStretchListColumns(root);
            PadScrollPanels(root);
            EnsureScrollRaycasts(root);

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
        public static void PrepareChipRowContainer(Transform node)
        {
            var row = GetChipRowContainer(node);
            if (row == null)
            {
                return;
            }

            // Only chip rows — never lock vertical list heights to 36px.
            var scroll = row.GetComponent<ScrollRect>();
            var isChipScroll = scroll != null && scroll.horizontal && !scroll.vertical;
            var isChipLayout = row.GetComponent<HorizontalLayoutGroup>() != null;
            if (!isChipScroll && !isChipLayout)
            {
                return;
            }

            PrepareChipRow(row);
            EnsureParentControlsChildHeight(row);
            RebuildLayoutChain(row);
        }

        public static Transform EnsureHorizontalChipScroll(Transform node)
        {
            if (node == null)
            {
                return null;
            }

            var row = GetChipRowContainer(node);

            // Already a horizontal chip scroller — prepare and return its content.
            var selfScroll = row.GetComponent<ScrollRect>();
            if (selfScroll != null && selfScroll.horizontal && !selfScroll.vertical)
            {
                PrepareChipRow(row);
                EnsureParentControlsChildHeight(row);
                var target = selfScroll.content != null ? selfScroll.content : row;
                RebuildLayoutChain(row);
                return target;
            }

            // Spawning into an existing horizontal chip content transform.
            var parentScroll = row.GetComponentInParent<ScrollRect>();
            if (parentScroll != null &&
                parentScroll.horizontal &&
                !parentScroll.vertical &&
                (parentScroll.content == node || node.IsChildOf(parentScroll.content)))
            {
                PrepareChipRow(parentScroll.transform);
                EnsureParentControlsChildHeight(parentScroll.transform);
                RebuildLayoutChain(parentScroll.transform);
                return node;
            }

            // Only convert true chip rows (HorizontalLayoutGroup). Vertical lists must
            // never run PrepareChipRow — it locks height to 36px and kills page scroll.
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            if (layout == null || row.GetComponent<ScrollRect>() != null)
            {
                return node;
            }

            PrepareChipRow(row);
            EnsureParentControlsChildHeight(row);

            var fitter = row.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(fitter);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(fitter);
                }
            }

            var spacing = layout.spacing;
            layout.enabled = false;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(layout);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(layout);
            }

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
            RebuildLayoutChain(row);
            return content.transform;
        }

        public static void RebuildLayoutChain(Transform from)
        {
            if (from == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            var current = from;
            while (current != null)
            {
                if (current is RectTransform rt)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                }

                current = current.parent;
            }

            Canvas.ForceUpdateCanvases();
        }

        static Transform GetChipRowContainer(Transform node)
        {
            if (node == null)
            {
                return null;
            }

            if (node.GetComponent<ScrollRect>() != null)
            {
                return node;
            }

            var parentScroll = node.GetComponentInParent<ScrollRect>();
            if (parentScroll != null &&
                parentScroll.horizontal &&
                !parentScroll.vertical &&
                (parentScroll.content == node || node.IsChildOf(parentScroll.content)))
            {
                return parentScroll.transform;
            }

            return node;
        }

        static void EnsureParentControlsChildHeight(Transform row)
        {
            var group = row.parent != null ? row.parent.GetComponent<VerticalLayoutGroup>() : null;
            if (group != null)
            {
                group.childControlHeight = true;
            }
        }

        static void PrepareChipRow(Transform row)
        {
            if (row == null)
            {
                return;
            }

            // Disable immediately: deferred Destroy leaves the fitter reporting 0 height
            // for the rest of the frame while the parent VLG skips child height control.
            var fitter = row.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(fitter);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(fitter);
                }
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

            if (row is RectTransform rt)
            {
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 36f);
            }
        }

        /// <summary>
        /// Nested VerticalLayoutGroups default to childControlWidth=false, so children
        /// keep Unity's 100px RectTransform and render as a skinny left column.
        /// Only width is forced here — stretch lists handle height in FixStretchListColumns.
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

        static readonly string[] StretchListNames =
        {
            "List", "WeakList", "ActivityList", "Revealed"
        };

        /// <summary>
        /// List/Revealed columns sit under the page ScrollRect content. They must report
        /// height through VerticalLayoutGroup only. A nested ContentSizeFitter or a
        /// flexible-only LayoutElement breaks preferred-height aggregation so the page
        /// cannot scroll (or collapses rows on top of each other).
        /// </summary>
        static void FixStretchListColumns(Transform root)
        {
            var groups = root.GetComponentsInChildren<VerticalLayoutGroup>(true);
            foreach (var group in groups)
            {
                if (group == null || !IsStretchListName(group.gameObject.name))
                {
                    continue;
                }

                group.childControlWidth = true;
                group.childControlHeight = true;
                group.childForceExpandWidth = true;
                group.childForceExpandHeight = false;

                // Stretch lists must never lock height (chip prep used to force 36px here).
                var layoutElement = group.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.enabled = false;
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(layoutElement);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(layoutElement);
                    }
                }

                var fitter = group.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.enabled = false;
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(fitter);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(fitter);
                    }
                }
            }
        }

        static bool IsStretchListName(string name)
        {
            for (var i = 0; i < StretchListNames.Length; i++)
            {
                if (name == StretchListNames[i])
                {
                    return true;
                }
            }

            return false;
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

        /// <summary>
        /// TMP labels are not raycast targets, so empty gaps and text never hit the
        /// ScrollRect unless Content and Viewport have a transparent Image.
        /// </summary>
        static void EnsureScrollRaycasts(Transform root)
        {
            var scrolls = root.GetComponentsInChildren<ScrollRect>(true);
            foreach (var scroll in scrolls)
            {
                if (scroll == null)
                {
                    continue;
                }

                EnsureRaycastImage(scroll.viewport);
                EnsureRaycastImage(scroll.content);
            }
        }

        static void EnsureRaycastImage(RectTransform rt)
        {
            if (rt == null)
            {
                return;
            }

            var image = rt.GetComponent<Image>();
            if (image == null)
            {
                image = rt.gameObject.AddComponent<Image>();
                image.color = Color.clear;
            }

            image.raycastTarget = true;
        }
    }
}
