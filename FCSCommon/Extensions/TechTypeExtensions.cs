using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class TechTypeExtensions
    {
        public static Pickupable ToPickupable(this TechType techtype)
        {
            var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(techtype));
            return go.GetComponent<Pickupable>().Pickup(false);
        }

        public static InventoryItem ToInventoryItem(this TechType techType)
        {
            var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(techType));
            var pickupable = go.GetComponent<Pickupable>().Pickup(false);
            return new InventoryItem(pickupable);
        }
    }
}
