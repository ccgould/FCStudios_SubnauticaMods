using System;

namespace FCSPowerStorage.Configuration
{
    [Serializable]
    public class BatteryConfiguration
    {
        /// <summary>
        /// The maximum charge of the battery
        /// </summary>
        public float Capacity { get; set; } = 2000;

        /// <summary>
        /// The charge speed of this battery
        /// </summary>
        public float ChargeSpeed { get; set; } = 0.005f;
    }
}
