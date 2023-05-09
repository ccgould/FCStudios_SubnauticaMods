using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_AlterraHub.Core.Services
{
    [HarmonyPatch(typeof(uGUI_PowerIndicator))]
    [HarmonyPatch("Initialize")]
    public class uGUIService
    {
        
        private static Dictionary<string, UIData> _uiInstances = new();


        public static uGUI_PowerIndicator IndicatorInstance { get; set; }

        public static void AddNewUI<T>(string id, GameObject prefab, Vector3 localScale = new()) where T : Component
        {
            prefab.AddComponent<T>();
            _uiInstances.Add(id, new UIData(prefab, localScale));
        }

        private static void Postfix(uGUI_PowerIndicator __instance)
        {

            if (IndicatorInstance == null)
            {
                if (Inventory.main == null)
                {
                    return;
                }

                var hudTransform = GameObject.Find("ScreenCanvas").transform.Find("HUD").Find("Content");


                foreach (var pendingUI in _uiInstances)
                {

                    var ui = GameObject.Instantiate(pendingUI.Value.Prefab);

                    ui.transform.SetParent(hudTransform, false);
                    ui.transform.SetSiblingIndex(0);
                    ui.transform.localScale = pendingUI.Value.LocalScale;
                }

                IndicatorInstance = __instance;
            }
        }

        public struct UIData
        {
            public GameObject Prefab { get; }
            public Vector3 LocalScale { get; }
            public GameObject Instance { get; set; }

            public Component Component { get; }
            public UIData(GameObject prefab, Vector3 scale)
            {
                Prefab = prefab;
                LocalScale = scale;
            }
        }
    }
}
