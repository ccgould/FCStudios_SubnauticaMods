using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.Shower.Buildable;
using FCS_HomeSolutions.Mods.Shower.Mono;
using FCS_HomeSolutions.Mods.Sink.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.Mods.Sink.Mono
{
    internal class SinkController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DecorationDataEntry _saveData;
        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, ShowerBuildable.ShowerTabID, Mod.ModPackID);
        }

        public override float GetPowerUsage()
        {
            return 0.01f;
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
                _colorManager.ChangeColor(_saveData.Fcs.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.Secondary.Vector4ToColor(), ColorTargetMode.Secondary);
                _colorManager.ChangeColor(_saveData.Emission.Vector4ToColor(), ColorTargetMode.Emission);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDecorationDataEntrySaveData(id);
        }

        public override void Initialize()
        {
            var sinkController = GameObjectHelpers.FindGameObject(gameObject, "HandTarget").EnsureComponent<ShowerController>();
            sinkController.TurnOnText = "Turn On Sink";
            sinkController.TurnOffText = "Turn Off Sink";

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,AlterraHub.BaseLightsEmissiveController);
            }

            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject, new Color(0, 1, 1, 1));
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, new Color(0.8f, 0.4933333f, 0f));
            MaterialHelpers.ChangeEmissionStrength(AlterraHub.BaseDecalsEmissiveController, gameObject, 2.5f);

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {SinkBuildable.SinkFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {SinkBuildable.SinkFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new DecorationDataEntry();
            }
            _saveData.Id = id;
            _saveData.Fcs = _colorManager.GetColor().ColorToVector4();
            _saveData.Secondary = _colorManager.GetSecondaryColor().ColorToVector4();
            _saveData.Emission = _colorManager.GetLumColor().ColorToVector4();

            newSaveData.DecorationEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
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

        public override void OnHandHover(GUIHand hand)
        {
            if (!IsInitialized || !IsConstructed) return;
            base.OnHandHover(hand);

            var data = new string[]{};
            data.HandHoverPDAHelperEx(GetTechType());
        }

        public void OnHandClick(GUIHand hand)
        {

        }
    }
}
