using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_AIMarineTurbine.Buildable
{
    internal partial class AIJetStreamT242Buildable
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
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Alterra Shipping Receptical");
            LanguageHandler.SetLanguageLine(OverLimitKey, "You can't place more than one unit in a habitat.");
        }

        private const string StorageLabelKey = "ShippingStorage";
        private const string OverLimitKey = "OverLimitMessage";

        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }
    }
}
