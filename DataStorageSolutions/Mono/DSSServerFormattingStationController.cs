using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Model;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Interfaces;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSServerFormattingStationController : DataStorageSolutionsController, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private string _prefabID;
        private bool _isContructed;
        private int _slotState;
        private List<ObjectData> _items;
        private DSSServerController _controller;

        public ColorManager ColorManager { get; private set; }
        public DSSServerFormattingStationDisplay DisplayManager { get; private set; }
        public AnimationManager AnimationManager { get; private set; }
        public DumpContainer DumpContainer { get; private set; }

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
            }
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
        }

        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabID = id != null ? id.Id : string.Empty;
            }

            return _prefabID;
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
                DumpContainer.Initialize(transform, AuxPatchers.BaseDumpReceptacle(), AuxPatchers.NotAllowed(), AuxPatchers.DriveFull(), this, 4, 4);
            }

            IsInitialized = true;
        }

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
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
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            _isContructed = constructed;

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

        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public bool CanBeStored(int amount)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            _controller = item.item.GetComponent<DSSServerController>();
            _items = new List<ObjectData>(_controller.Items);
            DisplayManager.GoToPage(FilterPages.FilterPage);
            ToggleDummyServer();
            Destroy(item.item.gameObject);
            return true;
        }

        public void GivePlayerItem()
        {
            DSSHelpers.GivePlayerItem(QPatch.Server.TechType, new ObjectDataTransferData{data = _items, IsServer = true}, null);
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            if (AnimationManager.GetBoolHash(_slotState))
            {
                return false;
            }

            return pickupable.GetTechType() == QPatch.Server.TechType;
        }
    }
}
