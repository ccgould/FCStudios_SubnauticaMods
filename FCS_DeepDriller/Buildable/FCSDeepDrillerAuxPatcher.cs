using SMLHelper.V2.Handlers;

namespace FCS_DeepDriller.Buildable
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

        internal static string BuildableName { get; private set; }

        internal static TechType TechTypeID { get; private set; }


        internal static string StorageContainerLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string DDAttachmentsOnly()
        {
            return Language.main.Get(DDAttachmentsOnlyKey);
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

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnlyPowercellsAllowedKey, "Only powercells are allowed.");
            LanguageHandler.SetLanguageLine(DDAttachmentsOnlyKey, "Only FCS Deep Driller Attachments allowed at a time!");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "FCS Deep Driller Receptacle.");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Cannot place this item in here.");
            LanguageHandler.SetLanguageLine(EquipmentLabelKey, "FCS Deep Driller Attachments.");
            LanguageHandler.SetLanguageLine(BEquipmentLabelKey, "FCS Deep Driller Battery Attachment Receptacle.");
            LanguageHandler.SetLanguageLine(OnBatteryHoverTextKey, "Open Battery Attachment.");
            LanguageHandler.SetLanguageLine(OPAllowedKey, "Only One Power Attachment Allowed.");
            LanguageHandler.SetLanguageLine(OneUpgradeAllowedKey, "Only One Upgrade Allowed.");
            LanguageHandler.SetLanguageLine(BatteryAttachmentHasBatteriesKey, "Battery attachment has batteries cannot remove!");
            LanguageHandler.SetLanguageLine(NextPageKey, "Next Page");
            LanguageHandler.SetLanguageLine(PrevPageKey, "Previous Page");
            LanguageHandler.SetLanguageLine(FocusKey, "FOCUS");
            LanguageHandler.SetLanguageLine(FocusingKey, "FOCUSING");
            LanguageHandler.SetLanguageLine(RemoveAllModulesKey, "Please Remove all items from the driller.");
        }

        public static string RemoveAllItems()
        {
            return Language.main.Get(RemoveAllModulesKey);

        }

        public static string OneUpgradeAllowed()
        {
            return Language.main.Get(OneUpgradeAllowedKey);
        }
    }
}
