using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvPowerManager : MonoBehaviour
    {
        private HydroHarvController _mono;
        private PowerRelay _connectedRelay;
        private float _energyToConsume;
        private float AvailablePower => this.ConnectedRelay.GetPower();
        
        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }

        internal Action OnPowerAvaliable { get; set; }

        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(this.transform);
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

        private void Update()
        {
            _energyToConsume =  _mono.EnergyConsumptionPerSecond * DayNightCycle.main.deltaTime;
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
    }
}
