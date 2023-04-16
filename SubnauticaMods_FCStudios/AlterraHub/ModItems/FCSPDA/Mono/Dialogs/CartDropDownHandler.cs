using System;
using System.Collections.Generic;
using FCS_AlterraHub.API;
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
    internal static CartDropDownHandler main;
    public Action<CartDropDownHandler> OnBuyAllBtnClick;
    private GameObject _cartList;
    private Text _totalAmount;
    internal Action<decimal> onTotalChanged;
    private ShipmentInfo _shipmentInfo;
    private IFCSAlterraHubGUI _mono;

    internal void Initialize()
    {
        main = this;
        _mono = FCSPDAController.Main.GetGUI();
        _cartList = GameObjectHelpers.FindGameObject(gameObject, "CartDropDownContent");
        _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
        ResetDropDown();

       var result =  ModPrefabService.LoadAsset("CartItem", FCSAssetBundlesService.PublicAPI.GetAssetBundleByName(FCSAssetBundlesService.PublicAPI.GlobalBundleName), out var go, false);

        QuickLogger.Info($"Cart Item Result; {result}");

        var buyAllButton = GameObjectHelpers.FindGameObject(gameObject, "BuyButton").GetComponent<Button>();
        buyAllButton.onClick.AddListener(() =>
        {
            NotificationService.CSVLog(buyAllButton);
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
            NotificationService.CSVLog(closeBTN);
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
        UpdateTotalAmount();
    }

    internal void AddItem(TechType item, TechType receiveTechType, int returnAmount)
    {
        var slots = Inventory.main.container.sizeX * Inventory.main.container.sizeY;

        if (StoreManager.main.GetCartCount(_shipmentInfo) < slots)
        {
            CreateCartItem(item, receiveTechType, returnAmount);
        }
        else
        {
            SendMessageFromDialog(LanguageService.CannotAddAnyMoreItemsToCart());
        }
    }

    private void CreateCartItem(TechType item, TechType receiveTechType, int returnAmount)
    {
        CartItem cartItemComponent = CreateCartItemGameObject(item, receiveTechType, returnAmount);
        _shipmentInfo = StoreManager.main.AddItemToCart(this, _shipmentInfo, cartItemComponent);
    }

    private CartItem CreateCartItemGameObject(TechType item, TechType receiveTechType, int returnAmount)
    {
        var cartItem = GameObject.Instantiate(ModPrefabService.GetPrefab("CartItem"));
        var cartItemComponent = cartItem.AddComponent<CartItem>();
        cartItemComponent.TechType = item;
        cartItemComponent.ReceiveTechType = receiveTechType;
        cartItemComponent.ReturnAmount = returnAmount;
        cartItem.transform.SetParent(_cartList.transform, false);

        cartItemComponent.onRemoveBTNClicked += (pendingItem) =>
        {
            StoreManager.main.RemoveCartItem(_shipmentInfo, pendingItem.Save());
            OnRemoveCartItem(pendingItem);
            OnDeletedCartItem();
        };

        return cartItemComponent;
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
        _totalAmount.text = GetTotal().ToString("n0");
        onTotalChanged?.Invoke(GetTotal());
        QuickLogger.Debug("Updated Total Amount");
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
        UpdateTotalAmount();
    }

    public void OnDeletedCartItem()
    {
        UpdateTotalAmount();
    }

    public void OnRemoveCartItem(CartItem cartItem)
    {
        Destroy(cartItem.gameObject);
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
            for (int i = 0; i < cartItems.Count; i++)
            {
                CartItemSaveData cartItem = cartItems[i];
                CreateCartItemGameObject(cartItem.TechType, cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
                //AddItem(cartItem.TechType, cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
            }
        }
    }
}
