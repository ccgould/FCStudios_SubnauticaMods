using FCS_EnergySolutions.WindSurferOperator.Buildables;
using HarmonyLib;
using UnityEngine;

namespace FCS_EnergySolutions.Patches
{
    [HarmonyPatch(typeof(BasePowerRelay),nameof(BasePowerRelay.GetConnectPoint))]
    internal static class BasePowerRelayGetConnectPoint_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(BasePowerRelay __instance,ref Vector3 __result)
        {
            if (__instance?.gameObject?.GetComponent<WindSurferOperatorSubroot>() == null)
            {
                return true;
            }

            __result = __instance?.gameObject?.transform?.position ?? Vector3.zero;
            return false;

        }
    }
}
