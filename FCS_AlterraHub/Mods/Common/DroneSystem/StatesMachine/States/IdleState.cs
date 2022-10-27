using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.AlterraHubPod.Spawnable;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Registration;
using FCSCommon.Utilities;
using Subnautica;
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
            QuickLogger.Debug(Name);
            return null;
        }
    }
}
