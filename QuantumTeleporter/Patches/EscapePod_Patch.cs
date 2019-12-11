using Harmony;
using QuantumTeleporter.Managers;

namespace QuantumTeleporter.Patches
{
    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("Update")]
    internal class EscapePod_Patch
    {
        internal static void Postfix(ref EscapePod __instance)
        {
            TeleportManager.Update();
        }
    }
}
