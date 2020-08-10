using FCSCommon.Utilities;
using HarmonyLib;

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
