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

        internal static string BuildableName { get; private set; }

        internal static TechType TechTypeID { get; private set; }

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

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnlyPowercellsAllowedKey, "Only powercells are allowed.");
            LanguageHandler.SetLanguageLine(DDAttachmentsOnlyKey, "Only FCS Deep Driller Attachments allowed at a time!");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "FCS Deep Driller Receptacle");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Cannot place this item in here.");
            LanguageHandler.SetLanguageLine(EquipmentLabelKey, "FCS Deep Driller Attachments");
            LanguageHandler.SetLanguageLine(BEquipmentLabelKey, "FCS Deep Driller Batteries");
            LanguageHandler.SetLanguageLine(OnBatteryHoverTextKey, "Open Battery Attachment");
            LanguageHandler.SetLanguageLine(OPAllowedKey, "Only One Power Attachment Allowed");
        }
    }
}
