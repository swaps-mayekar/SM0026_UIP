using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIP.UI
{
    public sealed class SplashScreen : UiScreen
    {
        [SerializeField] Button continueButton;
        [SerializeField] Image logo;

        public void Wire(Button continueBtn, Image logoImage)
        {
            continueButton = continueBtn;
            logo = logoImage;
        }

        protected override void OnBound()
        {
            BindButton(continueButton, () =>
            {
                if (!Ctx.Profile.onboardingCompleted)
                {
                    Go(Core.AppScreen.Onboarding);
                }
                else
                {
                    Go(Core.AppScreen.Home);
                }
            });

            if (logo != null && logo.sprite == null)
            {
                var sprite = Resources.Load<Sprite>("UI/AppLogo");
                if (sprite != null)
                {
                    logo.sprite = sprite;
                }
            }
        }

        public override void Refresh()
        {
        }
    }
}
