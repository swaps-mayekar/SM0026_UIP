using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UIP.Core;
using UIP.Features;

namespace UIP.UI
{
    public sealed class ScreenRouter
    {
        readonly AppContext _ctx;
        readonly VisualElement _host;
        readonly Action _refreshChrome;
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
        System.Collections.Generic.List<UIP.Core.FlashcardDefinition> _flashDeck;

        public ScreenRouter(AppContext ctx, VisualElement host, Action refreshChrome)
        {
            _ctx = ctx;
            _host = host;
            _refreshChrome = refreshChrome;
        }

        public void Render()
        {
            _host.Clear();
            switch (_ctx.Navigation.Current)
            {
                case AppScreen.Splash:
                    BuildSplash();
                    break;
                case AppScreen.Onboarding:
                    BuildOnboarding();
                    break;
                case AppScreen.Home:
                    BuildHome();
                    break;
                case AppScreen.Learn:
                    BuildLearn();
                    break;
                case AppScreen.LearnPathDetail:
                    BuildLearnPathDetail();
                    break;
                case AppScreen.Practice:
                    BuildPractice();
                    break;
                case AppScreen.QuestionDetail:
                    BuildQuestionDetail();
                    break;
                case AppScreen.MockSetup:
                    BuildMockSetup();
                    break;
                case AppScreen.MockSession:
                    BuildMockSession();
                    break;
                case AppScreen.MockSummary:
                    BuildMockSummary();
                    break;
                case AppScreen.Flashcards:
                    BuildFlashcardsHub();
                    break;
                case AppScreen.FlashcardSession:
                    BuildFlashcardSession();
                    break;
                case AppScreen.Progress:
                    BuildProgress();
                    break;
                case AppScreen.Bookmarks:
                    BuildBookmarks();
                    break;
                case AppScreen.CommonMistakes:
                    BuildMistakes();
                    break;
                case AppScreen.MistakeDetail:
                    BuildMistakeDetail();
                    break;
                case AppScreen.Settings:
                    BuildSettings();
                    break;
                case AppScreen.About:
                    BuildAbout();
                    break;
                case AppScreen.Privacy:
                    BuildPrivacy();
                    break;
                case AppScreen.Disclaimer:
                    BuildDisclaimer();
                    break;
                default:
                    BuildHome();
                    break;
            }

            _refreshChrome?.Invoke();
        }

        public void Tick(float dt)
        {
            if (_ctx.Navigation.Current != AppScreen.MockSession || _liveMock == null)
            {
                return;
            }

            if (_liveMock.paused || _liveMock.revealShown)
            {
                return;
            }

            _mockAccumulator += dt;
            if (_mockAccumulator >= 0.25f)
            {
                _ctx.Mock.Tick(_liveMock, _mockAccumulator);
                _mockAccumulator = 0f;
                // Update timer label if present.
                var timer = _host.Q<Label>("mock-timer");
                if (timer != null)
                {
                    timer.text = FormatTime(_liveMock.remainingSeconds);
                }
            }
        }

        void BuildSplash()
        {
            var root = UiFactory.Scroll();
            var logo = new Image { scaleMode = ScaleMode.ScaleToFit };
            var sprite = Resources.Load<Sprite>("UI/AppLogo");
            if (sprite == null)
            {
                // Fallback to texture if sprite sub-asset isn't available via Resources.
                var tex = Resources.Load<Texture2D>("UI/AppLogo");
                if (tex != null)
                {
                    logo.image = tex;
                }
            }
            else
            {
                logo.sprite = sprite;
            }

            logo.AddToClassList("logo");
            root.Add(logo);
            root.Add(UiFactory.Title("Unity Interview Prep", "splash-title"));
            root.Add(UiFactory.Title("Learn · Practice · Test · Improve", "splash-sub"));
            root.Add(UiFactory.Muted("Independent educational resource for Unity developers. Offline-first study tools with adaptive recommendations."));
            root.Add(UiFactory.Primary("Continue", () =>
            {
                if (!_ctx.Profile.onboardingCompleted)
                {
                    _ctx.Navigation.Go(AppScreen.Onboarding);
                }
                else
                {
                    _ctx.Navigation.Go(AppScreen.Home);
                }
            }));
            _host.Add(root);
        }

