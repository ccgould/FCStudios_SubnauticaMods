using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States
{
    public class IdleState : BaseState
    {
        private readonly DroneController _drone;
        public override string Name => "Idle";

        public IdleState()
        {
            
        }

        internal IdleState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }

        public override Type Tick()
        {
            //QuickLogger.Debug("Idle Tick",true);

            if (_drone.GetCurrentPort() == null)
            {
                var devices = FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(Mod.DronePortPadHubNewTabID);
                if (devices != null)
                {
                    List<Transform> processList = new();
                    foreach (KeyValuePair<string, FcsDevice> device in devices)
                    {
                        processList.Add(device.Value.transform);
                    }
                    var platform = WorldHelpers.FindNearestObject(processList, transform);
                    _drone.SetDeparturePort(platform.GetComponent<AlterraDronePortController>());
                }
            }

            if (!AlterraFabricatorStationController.Main.IsStationPort(_drone.GetCurrentPort()))
            {
                _drone.ShipOrder(AlterraFabricatorStationController.Main.GetOpenPort());
            }

            return null;
        }
    }
}
