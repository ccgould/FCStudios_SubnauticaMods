using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine.States
{
    internal class IdleState : BaseState
    {
        private readonly DroneController _drone;
        private float _timeRemaining = 5f;
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
            if (_drone == null || DroneDeliveryService.Main == null) return null;

            if (_drone.GetCurrentPort() == null)
            {
                var devices = FCSAlterraHubService.PublicAPI.GetRegisteredDevicesOfId(Mod.DronePortPadHubNewTabID);
                if (devices != null)
                {
                    List<Transform> processList = new();
                    foreach (KeyValuePair<string, FcsDevice> device in devices)
                    {
                        if(device.Value == null) continue;
                        processList.Add(device.Value.transform);
                    }
                    var platform = WorldHelpers.FindNearestObject(processList, transform);
                    _drone.SetDeparturePort(platform?.GetComponent<AlterraDronePortController>());
                }
            }

            if (!DroneDeliveryService.Main.IsStationPort(_drone.GetCurrentPort()))
            {
                QuickLogger.Debug("Getting port from station", true);
                _drone.ShipOrder(DroneDeliveryService.Main.GetOpenPort());                
            }

            return null;
        }
    }
}
