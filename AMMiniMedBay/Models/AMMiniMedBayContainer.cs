using AMMiniMedBay.Buildable;
using AMMiniMedBay.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AMMiniMedBay.Models
{
    internal class AMMiniMedBayContainer
    {
        private readonly ItemsContainer _medBayContainer;

        private readonly ChildObjectIdentifier _containerRoot;
        private readonly GameObject _medKit = CraftData.GetPrefabForTechType(TechType.FirstAidKit);
        private readonly Func<bool> _isConstructed;
        private readonly AMMiniMedBayController _mono;
        private float _timeSpawnMedKit = -1f;

        //TODO Change to 600f
        private const float MedKitSpawnInterval = 10f; //600f;
        public bool startWithMedKit;
        private const int ContainerWidth = 2;
        private const int ContainerHeight = 2;
        public int NumberOfCubes
        {
            get => _medBayContainer.count;
            set
            {
                if (value < 0 || value > MaxContainerSlots)
                    return;

                if (value < _medBayContainer.count)
                {
                    do
                    {
                        RemoveSingleKit();
                    } while (value < _medBayContainer.count);
                }
                else if (value > _medBayContainer.count)
                {
                    do
                    {
                        SpawnKit();
                    } while (value > _medBayContainer.count);
                }
            }
        }

        private void RemoveSingleKit()
        {
            IList<InventoryItem> kit = _medBayContainer.GetItems(TechType.PrecursorIonCrystal);
            _medBayContainer.RemoveItem(kit[0].item);
        }

        private void SpawnKit()
        {
            var medKit = GameObject.Instantiate(_medKit);
            var newInventoryItem = new InventoryItem(medKit.GetComponent<Pickupable>().Pickup(false));
            _medBayContainer.UnsafeAdd(newInventoryItem);
            _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
        }

        private int MaxContainerSlots => ContainerHeight * ContainerWidth;
        private int ContainerSlotsFilled => _medBayContainer.count;
        public bool IsContainerFull => _medBayContainer.count == MaxContainerSlots || !_medBayContainer.HasRoomFor(1, 1);

        public Action OnTimerEnd { get; set; }

        public Action<string> OnTimerUpdate { get; set; }

        public Action OnPDAClosedAction { get; set; }

        public Action OnPDAOpenedAction { get; set; }

        public AMMiniMedBayContainer(AMMiniMedBayController mono)
        {
            _isConstructed = () => { return mono.IsConstructed; };

            if (_containerRoot == null)
            {
                QuickLogger.Debug("Initializing Filter StorageRoot");
                var storageRoot = new GameObject("FilterStorageRoot");
                storageRoot.transform.SetParent(mono.transform, false);
                _containerRoot = storageRoot.AddComponent<ChildObjectIdentifier>();
                _mono = mono;
            }

            if (_medBayContainer == null)
            {
                QuickLogger.Debug("Initializing Filter Container");

                _medBayContainer = new ItemsContainer(ContainerWidth, ContainerHeight, _containerRoot.transform,
                    AMMiniMedBayBuildable.StorageLabel(), null);

                _medBayContainer.isAllowedToAdd += IsAllowedToAdd;
                _medBayContainer.isAllowedToRemove += IsAllowedToRemove;

                _medBayContainer.onAddItem += mono.OnAddItemEvent;
                _medBayContainer.onRemoveItem += mono.OnRemoveItemEvent;

                _medBayContainer.onAddItem += OnAddItemEvent;
                _medBayContainer.onRemoveItem += OnRemoveItemEvent;
            }

            _mono.OnMonoUpdate += OnMonoUpdate;

            DayNightCycle main = DayNightCycle.main;

            if (_timeSpawnMedKit < 0.0 && main)
            {
                this._timeSpawnMedKit = (float)(main.timePassed + (!this.startWithMedKit ? (double)MedKitSpawnInterval : 0.0));
            }
        }

        private void OnMonoUpdate()
        {
            if (!_mono.PowerManager.GetIsPowerAvailable() || !_mono.IsConstructed) return;

            if (IsContainerFull) return;

            DayNightCycle main = DayNightCycle.main;

            if (main.timePassed > _timeSpawnMedKit)
            {
                NumberOfCubes++;
            }
        }
        private void OnRemoveItemEvent(InventoryItem item)
        {
            QuickLogger.Debug("Resetting Time", true);
            _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
        }

        private void OnAddItemEvent(InventoryItem item)
        {
            if (item != null)
            {
                QuickLogger.Debug("New Health Pack Generated Added!", true);
            }
        }

        //TODO Remove if not needed
        private void TimerEnd()
        {
            OnTimerEnd?.Invoke();
        }

        //TODO Remove if not needed
        private void TimerTick(string obj)
        {
            OnTimerUpdate?.Invoke(obj);
        }

        private bool IsAllowedToRemove(Pickupable pickupable, bool verbose)
        {
            return true;
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return false;
        }

        public void OpenStorage()
        {
            if (!_isConstructed.Invoke())
                return;

            Player main = Player.main;
            PDA pda = main.GetPDA();
            Inventory.main.SetUsedStorage(_medBayContainer, false);
            pda.Open(PDATab.Inventory, null, OnPDAClose, 4f);
            OnPDAOpenedAction?.Invoke();
        }

        private void OnPDAClose(PDA pda)
        {
            OnPDAClosedAction?.Invoke();
        }

        public bool GetIsEmpty()
        {
            return _medBayContainer.count == 0;
        }
    }
}
