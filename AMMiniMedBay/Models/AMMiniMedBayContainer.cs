using AMMiniMedBay.Buildable;
using FCSCommon.Models.Abstract;
using FCSCommon.Utilities;
using System;
using UnityEngine;

namespace AMMiniMedBay.Models
{
    internal class AMMiniMedBayContainer
    {
        private readonly ItemsContainer _medBayContainer;

        private readonly ChildObjectIdentifier _containerRoot;
        private readonly GameObject _medKit = CraftData.GetPrefabForTechType(TechType.FirstAidKit);
        private readonly Func<bool> _isConstructed;
        private readonly FCSController _mono;
        private float _timeSpawnMedKit = -1f;
        private const float MedKitSpawnInterval = 600f;
        public bool startWithMedKit;
        private const int ContainerWidth = 2;
        private const int ContainerHeight = 2;

        private int MaxContainerSlots => ContainerHeight * ContainerWidth;
        private int ContainerSlotsFilled => _medBayContainer.count;
        private bool IsSlotAvailable => _medBayContainer.HasRoomFor(1, 1);

        public Action OnTimerEnd { get; set; }

        public Action<string> OnTimerUpdate { get; set; }

        public Action OnPDAClosedAction { get; set; }

        public Action OnPDAOpenedAction { get; set; }

        public AMMiniMedBayContainer(FCSController mono)
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
            //TODO Add check for if item removed to prevent constant call
            if (!IsSlotAvailable) return;

            DayNightCycle main = DayNightCycle.main;

            if (main.timePassed > _timeSpawnMedKit)
            {
                //TODO add code to update player on
                var medKit = GameObject.Instantiate(_medKit);
                var newInventoryItem = new InventoryItem(medKit.GetComponent<Pickupable>().Pickup(false));
                _medBayContainer.UnsafeAdd(newInventoryItem);
                _timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + MedKitSpawnInterval;
            }
        }

        private void OnRemoveItemEvent(InventoryItem item)
        {

        }

        private void OnAddItemEvent(InventoryItem item)
        {
            if (item != null)
            {
                QuickLogger.Debug("New Health Pack Generated Added!", true);
                //TODO Add amount of packaged available
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
