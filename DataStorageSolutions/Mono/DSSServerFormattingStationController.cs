using System;
using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Enumerators;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Controllers;
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
        private HashSet<ObjectData> _items;
        private DSSServerController _controller;
        private List<Filter> _filters = new List<Filter>();
        public override BaseManager Manager { get; set; }
        public ColorManager ColorManager { get; private set; }
        public DSSServerFormattingStationDisplay DisplayManager { get; private set; }
        public AnimationManager AnimationManager { get; private set; }
        public override DumpContainer DumpContainer { get; set; }
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
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

                _items = _savedData.ServerData;
                if (_items != null)
                {
                    ToggleDummyServer();
                    DisplayManager.GoToPage(FilterPages.FilterPage);
                }
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
            if (!IsInitialized || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.ServerData = _items;
            newSaveData.Entries.Add(_savedData);
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabIDString();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
            }
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
            if (_items != null)
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

        public void ToggleDummyServer()
        {
            AnimationManager.SetBoolHash(_slotState,!AnimationManager.GetBoolHash(_slotState));
        }

        public bool CanBeStored(int amount,TechType techType = TechType.None)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _controller = item.item.GetComponent<DSSServerController>();

            if (_controller != null)
            {
                if(_controller.FCSFilteredStorage.Items != null)
                    _items = new HashSet<ObjectData>(_controller.FCSFilteredStorage.Items);
                if (_controller.FCSFilteredStorage.Filters != null)
                    _filters = new List<Filter>(_controller.FCSFilteredStorage.Filters);
            }

            DisplayManager.GoToPage(FilterPages.FilterPage);
            DisplayManager.UpdatePages();
            ToggleDummyServer();
            Destroy(item.item.gameObject);
            var pda = Player.main.GetPDA();
            if(pda.isOpen)
            {
                pda.Close();
            }
            return true;
        }

        public void GivePlayerItem()
        {
            var result = DSSHelpers.GivePlayerItem(QPatch.Server.TechType, new ObjectDataTransferData{data = _items,Filters= _filters, IsServer = true}, null);
            
            if (result)
            {
                _items = null;
            }

            QuickLogger.Debug($"Items result: {_items} || Result = {result}");
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

        public List<Filter> GetFilters()
        {
            return _filters;
        }
    }
}