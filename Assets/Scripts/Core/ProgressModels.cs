using System;
using System.Collections.Generic;

namespace UIP.Core
{
    [Serializable]
    public class UserProfile
    {
        public int schemaVersion = 1;
        public bool onboardingCompleted;
        public string displayName = "Candidate";
        public int dailyGoalQuestions = 5;
        public int preferredThinkSeconds = 120;
        public int preferredMockLength = 5;
        public int currentStreak;
        public int longestStreak;
        public string lastStudyDate = "";
        public string lastActiveScreen = "Home";
        public string continueQuestionId = "";
        public string continuePathId = "";
        public Dictionary<string, QuestionProgress> questions = new Dictionary<string, QuestionProgress>();
        public Dictionary<string, LessonProgress> lessons = new Dictionary<string, LessonProgress>();
        public Dictionary<string, FlashcardProgress> flashcards = new Dictionary<string, FlashcardProgress>();
        public List<string> bookmarks = new List<string>();
        public List<MockSessionRecord> mockHistory = new List<MockSessionRecord>();
        public MockSessionState activeMock;
        public List<ActivityEvent> recentActivity = new List<ActivityEvent>();
        public bool reducedMotion;
        public bool hapticsEnabled = true;
    }

    [Serializable]
    public class QuestionProgress
    {
        public string questionId;
        public int timesSeen;
        public int timesAnswered;
        public SelfRating bestRating = SelfRating.Missed;
        public SelfRating lastRating = SelfRating.Missed;
        public ConfidenceLevel confidence = ConfidenceLevel.None;
        public string lastReviewedIso = "";
        public bool completed;
    }

    [Serializable]
    public class LessonProgress
    {
        public string lessonId;
        public bool completed;
        public string completedIso = "";
    }

    [Serializable]
    public class FlashcardProgress
    {
        public string cardId;
        public int intervalDays = 0;
        public float ease = 2.5f;
        public int repetitions;
        public string dueIso = "";
        public FlashcardGrade lastGrade = FlashcardGrade.Again;
        public string lastReviewedIso = "";
    }

    [Serializable]
    public class MockSessionRecord
    {
        public string sessionId;
        public string startedIso;
        public string completedIso;
        public int questionCount;
        public int thinkSeconds;
        public float averageScore;
        public List<string> questionIds = new List<string>();
        public List<SelfRating> ratings = new List<SelfRating>();
        public List<ConfidenceLevel> confidences = new List<ConfidenceLevel>();
    }

    [Serializable]
    public class MockSessionState
    {
        public string sessionId;
        public int thinkSeconds = 120;
        public int currentIndex;
        public bool revealShown;
        public float remainingSeconds;
        public bool paused;
        public List<string> questionIds = new List<string>();
        public List<SelfRating> ratings = new List<SelfRating>();
        public List<ConfidenceLevel> confidences = new List<ConfidenceLevel>();
        public string startedIso;
    }

    [Serializable]
    public class ActivityEvent
    {
        public string isoTimestamp;
        public string kind;
        public string label;
        public string relatedId;
    }

    [Serializable]
    public class TopicStats
    {
        public string topicId;
        public int totalQuestions;
        public int completedQuestions;
        public float averageScore;
        public float averageConfidence;
        public float weaknessScore;
    }

    [Serializable]
    public class DashboardStats
    {
        public int questionsCompleted;
        public float accuracy;
        public int dailyStreak;
        public int longestStreak;
        public int mockSessions;
        public float averageMockScore;
        public float averageConfidence;
        public int bookmarks;
        public int flashcardsDue;
        public int dailyGoalProgress;
        public int dailyGoalTarget;
        public List<TopicStats> weakTopics = new List<TopicStats>();
        public string recommendedTopicId;
        public string recommendedQuestionId;
    }
}
