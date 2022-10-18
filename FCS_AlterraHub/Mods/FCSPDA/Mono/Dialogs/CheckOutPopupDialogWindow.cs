using System.Collections.Generic;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers.FCSAlterraHub;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs
{
    internal class CheckOutPopupDialogWindow : MonoBehaviour
    {
        private Text _accountBalance;
        private Text _total;
        private Text _newBalance;

        private bool _isInitialized;
        private FCSAlterraHubGUI _mono;
        private CardSystem cardSystem => CardSystem.main;

        public CartDropDownHandler _cartDropDownHandler;
        public UnityEvent onCheckOutPopupDialogClosed = new UnityEvent();
        private DestinationDialogController _destinationDialogController;
        private Text _destinationText;
        internal IShippingDestination SelectedDestination { get; set; }


        private void Initialize(FCSAlterraHubGUI mono)
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
            foreach (CartItem cartItem in _cartDropDownHandler.GetItems())
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
#if SUBNAUTICA
                    totalSize.Add(CraftData.GetItemSize(cartItem.TechType));
#else
                    totalSize.Add(TechData.GetItemSize(cartItem.TechType));
#endif
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

            //if (!SelectedDestination.HasDepot())
            //{
            //    VoiceNotificationSystem.main.Play("PDA_Drone_Instructions_key");
            //    return false;
            //}

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

            if (CardSystem.main.HasEnough(_cartDropDownHandler.GetTotal()))
            {
                QuickLogger.Debug("1");
                if (StoreManager.main.CompleteOrder(_cartDropDownHandler, _cartDropDownHandler.GetOrderNumber(), SelectedDestination))
                {
                    QuickLogger.Debug("2");
                    _cartDropDownHandler.TransactionComplete();
                    HideDialog();
                    QuickLogger.Debug("3");
                    return true;
                }
            }
            else
            {
                MessageBoxHandler.main.Show(Buildables.AlterraHub.NotEnoughMoneyOnAccount(), FCSMessageButton.OK);
            }
            return false;
        }

        private void CreatePurchaseExitButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(_mono.gameObject, "PurchaseExitBTN").GetComponent<Button>();
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
                _destinationText.text = $"Destination: {SelectedDestination?.GetBaseName()}";
            };
        }

        private void UpdateScreen()
        {
            QuickLogger.Debug("Updating Screen", true);
            if (!PlayerInteractionHelper.HasCard())
            {
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(0);
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cartDropDownHandler?.GetTotal() ?? 0);
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(0);
            }
            else
            {
                _accountBalance.text = Buildables.AlterraHub.AccountBalanceFormat(CardSystem.main.GetAccountBalance());
                _total.text = Buildables.AlterraHub.CheckOutTotalFormat(_cartDropDownHandler.GetTotal());
                _newBalance.text = Buildables.AlterraHub.AccountNewBalanceFormat(CardSystem.main.GetAccountBalance() - _cartDropDownHandler.GetTotal());
            }
        }

        internal void ShowDialog(FCSAlterraHubGUI mono, CartDropDownHandler cart)
        {
            Initialize(mono);
            ResetScreen();
            _cartDropDownHandler = cart;
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