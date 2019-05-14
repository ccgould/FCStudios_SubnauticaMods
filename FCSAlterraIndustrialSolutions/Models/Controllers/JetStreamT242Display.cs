using FCSAlterraIndustrialSolutions.Logging;
using FCSAlterraIndustrialSolutions.Models.Abstract;
using FCSAlterraIndustrialSolutions.Models.Buttons;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSAlterraIndustrialSolutions.Models.Controllers
{
    public class JetStreamT242Display : AIDisplay, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {

        #region Private Members
        private GameObject _jetStream;
        private GameObject _model;
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


        #endregion

        #region Public Properties
        public JetStreamT242Controller Controller { get; set; }
        public GameObject CanvasGameObject { get; set; }
        public Animator Animator { get; private set; }

        #endregion

        #region Unity Methods


        private void Update()
        {
            UpdateValues();
        }

        #endregion

        #region Public Methods
        public void Setup(JetStreamT242Controller jetStreamController)
        {
            if (jetStreamController.IsBeingDeleted) return;
            Controller = jetStreamController;

            if (FindAllComponents() == false)
            {
                _foundComponents = false;
                Log.Error("// ============== Error getting all Components ============== //");
                TurnDisplayOff();
                return;
            }

            UpdateLanaguage();

            _foundComponents = true;

            if (Controller.HasBreakerTripped)
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
        public override bool FindAllComponents()
        {
            Log.Info("Find All Components");

            #region JetStream

            _jetStream = LoadItems.JetStreamT242Prefab;
            if (_jetStream == null)
            {
                Log.Error("Object: Jet Stream T242 not found.");
                return false;
            }
            #endregion

            #region Canvas

            CanvasGameObject = gameObject.GetComponentInChildren<Canvas>()?.gameObject;

            if (CanvasGameObject == null)
            {
                Log.Error("Canvas not found.");
                return false;
            }

            #endregion


            // == Canvas Elements == //

            #region Model

            _model = _jetStream.FindChild("model")?.gameObject;

            if (_model == null)
            {
                Log.Error("Gameobject: Model not found.");
                return false;
            }
            #endregion

            #region Home Screen
            _homePage = CanvasGameObject.transform.Find("HomePage")?.gameObject;
            if (_homePage == null)
            {
                Log.Error("Screen: Home Page not found.");
                return false;
            }
            #endregion

            #region Depth
            _depth = _homePage.transform.Find("Depth")?.gameObject;

            if (_depth == null)
            {
                Log.Error("Panel: Depth not found.");
                return false;
            }
            #endregion

            #region Depth Value
            _depthValue = _depth.transform.Find("Depth_Value")?.GetComponent<Text>();

            if (_depthValue == null)
            {
                Log.Error("Text: Depth Value not found.");
                return false;
            }
            #endregion

            #region Turbine Speed
            _turbineSpeed = _homePage.transform.Find("TurbineSpeed")?.gameObject;

            if (_turbineSpeed == null)
            {
                Log.Error("GameObject: Turbine Speed not found.");
                return false;
            }
            #endregion

            #region Turbine Speed Value
            _turbineSpeedValue = _turbineSpeed.transform.Find("TurbineSpeed_Value")?.GetComponent<Text>();

            if (_turbineSpeedValue == null)
            {
                Log.Error("Text: Turbine Speed Value not found.");
                return false;
            }
            #endregion

            #region Home Screen Power BTN
            _homeScreenPowerBTN = _homePage.transform.Find("Power_BTN")?.gameObject;

            if (_homeScreenPowerBTN == null)
            {
                Log.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }


            var homeScreenPowerBTN = _homeScreenPowerBTN.AddComponent<InterfaceButton>();
            homeScreenPowerBTN.Display = this;
            homeScreenPowerBTN.BtnName = "HPPBtn";

            #endregion

            #region Health Meter
            _healthMeters = _homePage.transform.Find("Health_Meter")?.gameObject;

            if (_healthMeters == null)
            {
                Log.Error("Screen: Health Meter not found.");
                return false;
            }
            #endregion

            #region Health Meter Bar
            _healthSlider = _healthMeters.transform.Find("Slider")?.gameObject;

            if (_healthSlider == null)
            {
                Log.Error("Screen: Health Slider not found.");
                return false;
            }
            #endregion

            #region Health Meter Percentage
            _healthPercentage = _healthSlider.transform.Find("Heath_Percentage")?.gameObject;

            if (_healthPercentage == null)
            {
                Log.Error("Screen: Health Percentage not found.");
                return false;
            }
            #endregion

            #region Power Meter
            _powerMeters = _homePage.transform.Find("Power_Meter")?.gameObject;

            if (_powerMeters == null)
            {
                Log.Error("Screen: Power Meter not found.");
                return false;
            }
            #endregion

            #region Power Meter Bar
            _powerSlider = _powerMeters.transform.Find("Slider")?.gameObject;

            if (_powerSlider == null)
            {
                Log.Error("Screen: Power Slider not found.");
                return false;
            }
            #endregion

            #region Power Meter Percentage
            _powerPercentage = _powerSlider.transform.Find("Power_Percentage")?.gameObject;

            if (_powerPercentage == null)
            {
                Log.Error("Screen: Power Percentage not found.");
                return false;
            }
            #endregion


            #region Background
            _background = CanvasGameObject.transform.Find("Background")?.gameObject;
            if (_background == null)
            {
                Log.Error("Screen: Background not found.");
                return false;
            }
            #endregion

            #region PowerOff
            _poweredOffScreen = CanvasGameObject.transform.Find("PowerOffPage")?.gameObject;
            if (_poweredOffScreen == null)
            {
                Log.Error("Screen: Powered Off Page not found.");
                return false;
            }
            #endregion

            #region Power BTN
            _poweredScreenPowerBTN = _poweredOffScreen.transform.Find("Power_BTN")?.gameObject;
            if (_poweredScreenPowerBTN == null)
            {
                Log.Error("Screen: Powered Off Screen Button not found.");
                return false;
            }

            var poweredOffScreenBTN = _poweredScreenPowerBTN.AddComponent<InterfaceButton>();
            poweredOffScreenBTN.Display = this;
            poweredOffScreenBTN.BtnName = "PPBtn";
            #endregion

            #region Animator

            Animator = gameObject.GetComponentInChildren<Animator>();

            if (Animator == null)
            {
                Log.Error("Animator not found.");
                return false;
            }

            #endregion

            return true;
        }

        private void UpdateValues()
        {
            if (_foundComponents)
            {
                _depthValue.text = $"{Math.Round(Controller.GetDepth())}m";

                _turbineSpeedValue.text = $"{Controller.GetSpeed()}rpm";

                _healthSlider.GetComponent<Slider>().value = Controller.LiveMixin.health / 100f;

                _healthPercentage.GetComponent<Text>().text = $"{Controller.LiveMixin.health}%";

                int powerPercent;
                if (Controller.Charge <= 0.0f || LoadItems.JetStreamT242Config.MaxCapacity <= 0.0f)
                {
                    powerPercent = 0;
                }
                else
                {
                    powerPercent = Convert.ToInt32((Controller.Charge / LoadItems.JetStreamT242Config.MaxCapacity) * 100);
                }

                _powerSlider.GetComponent<Slider>().value = powerPercent / 100f;


                _powerPercentage.GetComponent<Text>().text = $"{powerPercent}%";
            }

        }

        private void UpdateLanaguage()
        {
            _depth.GetComponent<Text>().text = $"{LoadItems.JetStreamT242ModStrings.Depth}:";
            _healthSlider.FindChild("Heath_LBL").GetComponent<Text>().text = $"{LoadItems.JetStreamT242ModStrings.Health}";
            _powerSlider.FindChild("Power_LBL").GetComponent<Text>().text = $"{LoadItems.JetStreamT242ModStrings.Power}";
            _turbineSpeed.GetComponent<Text>().text = $"{LoadItems.JetStreamT242ModStrings.Speed}:";
            _depth.GetComponent<Text>().text = $"{LoadItems.JetStreamT242ModStrings.Depth}:";
        }
        #endregion

        public override void OnButtonClick(string btnName, object tag)
        {
            if (btnName == string.Empty) return;

            switch (btnName)
            {
                case "PPBtn":
                    Controller.HasBreakerTripped = false;
                    StartCoroutine(PowerOn());
                    break;

                case "HPPBtn":
                    Controller.HasBreakerTripped = true;
                    StartCoroutine(PowerOff());
                    //Controller.LiveMixin.health = 0f;
                    //Controller.IsDamagedFlag = true;
                    break;

            }
        }

        public override void ItemModified<T>(T item)
        {

        }

        public override IEnumerator PowerOff()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;

            Log.Info($"Powering Off");
            Animator.SetBool("PowerOff", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;


            ResetAnimation();
        }

        public override IEnumerator PowerOn()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;


            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            ResetAnimation();

            Log.Info($"Powering On");
            StartCoroutine(CompleteSetup());
        }

        public override IEnumerator ShutDown()
        {
            Animator.enabled = true;

            yield return new WaitForEndOfFrame();
            if (Controller.IsBeingDeleted) yield break;


            Log.Info($"Shutting Down");
            Animator.SetBool("Reboot", true);

            if (Controller.IsBeingDeleted) yield break;
            yield return new WaitForSeconds(RESET_TIMER);
            if (Controller.IsBeingDeleted) yield break;

            ResetAnimation();

        }

        public override IEnumerator CompleteSetup()
        {
            Log.Info("InComplete Setup");
            Log.Info($"HasBreakerTripped || {Controller.HasBreakerTripped}");
            if (!Controller.HasBreakerTripped)
            {
                Animator.enabled = true;
                Log.Info($"Animator Enabled : {Animator.enabled}");

                yield return new WaitForEndOfFrame();
                if (Controller.IsBeingDeleted) yield break;

                Log.Info($"Starting Home Screen");

                Animator.SetBool("Home", true);

                if (Controller.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(RESET_TIMER);
                if (Controller.IsBeingDeleted) yield break;

                ResetAnimation();
            }
        }

        private void ResetAnimation()
        {
            foreach (var parameter in Animator.parameters)
            {
                Animator.SetBool(parameter.name, false);
                Log.Info($"Animator Parameter {parameter.name} was reset to false");
            }
            //Animator.SetBool("BootScreen", false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        public override void ChangePageBy(int amountToChangePageBy)
        {

        }
    }
}
