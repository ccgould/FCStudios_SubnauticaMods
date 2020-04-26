using System;
using System.Collections.Generic;
using FCSCommon.Abstract;
using FCSCommon.Controllers;
using FCSCommon.Utilities;
using FCSTechFabricator.Extensions;
using FCSTechFabricator.Managers;
using GasPodCollector.Buildables;
using GasPodCollector.Configuration;
using GasPodCollector.Models;
using GasPodCollector.Mono.Managers;
using UnityEngine;

namespace GasPodCollector.Mono
{
    internal class GaspodCollectorController : FCSController
    {
        private bool _runStartUpOnEnable;
        private bool _fromSave;
        private SaveDataEntry _savedData;
        private int _isExpanded;
        private bool _expand;
        private float _animationDelay = 1.5f;
        private float _floatingAnimationDelay;
        private float gravitySign = 1f;
        private int _page;
        private int _isFloating;
        private bool _float;
        private Rigidbody rigidbody;
        private string _beaconID;
        private Beacon _attachedBeacon;
        private float timeNextPhysicsChange;
        private static readonly Dictionary<Beacon, GaspodCollectorController> AllBeaconsAttached = new Dictionary<Beacon, GaspodCollectorController>();
        private bool lastPhysicsEnabled;


        public override bool IsConstructed { get; }
        public override bool IsInitialized { get; set; }
        internal GaspodManager GaspodManager { get; set; }
        internal GaspodCollectorStorage GaspodCollectorStorage { get; private set; }
        internal AnimationManager AnimationManager { get; private set; }
        internal ColorManager ColorManager { get; private set; }
        public GasopodCollectorDisplayManager DisplayManager { get; private set; }
        public GasopdCollectorPowerManager PowerManager { get; private set; }
        public SuctionFanManager SuctionFanManager { get; private set; }

        #region Unity Methods

        private void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.mass = QPatch.Configuration.Config.CollectorMass;
            rigidbody.isKinematic = true;

            try
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (_savedData == null)
                {
                    ReadySaveData();
                }
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
            }
            finally
            {
                _runStartUpOnEnable = false;
            }
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (_expand)
            {
                _animationDelay -= DayNightCycle.main.deltaTime;
                if (_animationDelay <= 0)
                {
                    AnimationManager.SetBoolHash(_isExpanded, true);
                    AnimationManager.SetIntHash(_page, 1);
                    _expand = false;
                }
            }
            if (gameObject.transform.position.y > -4500f)
            {
                updateDistanceFromCam();
            }

