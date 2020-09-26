using ExStorageDepot.Buildable;
using ExStorageDepot.Enumerators;
using ExStorageDepot.Model;
using FCSCommon.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExStorageDepot.Helpers;
using FCSCommon.Helpers;
using FCSTechFabricator.Interfaces;
using UnityEngine;


namespace ExStorageDepot.Mono.Managers
{
    internal class ExStorageDepotStorageManager : MonoBehaviour, IFCSStorage
    { 
        private ExStorageDepotController _mono;
        private int _multiplier = 1;
        private int _maxItems = QPatch.Config.MaxStorage;

        internal List<ItemData> ContainerItems { get; private set; } = new List<ItemData>();
        public int GetContainerFreeSpace => _maxItems - GetTotalCount();
        public bool IsFull => GetIsFull();
        public Action<int, int> OnContainerUpdate { get; set; } //Not being used causes lag when moving large items


        private bool GetIsFull()
        {
            return GetTotalCount() >= _maxItems;
        }
        
        internal void Initialize(ExStorageDepotController mono)
        {
            QuickLogger.Debug("Initializing Storage Container");
            _mono = mono;
            InvokeRepeating("UpdateStorageDisplayCount", 1, 0.5f);
        }

        internal bool HasItems()
        {
            return ContainerItems.Count > 0;
        }

        internal void OnDumpContainerClosed()
        {
            if (!_mono.AnimationManager.GetDriveState()) return;
            _mono.AnimationManager.ToggleDriveState();
        }


        #region IFCSStorage

        public bool CanBeStored(int amount, TechType techType)
        {
            if (amount + ContainerItems.Count > _maxItems) return false;
            if (IsFull) return false;

            return GetContainerFreeSpace > 0;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                ContainerItems.Add(InventoryHelpers.CovertToItemData(item, true));
                //OnContainerUpdate?.Invoke(GetTotalCount(), _maxItems);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Class: [Storage Container] | Method: [AddItemToContainer] | Error: ${e.Message}");
                return false;
            }

            return true;
        }

        bool IFCSStorage.IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var eatable = pickupable.gameObject.GetComponent<Eatable>();
            if (eatable != null)
            {
                if (eatable.decomposes && pickupable.GetTechType() != TechType.CreepvinePiece)
                {
                    QuickLogger.Message(ExStorageDepotBuildable.FoodNotAllowed(),true);
                    return false;
                }
            }

            if (_mono.DumpContainer == null)
            {
                QuickLogger.Error("DumpContainer returned null");
                return false;
            }
            
            if (!CanBeStored(_mono.DumpContainer.GetCount() + 1, pickupable.GetTechType()))
            {
                QuickLogger.Info(ExStorageDepotBuildable.NoMoreSpace(), true);
                return false;
            }
            
            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            if (ContainsItem(techType))
            {
                var itemData = ContainerItems.FirstOrDefault(x => x.TechType == techType);
                var item = InventoryHelpers.ConvertToPickupable(itemData);
                ContainerItems.Remove(itemData);
                //OnContainerUpdate?.Invoke(GetTotalCount(),_maxItems);
                return item;
            }

            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            var lookup = ContainerItems?.Where(x => x != null).ToLookup(x => x.TechType).ToArray();
            return lookup?.ToDictionary(count => count.Key, count => count.Count());
        }


        public bool ContainsItem(TechType techType)
        {
            return ContainerItems!=null && ContainerItems.Any(x=>x.TechType == techType);
        }

        #endregion
        
        internal int GetItemCount(TechType techType)
        {
            var items = ContainerItems.Where(x => x.TechType == techType);
            return items.Count();
        }

        internal int GetTotalCount()
        {
            return ContainerItems.Count;
        }

        internal void LoadFromSave(List<ItemData> storageItems)
        {
            ContainerItems = storageItems;
            //OnContainerUpdate?.Invoke(GetTotalCount(),_maxItems);
        }
        
        internal void SetMultiplier(int value)
        {
            _multiplier = value == 0 ? 1 : value;
        }
        
        internal void AttemptToTakeItem(TechType techType)
        {
            var amount = GetItemCount(techType);

            QuickLogger.Debug($"Container returned {amount} item/s for TechType {techType}");
#if SUBNAUTICA
            var itemSize = CraftData.GetItemSize(techType);
#elif BELOWZERO
            var itemSize = TechData.GetItemSize(techType);
#endif
            if (Inventory.main.HasRoomFor(itemSize.x, itemSize.y))
            {
                if (amount > 0)
                {
                    QuickLogger.Debug($"Attempting to take {_multiplier} item/s");

                    for (int i = 0; i < _multiplier; i++)
                    {
                        var itemData = ContainerItems.FirstOrDefault(x => x.TechType == techType);
                        Pickupable pickup = InventoryHelpers.ConvertToPickupable(itemData);

                        if (pickup == null)
                        {
                            QuickLogger.Debug($"There are 0 {techType} in the container while using first or default Current Amount of {techType} is: {GetItemCount(techType)}", true);
                            return;
                        }

                        Inventory.main.Pickup(pickup); 
                        ContainerItems.Remove(itemData);
                        //OnContainerUpdate?.Invoke(GetTotalCount(),_maxItems);
                    }
                }
                else
                {
                    QuickLogger.Debug($"There are 0 {techType} in the container.", true);
                }
            }
        }

        private void FlushContainer()
        {
            ContainerItems.Clear();
        }

        private void OnDestroy()
        {

        }

    }
}