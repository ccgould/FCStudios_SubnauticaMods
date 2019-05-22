using SMLHelper.V2.Handlers;

namespace FCSAlterraShipping.Display.Patching
{
    internal static class DisplayLanguagePatching
    {
        public const string StorageKey = "Storage";
        public const string PrevPageKey = "PrevPage";
        public const string NextPageKey = "NextPage";
        public const string OpenStorageKey = "OpenStorage";
        public const string CancelKey = "CancelTransfer";
        public const string ShippingKey = "Shipping";
        public const string TimeLeftKey = "TimeLeft";
        public const string SendPackageKey = "SendPackage";

        internal static void AdditionPatching()
        {
            LanguageHandler.SetLanguageLine(CancelKey, "< Cancel Transfer");
            LanguageHandler.SetLanguageLine(OpenStorageKey, "Open Storage");
            LanguageHandler.SetLanguageLine(NextPageKey, "Next Page");
            LanguageHandler.SetLanguageLine(PrevPageKey, "Previous Page");
            LanguageHandler.SetLanguageLine(StorageKey, "STORAGE");
            LanguageHandler.SetLanguageLine(SendPackageKey, "Send Package");
            LanguageHandler.SetLanguageLine(ShippingKey, "SHIPPING");
            LanguageHandler.SetLanguageLine(TimeLeftKey, "Time Left:");
        }
    }
}
