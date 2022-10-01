using FCS_HomeSolutions.Mods.JukeBox.Mono;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Model;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Mono;
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
            if (QPatch.Configuration.IsQuantumTeleporterEnabled)
            {
                TeleportManager.Update();
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.main.Awake))]
    internal class Player_Awake
    {
        [HarmonyPrefix]
        public static void Postfix(ref Player __instance)
        {
            var main = JukeBox.Main;
        }
    }
}
