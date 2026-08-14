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

            Clear(parent);
            for (var i = 0; i < count; i++)
            {
                var instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.SetActive(true);
                var layoutElement = instance.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = instance.AddComponent<LayoutElement>();
                }

                layoutElement.flexibleWidth = 1;
                layoutElement.minWidth = 0;
                var component = instance.GetComponent<T>() ?? instance.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    bind?.Invoke(component, i);
                    results.Add(component);
                }
            }

            TmpUiFixer.Fix(parent);
            return results;
        }
    }
}
