using SMLHelper.V2.Handlers;

namespace FCS_AIJetStreamT242.Display.Patching
{
    internal static class DisplayLanguagePatching
    {
        public const string DepthKey = "Depth";
        public const string SpeedKey = "Speed";
        public const string PowerKey = "Power";
        public const string HealthKey = "Health";
        //public const string DescriptionKey = "Description";


        internal static void AdditionPatching()
        {
            LanguageHandler.SetLanguageLine(DepthKey, "Depth");
            LanguageHandler.SetLanguageLine(SpeedKey, "Speed");
            LanguageHandler.SetLanguageLine(PowerKey, "Power");
            LanguageHandler.SetLanguageLine(HealthKey, "Health");
            //LanguageHandler.SetLanguageLine(DescriptionKey, "The Jet Stream T242 provides power by using the water current. The faster the turbine spins the more power output.");
        }
    }
}
