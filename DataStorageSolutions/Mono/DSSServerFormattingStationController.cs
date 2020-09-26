using System;
using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Managers;
using FCSTechFabricator.Objects;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerFormattingStationController : DataStorageSolutionsController, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private int _slotState;
        private HashSet<Filter> _filters = new HashSet<Filter>();
        private GameObject _slot;
        private Pickupable _server;
        private DSSServerController _controller;
        public override BaseManager Manager { get; set; }
        public ColorManager ColorManager { get; private set; }
        public DSSServerFormattingStationDisplay DisplayManager { get; private set; }
        public AnimationManager AnimationManager { get; private set; }
        public override DumpContainer DumpContainer { get; set; }
        public int GetContainerFreeSpace => _server != null ? 0 : 1;
        public bool IsFull => _server != null;
        Action<int, int> IFCSStorage.OnContainerUpdate { get; set; }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_fromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }
                
                var items = _savedData.ServerData;
                if (items != null)
                {
                    //TODO if Failed give player back item and display message
                    DisplayManager.GoToPage(FilterPages.FilterPage);
                    CreateAndAddNewServer(items);
                }
                else
                {
                    GetServer();
                }
            }
        }

        private void GetServer()
        {
            foreach (UniqueIdentifier uniqueIdentifier in gameObject.GetComponentsInChildren<UniqueIdentifier>(true))
            {
                var pickupable = uniqueIdentifier.gameObject.GetComponent<Pickupable>();
                if (pickupable != null && pickupable.GetTechType() != Mod.ServerFormattingStationClassID.ToTechType())
                {
                    QuickLogger.Debug($"Found item {pickupable?.GetTechType().ToString()}");
                    DSSHelpers.MoveItemToPosition(pickupable,GetSlot(),gameObject.transform);
                    ConnectServer(pickupable);
                    DisplayManager?.GoToPage(FilterPages.FilterPage);
                    DisplayManager?.UpdatePages();
                    break;
                }
            }
        }

        private Transform GetSlot()
        {
            if (_slot == null)
            {
                _slot = GameObjectHelpers.FindGameObject(gameObject, "Server");
            }

            return _slot.transform;
        }

        private void ConnectServer(Pickupable pickupable)
        {
            _server = pickupable;
            _controller = pickupable.gameObject.GetComponent<DSSServerController>();
            _controller.ConnectToDevice(Manager, 0);
            _filters = _controller.GetFilters();
        }
        
        private void CreateAndAddNewServer(HashSet<ObjectData> items)
        {
            try
            {
                var server = Mod.ServerClassID.ToTechType().ToInventoryItem();
                var controller = server.item.gameObject.GetComponent<DSSServerController>();

                foreach (ObjectData data in items)
                {
                    controller.AddItemToContainer(new InventoryItem(data.ToPickable()));
                }

                var result = AddItemToContainer(server);
                if (result)
                {
                    DisplayManager?.GoToPage(FilterPages.FilterPage);
                    DisplayManager?.UpdatePages();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Message: {e.Message} || StackTrace: {e.StackTrace}");
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
        }

        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabId))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabId = id != null ? id.Id : string.Empty;
            }

            return _prefabId;
        }

        public override void Initialize()
        {
            QuickLogger.Debug("Initialize Formatter", true);

            _slotState = Animator.StringToHash("SlotState");

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSServerFormattingStationDisplay>();
                DisplayManager.Setup(this);
            }

            if (DumpContainer == null)
            {
                DumpContainer = new DumpContainer();
                DumpContainer.Initialize(transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.CannotBeStored(), this, 1, 1);
            }

            IsInitialized = true;
        }

        public override void Save(SaveData newSaveData)
        {
            //if (!IsInitialized || !IsConstructed) return;

            //var id = GetPrefabIDString();

            //if (_savedData == null)
            //{
            //    _savedData = new SaveDataEntry();
            //}

            //_savedData.ID = id;
            //_savedData.ServerData = _items;
            //newSaveData.Entries.Add(_savedData);
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            //QuickLogger.Debug("In OnProtoSerialize");

            //if (!Mod.IsSaving())
            //{
            //    var id = GetPrefabIDString();
            //    QuickLogger.Info($"Saving {id}");
            //    Mod.Save();
            //    QuickLogger.Info($"Saved {id}");
            //}
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_server != null)
            {
                reason = AuxPatchers.HasItemsMessage();
                return false;
            }
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public bool CanBeStored(int amount,TechType techType = TechType.None)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            QuickLogger.Debug($"Adding Server to Formatter {item}",true);

            _controller = item.item.GetComponentInChildren<DSSServerController>();

            if (_controller != null)
            {
                ConnectServer(item.item);
                DisplayManager.GoToPage(FilterPages.FilterPage);
                DisplayManager.UpdatePages();
                DSSHelpers.MoveItemToPosition(item.item, GetSlot(), gameObject.transform);
            }
            var pda = Player.main.GetPDA();
            if(pda.isOpen)
            {
                pda.Close();
            }
            return _controller != null;
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (AnimationManager.GetBoolHash(_slotState))
            {
                return false;
            }

            return pickupable.GetTechType() == QPatch.Server.TechType;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            var result = DSSHelpers.GivePlayerItem(_server);
            
            if (result)
            {
                _controller.DisconnectFromDevice();
                _controller = null;
                _server = null;
                _filters = new HashSet<Filter>();
            }

            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        internal void AddFilter(Filter filter)
        {
            if (!_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        internal void RemoveFilter(Filter filter)
        {
            if (_filters.Contains(filter))
            {
                _filters.Remove(filter);
            }
        }

        public HashSet<Filter> GetFilters()
        {
            return _filters;
        }
    }
}