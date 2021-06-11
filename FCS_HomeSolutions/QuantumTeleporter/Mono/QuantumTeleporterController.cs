using FCS_AlterraHomeSolutions.Mono.PaintTool;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Helpers;
using FCS_AlterraHub.Managers;
using FCS_AlterraHub.Mono;
using FCS_AlterraHub.Mono.Controllers;
using FCS_AlterraHub.Registration;
using FCS_HomeSolutions.Buildables;
using FCS_HomeSolutions.Configuration;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using UnityEngine;

namespace FCS_HomeSolutions.QuantumTeleporter.Mono
{
    internal class QuantumTeleporterController: FcsDevice, IFCSSave<SaveData>
    {
        private bool _runStartUpOnEnable;
        private QuantumTeleporterDataEntry _data;
        private bool _fromSave;
        private Constructable _buildable;
        private Transform _target;
        private string _linkedPortal;
        private bool _displayRefreshed;
        private GameObject _portal;
        private bool _notifyCreation;
        private ParticleSystem _portalFx;
        private ParticleSystem.TrailModule _trails;
        internal bool IsGlobal { get; set; }
        public override bool IsConstructed => _buildable != null && _buildable.constructed;
        public override bool IsInitialized { get; set; }
        internal NameController NameController { get; set; }
        internal QTDisplayManager DisplayManager { get; private set; }
        internal AudioManager AudioManager { get; private set; }
        internal QTPowerManager PowerManager { get; private set; }
        internal SubRoot SubRoot { get; set; }
        internal bool IsLinked { get; set; }

        private void Start()
        {
            FCSAlterraHubService.PublicAPI.RegisterDevice(this, Mod.QuantumTeleporterTabID, Mod.ModPackID);
            NameController.SetCurrentName(string.IsNullOrWhiteSpace(_data?.UnitName) ? GetNewName() : _data.UnitName, DisplayManager.GetNameTextBox());
            
            if (Manager != null)
            {
                Manager.OnPowerStateChanged += status => { TeleporterState(status != PowerSystem.Status.Offline); };
                
                if (_notifyCreation)
                {
                    QuickLogger.Debug("Notifying Creation on start",true);
                    Manager?.NotifyByID(Mod.QuantumTeleporterTabID, "RefreshDisplay");
                    _notifyCreation = false;
                }
            }
        }

        internal void ChangeTrailColor()
        {
            var index = QPatch.Configuration.QuantumTeleporterPortalTrailBrightness;
            _trails.colorOverLifetime =  new Color(index, index, index);
        }

        public override Vector3 GetPosition()
        {
            return transform.position;
        }

        private void TeleporterState(bool isOn)
        {
            if (isOn)
            {
                _portal.SetActive(true);
                DisplayManager.ChangeScreenVisiblity(true);
            }
            else
            {
                _portal.SetActive(false);
                DisplayManager.ChangeScreenVisiblity(false);
            }
        }

        private void Update()
        {
            if (!_displayRefreshed && IsInitialized && LargeWorldStreamer.main.IsWorldSettled())
            {
                DisplayManager.RefreshTabs();
                _displayRefreshed = true;
            }
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_data == null)
            {
                ReadySaveData();
            }

            if (_fromSave)
            {
                NameController.SetCurrentName(_data.UnitName);
                IsGlobal = _data.IsGlobal;
                _colorManager.ChangeColor(_data.Fcs.Vector4ToColor());
                DisplayManager.Load(_data);
                _linkedPortal = _data.LinkedPortal;
                IsLinked = _data.IsLinked;
                _fromSave = false;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Manager?.NotifyByID(Mod.QuantumTeleporterTabID, "RefreshDisplay");
        }

        public override void Initialize()
        {
            _target = gameObject.FindChild("Target").transform;

            _portal = GameObjectHelpers.FindGameObject(gameObject, "Rings");
            _portalFx = _portal.GetComponentInChildren<ParticleSystem>();
            _trails = _portalFx.trails;
            ChangeTrailColor();

            if (_target == null)
            {
                QuickLogger.Error("Cant find trigger targetPos");
                return;
            }

            if (_buildable == null)
                _buildable = GetComponentInParent<Constructable>() ?? GetComponent<Constructable>();

            if (_colorManager == null)
                _colorManager = gameObject.AddComponent<ColorManager>();

            _colorManager.Initialize(gameObject, ModelPrefab.BodyMaterial);

            if (NameController == null)
                NameController = gameObject.EnsureComponent<NameController>();
            
            if (DisplayManager == null)
                DisplayManager = gameObject.AddComponent<QTDisplayManager>();

            if (AudioManager == null)
                AudioManager = new AudioManager(gameObject.GetComponent<FMOD_CustomLoopingEmitter>());

            AudioManager.LoadFModAssets("/env/use_teleporter_use_loop", "use_teleporter_use_loop");

            if (PowerManager == null)
            {
                PowerManager = new QTPowerManager(this);
            }

            IPCMessage += message =>
            {
                if (message.Equals("RefreshDisplay"))
                {
                    DisplayManager?.RefreshTabs();
                }

                if (message.Equals("UpdateTeleporterEffects"))
                {
                    ChangeTrailColor();
                }
            };

            DisplayManager.Setup(this);

            NameController.Initialize(AuxPatchers.Submit(), Mod.QuantumTeleporterFriendly);
            NameController.OnLabelChanged += OnLabelChanged;
            
            IsInitialized = true;
        }

        private void ReadySaveData()
        {
            QuickLogger.Debug("In OnProtoDeserialize");
            var prefabIdentifier = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;
            _data = Mod.GetQuantumTeleporterSaveData(id);
        }

        public void Save(SaveData saveData, ProtobufSerializer serializer)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (_data == null)
            {
                _data = new QuantumTeleporterDataEntry();
            }

            _data.Id = id;
            _data.Fcs = _colorManager.GetColor().ColorToVector4();
            _data.UnitName = NameController.GetCurrentName();
            _data.IsGlobal = IsGlobal;
            _data.SelectedTab = DisplayManager.GetSelectedTab();
            _data.IsLinked = IsLinked;
            _data.LinkedPortal = _linkedPortal;
            saveData.QuantumTeleporterEntries.Add(_data);
        }

        public override void OnProtoSerialize(ProtobufSerializer serializer)
        {
            if (!Mod.IsSaving())
            {
                QuickLogger.Info($"Saving {Mod.QuantumTeleporterFriendly}");
                Mod.Save(serializer);
                QuickLogger.Info($"Saved {Mod.QuantumTeleporterFriendly}");
            }
        }

        public override void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }

        public override bool CanDeconstruct(out string reason)
        {
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

                    _notifyCreation = true;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }
        
        private void OnLabelChanged(string obj, NameController nameController)
        {
            DisplayManager?.RefreshBaseName(GetName());
            Manager?.NotifyByID(Mod.QuantumTeleporterTabID, "RefreshDisplay");
        }
        
        public string GetName()
        {
            return NameController.GetCurrentName();
        }

        private string GetNewName()
        {
            return UnitID;
        }

        internal bool GetIsGlobal()
        {
            return IsGlobal;
        }

        internal Transform GetTarget()
        {
            return _target;
        }

        public void ToggleIsGlobal()
        {
            IsGlobal = !IsGlobal;
            BaseManager.GlobalNotifyByID(Mod.QuantumTeleporterTabID, "RefreshDisplay");
        }

        public override bool ChangeBodyColor(Color color, ColorTargetMode mode)
        {
            return _colorManager.ChangeColor(color, mode);
        }
    }
}