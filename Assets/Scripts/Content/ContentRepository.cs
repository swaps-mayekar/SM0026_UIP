using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UIP.Core;

namespace UIP.Content
{
    public sealed class ContentRepository
    {
        public const string CatalogResourcePath = "Content/catalog";
        public const int ExpectedSchemaVersion = 1;

        readonly ContentCatalog _catalog;
        readonly Dictionary<string, TopicDefinition> _topics;
        readonly Dictionary<string, InterviewQuestion> _questions;
        readonly Dictionary<string, LearningPath> _paths;
        readonly Dictionary<string, LessonDefinition> _lessons;
        readonly Dictionary<string, FlashcardDefinition> _flashcards;
        readonly Dictionary<string, CommonMistakeDefinition> _mistakes;

        public ContentCatalog Catalog => _catalog;
        public string ContentVersion => _catalog.contentVersion;
        public IReadOnlyList<TopicDefinition> Topics => _catalog.topics;
        public IReadOnlyList<InterviewQuestion> Questions => _catalog.questions;
        public IReadOnlyList<LearningPath> LearningPaths => _catalog.learningPaths;
        public IReadOnlyList<FlashcardDefinition> Flashcards => _catalog.flashcards;
        public IReadOnlyList<LessonDefinition> Lessons => _catalog.lessons;
        public IReadOnlyList<CommonMistakeDefinition> CommonMistakes => _catalog.commonMistakes;

        public ContentRepository(ContentCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Validate(_catalog);
            _topics = _catalog.topics.ToDictionary(t => t.id, StringComparer.Ordinal);
            _questions = _catalog.questions.ToDictionary(q => q.id, StringComparer.Ordinal);
            _paths = _catalog.learningPaths.ToDictionary(p => p.id, StringComparer.Ordinal);
            _lessons = _catalog.lessons.ToDictionary(l => l.id, StringComparer.Ordinal);
            _flashcards = _catalog.flashcards.ToDictionary(f => f.id, StringComparer.Ordinal);
            _mistakes = _catalog.commonMistakes.ToDictionary(m => m.id, StringComparer.Ordinal);
        }

        public static ContentRepository LoadFromResources(string resourcePath = CatalogResourcePath)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing content resource at Resources/{resourcePath}.json");
            }

