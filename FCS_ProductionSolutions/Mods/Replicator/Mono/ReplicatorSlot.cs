using System;
using System.Collections.Generic;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.Replicator.Mono
{
    internal class ReplicatorSlot : MonoBehaviour, IFCSStorage
    {
        private readonly IList<float> _progress = new List<float>(new[] { -1f, -1f, -1f });
        private int _itemCount;
        private ReplicatorController _mono;
        public bool IsOccupied => _targetItem != TechType.None;
        private SpeedModes _currentMode;
        private TechType _targetItem;
        private const int MAXCOUNT = 25;
        private const float EnergyConsumption = 15000f;
        internal bool PauseUpdates { get; set; }
        internal bool NotAllowToGenerate => _mono == null || !_mono.IsOperational || PauseUpdates || CurrentSpeedMode == SpeedModes.Off || _targetItem == TechType.None || IsFull;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
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
            ItemsContainer = new ItemsContainer(4,5,transform,"ReplicatorSlot",null);
            ItemsContainer.onRemoveItem += item => { RemoveItem(out _targetItem); };
        }

        internal void ChangeTargetItem(TechType type, bool force = false)
        {
            QuickLogger.Debug($"ChangeTargetItem condition: {IsOccupied && !force}",true);
            if (IsOccupied && !force) return;
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
            if (CurrentSpeedMode == SpeedModes.Off) return 0f;
            var creationTime = Convert.ToSingle(CurrentSpeedMode);
            return EnergyConsumption / creationTime;
        }

        internal bool TryClear()
        {
            QuickLogger.Debug($"Item Count {_itemCount}.", true);
            if (_itemCount > 0) return false;
            
            ChangeTargetItem(TechType.None,true);
            _mono.UpdateUI();
            return true;
        }

        public bool RemoveItem(out TechType techType)
        {
            techType = TechType.None;
            if (_itemCount <= 0) return false;

            if (PlayerInteractionHelper.CanPlayerHold(_targetItem))
            {
                techType = _targetItem;
                _itemCount--;
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
            _itemCount++;
            AddItemToInventory();
            _mono.UpdateUI();
        }

        public int GetContainerFreeSpace { get; }
        public bool IsFull =>_itemCount >= MAXCOUNT;
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
            _itemCount--;
            return ItemsContainer.RemoveItem(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }
        
        public bool ContainsItem(TechType techType)
        {
            return _targetItem == techType && _itemCount > 0;
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

            if (CurrentSpeedMode == SpeedModes.Off || _targetItem == TechType.None)
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
            return _itemCount;
        }

        public void SetItemCount(int amount)
        {
            _itemCount = amount;

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
