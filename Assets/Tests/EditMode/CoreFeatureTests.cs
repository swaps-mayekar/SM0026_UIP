using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UIP.Content;
using UIP.Core;
using UIP.Features;
using UIP.Persistence;

namespace UIP.Tests
{
    public class ContentRepositoryTests
    {
        [Test]
        public void Catalog_Loads_And_Validates_From_Resources()
        {
            var repo = ContentRepository.LoadFromResources();
            Assert.AreEqual(1, repo.Catalog.schemaVersion);
            Assert.GreaterOrEqual(repo.Questions.Count, 75);
            Assert.AreEqual(22, repo.Topics.Count);
            Assert.AreEqual(7, repo.LearningPaths.Count);
            Assert.Greater(repo.Flashcards.Count, 20);
            Assert.Greater(repo.CommonMistakes.Count, 5);
        }

        [Test]
        public void FilterQuestions_By_Topic_Works()
        {
            var repo = ContentRepository.LoadFromResources();
            var topicId = repo.Topics[0].id;
            var filtered = repo.FilterQuestions(topicId).ToList();
            Assert.IsNotEmpty(filtered);
            Assert.IsTrue(filtered.All(q => q.topicId == topicId));
        }

        [Test]
        public void FilterQuestions_Every_Difficulty_Has_Items()
        {
            var repo = ContentRepository.LoadFromResources();
            foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
            {
                var filtered = repo.FilterQuestions(difficulty: difficulty).ToList();
                Assert.IsNotEmpty(filtered, $"Practice filter '{difficulty}' should have questions.");
                Assert.IsTrue(filtered.All(q => q.difficulty == difficulty));
            }
        }
    }

    public class SpacedRepetitionTests
    {
        [Test]
        public void Again_Schedules_Soon_And_Resets_Repetitions()
        {
            var progress = new FlashcardProgress { cardId = "c1", ease = 2.5f, repetitions = 3, intervalDays = 10 };
            var now = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
            SpacedRepetition.Apply(progress, FlashcardGrade.Again, now);
            Assert.AreEqual(0, progress.repetitions);
            Assert.LessOrEqual(DateUtil.ParseIso(progress.dueIso), now.AddMinutes(15));
        }

