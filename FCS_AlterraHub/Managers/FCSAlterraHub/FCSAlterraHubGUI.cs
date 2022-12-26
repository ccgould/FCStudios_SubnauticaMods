using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Dialogs;
using FCS_AlterraHub.Patches;
using FCS_AlterraHub.Systems;
using UnityEngine;
using static Equipment;
using UnityEngine.UI;
using FCS_AlterraHub.Mods.FCSPDA.Enums;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Mods.FCSPDA.Mono.ScreenItems;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Mods.FCSPDA.Mono.Model;
using SMLHelper.V2.Utility;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mods.Common.DroneSystem;
using FCS_AlterraHub.Registration;
using FCS_AlterraHub.Structs;
using FCSCommon.Utilities;
using FCS_AlterraHub.Mods.Common.DroneSystem.Models;
using FCSCommon.Interfaces;
using System.Linq.Expressions;
using FCS_AlterraHub.Mono.Managers;

namespace FCS_AlterraHub.Managers.FCSAlterraHub
{
    public class FCSAlterraHubGUI : MonoBehaviour
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
        private Dictionary<StoreCategory, List<StoreItem>> _storeItems = new();
        private GameObject _storePageGrid;
        private Text _storeLabel;
        private CartDropDownHandler _cartDropDownManager;
        private bool _cartLoaded;
        private CheckOutPopupDialogWindow _checkoutDialog;
        private AccountPageHandler _accountPageHandler;
        private Text _currentBaseInfo;
        private TeleportationPageController _teleportationPageController;
        private GameObject _404;
        private ShipmentPageController _shipmentPageController;
        internal EncyclopediaTabController EncyclopediaTabController { get; set; }
        private ReturnsDialogController _returnsDialogController;

