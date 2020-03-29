using FCSCommon.Utilities;
using GasPodCollector.Configuration;
using SMLHelper.V2.Handlers;

namespace GasPodCollector.Buildables
{
    internal partial class GaspodCollectorBuildable
    {
        #region Public Properties

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }

        #endregion

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnHoverKey, "On Hover");
            LanguageHandler.SetLanguageLine(NotEmptyKey, $"{Mod.FriendlyName} storage is not empty!");
            LanguageHandler.SetLanguageLine(EquipmentContainerLabelKey, $"Gaspod Collector Powercell Receptacle");
            LanguageHandler.SetLanguageLine(HasBatteriesKey, $"Please remove all batteries in the {Mod.FriendlyName}.");
            LanguageHandler.SetLanguageLine(AmountOfPodsMessageKey, $"AMOUNT OF PODS");
            LanguageHandler.SetLanguageLine(InstructionsKey, $"Click the left button to take one gaspod or click the right to fill your inventory.");
            LanguageHandler.SetLanguageLine(HasBatteriesKey, $"Please remove all batteries in the {Mod.FriendlyName}.");
            LanguageHandler.SetLanguageLine(DumpPullKey, $"Bulk Pull");
            LanguageHandler.SetLanguageLine(DumpMessageKey, $"Fill Players Inventory");
            LanguageHandler.SetLanguageLine(TakeGaspodKey, $"Take gaspod.");
            LanguageHandler.SetLanguageLine(ColorPickerKey, $"Color Picker");
            LanguageHandler.SetLanguageLine(GoHomeKey, "Home");
            LanguageHandler.SetLanguageLine(BatteryReceptacleKey, $"Battery Receptacle");
            LanguageHandler.SetLanguageLine(RemoveBeaconKey, "You need to remove beacon first.");
        }

        private const string OnHoverKey = "GSC_OnHover";
        private const string NotEmptyKey = "GSC_NotEmpty";
        private const string EquipmentContainerLabelKey = "GSC_EquipmentContainerLabel";
        private const string HasBatteriesKey = "GSC_HasBatteries";
        private const string AmountOfPodsMessageKey = "GSC_AmountOfPodsMessage";
        private const string InstructionsKey = "GSC_Instructions";
        private const string DumpMessageKey = "GSC_DumpMessage";
        private const string DumpPullKey = "GSC_DumpPull";
        private const string TakeGaspodKey = "GSC_TakeGaspod";
        private const string BatteryReceptacleKey = "GSC_BatteryReceptacle";
        private const string GoHomeKey = "GSC_GoHome";
        private const string ColorPickerKey = "GSC_ColorPicker";
        private const string RemoveBeaconKey = "GSC_RemoveBeacon";


        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        internal static string NotEmpty()
        {
            return Language.main.Get(NotEmptyKey);
        }

        internal static string EquipmentContainerLabel()
        {
            return Language.main.Get(EquipmentContainerLabelKey);
        }

        internal static string HasBatteries()
        {
            return Language.main.Get(HasBatteriesKey);
        }

        internal static string InstructionsMessage()
        {
            return Language.main.Get(InstructionsKey);
        }

        internal static string Battery()
        {
            return Language.main.Get("Battery");
        }

        internal static string AmountOfPodsMessage()
        {
            return Language.main.Get(AmountOfPodsMessageKey);
        }

        internal static string DumpMessage()
        {
            return Language.main.Get(DumpMessageKey);
        }

        internal static string DumpPull()
        {
            return Language.main.Get(DumpPullKey);
        }

        internal static string TakeGaspod()
        {
            return Language.main.Get(TakeGaspodKey);
        }

        internal static string ColorPicker()
        {
            return Language.main.Get(ColorPickerKey);
        }

        internal static string BatteryReceptacle()
        {
            return Language.main.Get(BatteryReceptacleKey);
        }

        internal static string GoHome()
        {
            return Language.main.Get(GoHomeKey);
        }

        public static string RemoveBeacon()
        {
            return Language.main.Get(RemoveBeaconKey);
        }
    }
}
