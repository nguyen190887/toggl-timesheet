using System;

namespace TogglTimesheet.Utils
{
    public static class TimeUtils
    {
        /// <summary>
        /// Rounds a duration value to the nearest specified interval
        /// </summary>
        /// <param name="duration">The duration value to round</param>
        /// <param name="interval">The interval to round to (e.g. 0.25 for quarter hours)</param>
        /// <returns>The duration rounded to the nearest specified interval</returns>
        public static double RoundTo(double duration, double interval)
        {
            return Math.Round(duration / interval) * interval;
        }

        /// <summary>
        /// Rounds a duration value to the nearest quarter hour (0.25)
        /// </summary>
        /// <param name="duration">The duration value to round</param>
        /// <returns>The duration rounded to the nearest 0.25</returns>
        public static double RoundToQuarterHour(double duration)
        {
            return RoundTo(duration, 0.25);
        }
    }
}
