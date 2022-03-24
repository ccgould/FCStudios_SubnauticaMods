using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem;
using FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces;
using UnityEngine;

namespace FCS_AlterraHub.Managers
{
    internal class PortManager : MonoBehaviour
    {
        private string _prefabIdent;
        private readonly Dictionary<string, IDroneDestination> _dronePorts = new();

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

        public AlterraDronePortController GetOpenPort()
        {
            foreach (KeyValuePair<string, IDroneDestination> port in _dronePorts)
            {
                if (!port.Value.IsOccupied) return (AlterraDronePortController)port.Value;
            }

            return null;
        }
    }
}