        [Test]
        public void Good_Increases_Interval()
        {
            var progress = new FlashcardProgress { cardId = "c1", ease = 2.5f };
            var now = new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc);
            SpacedRepetition.Apply(progress, FlashcardGrade.Good, now);
            Assert.AreEqual(1, progress.repetitions);
            Assert.AreEqual(1, progress.intervalDays);
            SpacedRepetition.Apply(progress, FlashcardGrade.Good, now.AddDays(1));
            Assert.GreaterOrEqual(progress.intervalDays, 2);
        }
    }

    public class StreakTests
    {
        [Test]
        public void Streak_Increments_Across_Consecutive_Days()
        {
            var profile = new UserProfile();
            var day1 = new DateTime(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);
            StreakService.ApplyStudy(profile, day1);
            Assert.AreEqual(1, profile.currentStreak);
            StreakService.ApplyStudy(profile, day1.AddDays(1));
            Assert.AreEqual(2, profile.currentStreak);
            StreakService.ApplyStudy(profile, day1.AddDays(3));
            Assert.AreEqual(1, profile.currentStreak);
        }

        [Test]
        public void RefreshIfBroken_Clears_Old_Streak()
        {
            var profile = new UserProfile
            {
                currentStreak = 5,
                lastStudyDate = "2026-07-01"
            };
            StreakService.RefreshIfBroken(profile, new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc));
            Assert.AreEqual(0, profile.currentStreak);
        }
    }

    public class ProgressAndMockTests
    {
        AppContextFixture CreateFixture(int seed = 7)
        {
            return new AppContextFixture(seed);
        }

        [Test]
        public void WeakTopics_Prefer_Unpracticed_Or_Low_Scores()
        {
            var fx = CreateFixture();
            // Seed every topic with strong answers, then tank one topic.
            foreach (var topicDef in fx.Content.Topics)
            {
                foreach (var q in fx.Content.QuestionsForTopic(topicDef.id))
                {
                    fx.Progress.RecordQuestionAttempt(q.id, SelfRating.Excellent, ConfidenceLevel.High);
                }
            }

            var weakTopic = fx.Content.Topics[0].id;
            foreach (var q in fx.Content.QuestionsForTopic(weakTopic))
            {
                fx.Progress.RecordQuestionAttempt(q.id, SelfRating.Missed, ConfidenceLevel.Low);
            }

            var weak = fx.Progress.BuildTopicStats().OrderByDescending(t => t.weaknessScore).First();
            Assert.AreEqual(weakTopic, weak.topicId);
            var strong = fx.Progress.BuildTopicStats().OrderBy(t => t.weaknessScore).First();
            Assert.AreNotEqual(weakTopic, strong.topicId);
        }

        [Test]
        public void MockInterview_Completes_And_Records_History()
        {
            var fx = CreateFixture();
            var session = fx.Mock.StartSession(5, 120);
            Assert.AreEqual(5, session.questionIds.Count);
            for (var i = 0; i < 5; i++)
            {
                fx.Mock.Reveal(session);
                var done = fx.Mock.SubmitCurrent(session, SelfRating.Solid, ConfidenceLevel.Medium);
                if (i < 4)
                {
                    Assert.IsFalse(done);
                }
                else
                {
                    Assert.IsTrue(done);
                }
            }

            Assert.IsNull(fx.Profile.activeMock);
            Assert.AreEqual(1, fx.Profile.mockHistory.Count);
            Assert.Greater(fx.Profile.mockHistory[0].averageScore, 0f);
        }

        [Test]
        public void ResumeOrNull_Rejects_Empty_ActiveMock()
        {
            var fx = CreateFixture();
            fx.Profile.activeMock = new MockSessionState();
            Assert.IsFalse(fx.Profile.HasResumableMock);
            Assert.IsNull(fx.Mock.ResumeOrNull());
            Assert.IsNull(fx.Profile.activeMock);
        }

        [Test]
        public void ResumeOrNull_Returns_InProgress_Session()
        {
            var fx = CreateFixture();
            var session = fx.Mock.StartSession(3, 120);
            Assert.IsTrue(fx.Profile.HasResumableMock);
            Assert.AreSame(session, fx.Mock.ResumeOrNull());
        }

        [Test]
        public void Dashboard_Tracks_Bookmarks_And_Daily_Goal()
        {
            var fx = CreateFixture();
            var qid = fx.Content.Questions[0].id;
            fx.Progress.ToggleBookmark(qid);
            fx.Progress.RecordQuestionAttempt(qid, SelfRating.Solid, ConfidenceLevel.High);
            var dash = fx.Progress.BuildDashboard();
            Assert.AreEqual(1, dash.bookmarks);
            Assert.GreaterOrEqual(dash.dailyGoalProgress, 1);
            Assert.GreaterOrEqual(dash.questionsCompleted, 1);
        }
    }

    public class PersistenceTests
    {
        [Test]
        public void Save_Load_RoundTrip_Preserves_Progress()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "UIP_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new ProfileStore(dir);
                var profile = store.LoadOrCreate();
                profile.onboardingCompleted = true;
                profile.currentStreak = 4;
                profile.questions["q1"] = new QuestionProgress
                {
                    questionId = "q1",
                    completed = true,
                    timesAnswered = 2,
                    lastRating = SelfRating.Excellent,
                    confidence = ConfidenceLevel.High
                };
                profile.bookmarks.Add("q1");
                store.Save(profile);

                var loaded = store.LoadOrCreate();
                Assert.IsTrue(loaded.onboardingCompleted);
                Assert.AreEqual(4, loaded.currentStreak);
                Assert.IsTrue(loaded.questions.ContainsKey("q1"));
                Assert.AreEqual(SelfRating.Excellent, loaded.questions["q1"].lastRating);
                Assert.Contains("q1", loaded.bookmarks);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Test]
        public void Corrupt_Primary_Falls_Back_Or_Fresh()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "UIP_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new ProfileStore(dir);
                File.WriteAllText(store.FilePath, "{not-json");
                var profile = store.LoadOrCreate();
                Assert.IsNotNull(profile);
                Assert.AreEqual(1, profile.schemaVersion);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Test]
        public void Migrate_Fills_Defaults()
        {
            var profile = ProfileStore.Migrate(new UserProfile
            {
                schemaVersion = 0,
                dailyGoalQuestions = 0,
                preferredThinkSeconds = 0,
                displayName = ""
            });
            Assert.AreEqual(1, profile.schemaVersion);
            Assert.AreEqual(5, profile.dailyGoalQuestions);
            Assert.AreEqual(120, profile.preferredThinkSeconds);
            Assert.AreEqual("Candidate", profile.displayName);
        }

        [Test]
        public void Null_ActiveMock_Does_Not_Deserialize_As_Resumable()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "UIP_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new ProfileStore(dir);
                var profile = store.LoadOrCreate();
                profile.activeMock = null;
                store.Save(profile);

                var loaded = store.LoadOrCreate();
                Assert.IsNull(loaded.activeMock);
                Assert.IsFalse(loaded.HasResumableMock);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        [Test]
        public void ActiveMock_RoundTrip_Preserves_QuestionIds()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "UIP_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var store = new ProfileStore(dir);
                var profile = store.LoadOrCreate();
                profile.activeMock = new MockSessionState
                {
                    sessionId = "session-1",
                    thinkSeconds = 120,
                    currentIndex = 1,
                    remainingSeconds = 90f,
                    questionIds = new List<string> { "q1", "q2", "q3" },
                    ratings = new List<SelfRating> { SelfRating.Missed, SelfRating.Solid, SelfRating.Missed },
                    confidences = new List<ConfidenceLevel> { ConfidenceLevel.None, ConfidenceLevel.High, ConfidenceLevel.None },
                    startedIso = "2026-08-17T00:00:00Z"
                };
                store.Save(profile);

                var loaded = store.LoadOrCreate();
                Assert.IsNotNull(loaded.activeMock);
                Assert.IsTrue(loaded.HasResumableMock);
                Assert.AreEqual(3, loaded.activeMock.questionIds.Count);
                Assert.AreEqual("q2", loaded.activeMock.questionIds[1]);
                Assert.AreEqual(1, loaded.activeMock.currentIndex);
                Assert.AreEqual(SelfRating.Solid, loaded.activeMock.ratings[1]);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
        }
    }

    sealed class AppContextFixture
    {
        public ContentRepository Content { get; }
        public UserProfile Profile { get; }
        public ProgressService Progress { get; }
        public MockInterviewService Mock { get; }

        public AppContextFixture(int seed)
        {
            Content = ContentRepository.LoadFromResources();
            Profile = new UserProfile();
            Progress = new ProgressService(Content, Profile, () => { });
            Mock = new MockInterviewService(Content, Progress, seed);
        }
    }
}
