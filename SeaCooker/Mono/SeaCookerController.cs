using System;
using AE.SeaCooker.Buildable;
using AE.SeaCooker.Configuration;
using AE.SeaCooker.Helpers;
using AE.SeaCooker.Managers;
using AE.SeaCooker.Patchers;
using ARS_SeaBreezeFCS32.Mono;
using FCSCommon.Extensions;
using FCSCommon.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AE.SeaCooker.Mono
{
    internal class SeaCookerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        internal SCDisplayManager DisplayManager { get; set; }
        internal bool IsConstructed { get; private set; }
        internal GasManager GasManager { get; private set; }
        internal SCStorageManager StorageManager { get; private set; }
        internal PowerManager PowerManager { get; private set; }
        internal AnimationManager AnimationManager { get; set; }
        internal FoodManager FoodManager { get; private set; }
        internal string SelectedSeaBreezeID { get; set; }
        internal Dictionary<string, ARSolutionsSeaBreezeController> SeaBreezes { get; set; } = new Dictionary<string, ARSolutionsSeaBreezeController>();
        internal ColorManager ColorManager { get; private set; }
        internal AudioManager AudioManager { get; private set; }
        internal bool IsSebreezeSelected { get; set; }
        internal bool AutoChooseSeabreeze { get; set; } = true;

        public bool IsInitialized { get; set; }
        private int _isRunning;
        private GameObject _habitat;
        private bool _runStartUpOnEnable;
        private SaveDataEntry _savedData;
        private bool _fromSave;
        internal ARSolutionsSeaBreezeController SelectedSeaBreeze { get; set; }
        internal PlayerInteraction PlayerInteraction { get; private set; }

        private void Awake()
        {
            _isRunning = Animator.StringToHash("IsRunning");
        }
        
        private void Update()
        {
            PowerManager?.ConsumePower();
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (DisplayManager != null)
                {
                    DisplayManager.Setup(this);
                    _runStartUpOnEnable = false;
                }
                
                if (_fromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    AutoChooseSeabreeze = _savedData.AutoChooseSeabreeze;
                    GasManager.SetEquipment(_savedData.TankType);
                    GasManager.SetTankLevel(_savedData.FuelLevel);
                    ColorManager.SetCurrentBodyColor(_savedData.BodyColor.Vector4ToColor());
                    StorageManager.LoadExportContainer(_savedData.Export);
                    StorageManager.LoadInputContainer(_savedData.Input);
                    StorageManager.SetExportToSeabreeze(_savedData.ExportToSeaBreeze);
                    FoodManager.LoadRunningState(_savedData);
                    SelectedSeaBreezeID = string.IsNullOrEmpty(_savedData.CurrentSeaBreezeID) ? string.Empty : _savedData.CurrentSeaBreezeID;
                    DisplayManager.UpdateSeaBreezes();
                    QuickLogger.Info($"Loaded {Mod.FriendlyName}");
                }
                
                _runStartUpOnEnable = false;
            }
        }
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (StorageManager == null)
            {
                return true;
            }

            if (!StorageManager.CanDeconstruct())
            {
                reason = SeaCookerBuildable.UnitNotEmpty();
                return false;
            };

            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (!constructed) return;

            if (isActiveAndEnabled)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (DisplayManager != null)
                {
                    DisplayManager.Setup(this);
                    _runStartUpOnEnable = false;
                }
            }
            else
            {
                _runStartUpOnEnable = true;
            }

            _habitat = gameObject?.transform?.parent?.gameObject;
            GetSeaBreezes();
            //AnimationManager.SetBoolHash(IsRunningHash, true);
        }
        
        private void Initialize()
        {
            TechTypeHelpers.Initialize();
            ARSeaBreezeFCS32Awake_Patcher.AddEventHandlerIfMissing(AlertedNewSeaBreezePlaced);
            ARSeaBreezeFCS32Destroy_Patcher.AddEventHandlerIfMissing(AlertedSeaBreezeDestroyed);
            if (FoodManager == null)
            {
                FoodManager = gameObject.AddComponent<FoodManager>();
                FoodManager.Initialize(this);
            }

            if (StorageManager == null)
            {
                StorageManager = new SCStorageManager();
                StorageManager.Initialize(this);
            }

            if (GasManager == null)
            {
                GasManager = new GasManager();
                GasManager.Initialize(this);
            }

            if (PowerManager == null)
            {
                PowerManager = new PowerManager();
                PowerManager.Initialize(this);
                StartCoroutine(UpdatePowerState());
            }

            AnimationManager = gameObject.GetComponent<AnimationManager>();

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<SCDisplayManager>();
            }

            if (ColorManager == null)
            {
                ColorManager = new ColorManager();
                ColorManager.Initialize(this, SeaCookerBuildable.BodyMaterial);

            }

            if (AudioManager == null)
            {
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
                //InvokeRepeating(nameof(UpdateAudio), 0, 1);
            }

            if (PlayerInteraction == null)
            {
                PlayerInteraction = gameObject.GetComponent<PlayerInteraction>();

            }

            PlayerInteraction.Initialize(this);

            //FindHabitat();

            IsInitialized = true;
        }

        private void FindHabitat()
        {
            //if (_habitat != null) return;

            //_habitat = PowerManager.CurrentRelay()?.transform;

            //if (_habitat != null)
            //{
            //    QuickLogger.Debug($"Base Name = {_habitat.name}");
            //}
        }


        internal void UpdateIsRunning(bool value = true)
        {
            AnimationManager.SetBoolHash(_isRunning, value);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_savedData == null)
            {
                ReadySaveData();
            }

            _fromSave = true;
        }

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_savedData == null)
            {
                _savedData = new SaveDataEntry();
            }
            _savedData.ID = id;
            _savedData.FuelLevel = GasManager.GetTankLevel();
            _savedData.TankType = GasManager.CurrentFuel;
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.Export = StorageManager.GetExportContainer();
            _savedData.Input = StorageManager.GetInputContainer();
            _savedData.ExportToSeaBreeze = StorageManager.GetExportToSeabreeze();
            _savedData.CurrentSeaBreezeID = SelectedSeaBreezeID;
            _savedData.AutoChooseSeabreeze = AutoChooseSeabreeze;
            FoodManager.SaveRunningState(_savedData);


            saveData.Entries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        private IEnumerator UpdatePowerState()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                PowerManager.UpdatePowerState();
            }
        }

        private void AlertedNewSeaBreezePlaced(ARSolutionsSeaBreezeController obj)
        {
            if (obj != null)
            {
                QuickLogger.Debug("OBJ Not NULL", true);
                StartCoroutine(TrackNewSeabreezeCoroutine(obj));
            }
        }

        private void AlertedSeaBreezeDestroyed(ARSolutionsSeaBreezeController obj)
        {
            if (obj == null || obj.PrefabId == null) return;

            QuickLogger.Debug("OBJ Not NULL", true);
            SeaBreezes.Remove(obj.GetPrefabID());
            QuickLogger.Debug("Removed Seabreeze");
            DisplayManager.UpdateSeaBreezes();
        }

        private IEnumerator TrackNewSeabreezeCoroutine(ARSolutionsSeaBreezeController obj)
        {
            yield return new WaitForEndOfFrame();

            GameObject newSeaBase = obj?.gameObject?.transform?.parent?.gameObject;

            QuickLogger.Debug($"SeaBase Base Found in Track {newSeaBase?.name}");
            QuickLogger.Debug($"Cooker Base Found in Track {_habitat?.name}");

            if (newSeaBase != null && newSeaBase == _habitat)
            {
                QuickLogger.Debug("Adding Seabreeze");
                SeaBreezes.Add(obj.GetPrefabID(), obj);
                DisplayManager.UpdateSeaBreezes();
                QuickLogger.Debug("Added Seabreeze");
            }
        }

        private void OnDestroy()
        {
            ARSeaBreezeFCS32Awake_Patcher.RemoveEventHandler(AlertedNewSeaBreezePlaced);
            ARSeaBreezeFCS32Destroy_Patcher.RemoveEventHandler(AlertedSeaBreezeDestroyed);
        }

        /// <summary>
        /// Gets all the turbines in the base
        /// </summary>
        private void GetSeaBreezes()
        {
            //Clear the list
            SeaBreezes.Clear();

            //Check if there is a base connected
            if (_habitat != null)
            {
                var seaBreezeList = _habitat.GetComponentsInChildren<ARSolutionsSeaBreezeController>().ToList();

                foreach (var seaBreeze in seaBreezeList)
                {
                    SeaBreezes.Add(seaBreeze.GetPrefabID(), seaBreeze);
                }
            }
        }

        public void SetCurrentSeaBreeze(ARSolutionsSeaBreezeController sb)
        {
            SelectedSeaBreeze = sb;
            SelectedSeaBreezeID = sb.GetPrefabID();
        }
    }
}
