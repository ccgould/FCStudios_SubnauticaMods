using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Enumerators;
using FCSCommon.Abstract;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DataStorageSolutions.Mono
{
    internal class DSSOperatorDisplayManager : AIDisplay
    {
        private DSSOperatorController _mono;
        private readonly List<OperationInterfaceButton> _fromDevicesList = new List<OperationInterfaceButton>();
        private readonly List<OperationInterfaceButton> _toDevicesList = new List<OperationInterfaceButton>();
        private readonly List<OperationInterfaceButton> _itemTechTypeList = new List<OperationInterfaceButton>();
        private string _toCurrentSearchString;
        private string _fromCurrentSearchString;
        private string _itemCurrentSearchString;
        private bool _isBeingDestroyed;
        private GridHelper _fromGrid;
        private int _page;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private GridHelper _operationsGrid;
        private FCSConnectableDevice _fromDevices;
        private TechType _itemTechType;
        private FCSConnectableDevice _toDevices;
        private GridHelper _itemTechTypeGrid;
        private GridHelper _toGrid;

        private void RefreshOperatorButtons(OperationInterfaceButton button)
        {
            foreach (OperationInterfaceButton interfaceButton in _fromDevicesList)
            {
                if (interfaceButton != button)
                {
                    interfaceButton.SetCheck(false);
                }
            }

            foreach (OperationInterfaceButton interfaceButton in _itemTechTypeList)
            {
                if (interfaceButton != button)
                {
                    interfaceButton.SetCheck(false);
                }
            }

            foreach (OperationInterfaceButton interfaceButton in _toDevicesList)
            {
                if (interfaceButton != button)
                {
                    interfaceButton.SetCheck(false);
                }
            }
        }
        
        private void OnToSearchValueChanged(string newSearch)
        {
            _toCurrentSearchString = newSearch;
            _toGrid.DrawPage();
        }

        private void OnFromSearchValueChanged(string newSearch)
        {
            _fromCurrentSearchString = newSearch;
            _fromGrid.DrawPage();
        }

        private void OnSearchItemValueChanged(string newSearch)
        {
            _itemCurrentSearchString = newSearch;
            _itemTechTypeGrid.DrawPage();
        }

        private void ToButtonClick()
        {
            if (_toDevices == null)
            {
                //Add Message
                return;
            }
            GoToPage(OperatorPages.Operations);
        }

        private void ItemButtonClick()
        {
            if (_itemTechType == TechType.None)
            {
                //Add Message
                return;
            }
            GoToPage(OperatorPages.To);
            _toGrid.DrawPage();
        }

        private void FromButtonClick()
        {
            if (_fromDevices == null)
            {
                //Add Message
                return;
            }
            GoToPage(OperatorPages.Items);
            _itemTechTypeGrid.DrawPage();
        }

        private void OnLoadOperationsGrid(DisplayData obj)
        {

        }
        
        private void OnLoadFromGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _fromGrid.ClearPage();
                _fromDevicesList.Clear();

                var grouped = _mono.Manager.FCSConnectables;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                if (!string.IsNullOrEmpty(_fromCurrentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => p.Value.UnitID.StartsWith(_fromCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToDictionary(p => p.Key, p => p.Value);
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
                    var connectable = grouped.ElementAt(i);

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
                    
                    QuickLogger.Debug($"Adding Unit: {connectable.Value.UnitID}", true);

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var button = buttonPrefab.AddComponent<OperationInterfaceButton>();
                    if (connectable.Value == _fromDevices)
                    {
                        button.SetCheck(true);
                    }
                    button.BtnName = "OperatorDisplayBTN";
                    button.Tag = new OperatorButtonData{Button = button , Connectable = connectable.Value};
                    button.TextLineOne = $"Take from {Language.main.Get(connectable.Value.GetTechType())}";
                    button.OnButtonClick = OnButtonClick;
                    buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(connectable.Value.GetTechType());
                    _fromDevicesList.Add(button);
                }

                _fromGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnToFromGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _toGrid.ClearPage();
                _toDevicesList.Clear();

                var grouped = _mono.Manager.FCSConnectables;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                if (!string.IsNullOrEmpty(_toCurrentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => p.Value.UnitID.StartsWith(_toCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToDictionary(p => p.Key, p => p.Value);
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
                    var connectable = grouped.ElementAt(i);

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

                    QuickLogger.Debug($"Adding Unit: {connectable.Value.UnitID}", true);

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var button = buttonPrefab.AddComponent<OperationInterfaceButton>();
                    if (connectable.Value == _toDevices)
                    {
                        button.SetCheck(true);
                    }
                    button.BtnName = "OperatorDisplayBTN";
                    button.Tag = new OperatorButtonData { Button = button, Connectable = connectable.Value };
                    button.TextLineOne = $"Take from {Language.main.Get(connectable.Value.GetTechType())}";
                    button.OnButtonClick = OnButtonClick;
                    buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(connectable.Value.GetTechType());
                    _toDevicesList.Add(button);
                }

                _toGrid.UpdaterPaginator(grouped.Count);
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

                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _itemTechTypeGrid.ClearPage();
                _itemTechTypeList.Clear();

                var grouped = Mod.AllTechTypes;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                if (!string.IsNullOrEmpty(_itemCurrentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => Language.main.Get(p).StartsWith(_itemCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                QuickLogger.Debug($"1");

                if (data.ItemsGrid?.transform == null)
                {
                    QuickLogger.Debug("Grid returned null canceling operation");
                    return;
                }
                QuickLogger.Debug($"2");

            for (int i = data.StartPosition; i < data.EndPosition; i++)
            {
                GameObject buttonPrefab = GameObject.Instantiate(DSSModelPrefab.OperatorItemPrefab);

                if (buttonPrefab == null || data.ItemsGrid == null)
                {
                    if (buttonPrefab != null)
                    {
                        QuickLogger.Debug("Destroying Tab", true);
                        Destroy(buttonPrefab);
                    }
                    return;
                }
                
                var button = buttonPrefab.AddComponent<OperationInterfaceButton>();
                if (grouped[i] == _itemTechType)
                {
                    button.SetCheck(true);
                }
                buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                button.BtnName = "ItemTechBTN";
                button.OnButtonClick = OnButtonClick;
                button.Tag = new OperatorButtonData { Button = button, TechType = grouped[i] };
                button.TextLineOne = $"Send {Language.main.Get(grouped[i])}";
                buttonPrefab.GetComponentInChildren<Text>().text = Language.main.Get(grouped[i]);
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(grouped[i]);
                _itemTechTypeList.Add(button);
            }

                _itemTechTypeGrid.UpdaterPaginator(grouped.Count);
            
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        internal void Setup(DSSOperatorController mono)
        {
            _mono = mono;
            _page = Animator.StringToHash("Pages");
            if (FindAllComponents())
            {
            }
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "ConnectionsBTN":
                    GoToPage(OperatorPages.Operations);
                    break;
                case "AutoCraftBTN":
                    GoToPage(OperatorPages.Operations);
                    break;
                case "HomeBTN":
                    GoToPage(OperatorPages.Home);
                    break;
                case "AddBTN":
                    GoToPage(OperatorPages.From);
                    _fromGrid.DrawPage();
                    break;
                case "ItemsNextButton":
                    GoToPage(OperatorPages.To);
                    break;
                case "ToFinishButton":
                    GoToPage(OperatorPages.Operations);
                    break;
                case "OperatorDisplayBTN":
                    var operatorData = (OperatorButtonData)tag;
                    if (GetCurrentPage() == OperatorPages.From)
                    {
                        _fromDevices = operatorData.Button.IsChecked() ? operatorData.Connectable : null;

                    }
                    else if (GetCurrentPage() == OperatorPages.To)
                    {
                        _toDevices = operatorData.Button.IsChecked() ? operatorData.Connectable : null;
                    }

                    RefreshOperatorButtons(((OperatorButtonData)tag).Button);

                    break;
                case "ItemTechBTN":
                    var operatorItemData = (OperatorButtonData)tag;
                    _itemTechType = operatorItemData.Button.IsChecked() ? operatorItemData.TechType : TechType.None;
                    RefreshOperatorButtons(((OperatorButtonData)tag).Button);
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                #region Home

                var home = GameObjectHelpers.FindGameObject(gameObject, "Home");

                //connections BTN
                var connectionsBTNObject = GameObjectHelpers.FindGameObject(home, "ConnectionsBTN");
                InterfaceHelpers.CreateButton(connectionsBTNObject, "ConnectionsBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                //Auto Craft BTN
                var autoCraftBTNObject = GameObjectHelpers.FindGameObject(home, "AutoCraftBTN");
                InterfaceHelpers.CreateButton(autoCraftBTNObject, "AutoCraftBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #endregion

                #region Current Operations Page

                var currentOperationsPage = GameObjectHelpers.FindGameObject(gameObject, "OperationsPage");

                var addOperationBtnObject = GameObjectHelpers.FindGameObject(currentOperationsPage, "AddBTN");
                InterfaceHelpers.CreateButton(addOperationBtnObject, "AddBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                _operationsGrid = _mono.gameObject.AddComponent<GridHelper>();
                _operationsGrid.OnLoadDisplay += OnLoadOperationsGrid;
                _operationsGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, currentOperationsPage, _startColor, _hoverColor, OnButtonClick);


                #endregion

                #region From Page

                var fromPage = GameObjectHelpers.FindGameObject(gameObject, "FromPage");

                var fromNextBTNObject = GameObjectHelpers.FindGameObject(fromPage, "Button")?.GetComponent<Button>();
                fromNextBTNObject.onClick.AddListener(FromButtonClick);


                _fromGrid = _mono.gameObject.AddComponent<GridHelper>();
                _fromGrid.OnLoadDisplay += OnLoadFromGrid;
                _fromGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, fromPage, _startColor, _hoverColor, OnButtonClick);
                _fromGrid.DrawPage();

                var fromInputField = InterfaceHelpers.FindGameObject(fromPage, "InputField");
                var text = InterfaceHelpers.FindGameObject(fromInputField, "Placeholder")?.GetComponent<Text>();
                text.text = AuxPatchers.SearchForItemsMessage();

                var searchField = fromInputField.AddComponent<SearchField>();
                searchField.OnSearchValueChanged += OnFromSearchValueChanged;

                #endregion

                #region To Page

                var toPage = GameObjectHelpers.FindGameObject(gameObject, "ToPage");

                var toNextBTNObject = GameObjectHelpers.FindGameObject(toPage, "Button")?.GetComponent<Button>();
                toNextBTNObject.onClick.AddListener(ToButtonClick);

                _toGrid = _mono.gameObject.AddComponent<GridHelper>();
                _toGrid.OnLoadDisplay += OnToFromGrid;
                _toGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, toPage, _startColor, _hoverColor, OnButtonClick);
                _toGrid.DrawPage();

                var toInputField = InterfaceHelpers.FindGameObject(toPage, "InputField");
                var toText = InterfaceHelpers.FindGameObject(toInputField, "Placeholder")?.GetComponent<Text>();
                toText.text = AuxPatchers.SearchForItemsMessage();

                var toSearchField = toInputField.AddComponent<SearchField>();
                toSearchField.OnSearchValueChanged += OnToSearchValueChanged;

                #endregion


                var itemPage = GameObjectHelpers.FindGameObject(gameObject, "ItemPage");

                var itemNextBTNObject = GameObjectHelpers.FindGameObject(itemPage, "Button")?.GetComponent<Button>();
                itemNextBTNObject.onClick.AddListener(ItemButtonClick);

                _itemTechTypeGrid = _mono.gameObject.AddComponent<GridHelper>();
                _itemTechTypeGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemTechTypeGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, itemPage, _startColor, _hoverColor, OnButtonClick);
                _itemTechTypeGrid.DrawPage();

                var itemInputField = InterfaceHelpers.FindGameObject(itemPage, "InputField");
                var itemText = InterfaceHelpers.FindGameObject(itemInputField, "Placeholder")?.GetComponent<Text>();
                itemText.text = AuxPatchers.SearchForItemsMessage();

                var itemSearchField = itemInputField.AddComponent<SearchField>();
                itemSearchField.OnSearchValueChanged += OnSearchItemValueChanged;
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} | StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }

        internal void GoToPage(OperatorPages page)
        {
            _mono.AnimationHandler.SetIntHash(_page,(int) page);
        }

        internal OperatorPages GetCurrentPage()
        {
            return (OperatorPages)_mono.AnimationHandler.GetIntHash(_page);
        }

        internal void LoadCommands()
        {
#if DEBUG
            QuickLogger.Debug($"Refreshing Operator: {_mono?.GetPrefabID()}", true);
#endif
            _fromGrid?.DrawPage();
        }
    }

    internal struct OperatorButtonData
    {
        internal OperationInterfaceButton Button { get; set; }
        internal FCSConnectableDevice Connectable { get; set; }
        internal TechType TechType { get; set; }
    }
}
