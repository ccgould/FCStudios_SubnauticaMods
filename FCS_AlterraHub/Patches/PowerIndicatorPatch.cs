using FCS_AlterraHub.Helpers;
using HarmonyLib;

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

            IndicatorInstance = __instance;

        }

        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
    }
}
