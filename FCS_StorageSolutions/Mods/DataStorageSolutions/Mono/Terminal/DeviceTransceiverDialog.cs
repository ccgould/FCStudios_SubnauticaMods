using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Terminal
{
    internal class DeviceTransceiverDialog : MonoBehaviour, IFCSDisplay
    {
        private Toggle _performPullOperation;
        private PaginatorController _paginatorController;
        private string _searchText;
        private GridHelperV2 _techTypeGrid;
        private readonly List<TechTypeItem> _techTypeButtons = new List<TechTypeItem>();
        private FcsDevice _fcsDevice;
        private TechType _techType;
        private int _amount;
        private BaseTransferOperation _operation;
        private Text _title;

        internal void Initialize()
        {
            _performPullOperation = gameObject.GetComponentInChildren<Toggle>();

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");
            var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
            text.text = AuxPatchers.SearchForItemsMessage();

            var searchField = inputField.AddComponent<SearchField>();
            searchField.OnSearchValueChanged += UpdateSearch;
            #endregion

            #region Paginator

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            #endregion

            #region Grid

            _techTypeGrid = gameObject.EnsureComponent<GridHelperV2>();
            _techTypeGrid.OnLoadDisplay += OnTransceiverGrid;
            _techTypeGrid.Setup(32, gameObject, Color.gray, Color.white, OnButtonClick);

            #endregion

            foreach (Transform deviceItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var deviceBtn = deviceItem.gameObject.EnsureComponent<TechTypeItem>();
                deviceBtn.ButtonMode = InterfaceButtonMode.HoverImage;
                deviceBtn.BtnName = "TechTypeBTN";
                deviceBtn.OnButtonClick += OnButtonClick;
                _techTypeButtons.Add(deviceBtn);
            }

            var confirmBTN = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN").GetComponent<Button>();
            confirmBTN.onClick.AddListener((() =>
            {
                if (_techType == TechType.None && !_performPullOperation.isOn)
                {
                    QuickLogger.Message("Please select an Item to send",true);
                    return;
                }

                if (_operation != null)
                {
                    _operation.TransferItem = _techType;
                    _operation.Amount = _amount;
                    _operation.IsPullOperation = _performPullOperation.isOn;
                }
                else
                {
                    _fcsDevice.Manager?.AddOperationForDevice(new BaseTransferOperation
                    {
                        DeviceId = _fcsDevice.UnitID,
                        IsPullOperation = _performPullOperation.isOn,
                        TransferItem = _techType
                    });
                }
                Hide();
            }));

            _title = GameObjectHelpers.FindGameObject(gameObject, "Title")?.GetComponent<Text>();

            var cancelBTN = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN").GetComponent<Button>();
            cancelBTN.onClick.AddListener(Hide);
        }

        private void OnTransceiverGrid(DisplayData data)
        {
            try
            {
                QuickLogger.Debug("1");
                var grouped = _fcsDevice?.AllowedTransferItems?.ToList();
                QuickLogger.Debug("2");
                if (grouped == null) return;
                QuickLogger.Debug("3");
                if (!string.IsNullOrEmpty(_searchText?.Trim()))
                {
                    grouped = grouped.Where(p => TechTypeExtensions.Get(Language.main, p).ToLower().Contains(_searchText.Trim().ToLower())).ToList();
                }
                QuickLogger.Debug("4");
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                QuickLogger.Debug("5");
                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _techTypeButtons[i].Reset();
                }
                QuickLogger.Debug("6");
                int w = 0;
                QuickLogger.Debug("7");
                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    int index = w++;
                    _techTypeButtons[index].Set(grouped.ElementAt(i));
                    QuickLogger.Debug("8");
                    if (_techType == grouped.ElementAt(i))
                    {
                        _techTypeButtons[index].Select();
                    }
                    QuickLogger.Debug("9");
                }
                QuickLogger.Debug("10");
                _techTypeGrid.UpdaterPaginator(grouped.Count);
                QuickLogger.Debug("11");
                _paginatorController.ResetCount(_techTypeGrid.GetMaxPages());
                QuickLogger.Debug("12");
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            _techType = (TechType) arg2;
        }

        private void UpdateSearch(string obj)
        {
            _searchText = obj;
            _techTypeGrid.DrawPage();
        }

        public void Show(FcsDevice fcsDevice)
        {
            _fcsDevice = fcsDevice;
            gameObject.SetActive(true);
            _operation = fcsDevice.Manager.GetDeviceOperation(fcsDevice);
            BaseManager.GlobalNotifyByID(Mod.DSSTabID,"RefreshTransceiverData");
            _title.text = $"<color=#00ffffff>[{fcsDevice.UnitID}]</color> Item Transciever Settings";
            if (_operation != null)
            {
                _techType = _operation.TransferItem;
                _performPullOperation.isOn = _operation.IsPullOperation;
                _amount = _operation.Amount;
            }

            _techTypeGrid.DrawPage();
        }        
        
        public void Hide()
        {
            gameObject.SetActive(false);
            _operation = null;
            _techType = TechType.None;
            _amount = 1;
            _performPullOperation.isOn = false;
            _title.text = $"<color=#00ffffff>[]</color> Item Transciever Settings";
        }

        public void GoToPage(int index)
        {
            _techTypeGrid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            // Not in use
        }
    }
}