using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class MockSetupScreen : UiScreen
    {
        [SerializeField] TMP_Text prefsLabel;
        [SerializeField] Button option5x2;
        [SerializeField] Button option5x3;
        [SerializeField] Button option10x2;
        [SerializeField] Button option10x3;
        [SerializeField] Button resumeButton;

        public void Wire(TMP_Text prefs, Button a, Button b, Button c, Button d, Button resume)
        {
            prefsLabel = prefs;
            option5x2 = a;
            option5x3 = b;
            option10x2 = c;
            option10x3 = d;
            resumeButton = resume;
        }

        protected override void OnBound()
        {
            BindButton(option5x2, () => Router.StartMock(5, 120));
            BindButton(option5x3, () => Router.StartMock(5, 180));
            BindButton(option10x2, () => Router.StartMock(10, 120));
            BindButton(option10x3, () => Router.StartMock(10, 180));
            BindButton(resumeButton, () => Router.ResumeMock());
        }

        public override void Refresh()
        {
            SetText(prefsLabel, $"Preferred length: {Ctx.Profile.preferredMockLength} · Think: {Ctx.Profile.preferredThinkSeconds}s");
            SetActive(resumeButton, Ctx.Profile.activeMock != null);
        }
    }
}
