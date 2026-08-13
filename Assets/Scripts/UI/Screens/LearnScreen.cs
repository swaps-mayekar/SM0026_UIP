using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class LearnScreen : UiScreen
    {
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;
        [SerializeField] Button mistakesButton;
        [SerializeField] Button flashcardsButton;

        public void Wire(Transform list, GameObject prefab, Button mistakes, Button flashcards)
        {
            listRoot = list;
            rowPrefab = prefab;
            mistakesButton = mistakes;
            flashcardsButton = flashcards;
        }

        protected override void OnBound()
        {
            BindButton(mistakesButton, () => Go(AppScreen.CommonMistakes));
            BindButton(flashcardsButton, () => Go(AppScreen.Flashcards));
        }

        public override void Refresh()
        {
            var paths = Ctx.Content.LearningPaths.OrderBy(p => p.sortOrder).ToList();
            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, paths.Count, (row, i) =>
            {
                var path = paths[i];
                var completion = Ctx.Progress.PathCompletion(path.id);
                row.Bind(
                    path.title,
                    path.audience,
                    path.description,
                    $"{Mathf.RoundToInt(completion * 100)}% · {path.moduleIds.Count} modules",
                    completion,
                    () => Ctx.Navigation.OpenPath(path.id));
            });
        }
    }
}
