using GasPodCollector.Configuration;
using Harmony;

namespace AE.SeaCooker.Patchers
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("CanBeAttacked")]
    internal class Player_CanBeAttacked_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ref bool __result)
        {
            if (Mod.ProtectPlayer)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
