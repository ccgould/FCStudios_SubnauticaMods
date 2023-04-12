﻿using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class ShipmentPageController : Page
{
    private GameObject _grid;
    private List<ShipmentTracker> _trackedShipments = new();

    public void Initialize(FCSAlterraHubGUI gui)
    {
        var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener(() =>
        {
            gui.GoToPage(PDAPages.None);
        });

        _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
    }

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