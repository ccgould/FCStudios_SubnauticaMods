using System;
using System.Collections.Generic;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.Replicator.Mono
{
    internal class ReplicatorSlot : MonoBehaviour
    {
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private int _itemCount;
        private ReplicatorController _mono;
        public bool IsOccupied => _targetItem != TechType.None;
        private SpeedModes _currentMode;
        private TechType _targetItem;
        private const int MAXCOUNT = 25;
        internal bool PauseUpdates { get; set; }
        internal bool NotAllowToGenerate => _mono == null || !_mono.IsOperational || PauseUpdates || CurrentSpeedMode == SpeedModes.Off || _targetItem == TechType.None || IsFull;
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

        public void Initialize(ReplicatorController mono)
        {
            _mono = mono;
        }
        
        internal void ChangeTargetItem(TechType type)
        {
            if (IsOccupied) return;
            _targetItem = type;
            GenerationProgress = 0;
            
        }

        internal float GetPercentageDone()
        {
            if(Mathf.Approximately(GenerationProgress,-1f))
            {
                return 0f;
            }
            return GenerationProgress/QPatch.Configuration.EnergyConsumpion;
        }

        private void Update()
        {
            if (NotAllowToGenerate)
                return;
            
            var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

            if (!_mono.Manager.HasEnoughPower(_mono.GetPowerUsage()))
                return;
            
            if (GenerationProgress >= QPatch.Configuration.EnergyConsumpion)
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
                GenerationProgress = Mathf.Min(QPatch.Configuration.EnergyConsumpion, GenerationProgress + energyToConsume);
            }
        }

        private float CalculateEnergyPerSecond()
        {
            if (CurrentSpeedMode == SpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentSpeedMode);
            return QPatch.Configuration.EnergyConsumpion / creationTime;
        }

        internal bool TryClear()
        {
            if (_itemCount > 0) return false;
            _targetItem = TechType.None;
            return true;
        }

        public bool RemoveItem(out TechType techType)
        {
            techType = TechType.None;
            if (_itemCount <= 0) return false;
            techType = _targetItem;
            _itemCount--;
            TryStartingNextClone();
            _mono.UpdateUI();
            return true;
        }

        public void AddItem()
        {
            if(IsFull) return;
            _itemCount++;
            _mono.UpdateUI();
        }

        public bool IsFull =>_itemCount >= MAXCOUNT;

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

        public void SetItemCount(int amount)
        {
            _itemCount = amount;
        }

        public int GetMaxCount()
        {
           return MAXCOUNT;
        }

        public TechType GetTargetItem()
        {
            return _targetItem;
        }
    }
}
