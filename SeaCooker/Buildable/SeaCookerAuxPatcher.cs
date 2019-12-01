using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace AE.SeaCooker.Buildable
{
    internal partial class SeaCookerBuildable
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

            LanguageHandler.SetLanguageLine(OnHoverKey, "Click to add food to cook.");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "SeaCooker Food Receptacle");
            LanguageHandler.SetLanguageLine(GasContainerLabelKey, "SeaCooker Gas Receptacle");
            LanguageHandler.SetLanguageLine(ExportStorageLabelKey, "SeaCooker Food Export Receptacle");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Food items allowed only.");
            LanguageHandler.SetLanguageLine(NoFoodKey, "No food items to cook.");
            LanguageHandler.SetLanguageLine(NoFuelKey, "No fuel available.");
            LanguageHandler.SetLanguageLine(StartKey, "START");
            LanguageHandler.SetLanguageLine(TankPercentageKey, "Tank Percentage");
            LanguageHandler.SetLanguageLine(UnitNotEmptyKey, "One or both of the container are not empty.");
            LanguageHandler.SetLanguageLine(NoEnoughRoomKey, "There isn't enough room in the export container.");
            LanguageHandler.SetLanguageLine(CookingCantOpenKey, "Cannot open container while cooking.");
            LanguageHandler.SetLanguageLine(SettingsPageKey, "Settings Page");
            LanguageHandler.SetLanguageLine(GoToSettingsPageKey, "Go to settings page.");
            LanguageHandler.SetLanguageLine(GoToHomePageKey, "Go to home page.");
            LanguageHandler.SetLanguageLine(SettingsKey, "Settings");
        }


        private const string OnHoverKey = "SC_OnHover";
        private const string GasContainerLabelKey = "SC_GasContainerLabel";
        private const string StorageLabelKey = "SC_StorageLabel";
        private const string ItemNotAllowedKey = "SC_FoodItemNotAllowed";
        private const string ExportStorageLabelKey = "SC_ExportStorageLabel";
        private const string NoFoodKey = "SC_NoFood";
        private const string NoFuelKey = "SC_NoFuel";
        private const string StartKey = "SC_Start";
        private const string TankPercentageKey = "SC_TankPercentage";
        private const string UnitNotEmptyKey = "SC_UnitNotEmpty";
        private const string NoEnoughRoomKey = "SC_NoEnoughRoom";
        private const string CookingCantOpenKey = "SC_CookingCantOpen";
        private const string SettingsPageKey = "SC_SettingsPage";
        private const string GoToSettingsPageKey = "SC_GoToSettingsPage";
        private const string GoToHomePageKey = "SC_GoToHomePage";
        private const string SettingsKey = "SC_Settings";



        public static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        internal static string GasContainerLabel()
        {
            return Language.main.Get(GasContainerLabelKey);
        }

        internal static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string ItemNotAllowed()
        {
            return Language.main.Get(ItemNotAllowedKey);
        }

        internal static string ExportStorageLabel()
        {
            return Language.main.Get(ExportStorageLabelKey);
        }

        public static string NoFoodToCook()
        {
            return Language.main.Get(NoFoodKey);
        }

        public static string NoFuel()
        {
            return Language.main.Get(NoFuelKey);
        }

        public static string Cancel()
        {
            return Language.main.Get("Cancel");
        }

        public static string Start()
        {
            return Language.main.Get(StartKey);
        }

        public static string TankPercentage()
        {
            return Language.main.Get(TankPercentageKey);
        }

        public static string Version()
        {
            return Language.main.Get("Version");
        }

        public static string UnityNotEmpty()
        {
            return Language.main.Get(UnitNotEmptyKey);
        }

        public static string NoEnoughRoom()
        {
            return Language.main.Get(NoEnoughRoomKey);
        }

        public static string CookingCantOpen()
        {
            return Language.main.Get(CookingCantOpenKey);
        }

        public static string SettingsPage()
        {
            return Language.main.Get(SettingsPageKey);
        }

        public static string Settings()
        {
            return Language.main.Get(SettingsPageKey);
        }

        public static string GoToSettingsPage()
        {
            return Language.main.Get(GoToSettingsPageKey);
        }

        public static string GoToHomePage()
        {
            return Language.main.Get(GoToHomePageKey);
        }

        public static string NoPowerAvailable()
        {
            return Language.main.Get("NoPower");
        }
    }
}
