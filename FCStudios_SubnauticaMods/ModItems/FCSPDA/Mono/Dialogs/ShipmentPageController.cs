using FCS_AlterraHub.Core.Helpers;
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
    [SerializeField]
    private GameObject _grid;
    [SerializeField]
    private FCSAlterraHubGUI _gui;
    private List<ShipmentTracker> _trackedShipments = new();

    public override PDAPages PageType => PDAPages.Shipment;

    public void Awake()
    {
        var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener(() =>
        {
            _gui.GoToPage(PDAPages.None);
        });
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

    public override void OnBackButtonClicked()
    {
        _gui.GoBackAPage();
    }
}