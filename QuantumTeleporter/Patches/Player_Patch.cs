using Harmony;
using QuantumTeleporter.Managers;

namespace QuantumTeleporter.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal class Player_Patch
    {
        internal static void Postfix(ref Player __instance)
        {
            TeleportManager.Update();
        }
    }
}
