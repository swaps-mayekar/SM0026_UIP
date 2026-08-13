using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class DisclaimerScreen : UiScreen
    {
        [SerializeField] Button backButton;

        public void Wire(Button back) => backButton = back;

        protected override void OnBound()
        {
            BindButton(backButton, () => Go(AppScreen.Settings));
        }

        public override void Refresh()
        {
        }
    }
}
