using FCS_AlterraHub.Core.Helpers;
using Nautilus.Handlers;
using System;
using System.Collections.Generic;

namespace FCS_ProductionSolutions.Configuration;
internal static class AuxPatchers
{
    private const string ModKey = "PS";


    private static string GetLanguage(string key)
    {
        return Language.main.Get(key);
    }

    internal static string PrevPage()
    {
        return GetLanguage($"{ModKey}_PrevPage");
    }

    internal static string NextPage()
    {
        return GetLanguage($"{ModKey}_NextPage");
    }

    public static string FilterPageInformation(bool focusToggleValue, bool blackListToggleValue, int focusCount)
    {
        return string.Format(GetLanguage("FilterPageInformation"), focusToggleValue ? "On" : "Off", blackListToggleValue ? "On" : "Off", focusCount);
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
        return string.Format(GetLanguage($"{ModKey}_OilTankNotEmptyFormat"), time);
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
        return string.Format(GetLanguage($"{ModKey}_InvalidFunctionFormat"), invalidFunction);
    }

    internal static string InvalidClassFormat(string invalidClass)
    {
        return string.Format(GetLanguage($"{ModKey}_InvalidClassFormat"), invalidClass);
    }

    internal static string IncorrectParameterFormat(string expected, string example)
    {
        return string.Format(GetLanguage($"{ModKey}_IncorrectParameterFormat"), expected, example);
    }

    internal static string IncorrectAmountOfParameterFormat(string expected, int amount)
    {
        return string.Format(GetLanguage($"{ModKey}_IncorrectAmountOfParameterFormat"), expected, amount);
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
        return string.Format(GetLanguage($"{ModKey}_BlackListDesc"), isEnabled);
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
        return string.Format(GetLanguage($"{ModKey}_InventoryStorageFormat"), total, capacity);
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
        return string.Format(GetLanguage($"{ModKey}_BaseIDErrorFormat"), baseID);
    }

    public static string BeaconSettingsButton()
    {
        return GetLanguage($"{ModKey}_DrillBeaconPage");
    }

    public static string NoLubricantFound()
    {
        return GetLanguage($"{ModKey}_NoLubricantFound");
    }

    internal static string FormatError(string errorCode)
    {
        return $"{ModKey}_{errorCode}";
    }

    public static string PowerUsagePerSecondFormat(float amount)
    {
        return string.Format(GetLanguage($"{ModKey}_PowerUsagePerSecondFormat"), amount);
    }
    public static string GenerationTimeFormat(float amount)
    {
        return string.Format(GetLanguage($"{ModKey}_GenerationTimeFormat"), TimeConverters.SecondsToHMS(amount));
    }
}
