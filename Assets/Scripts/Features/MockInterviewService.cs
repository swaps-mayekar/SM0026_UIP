using System;
using System.Collections.Generic;
using System.Linq;
using UIP.Core;
using UIP.Content;

namespace UIP.Features
{
    public sealed class MockInterviewService
    {
        readonly ContentRepository _content;
        readonly ProgressService _progress;
        readonly Random _random;

        public MockInterviewService(ContentRepository content, ProgressService progress, int? seed = null)
        {
            _content = content;
            _progress = progress;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public MockSessionState StartSession(int questionCount, int thinkSeconds, string topicId = null)
        {
            questionCount = Math.Clamp(questionCount, 3, 20);
            thinkSeconds = thinkSeconds <= 0 ? 120 : thinkSeconds;
            var pool = string.IsNullOrEmpty(topicId)
                ? _content.Questions.ToList()
                : _content.QuestionsForTopic(topicId).ToList();

            if (pool.Count == 0)
            {
                throw new InvalidOperationException("No questions available for mock interview.");
            }

            var selected = WeightedSelect(pool, Math.Min(questionCount, pool.Count));
            var state = new MockSessionState
            {
                sessionId = Guid.NewGuid().ToString("N"),
                thinkSeconds = thinkSeconds,
                remainingSeconds = thinkSeconds,
                currentIndex = 0,
                revealShown = false,
                paused = false,
                questionIds = selected.Select(q => q.id).ToList(),
                ratings = Enumerable.Repeat(SelfRating.Missed, selected.Count).ToList(),
                confidences = Enumerable.Repeat(ConfidenceLevel.None, selected.Count).ToList(),
                startedIso = DateUtil.NowIso()
            };

            _progress.SaveActiveMock(state);
            return state;
        }

        public MockSessionState ResumeOrNull()
        {
            var state = _progress.Profile.activeMock;
            if (state == null)
            {
                return null;
            }

            if (!state.IsResumable)
            {
                _progress.ClearActiveMock();
                return null;
            }

            return state;
        }

        public void Tick(MockSessionState state, float deltaSeconds)
        {
            if (state == null || state.paused || state.revealShown)
            {
                return;
            }

            state.remainingSeconds = Math.Max(0f, state.remainingSeconds - deltaSeconds);
            _progress.SaveActiveMock(state);
        }

        public void Pause(MockSessionState state, bool paused)
        {
            state.paused = paused;
            _progress.SaveActiveMock(state);
        }

        public void Reveal(MockSessionState state)
        {
            state.revealShown = true;
            state.paused = true;
            _progress.SaveActiveMock(state);
        }

        public bool SubmitCurrent(MockSessionState state, SelfRating rating, ConfidenceLevel confidence)
        {
            state.ratings[state.currentIndex] = rating;
            state.confidences[state.currentIndex] = confidence;
            var questionId = state.questionIds[state.currentIndex];
            _progress.RecordQuestionAttempt(questionId, rating, confidence);

            if (state.currentIndex >= state.questionIds.Count - 1)
            {
                Finish(state);
                return true;
            }

            state.currentIndex++;
            state.revealShown = false;
            state.paused = false;
            state.remainingSeconds = state.thinkSeconds;
            _progress.SaveActiveMock(state);
            return false;
        }

        public MockSessionRecord Finish(MockSessionState state)
        {
            var scores = state.ratings.Select(ScoreUtil.RatingToScore).ToList();
            var record = new MockSessionRecord
            {
                sessionId = state.sessionId,
                startedIso = state.startedIso,
                completedIso = DateUtil.NowIso(),
                questionCount = state.questionIds.Count,
                thinkSeconds = state.thinkSeconds,
                averageScore = scores.Count == 0 ? 0f : scores.Average(),
                questionIds = new List<string>(state.questionIds),
                ratings = new List<SelfRating>(state.ratings),
                confidences = new List<ConfidenceLevel>(state.confidences)
            };

            _progress.CompleteMock(record);
            return record;
        }

        public void Abandon(MockSessionState state)
        {
            _progress.ClearActiveMock();
        }

        List<InterviewQuestion> WeightedSelect(List<InterviewQuestion> pool, int count)
        {
            var weighted = pool
                .Select(q =>
                {
                    var progress = _progress.GetQuestionProgress(q.id);
                    var weakness = 1.1f - ScoreUtil.RatingToScore(progress.lastRating);
                    var freshness = progress.timesAnswered == 0 ? 1.4f : 1f / (1f + progress.timesAnswered);
                    return (q, weight: Math.Max(0.1f, weakness * 0.7f + freshness * 0.3f));
                })
                .ToList();

            var selected = new List<InterviewQuestion>();
            while (selected.Count < count && weighted.Count > 0)
            {
                var total = weighted.Sum(w => w.weight);
                var roll = _random.NextDouble() * total;
                var cumulative = 0.0;
                var index = 0;
                for (var i = 0; i < weighted.Count; i++)
                {
                    cumulative += weighted[i].weight;
                    if (roll <= cumulative)
                    {
                        index = i;
                        break;
                    }
                }

                selected.Add(weighted[index].q);
                weighted.RemoveAt(index);
            }

            return selected;
        }
    }
}
