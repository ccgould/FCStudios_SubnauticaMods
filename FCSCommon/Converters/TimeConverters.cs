using System;

namespace FCSCommon.Converters
{
    internal static class TimeConverters
    {
        internal static string SecondsToHMS(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);

            return $"{t.Hours:D2}h:{t.Minutes:D2}m:{t.Seconds:D2}s";
        }

        public static string SecondsToMS(float seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D2}m:{t.Seconds:D2}s";
        }
    }
}
