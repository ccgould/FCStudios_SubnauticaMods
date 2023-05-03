using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class ShipmentTracker : MonoBehaviour
{
    private Text _orderName;
    private Slider _slider;
    private Shipment _shipment;
    private GameObject _itemsGrid;
    private Button _cancelButton;
    private Shipment _pendingOrder;

    public void Initialize(ShipmentPageController shipmentPageController, Shipment pendingOrder)
    {
        _orderName = gameObject.FindChild("OrderNumber").GetComponent<Text>();
        _itemsGrid = gameObject.FindChild("Items");
        _cancelButton = GetComponentInChildren<Button>();
        _pendingOrder = pendingOrder;

        _cancelButton.onClick.AddListener(() =>
        {
            StoreManager.main.CancelOrder(pendingOrder);
            Delete();
        });

        foreach (CartItemSaveData cartItem in pendingOrder.CartItems)
        {
            var item = GameObject.Instantiate(ModPrefabService.GetPrefab("PDAShipmentItemNode"));
            item.FindChild("Icon").AddComponent<uGUI_Icon>().sprite = SpriteManager.Get(cartItem.ReceiveTechType);
            item.transform.SetParent(_itemsGrid.transform, false);
        }
        _slider = gameObject.GetComponentInChildren<Slider>();
        _shipment = pendingOrder;
        InvokeRepeating(nameof(UpdateCheck), 1f, 1f);
    }

    private void Delete()
    {
        Destroy(gameObject);
    }

    //TODO Find away to decouple
    private void UpdateCheck()
    {
        //if (DroneDeliveryService.Main == null ||
        //    string.IsNullOrWhiteSpace(_pendingOrder.Info.OrderNumber) ||
        //    string.IsNullOrEmpty(_pendingOrder.Info.BaseName)) return;

        //var isCurrentOrder = DroneDeliveryService.Main.IsCurrentOrder(_shipment.Info.OrderNumber);
        //var status = isCurrentOrder ? "Shipping" : "Pending";
        //_orderName.text = $"Order: {_pendingOrder.Info.OrderNumber}: DestinationID: {_pendingOrder.Info.BaseName} Status: {status}";
        //_cancelButton.interactable = !isCurrentOrder;
        //_slider.value = DroneDeliveryService.Main.GetOrderCompletionPercentage(_shipment.Info.OrderNumber);
    }

    public bool TryDelete(Shipment shipment)
    {
        if (shipment.Info.OrderNumber.Equals(_shipment.Info.OrderNumber))
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}