using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Helpers
{
    internal static class MathOperations
    {
        internal static float PercentageOf(float number, float percentage)
        {
            return number * percentage / 100f;
        }
    }
}
