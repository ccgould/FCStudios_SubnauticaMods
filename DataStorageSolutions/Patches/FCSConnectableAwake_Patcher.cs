using System;
using FCSTechFabricator.Components;
using HarmonyLib;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(FCSConnectableDevice))]
    [HarmonyPatch("Start")]
    internal class FCSConnectableAwake_Patcher
    {
        private static Action<FCSConnectableDevice> onFCSConnectableAdded;

        [HarmonyPostfix]
        public static void Postfix(FCSConnectableDevice __instance)
        {
            onFCSConnectableAdded?.Invoke(__instance);
        }

        public static bool AddEventHandlerIfMissing(Action<FCSConnectableDevice> newHandler)
        {
            if (onFCSConnectableAdded == null)
            {
                onFCSConnectableAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FCSConnectableDevice> action in onFCSConnectableAdded.GetInvocationList())
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

        public static bool RemoveEventHandler(Action<FCSConnectableDevice> newHandler)
        {
            if (onFCSConnectableAdded != null)
            {
                onFCSConnectableAdded -= newHandler;
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FCSConnectableDevice))]
    [HarmonyPatch("OnDestroy")]
    internal class FCSConnectableDestroy_Patcher
    {
        private static Action<FCSConnectableDevice> onFCSConnectableDestroyed;

        [HarmonyPostfix]
        public static void Postfix(FCSConnectableDevice __instance)
        {
            if (onFCSConnectableDestroyed != null)
            {
                onFCSConnectableDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<FCSConnectableDevice> newHandler)
        {
            if (onFCSConnectableDestroyed == null)
            {
                onFCSConnectableDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FCSConnectableDevice> action in onFCSConnectableDestroyed.GetInvocationList())
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


        public static bool RemoveEventHandler(Action<FCSConnectableDevice> newHandler)
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
