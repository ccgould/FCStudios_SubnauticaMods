using FCSCommon.Extensions;
using FCSCommon.Utilities;
using FCSCommon.Utilities.Enums;
using FCSPowerStorage.Configuration;
using FCSPowerStorage.Helpers;
using FCSPowerStorage.Managers;
using FCSPowerStorage.Model.Components;
using Oculus.Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FCSPowerStorage.Model
{
    /// <summary>
    /// The controller for the custom battery
    /// </summary>
    internal class CustomBatteryController : MonoBehaviour, IProtoEventListener, IConstructable
    {
        #region Private Members
        //TODO Remove
        public GameObject seaBase { get; private set; }
        private FCSPowerStorageDisplay fcsPowerStorageDisplay;
        #endregion

        #region Public Properties
        public bool IsBeingDeleted { get; set; }
        public PowerToggleStates ChargeMode { get; set; }

        public Color MainBodyColor { get; set; }
        public string ID { get; set; }
        public FCSPowerManager PowerManager { get; set; }
        #endregion

        #region Private Members
        private bool _isEnabled;
        private Constructable _buildable;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            MainBodyColor = new Color(0.99609375f, 0.99609375f, 0.99609375f);
            ID = GetComponentInParent<PrefabIdentifier>().Id;
            _buildable = GetComponentInParent<Constructable>();

            PowerManager = GetComponentInParent<FCSPowerManager>();

            if (PowerManager == null)
            {
                QuickLogger.Error("Power Manager was not found.");
            }
            else
            {
                PowerManager.Initialize(this);
            }

            AnimationManager = GetComponentInParent<FCSPowerStorageAnimationManager>();

            if (PowerManager == null)
            {
                QuickLogger.Error("Animation Manager was not found.");
            }
            else
            {
                PowerManager.Initialize(this);
            }
        }

        public FCSPowerStorageAnimationManager AnimationManager { get; set; }
        internal readonly int BatteryCount = 6;

        #endregion

        #region Private Methods

        //private void UpdatePowerState()
        //{

        //    // If in charge mode and we are online turn the lights orange
        //    if (HasBreakerTripped && ColorChanger.CurrentColor != ColorChanger.Red)
        //    {
        //        ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);
        //        return;
        //    }

        //    if (Charge >= 1 && ChargeMode == PowerToggleStates.ChargeMode && ColorChanger.CurrentColor != ColorChanger.Orange && !HasBreakerTripped)
        //    {
        //        ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
        //    }
        //    // If in charge mode and we are online turn the lights orange
        //    else if (Charge < 1 && ChargeMode == PowerToggleStates.ChargeMode && ColorChanger.CurrentColor != ColorChanger.Orange && !HasBreakerTripped)
        //    {
        //        ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
        //    }
        //    else if (Charge < 1 && _previousPowerState != FCSPowerStates.Unpowered)
        //    {
        //        _previousPowerState = FCSPowerStates.Unpowered;
        //        ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);
        //    }
        //    else if (Charge >= 1 && _previousPowerState != FCSPowerStates.Powered)
        //    {
        //        _previousPowerState = FCSPowerStates.Powered;
        //        ColorChanger.ConfigureSystemLights(FCSPowerStates.Powered, transform.gameObject);
        //    }
        //}

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

        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            QuickLogger.Debug($"Get FCSPowerStorageDisplay");

            var activeDisplay = GetComponentInParent<FCSPowerStorageDisplay>();

            QuickLogger.Debug($"Found FCSPowerStorageDisplay");
            SaveData saveData = null;

            QuickLogger.Debug($"Create Save Data");

            saveData = new SaveData
            {
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
                LoadData.CleanOldSaveData();
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
        /// Turns off the battery
        /// </summary>
        internal void PowerOffBattery()
        {
            ColorChanger.ConfigureSystemLights(FCSPowerStates.Unpowered, transform.gameObject);

            PowerManager.SetBreakerTrip(true);

            PowerManager.StorePower();
        }

        /// <summary>
        /// Turns the battery on
        /// </summary>
        internal void PowerOnBattery()
        {
            if (ChargeMode == PowerToggleStates.ChargeMode)
            {
                ColorChanger.ConfigureSystemLights(FCSPowerStates.Buffer, transform.gameObject);
            }

            PowerManager.SetBreakerTrip(false);
        }

        #endregion

    }
}
