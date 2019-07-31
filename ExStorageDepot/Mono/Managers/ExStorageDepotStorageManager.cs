using ExStorageDepot.Buildable;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotStorageManager : MonoBehaviour
    {
        private readonly List<ItemData> _storageContainer = new List<ItemData>();
        private readonly List<ItemData> _dumpContainer = new List<ItemData>();
        internal Action<TechType> OnAddItem;
        internal Action<TechType> OnRemoveItem;
        private ChildObjectIdentifier _containerRoot;
        private ItemsContainer _container;
        private int _containerWidth = 8;
        private int _containerHeight = 10;
        private bool _containerHasItems;
        private const int MaxItems = 640;

        internal void Initialize(ExStorageDepotController mono)
        {
            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Deep Driller StorageRoot");
                var storageRoot = new GameObject("DeepDrillerStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
            }

            if (_container == null)
            {
                QuickLogger.Debug("Initializing Deep Driller Container");

                _container = new ItemsContainer(_containerWidth, _containerHeight, _containerRoot.transform,
                    ExStorageDepotBuildable.StorageContainerLabel(), null);

                _container.onRemoveItem += OnContainerRemoveItemEvent;
                _container.onAddItem += OnContainerAddItemEvent;

            }
        }

        private void OnContainerRemoveItemEvent(InventoryItem item)
        {
            _containerHasItems = _container.count > 0;
            var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;

            var itemMatch = _dumpContainer.Single(x => x.PrefabId == prefabId);

            if (itemMatch == null) return;

            _dumpContainer.Remove(itemMatch);
        }

        private void OnContainerAddItemEvent(InventoryItem item)
        {
            _containerHasItems = true;
            var techType = item.item.GetTechType();

            var itemMatch = _dumpContainer.Single(x => x.TechType == techType);

            if (itemMatch != null) return;
            var newItem = new ItemData { InventoryItem = item };
            newItem.ExposeInventoryData();
            _dumpContainer.Add(newItem);
        }

        internal bool AddItem(InventoryItem item)
        {
            if (GetItemsCount() == MaxItems)
            {
                QuickLogger.Message(ExStorageDepotBuildable.ContainerFullMessage(), true);
                return false;
            }
            _containerHasItems = true;

            var techType = item.item.GetTechType();

            var itemMatch = _storageContainer.Single(x => x.TechType == techType);

            if (itemMatch == null)
            {
                var newItem = new ItemData { InventoryItem = item };
                newItem.ExposeInventoryData();
                _storageContainer.Add(newItem);
            }

            QuickLogger.Debug($"Added TechType {techType} to the container.");

            return true;
        }

        internal void RemoveItem(InventoryItem item)
        {
            var prefabId = item.item.gameObject.GetComponent<PrefabIdentifier>()?.Id;
            var techType = item.item.GetTechType();

            var itemMatch = _storageContainer.Single(x => x.PrefabId == prefabId);

            if (itemMatch != null)
            {
                _storageContainer.Remove(itemMatch);
            }

            QuickLogger.Debug($"Removed TechType {techType} from the container.");
        }

        internal int GetTechTypeCount(TechType techType)
        {
            return _storageContainer.Count(x => x.TechType == techType);
        }

        internal int GetItemsCount()
        {
            return _storageContainer.Count;
        }

        internal void StoreItems()
        {
            foreach (ItemData data in _dumpContainer)
            {
                if (GetItemsCount() < MaxItems)
                {
                    AddItem(data.InventoryItem);
                }
            }
        }

        internal List<ItemData> SaveData()
        {
            foreach (ItemData itemData in _storageContainer)
            {
                itemData.SaveData();
            }

            return _storageContainer;
        }
    }
}
