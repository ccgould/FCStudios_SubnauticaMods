using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;

namespace DataStorageSolutions.Mono
{
    internal class DSSAntennaController : DataStorageSolutionsController, IBaseAntenna
    {
        private bool _isContructed;
        private PowerRelay _powerRelay;
        private float _energyToConsume;
        private bool _showConsumption = true;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;

        public override bool IsConstructed => _isContructed;
        public SubRoot SubRoot { get; private set; }
        public BaseManager Manager { get; set; }
        public TechType TechType => GetTechType();
        public ColorManager ColorManager { get; set; }
        
        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }
        
        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

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

                Manager?.SetBaseName(_savedData.AntennaName);

                if (_savedData?.AntennaBodyColor != null)
                {
                    ColorManager.SetMaskColorFromSave(_savedData.AntennaBodyColor.Vector4ToColor());
                }
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabIDString());
        }

        private void Update()
        {
            FindSubRoot();
            if (SubRoot == null || !_isContructed) return;
            ConsumePower();
        }

        private void OnDestroy()
        {
            BaseManager.RemoveAntenna(this);
            Mod.OnAntennaBuilt?.Invoke(false);
        }

        private void ConsumePower()
        {
            _energyToConsume = QPatch.Configuration.Config.AntennaPowerUsage * DayNightCycle.main.deltaTime;
            
            bool requiresEnergy = GameModeUtils.RequiresPower();

            if (!requiresEnergy) return;

            _powerRelay.ConsumeEnergy(_energyToConsume, out var amountConsumed);

            if (_showConsumption)
            {
                QuickLogger.Debug($"Energy Used by Antenna {GetPrefabIDString()}: {amountConsumed}");
            }
        }

        private void FindSubRoot()
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }
        }

        private void GetPowerRelay()
        {
            if (SubRoot != null)
            {
                _powerRelay = SubRoot.powerRelay;
            }
        }
        
        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                _prefabID = id != null ? id.Id : string.Empty;
            }

            return _prefabID;
        }

        internal void ToggleShowConsumption()
        {
            _showConsumption = !true;
        }
        
        public override void Initialize()
        {
            GetData();
            Manager.AddAntenna(this);

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }
            
            Mod.OnAntennaBuilt?.Invoke(true);

            IsInitialized = true;
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
            _savedData.AntennaBodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _savedData.AntennaName = ((IBaseAntenna) this).GetName(); //TODO Get From BaseManager
            newSaveData.Entries.Add(_savedData);
        }

        //TODO Check if needed
        internal List<DSSRackController> GetData()
        {
            if (!_isContructed) return null;

            FindSubRoot();

            Manager = Manager ?? BaseManager.FindManager(SubRoot);

            return Manager.BaseUnits;
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
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        string IBaseAntenna.GetName()
        {
            return $"Antenna: {GetPrefabIDString()}";
        }
    }
}
