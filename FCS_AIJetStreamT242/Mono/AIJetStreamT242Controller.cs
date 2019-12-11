using FCS_AIJetStreamT242.Mono;
using FCS_AIMarineTurbine.Display;
using FCS_AIMarineTurbine.Model;
using FCSCommon.Components;
using FCSCommon.Extensions;
using FCSCommon.Objects;
using FCSCommon.Utilities;
using Oculus.Newtonsoft.Json;
using SMLHelper.V2.Utility;
using System;
using System.IO;
using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Display.Patching;
using rail;
using UnityEngine;

namespace FCS_AIMarineTurbine.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class AIJetStreamT242Controller : HandTarget, IHandTarget, IConstructable, IProtoEventListener
    {
        #region Public Properties

        internal bool IsInitialized { get; private set; }
        internal bool IsBeingDeleted { get; set; }
        internal float IncreaseRate { get; set; } = 2f;
        internal bool Increasing { get; set; } = true;
        internal AIJetStreamT242PowerManager PowerManager { get; private set; }
        internal AIJetStreamT242HealthManager HealthManager { get; private set; }
        internal AIJetStreamT242AnimationManager AnimationManager { get; private set; }
        internal BeaconController BeaconManager { get; private set; }
        internal bool IsConstructed => _buildable != null && _buildable.constructed;
        #endregion

        #region Private Members


        private GameObject _seaBase;
        private float _rpmPerDeg = 0.16667f;
        private GameObject _rotor;
        private string _currentBiome;
        public float MaxSpeed;
        private PrefabIdentifier _prefabID;
        private readonly string SaveDirectory = Path.Combine(SaveUtils.GetCurrentSaveDataDir(), "AIMarineTurbine");
        private string SaveFile => Path.Combine(SaveDirectory, _prefabID.Id + ".json");
        private Quaternion _targetRotation;
        private GameObject _turbine;
        private float _currentSpeed;
        private GameObject _damage;
        private AIJetStreamT242Display _display;
        private bool _runStartUpOnEnable;
        private SaveData _data;
        private Constructable _buildable;

        #endregion

        #region Unity Methods

        public void Awake()
        {
            //Needed for the patch do not remove
        }

        private void OnEnable()
        {
            if (!_runStartUpOnEnable) return;

            if (!IsInitialized)
            {
                Initialize();
            }

            if (_display != null)
            {
                _display.Setup(this);
            }

            if (_seaBase != null)
            {
                _currentBiome = BiomeManager.GetBiome();
                RotateToMag();
                SetCurrentRotation();
                QuickLogger.Debug($"World Rotation {AISolutionsData.StartingRotation} ", true);

                QuickLogger.Debug($"Turbine Constructed Rotation Set {_rotor.transform.rotation.ToString()} ", true);
            }

            if (_data == null)
                ReadySaveData();

            if (_data != null)
            {
                QuickLogger.Debug("// ****************************** Load Data *********************************** //");

                if (_prefabID != null)
                {
                    QuickLogger.Info($"Loading JetStream {_prefabID.Id}");

                    PowerManager.SetHasBreakerTripped(_data.HasBreakerTripped);
                    HealthManager.SetHealth(_data.Health);
                    PowerManager.SetCharge(_data.Charge);
                    _currentSpeed = _data.CurrentSpeed;
                    //RotateTurbine(savedData.DegPerSec);
                    _targetRotation = _data.TurbineRot.TargetRotationToQuaternion();
                    QuickLogger.Debug($"Target Rotation Set {_targetRotation}", true);
                    _currentBiome = _data.Biome;
                    HealthManager.SetPassedTime(_data.PassedTime);
                    AISolutionsData.StartingRotation = _targetRotation;
                    PowerManager.SetStoredPower(_data.StoredPower);
                    if (_display != null)
                    {
                        _display.SetCurrentPage();
                    }
                }
                else
                {
                    QuickLogger.Error("PrefabIdentifier is null");
                }
                QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
            }

            _runStartUpOnEnable = false;
        }
        
        private void Update()
        {
            if(!IsOperational()) return;
            UpdatePowerSafeState();
        }
        
        #endregion

        #region Public Methods

        internal float GetCurrentSpeed()
        {
            return _currentSpeed;
        }

        internal float GetDepth()
        {
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
        }

        internal void ChangeMotorSpeed(float speed)
        {
            if (_seaBase == null) return;

            if (!_seaBase.name.StartsWith("Base", StringComparison.OrdinalIgnoreCase)) return;

            //increase or decrease the current speed depending on the value of increasing
            _currentSpeed = Mathf.Clamp(_currentSpeed + DayNightCycle.main.deltaTime * IncreaseRate * (Increasing ? 1 : -1), 0, speed);
            _turbine.transform.Rotate(Vector3.up, _currentSpeed * DayNightCycle.main.deltaTime);
        }

        internal int GetSpeed()
        {
            return Convert.ToInt32(_currentSpeed * _rpmPerDeg);
        }

        internal string GetPrefabId()
        {
            if (_prefabID == null)
            {
                _prefabID = GetComponent<PrefabIdentifier>() ?? GetComponentInParent<PrefabIdentifier>();
            }
            return _prefabID.Id;
        }

        #endregion

        #region Private Methods

        private void UpdatePowerSafeState()
        {

            if (PowerManager == null) return;

            if (IsInitialized && IsConstructed)
            {
                PowerManager.IsSafeToContinue = true;

                //HealthManager.IsSafeToContinue = true;

                SystemHandler();
            }
            else
            {
                PowerManager.IsSafeToContinue = false;

                //HealthManager.IsSafeToContinue = false;
            }
        }

        private void ReadySaveData()
        {
            _prefabID = GetComponentInParent<PrefabIdentifier>() ?? GetComponent<PrefabIdentifier>();
            var id = _prefabID.Id ?? string.Empty;

            if (File.Exists(SaveFile))
            {
                string savedDataJson = File.ReadAllText(SaveFile).Trim();

                //LoadData
                _data = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

            }
        }

        private void Initialize()
        {
            _buildable = GetComponent<Constructable>() ?? GetComponentInParent<Constructable>();

            if (FindComponents())
            {
                QuickLogger.Debug($"Turbine Components Found", true);

                _prefabID = GetComponentInParent<PrefabIdentifier>();

                var currentBiome = BiomeManager.GetBiome();

                if (!string.IsNullOrEmpty(currentBiome))
                {
                    var data = BiomeManager.GetBiomeData(currentBiome);
                }

                AISolutionsData.Instance.OnRotationChanged += AiSolutionsDataOnOnRotationChanged;

                if (HealthManager == null)
                    HealthManager = gameObject.GetComponent<AIJetStreamT242HealthManager>() ?? GetComponentInParent<AIJetStreamT242HealthManager>();


                HealthManager.Initialize(this);
                HealthManager.SetHealth(100);
                HealthManager.SetDamageModel(_damage);

                if (PowerManager == null)
                    PowerManager = GetComponentInParent<AIJetStreamT242PowerManager>() ?? GetComponent<AIJetStreamT242PowerManager>();

                PowerManager.Initialize(this);

                AnimationManager = gameObject.GetComponentInParent<AIJetStreamT242AnimationManager>();

                BeaconManager = gameObject.GetComponentInParent<BeaconController>();

                if (_display == null)
                    _display = GetComponent<AIJetStreamT242Display>() ?? GetComponentInParent<AIJetStreamT242Display>();

                IsInitialized = true;
                //_currentBiome = BiomeManager.GetBiome();
            }
            else
            {
                IsInitialized = false;
                throw new MissingComponentException("Failed to find all components");
            }

            if (!IsInitialized) return;

            PowerManager.OnKillBattery += Unsubscribe;
        }

        private bool FindComponents()
        {
            QuickLogger.Debug("********************************************** In FindComponents **********************************************");


            _turbine = transform.Find("model").Find("anim_parts").Find("Rotor").Find("Turbine_BladeWheel")?.gameObject;

            if (_turbine == null)
            {
                QuickLogger.Error($"Turbine_BladeWheel not found");
                IsInitialized = false;
                return false;
            }

            _rotor = transform.Find("model").Find("anim_parts").Find("Rotor")?.gameObject;

            if (_rotor == null)
            {
                QuickLogger.Error($"Rotor not found");
                IsInitialized = false;
                return false;
            }

            _damage = transform.Find("model").Find("static_parts").Find("MarineTurbineDamage")?.gameObject;

            if (_damage == null)
            {
                QuickLogger.Error($"Damage not found");
                IsInitialized = false;
                return false;
            }

            return true;
        }

        private void RotateRotor()
        {
            if (_seaBase == null) return;

            if (!_seaBase.name.StartsWith("Base", StringComparison.OrdinalIgnoreCase)) return;
            _rotor.transform.rotation = Quaternion.Lerp(_rotor.transform.rotation, _targetRotation, 1 * DayNightCycle.main.deltaTime);

        }

        private void AiSolutionsDataOnOnRotationChanged(Quaternion axis)
        {
            _targetRotation = axis;
        }

        private void StopMotor()
        {
            Increasing = false;
        }

        private void StartMotor()
        {
            Increasing = true;
        }

        private bool IsUnderWater()
        {
            return GetDepth() >= 7.0f;
        }

        private void Unsubscribe()
        {
            AISolutionsData.Instance.OnRotationChanged -= AiSolutionsDataOnOnRotationChanged;
        }

        private void SystemHandler()
        {
            if (MaxSpeed <= 0)
            {
                MaxSpeed = BiomeManager.GetBiomeData(_currentBiome).Speed;
            }


            /*
             * Handles starting and stopping the turbine and its rotor based off conditions
             */
            if (IsConstructed)
            {
                if (PowerManager.GetHasBreakerTripped() || !IsUnderWater() || HealthManager.GetHealth() <= 0)
                {
                    if (!string.IsNullOrEmpty(_currentBiome))
                    {
                        ChangeMotorSpeed(MaxSpeed);
                    }
                    StopMotor();
                }
                else
                {
                    if (!string.IsNullOrEmpty(_currentBiome))
                    {
                        ChangeMotorSpeed(MaxSpeed);
                    }
                    StartMotor();
                    RotateRotor();
                }
            }
        }

        private string GetTurbinePowerData()
        {
            return PowerManager == null ? "Output per second 0" : $"Output per second {Mathf.Round(PowerManager.GetEnergyPerSecond() * 10) / 10}";
        }

        #endregion

        #region Overrides
        private void OnDestroy()
        {
            Destroy(gameObject);
        }
        #endregion

        #region IConstructable
        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            QuickLogger.Debug($"Constructed - {constructed}");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                _seaBase = gameObject?.transform?.parent?.gameObject;

                if (isActiveAndEnabled)
                {
                    if (IsOperational())
                    {
                        if (!IsInitialized)
                        {
                            Initialize();
                        }

                        if (_display != null)
                        {
                            _display.Setup(this);
                            _runStartUpOnEnable = false;
                        }

                        _currentBiome = BiomeManager.GetBiome();
                        RotateToMag();
                        SetCurrentRotation();
                        QuickLogger.Debug($"Turbine Constructed Rotation Set {_rotor.transform.rotation.ToString()} ", true);
                    }
                    else
                    {
                        QuickLogger.Message(DisplayLanguagePatching.NotOperational(),true);
                    }
                }
                else
                {
                    _runStartUpOnEnable = true;
                }
            }
        }

        private void SetCurrentRotation()
        {
            QuickLogger.Debug("********************************************** In Set Current Rotation **********************************************");
            if (_rotor == null) return;
           _targetRotation = AISolutionsData.StartingRotation;
            QuickLogger.Debug("********************************************** In Set Current Rotation **********************************************");
        }

        private void RotateToMag()
        {
            if (_rotor == null) return;
            _rotor.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
        }


        public bool IsUpright()
        {

            if (Mathf.Approximately(transform.up.y, 1f))
            {
                return true;
            }

            return false;
        }

        #endregion

        #region IProtoEventListener

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Info($"Saving {_prefabID.Id} Data");

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            var saveData = new SaveData
            {
                HasBreakerTripped = PowerManager.GetHasBreakerTripped(),
                TurbineRot = new TargetRotation(_targetRotation),
                Health = HealthManager.GetHealth(),
                Charge = PowerManager.GetCharge(),
                DegPerSec = BiomeManager.GetBiomeData(_currentBiome).Speed,
                Biome = _currentBiome,
                CurrentSpeed = _currentSpeed,
                PassedTime = HealthManager.GetPassedTime(),
                StoredPower = PowerManager.GetStoredPower()
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(SaveFile, output);

            QuickLogger.Info($"Saved {_prefabID.Id} Data");


            //TODO Replace this
            //LoadItems.CleanOldSaveData();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            
        }

        public int ScreenState { get; set; }
        public bool IsConnectedToBase => _seaBase != null;

        #endregion

        internal bool IsOperational()
        {
            return IsUpright() && IsConnectedToBase;
        }

        public void OnHandHover(GUIHand hand)
        {
            if (!IsOperational())
            {
                HandReticle.main.SetInteractText(DisplayLanguagePatching.NotOperational(), false);
                HandReticle.main.SetIcon(HandReticle.IconType.Default);
            }
            else
            {
                HandReticle.main.SetInteractText(GetTurbinePowerData(), false);
                HandReticle.main.SetIcon(HandReticle.IconType.Default);
            }
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}