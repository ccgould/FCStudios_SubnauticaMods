using SMLHelper.V2.Handlers;

namespace ARS_SeaBreezeFCS32.Buildables
{
    internal partial class ARSSeaBreezeFCS32Buildable
    {
        private const string StorageLabelKey = "SeaBreezeStorage";
        internal const string FcsWorkBenchErrorMessageKey = "FCSWorkBenechErrorMessage";
        internal const string CannotRefillMessageKey = "CannotRefillMessage";
        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }


        private const string OnHoverUnpoweredKey = "SeaBreezeNoPower";
        public static string OnHoverTextNoPower()
        {
            return Language.main.Get(OnHoverUnpoweredKey);
        }

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }
        public const string TimeLeftMessageKey = "TimeLeftMessage";

        public static string AddedFreonMessageKey = "AddedFreonMessage";

        internal const string OnSeabreezeHoverkey = "OnSeabreezeHover";
        internal const string OnFreonBTNHoverKey = "OnFreonButtonHover";

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            //LanguageHandler.SetLanguageLine(TimeLeftMessageKey, "Freon Refill In");
            //LanguageHandler.SetLanguageLine(OnFreonBTNHoverKey, "Add Freon");
            //LanguageHandler.SetLanguageLine(AddedFreonMessageKey, "Added Freon to SeaBreeze");
            //LanguageHandler.SetLanguageLine(CannotRefillMessageKey, "Cannot refill freon hasn't ran out.");
            LanguageHandler.SetLanguageLine(OnSeabreezeHoverkey, "Add to SeaBreeze");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "SeaBreeze Receptacle");
            LanguageHandler.SetLanguageLine(OnHoverUnpoweredKey, "Insufficient power");
            LanguageHandler.SetLanguageLine(FcsWorkBenchErrorMessageKey, "FCS Tech Workbench Mod not found please install.");
        }
    }
}
