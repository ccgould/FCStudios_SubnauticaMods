namespace FCSPowerStorage.Configuration
{

    /// <summary>
    /// Class that stores custom properties from the configuration file
    /// </summary>
    public static class BatteryConfiguration
    {
        /// <summary>
        /// The maximum charge of the battery
        /// </summary>
        public static float Capacity { get; set; } = 2000;

        /// <summary>
        /// The charge speed of this battery
        /// </summary>
        public static float ChargeSpeed { get; set; } = 0.005f;
    }
}
