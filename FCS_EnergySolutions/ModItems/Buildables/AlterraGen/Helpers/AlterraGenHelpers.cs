using FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Enumerators;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;


namespace FCS_EnergySolutions.ModItems.Buildables.AlterraGen.Helpers;
internal static class AlterraGenHelpers
{
    private static Dictionary<TechType, float> _vanillaBioChargeValues;

    internal static Dictionary<TechType, float> GetBioChargeValues()
    {
        if (_vanillaBioChargeValues == null)
        {
            Type baseBioReactorType = typeof(BaseBioReactor);
            _vanillaBioChargeValues = (Dictionary<TechType, float>)AccessTools.Field(baseBioReactorType, "charge").GetValue(baseBioReactorType);
        }

        if (_vanillaBioChargeValues == null)
        {
            QuickLogger.Error("Failed to get vanilla bio charge values using stored values");
        }

        return _vanillaBioChargeValues ?? FuelTypes.Charge;
    }

    internal static List<TechType> AllowedBioItems()
    {
        var buffer = new List<TechType>();
        foreach (KeyValuePair<TechType, float> bioChargeValue in GetBioChargeValues())
        {
            buffer.Add(bioChargeValue.Key);
        }

        return buffer;
    }
}
