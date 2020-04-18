using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable
    {
        #region Public Properties

        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }

        private const string OnHandOverKey = "AIPS_OnHandOver";
        private const string StatusKey = "AIPS_StatusKey";
        private const string OnlyPowercellsAllowedKey = "AIPS_OnlyPowercellsAllowed";
        private const string RemoveAllPowercellsKey = "AIPS_RemoveAllPowercells";
        #endregion

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            LanguageHandler.SetLanguageLine(PowercellContainterKey, "AI Powercell Socket Slots");
            LanguageHandler.SetLanguageLine(StatusKey, "STATUS");
            LanguageHandler.SetLanguageLine(OnHandOverKey, "Open Powercell Socket Slots Container.");
            LanguageHandler.SetLanguageLine(OnlyPowercellsAllowedKey, "Only powercells are allowed.");
            LanguageHandler.SetLanguageLine(RemoveAllPowercellsKey, "Please remove all powercells");
        }

        private const string PowercellContainterKey = "PowercellContainter";

        internal static string PowercellContainterLabel()
        {
            return Language.main.Get(PowercellContainterKey);
        }

        internal static string OnlyPowercellsAllowed()
        {
            return Language.main.Get(OnlyPowercellsAllowedKey);
        }

        public static string RemoveAllPowercells()
        {
            return Language.main.Get(RemoveAllPowercellsKey);
        }

        public static string OnHover()
        {
            return Language.main.Get(OnHandOverKey);
        }

        public static string Status()
        {
            return Language.main.Get(StatusKey);
        }
    }
}
