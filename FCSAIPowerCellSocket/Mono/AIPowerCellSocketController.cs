using FCSAIPowerCellSocket.Model;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.IO;
using FCSAIPowerCellSocket.Buildables;
using FCSAIPowerCellSocket.Configuration;
using UnityEngine;

namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : IConstructable, IProtoEventListener
    {
        internal bool IsConstructed => _buildable != null && _buildable.constructed;
        internal AIPowerCellSocketPowerManager PowerManager { get; set; }
        internal AIPowerCellSocketDisplay Display { get; private set; }
        internal AIPowerCellSocketAnimator AnimationManager { get; set; }

        private PrefabIdentifier _prefabId;
        private Constructable _buildable;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _data;
        private bool _initialized;
        
        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!_initialized)
            {
                Initialize();
            }

            if (Display != null)
            {
                Display.Setup(this);
                _runStartUpOnEnable = false;
            }

            if (_data == null)
                ReadySaveData();

            if (_data != null)
            {
                PowerManager.LoadPowercellItems(_data.PowercellDatas);
            }
        }
        
        private void Initialize()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();

            if (_buildable == null)
            {
                _buildable = GetComponentInParent<Constructable>() ?? GetComponentInParent<Constructable>();
            }

            if(PowerManager == null)
                PowerManager = gameObject.GetComponent<AIPowerCellSocketPowerManager>();
            
            PowerManager.Initialize(this);

            if(AnimationManager == null)
                AnimationManager = gameObject.GetComponent<AIPowerCellSocketAnimator>();
    
            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager not found!");
            }

            if (Display == null)
                Display = gameObject.AddComponent<AIPowerCellSocketDisplay>();

            _initialized = true;
        }

        internal void UpdateSlots()
        {
            if (AnimationManager != null && PowerManager != null)
            {
                AnimationManager.SetBatteryState(PowerManager.GetPowercellCount());
            }
            else
            {
                QuickLogger.Error("Failed to update slots");
            }
        }

        internal void EmptySlot(int slot)
        {
            Display.EmptyBatteryVisual(slot);
        }

        private void OnDestroy()
        {

        }

        private void OpenStorage()
        {
            PowerManager.OpenSlots();
        }

        private void ReadySaveData()
        {
            _prefabId = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = _prefabId.Id ?? string.Empty;
            _data = Mod.GetSaveData(id);
        }

        internal void Save(SaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;
            var saveData = new SaveDataEntry();

            saveData.Id = id;
            saveData.PowercellDatas = PowerManager.GetSaveData();
            
            saveDataList.Entries.Add(saveData);
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = String.Empty;

            if (!_initialized || PowerManager == null) return true;


            if (PowerManager.GetPowercellCount() == 0) return true;
            reason = AIPowerCellSocketBuildable.RemoveAllPowercells();
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!_initialized)
                    {
                        Initialize();
                    }

                    if (Display != null)
                    {
                        Display.Setup(this);
                        _runStartUpOnEnable = false;
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        #region IPhotoTreeEventListener
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (Mod.IsSaving()) return;
            QuickLogger.Info("Saving PowerCell Socket");
            Mod.SaveMod();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_data == null)
            {
                ReadySaveData();
            }

            QuickLogger.Info($"Loading  {_prefabId.Id}");

            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion
    }
}
