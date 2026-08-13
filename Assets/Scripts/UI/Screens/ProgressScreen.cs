using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class ProgressScreen : UiScreen
    {
        [SerializeField] TMP_Text questionsValue;
        [SerializeField] TMP_Text accuracyValue;
        [SerializeField] TMP_Text streakValue;
        [SerializeField] TMP_Text mocksValue;
        [SerializeField] TMP_Text mockAvgValue;
        [SerializeField] TMP_Text confidenceValue;
        [SerializeField] Transform weakRoot;
        [SerializeField] GameObject weakPrefab;
        [SerializeField] Transform activityRoot;
        [SerializeField] GameObject activityPrefab;
        [SerializeField] TMP_Text weakEmptyLabel;
        [SerializeField] Button bookmarksButton;

        public void Wire(
            TMP_Text questions,
            TMP_Text accuracy,
            TMP_Text streak,
            TMP_Text mocks,
            TMP_Text mockAvg,
            TMP_Text confidence,
            Transform weak,
            GameObject weakRow,
            Transform activity,
            GameObject activityRow,
            TMP_Text weakEmpty,
            Button bookmarks)
        {
            questionsValue = questions;
            accuracyValue = accuracy;
            streakValue = streak;
            mocksValue = mocks;
            mockAvgValue = mockAvg;
            confidenceValue = confidence;
            weakRoot = weak;
            weakPrefab = weakRow;
            activityRoot = activity;
            activityPrefab = activityRow;
            weakEmptyLabel = weakEmpty;
            bookmarksButton = bookmarks;
        }

        protected override void OnBound()
        {
            BindButton(bookmarksButton, () => Go(AppScreen.Bookmarks));
        }

        public override void Refresh()
        {
            var stats = Ctx.Progress.BuildDashboard();
            SetText(questionsValue, stats.questionsCompleted.ToString());
            SetText(accuracyValue, ScoreUtil.FormatPercent(stats.accuracy));
            SetText(streakValue, stats.dailyStreak.ToString());
            SetText(mocksValue, stats.mockSessions.ToString());
            SetText(mockAvgValue, ScoreUtil.FormatPercent(stats.averageMockScore));
            SetText(confidenceValue, ScoreUtil.FormatPercent(stats.averageConfidence));

            var weak = stats.weakTopics;
            SetActive(weakEmptyLabel, weak.Count == 0);
            UiListSpawner.Spawn<UiSimpleRow>(weakRoot, weakPrefab, weak.Count, (row, i) =>
            {
                var item = weak[i];
                var name = Ctx.Content.TryGetTopic(item.topicId, out var t) ? t.name : item.topicId;
                row.Bind(
                    name,
                    $"Completion {item.completedQuestions}/{item.totalQuestions} · Score {ScoreUtil.FormatPercent(item.averageScore)}",
                    null,
                    "Practice",
                    1f - item.weaknessScore,
                    () =>
                    {
                        Ctx.Navigation.SetPracticeTopic(item.topicId);
                        Go(AppScreen.Practice);
                    });
            });

            var activity = Ctx.Profile.recentActivity.Take(12).ToList();
            UiListSpawner.Spawn<UiSimpleRow>(activityRoot, activityPrefab, activity.Count, (row, i) =>
            {
                row.Bind(null, activity[i].label, null, null, -1f, null);
            });
        }
    }
}
