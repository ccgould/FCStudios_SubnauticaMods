using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCSCommon.Utilities;
using UnityEngine;
using UWE;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    public class FCSDeepDrillerContainer : IFCSStorage
    {
        public int GetContainerFreeSpace => CalculateFreeSpace();

        public Action<int, int> OnContainerUpdate { get; set; } // Not being used casing lag on large transfers
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        private int CalculateFreeSpace()
        {
            var total = GetContainerTotal();
            return Mathf.Max(0, _storageSize - total);
        }

        public bool IsFull => IsContainerFull();
        private Dictionary<TechType, int> _container = new Dictionary<TechType, int>();
        private int _storageSize = Main.Configuration.DDStorageSize;

        internal void OverrideContainerSize(int newSize)
        {
            _storageSize = newSize;
        }

        internal int GetContainerTotal()
        {
            //Go through the dictionary of items and add all the values together if container is null return 0.
            return _container?.Select((t, i) => _container?.ElementAt(i).Value).Sum() ?? 0;
        }

        /// <summary>
        /// Checks if the container is full.
        /// </summary>
        /// <returns>Returns true if the container is full.</returns>
        private bool IsContainerFull()
        {
            return GetContainerTotal() >= _storageSize;
        }

        /// <summary>
        /// Removes item from container and sends to another device
        /// </summary>
        /// <param name="techType"></param>
        /// <param name="deviceStorage"></param>
        /// <returns></returns>
        internal bool TransferItem(TechType techType, IFCSStorage deviceStorage)
        {
            try
            {
                //Remove item from container
                var isRemoved = RemoveItemFromContainer(techType);

                if (isRemoved)
                {
                    var canBeStored = deviceStorage.CanBeStored(1, techType);
                    if (canBeStored)
                    {
                        CoroutineHost.StartCoroutine(
                            techType.AddTechTypeToContainerUnSafe(deviceStorage.ItemsContainer));
                    }
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDeepDrillerContainer>(e.Message);
                QuickLogger.Error<FCSDeepDrillerContainer>(e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Checks if the item can be stored in this container.
        /// </summary>
        /// <param name="amount">The amount to store in the container</param>
        /// <param name="techType">The techType to store</param>
        /// <returns>Returns true if the item can be stored</returns>
        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        internal bool ContainsItem(TechType techType, int amount)
        {
            return _container.Any(x => x.Key == techType && x.Value >= amount);
        }

        /// <summary>
        /// Add item to the container using the Inventory item.
        /// </summary>
        /// <param name="item">The TechType to add</param>
        /// <returns>Returns true if item was added</returns>
        public bool AddItemToContainer(InventoryItem item)
        {
            // We arent using this Method but the override because I dont want to use an Inventory Item.
            //TODO turn IFCSStorage into a Abstract Class
            return true;
        }

        /// <summary>
        /// Add item to the container using the techtype.
        /// </summary>
        /// <param name="item">The TechType to add</param>
        /// <returns>Returns true if item was added</returns>
        internal bool AddItemToContainer(TechType item)
        {
            try
            {
                if (_container.ContainsKey(item))
                {
                    _container[item] += 1;
                }
                else
                {
                    _container.Add(item, 1);
                }

                OnContainerUpdate?.Invoke(GetContainerTotal(), _storageSize);
            }
            catch (Exception e)
            {
                QuickLogger.Error<FCSDeepDrillerContainer>(e.Message);
                QuickLogger.Error<FCSDeepDrillerContainer>(e.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Remove item from the container
        /// </summary>
        /// <param name="item">The TechType to remove</param>
        /// <returns>Returns true if item was removed</returns>
        internal bool RemoveItemFromContainer(TechType item)
        {
            try
            {
                if (!PlayerInteractionHelper.CanPlayerHold(item)) return false;

                if (_container.ContainsKey(item))
                {
                    if (PlayerInteractionHelper.GivePlayerItem(item))
                    {
                        if (_container[item] == 1)
                        {
                            _container.Remove(item);
                        }
                        else
                        {
                            _container[item] -= 1;
                        }

                        OnContainerUpdate?.Invoke(GetContainerTotal(), _storageSize);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        internal bool OnlyRemoveItemFromContainer(TechType item, bool bypassNotification = false)
        {
            try
            {
                if (_container.ContainsKey(item))
                {
                    if (_container[item] == 1)
                    {
                        _container.Remove(item);
                    }
                    else
                    {
                        _container[item] -= 1;
                    }

                    OnContainerUpdate?.Invoke(GetContainerTotal(), _storageSize);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the item is allowed in the container
        /// </summary>
        /// <param name="pickupable">The pickable component of the object</param>
        /// <param name="verbose">Display messsage</param>
        /// <returns>Returns true if allowed in the container.</returns>
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return !IsContainerFull();
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        Pickupable IFCSStorage.RemoveItemFromContainer(TechType techType)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return _container;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public ItemsContainer ItemsContainer { get; set; }

        public int StorageCount()
        {
            return GetContainerTotal();
        }

        public bool HasItems()
        {
            return _container.Count > 0;
        }

        public void LoadData(Dictionary<TechType, int> dataItems)
        {
            if (dataItems == null) return;
            _container = dataItems;
        }

        public Dictionary<TechType, int> SaveData()
        {
            return _container;
        }

        public int GetItemCount(TechType item)
        {
            if (_container.ContainsKey(item))
            {
                return _container[item];
            }

            return 0;
        }

        public void Clear()
        {
            _container.Clear();
        }

        public bool IsEmpty()
        {
            return GetContainerTotal() <= 0;
        }

        public TechType GetRandomItem()
        {
            return _container.Keys.Count <= 0 ? TechType.None : _container.Keys.ElementAt(0);
        }

        public int GetContainerCapacity()
        {
            return _storageSize;
        }
    }
}