using System;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Abstract;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using Model;
using UnityEngine;

namespace FCS_HydroponicHarvesters.Mono
{
    internal class HydroHarvController : FCSController
    {
        private bool _fromSave;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _savedData;
        private SpeedModes _currentMode = StartingMode;
        private GameObject _seaBase;

        internal const SpeedModes StartingMode = SpeedModes.Off;
        public override bool IsConstructed { get; set; }
        internal ColorManager ColorManager { get; private set; }
        internal PrefabIdentifier PrefabId { get; private set; }
        internal HydroHarvContainer HydroHarvContainer { get; private set; }
        internal HydroHarvGrowBed HydroHarvGrowBed { get; private set; }
        internal DumpContainer DumpContainer { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal HydroHarvDisplayManager DisplayManager { get; private set; }
        internal HydroHarvEffectManager EffectManager { get; private set; }
        internal HydroHarvGenerator Producer { get; private set; }
        internal HydroHarvLightManager LightManager { get; private set; }
        internal HydroHarvCleanerManager CleanerManager { get; private set; }
        internal DumpContainer CleanerDumpContainer { get; private set; }
        internal bool IsConnectedToBase => _seaBase != null && (_seaBase.name.StartsWith("Cyclops", StringComparison.OrdinalIgnoreCase) || _seaBase.name.StartsWith("Base", StringComparison.OrdinalIgnoreCase));
        internal SpeedModes CurrentSpeedMode
        {
            get => _currentMode;
            set
            {
                SpeedModes previousMode = _currentMode;
                _currentMode = value;
                
                if (_currentMode != SpeedModes.Off)
                {
                    PowerManager.UpdateEnergyPerSecond(_currentMode);
                    if (previousMode == SpeedModes.Off)
                        Producer?.TryStartingNextClone();
                }
                else // Off State
                {
                    PowerManager.UpdateEnergyPerSecond(_currentMode);
                }
            }
        }
        internal HydroHarvPowerManager PowerManager { get; private set; }
        internal HydroHarvCleanerManager HydroHarvCleanerManager { get; private set; }
        public bool IsOperational => GetIsOperational();

        public FCSConnectableDevice FCSConnectableDevice { get; private set; }

        private bool GetIsOperational()
        {
            if (!IsConstructed || 
                !IsInitialized || 
                PowerManager == null || 
                !PowerManager.HasPowerToConsume()) return false;

            return true;
        }

        #region Unity

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (_savedData != null)
                    {
                        ColorManager.SetMaskColorFromSave(_savedData.BodyColor.Vector4ToColor());
                        HydroHarvContainer.Load(_savedData.Container);
                        HydroHarvGrowBed.SetBedType(_savedData.BedType);
                        DisplayManager.RefreshModeBTN(_savedData.BedType);
                        Producer.GenerationProgress = _savedData.GenerationProgress;
                        Producer.CoolDownProgress = _savedData.CoolDownProgress;
                        Producer.StartUpProgress = _savedData.StartUpProgress;
                        LightManager.SetLightSwitchedOff(_savedData.LightState);
                        CurrentSpeedMode = _savedData.CurrentSpeedMode;
                        HydroHarvGrowBed.Load(_savedData.DnaSamples);
                        HydroHarvCleanerManager.Load(_savedData.UnitSanitation);
                        DisplayManager.Load();
                    }
                }
                _runStartUpOnEnable = false;
                IsInitialized = true;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }
        
        private void Start()
        {

        }

        #endregion
        
