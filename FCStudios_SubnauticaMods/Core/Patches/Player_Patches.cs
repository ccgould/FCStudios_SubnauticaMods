using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Services;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Core.Patches;

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
        new GameObject("GamePlayService").AddComponent<GamePlayService>();
        new GameObject("uGUIFCS").AddComponent<uGUIFCS>();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnProtoSerialize))]
    [HarmonyPostfix]
    private static void OnProtoSerialize_Postfix()
    {
        //Mod.SaveGamePlaySettings();
        foreach (var modPack in ModRegistrationService.GetRegisteredMods())
        {
            modPack.Value.Save();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnProtoDeserialize))]
    [HarmonyPostfix]
    private static void OnProtoDeserialize_Postfix()
    {
        foreach (var modPack in ModRegistrationService.GetRegisteredMods())
        {
            modPack.Value.Load();
        }
    }
}
