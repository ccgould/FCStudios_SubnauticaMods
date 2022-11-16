using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs
{
    internal class CartDropDownHandler : MonoBehaviour,IStoreClient
    {
        public Action<CartDropDownHandler> OnBuyAllBtnClick;
        private GameObject _cartList;
        private Text _totalAmount;
        internal Action<decimal> onTotalChanged;
        private ShipmentInfo _shipmentInfo;
        private FCSAlterraHubGUI _mono;

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
                    SendMessageFromDialog(Buildables.AlterraHub.CardNotDetected());
                    return;
                }

                if (StoreManager.main.GetCartCount(_shipmentInfo) <= 0)
                {
                    SendMessageFromDialog(Buildables.AlterraHub.NoItemsInCart());
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
        
        internal void AddItem(TechType item,TechType receiveTechType,int returnAmount)
        {
            var slots = Inventory.main.container.sizeX * Inventory.main.container.sizeY;

            if (StoreManager.main.GetCartCount(_shipmentInfo) < slots)
            {
                CreateCartItem(item, receiveTechType,returnAmount);
                UpdateTotalAmount();
            }
            else
            {
                SendMessageFromDialog(Buildables.AlterraHub.CannotAddAnyMoreItemsToCart());
            }
        }

        private void CreateCartItem(TechType item,TechType receiveTechType, int returnAmount)
        {
            var cartItem = GameObject.Instantiate(Buildables.AlterraHub.CartItemPrefab);
            var cartItemComponent = cartItem.AddComponent<CartItem>();
            cartItemComponent.TechType = item;
            cartItemComponent.ReceiveTechType = receiveTechType;
            cartItemComponent.ReturnAmount = returnAmount;
            cartItem.transform.SetParent(_cartList.transform, false);
            _shipmentInfo = StoreManager.main.AddItemToCart(this, _shipmentInfo, cartItemComponent);

            cartItemComponent.onRemoveBTNClicked +=(pendingItem =>
            {
                QuickLogger.Debug("Remove Item.",true);
                StoreManager.main.RemoveCartItem(_shipmentInfo, pendingItem.Save());
                QuickLogger.Debug("Remove Pending Item.", true);
                Destroy(pendingItem.gameObject);
                QuickLogger.Debug("Destroy Item.", true);
                UpdateTotalAmount();
                QuickLogger.Debug("Updated Amount.", true);
            });
        }

        private void ResetDropDown()
        {

            StoreManager.main?.RemovePendingOrder(_shipmentInfo);

            foreach (Transform child in _cartList.transform)
            {
                GameObject.Destroy(child.gameObject);
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
            _mono.ShowMessage(Buildables.AlterraHub.PurchaseSuccessful());
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
        }
    }
}
