using System;
using AE.BaseTeleporter.Mono;
using Harmony;

namespace AE.BaseTeleporter.Patchers
{
    [HarmonyPatch(typeof(BaseTeleporterController))]
    [HarmonyPatch("Awake")]
    internal class BaseTeleporterController_Patcher
    {
        private static Action<BaseTeleporterController> BaseTeleporterControllerAdded;

        [HarmonyPostfix]
        internal static void Postfix(BaseTeleporterController __instance)
        {
            if (BaseTeleporterControllerAdded != null)
            {
                BaseTeleporterControllerAdded.Invoke(__instance);
            }
        }

        internal static bool AddEventHandlerIfMissing(Action<BaseTeleporterController> newHandler)
        {
            if (BaseTeleporterControllerAdded == null)
            {
                BaseTeleporterControllerAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<BaseTeleporterController> action in BaseTeleporterControllerAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                BaseTeleporterControllerAdded += newHandler;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(BaseTeleporterController))]
    [HarmonyPatch("OnDestroy")]
    internal class BaseTeleporterControllerDestroy_Patcher
    {
        private static Action<BaseTeleporterController> BaseTeleporterControllerDestroyed;

        [HarmonyPostfix]
        internal static void Postfix(BaseTeleporterController __instance)
        {
            if (BaseTeleporterControllerDestroyed != null)
            {
                BaseTeleporterControllerDestroyed.Invoke(__instance);
            }
        }

        internal static bool AddEventHandlerIfMissing(Action<BaseTeleporterController> newHandler)
        {
            if (BaseTeleporterControllerDestroyed == null)
            {
                BaseTeleporterControllerDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<BaseTeleporterController> action in BaseTeleporterControllerDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                BaseTeleporterControllerDestroyed += newHandler;
                return true;
            }
        }
    }


    [HarmonyPatch(typeof(BuilderTool))]
    [HarmonyPatch("Construct")]
    internal class BuilderToolConstruct_Patcher
    {
        private static Action<BuilderTool, Constructable> onContructing;

        [HarmonyPostfix]
        internal static void Postfix(BuilderTool __instance, ref Constructable c)
        {
            onContructing?.Invoke(__instance, c);
        }

        internal static bool AddEventHandlerIfMissing(Action<BuilderTool, Constructable> newHandler)
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
}
