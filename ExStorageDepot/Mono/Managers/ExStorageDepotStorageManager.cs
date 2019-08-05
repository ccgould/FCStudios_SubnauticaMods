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
        public List<ItemData> TrackedItems { get; private set; } = new List<ItemData>();
        internal Action<TechType> OnAddItem;
        internal Action<TechType> OnRemoveItem;
        private ChildObjectIdentifier _containerRoot;
        private ExStorageDepotController _mono;
        private int _multiplier;
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

            if (TrackedItems.Count >= MaxItems)
            {
                QuickLogger.Info("Ex-Storage is full", true);
                return false;
            }

            return true;
        }

        public void SetMultiplier(int value)
        {
            _multiplier = value;
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

            var item = TrackedItems.FirstOrDefault(x => x.TechType == techType);

            if (item == null)
            {
                QuickLogger.Error("Item Returned null", true);
                return;
            }

            Pickupable pickup = InventoryHelpers.ConvertToPickupable(item);

            if (Inventory.main.Pickup(pickup))
            {
                CrafterLogic.NotifyCraftEnd(Player.main.gameObject, techType);

                TrackedItems.Remove(item);

                _mono.Display.ItemModified(techType, GetItemCount(techType));
            }
        }

        private void StoreItems()
        {
            foreach (InventoryItem inventoryItem in _dumpContainer)
            {
                var techType = inventoryItem.item.GetTechType();

                if (TrackedItems.Count < MaxItems)
                {
                    TrackedItems.Add(InventoryHelpers.CovertToItemData(inventoryItem, true));
                    _mono.Display.ItemModified(techType, GetItemCount(techType));
                }
            }
        }

        internal int GetItemCount(TechType techType)
        {
            var items = TrackedItems.Where(x => x.TechType == techType);
            return items.Count();
        }

        internal int GetTotalCount()
        {
            return TrackedItems.Count;
        }

        public void LoadFromSave(List<ItemData> storageItems)
        {
            if (storageItems != null)
            {
                foreach (ItemData itemData in storageItems)
                {
                    TrackedItems.Add(itemData);
                    _mono.Display.ItemModified(itemData.TechType, GetItemCount(itemData.TechType));
                }

                TrackedItems = storageItems;
            }
        }
    }
}
