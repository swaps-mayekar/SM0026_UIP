using System;
using UIP.Core;

namespace UIP.Features
{
    public static class StreakService
    {
        public static void ApplyStudy(UserProfile profile, DateTime utcNow)
        {
            var today = DateUtil.TodayDayKey(utcNow);
            if (DateUtil.IsSameUtcDay(profile.lastStudyDate, utcNow))
            {
                return;
            }

            if (DateUtil.IsYesterdayUtcDay(profile.lastStudyDate, utcNow) || string.IsNullOrEmpty(profile.lastStudyDate))
            {
                profile.currentStreak = string.IsNullOrEmpty(profile.lastStudyDate) ? 1 : profile.currentStreak + 1;
            }
            else
            {
                profile.currentStreak = 1;
            }

            profile.lastStudyDate = today;
            if (profile.currentStreak > profile.longestStreak)
            {
                profile.longestStreak = profile.currentStreak;
            }
        }

        public static void RefreshIfBroken(UserProfile profile, DateTime utcNow)
        {
            if (string.IsNullOrEmpty(profile.lastStudyDate))
            {
                profile.currentStreak = 0;
                return;
            }

            if (DateUtil.IsSameUtcDay(profile.lastStudyDate, utcNow) ||
                DateUtil.IsYesterdayUtcDay(profile.lastStudyDate, utcNow))
            {
                return;
            }

            profile.currentStreak = 0;
        }
    }
}
