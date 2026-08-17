using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UIP.Core;

namespace UIP.Persistence
{
    /// <summary>
    /// Serializes dictionaries that Unity's JsonUtility cannot handle natively.
    /// </summary>
    [Serializable]
    public class SerializableProfile
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
        public List<QuestionProgress> questions = new List<QuestionProgress>();
        public List<LessonProgress> lessons = new List<LessonProgress>();
        public List<FlashcardProgress> flashcards = new List<FlashcardProgress>();
        public List<string> bookmarks = new List<string>();
        public List<MockSessionRecord> mockHistory = new List<MockSessionRecord>();
        public MockSessionState activeMock;
        public List<ActivityEvent> recentActivity = new List<ActivityEvent>();
        public bool reducedMotion;
        public bool hapticsEnabled = true;
    }

    public static class ProfileSerializer
    {
        public static SerializableProfile ToSerializable(UserProfile profile)
        {
            var data = new SerializableProfile
            {
                schemaVersion = profile.schemaVersion,
                onboardingCompleted = profile.onboardingCompleted,
                displayName = profile.displayName,
                dailyGoalQuestions = profile.dailyGoalQuestions,
                preferredThinkSeconds = profile.preferredThinkSeconds,
                preferredMockLength = profile.preferredMockLength,
                currentStreak = profile.currentStreak,
                longestStreak = profile.longestStreak,
                lastStudyDate = profile.lastStudyDate,
                lastActiveScreen = profile.lastActiveScreen,
                continueQuestionId = profile.continueQuestionId,
                continuePathId = profile.continuePathId,
                bookmarks = new List<string>(profile.bookmarks),
                mockHistory = new List<MockSessionRecord>(profile.mockHistory),
                activeMock = profile.activeMock,
                recentActivity = new List<ActivityEvent>(profile.recentActivity),
                reducedMotion = profile.reducedMotion,
                hapticsEnabled = profile.hapticsEnabled,
                questions = new List<QuestionProgress>(profile.questions.Values),
                lessons = new List<LessonProgress>(profile.lessons.Values),
                flashcards = new List<FlashcardProgress>(profile.flashcards.Values)
            };
            return data;
        }

        public static UserProfile FromSerializable(SerializableProfile data)
        {
            var profile = new UserProfile
            {
                schemaVersion = data.schemaVersion,
                onboardingCompleted = data.onboardingCompleted,
                displayName = string.IsNullOrWhiteSpace(data.displayName) ? "Candidate" : data.displayName,
                dailyGoalQuestions = Mathf.Clamp(data.dailyGoalQuestions, 1, 50),
                preferredThinkSeconds = data.preferredThinkSeconds <= 0 ? 120 : data.preferredThinkSeconds,
                preferredMockLength = data.preferredMockLength <= 0 ? 5 : data.preferredMockLength,
                currentStreak = Math.Max(0, data.currentStreak),
                longestStreak = Math.Max(0, data.longestStreak),
                lastStudyDate = data.lastStudyDate ?? "",
                lastActiveScreen = string.IsNullOrWhiteSpace(data.lastActiveScreen) ? "Home" : data.lastActiveScreen,
                continueQuestionId = data.continueQuestionId ?? "",
                continuePathId = data.continuePathId ?? "",
                bookmarks = data.bookmarks ?? new List<string>(),
                mockHistory = data.mockHistory ?? new List<MockSessionRecord>(),
                activeMock = data.activeMock != null && data.activeMock.IsResumable ? data.activeMock : null,
                recentActivity = data.recentActivity ?? new List<ActivityEvent>(),
                reducedMotion = data.reducedMotion,
                hapticsEnabled = data.hapticsEnabled
            };

            foreach (var item in data.questions ?? new List<QuestionProgress>())
            {
                if (!string.IsNullOrEmpty(item.questionId))
                {
                    profile.questions[item.questionId] = item;
                }
            }

            foreach (var item in data.lessons ?? new List<LessonProgress>())
            {
                if (!string.IsNullOrEmpty(item.lessonId))
                {
                    profile.lessons[item.lessonId] = item;
                }
            }

            foreach (var item in data.flashcards ?? new List<FlashcardProgress>())
            {
                if (!string.IsNullOrEmpty(item.cardId))
                {
                    profile.flashcards[item.cardId] = item;
                }
            }

            return profile;
        }

        public static string ToJson(UserProfile profile)
        {
            return JsonUtility.ToJson(ToSerializable(profile), true);
        }

        public static UserProfile FromJson(string json)
        {
            var data = JsonUtility.FromJson<SerializableProfile>(json);
            if (data == null)
            {
                throw new InvalidOperationException("Unable to parse profile JSON.");
            }

            return FromSerializable(data);
        }
    }

    public sealed class ProfileStore
    {
        public const int CurrentSchemaVersion = 1;
        readonly string _filePath;
        readonly string _backupPath;
        readonly string _tempPath;

        public string FilePath => _filePath;

        public ProfileStore(string directory = null)
        {
            var root = string.IsNullOrEmpty(directory)
                ? Path.Combine(Application.persistentDataPath, "UIP")
                : directory;
            Directory.CreateDirectory(root);
            _filePath = Path.Combine(root, "profile.json");
            _backupPath = Path.Combine(root, "profile.bak.json");
            _tempPath = Path.Combine(root, "profile.tmp.json");
        }

        public UserProfile LoadOrCreate()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    return Migrate(ProfileSerializer.FromJson(File.ReadAllText(_filePath)));
                }

                if (File.Exists(_backupPath))
                {
                    var restored = Migrate(ProfileSerializer.FromJson(File.ReadAllText(_backupPath)));
                    Save(restored);
                    return restored;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Profile load failed, creating fresh profile. {ex.Message}");
            }

            var fresh = new UserProfile { schemaVersion = CurrentSchemaVersion };
            Save(fresh);
            return fresh;
        }

        public void Save(UserProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            profile.schemaVersion = CurrentSchemaVersion;
            var json = ProfileSerializer.ToJson(profile);

            File.WriteAllText(_tempPath, json);
            if (File.Exists(_filePath))
            {
                File.Copy(_filePath, _backupPath, true);
            }

            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            File.Move(_tempPath, _filePath);
        }

        public void Reset()
        {
            var fresh = new UserProfile { schemaVersion = CurrentSchemaVersion };
            Save(fresh);
        }

        public string ExportSummary(UserProfile profile)
        {
            return ProfileSerializer.ToJson(profile);
        }

        public static UserProfile Migrate(UserProfile profile)
        {
            if (profile == null)
            {
                return new UserProfile { schemaVersion = CurrentSchemaVersion };
            }

            if (profile.schemaVersion <= 0)
            {
                profile.schemaVersion = 1;
            }

            if (profile.schemaVersion > CurrentSchemaVersion)
            {
                Debug.LogWarning($"Profile schema {profile.schemaVersion} is newer than app support {CurrentSchemaVersion}.");
            }

            profile.schemaVersion = CurrentSchemaVersion;
            profile.bookmarks ??= new List<string>();
            profile.mockHistory ??= new List<MockSessionRecord>();
            profile.recentActivity ??= new List<ActivityEvent>();
            profile.questions ??= new Dictionary<string, QuestionProgress>();
            profile.lessons ??= new Dictionary<string, LessonProgress>();
            profile.flashcards ??= new Dictionary<string, FlashcardProgress>();
            if (string.IsNullOrWhiteSpace(profile.displayName))
            {
                profile.displayName = "Candidate";
            }

            if (profile.dailyGoalQuestions <= 0)
            {
                profile.dailyGoalQuestions = 5;
            }

            if (profile.preferredThinkSeconds <= 0)
            {
                profile.preferredThinkSeconds = 120;
            }

            if (profile.preferredMockLength <= 0)
            {
                profile.preferredMockLength = 5;
            }

            profile.activeMock = SanitizeActiveMock(profile.activeMock);
            return profile;
        }

        static MockSessionState SanitizeActiveMock(MockSessionState state)
        {
            return state != null && state.IsResumable ? state : null;
        }
    }
}
