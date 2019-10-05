using SMLHelper.V2.Handlers;

namespace ARS_SeaBreezeFCS32.Buildables
{
    internal partial class ARSSeaBreezeFCS32Buildable
    {
        private const string StorageLabelKey = "SeaBreezeStorage";
        private const string FcsWorkBenchErrorMessageKey = "FCSWorkBenechErrorMessage";
        private const string CannotRefillMessageKey = "CannotRefillMessage";
        private const string SeaBreezeFullKey = "SeaBreezeFull";
        private const string ItemNotAllowedKey = "ItemNotAllowed";
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
            LanguageHandler.SetLanguageLine(SeaBreezeFullKey, "SeaBreeze is full and cannot add anymore items.");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Food items allowed only.");
        }

        public static string SeaBreezeFull()
        {
            return Language.main.Get(SeaBreezeFullKey);
        }

        public static string ItemNotAllowed()
        {
            return Language.main.Get(ItemNotAllowedKey);
        }

        public static string CannotRefillMessage()
        {
            return Language.main.Get(CannotRefillMessageKey);
        }

        public static string FcsWorkBenchErrorMessage()
        {
            return Language.main.Get(FcsWorkBenchErrorMessageKey);
        }
    }
}
