using System.Collections.Generic;
using SMLHelper.V2.Handlers;

namespace FCS_ProductionSolutions.Buildable
{
    internal static class AuxPatchers
    {
        private const string ModKey = "APS";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Scan","SCAN"},
            { $"{ModKey}_Stop","STOP"},
        };

        internal static void AdditionalPatching()
        {
            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }



        internal static string Stop()
        {
            return Language.main.Get($"{ModKey}_Stop");
        }
        
        internal static string Scan()
        {
            return Language.main.Get($"{ModKey}_Scan");
        }
    }
}
