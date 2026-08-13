using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public sealed class OnboardingScreen : UiScreen
    {
        [SerializeField] Button getStartedButton;

        public void Wire(Button getStarted)
        {
            getStartedButton = getStarted;
        }

        protected override void OnBound()
        {
            BindButton(getStartedButton, () =>
            {
                Ctx.Progress.CompleteOnboarding();
                Go(Core.AppScreen.Home);
            });
        }

        public override void Refresh()
        {
        }
    }
}
