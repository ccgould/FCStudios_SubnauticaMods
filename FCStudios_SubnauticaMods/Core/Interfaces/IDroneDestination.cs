using FCS_AlterraHub.ModItems.Spawnables.Drone;
using FCS_AlterraHub.ModItems.Spawnables.Drone.Enumerators;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Core.Interfaces;
internal interface IDroneDestination
{
    Transform BaseTransform { get; set; }
    string BaseId { get; set; }
    bool IsOccupied { get; }
    List<Transform> GetPaths();
    void Offload(DroneController order);
    Transform GetDockingPosition();
    void OpenDoors();
    void CloseDoors();
    string GetBaseName();
    void Depart(DroneController droneController);
    void Dock(DroneController droneController);
    int GetPortID();
    string GetPrefabID();
    IEnumerator SpawnDrone(Action<DroneController> callback);
    Transform GetEntryPoint();
    string GetBaseID();
    void SetInboundDrone(DroneController droneController);
    void PlayAnimationState(DronePortAnimation departing, Action action);
    void ClearInbound();
}
