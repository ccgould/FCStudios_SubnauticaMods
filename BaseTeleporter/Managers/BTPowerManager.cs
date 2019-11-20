using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AE.BaseTeleporter.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Utilities;

namespace AE.BaseTeleporter.Managers
{
    internal class BTPowerManager 
    {
        private float _powerUsage = 75.0f; //TODO Add to configuration
        private PowerRelay _connectedRelay;
        private FCSController _mono;

        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }

        public BTPowerManager(FCSController mono)
        {
            _mono = mono;
            UpdatePowerRelay();
        }

        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(_mono.transform);
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
        
        internal bool TakePower()
        {
            bool requiresEnergy = GameModeUtils.RequiresPower();
            bool powerAvailable = !requiresEnergy ||ConnectedRelay != null && ConnectedRelay.GetPower() >= _powerUsage * 2;
            
            if (powerAvailable)
            {
                ConnectedRelay.ConsumeEnergy(_powerUsage * 2, out var amountConsumed);
                QuickLogger.Debug($"Consumed {amountConsumed} amount of power for this operation");
                return true;
            }

            return false;
        }

        public float NeededPower()
        {
            return _powerUsage * 2;
        }
    }
}