            var catalog = JsonUtility.FromJson<ContentCatalog>(asset.text);
            return new ContentRepository(catalog);
        }

        public static ContentRepository FromJson(string json)
        {
            var catalog = JsonUtility.FromJson<ContentCatalog>(json);
            return new ContentRepository(catalog);
        }

        public static void Validate(ContentCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (catalog.schemaVersion != ExpectedSchemaVersion)
            {
                throw new InvalidOperationException(
                    $"Unsupported content schemaVersion {catalog.schemaVersion}; expected {ExpectedSchemaVersion}.");
            }

            if (string.IsNullOrWhiteSpace(catalog.contentVersion))
            {
                throw new InvalidOperationException("contentVersion is required.");
            }

            EnsureUniqueIds(catalog.topics.Select(t => t.id), "topic");
            EnsureUniqueIds(catalog.questions.Select(q => q.id), "question");
            EnsureUniqueIds(catalog.learningPaths.Select(p => p.id), "path");
            EnsureUniqueIds(catalog.lessons.Select(l => l.id), "lesson");
            EnsureUniqueIds(catalog.flashcards.Select(f => f.id), "flashcard");
            EnsureUniqueIds(catalog.commonMistakes.Select(m => m.id), "mistake");

            var topicIds = new HashSet<string>(catalog.topics.Select(t => t.id));
            var lessonIds = new HashSet<string>(catalog.lessons.Select(l => l.id));
            var questionIds = new HashSet<string>(catalog.questions.Select(q => q.id));

            foreach (var question in catalog.questions)
            {
                if (string.IsNullOrWhiteSpace(question.prompt))
                {
                    throw new InvalidOperationException($"Question {question.id} is missing prompt.");
                }

                if (!topicIds.Contains(question.topicId))
                {
                    throw new InvalidOperationException($"Question {question.id} references unknown topic {question.topicId}.");
                }
            }

            foreach (var path in catalog.learningPaths)
            {
                foreach (var moduleId in path.moduleIds)
                {
                    if (!lessonIds.Contains(moduleId))
                    {
                        throw new InvalidOperationException($"Path {path.id} references unknown lesson {moduleId}.");
                    }
                }
            }

            foreach (var lesson in catalog.lessons)
            {
                if (!topicIds.Contains(lesson.topicId))
                {
                    throw new InvalidOperationException($"Lesson {lesson.id} references unknown topic {lesson.topicId}.");
                }

                foreach (var related in lesson.relatedQuestionIds)
                {
                    if (!questionIds.Contains(related))
                    {
                        throw new InvalidOperationException($"Lesson {lesson.id} references unknown question {related}.");
                    }
                }
            }

            foreach (var card in catalog.flashcards)
            {
                if (!topicIds.Contains(card.topicId))
                {
                    throw new InvalidOperationException($"Flashcard {card.id} references unknown topic {card.topicId}.");
                }
            }

            foreach (var mistake in catalog.commonMistakes)
            {
                if (!topicIds.Contains(mistake.topicId))
                {
                    throw new InvalidOperationException($"Mistake {mistake.id} references unknown topic {mistake.topicId}.");
                }
            }
        }

        static void EnsureUniqueIds(IEnumerable<string> ids, string label)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidOperationException($"Empty {label} id found.");
                }

                if (!seen.Add(id))
                {
                    throw new InvalidOperationException($"Duplicate {label} id: {id}");
                }
            }
        }

        public bool TryGetTopic(string id, out TopicDefinition topic) => _topics.TryGetValue(id, out topic);
        public bool TryGetQuestion(string id, out InterviewQuestion question) => _questions.TryGetValue(id, out question);
        public bool TryGetPath(string id, out LearningPath path) => _paths.TryGetValue(id, out path);
        public bool TryGetLesson(string id, out LessonDefinition lesson) => _lessons.TryGetValue(id, out lesson);
        public bool TryGetFlashcard(string id, out FlashcardDefinition card) => _flashcards.TryGetValue(id, out card);
        public bool TryGetMistake(string id, out CommonMistakeDefinition mistake) => _mistakes.TryGetValue(id, out mistake);

        public TopicDefinition GetTopic(string id) => _topics[id];
        public InterviewQuestion GetQuestion(string id) => _questions[id];
        public LearningPath GetPath(string id) => _paths[id];
        public LessonDefinition GetLesson(string id) => _lessons[id];
        public FlashcardDefinition GetFlashcard(string id) => _flashcards[id];
        public CommonMistakeDefinition GetMistake(string id) => _mistakes[id];

        public IEnumerable<InterviewQuestion> FilterQuestions(string topicId = null, Difficulty? difficulty = null, string search = null)
        {
            IEnumerable<InterviewQuestion> query = _catalog.questions;
            if (!string.IsNullOrEmpty(topicId))
            {
                query = query.Where(q => q.topicId == topicId);
            }

            if (difficulty.HasValue)
            {
                query = query.Where(q => q.difficulty == difficulty.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(q =>
                    ContainsIgnoreCase(q.prompt, term) ||
                    ContainsIgnoreCase(q.interviewerIntent, term) ||
                    q.tags.Any(t => ContainsIgnoreCase(t, term)));
            }

            return query.OrderBy(q => q.difficulty).ThenBy(q => q.id);
        }

        public IEnumerable<FlashcardDefinition> FlashcardsForTopic(string topicId)
        {
            return _catalog.flashcards.Where(c => c.topicId == topicId);
        }

        public IEnumerable<InterviewQuestion> QuestionsForTopic(string topicId)
        {
            return _catalog.questions.Where(q => q.topicId == topicId);
        }

        static bool ContainsIgnoreCase(string source, string term)
        {
            return !string.IsNullOrEmpty(source) &&
                   source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
