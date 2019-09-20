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
using UnityEngine;

namespace FCS_AIMarineTurbine.Mono
{
    [RequireComponent(typeof(WeldablePoint))]
    internal class AIJetStreamT242Controller : HandTarget, IHandTarget, IConstructable, IProtoEventListener
    {
        #region Public Properties
        public bool IsBeingDeleted { get; set; }
        public float IncreaseRate { get; set; } = 2f;
        public bool Increasing { get; set; } = true;
        public AIJetStreamT242PowerManager PowerManager { get; private set; }
        public AIJetStreamT242HealthManager HealthManager { get; private set; }
        public AIJetStreamT242AnimationManager AnimationManager { get; private set; }
        public BeaconController BeaconManager { get; private set; }

        public bool _initialized { get; private set; } = true;
        #endregion

        #region Private Members
        private GameObject _seaBase;
        private float _rpmPerDeg = 0.16667f;
        private bool _constructed;
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


        #endregion

        #region Unity Methods
        private void Awake()
        {
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
                HealthManager = gameObject.GetComponent<AIJetStreamT242HealthManager>();
                HealthManager.Initialize(this);
                HealthManager.SetDamageModel(_damage);
                PowerManager = gameObject.GetComponentInParent<AIJetStreamT242PowerManager>();

                if (PowerManager == null)
                {
                    QuickLogger.Error("Power manager component not found!");
                    _initialized = false;
                    return;
                }

                PowerManager.Initialize(this);

                AnimationManager = gameObject.GetComponentInParent<AIJetStreamT242AnimationManager>();

                BeaconManager = gameObject.GetComponentInParent<BeaconController>();

                //_currentBiome = BiomeManager.GetBiome();
            }
            else
            {
                _initialized = false;
                throw new MissingComponentException("Failed to find all components");
            }

            if (!_initialized) return;

            PowerManager.OnKillBattery += Unsubscribe;
        }

        private void Update()
        {
            var constructable = GetComponentInParent<Constructable>();

            if (_initialized && constructable._constructed && transform.parent != null)
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

        internal string GetPrefabID()
        {
            if (_prefabID == null)
            {
                QuickLogger.Error("Prefab ID was null", true);
                return String.Empty;
            }

            return _prefabID.Id;
        }

        #endregion

        #region Private Methods

        private bool FindComponents()
        {
            QuickLogger.Debug("********************************************** In FindComponents **********************************************");


            _turbine = transform.Find("model").Find("anim_parts").Find("Rotor").Find("Turbine_BladeWheel")?.gameObject;

            if (_turbine == null)
            {
                QuickLogger.Error($"Turbine_BladeWheel not found");
                _initialized = false;
                return false;
            }

            _rotor = transform.Find("model").Find("anim_parts").Find("Rotor")?.gameObject;

            if (_rotor == null)
            {
                QuickLogger.Error($"Rotor not found");
                _initialized = false;
                return false;
            }

            _damage = transform.Find("model").Find("static_parts").Find("MarineTurbineDamage")?.gameObject;

            if (_damage == null)
            {
                QuickLogger.Error($"Damage not found");
                _initialized = false;
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
            if (_constructed)
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

        private string GetSolarPowerData()
        {
            return $"Output per second {Mathf.Round(PowerManager.GetEnergyPerSecond() * 10) / 10}";
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

            _constructed = constructed;

            _seaBase = gameObject?.transform?.parent?.gameObject;

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (_seaBase != null)
                {
                    _currentBiome = BiomeManager.GetBiome();
                    RotateToMag();
                    SetCurrentRotation();
                    QuickLogger.Debug($"Turbine Constructed Rotation Set {_rotor.transform.rotation.ToString()} ", true);

                    _display = gameObject.GetOrAddComponent<AIJetStreamT242Display>();
                    _display.Setup(this);
                }
                else
                {
                    ErrorMessage.AddMessage($"[AIJetStreamT242] ERROR: Must be on a platform to operate");
                    QuickLogger.Debug("ERROR: Can not work out what base it was placed inside.");
                }
            }
        }

        private void SetCurrentRotation()
        {
            QuickLogger.Debug("********************************************** In Set Current Rotation **********************************************");
            if (_rotor == null) return;
            _rotor.transform.rotation = AISolutionsData.StartingRotation;
            QuickLogger.Debug("********************************************** In Set Current Rotation **********************************************");
        }

        private void RotateToMag()
        {
            if (_rotor == null) return;

            _rotor.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
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
            QuickLogger.Debug("// ****************************** Load Data *********************************** //");

            if (_prefabID != null)
            {
                QuickLogger.Info($"Loading JetStream {_prefabID.Id}");

                if (File.Exists(SaveFile))
                {
                    string savedDataJson = File.ReadAllText(SaveFile).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                    PowerManager.SetHasBreakerTripped(savedData.HasBreakerTripped);
                    HealthManager.SetHealth(savedData.Health);
                    PowerManager.SetCharge(savedData.Charge);
                    _currentSpeed = savedData.CurrentSpeed;
                    //RotateTurbine(savedData.DegPerSec);
                    _targetRotation = savedData.TurbineRot.TargetRotationToQuaternion();
                    QuickLogger.Debug($"Target Rotation Set {_targetRotation}", true);
                    _currentBiome = savedData.Biome;
                    HealthManager.SetPassedTime(savedData.PassedTime);
                    AISolutionsData.StartingRotation = _targetRotation;
                    PowerManager.SetStoredPower(savedData.StoredPower);
                    if (_display != null)
                    {
                        _display.SetCurrentPage();
                    }
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
            QuickLogger.Debug("// ****************************** Loaded Data *********************************** //");
        }

        public int ScreenState { get; set; }

        #endregion

        public void OnHandHover(GUIHand hand)
        {
            HandReticle.main.SetInteractText(GetSolarPowerData(), false);
            HandReticle.main.SetIcon(HandReticle.IconType.Default);
        }

        public void OnHandClick(GUIHand hand)
        {
        }
    }
}