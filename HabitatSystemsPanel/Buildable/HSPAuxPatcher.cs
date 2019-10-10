using AE.HabitatSystemsPanel.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace AE.HabitatSystemsPanel.Buildable
{
    internal partial class HSPBuildable
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
            LanguageHandler.SetLanguageLine(StorageLabelKey, $"{Mod.FriendlyName} Receptacle");
        }

        private const string OnHoverKey = "MFF_OnHover";
        private const string StorageLabelKey = "MFF_StorageLabel";

        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }


        internal static string Version()
        {
            return Language.main.Get("Version");
        }
    }
}
