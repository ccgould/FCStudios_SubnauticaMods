using System;
using System.Collections.Generic;
using System.Text;
using FCSCommon.Utilities;

namespace FCSCommon.Helpers
{
    internal static class StringHelpers
    {
        public static string TruncateWEllipsis(this string value, int maxChars)
        {
            if(value == null)
            {
                QuickLogger.Error($"[TruncateWEllipisis] : String is null");
                return string.Empty;
            }
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
    }
}
