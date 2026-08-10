using System;
using System.Collections.Generic;
using System.Linq;
using UIP.Core;
using UIP.Content;

namespace UIP.Features
{
    public sealed class ProgressService
    {
        readonly ContentRepository _content;
        readonly UserProfile _profile;
        readonly Action _persist;

        public ProgressService(ContentRepository content, UserProfile profile, Action persist)
        {
            _content = content;
            _profile = profile;
            _persist = persist;
        }

        public UserProfile Profile => _profile;

        public void CompleteOnboarding()
        {
            _profile.onboardingCompleted = true;
            Save();
        }

        public void SetPreferences(int dailyGoal, int thinkSeconds, int mockLength, bool reducedMotion, bool haptics)
        {
            _profile.dailyGoalQuestions = Math.Clamp(dailyGoal, 1, 50);
            _profile.preferredThinkSeconds = thinkSeconds;
            _profile.preferredMockLength = mockLength;
            _profile.reducedMotion = reducedMotion;
            _profile.hapticsEnabled = haptics;
            Save();
        }

        public void RememberScreen(string screen)
        {
            _profile.lastActiveScreen = screen;
            Save();
        }

        public bool IsBookmarked(string questionId) => _profile.bookmarks.Contains(questionId);

        public void ToggleBookmark(string questionId)
        {
            if (_profile.bookmarks.Contains(questionId))
            {
                _profile.bookmarks.Remove(questionId);
                AddActivity("bookmark_remove", "Removed bookmark", questionId);
            }
            else
            {
                _profile.bookmarks.Add(questionId);
                AddActivity("bookmark_add", "Bookmarked question", questionId);
            }

            Save();
        }

        public void MarkLessonComplete(string lessonId)
        {
            if (!_profile.lessons.TryGetValue(lessonId, out var progress))
            {
                progress = new LessonProgress { lessonId = lessonId };
                _profile.lessons[lessonId] = progress;
            }

            progress.completed = true;
            progress.completedIso = DateUtil.NowIso();
            if (_content.TryGetLesson(lessonId, out var lesson))
            {
                _profile.continuePathId = FindPathForLesson(lessonId);
                AddActivity("lesson", $"Completed {lesson.title}", lessonId);
            }

            TouchStudyDay();
            Save();
        }

        public bool IsLessonComplete(string lessonId)
        {
            return _profile.lessons.TryGetValue(lessonId, out var p) && p.completed;
        }

        public float PathCompletion(string pathId)
        {
            if (!_content.TryGetPath(pathId, out var path) || path.moduleIds.Count == 0)
            {
                return 0f;
            }

            var done = path.moduleIds.Count(IsLessonComplete);
            return (float)done / path.moduleIds.Count;
        }

        public void RecordQuestionAttempt(string questionId, SelfRating rating, ConfidenceLevel confidence)
        {
            if (!_profile.questions.TryGetValue(questionId, out var progress))
            {
                progress = new QuestionProgress { questionId = questionId };
                _profile.questions[questionId] = progress;
            }

            progress.timesSeen++;
            progress.timesAnswered++;
            progress.lastRating = rating;
            if ((int)rating >= (int)progress.bestRating)
            {
                progress.bestRating = rating;
            }

            progress.confidence = confidence;
            progress.lastReviewedIso = DateUtil.NowIso();
            progress.completed = rating >= SelfRating.Partial;
            _profile.continueQuestionId = questionId;
            AddActivity("question", "Practiced interview question", questionId);
            TouchStudyDay();
            Save();
        }

        public void RecordFlashcardReview(string cardId, FlashcardGrade grade, DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            if (!_profile.flashcards.TryGetValue(cardId, out var progress))
            {
                progress = new FlashcardProgress
                {
                    cardId = cardId,
                    dueIso = DateUtil.NowIso()
                };
                _profile.flashcards[cardId] = progress;
            }

            SpacedRepetition.Apply(progress, grade, now);
            AddActivity("flashcard", "Reviewed flashcard", cardId);
            TouchStudyDay();
            Save();
        }

