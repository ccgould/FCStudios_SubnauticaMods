using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCS_DeepDriller.Helpers
{
    internal static class MathHelpers
    {
        internal static float GetRemainder(float amount, float total)
        {
            return total - amount;
        }

        public static string ToKiloFormat(this int num)
        {
            if (num >= 100000000)
                return (num / 1000000).ToString("#,0M");

            if (num >= 10000000)
                return (num / 1000000).ToString("0.#") + "M";

            if (num >= 100000)
                return (num / 1000).ToString("#,0K");

            if (num >= 10000)
                return (num / 1000).ToString("0.#") + "K";

            return num.ToString("#,0");
        }
    }
}
