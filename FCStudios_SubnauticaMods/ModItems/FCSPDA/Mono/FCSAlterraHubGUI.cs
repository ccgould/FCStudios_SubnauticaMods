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

namespace FCS_AlterraHub.ModItems.FCSPDA.Mono;

public class FCSAlterraHubGUI : uGUI_InputGroup, IFCSAlterraHubGUI, uGUI_IButtonReceiver
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

    [AssertNotNull]
    public CanvasGroup content;

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
    private PDAMenuController _menuController;
    [SerializeField]
    private StorePageController _storePage;
    private List<RectMask2D> rectMasks = new List<RectMask2D>();
    private List<ScrollRect> scrollRects = new List<ScrollRect>();
    [AssertNotNull]
    public uGUI_CanvasScaler canvasScaler;

    [AssertNotNull]
    public CanvasGroup canvasGroup;
    public bool IsOpen { get; set; }
    public FCSAlterraHubGUISender SenderType { get; set; }

    public override void Awake()
    {
        base.Awake();
        base.GetComponentsInChildren<RectMask2D>(true, this.rectMasks);
        base.GetComponentsInChildren<ScrollRect>(true, this.scrollRects);
        this.SetCanvasVisible(false);
    }

    public void Initialize()
    {
        _menuController = gameObject.GetComponent<PDAMenuController>();
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

        _currentPage = page;

        if (page == PDAPages.None)
        {
           currentPage =  _menuController.PopPage();
        }
        else
        {
            if(page == PDAPages.DevicePage)
            {
                var data = arg as Tuple<TechType, MonoBehaviour>;

                QuickLogger.Debug($"{data.Item1} || {data.Item2}");

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

    internal void OpenEncyclopedia(FCSDevice device)
    {
        PurgePages();
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
        _messageBox.ShowMessage(message, FCSMessageButton.OK);
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

    internal void PrepareDevicePage(TechType id, MonoBehaviour fcsDevice)
    {
        GoToPage(PDAPages.DevicePage,new Tuple<TechType,MonoBehaviour>(id, fcsDevice));
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
        this.canvasGroup.SetVisible(visible);
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
        //if (!this.introActive)
        //{
        //    RuntimeManager.PlayOneShot(this.soundOpen.path, default(Vector3));
        //}
        //bool flag = tabId == PDATab.None;
        //if (flag)
        //{
        //    uGUI_PopupNotification main = uGUI_PopupNotification.main;
        //    if (main.isShowingMessage && !string.IsNullOrEmpty(main.id))
        //    {
        //        string id = main.id;
        //        if (id == "PDAEncyclopediaTab")
        //        {
        //            tabId = PDATab.Encyclopedia;
        //        }
        //    }
        //    if (tabId == PDATab.None && this.tabOpen == PDATab.None)
        //    {
        //        tabId = this.tabPrev;
        //    }
        //}
        //if (tabId == PDATab.TimeCapsule)
        //{
        //    this.SetTabs(null);
        //    Inventory.main.SetUsedStorage(PlayerTimeCapsule.main.container, false);
        //    uGUI_GalleryTab uGUI_GalleryTab = this.GetTab(PDATab.Gallery) as uGUI_GalleryTab;
        //    uGUI_TimeCapsuleTab @object = this.GetTab(PDATab.TimeCapsule) as uGUI_TimeCapsuleTab;
        //    uGUI_GalleryTab.SetSelectListener(new uGUI_GalleryTab.ImageSelectListener(@object.SelectImage), "ScreenshotSelect", "ScreenshotSelectTooltip");
        //}
        //foreach (KeyValuePair<PDATab, uGUI_PDATab> keyValuePair in this.tabs)
        //{
        //    keyValuePair.Value.OnOpenPDA(tabId, flag);
        //}
        //this.OpenTab(tabId);
        //ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnUpdate));
        //ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnLateUpdate));
    }
    public void OnClosePDA()
    {
        //ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnUpdate));
        //ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdateAfterInput, new ManagedUpdate.OnUpdate(this.OnLateUpdate));
        //RuntimeManager.PlayOneShot(this.soundClose.path, default(Vector3));
        //if (this.tabOpen != PDATab.None)
        //{
        //    this.tabs[this.tabOpen].Close();
        //    this.tabOpen = PDATab.None;
        //}
        //foreach (KeyValuePair<PDATab, uGUI_PDATab> keyValuePair in this.tabs)
        //{
        //    keyValuePair.Value.OnClosePDA();
        //}
        base.Deselect();
        //this.SetTabs(uGUI_PDA.regularTabs);
        this.content.SetVisible(false);
    }

    internal Page GetPreviousPage()
    {
        return _menuController.GetPreviousPage();
    }

    public bool OnButtonDown(GameInput.Button button)
    {
        Page page = _menuController.GetCurrentPage();
        return page != null && page.OnButtonDown(button);
    }
}