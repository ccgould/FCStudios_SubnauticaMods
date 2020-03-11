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
            LanguageHandler.SetLanguageLine(EquipmentContainerLabelKey, $"Gaspod Collector Powercell Receptical");
            LanguageHandler.SetLanguageLine(HasBatteriesKey, $"Please remove all batteries in the {Mod.FriendlyName}.");
            LanguageHandler.SetLanguageLine(AmountOfPodsMessageKey, $"AMOUNT OF PODS");
            LanguageHandler.SetLanguageLine(InstructionsKey, $"Click the left button to take one gaspod or click the right to fill your inventory.");
            LanguageHandler.SetLanguageLine(HasBatteriesKey, $"Please remove all batteries in the {Mod.FriendlyName}.");
        }

        private const string OnHoverKey = "GSC_OnHover";
        private const string NotEmptyKey = "GSC_NotEmpty";
        private const string EquipmentContainerLabelKey = "GSC_EquipmentContainerLabel";
        private const string HasBatteriesKey = "GSC_HasBatteries";
        private const string AmountOfPodsMessageKey = "GSC_AmountOfPodsMessage";
        private const string InstructionsKey = "GSC_InstructionsKey";


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
    }
}
