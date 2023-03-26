using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono
{
    internal class StorePageController : Page
    {
        private Text _cartButtonNumber;
        protected override bool showHud => false;
        private FCSAlterraHubGUI _gui;
        private CartDropDownHandler _cartDropDownManager;
        private CheckOutPopupDialogWindow _checkoutDialog;
        private ReturnsDialogController _returnsDialogController;
        private ShipmentPageController _shipmentPageController;


        internal void Initialize(FCSAlterraHubGUI gui)
        {
            _gui = gui;
            _checkoutDialog = GameObjectHelpers.FindGameObject(gui.gameObject, "CheckOutPopUp")?.AddComponent<CheckOutPopupDialogWindow>();

            _returnsDialogController = GameObjectHelpers.FindGameObject(gui.gameObject, "ReturnItemsDialog")?.AddComponent<ReturnsDialogController>();
            _returnsDialogController?.Initialize(gui);

            _cartButtonNumber = GameObjectHelpers.FindGameObject(gameObject, "CartCount")?.GetComponentInChildren<Text>();
            var pageTextLabel = gameObject.FindChild("PageName")?.GetComponent<Text>();
            var radialMenu = gameObject.FindChild("RadialMenu")?.AddComponent<RadialMenu>();

            var cartBTN = gameObject.FindChild("Cart")?.GetComponent<Button>();
            if (cartBTN != null)
            {
                cartBTN.onClick.AddListener((() =>
                {
                    _cartDropDownManager.ToggleVisibility();
                }));
            }

            var returnsBTN = gameObject.FindChild("Returns").GetComponent<Button>();
            returnsBTN.onClick.AddListener(() =>
            {
                _returnsDialogController.Open();
            });

            var backButton = gameObject.FindChild("BackBTN")?.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.AddListener((() =>
                {
                    gui.GoToPage(PDAPages.None);
                }));
            }

            if (radialMenu is not null)
            {
                // TODO Figure out how to allor paths to icon FCSAssetBundlesService.PublicAPI.GetIconByName("Cart_Icon", Main.MODNAME)
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("HomeSolutionsIcon_W", Main.MODNAME), pageTextLabel, "Home Solutions", PDAPages.HomeSolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("LifeSupportIcon_W", Main.MODNAME), pageTextLabel, "Life Solutions", PDAPages.LifeSolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("EnergySolutionsIcon_W", Main.MODNAME), pageTextLabel, "Energy Solutions", PDAPages.EnergySolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("ProductionSolutionsIcon_W", Main.MODNAME), pageTextLabel, "Production Solutions", PDAPages.ProductionSolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("StoreSolutionsIcon_W", Main.MODNAME), pageTextLabel, "Storage Solutions", PDAPages.StorageSolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("VehicleSolutionsIcon_W", Main.MODNAME), pageTextLabel, "Vehicle Solutions", PDAPages.VehicleSolutions);
                radialMenu.AddEntry(gui, FCSAssetBundlesService.PublicAPI.GetIconByName("MiscIcon_W", Main.MODNAME), pageTextLabel, "Misc", PDAPages.MiscSolutions);
            }
            radialMenu?.Rearrange();

            CreateCartDropDown();

            LoadShipmentPage();
        }

        private void CreateCartDropDown()
        {

            _cartDropDownManager = GameObjectHelpers.FindGameObject(_gui.gameObject, "CartDropDown")?.AddComponent<CartDropDownHandler>();

            if (_cartDropDownManager is not null)
            {
                _cartDropDownManager.OnBuyAllBtnClick += OnBuyAllBtnClick;
                _cartDropDownManager.Initialize();
                _cartDropDownManager.onTotalChanged += amount =>
                {
                    _cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
                };
            }
        }

        private void LoadShipmentPage()
        {

            var shipmentButton = gameObject.FindChild("ShipmentBTN").GetComponent<Button>();
            shipmentButton.onClick.AddListener((() =>
            {
                _gui.GoToPage(PDAPages.Shipment);
            }));

            _shipmentPageController = _gui.GetPage(PDAPages.Shipment).gameObject.EnsureComponent<ShipmentPageController>();
            _shipmentPageController.Initialize(_gui);
        }

        internal void AddShipment(Shipment shipment)
        {
            _shipmentPageController.AddItem(shipment);
        }

        internal void RemoveShipment(Shipment shipment)
        {
            _shipmentPageController.RemoveItem(shipment);
        }

        private void OnBuyAllBtnClick(CartDropDownHandler obj)
        {
            _checkoutDialog.ShowDialog(_gui, _cartDropDownManager);
            _cartDropDownManager.ToggleVisibility();
        }

        private void OnDestroy()
        {
            _cartDropDownManager.OnBuyAllBtnClick -= OnBuyAllBtnClick;
        }

        internal ShipmentInfo GetShipmentInfo()
        {
            return _cartDropDownManager.GetShipmentInfo();
        }

        public override void Enter(object arg = null)
        {
            base.Enter(arg);
        }

        internal void AttemptToOpenReturnsDialog()
        {
            if (_returnsDialogController?.IsOpen ?? false)
            {
                _returnsDialogController.Open();
            }
        }

        internal void LoadSave(ShipmentInfo shipmentInfo)
        {
            _cartDropDownManager.LoadShipmentInfo(shipmentInfo);
        }
    }
}