using System;
using System.Collections.Generic;
using FCS_AlterraHub.Mono;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Rack;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.Server
{


    //TODO Add a name feature
    internal class DSSServerController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private bool _isBeingDestroyed;
        private FCSStorage _storageContainer;
        private Text _storageAmount;
        private InterfaceInteraction _interactionHelper;
        private ProtobufSerializer _serializer;
        private DSSServerDataEntry _savedData;
        private Rigidbody _rb;
        private BoxCollider[] _colliders;
        private string _rackSlot;
        private DSSWallServerRackController _rackController;
        private const int MAXSTORAGE = 48;
        public override bool BypassRegisterCheck => true;

        private void Start()
        {
            _storageContainer.CleanUpDuplicatedStorageNoneRoutine();
        }

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
            _isBeingDestroyed = true;
            Mod.UnRegisterServer(this);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override FCSStorage GetStorage()
        {
            return _storageContainer;
        }

        public override void Initialize()
        {
            IsVisible = false;
            IsConstructed = true;

            //_storageAmount = GameObjectHelpers.FindGameObject(gameObject, "StorageAmount").GetComponent<Text>();
            
            _rb = gameObject.GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<BoxCollider>();

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.EnsureComponent<FCSStorage>();
                _storageContainer.Initialize(MAXSTORAGE);
            }
            Mod.RegisterServer(this);

            //var canvas = gameObject.GetComponentInChildren<Canvas>();
            //_interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            //UpdateStorageCount();

            IsInitialized = true;
        }

        private void UpdateStorageCount()
        {
            _storageAmount.text = AuxPatchers.AlterraStorageAmountFormat(_storageContainer.GetCount(), MAXSTORAGE);
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "ItemBTN":
                    var size = CraftData.GetItemSize((TechType)arg2);
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
            QuickLogger.Debug("In OnProtoDeserialize We made it");

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
                _storageContainer.RestoreItems(_serializer, _savedData.Data);
            }

            _isFromSave = true;
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


        private void Test()
        {
            _storageContainer.AddItem(TechType.Copper.ToInventoryItem());
        }


        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (_savedData == null)
            {
                _savedData = new DSSServerDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.Data = _storageContainer.Save(serializer);
            if (_rackController != null)
            {
                _savedData.RackSlot = _rackSlot;
                _savedData.RackSlotUnitID = _rackController.UnitID;
            }
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

            main.SetInteractText(AuxPatchers.OpenAlterraStorage());
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
            return _storageContainer.AddItem(item);
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
            return _storageContainer.ItemsContainer.RemoveItem(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return _storageContainer.ItemsContainer.Contains(techType);
        }

        public void DockServer(DSSSlotController slot, DSSWallServerRackController controller)
        {
            _rackSlot = slot.GetSlotName();
            _rackController = controller;
            _rb.isKinematic = true;
            foreach (BoxCollider bc in _colliders)
            {
                bc.isTrigger = true;
            }
            ModelPrefab.ApplyShaders(gameObject);
            gameObject.SetActive(true);
            transform.parent = slot.transform;
            transform.localPosition = Vector3.zero;
            IsVisible = true;
        }

        public void UnDockServer()
        {
            _rackSlot = string.Empty;
            _rackController = null;
            IsVisible = false;
        }
    }
}
