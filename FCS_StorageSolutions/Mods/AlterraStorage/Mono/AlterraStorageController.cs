using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.Mods.AlterraStorage.Mono
{
    internal class AlterraStorageController : FcsDevice, IFCSSave<SaveData>, IHandTarget, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private AlterraStorageDataEntry _savedData;
        private bool _isFromSave;
        private DumpContainer _dumpContainer;
        private FCSStorage _storageContainer;
        private GridHelperV2 _inventoryGrid;
        private bool _isBeingDestroyed;
        private InterfaceInteraction _interactionHelper;
        private MotorHandler _motorHandler;
        private ProtobufSerializer _serializer;
        private Text _storageAmount;
        private const int MAXSTORAGE = 200;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public int GetContainerFreeSpace => MAXSTORAGE - _storageContainer.GetCount();
        public bool IsFull => _storageContainer.GetCount() >= MAXSTORAGE;
        private readonly List<InventoryButton> _inventoryButtons = new List<InventoryButton>();


        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraStorageTabID, Mod.ModName);
            _storageContainer.CleanUpDuplicatedStorageNoneRoutine();
            Manager.AlertNewFcsStoragePlaced(this);
        }

        private void Update()
        {

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

                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor(), ColorTargetMode.Both);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        #endregion
        
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
            IsVisible = true;

            _storageAmount = GameObjectHelpers.FindGameObject(gameObject, "StorageAmount").GetComponent<Text>();

            foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var invButton = invItem.gameObject.EnsureComponent<InventoryButton>();
                invButton.ButtonMode = InterfaceButtonMode.Background;
                invButton.BtnName = "InventoryBTN";
                invButton.OnButtonClick += OnButtonClick;
                _inventoryButtons.Add(invButton);
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainer>();
                _dumpContainer.Initialize(transform,AuxPatchers.AlterraStorageDumpContainerTitle(),this);
                _dumpContainer.OnDumpContainerClosed += () =>
                {

                };
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_storageContainer == null)
            {
                _storageContainer = gameObject.AddComponent<FCSStorage>();
                _storageContainer.Initialize( MAXSTORAGE);
                _storageContainer.ItemsContainer.onAddItem += item =>
                {
                    _inventoryGrid.DrawPage();
                    UpdateStorageCount();
                };

                _storageContainer.ItemsContainer.onRemoveItem += item =>
                {
                    _inventoryGrid.DrawPage();
                    UpdateStorageCount();
                };
            }
            
            if (_motorHandler == null)
            {
                _motorHandler = GameObjectHelpers.FindGameObject(gameObject, "radar").AddComponent<MotorHandler>();
                _motorHandler.Initialize(30);
                _motorHandler.Start();
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            _inventoryGrid = gameObject.EnsureComponent<GridHelperV2>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(12, gameObject, Color.gray, Color.white, OnButtonClick);

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial,gameObject,2f);

            UpdateStorageCount();

            InvokeRepeating(nameof(RefreshUI), .5f, .5f);

            IsInitialized = true;
        }

        public const string InventoryPoolTag = "AlterraInventoryTag";

        public override void RefreshUI()
        {
           _inventoryGrid?.DrawPage();
        }

        private void UpdateStorageCount()
        {
            _storageAmount.text = AuxPatchers.AlterraStorageAmountFormat(_storageContainer.GetCount(), MAXSTORAGE);
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "InventoryBTN":
                    var size = CraftData.GetItemSize((TechType) arg2);
                    if (Inventory.main.HasRoomFor(size.x,size.y))
                    {
                        FCS_AlterraHub.Helpers.PlayerInteractionHelper.GivePlayerItem(RemoveItemFromContainer((TechType)arg2));
                    }
                    break;
            }
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _storageContainer == null) return;
                var grouped = _storageContainer.GetItems();
                if (grouped == null) return;
                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }
                for (int i = data.EndPosition; i < data.MaxPerPage - 1; i++)
                {
                    _inventoryButtons[i].Reset();
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    _inventoryButtons[i].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

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

            _serializer = serializer;

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
            _storageContainer.RestoreItems(_serializer, _savedData.Data);
            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_storageContainer?.GetCount() > 0)
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
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.Data = _storageContainer.Save(serializer);
            newSaveData.AlterraStorageDataEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraStorageSaveData(GetPrefabID());
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
            if (!_interactionHelper.IsInRange && IsInitialized && IsConstructed)
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
            return _storageContainer.AddItem(item);
        }

        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return _storageContainer.GetCount() + _dumpContainer.GetCount() + 1 <= MAXSTORAGE;
        }

        public bool IsAllowedToRemoveItems()
        {
            return true;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return _storageContainer.ItemsContainer.RemoveItem(techType);
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
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

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override int GetMaxStorage()
        {
            return MAXSTORAGE;
        }
    }

    internal class InventoryButton : InterfaceButton
    {
        private uGUI_Icon _icon;
        private Text _amount;

        private void Initialize()
        {
            if (_icon == null)
            {
                _icon = gameObject.FindChild("Icon").EnsureComponent<uGUI_Icon>();
            }

            if (_amount == null)
            {
                _amount = gameObject.GetComponentInChildren<Text>();
            }
        }

        internal void Set(TechType techType, int amount)
        {
            Initialize();
            Tag = techType;
            _amount.text = amount.ToString();
            _icon.sprite = SpriteManager.Get(techType);
            Show();
        }

        internal void Reset()
        {
            Initialize();
            _amount.text = "";
            _icon.sprite = SpriteManager.Get(TechType.None);
            Tag = null;
            Hide();
        }

        internal void Hide()
        {
            gameObject.SetActive(false);
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }
    }

    internal class InterfaceInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsInRange { get; set; }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsInRange = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsInRange = false;
        }
    }
}
