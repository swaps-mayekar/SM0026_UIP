using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class SettingsScreen : UiScreen
    {
        [SerializeField] TMP_Text versionLabel;
        [SerializeField] TMP_Text goalLabel;
        [SerializeField] Button goal3Button;
        [SerializeField] Button goal5Button;
        [SerializeField] Button goal10Button;
        [SerializeField] Button reducedMotionButton;
        [SerializeField] TMP_Text reducedMotionLabel;
        [SerializeField] Button hapticsButton;
        [SerializeField] TMP_Text hapticsLabel;
        [SerializeField] Button aboutButton;
        [SerializeField] Button disclaimerButton;
        [SerializeField] Button privacyButton;
        [SerializeField] Button exportButton;
        [SerializeField] Button resetButton;

        public void Wire(
            TMP_Text version,
            TMP_Text goal,
            Button g3,
            Button g5,
            Button g10,
            Button reducedMotion,
            TMP_Text reducedMotionText,
            Button haptics,
            TMP_Text hapticsText,
            Button about,
            Button disclaimer,
            Button privacy,
            Button export,
            Button reset)
        {
            versionLabel = version;
            goalLabel = goal;
            goal3Button = g3;
            goal5Button = g5;
            goal10Button = g10;
            reducedMotionButton = reducedMotion;
            reducedMotionLabel = reducedMotionText;
            hapticsButton = haptics;
            hapticsLabel = hapticsText;
            aboutButton = about;
            disclaimerButton = disclaimer;
            privacyButton = privacy;
            exportButton = export;
            resetButton = reset;
        }

        protected override void OnBound()
        {
            BindButton(goal3Button, () => SetGoal(3));
            BindButton(goal5Button, () => SetGoal(5));
            BindButton(goal10Button, () => SetGoal(10));
            BindButton(reducedMotionButton, () =>
            {
                Ctx.Progress.SetPreferences(
                    Ctx.Profile.dailyGoalQuestions,
                    Ctx.Profile.preferredThinkSeconds,
                    Ctx.Profile.preferredMockLength,
                    !Ctx.Profile.reducedMotion,
                    Ctx.Profile.hapticsEnabled);
                Refresh();
            });
            BindButton(hapticsButton, () =>
            {
                Ctx.Progress.SetPreferences(
                    Ctx.Profile.dailyGoalQuestions,
                    Ctx.Profile.preferredThinkSeconds,
                    Ctx.Profile.preferredMockLength,
                    Ctx.Profile.reducedMotion,
                    !Ctx.Profile.hapticsEnabled);
                Refresh();
            });
            BindButton(aboutButton, () => Go(AppScreen.About));
            BindButton(disclaimerButton, () => Go(AppScreen.Disclaimer));
            BindButton(privacyButton, () => Go(AppScreen.Privacy));
            BindButton(exportButton, () =>
            {
                GUIUtility.systemCopyBuffer = Ctx.Store.ExportSummary(Ctx.Profile);
            });
            BindButton(resetButton, () =>
            {
                Ctx.ResetProgress();
                Go(AppScreen.Home);
            });
        }

        public override void Refresh()
        {
            SetText(versionLabel, $"Content version {Ctx.Content.ContentVersion}");
            SetText(goalLabel, $"Daily goal: {Ctx.Profile.dailyGoalQuestions} questions");
            SetText(reducedMotionLabel, Ctx.Profile.reducedMotion ? "Reduced motion: On" : "Reduced motion: Off");
            SetText(hapticsLabel, Ctx.Profile.hapticsEnabled ? "Haptics: On" : "Haptics: Off");
        }

        void SetGoal(int goal)
        {
            Ctx.Progress.SetPreferences(
                goal,
                Ctx.Profile.preferredThinkSeconds,
                Ctx.Profile.preferredMockLength,
                Ctx.Profile.reducedMotion,
                Ctx.Profile.hapticsEnabled);
            Refresh();
        }
    }
}
