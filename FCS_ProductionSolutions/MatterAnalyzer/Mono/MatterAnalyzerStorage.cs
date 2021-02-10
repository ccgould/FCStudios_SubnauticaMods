using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Utilities;
using SMLHelper.V2.Handlers;
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
                    var pickPrefab = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();
                    
                    if (pickPrefab != null)
                    {
                        QuickLogger.Debug($"PickPrefab: {pickPrefab?.pickTech}", true);
                        _device.PickTech = pickPrefab.pickTech;
                        pickTypeSet = true;
                    }
                    else
                    {
                        QuickLogger.Debug($"PickPrefab Not Found Checking Pickupable", true);
                        var pickup = growingPlant.grownModelPrefab.GetComponentInChildren<Pickupable>();
                        if (pickup != null)
                        {
                            QuickLogger.Debug($"Pickup: {pickup.GetTechType()}", true);
                            _device.PickTech = pickup.GetTechType();
                            pickTypeSet = true;
                        }
                    }

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

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (_device.DumpContainer.GetCount() + 1 > 1) return false;

            var techType = pickupable.GetTechType();

            if (!Mod.IsHydroponicKnownTech(techType, out var data1) && Mod.IsNonePlantableAllowedList.Contains(techType))
            {
                return true;
            }

            var plantable = pickupable.gameObject.GetComponentInChildren<Plantable>();
            if (plantable == null || !plantable.isSeedling || !IsValidSeedling(pickupable.GetTechType())) return false;

            return !Mod.IsHydroponicKnownTech(techType, out var data) &&_device.DumpContainer.GetCount() != 1;
        }

        private bool IsValidSeedling(TechType techType)
        {
            return ValidSeeds.Contains(techType);
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

        internal HashSet<TechType> ValidSeeds = new HashSet<TechType>
        {
            TechType.BluePalmSeed,
            TechType.PurpleBranchesSeed,
            TechType.EyesPlantSeed,
            TechType.FernPalmSeed,
            TechType.RedRollPlantSeed,
            TechType.GabeSFeatherSeed,
            TechType.RedGreenTentacleSeed,
            TechType.OrangePetalsPlantSeed,
            TechType.OrangeMushroomSpore,
            TechType.SnakeMushroomSpore,
            TechType.MembrainTreeSeed,
            TechType.PurpleVasePlantSeed,
            TechType.SmallFanSeed,
            TechType.RedBushSeed,
            TechType.RedConePlantSeed,
            TechType.RedBasketPlantSeed,
            TechType.SeaCrownSeed,
            TechType.ShellGrassSeed,
            TechType.PurpleRattleSpore,
            TechType.SpottedLeavesPlantSeed,
            TechType.SpikePlantSeed,
            TechType.PurpleFanSeed,
            TechType.PurpleStalkSeed,
            TechType.PinkFlowerSeed,
            TechType.PurpleTentacleSeed,
            TechType.PurpleBrainCoralPiece,
            TechType.PinkMushroomSpore,
            TechType.MelonSeed,
            TechType.HangingFruit,
            TechType.BulboTreePiece,
            TechType.PurpleVegetable,
            TechType.KooshChunk,
            TechType.BloodOil,
            TechType.AcidMushroomSpore,
            TechType.WhiteMushroomSpore,
            TechType.JellyPlantSeed,
            TechType.TreeMushroomPiece,
            TechType.RedGreenTentacleSeed,
            TechType.CreepvinePiece,
            TechType.CreepvineSeedCluster,
            TechType.EyesPlantSeed,



        };
    }
}
