using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class AboutScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text versionLabel;
        [SerializeField] Button disclaimerButton;

        public void Wire(Button back, TMP_Text version, Button disclaimer)
        {
            backButton = back;
            versionLabel = version;
            disclaimerButton = disclaimer;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Settings));
            BindButton(disclaimerButton, () => Go(AppScreen.Disclaimer));
        }

        public override void Refresh()
        {
            SetText(versionLabel, $"App content version: {Ctx.Content.ContentVersion}");
        }
    }
}
