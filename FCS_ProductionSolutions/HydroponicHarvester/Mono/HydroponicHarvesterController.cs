using System;
using System.Reflection.Emit;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class HydroponicHarvesterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private HydroponicHarvesterDataEntry _savedData;
        private EffectsManager _effectsManager;
        private bool _isInBase;

        public EffectsManager EffectsManager => _effectsManager;
        
        public AudioManager AudioManager { get; private set; }
        public Action<bool> onUpdateSound { get; private set; }
        public ColorManager ColorManager { get; private set; }
        public GrowBedManager GrowBedManager { get; set; }

        public override bool IsOperational => IsOperationalCheck();
        
        #region Unity Methods

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

                    ColorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor(), ColorTargetMode.Both);
                    _isInBase = _savedData.IsInBase;
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {

        }

        #endregion

        private bool IsOperationalCheck()
        {
            //Returning true until further notice
            return true;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetHydroponicHarvesterSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            //if (AudioManager == null)
            //{
            //    AudioManager = new AudioManager(gameObject.EnsureComponent<FMOD_CustomLoopingEmitter>());
            //    AudioManager.PlayMachineAudio();
            //}
            
            //onUpdateSound += value =>
            //{
            //    if (value)
            //    {
            //        AudioManager.PlayMachineAudio();
            //    }
            //    else
            //    {
            //        AudioManager.StopMachineAudio();
            //    }
            //};
            
            if (GrowBedManager == null)
            {
                GrowBedManager = gameObject.AddComponent<GrowBedManager>();
                GrowBedManager.Initialize(this);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

            if (EffectsManager == null)
            {
                _effectsManager = new EffectsManager(this);
                InvokeRepeating(nameof(CheckSystem), .5f, .5f);
            }

            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.HydroponicHarvesterModTabID);

            GameObjectHelpers.FindGameObject(gameObject, "UNITID").GetComponent<Text>().text = $"UNIT ID: {UnitID}";

#if DEBUG
            QuickLogger.Debug($"Initialized Harvester {GetPrefabID()}");
#endif

            MaterialHelpers.ChangeEmissionStrength(ModelPrefab.EmissionControllerMaterial, gameObject, 4f);
            
            IsInitialized = true;
        }

        private void CheckSystem()
        {
            _effectsManager?.ToggleLightsByDistance();
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

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (GrowBedManager != null)
            {
                var hasItems = GrowBedManager.HasItems();
                if (hasItems)
                {
                    reason = $"Cannot destroy harvester {UnitID} because it has plants inside";
                    return false;
                }
            }

            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (Player.main != null)
                {
                    _isInBase = Player.main.IsInBase();
                }

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

        public void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new HydroponicHarvesterDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.IsInBase = _isInBase;
            newSaveData.HydroponicHarvesterEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return ColorManager.ChangeColor(color, mode);
        }

        public bool HasPowerToConsume()
        {
            return true;
        }

        public void ConsumePower()
        {
            
        }

        public bool IsInBase()
        {
            return _isInBase;
        }
    }
}