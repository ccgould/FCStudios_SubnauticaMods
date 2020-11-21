using FCS_AlterraHub.Model;

namespace FCS_ProductionSolutions.DeepDriller.Configuration
{
    internal class DeepDrillerPowerData
    {
        public float SolarPanel { get; set; }
        public PowercellData Battery { get; set; }

        public DeepDrillerPowerData()
        {
            if (Battery == null)
            {
                Battery = new PowercellData();
                Battery.Initialize(0f, QPatch.DeepDrillerMk2Configuration.InternalBatteryCapacity);
            }
        }
    }
}
