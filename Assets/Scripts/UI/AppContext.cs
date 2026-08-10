using UIP.Content;
using UIP.Core;
using UIP.Features;
using UIP.Persistence;

namespace UIP.UI
{
    public sealed class AppContext
    {
        public ContentRepository Content { get; }
        public ProfileStore Store { get; }
        public UserProfile Profile { get; private set; }
        public ProgressService Progress { get; private set; }
        public MockInterviewService Mock { get; private set; }
        public NavigationService Navigation { get; } = new NavigationService();

        public AppContext(ContentRepository content, ProfileStore store)
        {
            Content = content;
            Store = store;
            Profile = store.LoadOrCreate();
            StreakService.RefreshIfBroken(Profile, System.DateTime.UtcNow);
            Progress = new ProgressService(Content, Profile, Persist);
            Mock = new MockInterviewService(Content, Progress);
        }

        public void Persist()
        {
            Store.Save(Profile);
        }

        public void ReloadProfile()
        {
            Profile = Store.LoadOrCreate();
            Progress = new ProgressService(Content, Profile, Persist);
            Mock = new MockInterviewService(Content, Progress);
        }

        public void ResetProgress()
        {
            Progress.ResetAllProgress();
        }
    }
}
