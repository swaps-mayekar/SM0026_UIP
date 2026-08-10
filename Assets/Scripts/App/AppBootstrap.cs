using UnityEngine;
using UnityEngine.UIElements;
using UIP.Content;
using UIP.Core;
using UIP.Persistence;
using UIP.UI;

namespace UIP.App
{
    [DefaultExecutionOrder(-100)]
    public sealed class AppBootstrap : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;
        [SerializeField] PanelSettings panelSettings;
        [SerializeField] VisualTreeAsset shellAsset;
        [SerializeField] StyleSheet theme;

        AppContext _context;
        ScreenRouter _router;
        VisualElement _contentHost;
        VisualElement _navBar;

        void Awake()
        {
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;

            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }

            if (panelSettings == null)
            {
                panelSettings = Resources.Load<PanelSettings>("Panel/AppPanelSettings");
            }

            if (shellAsset == null)
            {
                shellAsset = Resources.Load<VisualTreeAsset>("UI/AppShell");
            }

            if (theme == null)
            {
                theme = Resources.Load<StyleSheet>("UI/AppTheme");
            }

            if (panelSettings != null)
            {
                uiDocument.panelSettings = panelSettings;
            }

            if (shellAsset != null)
            {
                uiDocument.visualTreeAsset = shellAsset;
            }

            var content = ContentRepository.LoadFromResources();
            var store = new ProfileStore();
            _context = new AppContext(content, store);

            var root = uiDocument.rootVisualElement;
            if (theme != null && !root.styleSheets.Contains(theme))
            {
                root.styleSheets.Add(theme);
            }

            _contentHost = root.Q("content-host") ?? CreateFallbackHost(root);
            _navBar = root.Q("nav-bar");

            WireNav(root);
            ApplySafeArea(root.Q("safe-area") ?? root);

            _router = new ScreenRouter(_context, _contentHost, RefreshChrome);
            _context.Navigation.Navigated += OnNavigated;

            if (!_context.Profile.onboardingCompleted)
            {
                _context.Navigation.Go(AppScreen.Splash);
            }
            else
            {
                _context.Navigation.Go(AppScreen.Home);
            }
        }

        void Update()
        {
            _router?.Tick(Time.unscaledDeltaTime);
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
            _router.Render();
        }

        void WireNav(VisualElement root)
        {
            BindNav(root, "nav-home", AppScreen.Home);
            BindNav(root, "nav-learn", AppScreen.Learn);
            BindNav(root, "nav-practice", AppScreen.Practice);
            BindNav(root, "nav-mock", AppScreen.MockSetup);
            BindNav(root, "nav-progress", AppScreen.Progress);
        }

        void BindNav(VisualElement root, string name, AppScreen screen)
        {
            var button = root.Q<Button>(name);
            if (button == null)
            {
                return;
            }

            button.clicked += () => _context.Navigation.Go(screen);
        }

        void RefreshChrome()
        {
            if (_navBar == null)
            {
                return;
            }

            var hide = _context.Navigation.Current is AppScreen.Splash
                or AppScreen.Onboarding
                or AppScreen.MockSession
                or AppScreen.QuestionDetail
                or AppScreen.FlashcardSession
                or AppScreen.LearnPathDetail
                or AppScreen.MistakeDetail
                or AppScreen.About
                or AppScreen.Privacy
                or AppScreen.Disclaimer;

            _navBar.style.display = hide ? DisplayStyle.None : DisplayStyle.Flex;
            SetNavActive("nav-home", AppScreen.Home);
            SetNavActive("nav-learn", AppScreen.Learn, AppScreen.LearnPathDetail, AppScreen.CommonMistakes, AppScreen.Flashcards);
            SetNavActive("nav-practice", AppScreen.Practice, AppScreen.QuestionDetail, AppScreen.Bookmarks);
            SetNavActive("nav-mock", AppScreen.MockSetup, AppScreen.MockSession, AppScreen.MockSummary);
            SetNavActive("nav-progress", AppScreen.Progress, AppScreen.Settings);
        }

        void SetNavActive(string name, params AppScreen[] screens)
        {
            var button = uiDocument.rootVisualElement.Q<Button>(name);
            if (button == null)
            {
                return;
            }

            var active = false;
            foreach (var screen in screens)
            {
                if (_context.Navigation.Current == screen)
                {
                    active = true;
                    break;
                }
            }

            button.EnableInClassList("nav-item-active", active);
        }

        static VisualElement CreateFallbackHost(VisualElement root)
        {
            root.Clear();
            root.AddToClassList("screen-root");
            var host = new VisualElement { name = "content-host" };
            host.style.flexGrow = 1;
            root.Add(host);
            return host;
        }

        static void ApplySafeArea(VisualElement safeArea)
        {
            if (safeArea == null)
            {
                return;
            }

            var sa = Screen.safeArea;
            var left = sa.xMin;
            var right = Screen.width - sa.xMax;
            var top = Screen.height - sa.yMax;
            var bottom = sa.yMin;
            safeArea.style.paddingLeft = left > 0 ? Mathf.Clamp(left * 0.5f, 12f, 28f) : 16f;
            safeArea.style.paddingRight = right > 0 ? Mathf.Clamp(right * 0.5f, 12f, 28f) : 16f;
            safeArea.style.paddingTop = Mathf.Max(12f, top * 0.35f);
            safeArea.style.paddingBottom = Mathf.Max(8f, bottom * 0.35f);
        }
    }
}