        public IReadOnlyList<FlashcardDefinition> GetDueFlashcards(DateTime? utcNow = null, string topicId = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            var cards = string.IsNullOrEmpty(topicId)
                ? _content.Flashcards
                : _content.FlashcardsForTopic(topicId).ToList();

            return cards
                .Where(card => SpacedRepetition.IsDue(GetOrCreateCardProgress(card.id), now))
                .OrderBy(card => DateUtil.ParseIso(GetOrCreateCardProgress(card.id).dueIso))
                .ToList();
        }

        public FlashcardProgress GetOrCreateCardProgress(string cardId)
        {
            if (!_profile.flashcards.TryGetValue(cardId, out var progress))
            {
                progress = new FlashcardProgress
                {
                    cardId = cardId,
                    dueIso = DateUtil.NowIso()
                };
                _profile.flashcards[cardId] = progress;
            }

            return progress;
        }

        public QuestionProgress GetQuestionProgress(string questionId)
        {
            if (_profile.questions.TryGetValue(questionId, out var progress))
            {
                return progress;
            }

            return new QuestionProgress { questionId = questionId };
        }

        public void SaveActiveMock(MockSessionState state)
        {
            _profile.activeMock = state;
            Save();
        }

        public void ClearActiveMock()
        {
            _profile.activeMock = null;
            Save();
        }

        public void CompleteMock(MockSessionRecord record)
        {
            _profile.mockHistory.Insert(0, record);
            if (_profile.mockHistory.Count > 50)
            {
                _profile.mockHistory.RemoveAt(_profile.mockHistory.Count - 1);
            }

            _profile.activeMock = null;
            AddActivity("mock", $"Completed mock interview ({record.questionCount} Q)", record.sessionId);
            TouchStudyDay();
            Save();
        }

        public DashboardStats BuildDashboard(DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            var stats = new DashboardStats
            {
                dailyStreak = _profile.currentStreak,
                longestStreak = _profile.longestStreak,
                bookmarks = _profile.bookmarks.Count,
                mockSessions = _profile.mockHistory.Count,
                dailyGoalTarget = _profile.dailyGoalQuestions,
                flashcardsDue = GetDueFlashcards(now).Count
            };

            var answered = _profile.questions.Values.Where(q => q.timesAnswered > 0).ToList();
            stats.questionsCompleted = answered.Count(q => q.completed);
            if (answered.Count > 0)
            {
                stats.accuracy = answered.Average(q => ScoreUtil.RatingToScore(q.lastRating));
                stats.averageConfidence = answered.Average(q => ScoreUtil.ConfidenceToScore(q.confidence));
            }

            if (_profile.mockHistory.Count > 0)
            {
                stats.averageMockScore = _profile.mockHistory.Average(m => m.averageScore);
            }

            stats.dailyGoalProgress = CountTodayQuestionReviews(now);
            stats.weakTopics = BuildTopicStats()
                .OrderByDescending(t => t.weaknessScore)
                .ThenBy(t => t.averageScore)
                .Take(5)
                .ToList();

            if (stats.weakTopics.Count > 0)
            {
                stats.recommendedTopicId = stats.weakTopics[0].topicId;
                var candidate = _content.QuestionsForTopic(stats.recommendedTopicId)
                    .OrderBy(q => GetQuestionProgress(q.id).timesAnswered)
                    .ThenBy(q => q.difficulty)
                    .FirstOrDefault();
                stats.recommendedQuestionId = candidate?.id ?? _profile.continueQuestionId;
            }
            else if (!string.IsNullOrEmpty(_profile.continueQuestionId))
            {
                stats.recommendedQuestionId = _profile.continueQuestionId;
                if (_content.TryGetQuestion(_profile.continueQuestionId, out var q))
                {
                    stats.recommendedTopicId = q.topicId;
                }
            }
            else if (_content.Questions.Count > 0)
            {
                stats.recommendedQuestionId = _content.Questions[0].id;
                stats.recommendedTopicId = _content.Questions[0].topicId;
            }

            return stats;
        }

