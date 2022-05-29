using FCS_HomeSolutions.Buildables;
using HarmonyLib;
using UnityEngine;

namespace FCS_HomeSolutions.Patches
{
    [HarmonyPatch(typeof(BaseDeconstructable))]
    [HarmonyPatch("Awake")]
    class BaseDecontructable_Patches
    {
        [HarmonyPostfix]
        public static void Postfix(ref BaseDeconstructable __instance)
        {
            if(!QPatch.Configuration.IsWallPartitionsEnabled) return;
            if (__instance.gameObject.activeSelf && __instance.gameObject.transform.position != Vector3.zero)
            {
                if (__instance.gameObject.name.Equals("BaseRoom(Clone)"))
                {
                    var item = GameObject.Instantiate(ModelPrefab.GetPrefab("WallPlacements"));
                    item.transform.SetParent(__instance.gameObject.transform,false);
                    item.transform.localPosition = new Vector3(0f, -1.5f, 0f);
                }
            }
        }
    }
}
