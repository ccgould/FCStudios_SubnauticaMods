using FCS_DeepDriller.Configuration;
using FCS_DeepDriller.Extenstion;
using FCS_DeepDriller.Helpers;
using FCSCommon.Extensions;
using Harmony;
using System.Collections.Generic;
using System.Reflection;
using SMLHelper.V2.Utility;

namespace FCS_DeepDriller.Patchers
{
    [HarmonyPatch(typeof(Equipment))]
    [HarmonyPatch("GetSlotType")]
    internal class Equipment_GetSlotType_Patch
    {
        [HarmonyPrefix]
        internal static void Prefix(string slot, ref EquipmentType __result)
        {
            EquipmentConfiguration.AddNewSlots();
        }
    }

    [HarmonyPatch(typeof(uGUI_Equipment))]
    [HarmonyPatch("Awake")]
    internal class uGUI_Equipment_Awake_Patch
    {
        [HarmonyPrefix]
        internal static void Prefix(uGUI_Equipment __instance)
        {
            __instance.gameObject.EnsureComponent<Initialize_uGUI>();
        }

        [HarmonyPostfix]
        internal static void Postfix(ref uGUI_Equipment __instance)
        {
            var allSlots = (Dictionary<string, uGUI_EquipmentSlot>)__instance.GetPrivateField("allSlots", BindingFlags.SetField);

            Initialize_uGUI.Instance.Add_uGUIslots(__instance, allSlots);
        }
    }
}
