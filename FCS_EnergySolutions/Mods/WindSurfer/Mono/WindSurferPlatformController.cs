using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.WindSurfer.Mono
{
    internal class WindSurferPlatformController : WindSurferPlatformBase
    {
        private WindSurferDataEntry _savedData;
        private bool _fromSave;
        private PlatformController _platformController;
        public override WindSurferPowerController PowerController { get; set; }
        public override PlatformController PlatformController => _platformController ?? (_platformController = GetComponent<PlatformController>());
        public override bool BypassRegisterCheck { get; } = true;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.WindSurferPlatformTabID, Mod.ModPackID);
        }

        public override void Initialize()
        {
            base.Initialize();
            IsInitialized = true;
        }

        private void OnEnable()
        {
            if (!IsInitialized)
            {
                Initialize();
            }

            if (_savedData == null)
            {
                ReadySaveData();
            }

            if (_savedData != null && _fromSave)
            {
                _colorManager.LoadTemplate(_savedData.ColorTemplate);
            }
        }
        
        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetWindSurferSaveData(id);
        }


        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.WindSurferFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.WindSurferFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;
            return false;
        }

        public override void OnConstructedChanged(bool constructed)
        {

        }

        public override void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized) return;

            if (_savedData == null)
            {
                _savedData = new WindSurferDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
            _savedData.BaseId = BaseId;
            _savedData.Position = transform.position.ToVec3();
            _savedData.Rotation = transform.rotation.QuaternionToVec4();
            newSaveData.WindSurferEntries.Add(_savedData);
        }

        public override void TryMoveToPosition()
        {
            ReadySaveData();
            transform.position = _savedData.Position.ToVector3();
            transform.rotation = _savedData.Rotation.Vec4ToQuaternion();
        }

        public override void PoleState(bool value)
        {

        }
    }
}