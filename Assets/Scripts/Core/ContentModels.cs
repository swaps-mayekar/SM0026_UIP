using System;
using System.Collections.Generic;

namespace UIP.Core
{
    [Serializable]
    public class ContentCatalog
    {
        public int schemaVersion = 1;
        public string contentVersion = "1.0.0";
        public List<TopicDefinition> topics = new List<TopicDefinition>();
        public List<InterviewQuestion> questions = new List<InterviewQuestion>();
        public List<LearningPath> learningPaths = new List<LearningPath>();
        public List<FlashcardDefinition> flashcards = new List<FlashcardDefinition>();
        public List<LessonDefinition> lessons = new List<LessonDefinition>();
        public List<CommonMistakeDefinition> commonMistakes = new List<CommonMistakeDefinition>();
    }

    [Serializable]
    public class TopicDefinition
    {
        public string id;
        public string name;
        public string description;
        public string iconKey;
        public int sortOrder;
    }

    [Serializable]
    public class InterviewQuestion
    {
        public string id;
        public string topicId;
        public string prompt;
        public string interviewerIntent;
        public string idealAnswer;
        public List<string> commonMistakes = new List<string>();
        public List<string> followUps = new List<string>();
        public Difficulty difficulty = Difficulty.Junior;
        public int estimatedSeconds = 120;
        public List<string> tags = new List<string>();
        public string codeSnippet;
    }

    [Serializable]
    public class LearningPath
    {
        public string id;
        public string title;
        public string audience;
        public string description;
        public List<string> moduleIds = new List<string>();
        public int sortOrder;
    }

    [Serializable]
    public class LessonDefinition
    {
        public string id;
        public string title;
        public string topicId;
        public string summary;
        public string body;
        public List<string> relatedQuestionIds = new List<string>();
        public int estimatedMinutes = 5;
    }

    [Serializable]
    public class FlashcardDefinition
    {
        public string id;
        public string topicId;
        public string front;
        public string back;
        public Difficulty difficulty = Difficulty.Beginner;
    }

    [Serializable]
    public class CommonMistakeDefinition
    {
        public string id;
        public string title;
        public string topicId;
        public string whyProblem;
        public string interviewerExpectation;
        public string betterAlternative;
        public string codeAntiPattern;
        public string codeBetterPattern;
    }
}
