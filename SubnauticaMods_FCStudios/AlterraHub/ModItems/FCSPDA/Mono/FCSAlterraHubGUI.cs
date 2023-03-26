using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Enumerators;
using FCS_AlterraHub.Models.Mono.Handlers;
using FCS_AlterraHub.Models.Mono;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.ScreenItems;
using FCSCommon.Utilities;
using SMLHelper.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Models;
using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using static UWE.Utils;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

public class FCSAlterraHubGUI : MonoBehaviour, IFCSAlterraHubGUI
{
    private Text _cartButtonNumber;
    private Text _cartAmountLabel;
    private Text _cartTotalLabel;
    private Text _clock;
    private Text _currentBiome;
    private Text _accountName;
    private Text _accountBalance;
    private bool _goToEncyclopedia;
    private readonly Dictionary<PDAPages, GameObject> _pages = new();
    private readonly Dictionary<TechType, IuGUIAdditionalPage> _additionalPagesCollection = new();
    private GameObject _storePageGrid;
    private bool _cartLoaded;
    private AccountPageHandler _accountPageHandler;
    private Text _currentBaseInfo;
    private TeleportationPageController _teleportationPageController;
    private GameObject _404;
    internal EncyclopediaTabController EncyclopediaTabController { get; set; }

    private bool _isInitialized;
    private Canvas _canvas;
    private GameObject _toggleHud;
    private MessageBoxHandler _messageBox;
    private GameObject _additionalPages;
    private PDAPages _currentPage;
    private PDADeviceSettingsPage _deviceSettingsPage;
    private RadialMenu _radialMenu;
    private DevicePageController _devicePageController;
    private MenuController _menuController;
    private StorePageController _storePage;

    public bool IsOpen { get; set; }
    public FCSAlterraHubGUISender SenderType { get; set; }

    private void Update()
    {
        if (_clock != null)
        {
            _clock.text = WorldHelpers.GetGameTimeFormat();
        }
    }


    internal void SetInstance(FCSAlterraHubGUISender sender)
    {
        if (_isInitialized) return;

        SenderType = SenderType;
        _canvas = gameObject.GetComponent<Canvas>();
        _messageBox = gameObject.AddComponent<MessageBoxHandler>();
        _currentBiome = GameObjectHelpers.FindGameObject(_canvas.gameObject, "BiomeLBL")?.GetComponent<Text>();

        _404 = GameObjectHelpers.FindGameObject(_canvas.gameObject, "404");

        if (_404 is not null)
        {
            _404.FindChild("Message").GetComponent<Text>().text = LanguageService.Error404();
        }



        _toggleHud = GameObjectHelpers.FindGameObject(_canvas.gameObject, "ToggleHud");
        _accountName = GameObjectHelpers.FindGameObject(_canvas.gameObject, "UserName")?.GetComponent<Text>();
        _currentBaseInfo = GameObjectHelpers.FindGameObject(_canvas.gameObject, "CurrentBaseInfo")?.GetComponent<Text>();
        _accountBalance = GameObjectHelpers.FindGameObject(_canvas.gameObject, "AccountBalance")?.GetComponent<Text>();
        _clock = GameObjectHelpers.FindGameObject(_canvas.gameObject, "Clock")?.GetComponent<Text>();
        _menuController = gameObject.AddComponent<MenuController>();

        AddPages();
        CreateHomePage();
        CreateStorePage();
        CreateDeviceSettingsPage();
        EncyclopediaPage();
        CreateStorePagePage();
        AccountPage();
        DevicePage();
        TeleportationPage();

        _menuController.Initialize();
        //MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol, gameObject, Color.white, 0, 0.01f, 0.01f);
        //MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol, gameObject, 1, 6.15f);
        _messageBox.ObjectRoot = gameObject;
        //MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
        InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
        _isInitialized = true;
    }




