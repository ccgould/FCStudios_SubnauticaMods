using FCSAlterraShipping.Mono;
using Harmony;
using System;

namespace FCSAlterraShipping.Patchers
{
    [HarmonyPatch(typeof(AlterraShippingTarget))]
    [HarmonyPatch("Initialize")]
    internal class AlterraShippingTarget_Patcher
    {
        private static Action<AlterraShippingTarget> AlterraShippingTargetAdded;

        [HarmonyPostfix]
        internal static void Postfix(AlterraShippingTarget __instance)
        {
            if (AlterraShippingTargetAdded != null)
            {
                AlterraShippingTargetAdded.Invoke(__instance);
            }
        }

        internal static bool AddEventHandlerIfMissing(Action<AlterraShippingTarget> newHandler)
        {
            if (AlterraShippingTargetAdded == null)
            {
                AlterraShippingTargetAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<AlterraShippingTarget> action in AlterraShippingTargetAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                AlterraShippingTargetAdded += newHandler;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AlterraShippingTarget))]
    [HarmonyPatch("OnDestroy")]
    internal class AlterraShippingTargetDestroy_Patcher
    {
        private static Action<AlterraShippingTarget> AlterraShippingTargetDestroyed;

        [HarmonyPostfix]
        internal static void Postfix(AlterraShippingTarget __instance)
        {
            if (AlterraShippingTargetDestroyed != null)
            {
                AlterraShippingTargetDestroyed.Invoke(__instance);
            }
        }

        internal static bool AddEventHandlerIfMissing(Action<AlterraShippingTarget> newHandler)
        {
            if (AlterraShippingTargetDestroyed == null)
            {
                AlterraShippingTargetDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<AlterraShippingTarget> action in AlterraShippingTargetDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                AlterraShippingTargetDestroyed += newHandler;
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
