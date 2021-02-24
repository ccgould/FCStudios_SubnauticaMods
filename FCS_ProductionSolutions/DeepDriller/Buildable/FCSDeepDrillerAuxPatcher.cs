using System.Collections.Generic;
using SMLHelper.V2.Handlers;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Buildable
{
    internal partial class FCSDeepDrillerBuildable
    {
        private const string ModKey = "DD";
        internal static string BuildableName { get; private set; }
        internal static TechType TechTypeID { get; private set; }
        
        private static readonly Dictionary<string, string> LanguageDictionary = new Dictionary<string, string>
        {
            { $"{ModKey}_Stop","STOP"},
            { $"{ModKey}_ItemNotAllowed","Cannot place this item in here."},
            { $"{ModKey}_OnlyPowercellsAllowed","Only powercells are allowed."},
            { $"{ModKey}_NextPage","Next Page"},
            { $"{ModKey}_PrevPage","Previous Page"},
            { $"{ModKey}_ItemsButton","Go to inventory page."},
            { $"{ModKey}_ProgrammingButton","Go to programming page."},
            { $"{ModKey}_SettingsButton","Go to settings page."},
            { $"{ModKey}_TakeFormatted","Take {0}"},
            { $"{ModKey}_OilTankNotEmptyFormat","Cannot add anymore lubricant at this time try again in {0} minutes"},
            { $"{ModKey}_OilTankDropContainerTitle","Lubricant Tank Dump Receptacle."},
            { $"{ModKey}_PowercellDumpContainerTitle","PowerCell Draining Receptacle."},
            { $"{ModKey}_AddProgramButton","Add a upgrade function to the drill."},
            { $"{ModKey}_InvalidFunctionFormat","Invalid Function: {0}. This function doesn't exist please check the documentation."},
            { $"{ModKey}_InvalidClassFormat","Invalid Class: {0}. This class doesn't exist please check the documentation."},
            { $"{ModKey}_IncorrectParameterFormat", "Incorrect type in parameter expected: {0} ex: ({1};) ."},
            { $"{ModKey}_IncorrectAmountOfParameterFormat", "Incorrect amount of parameters expected {0} got {1}."},
            { $"{ModKey}_NotOreErrorFormat", "TechType {0} is not an ore."},
            { $"{ModKey}_FilterButton", "Toggle Filters."},
            { $"{ModKey}_FilterButtonDesc", "With filters enabled the drill will only drill the checked items, with filters disabled all items in the list will be drilled."},
            { $"{ModKey}_ToggleRangeButtonToolTip", "Show/Hide Range"},
            { $"{ModKey}_ExportToggleButtonToolTip", "Toggle export to Alterra Storage."},
            { $"{ModKey}_AddLubricantButtonToolTip", "Add lubricant to drill."},
            { $"{ModKey}_AddPowerButtonToolTip", "Add power to drill."},
            { $"{ModKey}_PowercellDrainButtonToolTip", "Add power from draining powercells."},
            { $"{ModKey}_LubeRefillButtonToolTip", "Refill lubricant tank."},
            { $"{ModKey}_LubeRefillButtonDescToolTip", "Refill lubricant tank for the drill to use for operation."},
            { $"{ModKey}_AlterraStorageButtonToolTip", "Change Alterra Storage connection settings."},
            { $"{ModKey}_GoToHomeToolTip", "Go to home page."},
            { $"{ModKey}_GoToSettingsToolTip", "Go to settings page."},
            { $"{ModKey}_BlackListToggleToolTip", "Blacklist Toggle."},
            { $"{ModKey}_BlackListDesc", "While in whitelist mode all checked items will be allowed in the drill, while in blacklist mode checked items will not be allowed. Blacklist Enabled: {0}."},
            { $"{ModKey}_AlterraStorageToggleToolTip", "Alterra Storage toggle."},
            { $"{ModKey}_AlterraStorageToggleDesc", "Toggles whether  the driller exports its items to a nearby Alterra Storage."},
            { $"{ModKey}_AlterraStorageRangeToggleToolTip", "Alterra Storage Range Viewer Toggle."},
            { $"{ModKey}_AlterraStorageRangeToggleDesc", "Toggles the range ring that show the search radius for nearby Alterra Storage."},
            { $"{ModKey}_RemoveAllItems", "Deep Driller is not empty please remove all items to deconstruct."},
            { $"{ModKey}_ProgrammingAddBTNDesc", "Allows you to add more functionality to your drill with functions."},
            { $"{ModKey}_ProgrammingTemplateBTN", "Programming Templates."},
            { $"{ModKey}_ProgrammingTemplateBTNDesc", "Shows all available functions the drill can use."},
            { $"{ModKey}_OresPerDay", "Ores Per Day:"},
            { $"{ModKey}_PowerConsumption", "Power Consumption:"},
            { $"{ModKey}_BiomeFormat", "Current Biome: {0}"},
            { $"{ModKey}_InventoryStorageFormat", "INVENTORY: Items {0}/{1}"},
            { $"{ModKey}_Idle", "Idle"},
            { $"{ModKey}_Drilling", "Drilling"},
            { $"{ModKey}_NoPower", "No Power"},
            { $"{ModKey}_NeedsOil", "Lubricant Needed"},
            { $"{ModKey}_DrillDeactivated", "Drill Deactivated"},
            { $"{ModKey}_BaseIDErrorFormat", "Base {0} is either to far away or the ID is incorrect."},
        };


        private void AdditionalPatching()
        {
            BuildableName = this.FriendlyName;
            TechTypeID = this.TechType;

            foreach (KeyValuePair<string, string> languageEntry in LanguageDictionary)
            {
                LanguageHandler.SetLanguageLine(languageEntry.Key, languageEntry.Value);
            }
        }

        private static string GetLanguage(string key)
        {
            return LanguageDictionary.ContainsKey(key) ? Language.main.Get(key) : "N/A";
        }
        
        internal static string PrevPage()
        {
            return GetLanguage($"{ModKey}_PrevPage");
        }

        internal static string NextPage()
        {
            return GetLanguage($"{ModKey}_NextPage");
        }
        
        internal static string InventoryButton()
        {
            return GetLanguage($"{ModKey}_ItemsButton");
        }

        internal static string ProgrammingButton()
        {
            return GetLanguage($"{ModKey}_ProgrammingButton");
        }

        internal static string SettingsButton()
        {
            return GetLanguage($"{ModKey}_GoToSettingsToolTip");
        }

        internal static string TakeFormatted(string item)
        {
            return string.Format(GetLanguage($"{ModKey}_TakeFormatted"), item);
        }

        internal static string StorageFull()
        {
            return Language.main.Get("InventoryFull");
        }

        internal static string OilTankNotFormatEmpty(string time)
        {
            return string.Format(GetLanguage($"{ModKey}_OilTankNotEmptyFormat"),time);
        }

        internal static string OilDropContainerTitle()
        {
            return GetLanguage($"{ModKey}_OilTankDropContainerTitle");
        }

        internal static string NotAllowedItem()
        {
            return Language.main.Get("TimeCapsuleItemNotAllowed");
        }

        internal static string PowercellDumpContainerTitle()
        {
            return GetLanguage($"{ModKey}_PowercellDumpContainerTitle");
        }

        internal static string AddProgramButton()
        {
            return GetLanguage($"{ModKey}_AddProgramButton");
        }

        internal static string InvalidFunctionFormat(string invalidFunction)
        {
            return string.Format(GetLanguage($"{ModKey}_InvalidFunctionFormat"),invalidFunction);
        }
        
        internal static string InvalidClassFormat(string invalidClass)
        {
            return string.Format(GetLanguage($"{ModKey}_InvalidClassFormat"), invalidClass);
        }

        internal static string IncorrectParameterFormat(string expected,string example)
        {
            return string.Format(GetLanguage($"{ModKey}_IncorrectParameterFormat"), expected,example);
        }

        internal static string IncorrectAmountOfParameterFormat(string expected, int amount)
        {
            return string.Format(GetLanguage($"{ModKey}_IncorrectAmountOfParameterFormat"),expected, amount);
        }

        internal static string NotOreErrorFormat(string invalidOreTechType)
        {
            return string.Format(GetLanguage($"{ModKey}_NotOreErrorFormat"), invalidOreTechType);
        }

        internal static string FilterButtonDesc()
        {
            return GetLanguage($"{ModKey}_FilterButtonDesc");
        }

        internal static string FilterButton()
        {
            return GetLanguage($"{ModKey}_FilterButton");
        }

        public static string PowercellDrainButton()
        {
            return GetLanguage($"{ModKey}_PowercellDrainButtonToolTip");
        }

        public static string LubeRefillButton()
        {
            return GetLanguage($"{ModKey}_LubeRefillButtonToolTip");
        }

        public static string LubeRefillButtonDesc()
        {
            return GetLanguage($"{ModKey}_LubeRefillButtonDescToolTip");
        }

        public static string AlterraStorageButton()
        {
            return GetLanguage($"{ModKey}_AlterraStorageButtonToolTip");
        }

        public static string GoToHome()
        {
            return GetLanguage($"{ModKey}_GoToHomeToolTip");
        }

        public static string GoToSettings()
        {
            return GetLanguage($"{ModKey}_GoToSettingsToolTip");
        }

        public static string BlackListToggle()
        {
            return GetLanguage($"{ModKey}_BlackListToggleToolTip");
        }

        public static string BlackListToggleDesc(bool isEnabled)
        {
            return string.Format(GetLanguage($"{ModKey}_BlackListDesc"),isEnabled);
        }

        public static string AlterraStorageToggle()
        {
            return GetLanguage($"{ModKey}_AlterraStorageToggleToolTip");
        }

        public static string AlterraStorageToggleDesc()
        {
            return GetLanguage($"{ModKey}_AlterraStorageToggleDesc");
        }

        public static string AlterraStorageRangeToggle()
        {
            return GetLanguage($"{ModKey}_AlterraStorageRangeToggleToolTip");
        }

        public static string AlterraStorageRangeToggleDesc()
        {
            return GetLanguage($"{ModKey}_AlterraStorageRangeToggleDesc");
        }

        public static string RemoveAllItems()
        {
            return GetLanguage($"{ModKey}_RemoveAllItems");
        }

        public static string AddProgramButtonDec()
        {
            return GetLanguage($"{ModKey}_ProgrammingAddBTNDesc");
        }

        public static string ProgrammingTemplateButton()
        {
            return GetLanguage($"{ModKey}_ProgrammingTemplateBTN");
        }

        public static string ProgrammingTemplateButtonDesc()
        {
            return GetLanguage($"{ModKey}_ProgrammingTemplateBTNDesc");
        }

        public static string OresPerDay()
        {
            return GetLanguage($"{ModKey}_OresPerDay");
        }        
        
        public static string PowerConsumption()
        {
            return GetLanguage($"{ModKey}_PowerConsumption");
        }

        public static string BiomeFormat(string biome)
        {
            return string.Format(GetLanguage($"{ModKey}_BiomeFormat"), biome);
        }

        public static string InventoryStorageFormat(int total, int capacity)
        {
            return string.Format(GetLanguage($"{ModKey}_InventoryStorageFormat"), total,capacity);
        }

        public static string NeedsOil()
        {
            return GetLanguage($"{ModKey}_NeedsOil");
        }

        public static string Idle()
        {
            return GetLanguage($"{ModKey}_Idle");
        }

        public static string Drilling()
        {
            return GetLanguage($"{ModKey}_Drilling");
        }

        public static string NoPower()
        {
            return GetLanguage($"{ModKey}_NoPower");
        }

        public static string DrillDeactivated()
        {
            return GetLanguage($"{ModKey}_DrillDeactivated");
        }

        public static string BaseIDErrorFormat(string baseID)
        {
            return string.Format(GetLanguage($"{ModKey}_BaseIDErrorFormat"),baseID);
        }
    }
}
