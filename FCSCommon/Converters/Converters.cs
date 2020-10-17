using System;
using System.Collections.Generic;
using System.Text;

namespace FCSCommon.Converters
{
    internal class Converters
    {
        internal static string FloatToMoney(string moneyUnit, float amount)
        {
            return moneyUnit + amount.ToString("N");
        }
    }
}
