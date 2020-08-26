using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;

namespace FCS_HydroponicHarvesters.Buildables
{
    internal partial class HydroponicHarvestersBuildable
    {
        private const string MaxKey = "HH_Max";
        private const string HighKey = "HH_High";
        private const string MinKey = "HH_Min";
        private const string LowKey = "HH_Low";
        private const string OffKey = "HH_Off";
        private const string CannotDeleteDNAItemKey = "HH_CannotDeleteDNAItem";
        private const string NotDirtyKey = "HH_NotDirty";
        private const string HMSTimeKey = "HH_HMSTime";
        private const string AmountOfItemsKey = "HH_Items";
        private const string HasItemsMessageKey = "HH_HasItemsMessage";
        private const string NotOnBaseKey = "HH_NotOnBase";
        private const string TakeKey = "HH_Take";
        private const string DNaSampleKey = "HH_DNASample";
        private const string ToggleLightMessageKey = "HH_ToggleLightMessage";
        private const string CleanerBTNMessageKey = "HH_CleanerBTNMessage";
        private const string DumpBTNMessageKey = "HH_DumpBTNMessage";
        private const string ColorPickerBTNMessageKey = "HH_ColorPickerBTNMessage";
        private const string PowerLevelBTNMessageKey = "HH_PowerLevelBTNMessage";
        private const string ModeBTNMessageKey = "HH_ModeBTNMessage";
        private const string StorageFullKey = "HH_StorageFull";
        private const string FloraKleenDropContainerTitleKey = "HH_FloraKleenDropContainerTitle";
        private const string DNADropContainerTitleKey = "HH_DNADropContainerTitle";
        private const string HasDNAItemsMessageKey = "HH_HasDNAItemsMessage";
        private const string IncorrectEnvironmentKey = "HH_IncorrectEnvironment";
        private const string PowerUnitPerSecondKey = "HH_PowerUnitPerSecond";

        internal string BuildableName { get; set; }

        internal TechType TechTypeID { get; set; }

        private void AdditionalPatching()
        {
            QuickLogger.Debug("Additional Properties");
            BuildableName = FriendlyName;
            TechTypeID = TechType;

            LanguageHandler.SetLanguageLine(MaxKey, "MAX");
            LanguageHandler.SetLanguageLine(HighKey, "HIGH");
            LanguageHandler.SetLanguageLine(MinKey, "MIN");
            LanguageHandler.SetLanguageLine(LowKey, "LOW");
            LanguageHandler.SetLanguageLine(OffKey, "OFF");
            LanguageHandler.SetLanguageLine(TakeKey, "Take");
            LanguageHandler.SetLanguageLine(DNaSampleKey, "DNA Sample");
            LanguageHandler.SetLanguageLine(CannotDeleteDNAItemKey, "Cannot delete DNA {0} because there are items still in the harvester.");
            LanguageHandler.SetLanguageLine(NotDirtyKey, "Cannot clean! This harvester isn't dirty enough.");
            LanguageHandler.SetLanguageLine(HMSTimeKey, "Time Left Until Dirty: {0}");
            LanguageHandler.SetLanguageLine(AmountOfItemsKey, "{0} Items");
            LanguageHandler.SetLanguageLine(HasItemsMessageKey, "Hydroponic Harvester is not empty.");
            LanguageHandler.SetLanguageLine(HasDNAItemsMessageKey, "Hydroponic Harvester has DNA samples please remove to deconstruct.");
            LanguageHandler.SetLanguageLine(NotOnBaseKey, "NOT CONNECTED TO A BASE CANNOT OPERATE");
            LanguageHandler.SetLanguageLine(ToggleLightMessageKey, "Toggle Internal Light");
            LanguageHandler.SetLanguageLine(CleanerBTNMessageKey, "Add FloraKleen");
            LanguageHandler.SetLanguageLine(DumpBTNMessageKey, "Add DNA Sample");
            LanguageHandler.SetLanguageLine(ColorPickerBTNMessageKey, "Change Device Color");
            LanguageHandler.SetLanguageLine(PowerLevelBTNMessageKey, "Change Power Level");
            LanguageHandler.SetLanguageLine(ModeBTNMessageKey, "Switch Unit Harvester Bed Mode (Water/Land).");
            LanguageHandler.SetLanguageLine(StorageFullKey, "Cannot add anymore DNA samples. Remove one to add.");
            LanguageHandler.SetLanguageLine(FloraKleenDropContainerTitleKey, "FloraKleen Refill.");
            LanguageHandler.SetLanguageLine(DNADropContainerTitleKey, "DNA Sample receptacle");
            LanguageHandler.SetLanguageLine(IncorrectEnvironmentKey, "Incorrect environment please switch the grow bed mod to another.");
            LanguageHandler.SetLanguageLine(PowerUnitPerSecondKey, "Power Per Second.");
        }

