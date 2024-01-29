using FCS_AlterraHub.Core.Managers;
using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Core.Patches;

[HarmonyPatch(typeof(SubRoot))]
[HarmonyPatch("Awake")]
internal class SubRoot_Awake
{
    [HarmonyPostfix]
    public static void Postfix(ref SubRoot __instance)
    {
        QuickLogger.Debug($"Awake Called on {__instance.gameObject.GetComponent<PrefabIdentifier>()?.id}");
        QuickLogger.Debug($"Awake Called on {__instance.gameObject.activeSelf} | {__instance.gameObject.transform.position != Vector3.zero}");
        if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
        {
            var vehDocking = __instance.gameObject.EnsureComponent<VehicleDockingBayManager>();
            var habitatManager = __instance.gameObject.EnsureComponent<HabitatManager>();
            var portManager = __instance.gameObject.EnsureComponent<PortManager>();
            portManager.Manager = habitatManager;
            habitatManager.SetPortManager(portManager);
            habitatManager.SetSubRoot(__instance);
            habitatManager.Initialize();
            vehDocking.Initialize(habitatManager);
        }
    }
}

//[HarmonyPatch(typeof(SubRoot))]
//[HarmonyPatch("OnKill")]
//internal class SubRoot_OnKill
//{
//    [HarmonyPostfix]
//    public static void Postfix(ref SubRoot __instance)
//    {
//        QuickLogger.Debug("On Kill", true);
//        //var manager = BaseManager.FindManager(__instance);
//    }
//}
