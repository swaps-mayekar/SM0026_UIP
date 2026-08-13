using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class BookmarksScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;
        [SerializeField] GameObject emptyRoot;

        public void Wire(Button back, Transform list, GameObject prefab, GameObject empty)
        {
            backButton = back;
            listRoot = list;
            rowPrefab = prefab;
            emptyRoot = empty;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Home));
        }

        public override void Refresh()
        {
            var ids = Ctx.Profile.bookmarks.ToList();
            SetActive(emptyRoot, ids.Count == 0);
            var questions = ids
                .Select(id => Ctx.Content.TryGetQuestion(id, out var q) ? q : null)
                .Where(q => q != null)
                .ToList();

            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, questions.Count, (row, i) =>
            {
                var q = questions[i];
                row.Bind(null, null, q.prompt, null, -1f, () => Ctx.Navigation.OpenQuestion(q.id));
            });
        }
    }
}
