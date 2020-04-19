using System;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvPowerManager : MonoBehaviour
    {
        private HydroHarvController _mono;
        private PowerRelay _connectedRelay;
        private float _energyToConsume;
        private float AvailablePower => ConnectedRelay.GetPower();
        
        private PowerRelay ConnectedRelay
        {
            get
            {
                if (_mono != null && _mono.IsConnectedToBase)
                {
                    while (_connectedRelay == null)
                        UpdatePowerRelay();
                }
                return _connectedRelay;
            }
        }

        private void Update()
        {
            if (_mono == null || !_mono.IsConnectedToBase) return;
            _energyToConsume = EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
            _mono.DisplayManager.UpdatePowerUsagePerSecond();
        }

        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(transform);
            if (relay != null && relay != _connectedRelay)
            {
                _connectedRelay = relay;
                QuickLogger.Debug("PowerRelay found at last!");
            }
            else
            {
                _connectedRelay = null;
            }
        }

        internal void Initialize(HydroHarvController mono)
        {
            _mono = mono;
        }
        
        internal float GetEnergyToConsume()
        {
            return _energyToConsume;
        }

        internal bool HasPowerToConsume()
        {
            bool requiresEnergy = GameModeUtils.RequiresPower();
            return !requiresEnergy || (this.AvailablePower >= _energyToConsume);
        }

        internal void ConsumePower()
        {
            bool requiresEnergy = GameModeUtils.RequiresPower();
            if (requiresEnergy)
                this.ConnectedRelay.ConsumeEnergy(_energyToConsume, out float amountConsumed);
        }

        internal void UpdateEnergyPerSecond(SpeedModes currentMode)
        {
            if (currentMode != SpeedModes.Off)
            {
                CreationTime = Convert.ToSingle(currentMode);
                EnergyConsumptionPerSecond = QPatch.Configuration.Config.EnergyCost / CreationTime;
            }
            else
            {
                EnergyConsumptionPerSecond = 0f;
            }
        }

        internal float EnergyConsumptionPerSecond { get; private set; }

        internal float CreationTime { get; set; }
    }
}
