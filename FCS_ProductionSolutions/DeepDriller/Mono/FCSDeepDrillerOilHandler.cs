using System;
using System.Collections.Generic;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_ProductionSolutions.DeepDriller.Buildable;
using FCSCommon.Converters;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.DeepDriller.Mono
{
    internal class FCSDeepDrillerOilHandler : MonoBehaviour, IFCSStorage
    {
        private FCSDeepDrillerController _mono;
        public int GetContainerFreeSpace { get; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public bool IsFull { get; }
        private const float KDayInSeconds = 1200f;
        private readonly float _setOilTime = KDayInSeconds * QPatch.DeepDrillerMk3Configuration.OilTimePeriodInDays;
        private float _timeLeft;
        private float _elapsed;
        internal Action OnOilUpdate { get; set; }

        private void Update()
        {
            if (_mono == null || _mono.DeepDrillerPowerManager == null) return;
            if (QPatch.DeepDrillerMk3Configuration.HardCoreMode)
            {
                if (_mono.DeepDrillerPowerManager.IsPowerAvailable() && _timeLeft > 0)
                {
                    _timeLeft -= DayNightCycle.main.deltaTime;
                    if (_timeLeft < 0)
                    {
                        _timeLeft = 0;
                    }
                }

                _elapsed += DayNightCycle.main.deltaTime;

                if (_elapsed >= 1f)
                {
                    _elapsed %= 1f;
                    OnOilUpdate?.Invoke();
                }
            }
        }

        internal void Initialize(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _timeLeft = 0f;
        }

        internal void SetOilTimeLeft(float amount)
        {
            _timeLeft = amount;
        }

        internal float GetOilTimeLeft()
        {
            return _timeLeft;
        }
        
        internal void ReplenishOil()
        {
            _timeLeft = Mathf.Clamp(_timeLeft + (KDayInSeconds * QPatch.DeepDrillerMk3Configuration.OilRestoresInDays), 0, _setOilTime);
        }

        internal float GetOilPercent()
        {
            return QPatch.DeepDrillerMk3Configuration.HardCoreMode ?  _timeLeft / _setOilTime : 1f;
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            //return _timeLeft + KDayInSeconds <= _setOilTime;
            return _timeLeft + KDayInSeconds * amount <= _setOilTime;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            ReplenishOil();
            Destroy(item.item.gameObject);
            return true;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (pickupable.GetTechType() != TechType.Lubricant)
            {
                QuickLogger.Message(FCSDeepDrillerBuildable.NotAllowedItem(),true);
                return false;
            }

            if (!CanBeStored(_mono.OilDumpContainer.GetCount(), TechType.Lubricant))
            {   
                QuickLogger.Message(FCSDeepDrillerBuildable.OilTankNotFormatEmpty(TimeTilRefuel()), true);
                return false;
            }

            return true;
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        private string TimeTilRefuel()
        {
            var mod = _timeLeft % KDayInSeconds;
            return TimeConverters.SecondsToHMS(mod);
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            if (_timeLeft - KDayInSeconds >= 0)
            {
                return techType.ToPickupable();
            }

            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            if (Mathf.Approximately(_timeLeft,0)) return null;

            var result = _timeLeft / KDayInSeconds;
            int amount = Convert.ToInt32(Math.Floor(result));

            return new Dictionary<TechType, int>
            {
                {TechType.Lubricant,amount}
            };
        }

        public bool ContainsItem(TechType techType)
        {
            return techType == TechType.Lubricant && !(_timeLeft < KDayInSeconds);
        }

        internal bool HasOil()
        {
            return !QPatch.DeepDrillerMk3Configuration.HardCoreMode || _timeLeft > 0;
        }
    }
}
