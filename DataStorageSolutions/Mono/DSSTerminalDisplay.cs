using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using DataStorageSolutions.Structs;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSTerminalDisplay : AIDisplay
    {
        private DSSTerminalController _mono;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private int _page;
        private GridHelper _baseGrid;
        private GridHelper _baseItemsGrid;
        private GridHelper _vehicleGrid;
        private GridHelper _vehicleContainersGrid;
        private BaseManager _currentBase;
        private TransferData _currentData;
        private ColorManager _terminalColorPage;
        private ColorManager _antennaColorPage;
        private GameObject _antennaColorPicker;
        private ColorPage _currentColorPage;
        private Text _baseNameLabel;
        private Text _gettingData;
        private string _currentSearchString;
        private Vehicle _currentVehicle;
        private Toggle _toggle;
        private GridHelper _itemGrid;
        private GridHelper _categoryGrid;
        private HashSet<Filter> _filters => FilterList.GetNewVersion(FilterList.GetFilters());
        private readonly List<InterfaceButton> _trackedButtons = new List<InterfaceButton>();
        private bool _isBeingDestroyed;
        private Text _counter;
        private ObjectPooler _objectPooler;
        
        private void OnLoadCategoryGrid(DisplayData data)
        {
            if (_isBeingDestroyed) return;

            _trackedButtons.Clear();
            _categoryGrid.ClearPage();

            var grouped = new List<Filter>();

            foreach (var filter in _filters)
            {
                if (filter.IsCategory())
                {
                    grouped.Add(filter);
                }
            }

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
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                CreateFilterButton(data, buttonPrefab, grouped, i);
            }
            _categoryGrid.UpdaterPaginator(grouped.Count);
            UpdateCheckMarks();
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            if (_isBeingDestroyed) return;

            _trackedButtons.Clear();

            _itemGrid.ClearPage();

            var grouped = new List<Filter>();

            QuickLogger.Debug("Load Filters");

            foreach (var filter in _filters)
            {
                if (!filter.IsCategory())
                {
                    grouped.Add(filter);
                }
            }

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
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }

                CreateFilterButton(data, buttonPrefab, grouped, i);
            }
            _itemGrid.UpdaterPaginator(grouped.Count);
            UpdateCheckMarks();
        }

        private void OnLoadVehicleGrid(DisplayData data)
        {
            try
            {
                if (_vehicleGrid == null) return;
                _vehicleGrid.ClearPage();

                var grouped = _mono?.Manager?.DockingManager?.Vehicles;

                if (grouped == null) return;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                if (data.ItemsGrid?.transform == null) return;

                QuickLogger.Debug($"Adding Vehicles");

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if (grouped[i] == null) continue;

                    GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        if (buttonPrefab != null)
                        {
                            QuickLogger.Debug("Destroying Tab", true);
                            Destroy(buttonPrefab);
                        }
                        return;
                    }

                    QuickLogger.Debug($"Creating Vehicle Button: {grouped[i].GetName()}");
                    CreateButton(data, buttonPrefab, new ButtonData { Vehicle = grouped[i] }, ButtonType.Vehicle, grouped[i].GetName(), "VehicleBTN");
                }

                _vehicleGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void UpdateCheckMarks()
        {
            var filters = QPatch.Configuration.Config.DockingBlackList;

            if (filters == null) return;

            foreach (InterfaceButton button in _trackedButtons)
            {
                foreach (Filter filter in filters)
                {
                    var curFilter = (FilterTransferData)button.Tag;

                    if (!curFilter.Filter.IsSame(filter)) continue;
                    curFilter.Toggle.isOn = true;
                    break;
                }

            }

        }

        private void OnLoadVehicleContainersGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed) return;
                _vehicleContainersGrid?.ClearPage();

                var grouped = DSSHelpers.GetVehicleContainers(_currentVehicle);

                if (grouped == null) return;

                QuickLogger.Debug($"Vehicle Count to process: {grouped.Count}");

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                if (data.ItemsGrid?.transform == null) return;

                QuickLogger.Debug($"Adding Vehicles");


                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if (grouped[i] == null) continue;

                    GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        if (buttonPrefab != null)
                        {
                            QuickLogger.Debug("Destroying Tab", true);
                            Destroy(buttonPrefab);
                        }
                        return;
                    }

                    CreateButton(data, buttonPrefab, new ButtonData { Container = grouped[i] }, ButtonType.Container, i.ToString(), "VehicleContainerBTN");
                }

                _vehicleContainersGrid?.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadBaseItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _objectPooler == null) return;

                QuickLogger.Debug($"OnLoadBaseItemsGrid : {data.ItemsGrid}", true);

                //_baseItemsGrid.ClearPage();
                _objectPooler.Reset(0);

                if (_currentBase == null || _mono.Manager.BaseOperators.Count <= 0) return;

                var grouped = _currentBase.StorageManager.GetItemsWithin().OrderBy(x => x.Key).ToList();

                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p.Key).StartsWith(_currentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                }

                QuickLogger.Debug($"Grouped Count = {grouped.Count} || Search: {_currentSearchString}", true);


                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {

                    GameObject buttonPrefab = _objectPooler.SpawnFromPool(0, data.ItemsGrid);

                    if (buttonPrefab == null || data.ItemsGrid == null)
                    {
                        return;
                    }

                    var itemBTN = buttonPrefab.EnsureComponent<ItemInterfaceButton>();
                    itemBTN.Manager = _currentBase;
                    itemBTN.TextComponent = buttonPrefab.GetComponentInChildren<Text>(); ;
                    itemBTN.ButtonMode = InterfaceButtonMode.Background;
                    itemBTN.STARTING_COLOR = _startColor;
                    itemBTN.HOVER_COLOR = _hoverColor;
                    itemBTN.BtnName = "ItemBTN";
                    itemBTN.Tag = grouped.ElementAt(i).Key;
                    itemBTN.OnButtonClick = OnButtonClick;
                }

                _baseItemsGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadBaseGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _baseGrid == null || _mono?.Manager == null) return;

                _baseGrid.ClearPage();

                var grouped = BaseManager.Managers;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                if (data.ItemsGrid?.transform == null)
                {
                    QuickLogger.Debug("Items Grid returned null canceling operation");
                    return;
                }

                if (_baseGrid.GetCurrentPage() < 2)
                {
                    if(_mono.Manager == null) { QuickLogger.Debug($"Manage is null trying to make button",true);}
                    CreateButton(data, Instantiate(data.ItemsPrefab), new ButtonData { Manager = _mono.Manager }, ButtonType.Home, AuxPatchers.Home(), "BaseBTN");
                    CreateButton(data, Instantiate(data.ItemsPrefab), new ButtonData(), ButtonType.None, AuxPatchers.GoToVehicles(), "VehiclesPageBTN");
                }

                // QuickLogger.Debug($"Bases Count: {grouped.Count}");
                //QuickLogger.Debug($"Bases Antenna: {_mono.Manager.GetCurrentBaseAntenna()}");

                if (_mono.Manager.GetCurrentBaseAntenna() != null || _mono.Manager.Habitat.isCyclops)
                {
                    for (int i = data.StartPosition; i < data.EndPosition; i++)
                    {
                        //QuickLogger.Debug($"Hab{grouped[i].Habitat} || AS{grouped[i].Habitat.gameObject.activeSelf} || TN{grouped[i].InstanceID == _mono.Manager.InstanceID} || HAN {grouped[i].HasAntenna()} || VIS {grouped[i].IsVisible} || Instance ID : {grouped[i].InstanceID}");
                        if (grouped[i].Habitat == null || !grouped[i].Habitat.gameObject.activeSelf || grouped[i].InstanceID == _mono.Manager.InstanceID || !grouped[i].HasAntenna() || !grouped[i].IsVisible) continue;
                        //QuickLogger.Debug($"Adding Base {grouped[i].InstanceID}");

                        GameObject buttonPrefab = Instantiate(data.ItemsPrefab);

                        if (buttonPrefab == null || data.ItemsGrid == null)
                        {
                            if (buttonPrefab != null)
                            {
                                QuickLogger.Debug("Destroying Tab", true);
                                Destroy(buttonPrefab);
                            }
                            return;
                        }

                        QuickLogger.Debug($"Adding Base: {grouped[i].GetBaseName()}", true);
                        CreateButton(data, buttonPrefab, new ButtonData { Manager = grouped[i] }, ButtonType.Base, grouped[i].GetBaseName().TruncateWEllipsis(30), "BaseBTN");
                    }
                }
                else
                {
                    QuickLogger.Debug("GetCurrentBaseAntenna most likely is null");
                }

                _baseGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void CreateButton(DisplayData data, GameObject buttonPrefab, ButtonData buttonData, ButtonType buttonType, string btnText, string btnName)
        {
            buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
            var mainBtn = buttonPrefab.AddComponent<DSSVehicleUIButton>();
            var text = buttonPrefab.GetComponentInChildren<Text>();
            text.text = btnText;
            mainBtn.ButtonMode = InterfaceButtonMode.Background;
            mainBtn.STARTING_COLOR = _startColor;
            mainBtn.HOVER_COLOR = _hoverColor;
            mainBtn.BtnName = btnName;
            mainBtn.OnButtonClick = OnButtonClick;

            switch (buttonType)
            {
                case ButtonType.Home:
                    mainBtn.TextLineOne = AuxPatchers.Home();
                    mainBtn.Tag = new TransferData { Manager = buttonData.Manager, ButtonType = buttonType };
                    break;
                case ButtonType.Item:
                    var amount = buttonPrefab.GetComponentInChildren<Text>();
                    amount.text = buttonData.Amount.ToString();
                    mainBtn.Tag = buttonData.TechType;
                    uGUI_Icon trashIcon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    trashIcon.sprite = SpriteManager.Get(buttonData.TechType);
                    break;
                case ButtonType.Vehicle:
                    mainBtn.TextLineOne = string.Format(AuxPatchers.ViewVehicleStorageFormat(), buttonData.Vehicle.GetName());
                    mainBtn.Tag = new TransferData { Vehicle = buttonData.Vehicle };
                    uGUI_Icon trashIcon2 = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    trashIcon2.sprite = SpriteManager.Get(buttonData.Vehicle is SeaMoth ? TechType.Seamoth : TechType.Exosuit);
                    mainBtn.Vehicle = buttonData.Vehicle;
                    mainBtn.Label = text;
                    break;
                case ButtonType.Container:
                    mainBtn.TextLineOne = "";
                    mainBtn.Tag = new TransferData { Container = buttonData.Container };
                    uGUI_Icon trashIcon3 = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    trashIcon3.sprite = SpriteManager.Get(TechType.VehicleStorageModule);
                    mainBtn.Label = text;
                    break;
                case ButtonType.Base:
                    mainBtn.TextLineOne = AuxPatchers.ViewBaseStorageFormat();
                    mainBtn.Tag = new TransferData { Manager = buttonData.Manager, ButtonType = buttonType };
                    break;
            }
        }

        private void CreateFilterButton(DisplayData data, GameObject buttonPrefab, List<Filter> grouped, int i, bool isMain = false)
        {
            buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
            buttonPrefab.GetComponentInChildren<Text>().text = grouped[i].GetString();

            if (!grouped[i].IsCategory())
            {
                var icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon");
                icon.AddComponent<uGUI_Icon>().sprite = SpriteManager.Get(grouped[i].Types[0]);
            }

            var buttonToggle = buttonPrefab.GetComponent<Toggle>();

            if (isMain)
            {
                buttonToggle.isOn = true;
            }

            var mainBTN = buttonPrefab.AddComponent<InterfaceButton>();
            mainBTN.ButtonMode = InterfaceButtonMode.Background;
            mainBTN.STARTING_COLOR = _startColor;
            mainBTN.HOVER_COLOR = _hoverColor;
            mainBTN.BtnName = "FilterBTN";
            mainBTN.Tag = new FilterTransferData { Filter = grouped[i], Toggle = buttonToggle };
            mainBTN.OnButtonClick = OnButtonClick;
            _trackedButtons.Add(mainBTN);
        }

        private void UpdateSearch(string newSearch)
        {
            _currentSearchString = newSearch;
            _baseItemsGrid.DrawPage();
        }

        private void UpdateCounterText()
        {
            if (_counter == null) return;
            _counter.text = _currentBase?.StorageManager?.GetTotalString() ?? "0 / 0";
        }

        internal void Setup(DSSTerminalController mono)
        {
            _mono = mono;

            _objectPooler = gameObject.AddComponent<ObjectPooler>();
            _objectPooler.Initialize();
            
            _terminalColorPage = mono.TerminalColorManager;

            if (FindAllComponents())
            {
                _page = Animator.StringToHash("TerminalPage");
                _baseGrid.DrawPage(1);
                PowerOnDisplay();
                InvokeRepeating(nameof(UpdateCounterText), 0.1f,0.1f);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "BaseBTN":
                    if (!SubrootCheck()) return;
                    _currentBase = ((TransferData)tag).Manager;
                    _currentData = (TransferData)tag;
                    _baseNameLabel.text = _currentBase.GetBaseName();

                    if (_currentData.ButtonType == ButtonType.Home)
                    {
                        GoToPage(TerminalPages.BaseItemsDirect);
                    }
                    else if (_currentData.ButtonType == ButtonType.Base)
                    {
                        _gettingData.text = string.Format(AuxPatchers.GettingData(), _currentBase.GetBaseName());
                        GoToPage(TerminalPages.BaseItems);
                    }

                    Refresh();
                    break;

                case "HomeBTN":
                    GoToPage(TerminalPages.Home);
                    break;

                case "ItemBTN":
                    _currentBase?.StorageManager.RemoveItemFromBase((TechType)tag);
                    break;

                case "DumpBTN":
                    _currentBase?.StorageManager.OpenDump(_currentData);
                    break;

                case "VehicleDumpBTN":
                    _vehicleContainersGrid.DrawPage();
                    GoToPage(TerminalPages.StorageContainer);
                    break;

                case "VehicleContainerBTN":
                    _mono.Manager.DockingManager.OpenContainer(_currentVehicle, ((TransferData)tag).Container);
                    break;

                case "TerminalColorBTN":
                    GoToPage(TerminalPages.TerminalColorPage);
                    _currentColorPage = ColorPage.Terminal;
                    break;

                case "AntennaColorBTN":
                    var antennas = _mono.Manager.GetCurrentBaseAntenna();

                    if (antennas != null)
                    {
                        GoToPage(TerminalPages.AntennaColorPage);
                        _currentColorPage = ColorPage.Antenna;
                        UpdateAntennaColorPage();
                    }
                    else if (_mono.Manager.Habitat.isBase)
                    {
                        QuickLogger.Message(AuxPatchers.NoAntennaOnBase(), true);
                    }
                    else if (_mono.Manager.Habitat.isCyclops)
                    {
                        QuickLogger.Message(AuxPatchers.CannotChangeCyclopsAntenna(), true);
                    }
                    break;

                case "ColorPickerBTN":
                    GoToPage(TerminalPages.ColorPageMain);
                    break;

                case "ColorItem":
                    if (_currentColorPage == ColorPage.Terminal)
                        _mono.TerminalColorManager.ChangeColorMask((Color)tag);
                    else
                        ChangeAntennaColor((Color)tag);
                    break;

                case "RenameBTN":
                    _mono.Manager.ChangeBaseName();
                    break;

                case "PowerBTN":
                    _mono.Manager.ToggleBreaker();
                    break;

                case "SettingsBTN":
                    GoToPage(TerminalPages.SettingsPage);
                    break;

                case "VehiclesPageBTN":
                    if (_mono.Manager.DockingManager.HasVehicles(true))
                    {
                        _vehicleGrid.DrawPage();
                        GoToPage(TerminalPages.VehiclesPage);
                    }
                    else
                    {
                        GoToPage(TerminalPages.Home);
                    }
                    break;

                case "VehicleBTN":
                    _currentVehicle = ((TransferData)tag).Vehicle;
                    _vehicleContainersGrid.DrawPage();
                    GoToPage(TerminalPages.StorageContainer);
                    break;

                case "AutoDockBTN":
                    _toggle.isOn = _mono.Manager.DockingManager.GetToggleState();
                    GoToPage(TerminalPages.DockSettingPage);
                    break;

                case "FilterBTN":
                    var data = (FilterTransferData)tag;
                    if (Mod.IsFilterAdded(data.Filter))
                    {
                        Mod.RemoveBlackListFilter(data.Filter);
                        return;
                    }
                    Mod.AddBlackListFilter(data.Filter);
                    break;
            }
        }

        private void ChangeAntennaColor(Color color)
        {
            foreach (var baseAntenna in _mono.Manager.GetBaseAntennas())
            {
                baseAntenna.ChangeColorMask(color);
            }
        }

        public override void PowerOnDisplay()
        {
            _mono.AnimationManager.SetIntHash(_page, 1);
        }

        public override void PowerOffDisplay()
        {
            if (_mono.Manager.BasePowerManager.GetPowerState() == FCSPowerStates.Unpowered)
            {
                GoToPage(TerminalPages.BlackOut);
            }
            else if (_mono.Manager.BasePowerManager.GetPowerState() == FCSPowerStates.Tripped)
            {
                GoToPage(TerminalPages.Tripped);
            }
        }

        public void Refresh()
        {
            if (_isBeingDestroyed) return;
            _baseGrid?.DrawPage();
            _baseItemsGrid?.DrawPage();
            if (_baseNameLabel != null)
            {
                _baseNameLabel.text = _currentBase?.GetBaseName() ?? AuxPatchers.FailedToGetBaseName();
            }
        }

        internal void GoToPage(TerminalPages page)
        {
            _mono.AnimationManager.SetIntHash(_page, (int)page);
        }

        internal bool SubrootCheck()
        {
            if (_mono.Manager != null) return true;
            QuickLogger.Error("Terminal cant find the base please contact FCS", true);
            return false;

        }

        internal void UpdateAntennaColorPage()
        {
            _antennaColorPage = _mono?.Manager?.GetCurrentBaseAntenna()?.ColorManager;

            if (_antennaColorPage != null)
            {
                #region ColorPage
                _antennaColorPage.SetupGrid(90, DSSModelPrefab.ColorItemPrefab, _antennaColorPicker, OnButtonClick, _startColor, _hoverColor, 5
                , "PrevBTN", "NextBTN", "Grid", "Paginator", "HomeBTN", "ColorPickerBTN");
                #endregion
            }
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
                var home = InterfaceHelpers.FindGameObject(canvasGameObject, "Home");
                #endregion

                #region VehiclesPage
                var vehiclesPage = InterfaceHelpers.FindGameObject(canvasGameObject, "VehiclesPage");
                #endregion

                #region VehicleDockingSettingsPage
                var vehiclesDockingSettingsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "VehicleDockingSettingsPage");
                #endregion

                #region Item Page
                var itemPage = InterfaceHelpers.FindGameObject(vehiclesDockingSettingsPage, "ItemPage");
                #endregion

                #region Category Page
                var categoryPage = InterfaceHelpers.FindGameObject(vehiclesDockingSettingsPage, "CategoryPage");
                #endregion

                #region VehiclesContainersPage
                var vehiclesContainersPage = InterfaceHelpers.FindGameObject(canvasGameObject, "VehiclesContainersPage");
                #endregion

                #region BaseItemsPage
                var baseItemsPage = InterfaceHelpers.FindGameObject(canvasGameObject, "BaseItemsPage");
                #endregion

                #region GettingDataPage
                var gettingDataPage = InterfaceHelpers.FindGameObject(canvasGameObject, "GettingData");
                #endregion

                #region Settings
                var settings = InterfaceHelpers.FindGameObject(canvasGameObject, "SettingsPage");
                #endregion

                #region PoweredOff
                var poweredOff = InterfaceHelpers.FindGameObject(canvasGameObject, "PowerOffPage");
                #endregion

                #region ColorPageMain
                var colorMainPage = InterfaceHelpers.FindGameObject(canvasGameObject, "ColorPageMain");
                #endregion

                #region ScreenColorPicker

                var screenColorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "TerminalColorPage");

                #endregion

                #region AntennnaColorPicker

                _antennaColorPicker = InterfaceHelpers.FindGameObject(canvasGameObject, "AntennaColorPage");

                #endregion

                #region Base Grid

                _baseGrid = _mono.gameObject.AddComponent<GridHelper>();
                _baseGrid.OnLoadDisplay += OnLoadBaseGrid;
                _baseGrid.Setup(12, DSSModelPrefab.BaseItemPrefab, home, _startColor, _hoverColor, OnButtonClick); //Minus 2 ItemPerPage because of the added Home button

                #endregion

                #region Vehicle Grid

                _vehicleGrid = _mono.gameObject.AddComponent<GridHelper>();
                _vehicleGrid.OnLoadDisplay += OnLoadVehicleGrid;
                _vehicleGrid.Setup(12, DSSModelPrefab.VehicleItemPrefab, vehiclesPage, _startColor, _hoverColor, OnButtonClick); //Minus 1 ItemPerPage because of the added Home button

                #endregion

                #region Vehicle Containers Grid

                _vehicleContainersGrid = _mono.gameObject.AddComponent<GridHelper>();
                _vehicleContainersGrid.OnLoadDisplay += OnLoadVehicleContainersGrid;
                _vehicleContainersGrid.Setup(12, DSSModelPrefab.VehicleItemPrefab, vehiclesContainersPage, _startColor, _hoverColor, OnButtonClick, 5,
                    "PrevBTN", "NextBTN", "Grid", "Paginator", "HomeBTN", "VehiclesPageBTN"); //Minus 1 ItemPerPage because of the added Home button

                #endregion

                #region Item Grid

                _itemGrid = _mono.gameObject.AddComponent<GridHelper>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(4, DSSModelPrefab.FilterItemPrefab, itemPage, _startColor, _hoverColor, OnButtonClick); //Minus 1 ItemPerPage because of the added Home button

                #endregion

                #region Category Grid

                _categoryGrid = _mono.gameObject.AddComponent<GridHelper>();
                _categoryGrid.OnLoadDisplay += OnLoadCategoryGrid;
                _categoryGrid.Setup(4, DSSModelPrefab.FilterItemPrefab, categoryPage, _startColor, _hoverColor, OnButtonClick); //Minus 1 ItemPerPage because of the added Home button

                #endregion

                #region Base Items Page

                _baseItemsGrid = _mono.gameObject.AddComponent<GridHelper>();
                _baseItemsGrid.OnLoadDisplay += OnLoadBaseItemsGrid;
                _baseItemsGrid.Setup(44, DSSModelPrefab.ItemPrefab, baseItemsPage, _startColor, _hoverColor, OnButtonClick);

                _counter = GameObjectHelpers.FindGameObject(baseItemsPage, "Counter").GetComponent<Text>();
                #endregion

                #region DumpBTNButton
                var closeBTN = InterfaceHelpers.FindGameObject(baseItemsPage, "DumpButton");

                InterfaceHelpers.CreateButton(closeBTN, "DumpBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.DumpToBase());
                #endregion

                #region ColorPickerBTN
                var colorPickerBTN = InterfaceHelpers.FindGameObject(settings, "ColorPickerBTN");

                InterfaceHelpers.CreateButton(colorPickerBTN, "ColorPickerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.ColorPage());
                #endregion

                #region RenameBTN
                var renameBTN = InterfaceHelpers.FindGameObject(settings, "RenameBaseBTN");

                InterfaceHelpers.CreateButton(renameBTN, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.Rename());
                #endregion

                #region SettingsBTN
                var settingsBTN = InterfaceHelpers.FindGameObject(home, "SettingsBTN");

                InterfaceHelpers.CreateButton(settingsBTN, "SettingsBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.SettingPage());
                #endregion

                #region HomePowerBTN
                var homePowerBTN = InterfaceHelpers.FindGameObject(home, "PowerBTN");

                InterfaceHelpers.CreateButton(homePowerBTN, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.PowerButton());
                #endregion

                #region PoweredOffPowerBTN
                var poweredOffPowerBTN = InterfaceHelpers.FindGameObject(poweredOff, "PowerBTN");

                InterfaceHelpers.CreateButton(poweredOffPowerBTN, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.PowerButton());
                #endregion

                #region ColorPickerMainHomeBTN
                var colorPickerMainHomeBTN = InterfaceHelpers.FindGameObject(colorMainPage, "HomeBTN");

                InterfaceHelpers.CreateButton(colorPickerMainHomeBTN, "SettingsBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.SettingPage());
                #endregion

                #region SettingHomeBTN
                var settingsHomeBTN = InterfaceHelpers.FindGameObject(settings, "HomeBTN");

                InterfaceHelpers.CreateButton(settingsHomeBTN, "HomeBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.GoToHome());
                #endregion

                #region Terminal Color BTN
                var terminalColorBTN = InterfaceHelpers.FindGameObject(colorMainPage, "TerminalColorBTN");

                InterfaceHelpers.CreateButton(terminalColorBTN, "TerminalColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, string.Format(AuxPatchers.ColorPageFormat(), AuxPatchers.Terminal()));
                #endregion

                #region VehicleSettingsBTN
                var vehicleSettingBTN = InterfaceHelpers.FindGameObject(settings, "AutoDockBTN");

                InterfaceHelpers.CreateButton(vehicleSettingBTN, "AutoDockBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.GoToDockedVehiclesSettings());
                #endregion

                #region Toggle

                _toggle = InterfaceHelpers.FindGameObject(vehiclesDockingSettingsPage, "Toggle").GetComponent<Toggle>();
                _toggle.onValueChanged.AddListener(value =>
                {
                    QuickLogger.Debug($"Setting Toggle to {value}", true);
                    _mono?.Manager?.DockingManager?.ToggleIsEnabled(value);
                });

                #endregion

                #region Search
                var inputField = InterfaceHelpers.FindGameObject(baseItemsPage, "InputField");

                if (inputField != null)
                {
                    var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
                    if (text != null)
                    {
                        text.text = AuxPatchers.SearchForItemsMessage();
                    }
                    else
                    {
                        return false;
                    }

                    var searchField = inputField.AddComponent<SearchField>();
                    searchField.OnSearchValueChanged += UpdateSearch;
                }
                else
                {
                    //throw new MissingComponentException("Cannot find Input Field");
                    return false;
                }

                #endregion

                #region Antenna Color BTN
                var antennaColorBTN = InterfaceHelpers.FindGameObject(colorMainPage, "AntennaColorBTN");

                InterfaceHelpers.CreateButton(antennaColorBTN, "AntennaColorBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, string.Format(AuxPatchers.ColorPageFormat(), AuxPatchers.Antenna()));
                #endregion

                #region ColorPage
                _terminalColorPage.SetupGrid(90, DSSModelPrefab.ColorItemPrefab, screenColorPicker, OnButtonClick, _startColor, _hoverColor, 5,
                    "PrevBTN", "NextBTN", "Grid", "Paginator", "HomeBTN", "ColorPickerBTN");
                #endregion

                #region BaseItemDecription

                var baseItemPageDesc = InterfaceHelpers.FindGameObject(colorMainPage, "Title")?.GetComponent<Text>();
                baseItemPageDesc.text = AuxPatchers.ColorMainPageDesc();

                #endregion

                #region BaseName

                _baseNameLabel = InterfaceHelpers.FindGameObject(baseItemsPage, "BaseLabel")?.GetComponent<Text>();

                #endregion

                #region BaseItemsLoading

                _gettingData = InterfaceHelpers.FindGameObject(gettingDataPage, "Title")?.GetComponent<Text>();

                #endregion

                #region DockSettingsHomeBTN
                var dockSettingsHomeBTN = InterfaceHelpers.FindGameObject(vehiclesDockingSettingsPage, "HomeBTN");

                InterfaceHelpers.CreateButton(dockSettingsHomeBTN, "SettingsBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE, AuxPatchers.SettingPage());
                #endregion

                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }
        }

        public void RefreshVehicles()
        {
            if (_isBeingDestroyed) return;

            QuickLogger.Debug("Refreshing Vehicles");
            
            if (!_mono.Manager.DockingManager.IsVehicleDocked(_currentVehicle))
            {
                _currentVehicle = null;
                _vehicleGrid.DrawPage();
                if (_mono.AnimationManager.GetIntHash(_page) != (int)TerminalPages.Home)
                {
                    GoToPage(TerminalPages.VehiclesPage);
                }
            }
        }

        public void RefreshVehicleItems()
        {
            if (_isBeingDestroyed) return;

            _vehicleContainersGrid?.DrawPage();
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        public void ChangeScreenPowerState(FCSPowerStates state)
        {
            if (state == FCSPowerStates.Powered)
            {
                PowerOnDisplay();
            }

            PowerOffDisplay();
        }
    }
}