using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.ModManagers;
using FCS_HomeSolutions.SeaBreeze.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.SeaBreeze.Mono
{
    internal class SeaBreezeController : FcsDevice, IFCSSave<SaveData>
    {
        #region Private Members

        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SeaBreezeDataEntry _savedData;
        protected internal DumpContainer _dumpContainer;

        #endregion

        #region Public Properties

        internal ColorManager ColorManager { get; private set; }
        internal Fridge FridgeComponent { get; private set; }
        internal SeaBreezePowerManager PowerManager { get; private set; }
        internal NameController NameController { get; private set; }
        public Action<string, NameController> OnLabelChanged { get; set; }
        internal SeaBreezeDisplay DisplayManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal int PageStateHash { get; private set; }
        internal PrefabIdentifier PrefabId { get; set; }

        #endregion

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.SeaBreezeTabID, Mod.ModName);
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (DisplayManager != null)
                {
                    var numberOfItems = FridgeComponent.NumberOfItems;
                    DisplayManager.OnContainerUpdate(numberOfItems, QPatch.SeaBreezeConfiguration.StorageLimit);
                    DisplayManager.UpdateScreenLabel(NameController.GetCurrentName(), NameController);
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    PowerManager.LoadSave(_savedData.PowercellData, _savedData.HasBreakerTripped);
                    FridgeComponent.LoadSave(_savedData.FridgeContainer);
                    NameController.SetCurrentName(_savedData.UnitName);
                    ColorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor());
                    IsVisible = _savedData.IsVisible;
                    QuickLogger.Info($"Loaded {Mod.SeaBreezeFriendly}");
                }

                _runStartUpOnEnable = false;
            }
        }

        #endregion

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSeabreezeSaveData(id);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {
            PageStateHash = Animator.StringToHash("PageState");
            
            QPatch.SeaBreezeConfiguration.OnGameModeChanged += OnModModeChanged;

            if (PrefabId == null)
            {
                PrefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<SeaBreezePowerManager>();
                PowerManager.Initialize(this);
                PowerManager.OnBreakerTripped += OnBreakerTripped;
                PowerManager.OnBreakerReset += OnBreakerReset;
                PowerManager.OnPowerResume += OnPowerResume;
                PowerManager.OnPowerOutage += OnPowerOutage;
            }

            if (FridgeComponent == null)
            {
                FridgeComponent = gameObject.AddComponent<Fridge>();
                FridgeComponent.Initialize(this, QPatch.SeaBreezeConfiguration.StorageLimit);
                FridgeComponent.OnContainerUpdate += OnContainerUpdate;
                FridgeComponent.SetModMode(QPatch.SeaBreezeConfiguration.ModMode);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_dumpContainer == null)
            {
                _dumpContainer = gameObject.AddComponent<DumpContainer>();
                _dumpContainer.Initialize(gameObject.transform, SeaBreezeAuxPatcher.StorageLabel(),FridgeComponent);
            }
            
            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<SeaBreezeDisplay>();
                DisplayManager.Setup(this);
            }

            if (NameController == null)
            {
                NameController = gameObject.EnsureComponent<NameController>();
                NameController.Initialize(SeaBreezeAuxPatcher.Submit(), Mod.SeaBreezeFriendly);
                NameController.OnLabelChanged += OnLabelChangedMethod;
                NameController.SetCurrentName(GetNewSeaBreezeName());
            }

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.LUMControllerMaterial, gameObject, 5);

            IsInitialized = true;
        }

        private string GetNewSeaBreezeName()
        {
            QuickLogger.Debug($"Get Seabreeze New Name");
            return $"{Mod.SeaBreezeFriendly} {UnitID}";
        }

        private void OnModModeChanged(int modMode)
        {
            FridgeComponent.SetModMode((FCSGameMode)modMode);
        }

        private void OnBreakerReset()
        {
            QuickLogger.Debug("Breaker Reset", true);
            FridgeComponent.SetDecay(false);
        }

        private void OnBreakerTripped()
        {
            QuickLogger.Debug("Breaker Tripped", true);
            FridgeComponent.SetDecay(true);
        }

        private void OnPowerOutage()
        {
            QuickLogger.Debug("Power Outage", true);
            FridgeComponent.SetDecay(true);
            AnimationManager.SetIntHash(PageStateHash, 0);
        }

        private void OnPowerResume()
        {
            if (PowerManager == null || FridgeComponent == null || AnimationManager == null) return;
            QuickLogger.Debug("Power Resumed", true);
            if (!PowerManager.GetHasBreakerTripped())
            {
                FridgeComponent.SetDecay(false);
                AnimationManager.SetIntHash(PageStateHash, 1);
            }
        }

        private void OnContainerUpdate(int arg1, int arg2)
        {
            DisplayManager.OnContainerUpdate(arg1, arg2);
        }

        private void OnLabelChangedMethod(string arg1, NameController arg2)
        {
            DisplayManager.UpdateScreenLabel(arg1, arg2);
            OnLabelChanged?.Invoke(arg1, arg2);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.SeaBreezeFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.SeaBreezeFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (FridgeComponent == null) return true;

            if (FridgeComponent.IsEmpty())
            {
                return true;
            }

            reason = SeaBreezeAuxPatcher.NotEmpty();
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {    
            var id = PrefabId.Id;
             
            if (_savedData == null)
            {
                _savedData = new SeaBreezeDataEntry();
            }
             
            _savedData.Id = id;
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.UnitName = NameController.GetCurrentName();
            _savedData.FridgeContainer = FridgeComponent.Save();
            _savedData.PowercellData = PowerManager.Save();
            _savedData.IsVisible = IsVisible;
            _savedData.HasBreakerTripped = PowerManager.GetHasBreakerTripped();
            newSaveData.SeaBreezeDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return ColorManager.ChangeColor(color, mode);
        }

        public void ClearSeaBreeze()
        {
            if (FridgeComponent != null)
            {
                FridgeComponent.Clear();
            }
        }
    }
}