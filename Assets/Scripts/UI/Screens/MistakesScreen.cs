using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class MistakesScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;

        public void Wire(Button back, Transform list, GameObject prefab)
        {
            backButton = back;
            listRoot = list;
            rowPrefab = prefab;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Learn));
        }

        public override void Refresh()
        {
            var mistakes = Ctx.Content.CommonMistakes.ToList();
            UiListSpawner.Spawn<UiSimpleRow>(listRoot, rowPrefab, mistakes.Count, (row, i) =>
            {
                var mistake = mistakes[i];
                var topic = Ctx.Content.GetTopic(mistake.topicId).name;
                row.Bind(mistake.title, topic, null, null, -1f, () => Ctx.Navigation.OpenMistake(mistake.id));
            });
        }
    }
}
