using System;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Mods.Common.DroneSystem.StatesMachine.States
{
    internal class DockingState : BaseState
    {
        private readonly DroneController _drone;
        public override string Name => "Docking";

        public DockingState()
        {
            
        }

        internal DockingState(DroneController drone) : base(drone.gameObject)
        {
            _drone = drone;
        }

        public override Type Tick()
        {
            //Debug.Log("Docking");
            _drone.Dock();
            var followPoint = _drone.GetTargetPort()?.ActivePort().GetEntryPoint();

            if (followPoint == null)
            {
                QuickLogger.DebugError("followPoint Returned null");
                return typeof(DockingState);
            }

            transform.position = followPoint.position;
            transform.rotation = followPoint.rotation;
           
            return null;
        }
    }
}
