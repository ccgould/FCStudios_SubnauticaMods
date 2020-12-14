using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
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
        private HydroponicHarvesterController _mono;
        private const float MaxPlantsHeight = 3f;
        internal PlantSlot[] Slots;
        private TechType _currentItemTech;
        private List<TechType> _activeClones;// TODO Remove is if unnecessary

        internal void Initialize(HydroponicHarvesterController mono)
        {
            _mono = mono;

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
            _mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, false);
            slotByID.Clear();
            SetSlotOccupiedState(slotID, false);
        }

        private static EffectType FindEffectType(PlantSlot slotByID)
        {
            QuickLogger.Debug($"PlantSlot: {slotByID.Plantable.underwater}",true);
            return slotByID.Plantable.underwater ? EffectType.Bubbles : EffectType.Smoke;
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
         
            GameObject gameObject = plantable.Spawn(slotByID.transform, _mono.IsInBase());

            this.SetupRenderers(gameObject, _mono.IsInBase());
            gameObject.SetActive(true);
            slotByID.Plantable = plantable;
            slotByID.PlantModel = gameObject;
            slotByID.SetMaxPlantHeight(MaxPlantsHeight);
            
            _mono.EffectsManager.ChangeEffectState(FindEffectType(slotByID), slotByID.Id, true);
            var growingPlant = gameObject.GetComponent<GrowingPlant>();

            if (growingPlant != null)
            {
                var fcsGrowing = gameObject.AddComponent<FCSGrowingPlant>();
                fcsGrowing.Initialize(growingPlant, this,slotByID.SlotBounds.GetComponent<Collider>().bounds.size, AddActiveClone);
                
                var pickPrefab = growingPlant.grownModelPrefab.GetComponentInChildren<PickPrefab>();

                slotByID.ReturnTechType = pickPrefab != null ? pickPrefab.pickTech : _currentItemTech;

                Destroy(growingPlant);
            }

            if (plantable.eatable != null)
            {
                plantable.eatable.SetDecomposes(false);
            }

        }

        internal void AddActiveClone(TechType techType)
        {
            _activeClones.Add(techType);
        }

        public void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer);
        }

        public GameObject grownPlantsRoot { get; set; }
        public bool IsInBase()
        {
            return _mono.IsInBase();
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
            _mono.DisplayManager.UpdatePowerUsage();
        }

        //public string GetSlotInfo(int slotIndex)
        //{
        //    if (Slots[slotIndex].Plantable != null)
        //    {
        //        var nameOfPlant = Language.main.Get(Slots[slotIndex].Plantable.plantTechType);
        //        if (Slots[slotIndex].GetPlantSeedTechType() != TechType.None)
        //        {
        //            int amount = 0;

        //            if (_storage.ContainsKey(Slots[slotIndex].ReturnTechType))
        //            {
        //                amount = _storage[Slots[slotIndex].ReturnTechType];
        //            }
                    
        //            return $"{nameOfPlant} x{amount}";
        //        }                
        //    }

        //    return "N/A";
        //}

        public bool GetIsConstructed()
        {
            return _mono.IsConstructed;
        }

        public bool HasPowerToConsume()
        {
            return _mono.Manager.HasEnoughPower(_mono.GetPowerUsage());
        }

        public PlantSlot GetSlot(int slotIndex)
        {
            return Slots[slotIndex];
        }

        public SpeedModes GetCurrentSpeedMode()
        {
            return Slots[0].CurrentSpeedMode;
        }
    }
}