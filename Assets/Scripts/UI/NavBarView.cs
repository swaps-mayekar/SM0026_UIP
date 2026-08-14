using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIP.Core;

namespace UIP.UI
{
    public sealed class NavBarView : MonoBehaviour
    {
        [SerializeField] Button homeButton;
        [SerializeField] Button learnButton;
        [SerializeField] Button practiceButton;
        [SerializeField] Button mockButton;
        [SerializeField] Button progressButton;
        [SerializeField] TMP_Text homeLabel;
        [SerializeField] TMP_Text learnLabel;
        [SerializeField] TMP_Text practiceLabel;
        [SerializeField] TMP_Text mockLabel;
        [SerializeField] TMP_Text progressLabel;

        AppContext _ctx;

        public void Bind(AppContext ctx)
        {
            _ctx = ctx;
            BindNav(homeButton, AppScreen.Home);
            BindNav(learnButton, AppScreen.Learn);
            BindNav(practiceButton, AppScreen.Practice);
            BindNav(mockButton, AppScreen.MockSetup);
            BindNav(progressButton, AppScreen.Progress);
        }

        public void WireSerialized(
            Button home, Button learn, Button practice, Button mock, Button progress,
            TMP_Text homeTxt, TMP_Text learnTxt, TMP_Text practiceTxt, TMP_Text mockTxt, TMP_Text progressTxt)
        {
            homeButton = home;
            learnButton = learn;
            practiceButton = practice;
            mockButton = mock;
            progressButton = progress;
            homeLabel = homeTxt;
            learnLabel = learnTxt;
            practiceLabel = practiceTxt;
            mockLabel = mockTxt;
            progressLabel = progressTxt;
        }

        public void Refresh()
        {
            if (_ctx == null)
            {
                return;
            }

            var current = _ctx.Navigation.Current;
            var hide = current is AppScreen.Onboarding
                or AppScreen.MockSession
                or AppScreen.QuestionDetail
                or AppScreen.FlashcardSession
                or AppScreen.LearnPathDetail
                or AppScreen.MistakeDetail;

            gameObject.SetActive(!hide);
            if (hide)
            {
                return;
            }

            SetActive(homeLabel, IsActive(current, AppScreen.Home));
            SetActive(learnLabel, IsActive(current, AppScreen.Learn, AppScreen.LearnPathDetail, AppScreen.CommonMistakes, AppScreen.Flashcards));
            SetActive(practiceLabel, IsActive(current, AppScreen.Practice, AppScreen.QuestionDetail, AppScreen.Bookmarks));
            SetActive(mockLabel, IsActive(current, AppScreen.MockSetup, AppScreen.MockSession, AppScreen.MockSummary));
            SetActive(progressLabel, IsActive(current, AppScreen.Progress));
        }

        void BindNav(Button button, AppScreen screen)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _ctx.Navigation.Go(screen));
        }

        static bool IsActive(AppScreen current, params AppScreen[] screens)
        {
            foreach (var screen in screens)
            {
                if (current == screen)
                {
                    return true;
                }
            }

            return false;
        }

        static void SetActive(TMP_Text label, bool active)
        {
            if (label == null)
            {
                return;
            }

            label.color = active ? UiTheme.Accent : UiTheme.TextMuted;
            label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
