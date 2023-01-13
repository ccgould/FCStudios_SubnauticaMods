using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
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
        public virtual void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DecorationItemTabId, Mod.ModPackID);
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

                    _colorManager.LoadTemplate(_savedData.ColorTemplate);
                }

                _runStartUpOnEnable = false;
            }
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,string.Empty);

            }

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, Color.cyan);
            MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject,3f); 
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

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new DecorationDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();
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

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
        }
    }
}