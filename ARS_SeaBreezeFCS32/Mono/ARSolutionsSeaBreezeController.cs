using System;
using ARS_SeaBreezeFCS32.Buildables;
using ARS_SeaBreezeFCS32.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace ARS_SeaBreezeFCS32.Mono
{
    internal class ARSolutionsSeaBreezeController : FCSController
    {
        #region Private Members

        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private int _dumpStorageSizeXY = 2;
        #endregion

        #region Public Properties

        internal ColorManager ColorManager { get; private set; }
        internal Fridge FridgeComponent { get; private set; }
        internal DumpContainer DumpContainer { get; private set; }
        internal ARSolutionsSeaBreezePowerManager PowerManager { get; private set; }
        internal NameController NameController { get; private set; }
        public Action<string, NameController> OnLabelChanged { get; set; }
        internal ARSolutionsSeaBreezeDisplay DisplayManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        public int PageStateHash { get; private set; }

        #endregion

        #region Unity Methods

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
                    DisplayManager.OnContainerUpdate(numberOfItems, QPatch.Configuration.StorageLimit);
                    DisplayManager.UpdateScreenLabel(NameController.GetCurrentName(), NameController);
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    FridgeComponent.LoadSave(_savedData.FridgeContainer);
                    NameController.SetCurrentName(_savedData.UnitName);
                    ColorManager.SetColorFromSave(_savedData.BodyColor.Vector4ToColor());
                    QuickLogger.Info($"Loaded {Mod.FriendlyName}");
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
            _savedData = Mod.GetSaveData(id);
        }

        public override void Initialize()
        {
            PageStateHash = Animator.StringToHash("PageState");

            QPatch.Configuration.OnModModeChanged += OnModModeChanged;

            if (PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<ARSolutionsSeaBreezePowerManager>();
                PowerManager.Initialize(this);
                PowerManager.OnBreakerTripped += OnBreakerTripped;
                PowerManager.OnBreakerReset += OnBreakerReset;
                PowerManager.OnPowerResume += OnPowerResume;
                PowerManager.OnPowerOutage += OnPowerOutage;
            }

            if (FridgeComponent == null)
            {
                FridgeComponent = gameObject.AddComponent<Fridge>();
                FridgeComponent.Initialize(this,QPatch.Configuration.StorageLimit);
                FridgeComponent.OnContainerUpdate += OnContainerUpdate;
                FridgeComponent.SetModMode(QPatch.Configuration.ModMode);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ARSSeaBreezeFCS32Buildable.BodyMaterial);
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(gameObject.transform, ARSSeaBreezeFCS32Buildable.StorageLabel(),
                    ARSSeaBreezeFCS32Buildable.ItemNotAllowed(),
                    ARSSeaBreezeFCS32Buildable.SeaBreezeFull(),
                    FridgeComponent, _dumpStorageSizeXY, _dumpStorageSizeXY);
            }

            if (NameController == null)
            {
                NameController = gameObject.EnsureComponent<NameController>();
                NameController.Initialize(ARSSeaBreezeFCS32Buildable.Submit(), Mod.FriendlyName);
                NameController.SetCurrentName(Mod.GetNewSeabreezeName());
                NameController.OnLabelChanged += OnLabelChangedMethod;
            }
            
            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<ARSolutionsSeaBreezeDisplay>();
                DisplayManager.Setup(this);
            }

            IsInitialized = true;
        }

        private void OnModModeChanged(ModModes modMode)
        {
            FridgeComponent.SetModMode(modMode);
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
            AnimationManager.SetIntHash(PageStateHash,0);
        }

        private void OnPowerResume()
        {
            QuickLogger.Debug("Power Resumed", true);
            if (!PowerManager.GetHasBreakerTripped())
            {
                FridgeComponent.SetDecay(false);
                AnimationManager.SetIntHash(PageStateHash, 1);
            }
        }

        internal void ToggleBreaker()
        {
            PowerManager.TogglePower();
        }

        private void OnContainerUpdate(int arg1, int arg2)
        {
            DisplayManager.OnContainerUpdate(arg1,arg2);
        }

        private void OnLabelChangedMethod(string arg1, NameController arg2)
        {
            DisplayManager.UpdateScreenLabel(arg1,arg2);
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
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

            reason = ARSSeaBreezeFCS32Buildable.NotEmpty();
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

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.UnitName = NameController.GetCurrentName();
            _savedData.FridgeContainer = FridgeComponent.Save();
            saveData.Entries.Add(_savedData);
        }

        public override string GetName()
        {
            return NameController != null ? NameController.GetCurrentName() : string.Empty;
        }
    }
}