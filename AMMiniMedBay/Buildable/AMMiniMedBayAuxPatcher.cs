using SMLHelper.V2.Handlers;

namespace AMMiniMedBay.Buildable
{
    internal partial class AMMiniMedBayBuildable
    {
        public const string HealthStatusLBLKey = "HeathStatus";
        public const string NoPowerKey = "NoPower";
        public const string NoPowerMessage = "NoPowerMessage";
        public const string ColorPageTextKey = "OnColorPageBTNHover";
        public const string OpenStorageKey = "OpenStorage";
        public const string NotInPositionMessageKey = "NotInPositionMessage";

        private const string StorageLabelKey = "AMMiniMedBayStorage";

        public const string HealKey = "Heal";

        public static string StorageLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        private const string OnHoverUnpoweredKey = "AMMiniMedBayNoPower";

        public const string OnHoverLPaginatorKey = "LNavText";

        public const string OnHoverRPaginatorKey = "RNavText";

        public static string OnHoverTextNoPower()
        {
            return Language.main.Get(OnHoverUnpoweredKey);
        }

        public static string BuildableName { get; private set; }

        public static TechType TechTypeID { get; private set; }

        public const string ContainerNotEmptyMessageKey = "ContainerNotEmpty";


        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Alterra Medical Mini MedBay Receptacle");
            LanguageHandler.SetLanguageLine(OnHoverLPaginatorKey, "Previous Page");
            LanguageHandler.SetLanguageLine(OnHoverRPaginatorKey, "Next Page");
            LanguageHandler.SetLanguageLine(OnHoverUnpoweredKey, "Insufficient power");
            LanguageHandler.SetLanguageLine(HealKey, "Heal");
            LanguageHandler.SetLanguageLine(HealthStatusLBLKey, "Health Status");
            LanguageHandler.SetLanguageLine(NoPowerKey, "NOT ENOUGH POWER");
            LanguageHandler.SetLanguageLine(OpenStorageKey, "Open MedBay Storage.");
            LanguageHandler.SetLanguageLine(NoPowerMessage, "Due to power loss this device is inoperable");
            LanguageHandler.SetLanguageLine(NotInPositionMessageKey, "You are not standing in the correct place. Please go to the (Stand Here) position.");
            LanguageHandler.SetLanguageLine(ColorPageTextKey, "Color Page");
            LanguageHandler.SetLanguageLine(ContainerNotEmptyMessageKey, "MedKit container is not empty.");

        }
    }
}
