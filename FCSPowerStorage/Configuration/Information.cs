namespace FCSPowerStorage.Configuration
{
    public static class Information
    {
        public static string ModName => "FCSTerminal";

        public static string PowerStorageDef => "This is a wall mounted battery storage for base backup power";

        public static BatteryConfiguration BatteryConfiguration;
        public static string ASSETSFOLDER { get; set; }
    }
}
