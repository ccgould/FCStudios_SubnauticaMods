using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Mods.JukeBox.Mono;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
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
                if (Main.Configuration.IsJukeBoxEnabled)
                {
                    var baseJukeBox = __instance.gameObject.EnsureComponent<BaseJukeBox>();
                    baseJukeBox.BaseManager = BaseManager.FindManager(__instance);

                }
            }
        }
    }
}
