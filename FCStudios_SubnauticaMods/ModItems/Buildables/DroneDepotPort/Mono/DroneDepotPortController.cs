using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Abstract;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.Buildables.DroneDepotPort.Mono;

internal class DroneDepotPortController : FCSDevice
{
    private DoorController _doorController;
    private DoorController _droneDoor1Controller;
    private DoorController _droneDoor2Controller;
    private Transform _droneOpenPos;

    public override bool CanDeconstruct(out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public override void Initialize()
    {
        var collectionDoor = GameObjectHelpers.FindGameObject(gameObject, "mesh_cargo_door");
        var topdoor01 = GameObjectHelpers.FindGameObject(gameObject, "topdoor01");
        var topdoor02 = GameObjectHelpers.FindGameObject(gameObject, "topdoor02");
        
        _doorController = collectionDoor.EnsureComponent<DoorController>();
        _doorController._slideAmount = 0.576912f;
        _doorController._slideDirection = Vector3.up;

        _droneDoor1Controller = topdoor01.EnsureComponent<DoorController>();
        _droneDoor1Controller._isRotatingDoor = true;
        _droneDoor1Controller._forwardDirection = 1.5f;

        _droneDoor2Controller = topdoor02.EnsureComponent<DoorController>();
        _droneDoor2Controller._isRotatingDoor = true;
        _droneDoor2Controller._forwardDirection = 1.5f;
        _droneOpenPos = GameObjectHelpers.FindGameObject(gameObject, "Unity_Collison_Object_0").transform;

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
