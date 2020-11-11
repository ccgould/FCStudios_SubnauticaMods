using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine.UI;

namespace FCS_HomeSolutions.Configuration
{
    internal static class AuxPatchers
    {
        private const string ModKey = "AHS";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Max","MAX"},
            { $"{ModKey}_High","HIGH"},
            { $"{ModKey}_Min","MIN"},
            { $"{ModKey}_Low","LOW"},
            { $"{ModKey}_GatesOpenMessage","Gates are open cannot move."},
            { $"{ModKey}_Save","SAVE"},
            { $"{ModKey}_SaveLevel","Save Current Level"},
            { $"{ModKey}_GoingToLevelFormat","Going to floor {0}"},
            { $"{ModKey}_FloorSelect","Selecting Floor: {0}"},
            { $"{ModKey}_HoverPadInOperation","Hover Lift Pad is in operation please unlock any prawn suit or yourself."},
            { $"{ModKey}_UnDockPrawnSuitToEnter","Undock prawn suit to enter."},
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        internal static string Max()
        {
            return Language.main.Get($"{ModKey}_Max");
        }

        internal static string High()
        {
            return Language.main.Get($"{ModKey}_High");
        }

        internal static string Min()
        {
            return Language.main.Get($"{ModKey}_Min");
        }

        internal static string Low()
        {
            return Language.main.Get($"{ModKey}_Low");
        }

        public static string GatesOpenMessage()
        {
            return Language.main.Get($"{ModKey}_GatesOpenMessage");
        }

        public static string Save()
        {
            return Language.main.Get($"{ModKey}_Save");
        }

        public static string SaveLevel()
        {
            return Language.main.Get($"{ModKey}_SaveLevel");
        }

        public static string GoingToLevelFormat(string levelName)
        {
            return string.Format(Language.main.Get($"{ModKey}_GoingToLevelFormat"),levelName);
        }

        public static string FloorSelect(string levelName)
        {
            return string.Format(Language.main.Get($"{ModKey}_FloorSelect"),levelName);
        }

        public static string HoverPadInOperation()
        {
            return Language.main.Get($"{ModKey}_HoverPadInOperation");
        }

        public static string UnDockPrawnSuitToEnter()
        {
            return Language.main.Get($"{ModKey}_UnDockPrawnSuitToEnter");
        }
    }
}
