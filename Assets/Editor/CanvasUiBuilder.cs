#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UIP.App;
using UIP.Core;
using UIP.UI;

namespace UIP.EditorTools
{
    public static class CanvasUiBuilder
    {
        const string PrefabFolder = "Assets/UI/Prefabs";
        const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        static TMP_FontAsset Font;

        public const string SplashScenePath = "Assets/Scenes/0_SplashScene.unity";
        public const string AppScenePath = "Assets/Scenes/1_AppScene.unity";

        public static void BuildAll()
        {
            BuildSplashScene(SplashScenePath);
            BuildAppScene(AppScenePath);
            EnsureBuildSettings();
        }

        public static void BuildScene(string scenePath)
        {
            if (scenePath != null && scenePath.Contains("0_SplashScene"))
            {
                BuildSplashScene(scenePath);
                return;
            }

            BuildAppScene(scenePath);
        }

        public static void BuildSplashScene(string scenePath)
        {
            Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var scene = PrepareEmptyScene(scenePath);
            EnsureEventSystem();

            var splashGo = new GameObject("Splash");
            var bootstrap = splashGo.AddComponent<SplashBootstrap>();
            var canvasGo = CreateRootCanvas();
            var canvas = canvasGo.GetComponent<Canvas>();
            var safe = CreateSafeArea(canvasGo.transform);

            var continueBtn = BuildFullScreenDisclaimer(safe.transform);
            bootstrap.Wire(continueBtn, canvas);

            SaveScene(scene, "UIP splash scene setup complete.");
        }

        public static void BuildAppScene(string scenePath)
        {
            EnsureFolder("Assets/UI");
            EnsureFolder(PrefabFolder);
            Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            var scene = PrepareEmptyScene(scenePath);
            EnsureEventSystem();

            var appGo = new GameObject("App");
            var bootstrap = appGo.AddComponent<AppBootstrap>();
            var router = appGo.AddComponent<ScreenRouter>();

            var canvasGo = CreateRootCanvas();
            var canvas = canvasGo.GetComponent<Canvas>();
            var safe = CreateSafeArea(canvasGo.transform);

            var screenHost = CreateUIObject("ScreenHost", safe.transform);
            var screenHostRt = screenHost.GetComponent<RectTransform>();
            Stretch(screenHostRt);
            screenHostRt.offsetMin = new Vector2(0, 72);
            screenHostRt.offsetMax = Vector2.zero;

            var chipPrefab = BuildChipPrefab();
            var simpleRowPrefab = BuildSimpleRowPrefab("SimpleRow");
            var lessonRowPrefab = BuildLessonRowPrefab();
            var pathRowPrefab = BuildSimpleRowPrefab("PathRow");
            var questionRowPrefab = BuildSimpleRowPrefab("QuestionRow");
            var topicRowPrefab = BuildSimpleRowPrefab("TopicRow");
            var mistakeRowPrefab = BuildSimpleRowPrefab("MistakeRow");
            var weakRowPrefab = BuildSimpleRowPrefab("WeakTopicRow");
            var activityRowPrefab = BuildSimpleRowPrefab("ActivityRow");

            var screens = new System.Collections.Generic.List<UiScreen>
            {
                BuildOnboarding(screenHost.transform),
                BuildHome(screenHost.transform),
                BuildLearn(screenHost.transform, pathRowPrefab),
                BuildLearnPathDetail(screenHost.transform, lessonRowPrefab),
                BuildPractice(screenHost.transform, chipPrefab, questionRowPrefab),
                BuildQuestionDetail(screenHost.transform, chipPrefab),
                BuildMockSetup(screenHost.transform),
                BuildMockSession(screenHost.transform, chipPrefab),
                BuildMockSummary(screenHost.transform, simpleRowPrefab),
                BuildFlashcardsHub(screenHost.transform, topicRowPrefab),
                BuildFlashcardSession(screenHost.transform),
                BuildProgress(screenHost.transform, weakRowPrefab, activityRowPrefab),
                BuildBookmarks(screenHost.transform, questionRowPrefab),
                BuildMistakes(screenHost.transform, mistakeRowPrefab),
                BuildMistakeDetail(screenHost.transform)
            };

            var nav = BuildNavBar(safe.transform);
            router.SetNavBar(nav);
            router.SetScreens(screens);
            bootstrap.Wire(router, nav, safe.GetComponent<RectTransform>(), canvas);

            SaveScene(scene, "UIP app scene setup complete.");
        }

