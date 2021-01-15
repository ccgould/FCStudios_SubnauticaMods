using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSAutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSAutoCrafterDataEntry _saveData;
        internal DSSAutoCrafterDisplay DisplayManager;
        private bool _hasBreakTripped;
        internal  DSSCraftManager CraftManager;
        internal ObservableCollection<CraftingItem> CraftingItems = new ObservableCollection<CraftingItem>();
        private bool _moveBelt;
        private float _beltSpeed = 0.1f;
        private IEnumerable<Material> _materials;
        private GameObject _canvas
            ;
        internal FCSStorage StorageManager { get; private set; }
        public override bool IsVisible => IsInitialized && IsConstructed;
        public override bool IsOperational => IsInitialized && IsConstructed;
        public override StorageType StorageType { get; } = StorageType.AutoCrafter;

        public override FCSStorage GetStorage()
        {
            return StorageManager;
        }

        public override float GetPowerUsage()
        {
            if (Manager == null || !IsConstructed || Manager.GetBreakerState() || CraftManager ==  null || !CraftManager.IsRunning()) return 0f;
            return 0.01f;
        }

        private void OnBreakerStateChanged(bool value)
        {
            OnPowerStateChanged(!value ? PowerSystem.Status.Offline : Manager.GetPowerState());
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {
            UpdateScreenState();
            if (obj == PowerSystem.Status.Offline)
            {
                StopBelt();
            }
            else
            {
                MoveBelt();
            }
        }

        private void UpdateScreenState()
        {
            if (Manager.GetBreakerState() || Manager.GetPowerState() != PowerSystem.Status.Normal)
            {
                if (_canvas.activeSelf)
                {
                    _canvas.SetActive(false);
                }
            }
            else
            {
                if (!_canvas.activeSelf)
                {
                    _canvas.SetActive(true);
                }
            }
        }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
            if (Mod.Craftables.Count == 0)
            {
               var fabricator =  CraftTree.GetTree(CraftTree.Type.Fabricator);
               GetCraftTreeData(fabricator.nodes);
               foreach (TechType craftable in Mod.Craftables)
               {
                   QuickLogger.Debug($"Craftable: {Language.main.Get(craftable)} was added.");
               }
            }
            
            _materials = MaterialHelpers.GetMaterials(gameObject, "DSS_ConveyorBelt");

            DisplayManager.Refresh();

            StorageManager.CleanUpDuplicatedStorageNoneRoutine();
            Manager.AlertNewFcsStoragePlaced(this);
            Manager.OnPowerStateChanged += OnPowerStateChanged;
            Manager.OnBreakerStateChanged += OnBreakerStateChanged;
            OnPowerStateChanged(Manager.GetPowerState());
        }

        private void Update()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset("_MainTex", new Vector2(0, offset));
                }
            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.OnBreakerStateChanged -= OnBreakerStateChanged;
            }
            base.OnDestroy();
        }

        public override Pickupable RemoveItemFromContainer(TechType techType)
        {
            return StorageManager.ItemsContainer.RemoveItem(techType);
        }

        private void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                QuickLogger.Debug($"Craftable: {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

                if (craftNode.techType0 != TechType.None)
                {
                    Mod.Craftables.Add(craftNode.techType0);
                }

                if (craftNode.childCount > 0)
                {
                    GetCraftTreeData(craftNode);
                }
            }
        }
        
        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                if (_saveData.CurrentProcess != null)
                {
                    CraftingItems = _saveData.CurrentProcess;
                }

                if (_saveData.IsRunning)
                {
                    DisplayManager.OnButtonClick("StartBTN",null);
                }
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        public override void Initialize()
        {
            _canvas = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<DSSAutoCrafterDisplay>();
                DisplayManager.Setup(this);
            }

            if (CraftManager == null)
            {
                CraftManager = gameObject.EnsureComponent<DSSCraftManager>();
                CraftManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (StorageManager == null)
            {
                StorageManager = gameObject.AddComponent<FCSStorage>();
                StorageManager.Initialize(48,gameObject.FindChild("StorageRoot"));
            }

            MoveBelt();

            IsInitialized = true;

            QuickLogger.Debug($"Initialized - {GetPrefabID()}");
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
           return StorageManager.AddItem(item);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSAutoCrafterFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSAutoCrafterFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (!IsInitialized)
            {
                Initialize();
            }
            StorageManager.RestoreItems(serializer, _saveData.Data);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                var prefabIdentifier = GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier.Id;

                if (_saveData == null)
                {
                    _saveData = new DSSAutoCrafterDataEntry();
                }

                _saveData.ID = id;
                _saveData.Body = _colorManager.GetColor().ColorToVector4();
                _saveData.Data = StorageManager.Save(serializer);
                _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
                _saveData.CurrentProcess = CraftingItems;
                _saveData.IsRunning = CraftManager.IsRunning();
                newSaveData.DSSAutoCrafterDataEntries.Add(_saveData);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public override bool CanDeconstruct(out string reason)
        {
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
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public bool ToggleBreaker()
        {
            _hasBreakTripped = !_hasBreakTripped;
            return _hasBreakTripped;
        }

        public bool HasPendingCraft()
        {
            return CraftingItems.Count > 0;
        }

        internal void MoveBelt()
        {
            _moveBelt = true;
        }

        internal void StopBelt()
        {
            _moveBelt = false;
        }

        public bool IsBeltMoving()
        {
            return _moveBelt;
        }
    }
}