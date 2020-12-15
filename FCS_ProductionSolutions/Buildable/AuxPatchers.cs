using System;
using System.Collections.Generic;
using FCSCommon.Converters;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.Buildable
{
    internal static class AuxPatchers
    {
        private const string ModKey = "APS";

        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Scan","SCAN"},
            { $"{ModKey}_Stop","STOP"},
            { $"{ModKey}_PressKeyToOperate","Press {0} to activate or deactivate {1}."},
            { $"{ModKey}_ToggleLight","Light Toggle"},
            { $"{ModKey}_ToggleLightDesc","Click to turn the harvester light on and off"},
            { $"{ModKey}_HHBackButton","Back"},
            { $"{ModKey}_HHBackButtonDesc","Go back to the Hydroponic Harvester home screen"},
            { $"{ModKey}_PowerUsagePerSecondFormat","Power Usage Per Second : {0}"},
            { $"{ModKey}_GenerationTimeFormat","Generation Time : {0}"},
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
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(key) : "N/A";
        }

        internal static string Stop()
        {
            return GetLanguage( $"{ModKey}_Stop");
        }
        
        internal static string Scan()
        {
            return GetLanguage($"{ModKey}_Scan");
        }

        public static string InventoryFull()
        {
            return Language.main.Get("InventoryFull");
        }

        internal static string HarvesterBackButtonDesc()
        {
            return GetLanguage($"{ModKey}_HHBackButtonDesc");
        }

        internal static string HarvesterBackButton()
        {
            return GetLanguage($"{ModKey}_HHBackButton");
        }

        public static string PressKeyToOperate(string buttonName,string modName)
        {
            return String.Format(GetLanguage($"{ModKey}_PressKeyToOperate"),buttonName,modName);
        }

        public static string HarvesterToggleLight()
        {
            return GetLanguage($"{ModKey}_ToggleLight");
        }

        public static string HarvesterToggleLightDesc()
        {
            return GetLanguage($"{ModKey}_ToggleLightDesc");
        }

        public static string PowerUsagePerSecondFormat(float amount)
        {
            return string.Format(GetLanguage($"{ModKey}_PowerUsagePerSecondFormat"),amount);
        }
        public static string GenerationTimeFormat(float amount)
        {
            return string.Format(GetLanguage($"{ModKey}_GenerationTimeFormat"), TimeConverters.SecondsToHMS(amount));
        }
    }
}
