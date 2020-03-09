using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace GasPodCollector.Buildables
{
    internal partial class GaspodCollectorCraftable
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
        }

        private const string OnHoverKey = "GSC_OnHover";


        internal static string OnHover()
        {
            return Language.main.Get(OnHoverKey);
        }
    }
}