            if (!rigidbody.isKinematic && Time.time > this.timeNextPhysicsChange)
            { 
                timeNextPhysicsChange = Time.time + UnityEngine.Random.Range(10f, 20f);
                updateGravityChange();
                UpdateMass();
            }

        }

        #endregion

        public override void Initialize()
        {

            _isExpanded = Animator.StringToHash("IsExpanded");
            _page = Animator.StringToHash("Page");
            _isFloating = Animator.StringToHash("IsFloating");

            if (GaspodManager == null)
            {
                GaspodManager = gameObject.GetComponent<GaspodManager>();
                GaspodManager.Initialize(this);
            }

            if (GaspodCollectorStorage == null)
            {
                GaspodCollectorStorage = gameObject.EnsureComponent<GaspodCollectorStorage>();
                GaspodCollectorStorage.OnGaspodCollected += OnGaspodCollected; 
            }

            if (AnimationManager == null)
            {
                AnimationManager = gameObject.GetComponent<AnimationManager>();
                _floatingAnimationDelay = AnimationManager.AnimationLength("GaspodCollector_Animation");
                QuickLogger.Debug($"Animation Time = {_floatingAnimationDelay}",true);
                _expand = true;
                _float = true;
            }

            if (ColorManager == null)
                ColorManager = new ColorManager();

            ColorManager.Initialize(gameObject, GaspodCollectorBuildable.BodyMaterial);

            if (DisplayManager == null)
            {
                DisplayManager = gameObject.EnsureComponent<GasopodCollectorDisplayManager>();
                DisplayManager.Setup(this);
            }

            if (PowerManager == null)
            {
                PowerManager = gameObject.EnsureComponent<GasopdCollectorPowerManager>();
                PowerManager.Setup(this);
            }

            if (SuctionFanManager == null)
            {
                SuctionFanManager = gameObject.EnsureComponent<SuctionFanManager>();
                SuctionFanManager.Initialize(this);
            }

            IsInitialized = true;
        }
        
        private void OnGaspodCollected()
        {
            PowerManager.TakePower();
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoSerialize");
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.FriendlyName}");
                Mod.Save();
                QuickLogger.Info($"Saved {Mod.FriendlyName}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            if (_savedData == null)
            {
                ReadySaveData();
                _fromSave = true;
            }

            if (!IsInitialized)
            {
                Initialize();
            }

            GaspodCollectorStorage.SetStorageAmount(_savedData.GaspodAmount);
            PowerManager.LoadSaveData(_savedData.Batteries);
            DisplayManager.OnStorageAmountChange(_savedData.GaspodAmount);
            ReattachBeaconAfterLoad();
            ColorManager.SetColorFromSave(_savedData.BodyColor.Vector4ToColor());
            QuickLogger.Info($"Loaded {Mod.FriendlyName}");
        }

        public override bool CanDeconstruct(out string reason)
        {
            if (IsBeaconAttached())
            {
                reason = GaspodCollectorBuildable.RemoveBeacon();
                return false;
            }

            if (GaspodCollectorStorage != null && GaspodCollectorStorage.GetStorageAmount() > 0)
            {
                reason = GaspodCollectorBuildable.NotEmpty();
                return false;
            }

            if (PowerManager != null && PowerManager.HasPower())
            {
                reason = GaspodCollectorBuildable.HasBatteries();
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public override void OnConstructedChanged(bool constructed)
        {
            if (constructed)
            {
                if (isActiveAndEnabled)
                {
                    if (!IsInitialized)
                    {
                        Initialize();
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
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
            _savedData.GaspodAmount = GaspodCollectorStorage.GetStorageAmount();
            _savedData.BodyColor = ColorManager.GetColor().ColorToVector4();
            _savedData.Batteries = PowerManager.GetBatteries();
            saveData.Entries.Add(_savedData);
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetSaveData(id);
        }

        internal void ChangePage(int pageNumber)
        {
            AnimationManager.SetIntHash(_page,pageNumber);
        }

        internal void UpdateBatteryDisplay(Dictionary<int, BatteryInfo> batteries)
        {
            DisplayManager.UpdateBatteries(batteries);
        }

        internal bool IsBeaconAttached()
        {
            return _attachedBeacon != null;
        }

        #region Code help by zorgesho
        internal bool SetBeaconAttached(Beacon beacon, bool attaching)
        {
            //Beacon Attachment code from FloatingCargoControl NexusMod: 303 by zorgesho

            if (beacon == null || attaching == IsBeaconAttached()) return false;


            QuickLogger.Debug($"IsBeaconAttached {IsBeaconAttached()} || Attaching {attaching}", true);

            if (attaching)
            {
                beacon.transform.parent = gameObject.transform;
                beacon.transform.localPosition = new Vector3(0.694f, 0f, 0f);
                beacon.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
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

        private void updateGravityChange()
        {
            gravitySign = -gravitySign;
            var wf = gameObject.GetComponent<WorldForces>();

            if (wf != null)
            {
                wf.underwaterGravity = gravitySign * 0.1f * UnityEngine.Random.value;
            }
        }

        private void setRigidBodyPhysicsEnabled(bool val)
        {
            if (val != this.lastPhysicsEnabled)
            {
                this.lastPhysicsEnabled = val;
                this.rigidbody.isKinematic = !val;
            }
        }

        private void updateDistanceFromCam()
        {
            LargeWorldStreamer main = LargeWorldStreamer.main;
            if (main == null)
            {
                return;
            }
            float sqrMagnitude = (base.gameObject.transform.position - main.cachedCameraPosition).sqrMagnitude;
            this.setRigidBodyPhysicsEnabled(sqrMagnitude < 1600f);
        }

        private void UpdateMass()
        {
            float massEmpty = QPatch.Configuration.Config.CollectorEmptyMass;
            float massFull = QPatch.Configuration.Config.CollectorFullMass;

            if (GaspodCollectorStorage != null)
                this.rigidbody.mass =
                    (massFull - massEmpty) * (GaspodCollectorStorage.GetStorageAmount() /
                                              (float)QPatch.Configuration.Config.StorageLimit) + massEmpty;
        }
        #endregion
    }
}