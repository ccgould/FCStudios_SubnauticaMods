using SMLHelper.V2.Handlers;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable
    {
        #region Prefab Global Properties
        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }
        #endregion

        public const string BatteryKey = "PS_MeterBatteryTxt";
        public const string BatteryMetersKey = "PS_BatterMeters";
        public const string SettingsKey = "PS_Settings";
        public const string ChargeKey = "PS_Charge";
        public const string BootingKey = "PS_Booting";
        public const string DischargeKey = "PS_Discharge";
        public const string UnitModeKey = "PS_UnitMode";
        public const string ColorPickerKey = "PS_ColorPicker";
        public const string StoragePercentageKey = "PS_StoragePercentage";
        public const string PoweredOffKey = "PS_PoweredOff";

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            PatchLangauges();
        }

        private void PatchLangauges()
        {
            LanguageHandler.SetLanguageLine(BatteryKey, "BATTERY");
            LanguageHandler.SetLanguageLine(PoweredOffKey, "POWERED OFF");
            LanguageHandler.SetLanguageLine(StoragePercentageKey, "Storage Percentage");
            LanguageHandler.SetLanguageLine(ColorPickerKey, "COLOR PICKER");
            LanguageHandler.SetLanguageLine(UnitModeKey, "Unit Mode");
            LanguageHandler.SetLanguageLine(DischargeKey, "DISCHARGE");
            LanguageHandler.SetLanguageLine(ChargeKey, "CHARGE");
            LanguageHandler.SetLanguageLine(BootingKey, "BOOTING");
            LanguageHandler.SetLanguageLine(SettingsKey, "SETTINGS");
            LanguageHandler.SetLanguageLine(BatteryMetersKey, "BATTERY METERS");
        }
    }
}
