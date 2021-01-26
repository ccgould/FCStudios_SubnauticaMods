using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    class uGUI_PowerIndicator_Initialize_Patch
    {
        private static FCSHUD tracker;

        private static void Postfix(uGUI_PowerIndicator __instance)
        {

            if (IndicatorInstance != null) return;

            if (Inventory.main == null)
            {
                return;
            }
            var prefab = GameObject.Instantiate(AlterraHub.MissionMessageBoxPrefab);
            var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");
            prefab.transform.SetParent(hudTransform, false);
            prefab.transform.localPosition = new Vector3(1400.00f, 340.00f, 0f);
            prefab.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.SetSiblingIndex(0);
            MissionHUD = prefab.AddComponent<MissionHUD>();
            IndicatorInstance = __instance;


            IndicatorInstance = __instance;
        }

        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
        public static MissionHUD MissionHUD { get; set; }
    }
}
