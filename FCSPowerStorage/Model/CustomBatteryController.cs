using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using FCSPowerStorage.Model.Components;
using FCSPowerStorage.Utilities.Enums;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace FCSPowerStorage.Model
{
    /// <summary>
    /// The controller for the custom battery
    /// </summary>
    public class CustomBatteryController : MonoBehaviour, IPowerInterface, IProtoEventListener, IConstructable
    {
        #region Constructor
        /// <summary>
        /// Default Constructor 
        /// </summary>
        public CustomBatteryController()
        {
            MainBodyColor = new Color(0.99609375f, 0.99609375f, 0.99609375f);
        }
        #endregion

        #region Private Members

        private PowerRelay _connectedRelay;
        public GameObject seaBase { get; private set; }
        private FCSPowerStorageDisplay fcsPowerStorageDisplay;
        private bool _initialized;
        #endregion

        #region Public Properties
        /// <summary>
        /// The sound emitter for the charging sound
        /// </summary>
        /// 
        public FMOD_StudioEventEmitter SoundCharge;
        public float Charge { get; private set; }
        public bool IsBeingDeleted { get; set; }
        public bool HasBreakerTripped { get; set; }
        public float StoredPower { get; set; }
        public PowerToggleStates ChargeMode
        {
            get => _chargeMode;
            set
            {
                _chargeMode = value;
                _previousToggleState = value;
            }
        }
        public Color MainBodyColor { get; set; }
        public string ID { get; set; }

        #endregion

        #region Private Members
        private bool _isEnabled;
        private float _battery_Status_1_Bar;
        private string _battery_Status_1_Percentage;
        //private float _battery_Status_2_Bar;
        //private string _battery_Status_2_Percentage;
        //private float _battery_Status_3_Bar;
        //private string _battery_Status_3_Percentage;
        //private float _battery_Status_4_Bar;
        //private string _battery_Status_4_Percentage;
        //private float _battery_Status_5_Bar;
        //private string _battery_Status_5_Percentage;
        //private float _battery_Status_6_Bar;
        //private string _battery_Status_6_Percentage;
        private FCSPowerStates _previousPowerState = FCSPowerStates.Buffer;
        private PowerToggleStates _previousToggleState = PowerToggleStates.None;
        private PowerToggleStates _chargeMode;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            ID = GetComponentInParent<PrefabIdentifier>().Id;
        }

        private void Update()
        {
            var constructable = GetComponentInParent<Constructable>();

            if (!_initialized && constructable._constructed && transform.parent != null)
            {
                Initialize();
            }
        }

        #endregion

        #region Private Methods
        private void Initialize()
        {
            InvokeRepeating("UpdatePowerRelay", 0, 1);
            InvokeRepeating("Recharge", 0, 1);
            _initialized = true;
        }

        private void UpdatePowerRelay()
        {
            var relay = PowerSource.FindRelay(transform);

            if (relay != null && relay != _connectedRelay)
            {
                if (_connectedRelay != null)
                {
                    _connectedRelay.RemoveInboundPower(this);
                }
                _connectedRelay = relay;
                _connectedRelay.AddInboundPower(this);
                CancelInvoke("UpdatePowerRelay");
            }
            else
            {
                _connectedRelay = null;
            }

            if (_connectedRelay != null)
            {
                _connectedRelay.RemoveInboundPower(this);
                _connectedRelay.AddInboundPower(this);
                CancelInvoke("UpdatePowerRelay");
            }
        }

        private void Recharge()
        {
            if (!HasBreakerTripped)
            {
                if (ChargeMode == PowerToggleStates.TrickleMode)
                {
                    int num1 = 0;
                    bool flag = false;
                    float amount = 0.0f;
                    bool charging = false;
                    PowerRelay relay = PowerSource.FindRelay(transform);

                    if (relay != null)
                    {
                        if (Charge < LoadItems.BatteryConfiguration.Capacity)
                        {
                            ++num1;
                            float num2 = DayNightCycle.main.deltaTime * LoadItems.BatteryConfiguration.ChargeSpeed *
                                         LoadItems.BatteryConfiguration.Capacity;
                            if (Charge + num2 > LoadItems.BatteryConfiguration.Capacity)
                                num2 = LoadItems.BatteryConfiguration.Capacity - Charge;
                            amount += num2;
                        }

                        UWE.Utils.Assert(amount >= 0.0, "Charger must request positive amounts", this);
                        float amountConsumed = 0.0f;
                        if (amount > 0.0 && relay.GetPower() > amount)
                        {
                            flag = true;
                            relay.ConsumeEnergy(amount, out amountConsumed);
                        }


                        UWE.Utils.Assert(amountConsumed >= 0.0, "Charger must result in positive amounts", this);
                        if (amountConsumed > 0.0)
                        {
                            charging = true;
                            float num2 = amountConsumed / num1;

                            if (Charge < (double)LoadItems.BatteryConfiguration.Capacity)
                            {
                                float num3 = num2;
                                float num4 = LoadItems.BatteryConfiguration.Capacity - Charge;
                                if (num3 > (double)num4)
                                    num3 = num4;
                                Charge += num3;
                            }

                        }

                        //ToggleChargeSound(charging);
                    }
                }
                else
                {

                    int num1 = 0;
                    bool flag = false;
                    float amount = 0.0f;
                    bool charging = false;
                    PowerRelay relay = PowerSource.FindRelay(transform);

                    if (relay != null)
                    {
                        if (StoredPower < LoadItems.BatteryConfiguration.Capacity)
                        {
                            ++num1;
                            float num2 = DayNightCycle.main.deltaTime * LoadItems.BatteryConfiguration.ChargeSpeed *
                                         LoadItems.BatteryConfiguration.Capacity;
                            if (StoredPower + num2 > LoadItems.BatteryConfiguration.Capacity)
                                num2 = LoadItems.BatteryConfiguration.Capacity - Charge;
                            amount += num2;
                        }

                        UWE.Utils.Assert(amount >= 0.0, "Charger must request positive amounts", this);
                        float amountConsumed = 0.0f;
                        if (amount > 0.0 && relay.GetPower() > amount)
                        {
                            flag = true;
                            relay.ConsumeEnergy(amount, out amountConsumed);
                        }


                        UWE.Utils.Assert(amountConsumed >= 0.0, "Charger must result in positive amounts", this);
                        if (amountConsumed > 0.0)
                        {
                            charging = true;
                            float num2 = amountConsumed / num1;

                            if (StoredPower < (double)LoadItems.BatteryConfiguration.Capacity)
                            {
                                float num3 = num2;
                                float num4 = LoadItems.BatteryConfiguration.Capacity - StoredPower;
                                if (num3 > (double)num4)
                                    num3 = num4;
                                StoredPower += num3;
                            }

                        }

                        ToggleChargeSound(charging);
                    }
                }
            }
        }

        protected void ToggleChargeSound(bool charging)
        {
            if (!(SoundCharge != null))
                return;
            var startingOrPlaying = SoundCharge.GetIsStartingOrPlaying();
            if (charging)
            {
                QuickLogger.Debug("Charging Battery");
                if (startingOrPlaying)
                    return;
                SoundCharge.StartEvent();
            }
            else
            {
                QuickLogger.Debug("Not Charging Battery");

                if (!startingOrPlaying)
                    return;
                SoundCharge.Stop();
            }
        }

        private void UpdatePowerState()
        {

            // If in charge mode and we are online turn the lights orange
            if (HasBreakerTripped && ColorChanger.CurrentColor != ColorChanger.Red)
            {
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);
                return;
            }

            if (Charge >= 1 && ChargeMode == PowerToggleStates.ChargeMode && ColorChanger.CurrentColor != ColorChanger.Orange && !HasBreakerTripped)
            {
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
            }
            // If in charge mode and we are online turn the lights orange
            else if (Charge < 1 && ChargeMode == PowerToggleStates.ChargeMode && ColorChanger.CurrentColor != ColorChanger.Orange && !HasBreakerTripped)
            {
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
            }
            else if (Charge < 1 && _previousPowerState != FCSPowerStates.Unpowered)
            {
                _previousPowerState = FCSPowerStates.Unpowered;
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);
            }
            else if (Charge >= 1 && _previousPowerState != FCSPowerStates.Powered)
            {
                _previousPowerState = FCSPowerStates.Powered;
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Powered, transform.gameObject);
            }
        }

        private void OnDestroy()
        {
            CancelInvoke("UpdatePowerRelay");
            if (!(_connectedRelay != null))
                return;
            QuickLogger.Debug("RemoveInboundPower");
            _connectedRelay.RemoveInboundPower(this);
        }

        private void TurnDisplayOn()
        {
            try
            {
                if (IsBeingDeleted) return;

                if (fcsPowerStorageDisplay != null)
                {
                    TurnDisplayOff();
                }

                fcsPowerStorageDisplay = gameObject.AddComponent<FCSPowerStorageDisplay>();
                fcsPowerStorageDisplay.Setup(this);
                fcsPowerStorageDisplay.BatteryStatus1Bar.GetComponent<Image>().fillAmount = _battery_Status_1_Bar;
                //fcsPowerStorageDisplay.BatteryStatus2Bar.GetComponent<Image>().fillAmount = _battery_Status_2_Bar;
                //fcsPowerStorageDisplay.BatteryStatus3Bar.GetComponent<Image>().fillAmount = _battery_Status_3_Bar;
                //fcsPowerStorageDisplay.BatteryStatus4Bar.GetComponent<Image>().fillAmount = _battery_Status_4_Bar;
                //fcsPowerStorageDisplay.BatteryStatus5Bar.GetComponent<Image>().fillAmount = _battery_Status_5_Bar;
                //fcsPowerStorageDisplay.BatteryStatus6Bar.GetComponent<Image>().fillAmount = _battery_Status_6_Bar;
                fcsPowerStorageDisplay.BatteryStatus1Percentage.GetComponent<Text>().text = _battery_Status_1_Percentage.ToString(CultureInfo.InvariantCulture);
                //fcsPowerStorageDisplay.BatteryStatus2Percentage.GetComponent<Text>().text = _battery_Status_2_Percentage.ToString(CultureInfo.InvariantCulture);
                //fcsPowerStorageDisplay.BatteryStatus3Percentage.GetComponent<Text>().text = _battery_Status_3_Percentage.ToString(CultureInfo.InvariantCulture);
                //fcsPowerStorageDisplay.BatteryStatus4Percentage.GetComponent<Text>().text = _battery_Status_4_Percentage.ToString(CultureInfo.InvariantCulture);
                //fcsPowerStorageDisplay.BatteryStatus5Percentage.GetComponent<Text>().text = _battery_Status_5_Percentage.ToString(CultureInfo.InvariantCulture);
                //fcsPowerStorageDisplay.BatteryStatus6Percentage.GetComponent<Text>().text = _battery_Status_6_Percentage.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                QuickLogger.Error($"Error in TurnDisplayOn Method: {e.Message} || {e.InnerException} || {e.Source}");
            }
        }

        private void TurnDisplayOff()
        {
            if (IsBeingDeleted) return;

            if (fcsPowerStorageDisplay != null)
            {
                fcsPowerStorageDisplay.TurnDisplayOff();
                Destroy(fcsPowerStorageDisplay);
                fcsPowerStorageDisplay = null;
            }
        }

        private IEnumerator Startup()
        {
            if (IsBeingDeleted) yield break;
            yield return new WaitForEndOfFrame();
            if (IsBeingDeleted) yield break;

            seaBase = gameObject?.transform?.parent?.gameObject;
            if (seaBase == null)
            {
                ErrorMessage.AddMessage("[FCS Power Storage] ERROR: Can not work out what base it was placed inside.");
                QuickLogger.Error("ERROR: Can not work out what base it was placed inside.");
                yield break;
            }

            TurnDisplayOn();
        }
        #endregion

        #region Inherited Public Methods
        public float GetPower()
        {
            UpdatePowerState();
            if (Charge < 0.1)
            {
                Charge = 0.0f;
            }
            return Charge;
        }

        public float GetMaxPower()
        {
            return LoadItems.BatteryConfiguration.Capacity;
        }

        public bool ModifyPower(float amount, out float modified)
        {
            modified = 0f;

            var battery = this;

            bool result;
            if (amount >= 0f)
            {
                result = (amount <= LoadItems.BatteryConfiguration.Capacity - Charge);
                modified = Mathf.Min(amount, LoadItems.BatteryConfiguration.Capacity - Charge);
                Charge += modified;
            }
            else
            {
                result = (Charge >= -amount);
                if (GameModeUtils.RequiresPower())
                {
                    if (ChargeMode == PowerToggleStates.TrickleMode)
                    {
                        modified = -Mathf.Min(-amount, Charge);
                        Charge += modified;
                    }
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

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Get FCSPowerStorageDisplay");

            var activeDisplay = GetComponentInParent<FCSPowerStorageDisplay>();

            QuickLogger.Debug($"Found FCSPowerStorageDisplay");
            SaveData saveData = null;

            QuickLogger.Debug($"Create Save Data");


            saveData = new SaveData
            {
                Charge = Charge,
                StoredPower = StoredPower,
                BatteryStatus1Bar = activeDisplay.BatteryStatus1Bar.GetComponent<Image>().fillAmount,
                //BatteryStatus2Bar = activeDisplay.BatteryStatus2Bar.GetComponent<Image>().fillAmount,
                //BatteryStatus3Bar = activeDisplay.BatteryStatus3Bar.GetComponent<Image>().fillAmount,
                //BatteryStatus4Bar = activeDisplay.BatteryStatus4Bar.GetComponent<Image>().fillAmount,
                //BatteryStatus5Bar = activeDisplay.BatteryStatus5Bar.GetComponent<Image>().fillAmount,
                //BatteryStatus6Bar = activeDisplay.BatteryStatus6Bar.GetComponent<Image>().fillAmount,
                BatteryStatus1Percentage = activeDisplay.BatteryStatus1Percentage.GetComponent<Text>().text,
                //BatteryStatus2Percentage = activeDisplay.BatteryStatus2Percentage.GetComponent<Text>().text,
                //BatteryStatus3Percentage = activeDisplay.BatteryStatus3Percentage.GetComponent<Text>().text,
                //BatteryStatus4Percentage = activeDisplay.BatteryStatus4Percentage.GetComponent<Text>().text,
                //BatteryStatus5Percentage = activeDisplay.BatteryStatus5Percentage.GetComponent<Text>().text,
                //BatteryStatus6Percentage = activeDisplay.BatteryStatus6Percentage.GetComponent<Text>().text,
                HasBreakerTripped = HasBreakerTripped,
                ChargeMode = ChargeMode,
                BodyColor = MainBodyColor.ColorToVector4()
            };

            QuickLogger.Debug($"Save Data Created");

            var id = GetComponentInParent<PrefabIdentifier>();
            if (id != null)
            {
                QuickLogger.Debug($"Loading FCS Power Storage {id.Id}");

                string saveFolder = FilesHelper.GetSaveFolderPath();
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);
                //saveData
                string output = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                File.WriteAllText(Path.Combine(saveFolder, "fcspowerstorage_" + id.Id + ".json"), output);
                LoadItems.CleanOldSaveData();
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }
        }

        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            var id = GetComponentInParent<PrefabIdentifier>();
            if (id != null)
            {
                QuickLogger.Debug($"Loading FCS Power Storage {id.Id}");

                string filePath = Path.Combine(FilesHelper.GetSaveFolderPath(), "fcspowerstorage_" + id.Id + ".json");
                if (File.Exists(filePath))
                {
                    string savedDataJson = File.ReadAllText(filePath).Trim();

                    //LoadData
                    var savedData = JsonConvert.DeserializeObject<SaveData>(savedDataJson);

                    Charge = savedData.Charge;
                    _battery_Status_1_Bar = savedData.BatteryStatus1Bar;
                    //_battery_Status_2_Bar = savedData.BatteryStatus2Bar;
                    //_battery_Status_3_Bar = savedData.BatteryStatus3Bar;
                    //_battery_Status_4_Bar = savedData.BatteryStatus4Bar;
                    //_battery_Status_5_Bar = savedData.BatteryStatus5Bar;
                    //_battery_Status_6_Bar = savedData.BatteryStatus6Bar;
                    _battery_Status_1_Percentage = savedData.BatteryStatus1Percentage;
                    //_battery_Status_2_Percentage = savedData.BatteryStatus2Percentage;
                    //_battery_Status_3_Percentage = savedData.BatteryStatus3Percentage;
                    //_battery_Status_4_Percentage = savedData.BatteryStatus4Percentage;
                    //_battery_Status_5_Percentage = savedData.BatteryStatus5Percentage;
                    //_battery_Status_6_Percentage = savedData.BatteryStatus6Percentage;
                    HasBreakerTripped = savedData.HasBreakerTripped;
                    StoredPower = savedData.StoredPower;
                    ChargeMode = savedData.ChargeMode;
                    MainBodyColor = savedData.BodyColor.Vector4ToColor();
                    QuickLogger.Debug($"// =========================================================== {gameObject.name} =============================//");
                    ColorChanger.ApplyMaterials(gameObject, MainBodyColor);
                }
            }
            else
            {
                QuickLogger.Error("PrefabIdentifier is null");
            }

        }

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
                if (_isEnabled == false)
                {
                    _isEnabled = true;
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
        #endregion

        #region Public Methods

        /// <summary>
        /// Places the battery on charge mode
        /// </summary>
        /// <param name="chargeState"></param>
        public void ActivateChargeState(PowerToggleStates chargeState)
        {

            if (chargeState == PowerToggleStates.ChargeMode && _previousToggleState != chargeState)
            {
                QuickLogger.Debug("Activating Charge Mode");
                //_previousToggleState = chargeState;
                ChargeMode = chargeState;
                StoredPower = Charge;
                Charge = 0.0f;
            }

            if (chargeState == PowerToggleStates.TrickleMode && _previousToggleState != chargeState)
            {
                QuickLogger.Debug("Activating Trickle Mode");
                //_previousToggleState = chargeState;
                ChargeMode = chargeState;
                Charge = StoredPower;
                StoredPower = 0.0f;
            }
        }

        /// <summary>
        /// Turns off the battery
        /// </summary>
        public void PowerOffBattery()
        {
            ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);

            HasBreakerTripped = true;

            if (ChargeMode == PowerToggleStates.TrickleMode)
            {
                StoredPower = Charge;
                Charge = 0.0f;
            }

            Recharge();
        }

        /// <summary>
        /// Turns the battery on
        /// </summary>
        public void PowerOnBattery()
        {

            if (_chargeMode == PowerToggleStates.ChargeMode)
            {
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
            }

            if (ChargeMode == PowerToggleStates.TrickleMode)
            {
                HasBreakerTripped = false;
                Charge = StoredPower;
                StoredPower = 0.0f;
            }
        }

        #endregion

    }
}
