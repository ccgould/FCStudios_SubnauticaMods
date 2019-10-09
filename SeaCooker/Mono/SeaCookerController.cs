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
        internal Dictionary<string, ARSolutionsSeaBreezeController> SeaBreezes { get; set; } = new Dictionary<string, ARSolutionsSeaBreezeController>();
        private SaveDataEntry _saveData;
        private bool _initialized;
        private int _isRunning;
        private GameObject _habitat;
        private void Awake()
        {
            _isRunning = Animator.StringToHash("IsRunning");
        }
        private void Update()
        {
            PowerManager?.ConsumePower();
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;

            if (StorageManager.CanDeconstruct()) return true;
            reason = SeaCookerBuildable.UnityNotEmpty();
            return false;

        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (!constructed) return;

            if (!_initialized)
            {
                Initialize();
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
                DisplayManager.Setup(this);
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

            //FindHabitat();

            _initialized = true;
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

        public ColorManager ColorManager { get; private set; }
        public AudioManager AudioManager { get; private set; }

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
            QuickLogger.Info($"Loading {Mod.FriendlyName}");
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            var data = Mod.GetSaveData(id);

            if (data == null)
            {
                QuickLogger.Info($"No save found for PrefabId {id}");
                return;
            }

            QuickLogger.Debug("Loading Current Fuel from save");

            GasManager.SetEquipment(data.TankType);
            GasManager.SetTankLevel(data.FuelLevel);
            ColorManager.SetCurrentBodyColor(data.BodyColor.Vector4ToColor());
            StorageManager.LoadExportContainer(data.Export);
            StorageManager.LoadInputContainer(data.Input);
            StorageManager.SetExportToSeabreeze(data.ExportToSeaBreeze);
            FoodManager.LoadRunningState(data);
            QuickLogger.Info($"Loaded {Mod.FriendlyName}");
        }

        internal void Save(SaveData saveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new SaveDataEntry();
            }
            _saveData.ID = id;
            _saveData.FuelLevel = GasManager.GetTankLevel();
            _saveData.TankType = GasManager.CurrentFuel;
            _saveData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _saveData.Export = StorageManager.GetExportContainer();
            _saveData.Input = StorageManager.GetInputContainer();
            _saveData.ExportToSeaBreeze = StorageManager.GetExportToSeabreeze();
            FoodManager.SaveRunningState(_saveData);

            saveData.Entries.Add(_saveData);
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
                StartCoroutine(TrackNewSeabreezeCoroutine(obj));
            }
        }

        private void AlertedSeaBreezeDestroyed(ARSolutionsSeaBreezeController obj)
        {
            if (obj != null)
            {
                QuickLogger.Debug("OBJ Not NULL", true);
                SeaBreezes.Remove(obj.GetPrefabID());
                QuickLogger.Debug("Removed Seabreeze");
            }
        }

        private IEnumerator TrackNewSeabreezeCoroutine(ARSolutionsSeaBreezeController obj)
        {
            yield return new WaitForEndOfFrame();

            GameObject newSeaBase = obj?.gameObject?.transform?.parent?.gameObject;

            QuickLogger.Debug($"SeaBase Base Found in Track {newSeaBase?.name}");
            QuickLogger.Debug($"Cooker Base Found in Track {_habitat?.name}");

            if (newSeaBase != null && newSeaBase == _habitat)
            {
                SeaBreezes.Add(obj.GetPrefabID(), obj);
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

    }
}
