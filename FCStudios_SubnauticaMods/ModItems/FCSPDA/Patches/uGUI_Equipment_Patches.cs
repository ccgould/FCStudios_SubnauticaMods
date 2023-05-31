using FCSCommon.Utilities;
using HarmonyLib;
using UnityEngine;
using UWE;
using System.Collections;
using FCS_AlterraHub.Core.Helpers;
using FCS_AlterraHub.ModItems.FCSPDA.Mono;
using FCS_AlterraHub.API;
using FCS_AlterraHub.ModItems.FCSPDA.Enums;

namespace FCS_AlterraHub.ModItems.FCSPDA.Patches;

[HarmonyPatch]
public static class uGUI_Equipment_Patches
{
    [HarmonyPatch(typeof(uGUI_Equipment), nameof(uGUI_Equipment.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(uGUI_Equipment __instance)
    {
        QuickLogger.Debug("uGUI_Equipment Awake", true);

        var modulePrefab = __instance.gameObject.FindChild($"CyclopsModule1").gameObject;

        var transform = __instance.gameObject.transform;

        for (int i = 1; i < 7; i++)
        {
            var name = $"BaseManagerModule{i}";
           var module =  __instance.gameObject.FindChild($"CyclopsModule{i}");
            var f = GameObject.Instantiate(modulePrefab,transform,false);
            var slot = f.GetComponent<uGUI_EquipmentSlot>();
            slot.active = false;
            slot.slot = $"BaseManagerModule{i}";
            slot.name = name;
            slot.rectTransform.anchoredPosition = module.GetComponent<uGUI_EquipmentSlot>().rectTransform.anchoredPosition;
            GameObject.Destroy(f.transform.GetChild(0).gameObject);
            __instance.allSlots.Add(name, slot);
        }
    }
}
