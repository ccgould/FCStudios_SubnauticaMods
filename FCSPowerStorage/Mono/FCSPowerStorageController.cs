using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Managers;
using Oculus.Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace FCSPowerStorage.Mono
{
    /// <summary>
    /// The controller for the custom battery
    /// </summary>
    internal class FCSPowerStorageController : MonoBehaviour, IProtoTreeEventListener, IConstructable
    {
        #region Private Members
        private PrefabIdentifier _prefabId;
        private readonly string _saveDirectory = Information.GetSaveFileDirectory();
        private string SaveFile => Path.Combine(_saveDirectory, _prefabId.Id + ".json");
        private bool _initialized;
        private Color _currentBodyColor;
        #endregion

        #region Internal Properties
        internal bool IsBeingDeleted { get; set; }
        internal FCSPowerManager PowerManager { get; private set; }
        internal FCSPowerStorageAnimationManager AnimationManager { get; private set; }
        internal SystemLightManager SystemLightManager { get; private set; }
        internal FCSPowerStorageDisplay Display { get; private set; }
        internal int StateHash { get; private set; }
        internal int ToggleHash { get; private set; }
        internal int AutoActiveHash { get; set; }
        internal int BaseDrainHash { get; set; }
        public SubRoot SubRoot { get; private set; }
        public BaseManager Manager { get; private set; }
        internal bool IsConstructed => _buildable != null && _buildable.constructed;
        internal readonly int BatteryCount = 6;
        #endregion

        #region Private Members
        private bool _isEnabled;
        private Constructable _buildable;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            ToggleHash = Animator.StringToHash("ToggleState");
            StateHash = Animator.StringToHash("state");
            AutoActiveHash = Animator.StringToHash("AutoActivateState");
            BaseDrainHash = Animator.StringToHash("BaseDrain");
        }

        private void OnDestroy()
        {
            BaseManager.RemovePowerStorage(this);
            BaseManager.RemoveBasePowerStorage(this);
        }

        #endregion

        #region Inherited Public Methods

        public bool CanDeconstruct(out string reason)
        {
            reason = "";
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug($"Constructed - {constructed}");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (!_initialized)
                {
                    Initialize();
                    AddToBaseManager();
                }
            }
        }

        internal string GetPrefabID()
        {
            return _prefabId.Id;
        }

        private void Initialize()
        {
            SetCurrentBodyColor(new Color(0.99609375f, 0.99609375f, 0.99609375f));
            _prefabId = GetComponentInParent<PrefabIdentifier>();
            _buildable = GetComponentInParent<Constructable>();

            SystemLightManager = gameObject.GetOrAddComponent<SystemLightManager>();
            SystemLightManager.Initialize(gameObject);

            PowerManager = gameObject.GetOrAddComponent<FCSPowerManager>();

            if (PowerManager == null)
            {
                QuickLogger.Error("Power Manager was not found.");
            }
            else
            {
                PowerManager.Initialize(this);
            }

            AnimationManager = gameObject.GetOrAddComponent<FCSPowerStorageAnimationManager>();

            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager was not found.");
            }
            else
            {
                AnimationManager.Initialize(this);
                AnimationManager.SetBoolHash(BaseDrainHash, LoadData.BatteryConfiguration.BaseDrainProtection);
            }

            Display = gameObject.AddComponent<FCSPowerStorageDisplay>();
            Display.Setup(this);
        }

        #endregion

        #region Internal Methods
        internal void SetCurrentBodyColor(Color color)
        {
            _currentBodyColor = color;
            MaterialHelpers.ChangeBodyColor("Power_Storage_StorageBaseColor_Albedo", color, gameObject);
        }

        internal void ActivateBaseDrain()
        {
            AnimationManager.SetBoolHash(BaseDrainHash, LoadData.BatteryConfiguration.BaseDrainProtection);
            LoadData.BatteryConfiguration.SaveConfiguration();
        }
        #endregion

        internal void AddToManager(BaseManager managers = null)
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }
            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddPowerStorage(this);

            QuickLogger.Debug("Power Storage has been connected", true);
        }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddBasePowerStorage(this);

            QuickLogger.Debug($"Power Storage has been connected to base list Count {Manager.BasePowerStorageUnits.Count}", true);
        }

        public void SyncAll()
        {
            Manager.SyncUnits(PowerManager.GetPowerState(), PowerManager.GetChargeMode(), PowerManager.GetAutoActivate());
        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {_prefabId.Id} Data");

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            var saveData = new SaveData
            {
                BodyColor = _currentBodyColor.ColorToVector4(),
                Batteries = PowerManager.Save(),
                PowerState = PowerManager.GetPowerState(),
                ChargeMode = PowerManager.GetChargeMode(),
                ToggleMode = AnimationManager.GetBoolHash(ToggleHash),
                AutoActivate = PowerManager.GetAutoActivate()
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);
            LoadData.CleanOldSaveData();
            QuickLogger.Debug($"Saved {_prefabId.Id} Data");
        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabId != null)
            {
                QuickLogger.Info($"Loading FCSPowerStorage {_prefabId.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson, new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    });

                    SetCurrentBodyColor(savedData.BodyColor.Vector4ToColor());
                    PowerManager.LoadSave(savedData);
                    AddToBaseManager();
                }
                else
                {
                    QuickLogger.Info($"No save file for {_prefabId.Id}");
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }
    }
}
