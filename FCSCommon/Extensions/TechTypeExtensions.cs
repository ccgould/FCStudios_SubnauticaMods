using UnityEngine;

namespace FCSCommon.Extensions
{
    public static class TechTypeExtensions
    {
        public static Pickupable ToPickupable(this TechType techType)
        {
            Pickupable pickupable = null;
            var prefab = CraftData.GetPrefabForTechType(techType);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                pickupable = go.GetComponent<Pickupable>().Pickup(false);
            }

            return pickupable;
        }

        public static InventoryItem ToInventoryItem(this TechType techType)
        {
            InventoryItem item = null;
            var prefab = CraftData.GetPrefabForTechType(techType);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                var pickupable = go.GetComponent<Pickupable>().Pickup(false);
                item = new InventoryItem(pickupable);
            }
            return item;
        }
    }
}
