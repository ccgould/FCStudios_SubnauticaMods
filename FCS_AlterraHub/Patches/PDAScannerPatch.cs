using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (PDAScanner.ContainsCompleteEntry(techType) || KnownTech.Contains(techType))
            {
                QPatch.QuestManagerGM.NotifyItemScanned(techType);
            }
        }
    }
}
