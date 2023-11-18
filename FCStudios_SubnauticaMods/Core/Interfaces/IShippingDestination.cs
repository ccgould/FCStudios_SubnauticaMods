using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.Spawnables.Drone;
using System.Collections.Generic;

namespace FCS_AlterraHub.Core.Interfaces;
internal interface IShippingDestination
{
    public bool HasDronePort { get; }
    public bool HasContructor { get; }
    public bool IsVehicle { get; set; }
    bool SendItemsToConstructor(List<CartItemSaveData> pendingItem);
    string GetBaseName();
    void SetInboundDrone(DroneController droneController);
    IDroneDestination ActivePort();
    string GetPreFabID();
}
