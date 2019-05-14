using FCSPowerStorage.Logging;
using FCSPowerStorage.Utilities.Enums;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSPowerStorage.Model.Components
{
    /// <summary>
    /// This class is a component for all interface buttons except the color picker and the paginator.
    /// For the color picker see the <see cref="ColorItemButton"/>
    /// For the paginator see the <see cref="PaginatorButton"/> 
    /// </summary>
    public class InterfaceButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        #region Private Member
        private static readonly Color HOVER_COLOR = new Color(0.07f, 0.38f, 0.7f, 1f);
        private static Color STARTING_COLOR = Color.white;
        #endregion

        #region Public Properties
        /// <summary>
        /// The pages to change to.
        /// </summary>
        public GameObject ChangePage { get; set; }

        /// <summary>
        /// Boolean that states if the button is a radial button
        /// </summary>
        public bool IsToggle { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// The name of the radial button
        /// </summary>
        public string ToggleBtnName { get; set; }

        public void Start()
        {
            if (!IsToggle)
            {
                STARTING_COLOR = GetComponent<Image>().color;
            }
        }

        public void OnEnable()
        {
            if (!IsToggle)
            {
                if (GetComponent<Image>() != null)
                {
                    GetComponent<Image>().color = STARTING_COLOR;
                }
            }
        }
        #endregion

        #region Private Methods
        private IEnumerator ChangeCurrentPage()
        {
            FcsPowerStorageDisplay.ResetAnimation();

            yield return new WaitForEndOfFrame();
            if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
            if (ChangePage.name.Equals("BatteryMonitorPage"))
            {
                Log.Info($"Clicked {ChangePage.name}");
                FcsPowerStorageDisplay.Animator.SetBool("SettingsPageTrans", false);
                FcsPowerStorageDisplay.Animator.SetBool("BatteryMonitorTrans", true);

                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(FcsPowerStorageDisplay.BATTERY_MONITOR_PAGE_ANIMATION_TIME);
            }
            else if (ChangePage.name.Equals("SettingsPage"))
            {

                Log.Info($"Clicked {ChangePage.name}");

                FcsPowerStorageDisplay.Animator.SetBool("ColorPageTrans", false);
                FcsPowerStorageDisplay.Animator.SetBool("SettingsPageTrans", true);


                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(FcsPowerStorageDisplay.SETTINGS_PAGE_ANIMATION_TIME);
            }
            else if (ChangePage.name.Equals("PowerOffPage"))
            {
                Log.Info($"Clicked {ChangePage.name}");

                if (!FcsPowerStorageDisplay.CustomBatteryController.HasBreakerTripped)
                {
                    Log.Info("System powering down down");
                    FcsPowerStorageDisplay.Animator.SetBool("Reboot", true);
                    FcsPowerStorageDisplay.Animator.SetBool("PowerOff", true);
                    FcsPowerStorageDisplay.EnterPowerOffScreen();
                    FcsPowerStorageDisplay.CustomBatteryController.PowerOffBattery();
                }
                else
                {
                    Log.Info("System booting up");
                    FcsPowerStorageDisplay.CustomBatteryController.HasBreakerTripped = false;
                    FcsPowerStorageDisplay.ResetNavigationBar();
                    FcsPowerStorageDisplay.CustomBatteryController.PowerOnBattery();
                    FcsPowerStorageDisplay.Animator.SetBool("ColorPageTrans", false);
                    FcsPowerStorageDisplay.Animator.SetBool("PowerOff", false);

                    FcsPowerStorageDisplay.Animator.SetBool("PowerOn", true);
                    FcsPowerStorageDisplay.Animator.SetBool("Reboot", true);
                    FcsPowerStorageDisplay.Animator.SetBool("BootMode", false); // Because we have an inverter is the method
                    FcsPowerStorageDisplay.ResetCoroutine();
                }


                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(1.0f);
            }
        }
        #endregion

        #region Public Overrides
        public override void OnDisable()
        {
            base.OnDisable();

            if (!IsToggle)
            {
                if (GetComponent<Image>() != null)
                {
                    GetComponent<Image>().color = STARTING_COLOR;
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                if (!IsToggle)
                {
                    GetComponent<Image>().color = HOVER_COLOR;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (!IsToggle)
            {
                GetComponent<Image>().color = STARTING_COLOR;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (IsHovered)
            {
                if (IsToggle && ToggleBtnName.Equals("Color_Picker"))
                {
                    FcsPowerStorageDisplay.Animator.SetBool("SettingsPageTrans", false);

                    FcsPowerStorageDisplay.Animator.SetBool("ColorPageTrans", true);
                }
                else if (IsToggle)
                {
                    if (ToggleBtnName.Equals("Trickle_Mode"))
                    {
                        FCSPowerStorageDisplay.PowerToggleState = PowerToggleStates.TrickleMode;
                        FcsPowerStorageDisplay.ChargeModeCheckBox.SetActive(false);
                        FcsPowerStorageDisplay.TrickleModeCheckBox.SetActive(true);
                        FcsPowerStorageDisplay.CustomBatteryController.ActivateChargeState(PowerToggleStates.TrickleMode);

                    }
                    else if (ToggleBtnName.Equals("Charge_Mode"))
                    {
                        FCSPowerStorageDisplay.PowerToggleState = PowerToggleStates.ChargeMode;
                        FcsPowerStorageDisplay.ChargeModeCheckBox.SetActive(true);
                        FcsPowerStorageDisplay.TrickleModeCheckBox.SetActive(false);
                        FcsPowerStorageDisplay.CustomBatteryController.ActivateChargeState(PowerToggleStates.ChargeMode);
                    }
                }
                else
                {
                    StartCoroutine(ChangeCurrentPage());
                }
            }
        }
        #endregion

    }
}
