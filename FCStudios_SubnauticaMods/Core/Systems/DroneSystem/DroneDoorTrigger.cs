using FCS_AlterraHub.Core.Systems.DroneSystem.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Core.Systems.DroneSystem;

internal class DroneDoorTrigger : MonoBehaviour
{
    public PortManager PortManager;

    private void OnTriggerEnter(Collider collider)
    {
        QuickLogger.Debug($"Entered Drone Trigger {collider.gameObject.name}", true);

        if (collider.gameObject.name.Contains("AlterraTransportDrone"))
        {
            QuickLogger.Debug("Drone Trigged Station", true);

            var droneController = collider.gameObject.GetComponent<DroneController>();

            if (droneController is not null)
            {
                droneController.SetDeparturePort(PortManager);
            }
            else
            {
                QuickLogger.Error("Drone Controller Returned NUll,true");
            }
        }
    }
}