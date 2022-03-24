using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack
{
    internal class RackConfigurationPageController : MonoBehaviour
    {
        private GameObject _homeObj;
        private GameObject _dialogObj;
        private DumpContainerSimplified _addItemDumpButton;
        private readonly HashSet<FilterItems> _filterItems = new HashSet<FilterItems>();
        private readonly HashSet<FilterDisplayItem> _filterDisplayItems = new HashSet<FilterDisplayItem>();
        private GameObject _messageObj;
        private GridHelperV2 _itemGrid;
        private readonly List<TechType> _pendingItems = new List<TechType>();
        private GridHelperV2 _filterGrid;
        private GameObject _canvas;
        private DSSSlotController _slot;
        private DSSRackBase _rackBase;
        private Button _backBtn;
        public AddFilterController AddFilterController { get; set; }
        internal enum ConfigurationPages { Home, Filter }

        private void OnEnable()
        {
            UpdateFilters();
        }

        internal void Setup(DSSRackBase rackBase)
        {
            if (FindAllComponents())
            {
                _rackBase = rackBase;
                AddFilterController = new AddFilterController();
                
                if (_addItemDumpButton == null)
                {
                    _addItemDumpButton = gameObject.AddComponent<DumpContainerSimplified>();
                    _addItemDumpButton.Initialize(transform, "Add Filter", AddFilterController);
                    AddFilterController.ItemAdded += item =>
                    {
                        _pendingItems.Add(item.item.GetTechType());
                        UpdateFilters();
                        PlayerInteractionHelper.GivePlayerItem(item);
                        GoToPage(ConfigurationPages.Filter);
                    };
                }
            }
        }

        internal void RemoveFromPending(TechType techType)
        {
            _pendingItems.Remove(techType);
        }

        public void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "CategoryBTN":
                    _addItemDumpButton.OpenStorage();
                    break;
                case "ConfirmBTN":
                    foreach (FilterItems item in _filterItems)
                    {
                        _slot.AddFilter(item.GetFilter());
                    }
                    _pendingItems.Clear();
                    UpdateFilters();
                    GoToPage(ConfigurationPages.Home);
                    break;
                case "CancelBTN":
                    _pendingItems.Clear();
                    GoToPage(ConfigurationPages.Home);
                    UpdateFilters();
                    break;
            }
        }

        public bool FindAllComponents()
        {
            try
            {
                _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
                _homeObj = GameObjectHelpers.FindGameObject(gameObject, "Home");
                _dialogObj = GameObjectHelpers.FindGameObject(gameObject, "Dialog");
                _messageObj = GameObjectHelpers.FindGameObject(gameObject, "Message");
                _backBtn = GameObjectHelpers.FindGameObject(gameObject, "BackBTN").GetComponent<Button>();
                _backBtn.onClick.AddListener(() =>
                {
                    Clear();
                    _rackBase.GoToPage(DSSRackPages.Home);
                });

                foreach (Transform child in _dialogObj.FindChild("Grid").transform)
                {
                    var filterItemController = child.gameObject.EnsureComponent<FilterItems>();
                    _filterItems.Add(filterItemController);
                }

                foreach (Transform child in _homeObj.FindChild("Grid").transform)
                {
                    var filterDisplayItemController = child.gameObject.EnsureComponent<FilterDisplayItem>();
                    filterDisplayItemController.Initialize(this);
                    _filterDisplayItems.Add(filterDisplayItemController);
                }

                _itemGrid = gameObject.AddComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(4, gameObject, Color.gray, Color.white, OnButtonClick);

                _filterGrid = gameObject.AddComponent<GridHelperV2>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(4, gameObject, Color.gray, Color.white, OnButtonClick);

                var categoryBTNObj = GameObjectHelpers.FindGameObject(gameObject, "CategoryBTN");
                InterfaceHelpers.CreateButton(categoryBTNObj, "CategoryBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.AddFilter(), AuxPatchers.AddFilterDesc());

                // Confirm button
                var confirm = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN");
                InterfaceHelpers.CreateButton(confirm, "ConfirmBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);

                // Cancel button
                var cancel = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN");
                InterfaceHelpers.CreateButton(cancel, "CancelBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
        }

        private void Clear()
        {
            _slot = null;
            _pendingItems.Clear();
            foreach (FilterDisplayItem displayItem in _filterDisplayItems)
            {
                displayItem.Reset();
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                var grouped = _pendingItems;

                QuickLogger.Debug($"GetFilters: {grouped.Count}");

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _filterItems.ElementAt(i).Reset();
                }


                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _filterItems.ElementAt(i).Set(grouped.ElementAt(i));
                }

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
                var grouped = _slot?.GetFilters();

                QuickLogger.Debug($"GetFilters: {grouped?.Count}");
                QuickLogger.Debug($"GetFilters: {data.EndPosition}");

                if (grouped == null) return;

                if (data.EndPosition > grouped.Count)
                {
                    QuickLogger.Debug($"GetFilters: 0.1");
                    data.EndPosition = grouped.Count;
                    QuickLogger.Debug($"GetFilters: 0.2");
                }

                QuickLogger.Debug($"GetFilters: 1");

                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    QuickLogger.Debug($"GetFilters: 1.1");
                    _filterDisplayItems.ElementAt(i).Reset();
                    QuickLogger.Debug($"GetFilters: 1.2");
                }

                QuickLogger.Debug($"GetFilters: 2");

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    QuickLogger.Debug($"GetFilters: 2.1");
                    _filterDisplayItems.ElementAt(i).Set(grouped.ElementAt(i));
                    QuickLogger.Debug($"GetFilters: 2.2");
                }

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        public void GoToPage(ConfigurationPages page)
        {
            switch (page)
            {
                case ConfigurationPages.Home:
                    _homeObj.SetActive(true);
                    _dialogObj.SetActive(false);
                    _backBtn.gameObject.SetActive(true);
                    break;
                case ConfigurationPages.Filter:
                    _homeObj.SetActive(false);
                    _dialogObj.SetActive(true); 
                    _backBtn.gameObject.SetActive(false);
                    break;
            }
        }

        public void UpdateFilters()
        {
            _messageObj?.SetActive(_slot?.GetFilters()?.Count <= 0);
            _filterGrid?.DrawPage();
            _itemGrid?.DrawPage();
        }

        public void SetSlot(DSSSlotController slot)
        {
            _slot = slot;
        }

        public void RemoveFilter(Filter filter)
        {
            if (_slot == null) return;
            _slot.RemoveFilter(filter);
            UpdateFilters();
        }
    }
}