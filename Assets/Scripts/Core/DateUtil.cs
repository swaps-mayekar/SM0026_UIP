using System;
using System.Globalization;

namespace UIP.Core
{
    public static class DateUtil
    {
        public const string IsoFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        public const string DayFormat = "yyyy-MM-dd";

        public static string NowIso()
        {
            return DateTime.UtcNow.ToString(IsoFormat, CultureInfo.InvariantCulture);
        }

        public static string TodayDayKey(DateTime? utcNow = null)
        {
            var now = utcNow ?? DateTime.UtcNow;
            return now.ToString(DayFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseIso(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.MinValue;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }

        public static bool IsSameUtcDay(string dayKey, DateTime utcNow)
        {
            return string.Equals(dayKey, TodayDayKey(utcNow), StringComparison.Ordinal);
        }

        public static bool IsYesterdayUtcDay(string dayKey, DateTime utcNow)
        {
            var yesterday = utcNow.AddDays(-1).ToString(DayFormat, CultureInfo.InvariantCulture);
            return string.Equals(dayKey, yesterday, StringComparison.Ordinal);
        }
    }
}