        private bool _isInitialized;
        private Canvas _canvas;
        private GameObject _toggleHud;
        private MessageBoxHandler _messageBox;
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
                _404.FindChild("Message").GetComponent<Text>().text = AlterraHub.Error404();
            }

            
            _checkoutDialog = GameObjectHelpers.FindGameObject(_canvas.gameObject,"CheckOutPopUp")?.AddComponent<CheckOutPopupDialogWindow>();
            _returnsDialogController = GameObjectHelpers.FindGameObject(_canvas.gameObject, "ReturnItemsDialog")?.AddComponent<ReturnsDialogController>();
            _returnsDialogController?.Initialize(this);
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

            _cartDropDownManager = GameObjectHelpers.FindGameObject(_canvas.gameObject, "CartDropDown")?.AddComponent<CartDropDownHandler>();
            
            if (_cartDropDownManager is not null)
            {
                _cartDropDownManager.OnBuyAllBtnClick += OnBuyAllBtnClick;
                _cartDropDownManager.Initialize(this);
                _cartDropDownManager.onTotalChanged += amount =>
                {
                    _cartAmountLabel.text = $"Cart Amount: {amount:n0}";
                    _cartTotalLabel.text = $"Cart Total: {_cartDropDownManager.GetCartCount()}";
                    _cartButtonNumber.text = _cartDropDownManager.GetCartCount().ToString();
                };
            }


            MaterialHelpers.ApplyEmissionShader(AlterraHub.BasePrimaryCol, gameObject, Color.white, 0, 0.01f, 0.01f);
            MaterialHelpers.ApplySpecShader(AlterraHub.BasePrimaryCol, gameObject, 1, 6.15f);
            _messageBox.ObjectRoot = gameObject;
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
            _isInitialized = true;
        }


        private void OnBuyAllBtnClick(CartDropDownHandler obj)
        {
            _checkoutDialog.ShowDialog(this, _cartDropDownManager);
            _cartDropDownManager.ToggleVisibility();
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
            var pageTextLabel = _pages[PDAPages.Home]?.FindChild("PageName")?.GetComponent<Text>();
            var radialMenu = _pages[PDAPages.Home]?.FindChild("RadialMenu")?.AddComponent<RadialMenu>();

            if (radialMenu is null) return;
            radialMenu.TabAmount = 4;
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "Cart_Icon.png")), pageTextLabel, "Store", PDAPages.Store);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "EncyclopediaIcon.png")), pageTextLabel, "Encyclopedia", PDAPages.Encyclopedia);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "IconAccount.png")), pageTextLabel, "Account", PDAPages.AccountPage);
            radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "QuantumTeleporterIcon_W.png")), pageTextLabel, "Teleportation", PDAPages.Teleportation);
            radialMenu.Rearrange();
        }

        private void CreateStorePage()
        {
            _cartButtonNumber = GameObjectHelpers.FindGameObject(_pages[PDAPages.Store], "CartCount")?.GetComponentInChildren<Text>();
            var pageTextLabel = _pages[PDAPages.Store]?.FindChild("PageName")?.GetComponent<Text>();
            var radialMenu = _pages[PDAPages.Store]?.FindChild("RadialMenu")?.AddComponent<RadialMenu>();

            var cartBTN = _pages[PDAPages.Store]?.FindChild("Cart")?.GetComponent<Button>();
            if (cartBTN != null)
            {
                cartBTN.onClick.AddListener((() =>
                {
                    _cartDropDownManager.ToggleVisibility();
                }));
            }

            var returnsBTN = _pages[PDAPages.Store].FindChild("Returns").GetComponent<Button>();
            returnsBTN.onClick.AddListener(() =>
            {
                _returnsDialogController.Open();
            });

            var backButton = _pages[PDAPages.Store]?.FindChild("BackBTN")?.GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.AddListener((() =>
                {
                    GoToPage(PDAPages.Home);
                }));
            }

            if (radialMenu is not null)
            {
                radialMenu.TabAmount = 7;
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "HomeSolutionsIcon_W.png")), pageTextLabel, "Home Solutions", PDAPages.HomeSolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "LifeSupportIcon_W.png")), pageTextLabel, "Life Solutions", PDAPages.LifeSolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "EnergySolutionsIcon_W.png")), pageTextLabel, "Energy Solutions", PDAPages.EnergySolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "ProductionSolutionsIcon_W.png")), pageTextLabel, "Production Solutions", PDAPages.ProductionSolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "StoreSolutionsIcon_W.png")), pageTextLabel, "Storage Solutions", PDAPages.StorageSolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "VehicleSolutionsIcon_W.png")), pageTextLabel, "Vehicle Solutions", PDAPages.VehicleSolutions);
                radialMenu.AddEntry(this, ImageUtils.LoadSpriteFromFile(Path.Combine(Mod.GetAssetPath(), "Icons", "MiscIcon_W.png")), pageTextLabel, "Misc", PDAPages.MiscSolutions);
            }


            //Set gameobject to toggle for pages
            _pages[PDAPages.HomeSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.LifeSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.EnergySolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.ProductionSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.StorageSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.VehicleSolutions] = _pages[PDAPages.StorePage];
            _pages[PDAPages.MiscSolutions] = _pages[PDAPages.StorePage];

            radialMenu?.Rearrange();
        }

        private void CreateStorePagePage()
        {
            var backButton = _pages[PDAPages.StorePage]?.FindChild("BackBTN")?.GetComponent<Button>();
            _cartAmountLabel = _pages[PDAPages.StorePage].FindChild("StoreHud")?.FindChild("CartAmount")?.GetComponent<Text>();
            _cartTotalLabel = _pages[PDAPages.StorePage].FindChild("StoreHud")?.FindChild("CartTotal")?.GetComponent<Text>();
            _storePageGrid = GameObjectHelpers.FindGameObject(_pages[PDAPages.StorePage], "Content");
            _storeLabel = GameObjectHelpers.FindGameObject(_pages[PDAPages.StorePage], "StoreLabel")?.GetComponent<Text>();

            if (backButton != null)
            {
                backButton.onClick.AddListener((() =>
                {
                    GoToPage(PDAPages.Store);
                }));
            }
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

        internal void AttemptToOpenReturnsDialog()
        {
            if (_returnsDialogController?.IsOpen ?? false)
            {
                _returnsDialogController.Open();
            }
        }


        internal void GoToPage(PDAPages page)
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

        internal bool GetAutomaticDebitDeduction()
        {
            return _accountPageHandler.GetAutomaticDebitDeduction();
        }

        private void LoadStore()
        {

            foreach (StoreCategory category in Enum.GetValues(typeof(StoreCategory)))
            {
                foreach (var storeItem in FCSAlterraHubService.PublicAPI.GetRegisteredKits())
                {
                    if (storeItem.Value.StoreCategory != category) continue;
                    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.Key)}");

                    var item = StoreInventorySystem.CreateStoreItem(storeItem.Value, AddToCartCallBack, IsInUse);

                    if (_storeItems.ContainsKey(category))
                    {
                        _storeItems[category].Add(item);
                    }
                    else
                    {
                        _storeItems.Add(category, new List<StoreItem> { item });
                    }

                    item.gameObject.transform.SetParent(_storePageGrid.transform, false);

                    QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.Key)} with category to Panel: {storeItem.Value.StoreCategory}:");
                }

                foreach (FCSStoreEntry storeItem in Main.Configuration.AdditionalStoreItems)
                {
                    if (storeItem.StoreCategory != category) continue;

                    QuickLogger.Debug($"Trying to add Store Item  {Language.main.Get(storeItem.TechType)}");

                    var item = StoreInventorySystem.CreateStoreItem(storeItem, AddToCartCallBack, IsInUse);
                    if (_storeItems.ContainsKey(category))
                    {
                        _storeItems[category].Add(item);
                    }
                    else
                    {
                        _storeItems.Add(category, new List<StoreItem> { item });
                    }

                    item.gameObject.transform.SetParent(_storePageGrid.transform, false);

                    QuickLogger.Debug($"Added Store Item  {Language.main.Get(storeItem.TechType)} with category to Panel: {storeItem.StoreCategory}:");
                }
            }
        }

        private bool IsInUse()
        {
            return IsOpen;
        }

        private void AddToCartCallBack(TechType techType, TechType receiveTechType, int returnAmount)
        {
            _cartDropDownManager?.AddItem(techType, receiveTechType, returnAmount);
        }
        
        internal void ExitStore()
        {
            GoToPage(PDAPages.Home);
        }

        internal void ShowMission()
        {
            // uGUI_PowerIndicator_Initialize_Patch.MissionHUD.ShowMessage("Hi","Eggo"); 
        }

        internal void ShowMessage(string message)
        {
            _messageBox.Show(message, FCSMessageButton.OK);
        }

        internal void RefreshTeleportationPage()
        {
            _teleportationPageController.Refresh();
        }

        internal void TryRemove404Screen()
        {
            if (DroneDeliveryService.Main == null)
            {
                _messageBox.Show(AlterraHub.ErrorHasOccurred("0x0001"), FCSMessageButton.OK);
                return;
            }

            _404?.SetActive(!DroneDeliveryService.Main.DetermineIfFixed());
        }

        internal void UpdateDisplay()
        {
            if (_currentBiome == null || _accountName == null || _currentBaseInfo == null) return;

            _currentBiome.text = Player.main.GetBiomeString();

            if (!string.IsNullOrWhiteSpace(CardSystem.main.GetUserName()))
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

        internal void AddShipment(Shipment shipment)
        {
            _shipmentPageController.AddItem(shipment);
        }

        internal void RemoveShipment(Shipment shipment)
        {
            _shipmentPageController.RemoveItem(shipment);
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
            _cartDropDownManager.OnBuyAllBtnClick -= OnBuyAllBtnClick;
        }

        internal void LoadFromSave(ShipmentInfo shipmentInfo)
        {
            if (shipmentInfo != null)
            {
                _cartDropDownManager.LoadShipmentInfo(shipmentInfo);
            }

            _accountPageHandler.Refresh();

            _cartLoaded = true;
        }

        internal ShipmentInfo GetShipmentInfo()
        {
            return _cartDropDownManager.GetShipmentInfo();
        }
    }
}
