using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.Models.Mono.Handlers;
using FCS_AlterraHub.Models.Mono;
using FCSCommon.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;
using FCS_AlterraHub.Models.Interfaces;
using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.ModItems.FCSPDA.Interfaces;
using FCS_AlterraHub.Models.Abstract;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.Model;
using static HandReticle;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

public class FCSAlterraHubGUI : MonoBehaviour, IFCSAlterraHubGUI
{


    [SerializeField]
    private Text _clock;


    private bool _goToEncyclopedia;

    private readonly Dictionary<PDAPages, GameObject> _pages = new();
    private readonly Dictionary<TechType, IuGUIAdditionalPage> _additionalPagesCollection = new();

    [SerializeField]
    private AccountPageHandler _accountPageHandler;

    [SerializeField]
    private TeleportationPageController _teleportationPageController;
    [SerializeField]
    private GameObject _404;

    public Action<TechType> OnInfoButtonClicked;

    [SerializeField]
    private EncyclopediaTabController _encyclopediaTabController;


    private bool _isInitialized;

    private Canvas _canvas;

    [SerializeField]
    private GameObject _toggleHud;

    [SerializeField]
    private MessageBoxHandler _messageBox;
    [SerializeField]
    private GameObject _additionalPages;

    private PDAPages _currentPage;

    [SerializeField]
    private CartDropDownHandler _cartDropDownController;
    [SerializeField]
    private MenuController _menuController;
    [SerializeField]
    private StorePageController _storePage;

    public bool IsOpen { get; set; }
    public FCSAlterraHubGUISender SenderType { get; set; }

    private void Awake()
    {
        _menuController = gameObject.GetComponent<MenuController>();
        _canvas = gameObject.GetComponent<Canvas>();
        _cartDropDownController.Initialize();
    }

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
        if (_404 is not null)
        {
            _404.FindChild("Message").GetComponent<Text>().text = LanguageService.Error404();
        }
        
        AddPages();
        CreateStorePage();

        OnInfoButtonClicked += onInfoButtonClicked;
        //MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol, gameObject, Color.white, 0, 0.01f, 0.01f);
        //MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol, gameObject, 1, 6.15f);
        //MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
        _isInitialized = true;
    }

    private void onInfoButtonClicked(TechType techType)
    {
        FCSPDAController.Main.GetGUI().OpenEncyclopedia(techType);
    }

    private void AddPages()
    {

        foreach (PDAPages page in Enum.GetValues(typeof(PDAPages)))
        {
            var gPage = GameObjectHelpers.FindGameObject(_canvas.gameObject, page.ToString());
            _pages.Add(page, gPage);
        }
    }

    private void CreateStorePage()
    {
        //Set gameobject to toggle for pages
        _pages[PDAPages.HomeSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.LifeSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.EnergySolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.ProductionSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.StorageSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.VehicleSolutions] = _pages[PDAPages.StorePage];
        _pages[PDAPages.MiscSolutions] = _pages[PDAPages.StorePage];
    }

    internal void GoToPage(PDAPages page,object arg = null)
    {
        QuickLogger.Debug($"Goto Page: Is Arg null =  {arg is null}");

        Page currentPage = null;

        _currentPage = page;

        if (page == PDAPages.None)
        {
           currentPage =  _menuController.PopPage();
        }
        else
        {
            if(page == PDAPages.DevicePage)
            {
                var data = arg as Tuple<TechType, FCSDevice>;
                if (_additionalPagesCollection.TryGetValue(data.Item1, out var ui))
                {
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
    }

    internal PDAPages CurrentPage() => _currentPage;

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

    
    internal void OpenEncyclopedia(TechType techType)
    {
        if (CheckIfPDAHasEntry(techType))
        {            
            QuickLogger.Debug($"Openning PDA Entry of techType:{techType}");
            EncyclopediaService.SetSelectedEntry(techType);
            GoToPage(PDAPages.Encyclopedia, EncyclopediaService.GetEntryByTechType(techType));
            EncyclopediaService.ClearSelectedEntry();
        }
        else
        {
            QuickLogger.ModMessage($"AlterraHub PDA doesn't have any entry for {Language.main.Get(techType)}");
        }
    }

    internal bool CheckIfPDAHasEntry(TechType techType)
    {
        return _encyclopediaTabController?.HasEntry(techType) ?? false;
    }

    internal bool CheckIfPDAHasEntry(string techType)
    {
        return _encyclopediaTabController.HasEntry(techType);
    }

    internal void CloseAccountPage()
    {
        _accountPageHandler.Close();
    }

    private void OnDestroy()
    {
        _accountPageHandler = null;
        OnInfoButtonClicked -= _encyclopediaTabController.OpenEntry;
    }

    internal void LoadFromSave(ShipmentInfo shipmentInfo)
    {
        if (shipmentInfo != null)
        {
            _storePage.LoadSave(shipmentInfo);
        }

        _accountPageHandler.Refresh();
    }

    public void ShowMessage(string message)
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

    public void GoBackAPage()
    {
        GoToPage(PDAPages.None);
    }

    internal ShipmentInfo GetShipmentInfo()
    {
        return _storePage.GetShipmentInfo();
    }

    internal PDAPages GetCurrentPage()
    {
        return _currentPage;
    }

    internal EncyclopediaTabController GetEncyclopediaTabController()
    {
        return _encyclopediaTabController;
    }
}