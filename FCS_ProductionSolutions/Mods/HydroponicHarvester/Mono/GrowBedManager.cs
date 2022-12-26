﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Models;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono
{
    internal class GrowBedManager : FCSStorage, IFCSGrowBed
    {

        //TODO fix clear slot changing first slot instead of target slot

        private const float MaxPlantsHeight = 3f;
        internal PlantSlot[] Slots;
        private PlantSlot _targetSlot;
        public GameObject grownPlantsRoot { get; set; }
        internal HydroponicHarvesterController HarvesterController { get; private set; }

        internal void Initialize(HydroponicHarvesterController mono)
        {
            HarvesterController = mono;

            if (FindPots())
            {
                grownPlantsRoot = GameObjectHelpers.FindGameObject(gameObject, "Planters");
            }
        }
        
        public void AddItemToItemsContainer(TechType techType)
        {
            Mod.IsHydroponicKnownTech(techType, out var data);
            StartCoroutine(data.PickType.AddTechTypeToContainerUnSafe(ItemsContainer));
        }

        internal void AddSample(TechType type, int slotID)
        {
            StartCoroutine(AddItemToContainer(type, slotID));
        }

        private bool FindPots()
        {
            try
            {
                var planters = gameObject.transform.Find("model").Find("Planters").transform;

                Slots = new PlantSlot[planters.childCount];

                for (int i = 0; i < planters.childCount; i++)
                {
                    var planter = planters.GetChild(i);
                    var slot = planter.gameObject.AddComponent<PlantSlot>();
                    slot.Initialize(this, i);
                    Slots[i] = slot;
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"[Find Pots] {e.Message}");
                return false;
            }

            return true;
        }

        private PlantSlot GetSlotByID(int slotID)
        {
            return Slots[slotID];
        }

        private void SetSlotOccupiedState(int slotID, bool state)
        {
            GetSlotByID(slotID).IsOccupied = state;
        }
        
        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            return HarvesterController.RemoveItemFromContainer(techType);
        }

        internal void ClearSlot(int slotID)
        {
            var slotByID = GetSlotByID(slotID);

            if (slotByID == null)
            {
                QuickLogger.Debug("SlotById returned null");
                return;
            }
            
            for (int i = slotByID.GetCount() - 1; i >= 0; i--)
            {
                TakeItemFromContainer(slotByID.GetReturnType(),slotByID,false);
            }
            
            SetSlotOccupiedState(slotID, false);
        }

        internal void TakeItemFromContainer(TechType techType, PlantSlot slot, bool giveToPlayer = true)
        {
            QuickLogger.Debug($"TakeItemFromContainer Called Slot: {slot?.Id} || Item: {techType.AsString()}");

            _targetSlot = slot;

            var item = ItemsContainer.RemoveItem(techType);

            if (item != null)
            {
                if (giveToPlayer)
                {
                    if (item.gameObject.GetComponent<Eatable>() != null)
                    {
                        var eatable = item.gameObject.GetComponent<Eatable>();
                        eatable.timeDecayStart = DayNightCycle.main.timePassedAsFloat;
                    }

                    Destroy(item.gameObject);
                    PlayerInteractionHelper.GivePlayerItem(techType);
                }
                else
                {
                    Destroy(item.gameObject);
                }
                _targetSlot = slot;
            }
        }

        internal void OnItemPulledFromStorage(InventoryItem inventoryItem)
        {
            QuickLogger.Debug($"OnItemPulledFromStorage Called Removing {inventoryItem.item.GetTechName()}");
            if (_targetSlot != null)
            {
                _targetSlot.RemoveItem();
            }
            else
            {
                QuickLogger.Debug($"TargetSlot Is Null Trying Again");
                _targetSlot = GetSlotByItem(inventoryItem.item.GetTechType());
                OnItemPulledFromStorage(inventoryItem);
            }

            _targetSlot = null;
        }
        





        internal static EffectType FindEffectType(PlantSlot slotByID)
        {
            if(slotByID == null || slotByID.GetPlantable() == null) return EffectType.None;
            QuickLogger.Debug($"PlantSlot: {slotByID.GetPlantable()?.underwater}", true);
            return slotByID.GetPlantable().underwater ? EffectType.Bubbles : EffectType.Smoke;
        }

        private void AddItem(Plantable plantable, int slotID,TechType techType)
        {
            PlantSlot slotByID = GetSlotByID(slotID);

            if (slotByID == null)
            {
                return;
            }

            if (slotByID.IsOccupied)
            {
                return;
            }

            this.SetSlotOccupiedState(slotID, true);

            GameObject gameObject = plantable.Spawn(slotByID.transform, HarvesterController.IsInBase());

            this.SetupRenderers(gameObject, HarvesterController.IsInBase());
            gameObject.SetActive(true);
            slotByID.SetPlantable(plantable);
            slotByID.PlantModel = gameObject;
            slotByID.SetMaxPlantHeight(MaxPlantsHeight);
            slotByID.SetSeedType(techType);
            HarvesterController.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, true);
            var growingPlant = gameObject.GetComponent<GrowingPlant>();

            if (growingPlant != null)
            {
                var fcsGrowing = gameObject.AddComponent<FCSGrowingPlant>();
                fcsGrowing.Initialize(growingPlant, this, slotByID.SlotBounds.GetComponent<Collider>().bounds.size,
                    null);

                //var pickPrefab = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();

                slotByID.GrowingPlant = fcsGrowing;
                Destroy(growingPlant);
            }

            if (plantable.eatable != null)
            {
                plantable.eatable.SetDecomposes(false);
            }

            HarvesterController.DisplayManager.UpdateUI();
        }

        public void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer);
        }

        public bool IsInBase()
        {
            return HarvesterController.IsInBase();
        }

        public void PerformPurge()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                ClearSlot(i);
                Slots[i].TryClear();
            }

            ItemsContainer.Clear();
        }

        public bool HasItems()
        {
            return Slots?.Any(plantSlot => plantSlot != null && plantSlot.IsOccupied) ?? false;
        }

        public IEnumerator AddItemToContainer(TechType techType, int slotId)
        {
            var itemTask = new TaskResult<InventoryItem>();
            yield return techType.ToInventoryItem(itemTask);
            var item = itemTask.Get();
            if (item != null)
            {
                Plantable component = item.item.GetComponent<Plantable>();
                item.item.SetTechTypeOverride(component.plantTechType);
                item.isEnabled = false;
                AddItem(component, slotId,WorldHelpers.PickReturns[techType].ReturnType);
            }
            else
            {
                QuickLogger.Error($"To inventory item failed to create with techtype {techType}");
            }
            yield break;
        }

        public void SetSpeedMode(HarvesterSpeedModes result)
        {
            QuickLogger.Debug($"Setting SpeedMode to {result}", true);
            foreach (PlantSlot plantSlot in Slots)
            {
                plantSlot.CurrentHarvesterSpeedMode = result;
            }

            HarvesterController.DisplayManager.UpdateUI();
        }

        public bool GetIsConstructed()
        {
            return HarvesterController.IsConstructed;
        }

        public bool HasPowerToConsume()
        {
            return HarvesterController.Manager.HasEnoughPower(HarvesterController.GetPowerUsage());
        }

        public PlantSlot GetSlot(int slotIndex)
        {
            return Slots[slotIndex];
        }

        public HarvesterSpeedModes GetCurrentSpeedMode()
        {
            return Slots[0].CurrentHarvesterSpeedMode;
        }

        public void Load(HydroponicHarvesterDataEntry savedData)
        {
            SetSpeedMode(savedData.HarvesterSpeedMode);

            if (savedData.SlotData.Count != 3) return;

            for (int i = 0; i < savedData.SlotData.Count; i++)
            {
                var slot = Slots[i];
                var data = savedData.SlotData[i];

                if (data.TechType == TechType.None) continue;

                slot.GenerationProgress = data.GenerationProgress;
                slot.SetCount(data.Amount);
                
                slot.GetTab().Load(data.TechType);

                if (slot.GrowingPlant != null)
                {
                    slot.GrowingPlant.SetProgress(data.PlantProgress);
                }
                else
                {
                    QuickLogger.Debug($"Growing Plant was null on load");
                }
            }
        }

        internal void Save(HydroponicHarvesterDataEntry data)
        {
            data.SlotData = new List<SlotsData>
            {
                new SlotsData
                {
                    Amount = Slots[0].GetCount(), TechType = Slots[0].GetPlantSeedTechType(),
                    GenerationProgress = Slots[0].GenerationProgress,
                    PlantProgress = Slots[0].GrowingPlant?.GetProgress() ?? 0
                },
                new SlotsData
                {
                    Amount = Slots[1].GetCount(), TechType = Slots[1].GetPlantSeedTechType(),
                    GenerationProgress = Slots[1].GenerationProgress,
                    PlantProgress = Slots[1].GrowingPlant?.GetProgress() ?? 0
                },
                new SlotsData
                {
                    Amount = Slots[2].GetCount(), TechType = Slots[2].GetPlantSeedTechType(),
                    GenerationProgress = Slots[2].GenerationProgress,
                    PlantProgress = Slots[2].GrowingPlant?.GetProgress() ?? 0
                },
            };
        }
        
        internal PlantSlot GetSlotByItem(TechType techType)
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (Slots[i].IsOccupied && Slots[i].GetReturnType() == techType && Slots[i].HasItems())
                {
                    return GetSlot(i);
                }
            }

            QuickLogger.DebugError($"Failed to find slot by item {techType}",true);
            return null;
        }

        public bool HasSeeds()
        {
            return Slots.Where(plantSlot => plantSlot != null).Any(plantSlot => plantSlot.IsOccupied);
        }
        
        public int GetItemCount(TechType techType)
        {
            return ItemsContainer.GetCount(techType);
        }
    }
}