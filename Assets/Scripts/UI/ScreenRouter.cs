using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UIP.Core;

namespace UIP.UI
{
    public sealed class ScreenRouter : MonoBehaviour
    {
        [SerializeField] NavBarView navBar;
        [SerializeField] List<UiScreen> screens = new List<UiScreen>();

        AppContext _ctx;
        MockSessionState _liveMock;
        float _mockAccumulator;

        string _practiceSearch = "";
        Difficulty? _practiceDifficulty;
        bool _answerRevealed;
        string _revealedQuestionId;
        ConfidenceLevel _pendingConfidence = ConfidenceLevel.Medium;
        SelfRating _pendingRating = SelfRating.Solid;
        bool _flashShowingBack;
        int _flashIndex;
        List<FlashcardDefinition> _flashDeck;

        public string PracticeSearch
        {
            get => _practiceSearch;
            set => _practiceSearch = value ?? string.Empty;
        }

        public Difficulty? PracticeDifficulty
        {
            get => _practiceDifficulty;
            set => _practiceDifficulty = value;
        }

        public bool AnswerRevealed
        {
            get => _answerRevealed;
            set => _answerRevealed = value;
        }

        public string RevealedQuestionId
        {
            get => _revealedQuestionId;
            set => _revealedQuestionId = value;
        }

        public ConfidenceLevel PendingConfidence
        {
            get => _pendingConfidence;
            set => _pendingConfidence = value;
        }

        public SelfRating PendingRating
        {
            get => _pendingRating;
            set => _pendingRating = value;
        }

        public bool FlashShowingBack
        {
            get => _flashShowingBack;
            set => _flashShowingBack = value;
        }

        public int FlashIndex
        {
            get => _flashIndex;
            set => _flashIndex = value;
        }

        public List<FlashcardDefinition> FlashDeck
        {
            get => _flashDeck;
            set => _flashDeck = value;
        }

        public MockSessionState LiveMock
        {
            get => _liveMock;
            set => _liveMock = value;
        }

        public void Initialize(AppContext ctx, NavBarView nav, IEnumerable<UiScreen> sceneScreens)
        {
            _ctx = ctx;
            navBar = nav;
            screens = sceneScreens?.Where(s => s != null).ToList() ?? new List<UiScreen>();
            foreach (var screen in screens)
            {
                screen.Bind(ctx, this);
                screen.Hide();
            }

            navBar?.Bind(ctx);
        }

        public void SetScreens(IEnumerable<UiScreen> sceneScreens)
        {
            screens = sceneScreens?.Where(s => s != null).ToList() ?? new List<UiScreen>();
        }

        public void SetNavBar(NavBarView nav) => navBar = nav;

        public void Render()
        {
            if (_ctx == null)
            {
                return;
            }

            var current = _ctx.Navigation.Current;
            UiScreen active = null;
            foreach (var screen in screens)
            {
                if (screen == null)
                {
                    continue;
                }

                if (screen.ScreenId == current)
                {
                    active = screen;
                    screen.Show();
                }
                else
                {
                    screen.Hide();
                }
            }

            if (active == null)
            {
                _ctx.Navigation.Go(AppScreen.Home);
                return;
            }

            navBar?.Refresh();
        }

        public void Tick(float dt)
        {
            if (_ctx == null || _ctx.Navigation.Current != AppScreen.MockSession || _liveMock == null)
            {
                return;
            }

            if (_liveMock.paused || _liveMock.revealShown)
            {
                return;
            }

            _mockAccumulator += dt;
            if (_mockAccumulator < 0.25f)
            {
                return;
            }

            _ctx.Mock.Tick(_liveMock, _mockAccumulator);
            _mockAccumulator = 0f;
            var mockScreen = GetScreen<MockSessionScreen>();
            mockScreen?.UpdateTimer(_liveMock.remainingSeconds);
        }

        public T GetScreen<T>() where T : UiScreen
        {
            foreach (var screen in screens)
            {
                if (screen is T typed)
                {
                    return typed;
                }
            }

            return null;
        }

        public void StartMock(int count, int think)
        {
            _ctx.Progress.SetPreferences(
                _ctx.Profile.dailyGoalQuestions,
                think,
                count,
                _ctx.Profile.reducedMotion,
                _ctx.Profile.hapticsEnabled);
            _liveMock = _ctx.Mock.StartSession(count, think);
            _mockAccumulator = 0f;
            _ctx.Navigation.Go(AppScreen.MockSession);
        }

        public void ResumeMock()
        {
            _liveMock = _ctx.Mock.ResumeOrNull();
            _mockAccumulator = 0f;
            if (_liveMock != null)
            {
                _ctx.Navigation.Go(AppScreen.MockSession);
            }
        }

        public void GradeFlash(string id, FlashcardGrade grade)
        {
            _ctx.Progress.RecordFlashcardReview(id, grade);
            _flashIndex++;
            _flashShowingBack = false;
            if (_flashDeck == null || _flashIndex >= _flashDeck.Count)
            {
                _flashDeck = null;
                _ctx.Navigation.Go(AppScreen.Flashcards);
            }
            else
            {
                Render();
            }
        }

        public static string FormatTime(float seconds)
        {
            var s = Mathf.CeilToInt(Mathf.Max(0, seconds));
            var m = s / 60;
            var r = s % 60;
            return $"{m:0}:{r:00}";
        }
    }
}
