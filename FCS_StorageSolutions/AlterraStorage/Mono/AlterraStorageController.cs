using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.ObjectPooler;
using FCS_AlterraHub.Mono.OreConsumer;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.AlterraStorage.Buildable;
using FCS_StorageSolutions.Configuration;
using FCSCommon.Components;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCS_StorageSolutions.AlterraStorage.Mono
{
    internal class AlterraStorageController : FcsDevice, IFCSSave<SaveData>, IHandTarget, IFCSStorage
    {
        private bool _runStartUpOnEnable;
        private AlterraStorageDataEntry _savedData;
        private ColorManager _colorManager;
        private bool _isFromSave;
        private DumpContainer _dumpContainer;
        private FCSStorage _storageContainer;
        private GridHelperPooled _inventoryGrid;
        private bool _isBeingDestroyed;
        private InterfaceInteration _interactionHelper;
        private MotorHandler _motorHandler;
        private ProtobufSerializer _serializer;
        private Text _storageAmount;
        private ObjectPooler _pooler;
        private HashSet<InventoryButton> _trackedItems = new HashSet<InventoryButton>();
        private const int MAXSTORAGE = 200;
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public int GetContainerFreeSpace => MAXSTORAGE - _storageContainer.GetCount();
        public bool IsFull => _storageContainer.GetCount() >= MAXSTORAGE;
        
        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraStorageTabID, Mod.ModName);
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

                    _colorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor(), ColorTargetMode.Both);
                    _storageContainer.RestoreItems(_serializer,_savedData.Data);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;
        }

        #endregion

        public override void Initialize()
        {
            IsVisible = true;

            _storageAmount = GameObjectHelpers.FindGameObject(gameObject, "StorageAmount").GetComponent<Text>();

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
                _storageContainer.Initialize(MAXSTORAGE,GameObjectHelpers.FindGameObject(gameObject,"StorageRoot"));
                _storageContainer.onAddItem += item =>
                {
                    _inventoryGrid.DrawPage();
                    UpdateStorageCount();
                };
                _storageContainer.onRemoveItem += item =>
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

            if (_pooler == null)
            {
                _pooler = gameObject.AddComponent<ObjectPooler>();
                _pooler.AddPool(InventoryPoolTag, 12, ModelPrefab.InventoryItemPrefab);
                _pooler.Initialize();
            }


            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteration>();

            _inventoryGrid = gameObject.AddComponent<GridHelperPooled>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(12, _pooler, canvas.gameObject, OnButtonClick,false);

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial,gameObject,2f);

            IsInitialized = true;
        }

        public const string InventoryPoolTag = "AlterraInventoryTag";

        private void UpdateStorageCount()
        {
            _storageAmount.text = AuxPatchers.AlterraStorageAmountFormat(_storageContainer.GetCount(), MAXSTORAGE);
        }

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "ItemBTN":
                    var size = CraftData.GetItemSize((TechType) arg2);
                    if (Inventory.main.HasRoomFor(size.x,size.y))
                    {
                        FCS_AlterraHub.Helpers.PlayerInteractionHelper.GivePlayerItem(_storageContainer.RemoveItem((TechType)arg2));
                    }
                    break;
            }
        }

        private void OnLoadItemsGrid(DisplayDataPooled data)
        {
            try
            {
                if (_isBeingDestroyed) return;

                var grouped = _storageContainer.GetItems();

                if (data.EndPosition > grouped.Count)
                {
                    data.EndPosition = grouped.Count;
                }

                for (int i = data.StartPosition; i < data.EndPosition; i++)
                {
                    if (CheckIfButtonIsActive(grouped.ElementAt(i).Key))
                    {
                        continue;
                    }

                    GameObject buttonPrefab = data.Pool.SpawnFromPool(InventoryPoolTag, data.ItemsGrid);
                    buttonPrefab.transform.SetParent(data.ItemsGrid.transform, false);
                    var itemBTN = buttonPrefab.EnsureComponent<InventoryButton>();
                    itemBTN.Storage = _storageContainer;
                    itemBTN.ButtonMode = InterfaceButtonMode.Background;
                    itemBTN.STARTING_COLOR = Color.gray;
                    itemBTN.HOVER_COLOR = Color.white;
                    itemBTN.BtnName = "ItemBTN";
                    itemBTN.TextLineOne = AuxPatchers.TakeFormatted(Language.main.Get(grouped.ElementAt(i).Key));
                    itemBTN.Tag = grouped.ElementAt(i).Key;
                    itemBTN.RefreshIcon();
                    itemBTN.OnButtonClick = OnButtonClick;
                    _trackedItems.Add(itemBTN);
                }
                _inventoryGrid.UpdaterPaginator(grouped.Count);

            }
            catch (Exception e)
            {
                QuickLogger.Error("Error Caught");
                QuickLogger.Error($"Error Message: {e.Message}");
                QuickLogger.Error($"Error StackTrace: {e.StackTrace}");
            }
        }

        private bool CheckIfButtonIsActive(TechType techType)
        {
            foreach (InventoryButton button in _trackedItems)
            {
                if (button.IsValidAndActive(techType))
                {
                    button.UpdateAmount();
                    return true;
                }
            }

            return false;
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

            _isFromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (_storageContainer.GetCount() > 0)
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
            _savedData.BodyColor = _colorManager.GetColor().ColorToVector4();
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
            if (!_interactionHelper.IsInRange && IsInitialized && IsConstructed)
            {
                HandReticle main = HandReticle.main;
                main.SetInteractText(AuxPatchers.OpenAlterraStorage());
                main.SetIcon(HandReticle.IconType.Hand);
            }
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
            return _storageContainer.RemoveItem(techType);
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return _storageContainer.Contains(techType);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }

    internal class InventoryButton : InterfaceButton
    {
        private Text _amount;
        private uGUI_Icon _icon;
        private bool _isInitialized;

        private void Start()
        {
            if (!_isInitialized)
            {
                _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();
                _isInitialized = true;
            }

            UpdateAmount();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            UpdateAmount();
        }

        internal void UpdateAmount()
        {
            var amount = Storage.GetCount((TechType) Tag);
            if (amount <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_amount == null)
            {
                _amount = gameObject.GetComponentInChildren<Text>();
            }

            _amount.text = amount.ToString();

        }

        public FCSStorage Storage { get; set; }

        public void RefreshIcon()
        {
            if (_icon == null)
            {
                _icon = InterfaceHelpers.FindGameObject(gameObject, "Icon").EnsureComponent<uGUI_Icon>();

            }
            _icon.sprite = SpriteManager.Get((TechType)Tag);
        }

        public bool IsValidAndActive(TechType techType)
        {
            return techType == (TechType)Tag && gameObject.activeSelf;
        }
    }

    internal class InterfaceInteration : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
