using System.Collections.Generic;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
#if SUBNAUTICA
using TechData = CraftData;
#endif

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs
{
    internal class CheckOutPopupDialogWindow : MonoBehaviour
    {
        private Text _accountBalance;
        private Text _total;
        private Text _newBalance;

        private bool _isInitialized;
        private IFCSAlterraHubGUI _mono;

        public CartDropDownHandler _cartDropDownHandler;
        public UnityEvent onCheckOutPopupDialogClosed = new UnityEvent();
        private DestinationDialogController _destinationDialogController;
        private Text _destinationText;
        //internal IShippingDestination SelectedDestination { get; set; }


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
            CreateDestinationButton();
            CreateBackButton();
            AccountService.main.onBalanceUpdated += UpdateScreen;

            _isInitialized = true;
        }

        private void CreateBackButton()
        {
            var backBtn = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();

            backBtn.onClick.AddListener(HideDialog);
        }

        private void CreateDestinationButton()
        {
            var destinationButton = GameObjectHelpers.FindGameObject(gameObject, "DestinationBTN").GetComponent<Button>();
            destinationButton.onClick.AddListener(() => { _destinationDialogController.Open(); });
        }

        private void CreatePurchaseButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN").GetComponent<Button>();
            purchaseBTN.onClick.AddListener(() =>
            {
                QuickLogger.Debug("Purchase Button Clicked", true);
                MakePurchase();
            });
        }

        private bool MakePurchase()
        {
            var totalSize = new List<Vector2int>();
            foreach (CartItemSaveData cartItem in _cartDropDownHandler.GetItems())
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
                    totalSize.Add(TechData.GetItemSize(cartItem.TechType));
                }
            }

            //if (!Inventory.main.container.HasRoomFor(totalSize))
            //{
            //    QuickLogger.ModMessage(Buildables.AlterraHub.InventoryFull());
            //    return false;
            //}

            //TODO Destination fix

            //if (SelectedDestination == null)
            //{
            //    _mono.ShowMessage(LanguageService.NoDestinationFound());
            //    return false;
            //}

            //if (!SelectedDestination.HasDepot())
            //{
            //    VoiceNotificationSystem.main.Play("PDA_Drone_Instructions_key");
            //    return false;
            //}

            if (!AccountService.main.HasBeenRegistered())
            {
                _mono.ShowMessage(LanguageService.AccountNotFoundFormat());
                return false;
            }

            if (Inventory.main.container.GetCount(DebitCardSpawnable.PatchedTechType) <= 0)
            {
                _mono.ShowMessage(LanguageService.CardNotDetected());
                return false;
            }

            if (AccountService.main.HasEnough(_cartDropDownHandler.GetTotal()))
            {
                QuickLogger.Debug("1");

                //_cartDropDownHandler.GetShipmentInfo().DestinationID = SelectedDestination.GetPreFabID();

                //_cartDropDownHandler.GetShipmentInfo().BaseName = SelectedDestination.GetBaseName();


                if (StoreManager.main.CompleteOrder(_cartDropDownHandler, _cartDropDownHandler.GetShipmentInfo()))
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
                _mono.ShowMessage(LanguageService.NotEnoughMoneyOnAccount());
            }
            return false;
        }

        private void CreatePurchaseExitButton()
        {
            var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "PurchaseExitBTN").GetComponent<Button>();
            purchaseBTN.onClick.AddListener(() =>
            {
                QuickLogger.Debug("Purchase Button Clicked", true);
                if (MakePurchase())
                {
                    //_mono.ExitStore();
                }
            });
        }

        private void CreateDestinationPopup()
        {
            var destinationPopDiag = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "DestinationPopUp");
            _destinationDialogController = destinationPopDiag.AddComponent<DestinationDialogController>();
            _destinationDialogController.Initialize(this);
            _destinationDialogController.OnClose += () =>
            {
                //_destinationText.text = $"DestinationID: {SelectedDestination?.GetBaseName()}";
            };
        }

        private void UpdateScreen()
        {
            QuickLogger.Debug("Updating Screen", true);
            if (!PlayerInteractionHelper.HasCard())
            {
                _accountBalance.text = LanguageService.AccountBalanceFormat(0);
                _total.text = LanguageService.CheckOutTotalFormat(_cartDropDownHandler?.GetTotal() ?? 0);
                _newBalance.text = LanguageService.AccountNewBalanceFormat(0);
            }
            else
            {
                _accountBalance.text = LanguageService.AccountBalanceFormat(AccountService.main.GetAccountBalance());
                _total.text = LanguageService.CheckOutTotalFormat(_cartDropDownHandler.GetTotal());
                _newBalance.text = LanguageService.AccountNewBalanceFormat(AccountService.main.GetAccountBalance() - _cartDropDownHandler.GetTotal());
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
            _accountBalance.text = LanguageService.AccountBalanceFormat(0);
            _total.text = LanguageService.CheckOutTotalFormat(0);
            _newBalance.text = LanguageService.CheckOutTotalFormat(0);
            _destinationText.text = "DestinationID:";
            //SelectedDestination = null;
        }
    }
}