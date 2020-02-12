using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using FCSPowerStorage.Buildables;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Managers;
using Oculus.Newtonsoft.Json;
using System.IO;
using FCSCommon.Enums;
using FCSTechFabricator.Extensions;
using SMLHelper.V2.Utility;
using UnityEngine;

namespace FCSPowerStorage.Mono
{
    /// <summary>
    /// The controller for the custom battery
    /// </summary>
    internal class FCSPowerStorageController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        #region Private Members
        private PrefabIdentifier _prefabId;
        private bool _initialized;
        private Color _currentBodyColor;
        private int _baseDrainProtectionGoal = 20;
        private int _autoActivateAt = 10;
        #endregion

        #region Internal Properties
        internal string SaveDirectory => Information.GetSaveFileDirectory();
        internal string SaveFile => Path.Combine(SaveDirectory, _prefabId.Id + ".json");
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
        private bool _baseDrainProtection;
        private bool _autoActivate;
        private bool _runStartUpOnEnable;
        private bool _fromSave;

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!_initialized)
            {
                AddToBaseManager();
                Initialize();
            }

            if(!_fromSave) return;

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
                    SetAutoActivateAt(savedData.AutoActivateAt);
                    SetBaseDrainProtection(savedData.BaseDrainProtection);
                    SetBasePowerProtectionGoal(savedData.BaseDrainProtectionGoal);
                    SetAutoActivate(savedData.AutoActivate);

                    if (Display != null)
                    {
                        Display.UpdateTextBoxes(GetAutoActivateAt(), GetBasePowerProtectionGoal());
                    }
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
                if (isActiveAndEnabled)
                {
                    if (!_initialized)
                    {
                        AddToBaseManager();
                        Initialize();
                    }
                    _runStartUpOnEnable = false;
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        private PrefabIdentifier GetPrefabID()
        {
            return GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
        }

        internal string GetPrefabIDString()
        {
            if (_prefabId == null)
            {
                _prefabId = GetPrefabID();
            }

            return _prefabId.Id;
        }

        private void Initialize()
        {
            ToggleHash = Animator.StringToHash("ToggleState");
            StateHash = Animator.StringToHash("state");
            AutoActiveHash = Animator.StringToHash("AutoActivateState");
            BaseDrainHash = Animator.StringToHash("BaseDrain");

            SetCurrentBodyColor(new Color(0.99609375f, 0.99609375f, 0.99609375f));

            _buildable = GetComponentInParent<Constructable>();

            if (_prefabId == null)
            {
                _prefabId = GetPrefabID();
            }

            SystemLightManager = gameObject.EnsureComponent<SystemLightManager>();
            SystemLightManager.Initialize(gameObject);

            PowerManager = gameObject.EnsureComponent<FCSPowerManager>();

            if (PowerManager == null)
            {
                QuickLogger.Error("Power Manager was not found.");
            }
            else
            {
                PowerManager.Initialize(this);
            }

            AnimationManager = gameObject.EnsureComponent<FCSPowerStorageAnimationManager>();

            if (AnimationManager == null)
            {
                QuickLogger.Error("Animation Manager was not found.");
            }
            else
            {
                AnimationManager.Initialize(this);

                if (Manager == null)
                {
                    QuickLogger.Error("Manager is null cannot set Base Drain Protection!");
                }
                else
                {
                    AnimationManager.SetBoolHash(BaseDrainHash, GetBaseDrainProtection());
                }
            }

            if(Display == null)
                Display = gameObject.GetComponent<FCSPowerStorageDisplay>();

            Display.Setup(this);
            _initialized = true;
        }

        #endregion

        #region Internal Methods
        internal void SetCurrentBodyColor(Color color)
        {
            _currentBodyColor = color;
            MaterialHelpers.ChangeBodyColor("Power_Storage_StorageBaseColor_Albedo", color, gameObject);
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

        internal void ValidateAutoConfigUnits(int value)
        {
            if (value > _baseDrainProtectionGoal)
            {
                _baseDrainProtectionGoal = value;
                ErrorMessage.AddMessage(LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.AutoActivationOverLimitMessageKey) + " " + value);
            }

            SetAutoActivateAt(value);
        }

        internal void ValidateBaseProtectionUnits(int value)
        {
            if (value < _autoActivateAt)
            {
                _autoActivateAt = value;
                ErrorMessage.AddMessage(LanguageHelpers.GetLanguage(FCSPowerStorageBuildable.BaseDrainLimitUnderMessageKey) + " " + value);
            }

            _baseDrainProtectionGoal = value;
        }

        internal void AddToBaseManager(BaseManager managers = null)
        {
            if (SubRoot == null)
            {
                SubRoot = GetComponentInParent<SubRoot>();
            }

            Manager = managers ?? BaseManager.FindManager(SubRoot);
            Manager.AddBasePowerStorage(this);
        }

        internal void SyncAll()
        {
            Manager.SyncUnits(PowerManager.GetPowerState(), PowerManager.GetChargeMode(), GetAutoActivate(), GetBaseDrainProtection());
        }

        internal int GetBasePowerProtectionGoal()
        {
            return _baseDrainProtectionGoal;
        }

        public bool GetBaseDrainProtection()
        {
            return _baseDrainProtection;
        }

        internal int GetAutoActivateAt()
        {
            return _autoActivateAt;
        }

        internal bool GetAutoActivate()
        {
            return _autoActivate;
        }

        internal void SetBasePowerProtectionGoal(int value)
        {
            _baseDrainProtectionGoal = value;
        }

        internal void SetBaseDrainProtection(bool value)
        {
            _baseDrainProtection = value;
            AnimationManager.SetBoolHash(BaseDrainHash, GetBaseDrainProtection());
        }

        internal void SetAutoActivateAt(int value)
        {
            _autoActivateAt = value;
        }

        internal void SetAutoActivate(bool value)
        {
            _autoActivate = value;

            if (value)
            {
                if (PowerManager.GetChargeMode() == PowerToggleStates.TrickleMode)
                {
                    PowerManager.SetChargeMode(PowerToggleStates.ChargeMode);
                }

                AddToManager();
            }
            else
            {
                BaseManager.RemovePowerStorage(this);
            }

            AnimationManager.SetBoolHash(AutoActiveHash, value);
            QuickLogger.Debug($"Auto Activate: {GetAutoActivate()}", true);
        }

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Saving {_prefabId.Id} Data");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var saveData = new SaveData
            {
                BodyColor = _currentBodyColor.ColorToVector4(),
                Batteries = PowerManager.Save(),
                PowerState = PowerManager.GetPowerState(),
                ChargeMode = PowerManager.GetChargeMode(),
                ToggleMode = AnimationManager.GetBoolHash(ToggleHash),
                AutoActivate = GetAutoActivate(),
                BaseDrainProtection = GetBaseDrainProtection(),
                BaseDrainProtectionGoal = GetBasePowerProtectionGoal(),
                AutoActivateAt = GetAutoActivateAt()
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);
            LoadData.CleanOldSaveData();
            BaseManager.SaveBases();
            QuickLogger.Debug($"Saved {_prefabId.Id} Data");
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            _fromSave = true;
        }
    }
}
