using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public static class UiListSpawner
    {
        public static void Clear(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(child);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child);
                }
            }
        }

        public static List<T> Spawn<T>(Transform parent, GameObject prefab, int count, Action<T, int> bind)
            where T : Component
        {
            var results = new List<T>(count);
            if (parent == null || prefab == null)
            {
                return results;
            }

            parent = TmpUiFixer.EnsureHorizontalChipScroll(parent);
            Clear(parent);
            var horizontal = parent.GetComponent<HorizontalLayoutGroup>() != null
                             || IsHorizontalChipContent(parent);
            for (var i = 0; i < count; i++)
            {
                var instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.SetActive(true);
                // Only chip rows need LayoutElement width hints; vertical list rows size via
                // ContentSizeFitter and a LayoutElement there collapses preferred height.
                if (horizontal)
                {
                    var layoutElement = instance.GetComponent<LayoutElement>();
                    if (layoutElement == null)
                    {
                        layoutElement = instance.AddComponent<LayoutElement>();
                    }

                    layoutElement.flexibleWidth = 0;
                }
                else
                {
                    var layoutElement = instance.GetComponent<LayoutElement>();
                    if (layoutElement != null &&
                        layoutElement.minHeight < 0f &&
                        layoutElement.preferredHeight < 0f)
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
                }

                var component = instance.GetComponent<T>() ?? instance.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    bind?.Invoke(component, i);
                    results.Add(component);
                }
            }

            TmpUiFixer.Fix(parent);
            TmpUiFixer.RebuildLayoutChain(parent);
            return results;
        }

        static bool IsHorizontalChipContent(Transform parent)
        {
            if (parent == null)
            {
                return false;
            }

            var scroll = parent.GetComponentInParent<ScrollRect>();
            return scroll != null && scroll.horizontal && !scroll.vertical;
        }
    }
}
