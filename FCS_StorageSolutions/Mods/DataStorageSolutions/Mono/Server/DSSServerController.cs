﻿using System;
using System.Collections.Generic;
using System.Text;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Spawnable;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server
{
    internal class DSSServerController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private FCSStorage _storageContainer;
        private Text _storageAmount;
        private InterfaceInteraction _interactionHelper;
        private ProtobufSerializer _serializer;
        private DSSServerDataEntry _savedData;
        private Rigidbody _rb;
        private BoxCollider[] _colliders;
        private string _rackSlot;
        private IDSSRack _rackController;
        private HashSet<Filter> _filteringSettings;
        private string _currentBase;
        private bool _isVisible;
        private DSSSlotController _slot;
        private Renderer[] _renders;
        internal const int MAXSTORAGE = 48;
        public bool IsBeingFormatted { get; set; }
        public bool IsFiltered => _filteringSettings != null && _filteringSettings.Count > 0;
        public override bool BypassRegisterCheck => true;

        private void Update()
        {

        }

        private void OnEnable()
        {
            GetPrefabID();
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_isFromSave)
            {
                if (_savedData == null)
                {
                    ReadySaveData();
                }

                _isFromSave = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Mod.UnRegisterServer(this);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override IFCSStorage GetStorage()
        {
            return _storageContainer;
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            IsConstructed = true;
            TabID = DSSServerSpawnable.DSSServerTabID;
            _storageAmount = gameObject.GetComponentInChildren<Text>();

            _rb = gameObject.GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<BoxCollider>();
            _renders = GetComponentsInChildren<Renderer>();
            if (_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponent<FCSStorage>();
                _storageContainer.SlotsAssigned = 48;
                _storageContainer.ItemsContainer.onAddItem += item =>
                {
                    OnAddItem?.Invoke(this,item);
                };

                _storageContainer.ItemsContainer.onRemoveItem += item =>
                {
                    OnRemoveItem?.Invoke(this, item);
                };
            }
            
            Mod.RegisterServer(this);
            
            InvokeRepeating(nameof(FindBaseManager),1f,1f);

            IsInitialized = true;
        }
        
        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "ItemBTN":
                    var size = TechDataHelpers.GetItemSize((TechType)arg2);
                    if (Inventory.main.HasRoomFor(size.x, size.y))
                    {
                        FCS_AlterraHub.Helpers.PlayerInteractionHelper.GivePlayerItem(_storageContainer.ItemsContainer.RemoveItem((TechType)arg2));
                    }
                    break;
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ====== De-Serializing Server ===== //");
            _isFromSave = true;
            _serializer = serializer;

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_savedData != null)
            {
                QuickLogger.Debug($"De-Serializing Server: {GetPrefabID()}");
                _currentBase = _savedData.CurrentBase;

                if (_savedData.SaveVersion.Equals("1.0"))
                {
                    //_storageContainer.RestoreItems(serializer, _savedData.Data);
                }

                if (_savedData.ServerFilters != null)
                {
                    _filteringSettings = _savedData.ServerFilters;
                }
            }
            _isFromSave = false;
            QuickLogger.Debug("// ====== De-Serializing Server ===== //");
        }
        
        public override bool CanDeconstruct(out string reason)
        {
            //NOT USED
            reason = String.Empty;
            return false;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            //NOT USED
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (_savedData == null)
            {
                _savedData = new DSSServerDataEntry();
            }

            _savedData.ID = GetPrefabID();
            //_savedData.Data = _storageContainer.Save(serializer);
            
            if (_rackController != null)
            {
                _savedData.RackSlot = _rackSlot;
                _savedData.RackSlotUnitID = _rackController.UnitID;
                _savedData.CurrentBase = _currentBase;
            }

            
            _savedData.IsBeingFormatted = IsBeingFormatted;
            _savedData.ServerFilters = _filteringSettings;

            newSaveData.DSSServerDataEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetDSSServerSaveData(GetPrefabID());
        }

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;

            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange)
            {
                main.SetIcon(HandReticle.IconType.Default);
                return;
            }

            main.SetTextRaw(HandReticle.TextType.Hand, AuxPatchers.OpenAlterraStorage());
            main.SetIcon(HandReticle.IconType.Hand);
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            return _storageContainer.ItemsContainer.count + 1 < MAXSTORAGE;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            var result = _storageContainer.AddItemToContainer(item);

            if (result)
            {
                if(_rackController == null)
                {
                    QuickLogger.Debug("Rack Controller is null");
                }

                _rackController?.UpdateStorageCount();
            }

            return result;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(1, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            var result =  _storageContainer.ItemsContainer.RemoveItem(techType);
            
            if (result)
            {
                _rackController?.UpdateStorageCount();
            }

            return result;
        }

        public bool ContainsItem(TechType techType)
        {
            return _storageContainer.ItemsContainer.Contains(techType);
        }

        public void DockServer(DSSSlotController slot, IDSSRack controller)
        {
            QuickLogger.Debug($"DockServer: {GetPrefabID()}");
            Initialize();
            _rackSlot = slot.GetSlotName();
            _rackController = controller;
            _rb.isKinematic = true;
            if (controller?.Manager?.BaseID != null)
            {
                _currentBase = controller.Manager.BaseID;
                FindBaseManager();
            }

            foreach (BoxCollider bc in _colliders)
            {
                bc.isTrigger = true;
            }

            foreach (Renderer rd in _renders)
            {
                rd.enabled = false;
            }

            AlterraHub.ApplyShadersV2(gameObject);
            gameObject.SetActive(true);
        }

        public override bool IsVisible => GetIsVisible();

        private bool GetIsVisible()
        {
            if (_rackController == null) return false;
            return _rackController.IsVisible;
        }
        
        private void FindBaseManager()
        {
            if (Manager == null && !string.IsNullOrEmpty(_currentBase))
            {
                Manager = BaseManager.FindManager(_currentBase);
            }
        }
        
        public void UnDockServer()
        {
            Manager = null;
            _currentBase = string.Empty;
            _rackSlot = string.Empty;
            _rackController = null;
            
            foreach (Renderer rd in _renders)
            {
                rd.enabled = true;
            }

            foreach (BoxCollider bc in _colliders)
            {
                bc.isTrigger = false;
            }
        }

        public int GetCount()
        {
            return _storageContainer.GetCount();
        }

        public bool HasSpace(int amount)
        {
            return _storageContainer.GetCount() + amount < MAXSTORAGE;
        }

        public bool IsFull()
        {
            return GetCount() >= MAXSTORAGE;
        }

        public int GetFreeSpace()
        {
            return MAXSTORAGE - GetCount();
        }

        public bool IsTechTypeAllowed(TechType techType)
        {
            if (!IsFiltered) return true;
            bool result = false;
            
            foreach (Filter filter in _filteringSettings)
            {
                if (filter.HasTechType(techType))
                {
                    result = true;
                }
            }
            return result;
        }

        public override IEnumerable<KeyValuePair<TechType, int>> GetItemsWithin()
        {
            return _storageContainer.GetItemsWithin();
        }

        public override int GetItemCount(TechType techType)
        {
            return _storageContainer.ItemsContainer.GetCount(techType);
        }

        public bool HasItem(TechType techType)
        {
            return _storageContainer.ItemsContainer.GetCount(techType) > 0;
        }

        public float GetPercentage()
        {
            if (_storageContainer == null) return 0f;
            return (float)_storageContainer.GetCount() / MAXSTORAGE;
        }

        public HashSet<Filter> GetFilters()
        {
            return _filteringSettings ?? (_filteringSettings = new HashSet<Filter>());
        }

        public void AddFilter(Filter filter)
        {
            if (_filteringSettings == null)
            {
                _filteringSettings = new HashSet<Filter>();
            }
            if (filter != null)
            {
                _filteringSettings.Add(filter);
            }
        }

        public void RemoveFilter(Filter curFilter)
        {
            foreach (Filter filter in _filteringSettings)
            {
                if (filter.IsSame(curFilter))
                {
                    _filteringSettings.Remove(filter);
                    break;
                }
            }
        }

        public string FormatFiltersData()
        {
            var sb = new StringBuilder();

            foreach (Filter filter in _filteringSettings)
            {
                sb.Append($"{filter.GetString()},");
            }

            return sb.ToString();
        }

        public DSSSlotController GetSlot()
        {
            return _slot;
        }
    }
}