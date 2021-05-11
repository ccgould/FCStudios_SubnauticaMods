using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
using UnityEngine;
using UWE;

namespace FCSCommon.Extensions
{
    internal static class AsyncExtensions
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
            
            if(pickupable ==null)
            {
                QuickLogger.Error("Failed to get pickupable through PrefabDatabase trying with utils.");
                var item = Utils.CreateGenericLoot(techType);
                pickupable = item?.GetComponentInChildren<Pickupable>();
                var result = pickupable == null ? "not successful" : "successful";
                QuickLogger.Info($"Attempt was {result}");
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

#if SUBNAUTICA_STABLE
        internal static InventoryItem ToInventoryItemLegacy(this TechType techType)
        {
            GameObject prefabForTechType = CraftData.GetPrefabForTechType(techType, false);
            GameObject gameObject = (prefabForTechType != null) ? Utils.SpawnFromPrefab(prefabForTechType, null) : Utils.CreateGenericLoot(techType);
            gameObject.transform.position = gameObject.transform.position;
            var pickupable = gameObject.GetComponent<Pickupable>();
            return new InventoryItem(pickupable.Pickup(false));
        }
#else
        internal static IEnumerable ToInventoryItemLegacy(TechType techType, IOut<InventoryItem> result)
        {
            var prefabForTechType = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return prefabForTechType;

            var prefabResult = prefabForTechType.GetResult();

            GameObject gameObject = (prefabResult != null) ? Utils.SpawnFromPrefab(prefabResult, null) : Utils.CreateGenericLoot(techType);
            gameObject.transform.position = gameObject.transform.position;
            var pickupable = gameObject.GetComponent<Pickupable>();

            TaskResult<Pickupable> pickupResult = new TaskResult<Pickupable>();
            yield return pickupable.PickupAsync(pickupResult, false);
            result.Set(new InventoryItem(pickupResult.Get()));
            yield break;
        }
#endif

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

#if SUBNAUTICA_STABLE

#else
        internal static IEnumerator AddToContainerAsync(TechType techType, ItemsContainer container)
        {
            TaskResult<InventoryItem> taskResult = new TaskResult<InventoryItem>();
            yield return AsyncExtensions.ToInventoryItemLegacyAsync(techType, taskResult);
            container.UnsafeAdd(taskResult.Get());
            yield break;
        }
#endif

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
