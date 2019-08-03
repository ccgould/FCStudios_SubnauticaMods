using SMLHelper.V2.Handlers;

namespace ExStorageDepot.Buildable
{
    internal partial class ExStorageDepotBuildable
    {
        private const string ContainerFullMessageKey = "ES_ContainerFullMessage";
        private const string StorageLabelKey = "Ex_StorageLabel";
        private const string DumpStorageLabelKey = "Ex_DumpStorageLabel";
        private const string PrevButtonKey = "Ex_PrevButton";
        private const string NextButtonKey = "Ex_NextButton";
        private const string RenameStorageKey = "Ex_RenameStorage";
        private const string SubmitKey = "Ex_Submit";

        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }

        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            LanguageHandler.SetLanguageLine(ContainerFullMessageKey, "The storage is full item couldn't be added");
            LanguageHandler.SetLanguageLine(StorageLabelKey, "Ex-Storage Depot Receptacle.");
            LanguageHandler.SetLanguageLine(DumpStorageLabelKey, "Ex-Storage Depot Dump Receptacle.");
            LanguageHandler.SetLanguageLine(PrevButtonKey, "Previous Page");
            LanguageHandler.SetLanguageLine(NextButtonKey, "Next Page");
            LanguageHandler.SetLanguageLine(RenameStorageKey, "Rename Storage");
            LanguageHandler.SetLanguageLine(SubmitKey, "Submit");
        }

        internal static string ContainerFullMessage()
        {
            return Language.main.Get(ContainerFullMessageKey);
        }

        internal static string StorageContainerLabel()
        {
            return Language.main.Get(StorageLabelKey);
        }

        internal static string PrevPage()
        {
            return Language.main.Get(PrevButtonKey);
        }

        internal static string NextPage()
        {
            return Language.main.Get(NextButtonKey);
        }

        public static string RenameStorage()
        {
            return Language.main.Get(RenameStorageKey);
        }

        public static string Submit()
        {
            return Language.main.Get(SubmitKey);
        }

        public static string DumpContainerLabel()
        {
            return Language.main.Get(DumpStorageLabelKey);
        }
    }
}
