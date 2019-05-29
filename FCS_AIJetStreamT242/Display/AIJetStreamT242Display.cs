using FCS_AIMarineTurbine.Buildable;
using FCS_AIMarineTurbine.Display.Patching;
using FCS_AIMarineTurbine.Mono;
using FCSCommon.Enums;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_AIMarineTurbine.Display
{
    public class AIJetStreamT242Display : AIDisplay
    {

        #region Private Members
        private GameObject _homePage;
        private GameObject _depth;
        private Text _depthValue;
        private GameObject _turbineSpeed;
        private Text _turbineSpeedValue;
        private GameObject _background;
        private GameObject _poweredOffScreen;
        private GameObject _poweredScreenPowerBTN;
        private GameObject _homeScreenPowerBTN;
        private readonly float RESET_TIMER = 2f;
        private bool _foundComponents;
        private GameObject _healthMeters;
        private GameObject _healthSlider;
        private GameObject _healthPercentage;
        private GameObject _powerMeters;
        private GameObject _powerSlider;
        private GameObject _powerPercentage;
        private AIJetStreamT242Controller _mono;
        private int _screenStateHash;
        private int _powerOff = 2;
        private int _powerOn = 1;
        private int _blackOut = 0;

        #endregion

        #region Public Properties

        public GameObject CanvasGameObject { get; set; }


        #endregion

        #region Unity Methods

        private void Awake()
        {
            _screenStateHash = UnityEngine.Animator.StringToHash("ScreenState");
        }

        private void Update()
        {
            UpdateValues();
        }

        #endregion

        #region Public Methods
        internal void Setup(AIJetStreamT242Controller jetStreamController)
        {
            if (jetStreamController.IsBeingDeleted) return;
            _mono = jetStreamController;

            if (FindAllComponents() == false)
            {
                _foundComponents = false;
                QuickLogger.Error("// ============== Error getting all Components ============== //");
                TurnDisplayOff();
                return;
            }

            UpdateLanaguage();

            _foundComponents = true;

            if (_mono.PowerManager.GetHasBreakerTripped())
            {
                StartCoroutine(PowerOff());
            }
            else
            {
                StartCoroutine(CompleteSetup());

            }
        }

        public void TurnDisplayOff()
        {
            StartCoroutine(ShutDown());
        }

        #endregion

        #region Private Methods

        public override void ItemModified<T>(T item)
        {
            throw new NotImplementedException();
        }

        public override bool FindAllComponents()
        {
            QuickLogger.Debug("Find All Components");

            #region Canvas

            CanvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (CanvasGameObject == null)
            {
                QuickLogger.Error("Canvas not found.");
                return false;
            }

            #endregion


            // == Canvas Elements == //

            #region Home Screen
            _homePage = CanvasGameObject.transform.Find("HomePage")?.gameObject;
            if (_homePage == null)
            {
                QuickLogger.Error("Screen: Home Page not found.");
                return false;
            }
            #endregion

            #region Depth
            _depth = _homePage.transform.Find("Depth")?.gameObject;

            if (_depth == null)
            {
                QuickLogger.Error("Panel: Depth not found.");
                return false;
            }
            #endregion

            #region Depth Value
            _depthValue = _depth.transform.Find("Depth_Value")?.GetComponent<Text>();

            if (_depthValue == null)
            {
                QuickLogger.Error("Text: Depth Value not found.");
                return false;
            }
            #endregion

            #region Turbine Speed
            _turbineSpeed = _homePage.transform.Find("TurbineSpeed")?.gameObject;

            if (_turbineSpeed == null)
            {
                QuickLogger.Error("GameObject: Turbine Speed not found.");
                return false;
            }
            #endregion

            #region Turbine Speed Value
            _turbineSpeedValue = _turbineSpeed.transform.Find("TurbineSpeed_Value")?.GetComponent<Text>();

            if (_turbineSpeedValue == null)
            {
                QuickLogger.Error("Text: Turbine Speed Value not found.");
                return false;
            }
            #endregion

            #region Home Screen Power BTN
            _homeScreenPowerBTN = _homePage.transform.Find("Power_BTN")?.gameObject;

            if (_homeScreenPowerBTN == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }


            var homeScreenPowerBTN = _homeScreenPowerBTN.AddComponent<InterfaceButton>();
            homeScreenPowerBTN.ButtonMode = InterfaceButtonMode.Background;
            homeScreenPowerBTN.OnButtonClick = OnButtonClick;
            homeScreenPowerBTN.BtnName = "HPPBtn";

            #endregion

            #region Health Meter
            _healthMeters = _homePage.transform.Find("Health_Meter")?.gameObject;

            if (_healthMeters == null)
            {
                QuickLogger.Error("Screen: Health Meter not found.");
                return false;
            }
            #endregion

            #region Health Meter Bar
            _healthSlider = _healthMeters.transform.Find("Slider")?.gameObject;

            if (_healthSlider == null)
            {
                QuickLogger.Error("Screen: Health Slider not found.");
                return false;
            }
            #endregion

            #region Health Meter Percentage
            _healthPercentage = _healthSlider.transform.Find("Heath_Percentage")?.gameObject;

            if (_healthPercentage == null)
            {
                QuickLogger.Error("Screen: Health Percentage not found.");
                return false;
            }
            #endregion

            #region Power Meter
            _powerMeters = _homePage.transform.Find("Power_Meter")?.gameObject;

            if (_powerMeters == null)
            {
                QuickLogger.Error("Screen: Power Meter not found.");
                return false;
            }
            #endregion

            #region Power Meter Bar
            _powerSlider = _powerMeters.transform.Find("Slider")?.gameObject;

            if (_powerSlider == null)
            {
                QuickLogger.Error("Screen: Power Slider not found.");
                return false;
            }
            #endregion

            #region Power Meter Percentage
            _powerPercentage = _powerSlider.transform.Find("Power_Percentage")?.gameObject;

            if (_powerPercentage == null)
            {
                QuickLogger.Error("Screen: Power Percentage not found.");
                return false;
            }
            #endregion


            #region Background
            _background = CanvasGameObject.transform.Find("Background")?.gameObject;
            if (_background == null)
            {
                QuickLogger.Error("Screen: Background not found.");
                return false;
            }
            #endregion

            #region PowerOff
            _poweredOffScreen = CanvasGameObject.transform.Find("PowerOffPage")?.gameObject;
            if (_poweredOffScreen == null)
            {
                QuickLogger.Error("Screen: Powered Off Page not found.");
                return false;
            }
            #endregion

            #region Power BTN
            _poweredScreenPowerBTN = _poweredOffScreen.transform.Find("Power_BTN")?.gameObject;
            if (_poweredScreenPowerBTN == null)
            {
                QuickLogger.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }

            var poweredOffScreenBTN = _poweredScreenPowerBTN.AddComponent<InterfaceButton>();
            poweredOffScreenBTN.ButtonMode = InterfaceButtonMode.Background;
            poweredOffScreenBTN.OnButtonClick = OnButtonClick;
            poweredOffScreenBTN.BtnName = "PPBtn";
            #endregion
            return true;
        }

        private void UpdateValues()
        {
            if (_foundComponents)
            {
                _depthValue.text = $"{Math.Round(_mono.GetDepth())}m";

                _turbineSpeedValue.text = $"{_mono.GetSpeed()}rpm";

                _healthSlider.GetComponent<Slider>().value = _mono.HealthManager.GetHealth() / 100f;

                _healthPercentage.GetComponent<Text>().text = $"{_mono.HealthManager.GetHealth()}%";

                int powerPercent;
                if (_mono.PowerManager.GetCharge() <= 0.0f || AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity <= 0.0f)
                {
                    powerPercent = 0;
                }
                else
                {
                    powerPercent = Convert.ToInt32((_mono.PowerManager.GetCharge() / AIJetStreamT242Buildable.JetStreamT242Config.MaxCapacity) * 100);
                }

                _powerSlider.GetComponent<Slider>().value = powerPercent / 100f;


                _powerPercentage.GetComponent<Text>().text = $"{powerPercent}%";
            }

        }

        private void UpdateLanaguage()
        {
            _depth.GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.DepthKey);
            _healthSlider.FindChild("Heath_LBL").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.HealthKey);
            _powerSlider.FindChild("Power_LBL").GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.PowerKey);
            _turbineSpeed.GetComponent<Text>().text = LanguageHelpers.GetLanguage(DisplayLanguagePatching.SpeedKey);
        }
        #endregion

        public override void ChangePageBy(int amountToChangePageBy)
        {
            throw new NotImplementedException();
        }

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PPBtn":
                    _mono.PowerManager.TriggerPowerOn();
                    StartCoroutine(PowerOn());
                    break;

                case "HPPBtn":
                    _mono.PowerManager.TriggerPowerOff();
                    StartCoroutine(PowerOff());
                    break;

            }
        }

        public override IEnumerator PowerOff()
        {
            yield return new WaitForEndOfFrame();
            if (_mono.IsBeingDeleted) yield break;

            QuickLogger.Info($"Powering Off");
            _mono.AnimationManager.SetFloatHash(_screenStateHash, _powerOff);

            if (_mono.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (_mono.IsBeingDeleted) yield break;


            //ResetAnimation();
        }

        public override IEnumerator PowerOn()
        {
            yield return new WaitForEndOfFrame();
            if (_mono.IsBeingDeleted) yield break;


            if (_mono.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (_mono.IsBeingDeleted) yield break;

            //ResetAnimation();

            QuickLogger.Info($"Powering On");
            StartCoroutine(CompleteSetup());
        }

        public override IEnumerator ShutDown()
        {
            yield return new WaitForEndOfFrame();
            if (_mono.IsBeingDeleted) yield break;


            QuickLogger.Info($"Shutting Down");
            _mono.AnimationManager.SetFloatHash(_screenStateHash, _powerOff);

            if (_mono.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (_mono.IsBeingDeleted) yield break;

            //ResetAnimation();

        }

        public override IEnumerator CompleteSetup()
        {
            QuickLogger.Info("InComplete Setup");
            if (!_mono.PowerManager.GetHasBreakerTripped())
            {
                yield return new WaitForEndOfFrame();
                if (_mono.IsBeingDeleted) yield break;

                QuickLogger.Info($"Starting Home Screen");

                _mono.AnimationManager.SetFloatHash(_screenStateHash, _powerOn);

                if (_mono.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(RESET_TIMER);
                if (_mono.IsBeingDeleted) yield break;

                //ResetAnimation();
            }
        }

        private void ResetAnimation()
        {
            foreach (var parameter in _mono.AnimationManager.GetParameters())
            {
                _mono.AnimationManager.SetBoolHash(parameter.GetHashCode(), false);
                QuickLogger.Info($"Animator Parameter {parameter.name} was reset to false");
            }
        }
    }
}
