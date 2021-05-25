using System;
using System.Collections.Generic;
using System.IO;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono.AlterraHub;
using FCS_AlterraHub.Mono.AlterraHubDepot.Mono;
using FCS_AlterraHub.Mono.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Mono.FCSPDA.Mono.Model;
using FCS_AlterraHub.Mono.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCS_AlterraHub.Systems;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using SMLHelper.V2.Utility;
using UnityEngine;
using UnityEngine.UI;
using PDAPages = FCS_AlterraHub.Mono.FCSPDA.Enums.PDAPages;
using WorldHelpers = FCS_AlterraHub.Helpers.WorldHelpers;

namespace FCS_AlterraHub.Mono.FCSPDA.Mono
{
    public class FCSPDAController : MonoBehaviour
    {
        private PDA _pda;
        private int prevQuickSlot;
        private Sequence sequence = new Sequence(false);
        private BaseManager _currentBase;
        private GameObject _inputDummy;
        private uGUI_InputGroup _ui;
        private Text _clock;
        private bool _isBeingDestroyed;
        private Text _currentBiome;
        private Text _accountName;
        private Text _accountBalance;
        private bool _goToEncyclopedia;
        private bool _depthState;
        private readonly Dictionary<PDAPages, GameObject> _pages = new Dictionary<PDAPages, GameObject>();
        private GameObject _toggleHud;
        private Dictionary<StoreCategory, List<StoreItem>> _storeItems = new Dictionary<StoreCategory, List<StoreItem>>();
        private GameObject _storePageGrid;
        private Text _storeLabel;
        private CartDropDownHandler _cartDropDownManager;
        private Text _cartButtonNumber;
        private Text _cartAmountLabel;
        private Text _cartTotalLabel;
        private CheckOutPopupDialogWindow _checkoutDialog;
        private AccountPageHandler _accountPageHandler;
        private FCSPDAEntry _savedData;
        private bool _isInitialized;
        private Canvas _canvas;

