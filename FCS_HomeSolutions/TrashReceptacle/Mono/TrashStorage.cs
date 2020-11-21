using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using UnityEngine;

namespace FCS_HomeSolutions.TrashReceptacle.Mono
{
    internal class TrashStorage: MonoBehaviour, IFCSStorage
    {
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; } = false;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public BaseManager Manager { get; set; }


        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var recycler = FindRecycler();
            if (recycler != null)
            {
                //TODO SendItem to recycler
                return true;
            }

            return false;
        }

        private object FindRecycler()
        {
            //TODO Change to a recycler no object
            var recyclers = Manager.GetDevices(Mod.RecyclerTabID).ToArray();
            return recyclers.Any() ? recyclers[0] : null;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return true;
        }
        
        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }
    }
}