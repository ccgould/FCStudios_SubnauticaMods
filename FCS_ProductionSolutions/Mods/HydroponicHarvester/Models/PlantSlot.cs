using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Mono;
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
        private SpeedModes _currentMode;
        public GameObject PlantModel;
        private const int MaxCapacity = 50;
        public bool PauseUpdates { get; set; }
        public bool IsFull => GrowBedManager.GetItemCount(_returnTechType) >= MaxCapacity;
        internal GameObject SlotBounds { get; set; }
        internal GrowBedManager GrowBedManager;
        private int _count;

        public bool NotAllowToGenerate()
        {
            return GrowBedManager == null || !GrowBedManager.GetIsConstructed() || PauseUpdates|| CurrentSpeedMode == SpeedModes.Off || _plantable == null;
        }

        internal float GenerationProgress
        {
            get => _progress[(int)ClonePhases.Generating];
            set => _progress[(int)ClonePhases.Generating] = value;
        }
        
        internal SpeedModes CurrentSpeedMode
        {
            get => _currentMode;
            set
            {
                SpeedModes previousMode = _currentMode;
                _currentMode = value;

                if (_currentMode != SpeedModes.Off)
                {
                    if (previousMode == SpeedModes.Off)
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
            if (CurrentSpeedMode == SpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentSpeedMode);
            return EnergyConsumption / creationTime;
        }
        
        public void Clear()
        {
            if (PlantModel != null)
            {
                GameObject.Destroy(PlantModel);
            }

            if (_plantable.linkedGrownPlant != null)
            {
                GameObject.Destroy(_plantable.linkedGrownPlant.gameObject);
            }

            GrowBedManager.HarvesterController.EffectsManager.ChangeEffectState(GrowBedManager.FindEffectType(this),Id,false);
            IsOccupied = false;
            _seedTech = TechType.None;
            _returnTechType = TechType.None;
            GrowingPlant = null;
            PlantModel = null;
            _plantable = null;
        }

        internal bool TryClear()
        {
            if (GrowBedManager.GetItemCount(_returnTechType) > 0)
            {
                QuickLogger.Message(AuxPatchers.PleaseEmptyHarvesterSlot(), true);
                return false;
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

        public bool CanRemoveItem() => _count > 0; //GrowBedManager.GetItemCount(_returnTechType) > 0;

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

            if (CurrentSpeedMode == SpeedModes.Off)
                return;// Powered off, can't start a new clone

            if (!IsFull && GenerationProgress == -1f)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
                GenerationProgress = 0f;
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone, container is full", true);
            }
        }

        public int GetCount()
        {
            return _count; //GrowBedManager.GetItemCount(_returnTechType);
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
            Mod.IsHydroponicKnownTech(currentItemTech, out DNASampleData data);
            _seedTech = currentItemTech;
            _returnTechType = data.PickType;
        }

        public void DeductCount()
        {
            _count--;
            _trackedTab?.UpdateCount();
        }

        public void SetCount(int count)
        {
            _count = count;
        }
    }
}
