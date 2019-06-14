using FCSCommon.Utilities;

namespace FCSAIPowerCellSocket.Buildables
{
    internal partial class AIPowerCellSocketBuildable
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
            //LanguageHandler.SetLanguageLine(OverLimitKey, "You can't place more than one unit in a habitat.");
        }

        //private const string OverLimitKey = "OverLimitMessage";

    }
}
