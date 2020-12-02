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

        private static string GetLanguage(string key)
        {
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(LanguageDictionary[key]) : string.Empty;
        }

        internal static string Stop()
        {
            return GetLanguage( $"{ModKey}_Stop");
        }
        
        internal static string Scan()
        {
            return GetLanguage($"{ModKey}_Scan");
        }
    }
}
