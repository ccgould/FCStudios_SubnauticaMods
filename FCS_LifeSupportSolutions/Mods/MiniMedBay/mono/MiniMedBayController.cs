using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_LifeSupportSolutions.Buildable;
using FCS_LifeSupportSolutions.Configuration;
using FCS_LifeSupportSolutions.Mods.MiniMedBay.Enumerators;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Mods.MiniMedBay.mono
{
    internal class MiniMedBayController: FcsDevice,IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private ColorManager _colorManager;
        internal MiniMedBayDisplay DisplayManager;
        private MiniMedBayEntry _savedData;
        internal AudioManager AudioHandler;
        internal MiniMedBayContainer Container;
        internal MiniMedBayBedManager HealBedManager;
        private MedKitDispenser _medKitDispenser;
        public override bool IsOperational => IsConstructed && IsInitialized && Manager != null && Manager.HasEnoughPower(GetPowerUsage());
        public MiniMedBayTrigger Trigger { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.MiniMedBayTabID, Mod.ModName);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_isFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    Container.NumberOfFirstAids = _savedData.FirstAidCount;
                    Container.SetTimeToSpawn(_savedData.TimeToSpawn);
                    _colorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.SecondaryBodyColor.Vector4ToColor(), ColorTargetMode.Secondary);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (Trigger == null)
            {
                Trigger = gameObject.FindChild("Trigger").AddComponent<MiniMedBayTrigger>();
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<MiniMedBayDisplay>();
                DisplayManager.Setup(this);
            }

            if (AudioHandler == null)
            {
                AudioHandler = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
            }
            
            if (Container == null)
            {
                Container = gameObject.AddComponent<MiniMedBayContainer>();
                Container.Initialize(this);
            }

            if (HealBedManager == null)
            {
                HealBedManager = gameObject.AddComponent<MiniMedBayBedManager>();
                HealBedManager.Initialize(this);
            }

            if (_medKitDispenser == null)
            {
                _medKitDispenser = InterfaceHelpers.FindGameObject(gameObject, "Unity_Collison_Object_0_2").AddComponent<MedKitDispenser>();
                _medKitDispenser.Initialize(this);
            }

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissiveControllerMaterial,gameObject,5f);

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {GetPrefabID()}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            _isFromSave = true;
        }

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

                    IsInitialized = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
        
        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new MiniMedBayEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.BodyColor = _colorManager.GetColor().ColorToVector4();
            _savedData.SecondaryBodyColor = _colorManager.GetSecondaryColor().ColorToVector4();
            _savedData.FirstAidCount = Container.NumberOfFirstAids;
            _savedData.TimeToSpawn = Container.GetTimeToSpawn();
            newSaveData.MiniMedBayEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetMiniMedBaySaveData(GetPrefabID());
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            QuickLogger.Debug($"Change Color: {color} {mode}",true);
            return _colorManager.ChangeColor(color, mode);
        }

        public override float GetPowerUsage()
        {
            return HealBedManager != null && HealBedManager.IsHealing ? 8.0f : 0f;
        }
    }
}