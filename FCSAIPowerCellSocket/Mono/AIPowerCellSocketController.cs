using FCSAIPowerCellSocket.Model;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using System;
using System.IO;
using FCSAIPowerCellSocket.Configuration;
using UnityEngine;

namespace FCSAIPowerCellSocket.Mono
{
    internal partial class AIPowerCellSocketController : IConstructable, IProtoEventListener
    {
        public bool IsConstructed => _buildable != null && _buildable.constructed;
        public AIPowerCellSocketPowerManager PowerManager { get; set; }
        public AIPowerCellSocketDisplay Display { get; private set; }
        public AIPowerCellSocketAnimator AnimationManager { get; set; }

        private PrefabIdentifier _prefabId;

        //private readonly string _saveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "AIPowerCellSocket");

        private Constructable _buildable;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _data;
        //private SaveDataEntry _oldSavedData;
        private bool _initialized;

       // private string SaveFile => Path.Combine(_saveDirectory, _prefabId?.Id + ".json");

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

            //if (_oldSavedData == null)
            //{
            //    PowerManager.LoadPowercellItems(_data.PowercellDatas);
            //}
            //else
            //{
            //    PowerManager.LoadPowercellItems(_oldSavedData.PowercellDatas);
            //    QuickLogger.Debug("Load Items");
            //    UpdateSlots();
            //    File.Delete(SaveFile);
            //}
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
                AnimationManager.SetBatteryState(PowerManager.PowercellTracker.Count);
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

            if (PowerManager.PowercellTracker.Count == 0) return true;
            reason = "Please remove all powercells";
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

            //if (File.Exists(SaveFile)) //Load old Save Data and delete
            //{
            //    QuickLogger.Debug("Loading old save.");
            //    string savedDataJson = File.ReadAllText(SaveFile).Trim();

            //    //LoadData
            //    QuickLogger.Debug("Loading Data");
            //    _oldSavedData = JsonConvert.DeserializeObject<SaveDataEntry>(savedDataJson);
            //    QuickLogger.Debug("Loaded Data");
            //}

            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
        #endregion
    }
}
