using FCS_AlterraHub.Core.Services;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Core.Patches
{
    [HarmonyPatch]
    internal class Player_Patches
    {
        [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
        [HarmonyPostfix]
        private static void Awake_Postfix(Player __instance)
        {
            new GameObject("HabitatService").AddComponent<HabitatService>();
            new GameObject("StoreManager").AddComponent<StoreManager>();
            new GameObject("VoiceNotificationService").AddComponent<VoiceNotificationService>();
            new GameObject("AccountService").AddComponent<AccountService>();
            new GameObject("SaveLoadService").AddComponent<SaveLoadDataService>();
        }
    }
}
