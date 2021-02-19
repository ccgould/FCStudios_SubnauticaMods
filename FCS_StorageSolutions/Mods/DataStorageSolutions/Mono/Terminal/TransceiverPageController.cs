using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class TransceiverPageController : MonoBehaviour, IFCSDisplay
    {
        private DSSTerminalDisplayManager _displayManager;
        private DSSListItemController _itemController;
        private readonly List<DeviceItem> _deviceButtons = new List<DeviceItem>();
        private bool _isInitialized;
        private InputField _inputField;
        private string _searchText;
        private Text _totalOperations;
        private GridHelperV2 _operationsGrid;
        private PaginatorController _paginatorController;
        private Text _baseName;


        private void UpdateSearch(string obj)
        {
            _searchText = obj;
            _operationsGrid.DrawPage();
        }

        /// <summary>
        /// Initializes the page
        /// </summary>
        /// <param name="displayManager"></param>
        internal void Initialize(DSSTerminalDisplayManager displayManager)
        {
            if (_isInitialized) return;

            _displayManager = displayManager;

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
            var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
            text.text = AuxPatchers.SearchForItemsMessage();

            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            #region Total Operations Label
            _totalOperations = GameObjectHelpers.FindGameObject(gameObject, "TotalOperations")?.GetComponent<Text>();
            #endregion

            #region Back Button

            var backBtn = GameObjectHelpers.FindGameObject(gameObject, "BackBTN")?.GetComponent<Button>();
            backBtn?.onClick.AddListener(() => { _displayManager.GoToTerminalPage(TerminalPages.Home); });

            #endregion

            #region Grid

            _operationsGrid = gameObject.EnsureComponent<GridHelperV2>();
            _operationsGrid.OnLoadDisplay += OnTransceiverGrid;
            _operationsGrid.Setup(33, gameObject, Color.gray, Color.white, OnButtonClick);

            #endregion

            #region TransceiverBTN

            foreach (Transform deviceItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var deviceBtn = deviceItem.gameObject.EnsureComponent<DeviceItem>();
                deviceBtn.ButtonMode = InterfaceButtonMode.HoverImage;
                deviceBtn.BtnName = "DeviceBTN";
                deviceBtn.OnButtonClick += OnButtonClick;
                _deviceButtons.Add(deviceBtn);
            }

            #endregion

            #region Paginator

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            #endregion

            #region Base Label

            _baseName = GameObjectHelpers.FindGameObject(gameObject, "BaseName")?.GetComponent<Text>();

            #endregion

            displayManager.GetController().IPCMessage += IpcMessage;

            _isInitialized = true;
        }

        private void IpcMessage(string obj)
        {
            if (obj.Equals("RefreshTransceiverData"))
            {
                UpdateTransceiver();
            }
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            _displayManager.OpenTransceiverDialog((FcsDevice)arg2);
        }

        private void OnTransceiverGrid(DisplayData data)
        {
            try
            {
                if (_itemController == null) return;

                var grouped = _itemController.GetBaseManager().GetRegisteredDevices().Where(x=>x.Value.IsVisible && x.Value.IsConstructed && x.Value.IsInitialized && x.Value.CanBeSeenByTransceiver).OrderBy(x => x.Value.UnitID).ToList();

                if (!string.IsNullOrEmpty(_searchText?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p.Key).ToLower().Contains(_searchText.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _deviceButtons[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _deviceButtons[w++].Set(grouped.ElementAt(i).Value);
                }

                _operationsGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_operationsGrid.GetMaxPages());

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnDestroy()
        {
            if(_displayManager?.GetController() != null)
                _displayManager.GetController().IPCMessage += IpcMessage;
        }

        /// <summary>
        /// Shows the page and pulls all available devices.
        /// </summary>
        /// <param name="itemController"></param>
        internal void Show(DSSListItemController itemController)
        {
            gameObject.SetActive(true);

            //Set the current ItemController
            _itemController = itemController;

            //Load Operations
            _operationsGrid.DrawPage();

            //Set Base Name
            _baseName.text = itemController.GetBaseManager()?.GetBaseName();

            //Update Operations
            UpdateTransceiver();

        }

        /// <summary>
        /// Hide the page and clears data.
        /// </summary>
        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        public void GoToPage(int index)
        {
            _operationsGrid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            //Not in use
        }

        internal void UpdateTransceiver()
        {
            if (_itemController == null) return;
            var totalOperations = _itemController.GetBaseManager().GetBaseOperations().Count;
            _totalOperations.text = $"{totalOperations}/{_itemController.GetBaseManager().GetBaseOperators().Count * 10}";
        }

        internal void RefreshList()
        {
            _operationsGrid.DrawPage();
        }
        
    }
}