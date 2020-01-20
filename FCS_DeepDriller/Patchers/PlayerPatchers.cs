using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_DeepDriller.Configuration;
using Harmony;

namespace FCS_DeepDriller.Patchers
{
    [HarmonyPatch(typeof(Player))]
    [HarmonyPatch(nameof(Player.Update))]
    internal class Player_Patch
    {
        [HarmonyPostfix]
        internal static void Postfix(Player __instance)
        {
            EquipmentConfiguration.RefreshPDA();
        }
    }
}
