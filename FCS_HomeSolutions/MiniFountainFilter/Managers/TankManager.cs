using System;
using FCS_AlterraHub.Enumerators;
using FCS_HomeSolutions.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.MiniFountainFilter.Managers
{
    internal class TankManager
    {
        private float _tankLevel;

        public float TankLevel
        {
            get => _tankLevel;
            set
            {
                _tankLevel = value;
                OnTankUpdate?.Invoke();
            }
        }

        private readonly float _tankCapacity = QPatch.Configuration.MiniFountainFilterTankCapacity;
        private MiniFountainFilterController _mono;
        public Action OnTankUpdate { get; set; }

        internal void Initialize(MiniFountainFilterController mono)
        {
            _mono = mono;
            mono.OnMonoUpdate += OnMonoUpdate;
        }

        private void OnMonoUpdate()
        {
            if (!_mono.GetIsOperational()) return;

            GenerateWater();
        }

        internal void AddWater(float amount)
        {
            if (TankLevel >= _tankCapacity) return;
            TankLevel = Mathf.Clamp(TankLevel + (amount * DayNightCycle.main.deltaTime), 0, _tankCapacity);
            OnTankUpdate?.Invoke();
        }

        internal void RemoveWater(float amount)
        {
            if (TankLevel <= 0) return;

            TankLevel = Mathf.Clamp(TankLevel - amount, 0, _tankCapacity);
            OnTankUpdate?.Invoke();
        }

        internal void GivePlayerWater()
        {
            QuickLogger.Debug($"Current Tank Level: {TankLevel}");
            if (TankLevel <= 0) return;

            var manager = _mono.PlayerManager;

            var playerWaterRequest = manager.PlayerMaxWater - manager.GetPlayerWaterLevel();

            QuickLogger.Debug($"Taking: {playerWaterRequest}", true);

            if (TankLevel >= playerWaterRequest)
            {
                RemoveWater(playerWaterRequest);
                manager.AddWaterToPlayer(Mathf.Abs(playerWaterRequest));
                QuickLogger.Debug($"Tank Level: {TankLevel} || Player Water Request {playerWaterRequest}", true);
                return;
            }

            var playerO2RequestRemainder = Mathf.Min(TankLevel, playerWaterRequest);
            RemoveWater(playerO2RequestRemainder);
            manager.AddWaterToPlayer(Mathf.Abs(playerO2RequestRemainder));
            QuickLogger.Debug($"Tank Level: {TankLevel} || Player Request Remainder {playerO2RequestRemainder}", true);
        }

        internal void SetTankLevel(float amount)
        {
            if (TankLevel >= _tankCapacity) return;
            TankLevel = Mathf.Clamp(amount, 0, _tankCapacity);
        }

        internal float GetTankLevel()
        {
            return TankLevel;
        }

        internal float GetTankPercentage()
        {
            return Mathf.FloorToInt(TankLevel / _tankCapacity * 100);
        }

        internal float GetTankPercentageDec()
        {
            return TankLevel / _tankCapacity;
        }

        internal void GenerateWater()
        {
            if (_mono.PowerManager.GetPowerState() == FCSPowerStates.Powered)
            {
                AddWater(QPatch.Configuration.MiniFountainFilterWaterPerSecond);
            }
        }

        internal bool HasEnoughWater(float amount)
        {
            return _tankLevel >= amount;
        }

        internal bool IsFull()
        {
            return _tankLevel >= _tankCapacity;
        }
    }
}
