using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono;
using FCS_ProductionSolutions.Structs;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.HydroponicHarvester.Models
{
    internal class PlantSlot : MonoBehaviour
    {
        private const float EnergyConsumption = 15000f;
        private Plantable _plantable { get; set; }
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private TechType _returnTechType;
        private SlotItemTab _trackedTab;
        private TechType _seedTech;
        
        public int Id;
        public bool IsOccupied;
        private HarvesterSpeedModes _currentHarvesterMode;
        public GameObject PlantModel;
        private const int MaxCapacity = 50;
        public bool PauseUpdates { get; set; }
        public bool IsFull => _count >= MaxCapacity;
        internal GameObject SlotBounds { get; set; }
        internal GrowBedManager GrowBedManager;
        private int _count;

        public bool NotAllowToGenerate()
        {
            return GrowBedManager == null || !GrowBedManager.GetIsConstructed() || PauseUpdates|| CurrentHarvesterSpeedMode == HarvesterSpeedModes.Off || _plantable == null;
        }

        internal float GenerationProgress
        {
            get => _progress[(int)ClonePhases.Generating];
            set => _progress[(int)ClonePhases.Generating] = value;
        }
        
        internal HarvesterSpeedModes CurrentHarvesterSpeedMode
        {
            get => _currentHarvesterMode;
            set
            {
                HarvesterSpeedModes previousMode = _currentHarvesterMode;
                _currentHarvesterMode = value;

                if (_currentHarvesterMode != HarvesterSpeedModes.Off)
                {
                    if (previousMode == HarvesterSpeedModes.Off)
                        TryStartingNextClone();
                }
            }
        }

        public FCSGrowingPlant GrowingPlant { get; set; }

        private void Update()
        {
            if (NotAllowToGenerate())
                return;
            
            var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

            if (!GrowBedManager.HasPowerToConsume())
                return;
            
            if (GenerationProgress >= EnergyConsumption)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generated Clone", true);
                PauseUpdates = true;
                GenerationProgress = -1f;
                SpawnClone();
                TryStartingNextClone();
                PauseUpdates = false;
            }
            else if (GenerationProgress >= 0f)
            {
                // Is currently generating clone
                GenerationProgress = Mathf.Min(EnergyConsumption, GenerationProgress + energyToConsume);
            }
        }

        private float CalculateEnergyPerSecond()
        {
            if (CurrentHarvesterSpeedMode == HarvesterSpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentHarvesterSpeedMode);
            return EnergyConsumption / creationTime;
        }
        
        private void Clear()
        {
            if (PlantModel != null)
            {
                GameObject.Destroy(PlantModel);
            }

            if (_plantable != null  && _plantable.linkedGrownPlant != null)
            {
                GameObject.Destroy(_plantable.linkedGrownPlant.gameObject);
            }

            _count = 0;
            GrowBedManager.HarvesterController.EffectsManager.ChangeEffectState(GrowBedManager.FindEffectType(this), Id, false);
            IsOccupied = false;
            _seedTech = TechType.None;
            _returnTechType = TechType.None;
            GrowingPlant = null;
            PlantModel = null;
            _plantable = null;
            _trackedTab.Clear();
        }

        internal bool TryClear(bool forceClear = false, bool clearContainer = false)
        {
            if (!forceClear && _count > 0)
            {
                GrowBedManager.HarvesterController.DisplayManager.ShowMessage(AuxPatchers.PleaseClearHarvesterSlot());
                return false;
            }

            if (clearContainer)
            {
                GrowBedManager.ClearSlot(GetTab().Slot.Id);
            }

            Clear();

            return true;
        }
        
        public TechType GetPlantSeedTechType()
        {
            return _seedTech;
        }
        
        public void SetMaxPlantHeight(float height)
        {

            if (height <= 0f || _plantable == null)
            {
                return;
            }
            _plantable.SetMaxPlantHeight(height - transform.localPosition.y);
        }

        public bool RemoveItem()
        {
            _count--;
            TryStartingNextClone();
            _trackedTab?.UpdateCount();
            return true;
        }

        public bool CanRemoveItem() => _count > 0;

        public void AddItem()
        {
            if(IsFull) return;
            _count++;
            GrowBedManager.AddItemToItemsContainer(GetPlantSeedTechType());
            _trackedTab?.UpdateCount();
        }
        
        public TechType GetReturnType()
        {
            return _returnTechType;
        }

        internal void SpawnClone()
        {
            AddItem();
        }

        private void TryStartingNextClone()
        {
            QuickLogger.Debug("Trying to start another clone", true);

            if (CurrentHarvesterSpeedMode == HarvesterSpeedModes.Off)
                return;// Powered off, can't start a new clone

            if (IsFull)
            {
                QuickLogger.Debug("Cannot start another clone, container is full", true);
                return;
            }

            if (Mathf.Approximately( GenerationProgress, -1f)) 
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
                GenerationProgress = 0f;
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone", true);
            }
        }

        public int GetCount()
        {
            return _count;
        }

        public int GetMaxCapacity()
        {
            return MaxCapacity;
        }

        public void TrackTab(SlotItemTab slotItemTab)
        {
            _trackedTab = slotItemTab;
        }

        public void Initialize(GrowBedManager growBedManager, int id)
        {
            GrowBedManager = growBedManager;
            Id = id;
            SlotBounds = gameObject.FindChild("PlanterBounds");
            PlantModel = transform.GetChild(0).gameObject;
        }
        
        public SlotItemTab GetTab()
        {
            return _trackedTab;
        }

        internal void SetPlantable(Plantable plantable)
        {
            _plantable = plantable;
        }

        internal Plantable GetPlantable()
        {
            return _plantable;
        }

        public void SetSeedType(TechType currentItemTech)
        {
            Mod.IsHydroponicKnownTech(currentItemTech, out FCSDNASampleData data);
            _seedTech = currentItemTech;
            _returnTechType = data.PickType;
        }
        
        public void SetCount(int count)
        {
            _count = count;
        }

        public bool HasItems()
        {
            return GetCount() > 0;
        }
    }
}
