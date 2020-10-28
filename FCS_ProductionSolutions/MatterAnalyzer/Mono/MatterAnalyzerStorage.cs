using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Configuration;
using UnityEngine;

namespace FCS_ProductionSolutions.MatterAnalyzer.Mono
{
    internal class MatterAnalyzerStorage : IFCSStorage
    {
        private MatterAnalyzerController _device;

        public MatterAnalyzerStorage(MatterAnalyzerController device)
        {
            _device = device;
        }

        public int GetContainerFreeSpace => 1;
        public bool IsFull => false;
        public bool CanBeStored(int amount, TechType techType)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            var size = item.item.gameObject.GetComponentInChildren<Plantable>().size;
            _device.SetScanTime(size);
            OnContainerAddItem?.Invoke(_device,item.item.GetTechType());
            GameObject.Destroy(item.item.gameObject);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return !Mod.IsHydroponicKnownTech(pickupable.GetTechType()) && 
                   _device.DumpContainer.GetCount() != 1 && 
                   pickupable.gameObject.GetComponentInChildren<Plantable>() != null;
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

        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public bool ContainsItem(TechType techType)
        {
            return false;
        }
    }
}
