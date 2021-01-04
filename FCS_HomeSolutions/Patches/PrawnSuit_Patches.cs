using System.Collections.Generic;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_HomeSolutions.Patches
{
    internal static class PrawnSuitHandler
    {
        private static readonly List<Exosuit> ActivelyDocked = new List<Exosuit>();

        internal static void RegisterExoSuit(Exosuit exosuit)
        {

            if (!Contains(exosuit))
            {
                ActivelyDocked.Add(exosuit);
                QuickLogger.Debug($"Registered Exosuit {ActivelyDocked.Count}", true);
            }
        }

        internal static void RemoveExoSuit(Exosuit exosuit)
        {
            if (exosuit == null) return; 
             ActivelyDocked.Remove(exosuit);
             QuickLogger.Debug($"Removed Exosuit {ActivelyDocked.Count}", true);
        }

        internal static bool Contains(Exosuit exosuit)
        {
            return ActivelyDocked.Contains(exosuit);
        }
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnHandHover))]
    internal class PrawnSuitOnHandHover
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Exosuit __instance, GUIHand hand)
        {
            if (Mod.IsModPatched(Mod.HoverLiftPadClassID))
            {
                if (PrawnSuitHandler.Contains(__instance))
                {
                    HandReticle.main.SetIcon(HandReticle.IconType.Info);
                    HandReticle.main.SetInteractTextRaw(AuxPatchers.UnDockPrawnSuitToEnter(),"");
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.OnHandClick))]
    internal class PrawnSuitOnHandClick
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Exosuit __instance, GUIHand hand)
        {
            if (Mod.IsModPatched(Mod.HoverLiftPadClassID))
            {
                if (PrawnSuitHandler.Contains(__instance))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
