using UnityEngine;

namespace UIP.UI
{
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] RectTransform target;

        void Awake()
        {
            if (target == null)
            {
                target = transform as RectTransform;
            }

            Apply();
        }

        void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        public void Apply()
        {
            if (target == null)
            {
                return;
            }

            var sa = Screen.safeArea;
            var screen = new Vector2(Screen.width, Screen.height);
            if (screen.x <= 0f || screen.y <= 0f)
            {
                return;
            }

            var anchorMin = sa.position;
            var anchorMax = sa.position + sa.size;
            anchorMin.x /= screen.x;
            anchorMin.y /= screen.y;
            anchorMax.x /= screen.x;
            anchorMax.y /= screen.y;
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }
    }
}
