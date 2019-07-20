using FCSAlterraIndustrialSolutions.Configuration;
using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Components;
using FCSAlterraIndustrialSolutions.Models.Controllers.Logic;
using FCSAlterraIndustrialSolutions.Models.Enums;
using FCSAlterraIndustrialSolutions.Utilities;
using FCSCommon.Components;
using FCSCommon.Controllers;
using FCSCommon.Extensions;
using FCSCommon.Helpers;
using FCSCommon.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonItemsContainer = FCSAlterraIndustrialSolutions.Models.Components.CommonItemsContainer;
using LogType = Utilites.Logger.LogType;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    /// <summary>
    /// This controller controls all functions of the Deep Driller
    /// It automatically adds the <see cref="PowerController"/> to the game object so you will NOT have to do this manually
    /// </summary>
    [RequireComponent(typeof(WeldablePoint))]
    public class DeepDrillerController : MonoBehaviour, IConstructable, IProtoEventListener
    {
        #region Private Members
        private float _currentSpeed;
        private GameObject _drillShaft;
        private float _passedTime;
        private const float _damageTimeInSeconds = 2520;
        private HealthController _healthSystem;
        private DeepDrillerSaveDataEntry saveData;
        private bool _foundComponents;
        private GameObject _model;
        private readonly GameObject _storageContainer1GO;
        private readonly CommonItemsContainer _storageContainer1;
        private ItemsContainer _itemContainer;
        private Animator _animation;
        private bool _armsDropped;
        private bool _constructed;
        private bool _isEnabled;
        private GameObject _seaBase;
        private DeepDrillerDisplay _deepDrillerDisplay;
        private GameObject _digSite;
        private PowerController _powerSystem;
        private bool _startDrill;
        private OreGenerator _oreGenerator;
        private string _currentBiome;
        private GameObject _damage;
        private ItemsContainer _moduleContainer;
        private GameObject _modules;
        private GameObject _battery_Module;
        private GameObject _solar_Panel_Module;
        private PowerSource _modulePower;
        private bool searching;
        private PowerRelay powerRelay;
        private static readonly float connectionDistance = 100f;
        private readonly float _powerPerSecond = 0.2f;
        private readonly float _updateInterval = 3f;
        private PowerFX powerFX;
        private ShaftStates _shaftState = ShaftStates.Up;
        private ShaftStates _prevShaftState = ShaftStates.Up;
        private DrillState _drillState = DrillState.Idle;
        private PowerState _powerState;

        #endregion

        #region Public Properties
        public float IncreaseRate { get; set; } = 20f;
        public bool Increasing { get; set; }
        public StorageContainer Container { get; set; }
        public bool IsBeingDeleted { get; set; }
        public bool HasBreakerTripped { get; set; }
        public PowerState PowerState
        {

            get => _powerState;
            set
            {
                _powerState = value;
                if (gameObject != null)
                {
                    ChangeSystemLights(value, gameObject);
                }
            }
        }
        #endregion

        #region Unity Methods

        private void Awake()
        {
            //InvokeRepeating("Test", 0 , 3);

            var unqiueLiveMixingData = new Data.AISolutionsData.CustomLiveMixinData.UniqueLiveMixinData();
            var liveMixinData = unqiueLiveMixingData.Create(100, true, true, true);

            powerFX = gameObject.GetComponent<PowerFX>();
            powerFX.vfxPrefab = LoadItems.XPowerConnectionPrefab;
            _healthSystem = gameObject.AddComponent<HealthController>();

            _oreGenerator = gameObject.GetOrAddComponent<OreGenerator>();
            _oreGenerator.Start(1, 2);
            _oreGenerator.OnAddCreated += OreGeneratorOnOnAddCreated;

            var liveMixin = gameObject.GetComponentInParent<LiveMixin>();

            _healthSystem.Startup(liveMixin, liveMixinData);
            _healthSystem.FullHealth();

            _powerSystem = gameObject.AddComponent<PowerController>();
            _powerSystem.CreateBattery(100, 2000);
            
            _damageMaterialController = gameObject.GetComponent<DamagedMaterialController>();


            if (FindAllComponents() == false)
            {
                _foundComponents = false;
                Log.Error("// ============== Error getting all Components ============== //");
                return;
            }

            CreateContainers();

            ApplyShaders();

            /*
             *TODO: Move Drop Arm Function and Hide Modules
             * Move Drop Arms and HideModules to the GetGameObject Function to prevent when Shaders are
             * missing the object wont build broken
             */

            DropArms();

            HideModules();

        }

        private void Test()
        {
            PowerCable[] submarines = FindObjectsOfType<PowerCable>();
            Log.Info("Found " + submarines.Length + " Power Plugs");
            foreach (var s in submarines)
            {
                Log.Info(s.name);
            }
        }

        private void HideModules()
        {
            _battery_Module.SetActive(false);
            _solar_Panel_Module.SetActive(false);
        }

        private void Update()
        {
            SystemHandler();

            //Log.Info(SunOrbit.main.transform.rotation.ToString());
        }

        private void Start()
        {
            InvokeRepeating("FindNearestValidRelay", 0.0f, 2f);
            InvokeRepeating("ApplyTestDamage", 0, 5f);
            InvokeRepeating("UpdatePower", 0.0f, _updateInterval);
            PowerState = PowerState.Off;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Drops the Driller Arms to the ground
        /// </summary>
        public void DropArms()
        {
            Log.Info("Dropping arms");
            StartCoroutine(IEDropArms());
        }

        public void StartDrill()
        {
            if (_drillState == DrillState.Running) return;
            _oreGenerator.AllowTick = true;
            _startDrill = true;
            Increasing = true;
            Log.Info($"Starting Drill: Increasing = {Increasing}");
            _shaftState = ShaftStates.Down;
            _drillState = DrillState.Running;
            //PowerState = PowerState.On;
            //ChangeSystemLights(PowerState.On, gameObject);
        }

        public void RaiseDrill()
        {
            StartCoroutine(IERaiseDrill());
        }

        public void StopDrill()
        {
            if (_drillState == DrillState.Idle) return;

            _oreGenerator.AllowTick = false;
            //_startDrill = false;
            Increasing = false;
            Log.Info($"Stopping Drill: Increasing = {Increasing}");
            _shaftState = ShaftStates.Up;
            _drillState = DrillState.Idle;
            //PowerState = PowerState.Off;

            //ChangeSystemLights(PowerState.Off, gameObject);
        }

        private void ChangeSystemLights(PowerState powerState, GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

            Log.Info($"Renders Count {renderers.Length} || GameObject Name: {gameObject.name}");

            foreach (var renderer in renderers)
            {

                foreach (var material in renderer.materials)
                {
                    Log.Info($"Material Name: {material.name}");
                    switch (powerState)
                    {
                        case PowerState.None:
                            break;
                        case PowerState.On:
                            if (!material.name.StartsWith("SystemLights")) break;
                            material.SetVector("_EmissionColor", Colors.SystemOnColor * 1.0f);
                            material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("SystemLights_OnMode_Emissive", LoadItems.GlobalBundle));
                            material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                            //renderer.material.SetTexture("_EmissionMap", MaterialHelpers.FindTexture2D("SystemLights_OnMode_Emissive", LoadItems.ASSETBUNDLE));

                            break;
                        case PowerState.Off:
                            if (!material.name.StartsWith("SystemLights")) break;
                            material.SetVector("_EmissionColor", Colors.SystemOffColor * 1.0f);
                            material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("SystemLights_OffMode_Emissive", LoadItems.GlobalBundle));
                            material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));

                            //renderer.material.SetTexture("_EmissionMap", MaterialHelpers.FindTexture2D("SystemLights_OffMode_Emissive", LoadItems.ASSETBUNDLE));

                            break;
                        case PowerState.StandBy:
                            if (!material.name.StartsWith("SystemLights")) break;
                            material.SetVector("_EmissionColor", Colors.SystemChargeColor * 1.0f);
                            material.SetTexture("_Illum", MaterialHelpers.FindTexture2D("SystemLights_ChargeMode_Emissive", LoadItems.GlobalBundle));
                            material.SetVector("_Illum_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                            //renderer.material.SetTexture("_EmissionMap", MaterialHelpers.FindTexture2D("SystemLights_ChargeMode_Emissive", LoadItems.ASSETBUNDLE));

                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(powerState), powerState, null);
                    }
                }


            }
            //renderer.material.shader = marmosetUber;

        }


        public void PowerOffDrill()
        {
            if (!_powerAvaliable || _healthSystem.GetHealth() <= 0) return;
            PowerState = PowerState.Off;
            StopDrill();
        }

        public void PowerOnDrill()
        {
            if (!_powerAvaliable || _healthSystem.GetHealth() <= 0) return;
            PowerState = PowerState.On;
            StartDrill();
        }

        public void SoundSystem(bool play)
        {

        }

        public void Save(DeepDrillerSaveData saveDataList)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier.Id;

            if (saveData == null)
            {
                saveData = new DeepDrillerSaveDataEntry() { Id = id };
            }

            saveData.Id = id;
            saveDataList.Entries.Add(saveData);
        }

        public void ApplyDamage()
        {
            if (_healthSystem.GetHealth() > 0 && LoadItems.DeepDrillConfig.EnableWear)
            {
                _healthSystem.ApplyDamage(10);
            }
            ResetTimer();
        }

        public void OpenStorage()
        {
            var pda = Player.main.GetPDA();
            Inventory.main.SetUsedStorage(_itemContainer);
            pda.Open(PDATab.Inventory, gameObject.transform, null, 10f);
        }

        public void OpenModulesDoor()
        {
            var pda = Player.main.GetPDA();
            Inventory.main.SetUsedStorage(_moduleContainer);
            pda.Open(PDATab.Inventory, gameObject.transform, null, 10f);
        }

        #endregion

        #region Private Methods

        private void OreGeneratorOnOnAddCreated(string type)
        {
            Log.Info($"In OreGeneratorOnOnAddCreated {type}");
            _itemContainer.AddItem(type.ToPickupable());

            //Log.Info($"// ====================================================== Random TechType = {type.ToString()} ==================================== //");
        }

        private void ApplyTestDamage()
        {
            Log.Info($"Health = {_healthSystem.GetHealth()}");

            _healthSystem.ApplyDamage(1);
        }

        private void CreateContainers()
        {
            _itemContainer = new ItemsContainer(6, 6, gameObject.transform, $"{Information.DeepDrillerFriendly}", null);
            _itemContainer.isAllowedToAdd += IsAllowedToAdd;
            _itemContainer.onRemoveItem += ItemContainerOnOnRemoveItem;

            _moduleContainer = new ItemsContainer(1, 1, gameObject.transform, "Modules", null);
            _moduleContainer.isAllowedToAdd += IsAllowedToAddModules;
            _moduleContainer.onRemoveItem += ModuleContainerOnRemove;
            _moduleContainer.onAddItem += ModuleContainerOnAddItem;

            #region Omit
            //var containerPrefab = GameObject.Instantiate(LoadItems.AIItemContainerPrefab);
            //containerPrefab.transform.SetParent(gameObject.transform, false);
            //containerPrefab.name = $"{Information.DeepDrillerName}S1";

            //_storageContainer1 = containerPrefab.AddComponent<CommonItemsContainer>();
            //_storageContainer1.Controller = this;
            //_storageContainer1.transform.localPosition = new Vector3(0.0226624f, 3.9232f, 1f);
            //_storageContainer1.transform.Rotate(new Vector3(90, 180, 0));
            //_storageContainer1.CreateContainer(6, 6, gameObject.transform, $"{Information.DeepDrillerFriendly}");



            //_storageContainer1.Transform = gameObject.transform;

            //_storageContainer1.ItemsContainer = itemContainer;



            //_storageContainer1 = _storageContainer1GO.AddComponent<CommonItemsContainer>();
            //_storageContainer1.CreateContainer(6,6,transform,$"{Information.DeepDrillerFriendly}"); 
            #endregion
        }

        private void ModuleContainerOnAddItem(InventoryItem item)
        {
            Log.Info($"ModuleContainerOnAddItem Item Name {item.item.name}");
            switch (item.item.name.RemoveClone())
            {
                case "AIDeepDrillerBattery":
                    //_modulePower = item.item.GetComponent<InternalBatteryController>();
                    //_battery_Module.SetActive(true);
                    //_solar_Panel_Module.SetActive(false);
                    //_moduleInserted = true;
                    break;
                case "AIDeepDrillerSolar":
                    _modulePower = item.item.GetComponent<PowerSource>();
                    var module = item.item.GetComponent<AIDeepDrillerSolarController>();
                    module.DeepDrillerObject = gameObject;
                    _battery_Module.SetActive(false);
                    _solar_Panel_Module.SetActive(true);
                    _moduleInserted = true;

                    break;
            }
        }

        private void UsePower()
        {

        }

        private void ModuleContainerOnRemove(InventoryItem item)
        {
            switch (item.item.name.RemoveClone())
            {
                case "AIDeepDrillerBattery":
                    _battery_Module.SetActive(false);
                    _moduleInserted = false;
                    break;
                case "AIDeepDrillerSolar":
                    _solar_Panel_Module.SetActive(false);
                    _moduleInserted = false;
                    break;
            }
        }

        private bool IsAllowedToAddModules(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                if (LoadItems.DeepDrillerAllowedModules.Contains(techType))
                    flag = true;
            }
            if (!flag && verbose)
                Log.Info(LoadItems.DeepDrillerModStrings.ModulesItemNotAllowed, LogType.PlayerScreen);

            return flag;
        }

        private void ItemContainerOnOnRemoveItem(InventoryItem item)
        {
            //TODO If container has space start operation
        }

        private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
        {
            bool flag = false;
            if (pickupable != null)
            {
                TechType techType = pickupable.GetTechType();
                if (AllowedResources.Contains(techType))
                    flag = true;
            }
            if (!flag && verbose)
                Log.Info(LoadItems.DeepDrillerModStrings.ItemNotAllowed, LogType.PlayerScreen);

            return flag;
        }

        private void ChangeDrillSpeed(float speed)
        {
            // increase or decrease the current speed depending on the value of increasing
            //Log.Info($"In Change Speed: Current Speed= {_currentSpeed} || IncreaseRate = {IncreaseRate} || Speed = {speed} || Increasing {Increasing}");
            _currentSpeed = Mathf.Clamp(_currentSpeed + DayNightCycle.main.deltaTime * IncreaseRate * (Increasing ? 1 : -1), 0, speed);
            _drillShaft.transform.Rotate(Vector3.up, _currentSpeed * DayNightCycle.main.deltaTime);
        }

        private List<string> GetBiomeData(string biome)
        {
            foreach (var biomeOre in LoadItems.DeepDrillConfig.BiomeOres)
            {
                if (biomeOre.Key == biome.ToLower())
                {
                    return biomeOre.Value;
                }
            }

            return null;
        }

        private void UpdateDamageMesh()
        {
            _damage.SetActive(_healthSystem.GetHealth() <= 0);

            if (_healthSystem.GetHealth() <= 0)
            {
                _drillBit.SetActive(false);
                _damageDrillBit.SetActive(true);
            }
            else if (_healthSystem.GetHealth() > 0)
            {
                _drillBit.SetActive(true);
                _damageDrillBit.SetActive(false);
            }
        }

        private void SystemHandler()
        {
            /*
             * Handles starting and stopping the turbine and its rotor based off conditions
             */
            if (_constructed)
            {
                UpdateShaftState();

                if (_modulePower != null)
                {
                    UsePower();
                }

                UpdateDamageMesh();

                UpdateDamageSystem();

                //Log.Info($"HasBreakerTripper = {HasBreakerTripped} || IsUnderWater = {IsUnderWater()} || Health = {_healthSystem.GetHealth()}");

                if (HasBreakerTripped || !_powerAvaliable || !IsUnderWater() || _healthSystem.GetHealth() <= 0)
                {
                    //Log.Info("No Power");
                    ChangeDrillSpeed(300);

                    _oreGenerator.AllowTick = false;

                    //ChangeSystemLights(PowerState.Off, gameObject);
                }
                else
                {
                    if (_startDrill)
                    {
                        if (!string.IsNullOrEmpty(_currentBiome))
                        {
                            _oreGenerator.AllowedOres = GetBiomeData(_currentBiome);
                            _oreGenerator.AllowTick = true;
                            //Log.Info("Powered");

                            //Log.Info($"Remaining Time: {_oreGenerator.TimeRemaining}");
                        }
                        ChangeDrillSpeed(300);
                        //StartDrill();
                    }
                }

                if (_healthSystem.GetHealth() <= 0 && !_powerSystem.IsBatteryDistroyed)
                {
                    _powerSystem.KillBattery();
                    _oreGenerator.AllowTick = false;
                    StopDrill();
                    _shaftState = ShaftStates.Up;
                    PowerState = PowerState.Off;
                    //ChangeSystemLights(PowerState.StandBy, gameObject);
                }
                else if (_healthSystem.GetHealth() > 0 && _powerSystem.IsBatteryDistroyed)
                {
                    _powerSystem.RestoreBattery(false);
                    _oreGenerator.AllowTick = false;
                    if (!HasBreakerTripped)
                    {
                        StartDrill();
                        _shaftState = ShaftStates.Down;
                        PowerState = PowerState.On;
                    }
                }
            }
        }

        private void UpdateShaftState()
        {
            if (_shaftState == ShaftStates.Down && _prevShaftState == ShaftStates.Up)
            {
                StartCoroutine(IEStartDrill());
                _prevShaftState = ShaftStates.Down;
            }

            if (_shaftState == ShaftStates.Up && _prevShaftState == ShaftStates.Down)
            {
                StartCoroutine(IEStopDrill());
                _prevShaftState = ShaftStates.Up;
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

            foreach (var biomeItem in LoadItems.DeepDrillConfig.BiomeOres)
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
            return WorldHelpers.GetDepth(gameObject) >= 7.0f;
        }

        private bool FindAllComponents()
        {

            #region Model

            _model = transform.Find("model")?.gameObject;

            if (_model == null)
            {
                Log.Error("Model not found");
                return false;
            }
            #endregion

            #region Modules

            _modules = _model.FindChild("modules")?.gameObject;

            if (_modules == null)
            {
                Log.Error("Modules not found");
                return false;
            }

            #endregion

            #region Battery Module
            _battery_Module = _modules.FindChild("battery_module")?.gameObject;

            if (_battery_Module == null)
            {
                Log.Error("Battery Module not found");
                return false;
            }
            #endregion

            #region Solar Module
            _solar_Panel_Module = _modules.FindChild("solar_panel_module")?.gameObject;

            if (_solar_Panel_Module == null)
            {
                Log.Error("Solar Panel Module not found");
                return false;
            }
            #endregion

            #region Drill Shaft
            _drillShaft = _model.FindChild("drill_machine").FindChild("drill_rod_1").FindChild("drill_rod_2")?.gameObject;

            if (_drillShaft == null)
            {
                Log.Error($"GameObject: Drill Rod 1 not found!");
                return false;
            }
            #endregion

            #region digSite
            _digSite = _model.FindChild("ExtraModels").FindChild("DeepDriller_DigState")?.gameObject;

            if (_digSite == null)
            {
                Log.Error($"GameObject: Drill site not found!");
                return false;
            }

            #endregion

            #region Damage

            _damage = _model.FindChild("DeepDriller_Damage").FindChild("Damage_Decals_2")?.gameObject;

            if (_damage == null)
            {
                Log.Error("Damage not found");
                return false;
            }

            #endregion

            #region Damage Drill Bit

            _damageDrillBit = _drillShaft.FindChild("drill_bit_damage")?.gameObject;

            if (_damageDrillBit == null)
            {
                Log.Error("Damage Drill Bit not found");
                return false;
            }

            #endregion

            #region Drill Bit

            _drillBit = _drillShaft.FindChild("drill_bit_default")?.gameObject;

            if (_damageDrillBit == null)
            {
                Log.Error("Drill Bit not found");
                return false;
            }

            #endregion

            #region Animation

            _animation = gameObject.GetComponentInChildren<Animator>();
            if (_animation == null)
            {
                Log.Error("Animator not found.");
                return false;
            }
            #endregion

            return true;
        }

        private void UpdateDamageSystem()
        {
            _passedTime += DayNightCycle.main.deltaTime;

            if (_passedTime >= _damageTimeInSeconds)
            {
                ApplyDamage();
            }
        }

        private void ResetTimer()
        {
            _passedTime = 0;
        }

        private void ApplyShaders()
        {
            #region DeepDriller_DefaultState

            MaterialHelpers.ApplySpecShader("DeepDrillerDefaultState", "DeepDrillerDefault_Spec", transform, Color.white, 0.1f, 6.5f, LoadItems.ASSETBUNDLE);
            MaterialHelpers.ApplyNormalShader("DeepDrillerDefaultState", "DeepDrillerNorm", transform, LoadItems.ASSETBUNDLE);

            #endregion

            #region DeepDrillerModules

            Log.Info("Deep Driller", LogType.PlayerScreen);

            if (LoadItems.GlobalBundle == null)
            {
                Log.Error("GlobalBundle is null", LogType.PlayerScreen);
            }

            MaterialHelpers.ApplySpecShader("DeepDrillerModules", "DeepDrillerModules_Spec", transform, Color.white, 0.1f, 6.5f, LoadItems.GlobalBundle);

            MaterialHelpers.ApplyNormalShader("DeepDrillerModules", "DeepModuleNorm", transform, LoadItems.GlobalBundle);

            #endregion

            #region DeepDriller_DigState
            MaterialHelpers.ApplyEmissionShader("DeepDriller_DigState", "DeepDriller_DigStateEmissive", transform, LoadItems.ASSETBUNDLE, new Color(1f, 1f, 1f, 1f));
            #endregion

            #region DeepDriller_GlobalDecals

            MaterialHelpers.ApplyAlphaShader("FCS_SUBMods_GlobalDecals", transform);
            MaterialHelpers.ApplyEmissionShader("FCS_SUBMods_GlobalDecals", "FCS_SUBMods_GlobalDecals_Emissive", transform, LoadItems.GlobalBundle, Color.white);

            #endregion

            #region System Lights
            MaterialHelpers.ApplyEmissionShader("SystemLights_ChargeState", "SystemLights_ChargeMode_Emissive", transform, LoadItems.GlobalBundle, new Color(0.8627452f, 0.6784314f, 0.3098039f));
            MaterialHelpers.ApplyNormalShader("SystemLights_ChargeState", "SystemLights_Norm", transform, LoadItems.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_ChargeState", transform);

            MaterialHelpers.ApplyEmissionShader("SystemLights_OffState", "SystemLights_OffMode_Emissive", transform, LoadItems.GlobalBundle, new Color(1f, 0.07843138f, 0.07843138f));
            MaterialHelpers.ApplyNormalShader("SystemLights_OffState", "SystemLights_Norm", transform, LoadItems.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_OffState", transform);

            MaterialHelpers.ApplyEmissionShader("SystemLights_OnState", "SystemLights_OnMode_Emissive", transform, LoadItems.GlobalBundle, new Color(0.08235294f, 1f, 1f));
            MaterialHelpers.ApplyNormalShader("SystemLights_OnState", "SystemLights_Norm", transform, LoadItems.GlobalBundle);
            MaterialHelpers.ApplyAlphaShader("SystemLights_OnState", transform);
            #endregion
        }

        private void ResetAnimation() //TODO Prevent the coroutine from being called ever frame
        {

            //Log.Info($"Resetting {Information.DeepDrillerFriendly} Animation");
            _animation.SetBool("Extend", false);
            _animation.SetBool("DropShaft", false);
            _animation.SetBool("RaiseShaft", false);
            _animation.SetBool("Reset", false);
        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;

                if (_deepDrillerDisplay != null)
                {
                    Log.Info("Turnoff");

                    TurnDisplayOff();
                }

                _deepDrillerDisplay = gameObject.AddComponent<DeepDrillerDisplay>();
                _deepDrillerDisplay.Setup(this);

            }
            catch (Exception e)
            {
                Log.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (_deepDrillerDisplay != null)
            {
                _deepDrillerDisplay.TurnDisplayOff();
                Destroy(_deepDrillerDisplay);
                _deepDrillerDisplay = null;
            }
        }

        private static PowerRelay GetNearestValidRelay(GameObject fromObject)
        {

            PowerRelay powerRelay = null;
            float num = 1000f;
            for (int index = 0; index < PowerRelay.relayList.Count; ++index)
            {

                PowerRelay relay = PowerRelay.relayList[index];

                //Log.Info($"Relay Name: {relay.name}");
                //Log.Info($"Relay IsActive: {relay.gameObject.activeInHierarchy}");
                //Log.Info($"Relay Enabled: {relay.enabled}");
                //Log.Info($"Relay DontConnectToRelays: {!relay.dontConnectToRelays}");
                //Log.Info($"Relay GetGhostModel doesn't Equal: {!Builder.GetGhostModel() == relay.gameObject}");


                if (relay is BasePowerRelay && relay.gameObject.activeInHierarchy && (relay.enabled && !relay.dontConnectToRelays) && !(Builder.GetGhostModel() == relay.gameObject))
                {
                    float magnitude = (relay.GetConnectPoint(fromObject.transform.position) - fromObject.transform.position).magnitude;
                    if ((double)magnitude <= connectionDistance && magnitude < (double)num)
                    {
                        num = magnitude;
                        powerRelay = relay;
                        Log.Info($"Relay Name: {relay.name}");
                    }
                }
            }
            return powerRelay;
        }

        private void FindNearestValidRelay()
        {
            //Log.Info("// ===================================== Find Power Relay =============================== //");

            PowerRelay nearestValidRelay = GetNearestValidRelay(this.gameObject);
            if (!nearestValidRelay)
                return;
            Log.Info("// ===================================== Power Relay FOund =============================== //");
            this.powerRelay = nearestValidRelay;
            Log.Info($"PowerRelay Name: {powerRelay.name}");
            if (powerRelay.gameObject == null)

            {
                Log.Info($"PowerRelay {powerRelay.name} gameObject is null");
            }
            this.powerFX.SetTarget(powerRelay.gameObject);
            Log.Info("// ===================================== powerFX Set =============================== //");

            this.searching = false;
            this.CancelInvoke(nameof(FindNearestValidRelay));
        }

        private void UpdatePower()
        {
            if (!_constructed)
                return;
            if (powerRelay)
            {
                Log.Info($"PowerRelay {powerRelay.GetPowerStatus()}");

                if (powerRelay.GetPowerStatus() == PowerSystem.Status.Normal)
                {
                    _powerAvaliable = true;
                    powerRelay.ConsumeEnergy(_powerPerSecond * _updateInterval, out var amountConsumed);
                }
                else
                {
                    _powerAvaliable = false;
                }
            }
            else if (!powerRelay && _moduleInserted == false)
            {
                _powerAvaliable = false;
                if (this.searching)
                    return;
                this.searching = true;
                this.InvokeRepeating("FindNearestValidRelay", 0.0f, 2f);
            }
            else if (!powerRelay && _moduleInserted)
            {
                Log.Info($"Solar Power = {_modulePower.power} || Solar Max Power = {_modulePower.maxPower}");

                if (_modulePower.power >= _powerPerSecond)
                {
                    _powerAvaliable = true;
                    powerRelay.ConsumeEnergy(_powerPerSecond * _updateInterval, out var amountConsumed);
                    Log.Info($"Power Test: Amount Consumed = {amountConsumed} || Solar Power = {_modulePower.power} || Solar Max Power = {_modulePower.maxPower}");

                }
                else
                {
                    _powerAvaliable = false;
                }
            }
            else
            {
                _powerAvaliable = false;
            }


        }

        #endregion

        #region IEnumerators

        private IEnumerator IEDropArms()
        {
            Log.Info("Starting Dropping arms");
            _animation.enabled = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(2f);

            _animation.SetBool("Extend", true);

            yield return new WaitForSeconds(6.666666666666667f);

            _armsDropped = true;

            ResetAnimation();
        }

        private IEnumerator IEStartDrill()
        {
            _animation.enabled = true;

            yield return new WaitForEndOfFrame();
            _animation.SetBool("DropShaft", true);
            yield return new WaitForSeconds(10f);

            //PowerState = PowerState.On;
            ResetAnimation();
        }

        private IEnumerator IEStopDrill()
        {
            _animation.enabled = true;

            yield return new WaitForEndOfFrame();
            _animation.SetBool("RaiseShaft", true);
            yield return new WaitForSeconds(10f);

            //PowerState = PowerState.Off;
            ResetAnimation();
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

        private IEnumerator IERaiseDrill()
        {
            _animation.enabled = true;
            yield return new WaitForEndOfFrame();
            _animation.SetBool("RaiseShaft", true);
            yield return new WaitForSeconds(10f);

            PowerState = PowerState.On;
            ResetAnimation();
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
            Log.Info("In Constructed Changed");

            if (IsBeingDeleted) return;

            if (constructed)
            {
                _constructed = true;
                if (_isEnabled == false)
                {
                    _isEnabled = true;
                    _currentBiome = GetBiome();
                    StartCoroutine(Startup());
                }
                else
                {
                    TurnDisplayOn();
                }
            }
            else
            {
                _constructed = false;
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
            if (!Mod.IsSaving())
            {
                Log.Info("Saving Drills");
                Mod.SaveDeepDriller();
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            var prefabIdentifier = GetComponent<PrefabIdentifier>();
            var id = prefabIdentifier?.Id ?? string.Empty;

            var data = Mod.GetDeepDrillerSaveData(id);
        }
        #endregion

        #region Enums
        public static List<TechType> AllowedResources = new List<TechType> {
            TechType.Copper,
            TechType.Gold,
            TechType.Lead,
            TechType.Lithium,
            TechType.Magnetite,
            TechType.Nickel,
            TechType.Silver,
            TechType.Titanium,
            TechType.AluminumOxide,
            TechType.Diamond,
            TechType.Kyanite,
            TechType.Quartz,
            TechType.UraniniteCrystal,
            TechType.Sulphur
        };

        private bool _powerAvaliable;
        private GameObject _damageDrillBit;
        private GameObject _drillBit;
        private DamagedMaterialController _damageMaterialController;
        private bool _moduleInserted;

        #endregion
    }
}
