using System;
using System.Collections.Generic;
using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Interfaces;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Registration;
using FCS_ProductionSolutions.Buildable;
using FCS_ProductionSolutions.Configuration;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;

namespace FCS_ProductionSolutions.HydroponicHarvester.Mono
{
    internal class HydroponicHarvesterController : FcsDevice, IFCSStorage, IFCSSave<SaveData>
    {
        private bool _isFromSave;
        private bool _runStartUpOnEnable;
        private HydroponicHarvesterDataEntry _savedData;
        public int GetContainerFreeSpace { get; }
        public bool IsFull { get; }
        public Action<int, int> OnContainerUpdate { get; set; }
        public Action<FcsDevice, TechType> OnContainerAddItem { get; set; }
        public Action<FcsDevice, TechType> OnContainerRemoveItem { get; set; }
        public bool IsOperational { get; set; }

        //public EffectsManager EffectsManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public Action<bool> onUpdateSound { get; private set; }
        public ColorManager ColorManager { get; private set; }

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {

        }

        private void Update()
        {
            //Try not to use update if possible
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

                    ColorManager.ChangeColor(_savedData.BodyColor.Vector4ToColor(), ColorTargetMode.Both);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void OnDestroy()
        {

        }

        #endregion

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            _savedData = Mod.GetHydroponicHarvesterSaveData(GetPrefabID());
        }

        public override void Initialize()
        {
            if (IsInitialized) return;

            //if (EffectsManager == null)
            //{
            //    EffectsManager = gameObject.AddComponent<EffectsManager>();
            //    EffectsManager.Initialize(IsUnderWater());
            //    //TODO Control effect based off power handler
            //    EffectsManager.ShowEffect();
            //}

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

            if(GrowBedManager == null)
            {
                GrowBedManager = gameObject.AddComponent<GrowBedManager>();
                GrowBedManager.Initialize(this);
            }

            if (ColorManager == null)
            {
                ColorManager = gameObject.AddComponent<ColorManager>();
                ColorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);
            }

#if DEBUG
            QuickLogger.Debug($"Initialized Ore Consumer {GetPrefabID()}");
#endif

            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.HydroponicHarvesterModTabID);

            IsInitialized = true;
        }

        public GrowBedManager GrowBedManager { get; set; }

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

        public bool CanBeStored(int amount, TechType techType)
        {
            return false;
        }

        public bool AddItemToContainer(InventoryItem item)
        {
            try
            {

            }
            catch (Exception e)
            {
                QuickLogger.DebugError($"Message: {e.Message} || StackTrace: {e.StackTrace}");
                return false;
            }

            return true;
        }
        
        public bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            return CanBeStored(0, pickupable.GetTechType());
        }

        public bool IsAllowedToRemoveItems()
        {
            return false;
        }

        public Pickupable RemoveItemFromContainer(TechType techType, int amount)
        {
            return null;
        }

        public Dictionary<TechType, int> GetItemsWithin()
        {
            return null;
        }

        public bool ContainsItem(TechType techType)
        {
            return false;
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
            newSaveData.HydroponicHarvesterEntries.Add(_savedData);
            QuickLogger.Debug($"Saving ID {_savedData.ID}", true);
        }
        
        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return ColorManager.ChangeColor(color, mode);
        }
    }
}
