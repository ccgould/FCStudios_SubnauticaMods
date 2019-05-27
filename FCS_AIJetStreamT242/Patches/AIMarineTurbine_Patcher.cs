using FCS_AIMarineTurbine.Mono;
using Harmony;
using System;

namespace FCS_AIMarineTurbine.Patches
{
    [HarmonyPatch(typeof(AIJetStreamT242Controller))]
    [HarmonyPatch("Awake")]
    internal class AIMarineTurbine_Patcher
    {
        private static Action<AIJetStreamT242Controller> onJetStreamAdded;

        [HarmonyPostfix]
        public static void Postfix(AIJetStreamT242Controller __instance)
        {
            if (onJetStreamAdded != null)
            {
                onJetStreamAdded.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<AIJetStreamT242Controller> newHandler)
        {
            if (onJetStreamAdded == null)
            {
                onJetStreamAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<AIJetStreamT242Controller> action in onJetStreamAdded.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onJetStreamAdded += newHandler;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AIJetStreamT242Controller))]
    [HarmonyPatch("OnDestroy")]
    internal class JetStreamDestroy_Patcher
    {
        private static Action<AIJetStreamT242Controller> onJetStreamDestroyed;

        [HarmonyPostfix]
        public static void Postfix(AIJetStreamT242Controller __instance)
        {
            if (onJetStreamDestroyed != null)
            {
                onJetStreamDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<AIJetStreamT242Controller> newHandler)
        {
            if (onJetStreamDestroyed == null)
            {
                onJetStreamDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<AIJetStreamT242Controller> action in onJetStreamDestroyed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onJetStreamDestroyed += newHandler;
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
}
