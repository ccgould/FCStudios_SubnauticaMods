using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Helpers
{
    internal static class StringHelpers
    {
        public static string TruncateWEllipsis(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }
    }
}
