using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace AE.IntraBaseTeleporter.Buildables
{
    internal partial class BaseTeleporterBuildable
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
            LanguageHandler.SetLanguageLine(StartKey, "START");
            LanguageHandler.SetLanguageLine(SettingsPageKey, "Settings Page");
            LanguageHandler.SetLanguageLine(GoToSettingsPageKey, "Go to settings page.");
            LanguageHandler.SetLanguageLine(GoToHomePageKey, "Go to home page.");
            LanguageHandler.SetLanguageLine(SettingsKey, "Settings");
            LanguageHandler.SetLanguageLine(NotEnoughPowerKey, "Not Enough Power To Teleport");
        }


        private const string OnHoverKey = "BT_OnHover";
        private const string StartKey = "BT_Start";
        private const string SettingsPageKey = "BT_SettingsPage";
        private const string GoToSettingsPageKey = "BT_GoToSettingsPage";
        private const string GoToHomePageKey = "BT_GoToHomePage";
        private const string SettingsKey = "BT_Settings";
        private const string NotEnoughPowerKey = "BT_NotEnoughPower";



        public static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        public static string Cancel()
        {
            return Language.main.Get("Cancel");
        }

        public static string Start()
        {
            return Language.main.Get(StartKey);
        }

        
        public static string Version()
        {
            return Language.main.Get("Version");
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

        public static string Submit()
        {
            return Language.main.Get("Submit");
        }

        public static string NotEnoughPower()
        {
            return Language.main.Get(NotEnoughPowerKey);
        }
    }
}
