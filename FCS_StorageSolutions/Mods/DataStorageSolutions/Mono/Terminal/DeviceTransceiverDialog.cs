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
using UnityEngine.EventSystems;
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
        private int _amount;
        private BaseTransferOperation _operation;
        private Text _title;
        private BaseTransferOperation _operationHistory;
        private Text _amountText;
        private NumberIncreaseButton _appendToAmountBTN;
        private Toggle _isEnabled;

        internal void Initialize()
        {
            _performPullOperation = GameObjectHelpers.FindGameObject(gameObject, "PullOperationToggle").GetComponent<Toggle>();
            _isEnabled = GameObjectHelpers.FindGameObject(gameObject, "IsEnabledToggle").GetComponent<Toggle>();

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
                if (_operation == null) return;

                if (_operation.TransferItems.Count == 0 && !_performPullOperation.isOn)
                {
                    QuickLogger.Message("Please select an Item to send",true);
                    return;
                }

                if (_operation != null)
                {
                    _operation.MaxAmount = _amount;
                    _operation.IsPullOperation = _performPullOperation.isOn;
                    _operation.IsEnabled = _isEnabled.isOn;
                    _fcsDevice.Manager?.AddOperationForDevice(_operation);
                }
                
                Hide();
                BaseManager.GlobalNotifyByID(Mod.DSSTabID, "RefreshTransceiverData");
            }));

            _title = GameObjectHelpers.FindGameObject(gameObject, "Title")?.GetComponent<Text>();

            var cancelBTN = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN").GetComponent<Button>();
            cancelBTN.onClick.AddListener(() =>
            {
                _operation = new BaseTransferOperation(_operationHistory);
                Hide();
            });

            var appendToAmountObj = GameObjectHelpers.FindGameObject(gameObject, "AppendToAmountBTN");
            _amountText = GameObjectHelpers.FindGameObject(appendToAmountObj, "Text")?.GetComponent<Text>();
            _appendToAmountBTN = appendToAmountObj.EnsureComponent<NumberIncreaseButton>();
            _appendToAmountBTN.TextComponent = _amountText;
            _appendToAmountBTN.OnAmountChanged += AddToAmount;
        }

        private void AddToAmount(int amount)
        {
            _amount = amount;
        }

        private void OnTransceiverGrid(DisplayData data)
        {
            try
            {
                var grouped = _fcsDevice?.AllowedTransferItems?.ToList();

                if (grouped == null) return;

                if (!string.IsNullOrEmpty(_searchText?.Trim()))
                {
                    grouped = grouped.Where(p => TechTypeExtensions.Get(Language.main, p).ToLower().Contains(_searchText.Trim().ToLower())).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _techTypeButtons[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    int index = w++;
                    _techTypeButtons[index].Set(grouped.ElementAt(i));

                    if (_operation.TransferItems.Contains(grouped.ElementAt(i)))
                    {
                        _techTypeButtons[index].Select();
                    }
                }

                _techTypeGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_techTypeGrid.GetMaxPages());
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
            var techType = (TechType) arg2;

            if (_operation.TransferItems.Contains(techType))
            {
                _operation.TransferItems.Remove(techType);
            }
            else
            {
                _operation.TransferItems.Add(techType);
            }
        }

        private void UpdateSearch(string obj)
        {
            _searchText = obj;
            _techTypeGrid.DrawPage();
        }

        public void Show(FcsDevice fcsDevice)
        {
            _fcsDevice = fcsDevice;

            

            if (_fcsDevice == null)
            {
                QuickLogger.DebugError("FCSDevice returned null",true);
                return;
            }

            _operation = fcsDevice.Manager.GetDeviceOperation(fcsDevice) ?? new BaseTransferOperation
            {
                DeviceId = _fcsDevice.UnitID,
                IsBeingEdited = true
            };

            _operationHistory = new BaseTransferOperation(_operation);

            _appendToAmountBTN.Initialize(_operation.MaxAmount,1,fcsDevice.MaxItemAllowForTransfer);

            _title.text = $"<color=#00ffffff>[{fcsDevice.UnitID}]</color> Item Transciever Settings";
            _techTypeGrid.DrawPage();

            gameObject.SetActive(true);
        }        
        
        public void Hide()
        {
            _operation.IsBeingEdited = false;
            gameObject.SetActive(false);
            _operation = null;
            _operation = null;
            _amount = 1;
            _performPullOperation.isOn = false;
            _isEnabled.isOn = false;
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

    internal class NumberIncreaseButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private int _amount;
        private int _max;
        private int _min;
        private int _increaseBy;
        public Action<int> OnAmountChanged { get; set; }
        public Text TextComponent { get; set; }

        public void Initialize(int amount,int min,int max,int increaseBy = 1)
        {
            _increaseBy = increaseBy;
            _max = max;
            _min = min;
            _amount = amount;
            Notify();
        }

        private void Notify()
        {
            OnAmountChanged?.Invoke(_amount);
            if (TextComponent != null)
                TextComponent.text = $"{_amount}";
        }

        public void SetAmount(int amount)
        {
            _amount = amount;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            _amount++;
            
            if (_amount > _max)
            {
                _amount = _min;
            }
            
            Notify();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            TextLineOne  = $"Min: {_min}/{_max}";
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
        }
    }
}