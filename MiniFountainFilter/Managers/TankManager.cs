using AE.MiniFountainFilter.Mono;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using System;
using UnityEngine;

namespace AE.MiniFountainFilter.Managers
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

        private readonly float _tankCapacity = QPatch.Configuration.Config.TankCapacity;
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
            TankLevel = Mathf.Clamp(TankLevel + amount, 0, _tankCapacity);
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
                QuickLogger.Debug($"Tank Level: {TankLevel} || Tank Level {playerWaterRequest}", true);

                return;
            }

            var playerO2RequestRemainder = Mathf.Min(TankLevel, playerWaterRequest);
            RemoveWater(TankLevel - playerO2RequestRemainder);
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
                AddWater(QPatch.Configuration.Config.WaterPerSecond);
            }
        }
    }
}
