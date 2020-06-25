using System.Collections.Generic;
using System.Globalization;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSAntennaController : DataStorageSolutionsController, IBaseAntenna, IHandTarget
    {
        private bool _isContructed;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;

        public override bool IsConstructed => _isContructed;
        public override BaseManager Manager { get; set; }
        public TechType TechType => GetTechType();
        public ColorManager ColorManager { get; set; }
        public PowerManager PowerManager { get; private set; }

        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
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
                    
                    if (_savedData?.AntennaBodyColor != null)
                    {
                        ColorManager.SetMaskColorFromSave(_savedData.AntennaBodyColor.Vector4ToColor());
                    }
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
            if (IsConstructed && PowerManager != null && SubRoot != null)
            {
                PowerManager?.UpdatePowerState();
                PowerManager?.ConsumePower();
            }
        }

        private void OnDestroy()
        {
            IsInitialized = false;
            BaseManager.RemoveAntenna(this);
            Mod.OnAntennaBuilt?.Invoke(false);
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
                _prefabID = id != null ? id.Id : string.Empty;
            }

            return _prefabID;
        }

        public override void Initialize()
        {
            GetData();
            
            QuickLogger.Debug($"Antenna Count: {BaseManager.BaseAntennas.Count}");

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(this, QPatch.Configuration.Config.AntennaPowerUsage);
                PowerManager.OnPowerUpdate += OnPowerUpdate;
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            BaseManager.AddAntenna(this);

            Mod.OnAntennaBuilt?.Invoke(true);

            IsInitialized = true;
        }

        private void OnPowerUpdate(FCSPowerStates obj, BaseManager manager)
        {
            Mod.OnBaseUpdate?.Invoke();
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
            _savedData.AntennaName = Manager.GetBaseName();
            newSaveData.Entries.Add(_savedData);
        }

        internal HashSet<DSSRackController> GetData()
        {
            if (!_isContructed) return null;

            FindSubRoot();

            Manager = Manager ?? BaseManager.FindManager(SubRoot);

            return Manager.BaseRacks;
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
        
        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            var state = PowerManager?.GetPowerState() == FCSPowerStates.Powered ? "On" : "Off";
#if SUBNAUTICA
            main.SetInteractTextRaw(Manager?.GetBaseName(), $"{AuxPatchers.Antenna()}: {state} || {AuxPatchers.PowerUsage()}: {PowerManager?.GetPowerUsage():F1}");
#elif BELOWZERO
            main.SetText(HandReticle.TextType.Info, Manager.GetBaseName(), false);
#endif
            main.SetIcon(HandReticle.IconType.Info, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        public bool IsVisible()
        {
            return IsConstructed && PowerManager.GetPowerState() == FCSPowerStates.Powered;
        }

        public void ChangeColorMask(Color color)
        {
            ColorManager.ChangeColorMask(color);
        }
    }
}