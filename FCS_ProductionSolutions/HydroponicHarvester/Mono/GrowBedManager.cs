using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Models;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class GrowBedManager : MonoBehaviour, IFCSGrowBed
    {
        internal HydroponicHarvesterController Mono;
        private const float MaxPlantsHeight = 3f;
        private TechType _currentItemTech;


        internal PlantSlot[] Slots;
        public GameObject grownPlantsRoot { get; set; }

        internal void Initialize(HydroponicHarvesterController mono)
        {
            Mono = mono;

            if (FindPots())
            {
                grownPlantsRoot = GameObjectHelpers.FindGameObject(gameObject, "Planters");
            }
        }
        
        internal void AddSample(TechType type,int slotID)
        {
            var item = type.ToInventoryItem();
            if (item != null)
            {
                AddItemToContainer(item,slotID);
            }
            else
            {
                QuickLogger.Error($"To inventory item failed to create with techtype {type}");
            }
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
                    slot.Initialize(this,i);
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

        internal void RemoveItem(int slotID)
        {
            var slotByID = GetSlotByID(slotID);

            if (slotByID == null)
            {
                QuickLogger.Debug("SlotById returned null");
                return;
            }
            Mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, false);
            slotByID.Clear();
            SetSlotOccupiedState(slotID, false);
        }

        internal static EffectType FindEffectType(PlantSlot slotByID)
        {
            QuickLogger.Debug($"PlantSlot: {slotByID.GetPlantable().underwater}",true);
            return slotByID.GetPlantable().underwater ? EffectType.Bubbles : EffectType.Smoke;
        }

        private void AddItem(Plantable plantable, int slotID)
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
         
            GameObject gameObject = plantable.Spawn(slotByID.transform, Mono.IsInBase());

            this.SetupRenderers(gameObject, Mono.IsInBase());
            gameObject.SetActive(true);
            slotByID.SetPlantable(plantable);
            slotByID.PlantModel = gameObject;
            slotByID.SetMaxPlantHeight(MaxPlantsHeight);
            slotByID.SetSeedType(_currentItemTech);
            Mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, true);
            var growingPlant = gameObject.GetComponent<GrowingPlant>();

            if (growingPlant != null)
            {
                var fcsGrowing = gameObject.AddComponent<FCSGrowingPlant>();
                fcsGrowing.Initialize(growingPlant, this,slotByID.SlotBounds.GetComponent<Collider>().bounds.size, null);
                
                var pickPrefab = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();

                slotByID.GrowingPlant = fcsGrowing;
                slotByID.ReturnTechType = pickPrefab != null ? pickPrefab.pickTech : _currentItemTech;

                Destroy(growingPlant);
            }

            if (plantable.eatable != null)
            {
                plantable.eatable.SetDecomposes(false);
            }

        }

        public void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer);
        }

        public bool IsInBase()
        {
            return Mono.IsInBase();
        }

        public void ClearGrowBed()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                RemoveItem(i);
            }
        }

        public bool HasItems()
        {
            return Slots?.Any(plantSlot => plantSlot != null && plantSlot.IsOccupied) ?? false;
        }

        public bool AddItemToContainer(InventoryItem item, int slotId)
        {
            _currentItemTech = item.item.GetTechType();
            Plantable component = item.item.GetComponent<Plantable>();
            item.item.SetTechTypeOverride(component.plantTechType);
            item.isEnabled = false;
            AddItem(component, slotId);
            return true;
        }

        public void SetSpeedMode(SpeedModes result)
        {
            QuickLogger.Debug($"Setting SpeedMode to {result}",true);
            foreach (PlantSlot plantSlot in Slots)
            {
                plantSlot.CurrentSpeedMode = result;
            }
            Mono.DisplayManager.UpdateUI();
        }

        public bool GetIsConstructed()
        {
            return Mono.IsConstructed;
        }

        public bool HasPowerToConsume()
        {
            return Mono.Manager.HasEnoughPower(Mono.GetPowerUsage());
        }

        public PlantSlot GetSlot(int slotIndex)
        {
            return Slots[slotIndex];
        }

        public SpeedModes GetCurrentSpeedMode()
        {
            return Slots[0].CurrentSpeedMode;
        }

        public void Load(HydroponicHarvesterDataEntry savedData)
        {
            SetSpeedMode(savedData.SpeedMode);
            
            if (savedData.SlotData.Count != 3) return;
            for (int i = 0; i < savedData.SlotData.Count; i++)
            {
                var slot = Slots[i];
                var data = savedData.SlotData[i];
                if(data.TechType == TechType.None) continue;
                slot.GenerationProgress = data.GenerationProgress;
                slot.SetItemCount(data.Amount);
                slot.GetTab().SetIcon(data.TechType);
                slot.GrowingPlant?.SetProgress(data.PlantProgress);

            }
        }

        internal void Save(HydroponicHarvesterDataEntry data)
        {
            data.SlotData = new List<SlotsData>
            {
                new SlotsData{Amount = Slots[0].GetCount(),TechType = Slots[0].GetPlantSeedTechType(), GenerationProgress = Slots[0].GenerationProgress,PlantProgress = Slots[0].GrowingPlant?.GetProgress() ?? 0},
                new SlotsData{Amount = Slots[1].GetCount(),TechType = Slots[1].GetPlantSeedTechType(), GenerationProgress = Slots[1].GenerationProgress,PlantProgress = Slots[1].GrowingPlant?.GetProgress() ?? 0},
                new SlotsData{Amount = Slots[2].GetCount(),TechType = Slots[2].GetPlantSeedTechType(), GenerationProgress = Slots[2].GenerationProgress,PlantProgress = Slots[2].GrowingPlant?.GetProgress() ?? 0},
            };
        }

        public void TakeItem(TechType techType,int slotId)
        {
            if (PlayerInteractionHelper.CanPlayerHold(techType))
            {
                var slot = GetSlotByID(slotId);
                if (slot.GetPlantSeedTechType() != techType) return;
                if (slot.RemoveItem())
                {
                    PlayerInteractionHelper.GivePlayerItem(slot.ReturnTechType);
                }
            }
            else
            {
                QuickLogger.ModMessage(AuxPatchers.InventoryFull());
            }
        }
    }
}