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
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

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
            if (item == null) return false;
            var plantable = item.item.gameObject.GetComponentInChildren<Plantable>();
            
            if (plantable != null)
            {
                var size = plantable.size;
                var grown = plantable.Spawn(_device.transform, true);
                grown.SetActive(false);
                var growingPlant = grown.GetComponent<GrowingPlant>();

                if (growingPlant != null)
                {
                    _device.PickTech = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>()?.pickTech ?? TechType.None;
                }

                GameObject.Destroy(grown);
                _device.Seed = plantable;
                _device.SetScanTime(size);
                OnContainerAddItem?.Invoke(_device, item.item.GetTechType());
            }
            else
            {
                _device.SetScanTime(Plantable.PlantSize.Large);
                OnContainerAddItem?.Invoke(_device, item.item.GetTechType());
            }

            GameObject.Destroy(item.item.gameObject);

            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var techType = pickupable.GetTechType();
            if (Mod.IsNonePlantableAllowedList.Contains(techType))
            {
                return true;
            }

            return !Mod.IsHydroponicKnownTech(techType, out var data) && 
                   _device.DumpContainer.GetCount() != 1 && 
                   pickupable.gameObject.GetComponentInChildren<Plantable>().isSeedling;
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
