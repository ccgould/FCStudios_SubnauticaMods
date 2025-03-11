using FCS_AlterraHub.Models.Abstract;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_ProductionSolutions.ModItems.Buildables.HydroponicHarvester.Patcher;

[HarmonyPatch(typeof(FruitPlant))]
[HarmonyPatch("OnGrown")]
internal class CreepvineSeed_Patch
{
    //[HarmonyPostfix]
    //public static void Postfix(ref FruitPlant __instance)
    //{
    //    QuickLogger.Debug("GrownPLant Found", true);
    //    QuickLogger.Debug($"Found FCSDevice Found {__instance.gameObject.GetComponentInParent<FCSDevice>()}", true);

    //    if (__instance.gameObject.GetComponentInChildren<Light>())
    //    {
    //        QuickLogger.Debug("GrownPLant Found", true);
    //        //var light = __instance.GetComponentInChildren<Light>();

    //        //UnityEngine.Object.Destroy(light.gameObject)
    //    }
    //}
}