        private Dictionary<string, List<EncyclopediaEntryData>> encyclopediaEntryDatas =>
            QPatch.EncyclopediaConfig.EncyclopediaEntries;
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
                    _ui = gameObject.GetComponentInChildren<Canvas>(true).gameObject.AddComponent<uGUI_InputGroup>();
                }
                return _ui;
            }
        }
        
        #region SINGLETON PATTERN
        private static FCSPDAController _instance;
        private ReturnsDialogController _returnsDialogController;
        private List<MeshRenderer> _pdaMeshes = new List<MeshRenderer>();
        public static FCSPDAController Instance => _instance;
        #endregion

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
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

        private void Start()
        {
            if (_isInitialized) return;
             _canvas = gameObject.GetComponentInChildren<Canvas>();
            _currentBiome = GameObjectHelpers.FindGameObject(gameObject, "BiomeLBL")?.GetComponent<Text>();
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
            _accountName = GameObjectHelpers.FindGameObject(gameObject, "UserName")?.GetComponent<Text>();
            _accountBalance = GameObjectHelpers.FindGameObject(gameObject, "AccountBalance")?.GetComponent<Text>();
            _clock = GameObjectHelpers.FindGameObject(gameObject, "Clock")?.GetComponent<Text>();
            
            AddPages();
            CreateHomePage();
            CreateStorePage();
            EncyclopediaPage();
            CreateStorePagePage();
            AccountPage();
            LoadStore();

            MessageBoxHandler.main.ObjectRoot = gameObject;
            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject,Color.cyan);
            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
            InGameMenuQuitPatcher.AddEventHandlerIfMissing(OnQuit);
            _isInitialized = true;
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
            radialMenu.TabAmount = 3;
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "Cart_Icon.png")),pageTextLabel,"Store",PDAPages.Store);
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "EncyclopediaIcon.png")), pageTextLabel,"Encyclopedia",PDAPages.Encyclopedia);
            radialMenu.AddEntry(this,ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(),"Icons", "IconAccount.png")), pageTextLabel,"Account",PDAPages.AccountPage);
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

        private void AccountPage()
        {
            _accountPageHandler = new AccountPageHandler(this);
            var backButton = _pages[PDAPages.AccountPage].FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Home);
            }));
        }

        private void EncyclopediaPage()
        {
            _pages[PDAPages.Encyclopedia].AddComponent<EncyclopediaTabController>();
            var backButton = _pages[PDAPages.Encyclopedia].FindChild("BackBTN").GetComponent<Button>();
            backButton.onClick.AddListener((() =>
            {
                GoToPage(PDAPages.Home);
            }));
        }

        private void UpdateDisplay()
        {
            if (_currentBiome == null || _accountName == null) return;

            _currentBiome.text = Player.main.GetBiomeString();
            _accountName.text = CardSystem.main.GetUserName();
            _accountBalance.text = $"{CardSystem.main.GetAccountBalance():N0}";
        }
        
        internal void OnEnable()
        {
            QuickLogger.Debug($"FCS PDA: Active and Enabled {isActiveAndEnabled}",true);
        }

        public void Open()
        {
            FindPDA();

            ChangePDAVisibility(false);

            if (_returnsDialogController?.IsOpen ?? false)
            {
                _returnsDialogController.Open();
            }

            _depthState = UwePostProcessingManager.GetDofEnabled();

            UwePostProcessingManager.ToggleDof(false);

            _pda.isInUse = true;
            uGUI.main.quickSlots.SetTarget(null);
            prevQuickSlot = Inventory.main.quickSlots.activeSlot;
            bool flag = Inventory.main.ReturnHeld();
            Player main = Player.main;
            if (!flag || main.cinematicModeActive)
            {
                return;
            }
            
            MainCameraControl.main.SaveLockedVRViewModelAngle();
            IsOpen = true;
            gameObject.SetActive(true);
            sequence.Set(0.5f, true, Activated);
            UWE.Utils.lockCursor = false;
            if (HandReticle.main != null)
            {
                HandReticle.main.RequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(false);
            UwePostProcessingManager.OpenPDA();
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", true);
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundOpen);
            if (_pda.screen.activeSelf)
            {
                _pda.screen.SetActive(false);
            }
            
            QuickLogger.Debug("FCS PDA Is Open", true);

        }
        
        public void Close()
        {
            IsOpen = false;
            ChangePDAVisibility(true);
            _pda.isInUse = false;
            Player main = Player.main;
            MainCameraControl.main.ResetLockedVRViewModelAngle();
            Vehicle vehicle = main.GetVehicle();
            if (vehicle != null)
            {
                uGUI.main.quickSlots.SetTarget(vehicle);
            }

#if SUBNAUTICA_STABLE
            MainGameController.Instance.PerformGarbageAndAssetCollection();
#else
            MainGameController.Instance.PerformIncrementalGarbageCollection();
#endif
            if (HandReticle.main != null)
            {
                HandReticle.main.UnrequestCrosshairHide();
            }
            Inventory.main.SetViewModelVis(true);
            sequence.Set(0.5f, false, Deactivated);
            
            SafeAnimator.SetBool(Player.main.armsController.animator, "using_pda", false);
            ui.Deselect(null);
            UwePostProcessingManager.ClosePDA();
            _pda.ui.soundQueue.PlayImmediately(_pda.ui.soundClose);
            UwePostProcessingManager.ToggleDof(_depthState);
            QuickLogger.Debug("FCS PDA Is Closed", true);
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
            QuickLogger.Debug("In Find PDA");
            if (PdaCanvas == null)
            {
                PdaCanvas = PDAObj?.GetComponent<PDA>()?.screen?.gameObject?.GetComponent<Canvas>();
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
        }

        internal void ShowMission()
        {
            uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowMessage("Hi","Eggo"); 
        }

        internal bool MakeAPurchase(CartDropDownHandler cart, AlterraHubDepotController depot = null, bool giveToPlayer = false)
        {
            var totalCash = cart.GetTotal();
            var sizes = GetSizes(cart);
            if (giveToPlayer)
            {
                if (CardSystem.main.HasEnough(totalCash) && Inventory.main.container.HasRoomFor(sizes))
                {
                    foreach (CartItem item in cart.GetItems())
                    {
                        for (int i = 0; i < item.ReturnAmount; i++)
                        {
                            QuickLogger.Debug($"{item.ReceiveTechType}", true);
                            PlayerInteractionHelper.GivePlayerItem(item.ReceiveTechType);
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (depot == null || !depot.HasRoomFor(sizes))
                {
                    MessageBoxHandler.main.Show(depot == null ? Buildables.AlterraHub.DepotNotFound() : Buildables.AlterraHub.DepotFull(), FCSMessageButton.OK);
                    return false;
                }

                foreach (var cartItem in cart.GetItems())
                {
                    for (int i = 0; i < cartItem.ReturnAmount; i++)
                    {
                        depot.AddItemToStorage(cartItem.ReceiveTechType.ToInventoryItemLegacy());
                    }
                }
            }

            CardSystem.main.RemoveFinances(totalCash);
            return true;
        }

        private static List<Vector2int> GetSizes(CartDropDownHandler cart)
        {
            var items = new List<Vector2int>();
            foreach (CartItem cartItem in cart.GetItems())
            {
                for (int i = 0; i < cartItem.ReturnAmount; i++)
                {
                    items.Add(CraftData.GetItemSize(cartItem.TechType));
                }
            }

            return items;
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
                    if (_savedData == null || !_cartLoaded)
                    {
                        _savedData = Mod.GetAlterraHubSaveData();
                        LoadCart();
                    }
                    _toggleHud.gameObject.SetActive(true);
                    break;
                case PDAPages.Home:
                case PDAPages.StorePage:
                case PDAPages.Encyclopedia:
                case PDAPages.AccountPage:
                    _toggleHud.gameObject.SetActive(true);
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

            if (_savedData == null)
            {
                _savedData = new FCSPDAEntry();
            }

            _savedData.CartItems = _cartDropDownManager.Save();
            newSaveData.FCSPDAEntry = _savedData;
        }

        internal void LoadCart()
        {
            if (_savedData?.CartItems == null)
            {
                QuickLogger.Debug("Cart Items returned Null");
                _cartLoaded = true;
                return;
            }

            foreach (CartItemSaveData cartItem in _savedData.CartItems)
            {
                _cartDropDownManager.AddItem(cartItem.TechType, cartItem.ReceiveTechType, cartItem.ReturnAmount <= 0 ? 1 : cartItem.ReturnAmount);
            }

            _cartLoaded = true;
        }

        public void OpenEncyclopedia(TechType techType)
        {
            GoToPage(PDAPages.Encyclopedia);
            Open();
        }
    }
    
    internal class EncyclopediaEntryController : MonoBehaviour
    {
        private EncyclopediaEntryData _data;

        internal void Initialize(EncyclopediaEntryData data, Action<EncyclopediaEntryData> callback)
        {
            _data = data;
            gameObject.GetComponent<Text>().text = data.TabTitle;
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener((() => {
                callback?.Invoke(_data);
            }));
        }
    }
}