    private void AddPages()
    {

        foreach (PDAPages page in Enum.GetValues(typeof(PDAPages)))
        {
            var gPage = GameObjectHelpers.FindGameObject(_canvas.gameObject, page.ToString());
            _pages.Add(page, gPage);
        }
    }

    private void CreateHomePage()
    {
        _menuController.InitialPage = _pages[PDAPages.Home].gameObject.EnsureComponent<BlankPageController>();


        var pageTextLabel = _pages[PDAPages.Home]?.FindChild("PageName")?.GetComponent<Text>();
        _radialMenu = _pages[PDAPages.Home]?.FindChild("RadialMenu")?.AddComponent<RadialMenu>();

        if (_radialMenu is null) return;

        _radialMenu.AddEntry(this, FCSAssetBundlesService.PublicAPI.GetIconByName("Cart_Icon"), pageTextLabel, "Store", PDAPages.Store);
        _radialMenu.AddEntry(this, FCSAssetBundlesService.PublicAPI.GetIconByName("EncyclopediaIcon"), pageTextLabel, "Encyclopedia", PDAPages.Encyclopedia);
        _radialMenu.AddEntry(this, FCSAssetBundlesService.PublicAPI.GetIconByName("IconAccount"), pageTextLabel, "Account", PDAPages.AccountPage);
        _radialMenu.AddEntry(this, FCSAssetBundlesService.PublicAPI.GetIconByName("QuantumTeleporterIcon_W"), pageTextLabel, "Teleportation", PDAPages.Teleportation);
        _radialMenu.AddEntry(this, FCSAssetBundlesService.PublicAPI.GetIconByName("QuantumTeleporterIcon_W"), pageTextLabel, "Base Devices", PDAPages.BaseDevices,false);
    }

    private void CreateStorePage()
    {
        _storePage = _pages[PDAPages.Store].gameObject.EnsureComponent<StorePageController>();
        _storePage.Initialize(this);

        //Set gameobject to toggle for pages
        _pages[PDAPages.HomeSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.LifeSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.EnergySolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.ProductionSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.StorageSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.VehicleSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.MiscSolutions] = _pages[PDAPages.StorePage];
    }

    private void CreateDeviceSettingsPage()
    {
        _deviceSettingsPage = _pages[PDAPages.DeviceSettings]?.EnsureComponent<PDADeviceSettingsPage>();


    }

    private void CreateStorePagePage()
    {
        var storePage = _pages[PDAPages.StorePage].gameObject.EnsureComponent<StoreItemPageController>();
        storePage.Initialize(this);
    }

    

    private void AccountPage()
    {
        _accountPageHandler = _pages[PDAPages.AccountPage].gameObject.EnsureComponent<AccountPageHandler>();
        _accountPageHandler.Initialize(this);
        var backButton = _pages[PDAPages.AccountPage].FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener((() =>
        {
            GoToPage(PDAPages.None);
        }));
    }

    private void TeleportationPage()
    {
        _teleportationPageController = _pages[PDAPages.Teleportation].AddComponent<TeleportationPageController>();
        _teleportationPageController.Initialize(this);

    }

    private void EncyclopediaPage()
    {
        EncyclopediaTabController = _pages[PDAPages.Encyclopedia].AddComponent<EncyclopediaTabController>();
        var backButton = _pages[PDAPages.Encyclopedia].FindChild("BackBTN").GetComponent<Button>();
        backButton.onClick.AddListener((() =>
        {
            GoToPage(PDAPages.None);
        }));
        EncyclopediaTabController.Initialize();
    }



    private void DevicePage()
    {
        _devicePageController = _pages[PDAPages.BaseDevices].AddComponent<DevicePageController>();
        _devicePageController.Initialize(this);

    }

