using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_DeepDriller.Buildable.MK1
{
    internal partial class FCSDeepDrillerBuildable
    {
        private const string StorageLabelKey = "DD_StorageLabel";
        private const string ItemNotAllowedKey = "DD_ItemNotAllowed";
        private const string EquipmentLabelKey = "DD_EquipmentLabel";
        private const string BEquipmentLabelKey = "DD_BEquipmentLabel";
        private const string DDAttachmentsOnlyKey = "DD_AttachmentsOnly";
        private const string OnlyPowercellsAllowedKey = "DD_OnlyPowercellsAllowed";
        private const string OnBatteryHoverTextKey = "DD_OnBatteryHoverText";
        private const string OPAllowedKey = "DD_OPAllowed";
        private const string BatteryAttachmentHasBatteriesKey = "DD_BatteryAttachmentHasBatteries";
        private const string NextPageKey = "DD_NextPage";
        private const string PrevPageKey = "DD_PrevPage";
        private const string FocusingKey = "DD_Focusing";
        private const string FocusKey = "DD_Focus";
        private const string RemoveAllModulesKey = "DD_RemoveAllAttachments";
        private const string OneUpgradeAllowedKey = "DD_OneUpgradeAllowed";
        private const string BiomeKey = "DD_Biome";
        private const string PowerButtonKey = "DD_PowerBTN";
        private const string ItemsPerDayFormatKey = "DD_ItemsPerDayFormat";
        private const string PowerUsageFormatKey = "DD_PowerUsageFormat";
        private const string ItemsButtonKey = "DD_ItemsButton";
        private const string MaintenanceButtonKey = "DD_MaintenanceButton";
        private const string ProgrammingButtonKey = "DD_ProgrammingButton";
        private const string SettingsButtonKey = "DD_SettingsButton";
        private const string TakeFormattedKey = "DD_TakeFormatted";
        private const string OilTankNotEmptyFormatKey = "DD_OilTankNotEmptyFormat";
        private const string OilTankDumpContainerTitleKey = "DD_OilTankDropContainerTitle";
        private const string PowercellDumpContainerTitleKey = "DD_PowercellDumpContainerTitle";

        internal static string BuildableName { get; private set; }

        internal static TechType TechTypeID { get; private set; }
        
        internal static string ItemNotAllowed()
        {
            return Language.main.Get(ItemNotAllowedKey);
        }
        internal static string EquipmentContainerLabel()
        {
            return Language.main.Get(EquipmentLabelKey);
        }

        internal static string BEquipmentContainerLabel()
        {
            return Language.main.Get(BEquipmentLabelKey);
        }

        internal static string OnlyPowercellsAllowed()
        {
            return Language.main.Get(OnlyPowercellsAllowedKey);
        }

        internal static string OnBatteryHoverText()
        {
            return Language.main.Get(OnBatteryHoverTextKey);
        }

        internal static string OnePowerAttachmentAllowed()
        {
            return Language.main.Get(OPAllowedKey);
        }

        internal static string BatteryAttachmentHasBatteries()
        {
            return Language.main.Get(BatteryAttachmentHasBatteriesKey);
        }

        internal static string PrevPage()
        {
            return Language.main.Get(PrevPageKey);
        }

        internal static string NextPage()
        {
            return Language.main.Get(NextPageKey);
        }

        internal static string Focusing()
        {
            return Language.main.Get(FocusingKey);
        }

        internal static string Focus()
        {
            return Language.main.Get(FocusKey);
        }

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnlyPowercellsAllowedKey, "Only powercells are allowed.");
            LanguageHandler.SetLanguageLine(DDAttachmentsOnlyKey, "Only FCS Deep Driller Attachments allowed!");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "FCS Deep Driller Receptacle.");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Cannot place this item in here.");
            LanguageHandler.SetLanguageLine(EquipmentLabelKey, "FCS Deep Driller Attachments.");
            LanguageHandler.SetLanguageLine(BEquipmentLabelKey, "FCS Deep Driller Powercell Attachment Receptacle.");
            LanguageHandler.SetLanguageLine(OnBatteryHoverTextKey, "Open Powercell Attachment.");
            LanguageHandler.SetLanguageLine(OPAllowedKey, "Only One Power Attachment Allowed.");
            LanguageHandler.SetLanguageLine(OneUpgradeAllowedKey, "Only One Upgrade Allowed.");
            LanguageHandler.SetLanguageLine(BatteryAttachmentHasBatteriesKey, "Battery attachment has powercells cannot remove!");
            LanguageHandler.SetLanguageLine(NextPageKey, "Next Page");
            LanguageHandler.SetLanguageLine(PrevPageKey, "Previous Page");
            LanguageHandler.SetLanguageLine(FocusKey, "FOCUS");
            LanguageHandler.SetLanguageLine(FocusingKey, "FOCUSING");
            LanguageHandler.SetLanguageLine(RemoveAllModulesKey, "Please Remove all items from the driller.");
            LanguageHandler.SetLanguageLine(BiomeKey, "BIOME");
            LanguageHandler.SetLanguageLine(PowerButtonKey, "Power ON/OFF");
            LanguageHandler.SetLanguageLine(ItemsPerDayFormatKey, "Items Daily: {0}");
            LanguageHandler.SetLanguageLine(PowerUsageFormatKey, "Power Usage Per Second: {0}");
            LanguageHandler.SetLanguageLine(ItemsButtonKey, "Go to inventory page");
            LanguageHandler.SetLanguageLine(MaintenanceButtonKey, "Go to maintenance page");
            LanguageHandler.SetLanguageLine(ProgrammingButtonKey, "Go to programming page");
            LanguageHandler.SetLanguageLine(SettingsButtonKey, "Go to settings page");
            LanguageHandler.SetLanguageLine(TakeFormattedKey, "Take {0}");
            LanguageHandler.SetLanguageLine(OilTankNotEmptyFormatKey, "Oil tank cannot oil anymore oil at this time try again in {0} minutes");
            LanguageHandler.SetLanguageLine(OilTankDumpContainerTitleKey, "Oil Tank Dump Receptical.");
            LanguageHandler.SetLanguageLine(PowercellDumpContainerTitleKey, "Powercell Draining Receptical.");
        }

        public static string RemoveAllItems()
        {
            return Language.main.Get(RemoveAllModulesKey);

        }


        internal static string Biome()
        {
            return Language.main.Get(BiomeKey);
        }

        internal static string PowerButton()
        {
            return Language.main.Get(PowerButtonKey);
        }

        internal static string ItemsPerDayFormat()
        {
            return Language.main.Get(ItemsPerDayFormatKey);
        }

        internal static string PowerUsageFormat()
        {
            return Language.main.Get(PowerUsageFormatKey);
        }

        public static string ItemsButton()
        {
            return Language.main.Get(ItemsButtonKey);
        }

        public static string MaintenanceButton()
        {
            return Language.main.Get(MaintenanceButtonKey);
        }

        public static string ProgrammingButton()
        {
            return Language.main.Get(ProgrammingButtonKey);
        }

        public static string SettingsButton()
        {
            return Language.main.Get(SettingsButtonKey);
        }

        public static string TakeFormatted()
        {
            return Language.main.Get(TakeFormattedKey);
        }

        public static string StorageFull()
        {
            return Language.main.Get("InventoryFull");
        }

        public static string OilTankNotEmpty()
        {
            return Language.main.Get(OilTankNotEmptyFormatKey);
        }

        public static string OilDropContainerTitle()
        {
            return Language.main.Get(OilTankDumpContainerTitleKey);
        }

        public static string NotAllowedItem()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }

        public static string PowercellDumpContainerTitle()
        {
            return Language.main.Get(PowercellDumpContainerTitleKey);
        }
    }
}
