using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono.AlterraHubDepot.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Systems;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono.Dialogs
{
    internal class CheckOutPopupDialogWindow : MonoBehaviour
    {
        private Text _accountBalance;
        private Text _total;
        private Text _newBalance;

        private bool _isInitialized;
        private FCSPDAController _mono;
        private CardSystem cardSystem => CardSystem.main;

        public CartDropDownHandler _cart;
        public UnityEvent onCheckOutPopupDialogClosed = new UnityEvent();
        private DestinationDialogController _destinationDialogController;
        private Text _destinationText;
        internal AlterraHubDepotController SelectedDestination { get; set; }

        private void Initialize(FCSPDAController mono)
        {
            if (_isInitialized) return;

            _mono = mono;

            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance").GetComponent<Text>();

            _total = GameObjectHelpers.FindGameObject(gameObject, "Total").GetComponent<Text>();
            _newBalance = GameObjectHelpers.FindGameObject(gameObject, "NewBalance").GetComponent<Text>();
            _destinationText = GameObjectHelpers.FindGameObject(gameObject, "Destination").GetComponent<Text>();
            
            CreatePurchaseButton();
            CreatePurchaseExitButton();
            CreateDestinationPopup();

            var destinationButton = GameObjectHelpers.FindGameObject(gameObject, "DestinationBTN").GetComponent<Button>();
            destinationButton.onClick.AddListener((() =>
            {
                _destinationDialogController.Open();
            }));
            var backBtn = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();
            backBtn.onClick.AddListener(HideDialog);

            CardSystem.main.onBalanceUpdated += UpdateScreen;
            //_mono.AlterraHubTrigger.onTriggered += value => { UpdateScreen(); };
            
            _isInitialized = true;
        }

        private void CreatePurchaseButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN").GetComponent<Button>();
            purchaseBTN.onClick.AddListener(() =>
            {
                QuickLogger.Debug("Purchase Button Clicked",true);
                MakePurchase();
            });
        }

        private bool MakePurchase()
        {
            var totalSize = new List<Vector2int>();
            foreach (CartItem cartItem in _cart.GetItems())
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
                    totalSize.Add(CraftData.GetItemSize(cartItem.TechType));
                }
            }

            //if (!Inventory.main.container.HasRoomFor(totalSize))
            //{
            //    QuickLogger.ModMessage(Buildables.AlterraHub.InventoryFull());
            //    return false;
            //}

            if (SelectedDestination == null)
            {
                MessageBoxHandler.main.Show( Buildables.AlterraHub.NoDestinationFound(),FCSMessageButton.OK);
                return false;
            }

            if (!CardSystem.main.HasBeenRegistered())
            {
                MessageBoxHandler.main.Show(Buildables.AlterraHub.AccountNotFoundFormat(),FCSMessageButton.OK);
                return false;
            }

            if (Inventory.main.container.GetCount(Mod.DebitCardTechType) <= 0)
            {
                MessageBoxHandler.main.Show(Buildables.AlterraHub.CardNotDetected(),FCSMessageButton.OK);
                return false;
            }

            if (CardSystem.main.HasEnough(_cart.GetTotal()))
            {
                if (!_mono.MakeAPurchase(_cart,SelectedDestination))
                {
                    return false;
                }
                _cart.TransactionComplete();
                HideDialog();
                return true;
            }

            MessageBoxHandler.main.Show(Buildables.AlterraHub.NotEnoughMoneyOnAccount(),FCSMessageButton.OK);
            return false;
        }

        private void CreatePurchaseExitButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseExitBTN").GetComponent<Button>();
            purchaseBTN.onClick.AddListener(() =>
            {
                QuickLogger.Debug("Purchase Button Clicked", true);
                if (MakePurchase())
                {
                    _mono.ExitStore();
                }
            });
        }

        private void CreateDestinationPopup()
        {
            var destinationPopDiag = GameObjectHelpers.FindGameObject(_mono.gameObject, "DestinationPopUp");
            _destinationDialogController = destinationPopDiag.AddComponent<DestinationDialogController>();
            _destinationDialogController.Initialize(this);
            _destinationDialogController.OnClose += () =>
            {
                    _destinationText.text = $"Destination: {SelectedDestination?.DepotName}";
            };
        }

        private void UpdateScreen()
        {
            QuickLogger.Debug("Updating Screen", true);
            if (!PlayerInteractionHelper.HasCard())
            {
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cart?.GetTotal() ?? 0);
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            }
            else
            {
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.GetAccountBalance());
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cart.GetTotal());
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(CardSystem.main.GetAccountBalance() - _cart.GetTotal());
            }
        }

        internal void ShowDialog(FCSPDAController mono, CartDropDownHandler cart)
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

        internal void ResetScreen()
        {
            _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
            _total.text = Buildables.AlterraHub.CheckOutTotalFormat(0);
            _newBalance.text = Buildables.AlterraHub.CheckOutTotalFormat(0);
            _destinationText.text = "Destination:";
            SelectedDestination = null;
        }
    }
}