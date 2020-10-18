using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class CartDropDownHandler : MonoBehaviour
    {
        public Action<CartDropDownHandler> OnBuyAllBtnClick;
        private GameObject _cartList;
        private readonly List<CartItem> _pendingItems = new List<CartItem>();
        private Text _totalAmount;
        internal Action<float> onTotalChanged;

        internal void Initialize()
        {
            _cartList = GameObjectHelpers.FindGameObject(gameObject, "CartDropDownContent");
            _totalAmount = GameObjectHelpers.FindGameObject(gameObject, "TotalAmount").GetComponent<Text>();
            ResetDropDown();

            var buyAllButton = GameObjectHelpers.FindGameObject(gameObject, "BuyButton").GetComponent<Button>();
            buyAllButton.onClick.AddListener(() =>
            {
                if (_pendingItems.Count <= 0)
                {
                    SendMessageFromDialog(Buildables.AlterraHub.NoItemsInCart());
                    return;
                }
                
                OnBuyAllBtnClick?.Invoke(this);
            });
        }

        private void SendMessageFromDialog(string message)
        {
            MessageBoxHandler.main.Show(message);
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
        
        internal void AddItem(TechType item,TechType receiveTechType)
        {
            var slots = Inventory.main.container.sizeX * Inventory.main.container.sizeY;
            if (_pendingItems.Count < slots)
            {
                CreateCartItem(item, receiveTechType);
                UpdateTotalAmount();
            }
            else
            {
                SendMessageFromDialog(Buildables.AlterraHub.CannotAddAnyMoreItemsToCart());
            }
        }

        private void CreateCartItem(TechType item,TechType receiveTechType)
        {
            var cartItem = GameObject.Instantiate(Buildables.AlterraHub.CartItemPrefab);
            var cartItemComponent = cartItem.AddComponent<CartItem>();
            cartItemComponent.TechType = item;
            cartItemComponent.ReceiveTechType = receiveTechType;
            cartItem.transform.SetParent(_cartList.transform, false);
            _pendingItems.Add(cartItemComponent);

            cartItemComponent.onRemoveBTNClicked +=(pendingItem =>
            {
                QuickLogger.Debug("Remove Item.",true);
                _pendingItems.Remove(pendingItem);
                QuickLogger.Debug("Remove Pending Item.", true);
                Destroy(pendingItem.gameObject);
                QuickLogger.Debug("Destroy Item.", true);
                UpdateTotalAmount();
                QuickLogger.Debug("Updated Amount.", true);
            });
        }

        private void ResetDropDown()
        {
            _pendingItems?.Clear();

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

        internal float GetTotal()
        {
            return  _pendingItems.Sum(x => StoreInventorySystem.GetPrice(x.TechType));
        }

        internal int GetCartCount()
        {
            return _pendingItems.Count;
        }
        
        internal void TransactionComplete()
        {
            ResetDropDown();
        }

        public IEnumerable<CartItem> GetItems()
        {
            return _pendingItems;
        }

        public IEnumerable<CartItemSaveData> Save()
        {
            foreach (CartItem cartItem in _pendingItems)
            {
                yield return cartItem.Save();
            }
        }
    }
}
