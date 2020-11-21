using System;
using HarmonyLib;


namespace FCS_AlterraHub.Patches
{
    
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    class Player_Patch
    {
        internal static Action OnPlayerUpdate;

        private static void Postfix(Player __instance)
        {
            OnPlayerUpdate?.Invoke();
        }
    }
}
