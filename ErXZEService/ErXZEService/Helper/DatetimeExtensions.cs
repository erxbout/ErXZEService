using System;

namespace ErXZEService.Helper
{
    public static class DatetimeExtensions
    {
        public static bool IsCloseEnough(this DateTime first, DateTime second, int maxSecondsApart)
        {
            var span = first - second;

            if (span < TimeSpan.FromSeconds(maxSecondsApart))
                return Math.Abs(span.Seconds) <= maxSecondsApart;
            else
                return false;
        }

        public static bool IsTimespanElapsed(this DateTime since, TimeSpan cooldown)
        {
            return since.Add(cooldown) < DateTime.Now;
        }
    }
}
