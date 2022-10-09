using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Interfaces;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Managers
{
    internal class PortManager : MonoBehaviour, IShippingDestination
    {
        private string _prefabIdent;
        private readonly Dictionary<string, IDroneDestination> _dronePorts = new();
        private BaseManager _baseManager;
        public BaseManager Manager => GetManager();

        private BaseManager GetManager()
        {
            if (_baseManager is null)
            {
                _baseManager = BaseManager.FindManager(_prefabIdent);
            }

            return _baseManager;
        }

        private void Start()
        {
            _prefabIdent = gameObject.GetComponent<PrefabIdentifier>().id;
        }

        internal void RegisterDronePort(IDroneDestination port)
        {
            if(!_dronePorts.ContainsKey(port.GetPrefabID()))
            {
                _dronePorts.Add(port.GetPrefabID(),port);
            }
        }

        internal void UnRegisterDronePort(IDroneDestination port)
        {
            if (_dronePorts.ContainsKey(port.GetPrefabID()))
            {
                _dronePorts.Remove(port.GetPrefabID());
            }
        }
        
        internal Dictionary<string, IDroneDestination> GetPorts()
        {
            return _dronePorts;
        }

        public string GetBaseID()
        {
            return _prefabIdent;
        }

        public IDroneDestination FindPort(int dockedPortId)
        {
            return (from port in _dronePorts where port.Value.GetPortID() == dockedPortId select port.Value).FirstOrDefault();
        }

        public AlterraDronePortController FindPort(string prefabId)
        {
            QuickLogger.Debug($"Port Count:{_dronePorts.Count} | Port ID: {prefabId}");
            foreach (KeyValuePair<string, IDroneDestination> port in _dronePorts)
            {
                QuickLogger.Debug($"Checking: {port.Key}");
                if (port.Key.Equals(prefabId.Trim(),StringComparison.OrdinalIgnoreCase))
                {
                    QuickLogger.Debug("Target Port Found", true);
                    return (AlterraDronePortController)port.Value;
                }
            }

            return null;
        }

        public AlterraDronePortController GetOpenPort()
        {
            foreach (KeyValuePair<string, IDroneDestination> port in _dronePorts)
            {
                if (!port.Value.IsOccupied) return (AlterraDronePortController)port.Value;
            }

            return null;
        }

        public bool HasAccessPoint()
        {
            return _dronePorts.Any();
        }

        public string GetStatus()
        {
            return !HasAccessPoint() ? "Depot Not Built At Base" : "Ready";
        }

        public bool HasDronePort => Manager?.IsDeviceBuilt(Mod.DronePortPadHubNewTabID) ?? false;
        public bool HasContructor { get; }
        public bool IsVehicle { get; set; }
        public bool SendItemsToConstructor(List<CartItem> pendingItem)
        {
            return false;
        }

        public string GetBaseName()
        {
            return Manager?.GetBaseName() ?? "Station Port";
        }
    }
}
