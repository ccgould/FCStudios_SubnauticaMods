using ARS_SeaBreezeFCS32.Configuration;
using SMLHelper.V2.Handlers;
using System;

namespace ARS_SeaBreezeFCS32.Buildables
{
    internal partial class ARSSeaBreezeFCS32Buildable
    {
        private const string StorageLabelKey = "SeaBreezeStorage";
        private const string FcsWorkBenchErrorMessageKey = "FCSWorkBenechErrorMessage";
        private const string CannotRefillMessageKey = "CannotRefillMessage";
        private const string SeaBreezeFullKey = "SeaBreezeFull";
        private const string ItemNotAllowedKey = "ItemNotAllowed";
        private const string TimeLeftMessageKey = "TimeLeftMessage";
        private const string AddedFreonMessageKey = "AddedFreonMessage";
        private const string OnSeabreezeHoverkey = "OnSeabreezeHover";
        private const string OnFreonBTNHoverKey = "OnFreonButtonHover";
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
        private const string NoPowerKey = "SB_NoPower";
        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }



        public static string OnHoverTextNoPower()
        {
            return Language.main.Get(OnHoverUnpoweredKey);
        }

        public static string BuildableName { get; private set; }
        public static TechType TechTypeID { get; private set; }


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
            LanguageHandler.SetLanguageLine(ItemKey, "Item/s");
            LanguageHandler.SetLanguageLine(EditUnitNameKey, $"Edit {Mod.FriendlyName} Name");
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
        }


        public static string TimeLeftMessage()
        {
            return Language.main.Get(TimeLeftMessageKey);
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

        public static string GoHome()
        {
            return Language.main.Get(GoHomeKey);
        }

        public static string ColorPicker()
        {
            return Language.main.Get(ColorPickerKey);
        }

        public static string OnFreonBTNHover()
        {
            return Language.main.Get(OnFreonBTNHoverKey);
        }

        public static string AddedFreonMessage()
        {
            return Language.main.Get(AddedFreonMessageKey);
        }

        public static string OnSeabreezeHover()
        {
            return Language.main.Get(OnSeabreezeHoverkey);
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
    }
}
