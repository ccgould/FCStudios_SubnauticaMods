using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSTechFabricator.Components;
using Harmony;
using MAC.OxStation.Mono;

namespace MAC.OxStation.Patches
{
    [HarmonyPatch(typeof(OxStationController))]
    [HarmonyPatch("Start")]
    internal class OxStationAwake_Patcher
    {
        private static Action<OxStationController> onDeviceAdded;

        [HarmonyPostfix]
        public static void Postfix(OxStationController __instance)
        {
            if (onDeviceAdded != null)
            {
                onDeviceAdded.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<OxStationController> newHandler)
        {
            if (onDeviceAdded == null)
            {
                onDeviceAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<OxStationController> action in onDeviceAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onDeviceAdded += newHandler;
                return true;
            }
        }

        public static bool RemoveEventHandler(Action<OxStationController> newHandler)
        {
            if (onDeviceAdded != null)
            {
                onDeviceAdded -= newHandler;
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(OxStationController))]
    [HarmonyPatch("OnDestroy")]
    internal class OxStationDestroy_Patcher
    {
        private static Action<OxStationController> onDeviceDestroyed;

        [HarmonyPostfix]
        public static void Postfix(OxStationController __instance)
        {
            if (onDeviceDestroyed != null)
            {
                onDeviceDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<OxStationController> newHandler)
        {
            if (onDeviceDestroyed == null)
            {
                onDeviceDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<OxStationController> action in onDeviceDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onDeviceDestroyed += newHandler;
                return true;
            }
        }


        public static bool RemoveEventHandler(Action<OxStationController> newHandler)
        {
            if (onDeviceDestroyed != null)
            {
                onDeviceDestroyed -= newHandler;
                return true;
            }

            return false;
        }

    }
}
