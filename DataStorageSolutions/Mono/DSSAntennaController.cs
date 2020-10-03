using System.Collections.Generic;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Helpers;
using DataStorageSolutions.Interfaces;
using DataStorageSolutions.Model;
using FCSCommon.Utilities;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSAntennaController : DataStorageSolutionsController, IBaseAntenna, IHandTarget
    {
        #region Private Fields

        private bool _isContructed;
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;

        #endregion

        #region Public Properties

        public override bool IsConstructed => _isContructed;
        public override BaseManager Manager { get; set; }

        public ColorManager ColorManager { get; set; }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }
                    
                    if (_savedData?.AntennaBodyColor != null)
                    {
                        ColorManager.SetMaskColorFromSave(_savedData.AntennaBodyColor.Vector4ToColor());
                    }
                }
            }
        }

        private void OnDestroy()
        {
            IsInitialized = false;
            BaseManager.RemoveAntenna(this);
            Mod.OnAntennaBuilt?.Invoke(false);
        }

        #endregion
        
        public override void Initialize()
        {
            Manager = BaseManager.FindManager(gameObject);

            if (Manager == null)
            {
                //TODO Notify Player somehow.
                return;
            }


#if DEBUG
            QuickLogger.Debug($"Antenna Count: {BaseManager.BaseAntennas.Count}");
#endif

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            BaseManager.RegisterAntenna(this);
            
            PowerManager = Manager.BasePowerManager;

            IsInitialized = true;
        }
        
        public bool IsVisible()
        {
            return IsConstructed && IsInitialized && PowerManager.GetPowerState() == FCSPowerStates.Powered;
        }

        public void ChangeColorMask(Color color)
        {
            if (!IsInitialized) return;
            ColorManager.ChangeColorMask(color);
        }

        #region IProtoEventListener

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {

                QuickLogger.Info($"Saving {_prefabId}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {_prefabId}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }
            _fromSave = true;
        }

        #endregion

        #region IConstructable

        public override void OnConstructedChanged(bool constructed)
        {
            _isContructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Initialize();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        #endregion

        #region IHandTarget

        public void OnHandHover(GUIHand hand)
        {
            HandReticle main = HandReticle.main;
            
            var state = PowerManager?.GetPowerState() == FCSPowerStates.Powered ? "On" : "Off";
#if SUBNAUTICA
            if(!IsInitialized)
            {
                main.SetInteractTextRaw("Error","Base Not Found");
                return;
            }
            main.SetInteractTextRaw(Manager?.GetBaseName(), $"{AuxPatchers.Antenna()}: {state} || {AuxPatchers.PowerUsage()}: {QPatch.Configuration.Config.AntennaPowerUsage:F1}");
#elif BELOWZERO
            if(!IsInitialized)
            {
                main.SetText("Error","Base Not Found",false);
                return;
            }
            main.SetText(HandReticle.TextType.Info, Manager.GetBaseName(), false);
#endif
            main.SetIcon(HandReticle.IconType.Info, 1f);
        }

        public void OnHandClick(GUIHand hand)
        {

        }

        #endregion

        #region Save Handler

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetSaveData(GetPrefabID());
        }

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = GetPrefabID();
            _savedData.AntennaBodyColor = ColorManager.GetMaskColor().ColorToVector4();
            _savedData.AntennaName = Manager.GetBaseName();
            newSaveData.Entries.Add(_savedData);
        }
        
        #endregion

    }
}