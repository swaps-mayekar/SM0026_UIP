using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UIP.Content;
using UIP.Core;
using UIP.Features;
using UIP.Persistence;

namespace UIP.Tests
{
    public class PlayModeSmokeTests
    {
        [UnityTest]
        public IEnumerator Content_And_Profile_Survive_Frame()
        {
            var content = ContentRepository.LoadFromResources();
            Assert.Greater(content.Questions.Count, 0);

            var dir = System.IO.Path.Combine(Application.temporaryCachePath, "UIP_Play_" + System.Guid.NewGuid().ToString("N"));
            var store = new ProfileStore(dir);
            var profile = store.LoadOrCreate();
            var progress = new ProgressService(content, profile, () => store.Save(profile));
            progress.CompleteOnboarding();
            progress.RecordQuestionAttempt(content.Questions[0].id, SelfRating.Solid, ConfidenceLevel.Medium);
            yield return null;
            var reloaded = store.LoadOrCreate();
            Assert.IsTrue(reloaded.onboardingCompleted);
            Assert.IsTrue(reloaded.questions.ContainsKey(content.Questions[0].id));
            if (System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.Delete(dir, true);
            }
        }

        [UnityTest]
        public IEnumerator Mock_Session_Can_Be_Saved_Across_Frames()
        {
            var content = ContentRepository.LoadFromResources();
            var profile = new UserProfile();
            var progress = new ProgressService(content, profile, () => { });
            var mock = new MockInterviewService(content, progress, 3);
            var session = mock.StartSession(3, 60);
            mock.Tick(session, 1.5f);
            yield return null;
            Assert.Less(session.remainingSeconds, 60f);
            Assert.IsNotNull(profile.activeMock);
            mock.Abandon(session);
            Assert.IsNull(profile.activeMock);
        }
    }
}
