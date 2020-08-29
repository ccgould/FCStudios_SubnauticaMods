using SMLHelper.V2.Handlers;

namespace FCS_DeepDriller.Buildable.MK2
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
        private const string AddProgramButtonKey = "DD_AddProgramButton";
        private const string InvalidFunctionFormatKey = "DD_InvalidFunctionFormat";
        private const string InvalidClassFormatKey = "DD_InvalidClassFormat";
        private const string IncorrectAmountOfParameterFormatKey = "DD_IncorrectAmountOfParameterFormat";
        private const string IncorrectParameterFormatKey = "DD_IncorrectParameterFormat";
        private const string NotOreErrorFormatKey = "DD_NotOreErrorFormat";
        private const string FilterButtonKey = "DD_FilterButton";
        private const string ColorButtonKey = "DD_ColorButton";
        private const string SolarButtonKey = "DD_SolarButton";
        private const string ExportToggleKey = "DD_ExportToggle";
        private const string ToggleRangeKey = "DD_ToggleRange";

        internal static string BuildableName { get; private set; }

        internal static TechType TechTypeID { get; private set; }
        
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
            LanguageHandler.SetLanguageLine(ItemsButtonKey, "Go to inventory page.");
            LanguageHandler.SetLanguageLine(MaintenanceButtonKey, "Go to maintenance page.");
            LanguageHandler.SetLanguageLine(ProgrammingButtonKey, "Go to programming page.");
            LanguageHandler.SetLanguageLine(SettingsButtonKey, "Go to settings page.");
            LanguageHandler.SetLanguageLine(TakeFormattedKey, "Take {0}");
            LanguageHandler.SetLanguageLine(OilTankNotEmptyFormatKey, "Oil tank cannot oil anymore oil at this time try again in {0} minutes");
            LanguageHandler.SetLanguageLine(OilTankDumpContainerTitleKey, "Oil Tank Dump Receptical.");
            LanguageHandler.SetLanguageLine(PowercellDumpContainerTitleKey, "Powercell Draining Receptical.");
            LanguageHandler.SetLanguageLine(AddProgramButtonKey, "Add a upgrade function to the drill.");
            LanguageHandler.SetLanguageLine(InvalidFunctionFormatKey, "Invalid Function: {0}. This function doesn't exist please check the documentation.");
            LanguageHandler.SetLanguageLine(InvalidClassFormatKey, "Invalid Class: {0}. This class doesn't exist please check the documentation.");
            LanguageHandler.SetLanguageLine(IncorrectParameterFormatKey, "Incorrect type in parameter expected: {0} ex: ({1};) .");
            LanguageHandler.SetLanguageLine(IncorrectAmountOfParameterFormatKey, "Incorrect amount of parameters expected {0} got {1}.");
            LanguageHandler.SetLanguageLine(NotOreErrorFormatKey, "TechType {0} is not an ore.");
            LanguageHandler.SetLanguageLine(FilterButtonKey, "Toggle Filters.");
            LanguageHandler.SetLanguageLine(ColorButtonKey, "Go to color picker page.");
            LanguageHandler.SetLanguageLine(SolarButtonKey, "Toggle Solar Panels.");
            LanguageHandler.SetLanguageLine(ToggleRangeKey, "Show/Hide Range");
            LanguageHandler.SetLanguageLine(ExportToggleKey, "Toggle export to ExStorage.");
        }


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

        internal static string RemoveAllItems()
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

        internal static string ItemsButton()
        {
            return Language.main.Get(ItemsButtonKey);
        }

        internal static string MaintenanceButton()
        {
            return Language.main.Get(MaintenanceButtonKey);
        }

        internal static string ProgrammingButton()
        {
            return Language.main.Get(ProgrammingButtonKey);
        }

        internal static string SettingsButton()
        {
            return Language.main.Get(SettingsButtonKey);
        }

        internal static string TakeFormatted()
        {
            return Language.main.Get(TakeFormattedKey);
        }

        internal static string StorageFull()
        {
            return Language.main.Get("InventoryFull");
        }

        internal static string OilTankNotEmpty()
        {
            return Language.main.Get(OilTankNotEmptyFormatKey);
        }

        internal static string OilDropContainerTitle()
        {
            return Language.main.Get(OilTankDumpContainerTitleKey);
        }

        internal static string NotAllowedItem()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }

        internal static string PowercellDumpContainerTitle()
        {
            return Language.main.Get(PowercellDumpContainerTitleKey);
        }

        internal static string AddProgramButton()
        {
            return Language.main.Get(AddProgramButtonKey);
        }

        internal static string InvalidFunctionFormat()
        {
            return Language.main.Get(InvalidFunctionFormatKey);
        }
        internal static string InvalidClassFormat()
        {
            return Language.main.Get(InvalidClassFormatKey);
        }

        internal static string IncorrectParameterFormat()
        {
            return Language.main.Get(IncorrectParameterFormatKey);
        }

        internal static string IncorrectAmountOfParameterFormat()
        {
            return Language.main.Get(IncorrectAmountOfParameterFormatKey);
        }

        internal static string NotOreErrorFormat()
        {
            return Language.main.Get(NotOreErrorFormatKey);
        }

        internal static string ColorButton()
        {
            return Language.main.Get(ColorButtonKey);
        }

        internal static string FilterButton()
        {
            return Language.main.Get(FilterButtonKey);
        }

        internal static string SolarButton()
        {
            return Language.main.Get(SolarButtonKey);
        }

        internal static string ExportToggleButton()
        {
            return Language.main.Get(ExportToggleKey);
        }

        internal static string ToggleRangeButton()
        {
            return Language.main.Get(ToggleRangeKey);
        }
    }
}
