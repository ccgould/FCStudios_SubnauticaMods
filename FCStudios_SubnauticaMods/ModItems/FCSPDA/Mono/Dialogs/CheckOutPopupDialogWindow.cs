using System.Collections.Generic;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Interfaces;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.ModItems.FCSPDA.Struct;
using FCS_AlterraHub.ModItems.Spawnables.DebitCard.Spawnable;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if SUBNAUTICA
using TechData = CraftData;
#endif

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;

internal class CheckOutPopupDialogWindow : Page
{
    [SerializeField] private Text _accountBalance;
    [SerializeField] private Text _total;
    [SerializeField] private Text _newBalance;
    [SerializeField] private Text _destinationText;
    [SerializeField] private CartDropDownHandler _cartDropDownHandler;
    [SerializeField] private FCSAlterraHubGUI _gui;

    private bool _isInitialized;



    public UnityEvent onCheckOutPopupDialogClosed = new UnityEvent();
    private DestinationDialogController _destinationDialogController;

    internal IShippingDestination SelectedDestination { get; set; }


    public override void Enter(object arg = null)
    {
        base.Enter(arg);

        Initialize();

        //gameObject.SetActive(true);
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        //CreatePurchaseButton();
        //CreatePurchaseExitButton();
        CreateDestinationPopup();
        //CreateDestinationButton();
        //CreateBackButton();
        AccountService.main.onBalanceUpdated += UpdateScreen;
        ResetScreen();
        UpdateScreen();

        _isInitialized = true;
    }

    //private void CreateBackButton()
    //{
    //    var backBtn = GameObjectHelpers.FindGameObject(gameObject, "CloseBTN").GetComponent<Button>();

    //    backBtn.onClick.AddListener(() => { HideDialog(); });
    //}

    public void OnDestinationBtnClicked()
    {
        _destinationDialogController.Open();
    }

    //private void CreatePurchaseButton()
    //{
    //    //var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject, "PurchaseBTN").GetComponent<Button>();
    //    //purchaseBTN.onClick.AddListener(() =>
    //    //{
    //    //    QuickLogger.Debug("Purchase Button Clicked", true);
    //    //    MakePurchase();
    //    //});
    //}

    public void MakePurchase()
    {
        var totalSize = new List<Vector2int>();
        foreach (CartItemSaveData cartItem in _cartDropDownHandler.GetItems())
        {
            for (int i = 0; i < cartItem.ReturnAmount; i++)
            {
                totalSize.Add(TechData.GetItemSize(cartItem.TechType));
            }
        }

        if (!Inventory.main.container.HasRoomFor(totalSize))
        {
            QuickLogger.ModMessage(LanguageService.InventoryFull());
            return;
        }

        if (SelectedDestination == null)
        {
            _gui.ShowMessage(LanguageService.NoDestinationFound());
            return;
        }

        //if (!SelectedDestination.HasDepot())
        //{
        //    //VoiceNotificationSystem.main.Play("PDA_Drone_Instructions_key");
        //    return;
        //}

        if (!AccountService.main.HasBeenRegistered())
        {
            _gui.ShowMessage(LanguageService.AccountNotFoundFormat());
        }

        if (Inventory.main.container.GetCount(DebitCardSpawnable.PatchedTechType) <= 0)
        {
            _gui.ShowMessage(LanguageService.CardNotDetected());
        }

        if (AccountService.main.HasEnough(_cartDropDownHandler.GetTotal()))
        {
            _cartDropDownHandler.GetShipmentInfo().DestinationID = SelectedDestination.GetPreFabID();

            _cartDropDownHandler.GetShipmentInfo().BaseName = SelectedDestination.GetBaseName();


            if (StoreManager.main.CompleteOrder(_cartDropDownHandler, _cartDropDownHandler.GetShipmentInfo()))
            {
                _cartDropDownHandler.TransactionComplete();
                HideDialog();
            }
        }
        else
        {
            _gui.ShowMessage(LanguageService.NotEnoughMoneyOnAccount());
        }
    }

    //private void CreatePurchaseExitButton()
    //{
    //    //var purchaseBTN = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "PurchaseExitBTN").GetComponent<Button>();
    //    //purchaseBTN.onClick.AddListener(() =>
    //    //{
    //    //    QuickLogger.Debug("Purchase Button Clicked", true);

    //    //});
    //}

    private void CreateDestinationPopup()
    {
        var destinationPopDiag = GameObjectHelpers.FindGameObject(gameObject.transform.parent.gameObject, "DestinationPopUp");
        _destinationDialogController = destinationPopDiag.GetComponent<DestinationDialogController>();
        _destinationDialogController.Initialize();
        _destinationDialogController.OnClose += () =>
        {
            //_destinationText.text = $"DestinationID: {SelectedDestination?.GetBaseName()}";
        };
    }

    public void OnDestinationChanged(AlterraHubDepotItemController toggle)
    {
        if(toggle.IsChecked)
        {
            SelectedDestination = toggle.Destination;
        }
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

    internal void HideDialog()
    {
        ResetScreen();
        onCheckOutPopupDialogClosed?.Invoke();
        Exit();
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