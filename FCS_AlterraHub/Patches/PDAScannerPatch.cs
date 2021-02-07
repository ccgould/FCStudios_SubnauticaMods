using System;
using FCSCommon.Utilities;
using HarmonyLib;

namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(PDAScanner), nameof(PDAScanner.Scan), new Type[] { })]
    public class PDAScanner_Scan
    {
        public static TechType techType;
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (PDAScanner.scanTarget.techType != TechType.None)
            {
                techType = PDAScanner.scanTarget.techType;
            }
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            QuickLogger.Debug($"Contains Complete Entry: {PDAScanner.ContainsCompleteEntry(techType) || KnownTech.Contains(techType)}", true);
            if (PDAScanner.ContainsCompleteEntry(techType) || KnownTech.Contains(techType))
            {
                //QPatch.MissionManagerGM.NotifyItemScanned(techType);
            }
        }
    }
}
