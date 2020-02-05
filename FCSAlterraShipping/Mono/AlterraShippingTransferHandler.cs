using FCSAlterraShipping.Models;
using FCSCommon.Converters;
using FCSCommon.Utilities;
using System;
using System.Collections;
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
        private float _currentTime = 1f;
        private AlterraShippingTarget _mono;
        private bool _done;
        private float _botSpeedInKmh = 40f;
        private readonly List<Pickupable> _itemsToRemove = new List<Pickupable>();
        private const float ResetTime = 1f;


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
            return _target == null ? string.Empty : _target.GetPrefabIdentifier();
        }

        internal void SetCurrentTarget(string target)
        {
            StartCoroutine(FindTarget(target));
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
                _currentTime = GetTime(_target);
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

                ErrorMessage.AddMessage($"[{_target.Name} Message]: Shipment has arrived!");

               _itemsToRemove.Clear();
                _target.Recieved = true;
                _currentTime = ResetTime;
                _mono.OnItemSent?.Invoke();
                _target.OnItemSent?.Invoke();
                _target.IsReceivingTransfer = false;

                _items = null;
                _target = null;

            }
            else
            {
                _currentTime -= 1 * DayNightCycle.main.deltaTime;
            }

            if (_target != null) _target.OnTimerChanged?.Invoke(TimeConverters.SecondsToHMS(_currentTime));
            _mono.OnTimerChanged?.Invoke(TimeConverters.SecondsToHMS(_currentTime));
        }


        private float GetTime(AlterraShippingTarget target)
        {
            var dist = Vector3.Distance(target.transform.position, _mono.transform.position);
            QuickLogger.Debug($"Distance to other: {dist}", true);
            var km = MeterToKilometer(dist);
            var time = DistanceToTime(km, _botSpeedInKmh);
            return HourToSeconds(time);
        }

        private float MeterToKilometer(float distance)
        {
            return distance / 1000f;
        }

        private float DistanceToTime(float distance,float speed)
        {
            return distance / speed;
        }

        private float HourToSeconds(float time)
        {
            return time * 3600;
        }

        private IEnumerator FindTarget(string currentTarget)
        {
            while (!_done && _currentTime > 0 && _target == null)
            {
                if (string.IsNullOrEmpty(currentTarget)) break;

                foreach (var target in ShippingTargetManager.GlobalShippingTargets)
                {
                    QuickLogger.Debug($"Target: {target.GetInstanceID()} located");
                    if (target.GetPrefabIdentifier() == currentTarget)
                    {
                        _target =  target;
                    }
                }

                QuickLogger.Debug("No Target Found!", true);

                yield return null;
            }
        }
    }
}
