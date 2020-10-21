using System;
using System.Collections.Generic;
using FCS_AlterraHub.Buildables;
using FCS_AlterraHub.Helpers;
using HarmonyLib;
using UnityEngine;

namespace FCS_AlterraHub.Patch
{
    internal class UIPatches
    {
        [HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
        private static void Postfix_GetValues(Type enumType, ref Array __result)
        {
            if (enumType == typeof(PDATab))
            {
                __result = GetValues(PDATabPatcher.cacheManager, __result);
            }

        }


        private static T[] GetValues<T>(EnumCacheManager<T> cacheManager, Array __result) where T : Enum
        {
            var list = new List<T>();
            foreach (T type in __result)
            {
                list.Add(type);
            }

            list.AddRange(cacheManager.ModdedKeys);
            return list.ToArray();
        }

        //[HarmonyPatch(typeof(uGUI_DepthCompass))]
        //[HarmonyPatch("Start")]
        //internal class UGUIPatcher
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(ref uGUI_DepthCompass __instance)
        //    {
        //        var colorPickerDialog = GameObject.Instantiate(AlterraHub.ColorPickerDialogPrefab).AddComponent<ColorPickerDialog>();
        //        colorPickerDialog.SetupGrid(__instance.gameObject,44);
        //        colorPickerDialog.CloseColorPicker();

        //    }
        //}
    }
}


