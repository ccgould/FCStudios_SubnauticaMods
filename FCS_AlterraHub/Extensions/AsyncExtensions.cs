using System.Collections;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Extensions
{
    public static class AsyncExtensions
    {
#if SUBNAUTICA
        public static Pickupable ToPickupable(this TechType techType)
        {
            GameObject gameObject = CraftData.InstantiateFromPrefab(techType, false);
            if (gameObject != null)
            {
                gameObject.transform.position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 3f;
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                Pickupable component = gameObject.GetComponent<Pickupable>();
                if(component !=null) return component;
            }

            return null;
        }

        public static InventoryItem ToInventoryItem(this TechType techType)
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

            if (item == null)
            {
                item = techType.ToInventoryItemLegacy();
            }

        return item;
        }

        public static InventoryItem ToInventoryItemLegacy(this TechType techType)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(techType, false);
            GameObject gameObject = (prefabForTechType != null) ? Utils.SpawnFromPrefab(prefabForTechType, null) : Utils.CreateGenericLoot(techType);
            gameObject.transform.position = gameObject.transform.position;
            var pickupable = gameObject.GetComponent<Pickupable>();
            return new InventoryItem(pickupable.Pickup(false));
        }

#endif

        public static InventoryItem ToInventoryItem(this Pickupable pickupable)
        {
            InventoryItem item = null;
            if (pickupable != null)
            {
                PickupReplacement(pickupable);
                item = new InventoryItem(pickupable);
            }
            return item;
        }

        public static TechType ToTechType(this string value)
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
