using FCS_AlterraHub.API;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
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
        _radialMenu.AddEntry(_gui, FCSAssetBundlesService.PublicAPI.GetIconByName("QuantumTeleporterIcon_W"), _pageTextLabel, "Base Devices", PDAPages.BaseDevices, false);
    }
}