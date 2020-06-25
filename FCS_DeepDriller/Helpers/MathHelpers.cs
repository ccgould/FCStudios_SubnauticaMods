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
    }
}
