using HarmonyLib;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Patchers
{
    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("LateUpdate")]
    internal class uGUI_DepthCompass_Patch
    {
        [HarmonyPostfix]
        internal static void Postfix(uGUI_DepthCompass __instance)
        {

        }
    }
}
