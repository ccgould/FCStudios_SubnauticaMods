using SMLHelper.V2.Handlers;

namespace FCS_DeepDriller.Buildable
{
    internal partial class FCSDeepDrillerBuildable
    {
        private const string StorageLabelKey = "DD_StorageLabel";
        private const string ItemNotAllowedKey = "DD_ItemNotAllowed";
        private const string EquipmentLabelKey = "DD_EquipmentLabel";


        internal static string StorageContainerLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string ItemNotAllowed()
        {
            return Language.main.Get(ItemNotAllowedKey);
        }
        internal static string EquipmentContainerLabel()
        {
            return Language.main.Get(EquipmentLabelKey);
        }

        public static string BuildableName { get; private set; }

        public static TechType TechTypeID { get; private set; }

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            LanguageHandler.SetLanguageLine(StorageLabelKey, "FCS Deep Driller Receptacle");
            LanguageHandler.SetLanguageLine(ItemNotAllowedKey, "Cannot place this item in here.");
            LanguageHandler.SetLanguageLine(EquipmentLabelKey, "FCS Deep Driller Modules");
        }


    }
}