    internal void GoToPage(PDAPages page,object arg = null)
    {
        QuickLogger.Debug($"Goto Page {arg is null}");

        Page currentPage = null;

        if(page == PDAPages.None)
        {
           currentPage =  _menuController.PopPage();

        }
        else
        {
            if(page == PDAPages.DevicePage)
            {
                var data = arg as Tuple<TechType, FCSDevice>;
                QuickLogger.Debug($"data {data?.Item1} | {data?.Item2}", true);
                if (_additionalPagesCollection.TryGetValue(data.Item1, out var ui))
                {
                    QuickLogger.Debug($"data {ui}", true);
                    ui.Initialize(data.Item2);
                    currentPage = (Page)ui;
                }
            }
            else
            {
                currentPage = _pages[page].GetComponent<Page>();
            }
            _menuController.PushPage(currentPage,arg);
        }

        _toggleHud.gameObject.SetActive(currentPage.ShowHud());

        //foreach (KeyValuePair<PDAPages, GameObject> cachedPage in _pages)
        //{
        //    cachedPage.Value?.SetActive(false);
        //}

        //foreach (KeyValuePair<TechType, IuGUIAdditionalPage> cachedPage in _additionalPagesCollection)
        //{
        //    cachedPage.Value?.Hide();
        //}

        //if (_pages.ContainsKey(page))
        //    _pages[page]?.SetActive(true);

        //switch (page)
        //{
        //    case PDAPages.Store:
        //        _toggleHud.gameObject.SetActive(true);
        //        break;
        //    case PDAPages.Encyclopedia:
        //        EncyclopediaTabController.Refresh();
        //        _toggleHud.gameObject.SetActive(true);
        //        break;
        //    case PDAPages.Home:
        //    case PDAPages.StorePage:
        //    case PDAPages.AccountPage:
        //        _toggleHud.gameObject.SetActive(true);
        //        _accountPageHandler.UpdateRequestBTN(AccountService.main.HasBeenRegistered());
        //        break;
        //    case PDAPages.Shipment:
        //        _shipmentPageController.gameObject.SetActive(true);
        //        break;
        //    case PDAPages.Teleportation:
        //        _teleportationPageController.Refresh();
        //        break;
        //    case PDAPages.DevicePage:
        //        var data = arg as Tuple<TechType, FCSDevice>;
        //        QuickLogger.Debug($"data {data?.Item1} | {data?.Item2}",true);
        //        if (_additionalPagesCollection.TryGetValue(data.Item1, out var ui))
        //        {
        //            QuickLogger.Debug($"data {ui}", true);
        //            ui.Initialize(data.Item2);
        //        }
        //        break;
        //    case PDAPages.DeviceSettings:
        //        var data2 = arg as FCSDevice;
        //        _deviceSettingsPage.Show(this, data2);
        //        break;
        //    case PDAPages.BaseDevices:
        //        _devicePageController.Open();
        //        break;
        //    default:
        //        LoadStorePage(page);
        //        _toggleHud.gameObject.SetActive(false);
        //        break;
        //}

        _currentPage = page;
    }

    internal PDAPages CurrentPage() => _currentPage;


    internal bool GetAutomaticDebitDeduction()
    {
        return _accountPageHandler.GetAutomaticDebitDeduction();
    }

    internal void ShowMission()
    {
        // uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowMessage("Hi","Eggo"); 
    }

    internal void RefreshTeleportationPage()
    {
        _teleportationPageController.Refresh();
    }

    internal void TryRemove404Screen()
    {
        //TODO Drone System
        //if (DroneDeliveryService.Main == null)
        //{
        //    _messageBox.Show(LanguageService.ErrorHasOccurred("0x0001"), FCSMessageButton.OK);
        //    return;
        //}

        //_404?.SetActive(!DroneDeliveryService.Main.DetermineIfFixed());
    }

