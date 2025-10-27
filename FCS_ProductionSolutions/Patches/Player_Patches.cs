using FCS_ProductionSolutions.ModItems.Buildables.DeepDrillers.Services;
using HarmonyLib;

using UnityEngine;

namespace FCS_ProductionSolutions.Patches;
[HarmonyPatch]
internal class Player_Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(Player __instance)
    {
        new GameObject("DeepDrillerManager").AddComponent<DeepDrillerManager>();
    }

}