using System;
using System.Collections.Generic;
using System.Linq;
using FCS_DeepDriller.Buildable.MK2;
using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_DeepDriller.Mono.MK2
{
    internal class FCSDeepDrillerDisplay : AIDisplay
    {
        private FCSDeepDrillerController _mono;
        private bool _isInitialized;
        private string _currentBiome;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private readonly Color _colorEmpty = new Color(1f, 0f, 0f, 1f);
        private readonly Color _colorHalf = new Color(1f, 1f, 0f, 1f);
        private readonly Color _colorFull = new Color(0f, 1f, 0f, 1f);
        private bool IsOperational => _isInitialized && _mono != null && _mono.IsInitialized && !_mono.IsBeingDeleted;
        private HashSet<InterfaceButton> TrackedFilterItems = new HashSet<InterfaceButton>();
        private int _pageHash;
        private Image _batteryFill;
        private Text _batteryPercentage;
        private Image _oilFill;
        private Text _itemsPerDay;
        private Text _powerUsage;
        private GridHelper _itemsGrid;
        private GridHelper _filterGrid;
        private GridHelper _programmingGrid;
        private bool _isBeingDestroyed;
        private ColorManager _colorPage;
        private Text _filterBTNText;
        private readonly Dictionary<TechType,Toggle> _trackedFilterState = new Dictionary<TechType, Toggle>();

        private Text _itemCounter;
        private Image _solarPanelBtnIcon;
        private Text _exportToggleBTNText;
        private Image _rangeToggleBTNIcon;
        private float _timePassed;
        private Text _unitID;
        private Toggle _blackListToggle;

        private void Update()
        {
            _timePassed += DayNightCycle.main.deltaTime;

            if (_timePassed >= 1)
            {
                UpdateToggleButtons();
                
                _timePassed = 0.0f;
            }
        }
        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(FCSDeepDrillerController mono)
        {
            _mono = mono;
            _pageHash = Animator.StringToHash("page");
            _colorPage = mono.ColorManager;

            if (FindAllComponents())
            {
                _isInitialized = true;
                _mono.DeepDrillerPowerManager.OnBatteryUpdate += OnBatteryUpdate;
                //_mono.DeepDrillerContainer.OnContainerUpdate += OnContainerUpdate;
                _mono.OreGenerator.OnIsFocusedChanged += OnIsFocusedChanged;
                _mono.UpgradeManager.OnUpgradeUpdate += OnUpgradeUpdate;
                _itemsGrid.DrawPage(1);
                _filterGrid.DrawPage(1);
                UpdateDisplayValues();
                PowerOnDisplay();
                RecheckFilters();
                InvokeRepeating(nameof(Updater),0.5f,0.5f);
            }
        }

        private void Updater()
        {
            RefreshItemCount(_mono.DeepDrillerContainer.GetContainerTotal());
            RefreshItems();
            UpdateOilLevel();
            UpdateUnitID();
        }

        private void UpdateUnitID()
        {
            if (!string.IsNullOrWhiteSpace(_mono.FCSConnectableDevice.UnitID) && _unitID != null &&
                string.IsNullOrWhiteSpace(_unitID.text))
            {
                QuickLogger.Debug("Setting Unit ID",true);
                _unitID.text = $"UnitID: {_mono.FCSConnectableDevice.UnitID}";
            }
        }

        private void RefreshItemCount(int currentTotal = 0)
        {
            _itemCounter.text = $"{currentTotal} / {QPatch.Configuration.StorageSize}";
        }

        //private IEnumerable CheckForCraftingItems()
        //{
        //    foreach (var data in CraftData.techData)
        //    {
        //        bool passed = true;
        //        foreach (CraftData.Ingredient ingredient in data.Value._ingredients)
        //        {
        //            if (!_mono.DeepDrillerContainer.ContainsItem(ingredient.techType, ingredient.amount))
        //            {
        //                passed = false;
        //                goto _break;
        //            }
        //        }

        //        _break:

        //        if (passed && CraftTree.IsCraftable(data.Key))
        //        {
        //            _craftableItems.Add(data);
        //        }
        //    }

        //    yield return null;

        //}

        private void RecheckFilters()
        {
            foreach (TechType ore in _mono.OreGenerator.GetFocusedOres())
            {
                if (_trackedFilterState.ContainsKey(ore))
                {
                    _trackedFilterState[ore].isOn = true;
                }
            }

            _blackListToggle.isOn = _mono.OreGenerator.GetInBlackListMode();
        }

        internal void OnIsFocusedChanged(bool state)
        {
            if (state)
            {
                _filterBTNText.text = "ON";
                _filterBTNText.color = Color.green;
            }
            else
            {
                _filterBTNText.text = "OFF";
                _filterBTNText.color = Color.red;
            }
        }

        internal void UpdateExport(bool state)
        {
            if (state)
            {
                _exportToggleBTNText.text = "ON";
                _exportToggleBTNText.color = Color.green;
            }
            else
            {
                _exportToggleBTNText.text = "OFF";
                _exportToggleBTNText.color = Color.red;
            }
        }

        internal void UpdateDisplayValues()
        {
            _powerUsage.text = string.Format(FCSDeepDrillerBuildable.PowerUsageFormat(),_mono.DeepDrillerPowerManager.GetPowerUsage());
            _itemsPerDay.text = _mono.OreGenerator.GetItemsPerDay();
        }

        private void OnBatteryUpdate(PowercellData data)
        { 
            UpdateBatteryStatus(data);
        }

        public override void PowerOnDisplay()
        {
            QuickLogger.Debug("Powering On Display!",true);
            _mono.AnimationHandler.BootUp(_pageHash);
        }

        public override void PowerOffDisplay()
        {
            QuickLogger.Debug("Powering Off Display!", true);
            _mono.AnimationHandler.SetIntHash(_pageHash,6);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "PowerBTN":
                    _mono.DeepDrillerPowerManager.TogglePowerState();
                    break;
                case "SolarStateBTN":
                    _mono.DeepDrillerPowerManager.ToggleSolarState();
                    break;
                case "ItemsBTN":
                    GotoPage(FCSDeepDrillerPages.ItemsPage);
                    break;
                case "ProgrammingBTN":
                    QuickLogger.Debug("Color Picker", true);
                    GotoPage(FCSDeepDrillerPages.Programming);

                    break;
                case "AddProgramBTN":
                    QuickLogger.Debug("Opening Programming window!", true);
                    _mono.UpgradeManager.Show();
                    break;
                case "SettingsBTN":
                    GotoPage(FCSDeepDrillerPages.Settings);
                    break;
                case "MaintenanceBTN":
                    GotoPage(FCSDeepDrillerPages.Maintenance);
                    break;
                case "HomeBTN":
                    GotoPage(FCSDeepDrillerPages.Home);
                    break;
                case "SolarPanelBTN":
                    _mono.DeepDrillerPowerManager.ToggleSolarState();
                    break;
                case "RangeToggleBTN":
                    _mono.ToggleRangeView();
                    break;
                case "FilterBTN":
                    QuickLogger.Debug("Toggling Filter",true);
                    _mono.OreGenerator.ToggleFocus();
                    break;
                case "ExportToggleBTN":
                    QuickLogger.Debug("Export Toggle", true);
                    _mono.TransferManager.Toggle();
                    break;
                case "ColorPickerBTN":
                    QuickLogger.Debug("Color Picker", true);
                    GotoPage(FCSDeepDrillerPages.ColorPicker);
                    break;
                case "OilDumpBTN":
                    QuickLogger.Debug("Opening oil dump container", true);
                    _mono.OilDumpContainer.OpenStorage();
                    break;
                case "PowercellDumpBTN":
                    QuickLogger.Debug("Opening powercell dump container", true);
                    _mono.PowercellDumpContainer.OpenStorage();
                    break;
                case "ItemBTN":
                    var item = (TechType) tag;
                    _mono.DeepDrillerContainer.RemoveItemFromContainer(item);
                    break;
                case "FilterToggleBTN":
                    var data = (FilterBtnData) tag;
                    QuickLogger.Debug($"Toggle for {data.TechType} is {data.Toggle.isOn}",true);

                    if (data.Toggle.isOn)
                    {
                        _mono.OreGenerator.AddFocus(data.TechType);
                    }
                    else
                    {
                        _mono.OreGenerator.RemoveFocus(data.TechType);
                    }

                    break;
                case "ColorItem":
                    _mono.ColorManager.ChangeColorMask((Color)tag);
                    break;
            }
        }

        internal void UpdateToggleButtons()
        {
            if (_mono == null || _mono.DeepDrillerPowerManager == null) return;

            if (_solarPanelBtnIcon != null)
            {
                _solarPanelBtnIcon.color = _mono.DeepDrillerPowerManager.IsSolarExtended() ? Color.green : Color.red;
            }

            if (_rangeToggleBTNIcon != null)
            {
                _rangeToggleBTNIcon.color = _mono.GetIsRangeVisible() ? Color.green : Color.red;
            }
        }

        internal void GotoPage(FCSDeepDrillerPages page)
        {
            _mono.AnimationHandler.SetIntHash(_pageHash,(int)page);
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Canvas  
                var canvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

                if (canvasGameObject == null)
                {
                    QuickLogger.Error("Canvas cannot be found");
                    return false;
                }
                #endregion

                #region Home
                var homePage = InterfaceHelpers.FindGameObject(canvasGameObject, "HomePage");
                #endregion

                #region Color Picker
                var colorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPickerPage");
                #endregion

                #region Items Page
                var itemsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ItemsPage");
                #endregion

                #region Settings Page
                var settingsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "SettingsPage");
                #endregion

                #region Maintenance Page
                var maintenancePage = InterfaceHelpers.FindGameObject(canvasGameObject, "MaintenancePage");
                #endregion

                #region Powered Off Page
                var poweredOffPage = InterfaceHelpers.FindGameObject(canvasGameObject, "PoweredOffPage");
                #endregion

                #region Powered Off Page
                var programmingPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ProgrammingPage");
                #endregion


                //================= Home Page ================//


                #region Items Button

                var itemsBTN = GameObjectHelpers.FindGameObject(homePage, "ItemsBTN");
                InterfaceHelpers.CreateButton(itemsBTN, "ItemsBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ItemsButton());

                #endregion

                #region Maintenance Button

                var maintenanceBTN = GameObjectHelpers.FindGameObject(homePage, "MaintenanceBTN");
                InterfaceHelpers.CreateButton(maintenanceBTN, "MaintenanceBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.MaintenanceButton());

                #endregion

                #region Programming Button

                var programmingBTN = GameObjectHelpers.FindGameObject(homePage, "ProgrammingBTN");
                InterfaceHelpers.CreateButton(programmingBTN, "ProgrammingBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ProgrammingButton());

                #endregion

                #region Programming Button

                var settingsBTN = GameObjectHelpers.FindGameObject(homePage, "SettingsBTN");
                InterfaceHelpers.CreateButton(settingsBTN, "SettingsBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.SettingsButton());

                #endregion

                #region Power Button

                var hPowerBtn = GameObjectHelpers.FindGameObject(homePage, "PowerBTN");
                InterfaceHelpers.CreateButton(hPowerBtn, "PowerBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.PowerButton());

                #endregion

                #region Battery Meter

                var batteryMeter = GameObjectHelpers.FindGameObject(homePage, "battery");
                _batteryFill = batteryMeter?.FindChild("Fill")?.GetComponent<Image>();
                
                if (_batteryFill != null)
                {
                    _batteryFill.color = _colorEmpty;
                    _batteryFill.fillAmount = 0f;
                }

                _batteryPercentage = batteryMeter?.FindChild("Percentage")?.GetComponent<Text>();


                #endregion

                #region Oil Meter

                var oilMeter = GameObjectHelpers.FindGameObject(homePage, "Oil");
                _oilFill = oilMeter?.FindChild("Fill")?.GetComponent<Image>();

                if (_oilFill != null)
                {
                    _oilFill.color = _colorEmpty;
                    _oilFill.fillAmount = 0f;
                }

                #endregion

                #region Items Per Day
                _itemsPerDay = GameObjectHelpers.FindGameObject(homePage, "ItemsPerDayLBL")?.GetComponent<Text>();
                #endregion

                #region Power Usage

                _powerUsage = GameObjectHelpers.FindGameObject(homePage, "PowerUsageLBL")?.GetComponent<Text>();
                #endregion


                //================= Power Off Page ================//

                #region Power Button

                var pPowerBtn = GameObjectHelpers.FindGameObject(poweredOffPage, "PowerBTN");
                InterfaceHelpers.CreateButton(pPowerBtn, "PowerBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.PowerButton());

                #endregion

                //================= Items Page ================//

                #region Items Grid

                _itemsGrid = _mono.gameObject.AddComponent<GridHelper>();
                _itemsGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemsGrid.Setup(9, FCSDeepDrillerBuildable.ItemPrefab, itemsPage, _startColor, _hoverColor, OnButtonClick);

                #endregion

                _itemCounter = GameObjectHelpers.FindGameObject(itemsPage, "ItemsCounter")?.GetComponent<Text>();

                //================= Settings Page ================//

                #region Filter Grid

                _filterGrid = _mono.gameObject.AddComponent<GridHelper>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(4, FCSDeepDrillerBuildable.ListItemPrefab, settingsPage, _startColor, _hoverColor, OnButtonClick);

                #endregion

                #region Solar Panel Button

                var solarPanelBTN = GameObjectHelpers.FindGameObject(settingsPage, "SolarPanelBTN");
                _solarPanelBtnIcon = GameObjectHelpers.FindGameObject(solarPanelBTN, "Icon")?.GetComponent<Image>();
                InterfaceHelpers.CreateButton(solarPanelBTN, "SolarPanelBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.SolarButton());

                #endregion

                #region Filter Toggle Button

                var filterToggleBTN = GameObjectHelpers.FindGameObject(settingsPage, "FilterBTN");

                _filterBTNText = filterToggleBTN.GetComponentInChildren<Text>();


                InterfaceHelpers.CreateButton(filterToggleBTN, "FilterBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.FilterButton());

                #endregion


                #region Export Toggle Button

                var exportToggleBTN = GameObjectHelpers.FindGameObject(settingsPage, "ExportToggleBTN");

                _exportToggleBTNText = exportToggleBTN.GetComponentInChildren<Text>();
                
                InterfaceHelpers.CreateButton(exportToggleBTN, "ExportToggleBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ExportToggleButton());

                #endregion

                #region Blacklist Toggle

                _blackListToggle = GameObjectHelpers.FindGameObject(settingsPage, "Toggle").GetComponent<Toggle>();
                _blackListToggle.onValueChanged.AddListener(  (toggleState) =>
                {
                    _mono.OreGenerator.SetBlackListMode(toggleState);
                });
                #endregion

                #region Color Picker Button

                var colorPickerBTN = GameObjectHelpers.FindGameObject(settingsPage, "ColorPickerBTN");

                InterfaceHelpers.CreateButton(colorPickerBTN, "ColorPickerBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ColorButton());

                #endregion

                #region ColorPage
                _colorPage.SetupGrid(36, FCSDeepDrillerBuildable.ColorItemPrefab, colorPicker, OnButtonClick, _startColor, _hoverColor);
                #endregion

                #region Solar Panel Button

                var rangeToggleBTN = GameObjectHelpers.FindGameObject(settingsPage, "RangeToggleBTN");
                _rangeToggleBTNIcon = GameObjectHelpers.FindGameObject(rangeToggleBTN, "Icon")?.GetComponent<Image>();
                InterfaceHelpers.CreateButton(rangeToggleBTN, "RangeToggleBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.ToggleRangeButton());

                #endregion

                //================= Maintenance Page ================//

                #region Maintenance Home Button


                var maintenanceHomeBTN = GameObjectHelpers.FindGameObject(maintenancePage, "HomeBTN");
                InterfaceHelpers.CreateButton(maintenanceHomeBTN, "HomeBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.MaintenanceButton());

                #endregion

                #region Oil Dump Button

                var oilDumpBTN = GameObjectHelpers.FindGameObject(maintenancePage, "OilBTN");
                InterfaceHelpers.CreateButton(oilDumpBTN, "OilDumpBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AddOil());

                #endregion

                #region Powercell Dump Button

                var powercellDumpBTN = GameObjectHelpers.FindGameObject(maintenancePage, "PowerBTN");
                InterfaceHelpers.CreateButton(powercellDumpBTN, "PowercellDumpBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AddPower());

                #endregion

                //================= Programming Page ================//

                #region Programming Grid

                _programmingGrid = _mono.gameObject.AddComponent<GridHelper>();
                _programmingGrid.OnLoadDisplay += OnLoadProgrammingGrid;
                _programmingGrid.Setup(6, FCSDeepDrillerBuildable.ProgrammingItemPrefab, programmingPage, _startColor, _hoverColor, OnButtonClick);
                
                var addBTN = GameObjectHelpers.FindGameObject(programmingPage, "AddBTN");

                InterfaceHelpers.CreateButton(addBTN, "AddProgramBTN", InterfaceButtonMode.Background, OnButtonClick,
                    _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, FCSDeepDrillerBuildable.AddProgramButton());


                #endregion

                #region Find Unit

                _unitID = GameObjectHelpers.FindGameObject(homePage, "UnitID")?.GetComponent<Text>();

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
        
        private void OnUpgradeUpdate(UpgradeFunction obj)
        {
            QuickLogger.Debug("Refreshing the Upgrade Page",true);
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
                    
                    var deleteButton = GameObjectHelpers.FindGameObject(buttonPrefab, "DeleteBTN")?.GetComponent<Button>();
                    deleteButton?.onClick.AddListener(Test);
                    var function = grouped.ElementAt(i);
                    function.Label = upgradeText; 
                    deleteButton?.onClick.AddListener(() => {_mono.UpgradeManager.DeleteFunction(function);});

                    var activateButton = GameObjectHelpers.FindGameObject(buttonPrefab, "ActivationBTN")?.GetComponent<Button>();
                    activateButton?.onClick.AddListener(Test);
                    activateButton?.onClick.AddListener(() =>
                    {
                        function.ToggleUpdate();
                    });
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

        private void Test()
        {
            QuickLogger.Debug("Got it", true);
        }

        private void OnLoadFilterGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadFilterGrid : {data.ItemsGrid}", true);

                //_filterGrid.ClearPage();

                if (_trackedFilterState.Count <= 0)
                {
                    var grouped = _mono.OreGenerator.AllowedOres;
                    
                    for (int i = 0; i < grouped.Count; i++)
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
                        var toggle = buttonPrefab.GetComponent<Toggle>();
                        toggle.isOn = false;
                        buttonPrefab.SetActive(false);
                        var oreIndex = i;
                        toggle.onValueChanged.AddListener(delegate {
                            OnButtonClick("FilterToggleBTN", new FilterBtnData{TechType = grouped.ElementAt(oreIndex) , Toggle = toggle});
                        });

                        var itemName = buttonPrefab.GetComponentInChildren<Text>();
                        itemName.text = TechTypeExtensions.Get(Language.main, grouped.ElementAt(i));

                        if (_trackedFilterState.ContainsKey(grouped.ElementAt(i)))
                        {
                            toggle.isOn = _trackedFilterState[grouped.ElementAt(i)].isOn;
                        }
                        else
                        {
                            _trackedFilterState.Add(grouped.ElementAt(i), toggle);
                        }
                    }
                }

                foreach (KeyValuePair<TechType, Toggle> toggle in _trackedFilterState)
                {
                    toggle.Value.gameObject.SetActive(false);
                }

                var trackedFilterState = _trackedFilterState;

                if (data.EndPosition > trackedFilterState.Count)
                {
                    data.EndPosition = trackedFilterState.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _trackedFilterState.ElementAt(i).Value.gameObject.SetActive(true);
                }

                _filterGrid.UpdaterPaginator(_trackedFilterState.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                QuickLogger.Debug($"OnLoadBaseItemsGrid : {data.ItemsGrid}", true);

                _itemsGrid.ClearPage();


                var grouped = _mono.DeepDrillerContainer.GetItemsWithin();
                

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
                    var amount = buttonPrefab.GetComponentInChildren<Text>();
                    amount.text = grouped.ElementAt(i).Value.ToString();
                    var itemBTN = buttonPrefab.AddComponent<InterfaceButton>();
                    itemBTN.ButtonMode = InterfaceButtonMode.Background;
                    itemBTN.STARTING_COLOR = _startColor;
                    itemBTN.HOVER_COLOR = _hoverColor;
                    itemBTN.BtnName = "ItemBTN";
                    itemBTN.TextLineOne = string.Format(FCSDeepDrillerBuildable.TakeFormatted(), Language.main.Get(grouped.ElementAt(i).Key));
                    itemBTN.Tag = grouped.ElementAt(i).Key;
                    itemBTN.OnButtonClick = OnButtonClick;

                    uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    trashIcon.sprite = SpriteManager.Get(grouped.ElementAt(i).Key);
                }
                _itemsGrid.UpdaterPaginator(grouped.Count());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void UpdateBiome(string currentBiome)
        {
            if (!IsOperational) return;
            //TODO Change to the text component
            _currentBiome = currentBiome;
        }

        /// <summary>
        /// Updates the checked state of the focus items on the screen
        /// </summary>
        /// <param name="dataFocusOres"></param>
        internal void UpdateListItemsState(HashSet<TechType> dataFocusOres)
        {
            for (int dataFocusOresIndex = 0; dataFocusOresIndex < dataFocusOres.Count; dataFocusOresIndex++)
            {
                for (int trackedFilterItemsIndex = 0; trackedFilterItemsIndex < TrackedFilterItems.Count; trackedFilterItemsIndex++)
                {
                    var filterData = (FilterBtnData)TrackedFilterItems.ElementAt(trackedFilterItemsIndex).Tag;
                    if (filterData.TechType == dataFocusOres.ElementAt(dataFocusOresIndex))
                    {
                        filterData.Toggle.isOn = true;
                    }
                }
            }
        }

        internal void UpdateBatteryStatus(PowercellData data)
        {
            var charge = data.Charge < 1 ? 0f : data.Charge;

            var percent = charge / data.Capacity;

            if (_batteryFill != null)
            {
                if (data.Charge >= 0f)
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

            _batteryPercentage.text = ((data.Charge < 0f) ? Language.main.Get("ChargerSlotEmpty") : $"{Mathf.CeilToInt(percent * 100)}%");
        }

        internal void UpdateOilLevel()
        {
            var percent = _mono.OilHandler.GetOilPercent();
            _oilFill.fillAmount = percent;
            Color value = (percent >= 0.5f) ? Color.Lerp(this._colorHalf, this._colorFull, 2f * percent - 1f) : Color.Lerp(this._colorEmpty, this._colorHalf, 2f * percent);
            _oilFill.color = value;
        }

        internal void RefreshItems()
        {
            if (_itemsGrid != null)
            {
                QuickLogger.Debug("Refreshing Items",true);
                _itemsGrid.DrawPage();
            }
        }
    }

    internal struct FilterBtnData
    {
        public TechType TechType { get; set; }
        public Toggle Toggle { get; set; }
    }
}
