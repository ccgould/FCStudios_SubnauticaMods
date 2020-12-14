using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using UnityEngine;
namespace FCS_ProductionSolutions.HydroponicHarvester.Models
{
    internal class PlantSlot : MonoBehaviour
    {
        internal GameObject SlotBounds { get; set; }
        internal Plantable Plantable { get; set; }
        public int Id;
        public bool IsOccupied;
        private SpeedModes _currentMode;
        public GameObject PlantModel;
        private const int MaxCapacity = 50;
        public bool PauseUpdates { get; set; }
        public bool IsFull => _itemCount >= MaxCapacity;


        internal const float CooldownComplete = 19f;
        internal const float StartUpComplete = 4f;
        public bool NotAllowToGenerate => PauseUpdates || !GrowBedManager.GetIsConstructed() || CurrentSpeedMode == SpeedModes.Off || Plantable == null;
        internal float StartUpProgress
        {
            get => _progress[(int)ClonePhases.StartUp];
            set => _progress[(int)ClonePhases.StartUp] = value;
        }

        internal float GenerationProgress
        {
            get => _progress[(int)ClonePhases.Generating];
            set => _progress[(int)ClonePhases.Generating] = value;
        }

        internal float CoolDownProgress
        {
            get => _progress[(int)ClonePhases.CoolDown];
            set => _progress[(int)ClonePhases.CoolDown] = value;
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
        
        public TechType ReturnTechType
        {
            get => _returnTechType;
            set
            {
                _returnTechType = value;
            }
        }
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private TechType _returnTechType;
        internal GrowBedManager GrowBedManager;
        private int _itemCount;
        private SlotItemTab _trackedTab;
        
        private void Update()
        {
            //QuickLogger.Debug($"PauseUpdates: {PauseUpdates} || IsConstructed: {_mono.IsConstructed} || CurrentSpeedMode: {_mono.CurrentSpeedMode}",true);

            if (NotAllowToGenerate)
                return;

            var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

            //TODO Deal with Power
            if (!GrowBedManager.HasPowerToConsume())
                return;

            if (CoolDownProgress >= CooldownComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Finished Cooldown", true);

                PauseUpdates = true;
                SpawnClone();
                // Finished cool down - See if the next clone can be started                
                TryStartingNextClone();

                PauseUpdates = false;
            }
            else if (CoolDownProgress >= 0f)
            {
                // Is currently cooling down
                CoolDownProgress = Mathf.Min(CooldownComplete, CoolDownProgress + DayNightCycle.main.deltaTime);
            }
            else if (GenerationProgress >= QPatch.Configuration.EnergyConsumpion)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Cooldown", true);

                // Finished generating clone - Start cool down
                CoolDownProgress = 0f;
            }
            else if (GenerationProgress >= 0f)
            {
                // Is currently generating clone
                GenerationProgress = Mathf.Min(QPatch.Configuration.EnergyConsumpion, GenerationProgress + energyToConsume);
            }
            else if (StartUpProgress >= StartUpComplete)
            {
                QuickLogger.Debug("[Hydroponic Harvester] Generating", true);
                // Finished start up - Start generating clone
                GenerationProgress = 0f;
            }
            else if (StartUpProgress >= 0f)
            {
                // Is currently in start up routine
                StartUpProgress = Mathf.Min(StartUpComplete, StartUpProgress + DayNightCycle.main.deltaTime);
            }
        }

        private float CalculateEnergyPerSecond()
        {
            if (CurrentSpeedMode == SpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentSpeedMode);
            return QPatch.Configuration.EnergyConsumpion / creationTime;
        }

        public void Clear()
        {
            GameObject.Destroy(PlantModel);

            if (Plantable.linkedGrownPlant)
            {
                GameObject.Destroy(Plantable.linkedGrownPlant.gameObject);
            }
            PlantModel = null;
            Plantable = null;
        }
        
        public void ShowPlant()
        {
            var model = GameObject.Instantiate(PlantModel);
            model.transform.SetParent(transform,false);
            model.transform.localPosition = Vector3.zero;
        }

        public TechType GetPlantSeedTechType()
        {
            if (Plantable != null)
            {
                return Plantable.pickupable.GetTechType();
            }

            return TechType.None;
        }

        public void SetMaxPlantHeight(float height)
        {
            if (height <= 0f || Plantable == null)
            {
                return;
            }
            Plantable.SetMaxPlantHeight(height - transform.localPosition.y);
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

            if (StartUpProgress < 0f || // Has not started a clone yet
                CoolDownProgress == CooldownComplete) // Has finished a clone
            {
                if (!IsFull)
                {
                    QuickLogger.Debug("[Hydroponic Harvester] Start up", true);
                    CoolDownProgress = -1f;
                    GenerationProgress = -1f;
                    StartUpProgress = 0f;
                }
                else
                {
                    QuickLogger.Debug("Cannot start another clone, container is full", true);
                }
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone, another clone is currently in progress", true);
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
    }
}
