using FCS_AlterraHub.Core.Helpers;
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
using FCS_AlterraHub.Core.Navigation;
using FCS_AlterraHub.ModItems.FCSPDA.Mono.uGUIComponents;

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

public class FCSAlterraHubGUI : uGUI_InputGroup, IFCSAlterraHubGUI
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
    public Action OnStorageButtonClicked;

    [SerializeField]
    private EncyclopediaTabController _encyclopediaTabController;

    [AssertNotNull]
    public CanvasGroup content;

    private bool _isInitialized;

    private Canvas _canvas;

    [SerializeField]
    private GameObject _toggleHud;

    [SerializeField]
    private uGUI_MessageBoxHandler _messageBox;
    [SerializeField]
    private GameObject _additionalPages;

    private PDAPages _currentPage;

    [SerializeField]
    private CartDropDownHandler _cartDropDownController;
    [SerializeField]
    private MenuController _menuController;
    [SerializeField]
    private StorePageController _storePage;
    private List<RectMask2D> rectMasks = new List<RectMask2D>();
    private List<ScrollRect> scrollRects = new List<ScrollRect>();
    [AssertNotNull]
    public uGUI_CanvasScaler canvasScaler;

    [AssertNotNull]
    public CanvasGroup canvasGroup;

    [SerializeField]
    private uGUI_PDANavigationController _uGUI_PDANavigationController;
    private IFCSObject _currentDevice;

    public bool IsOpen { get; set; }
    public FCSAlterraHubGUISender SenderType { get; set; }

    public override void Awake()
    {
        base.Awake();
        base.GetComponentsInChildren<RectMask2D>(true, this.rectMasks);
        base.GetComponentsInChildren<ScrollRect>(true, this.scrollRects);
        this.SetCanvasVisible(false);
        _menuController.OnMenuContollerPop += _menuController_OnMenuContollerChange;
        _menuController.OnMenuContollerPush += _menuController_OnMenuContollerChange;
        _uGUI_PDANavigationController.onSettingsClicked += (device) =>
        {
            GoToPage(PDAPages.DeviceSettings, device);
        };
    }

    private void _menuController_OnMenuContollerChange(object sender, MenuController.OnMenuControllerEventArg e)
    {
        _uGUI_PDANavigationController.SetNavigationButtons(e.Page);
    }

    public void Initialize()
    {
        _canvas = gameObject.GetComponent<Canvas>();
        _cartDropDownController.Initialize();
        SetInstance(FCSAlterraHubGUISender.PDA);
    }

    public override void Update()
    {
        base.Update();
        if (_clock != null)
        {
            _clock.text = WorldHelpers.GetGameTimeFormat();
        }

        if (!base.selected && FCSPDAController.Main.isOpen && AvatarInputHandler.main.IsEnabled())
        {
            base.Select(false);
        }
        if (!uGUI.isIntro)
        {
            FPSInputModule.current.EscapeMenu();
        }
    }

    internal void SetInstance(FCSAlterraHubGUISender sender)
    {
        if (_isInitialized) return;

        SenderType = sender;
        if (_404 is not null)
        {
            _404.FindChild("Message").GetComponent<Text>().text = LanguageService.Error404();
        }
        
        AddPages();
        CreateStorePage();

        OnInfoButtonClicked += onInfoButtonClicked;
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
        _pages[PDAPages.AlterraHub] = _pages[PDAPages.StorePage];
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
        QuickLogger.Debug($"Goto Page: {page} Is Arg null =  {arg is null}");

        Page currentPage = null;
        object resultArg = arg;

        _currentPage = page;

        if(page != PDAPages.None)
        {
            if(page == PDAPages.DevicePage)
            {
                var data = arg as Tuple<TechType, IFCSObject>;

                QuickLogger.Debug($"{data.Item1} || {data.Item2}");

                if (_additionalPagesCollection.TryGetValue(data.Item1, out var ui))
                {
                    //ui.Enter(data.Item2);
                    resultArg = data.Item2;
                    _uGUI_PDANavigationController.RegisterPage(ui);
                    currentPage = (Page)ui;
                    _currentDevice = data.Item2;
                }
            }
            else
            {
                currentPage = _pages[page].GetComponent<Page>();
            }

            _menuController.PushPage(currentPage, resultArg);
        }
        else
        {
            currentPage  = _menuController.PopAndPeek();
            _currentPage = currentPage.PDAGetPageType();
        }

        _toggleHud.gameObject.SetActive(currentPage?.ShowHud() ?? false);
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

    internal void OpenEncyclopedia(FCSDevice device)
    {
        PurgeData();
        EncyclopediaService.SetCurrentDevice(device);
        GoToPage(PDAPages.BaseDevices);
        PrepareDevicePage(device.GetTechType(),device);
        OpenEncyclopedia(device.GetTechType());
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
        _menuController.OnMenuContollerPop -= _menuController_OnMenuContollerChange;
        _menuController.OnMenuContollerPush -= _menuController_OnMenuContollerChange;
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
        uGUI_MessageBoxHandler.Instance.ShowMessage(message,FCSMessageButton.OK);

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
        _additionalPagesCollection.Add(id, interf);
        QuickLogger.Info("Added Additional Page");

    }

    internal void PrepareDevicePage(TechType id, IFCSObject fcsDevice)
    {
        GoToPage(PDAPages.DevicePage,new Tuple<TechType,IFCSObject>(id, fcsDevice));
    }

    /// <summary>
    /// Purges the current device and closes all pages to home.
    /// </summary>
    internal void PurgeData()
    {
        _menuController.PopAllPages();
        _currentDevice = null;
    }

    public IFCSObject GetCurrentDevice()
    {
        return _currentDevice;
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

    public void OnPDAOpened()
    {
        this.content.interactable = true;
        this.content.blocksRaycasts = true;
    }

    public void OnPDAClosed()
    {
        this.SetCanvasVisible(false);
    }

    private void SetCanvasVisible(bool visible)
    {
        //this.canvasGroup.SetVisible(visible);
        this.canvasScaler.active = visible;
        foreach (RectMask2D rectMask2D in this.rectMasks)
        {
            rectMask2D.enabled = visible;
        }
        foreach (ScrollRect scrollRect in this.scrollRects)
        {
            scrollRect.enabled = visible;
        }
    }

    public void OnOpenPDA(PDATab tabId)
    {
        this.SetCanvasVisible(true);
        this.content.interactable = false;
        this.content.blocksRaycasts = false;
        this.content.alpha = 1f;
    }

    public void OnClosePDA()
    {
        base.Deselect();
        this.content.SetVisible(false);
    }

    public void SetPDAAdditionalLabel(string value)
    {
        _uGUI_PDANavigationController.SetLabel(value);
    }

    public MenuController GetMenuController()
    {
        return _menuController;
    }
}