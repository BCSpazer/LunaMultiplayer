﻿using System;

namespace LunaCommon.Time
{
    /// <summary>
    /// TimeSpan class have a very low max value and result in overflows when there are time differences greater than 10,675,199 days.
    /// So we use this helper simple class
    /// </summary>
    public static class TimeUtil
    {
        public static double SecondsToMilliseconds(double seconds)
        {
            return seconds * 1000;
        }

        public static double MillisecondsToSeconds(double milliseconds)
        {
            return milliseconds / 1000;
        }

        public static double SecondsToTicks(double seconds)
        {
            return seconds * TimeSpan.TicksPerSecond;
        }

        public static double TicksToSeconds(double ticks)
        {
            return ticks / TimeSpan.TicksPerSecond;
        }
    }
}
