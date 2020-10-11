using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Abstract;

namespace DataStorageSolutions.Mono
{
    internal class DSSAutoCraftPowerManager : FCSPowerManager
    {
        public override float GetPowerUsagePerSecond()
        {
            return 0.01f;
        }
    }
}
