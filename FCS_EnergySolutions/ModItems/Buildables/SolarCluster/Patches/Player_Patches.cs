using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_EnergySolutions.ModItems.Buildables.SolarCluster.Patches;

[HarmonyPatch]
internal static class Player_Patches
{
    public static Transform SunTarget { get; private set; }

    [HarmonyPatch(typeof(Player), nameof(Player.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(Player __instance)
    {
        var f = uSkyManager.main.SunLight.transform;
        if (f != null)
        {
            QuickLogger.Debug("Found Directional Light");
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(f.transform, false);
            Utils.ZeroTransform(go.transform);
            go.transform.localPosition = new Vector3(0, 0, -50000);
            SunTarget = go.transform;
        }
    }
}