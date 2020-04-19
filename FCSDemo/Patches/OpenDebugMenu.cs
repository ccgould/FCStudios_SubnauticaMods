using FCSDemo.Mono;
using Harmony;
using UnityEngine;

namespace FCStudioDebugger.Debug.Patches
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
                DebugMenu.Main.Toggle();
            }
        }
    }
}
