using FCSCommon.Controllers;

namespace AMMiniMedBay.Models
{
    internal class AMMiniMedBayPowerManager : PowerManager
    {
        public override float EnergyConsumptionPerSecond { get; set; } = 8.0f;
    }
}
