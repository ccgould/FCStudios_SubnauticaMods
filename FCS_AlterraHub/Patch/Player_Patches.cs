using FCS_AlterraHub.Debug;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Patch
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch("Update")]
    public class OpenDebugMenu
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (Input.GetKey(KeyCode.F9))
            {
                DebugController.Main.Toggle();
            }
        }
    }
}
