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
    }
}
