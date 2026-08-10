namespace UIP.Core
{
    public enum Difficulty
    {
        Beginner = 1,
        Junior = 2,
        Mid = 3,
        Senior = 4,
        Lead = 5
    }

    public enum ConfidenceLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public enum SelfRating
    {
        Missed = 0,
        Partial = 1,
        Solid = 2,
        Excellent = 3
    }

    public enum FlashcardGrade
    {
        Again = 0,
        Hard = 1,
        Good = 2
    }

    public enum AppScreen
    {
        Splash,
        Onboarding,
        Home,
        Learn,
        LearnPathDetail,
        Practice,
        QuestionDetail,
        MockSetup,
        MockSession,
        MockSummary,
        Flashcards,
        FlashcardSession,
        Progress,
        Bookmarks,
        CommonMistakes,
        MistakeDetail,
        Settings,
        About,
        Privacy,
        Disclaimer
    }
}
