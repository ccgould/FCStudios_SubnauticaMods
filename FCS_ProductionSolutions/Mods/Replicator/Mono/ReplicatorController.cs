using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCS_ProductionSolutions.HydroponicHarvester.Enumerators;
using FCS_ProductionSolutions.HydroponicHarvester.Mono;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_ProductionSolutions.Mods.Replicator.Mono
{
    internal class ReplicatorController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private ReplicatorDataEntry _saveData;
        private ColorManager _colorManager;
        private ReplicatorSlot _replicatorSlot;
        private const float PowerUsage = 0.85f;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.ReplicatorTabID, Mod.ModName);
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_saveData == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                _colorManager.ChangeColor(_saveData.BodyColor.Vector4ToColor());
                _fromSave = false;
            }
        }

        private void ChangeSpeed()
        {
            if (_replicatorSlot != null)
            {
                _replicatorSlot.CurrentSpeedMode = SpeedModes.Max;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetReplicatorSaveData(id);
        }

        public override float GetPowerUsage()
        {
            switch(_replicatorSlot.CurrentSpeedMode)
            {
                case SpeedModes.Off:
                return 0;
                case SpeedModes.Max:
                return PowerUsage * 4;
                case SpeedModes.High:
                return PowerUsage * 3;
                case SpeedModes.Low:
                return PowerUsage * 2;
                case SpeedModes.Min:
                return PowerUsage;
                default:
                return 0f;
            }
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (_replicatorSlot == null)
            {
                _replicatorSlot = gameObject.AddComponent<ReplicatorSlot>();
                _replicatorSlot.Initialize(this);
            }

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial,gameObject,5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.ReplicatorFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.ReplicatorFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
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
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new ReplicatorDataEntry();
            }
            _saveData.ID = id;
            _saveData.BodyColor = _colorManager.GetColor().ColorToVector4();
            newSaveData.ReplicatorEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
