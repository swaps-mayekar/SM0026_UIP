using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class MockSummaryScreen : UiScreen
    {
        [SerializeField] TMP_Text scoreLabel;
        [SerializeField] TMP_Text metaLabel;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;
        [SerializeField] Button homeButton;
        [SerializeField] Button againButton;

        public void Wire(TMP_Text score, TMP_Text meta, Transform list, GameObject prefab, Button home, Button again)
        {
            scoreLabel = score;
            metaLabel = meta;
            listRoot = list;
            rowPrefab = prefab;
            homeButton = home;
            againButton = again;
        }

        protected override void OnBound()
        {
            BindButton(homeButton, () => Go(AppScreen.Home));
            BindButton(againButton, () => Go(AppScreen.MockSetup));
        }

        public override void Refresh()
        {
            var record = Ctx.Navigation.LastMockSummary ?? Ctx.Profile.mockHistory.FirstOrDefault();
            if (record == null)
            {
                SetText(scoreLabel, "—");
                SetText(metaLabel, "No mock results yet.");
                UiListSpawner.Clear(listRoot);
                return;
            }

            SetText(scoreLabel, ScoreUtil.FormatPercent(record.averageScore));
            SetText(metaLabel, $"{record.questionCount} questions · {record.thinkSeconds}s think time");
            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, record.questionIds.Count, (row, i) =>
            {
                var q = Ctx.Content.GetQuestion(record.questionIds[i]);
                row.Bind(
                    null,
                    $"Q{i + 1} · {record.ratings[i]} · {record.confidences[i]}",
                    q.prompt,
                    null,
                    -1f,
                    null);
            });
        }
    }
}
