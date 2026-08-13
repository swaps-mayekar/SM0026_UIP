using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class PracticeScreen : UiScreen
    {
        [SerializeField] TMP_InputField searchField;
        [SerializeField] Transform difficultyRoot;
        [SerializeField] Transform topicRoot;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject chipPrefab;
        [SerializeField] GameObject rowPrefab;
        [SerializeField] TMP_Text countLabel;

        public void Wire(
            TMP_InputField search,
            Transform difficulty,
            Transform topic,
            Transform list,
            GameObject chip,
            GameObject row,
            TMP_Text count)
        {
            searchField = search;
            difficultyRoot = difficulty;
            topicRoot = topic;
            listRoot = list;
            chipPrefab = chip;
            rowPrefab = row;
            countLabel = count;
        }

        protected override void OnBound()
        {
            if (searchField != null)
            {
                searchField.onValueChanged.RemoveAllListeners();
                searchField.onValueChanged.AddListener(value =>
                {
                    Router.PracticeSearch = value;
                    Refresh();
                });
            }
        }

        public override void Refresh()
        {
            if (searchField != null && searchField.text != Router.PracticeSearch)
            {
                searchField.SetTextWithoutNotify(Router.PracticeSearch);
            }

            BuildDifficultyChips();
            BuildTopicChips();

            var results = Ctx.Content.FilterQuestions(
                Ctx.Navigation.SelectedTopicId,
                Router.PracticeDifficulty,
                Router.PracticeSearch).ToList();

            SetText(countLabel, $"{results.Count} questions");
            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, results.Count, (row, i) =>
            {
                var q = results[i];
                var topicName = Ctx.Content.TryGetTopic(q.topicId, out var t) ? t.name : q.topicId;
                row.Bind(
                    null,
                    topicName,
                    q.prompt,
                    ScoreUtil.DifficultyLabel(q.difficulty),
                    -1f,
                    () => Ctx.Navigation.OpenQuestion(q.id));
            });
        }

        void BuildDifficultyChips()
        {
            if (difficultyRoot == null || chipPrefab == null)
            {
                return;
            }

            var values = new Difficulty?[] { null }
                .Concat(System.Enum.GetValues(typeof(Difficulty)).Cast<Difficulty>().Select(d => (Difficulty?)d))
                .ToList();

            UiListSpawner.Spawn<UiChipView>(difficultyRoot, chipPrefab, values.Count, (chip, i) =>
            {
                var value = values[i];
                var label = value == null ? "All" : ScoreUtil.DifficultyLabel(value.Value);
                var selected = Router.PracticeDifficulty == value;
                chip.Bind(label, selected, () =>
                {
                    Router.PracticeDifficulty = value;
                    Refresh();
                });
            });
        }

        void BuildTopicChips()
        {
            if (topicRoot == null || chipPrefab == null)
            {
                return;
            }

            var topics = Ctx.Content.Topics.ToList();
            UiListSpawner.Spawn<UiChipView>(topicRoot, chipPrefab, topics.Count + 1, (chip, i) =>
            {
                if (i == 0)
                {
                    var selected = string.IsNullOrEmpty(Ctx.Navigation.SelectedTopicId);
                    chip.Bind("All topics", selected, () =>
                    {
                        Ctx.Navigation.SetPracticeTopic(null);
                        Refresh();
                    });
                    return;
                }

                var topic = topics[i - 1];
                var isSelected = string.Equals(Ctx.Navigation.SelectedTopicId, topic.id, System.StringComparison.Ordinal);
                chip.Bind(topic.name, isSelected, () =>
                {
                    Ctx.Navigation.SetPracticeTopic(topic.id);
                    Refresh();
                });
            });
        }
    }
}
