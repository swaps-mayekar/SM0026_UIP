using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class FlashcardsHubScreen : UiScreen
    {
        [SerializeField] TMP_Text dueLabel;
        [SerializeField] Button reviewDueButton;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;

        public void Wire(TMP_Text due, Button reviewDue, Transform list, GameObject prefab)
        {
            dueLabel = due;
            reviewDueButton = reviewDue;
            listRoot = list;
            rowPrefab = prefab;
        }

        protected override void OnBound()
        {
            BindButton(reviewDueButton, () => Ctx.Navigation.OpenFlashcards(null));
        }

        public override void Refresh()
        {
            var due = Ctx.Progress.GetDueFlashcards();
            SetText(dueLabel, $"{due.Count} cards due now (spaced review).");

            var topics = Ctx.Content.Topics
                .Where(t => Ctx.Content.FlashcardsForTopic(t.id).Any())
                .ToList();

            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, topics.Count, (row, i) =>
            {
                var topic = topics[i];
                var count = Ctx.Content.FlashcardsForTopic(topic.id).Count();
                var dueCount = Ctx.Progress.GetDueFlashcards(topicId: topic.id).Count;
                row.Bind(topic.name, $"{count} cards · {dueCount} due", null, null, -1f,
                    () => Ctx.Navigation.OpenFlashcards(topic.id));
            });
        }
    }
}
