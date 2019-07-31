using SMLHelper.V2.Handlers;

namespace ExStorageDepot.Buildable
{
    internal partial class ExStorageDepotBuildable
    {
        private const string ContainerFullMessageKey = "ES_ContainerFullMessage";
        private const string StorageLabelKey = "Ex_StorageLabel";

        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(ContainerFullMessageKey, "The storage is full item couldn't be added");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Ex-Storage Depot Receptacle.");
        }

        internal static string ContainerFullMessage()
        {
            return Language.main.Get(ContainerFullMessageKey);
        }

        internal static string StorageContainerLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

    }
}
