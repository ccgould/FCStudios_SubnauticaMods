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
        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(OnlyPowercellsAllowedKey, "Only powercells are allowed.");
            LanguageHandler.SetLanguageLine(DDAttachmentsOnlyKey, "Only FCS Deep Driller Attachments allowed!");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "FCS Deep Driller Receptacle");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Cannot place this item in here.");
            LanguageHandler.SetLanguageLine(EquipmentLabelKey, "FCS Deep Driller Attachments");
            LanguageHandler.SetLanguageLine(BEquipmentLabelKey, "FCS Deep Driller Batteries");
        }
    }
}
