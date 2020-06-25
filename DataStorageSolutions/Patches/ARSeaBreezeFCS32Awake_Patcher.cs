using System;
using FCSTechFabricator.Components;
using Harmony;

namespace DataStorageSolutions.Patches
{
    [HarmonyPatch(typeof(FCSConnectableDevice))]
    [HarmonyPatch("Start")]
    internal class ARSeaBreezeFCS32Awake_Patcher
    {
        private static Action<FCSConnectableDevice> onSeaBreezeAdded;

        [HarmonyPostfix]
        public static void Postfix(FCSConnectableDevice __instance)
        {
            if (onSeaBreezeAdded != null)
            {
                onSeaBreezeAdded.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<FCSConnectableDevice> newHandler)
        {
            if (onSeaBreezeAdded == null)
            {
                onSeaBreezeAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FCSConnectableDevice> action in onSeaBreezeAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onSeaBreezeAdded += newHandler;
                return true;
            }
        }

        public static bool RemoveEventHandler(Action<FCSConnectableDevice> newHandler)
        {
            if (onSeaBreezeAdded != null)
            {
                onSeaBreezeAdded -= newHandler;
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(FCSConnectableDevice))]
    [HarmonyPatch("OnDestroy")]
    internal class ARSeaBreezeFCS32Destroy_Patcher
    {
        private static Action<FCSConnectableDevice> onSeaBreezeDestroyed;

        [HarmonyPostfix]
        public static void Postfix(FCSConnectableDevice __instance)
        {
            if (onSeaBreezeDestroyed != null)
            {
                onSeaBreezeDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<FCSConnectableDevice> newHandler)
        {
            if (onSeaBreezeDestroyed == null)
            {
                onSeaBreezeDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<FCSConnectableDevice> action in onSeaBreezeDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onSeaBreezeDestroyed += newHandler;
                return true;
            }
        }


        public static bool RemoveEventHandler(Action<FCSConnectableDevice> newHandler)
        {
            if (onSeaBreezeDestroyed != null)
            {
                onSeaBreezeDestroyed -= newHandler;
                return true;
            }

            return false;
        }

    }


    [HarmonyPatch(typeof(BuilderTool))]
    [HarmonyPatch("Construct")]
    internal class BuilderToolConstruct_Patcher
    {
        private static Action<BuilderTool, Constructable> onContructing;

        [HarmonyPostfix]
        public static void Postfix(BuilderTool __instance, ref Constructable c)
        {
            onContructing?.Invoke(__instance, c);
        }

        public static bool AddEventHandlerIfMissing(Action<BuilderTool, Constructable> newHandler)
        {
            if (onContructing == null)
            {
                onContructing += newHandler;
                return true;
            }
            else
            {
                foreach (Action<BuilderTool, Constructable> action in onContructing.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onContructing += newHandler;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(uGUI_DepthCompass))]
    [HarmonyPatch("LateUpdate")]
    internal class uGUI_DepthCompassLateUpdate_Patcher
    {
        private static Action onLateUpdate;

        [HarmonyPostfix]
        public static void Postfix(uGUI_DepthCompass __instance)
        {
            onLateUpdate?.Invoke();
        }

        public static bool AddEventHandlerIfMissing(Action newHandler)
        {
            if (onLateUpdate == null)
            {
                onLateUpdate += newHandler;
                return true;
            }
            else
            {
                foreach (Action action in onLateUpdate.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onLateUpdate += newHandler;
                return true;
            }
        }
    }
}
