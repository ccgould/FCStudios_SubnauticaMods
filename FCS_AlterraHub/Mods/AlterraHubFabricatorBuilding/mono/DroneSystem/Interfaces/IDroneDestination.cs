﻿using System.Collections.Generic;
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
        DroneController SpawnDrone();
        Transform GetEntryPoint();
        string GetBaseID();
    }
}
