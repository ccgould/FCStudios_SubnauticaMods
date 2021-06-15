using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DockTriggerController : MonoBehaviour
    {
        private AlterraDronePortController _port;
        private DroneController _drone;

        private void Awake()
        {
            _port = GetComponentInParent<AlterraDronePortController>();
            if (_port == null)
            {
                QuickLogger.Error("Failed to fine port controller in dock trigger");
            }
        }
        private void OnTriggerStay(Collider collider)
        {
            var drone = collider.gameObject.name.StartsWith("AlterraTransportDrone");
            if (!drone) return;
            GetDroneController(collider);
            _drone.SetPort(_port);
            IsDocked = true;
        }

        private void GetDroneController(Collider collider)
        {
            if (_drone == null)
            {
                _drone = collider.GetComponentInChildren<DroneController>();
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            var drone = collider.gameObject.name.StartsWith("AlterraTransportDrone");
            
            if (!drone) return;
            GetDroneController(collider);
            _drone.SetPort(_port);
        }

        private void OnTriggerExit(Collider collider)
        {
            var drone = collider.gameObject.name.StartsWith("AlterraTransportDrone");
            
            if (!drone) return;
            IsDocked = false;
            GetDroneController(collider);
            _drone.SetPort(null);
            _drone = null;
        }

        public bool IsDocked { get; set; }
    }
}