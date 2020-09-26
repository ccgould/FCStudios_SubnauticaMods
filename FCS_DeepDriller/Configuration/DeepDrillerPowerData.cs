using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FCS_DeepDriller.Configuration
{
    internal class DeepDrillerPowerData
    {
        public float SolarPanel { get; set; }
        public PowercellData Battery { get; set; } = new PowercellData{Charge = 0f,Capacity=QPatch.Configuration.InternalBatteryCapacity};
    }
}
