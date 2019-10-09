using AE.MiniFountainFilter.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace AE.MiniFountainFilter.Buildable
{
    internal partial class MiniFountainFilterBuildable
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
            LanguageHandler.SetLanguageLine(UnitNotEmptyKey, "One or both of the container are not empty.");
            LanguageHandler.SetLanguageLine(StorageLabelKey, $"{Mod.FriendlyName} Receptacle");
            LanguageHandler.SetLanguageLine(BottlesKey, $"Bottle(s)");
        }

        private const string OnHoverKey = "MFF_OnHover";
        private const string UnitNotEmptyKey = "MFF_UnitNotEmpty";
        private const string StorageLabelKey = "MFF_StorageLabel";
        private const string TankPercentageKey = "MFF_TankPercentage";
        private const string BottlesKey = "MFF_BottleKey";

        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        internal static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string UnitNotEmpty()
        {
            return Language.main.Get(UnitNotEmptyKey);
        }

        internal static string TankPercentage()
        {
            return Language.main.Get(TankPercentageKey);
        }

        internal static string Version()
        {
            return Language.main.Get("Version");
        }

        internal static string Bottles()
        {
            return Language.main.Get(BottlesKey);
        }
    }
}
