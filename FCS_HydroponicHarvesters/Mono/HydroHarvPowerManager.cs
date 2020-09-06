using System;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Interfaces;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvPowerManager : MonoBehaviour,IFCSPowerManager
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
                EnergyConsumptionPerSecond = _mono.EnergyCost / CreationTime;
            }
            else
            {
                EnergyConsumptionPerSecond = 0f;
            }
        }

        internal float EnergyConsumptionPerSecond { get; private set; }

        internal float GetEnergyPerSecond()
        {
            return Mathf.Round(EnergyConsumptionPerSecond);
        }

        internal float CreationTime { get; set; }
        public float GetPowerUsagePerSecond()
        {
            return EnergyConsumptionPerSecond;
        }

        public float GetDevicePowerCharge()
        {
            return 0f;
        }

        public float GetDevicePowerCapacity()
        {
            return 0f;
        }

        public FCSPowerStates GetPowerState()
        {
            return HasPowerToConsume() ? FCSPowerStates.Powered : FCSPowerStates.Unpowered;
        }

        public void TogglePowerState()
        {
     
        }

        public void SetPowerState(FCSPowerStates state)
        {

        }

        public bool IsDevicePowerFull()
        {
            return true;
        }

        public bool ModifyPower(float amount, out float consumed)
        {
            consumed = 0f;
            return false;
        }
    }
}
