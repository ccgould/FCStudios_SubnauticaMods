using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Mono;
using DataStorageSolutions.Structs;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

namespace DataStorageSolutions.Model
{
    internal class AutoCraftingWizardDialog : MonoBehaviour
    {
        private GridHelper _grid;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private string _itemSearchString;

        public Action OnCancelButtonClick { get; set; }
        public Action OnConfirmButtonClick { get; set; }

        private Button _cancelBTN;
        private Button _confirmBTN;
        private Text _confirmBTNText;
        private Text _cancelBTNText;
        private GameObject _confirmButtonObject;
        private GameObject _cancelBTNObject;
        private bool _initialized;
        private FCSOperation _operation;
        private TechType _techType;
        private int _amount;
        private IMessageDialogSender _sender;
        private Dictionary<TechType,Button> _buttonsList = new Dictionary<TechType,Button>();


        private void Initialize()
        {
            if (_initialized) return;

            #region Search
            var inputField = InterfaceHelpers.FindGameObject(gameObject, "InputField");

            if (inputField != null)
            {
                var text = InterfaceHelpers.FindGameObject(inputField, "Placeholder")?.GetComponent<Text>();
                if (text != null)
                {
                    text.text = AuxPatchers.SearchForItemsMessage();
                }

                var searchField = inputField.AddComponent<SearchField>();
                searchField.OnSearchValueChanged += UpdateSearch;
            }

            #endregion

            _grid = gameObject.AddComponent<GridHelper>();
            _grid.OnLoadDisplay += OnLoadCategoryGrid;
            _grid.Setup(4, DSSModelPrefab.AutoCraftTechTypeItem, gameObject, _startColor, _hoverColor, OnButtonClick);

            _cancelBTNObject = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN");
            _cancelBTN = _cancelBTNObject?.GetComponent<Button>();
            _cancelBTNText = _cancelBTNObject?.GetComponentInChildren<Text>(true);
            _cancelBTN?.onClick.AddListener(() =>
            {
                OnCancelButtonClick?.Invoke();
                HideMessageBox();
            });

            _confirmButtonObject = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN");
            var amountInputField = GameObjectHelpers.FindGameObject(gameObject, "AmountInputField").GetComponent<InputField>();
            amountInputField.onValueChanged.AddListener(value =>
            {
                if (int.TryParse(value, out var result))
                {
                    _amount = result;
                }
            });
            _confirmBTNText = _confirmButtonObject?.GetComponentInChildren<Text>(true);
            _confirmBTN = _confirmButtonObject?.GetComponent<Button>();
            _confirmBTN?.onClick.AddListener(() =>
            {
                OnConfirmButtonClick?.Invoke();
                _operation.AddCraftingOperation(_sender,_techType,_amount <= 0 ? 1 : _amount);
                HideMessageBox();
            });

            _initialized = true;
        }

        private void UpdateSearch(string value)
        {
            _itemSearchString = value;
            _grid.DrawPage();
        }

        private void ChangeConfirmButtonText(string text)
        {
            if (_confirmBTNText != null)
                _confirmBTNText.text = text;
        }

        private void ChangeCancelButtonText(string text)
        {
            if (_cancelBTNText != null)
                _cancelBTNText.text = text;
        }

        private void HideMessageBox()
        {
            gameObject.SetActive(false);
        }

        private void HideCancelBTN()
        {
            _cancelBTNObject?.SetActive(false);
        }

        public virtual void ShowDialog(IMessageDialogSender sender, FCSOperation operation, FCSMessageBox buttons = FCSMessageBox.YesNo)
        {
            Initialize();
            RefreshButtons(buttons);
            gameObject.SetActive(true);
            _operation = operation;
            _sender = sender;
        }

        private void RefreshButtons(FCSMessageBox buttons)
        {
            switch (buttons)
            {
                case FCSMessageBox.OK:
                    ChangeConfirmButtonText("OK");
                    HideCancelBTN();
                    break;
                case FCSMessageBox.OKCancel:
                    ChangeConfirmButtonText("OK");
                    ChangeCancelButtonText("CANCEL");
                    break;
                case FCSMessageBox.RetryCancel:
                    ChangeConfirmButtonText("RETRY");
                    ChangeCancelButtonText("CANCEL");
                    break;
                case FCSMessageBox.YesNo:
                    ChangeConfirmButtonText("YES");
                    ChangeCancelButtonText("NO");
                    break;
            }
        }

        private void OnLoadCategoryGrid(DisplayData data)
        {
            _grid.ClearPage();
            _buttonsList.Clear();
            var grouped = Mod.AllTechTypes.OrderBy(x=>Language.main.Get(x)).ToList();

            if (!string.IsNullOrEmpty(_itemSearchString?.Trim()))
            {
                grouped = grouped.Where(p => Language.main.Get(p).StartsWith(_itemSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (data.EndPosition > grouped.Count)
            {
                data.EndPosition = grouped.Count;
            }

            if (data.ItemsGrid?.transform == null)
            {
                QuickLogger.Debug("Grid returned null canceling operation");
                return;
            }

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                GameObject buttonPrefab = GameObject.Instantiate(data.ItemsPrefab);

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }
                buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                buttonPrefab.gameObject.GetComponentInChildren<Text>().text = Language.main.Get(grouped[i]);
                var button = buttonPrefab.GetComponent<Button>();
                var i1 = i;
                button.onClick.AddListener(() =>
                {
                    _techType = grouped[i1];
                    RefreshTechTypeButtons();
                });
                _buttonsList.Add(grouped[i], button);
            }

            _grid.UpdaterPaginator(grouped.Count);
        }

        private void RefreshTechTypeButtons()
        {
            foreach (KeyValuePair<TechType, Button> button in _buttonsList)
            {
                button.Value.GetComponentInChildren<Text>().text = button.Key == _techType ? $"{Language.main.Get(button.Key)} [Selected]" : Language.main.Get(button.Key);
            }
        }

        private void OnButtonClick(string button, object arg2)
        {
            _techType = (TechType) arg2;
        }
    }
}
