using FCSAlterraIndustrialSolutions.Models.Controllers;
using Harmony;
using System;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Patches
{
    [HarmonyPatch(typeof(JetStreamT242Controller))]
    [HarmonyPatch("Awake")]
    public class AISolutions_Patcher
    {
        private static Action<JetStreamT242Controller> onJetStreamAdded;

        [HarmonyPostfix]
        public static void Postfix(JetStreamT242Controller __instance)
        {
            if (onJetStreamAdded != null)
            {
                onJetStreamAdded.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<JetStreamT242Controller> newHandler)
        {
            if (onJetStreamAdded == null)
            {
                onJetStreamAdded += newHandler;
                return true;
            }
            else
            {
                foreach (Action<JetStreamT242Controller> action in onJetStreamAdded.GetInvocationList())
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

    [HarmonyPatch(typeof(JetStreamT242Controller))]
    [HarmonyPatch("OnDestroy")]
    public class JetStreamDestroy_Patcher
    {
        private static Action<JetStreamT242Controller> onJetStreamDestroyed;

        [HarmonyPostfix]
        public static void Postfix(JetStreamT242Controller __instance)
        {
            if (onJetStreamDestroyed != null)
            {
                onJetStreamDestroyed.Invoke(__instance);
            }
        }

        public static bool AddEventHandlerIfMissing(Action<JetStreamT242Controller> newHandler)
        {
            if (onJetStreamDestroyed == null)
            {
                onJetStreamDestroyed += newHandler;
                return true;
            }
            else
            {
                foreach (Action<JetStreamT242Controller> action in onJetStreamDestroyed.GetInvocationList())
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
    public class BuilderToolConstruct_Patcher
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

    //[HarmonyPatch(typeof(PowerFX))]
    //[HarmonyPatch("SetTarget")]
    //public class PowerFXSetTarget_Patcher
    //{
    //    private static Action<PowerFX, GameObject> onSetTarget;

    //    [HarmonyPostfix]
    //    public static void Postfix(PowerFX __instance, ref GameObject gameobject)
    //    {
    //        onSetTarget?.Invoke(__instance, gameobject);
    //    }

    //    public static bool AddEventHandlerIfMissing(Action<PowerFX, GameObject> newHandler)
    //    {
    //        if (onSetTarget == null)
    //        {
    //            onSetTarget += newHandler;
    //            return true;
    //        }
    //        else
    //        {
    //            foreach (Action<PowerFX, GameObject> action in onSetTarget.GetInvocationList())
    //            {
    //                if (action == newHandler)
    //                {
    //                    return false;
    //                }
    //            }

    //            onSetTarget += newHandler;
    //            return true;
    //        }
    //    }
    //}
}
