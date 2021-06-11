using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.BaseOperator.Mono
{
    internal class BaseOperatorController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private BaseOperatorDataEntry _savedData;
        private BaseOperatorDisplay _displayManager;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.BaseOperatorTabID, Mod.ModPackID);
            _displayManager?.SetBaseName();
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

                    _colorManager.ChangeColor(_savedData.Fcs.Vector4ToColor());
                    _colorManager.ChangeColor(_savedData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetBaseOperatorDataEntrySaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            if (_displayManager == null)
            {
                _displayManager = gameObject.AddComponent<BaseOperatorDisplay>();
                _displayManager.Setup(this);
            }

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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            if (!IsInitialized || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new BaseOperatorDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Fcs = _colorManager.GetColor().ColorToVector4();
            _savedData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.BaseOperatorEntries.Add(_savedData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}
