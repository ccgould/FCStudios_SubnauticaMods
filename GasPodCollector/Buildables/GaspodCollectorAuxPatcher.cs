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
        }

        private const string OnHoverKey = "GSC_OnHover";
        private const string NotEmptyKey = "GSC_NotEmpty";


        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }

        public static string NotEmpty()
        {
            return Language.main.Get(NotEmptyKey);
        }
    }
}
