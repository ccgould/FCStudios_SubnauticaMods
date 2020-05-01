using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Enums;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;

namespace DataStorageSolutions.Mono
{
    internal class DSSTerminalController : DataStorageSolutionsController,IBaseUnit
    {
        private bool _isContructed;
        private PowerRelay _powerRelay;
        private float _energyToConsume;
        private bool _showConsumption = true;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable;
        private ColorManager _antennaColorController;
        private SaveDataEntry _savedData;
        private bool _fromSave;
        private float _amountConsumed;

        public override bool IsConstructed => _isContructed;
        public SubRoot SubRoot { get; private set; }
        public BaseManager Manager { get; set; }
        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        public DSSTerminalDisplay DisplayManager { get; private set; }
        internal ColorManager TerminalColorManager { get; private set; }
        public ColorManager AntennaColorManager => GetAntennaColorManager();

        public PowerManager PowerManager { get; private set; }

        private ColorManager GetAntennaColorManager()
        {
            if (_antennaColorController != null) return _antennaColorController;

            if (Manager == null) return null;

            _antennaColorController = Manager?.GetCurrentBaseAntenna()?.ColorManager;

            return _antennaColorController != null ? _antennaColorController : null;
        }

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

                    if (_savedData?.TerminalBodyColor != null)
                    {
                        TerminalColorManager.SetMaskColorFromSave(_savedData.TerminalBodyColor.Vector4ToColor());
                    }
                }

                UpdateScreen();
                _runStartUpOnEnable = false;
                IsInitialized = true;
            }
        }
        
        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }

        private void Update()
        {
            FindSubRoot();
            
            if(IsConstructed && PowerManager != null)
            {
                PowerManager?.UpdatePowerState();
                PowerManager?.ConsumePower();
                QuickLogger.Debug($"Terminal {GetPrefabIDString()} Power Usage: {PowerManager.GetPowerUsage()}");
            }
        }

        private void FindSubRoot()
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }
        }
        
        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                
                if (id != null)
                {
                    _prefabID = id.Id;
                }
            }

            return _prefabID;
        }
        
        public override void Initialize()
        {
            GetData();

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(gameObject,QPatch.Configuration.Config.ScreenPowerUsage);
                PowerManager.OnPowerUpdate += OnPowerUpdate;
            }

            if (TerminalColorManager == null)
            {
                TerminalColorManager = gameObject.AddComponent<ColorManager>();
                TerminalColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSTerminalDisplay>();
                DisplayManager.Setup(this);
            }
            
            Mod.OnAntennaBuilt += OnAntennaBuilt;
            Mod.OnBaseUpdate += OnBaseUpdate;
        }

        private void OnPowerUpdate(FCSPowerStates obj)
        {
            QuickLogger.Debug($"Terminal {GetPrefabIDString()} Power State Updated", true);
            switch (obj)
            {
                case FCSPowerStates.Powered:
                    DisplayManager.PowerOnDisplay();
                    break;
                case FCSPowerStates.Tripped:
                case FCSPowerStates.Unpowered:
                    DisplayManager.PowerOffDisplay();
                    break;
            }
        }

        private void OnBaseUpdate()
        {
            GetData();
            UpdateScreen();
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        public override void UpdateScreen()
        {
            QuickLogger.Debug($"Refreshing Monitor: {GetPrefabIDString()}", true);
            DisplayManager.Refresh();
        }

        private void OnAntennaBuilt(bool isBuilt)
        {
            UpdateScreen();

            if (!isBuilt)
            {
                _antennaColorController = null;
            }

            DisplayManager.UpdateAntennaColorPage();
        }

        internal List<DSSRackController> GetData()
        {
            FindSubRoot();

            if (SubRoot)
            {
           
                if (!_isContructed ) return null;

                Manager = Manager ?? BaseManager.FindManager(SubRoot);
                
                return Manager.BaseUnits;
            }

            QuickLogger.Error("SubRoot not found");
            return null;
        }
        
        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabIDString();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
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

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.TerminalBodyColor = TerminalColorManager.GetMaskColor().ColorToVector4();
            newSaveData.Entries.Add(_savedData);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            _isContructed = constructed;

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
    }
}
