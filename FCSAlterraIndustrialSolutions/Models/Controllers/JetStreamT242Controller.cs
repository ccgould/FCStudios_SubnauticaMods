using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Data;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Utilities;
using FCSCommon.Components;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public class TurbineEventArgs : EventArgs
    {
        public JetStreamT242Controller Controller { get; set; }
    }

    [RequireComponent(typeof(WeldablePoint))]
    public partial class JetStreamT242Controller : MonoBehaviour, IConstructable, IProtoEventListener, IPowerInterface
    {
        #region Public Properties

        public bool HasBreakerTripped { get; set; }
        public LiveMixin LiveMixin { get; set; } = new LiveMixin();
        public int HealthMultiplyer { get; set; } = 10;
        public float DamageDayCycle { get; set; } = 1.0f;
        public bool IsBeingDeleted { get; set; }
        public bool IsBeingPinged { get; set; }
        public float MaxSpeed = 300f;
        public float MinSpeed = 90f;
        public float Multiplier = 0.161f;
        public float Charge { get; set; }
        public string ID { get; set; }
        public float IncreaseRate { get; set; } = 2f;
        public bool Increasing { get; set; } = true;
        public bool IsDamagedFlag { get; set; } = true;

        #endregion

        #region Private Members

        private bool _initialized;
        private JetStreamT242Display _jetStreamT242Display;
        private GameObject _seaBase;
        private bool _isEnabled;
        private float _rpmPerDeg = 0.16667f;
        private PowerRelay _powerRelay;
        private bool _constructed;
        private GameObject _rotor;
        private string _currentBiome;
        private float _currentSpeed = 0;
        private float _passedTime = 0f;
        private Quaternion _targetRotation;
        private GameObject _turbine;
        private double _damageTimeInSeconds = 2520;
        private float _capacity;
        private bool _isBatteryDestroyed;
        public float MaxPowerPerMin { get; set; }= 100;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            try
            {
                ID = GetComponentInParent<PrefabIdentifier>().Id;
                LiveMixin = GetComponentInParent<LiveMixin>();
                _capacity = LoadItems.JetStreamT242Config.MaxCapacity;
                InvokeRepeating("HealthChecks", 0, 1);

                if (LiveMixin == null)
                {
                    Log.Error($"LiveMixing not found!");
                }

                if (LiveMixin.data == null)
                {
                    Log.Error($"LiveMixing Data  is null!");
                    Log.Info($"Creating Data");
                    LiveMixin.data = AISolutionsData.CustomLiveMixinData.Get();
                    Log.Info($"Created Data");
                }
                else
                {
                    LiveMixin.data.weldable = true;
                }

                _turbine = transform.Find("model").Find("Rotor").Find("Turbine_BladesWheel")?.gameObject;
                _rotor = transform.Find("model").Find("Rotor")?.gameObject;
                ID = GetComponentInParent<PrefabIdentifier>().Id;

                //InvokeRepeating("GetLog", 1, 3);
                var currentBiome = GetBiome();
                if (!string.IsNullOrEmpty(currentBiome))
                {
                    var data = GetBiomeData(currentBiome);
                }

                AISolutionsData.OnRotationChanged += AiSolutionsDataOnOnRotationChanged;

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private void Update()
        {
            var constructable = GetComponentInParent<Constructable>();

            if (!_initialized && constructable._constructed && transform.parent != null)
            {
                Initialize();
            }

            if (!_initialized || !constructable._constructed)
            {
                //Log.Info("Not Constructed");
            }

            SystemHandler();

            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= _damageTimeInSeconds)
            {
                ApplyDamage();
            }

            AISolutionsData.UpdateTime();

            ProducePower();
        }

        #endregion

        #region Public Methods

        public bool IsDamageApplied()
        {
            if (LiveMixin == null) return false;

            return LiveMixin.health <= 0;
        }

        public float GetHealth()
        {
            return LiveMixin.health;
        }

        public float GetDepth()
        {
            return gameObject == null ? 0f : Ocean.main.GetDepthOf(gameObject);
        }

        public void ChangeMotorSpeed(float speed)
        {
            // increase or decrease the current speed depending on the value of increasing
            _currentSpeed = Mathf.Clamp(_currentSpeed + DayNightCycle.main.deltaTime * IncreaseRate * (Increasing ? 1 : -1), 0, speed);
            _turbine.transform.Rotate(Vector3.forward, _currentSpeed * DayNightCycle.main.deltaTime);
        }

        public void ApplyDamage()
        {
            if (LiveMixin.health > 0 && LoadItems.JetStreamT242Config.EnableWear)
            {
                LiveMixin.health = Mathf.Clamp(LiveMixin.maxHealth - HealthMultiplyer, 0f, 100f);
            }
            ResetTimer();
        }

        public int GetSpeed()
        {
            return Convert.ToInt32(_currentSpeed * _rpmPerDeg);
        }

        public void TriggerPowerOff()
        {
            HasBreakerTripped = true;
            _jetStreamT242Display.StartCoroutine("PowerOff");
        }

        public void TriggerPowerOn()
        {
            if (LiveMixin.health <= 0.0f) return;
            _jetStreamT242Display.StartCoroutine("PowerOn");
            HasBreakerTripped = false;
        }

        public void PingObject(bool isBeingPinged)
        {
            #region Animator

            var animator = transform.GetComponent<Animator>();

            if (animator == null)
            {
                Log.Error($"Cannot find animator to animate the pinger");
                return;
            }
            #endregion

            animator.enabled = true;

            #region Arrow Indicator

            var arrowIndic = transform.Find("model").Find("ArrowIndic")?.gameObject;

            if (arrowIndic == null)
            {
                Log.Error($"Cannot find arrowIndic");
                return;
            }
            #endregion


            if (isBeingPinged)
            {
                IsBeingPinged = true;
                arrowIndic.SetActive(true);
                animator.SetBool("BeaconOff", false);
                animator.SetBool("BeaconOn", true);
            }
            else
            {
                IsBeingPinged = false;
                arrowIndic.SetActive(false);
                animator.SetBool("BeaconOn", false);
                animator.SetBool("BeaconOff", true);
            }
        }

        #endregion

        #region Private Methods

        private void RotateRotor()
        {
            _rotor.transform.rotation = Quaternion.Lerp(_rotor.transform.rotation, _targetRotation, 1 * DayNightCycle.main.deltaTime);
        }

        private void AiSolutionsDataOnOnRotationChanged(Quaternion axis)
        {
            _targetRotation = axis;
        }

        private void Initialize()
        {
            InvokeRepeating("UpdatePowerRelay", 0, 1);
            _initialized = true;
        }

        private void UpdatePowerRelay()
        {
            try
            {
                var relay = PowerSource.FindRelay(transform);

                if (relay != null && relay != _powerRelay)
                {
                    if (_powerRelay != null)
                    {
                        _powerRelay.RemoveInboundPower(this);
                    }
                    _powerRelay = relay;
                    _powerRelay.AddInboundPower(this);
                    CancelInvoke("UpdatePowerRelay");
                }
                else
                {
                    _powerRelay = null;
                }

                if (_powerRelay != null)
                {
                    _powerRelay.RemoveInboundPower(this);
                    _powerRelay.AddInboundPower(this);
                    CancelInvoke("UpdatePowerRelay");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private void GetLog()
        {

            Log.Info($"// ============================= Turbine - {ID} ============================= //");

            //Log.Info("Health");
            //Log.Info($"Health {Health}%");
            //Log.Info($"Health Passed Time {passedTime}%");
            //Log.Info($"Passed Time =  {passedTime}");
            //Log.Info("Passed Time");
            //Log.Info($"{passedTime}%");
            Log.Info($"Current Speed for ID = { ID} || Biome = {_currentBiome} || MaxSpeed {MaxSpeed}");
            Log.Info($"Charge: {Charge} || MaxCapacity: {LoadItems.JetStreamT242Config.MaxCapacity} || Capacity {_capacity}");

            Log.Info($"// ============================= Turbine - {ID} ============================= //");

            //Log.Info($"Current biome: {Player.main.GetBiomeString()}");
            //Log.Info($"Charge: {_charge} || Capacity: {Capacity}");
        }

        private void StopMotor()
        {
            Increasing = false;
        }

        private void StartMotor()
        {
            Increasing = true;
        }

        private void Start()
        {
            ApplyShaders();
        }

        private void ApplyShaders()
        {
            //Use this to do the Emission
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;


                    if (material.name.StartsWith("FCS_MarineTurbine_Tex_Damaged"))
                    {
                        //material.EnableKeyword("MARMO_SPECMAP");
                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");
                        //material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetVector("_EmissionColor", new Color(1f, 1f, 1f, 1f) * 1.0f);
                        material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("JetStreamT242_MarineTurbineMat_Emission_Red", LoadItems.ASSETBUNDLE));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));


                        //material.SetFloat("_Fresnel", 0f);
                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_Metallic", MaterialHelpers.FindTexture2D("JetStreamT242_MarineTurbineMat_MetallicSmoothness", LoadItems.ASSETBUNDLE));
                        material.SetFloat("_Glossiness", 0.2f);
                    }
                    //I am using StartsWith because the material name contains (Instance)
                    else if (material.name.StartsWith("FCS_MarineTurbine_Tex"))
                    {
                        //material.EnableKeyword("MARMO_SPECMAP");
                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");
                        //material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetVector("_EmissionColor", new Color(0f, 1.437931f, 1.5f, 1.0f) * 1.0f);
                        material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("JetStreamT242_MarineTurbineMat_Emission", LoadItems.ASSETBUNDLE));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));


                        //material.SetFloat("_Fresnel", 0f);
                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_Metallic", MaterialHelpers.FindTexture2D("JetStreamT242_MarineTurbineMat_MetallicSmoothness", LoadItems.ASSETBUNDLE));
                        material.SetFloat("_Glossiness", 0.2f);
                    }
                    else if (material.name.StartsWith("FCS_TurbinesMonitor"))
                    {
                        //material.EnableKeyword("MARMO_SPECMAP");
                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");
                        //material.EnableKeyword("_METALLICGLOSSMAP");

                        material.SetVector("_EmissionColor", new Color(0f, 1.437931f, 1.5f, 1.0f) * 1.0f);
                        material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("FCS_TurbinesMonitor_llum_Emission", LoadItems.ASSETBUNDLE));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));


                        //material.SetFloat("_Fresnel", 0f);
                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_Metallic", MaterialHelpers.FindTexture2D("JetStreamT242_MarineTurbineMat_MetallicSmoothness", LoadItems.ASSETBUNDLE));
                        material.SetFloat("_Glossiness", 0.2f);
                    }


                    if (material.name.StartsWith("FCS_SUBMods_GlobalDecals"))
                    {
                        material.EnableKeyword("_ZWRITE_ON");
                        material.EnableKeyword("MARMO_ALPHA");
                        material.EnableKeyword("MARMO_ALPHA_CLIP");
                    }
                }
            }
        }

        private void ProducePower()
        {
            if (HasBreakerTripped)
            {
                Charge = 0.0f;
            }
            else
            {
                var decPercentage = (MaxPowerPerMin / MaxSpeed)/60;

                //Energy per sec
                var energyPerSec = _currentSpeed * decPercentage;
                //var chargeAmount = (_currentSpeed / MaxSpeed);
                //Log.Info(energyPerSec.ToString());

                Charge = Mathf.Clamp(Charge + energyPerSec * DayNightCycle.main.deltaTime, 0, LoadItems.JetStreamT242Config.MaxCapacity);
            }
        }

        private AISolutionsData.BiomeItem GetBiomeData(string biome)
        {

            return LoadItems.JetStreamT242Config.BiomeSpeeds.GetOrDefault(biome.ToLower(), new AISolutionsData.BiomeItem { Speed = 90 });

        }

        private void HealthChecks()
        {
            try
            {
                if (GetHealth() >= 100f && !IsDamagedFlag && _constructed)
                {
                    Log.Info("Turbine Repaired");
                    StartCoroutine(UpdateDamageMaterial());
                    IsDamagedFlag = true;
                }

                if (GetHealth() <= 0f && IsDamagedFlag && _constructed)
                {
                    Log.Info("Turbine Damaged");
                    StartCoroutine(UpdateDamageMaterial());
                    IsDamagedFlag = false;
                }
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }
        }

        private string GetBiome()
        {
            var biome = string.Empty;
            var curBiome = Player.main.GetBiomeString().ToLower();

            Log.Info($"Current Player Biome: {curBiome}");

            var match = FindMatchingBiome(curBiome);

            if (string.IsNullOrEmpty(match))
            {
                Log.Error($"Biome {curBiome} not found! Setting biome to none");
                biome = "none";
            }
            else
            {
                biome = match;
            }

            return biome;
        }

        private string FindMatchingBiome(string biome)
        {

            if (string.IsNullOrEmpty(biome)) return String.Empty;

            var result = string.Empty;
            Log.Info("// ============================= IN FindMatchingBiome ============================= //");

            foreach (var biomeItem in LoadItems.JetStreamT242Config.BiomeSpeeds)
            {
                Log.Info($"Checking for {biome} || Current biome in iteration = {biomeItem.Key}");

                if (biome.StartsWith(biomeItem.Key))
                {
                    result = biomeItem.Key;
                    Log.Info($"Find Biome Result = {result}");
                    break;
                }
            }

            Log.Info("// ============================= IN FindMatchingBiome ============================= //");

            return result;
        }

        private bool IsUnderWater()
        {
            return GetDepth() >= 7.0f;
        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;

                if (_jetStreamT242Display != null)
                {
                    Log.Info("Turnoff");

                    TurnDisplayOff();
                }

                _jetStreamT242Display = gameObject.AddComponent<JetStreamT242Display>();
                _jetStreamT242Display.Setup(this);

            }
            catch (Exception e)
            {
                Log.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (_jetStreamT242Display != null)
            {
                _jetStreamT242Display.TurnDisplayOff();
                Destroy(_jetStreamT242Display);
                _jetStreamT242Display = null;
            }
        }

        private void KillBattery()
        {
            Log.Info($"KillBattery");
            AISolutionsData.OnRotationChanged -= AiSolutionsDataOnOnRotationChanged;
            _capacity = Charge = 0;
        }

        private void DestroyBattery()
        {
            Log.Info($"DestroyBattery");
            Charge = _capacity = 0f;
            _isBatteryDestroyed = true;
        }

        private void ResetTimer()
        {
            _passedTime = 0;
        }

        private void SystemHandler()
        {
            /*
             * Handles starting and stopping the turbine and its rotor based off conditions
             */
            if (_constructed)
            {
                if (HasBreakerTripped || !IsUnderWater() || GetHealth() <= 0)
                {
                    if (!string.IsNullOrEmpty(_currentBiome))
                    {
                        ChangeMotorSpeed(GetBiomeData(_currentBiome).Speed);
                    }
                    StopMotor();
                }
                else
                {
                    if (!string.IsNullOrEmpty(_currentBiome))
                    {
                        ChangeMotorSpeed(GetBiomeData(_currentBiome).Speed);
                    }
                    StartMotor();
                    RotateRotor();
                }

                if (LiveMixin.health <= 0 && !_isBatteryDestroyed)
                {
                    DestroyBattery();
                }
                else if (LiveMixin.health > 0 && _isBatteryDestroyed)
                {
                    RepairBattery();
                }
            }

        }

        private void RepairBattery()
        {
            _capacity = LoadItems.JetStreamT242Config.MaxCapacity;
            _isBatteryDestroyed = false;
        }

        #endregion

        #region Overrides

        private void OnDestroy()
        {
            KillBattery();

            try
            {
                if (_powerRelay != null)
                {
                    _powerRelay.RemoveInboundPower(this);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

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
            Log.Info($"Constructed - {constructed}");

            _constructed = constructed;

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (_isEnabled == false)
                {
                    _currentBiome = GetBiome();
                    _isEnabled = true;
                    _rotor.transform.rotation = Quaternion.Euler(0, -Input.compass.magneticHeading, 0);
                    _rotor.transform.rotation = AISolutionsData.StartingRotation;
                    StartCoroutine(Startup());
                    //LiveMixin.health = 10f;
                }
                else
                {
                    TurnDisplayOn();
                }
            }
            else
            {
                if (_isEnabled)
                {
                    TurnDisplayOff();
                }
            }
        }

        #endregion

        #region IProtoEventListener

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

            if (!Directory.Exists(Information.GetSaveFileDirectory()))
                Directory.CreateDirectory(Information.GetSaveFileDirectory());

            var saveData = new SaveData
            {
                HasBreakerTripped = HasBreakerTripped,
                TurbineRot = new TargetRotation(_targetRotation),
                Health = LiveMixin.health,
                Charge = Charge,
                DegPerSec = GetBiomeData(_currentBiome).Speed,
                Biome = _currentBiome,
                CurrentSpeed = _currentSpeed,
                PassedTime = _passedTime
            };

            var output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(FilesHelper.GenerateSaveFileString(ID), output);

            LoadItems.CleanOldSaveData();
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            Log.Info("// ****************************** Load Data *********************************** //");

            if (ID != null)
            {
                Log.Info($"Loading ARSolutions {ID}");

                string filePath = FilesHelper.GenerateSaveFileString(ID);
                if (File.Exists(filePath))
                {
                    string savedDataJson = File.ReadAllText(filePath).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                    HasBreakerTripped = savedData.HasBreakerTripped;
                    LiveMixin.health = savedData.Health;
                    Charge = savedData.Charge;
                    _currentSpeed = savedData.CurrentSpeed;
                    _turbine.transform.Rotate(Vector3.forward, savedData.DegPerSec);
                    _targetRotation = savedData.TurbineRot.TargetRotationToQuaternion();
                    _currentBiome = savedData.Biome;
                    _passedTime = savedData.PassedTime;
                    AISolutionsData.StartingRotation = _targetRotation;
                }
            }
            else
            {
                Log.Error("PrefabIdentifier is null");
            }
            Log.Info("// ****************************** Loaded Data *********************************** //");
        }

        #endregion

        #region IPowerInterface

        public float GetPower()
        {
            if (Charge < 0.1)
            {
                Charge = 0.0f;
            }

            return Charge;
        }

        public float GetMaxPower()
        {
            return _capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;


            bool result;
            if (amount >= 0f)
            {
                result = (amount <= LoadItems.JetStreamT242Config.MaxCapacity - Charge);
                modified = Mathf.Min(amount, LoadItems.JetStreamT242Config.MaxCapacity - Charge);
                Charge += Mathf.Round(modified);
            }
            else
            {
                result = (Charge >= -amount);
                if (GameModeUtils.RequiresPower())
                {
                    modified = -Mathf.Min(-amount, Charge);
                    Charge += Mathf.Round(modified);
                }
                else
                {
                    modified = amount;
                }
            }

            return result;
        }

        public bool HasInboundPower(IPowerInterface powerInterface)
        {
            return false;
        }

        public bool GetInboundHasSource(IPowerInterface powerInterface)
        {
            return false;
        }
        #endregion

        #region IEnumerator

        private IEnumerator Startup()
        {
            if (IsBeingDeleted) yield break;
            yield return new WaitForEndOfFrame();
            if (IsBeingDeleted) yield break;

            _seaBase = gameObject?.transform?.parent?.gameObject;
            if (_seaBase == null)
            {
                ErrorMessage.AddMessage($"[{Information.ModName}] ERROR: Can not work out what base it was placed inside.");
                Log.Error("ERROR: Can not work out what base it was placed inside.");
                yield break;
            }

            TurnDisplayOn();
        }

        private IEnumerator UpdateDamageMaterial()
        {
            yield return new WaitForEndOfFrame();
            var damageMatController = GetComponentInParent<DamagedMaterialController>();

            if (damageMatController != null)
            {
                if (LiveMixin.health <= 0f)
                {
                    Log.Info($"Applying Damage to {ID}");
                    var damage = damageMatController.ReplaceMaterials(LoadItems.ASSETBUNDLE, transform, out string log);
                    Log.Info($"Found Material: {damage} || Log:{log}");
                }

                if (LiveMixin.health >= 100f)
                {
                    Log.Info($"Removing Damage to {ID}");
                    var damage = damageMatController.ReplaceMaterials(LoadItems.ASSETBUNDLE, transform, out string log);
                    Log.Info($"Found Material: {damage} || Log:{log}");
                }

                ApplyShaders();
            }
            else
            {
                Log.Error($"Component {nameof(DamagedMaterialController)} not found!");
            }

            yield return new WaitForEndOfFrame();
        }

        #endregion
    }
}