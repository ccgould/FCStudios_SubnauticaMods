using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_EnergySolutions.Patches
{

    [HarmonyPatch(typeof(Battery))]
    [HarmonyPatch("OnAfterDeserialize")]
    internal class Battery_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Battery __instance)
        {
            if (__instance == null || !__instance.gameObject.name.StartsWith("PowerStorageCell"))
            {
                return true;
            }

            return false;
        }
    }
}
