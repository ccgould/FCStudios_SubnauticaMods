using FCS_StorageSolutions.Services;
using HarmonyLib;

using UnityEngine;

namespace FCS_StorageSolutions.Patches;
[HarmonyPatch]
internal class Player_Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(Player __instance)
    {
        new GameObject("DSSService").AddComponent<DSSService>();
    }

}