        internal static string Max()
        {
            return Language.main.Get(MaxKey);
        }

        internal static string High()
        {
            return Language.main.Get(HighKey);
        }

        internal static string Min()
        {
            return Language.main.Get(MinKey);
        }

        internal static string Low()
        {
            return Language.main.Get(LowKey);
        }

        internal static string Off()
        {
            return Language.main.Get(OffKey);
        }

        internal static string CannotDeleteDNAItem(string itemName)
        {
            return string.Format(Language.main.Get(CannotDeleteDNAItemKey), itemName);
        }

        internal static string UnitIsntDirty()
        {
            return Language.main.Get(NotDirtyKey);
        }

        internal static string HMSTime()
        {
            return Language.main.Get(HMSTimeKey);
        }
        internal static string AmountOfItems()
        {
            return Language.main.Get(AmountOfItemsKey);
        }

        internal static string NotAllowedItem()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }

        internal static string HasItemsMessage()
        {
            return Language.main.Get(HasItemsMessageKey);
        }

        internal static string NotOnBaseMessage()
        {
            return Language.main.Get(NotOnBaseKey);
        }

        internal static string Delete()
        {
            return Language.main.Get("ScreenshotDelete");
        }

        internal static string Take()
        {
            return Language.main.Get(TakeKey);
        }

        internal static string DNASample()
        {
            return Language.main.Get(DNaSampleKey);
        }

        public static string ToggleLightMessage()
        {
            return Language.main.Get(ToggleLightMessageKey);
        }

        public static string CleanerBTNMessage()
        {
            return Language.main.Get(CleanerBTNMessageKey);
        }

        public static string DumpBTNMessage()
        {
            return Language.main.Get(DumpBTNMessageKey);
        }

        internal static string ColorPickerBTNMessage()
        {
            return Language.main.Get(ColorPickerBTNMessageKey);
        }

        internal static string PowerLevelBTNMessage()
        {
            return Language.main.Get(PowerLevelBTNMessageKey);
        }

        internal static string ModeBTNMessage()
        {
            return Language.main.Get(ModeBTNMessageKey);
        }

        internal static string DNADropContainerTitle()
        {
            return Language.main.Get(DNADropContainerTitleKey);
        }

        internal static string FloraKleenDropContainerTitle()
        {
            return Language.main.Get(FloraKleenDropContainerTitleKey);
        }

        internal static string StorageFull()
        {
            return Language.main.Get(StorageFullKey);
        }

        internal static string NotAvailable()
        {
            return string.Format(HMSTime(), Language.main.Get("ChargerSlotEmpty"));
        }

        public static string HasDNAItemsMessage()
        {
            return Language.main.Get(HasDNAItemsMessageKey);
        }

        public static string IncorrectEnvironment()
        {
            return Language.main.Get(IncorrectEnvironmentKey);
        }

        public static string PowerUnitPerSecond()
        {
            return Language.main.Get(PowerUnitPerSecondKey);
        }
    }
}
