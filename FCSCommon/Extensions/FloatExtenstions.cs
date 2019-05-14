using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Extensions
{
    public static class FloatExtenstions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
