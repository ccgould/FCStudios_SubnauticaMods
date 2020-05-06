using System;
using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Buildables
{
    internal static class AuxPatchers
    {
        private const string ModKey = "DSS";

        private static readonly Dictionary<string,string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Take","Take"},
            { $"{ModKey}_AmountOfItems","{0} Items."},
            { $"{ModKey}_HasItemsMessage","Server rack is not empty."},
            { $"{ModKey}_NotOnBase","NOT CONNECTED TO A BASE CANNOT OPERATE."},
            { $"{ModKey}_RackFull","This rack cannot hold anymore items."},
            { $"{ModKey}_CannotBeStored","This base cannot hold this item. The drives may be full, there are no  drives or they are formatted."},
            { $"{ModKey}_GoToHome","Go to home."},
            { $"{ModKey}_ColorPage","Go to color page."},
            { $"{ModKey}_NextPage","Next Page"},
            { $"{ModKey}_PrevPage","Previous Page"},
            { $"{ModKey}_OpenServerRack","Open Server Rack"},
            { $"{ModKey}_NoFoodItems","Cannot store food that decay. Please use a seabreeze or another container to store food items that can decay."},
            { $"{ModKey}_CloseServerRack","Close Server Rack"},
            { $"{ModKey}_DriveReceptacle","Drive Receptacle"},
            { $"{ModKey}_AddItemsToBase","Add Items to Base"},
            { $"{ModKey}_BaseDumpReceptacle","Base Dump Receptacle"},
            { $"{ModKey}_AddFilterItem","Item Filters"},
            { $"{ModKey}_AddCategoryItem","Category Filters"},
            { $"{ModKey}_RemoveServer","Remove Server"},
            { $"{ModKey}_InsertServer","Insert Server"},
            { $"{ModKey}_AddServer","Add Server To Rack"},
            { $"{ModKey}_NoAntennaAttached","No Antenna Attached To Base."},
            { $"{ModKey}_ColorPageFormat","Go to {0} color page."},
            { $"{ModKey}_Antenna","antenna"},
            { $"{ModKey}_Terminal","terminal"},
            { $"{ModKey}_Rename","Rename this base."},
            { $"{ModKey}_ColorPageDesc","Rename this base."},
            { $"{ModKey}_PowerUsage","Power Usage"},
            { $"{ModKey}_GettingData","Please Wait Retrieving {0} data."},
            { $"{ModKey}_IsFormattedFormat","Is Formatted: {0}."},
            { $"{ModKey}_Home","HOME"},
            { $"{ModKey}_FailedToGetBaseName","[Failed To Get Base Name.]"},
            { $"{ModKey}_CyclopsAntennaMessage","Cannot change the cyclops antenna color."},
            { $"{ModKey}_SearchForItemsMessage","Click to search for items..."},
            { $"{ModKey}_TakeFormatted","Take {0}"},
            { $"{ModKey}_SettingsPage","Go to settings page."},
            { $"{ModKey}_PowerBTN","Turn ON/OFF"},
            { $"{ModKey}_PoweredOff","POWERED OFF"},
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }
        
        internal static string Take()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(0));
        }

        internal static string AmountOfItems()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(1));
        }

        internal static string HasItemsMessage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(2));
        }
        
        internal static string NotOnBase()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(3));
        }

        internal static string RackFull()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(4));
        }
        
        internal static string CannotBeStored()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(5));
        }

        internal static string GoToHome()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(6));
        }

        internal static string ColorPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(7));
        }

        internal static string NextPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(8));
        }

        internal static string PrevPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(9));
        }

        internal static string OpenServerRackPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(10));
        }

        internal static string NoFoodItems()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(11));
        }

        internal static string CloseServerRackPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(12));
        }

        internal static string DriveReceptacle()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(13));
        }

        internal static string NotAllowed()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }

        internal static string DumpToBase()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(14));
        }

        internal static string BaseDumpReceptacle()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(15));
        }

        internal static string TakeServer()
        {
            return Language.main.Get("PickUpFormat");
        }

        internal static string AddFilterItem()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(16));
        }

        internal static string AddFilterCategory()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(17));
        }

        internal static string RemoveServer()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(18));
        }
        
        internal static string InsertServer()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(19));
        }

        internal static string Submit()
        { 
            return Language.main.Get("Submit");
        }

        internal static string AddServer()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(20));
        }
        
        internal static string NoAntennaOnBase()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(21));
        }

        internal static string ColorPageFormat()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(22));
        }

        internal static string Antenna()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(23));
        }

        internal static string Terminal()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(24));
        }

        internal static string Rename()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(25));
        }

        internal static string ColorMainPageDesc()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(26));
        }

        internal static string PowerUsage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(27));
        }

        internal static string GettingData()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(28));
        }

        internal static string FiltersCheckFormat()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(29));
        }

        internal static string Home()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(30));
        }

        internal static string FailedToGetBaseName()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(31));
        }

        internal static string CannotChangeCyclopsAntenna()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(32));
        }

        internal static string SearchForItemsMessage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(33));
        }

        internal static string TakeFormatted()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(34));
        }

        public static string SettingPage()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(35));
        }

        public static string PowerButton()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(36));
        }

        public static string PoweredOff()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(37));
        }
    }
}
