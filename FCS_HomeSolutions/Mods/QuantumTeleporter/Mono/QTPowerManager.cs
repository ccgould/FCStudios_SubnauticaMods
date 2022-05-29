using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal class QTPowerManager : IQTPower
    {
        private readonly float _interPowerUsage = QPatch.Configuration.QuantumTeleporterGlobalTeleportPowerUsage;
        private readonly float _intraPowerUsage = QPatch.Configuration.QuantumTeleporterInternalTeleportPowerUsage;
        private PowerRelay _connectedRelay;
        private readonly QuantumTeleporterController _mono;

        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }

        public QTPowerManager(QuantumTeleporterController mono)
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
        
        public bool TakePower(QTTeleportTypes type)
        {
            QuickLogger.Debug($"Available power {ConnectedRelay?.GetPower()}",true);

            if (HasEnoughPower(type))
            {
                float amountConsumed = 0;
                switch (type)
                {
                    case QTTeleportTypes.Global:
                        ConnectedRelay.ConsumeEnergy(_interPowerUsage, out amountConsumed);
                        QuickLogger.Debug($"Consumed {amountConsumed} amount of power for this operation", true);

                        return true;
                    case QTTeleportTypes.Intra:
                        ConnectedRelay.ConsumeEnergy(_intraPowerUsage, out amountConsumed);
                        QuickLogger.Debug($"Consumed {amountConsumed} amount of power for this operation", true);
                        return true;
                }
            }

            return false;
        }

        public bool HasEnoughPower(QTTeleportTypes type)
        {
            bool requiresEnergy = GameModeUtils.RequiresPower();

            if (!requiresEnergy) return true;

            switch (type)
            {
                case QTTeleportTypes.Global:
                     return ConnectedRelay != null && ConnectedRelay.GetPower() >= _interPowerUsage;

                case QTTeleportTypes.Intra:
                    return ConnectedRelay != null && ConnectedRelay.GetPower() >= _intraPowerUsage;
                    
            }
            return false;
        }

        public float PowerAvailable()
        {
            return Mathf.RoundToInt(_connectedRelay.GetPower());
        }

        public void FullReCharge()
        {
            
        }

        public void SetCharge(float charge)
        {
            
        }

        public void ModifyCharge(float amount)
        {
            
        }

        public bool IsFull()
        {
            return false;
        }
    }
}
