using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Display;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
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
    internal class RackSlot: ISlot
    {
        private readonly DSSRackController _mono;
        private DSSServerController _server;
        private Pickupable _pickupable;
        private ServerHitController _hitController;

        internal readonly int Id;
        internal readonly Transform Slot;
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
        
        internal RackSlot(DSSRackController controller, int id, Transform slot)
        {
            _mono = controller;
            Id = id;
            Slot = slot;
        }

        internal bool IsFull()
        {
            return _server.IsFull;
        }

        internal bool IsAllowedToAdd(TechType techType)
        {
            if (!IsOccupied || IsFull()) return false;

            QuickLogger.Debug($"Slot ID: {Id}", true);
            QuickLogger.Debug($"Filter Cross Check: {FilterCrossCheck(techType)}", true);
            QuickLogger.Debug($"Has Filters Check: {!_server.HasFilters()}", true);

            if (!_server.HasFilters()) return true;
            var filterCheckResult = FilterCrossCheck(techType);
            return filterCheckResult;
        }

        internal int SpaceAvailable()
        {
            if (_server == null) return 0;
            return (int)_server?.GetContainerFreeSpace;
        }

        public  void ConnectServer(Pickupable server)
        {
#if DEBUG
            QuickLogger.Debug($"Connecting Server: {server.gameObject.GetComponent<DSSServerController>()?.GetPrefabID()}");
#endif
            _server = server.gameObject.GetComponent<DSSServerController>();
            _server.ConnectToDevice(_mono.Manager, this);
            _pickupable = server;
            _hitController = server.gameObject.GetComponentInChildren<ServerHitController>();
            _hitController.OnButtonClick += OnServerHitClick;
            _mono.Manager.RegisterServerInBase(_server);
            DSSHelpers.MoveItemToPosition(server, Slot, Slot.transform);
            _mono.UpdateScreen();
        }
        
        internal void DisconnectFromRack()
        {
            _mono.Manager.UnRegisterServerFromBase(_server);
            _server.DisconnectFromDevice();
            _server = null;
            _pickupable = null;
            _hitController.OnButtonClick = null;
            _hitController = null;
            _mono.UpdateScreen();
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

        public void UpdateRackScreen()
        {
            _mono.UpdateScreen();
        }

        public ISlottedDevice GetConnectedDevice()
        {
            return _mono;
        }
    }
}