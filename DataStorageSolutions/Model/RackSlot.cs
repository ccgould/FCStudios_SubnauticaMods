using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Mono;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Objects;
using UnityEngine;
using UWE;

namespace DataStorageSolutions.Model
{
    internal class RackSlot
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly DSSRackController _mono;
        private DSSServerController _server;

        internal readonly int Id;
        internal readonly Transform Slot;
        private Pickupable _pickupable;
        private ServerHitController _hitController;
        internal bool IsOccupied => _server != null;
        
        private bool FilterCrossCheck(TechType techType)
        {
            foreach (Filter filter in _server.GetFilters())
            {
                if (filter.IsCategory() && filter.IsTechTypeAllowed(techType))
                {
                    return true;
                }
            }

            foreach (var filter in _server.GetFilters())
            {
                if (!filter.IsCategory() && filter.IsTechTypeAllowed(techType))
                {
                    return true;
                }
            }

            return false;
        }
        
        private string FormatData()
        {
            _sb.Clear();

            _sb.Append(string.Format(AuxPatchers.FiltersCheckFormat(), _server != null && _server.GetFilters().Any()));
            _sb.Append(Environment.NewLine);
            var items = _server.GetItemsWithin().ToArray();

            for (int i = 0; i < items.Length; i++)
            {
                if (i < 4)
                {
                    _sb.Append($"{Language.main.Get(items[i].Key)} x{items[i].Value}");
                    _sb.Append(Environment.NewLine);
                }
            }

            return _sb.ToString();
        }

        internal RackSlot(DSSRackController controller, int id, Transform slot)
        {
            _mono = controller;
            Id = id;
            Slot = slot;
        }
        
        internal void AddItemToServer(InventoryItem item)
        {
            _server.AddItemToContainer(item);
            _mono.Manager.StorageManager.AddToTrackedItems(item.item.GetTechType());
            _mono.AddItemToRackItemTracker(item.item.GetTechType());
            _mono.UpdateScreen();
        }

        internal Pickupable RemoveItemFromServer(TechType techType)
        {
            var pickup =  _server.RemoveItemFromContainer(techType, 1);
            _mono.Manager.StorageManager.RemoveFromTrackedItems(techType);
            _mono.RemoveFromRackTrackedItems(techType);
            _mono.UpdateScreen();
            return pickup;
        }
        
        internal bool IsFull()
        {
            return _server.IsFull;
        }
        
        internal bool IsAllowedToAdd(TechType techType)
        {
            if (!IsOccupied || IsFull()) return false;

            QuickLogger.Debug($"Slot ID: {Id}",true);
            QuickLogger.Debug($"Filter Cross Check: {FilterCrossCheck(techType)}",true);
            QuickLogger.Debug($"Has Filters Check: {!_server.HasFilters()}",true);

            if (!_server.HasFilters()) return true;
            var filterCheckResult = FilterCrossCheck(techType);
            return filterCheckResult;
        }
        
        internal int GetItemCount(TechType techType)
        {
            return _server?.GetItemsWithin()[techType] ?? 0;
        }

        internal int SpaceAvailable()
        {
            if (_server == null) return 0;
            return (int) _server?.GetContainerFreeSpace;
        }

        internal bool CanHoldAmount(int amount)
        {
            return _server != null && _server.CanBeStored(amount);
        }

        internal void ConnectServer(InventoryItem server)
        {
            _server = server.item.gameObject.GetComponent<DSSServerController>();
            _server.ConnectToDevice(_mono.Manager,Id);
            _pickupable = server.item.gameObject.GetComponent<Pickupable>();
            TrackServerItems(_server.GetItemsWithin());
            _hitController = server.item.gameObject.GetComponentInChildren<ServerHitController>();
            _hitController.GetAdditionalString += FormatData;
            _hitController.OnButtonClick += OnServerHitClick;
            _mono.Manager.RegisterServerInBase(_server);
            DSSHelpers.MoveServerToSlot(server.item, Slot, Slot.transform);
            BaseManager.UpdateGlobalTerminals();
        }

        private void TrackServerItems(Dictionary<TechType, int> itemsWithin)
        {
            foreach (KeyValuePair<TechType, int> item in itemsWithin)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    AddToTrackedItems(item.Key);
                }
            }
        }

        private void RemoveServerItemsFromTracker(Dictionary<TechType, int> itemsWithin)
        {
            foreach (KeyValuePair<TechType, int> item in itemsWithin)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    RemoveFromTrackedItems(item.Key);
                }
            }
        }

        private void RemoveFromTrackedItems(TechType techType)
        {
            if (_mono.GetTrackedItems().ContainsKey(techType))
            {
                _mono.GetTrackedItems()[techType] -= 1;

                if (_mono.GetTrackedItems()[techType] <= 0)
                {
                    _mono.GetTrackedItems().Remove(techType);
                }
            }

            _mono.Manager.StorageManager.RemoveFromTrackedItems(techType);
        }

        private void AddToTrackedItems(TechType techType)
        {
            if (_mono.GetTrackedItems().ContainsKey(techType))
            {
                _mono.GetTrackedItems()[techType] += 1;
            }
            else
            {
                _mono.GetTrackedItems().Add(techType, 1);
            }
            _mono.Manager.StorageManager.AddToTrackedItems(techType);
        }

        internal void DisconnectFromRack()
        {
            RemoveServerItemsFromTracker(_server.GetItemsWithin());
            _mono.Manager.UnRegisterServerFromBase(_server);
            _server.DisconnectFromDevice();
            _server = null;
            _pickupable = null;
            _hitController.GetAdditionalString = null;
            _hitController.OnButtonClick = null;
            _hitController = null;
            _mono.DisplayManager.UpdateContainerAmount();
            BaseManager.UpdateGlobalTerminals();
        }

        private void OnServerHitClick(string s, object obj)
        {
            DSSHelpers.GivePlayerItem(_pickupable);
            DisconnectFromRack();
        }

        internal int GetTotal()
        {
            return _server.GetTotal();
        }

        internal bool HasFilters()
        {
            return _server.HasFilters();
        }

        public bool HasItem(TechType techType)
        {
            return _server.HasItem(techType);
        }

        public DSSServerController GetConnectedServer()
        {
            return _server;
        }
    }
}