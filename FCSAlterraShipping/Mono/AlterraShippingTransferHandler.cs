using FCSAlterraShipping.Models;
using FCSCommon.Converters;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FCSAlterraShipping.Mono
{
    internal class AlterraShippingTransferHandler : MonoBehaviour
    {
        private Constructable _buildable;
        private AlterraShippingTarget _target;
        private ItemsContainer _items;
        private float _currentTime;
        private const float WaitTime = 120f;
        private AlterraShippingTarget _mono;
        private bool _done;
        private readonly List<Pickupable> _itemsToRemove = new List<Pickupable>();


        internal float GetCurrentTime()
        {
            return _currentTime;
        }

        internal void SetCurrentTime(float time)
        {
            _currentTime = time;
        }

        internal string GetCurrentTarget()
        {
            if (_target == null) return String.Empty;
            return _target.GetPrefabIdentifier();
        }

        internal void SetCurrentTarget(string target)
        {
            _target = FindTarget(target);
        }

        internal void SetMono(AlterraShippingTarget mono)
        {
            _mono = mono;
        }

        internal void SetCurrentItems(ItemsContainer items)
        {
            _items = items;
        }
        private void Awake()
        {
            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>();
            }

            _currentTime = WaitTime;
        }

        private void Update()
        {
            if (!_buildable.constructed)
            {
                QuickLogger.Debug($"Not Built");
                return;
            }

            //QuickLogger.Debug($"T: {_target}, I: {_items}, D: {_done}", true);

            if (_target == null || _items == null || _done) return;

            PendTransfer();


        }

        internal void SendItems(ItemsContainer items, AlterraShippingTarget target)
        {
            QuickLogger.Debug($"Starting Transfer to {target.GetInstanceID()}", true);
            QuickLogger.Debug($"Target Found: {target.Name}", true);
            QuickLogger.Debug($"Target IsReceivingTransfer: {target.IsReceivingTransfer}", true);

            if (_mono == null)
            {
                _mono = gameObject.GetComponent<AlterraShippingTarget>();
            }

            if (!target.IsReceivingTransfer)
            {
                QuickLogger.Debug($"Starting Transfer to {target.Name}", true);
                _target = target;
                _items = items;
                _done = false;
                _target.IsReceivingTransfer = true;
                _target.OnReceivingTransfer?.Invoke();
            }

        }

        private void PendTransfer()
        {
            if (Mathf.CeilToInt(_currentTime) == 0)
            {
                _done = true;

                QuickLogger.Debug($"Done: {_done}");

                var listOfItems = _items.ToList();

                for (int i = 0; i < _items.count; i++)
                {
                    _itemsToRemove.Add(listOfItems[i].item);
                }

                foreach (Pickupable pickupable in _itemsToRemove)
                {
                    _mono.RemoveItem(pickupable);
                    _target.AddItem(new InventoryItem(pickupable));
                }

                ErrorMessage.AddMessage($"{_target.Name} shipment has arrived!");

                _itemsToRemove.Clear();
                _target.Recieved = true;
                _currentTime = WaitTime;
                _mono.OnItemSent?.Invoke();
                _target.OnItemSent?.Invoke();
                _target.IsReceivingTransfer = false;

                _items = null;
                _target = null;

            }
            else
            {
                _currentTime = Mathf.Clamp(_currentTime - 1 * DayNightCycle.main.deltaTime, 0, WaitTime);
            }

            if (_target != null) _target.OnTimerChanged?.Invoke(TimeConverters.SecondsToHMS(_currentTime));
            _mono.OnTimerChanged?.Invoke(TimeConverters.SecondsToHMS(_currentTime));
        }

        //TODO Remove if not needed
        private AlterraShippingTarget FindTarget(string currentTarget)
        {
            if (currentTarget == String.Empty) return null;

            foreach (var target in ShippingTargetManager.GlobalShippingTargets)
            {
                QuickLogger.Debug($"Target: {target.GetInstanceID()} located");
                if (target.GetPrefabIdentifier() == currentTarget)
                {
                    return target;
                }
            }

            QuickLogger.Debug("No Target Found!", true);

            return null;
        }
    }
}