        static Scene PrepareEmptyScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != "Main Camera")
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                camera.backgroundColor = UiTheme.Bg;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographic = true;
            }

            return scene;
        }

        static GameObject CreateRootCanvas()
        {
            var canvasGo = CreateUIObject("Canvas", null);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<TmpUiFixer>();
            var canvasBg = canvasGo.AddComponent<Image>();
            canvasBg.color = UiTheme.Bg;
            canvasBg.raycastTarget = false;
            return canvasGo;
        }

        static GameObject CreateSafeArea(Transform canvas)
        {
            var safe = CreateUIObject("SafeArea", canvas);
            Stretch(safe.GetComponent<RectTransform>());
            safe.AddComponent<SafeAreaFitter>();
            return safe;
        }

        static void SaveScene(Scene scene, string message)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(message);
        }

        static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SplashScenePath, true),
                new EditorBuildSettingsScene(AppScenePath, true)
            };
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        static Button BuildFullScreenDisclaimer(Transform host)
        {
            var root = CreateUIObject("DisclaimerPanel", host);
            Stretch(root.GetComponent<RectTransform>());

            var scrollContent = CreateScrollContent(root.transform, out var scroll);
            var scrollRt = scroll.GetComponent<RectTransform>();
            Stretch(scrollRt, 24, 24, 24, 88);

            CreateLabel("Title", scrollContent, "Disclaimer", 28, true, UiTheme.Text);
            CreateLabel("Body1", scrollContent, "Unity is a trademark of Unity Technologies. This application is an independent educational resource and is not affiliated with, endorsed by, or sponsored by Unity Technologies.", 15, false, UiTheme.TextMuted);
            CreateLabel("Body2", scrollContent, "Do not interpret this app as an official Unity certification product, official exam prep from Unity Technologies, or authorized training-partner material.", 15, false, UiTheme.TextMuted);
            CreateLabel("Body3", scrollContent, "Educational explanations and sample code inside the app are original. They are intended to help candidates practice interviews and are not a substitute for official documentation.", 15, false, UiTheme.TextMuted);
            CreateLabel("Body4", scrollContent, "Interview outcomes depend on many factors beyond this app. Content is for education and practice only.", 15, false, UiTheme.TextMuted);

            var continueGo = CreateUIObject("ContinueButton", root.transform);
            var continueRt = continueGo.GetComponent<RectTransform>();
            continueRt.anchorMin = new Vector2(0, 0);
            continueRt.anchorMax = new Vector2(1, 0);
            continueRt.pivot = new Vector2(0.5f, 0);
            continueRt.sizeDelta = new Vector2(-48, 52);
            continueRt.anchoredPosition = new Vector2(0, 24);
            var img = continueGo.AddComponent<Image>();
            img.color = UiTheme.Accent;
            var btn = continueGo.AddComponent<Button>();
            var text = CreateLabel("Label", continueGo.transform, "Continue", 16, true, UiTheme.OnAccent);
            text.alignment = TextAlignmentOptions.Center;
            Stretch(text.rectTransform);
            return btn;
        }

        static UiScreen BuildOnboarding(Transform host)
        {
            var root = CreatePanel(host, "OnboardingPanel", AppScreen.Onboarding, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Welcome", 24, true, UiTheme.Text);
            CreateLabel("Body", content, "This app helps you prepare for Unity interviews with learning paths, interview questions, timed mock interviews, flashcards, and a progress dashboard.", 13, false, UiTheme.TextMuted);
            CreateLabel("How", content, "How it works", 14, true, UiTheme.Accent);
            CreateLabel("B1", content, "• Learn concepts through role-based paths", 13, false, UiTheme.TextMuted);
            CreateLabel("B2", content, "• Practice 100+ original interview prompts", 13, false, UiTheme.TextMuted);
            CreateLabel("B3", content, "• Test yourself with timed mock interviews", 13, false, UiTheme.TextMuted);
            CreateLabel("B4", content, "• Improve using weak-topic tracking and streaks", 13, false, UiTheme.TextMuted);
            CreateLabel("Important", content, "Important", 14, true, UiTheme.Accent);
            CreateLabel("Disclaimer", content, "Unity is a trademark of Unity Technologies. This application is an independent educational resource and is not affiliated with, endorsed by, or sponsored by Unity Technologies.", 13, false, UiTheme.TextMuted);
            var started = CreateButton("GetStartedButton", content, "Get started", true);
            var screen = screenGo.AddComponent<OnboardingScreen>();
            screen.Configure(AppScreen.Onboarding);
            screen.Wire(started);
            return screen;
        }

        static UiScreen BuildHome(Transform host)
        {
            var root = CreatePanel(host, "HomePanel", AppScreen.Home, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Home", 24, true, UiTheme.Text);
            var streak = CreateLabel("Streak", content, "Streak", 13, false, UiTheme.TextMuted);

            var continueCard = CreateCard("ContinueCard", content);
            CreateLabel("ContinueTitle", continueCard, "Continue studying", 16, true, UiTheme.Text);
            var continueBody = CreateLabel("ContinueBody", continueCard, "", 13, false, UiTheme.TextMuted);
            var resumeQ = CreateButton("ResumeQuestion", continueCard, "Resume question", true);

            var weakCard = CreateCard("WeakCard", content);
            CreateLabel("WeakTitle", weakCard, "Recommended focus", 16, true, UiTheme.Text);
            var weakBody = CreateLabel("WeakBody", weakCard, "", 13, false, UiTheme.TextMuted);
            var practiceWeak = CreateButton("PracticeWeak", weakCard, "Practice this topic", false);

            var quick = CreateCard("QuickCard", content);
            CreateLabel("QuickTitle", quick, "Quick actions", 16, true, UiTheme.Text);
            var startMock = CreateButton("StartMock", quick, "Start mock interview", true);
            var flashcards = CreateButton("Flashcards", quick, "Review flashcards", false);
            var mistakes = CreateButton("Mistakes", quick, "Common mistakes", false);
            var bookmarks = CreateButton("Bookmarks", quick, "Bookmarks", false);

            var resumeMockCard = CreateCard("ResumeMockCard", content);
            CreateLabel("ResumeMockTitle", resumeMockCard, "Interrupted mock interview", 16, true, UiTheme.Text);
            CreateLabel("ResumeMockBody", resumeMockCard, "You have an unfinished timed session.", 13, false, UiTheme.TextMuted);
            var resumeMock = CreateButton("ResumeMock", resumeMockCard, "Resume mock", true);

            var screen = screenGo.AddComponent<HomeScreen>();
            screen.Configure(AppScreen.Home);
            screen.Wire(streak, continueBody, resumeQ, weakBody, practiceWeak, startMock, flashcards, mistakes, bookmarks, resumeMockCard.gameObject, resumeMock);
            return screen;
        }

        static UiScreen BuildLearn(Transform host, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "LearnPanel", AppScreen.Learn, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Learn", 24, true, UiTheme.Text);
            CreateLabel("Sub", content, "Role-based paths with original lessons.", 13, false, UiTheme.TextMuted);
            var list = CreateStretchColumn("List", content, 10);
            var mistakes = CreateButton("Mistakes", content, "Browse common mistakes", false);
            var flashcards = CreateButton("Flashcards", content, "Flashcards", false);
            var screen = screenGo.AddComponent<LearnScreen>();
            screen.Configure(AppScreen.Learn);
            screen.Wire(list.transform, rowPrefab, mistakes, flashcards);
            return screen;
        }

        static UiScreen BuildLearnPathDetail(Transform host, GameObject lessonPrefab)
        {
            var root = CreatePanel(host, "LearnPathDetailPanel", AppScreen.LearnPathDetail, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            var title = CreateLabel("Title", content, "Path", 24, true, UiTheme.Text);
            var body = CreateLabel("Body", content, "", 13, false, UiTheme.TextMuted);
            CreateProgress("Progress", content, out var fill);
            var list = CreateStretchColumn("List", content, 10);
            var screen = screenGo.AddComponent<LearnPathDetailScreen>();
            screen.Configure(AppScreen.LearnPathDetail);
            screen.Wire(back, title, body, fill, list.transform, lessonPrefab);
            return screen;
        }

        static UiScreen BuildPractice(Transform host, GameObject chipPrefab, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "PracticePanel", AppScreen.Practice, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Practice", 24, true, UiTheme.Text);
            var search = CreateInput("Search", content, "Search questions");
            var difficulty = CreateWrapRow("Difficulty", content);
            var topics = CreateWrapRow("Topics", content);
            var count = CreateLabel("Count", content, "0 questions", 12, false, UiTheme.TextMuted);
            var list = CreateStretchColumn("List", content, 10);
            var screen = screenGo.AddComponent<PracticeScreen>();
            screen.Configure(AppScreen.Practice);
            screen.Wire(search, difficulty.transform, topics.transform, list.transform, chipPrefab, rowPrefab, count);
            return screen;
        }

        static UiScreen BuildQuestionDetail(Transform host, GameObject chipPrefab)
        {
            var root = CreatePanel(host, "QuestionDetailPanel", AppScreen.QuestionDetail, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            var topic = CreateLabel("Topic", content, "", 12, false, UiTheme.TextMuted);
            var difficulty = CreateLabel("Difficulty", content, "", 12, true, UiTheme.Accent);
            CreateLabel("Heading", content, "Interview question", 24, true, UiTheme.Text);
            var prompt = CreateLabel("Prompt", content, "", 14, false, UiTheme.Text);
            var meta = CreateLabel("Meta", content, "", 12, false, UiTheme.TextMuted);
            var bookmark = CreateButton("Bookmark", content, "Bookmark", false);
            var bookmarkLabel = bookmark.GetComponentInChildren<TMP_Text>();
            var reveal = CreateButton("Reveal", content, "Reveal recommended answer", true);
            var revealed = CreateStretchColumn("Revealed", content, 8);
            CreateLabel("IntentTitle", revealed.transform, "Interviewer's intent", 14, true, UiTheme.Accent);
            var intent = CreateLabel("Intent", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("IdealTitle", revealed.transform, "Ideal answer", 14, true, UiTheme.Accent);
            var ideal = CreateLabel("Ideal", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("MistakesTitle", revealed.transform, "Common mistakes", 14, true, UiTheme.Accent);
            var mistakes = CreateLabel("Mistakes", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("FollowTitle", revealed.transform, "Follow-ups", 14, true, UiTheme.Accent);
            var followUps = CreateLabel("FollowUps", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("CodeTitle", revealed.transform, "Code sketch", 14, true, UiTheme.Accent);
            var code = CreateLabel("Code", revealed.transform, "", 12, false, UiTheme.Text);
            CreateLabel("RateTitle", revealed.transform, "Self-rate your answer", 14, true, UiTheme.Accent);
            var rating = CreateWrapRow("Rating", revealed.transform);
            CreateLabel("ConfTitle", revealed.transform, "Confidence", 14, true, UiTheme.Accent);
            var confidence = CreateWrapRow("Confidence", revealed.transform);
            var save = CreateButton("Save", revealed.transform, "Save progress", true);
            revealed.SetActive(false);
            var screen = screenGo.AddComponent<QuestionDetailScreen>();
            screen.Configure(AppScreen.QuestionDetail);
            screen.Wire(back, topic, difficulty, prompt, meta, bookmark, bookmarkLabel, reveal, revealed, intent, ideal, mistakes, followUps, code, rating.transform, confidence.transform, chipPrefab, save);
            return screen;
        }

        static UiScreen BuildMockSetup(Transform host)
        {
            var root = CreatePanel(host, "MockSetupPanel", AppScreen.MockSetup, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Mock interview", 24, true, UiTheme.Text);
            CreateLabel("Body", content, "Timed questions with thinking time, reveal, and self-rating. Progress is tracked locally.", 13, false, UiTheme.TextMuted);
            var prefs = CreateLabel("Prefs", content, "", 12, false, UiTheme.TextMuted);
            var a = CreateButton("Opt52", content, "5 questions · 2 min each", true);
            var b = CreateButton("Opt53", content, "5 questions · 3 min each", false);
            var c = CreateButton("Opt102", content, "10 questions · 2 min each", false);
            var d = CreateButton("Opt103", content, "10 questions · 3 min each", false);
            var resume = CreateButton("Resume", content, "Resume interrupted session", true);
            var screen = screenGo.AddComponent<MockSetupScreen>();
            screen.Configure(AppScreen.MockSetup);
            screen.Wire(prefs, a, b, c, d, resume);
            return screen;
        }

        static UiScreen BuildMockSession(Transform host, GameObject chipPrefab)
        {
            var root = CreatePanel(host, "MockSessionPanel", AppScreen.MockSession, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var progress = CreateLabel("Progress", content, "", 12, false, UiTheme.TextMuted);
            var timer = CreateLabel("Timer", content, "0:00", 40, true, UiTheme.Accent);
            var difficulty = CreateLabel("Difficulty", content, "", 12, true, UiTheme.Accent);
            var prompt = CreateLabel("Prompt", content, "", 14, false, UiTheme.Text);
            var pause = CreateButton("Pause", content, "Pause", false);
            var pauseLabel = pause.GetComponentInChildren<TMP_Text>();
            var reveal = CreateButton("Reveal", content, "Reveal recommended answer", true);
            var revealed = CreateStretchColumn("Revealed", content, 8);
            CreateLabel("IdealTitle", revealed.transform, "Ideal answer", 14, true, UiTheme.Accent);
            var ideal = CreateLabel("Ideal", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("IntentTitle", revealed.transform, "Interviewer's intent", 14, true, UiTheme.Accent);
            var intent = CreateLabel("Intent", revealed.transform, "", 13, false, UiTheme.TextMuted);
            CreateLabel("RateTitle", revealed.transform, "Self-rate", 14, true, UiTheme.Accent);
            var rating = CreateWrapRow("Rating", revealed.transform);
            var confidence = CreateWrapRow("Confidence", revealed.transform);
            var next = CreateButton("Next", revealed.transform, "Next question", true);
            var nextLabel = next.GetComponentInChildren<TMP_Text>();
            revealed.SetActive(false);
            var abandon = CreateButton("Abandon", content, "Abandon session", false);
            var screen = screenGo.AddComponent<MockSessionScreen>();
            screen.Configure(AppScreen.MockSession);
            screen.Wire(progress, timer, difficulty, prompt, pause, pauseLabel, reveal, revealed, ideal, intent, rating.transform, confidence.transform, chipPrefab, next, nextLabel, abandon);
            return screen;
        }

        static UiScreen BuildMockSummary(Transform host, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "MockSummaryPanel", AppScreen.MockSummary, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Mock summary", 24, true, UiTheme.Text);
            var score = CreateLabel("Score", content, "—", 22, true, UiTheme.Accent);
            var meta = CreateLabel("Meta", content, "", 13, false, UiTheme.TextMuted);
            var list = CreateStretchColumn("List", content, 10);
            var home = CreateButton("Home", content, "Back to home", true);
            var again = CreateButton("Again", content, "Try another mock", false);
            var screen = screenGo.AddComponent<MockSummaryScreen>();
            screen.Configure(AppScreen.MockSummary);
            screen.Wire(score, meta, list.transform, rowPrefab, home, again);
            return screen;
        }

        static UiScreen BuildFlashcardsHub(Transform host, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "FlashcardsPanel", AppScreen.Flashcards, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Flashcards", 24, true, UiTheme.Text);
            var due = CreateLabel("Due", content, "", 13, false, UiTheme.TextMuted);
            var review = CreateButton("ReviewDue", content, "Review due cards", true);
            var list = CreateStretchColumn("List", content, 10);
            var screen = screenGo.AddComponent<FlashcardsHubScreen>();
            screen.Configure(AppScreen.Flashcards);
            screen.Wire(due, review, list.transform, rowPrefab);
            return screen;
        }

        static UiScreen BuildFlashcardSession(Transform host)
        {
            var root = CreatePanel(host, "FlashcardSessionPanel", AppScreen.FlashcardSession, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            var progress = CreateLabel("Progress", content, "", 12, false, UiTheme.TextMuted);
            var cardGo = CreateCard("FlashCard", content);
            var cardLe = cardGo.gameObject.AddComponent<LayoutElement>();
            cardLe.minHeight = 220;
            cardLe.preferredHeight = 220;
            var cardLabel = CreateLabel("CardText", cardGo, "", 18, true, UiTheme.Text);
            cardLabel.alignment = TextAlignmentOptions.Center;
            var flip = cardGo.gameObject.GetComponent<Button>();
            if (flip == null)
            {
                flip = cardGo.gameObject.AddComponent<Button>();
            }

            var empty = CreateLabel("Empty", content, "No flashcards", 14, false, UiTheme.TextMuted);
            empty.gameObject.SetActive(false);
            var show = CreateButton("ShowAnswer", content, "Show answer", true);
            var grade = CreateUIObject("Grade", content);
            var gradeLayout = grade.AddComponent<HorizontalLayoutGroup>();
            gradeLayout.spacing = 8;
            gradeLayout.childControlWidth = true;
            gradeLayout.childForceExpandWidth = true;
            var again = CreateButton("Again", grade.transform, "Again", false);
            var hard = CreateButton("Hard", grade.transform, "Hard", false);
            var good = CreateButton("Good", grade.transform, "Good", true);
            grade.SetActive(false);
            var screen = screenGo.AddComponent<FlashcardSessionScreen>();
            screen.Configure(AppScreen.FlashcardSession);
            screen.Wire(back, progress, cardLabel, flip, show, grade, again, hard, good, empty.gameObject);
            return screen;
        }

        static UiScreen BuildProgress(Transform host, GameObject weakPrefab, GameObject activityPrefab)
        {
            var root = CreatePanel(host, "ProgressPanel", AppScreen.Progress, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            CreateLabel("Title", content, "Improve", 24, true, UiTheme.Text);
            var grid = CreateUIObject("Stats", content);
            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(160, 70);
            gridLayout.spacing = new Vector2(8, 8);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            TMP_Text MakeStat(string name, string label)
            {
                var cell = CreateCard(name, grid.transform);
                var value = CreateLabel("Value", cell, "0", 22, true, UiTheme.Accent);
                CreateLabel("Label", cell, label, 11, false, UiTheme.TextMuted);
                return value;
            }

            var q = MakeStat("Questions", "Questions completed");
            var acc = MakeStat("Accuracy", "Self-rated accuracy");
            var streak = MakeStat("Streak", "Daily streak");
            var mocks = MakeStat("Mocks", "Mock interviews");
            var mockAvg = MakeStat("MockAvg", "Mock avg score");
            var conf = MakeStat("Confidence", "Confidence");
            CreateLabel("WeakTitle", content, "Weak topics", 14, true, UiTheme.Accent);
            var weakEmpty = CreateLabel("WeakEmpty", content, "Practice more to generate coaching signals.", 12, false, UiTheme.TextMuted);
            var weak = CreateStretchColumn("WeakList", content, 10);
            CreateLabel("ActivityTitle", content, "Recent activity", 14, true, UiTheme.Accent);
            var activity = CreateStretchColumn("ActivityList", content, 6);
            var bookmarks = CreateButton("Bookmarks", content, "Bookmarks", false);
            var screen = screenGo.AddComponent<ProgressScreen>();
            screen.Configure(AppScreen.Progress);
            screen.Wire(q, acc, streak, mocks, mockAvg, conf, weak.transform, weakPrefab, activity.transform, activityPrefab, weakEmpty, bookmarks);
            return screen;
        }

        static UiScreen BuildBookmarks(Transform host, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "BookmarksPanel", AppScreen.Bookmarks, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            CreateLabel("Title", content, "Bookmarks", 24, true, UiTheme.Text);
            var empty = CreateLabel("Empty", content, "No bookmarks yet", 14, false, UiTheme.TextMuted);
            var list = CreateStretchColumn("List", content, 10);
            var screen = screenGo.AddComponent<BookmarksScreen>();
            screen.Configure(AppScreen.Bookmarks);
            screen.Wire(back, list.transform, rowPrefab, empty.gameObject);
            return screen;
        }

        static UiScreen BuildMistakes(Transform host, GameObject rowPrefab)
        {
            var root = CreatePanel(host, "MistakesPanel", AppScreen.CommonMistakes, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            CreateLabel("Title", content, "Common interview mistakes", 24, true, UiTheme.Text);
            var list = CreateStretchColumn("List", content, 10);
            var screen = screenGo.AddComponent<MistakesScreen>();
            screen.Configure(AppScreen.CommonMistakes);
            screen.Wire(back, list.transform, rowPrefab);
            return screen;
        }

        static UiScreen BuildMistakeDetail(Transform host)
        {
            var root = CreatePanel(host, "MistakeDetailPanel", AppScreen.MistakeDetail, out var screenGo);
            var content = CreateScrollContent(root.transform, out _);
            var back = CreateButton("Back", content, "← Back", false);
            var title = CreateLabel("Title", content, "", 24, true, UiTheme.Text);
            CreateLabel("WhyTitle", content, "Why it's a problem", 14, true, UiTheme.Accent);
            var why = CreateLabel("Why", content, "", 13, false, UiTheme.TextMuted);
            CreateLabel("ExpectTitle", content, "What interviewers expect", 14, true, UiTheme.Accent);
            var expect = CreateLabel("Expect", content, "", 13, false, UiTheme.TextMuted);
            CreateLabel("BetterTitle", content, "Better alternative", 14, true, UiTheme.Accent);
            var better = CreateLabel("Better", content, "", 13, false, UiTheme.TextMuted);
            CreateLabel("AntiTitle", content, "Anti-pattern", 14, true, UiTheme.Accent);
            var anti = CreateLabel("Anti", content, "", 12, false, UiTheme.Text);
            CreateLabel("PreferredTitle", content, "Preferred pattern", 14, true, UiTheme.Accent);
            var preferred = CreateLabel("Preferred", content, "", 12, false, UiTheme.Text);
            var screen = screenGo.AddComponent<MistakeDetailScreen>();
            screen.Configure(AppScreen.MistakeDetail);
            screen.Wire(back, title, why, expect, better, anti, preferred);
            return screen;
        }

        static NavBarView BuildNavBar(Transform parent)
        {
            var bar = CreateUIObject("NavBar", parent);
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 72);
            rt.anchoredPosition = Vector2.zero;
            var bg = bar.AddComponent<Image>();
            bg.color = UiTheme.BgElevated;
            var layout = bar.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            Button MakeNav(string name, string label)
            {
                var go = CreateUIObject(name, bar.transform);
                var btn = go.AddComponent<Button>();
                go.AddComponent<Image>().color = Color.clear;
                var text = CreateLabel("Label", go.transform, label, 11, false, UiTheme.TextMuted);
                text.alignment = TextAlignmentOptions.Center;
                var le = text.gameObject.AddComponent<LayoutElement>();
                le.flexibleWidth = 1;
                return btn;
            }

            var home = MakeNav("NavHome", "Home");
            var learn = MakeNav("NavLearn", "Learn");
            var practice = MakeNav("NavPractice", "Practice");
            var mock = MakeNav("NavMock", "Mock");
            var progress = MakeNav("NavProgress", "Progress");
            var nav = bar.AddComponent<NavBarView>();
            nav.WireSerialized(
                home, learn, practice, mock, progress,
                home.GetComponentInChildren<TMP_Text>(),
                learn.GetComponentInChildren<TMP_Text>(),
                practice.GetComponentInChildren<TMP_Text>(),
                mock.GetComponentInChildren<TMP_Text>(),
                progress.GetComponentInChildren<TMP_Text>());
            return nav;
        }

        static GameObject CreatePanel(Transform host, string name, AppScreen id, out GameObject screenGo)
        {
            screenGo = CreateUIObject(name, host);
            Stretch(screenGo.GetComponent<RectTransform>());
            screenGo.SetActive(false);
            return screenGo;
        }

        static GameObject CreateStretchColumn(string name, Transform parent, float spacing)
        {
            var go = CreateUIObject(name, parent);
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = spacing;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 0;
            return go;
        }

        static Transform CreateScrollContent(Transform parent, out ScrollRect scroll)
        {
            var scrollGo = CreateUIObject("Scroll", parent);
            Stretch(scrollGo.GetComponent<RectTransform>());
            scroll = scrollGo.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = CreateUIObject("Viewport", scrollGo.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var content = CreateUIObject("Content", viewport.transform);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.anchoredPosition = new Vector2(0, -12);
            contentRt.sizeDelta = new Vector2(-32, 0);
            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(0, 0, 10, 0);
            vlg.spacing = 10;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = contentRt;
            return content.transform;
        }

        static Transform CreateCard(string name, Transform parent)
        {
            var card = CreateUIObject(name, parent);
            var img = card.AddComponent<Image>();
            img.color = UiTheme.BgCard;
            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.spacing = 8;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            card.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return card.transform;
        }

        static GameObject CreateWrapRow(string name, Transform parent)
        {
            var row = CreateUIObject(name, parent);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childControlHeight = true;
            var fitter = row.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return row;
        }

        static GameObject CreateProgress(string name, Transform parent, out Image fill)
        {
            var track = CreateUIObject(name, parent);
            var trackImg = track.AddComponent<Image>();
            trackImg.color = UiTheme.BgElevated;
            var le = track.AddComponent<LayoutElement>();
            le.minHeight = 10;
            le.preferredHeight = 10;
            var fillGo = CreateUIObject("Fill", track.transform);
            Stretch(fillGo.GetComponent<RectTransform>());
            fill = fillGo.AddComponent<Image>();
            fill.color = UiTheme.Accent;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0.4f;
            return track;
        }

        static TMP_InputField CreateInput(string name, Transform parent, string placeholder)
        {
            var go = CreateUIObject(name, parent);
            var bg = go.AddComponent<Image>();
            bg.color = UiTheme.BgElevated;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 40;
            le.preferredHeight = 40;
            var textArea = CreateUIObject("TextArea", go.transform);
            Stretch(textArea.GetComponent<RectTransform>(), 10, 8, 10, 8);
            textArea.AddComponent<RectMask2D>();
            var text = CreateLabel("Text", textArea.transform, "", 14, false, UiTheme.Text);
            Stretch(text.rectTransform);
            var ph = CreateLabel("Placeholder", textArea.transform, placeholder, 14, false, UiTheme.TextMuted);
            Stretch(ph.rectTransform);
            var input = go.AddComponent<TMP_InputField>();
            input.textViewport = textArea.GetComponent<RectTransform>();
            input.textComponent = text;
            input.placeholder = ph;
            input.fontAsset = Font;
            return input;
        }

        static Button CreateButton(string name, Transform parent, string label, bool primary)
        {
            var go = CreateUIObject(name, parent);
            var img = go.AddComponent<Image>();
            img.color = primary ? UiTheme.Accent : UiTheme.BgCardAlt;
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = primary ? UiTheme.Accent : UiTheme.BgCard;
            btn.colors = colors;
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 44;
            le.preferredHeight = 44;
            var text = CreateLabel("Label", go.transform, label, 14, true, primary ? UiTheme.OnAccent : UiTheme.Text);
            text.alignment = TextAlignmentOptions.Center;
            Stretch(text.rectTransform);
            return btn;
        }

        static TMP_Text CreateLabel(string name, Transform parent, string value, float size, bool bold, Color color)
        {
            var go = CreateUIObject(name, parent);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.fontSize = size;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.color = color;
            tmp.text = value;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            // Required for Screen Space Overlay / Camera UI canvases.
            tmp.isOrthographic = true;
            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return tmp;
        }

        static GameObject BuildChipPrefab()
        {
            var path = PrefabFolder + "/Chip.prefab";
            var go = CreateUIObject("Chip", null);
            var img = go.AddComponent<Image>();
            img.color = UiTheme.BgCardAlt;
            var btn = go.AddComponent<Button>();
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 32;
            le.preferredHeight = 32;
            var label = CreateLabel("Label", go.transform, "Chip", 12, false, UiTheme.Text);
            label.alignment = TextAlignmentOptions.Center;
            Stretch(label.rectTransform, 10, 6, 10, 6);
            var chip = go.AddComponent<UiChipView>();
            chip.Wire(btn, label, img);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject BuildSimpleRowPrefab(string name)
        {
            var path = $"{PrefabFolder}/{name}.prefab";
            var go = CreateUIObject(name, null);
            var img = go.AddComponent<Image>();
            img.color = UiTheme.BgCard;
            var btn = go.AddComponent<Button>();
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(12, 12, 12, 12);
            vlg.spacing = 4;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rowLe = go.AddComponent<LayoutElement>();
            rowLe.flexibleWidth = 1;
            rowLe.minWidth = 0;
            var title = CreateLabel("Title", go.transform, "Title", 16, true, UiTheme.Text);
            var subtitle = CreateLabel("Subtitle", go.transform, "Subtitle", 12, false, UiTheme.TextMuted);
            var body = CreateLabel("Body", go.transform, "Body", 13, false, UiTheme.TextMuted);
            var badge = CreateLabel("Badge", go.transform, "Badge", 11, true, UiTheme.Accent);
            var track = CreateProgress("Progress", go.transform, out var fill);
            var row = go.AddComponent<UiSimpleRow>();
            row.Wire(btn, title, subtitle, body, badge, fill);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject BuildLessonRowPrefab()
        {
            var path = PrefabFolder + "/LessonRow.prefab";
            var go = CreateUIObject("LessonRow", null);
            go.AddComponent<Image>().color = UiTheme.BgCard;
            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(14, 14, 14, 14);
            vlg.spacing = 8;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            go.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rowLe = go.AddComponent<LayoutElement>();
            rowLe.flexibleWidth = 1;
            rowLe.minWidth = 0;
            var title = CreateLabel("Title", go.transform, "Lesson", 16, true, UiTheme.Text);
            var badge = CreateLabel("Badge", go.transform, "5 min", 11, true, UiTheme.Accent);
            var summary = CreateLabel("Summary", go.transform, "", 13, false, UiTheme.TextMuted);
            var body = CreateLabel("Body", go.transform, "", 13, false, UiTheme.TextMuted);
            var mark = CreateButton("MarkComplete", go.transform, "Mark complete", true);
            var practice = CreateButton("Practice", go.transform, "Practice related question", false);
            var row = go.AddComponent<LessonRowView>();
            row.Wire(title, badge, summary, body, mark, practice);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        static void Stretch(RectTransform rt)
        {
            Stretch(rt, 0, 0, 0, 0);
        }

        static void Stretch(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            rt.localScale = Vector3.one;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
