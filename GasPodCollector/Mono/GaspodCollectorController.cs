using System;
using System.Collections.Generic;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using GasPodCollector.Buildables;
using GasPodCollector.Configuration;
using GasPodCollector.Models;
using GasPodCollector.Mono.Managers;
using UnityEngine;

namespace GasPodCollector.Mono
{
    internal class GaspodCollectorController : FCSController
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private int _isExpanded;
        private bool _expand;
        private float _animationDelay = 1.5f;
        private int _page;
        public override bool IsConstructed { get; }
        public override bool IsInitialized { get; set; }
        internal GaspodManager GaspodManager { get; set; }
        internal GaspodCollectorStorage GaspodCollectorStorage { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal ColorManager ColorManager { get; private set; }
        public GasopodCollectorDisplayManager DisplayManager { get; private set; }
        public GasopdCollectorPowerManager PowerManager { get; private set; }

        #region Unity Methods

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

                    ColorManager.SetColorFromSave(_savedData.BodyColor.Vector4ToColor());
                    GaspodCollectorStorage.SetStorageAmount(_savedData.GaspodAmount);

                    QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                }

                _runStartUpOnEnable = false;
            }
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (_expand)
            {
                _animationDelay -= Time.deltaTime;
                if (_animationDelay <= 0)
                {
                    AnimationManager.SetBoolHash(_isExpanded, true);
                    AnimationManager.SetIntHash(_page, 1);
                    _expand = false;
                }

            }
        }

        #endregion

        public override void Initialize()
        {

            _isExpanded = Animator.StringToHash("IsExpanded");
            _page = Animator.StringToHash("Page");

            if (GaspodManager == null)
            {
                GaspodManager = gameObject.GetComponent<GaspodManager>();
                GaspodManager.Initialize(this);
            }

            if (GaspodCollectorStorage == null)
            {
                GaspodCollectorStorage = gameObject.GetComponent<GaspodCollectorStorage>();
                GaspodCollectorStorage.OnGaspodCollected += OnGaspodCollected; 
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.GetComponent<AnimationManager>();
                _expand = true;
            }

            if (ColorManager == null)
                ColorManager = new ColorManager();

            ColorManager.Initialize(gameObject, GaspodCollectorBuildable.BodyMaterial);

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.GetComponent<GasopodCollectorDisplayManager>();
                DisplayManager.Setup(this);
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.GetComponent<GasopdCollectorPowerManager>();
                PowerManager.Setup(this);
            }

            IsInitialized = true;
        }

        private void OnGaspodCollected()
        {
            PowerManager.TakePower();
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_savedData == null)
            {
                ReadySaveData();
            }

            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (GaspodCollectorStorage != null && GaspodCollectorStorage.GetStorageAmount() > 0)
            {
                reason = GaspodCollectorBuildable.NotEmpty();
                return false;
            }

            if (PowerManager != null && PowerManager.HasPower())
            {
                reason = GaspodCollectorBuildable.HasBatteries();
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
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
            _savedData.GaspodAmount = GaspodCollectorStorage.GetStorageAmount();
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.Batteries = PowerManager.GetBatteries();
            saveData.Entries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        internal void ChangePage(int pageNumber)
        {
            AnimationManager.SetIntHash(_page,pageNumber);
        }

        public void UpdateBatteryDisplay(Dictionary<int, BatteryInfo> batteries)
        {
            DisplayManager.UpdateBatteries(batteries);
        }
    }
}