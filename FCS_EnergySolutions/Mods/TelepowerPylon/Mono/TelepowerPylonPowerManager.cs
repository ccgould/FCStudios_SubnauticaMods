﻿using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<string,TelepowerPylonController> _connections = new Dictionary<string, TelepowerPylonController>();
        private bool _pauseUpdates;


        private void UpdateConnections()
        {
            if (_mono != null && _mono.Manager != null && _mono.IsConstructed && _powerRelay != null && !_pauseUpdates)
            {

                if (_connectedPowerSource == null)
                {
                    _connectedPowerSource = _mono.Manager.Habitat.powerRelay;
                }

                foreach (var connection in _connections)
                {
                    if (connection.Value.GetPowerRelay() == null) continue;
                    _connectedPowerSource.AddInboundPower(connection.Value.GetPowerRelay());
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
            if(!_connections.ContainsKey(controller.UnitID.ToLower()))
                _connections?.Add(controller.UnitID.ToLower(),controller);
        }

        public IPowerInterface GetPowerRelay()
        {
            return _connectedPowerSource;
        }

        public bool HasConnections()
        {
            return _connections.Any();
        }

        public void RemoveConnection(string id)
        {
            if (!_connections.ContainsKey(id.ToLower())) return;
            _pauseUpdates = true;
            _connectedPowerSource.RemoveInboundPower(_connections[id.ToLower()].GetPowerRelay());
            _connections.Remove(id.ToLower());
            _pauseUpdates = false;
        }
    }
}