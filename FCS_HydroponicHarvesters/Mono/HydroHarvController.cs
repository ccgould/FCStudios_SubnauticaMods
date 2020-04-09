using System;
using System.Collections.Generic;
using FCS_HydroponicHarvesters.Buildables;
using FCS_HydroponicHarvesters.Configuration;
using FCS_HydroponicHarvesters.Enumerators;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
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
        
        public SpeedModes CurrentSpeedMode
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

        #region Unity

        private void OnEnabled()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                //if (DisplayManager != null)
                //{

                //}

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    QuickLogger.Info($"Loaded {Mod.LargeFriendlyName}");
                }

                _runStartUpOnEnable = false;
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

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
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

            if (HydroHarvContainer == null)
            {
                HydroHarvContainer = gameObject.AddComponent<HydroHarvContainer>();
                HydroHarvContainer.Initialize(this);
            }

            if (HydroHarvGrowBed == null)
            {
                HydroHarvGrowBed = gameObject.AddComponent<HydroHarvGrowBed>();
                HydroHarvGrowBed.Initialize(this);
            }

            if (DumpContainer == null)
            {
                DumpContainer = gameObject.AddComponent<DumpContainer>();
                DumpContainer.Initialize(transform,"Please Replace","Item Not Allowed","Storage is full", HydroHarvGrowBed,1,1);
            }

            if (CleanerManager == null)
            {
                CleanerManager = gameObject.AddComponent<HydroHarvCleanerManager>();
                CleanerManager.Initialize(this);
            }

            if (CleanerDumpContainer == null)
            {
                CleanerDumpContainer = gameObject.AddComponent<DumpContainer>();
                CleanerDumpContainer.Initialize(transform, "Please Replace", "Item Not Allowed", "Storage is full", CleanerManager, 1, 1);
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

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<HydroHarvDisplayManager>();
                DisplayManager.Setup(this);
            }

            IsInitialized = true;
        }

        internal void Save(SaveData newSaveData)
        {
            var id = PrefabId.Id;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
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