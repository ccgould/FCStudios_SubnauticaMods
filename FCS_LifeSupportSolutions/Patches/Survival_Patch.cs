using FCS_LifeSupportSolutions.Mods.EnergyPillVendingMachine.mono;
using FCS_LifeSupportSolutions.Spawnables;
using HarmonyLib;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Patches
{
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("GetWeaknessSpeedScalar")]
    internal class Survival_GetWeaknessSpeedScalar
    {
        internal static PlayerAdrenaline PlayerAdrenaline;

        [HarmonyPrefix]
        public static bool Prefix(ref Survival __instance, ref float __result)
        {
            if (!Main.Configuration.IsEnergyPillVendingMachineEnabled) return true;

            if (PlayerAdrenaline == null)
            {
                PlayerAdrenaline = Player.main.gameObject.EnsureComponent<PlayerAdrenaline>();
            }

            if (!PlayerAdrenaline.HasAdrenaline)
            {
                Player.main.liveMixin.invincible = false;
                return true;
            }
            Player.main.liveMixin.invincible = true;
            __result = 1;
            return false;

        }
    }
    
    [HarmonyPatch(typeof(Survival))]
    [HarmonyPatch("Eat")]
    internal class Survival_Eat
    {
        [HarmonyPrefix]
        public static void Prefix(ref Survival __instance, ref GameObject useObj, ref bool __result)
        {
            if (!Main.Configuration.IsEnergyPillVendingMachineEnabled) return;

            var comp = useObj.GetComponent<PillComponent>();

            if (comp != null)
            {
                Survival_GetWeaknessSpeedScalar.PlayerAdrenaline.AddAdrenaline(comp);
            }
        }
    }
}
