using SMLHelper.V2.Handlers;

namespace AMMiniMedBay.Buildable
{
    internal partial class AMMiniMedBayBuildable
    {
        private const string StorageLabelKey = "AMMiniMedBayStorage";
        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        private const string OnHoverUnpoweredKey = "AMMiniMedBayNoPower";
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
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Alterra Medical Mini MedBay Receptacle");
            LanguageHandler.SetLanguageLine(OnHoverUnpoweredKey, "Insufficient power");
        }
    }
}