        public List<TopicStats> BuildTopicStats()
        {
            var result = new List<TopicStats>();
            foreach (var topic in _content.Topics)
            {
                var questions = _content.QuestionsForTopic(topic.id).ToList();
                if (questions.Count == 0)
                {
                    continue;
                }

                var progresses = questions.Select(q => GetQuestionProgress(q.id)).ToList();
                var answered = progresses.Where(p => p.timesAnswered > 0).ToList();
                var avgScore = answered.Count == 0 ? 0f : answered.Average(p => ScoreUtil.RatingToScore(p.lastRating));
                var avgConfidence = answered.Count == 0 ? 0f : answered.Average(p => ScoreUtil.ConfidenceToScore(p.confidence));
                var completion = (float)progresses.Count(p => p.completed) / questions.Count;
                var exposurePenalty = answered.Count == 0 ? 1f : 1f - Math.Min(1f, answered.Count / (float)questions.Count);
                var weakness = (1f - avgScore) * 0.55f + (1f - avgConfidence) * 0.25f + exposurePenalty * 0.2f;

                result.Add(new TopicStats
                {
                    topicId = topic.id,
                    totalQuestions = questions.Count,
                    completedQuestions = progresses.Count(p => p.completed),
                    averageScore = avgScore,
                    averageConfidence = avgConfidence,
                    weaknessScore = weakness
                });
            }

            return result;
        }

        public void TouchStudyDay(DateTime? utcNow = null)
        {
            StreakService.ApplyStudy(_profile, utcNow ?? DateTime.UtcNow);
        }

        public void ResetAllProgress()
        {
            var prefs = (
                _profile.dailyGoalQuestions,
                _profile.preferredThinkSeconds,
                _profile.preferredMockLength,
                _profile.reducedMotion,
                _profile.hapticsEnabled,
                _profile.onboardingCompleted,
                _profile.displayName);

            _profile.questions.Clear();
            _profile.lessons.Clear();
            _profile.flashcards.Clear();
            _profile.bookmarks.Clear();
            _profile.mockHistory.Clear();
            _profile.recentActivity.Clear();
            _profile.activeMock = null;
            _profile.currentStreak = 0;
            _profile.longestStreak = 0;
            _profile.lastStudyDate = "";
            _profile.continueQuestionId = "";
            _profile.continuePathId = "";
            _profile.dailyGoalQuestions = prefs.dailyGoalQuestions;
            _profile.preferredThinkSeconds = prefs.preferredThinkSeconds;
            _profile.preferredMockLength = prefs.preferredMockLength;
            _profile.reducedMotion = prefs.reducedMotion;
            _profile.hapticsEnabled = prefs.hapticsEnabled;
            _profile.onboardingCompleted = prefs.onboardingCompleted;
            _profile.displayName = prefs.displayName;
            Save();
        }

        int CountTodayQuestionReviews(DateTime utcNow)
        {
            var today = DateUtil.TodayDayKey(utcNow);
            return _profile.questions.Values.Count(q =>
                !string.IsNullOrEmpty(q.lastReviewedIso) &&
                DateUtil.TodayDayKey(DateUtil.ParseIso(q.lastReviewedIso)) == today);
        }

        string FindPathForLesson(string lessonId)
        {
            foreach (var path in _content.LearningPaths)
            {
                if (path.moduleIds.Contains(lessonId))
                {
                    return path.id;
                }
            }

            return _profile.continuePathId;
        }

        void AddActivity(string kind, string label, string relatedId)
        {
            _profile.recentActivity.Insert(0, new ActivityEvent
            {
                isoTimestamp = DateUtil.NowIso(),
                kind = kind,
                label = label,
                relatedId = relatedId
            });

            if (_profile.recentActivity.Count > 30)
            {
                _profile.recentActivity.RemoveAt(_profile.recentActivity.Count - 1);
            }
        }

        void Save() => _persist?.Invoke();
    }
}
