using System;
using UIP.Core;

namespace UIP.UI
{
    public sealed class NavigationService
    {
        AppScreen _current = AppScreen.Home;
        string _selectedPathId;
        string _selectedQuestionId;
        string _selectedMistakeId;
        string _selectedTopicId;
        string _flashTopicId;
        MockSessionRecord _lastMockSummary;

        public AppScreen Current => _current;
        public string SelectedPathId => _selectedPathId;
        public string SelectedQuestionId => _selectedQuestionId;
        public string SelectedMistakeId => _selectedMistakeId;
        public string SelectedTopicId => _selectedTopicId;
        public string FlashTopicId => _flashTopicId;
        public MockSessionRecord LastMockSummary => _lastMockSummary;

        public event Action Navigated;

        public void Go(AppScreen screen)
        {
            _current = screen;
            Navigated?.Invoke();
        }

        public void OpenPath(string pathId)
        {
            _selectedPathId = pathId;
            Go(AppScreen.LearnPathDetail);
        }

        public void OpenQuestion(string questionId)
        {
            _selectedQuestionId = questionId;
            Go(AppScreen.QuestionDetail);
        }

        public void OpenMistake(string mistakeId)
        {
            _selectedMistakeId = mistakeId;
            Go(AppScreen.MistakeDetail);
        }

        public void OpenFlashcards(string topicId = null)
        {
            _flashTopicId = topicId;
            Go(AppScreen.FlashcardSession);
        }

        public void SetPracticeTopic(string topicId)
        {
            _selectedTopicId = topicId;
        }

        public void ShowMockSummary(MockSessionRecord record)
        {
            _lastMockSummary = record;
            Go(AppScreen.MockSummary);
        }
    }
}
