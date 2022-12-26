using System;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_HomeSolutions.Mods.SeaBreeze.Patches
{
    [HarmonyPatch(typeof(StorageContainer))]
    
    public class StorageContainer_Patches
    {
        [HarmonyPatch("OnHandHover")]
        [HarmonyPrefix]
        public static bool OnHandHover_Prefix(StorageContainer __instance, ref GUIHand hand)
        {
            //return false to skip execution of the original.
            if (!Main.Configuration.IsSeaBreezeEnabled) return true;
            return !__instance.gameObject.name.StartsWith(SeaBreezeBuildable.SeaBreezeClassID, StringComparison.OrdinalIgnoreCase);
        }

        [HarmonyPatch("OnHandClick")]
        [HarmonyPrefix]
        public static bool OnHandClick_Prefix(StorageContainer __instance, ref GUIHand guiHand)
        {
            //return false to skip execution of the original.
            if (!Main.Configuration.IsSeaBreezeEnabled) return true;
            return !__instance.gameObject.name.StartsWith(SeaBreezeBuildable.SeaBreezeClassID, StringComparison.OrdinalIgnoreCase);
        }
    }
}
