using System;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine.States
{
    internal class DepartState : BaseState
    {
        private readonly DroneController _drone;
        public override string Name => "Departing";

        public DepartState()
        {
            
        }

        internal DepartState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }

        public override Type Tick()
        {
            //Debug.Log("Departing");
            //_drone.Depart();
            var followPoint = _drone.GetCurrentPort()?.GetEntryPoint();

            if (followPoint != null)
            {
                transform.position = followPoint.position;
                transform.rotation = followPoint.rotation;
            }
            return null;
        }
    }
}
