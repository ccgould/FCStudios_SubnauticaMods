using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Abstract;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Buildable;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Enumerators;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Models.Upgrades;
using FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Structs;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.Mods.DeepDriller.HeavyDuty.Mono
{
    internal class FCSDeepDrillerDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private bool _isInitialized;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private readonly Color _colorEmpty = new(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new(0f, 1f, 0f, 1f);
        private int _pageHash;
        private Image _batteryFill;
        private Text _batteryPercentage;
        private Image _oilFill;
        private Text _oresPerDay;
        private Text _powerUsage;
        private GridHelperPooled _inventoryGrid;
        private GridHelper _filterGrid;
        private GridHelper _programmingGrid;
        private bool _isBeingDestroyed;
        private Text _filterBTNText;
        private readonly Dictionary<TechType, uGUI_FCSDisplayItem> _trackedFilterState = new();
        private Text _itemCounter;
        private Text _unitID;
        private Text _batteryStatus;
        private Text _oilPercentage;
        private FCSToggleButton _filterToggle;
        private FCSToggleButton _filterBlackListToggle;
        private Text _statusLabel;
        private FCSToggleButton _alterraRangeToggle;
        private FCSToggleButton _alterraStorageToggle;
        private ObjectPooler _pooler;
        private HashSet<DrillInventoryButton> _trackedItems = new();
        private InterfaceInteraction _interfaceInteraction;
        private float _updateStatusTimeLeft;
        private GridHelperPooled _libraryGrid;
        private GameObject _libraryDialogWindow;
        private GridHelperV2 _remoteStorageGrid;
        private Text _alterraStorageInformation;
        private Toggle _isVisibleToggle;
        private Text _pingInformation;
        private Text _filterPageInformation;
        private Text _biomeLbl;
        private const string InventoryPoolTag = "Inventory";
        private const string FunctionPoolTag = "Function";

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        private void Update()
        {
            UpdateStatus();
        }

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _pageHash = Animator.StringToHash("Page");

            if (FindAllComponents())
            {
                _isInitialized = true;
                _mono.DeepDrillerPowerManager.OnBatteryUpdate += OnBatteryUpdate;
                _mono.DeepDrillerContainer.OnContainerUpdate += OnContainerUpdate;
                _mono.UpgradeManager.OnUpgradeUpdate += OnUpgradeUpdate;
                _inventoryGrid.DrawPage(1);
                _filterGrid.DrawPage(1);
                UpdateDisplayValues();
                TurnOnDisplay();
                InvokeRepeating(nameof(Updater), 0.3f, 0.3f);
                UpdateCurrentBiome();
                _libraryGrid.DrawPage();
            }
        }

        private void OnContainerUpdate(int arg1, int arg2)
        {
            _inventoryGrid.DrawPage();
        }

        private void Updater()
        {
            UpdateOilLevel();

            if (_filterPageInformation == null) return;
            _filterPageInformation.text = AuxPatchers.FilterPageInformation(_filterToggle.IsSelected,_filterBlackListToggle.IsSelected,_mono.OreGenerator.FocusCount());
        }

        internal void UpdateUnitID()
        {
            if (!string.IsNullOrWhiteSpace(_mono?.UnitID) && !string.IsNullOrWhiteSpace(_unitID?.text))
            {
                QuickLogger.Debug("Setting Unit ID", true);
                _unitID.text = $"UnitID: {_mono.UnitID}";
                UpdateBeaconName();
            }
        }

        internal void UpdateDisplayValues()
        {
            _powerUsage.text = _mono.DeepDrillerPowerManager.GetPowerUsage().ToString();
            _oresPerDay.text = _mono.OreGenerator.GetItemsPerDay();
        }
        
        internal bool IsInteraction()
        {
            return _interfaceInteraction.IsInRange;
        }

        private void OnBatteryUpdate(PowercellData data)
        {
            UpdateBatteryStatus(data);
        }

        public override void TurnOnDisplay()
        {
            QuickLogger.Debug("Powering On Display!", true);
            GotoPage(FCSDeepDrillerPages.Boot);
        }

        public override void TurnOffDisplay()
        {
            QuickLogger.Debug("Powering Off Display!", true);
            _mono.AnimationHandler.SetIntHash(_pageHash, 6);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "InventoryBTN":
                    GotoPage(FCSDeepDrillerPages.Inventory);
                    break;
                case "ProgramBTN":
                    GotoPage(FCSDeepDrillerPages.Programming);
                    break;
                case "ProgrammingAddBTN":
                    _mono.UpgradeManager.Show();
                    break;
                case "SettingsBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "ExStorageBTN":
                    GotoPage(FCSDeepDrillerPages.AlterraStorage);
                    break;
                case "HomeBTN":
                    GotoPage(FCSDeepDrillerPages.Home);
                    break;
                case "ToggleRangeBTN":
                    _mono.ToggleRangeView();
                    break;
                case "ToggleFilterBTN":
                    QuickLogger.Debug("Toggling Filter", true);
                    _mono.OreGenerator.ToggleFocus();
                    break;
                case "FilterPageBTN":
                    GotoPage(FCSDeepDrillerPages.Filter);
                    break;
                case "ExportToggleBTN":
                    QuickLogger.Debug("Export Toggle", true);
                    _mono.TransferManager.Toggle();
                    break;
                case "PowercellDrainBTN":
                    QuickLogger.Debug("Opening powercell dump container", true);
                    _mono.PowercellDumpContainer.OpenStorage();
                    break;
                case "ItemBTN":
                    var item = (TechType)tag;
                    _mono.DeepDrillerContainer.RemoveItemFromContainer(item);
                    break;
                case "LubeRefillBTN":
                    QuickLogger.Debug("Opening Lube Drop Container", true);
                    _mono.OilDumpContainer.OpenStorage();
                    break;
                case "SettingsBackBTN":
                    GotoPage(FCSDeepDrillerPages.Home);
                    break;
                case "FilterBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "AlterraStorageBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "PingBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "ProgrammingBackBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;                
                case "BeaconPageBTN":
                    GotoPage(FCSDeepDrillerPages.BeaconSettings);
                    break;
                case "ToggleBlackListBTN":
                    _mono.OreGenerator.SetBlackListMode(((FilterBtnData)tag).Toggle.IsSelected);
                    break;
                case "LibraryBTN":
                    _mono.UpgradeManager.SetUpdateDialogText(((UpgradeClass) tag).Template);
                    _mono.UpgradeManager.Show();
                    _libraryDialogWindow.SetActive(false);
                    break;
                case "LibraryCloseBTN":
                    _libraryDialogWindow.SetActive(false);
                    break;
                case "ProgrammingTemplateBTN":
                    _libraryDialogWindow.SetActive(true);
                    break;
            }
        }

        internal void GotoPage(FCSDeepDrillerPages page)
        {
            _mono.AnimationHandler.SetIntHash(_pageHash, (int)page);
        }

        public override bool FindAllComponents()
        {
            try
            {

                if (_pooler == null)
                {
                    _pooler = gameObject.AddComponent<ObjectPooler>();
                    _pooler.AddPool(InventoryPoolTag, 12, ModelPrefab.DeepDrillerItemPrefab);
                    _pooler.AddPool(FunctionPoolTag, 9, ModelPrefab.DeepDrillerFunctionOptionItemPrefab);
                    _pooler.Initialize();
                }

                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
                _interfaceInteraction = canvasGameObject.AddComponent<InterfaceInteraction>();
                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Home
                var homePage = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region Inventory Page
                var inventoryPage = InterfaceHelpers.FindGameObject(canvasGameObject, "InventoryPage");
                #endregion

                #region Alterra Storage Page
                var alterraStoragePage = InterfaceHelpers.FindGameObject(canvasGameObject, "AlterraStoragePage");
                #endregion

                #region Filter Page
                var filterPage = InterfaceHelpers.FindGameObject(canvasGameObject, "FilterPage");
                #endregion

                #region Settings Page
                var settingsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "Settings");
                #endregion

                #region Programming Page
                var programmingPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ProgrammingPage");
                #endregion

                #region Signal Page
                var signalPage = InterfaceHelpers.FindGameObject(canvasGameObject, "SignalPage");
                #endregion

                //================= Statue Label =============//

                #region Status Label

                _statusLabel = InterfaceHelpers.FindGameObject(canvasGameObject, "StatusLabel").GetComponent<Text>();

                #endregion

                //================= Home Page ================//


                #region Inventory Button

                var inventoryBTN = GameObjectHelpers.FindGameObject(homePage, "StorageBTN");
                InterfaceHelpers.CreateButton(inventoryBTN, "InventoryBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.InventoryButton());

                #endregion

                #region Settings Button

                var settingsBTN = GameObjectHelpers.FindGameObject(homePage, "SettingsBTN");
                InterfaceHelpers.CreateButton(settingsBTN, "SettingsBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.SettingsButton());

                #endregion

                #region Battery Meter

                var batteryMeter = InterfaceHelpers.FindGameObject(homePage, "BatteryMeter");
                _batteryFill = batteryMeter?.FindChild("Fill")?.GetComponent<Image>();
                _batteryStatus = batteryMeter?.FindChild("BatteryStatus")?.GetComponent<Text>();
                _batteryStatus.text = $"0/{QPatch.Configuration.DDInternalBatteryCapacity}";

                if (_batteryFill != null)
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }

                _batteryPercentage = batteryMeter?.FindChild("Percentage")?.GetComponent<Text>();



                #endregion

                #region Oil Meter

                var oilMeter = GameObjectHelpers.FindGameObject(homePage, "LubeMeter");
                _oilFill = oilMeter?.FindChild("Fill")?.GetComponent<Image>();
                _oilPercentage = oilMeter?.FindChild("Percentage")?.GetComponent<Text>();
                if (_oilFill != null)
                {
                    _oilFill.color = _colorEmpty;
                    _oilFill.fillAmount = 0f;
                }

                #endregion

                #region Items Per Day
                //_itemsPerDay = GameObjectHelpers.FindGameObject(homePage, "ItemsPerDayLBL")?.GetComponent<Text>();
                #endregion

                #region UnitID

                _unitID = InterfaceHelpers.FindGameObject(homePage, "UnitID").GetComponent<Text>();

                #endregion

                #region OresPerDay

                _oresPerDay = InterfaceHelpers.FindGameObject(homePage, "OresPerDayAmount").GetComponent<Text>();
                var oresPerDayLabel = InterfaceHelpers.FindGameObject(homePage, "OresPerDay").GetComponent<Text>();
                oresPerDayLabel.text = FCSDeepDrillerBuildable.OresPerDay();
                #endregion

                #region Power Consumption

                _powerUsage = InterfaceHelpers.FindGameObject(homePage, "PowerConsumptionAmount").GetComponent<Text>();
                var powerConsumptionLabel = InterfaceHelpers.FindGameObject(homePage, "PowerConsumption").GetComponent<Text>();
                powerConsumptionLabel.text = FCSDeepDrillerBuildable.PowerConsumption();

                #endregion

                #region Biome

                _biomeLbl = InterfaceHelpers.FindGameObject(homePage, "Biome").GetComponent<Text>();

                #endregion

                //================= Inventory Page ================//

                #region Inventory Grid

                _inventoryGrid = _mono.gameObject.AddComponent<GridHelperPooled>();
                _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
                _inventoryGrid.Setup(12, _pooler, inventoryPage, OnButtonClick);

                #endregion

                _itemCounter = GameObjectHelpers.FindGameObject(inventoryPage, "InventoryLabel")?.GetComponent<Text>();

                //================= Settings Page ================//

                #region Find Unit

                //_unitID = GameObjectHelpers.FindGameObject(homePage, "UnitID")?.GetComponent<Text>();

                #endregion

                #region Filter Button

                var filterBTN = InterfaceHelpers.FindGameObject(settingsPage, "FilterBTN");
                InterfaceHelpers.CreateButton(filterBTN, "FilterPageBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.FilterButton());

                #endregion

                #region Program Button

                var programBTN = GameObjectHelpers.FindGameObject(settingsPage, "ProgramBTN");
                InterfaceHelpers.CreateButton(programBTN, "ProgramBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ProgrammingButton());

                #endregion

                #region Powercell Drain Button

                var powercellDrainBTN = InterfaceHelpers.FindGameObject(settingsPage, "PowercellDrainBTN");
                InterfaceHelpers.CreateButton(powercellDrainBTN, "PowercellDrainBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.PowercellDrainButton());

                #endregion

                #region Lube Refill Button

                var lubeRefillBTN = InterfaceHelpers.FindGameObject(settingsPage, "LubeRefillBTN");
                InterfaceHelpers.CreateButton(lubeRefillBTN, "LubeRefillBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.LubeRefillButton());

                #endregion

                #region ExStorage Button

                var exStorageBTN = InterfaceHelpers.FindGameObject(settingsPage, "ExStorageBTN");
                InterfaceHelpers.CreateButton(exStorageBTN, "ExStorageBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AlterraStorageButton());

                #endregion

                #region Ping Button

                var pingSettingsBTN = InterfaceHelpers.FindGameObject(settingsPage, "PingSettingsBTN");
                InterfaceHelpers.CreateButton(pingSettingsBTN, "BeaconPageBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.BeaconSettingsButton());

                #endregion

                #region Setting Back Button

                var backBTN = InterfaceHelpers.FindGameObject(settingsPage, "BackBTN");
                InterfaceHelpers.CreateButton(backBTN, "SettingsBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToHome());

                #endregion

                //================= Filter Page ================//

                #region Filter Grid

                _filterGrid = _mono.gameObject.AddComponent<GridHelper>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(6, ModelPrefab.DeepDrillerOreBTNPrefab, filterPage, _startColor, _hoverColor, OnButtonClick, 5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);

                #endregion

                #region Filter Back Button

                var filterBackBTN = InterfaceHelpers.FindGameObject(filterPage, "BackBTN");
                InterfaceHelpers.CreateButton(filterBackBTN, "FilterBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Filter Toggle Button

                var filterToggleBTN = InterfaceHelpers.FindGameObject(filterPage, "ToggleFilterBTN");
                _filterToggle = filterToggleBTN.AddComponent<FCSToggleButton>();
                _filterToggle.ButtonMode = InterfaceButtonMode.Background;
                _filterToggle.STARTING_COLOR = _startColor;
                _filterToggle.HOVER_COLOR = _hoverColor;
                _filterToggle.BtnName = "ToggleFilterBTN";
                _filterToggle.TextLineOne = FCSDeepDrillerBuildable.FilterButton();
                _filterToggle.TextLineTwo = FCSDeepDrillerBuildable.FilterButtonDesc();
                _filterToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Filter Blacklist Toggle Button

                var filterBlackListToggleBTN = InterfaceHelpers.FindGameObject(filterPage, "ToggleBlackListBTN");
                _filterBlackListToggle = filterBlackListToggleBTN.AddComponent<FCSToggleButton>();
                _filterBlackListToggle.ButtonMode = InterfaceButtonMode.Background;
                _filterBlackListToggle.STARTING_COLOR = _startColor;
                _filterBlackListToggle.HOVER_COLOR = _hoverColor;
                _filterBlackListToggle.GetAdditionalDataFromString = true;
                _filterBlackListToggle.GetAdditionalString += o =>
                {
                    
                    return FCSDeepDrillerBuildable.BlackListToggleDesc(_filterBlackListToggle.IsSelected); 

                };
                _filterBlackListToggle.BtnName = "ToggleBlackListBTN";
                _filterBlackListToggle.Tag = new FilterBtnData { Toggle = _filterBlackListToggle };
                _filterBlackListToggle.TextLineOne = FCSDeepDrillerBuildable.BlackListToggle();
                _filterBlackListToggle.OnButtonClick = OnButtonClick;

                #endregion

                _filterPageInformation = GameObjectHelpers.FindGameObject(gameObject, "FilterPageInformation")?.GetComponent<Text>();

                //================= Alterra Storage Page ================//

                #region Alterra Storage Back Button

                var alterraStorageBackBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "BackBTN");
                InterfaceHelpers.CreateButton(alterraStorageBackBTN, "AlterraStorageBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Alterra Storage Toggle Button

                var alterraStorageToggleBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "ToggleAlterraStorageBTN");
                _alterraStorageToggle = alterraStorageToggleBTN.AddComponent<FCSToggleButton>();
                _alterraStorageToggle.ButtonMode = InterfaceButtonMode.Background;
                _alterraStorageToggle.STARTING_COLOR = _startColor;
                _alterraStorageToggle.HOVER_COLOR = _hoverColor;
                _alterraStorageToggle.BtnName = "ExportToggleBTN";
                _alterraStorageToggle.TextLineOne = FCSDeepDrillerBuildable.AlterraStorageToggle();
                _alterraStorageToggle.TextLineTwo = FCSDeepDrillerBuildable.AlterraStorageToggleDesc();
                _alterraStorageToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Alterra Storage Range Button

                var alterraRangeToggleBTN = InterfaceHelpers.FindGameObject(alterraStoragePage, "ToggleRangeBTN");
                _alterraRangeToggle = alterraRangeToggleBTN.AddComponent<FCSToggleButton>();
                _alterraRangeToggle.ButtonMode = InterfaceButtonMode.Background;
                _alterraRangeToggle.STARTING_COLOR = _startColor;
                _alterraRangeToggle.HOVER_COLOR = _hoverColor;
                _alterraRangeToggle.BtnName = "ToggleRangeBTN";
                _alterraRangeToggle.TextLineOne = FCSDeepDrillerBuildable.AlterraStorageRangeToggle();
                _alterraRangeToggle.TextLineTwo = FCSDeepDrillerBuildable.AlterraStorageRangeToggleDesc();
                _alterraRangeToggle.OnButtonClick = OnButtonClick;

                #endregion

                #region Information

                _alterraStorageInformation = GameObjectHelpers.FindGameObject(gameObject, "Information")?.GetComponent<Text>();

                #endregion

                //================= Programming Page ================//

                #region Programming Grid

                _programmingGrid = _mono.gameObject.AddComponent<GridHelper>();
                _programmingGrid.OnLoadDisplay += OnLoadProgrammingGrid;
                _programmingGrid.Setup(4, ModelPrefab.DeepDrillerOverrideItemPrefab, programmingPage, _startColor, _hoverColor, OnButtonClick, 5, "PrevBTN", "NextBTN", "Grid", "Paginator", string.Empty);

                #endregion

                #region Library Grid

                _libraryDialogWindow = programmingPage.FindChild("Library");
                _libraryGrid = _mono.gameObject.AddComponent<GridHelperPooled>();
                _libraryGrid.OnLoadDisplay += OnLoadLibraryGrid;
                _libraryGrid.Setup(9, _pooler, _libraryDialogWindow, OnButtonClick,false);

                var closeBtnObj = _libraryDialogWindow.FindChild("CloseBTN");
                InterfaceHelpers.CreateButton(closeBtnObj, "LibraryCloseBTN",
                    InterfaceButtonMode.Background, OnButtonClick, Color.white, Color.cyan,5);

                #endregion

                #region Programming Back Button

                var programmingBackBTN = InterfaceHelpers.FindGameObject(programmingPage, "BackBTN");
                InterfaceHelpers.CreateButton(programmingBackBTN, "ProgrammingBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Programming Add Button

                var programmingAddBTN = InterfaceHelpers.FindGameObject(programmingPage, "AddBTN");
                InterfaceHelpers.CreateButton(programmingAddBTN, "ProgrammingAddBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AddProgramButton(), FCSDeepDrillerBuildable.AddProgramButtonDec());

                #endregion

                #region Programming Template Button
                var programmingTemplateBTN = InterfaceHelpers.FindGameObject(programmingPage, "TemplateBTN");
                InterfaceHelpers.CreateButton(programmingTemplateBTN, "ProgrammingTemplateBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ProgrammingTemplateButton(), FCSDeepDrillerBuildable.ProgrammingTemplateButtonDesc());

                #endregion

                //================= Ping Page ================//


                #region Ping Back Button

                var pingBackBtn = InterfaceHelpers.FindGameObject(signalPage, "BackBTN");
                InterfaceHelpers.CreateButton(pingBackBtn, "PingBackBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.GoToSettings());

                #endregion

                #region Visible Toggle Button

                var isVisibleToggleBTN = GameObjectHelpers.FindGameObject(signalPage, "ToggleIsVisibleBTN");
                
                _isVisibleToggle = isVisibleToggleBTN.GetComponent<Toggle>();
                _isVisibleToggle.onValueChanged.AddListener((state =>
                {
                    _mono.ToggleVisibility(state);
                }));

                #endregion

                #region Information

                _pingInformation = GameObjectHelpers.FindGameObject(signalPage, "Information").GetComponent<Text>();

                #endregion

                #region Edit Button

                var editBackBTN = GameObjectHelpers.FindGameObject(signalPage, "EditNameButton");
                InterfaceHelpers.CreateButton(editBackBTN, "EditNameButton", InterfaceButtonMode.Background,
                    ((s, o) =>
                    {
                        uGUI.main.userInput.RequestString("Enter Beacon Name", "Rename", _pingInformation.text, 100,
                            (text =>
                            {
                                _pingInformation.text = text;
                                _mono.SetPingName(text);
                            }));
                    }),
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.ClickToEdit());

                #endregion



            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Has been caught:");
                QuickLogger.Error($"Message:\n {e.Message}");
                QuickLogger.Error($"StackTrace:\n {e.StackTrace}");
                return false;
            }

            return true;
        }

        internal void UpdatePingToggleState(bool state)
        {
            _isVisibleToggle.SetIsOnWithoutNotify(state);
        }

        private void OnLoadLibraryGrid(DisplayDataPooled data)
        {
            data.Pool.Reset(FunctionPoolTag);

            var grouped = _mono.UpgradeManager.Classes;

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                if(string.IsNullOrEmpty(grouped[i].Template)) continue;

                GameObject buttonPrefab = data.Pool.SpawnFromPool(FunctionPoolTag, data.ItemsGrid);

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    return;
                }

                var item = buttonPrefab.EnsureComponent<LibraryUpdateItemButton>();
                item.ChangeText(grouped[i].FriendlyName);
                item.ButtonMode = InterfaceButtonMode.Background;
                item.Tag = grouped[i];
                item.TextLineOne = grouped[i].FriendlyName;
                item.STARTING_COLOR = Color.gray;
                item.HOVER_COLOR = Color.white;
                item.BtnName = "LibraryBTN";
                item.OnButtonClick = OnButtonClick;

            }
            _libraryGrid.UpdaterPaginator(grouped.Count);
        }


        internal void RefreshAlterraStorageList(int amount)
        {
            if(_alterraStorageInformation == null) return;
            _alterraStorageInformation.text = $"Remote Storage Connections:\n{amount}";
        }

        private void OnUpgradeUpdate(UpgradeFunction obj)
        {
            QuickLogger.Debug("Refreshing the Upgrade Page", true);
            UpdateDisplayValues();
            _programmingGrid.DrawPage();

        }

        private void OnLoadProgrammingGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadProgrammingGrid : {data.ItemsGrid}", true);

                _programmingGrid.ClearPage();

                var grouped = _mono.UpgradeManager.Upgrades;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {

                    GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        if (buttonPrefab != null)
                        {
                            Destroy(buttonPrefab);
                        }
                        return;
                    }

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var upgradeText = buttonPrefab.GetComponentInChildren<Text>();
                    upgradeText.text = grouped.ElementAt(i).Format();

                    var deleteButton = GameObjectHelpers.FindGameObject(buttonPrefab, "DeleteBTN");
                    var deleteBTN = deleteButton.AddComponent<InterfaceButton>();
                    var function = grouped.ElementAt(i);
                    function.Label = upgradeText;
                    deleteBTN.OnButtonClick += (s, o) =>
                    {
                        _mono.UpgradeManager.DeleteFunction(function);
                    };

                    var activateButton = GameObjectHelpers.FindGameObject(buttonPrefab, "EnableToggleBTN");
                    var activateToggleBTN = activateButton.GetComponent<Toggle>();
                    //activateToggleBTN.TextLineOne = FCSDeepDrillerBuildable.FilterButton();
                    activateToggleBTN.onValueChanged.AddListener((value =>
                    {
                        function.ToggleUpdate();
                    }));
                }

                _programmingGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadFilterGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadFilterGrid : {data.ItemsGrid}");

                if (_trackedFilterState.Count <= 0)
                {
                    //Create all filters
                    var grouped = _mono.OreGenerator.AllowedOres;
                    foreach (TechType techType in grouped)
                    {
                        GameObject buttonPrefab = Instantiate(data.ItemsPrefab);
                        buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                        var itemBTN = buttonPrefab.AddComponent<uGUI_FCSDisplayItem>();
                        itemBTN.Initialize(techType, true);
                        itemBTN.Subscribe((value =>
                        {
                            if (value)
                            {
                                _mono.OreGenerator.AddFocus(techType);
                            }
                            else
                            {
                                _mono.OreGenerator.RemoveFocus(techType);
                            }
                        }));
                        itemBTN.Hide();
                        _trackedFilterState.Add(techType, itemBTN);
                    }
                }

                var allowedOres = _mono.OreGenerator.AllowedOres;

                if (data.EndPosition > allowedOres.Count)
                {
                    data.EndPosition = allowedOres.Count;
                }

                foreach (KeyValuePair<TechType, uGUI_FCSDisplayItem> toggle in _trackedFilterState)
                { 
                    toggle.Value.Hide();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedFilterState.ElementAt(i).Value.Show();
                }
                _filterGrid.UpdaterPaginator(allowedOres.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadItemsGrid(DisplayDataPooled data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                var grouped = _mono.DeepDrillerContainer.GetItemsWithin();
                
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if(CheckIfButtonIsActive(grouped.ElementAt(i).Key)) {continue;}
                    
                    GameObject buttonPrefab = data.Pool.SpawnFromPool(InventoryPoolTag, data.ItemsGrid);
                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var itemBTN = buttonPrefab.EnsureComponent<DrillInventoryButton>();
                    itemBTN.ButtonMode = InterfaceButtonMode.Background;
                    itemBTN.STARTING_COLOR = _startColor;
                    itemBTN.HOVER_COLOR = _hoverColor;
                    itemBTN.BtnName = "ItemBTN";
                    itemBTN.TextLineOne = FCSDeepDrillerBuildable.TakeFormatted(Language.main.Get(grouped.ElementAt(i).Key));
                    itemBTN.Tag = grouped.ElementAt(i).Key;
                    itemBTN.RefreshIcon();
                    itemBTN.DrillStorage = _mono.DeepDrillerContainer;
                    itemBTN.OnButtonClick = OnButtonClick;
                    _trackedItems.Add(itemBTN);
                }
                _inventoryGrid.UpdaterPaginator(grouped.Count);
                RefreshStorageAmount();
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private bool CheckIfButtonIsActive(TechType techType)
        {
            foreach (DrillInventoryButton button in _trackedItems)
            {
                if (button.IsValidAndActive(techType))
                {
                    QuickLogger.Debug($"Button is valid: {techType} UpdatingButton",true);
                    button.UpdateAmount();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates the checked state of the focus items on the screen
        /// </summary>
        /// <param name="dataFocusOres"></param>
        internal void UpdateListItemsState(HashSet<TechType> dataFocusOres)
        {
            //for (int dataFocusOresIndex = 0; dataFocusOresIndex < dataFocusOres.Count; dataFocusOresIndex++)
            //{
            //    for (int trackedFilterItemsIndex = 0; trackedFilterItemsIndex < TrackedFilterItems.Count; trackedFilterItemsIndex++)
            //    {
            //        var filterData = (FilterBtnData)TrackedFilterItems.ElementAt(trackedFilterItemsIndex).Tag;
            //        if (filterData.TechType == dataFocusOres.ElementAt(dataFocusOresIndex))
            //        {
            //            filterData.Toggle.IsSelected = true;
            //        }
            //    }
            //}
        }

        internal void UpdateBatteryStatus(PowercellData data)
        {
            var charge = data.GetCharge() < 1 ? 0f : data.GetCharge();

            var percent = charge / data.GetCapacity();

            if (_batteryFill != null)
            {
                if (data.GetCharge() >= 0f)
                {
                    var value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
                    _batteryFill.color = value;
                    _batteryFill.fillAmount = percent;
                }
                else
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }
            }

            _batteryPercentage.text = ((data.GetCharge() < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{percent:P0}"
                );
            _batteryStatus.text = $"{Mathf.RoundToInt(data.GetCharge())}/{data.GetCapacity()}";

        }

        internal void UpdateOilLevel()
        {
            var percent = _mono.OilHandler.GetOilPercent();
            _oilFill.fillAmount = percent;
            Color value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
            _oilFill.color = value;
            _oilPercentage.text = $"{percent / 1 * 100}%";
        }

        internal void LoadFromSave(DeepDrillerSaveDataEntry save)
        {
            if (save.IsFocused)
            {
                _filterToggle.Select();
            }

            if (save.IsBlackListMode)
            {
                _filterBlackListToggle.Select();
            }

            if (save.AllowedToExport)
            {
                _alterraStorageToggle.Select();
            }

            if (save.IsRangeVisible)
            {
                _alterraRangeToggle.Select();
            }

            if (!string.IsNullOrWhiteSpace(save.BeaconName))
            {
                UpdateBeaconName(save.BeaconName);
            }

            foreach (KeyValuePair<TechType, uGUI_FCSDisplayItem> toggleButton in _trackedFilterState)
            {
                if (_mono.OreGenerator.GetFocusedOres().Contains(toggleButton.Key))
                {
                    toggleButton.Value.Select();
                }
            }
        }

        internal void UpdateBeaconName(string beaconName = null)
        {
            if (string.IsNullOrWhiteSpace(beaconName))
            {
                var defaultName = $"Deep Driller - {_mono.UnitID}";
                _mono.SetPingName(defaultName);
                _pingInformation.text = defaultName;
            }
            else
            {
                _mono.SetPingName(beaconName);
                _pingInformation.text = beaconName;
            }
        }

        internal void RefreshStorageAmount()
        {
            if (_itemCounter == null || _mono?.DeepDrillerContainer == null) return;
            _itemCounter.text = FCSDeepDrillerBuildable.InventoryStorageFormat(_mono.DeepDrillerContainer.GetContainerTotal(), QPatch.Configuration.DDStorageSize);
        }

        internal void UpdateStatus()
        {
            _updateStatusTimeLeft -= DayNightCycle.main.deltaTime;
            if (_updateStatusTimeLeft <= 0)
            {
                if (_mono == null || !_mono.IsConstructed || !_mono.IsInitialized) return;

                var message = string.Empty;

                if (!_mono.OilHandler.HasOil())
                {
                    message = FCSDeepDrillerBuildable.NeedsOil();
                }
                else if (_mono.IsBreakSet())
                {
                    message = FCSDeepDrillerBuildable.DrillDeactivated();
                }
                else if (_mono.DeepDrillerContainer.IsFull)
                {
                    message = AuxPatchers.InventoryFull();
                }
                else if (!_mono.OreGenerator.GetIsDrilling())
                {
                    message = FCSDeepDrillerBuildable.Idle();
                }
                else if (_mono.OreGenerator.GetIsDrilling())
                {
                    message = FCSDeepDrillerBuildable.Drilling();
                }

                _updateStatusTimeLeft = 1f;

                if (_statusLabel.text.Equals(message)) return;
                _statusLabel.text = message;
            }
        }

        public Text GetStatusField()
        {
            return _statusLabel;
        }

        internal void UpdateCurrentBiome()
        {
            _biomeLbl.text = FCSDeepDrillerBuildable.BiomeFormat(_mono.CurrentBiome);
        }
    }

    internal class InfoButton : OnScreenButton, IPointerEnterHandler, IPointerExitHandler
    {
        private void OnEnable()
        {
            Disabled = false;
        }
    }

    internal class LibraryUpdateItemButton : InterfaceButton
    {
    }
}
