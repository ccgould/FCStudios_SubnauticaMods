using FCS_EnergySolutions.Mods.PowerStorage.Mono;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_EnergySolutions.Patches
{
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
                if (QPatch.Configuration.IsPowerStorageEnabled)
                {
                    __instance.gameObject.AddComponent<BasePowerStorage>();
                }

                if (QPatch.Configuration.IsTelepowerPylonEnabled)
                {
                   var baseTPowerManager = __instance.gameObject.EnsureComponent<BaseTelepowerPylonManager>();
                   baseTPowerManager.SubRoot = __instance;
                   BaseTelepowerPylonManager.RegisterPylonManager(baseTPowerManager);
                }
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

            if (QPatch.Configuration.IsTelepowerPylonEnabled)
            {
                var baseTPowerManager = __instance.gameObject.GetComponent<BaseTelepowerPylonManager>();
                BaseTelepowerPylonManager.UnRegisterPylonManager(baseTPowerManager);
            }

            QuickLogger.Debug("On Kill",true);
            //var manager = BaseManager.FindManager(__instance);
        }
    }
}
