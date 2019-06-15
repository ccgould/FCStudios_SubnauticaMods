using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable
    {
        #region Public Properties

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }

        public const string OnHandOverKey = "AIPS_OnHandOver";

        public const string StatusKey = "AIPS_StatusKey";

        #endregion

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            LanguageHandler.SetLanguageLine(PowercellContainterKey, "AI Powercell Socket Slots");
            LanguageHandler.SetLanguageLine(StatusKey, "STATUS");
            LanguageHandler.SetLanguageLine(OnHandOverKey, "Open Powercell Socket Slots Container.");
        }

        private const string PowercellContainterKey = "PowercellContainter";

        public static string PowercellContainterLabel()
        {
            return Language.main.Get(PowercellContainterKey);
        }
    }
}
