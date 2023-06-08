using FCS_AlterraHub.Core.Components;
using FCS_AlterraHub.ModItems.Buildables.BaseManager.Mono.GUI.Dialogs;
using FCSCommon.Utilities;
using HarmonyLib;
using System;
using System.Runtime.Remoting.Messaging;

namespace FCS_AlterraHub.ModItems.FCSPDA.Patches;

[HarmonyPatch]
internal class PowerConsumer_Patches
{

    [HarmonyPatch(typeof(FiltrationMachine), nameof(FiltrationMachine.Start))]
    [HarmonyPostfix]
    private static void Start_Postfix(FiltrationMachine __instance)
    {
        var powerInterface = __instance.gameObject.EnsureComponent<FCSPowerInterface>();

        var prefrabID = Utils.FindAncestorWithComponent<PrefabIdentifier>(__instance.gameObject);

        powerInterface.PrefabID = prefrabID.id;
        powerInterface.PowerPerSec = FiltrationMachine.powerPerSecond;
        powerInterface.DeviceFriendlyName = "FilrationMachine";
        powerInterface.Initialize(() => FiltrationMachine.powerPerSecond, () =>
        {
            QuickLogger.Debug("Updating Filter",true);
            if (__instance.timeRemainingWater > 0f || __instance.timeRemainingSalt > 0f || __instance.IsInvoking("UpdateFiltering"))
            {
                QuickLogger.Debug("Updating", true);
                float num = 1f * DayNightCycle.main.dayNightSpeed;

                if (!GameModeUtils.RequiresPower() || (__instance.powerRelay != null && __instance.powerRelay.GetPower() >= 0.85f * num))
                {
                    if (GameModeUtils.RequiresPower())
                    {
                        return true;
                    }
                }
            }

            QuickLogger.Debug("Not Updating", true);
            return false;
        });

        BasePowerDetailsDialogController.Register(powerInterface);


    }

    [HarmonyPatch(typeof(FiltrationMachine), nameof(FiltrationMachine.OnDestroy))]
    [HarmonyPostfix]
    private static void OnDestroy_Postfix(FiltrationMachine __instance)
    {
        var powerInterface = __instance.gameObject.GetComponent<FCSPowerInterface>();
        BasePowerDetailsDialogController.UnRegister(powerInterface);
    }
}
