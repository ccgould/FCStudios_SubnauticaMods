using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using Harmony;

namespace FCSTechFabricator.Patchers
{
    [HarmonyPatch(typeof(Inventory))]
    [HarmonyPatch("ForcePickup")]
    internal class Inventory_Patch
    {
        [HarmonyPrefix]
        internal static void Postfix(ref Inventory __instance, Pickupable pickupable)
        {
            QuickLogger.Debug($"Pickupable Type {pickupable.GetTechType()}",true);
        }
    }
}
