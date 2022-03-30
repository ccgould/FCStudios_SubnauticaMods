using System;
using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.MatterAnalyzer.Mono
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
            var pickTypeSet = false;
            var plantable = item.item.gameObject.GetComponentInChildren<Plantable>();

            if (plantable != null)
            {
                var size = plantable.size;
                var grown = plantable.Spawn(_device.transform, true);
                grown.SetActive(false);
                var growingPlant = grown.GetComponent<GrowingPlant>();

                if (growingPlant != null)
                {
#if SUBNAUTICA
                    var pickPrefab = GetPickPrefab(growingPlant);
                    
                    if (pickPrefab != null)
                    {
                        QuickLogger.Debug($"PickPrefab: {Language.main.Get(pickPrefab.pickTech)}", true);
                        _device.PickTech = pickPrefab.pickTech;
                        pickTypeSet = true;
                    }
                    else
                    {
                        QuickLogger.Debug($"PickPrefab Not Found Checking Pickupable", true);
                        var pickup = growingPlant.grownModelPrefab.GetComponentInChildren<Pickupable>();
                        if (pickup != null)
                        {
                            QuickLogger.Debug($"Pickup: {Language.main.Get(pickup.GetTechType())}", true);
                            _device.PickTech = pickup.GetTechType();
                            pickTypeSet = true;
                        }
                    }
#else
                    if (growingPlant.plantTechType != TechType.None)
                    {
                        QuickLogger.Debug($"Pickup: {Language.main.Get(growingPlant.plantTechType)}", true);
                        _device.PickTech = growingPlant.plantTechType;
                        pickTypeSet = true;
                    }
#endif

                    if (!pickTypeSet)
                    {
                        _device.PickTech = TechType.None;
                    }
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

#if SUBNAUTICA
        private static PickPrefab GetPickPrefab(GrowingPlant growingPlant)
        {
            return growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();
        }
#endif

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (_device.DumpContainer.GetCount() + 1 > 1) return false;

            var techType = pickupable.GetTechType();

            if (!Mod.IsHydroponicKnownTech(techType, out var data1) &&
                WorldHelpers.IsNonePlantableAllowedList.Contains(techType))
            {
                return true;
            }

            var plantable = pickupable.gameObject.GetComponentInChildren<Plantable>();

            if (plantable == null || !plantable.isSeedling || !IsValidSeedling(pickupable.GetTechType())) return false;

            return !Mod.IsHydroponicKnownTech(techType, out var data) && _device.DumpContainer.GetCount() != 1;
        }

        private bool IsValidSeedling(TechType techType)
        {
            return WorldHelpers.KnownPickTypesContains(techType);
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
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

        public ItemsContainer ItemsContainer { get; set; }

        public int StorageCount()
        {
            return ItemsContainer?.count ?? 0;
        }
    }
}