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

    [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("OnHandHover")]
    internal class PrawnSuitOnHandHover
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Exosuit __instance, GUIHand hand)
        {
            QuickLogger.Debug($"On Hand Over Patch",true);
            if (Mod.IsModPatched(Mod.HoverLiftPadClassID))
            {
                QuickLogger.Debug($"[On Hand Over Patch] Mod Registered", true);

                if (PrawnSuitHandler.Contains(__instance))
                {
                    QuickLogger.Debug($"[On Hand Over Patch] Exosuit Found", true);

                    HandReticle.main.SetIcon(HandReticle.IconType.Info);
                    HandReticle.main.SetInteractTextRaw(AuxPatchers.UnDockPrawnSuitToEnter(),"");
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Exosuit))]
    [HarmonyPatch("OnHandClick")]
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
