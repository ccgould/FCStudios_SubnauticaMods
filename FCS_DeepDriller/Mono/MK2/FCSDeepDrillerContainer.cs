using System;
using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Helpers;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerContainer : IFCSStorage
    {
        public int GetContainerFreeSpace => CalculateFreeSpace();

       public Action<int, int> OnContainerUpdate { get; set; } // Not being used casing lag on large transfers

        private int CalculateFreeSpace()
        {
            var total = GetContainerTotal();
            return Mathf.Max(0, QPatch.Configuration.StorageSize - total);
        }

        public bool IsFull => IsContainerFull(); 
        private Dictionary<TechType,int> _container = new Dictionary<TechType, int>();

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
            return GetContainerTotal() >= QPatch.Configuration.StorageSize;
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
                        deviceStorage.AddItemToContainer(techType.ToInventoryItem());
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

        //internal void CraftItem(KeyValuePair<TechType, CraftData.TechData> item)
        //{
        //    foreach (CraftData.Ingredient ingredient in item.Value._ingredients)
        //    {
        //        for (int i = 0; i < ingredient.amount; i++)
        //        {
        //            RemoveItemFromContainer(ingredient.techType);
        //        }
        //    }

        //    AddItemToContainer(item.Key.ToInventoryItem());
        //}

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
                    _container.Add(item,1);
                }

                //OnContainerUpdate?.Invoke(GetContainerTotal(),QPatch.Configuration.StorageSize);
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
                if (!FCSDeepDrillerOperations.CanPlayerHold(item)) return false;
                
                if (_container.ContainsKey(item))
                {

                    if (FCSDeepDrillerOperations.GivePlayerItem(item))
                    {
                        if (_container[item] == 1)
                        {
                            _container.Remove(item);
                        }
                        else
                        {
                            _container[item] -= 1;
                        }

                        //OnContainerUpdate?.Invoke(GetContainerTotal(), QPatch.Configuration.StorageSize);
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

        internal bool OnlyRemoveItemFromContainer(TechType item)
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

                    //OnContainerUpdate?.Invoke(GetContainerTotal(), QPatch.Configuration.StorageSize);
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

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            throw new NotImplementedException();
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return _container;
        }

        public bool ContainsItem(TechType techType)
        {
            throw new NotImplementedException();
        }

        public void Setup(FCSDeepDrillerController fcsDeepDrillerController)
        {
            
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

        public Dictionary<TechType,int> SaveData()
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
    }
}
