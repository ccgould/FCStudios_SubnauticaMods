using FCS_AlterraHub.Core.Managers;
using FCS_AlterraHub.Models.Mono;
using FCS_EnergySolutions.ModItems.Buildables.PowerStorage.Mono.Base;
using FCS_EnergySolutions.ModItems.Buildables.TelepowerPylon.Model;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_EnergySolutions.Patches;

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
            var basePowerStorage = __instance.gameObject.EnsureComponent<BasePowerStorage>();
            basePowerStorage.SetHabitatManager(__instance.gameObject.GetComponent<HabitatManager>());
            var baseTPowerManager = __instance.gameObject.EnsureComponent<BaseTelepowerPylonManager>();
            BaseTelepowerPylonManager.RegisterPylonManager(baseTPowerManager);
            baseTPowerManager.SubRoot = __instance;
            baseTPowerManager.LoadSave();

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
            var baseTPowerManager = __instance.gameObject.GetComponent<BaseTelepowerPylonManager>();
            BaseTelepowerPylonManager.UnRegisterPylonManager(baseTPowerManager);

        }
    }
}
