using System;
using System.Collections.Generic;
using System.Linq;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerContainer : IFCSStorage
    {
        public int GetContainerFreeSpace => CalculateFreeSpace();

        private int CalculateFreeSpace()
        {
            var total = GetContainerTotal();
            return Mathf.Max(0, QPatch.Configuration.StorageSize - total);
        }

        public bool IsFull => IsContainerFull();
        private Dictionary<TechType,int> Container { get; set; } = new Dictionary<TechType, int>();

        private int GetContainerTotal()
        {
            //Go through the dictionary of items and add all the values together if container is null return 0.
            return Container?.Select((t, i) => Container?.ElementAt(i).Value).Sum() ?? 0;
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
                if (Container.ContainsKey(item))
                {
                    Container[item] += 1;
                }
                else
                {
                    Container.Add(item,1);
                }
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
                if (Container.ContainsKey(item))
                {
                    if (Container[item] == 1)
                    {
                        Container.Remove(item);
                    }

                    Container[item] -= 1;
                }
                else
                {
                    Container.Add(item, 1);
                }
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
        /// Checks to see if the item is allowed in the container
        /// </summary>
        /// <param name="pickupable">The pickable component of the object</param>
        /// <param name="verbose">Display messsage</param>
        /// <returns>Returns true if allowed in the container.</returns>
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return !IsContainerFull();
        }

        public Dictionary<TechType, int> GetItems()
        {
            return Container;
        }

        public void Setup(FCSDeepDrillerController fcsDeepDrillerController)
        {
            
        }
    }
}
