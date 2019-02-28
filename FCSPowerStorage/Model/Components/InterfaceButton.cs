using FCSPowerStorage.Utilities.Enums;
using FCSTerminal.Logging;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FCSPowerStorage.Model.Components
{
    public class InterfaceButton : OnScreenButton, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        private static readonly Color HOVER_COLOR = new Color(0.07f, 0.38f, 0.7f, 1f);
        private static Color STARTING_COLOR = Color.white;

        public GameObject ChangePage { get; set; }
        public bool IsToggle { get; set; }
        public string ToggleBtnName { get; set; }

        public void Start()
        {
            if (IsToggle)
            {
                //STARTING_COLOR = GetComponent<Text>().color;
            }
            else
            {
                STARTING_COLOR = GetComponent<Image>().color;
            }
        }

        public void OnEnable()
        {
            if (IsToggle)
            {
                if (GetComponent<Text>() != null)
                {
                    //GetComponent<Text>().color = STARTING_COLOR;
                }
            }
            else
            {
                if (GetComponent<Image>() != null)
                {
                    GetComponent<Image>().color = STARTING_COLOR;
                }
            }
        }

        public override void OnDisable()
        {
            if (IsToggle)
            {
                if (GetComponent<Text>() != null)
                {
                    //GetComponent<Text>().color = STARTING_COLOR;
                }
            }
            else
            {
                if (GetComponent<Image>() != null)
                {
                    GetComponent<Image>().color = STARTING_COLOR;
                }
            }

            base.OnDisable();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsHovered)
            {
                if (IsToggle)
                {
                    //GetComponent<Text>().color = new Color(0.1484375f, 0.44921875f, 0.87109375f);
                }
                else
                {
                    GetComponent<Image>().color = HOVER_COLOR;
                }

            }

        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (IsToggle)
            {
                //GetComponent<Text>().color = STARTING_COLOR;
            }
            else
            {
                GetComponent<Image>().color = STARTING_COLOR;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            Log.Info("OnPointerClick");


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

        private IEnumerator ChangeCurrentPage()
        {
            Log.Info("ChangeCurrentPage");
            FcsPowerStorageDisplay.ResetAnimation();

            yield return new WaitForEndOfFrame();
            if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;


            if (ChangePage.name.Equals("BatteryMonitorPage"))
            {
                //FcsPowerStorageDisplay.NavigationDock.SetActive(true);
                Log.Info($"Clicked {ChangePage.name}");
                FcsPowerStorageDisplay.Animator.SetBool("SettingsPageTrans", false);
                FcsPowerStorageDisplay.Animator.SetBool("BatteryMonitorTrans", true);

                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(FcsPowerStorageDisplay.BATTERY_MONITOR_PAGE_ANIMATION_TIME);
            }
            else if (ChangePage.name.Equals("SettingsPage"))
            {
                FcsPowerStorageDisplay.PrintAnimationValues();
                //FcsPowerStorageDisplay.NavigationDock.SetActive(true);
                Log.Info($"Clicked {ChangePage.name}");

                FcsPowerStorageDisplay.Animator.SetBool("ColorPageTrans", false);
                FcsPowerStorageDisplay.Animator.SetBool("SettingsPageTrans", true);


                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(FcsPowerStorageDisplay.SETTINGS_PAGE_ANIMATION_TIME);
            }
            else if (ChangePage.name.Equals("PowerOffPage"))
            {
                Log.Info($"Clicked {ChangePage.name}");
                FcsPowerStorageDisplay.PrintAnimationValues();


                if (!FcsPowerStorageDisplay.CustomBatteryController.HasBreakerTripped)
                {
                    Log.Info("System powering down up");

                    FcsPowerStorageDisplay.Animator.SetBool("Reboot", true);
                    FcsPowerStorageDisplay.Animator.SetBool("PowerOff", true);
                    FcsPowerStorageDisplay.EnterPowerOffScreen();
                    FcsPowerStorageDisplay.CustomBatteryController.PowerOffBattery();
                }
                else
                {
                    Log.Info("System booting up");
                    FcsPowerStorageDisplay.CustomBatteryController.HasBreakerTripped = false;
                    //FcsPowerStorageDisplay.NavigationDock.SetActive(false);
                    FcsPowerStorageDisplay.ResetNavigationBar();
                    FcsPowerStorageDisplay.CustomBatteryController.PowerOnBattery();
                    FcsPowerStorageDisplay.Animator.SetBool("ColorPageTrans", false);
                    Log.Info(FcsPowerStorageDisplay.Animator.GetBool("ColorPageTrans").ToString());
                    FcsPowerStorageDisplay.Animator.SetBool("PowerOff", false);
                    Log.Info(FcsPowerStorageDisplay.Animator.GetBool("PowerOff").ToString());

                    FcsPowerStorageDisplay.Animator.SetBool("PowerOn", true);
                    FcsPowerStorageDisplay.Animator.SetBool("Reboot", true);
                    FcsPowerStorageDisplay.Animator.SetBool("BootMode", false); // Because we have an inverter is the method
                    FcsPowerStorageDisplay.ResetCoroutine();
                }


                if (FcsPowerStorageDisplay.CustomBatteryController.IsBeingDeleted) yield break;
                yield return new WaitForSeconds(1.0f);
            }
        }


    }
}
