using System;
using FCS_AlterraHub.Mono;
using HarmonyLib;

namespace FCS_ProductionSolutions.DeepDriller.Patchers
{
    [HarmonyPatch(typeof(FcsDevice))]
    [HarmonyPatch("Start")]
    internal class FCSConnectableAwake_Patcher
    {
        private static Action<FcsDevice> onFCSConnectableAdded;

        [HarmonyPostfix]
        public static void Postfix(FcsDevice __instance)
        {
            onFCSConnectableAdded?.Invoke(__instance);
        }

        public static bool AddEventHandlerIfMissing(Action<FcsDevice> newHandler)
        {
            if (onFCSConnectableAdded == null)
            {
                onFCSConnectableAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FcsDevice> action in onFCSConnectableAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onFCSConnectableAdded += newHandler;
                return true;
            }
        }

        public static bool RemoveEventHandler(Action<FcsDevice> newHandler)
        {
            if (onFCSConnectableAdded != null)
            {
                onFCSConnectableAdded -= newHandler;
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FcsDevice))]
    [HarmonyPatch("OnDestroy")]
    internal class FCSConnectableDestroy_Patcher
    {
        private static Action<FcsDevice> onFCSConnectableDestroyed;

        [HarmonyPostfix]
        public static void Postfix(FcsDevice __instance)
        {
            if (onFCSConnectableDestroyed != null)
            {
                onFCSConnectableDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<FcsDevice> newHandler)
        {
            if (onFCSConnectableDestroyed == null)
            {
                onFCSConnectableDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FcsDevice> action in onFCSConnectableDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onFCSConnectableDestroyed += newHandler;
                return true;
            }
        }


        public static bool RemoveEventHandler(Action<FcsDevice> newHandler)
        {
            if (onFCSConnectableDestroyed != null)
            {
                onFCSConnectableDestroyed -= newHandler;
                return true;
            }

            return false;
        }

    }
}
