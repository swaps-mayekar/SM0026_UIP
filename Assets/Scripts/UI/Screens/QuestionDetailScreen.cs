using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class QuestionDetailScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text topicLabel;
        [SerializeField] TMP_Text difficultyLabel;
        [SerializeField] TMP_Text promptLabel;
        [SerializeField] TMP_Text metaLabel;
        [SerializeField] Button bookmarkButton;
        [SerializeField] TMP_Text bookmarkLabel;
        [SerializeField] Button revealButton;
        [SerializeField] GameObject revealedRoot;
        [SerializeField] TMP_Text intentLabel;
        [SerializeField] TMP_Text idealLabel;
        [SerializeField] TMP_Text mistakesLabel;
        [SerializeField] TMP_Text followUpsLabel;
        [SerializeField] TMP_Text codeLabel;
        [SerializeField] Transform ratingRoot;
        [SerializeField] Transform confidenceRoot;
        [SerializeField] GameObject chipPrefab;
        [SerializeField] Button saveButton;

        public void Wire(
            Button back,
            TMP_Text topic,
            TMP_Text difficulty,
            TMP_Text prompt,
            TMP_Text meta,
            Button bookmark,
            TMP_Text bookmarkText,
            Button reveal,
            GameObject revealed,
            TMP_Text intent,
            TMP_Text ideal,
            TMP_Text mistakes,
            TMP_Text followUps,
            TMP_Text code,
            Transform rating,
            Transform confidence,
            GameObject chip,
            Button save)
        {
            backButton = back;
            topicLabel = topic;
            difficultyLabel = difficulty;
            promptLabel = prompt;
            metaLabel = meta;
            bookmarkButton = bookmark;
            bookmarkLabel = bookmarkText;
            revealButton = reveal;
            revealedRoot = revealed;
            intentLabel = intent;
            idealLabel = ideal;
            mistakesLabel = mistakes;
            followUpsLabel = followUps;
            codeLabel = code;
            ratingRoot = rating;
            confidenceRoot = confidence;
            chipPrefab = chip;
            saveButton = save;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Practice));
            BindButton(bookmarkButton, () =>
            {
                if (Ctx.Content.TryGetQuestion(Ctx.Navigation.SelectedQuestionId, out var q))
                {
                    Ctx.Progress.ToggleBookmark(q.id);
                    Refresh();
                }
            });
            BindButton(revealButton, () =>
            {
                Router.AnswerRevealed = true;
                Refresh();
            });
            BindButton(saveButton, () =>
            {
                if (!Ctx.Content.TryGetQuestion(Ctx.Navigation.SelectedQuestionId, out var q))
                {
                    return;
                }

                Ctx.Progress.RecordQuestionAttempt(q.id, Router.PendingRating, Router.PendingConfidence);
                Router.AnswerRevealed = false;
                Go(AppScreen.Practice);
            });
        }

        public override void Refresh()
        {
            if (!Ctx.Content.TryGetQuestion(Ctx.Navigation.SelectedQuestionId, out var q))
            {
                Go(AppScreen.Practice);
                return;
            }

            if (Router.RevealedQuestionId != q.id)
            {
                Router.AnswerRevealed = false;
                Router.RevealedQuestionId = q.id;
            }

            var progress = Ctx.Progress.GetQuestionProgress(q.id);
            SetText(topicLabel, Ctx.Content.GetTopic(q.topicId).name);
            SetText(difficultyLabel, ScoreUtil.DifficultyLabel(q.difficulty));
            SetText(promptLabel, q.prompt);
            SetText(metaLabel, $"Think time ~{q.estimatedSeconds / 60} min · Seen {progress.timesSeen}x");
            SetText(bookmarkLabel, Ctx.Progress.IsBookmarked(q.id) ? "Remove bookmark" : "Bookmark");

            var revealed = Router.AnswerRevealed;
            SetActive(revealButton, !revealed);
            if (revealed)
            {
                TmpUiFixer.PrepareChipRowContainer(ratingRoot);
                TmpUiFixer.PrepareChipRowContainer(confidenceRoot);
            }

            SetActive(revealedRoot, revealed);
            if (!revealed)
            {
                return;
            }

            SetText(intentLabel, q.interviewerIntent);
            SetText(idealLabel, q.idealAnswer);
            SetText(mistakesLabel, JoinBullets(q.commonMistakes));
            SetText(followUpsLabel, JoinBullets(q.followUps));
            SetText(codeLabel, q.codeSnippet);
            SetActive(codeLabel, !string.IsNullOrWhiteSpace(q.codeSnippet));
            BuildRatingChips();
            BuildConfidenceChips();
            if (revealedRoot != null)
            {
                TmpUiFixer.Fix(revealedRoot.transform);
                TmpUiFixer.RebuildLayoutChain(revealedRoot.transform);
            }
        }

        void BuildRatingChips()
        {
            if (ratingRoot == null || chipPrefab == null)
            {
                return;
            }

            var values = ((SelfRating[])System.Enum.GetValues(typeof(SelfRating)))
                .Where(r => r != SelfRating.Missed)
                .ToArray();
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

        static string JoinBullets(System.Collections.Generic.IList<string> items)
        {
            if (items == null || items.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.Append("• ").Append(item).Append('\n');
            }

            return sb.ToString().TrimEnd();
        }
    }
}
