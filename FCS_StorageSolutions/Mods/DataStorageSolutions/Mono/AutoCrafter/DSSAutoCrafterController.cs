﻿using System;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_StorageSolutions.Configuration;
using FCS_StorageSolutions.Mods.AlterraStorage.Buildable;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_StorageSolutions.Mods.DataStorageSolutions.Mono.AutoCrafter
{
    internal class DSSAutoCrafterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private DSSAutoCrafterDataEntry _saveData;

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.DSSTabID, Mod.ModName);
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
                _colorManager.ChangeColor(_saveData.Body.Vector4ToColor());
                _colorManager.ChangeColor(_saveData.SecondaryBody.Vector4ToColor(), ColorTargetMode.Secondary);
                _fromSave = false;
            }
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _saveData = Mod.GetDSSAutoCrafterSaveData(id);
        }

        public override void Initialize()
        {
            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial, ModelPrefab.SecondaryMaterial);
            }

            IsInitialized = true;

            QuickLogger.Debug($"Initialized");
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.DSSAutoCrafterFriendlyName}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.DSSAutoCrafterFriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer = null)
        {
            try
            {
                var prefabIdentifier = GetComponent<PrefabIdentifier>();
                var id = prefabIdentifier.Id;

                if (_saveData == null)
                {
                    _saveData = new DSSAutoCrafterDataEntry();
                }

                _saveData.ID = id;
                _saveData.Body = _colorManager.GetColor().ColorToVector4();
                _saveData.SecondaryBody = _colorManager.GetSecondaryColor().ColorToVector4();

                newSaveData.DSSAutoCrafterDataEntries.Add(_saveData);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Failed to save {UnitID}:");
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Error(e.InnerException);
            }
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
    }
}