using System;
using UIP.Core;

namespace UIP.Features
{
    public static class SpacedRepetition
    {
        public static bool IsDue(FlashcardProgress progress, DateTime utcNow)
        {
            if (progress == null || string.IsNullOrEmpty(progress.dueIso))
            {
                return true;
            }

            return DateUtil.ParseIso(progress.dueIso) <= utcNow;
        }

        public static void Apply(FlashcardProgress progress, FlashcardGrade grade, DateTime utcNow)
        {
            progress.lastGrade = grade;
            progress.lastReviewedIso = utcNow.ToString(DateUtil.IsoFormat);

            if (grade == FlashcardGrade.Again)
            {
                progress.repetitions = 0;
                progress.intervalDays = 0;
                progress.ease = Math.Max(1.3f, progress.ease - 0.2f);
                progress.dueIso = utcNow.AddMinutes(10).ToString(DateUtil.IsoFormat);
                return;
            }

            if (progress.repetitions == 0)
            {
                progress.intervalDays = grade == FlashcardGrade.Hard ? 1 : 1;
            }
            else if (progress.repetitions == 1)
            {
                progress.intervalDays = grade == FlashcardGrade.Hard ? 2 : 3;
            }
            else
            {
                var multiplier = grade == FlashcardGrade.Hard ? Math.Max(1.2f, progress.ease - 0.15f) : progress.ease;
                progress.intervalDays = Math.Max(1, (int)Math.Round(progress.intervalDays * multiplier));
            }

            if (grade == FlashcardGrade.Good)
            {
                progress.ease = Math.Min(3.0f, progress.ease + 0.05f);
            }
            else if (grade == FlashcardGrade.Hard)
            {
                progress.ease = Math.Max(1.3f, progress.ease - 0.05f);
            }

            progress.repetitions++;
            progress.dueIso = utcNow.AddDays(progress.intervalDays).ToString(DateUtil.IsoFormat);
        }
    }
}
