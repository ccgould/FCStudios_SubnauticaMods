using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(BaseWaterPlane))]
    [HarmonyPatch("Awake")]
    internal class WaterPlanePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref BaseWaterPlane __instance)
        {
            QuickLogger.Debug($"Awake Called on BaseWaterPlane: {__instance?.waterPlane?.name}");

            var waterPlane = __instance?.waterPlane?.gameObject;
            

            if (waterPlane is not null)
            {

                var material = waterPlane.GetComponent<MeshRenderer>()?.material;


                QuickLogger.Debug($"BaseWaterPlane material found: {material is not null}");


                MaterialHelpers._waterMaterial = material;

            }

        }
    }
}
