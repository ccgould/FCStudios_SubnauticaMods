using System;
using FCS_HomeSolutions.Mods.Replicator.Buildables;
using HarmonyLib;

namespace FCS_ProductionSolutions.Mods.Replicator.Patches
{
    [HarmonyPatch(typeof(StorageContainer))]
    
    public class StorageContainer_Patches
    {
        [HarmonyPatch("OnHandHover")]
        [HarmonyPrefix]
        public static bool OnHandHover_Prefix(StorageContainer __instance, ref GUIHand hand)
        {
            //return false to skip execution of the original.
            if (!Main.Configuration.IsReplicatorEnabled) return true;
            return !__instance.gameObject.name.StartsWith(ReplicatorBuildable.ReplicatorClassName, StringComparison.OrdinalIgnoreCase);
        }

        [HarmonyPatch("OnHandClick")]
        [HarmonyPrefix]
        public static bool OnHandClick_Prefix(StorageContainer __instance, ref GUIHand guiHand)
        {
            //return false to skip execution of the original.
            if (!Main.Configuration.IsReplicatorEnabled) return true;
            return !__instance.gameObject.name.StartsWith(ReplicatorBuildable.ReplicatorClassName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
