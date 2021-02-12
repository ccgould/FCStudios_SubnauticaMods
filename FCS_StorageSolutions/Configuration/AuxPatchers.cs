﻿using System.Collections.Generic;
using SMLHelper.V2.Handlers;

namespace FCS_StorageSolutions.Configuration
{
    internal static class AuxPatchers
    {
        private const string ModKey = "ASS";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_OpenAlterraStorage","Open Alterra Storage"},
            { $"{ModKey}_AlterraStorageDumpContainerTitle","Alterra Storage"},
            { $"{ModKey}_TakeFormat","Take {0}"},
            { $"{ModKey}_StorageAmountFormat","{0}/{1} Items"},
            { $"{ModKey}_OpenWallServerRack","Open Wall Server Rack"},
            { $"{ModKey}_CloseWallServerRack","Close Wall Server Rack"},
            { $"{ModKey}_AddItemToItemDisplay","Add Item"},
            { $"{ModKey}_AddItemToItemDisplayDesc","Click to add an item to the display to should the quantity in the system"},
            { $"{ModKey}_NoItemToTake","Please choose an item to display and extract from the network."},
            { $"{ModKey}_AddItemToNetwork","Add item/s to base"},
            { $"{ModKey}_AddItemToNetworkDesc","Allows you to add items to your Data Storage network."},
            { $"{ModKey}_Reset","Reset."},
            { $"{ModKey}_ItemDisplayResetDesc","Resets the item display so you can choose another item to display."},
            { $"{ModKey}_RackCountFormat","Racks:{0}"},
            { $"{ModKey}_ServerCountFormat","Servers:{0}"},
            { $"{ModKey}_TotalItemFormat","Total Items:{0}/{1}"},
            { $"{ModKey}_Rename","Rename"},
            { $"{ModKey}_RenameDesc","Rename this base's name."},
            { $"{ModKey}_GlobalNetwork","Global Network"},
            { $"{ModKey}_Multiplier","Multiplier"},
            { $"{ModKey}_MultiplierDesc","Allows you to extract more with one click."},
            { $"{ModKey}_SearchItem","Search item..."},
            { $"{ModKey}_RackNotEmpty","Cannot deconstruct rack is not empty."},
            { $"{ModKey}_AddServer","Add Server"},
            { $"{ModKey}_AddServerDesc","Add server to formatting station to add filters."},
            { $"{ModKey}_FilterItemName","Item: {0}"},
            { $"{ModKey}_FilterCategoryName","Category: {0}"},
            { $"{ModKey}_MoonpoolSettings","Moonpool Settings"},
            { $"{ModKey}_AddToBlackList","Add item to blacklist"},
            { $"{ModKey}_CraftFormatted","Add {0} to queue"},
            { $"{ModKey}_EnterAmount","Enter Amount"},
            { $"{ModKey}_AmountIsZero","{0} entry amount is 0 please specify ab amount."},
            { $"{ModKey}_CurrentBase","[Current Base]"},
            { $"{ModKey}_RemoteBase","[Remote Base]"},
            { $"{ModKey}_IsFormatted","Is Formatted:"},
            { $"{ModKey}_Filters","Filters:"},
            { $"{ModKey}_PowerButton","Power Button"},
            { $"{ModKey}_RenameRemoteStorage","Rename this remote storage"},
            { $"{ModKey}_AutocrafterItemIsBeingCrafted","Cannot deconstruct because AutoCrafter is currently crafting an item or there are items on the belt"},
        };
        
        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(LanguageDictionary[key]) : string.Empty;
        }

        internal static string OpenAlterraStorage()
        {
            return GetLanguage($"{ModKey}_OpenAlterraStorage");
        }

        public static string AlterraStorageDumpContainerTitle()
        {
            return GetLanguage($"{ModKey}_AlterraStorageDumpContainerTitle");
        }

        public static string TakeFormatted(string techType)
        {
            return string.Format(GetLanguage($"{ModKey}_TakeFormat"),techType);
        }

        public static string AlterraStorageAmountFormat(int count, int maxStorage)
        {
            return string.Format(GetLanguage($"{ModKey}_StorageAmountFormat"), count,maxStorage);
        }

        public static string ContainerNotEmpty()
        {
            return Language.main.Get("DeconstructNonEmptyStorageContainerError");
        }

        public static string OpenWallServerRack()
        {
            return GetLanguage($"{ModKey}_OpenWallServerRack");
        }

        public static string CloseWallServerRack()
        {
            return GetLanguage($"{ModKey}_CloseWallServerRack");
        }

        public static string AddItemToItemDisplay()
        {
            return GetLanguage($"{ModKey}_AddItemToItemDisplay");
        }

        public static string AddItemToItemDisplayDesc()
        {
            return GetLanguage($"{ModKey}_AddItemToItemDisplayDesc");
        }

        public static string NoItemToTake()
        {
            return GetLanguage($"{ModKey}_NoItemToTake");
        }

        public static string AddItemToNetwork()
        {
            return GetLanguage($"{ModKey}_AddItemToNetwork");
        }        
        
        public static string AddItemToNetworkDesc()
        {
            return GetLanguage($"{ModKey}_AddItemToNetworkDesc");
        }

        public static string Reset()
        {
            return GetLanguage($"{ModKey}_Reset");
        }
        
        public static string ItemDisplayResetDesc()
        {
            return GetLanguage($"{ModKey}_ItemDisplayResetDesc");
        }
        
        public static string RackCountFormat(int amount)
        {
            return string.Format(GetLanguage($"{ModKey}_RackCountFormat"),amount);
        }

        public static string ServerCountFormat(int amount)
        {
            return string.Format(GetLanguage($"{ModKey}_ServerCountFormat"),amount);
        }

        public static string TotalItemsFormat(int itemTotal,int serverTotal)
        {
            return string.Format(GetLanguage($"{ModKey}_TotalItemFormat"), itemTotal,serverTotal);
        }

        public static string RenameDesc()
        {
            return GetLanguage($"{ModKey}_RenameDesc");
        }

        public static string Rename()
        {
            return GetLanguage($"{ModKey}_Rename");
        }

        public static string GlobalNetwork()
        {
            return GetLanguage($"{ModKey}_GlobalNetwork");
        }

        public static string GlobalNetworkDesc()
        {
            return GetLanguage($"{ModKey}_GlobalNetworkDesc");
        }

        public static string Multiplier()
        {
            return GetLanguage($"{ModKey}_Multiplier");
        }

        public static string MultiplierDesc()
        {
            return GetLanguage($"{ModKey}_MultiplierDesc");
        }
        
        public static string SearchForItemsMessage()
        {
            return GetLanguage($"{ModKey}_SearchItem");
        }

        public static string RackNotEmpty()
        {
            return GetLanguage($"{ModKey}_RackNotEmpty");
        }

        public static string AddServer()
        {
            return GetLanguage($"{ModKey}_AddServer");
        }

        public static string FormattingMachineAddServerDesc()
        {
            return GetLanguage($"{ModKey}_AddServerDesc");
        }

        public static string AddFilter()
        {
            return GetLanguage($"{ModKey}_AddFilter");
        }

        public static string AddFilterDesc()
        {
            return GetLanguage($"{ModKey}_AddFilterDesc");
        }

        public static string FilterItemNameFormat(string name)
        {
            return string.Format(GetLanguage($"{ModKey}_FilterItemName"), name);
        }        
        
        public static string FilterCategoryNameFormat(string name)
        {
            return string.Format(GetLanguage($"{ModKey}_FilterCategoryName"), name);
        }

        public static string MoonpoolSettings()
        {
            return GetLanguage($"{ModKey}_MoonpoolSettings");
        }        
        
        public static string AddToBlackList()
        {
            return GetLanguage($"{ModKey}_AddToBlackList");
        }

        public static string CraftFormatted(string name)
        {
            return string.Format(GetLanguage($"{ModKey}_CraftFormatted"),name);
        }

        public static string EnterAmount()
        {
            return GetLanguage($"{ModKey}_EnterAmount");
        }

        public static string AmountIsZero(string name)
        {
            return string.Format(GetLanguage($"{ModKey}_AmountIsZero"), name);
        }

        public static string CurrentBase()
        {
            return GetLanguage($"{ModKey}_CurrentBase");
        }
        
        public static string RemoteBase()
        {
            return GetLanguage($"{ModKey}_RemoteBase");
        }

        public static string IsFormatted()
        {
            return GetLanguage($"{ModKey}_IsFormatted");
        }

        public static string Filters()
        {
            return GetLanguage($"{ModKey}_Filters");
        }

        public static string PowerOnOff()
        {
            return GetLanguage($"{ModKey}_PowerButton");
        }

        public static string Submit()
        {
            return Language.main.Get("Submit");
        }

        public static string RenameAlterraStorage()
        {
            return GetLanguage($"{ModKey}_RenameRemoteStorage");
        }

        public static string AutocrafterItemIsBeingCrafted()
        {
            return GetLanguage($"{ModKey}_AutocrafterItemIsBeingCrafted");
        }
    }
}
