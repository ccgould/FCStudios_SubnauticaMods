using SMLHelper.V2.Handlers;

namespace FCS_AIMarineTurbine.Display.Patching
{
    internal static class DisplayLanguagePatching
    {
        public const string RPMKey = "RPM";
        public const string DepthKey = "Depth";
        public const string SpeedKey = "Speed";
        public const string PowerKey = "Power";
        public const string HealthKey = "Health";
        public const string OffKey = "Off";
        public const string OnKey = "On";
        public const string WorkingKey = "Working";
        public const string FailedKey = "Failed";
        public const string HealthyLegendKey = "HealthyLegend";
        public const string MidlyDamagedLegendKey = "MidlyDamagedLegend";
        public const string FailedLegendKey = "WornLegend";
        public const string PingKey = "Ping";
        public const string PoweredOffKey = "PoweredOff";
        public const string PingingKey = "Pinging";
        public const string LegendKey = "Legend";
        public const string StatusOverviewKey = "StatusOverview";
        private const string NotOperationalKey = "MT_NotOperational";


        internal static void AdditionPatching()
        {
            LanguageHandler.SetLanguageLine(DepthKey, "Depth");
            LanguageHandler.SetLanguageLine(SpeedKey, "Speed");
            LanguageHandler.SetLanguageLine(PowerKey, "Power");
            LanguageHandler.SetLanguageLine(HealthKey, "Health");
            LanguageHandler.SetLanguageLine(OffKey, "OFF");
            LanguageHandler.SetLanguageLine(PingKey, "PING");
            LanguageHandler.SetLanguageLine(PingingKey, "PINGING");
            LanguageHandler.SetLanguageLine(OnKey, "ON");
            LanguageHandler.SetLanguageLine(WorkingKey, "Working");
            LanguageHandler.SetLanguageLine(FailedKey, "Failed");
            LanguageHandler.SetLanguageLine(FailedLegendKey, "Failed\n0%");
            LanguageHandler.SetLanguageLine(HealthyLegendKey, "Healthy\n60 % => 100%");
            LanguageHandler.SetLanguageLine(MidlyDamagedLegendKey, "Worn\n1% => 59%");
            LanguageHandler.SetLanguageLine(OnKey, "ON");
            LanguageHandler.SetLanguageLine(RPMKey, "rpm");
            LanguageHandler.SetLanguageLine(LegendKey, "Legend");
            LanguageHandler.SetLanguageLine(PoweredOffKey, "Powered Off");
            LanguageHandler.SetLanguageLine(StatusOverviewKey, "Status Overview");
            LanguageHandler.SetLanguageLine(NotOperationalKey, "Must be on a platform to operate and underwater level.");
        }

        public static string NotOperational()
        {
            return Language.main.Get(NotOperationalKey);
        }
    }
}
