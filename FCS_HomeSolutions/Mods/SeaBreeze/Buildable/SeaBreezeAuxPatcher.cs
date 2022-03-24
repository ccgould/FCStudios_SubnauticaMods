using SMLHelper.V2.Handlers;

namespace FCS_HomeSolutions.Mods.SeaBreeze.Buildable
{
    internal static class SeaBreezeAuxPatcher
    {
        private const string StorageLabelKey = "SeaBreezeStorage";
        private const string FcsWorkBenchErrorMessageKey = "FCSWorkBenechErrorMessage";
        private const string SeaBreezeFullKey = "SeaBreezeFull";
        private const string ItemNotAllowedKey = "ItemNotAllowed";
        private const string OnSeabreezeHoverkey = "OnSeabreezeHover";
        private const string ItemKey = "SB_Items";
        private const string EditUnitNameKey = "SB_EditName";
        private const string GoHomeKey = "SB_GoHome";
        private const string ColorPickerKey = "SB_ColorPicker";
        private const string PowerBTNMessageKey = "SB_PowerBTNMessage";
        private const string DumpMessageKey = "SB_DumpMessage";
        private const string DumpButtonKey = "SB_DumpButton";
        private const string FoodCButtonKey = "SB_FoodCButton";
        private const string WaterCButtonKey = "SB_WaterCButton";
        private const string TrashButtonKey = "SB_TrashButton";
        private const string TrashMessageKey = "SB_TrashMessage";
        private const string NotEmptyKey = "SB_NotEmpty";
        private const string OnHoverUnpoweredKey = "SeaBreezeNoPower";
        private const string TakeItemFormatKey = "SB_TakeItem";
        private const string NoPowerKey = "SB_NoPower";
        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static void AdditionalPatching()
        {
            LanguageHandler.SetLanguageLine(OnSeabreezeHoverkey, "Add to SeaBreeze");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "SeaBreeze Receptacle");
            LanguageHandler.SetLanguageLine(OnHoverUnpoweredKey, "Insufficient power");
            LanguageHandler.SetLanguageLine(FcsWorkBenchErrorMessageKey, "FCS Tech Workbench Mod not found please install.");
            LanguageHandler.SetLanguageLine(SeaBreezeFullKey, "SeaBreeze is full and cannot add anymore items.");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Food items allowed only.");
            LanguageHandler.SetLanguageLine(ItemKey, "Item/s");
            LanguageHandler.SetLanguageLine(EditUnitNameKey, $"Edit {SeaBreezeBuildable.SeaBreezeFriendly} Name");
            LanguageHandler.SetLanguageLine(ColorPickerKey, $"Color Picker");
            LanguageHandler.SetLanguageLine(GoHomeKey, "Home");
            LanguageHandler.SetLanguageLine(PowerBTNMessageKey, "Power Button");
            LanguageHandler.SetLanguageLine(DumpButtonKey, "Open Seabreeze");
            LanguageHandler.SetLanguageLine(FoodCButtonKey, "Food Items");
            LanguageHandler.SetLanguageLine(WaterCButtonKey, "Water Items");
            LanguageHandler.SetLanguageLine(TrashButtonKey, "Trash");
            LanguageHandler.SetLanguageLine(TrashMessageKey, "All rotten food will be moved here");
            LanguageHandler.SetLanguageLine(NotEmptyKey, "SeaBreeze is not empty cannot destroy!.");
            LanguageHandler.SetLanguageLine(NoPowerKey, "POWERED OFF");
            LanguageHandler.SetLanguageLine(DumpMessageKey, "Add items to the seabreeze");
            LanguageHandler.SetLanguageLine(TakeItemFormatKey, "Take {0}");
        }
        
        public static string SeaBreezeFull()
        {
            return Language.main.Get(SeaBreezeFullKey);
        }

        public static string ItemNotAllowed()
        {
            return Language.main.Get(ItemNotAllowedKey);
        }

        public static string Items()
        {
            return Language.main.Get(ItemKey);
        }

        public static string Submit()
        {
            return Language.main.Get("Submit");
        }

        public static string RenameButton()
        {
            return Language.main.Get(EditUnitNameKey);
        }

        public static string ColorPicker()
        {
            return Language.main.Get(ColorPickerKey);
        }


        public static string PowerBTNMessage()
        {
            return Language.main.Get(PowerBTNMessageKey);
        }

        public static string DumpButton()
        {
            return Language.main.Get(DumpButtonKey);
        }
        
        public static string DumpMessage()
        {
            return Language.main.Get(DumpMessageKey);
        }

        public static string FoodCButton()
        {
            return Language.main.Get(FoodCButtonKey);
        }
        
        public static string WaterCButton()
        {
            return Language.main.Get(WaterCButtonKey);
        }

        public static string TrashButton()
        {
            return Language.main.Get(TrashButtonKey);
        }

        public static string TrashMessage()
        {
            return Language.main.Get(TrashMessageKey);
        }

        public static string NotEmpty()
        {
            return Language.main.Get(NotEmptyKey);
        }

        public static string NoPower()
        {
            return Language.main.Get(NoPowerKey);
        }

        public static string TakeItemFormat()
        {
            return Language.main.Get(TakeItemFormatKey);
        }
    }
}
