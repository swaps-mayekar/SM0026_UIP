using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class MockSessionScreen : UiScreen
    {
        [SerializeField] TMP_Text progressLabel;
        [SerializeField] TMP_Text timerLabel;
        [SerializeField] TMP_Text difficultyLabel;
        [SerializeField] TMP_Text promptLabel;
        [SerializeField] Button pauseButton;
        [SerializeField] TMP_Text pauseLabel;
        [SerializeField] Button revealButton;
        [SerializeField] GameObject revealedRoot;
        [SerializeField] TMP_Text idealLabel;
        [SerializeField] TMP_Text intentLabel;
        [SerializeField] Transform ratingRoot;
        [SerializeField] Transform confidenceRoot;
        [SerializeField] GameObject chipPrefab;
        [SerializeField] Button nextButton;
        [SerializeField] TMP_Text nextLabel;
        [SerializeField] Button abandonButton;

        public void Wire(
            TMP_Text progress,
            TMP_Text timer,
            TMP_Text difficulty,
            TMP_Text prompt,
            Button pause,
            TMP_Text pauseText,
            Button reveal,
            GameObject revealed,
            TMP_Text ideal,
            TMP_Text intent,
            Transform rating,
            Transform confidence,
            GameObject chip,
            Button next,
            TMP_Text nextText,
            Button abandon)
        {
            progressLabel = progress;
            timerLabel = timer;
            difficultyLabel = difficulty;
            promptLabel = prompt;
            pauseButton = pause;
            pauseLabel = pauseText;
            revealButton = reveal;
            revealedRoot = revealed;
            idealLabel = ideal;
            intentLabel = intent;
            ratingRoot = rating;
            confidenceRoot = confidence;
            chipPrefab = chip;
            nextButton = next;
            nextLabel = nextText;
            abandonButton = abandon;
        }

        protected override void OnBound()
        {
            BindButton(pauseButton, () =>
            {
                var mock = Router.LiveMock;
                if (mock == null)
                {
                    return;
                }

                Ctx.Mock.Pause(mock, !mock.paused);
                Refresh();
            });
            BindButton(revealButton, () =>
            {
                var mock = Router.LiveMock;
                if (mock == null)
                {
                    return;
                }

                Ctx.Mock.Reveal(mock);
                Refresh();
            });
            BindButton(nextButton, () =>
            {
                var mock = Router.LiveMock;
                if (mock == null)
                {
                    return;
                }

                var done = Ctx.Mock.SubmitCurrent(mock, Router.PendingRating, Router.PendingConfidence);
                if (done)
                {
                    var summary = Ctx.Profile.mockHistory.FirstOrDefault();
                    Router.LiveMock = null;
                    Ctx.Navigation.ShowMockSummary(summary);
                }
                else
                {
                    Refresh();
                }
            });
            BindButton(abandonButton, () =>
            {
                var mock = Router.LiveMock;
                if (mock != null)
                {
                    Ctx.Mock.Abandon(mock);
                }

                Router.LiveMock = null;
                Go(AppScreen.MockSetup);
            });
        }

        public override void Refresh()
        {
            Router.LiveMock ??= Ctx.Mock.ResumeOrNull();
            var mock = Router.LiveMock;
            if (mock == null || !mock.IsResumable)
            {
                Router.LiveMock = null;
                Go(AppScreen.MockSetup);
                return;
            }

            if (!Ctx.Content.TryGetQuestion(mock.questionIds[mock.currentIndex], out var q))
            {
                Ctx.Mock.Abandon(mock);
                Router.LiveMock = null;
                Go(AppScreen.MockSetup);
                return;
            }
            SetText(progressLabel, $"Question {mock.currentIndex + 1} / {mock.questionIds.Count}");
            UpdateTimer(mock.remainingSeconds);
            SetText(difficultyLabel, ScoreUtil.DifficultyLabel(q.difficulty));
            SetText(promptLabel, q.prompt);

            var revealed = mock.revealShown;
            SetActive(pauseButton, !revealed);
            SetActive(revealButton, !revealed);
            SetActive(revealedRoot, revealed);
            SetText(pauseLabel, mock.paused ? "Resume timer" : "Pause");

            if (!revealed)
            {
                return;
            }

            SetText(idealLabel, q.idealAnswer);
            SetText(intentLabel, q.interviewerIntent);
            SetText(nextLabel, mock.currentIndex >= mock.questionIds.Count - 1 ? "Finish interview" : "Next question");
            BuildRatingChips();
            BuildConfidenceChips();
        }

        public void UpdateTimer(float seconds)
        {
            SetText(timerLabel, ScreenRouter.FormatTime(seconds));
        }

        void BuildRatingChips()
        {
            if (ratingRoot == null || chipPrefab == null)
            {
                return;
            }

            var values = (SelfRating[])System.Enum.GetValues(typeof(SelfRating));
            UiListSpawner.Spawn<UiChipView>(ratingRoot, chipPrefab, values.Length, (chip, i) =>
            {
                var rating = values[i];
                chip.Bind(rating.ToString(), Router.PendingRating == rating, () =>
                {
                    Router.PendingRating = rating;
                    Refresh();
                });
            });
        }

        void BuildConfidenceChips()
        {
            if (confidenceRoot == null || chipPrefab == null)
            {
                return;
            }

            var values = new[] { ConfidenceLevel.Low, ConfidenceLevel.Medium, ConfidenceLevel.High };
            UiListSpawner.Spawn<UiChipView>(confidenceRoot, chipPrefab, values.Length, (chip, i) =>
            {
                var confidence = values[i];
                chip.Bind(confidence.ToString(), Router.PendingConfidence == confidence, () =>
                {
                    Router.PendingConfidence = confidence;
                    Refresh();
                });
            });
        }
    }
}
