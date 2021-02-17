using System;
using System.Reflection;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_StorageSolutions.Patches
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    internal static class Player_Update_Patch
    {
        private static bool _hasBeenPurged;

        private static void Postfix(Player __instance)
        {
            if (LargeWorldStreamer.main.IsWorldSettled())
            {
                return;
                Mod.CleanDummyServers();
                _hasBeenPurged = true;
            }

        }
    }
}