    internal void UpdateDisplay()
    {
        if (_currentBiome == null || _accountName == null || _currentBaseInfo == null) return;

        _currentBiome.text = Player.main.GetBiomeString();

        if (!string.IsNullOrWhiteSpace(AccountService.main.GetUserName()))
            _accountName.text = AccountService.main.GetUserName();

        _accountBalance.text = $"{AccountService.main.GetAccountBalance():N0}";

        var currentBase = HabitatService.GetPlayersCurrentBase();
        var friendly = currentBase?.GetBaseFriendlyName();
        var baseId = currentBase?.GetBaseID() ?? 0;

        if (!string.IsNullOrWhiteSpace(friendly))
        {
            SetCurrentBaseInfoText($"{friendly} | {LanguageService.BaseIDFormat(baseId.ToString("D3"))}");
        }
        else
        {
            SetCurrentBaseInfoText("N/A");
        }
    }

    private void SetCurrentBaseInfoText(string text)
    {
        _currentBaseInfo.text = $"Current Base : {text}";
    }

    internal void OpenEncyclopedia(TechType techType)
    {
        if (CheckIfPDAHasEntry(techType))
        {
            GoToPage(PDAPages.Encyclopedia);
            EncyclopediaTabController.OpenEntry(techType);
        }
        else
        {
            QuickLogger.ModMessage($"AlterraHub PDA doesn't have any entry for {Language.main.Get(techType)}");
        }
    }

    internal void OpenEncyclopedia(string techType)
    {
        if (CheckIfPDAHasEntry(techType))
        {
            GoToPage(PDAPages.Encyclopedia);
            EncyclopediaTabController.OpenEntry(techType);
        }
        else
        {
            QuickLogger.ModMessage($"AlterraHub PDA doesn't have any entry for {Language.main.Get(techType)}");
        }
    }

    internal bool CheckIfPDAHasEntry(TechType techType)
    {
        return EncyclopediaTabController.HasEntry(techType);
    }

    internal bool CheckIfPDAHasEntry(string techType)
    {
        return EncyclopediaTabController.HasEntry(techType);
    }

    internal CraftNode GetCraftTree()
    {
        return EncyclopediaTabController.Tree;
    }

    internal float GetRate()
    {
        return _accountPageHandler.GetRate();
    }

    internal void CloseAccountPage()
    {
        _accountPageHandler.Close();
    }

    private void OnDestroy()
    {
        _accountPageHandler = null;
    }

    internal void LoadFromSave(ShipmentInfo shipmentInfo)
    {
        if (shipmentInfo != null)
        {
            _storePage.LoadSave(shipmentInfo);
        }

        _accountPageHandler.Refresh();

        _cartLoaded = true;
    }

    void IFCSAlterraHubGUI.ShowMessage(string message)
    {
        _messageBox.Show(message, FCSMessageButton.OK);
    }

    internal void AddAdditionalPage(TechType id, GameObject uiPrefab)
    {
        QuickLogger.Info("Adding Additional Page");
        if (_additionalPages is null)
        {
            _additionalPages = GameObjectHelpers.FindGameObject(gameObject, "AdditionalPages");
        }

        var ui = Instantiate(uiPrefab);
        ui.SetActive(false);
        ui.transform.SetParent(_additionalPages.gameObject.transform, false);
        var interf = ui.GetComponent<IuGUIAdditionalPage>();
        interf.onBackClicked += (s) => {
            GoToPage(s);
        };
        interf.onSettingsClicked += (device)=> {
            GoToPage(PDAPages.DeviceSettings, device);
        };
        _additionalPagesCollection.Add(id, interf);
        QuickLogger.Info("Added Additional Page");

    }

    internal void PrepareDevicePage(TechType id, FCSDevice fcsDevice)
    {
        GoToPage(PDAPages.DevicePage,new Tuple<TechType,FCSDevice>(id, fcsDevice));
    }

    internal void PurgePages()
    {
        _menuController.PopAllPages();
    }

    /// <summary>
    /// Gets the GameObject of the requested <see cref="PDAPages"/>
    /// </summary>
    /// <param name="page">The page to retrieve</param>
    /// <returns>Returns the <see cref="GameObject"/> of the supplied <see cref="PDAPages"/></returns>
    internal GameObject GetPage(PDAPages page)
    {
        if (!_pages.ContainsKey(page)) return null;
        return _pages[page];
    }
}