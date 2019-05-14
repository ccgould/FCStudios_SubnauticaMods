using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Harmony;

namespace FCSAlterraIndustrialSolutions.Patches
{
    [HarmonyPatch(typeof(SolarPanel))]
    [HarmonyPatch("Awake")]
    public class SolarPanel_Patcher
    {
        private static Action<SolarPanel> onAwake;

        [HarmonyPostfix]
        public static void Postfix(SolarPanel __instance)
        {
            if (onAwake != null)
            {
                onAwake.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<SolarPanel> newHandler)
        {
            if (onAwake == null)
            {
                onAwake += newHandler;
                return true;
            }
            else
            {
                foreach (Action<SolarPanel> action in onAwake.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onAwake += newHandler;
                return true;
            }
        }
    }
}
