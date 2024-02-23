using FCS_EnergySolutions.ModItems.Buildables.JetStream.Buildables;
using FCSCommon.Utilities;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FCS_EnergySolutions.Patches;
[HarmonyPatch]
public static class uGUI_Equipment_Patches
{
    [HarmonyPatch(typeof(uGUI_Equipment), nameof(uGUI_Equipment.Awake))]
    [HarmonyPostfix]
    private static void Awake_Postfix(uGUI_Equipment __instance)
    {
        QuickLogger.Debug("uGUI_Equipment Awake UC", true);


        CreateSlots(__instance, __instance.gameObject.transform,5,2,200, UniversalChargerBuildable.ucPowercellSlots);
        
        CreateSlots(__instance, __instance.gameObject.transform,5,2, 200, UniversalChargerBuildable.ucBatterySlots);
    }

    private static void CreateSlots(uGUI_Equipment __instance, Transform grid,int sizeY,int sizeX,int spacing, List<string> slots)
    {
        QuickLogger.Debug($"==================================== [Create Slots] ====================================");
        var modulePrefab = __instance.gameObject.FindChild($"CyclopsModule1").gameObject;

        int topMax = 200;
        int bottomMax = -320;


        int leftMax = -249;
        int rightMax = 167;
        int size = 130;


        int i = 0;
        int currentRow = leftMax;
        int currentRowh = topMax;

        for (int y = sizeY - 1; y >= 0; y--)
        {
            QuickLogger.Debug($"Y = {y}, SizeY = {sizeY}");

            for (int x = 0; x < sizeX; x++)
            {
                QuickLogger.Debug($"Index : {i}");
                //Add new slot
                var slotName = slots[i];

                QuickLogger.Debug($"Slot Name : {slotName}");

                var f = GameObject.Instantiate(modulePrefab, grid, false);
                var slot = f.GetComponent<uGUI_EquipmentSlot>();
                slot.active = false;
                slot.slot = slotName;
                slot.name = slotName;

                slot.gameObject.transform.localPosition = new Vector3(currentRow, currentRowh, 0);
                GameObject.Destroy(f.transform.GetChild(0).gameObject);
                __instance.allSlots.Add(slotName, slot);
                i++;
                currentRow += size;
            }

            //Increment Row
            currentRowh -= size;
            currentRow = leftMax;
        }

        QuickLogger.Debug($"==================================== [Create Slots] ====================================");
    }
}
