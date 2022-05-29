using System;
using System.Collections;
using FCS_AlterraHub.Systems;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace FCS_AlterraHub.Patches
{
    [HarmonyPatch(typeof(EndCreditsManager), nameof(EndCreditsManager.Start))]
    public class EndCreditsManager_Start_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(EndCreditsManager __instance)
        {
#if SUBNAUTICA

            try
            {
                if (PlatformUtils.isPS4Platform)
                {
                    __instance.leftText.text = __instance.ps4CreditsLeft;
                    __instance.centerText.text = __instance.ps4CreditsCenter;
                    __instance.rightText.text = __instance.ps4CreditsRight;
                }

                __instance.fadeLogo = true;
                __instance.startFadeTime = Time.time;
                __instance.Invoke("PlayMusic", 0.5f);
#if SUBNAUTICA_STABLE
                __instance.goToPos = new Vector3(0f, (float)__instance.scrollMaxValue, 0f);
#else
                __instance.goToPos = new Vector3(0f,
                    __instance.centerText.preferredHeight +
                    __instance.creditsText.parent.GetComponent<RectTransform>().rect.height, 0f);
#endif

                if (CardSystem.main.IsDebitPaid())
                {
                    __instance.StartCoroutine(ReturnToMainMenu(__instance.secondsUntilScrollComplete, "Play_DebtPaid"));
                    //QPatch.MissionManagerGM.NotifyTechTypeConstructed(__instance.techType);
                }
                else if (CardSystem.main.HasPaymentBeenMadeToDebit())
                {
                    __instance.StartCoroutine(ReturnToMainMenu(__instance.secondsUntilScrollComplete,
                        "Play_NotDebtPaid"));
                }
                else
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                QuickLogger.Error(e.Message);
                QuickLogger.Error(e.StackTrace);
                QuickLogger.Info("Failed to patch EndCreditManager returning to origin");
                // Give back execution to origin function.
                return true;
            }
#else
            return true;
#endif
        }

        public static IEnumerator ReturnToMainMenu(float seconds, string key)
        {
            yield return new WaitForSeconds(seconds - 3f);
            QuickLogger.Debug("Debt has been paid Skipping", true);
            VoiceNotificationSystem.main.Play($"{key}_key");
            yield return new WaitForSeconds(10.5f);
            yield return SceneManager.LoadSceneAsync("Cleaner", LoadSceneMode.Single);
            yield break;
        }
    }
}