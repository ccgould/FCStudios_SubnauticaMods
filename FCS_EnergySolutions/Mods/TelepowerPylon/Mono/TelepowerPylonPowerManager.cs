using System.Collections.Generic;
using FCS_EnergySolutions.Mods.TelepowerPylon.Model;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.TelepowerPylon.Mono
{
    internal class TelepowerPylonPowerManager : MonoBehaviour
    {
        private bool _isInitialized;
        private TelepowerPylonController _mono;
        private PowerRelay _powerRelay;
        private PowerRelay _connectedPowerSource;
        private readonly HashSet<TelepowerPylonController> _connections = new HashSet<TelepowerPylonController>();


        private void UpdateConnections()
        {
            if (_mono != null && _mono.Manager != null && _mono.IsConstructed && _powerRelay != null)
            {

                if (_connectedPowerSource == null)
                {
                    _connectedPowerSource = _mono.Manager.Habitat.powerRelay;
                }
                
                foreach (TelepowerPylonController connection in _connections)
                {
                    if(connection.GetPowerRelay() == null) continue;
                    _connectedPowerSource.AddInboundPower(connection.GetPowerRelay());
                }
            }
        }

        public void Initialize(TelepowerPylonController mono)
        {
            if (_isInitialized) return;
            _mono = mono;
            _powerRelay = gameObject.GetComponent<PowerRelay>();
            
            if (_powerRelay == null)
            {
                QuickLogger.Error("TelepowerPylonPower could not find PowerRelay", this);
                return;
            }
            
            InvokeRepeating(nameof(UpdateConnections),1f,1f);

            _isInitialized = true;
        }
        
        public void AddConnection(TelepowerPylonController controller)
        {
            _connections?.Add(controller);
        }

        public IPowerInterface GetPowerRelay()
        {
            return _connectedPowerSource;
        }
    }
}
