using System;
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

            if (_drone.GetTargetPort() == null)
            {
                return typeof(IdleState);
            }
            
            if (transform.position == _drone.GetTargetPort().GetEntryPoint().position)
            {
                return typeof(AlignState);
            }

            var targetPos = _drone.GetTargetPort().GetEntryPoint().position;
            

            float destDistance = Vector3.Distance(transform.position, targetPos);
            float depDistance = Vector3.Distance(transform.position, _drone.GetCurrentPort().GetEntryPoint().position);
            speed = destDistance > 20 && depDistance > 20 ? maxSpeed : minSpeed;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            var rotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1 * Time.deltaTime);

            return null;
        }


    }
}
