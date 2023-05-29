using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
internal class HomePageController : Page
{
    [SerializeField]
    private RadialMenu _radialMenu;
    [SerializeField]
    private Text _pageTextLabel;
    [SerializeField]
    private FCSAlterraHubGUI _gui;

    public override PDAPages PageType => PDAPages.Home;

    public override void OnBackButtonClicked()
    {
        _gui.GoBackAPage();
    }

    private void Awake()
    {
        if (_radialMenu is null) return;
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("Cart_Icon"), _pageTextLabel, "Store", PDAPages.Store);
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("EncyclopediaIcon"), _pageTextLabel, "Encyclopedia", PDAPages.EncyclopediaMain);
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("IconAccount"), _pageTextLabel, "Account", PDAPages.AccountPage);
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("QuantumTeleporterIcon_W"), _pageTextLabel, "Teleportation", PDAPages.Teleportation);
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("HomeSolutionsIcon_W"), _pageTextLabel, "Base Devices", PDAPages.BaseDevices, false);
    }

    public override bool OnButtonDown(GameInput.Button button)
    {
        base.OnButtonDown(button);

        if (button == GameInput.Button.UIUp || button == GameInput.Button.UIRight)
        {
            //QuickLogger.Debug("UP BUTTON PRESSED", true);
            _radialMenu.SelectNextItem();
        }

        if (button == GameInput.Button.UIDown || button == GameInput.Button.UILeft)
        {
            //QuickLogger.Debug("UP BUTTON PRESSED", true);
            _radialMenu.SelectPrevItem();
        }

        if(button == GameInput.Button.LeftHand)
        {
            _radialMenu.PressSelectedButton();
        }

        return false;
    }

    public override void Exit()
    {
        _radialMenu.ClearSelectedItem();
        base.Exit();
    }
}