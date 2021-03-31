using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FMOD;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter
{
    internal class DSSAutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSAutoCrafterDataEntry _saveData;
        internal DSSAutoCrafterDisplay DisplayManager;
        private bool _hasBreakTripped;
        internal  DSSCraftManager CraftManager;
        internal CraftingOperation CraftingItem;
        private bool _moveBelt;
        private float _beltSpeed = 0.01f;
        private IEnumerable<Material> _materials;
        private GameObject _canvas;
        private bool IsCrafting => _craftingItem != null;
        private CraftingOperation _craftingItem;
        public override bool IsVisible => IsInitialized && IsConstructed;
        public override bool IsOperational => IsInitialized && IsConstructed;

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
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSAutoCrafterTabID, Mod.ModName);
           _materials = MaterialHelpers.GetMaterials(GameObjectHelpers.FindGameObject(gameObject, "ConveyorBelts"), "fcs01_BD");

            if (Manager != null)
            {
                Manager.OnPowerStateChanged += OnPowerStateChanged;
                Manager.OnBreakerStateChanged += OnBreakerStateChanged;
                OnPowerStateChanged(Manager.GetPowerState());
            }
        }

        private void Update()
        {
            MoveBeltMaterial();
        }

        private void MoveBeltMaterial()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset("_MainTex", new Vector2(-offset, 0));
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

        private void GetCraftTreeData(CraftNode innerNodes)
        {
            foreach (CraftNode craftNode in innerNodes)
            {
                QuickLogger.Debug($"Craftable: {craftNode.id} | {craftNode.string0} | {craftNode.string1} | {craftNode.techType0}");

                if (string.IsNullOrWhiteSpace(craftNode.id)) continue;
                if (craftNode.id.Equals("CookedFood") || craftNode.id.Equals("CuredFood")) return;
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
                    //CraftingItems = _saveData.CurrentProcess;
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
                DisplayManager.OnCancelBtnClick += OnCancelBtnClick;
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

            MoveBelt();

            //TODO Reenable
            InvokeRepeating(nameof(CheckForAvailableCrafts),1f,1f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized - {GetPrefabID()}");
        }

        private void OnCancelBtnClick()
        {
            Manager.RemoveCraftingOperation(CraftingItem);
            DisplayManager.Clear();
            CraftingItem = null;
            CraftManager.StopOperation();
            CraftManager.Reset(true);
        }

        private void AddDummy()
        {
            CraftItem(new CraftingOperation(TechType.AdvancedWiringKit,1,true));
        }

        private void CheckForAvailableCrafts()
        {
            if (Manager == null || IsCrafting) return;

            //Check if already has operation
            var hasOperation = Manager.GetBaseCraftingOperations().FirstOrDefault(x => x.Devices.Contains(UnitID));
            
            if (hasOperation != null)
            {
                CraftItem(hasOperation);
                return;
            }

            foreach (CraftingOperation baseCraftingOperation in Manager.GetBaseCraftingOperations())
            {
                if (!baseCraftingOperation.CanCraft()) continue;
                CraftItem(baseCraftingOperation);
                break;
            }
        }

        private void CraftItem(CraftingOperation operation)
        {
            try
            {
                _craftingItem = operation;
                DisplayManager.LoadCraft(operation);
                operation.IsBeingCrafted = true;
                CraftingItem = operation;
                CraftManager.StartOperation();
                operation.Mount(this);
            }
            catch (Exception e)
            {
                QuickLogger.DebugError(e.Message);
                QuickLogger.DebugError(e.StackTrace);
            }
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
                _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();
                //_saveData.CurrentProcess = CraftingItems;
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
            if(CraftManager.IsRunning())
            {
                reason = AuxPatchers.AutocrafterItemIsBeingCrafted();
                return false;
            }


            if (CraftManager.ItemsOnBelt())
            {
                reason = AuxPatchers.AutocrafterItemsOnBelt();
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

        public CraftingOperation GetCraftingItem()
        {
            return _craftingItem;
        }

        public void ShowMessage(string message)
        {
            
        }

        public void ClearMissingItems()
        {
            DisplayManager?.ClearMissingItem();
        }

        public void AddMissingItem(string item, int amount)
        {
            DisplayManager?.AddMissingItem(item, amount);
        }

        public void ClearCraftingItem()
        {
            _craftingItem = null;
        }
    }
}