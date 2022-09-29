using System;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine.States
{
    internal class TransportState : BaseState
    {
        private readonly DroneController _drone;
        private float speed = 5;
        private Vector3 _targetPos;
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
            //QuickLogger.Debug("TransportState",true);
            if (_drone.GetTargetPort() == null)
            {
                return typeof(IdleState);
            }

            _targetPos = new Vector3(_drone.GetTargetPort().GetEntryPoint().position.x, 100, _drone.GetTargetPort().GetEntryPoint().position.z);

            if (transform.position == _targetPos)
            {
                return typeof(DescendState);
            }


            if (_drone?.GetCurrentPort()?.GetEntryPoint() != null)
            {

                float destDistance = Vector3.Distance(transform.position, _targetPos);

                float depDistance = Vector3.Distance(transform.position, _drone.GetCurrentPort().GetEntryPoint().position);

                if (depDistance <= 20 || destDistance <= 20)
                {
                    speed = minSpeed;
                }
                else
                {
                    speed = maxSpeed;
                }
            }
            else
            {
                speed = maxSpeed;
            }

            transform.position = Vector3.MoveTowards(transform.position, _targetPos, speed * Time.deltaTime);

            var vector = _targetPos - transform.position;
            var rotation = vector == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(vector);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1 * Time.deltaTime);

            return null;
        }
    }
}
