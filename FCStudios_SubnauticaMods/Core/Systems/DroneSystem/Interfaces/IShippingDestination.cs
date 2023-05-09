using System.Collections.Generic;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;


namespace FCS_AlterraHub.Core.Systems.DroneSystem.Interfaces;

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
