using FCS_AlterraHub.Core.Services;
using FCS_AlterraHub.Models.Mono;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseOxygenTank.Mono.Service;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseUtilityUnit.Buildable;
using FCS_LifeSupportSolutions.ModItems.Buildables.BaseUtilityUnit.Mono;
using FCSCommon.Utilities;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace FCS_LifeSupportSolutions.Patches;

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("Awake")]
internal class Player_Awake
{
    [HarmonyPrefix]
    public static void Postfix(ref Player __instance)
    {
        //__instance.gameObject.EnsureComponent<PlayerAdrenaline>();
        new GameObject("BaseOxygenTankService").AddComponent<BaseOxygenTankService>();

    }
}

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("Update")]
internal class Player_Update
{
    [HarmonyPrefix]
    public static void Postfix(ref Player __instance)
    {
        //if (uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD != null)
        //{
        //    uGUI_PowerIndicator_Initialize_Patch.LifeSupportHUD.ToggleVisibility();
        //}
    }
}

[HarmonyPatch(typeof(Player))]
[HarmonyPatch("CanBreathe")]
internal class Player_CanBreathe
{
    internal static float DefaultO2Level;

    private static readonly float _oxygenPerSecond = 10f;


    public static bool Prefix(ref Player __instance, ref bool __result)
    {
        if (!Plugin.Configuration.BaseUtilityUnitAffectPlayerOxygen || !Plugin.Configuration.BaseUtilityUnitIsModEnabled) return true;

        GetDefaultO2Level(__instance);

        if (__instance.IsInBase())
        {
            if (__instance.IsUnderwater()) return true;

            var curBase = __instance.GetCurrentSub();

            var manager = HabitatService.main.GetBaseManager(curBase.gameObject);

            if(manager is null) return true;

            if (!IsThereAnyBaseUtilityUnitAttached(manager)) return false;

            PerformOxygenCheckForBases(__instance, out __result, manager);

            return false;
        }

        return true; //return false to skip execution of the original.
    }

    private static bool IsThereAnyOxStationModule(Vehicle curSub)
    {
        if (curSub == null) return false;

        var oxStationCount = curSub.modules.GetCount(TechType.SeamothSonarModule);

        if (oxStationCount <= 0)
        {
            return false;
        }

        return true;
    }

    private static bool IsThereAnyBaseUtilityUnitAttached(HabitatManager manager)
    {
        return manager.GetCountDevicesOfType<BaseUtilityUnitController>() + manager.GetCountDevicesOfType<BaseOxygenTankController>() > 0;
    }

    private static void PerformOxygenCheckForBases(Player instance, out bool outResult, HabitatManager manager)
    {
        outResult = false;
        float o2Available = instance.oxygenMgr.GetOxygenAvailable();
        float o2Capacity = BaseUtilityUnitBuildable.IsRefillableOxygenTanksInstalled ? DefaultO2Level : Player.main.oxygenMgr.GetOxygenCapacity();

        if (o2Available >= o2Capacity)
            return;

        if (TryAddOxygen(ref outResult, manager))
            return;

    }

    private static bool TryAddOxygen(ref bool outResult, HabitatManager manager)
    {
        QuickLogger.Debug("1");
        var tankManager = BaseOxygenTankService.main.GetBaseOxygenTankManager(manager.GetBasePrefabID());

        QuickLogger.Debug("2");

        var amount = _oxygenPerSecond * DayNightCycle.main.deltaTime;
        QuickLogger.Debug("3");

        var baseUtilityUnits = manager.GetDevicesOfType<BaseUtilityUnitController>();
        QuickLogger.Debug("4");

        foreach (var baseUnit in baseUtilityUnits)
        {
            QuickLogger.Debug("4.1");

            var utility = (BaseUtilityUnitController)baseUnit;
            QuickLogger.Debug("4.2");

            if (utility.GetOxygenManager().GetO2Level() <= 0 || !utility.IsOperational())
                continue;
            QuickLogger.Debug("4.3");

            var result = utility.GetOxygenManager().RemoveOxygen(amount);
            QuickLogger.Debug("4.4");

            if (result)
            {
                QuickLogger.Debug("4.5");

                Player.main.oxygenMgr.AddOxygen(amount);
                outResult = true;
                QuickLogger.Debug("4.6");

            }
        }
        QuickLogger.Debug("5");

        if (tankManager is not null)
        {
            var requiredTankCount = tankManager.GetRequiredTankCount(Plugin.Configuration.SmallBaseOxygenHardcore);

            float ActiveTankCount = tankManager.GetActiveTankCount();

            if (ActiveTankCount >= requiredTankCount)
            {
                outResult = true;
            }
            else if (requiredTankCount - ActiveTankCount <= 1)
            {
                amount = 1.05f * DayNightCycle.main.deltaTime * ActiveTankCount / requiredTankCount;
                Player.main.oxygenMgr.AddOxygen(amount);
            }

            QuickLogger.Debug($"ActiveTankCount: {ActiveTankCount}, RequiredTankCount: {requiredTankCount}", true);
        }
        QuickLogger.Debug("5");

        return outResult;
    }

    private static void GetDefaultO2Level(Player instance)
    {
        if (DefaultO2Level <= 0)
        {
            var oxygen = instance.gameObject.GetComponentInParent<Oxygen>();

            if (oxygen == null || !oxygen.isPlayer) return;

            DefaultO2Level = oxygen.oxygenCapacity;
        }
    }
}

