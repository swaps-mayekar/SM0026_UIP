namespace UIP.Core
{
    public static class ScoreUtil
    {
        public static float RatingToScore(SelfRating rating)
        {
            switch (rating)
            {
                case SelfRating.Excellent:
                    return 1f;
                case SelfRating.Solid:
                    return 0.75f;
                case SelfRating.Partial:
                    return 0.4f;
                default:
                    return 0f;
            }
        }

        public static float ConfidenceToScore(ConfidenceLevel confidence)
        {
            switch (confidence)
            {
                case ConfidenceLevel.High:
                    return 1f;
                case ConfidenceLevel.Medium:
                    return 0.66f;
                case ConfidenceLevel.Low:
                    return 0.33f;
                default:
                    return 0f;
            }
        }

        public static string FormatPercent(float value01)
        {
            return $"{UnityEngine.Mathf.RoundToInt(value01 * 100f)}%";
        }

        public static string DifficultyLabel(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Beginner:
                    return "Beginner";
                case Difficulty.Junior:
                    return "Junior";
                case Difficulty.Mid:
                    return "Mid";
                case Difficulty.Senior:
                    return "Senior";
                case Difficulty.Lead:
                    return "Lead";
                default:
                    return difficulty.ToString();
            }
        }
    }
}
