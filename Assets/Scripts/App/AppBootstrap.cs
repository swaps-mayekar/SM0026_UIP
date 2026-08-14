using UnityEngine;
using UnityEngine.UI;
using UIP.Content;
using UIP.Core;
using UIP.Persistence;
using UIP.UI;

namespace UIP.App
{
    [DefaultExecutionOrder(-100)]
    public sealed class AppBootstrap : MonoBehaviour
    {
        [SerializeField] ScreenRouter screenRouter;
        [SerializeField] NavBarView navBar;
        [SerializeField] RectTransform safeArea;
        [SerializeField] Canvas rootCanvas;

        AppContext _context;

        public void Wire(ScreenRouter router, NavBarView nav, RectTransform safe, Canvas canvas)
        {
            screenRouter = router;
            navBar = nav;
            safeArea = safe;
            rootCanvas = canvas;
        }

        void Awake()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            if (screenRouter == null)
            {
                screenRouter = FindFirstObjectByType<ScreenRouter>();
            }

            if (navBar == null)
            {
                navBar = FindFirstObjectByType<NavBarView>();
            }

            if (safeArea == null)
            {
                var fitter = FindFirstObjectByType<SafeAreaFitter>();
                if (fitter != null)
                {
                    safeArea = fitter.transform as RectTransform;
                }
            }

            if (rootCanvas == null)
            {
                rootCanvas = FindFirstObjectByType<Canvas>();
            }

            var content = ContentRepository.LoadFromResources();
            var store = new ProfileStore();
            _context = new AppContext(content, store);

            if (rootCanvas != null)
            {
                TmpUiFixer.Fix(rootCanvas.transform);
            }

            var screens = FindObjectsByType<UiScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            screenRouter.Initialize(_context, navBar, screens);
            _context.Navigation.Navigated += OnNavigated;

            if (!_context.Profile.onboardingCompleted)
            {
                _context.Navigation.Go(AppScreen.Onboarding);
            }
            else
            {
                _context.Navigation.Go(AppScreen.Home);
            }

            // Ensure first paint even if Navigated was suppressed.
            screenRouter.Render();
        }

        void Update()
        {
            screenRouter?.Tick(Time.unscaledDeltaTime);
        }

        void OnDestroy()
        {
            if (_context != null)
            {
                _context.Navigation.Navigated -= OnNavigated;
                _context.Persist();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                _context?.Persist();
            }
        }

        void OnApplicationQuit()
        {
            _context?.Persist();
        }

        void OnNavigated()
        {
            _context.Progress.RememberScreen(_context.Navigation.Current.ToString());
            screenRouter.Render();
        }
    }
}
