using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.Mods.AutoCrafter.Buildable;
using FCS_ProductionSolutions.Mods.AutoCrafter.Helpers;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine;
using FCS_ProductionSolutions.Mods.AutoCrafter.Models.StateMachine.States;
using FCS_ProductionSolutions.Mods.AutoCrafter.Patches;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.AutoCrafter.Mono
{

    /// <summary>
    /// Controller for the Autocrafter handles initializations, loading from saves and main controls.
    /// </summary>
    internal class AutoCrafterController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        private IEnumerable<Material> _materials;
        private const float _beltSpeed = 0.01f;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private float _transferTimer;
        private List<TechType> _storedItems = new();
        private AutoCrafterDataEntry _saveData;
        private bool _moveBelt;
        public StorageContainer Storage;
        public CrafterMode _mode = CrafterMode.Normal;
        private HashSet<string> _linkedChildDevices = new();
        private HashSet<string> _parentCrafters = new();
        private bool _isStandBy;

        public CraftMachine CraftMachine { get; private set; }


        #region Unity Methods

        private void Update()
        {
            if (WorldHelpers.CheckIfPaused()) return;
            MoveBeltMaterial();

            if (_transferTimer >= 1f)
            {
                for (int i = _storedItems.Count - 1; i >= 0; i--)
                {

                    CompatibilityMethods.AttemptToAddToNetwork(_storedItems[i], Manager, _storedItems);
                }

                _transferTimer = 0f;
            }

        }
        public bool IsBeltMoving()
        {
            return _moveBelt;
        }
        
        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, AutoCrafterPatch.AutoCrafterTabID, Mod.ModPackID);
            _materials = MaterialHelpers.GetMaterials(GameObjectHelpers.FindGameObject(gameObject, "ConveyorBelts"), AlterraHub.BaseOpaqueInterior);

            if (Manager != null)
            {
                Manager.OnPowerStateChanged += OnPowerStateChanged;
                OnPowerStateChanged(Manager.GetPowerState());
                Manager.NotifyByID(AutoCrafterPatch.AutoCrafterTabID, "RefreshAutoCrafterList");
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
                QuickLogger.Debug($"------------------------------------------");
                QuickLogger.Debug($"Loading {UnitID} from save.");

                _colorManager.LoadTemplate(_saveData.ColorTemplate);

                _isStandBy = _saveData.IsStandBy;

                if (_saveData.StoredItems != null)
                {
                    _storedItems = _saveData.StoredItems;
                }

                if (_saveData.StateData != null)
                {
                    StateMachine.LoadFromSave(_saveData.StateData);
                }

                if(_saveData.ConnectedDevices != null) _linkedChildDevices = new HashSet<string>(_saveData.ConnectedDevices);
                if(_saveData.ParentDevices != null) _parentCrafters = new HashSet<string>(_saveData.ParentDevices);
                QuickLogger.Debug($"------------------------------------------");


                _fromSave = false;
            }
        }

        public override void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnPowerStateChanged -= OnPowerStateChanged;
                Manager.NotifyByID(AutoCrafterPatch.AutoCrafterTabID, "RefreshAutoCrafterList");
            }
            base.OnDestroy();
        }
        
        #endregion

        #region FCS Overrides

        public override void Initialize()
        {
            if (IsInitialized) return;

            QuickLogger.Debug($"Initializing");

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol);
                _colorManager.ChangeColor(new ColorTemplate());
            }

            if (Storage == null)
            {
                Storage = GetComponent<StorageContainer>();
            }

            if (StateMachine == null)
            {
                StateMachine = gameObject.AddComponent<CrafterStateManager>();
                StateMachine.Crafter = this;
            }

            if (CrafterBelt == null)
            {
                CrafterBelt = gameObject.AddComponent<CrafterBeltController>();
                CrafterBelt.Crafter = this;
            }

            if (CraftMachine == null)
            {
                CraftMachine = GetComponent<CraftMachine>();
                CraftMachine.Crafter = this;
            }

            Storage.enabled = false;

            IsInitialized = true;

            QuickLogger.Debug($"Initializing Completed");
        }

        public CrafterBeltController CrafterBelt { get; set; }

        public CrafterStateManager StateMachine { get; set; }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
        
        #endregion

        #region IProtoEventListener

        public void Save(SaveData saveDataList, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || _colorManager == null)
            {
                QuickLogger.Error($"Failed to save driller {GetPrefabID()}");
                return;
            }

            if (_saveData == null) 
            {
                _saveData = new AutoCrafterDataEntry();
            }
            
            QuickLogger.Message($"SaveData = {_saveData}", true);

            _saveData.ID = GetPrefabID();
            _saveData.ColorTemplate = _colorManager.SaveTemplate();
            _saveData.StateData = StateMachine.CurrentState.GetType() == typeof(CrafterCraftingState)
                ? StateMachine.CurrentState as CrafterCraftingState
                : null;
            _saveData.ConnectedDevices = _linkedChildDevices.ToList();
            _saveData.ParentDevices = _parentCrafters.ToList();
            _saveData.IsStandBy = _isStandBy; 
            saveDataList.AutoCrafterDataEntries.Add(_saveData);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info("Saving Drills");
                Mod.Save(serializer);
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        #endregion

        #region IConstructable
        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (IsInitialized == false)
            {
                return true;
            }
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Info("In Constructed Changed");

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
        #endregion

        #region Internal Methods
        internal bool AddItemToStorage(TechType techType)
        {
            try
            {
                _storedItems.Add(techType);
            }
            catch (Exception e)
            {
                QuickLogger.Debug(e.Message);
                QuickLogger.Debug(e.StackTrace);
                return false;
            }
            return true;
        }

        internal void ShowMessage(string message)
        {
            QuickLogger.Debug(message);
        }

        internal void AddLinkedDevice(AutoCrafterController childDeviceID)
        {
            _linkedChildDevices.Add(childDeviceID.UnitID);
            childDeviceID.AddParentCrafter(this);
        }
        
        internal void RemoveLinkedDevice(AutoCrafterController childDeviceID)
        {
            _linkedChildDevices.Remove(childDeviceID.UnitID);
            childDeviceID.RemoveParentCrafter(this);
        }

        internal void AddParentCrafter(AutoCrafterController parentDeviceID)
        {
            _parentCrafters.Add(parentDeviceID.UnitID);
        }

        internal void RemoveParentCrafter(AutoCrafterController parentDeviceID)
        {
            _parentCrafters.Remove(parentDeviceID.UnitID);
        }

        internal void CancelLinkedCraftersOperations()
        {
            foreach (string childDevice in _linkedChildDevices)
            {
                var device = FCSAlterraHubService.PublicAPI.FindDevice(childDevice);
                if (device.Value != null)
                {
                    var crafter = device.Value.gameObject.GetComponent<AutoCrafterController>();
                    if (crafter != null)
                    {
                        crafter.CraftMachine.CancelOperation();
                    }
                }
            }
        }

        #endregion

        #region Private Method
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        private void MoveBelt()
        {
            _moveBelt = true;
        }

        private void StopBelt()
        {
            _moveBelt = false;
        }

        private void MoveBeltMaterial()
        {
            if (_materials != null && _moveBelt)
            {
                float offset = Time.time * _beltSpeed;
                foreach (Material material in _materials)
                {
                    material.SetTextureOffset(ShaderPropertyID._MainTex, new Vector2(-offset, 0));
                    material.SetTextureOffset(ShaderPropertyID._NormalsTex, new Vector2(-offset, 0));
                    material.SetTextureOffset(ShaderPropertyID._Illum, new Vector2(-offset, 0));
                }
            }
        }

        private void OnPowerStateChanged(PowerSystem.Status obj)
        {

            if (obj == PowerSystem.Status.Offline)
            {
                StopBelt();
            }
            else
            {
                MoveBelt();
            }
        }

        #endregion

        public void DistributeLoad(CraftingOperation operation)
        {
            foreach (string deviceUnitID in _linkedChildDevices)
            {
                var device = FCSAlterraHubService.PublicAPI.FindDevice(deviceUnitID);
                if (device.Value != null)
                {
                    var crafter = (AutoCrafterController) device.Value;

                    if (!crafter.CraftMachine.IsCrafting())
                    {
                        crafter.CraftMachine.StartCrafting(operation);
                    }
                }
            }
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsConstructed || !IsInitialized) return;

            base.OnHandHover(hand);


            var message = hand.IsTool()
                ? $"Please clear hand to use {AutoCrafterPatch.AutoCrafterFriendlyName}."
                : $"Press {KeyCode.F} to interact with {AutoCrafterPatch.AutoCrafterFriendlyName}.";

            var data = new[]
            {
                message,
                $"UnitID: {UnitID}"
            };
            
            if (Input.GetKeyDown(KeyCode.F) && !hand.IsTool())
            {
                AutocrafterHUD.Main.Show(this);
            }

            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
            
        }

        public void SetIsStandBy(bool value)
        {
            _isStandBy = value;
        }

        public bool GetIsStandBy()
        {
            return _isStandBy;
        }

        public IEnumerable<string> GetParentCrafters()
        {
            return _parentCrafters;
        }
    }

    internal enum CrafterMode
    {
        Normal,
        LoadSharing
    }
}
