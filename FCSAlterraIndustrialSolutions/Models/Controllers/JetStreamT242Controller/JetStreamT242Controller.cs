using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Data;
using FCSAlterraIndustrialSolutions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public partial class JetStreamT242Controller : MonoBehaviour, IConstructable, IProtoEventListener, IProtoTreeEventListener, IPowerInterface
    {
        private bool _hasBreakerTripped;
        public bool HasBreakerTripped
        {
            get => _hasBreakerTripped;
            set
            {
                _hasBreakerTripped = value;
                if (_hasBreakerTripped)
                {
                    StopMotor();
                }
                else
                {
                    ChangeMotorSpeed();
                }
            }
        }

        public int Health { get; set; } = 100;
        public int MaxHealth { get; } = 100;
        public int HealthMultiplyer { get; set; } = 25;
        public float DamageDayCycle { get; set; } = 1.0f;
        public bool IsBeingDeleted { get; set; }

        private const int GameDayCycle = 1200;
        private ItemsContainer container;
        private readonly bool _containerHasItem;
        private float powerPerSecond = 4.166667f;
        private float _totalDays = 36000f;
        private PowerRelay _powerRelay;
        private HingeJoint _hinge;
        private JointMotor _motor;
        public float MaxSpeed = 300f;
        public float MinSpeed = 90f;
        private readonly float maxDepth = 1456f;
        public float multiplier = 0.161f;
        public float Capacity = 20.0f;
        public float Charge;
        private bool _initialized;
        private JetStreamT242Display _jetStreamT242Display;
        private GameObject _seaBase;
        private bool _isEnabled;
        private float _rpmPerDeg = 0.16667f;
        private string _id;
        private float passedTime = 0f;
        private float _turnTableSpeed = 1;
        private bool _rotate;
        private Quaternion _targetRotation;
        #region Unity Methods

        private void Awake()
        {
            _id = GetComponentInParent<PrefabIdentifier>().ClassId;
            container = new ItemsContainer(1, 1, gameObject.transform, "Filter", null);
            container.isAllowedToAdd += ItemContainerIsAllowedToAdd;
            container.onAddItem += ItemContainerOnAddItem;
            container.onRemoveItem += ItemContainerOnRemoveItem;
            StopMotor();
            InvokeRepeating("GetLog", 1, 3);
            var currentBiome = GetBiome();
            var data = GetBiomeData(currentBiome);
            powerPerSecond = data.PowerPerSecond;
            AISolutionsData.OnRotationChanged += AiSolutionsDataOnOnRotationChanged;
        }

        private void AiSolutionsDataOnOnRotationChanged(Quaternion axis)
        {
            _targetRotation = axis;
            _rotate = true;
        }


        private void Update()
        {
            RotateRotor();
            var constructable = GetComponentInParent<Constructable>();

            if (!_initialized && constructable._constructed && transform.parent != null)
            {
                Initialize();
            }

            if (!_initialized || !constructable._constructed)
            {
                //Log.Info("Not Constructed");
            }

            passedTime += Time.deltaTime;

            if (passedTime >= 0.25f * _totalDays)
            {
                ApplyDamage();
            }

            AISolutionsData.UpdateTime();

            float requested = powerPerSecond * DayNightCycle.main.deltaTime;
            float num = Capacity - Charge;
            if (num <= 0.0)
                return;
            if (num < requested)
                requested = num;
            float amount = ProducePower(requested);
        }

        private void RotateRotor()
        {
            if (_rotate)
            {
                transform.Find("model").Find("Rotor").rotation = Quaternion.Lerp(transform.Find("model").Find("Rotor").rotation, _targetRotation, 1 * DayNightCycle.main.deltaTime);
                if (transform.Find("model").Find("Rotor").rotation.y >= _targetRotation.y)
                {
                    _rotate = false;
                    Log.Info("Done Rotating");
                }
            }


        }

        #endregion

        private void Initialize()
        {
            InvokeRepeating("UpdatePowerRelay", 0, 1);
            _initialized = true;
        }

        private void UpdatePowerRelay()
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
            }
            else
            {
                _powerRelay = null;
            }

            if (_powerRelay != null)
            {
                _powerRelay.RemoveInboundPower(this);
                _powerRelay.AddInboundPower(this);
            }

        }

        private void GetLog()
        {
            //Log.Info($"// ============================= Turbine - {_id} ============================= //");

            //Log.Info("Health");
            //Log.Info($"{Health}%");

            //Log.Info("Passed Time");
            //Log.Info($"{passedTime}%");

            //Log.Info($"// ============================= Turbine - {_id} ============================= //");

            //Log.Info($"Current biome: {Player.main.GetBiomeString()}");
            //Log.Info($"Charge: {_charge} || Capacity: {Capacity}");
        }

        private void StopMotor()
        {
            _hinge = transform?.GetComponentInChildren<HingeJoint>();

            if (_hinge == null)
            {
                Log.Error("Couldn't find motor hinge");
            }
            else
            {
                _hinge.useMotor = true;
                _motor = _hinge.motor;
                _motor.force = 0;
                _motor.targetVelocity = 0;
                _motor.freeSpin = false;
                _hinge.motor = _motor;
            }
        }

        private void ItemContainerOnRemoveItem(InventoryItem item)
        {

        }

        private bool ItemContainerIsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            var flag = false;

            if (pickupable != null && Health < MaxHealth)
            {
                var techType = pickupable.GetTechType();

                if (techType == TechType.Titanium)
                    flag = true;
            }

            if (Health >= MaxHealth)
            {
                ErrorMessage.AddMessage("Turbine not Damaged");
            }

            if (!flag && verbose)
            {
                ErrorMessage.AddMessage("Only Titanium allowed");
            }

            return flag;
        }

        private void Start()
        {
            //Use this to do the Emission
            Shader shader = Shader.Find("MarmosetUBER");
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Log.Info($"Render: {renderer.name} || Material Count {renderer.materials.Length}");

                if (renderer.materials.Length == 2)
                {
                    Log.Info($"Material #1: {renderer.materials[0]}");
                    Log.Info($"Material #2: {renderer.materials[1]}");
                }

                foreach (Material material in renderer.materials)
                {
                    material.shader = shader;
                    Log.Info(material.name);
                    //I am using StartsWith because the material name contains (Instance)
                    if (material.name.StartsWith("FCS_MarineTurbine_Tex"))
                    {
                        Log.Info("FoundMat");
                        material.EnableKeyword("MARMO_SPECMAP");
                        //material.EnableKeyword("_ZWRITE_ON");
                        material.EnableKeyword("_EMISSION");
                        material.EnableKeyword("MARMO_EMISSION");
                        material.EnableKeyword("_METALLICGLOSSMAP");

                        //material.SetColor("_GlowColor", Color.blue);
                        //material.SetFloat("_GlowStrength", 1f);
                        //material.SetFloat("_EmissionLM", 0f);
                        material.SetVector("_EmissionColor", new Color(0f, 1.437931f, 1.5f, 1.0f) * 1.0f);
                        material.SetTexture("_Illum", FindTexture2D("JetStreamT242_MarineTurbineMat_Emission"));
                        material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                        //material.SetFloat("_EnableGlow", 1f);
                        //material.SetFloat("_EMISSION", 0.1f);

                        material.SetFloat("_Fresnel", 0f);
                        material.SetColor("_Color", Color.white);
                        material.SetTexture("_MetallicGlossMap", FindTexture2D("JetStreamT242_MarineTurbineMat_MetallicSmoothness"));
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


            //transform.eulerAngles = AISolutionsData.GetStartingRotation(transform);

        }

        private float ProducePower(float requested)
        {
            if (HasBreakerTripped)
            {
                Charge = 0.0f;
                return 0.0f;
            }
            float num1 = 0.0f;
            if (requested > 0.0)
            {
                Charge += requested;
                num1 = requested;
                int num2 = 0;
                var num3 = 20f;
                if (Charge < num3)
                {
                    ++num2;
                }
                else
                {
                    Charge -= num3;
                }

                if (num2 == 0)
                {
                    num1 -= this.Charge;
                    Charge = 0.0f;
                }
            }
            return num1;
        }

        public float GetDepth()
        {
            return Ocean.main.GetDepthOf(gameObject);
        }

        public static Texture2D FindTexture2D(string textureName)
        {
            List<object> objects = new List<object>(LoadItems.ASSETBUNDLE.LoadAllAssets(typeof(object)));

            Log.Info("FindTexture2D");

            for (int i = 0; i < objects.Count; i++)
            {

                Log.Info($"Asset Found {objects[i]}");
                if (objects[i] is Texture2D)
                {
                    if (((Texture2D)objects[i]).name.Equals(textureName))
                    {
                        Log.Info($"Found : {textureName} in Asset Bundle");
                        Log.Info(((Texture2D)objects[i]).NullOrID());
                        return ((Texture2D)objects[i]);
                    }
                }
            }

            return null;
        }

        public static AISolutionsData.BiomeItem GetBiomeData(string biome)
        {
            return Data.AISolutionsData.BiomeCurrent.GetOrDefault(biome, new Data.AISolutionsData.BiomeItem { PowerPerSecond = 4.166667f, Speed = 90 });
        }

        public void ChangeMotorSpeed()
        {
            var currentBiome = GetBiome();

            var speed = GetBiomeData(currentBiome);

            //Log.Info("Use Motor");
            _hinge.useMotor = true;

            //Log.Info("Use Force");
            _motor.force = 1;

            //Log.Info("Use Velocity");
            //var velocity = MinSpeed + (GetDepth() * multiplier);

            //Log.Info("Use Calculator");
            //var calc = Mathf.Clamp(velocity, MinSpeed, MaxSpeed);

            //Log.Info("Use Target Velocity");
            _motor.targetVelocity = speed.Speed;

            _motor.freeSpin = true;

            //Log.Info("Use Motor");
            _hinge.motor = _motor;


        }

        private string GetBiome()
        {
            var biome = string.Empty;
            var curBiome = Player.main.GetBiomeString().ToLower();

            if (curBiome.StartsWith("safe"))
            {
                biome = "safeshallows";
            }

            return biome;
        }

        public void ApplyDamage()
        {
            if (Health > 0)
            {
                Health -= HealthMultiplyer;
            }
        }

        public int GetSpeed()
        {
            return Convert.ToInt32(_hinge.velocity * _rpmPerDeg);
        }

        public void Heal()
        {
            if (_containerHasItem)
            {
                if (Health < MaxHealth)
                {
                    container.DestroyItem(TechType.Titanium);
                    Health += HealthMultiplyer;
                    ResetTimer();
                    ErrorMessage.AddMessage($"Turbine health = {Health}");
                }
            }
        }

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

        private void TurnDisplayOn()
        {
            //try
            //{
            if (IsBeingDeleted) return;

            if (_jetStreamT242Display != null)
            {
                Log.Info("Turnoff");

                TurnDisplayOff();
            }

            Log.Info("JetStreamT242 Add Component");

            _jetStreamT242Display = gameObject.AddComponent<JetStreamT242Display>();

            Log.Info("JetStreamT242 Setup");

            _jetStreamT242Display.Setup(this);

            //}
            //catch (Exception e)
            //{
            //    Log.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            //}
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

        private void ItemContainerOnAddItem(InventoryItem item)
        {
            Heal();
        }

        public bool CanDeconstruct(out string reason)
        {
            reason = string.Empty;
            return true;
        }

        public void OnConstructedChanged(bool constructed)
        {
            Log.Info($"Constructed - {constructed}");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                if (_isEnabled == false)
                {
                    _isEnabled = true;
                    ChangeMotorSpeed();
                    StartCoroutine(Startup());
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

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {

        }

        public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
        {

        }

        public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
        {

        }

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
            return Capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;

            bool result;
            if (amount >= 0f)
            {
                result = (amount <= Capacity - Charge);
                modified = Mathf.Min(amount, Capacity - Charge);
                Charge += modified;
            }
            else
            {
                result = (Charge >= -amount);
                if (GameModeUtils.RequiresPower())
                {
                    modified = -Mathf.Min(-amount, Charge);
                    Charge += modified;
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

        private void KillBattery()
        {
            AISolutionsData.OnRotationChanged -= AiSolutionsDataOnOnRotationChanged;
            Capacity = Charge = 0;
        }

        private void OnDestroy()
        {
            KillBattery();

            if (_powerRelay != null)
            {
                _powerRelay.RemoveInboundPower(this);
            }

            Destroy(gameObject);
        }

        private void ResetTimer()
        {
            passedTime = 0;
        }

    }
}