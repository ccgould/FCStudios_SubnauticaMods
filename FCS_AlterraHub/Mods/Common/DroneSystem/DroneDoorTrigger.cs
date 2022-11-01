using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mono.Managers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem;

internal class DroneDoorTrigger : MonoBehaviour
{
    public PortManager Port;

    private void OnTriggerEnter(Collider collider)
    {
        QuickLogger.Debug($"Entered Drone Trigger {collider.gameObject.name}",true);
        
        if (collider.gameObject.name.Contains("AlterraTransportDrone"))
        {
            QuickLogger.Debug("Drone Trigged Station",true);

            var droneController = collider.gameObject.GetComponent<DroneController>();

            if (droneController is not null)
            {
                droneController.SetDeparturePort(Port);
            }
            else
            {
                QuickLogger.Error("Drone Controller Returned NUll,true");
            }
        }
    }
}