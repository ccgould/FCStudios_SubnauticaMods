using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono;
using HarmonyLib;

namespace FCS_LifeSupportSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Awake")]
    internal class Player_Awake
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            __instance.gameObject.EnsureComponent<PlayerAdrenaline>();
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Update
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            if (uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD != null)
            {
                uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD.ToggleVisibility();
            }
        }
    }
}
