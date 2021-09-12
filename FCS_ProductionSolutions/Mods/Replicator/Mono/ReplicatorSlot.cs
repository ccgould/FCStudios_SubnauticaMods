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
    internal class ReplicatorSlot : MonoBehaviour, IFCSStorage
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
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
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
            ItemsContainer = new ItemsContainer(4,5,transform,"ReplicatorSlot",null);
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

        internal bool TryClear()
        {
            QuickLogger.Debug($"Item Count {ItemsContainer.count}.", true);
            if (ItemsContainer.count > 0) return false;
            
            ChangeTargetItem(TechType.None,true);
            _mono.UpdateUI();
            return true;
        }

        public bool RemoveItem(out TechType techType)
        {
            techType = TechType.None;
            
            if (ItemsContainer.count <= 0) return false;

            if (PlayerInteractionHelper.CanPlayerHold(_targetItem))
            {
                ItemsContainer.RemoveItem(_targetItem);
                techType = _targetItem;
                TryStartingNextClone();
                _mono.UpdateUI();
                return true;
            }

            QuickLogger.ModMessage(AuxPatchers.InventoryFull());
            return false;
        }
        
        public void AddItem()
        {
            if(IsFull) return;
            AddItemToInventory();
            _mono.UpdateUI();
        }

        public int GetContainerFreeSpace { get; }
        public bool IsFull =>ItemsContainer.count >= MAXCOUNT;
        public bool CanBeStored(int amount, TechType techType)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType)
        {
            return ItemsContainer.RemoveItem(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return new(){{ _targetItem ,ItemsContainer.count} };
        }
        
        public bool ContainsItem(TechType techType)
        {
            return _targetItem == techType && ItemsContainer.count > 0;
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return ItemsContainer?.count ?? 0;
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

        public int GetCount()
        {
            return ItemsContainer.count;
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
 StartCoroutine(AsyncExtensions.AddToContainerAsync(_targetItem, ItemsContainer));
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