        void BuildOnboarding()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Welcome"));
            root.Add(UiFactory.Body("This app helps you prepare for Unity interviews with learning paths, interview questions, timed mock interviews, flashcards, and a progress dashboard."));
            root.Add(UiFactory.Section("How it works"));
            root.Add(Bullet("Learn concepts through role-based paths"));
            root.Add(Bullet("Practice 100+ original interview prompts"));
            root.Add(Bullet("Test yourself with timed mock interviews"));
            root.Add(Bullet("Improve using weak-topic tracking and streaks"));
            root.Add(UiFactory.Section("Important"));
            root.Add(UiFactory.Body("Unity is a trademark of Unity Technologies. This application is an independent educational resource and is not affiliated with, endorsed by, or sponsored by Unity Technologies."));
            root.Add(UiFactory.Primary("Get started", () =>
            {
                _ctx.Progress.CompleteOnboarding();
                _ctx.Navigation.Go(AppScreen.Home);
            }));
            _host.Add(root);
        }

        void BuildHome()
        {
            var stats = _ctx.Progress.BuildDashboard();
            var root = UiFactory.Scroll();
            var header = new VisualElement();
            header.Add(UiFactory.Title("Home"));
            header.Add(UiFactory.Muted($"Streak {stats.dailyStreak} days · Goal {stats.dailyGoalProgress}/{stats.dailyGoalTarget}"));
            root.Add(header);

            var continueCard = UiFactory.Card();
            continueCard.Add(UiFactory.Title("Continue studying", "card-title"));
            if (!string.IsNullOrEmpty(stats.recommendedQuestionId) &&
                _ctx.Content.TryGetQuestion(stats.recommendedQuestionId, out var q))
            {
                var topicName = _ctx.Content.TryGetTopic(q.topicId, out var t) ? t.name : q.topicId;
                continueCard.Add(UiFactory.Body($"{topicName}: {q.prompt}"));
                continueCard.Add(UiFactory.Primary("Resume question", () => _ctx.Navigation.OpenQuestion(q.id)));
            }
            else
            {
                continueCard.Add(UiFactory.Body("Start with a learning path or browse practice questions."));
            }

            root.Add(continueCard);

            var weak = UiFactory.Card();
            weak.Add(UiFactory.Title("Recommended focus", "card-title"));
            if (!string.IsNullOrEmpty(stats.recommendedTopicId) &&
                _ctx.Content.TryGetTopic(stats.recommendedTopicId, out var topic))
            {
                weak.Add(UiFactory.Body($"Weak topic: {topic.name}. Practice here to raise confidence."));
                weak.Add(UiFactory.Secondary("Practice this topic", () =>
                {
                    _ctx.Navigation.SetPracticeTopic(topic.id);
                    _ctx.Navigation.Go(AppScreen.Practice);
                }));
            }
            else
            {
                weak.Add(UiFactory.Body("Answer a few questions to unlock weak-topic coaching."));
            }

            root.Add(weak);

            var quick = UiFactory.Card();
            quick.Add(UiFactory.Title("Quick actions", "card-title"));
            quick.Add(UiFactory.Primary("Start mock interview", () => _ctx.Navigation.Go(AppScreen.MockSetup)));
            quick.Add(UiFactory.Secondary("Review flashcards", () => _ctx.Navigation.Go(AppScreen.Flashcards)));
            quick.Add(UiFactory.Secondary("Common mistakes", () => _ctx.Navigation.Go(AppScreen.CommonMistakes)));
            quick.Add(UiFactory.Secondary("Bookmarks", () => _ctx.Navigation.Go(AppScreen.Bookmarks)));
            quick.Add(UiFactory.Secondary("Settings", () => _ctx.Navigation.Go(AppScreen.Settings)));
            root.Add(quick);

            if (_ctx.Profile.activeMock != null)
            {
                var resume = UiFactory.Card();
                resume.Add(UiFactory.Title("Interrupted mock interview", "card-title"));
                resume.Add(UiFactory.Body("You have an unfinished timed session."));
                resume.Add(UiFactory.Primary("Resume mock", () =>
                {
                    _liveMock = _ctx.Mock.ResumeOrNull();
                    _ctx.Navigation.Go(AppScreen.MockSession);
                }));
                root.Add(resume);
            }

            _host.Add(root);
        }

