using System;
using HarmonyLib;

namespace FCS_HomeSolutions.Patches
{
    [HarmonyPatch(typeof(IngameMenu))]
    [HarmonyPatch("SaveGame")]
    public class InGameMenuSavePatcher
    {
        private static Action onSaveButtonPressed;

        [HarmonyPrefix]
        public static void Prefix()
        {
            onSaveButtonPressed?.Invoke();

            onSaveButtonPressed = null;
        }

        public static bool AddEventHandlerIfMissing(Action newHandler)
        {
            if (onSaveButtonPressed == null)
            {
                onSaveButtonPressed += newHandler;
                return true;
            }
            else
            {
                foreach (Action action in onSaveButtonPressed.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onSaveButtonPressed += newHandler;
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(IngameMenu))]
    [HarmonyPatch("SetPleaseWaitVisible")]
    public class InGameMenuClosePatcher
    {
        private static Action onSetPleaseWaitVisible;

        [HarmonyPostfix]
        public static void Posfix(bool visible)
        {

            if(visible) return;

            onSetPleaseWaitVisible?.Invoke();

            onSetPleaseWaitVisible = null;
        }

        public static bool AddEventHandlerIfMissing(Action newHandler)
        {
            if (onSetPleaseWaitVisible == null)
            {
                onSetPleaseWaitVisible += newHandler;
                return true;
            }
            else
            {
                foreach (Action action in onSetPleaseWaitVisible.GetInvocationList())
                {
                    if (action == newHandler)
                    {
                        return false;
                    }
                }

                onSetPleaseWaitVisible += newHandler;
                return true;
            }
        }
    }
}
