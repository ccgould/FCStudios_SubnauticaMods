using FCS_AlterraHub.Helpers;
using FCS_ProductionSolutions.Buildable;
using HarmonyLib;
using UnityEngine;

#if SUBNAUTICA

#endif

namespace FCS_ProductionSolutions.Mods.DeepDriller.Patchers
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    class uGUI_PowerIndicator_Initialize_Patch
    {
        private static FCSHUD tracker;

        private static void Postfix(uGUI_PowerIndicator __instance)
        {

            if (IndicatorInstance == null)
            {
                if (Inventory.main == null)
                {
                    return;
                }

                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");


                var deepDrillerGui = GameObject.Instantiate(ModelPrefab.GetPrefab("uGUI_DeepDriller"));

                deepDrillerGui.transform.SetParent(hudTransform, false);
                deepDrillerGui.transform.SetSiblingIndex(0);
                //deepDrillerGui.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                var autocrafterHud = deepDrillerGui.AddComponent<DeepDrillerHUD>();
                autocrafterHud.Hide();
                
                IndicatorInstance = __instance;
            }
        }


        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
    }
}