using System;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.StatesMachine.States
{
    internal class ClimbState : BaseState
    {
        private readonly DroneController _drone;
        private float speed = 5;
        private const float maxSpeed = 35f;
        private const float minSpeed = 5f;
        private bool _straightened;
        private Quaternion _rot;
        public override string Name => "Climbing";

        public ClimbState()
        {
            
        }

        internal ClimbState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }

        public override Type Tick()
        {
            //QuickLogger.Debug("Climbing",true);
            if (transform.position.y >= 100)
            {
                _straightened = false;
                return typeof(TransportState);
            }

            var targetPos = new Vector3(transform.position.x, 100, transform.position.z);

            if (_drone?.GetCurrentPort()?.GetEntryPoint() != null)
            {

                float destDistance = Vector3.Distance(transform.position, targetPos);

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
            
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, 100 ,transform.position.z), speed * Time.deltaTime);

            if (!_straightened)
            {
                var forward = -transform.forward;
                forward.y = 0f;
                forward.Normalize();
                _rot = Quaternion.LookRotation(forward, Vector3.up);
                _straightened = true;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, _rot, 1f * Time.deltaTime);


            return typeof(ClimbState);
        }
    }
}
