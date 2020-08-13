using SMLHelper.V2.Handlers;
using UnityEngine;

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
        private const string NoMoreSpaceKey = "Ex_NoMoreSpace";
        private const string NoPlayerToolsMessageKey = "Ex_NoPlayerToolsMessage";
        private const string NoUndiscorveredEggsMessageKey = "Ex_NoUndiscorveredEggsMessage";
        private const string NoDegradableFoodMessageKey = "Ex_NoDegradableFoodMessage";
        private const string AddToExStorageKey = "Ex_AddToExStorage";
        private const string TakeItemFormatKey = "Ex_TakeItemFormat";

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
            LanguageHandler.SetLanguageLine(NoMoreSpaceKey, "Ex-Storage wont be able to hold any more items");
            LanguageHandler.SetLanguageLine(NoPlayerToolsMessageKey, "Ex-Storage cannot store PlayerTools at this time.");
            LanguageHandler.SetLanguageLine(NoUndiscorveredEggsMessageKey, "Ex-Storage cannot store undiscovered eggs at this time.");
            LanguageHandler.SetLanguageLine(NoDegradableFoodMessageKey, "Ex-Storage cannot store degradable food.");
            LanguageHandler.SetLanguageLine(AddToExStorageKey, "Add items to Ex-Storage.");
            LanguageHandler.SetLanguageLine(TakeItemFormatKey, "Take {0}.");
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

        public static string NoMoreSpace()
        {
            return Language.main.Get(NoMoreSpaceKey);

        }

        public static string NoPlayerTools()
        {
            return Language.main.Get(NoPlayerToolsMessageKey);

        }

        public static string NoUndiscorveredEggsMessage()
        {
            return Language.main.Get(NoUndiscorveredEggsMessageKey);

        }

        public static string FoodNotAllowed()
        {
            return Language.main.Get(NoDegradableFoodMessageKey);
        }

        public static string AddToExStorage()
        {
            return Language.main.Get(AddToExStorageKey);
        }

        public static string TakeItemFormat()
        {
            return Language.main.Get(TakeItemFormatKey);
        }
    }
}
