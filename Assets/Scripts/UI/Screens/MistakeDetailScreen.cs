using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class MistakeDetailScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text whyLabel;
        [SerializeField] TMP_Text expectLabel;
        [SerializeField] TMP_Text betterLabel;
        [SerializeField] TMP_Text antiLabel;
        [SerializeField] TMP_Text preferredLabel;

        public void Wire(
            Button back,
            TMP_Text title,
            TMP_Text why,
            TMP_Text expect,
            TMP_Text better,
            TMP_Text anti,
            TMP_Text preferred)
        {
            backButton = back;
            titleLabel = title;
            whyLabel = why;
            expectLabel = expect;
            betterLabel = better;
            antiLabel = anti;
            preferredLabel = preferred;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.CommonMistakes));
        }

        public override void Refresh()
        {
            if (!Ctx.Content.TryGetMistake(Ctx.Navigation.SelectedMistakeId, out var mistake))
            {
                Go(AppScreen.CommonMistakes);
                return;
            }

            SetText(titleLabel, mistake.title);
            SetText(whyLabel, mistake.whyProblem);
            SetText(expectLabel, mistake.interviewerExpectation);
            SetText(betterLabel, mistake.betterAlternative);
            SetText(antiLabel, mistake.codeAntiPattern);
            SetText(preferredLabel, mistake.codeBetterPattern);
            SetActive(antiLabel, !string.IsNullOrWhiteSpace(mistake.codeAntiPattern));
            SetActive(preferredLabel, !string.IsNullOrWhiteSpace(mistake.codeBetterPattern));
        }
    }
}
