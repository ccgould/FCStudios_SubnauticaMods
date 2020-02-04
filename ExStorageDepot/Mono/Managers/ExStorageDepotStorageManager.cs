using ExStorageDepot.Buildable;
using ExStorageDepot.Enumerators;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Extensions;
using UnityEngine;


namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotStorageManager : MonoBehaviour
    {
        //private readonly List<ItemData> _trackedItems = new List<ItemData>();
        internal Dictionary<TechType, int> ItemsDictionary { get; } = new Dictionary<TechType, int>();

        public bool IsEmpty => _container != null && _container.container.count <= 0;

        private ChildObjectIdentifier _containerRoot;
        private ExStorageDepotController _mono;
        private int _multiplier = 1;
        private const int DumpContainerWidth = 6;
        private const int DumpContainerHeight = 8;
        private ItemsContainer _dumpContainer;
        private int _maxItems;
        private StorageContainer _container;
        private int _containerWidth = 1;
        private int _containerHeight;

        private int ItemTotalCount => _container.container.count + _dumpContainer.count;

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;
            _containerHeight = _maxItems = QPatch.Config.MaxStorage;

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Ex-Storage Root");
                var storageRoot = new GameObject("ExStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _mono = mono;
            }

            if (_container == null)
            {
                QuickLogger.Debug("Initializing Storage Container");

                _container = _mono.gameObject.GetComponentInChildren<StorageContainer>();
                _container.width = _containerWidth;
                _container.height = _containerHeight;
                _container.container.onRemoveItem += ContainerOnRemoveItem;
                _container.container.onAddItem += ContainerOnAddItem;
                _container.container.Resize(_containerWidth, _containerHeight);
                _container.storageLabel = ExStorageDepotBuildable.StorageContainerLabel();
            }

            if (_dumpContainer == null)
            {
                QuickLogger.Debug("Initializing Dump Container");

                _dumpContainer = new ItemsContainer(DumpContainerWidth, DumpContainerHeight, _containerRoot.transform,
                    ExStorageDepotBuildable.DumpContainerLabel(), null);
                _dumpContainer.Resize(DumpContainerWidth, DumpContainerHeight);
                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
            }

            InvokeRepeating("UpdateStorageDisplayCount", 1, 0.5f);
        }

        private void ContainerOnAddItem(InventoryItem item)
        {
            QuickLogger.Debug($"Item Added {item.item.name}");

            UpdateScreen(item.item.GetTechType());
        }

        private void UpdateStorageDisplayCount()
        {
            if (_mono.Display == null) return;
            _mono.Display.SetItemCount(GetTotalCount(), _maxItems);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var containerTotal = ItemTotalCount + 1;

            if (_container.container.count >= _maxItems || containerTotal > _maxItems)
            {
                QuickLogger.Info(ExStorageDepotBuildable.NoMoreSpace(), true);
                return false;
            }


            return true;
        }

        private void OnPDACloseMethod(PDA pda)
        {
            _mono.AnimationManager.ToggleDriveState();
            StartCoroutine(StoreItems());
        }

        private IEnumerator StoreItems()
        {
            QuickLogger.Debug($"Store Items Dump Count: {_dumpContainer.count}");

            var amount = _dumpContainer.count;

            for (int i = amount - 1; i > -1; i--)
            {
                QuickLogger.Debug($"Number of iteration: {i}");
                if (_container.container.count < _maxItems)
                {
                    AddItem(_dumpContainer.ElementAt(i));
                }
            }

            QuickLogger.Debug($"Items Container Count: {_container.container.count}");

            yield return null;
        }

        private void AddItem(InventoryItem item)
        {
            if (_container.container.count < _maxItems)
            {
                item.SetGhostDims(1, 1);
                _container.container.UnsafeAdd(item);
                _dumpContainer.RemoveItem(item.item);
            }
        }

        private void UpdateScreen(TechType techType, OperationMode mode = OperationMode.Addition)
        {
            switch (mode)
            {
                case OperationMode.Addition when ItemsDictionary.ContainsKey(techType):
                    ItemsDictionary[techType] += 1;
                    break;
                case OperationMode.Addition:
                    ItemsDictionary.Add(techType, 1);
                    break;
                case OperationMode.Removal when ItemsDictionary.ContainsKey(techType):
                    ItemsDictionary[techType] -= 1;
                    break;
                case OperationMode.Removal:
                    ItemsDictionary.Remove(techType);
                    break;
            }

            _mono.Display.ItemModified(techType, GetItemCount(techType));
        }

        private void AddItem(ItemData itemData)
        {
            var prefab = CraftData.GetPrefabForTechType(itemData.TechType, false);

            if (prefab != null)
            {
                var go = GameObject.Instantiate(prefab);
#if SUBNAUTICA
                var newInventoryItem = new InventoryItem(go.GetComponent<Pickupable>().Pickup(false));
#elif BELOWZERO
                Pickupable pickupable = go.GetComponent<Pickupable>();
                pickupable.Pickup(false);
                var newInventoryItem = new InventoryItem(pickupable);
#endif
                newInventoryItem.SetGhostDims(1, 1);
                _container.container.UnsafeAdd(newInventoryItem);
            }
        }

        internal int GetItemCount(TechType techType)
        {
            var items = _container.container.Where(x => x.item.GetTechType() == techType);
            return items.Count();
        }

        internal int GetTotalCount()
        {
            return _container.container.count;
        }

        internal void LoadFromSave(List<ItemData> storageItems)
        {
            if (storageItems != null)
            {
                foreach (ItemData itemData in storageItems)
                {
                    QuickLogger.Debug($"Load from Save {itemData.TechType}");
                    AddItem(itemData);
                }
            }
        }

        //internal List<ItemData> GetTrackedItems()
        //{
        //    return _trackedItems;
        //}

        internal void ForceAddItem(InventoryItem item)
        {
            AddItem(item);
        }

        internal bool DoesItemExist(TechType item)
        {
            var result = _container.container.SingleOrDefault(x => x.item.GetTechType() == item);

            return result != null;
        }

        internal InventoryItem ForceRemoveItem(TechType item)
        {
            return _container.container.SingleOrDefault(x => x.item.GetTechType() == item);
        }

        internal bool CanHoldItem(int amount)
        {
            return _container.container.count + amount <= _maxItems;
        }

        internal void SetMultiplier(int value)
        {
            _multiplier = value == 0 ? 1 : value;
        }

        internal void OpenStorage()
        {
            QuickLogger.Debug($"Dump Button Clicked", true);

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_dumpContainer, false);
            pda.Open(PDATab.Inventory, null, new PDA.OnClose(OnPDACloseMethod), 4f);
            _mono.AnimationManager.ToggleDriveState();
        }

        internal void AttemptToTakeItem(TechType techType)
        {
            var amount = _container.container.GetCount(techType);

            QuickLogger.Debug($"Container returned {amount} item/s for TechType {techType}");

            var itemSize = CraftData.GetItemSize(techType);

            if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                if (amount > 0)
                {
                    QuickLogger.Debug($"Attempting to take {_multiplier} item/s");

                    for (int i = 0; i < _multiplier; i++)
                    {
                        Pickupable pickup = _container.container.RemoveItem(techType);

                        if (pickup == null)
                        {
                            QuickLogger.Debug($"There are 0 {techType} in the container while using first or default Current Amount of {techType} is: {_container.container.GetCount(techType)}", true);
                            return;
                        }

                        Inventory.main.Pickup(pickup);
                    }
                }
                else
                {
                    QuickLogger.Debug($"There are 0 {techType} in the container.", true);
                }
            }
        }

        private void ContainerOnRemoveItem(InventoryItem item)
        {
            var techType = item.item.GetTechType();
            QuickLogger.Debug($"Recently {techType} was removed from the container");

            if (_container.container.GetCount(techType) <= 0)
            {
                ItemsDictionary.Remove(techType);
            }

            UpdateScreen(techType, OperationMode.Removal);

        }
        private void OnDestroy()
        {
            //if (_container != null)
            //{
            //    QuickLogger.Debug($"Clearing Container");
            //    _container.container.Clear();
            //}

            StopCoroutine(StoreItems());
        }

        internal void UpdateInventory()
        {
            ItemsDictionary.Clear();
            foreach (var item in _container.container)
            {
                UpdateScreen(item.item.GetTechType());
            }
        }
    }
}
