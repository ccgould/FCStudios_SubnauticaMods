using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces
{
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
        int GetPortID();
        string GetPrefabID();
#if SUBNAUTICA_STABLE
        DroneController SpawnDrone();
#endif
        IEnumerator SpawnDrone(Action<DroneController> callback);
        Transform GetEntryPoint();
        string GetBaseID();
    }
}
