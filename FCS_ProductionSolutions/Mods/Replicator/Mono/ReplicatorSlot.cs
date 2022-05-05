using System;
using System.Collections.Generic;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Mods.HydroponicHarvester.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.Replicator.Mono
{
    internal class ReplicatorSlot : FCSStorage
    {
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private ReplicatorController _mono;
        public bool IsOccupied => _targetItem != TechType.None;
        private HarvesterSpeedModes _currentHarvesterMode;
        private TechType _targetItem;
        private const int MAXCOUNT = 25;
        private const float EnergyConsumption = 15000f;
        internal bool PauseUpdates { get; set; }
        internal bool NotAllowToGenerate => _mono == null || !_mono.IsOperational || PauseUpdates || CurrentHarvesterSpeedMode == HarvesterSpeedModes.Off || _targetItem == TechType.None || IsFull;
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

        public void Initialize(ReplicatorController mono)
        {
            _mono = mono;
            ItemsContainer.onRemoveItem += item =>
            {
                TryStartingNextClone();
                _mono.UpdateUI();
            };
        }

        internal void ChangeTargetItem(TechType type, bool force = false)
        {
            QuickLogger.Debug($"ChangeTargetItem condition: {IsOccupied && !force}",true);
            if (IsOccupied && !force)
            {
                QuickLogger.Message(AuxPatchers.PleaseClearReplicatorSlot(),true);
                return;
            }
            QuickLogger.Debug($"Changing target item {type}", true);
            _targetItem = type;
            GenerationProgress = 0;
        }

        internal float GetPercentageDone()
        {
            if(Mathf.Approximately(GenerationProgress,-1f))
            {
                return 0f;
            }
            return GenerationProgress/EnergyConsumption;
        }

        private void Update()
        {
            if (NotAllowToGenerate)
                return;
            
            var energyToConsume = CalculateEnergyPerSecond() * DayNightCycle.main.deltaTime;

            if (!_mono.Manager.HasEnoughPower(_mono.GetPowerUsage()))
                return;
            
            if (GenerationProgress >= EnergyConsumption)
            {
                QuickLogger.Debug("[Replicator] Generated Clone", true);
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

        internal bool TryClear(bool force = false)
        {
            QuickLogger.Debug($"Item Count {ItemsContainer.count}.", true);

            if (!force && ItemsContainer.count > 0)
            {
                _mono.ShowMessage(AuxPatchers.PleaseClearReplicatorSlot());
                return false;
            }

            if (force)
            {
                ItemsContainer.Clear();
            }

            ChangeTargetItem(TechType.None,true);
            _mono.UpdateUI();
            return true;
        }

        public Pickupable RemoveItem()
        {
           if (ItemsContainer.count <= 0) return null;

            if (PlayerInteractionHelper.CanPlayerHold(_targetItem))
            {
                
                TryStartingNextClone();
                _mono.UpdateUI();
                return ItemsContainer.RemoveItem(_targetItem); ;
            }

            QuickLogger.ModMessage(AuxPatchers.InventoryFull());
            return null;
        }
        
        public void AddItem()
        {
            if(IsFull) return;
            AddItemToInventory();
            _mono.UpdateUI();
        }
        
        internal void SpawnClone()
        {
            AddItem();
        }

        private void TryStartingNextClone()
        {
            QuickLogger.Debug("Trying to start another clone", true);

            if (CurrentHarvesterSpeedMode == HarvesterSpeedModes.Off || _targetItem == TechType.None)
                return;// Powered off, can't start a new clone

            if (!IsFull && GenerationProgress == -1f)
            {
                QuickLogger.Debug("[Replicator] Generating", true);
                GenerationProgress = 0f;
            }
            else
            {
                QuickLogger.Debug("Cannot start another clone, container is full", true);
            }
        }


        public void SetItemCount(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                AddItemToInventory();
            }
        }

        private void AddItemToInventory()
        {
#if SUBNAUTICA_STABLE
            ItemsContainer.UnsafeAdd(_targetItem.ToInventoryItemLegacy());
#else
            StartCoroutine(_targetItem.AddTechTypeToContainerUnSafe(ItemsContainer));
#endif
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
