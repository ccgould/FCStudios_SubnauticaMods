using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStorageSolutions.Abstract;
using DataStorageSolutions.Buildables;
using DataStorageSolutions.Configuration;
using DataStorageSolutions.Model;
using DataStorageSolutions.Patches;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Enums;
using FCSCommon.Extensions;
using FCSCommon.Interfaces;
using FCSCommon.Utilities;
using FCSTechFabricator.Components;
using FCSTechFabricator.Enums;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using UnityEngine;

namespace DataStorageSolutions.Mono
{
    internal class DSSTerminalController : DataStorageSolutionsController,IBaseUnit
    {
        private bool _isContructed;
        private string _prefabID;
        private TechType _techType = TechType.None;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _savedData;
        private bool _fromSave;
        private bool _isBeingDestroyed;

        public override bool IsConstructed => _isContructed;
        public override BaseManager Manager { get; set; }

        public TechType TechType => GetTechType();
        internal AnimationManager AnimationManager { get; private set; }
        public DSSTerminalDisplay DisplayManager { get; private set; }
        internal ColorManager TerminalColorManager { get; private set; }


        private void OnEnable()
        {

            //TODO Put a Message on the screen saying it cant find the manager
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

                    if (_savedData?.TerminalBodyColor != null)
                    {
                        TerminalColorManager.SetMaskColorFromSave(_savedData.TerminalBodyColor.Vector4ToColor());
                    }

                    FindBaseById(_savedData?.BaseID);
                }

                UpdateScreen();
                _runStartUpOnEnable = false;
                IsInitialized = true;
            }
        }
        
        private void OnDestroy()
        {
            IsInitialized = false;
            _isBeingDestroyed = true;
            Manager?.RemoveTerminal(this);
        }

        private TechType GetTechType()
        {
            if (_techType != TechType.None) return _techType;

            var techTag = gameObject.GetComponentInChildren<TechTag>();
            _techType = techTag != null ? techTag.type : TechType.None;

            return _techType;
        }

        private void OnPowerUpdate(FCSPowerStates obj, BaseManager manager)
        {
            switch (obj)
            {
                case FCSPowerStates.Powered:
                    DisplayManager.PowerOnDisplay();
                    break;
                case FCSPowerStates.Tripped:
                    DisplayManager.PowerOffDisplay();
                    break;
                case FCSPowerStates.Unpowered:
                    DisplayManager.PowerOffDisplay();
                    break;
            }
        }

        //private void OnBaseUpdate()
        //{
        //    GetData();
        //    UpdateScreen();
        //}

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        private void OnAntennaBuilt(bool isBuilt)
        {
            if (isBuilt)
            {
                UpdateScreen();
                DisplayManager.UpdateAntennaColorPage();
            }
        }

        private void FindBaseById(string id)
        {
            if(Manager == null)
                Manager = BaseManager.FindManager(id);
        }

        public string GetPrefabIDString()
        {
            if (string.IsNullOrEmpty(_prefabID))
            {
                var id = GetComponentInChildren<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
                
                if (id != null)
                {
                    _prefabID = id.Id;
                }
            }

            return _prefabID;
        }
        
        public override void Initialize()
        {
            Manager = BaseManager.FindManager(gameObject);

            if (TerminalColorManager == null)
            {
                TerminalColorManager = gameObject.AddComponent<ColorManager>();
                TerminalColorManager.Initialize(gameObject, DSSModelPrefab.BodyMaterial);
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.AddComponent<AnimationManager>();
            }
            
            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<DSSTerminalDisplay>();
                DisplayManager.Setup(this);
            }
            
            Mod.OnAntennaBuilt += OnAntennaBuilt;
            Manager?.RegisterTerminal(this);
            //Mod.OnBaseUpdate += OnBaseUpdate;
        }
        
        public override void UpdateScreen()
        {
            if (_isBeingDestroyed) return;
            QuickLogger.Debug($"Refreshing Monitor: {GetPrefabIDString()}", true);
            DisplayManager.Refresh();
        }
        
        private void OnVehicleUpdate(List<Vehicle> vehicles, BaseManager baseManager)
        {
            if (baseManager == Manager)
            {
                DisplayManager.RefreshVehicles(vehicles);
            }
        }

        private void OnVehicleStorageUpdate(BaseManager manager)
        {
            if (manager == Manager)
            {
                DisplayManager.RefreshVehicleItems();
            }
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");

            if (!Mod.IsSaving())
            {
                var id = GetPrefabIDString();
                QuickLogger.Info($"Saving {id}");
                Mod.Save();
                QuickLogger.Info($"Saved {id}");
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

        public override void Save(SaveData newSaveData)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            var id = GetPrefabIDString();

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }

            _savedData.ID = id;
            _savedData.TerminalBodyColor = TerminalColorManager.GetMaskColor().ColorToVector4();
            _savedData.BaseID = Manager.InstanceID;
            newSaveData.Entries.Add(_savedData);
        }

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            _isContructed = constructed;

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
    }
}