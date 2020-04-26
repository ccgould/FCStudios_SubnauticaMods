using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using UnityEngine;

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
            { $"{ModKey}_DriveFull","This drive cannot hold anymore items."},
            { $"{ModKey}_Home","Go to home."},
            { $"{ModKey}_ColorPage","Go to color page."},
            { $"{ModKey}_NextPage","Next Page"},
            { $"{ModKey}_PrevPage","Previous Page"},
            { $"{ModKey}_OpenServerRack","Open Server Rack"},
            { $"{ModKey}_NoFoodItems","Please use a seabreeze or another container to store food items"},
            { $"{ModKey}_CloseServerRack","Close Server Rack"},
            { $"{ModKey}_DriveReceptacle","Drive Receptacle"},
            { $"{ModKey}_AddItemsToBase","Add Items to Base"},
            { $"{ModKey}_BaseDumpReceptacle","Base Dump Receptacle"},
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
        
        internal static string DriveFull()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(5));
        }

        internal static string Home()
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

        public static string DumpToBase()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(14));
        }

        public static string BaseDumpReceptacle()
        {
            return Language.main.Get(LanguageDictionary.Keys.ElementAt(15));
        }

        public static string TakeServer()
        {
            return Language.main.Get("PickUpFormat");
        }
    }
}
