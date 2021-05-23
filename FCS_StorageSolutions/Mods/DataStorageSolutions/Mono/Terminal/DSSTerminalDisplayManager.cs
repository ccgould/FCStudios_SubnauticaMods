using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DSSTerminalDisplayManager : AIDisplay
    {
        #region Private Fields

        private DSSTerminalController _mono;
        private GridHelperV2 _itemGrid;
        private bool _isBeingDestroyed;
        private readonly List<DSSInventoryItem> _inventoryButtons = new List<DSSInventoryItem>();
        private Text _baseName;
        private Text _serverAmount;
        private Text _rackCountAmount;
        private Text _totalItemsAmount;
        private BaseManager _currentBase;
        private string _currentSearchString;
        private StorageType _storageFilter;
        private GameObject _homeObj;
        private Canvas _canvas;
        private NetworkDialogController _networkBTN;
        private GameObject _powerOffScreen;
        private Text _currentBaseLBL;
        private GameObject _screen;
        private StringBuilder _sb = new StringBuilder();
        private int _serverCapacity;
        private int _alterraStorageCapacity;
        private PaginatorController _paginatorController;
        private TransceiverPageController _itemTransceiverPage;
        private DeviceTransceiverDialog _deviceTransceiverDialog;
        private TerminalConfigurationPageController _terminalConfigurationPage;
        private MoonPoolPageController _moonPoolController;
        private FilterSettingDialog _filterSettingList;
        private CraftingPageController _craftingController;
        private DumpContainerSimplified _dumpContainer;
        private FCSMessageBox _messageBox;

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        #endregion
        
        private void OnCurrentSubRootChanged(SubRoot parms)
        {
            _networkBTN.Refresh(parms);
        }
        
        private void UpdateDisplay()
        {
            if (_currentBase == null || Player.main.currentSub == null) return;
            _baseName.text = $"{_currentBase?.GetBaseName()} - {_currentBase.GetBaseFriendlyId()}";
            _rackCountAmount.text = AuxPatchers.RackCountFormat(_currentBase?.BaseRacks.Count ?? 0);
            _serverAmount.text = AuxPatchers.ServerCountFormat(_currentBase?.BaseServers.Count ?? 0);
            _currentBaseLBL.text = Player.main.currentSub == _currentBase.Habitat ? AuxPatchers.CurrentBase() : AuxPatchers.RemoteBase();
            _serverCapacity = _currentBase?.BaseServers.Count * 48 ?? 0;
            _alterraStorageCapacity = _currentBase?.BaseFcsStorage.Sum(x => x.GetMaxStorage()) ?? 0;

            switch (_storageFilter)
            {
                case StorageType.All:
                case StorageType.Servers:
                    _totalItemsAmount.text = AuxPatchers.TotalItemsFormat(_currentBase.GetTotal(StorageType.Servers), _serverCapacity);
                    break;
                case StorageType.StorageLockers:
                    _totalItemsAmount.text = AuxPatchers.TotalItemsFormat(_currentBase.GetTotal(_storageFilter), 0);
                    break;
                case StorageType.OtherStorage:
                    _totalItemsAmount.text = AuxPatchers.TotalItemsFormat(_currentBase.GetTotal(_storageFilter), _alterraStorageCapacity);
                    break;
            }

            _moonPoolController.RefreshVehicleName();
            
        }

        internal void Setup(DSSTerminalController mono)
        {
            _mono = mono;

            if (FindAllComponents())
            {
                _currentBase = _mono.Manager;
                _screen.SetActive(true);
                Player.main.currentSubChangedEvent.AddHandler(base.gameObject, OnCurrentSubRootChanged);
                InvokeRepeating(nameof(UpdateDisplay), .5f, .5f);
                GoToTerminalPage(TerminalPages.Home);
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "BaseDump":
                    _currentBase.OpenBaseStorage();
                    break;
                case "InventoryBTN":
                    var techType = (TechType)tag;
                    var amount = (int)_mono.BulkMultiplier;
                    for (int i = 0; i < amount; i++)
                    {
                        if (!PlayerInteractionHelper.CanPlayerHold(techType))
                        {
                            QuickLogger.Message(AlterraHub.InventoryFull());
                            continue;
                        }
                        var result = _currentBase.TakeItem(techType, _storageFilter);
                        if (result != null)
                        {
                            UpdateDisplay();
                            PlayerInteractionHelper.GivePlayerItem(result);
                        }
                    }
                    break;
                case "RenameBTN":
                    _currentBase.ChangeBaseName();
                    break;
                case "BaseBTN":
                    _currentBase = (BaseManager)tag;
                    Refresh();
                    break;
                case "ConfigurationToggleBTN":
                    GoToTerminalPage(TerminalPages.Home);
                    break;
                case "PowerBTN":
                    _mono.Manager.ToggleBreaker();
                    _networkBTN.Refresh(null);
                    break;
            }
        }

        public override void PowerOnDisplay()
        {
            _powerOffScreen.SetActive(false);
            _moonPoolController.Hide();
            _homeObj.SetActive(true);
        }

        public override void HibernateDisplay()
        {
            HideAllPages();
            _powerOffScreen.SetActive(true);
        }
        
        internal void Refresh()
        {
            _itemGrid?.DrawPage();
            _itemTransceiverPage?.RefreshList();
        }

        internal void RefreshCraftingGrid()
        {
            _craftingController?.Refresh();
        }

        public override bool FindAllComponents()
        {
            try
            {
                foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
                {
                    var invButton = invItem.gameObject.EnsureComponent<DSSInventoryItem>();
                    invButton.ButtonMode = InterfaceButtonMode.HoverImage;
                    invButton.BtnName = "InventoryBTN";
                    invButton.OnButtonClick += OnButtonClick;
                    _inventoryButtons.Add(invButton);
                }
                
                _canvas = gameObject.GetComponentInChildren<Canvas>(true);
                _screen = _canvas.gameObject;
                _homeObj = GameObjectHelpers.FindGameObject(gameObject, "Home");
                _deviceTransceiverDialog = GameObjectHelpers.FindGameObject(gameObject, "DeviceTransceiverDialog").AddComponent<DeviceTransceiverDialog>();
                _deviceTransceiverDialog.Initialize();
                _itemTransceiverPage = GameObjectHelpers.FindGameObject(gameObject, "TransceiverSettings").AddComponent<TransceiverPageController>();
                _terminalConfigurationPage = GameObjectHelpers.FindGameObject(gameObject, "Configuration").AddComponent<TerminalConfigurationPageController>();
                _terminalConfigurationPage.Initialize(this);
                _itemTransceiverPage.Initialize(this);
                
                var screenToggleObj = GameObjectHelpers.FindGameObject(gameObject, "ServerToggleBTN").AddComponent<FCSButton>();
                screenToggleObj.TextLineOne = "Change screen";
                screenToggleObj.Subscribe((() =>
                {
                    GoToTerminalPage(TerminalPages.Configuration);
                }));

                //InterfaceHelpers.CreateButton(screenToggleObj, "ScreenToggleBTN", InterfaceButtonMode.Background, OnButtonClick, Color.gray, Color.white, 2.4f, "Change screen");

                var configurationToggleBTN = GameObjectHelpers.FindGameObject(gameObject, "ConfigurationToggleBTN");
                InterfaceHelpers.CreateButton(configurationToggleBTN, "ConfigurationToggleBTN", InterfaceButtonMode.Background, OnButtonClick, Color.gray, Color.white, 2.4f, "Change screen");

                var addToBaseBTNObj = InterfaceHelpers.FindGameObject(gameObject, "AddToBaseBTN");
                InterfaceHelpers.CreateButton(addToBaseBTNObj, "BaseDump", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.AddItemToNetwork(), AuxPatchers.AddItemToNetworkDesc());

                var renameBTNObj = GameObjectHelpers.FindGameObject(gameObject, "RenameBTN");
                InterfaceHelpers.CreateButton(renameBTNObj, "RenameBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.Rename(),
                    AuxPatchers.RenameDesc());

                _networkBTN = GameObjectHelpers.FindGameObject(gameObject, "NetworkBTN").EnsureComponent<NetworkDialogController>();
                _networkBTN.Initialize(_mono.Manager, gameObject, this);

                var multiplierBTN = GameObjectHelpers.FindGameObject(gameObject, "MultiplyBTN").EnsureComponent<MultiplierController>();
                multiplierBTN.Initialize(_mono);
                multiplierBTN.UpdateLabel();
                
                _itemGrid = _mono.gameObject.EnsureComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(44, gameObject, Color.gray, Color.white, OnButtonClick);

                #region Search
                var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
                var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
                text.text = AuxPatchers.SearchForItemsMessage();

                var searchField = inputField.AddComponent<SearchField>();
                searchField.OnSearchValueChanged += UpdateSearch;
                #endregion


                _filterSettingList = InterfaceHelpers.FindGameObject(gameObject, "ShowAllBTN").AddComponent<FilterSettingDialog>();
                _filterSettingList.Initialize(gameObject, this);
                _filterSettingList.OnButtonClick += OnButtonClick;
                _filterSettingList.STARTING_COLOR = Color.white;
                _filterSettingList.HOVER_COLOR = new Color(0.5471698f, 0.5471698f, 0.5471698f, 1);

                _powerOffScreen = GameObjectHelpers.FindGameObject(gameObject, "PowerOffScreen");

                _moonPoolController = GameObjectHelpers.FindGameObject(gameObject, "MoonPool").AddComponent<MoonPoolPageController>();
                _moonPoolController.Initialize(this);
                
                _craftingController = GameObjectHelpers.FindGameObject(gameObject, "Crafting").AddComponent<CraftingPageController>();
                _craftingController.Initialize(this);

                var powerButton = GameObjectHelpers.FindGameObject(_powerOffScreen, "PowerBTN");
                InterfaceHelpers.CreateButton(powerButton, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.PowerOnOff());

                var powerHButton = GameObjectHelpers.FindGameObject(gameObject, "PowerBTN");
                InterfaceHelpers.CreateButton(powerHButton, "PowerBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.PowerOnOff());

                _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName").GetComponent<Text>();
                _serverAmount = GameObjectHelpers.FindGameObject(gameObject, "ServerCount").GetComponent<Text>();
                _rackCountAmount = GameObjectHelpers.FindGameObject(gameObject, "RackCount").GetComponent<Text>();
                var totalItemsLbl = GameObjectHelpers.FindGameObject(gameObject, "TotalItems");
                var information = GameObjectHelpers.FindGameObject(gameObject, "InformationIcon");
                
                var toolTip = information.AddComponent<FCSToolTip>();
                toolTip.RequestPermission += () => true;
                toolTip.ToolTipStringDelegate += ToolTipStringDelegate;
                _totalItemsAmount = totalItemsLbl.GetComponent<Text>();
                _currentBaseLBL = GameObjectHelpers.FindGameObject(gameObject, "CurrentBaseLBL").GetComponent<Text>();

                _messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox").AddComponent<FCSMessageBox>();

                _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
                _paginatorController.Initialize(this);

            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.Source);
                return false;
            }

            return true;
        }

        private string ToolTipStringDelegate()
        {
            _sb.Clear();
            _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color></size>", "Additional Storage Information");
            _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color> <color=#DDDEDEFF>{1}</color></size>", "Storage Lockers",$"{_currentBase.GetTotal(StorageType.StorageLockers)}");
            _sb.AppendFormat("\n<size=20><color=#FFA500FF>{0}:</color> <color=#DDDEDEFF>{1}</color></size>", Mod.AlterraStorageFriendlyName,$"{_currentBase.GetTotal(StorageType.OtherStorage)}/{_alterraStorageCapacity}");
            return _sb.ToString();
        }

        private void UpdateSearch(string newSearch)
        {
            _currentSearchString = newSearch;
            _itemGrid.DrawPage();
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _currentBase == null) return;

                var grouped = _currentBase.GetItemsWithin(_storageFilter).OrderBy(x => x.Key).ToList();

                if (!string.IsNullOrEmpty(_currentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p.Key).ToLower().Contains(_currentSearchString.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

                _itemGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_itemGrid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        internal void ChangeStorageFilter(StorageType storage)
        {
            _storageFilter = storage;
            _itemGrid.DrawPage();
        }

        internal void ShowVehicleContainers(Vehicle vehicle)
        {
            _moonPoolController.ShowVehicleContainers(vehicle);
        }
        
        public override void TurnOffDisplay()
        {
            _screen?.SetActive(false);
        }

        public override void TurnOnDisplay()
        {
            _screen?.SetActive(true);
        }

        internal DSSTerminalController GetController()
        {
            return _mono;
        }

        public override void GoToPage(int index)
        {
            _itemGrid.DrawPage(index);
        }

        /// <summary>
        /// Changes the terminal page to the transceiver page and uses the <see cref="DSSListItemController"/>
        /// to receive the manager data for screen population.
        /// </summary>
        /// <param name="item"></param>
        internal void OpenItemTransceiverPage(DSSListItemController item)
        {
            if (item == null) return;
            GoToTerminalPage(TerminalPages.Transceiver,item);
        }

        /// <summary>
        /// Changes the current page of the terminal
        /// </summary>
        /// <param name="page">The page to go to</param>
        /// <param name="obj">The object to pass with the page switch is <code>null</code> by default</param>
        internal void GoToTerminalPage(TerminalPages page,object obj = null)
        {
            switch (page)
            {
                case TerminalPages.Home:
                    HideAllPages();
                    _homeObj.SetActive(true);
                    break;
                
                case TerminalPages.MoonPoolSettings:
                    HideAllPages();
                    _moonPoolController.Show();
                    break;
                case TerminalPages.Transceiver:
                    HideAllPages();
                    _itemTransceiverPage.Show((DSSListItemController)obj);
                    break;
                case TerminalPages.Configuration:
                    HideAllPages();
                    _terminalConfigurationPage.Show();
                    break;
                case TerminalPages.Crafting:
                    HideAllPages();
                    _craftingController.Show();
                    break;
            }

            //Always should hide these when not focused
            _filterSettingList.Hide();
            _networkBTN.Hide();
        }

        internal void HideAllPages()
        {
            _itemTransceiverPage.Hide();
            _moonPoolController.Hide();
            _terminalConfigurationPage.Hide();
            _homeObj.SetActive(false);
            _filterSettingList.Hide();
            _networkBTN.Hide();
        }

        public void OpenTransceiverDialog(FcsDevice fcsDevice)
        {
            _deviceTransceiverDialog.Show(fcsDevice);
        }

        public void ShowMessage(string message)
        {
            _messageBox.Show(message,FCSMessageButton.OK,null);
        }
    }
}