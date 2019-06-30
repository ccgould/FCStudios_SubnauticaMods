using SMLHelper.V2.Handlers;

namespace FCSPowerStorage.Buildables
{
    internal partial class FCSPowerStorageBuildable
    {
        #region Prefab Global Properties
        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }
        public static string AutoActivationOverLimitMessageKey = "AutoActivationOverLimitMessage";

        #endregion

        public const string AutoDischargeEnabledMessageKey = "PS_AutoDischargeMessage";
        public const string BatteryKey = "PS_MeterBatteryTxt";
        public const string BatteryMetersKey = "PS_BatterMeters";
        public const string SettingsKey = "PS_Settings";
        public const string ChargeKey = "PS_Charge";
        public const string BootingKey = "PS_Booting";
        public const string DischargeKey = "PS_Discharge";
        public const string SystemSettingsLblKey = "PS_SystemSettingsLBL";
        public const string ColorPickerKey = "PS_ColorPicker";
        public const string PoweredOffKey = "PS_PoweredOff";
        public const string AutoActivateKey = "PS_AutoActivate";
        public const string AutoActivateAtKey = "PS_AutoActivateAt";
        public const string BaseDrainProtectionKey = "PS_BaseDrainProtection";
        public const string BaseDrainLimitKey = "PS_BaseDrainLimit";
        public const string SyncAllKey = "PS_SyncAll";
        public static string AutoActivateDescKey = "AutoActivateDesc";
        public const string AutoActivateOnHoverKey = "AutoActivateOnHover";
        public const string BaseDrainLimitOnHoverKey = "PS_BaseDrainLimitOnHover";
        public const string BaseDrainLimitDescKey = "PS_BaseDrainLimitDesc";
        public const string SubmitKey = "PS_Submit";

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            PatchLanguages();
        }

        private void PatchLanguages()
        {
            LanguageHandler.SetLanguageLine(SubmitKey, "Submit");
            LanguageHandler.SetLanguageLine(AutoActivationOverLimitMessageKey, "Auto Activate cant be higher than Base Drain Protection and was set to");
            LanguageHandler.SetLanguageLine(AutoActivateOnHoverKey, "Change auto activate limit");
            LanguageHandler.SetLanguageLine(AutoActivateDescKey, "Activate discharge mode at the specified base power amount");
            LanguageHandler.SetLanguageLine(BaseDrainLimitOnHoverKey, "Change base drain protection limit");
            LanguageHandler.SetLanguageLine(BaseDrainLimitDescKey, "Stop power storage from changing at the specified base power amount");
            LanguageHandler.SetLanguageLine(BatteryKey, "BATTERY");
            LanguageHandler.SetLanguageLine(PoweredOffKey, "POWERED OFF");
            LanguageHandler.SetLanguageLine(AutoDischargeEnabledMessageKey, "Disable Auto Discharge to use this feature.");
            LanguageHandler.SetLanguageLine(ColorPickerKey, "COLOR PICKER");
            LanguageHandler.SetLanguageLine(SystemSettingsLblKey, "System Settings");
            LanguageHandler.SetLanguageLine(DischargeKey, "DISCHARGE MODE");
            LanguageHandler.SetLanguageLine(ChargeKey, "CHARGE MODE");
            LanguageHandler.SetLanguageLine(BootingKey, "BOOTING");
            LanguageHandler.SetLanguageLine(SettingsKey, "SETTINGS");
            LanguageHandler.SetLanguageLine(BatteryMetersKey, "BATTERY METERS");
            LanguageHandler.SetLanguageLine(AutoActivateKey, "AUTO ACTIVATE");
            LanguageHandler.SetLanguageLine(AutoActivateAtKey, "Auto Activate At");
            LanguageHandler.SetLanguageLine(BaseDrainProtectionKey, "BASE DRAIN PROTECTION");
            LanguageHandler.SetLanguageLine(BaseDrainLimitKey, "Base Drain Limit");
            LanguageHandler.SetLanguageLine(SyncAllKey, "SYNC ALL");
        }
    }
}
