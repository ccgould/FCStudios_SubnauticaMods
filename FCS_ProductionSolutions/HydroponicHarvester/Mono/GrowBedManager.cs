using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
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
        private List<TechType> _activeClones;
        internal PlantSlot[] Slots;
        private TechType _currentItemTech;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        
        internal void Initialize(HydroponicHarvesterController mono)
        {
            _mono = mono;

            if (FindPots())
            {
                _activeClones = new List<TechType>();
                grownPlantsRoot = GameObjectHelpers.FindGameObject(gameObject, "Planters");
            }
        }
        
        internal void AddSample(TechType type,int slotID)
        {
            var item = type.ToInventoryItem();
            if (item != null)
            {
                AddItemToContainer(item,slotID);
                //Destroy(item.item.gameObject);
            }
            else
            {
                QuickLogger.Error($"To inventory item failed to create with techtype {type}");
            }
        }

        internal void ShowDisplay()
        {

        }

        internal void HideDisplay()
        {

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

        private int GetFreeSlotID()
        {
            for (int i = 0; i < Slots.Length; i++)
            {
                if (!Slots[i].IsOccupied)
                {
                    return i;
                }
            }

            return 0;
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

        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount, TechType techType = TechType.None)
        {
            if (IsFull) return false;
            return true;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            throw new NotImplementedException();
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

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var plantable = pickupable.gameObject.GetComponentInChildren<Plantable>();
            if (plantable  != null && !plantable.isSeedling) return false;
            return CanBeStored(1);
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            foreach (PlantSlot plantSlot in Slots)
            {
                if (plantSlot.GetReturnType() == techType)
                {
                    if (plantSlot.RemoveItem())
                    {
                        return techType.ToPickupable();
                    }
                }
            }
            return null;
        }

        internal void RemoveSample(TechType techType)
        {
            _activeClones.Remove(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return Slots.Any(plantSlot => plantSlot.GetReturnType() == techType);
        }
        
        public void SetSpeedMode(SpeedModes result)
        {
            QuickLogger.Debug($"Setting SpeedMode to {result}",true);
            foreach (PlantSlot plantSlot in Slots)
            {
                plantSlot.CurrentSpeedMode = result;
            }
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
    }
}
