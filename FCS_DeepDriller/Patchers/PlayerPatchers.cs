using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_DeepDriller.Configuration;
using Harmony;

namespace FCS_DeepDriller.Patchers
{
    [HarmonyPatch(typeof(EscapePod))]
    [HarmonyPatch("Awake")]
    internal class Player_Patch
    {
        [HarmonyPostfix]
        internal static void Postfix(EscapePod __instance)
        {
            EquipmentConfiguration.RefreshPDA();
        }
    }
}
