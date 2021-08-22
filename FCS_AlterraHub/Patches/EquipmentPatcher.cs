using System.Collections.Generic;
using System.Reflection;
using FCS_AlterraHub.Extensions;
using FCS_AlterraHub.Model.GUI;
using HarmonyLib;

namespace FCS_AlterraHub.Patches
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