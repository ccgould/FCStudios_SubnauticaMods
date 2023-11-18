using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.Models.Structs;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

internal class StorePageController : Page
{
    [SerializeField]
    private Text _cartButtonNumber;
    [SerializeField]
    private Text _pageTextLabel;
    [SerializeField]
    private FCSAlterraHubGUI _gui;
    [SerializeField]
    private CartDropDownHandler _cartDropDownManager;
    [SerializeField]
    private CheckOutPopupDialogWindow _checkoutDialog;
    [SerializeField]
    private ReturnsDialogController _returnsDialogController;
    [SerializeField]
    private ShipmentPageController _shipmentPageController;
    [SerializeField]
    private RadialMenu _radialMenu;
    [SerializeField]
    private Button _shipmentButton;


    public override void Awake()
    {
        base.Awake();
        var returnsBTN = gameObject.FindChild("Returns").GetComponent<Button>();
        returnsBTN.onClick.AddListener(() =>
        {
            _returnsDialogController.Open();
        });

        if (_radialMenu is not null)
        {
            // TODO Figure out how to allor paths to icon FCSAssetBundlesService.PublicAPI.GetIconByName("Cart_Icon", PluginInfo.PLUGIN_NAME)
            //if(Mod)


            foreach (StoreModCategory mod in StoreManager.main.GetRegisteredMods())
            {
                _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName(mod.IconName, mod.ModPackGUID), _pageTextLabel, mod.PageName, mod.PDAPage);
            }

            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("HomeSolutionsIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Home Solutions", PDAPages.HomeSolutions);
            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("LifeSupportIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Life Solutions", PDAPages.LifeSolutions);
            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("EnergySolutionsIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Energy Solutions", PDAPages.EnergySolutions);
            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("ProductionSolutionsIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Production Solutions", PDAPages.ProductionSolutions);
            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("StoreSolutionsIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Storage Solutions", PDAPages.StorageSolutions);
            //_radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("VehicleSolutionsIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Vehicle Solutions", PDAPages.VehicleSolutions);
            _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("MiscIcon_W", PluginInfo.PLUGIN_NAME), _pageTextLabel, "Misc", PDAPages.MiscSolutions);
        }
        _radialMenu?.Rearrange();

        LoadShipmentPage();
    }

    public void UpdateCartTotals()
    {
        _cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
    }

    private void LoadShipmentPage()
    {
        _shipmentButton.onClick.AddListener((() =>
        {
            _gui.GoToPage(PDAPages.Shipment);
        }));

    }

    internal void AddShipment(Shipment shipment)
    {
        _shipmentPageController.AddItem(shipment);
    }

    internal void RemoveShipment(Shipment shipment)
    {
        _shipmentPageController.RemoveItem(shipment);
    }

    public void OnBuyAllBtnClick()
    {
        _checkoutDialog.Enter();
        _cartDropDownManager.ToggleVisibility();
    }

    internal ShipmentInfo GetShipmentInfo()
    {
        return _cartDropDownManager.GetShipmentInfo();
    }

    public override void Enter(object arg = null)
    {
        base.Enter(arg);
        UpdateCartTotals();
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