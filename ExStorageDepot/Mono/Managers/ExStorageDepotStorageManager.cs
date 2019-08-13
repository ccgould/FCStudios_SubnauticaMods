using ExStorageDepot.Buildable;
using ExStorageDepot.Helpers;
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
        private readonly List<ItemData> _trackedItems = new List<ItemData>();
        internal Dictionary<TechType, int> ItemsDictionary { get; } = new Dictionary<TechType, int>();
        public bool IsEmpty => _trackedItems != null && _trackedItems.Count <= 0;
        internal Action<TechType> OnAddItem;
        internal Action<TechType> OnRemoveItem;
        private ChildObjectIdentifier _containerRoot;
        private ExStorageDepotController _mono;
        private int _multiplier = 1;
        private const int DumpContainerWidth = 8;
        private const int DumpContainerHeight = 10;
        private ItemsContainer _dumpContainer;
        private const int MaxItems = 150;

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

            if (_dumpContainer == null)
            {
                QuickLogger.Debug("Initializing Dump Container");

                _dumpContainer = new ItemsContainer(DumpContainerWidth, DumpContainerHeight, _containerRoot.transform,
                    ExStorageDepotBuildable.DumpContainerLabel(), null);
                _dumpContainer.isAllowedToAdd += IsAllowedToAdd;
                //_dumpContainer.onAddItem += OnDumpAddItemEvent;
                //_dumpContainer.onRemoveItem += OnDumpRemoveItemEvent;
            }

            InvokeRepeating("UpdateStorageDisplayCount", 1, 0.5f);
        }

        private void UpdateStorageDisplayCount()
        {
            if (_mono.Display == null) return;
            _mono.Display.SetItemCount(GetTotalCount(), MaxItems);
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.gameObject?.GetComponent<EnergyMixin>() != null)
            {
                QuickLogger.Info("Ex-Storage cannot store PlayerTools at this time.", true);
                return false;
            }

            QuickLogger.Debug("Item is not a Player Tool", true);

            if (_trackedItems.Count >= MaxItems)
            {
                QuickLogger.Info("Ex-Storage is full", true);
                return false;
            }

            return true;
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
            StoreItems();
        }

        public void AttemptToTakeItem(TechType techType)
        {
            QuickLogger.Debug($"Attempting to take item {techType}", true);

            var amountToRemove = 1 * _multiplier;

            for (int i = 0; i < amountToRemove; i++)
            {

                var item = _trackedItems.FirstOrDefault(x => x.TechType == techType);

                if (item == null)
                {
                    QuickLogger.Error("Item Returned null", true);
                    return;
                }

                Pickupable pickup = InventoryHelpers.ConvertToPickupable(item);

                QuickLogger.Debug($"Attempting to take ({amountToRemove}) {item.TechType}", true);

                if (ItemsDictionary.ContainsKey(item.TechType))
                {
                    if (Inventory.main.Pickup(pickup))
                    {
                        CrafterLogic.NotifyCraftEnd(Player.main.gameObject, techType);
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

                        _mono.Display.ItemModified(techType, GetItemCount(techType));
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void StoreItems()
        {
            foreach (InventoryItem inventoryItem in _dumpContainer)
            {
                if (_trackedItems.Count < MaxItems)
                {
                    AddItem(InventoryHelpers.CovertToItemData(inventoryItem, true));
                }
            }
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

        public void LoadFromSave(List<ItemData> storageItems)
        {
            if (storageItems != null)
            {
                foreach (ItemData itemData in storageItems)
                {
                    AddItem(itemData);
                }
            }
        }

        public List<ItemData> GetTrackedItems()
        {
            return _trackedItems;
        }
    }
}
