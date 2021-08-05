using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States
{
    internal class TransportState : BaseState
    {
        private readonly DroneController _drone;
        private float speed = 5;
        private const float maxSpeed = 35f;
        private const float minSpeed = 5f;
        public override string Name => "Transport";

        public TransportState()
        {
            
        }
        
        internal TransportState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }



        public override Type Tick()
        {
            QuickLogger.Debug("1");
            if (_drone.GetTargetPort() == null)
            {
                QuickLogger.Debug("2");
                return typeof(IdleState);
            }

            if (transform.position == _drone.GetTargetPort().GetEntryPoint().position)
            {
                QuickLogger.Debug("3");
                return typeof(AlignState);
            }

            var targetPos = _drone.GetTargetPort().GetEntryPoint().position;
            QuickLogger.Debug("4");

            if (_drone?.GetCurrentPort()?.GetEntryPoint() != null)
            {
                QuickLogger.Debug("5");

                float destDistance = Vector3.Distance(transform.position, targetPos);
                QuickLogger.Debug("6");

                float depDistance = Vector3.Distance(transform.position, _drone.GetCurrentPort().GetEntryPoint().position);
                QuickLogger.Debug("7");

                speed = destDistance > 20 && depDistance > 20 ? maxSpeed : minSpeed;
                QuickLogger.Debug("8");

            }
            else
            {
                speed = maxSpeed;
            }
            QuickLogger.Debug("9");


            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            QuickLogger.Debug("10");

            var rotation = Quaternion.LookRotation(targetPos - transform.position);
            QuickLogger.Debug("11");

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1 * Time.deltaTime);
            QuickLogger.Debug("12");


            return null;
        }


    }
}
