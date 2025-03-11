using FCS_AlterraHub.Models.Mono;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono.Service;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Patches;

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
            __instance.gameObject.EnsureComponent<BaseOxygenTankManager>();
        }
    }
}

[HarmonyPatch(typeof(SubRoot))]
[HarmonyPatch("OnKill")]
internal class SubRoot_OnKill
{
    [HarmonyPostfix]
    public static void Postfix(ref SubRoot __instance)
    {
        QuickLogger.Debug($"Awake Called on {__instance.gameObject.GetComponent<PrefabIdentifier>()?.id}");
        QuickLogger.Debug($"Awake Called on {__instance.gameObject.activeSelf} | {__instance.gameObject.transform.position != Vector3.zero}");
        if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
        {
            BaseOxygenTankService.main.UnRegisterBaseOxygenTankManager(__instance.gameObject.GetComponent<PrefabIdentifier>()?.id);

        }
    }
}