        public override bool CanDeconstruct(out string reason)
        {
            if (HydroHarvContainer == null)
            {
                reason = string.Empty;
                return true;
            }

            if (HydroHarvGrowBed.HasItems())
            {
                reason = HydroponicHarvestersBuildable.HasDNAItemsMessage();
                return false;
            }

            if (HydroHarvContainer.HasItems())
            {
                reason = HydroponicHarvestersBuildable.HasItemsMessage();
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
                _seaBase = gameObject?.transform?.parent?.gameObject;

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

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            
            _fromSave = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.LargeFriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.LargeFriendlyName}");
            }
        }

        public override void Initialize()
        {
            if (PrefabId == null)
            {
                PrefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, HydroponicHarvestersModelPrefab.BodyMaterial);
            }

            if (HydroHarvGrowBed == null)
            {
                HydroHarvGrowBed = gameObject.AddComponent<HydroHarvGrowBed>();
                HydroHarvGrowBed.Initialize(this);
            }

            if (HydroHarvContainer == null)
            {
                HydroHarvContainer = gameObject.AddComponent<HydroHarvContainer>();
                HydroHarvContainer.Initialize(this);
            }
            
            if (DumpContainer == null)
            {
                Vector2 size = Vector2.zero;

                switch (HydroHarvGrowBed.GetHydroHarvSize())
                {
                    case HydroHarvSize.Unknown:
                        size = new Vector2(1,1);
                        break;
                    case HydroHarvSize.Large:
                        size = new Vector2(2, 2);
                        break;
                    case HydroHarvSize.Medium:
                        size = new Vector2(1, 2);
                        break;
                    case HydroHarvSize.Small:
                        size = Vector2.one;
                        break;
                }

                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,HydroponicHarvestersBuildable.DNADropContainerTitle(), HydroponicHarvestersBuildable.NotAllowedItem(), HydroponicHarvestersBuildable.StorageFull(), HydroHarvGrowBed, (int)size.x, (int)size.y);
            }

            if (CleanerManager == null)
            {
                CleanerManager = gameObject.AddComponent<HydroHarvCleanerManager>();
                CleanerManager.Initialize(this);
            }

            if (CleanerDumpContainer == null)
            {
                CleanerDumpContainer = gameObject.AddComponent<DumpContainer>();
                CleanerDumpContainer.Initialize(transform, HydroponicHarvestersBuildable.FloraKleenDropContainerTitle(), HydroponicHarvestersBuildable.NotAllowedItem(), HydroponicHarvestersBuildable.StorageFull(), CleanerManager, 1, 1);
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if(PowerManager == null)
            {
                PowerManager = gameObject.AddComponent<HydroHarvPowerManager>();
                PowerManager.Initialize(this);
            }

            if (Producer == null)
            {
                Producer = gameObject.AddComponent<HydroHarvGenerator>();
                Producer.Initialize(this);
            }

            if (EffectManager == null)
            {
                EffectManager = gameObject.AddComponent<HydroHarvEffectManager>();
                EffectManager.Initialize(this);
            }

            if (LightManager == null)
            {
                LightManager = gameObject.AddComponent<HydroHarvLightManager>();
                LightManager.Initialize(this);
            }

            if (HydroHarvCleanerManager == null)
            {
                HydroHarvCleanerManager = gameObject.AddComponent<HydroHarvCleanerManager>(); 
                HydroHarvCleanerManager.Initialize(this);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<HydroHarvDisplayManager>();
                DisplayManager.Setup(this);
                DisplayManager.OnContainerUpdate(0,0);
            }

            if (FCSConnectableDevice == null)
            {
                FCSConnectableDevice = gameObject.AddComponent<FCSConnectableDevice>();
                FCSConnectableDevice.Initialize(this, HydroHarvContainer);
            }
        }

        internal void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                ||!IsConstructed
                ||PrefabId == null 
                || newSaveData == null 
                || ColorManager == null 
                || HydroHarvContainer == null 
                || Producer == null 
                || LightManager == null 
                || HydroHarvGrowBed == null 
                || HydroHarvCleanerManager == null) return;

            var id = PrefabId.Id;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }
            
            _savedData.ID = id;
            _savedData.BodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _savedData.Container = HydroHarvContainer.Save();
            _savedData.GenerationProgress = Producer.GenerationProgress;
            _savedData.CoolDownProgress = Producer.CoolDownProgress;
            _savedData.StartUpProgress = Producer.StartUpProgress;
            _savedData.LightState = LightManager.GetLightSwitchedOff();
            _savedData.CurrentSpeedMode = CurrentSpeedMode;
            _savedData.BedType = HydroHarvGrowBed.GetBedType();
            _savedData.DnaSamples = HydroHarvGrowBed.GetDNASamples();
            _savedData.UnitSanitation = HydroHarvCleanerManager.GetUnitSanitation();
            newSaveData.Entries.Add(_savedData);
        }

        internal void ToggleMode()
        {
            if (HydroHarvGrowBed.HasItems())
            {
                QuickLogger.Message("Please remove all items from the container before switching modes",true);
                return;
            }

            switch (HydroHarvGrowBed.GetBedType())
            {
                case FCSEnvironment.Air:
                    HydroHarvGrowBed.SetBedType(FCSEnvironment.Water);
                    break;
                case FCSEnvironment.Water:
                    HydroHarvGrowBed.SetBedType(FCSEnvironment.Air);
                    break;
            }

            DisplayManager.ToggleMode(HydroHarvGrowBed.GetBedType());
        }
    }
}