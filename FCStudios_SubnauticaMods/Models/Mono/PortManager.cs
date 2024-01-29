using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.Spawnables.Drone;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Models.Mono;
internal class PortManager : MonoBehaviour, IShippingDestination 
{
    public HabitatManager Manager { get; internal set; }

    public bool HasDronePort => true;

    public bool HasContructor => true;

    public bool IsVehicle { get; set; }

    public IDroneDestination ActivePort()
    {
        return null;
    }

    public string GetBaseName()
    {
        return Manager.GetBaseFormatedID();
    }

    public string GetPreFabID()
    {
        return Manager.GetBasePrefabID();
    }

    public bool SendItemsToConstructor(List<CartItemSaveData> pendingItem)
    {
        //TODO Replace with contructor code
        if(pendingItem is not null && pendingItem.Count > 0)
        {
            foreach(CartItemSaveData item in pendingItem)
            {
                PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType, item.ReturnAmount);
            }
            return true;
        }
        return false;
    }

    public void SetInboundDrone(DroneController droneController)
    {
        
    }

    internal string GetBaseID()
    {
        return Manager.GetBaseIDFormatted();
    }

    internal bool HasAccessPoint()
    {
        return true;
    }
}
