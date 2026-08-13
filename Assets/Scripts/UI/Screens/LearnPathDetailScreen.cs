using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class LearnPathDetailScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [SerializeField] Image progressFill;
        [SerializeField] Transform listRoot;
        [SerializeField] GameObject rowPrefab;

        public void Wire(Button back, TMP_Text title, TMP_Text body, Image fill, Transform list, GameObject prefab)
        {
            backButton = back;
            titleLabel = title;
            bodyLabel = body;
            progressFill = fill;
            listRoot = list;
            rowPrefab = prefab;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Learn));
        }

        public override void Refresh()
        {
            if (!Ctx.Content.TryGetPath(Ctx.Navigation.SelectedPathId, out var path))
            {
                Go(AppScreen.Learn);
                return;
            }

            SetText(titleLabel, path.title);
            SetText(bodyLabel, path.description);
            if (progressFill != null)
            {
                progressFill.fillAmount = Ctx.Progress.PathCompletion(path.id);
            }

            var lessonIds = path.moduleIds;
            UiListSpawner.Spawn<LessonRowView>(listRoot, rowPrefab, lessonIds.Count, (row, i) =>
            {
                if (!Ctx.Content.TryGetLesson(lessonIds[i], out var lesson))
                {
                    row.gameObject.SetActive(false);
                    return;
                }

                var done = Ctx.Progress.IsLessonComplete(lesson.id);
                row.Bind(lesson, done,
                    () =>
                    {
                        Ctx.Progress.MarkLessonComplete(lesson.id);
                        Router.Render();
                    },
                    () =>
                    {
                        if (lesson.relatedQuestionIds.Count > 0)
                        {
                            Ctx.Navigation.OpenQuestion(lesson.relatedQuestionIds[0]);
                        }
                    });
            });
        }
    }
}
