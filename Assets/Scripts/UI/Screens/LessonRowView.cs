using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public sealed class LessonRowView : MonoBehaviour
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text badge;
        [SerializeField] TMP_Text summary;
        [SerializeField] TMP_Text body;
        [SerializeField] Button markCompleteButton;
        [SerializeField] Button practiceButton;

        public void Wire(TMP_Text titleLabel, TMP_Text badgeLabel, TMP_Text summaryLabel, TMP_Text bodyLabel, Button markComplete, Button practice)
        {
            title = titleLabel;
            badge = badgeLabel;
            summary = summaryLabel;
            body = bodyLabel;
            markCompleteButton = markComplete;
            practiceButton = practice;
        }

        public void Bind(UIP.Core.LessonDefinition lesson, bool done, System.Action onComplete, System.Action onPractice)
        {
            if (title != null) title.text = lesson.title;
            if (badge != null) badge.text = done ? "Done" : $"{lesson.estimatedMinutes} min";
            if (summary != null) summary.text = lesson.summary;
            if (body != null) body.text = lesson.body;

            if (markCompleteButton != null)
            {
                markCompleteButton.gameObject.SetActive(!done);
                markCompleteButton.onClick.RemoveAllListeners();
                if (!done && onComplete != null)
                {
                    markCompleteButton.onClick.AddListener(() => onComplete());
                }
            }

            if (practiceButton != null)
            {
                var hasRelated = lesson.relatedQuestionIds != null && lesson.relatedQuestionIds.Count > 0;
                practiceButton.gameObject.SetActive(hasRelated);
                practiceButton.onClick.RemoveAllListeners();
                if (hasRelated && onPractice != null)
                {
                    practiceButton.onClick.AddListener(() => onPractice());
                }
            }
        }
    }
}
