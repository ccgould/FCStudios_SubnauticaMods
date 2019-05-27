using SMLHelper.V2.Handlers;

namespace FCS_AIMarineTurbine.Display.Patching
{
    internal static class DisplayLanguagePatching
    {
        public const string DepthKey = "Depth";
        public const string SpeedKey = "Speed";
        public const string PowerKey = "Power";
        public const string HealthKey = "Health";
        public const string OffKey = "Off";
        public const string OnKey = "On";
        public const string WorkingKey = "Working";
        public const string DamagedKey = "Damaged";
        public const string HealthyLegendKey = "HealthyLegend";
        public const string MidlyDamagedLegendKey = "MidlyDamagedLegend";
        public const string DamagedLegendKey = "DamagedLegend";
        public const string PingKey = "Ping";
        public const string PoweredOffKey = "PoweredOff";
        public const string PingingKey = "Pinging";
        public const string LegendKey = "Legend";
        public const string StatusOverviewKey = "StatusOverview";
        //public const string DescriptionKey = "Description";


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
            LanguageHandler.SetLanguageLine(DamagedKey, "DAMAGED");
            LanguageHandler.SetLanguageLine(DamagedLegendKey, "Damaged\n0% => 49%");
            LanguageHandler.SetLanguageLine(HealthyLegendKey, "Healthy\n51 % => 100%");
            LanguageHandler.SetLanguageLine(MidlyDamagedLegendKey, "Midly Damaged\n20% => 5");
            LanguageHandler.SetLanguageLine(OnKey, "ON");
            LanguageHandler.SetLanguageLine(LegendKey, "Legend");
            LanguageHandler.SetLanguageLine(PoweredOffKey, "Powered Off");
            LanguageHandler.SetLanguageLine(StatusOverviewKey, "Status Overview");
            //LanguageHandler.SetLanguageLine(DescriptionKey, "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.");
        }
    }
}
