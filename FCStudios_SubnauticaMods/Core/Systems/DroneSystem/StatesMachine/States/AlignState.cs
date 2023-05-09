using System;
using UnityEngine;

namespace FCS_AlterraHub.Core.Systems.DroneSystem.StatesMachine.States;

internal class AlignState : BaseState
{
    private readonly DroneController _drone;
    public override string Name => "Aligning";

    public AlignState()
    {

    }

    internal AlignState(DroneController drone) : base(drone.gameObject)
    {
        _drone = drone;
    }

    public override Type Tick()
    {
        //Debug.Log("Aligning Drone");
        var rot = Quaternion.LookRotation(_drone.GetTargetPort().ActivePort().GetEntryPoint().transform.forward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f * Time.deltaTime);

        //Thanks to https://forum.unity.com/threads/checking-if-rotation-is-complete.515058/ user McDev02
        if (Quaternion.Angle(transform.rotation, rot) <= 0.01f)
        {
            return typeof(DockingState);
        }
        //Debug.Log("Align Drone");
        return null;
    }
}
