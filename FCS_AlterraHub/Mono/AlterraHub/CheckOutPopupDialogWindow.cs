using System;
using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.AlterraHub
{
    internal class CheckOutPopupDialogWindow : MonoBehaviour, IFCSStorage
    {
        private Text _accountBalance;
        private Text _total;
        private Text _newBalance;
        public CartDropDownHandler _cart;
        public UnityEvent onCheckOutPopupDialogClosed  = new UnityEvent();
        private bool _cardLoaded;
        private string _cardNumber;
        private bool _isInitialized;
        private AlterraHubController _mono;
        private AccountDetails cards => CardSystem.main.AccountDetails;

        public int GetContainerFreeSpace => 0;
        public bool IsFull => true;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        private void Initialize(AlterraHubController mono)
        {
            if (_isInitialized) return;

            _mono = mono;

            //var dumpContainer = CreateDumpContainer();

            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance").GetComponent<Text>();

            _total = GameObjectHelpers.FindGameObject(gameObject, "Total").GetComponent<Text>();
            _newBalance = GameObjectHelpers.FindGameObject(gameObject, "NewBalance").GetComponent<Text>();
            
            //CreateSelectButton(dumpContainer);

            CreatePurchaseButton();

            var backBtn = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            backBtn.onClick.AddListener(HideDialog);

            CardSystem.main.onBalanceUpdated += balance => { UpdateScreen(); };
            _mono.AlterraHubTrigger.onTriggered += value => { UpdateScreen(); };
            
            _isInitialized = true;
        }

        private void CreatePurchaseButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN").GetComponent<Button>();
            purchaseBTN.onClick.AddListener(() =>
            {
                if (_cardLoaded)
                {
                    if (CardSystem.main.HasEnough( _cart.GetTotal()))
                    {
                        var result = _mono.MakeAPurchase(_cart);
                        if (result)
                        {
                            _cart.TransactionComplete();
                            HideDialog();
                        }

                        return;
                    }
                }

                MessageBoxHandler.main.Show(Buildables.AlterraHub.NoValidCardForPurchase());
            });
        }

        private void CreateSelectButton(DumpContainer dumpContainer)
        {
            var selectCardBTN = GameObjectHelpers.FindGameObject(gameObject, "SelectCardBTN").GetComponent<Button>();
            selectCardBTN.onClick.AddListener(() =>
            {
                QuickLogger.Debug("Opening Card Dump", true);
                dumpContainer.OpenStorage();
            });
        }

        private DumpContainer CreateDumpContainer()
        {
            var dumpContainer = gameObject.AddComponent<DumpContainer>();
            dumpContainer.Initialize(gameObject.transform, Buildables.AlterraHub.CardReader(), this, 1, 1);
            return dumpContainer;
        }

        private void UpdateScreen()
        {
            QuickLogger.Debug("Updating Screen", true);
            if (!_mono.IsPlayerInRange()  || !PlayerInteractionHelper.HasCard())
            {
                QuickLogger.Debug($"Player not in range: {_mono.IsPlayerInRange()}",true);
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cart?.GetTotal() ?? 0);
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            }
            else
            {
                QuickLogger.Debug($"Player is in range: {_mono.IsPlayerInRange()}", true);
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.AccountDetails.Balance);
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cart.GetTotal());
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(CardSystem.main.AccountDetails.Balance - _cart.GetTotal());
            }
        }

        internal void ShowDialog(AlterraHubController mono, CartDropDownHandler cart)
        {
            Initialize(mono);
            ResetScreen();
            _cart = cart;
            UpdateScreen();
            gameObject.SetActive(true);
        }

        internal void HideDialog()
        {
            gameObject.SetActive(false);
            ResetScreen();
            onCheckOutPopupDialogClosed?.Invoke();
        }

        public bool CanBeStored(int amount, TechType techType)
        {
            return techType == Mod.DebitCardTechType;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _cardNumber = item.item.gameObject.GetComponent<FcsCard>().GetCardNumber();
            PlayerInteractionHelper.GivePlayerItem(item);
            QuickLogger.Debug($"Checking if card number {_cardNumber} exist",true);
            if(CardSystem.main.CardExist(_cardNumber))
            {
                UpdateScreen();
                _cardLoaded = true;
            }
            else
            {
                QuickLogger.Debug("Card doesnt exist",true);
                return false;
            }
            return true;
        }
        
        internal void ResetScreen()
        {
            _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
            _total.text = Buildables.AlterraHub.CheckOutTotalFormat(0);
            _newBalance.text = Buildables.AlterraHub.CheckOutTotalFormat(0);
            _cardLoaded = false;
            _cardNumber = string.Empty;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(1, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }
    }
}