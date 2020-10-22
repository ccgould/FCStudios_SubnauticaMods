using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Buildables
{
    internal class DecorationController : FcsDevice,IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private DecorationDataEntry _savedData;
        public ColorManager ColorManager { get; private set; }
        
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

                    ColorManager.ChangeColor(_savedData.Color.Vector4ToColor(),ColorTargetMode.Both);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject,ModelPrefab.BodyMaterial);
            }

            IsInitialized = true;
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {GetPrefabID()}");
                Mod.Save();
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

        public void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new DecorationDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.Color = ColorManager.GetColor().ColorToVector4();
            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.DecorationEntries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetDecorationDataEntrySaveData(GetPrefabID());
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

        public override void ChangeBodyColor(Color color,ColorTargetMode mode)
        {
            ColorManager.ChangeColor(color,mode);
        }
    }
}