using System;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using DataStorageSolutions.Structs;
using FCSCommon.Abstract;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
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
        private readonly List<OperationInterfaceButton> _autoCraftItemTechTypeList = new List<OperationInterfaceButton>();
        private string _toCurrentSearchString;
        private string _fromCurrentSearchString;
        private string _itemCurrentSearchString;
        private string _autocraftItemSearchString;
        private bool _isBeingDestroyed;
        private GridHelper _fromGrid;
        private int _page;
        private readonly Color _startColor = Color.grey;
        private readonly Color _hoverColor = Color.white;
        private GridHelper _operationsGrid;
        private FCSConnectableDevice _fromDevice;
        private TechType _itemTechType;
        private FCSConnectableDevice _toDevice;
        private GridHelper _itemTechTypeGrid;
        private GridHelper _toGrid;
        private FCSMessageBoxDialog _messageBoxDialog;
        private GridHelper _visibilityGrid;
        private string _visibilityCurrentSearchString;
        private GridHelper _autoCraftingGrid;
        private GridHelper _autocraftItemTechTypeGrid;
        private TechType _craftTechType;
        
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
            
            foreach (OperationInterfaceButton interfaceButton in _autoCraftItemTechTypeList)
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

        private void OnVisibiltySearchValueChanged(string newSearch)
        {
            _visibilityCurrentSearchString = newSearch;
            _visibilityGrid.DrawPage();
        }

        private void OnSearchAutocraftTechValueChanged(string newSearch)
        {
            _autocraftItemSearchString = newSearch;
            _autocraftItemTechTypeGrid.DrawPage();
        }

        private void ToButtonClick()
        {
            if (_toDevice == null)
            {
                //Add Message
                return;
            }

            BaseManager.AddOperation(new FCSOperation { FromDevice = _fromDevice, ToDevice = _toDevice, TechType = _itemTechType, Manager = _mono.Manager});

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
            if (_fromDevice == null)
            {
                //Add Message
                return;
            }
            GoToPage(OperatorPages.Items);
            _itemTechTypeGrid.DrawPage();
        }

        private void OnOperationCancelButtonClick()
        {
            _messageBoxDialog.ShowMessageBox("Are you sure you would like to cancel this operation this you will have to start over", "cancel");
        }

        private void OnCraftItemCancelButtonClick()
        {
            _messageBoxDialog.ShowMessageBox("Are you sure you would like to cancel this operation this you will have to start over", "itemCancel");
        }

        private void OnLoadOperationsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _operationsGrid.ClearPage();

                var grouped = BaseManager.Operations;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                //if (!string.IsNullOrEmpty(_toCurrentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => p.Value.UnitID.StartsWith(_toCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToDictionary(p => p.Key, p => p.Value);
                //}

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

                    if (connectable.FromDevice == null || connectable.ToDevice == null) {continue;}
                    
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

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var button = buttonPrefab.AddComponent<OperationInterfaceButton>();

                    button.BtnName = "OperationBTN";
                    button.Tag = null;
                    button.TextLineOne = $"{connectable.FromDevice.UnitID} => {Language.main.Get(connectable.TechType)} => {connectable.ToDevice.UnitID}";
                    //buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(InterfaceHelpers.FindGameObject(buttonPrefab,"FromIcon"), "Image").AddComponent<uGUI_Icon>();
                    icon.sprite = connectable.FromDevice.IsBase() ? SpriteManager.Get(TechType.BaseRoom) : SpriteManager.Get(connectable.FromDevice.GetTechType());

                    uGUI_Icon icon1 = InterfaceHelpers.FindGameObject(InterfaceHelpers.FindGameObject(buttonPrefab, "TechTypeIcon"), "Image").AddComponent<uGUI_Icon>();
                    icon1.sprite = SpriteManager.Get(connectable.TechType);

                    uGUI_Icon icon2 = InterfaceHelpers.FindGameObject(InterfaceHelpers.FindGameObject(buttonPrefab, "ToIcon"), "Image").AddComponent<uGUI_Icon>();
                    icon2.sprite = connectable.ToDevice.IsBase() ? SpriteManager.Get(TechType.BaseRoom) : SpriteManager.Get(connectable.ToDevice.GetTechType());

                    var deleteBTN = buttonPrefab.GetComponentInChildren<Button>();
                    deleteBTN.onClick.AddListener(() => { _mono.Manager.DeleteOperator(connectable); });
                }

                _operationsGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
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
                    if (connectable.Value == _fromDevice)
                    {
                        button.SetCheck(true);
                    }
                    button.BtnName = "OperatorDisplayBTN";
                    button.Tag = new OperatorButtonData{Button = button , Connectable = connectable.Value};
                    button.TextLineOne = connectable.Value.IsBase() ? "Take from base." : $"Take from {Language.main.Get(connectable.Value.GetTechType())}";
                    button.OnButtonClick = OnButtonClick;
                    buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = connectable.Value.IsBase() ? SpriteManager.Get(TechType.BaseRoom)  : SpriteManager.Get(connectable.Value.GetTechType());
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

        private void OnLoadToGrid(DisplayData data)
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
                    if (connectable.Value == _toDevice)
                    {
                        button.SetCheck(true);
                    }
                    button.BtnName = "OperatorDisplayBTN";
                    button.Tag = new OperatorButtonData { Button = button, Connectable = connectable.Value };
                    button.TextLineOne = connectable.Value.IsBase() ? "Take from base." : $"Take from {Language.main.Get(connectable.Value.GetTechType())}";
                    button.OnButtonClick = OnButtonClick;
                    buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = connectable.Value.IsBase() ? SpriteManager.Get(TechType.BaseRoom) : SpriteManager.Get(connectable.Value.GetTechType());
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

        private void OnLoadAutoCraftItemsGrid(DisplayData data)
        {
            if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

            _autocraftItemTechTypeGrid.ClearPage();
            _autoCraftItemTechTypeList.Clear();

            var grouped = Mod.AllTechTypes.Where(DSSHelpers.CheckIfTechDataAvailable).ToList();

            if (!string.IsNullOrEmpty(_autocraftItemSearchString?.Trim()))
            {
                grouped = grouped.Where(p => Language.main.Get(p).StartsWith(_autocraftItemSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
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
                if (grouped[i] == _craftTechType)
                {
                    button.SetCheck(true);
                }
                buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                button.BtnName = "AutoCraftItemTechBTN";
                button.OnButtonClick = OnButtonClick;
                button.STARTING_COLOR = _startColor;
                button.HOVER_COLOR = _hoverColor;
                button.Tag = new OperatorButtonData { Button = button, TechType = grouped[i] };
                button.TextLineOne = DSSHelpers.GetTechDataString(grouped[i]);
                //buttonPrefab.GetComponentInChildren<Text>().text = Language.main.Get(grouped[i]);
                uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                icon.sprite = SpriteManager.Get(grouped[i]);
                _autoCraftItemTechTypeList.Add(button);
            }

            _autocraftItemTechTypeGrid.UpdaterPaginator(grouped.Count);
        }

        private void OnLoadAutoCraftGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _autoCraftingGrid.ClearPage();

                var grouped = BaseManager.Crafts;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                //if (!string.IsNullOrEmpty(_toCurrentSearchString?.Trim()))
                //{
                //    grouped = grouped.Where(p => p.Value.UnitID.StartsWith(_toCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToDictionary(p => p.Key, p => p.Value);
                //}

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
                    var craft = grouped.ElementAt(i);

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

                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);

                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = SpriteManager.Get(craft.TechType);

                    var deleteBTN = InterfaceHelpers.FindGameObject(buttonPrefab, "Delete")?.AddComponent<InterfaceButton>();
                    if (deleteBTN != null)
                    {
                        deleteBTN.BtnName = "AutoCraftItemDeleteBTN";
                        deleteBTN.TextLineOne = AuxPatchers.Delete();
                        deleteBTN.STARTING_COLOR = _startColor;
                        deleteBTN.HOVER_COLOR = _hoverColor;
                        deleteBTN.OnButtonClick = delegate
                        {
                            BaseManager.DeleteAutoCraft(craft); 
                            Refresh();
                        };
                    }

                    var buttonText = buttonPrefab.GetComponentInChildren<Text>();
                    buttonText.text = Language.main.Get(craft.TechType);

                    var checkbox = buttonPrefab.GetComponentInChildren<Toggle>();
                    checkbox.isOn = craft.IsCraftable;
                    checkbox.onValueChanged.AddListener((value) =>
                    {
                        craft.IsCraftable = value;
                    });

                    var craftBTN = InterfaceHelpers.FindGameObject(buttonPrefab, "AutocraftBTN")?.AddComponent<InterfaceButton>();

                    if (craftBTN != null)
                    {
                        craftBTN.BtnName = "AutoCraftItemDeleteBTN";
                        craftBTN.TextLineOne = AuxPatchers.Craft();
                        craftBTN.STARTING_COLOR = _startColor;
                        craftBTN.HOVER_COLOR = _hoverColor;
                        craftBTN.OnButtonClick = delegate { BaseManager.PerformCraft(craft); };
                    }
                }

                _autoCraftingGrid.UpdaterPaginator(grouped.Count);
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private void OnLoadVisibilityGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _mono == null || _mono?.Manager == null) return;

                _visibilityGrid.ClearPage();


                var grouped = _mono.Manager.FCSConnectables;

                if (grouped == null)
                {
                    QuickLogger.Debug("Grouped returned null canceling operation");
                    return;
                }

                if (!string.IsNullOrEmpty(_visibilityCurrentSearchString?.Trim()))
                {
                    grouped = grouped.Where(p => p.Value.UnitID.StartsWith(_visibilityCurrentSearchString.Trim(), StringComparison.OrdinalIgnoreCase)).ToDictionary(p => p.Key, p => p.Value);
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

                    if (connectable.Value.IsBase() || !connectable.Value.CanBeVisible()) continue;

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
                    button.BtnName = "VisibilityItemBTN";
                    button.Tag = new OperatorButtonData { Button = button, Connectable = connectable.Value };
                    button.TextLineOne = $"Make {Language.main.Get(connectable.Value.GetTechType())} visible to the terminal";
                    button.OnButtonClick = OnButtonClick;
                    button.SetCheck(connectable.Value.IsVisible);
                    buttonPrefab.GetComponentInChildren<Text>().text = connectable.Value.UnitID;
                    uGUI_Icon icon = InterfaceHelpers.FindGameObject(buttonPrefab, "Icon").AddComponent<uGUI_Icon>();
                    icon.sprite = connectable.Value.IsBase() ? SpriteManager.Get(TechType.BaseRoom) : SpriteManager.Get(connectable.Value.GetTechType());
                }

                _visibilityGrid.UpdaterPaginator(grouped.Count);
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
                icon.sprite =  SpriteManager.Get(grouped[i]);
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
                _mono.Manager.LoadCraftingOperations();
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

                //Visibility BTN
                var visiblityBTNObject = GameObjectHelpers.FindGameObject(home, "VisiblityBTN");
                InterfaceHelpers.CreateButton(visiblityBTNObject, "VisibilityBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #endregion

                #region Visibility Page

                var visibilityPage = GameObjectHelpers.FindGameObject(gameObject, "VisibliltyPage");
                _visibilityGrid = _mono.gameObject.AddComponent<GridHelper>();
                _visibilityGrid.OnLoadDisplay += OnLoadVisibilityGrid;
                _visibilityGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, visibilityPage, _startColor, _hoverColor, OnButtonClick);
                _visibilityGrid.DrawPage();

                var visibilityInputField = InterfaceHelpers.FindGameObject(visibilityPage, "InputField");
                var visibilityText = InterfaceHelpers.FindGameObject(visibilityInputField, "Placeholder")?.GetComponent<Text>();
                visibilityText.text = AuxPatchers.SearchForItemsMessage();

                var visibilitySearchField = visibilityInputField.AddComponent<SearchField>();
                visibilitySearchField.OnSearchValueChanged += OnVisibiltySearchValueChanged;

                #endregion

                #region Autocrafting Page

                var autoCraftingPage = GameObjectHelpers.FindGameObject(gameObject, "AutoCraftPage");
                _autoCraftingGrid = _mono.gameObject.AddComponent<GridHelper>();
                _autoCraftingGrid.OnLoadDisplay += OnLoadAutoCraftGrid;
                _autoCraftingGrid.Setup(6, DSSModelPrefab.AutoCraftItemPrefab, autoCraftingPage, _startColor, _hoverColor, OnButtonClick);
                _autoCraftingGrid.DrawPage();
                
                var addCraftBtnObject = GameObjectHelpers.FindGameObject(autoCraftingPage, "AddBTN");
                InterfaceHelpers.CreateButton(addCraftBtnObject, "AddCraftBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                #region Autocrafting Items Page

                var autocraftingitemPage = GameObjectHelpers.FindGameObject(gameObject, "CraftingChoicePage");

                var autocraftingitemBTNObject = GameObjectHelpers.FindGameObject(autocraftingitemPage, "CancelBTN")?.GetComponent<Button>();
                autocraftingitemBTNObject.onClick.AddListener(OnCraftItemCancelButtonClick);

                var autocraftingBTNObject = GameObjectHelpers.FindGameObject(autocraftingitemPage, "Button")?.GetComponent<Button>();
                autocraftingBTNObject?.onClick.AddListener(()=>{
                    if (BaseManager.Crafts.Any(x => x?.TechType == _craftTechType))
                    {
                        _messageBoxDialog.ShowMessageBox("This craft request already exists canceling","craft",FCSMessageBox.OK);
                        return;
                    }
                    GoToPage(OperatorPages.AutoCraft);
                    BaseManager.AddCraft(new FCSOperation {  TechType = _craftTechType, Manager = _mono.Manager });
                    _autoCraftingGrid.DrawPage();
                });

                _autocraftItemTechTypeGrid = _mono.gameObject.AddComponent<GridHelper>();
                _autocraftItemTechTypeGrid.OnLoadDisplay += OnLoadAutoCraftItemsGrid;
                _autocraftItemTechTypeGrid.Setup(18, DSSModelPrefab.AutoCraftItemPrefab, autocraftingitemPage, _startColor, _hoverColor, OnButtonClick);
                _autocraftItemTechTypeGrid.DrawPage();

                var autoCraftItemInputField = InterfaceHelpers.FindGameObject(autocraftingitemPage, "InputField");
                var autocraftItemText = InterfaceHelpers.FindGameObject(autoCraftItemInputField, "Placeholder")?.GetComponent<Text>();
                autocraftItemText.text = AuxPatchers.SearchForItemsMessage();

                var autoCraftItemSearchField = autoCraftItemInputField.AddComponent<SearchField>();
                autoCraftItemSearchField.OnSearchValueChanged += OnSearchAutocraftTechValueChanged;

                #endregion

                #endregion
                
                #region Current Operations Page

                var currentOperationsPage = GameObjectHelpers.FindGameObject(gameObject, "OperationsPage");

                var addOperationBtnObject = GameObjectHelpers.FindGameObject(currentOperationsPage, "AddBTN");
                InterfaceHelpers.CreateButton(addOperationBtnObject, "AddBTN", InterfaceButtonMode.Background,
                    OnButtonClick, _startColor, _hoverColor, MAX_INTERACTION_DISTANCE);

                _operationsGrid = _mono.gameObject.AddComponent<GridHelper>();
                _operationsGrid.OnLoadDisplay += OnLoadOperationsGrid;
                _operationsGrid.Setup(5, DSSModelPrefab.ItemEntryPrefab, currentOperationsPage, _startColor, _hoverColor, OnButtonClick);


                #endregion

                #region From Page

                var fromPage = GameObjectHelpers.FindGameObject(gameObject, "FromPage");

                var fromNextBTNObject = GameObjectHelpers.FindGameObject(fromPage, "Button")?.GetComponent<Button>();
                fromNextBTNObject.onClick.AddListener(FromButtonClick);
                var fromCancelBTNObject = GameObjectHelpers.FindGameObject(fromPage, "CancelBTN")?.GetComponent<Button>();
                fromCancelBTNObject.onClick.AddListener(OnOperationCancelButtonClick);




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
                var toCancelBTNObject = GameObjectHelpers.FindGameObject(toPage, "CancelBTN")?.GetComponent<Button>();
                toCancelBTNObject.onClick.AddListener(OnOperationCancelButtonClick);
                toNextBTNObject.onClick.AddListener(ToButtonClick);

                _toGrid = _mono.gameObject.AddComponent<GridHelper>();
                _toGrid.OnLoadDisplay += OnLoadToGrid;
                _toGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, toPage, _startColor, _hoverColor, OnButtonClick);
                _toGrid.DrawPage();

                var toInputField = InterfaceHelpers.FindGameObject(toPage, "InputField");
                var toText = InterfaceHelpers.FindGameObject(toInputField, "Placeholder")?.GetComponent<Text>();
                toText.text = AuxPatchers.SearchForItemsMessage();

                var toSearchField = toInputField.AddComponent<SearchField>();
                toSearchField.OnSearchValueChanged += OnToSearchValueChanged;

                #endregion
                
                #region Items Page

                var itemPage = GameObjectHelpers.FindGameObject(gameObject, "ItemPage");

                var itemNextBTNObject = GameObjectHelpers.FindGameObject(itemPage, "Button")?.GetComponent<Button>();
                itemNextBTNObject.onClick.AddListener(ItemButtonClick);
                var itemsCancelBTNObject = GameObjectHelpers.FindGameObject(itemPage, "CancelBTN")?.GetComponent<Button>();
                itemsCancelBTNObject.onClick.AddListener(OnOperationCancelButtonClick);

                _itemTechTypeGrid = _mono.gameObject.AddComponent<GridHelper>();
                _itemTechTypeGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemTechTypeGrid.Setup(18, DSSModelPrefab.OperatorItemPrefab, itemPage, _startColor, _hoverColor, OnButtonClick);
                _itemTechTypeGrid.DrawPage();

                var itemInputField = InterfaceHelpers.FindGameObject(itemPage, "InputField");
                var itemText = InterfaceHelpers.FindGameObject(itemInputField, "Placeholder")?.GetComponent<Text>();
                itemText.text = AuxPatchers.SearchForItemsMessage();

                var itemSearchField = itemInputField.AddComponent<SearchField>();
                itemSearchField.OnSearchValueChanged += OnSearchItemValueChanged;

                #endregion

                #region MessageBox

                var messageBox = GameObjectHelpers.FindGameObject(gameObject, "MessageBox");
                _messageBoxDialog =  messageBox.AddComponent<FCSMessageBoxDialog>();
                _messageBoxDialog.OnConfirmButtonClick += id =>
                {
                    switch (id)
                    {
                        case "cancel":
                            GoToPage(OperatorPages.Operations);
                            ResetData();
                            break;
                        case "itemCancel":
                            GoToPage(OperatorPages.AutoCraft);
                            ResetData();
                            break;
                        case "craft":
                            GoToPage(OperatorPages.AutoCraft);
                            _craftTechType = TechType.None;
                            break;
                    }
                };

                #endregion

            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error: {e.Message} | StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }
        
        private void ResetData()
        {
            _fromCurrentSearchString = _itemCurrentSearchString = _toCurrentSearchString = string.Empty;
            _fromDevice = _toDevice = null;
            _itemTechType = TechType.None;
            _craftTechType = TechType.None;

        }
        
        internal void GoToPage(OperatorPages page)
        {
            _mono.AnimationHandler.SetIntHash(_page,(int) page);
        }

        internal OperatorPages GetCurrentPage()
        {
            return (OperatorPages)_mono.AnimationHandler.GetIntHash(_page);
        }

        internal void Refresh()
        {
#if DEBUG
            QuickLogger.Debug($"Refreshing Operator: {_mono?.GetPrefabID()}", true);
#endif
            _operationsGrid?.DrawPage();
            _autoCraftingGrid.DrawPage();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "ConnectionsBTN":
                    GoToPage(OperatorPages.Operations);
                    _mono.Manager.LoadOperationSaveData();
                    break;
                case "AutoCraftBTN":
                    GoToPage(OperatorPages.AutoCraft);
                    break;
                case "HomeBTN":
                    GoToPage(OperatorPages.Home);
                    break;
                case "AddBTN":
                    GoToPage(OperatorPages.From);
                    _fromGrid?.DrawPage();
                    break;
                case "ItemsNextButton":
                    GoToPage(OperatorPages.To);
                    break;
                case "ToFinishButton":
                    GoToPage(OperatorPages.Operations);
                    break;
                case "VisibilityBTN":
                    GoToPage(OperatorPages.Visibility);
                    _visibilityGrid?.DrawPage();
                    break;
                case "OperatorDisplayBTN":
                    var operatorData = (OperatorButtonData)tag;
                    if (GetCurrentPage() == OperatorPages.From)
                    {
                        _fromDevice = operatorData.Button.IsChecked() ? operatorData.Connectable : null;

                    }
                    else if (GetCurrentPage() == OperatorPages.To)
                    {
                        _toDevice = operatorData.Button.IsChecked() ? operatorData.Connectable : null;
                    }

                    RefreshOperatorButtons(((OperatorButtonData)tag).Button);
                    break;
                case "ItemTechBTN":
                    var operatorItemData = (OperatorButtonData)tag;
                    _itemTechType = operatorItemData.Button.IsChecked() ? operatorItemData.TechType : TechType.None;
                    RefreshOperatorButtons(((OperatorButtonData)tag).Button);
                    break;
                case "VisibilityItemBTN":
                    var visibilityData = (OperatorButtonData)tag;
                    visibilityData.Connectable.IsVisible = visibilityData.Button.IsChecked();
                    Mod.OnBaseUpdate?.Invoke();
                    break;
                case "AddCraftBTN":
                    GoToPage(OperatorPages.CraftingItems);
                    _autocraftItemTechTypeGrid?.DrawPage();
                    break;
                case "AutoCraftItemTechBTN":
                    var autocraftItemData = (OperatorButtonData)tag;
                    _craftTechType = autocraftItemData.Button.IsChecked() ? autocraftItemData.TechType : TechType.None;
                    RefreshOperatorButtons(((OperatorButtonData)tag).Button);
                    break;
            }
        }
    }
}
