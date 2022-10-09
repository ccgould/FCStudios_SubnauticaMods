using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Model;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Managers;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using PDAPages = FCS_AlterraHub.Mods.FCSPDA.Enums.PDAPages;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mods.FCSPDA.Mono
{
    public class FCSPDAController : MonoBehaviour
    {
        private PDA _pda;
        private int prevQuickSlot;
        private Sequence sequence = new(false);
        private GameObject _inputDummy;
        private uGUI_InputGroup _ui;
        private Text _clock;
        private bool _isBeingDestroyed;
        private Text _currentBiome;
        private Text _accountName;
        private Text _accountBalance;
        private bool _goToEncyclopedia;
        private bool _depthState;
        private readonly Dictionary<PDAPages, GameObject> _pages = new();
        private GameObject _toggleHud;
        private Dictionary<StoreCategory, List<StoreItem>> _storeItems = new();
        private GameObject _storePageGrid;
        private Text _storeLabel;
        private CartDropDownHandler _cartDropDownManager;
        private Text _cartButtonNumber;
        private Text _cartAmountLabel;
        private Text _cartTotalLabel;
        private CheckOutPopupDialogWindow _checkoutDialog;
        private AccountPageHandler _accountPageHandler;
        private bool _isInitialized;
        private Canvas _canvas;
        private bool _cartLoaded;

        public GameObject PDAObj { get; set; }
        public float cameraFieldOfView = 62f;
        public float cameraFieldOfViewAtFourThree = 66f;
        public Canvas PdaCanvas { get; set; }
        internal bool IsOpen { get; private set; }
        public Action OnClose { get; set; }
        public Channel AudioTrack { get; set; }
        public bool isFocused => this.ui != null && this.ui.focused;
        public uGUI_InputGroup ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = _canvas.gameObject.GetComponentInChildren<Canvas>(true).gameObject.AddComponent<uGUI_InputGroup>();
                }
                return _ui;
            }
        }
        
        #region SINGLETON PATTERN
        private ReturnsDialogController _returnsDialogController;
        private List<MeshRenderer> _pdaMeshes = new();
        private GameObject _screen;
        private Transform _pdaAnchor;
        private uGUI_CanvasScaler _canvasScalar;
        internal EncyclopediaTabController EncyclopediaTabController { get; set; }
        public static FCSPDAController Main;
        private GameObject _404;
        private ShipmentPageController _shipmentPageController;
        private int _timesOpen;
        private Text _currentBaseInfo;
        private TeleportationPageController _teleportationPageController;
        private bool _firstTimeOpen = true;

        #endregion

        private void Awake()
        {
            if (Main == null)
            {
                Main = this;
                DontDestroyOnLoad(this);
            }
            else if (Main != null)
            {
                Destroy(gameObject);
                return;
            }
        }


        public bool Open()
        {
            QuickLogger.Debug("PDA Open : 1");

            Player main = Player.main;

            main.GetPDA().sequence.ForceState(true);

            PlayAppropriateVoiceMessage();

            TryRemove404Screen();

            RefreshTeleportationPage();

            CreateScreen();

            FindPDA();

            ChangePDAVisibility(false);

            AttemptToOpenReturnsDialog();

            UpdateDisplay();

            DOFOperations();

            SetPDAInUse(true);

            if (!DetemineIfInCinematicMode(main)) return false;

            SetRequiredParametersToOpenPDA();

            QuickLogger.Debug("FCS PDA Is Open", true);
            return true;
        }



        public void Close()
        {
            IsOpen = false;

            ChangePDAVisibility(true);
            gameObject.SetActive(false);
            SetPDAInUse(false);
            Player main = Player.main;
            main.GetPDA().sequence.ForceState(false);
            MainCameraControl.main.ResetLockedVRViewModelAngle();
            _screen.SetActive(false);
            Vehicle vehicle = main.GetVehicle();
            if (vehicle != null)
            {
                uGUI.main.quickSlots.SetTarget(vehicle);
            }

            _accountPageHandler.Close();


#if SUBNAUTICA_STABLE
            MainGameController.Instance.PerformGarbageAndAssetCollection();
#else
            MainGameController.Instance.PerformIncrementalGarbageCollection();
#endif
            HandReticle.main?.UnrequestCrosshairHide();
            Inventory.main.SetViewModelVis(true);
            
            
            //sequence.Set(0.5f, false, Deactivated);

            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", false);
            ui.Deselect(null);
            UwePostProcessingManager.ClosePDA();
#if SUBNAUTICA
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundClose);
#else
#endif
            UwePostProcessingManager.ToggleDof(_depthState);
            QuickLogger.Debug("FCS PDA Is Closed", true);
        }


        internal void SetInstance()
        {
            if (_isInitialized) return;
            
            CreateScreen();

            _pdaAnchor = GameObjectHelpers.FindGameObject(gameObject, "ScreenAnchor").transform;

            _canvasScalar = _screen.AddComponent<uGUI_CanvasScaler>();
            var raycaster = _screen.AddComponent<uGUI_GraphicRaycaster>();
            raycaster.guiCameraSpace = true;
            raycaster.ignoreReversedGraphics = false;

            _canvasScalar.mode = uGUI_CanvasScaler.Mode.Inversed;
            _canvasScalar.vrMode = uGUI_CanvasScaler.Mode.Inversed;
            _canvasScalar.SetAnchor(_pdaAnchor.transform);


            _canvas = _screen.GetComponent<Canvas>();
            _canvas.sortingLayerName = "PDA";
            _canvas.sortingLayerID = 1479780821;

            _currentBiome = GameObjectHelpers.FindGameObject(_canvas.gameObject, "BiomeLBL")?.GetComponent<Text>();
            _404 = GameObjectHelpers.FindGameObject(_canvas.gameObject, "404");
            _404.FindChild("Message").GetComponent<Text>().text = AlterraHub.Error404();
            _checkoutDialog = _canvas.gameObject.FindChild("Dialogs").FindChild("CheckOutPopUp").AddComponent<CheckOutPopupDialogWindow>();

            _returnsDialogController = _canvas.gameObject.FindChild("Dialogs").FindChild("ReturnItemsDialog").AddComponent<ReturnsDialogController>();
            _returnsDialogController.Initialize(this);
            
            _cartDropDownManager = _canvas.gameObject.FindChild("Dialogs").FindChild("CartDropDown").AddComponent<CartDropDownHandler>();
            _cartDropDownManager.OnBuyAllBtnClick += OnBuyAllBtnClick;
            _cartDropDownManager.Initialize();
            _cartDropDownManager.onTotalChanged += amount =>
            {
                _cartAmountLabel.text = $"Cart Amount: {amount:n0}";
                _cartTotalLabel.text = $"Cart Total: {_cartDropDownManager.GetCartCount()}";
                _cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
            };

            _toggleHud = GameObjectHelpers.FindGameObject(_canvas.gameObject, "ToggleHud");
            _accountName = GameObjectHelpers.FindGameObject(_canvas.gameObject, "UserName")?.GetComponent<Text>();
            _currentBaseInfo = GameObjectHelpers.FindGameObject(_canvas.gameObject, "CurrentBaseInfo")?.GetComponent<Text>();
            _accountBalance = GameObjectHelpers.FindGameObject(_canvas.gameObject, "AccountBalance")?.GetComponent<Text>();
            _clock = GameObjectHelpers.FindGameObject(_canvas.gameObject, "Clock")?.GetComponent<Text>();

            AddPages();
            CreateHomePage();
            CreateStorePage();
            EncyclopediaPage();
            CreateStorePagePage();
            AccountPage();
            LoadStore();
            LoadShipmentPage();
            TeleportationPage();

            MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol,gameObject,Color.white,0,0.01f,0.01f);
            MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol,gameObject,1, 6.15f);
            MessageBoxHandler.main.ObjectRoot = ui.gameObject;
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject,Color.cyan);
            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
            InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);
            _screen.SetActive(false);
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            _accountPageHandler = null;
            _cartDropDownManager.OnBuyAllBtnClick -= OnBuyAllBtnClick;
            _isBeingDestroyed = true;
        }

        private void OnBuyAllBtnClick(CartDropDownHandler obj)
        {
            _checkoutDialog.ShowDialog(this, _cartDropDownManager);
            _cartDropDownManager.ToggleVisibility();
        }

        private void Update()
        {
            sequence.Update();

            if (sequence.active)
            {
                float b = (SNCameraRoot.main.mainCamera.aspect > 1.5f) ? cameraFieldOfView : cameraFieldOfViewAtFourThree;
                SNCameraRoot.main.SetFov(Mathf.Lerp(MiscSettings.fieldOfView, b, sequence.t));
            }

            if (!ui.selected && IsOpen && AvatarInputHandler.main.IsEnabled())
            {
                ui.Select(false);
            }

            if (IsOpen && this.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) || Input.GetKeyDown(QPatch.Configuration.FCSPDAKeyCode)))
            {
                this.Close();
                return;
            }

            if (_clock != null)
            {
                _clock.text = WorldHelpers.GetGameTimeFormat();
            }

            FPSInputModule.current.EscapeMenu();
        }

        private void CreateScreen()
        {
            if (_screen == null)
            {
                _screen = Instantiate(AlterraHub.PDAScreenPrefab);
            }
        }

        private void AddPages()
        {
            foreach (PDAPages page in Enum.GetValues(typeof(PDAPages)))
            {
                var gPage = GameObjectHelpers.FindGameObject(_canvas.gameObject, page.ToString());
                _pages.Add(page,gPage);
            }
        }
        
        private void CreateHomePage()
        {
            var pageTextLabel = _pages[PDAPages.Home].FindChild("PageName").GetComponent<Text>();
            var radialMenu = _pages[PDAPages.Home].FindChild("RadialMenu").AddComponent<RadialMenu>();
            radialMenu.TabAmount = 4;
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "Cart_Icon.png")),pageTextLabel,"Store",PDAPages.Store);
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "EncyclopediaIcon.png")), pageTextLabel,"Encyclopedia",PDAPages.Encyclopedia);
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "IconAccount.png")), pageTextLabel,"Account",PDAPages.AccountPage);
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "QuantumTeleporterIcon_W.png")), pageTextLabel,"Teleportation",PDAPages.Teleportation);
            radialMenu.Rearrange();
        }

        private void CreateStorePage()
        {
            _cartButtonNumber = GameObjectHelpers.FindGameObject(_pages[PDAPages.Store], "CartCount").GetComponentInChildren<Text>();
            var pageTextLabel = _pages[PDAPages.Store].FindChild("PageName").GetComponent<Text>();
            var radialMenu = _pages[PDAPages.Store].FindChild("RadialMenu").AddComponent<RadialMenu>();
            
            var cartBTN = _pages[PDAPages.Store].FindChild("Cart").GetComponent<Button>();
            cartBTN.onClick.AddListener((() =>
            {
                _cartDropDownManager.ToggleVisibility();
            }));

            var returnsBTN = _pages[PDAPages.Store].FindChild("Returns").GetComponent<Button>();
            returnsBTN.onClick.AddListener(() =>
            {
                _returnsDialogController.Open();
            });

            var backButton = _pages[PDAPages.Store].FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Home);
            }));


            radialMenu.TabAmount = 7;
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "HomeSolutionsIcon_W.png")), pageTextLabel, "Home Solutions", PDAPages.HomeSolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "LifeSupportIcon_W.png")), pageTextLabel, "Life Solutions", PDAPages.LifeSolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "EnergySolutionsIcon_W.png")), pageTextLabel, "Energy Solutions", PDAPages.EnergySolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "ProductionSolutionsIcon_W.png")), pageTextLabel, "Production Solutions", PDAPages.ProductionSolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "StoreSolutionsIcon_W.png")), pageTextLabel, "Storage Solutions", PDAPages.StorageSolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "VehicleSolutionsIcon_W.png")), pageTextLabel, "Vehicle Solutions", PDAPages.VehicleSolutions);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "MiscIcon_W.png")), pageTextLabel, "Misc", PDAPages.MiscSolutions);

            //Set gameobject to toggle for pages
            _pages[PDAPages.HomeSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.LifeSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.EnergySolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.ProductionSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.StorageSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.VehicleSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.MiscSolutions] = _pages[PDAPages.StorePage];

            radialMenu.Rearrange();
        }

        private void CreateStorePagePage()
        {
            var backButton = _pages[PDAPages.StorePage].FindChild("BackBTN").GetComponent<Button>();
            _cartAmountLabel = _pages[PDAPages.StorePage].FindChild("StoreHud").FindChild("CartAmount").GetComponent<Text>();
            _cartTotalLabel = _pages[PDAPages.StorePage].FindChild("StoreHud").FindChild("CartTotal").GetComponent<Text>();
            _storePageGrid = GameObjectHelpers.FindGameObject(_pages[PDAPages.StorePage], "Content");
            _storeLabel = GameObjectHelpers.FindGameObject(_pages[PDAPages.StorePage], "StoreLabel").GetComponent<Text>();
            backButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Store);
            }));
        }

        private void LoadShipmentPage()
        {

            var shipmentButton = _pages[PDAPages.Store].FindChild("ShipmentBTN").GetComponent<Button>();
            shipmentButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Shipment);
            }));

            _shipmentPageController = _pages[PDAPages.Shipment].AddComponent<ShipmentPageController>();
            _shipmentPageController.Initialize(this);
        }

        private void AccountPage()
        {
            _accountPageHandler = new AccountPageHandler(this);
            var backButton = _pages[PDAPages.AccountPage].FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Home);
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
                GoToPage(PDAPages.Home);
            }));
            EncyclopediaTabController.Initialize();
        }

        private void UpdateDisplay()
        {
            if (_currentBiome == null || _accountName == null || _currentBaseInfo == null) return;

            _currentBiome.text = Player.main.GetBiomeString();

            if(!string.IsNullOrWhiteSpace(CardSystem.main.GetUserName()))
                _accountName.text = CardSystem.main.GetUserName();

            _accountBalance.text = $"{CardSystem.main.GetAccountBalance():N0}";

            var friendly = BaseManager.GetPlayersCurrentBase()?.GetBaseName();
            var baseId = BaseManager.GetPlayersCurrentBase()?.GetBaseFriendlyId();

            if (!string.IsNullOrWhiteSpace(friendly))
            {
                SetCurrentBaseInfoText($"{friendly} | {AlterraHub.BaseIDFormat(baseId)}");
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
        
        private void SetRequiredParametersToOpenPDA()
        {
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            IsOpen = true;
            gameObject.SetActive(true);
            sequence.Set(0.5f, true, Activated);
            UWE.Utils.lockCursor = false;
            HandReticle.main?.RequestCrosshairHide();

            if (HandReticle.main?.hideCount > 1)
            {
                QuickLogger.Debug("Fixing Hide Count", true);
                while (HandReticle.main.hideCount > 1)
                {
                    HandReticle.main?.UnrequestCrosshairHide();
                }
            }

            Inventory.main.SetViewModelVis(false);
            _screen.SetActive(true);
            UwePostProcessingManager.OpenPDA();
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", true);

#if SUBNAUTICA
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);
            if (_pda.screen.activeSelf)
            {
                _pda.screen.SetActive(false);
            }
#else
#endif
        }

        private bool DetemineIfInCinematicMode(Player main)
        {
            var flag = InventorySlotHandler();

            if (!flag || main.cinematicModeActive)
            {
                return false;
            }

            return true;
        }

        private void SetPDAInUse(bool isInUse)
        {
            _pda.isInUse = isInUse;
        }

        private void AttemptToOpenReturnsDialog()
        {
            if (_returnsDialogController?.IsOpen ?? false)
            {
                _returnsDialogController.Open();
            }
        }

        private void RefreshTeleportationPage()
        {
            _teleportationPageController.Refresh();
        }

        private void TryRemove404Screen()
        {
            if (DroneDeliveryService.Main == null)
            {
                MessageBoxHandler.main.Show(AlterraHub.ErrorHasOccurred("0x0001"), FCSMessageButton.OK);
                return;
            }
            
            _404?.SetActive(!DroneDeliveryService.Main.DetermineIfFixed());
        }

        private void PlayAppropriateVoiceMessage()
        {
            if (_timesOpen > 0 && !CardSystem.main.HasBeenRegistered() && !_firstTimeOpen &&
                DroneDeliveryService.Main.DetermineIfFixed())
            {
                VoiceNotificationSystem.main.Play("PDA_Account_Instructions_key", 26);
            }

            if (_firstTimeOpen && DroneDeliveryService.Main.DetermineIfFixed())
            {
                VoiceNotificationSystem.main.Play("PDA_Instructions_key", 26);
                _firstTimeOpen = false;
                _timesOpen++;
                Mod.SaveGamePlaySettings();
            }
        }

        private bool InventorySlotHandler()
        {
            uGUI.main.quickSlots.SetTarget(null);
            prevQuickSlot = Inventory.main.quickSlots.activeSlot;
            bool flag = Inventory.main.ReturnHeld();
            return flag;
        }

        private void DOFOperations()
        {
            _depthState = UwePostProcessingManager.GetDofEnabled();

            UwePostProcessingManager.ToggleDof(false);
        }

        private void ChangePDAVisibility(bool value)
        {
            _pda.gameObject.SetActive(!value);
            foreach (var meshRenderer in _pdaMeshes)
            {
                meshRenderer.enabled = value;
            }
        }

        public void Activated()
        {
            ui.Select();
        }

        public void Deactivated()
        {
            SNCameraRoot.main.SetFov(0f);
            OnClose?.Invoke();
            gameObject.SetActive(false);

            if (!_goToEncyclopedia)
            {
                Inventory.main.quickSlots.Select(prevQuickSlot);
            }
            else
            {
                Player.main.GetPDA().Open(PDATab.Encyclopedia);
                _goToEncyclopedia = false;
            }
        }
        
        private void FindPDA()
        {
            QuickLogger.Debug("In Find PDA",true);
            if (PdaCanvas == null)
            {
#if SUBNAUTICA
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
#else
#endif
                Player main = Player.main;
                _pda = main.GetPDA();
            }

            foreach (MeshRenderer meshRenderer in _pda.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                _pdaMeshes.Add(meshRenderer);
            }
        }

        internal void ExitStore()
        {
            GoToPage(PDAPages.Home);
            Close();
        }

        internal void ShowMission()
        {
           // uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowMessage("Hi","Eggo"); 
        }

        internal void ShowMessage(string message)
        {
            MessageBoxHandler.main.Show(message, FCSMessageButton.OK);
        }
        
        public void GoToPage(PDAPages page)
        {
            foreach (KeyValuePair<PDAPages, GameObject> cachedPage in _pages)
            {
                cachedPage.Value?.SetActive(false);
            }

            _pages[page].SetActive(true);
            
            switch (page)
            {
                case PDAPages.Store:
                    _toggleHud.gameObject.SetActive(true);
                    break;
                case PDAPages.Encyclopedia:
                    EncyclopediaTabController.Refresh();
                    _toggleHud.gameObject.SetActive(true);
                    break;
                case PDAPages.Home:
                case PDAPages.StorePage:
                case PDAPages.AccountPage:
                    _toggleHud.gameObject.SetActive(true);
                    _accountPageHandler.UpdateRequestBTN(CardSystem.main.HasBeenRegistered());
                    break;
                case PDAPages.Shipment:
                    _shipmentPageController.gameObject.SetActive(true);
                    break;
                case PDAPages.Teleportation:
                    _teleportationPageController.Refresh();
                    break;
                default:
                    LoadStorePage(page);
                    _toggleHud.gameObject.SetActive(false);
                    break;
            }
        }

        private void LoadStorePage(PDAPages pages)
        {
            StoreCategory category = StoreCategory.None;

            switch (pages)
            {
                case PDAPages.HomeSolutions:
                    category = StoreCategory.Home;
                    _storeLabel.text = "Home Solutions";
                    break;
                case PDAPages.LifeSolutions:
                    category = StoreCategory.LifeSupport;
                    _storeLabel.text = "Life Solutions";
                    break;
                case PDAPages.EnergySolutions:
                    category = StoreCategory.Energy;
                    _storeLabel.text = "Energy Solutions";
                    break;
                case PDAPages.ProductionSolutions:
                    category = StoreCategory.Production;
                    _storeLabel.text = "Production Solutions";
                    break;
                case PDAPages.StorageSolutions:
                    category = StoreCategory.Storage;
                    _storeLabel.text = "Storage Solutions";
                    break;
                case PDAPages.VehicleSolutions:
                    category = StoreCategory.Vehicles;
                    _storeLabel.text = "Vehicle Solutions";
                    break;
                case PDAPages.MiscSolutions:
                    category = StoreCategory.Misc;
                    _storeLabel.text = "Misc";
                    break;
            }

            foreach (var storeItem in _storeItems)
            {
                if (storeItem.Key == category)
                {
                    foreach (StoreItem item in storeItem.Value)
                    {
                        item.Show();
                    }
                }
                else
                {
                    foreach (StoreItem item in storeItem.Value)
                    {
                        item.Hide();
                    }
                }
            }
        }

        private void LoadStore()
        {

            foreach (StoreCategory category in Enum.GetValues(typeof(StoreCategory)))
            {
                foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
                {
                    if(storeItem.Value.StoreCategory != category) continue;
                    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.Key)}");
                    
                    var item = StoreInventorySystem.CreateStoreItem(storeItem.Value, AddToCardCallBack, IsInUse);
                    
                    if (_storeItems.ContainsKey(category))
                    {
                        _storeItems[category].Add(item);
                    }
                    else
                    {
                        _storeItems.Add(category, new List<StoreItem>{item});
                    }

                    item.gameObject.transform.SetParent(_storePageGrid.transform, false);

                    QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.Key)} with category to Panel: {storeItem.Value.StoreCategory}:");
                }

                foreach (FCSStoreEntry storeItem in QPatch.Configuration.AdditionalStoreItems)
                {
                    if (storeItem.StoreCategory != category) continue;

                    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.TechType)}");

                    var item = StoreInventorySystem.CreateStoreItem(storeItem, AddToCardCallBack, IsInUse);
                    if (_storeItems.ContainsKey(category))
                    {
                        _storeItems[category].Add(item);
                    }
                    else
                    {
                        _storeItems.Add(category, new List<StoreItem>{item});
                    }

                    item.gameObject.transform.SetParent(_storePageGrid.transform,false);

                    QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.TechType)} with category to Panel: {storeItem.StoreCategory}:");
                }
            }
        }

        private void AddToCardCallBack(TechType techType, TechType receiveTechType, int returnAmount)
        {
            _cartDropDownManager?.AddItem(techType, receiveTechType, returnAmount);
        }

        private bool IsInUse()
        {
            return IsOpen;
        }

        private void OnQuit()
        {
            Mod.DeepCopySave(CardSystem.main.SaveDetails());
            QuickLogger.Debug("Quitting Purging CardSystem and AlterraHubSave", true);
            CardSystem.main.Purge();
            Mod.PurgeSave();
        }

        internal void Save(SaveData newSaveData)
        {
            newSaveData.FCSPDAEntry = new FCSPDAEntry
            {
                //CartItems = _cartDropDownManager?.Save() ?? new List<CartItemSaveData>()
            };

            Mod.GamePlaySettings.Rate = GetRate();
            Mod.GamePlaySettings.AutomaticDebitDeduction = GetAutomaticDebitDeduction();
            Mod.GamePlaySettings.IsPDAOpenFirstTime = _firstTimeOpen;
        }


        public bool GetAutomaticDebitDeduction()
        {
            return _accountPageHandler.GetAutomaticDebitDeduction();
        }

        internal void LoadFromSave(FCSPDAEntry savedData)
        {
            _firstTimeOpen = Mod.GamePlaySettings.IsPDAOpenFirstTime;

            if (savedData?.CartItems == null)
            {
                QuickLogger.Debug("Cart Items returned Null");
            }
            else
            {
                foreach (CartItemSaveData cartItem in savedData.CartItems)
                {
                    _cartDropDownManager.AddItem(cartItem.TechType, cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
                }
            }

            _accountPageHandler.Refresh();

            _cartLoaded = true;
        }

        public void OpenEncyclopedia(TechType techType)
        {
            if (CheckIfPDAHasEntry(techType))
            {
                if (!Open()) return ;
                GoToPage(PDAPages.Encyclopedia);
                EncyclopediaTabController.OpenEntry(techType);
            }
            else
            {
                QuickLogger.ModMessage($"AlterraHub PDA doesn't have any entry for {Language.main.Get(techType)}");
            }
        }

        public void OpenEncyclopedia(string techType)
        {
            if (CheckIfPDAHasEntry(techType))
            {
                if (!Open()) return;
                GoToPage(PDAPages.Encyclopedia);
                EncyclopediaTabController.OpenEntry(techType);
            }
            else
            {
                QuickLogger.ModMessage($"AlterraHub PDA doesn't have any entry for {Language.main.Get(techType)}");
            }
        }


        public bool CheckIfPDAHasEntry(TechType techType)
        {
            return EncyclopediaTabController.HasEntry(techType);
        }

        public bool CheckIfPDAHasEntry(string techType)
        {
            return EncyclopediaTabController.HasEntry(techType);
        }

        public CraftNode GetCraftTree()
        {
            return EncyclopediaTabController.Tree;
        }

        public static void ForceOpen()
        {
            Player_Patches.ForceOpenPDA = true;
        }

        public static void ForceClose()
        {
            Main.Close();
        }

        internal void AddShipment(Shipment shipment)
        {
            _shipmentPageController.AddItem(shipment);
        }

        internal void RemoveShipment(Shipment shipment)
        {
            _shipmentPageController.RemoveItem(shipment);
        }

        public float GetRate()
        {
            return _accountPageHandler.GetRate();
        }
    }

    internal class ShipmentPageController : MonoBehaviour
    {
        private GameObject _grid;
        private List<ShipmentTracker> _trackedShipments = new();

        public void Initialize(FCSPDAController controller)
        {
            var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                controller.GoToPage(PDAPages.Store);
            }));

            _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
        }

        internal void AddItem(Shipment pendingOrder)
        {
            //Instantiate Item
            var item = GameObject.Instantiate(AlterraHub.PDAShipmentItemPrefab);
            
            //Move in scrollview
            item.transform.SetParent(_grid.transform,false);

            //Add controller
            var shipmentTracker = item.AddComponent<ShipmentTracker>();
            shipmentTracker.Initialize(this,pendingOrder);

            _trackedShipments.Add(shipmentTracker);
        }

        public void RemoveItem(Shipment shipment)
        {
            foreach (ShipmentTracker shipmentTracker in _trackedShipments)
            {
                if (shipmentTracker.TryDelete(shipment))
                {
                    break;
                }
            }   
        }
    }

    internal class TeleportationPageController : MonoBehaviour
    {
        private GameObject _grid;
        private List<ShipmentTracker> _trackedShipments = new();

        public void Initialize(FCSPDAController controller)
        {
            var backButton = gameObject.FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                controller.GoToPage(PDAPages.Home);
            }));

            _grid = GameObjectHelpers.FindGameObject(gameObject, "Content");
        }

        internal void Refresh()
        {
            QuickLogger.Debug("Refreshing Teleportation list",true);
            foreach (Transform child in _grid.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (BaseManager baseManager in BaseManager.Managers)
            {
                if (baseManager.IsValidForTeleport())
                {
                    //Instantiate Item
                    var item = GameObject.Instantiate(AlterraHub.PDATeleportItemPrefab);
                    var baseItem = item.AddComponent<BaseTeleportItem>();
                    baseItem.Initialize(baseManager);
                    //Move in scrollview
                    item.transform.SetParent(_grid.transform, false);
                }
            }
        }
    }

    internal class BaseTeleportItem : MonoBehaviour
    {
        private Text _baseName;
        private BaseManager _baseManager;

        private void Awake()
        {
            _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName")?.GetComponent<Text>();
            var button = GameObjectHelpers.FindGameObject(gameObject, "TeleportBtn")?.GetComponent<Button>();
            button?.onClick.AddListener((() =>
            {
                QuickLogger.Debug($"Trying to teleport to base: {_baseManager.GetBaseName()}",true);
                FCSAlterraHubService.PublicAPI.TeleportationIgnitiated?.Invoke(_baseManager);
            }));
        }

        internal void Initialize(BaseManager manager)
        {
            _baseName.text = manager.GetBaseName();
            _baseManager = manager;
        }
    }


    internal class ShipmentTracker : MonoBehaviour
    {
        private Text _orderName;
        private Slider _slider;
        private Shipment _shipment;
        private GameObject _itemsGrid;
        private Button _cancelButton;
        private Shipment _pendingOrder;

        public void Initialize(ShipmentPageController shipmentPageController, Shipment pendingOrder)
        {
            _orderName = gameObject.FindChild("OrderNumber").GetComponent<Text>();
            _itemsGrid = gameObject.FindChild("Items");
            _cancelButton = GetComponentInChildren<Button>();
            _pendingOrder = pendingOrder;

            _cancelButton.onClick.AddListener((() =>
            {
                StoreManager.main.CancelOrder(pendingOrder);
                Delete();
            }));

            foreach (CartItemSaveData cartItem in pendingOrder.CartItems)
            {
                var item = GameObject.Instantiate(AlterraHub.PDAShipmentItemNodePrefab);
                item.FindChild("Icon").AddComponent<uGUI_Icon>().sprite = SpriteManager.Get(cartItem.ReceiveTechType);
                item.transform.SetParent(_itemsGrid.transform,false);
            }
            _slider = gameObject.GetComponentInChildren<Slider>();
            _shipment = pendingOrder;
            InvokeRepeating(nameof(UpdateCheck),1f,1f);
        }

        private void Delete()
        {
            Destroy(gameObject);
        }

        private void UpdateCheck()
        {
            if (DroneDeliveryService.Main == null ||
                string.IsNullOrWhiteSpace(_pendingOrder.OrderNumber) ||
                _pendingOrder.Port?.GetBaseName() == null) return;

            var isCurrentOrder = DroneDeliveryService.Main.IsCurrentOrder(_shipment.OrderNumber);
            var status = isCurrentOrder ? "Shipping" : "Pending";
            _orderName.text = $"Order: {_pendingOrder.OrderNumber}: Destination: {_pendingOrder.Port.GetBaseName()} Status: {status}";
            _cancelButton.interactable = !isCurrentOrder;
            _slider.value = DroneDeliveryService.Main.GetOrderCompletionPercentage(_shipment.OrderNumber);
        }

        public bool TryDelete(Shipment shipment)
        {
            if (shipment.OrderNumber.Equals(_shipment.OrderNumber))
            {
                Destroy(gameObject);
                return true;
            }

            return false;
        }
    }
}