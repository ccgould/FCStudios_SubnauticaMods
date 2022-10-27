using System;
using UnityEngine;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine.States
{
    internal class DescendState : BaseState
    {
        private readonly DroneController _drone;
        private float speed = 5;
        private const float maxSpeed = 35f;
        private const float minSpeed = 5f;
        private bool _straightened;
        public override string Name => "Descend";

        public DescendState()
        {
            
        }

        internal DescendState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }

        public override Type Tick()
        {
            //QuickLogger.Debug("Descending", true);
            var target = _drone.GetTargetPort().ActivePort().GetEntryPoint().position;
            if (transform.position == target)
            {
                _straightened = false;
                return typeof(AlignState);
            }

            var targetPos = new Vector3(transform.position.x, 100, transform.position.z);

            if (_drone?.GetTargetPort()?.ActivePort().GetEntryPoint() != null)
            {
                float destDistance = Vector3.Distance(transform.position, targetPos);

                float depDistance = Vector3.Distance(transform.position, _drone.GetTargetPort().ActivePort().GetEntryPoint().position);

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

            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, target.y ,transform.position.z), speed * Time.deltaTime);

            if (!_straightened)
            {
                var forward = -transform.forward;
                forward.y = 0f;
                forward.Normalize();
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                _straightened = true;
            }

            var rot = Quaternion.LookRotation(_drone.GetTargetPort().ActivePort().GetEntryPoint().transform.forward, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f * Time.deltaTime);


            return typeof(DescendState);
        }
    }
}
