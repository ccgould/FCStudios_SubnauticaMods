using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem
{
    internal class DockTriggerController : MonoBehaviour
    {
        private AlterraDronePortController _port;

        private void Awake()
        {
            _port = GetComponentInParent<AlterraDronePortController>();
            if (_port == null)
            {
                QuickLogger.Error("Failed to fine port controller in dock trigger");
            }
        }
        private void OnTriggerEnter(Collider collider)
        {
            var drone = collider.gameObject.name.StartsWith("AlterraHubTransportDrone");
            if (!drone) return;
            var droneController = collider.GetComponentInChildren<DroneController>();
            droneController.SetPort(_port);
        }
    }
}