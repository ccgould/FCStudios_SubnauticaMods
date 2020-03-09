

using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCSAlterraShipping.Buildable
{
    internal partial class AlterraShippingBuildable
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
            LanguageHandler.SetLanguageLine(TargetIsShippingKey, "{0} already has a shipping in progress.");
            LanguageHandler.SetLanguageLine(TargetIsFullKey, "{0} already has a shipping in progress.");
        }

        private const string StorageLabelKey = "ATS_ShippingStorage";
        private const string OverLimitKey = "ATS_OverLimitMessage";
        private const string TargetIsShippingKey = "ATS_TargetIsShipping";
        private const string TargetIsFullKey = "ATS_TargetIsFulL";

        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        public static string TargetIsShipping()
        {
            return Language.main.Get(TargetIsShippingKey);
        }

        public static string TargetIsFull()
        {
            return Language.main.Get(TargetIsFullKey);

        }
    }
}
