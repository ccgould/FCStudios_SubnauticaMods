using SMLHelper.V2.Handlers;

namespace ARS_SeaBreezeFCS32.Buildables
{
    internal partial class ARSSeaBreezeFCS32Buildable
    {
        private const string StorageLabelKey = "SeaBreezeStorage";
        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }


        private const string OnHoverUnpoweredKey = "SeaBreezeNoPower";
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
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Ion Cube Generator Receptical");
            LanguageHandler.SetLanguageLine(OnHoverUnpoweredKey, "Insufficient power");
        }
    }
}
