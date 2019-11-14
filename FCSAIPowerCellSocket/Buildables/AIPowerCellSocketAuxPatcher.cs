using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable
    {
        #region Public Properties

        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }

        internal const string OnHandOverKey = "AIPS_OnHandOver";

        internal const string StatusKey = "AIPS_StatusKey";

        private const string OnlyPowercellsAllowedKey = "AIPS_OnlyPowercellsAllowed";
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
    }
}
