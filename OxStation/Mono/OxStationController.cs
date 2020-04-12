using MAC.OxStation.Config;
using MAC.OxStation.Enums;
using MAC.OxStation.Managers;
using System.Collections;
using System.Collections.Generic;
using FCSCommon.Enums;
using FCSCommon.Utilities;
using MAC.OxStation.Buildables;
using UnityEngine;

namespace MAC.OxStation.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class OxStationController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        #region Private Members

        private static readonly Dictionary<Beacon, OxStationController> AllBeaconsAttached = new Dictionary<Beacon, OxStationController>();
        private SaveDataEntry _saveData;
        private bool _initialized;
        private Coroutine _powerStateCoroutine;
        private Coroutine _healthCheckCoroutine;
        private Coroutine _generateOxygenCoroutine;
        private bool _fromSave;
        private bool _runStartUpOnEnable;
        private string _beaconID;
        private Beacon _attachedBeacon;
        private int _isPinging;
        private PrefabIdentifier _prefabId;

        #endregion

        #region Internal Properties

        internal BaseManager Manager { get; private set; }
        internal SubRoot SubRoot { get; private set; }
        internal bool IsConstructed { get; private set; }
        internal OxOxygenManager OxygenManager { get; private set; }
        internal PowerManager PowerManager { get; private set; }
        internal HealthManager HealthManager { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal OxDisplayManager DisplayManager { get; private set; }
        internal AudioManager AudioManager;
        internal int IsRunningHash { get; set; }

        #endregion

        #region Unity Methods

        public void Start()
        {

        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            Setup();

            if (_fromSave)
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

                SetPinging(data.IsPinging);
                HealthManager.SetHealth(data.HealthLevel);
                OxygenManager.SetO2Level(data.OxygenLevel);
                _beaconID = data.BeaconID;
                _fromSave = false;

                ReattachBeaconAfterLoad();

                QuickLogger.Info($"Loaded {Mod.FriendlyName}");
            }
        }

        private void Update()
        {
            OxygenManager?.GenerateOxygen();
            HealthManager?.HealthChecks();
            HealthManager?.UpdateHealthSystem();
            PowerManager?.ConsumePower();
            PowerManager?.UpdatePowerState();
        }

        private void OnDestroy()
        {
            if (!_initialized) return;
            StopCoroutine(_powerStateCoroutine);
            StopCoroutine(_generateOxygenCoroutine);
            StopCoroutine(_healthCheckCoroutine);
            CancelInvoke(nameof(UpdateAudio));
            BaseManager.RemoveBaseUnit(this);
        }

        #endregion

        #region Private Methods

        private void Initialize()
        {
            QuickLogger.Debug("Initializing");

            _isPinging = Animator.StringToHash("IsPinging");
            IsRunningHash = Animator.StringToHash("IsRunning");

            AddToBaseManager();
            
            if (OxygenManager == null)
            {
                OxygenManager = new OxOxygenManager();
                OxygenManager.SetAmountPerSecond(QPatch.Configuration.OxygenPerSecond);
                OxygenManager.Initialize(this);
            }

            if (HealthManager == null)
            {
                HealthManager = new HealthManager();
                HealthManager.Initialize(this);
                HealthManager.SetHealth(100);
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.GetComponent<PowerManager>();
                PowerManager.Initialize(this);
                PowerManager.OnPowerUpdate += OnPowerUpdate;
            }

            if (AudioManager == null)
            {
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());
                InvokeRepeating(nameof(UpdateAudio), 0, 1);
            }
            
            if (AnimationManager == null)
            {
                AnimationManager = gameObject.GetComponent<AnimationManager>();
                AnimationManager.SetBoolHash(IsRunningHash, true);
            }
            
            if (DisplayManager == null)
            {
                DisplayManager = gameObject.AddComponent<OxDisplayManager>();
                DisplayManager.Setup(this);
            }

            QuickLogger.Debug("Initialized");
            _initialized = true;
        }
        
        private void OnPowerUpdate(FCSPowerStates obj)
        {
            AnimationManager.SetBoolHash(IsRunningHash, obj == FCSPowerStates.Powered);
        }
        private PrefabIdentifier GetPrefabID()
        {
            return GetComponentInParent<PrefabIdentifier>() ?? GetComponentInChildren<PrefabIdentifier>();
        }
        
        private void Setup()
        {

            var playerInterationManager = gameObject.GetComponent<PlayerInteractionManager>();

            if (playerInterationManager != null)
            {
                playerInterationManager.Initialize(this);
            }

            if (!_initialized)
            {
                Initialize();
            }
        }

        private void UpdateAudio()
        {
            if (!IsConstructed || PowerManager == null || AudioManager == null) return;

            if (IsConstructed && PowerManager.GetPowerState() != FCSPowerStates.Powered || !QPatch.Configuration.PlaySFX)
            {
                AudioManager.StopMachineAudio();
                return;
            }

            AudioManager.PlayMachineAudio();
        }
        
        private void SetPinging(bool state)
        {
            AnimationManager.SetBoolHash(_isPinging, state);
        }

        #endregion
        
        #region Public/Internal Methods

        internal void AddToBaseManager(BaseManager managers = null)
        {
            SubRoot = GetComponentInParent<SubRoot>() ?? GetComponent<SubRoot>();

            if (SubRoot == null) return;

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddBaseUnit(this);
        }

        internal void Save(SaveData newSaveData)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_saveData == null)
            {
                _saveData = new SaveDataEntry();
            }
            _saveData.ID = id;
            _saveData.OxygenLevel = OxygenManager.GetO2Level();
            _saveData.HealthLevel = HealthManager.GetHealth();
            _saveData.BeaconID = (((_attachedBeacon != null) ? _attachedBeacon.GetComponent<UniqueIdentifier>().Id : null) ?? "");
            _saveData.IsPinging = AnimationManager.GetBoolHash(_isPinging);
            newSaveData.Entries.Add(_saveData);
        }

        internal string GetPrefabIDString()
        {
            if (_prefabId == null)
            {
                _prefabId = GetPrefabID();
            }

            return _prefabId?.Id;
        }
        
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            if (_attachedBeacon == null) return true;

            reason = OxStationBuildable.BeaconAttached();
            return false;
        }

        public void OnConstructedChanged(bool constructed)
        {
            IsConstructed = constructed;

            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    Setup();
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
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
            _fromSave = true;
        }

        internal bool IsBeaconAttached()
        {
            return _attachedBeacon != null;
        }

        internal void TogglePinging()
        {
            AnimationManager.SetBoolHash(_isPinging, !AnimationManager.GetBoolHash(_isPinging));
        }
        
        internal bool GetPingState()
        {
            return AnimationManager.GetBoolHash(_isPinging);
        }

        #endregion

        #region Code help by zorgesho
        internal bool SetBeaconAttached(Beacon beacon, bool attaching)
        {
            //Beacon Attachment code from FloatingCargoControl NexusMod: 303 by zorgesho

            if (beacon == null || attaching == IsBeaconAttached()) return false;


            QuickLogger.Debug($"IsBeaconAttached {IsBeaconAttached()} || Attaching {attaching}", true);

            if (attaching)
            {
                beacon.transform.parent = gameObject.transform;
                beacon.transform.localPosition = new Vector3(1.113f, 0.859f, 0.35f);
                beacon.transform.localEulerAngles = new Vector3(0.04427741f, -90f, 0.1849947f);
            }

            beacon.GetComponent<WorldForces>().enabled = !attaching;
            beacon.GetComponent<Stabilizer>().enabled = !attaching;
            beacon.GetComponentInChildren<Animator>().enabled = !attaching;
            beacon.GetComponent<Rigidbody>().isKinematic = attaching;

            GameObject buildCheck = beacon.gameObject.FindChild("buildcheck");
            buildCheck.GetComponent<BoxCollider>().center = new Vector3(0f, 0f, attaching ? 0.1f : 0f);
            buildCheck.GetComponent<BoxCollider>().size = (attaching ? new Vector3(0.5f, 0.8f, 0.15f) : new Vector3(0.2f, 0.5f, 0.15f));
            buildCheck.layer = (attaching ? LayerID.Player : LayerID.Default);

            GameObject label = beacon.gameObject.FindChild("label");
            label.GetComponent<BoxCollider>().center = new Vector3(0f, 0.065f, attaching ? 0.18f : 0.08f);
            label.layer = (attaching ? LayerID.Player : LayerID.Default);

            if (attaching)
            {
                AllBeaconsAttached.Add(beacon, this);
                QuickLogger.Debug($"AllBeaconsAttached Count: {AllBeaconsAttached.Count}", true);
            }
            else
            {
                AllBeaconsAttached.Remove(beacon);
                QuickLogger.Debug($"AllBeaconsAttached Count: {AllBeaconsAttached.Count}", true);
            }
            _attachedBeacon = (attaching ? beacon : null);
            return true;
        }

        internal bool TryAttachBeacon(Beacon beacon)
        {
            //Beacon Attachment code from FloatingCargoControl NexusMod: 303 by zorgesho
            return beacon && !IsBeaconAttached() && (base.gameObject.transform.position - beacon.gameObject.transform.position).sqrMagnitude < 16f && SetBeaconAttached(beacon, true);
        }

        internal static void TryDetachBeacon(Beacon beacon)
        {
            //Beacon Attachment code from FloatingCargoControl NexusMod: 303 by zorgesho
            if (AllBeaconsAttached.TryGetValue(beacon, out var floatingCargoCrateControl))
            {
                QuickLogger.Debug($"Detaching Beacon", true);
                floatingCargoCrateControl.SetBeaconAttached(beacon, false);
            }
        }

        private void ReattachBeaconAfterLoad()
        {
            //Beacon Attachment code from FloatingCargoControl NexusMod: 303 by zorgesho
            UniqueIdentifier uniqueIdentifier;
            QuickLogger.Debug($"Trying to reattachBeacon after load");
            if (UniqueIdentifier.TryGetIdentifier(_beaconID, out uniqueIdentifier))
            {
                SetBeaconAttached(uniqueIdentifier.GetComponent<Beacon>(), true);
            }
        }
        #endregion
    }
}
