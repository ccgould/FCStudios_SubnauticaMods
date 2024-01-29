using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCS_EnergySolutions.Configuration;
internal static class AuxPatchers
{
    private const string ModKey = "AES";


    private static string GetLanguage(string key)
    {
        var newKey = $"{ModKey}_{key}";
        return Language.main.Get(newKey);
    }

    internal static string JetStreamOnHover()
    {
        return GetLanguage("JetStreamT242OnHover");
    }

    public static string JetStreamCurrentStateFormatted( string state)
    {
        return string.Format(GetLanguage("JetStreamCurrentStateFormatted"), state);
    }


    public static string UniversalChargerNotEmpty()
    {
        return GetLanguage("UniversalChargerNotEmpty");
    }

    public static string UniversalChargerCannotChangeMode()
    {
        return GetLanguage("UniversalChargerCannotChangeMode");
    }

    public static string NotUnderWater()
    {
        return GetLanguage("NotUnderWater");
    }

    public static string NotUnderWaterDesc()
    {
        return GetLanguage("NotUnderWaterDesc");
    }

    public static string UniversalChargerSwitchMode()
    {
        return GetLanguage("UniversalChargerSwitchMode");
    }

    public static string UniversalChargerSwitchModeDesc()
    {
        return GetLanguage("UniversalChargerSwitchModeDesc");
    }

    public static string NotOnPlatform()
    {
        return GetLanguage("NotOnPlatform");
    }

    public static string SolarClusterHover(int sun, int charge, int capacity, int produce)
    {
        return string.Format(GetLanguage("SolarClusterHover"), sun, charge, capacity, produce);
    }

    public static string RemoveAllTelepowerConnections()
    {
        return GetLanguage("RemoveAllTelepowerConnections");
    }

    public static string RemoveAllTelepowerConnectionsPush()
    {
        return GetLanguage("RemoveAllTelepowerConnectionsPush");
    }

    public static string RemoveAllTelepowerConnectionsPull()
    {
        return GetLanguage("RemoveAllTelepowerConnectionsPull");
    }

    public static string MaximumConnectionsReached()
    {
        return GetLanguage("MaximumConnectionsReached");
    }

    public static string WindSurferOnHover()
    {
        return GetLanguage("WindSurferOnHover");
    }

    public static string SelectAMode()
    {
        return GetLanguage("SelectAMode");
    }

    public static string WindSurferMaxReached()
    {
        return GetLanguage("WindSurferMaxReached");
    }
}
