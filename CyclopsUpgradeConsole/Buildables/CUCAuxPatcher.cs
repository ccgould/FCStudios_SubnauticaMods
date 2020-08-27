using CyclopsUpgradeConsole.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace CyclopsUpgradeConsole.Buildables
{
    internal partial class CUCBuildable
    {
        private const string NotEmptyKey = "CUC_NotEmpty";
        private const string HoverTextKey = "CUC_HoverText";
        internal string BuildableName { get; set; }

        internal TechType TechTypeID { get; set; }

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = FriendlyName;
            TechTypeID = TechType;
            LanguageHandler.SetLanguageLine(NotEmptyKey, "Please remove all upgrades.");
            LanguageHandler.SetLanguageLine(HoverTextKey, "Use {0}.");
        }
        
        public static string NotEmpty()
        {
            return Language.main.Get(NotEmptyKey);
        }

        public static string HoverText()
        {
            return string.Format(Language.main.Get(HoverTextKey), Mod.ModFriendlyName);
        }
    }
}
