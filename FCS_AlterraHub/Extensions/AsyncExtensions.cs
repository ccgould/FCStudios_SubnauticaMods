using System.Collections;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCS_AlterraHub.Extensions
{
    public static class AsyncExtensions
    {
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
#if SUBNAUTICA_STABLE
        public static Pickupable ToPickupable(this TechType techType)
        {
            GameObject gameObject = CraftData.InstantiateFromPrefab(techType);
            if (gameObject != null)
            {
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                Pickupable component = gameObject.GetComponent<Pickupable>();
                if(component !=null) return component;
            }

            return null;
        }
        
        public static InventoryItem ToInventoryItemLegacy(this TechType techType)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(techType, false);
            GameObject gameObject =
 (prefabForTechType != null) ? Utils.SpawnFromPrefab(prefabForTechType, null) : Utils.CreateGenericLoot(techType);
            gameObject.transform.position = gameObject.transform.position;
            var pickupable = gameObject.GetComponent<Pickupable>();
            return new InventoryItem(pickupable.Pickup(false));
        }
#else

        public static IEnumerator AddTechTypeToContainerUnSafe(this TechType techType, ItemsContainer container)
        {
            if (techType != TechType.None)
            {
                var itemResult = new TaskResult<InventoryItem>();
                yield return techType.ToInventoryItem(itemResult);
                var item = itemResult.Get();
                container.UnsafeAdd(item);
            }
        }
        
        public static IEnumerator ToInventoryItem(this TechType techType, TaskResult<InventoryItem> item)
        {
            if (techType != TechType.None)
            {
                var result = CraftData.GetPrefabForTechTypeAsync(techType, false);

                yield return result;

                var go = GameObject.Instantiate(result.GetResult());

                if (!go.TryGetComponent(out Pickupable pickupable))
                    pickupable = go.AddComponent<Pickupable>();
                
                PickupReplacement(pickupable);
                item.Set(new InventoryItem(pickupable));
            }
            else
            {
                QuickLogger.DebugError("ToInventoryItem: TechType is None ");
            }


            yield break;
        }
#endif



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


#if !SUBNAUTICA_STABLE
        public static IEnumerator ToInventoryItemAsync(TechType techType, IOut<InventoryItem> result2)
        {
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(techType, result, false);
            GameObject gameObject = result.Get();
            if (gameObject != null)
            {
                CrafterLogic.NotifyCraftEnd(gameObject, techType);
                Pickupable component = gameObject.GetComponent<Pickupable>();
                if (component != null)
                {
                    result2.Set(new InventoryItem(component));
                }

                yield break;
            }
        }
#endif
    }
}
