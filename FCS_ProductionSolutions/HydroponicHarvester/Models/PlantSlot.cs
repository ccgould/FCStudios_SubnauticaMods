using System;
using System.Collections.Generic;
using FCS_AlterraHub.Model;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Converters;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;
namespace FCS_ProductionSolutions.HydroponicHarvester.Models
{
    internal class PlantSlot : MonoBehaviour
    {
        internal GameObject SlotBounds { get; set; }
        private const float EnergyConsumption = 15000f;
        private Plantable _plantable { get; set; }
        public int Id;
        public bool IsOccupied;
        private SpeedModes _currentMode;
        public GameObject PlantModel;
        private const int MaxCapacity = 50;
        public bool PauseUpdates { get; set; }
        public bool IsFull => _itemCount >= MaxCapacity;

        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private TechType _returnTechType;
        internal GrowBedManager GrowBedManager;
        private int _itemCount;
        private SlotItemTab _trackedTab;
        private TechType _seedTech;

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

            GrowBedManager.Mono.EffectsManager.ChangeEffectState(GrowBedManager.FindEffectType(this),Id,false);
            IsOccupied = false;
            _seedTech = TechType.None;
            GrowingPlant = null;
            PlantModel = null;
            _plantable = null;
        }

        internal bool TryClear()
        {
            if (_itemCount > 0) return false;
            Clear();
            return true;
        }

        public void ShowPlant()
        {
            var model = GameObject.Instantiate(PlantModel);
            model.transform.SetParent(transform,false);
            model.transform.localPosition = Vector3.zero;
        }

        public TechType GetPlantSeedTechType()
        {
            return _seedTech;
        }

        public TechType GetPlantTechType()
        {
            return _plantable != null ? _plantable.plantTechType : TechType.None;
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
            if (_itemCount <= 0) return false;
            _itemCount--;
            TryStartingNextClone();
            _trackedTab?.UpdateCount();
            return true;
        }

        public void AddItem()
        {
            if(IsFull) return;
            _itemCount++;
            _trackedTab?.UpdateCount();
            GrowBedManager.IncreaseByOne(GetPlantSeedTechType());
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
            return _itemCount;
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

        public void SetItemCount(int amount)
        {
            _itemCount = amount;
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
            _seedTech = currentItemTech;
        }
    }
}
