using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.QuantumTeleporter.Mono;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_HomeSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Update
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            TeleportManager.Update();
        }
    }
}
