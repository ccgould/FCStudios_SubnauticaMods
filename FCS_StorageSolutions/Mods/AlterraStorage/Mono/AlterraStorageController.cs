using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.AlterraStorage.Mono
{
    internal class AlterraStorageController : FcsDevice, IFCSSave<SaveData>, IHandTarget, IFCSDisplay,IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private AlterraStorageDataEntry _savedData;
        private bool _isFromSave;
        private DumpContainer _dumpContainer;
        private StorageContainer _storageContainer;
        private GridHelperV2 _inventoryGrid;
        private bool _isBeingDestroyed;
        private InterfaceInteraction _interactionHelper;
        private Text _storageAmount;
        private const int MAXSTORAGE = 200;

        public override bool IsVisible => true;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }

        public int GetContainerFreeSpace => MAXSTORAGE - _storageContainer.container.count; 
        public bool IsFull => _storageContainer.container.count >= MAXSTORAGE;
        private readonly List<InventoryButton> _inventoryButtons = new();
        private NameController _nameController;
        private Text _labelText;
        private PaginatorController _paginatorController;
        private bool _subscribed;

        public override StorageType StorageType => StorageType.OtherStorage;

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraStorageTabID, Mod.ModPackID);
            Manager.AlertNewFcsStoragePlaced(this);
            UpdateStorageCount();
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                    _labelText.text = _savedData.StorageName;
                    _nameController.SetCurrentName(_savedData.StorageName);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _isBeingDestroyed = true;
            if (_storageContainer?.container != null)
            {
                _storageContainer.container.onAddItem -= OnStorageUpdate;
                _storageContainer.container.onRemoveItem -= OnStorageUpdate;
            }
        }

        #endregion

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override IFCSStorage GetStorage()
        {
            return this;
        }

        private void LateUpdate()
        {
            Subscribe(true);
        }

        public override void Initialize()
        {
            
            _storageAmount = GameObjectHelpers.FindGameObject(gameObject, "StorageAmount").GetComponent<Text>();

            var grid = GameObjectHelpers.FindGameObject(gameObject, "Grid"); 
            if (grid != null)
            {
                foreach (Transform invItem in grid.transform)
                {
                    var invButton = invItem.gameObject.EnsureComponent<InventoryButton>();
                    invButton.ButtonMode = InterfaceButtonMode.Background;
                    invButton.BtnName = "InventoryBTN";
                    invButton.OnButtonClick += OnButtonClick;
                    _inventoryButtons.Add(invButton);
                }
            }


            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainer>();
                _dumpContainer.Initialize(transform,AuxPatchers.AlterraStorageDumpContainerTitle(),this);

            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BaseOpaqueExterior);
            }

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.GetComponent<StorageContainer>();
                _storageContainer.enabled = false;
            }


            _labelText = GameObjectHelpers.FindGameObject(gameObject, "ContainerLabel").GetComponent<Text>();
            var label = GameObjectHelpers.FindGameObject(gameObject, "ContainerLabel").AddComponent<InterfaceButton>();
            label.ButtonMode = InterfaceButtonMode.TextColor;
            label.TextLineOne = AuxPatchers.Rename();
            label.TextComponent = _labelText;
            label.OnButtonClick += (s, o) => { _nameController.Show(); };

            if (_nameController == null)
            {
                _nameController = gameObject.AddComponent<NameController>();
                _nameController.Initialize(AuxPatchers.Submit(),AuxPatchers.RenameAlterraStorage());
                _nameController.OnLabelChanged += OnLabelChanged;
                _nameController.SetCurrentName("Remote Storage");
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            _inventoryGrid = gameObject.EnsureComponent<GridHelperV2>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(12, gameObject, Color.gray, Color.white, OnButtonClick);

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            UpdateStorageCount();

            InvokeRepeating(nameof(RefreshUI), .5f, .5f);

            IsInitialized = true;
        }

        private void OnDisable()
        {
            Subscribe(false);
            if (_storageContainer != null)
            {
                _storageContainer.enabled = false;
            }
        }

        private void Subscribe(bool state)
        {
            if (_subscribed == state)
            {
                return;
            }
            if (_storageContainer.container == null)
            {
                QuickLogger.Debug("AlterraStorager.Subscribe(): container null; will retry next frame");
                return;
            }
            if (_subscribed)
            {
                _storageContainer.container.onAddItem -= OnStorageUpdate;
                _storageContainer.container.onRemoveItem -= OnStorageUpdate;
                _storageContainer.container.isAllowedToAdd = null;
                _storageContainer.container.isAllowedToRemove = null;
            }
            else
            {
                _storageContainer.container.onAddItem += OnStorageUpdate;
                _storageContainer.container.onRemoveItem += OnStorageUpdate;
                _storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
            }
            _subscribed = state;
        }

        private void OnStorageUpdate(InventoryItem item)
        {
            _inventoryGrid.DrawPage();
            UpdateStorageCount();
        }


        private void OnLabelChanged(string arg1, NameController arg2)
        {
            _labelText.text = arg1;
        }

        public const string InventoryPoolTag = "AlterraInventoryTag";

        public override void RefreshUI()
        {
           _inventoryGrid?.DrawPage();
        }

        private void UpdateStorageCount()
        {
            _storageAmount.text = AuxPatchers.AlterraStorageAmountFormat(_storageContainer?.container?.count ?? 0, MAXSTORAGE);
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "InventoryBTN":
                    var size = CraftData.GetItemSize((TechType) arg2);
                    if (Inventory.main.HasRoomFor(size.x,size.y))
                    {
                        PlayerInteractionHelper.GivePlayerItem(RemoveItemFromContainer((TechType)arg2));
                    }
                    break;
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _storageContainer == null || _inventoryButtons == null || _inventoryGrid == null || _paginatorController == null) return;
               
                var grouped = _storageContainer.container._items;

                if (grouped == null) return;

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = 0; i < data.MaxPerPage; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                int w = 0;

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value.items.Count);
                }

                _inventoryGrid.UpdaterPaginator(grouped.Count);
                _paginatorController.ResetCount(_inventoryGrid.GetMaxPages());
            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
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
            QuickLogger.Debug("In OnProtoDeserialize");
            
            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_storageContainer?.container.count > 0)
            {
                reason = AuxPatchers.ContainerNotEmpty();
                return false;
            }
            reason = String.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraStorageDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.StorageName = _nameController.GetCurrentName();
            newSaveData.AlterraStorageDataEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraStorageSaveData(GetPrefabID());
        }

        public override void OnHandHover(GUIHand hand)
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
            if (IsInitialized && IsConstructed && _interactionHelper!= null && !_interactionHelper.IsInRange)
            {
                _dumpContainer.OpenStorage();
            }
        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            return IsFull == false;
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            try
            {
                _storageContainer.container.UnsafeAdd(item);
                return true;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }
            return false;
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return _storageContainer.container.count + _dumpContainer.GetCount() + 1 <= MAXSTORAGE;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            return _storageContainer.container.RemoveItem(techType);
        }

        public  Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return _storageContainer.container.Contains(techType);
        }

        public ItemsContainer ItemsContainer { get; set; }
        public int StorageCount()
        {
            return _storageContainer.container.count;
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override int GetMaxStorage()
        {
            return MAXSTORAGE;
        }

        public void GoToPage(int index)
        {
            _inventoryGrid.DrawPage(index);
        }

        public void GoToPage(int index, PaginatorController sender)
        {
            _inventoryGrid.DrawPage(index);
        }
    }
}
