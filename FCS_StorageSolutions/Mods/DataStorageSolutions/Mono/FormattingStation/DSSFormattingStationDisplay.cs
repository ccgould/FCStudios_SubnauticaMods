﻿using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Abstract;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.FormattingStation
{
    internal class DSSFormattingStationDisplay : AIDisplay
    {
        private DSSFormattingStationController _mono;
        private DumpContainerSimplified _serverDumpButton;
        private GameObject _dropBTNObj;
        private GameObject _homeObj;
        private GameObject _dialogObj;
        private DumpContainerSimplified _addItemDumpButton;
        private readonly HashSet<FilterItems> _filterItems = new HashSet<FilterItems>();
        private readonly HashSet<FilterDisplayItem> _filterDisplayItems = new HashSet<FilterDisplayItem>();
        private GameObject _messageObj;
        private GridHelperV2 _itemGrid;
        private List<TechType> _pendingItems = new List<TechType>();
        private GridHelperV2 _filterGrid;
        private GameObject _canvas;
        public AddFilterController AddFilterController { get; set; }

        internal void Setup(DSSFormattingStationController mono)
        {
            _mono = mono;

            if(FindAllComponents())
            {
                AddFilterController = new AddFilterController();

                if (_serverDumpButton == null)
                {
                    _serverDumpButton = gameObject.AddComponent<DumpContainerSimplified>();
                    _serverDumpButton.Initialize(transform,"Server DropBox",_mono,1,1);
                }

                if (_addItemDumpButton == null)
                {
                    _addItemDumpButton = gameObject.AddComponent<DumpContainerSimplified>();
                    _addItemDumpButton.Initialize(transform, "Add Filter", AddFilterController);
                    AddFilterController.ItemAdded += item =>
                    {
                        _pendingItems.Add(item.item.GetTechType());
                        UpdateFilters();
                        PlayerInteractionHelper.GivePlayerItem(item);
                        GoToPage(FormattingStationPages.Filter);
                    };
                }
            }
        }
        
        internal void RemoveFromPending(TechType techType)
        {
            _pendingItems.Remove(techType);
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            switch (btnName)
            {
                case "DriveInsertBTN":
                    _serverDumpButton.OpenStorage();
                    break;

                case "CategoryBTN":
                    _addItemDumpButton.OpenStorage();
                    break;
                case "ConfirmBTN":
                    foreach (FilterItems item in _filterItems)
                    {
                        _mono.AddFilter(item.GetFilter());
                    }
                    _pendingItems.Clear();
                    UpdateFilters();
                    GoToPage(FormattingStationPages.Home);
                    break;
                case "CancelBTN":
                    _pendingItems.Clear();
                    GoToPage(FormattingStationPages.Home);
                    UpdateFilters();
                    break;
                case "EjectBTN":
                    _mono.UnDockServer();
                    GoToPage(FormattingStationPages.AddServer);
                    break;
            }
        }

        public override bool FindAllComponents()
        {
            try
            {
                _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;
                _homeObj = GameObjectHelpers.FindGameObject(gameObject, "Home");
                _dialogObj = GameObjectHelpers.FindGameObject(gameObject, "Dialog");
                _messageObj = GameObjectHelpers.FindGameObject(gameObject, "Message");

                QuickLogger.Debug("1");

                foreach (Transform child in _dialogObj.FindChild("Grid").transform)
                {
                    var filterItemController = child.gameObject.EnsureComponent<FilterItems>();
                    _filterItems.Add(filterItemController);
                }

                foreach (Transform child in _homeObj.FindChild("Grid").transform)
                {
                    var filterDisplayItemController = child.gameObject.EnsureComponent<FilterDisplayItem>();
                    filterDisplayItemController.Initialize(_mono);
                    _filterDisplayItems.Add(filterDisplayItemController);
                }

                QuickLogger.Debug("2");

                _itemGrid = _mono.gameObject.EnsureComponent<GridHelperV2>();
                _itemGrid.OnLoadDisplay += OnLoadItemsGrid;
                _itemGrid.Setup(4, gameObject, Color.gray, Color.white, OnButtonClick);

                _filterGrid = _mono.gameObject.EnsureComponent<GridHelperV2>();
                _filterGrid.OnLoadDisplay += OnLoadFilterGrid;
                _filterGrid.Setup(4, gameObject, Color.gray, Color.white, OnButtonClick);

                QuickLogger.Debug("3");
                var categoryBTNObj = GameObjectHelpers.FindGameObject(gameObject, "CategoryBTN");
                InterfaceHelpers.CreateButton(categoryBTNObj, "CategoryBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f, AuxPatchers.AddFilter(), AuxPatchers.AddFilterDesc());
                QuickLogger.Debug("4");
                // Drop button
                _dropBTNObj = GameObjectHelpers.FindGameObject(gameObject, "DriveInsertBTN");
                InterfaceHelpers.CreateButton(_dropBTNObj, "DriveInsertBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f,AuxPatchers.AddServer(),AuxPatchers.FormattingMachineAddServerDesc());

                // Confirm button
                var confirm = GameObjectHelpers.FindGameObject(gameObject, "ConfirmBTN");
                InterfaceHelpers.CreateButton(confirm, "ConfirmBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);

                // Cancel button
                var cancel = GameObjectHelpers.FindGameObject(gameObject, "CancelBTN");
                InterfaceHelpers.CreateButton(cancel, "CancelBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);

                // Cancel button
                var ejectBTN = GameObjectHelpers.FindGameObject(gameObject, "ExpellDriveBTN");
                InterfaceHelpers.CreateButton(ejectBTN, "EjectBTN", InterfaceButtonMode.Background,
                    OnButtonClick, Color.white, new Color(0, 1, 1, 1), 2.5f);
                

                QuickLogger.Debug("5");
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                return false;
            }

            return true;
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
                var grouped = _mono.GetFilters();

                QuickLogger.Debug($"GetFilters: {grouped?.Count}");
                QuickLogger.Debug($"GetFilters: {data.EndPosition}");

                if(grouped==null) return;

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

        public void GoToPage(FormattingStationPages page)
        {
            switch (page)
            {
                case FormattingStationPages.Home:
                    _dropBTNObj.SetActive(false);
                    _homeObj.SetActive(true);
                    _dialogObj.SetActive(false);
                    break;
                case FormattingStationPages.AddServer:
                    _dropBTNObj.SetActive(true);
                    _homeObj.SetActive(false);
                    _dialogObj.SetActive(false);
                    break;
                case FormattingStationPages.Filter:
                    _dropBTNObj.SetActive(false);
                    _homeObj.SetActive(false);
                    _dialogObj.SetActive(true);
                    break;
            }
        }

        public void UpdateFilters()
        {
            _messageObj?.SetActive(_mono?.GetFilters()?.Count <= 0);
            _filterGrid.DrawPage();
            _itemGrid.DrawPage();
        }

        public override void TurnOffDisplay()
        {
            if (_canvas.activeSelf)
            {
                _canvas.SetActive(false);
            }
        }

        public override void TurnOnDisplay()
        {
            if (!_canvas.activeSelf)
            {
                _canvas.SetActive(true);
            }
        }
    }
}