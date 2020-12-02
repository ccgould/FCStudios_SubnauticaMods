using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.TrashRecycler.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.TrashReceptacle.Mono
{
    internal class TrashStorage: MonoBehaviour, IFCSStorage
    {
        private Recycler _recycler;
        private FcsDevice _mono;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; } = false;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        internal void Initialize(FcsDevice mono)
        {
            _mono = mono;
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _recycler.AddItem(item);
            ((TrashRecyclerController)_recycler.Controller).TryStartRecycling();
            return true;
        }

        private Recycler FindRecycler(Pickupable item)
        {
            var recyclers = _mono.Manager.GetDevices(Mod.RecyclerTabID).ToArray();
            
            foreach (FcsDevice device in recyclers)
            {
                var trashRecycler = (TrashRecyclerController) device;
                if (device.IsOperational && trashRecycler.IsAllowedToAdd(item,false))
                {
                    return trashRecycler.GetRecycler();
                }
            }

            return null;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            _recycler = FindRecycler(pickupable);
            
            if (_recycler == null)
            {
                QuickLogger.Debug($"Failed to locate a Recycler",true);
                return false;
            }
            
            return _recycler.IsAllowedToAdd(pickupable);
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