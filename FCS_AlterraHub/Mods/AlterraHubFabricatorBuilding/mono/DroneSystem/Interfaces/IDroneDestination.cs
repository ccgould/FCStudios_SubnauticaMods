using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces
{
    internal interface IDroneDestination
    {
        Transform BaseTransform { get; set; }
        string BaseId { get; set; }
        List<Transform> GetPaths();
        void Offload(DroneController order);
        Transform GetDockingPosition();
        void OpenDoors();
        void CloseDoors();
        void DockDrone(DroneController droneController);
        string GetBaseName();
        void Depart(DroneController droneController);
        Transform GetTransform();
    }
}
