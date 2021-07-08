using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model.Converters;
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
            { $"{ModKey}_PressKeyToOperate","Press {0} to activate or deactivate {1}."},
            { $"{ModKey}_ToggleLight","Light Toggle"},
            { $"{ModKey}_ToggleLightDesc","Click to turn the harvester light on and off"},
            { $"{ModKey}_HHBackButton","Back"},
            { $"{ModKey}_HHBackButtonDesc","Go back to the Hydroponic Harvester home screen"},
            { $"{ModKey}_PowerUsagePerSecondFormat","Power Usage Per Second : {0}"},
            { $"{ModKey}_GenerationTimeFormat","Generation Time : {0}"},
            { $"{ModKey}_MatterAnalyzerHasItems","Please cancel the scan to deconstruct the matter analyzer."},
            { $"{ModKey}_PleaseClearReplicatorSlot","Please clear the replicator slot before trying to replicate another item."},
            { $"{ModKey}_PleaseEmptyHarvesterSlot","Harvester slot not empty. Please remove all items before trying to clear."},
            { $"{ModKey}_PleaseClearHarvesterSlot","Please clear the harvester slot before trying to select another item."},
            { $"{ModKey}_NotBuildOnBase","Base not found. Please build on/in a base"},
            { $"{ModKey}_HarvesterSpeedToggle","Speed Switch"},
            { $"{ModKey}_HarvesterSpeedToggleDesc","Changes the rate of sample replication. Stages: OFF|MIN|LOW|HIGH|MAX"},
            { $"{ModKey}_HarvesterAddSample","Sample Selector Page"},
            { $"{ModKey}_HarvesterAddSampleDesc","Opens the dialog that allows you to choose a sample for this slot"},
            { $"{ModKey}_HarvesterDeleteSample","Remove Sample"},
            { $"{ModKey}_HarvesterDeleteSampleDesc","Clears this slot. Slot must be empty to clear."},
            { $"{ModKey}_AutocrafterItemIsBeingCrafted","Cannot deconstruct because AutoCrafter is currently crafting an item."},
            { $"{ModKey}_AutocrafterItemsOnBelt","Cannot deconstruct because there are items on the belt."},
            { $"{ModKey}_CannotSetStandByHasConnections","Standby cannot be set due to this crafter already having connections to other crafters:{0}"},

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

        public static string GenerationTimeMinutesOnlyFormat(float amount)
        {
            return string.Format(GetLanguage($"{ModKey}_GenerationTimeFormat"), TimeConverters.SecondsToMS(amount));
        }

        public static string MatterAnalyzerHasItems()
        {
            return GetLanguage($"{ModKey}_MatterAnalyzerHasItems");
        }

        public static string PleaseClearReplicatorSlot()
        {
            return GetLanguage($"{ModKey}_PleaseClearReplicatorSlot");
        }

        public static string NotBuildOnBase()
        {
            return GetLanguage($"{ModKey}_NotBuildOnBase");
        }

        public static string HarvesterSpeedToggle()
        {
            return GetLanguage($"{ModKey}_HarvesterSpeedToggle");
        }

        public static string HarvesterSpeedToggleDesc()
        {
            return GetLanguage($"{ModKey}_HarvesterSpeedToggleDesc");
        }

        public static string HarvesterAddSample()
        {
            return GetLanguage($"{ModKey}_HarvesterAddSample");
        }

        public static string HarvesterAddSampleDesc()
        {
            return GetLanguage($"{ModKey}_HarvesterAddSampleDesc");
        }

        public static string HarvesterDeleteSample()
        {
            return GetLanguage($"{ModKey}_HarvesterDeleteSample");
        }

        public static string HarvesterDeleteSampleDesc()
        {
            return GetLanguage($"{ModKey}_HarvesterDeleteSampleDesc");
        }

        public static string AutocrafterItemIsBeingCrafted()
        {
            return GetLanguage($"{ModKey}_AutocrafterItemIsBeingCrafted");
        }

        public static string AutocrafterItemsOnBelt()
        {
            return GetLanguage($"{ModKey}_AutocrafterItemsOnBelt");
        }

        public static string CannotSetStandByHasConnections(string otherCrafterId)
        {
            return string.Format(GetLanguage($"{ModKey}_CannotSetStandByHasConnections"),otherCrafterId);
        }

        public static string ClickToEdit()
        {
            return Language.main.Get("SubmarineNameEditLabel");
        }

        public static string PleaseClearHarvesterSlot()
        {
            return GetLanguage($"{ModKey}_PleaseClearHarvesterSlot");
        }

        public static string PleaseEmptyHarvesterSlot()
        {
            throw new NotImplementedException();
        }
    }
}
