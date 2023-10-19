using FCS_AlterraHub.Models.Mono;
using FCS_StorageSolutions.Models;
using HarmonyLib;
using UnityEngine;

namespace FCS_StorageSolutions.Patches;

[HarmonyPatch(typeof(SubRoot))]
[HarmonyPatch("Awake")]
internal class SubRoot_Awake
{
    [HarmonyPostfix]
    public static void Postfix(ref SubRoot __instance)
    {
        if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
        {
           __instance.gameObject.EnsureComponent<DSSManager>();
        }
    }
}

[HarmonyPatch(typeof(SubRoot))]
[HarmonyPatch("Start")]
internal class SubRoot_Start
{
    [HarmonyPostfix]
    public static void Postfix(ref SubRoot __instance)
    {
        if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
        {
            var manager = __instance.gameObject.GetComponent<DSSManager>();
            manager.Initialize(__instance.gameObject.GetComponent<HabitatManager>());
        }
    }
}
