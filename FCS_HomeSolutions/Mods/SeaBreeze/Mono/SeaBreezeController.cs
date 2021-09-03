using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Enumerators;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.ModManagers;
using FCS_HomeSolutions.Mods.SeaBreeze.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.SeaBreeze.Mono
{
    internal class SeaBreezeController : FcsDevice, IFCSSave<SaveData>, IHandTarget
    {
        #region Private Members

        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SeaBreezeDataEntry _savedData;
        protected internal DumpContainer _dumpContainer;
        private InterfaceInteraction _interactionHelper;

        #endregion

        #region Public Properties

        internal Fridge FridgeComponent { get; private set; }
        internal SeaBreezePowerManager PowerManager { get; private set; }
        internal NameController NameController { get; private set; }
        public Action<string, NameController> OnLabelChanged { get; set; }
        internal SeaBreezeDisplay DisplayManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal int PageStateHash { get; private set; }
        internal PrefabIdentifier PrefabId { get; set; }
        public override bool IsVisible => GetHasBreakerTripped();
        private bool GetHasBreakerTripped()
        {
            return PowerManager.GetHasBreakerTripped();
        }

        

        #endregion

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, SeaBreezeBuildable.SeaBreezeTabID, Mod.ModPackID);
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
                    DisplayManager.OnContainerUpdate(numberOfItems, QPatch.Configuration.SeaBreezeStorageLimit);
                    DisplayManager.UpdateScreenLabel(NameController.GetCurrentName(), NameController);
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    PowerManager.LoadSave(_savedData.PowercellData, _savedData.HasBreakerTripped);
                    StartCoroutine( FridgeComponent.LoadSave(_savedData.FridgeContainer));
                    NameController.SetCurrentName(_savedData.UnitName);
                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(),ColorTargetMode.Secondary);
                    _colorManager.ChangeColor(_savedData.Emission.Vector4ToColor(),ColorTargetMode.Emission);
                    QuickLogger.Info($"Loaded {SeaBreezeBuildable.SeaBreezeFriendly}");
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

            QPatch.Configuration.OnSeaBreezeGameModeChanged += OnModModeChanged;

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
                PowerManager.OnPowerOutage += OnPowerOutage;
                PowerManager.OnPowerResume += OnPowerResume;
            }

            if (FridgeComponent == null)
            {
                FridgeComponent = gameObject.AddComponent< Fridge>();
                FridgeComponent.OnContainerUpdate += OnFridgeContainerUpdate;
                FridgeComponent.Initialize(this,QPatch.Configuration.SeaBreezeStorageLimit);
                FridgeComponent.SetModMode(QPatch.Configuration.SeaBreezeModMode);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol,AlterraHub.BaseSecondaryCol,AlterraHub.BaseLightsEmissiveController);
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
                //Refresh the Count on the screen
                OnFridgeContainerUpdate(0, QPatch.Configuration.SeaBreezeStorageLimit);
            }

            if (NameController == null)
            {
                NameController = gameObject.EnsureComponent<NameController>();
                NameController.Initialize(SeaBreezeAuxPatcher.Submit(), SeaBreezeBuildable.SeaBreezeFriendly);
                NameController.OnLabelChanged += OnLabelChangedMethod;
                NameController.SetCurrentName(GetNewSeaBreezeName());
            }

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            _interactionHelper = canvas.gameObject.AddComponent<InterfaceInteraction>();

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, Color.cyan);

            IsInitialized = true;
        }

        private string GetNewSeaBreezeName()
        {
            QuickLogger.Debug($"Get Seabreeze New Name");
            return $"{SeaBreezeBuildable.SeaBreezeFriendly} {UnitID}";
        }

        private void OnModModeChanged(int modMode)
        {
            QuickLogger.Debug($"Changing Seabreeze Game Mode to {(FCSGameMode)modMode}",true);
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

        #region Debugging

        private void SetDecaying()
        {
            FridgeComponent.SetDecay(true);
        }

        private void SetFrigerate()
        {
            FridgeComponent.SetDecay(false);
        }
        #endregion

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

        private void OnFridgeContainerUpdate(int arg1, int arg2)
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
                QuickLogger.Info($"Saving {SeaBreezeBuildable.SeaBreezeFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {SeaBreezeBuildable.SeaBreezeFriendly}");
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
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.Emission = _colorManager.GetLumColor().ColorToVector4();
            _savedData.UnitName = NameController.GetCurrentName();
            _savedData.FridgeContainer = FridgeComponent.Save();
            _savedData.PowercellData = PowerManager.Save();
            _savedData.HasBreakerTripped = PowerManager.GetHasBreakerTripped();
            newSaveData.SeaBreezeDataEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }

        public void ClearSeaBreeze()
        {
            if (FridgeComponent != null)
            {
                FridgeComponent.Clear();
            }
        }

        public override bool CanBeStored(int amount, TechType techType)
        {
            return FridgeComponent.CanBeStored(amount, techType);
        }

        public override bool AddItemToContainer(InventoryItem item)
        {
            return FridgeComponent.AddItemToContainer(item);
        }

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed || _interactionHelper.IsInRange) return;
            base.OnHandHover(hand);

            var data = new[]
            {
                AlterraHub.PowerPerMinute(PowerManager.GetPowerUsagePerSecond() * 60)
            };
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}