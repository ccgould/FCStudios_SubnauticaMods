﻿using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
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
                _colorManager.LoadTemplate(_saveData.ColorTemplate);
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
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol,string.Empty);
            }

            MaterialHelpers.ChangeEmissionColor(string.Empty, gameObject, new Color(0, 1, 1, 1));
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseSecondaryCol, gameObject, new Color(0.8f, 0.4933333f, 0f));
            MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 2.5f);

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
            _saveData.ColorTemplate = _colorManager.SaveTemplate();


            newSaveData.DecorationEntries.Add(_saveData);
        }

        public override bool ChangeBodyColor(ColorTemplate template)
        {
            return _colorManager.ChangeColor(template);
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
