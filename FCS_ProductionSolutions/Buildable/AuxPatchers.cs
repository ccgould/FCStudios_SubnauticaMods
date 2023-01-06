using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Model.Converters;
using SMLHelper.Handlers;

namespace FCS_ProductionSolutions.Buildable
{
    internal static class AuxPatchers
    {
        private const string ModKey = "APS";
        private static StringBuilder _sb = new StringBuilder();


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
            { $"{ModKey}_HarvesterDeleteSampleDesc","Clears this slot. (Slot must be empty to clear or long press to force clear.)"},
            { $"{ModKey}_AutocrafterItemIsBeingCrafted","Cannot deconstruct because AutoCrafter is currently crafting an item."},
            { $"{ModKey}_AutocrafterItemsOnBelt","Cannot deconstruct because there are items on the belt."},
            { $"{ModKey}_CannotSetStandByHasConnections","Standby cannot be set due to this crafter already having connections to other crafters:{0}"},
            { $"{ModKey}_FilterPageInformation","Filter: {0} | Black List: {1} | Enabled Filters Count : {2}"},
            { $"{ModKey}_ClearSlotLongPress","Are you sure you would like to clear this slot? (All items will be destroyed)"},
            { $"{ModKey}_AutocrafterStandbyTxt","Assist Mode Enabled. \n\nParent Crafter: {0}"},
            { $"{ModKey}_AutocrafterHoverInformation","Assist Mode Enabled: {0} | Continuous Mode: {1} | Is Limited {2} | {3}"},
            { $"{ModKey}_CraftAmount","Craft Amount: {0} | {1}"},

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
            var value = $"{ModKey}_{key}";
            return LanguageDictionary.ContainsKey(value) ? Language.main.Get(value) : "N/A";
        }

        internal static string Stop()
        {
            return GetLanguage( "Stop");
        }
        
        internal static string Scan()
        {
            return GetLanguage("Scan");
        }

        public static string InventoryFull()
        {
            return Language.main.Get("InventoryFull");
        }

        internal static string HarvesterBackButtonDesc()
        {
            return GetLanguage("HHBackButtonDesc");
        }

        internal static string HarvesterBackButton()
        {
            return GetLanguage("HHBackButton");
        }

        public static string PressKeyToOperate(string buttonName,string modName)
        {
            return String.Format(GetLanguage("PressKeyToOperate"),buttonName,modName);
        }

        public static string HarvesterToggleLight()
        {

             
            return GetLanguage("ToggleLight");
        }

        public static string HarvesterToggleLightDesc()
        {
            return GetLanguage("ToggleLightDesc");
        }

        public static string PowerUsagePerSecondFormat(float amount)
        {
            return string.Format(GetLanguage("PowerUsagePerSecondFormat"),amount);
        }
        public static string GenerationTimeFormat(float amount)
        {
            return string.Format(GetLanguage("GenerationTimeFormat"), TimeConverters.SecondsToHMS(amount));
        }

        public static string GenerationTimeMinutesOnlyFormat(float amount)
        {
            return string.Format(GetLanguage("GenerationTimeFormat"), TimeConverters.SecondsToMS(amount));
        }

        public static string MatterAnalyzerHasItems()
        {
            return GetLanguage("MatterAnalyzerHasItems");
        }

        public static string PleaseClearReplicatorSlot()
        {
            return GetLanguage("PleaseClearReplicatorSlot");
        }

        public static string NotBuildOnBase()
        {
            return GetLanguage("NotBuildOnBase");
        }

        public static string HarvesterSpeedToggle()
        {
            return GetLanguage("HarvesterSpeedToggle");
        }

        public static string HarvesterSpeedToggleDesc()
        {
            return GetLanguage("HarvesterSpeedToggleDesc");
        }

        public static string HarvesterAddSample()
        {
            return GetLanguage("HarvesterAddSample");
        }

        public static string HarvesterAddSampleDesc()
        {
            return GetLanguage("HarvesterAddSampleDesc");
        }

        public static string HarvesterDeleteSample()
        {
            return GetLanguage("HarvesterDeleteSample");
        }

        public static string HarvesterDeleteSampleDesc()
        {
            return GetLanguage("HarvesterDeleteSampleDesc");
        }

        public static string AutocrafterItemIsBeingCrafted()
        {
            return GetLanguage("AutocrafterItemIsBeingCrafted");
        }

        public static string AutocrafterItemsOnBelt()
        {
            return GetLanguage("AutocrafterItemsOnBelt");
        }

        public static string CannotSetStandByHasConnections(string otherCrafterId)
        {
            return string.Format(GetLanguage("CannotSetStandByHasConnections"),otherCrafterId);
        }

        public static string ClickToEdit()
        {
            return Language.main.Get("SubmarineNameEditLabel");
        }

        public static string PleaseClearHarvesterSlot()
        {
            return GetLanguage("PleaseClearHarvesterSlot");
        }
        
        public static string FilterPageInformation(bool focusToggleValue, bool blackListToggleValue, int focusCount)
        {
            return string.Format(GetLanguage("FilterPageInformation"), focusToggleValue ? "On" : "Off", blackListToggleValue ? "On" : "Off", focusCount);
        }

        public static string ClearSlotLongPress()
        {
            return GetLanguage("ClearSlotLongPress");

        }

        public static string StandbyTxt(string[] name)
        {

            _sb.Clear();
            for (int i = 0; i < name.Length; i++)
            {
                _sb.Append(name[i]);

                if (i != name.Length - 1)
                {
                    _sb.Append(",");
                }
            }

            return string.Format(GetLanguage("AutocrafterStandbyTxt"),_sb);
        }

        public static string AutocrafterHoverInformation(bool isStandBy, bool isRecursiveOperation, bool isLimitedOperation, Vector2int amount)
        {

            return string.Format(GetLanguage("AutocrafterHoverInformation"), isStandBy,isRecursiveOperation,isLimitedOperation, string.Format(GetLanguage("CraftAmount"), amount.x, amount.y));
        }
    }
}
