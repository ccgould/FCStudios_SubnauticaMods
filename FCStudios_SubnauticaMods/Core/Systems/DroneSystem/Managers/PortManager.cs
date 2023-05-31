﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Core.Systems.DroneSystem.Managers
{
    internal class PortManager : MonoBehaviour, IShippingDestination
    {
        private string _prefabIdent;
        private readonly Dictionary<string, IDroneDestination> _dronePorts = new();
        private readonly Dictionary<string, AlterraHubConstructorController> _alterraHubConstructors = new();
        private BaseManager _baseManager;
        private DroneController _inboundDrone;
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
            if (!_dronePorts.ContainsKey(port.GetPrefabID()))
            {
                _dronePorts.Add(port.GetPrefabID(), port);
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

        public IDroneDestination FindPort(string prefabId)
        {
            QuickLogger.Debug($"PortManager Count:{_dronePorts.Count} | PortManager ID: {prefabId}");
            foreach (KeyValuePair<string, IDroneDestination> port in _dronePorts)
            {
                QuickLogger.Debug($"Checking: {port.Key}");
                if (port.Key.Equals(prefabId.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    QuickLogger.Debug("Target PortManager Found", true);
                    return port.Value;
                }
            }

            return null;
        }

        public IDroneDestination GetOpenPort()
        {
            foreach (KeyValuePair<string, IDroneDestination> port in _dronePorts)
            {
                if (!port.Value.IsOccupied) return port.Value;
            }

            return null;
        }

        public bool HasAccessPoint()
        {
            return _dronePorts.Any() || HasContructor;
        }

        public string GetStatus()
        {
            return !HasAccessPoint() ? "Depot Not Built At Base" : "Ready";
        }

        public bool HasDronePort => Manager?.IsDeviceBuilt(Mod.DronePortPadHubNewTabID) ?? false;
        public bool HasContructor => _alterraHubConstructors.Any();
        public bool IsVehicle { get; set; }
        public bool SendItemsToConstructor(List<CartItemSaveData> pendingItem)
        {
            var constructors = _alterraHubConstructors.FirstOrDefault(x => x.Value.IsConstructed);
            return constructors.Value.ShipItems(pendingItem);
        }

        public string GetBaseName()
        {
            return Manager?.GetBaseName() ?? "Station PortManager";
        }

        public void SetInboundDrone(DroneController droneController)
        {
            _inboundDrone = droneController;
        }

        public IDroneDestination ActivePort()
        {
            return _dronePorts.FirstOrDefault().Value;
        }

        public string GetPreFabID()
        {
            return _baseManager.BaseID;
        }

        public IEnumerator SpawnDrone(TaskResult<DroneController> itemResult)
        {
            foreach (KeyValuePair<string, IDroneDestination> destination in _dronePorts)
            {
                yield return destination.Value.SpawnDrone(controller =>
                {
                    itemResult.Set(controller);
                });
            }

            yield break;
        }

        public bool ContainsPort(IDroneDestination port)
        {
            return _dronePorts.ContainsValue(port);
        }

        public bool ContainsPort(IShippingDestination port)
        {
            return _dronePorts.ContainsValue(port.ActivePort());
        }

        public void RegisterConstructor(AlterraHubConstructorController constructor)
        {
            if (!_alterraHubConstructors.ContainsKey(constructor.GetPrefabID()))
            {
                _alterraHubConstructors.Add(constructor.GetPrefabID(), constructor);
            }
        }

        public void UnRegisterConstructor(AlterraHubConstructorController constructor)
        {
            _alterraHubConstructors.Remove(constructor.GetPrefabID());
        }
    }
}