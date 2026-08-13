using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UIP.Core;

namespace UIP.UI
{
    public sealed class FlashcardSessionScreen : UiScreen
    {
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text progressLabel;
        [SerializeField] TMP_Text cardLabel;
        [SerializeField] Button flipButton;
        [SerializeField] Button showAnswerButton;
        [SerializeField] GameObject gradeRoot;
        [SerializeField] Button againButton;
        [SerializeField] Button hardButton;
        [SerializeField] Button goodButton;
        [SerializeField] GameObject emptyRoot;

        public void Wire(
            Button back,
            TMP_Text progress,
            TMP_Text card,
            Button flip,
            Button showAnswer,
            GameObject grade,
            Button again,
            Button hard,
            Button good,
            GameObject empty)
        {
            backButton = back;
            progressLabel = progress;
            cardLabel = card;
            flipButton = flip;
            showAnswerButton = showAnswer;
            gradeRoot = grade;
            againButton = again;
            hardButton = hard;
            goodButton = good;
            emptyRoot = empty;
        }

        protected override void OnBound()
        {
            BindButton(backButton, () =>
            {
                Router.FlashDeck = null;
                Go(AppScreen.Flashcards);
            });
            BindButton(flipButton, () =>
            {
                Router.FlashShowingBack = !Router.FlashShowingBack;
                Refresh();
            });
            BindButton(showAnswerButton, () =>
            {
                Router.FlashShowingBack = true;
                Refresh();
            });
            BindButton(againButton, () => Grade(FlashcardGrade.Again));
            BindButton(hardButton, () => Grade(FlashcardGrade.Hard));
            BindButton(goodButton, () => Grade(FlashcardGrade.Good));
        }

        public override void Refresh()
        {
            EnsureDeck();
            var deck = Router.FlashDeck;
            if (deck == null || deck.Count == 0)
            {
                SetActive(emptyRoot, true);
                SetActive(cardLabel, false);
                SetActive(showAnswerButton, false);
                SetActive(gradeRoot, false);
                SetText(progressLabel, string.Empty);
                return;
            }

            SetActive(emptyRoot, false);
            SetActive(cardLabel, true);
            var index = Mathf.Clamp(Router.FlashIndex, 0, deck.Count - 1);
            var card = deck[index];
            SetText(progressLabel, $"Card {index + 1} / {deck.Count}");
            SetText(cardLabel, Router.FlashShowingBack ? card.back : card.front);
            SetActive(showAnswerButton, !Router.FlashShowingBack);
            SetActive(gradeRoot, Router.FlashShowingBack);
        }

        void EnsureDeck()
        {
            if (Router.FlashDeck != null && Router.FlashDeck.Count > 0 && Router.FlashIndex < Router.FlashDeck.Count)
            {
                return;
            }

            Router.FlashDeck = Ctx.Progress.GetDueFlashcards(topicId: Ctx.Navigation.FlashTopicId).ToList();
            if (Router.FlashDeck.Count == 0)
            {
                Router.FlashDeck = (string.IsNullOrEmpty(Ctx.Navigation.FlashTopicId)
                    ? Ctx.Content.Flashcards
                    : Ctx.Content.FlashcardsForTopic(Ctx.Navigation.FlashTopicId)).ToList();
            }

            Router.FlashIndex = 0;
            Router.FlashShowingBack = false;
        }

        void Grade(FlashcardGrade grade)
        {
            var deck = Router.FlashDeck;
            if (deck == null || deck.Count == 0)
            {
                return;
            }

            var index = Mathf.Clamp(Router.FlashIndex, 0, deck.Count - 1);
            Router.GradeFlash(deck[index].id, grade);
        }
    }
}
