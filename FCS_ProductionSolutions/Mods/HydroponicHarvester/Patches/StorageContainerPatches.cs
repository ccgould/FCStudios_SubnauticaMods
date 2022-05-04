using System;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Buildable;
using HarmonyLib;

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Patches
{
    [HarmonyPatch(typeof(StorageContainer))]
    
    public class StorageContainer_Patches
    {
        [HarmonyPatch("OnHandHover")]
        [HarmonyPrefix]
        public static bool OnHandHover_Prefix(StorageContainer __instance, ref GUIHand hand)
        {
            //return false to skip execution of the original.
            if (!QPatch.Configuration.IsHydroponicHarvesterEnabled) return true;
            return !__instance.gameObject.name.StartsWith(HydroponicHarvesterPatch.HydroponicHarvesterModClassName, StringComparison.OrdinalIgnoreCase);
        }

        [HarmonyPatch("OnHandClick")]
        [HarmonyPrefix]
        public static bool OnHandClick_Prefix(StorageContainer __instance, ref GUIHand guiHand)
        {
            //return false to skip execution of the original.
            if (!QPatch.Configuration.IsHydroponicHarvesterEnabled) return true;
            return !__instance.gameObject.name.StartsWith(HydroponicHarvesterPatch.HydroponicHarvesterModClassName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
