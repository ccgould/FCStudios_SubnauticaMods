using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Managers.FCSAlterraHub;

internal class ShipmentPageController : MonoBehaviour
{
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    public void Initialize(FCSPDAController controller)
    {
        var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener((() =>
        {
            controller.GoToPage(PDAPages.Store);
        }));

        _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
    }

    internal void AddItem(Shipment pendingOrder)
    {
        //Instantiate Item
        var item = GameObject.Instantiate(AlterraHub.PDAShipmentItemPrefab);
            
        //Move in scrollview
        item.transform.SetParent(_grid.transform,false);

        //Add controller
        var shipmentTracker = item.AddComponent<ShipmentTracker>();
        shipmentTracker.Initialize(this,pendingOrder);

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