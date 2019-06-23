using SMLHelper.V2.Handlers;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable
    {
        private const string StorageLabelKey = "SeaBreezeStorage";

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }
        public const string BatteryKey = "PS_MeterBatteryTxt";
        public const string BatteryMetersKey = "PS_BatterMeters";

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(StorageLabelKey, "Freon Refill In");
            LanguageHandler.SetLanguageLine(BatteryKey, "Battery");
            LanguageHandler.SetLanguageLine(BatteryMetersKey, "BATTERY METERS");
        }
    }
}
