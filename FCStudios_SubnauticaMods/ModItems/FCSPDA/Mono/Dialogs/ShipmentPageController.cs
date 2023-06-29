using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class ShipmentPageController : Page
{
    [SerializeField]
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    internal void AddItem(Shipment pendingOrder)
    {
        //Instantiate Item
        var item = GameObject.Instantiate(ModPrefabService.GetPrefab("PDAShipmentItem"));

        //Move in scrollview
        item.transform.SetParent(_grid.transform, false);

        //Add controller
        var shipmentTracker = item.AddComponent<ShipmentTracker>();
        shipmentTracker.Initialize(this, pendingOrder);

        _trackedShipments.Add(shipmentTracker);
    }

    public void RemoveItem(Shipment shipment)
    {
        foreach (ShipmentTracker shipmentTracker in _trackedShipments)
        {
            if (shipmentTracker.TryDelete(shipment))
            {
                break;
            }
        }
    }
}