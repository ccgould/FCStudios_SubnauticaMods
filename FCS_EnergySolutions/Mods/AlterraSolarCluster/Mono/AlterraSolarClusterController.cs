using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Objects;
using FCS_AlterraHub.Registration;
using FCS_EnergySolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_EnergySolutions.Mods.AlterraSolarCluster.Mono
{
    internal class AlterraSolarClusterController : FcsDevice,IFCSSave<SaveData>
    {
        private AlterraSolarClusterDataEntry _savedData;
        internal bool IsFromSave { get; private set; }
        private bool _runStartUpOnEnable;
        private AlterraSolarClusterPowerManager _powerManager;
        private AlterraSolarClusterMovementManager _movementManager;
        private float nextChargeAttemptTimer;
        public override bool IsOperational => Manager != null && IsConstructed;
        public override bool IsVisible { get; } = true;

        #region Unity Methods

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.AlterraSolarClusterModTabID, Mod.ModPackID);
            _powerManager.CheckIfConnected();
        }

        private void OnGlobalEntitiesLoaded()
        {
            QuickLogger.Debug($"On Global Entites Loaded: {GetPrefabID()}");
        }

        private void OnEnable()
        {
            if (_runStartUpOnEnable)
            {
                if (!IsInitialized)
                {
                    Initialize();
                }

                if (IsFromSave)
                {
                    if (_savedData == null)
                    {
                        ReadySaveData();
                    }

                    if (!string.IsNullOrEmpty(_savedData.BaseId))
                    {
                        BaseId = _savedData.BaseId;
                    }
                    _colorManager.ChangeColor(_savedData.Body.Vector4ToColor());
                    _powerManager.SetPower(_savedData.StoredPower);
                }

                _runStartUpOnEnable = false;
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (Manager == null && !string.IsNullOrWhiteSpace(BaseId))
            {

                if (nextChargeAttemptTimer > 0f)
                {
                    this.nextChargeAttemptTimer -= DayNightCycle.main.deltaTime;
                    if (this.nextChargeAttemptTimer < 0f)
                    {
                        var manager = BaseManager.FindManager(BaseId);
                        if (manager != null)
                        {
                            Manager = manager;
                        }
                        else
                        {
                            this.nextChargeAttemptTimer = 5f;
                        }
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        public override void Initialize()
        {

            if (_movementManager == null)
            {
                _movementManager = gameObject.EnsureComponent<AlterraSolarClusterMovementManager>();
                _movementManager.Initialize(this);
            }

            if (_powerManager == null)
            {
                _powerManager = gameObject.EnsureComponent<AlterraSolarClusterPowerManager>();
                _powerManager.Initialize(this);
            }

            if (_colorManager == null)
            {
                _colorManager = gameObject.AddComponent<ColorManager>();
                _colorManager.Initialize(gameObject, AlterraHub.BasePrimaryCol);
            }
            MaterialHelpers.ChangeEmissionColor(AlterraHub.BaseDecalsEmissiveController, gameObject,Color.cyan);
            MaterialHelpers.ChangeSpecSettings(AlterraHub.BaseDecalsExterior,AlterraHub.TBaseSpec,gameObject,2.61f,8f);
            IsInitialized = true;
        }
        
        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
#if DEBUG
            QuickLogger.Debug($"Changing Alterra Solar Cluster color to {ColorList.GetName(color)}", true);
#endif

            return _colorManager.ChangeColor(color, mode);
        }

        #endregion

        #region IConstructable

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

        public override bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        #endregion

        #region IProtoEventListener


        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _savedData = Mod.GetAlterraSolarClusterSaveData(id);
        }

        public void Save(SaveData newSaveData, ProtobufSerializer serializer)
        {
            if (!IsInitialized
                || !IsConstructed) return;

            if (_savedData == null)
            {
                _savedData = new AlterraSolarClusterDataEntry();
            }

            _savedData.Id = GetPrefabID();

            QuickLogger.Debug($"Saving ID {_savedData.Id}", true);
            _savedData.Body = _colorManager.GetColor().ColorToVector4();
            _savedData.BaseId = BaseId;
            _savedData.StoredPower = _powerManager.GetStoredPower();
            newSaveData.AlterraSolarClusterEntries.Add(_savedData);
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("In OnProtoDeserialize");

            if (_savedData == null)
            {
                ReadySaveData();
            }

            IsFromSave = true;
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

        #endregion
    }
}
