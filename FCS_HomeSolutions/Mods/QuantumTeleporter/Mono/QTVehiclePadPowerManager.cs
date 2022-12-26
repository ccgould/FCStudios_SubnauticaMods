using FCS_AlterraHub.Helpers;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Enumerators;
using FCS_HomeSolutions.Mods.QuantumTeleporter.Interface;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.QuantumTeleporter.Mono
{
    internal class QTVehiclePadPowerManager : MonoBehaviour, IQTPower
    {
        private PowerRelay _connectedRelay;
        private readonly float _interPowerUsage = Main.Configuration.QuantumTeleporterGlobalTeleportPowerUsage;

        public void Initialize()
        {
            UpdatePowerRelay();
        }

        private PowerRelay ConnectedRelay
        {
            get
            {
                while (_connectedRelay == null)
                    UpdatePowerRelay();

                return _connectedRelay;
            }
        }

        private void UpdatePowerRelay()
        {
            PowerRelay relay = PowerSource.FindRelay(gameObject.transform);
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

        public bool TakePower(QTTeleportTypes tab)
        {
            if (HasEnoughPower(QTTeleportTypes.Global))
            {
                ConnectedRelay.ConsumeEnergy(_interPowerUsage, out var amountConsumed);
                return true;
            }

            return false;
        }

        public bool HasEnoughPower(QTTeleportTypes selectedTab)
        {
            bool requiresEnergy = UWEHelpers.RequiresPower();

            if (!requiresEnergy) return true;

            return ConnectedRelay != null && ConnectedRelay.GetPower() >= _interPowerUsage;
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