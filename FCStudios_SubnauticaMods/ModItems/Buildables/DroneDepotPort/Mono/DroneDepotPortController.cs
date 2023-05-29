using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Models.Abstract;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Mono;

internal class DroneDepotPortController : FCSDevice
{
    [SerializeField]
    private DoorController _doorController;
    [SerializeField]
    private DoorController _droneDoor1Controller;
    [SerializeField]
    private DoorController _droneDoor2Controller;
    [SerializeField]
    private Transform _droneOpenPos;

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    private void ToggleCargoDoor()
    {
        _doorController.Toggle(Vector3.zero);
    }

    private void ToggleDroneDoors()
    {
        _droneDoor1Controller.Toggle(_droneOpenPos.position);
        _droneDoor2Controller.Toggle(_droneOpenPos.position);
    }

    public override bool IsDeconstructionObstacle()
    {
        return true;
    }

    public override void OnConstructedChanged(bool constructed)
    {
        IsConstructed = constructed;

        if (constructed)
        {
            if (base.isActiveAndEnabled)
            {
                if (!this.IsInitialized)
                {
                    this.Initialize();
                }
                this.IsInitialized = true;
                return;
            }
            _runStartUpOnEnable = true;
        }
    }

    public override void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        
    }

    public override void OnProtoSerialize(ProtobufSerializer serializer)
    {
        
    }

    public override void ReadySaveData()
    {
        
    }
}
