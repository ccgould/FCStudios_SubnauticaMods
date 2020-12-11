using FCS_LifeSupportSolutions.Buildable;
using FCSCommon.Helpers;
using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace FCS_LifeSupportSolutions.Patches
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    internal class uGUI_PowerIndicator_Initialize_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(uGUI_PowerIndicator __instance)
        {
            if (IndicatorInstance != null) return;

                if (Inventory.main == null)
            {
                return;
            }

            var prefab = GameObject.Instantiate(ModelPrefab.PillHudPrefab);
            var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content").Find("BarsPanel");
            prefab.transform.SetParent(hudTransform, false);
            prefab.transform.localPosition = new Vector3(-24.90f, 128.10f, 0f);
            prefab.transform.localEulerAngles = new Vector3(0f,0f,34.90f);
            prefab.transform.localScale = Vector3.one;
            prefab.transform.SetSiblingIndex(0);
            LifeSupportHUD = prefab.AddComponent<LifeSupportHUD>();
            IndicatorInstance = __instance;
        }

        public static LifeSupportHUD LifeSupportHUD { get; private set; }
        public static uGUI_PowerIndicator IndicatorInstance { get; set; }
    }

    internal class LifeSupportHUD : MonoBehaviour
    {
        private Image _percentage;

        private void Start()
        {
            _percentage = GameObjectHelpers.FindGameObject(gameObject, "Percentage")?.GetComponent<Image>();
        }
        private void Update()
        {
            if (_percentage != null)
            {
                _percentage.fillAmount = Survival_GetWeaknessSpeedScalar.PlayerAdrenaline?.Percentage ?? 0;
            }
        }

        public void ToggleVisibility()
        {
            if (Survival_GetWeaknessSpeedScalar.PlayerAdrenaline == null) return;
            gameObject.SetActive(GameModeUtils.RequiresSurvival() && Survival_GetWeaknessSpeedScalar.PlayerAdrenaline.HasAdrenaline);
        }
    }
}
