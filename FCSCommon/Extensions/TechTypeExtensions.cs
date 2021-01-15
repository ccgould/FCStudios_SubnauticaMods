using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCSCommon.Extensions
{
    internal static class FCSTechTypeExtensions
    {
#if SUBNAUTICA
        internal static Pickupable ToPickupable(this TechType techType)
        {
            Pickupable pickupable = null;

            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(techType), out string filepath))
            {
                GameObject prefab = Resources.Load<GameObject>(filepath);

                if (prefab != null)
                { 
                    var go = GameObject.Instantiate(prefab);
                    pickupable = go.EnsureComponent<Pickupable>();
                    PickupReplacement(pickupable);
                }
            }

            return pickupable;
        }

        internal static InventoryItem ToInventoryItem(this TechType techType)
        {

            InventoryItem item = null;
            if (PrefabDatabase.TryGetPrefabFilename(CraftData.GetClassIdForTechType(techType), out string filepath))
            {
                GameObject prefab = Resources.Load<GameObject>(filepath);

                if (prefab != null)
                {
                    var go = GameObject.Instantiate(prefab);
                    var pickupable = go.EnsureComponent<Pickupable>();
                    PickupReplacement(pickupable);
                    item = new InventoryItem(pickupable);
                }
                else
                {
                    var go = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                    go.EnsureComponent<PrefabIdentifier>();
                    var pickupable = go.EnsureComponent<Pickupable>();
                    PickupReplacement(pickupable);
                    item = new InventoryItem(pickupable);
                }
            }
            return item;
        }

        internal static InventoryItem ToInventoryItemLegacy(this TechType techType)
        {

            InventoryItem item = null;
            var prefab = CraftData.GetPrefabForTechType(techType);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                var pickupable = go.EnsureComponent<Pickupable>().Pickup(false);
                item = new InventoryItem(pickupable);
            }
            else
            {
                var go = GameObject.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube));
                go.EnsureComponent<PrefabIdentifier>();
                var pickupable = go.EnsureComponent<Pickupable>().Pickup(false);
                item = new InventoryItem(pickupable);
            }
            return item;
        }

        internal static InventoryItem ToInventoryItem(this Pickupable pickupable)
        {
            InventoryItem item = null;
            if (pickupable != null)
            {
                PickupReplacement(pickupable);
                item = new InventoryItem(pickupable);
            }
            return item;
        }

#elif BELOWZERO
        internal static Pickupable ToPickupable(this TechType techType)
        {
            Pickupable pickupable = null;
            var prefab = CraftData.GetPrefabForTechType(techType);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                pickupable = go.GetComponent<Pickupable>();
                pickupable.Pickup(false);
            }

            return pickupable;
        }

        internal static InventoryItem ToInventoryItem(this TechType techType)
        {
            InventoryItem item = null;
            var prefab = CraftData.GetPrefabForTechType(techType);
            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
                var pickupable = go.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                item = new InventoryItem(pickupable);
            }
            return item;
        }

#endif

        internal static TechType ToTechType(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return TechType.None;
            }

            // Look for a known TechType
            if (TechTypeExtensions.FromString(value, out TechType tType, true))
            {
                return tType;
            }

            //  Not one of the known TechTypes - is it registered with SMLHelper?
            if (TechTypeHandler.TryGetModdedTechType(value, out TechType custom))
            {
                return custom;
            }

            return TechType.None;
        }

        private static void PickupReplacement(Pickupable pickupable)
        {
            pickupable.SendMessage("OnExamine", SendMessageOptions.DontRequireReceiver);
            int num = pickupable.gameObject.GetComponentsInChildren<Rigidbody>(true).Length;
            if (num == 0)
            {
                pickupable.gameObject.AddComponent<Rigidbody>();
            }
            pickupable.Deactivate();
            pickupable.attached = true;
            if (pickupable._isInSub)
            {
                pickupable.Unplace();
                pickupable._isInSub = false;
            }
        }
    }
}
