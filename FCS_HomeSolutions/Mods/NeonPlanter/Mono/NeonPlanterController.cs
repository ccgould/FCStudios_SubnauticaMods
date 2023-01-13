﻿using System;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Model;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Configuration;
using FCS_HomeSolutions.Mods.NeonPlanter.Buildable;
using FCSCommon.Utilities;

namespace FCS_HomeSolutions.Mods.NeonPlanter.Mono
{
    internal class NeonPlanterController : FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private bool _isFromSave;
        private PlanterDataEntry _savedData;


        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, NeonPlanterPatch.NeonPlanterTabID, Mod.ModPackID);
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

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetPlanterDataEntrySaveData(GetPrefabID());
        }

        public void Save(SaveData newSaveData,ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new PlanterDataEntry();
            }

            _savedData.Id = GetPrefabID();
            _savedData.ColorTemplate = _colorManager.SaveTemplate();

            QuickLogger.Debug($"Saving ID {_savedData.Id}");
            newSaveData.PlanterEntries.Add(_savedData);
        }

        public override void Initialize()
        {
            _colorManager = gameObject.AddComponent<ColorManager>();
            _colorManager.Initialize(gameObject,AlterraHub.BasePrimaryCol, AlterraHub.BaseSecondaryCol, string.Empty);
            MaterialHelpers.ChangeEmissionStrength(string.Empty, gameObject, 2);

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
            reason = String.Empty;
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

    internal struct PlantData
    {
        public float Age { get; set; }
        public int Slot { get; set; }

        public PlantData(float age,int slot)
        {
            Age = age;
            Slot = slot;
        }
    }
}
