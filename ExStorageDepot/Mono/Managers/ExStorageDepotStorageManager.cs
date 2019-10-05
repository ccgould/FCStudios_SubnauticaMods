using ExStorageDepot.Buildable;
using ExStorageDepot.Enumerators;
using ExStorageDepot.Helpers;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotStorageManager : MonoBehaviour
    {
        private readonly List<ItemData> _trackedItems = new List<ItemData>();
        internal Dictionary<TechType, int> ItemsDictionary { get; } = new Dictionary<TechType, int>();
        internal List<TechType> BannedTechTypes = new List<TechType>
        {
            TechType.RabbitrayEggUndiscovered,
            TechType.JellyrayEggUndiscovered,
            TechType.StalkerEggUndiscovered,
            TechType.ReefbackEggUndiscovered,
            TechType.JumperEggUndiscovered,
            TechType.BonesharkEggUndiscovered,
            TechType.GasopodEggUndiscovered,
            TechType.MesmerEggUndiscovered,
            TechType.SandsharkEggUndiscovered,
            TechType.ShockerEggUndiscovered,
            TechType.CrashEggUndiscovered,
            TechType.CrabsquidEggUndiscovered,
            TechType.CutefishEggUndiscovered,
            TechType.LavaLizardEggUndiscovered,
            TechType.CrabsnakeEggUndiscovered,
            TechType.SpadefishEggUndiscovered,

        };

        public bool IsEmpty => _trackedItems != null && _trackedItems.Count <= 0;
        internal Action<TechType> OnAddItem;
        internal Action<TechType> OnRemoveItem;
        private ChildObjectIdentifier _containerRoot;
        private ExStorageDepotController _mono;
        private int _multiplier = 1;
        private const int DumpContainerWidth = 8;
        private const int DumpContainerHeight = 10;
        private ItemsContainer _dumpContainer;
        private readonly int _maxItems = QPatch.Configuration.Config.MaxStorage;
        private StorageContainer _container;
        private int _containerWidth = 1;
        private readonly int _containerHeight = QPatch.Configuration.Config.MaxStorage;
        private RemovalType _removalType;
        private int ItemTotalCount => _trackedItems.Count + _dumpContainer.count;

        internal void Initialize(ExStorageDepotController mono)
        {
            _mono = mono;

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
                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
            }

            InvokeRepeating("UpdateStorageDisplayCount", 1, 0.5f);
        }

        private void ContainerOnRemoveItem(InventoryItem item)
        {
            QuickLogger.Debug($"Item Removed {item.item.name}");
            QuickLogger.Debug($"Removal Type {_removalType}");

            if (_removalType != RemovalType.Click)
            {
                AttemptToTakeItem(item.item.GetTechType(), RemovalType.Craft);
                _removalType = RemovalType.None;
            }
        }

        private void ContainerOnAddItem(InventoryItem item)
        {
            //QuickLogger.Debug($"Item Added {item.item.name}");
        }

        private void UpdateStorageDisplayCount()
        {
            if (_mono.Display == null) return;
            _mono.Display.SetItemCount(GetTotalCount(), _maxItems);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var food = pickupable.GetComponent<Eatable>();

            if (food != null && food.decomposes && pickupable.GetTechType() != TechType.CreepvinePiece)
            {
                QuickLogger.Info(ExStorageDepotBuildable.FoodNotAllowed(), true);
                return false;
            }

            if (pickupable.gameObject?.GetComponent<EnergyMixin>() != null)
            {
                QuickLogger.Info(ExStorageDepotBuildable.NoPlayerTools(), true);
                return false;
            }

            if (BannedTechTypes.Contains(pickupable.GetTechType()))
            {
                QuickLogger.Info(ExStorageDepotBuildable.NoUndiscorveredEggsMessage(), true);
                return false;
            }

            var containerTotal = ItemTotalCount + 1;

            if (_trackedItems.Count >= _maxItems || containerTotal > _maxItems)
            {
                QuickLogger.Info(ExStorageDepotBuildable.NoMoreSpace(), true);
                return false;
            }


            return true;
        }

        internal bool CanHoldItem(int amount)
        {
            return _trackedItems.Count + amount <= _maxItems;
        }

        public void SetMultiplier(int value)
        {
            _multiplier = value == 0 ? 1 : value;
        }

        public void OpenStorage()
        {
            QuickLogger.Debug($"Dump Button Clicked", true);

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_dumpContainer, false);
            pda.Open(PDATab.Inventory, null, new PDA.OnClose(OnPDACloseMethod), 4f);
            _mono.AnimationManager.ToggleDriveState();
        }

        private void OnPDACloseMethod(PDA pda)
        {
            _mono.AnimationManager.ToggleDriveState();
            StartCoroutine(StoreItems());
        }

        internal void AttemptToTakeItem(TechType techType, RemovalType removeType = RemovalType.Click)
        {
            if (removeType == RemovalType.Craft)
            {
                var item = _trackedItems.FirstOrDefault(x => x.TechType == techType);
                if (item == null) return;

                _removalType = RemovalType.Craft;

                if (ItemsDictionary.ContainsKey(item.TechType))
                {
                    RemoveStoredItem(item, removeType);
                    return;
                }
            }

            QuickLogger.Debug($"Attempting to take item {techType}", true);

            var amountToRemove = 1 * _multiplier;

            for (int i = 0; i < amountToRemove; i++)
            {
                var item = _trackedItems.FirstOrDefault(x => x.TechType == techType);
                if (item == null) return;
                Pickupable pickup = InventoryHelpers.ConvertToPickupable(item);

                if (pickup == null)
                {
                    QuickLogger.Error($"Attempting to get prefab failed canceling attempt to take item.");
                    return;
                }

                QuickLogger.Debug($"Attempting to take ({amountToRemove}) {techType}");

                if (ItemsDictionary.ContainsKey(techType))
                {
                    if (Inventory.main.Pickup(pickup))
                    {
                        CrafterLogic.NotifyCraftEnd(Player.main.gameObject, techType);

                        RemoveStoredItem(item, removeType);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void RemoveStoredItem(ItemData item, RemovalType removeType)
        {
            _trackedItems.Remove(item);

            // Moved here to prevent display item premature removal
            if (ItemsDictionary[item.TechType] > 1)
            {
                ItemsDictionary[item.TechType] -= 1;
            }
            else
            {
                ItemsDictionary.Remove(item.TechType);
            }

            if (removeType == RemovalType.Click)
            {
                _removalType = RemovalType.Click;
                _container.container.RemoveItem(item.TechType);
                _removalType = RemovalType.None;
            }

            _mono.Display.ItemModified(item.TechType, GetItemCount(item.TechType));
        }

        private IEnumerator StoreItems()
        {
            QuickLogger.Debug($"Store Items Dump Count: {_dumpContainer.count}");
            var amount = _dumpContainer.count;

            for (int i = amount - 1; i > -1; i--)
            {
                QuickLogger.Debug($"Number of iteration: {i}");
                if (_trackedItems.Count < _maxItems)
                {
                    AddItem(InventoryHelpers.CovertToItemData(_dumpContainer.ElementAt(i), true));
                }
            }
            QuickLogger.Debug($"Items Container Count: {_container.container.count}");

            yield return null;
        }

        private void AddItem(ItemData itemData)
        {
            _trackedItems.Add(itemData);

            if (ItemsDictionary.ContainsKey(itemData.TechType))
            {
                ItemsDictionary[itemData.TechType] += 1;
            }
            else
            {
                ItemsDictionary.Add(itemData.TechType, 1);
            }

            var go = GameObject.Instantiate(CraftData.GetPrefabForTechType(itemData.TechType));
            var newInventoryItem = new InventoryItem(go.GetComponent<Pickupable>().Pickup(false));
            newInventoryItem.SetGhostDims(1, 1);
            _container.container.UnsafeAdd(newInventoryItem);
            _mono.Display.ItemModified(itemData.TechType, GetItemCount(itemData.TechType));
        }

        internal int GetItemCount(TechType techType)
        {
            var items = _trackedItems.Where(x => x.TechType == techType);
            return items.Count();
        }

        internal int GetTotalCount()
        {
            return _trackedItems.Count;
        }

        internal void LoadFromSave(List<ItemData> storageItems)
        {
            if (storageItems != null)
            {
                foreach (ItemData itemData in storageItems)
                {
                    AddItem(itemData);
                }
            }
        }

        internal List<ItemData> GetTrackedItems()
        {
            return _trackedItems;
        }

        internal void ForceAddItem(InventoryItem item)
        {
            AddItem(InventoryHelpers.CovertToItemData(item, true));
        }

        internal bool DoesItemExist(TechType item)
        {
            var result = _trackedItems.SingleOrDefault(x => x.TechType == item);

            return result != null;
        }

        public InventoryItem ForceRemoveItem(TechType item)
        {
            return _trackedItems.SingleOrDefault(x => x.TechType == item)?.InventoryItem;
        }

        private void OnDestroy()
        {
            if (_container != null)
            {
                QuickLogger.Debug($"Clearing Container");
                _container.container.Clear();
            }

            StopCoroutine(StoreItems());
        }
    }
}
