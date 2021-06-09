using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Mods.AlterraHubFabricatorBuilding.Mono.DroneSystem.Interfaces
{
    internal interface IDroneDestination
    {
        Transform BaseTransform { get; set; }
        string BaseId { get; set; }
        List<Transform> GetPaths();
        void Offload(Dictionary<TechType, int> order, Action onOffloadCompleted);
    }
}
