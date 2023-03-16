using System;
using FCSCommon.Utilities;

namespace FCS_AlterraHub.Core.Systems.DroneSystem.StatesMachine.States;

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