        void BuildLearn()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Learn"));
            root.Add(UiFactory.Muted("Role-based paths with original lessons."));
            foreach (var path in _ctx.Content.LearningPaths.OrderBy(p => p.sortOrder))
            {
                var completion = _ctx.Progress.PathCompletion(path.id);
                var card = new Button(() => _ctx.Navigation.OpenPath(path.id));
                card.AddToClassList("list-button");
                card.Add(UiFactory.Title(path.title, "card-title"));
                card.Add(UiFactory.Muted(path.audience));
                card.Add(UiFactory.Body(path.description));
                card.Add(UiFactory.ProgressBar(completion));
                card.Add(UiFactory.Muted($"{Mathf.RoundToInt(completion * 100)}% complete · {path.moduleIds.Count} modules"));
                root.Add(card);
            }

            root.Add(UiFactory.Secondary("Browse common mistakes", () => _ctx.Navigation.Go(AppScreen.CommonMistakes)));
            root.Add(UiFactory.Secondary("Flashcards", () => _ctx.Navigation.Go(AppScreen.Flashcards)));
            _host.Add(root);
        }

        void BuildLearnPathDetail()
        {
            if (!_ctx.Content.TryGetPath(_ctx.Navigation.SelectedPathId, out var path))
            {
                _ctx.Navigation.Go(AppScreen.Learn);
                return;
            }

            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Learn)));
            root.Add(UiFactory.Title(path.title));
            root.Add(UiFactory.Body(path.description));
            root.Add(UiFactory.ProgressBar(_ctx.Progress.PathCompletion(path.id)));

            foreach (var lessonId in path.moduleIds)
            {
                if (!_ctx.Content.TryGetLesson(lessonId, out var lesson))
                {
                    continue;
                }

                var done = _ctx.Progress.IsLessonComplete(lessonId);
                var card = UiFactory.Card();
                var row = UiFactory.RowSpread();
                row.Add(UiFactory.Title(lesson.title, "card-title"));
                row.Add(UiFactory.Chip(done ? "Done" : $"{lesson.estimatedMinutes} min", done));
                card.Add(row);
                card.Add(UiFactory.Body(lesson.summary));
                card.Add(UiFactory.Body(lesson.body));
                if (!done)
                {
                    card.Add(UiFactory.Primary("Mark complete", () =>
                    {
                        _ctx.Progress.MarkLessonComplete(lesson.id);
                        Render();
                    }));
                }

                if (lesson.relatedQuestionIds.Count > 0)
                {
                    card.Add(UiFactory.Secondary("Practice related question", () =>
                        _ctx.Navigation.OpenQuestion(lesson.relatedQuestionIds[0])));
                }

                root.Add(card);
            }

            _host.Add(root);
        }

        void BuildPractice()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Practice"));
            root.Add(UiFactory.SearchField("Search questions", value =>
            {
                _practiceSearch = value;
                Render();
            }));

            var filters = new VisualElement();
            filters.AddToClassList("row");
            filters.style.flexWrap = Wrap.Wrap;
            filters.Add(FilterChip("All", null));
            foreach (Difficulty d in Enum.GetValues(typeof(Difficulty)))
            {
                filters.Add(FilterChip(ScoreUtil.DifficultyLabel(d), d));
            }

            root.Add(filters);

            var topicBar = new VisualElement();
            topicBar.AddToClassList("row");
            topicBar.style.flexWrap = Wrap.Wrap;
            topicBar.Add(TopicChip("All topics", null));
            foreach (var topic in _ctx.Content.Topics)
            {
                topicBar.Add(TopicChip(topic.name, topic.id));
            }

            root.Add(topicBar);

            var results = _ctx.Content.FilterQuestions(
                _ctx.Navigation.SelectedTopicId,
                _practiceDifficulty,
                _practiceSearch).ToList();

            root.Add(UiFactory.Muted($"{results.Count} questions"));
            foreach (var q in results)
            {
                var topicName = _ctx.Content.TryGetTopic(q.topicId, out var t) ? t.name : q.topicId;
                var button = new Button(() => _ctx.Navigation.OpenQuestion(q.id));
                button.AddToClassList("list-button");
                var row = UiFactory.RowSpread();
                row.Add(UiFactory.Muted(topicName));
                row.Add(UiFactory.DifficultyBadge(q.difficulty));
                button.Add(row);
                button.Add(UiFactory.Body(q.prompt));
                root.Add(button);
            }

            _host.Add(root);
        }

        Label FilterChip(string label, Difficulty? difficulty)
        {
            var selected = _practiceDifficulty == difficulty;
            var chip = UiFactory.Chip(label, selected);
            chip.RegisterCallback<ClickEvent>(_ =>
            {
                _practiceDifficulty = difficulty;
                Render();
            });
            return chip;
        }

        Label TopicChip(string label, string topicId)
        {
            var selected = string.Equals(_ctx.Navigation.SelectedTopicId, topicId, StringComparison.Ordinal);
            var chip = UiFactory.Chip(label, selected);
            chip.RegisterCallback<ClickEvent>(_ =>
            {
                _ctx.Navigation.SetPracticeTopic(topicId);
                Render();
            });
            return chip;
        }

        void BuildQuestionDetail()
        {
            if (!_ctx.Content.TryGetQuestion(_ctx.Navigation.SelectedQuestionId, out var q))
            {
                _ctx.Navigation.Go(AppScreen.Practice);
                return;
            }

            if (_revealedQuestionId != q.id)
            {
                _answerRevealed = false;
                _revealedQuestionId = q.id;
            }
            var progress = _ctx.Progress.GetQuestionProgress(q.id);
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Practice)));
            var top = UiFactory.RowSpread();
            top.Add(UiFactory.Muted(_ctx.Content.GetTopic(q.topicId).name));
            top.Add(UiFactory.DifficultyBadge(q.difficulty));
            root.Add(top);
            root.Add(UiFactory.Title("Interview question"));
            root.Add(UiFactory.Body(q.prompt));
            root.Add(UiFactory.Muted($"Think time ~{q.estimatedSeconds / 60} min · Seen {progress.timesSeen}x"));

            var bookmarkLabel = _ctx.Progress.IsBookmarked(q.id) ? "Remove bookmark" : "Bookmark";
            root.Add(UiFactory.Secondary(bookmarkLabel, () =>
            {
                _ctx.Progress.ToggleBookmark(q.id);
                Render();
            }));

            if (!_answerRevealed)
            {
                root.Add(UiFactory.Primary("Reveal recommended answer", () =>
                {
                    _answerRevealed = true;
                    Render();
                }));
            }
            else
            {
                root.Add(UiFactory.Section("Interviewer's intent"));
                root.Add(UiFactory.Body(q.interviewerIntent));
                root.Add(UiFactory.Section("Ideal answer"));
                root.Add(UiFactory.Body(q.idealAnswer));
                root.Add(UiFactory.Section("Common mistakes"));
                foreach (var m in q.commonMistakes)
                {
                    root.Add(Bullet(m));
                }

                root.Add(UiFactory.Section("Follow-ups"));
                foreach (var f in q.followUps)
                {
                    root.Add(Bullet(f));
                }

                if (!string.IsNullOrWhiteSpace(q.codeSnippet))
                {
                    root.Add(UiFactory.Section("Code sketch"));
                    root.Add(UiFactory.Code(q.codeSnippet));
                }

                root.Add(UiFactory.Section("Self-rate your answer"));
                root.Add(RatingRow());
                root.Add(UiFactory.Section("Confidence"));
                root.Add(ConfidenceRow());
                root.Add(UiFactory.Primary("Save progress", () =>
                {
                    _ctx.Progress.RecordQuestionAttempt(q.id, _pendingRating, _pendingConfidence);
                    _answerRevealed = false;
                    _ctx.Navigation.Go(AppScreen.Practice);
                }));
            }

            _host.Add(root);
        }

        void BuildMockSetup()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Mock interview"));
            root.Add(UiFactory.Body("Timed questions with thinking time, reveal, and self-rating. Progress is tracked locally."));
            root.Add(UiFactory.Muted($"Preferred length: {_ctx.Profile.preferredMockLength} · Think: {_ctx.Profile.preferredThinkSeconds}s"));

            root.Add(UiFactory.Primary("5 questions · 2 min each", () => StartMock(5, 120)));
            root.Add(UiFactory.Secondary("5 questions · 3 min each", () => StartMock(5, 180)));
            root.Add(UiFactory.Secondary("10 questions · 2 min each", () => StartMock(10, 120)));
            root.Add(UiFactory.Secondary("10 questions · 3 min each", () => StartMock(10, 180)));
            if (_ctx.Profile.activeMock != null)
            {
                root.Add(UiFactory.Primary("Resume interrupted session", () =>
                {
                    _liveMock = _ctx.Mock.ResumeOrNull();
                    _ctx.Navigation.Go(AppScreen.MockSession);
                }));
            }

            _host.Add(root);
        }

        void StartMock(int count, int think)
        {
            _ctx.Progress.SetPreferences(_ctx.Profile.dailyGoalQuestions, think, count, _ctx.Profile.reducedMotion, _ctx.Profile.hapticsEnabled);
            _liveMock = _ctx.Mock.StartSession(count, think);
            _mockAccumulator = 0f;
            _ctx.Navigation.Go(AppScreen.MockSession);
        }

        void BuildMockSession()
        {
            _liveMock ??= _ctx.Mock.ResumeOrNull();
            if (_liveMock == null)
            {
                _ctx.Navigation.Go(AppScreen.MockSetup);
                return;
            }

            var q = _ctx.Content.GetQuestion(_liveMock.questionIds[_liveMock.currentIndex]);
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Muted($"Question {_liveMock.currentIndex + 1} / {_liveMock.questionIds.Count}"));
            var timer = UiFactory.Title(FormatTime(_liveMock.remainingSeconds), "timer");
            timer.name = "mock-timer";
            root.Add(timer);
            root.Add(UiFactory.DifficultyBadge(q.difficulty));
            root.Add(UiFactory.Body(q.prompt));

            if (!_liveMock.revealShown)
            {
                root.Add(UiFactory.Secondary(_liveMock.paused ? "Resume timer" : "Pause", () =>
                {
                    _ctx.Mock.Pause(_liveMock, !_liveMock.paused);
                    Render();
                }));
                root.Add(UiFactory.Primary("Reveal recommended answer", () =>
                {
                    _ctx.Mock.Reveal(_liveMock);
                    Render();
                }));
            }
            else
            {
                root.Add(UiFactory.Section("Ideal answer"));
                root.Add(UiFactory.Body(q.idealAnswer));
                root.Add(UiFactory.Section("Interviewer's intent"));
                root.Add(UiFactory.Body(q.interviewerIntent));
                root.Add(UiFactory.Section("Self-rate"));
                root.Add(RatingRow());
                root.Add(ConfidenceRow());
                root.Add(UiFactory.Primary(_liveMock.currentIndex >= _liveMock.questionIds.Count - 1 ? "Finish interview" : "Next question", () =>
                {
                    var done = _ctx.Mock.SubmitCurrent(_liveMock, _pendingRating, _pendingConfidence);
                    if (done)
                    {
                        var summary = _ctx.Profile.mockHistory.FirstOrDefault();
                        _liveMock = null;
                        _ctx.Navigation.ShowMockSummary(summary);
                    }
                    else
                    {
                        Render();
                    }
                }));
            }

            root.Add(UiFactory.Secondary("Abandon session", () =>
            {
                _ctx.Mock.Abandon(_liveMock);
                _liveMock = null;
                _ctx.Navigation.Go(AppScreen.MockSetup);
            }));
            _host.Add(root);
        }

        void BuildMockSummary()
        {
            var record = _ctx.Navigation.LastMockSummary ?? _ctx.Profile.mockHistory.FirstOrDefault();
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Mock summary"));
            if (record == null)
            {
                root.Add(UiFactory.Body("No mock results yet."));
            }
            else
            {
                root.Add(UiFactory.StatCell(ScoreUtil.FormatPercent(record.averageScore), "Average score"));
                root.Add(UiFactory.Body($"{record.questionCount} questions · {record.thinkSeconds}s think time"));
                for (var i = 0; i < record.questionIds.Count; i++)
                {
                    var q = _ctx.Content.GetQuestion(record.questionIds[i]);
                    var card = UiFactory.Card();
                    card.Add(UiFactory.Muted($"Q{i + 1} · {record.ratings[i]} · {record.confidences[i]}"));
                    card.Add(UiFactory.Body(q.prompt));
                    root.Add(card);
                }
            }

            root.Add(UiFactory.Primary("Back to home", () => _ctx.Navigation.Go(AppScreen.Home)));
            root.Add(UiFactory.Secondary("Try another mock", () => _ctx.Navigation.Go(AppScreen.MockSetup)));
            _host.Add(root);
        }

        void BuildFlashcardsHub()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Flashcards"));
            var due = _ctx.Progress.GetDueFlashcards();
            root.Add(UiFactory.Body($"{due.Count} cards due now (spaced review)."));
            root.Add(UiFactory.Primary("Review due cards", () => _ctx.Navigation.OpenFlashcards(null)));
            foreach (var topic in _ctx.Content.Topics)
            {
                var count = _ctx.Content.FlashcardsForTopic(topic.id).Count();
                if (count == 0)
                {
                    continue;
                }

                var dueCount = _ctx.Progress.GetDueFlashcards(topicId: topic.id).Count;
                var button = new Button(() => _ctx.Navigation.OpenFlashcards(topic.id));
                button.AddToClassList("list-button");
                button.Add(UiFactory.Title(topic.name, "card-title"));
                button.Add(UiFactory.Muted($"{count} cards · {dueCount} due"));
                root.Add(button);
            }

            _host.Add(root);
        }

        void BuildFlashcardSession()
        {
            if (_flashDeck == null || _flashDeck.Count == 0 || _flashIndex >= _flashDeck.Count)
            {
                _flashDeck = _ctx.Progress.GetDueFlashcards(topicId: _ctx.Navigation.FlashTopicId).ToList();
                if (_flashDeck.Count == 0)
                {
                    _flashDeck = (string.IsNullOrEmpty(_ctx.Navigation.FlashTopicId)
                        ? _ctx.Content.Flashcards
                        : _ctx.Content.FlashcardsForTopic(_ctx.Navigation.FlashTopicId)).ToList();
                }

                _flashIndex = 0;
                _flashShowingBack = false;
            }

            var root = UiFactory.Scroll();
            root.Add(Back(() =>
            {
                _flashDeck = null;
                _ctx.Navigation.Go(AppScreen.Flashcards);
            }));

            if (_flashDeck.Count == 0)
            {
                root.Add(UiFactory.Title("No flashcards", "empty"));
                _host.Add(root);
                return;
            }

            var card = _flashDeck[Mathf.Clamp(_flashIndex, 0, _flashDeck.Count - 1)];
            root.Add(UiFactory.Muted($"Card {_flashIndex + 1} / {_flashDeck.Count}"));
            var panel = new VisualElement();
            panel.AddToClassList("flash-card");
            var text = new Label(_flashShowingBack ? card.back : card.front);
            text.AddToClassList("flash-text");
            panel.Add(text);
            panel.RegisterCallback<ClickEvent>(_ =>
            {
                _flashShowingBack = !_flashShowingBack;
                Render();
            });
            root.Add(panel);
            root.Add(UiFactory.Muted("Tap card to flip"));

            if (_flashShowingBack)
            {
                var row = new VisualElement();
                row.AddToClassList("row");
                row.Add(UiFactory.Small("Again", () => GradeFlash(card.id, FlashcardGrade.Again), false));
                row.Add(UiFactory.Small("Hard", () => GradeFlash(card.id, FlashcardGrade.Hard), false));
                row.Add(UiFactory.Small("Good", () => GradeFlash(card.id, FlashcardGrade.Good), true));
                root.Add(row);
            }
            else
            {
                root.Add(UiFactory.Primary("Show answer", () =>
                {
                    _flashShowingBack = true;
                    Render();
                }));
            }

            _host.Add(root);
        }

        void GradeFlash(string id, FlashcardGrade grade)
        {
            _ctx.Progress.RecordFlashcardReview(id, grade);
            _flashIndex++;
            _flashShowingBack = false;
            if (_flashIndex >= _flashDeck.Count)
            {
                _flashDeck = null;
                _ctx.Navigation.Go(AppScreen.Flashcards);
            }
            else
            {
                Render();
            }
        }

        void BuildProgress()
        {
            var stats = _ctx.Progress.BuildDashboard();
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Improve"));
            var grid = new VisualElement();
            grid.AddToClassList("stat-grid");
            grid.Add(UiFactory.StatCell(stats.questionsCompleted.ToString(), "Questions completed"));
            grid.Add(UiFactory.StatCell(ScoreUtil.FormatPercent(stats.accuracy), "Self-rated accuracy"));
            grid.Add(UiFactory.StatCell(stats.dailyStreak.ToString(), "Daily streak"));
            grid.Add(UiFactory.StatCell(stats.mockSessions.ToString(), "Mock interviews"));
            grid.Add(UiFactory.StatCell(ScoreUtil.FormatPercent(stats.averageMockScore), "Mock avg score"));
            grid.Add(UiFactory.StatCell(ScoreUtil.FormatPercent(stats.averageConfidence), "Confidence"));
            root.Add(grid);

            root.Add(UiFactory.Section("Weak topics"));
            if (stats.weakTopics.Count == 0)
            {
                root.Add(UiFactory.Muted("Practice more to generate coaching signals."));
            }
            else
            {
                foreach (var weak in stats.weakTopics)
                {
                    var name = _ctx.Content.TryGetTopic(weak.topicId, out var t) ? t.name : weak.topicId;
                    var card = UiFactory.Card();
                    card.Add(UiFactory.Title(name, "card-title"));
                    card.Add(UiFactory.Body($"Completion {weak.completedQuestions}/{weak.totalQuestions} · Score {ScoreUtil.FormatPercent(weak.averageScore)}"));
                    card.Add(UiFactory.ProgressBar(1f - weak.weaknessScore));
                    card.Add(UiFactory.Secondary("Practice", () =>
                    {
                        _ctx.Navigation.SetPracticeTopic(weak.topicId);
                        _ctx.Navigation.Go(AppScreen.Practice);
                    }));
                    root.Add(card);
                }
            }

            root.Add(UiFactory.Section("Recent activity"));
            foreach (var activity in _ctx.Profile.recentActivity.Take(12))
            {
                root.Add(UiFactory.Muted($"{activity.label}"));
            }

            root.Add(UiFactory.Secondary("Bookmarks", () => _ctx.Navigation.Go(AppScreen.Bookmarks)));
            _host.Add(root);
        }

        void BuildBookmarks()
        {
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Home)));
            root.Add(UiFactory.Title("Bookmarks"));
            if (_ctx.Profile.bookmarks.Count == 0)
            {
                root.Add(UiFactory.Title("No bookmarks yet", "empty"));
            }
            else
            {
                foreach (var id in _ctx.Profile.bookmarks.ToList())
                {
                    if (!_ctx.Content.TryGetQuestion(id, out var q))
                    {
                        continue;
                    }

                    var button = new Button(() => _ctx.Navigation.OpenQuestion(q.id));
                    button.AddToClassList("list-button");
                    button.Add(UiFactory.Body(q.prompt));
                    root.Add(button);
                }
            }

            _host.Add(root);
        }

        void BuildMistakes()
        {
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Learn)));
            root.Add(UiFactory.Title("Common interview mistakes"));
            foreach (var mistake in _ctx.Content.CommonMistakes)
            {
                var button = new Button(() => _ctx.Navigation.OpenMistake(mistake.id));
                button.AddToClassList("list-button");
                button.Add(UiFactory.Title(mistake.title, "card-title"));
                button.Add(UiFactory.Muted(_ctx.Content.GetTopic(mistake.topicId).name));
                root.Add(button);
            }

            _host.Add(root);
        }

        void BuildMistakeDetail()
        {
            if (!_ctx.Content.TryGetMistake(_ctx.Navigation.SelectedMistakeId, out var mistake))
            {
                _ctx.Navigation.Go(AppScreen.CommonMistakes);
                return;
            }

            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.CommonMistakes)));
            root.Add(UiFactory.Title(mistake.title));
            root.Add(UiFactory.Section("Why it's a problem"));
            root.Add(UiFactory.Body(mistake.whyProblem));
            root.Add(UiFactory.Section("What interviewers expect"));
            root.Add(UiFactory.Body(mistake.interviewerExpectation));
            root.Add(UiFactory.Section("Better alternative"));
            root.Add(UiFactory.Body(mistake.betterAlternative));
            if (!string.IsNullOrWhiteSpace(mistake.codeAntiPattern))
            {
                root.Add(UiFactory.Section("Anti-pattern"));
                root.Add(UiFactory.Code(mistake.codeAntiPattern));
            }

            if (!string.IsNullOrWhiteSpace(mistake.codeBetterPattern))
            {
                root.Add(UiFactory.Section("Preferred pattern"));
                root.Add(UiFactory.Code(mistake.codeBetterPattern));
            }

            _host.Add(root);
        }

        void BuildSettings()
        {
            var root = UiFactory.Scroll();
            root.Add(UiFactory.Title("Settings"));
            root.Add(UiFactory.Body($"Content version {_ctx.Content.ContentVersion}"));
            root.Add(UiFactory.Muted($"Daily goal: {_ctx.Profile.dailyGoalQuestions} questions"));
            var goals = new VisualElement();
            goals.AddToClassList("row");
            foreach (var g in new[] { 3, 5, 10 })
            {
                goals.Add(UiFactory.Small($"{g}/day", () =>
                {
                    _ctx.Progress.SetPreferences(g, _ctx.Profile.preferredThinkSeconds, _ctx.Profile.preferredMockLength, _ctx.Profile.reducedMotion, _ctx.Profile.hapticsEnabled);
                    Render();
                }, g == _ctx.Profile.dailyGoalQuestions));
            }

            root.Add(goals);
            root.Add(UiFactory.Secondary(_ctx.Profile.reducedMotion ? "Reduced motion: On" : "Reduced motion: Off", () =>
            {
                _ctx.Progress.SetPreferences(_ctx.Profile.dailyGoalQuestions, _ctx.Profile.preferredThinkSeconds, _ctx.Profile.preferredMockLength, !_ctx.Profile.reducedMotion, _ctx.Profile.hapticsEnabled);
                Render();
            }));
            root.Add(UiFactory.Secondary(_ctx.Profile.hapticsEnabled ? "Haptics: On" : "Haptics: Off", () =>
            {
                _ctx.Progress.SetPreferences(_ctx.Profile.dailyGoalQuestions, _ctx.Profile.preferredThinkSeconds, _ctx.Profile.preferredMockLength, _ctx.Profile.reducedMotion, !_ctx.Profile.hapticsEnabled);
                Render();
            }));
            root.Add(UiFactory.Secondary("About", () => _ctx.Navigation.Go(AppScreen.About)));
            root.Add(UiFactory.Secondary("Disclaimer", () => _ctx.Navigation.Go(AppScreen.Disclaimer)));
            root.Add(UiFactory.Secondary("Privacy", () => _ctx.Navigation.Go(AppScreen.Privacy)));
            root.Add(UiFactory.Secondary("Export local summary", () =>
            {
                GUIUtility.systemCopyBuffer = _ctx.Store.ExportSummary(_ctx.Profile);
            }));
            root.Add(UiFactory.Primary("Reset all progress", () =>
            {
                _ctx.ResetProgress();
                _ctx.Navigation.Go(AppScreen.Home);
            }));
            _host.Add(root);
        }

        void BuildAbout()
        {
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Settings)));
            root.Add(UiFactory.Title("About"));
            root.Add(UiFactory.Body("Unity Interview Prep by Gold Box helps candidates prepare for Unity developer interviews with original educational content, timed mock interviews, flashcards, and progress analytics."));
            root.Add(UiFactory.Body($"App content version: {_ctx.Content.ContentVersion}"));
            root.Add(UiFactory.Body("All explanations and sample code are original educational material. They are not copied from Unity Manual, Scripting API, or Unity Learn."));
            root.Add(UiFactory.Secondary("View disclaimer", () => _ctx.Navigation.Go(AppScreen.Disclaimer)));
            _host.Add(root);
        }

        void BuildDisclaimer()
        {
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Settings)));
            root.Add(UiFactory.Title("Disclaimer"));
            root.Add(UiFactory.Body("Unity is a trademark of Unity Technologies. This application is an independent educational resource and is not affiliated with, endorsed by, or sponsored by Unity Technologies."));
            root.Add(UiFactory.Body("Interview outcomes depend on many factors beyond this app. Content is for education and practice only."));
            _host.Add(root);
        }

        void BuildPrivacy()
        {
            var root = UiFactory.Scroll();
            root.Add(Back(() => _ctx.Navigation.Go(AppScreen.Settings)));
            root.Add(UiFactory.Title("Privacy"));
            root.Add(UiFactory.Body("This MVP stores progress only on your device (Application.persistentDataPath). It does not require an account and does not include ads or third-party analytics SDKs."));
            root.Add(UiFactory.Body("You can reset progress or copy a local summary export from Settings. No personal identity data is collected by the app itself."));
            _host.Add(root);
        }

        VisualElement RatingRow()
        {
            var row = new VisualElement();
            row.AddToClassList("row");
            row.style.flexWrap = Wrap.Wrap;
            foreach (SelfRating rating in Enum.GetValues(typeof(SelfRating)))
            {
                var captured = rating;
                row.Add(UiFactory.Small(captured.ToString(), () => { _pendingRating = captured; Render(); }, captured == _pendingRating));
            }

            return row;
        }

        VisualElement ConfidenceRow()
        {
            var row = new VisualElement();
            row.AddToClassList("row");
            foreach (var confidence in new[] { ConfidenceLevel.Low, ConfidenceLevel.Medium, ConfidenceLevel.High })
            {
                var captured = confidence;
                row.Add(UiFactory.Small(captured.ToString(), () => { _pendingConfidence = captured; Render(); }, captured == _pendingConfidence));
            }

            return row;
        }

        Button Back(Action action) => UiFactory.Small("← Back", action);

        Label Bullet(string text)
        {
            var label = UiFactory.Body("• " + text);
            return label;
        }

        static string FormatTime(float seconds)
        {
            var s = Mathf.CeilToInt(Mathf.Max(0, seconds));
            var m = s / 60;
            var r = s % 60;
            return $"{m:0}:{r:00}";
        }
    }
}
