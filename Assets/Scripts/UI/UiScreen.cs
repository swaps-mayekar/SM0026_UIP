using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public abstract class UiScreen : MonoBehaviour
    {
        [SerializeField] AppScreen screenId;

        protected AppContext Ctx { get; private set; }
        protected ScreenRouter Router { get; private set; }

        public AppScreen ScreenId => screenId;

        public void Configure(AppScreen id)
        {
            screenId = id;
        }

        public virtual void Bind(AppContext ctx, ScreenRouter router)
        {
            Ctx = ctx;
            Router = router;
            OnBound();
        }

        protected virtual void OnBound()
        {
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
            TmpUiFixer.Fix(transform);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public abstract void Refresh();

        protected void Go(AppScreen screen) => Ctx.Navigation.Go(screen);

        protected static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = value ?? string.Empty;
            }
        }

        protected static void SetActive(GameObject go, bool active)
        {
            if (go != null && go.activeSelf != active)
            {
                go.SetActive(active);
                if (active)
                {
                    TmpUiFixer.Fix(go.transform);
                }
            }
        }

        protected static void SetActive(Component component, bool active)
        {
            if (component != null)
            {
                SetActive(component.gameObject, active);
            }
        }

        protected static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }
        }
    }
}
