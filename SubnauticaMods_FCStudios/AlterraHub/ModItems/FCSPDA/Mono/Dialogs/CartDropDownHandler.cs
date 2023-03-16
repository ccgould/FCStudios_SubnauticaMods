using System;
using System.Collections.Generic;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class CartDropDownHandler : MonoBehaviour, IStoreClient
{
    public Action<CartDropDownHandler> OnBuyAllBtnClick;
    private GameObject _cartList;
    private Text _totalAmount;
    internal Action<decimal> onTotalChanged;
    private ShipmentInfo _shipmentInfo;
    private IFCSAlterraHubGUI _mono;

    internal void Initialize(FCSAlterraHubGUI mono)
    {
        _mono = mono;
        _cartList = GameObjectHelpers.FindGameObject(gameObject, "CartDropDownContent");
        _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
        ResetDropDown();

        var buyAllButton = GameObjectHelpers.FindGameObject(gameObject, "BuyButton").GetComponent<Button>();
        buyAllButton.onClick.AddListener(() =>
        {
            if (!PlayerInteractionHelper.HasCard())
            {
                SendMessageFromDialog(LanguageService.CardNotDetected());
                return;
            }

            if (StoreManager.main.GetCartCount(_shipmentInfo) <= 0)
            {
                SendMessageFromDialog(LanguageService.NoItemsInCart());
                return;
            }
            OnBuyAllBtnClick?.Invoke(this);
        });

        var closeBTN = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
        closeBTN.onClick.AddListener(() =>
        {
            ToggleVisibility();
        });
    }

    private void SendMessageFromDialog(string message)
    {
        _mono.ShowMessage(message);
    }

    internal void ToggleVisibility(bool forceClose = false)
    {
        if (forceClose)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(!gameObject.activeSelf);
    }

    internal void AddItem(TechType item, TechType receiveTechType, int returnAmount)
    {
        var slots = Inventory.main.container.sizeX * Inventory.main.container.sizeY;

        if (StoreManager.main.GetCartCount(_shipmentInfo) < slots)
        {
            CreateCartItem(item, receiveTechType, returnAmount);
            UpdateTotalAmount();
        }
        else
        {
            SendMessageFromDialog(LanguageService.CannotAddAnyMoreItemsToCart());
        }
    }

    private void CreateCartItem(TechType item, TechType receiveTechType, int returnAmount)
    {
        var cartItem = GameObject.Instantiate(ModPrefabService.GetPrefab("DebitCard"));
        var cartItemComponent = cartItem.AddComponent<CartItem>();
        cartItemComponent.TechType = item;
        cartItemComponent.ReceiveTechType = receiveTechType;
        cartItemComponent.ReturnAmount = returnAmount;
        cartItem.transform.SetParent(_cartList.transform, false);
        _shipmentInfo = StoreManager.main.AddItemToCart(this, _shipmentInfo, cartItemComponent);

        cartItemComponent.onRemoveBTNClicked += pendingItem =>
        {
            QuickLogger.Debug("Remove Item.", true);
            StoreManager.main.RemoveCartItem(_shipmentInfo, pendingItem.Save());
            QuickLogger.Debug("Remove Pending Item.", true);
            Destroy(pendingItem.gameObject);
            QuickLogger.Debug("Destroy Item.", true);
            UpdateTotalAmount();
            QuickLogger.Debug("Updated Amount.", true);
        };
    }

    private void ResetDropDown()
    {

        StoreManager.main?.RemovePendingOrder(_shipmentInfo);

        foreach (Transform child in _cartList.transform)
        {
            Destroy(child.gameObject);
        }

        UpdateTotalAmount();
    }

    private void UpdateTotalAmount()
    {
        QuickLogger.Debug("Updating Total Amount");
        _totalAmount.text = GetTotal().ToString("n0");
        QuickLogger.Debug("Updating Total Amount");
        onTotalChanged?.Invoke(GetTotal());
        QuickLogger.Debug("Updating Total Amount");
    }

    internal decimal GetTotal()
    {
        return StoreManager.main?.GetCartTotal(_shipmentInfo) ?? 0;
    }

    internal int GetCartCount()
    {
        return StoreManager.main?.GetCartCount(_shipmentInfo) ?? 0;
    }

    internal void TransactionComplete()
    {
        _shipmentInfo = null;
        ResetDropDown();
        _mono.ShowMessage(LanguageService.PurchaseSuccessful());
    }

    public IEnumerable<CartItemSaveData> GetItems()
    {
        return StoreManager.main.GetCartItems(_shipmentInfo);
    }

    public StoreClientType ClientType => StoreClientType.PDA;

    public void OnOrderComplete(bool result)
    {
    }

    public void OnCreatedCartItem()
    {
    }

    public void OnDeletedCartItem()
    {
    }

    public void OnRemoveCartItem(GameObject go)
    {
    }

    public ShipmentInfo GetShipmentInfo()
    {
        return _shipmentInfo;
    }

    public void LoadShipmentInfo(ShipmentInfo info)
    {
        _shipmentInfo = info;
        var cartItems = StoreManager.main.GetCartItems(_shipmentInfo);
        if (cartItems == null)
        {
            QuickLogger.Debug("Cart Items returned Null");
        }
        else
        {
            foreach (CartItemSaveData cartItem in cartItems)
            {
                AddItem(cartItem.TechType, cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
            }
        }
    }
}
