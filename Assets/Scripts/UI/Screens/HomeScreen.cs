using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class HomeScreen : UiScreen
    {
        [SerializeField] TMP_Text streakLabel;
        [SerializeField] TMP_Text continueBody;
        [SerializeField] Button resumeQuestionButton;
        [SerializeField] TMP_Text weakBody;
        [SerializeField] Button practiceWeakButton;
        [SerializeField] Button startMockButton;
        [SerializeField] Button flashcardsButton;
        [SerializeField] Button mistakesButton;
        [SerializeField] Button bookmarksButton;
        [SerializeField] Button settingsButton;
        [SerializeField] GameObject resumeMockCard;
        [SerializeField] Button resumeMockButton;

        string _resumeQuestionId;
        string _weakTopicId;

        public void Wire(
            TMP_Text streak,
            TMP_Text continueText,
            Button resumeQuestion,
            TMP_Text weakText,
            Button practiceWeak,
            Button startMock,
            Button flashcards,
            Button mistakes,
            Button bookmarks,
            Button settings,
            GameObject resumeMockRoot,
            Button resumeMock)
        {
            streakLabel = streak;
            continueBody = continueText;
            resumeQuestionButton = resumeQuestion;
            weakBody = weakText;
            practiceWeakButton = practiceWeak;
            startMockButton = startMock;
            flashcardsButton = flashcards;
            mistakesButton = mistakes;
            bookmarksButton = bookmarks;
            settingsButton = settings;
            resumeMockCard = resumeMockRoot;
            resumeMockButton = resumeMock;
        }

        protected override void OnBound()
        {
            BindButton(resumeQuestionButton, () =>
            {
                if (!string.IsNullOrEmpty(_resumeQuestionId))
                {
                    Ctx.Navigation.OpenQuestion(_resumeQuestionId);
                }
            });
            BindButton(practiceWeakButton, () =>
            {
                if (!string.IsNullOrEmpty(_weakTopicId))
                {
                    Ctx.Navigation.SetPracticeTopic(_weakTopicId);
                    Go(AppScreen.Practice);
                }
            });
            BindButton(startMockButton, () => Go(AppScreen.MockSetup));
            BindButton(flashcardsButton, () => Go(AppScreen.Flashcards));
            BindButton(mistakesButton, () => Go(AppScreen.CommonMistakes));
            BindButton(bookmarksButton, () => Go(AppScreen.Bookmarks));
            BindButton(settingsButton, () => Go(AppScreen.Settings));
            BindButton(resumeMockButton, () => Router.ResumeMock());
        }

        public override void Refresh()
        {
            var stats = Ctx.Progress.BuildDashboard();
            SetText(streakLabel, $"Streak {stats.dailyStreak} days · Goal {stats.dailyGoalProgress}/{stats.dailyGoalTarget}");

            _resumeQuestionId = null;
            if (!string.IsNullOrEmpty(stats.recommendedQuestionId) &&
                Ctx.Content.TryGetQuestion(stats.recommendedQuestionId, out var q))
            {
                _resumeQuestionId = q.id;
                var topicName = Ctx.Content.TryGetTopic(q.topicId, out var t) ? t.name : q.topicId;
                SetText(continueBody, $"{topicName}: {q.prompt}");
                SetActive(resumeQuestionButton, true);
            }
            else
            {
                SetText(continueBody, "Start with a learning path or browse practice questions.");
                SetActive(resumeQuestionButton, false);
            }

            _weakTopicId = null;
            if (!string.IsNullOrEmpty(stats.recommendedTopicId) &&
                Ctx.Content.TryGetTopic(stats.recommendedTopicId, out var topic))
            {
                _weakTopicId = topic.id;
                SetText(weakBody, $"Weak topic: {topic.name}. Practice here to raise confidence.");
                SetActive(practiceWeakButton, true);
            }
            else
            {
                SetText(weakBody, "Answer a few questions to unlock weak-topic coaching.");
                SetActive(practiceWeakButton, false);
            }

            SetActive(resumeMockCard, Ctx.Profile.activeMock != null);
        }
    }
}
