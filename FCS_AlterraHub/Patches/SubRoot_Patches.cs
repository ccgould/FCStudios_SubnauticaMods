using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(SubRoot))]
    [HarmonyPatch("Awake")]
    internal class SubRoot_Awake
    {
        [HarmonyPostfix]
        public static void Postfix(ref SubRoot __instance)
        {
            if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
            {
               BaseManager.FindManager(__instance);
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
            QuickLogger.Debug("On Kill",true);
            //var manager = BaseManager.FindManager(__instance);
        }
    }
}
