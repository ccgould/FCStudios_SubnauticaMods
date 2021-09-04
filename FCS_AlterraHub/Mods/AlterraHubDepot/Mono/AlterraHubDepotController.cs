using System;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Configuration;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Model.GUI;
using FCS_AlterraHub.Mods.FCSPDA.Mono;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AlterraHub.Mods.AlterraHubDepot.Mono
{
    internal class AlterraHubDepotController: FcsDevice, IFCSSave<SaveData>, IFCSDisplay,IHandTarget
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private AlterraHubDepotEntry _savedData;
        private Text _status;
        private GridHelperV2 _inventoryGrid;
        public override bool IsOperational => IsConstructed && IsInitialized;
        private readonly List<StorageInventoryButton> _inventoryButtons = new();
        private Dictionary<TechType, int> _storage = new();
        private PaginatorController _paginatorController;
        private bool _isBeingDestroyed;
        private const int MAXSTORAGE = 48;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraHubDepotTabID, Mod.ModPackID);
            RefreshUI();
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
                    if (_savedData.Storage != null)
                    {
                        _storage = _savedData.Storage;
                        _inventoryGrid.DrawPage();
                        RefreshUI();
                    }
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _isBeingDestroyed = true;
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }

            foreach (Transform invItem in GameObjectHelpers.FindGameObject(gameObject, "Grid").transform)
            {
                var invButton = invItem.gameObject.EnsureComponent<StorageInventoryButton>();
                invButton.ButtonMode = InterfaceButtonMode.Background;
                invButton.BtnName = "InventoryBTN";
                invButton.OnButtonClick += OnButtonClick;
                _inventoryButtons.Add(invButton);
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();

            _status = GameObjectHelpers.FindGameObject(canvas.gameObject, "Status")?.GetComponent<Text>();

            _inventoryGrid = gameObject.EnsureComponent<GridHelperV2>();
            _inventoryGrid.OnLoadDisplay += OnLoadItemsGrid;
            _inventoryGrid.Setup(28, gameObject, Color.gray, Color.white, OnButtonClick);

            _paginatorController = GameObjectHelpers.FindGameObject(gameObject, "Paginator").AddComponent<PaginatorController>();
            _paginatorController.Initialize(this);

            MaterialHelpers.ChangeEmissionColor(Buildables.AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);
            IsInitialized = true;
        }

        private void OnLoadItemsGrid(DisplayData data)
        {
            try
            {
                if (_isBeingDestroyed || _storage == null || _inventoryButtons == null || _inventoryGrid == null || _paginatorController == null) return;

                var grouped = _storage;

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
                    _inventoryButtons[w++].Set(grouped.ElementAt(i).Key, grouped.ElementAt(i).Value);
                }

                RefreshUI();
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

        private void OnButtonClick(string arg1, object arg2)
        {
            switch (arg1)
            {
                case "InventoryBTN":
                    var size = CraftData.GetItemSize((TechType)arg2);
                    if (Inventory.main.HasRoomFor(size.x, size.y))
                    {
                        PlayerInteractionHelper.GivePlayerItem(RemoveItemFromContainer((TechType)arg2));
                    }
                    break;
            }
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            if (!_storage.ContainsKey(techType)) return null;
            _storage[techType] -= 1;
            if (_storage[techType] <= 0)
            {
                _storage.Remove(techType);
            }
            _inventoryGrid.DrawPage();
            return techType.ToPickupable();
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"In OnProtoSerialize -  Hub Depot");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
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

            _isFromSave = true;
        }
        
        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            if (_storage.Count == 0) return true;
            reason = Buildables.AlterraHub.NotEmpty();
            return false;
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraHubDepotEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            _savedData.Storage = _storage;
            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            newSaveData.AlterraHubDepotEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetAlterraHubDepotEntrySaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized) return;

            base.OnHandHover(hand);

            var data = new[]
            {
                $"UnitID: {UnitID} | Depot Name: {DepotName}"
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            //Not in use
        }

        public string DepotName => $"{UnitID} : Depot";
        
        internal string GetUnitName()
        {
            return DepotName;
        }
        
        internal bool AddItemToStorage(TechType item)
        {
            if(IsFull()) return false;
            if (_storage.ContainsKey(item))
            {
                _storage[item] += 1;
            }
            else
            {
                _storage.Add(item,1);
            }

            _inventoryGrid.DrawPage();
            return true;
        }

        public string GetStatus()
        {
            return IsFull() ? "Full" : "Ready";
        }

        public bool IsFull()
        {
            var sum = _storage.Sum(x => x.Value);
            return sum >= MAXSTORAGE;
        }

        public override void RefreshUI()
        {
            if (_status == null || _storage == null) return;
            _status.text = _storage.Count > 0 ? "PICK AVAILABLE" : "EMPTY";
        }

        public int GetFreeSlotsCount()
        {
            return MAXSTORAGE - _storage.Count;
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