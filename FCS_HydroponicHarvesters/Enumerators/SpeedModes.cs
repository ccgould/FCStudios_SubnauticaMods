using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCS_HydroponicHarvesters.Enumerators
{
    internal enum SpeedModes : int
    {
        Off = 0,
        Max = 1 * 80,
        High = 3 * 90,
        Low = 6 * 100,
        Min = 10 * 110,
    }
}
